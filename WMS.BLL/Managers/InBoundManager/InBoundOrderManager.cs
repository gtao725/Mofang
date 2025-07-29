using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MODEL_MSSQL;
using WMS.BLLClass;
using WMS.IBLL;
using System.Transactions;
using System.Text.RegularExpressions;
using System.Collections;

namespace WMS.BLL
{
    public class InBoundOrderManager : IInBoundOrderManager
    {
        IDAL.IDALSession idal = BLL.BLLHelper.GetDal();
        RegInBoundOrderManager reg = new RegInBoundOrderManager();

        #region 1.CFS预录入

        //客户下拉菜单列表
        public IEnumerable<WhClient> WhClientListSelect(string whCode)
        {
            var sql = from a in idal.IWhClientDAL.SelectAll()
                      where a.Status == "Active" && a.WhCode == whCode
                      select a;
            sql = sql.OrderBy(u => u.ClientCode);

            return sql.AsEnumerable();
        }

        //客户拥有流程 下拉列表
        //先查询 是否已经预录入，如果已经预录 则默认流程，没有预录 显示全部流程
        public IEnumerable<FlowHead> ClientFlowNameSelect(string whCode, int clientId, string soNumber, string customerPo, string orderType)
        {
            var sql1 = from a in idal.IInBoundOrderDAL.SelectAll()
                       where a.ClientId == clientId && a.OrderType == orderType && a.WhCode == whCode
                       join b in idal.IInBoundSODAL.SelectAll()
                             on new { SoId = (Int32)a.SoId }
                         equals new { SoId = b.Id } into b_join
                       from b in b_join.DefaultIfEmpty()
                       select new InBoundOrderResult
                       {
                           SoNumber = b.SoNumber,
                           CustomerPoNumber = a.CustomerPoNumber,
                           ProcessId = a.ProcessId,
                           ProcessName = a.ProcessName
                       };
            if (!string.IsNullOrEmpty(soNumber) && soNumber != "")
            {
                sql1 = sql1.Where(u => u.SoNumber == soNumber);
            }
            if (!string.IsNullOrEmpty(customerPo) && customerPo != "")
            {
                sql1 = sql1.Where(u => u.CustomerPoNumber == customerPo);
            }

            if (sql1.Count() > 0)
            {
                InBoundOrderResult result = sql1.First();
                var sql = from a in idal.IFlowHeadDAL.SelectAll()
                          where a.Id == result.ProcessId
                          select a;
                return sql.AsEnumerable();
            }
            else
            {
                var sql = from a in idal.IFlowHeadDAL.SelectAll()
                          join b in idal.IR_Client_FlowRuleDAL.SelectAll()
                          on new { Id = a.Id } equals new { Id = (Int32)b.BusinessFlowGroupId } into b_join
                          from b in b_join.DefaultIfEmpty()
                          where b.ClientId == clientId && b.Type == "InBound" && b.WhCode == whCode
                          select a;
                return sql.AsEnumerable();
            }
        }

        //CFS批量添加预录入
        public string InBoundOrderListAdd(InBoundOrderInsert entity)
        {
            string result = "";     //执行总结果

            if (entity == null || entity.InBoundOrderDetailInsert == null)
            {
                return "数据有误，请重新操作！";
            }

            foreach (var item in entity.InBoundOrderDetailInsert)
            {
                //添加InBoundSO 
                InBoundSO inBoundSO = InsertInBoundSO(entity, item);

                //添加InBoundOrder  
                //判断客户PO是否存在
                List<InBoundOrder> listInBoundOrder = idal.IInBoundOrderDAL.SelectBy(u => u.CustomerPoNumber == item.CustomerPoNumber && u.WhCode == entity.WhCode && u.SoId == inBoundSO.Id);

                InBoundOrder inBoundOrder = InsertInBoundOrder(entity, item, inBoundSO, listInBoundOrder);

                //添加ItemMaster
                ItemMaster itemMaster = InsertItemMaster(entity, item);

                //添加InBoundOrderDetail
                int insertResult = InsertInBoundOrderDetail(entity, item, inBoundOrder, itemMaster);
                if (insertResult == 1)
                {
                    result += item.JsonId + " " + item.CustomerPoNumber + " " + item.AltItemNumber + " " + item.Qty + "添加成功!<br>";
                }
                else if (insertResult == 2)
                {
                    result += item.JsonId + " " + "<font color=red>" + item.CustomerPoNumber + " " + item.AltItemNumber + "更新成功,数量累加" + item.Qty + "!</font><br>";
                }
                else
                {
                    result += "<font color=red>保存出错!</font><br>";
                }

            }

            return result;
        }

        public InBoundSO InsertInBoundSO(InBoundOrderInsert entity, InBoundOrderDetailInsert item)
        {
            //验证InBoundSO
            List<InBoundSO> listInBoundSO = idal.IInBoundSODAL.SelectBy(u => u.SoNumber == entity.SoNumber && u.WhCode == entity.WhCode && u.ClientCode == entity.ClientCode);
            InBoundSO inBoundSO = new InBoundSO();
            //如果SO不存在 就添加
            if (listInBoundSO.Count == 0)
            {
                inBoundSO.WhCode = entity.WhCode;
                inBoundSO.SoNumber = entity.SoNumber;
                inBoundSO.ClientCode = entity.ClientCode;
                inBoundSO.ClientId = (int)entity.ClientId;              //添加新数据 必须赋予客户ID
                inBoundSO.CreateUser = item.CreateUser;
                inBoundSO.CreateDate = DateTime.Now;
                idal.IInBoundSODAL.Add(inBoundSO);
                idal.IInBoundSODAL.SaveChanges();
            }
            else
            {
                //存在，就获取
                inBoundSO = listInBoundSO.First();
            }

            return inBoundSO;
        }

        //新增录入明细
        public int InsertInBoundOrderDetail(InBoundOrderInsert entity, InBoundOrderDetailInsert item, InBoundOrder inBoundOrder, ItemMaster itemMaster)
        {
            List<InBoundOrderDetail> listInBoundOrderDetail = new List<InBoundOrderDetail>();

            if (itemMaster.UnitFlag == 1)
            {
                listInBoundOrderDetail = idal.IInBoundOrderDetailDAL.SelectBy(u => u.WhCode == entity.WhCode && u.ItemId == itemMaster.Id && u.PoId == inBoundOrder.Id && u.UnitId == item.UnitId && u.UnitName == item.UnitName).ToList();
            }
            else
            {
                listInBoundOrderDetail = idal.IInBoundOrderDetailDAL.SelectBy(u => u.WhCode == entity.WhCode && u.ItemId == itemMaster.Id && u.PoId == inBoundOrder.Id).ToList();
            }

            int insertResult = 0;
            if (listInBoundOrderDetail.Count == 0)
            {
                InBoundOrderDetail inBoundOrderDetail = new InBoundOrderDetail();
                inBoundOrderDetail.WhCode = entity.WhCode;
                inBoundOrderDetail.PoId = inBoundOrder.Id;
                inBoundOrderDetail.ItemId = itemMaster.Id;
                inBoundOrderDetail.UnitId = item.UnitId;
                if (itemMaster.UnitName == "" || itemMaster.UnitName == null)
                {
                    inBoundOrderDetail.UnitName = "none";
                }
                else
                {
                    if (itemMaster.UnitFlag == 1)
                    {
                        inBoundOrderDetail.UnitName = item.UnitName;
                    }
                    else
                    {
                        inBoundOrderDetail.UnitName = itemMaster.UnitName;
                    }
                }

                inBoundOrderDetail.Qty = item.Qty;
                inBoundOrderDetail.Weight = item.Weight;
                inBoundOrderDetail.CBM = item.CBM;
                inBoundOrderDetail.CreateUser = item.CreateUser;
                inBoundOrderDetail.CustomDate = item.CustomDate;
                inBoundOrderDetail.CustomNumber1 = item.CustomNumber1;
                inBoundOrderDetail.CreateDate = DateTime.Now;
                idal.IInBoundOrderDetailDAL.Add(inBoundOrderDetail);
                insertResult = 1;
            }
            else
            {
                InBoundOrderDetail inBoundOrderDetail = listInBoundOrderDetail.First();
                int yuluruQty = inBoundOrderDetail.Qty - inBoundOrderDetail.RegQty;
                if (yuluruQty < item.Qty)
                {
                    inBoundOrderDetail.Qty = inBoundOrderDetail.Qty + (item.Qty - yuluruQty);
                    inBoundOrderDetail.UpdateUser = item.CreateUser;
                    inBoundOrderDetail.UpdateDate = DateTime.Now;

                    inBoundOrderDetail.Weight = item.Weight;
                    inBoundOrderDetail.CBM = item.CBM;
                    inBoundOrderDetail.CustomDate = item.CustomDate;

                    idal.IInBoundOrderDetailDAL.UpdateBy(inBoundOrderDetail, u => u.Id == inBoundOrderDetail.Id, new string[] { "Qty", "UpdateUser", "UpdateDate", "Weight", "CBM", "CustomDate" });
                }

                insertResult = 2;
            }

            idal.IInBoundOrderDetailDAL.SaveChanges();
            return insertResult;
        }

        //新增预录入
        public InBoundOrder InsertInBoundOrder(InBoundOrderInsert entity, InBoundOrderDetailInsert item, InBoundSO inBoundSO, List<InBoundOrder> listInBoundOrder)
        {
            InBoundOrder inBoundOrder = new InBoundOrder();
            if (listInBoundOrder.Count == 0)
            {
                inBoundOrder.WhCode = entity.WhCode;
                if (inBoundSO != null)
                {
                    inBoundOrder.SoId = inBoundSO.Id;
                }
                inBoundOrder.PoNumber = "ST" + DI.IDGenerator.NewId;
                inBoundOrder.CustomerPoNumber = item.CustomerPoNumber;
                inBoundOrder.AltCustomerPoNumber = item.AltCustomerPoNumber;
                inBoundOrder.ClientId = (int)entity.ClientId;
                inBoundOrder.ClientCode = entity.ClientCode;
                inBoundOrder.OrderType = entity.OrderType;
                inBoundOrder.ProcessId = entity.ProcessId;
                inBoundOrder.ProcessName = entity.ProcessName;
                inBoundOrder.OrderSource = "WMS";
                inBoundOrder.CreateUser = item.CreateUser;
                inBoundOrder.CreateDate = DateTime.Now;
                idal.IInBoundOrderDAL.Add(inBoundOrder);   //不存在就新增
            }
            else
            {
                inBoundOrder = listInBoundOrder.First();
            }

            return inBoundOrder;
        }

        //CFS预录入明细列表
        public List<InBoundOrderDetailResult> InBoundOrderDetailList(InBoundOrderDetailSearch searchEntity, string[] poNumberList, string[] itemNumberList, out int total, out string str)
        {
            var sql = from a in idal.IInBoundSODAL.SelectAll()
                      where a.SoNumber == searchEntity.SoNumber && a.WhCode == searchEntity.WhCode && a.ClientId == searchEntity.ClientId
                      join b in idal.IInBoundOrderDAL.SelectAll()
                      on new { B = a.Id } equals new { B = (int)b.SoId }
                      where b.OrderType == searchEntity.OrderType
                      join d in idal.IInBoundOrderDetailDAL.SelectAll()
                      on new { B = b.Id } equals new { B = d.PoId }
                      join c in idal.IItemMasterDAL.SelectAll()
                      on new { B = d.ItemId } equals new { B = c.Id }
                      select new InBoundOrderDetailResult
                      {
                          Id = d.Id,
                          WhCode = d.WhCode,
                          CustomerPoNumber = b.CustomerPoNumber,
                          AltItemNumber = c.AltItemNumber,
                          Style1 = c.Style1,
                          Style2 = c.Style2,
                          Style3 = c.Style3,
                          Qty = d.Qty,
                          RegQty = d.RegQty,
                          Weight = d.Weight,
                          UnitId = d.UnitId,
                          UnitName = d.UnitName,
                          CBM = d.CBM
                      };

            if (poNumberList != null)
                sql = sql.Where(u => poNumberList.Contains(u.CustomerPoNumber));
            if (itemNumberList != null)
                sql = sql.Where(u => itemNumberList.Contains(u.AltItemNumber));

            total = sql.Count();
            str = "";
            if (total > 0)
            {
                str = "{\"预录入数量\":\"" + sql.Sum(u => u.Qty).ToString() + "\",\"已登记数量\":\"" + sql.Sum(u => u.RegQty).ToString() + "\"}";
            }
            sql = sql.OrderByDescending(u => u.Id);
            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);
            return sql.ToList();
        }

        //CFS预录入信息修改
        public string InBoundOrderDetailEdit(InBoundOrderDetail entity, params string[] modifiedProNames)
        {
            if (entity.Id == 0)
            {
                return "明细ID为空，修改有误！";
            }

            InBoundOrderDetail orderDetail = idal.IInBoundOrderDetailDAL.SelectBy(u => u.Id == entity.Id).First();
            if (orderDetail.RegQty != 0)
            {
                if (entity.Qty < orderDetail.RegQty)
                {
                    return "预录入数量不能小于登记数量！";
                }
            }

            //如果修改数量与原数量不一致 需要增加Log日志
            if (entity.Qty != orderDetail.Qty)
            {
                var sql = from b in idal.IInBoundOrderDAL.SelectAll()
                          join a in idal.IInBoundSODAL.SelectAll()
                          on new { B = (int)b.SoId } equals new { B = a.Id } into temp_a
                          from ab in temp_a.DefaultIfEmpty()
                          join d in idal.IInBoundOrderDetailDAL.SelectAll()
                          on new { B = b.Id } equals new { B = d.PoId }
                          join c in idal.IItemMasterDAL.SelectAll()
                          on new { B = d.ItemId } equals new { B = c.Id }
                          where d.Id == entity.Id
                          select new InBoundOrderDetailResult
                          {
                              WhCode = b.WhCode,
                              ClientCode = b.ClientCode,
                              SoNumber = ab.SoNumber,
                              CustomerPoNumber = b.CustomerPoNumber,
                              AltItemNumber = c.AltItemNumber,
                              Qty = d.Qty,
                              ItemId = d.ItemId,
                              UnitName = d.UnitName
                          };

                List<InBoundOrderDetailResult> list = sql.ToList();
                List<TranLog> tranLogList = new List<TranLog>();
                foreach (var item in list)
                {
                    //插入记录
                    TranLog tl = new TranLog();
                    tl.TranType = "1210";
                    tl.Description = "编辑预录入";
                    tl.TranDate = DateTime.Now;
                    tl.TranUser = entity.UpdateUser;
                    tl.ClientCode = item.ClientCode;
                    tl.WhCode = item.WhCode;
                    tl.SoNumber = item.SoNumber ?? "";
                    tl.CustomerPoNumber = item.CustomerPoNumber;
                    tl.AltItemNumber = item.AltItemNumber;
                    tl.ItemId = item.ItemId;
                    tl.UnitName = item.UnitName;
                    tl.TranQty = item.Qty;
                    tl.TranQty2 = entity.Qty;
                    tl.Remark = "编辑预录入,原数量:" + tl.TranQty + "变更为:" + tl.TranQty2;
                    tranLogList.Add(tl);
                }
                idal.ITranLogDAL.Add(tranLogList);
            }

            idal.IInBoundOrderDetailDAL.UpdateBy(entity, u => u.Id == entity.Id, modifiedProNames);
            idal.IInBoundOrderDetailDAL.SaveChanges();
            return "Y";
        }

        //CFS预录入信息删除
        public string InBoundOrderDetailDel(int id, string userName)
        {
            InBoundOrderDetail orderDetail = idal.IInBoundOrderDetailDAL.SelectBy(u => u.Id == id).First();
            if (orderDetail.RegQty != 0)
            {
                return "已做收货登记的明细不能删除！";
            }

            List<ReceiptRegisterDetail> checkRegDetail = idal.IReceiptRegisterDetailDAL.SelectBy(u => u.InOrderDetailId == id);
            if (checkRegDetail.Count > 0)
            {
                return "收货登记已做选择,请先删除收货登记！";
            }

            var sql = from b in idal.IInBoundOrderDAL.SelectAll()
                      join a in idal.IInBoundSODAL.SelectAll()
                      on new { B = (int)b.SoId } equals new { B = a.Id } into temp_a
                      from ab in temp_a.DefaultIfEmpty()
                      join d in idal.IInBoundOrderDetailDAL.SelectAll()
                      on new { B = b.Id } equals new { B = d.PoId }
                      join c in idal.IItemMasterDAL.SelectAll()
                      on new { B = d.ItemId } equals new { B = c.Id }
                      where d.Id == id
                      select new InBoundOrderDetailResult
                      {
                          WhCode = b.WhCode,
                          ClientCode = b.ClientCode,
                          SoNumber = ab.SoNumber,
                          CustomerPoNumber = b.CustomerPoNumber,
                          AltItemNumber = c.AltItemNumber,
                          Qty = d.Qty,
                          ItemId = d.ItemId,
                          UnitName = d.UnitName
                      };

            List<InBoundOrderDetailResult> list = sql.ToList();
            List<TranLog> tranLogList = new List<TranLog>();
            foreach (var item in list)
            {
                //插入记录
                TranLog tl = new TranLog();
                tl.TranType = "1220";
                tl.Description = "删除预录入";
                tl.TranDate = DateTime.Now;
                tl.TranUser = userName;
                tl.ClientCode = item.ClientCode;
                tl.WhCode = item.WhCode;
                tl.SoNumber = item.SoNumber ?? "";
                tl.CustomerPoNumber = item.CustomerPoNumber;
                tl.AltItemNumber = item.AltItemNumber;
                tl.ItemId = item.ItemId;
                tl.UnitName = item.UnitName;
                tl.TranQty = item.Qty;
                tl.Remark = "删除预录入";
                tranLogList.Add(tl);
            }
            idal.ITranLogDAL.Add(tranLogList);

            idal.IInBoundOrderDetailDAL.DeleteBy(u => u.Id == id);
            idal.IInBoundOrderDetailDAL.SaveChanges();
            return "Y";
        }

        //预录入批量修改客户名
        public string InBoundOrderEditClientCode(EditInBoundResult Editentity, string[] getsoList, string[] clientCodeList)
        {
            string result1 = "";

            List<WhClient> WhClientList = idal.IWhClientDAL.SelectBy(u => u.WhCode == Editentity.WhCode && u.ClientCode == Editentity.ClientCode && u.Status == "Active" && u.Id == Editentity.ClientId);
            if (WhClientList.Count == 0)
            {
                return "未找到客户，无法修改！";
            }

            List<EditInBoundResult> list = new List<EditInBoundResult>();
            for (int i = 0; i < clientCodeList.Length; i++)
            {
                if (Editentity.ClientCode == clientCodeList[i])
                {
                    continue;
                }
                else
                {
                    if (list.Where(u => u.ClientCode == clientCodeList[i] && u.SoNumber == getsoList[i]).Count() == 0)
                    {
                        EditInBoundResult result = new EditInBoundResult();
                        result.SoNumber = getsoList[i].ToString();
                        result.ClientCode = clientCodeList[i].ToString();
                        list.Add(result);
                    }
                }
            }

            //select* from InBoundSO a
            //inner join InBoundOrder b on a.Id = b.SoId
            //inner join InBoundOrderDetail c on b.Id = c.PoId
            //inner join ItemMaster d on c.ItemId = d.Id
            //where a.WhCode = '10' and SoNumber = 'SO2' and a.ClientCode = 'TEST'

            //批量执行修改客户名
            foreach (var item1 in list)
            {
                var sql = from a in idal.IInBoundSODAL.SelectAll()
                          join b in idal.IInBoundOrderDAL.SelectAll()
                          on new { A = a.Id } equals new { A = (Int32)b.SoId }
                          join c in idal.IInBoundOrderDetailDAL.SelectAll()
                          on new { b = b.Id } equals new { b = c.PoId }
                          join d in idal.IItemMasterDAL.SelectAll()
                          on new { ItemId = c.ItemId } equals new { ItemId = d.Id }
                          where a.ClientCode == item1.ClientCode && a.SoNumber == item1.SoNumber && a.WhCode == Editentity.WhCode
                          select new EditInBoundDetailResult
                          {
                              ClientCode = a.ClientCode,
                              SoNumber = a.SoNumber,
                              SoId = a.Id,
                              PoId = b.Id,
                              CustomerPoNumber = b.CustomerPoNumber,
                              ItemId = d.Id,
                              AltItemNumber = d.AltItemNumber,
                              Style1 = d.Style1,
                              Style2 = d.Style2,
                              Style3 = d.Style3,
                              Qty = c.Qty,
                              RegQty = c.RegQty,
                              UnitName = c.UnitName,
                              UnitId = (Int32)c.UnitId,
                              InOrderDetailId = c.Id,
                              OrderType = b.OrderType,
                              ProcessId = b.ProcessId,
                              ProcessName = b.ProcessName
                          };
                if (sql.Where(u => u.RegQty > 0).Count() > 0)
                {
                    result1 = "客户名：" + item1.ClientCode + "SO：" + item1.SoNumber + "已做登记，无法修改，请重新查询！";
                    break;
                }
                else
                {
                    //批量重建预录入
                    List<EditInBoundDetailResult> getList = sql.ToList();

                    //2. 重建预录入基础信息
                    #region
                    string getclient = "", getSo = "", getPo = "", getSku = "", getStyle1 = "", getStyle2 = "", getStyle3 = "", getQty = "", unitName = "";

                    List<InBoundOrderInsert> entityList = new List<InBoundOrderInsert>();
                    foreach (var item in getList)
                    {
                        getclient = Editentity.ClientCode;
                        getSo = item.SoNumber;
                        getPo = item.CustomerPoNumber;
                        getSku = item.AltItemNumber;
                        getStyle1 = item.Style1;
                        getStyle2 = item.Style2;
                        getStyle3 = item.Style3;
                        getQty = item.Qty.ToString();
                        unitName = item.UnitName;

                        if (entityList.Where(u => u.SoNumber == getSo.Trim() && u.ClientCode == getclient.Trim()).Count() == 0)
                        {
                            InBoundOrderInsert entity1 = new InBoundOrderInsert();
                            entity1.WhCode = Editentity.WhCode;
                            entity1.ClientCode = getclient.Trim();
                            entity1.SoNumber = getSo.Trim();
                            entity1.CreateUser = Editentity.UserCode;
                            entity1.OrderType = item.OrderType;
                            entity1.ClientId = Editentity.ClientId;
                            entity1.ProcessId = item.ProcessId;
                            entity1.ProcessName = item.ProcessName;

                            List<InBoundOrderDetailInsert> orderDetailList = new List<InBoundOrderDetailInsert>();

                            InBoundOrderDetailInsert orderDetail = new InBoundOrderDetailInsert();
                            orderDetail.CustomerPoNumber = getPo.Trim().ToString();
                            orderDetail.AltItemNumber = getSku.Trim().ToString();
                            orderDetail.Style1 = getStyle1.ToString();
                            orderDetail.Style2 = getStyle2.ToString();
                            orderDetail.Style3 = getStyle3.ToString();
                            orderDetail.UnitName = unitName;
                            orderDetail.Qty = Convert.ToInt32(getQty);
                            orderDetail.CreateUser = Editentity.UserCode;
                            orderDetailList.Add(orderDetail);

                            entity1.InBoundOrderDetailInsert = orderDetailList;
                            entityList.Add(entity1);
                        }
                        else
                        {
                            InBoundOrderInsert oldentity = entityList.Where(u => u.SoNumber == getSo.Trim() && u.ClientCode == getclient.Trim()).First();
                            entityList.Remove(oldentity);

                            InBoundOrderInsert newentity = oldentity;

                            List<InBoundOrderDetailInsert> orderDetailList = oldentity.InBoundOrderDetailInsert.ToList();
                            if (orderDetailList.Where(u => u.CustomerPoNumber == getPo.Trim() && u.AltItemNumber == getSku.Trim() && u.Style1 == getStyle1 && u.Style2 == getStyle2 && u.Style3 == getStyle3).Count() == 0)
                            {
                                InBoundOrderDetailInsert orderDetail = new InBoundOrderDetailInsert();
                                orderDetail.CustomerPoNumber = getPo.Trim().ToString();
                                orderDetail.AltItemNumber = getSku.Trim().ToString();
                                orderDetail.Style1 = getStyle1.Trim().ToString();
                                orderDetail.Style2 = getStyle2.ToString();
                                orderDetail.Style3 = getStyle3.ToString();
                                orderDetail.UnitName = unitName;
                                orderDetail.Qty = Convert.ToInt32(getQty);
                                orderDetail.CreateUser = Editentity.UserCode;
                                orderDetailList.Add(orderDetail);
                            }
                            else
                            {
                                InBoundOrderDetailInsert oldorderDetail = orderDetailList.Where(u => u.CustomerPoNumber == getPo.Trim() && u.AltItemNumber == getSku.Trim() && u.Style1 == getStyle1.Trim() && u.Style2 == getStyle2 && u.Style3 == getStyle3).First();

                                orderDetailList.Remove(oldorderDetail);
                                InBoundOrderDetailInsert neworderDetail = oldorderDetail;
                                neworderDetail.Qty = oldorderDetail.Qty + Convert.ToInt32(getQty);
                                orderDetailList.Add(neworderDetail);
                            }
                            newentity.InBoundOrderDetailInsert = orderDetailList;
                            entityList.Add(newentity);
                        }
                    }
                    #endregion

                    //3.批量导入预录入信息
                    #region 

                    InBoundOrderInsert fir = entityList.First();

                    List<string> soList = new List<string>();
                    List<string> poList = new List<string>();
                    List<string> skuList = new List<string>();

                    //批量导入预录入SO
                    List<InBoundSO> InBoundSOAddList = new List<InBoundSO>();
                    foreach (var entity in entityList)
                    {
                        if (!string.IsNullOrEmpty(entity.SoNumber))
                        {
                            var ChecklistInBoundSO = idal.IInBoundSODAL.SelectBy(u => u.WhCode == entity.WhCode && u.ClientCode == entity.ClientCode && u.SoNumber == entity.SoNumber);

                            if (ChecklistInBoundSO.Count() == 0)
                            {
                                InBoundSO inBoundSO = new InBoundSO();
                                inBoundSO.WhCode = entity.WhCode;
                                inBoundSO.SoNumber = entity.SoNumber;
                                inBoundSO.ClientCode = entity.ClientCode;
                                inBoundSO.ClientId = (int)entity.ClientId;              //添加新数据 必须赋予客户ID
                                inBoundSO.CreateUser = fir.CreateUser;
                                inBoundSO.CreateDate = DateTime.Now;
                                InBoundSOAddList.Add(inBoundSO);
                            }
                            soList.Add(entity.SoNumber);
                        }
                    }

                    if (InBoundSOAddList.Count > 0)
                    {
                        idal.IInBoundSODAL.Add(InBoundSOAddList);
                        idal.IInBoundSODAL.SaveChanges();
                    }

                    List<InBoundSO> listInBoundSO = idal.IInBoundSODAL.SelectBy(u => u.WhCode == fir.WhCode && soList.Contains(u.SoNumber));

                    List<InBoundOrder> InBoundOrderAddList = new List<InBoundOrder>();
                    List<ItemMaster> ItemMasterAddList = new List<ItemMaster>();

                    List<InBoundOrder> checkInBoundOrderAddResult = new List<InBoundOrder>();
                    List<ItemMaster> checkItemMasterAddResult = new List<ItemMaster>();

                    //批量导入预录入PO
                    foreach (var entity in entityList)
                    {
                        if (!string.IsNullOrEmpty(entity.SoNumber))
                        {
                            InBoundSO inBoundSO = listInBoundSO.Where(u => u.SoNumber == entity.SoNumber && u.ClientCode == entity.ClientCode).First();

                            string[] getPoArr = (from a in entity.InBoundOrderDetailInsert select a.CustomerPoNumber).ToList().Distinct().ToArray();

                            List<InBoundOrder> checkPoArr = idal.IInBoundOrderDAL.SelectBy(u => getPoArr.Contains(u.CustomerPoNumber) && u.WhCode == entity.WhCode && u.ClientCode == entity.ClientCode && u.SoId == inBoundSO.Id);

                            string[] getSkuArr = (from a in entity.InBoundOrderDetailInsert select a.AltItemNumber).ToList().Distinct().ToArray();

                            List<ItemMaster> checkSkuArr = idal.IItemMasterDAL.SelectBy(u => u.WhCode == entity.WhCode && getSkuArr.Contains(u.AltItemNumber) && u.ClientId == entity.ClientId).OrderBy(u => u.Id).ToList();

                            foreach (var item in entity.InBoundOrderDetailInsert)
                            {
                                if (checkInBoundOrderAddResult.Where(u => u.SoId == inBoundSO.Id && u.CustomerPoNumber == item.CustomerPoNumber).Count() == 0)
                                {
                                    if (checkPoArr.Where(u => u.CustomerPoNumber == item.CustomerPoNumber && u.WhCode == entity.WhCode && u.ClientCode == entity.ClientCode && u.SoId == inBoundSO.Id).Count() == 0)
                                    {
                                        InBoundOrder inBoundOrder = new InBoundOrder();
                                        inBoundOrder.WhCode = entity.WhCode;
                                        inBoundOrder.SoId = inBoundSO.Id;
                                        inBoundOrder.PoNumber = "ST" + DI.IDGenerator.NewId;
                                        inBoundOrder.CustomerPoNumber = item.CustomerPoNumber;
                                        inBoundOrder.AltCustomerPoNumber = item.AltCustomerPoNumber;
                                        inBoundOrder.ClientId = (int)entity.ClientId;
                                        inBoundOrder.ClientCode = entity.ClientCode;
                                        inBoundOrder.OrderType = entity.OrderType;
                                        inBoundOrder.ProcessId = entity.ProcessId;
                                        inBoundOrder.ProcessName = entity.ProcessName;
                                        inBoundOrder.OrderSource = "WMS";
                                        inBoundOrder.CreateUser = item.CreateUser;
                                        inBoundOrder.CreateDate = DateTime.Now;
                                        InBoundOrderAddList.Add(inBoundOrder);
                                    }
                                    poList.Add(item.CustomerPoNumber);

                                    InBoundOrder inboundResult = new InBoundOrder();
                                    inboundResult.SoId = inBoundSO.Id;
                                    inboundResult.CustomerPoNumber = item.CustomerPoNumber;
                                    checkInBoundOrderAddResult.Add(inboundResult);
                                }

                                if (checkItemMasterAddResult.Where(u => u.AltItemNumber == item.AltItemNumber && (u.Style1 == null ? "" : u.Style1) == (item.Style1 == null ? "" : item.Style1) && (u.Style2 == null ? "" : u.Style2) == (item.Style2 == null ? "" : item.Style2) && (u.Style3 == null ? "" : u.Style3) == (item.Style3 == null ? "" : item.Style3) && u.ClientId == entity.ClientId).Count() == 0)
                                {
                                    if (checkSkuArr.Where(u => u.WhCode == entity.WhCode && u.AltItemNumber == item.AltItemNumber && (u.Style1 == null ? "" : u.Style1) == (item.Style1 == null ? "" : item.Style1) && (u.Style2 == null ? "" : u.Style2) == (item.Style2 == null ? "" : item.Style2) && (u.Style3 == null ? "" : u.Style3) == (item.Style3 == null ? "" : item.Style3) && u.ClientId == entity.ClientId).Count() == 0)
                                    {
                                        ItemMaster itemMaster = new ItemMaster();
                                        itemMaster.ItemNumber = "SYS" + DI.IDGenerator.NewId;
                                        itemMaster.WhCode = entity.WhCode;
                                        itemMaster.AltItemNumber = item.AltItemNumber;
                                        itemMaster.ClientId = (int)entity.ClientId;
                                        itemMaster.ClientCode = entity.ClientCode;
                                        itemMaster.Style1 = item.Style1 ?? "";
                                        itemMaster.Style2 = item.Style2 ?? "";
                                        itemMaster.Style3 = item.Style3 ?? "";
                                        itemMaster.UnitFlag = 0;
                                        if (item.UnitName == "" || item.UnitName == null)
                                        {
                                            itemMaster.UnitName = "none";
                                        }
                                        else
                                        {
                                            itemMaster.UnitName = item.UnitName;
                                        }

                                        itemMaster.CreateUser = item.CreateUser;
                                        itemMaster.CreateDate = DateTime.Now;
                                        ItemMasterAddList.Add(itemMaster);   //款号不存在就新增
                                    }
                                    skuList.Add(item.AltItemNumber);

                                    ItemMaster itemResult = new ItemMaster();
                                    itemResult.ClientId = (int)entity.ClientId;
                                    itemResult.AltItemNumber = item.AltItemNumber;
                                    itemResult.Style1 = item.Style1 ?? "";
                                    itemResult.Style2 = item.Style2 ?? "";
                                    itemResult.Style3 = item.Style3 ?? "";
                                    checkItemMasterAddResult.Add(itemResult);
                                }

                            }
                        }
                        else
                        {
                            string[] getPoArr = (from a in entity.InBoundOrderDetailInsert select a.CustomerPoNumber).ToList().Distinct().ToArray();

                            List<InBoundOrder> checkPoArr = idal.IInBoundOrderDAL.SelectBy(u => getPoArr.Contains(u.CustomerPoNumber) && u.WhCode == entity.WhCode && u.ClientCode == entity.ClientCode && u.SoId == null);

                            string[] getSkuArr = (from a in entity.InBoundOrderDetailInsert select a.AltItemNumber).ToList().Distinct().ToArray();

                            List<ItemMaster> checkSkuArr = idal.IItemMasterDAL.SelectBy(u => u.WhCode == entity.WhCode && getSkuArr.Contains(u.AltItemNumber) && u.ClientId == entity.ClientId).OrderBy(u => u.Id).ToList();

                            foreach (var item in entity.InBoundOrderDetailInsert)
                            {
                                if (checkInBoundOrderAddResult.Where(u => u.SoId == null && u.CustomerPoNumber == item.CustomerPoNumber).Count() == 0)
                                {
                                    if (checkPoArr.Where(u => u.CustomerPoNumber == item.CustomerPoNumber && u.WhCode == entity.WhCode && u.ClientCode == entity.ClientCode && u.SoId == null).Count() == 0)
                                    {
                                        InBoundOrder inBoundOrder = new InBoundOrder();
                                        inBoundOrder.WhCode = entity.WhCode;
                                        inBoundOrder.PoNumber = "ST" + DI.IDGenerator.NewId;
                                        inBoundOrder.CustomerPoNumber = item.CustomerPoNumber;
                                        inBoundOrder.AltCustomerPoNumber = item.AltCustomerPoNumber;
                                        inBoundOrder.ClientId = (int)entity.ClientId;
                                        inBoundOrder.ClientCode = entity.ClientCode;
                                        inBoundOrder.OrderType = entity.OrderType;
                                        inBoundOrder.ProcessId = entity.ProcessId;
                                        inBoundOrder.ProcessName = entity.ProcessName;
                                        inBoundOrder.OrderSource = "WMS";
                                        inBoundOrder.CreateUser = item.CreateUser;
                                        inBoundOrder.CreateDate = DateTime.Now;
                                        InBoundOrderAddList.Add(inBoundOrder);
                                    }

                                    poList.Add(item.CustomerPoNumber);

                                    InBoundOrder inboundResult = new InBoundOrder();
                                    inboundResult.SoId = null;
                                    inboundResult.CustomerPoNumber = item.CustomerPoNumber;
                                    checkInBoundOrderAddResult.Add(inboundResult);
                                }

                                if (checkItemMasterAddResult.Where(u => u.AltItemNumber == item.AltItemNumber && (u.Style1 == null ? "" : u.Style1) == (item.Style1 == null ? "" : item.Style1) && (u.Style2 == null ? "" : u.Style2) == (item.Style2 == null ? "" : item.Style2) && (u.Style3 == null ? "" : u.Style3) == (item.Style3 == null ? "" : item.Style3) && u.ClientId == entity.ClientId).Count() == 0)
                                {
                                    if (checkSkuArr.Where(u => u.WhCode == entity.WhCode && u.AltItemNumber == item.AltItemNumber && (u.Style1 == null ? "" : u.Style1) == (item.Style1 == null ? "" : item.Style1) && (u.Style2 == null ? "" : u.Style2) == (item.Style2 == null ? "" : item.Style2) && (u.Style3 == null ? "" : u.Style3) == (item.Style3 == null ? "" : item.Style3) && u.ClientId == entity.ClientId).Count() == 0)
                                    {
                                        ItemMaster itemMaster = new ItemMaster();
                                        itemMaster.ItemNumber = "SYS" + DI.IDGenerator.NewId;
                                        itemMaster.WhCode = entity.WhCode;
                                        itemMaster.AltItemNumber = item.AltItemNumber;
                                        itemMaster.ClientId = (int)entity.ClientId;
                                        itemMaster.ClientCode = entity.ClientCode;
                                        itemMaster.Style1 = item.Style1 ?? "";
                                        itemMaster.Style2 = item.Style2 ?? "";
                                        itemMaster.Style3 = item.Style3 ?? "";
                                        itemMaster.UnitFlag = 0;
                                        if (item.UnitName == "" || item.UnitName == null)
                                        {
                                            itemMaster.UnitName = "none";
                                        }
                                        else
                                        {
                                            itemMaster.UnitName = item.UnitName;
                                        }

                                        itemMaster.CreateUser = item.CreateUser;
                                        itemMaster.CreateDate = DateTime.Now;
                                        ItemMasterAddList.Add(itemMaster);   //款号不存在就新增
                                    }
                                    skuList.Add(item.AltItemNumber);

                                    ItemMaster itemResult = new ItemMaster();
                                    itemResult.ClientId = (int)entity.ClientId;
                                    itemResult.AltItemNumber = item.AltItemNumber;
                                    itemResult.Style1 = item.Style1 ?? "";
                                    itemResult.Style2 = item.Style2 ?? "";
                                    itemResult.Style3 = item.Style3 ?? "";
                                    checkItemMasterAddResult.Add(itemResult);
                                }
                            }
                        }
                    }

                    if (InBoundOrderAddList.Count > 0)
                    {
                        idal.IInBoundOrderDAL.Add(InBoundOrderAddList);
                        idal.IInBoundSODAL.SaveChanges();
                    }

                    if (ItemMasterAddList.Count > 0)
                    {
                        idal.IItemMasterDAL.Add(ItemMasterAddList);
                        idal.IItemMasterDAL.SaveChanges();
                    }

                    List<InBoundOrder> listInBoundOrder = idal.IInBoundOrderDAL.SelectBy(u => poList.Contains(u.CustomerPoNumber) && u.WhCode == fir.WhCode);

                    List<ItemMaster> listItemMaster = idal.IItemMasterDAL.SelectBy(u => skuList.Contains(u.AltItemNumber) && u.WhCode == fir.WhCode).OrderBy(u => u.Id).ToList();

                    foreach (var entity1 in entityList)
                    {
                        foreach (var item11 in entity1.InBoundOrderDetailInsert)
                        {
                            InBoundOrder inBoundOrder = new InBoundOrder();

                            if (!string.IsNullOrEmpty(entity1.SoNumber))
                            {
                                InBoundSO inBoundSO = listInBoundSO.Where(u => u.SoNumber == entity1.SoNumber && u.ClientCode == entity1.ClientCode).First();

                                inBoundOrder = listInBoundOrder.Where(u => u.CustomerPoNumber == item11.CustomerPoNumber && u.ClientCode == entity1.ClientCode && u.SoId == inBoundSO.Id).First();
                            }
                            else
                            {
                                inBoundOrder = listInBoundOrder.Where(u => u.CustomerPoNumber == item11.CustomerPoNumber && u.ClientCode == entity1.ClientCode && u.SoId == null).First();
                            }

                            //判断款号是否存在
                            ItemMaster itemMaster = listItemMaster.Where(u => u.WhCode == entity1.WhCode && u.AltItemNumber == item11.AltItemNumber && (u.Style1 == null ? "" : u.Style1) == (item11.Style1 == null ? "" : item11.Style1) && (u.Style2 == null ? "" : u.Style2) == (item11.Style2 == null ? "" : item11.Style2) && (u.Style3 == null ? "" : u.Style3) == (item11.Style3 == null ? "" : item11.Style3) && u.ClientId == entity1.ClientId).First();

                            //添加InBoundOrderDetail
                            List<InBoundOrderDetail> listInBoundOrderDetail = new List<InBoundOrderDetail>();

                            if (itemMaster.UnitFlag == 1)
                            {
                                listInBoundOrderDetail = idal.IInBoundOrderDetailDAL.SelectBy(u => u.WhCode == Editentity.WhCode && u.ItemId == itemMaster.Id && u.PoId == inBoundOrder.Id && u.UnitId == item11.UnitId && u.UnitName == item11.UnitName).ToList();
                            }
                            else
                            {
                                listInBoundOrderDetail = idal.IInBoundOrderDetailDAL.SelectBy(u => u.WhCode == Editentity.WhCode && u.ItemId == itemMaster.Id && u.PoId == inBoundOrder.Id).ToList();
                            }

                            InBoundOrderDetail inBoundOrderDetail = new InBoundOrderDetail();
                            if (listInBoundOrderDetail.Count == 0)
                            {
                                inBoundOrderDetail.WhCode = Editentity.WhCode;
                                inBoundOrderDetail.PoId = inBoundOrder.Id;
                                inBoundOrderDetail.ItemId = itemMaster.Id;
                                inBoundOrderDetail.UnitId = item11.UnitId;
                                if (itemMaster.UnitName == "" || itemMaster.UnitName == null)
                                {
                                    inBoundOrderDetail.UnitName = "none";
                                }
                                else
                                {
                                    if (itemMaster.UnitFlag == 1)
                                    {
                                        inBoundOrderDetail.UnitName = item11.UnitName;
                                    }
                                    else
                                    {
                                        inBoundOrderDetail.UnitName = itemMaster.UnitName;
                                    }
                                }
                                inBoundOrderDetail.Qty = item11.Qty;
                                inBoundOrderDetail.Weight = (item11.Weight ?? 0);
                                inBoundOrderDetail.CBM = (item11.CBM ?? 0);
                                inBoundOrderDetail.CreateUser = item11.CreateUser;
                                inBoundOrderDetail.CreateDate = DateTime.Now;
                                idal.IInBoundOrderDetailDAL.Add(inBoundOrderDetail);
                            }
                            else
                            {
                                inBoundOrderDetail = listInBoundOrderDetail.First();
                                int yuluruQty = inBoundOrderDetail.Qty - inBoundOrderDetail.RegQty;
                                if (yuluruQty < item11.Qty)
                                {
                                    inBoundOrderDetail.Qty = inBoundOrderDetail.Qty + (item11.Qty - yuluruQty);
                                    inBoundOrderDetail.UpdateUser = item11.CreateUser;
                                    inBoundOrderDetail.UpdateDate = DateTime.Now;
                                    idal.IInBoundOrderDetailDAL.UpdateBy(inBoundOrderDetail, u => u.Id == inBoundOrderDetail.Id, new string[] { "Qty", "UpdateUser", "UpdateDate" });
                                }
                            }
                            idal.IInBoundOrderDetailDAL.SaveChanges();
                        }
                    }

                    #endregion

                    //4.删除原预录入信息
                    int[] getsoid = (from a in getList
                                     select a.SoId).Distinct().ToArray();

                    int[] getpoid = (from a in getList
                                     select a.PoId).Distinct().ToArray();

                    int[] getOrderDetailId = (from a in getList
                                              select a.InOrderDetailId).Distinct().ToArray();

                    idal.IInBoundSODAL.DeleteBy(u => getsoid.Contains(u.Id));
                    idal.IInBoundOrderDAL.DeleteBy(u => getpoid.Contains(u.Id));
                    idal.IInBoundOrderDetailDAL.DeleteBy(u => getOrderDetailId.Contains(u.Id));

                    //5.插入记录
                    TranLog tl = new TranLog();
                    tl.TranType = "67";
                    tl.Description = "预录SO修改客户名";
                    tl.TranDate = DateTime.Now;
                    tl.TranUser = Editentity.UserCode;
                    tl.WhCode = Editentity.WhCode;
                    tl.SoNumber = item1.SoNumber;
                    tl.ClientCode = item1.ClientCode;
                    idal.ITranLogDAL.Add(tl);

                    TranLog tl1 = new TranLog();
                    tl1.TranType = "67";
                    tl1.Description = "预录SO修改客户名";
                    tl1.TranDate = DateTime.Now;
                    tl1.TranUser = Editentity.UserCode;
                    tl1.WhCode = Editentity.WhCode;
                    tl1.SoNumber = item1.SoNumber;
                    tl1.ClientCode = Editentity.ClientCode;
                    idal.ITranLogDAL.Add(tl1);

                    idal.SaveChanges();
                }

            }
            if (result1 + "" != "")
            {
                return result1;
            }

            return "Y";
        }

        //编辑预录入查询列表
        public List<InBoundOrderResult> InBoundListDetail(InBoundOrderSearch searchEntity, string[] soList, out int total)
        {
            var sql = from b in idal.IInBoundOrderDAL.SelectAll()
                      where b.WhCode == searchEntity.WhCode
                      join a in idal.IInBoundSODAL.SelectAll()
                      on new { B = (int)b.SoId } equals new { B = a.Id } into temp1
                      from ab in temp1.DefaultIfEmpty()
                      join c in idal.IInBoundOrderDetailDAL.SelectAll()
                      on b.Id equals c.PoId
                      group new { ab, b, c } by new
                      {
                          b.WhCode,
                          b.ClientId,
                          b.ClientCode,
                          ab.SoNumber,
                          b.CustomerPoNumber,
                          b.ProcessName,
                          b.OrderType,
                          b.CreateDate
                      } into g
                      select new InBoundOrderResult
                      {
                          Action = "",
                          Action1 = "",
                          WhCode = g.Key.WhCode,
                          ClientId = g.Key.ClientId,
                          ClientCode = g.Key.ClientCode,
                          SoNumber = g.Key.SoNumber ?? "",
                          CustomerPoNumber = g.Key.CustomerPoNumber,
                          ProcessName = g.Key.ProcessName,
                          OrderType = g.Key.OrderType,
                          CreateDate = (DateTime)g.Key.CreateDate,
                          TotalQty = g.Sum(p => p.c.Qty),
                          TotalRegQty = g.Sum(p => p.c.RegQty)
                      };

            if (!string.IsNullOrEmpty(searchEntity.ClientCode))
                sql = sql.Where(u => u.ClientCode == searchEntity.ClientCode);

            if (soList != null)
                sql = sql.Where(u => soList.Contains(u.SoNumber));

            if (!string.IsNullOrEmpty(searchEntity.CustomerPoNumber))
                sql = sql.Where(u => u.CustomerPoNumber == searchEntity.CustomerPoNumber);
            if (searchEntity.BeginCreateDate != null)
            {
                sql = sql.Where(u => u.CreateDate >= searchEntity.BeginCreateDate);
            }
            if (searchEntity.EndCreateDate != null)
            {
                sql = sql.Where(u => u.CreateDate < searchEntity.EndCreateDate);
            }

            total = sql.Count();
            sql = sql.OrderBy(u => u.SoNumber).ThenBy(u => u.ClientCode);
            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);
            return sql.ToList();
        }

        //删除预录入
        public string DeleteInBound(string whCode, string clientCode, string customerPoNumber, string soNumber)
        {
            List<InBoundOrder> inorderlist = idal.IInBoundOrderDAL.SelectBy(u => u.CustomerPoNumber == customerPoNumber && u.WhCode == whCode && u.ClientCode == clientCode);
            if (inorderlist.Count > 0)
            {
                if (!string.IsNullOrEmpty(soNumber))
                {
                    List<InBoundSO> inBoundSOList = idal.IInBoundSODAL.SelectBy(u => u.WhCode == whCode && u.SoNumber == soNumber && u.ClientCode == clientCode);
                    if (inBoundSOList.Count == 0)
                    {
                        return "预录入数据有误！";
                    }

                    int[] soId = (from a in inBoundSOList select a.Id).ToArray();

                    var poId = from a in inorderlist where soId.Contains((Int32)(a.SoId ?? 0)) select a.Id;

                    List<InBoundOrderDetail> inorderdetaillist = idal.IInBoundOrderDetailDAL.SelectBy(u => poId.Contains(u.PoId));
                    if (inorderdetaillist.Where(u => u.RegQty > 0).Count() > 0)
                    {
                        return "预录入已有收货登记，无法删除！";
                    }
                    else
                    {
                        //删除预录入要判断 是否有直装订单
                        var inorderdetailId = from b in inorderdetaillist select b.Id;
                        List<OutBoundOrderDetail> outOrderDetailList = idal.IOutBoundOrderDetailDAL.SelectBy(u => inorderdetailId.Contains((Int32)(u.InBoundOrderDetailId ?? 0)));
                        if (outOrderDetailList.Count > 0)
                        {
                            return "预录入包含直装订单，无法删除！";
                        }
                        else
                        {
                            idal.IInBoundOrderDetailDAL.DeleteByExtended(u => poId.Contains(u.PoId));
                            idal.IInBoundOrderDAL.DeleteByExtended(u => poId.Contains(u.Id));

                            List<InBoundOrder> checklist = idal.IInBoundOrderDAL.SelectBy(u => soId.Contains(u.SoId ?? 0));
                            if (checklist.Count == 0)
                            {
                                idal.IInBoundSODAL.DeleteByExtended(u => soId.Contains(u.Id));
                            }
                        }
                    }
                }
                else
                {
                    var poId = from a in inorderlist select a.Id;

                    List<InBoundOrderDetail> inorderdetaillist = idal.IInBoundOrderDetailDAL.SelectBy(u => poId.Contains(u.PoId));
                    if (inorderdetaillist.Where(u => u.RegQty > 0).Count() > 0)
                    {
                        return "预录入已有收货登记，无法删除！";
                    }
                    else
                    {
                        //删除预录入要判断 是否有直装订单
                        var inorderdetailId = from b in inorderdetaillist select b.Id;
                        List<OutBoundOrderDetail> outOrderDetailList = idal.IOutBoundOrderDetailDAL.SelectBy(u => inorderdetailId.Contains((Int32)(u.InBoundOrderDetailId ?? 0)));
                        if (outOrderDetailList.Count > 0)
                        {
                            return "预录入包含直装订单，无法删除！";
                        }
                        else
                        {
                            idal.IInBoundOrderDetailDAL.DeleteByExtended(u => poId.Contains(u.PoId));
                            idal.IInBoundOrderDAL.DeleteByExtended(u => poId.Contains(u.Id));
                        }
                    }
                }
            }

            return "Y";
        }

        //一键删除SO预录入
        public string DelInBoundBySO(string whCode, string clientCode, string soNumber)
        {
            if (string.IsNullOrEmpty(clientCode) || string.IsNullOrEmpty(soNumber))
            {
                return "基础数据有误，无法删除！";
            }
            if (!string.IsNullOrEmpty(soNumber))
            {
                List<InBoundSO> inBoundSOList = idal.IInBoundSODAL.SelectBy(u => u.WhCode == whCode && u.SoNumber == soNumber && u.ClientCode == clientCode);
                if (inBoundSOList.Count == 0)
                {
                    return "预录入数据有误！";
                }

                int[] soId = (from a in inBoundSOList select a.Id).ToArray();

                var poId = from a in idal.IInBoundOrderDAL.SelectAll() where soId.Contains((a.SoId ?? 0)) select a.Id;

                List<InBoundOrderDetail> inorderdetaillist = idal.IInBoundOrderDetailDAL.SelectBy(u => poId.Contains(u.PoId));
                if (inorderdetaillist.Where(u => u.RegQty > 0).Count() > 0)
                {
                    return "预录入已有收货登记，无法删除！";
                }
                else
                {
                    //删除预录入要判断 是否有直装订单
                    var inorderdetailId = from b in inorderdetaillist select b.Id;
                    List<OutBoundOrderDetail> outOrderDetailList = idal.IOutBoundOrderDetailDAL.SelectBy(u => inorderdetailId.Contains((Int32)(u.InBoundOrderDetailId ?? 0)));
                    if (outOrderDetailList.Count > 0)
                    {
                        return "预录入包含直装订单，无法删除！";
                    }
                    else
                    {
                        idal.IInBoundOrderDetailDAL.DeleteByExtended(u => poId.Contains(u.PoId));
                        idal.IInBoundOrderDAL.DeleteByExtended(u => poId.Contains(u.Id));
                        idal.IInBoundSODAL.DeleteByExtended(u => soId.Contains(u.Id));
                    }
                }
            }

            return "Y";
        }

        #endregion

        #region 2.DC预录入

        //DC批量添加预录入
        //对应InBoundOrderDCController中的 AddInBoundOrder 方法
        public string InBoundOrderListAddDC(InBoundOrderInsert entity)
        {
            string result = "";     //执行总结果

            if (entity == null || entity.InBoundOrderDetailInsert == null)
            {
                return "数据有误，请重新操作！";
            }

            foreach (var item in entity.InBoundOrderDetailInsert)
            {
                #region 添加InBoundOrder  
                //判断客户PO是否存在
                List<InBoundOrder> listInBoundOrder = idal.IInBoundOrderDAL.SelectBy(u => u.CustomerPoNumber == item.CustomerPoNumber && u.WhCode == entity.WhCode && u.ClientId == entity.ClientId && u.SoId == null);

                InBoundOrder inBoundOrder = InsertInBoundOrder(entity, item, null, listInBoundOrder);

                #endregion

                #region 添加ItemMaster
                //判断款号是否存在
                List<ItemMaster> listItemMaster = idal.IItemMasterDAL.SelectBy(u => u.WhCode == entity.WhCode && u.AltItemNumber == item.AltItemNumber && (u.Style1 == null ? "" : u.Style1) == (item.Style1 == null ? "" : item.Style1) && (u.Style2 == null ? "" : u.Style2) == (item.Style2 == null ? "" : item.Style2) && (u.Style3 == null ? "" : u.Style3) == (item.Style3 == null ? "" : item.Style3) && u.ClientId == entity.ClientId).OrderBy(u => u.Id).ToList();
                ItemMaster itemMaster = new ItemMaster();
                if (listItemMaster.Count == 0)
                {
                    result += "<font color=red>" + item.CustomerPoNumber + " " + item.AltItemNumber + " " + item.Qty + " " + item.UnitName + "添加失败,款号有误!</font><br>";
                    continue;
                }
                else
                {
                    itemMaster = listItemMaster.First();
                }

                #endregion

                #region 添加InBoundOrderDetail

                int insertResult = InsertInBoundOrderDetail(entity, item, inBoundOrder, itemMaster);
                if (insertResult == 1)
                {
                    result += item.JsonId + " " + item.CustomerPoNumber + " " + item.AltItemNumber + " " + item.Qty + "添加成功!<br>";
                }
                else if (insertResult == 2)
                {
                    result += item.JsonId + " " + "<font color=red>" + item.CustomerPoNumber + " " + item.AltItemNumber + "更新成功,数量累加" + item.Qty + "!</font><br>";
                }
                else
                {
                    result += "<font color=red>保存出错!</font><br>";
                }

                #endregion
            }

            return result;
        }

        //DC预录入明细列表
        //对应InBoundOrderDCController中的 List 方法
        public List<InBoundOrderDetailResult> InBoundOrderDetailListDC(InBoundOrderDetailSearch searchEntity, string[] itemNumberList, out int total, out string str)
        {
            var sql = from a in idal.IInBoundOrderDAL.SelectAll()
                      where a.WhCode == searchEntity.WhCode && a.ClientId == searchEntity.ClientId && a.CustomerPoNumber == searchEntity.CustomerPoNumber && a.OrderType == searchEntity.OrderType
                      join d in idal.IInBoundOrderDetailDAL.SelectAll()
                      on new { B = a.Id } equals new { B = d.PoId }
                      join c in idal.IItemMasterDAL.SelectAll()
                     on new { B = d.ItemId } equals new { B = c.Id }
                      select new InBoundOrderDetailResult
                      {
                          Id = d.Id,
                          WhCode = d.WhCode,
                          CustomerPoNumber = a.CustomerPoNumber,
                          AltItemNumber = c.AltItemNumber,
                          Style1 = c.Style1,
                          Style2 = c.Style2,
                          Style3 = c.Style3,
                          Qty = d.Qty,
                          RegQty = d.RegQty,
                          Weight = d.Weight,
                          UnitName = d.UnitName,
                          UnitId = d.UnitId,
                          CBM = d.CBM
                      };

            if (itemNumberList != null)
                sql = sql.Where(u => itemNumberList.Contains(u.AltItemNumber));

            total = sql.Count();
            str = "";
            if (total > 0)
            {
                str = "{\"预录入数量\":\"" + sql.Sum(u => u.Qty).ToString() + "\",\"已登记数量\":\"" + sql.Sum(u => u.RegQty).ToString() + "\"}";
            }
            sql = sql.OrderByDescending(u => u.Id);
            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);
            return sql.ToList();
        }

        //DC编辑预录入查询列表
        //对应InBoundOrderDCController中的 ListDetail 方法
        public List<InBoundOrderResult> InBoundListDetailDC(InBoundOrderSearch searchEntity, out int total)
        {
            var sql = from a in idal.IInBoundOrderDAL.SelectAll()
                      select new InBoundOrderResult
                      {
                          Action = "",
                          WhCode = a.WhCode,
                          ClientId = a.ClientId,
                          ClientCode = a.ClientCode,
                          CustomerPoNumber = a.CustomerPoNumber
                      };

            if (!string.IsNullOrEmpty(searchEntity.CustomerPoNumber))
                sql = sql.Where(u => u.CustomerPoNumber.Contains(searchEntity.CustomerPoNumber));

            total = sql.Count();
            sql = sql.OrderByDescending(u => u.ClientCode);
            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);
            return sql.ToList();
        }

        #endregion

        #region 3.通用预录入

        //添加预录入
        public string InBoundOrderListAddCommon(InBoundOrderInsert entity)
        {
            string result = "";     //执行总结果

            if (entity == null || entity.InBoundOrderDetailInsert == null)
            {
                return "数据有误，请重新操作！";
            }


            foreach (var item in entity.InBoundOrderDetailInsert)
            {
                InBoundOrder inBoundOrder = new InBoundOrder();

                if (entity.SoNumber != null && entity.SoNumber != "")
                {
                    //添加InBoundSO 
                    InBoundSO inBoundSO = InsertInBoundSO(entity, item);

                    //添加InBoundOrder  
                    //判断客户PO是否存在
                    List<InBoundOrder> listInBoundOrder = idal.IInBoundOrderDAL.SelectBy(u => u.CustomerPoNumber == item.CustomerPoNumber && u.WhCode == entity.WhCode && u.SoId == inBoundSO.Id);

                    inBoundOrder = InsertInBoundOrder(entity, item, inBoundSO, listInBoundOrder);
                }
                else
                {
                    //添加InBoundOrder  
                    //判断客户PO是否存在
                    List<InBoundOrder> listInBoundOrder = idal.IInBoundOrderDAL.SelectBy(u => u.CustomerPoNumber == item.CustomerPoNumber && u.WhCode == entity.WhCode && u.ClientId == entity.ClientId && u.SoId == null);

                    inBoundOrder = InsertInBoundOrder(entity, item, null, listInBoundOrder);
                }

                //判断款号是否存在
                ItemMaster itemMaster = InsertItemMaster(entity, item);

                //添加InBoundOrderDetail
                int insertResult = InsertInBoundOrderDetail(entity, item, inBoundOrder, itemMaster);
                if (insertResult == 1)
                {
                    result += item.JsonId + " " + item.CustomerPoNumber + " " + item.AltItemNumber + " " + item.Qty + "添加成功!<br>";
                }
                else if (insertResult == 2)
                {
                    result += item.JsonId + " " + "<font color=red>" + item.CustomerPoNumber + " " + item.AltItemNumber + "更新成功,数量累加" + item.Qty + "!</font><br>";
                }
                else
                {
                    result += "<font color=red>保存出错!</font><br>";
                }
            }

            return result;
        }

        //新增款号
        public ItemMaster InsertItemMaster(InBoundOrderInsert entity, InBoundOrderDetailInsert item)
        {
            List<ItemMaster> listItemMaster = idal.IItemMasterDAL.SelectBy(u => u.WhCode == entity.WhCode && u.AltItemNumber == item.AltItemNumber && (u.Style1 == null ? "" : u.Style1) == (item.Style1 == null ? "" : item.Style1) && (u.Style2 == null ? "" : u.Style2) == (item.Style2 == null ? "" : item.Style2) && (u.Style3 == null ? "" : u.Style3) == (item.Style3 == null ? "" : item.Style3) && u.ClientId == entity.ClientId).OrderBy(u => u.Id).ToList();
            ItemMaster itemMaster = new ItemMaster();
            if (listItemMaster.Count == 0)
            {
                itemMaster.ItemNumber = "SYS" + DI.IDGenerator.NewId;
                itemMaster.WhCode = entity.WhCode;
                itemMaster.AltItemNumber = item.AltItemNumber;
                itemMaster.ClientId = (int)entity.ClientId;
                itemMaster.ClientCode = entity.ClientCode;
                itemMaster.Style1 = item.Style1 ?? "";
                itemMaster.Style2 = item.Style2 ?? "";
                itemMaster.Style3 = item.Style3 ?? "";
                itemMaster.UnitFlag = 0;
                if (item.UnitName == "" || item.UnitName == null)
                {
                    itemMaster.UnitName = "none";
                }
                else
                {
                    itemMaster.UnitName = item.UnitName;
                }

                itemMaster.CreateUser = item.CreateUser;
                itemMaster.CreateDate = DateTime.Now;
                idal.IItemMasterDAL.Add(itemMaster);   //款号不存在就新增
            }
            else
            {
                itemMaster = listItemMaster.First();
            }

            idal.IItemMasterDAL.SaveChanges();
            return itemMaster;
        }


        //通过款号得到单位
        public IEnumerable<UnitsResult> GetUnitSelList(UnitsSearch entity)
        {
            List<ItemMaster> itemList = idal.IItemMasterDAL.SelectBy(u => u.WhCode == entity.WhCode && u.AltItemNumber == entity.AltItemNumber && u.ClientId == entity.ClientId && (u.Style1 == null ? "" : u.Style1) == (entity.Style1 == null ? "" : entity.Style1) && (u.Style2 == null ? "" : u.Style2) == (entity.Style2 == null ? "" : entity.Style2) && (u.Style3 == null ? "" : u.Style3) == (entity.Style3 == null ? "" : entity.Style3)).OrderBy(u => u.Id).ToList();

            if (itemList.Count > 0)
            {
                var sql = from a in itemList select new UnitsResult { UnitName = a.UnitName, Id = 0 };
                if (sql.Count() > 0)
                {
                    return sql.AsEnumerable();
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        //通过款号 验证单位 是否有误
        public string CheckUnitName(InBoundOrderInsert entity)
        {
            string result = "";     //执行总结果
            int recount = 0;
            if (entity == null || entity.InBoundOrderDetailInsert == null)
            {
                return "数据有误，请重新操作！";
            }

            foreach (var item in entity.InBoundOrderDetailInsert)
            {
                //判断款号是否存在
                List<ItemMaster> listItemMaster = idal.IItemMasterDAL.SelectBy(u => u.WhCode == entity.WhCode && u.AltItemNumber == item.AltItemNumber && u.Style1 == item.Style1 && u.Style2 == item.Style2 && u.Style3 == item.Style3 && u.ClientId == entity.ClientId).OrderBy(u => u.Id).ToList();

                if (listItemMaster.Count != 0)
                {
                    ItemMaster itemMaster = listItemMaster.First();
                    if (itemMaster.UnitFlag > 0)
                    {
                        var sql = idal.IUnitDAL.SelectBy(u => u.WhCode == itemMaster.WhCode && u.ItemId == itemMaster.Id);
                        foreach (var unit in sql)
                        {
                            if (unit.Id == item.UnitId && unit.UnitName == item.UnitName)
                            {
                                recount++;
                            }
                            else
                            {
                                result = "N款号" + itemMaster.AltItemNumber + "单位录入有误,请重新录入!$" + item.JsonId;
                            }
                        }
                    }
                    else
                    {
                        recount++;
                    }
                }
                else
                {
                    recount++;
                }
            }

            if (recount != entity.InBoundOrderDetailInsert.Count)
            {
                return result;
            }
            else
            {
                return "Y";
            }
        }


        #endregion

        #region 4.电商订单导入
        public string InBoundOrderListAddEcl(InBoundOrderInsert entity)
        {
            if (entity == null || entity.InBoundOrderDetailInsert == null)
            {
                return "数据有误，请重新操作！";
            }

            string result = "";     //执行总结果
            string CustomerPoNumber = "";
            RegInBoundOrderManager rm = new RegInBoundOrderManager();
            int ClientId = idal.IWhClientDAL.SelectBy(u => u.ClientCode == entity.ClientCode && u.WhCode == entity.WhCode).First().Id;


            #region 判断订单号码是否存在
            if (entity.InBoundOrderDetailInsert.Count() == 0)
            {
                return "失败!无明细!";
            }
            else
            {
                CustomerPoNumber = entity.InBoundOrderDetailInsert.First().CustomerPoNumber;
            }

            if (idal.IInBoundOrderDAL.SelectBy(u => u.CustomerPoNumber == CustomerPoNumber && u.ClientCode == entity.ClientCode && u.WhCode == entity.WhCode).Count() > 0)
            {
                return "重复导入!";
            };

            #endregion


            foreach (var item in entity.InBoundOrderDetailInsert)
            {


                #region 添加InBoundOrder  
                //判断客户PO是否存在
                List<InBoundOrder> listInBoundOrder = idal.IInBoundOrderDAL.SelectBy(u => u.CustomerPoNumber == item.CustomerPoNumber && u.WhCode == entity.WhCode && u.ClientCode == entity.ClientCode);
                InBoundOrder inBoundOrder = new InBoundOrder();
                if (listInBoundOrder.Count == 0)
                {
                    inBoundOrder.WhCode = entity.WhCode;
                    inBoundOrder.PoNumber = "ST" + DI.IDGenerator.NewId;
                    inBoundOrder.CustomerPoNumber = item.CustomerPoNumber;
                    inBoundOrder.AltCustomerPoNumber = item.AltCustomerPoNumber;
                    inBoundOrder.ClientId = ClientId;
                    inBoundOrder.ClientCode = entity.ClientCode;
                    inBoundOrder.OrderType = entity.OrderType;
                    inBoundOrder.ProcessId = entity.ProcessId;
                    inBoundOrder.ProcessName = entity.ProcessName;
                    inBoundOrder.OrderSource = "ECL";
                    inBoundOrder.CreateUser = item.CreateUser;
                    inBoundOrder.CreateDate = DateTime.Now;
                    idal.IInBoundOrderDAL.Add(inBoundOrder);   //不存在就新增
                }
                else
                {
                    inBoundOrder = listInBoundOrder.First();
                }

                #endregion

                #region 添加ItemMaster
                //判断款号是否存在
                List<ItemMaster> listItemMaster = idal.IItemMasterDAL.SelectBy(u => u.WhCode == entity.WhCode && u.AltItemNumber == item.AltItemNumber && (u.Style1 == null ? "" : u.Style1) == (item.Style1 == null ? "" : item.Style1) && (u.Style2 == null ? "" : u.Style2) == (item.Style2 == null ? "" : item.Style2) && (u.Style3 == null ? "" : u.Style3) == (item.Style3 == null ? "" : item.Style3) && u.ClientId == ClientId).OrderBy(u => u.Id).ToList();
                ItemMaster itemMaster = new ItemMaster();
                if (listItemMaster.Count == 0)
                {
                    itemMaster.ItemNumber = "SYS" + DI.IDGenerator.NewId;
                    itemMaster.WhCode = entity.WhCode;
                    itemMaster.AltItemNumber = item.AltItemNumber;
                    itemMaster.ClientId = ClientId;
                    itemMaster.ClientCode = entity.ClientCode;
                    itemMaster.Style1 = item.Style1 ?? "";
                    itemMaster.Style2 = item.Style2 ?? "";
                    itemMaster.Style3 = item.Style3 ?? "";
                    itemMaster.UnitFlag = 0;

                    if (item.UnitName == "" || item.UnitName == null)
                    {
                        itemMaster.UnitName = "none";
                    }
                    else
                    {
                        itemMaster.UnitName = item.UnitName;
                    }

                    itemMaster.CreateUser = item.CreateUser;
                    itemMaster.CreateDate = DateTime.Now;
                    idal.IItemMasterDAL.Add(itemMaster);   //款号不存在就新增
                }
                else
                {
                    itemMaster = listItemMaster.First();
                }
                idal.IItemMasterDAL.SaveChanges();
                #endregion

                #region 添加InBoundOrderDetail

                List<InBoundOrderDetail> listInBoundOrderDetail = idal.IInBoundOrderDetailDAL.SelectBy(u => u.WhCode == entity.WhCode && u.ItemId == itemMaster.Id && u.PoId == inBoundOrder.Id && u.UnitId == item.UnitId).ToList();

                if (listInBoundOrderDetail.Count == 0)
                {
                    InBoundOrderDetail inBoundOrderDetail = new InBoundOrderDetail();
                    inBoundOrderDetail.WhCode = entity.WhCode;
                    inBoundOrderDetail.PoId = inBoundOrder.Id;
                    inBoundOrderDetail.ItemId = itemMaster.Id;
                    inBoundOrderDetail.UnitId = item.UnitId;

                    if (item.UnitName == "" || item.UnitName == null)
                    {
                        inBoundOrderDetail.UnitName = "none";
                    }
                    else
                    {
                        inBoundOrderDetail.UnitName = item.UnitName;
                    }

                    inBoundOrderDetail.Qty = item.Qty;
                    inBoundOrderDetail.Weight = item.Weight;
                    inBoundOrderDetail.CBM = item.CBM;
                    inBoundOrderDetail.CreateUser = item.CreateUser;
                    inBoundOrderDetail.CreateDate = DateTime.Now;
                    idal.IInBoundOrderDetailDAL.Add(inBoundOrderDetail);
                    //result += item.CustomerPoNumber + " " + item.AltItemNumber + " " + item.Qty + "添加成功!<br>";
                }
                else
                {
                    InBoundOrderDetail inBoundOrderDetail = listInBoundOrderDetail.First();
                    int yuluruQty = inBoundOrderDetail.Qty - inBoundOrderDetail.RegQty;
                    if (yuluruQty < item.Qty)
                    {
                        inBoundOrderDetail.Qty = inBoundOrderDetail.Qty + (item.Qty - yuluruQty);
                        inBoundOrderDetail.UpdateUser = item.CreateUser;
                        inBoundOrderDetail.UpdateDate = DateTime.Now;
                        idal.IInBoundOrderDetailDAL.UpdateBy(inBoundOrderDetail, u => u.Id == inBoundOrderDetail.Id, new string[] { "Qty", "UpdateUser", "UpdateDate" });
                    }

                    //result += "<font color=red>" + item.CustomerPoNumber + " " + item.AltItemNumber + "更新成功,数量累加" + item.Qty + "!</font><br>";
                }

                #endregion
            }
            idal.IInBoundOrderDetailDAL.SaveChanges();

            #region 创建收货登记表头
            ReceiptRegister rr = new ReceiptRegister();
            rr.ClientCode = entity.ClientCode;
            rr.ClientId = ClientId;
            rr.CreateDate = DateTime.Now;
            rr.CreateUser = "ECL";
            rr.LocationId = "DD01";
            rr.ProcessId = entity.ProcessId;
            rr.ProcessName = entity.ProcessName;
            rr.WhCode = entity.WhCode;
            rr.Status = "U";
            rr.ReceiptType = "Ecl";
            rr.RegisterDate = DateTime.Now;
            rr.TruckNumber = "";
            //rr.ArriveDate = Convert.ToDateTime(Request["txt_ArriveDate"]);

            rr = rm.AddReceiptRegister(rr);
            #endregion

            #region 添加收货操作单明细
            List<ReceiptRegisterInsert> rrilist = new List<ReceiptRegisterInsert>();
            ReceiptRegisterInsert rri;
            ItemMaster sku;
            InBoundOrder ibod = idal.IInBoundOrderDAL.SelectBy(u => u.WhCode == entity.WhCode && u.ClientId == ClientId && u.CustomerPoNumber == CustomerPoNumber).First();
            List<InBoundOrderDetail> iboddetail = idal.IInBoundOrderDetailDAL.SelectBy(u => u.PoId == ibod.Id);

            foreach (var item in iboddetail)
            {
                sku = idal.IItemMasterDAL.SelectBy(u => u.Id == item.ItemId).First();

                rri = new ReceiptRegisterInsert();
                rri.ReceiptId = rr.ReceiptId;
                rri.WhCode = item.WhCode;
                rri.InBoundOrderDetailId = item.Id;
                rri.CustomerPoNumber = CustomerPoNumber;
                rri.AltItemNumber = sku.AltItemNumber;
                rri.PoId = item.PoId;
                rri.ItemId = item.ItemId;
                rri.UnitId = (int)item.UnitId;
                rri.ProcessName = rr.ProcessName;
                rri.ProcessId = (int)rr.ProcessId;

                if (item.UnitName == "" || item.UnitName == null)
                {
                    rri.UnitName = "none";
                }
                else
                {
                    rri.UnitName = item.UnitName;
                }

                rri.RegQty = item.Qty;
                rri.CreateUser = "ECL";
                rri.CreateDate = DateTime.Now;

                rrilist.Add(rri);

            }

            rm.AddReceiptRegisterDetail(rrilist);
            idal.IInBoundOrderDetailDAL.SaveChanges();
            #endregion

            return "Y$" + rr.ReceiptId;
        }



        #endregion

        #region 6.删除入库订单
        public string DeleteInorderBySO(string SoNumber, string WhCode, string ClientCode)
        {
            List<InBoundSO> SoL = idal.IInBoundSODAL.SelectBy(u => u.WhCode == WhCode && u.ClientCode == ClientCode && u.SoNumber == SoNumber);
            if (SoL.Count == 0)
            {
                return ClientCode + SoNumber + "不存在!";
            }
            InBoundSO SO = SoL.First();


            List<InBoundOrder> InOrderL = idal.IInBoundOrderDAL.SelectBy(u => u.SoId == SO.Id);
            if (InOrderL.Count > 0)
            {

                List<int> SL = new List<int>();
                foreach (var item in InOrderL)
                {
                    SL.Add(item.Id);
                }

                int RegQty = idal.IInBoundOrderDetailDAL.SelectBy(u => SL.Contains(u.PoId) && u.RegQty > 0).Count();
                if (RegQty > 0)
                {
                    return ClientCode + SoNumber + "存在已做收货登记数量,不可删除";
                }
                else
                {
                    idal.IInBoundOrderDetailDAL.DeleteBy(u => SL.Contains(u.PoId));
                }
                idal.IInBoundOrderDAL.DeleteBy(u => SL.Contains(u.Id));

            }
            idal.IInBoundSODAL.Delete(SoL.First());
            idal.SaveChanges();

            return "Y";
        }

        #endregion

        #region 7.批量导入预录入

        //验证客户收货流程是否一致
        public List<FlowHeadResult> CheckClientCodeRule(string whCode, List<WhClient> client)
        {
            List<FlowHeadResult> result = new List<FlowHeadResult>();

            List<FlowHeadResult> sql = (from a in idal.IWhClientDAL.SelectAll()
                                        join b in idal.IR_Client_FlowRuleDAL.SelectAll() on new { Id = a.Id } equals new { Id = b.ClientId } into b_join
                                        from b in b_join.DefaultIfEmpty()
                                        join c in idal.IFlowHeadDAL.SelectAll() on new { BusinessFlowGroupId = (Int32)b.BusinessFlowGroupId } equals new { BusinessFlowGroupId = c.Id } into c_join
                                        from c in c_join.DefaultIfEmpty()
                                        where
                                          a.WhCode == whCode && c.Type == "InBound"
                                        select new FlowHeadResult
                                        {
                                            Remark = a.ClientCode,
                                            FlowName = c.FlowName,
                                            Id = c.Id
                                        }).ToList();

            string[] clientCode = (from a in client
                                   select a.ClientCode).Distinct().ToArray();

            sql = sql.Where(u => clientCode.Contains(u.Remark)).ToList();
            if (sql.Count == 0)
            {
                return result;
            }
            else
            {
                //得到第一个客户的流程
                List<FlowHeadResult> first = sql.Where(u => u.Remark == clientCode[0]).ToList();
                if (clientCode.Length == 1)
                {
                    result = first;
                    return result;
                }

                //循环第二个客户以后的流程
                for (int i = 1; i < clientCode.Length; i++)
                {
                    List<FlowHeadResult> check = sql.Where(u => u.Remark == clientCode[i]).ToList();
                    if (i == 1)
                    {
                        //如果第一个客户的流程多余第二个客户的流程
                        if (first.Count >= check.Count)
                        {
                            foreach (var item in first)
                            {
                                if (check.Where(u => u.FlowName == item.FlowName && u.Id == item.Id).Count() > 0)
                                {
                                    FlowHeadResult f = new FlowHeadResult();
                                    f.FlowName = item.FlowName;
                                    f.Id = item.Id;
                                    result.Add(f);
                                }
                            }
                        }
                        else
                        {
                            foreach (var item in check)
                            {
                                if (first.Where(u => u.FlowName == item.FlowName && u.Id == item.Id).Count() > 0)
                                {
                                    FlowHeadResult f = new FlowHeadResult();
                                    f.FlowName = item.FlowName;
                                    f.Id = item.Id;
                                    result.Add(f);
                                }
                            }
                        }
                        //如果第一个客户和第二个客户 没有共同的流程 直接断开循环 返回Null
                        if (result.Count == 0)
                        {
                            break;
                        }
                    }
                    else
                    {
                        List<FlowHeadResult> resulttemp = new List<FlowHeadResult>();

                        foreach (var item in result)
                        {
                            if (check.Where(u => u.FlowName == item.FlowName && u.Id == item.Id).Count() > 0)
                            {
                                FlowHeadResult f = new FlowHeadResult();
                                f.FlowName = item.FlowName;
                                f.Id = item.Id;
                                resulttemp.Add(f);
                            }
                        }

                        if (resulttemp.Count == 0)
                        {
                            result.Clear();
                        }
                        else
                        {
                            if (resulttemp.Count < result.Count)
                            {
                                result = resulttemp;
                            }
                        }
                    }
                }

                return result;
            }
        }

        //批量导入预录入
        public string ImportsInBoundOrder(List<InBoundOrderInsert> entityList)
        {
            string result = "";     //执行总结果
            int soId;

            if (entityList == null)
            {
                return "数据有误，请重新操作！";
            }

            string whCode = entityList.First().WhCode;

            List<WhClient> clientList = idal.IWhClientDAL.SelectBy(u => u.WhCode == whCode && u.Status == "Active");

            string[] clientCode = (from a in entityList select a.ClientCode).Distinct().ToArray();

            if (clientList.Where(u => clientCode.Contains(u.ClientCode)).Count() == 0)
            {
                return "客户名不存在，请检查客户名或大小写！";
            }

            string[] soArr = (from a in entityList select a.SoNumber).ToList().Distinct().ToArray();
            string[] clientArr = (from a in entityList select a.ClientCode).ToList().Distinct().ToArray();

            List<InBoundSO> checkInBound = idal.IInBoundSODAL.SelectBy(u => u.WhCode == whCode && clientArr.Contains(u.ClientCode) && soArr.Contains(u.SoNumber));

            foreach (var entity in entityList)
            {
                soId = 0;
                if (result != "")
                {
                    break;
                }
                if (!string.IsNullOrEmpty(entity.SoNumber))
                {
                    List<InBoundSO> ibsoL = checkInBound.Where(u => u.SoNumber == entity.SoNumber && u.ClientCode == entity.ClientCode).ToList();
                    if (ibsoL.Count > 0)
                    {
                        soId = ibsoL.First().Id;
                    }
                }

                string[] poArr = (from a in entity.InBoundOrderDetailInsert select a.CustomerPoNumber).ToList().Distinct().ToArray();

                var sqlCheck = from a in idal.IInBoundOrderDAL.SelectAll()
                               where a.ClientCode == entity.ClientCode && a.WhCode == whCode && poArr.Contains(a.CustomerPoNumber) && (a.SoId ?? 0) == soId && a.ProcessId != entity.ProcessId
                               select a;

                if (sqlCheck.Count() > 0)
                {
                    result = "已存在不同流程的录入:" + sqlCheck.First().CustomerPoNumber + " 无法添加，请更换流程！";
                    break;
                }

            }
            if (result != "")
            {
                return result;
            }

            InBoundOrderInsert fir = entityList.First();

            List<string> soList = new List<string>();
            List<string> poList = new List<string>();
            List<string> skuList = new List<string>();

            //批量导入预录入SO
            List<InBoundSO> InBoundSOAddList = new List<InBoundSO>();
            foreach (var entity in entityList)
            {
                if (!string.IsNullOrEmpty(entity.SoNumber))
                {
                    WhClient client = clientList.Where(u => u.ClientCode == entity.ClientCode && u.WhCode == entity.WhCode).First();

                    entity.ClientId = client.Id;

                    if (entity.ProcessId == null || entity.ProcessId == 0)
                    {
                        List<R_Client_FlowRule> a1 = idal.IR_Client_FlowRuleDAL.SelectBy(u => u.ClientId == client.Id && u.Type == "InBound");
                        if (a1.Count > 0)
                        {
                            int FlowHead = (int)a1.First().BusinessFlowGroupId;
                            List<FlowHead> a2 = idal.IFlowHeadDAL.SelectBy(u => u.Id == FlowHead);
                            if (a2.Count > 0)
                            {
                                entity.ProcessId = a2.First().Id;
                                entity.ProcessName = a2.First().FlowName;
                            }
                            else
                            {
                                result = "未找到客户收货流程！" + entity.ClientCode;
                                break;
                            }
                        }
                        else
                        {
                            result = "未找到客户收货流程！" + entity.ClientCode;
                            break;
                        }
                    }

                    //var ChecklistInBoundSO = idal.IInBoundSODAL.SelectBy(u => u.WhCode == entity.WhCode && u.ClientCode == entity.ClientCode && u.SoNumber == entity.SoNumber);

                    if (checkInBound.Where(u => u.WhCode == entity.WhCode && u.ClientCode == entity.ClientCode && u.SoNumber == entity.SoNumber).Count() == 0)
                    {
                        if (InBoundSOAddList.Where(u => u.WhCode == entity.WhCode && u.ClientCode == entity.ClientCode && u.SoNumber == entity.SoNumber).Count() == 0)
                        {
                            InBoundSO inBoundSO = new InBoundSO();
                            inBoundSO.WhCode = entity.WhCode;
                            inBoundSO.SoNumber = entity.SoNumber;
                            inBoundSO.ClientCode = entity.ClientCode;
                            inBoundSO.ClientId = (int)entity.ClientId;              //添加新数据 必须赋予客户ID
                            inBoundSO.CreateUser = fir.CreateUser;
                            inBoundSO.CreateDate = DateTime.Now;
                            InBoundSOAddList.Add(inBoundSO);
                        }
                    }
                    soList.Add(entity.SoNumber);
                }
            }

            if (InBoundSOAddList.Count > 0)
            {
                idal.IInBoundSODAL.Add(InBoundSOAddList);
                idal.IInBoundSODAL.SaveChanges();
            }

            List<InBoundSO> listInBoundSO = idal.IInBoundSODAL.SelectBy(u => u.WhCode == fir.WhCode && soList.Contains(u.SoNumber));

            List<InBoundOrder> InBoundOrderAddList = new List<InBoundOrder>();
            List<ItemMaster> ItemMasterAddList = new List<ItemMaster>();

            List<InBoundOrder> checkInBoundOrderAddResult = new List<InBoundOrder>();
            List<ItemMaster> checkItemMasterAddResult = new List<ItemMaster>();

            //批量导入预录入PO
            foreach (var entity in entityList)
            {
                WhClient client = clientList.Where(u => u.ClientCode == entity.ClientCode && u.WhCode == entity.WhCode).First();

                entity.ClientId = client.Id;

                if (entity.ProcessId == null || entity.ProcessId == 0)
                {
                    List<R_Client_FlowRule> a1 = idal.IR_Client_FlowRuleDAL.SelectBy(u => u.ClientId == client.Id && u.Type == "InBound");
                    if (a1.Count > 0)
                    {
                        int FlowHead = (int)a1.First().BusinessFlowGroupId;
                        List<FlowHead> a2 = idal.IFlowHeadDAL.SelectBy(u => u.Id == FlowHead);
                        if (a2.Count > 0)
                        {
                            entity.ProcessId = a2.First().Id;
                            entity.ProcessName = a2.First().FlowName;
                        }
                        else
                        {
                            result = "未找到客户收货流程！" + entity.ClientCode;
                            break;
                        }
                    }
                    else
                    {
                        result = "未找到客户收货流程！" + entity.ClientCode;
                        break;
                    }
                }

                if (!string.IsNullOrEmpty(entity.SoNumber))
                {
                    InBoundSO inBoundSO = listInBoundSO.Where(u => u.SoNumber == entity.SoNumber && u.ClientCode == entity.ClientCode).First();

                    string[] getPoArr = (from a in entity.InBoundOrderDetailInsert select a.CustomerPoNumber).ToList().Distinct().ToArray();

                    List<InBoundOrder> checkPoArr = idal.IInBoundOrderDAL.SelectBy(u => getPoArr.Contains(u.CustomerPoNumber) && u.WhCode == entity.WhCode && u.ClientCode == entity.ClientCode && u.SoId == inBoundSO.Id);

                    string[] getSkuArr = (from a in entity.InBoundOrderDetailInsert select a.AltItemNumber).ToList().Distinct().ToArray();

                    List<ItemMaster> checkSkuArr = idal.IItemMasterDAL.SelectBy(u => u.WhCode == entity.WhCode && getSkuArr.Contains(u.AltItemNumber) && u.ClientId == entity.ClientId).OrderBy(u => u.Id).ToList();

                    foreach (var item in entity.InBoundOrderDetailInsert)
                    {
                        if (checkInBoundOrderAddResult.Where(u => u.SoId == inBoundSO.Id && u.CustomerPoNumber == item.CustomerPoNumber).Count() == 0)
                        {
                            if (checkPoArr.Where(u => u.CustomerPoNumber == item.CustomerPoNumber && u.WhCode == entity.WhCode && u.ClientCode == entity.ClientCode && u.SoId == inBoundSO.Id).Count() == 0)
                            {
                                InBoundOrder inBoundOrder = new InBoundOrder();
                                inBoundOrder.WhCode = entity.WhCode;
                                inBoundOrder.SoId = inBoundSO.Id;
                                inBoundOrder.PoNumber = "ST" + DI.IDGenerator.NewId;
                                inBoundOrder.CustomerPoNumber = item.CustomerPoNumber;
                                inBoundOrder.AltCustomerPoNumber = item.AltCustomerPoNumber;
                                inBoundOrder.ClientId = (int)entity.ClientId;
                                inBoundOrder.ClientCode = entity.ClientCode;
                                inBoundOrder.OrderType = entity.OrderType;
                                inBoundOrder.ProcessId = entity.ProcessId;
                                inBoundOrder.ProcessName = entity.ProcessName;
                                inBoundOrder.OrderSource = "WMS";
                                inBoundOrder.CreateUser = item.CreateUser;
                                inBoundOrder.CreateDate = DateTime.Now;
                                InBoundOrderAddList.Add(inBoundOrder);
                            }
                            poList.Add(item.CustomerPoNumber);

                            InBoundOrder inboundResult = new InBoundOrder();
                            inboundResult.SoId = inBoundSO.Id;
                            inboundResult.CustomerPoNumber = item.CustomerPoNumber;
                            checkInBoundOrderAddResult.Add(inboundResult);
                        }

                        if (checkItemMasterAddResult.Where(u => u.AltItemNumber == item.AltItemNumber && (u.Style1 == null ? "" : u.Style1) == (item.Style1 == null ? "" : item.Style1) && (u.Style2 == null ? "" : u.Style2) == (item.Style2 == null ? "" : item.Style2) && (u.Style3 == null ? "" : u.Style3) == (item.Style3 == null ? "" : item.Style3) && u.ClientId == entity.ClientId).Count() == 0)
                        {
                            if (checkSkuArr.Where(u => u.WhCode == entity.WhCode && u.AltItemNumber == item.AltItemNumber && (u.Style1 == null ? "" : u.Style1) == (item.Style1 == null ? "" : item.Style1) && (u.Style2 == null ? "" : u.Style2) == (item.Style2 == null ? "" : item.Style2) && (u.Style3 == null ? "" : u.Style3) == (item.Style3 == null ? "" : item.Style3) && u.ClientId == entity.ClientId).Count() == 0)
                            {
                                ItemMaster itemMaster = new ItemMaster();
                                itemMaster.ItemNumber = "SYS" + DI.IDGenerator.NewId;
                                itemMaster.WhCode = entity.WhCode;
                                itemMaster.AltItemNumber = item.AltItemNumber;
                                itemMaster.ClientId = (int)entity.ClientId;
                                itemMaster.ClientCode = entity.ClientCode;
                                itemMaster.Style1 = item.Style1 ?? "";
                                itemMaster.Style2 = item.Style2 ?? "";
                                itemMaster.Style3 = item.Style3 ?? "";
                                itemMaster.UnitFlag = 0;
                                if (item.UnitName == "" || item.UnitName == null)
                                {
                                    itemMaster.UnitName = "none";
                                }
                                else
                                {
                                    itemMaster.UnitName = item.UnitName;
                                }

                                itemMaster.CreateUser = item.CreateUser;
                                itemMaster.CreateDate = DateTime.Now;
                                ItemMasterAddList.Add(itemMaster);   //款号不存在就新增
                            }
                            skuList.Add(item.AltItemNumber);

                            ItemMaster itemResult = new ItemMaster();
                            itemResult.ClientId = (int)entity.ClientId;
                            itemResult.AltItemNumber = item.AltItemNumber;
                            itemResult.Style1 = item.Style1 ?? "";
                            itemResult.Style2 = item.Style2 ?? "";
                            itemResult.Style3 = item.Style3 ?? "";
                            checkItemMasterAddResult.Add(itemResult);
                        }
                    }
                }
                else
                {

                    string[] getPoArr = (from a in entity.InBoundOrderDetailInsert select a.CustomerPoNumber).ToList().Distinct().ToArray();

                    List<InBoundOrder> checkPoArr = idal.IInBoundOrderDAL.SelectBy(u => getPoArr.Contains(u.CustomerPoNumber) && u.WhCode == entity.WhCode && u.ClientCode == entity.ClientCode && u.SoId == null);

                    string[] getSkuArr = (from a in entity.InBoundOrderDetailInsert select a.AltItemNumber).ToList().Distinct().ToArray();

                    List<ItemMaster> checkSkuArr = idal.IItemMasterDAL.SelectBy(u => u.WhCode == entity.WhCode && getSkuArr.Contains(u.AltItemNumber) && u.ClientId == entity.ClientId).OrderBy(u => u.Id).ToList();

                    foreach (var item in entity.InBoundOrderDetailInsert)
                    {
                        if (checkInBoundOrderAddResult.Where(u => u.SoId == null && u.CustomerPoNumber == item.CustomerPoNumber).Count() == 0)
                        {
                            if (checkPoArr.Where(u => u.CustomerPoNumber == item.CustomerPoNumber && u.WhCode == entity.WhCode && u.ClientCode == entity.ClientCode && u.SoId == null).Count() == 0)
                            {
                                InBoundOrder inBoundOrder = new InBoundOrder();
                                inBoundOrder.WhCode = entity.WhCode;
                                inBoundOrder.PoNumber = "ST" + DI.IDGenerator.NewId;
                                inBoundOrder.CustomerPoNumber = item.CustomerPoNumber;
                                inBoundOrder.AltCustomerPoNumber = item.AltCustomerPoNumber;
                                inBoundOrder.ClientId = (int)entity.ClientId;
                                inBoundOrder.ClientCode = entity.ClientCode;
                                inBoundOrder.OrderType = entity.OrderType;
                                inBoundOrder.ProcessId = entity.ProcessId;
                                inBoundOrder.ProcessName = entity.ProcessName;
                                inBoundOrder.OrderSource = "WMS";
                                inBoundOrder.CreateUser = item.CreateUser;
                                inBoundOrder.CreateDate = DateTime.Now;
                                InBoundOrderAddList.Add(inBoundOrder);
                            }

                            poList.Add(item.CustomerPoNumber);

                            InBoundOrder inboundResult = new InBoundOrder();
                            inboundResult.SoId = null;
                            inboundResult.CustomerPoNumber = item.CustomerPoNumber;
                            checkInBoundOrderAddResult.Add(inboundResult);
                        }


                        if (checkItemMasterAddResult.Where(u => u.AltItemNumber == item.AltItemNumber && (u.Style1 == null ? "" : u.Style1) == (item.Style1 == null ? "" : item.Style1) && (u.Style2 == null ? "" : u.Style2) == (item.Style2 == null ? "" : item.Style2) && (u.Style3 == null ? "" : u.Style3) == (item.Style3 == null ? "" : item.Style3) && u.ClientId == entity.ClientId).Count() == 0)
                        {
                            if (checkSkuArr.Where(u => u.WhCode == entity.WhCode && u.AltItemNumber == item.AltItemNumber && (u.Style1 == null ? "" : u.Style1) == (item.Style1 == null ? "" : item.Style1) && (u.Style2 == null ? "" : u.Style2) == (item.Style2 == null ? "" : item.Style2) && (u.Style3 == null ? "" : u.Style3) == (item.Style3 == null ? "" : item.Style3) && u.ClientId == entity.ClientId).Count() == 0)
                            {
                                ItemMaster itemMaster = new ItemMaster();
                                itemMaster.ItemNumber = "SYS" + DI.IDGenerator.NewId;
                                itemMaster.WhCode = entity.WhCode;
                                itemMaster.AltItemNumber = item.AltItemNumber;
                                itemMaster.ClientId = (int)entity.ClientId;
                                itemMaster.ClientCode = entity.ClientCode;
                                itemMaster.Style1 = item.Style1 ?? "";
                                itemMaster.Style2 = item.Style2 ?? "";
                                itemMaster.Style3 = item.Style3 ?? "";
                                itemMaster.UnitFlag = 0;
                                if (item.UnitName == "" || item.UnitName == null)
                                {
                                    itemMaster.UnitName = "none";
                                }
                                else
                                {
                                    itemMaster.UnitName = item.UnitName;
                                }

                                itemMaster.CreateUser = item.CreateUser;
                                itemMaster.CreateDate = DateTime.Now;
                                ItemMasterAddList.Add(itemMaster);   //款号不存在就新增
                            }
                            skuList.Add(item.AltItemNumber);

                            ItemMaster itemResult = new ItemMaster();
                            itemResult.ClientId = (int)entity.ClientId;
                            itemResult.AltItemNumber = item.AltItemNumber;
                            itemResult.Style1 = item.Style1 ?? "";
                            itemResult.Style2 = item.Style2 ?? "";
                            itemResult.Style3 = item.Style3 ?? "";
                            checkItemMasterAddResult.Add(itemResult);
                        }
                    }
                }
            }

            if (InBoundOrderAddList.Count > 0)
            {
                idal.IInBoundOrderDAL.Add(InBoundOrderAddList);
                idal.IInBoundSODAL.SaveChanges();
            }

            if (ItemMasterAddList.Count > 0)
            {
                idal.IItemMasterDAL.Add(ItemMasterAddList);
                idal.IItemMasterDAL.SaveChanges();
            }

            List<InBoundOrder> listInBoundOrder = idal.IInBoundOrderDAL.SelectBy(u => poList.Contains(u.CustomerPoNumber) && u.WhCode == fir.WhCode);

            List<ItemMaster> listItemMaster = idal.IItemMasterDAL.SelectBy(u => skuList.Contains(u.AltItemNumber) && u.WhCode == fir.WhCode).OrderBy(u => u.Id).ToList();

            //批量导入预录入
            foreach (var entity in entityList)
            {
                if (result != "")
                {
                    break;
                }
                WhClient client = clientList.Where(u => u.ClientCode == entity.ClientCode && u.WhCode == entity.WhCode).First();

                entity.ClientId = client.Id;

                if (entity.ProcessId == null || entity.ProcessId == 0)
                {
                    List<R_Client_FlowRule> a1 = idal.IR_Client_FlowRuleDAL.SelectBy(u => u.ClientId == client.Id && u.Type == "InBound");
                    if (a1.Count > 0)
                    {
                        int FlowHead = (int)a1.First().BusinessFlowGroupId;
                        List<FlowHead> a2 = idal.IFlowHeadDAL.SelectBy(u => u.Id == FlowHead);
                        if (a2.Count > 0)
                        {
                            entity.ProcessId = a2.First().Id;
                            entity.ProcessName = a2.First().FlowName;
                        }
                        else
                        {
                            result = "未找到客户收货流程！" + entity.ClientCode;
                            break;
                        }
                    }
                    else
                    {
                        result = "未找到客户收货流程！" + entity.ClientCode;
                        break;
                    }
                }

                foreach (var item in entity.InBoundOrderDetailInsert)
                {
                    InBoundOrder inBoundOrder = new InBoundOrder();

                    if (!string.IsNullOrEmpty(entity.SoNumber))
                    {
                        InBoundSO inBoundSO = listInBoundSO.Where(u => u.SoNumber == entity.SoNumber && u.ClientCode == entity.ClientCode).First();

                        inBoundOrder = listInBoundOrder.Where(u => u.CustomerPoNumber == item.CustomerPoNumber && u.ClientCode == entity.ClientCode && u.SoId == inBoundSO.Id).First();
                    }
                    else
                    {
                        inBoundOrder = listInBoundOrder.Where(u => u.CustomerPoNumber == item.CustomerPoNumber && u.ClientCode == entity.ClientCode && u.SoId == null).First();
                    }

                    ItemMaster itemMaster = listItemMaster.Where(u => u.WhCode == entity.WhCode && u.AltItemNumber == item.AltItemNumber && (u.Style1 == null ? "" : u.Style1) == (item.Style1 == null ? "" : item.Style1) && (u.Style2 == null ? "" : u.Style2) == (item.Style2 == null ? "" : item.Style2) && (u.Style3 == null ? "" : u.Style3) == (item.Style3 == null ? "" : item.Style3) && u.ClientId == entity.ClientId).First();

                    //添加InBoundOrderDetail
                    int insertResult = InsertInBoundOrderDetail(entity, item, inBoundOrder, itemMaster);

                    if (insertResult == 1 || insertResult == 2)
                    {
                        continue;
                    }
                    else
                    {
                        result = "导入出错!" + entity.ClientCode + "-" + item.CustomerPoNumber + "-" + item.AltItemNumber;
                        break;
                    }

                }

            }
            if (result != "")
            {
                return result;
            }


            return "Y";
        }



        public string ImportsInBoundOrderExtend(List<InBoundOrderInsert> entityList)
        {
            string result = "", resultt = "";     //执行总结果

            if (entityList == null)
            {
                return "数据有误，请重新操作！";
            }

            string whCode = entityList.First().WhCode;

            List<WhClient> clientList = idal.IWhClientDAL.SelectBy(u => u.WhCode == whCode && u.Status == "Active");

            string[] clientCodeArr = (from a in entityList select a.ClientCode).Distinct().ToArray();
            string[] soArr = (from a in entityList select a.SoNumber).ToList().Distinct().ToArray();

            if (clientList.Where(u => clientCodeArr.Contains(u.ClientCode)).Count() == 0)
            {
                return "客户名不存在，请检查客户名或大小写！";
            }

            foreach (var item in entityList)
            {
                List<string> skuArr = (from a in item.InBoundOrderDetailInsert where a.CheckAltItemNumberFlag == "Y" select a.AltItemNumber).ToList().Distinct().ToList();

                var sql = idal.IItemMasterDAL.SelectBy(u => u.WhCode == whCode && u.ClientCode == item.ClientCode && skuArr.Contains(u.AltItemNumber));
                if (sql.Count() != skuArr.Count)
                {
                    result += "SO:" + item.SoNumber + "需验证款号数量:" + skuArr.Count + "与系统已有款号数量:" + sql.Count() + "不一致，无法导入！";
                }
            }

            if (result != "")
            {
                return result;
            }

            foreach (var item in entityList)
            {
                List<string> poArr = (from a in item.InBoundOrderDetailInsert select a.CustomerPoNumber).ToList().Distinct().ToList();

                List<string> skuArr = (from a in item.InBoundOrderDetailInsert select a.AltItemNumber).ToList().Distinct().ToList();

                var sql = from a in idal.IInBoundSODAL.SelectAll()
                          join b in idal.IInBoundOrderDAL.SelectAll()
                          on a.Id equals b.SoId
                          join c in idal.IInBoundOrderDetailDAL.SelectAll()
                          on b.Id equals c.PoId
                          join d in idal.IItemMasterDAL.SelectAll()
                          on c.ItemId equals d.Id
                          where a.WhCode == whCode && a.SoNumber == item.SoNumber && a.ClientCode == item.ClientCode && poArr.Contains(b.CustomerPoNumber) &&
                          skuArr.Contains(d.AltItemNumber)
                          select new { c.RegQty, c.Id };
                if (sql.Where(u => u.RegQty > 0).Count() > 0)
                {
                    string result1 = "";
                    foreach (var item1 in skuArr)
                    {
                        result1 += item1 + ",";
                    }
                    if (result1 != "")
                    {
                        result1 = result1.Substring(0, result1.Length - 1);
                    }
                    result += "SO:" + item.SoNumber + "SKU:" + result1;
                }

                WhClient getWhClientFirst = clientList.Where(u => u.ClientCode == item.ClientCode && u.WhCode == item.WhCode).First();

                List<WhClientExtend> WhClientExtendList = idal.IWhClientExtendDAL.SelectBy(u => u.ClientId == getWhClientFirst.Id);
                if (WhClientExtendList.Count > 0)
                {
                    WhClientExtend getWhClientExtendFirst = WhClientExtendList.First();

                    if (!string.IsNullOrEmpty(getWhClientExtendFirst.RegularExpression))
                    {
                        foreach (var item1 in item.InBoundOrderDetailInsert)
                        {
                            foreach (var item2 in item1.InBoundOrderDetailSNInsert)
                            {
                                if (!Regex.IsMatch(item2.SN, getWhClientExtendFirst.RegularExpression))
                                {
                                    resultt += item2.SN + ",";
                                }
                            }
                        }
                    }
                }
            }

            if (resultt != "")
            {
                resultt = resultt.Substring(0, resultt.Length - 1);
            }

            if (result != "")
            {
                return result + "已做收货登记，请先删除收货登记！";
            }
            if (resultt != "")
            {
                return resultt + "不符合客户扩展信息的匹配规则正则，请检查！";
            }

            foreach (var item in entityList)
            {
                List<string> poArr = (from a in item.InBoundOrderDetailInsert select a.CustomerPoNumber).ToList().Distinct().ToList();

                List<string> skuArr = (from a in item.InBoundOrderDetailInsert select a.AltItemNumber).ToList().Distinct().ToList();

                var sql = from a in idal.IInBoundSODAL.SelectAll()
                          join b in idal.IInBoundOrderDAL.SelectAll()
                          on a.Id equals b.SoId
                          join c in idal.IInBoundOrderDetailDAL.SelectAll()
                          on b.Id equals c.PoId
                          join d in idal.IItemMasterDAL.SelectAll()
                          on c.ItemId equals d.Id
                          where a.WhCode == whCode && a.SoNumber == item.SoNumber && a.ClientCode == item.ClientCode && poArr.Contains(b.CustomerPoNumber) &&
                          skuArr.Contains(d.AltItemNumber)
                          select new { orderId = c.Id, PoId = b.Id };
                if (sql.Count() > 0)
                {
                    foreach (var item1 in sql.ToList())
                    {
                        int poId = Convert.ToInt32(item1.PoId);
                        idal.IInBoundOrderDAL.DeleteByExtended(u => u.Id == poId);

                        int inboundorderId = Convert.ToInt32(item1.orderId);
                        idal.IInBoundOrderDetailDAL.DeleteByExtended(u => u.Id == inboundorderId);

                    }
                }

                idal.ISerialNumberInOutExtendDAL.DeleteByExtended(u => u.WhCode == whCode && u.ClientCode == item.ClientCode && u.SoNumber == item.SoNumber && skuArr.Contains(u.AltItemNumber));
            }



            InBoundOrderInsert fir = entityList.First();

            List<string> soList = new List<string>();
            List<string> poList = new List<string>();
            List<string> skuList = new List<string>();

            //批量导入预录入SO
            List<InBoundSO> InBoundSOAddList = new List<InBoundSO>();
            foreach (var entity in entityList)
            {
                if (!string.IsNullOrEmpty(entity.SoNumber))
                {
                    WhClient client = clientList.Where(u => u.ClientCode == entity.ClientCode && u.WhCode == entity.WhCode).First();

                    entity.ClientId = client.Id;

                    if (entity.ProcessId == null || entity.ProcessId == 0)
                    {
                        List<R_Client_FlowRule> a1 = idal.IR_Client_FlowRuleDAL.SelectBy(u => u.ClientId == client.Id && u.Type == "InBound");
                        if (a1.Count > 0)
                        {
                            int FlowHead = (int)a1.First().BusinessFlowGroupId;
                            List<FlowHead> a2 = idal.IFlowHeadDAL.SelectBy(u => u.Id == FlowHead);
                            if (a2.Count > 0)
                            {
                                entity.ProcessId = a2.First().Id;
                                entity.ProcessName = a2.First().FlowName;
                            }
                            else
                            {
                                result = "未找到客户收货流程！" + entity.ClientCode;
                                break;
                            }
                        }
                        else
                        {
                            result = "未找到客户收货流程！" + entity.ClientCode;
                            break;
                        }
                    }

                    var ChecklistInBoundSO = idal.IInBoundSODAL.SelectBy(u => u.WhCode == entity.WhCode && u.ClientCode == entity.ClientCode && u.SoNumber == entity.SoNumber);

                    if (ChecklistInBoundSO.Count() == 0)
                    {
                        InBoundSO inBoundSO = new InBoundSO();
                        inBoundSO.WhCode = entity.WhCode;
                        inBoundSO.SoNumber = entity.SoNumber;
                        inBoundSO.ClientCode = entity.ClientCode;
                        inBoundSO.ClientId = (int)entity.ClientId;              //添加新数据 必须赋予客户ID
                        inBoundSO.CreateUser = fir.CreateUser;
                        inBoundSO.CreateDate = DateTime.Now;
                        InBoundSOAddList.Add(inBoundSO);
                    }
                    soList.Add(entity.SoNumber);
                }
            }

            if (InBoundSOAddList.Count > 0)
            {
                idal.IInBoundSODAL.Add(InBoundSOAddList);
                idal.IInBoundSODAL.SaveChanges();
            }

            List<InBoundSO> listInBoundSO = idal.IInBoundSODAL.SelectBy(u => u.WhCode == fir.WhCode && soList.Contains(u.SoNumber));

            List<InBoundOrder> InBoundOrderAddList = new List<InBoundOrder>();
            List<ItemMaster> ItemMasterAddList = new List<ItemMaster>();

            List<InBoundOrder> checkInBoundOrderAddResult = new List<InBoundOrder>();
            List<ItemMaster> checkItemMasterAddResult = new List<ItemMaster>();

            //批量导入预录入PO
            foreach (var entity in entityList)
            {
                WhClient client = clientList.Where(u => u.ClientCode == entity.ClientCode && u.WhCode == entity.WhCode).First();

                entity.ClientId = client.Id;

                if (entity.ProcessId == null || entity.ProcessId == 0)
                {
                    List<R_Client_FlowRule> a1 = idal.IR_Client_FlowRuleDAL.SelectBy(u => u.ClientId == client.Id && u.Type == "InBound");
                    if (a1.Count > 0)
                    {
                        int FlowHead = (int)a1.First().BusinessFlowGroupId;
                        List<FlowHead> a2 = idal.IFlowHeadDAL.SelectBy(u => u.Id == FlowHead);
                        if (a2.Count > 0)
                        {
                            entity.ProcessId = a2.First().Id;
                            entity.ProcessName = a2.First().FlowName;
                        }
                        else
                        {
                            result = "未找到客户收货流程！" + entity.ClientCode;
                            break;
                        }
                    }
                    else
                    {
                        result = "未找到客户收货流程！" + entity.ClientCode;
                        break;
                    }
                }

                if (!string.IsNullOrEmpty(entity.SoNumber))
                {
                    InBoundSO inBoundSO = listInBoundSO.Where(u => u.SoNumber == entity.SoNumber && u.ClientCode == entity.ClientCode).First();

                    string[] getPoArr = (from a in entity.InBoundOrderDetailInsert select a.CustomerPoNumber).ToList().Distinct().ToArray();

                    List<InBoundOrder> checkPoArr = idal.IInBoundOrderDAL.SelectBy(u => getPoArr.Contains(u.CustomerPoNumber) && u.WhCode == entity.WhCode && u.ClientCode == entity.ClientCode && u.SoId == inBoundSO.Id);

                    string[] getSkuArr = (from a in entity.InBoundOrderDetailInsert select a.AltItemNumber).ToList().Distinct().ToArray();

                    List<ItemMaster> checkSkuArr = idal.IItemMasterDAL.SelectBy(u => u.WhCode == entity.WhCode && getSkuArr.Contains(u.AltItemNumber) && u.ClientId == entity.ClientId).OrderBy(u => u.Id).ToList();

                    foreach (var item in entity.InBoundOrderDetailInsert)
                    {
                        if (checkInBoundOrderAddResult.Where(u => u.SoId == inBoundSO.Id && u.CustomerPoNumber == item.CustomerPoNumber).Count() == 0)
                        {
                            if (checkPoArr.Where(u => u.CustomerPoNumber == item.CustomerPoNumber && u.WhCode == entity.WhCode && u.ClientCode == entity.ClientCode && u.SoId == inBoundSO.Id).Count() == 0)
                            {
                                InBoundOrder inBoundOrder = new InBoundOrder();
                                inBoundOrder.WhCode = entity.WhCode;
                                inBoundOrder.SoId = inBoundSO.Id;
                                inBoundOrder.PoNumber = "ST" + DI.IDGenerator.NewId;
                                inBoundOrder.CustomerPoNumber = item.CustomerPoNumber;
                                inBoundOrder.AltCustomerPoNumber = item.AltCustomerPoNumber;
                                inBoundOrder.ClientId = (int)entity.ClientId;
                                inBoundOrder.ClientCode = entity.ClientCode;
                                inBoundOrder.OrderType = entity.OrderType;
                                inBoundOrder.ProcessId = entity.ProcessId;
                                inBoundOrder.ProcessName = entity.ProcessName;
                                inBoundOrder.OrderSource = "WMS";
                                inBoundOrder.CreateUser = item.CreateUser;
                                inBoundOrder.CreateDate = DateTime.Now;
                                InBoundOrderAddList.Add(inBoundOrder);
                            }
                            poList.Add(item.CustomerPoNumber);

                            InBoundOrder inboundResult = new InBoundOrder();
                            inboundResult.SoId = inBoundSO.Id;
                            inboundResult.CustomerPoNumber = item.CustomerPoNumber;
                            checkInBoundOrderAddResult.Add(inboundResult);
                        }

                        if (checkItemMasterAddResult.Where(u => u.AltItemNumber == item.AltItemNumber && u.ClientId == entity.ClientId).Count() == 0)
                        {
                            if (checkSkuArr.Where(u => u.WhCode == entity.WhCode && u.AltItemNumber == item.AltItemNumber && u.ClientId == entity.ClientId).Count() == 0)
                            {
                                ItemMaster itemMaster = new ItemMaster();
                                itemMaster.ItemNumber = "SYS" + DI.IDGenerator.NewId;
                                itemMaster.WhCode = entity.WhCode;
                                itemMaster.AltItemNumber = item.AltItemNumber;
                                itemMaster.ClientId = (int)entity.ClientId;
                                itemMaster.ClientCode = entity.ClientCode;
                                itemMaster.Style1 = item.Style1 ?? "";
                                itemMaster.Style2 = item.Style2 ?? "";
                                itemMaster.Style3 = item.Style3 ?? "";
                                itemMaster.UnitFlag = 0;
                                if (item.UnitName == "" || item.UnitName == null)
                                {
                                    itemMaster.UnitName = "none";
                                }
                                else
                                {
                                    itemMaster.UnitName = item.UnitName;
                                }

                                itemMaster.CreateUser = item.CreateUser;
                                itemMaster.CreateDate = DateTime.Now;
                                ItemMasterAddList.Add(itemMaster);   //款号不存在就新增
                            }
                            skuList.Add(item.AltItemNumber);

                            ItemMaster itemResult = new ItemMaster();
                            itemResult.ClientId = (int)entity.ClientId;
                            itemResult.AltItemNumber = item.AltItemNumber;
                            itemResult.Style1 = item.Style1 ?? "";
                            itemResult.Style2 = item.Style2 ?? "";
                            itemResult.Style3 = item.Style3 ?? "";
                            checkItemMasterAddResult.Add(itemResult);
                        }
                    }
                }
                else
                {

                    string[] getPoArr = (from a in entity.InBoundOrderDetailInsert select a.CustomerPoNumber).ToList().Distinct().ToArray();

                    List<InBoundOrder> checkPoArr = idal.IInBoundOrderDAL.SelectBy(u => getPoArr.Contains(u.CustomerPoNumber) && u.WhCode == entity.WhCode && u.ClientCode == entity.ClientCode && u.SoId == null);

                    string[] getSkuArr = (from a in entity.InBoundOrderDetailInsert select a.AltItemNumber).ToList().Distinct().ToArray();

                    List<ItemMaster> checkSkuArr = idal.IItemMasterDAL.SelectBy(u => u.WhCode == entity.WhCode && getSkuArr.Contains(u.AltItemNumber) && u.ClientId == entity.ClientId).OrderBy(u => u.Id).ToList();

                    foreach (var item in entity.InBoundOrderDetailInsert)
                    {
                        if (checkInBoundOrderAddResult.Where(u => u.SoId == null && u.CustomerPoNumber == item.CustomerPoNumber).Count() == 0)
                        {
                            if (checkPoArr.Where(u => u.CustomerPoNumber == item.CustomerPoNumber && u.WhCode == entity.WhCode && u.ClientCode == entity.ClientCode && u.SoId == null).Count() == 0)
                            {
                                InBoundOrder inBoundOrder = new InBoundOrder();
                                inBoundOrder.WhCode = entity.WhCode;
                                inBoundOrder.PoNumber = "ST" + DI.IDGenerator.NewId;
                                inBoundOrder.CustomerPoNumber = item.CustomerPoNumber;
                                inBoundOrder.AltCustomerPoNumber = item.AltCustomerPoNumber;
                                inBoundOrder.ClientId = (int)entity.ClientId;
                                inBoundOrder.ClientCode = entity.ClientCode;
                                inBoundOrder.OrderType = entity.OrderType;
                                inBoundOrder.ProcessId = entity.ProcessId;
                                inBoundOrder.ProcessName = entity.ProcessName;
                                inBoundOrder.OrderSource = "WMS";
                                inBoundOrder.CreateUser = item.CreateUser;
                                inBoundOrder.CreateDate = DateTime.Now;
                                InBoundOrderAddList.Add(inBoundOrder);
                            }

                            poList.Add(item.CustomerPoNumber);

                            InBoundOrder inboundResult = new InBoundOrder();
                            inboundResult.SoId = null;
                            inboundResult.CustomerPoNumber = item.CustomerPoNumber;
                            checkInBoundOrderAddResult.Add(inboundResult);
                        }


                        if (checkItemMasterAddResult.Where(u => u.AltItemNumber == item.AltItemNumber && u.ClientId == entity.ClientId).Count() == 0)
                        {
                            if (checkSkuArr.Where(u => u.WhCode == entity.WhCode && u.AltItemNumber == item.AltItemNumber && u.ClientId == entity.ClientId).Count() == 0)
                            {
                                ItemMaster itemMaster = new ItemMaster();
                                itemMaster.ItemNumber = "SYS" + DI.IDGenerator.NewId;
                                itemMaster.WhCode = entity.WhCode;
                                itemMaster.AltItemNumber = item.AltItemNumber;
                                itemMaster.ClientId = (int)entity.ClientId;
                                itemMaster.ClientCode = entity.ClientCode;
                                itemMaster.Style1 = item.Style1 ?? "";
                                itemMaster.Style2 = item.Style2 ?? "";
                                itemMaster.Style3 = item.Style3 ?? "";
                                itemMaster.UnitFlag = 0;
                                if (item.UnitName == "" || item.UnitName == null)
                                {
                                    itemMaster.UnitName = "none";
                                }
                                else
                                {
                                    itemMaster.UnitName = item.UnitName;
                                }

                                itemMaster.CreateUser = item.CreateUser;
                                itemMaster.CreateDate = DateTime.Now;
                                ItemMasterAddList.Add(itemMaster);   //款号不存在就新增
                            }
                            skuList.Add(item.AltItemNumber);

                            ItemMaster itemResult = new ItemMaster();
                            itemResult.ClientId = (int)entity.ClientId;
                            itemResult.AltItemNumber = item.AltItemNumber;
                            itemResult.Style1 = item.Style1 ?? "";
                            itemResult.Style2 = item.Style2 ?? "";
                            itemResult.Style3 = item.Style3 ?? "";
                            checkItemMasterAddResult.Add(itemResult);
                        }
                    }
                }
            }

            if (InBoundOrderAddList.Count > 0)
            {
                idal.IInBoundOrderDAL.Add(InBoundOrderAddList);
                idal.IInBoundSODAL.SaveChanges();
            }

            if (ItemMasterAddList.Count > 0)
            {
                idal.IItemMasterDAL.Add(ItemMasterAddList);
                idal.IItemMasterDAL.SaveChanges();
            }

            List<InBoundOrder> listInBoundOrder = idal.IInBoundOrderDAL.SelectBy(u => poList.Contains(u.CustomerPoNumber) && u.WhCode == fir.WhCode);

            List<ItemMaster> listItemMaster = idal.IItemMasterDAL.SelectBy(u => skuList.Contains(u.AltItemNumber) && u.WhCode == fir.WhCode).OrderBy(u => u.Id).ToList();

            List<SerialNumberInOutExtend> serExtendAddList = new List<SerialNumberInOutExtend>();
            //批量导入预录入
            foreach (var entity in entityList)
            {
                if (result != "")
                {
                    break;
                }
                WhClient client = clientList.Where(u => u.ClientCode == entity.ClientCode && u.WhCode == entity.WhCode).First();

                entity.ClientId = client.Id;

                if (entity.ProcessId == null || entity.ProcessId == 0)
                {
                    List<R_Client_FlowRule> a1 = idal.IR_Client_FlowRuleDAL.SelectBy(u => u.ClientId == client.Id && u.Type == "InBound");
                    if (a1.Count > 0)
                    {
                        int FlowHead = (int)a1.First().BusinessFlowGroupId;
                        List<FlowHead> a2 = idal.IFlowHeadDAL.SelectBy(u => u.Id == FlowHead);
                        if (a2.Count > 0)
                        {
                            entity.ProcessId = a2.First().Id;
                            entity.ProcessName = a2.First().FlowName;
                        }
                        else
                        {
                            result = "未找到客户收货流程！" + entity.ClientCode;
                            break;
                        }
                    }
                    else
                    {
                        result = "未找到客户收货流程！" + entity.ClientCode;
                        break;
                    }
                }

                foreach (var item in entity.InBoundOrderDetailInsert)
                {
                    InBoundOrder inBoundOrder = new InBoundOrder();

                    if (!string.IsNullOrEmpty(entity.SoNumber))
                    {
                        InBoundSO inBoundSO = listInBoundSO.Where(u => u.SoNumber == entity.SoNumber && u.ClientCode == entity.ClientCode).First();

                        inBoundOrder = listInBoundOrder.Where(u => u.CustomerPoNumber == item.CustomerPoNumber && u.ClientCode == entity.ClientCode && u.SoId == inBoundSO.Id).First();
                    }
                    else
                    {
                        inBoundOrder = listInBoundOrder.Where(u => u.CustomerPoNumber == item.CustomerPoNumber && u.ClientCode == entity.ClientCode && u.SoId == null).First();
                    }

                    ItemMaster itemMaster = listItemMaster.Where(u => u.WhCode == entity.WhCode && u.AltItemNumber == item.AltItemNumber && u.ClientId == entity.ClientId).First();

                    //添加InBoundOrderDetail
                    int insertResult = InsertInBoundOrderDetail(entity, item, inBoundOrder, itemMaster);

                    if (insertResult == 1 || insertResult == 2)
                    {
                        foreach (var item3 in item.InBoundOrderDetailSNInsert)
                        {
                            if (!string.IsNullOrEmpty(item3.SN))
                            {
                                SerialNumberInOutExtend ser = new SerialNumberInOutExtend();
                                ser.WhCode = entity.WhCode;
                                ser.ClientCode = entity.ClientCode;
                                ser.SoNumber = entity.SoNumber;
                                ser.CustomerPoNumber = item.CustomerPoNumber;
                                ser.AltItemNumber = item.AltItemNumber;
                                ser.Style1 = itemMaster.Style1;
                                ser.CartonId = item3.SN.ToUpper();
                                ser.CreateUser = item.CreateUser;
                                ser.CreateDate = DateTime.Now;
                                serExtendAddList.Add(ser);
                            }
                        }

                        continue;
                    }
                    else
                    {
                        result = "导入出错!" + entity.ClientCode + "-" + entity.SoNumber + "-" + item.AltItemNumber;
                        break;
                    }

                }

            }
            if (result != "")
            {
                return result;
            }

            if (serExtendAddList.Count > 0)
            {
                idal.ISerialNumberInOutExtendDAL.Add(serExtendAddList);
                idal.ISerialNumberInOutExtendDAL.SaveChanges();
            }

            return "Y";
        }


        #endregion

        #region 8.修改收货的客户名、SO、PO、款号、单位

        //目前所有修改异常收货信息的， 均不设计 多种单位修改，验证也未增加

        //yujia 2018-08-19
        //增加 对多种情况的异常收货信息修改


        //查询列表，显示SO
        public List<EditInBoundResult> EditInBoundClientCodeList(EditInBoundSearch searchEntity, out int total)
        {
            //            string s = "select a.ClientCode,a.ReceiptId,e.SoNumber,a.SumQty,sum(g.Qty) Qty,a.RegisterDate,a.BeginReceiptDate from ReceiptRegister a
            //left join ReceiptRegisterDetail b on a.WhCode = b.WhCode and a.ReceiptId = b.ReceiptId
            //left join InBoundOrderDetail c on b.InOrderDetailId = c.Id
            //left join InBoundOrder d on c.PoId = d.Id
            //left join InBoundSO e on d.SoId = e.Id
            //left join ItemMaster f on c.ItemId = f.Id
            //left join Receipt g on b.ReceiptId = g.ReceiptId and b.WhCode = g.WhCode and g.PoId = b.PoId and g.ItemId = b.ItemId and g.UnitName = b.UnitName
            //where a.ReceiptId = 'EI180522144418104'
            //group by a.ClientCode,a.ReceiptId,e.SoNumber, a.SumQty,a.RegisterDate,a.BeginReceiptDate";

            var sql = (from a in idal.IReceiptRegisterDAL.SelectAll()
                       join b in idal.IReceiptRegisterDetailDAL.SelectAll()
                             on new { a.WhCode, a.ReceiptId }
                         equals new { b.WhCode, b.ReceiptId }
                       join c in idal.IInBoundOrderDetailDAL.SelectAll() on new { InOrderDetailId = b.InOrderDetailId } equals new { InOrderDetailId = c.Id } into c_join
                       from c in c_join.DefaultIfEmpty()
                       join d in idal.IInBoundOrderDAL.SelectAll() on new { PoId = c.PoId } equals new { PoId = d.Id } into d_join
                       from d in d_join.DefaultIfEmpty()
                       join e in idal.IInBoundSODAL.SelectAll() on new { SoId = (Int32)d.SoId } equals new { SoId = e.Id } into e_join
                       from e in e_join.DefaultIfEmpty()
                       join g in idal.IReceiptDAL.SelectAll()
                             on new { b.ReceiptId, b.WhCode, b.PoId, b.ItemId, b.UnitName }
                         equals new { g.ReceiptId, g.WhCode, g.PoId, g.ItemId, g.UnitName } into g_join
                       from g in g_join.DefaultIfEmpty()
                       where a.WhCode == searchEntity.WhCode
                       group new { a, e, g } by new
                       {
                           a.ClientCode,
                           a.ReceiptId,
                           e.SoNumber,
                           a.SumQty,
                           a.RegisterDate,
                           a.BeginReceiptDate,
                           a.Status
                       } into g
                       select new EditInBoundResult
                       {
                           Action = "",
                           ReceiptId = g.Key.ReceiptId,
                           ClientCode = g.Key.ClientCode,
                           SoNumber = g.Key.SoNumber,
                           RegisterDate = g.Key.RegisterDate,
                           ReceiptDate = g.Key.BeginReceiptDate,
                           Status =
                           g.Key.Status == "N" ? "未释放" :
                           g.Key.Status == "U" ? "未收货" :
                           g.Key.Status == "A" ? "正在收货" :
                           g.Key.Status == "P" ? "暂停收货" :
                           g.Key.Status == "C" ? "完成收货" : null,
                           RegQty = (Int32?)g.Sum(p => p.g.Qty),
                           RecQty = (Int32?)g.Sum(p => p.g.Qty)
                       });

            if (!string.IsNullOrEmpty(searchEntity.ReceiptId))
                sql = sql.Where(u => u.ReceiptId == searchEntity.ReceiptId);
            if (!string.IsNullOrEmpty(searchEntity.SoNumber))
                sql = sql.Where(u => u.SoNumber == searchEntity.SoNumber);

            List<EditInBoundResult> list = sql.ToList();
            total = list.Count;
            list = list.OrderByDescending(u => u.ReceiptId).ThenBy(u => u.ClientCode).ThenBy(u => u.SoNumber).ToList();
            list = list.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize).ToList();
            return list;

        }

        //查询列表，显示SO\PO
        public List<EditInBoundResult> EditInBoundCustomerPoList(EditInBoundSearch searchEntity, out int total)
        {
            //            select a.ClientCode,a.ReceiptId,e.SoNumber,d.CustomerPoNumber,sum(g.Qty) SumQty,sum(g.Qty) Qty,a.RegisterDate,a.BeginReceiptDate
            //from ReceiptRegister a
            //left join ReceiptRegisterDetail b on a.WhCode = b.WhCode and a.ReceiptId = b.ReceiptId
            //left join InBoundOrderDetail c on b.InOrderDetailId = c.Id
            //left join InBoundOrder d on c.PoId = d.Id
            //left join InBoundSO e on d.SoId = e.Id
            //left join ItemMaster f on c.ItemId = f.Id
            //left join Receipt g on b.ReceiptId = g.ReceiptId and b.WhCode = g.WhCode and g.PoId = b.PoId and g.ItemId = b.ItemId and g.UnitName = b.UnitName
            //where a.ReceiptId = 'EI180717160204103'
            //group by a.ClientCode,a.ReceiptId,e.SoNumber, d.CustomerPoNumber,a.SumQty,a.RegisterDate,a.BeginReceiptDate

            var sql = (from a in idal.IReceiptRegisterDAL.SelectAll()
                       join b in idal.IReceiptRegisterDetailDAL.SelectAll()
                             on new { a.WhCode, a.ReceiptId }
                         equals new { b.WhCode, b.ReceiptId }
                       join c in idal.IInBoundOrderDetailDAL.SelectAll() on new { InOrderDetailId = b.InOrderDetailId } equals new { InOrderDetailId = c.Id } into c_join
                       from c in c_join.DefaultIfEmpty()
                       join d in idal.IInBoundOrderDAL.SelectAll() on new { PoId = c.PoId } equals new { PoId = d.Id } into d_join
                       from d in d_join.DefaultIfEmpty()
                       join e in idal.IInBoundSODAL.SelectAll() on new { SoId = (Int32)d.SoId } equals new { SoId = e.Id } into e_join
                       from e in e_join.DefaultIfEmpty()
                       join g in idal.IReceiptDAL.SelectAll()
                             on new { b.ReceiptId, b.WhCode, b.PoId, b.ItemId, b.UnitName }
                         equals new { g.ReceiptId, g.WhCode, g.PoId, g.ItemId, g.UnitName } into g_join
                       from g in g_join.DefaultIfEmpty()
                       where a.WhCode == searchEntity.WhCode
                       group new { a, e, g } by new
                       {
                           a.ClientCode,
                           a.ReceiptId,
                           e.SoNumber,
                           a.SumQty,
                           a.RegisterDate,
                           a.BeginReceiptDate,
                           a.Status,
                           d.CustomerPoNumber
                       } into g
                       select new EditInBoundResult
                       {
                           Action = "",
                           ReceiptId = g.Key.ReceiptId,
                           ClientCode = g.Key.ClientCode,
                           SoNumber = g.Key.SoNumber,
                           CustomerPoNumber = g.Key.CustomerPoNumber,
                           RegisterDate = g.Key.RegisterDate,
                           ReceiptDate = g.Key.BeginReceiptDate,
                           Status =
                           g.Key.Status == "N" ? "未释放" :
                           g.Key.Status == "U" ? "未收货" :
                           g.Key.Status == "A" ? "正在收货" :
                           g.Key.Status == "P" ? "暂停收货" :
                           g.Key.Status == "C" ? "完成收货" : null,
                           RegQty = (Int32?)g.Sum(p => p.g.Qty),
                           RecQty = (Int32?)g.Sum(p => p.g.Qty)
                       });

            if (!string.IsNullOrEmpty(searchEntity.ReceiptId))
                sql = sql.Where(u => u.ReceiptId == searchEntity.ReceiptId);
            if (!string.IsNullOrEmpty(searchEntity.SoNumber))
                sql = sql.Where(u => u.SoNumber == searchEntity.SoNumber);
            if (!string.IsNullOrEmpty(searchEntity.CustomerPoNumber))
                sql = sql.Where(u => u.CustomerPoNumber == searchEntity.CustomerPoNumber);

            List<EditInBoundResult> list = sql.ToList();
            total = list.Count;
            list = list.OrderByDescending(u => u.ReceiptId).ThenBy(u => u.ClientCode).ThenBy(u => u.SoNumber).ToList();
            list = list.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize).ToList();
            return list;

        }

        //查询列表，显示SO\PO\款号
        public List<EditInBoundResult> EditInBoundAltItemNumberList(EditInBoundSearch searchEntity, out int total)
        {
            //            select a.ClientCode,a.ReceiptId,e.SoNumber,d.CustomerPoNumber,f.AltItemNumber,sum(g.Qty) SumQty,sum(g.Qty) Qty,a.RegisterDate,a.BeginReceiptDate
            // from ReceiptRegister a
            //left join ReceiptRegisterDetail b on a.WhCode = b.WhCode and a.ReceiptId = b.ReceiptId
            //left join InBoundOrderDetail c on b.InOrderDetailId = c.Id
            //left join InBoundOrder d on c.PoId = d.Id
            //left join InBoundSO e on d.SoId = e.Id
            //left join ItemMaster f on c.ItemId = f.Id
            //left join Receipt g on b.ReceiptId = g.ReceiptId and b.WhCode = g.WhCode and g.PoId = b.PoId and g.ItemId = b.ItemId and g.UnitName = b.UnitName
            //where a.ReceiptId = 'EI180717160204103'
            //group by a.ClientCode,a.ReceiptId,e.SoNumber, d.CustomerPoNumber,f.AltItemNumber,a.SumQty,a.RegisterDate,a.BeginReceiptDate


            var sql = (from a in idal.IReceiptRegisterDAL.SelectAll()
                       join b in idal.IReceiptRegisterDetailDAL.SelectAll()
                             on new { a.WhCode, a.ReceiptId }
                         equals new { b.WhCode, b.ReceiptId }
                       join c in idal.IInBoundOrderDetailDAL.SelectAll() on new { InOrderDetailId = b.InOrderDetailId } equals new { InOrderDetailId = c.Id } into c_join
                       from c in c_join.DefaultIfEmpty()
                       join d in idal.IInBoundOrderDAL.SelectAll() on new { PoId = c.PoId } equals new { PoId = d.Id } into d_join
                       from d in d_join.DefaultIfEmpty()
                       join e in idal.IInBoundSODAL.SelectAll() on new { SoId = (Int32)d.SoId } equals new { SoId = e.Id } into e_join
                       from e in e_join.DefaultIfEmpty()
                       join f in idal.IItemMasterDAL.SelectAll() on new { ItemId = c.ItemId } equals new { ItemId = f.Id } into f_join
                       from f in f_join.DefaultIfEmpty()
                       join g in idal.IReceiptDAL.SelectAll()
                             on new { b.ReceiptId, b.WhCode, b.PoId, b.ItemId, b.UnitName }
                         equals new { g.ReceiptId, g.WhCode, g.PoId, g.ItemId, g.UnitName } into g_join
                       from g in g_join.DefaultIfEmpty()
                       where a.WhCode == searchEntity.WhCode
                       group new { a, e, g } by new
                       {
                           a.ClientCode,
                           a.ReceiptId,
                           e.SoNumber,
                           a.SumQty,
                           a.RegisterDate,
                           a.BeginReceiptDate,
                           a.Status,
                           d.CustomerPoNumber,
                           f.AltItemNumber

                       } into g
                       select new EditInBoundResult
                       {
                           Action = "",
                           ReceiptId = g.Key.ReceiptId,
                           ClientCode = g.Key.ClientCode,
                           SoNumber = g.Key.SoNumber,
                           CustomerPoNumber = g.Key.CustomerPoNumber,
                           AltItemNumber = g.Key.AltItemNumber,
                           RegisterDate = g.Key.RegisterDate,
                           ReceiptDate = g.Key.BeginReceiptDate,
                           Status =
                           g.Key.Status == "N" ? "未释放" :
                           g.Key.Status == "U" ? "未收货" :
                           g.Key.Status == "A" ? "正在收货" :
                           g.Key.Status == "P" ? "暂停收货" :
                           g.Key.Status == "C" ? "完成收货" : null,
                           RegQty = (Int32?)g.Sum(p => p.g.Qty),
                           RecQty = (Int32?)g.Sum(p => p.g.Qty)
                       });

            if (!string.IsNullOrEmpty(searchEntity.ReceiptId))
                sql = sql.Where(u => u.ReceiptId == searchEntity.ReceiptId);
            if (!string.IsNullOrEmpty(searchEntity.SoNumber))
                sql = sql.Where(u => u.SoNumber == searchEntity.SoNumber);
            if (!string.IsNullOrEmpty(searchEntity.CustomerPoNumber))
                sql = sql.Where(u => u.CustomerPoNumber == searchEntity.CustomerPoNumber);
            if (!string.IsNullOrEmpty(searchEntity.AltItemNumber))
                sql = sql.Where(u => u.AltItemNumber == searchEntity.AltItemNumber);

            List<EditInBoundResult> list = sql.ToList();
            total = list.Count;
            list = list.OrderByDescending(u => u.ReceiptId).ThenBy(u => u.ClientCode).ThenBy(u => u.SoNumber).ToList();
            list = list.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize).ToList();
            return list;

        }


        //查询列表，显示收货明细及单位
        public List<ReceiptResult> EditUnitNameList(EditInBoundSearch searchEntity, out int total)
        {
            var sql = from a in idal.IReceiptDAL.SelectAll()
                      join c in idal.IUnitDefaultDAL.SelectAll()
                      on new { A = a.WhCode, B = a.UnitName } equals new { A = c.WhCode, B = c.UnitName } into temp2
                      from c in temp2.DefaultIfEmpty()

                      where a.WhCode == searchEntity.WhCode && a.ReceiptId == searchEntity.ReceiptId
                      group a by new
                      {
                          a.ReceiptId,
                          a.ClientCode,
                          a.SoNumber,
                          a.CustomerPoNumber,
                          a.AltItemNumber,
                          a.UnitName,
                          c.UnitNameCN
                      }
                      into g
                      select new ReceiptResult
                      {
                          WhCode = "",
                          ReceiptId = g.Key.ReceiptId,
                          ClientCode = g.Key.ClientCode,
                          SoNumber = g.Key.SoNumber,
                          CustomerPoNumber = g.Key.CustomerPoNumber,
                          AltItemNumber = g.Key.AltItemNumber,
                          Qty = g.Sum(p => p.Qty),
                          UnitName = g.Key.UnitName,
                          UnitNameShow = g.Key.UnitNameCN
                      };

            List<ReceiptResult> list = sql.ToList();

            total = list.Count;
            list = list.OrderByDescending(u => u.ReceiptId).ThenBy(u => u.ClientCode).ThenBy(u => u.SoNumber).ToList();
            list = list.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize).ToList();
            return list;
        }


        //查询列表，显示收货PO含托盘
        public List<ReceiptResult> EditInBoundCustPoByHuList(EditInBoundSearch searchEntity, out int total)
        {
            var sql = from a in idal.IReceiptDAL.SelectAll()
                      join b in idal.IReceiptRegisterDAL.SelectAll()
                      on new { A = a.WhCode, B = a.ReceiptId } equals new { A = b.WhCode, B = b.ReceiptId } into temp1
                      from b in temp1.DefaultIfEmpty()

                      join c in idal.IUnitDefaultDAL.SelectAll()
                     on new { A = a.WhCode, B = a.UnitName } equals new { A = c.WhCode, B = c.UnitName } into temp2
                      from c in temp2.DefaultIfEmpty()

                      where a.WhCode == searchEntity.WhCode && a.ReceiptId == searchEntity.ReceiptId
                      group a by new
                      {
                          a.ReceiptId,
                          a.ClientCode,
                          a.SoNumber,
                          a.CustomerPoNumber,
                          a.UnitName,
                          a.HuId,
                          b.Status,
                          c.UnitNameCN
                      }
                      into g
                      select new ReceiptResult
                      {
                          ReceiptId = g.Key.ReceiptId,
                          Status =
                           g.Key.Status == "N" ? "未释放" :
                           g.Key.Status == "U" ? "未收货" :
                           g.Key.Status == "A" ? "正在收货" :
                           g.Key.Status == "P" ? "暂停收货" :
                           g.Key.Status == "C" ? "完成收货" : null,
                          ClientCode = g.Key.ClientCode,
                          SoNumber = g.Key.SoNumber,
                          CustomerPoNumber = g.Key.CustomerPoNumber,
                          HuId = g.Key.HuId,
                          Qty = g.Sum(p => p.Qty),
                          UnitName = g.Key.UnitName,
                          UnitNameShow = g.Key.UnitNameCN
                      };

            if (!string.IsNullOrEmpty(searchEntity.HuId))
                sql = sql.Where(u => u.HuId == searchEntity.HuId);

            List<ReceiptResult> list = sql.ToList();
            total = list.Count;
            list = list.OrderByDescending(u => u.ReceiptId).ThenBy(u => u.ClientCode).ThenBy(u => u.SoNumber).ToList();
            list = list.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize).ToList();
            return list;
        }


        //查询列表，显示收货POSKU含托盘
        public List<ReceiptResult> EditInBoundAltItemByHuList(EditInBoundSearch searchEntity, out int total)
        {
            var sql = from a in idal.IReceiptDAL.SelectAll()
                      join b in idal.IReceiptRegisterDAL.SelectAll()
                      on new { A = a.WhCode, B = a.ReceiptId } equals new { A = b.WhCode, B = b.ReceiptId } into temp1
                      from b in temp1.DefaultIfEmpty()

                      join c in idal.IUnitDefaultDAL.SelectAll()
                   on new { A = a.WhCode, B = a.UnitName } equals new { A = c.WhCode, B = c.UnitName }
                   into temp2
                      from c in temp2.DefaultIfEmpty()

                      where a.WhCode == searchEntity.WhCode && a.ReceiptId == searchEntity.ReceiptId
                      group a by new
                      {
                          a.ReceiptId,
                          a.ClientCode,
                          a.SoNumber,
                          a.CustomerPoNumber,
                          a.AltItemNumber,
                          a.UnitName,
                          a.HuId,
                          b.Status,
                          c.UnitNameCN
                      }
                      into g
                      select new ReceiptResult
                      {
                          ReceiptId = g.Key.ReceiptId,
                          Status =
                           g.Key.Status == "N" ? "未释放" :
                           g.Key.Status == "U" ? "未收货" :
                           g.Key.Status == "A" ? "正在收货" :
                           g.Key.Status == "P" ? "暂停收货" :
                           g.Key.Status == "C" ? "完成收货" : null,
                          ClientCode = g.Key.ClientCode,
                          SoNumber = g.Key.SoNumber,
                          CustomerPoNumber = g.Key.CustomerPoNumber,
                          AltItemNumber = g.Key.AltItemNumber,
                          HuId = g.Key.HuId,
                          Qty = g.Sum(p => p.Qty),
                          UnitName = g.Key.UnitName,
                          UnitNameShow = g.Key.UnitNameCN
                      };

            if (!string.IsNullOrEmpty(searchEntity.HuId))
                sql = sql.Where(u => u.HuId == searchEntity.HuId);

            List<ReceiptResult> list = sql.ToList();
            total = list.Count;
            list = list.OrderByDescending(u => u.ReceiptId).ThenBy(u => u.ClientCode).ThenBy(u => u.SoNumber).ToList();
            list = list.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize).ToList();
            return list;
        }


        //查询列表，显示收货POSKU属性含托盘
        public List<ReceiptResult> EditInBoundAltItemStyleByHuList(EditInBoundSearch searchEntity, out int total)
        {
            var sql = from a in idal.IReceiptDAL.SelectAll()
                      join b in idal.IReceiptRegisterDAL.SelectAll()
                      on new { A = a.WhCode, B = a.ReceiptId } equals new { A = b.WhCode, B = b.ReceiptId } into temp1
                      from b in temp1.DefaultIfEmpty()

                      join c in idal.IUnitDefaultDAL.SelectAll()
                      on new { A = a.WhCode, B = a.UnitName } equals new { A = c.WhCode, B = c.UnitName } into temp2
                      from c in temp2.DefaultIfEmpty()

                      join d in idal.IItemMasterDAL.SelectAll()
                      on a.ItemId equals d.Id into temp3
                      from d in temp3.DefaultIfEmpty()

                      where a.WhCode == searchEntity.WhCode && a.ReceiptId == searchEntity.ReceiptId
                      group a by new
                      {
                          a.ReceiptId,
                          a.ClientCode,
                          a.SoNumber,
                          a.CustomerPoNumber,
                          a.AltItemNumber,
                          a.UnitName,
                          a.HuId,
                          b.Status,
                          c.UnitNameCN,
                          d.Style1,
                          d.Style2,
                          d.Style3
                      }
                      into g
                      select new ReceiptResult
                      {
                          ReceiptId = g.Key.ReceiptId,
                          Status =
                           g.Key.Status == "N" ? "未释放" :
                           g.Key.Status == "U" ? "未收货" :
                           g.Key.Status == "A" ? "正在收货" :
                           g.Key.Status == "P" ? "暂停收货" :
                           g.Key.Status == "C" ? "完成收货" : null,
                          ClientCode = g.Key.ClientCode,
                          SoNumber = g.Key.SoNumber,
                          CustomerPoNumber = g.Key.CustomerPoNumber,
                          AltItemNumber = g.Key.AltItemNumber,
                          Style1 = g.Key.Style1 ?? "",
                          Style2 = g.Key.Style2 ?? "",
                          Style3 = g.Key.Style3 ?? "",
                          HuId = g.Key.HuId,
                          Qty = g.Sum(p => p.Qty),
                          UnitName = g.Key.UnitName,
                          UnitNameShow = g.Key.UnitNameCN
                      };

            if (!string.IsNullOrEmpty(searchEntity.HuId))
                sql = sql.Where(u => u.HuId == searchEntity.HuId);

            List<ReceiptResult> list = sql.ToList();
            total = list.Count;
            list = list.OrderByDescending(u => u.ReceiptId).ThenBy(u => u.ClientCode).ThenBy(u => u.SoNumber).ToList();
            list = list.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize).ToList();
            return list;
        }


        //修改收货客户名
        public string EditDetailClientCode(EditInBoundResult Editentity, string[] getsoList)
        {
            using (TransactionScope trans = new TransactionScope())
            {
                try
                {
                    #region 优化
                    List<ReceiptRegister> ReceiptRegisterList = idal.IReceiptRegisterDAL.SelectBy(u => u.WhCode == Editentity.WhCode && u.ReceiptId == Editentity.ReceiptId);
                    if (ReceiptRegisterList.Count == 0)
                    {
                        return "未找到明细，无法修改！";
                    }

                    List<HuDetail> checkPlanQtyList = idal.IHuDetailDAL.SelectBy(u => u.WhCode == Editentity.WhCode && u.ReceiptId == Editentity.ReceiptId && (u.PlanQty ?? 0) != 0 && getsoList.Contains(u.SoNumber));
                    if (checkPlanQtyList.Count > 0)
                    {
                        return "SO已做备货单无法修改，请撤销释放后修改！";
                    }

                    List<WhClient> WhClientList = idal.IWhClientDAL.SelectBy(u => u.WhCode == Editentity.WhCode && u.ClientCode == Editentity.ClientCode && u.Status == "Active" && u.Id == Editentity.ClientId);
                    if (WhClientList.Count == 0)
                    {
                        return "未找到客户，无法修改！";
                    }

                    ReceiptRegister receiptRegister = ReceiptRegisterList.First();
                    if (Editentity.ClientCode == receiptRegister.ClientCode)
                    {
                        return "客户名与原客户名一致，无需修改！";
                    }

                    //string s = "select a.ClientCode,a.ReceiptId,e.SoNumber,e.Id soid,d.Id poid,d.CustomerPoNumber,f.Id itmeid,f.AltItemNumber,f.Style1,f.Style2,f.Style3,b.RegQty,b.UnitName,b.UnitId,b.Id regDetailId from ReceiptRegister a
                    //left join ReceiptRegisterDetail b on a.WhCode = b.WhCode and a.ReceiptId = b.ReceiptId
                    //left join InBoundOrderDetail c on b.InOrderDetailId = c.Id
                    //left join InBoundOrder d on c.PoId = d.Id
                    //left join InBoundSO e on d.SoId = e.Id
                    //left join ItemMaster f on c.ItemId = f.Id
                    //where a.ReceiptId = 'EI180522144418104' and e.SoNumber = 'T052201'";


                    var sql = (from a in idal.IReceiptRegisterDAL.SelectAll()
                               join b in idal.IReceiptRegisterDetailDAL.SelectAll()
                                     on new { a.WhCode, a.ReceiptId }
                                 equals new { b.WhCode, b.ReceiptId } into b_join
                               from b in b_join.DefaultIfEmpty()
                               join c in idal.IInBoundOrderDetailDAL.SelectAll() on new { InOrderDetailId = b.InOrderDetailId } equals new { InOrderDetailId = c.Id } into c_join
                               from c in c_join.DefaultIfEmpty()
                               join d in idal.IInBoundOrderDAL.SelectAll() on new { PoId = c.PoId } equals new { PoId = d.Id } into d_join
                               from d in d_join.DefaultIfEmpty()
                               join e in idal.IInBoundSODAL.SelectAll() on new { SoId = (Int32)d.SoId } equals new { SoId = e.Id } into e_join
                               from e in e_join.DefaultIfEmpty()
                               join f in idal.IItemMasterDAL.SelectAll() on new { ItemId = c.ItemId } equals new { ItemId = f.Id } into f_join
                               from f in f_join.DefaultIfEmpty()
                               where
                                 a.ReceiptId == Editentity.ReceiptId && a.WhCode == Editentity.WhCode && getsoList.Contains(e.SoNumber)
                               select new EditInBoundDetailResult
                               {
                                   ClientCode = a.ClientCode,
                                   ReceiptId = a.ReceiptId,
                                   SoNumber = e.SoNumber,
                                   SoId = e.Id,
                                   PoId = d.Id,
                                   CustomerPoNumber = d.CustomerPoNumber,
                                   ItemId = f.Id,
                                   AltItemNumber = f.AltItemNumber,
                                   Style1 = f.Style1 ?? "",
                                   Style2 = f.Style2 ?? "",
                                   Style3 = f.Style3 ?? "",
                                   Qty = (Int32?)b.RegQty,
                                   UnitName = b.UnitName,
                                   UnitId = b.UnitId,
                                   RegDetailId = b.Id,
                                   OrderType = d.OrderType
                               }).Distinct();

                    List<EditInBoundDetailResult> getList = sql.ToList();

                    //1. 取得原预录入明细--后面修改收货及库存有用
                    int[] getsoid = (from a in getList
                                     select a.SoId).Distinct().ToArray();

                    int[] getpoid = (from a in getList
                                     select a.PoId).Distinct().ToArray();

                    int[] getitemid = (from a in getList
                                       select a.ItemId).Distinct().ToArray();

                    int[] regDetailid = (from a in getList
                                         select a.RegDetailId).Distinct().ToArray();

                    string[] customerPoNumber = (from a in getList
                                                 select a.CustomerPoNumber).Distinct().ToArray();

                    //2. 重建预录入基础信息
                    string getclient = "", getSo = "", getPo = "", getSku = "", getStyle1 = "", getStyle2 = "", getStyle3 = "", getQty = "", unitName = "";

                    List<InBoundOrderInsert> entityList = new List<InBoundOrderInsert>();
                    foreach (var item in getList)
                    {
                        getclient = Editentity.ClientCode;
                        getSo = item.SoNumber;
                        getPo = item.CustomerPoNumber;
                        getSku = item.AltItemNumber;
                        getStyle1 = item.Style1;
                        getStyle2 = item.Style2;
                        getStyle3 = item.Style3;
                        getQty = item.Qty.ToString();
                        unitName = item.UnitName;

                        if (entityList.Where(u => u.SoNumber == getSo.Trim() && u.ClientCode == getclient.Trim()).Count() == 0)
                        {
                            InBoundOrderInsert entity1 = new InBoundOrderInsert();
                            entity1.WhCode = Editentity.WhCode;
                            entity1.ClientCode = getclient.Trim();
                            entity1.ClientId = Editentity.ClientId;
                            entity1.SoNumber = getSo.Trim();
                            entity1.CreateUser = Editentity.UserCode;
                            entity1.OrderType = item.OrderType;
                            entity1.ProcessId = receiptRegister.ProcessId;
                            entity1.ProcessName = receiptRegister.ProcessName;

                            List<InBoundOrderDetailInsert> orderDetailList = new List<InBoundOrderDetailInsert>();

                            InBoundOrderDetailInsert orderDetail = new InBoundOrderDetailInsert();
                            orderDetail.CustomerPoNumber = getPo.Trim().ToString();
                            orderDetail.AltItemNumber = getSku.Trim().ToString();
                            orderDetail.Style1 = getStyle1.ToString();
                            orderDetail.Style2 = getStyle2.ToString();
                            orderDetail.Style3 = getStyle3.ToString();
                            orderDetail.UnitName = unitName;
                            orderDetail.Qty = Convert.ToInt32(getQty);
                            orderDetail.CreateUser = Editentity.UserCode;
                            orderDetailList.Add(orderDetail);

                            entity1.InBoundOrderDetailInsert = orderDetailList;
                            entityList.Add(entity1);
                        }
                        else
                        {
                            InBoundOrderInsert oldentity = entityList.Where(u => u.SoNumber == getSo.Trim() && u.ClientCode == getclient.Trim()).First();
                            entityList.Remove(oldentity);

                            InBoundOrderInsert newentity = oldentity;

                            List<InBoundOrderDetailInsert> orderDetailList = oldentity.InBoundOrderDetailInsert.ToList();
                            if (orderDetailList.Where(u => u.CustomerPoNumber == getPo.Trim() && u.AltItemNumber == getSku.Trim() && u.Style1 == getStyle1 && u.Style2 == getStyle2 && u.Style3 == getStyle3).Count() == 0)
                            {
                                InBoundOrderDetailInsert orderDetail = new InBoundOrderDetailInsert();
                                orderDetail.CustomerPoNumber = getPo.Trim().ToString();
                                orderDetail.AltItemNumber = getSku.Trim().ToString();
                                orderDetail.Style1 = getStyle1.Trim().ToString();
                                orderDetail.Style2 = getStyle2.ToString();
                                orderDetail.Style3 = getStyle3.ToString();
                                orderDetail.UnitName = unitName;
                                orderDetail.Qty = Convert.ToInt32(getQty);
                                orderDetail.CreateUser = Editentity.UserCode;
                                orderDetailList.Add(orderDetail);
                            }
                            else
                            {
                                InBoundOrderDetailInsert oldorderDetail = orderDetailList.Where(u => u.CustomerPoNumber == getPo.Trim() && u.AltItemNumber == getSku.Trim() && u.Style1 == getStyle1.Trim() && u.Style2 == getStyle2 && u.Style3 == getStyle3).First();

                                orderDetailList.Remove(oldorderDetail);
                                InBoundOrderDetailInsert neworderDetail = oldorderDetail;
                                neworderDetail.Qty = oldorderDetail.Qty + Convert.ToInt32(getQty);
                                orderDetailList.Add(neworderDetail);
                            }

                            newentity.InBoundOrderDetailInsert = orderDetailList;
                            entityList.Add(newentity);
                        }
                    }

                    //3.验证收货批次是否只有该SO
                    var sqlcheck = (from a in idal.IReceiptRegisterDAL.SelectAll()
                                    join b in idal.IReceiptRegisterDetailDAL.SelectAll()
                                          on new { a.WhCode, a.ReceiptId }
                                      equals new { b.WhCode, b.ReceiptId } into b_join
                                    from b in b_join.DefaultIfEmpty()
                                    join c in idal.IInBoundOrderDetailDAL.SelectAll() on new { InOrderDetailId = b.InOrderDetailId } equals new { InOrderDetailId = c.Id } into c_join
                                    from c in c_join.DefaultIfEmpty()
                                    join d in idal.IInBoundOrderDAL.SelectAll() on new { PoId = c.PoId } equals new { PoId = d.Id } into d_join
                                    from d in d_join.DefaultIfEmpty()
                                    join e in idal.IInBoundSODAL.SelectAll() on new { SoId = (Int32)d.SoId } equals new { SoId = e.Id } into e_join
                                    from e in e_join.DefaultIfEmpty()
                                    join f in idal.IItemMasterDAL.SelectAll() on new { ItemId = c.ItemId } equals new { ItemId = f.Id } into f_join
                                    from f in f_join.DefaultIfEmpty()
                                    where
                                      a.ReceiptId == Editentity.ReceiptId && a.WhCode == Editentity.WhCode && !getsoList.Contains(e.SoNumber)
                                    select new EditInBoundDetailResult
                                    {
                                        ClientCode = a.ClientCode,
                                        ReceiptId = a.ReceiptId,
                                        SoNumber = e.SoNumber,
                                        SoId = e.Id,
                                        PoId = d.Id,
                                        CustomerPoNumber = d.CustomerPoNumber,
                                        ItemId = f.Id,
                                        AltItemNumber = f.AltItemNumber,
                                        Style1 = f.Style1,
                                        Style2 = f.Style2,
                                        Style3 = f.Style3,
                                        Qty = (Int32?)b.RegQty,
                                        UnitName = b.UnitName,
                                        UnitId = b.UnitId,
                                        RegDetailId = b.Id,
                                        OrderType = d.OrderType
                                    }).Distinct();

                    //3.2 创建收货登记

                    string receipt = "EI" + DI.IDGenerator.NewId;

                    if (sqlcheck.Count() == 0)
                    {
                        receipt = Editentity.ReceiptId;
                    }

                    List<ReceiptRegisterInsert> listEntity = new List<ReceiptRegisterInsert>();     //收货登记明细列表

                    InBoundOrderInsert fir = entityList.First();

                    List<string> soList = new List<string>();
                    List<string> poList = new List<string>();
                    List<string> skuList = new List<string>();

                    //批量导入预录入SO
                    List<InBoundSO> InBoundSOAddList = new List<InBoundSO>();
                    foreach (var entity in entityList)
                    {
                        if (!string.IsNullOrEmpty(entity.SoNumber))
                        {
                            var ChecklistInBoundSO = idal.IInBoundSODAL.SelectBy(u => u.WhCode == entity.WhCode && u.ClientCode == entity.ClientCode && u.SoNumber == entity.SoNumber);

                            if (ChecklistInBoundSO.Count() == 0)
                            {
                                InBoundSO inBoundSO = new InBoundSO();
                                inBoundSO.WhCode = entity.WhCode;
                                inBoundSO.SoNumber = entity.SoNumber;
                                inBoundSO.ClientCode = entity.ClientCode;
                                inBoundSO.ClientId = (int)entity.ClientId;              //添加新数据 必须赋予客户ID
                                inBoundSO.CreateUser = fir.CreateUser;
                                inBoundSO.CreateDate = DateTime.Now;
                                InBoundSOAddList.Add(inBoundSO);
                            }
                            soList.Add(entity.SoNumber);
                        }
                    }

                    if (InBoundSOAddList.Count > 0)
                    {
                        idal.IInBoundSODAL.Add(InBoundSOAddList);
                        idal.IInBoundSODAL.SaveChanges();
                    }

                    List<InBoundSO> listInBoundSO = idal.IInBoundSODAL.SelectBy(u => u.WhCode == fir.WhCode && soList.Contains(u.SoNumber));

                    List<InBoundOrder> InBoundOrderAddList = new List<InBoundOrder>();
                    List<ItemMaster> ItemMasterAddList = new List<ItemMaster>();

                    List<InBoundOrder> checkInBoundOrderAddResult = new List<InBoundOrder>();
                    List<ItemMaster> checkItemMasterAddResult = new List<ItemMaster>();

                    //批量导入预录入PO
                    foreach (var entity in entityList)
                    {
                        if (!string.IsNullOrEmpty(entity.SoNumber))
                        {
                            InBoundSO inBoundSO = listInBoundSO.Where(u => u.SoNumber == entity.SoNumber && u.ClientCode == entity.ClientCode).First();

                            foreach (var item in entity.InBoundOrderDetailInsert)
                            {
                                if (checkInBoundOrderAddResult.Where(u => u.SoId == inBoundSO.Id && u.CustomerPoNumber == item.CustomerPoNumber).Count() == 0)
                                {
                                    var ChecklistInBoundOrder = idal.IInBoundOrderDAL.SelectBy(u => u.CustomerPoNumber == item.CustomerPoNumber && u.WhCode == entity.WhCode && u.ClientCode == entity.ClientCode && u.SoId == inBoundSO.Id);

                                    if (ChecklistInBoundOrder.Count() == 0)
                                    {
                                        InBoundOrder inBoundOrder = new InBoundOrder();
                                        inBoundOrder.WhCode = entity.WhCode;
                                        inBoundOrder.SoId = inBoundSO.Id;
                                        inBoundOrder.PoNumber = "ST" + DI.IDGenerator.NewId;
                                        inBoundOrder.CustomerPoNumber = item.CustomerPoNumber;
                                        inBoundOrder.AltCustomerPoNumber = item.AltCustomerPoNumber;
                                        inBoundOrder.ClientId = (int)entity.ClientId;
                                        inBoundOrder.ClientCode = entity.ClientCode;
                                        inBoundOrder.OrderType = entity.OrderType;
                                        inBoundOrder.ProcessId = entity.ProcessId;
                                        inBoundOrder.ProcessName = entity.ProcessName;
                                        inBoundOrder.OrderSource = "WMS";
                                        inBoundOrder.CreateUser = item.CreateUser;
                                        inBoundOrder.CreateDate = DateTime.Now;
                                        InBoundOrderAddList.Add(inBoundOrder);
                                    }
                                    poList.Add(item.CustomerPoNumber);

                                    InBoundOrder inboundResult = new InBoundOrder();
                                    inboundResult.SoId = inBoundSO.Id;
                                    inboundResult.CustomerPoNumber = item.CustomerPoNumber;
                                    checkInBoundOrderAddResult.Add(inboundResult);
                                }

                                if (checkItemMasterAddResult.Where(u => u.AltItemNumber == item.AltItemNumber && (u.Style1 == null ? "" : u.Style1) == (item.Style1 == null ? "" : item.Style1) && (u.Style2 == null ? "" : u.Style2) == (item.Style2 == null ? "" : item.Style2) && (u.Style3 == null ? "" : u.Style3) == (item.Style3 == null ? "" : item.Style3) && u.ClientId == entity.ClientId).Count() == 0)
                                {
                                    var checklistItemMaster = idal.IItemMasterDAL.SelectBy(u => u.WhCode == entity.WhCode && u.AltItemNumber == item.AltItemNumber && (u.Style1 == null ? "" : u.Style1) == (item.Style1 == null ? "" : item.Style1) && (u.Style2 == null ? "" : u.Style2) == (item.Style2 == null ? "" : item.Style2) && (u.Style3 == null ? "" : u.Style3) == (item.Style3 == null ? "" : item.Style3) && u.ClientId == entity.ClientId);

                                    if (checklistItemMaster.Count() == 0)
                                    {
                                        ItemMaster itemMaster = new ItemMaster();
                                        itemMaster.ItemNumber = "SYS" + DI.IDGenerator.NewId;
                                        itemMaster.WhCode = entity.WhCode;
                                        itemMaster.AltItemNumber = item.AltItemNumber;
                                        itemMaster.ClientId = (int)entity.ClientId;
                                        itemMaster.ClientCode = entity.ClientCode;
                                        itemMaster.Style1 = item.Style1 ?? "";
                                        itemMaster.Style2 = item.Style2 ?? "";
                                        itemMaster.Style3 = item.Style3 ?? "";
                                        itemMaster.UnitFlag = 0;
                                        if (item.UnitName == "" || item.UnitName == null)
                                        {
                                            itemMaster.UnitName = "none";
                                        }
                                        else
                                        {
                                            itemMaster.UnitName = item.UnitName;
                                        }

                                        itemMaster.CreateUser = item.CreateUser;
                                        itemMaster.CreateDate = DateTime.Now;
                                        ItemMasterAddList.Add(itemMaster);   //款号不存在就新增
                                    }
                                    skuList.Add(item.AltItemNumber);

                                    ItemMaster itemResult = new ItemMaster();
                                    itemResult.ClientId = (int)entity.ClientId;
                                    itemResult.AltItemNumber = item.AltItemNumber;
                                    itemResult.Style1 = item.Style1 ?? "";
                                    itemResult.Style2 = item.Style2 ?? "";
                                    itemResult.Style3 = item.Style3 ?? "";
                                    checkItemMasterAddResult.Add(itemResult);
                                }

                            }
                        }
                        else
                        {
                            foreach (var item in entity.InBoundOrderDetailInsert)
                            {
                                if (checkInBoundOrderAddResult.Where(u => u.SoId == null && u.CustomerPoNumber == item.CustomerPoNumber).Count() == 0)
                                {
                                    var ChecklistInBoundOrder = idal.IInBoundOrderDAL.SelectBy(u => u.CustomerPoNumber == item.CustomerPoNumber && u.WhCode == entity.WhCode && u.ClientCode == entity.ClientCode && u.SoId == null);

                                    if (ChecklistInBoundOrder.Count() == 0)
                                    {
                                        InBoundOrder inBoundOrder = new InBoundOrder();
                                        inBoundOrder.WhCode = entity.WhCode;
                                        inBoundOrder.PoNumber = "ST" + DI.IDGenerator.NewId;
                                        inBoundOrder.CustomerPoNumber = item.CustomerPoNumber;
                                        inBoundOrder.AltCustomerPoNumber = item.AltCustomerPoNumber;
                                        inBoundOrder.ClientId = (int)entity.ClientId;
                                        inBoundOrder.ClientCode = entity.ClientCode;
                                        inBoundOrder.OrderType = entity.OrderType;
                                        inBoundOrder.ProcessId = entity.ProcessId;
                                        inBoundOrder.ProcessName = entity.ProcessName;
                                        inBoundOrder.OrderSource = "WMS";
                                        inBoundOrder.CreateUser = item.CreateUser;
                                        inBoundOrder.CreateDate = DateTime.Now;
                                        InBoundOrderAddList.Add(inBoundOrder);
                                    }

                                    poList.Add(item.CustomerPoNumber);

                                    InBoundOrder inboundResult = new InBoundOrder();
                                    inboundResult.SoId = null;
                                    inboundResult.CustomerPoNumber = item.CustomerPoNumber;
                                    checkInBoundOrderAddResult.Add(inboundResult);
                                }

                                if (checkItemMasterAddResult.Where(u => u.AltItemNumber == item.AltItemNumber && (u.Style1 == null ? "" : u.Style1) == (item.Style1 == null ? "" : item.Style1) && (u.Style2 == null ? "" : u.Style2) == (item.Style2 == null ? "" : item.Style2) && (u.Style3 == null ? "" : u.Style3) == (item.Style3 == null ? "" : item.Style3) && u.ClientId == entity.ClientId).Count() == 0)
                                {
                                    var checklistItemMaster = idal.IItemMasterDAL.SelectBy(u => u.WhCode == entity.WhCode && u.AltItemNumber == item.AltItemNumber && (u.Style1 == null ? "" : u.Style1) == (item.Style1 == null ? "" : item.Style1) && (u.Style2 == null ? "" : u.Style2) == (item.Style2 == null ? "" : item.Style2) && (u.Style3 == null ? "" : u.Style3) == (item.Style3 == null ? "" : item.Style3) && u.ClientId == entity.ClientId);

                                    if (checklistItemMaster.Count() == 0)
                                    {
                                        ItemMaster itemMaster = new ItemMaster();
                                        itemMaster.ItemNumber = "SYS" + DI.IDGenerator.NewId;
                                        itemMaster.WhCode = entity.WhCode;
                                        itemMaster.AltItemNumber = item.AltItemNumber;
                                        itemMaster.ClientId = (int)entity.ClientId;
                                        itemMaster.ClientCode = entity.ClientCode;
                                        itemMaster.Style1 = item.Style1 ?? "";
                                        itemMaster.Style2 = item.Style2 ?? "";
                                        itemMaster.Style3 = item.Style3 ?? "";
                                        itemMaster.UnitFlag = 0;
                                        if (item.UnitName == "" || item.UnitName == null)
                                        {
                                            itemMaster.UnitName = "none";
                                        }
                                        else
                                        {
                                            itemMaster.UnitName = item.UnitName;
                                        }

                                        itemMaster.CreateUser = item.CreateUser;
                                        itemMaster.CreateDate = DateTime.Now;
                                        ItemMasterAddList.Add(itemMaster);   //款号不存在就新增
                                    }
                                    skuList.Add(item.AltItemNumber);

                                    ItemMaster itemResult = new ItemMaster();
                                    itemResult.ClientId = (int)entity.ClientId;
                                    itemResult.AltItemNumber = item.AltItemNumber;
                                    itemResult.Style1 = item.Style1 ?? "";
                                    itemResult.Style2 = item.Style2 ?? "";
                                    itemResult.Style3 = item.Style3 ?? "";
                                    checkItemMasterAddResult.Add(itemResult);
                                }
                            }
                        }
                    }

                    if (InBoundOrderAddList.Count > 0)
                    {
                        idal.IInBoundOrderDAL.Add(InBoundOrderAddList);
                        idal.IInBoundSODAL.SaveChanges();
                    }

                    if (ItemMasterAddList.Count > 0)
                    {
                        idal.IItemMasterDAL.Add(ItemMasterAddList);
                        idal.IItemMasterDAL.SaveChanges();
                    }

                    List<InBoundOrder> listInBoundOrder = idal.IInBoundOrderDAL.SelectBy(u => poList.Contains(u.CustomerPoNumber) && u.WhCode == fir.WhCode);

                    List<ItemMaster> listItemMaster = idal.IItemMasterDAL.SelectBy(u => skuList.Contains(u.AltItemNumber) && u.WhCode == fir.WhCode).OrderBy(u => u.Id).ToList();

                    //批量导入预录入
                    foreach (var entity1 in entityList)
                    {
                        foreach (var item11 in entity1.InBoundOrderDetailInsert)
                        {
                            InBoundOrder inBoundOrder = new InBoundOrder();

                            if (!string.IsNullOrEmpty(entity1.SoNumber))
                            {
                                InBoundSO inBoundSO = listInBoundSO.Where(u => u.SoNumber == entity1.SoNumber && u.ClientCode == entity1.ClientCode).First();

                                inBoundOrder = listInBoundOrder.Where(u => u.CustomerPoNumber == item11.CustomerPoNumber && u.ClientCode == entity1.ClientCode && u.SoId == inBoundSO.Id).First();
                            }
                            else
                            {
                                inBoundOrder = listInBoundOrder.Where(u => u.CustomerPoNumber == item11.CustomerPoNumber && u.ClientCode == entity1.ClientCode && u.SoId == null).First();
                            }

                            //判断款号是否存在
                            ItemMaster itemMaster = listItemMaster.Where(u => u.WhCode == entity1.WhCode && u.AltItemNumber == item11.AltItemNumber && (u.Style1 == null ? "" : u.Style1) == (item11.Style1 == null ? "" : item11.Style1) && (u.Style2 == null ? "" : u.Style2) == (item11.Style2 == null ? "" : item11.Style2) && (u.Style3 == null ? "" : u.Style3) == (item11.Style3 == null ? "" : item11.Style3) && u.ClientId == entity1.ClientId).First();

                            //添加InBoundOrderDetail
                            List<InBoundOrderDetail> listInBoundOrderDetail = new List<InBoundOrderDetail>();

                            if (itemMaster.UnitFlag == 1)
                            {
                                listInBoundOrderDetail = idal.IInBoundOrderDetailDAL.SelectBy(u => u.WhCode == Editentity.WhCode && u.ItemId == itemMaster.Id && u.PoId == inBoundOrder.Id && u.UnitId == item11.UnitId && u.UnitName == item11.UnitName).ToList();
                            }
                            else
                            {
                                listInBoundOrderDetail = idal.IInBoundOrderDetailDAL.SelectBy(u => u.WhCode == Editentity.WhCode && u.ItemId == itemMaster.Id && u.PoId == inBoundOrder.Id && u.UnitId == item11.UnitId).ToList();
                            }

                            InBoundOrderDetail inBoundOrderDetail = new InBoundOrderDetail();
                            int insertResult = 0;
                            if (listInBoundOrderDetail.Count == 0)
                            {
                                inBoundOrderDetail.WhCode = Editentity.WhCode;
                                inBoundOrderDetail.PoId = inBoundOrder.Id;
                                inBoundOrderDetail.ItemId = itemMaster.Id;
                                inBoundOrderDetail.UnitId = item11.UnitId;
                                if (item11.UnitName == "" || item11.UnitName == null)
                                {
                                    inBoundOrderDetail.UnitName = "none";
                                }
                                else
                                {
                                    if (itemMaster.UnitFlag == 1)
                                    {
                                        inBoundOrderDetail.UnitName = item11.UnitName;
                                    }
                                    else
                                    {
                                        inBoundOrderDetail.UnitName = itemMaster.UnitName;
                                    }
                                }

                                inBoundOrderDetail.Qty = item11.Qty;
                                inBoundOrderDetail.Weight = (item11.Weight ?? 0);
                                inBoundOrderDetail.CBM = (item11.CBM ?? 0);
                                inBoundOrderDetail.CreateUser = item11.CreateUser;
                                inBoundOrderDetail.CreateDate = DateTime.Now;
                                idal.IInBoundOrderDetailDAL.Add(inBoundOrderDetail);
                                insertResult = 1;
                            }
                            else
                            {
                                inBoundOrderDetail = listInBoundOrderDetail.First();
                                int yuluruQty = inBoundOrderDetail.Qty - inBoundOrderDetail.RegQty;
                                if (yuluruQty < item11.Qty)
                                {
                                    inBoundOrderDetail.Qty = inBoundOrderDetail.Qty + (item11.Qty - yuluruQty);
                                    inBoundOrderDetail.UpdateUser = item11.CreateUser;
                                    inBoundOrderDetail.UpdateDate = DateTime.Now;
                                    idal.IInBoundOrderDetailDAL.UpdateBy(inBoundOrderDetail, u => u.Id == inBoundOrderDetail.Id, new string[] { "Qty", "UpdateUser", "UpdateDate" });
                                }
                                insertResult = 2;
                            }

                            idal.IInBoundOrderDetailDAL.SaveChanges();

                            if (insertResult == 1 || insertResult == 2)
                            {
                                ReceiptRegisterInsert receiptRegisterInsert = new ReceiptRegisterInsert();
                                receiptRegisterInsert.WhCode = Editentity.WhCode;
                                receiptRegisterInsert.ReceiptId = receipt;
                                receiptRegisterInsert.InBoundOrderDetailId = inBoundOrderDetail.Id;
                                receiptRegisterInsert.CustomerPoNumber = item11.CustomerPoNumber;
                                receiptRegisterInsert.AltItemNumber = item11.AltItemNumber;
                                receiptRegisterInsert.PoId = inBoundOrder.Id;
                                receiptRegisterInsert.ItemId = itemMaster.Id;
                                receiptRegisterInsert.UnitName = item11.UnitName;
                                receiptRegisterInsert.UnitId = item11.UnitId;
                                receiptRegisterInsert.ProcessName = receiptRegister.ProcessName;
                                receiptRegisterInsert.ProcessId = (Int32)receiptRegister.ProcessId;

                                receiptRegisterInsert.RegQty = item11.Qty;
                                receiptRegisterInsert.CreateUser = item11.CreateUser;
                                receiptRegisterInsert.CreateDate = DateTime.Now;
                                listEntity.Add(receiptRegisterInsert);
                            }
                        }
                    }

                    if (listEntity.Count == 0)
                    {
                        return "导入登记明细出错！";
                    }

                    //创建收货批次
                    //如果 收货批次号有变化，才重新创建一个新的
                    if (receipt != Editentity.ReceiptId)
                    {
                        ReceiptRegisterAdd(Editentity, receiptRegister, receipt);
                    }
                    idal.SaveChanges();

                    //添加收货批次明细
                    foreach (var item in listEntity)
                    {
                        List<ReceiptRegisterDetail> list = idal.IReceiptRegisterDetailDAL.SelectBy(u => u.ReceiptId == item.ReceiptId && u.WhCode == item.WhCode && u.InOrderDetailId == item.InBoundOrderDetailId);
                        ReceiptRegisterDetail entityDetail = new ReceiptRegisterDetail();
                        if (list.Count == 0)
                        {
                            entityDetail.ReceiptId = item.ReceiptId;
                            entityDetail.InOrderDetailId = item.InBoundOrderDetailId;
                            entityDetail.WhCode = item.WhCode;
                            entityDetail.PoId = item.PoId;
                            entityDetail.ItemId = item.ItemId;
                            entityDetail.UnitId = item.UnitId;
                            entityDetail.UnitName = item.UnitName;
                            entityDetail.CustomerPoNumber = item.CustomerPoNumber;
                            entityDetail.AltItemNumber = item.AltItemNumber;
                            entityDetail.RegQty = item.RegQty;
                            entityDetail.CreateUser = item.CreateUser;
                            entityDetail.CreateDate = item.CreateDate;
                            idal.IReceiptRegisterDetailDAL.Add(entityDetail);
                        }
                        else
                        {
                            entityDetail = list.First();
                            entityDetail.RegQty = entityDetail.RegQty + item.RegQty;
                            entityDetail.UpdateUser = item.CreateUser;
                            entityDetail.UpdateDate = item.CreateDate;
                            idal.IReceiptRegisterDetailDAL.UpdateBy(entityDetail, u => u.Id == entityDetail.Id, "RegQty");
                        }

                        InBoundOrderDetail inBoundOrderDetail = idal.IInBoundOrderDetailDAL.SelectBy(u => u.Id == item.InBoundOrderDetailId).First();
                        inBoundOrderDetail.RegQty = inBoundOrderDetail.RegQty + item.RegQty;
                        inBoundOrderDetail.UpdateUser = item.CreateUser;
                        inBoundOrderDetail.UpdateDate = item.CreateDate;
                        idal.IInBoundOrderDetailDAL.UpdateBy(inBoundOrderDetail, u => u.Id == item.InBoundOrderDetailId, "RegQty");
                    }
                    idal.IInBoundOrderDetailDAL.SaveChanges();

                    //3.3 删除原收货批次明细
                    idal.IReceiptRegisterDetailDAL.DeleteByExtended(u => u.ReceiptId == Editentity.ReceiptId && u.WhCode == Editentity.WhCode && regDetailid.Contains(u.Id));

                    if (idal.IReceiptRegisterDetailDAL.SelectBy(u => u.ReceiptId == Editentity.ReceiptId && u.WhCode == Editentity.WhCode).Count == 0)
                    {
                        idal.IReceiptRegisterDAL.DeleteBy(u => u.ReceiptId == Editentity.ReceiptId && u.WhCode == Editentity.WhCode);
                    }

                    idal.IInBoundOrderDetailDAL.SaveChanges();

                    ReceiptRegister getregId = idal.IReceiptRegisterDAL.SelectBy(u => u.WhCode == Editentity.WhCode && u.ReceiptId == receipt).First();

                    //3.5 修改收货

                    var getReceiptList1 = from a in idal.IReceiptDAL.SelectAll()
                                          where a.WhCode == Editentity.WhCode && getpoid.Contains(a.PoId) && getitemid.Contains(a.ItemId) && a.ReceiptId == Editentity.ReceiptId
                                          && a.ClientCode == receiptRegister.ClientCode && getsoList.Contains(a.SoNumber)
                                          select a;
                    List<Receipt> getReceiptList = getReceiptList1.ToList();

                    var sql1 = from a in idal.IReceiptRegisterDAL.SelectAll()
                               join b in idal.IReceiptRegisterDetailDAL.SelectAll()
                                     on new { a.WhCode, a.ReceiptId }
                                 equals new { b.WhCode, b.ReceiptId } into b_join
                               from b in b_join.DefaultIfEmpty()
                               join c in idal.IInBoundOrderDetailDAL.SelectAll() on new { InOrderDetailId = b.InOrderDetailId } equals new { InOrderDetailId = c.Id } into c_join
                               from c in c_join.DefaultIfEmpty()
                               join d in idal.IInBoundOrderDAL.SelectAll() on new { PoId = c.PoId } equals new { PoId = d.Id } into d_join
                               from d in d_join.DefaultIfEmpty()
                               join e in idal.IInBoundSODAL.SelectAll() on new { SoId = (Int32)d.SoId } equals new { SoId = e.Id } into e_join
                               from e in e_join.DefaultIfEmpty()
                               join f in idal.IItemMasterDAL.SelectAll() on new { ItemId = c.ItemId } equals new { ItemId = f.Id } into f_join
                               from f in f_join.DefaultIfEmpty()
                               where
                                 a.ReceiptId == receipt && a.WhCode == Editentity.WhCode && getsoList.Contains(e.SoNumber)
                               select new EditInBoundDetailResult
                               {
                                   ClientCode = a.ClientCode,
                                   ReceiptId = a.ReceiptId,
                                   SoNumber = e.SoNumber,
                                   SoId = e.Id,
                                   PoId = d.Id,
                                   CustomerPoNumber = d.CustomerPoNumber,
                                   ItemId = f.Id,
                                   AltItemNumber = f.AltItemNumber,
                                   Style1 = f.Style1,
                                   Style2 = f.Style2,
                                   Style3 = f.Style3,
                                   Qty = (Int32?)b.RegQty,
                                   UnitName = b.UnitName,
                                   UnitId = b.UnitId,
                                   RegDetailId = b.Id
                               };

                    List<EditInBoundDetailResult> getNewList = sql1.ToList();
                    foreach (var item1 in getReceiptList)
                    {
                        List<EditInBoundDetailResult> list = getNewList.Where(u => u.SoNumber == item1.SoNumber && u.CustomerPoNumber == item1.CustomerPoNumber && u.AltItemNumber == item1.AltItemNumber && u.UnitName == item1.UnitName).ToList();

                        EditInBoundDetailResult edit = list.First();

                        idal.IReceiptDAL.UpdateByExtended(u => u.Id == item1.Id, u => new Receipt() { ClientCode = Editentity.ClientCode, ClientId = Editentity.ClientId, ReceiptId = receipt, RegId = getregId.Id, SoNumber = edit.SoNumber, CustomerPoNumber = edit.CustomerPoNumber, PoId = edit.PoId, AltItemNumber = edit.AltItemNumber, ItemId = edit.ItemId });
                    }

                    //3.6 修改库存

                    var getHudetailList1 = from a in idal.IHuDetailDAL.SelectAll()
                                           where a.WhCode == Editentity.WhCode && customerPoNumber.Contains(a.CustomerPoNumber) && getitemid.Contains(a.ItemId) && a.ReceiptId == Editentity.ReceiptId
                                           && a.ClientCode == receiptRegister.ClientCode && getsoList.Contains(a.SoNumber)
                                           select a;

                    List<HuDetail> getHudetailList = getHudetailList1.ToList();
                    foreach (var item2 in getHudetailList)
                    {
                        EditInBoundDetailResult edit = getNewList.Where(u => u.SoNumber == item2.SoNumber && u.CustomerPoNumber == item2.CustomerPoNumber && u.AltItemNumber == item2.AltItemNumber && u.UnitName == item2.UnitName).First();

                        idal.IHuDetailDAL.UpdateByExtended(u => u.Id == item2.Id, u => new HuDetail() { ClientCode = Editentity.ClientCode, ClientId = Editentity.ClientId, ReceiptId = receipt, SoNumber = edit.SoNumber, AltItemNumber = edit.AltItemNumber, ItemId = edit.ItemId });

                        idal.IHuMasterDAL.UpdateByExtended(u => u.WhCode == item2.WhCode && u.HuId == item2.HuId, u => new HuMaster() { ReceiptId = receipt });
                    }

                    //3.7 修改收货扫描

                    var getSerialNumberInList1 = from a in idal.ISerialNumberInDAL.SelectAll()
                                                 where a.WhCode == Editentity.WhCode && getpoid.Contains((Int32)a.PoId) && getitemid.Contains((Int32)a.ItemId) && a.ReceiptId == Editentity.ReceiptId
                                                 && a.ClientCode == receiptRegister.ClientCode && getsoList.Contains(a.SoNumber)
                                                 select a;

                    List<SerialNumberIn> getSerialNumberInList = getSerialNumberInList1.ToList();
                    foreach (var item2 in getSerialNumberInList)
                    {
                        EditInBoundDetailResult edit = getNewList.Where(u => u.SoNumber == item2.SoNumber && u.CustomerPoNumber == item2.CustomerPoNumber && u.AltItemNumber == item2.AltItemNumber).First();

                        idal.ISerialNumberInDAL.UpdateByExtended(u => u.Id == item2.Id, u => new SerialNumberIn() { ClientCode = Editentity.ClientCode, ClientId = Editentity.ClientId, ReceiptId = receipt, SoNumber = edit.SoNumber, CustomerPoNumber = edit.CustomerPoNumber, PoId = edit.PoId, AltItemNumber = edit.AltItemNumber, ItemId = edit.ItemId });
                    }

                    //3.8 修改工人工作量
                    idal.IWorkloadAccountDAL.UpdateByExtended(u => u.WhCode == Editentity.WhCode && u.ReceiptId == receiptRegister.ReceiptId, u => new WorkloadAccount() { ReceiptId = receipt });

                    List<TranLog> tranLogList = new List<TranLog>();
                    foreach (var item in getsoList)
                    {
                        //插入记录
                        TranLog tl = new TranLog();
                        tl.TranType = "60";
                        tl.Description = "修改客户名";
                        tl.TranDate = DateTime.Now;
                        tl.TranUser = Editentity.UserCode;
                        tl.WhCode = Editentity.WhCode;
                        tl.SoNumber = item;
                        tl.ReceiptId = Editentity.ReceiptId;
                        tl.ClientCode = receiptRegister.ClientCode;
                        tranLogList.Add(tl);

                        TranLog tl1 = new TranLog();
                        tl1.TranType = "60";
                        tl1.Description = "修改客户名";
                        tl1.TranDate = DateTime.Now;
                        tl1.TranUser = Editentity.UserCode;
                        tl1.WhCode = Editentity.WhCode;
                        tl1.SoNumber = item;
                        tl1.ReceiptId = receipt;
                        tl1.ClientCode = Editentity.ClientCode;
                        tranLogList.Add(tl1);
                    }
                    idal.ITranLogDAL.Add(tranLogList);

                    //3.8 最后比较 收货批次号是否更改
                    if (receipt == Editentity.ReceiptId)
                    {
                        ReceiptRegister regEdit = new ReceiptRegister();
                        regEdit.ClientCode = Editentity.ClientCode;
                        regEdit.ClientId = Editentity.ClientId;

                        idal.IReceiptRegisterDAL.UpdateBy(regEdit, u => u.Id == receiptRegister.Id, new string[] { "ClientCode", "ClientId" });
                    }
                    #endregion

                    idal.SaveChanges();
                    trans.Complete();
                    return "Y";
                }
                catch
                {
                    trans.Dispose();//出现异常，事务手动释放
                    return "修改异常，请重新提交！";
                }
            }
        }

        //修改SO
        public string EditDetailSoNumber(EditInBoundResult entity, string[] soList)
        {
            using (TransactionScope trans = new TransactionScope())
            {
                try
                {
                    #region 优化
                    List<ReceiptRegister> ReceiptRegisterList = idal.IReceiptRegisterDAL.SelectBy(u => u.WhCode == entity.WhCode && u.ReceiptId == entity.ReceiptId);
                    if (ReceiptRegisterList.Count == 0)
                    {
                        return "未找到明细，无法修改！";
                    }

                    List<HuDetail> checkPlanQtyList = idal.IHuDetailDAL.SelectBy(u => u.WhCode == entity.WhCode && u.ReceiptId == entity.ReceiptId && (u.PlanQty ?? 0) != 0 && soList.Contains(u.SoNumber));
                    if (checkPlanQtyList.Count > 0)
                    {
                        return "SO已做备货单无法修改，请撤销释放后修改！";
                    }

                    ReceiptRegister receiptRegister = ReceiptRegisterList.First();
                    entity.ClientCode = receiptRegister.ClientCode;
                    entity.ClientId = receiptRegister.ClientId;

                    //string s = "select a.ClientCode,a.ReceiptId,e.SoNumber,e.Id soid,d.Id poid,d.CustomerPoNumber,f.Id itmeid,f.AltItemNumber,f.Style1,f.Style2,f.Style3,b.RegQty,b.UnitName,b.UnitId,b.Id regDetailId from ReceiptRegister a
                    //left join ReceiptRegisterDetail b on a.WhCode = b.WhCode and a.ReceiptId = b.ReceiptId
                    //left join InBoundOrderDetail c on b.InOrderDetailId = c.Id
                    //left join InBoundOrder d on c.PoId = d.Id
                    //left join InBoundSO e on d.SoId = e.Id
                    //left join ItemMaster f on c.ItemId = f.Id
                    //where a.ReceiptId = 'EI180522144418104' and e.SoNumber = 'T052201'";

                    var sql = (from a in idal.IReceiptRegisterDAL.SelectAll()
                               join b in idal.IReceiptRegisterDetailDAL.SelectAll()
                                     on new { a.WhCode, a.ReceiptId }
                                 equals new { b.WhCode, b.ReceiptId } into b_join
                               from b in b_join.DefaultIfEmpty()
                               join c in idal.IInBoundOrderDetailDAL.SelectAll() on new { InOrderDetailId = b.InOrderDetailId } equals new { InOrderDetailId = c.Id } into c_join
                               from c in c_join.DefaultIfEmpty()
                               join d in idal.IInBoundOrderDAL.SelectAll() on new { PoId = c.PoId } equals new { PoId = d.Id } into d_join
                               from d in d_join.DefaultIfEmpty()
                               join e in idal.IInBoundSODAL.SelectAll() on new { SoId = (Int32)d.SoId } equals new { SoId = e.Id } into e_join
                               from e in e_join.DefaultIfEmpty()
                               join f in idal.IItemMasterDAL.SelectAll() on new { ItemId = c.ItemId } equals new { ItemId = f.Id } into f_join
                               from f in f_join.DefaultIfEmpty()
                               where
                                 a.ReceiptId == entity.ReceiptId && a.WhCode == entity.WhCode && soList.Contains(e.SoNumber)
                               select new EditInBoundDetailResult
                               {
                                   ClientCode = a.ClientCode,
                                   ReceiptId = a.ReceiptId,
                                   SoNumber = e.SoNumber,
                                   SoId = e.Id,
                                   PoId = d.Id,
                                   CustomerPoNumber = d.CustomerPoNumber,
                                   ItemId = f.Id,
                                   AltItemNumber = f.AltItemNumber,
                                   Style1 = f.Style1 ?? "",
                                   Style2 = f.Style2 ?? "",
                                   Style3 = f.Style3 ?? "",
                                   Qty = (Int32?)b.RegQty,
                                   UnitName = b.UnitName,
                                   UnitId = b.UnitId,
                                   RegDetailId = b.Id,
                                   OrderType = d.OrderType
                               }).Distinct();

                    if (sql.Where(u => u.SoNumber == entity.SoNumber).Count() > 0)
                    {
                        return "SO号与原SO一致，无需修改！";
                    }
                    //如果是多种单位的货 暂时无法修改
                    string[] unitName1 = (from a in idal.IUnitDefaultDAL.SelectAll()
                                          where a.WhCode == entity.WhCode
                                          select a.UnitName).ToArray();
                    if (sql.Where(u => !unitName1.Contains(u.UnitName)).Count() > 0)
                    {
                        return "款号属于多种定制单位，无法修改！";
                    }

                    List<EditInBoundDetailResult> getList = sql.ToList();

                    //1. 取得原预录入明细--后面修改收货及库存有用
                    int[] getsoid = (from a in getList
                                     select a.SoId).Distinct().ToArray();

                    int[] getpoid = (from a in getList
                                     select a.PoId).Distinct().ToArray();

                    int[] getitemid = (from a in getList
                                       select a.ItemId).Distinct().ToArray();

                    int[] regDetailid = (from a in getList
                                         select a.RegDetailId).Distinct().ToArray();

                    string[] customerPoNumber = (from a in getList
                                                 select a.CustomerPoNumber).Distinct().ToArray();

                    //2. 重建预录入
                    string getclient = "", getSo = "", getPo = "", getSku = "", getStyle1 = "", getStyle2 = "", getStyle3 = "", getQty = "", unitName = "";

                    List<InBoundOrderInsert> entityList = new List<InBoundOrderInsert>();
                    foreach (var item in getList)
                    {
                        getclient = entity.ClientCode;
                        getSo = entity.SoNumber;
                        getPo = item.CustomerPoNumber;
                        getSku = item.AltItemNumber;
                        getStyle1 = item.Style1;
                        getStyle2 = item.Style2;
                        getStyle3 = item.Style3;
                        getQty = item.Qty.ToString();
                        unitName = item.UnitName;

                        if (entityList.Where(u => u.SoNumber == getSo.Trim() && u.ClientCode == getclient.Trim()).Count() == 0)
                        {
                            InBoundOrderInsert entity1 = new InBoundOrderInsert();
                            entity1.WhCode = entity.WhCode;
                            entity1.ClientCode = getclient.Trim();
                            entity1.SoNumber = getSo.Trim();
                            entity1.CreateUser = entity.UserCode;
                            entity1.OrderType = item.OrderType;

                            List<InBoundOrderDetailInsert> orderDetailList = new List<InBoundOrderDetailInsert>();

                            InBoundOrderDetailInsert orderDetail = new InBoundOrderDetailInsert();
                            orderDetail.CustomerPoNumber = getPo.Trim().ToString();
                            orderDetail.AltItemNumber = getSku.Trim().ToString();
                            orderDetail.Style1 = getStyle1.ToString();
                            orderDetail.Style2 = getStyle2.ToString();
                            orderDetail.Style3 = getStyle3.ToString();
                            orderDetail.UnitName = unitName;
                            orderDetail.Qty = Convert.ToInt32(getQty);
                            orderDetail.CreateUser = entity.UserCode;
                            orderDetailList.Add(orderDetail);

                            entity1.InBoundOrderDetailInsert = orderDetailList;
                            entityList.Add(entity1);
                        }
                        else
                        {
                            InBoundOrderInsert oldentity = entityList.Where(u => u.SoNumber == getSo.Trim() && u.ClientCode == getclient.Trim()).First();
                            entityList.Remove(oldentity);

                            InBoundOrderInsert newentity = oldentity;

                            List<InBoundOrderDetailInsert> orderDetailList = oldentity.InBoundOrderDetailInsert.ToList();
                            if (orderDetailList.Where(u => u.CustomerPoNumber == getPo.Trim() && u.AltItemNumber == getSku.Trim() && u.Style1 == getStyle1 && u.Style2 == getStyle2 && u.Style3 == getStyle3).Count() == 0)
                            {
                                InBoundOrderDetailInsert orderDetail = new InBoundOrderDetailInsert();
                                orderDetail.CustomerPoNumber = getPo.Trim().ToString();
                                orderDetail.AltItemNumber = getSku.Trim().ToString();
                                orderDetail.Style1 = getStyle1.Trim().ToString();
                                orderDetail.Style2 = getStyle2.ToString();
                                orderDetail.Style3 = getStyle3.ToString();
                                orderDetail.UnitName = unitName;
                                orderDetail.Qty = Convert.ToInt32(getQty);
                                orderDetail.CreateUser = entity.UserCode;
                                orderDetailList.Add(orderDetail);
                            }
                            else
                            {
                                InBoundOrderDetailInsert oldorderDetail = orderDetailList.Where(u => u.CustomerPoNumber == getPo.Trim() && u.AltItemNumber == getSku.Trim() && u.Style1 == getStyle1.Trim() && u.Style2 == getStyle2 && u.Style3 == getStyle3).First();

                                orderDetailList.Remove(oldorderDetail);
                                InBoundOrderDetailInsert neworderDetail = oldorderDetail;
                                neworderDetail.Qty = oldorderDetail.Qty + Convert.ToInt32(getQty);
                                orderDetailList.Add(neworderDetail);
                            }

                            newentity.InBoundOrderDetailInsert = orderDetailList;
                            entityList.Add(newentity);
                        }
                    }

                    //3.2 创建收货登记
                    string receipt = entity.ReceiptId;

                    string s = AddInBoundRegDetail(entity, entityList, receiptRegister, receipt);
                    if (s != "Y")
                    {
                        return s;
                    }

                    //3.3 删除原收货批次明细
                    idal.IReceiptRegisterDetailDAL.DeleteByExtended(u => u.ReceiptId == entity.ReceiptId && u.WhCode == entity.WhCode && regDetailid.Contains(u.Id));

                    idal.IInBoundOrderDetailDAL.SaveChanges();

                    //3.5 修改收货

                    var sql1 = from a in idal.IReceiptRegisterDAL.SelectAll()
                               join b in idal.IReceiptRegisterDetailDAL.SelectAll()
                                     on new { a.WhCode, a.ReceiptId }
                                 equals new { b.WhCode, b.ReceiptId } into b_join
                               from b in b_join.DefaultIfEmpty()
                               join c in idal.IInBoundOrderDetailDAL.SelectAll() on new { InOrderDetailId = b.InOrderDetailId } equals new { InOrderDetailId = c.Id } into c_join
                               from c in c_join.DefaultIfEmpty()
                               join d in idal.IInBoundOrderDAL.SelectAll() on new { PoId = c.PoId } equals new { PoId = d.Id } into d_join
                               from d in d_join.DefaultIfEmpty()
                               join e in idal.IInBoundSODAL.SelectAll() on new { SoId = (Int32)d.SoId } equals new { SoId = e.Id } into e_join
                               from e in e_join.DefaultIfEmpty()
                               join f in idal.IItemMasterDAL.SelectAll() on new { ItemId = c.ItemId } equals new { ItemId = f.Id } into f_join
                               from f in f_join.DefaultIfEmpty()
                               where
                                 a.ReceiptId == receipt && a.WhCode == entity.WhCode && e.SoNumber == entity.SoNumber
                               select new EditInBoundDetailResult
                               {
                                   ClientCode = a.ClientCode,
                                   ReceiptId = a.ReceiptId,
                                   SoNumber = e.SoNumber,
                                   SoId = e.Id,
                                   PoId = d.Id,
                                   CustomerPoNumber = d.CustomerPoNumber,
                                   ItemId = f.Id,
                                   AltItemNumber = f.AltItemNumber,
                                   Style1 = f.Style1,
                                   Style2 = f.Style2,
                                   Style3 = f.Style3,
                                   Qty = (Int32?)b.RegQty,
                                   UnitName = b.UnitName,
                                   UnitId = b.UnitId,
                                   RegDetailId = b.Id
                               };

                    List<EditInBoundDetailResult> getNewList = sql1.ToList();

                    var getReceiptList1 = from a in idal.IReceiptDAL.SelectAll()
                                          where a.WhCode == entity.WhCode && getpoid.Contains(a.PoId) && getitemid.Contains(a.ItemId) && a.ReceiptId == entity.ReceiptId
                                          && a.ClientCode == receiptRegister.ClientCode && soList.Contains(a.SoNumber)
                                          select a;
                    List<Receipt> getReceiptList = getReceiptList1.ToList();

                    foreach (var item1 in getReceiptList)
                    {
                        EditInBoundDetailResult edit = getNewList.Where(u => u.SoNumber == entity.SoNumber && u.CustomerPoNumber == item1.CustomerPoNumber && u.AltItemNumber == item1.AltItemNumber && u.UnitName == item1.UnitName && u.ItemId == item1.ItemId).First();

                        idal.IReceiptDAL.UpdateByExtended(u => u.Id == item1.Id, u => new Receipt() { SoNumber = edit.SoNumber, CustomerPoNumber = edit.CustomerPoNumber, PoId = edit.PoId, AltItemNumber = edit.AltItemNumber, ItemId = edit.ItemId });
                    }

                    //3.6 修改库存

                    var getHudetailList1 = from a in idal.IHuDetailDAL.SelectAll()
                                           where a.WhCode == entity.WhCode && customerPoNumber.Contains(a.CustomerPoNumber) && getitemid.Contains(a.ItemId) && a.ReceiptId == entity.ReceiptId
                                           && a.ClientCode == receiptRegister.ClientCode && soList.Contains(a.SoNumber)
                                           select a;

                    List<HuDetail> getHudetailList = getHudetailList1.ToList();
                    foreach (var item2 in getHudetailList)
                    {
                        EditInBoundDetailResult edit = getNewList.Where(u => u.SoNumber == entity.SoNumber && u.CustomerPoNumber == item2.CustomerPoNumber && u.AltItemNumber == item2.AltItemNumber && u.UnitName == item2.UnitName && u.ItemId == item2.ItemId).First();

                        idal.IHuDetailDAL.UpdateByExtended(u => u.Id == item2.Id, u => new HuDetail() { SoNumber = edit.SoNumber, CustomerPoNumber = edit.CustomerPoNumber, AltItemNumber = edit.AltItemNumber, ItemId = edit.ItemId });
                    }

                    //3.7 修改收货扫描

                    var getSerialNumberInList1 = from a in idal.ISerialNumberInDAL.SelectAll()
                                                 where a.WhCode == entity.WhCode && customerPoNumber.Contains(a.CustomerPoNumber) && getitemid.Contains((Int32)a.ItemId) && a.ReceiptId == entity.ReceiptId
                                                 && a.ClientCode == receiptRegister.ClientCode && soList.Contains(a.SoNumber)
                                                 select a;

                    List<SerialNumberIn> getSerialNumberInList = getSerialNumberInList1.ToList();
                    foreach (var item2 in getSerialNumberInList)
                    {
                        EditInBoundDetailResult edit = getNewList.Where(u => u.SoNumber == entity.SoNumber && u.CustomerPoNumber == item2.CustomerPoNumber && u.AltItemNumber == item2.AltItemNumber && u.ItemId == item2.ItemId).First();

                        idal.ISerialNumberInDAL.UpdateByExtended(u => u.Id == item2.Id, u => new SerialNumberIn() { SoNumber = edit.SoNumber, CustomerPoNumber = edit.CustomerPoNumber, PoId = edit.PoId, AltItemNumber = edit.AltItemNumber, ItemId = edit.ItemId });
                    }


                    List<TranLog> tranLogList = new List<TranLog>();
                    foreach (var item in soList)
                    {
                        //插入记录
                        TranLog tl = new TranLog();
                        tl.TranType = "61";
                        tl.Description = "修改SO";
                        tl.TranDate = DateTime.Now;
                        tl.TranUser = entity.UserCode;
                        tl.WhCode = entity.WhCode;
                        tl.SoNumber = item;
                        tl.ReceiptId = entity.ReceiptId;
                        tl.ClientCode = receiptRegister.ClientCode;
                        tranLogList.Add(tl);

                        TranLog tl1 = new TranLog();
                        tl1.TranType = "61";
                        tl1.Description = "修改SO";
                        tl1.TranDate = DateTime.Now;
                        tl1.TranUser = entity.UserCode;
                        tl1.WhCode = entity.WhCode;
                        tl1.SoNumber = entity.SoNumber;
                        tl1.ReceiptId = receipt;
                        tl1.ClientCode = receiptRegister.ClientCode;
                        tranLogList.Add(tl1);
                    }
                    idal.ITranLogDAL.Add(tranLogList);

                    #endregion

                    idal.SaveChanges();
                    trans.Complete();
                    return "Y";
                }
                catch
                {
                    trans.Dispose();//出现异常，事务手动释放
                    return "修改异常，请重新提交！";
                }
            }
        }

        //修改PO
        public string EditDetailCustomerPoNumber(EditInBoundResult entity, string[] soList, string[] poList)
        {
            using (TransactionScope trans = new TransactionScope())
            {
                try
                {
                    #region 优化
                    List<ReceiptRegister> ReceiptRegisterList = idal.IReceiptRegisterDAL.SelectBy(u => u.WhCode == entity.WhCode && u.ReceiptId == entity.ReceiptId);
                    if (ReceiptRegisterList.Count == 0)
                    {
                        return "未找到明细，无法修改！";
                    }

                    List<HuDetail> checkPlanQtyList = idal.IHuDetailDAL.SelectBy(u => u.WhCode == entity.WhCode && u.ReceiptId == entity.ReceiptId && (u.PlanQty ?? 0) != 0 && soList.Contains(u.SoNumber) && poList.Contains(u.CustomerPoNumber));
                    if (checkPlanQtyList.Count > 0)
                    {
                        return "SOPO已做备货单无法修改，请撤销释放后修改！";
                    }

                    ReceiptRegister receiptRegister = ReceiptRegisterList.First();
                    entity.ClientCode = receiptRegister.ClientCode;
                    entity.ClientId = receiptRegister.ClientId;

                    var sql = (from a in idal.IReceiptRegisterDAL.SelectAll()
                               join b in idal.IReceiptRegisterDetailDAL.SelectAll()
                                     on new { a.WhCode, a.ReceiptId }
                                 equals new { b.WhCode, b.ReceiptId } into b_join
                               from b in b_join.DefaultIfEmpty()
                               join c in idal.IInBoundOrderDetailDAL.SelectAll() on new { InOrderDetailId = b.InOrderDetailId } equals new { InOrderDetailId = c.Id } into c_join
                               from c in c_join.DefaultIfEmpty()
                               join d in idal.IInBoundOrderDAL.SelectAll() on new { PoId = c.PoId } equals new { PoId = d.Id } into d_join
                               from d in d_join.DefaultIfEmpty()
                               join e in idal.IInBoundSODAL.SelectAll() on new { SoId = (Int32)d.SoId } equals new { SoId = e.Id } into e_join
                               from e in e_join.DefaultIfEmpty()
                               join f in idal.IItemMasterDAL.SelectAll() on new { ItemId = c.ItemId } equals new { ItemId = f.Id } into f_join
                               from f in f_join.DefaultIfEmpty()
                               where
                                 a.ReceiptId == entity.ReceiptId && a.WhCode == entity.WhCode && soList.Contains(e.SoNumber) && poList.Contains(d.CustomerPoNumber)
                               select new EditInBoundDetailResult
                               {
                                   ClientCode = a.ClientCode,
                                   ReceiptId = a.ReceiptId,
                                   SoNumber = e.SoNumber,
                                   SoId = e.Id,
                                   PoId = d.Id,
                                   CustomerPoNumber = d.CustomerPoNumber,
                                   ItemId = f.Id,
                                   AltItemNumber = f.AltItemNumber,
                                   Style1 = f.Style1 ?? "",
                                   Style2 = f.Style2 ?? "",
                                   Style3 = f.Style3 ?? "",
                                   Qty = (Int32?)b.RegQty,
                                   UnitName = b.UnitName,
                                   UnitId = b.UnitId,
                                   RegDetailId = b.Id,
                                   OrderType = d.OrderType
                               }).Distinct();

                    if (sql.Where(u => u.CustomerPoNumber == entity.CustomerPoNumber).Count() > 0)
                    {
                        return "PO号与原PO一致，无需修改！";
                    }

                    //如果是多种单位的货 暂时无法修改
                    string[] unitName1 = (from a in idal.IUnitDefaultDAL.SelectAll()
                                          where a.WhCode == entity.WhCode
                                          select a.UnitName).ToArray();
                    if (sql.Where(u => !unitName1.Contains(u.UnitName)).Count() > 0)
                    {
                        return "款号属于多种定制单位，无法修改！";
                    }

                    List<EditInBoundDetailResult> getList = sql.ToList();

                    //1. 取得原预录入明细--后面修改收货及库存有用
                    int[] getsoid = (from a in getList
                                     select a.SoId).Distinct().ToArray();

                    int[] getpoid = (from a in getList
                                     select a.PoId).Distinct().ToArray();

                    int[] getitemid = (from a in getList
                                       select a.ItemId).Distinct().ToArray();

                    int[] regDetailid = (from a in getList
                                         select a.RegDetailId).Distinct().ToArray();

                    string[] customerPoNumber = (from a in getList
                                                 select a.CustomerPoNumber).Distinct().ToArray();

                    //2. 重建预录入
                    string getclient = "", getSo = "", getPo = "", getSku = "", getStyle1 = "", getStyle2 = "", getStyle3 = "", getQty = "", unitName = "";

                    List<InBoundOrderInsert> entityList = new List<InBoundOrderInsert>();
                    foreach (var item in getList)
                    {
                        getclient = entity.ClientCode;
                        getSo = item.SoNumber;
                        getPo = entity.CustomerPoNumber;
                        getSku = item.AltItemNumber;
                        getStyle1 = item.Style1;
                        getStyle2 = item.Style2;
                        getStyle3 = item.Style3;
                        getQty = item.Qty.ToString();
                        unitName = item.UnitName;

                        if (entityList.Where(u => u.SoNumber == getSo.Trim() && u.ClientCode == getclient.Trim()).Count() == 0)
                        {
                            InBoundOrderInsert entity1 = new InBoundOrderInsert();
                            entity1.WhCode = entity.WhCode;
                            entity1.ClientCode = getclient.Trim();
                            entity1.SoNumber = getSo.Trim();
                            entity1.CreateUser = entity.UserCode;
                            entity1.OrderType = item.OrderType;

                            List<InBoundOrderDetailInsert> orderDetailList = new List<InBoundOrderDetailInsert>();

                            InBoundOrderDetailInsert orderDetail = new InBoundOrderDetailInsert();
                            orderDetail.CustomerPoNumber = getPo.Trim().ToString();
                            orderDetail.AltItemNumber = getSku.Trim().ToString();
                            orderDetail.Style1 = getStyle1.ToString();
                            orderDetail.Style2 = getStyle2.ToString();
                            orderDetail.Style3 = getStyle3.ToString();
                            orderDetail.UnitName = unitName;
                            orderDetail.Qty = Convert.ToInt32(getQty);
                            orderDetail.CreateUser = entity.UserCode;
                            orderDetailList.Add(orderDetail);

                            entity1.InBoundOrderDetailInsert = orderDetailList;
                            entityList.Add(entity1);
                        }
                        else
                        {
                            InBoundOrderInsert oldentity = entityList.Where(u => u.SoNumber == getSo.Trim() && u.ClientCode == getclient.Trim()).First();
                            entityList.Remove(oldentity);

                            InBoundOrderInsert newentity = oldentity;

                            List<InBoundOrderDetailInsert> orderDetailList = oldentity.InBoundOrderDetailInsert.ToList();
                            if (orderDetailList.Where(u => u.CustomerPoNumber == getPo.Trim() && u.AltItemNumber == getSku.Trim() && u.Style1 == getStyle1 && u.Style2 == getStyle2 && u.Style3 == getStyle3).Count() == 0)
                            {
                                InBoundOrderDetailInsert orderDetail = new InBoundOrderDetailInsert();
                                orderDetail.CustomerPoNumber = getPo.Trim().ToString();
                                orderDetail.AltItemNumber = getSku.Trim().ToString();
                                orderDetail.Style1 = getStyle1.Trim().ToString();
                                orderDetail.Style2 = getStyle2.ToString();
                                orderDetail.Style3 = getStyle3.ToString();
                                orderDetail.UnitName = unitName;
                                orderDetail.Qty = Convert.ToInt32(getQty);
                                orderDetail.CreateUser = entity.UserCode;
                                orderDetailList.Add(orderDetail);
                            }
                            else
                            {
                                InBoundOrderDetailInsert oldorderDetail = orderDetailList.Where(u => u.CustomerPoNumber == getPo.Trim() && u.AltItemNumber == getSku.Trim() && u.Style1 == getStyle1.Trim() && u.Style2 == getStyle2 && u.Style3 == getStyle3).First();

                                orderDetailList.Remove(oldorderDetail);
                                InBoundOrderDetailInsert neworderDetail = oldorderDetail;
                                neworderDetail.Qty = oldorderDetail.Qty + Convert.ToInt32(getQty);
                                orderDetailList.Add(neworderDetail);
                            }

                            newentity.InBoundOrderDetailInsert = orderDetailList;
                            entityList.Add(newentity);
                        }
                    }


                    //3.2 创建收货登记
                    string receipt = entity.ReceiptId;

                    string s = AddInBoundRegDetail(entity, entityList, receiptRegister, receipt);
                    if (s != "Y")
                    {
                        return s;
                    }

                    //3.3 删除原收货批次明细
                    idal.IReceiptRegisterDetailDAL.DeleteByExtended(u => u.ReceiptId == entity.ReceiptId && u.WhCode == entity.WhCode && regDetailid.Contains(u.Id));

                    idal.IInBoundOrderDetailDAL.SaveChanges();

                    //3.5 修改收货

                    var sql1 = from a in idal.IReceiptRegisterDAL.SelectAll()
                               join b in idal.IReceiptRegisterDetailDAL.SelectAll()
                                     on new { a.WhCode, a.ReceiptId }
                                 equals new { b.WhCode, b.ReceiptId } into b_join
                               from b in b_join.DefaultIfEmpty()
                               join c in idal.IInBoundOrderDetailDAL.SelectAll() on new { InOrderDetailId = b.InOrderDetailId } equals new { InOrderDetailId = c.Id } into c_join
                               from c in c_join.DefaultIfEmpty()
                               join d in idal.IInBoundOrderDAL.SelectAll() on new { PoId = c.PoId } equals new { PoId = d.Id } into d_join
                               from d in d_join.DefaultIfEmpty()
                               join e in idal.IInBoundSODAL.SelectAll() on new { SoId = (Int32)d.SoId } equals new { SoId = e.Id } into e_join
                               from e in e_join.DefaultIfEmpty()
                               join f in idal.IItemMasterDAL.SelectAll() on new { ItemId = c.ItemId } equals new { ItemId = f.Id } into f_join
                               from f in f_join.DefaultIfEmpty()
                               where
                                 a.ReceiptId == receipt && a.WhCode == entity.WhCode && soList.Contains(e.SoNumber) && d.CustomerPoNumber == entity.CustomerPoNumber
                               select new EditInBoundDetailResult
                               {
                                   ClientCode = a.ClientCode,
                                   ReceiptId = a.ReceiptId,
                                   SoNumber = e.SoNumber,
                                   SoId = e.Id,
                                   PoId = d.Id,
                                   CustomerPoNumber = d.CustomerPoNumber,
                                   ItemId = f.Id,
                                   AltItemNumber = f.AltItemNumber,
                                   Style1 = f.Style1,
                                   Style2 = f.Style2,
                                   Style3 = f.Style3,
                                   Qty = (Int32?)b.RegQty,
                                   UnitName = b.UnitName,
                                   UnitId = b.UnitId,
                                   RegDetailId = b.Id
                               };

                    List<EditInBoundDetailResult> getNewList = sql1.ToList();

                    var getReceiptList1 = from a in idal.IReceiptDAL.SelectAll()
                                          where a.WhCode == entity.WhCode && getpoid.Contains(a.PoId) && getitemid.Contains(a.ItemId) && a.ReceiptId == entity.ReceiptId
                                          && a.ClientCode == receiptRegister.ClientCode && soList.Contains(a.SoNumber)
                                          select a;
                    List<Receipt> getReceiptList = getReceiptList1.ToList();

                    foreach (var item1 in getReceiptList)
                    {
                        EditInBoundDetailResult edit = getNewList.Where(u => u.SoNumber == item1.SoNumber && u.CustomerPoNumber == entity.CustomerPoNumber && u.AltItemNumber == item1.AltItemNumber && u.UnitName == item1.UnitName && u.ItemId == item1.ItemId).First();

                        idal.IReceiptDAL.UpdateByExtended(u => u.Id == item1.Id, u => new Receipt() { SoNumber = edit.SoNumber, CustomerPoNumber = edit.CustomerPoNumber, PoId = edit.PoId, AltItemNumber = edit.AltItemNumber, ItemId = edit.ItemId });
                    }

                    //3.6 修改库存

                    var getHudetailList1 = from a in idal.IHuDetailDAL.SelectAll()
                                           where a.WhCode == entity.WhCode && customerPoNumber.Contains(a.CustomerPoNumber) && getitemid.Contains(a.ItemId) && a.ReceiptId == entity.ReceiptId
                                           && a.ClientCode == receiptRegister.ClientCode && soList.Contains(a.SoNumber)
                                           select a;

                    List<HuDetail> getHudetailList = getHudetailList1.ToList();
                    foreach (var item2 in getHudetailList)
                    {
                        EditInBoundDetailResult edit = getNewList.Where(u => u.SoNumber == item2.SoNumber && u.CustomerPoNumber == entity.CustomerPoNumber && u.AltItemNumber == item2.AltItemNumber && u.UnitName == item2.UnitName && u.ItemId == item2.ItemId).First();

                        idal.IHuDetailDAL.UpdateByExtended(u => u.Id == item2.Id, u => new HuDetail() { SoNumber = edit.SoNumber, CustomerPoNumber = edit.CustomerPoNumber, AltItemNumber = edit.AltItemNumber, ItemId = edit.ItemId });
                    }

                    //3.7 修改收货扫描

                    var getSerialNumberInList1 = from a in idal.ISerialNumberInDAL.SelectAll()
                                                 where a.WhCode == entity.WhCode && customerPoNumber.Contains(a.CustomerPoNumber) && getitemid.Contains((Int32)a.ItemId) && a.ReceiptId == entity.ReceiptId
                                                 && a.ClientCode == receiptRegister.ClientCode && soList.Contains(a.SoNumber)
                                                 select a;

                    List<SerialNumberIn> getSerialNumberInList = getSerialNumberInList1.ToList();
                    foreach (var item2 in getSerialNumberInList)
                    {
                        EditInBoundDetailResult edit = getNewList.Where(u => u.SoNumber == item2.SoNumber && u.CustomerPoNumber == entity.CustomerPoNumber && u.AltItemNumber == item2.AltItemNumber && u.ItemId == item2.ItemId).First();

                        idal.ISerialNumberInDAL.UpdateByExtended(u => u.Id == item2.Id, u => new SerialNumberIn() { SoNumber = edit.SoNumber, CustomerPoNumber = edit.CustomerPoNumber, PoId = edit.PoId, AltItemNumber = edit.AltItemNumber, ItemId = edit.ItemId });
                    }

                    List<TranLog> tranLogList = new List<TranLog>();
                    for (int i = 0; i < soList.Length; i++)
                    {
                        //插入记录
                        TranLog tl = new TranLog();
                        tl.TranType = "62";
                        tl.Description = "修改PO";
                        tl.TranDate = DateTime.Now;
                        tl.TranUser = entity.UserCode;
                        tl.WhCode = entity.WhCode;
                        tl.SoNumber = soList[i].ToString();
                        tl.CustomerPoNumber = poList[i].ToString();
                        tl.ReceiptId = entity.ReceiptId;
                        tl.ClientCode = receiptRegister.ClientCode;
                        tranLogList.Add(tl);

                        TranLog tl1 = new TranLog();
                        tl1.TranType = "62";
                        tl1.Description = "修改PO";
                        tl1.TranDate = DateTime.Now;
                        tl1.TranUser = entity.UserCode;
                        tl1.WhCode = entity.WhCode;
                        tl1.SoNumber = soList[i].ToString();
                        tl1.CustomerPoNumber = entity.CustomerPoNumber;
                        tl1.ReceiptId = receipt;
                        tl1.ClientCode = receiptRegister.ClientCode;
                        tranLogList.Add(tl1);
                    }
                    idal.ITranLogDAL.Add(tranLogList);
                    #endregion

                    idal.SaveChanges();
                    trans.Complete();
                    return "Y";
                }
                catch
                {
                    trans.Dispose();//出现异常，事务手动释放
                    return "修改异常，请重新提交！";
                }
            }
        }

        //修改款号
        public string EditDetailAltItemNumber(EditInBoundResult entity, string[] soList, string[] poList, string[] itemList)
        {
            using (TransactionScope trans = new TransactionScope())
            {
                try
                {
                    #region 优化
                    List<ReceiptRegister> ReceiptRegisterList = idal.IReceiptRegisterDAL.SelectBy(u => u.WhCode == entity.WhCode && u.ReceiptId == entity.ReceiptId);
                    if (ReceiptRegisterList.Count == 0)
                    {
                        return "未找到明细，无法修改！";
                    }


                    List<HuDetail> checkPlanQtyList = idal.IHuDetailDAL.SelectBy(u => u.WhCode == entity.WhCode && u.ReceiptId == entity.ReceiptId && (u.PlanQty ?? 0) != 0 && soList.Contains(u.SoNumber) && poList.Contains(u.CustomerPoNumber) && itemList.Contains(u.AltItemNumber));
                    if (checkPlanQtyList.Count > 0)
                    {
                        return "款号已做备货单无法修改，请撤销释放后修改！";
                    }

                    ReceiptRegister receiptRegister = ReceiptRegisterList.First();
                    entity.ClientCode = receiptRegister.ClientCode;
                    entity.ClientId = receiptRegister.ClientId;

                    var sql = (from a in idal.IReceiptRegisterDAL.SelectAll()
                               join b in idal.IReceiptRegisterDetailDAL.SelectAll()
                                     on new { a.WhCode, a.ReceiptId }
                                 equals new { b.WhCode, b.ReceiptId } into b_join
                               from b in b_join.DefaultIfEmpty()
                               join c in idal.IInBoundOrderDetailDAL.SelectAll() on new { InOrderDetailId = b.InOrderDetailId } equals new { InOrderDetailId = c.Id } into c_join
                               from c in c_join.DefaultIfEmpty()
                               join d in idal.IInBoundOrderDAL.SelectAll() on new { PoId = c.PoId } equals new { PoId = d.Id } into d_join
                               from d in d_join.DefaultIfEmpty()
                               join e in idal.IInBoundSODAL.SelectAll() on new { SoId = (Int32)d.SoId } equals new { SoId = e.Id } into e_join
                               from e in e_join.DefaultIfEmpty()
                               join f in idal.IItemMasterDAL.SelectAll() on new { ItemId = c.ItemId } equals new { ItemId = f.Id } into f_join
                               from f in f_join.DefaultIfEmpty()
                               where
                                 a.ReceiptId == entity.ReceiptId && a.WhCode == entity.WhCode && soList.Contains(e.SoNumber) && poList.Contains(d.CustomerPoNumber) && itemList.Contains(f.AltItemNumber)
                               select new EditInBoundDetailResult
                               {
                                   ClientCode = a.ClientCode,
                                   ReceiptId = a.ReceiptId,
                                   SoNumber = e.SoNumber,
                                   SoId = e.Id,
                                   PoId = d.Id,
                                   CustomerPoNumber = d.CustomerPoNumber,
                                   ItemId = f.Id,
                                   AltItemNumber = f.AltItemNumber,
                                   Style1 = f.Style1 ?? "",
                                   Style2 = f.Style2 ?? "",
                                   Style3 = f.Style3 ?? "",
                                   Qty = (Int32?)b.RegQty,
                                   UnitName = b.UnitName,
                                   UnitId = b.UnitId,
                                   RegDetailId = b.Id,
                                   OrderType = d.OrderType
                               }).Distinct();

                    if (sql.Where(u => u.AltItemNumber == entity.AltItemNumber).Count() > 0)
                    {
                        return "款号与原款号一致，无需修改！";
                    }
                    //如果是多种单位的货 暂时无法修改
                    string[] unitName1 = (from a in idal.IUnitDefaultDAL.SelectAll()
                                          where a.WhCode == entity.WhCode
                                          select a.UnitName).ToArray();
                    if (sql.Where(u => !unitName1.Contains(u.UnitName)).Count() > 0)
                    {
                        return "款号属于多种定制单位，无法修改！";
                    }

                    List<EditInBoundDetailResult> getList = sql.ToList();

                    //1. 取得原预录入明细--后面修改收货及库存有用
                    int[] getsoid = (from a in getList
                                     select a.SoId).Distinct().ToArray();

                    int[] getpoid = (from a in getList
                                     select a.PoId).Distinct().ToArray();

                    int[] getitemid = (from a in getList
                                       select a.ItemId).Distinct().ToArray();

                    int[] regDetailid = (from a in getList
                                         select a.RegDetailId).Distinct().ToArray();

                    string[] customerPoNumber = (from a in getList
                                                 select a.CustomerPoNumber).Distinct().ToArray();

                    //2. 重建预录入
                    string getclient = "", getSo = "", getPo = "", getSku = "", getStyle1 = "", getStyle2 = "", getStyle3 = "", getQty = "", unitName = "";

                    List<InBoundOrderInsert> entityList = new List<InBoundOrderInsert>();
                    foreach (var item in getList)
                    {
                        getclient = entity.ClientCode;
                        getSo = item.SoNumber;
                        getPo = item.CustomerPoNumber;
                        getSku = entity.AltItemNumber;
                        getStyle1 = item.Style1;
                        getStyle2 = item.Style2;
                        getStyle3 = item.Style3;
                        getQty = item.Qty.ToString();
                        unitName = item.UnitName;

                        if (entityList.Where(u => u.SoNumber == getSo.Trim() && u.ClientCode == getclient.Trim()).Count() == 0)
                        {
                            InBoundOrderInsert entity1 = new InBoundOrderInsert();
                            entity1.WhCode = entity.WhCode;
                            entity1.ClientCode = getclient.Trim();
                            entity1.SoNumber = getSo.Trim();
                            entity1.CreateUser = entity.UserCode;
                            entity1.OrderType = item.OrderType;

                            List<InBoundOrderDetailInsert> orderDetailList = new List<InBoundOrderDetailInsert>();

                            InBoundOrderDetailInsert orderDetail = new InBoundOrderDetailInsert();
                            orderDetail.CustomerPoNumber = getPo.Trim().ToString();
                            orderDetail.AltItemNumber = getSku.Trim().ToString();
                            orderDetail.Style1 = getStyle1.ToString();
                            orderDetail.Style2 = getStyle2.ToString();
                            orderDetail.Style3 = getStyle3.ToString();
                            orderDetail.UnitName = unitName;
                            orderDetail.Qty = Convert.ToInt32(getQty);
                            orderDetail.CreateUser = entity.UserCode;
                            orderDetailList.Add(orderDetail);

                            entity1.InBoundOrderDetailInsert = orderDetailList;
                            entityList.Add(entity1);
                        }
                        else
                        {
                            InBoundOrderInsert oldentity = entityList.Where(u => u.SoNumber == getSo.Trim() && u.ClientCode == getclient.Trim()).First();
                            entityList.Remove(oldentity);

                            InBoundOrderInsert newentity = oldentity;

                            List<InBoundOrderDetailInsert> orderDetailList = oldentity.InBoundOrderDetailInsert.ToList();
                            if (orderDetailList.Where(u => u.CustomerPoNumber == getPo.Trim() && u.AltItemNumber == getSku.Trim() && u.Style1 == getStyle1 && u.Style2 == getStyle2 && u.Style3 == getStyle3).Count() == 0)
                            {
                                InBoundOrderDetailInsert orderDetail = new InBoundOrderDetailInsert();
                                orderDetail.CustomerPoNumber = getPo.Trim().ToString();
                                orderDetail.AltItemNumber = getSku.Trim().ToString();
                                orderDetail.Style1 = getStyle1.Trim().ToString();
                                orderDetail.Style2 = getStyle2.ToString();
                                orderDetail.Style3 = getStyle3.ToString();
                                orderDetail.UnitName = unitName;
                                orderDetail.Qty = Convert.ToInt32(getQty);
                                orderDetail.CreateUser = entity.UserCode;
                                orderDetailList.Add(orderDetail);
                            }
                            else
                            {
                                InBoundOrderDetailInsert oldorderDetail = orderDetailList.Where(u => u.CustomerPoNumber == getPo.Trim() && u.AltItemNumber == getSku.Trim() && u.Style1 == getStyle1.Trim() && u.Style2 == getStyle2 && u.Style3 == getStyle3).First();

                                orderDetailList.Remove(oldorderDetail);
                                InBoundOrderDetailInsert neworderDetail = oldorderDetail;
                                neworderDetail.Qty = oldorderDetail.Qty + Convert.ToInt32(getQty);
                                orderDetailList.Add(neworderDetail);
                            }

                            newentity.InBoundOrderDetailInsert = orderDetailList;
                            entityList.Add(newentity);
                        }
                    }


                    //3.2 创建收货登记
                    string receipt = entity.ReceiptId;

                    string s = AddInBoundRegDetail(entity, entityList, receiptRegister, receipt);
                    if (s != "Y")
                    {
                        return s;
                    }

                    //3.3 删除原收货批次明细
                    idal.IReceiptRegisterDetailDAL.DeleteByExtended(u => u.ReceiptId == entity.ReceiptId && u.WhCode == entity.WhCode && regDetailid.Contains(u.Id));

                    idal.IInBoundOrderDetailDAL.SaveChanges();

                    //3.5 修改收货

                    var sql1 = from a in idal.IReceiptRegisterDAL.SelectAll()
                               join b in idal.IReceiptRegisterDetailDAL.SelectAll()
                                     on new { a.WhCode, a.ReceiptId }
                                 equals new { b.WhCode, b.ReceiptId } into b_join
                               from b in b_join.DefaultIfEmpty()
                               join c in idal.IInBoundOrderDetailDAL.SelectAll() on new { InOrderDetailId = b.InOrderDetailId } equals new { InOrderDetailId = c.Id } into c_join
                               from c in c_join.DefaultIfEmpty()
                               join d in idal.IInBoundOrderDAL.SelectAll() on new { PoId = c.PoId } equals new { PoId = d.Id } into d_join
                               from d in d_join.DefaultIfEmpty()
                               join e in idal.IInBoundSODAL.SelectAll() on new { SoId = (Int32)d.SoId } equals new { SoId = e.Id } into e_join
                               from e in e_join.DefaultIfEmpty()
                               join f in idal.IItemMasterDAL.SelectAll() on new { ItemId = c.ItemId } equals new { ItemId = f.Id } into f_join
                               from f in f_join.DefaultIfEmpty()
                               where
                                 a.ReceiptId == receipt && a.WhCode == entity.WhCode && soList.Contains(e.SoNumber) && poList.Contains(d.CustomerPoNumber) && f.AltItemNumber == entity.AltItemNumber
                               select new EditInBoundDetailResult
                               {
                                   ClientCode = a.ClientCode,
                                   ReceiptId = a.ReceiptId,
                                   SoNumber = e.SoNumber,
                                   SoId = e.Id,
                                   PoId = d.Id,
                                   CustomerPoNumber = d.CustomerPoNumber,
                                   ItemId = f.Id,
                                   AltItemNumber = f.AltItemNumber,
                                   Style1 = f.Style1,
                                   Style2 = f.Style2,
                                   Style3 = f.Style3,
                                   Qty = (Int32?)b.RegQty,
                                   UnitName = b.UnitName,
                                   UnitId = b.UnitId,
                                   RegDetailId = b.Id
                               };

                    List<EditInBoundDetailResult> getNewList = sql1.ToList();

                    var getReceiptList1 = from a in idal.IReceiptDAL.SelectAll()
                                          where a.WhCode == entity.WhCode && getpoid.Contains(a.PoId) && getitemid.Contains(a.ItemId) && a.ReceiptId == entity.ReceiptId
                                          && a.ClientCode == receiptRegister.ClientCode && soList.Contains(a.SoNumber)
                                          select a;
                    List<Receipt> getReceiptList = getReceiptList1.ToList();

                    foreach (var item1 in getReceiptList)
                    {
                        ItemMaster getItemFirst = idal.IItemMasterDAL.SelectBy(u => u.Id == item1.ItemId).First();

                        EditInBoundDetailResult edit = getNewList.Where(u => u.SoNumber == item1.SoNumber && u.CustomerPoNumber == item1.CustomerPoNumber && u.AltItemNumber == entity.AltItemNumber && u.UnitName == item1.UnitName && (u.Style1 ?? "") == (getItemFirst.Style1 ?? "") && (u.Style2 ?? "") == (getItemFirst.Style2 ?? "") && (u.Style3 ?? "") == (getItemFirst.Style3 ?? "")).First();

                        idal.IReceiptDAL.UpdateByExtended(u => u.Id == item1.Id, u => new Receipt() { SoNumber = edit.SoNumber, CustomerPoNumber = edit.CustomerPoNumber, PoId = edit.PoId, AltItemNumber = edit.AltItemNumber, ItemId = edit.ItemId });
                    }

                    //3.6 修改库存

                    var getHudetailList1 = from a in idal.IHuDetailDAL.SelectAll()
                                           where a.WhCode == entity.WhCode && customerPoNumber.Contains(a.CustomerPoNumber) && getitemid.Contains(a.ItemId) && a.ReceiptId == entity.ReceiptId
                                           && a.ClientCode == receiptRegister.ClientCode && soList.Contains(a.SoNumber)
                                           select a;

                    List<HuDetail> getHudetailList = getHudetailList1.ToList();
                    foreach (var item2 in getHudetailList)
                    {
                        ItemMaster getItemFirst = idal.IItemMasterDAL.SelectBy(u => u.Id == item2.ItemId).First();

                        EditInBoundDetailResult edit = getNewList.Where(u => u.SoNumber == item2.SoNumber && u.CustomerPoNumber == item2.CustomerPoNumber && u.AltItemNumber == entity.AltItemNumber && u.UnitName == item2.UnitName && (u.Style1 ?? "") == (getItemFirst.Style1 ?? "") && (u.Style2 ?? "") == (getItemFirst.Style2 ?? "") && (u.Style3 ?? "") == (getItemFirst.Style3 ?? "")).First();

                        idal.IHuDetailDAL.UpdateByExtended(u => u.Id == item2.Id, u => new HuDetail() { SoNumber = edit.SoNumber, CustomerPoNumber = edit.CustomerPoNumber, AltItemNumber = edit.AltItemNumber, ItemId = edit.ItemId });
                    }

                    //3.7 修改收货扫描

                    var getSerialNumberInList1 = from a in idal.ISerialNumberInDAL.SelectAll()
                                                 where a.WhCode == entity.WhCode && customerPoNumber.Contains(a.CustomerPoNumber) && getitemid.Contains((Int32)a.ItemId) && a.ReceiptId == entity.ReceiptId
                                                 && a.ClientCode == receiptRegister.ClientCode && soList.Contains(a.SoNumber)
                                                 select a;

                    List<SerialNumberIn> getSerialNumberInList = getSerialNumberInList1.ToList();
                    foreach (var item2 in getSerialNumberInList)
                    {
                        ItemMaster getItemFirst = idal.IItemMasterDAL.SelectBy(u => u.Id == item2.ItemId).First();

                        EditInBoundDetailResult edit = getNewList.Where(u => u.SoNumber == item2.SoNumber && u.CustomerPoNumber == item2.CustomerPoNumber && u.AltItemNumber == entity.AltItemNumber && (u.Style1 ?? "") == (getItemFirst.Style1 ?? "") && (u.Style2 ?? "") == (getItemFirst.Style2 ?? "") && (u.Style3 ?? "") == (getItemFirst.Style3 ?? "")).First();

                        idal.ISerialNumberInDAL.UpdateByExtended(u => u.Id == item2.Id, u => new SerialNumberIn() { SoNumber = edit.SoNumber, CustomerPoNumber = edit.CustomerPoNumber, PoId = edit.PoId, AltItemNumber = edit.AltItemNumber, ItemId = edit.ItemId });
                    }

                    List<TranLog> tranLogList = new List<TranLog>();
                    for (int i = 0; i < soList.Length; i++)
                    {
                        //插入记录
                        TranLog tl = new TranLog();
                        tl.TranType = "63";
                        tl.Description = "修改Item";
                        tl.TranDate = DateTime.Now;
                        tl.TranUser = entity.UserCode;
                        tl.WhCode = entity.WhCode;
                        tl.SoNumber = soList[i].ToString();
                        tl.CustomerPoNumber = poList[i].ToString();
                        tl.AltItemNumber = itemList[i].ToString();
                        tl.ReceiptId = entity.ReceiptId;
                        tl.ClientCode = receiptRegister.ClientCode;
                        tranLogList.Add(tl);

                        TranLog tl1 = new TranLog();
                        tl1.TranType = "63";
                        tl1.Description = "修改Item";
                        tl1.TranDate = DateTime.Now;
                        tl1.TranUser = entity.UserCode;
                        tl1.WhCode = entity.WhCode;
                        tl1.SoNumber = soList[i].ToString();
                        tl1.CustomerPoNumber = poList[i].ToString();
                        tl1.AltItemNumber = entity.AltItemNumber;
                        tl1.ReceiptId = receipt;
                        tl1.ClientCode = receiptRegister.ClientCode;
                        tranLogList.Add(tl1);
                    }
                    idal.ITranLogDAL.Add(tranLogList);
                    #endregion

                    idal.SaveChanges();
                    trans.Complete();
                    return "Y";
                }
                catch
                {
                    trans.Dispose();//出现异常，事务手动释放
                    return "修改异常，请重新提交！";
                }
            }
        }

        //修改单位
        public string EditDetailUnitName(EditInBoundResult entity, string[] soList, string[] poList, string[] itemList, string[] unitNameList)
        {
            List<ReceiptRegister> list = idal.IReceiptRegisterDAL.SelectBy(u => u.WhCode == entity.WhCode && u.ReceiptId == entity.ReceiptId && u.Status == "C");
            if (list.Count == 0)
            {
                return "请先完成收货后再进行单位修改！";
            }

            using (TransactionScope trans = new TransactionScope())
            {
                try
                {
                    #region 优化
                    List<HuDetail> checkPlanQtyList = idal.IHuDetailDAL.SelectBy(u => u.WhCode == entity.WhCode && u.ReceiptId == entity.ReceiptId && (u.PlanQty ?? 0) != 0 && soList.Contains(u.SoNumber) && poList.Contains(u.CustomerPoNumber) && itemList.Contains(u.AltItemNumber) && unitNameList.Contains(u.UnitName));
                    if (checkPlanQtyList.Count > 0)
                    {
                        return "款号对应单位已做备货单无法修改，请撤销释放后修改！";
                    }

                    string result = "";
                    List<TranLog> tranLogList = new List<TranLog>();
                    for (int i = 0; i < soList.Length; i++)
                    {
                        string so = soList[i].ToString();
                        string po = poList[i].ToString();
                        string item = itemList[i].ToString();
                        string unitname = unitNameList[i].ToString();

                        List<Receipt> receiptList = idal.IReceiptDAL.SelectBy(u => u.WhCode == entity.WhCode && u.ReceiptId == entity.ReceiptId && u.SoNumber == so && u.CustomerPoNumber == po && u.AltItemNumber == item && u.UnitName == unitname);
                        if (receiptList.Count > 0)
                        {
                            foreach (var item1 in receiptList)
                            {
                                if (unitname == entity.UnitName)
                                {
                                    result = "单位与原单位一致，无需修改！";
                                    break;
                                }
                                HuDetail hudetail = new HuDetail();
                                hudetail.UnitName = entity.UnitName;
                                idal.IHuDetailDAL.UpdateBy(hudetail, u => u.HuId == item1.HuId && u.WhCode == entity.WhCode && u.ClientCode == item1.ClientCode && u.SoNumber == item1.SoNumber && u.CustomerPoNumber == item1.CustomerPoNumber && u.AltItemNumber == item1.AltItemNumber && u.UnitName == item1.UnitName, new string[] { "UnitName" });

                                Receipt rec = new Receipt();
                                rec.UnitName = entity.UnitName;
                                idal.IReceiptDAL.UpdateBy(rec, u => u.Id == item1.Id, new string[] { "UnitName" });

                                if (entity.UnitName.Contains("ECH"))
                                {
                                    WorkloadAccount work = new WorkloadAccount();
                                    work.EchFlag = 1;
                                    idal.IWorkloadAccountDAL.UpdateBy(work, u => u.ReceiptId == item1.ReceiptId && u.WhCode == item1.WhCode && u.HuId == item1.HuId, new string[] { "EchFlag" });
                                }
                                else
                                {
                                    WorkloadAccount work = new WorkloadAccount();
                                    work.EchFlag = 0;
                                    idal.IWorkloadAccountDAL.UpdateBy(work, u => u.ReceiptId == item1.ReceiptId && u.WhCode == item1.WhCode && u.HuId == item1.HuId, new string[] { "EchFlag" });
                                }

                                //插入记录
                                TranLog tl = new TranLog();
                                tl.TranType = "64";
                                tl.Description = "修改单位";
                                tl.TranDate = DateTime.Now;
                                tl.TranUser = entity.UserCode;
                                tl.WhCode = entity.WhCode;
                                tl.HuId = item1.HuId;
                                tl.SoNumber = so;
                                tl.CustomerPoNumber = po;
                                tl.AltItemNumber = item;
                                tl.UnitName = unitname;
                                tl.ReceiptId = entity.ReceiptId;
                                tl.ClientCode = entity.ClientCode;
                                tl.Remark = "单位修改为：" + entity.UnitName;
                                tranLogList.Add(tl);
                            }
                        }
                        if (result != "")
                        {
                            break;
                        }
                    }
                    if (result != "")
                    {
                        return result;
                    }

                    idal.ITranLogDAL.Add(tranLogList);
                    #endregion

                    idal.SaveChanges();
                    trans.Complete();
                    return "Y";
                }
                catch
                {
                    trans.Dispose();//出现异常，事务手动释放
                    return "修改异常，请重新提交！";
                }
            }
        }


        //修改SO By托盘
        public string EditDetailSoNumberByHu(EditInBoundResult entity, string[] soList, string[] poList, string[] huList)
        {
            using (TransactionScope trans = new TransactionScope())
            {
                try
                {
                    #region 优化
                    List<ReceiptRegister> ReceiptRegisterList = idal.IReceiptRegisterDAL.SelectBy(u => u.WhCode == entity.WhCode && u.ReceiptId == entity.ReceiptId);
                    if (ReceiptRegisterList.Count == 0)
                    {
                        return "未找到明细，无法修改！";
                    }

                    List<HuDetail> checkPlanQtyList = idal.IHuDetailDAL.SelectBy(u => u.WhCode == entity.WhCode && u.ReceiptId == entity.ReceiptId && (u.PlanQty ?? 0) != 0 && soList.Contains(u.SoNumber) && poList.Contains(u.CustomerPoNumber) && huList.Contains(u.HuId));
                    if (checkPlanQtyList.Count > 0)
                    {
                        return "SOPO托盘已做备货单无法修改，请撤销释放后修改！";
                    }

                    ReceiptRegister receiptRegister = ReceiptRegisterList.First();
                    entity.ClientCode = receiptRegister.ClientCode;
                    entity.ClientId = receiptRegister.ClientId;

                    var sql = (from a in idal.IReceiptRegisterDAL.SelectAll()
                               join b in idal.IReceiptRegisterDetailDAL.SelectAll()
                                     on new { a.WhCode, a.ReceiptId }
                                 equals new { b.WhCode, b.ReceiptId } into b_join
                               from b in b_join.DefaultIfEmpty()
                               join c in idal.IInBoundOrderDetailDAL.SelectAll() on new { InOrderDetailId = b.InOrderDetailId } equals new { InOrderDetailId = c.Id } into c_join
                               from c in c_join.DefaultIfEmpty()
                               join d in idal.IInBoundOrderDAL.SelectAll() on new { PoId = c.PoId } equals new { PoId = d.Id } into d_join
                               from d in d_join.DefaultIfEmpty()
                               join e in idal.IInBoundSODAL.SelectAll() on new { SoId = (Int32)d.SoId } equals new { SoId = e.Id } into e_join
                               from e in e_join.DefaultIfEmpty()
                               join f in idal.IItemMasterDAL.SelectAll() on new { ItemId = c.ItemId } equals new { ItemId = f.Id } into f_join
                               from f in f_join.DefaultIfEmpty()
                               where
                                 a.ReceiptId == entity.ReceiptId && a.WhCode == entity.WhCode && soList.Contains(e.SoNumber) && poList.Contains(d.CustomerPoNumber)
                               select new EditInBoundDetailResult
                               {
                                   ClientCode = a.ClientCode,
                                   ReceiptId = a.ReceiptId,
                                   SoNumber = e.SoNumber,
                                   SoId = e.Id,
                                   PoId = d.Id,
                                   CustomerPoNumber = d.CustomerPoNumber,
                                   ItemId = f.Id,
                                   AltItemNumber = f.AltItemNumber,
                                   Style1 = f.Style1 ?? "",
                                   Style2 = f.Style2 ?? "",
                                   Style3 = f.Style3 ?? "",
                                   Qty = (Int32?)b.RegQty,
                                   UnitName = b.UnitName,
                                   UnitId = b.UnitId,
                                   RegDetailId = b.Id,
                                   OrderType = d.OrderType
                               }).Distinct();

                    if (sql.Where(u => u.SoNumber == entity.SoNumber).Count() > 0)
                    {
                        return "SO号与原SO一致，无需修改！";
                    }

                    //如果是多种单位的货 暂时无法修改
                    string[] unitName1 = (from a in idal.IUnitDefaultDAL.SelectAll()
                                          where a.WhCode == entity.WhCode
                                          select a.UnitName).ToArray();
                    if (sql.Where(u => !unitName1.Contains(u.UnitName)).Count() > 0)
                    {
                        return "款号属于多种定制单位，无法修改！";
                    }

                    List<EditInBoundDetailResult> getList = sql.ToList();

                    //1. 取得原预录入明细--后面修改收货及库存有用
                    int[] getsoid = (from a in getList
                                     select a.SoId).Distinct().ToArray();

                    int[] getpoid = (from a in getList
                                     select a.PoId).Distinct().ToArray();

                    int[] getitemid = (from a in getList
                                       select a.ItemId).Distinct().ToArray();

                    int[] regDetailid = (from a in getList
                                         select a.RegDetailId).Distinct().ToArray();

                    string[] SoNumberArr = (from a in getList
                                            select a.SoNumber).Distinct().ToArray();

                    //2. 重建预录入
                    string getclient = "", getSo = "", getPo = "", getSku = "", getStyle1 = "", getStyle2 = "", getStyle3 = "", getQty = "", unitName = "";

                    //2.1 得到需调整的总件数
                    List<Receipt> getOldReceiptList = idal.IReceiptDAL.SelectBy(u => u.WhCode == entity.WhCode && u.ReceiptId == entity.ReceiptId && soList.Contains(u.SoNumber) && poList.Contains(u.CustomerPoNumber) && huList.Contains(u.HuId));
                    string[] getunitNamecheck = (from a in getOldReceiptList
                                                 select a.UnitName).Distinct().ToArray();

                    if (getunitNamecheck.Count() > 1)
                    {
                        return "该托盘存在多种收货单位，请先调整为一种单位！";
                    }

                    //2.2 配置预录入
                    List<InBoundOrderInsert> entityList = new List<InBoundOrderInsert>();
                    foreach (var item in getList)
                    {
                        int getRecQty = getOldReceiptList.Where(u => u.SoNumber == item.SoNumber && u.CustomerPoNumber == item.CustomerPoNumber && u.AltItemNumber == item.AltItemNumber).Sum(u => u.Qty);

                        getclient = entity.ClientCode;
                        getSo = entity.SoNumber;
                        getPo = item.CustomerPoNumber;
                        getSku = item.AltItemNumber;
                        getStyle1 = item.Style1;
                        getStyle2 = item.Style2;
                        getStyle3 = item.Style3;
                        getQty = getRecQty.ToString();
                        unitName = item.UnitName;

                        if (entityList.Where(u => u.SoNumber == getSo.Trim() && u.ClientCode == getclient.Trim()).Count() == 0)
                        {
                            InBoundOrderInsert entity1 = new InBoundOrderInsert();
                            entity1.WhCode = entity.WhCode;
                            entity1.ClientCode = getclient.Trim();
                            entity1.SoNumber = getSo.Trim();
                            entity1.CreateUser = entity.UserCode;
                            entity1.OrderType = item.OrderType;

                            List<InBoundOrderDetailInsert> orderDetailList = new List<InBoundOrderDetailInsert>();

                            InBoundOrderDetailInsert orderDetail = new InBoundOrderDetailInsert();
                            orderDetail.CustomerPoNumber = getPo.Trim().ToString();
                            orderDetail.AltItemNumber = getSku.Trim().ToString();
                            orderDetail.Style1 = getStyle1.ToString();
                            orderDetail.Style2 = getStyle2.ToString();
                            orderDetail.Style3 = getStyle3.ToString();
                            orderDetail.UnitName = unitName;
                            orderDetail.Qty = Convert.ToInt32(getQty);
                            orderDetail.CreateUser = entity.UserCode;
                            orderDetailList.Add(orderDetail);

                            entity1.InBoundOrderDetailInsert = orderDetailList;
                            entityList.Add(entity1);
                        }
                        else
                        {
                            InBoundOrderInsert oldentity = entityList.Where(u => u.SoNumber == getSo.Trim() && u.ClientCode == getclient.Trim()).First();
                            entityList.Remove(oldentity);

                            InBoundOrderInsert newentity = oldentity;

                            List<InBoundOrderDetailInsert> orderDetailList = oldentity.InBoundOrderDetailInsert.ToList();
                            if (orderDetailList.Where(u => u.CustomerPoNumber == getPo.Trim() && u.AltItemNumber == getSku.Trim() && u.Style1 == getStyle1 && u.Style2 == getStyle2 && u.Style3 == getStyle3).Count() == 0)
                            {
                                InBoundOrderDetailInsert orderDetail = new InBoundOrderDetailInsert();
                                orderDetail.CustomerPoNumber = getPo.Trim().ToString();
                                orderDetail.AltItemNumber = getSku.Trim().ToString();
                                orderDetail.Style1 = getStyle1.Trim().ToString();
                                orderDetail.Style2 = getStyle2.ToString();
                                orderDetail.Style3 = getStyle3.ToString();
                                orderDetail.UnitName = unitName;
                                orderDetail.Qty = Convert.ToInt32(getQty);
                                orderDetail.CreateUser = entity.UserCode;
                                orderDetailList.Add(orderDetail);
                            }
                            else
                            {
                                InBoundOrderDetailInsert oldorderDetail = orderDetailList.Where(u => u.CustomerPoNumber == getPo.Trim() && u.AltItemNumber == getSku.Trim() && u.Style1 == getStyle1.Trim() && u.Style2 == getStyle2 && u.Style3 == getStyle3).First();

                                orderDetailList.Remove(oldorderDetail);
                                InBoundOrderDetailInsert neworderDetail = oldorderDetail;
                                neworderDetail.Qty = oldorderDetail.Qty + Convert.ToInt32(getQty);
                                orderDetailList.Add(neworderDetail);
                            }

                            newentity.InBoundOrderDetailInsert = orderDetailList;
                            entityList.Add(newentity);
                        }
                    }

                    string result = "";
                    //3.1 修改原收货批次明细
                    foreach (var item in getOldReceiptList)
                    {
                        List<ReceiptRegisterDetail> regDetailList = idal.IReceiptRegisterDetailDAL.SelectBy(u => u.WhCode == entity.WhCode && u.ReceiptId == entity.ReceiptId && u.PoId == item.PoId && u.ItemId == item.ItemId);
                        if (regDetailList.Count > 0)
                        {
                            ReceiptRegisterDetail regDetail = regDetailList.First();
                            regDetail.RegQty = regDetail.RegQty - item.Qty;
                            idal.IReceiptRegisterDetailDAL.UpdateBy(regDetail, u => u.Id == regDetail.Id, new string[] { "RegQty" });
                        }
                        else
                        {
                            result = "收货登记明细异常，无法修改！";
                            break;
                        }
                    }
                    if (result != "")
                    {
                        return result;
                    }

                    //3.2 创建收货登记
                    string receipt = entity.ReceiptId;

                    string s = AddInBoundRegDetail(entity, entityList, receiptRegister, receipt);
                    if (s != "Y")
                    {
                        return s;
                    }

                    idal.IInBoundOrderDetailDAL.SaveChanges();

                    //3.3 删除原收货登记下 数量为0的数据
                    idal.IReceiptRegisterDetailDAL.DeleteByExtended(u => u.ReceiptId == entity.ReceiptId && u.WhCode == entity.WhCode && u.RegQty == 0);

                    //3.5 修改收货

                    var sql1 = from a in idal.IReceiptRegisterDAL.SelectAll()
                               join b in idal.IReceiptRegisterDetailDAL.SelectAll()
                                     on new { a.WhCode, a.ReceiptId }
                                 equals new { b.WhCode, b.ReceiptId } into b_join
                               from b in b_join.DefaultIfEmpty()
                               join c in idal.IInBoundOrderDetailDAL.SelectAll() on new { InOrderDetailId = b.InOrderDetailId } equals new { InOrderDetailId = c.Id } into c_join
                               from c in c_join.DefaultIfEmpty()
                               join d in idal.IInBoundOrderDAL.SelectAll() on new { PoId = c.PoId } equals new { PoId = d.Id } into d_join
                               from d in d_join.DefaultIfEmpty()
                               join e in idal.IInBoundSODAL.SelectAll() on new { SoId = (Int32)d.SoId } equals new { SoId = e.Id } into e_join
                               from e in e_join.DefaultIfEmpty()
                               join f in idal.IItemMasterDAL.SelectAll() on new { ItemId = c.ItemId } equals new { ItemId = f.Id } into f_join
                               from f in f_join.DefaultIfEmpty()
                               where
                                 a.ReceiptId == receipt && a.WhCode == entity.WhCode && e.SoNumber == entity.SoNumber && poList.Contains(d.CustomerPoNumber)
                               select new EditInBoundDetailResult
                               {
                                   ClientCode = a.ClientCode,
                                   ReceiptId = a.ReceiptId,
                                   SoNumber = e.SoNumber,
                                   SoId = e.Id,
                                   PoId = d.Id,
                                   CustomerPoNumber = d.CustomerPoNumber,
                                   ItemId = f.Id,
                                   AltItemNumber = f.AltItemNumber,
                                   Style1 = f.Style1,
                                   Style2 = f.Style2,
                                   Style3 = f.Style3,
                                   Qty = (Int32?)b.RegQty,
                                   UnitName = b.UnitName,
                                   UnitId = b.UnitId,
                                   RegDetailId = b.Id
                               };
                    //得到新PO的预录入信息
                    List<EditInBoundDetailResult> getNewList = sql1.ToList();

                    var getReceiptList1 = from a in idal.IReceiptDAL.SelectAll()
                                          where a.WhCode == entity.WhCode && getpoid.Contains(a.PoId) && getitemid.Contains(a.ItemId) && a.ReceiptId == entity.ReceiptId
                                          && a.ClientCode == receiptRegister.ClientCode && soList.Contains(a.SoNumber) && huList.Contains(a.HuId)
                                          select a;
                    List<Receipt> getReceiptList = getReceiptList1.ToList();

                    if (getReceiptList.Count > 0)
                    {
                        foreach (var item1 in getReceiptList)
                        {
                            EditInBoundDetailResult edit = getNewList.Where(u => u.SoNumber == entity.SoNumber && u.CustomerPoNumber == item1.CustomerPoNumber && u.AltItemNumber == item1.AltItemNumber && u.UnitName == item1.UnitName && u.ItemId == item1.ItemId).First();

                            idal.IReceiptDAL.UpdateByExtended(u => u.Id == item1.Id, u => new Receipt() { SoNumber = edit.SoNumber, CustomerPoNumber = edit.CustomerPoNumber, PoId = edit.PoId, AltItemNumber = edit.AltItemNumber, ItemId = edit.ItemId });
                        }
                    }


                    //3.6 修改库存

                    var getHudetailList1 = from a in idal.IHuDetailDAL.SelectAll()
                                           where a.WhCode == entity.WhCode && poList.Contains(a.CustomerPoNumber) && getitemid.Contains(a.ItemId) && a.ReceiptId == entity.ReceiptId
                                           && a.ClientCode == receiptRegister.ClientCode && SoNumberArr.Contains(a.SoNumber) && huList.Contains(a.HuId)
                                           select a;

                    List<HuDetail> getHudetailList = getHudetailList1.ToList();
                    if (getHudetailList.Count > 0)
                    {
                        foreach (var item2 in getHudetailList)
                        {
                            EditInBoundDetailResult edit = getNewList.Where(u => u.SoNumber == entity.SoNumber && u.CustomerPoNumber == item2.CustomerPoNumber && u.AltItemNumber == item2.AltItemNumber && u.UnitName == item2.UnitName && u.ItemId == item2.ItemId).First();

                            idal.IHuDetailDAL.UpdateByExtended(u => u.Id == item2.Id, u => new HuDetail() { SoNumber = edit.SoNumber, CustomerPoNumber = edit.CustomerPoNumber, AltItemNumber = edit.AltItemNumber, ItemId = edit.ItemId });
                        }
                    }


                    //3.7 修改收货扫描

                    var getSerialNumberInList1 = from a in idal.ISerialNumberInDAL.SelectAll()
                                                 where a.WhCode == entity.WhCode && poList.Contains(a.CustomerPoNumber) && getitemid.Contains((Int32)a.ItemId) && a.ReceiptId == entity.ReceiptId
                                                 && a.ClientCode == receiptRegister.ClientCode && soList.Contains(a.SoNumber) && huList.Contains(a.HuId)
                                                 select a;

                    List<SerialNumberIn> getSerialNumberInList = getSerialNumberInList1.ToList();
                    if (getSerialNumberInList.Count > 0)
                    {
                        foreach (var item2 in getSerialNumberInList)
                        {
                            EditInBoundDetailResult edit = getNewList.Where(u => u.SoNumber == entity.SoNumber && u.CustomerPoNumber == item2.CustomerPoNumber && u.AltItemNumber == item2.AltItemNumber && u.ItemId == item2.ItemId).First();

                            idal.ISerialNumberInDAL.UpdateByExtended(u => u.Id == item2.Id, u => new SerialNumberIn() { SoNumber = edit.SoNumber, CustomerPoNumber = edit.CustomerPoNumber, PoId = edit.PoId, AltItemNumber = edit.AltItemNumber, ItemId = edit.ItemId });
                        }
                    }


                    List<TranLog> tranLogList = new List<TranLog>();
                    for (int i = 0; i < soList.Length; i++)
                    {
                        //插入记录
                        TranLog tl = new TranLog();
                        tl.TranType = "69";
                        tl.Description = "修改SO按托盘";
                        tl.TranDate = DateTime.Now;
                        tl.TranUser = entity.UserCode;
                        tl.WhCode = entity.WhCode;
                        tl.SoNumber = soList[i].ToString();
                        tl.CustomerPoNumber = poList[i].ToString();
                        tl.ReceiptId = entity.ReceiptId;
                        tl.ClientCode = receiptRegister.ClientCode;
                        tranLogList.Add(tl);

                        TranLog tl1 = new TranLog();
                        tl1.TranType = "69";
                        tl1.Description = "修改SO按托盘";
                        tl1.TranDate = DateTime.Now;
                        tl1.TranUser = entity.UserCode;
                        tl1.WhCode = entity.WhCode;
                        tl1.SoNumber = entity.SoNumber;
                        tl1.CustomerPoNumber = poList[i].ToString();
                        tl1.ReceiptId = receipt;
                        tl1.ClientCode = receiptRegister.ClientCode;
                        tranLogList.Add(tl1);
                    }
                    idal.ITranLogDAL.Add(tranLogList);
                    #endregion

                    idal.SaveChanges();
                    trans.Complete();
                    return "Y";
                }
                catch
                {
                    trans.Dispose();//出现异常，事务手动释放
                    return "修改异常，请重新提交！";
                }
            }
        }


        //修改PO By托盘
        public string EditDetailCustomerPoNumberByHu(EditInBoundResult entity, string[] soList, string[] poList, string[] huList)
        {
            using (TransactionScope trans = new TransactionScope())
            {
                try
                {
                    #region 优化
                    List<ReceiptRegister> ReceiptRegisterList = idal.IReceiptRegisterDAL.SelectBy(u => u.WhCode == entity.WhCode && u.ReceiptId == entity.ReceiptId);
                    if (ReceiptRegisterList.Count == 0)
                    {
                        return "未找到明细，无法修改！";
                    }

                    List<HuDetail> checkPlanQtyList = idal.IHuDetailDAL.SelectBy(u => u.WhCode == entity.WhCode && u.ReceiptId == entity.ReceiptId && (u.PlanQty ?? 0) != 0 && soList.Contains(u.SoNumber) && poList.Contains(u.CustomerPoNumber) && huList.Contains(u.HuId));
                    if (checkPlanQtyList.Count > 0)
                    {
                        return "SOPO托盘已做备货单无法修改，请撤销释放后修改！";
                    }

                    ReceiptRegister receiptRegister = ReceiptRegisterList.First();
                    entity.ClientCode = receiptRegister.ClientCode;
                    entity.ClientId = receiptRegister.ClientId;

                    var sql = (from a in idal.IReceiptRegisterDAL.SelectAll()
                               join b in idal.IReceiptRegisterDetailDAL.SelectAll()
                                     on new { a.WhCode, a.ReceiptId }
                                 equals new { b.WhCode, b.ReceiptId } into b_join
                               from b in b_join.DefaultIfEmpty()
                               join c in idal.IInBoundOrderDetailDAL.SelectAll() on new { InOrderDetailId = b.InOrderDetailId } equals new { InOrderDetailId = c.Id } into c_join
                               from c in c_join.DefaultIfEmpty()
                               join d in idal.IInBoundOrderDAL.SelectAll() on new { PoId = c.PoId } equals new { PoId = d.Id } into d_join
                               from d in d_join.DefaultIfEmpty()
                               join e in idal.IInBoundSODAL.SelectAll() on new { SoId = (Int32)d.SoId } equals new { SoId = e.Id } into e_join
                               from e in e_join.DefaultIfEmpty()
                               join f in idal.IItemMasterDAL.SelectAll() on new { ItemId = c.ItemId } equals new { ItemId = f.Id } into f_join
                               from f in f_join.DefaultIfEmpty()
                               where
                                 a.ReceiptId == entity.ReceiptId && a.WhCode == entity.WhCode && soList.Contains(e.SoNumber) && poList.Contains(d.CustomerPoNumber)
                               select new EditInBoundDetailResult
                               {
                                   ClientCode = a.ClientCode,
                                   ReceiptId = a.ReceiptId,
                                   SoNumber = e.SoNumber,
                                   SoId = e.Id,
                                   PoId = d.Id,
                                   CustomerPoNumber = d.CustomerPoNumber,
                                   ItemId = f.Id,
                                   AltItemNumber = f.AltItemNumber,
                                   Style1 = f.Style1 ?? "",
                                   Style2 = f.Style2 ?? "",
                                   Style3 = f.Style3 ?? "",
                                   Qty = (Int32?)b.RegQty,
                                   UnitName = b.UnitName,
                                   UnitId = b.UnitId,
                                   RegDetailId = b.Id,
                                   OrderType = d.OrderType
                               }).Distinct();

                    if (sql.Where(u => u.CustomerPoNumber == entity.CustomerPoNumber).Count() > 0)
                    {
                        return "PO号与原PO一致，无需修改！";
                    }

                    //如果是多种单位的货 暂时无法修改
                    string[] unitName1 = (from a in idal.IUnitDefaultDAL.SelectAll()
                                          where a.WhCode == entity.WhCode
                                          select a.UnitName).ToArray();
                    if (sql.Where(u => !unitName1.Contains(u.UnitName)).Count() > 0)
                    {
                        return "款号属于多种定制单位，无法修改！";
                    }

                    List<EditInBoundDetailResult> getList = sql.ToList();

                    //1. 取得原预录入明细--后面修改收货及库存有用
                    int[] getsoid = (from a in getList
                                     select a.SoId).Distinct().ToArray();

                    int[] getpoid = (from a in getList
                                     select a.PoId).Distinct().ToArray();

                    int[] getitemid = (from a in getList
                                       select a.ItemId).Distinct().ToArray();

                    int[] regDetailid = (from a in getList
                                         select a.RegDetailId).Distinct().ToArray();

                    string[] customerPoNumber = (from a in getList
                                                 select a.CustomerPoNumber).Distinct().ToArray();

                    //2. 重建预录入
                    string getclient = "", getSo = "", getPo = "", getSku = "", getStyle1 = "", getStyle2 = "", getStyle3 = "", getQty = "", unitName = "";

                    //2.1 得到需调整的总件数
                    List<Receipt> getOldReceiptList = idal.IReceiptDAL.SelectBy(u => u.WhCode == entity.WhCode && u.ReceiptId == entity.ReceiptId && soList.Contains(u.SoNumber) && poList.Contains(u.CustomerPoNumber) && huList.Contains(u.HuId));
                    string[] getunitNamecheck = (from a in getOldReceiptList
                                                 select a.UnitName).Distinct().ToArray();

                    if (getunitNamecheck.Count() > 1)
                    {
                        return "该托盘存在多种收货单位，请先调整为一种单位！";
                    }

                    //2.2 配置预录入
                    List<InBoundOrderInsert> entityList = new List<InBoundOrderInsert>();
                    foreach (var item in getList)
                    {
                        int getRecQty = getOldReceiptList.Where(u => u.SoNumber == item.SoNumber && u.CustomerPoNumber == item.CustomerPoNumber && u.AltItemNumber == item.AltItemNumber).Sum(u => u.Qty);

                        getclient = entity.ClientCode;
                        getSo = item.SoNumber;
                        getPo = entity.CustomerPoNumber;
                        getSku = item.AltItemNumber;
                        getStyle1 = item.Style1;
                        getStyle2 = item.Style2;
                        getStyle3 = item.Style3;
                        getQty = getRecQty.ToString();
                        unitName = item.UnitName;

                        if (entityList.Where(u => u.SoNumber == getSo.Trim() && u.ClientCode == getclient.Trim()).Count() == 0)
                        {
                            InBoundOrderInsert entity1 = new InBoundOrderInsert();
                            entity1.WhCode = entity.WhCode;
                            entity1.ClientCode = getclient.Trim();
                            entity1.SoNumber = getSo.Trim();
                            entity1.CreateUser = entity.UserCode;
                            entity1.OrderType = item.OrderType;

                            List<InBoundOrderDetailInsert> orderDetailList = new List<InBoundOrderDetailInsert>();

                            InBoundOrderDetailInsert orderDetail = new InBoundOrderDetailInsert();
                            orderDetail.CustomerPoNumber = getPo.Trim().ToString();
                            orderDetail.AltItemNumber = getSku.Trim().ToString();
                            orderDetail.Style1 = getStyle1.ToString();
                            orderDetail.Style2 = getStyle2.ToString();
                            orderDetail.Style3 = getStyle3.ToString();
                            orderDetail.UnitName = unitName;
                            orderDetail.Qty = Convert.ToInt32(getQty);
                            orderDetail.CreateUser = entity.UserCode;
                            orderDetailList.Add(orderDetail);

                            entity1.InBoundOrderDetailInsert = orderDetailList;
                            entityList.Add(entity1);
                        }
                        else
                        {
                            InBoundOrderInsert oldentity = entityList.Where(u => u.SoNumber == getSo.Trim() && u.ClientCode == getclient.Trim()).First();
                            entityList.Remove(oldentity);

                            InBoundOrderInsert newentity = oldentity;

                            List<InBoundOrderDetailInsert> orderDetailList = oldentity.InBoundOrderDetailInsert.ToList();
                            if (orderDetailList.Where(u => u.CustomerPoNumber == getPo.Trim() && u.AltItemNumber == getSku.Trim() && u.Style1 == getStyle1 && u.Style2 == getStyle2 && u.Style3 == getStyle3).Count() == 0)
                            {
                                InBoundOrderDetailInsert orderDetail = new InBoundOrderDetailInsert();
                                orderDetail.CustomerPoNumber = getPo.Trim().ToString();
                                orderDetail.AltItemNumber = getSku.Trim().ToString();
                                orderDetail.Style1 = getStyle1.Trim().ToString();
                                orderDetail.Style2 = getStyle2.ToString();
                                orderDetail.Style3 = getStyle3.ToString();
                                orderDetail.UnitName = unitName;
                                orderDetail.Qty = Convert.ToInt32(getQty);
                                orderDetail.CreateUser = entity.UserCode;
                                orderDetailList.Add(orderDetail);
                            }
                            else
                            {
                                InBoundOrderDetailInsert oldorderDetail = orderDetailList.Where(u => u.CustomerPoNumber == getPo.Trim() && u.AltItemNumber == getSku.Trim() && u.Style1 == getStyle1.Trim() && u.Style2 == getStyle2 && u.Style3 == getStyle3).First();

                                orderDetailList.Remove(oldorderDetail);
                                InBoundOrderDetailInsert neworderDetail = oldorderDetail;
                                neworderDetail.Qty = oldorderDetail.Qty + Convert.ToInt32(getQty);
                                orderDetailList.Add(neworderDetail);
                            }

                            newentity.InBoundOrderDetailInsert = orderDetailList;
                            entityList.Add(newentity);
                        }
                    }

                    string result = "";
                    //3.1 修改原收货批次明细
                    foreach (var item in getOldReceiptList)
                    {
                        List<ReceiptRegisterDetail> regDetailList = idal.IReceiptRegisterDetailDAL.SelectBy(u => u.WhCode == entity.WhCode && u.ReceiptId == entity.ReceiptId && u.PoId == item.PoId && u.ItemId == item.ItemId);
                        if (regDetailList.Count > 0)
                        {
                            ReceiptRegisterDetail regDetail = regDetailList.First();
                            regDetail.RegQty = regDetail.RegQty - item.Qty;
                            idal.IReceiptRegisterDetailDAL.UpdateBy(regDetail, u => u.Id == regDetail.Id, new string[] { "RegQty" });
                        }
                        else
                        {
                            result = "收货登记明细异常，无法修改！";
                            break;
                        }
                    }
                    if (result != "")
                    {
                        return result;
                    }

                    //3.2 创建收货登记
                    string receipt = entity.ReceiptId;

                    string s = AddInBoundRegDetail(entity, entityList, receiptRegister, receipt);
                    if (s != "Y")
                    {
                        return s;
                    }

                    idal.IInBoundOrderDetailDAL.SaveChanges();

                    //3.3 删除原收货登记下 数量为0的数据
                    idal.IReceiptRegisterDetailDAL.DeleteByExtended(u => u.ReceiptId == entity.ReceiptId && u.WhCode == entity.WhCode && u.RegQty == 0);

                    //3.5 修改收货

                    var sql1 = from a in idal.IReceiptRegisterDAL.SelectAll()
                               join b in idal.IReceiptRegisterDetailDAL.SelectAll()
                                     on new { a.WhCode, a.ReceiptId }
                                 equals new { b.WhCode, b.ReceiptId } into b_join
                               from b in b_join.DefaultIfEmpty()
                               join c in idal.IInBoundOrderDetailDAL.SelectAll() on new { InOrderDetailId = b.InOrderDetailId } equals new { InOrderDetailId = c.Id } into c_join
                               from c in c_join.DefaultIfEmpty()
                               join d in idal.IInBoundOrderDAL.SelectAll() on new { PoId = c.PoId } equals new { PoId = d.Id } into d_join
                               from d in d_join.DefaultIfEmpty()
                               join e in idal.IInBoundSODAL.SelectAll() on new { SoId = (Int32)d.SoId } equals new { SoId = e.Id } into e_join
                               from e in e_join.DefaultIfEmpty()
                               join f in idal.IItemMasterDAL.SelectAll() on new { ItemId = c.ItemId } equals new { ItemId = f.Id } into f_join
                               from f in f_join.DefaultIfEmpty()
                               where
                                 a.ReceiptId == receipt && a.WhCode == entity.WhCode && soList.Contains(e.SoNumber) && d.CustomerPoNumber == entity.CustomerPoNumber
                               select new EditInBoundDetailResult
                               {
                                   ClientCode = a.ClientCode,
                                   ReceiptId = a.ReceiptId,
                                   SoNumber = e.SoNumber,
                                   SoId = e.Id,
                                   PoId = d.Id,
                                   CustomerPoNumber = d.CustomerPoNumber,
                                   ItemId = f.Id,
                                   AltItemNumber = f.AltItemNumber,
                                   Style1 = f.Style1,
                                   Style2 = f.Style2,
                                   Style3 = f.Style3,
                                   Qty = (Int32?)b.RegQty,
                                   UnitName = b.UnitName,
                                   UnitId = b.UnitId,
                                   RegDetailId = b.Id
                               };
                    //得到新PO的预录入信息
                    List<EditInBoundDetailResult> getNewList = sql1.ToList();

                    var getReceiptList1 = from a in idal.IReceiptDAL.SelectAll()
                                          where a.WhCode == entity.WhCode && getpoid.Contains(a.PoId) && getitemid.Contains(a.ItemId) && a.ReceiptId == entity.ReceiptId
                                          && a.ClientCode == receiptRegister.ClientCode && soList.Contains(a.SoNumber) && huList.Contains(a.HuId)
                                          select a;
                    List<Receipt> getReceiptList = getReceiptList1.ToList();

                    foreach (var item1 in getReceiptList)
                    {
                        EditInBoundDetailResult edit = getNewList.Where(u => u.SoNumber == item1.SoNumber && u.CustomerPoNumber == entity.CustomerPoNumber && u.AltItemNumber == item1.AltItemNumber && u.UnitName == item1.UnitName && u.ItemId == item1.ItemId).First();

                        idal.IReceiptDAL.UpdateByExtended(u => u.Id == item1.Id, u => new Receipt() { SoNumber = edit.SoNumber, CustomerPoNumber = edit.CustomerPoNumber, PoId = edit.PoId, AltItemNumber = edit.AltItemNumber, ItemId = edit.ItemId });
                    }

                    //3.6 修改库存

                    var getHudetailList1 = from a in idal.IHuDetailDAL.SelectAll()
                                           where a.WhCode == entity.WhCode && customerPoNumber.Contains(a.CustomerPoNumber) && getitemid.Contains(a.ItemId) && a.ReceiptId == entity.ReceiptId
                                           && a.ClientCode == receiptRegister.ClientCode && soList.Contains(a.SoNumber) && huList.Contains(a.HuId)
                                           select a;

                    List<HuDetail> getHudetailList = getHudetailList1.ToList();
                    foreach (var item2 in getHudetailList)
                    {
                        EditInBoundDetailResult edit = getNewList.Where(u => u.SoNumber == item2.SoNumber && u.CustomerPoNumber == entity.CustomerPoNumber && u.AltItemNumber == item2.AltItemNumber && u.UnitName == item2.UnitName && u.ItemId == item2.ItemId).First();

                        idal.IHuDetailDAL.UpdateByExtended(u => u.Id == item2.Id, u => new HuDetail() { SoNumber = edit.SoNumber, CustomerPoNumber = edit.CustomerPoNumber, AltItemNumber = edit.AltItemNumber, ItemId = edit.ItemId });
                    }

                    //3.7 修改收货扫描

                    var getSerialNumberInList1 = from a in idal.ISerialNumberInDAL.SelectAll()
                                                 where a.WhCode == entity.WhCode && customerPoNumber.Contains(a.CustomerPoNumber) && getitemid.Contains((Int32)a.ItemId) && a.ReceiptId == entity.ReceiptId
                                                 && a.ClientCode == receiptRegister.ClientCode && soList.Contains(a.SoNumber) && huList.Contains(a.HuId)
                                                 select a;

                    List<SerialNumberIn> getSerialNumberInList = getSerialNumberInList1.ToList();
                    foreach (var item2 in getSerialNumberInList)
                    {
                        EditInBoundDetailResult edit = getNewList.Where(u => u.SoNumber == item2.SoNumber && u.CustomerPoNumber == entity.CustomerPoNumber && u.AltItemNumber == item2.AltItemNumber && u.ItemId == item2.ItemId).First();

                        idal.ISerialNumberInDAL.UpdateByExtended(u => u.Id == item2.Id, u => new SerialNumberIn() { SoNumber = edit.SoNumber, CustomerPoNumber = edit.CustomerPoNumber, PoId = edit.PoId, AltItemNumber = edit.AltItemNumber, ItemId = edit.ItemId });
                    }

                    List<TranLog> tranLogList = new List<TranLog>();
                    for (int i = 0; i < soList.Length; i++)
                    {
                        //插入记录
                        TranLog tl = new TranLog();
                        tl.TranType = "65";
                        tl.Description = "修改PO按托盘";
                        tl.TranDate = DateTime.Now;
                        tl.TranUser = entity.UserCode;
                        tl.WhCode = entity.WhCode;
                        tl.SoNumber = soList[i].ToString();
                        tl.CustomerPoNumber = poList[i].ToString();
                        tl.ReceiptId = entity.ReceiptId;
                        tl.ClientCode = receiptRegister.ClientCode;
                        tranLogList.Add(tl);

                        TranLog tl1 = new TranLog();
                        tl1.TranType = "65";
                        tl1.Description = "修改PO按托盘";
                        tl1.TranDate = DateTime.Now;
                        tl1.TranUser = entity.UserCode;
                        tl1.WhCode = entity.WhCode;
                        tl1.SoNumber = soList[i].ToString();
                        tl1.CustomerPoNumber = entity.CustomerPoNumber;
                        tl1.ReceiptId = receipt;
                        tl1.ClientCode = receiptRegister.ClientCode;
                        tranLogList.Add(tl1);
                    }
                    idal.ITranLogDAL.Add(tranLogList);
                    #endregion

                    idal.SaveChanges();
                    trans.Complete();
                    return "Y";
                }
                catch
                {
                    trans.Dispose();//出现异常，事务手动释放
                    return "修改异常，请重新提交！";
                }
            }
        }


        //修改款号 By托盘
        public string EditDetailAltItemByHu(EditInBoundResult entity, string[] soList, string[] poList, string[] itemList, string[] huList)
        {
            using (TransactionScope trans = new TransactionScope())
            {
                try
                {
                    #region 优化
                    List<ReceiptRegister> ReceiptRegisterList = idal.IReceiptRegisterDAL.SelectBy(u => u.WhCode == entity.WhCode && u.ReceiptId == entity.ReceiptId);
                    if (ReceiptRegisterList.Count == 0)
                    {
                        return "未找到明细，无法修改！";
                    }


                    List<HuDetail> checkPlanQtyList = idal.IHuDetailDAL.SelectBy(u => u.WhCode == entity.WhCode && u.ReceiptId == entity.ReceiptId && (u.PlanQty ?? 0) != 0 && soList.Contains(u.SoNumber) && poList.Contains(u.CustomerPoNumber) && itemList.Contains(u.AltItemNumber) && huList.Contains(u.HuId));
                    if (checkPlanQtyList.Count > 0)
                    {
                        return "款号已做备货单无法修改，请撤销释放后修改！";
                    }

                    ReceiptRegister receiptRegister = ReceiptRegisterList.First();
                    entity.ClientCode = receiptRegister.ClientCode;
                    entity.ClientId = receiptRegister.ClientId;

                    var sql = (from a in idal.IReceiptRegisterDAL.SelectAll()
                               join b in idal.IReceiptRegisterDetailDAL.SelectAll()
                                     on new { a.WhCode, a.ReceiptId }
                                 equals new { b.WhCode, b.ReceiptId } into b_join
                               from b in b_join.DefaultIfEmpty()
                               join c in idal.IInBoundOrderDetailDAL.SelectAll() on new { InOrderDetailId = b.InOrderDetailId } equals new { InOrderDetailId = c.Id } into c_join
                               from c in c_join.DefaultIfEmpty()
                               join d in idal.IInBoundOrderDAL.SelectAll() on new { PoId = c.PoId } equals new { PoId = d.Id } into d_join
                               from d in d_join.DefaultIfEmpty()
                               join e in idal.IInBoundSODAL.SelectAll() on new { SoId = (Int32)d.SoId } equals new { SoId = e.Id } into e_join
                               from e in e_join.DefaultIfEmpty()
                               join f in idal.IItemMasterDAL.SelectAll() on new { ItemId = c.ItemId } equals new { ItemId = f.Id } into f_join
                               from f in f_join.DefaultIfEmpty()
                               where
                                 a.ReceiptId == entity.ReceiptId && a.WhCode == entity.WhCode && soList.Contains(e.SoNumber) && poList.Contains(d.CustomerPoNumber) && itemList.Contains(f.AltItemNumber)
                               select new EditInBoundDetailResult
                               {
                                   ClientCode = a.ClientCode,
                                   ReceiptId = a.ReceiptId,
                                   SoNumber = e.SoNumber,
                                   SoId = e.Id,
                                   PoId = d.Id,
                                   CustomerPoNumber = d.CustomerPoNumber,
                                   ItemId = f.Id,
                                   AltItemNumber = f.AltItemNumber,
                                   Style1 = f.Style1 ?? "",
                                   Style2 = f.Style2 ?? "",
                                   Style3 = f.Style3 ?? "",
                                   Qty = (Int32?)b.RegQty,
                                   UnitName = b.UnitName,
                                   UnitId = b.UnitId,
                                   RegDetailId = b.Id,
                                   OrderType = d.OrderType
                               }).Distinct();

                    if (sql.Where(u => u.AltItemNumber == entity.AltItemNumber).Count() > 0)
                    {
                        return "款号与原款号一致，无需修改！";
                    }
                    //如果是多种单位的货 暂时无法修改
                    string[] unitName1 = (from a in idal.IUnitDefaultDAL.SelectAll()
                                          where a.WhCode == entity.WhCode
                                          select a.UnitName).ToArray();
                    if (sql.Where(u => !unitName1.Contains(u.UnitName)).Count() > 0)
                    {
                        return "款号属于多种定制单位，无法修改！";
                    }

                    List<EditInBoundDetailResult> getList = sql.ToList();

                    //1. 取得原预录入明细--后面修改收货及库存有用
                    int[] getsoid = (from a in getList
                                     select a.SoId).Distinct().ToArray();

                    int[] getpoid = (from a in getList
                                     select a.PoId).Distinct().ToArray();

                    int[] getitemid = (from a in getList
                                       select a.ItemId).Distinct().ToArray();

                    int[] regDetailid = (from a in getList
                                         select a.RegDetailId).Distinct().ToArray();

                    string[] customerPoNumber = (from a in getList
                                                 select a.CustomerPoNumber).Distinct().ToArray();

                    //2. 重建预录入
                    string getclient = "", getSo = "", getPo = "", getSku = "", getStyle1 = "", getStyle2 = "", getStyle3 = "", getQty = "", unitName = "";


                    //2.1 得到需调整的总件数
                    List<Receipt> getOldReceiptList = idal.IReceiptDAL.SelectBy(u => u.WhCode == entity.WhCode && u.ReceiptId == entity.ReceiptId && soList.Contains(u.SoNumber) && poList.Contains(u.CustomerPoNumber) && itemList.Contains(u.AltItemNumber) && huList.Contains(u.HuId));
                    string[] getunitNamecheck = (from a in getOldReceiptList
                                                 select a.UnitName).Distinct().ToArray();

                    if (getunitNamecheck.Count() > 1)
                    {
                        return "该托盘存在多种收货单位，请先调整为一种单位！";
                    }

                    List<InBoundOrderInsert> entityList = new List<InBoundOrderInsert>();
                    foreach (var item in getList)
                    {
                        int getRecQty = getOldReceiptList.Where(u => u.SoNumber == item.SoNumber && u.CustomerPoNumber == item.CustomerPoNumber && u.AltItemNumber == item.AltItemNumber).Sum(u => u.Qty);

                        getclient = entity.ClientCode;
                        getSo = item.SoNumber;
                        getPo = item.CustomerPoNumber;
                        getSku = entity.AltItemNumber;
                        getStyle1 = item.Style1;
                        getStyle2 = item.Style2;
                        getStyle3 = item.Style3;
                        getQty = getRecQty.ToString();
                        unitName = item.UnitName;

                        if (entityList.Where(u => u.SoNumber == getSo.Trim() && u.ClientCode == getclient.Trim()).Count() == 0)
                        {
                            InBoundOrderInsert entity1 = new InBoundOrderInsert();
                            entity1.WhCode = entity.WhCode;
                            entity1.ClientCode = getclient.Trim();
                            entity1.SoNumber = getSo.Trim();
                            entity1.CreateUser = entity.UserCode;
                            entity1.OrderType = item.OrderType;

                            List<InBoundOrderDetailInsert> orderDetailList = new List<InBoundOrderDetailInsert>();

                            InBoundOrderDetailInsert orderDetail = new InBoundOrderDetailInsert();
                            orderDetail.CustomerPoNumber = getPo.Trim().ToString();
                            orderDetail.AltItemNumber = getSku.Trim().ToString();
                            orderDetail.Style1 = getStyle1.ToString();
                            orderDetail.Style2 = getStyle2.ToString();
                            orderDetail.Style3 = getStyle3.ToString();
                            orderDetail.UnitName = unitName;
                            orderDetail.Qty = Convert.ToInt32(getQty);
                            orderDetail.CreateUser = entity.UserCode;
                            orderDetailList.Add(orderDetail);

                            entity1.InBoundOrderDetailInsert = orderDetailList;
                            entityList.Add(entity1);
                        }
                        else
                        {
                            InBoundOrderInsert oldentity = entityList.Where(u => u.SoNumber == getSo.Trim() && u.ClientCode == getclient.Trim()).First();
                            entityList.Remove(oldentity);

                            InBoundOrderInsert newentity = oldentity;

                            List<InBoundOrderDetailInsert> orderDetailList = oldentity.InBoundOrderDetailInsert.ToList();
                            if (orderDetailList.Where(u => u.CustomerPoNumber == getPo.Trim() && u.AltItemNumber == getSku.Trim() && u.Style1 == getStyle1 && u.Style2 == getStyle2 && u.Style3 == getStyle3).Count() == 0)
                            {
                                InBoundOrderDetailInsert orderDetail = new InBoundOrderDetailInsert();
                                orderDetail.CustomerPoNumber = getPo.Trim().ToString();
                                orderDetail.AltItemNumber = getSku.Trim().ToString();
                                orderDetail.Style1 = getStyle1.Trim().ToString();
                                orderDetail.Style2 = getStyle2.ToString();
                                orderDetail.Style3 = getStyle3.ToString();
                                orderDetail.UnitName = unitName;
                                orderDetail.Qty = Convert.ToInt32(getQty);
                                orderDetail.CreateUser = entity.UserCode;
                                orderDetailList.Add(orderDetail);
                            }
                            else
                            {
                                InBoundOrderDetailInsert oldorderDetail = orderDetailList.Where(u => u.CustomerPoNumber == getPo.Trim() && u.AltItemNumber == getSku.Trim() && u.Style1 == getStyle1.Trim() && u.Style2 == getStyle2 && u.Style3 == getStyle3).First();

                                orderDetailList.Remove(oldorderDetail);
                                InBoundOrderDetailInsert neworderDetail = oldorderDetail;
                                neworderDetail.Qty = oldorderDetail.Qty + Convert.ToInt32(getQty);
                                orderDetailList.Add(neworderDetail);
                            }

                            newentity.InBoundOrderDetailInsert = orderDetailList;
                            entityList.Add(newentity);
                        }
                    }

                    string result = "";
                    //3.1 修改原收货批次明细
                    foreach (var item in getOldReceiptList)
                    {
                        List<ReceiptRegisterDetail> regDetailList = idal.IReceiptRegisterDetailDAL.SelectBy(u => u.WhCode == entity.WhCode && u.ReceiptId == entity.ReceiptId && u.PoId == item.PoId && u.ItemId == item.ItemId);
                        if (regDetailList.Count > 0)
                        {
                            ReceiptRegisterDetail regDetail = regDetailList.First();
                            regDetail.RegQty = regDetail.RegQty - item.Qty;
                            idal.IReceiptRegisterDetailDAL.UpdateBy(regDetail, u => u.Id == regDetail.Id, new string[] { "RegQty" });
                        }
                        else
                        {
                            result = "收货登记明细异常，无法修改！";
                            break;
                        }
                    }
                    if (result != "")
                    {
                        return result;
                    }

                    //3.2 创建收货登记
                    string receipt = entity.ReceiptId;

                    string s = AddInBoundRegDetail(entity, entityList, receiptRegister, receipt);
                    if (s != "Y")
                    {
                        return s;
                    }

                    idal.IInBoundOrderDetailDAL.SaveChanges();

                    //3.3 删除原收货登记下 数量为0的数据
                    idal.IReceiptRegisterDetailDAL.DeleteByExtended(u => u.ReceiptId == entity.ReceiptId && u.WhCode == entity.WhCode && u.RegQty == 0);

                    //3.5 修改收货

                    var sql1 = from a in idal.IReceiptRegisterDAL.SelectAll()
                               join b in idal.IReceiptRegisterDetailDAL.SelectAll()
                                     on new { a.WhCode, a.ReceiptId }
                                 equals new { b.WhCode, b.ReceiptId } into b_join
                               from b in b_join.DefaultIfEmpty()
                               join c in idal.IInBoundOrderDetailDAL.SelectAll() on new { InOrderDetailId = b.InOrderDetailId } equals new { InOrderDetailId = c.Id } into c_join
                               from c in c_join.DefaultIfEmpty()
                               join d in idal.IInBoundOrderDAL.SelectAll() on new { PoId = c.PoId } equals new { PoId = d.Id } into d_join
                               from d in d_join.DefaultIfEmpty()
                               join e in idal.IInBoundSODAL.SelectAll() on new { SoId = (Int32)d.SoId } equals new { SoId = e.Id } into e_join
                               from e in e_join.DefaultIfEmpty()
                               join f in idal.IItemMasterDAL.SelectAll() on new { ItemId = c.ItemId } equals new { ItemId = f.Id } into f_join
                               from f in f_join.DefaultIfEmpty()
                               where
                                 a.ReceiptId == receipt && a.WhCode == entity.WhCode && soList.Contains(e.SoNumber) && poList.Contains(d.CustomerPoNumber) && f.AltItemNumber == entity.AltItemNumber
                               select new EditInBoundDetailResult
                               {
                                   ClientCode = a.ClientCode,
                                   ReceiptId = a.ReceiptId,
                                   SoNumber = e.SoNumber,
                                   SoId = e.Id,
                                   PoId = d.Id,
                                   CustomerPoNumber = d.CustomerPoNumber,
                                   ItemId = f.Id,
                                   AltItemNumber = f.AltItemNumber,
                                   Style1 = f.Style1,
                                   Style2 = f.Style2,
                                   Style3 = f.Style3,
                                   Qty = (Int32?)b.RegQty,
                                   UnitName = b.UnitName,
                                   UnitId = b.UnitId,
                                   RegDetailId = b.Id
                               };

                    List<EditInBoundDetailResult> getNewList = sql1.ToList();

                    var getReceiptList1 = from a in idal.IReceiptDAL.SelectAll()
                                          where a.WhCode == entity.WhCode && getpoid.Contains(a.PoId) && getitemid.Contains(a.ItemId) && a.ReceiptId == entity.ReceiptId
                                          && a.ClientCode == receiptRegister.ClientCode && soList.Contains(a.SoNumber) && huList.Contains(a.HuId)
                                          select a;
                    List<Receipt> getReceiptList = getReceiptList1.ToList();

                    foreach (var item1 in getReceiptList)
                    {
                        ItemMaster getItemFirst = idal.IItemMasterDAL.SelectBy(u => u.Id == item1.ItemId).First();

                        EditInBoundDetailResult edit = getNewList.Where(u => u.SoNumber == item1.SoNumber && u.CustomerPoNumber == item1.CustomerPoNumber && u.AltItemNumber == entity.AltItemNumber && u.UnitName == item1.UnitName && (u.Style1 ?? "") == (getItemFirst.Style1 ?? "") && (u.Style2 ?? "") == (getItemFirst.Style2 ?? "") && (u.Style3 ?? "") == (getItemFirst.Style3 ?? "")).First();

                        idal.IReceiptDAL.UpdateByExtended(u => u.Id == item1.Id, u => new Receipt() { SoNumber = edit.SoNumber, CustomerPoNumber = edit.CustomerPoNumber, PoId = edit.PoId, AltItemNumber = edit.AltItemNumber, ItemId = edit.ItemId });
                    }

                    //3.6 修改库存

                    var getHudetailList1 = from a in idal.IHuDetailDAL.SelectAll()
                                           where a.WhCode == entity.WhCode && customerPoNumber.Contains(a.CustomerPoNumber) && getitemid.Contains(a.ItemId) && a.ReceiptId == entity.ReceiptId
                                           && a.ClientCode == receiptRegister.ClientCode && soList.Contains(a.SoNumber) && huList.Contains(a.HuId)
                                           select a;

                    List<HuDetail> getHudetailList = getHudetailList1.ToList();
                    foreach (var item2 in getHudetailList)
                    {
                        ItemMaster getItemFirst = idal.IItemMasterDAL.SelectBy(u => u.Id == item2.ItemId).First();

                        EditInBoundDetailResult edit = getNewList.Where(u => u.SoNumber == item2.SoNumber && u.CustomerPoNumber == item2.CustomerPoNumber && u.AltItemNumber == entity.AltItemNumber && u.UnitName == item2.UnitName && (u.Style1 ?? "") == (getItemFirst.Style1 ?? "") && (u.Style2 ?? "") == (getItemFirst.Style2 ?? "") && (u.Style3 ?? "") == (getItemFirst.Style3 ?? "")).First();

                        idal.IHuDetailDAL.UpdateByExtended(u => u.Id == item2.Id, u => new HuDetail() { SoNumber = edit.SoNumber, CustomerPoNumber = edit.CustomerPoNumber, AltItemNumber = edit.AltItemNumber, ItemId = edit.ItemId });
                    }

                    //3.7 修改收货扫描

                    var getSerialNumberInList1 = from a in idal.ISerialNumberInDAL.SelectAll()
                                                 where a.WhCode == entity.WhCode && customerPoNumber.Contains(a.CustomerPoNumber) && getitemid.Contains((Int32)a.ItemId) && a.ReceiptId == entity.ReceiptId
                                                 && a.ClientCode == receiptRegister.ClientCode && soList.Contains(a.SoNumber) && huList.Contains(a.HuId)
                                                 select a;

                    List<SerialNumberIn> getSerialNumberInList = getSerialNumberInList1.ToList();
                    foreach (var item2 in getSerialNumberInList)
                    {
                        ItemMaster getItemFirst = idal.IItemMasterDAL.SelectBy(u => u.Id == item2.ItemId).First();

                        EditInBoundDetailResult edit = getNewList.Where(u => u.SoNumber == item2.SoNumber && u.CustomerPoNumber == item2.CustomerPoNumber && u.AltItemNumber == entity.AltItemNumber && (u.Style1 ?? "") == (getItemFirst.Style1 ?? "") && (u.Style2 ?? "") == (getItemFirst.Style2 ?? "") && (u.Style3 ?? "") == (getItemFirst.Style3 ?? "")).First();

                        idal.ISerialNumberInDAL.UpdateByExtended(u => u.Id == item2.Id, u => new SerialNumberIn() { SoNumber = edit.SoNumber, CustomerPoNumber = edit.CustomerPoNumber, PoId = edit.PoId, AltItemNumber = edit.AltItemNumber, ItemId = edit.ItemId });
                    }

                    List<TranLog> tranLogList = new List<TranLog>();
                    for (int i = 0; i < soList.Length; i++)
                    {
                        //插入记录
                        TranLog tl = new TranLog();
                        tl.TranType = "66";
                        tl.Description = "修改Item按托盘";
                        tl.TranDate = DateTime.Now;
                        tl.TranUser = entity.UserCode;
                        tl.WhCode = entity.WhCode;
                        tl.SoNumber = soList[i].ToString();
                        tl.CustomerPoNumber = poList[i].ToString();
                        tl.AltItemNumber = itemList[i].ToString();
                        tl.ReceiptId = entity.ReceiptId;
                        tl.ClientCode = receiptRegister.ClientCode;
                        tranLogList.Add(tl);

                        TranLog tl1 = new TranLog();
                        tl1.TranType = "66";
                        tl1.Description = "修改Item按托盘";
                        tl1.TranDate = DateTime.Now;
                        tl1.TranUser = entity.UserCode;
                        tl1.WhCode = entity.WhCode;
                        tl1.SoNumber = soList[i].ToString();
                        tl1.CustomerPoNumber = poList[i].ToString();
                        tl1.AltItemNumber = entity.AltItemNumber;
                        tl1.ReceiptId = receipt;
                        tl1.ClientCode = receiptRegister.ClientCode;
                        tranLogList.Add(tl1);
                    }
                    idal.ITranLogDAL.Add(tranLogList);
                    #endregion

                    idal.SaveChanges();
                    trans.Complete();
                    return "Y";
                }
                catch
                {
                    trans.Dispose();//出现异常，事务手动释放
                    return "修改异常，请重新提交！";
                }
            }
        }


        //修改款号的属性 By托盘
        public string EditDetailAltItemStyleByHu(EditInBoundResult entity, string[] soList, string[] poList, string[] itemList, string[] style1List, string[] style2List, string[] style3List, string[] huList)
        {
            using (TransactionScope trans = new TransactionScope())
            {
                try
                {
                    #region 优化
                    List<ReceiptRegister> ReceiptRegisterList = idal.IReceiptRegisterDAL.SelectBy(u => u.WhCode == entity.WhCode && u.ReceiptId == entity.ReceiptId);
                    if (ReceiptRegisterList.Count == 0)
                    {
                        return "未找到明细，无法修改！";
                    }

                    ReceiptRegister receiptRegister = ReceiptRegisterList.First();
                    entity.ClientCode = receiptRegister.ClientCode;
                    entity.ClientId = receiptRegister.ClientId;

                    List<ItemMaster> getItemMasterList = idal.IItemMasterDAL.SelectBy(u => u.WhCode == entity.WhCode && u.ClientCode == entity.ClientCode && itemList.Contains(u.AltItemNumber) && style1List.Contains(u.Style1) && style2List.Contains(u.Style2) && style3List.Contains(u.Style3)).OrderBy(u => u.Id).ToList();

                    if (getItemMasterList.Count == 0)
                    {
                        return "系统未找到该款号属性信息，无法继续修改！";
                    }

                    ItemMaster itemFirst = getItemMasterList.First();

                    List<HuDetail> checkPlanQtyList = idal.IHuDetailDAL.SelectBy(u => u.WhCode == entity.WhCode && u.ReceiptId == entity.ReceiptId && (u.PlanQty ?? 0) != 0 && soList.Contains(u.SoNumber) && poList.Contains(u.CustomerPoNumber) && itemList.Contains(u.AltItemNumber) && huList.Contains(u.HuId) && u.ItemId == itemFirst.Id);

                    if (checkPlanQtyList.Count > 0)
                    {
                        return "款号已做备货单无法修改，请撤销释放后修改！";
                    }

                    var sql = (from a in idal.IReceiptRegisterDAL.SelectAll()
                               join b in idal.IReceiptRegisterDetailDAL.SelectAll()
                                     on new { a.WhCode, a.ReceiptId }
                                 equals new { b.WhCode, b.ReceiptId } into b_join
                               from b in b_join.DefaultIfEmpty()
                               join c in idal.IInBoundOrderDetailDAL.SelectAll() on new { InOrderDetailId = b.InOrderDetailId } equals new { InOrderDetailId = c.Id } into c_join
                               from c in c_join.DefaultIfEmpty()
                               join d in idal.IInBoundOrderDAL.SelectAll() on new { PoId = c.PoId } equals new { PoId = d.Id } into d_join
                               from d in d_join.DefaultIfEmpty()
                               join e in idal.IInBoundSODAL.SelectAll() on new { SoId = (Int32)d.SoId } equals new { SoId = e.Id } into e_join
                               from e in e_join.DefaultIfEmpty()
                               join f in idal.IItemMasterDAL.SelectAll() on new { ItemId = c.ItemId } equals new { ItemId = f.Id } into f_join
                               from f in f_join.DefaultIfEmpty()
                               where
                                 a.ReceiptId == entity.ReceiptId && a.WhCode == entity.WhCode && soList.Contains(e.SoNumber) && poList.Contains(d.CustomerPoNumber) && itemList.Contains(f.AltItemNumber) && f.Id == itemFirst.Id
                               select new EditInBoundDetailResult
                               {
                                   ClientCode = a.ClientCode,
                                   ReceiptId = a.ReceiptId,
                                   SoNumber = e.SoNumber,
                                   SoId = e.Id,
                                   PoId = d.Id,
                                   CustomerPoNumber = d.CustomerPoNumber,
                                   ItemId = f.Id,
                                   AltItemNumber = f.AltItemNumber,
                                   Style1 = f.Style1 ?? "",
                                   Style2 = f.Style2 ?? "",
                                   Style3 = f.Style3 ?? "",
                                   Qty = (Int32?)b.RegQty,
                                   UnitName = b.UnitName,
                                   UnitId = b.UnitId,
                                   RegDetailId = b.Id,
                                   OrderType = d.OrderType
                               }).Distinct();

                    if (sql.Where(u => u.Style1 == (entity.Style1 ?? "") && u.Style2 == (entity.Style2 ?? "") && u.Style3 == (entity.Style3 ?? "")).Count() > 0)
                    {
                        return "款号属性与原款号属性一致，无需修改！";
                    }
                    //如果是多种单位的货 暂时无法修改
                    string[] unitName1 = (from a in idal.IUnitDefaultDAL.SelectAll()
                                          where a.WhCode == entity.WhCode
                                          select a.UnitName).ToArray();
                    if (sql.Where(u => !unitName1.Contains(u.UnitName)).Count() > 0)
                    {
                        return "款号属于多种定制单位，无法修改！";
                    }

                    List<EditInBoundDetailResult> getList = sql.ToList();

                    //1. 取得原预录入明细--后面修改收货及库存有用
                    int[] getsoid = (from a in getList
                                     select a.SoId).Distinct().ToArray();

                    int[] getpoid = (from a in getList
                                     select a.PoId).Distinct().ToArray();

                    int[] getitemid = (from a in getList
                                       select a.ItemId).Distinct().ToArray();

                    int[] regDetailid = (from a in getList
                                         select a.RegDetailId).Distinct().ToArray();

                    string[] customerPoNumber = (from a in getList
                                                 select a.CustomerPoNumber).Distinct().ToArray();

                    //2. 重建预录入
                    string getclient = "", getSo = "", getPo = "", getSku = "", getStyle1 = "", getStyle2 = "", getStyle3 = "", getQty = "", unitName = "";


                    //2.1 得到需调整的总件数
                    List<Receipt> getOldReceiptList = idal.IReceiptDAL.SelectBy(u => u.WhCode == entity.WhCode && u.ReceiptId == entity.ReceiptId && soList.Contains(u.SoNumber) && poList.Contains(u.CustomerPoNumber) && itemList.Contains(u.AltItemNumber) && huList.Contains(u.HuId) && u.ItemId == itemFirst.Id);
                    string[] getunitNamecheck = (from a in getOldReceiptList
                                                 select a.UnitName).Distinct().ToArray();

                    if (getunitNamecheck.Count() > 1)
                    {
                        return "该托盘存在多种收货单位，请先调整为一种单位！";
                    }

                    List<InBoundOrderInsert> entityList = new List<InBoundOrderInsert>();
                    foreach (var item in getList)
                    {
                        int getRecQty = getOldReceiptList.Where(u => u.SoNumber == item.SoNumber && u.CustomerPoNumber == item.CustomerPoNumber && u.AltItemNumber == item.AltItemNumber).Sum(u => u.Qty);

                        getclient = entity.ClientCode;
                        getSo = item.SoNumber;
                        getPo = item.CustomerPoNumber;
                        getSku = item.AltItemNumber;
                        getStyle1 = entity.Style1;
                        getStyle2 = entity.Style2;
                        getStyle3 = entity.Style3;
                        getQty = getRecQty.ToString();
                        unitName = item.UnitName;

                        if (entityList.Where(u => u.SoNumber == getSo.Trim() && u.ClientCode == getclient.Trim()).Count() == 0)
                        {
                            InBoundOrderInsert entity1 = new InBoundOrderInsert();
                            entity1.WhCode = entity.WhCode;
                            entity1.ClientCode = getclient.Trim();
                            entity1.SoNumber = getSo.Trim();
                            entity1.CreateUser = entity.UserCode;
                            entity1.OrderType = item.OrderType;

                            List<InBoundOrderDetailInsert> orderDetailList = new List<InBoundOrderDetailInsert>();

                            InBoundOrderDetailInsert orderDetail = new InBoundOrderDetailInsert();
                            orderDetail.CustomerPoNumber = getPo.Trim().ToString();
                            orderDetail.AltItemNumber = getSku.Trim().ToString();
                            orderDetail.Style1 = getStyle1.ToString();
                            orderDetail.Style2 = getStyle2.ToString();
                            orderDetail.Style3 = getStyle3.ToString();
                            orderDetail.UnitName = unitName;
                            orderDetail.Qty = Convert.ToInt32(getQty);
                            orderDetail.CreateUser = entity.UserCode;
                            orderDetailList.Add(orderDetail);

                            entity1.InBoundOrderDetailInsert = orderDetailList;
                            entityList.Add(entity1);
                        }
                        else
                        {
                            InBoundOrderInsert oldentity = entityList.Where(u => u.SoNumber == getSo.Trim() && u.ClientCode == getclient.Trim()).First();
                            entityList.Remove(oldentity);

                            InBoundOrderInsert newentity = oldentity;

                            List<InBoundOrderDetailInsert> orderDetailList = oldentity.InBoundOrderDetailInsert.ToList();
                            if (orderDetailList.Where(u => u.CustomerPoNumber == getPo.Trim() && u.AltItemNumber == getSku.Trim() && u.Style1 == getStyle1 && u.Style2 == getStyle2 && u.Style3 == getStyle3).Count() == 0)
                            {
                                InBoundOrderDetailInsert orderDetail = new InBoundOrderDetailInsert();
                                orderDetail.CustomerPoNumber = getPo.Trim().ToString();
                                orderDetail.AltItemNumber = getSku.Trim().ToString();
                                orderDetail.Style1 = getStyle1.Trim().ToString();
                                orderDetail.Style2 = getStyle2.ToString();
                                orderDetail.Style3 = getStyle3.ToString();
                                orderDetail.UnitName = unitName;
                                orderDetail.Qty = Convert.ToInt32(getQty);
                                orderDetail.CreateUser = entity.UserCode;
                                orderDetailList.Add(orderDetail);
                            }
                            else
                            {
                                InBoundOrderDetailInsert oldorderDetail = orderDetailList.Where(u => u.CustomerPoNumber == getPo.Trim() && u.AltItemNumber == getSku.Trim() && u.Style1 == getStyle1.Trim() && u.Style2 == getStyle2 && u.Style3 == getStyle3).First();

                                orderDetailList.Remove(oldorderDetail);
                                InBoundOrderDetailInsert neworderDetail = oldorderDetail;
                                neworderDetail.Qty = oldorderDetail.Qty + Convert.ToInt32(getQty);
                                orderDetailList.Add(neworderDetail);
                            }

                            newentity.InBoundOrderDetailInsert = orderDetailList;
                            entityList.Add(newentity);
                        }
                    }

                    string result = "";
                    //3.1 修改原收货批次明细
                    foreach (var item in getOldReceiptList)
                    {
                        List<ReceiptRegisterDetail> regDetailList = idal.IReceiptRegisterDetailDAL.SelectBy(u => u.WhCode == entity.WhCode && u.ReceiptId == entity.ReceiptId && u.PoId == item.PoId && u.ItemId == item.ItemId);
                        if (regDetailList.Count > 0)
                        {
                            ReceiptRegisterDetail regDetail = regDetailList.First();
                            regDetail.RegQty = regDetail.RegQty - item.Qty;
                            idal.IReceiptRegisterDetailDAL.UpdateBy(regDetail, u => u.Id == regDetail.Id, new string[] { "RegQty" });
                        }
                        else
                        {
                            result = "收货登记明细异常，无法修改！";
                            break;
                        }
                    }
                    if (result != "")
                    {
                        return result;
                    }

                    //3.2 创建收货登记
                    string receipt = entity.ReceiptId;

                    string s = AddInBoundRegDetail(entity, entityList, receiptRegister, receipt);
                    if (s != "Y")
                    {
                        return s;
                    }

                    idal.IInBoundOrderDetailDAL.SaveChanges();

                    //3.3 删除原收货登记下 数量为0的数据
                    idal.IReceiptRegisterDetailDAL.DeleteByExtended(u => u.ReceiptId == entity.ReceiptId && u.WhCode == entity.WhCode && u.RegQty == 0);

                    //3.5 修改收货

                    var sql1 = from a in idal.IReceiptRegisterDAL.SelectAll()
                               join b in idal.IReceiptRegisterDetailDAL.SelectAll()
                                     on new { a.WhCode, a.ReceiptId }
                                 equals new { b.WhCode, b.ReceiptId } into b_join
                               from b in b_join.DefaultIfEmpty()
                               join c in idal.IInBoundOrderDetailDAL.SelectAll() on new { InOrderDetailId = b.InOrderDetailId } equals new { InOrderDetailId = c.Id } into c_join
                               from c in c_join.DefaultIfEmpty()
                               join d in idal.IInBoundOrderDAL.SelectAll() on new { PoId = c.PoId } equals new { PoId = d.Id } into d_join
                               from d in d_join.DefaultIfEmpty()
                               join e in idal.IInBoundSODAL.SelectAll() on new { SoId = (Int32)d.SoId } equals new { SoId = e.Id } into e_join
                               from e in e_join.DefaultIfEmpty()
                               join f in idal.IItemMasterDAL.SelectAll() on new { ItemId = c.ItemId } equals new { ItemId = f.Id } into f_join
                               from f in f_join.DefaultIfEmpty()
                               where
                                 a.ReceiptId == receipt && a.WhCode == entity.WhCode && soList.Contains(e.SoNumber) && poList.Contains(d.CustomerPoNumber) && itemList.Contains(f.AltItemNumber) && f.Style1 == (entity.Style1 ?? "") && f.Style2 == (entity.Style2 ?? "") && f.Style3 == (entity.Style3 ?? "")
                               select new EditInBoundDetailResult
                               {
                                   ClientCode = a.ClientCode,
                                   ReceiptId = a.ReceiptId,
                                   SoNumber = e.SoNumber,
                                   SoId = e.Id,
                                   PoId = d.Id,
                                   CustomerPoNumber = d.CustomerPoNumber,
                                   ItemId = f.Id,
                                   AltItemNumber = f.AltItemNumber,
                                   Style1 = f.Style1 ?? "",
                                   Style2 = f.Style2 ?? "",
                                   Style3 = f.Style3 ?? "",
                                   Qty = (Int32?)b.RegQty,
                                   UnitName = b.UnitName,
                                   UnitId = b.UnitId,
                                   RegDetailId = b.Id
                               };

                    List<EditInBoundDetailResult> getNewList = sql1.ToList();

                    var getReceiptList1 = from a in idal.IReceiptDAL.SelectAll()
                                          where a.WhCode == entity.WhCode && getpoid.Contains(a.PoId) && getitemid.Contains(a.ItemId) && a.ReceiptId == entity.ReceiptId
                                          && a.ClientCode == receiptRegister.ClientCode && soList.Contains(a.SoNumber) && huList.Contains(a.HuId)
                                          select a;
                    List<Receipt> getReceiptList = getReceiptList1.ToList();

                    foreach (var item1 in getReceiptList)
                    {
                        EditInBoundDetailResult edit = getNewList.Where(u => u.SoNumber == item1.SoNumber && u.CustomerPoNumber == item1.CustomerPoNumber && u.AltItemNumber == item1.AltItemNumber && u.UnitName == item1.UnitName && u.Style1 == (entity.Style1 ?? "") && u.Style2 == (entity.Style2 ?? "") && u.Style3 == (entity.Style3 ?? "")).First();

                        idal.IReceiptDAL.UpdateByExtended(u => u.Id == item1.Id, u => new Receipt() { SoNumber = edit.SoNumber, CustomerPoNumber = edit.CustomerPoNumber, PoId = edit.PoId, AltItemNumber = edit.AltItemNumber, ItemId = edit.ItemId });
                    }

                    //3.6 修改库存

                    var getHudetailList1 = from a in idal.IHuDetailDAL.SelectAll()
                                           where a.WhCode == entity.WhCode && customerPoNumber.Contains(a.CustomerPoNumber) && getitemid.Contains(a.ItemId) && a.ReceiptId == entity.ReceiptId
                                           && a.ClientCode == receiptRegister.ClientCode && soList.Contains(a.SoNumber) && huList.Contains(a.HuId)
                                           select a;

                    List<HuDetail> getHudetailList = getHudetailList1.ToList();
                    foreach (var item2 in getHudetailList)
                    {
                        EditInBoundDetailResult edit = getNewList.Where(u => u.SoNumber == item2.SoNumber && u.CustomerPoNumber == item2.CustomerPoNumber && u.AltItemNumber == item2.AltItemNumber && u.UnitName == item2.UnitName && u.Style1 == (entity.Style1 ?? "") && u.Style2 == (entity.Style2 ?? "") && u.Style3 == (entity.Style3 ?? "")).First();

                        idal.IHuDetailDAL.UpdateByExtended(u => u.Id == item2.Id, u => new HuDetail() { SoNumber = edit.SoNumber, CustomerPoNumber = edit.CustomerPoNumber, AltItemNumber = edit.AltItemNumber, ItemId = edit.ItemId });
                    }

                    //3.7 修改收货扫描

                    var getSerialNumberInList1 = from a in idal.ISerialNumberInDAL.SelectAll()
                                                 where a.WhCode == entity.WhCode && customerPoNumber.Contains(a.CustomerPoNumber) && getitemid.Contains((Int32)a.ItemId) && a.ReceiptId == entity.ReceiptId
                                                 && a.ClientCode == receiptRegister.ClientCode && soList.Contains(a.SoNumber) && huList.Contains(a.HuId)
                                                 select a;

                    List<SerialNumberIn> getSerialNumberInList = getSerialNumberInList1.ToList();
                    foreach (var item2 in getSerialNumberInList)
                    {
                        EditInBoundDetailResult edit = getNewList.Where(u => u.SoNumber == item2.SoNumber && u.CustomerPoNumber == item2.CustomerPoNumber && u.AltItemNumber == item2.AltItemNumber && u.Style1 == (entity.Style1 ?? "") && u.Style2 == (entity.Style2 ?? "") && u.Style3 == (entity.Style3 ?? "")).First();

                        idal.ISerialNumberInDAL.UpdateByExtended(u => u.Id == item2.Id, u => new SerialNumberIn() { SoNumber = edit.SoNumber, CustomerPoNumber = edit.CustomerPoNumber, PoId = edit.PoId, AltItemNumber = edit.AltItemNumber, ItemId = edit.ItemId });
                    }

                    List<TranLog> tranLogList = new List<TranLog>();
                    for (int i = 0; i < soList.Length; i++)
                    {
                        //插入记录
                        TranLog tl = new TranLog();
                        tl.TranType = "66";
                        tl.Description = "修改款号属性按托盘";
                        tl.TranDate = DateTime.Now;
                        tl.TranUser = entity.UserCode;
                        tl.WhCode = entity.WhCode;
                        tl.SoNumber = soList[i].ToString();
                        tl.CustomerPoNumber = poList[i].ToString();
                        tl.AltItemNumber = itemList[i].ToString();
                        tl.ReceiptId = entity.ReceiptId;
                        tl.ClientCode = receiptRegister.ClientCode;
                        tl.Remark = style1List[i].ToString() + style2List[i].ToString() + style3List[i].ToString();
                        tranLogList.Add(tl);

                        TranLog tl1 = new TranLog();
                        tl1.TranType = "66";
                        tl1.Description = "修改款号属性按托盘";
                        tl1.TranDate = DateTime.Now;
                        tl1.TranUser = entity.UserCode;
                        tl1.WhCode = entity.WhCode;
                        tl1.SoNumber = soList[i].ToString();
                        tl1.CustomerPoNumber = poList[i].ToString();
                        tl1.AltItemNumber = itemList[i].ToString();
                        tl1.ReceiptId = receipt;
                        tl1.ClientCode = receiptRegister.ClientCode;
                        tl.Remark = entity.Style1 + entity.Style2 + entity.Style3;
                        tranLogList.Add(tl1);
                    }
                    idal.ITranLogDAL.Add(tranLogList);
                    #endregion

                    idal.SaveChanges();
                    trans.Complete();
                    return "Y";
                }
                catch
                {
                    trans.Dispose();//出现异常，事务手动释放
                    return "修改异常，请重新提交！";
                }
            }
        }


        //新增款号
        public ItemMaster InsertItemMaster1(InBoundOrderInsert entity, InBoundOrderDetailInsert item)
        {
            List<ItemMaster> listItemMaster = idal.IItemMasterDAL.SelectBy(u => u.WhCode == entity.WhCode && u.AltItemNumber == item.AltItemNumber && (u.Style1 == null ? "" : u.Style1) == (item.Style1 == null ? "" : item.Style1) && (u.Style2 == null ? "" : u.Style2) == (item.Style2 == null ? "" : item.Style2) && (u.Style3 == null ? "" : u.Style3) == (item.Style3 == null ? "" : item.Style3) && u.ClientId == entity.ClientId).OrderBy(u => u.Id).ToList();
            ItemMaster itemMaster = new ItemMaster();
            if (listItemMaster.Count == 0)
            {
                itemMaster.ItemNumber = "SYS" + DI.IDGenerator.NewId;
                itemMaster.WhCode = entity.WhCode;
                itemMaster.AltItemNumber = item.AltItemNumber;
                itemMaster.ClientId = (int)entity.ClientId;
                itemMaster.ClientCode = entity.ClientCode;
                itemMaster.Style1 = item.Style1 ?? "";
                itemMaster.Style2 = item.Style2 ?? "";
                itemMaster.Style3 = item.Style3 ?? "";
                itemMaster.UnitFlag = 0;
                if (item.UnitName == "" || item.UnitName == null)
                {
                    itemMaster.UnitName = "none";
                }
                else
                {
                    itemMaster.UnitName = item.UnitName;
                }

                itemMaster.CreateUser = item.CreateUser;
                itemMaster.CreateDate = DateTime.Now;
                idal.IItemMasterDAL.Add(itemMaster);   //款号不存在就新增
            }
            else
            {
                itemMaster = listItemMaster.First();

                if (itemMaster.UnitName != item.UnitName)
                {
                    ItemMaster editItem = new ItemMaster();
                    editItem.UnitName = item.UnitName;
                    idal.IItemMasterDAL.UpdateBy(editItem, u => u.Id == itemMaster.Id, new string[] { "UnitName" });
                    idal.IItemMasterDAL.SaveChanges();

                    itemMaster = idal.IItemMasterDAL.SelectBy(u => u.WhCode == entity.WhCode && u.AltItemNumber == item.AltItemNumber && (u.Style1 == null ? "" : u.Style1) == (item.Style1 == null ? "" : item.Style1) && (u.Style2 == null ? "" : u.Style2) == (item.Style2 == null ? "" : item.Style2) && (u.Style3 == null ? "" : u.Style3) == (item.Style3 == null ? "" : item.Style3) && u.ClientId == entity.ClientId).OrderBy(u => u.Id).ToList().First();
                }
            }

            idal.IItemMasterDAL.SaveChanges();
            return itemMaster;
        }


        //新增预录入及收货登记明细
        public string AddInBoundRegDetail(EditInBoundResult entity, List<InBoundOrderInsert> entityList, ReceiptRegister receiptRegister, string receipt)
        {
            List<ReceiptRegisterInsert> listEntity = new List<ReceiptRegisterInsert>();     //收货登记明细列表

            //批量导入预录入
            foreach (var entity1 in entityList)
            {
                entity1.ClientId = entity.ClientId;
                entity1.ProcessId = receiptRegister.ProcessId;
                entity1.ProcessName = receiptRegister.ProcessName;

                foreach (var item11 in entity1.InBoundOrderDetailInsert)
                {
                    InBoundOrder inBoundOrder = new InBoundOrder();

                    if (!string.IsNullOrEmpty(entity1.SoNumber))
                    {
                        //添加InBoundSO 
                        InBoundSO inBoundSO = InsertInBoundSO(entity1, item11);

                        //添加InBoundOrder  
                        //判断客户PO是否存在
                        List<InBoundOrder> listInBoundOrder = idal.IInBoundOrderDAL.SelectBy(u => u.CustomerPoNumber == item11.CustomerPoNumber && u.WhCode == entity.WhCode && u.SoId == inBoundSO.Id);

                        inBoundOrder = InsertInBoundOrder(entity1, item11, inBoundSO, listInBoundOrder);
                    }
                    else
                    {
                        //添加InBoundOrder  
                        //判断客户PO是否存在
                        List<InBoundOrder> listInBoundOrder = idal.IInBoundOrderDAL.SelectBy(u => u.CustomerPoNumber == item11.CustomerPoNumber && u.WhCode == entity.WhCode && u.ClientId == entity.ClientId && u.SoId == null);

                        inBoundOrder = InsertInBoundOrder(entity1, item11, null, listInBoundOrder);
                    }

                    //判断款号是否存在
                    ItemMaster itemMaster = InsertItemMaster1(entity1, item11);

                    //添加InBoundOrderDetail
                    List<InBoundOrderDetail> listInBoundOrderDetail = new List<InBoundOrderDetail>();

                    if (itemMaster.UnitFlag == 1)
                    {
                        listInBoundOrderDetail = idal.IInBoundOrderDetailDAL.SelectBy(u => u.WhCode == entity.WhCode && u.ItemId == itemMaster.Id && u.PoId == inBoundOrder.Id && u.UnitId == item11.UnitId && u.UnitName == item11.UnitName).ToList();
                    }
                    else
                    {
                        listInBoundOrderDetail = idal.IInBoundOrderDetailDAL.SelectBy(u => u.WhCode == entity.WhCode && u.ItemId == itemMaster.Id && u.PoId == inBoundOrder.Id && u.UnitId == item11.UnitId).ToList();
                    }

                    InBoundOrderDetail inBoundOrderDetail = new InBoundOrderDetail();
                    int insertResult = 0;
                    if (listInBoundOrderDetail.Count == 0)
                    {
                        inBoundOrderDetail.WhCode = entity.WhCode;
                        inBoundOrderDetail.PoId = inBoundOrder.Id;
                        inBoundOrderDetail.ItemId = itemMaster.Id;
                        inBoundOrderDetail.UnitId = item11.UnitId;
                        if (itemMaster.UnitName == "" || itemMaster.UnitName == null)
                        {
                            inBoundOrderDetail.UnitName = "none";
                        }
                        else
                        {
                            if (itemMaster.UnitFlag == 1)
                            {
                                inBoundOrderDetail.UnitName = item11.UnitName;
                            }
                            else
                            {
                                inBoundOrderDetail.UnitName = itemMaster.UnitName;
                            }
                        }

                        inBoundOrderDetail.Qty = item11.Qty;
                        inBoundOrderDetail.Weight = (item11.Weight ?? 0);
                        inBoundOrderDetail.CBM = (item11.CBM ?? 0);
                        inBoundOrderDetail.CreateUser = item11.CreateUser;
                        inBoundOrderDetail.CreateDate = DateTime.Now;
                        idal.IInBoundOrderDetailDAL.Add(inBoundOrderDetail);
                        insertResult = 1;
                    }
                    else
                    {
                        inBoundOrderDetail = listInBoundOrderDetail.First();
                        int yuluruQty = inBoundOrderDetail.Qty - inBoundOrderDetail.RegQty;
                        if (yuluruQty < item11.Qty)
                        {
                            inBoundOrderDetail.Qty = inBoundOrderDetail.Qty + (item11.Qty - yuluruQty);
                            inBoundOrderDetail.UpdateUser = item11.CreateUser;
                            inBoundOrderDetail.UpdateDate = DateTime.Now;
                            idal.IInBoundOrderDetailDAL.UpdateBy(inBoundOrderDetail, u => u.Id == inBoundOrderDetail.Id, new string[] { "Qty", "UpdateUser", "UpdateDate" });
                        }
                        insertResult = 2;
                    }

                    idal.IInBoundOrderDetailDAL.SaveChanges();

                    if (insertResult == 1 || insertResult == 2)
                    {
                        ReceiptRegisterInsert receiptRegisterInsert = new ReceiptRegisterInsert();
                        receiptRegisterInsert.WhCode = entity.WhCode;
                        receiptRegisterInsert.ReceiptId = receipt;
                        receiptRegisterInsert.InBoundOrderDetailId = inBoundOrderDetail.Id;
                        receiptRegisterInsert.CustomerPoNumber = item11.CustomerPoNumber;
                        receiptRegisterInsert.AltItemNumber = item11.AltItemNumber;
                        receiptRegisterInsert.PoId = inBoundOrder.Id;
                        receiptRegisterInsert.ItemId = itemMaster.Id;
                        receiptRegisterInsert.UnitName = itemMaster.UnitName;
                        receiptRegisterInsert.UnitId = item11.UnitId;
                        receiptRegisterInsert.ProcessName = receiptRegister.ProcessName;
                        receiptRegisterInsert.ProcessId = (Int32)receiptRegister.ProcessId;

                        receiptRegisterInsert.RegQty = item11.Qty;
                        receiptRegisterInsert.CreateUser = item11.CreateUser;
                        receiptRegisterInsert.CreateDate = DateTime.Now;
                        listEntity.Add(receiptRegisterInsert);
                    }
                }
            }

            if (listEntity.Count == 0)
            {
                return "导入登记明细出错！";
            }

            //添加收货批次明细
            foreach (var item in listEntity)
            {
                List<ReceiptRegisterDetail> list = idal.IReceiptRegisterDetailDAL.SelectBy(u => u.ReceiptId == item.ReceiptId && u.WhCode == item.WhCode && u.InOrderDetailId == item.InBoundOrderDetailId);
                ReceiptRegisterDetail entityDetail = new ReceiptRegisterDetail();
                if (list.Count == 0)
                {
                    entityDetail.ReceiptId = item.ReceiptId;
                    entityDetail.InOrderDetailId = item.InBoundOrderDetailId;
                    entityDetail.WhCode = item.WhCode;
                    entityDetail.PoId = item.PoId;
                    entityDetail.ItemId = item.ItemId;
                    entityDetail.UnitId = item.UnitId;
                    entityDetail.UnitName = item.UnitName;
                    entityDetail.CustomerPoNumber = item.CustomerPoNumber;
                    entityDetail.AltItemNumber = item.AltItemNumber;
                    entityDetail.RegQty = item.RegQty;
                    entityDetail.CreateUser = item.CreateUser;
                    entityDetail.CreateDate = item.CreateDate;
                    idal.IReceiptRegisterDetailDAL.Add(entityDetail);
                }
                else
                {
                    entityDetail = list.First();
                    entityDetail.RegQty = entityDetail.RegQty + item.RegQty;
                    entityDetail.UpdateUser = item.CreateUser;
                    entityDetail.UpdateDate = item.CreateDate;
                    idal.IReceiptRegisterDetailDAL.UpdateBy(entityDetail, u => u.Id == entityDetail.Id, "RegQty");
                }

                InBoundOrderDetail inBoundOrderDetail = idal.IInBoundOrderDetailDAL.SelectBy(u => u.Id == item.InBoundOrderDetailId).First();
                inBoundOrderDetail.RegQty = inBoundOrderDetail.RegQty + item.RegQty;
                inBoundOrderDetail.UpdateUser = item.CreateUser;
                inBoundOrderDetail.UpdateDate = item.CreateDate;
                idal.IInBoundOrderDetailDAL.UpdateBy(inBoundOrderDetail, u => u.Id == item.InBoundOrderDetailId, "RegQty");
            }
            idal.IInBoundOrderDetailDAL.SaveChanges();

            return "Y";
        }


        //创建收货批次
        private void ReceiptRegisterAdd(EditInBoundResult entity, ReceiptRegister receiptRegister, string receipt)
        {
            ReceiptRegister receiptReg = new ReceiptRegister();
            receiptReg.WhCode = receiptRegister.WhCode;
            receiptReg.ReceiptId = receipt;
            receiptReg.ClientId = entity.ClientId;
            receiptReg.ClientCode = entity.ClientCode;
            receiptReg.RegisterDate = receiptRegister.RegisterDate;
            receiptReg.ReceiptType = receiptRegister.ReceiptType;
            receiptReg.Status = receiptRegister.Status;
            receiptReg.ProcessId = receiptRegister.ProcessId;
            receiptReg.ProcessName = receiptRegister.ProcessName;
            receiptReg.HoldOutBoundOrderId = receiptRegister.HoldOutBoundOrderId;
            receiptReg.LocationId = receiptRegister.LocationId;

            receiptReg.PrintDate = receiptRegister.PrintDate;
            receiptReg.TransportType = receiptRegister.TransportType;
            receiptReg.TruckNumber = receiptRegister.TruckNumber;
            receiptReg.PhoneNumber = receiptRegister.PhoneNumber;
            receiptReg.DSFLag = receiptRegister.DSFLag;
            receiptReg.LoadMasterId = receiptRegister.LoadMasterId;
            receiptReg.OutBoundOrderId = receiptRegister.OutBoundOrderId;

            receiptReg.SumQty = receiptRegister.SumQty;
            receiptReg.BkDate = receiptRegister.BkDate;
            receiptReg.ArriveDate = receiptRegister.ArriveDate;
            receiptReg.ParkingDate = receiptRegister.ParkingDate;
            receiptReg.StorageDate = receiptRegister.StorageDate;
            receiptReg.DepartureDate = receiptRegister.DepartureDate;
            receiptReg.BeginReceiptDate = receiptRegister.BeginReceiptDate;
            receiptReg.EndReceiptDate = receiptRegister.EndReceiptDate;
            receiptReg.CreateUser = receiptRegister.CreateUser;
            receiptReg.CreateDate = receiptRegister.CreateDate;

            idal.IReceiptRegisterDAL.Add(receiptReg);
        }

        #endregion

        #region 9.预约单批量导入预录入 同时生成收货操作单

        public string ImportsInBoundOrderAndReceiptByOrder(List<InBoundOrderInsert> entityList)
        {
            string result = "";     //执行总结果

            if (entityList == null)
            {
                return "数据有误，请重新操作！";
            }

            InBoundOrderInsert fir = entityList.First();
            string whCode = fir.WhCode;
            string truckNumber = fir.TruckNumber;
            string TruckLength = fir.TruckLength;
            string BookOrigin = fir.BookOrigin;  //add by yangxin 2023-05-12 预约订单来源
            int? truckCount = fir.TruckCount;
            int? dnCount = (fir.DNCount == null ? 1 : fir.DNCount);
            int? faxInCount = (fir.FaxInCount == null ? 0 : fir.FaxInCount);
            int? faxOutCount = (fir.FaxOutCount == null ? 0 : fir.FaxOutCount);
            int? billCount = (fir.BillCount == null ? 0 : fir.BillCount);
            int? greenPassFlag = (fir.GreenPassFlag == null ? 0 : fir.GreenPassFlag);
            string goodType = fir.GoodType;

            if (string.IsNullOrEmpty(fir.CreateUser))
            {
                return "创建人为空，EIP请重新登录！";
            }

            List<WhClient> clientList = idal.IWhClientDAL.SelectBy(u => u.WhCode == whCode && u.Status == "Active");

            var checkclient = clientList.Where(u => u.ClientCode == fir.ClientCode && u.WhCode == fir.WhCode && u.Status == "Active");
            if (checkclient.Count() == 0)
            {
                return "客户不存在！" + fir.ClientCode;
            }

            WhClient client = checkclient.First();
            List<FlowHead> flowHeadList = (from a in idal.IWhClientDAL.SelectAll()
                                           join b in idal.IR_Client_FlowRuleDAL.SelectAll()
                                           on a.Id equals b.ClientId
                                           join c in idal.IFlowHeadDAL.SelectAll()
                                           on b.BusinessFlowGroupId equals c.Id
                                           where a.WhCode == whCode && c.Type == "InBound" && a.Id == client.Id
                                           select c).ToList();
            if (flowHeadList.Count == 0)
            {
                return "客户收货流程未配置！" + client.ClientCode;
            }

            FlowHead flowhead = flowHeadList.First();

            List<ReceiptRegisterInsert> listEntity = new List<ReceiptRegisterInsert>();     //收货登记明细列表

            string receipt = "EI" + DI.IDGenerator.NewId;

            List<string> soList = new List<string>();
            List<string> poList = new List<string>();
            List<string> skuList = new List<string>();

            //批量导入预录入SO
            List<InBoundSO> InBoundSOAddList = new List<InBoundSO>();
            foreach (var entity in entityList)
            {
                if (!string.IsNullOrEmpty(entity.SoNumber))
                {
                    entity.ClientId = client.Id;
                    entity.ProcessId = flowhead.Id;
                    entity.ProcessName = flowhead.FlowName;

                    var ChecklistInBoundSO = idal.IInBoundSODAL.SelectBy(u => u.WhCode == entity.WhCode && u.ClientCode == entity.ClientCode && u.SoNumber == entity.SoNumber);

                    if (ChecklistInBoundSO.Count() == 0)
                    {
                        InBoundSO inBoundSO = new InBoundSO();
                        inBoundSO.WhCode = entity.WhCode;
                        inBoundSO.SoNumber = entity.SoNumber;
                        inBoundSO.ClientCode = entity.ClientCode;
                        inBoundSO.ClientId = (int)entity.ClientId;              //添加新数据 必须赋予客户ID
                        inBoundSO.CreateUser = fir.CreateUser;
                        inBoundSO.CreateDate = DateTime.Now;
                        InBoundSOAddList.Add(inBoundSO);
                    }
                    soList.Add(entity.SoNumber);
                }
            }

            if (InBoundSOAddList.Count > 0)
            {
                idal.IInBoundSODAL.Add(InBoundSOAddList);
                idal.IInBoundSODAL.SaveChanges();
            }

            List<InBoundSO> listInBoundSO = idal.IInBoundSODAL.SelectBy(u => u.WhCode == fir.WhCode && soList.Contains(u.SoNumber));

            List<InBoundOrder> InBoundOrderAddList = new List<InBoundOrder>();
            List<ItemMaster> ItemMasterAddList = new List<ItemMaster>();

            List<InBoundOrder> checkInBoundOrderAddResult = new List<InBoundOrder>();
            List<ItemMaster> checkItemMasterAddResult = new List<ItemMaster>();

            //批量导入预录入PO
            foreach (var entity in entityList)
            {
                entity.ClientId = client.Id;
                entity.ProcessId = flowhead.Id;
                entity.ProcessName = flowhead.FlowName;

                if (!string.IsNullOrEmpty(entity.SoNumber))
                {
                    InBoundSO inBoundSO = listInBoundSO.Where(u => u.SoNumber == entity.SoNumber && u.ClientCode == entity.ClientCode).First();

                    string[] getPoArr = (from a in entity.InBoundOrderDetailInsert select a.CustomerPoNumber).ToList().Distinct().ToArray();

                    List<InBoundOrder> checkPoArr = idal.IInBoundOrderDAL.SelectBy(u => getPoArr.Contains(u.CustomerPoNumber) && u.WhCode == entity.WhCode && u.ClientCode == entity.ClientCode && u.SoId == inBoundSO.Id);

                    string[] getSkuArr = (from a in entity.InBoundOrderDetailInsert select a.AltItemNumber).ToList().Distinct().ToArray();

                    List<ItemMaster> checkSkuArr = idal.IItemMasterDAL.SelectBy(u => u.WhCode == entity.WhCode && getSkuArr.Contains(u.AltItemNumber) && u.ClientId == entity.ClientId).OrderBy(u => u.Id).ToList();

                    foreach (var item in entity.InBoundOrderDetailInsert)
                    {
                        if (checkInBoundOrderAddResult.Where(u => u.SoId == inBoundSO.Id && u.CustomerPoNumber == item.CustomerPoNumber).Count() == 0)
                        {
                            if (checkPoArr.Where(u => u.CustomerPoNumber == item.CustomerPoNumber && u.WhCode == entity.WhCode && u.ClientCode == entity.ClientCode && u.SoId == inBoundSO.Id).Count() == 0)
                            {
                                InBoundOrder inBoundOrder = new InBoundOrder();
                                inBoundOrder.WhCode = entity.WhCode;
                                inBoundOrder.SoId = inBoundSO.Id;
                                inBoundOrder.PoNumber = "ST" + DI.IDGenerator.NewId;
                                inBoundOrder.CustomerPoNumber = item.CustomerPoNumber;
                                inBoundOrder.AltCustomerPoNumber = item.AltCustomerPoNumber;
                                inBoundOrder.ClientId = (int)entity.ClientId;
                                inBoundOrder.ClientCode = entity.ClientCode;
                                inBoundOrder.OrderType = entity.OrderType;
                                inBoundOrder.ProcessId = entity.ProcessId;
                                inBoundOrder.ProcessName = entity.ProcessName;
                                inBoundOrder.OrderSource = "WMS";
                                inBoundOrder.CreateUser = item.CreateUser;
                                inBoundOrder.CreateDate = DateTime.Now;
                                InBoundOrderAddList.Add(inBoundOrder);
                            }
                            poList.Add(item.CustomerPoNumber);

                            InBoundOrder inboundResult = new InBoundOrder();
                            inboundResult.SoId = inBoundSO.Id;
                            inboundResult.CustomerPoNumber = item.CustomerPoNumber;
                            checkInBoundOrderAddResult.Add(inboundResult);
                        }

                        if (checkItemMasterAddResult.Where(u => u.AltItemNumber == item.AltItemNumber && (u.Style1 == null ? "" : u.Style1) == (item.Style1 == null ? "" : item.Style1) && (u.Style2 == null ? "" : u.Style2) == (item.Style2 == null ? "" : item.Style2) && (u.Style3 == null ? "" : u.Style3) == (item.Style3 == null ? "" : item.Style3) && u.ClientId == entity.ClientId).Count() == 0)
                        {
                            if (checkSkuArr.Where(u => u.WhCode == entity.WhCode && u.AltItemNumber == item.AltItemNumber && (u.Style1 == null ? "" : u.Style1) == (item.Style1 == null ? "" : item.Style1) && (u.Style2 == null ? "" : u.Style2) == (item.Style2 == null ? "" : item.Style2) && (u.Style3 == null ? "" : u.Style3) == (item.Style3 == null ? "" : item.Style3) && u.ClientId == entity.ClientId).Count() == 0)
                            {
                                ItemMaster itemMaster = new ItemMaster();
                                itemMaster.ItemNumber = "SYS" + DI.IDGenerator.NewId;
                                itemMaster.WhCode = entity.WhCode;
                                itemMaster.AltItemNumber = item.AltItemNumber;
                                itemMaster.ClientId = (int)entity.ClientId;
                                itemMaster.ClientCode = entity.ClientCode;
                                itemMaster.Style1 = item.Style1 ?? "";
                                itemMaster.Style2 = item.Style2 ?? "";
                                itemMaster.Style3 = item.Style3 ?? "";
                                itemMaster.UnitFlag = 0;
                                if (item.UnitName == "" || item.UnitName == null)
                                {
                                    itemMaster.UnitName = "none";
                                }
                                else
                                {
                                    itemMaster.UnitName = item.UnitName;
                                }

                                itemMaster.CreateUser = item.CreateUser;
                                itemMaster.CreateDate = DateTime.Now;
                                ItemMasterAddList.Add(itemMaster);   //款号不存在就新增
                            }
                            skuList.Add(item.AltItemNumber);

                            ItemMaster itemResult = new ItemMaster();
                            itemResult.ClientId = (int)entity.ClientId;
                            itemResult.AltItemNumber = item.AltItemNumber;
                            itemResult.Style1 = item.Style1 ?? "";
                            itemResult.Style2 = item.Style2 ?? "";
                            itemResult.Style3 = item.Style3 ?? "";
                            checkItemMasterAddResult.Add(itemResult);
                        }

                    }
                }
                else
                {
                    string[] getPoArr = (from a in entity.InBoundOrderDetailInsert select a.CustomerPoNumber).ToList().Distinct().ToArray();

                    List<InBoundOrder> checkPoArr = idal.IInBoundOrderDAL.SelectBy(u => getPoArr.Contains(u.CustomerPoNumber) && u.WhCode == entity.WhCode && u.ClientCode == entity.ClientCode && u.SoId == null);

                    string[] getSkuArr = (from a in entity.InBoundOrderDetailInsert select a.AltItemNumber).ToList().Distinct().ToArray();

                    List<ItemMaster> checkSkuArr = idal.IItemMasterDAL.SelectBy(u => u.WhCode == entity.WhCode && getSkuArr.Contains(u.AltItemNumber) && u.ClientId == entity.ClientId).OrderBy(u => u.Id).ToList();

                    foreach (var item in entity.InBoundOrderDetailInsert)
                    {
                        if (checkInBoundOrderAddResult.Where(u => u.SoId == null && u.CustomerPoNumber == item.CustomerPoNumber).Count() == 0)
                        {
                            if (checkPoArr.Where(u => u.CustomerPoNumber == item.CustomerPoNumber && u.WhCode == entity.WhCode && u.ClientCode == entity.ClientCode && u.SoId == null).Count() == 0)
                            {
                                InBoundOrder inBoundOrder = new InBoundOrder();
                                inBoundOrder.WhCode = entity.WhCode;
                                inBoundOrder.PoNumber = "ST" + DI.IDGenerator.NewId;
                                inBoundOrder.CustomerPoNumber = item.CustomerPoNumber;
                                inBoundOrder.AltCustomerPoNumber = item.AltCustomerPoNumber;
                                inBoundOrder.ClientId = (int)entity.ClientId;
                                inBoundOrder.ClientCode = entity.ClientCode;
                                inBoundOrder.OrderType = entity.OrderType;
                                inBoundOrder.ProcessId = entity.ProcessId;
                                inBoundOrder.ProcessName = entity.ProcessName;
                                inBoundOrder.OrderSource = "WMS";
                                inBoundOrder.CreateUser = item.CreateUser;
                                inBoundOrder.CreateDate = DateTime.Now;
                                InBoundOrderAddList.Add(inBoundOrder);
                            }

                            poList.Add(item.CustomerPoNumber);

                            InBoundOrder inboundResult = new InBoundOrder();
                            inboundResult.SoId = null;
                            inboundResult.CustomerPoNumber = item.CustomerPoNumber;
                            checkInBoundOrderAddResult.Add(inboundResult);
                        }

                        if (checkItemMasterAddResult.Where(u => u.AltItemNumber == item.AltItemNumber && (u.Style1 == null ? "" : u.Style1) == (item.Style1 == null ? "" : item.Style1) && (u.Style2 == null ? "" : u.Style2) == (item.Style2 == null ? "" : item.Style2) && (u.Style3 == null ? "" : u.Style3) == (item.Style3 == null ? "" : item.Style3) && u.ClientId == entity.ClientId).Count() == 0)
                        {
                            if (checkSkuArr.Where(u => u.WhCode == entity.WhCode && u.AltItemNumber == item.AltItemNumber && (u.Style1 == null ? "" : u.Style1) == (item.Style1 == null ? "" : item.Style1) && (u.Style2 == null ? "" : u.Style2) == (item.Style2 == null ? "" : item.Style2) && (u.Style3 == null ? "" : u.Style3) == (item.Style3 == null ? "" : item.Style3) && u.ClientId == entity.ClientId).Count() == 0)
                            {
                                ItemMaster itemMaster = new ItemMaster();
                                itemMaster.ItemNumber = "SYS" + DI.IDGenerator.NewId;
                                itemMaster.WhCode = entity.WhCode;
                                itemMaster.AltItemNumber = item.AltItemNumber;
                                itemMaster.ClientId = (int)entity.ClientId;
                                itemMaster.ClientCode = entity.ClientCode;
                                itemMaster.Style1 = item.Style1 ?? "";
                                itemMaster.Style2 = item.Style2 ?? "";
                                itemMaster.Style3 = item.Style3 ?? "";
                                itemMaster.UnitFlag = 0;
                                if (item.UnitName == "" || item.UnitName == null)
                                {
                                    itemMaster.UnitName = "none";
                                }
                                else
                                {
                                    itemMaster.UnitName = item.UnitName;
                                }

                                itemMaster.CreateUser = item.CreateUser;
                                itemMaster.CreateDate = DateTime.Now;
                                ItemMasterAddList.Add(itemMaster);   //款号不存在就新增
                            }
                            skuList.Add(item.AltItemNumber);

                            ItemMaster itemResult = new ItemMaster();
                            itemResult.ClientId = (int)entity.ClientId;
                            itemResult.AltItemNumber = item.AltItemNumber;
                            itemResult.Style1 = item.Style1 ?? "";
                            itemResult.Style2 = item.Style2 ?? "";
                            itemResult.Style3 = item.Style3 ?? "";
                            checkItemMasterAddResult.Add(itemResult);
                        }
                    }
                }
            }

            if (InBoundOrderAddList.Count > 0)
            {
                idal.IInBoundOrderDAL.Add(InBoundOrderAddList);
                idal.IInBoundSODAL.SaveChanges();
            }

            if (ItemMasterAddList.Count > 0)
            {
                idal.IItemMasterDAL.Add(ItemMasterAddList);
                idal.IItemMasterDAL.SaveChanges();
            }

            List<InBoundOrder> listInBoundOrder = idal.IInBoundOrderDAL.SelectBy(u => poList.Contains(u.CustomerPoNumber) && u.WhCode == fir.WhCode);

            List<ItemMaster> listItemMaster = idal.IItemMasterDAL.SelectBy(u => skuList.Contains(u.AltItemNumber) && u.WhCode == fir.WhCode).OrderBy(u => u.Id).ToList();

            foreach (var entity in entityList)
            {
                entity.ClientId = client.Id;
                entity.ProcessId = flowhead.Id;
                entity.ProcessName = flowhead.FlowName;

                foreach (var item in entity.InBoundOrderDetailInsert)
                {
                    InBoundOrder inBoundOrder = new InBoundOrder();

                    if (!string.IsNullOrEmpty(entity.SoNumber))
                    {
                        InBoundSO inBoundSO = listInBoundSO.Where(u => u.SoNumber == entity.SoNumber && u.ClientCode == entity.ClientCode).First();

                        inBoundOrder = listInBoundOrder.Where(u => u.CustomerPoNumber == item.CustomerPoNumber && u.ClientCode == entity.ClientCode && u.SoId == inBoundSO.Id).First();
                    }
                    else
                    {
                        inBoundOrder = listInBoundOrder.Where(u => u.CustomerPoNumber == item.CustomerPoNumber && u.ClientCode == entity.ClientCode && u.SoId == null).First();
                    }

                    //判断款号是否存在
                    ItemMaster itemMaster = listItemMaster.Where(u => u.WhCode == entity.WhCode && u.AltItemNumber == item.AltItemNumber && (u.Style1 == null ? "" : u.Style1) == (item.Style1 == null ? "" : item.Style1) && (u.Style2 == null ? "" : u.Style2) == (item.Style2 == null ? "" : item.Style2) && (u.Style3 == null ? "" : u.Style3) == (item.Style3 == null ? "" : item.Style3) && u.ClientId == entity.ClientId).First();

                    //添加InBoundOrderDetail
                    var listInBoundOrderDetail = idal.IInBoundOrderDetailDAL.SelectBy(u => u.WhCode == entity.WhCode && u.ItemId == itemMaster.Id && u.PoId == inBoundOrder.Id);

                    InBoundOrderDetail inBoundOrderDetail = new InBoundOrderDetail();
                    int insertResult = 0;
                    if (listInBoundOrderDetail.Count() == 0)
                    {
                        inBoundOrderDetail.WhCode = entity.WhCode;
                        inBoundOrderDetail.PoId = inBoundOrder.Id;
                        inBoundOrderDetail.ItemId = itemMaster.Id;
                        inBoundOrderDetail.UnitId = item.UnitId;
                        if (itemMaster.UnitName == "" || itemMaster.UnitName == null)
                        {
                            inBoundOrderDetail.UnitName = "none";
                        }
                        else
                        {
                            if (itemMaster.UnitFlag == 1)
                            {
                                inBoundOrderDetail.UnitName = item.UnitName;
                            }
                            else
                            {
                                inBoundOrderDetail.UnitName = itemMaster.UnitName;
                            }
                        }

                        inBoundOrderDetail.Qty = item.Qty;
                        inBoundOrderDetail.Weight = item.Weight;
                        inBoundOrderDetail.CBM = item.CBM;
                        inBoundOrderDetail.CreateUser = item.CreateUser;
                        inBoundOrderDetail.CreateDate = DateTime.Now;
                        inBoundOrderDetail.CustomNumber1 = item.CustomNumber1;
                        idal.IInBoundOrderDetailDAL.Add(inBoundOrderDetail);
                        insertResult = 1;
                    }
                    else
                    {
                        inBoundOrderDetail = listInBoundOrderDetail.ToList().First();
                        int yuluruQty = inBoundOrderDetail.Qty - inBoundOrderDetail.RegQty;
                        if (yuluruQty < item.Qty)
                        {
                            inBoundOrderDetail.Qty = inBoundOrderDetail.Qty + (item.Qty - yuluruQty);
                            inBoundOrderDetail.UpdateUser = item.CreateUser;
                            inBoundOrderDetail.UpdateDate = DateTime.Now;
                            idal.IInBoundOrderDetailDAL.UpdateBy(inBoundOrderDetail, u => u.Id == inBoundOrderDetail.Id, new string[] { "Qty", "UpdateUser", "UpdateDate" });
                        }
                        insertResult = 2;
                    }

                    idal.IInBoundOrderDetailDAL.SaveChanges();

                    if (insertResult == 1 || insertResult == 2)
                    {
                        ReceiptRegisterInsert receiptRegisterInsert = new ReceiptRegisterInsert();
                        receiptRegisterInsert.WhCode = whCode;
                        receiptRegisterInsert.ReceiptId = receipt;
                        receiptRegisterInsert.InBoundOrderDetailId = inBoundOrderDetail.Id;
                        receiptRegisterInsert.CustomerPoNumber = item.CustomerPoNumber;
                        receiptRegisterInsert.AltItemNumber = item.AltItemNumber;
                        receiptRegisterInsert.PoId = inBoundOrder.Id;
                        receiptRegisterInsert.ItemId = itemMaster.Id;
                        receiptRegisterInsert.UnitName = itemMaster.UnitName;
                        receiptRegisterInsert.UnitId = item.UnitId;
                        receiptRegisterInsert.ProcessName = entity.ProcessName;
                        receiptRegisterInsert.ProcessId = (Int32)entity.ProcessId;

                        receiptRegisterInsert.RegQty = item.Qty;
                        receiptRegisterInsert.CreateUser = item.CreateUser;
                        receiptRegisterInsert.CreateDate = DateTime.Now;
                        listEntity.Add(receiptRegisterInsert);
                    }
                }
            }

            if (listEntity.Count == 0)
            {
                return "导入登记明细出错！";
            }

            IEnumerable<WhZoneResult> list = reg.RecZoneSelect(whCode, client.Id);
            string locationId = "";
            if (list != null)
            {
                locationId = list.First().ZoneName;
            }

            //创建收货批次
            ReceiptRegister receiptReg = new ReceiptRegister();
            receiptReg.WhCode = whCode;
            receiptReg.ClientId = client.Id;
            receiptReg.ClientCode = client.ClientCode;
            receiptReg.ReceiptType = "CFS";
            receiptReg.TruckNumber = truckNumber;
            receiptReg.TruckLength = TruckLength;
            receiptReg.BookOrigin = BookOrigin; //add by yangxin 2023-05-12 预约订单来源
            receiptReg.TruckCount = truckCount;
            receiptReg.DNCount = dnCount;
            receiptReg.FaxInCount = faxInCount;
            receiptReg.FaxOutCount = faxOutCount;
            receiptReg.BillCount = billCount;
            receiptReg.GreenPassFLag = greenPassFlag;
            receiptReg.GoodType = goodType;

            if (!string.IsNullOrEmpty(fir.BkDate))
            {
                receiptReg.BkDate = fir.BkDate;

                receiptReg.BkDateBegin = Convert.ToDateTime(fir.BkDate.Substring(0, 13) + ":00:00");
                if (fir.BkDate.Substring(17, 2) == "24")
                {
                    receiptReg.BkDateEnd = Convert.ToDateTime(fir.BkDate.Substring(0, 11) + " 23:59:59");
                }
                else
                {
                    receiptReg.BkDateEnd = Convert.ToDateTime(fir.BkDate.Substring(0, 11) + fir.BkDate.Substring(17, 2) + ":00:00");
                }
            }
            receiptReg.LocationId = locationId;
            receiptReg.ArriveDate = DateTime.Now;
            receiptReg.CreateUser = fir.CreateUser;
            receiptReg.PhoneNumber = fir.PhoneNumber;
            receiptReg.ReceiptId = receipt;
            receiptReg.Status = "N";
            receiptReg.DSFLag = 0;
            receiptReg.LoadMasterId = 0;
            receiptReg.OutBoundOrderId = 0;
            receiptReg.HoldOutBoundOrderId = 0;
            receiptReg.RegisterDate = DateTime.Now;
            receiptReg.CreateDate = DateTime.Now;

            idal.IReceiptRegisterDAL.Add(receiptReg);

            idal.SaveChanges();

            //添加收货批次明细
            reg.AddReceiptRegisterDetail1(listEntity);

            idal.SaveChanges();

            //释放收货批次
            string s = reg.ReleaseReceipt(receiptReg.WhCode, receiptReg.ReceiptId);

            idal.SaveChanges();

            if (s.Substring(0, s.IndexOf('$')) == "Y1")
            {
                return receiptReg.ReceiptId;
            }
            else if (s.Substring(0, s.IndexOf('$')) == "Y")
            {
                //带直装的

                var sql = from a in idal.IReceiptRegisterDAL.SelectAll()
                          where a.WhCode == whCode && a.ReceiptId == receiptReg.ReceiptId || a.TransportType == receiptReg.ReceiptId
                          select a;

                string rec = "";
                foreach (var item in sql)
                {
                    rec += item.ReceiptId + ",";
                }

                return rec.Substring(0, rec.Length - 1);
            }
            else
            {
                return s;
            }

        }

        #endregion


        #region 10.入库导入SN

        public List<SerialNumberInOutExtend> InBoundOrderExtendList(InBoundOrderExntedSearch searchEntity, out int total)
        {
            var sql = from a in idal.ISerialNumberInOutExtendDAL.SelectAll()
                      where a.WhCode == searchEntity.WhCode
                      select a;

            if (!string.IsNullOrEmpty(searchEntity.ClientCode))
                sql = sql.Where(u => u.ClientCode == searchEntity.ClientCode);
            if (!string.IsNullOrEmpty(searchEntity.SoNumber))
                sql = sql.Where(u => u.SoNumber == searchEntity.SoNumber);
            if (!string.IsNullOrEmpty(searchEntity.AltItemNumber))
                sql = sql.Where(u => u.AltItemNumber == searchEntity.AltItemNumber);
            if (!string.IsNullOrEmpty(searchEntity.CartonId))
                sql = sql.Where(u => u.CartonId == searchEntity.CartonId);

            if (searchEntity.BeginCreateDate != null)
            {
                sql = sql.Where(u => u.CreateDate >= searchEntity.BeginCreateDate);
            }
            if (searchEntity.EndCreateDate != null)
            {
                sql = sql.Where(u => u.CreateDate <= searchEntity.EndCreateDate);
            }

            total = sql.Count();
            sql = sql.OrderByDescending(u => u.Id);
            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);
            return sql.ToList();
        }

        #endregion

        #region 11.导入Forecast

        public string InBoundOrderExtendLists(InBoundOrderExntedSearch searchEntity)
        {
            return "Y";
        }


        public string chec(InBoundOrderExntedSearch searchEntity)
        {
            return "Y";
        }


        //收货单位下拉菜单列表
        public IEnumerable<UnitDefault> UnitDefaultListSelect(string whCode)
        {
            var sql = from a in idal.IUnitDefaultDAL.SelectAll()
                      where a.WhCode == whCode
                      select a;
            sql = sql.OrderBy(u => u.Id);
            return sql.AsEnumerable();
        }


        //forecast 列表查询
        public List<ExcelImportInBound> ExcelImportInBoundList(ExcelImportInBoundSearch searchEntity, out int total, out string str)
        {
            var sql = from a in idal.IExcelImportInBoundDAL.SelectAll()

                          //where a.WhCode == searchEntity.WhCode
                      select a;

            if (!string.IsNullOrEmpty(searchEntity.SystemNumber))
                sql = sql.Where(u => u.SystemNumber == searchEntity.SystemNumber);
            if (!string.IsNullOrEmpty(searchEntity.PoNumber))
                sql = sql.Where(u => u.PoNumber == searchEntity.PoNumber);
            if (!string.IsNullOrEmpty(searchEntity.SoNumber))
                sql = sql.Where(u => u.SoNumber == searchEntity.SoNumber);
            if (!string.IsNullOrEmpty(searchEntity.Supplier))
                sql = sql.Where(u => u.Supplier == searchEntity.Supplier);
            if (!string.IsNullOrEmpty(searchEntity.ItemNumber))
                sql = sql.Where(u => u.ItemNumber == searchEntity.ItemNumber);

            if (!string.IsNullOrEmpty(searchEntity.Labeling))
                sql = sql.Where(u => u.Labeling.Contains(searchEntity.Labeling));

            if (!string.IsNullOrEmpty(searchEntity.PlatHeavyCargo))
                sql = sql.Where(u => u.PalletizationForHeavyCargo.Contains(searchEntity.PlatHeavyCargo));

            if (searchEntity.BeginDate != null)
            {
                sql = sql.Where(u => u.CreateDate >= searchEntity.BeginDate);
            }
            if (searchEntity.EndDate != null)
            {
                sql = sql.Where(u => u.CreateDate <= searchEntity.EndDate);
            }

            if (searchEntity.BeginConsDate != null)
            {
                sql = sql.Where(u => u.ConsDate >= searchEntity.BeginConsDate);
            }
            if (searchEntity.EndConsDate != null)
            {
                sql = sql.Where(u => u.ConsDate <= searchEntity.EndConsDate);
            }

            total = sql.Count();
            str = "";
            if (total > 0)
            {
                str = "{\"Booked CBM体积\":\"" + sql.Sum(u => u.BookedCBM).ToString() + "\",\"Booked Carton 箱数\":\"" + sql.Sum(u => u.BookedCarton).ToString() + "\"}";
            }
            sql = sql.OrderByDescending(u => u.Id);
            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);
            return sql.ToList();
        }

        //验证SO是否已存在
        public string checkSONumber(List<ExcelImportInBoundSo> checkSoList)
        {
            string result = "";
            Hashtable data = new Hashtable();   //去excel重复的SO、PO、款号
            if (checkSoList.Count <= 0)
            {
                result += "数据异常";
            }
            if (result == "")
            {
                string[] getSoArr = (from a in checkSoList select a.SoNumber).ToList().Distinct().ToArray();
                int k = 0;
                ExcelImportInBoundSo firsArrt = checkSoList.First();
                List<InBoundSO> checkSoArrList = idal.IInBoundSODAL.SelectBy(u => getSoArr.Contains(u.SoNumber) && u.ClientCode == firsArrt.ClientCode).ToList();

                if (checkSoArrList.Count > 0)
                {
                    result += "存在已预录入的SO:";
                    for (int i = 0; i < checkSoArrList.Count; i++)
                    {
                        if (!data.ContainsValue(checkSoArrList[i].SoNumber)) //Ecxel 
                        {
                            data.Add(k, checkSoArrList[i].SoNumber);
                            result += checkSoArrList[i].SoNumber + ",";
                            k += 1;
                        }

                    }
                    result += " 不允许导入";
                }


            }
            return result;
        }

        public string importForecast(List<ExcelImportInBound> checkSoList)
        {
            ExcelImportInBound first = checkSoList.First();

            string[] soArr = (from a in checkSoList
                              select a.SoNumber).Distinct().ToArray();

            List<InBoundOrderResult> getList = (from a in idal.IInBoundSODAL.SelectAll()
                                                join b in idal.IInBoundOrderDAL.SelectAll()
                                                on a.Id equals b.SoId
                                                join c in idal.IInBoundOrderDetailDAL.SelectAll()
                                                on b.Id equals c.PoId
                                                where soArr.Contains(a.SoNumber)
                                                && a.WhCode == first.WhCode
                                                && a.ClientCode == "Kmart"
                                                && c.RegQty != 0
                                                select new InBoundOrderResult
                                                {
                                                    SoNumber = a.SoNumber,
                                                    Id = b.Id
                                                }).Distinct().ToList();

            //List<InBoundOrderResult> getList = sql.ToList();

            foreach (var item in soArr)
            {
                if (getList.Where(u => u.SoNumber == item).Count() == 0)
                {
                    List<InBoundOrderResult> getPoList = getList.Where(u => u.SoNumber == item).ToList();
                    foreach (var itemPo in getPoList)
                    {
                        idal.IInBoundOrderDetailDAL.DeleteByExtended(u => u.PoId == itemPo.Id && u.WhCode == first.WhCode);
                        idal.IInBoundOrderDAL.DeleteByExtended(u => u.Id == itemPo.Id);
                    }
                }
                idal.IExcelImportInBoundDAL.DeleteByExtended(u => u.SoNumber == item);
                // idal.IExcelImportInBoundDAL.DeleteByExtended(u => u.SoNumber == item && u.WhCode == first.WhCode);
            }

            string systemNumber = "SYS" + DI.IDGenerator.NewId;
            int? sumQty = checkSoList.Sum(u => u.BookedCarton);
            foreach (var item in checkSoList)
            {
                item.SystemNumber = systemNumber;
            }

            idal.IExcelImportInBoundDAL.Add(checkSoList);
            idal.SaveChanges();
            return "Y$" + systemNumber + "$" + sumQty;
        }

        //Kmart 修改forecast数据
        public string ExcelImportInBoundEdit(ExcelImportInBound entity)
        {
            if (entity.Id == 0)
            {
                return "明细ID为空，修改有误！";
            }

            ExcelImportInBound orderDetail = idal.IExcelImportInBoundDAL.SelectBy(u => u.Id == entity.Id).First();
            //  if (orderDetail.RegQty != 0)
            //  {
            //      if (entity.Qty < orderDetail.RegQty)
            //      {
            //          return "预录入数量不能小于登记数量！";
            //      }
            //  }

            idal.IExcelImportInBoundDAL.UpdateBy(entity, u => u.Id == entity.Id, new string[] { "LabelingSend", "ExpressNumber", "DoesTheFactoryConfirm", "UpdateUser", "UpdateDate" });
            idal.IExcelImportInBoundDAL.SaveChanges();
            return "Y";
        }

        //Kmart 按照SO删除forecast和预录入
        public string ExcelImportInBoundDelete(string whCode, string clientCode, string soNumber)
        {

            List<InBoundOrder> inorderlist = (from a in idal.IInBoundSODAL.SelectAll()
                                              join b in idal.IInBoundOrderDAL.SelectAll()
                                              on a.Id equals b.SoId
                                              where a.SoNumber == soNumber && a.WhCode == whCode && a.ClientCode == clientCode
                                              select b).ToList();
            if (inorderlist.Count > 0)
            {
                if (!string.IsNullOrEmpty(soNumber))
                {
                    List<InBoundSO> inBoundSOList = idal.IInBoundSODAL.SelectBy(u => u.WhCode == whCode && u.SoNumber == soNumber && u.ClientCode == clientCode);
                    if (inBoundSOList.Count == 0)
                    {
                        return "预录入数据有误！";
                    }

                    int[] soId = (from a in inBoundSOList select a.Id).ToArray();

                    var poId = from a in inorderlist where soId.Contains((Int32)(a.SoId ?? 0)) select a.Id;

                    List<InBoundOrderDetail> inorderdetaillist = idal.IInBoundOrderDetailDAL.SelectBy(u => poId.Contains(u.PoId));
                    if (inorderdetaillist.Where(u => u.RegQty > 0).Count() > 0)
                    {
                        return "预录入已有收货登记，无法删除！";
                    }
                    else
                    {
                        //删除预录入要判断 是否有直装订单
                        var inorderdetailId = from b in inorderdetaillist select b.Id;
                        List<OutBoundOrderDetail> outOrderDetailList = idal.IOutBoundOrderDetailDAL.SelectBy(u => inorderdetailId.Contains((Int32)(u.InBoundOrderDetailId ?? 0)));
                        if (outOrderDetailList.Count > 0)
                        {
                            return "预录入包含直装订单，无法删除！";
                        }
                        else
                        {
                            idal.IInBoundOrderDetailDAL.DeleteByExtended(u => poId.Contains(u.PoId));
                            idal.IInBoundOrderDAL.DeleteByExtended(u => poId.Contains(u.Id));

                            List<InBoundOrder> checklist = idal.IInBoundOrderDAL.SelectBy(u => soId.Contains(u.SoId ?? 0));
                            if (checklist.Count == 0)
                            {
                                idal.IInBoundSODAL.DeleteByExtended(u => soId.Contains(u.Id));
                            }
                        }
                    }
                }
                else
                {
                    var poId = from a in inorderlist select a.Id;

                    List<InBoundOrderDetail> inorderdetaillist = idal.IInBoundOrderDetailDAL.SelectBy(u => poId.Contains(u.PoId));
                    if (inorderdetaillist.Where(u => u.RegQty > 0).Count() > 0)
                    {
                        return "预录入已有收货登记，无法删除！";
                    }
                    else
                    {
                        //删除预录入要判断 是否有直装订单
                        var inorderdetailId = from b in inorderdetaillist select b.Id;
                        List<OutBoundOrderDetail> outOrderDetailList = idal.IOutBoundOrderDetailDAL.SelectBy(u => inorderdetailId.Contains((Int32)(u.InBoundOrderDetailId ?? 0)));
                        if (outOrderDetailList.Count > 0)
                        {
                            return "预录入包含直装订单，无法删除！";
                        }
                        else
                        {
                            idal.IInBoundOrderDetailDAL.DeleteByExtended(u => poId.Contains(u.PoId));
                            idal.IInBoundOrderDAL.DeleteByExtended(u => poId.Contains(u.Id));
                        }
                    }
                }
            }

            //return "Y";

            idal.IExcelImportInBoundDAL.DeleteByExtended(u => u.SoNumber.Contains(soNumber));
            //ExcelImportInBound orderDetail = idal.IExcelImportInBoundDAL.SelectBy(u => u.Id == entity.Id).First();
            //  if (orderDetail.RegQty != 0)
            //  {
            //      if (entity.Qty < orderDetail.RegQty)
            //      {
            //          return "预录入数量不能小于登记数量！";
            //      }
            //  }


            return "Y";
        }

        #endregion

        #region 12.Kmart

        //Kmart 延时Reason
        public IEnumerable<KmartWaitingReason> KmartReasontSelect()
        {
            var sql = from a in idal.IKmartWaitingReasonDAL.SelectAll()
                      select a;
            sql = sql.OrderBy(u => u.Id);
            return sql.AsEnumerable();
        }

        //Kmart 收货延时服务
        public List<KmartReceiptDelay> KmartReceiptDelayList(ExcelImportInBoundSearch searchEntity, out int total, out string str)
        {
            string[] statusArr = new string[] { "A", "U", "C" };
            var sql = (from a in idal.IReceiptRegisterDAL.SelectAll()
                       join b in (from a in idal.IReceiptDAL.SelectAll()
                                  join c in idal.IExcelImportInBoundDAL.SelectAll()
                                  on new { SoNumber = a.SoNumber, PoNumber = a.CustomerPoNumber }
                                  equals new { SoNumber = c.SoNumber, PoNumber = c.PoNumber }
                                  //on new { SoNumber = a.SoNumber, PoNumber = a.CustomerPoNumber, WhCode = a.WhCode }
                                  //equals new { SoNumber = c.SoNumber, PoNumber = c.PoNumber, WhCode = c.WhCode }
                                  into tempex
                                  from c in tempex.DefaultIfEmpty()
                                  where a.ClientCode == "Kmart" && a.WhCode == searchEntity.WhCode
                                  group a by
                                  new
                                  {
                                      a.WhCode,
                                      a.ReceiptId,
                                      a.SoNumber,
                                      a.CustomerPoNumber,
                                      c.ConsDate,
                                      c.Supplier
                                  }
                             into g
                                  select new
                                  {
                                      PlaceOfDeparture = "SHA",
                                      WhCode = g.Key.WhCode,
                                      ReceiptId = g.Key.ReceiptId,
                                      SoNumber = g.Key.SoNumber,
                                      CustomerPoNumber = g.Key.CustomerPoNumber,
                                      Supplier = g.Key.Supplier,
                                      ConsDate = g.Key.ConsDate,
                                      TruckwaitingTime = "",
                                      WaitingDescription = "",
                                      UnloadingEfficiency = "",
                                      UnloadingDescription = "",
                                      CombinedWaitingTime = "",
                                      CombinedDescription = "",
                                  })
                  on new { a.ReceiptId } equals new { b.ReceiptId }
                  //on new { a.WhCode, a.ReceiptId } equals new { b.WhCode, b.ReceiptId }
                       join c in idal.IReceiptRegisterExtendDAL.SelectAll()
                       on new { a.ReceiptId } equals new { c.ReceiptId } into tempc
                       //on new { a.ReceiptId, a.WhCode } equals new { c.ReceiptId, c.WhCode } into tempc
                       from c in tempc.DefaultIfEmpty()
                       where a.ClientCode == "Kmart"
                      // where a.ClientCode == "Kmart" && a.WhCode == searchEntity.WhCode
                      && statusArr.Contains(a.Status)
                       select new KmartReceiptDelay
                       {
                           Id = a.Id.ToString(),
                           ReceiptId = b.ReceiptId,
                           PlaceOfDeparture = b.PlaceOfDeparture,
                           ConsDate = b.ConsDate,
                           Supplier = b.Supplier,
                           SoNumber = b.SoNumber,
                           TruckNumber = a.TruckNumber,
                           TransportType = a.TransportType,
                           BkDate = a.BkDate,

                           RegistrationDate = a.RegisterDate,
                           BeginDate = a.ParkingDate,
                           EndDate = a.EndReceiptDate,
                           ReasonCode1 = c.HoldReason1 ?? "",
                           ReasonCode2 = c.HoldReason2 ?? "",
                           ReasonCode3 = c.HoldReason3 ?? "",
                           Reason1 = c.HoldReason1 ?? "",
                           Reason2 = c.HoldReason2 ?? "",
                           Reason3 = c.HoldReason3 ?? "",
                           BkDateBegin = a.BkDateBegin,
                           BkDateEnd = a.BkDateEnd
                       }).Distinct();

            if (!string.IsNullOrEmpty(searchEntity.SoNumber))
                sql = sql.Where(u => u.SoNumber == searchEntity.SoNumber);
            if (!string.IsNullOrEmpty(searchEntity.Supplier))
                sql = sql.Where(u => u.Supplier == searchEntity.Supplier);
            if (!string.IsNullOrEmpty(searchEntity.ReceiptId))
                sql = sql.Where(u => u.ReceiptId == searchEntity.ReceiptId);

            if (searchEntity.BeginDate != null)
            {
                sql = sql.Where(u => u.RegistrationDate >= searchEntity.BeginDate);
            }
            if (searchEntity.EndDate != null)
            {
                sql = sql.Where(u => u.RegistrationDate <= searchEntity.EndDate);
            }

            if (searchEntity.BeginConsDate != null)
            {
                sql = sql.Where(u => u.ConsDate >= searchEntity.BeginConsDate);
            }
            if (searchEntity.EndConsDate != null)
            {
                sql = sql.Where(u => u.ConsDate <= searchEntity.EndConsDate);
            }

            List<KmartReceiptDelay> list = sql.ToList();

            List<KmartReceiptDelay> list1 = new List<KmartReceiptDelay>();
            List<KmartWaitingReason> kmartreasonList = idal.IKmartWaitingReasonDAL.SelectAll().ToList();
            foreach (var item in list)
            {
                if (!string.IsNullOrEmpty(item.ReasonCode1))
                {
                    KmartWaitingReason Reason1 = kmartreasonList.Where(u => u.ReasonCode == item.ReasonCode1).First();
                    item.ReasonCode1 = Reason1.Comments;
                }
                if (!string.IsNullOrEmpty(item.ReasonCode2))
                {
                    KmartWaitingReason Reason2 = kmartreasonList.Where(u => u.ReasonCode == item.ReasonCode2).First();
                    item.ReasonCode2 = Reason2.Comments;
                }
                if (!string.IsNullOrEmpty(item.ReasonCode3))
                {
                    KmartWaitingReason Reason3 = kmartreasonList.Where(u => u.ReasonCode == item.ReasonCode3).First();
                    item.ReasonCode3 = Reason3.Comments;
                }


                TimeSpan d3 = Convert.ToDateTime(item.BeginDate == null ? DateTime.Now : item.BeginDate).Subtract(Convert.ToDateTime(item.RegistrationDate == null ? DateTime.Now : item.RegistrationDate));

                double getmin = d3.TotalMinutes;
                item.TruckwaitingTime = Math.Round(Convert.ToDouble(Convert.ToDouble(getmin) / 60), 3);

                if (item.TruckwaitingTime < 2)
                {
                    item.WaitingDescription = "0-2 Hours";
                }
                else if (item.TruckwaitingTime >= 2 && item.TruckwaitingTime < 4)
                {
                    item.WaitingDescription = "2-4 Hours";
                }
                else if (item.TruckwaitingTime >= 4 && item.TruckwaitingTime < 6)
                {
                    item.WaitingDescription = "4-6 Hours";
                }
                else if (item.TruckwaitingTime >= 6 && item.TruckwaitingTime < 8)
                {
                    item.WaitingDescription = "6-8 Hours";
                }
                else if (item.TruckwaitingTime >= 8)
                {
                    item.WaitingDescription = "8+ Hours";
                }

                TimeSpan d4 = Convert.ToDateTime(item.EndDate == null ? DateTime.Now : item.EndDate).Subtract(Convert.ToDateTime(item.BeginDate == null ? DateTime.Now : item.BeginDate));

                getmin = d4.TotalMinutes;
                item.UnloadingEfficiency = Math.Round(Convert.ToDouble(Convert.ToDouble(getmin) / 60), 3);

                if (item.UnloadingEfficiency < 2)
                {
                    item.UnloadingDescription = "0-2 Hours";
                }
                else if (item.UnloadingEfficiency >= 2 && item.UnloadingEfficiency < 4)
                {
                    item.UnloadingDescription = "2-4 Hours";
                }
                else if (item.UnloadingEfficiency >= 4 && item.UnloadingEfficiency < 6)
                {
                    item.UnloadingDescription = "4-6 Hours";
                }
                else if (item.UnloadingEfficiency >= 6 && item.UnloadingEfficiency < 8)
                {
                    item.UnloadingDescription = "6-8 Hours";
                }
                else if (item.UnloadingEfficiency >= 8)
                {
                    item.UnloadingDescription = "8+ Hours";
                }

                item.CombinedWaitingTime = item.TruckwaitingTime + item.UnloadingEfficiency;
                if (item.CombinedWaitingTime < 2)
                {
                    item.CombinedDescription = "0-2 Hours";
                }
                else if (item.CombinedWaitingTime >= 2 && item.CombinedWaitingTime < 4)
                {
                    item.CombinedDescription = "2-4 Hours";
                }
                else if (item.CombinedWaitingTime >= 4 && item.CombinedWaitingTime < 6)
                {
                    item.CombinedDescription = "4-6 Hours";
                }
                else if (item.CombinedWaitingTime >= 6 && item.CombinedWaitingTime < 8)
                {
                    item.CombinedDescription = "6-8 Hours";
                }
                else if (item.CombinedWaitingTime >= 8)
                {
                    item.CombinedDescription = "8+ Hours";
                }

                if (string.IsNullOrEmpty(item.BkDate))
                {
                    item.Appointment = "NOB";
                }
                else if (!string.IsNullOrEmpty(item.BkDate) && (item.RegistrationDate < item.BkDateBegin || item.RegistrationDate > item.BkDateEnd))
                {
                    item.Appointment = "OSB";
                }
                else if (item.RegistrationDate >= item.BkDateBegin && item.RegistrationDate <= item.BkDateEnd)
                {
                    item.Appointment = "WIB";
                }

                list1.Add(item);
            }

            if (!string.IsNullOrEmpty(searchEntity.TruckWaitingTime))
            {
                list1 = list1.Where(u => u.TruckwaitingTime > 2).ToList();
            }

            total = list1.Count;
            str = "";

            list1 = list1.OrderByDescending(u => u.ReceiptId).ToList();
            list1 = list1.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize).ToList();

            return list1;
        }

        //Kmart 收货延时原因维护
        public string KmartDelayReason(ReceiptRegisterExtend entity)
        {
            List<ReceiptRegisterExtend> SoL = idal.IReceiptRegisterExtendDAL.SelectBy(u => u.WhCode == entity.WhCode && u.ReceiptId == entity.ReceiptId);
            if (SoL.Count != 0)
            {
                ReceiptRegisterExtend inorderlist = idal.IReceiptRegisterExtendDAL.SelectBy(u => u.ReceiptId == entity.ReceiptId).First();
                idal.IReceiptRegisterExtendDAL.UpdateBy(entity, u => u.ReceiptId == entity.ReceiptId, new string[] { "HoldReason1", "HoldReason2", "HoldReason3", "UpdateUser", "UpdateDate" });
                idal.IReceiptRegisterExtendDAL.SaveChanges();
            }
            else
            {
                idal.IReceiptRegisterExtendDAL.Add(entity);
                idal.SaveChanges();

            }
            return "Y";
        }


        //Kmart打托列表查询
        public List<KmartHeavyPalletResult> KmartHeavyPalletList(KmartHeavyPalletSearch searchEntity, string[] soNumber, out int total)
        {
            var sql = from a in idal.IHuMasterDAL.SelectAll()
                      join b in idal.IHuDetailDAL.SelectAll()
                       //on new { a.WhCode, a.HuId } equals new { b.WhCode, b.HuId }
                       on new { a.HuId } equals new { b.HuId }
                      join c in idal.IItemMasterDAL.SelectAll() on new { ItemId = b.ItemId } equals new { ItemId = c.Id }
                      join d in idal.IExcelImportInBoundDAL.SelectAll()
                      //on new { b.WhCode, b.SoNumber, b.CustomerPoNumber, b.AltItemNumber }
                      //equals new { d.WhCode, d.SoNumber, CustomerPoNumber = d.PoNumber, AltItemNumber = d.ItemNumber } into d_join
                      on new { b.SoNumber, b.CustomerPoNumber, b.AltItemNumber }
                      equals new { d.SoNumber, CustomerPoNumber = d.PoNumber, AltItemNumber = d.ItemNumber } into d_join
                      from d in d_join.DefaultIfEmpty()
                      join e in (
                          (from a0 in idal.IHuDetailDAL.SelectAll()
                           join b1 in idal.IHuMasterDAL.SelectAll()
                           on new { a0.HuId } equals new { b1.HuId }
                           // on new { a0.WhCode, a0.HuId } equals new { b1.WhCode, b1.HuId }
                           where a0.ClientCode == "Kmart"
                           group new { a0, b1 } by new
                           {
                               a0.SoNumber,
                               a0.CustomerPoNumber,
                               a0.AltItemNumber,
                               a0.WhCode
                           } into g
                           select new
                           {
                               g.Key.SoNumber,
                               g.Key.CustomerPoNumber,
                               g.Key.AltItemNumber,
                               g.Key.WhCode,
                               recCBM = (System.Decimal?)g.Sum(p => p.a0.Qty * p.a0.Height * p.a0.Length * p.a0.Width)
                           }))
                             on new { b.SoNumber, b.CustomerPoNumber, b.AltItemNumber }
                        equals new { e.SoNumber, e.CustomerPoNumber, e.AltItemNumber } into e_join
                      //    on new { b.WhCode, b.SoNumber, b.CustomerPoNumber, b.AltItemNumber }
                      //equals new { e.WhCode, e.SoNumber, e.CustomerPoNumber, e.AltItemNumber } into e_join
                      from e in e_join.DefaultIfEmpty()
                      where b.ClientCode == "Kmart" && (c.Style1 ?? "") != ""
                      //where b.ClientCode == "Kmart" && a.WhCode == searchEntity.WhCode && (c.Style1 ?? "") != ""
                      select new KmartHeavyPalletResult
                      {
                          ClientCode = b.ClientCode,
                          ConsDate = d.ConsDate,
                          DCDD = d.DCDD,
                          Supplier = d.Supplier,
                          SoNumber = d.SoNumber,
                          PoNumber = d.PoNumber,
                          ReceiptDate = b.ReceiptDate,
                          ReceiptId = b.ReceiptId,
                          HuId = b.HuId,
                          LocationId = a.Location,
                          ItemNumber = d.ItemNumber,
                          PlaceOfDelivery = d.PlaceOfDelivery,
                          BookedCarton = d.BookedCarton,
                          Qty = b.Qty,
                          recCBM = e.recCBM,
                          Height = a.HuHeight,
                          Length = a.HuLength,
                          Width = a.HuWidth,
                          Show = "",
                          Show1 = ""
                      };

            if (!string.IsNullOrEmpty(searchEntity.HuId))
                sql = sql.Where(u => u.HuId == searchEntity.HuId);
            if (!string.IsNullOrEmpty(searchEntity.ReceiptId))
                sql = sql.Where(u => u.ReceiptId == searchEntity.ReceiptId);
            if (!string.IsNullOrEmpty(searchEntity.SoNumber))
                sql = sql.Where(u => u.SoNumber.Contains(searchEntity.SoNumber));
            if (!string.IsNullOrEmpty(searchEntity.CustomerPoNumber))
                sql = sql.Where(u => u.PoNumber.Contains(searchEntity.CustomerPoNumber));
            if (!string.IsNullOrEmpty(searchEntity.AltItemNumber))
                sql = sql.Where(u => u.ItemNumber.Contains(searchEntity.AltItemNumber));

            if (soNumber != null)
                sql = sql.Where(u => soNumber.Contains(u.SoNumber));

            if (searchEntity.BeginReceiptDate != null)
            {
                sql = sql.Where(u => u.ReceiptDate >= searchEntity.BeginReceiptDate);
            }
            if (searchEntity.EndReceiptDate != null)
            {
                sql = sql.Where(u => u.ReceiptDate <= searchEntity.EndReceiptDate);
            }

            if (searchEntity.BeginConsDate != null)
            {
                sql = sql.Where(u => u.ConsDate >= searchEntity.BeginConsDate);
            }
            if (searchEntity.EndConsDate != null)
            {
                sql = sql.Where(u => u.ConsDate <= searchEntity.EndConsDate);
            }

            total = sql.Count();
            sql = sql.OrderBy(u => u.SoNumber).ThenBy(u => u.PoNumber).ThenBy(u => u.ItemNumber);
            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);
            return sql.ToList();
        }

        //Kmart打托修改长宽高
        public string KmartHeavyPalletEdit(KmartHeavyPalletResult entity)
        {
            List<HuMaster> huMasterList = idal.IHuMasterDAL.SelectBy(u => u.WhCode == entity.WhCode && u.HuId == entity.HuId);

            if (huMasterList.Count == 0)
            {
                return "托盘不存在请重新查询！";
            }
            else
            {
                HuMaster huMaster = huMasterList.First();
                huMaster.HuLength = entity.Length;
                huMaster.HuWidth = entity.Width;
                huMaster.HuHeight = entity.Height;
                huMaster.UpdateDate = DateTime.Now;

                idal.IHuMasterDAL.UpdateBy(huMaster, u => u.WhCode == entity.WhCode && u.HuId == entity.HuId, new string[] { "HuLength", "HuWidth", "HuHeight", "UpdateUser", "UpdateDate" });
                idal.IHuMasterDAL.SaveChanges();
                return "Y";
            }
        }

        #endregion


        #region 13.Cotton导入Forecast


        //forecast 列表查询
        public List<ExcelImportInBoundCotton> CottonExcelImportInBoundList(ExcelImportInBoundSearch searchEntity, out int total, out string str)
        {
            var sql = from a in idal.IExcelImportInBoundCottonDAL.SelectAll()
                      where a.WhCode == searchEntity.WhCode
                      select a;

            if (!string.IsNullOrEmpty(searchEntity.SystemNumber))
                sql = sql.Where(u => u.SystemNumber == searchEntity.SystemNumber);
            if (!string.IsNullOrEmpty(searchEntity.PoNumber))
                sql = sql.Where(u => u.PoNumber == searchEntity.PoNumber);
            if (!string.IsNullOrEmpty(searchEntity.SoNumber))
                sql = sql.Where(u => u.SoNumber == searchEntity.SoNumber);
            if (!string.IsNullOrEmpty(searchEntity.ItemNumber))
                sql = sql.Where(u => u.ItemNumber == searchEntity.ItemNumber);

            if (!string.IsNullOrEmpty(searchEntity.ASOS))
                sql = sql.Where(u => u.ASOS.Contains(searchEntity.ASOS));
            if (!string.IsNullOrEmpty(searchEntity.ClothesHanger))
                sql = sql.Where(u => u.ClothesHanger.Contains(searchEntity.ClothesHanger));
            if (!string.IsNullOrEmpty(searchEntity.Hudson))
                sql = sql.Where(u => u.Hudson.Contains(searchEntity.Hudson));
            if (!string.IsNullOrEmpty(searchEntity.ShipeeziSO))
                sql = sql.Where(u => u.ShipeeziSO.Contains(searchEntity.ShipeeziSO));

            if (searchEntity.BeginDate != null)
            {
                sql = sql.Where(u => u.CreateDate >= searchEntity.BeginDate);
            }
            if (searchEntity.EndDate != null)
            {
                sql = sql.Where(u => u.CreateDate <= searchEntity.EndDate);
            }

            total = sql.Count();
            str = "";
            if (total > 0)
            {
                str = "{\"CTN\":\"" + sql.Sum(u => u.Qty).ToString() + "\"}";
            }
            sql = sql.OrderByDescending(u => u.Id);
            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);
            return sql.ToList();
        }

        public string ExcelImportForecastCotton(List<ExcelImportInBoundCotton> entityList)
        {
            string result = "";
            Hashtable data = new Hashtable();   //去excel重复的SO、PO、款号
            if (entityList.Count <= 0)
            {
                result += "数据异常！";
            }

            if (result != "")
            {
                return result;
            }

            ExcelImportInBoundCotton first = entityList.First();

            string[] soArr = (from a in entityList
                              select a.ShipeeziSO).Distinct().ToArray();

            var sql1 = (from a in idal.IInBoundSODAL.SelectAll()
                        join b in idal.IInBoundOrderDAL.SelectAll()
                        on a.Id equals b.SoId
                        join c in idal.IInBoundOrderDetailDAL.SelectAll()
                        on b.Id equals c.PoId
                        where soArr.Contains(a.SoNumber)
                        && a.WhCode == first.WhCode
                        && a.ClientCode == first.ClientCode
                        && c.RegQty > 0
                        select new InBoundOrderResult
                        {
                            SoNumber = a.SoNumber
                        }).Distinct();

            List<InBoundOrderResult> getList1 = sql1.ToList();

            string[] getRegSoArr = (from a in getList1
                                    select a.SoNumber).Distinct().ToArray();

            var sql2 = (from a in idal.IInBoundSODAL.SelectAll()
                        join b in idal.IInBoundOrderDAL.SelectAll()
                        on a.Id equals b.SoId
                        join c in idal.IInBoundOrderDetailDAL.SelectAll()
                        on b.Id equals c.PoId
                        where !getRegSoArr.Contains(a.SoNumber)
                        && a.WhCode == first.WhCode
                        && a.ClientCode == first.ClientCode
                        select new InBoundOrderResult
                        {
                            Id = b.Id,
                            SoNumber = a.SoNumber
                        }).Distinct();

            List<InBoundOrderResult> getList2 = sql2.ToList();

            foreach (var item in soArr)
            {
                if (getList1.Where(u => u.SoNumber == item).Count() == 0)
                {
                    List<InBoundOrderResult> getPoList = getList2.Where(u => u.SoNumber == item).ToList();
                    foreach (var itemPo in getPoList)
                    {
                        idal.IInBoundOrderDetailDAL.DeleteByExtended(u => u.PoId == itemPo.Id && u.WhCode == first.WhCode);
                        idal.IInBoundOrderDAL.DeleteByExtended(u => u.Id == itemPo.Id);
                    }
                }
            }

            idal.IExcelImportInBoundCottonDAL.DeleteByExtended(u => soArr.Contains(u.ShipeeziSO) && u.WhCode == first.WhCode);

            string systemNumber = "SYS" + DI.IDGenerator.NewId;
            int? sumQty = entityList.Sum(u => u.Qty);

            List<InBoundOrderInsert> entityInBoundList = new List<InBoundOrderInsert>();

            foreach (var item in entityList)
            {
                item.SystemNumber = systemNumber;

                if (entityInBoundList.Where(u => u.SoNumber == item.ShipeeziSO).Count() == 0)
                {
                    InBoundOrderInsert entity = new InBoundOrderInsert();
                    entity.WhCode = first.WhCode;
                    entity.ClientCode = first.ClientCode;
                    entity.SoNumber = item.ShipeeziSO;
                    entity.OrderType = "CFS";
                    entity.ProcessId = 132;
                    entity.ProcessName = "收货采集-Cotton";

                    List<InBoundOrderDetailInsert> orderDetailList = new List<InBoundOrderDetailInsert>();

                    InBoundOrderDetailInsert orderDetail = new InBoundOrderDetailInsert();
                    orderDetail.CustomerPoNumber = item.PoNumber;
                    orderDetail.AltItemNumber = item.PoNumber;
                    orderDetail.Style1 = "";
                    orderDetail.Style2 = "";
                    orderDetail.Style3 = "";
                    orderDetail.CustomNumber1 = item.SoNumber;
                    orderDetail.Weight = 0;
                    orderDetail.CBM = item.CBM;
                    orderDetail.Qty = Convert.ToInt32(item.Qty);
                    orderDetail.CreateUser = item.CreateUser;

                    orderDetailList.Add(orderDetail);

                    entity.InBoundOrderDetailInsert = orderDetailList;
                    entityInBoundList.Add(entity);
                }
                else
                {
                    InBoundOrderInsert oldentity = entityInBoundList.Where(u => u.SoNumber == item.ShipeeziSO).First();
                    entityInBoundList.Remove(oldentity);

                    InBoundOrderInsert newentity = oldentity;

                    List<InBoundOrderDetailInsert> orderDetailList = oldentity.InBoundOrderDetailInsert.ToList();

                    if (orderDetailList.Where(u => u.CustomerPoNumber == item.PoNumber).Count() == 0)
                    {
                        InBoundOrderDetailInsert orderDetail = new InBoundOrderDetailInsert();
                        orderDetail.CustomerPoNumber = item.PoNumber;
                        orderDetail.AltItemNumber = item.PoNumber;
                        orderDetail.Style1 = "";
                        orderDetail.Style2 = "";
                        orderDetail.Style3 = "";
                        orderDetail.CustomNumber1 = item.SoNumber;
                        orderDetail.Weight = 0;
                        orderDetail.CBM = item.CBM;
                        orderDetail.Qty = Convert.ToInt32(item.Qty);
                        orderDetail.CreateUser = item.CreateUser;
                        orderDetailList.Add(orderDetail);
                    }
                    else
                    {
                        InBoundOrderDetailInsert oldorderDetail = orderDetailList.Where(u => u.CustomerPoNumber == item.PoNumber).First();
                        orderDetailList.Remove(oldorderDetail);

                        InBoundOrderDetailInsert neworderDetail = oldorderDetail;
                        neworderDetail.CBM = oldorderDetail.CBM + item.CBM;
                        neworderDetail.Qty = oldorderDetail.Qty + Convert.ToInt32(item.Qty);
                        orderDetailList.Add(neworderDetail);
                    }

                    newentity.InBoundOrderDetailInsert = orderDetailList;
                    entityInBoundList.Add(newentity);
                }
            }

            idal.IExcelImportInBoundCottonDAL.Add(entityList);

            if (entityInBoundList.Count > 0)
            {
                ImportsInBoundOrder(entityInBoundList);
            }


            idal.SaveChanges();
            return "Y$" + systemNumber + "$" + sumQty;
        }


        //编辑
        public string EditForecastCotton(ExcelImportInBoundCotton entityCotton)
        {

            return "Y";

        }

        #endregion


        #region 14.Mosaic导入Forecast

        //forecast 列表查询
        public List<ExcelImportInBoundCom> MosaicExcelImportInBoundList(ExcelImportInBoundSearch searchEntity, out int total, out string str)
        {
            var sql = from a in idal.IExcelImportInBoundComDAL.SelectAll()
                      where a.WhCode == searchEntity.WhCode && a.ClientCode == "Mosaic"
                      select a;

            if (!string.IsNullOrEmpty(searchEntity.SystemNumber))
                sql = sql.Where(u => u.SystemNumber == searchEntity.SystemNumber);
            if (!string.IsNullOrEmpty(searchEntity.PoNumber))
                sql = sql.Where(u => u.PoNumber == searchEntity.PoNumber);
            if (!string.IsNullOrEmpty(searchEntity.SoNumber))
                sql = sql.Where(u => u.SoNumber == searchEntity.SoNumber);
            if (!string.IsNullOrEmpty(searchEntity.ItemNumber))
                sql = sql.Where(u => u.ItemNumber == searchEntity.ItemNumber);

            if (!string.IsNullOrEmpty(searchEntity.RatioorBulk))
                sql = sql.Where(u => u.Remark2.Contains(searchEntity.RatioorBulk));

            if (searchEntity.BeginDate != null)
            {
                sql = sql.Where(u => u.CreateDate >= searchEntity.BeginDate);
            }
            if (searchEntity.EndDate != null)
            {
                sql = sql.Where(u => u.CreateDate <= searchEntity.EndDate);
            }

            if (searchEntity.BeginConsDate != null)
            {
                sql = sql.Where(u => u.Remark1 >= searchEntity.BeginConsDate);
            }
            if (searchEntity.EndConsDate != null)
            {
                sql = sql.Where(u => u.Remark1 <= searchEntity.EndConsDate);
            }

            total = sql.Count();
            str = "";
            if (total > 0)
            {
                str = "{\"数量\":\"" + sql.Sum(u => u.Qty).ToString() + "\"}";
            }
            sql = sql.OrderByDescending(u => u.Id);
            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);
            return sql.ToList();
        }

        //Excel导入Forecast通用方法
        public string ExcelImportForecastCommon(List<ExcelImportInBoundCom> entityList)
        {
            string result = "";
            Hashtable data = new Hashtable();   //去excel重复的SO、PO、款号
            if (entityList.Count <= 0)
            {
                result += "数据异常！";
            }

            if (result != "")
            {
                return result;
            }

            ExcelImportInBoundCom first = entityList.First();

            string[] soArr = (from a in entityList
                              select a.SoNumber).Distinct().ToArray();

            var sql1 = (from a in idal.IInBoundSODAL.SelectAll()
                        join b in idal.IInBoundOrderDAL.SelectAll()
                        on a.Id equals b.SoId
                        join c in idal.IInBoundOrderDetailDAL.SelectAll()
                        on b.Id equals c.PoId
                        where soArr.Contains(a.SoNumber)
                        && a.WhCode == first.WhCode
                        && a.ClientCode == first.ClientCode
                        && c.RegQty > 0
                        select new InBoundOrderResult
                        {
                            SoNumber = a.SoNumber
                        }).Distinct();

            List<InBoundOrderResult> getList1 = sql1.ToList();

            string[] getRegSoArr = (from a in getList1
                                    select a.SoNumber).Distinct().ToArray();

            var sql2 = (from a in idal.IInBoundSODAL.SelectAll()
                        join b in idal.IInBoundOrderDAL.SelectAll()
                        on a.Id equals b.SoId
                        join c in idal.IInBoundOrderDetailDAL.SelectAll()
                        on b.Id equals c.PoId
                        where !getRegSoArr.Contains(a.SoNumber)
                        && a.WhCode == first.WhCode
                        && a.ClientCode == first.ClientCode
                        select new InBoundOrderResult
                        {
                            Id = b.Id,
                            SoNumber = a.SoNumber
                        }).Distinct();

            List<InBoundOrderResult> getList2 = sql2.ToList();

            foreach (var item in soArr)
            {
                if (getList1.Where(u => u.SoNumber == item).Count() == 0)
                {
                    List<InBoundOrderResult> getPoList = getList2.Where(u => u.SoNumber == item).ToList();
                    foreach (var itemPo in getPoList)
                    {
                        idal.IInBoundOrderDetailDAL.DeleteByExtended(u => u.PoId == itemPo.Id && u.WhCode == first.WhCode);
                        idal.IInBoundOrderDAL.DeleteByExtended(u => u.Id == itemPo.Id);
                    }
                }
            }

            idal.IExcelImportInBoundComDAL.DeleteByExtended(u => soArr.Contains(u.SoNumber) && u.WhCode == first.WhCode);

            string systemNumber = "SYS" + DI.IDGenerator.NewId;
            int? sumQty = entityList.Sum(u => u.Qty);

            List<InBoundOrderInsert> entityInBoundList = new List<InBoundOrderInsert>();

            foreach (var item in entityList)
            {
                item.SystemNumber = systemNumber;

                if (entityInBoundList.Where(u => u.SoNumber == item.SoNumber).Count() == 0)
                {
                    InBoundOrderInsert entity = new InBoundOrderInsert();
                    entity.WhCode = first.WhCode;
                    entity.ClientCode = first.ClientCode;
                    entity.SoNumber = item.SoNumber;
                    entity.OrderType = "CFS";
                    entity.ProcessId = 25;
                    entity.ProcessName = "收货默认流程";

                    List<InBoundOrderDetailInsert> orderDetailList = new List<InBoundOrderDetailInsert>();

                    InBoundOrderDetailInsert orderDetail = new InBoundOrderDetailInsert();
                    orderDetail.CustomerPoNumber = item.PoNumber;
                    orderDetail.AltItemNumber = item.ItemNumber;
                    orderDetail.Style1 = item.Style1 ?? "";
                    orderDetail.Style2 = item.Style2 ?? "";
                    orderDetail.Style3 = item.Style3 ?? "";
                    orderDetail.Weight = item.Weight;
                    orderDetail.CBM = item.CBM;
                    orderDetail.Qty = Convert.ToInt32(item.Qty);
                    orderDetail.CreateUser = item.CreateUser;

                    orderDetailList.Add(orderDetail);

                    entity.InBoundOrderDetailInsert = orderDetailList;
                    entityInBoundList.Add(entity);
                }
                else
                {
                    InBoundOrderInsert oldentity = entityInBoundList.Where(u => u.SoNumber == item.SoNumber).First();
                    entityInBoundList.Remove(oldentity);

                    InBoundOrderInsert newentity = oldentity;

                    List<InBoundOrderDetailInsert> orderDetailList = oldentity.InBoundOrderDetailInsert.ToList();

                    if (orderDetailList.Where(u => u.CustomerPoNumber == item.PoNumber && u.AltItemNumber == item.ItemNumber && u.Style1 == (item.Style1 ?? "") && u.Style2 == (item.Style2 ?? "") && u.Style3 == (item.Style3 ?? "")).Count() == 0)
                    {
                        InBoundOrderDetailInsert orderDetail = new InBoundOrderDetailInsert();
                        orderDetail.CustomerPoNumber = item.PoNumber;
                        orderDetail.AltItemNumber = item.ItemNumber;
                        orderDetail.Style1 = item.Style1 ?? "";
                        orderDetail.Style2 = item.Style2 ?? "";
                        orderDetail.Style3 = item.Style3 ?? "";
                        orderDetail.Weight = item.Weight;
                        orderDetail.CBM = item.CBM;
                        orderDetail.Qty = Convert.ToInt32(item.Qty);
                        orderDetail.CreateUser = item.CreateUser;
                        orderDetailList.Add(orderDetail);
                    }
                    else
                    {
                        InBoundOrderDetailInsert oldorderDetail = orderDetailList.Where(u => u.CustomerPoNumber == item.PoNumber && u.AltItemNumber == item.ItemNumber && u.Style1 == (item.Style1 ?? "") && u.Style2 == (item.Style2 ?? "") && u.Style3 == (item.Style3 ?? "")).First();
                        orderDetailList.Remove(oldorderDetail);

                        InBoundOrderDetailInsert neworderDetail = oldorderDetail;
                        neworderDetail.CBM = oldorderDetail.CBM + item.CBM;
                        neworderDetail.Qty = oldorderDetail.Qty + Convert.ToInt32(item.Qty);
                        orderDetailList.Add(neworderDetail);
                    }

                    newentity.InBoundOrderDetailInsert = orderDetailList;
                    entityInBoundList.Add(newentity);
                }
            }

            idal.IExcelImportInBoundComDAL.Add(entityList);

            if (entityInBoundList.Count > 0)
            {
                ImportsInBoundOrder(entityInBoundList);
            }

            idal.SaveChanges();
            return "Y$" + systemNumber + "$" + sumQty;
        }

        #endregion

    }
}

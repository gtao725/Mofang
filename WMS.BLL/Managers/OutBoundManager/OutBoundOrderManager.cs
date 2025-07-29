using MODEL_MSSQL;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMS.BLLClass;
using WMS.IBLL;

namespace WMS.BLL
{

    public class OutBoundOrderManager : IOutBoundOrderManager
    {
        IDAL.IDALSession idal = BLL.BLLHelper.GetDal();
        private static object o = new object();

        ShipHelper shipHelper = new ShipHelper();


        //出库订单查询
        //对应 OutBoundOrderController 中的 List 方法
        public List<OutBoundOrderResult> OutBoundOrderList(OutBoundOrderSearch searchEntity, out int total)
        {
            var sql = from a in idal.IOutBoundOrderDAL.SelectAll()
                      where a.WhCode == searchEntity.WhCode
                      join b in idal.IFlowHeadDAL.SelectAll()
                      on a.ProcessId equals b.Id
                      join c in idal.IFlowDetailDAL.SelectAll()
                      on new { A = a.ProcessId, B = a.NowProcessId }
                      equals new { A = c.FlowHeadId, B = c.FlowRuleId } into te1
                      from ab in te1.DefaultIfEmpty()
                      join d in idal.ILoadDetailDAL.SelectAll()
                      on a.Id equals d.OutBoundOrderId into temp1
                      from ad in temp1.DefaultIfEmpty()
                      join e in idal.ILoadMasterDAL.SelectAll()
                      on ad.LoadMasterId equals e.Id into temp2
                      from ade in temp2.DefaultIfEmpty()
                      select new OutBoundOrderResult
                      {
                          Id = a.Id,
                          ClientId = a.ClientId,
                          ClientCode = a.ClientCode,
                          OutPoNumber = a.OutPoNumber,
                          CustomerOutPoNumber = a.CustomerOutPoNumber,
                          AltCustomerOutPoNumber = a.AltCustomerOutPoNumber,
                          ProcessId = a.ProcessId,
                          NowProcessId = a.NowProcessId,
                          NowProcessName = ab.FunctionName,
                          StatusId = a.StatusId,
                          StatusName = a.StatusName,
                          OrderSource = a.OrderSource ?? "",
                          PlanOutTime = a.PlanOutTime,
                          CreateUser = a.CreateUser,
                          CreateDate = a.CreateDate,
                          FlowName = b.FlowName,
                          RollbackFlag = ab.RollbackFlag,
                          LoadId = ade.LoadId,
                          DSShow = a.DSFLag == null ? "普通" :
                            a.DSFLag == 0 ? "普通" :
                            a.DSFLag == 1 ? "直装" : null,
                      };

            if (searchEntity.ClientId != 0)
                sql = sql.Where(u => u.ClientId == searchEntity.ClientId);
            if (!string.IsNullOrEmpty(searchEntity.CustomerOutPoNumber))
                sql = sql.Where(u => u.CustomerOutPoNumber == searchEntity.CustomerOutPoNumber);
            if (!string.IsNullOrEmpty(searchEntity.AltCustomerOutPoNumber))
                sql = sql.Where(u => u.AltCustomerOutPoNumber == searchEntity.AltCustomerOutPoNumber);

            if (!string.IsNullOrEmpty(searchEntity.OutPoNumber))
                sql = sql.Where(u => u.OutPoNumber == searchEntity.OutPoNumber);
            if (!string.IsNullOrEmpty(searchEntity.OrderSource))
                sql = sql.Where(u => u.OrderSource == searchEntity.OrderSource);

            if (searchEntity.StatusId != 0)
                sql = sql.Where(u => u.StatusId == searchEntity.StatusId);
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

        //根据选择的客户获得出货流程
        public IEnumerable<FlowHead> OutFlowHeadListByClientId(string whCode, int clientId)
        {
            var sql = from a in idal.IFlowHeadDAL.SelectAll()
                      join b in idal.IR_Client_FlowRuleDAL.SelectAll()
                      on a.Id equals b.BusinessFlowGroupId
                      join c in idal.IWhClientDAL.SelectAll()
                      on b.ClientId equals c.Id
                      where c.Id == clientId && a.WhCode == whCode && a.Type == "OutBound"
                      select a;
            return sql.AsEnumerable();
        }


        //出库流程下拉列表
        public IEnumerable<FlowHead> OutFlowHeadListSelect(string whCode)
        {
            var sql = from a in idal.IFlowHeadDAL.SelectAll()
                      where a.WhCode == whCode && a.Type == "OutBound"
                      select a;
            return sql.AsEnumerable();
        }

        //状态查询下拉列表
        public IEnumerable<LookUp> FlowRuleStatusListSelect()
        {
            var sql = from a in idal.ILookUpDAL.SelectAll()
                      where a.TableName == "FlowRule" && a.ColumnName == "StatusId"
                      select a;
            return sql.AsEnumerable();
        }

        //出库订单添加
        //对应 OutBoundOrderController 中的 OutBoundOrderAdd 方法
        public OutBoundOrder OutBoundOrderAdd(OutBoundOrder entity)
        {
            if (!shipHelper.CheckClientCode(entity.WhCode, entity.ClientCode))
            {
                return null;
            }
            if (entity.CustomerOutPoNumber == "" || entity.CustomerOutPoNumber == null || entity.ProcessId == 0)
            {
                return null;
            }

            var sqlCheck = from a in idal.IOutBoundOrderDAL.SelectAll()
                           where a.ClientId == entity.ClientId && a.WhCode == entity.WhCode && a.CustomerOutPoNumber == entity.CustomerOutPoNumber
                           select a;
            if (sqlCheck.Count() > 0)
            {
                return null;
            }
            entity.OutPoNumber = "SA" + DI.IDGenerator.NewId;
            entity.ReceiptId = "";
            entity.DSFLag = 0;
            var sql = from a in idal.IFlowDetailDAL.SelectAll()
                      where a.FlowHeadId == entity.ProcessId
                      select a;
            FlowDetail flowDetail = sql.OrderBy(u => u.OrderId).First();
            entity.NowProcessId = flowDetail.FlowRuleId;
            entity.StatusId = flowDetail.StatusId;
            entity.StatusName = flowDetail.StatusName;
            idal.IOutBoundOrderDAL.Add(entity);
            idal.IOutBoundOrderDAL.SaveChanges();
            return entity;
        }

        //确认出库订单
        public string ConfirmOutBoundOrder(OutBoundOrder eneity)
        {
            if (eneity.Id == 0)
            {
                return "订单有误，请重新查询！";
            }
            OutBoundOrder outBoundOrder = idal.IOutBoundOrderDAL.SelectBy(u => u.Id == eneity.Id).First();

            List<OutBoundOrderDetail> orderDetailList = idal.IOutBoundOrderDetailDAL.SelectBy(u => u.OutBoundOrderId == outBoundOrder.Id);
            if (orderDetailList.Count == 0)
            {
                return "订单未添加明细！";
            }

            if (outBoundOrder.StatusId != 5)
            {
                return "订单状态有误，请重新查询！";
            }

            string remark1 = "原状态：" + outBoundOrder.StatusId + outBoundOrder.StatusName;

            FlowHelper flowHelper = new FlowHelper(eneity, "OutBound");
            FlowDetail flowDetail = flowHelper.GetNextFlowDetail();
            if (flowDetail != null && flowDetail.StatusId != 0)
            {
                eneity.NowProcessId = flowDetail.FlowRuleId;
                eneity.StatusId = flowDetail.StatusId;
                eneity.StatusName = flowDetail.StatusName;
                eneity.UpdateUser = eneity.CreateUser;
                eneity.UpdateDate = DateTime.Now;
                idal.IOutBoundOrderDAL.UpdateBy(eneity, u => u.Id == eneity.Id, new string[] { "NowProcessId", "StatusId", "StatusName", "UpdateUser", "UpdateDate" });

                //更新订单状态，插入日志
                TranLog tlorder = new TranLog();
                tlorder.TranType = "32";
                tlorder.Description = "更新订单状态";
                tlorder.TranDate = DateTime.Now;
                tlorder.TranUser = eneity.CreateUser;
                tlorder.WhCode = outBoundOrder.WhCode;
                tlorder.CustomerOutPoNumber = outBoundOrder.CustomerOutPoNumber;
                tlorder.OutPoNumber = outBoundOrder.OutPoNumber;
                tlorder.Remark = remark1 + "变更为：" + eneity.StatusId + eneity.StatusName;
                idal.ITranLogDAL.Add(tlorder);

                idal.IOutBoundOrderDAL.SaveChanges();
                return "Y";
            }
            else
            {
                return "错误！获取订单状态有误！";
            }
        }

        //回滚出库订单
        public string RollbackOutBoundOrder(OutBoundOrder eneity)
        {
            if (eneity.Id == 0)
            {
                return "订单有误，请重新查询！";
            }
            OutBoundOrder outBoundOrder = idal.IOutBoundOrderDAL.SelectBy(u => u.Id == eneity.Id).First();
            if (outBoundOrder.StatusId != 10)
            {
                return "订单状态有误无法回滚，请重新查询！";
            }

            string remark1 = "原状态：" + outBoundOrder.StatusId + outBoundOrder.StatusName;

            FlowHelper flowHelper = new FlowHelper(eneity, "OutBound");
            FlowDetail flowDetail = flowHelper.GetPreviousFlowDetail();
            if (flowDetail != null && flowDetail.StatusId != 0)
            {
                eneity.NowProcessId = flowDetail.FlowRuleId;
                eneity.StatusId = flowDetail.StatusId;
                eneity.StatusName = flowDetail.StatusName;
                eneity.UpdateUser = eneity.CreateUser;
                eneity.UpdateDate = DateTime.Now;
                idal.IOutBoundOrderDAL.UpdateBy(eneity, u => u.Id == eneity.Id, new string[] { "NowProcessId", "StatusId", "StatusName", "UpdateUser", "UpdateDate" });

                //更新订单状态，插入日志
                TranLog tlorder = new TranLog();
                tlorder.TranType = "32";
                tlorder.Description = "更新订单状态";
                tlorder.TranDate = DateTime.Now;
                tlorder.TranUser = eneity.CreateUser;
                tlorder.WhCode = outBoundOrder.WhCode;
                tlorder.CustomerOutPoNumber = outBoundOrder.CustomerOutPoNumber;
                tlorder.OutPoNumber = outBoundOrder.OutPoNumber;
                tlorder.Remark = remark1 + "变更为：" + eneity.StatusId + eneity.StatusName;
                idal.ITranLogDAL.Add(tlorder);

                idal.IOutBoundOrderDAL.SaveChanges();
                return "Y";
            }
            else
            {
                return "错误！获取订单状态有误！";
            }
        }

        //验证款号、款号对应的单位 是否有误
        public string OutBoundCheckUnitName(OutBoundOrderDetailInsert entity)
        {
            string result = "";     //执行总结果
            int recount = 0;

            if (entity.OutBoundOrderDetailModel == null)
            {
                return "数据有误，请重新操作！";
            }

            foreach (var item in entity.OutBoundOrderDetailModel)
            {
                //判断款号是否存在
                List<ItemMaster> listItemMaster = idal.IItemMasterDAL.SelectBy(u => u.WhCode == entity.WhCode && u.AltItemNumber == item.AltItemNumber && u.ClientId == entity.ClientId).OrderBy(u => u.Id).ToList();

                //属性1
                if (item.Style1 == "" || item.Style1 == null)
                {
                    listItemMaster = listItemMaster.Where(u => u.Style1 == "" || u.Style1 == null).ToList();
                }
                else
                {
                    listItemMaster = listItemMaster.Where(u => u.Style1 == item.Style1).ToList();
                }
                //属性2
                if (item.Style2 == "" || item.Style2 == null)
                {
                    listItemMaster = listItemMaster.Where(u => u.Style2 == "" || u.Style2 == null).ToList();
                }
                else
                {
                    listItemMaster = listItemMaster.Where(u => u.Style2 == item.Style2).ToList();
                }
                //属性3
                if (item.Style3 == "" || item.Style3 == null)
                {
                    listItemMaster = listItemMaster.Where(u => u.Style3 == "" || u.Style3 == null).ToList();
                }
                else
                {
                    listItemMaster = listItemMaster.Where(u => u.Style3 == item.Style3).ToList();
                }

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
                                result = "N款号" + itemMaster.AltItemNumber + "单位录入有误!$" + item.JsonId;
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
                    result = "N款号" + item.AltItemNumber + "不存在,请重新录入!$" + item.JsonId;
                }
            }

            if (recount != entity.OutBoundOrderDetailModel.Count)
            {
                return result;
            }
            else
            {
                return "Y";
            }
        }

        //出库订单明细添加
        //对应 OutBoundOrderController 中的 OutBoundOrderAdd 方法
        public string OutBoundOrderDetailAdd(OutBoundOrderDetailInsert entity)
        {
            string result = "";     //执行总结果

            if (entity.OutBoundOrderDetailModel == null)
            {
                return "数据有误，请重新操作！";
            }

            OutBoundOrder outBoundOrder = idal.IOutBoundOrderDAL.SelectBy(u => u.Id == entity.OutBoundOrderId).First();
            if (outBoundOrder.StatusId != 5)
            {
                return "<font color=red>订单状态有误，请重新返回查询！</font>";
            }

            foreach (var item in entity.OutBoundOrderDetailModel)
            {
                ItemMaster itemMaster = idal.IItemMasterDAL.SelectBy(u => u.WhCode == entity.WhCode && u.AltItemNumber == item.AltItemNumber && (u.Style1 == null ? "" : u.Style1) == (item.Style1 == null ? "" : item.Style1) && (u.Style2 == null ? "" : u.Style2) == (item.Style2 == null ? "" : item.Style2) && (u.Style3 == null ? "" : u.Style3) == (item.Style3 == null ? "" : item.Style3) && u.ClientId == entity.ClientId).OrderBy(u => u.Id).ToList().First();

                List<OutBoundOrderDetail> listOutBoundOrderDetail = idal.IOutBoundOrderDetailDAL.SelectBy(u => u.WhCode == entity.WhCode && u.ItemId == itemMaster.Id && u.UnitId == item.UnitId && u.AltItemNumber == item.AltItemNumber && (u.SoNumber == null ? "" : u.SoNumber) == item.SoNumber && (u.CustomerPoNumber == null ? "" : u.CustomerPoNumber) == item.CustomerPoNumber && u.OutBoundOrderId == entity.OutBoundOrderId).ToList();

                if (listOutBoundOrderDetail.Count == 0)
                {
                    OutBoundOrderDetail orderDetail = new OutBoundOrderDetail();
                    orderDetail.OutBoundOrderId = entity.OutBoundOrderId;
                    orderDetail.WhCode = entity.WhCode;
                    orderDetail.SoNumber = item.SoNumber;
                    orderDetail.CustomerPoNumber = item.CustomerPoNumber;
                    orderDetail.AltItemNumber = item.AltItemNumber;
                    orderDetail.ItemId = itemMaster.Id;
                    orderDetail.UnitId = item.UnitId;
                    if (itemMaster.UnitFlag == 0)
                    {
                        orderDetail.UnitName = itemMaster.UnitName;
                    }
                    else
                    {
                        orderDetail.UnitName = item.UnitName;
                    }
                    orderDetail.Qty = item.Qty;
                    orderDetail.DSFLag = entity.DSFlag;
                    orderDetail.LotNumber1 = item.LotNumber1;
                    orderDetail.LotNumber2 = item.LotNumber2;
                    orderDetail.LotDate = item.LotDate;
                    orderDetail.CreateUser = item.CreateUser;
                    orderDetail.CreateDate = DateTime.Now;
                    idal.IOutBoundOrderDetailDAL.Add(orderDetail);
                    result += item.SoNumber + " " + item.CustomerPoNumber + " " + item.AltItemNumber + " " + item.Qty + "添加成功!<br>";
                }
                else
                {
                    OutBoundOrderDetail orderDetail = listOutBoundOrderDetail.First();
                    orderDetail.Qty += item.Qty;
                    orderDetail.UpdateUser = item.CreateUser;
                    orderDetail.UpdateDate = DateTime.Now;
                    idal.IOutBoundOrderDetailDAL.UpdateBy(orderDetail, u => u.Id == orderDetail.Id, new string[] { "Qty", "UpdateUser", "UpdateDate" });

                    result += "<font color=red>" + item.SoNumber + " " + item.CustomerPoNumber + " " + item.AltItemNumber + "更新成功,数量累加" + item.Qty + "!</font><br>";
                }

            }
            idal.IOutBoundOrderDAL.SaveChanges();

            return result;
        }

        //出库订单明细查询
        //对应 OutBoundOrderController 中的 OutBoundOrderDetailList 方法
        public List<OutBoundOrderDetailResult> OutBoundOrderDetailList(OutBoundOrderDetailSearch searchEntity, string[] itemNumberList, out int total)
        {
            var sql = from a in idal.IOutBoundOrderDetailDAL.SelectAll()
                      where a.WhCode == searchEntity.WhCode && a.OutBoundOrderId == searchEntity.OutBoundOrderId
                      join b in idal.IItemMasterDAL.SelectAll()
                      on a.ItemId equals b.Id
                      select new OutBoundOrderDetailResult
                      {
                          Id = a.Id,
                          SoNumber = a.SoNumber,
                          CustomerPoNumber = a.CustomerPoNumber,
                          AltItemNumber = b.AltItemNumber,
                          ItemId = a.ItemId,
                          Style1 = b.Style1,
                          Style2 = b.Style2,
                          Style3 = b.Style3,
                          Qty = a.Qty,
                          Sequence = a.Sequence ?? 0,
                          UnitName = a.UnitName,
                          LotNumber1 = a.LotNumber1,
                          LotNumber2 = a.LotNumber2,
                          StowPosition = a.StowPosition,
                          LotDate = a.LotDate
                      };

            if (!string.IsNullOrEmpty(searchEntity.CustomerPoNumber))
                sql = sql.Where(u => u.CustomerPoNumber.Contains(searchEntity.CustomerPoNumber));
            if (!string.IsNullOrEmpty(searchEntity.AltItemNumber))
                sql = sql.Where(u => u.AltItemNumber.Contains(searchEntity.AltItemNumber));

            if (itemNumberList != null)
                sql = sql.Where(u => itemNumberList.Contains(u.AltItemNumber));

            total = sql.Count();
            sql = sql.OrderByDescending(u => u.Id);
            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);
            return sql.ToList();
        }

        //出库订单录入信息修改
        public int OutBoundOrderDetailEdit(OutBoundOrderDetail entity, params string[] modifiedProNames)
        {
            if (entity.Id == 0)
            {
                return 0;
            }
            entity.UpdateDate = DateTime.Now;
            idal.IOutBoundOrderDetailDAL.UpdateBy(entity, u => u.Id == entity.Id, modifiedProNames);
            idal.IOutBoundOrderDetailDAL.SaveChanges();
            return 1;
        }

        //出库订单删除 同时删除明细
        public string OutBoundOrderDel(int id)
        {
            if (id == 0)
            {
                return "数据有误，请重新操作！";
            }

            OutBoundOrder outBoundOrder = idal.IOutBoundOrderDAL.SelectBy(u => u.Id == id).First();
            if (outBoundOrder.StatusId != 5 && outBoundOrder.StatusId != 10)
            {
                return "订单状态有误，请重新查询！";
            }

            idal.IOutBoundOrderDetailDAL.DeleteBy(u => u.OutBoundOrderId == id);
            idal.IOutBoundOrderDAL.DeleteBy(u => u.Id == id);
            idal.IOutBoundOrderDAL.SaveChanges();
            return "Y";
        }


        //出库订单导入
        public string ImportsOutBoundOrder(List<ImportOutBoundOrder> entityList)
        {
            string result = "";     //执行总结果

            if (entityList == null)
            {
                return "数据有误，请重新操作！";
            }

            string whCode = entityList.First().WhCode;
            string userName = entityList.First().OutBoundOrderDetailModel.First().CreateUser;

            foreach (var entity in entityList)
            {
                if (result != "")
                {
                    break;
                }

                //首先验证 出库订单号是否已存在 和状态
                List<OutBoundOrder> sqlCheck = (from a in idal.IOutBoundOrderDAL.SelectAll()
                                                where a.ClientCode == entity.ClientCode && a.WhCode == whCode && a.CustomerOutPoNumber == entity.CustomerOutPoNumber
                                                select a).ToList();
                if (sqlCheck.Count > 0)
                {
                    OutBoundOrder outCheck = sqlCheck.First();
                    if (outCheck.ProcessId != entity.ProcessId)
                    {
                        FlowHead flowHead = idal.IFlowHeadDAL.SelectBy(u => u.Id == outCheck.ProcessId).First();
                        result = "订单号已存在:" + entity.CustomerOutPoNumber + "<br/>且流程为：" + flowHead.FlowName + "，无法添加！";
                        break;
                    }
                    if ((outCheck.DSFLag ?? 0) != 0)
                    {
                        result = "直装订单已存在：" + entity.ClientCode + "-" + entity.CustomerOutPoNumber;
                        break;
                    }
                    if (outCheck.StatusId != 5)
                    {
                        result = "出库订单状态有误：" + entity.ClientCode + "-" + entity.CustomerOutPoNumber;
                        break;
                    }
                }

                //验证款号是否存在 和 是否是多种单位
                List<string> getItemNumber = (from a in entity.OutBoundOrderDetailModel
                                              select a.AltItemNumber).ToList();
                foreach (var item in getItemNumber)
                {
                    if (idal.IItemMasterDAL.SelectBy(u => u.AltItemNumber == item && u.WhCode == whCode && u.ClientCode == entity.ClientCode).Count() == 0)
                    {
                        result = "款号不存在：" + entity.ClientCode + "-" + item;
                        break;
                    }
                    if (idal.IItemMasterDAL.SelectBy(u => u.UnitFlag == 1).Where(u => u.AltItemNumber == item && u.WhCode == whCode && u.ClientCode == entity.ClientCode).Count() > 0)
                    {
                        result = "款号为多种单位，请手动录入明细：" + entity.ClientCode + "-" + item;
                        break;
                    }
                }
            }
            if (result != "")
            {
                return result;
            }

            List<WhClient> clientList = idal.IWhClientDAL.SelectBy(u => u.WhCode == whCode);

            //批量导入
            foreach (var entity in entityList)
            {
                List<OutBoundOrder> sqlCheck = (from a in idal.IOutBoundOrderDAL.SelectAll()
                                                where a.ClientCode == entity.ClientCode && a.WhCode == whCode && a.CustomerOutPoNumber == entity.CustomerOutPoNumber
                                                select a).ToList();

                OutBoundOrder outBoundOrder = new OutBoundOrder();
                WhClient client = clientList.Where(u => u.ClientCode == entity.ClientCode && u.WhCode == entity.WhCode).First();
                if (sqlCheck.Count == 0)
                {
                    outBoundOrder.WhCode = whCode;
                    outBoundOrder.ClientId = client.Id;
                    outBoundOrder.ClientCode = entity.ClientCode;
                    outBoundOrder.OutPoNumber = "SA" + DI.IDGenerator.NewId;
                    outBoundOrder.CustomerOutPoNumber = entity.CustomerOutPoNumber;
                    outBoundOrder.ProcessId = entity.ProcessId;

                    var sql = from a in idal.IFlowDetailDAL.SelectAll()
                              where a.FlowHeadId == outBoundOrder.ProcessId
                              select a;
                    FlowDetail flowDetail = sql.OrderBy(u => u.OrderId).First();
                    outBoundOrder.NowProcessId = flowDetail.FlowRuleId;
                    outBoundOrder.StatusId = flowDetail.StatusId;
                    outBoundOrder.StatusName = flowDetail.StatusName;
                    outBoundOrder.ReceiptId = "";
                    outBoundOrder.OrderSource = "WMS录入";
                    outBoundOrder.PlanOutTime = DateTime.Now;
                    outBoundOrder.LoadFlag = 0;
                    outBoundOrder.DSFLag = 0;
                    outBoundOrder.CreateUser = userName;
                    outBoundOrder.CreateDate = DateTime.Now;

                    idal.IOutBoundOrderDAL.Add(outBoundOrder);
                    idal.SaveChanges();
                }
                else
                {
                    outBoundOrder = sqlCheck.First();
                }

                foreach (var item in entity.OutBoundOrderDetailModel)
                {
                    ItemMaster itemMaster = idal.IItemMasterDAL.SelectBy(u => u.WhCode == entity.WhCode && u.AltItemNumber == item.AltItemNumber && u.ClientId == outBoundOrder.ClientId).OrderBy(u => u.Id).ToList().First();

                    List<OutBoundOrderDetail> listOutBoundOrderDetail = idal.IOutBoundOrderDetailDAL.SelectBy(u => u.WhCode == entity.WhCode && u.ItemId == itemMaster.Id && u.AltItemNumber == item.AltItemNumber && u.SoNumber == item.SoNumber && u.CustomerPoNumber == item.CustomerPoNumber && u.OutBoundOrderId == outBoundOrder.Id).ToList();

                    if (listOutBoundOrderDetail.Count == 0)
                    {
                        OutBoundOrderDetail orderDetail = new OutBoundOrderDetail();
                        orderDetail.OutBoundOrderId = outBoundOrder.Id;
                        orderDetail.WhCode = entity.WhCode;
                        orderDetail.SoNumber = item.SoNumber;
                        orderDetail.CustomerPoNumber = item.CustomerPoNumber;
                        orderDetail.AltItemNumber = item.AltItemNumber;
                        orderDetail.ItemId = itemMaster.Id;
                        orderDetail.UnitId = 0;
                        orderDetail.UnitName = itemMaster.UnitName;

                        orderDetail.Qty = item.Qty;
                        orderDetail.DSFLag = 0;
                        orderDetail.LotNumber1 = item.LotNumber1;
                        orderDetail.LotNumber2 = item.LotNumber2;
                        orderDetail.CreateUser = item.CreateUser;
                        orderDetail.CreateDate = DateTime.Now;
                        idal.IOutBoundOrderDetailDAL.Add(orderDetail);
                    }
                    else
                    {
                        OutBoundOrderDetail orderDetail = listOutBoundOrderDetail.First();
                        orderDetail.Qty += item.Qty;
                        orderDetail.UpdateUser = item.CreateUser;
                        orderDetail.UpdateDate = DateTime.Now;
                        idal.IOutBoundOrderDetailDAL.UpdateBy(orderDetail, u => u.Id == orderDetail.Id, new string[] { "Qty", "UpdateUser", "UpdateDate" });
                    }
                }
            }

            idal.IOutBoundOrderDAL.SaveChanges();
            return "Y";
        }


        //出库发货揽件异常登记
        public string DeliveryExceptionRegister(string WhCode, string ExpressNumber, string user)
        {
            string res = "N";
            int rowcount = idal.IDcShipingExceptionDAL.SelectBy(u => u.ExpressNumber == ExpressNumber).Count();
            DcShipingException AddDcShipingException = new DcShipingException();
            if (rowcount == 0)
            {
                AddDcShipingException.ExpressNumber = ExpressNumber;
                AddDcShipingException.CreateUser = user;
                AddDcShipingException.CreateDate = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"));
                idal.IDcShipingExceptionDAL.Add(AddDcShipingException);
                idal.IDcShipingExceptionDAL.SaveChanges();
                res = "Y";
            }
            else
            {
                res = "N";
            }
            return res;
        }




    }

}

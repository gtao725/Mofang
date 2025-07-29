using MODEL_MSSQL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMS.BLLClass;
using WMS.IBLL;
using System.Transactions;

namespace WMS.BLL
{
    public class RegInBoundOrderManager : IRegInBoundOrderManager
    {
        IDAL.IDALSession idal = BLL.BLLHelper.GetDal();
        private static object o = new object();

        #region 1.收货登记 

        //收货登记区域下拉列表
        public IEnumerable<WhZoneResult> RecZoneSelect(string whCode, int clientId)
        {
            if (clientId == 0)
            {
                var sql = (from a in idal.IZoneDAL.SelectAll()
                           where a.WhCode == whCode && a.RegFlag == 1
                           select new WhZoneResult
                           {
                               Id = a.Id,
                               ZoneName = a.ZoneName
                           }).OrderBy(u => u.Id);

                return sql.AsEnumerable();
            }
            else
            {
                var sql = (from a in idal.IWhClientDAL.SelectAll()
                           join b in idal.IZoneDAL.SelectAll() on new { ZoneId = (Int32)a.ZoneId } equals new { ZoneId = b.Id } into b_join
                           from b in b_join.DefaultIfEmpty()
                           where a.WhCode == whCode && b.RegFlag == 1 && a.Id == clientId
                           select new WhZoneResult
                           {
                               Id = b.Id,
                               ZoneName = b.ZoneName
                           }).OrderByDescending(u => u.Id);

                return sql.AsEnumerable();
            }
        }

        //客户流程下拉列表
        public IEnumerable<BusinessFlowGroupResult> RegClientFlowNameSelect(int clientId)
        {
            var sql = from a in idal.IWhClientDAL.SelectAll()
                      where a.Id == clientId
                      join b in idal.IR_Client_FlowRuleDAL.SelectAll()
                      on new { A = a.WhCode, B = a.Id } equals new { A = b.WhCode, B = b.ClientId } into b_join
                      from b in b_join.DefaultIfEmpty()
                      join c in idal.IFlowHeadDAL.SelectAll()
                      on new { A = (Int32)b.BusinessFlowGroupId } equals new { A = c.Id } into c_join
                      from c in c_join.DefaultIfEmpty()
                      where c.Type == "InBound"
                      select new BusinessFlowGroupResult
                      {
                          Id = c.Id,
                          FlowName = c.FlowName
                      };

            return sql.AsEnumerable();
        }

        //收货登记查询
        public List<ReceiptRegisterResult> ReceiptRegisterList(ReceiptRegisterSearch searchEntity, out int total)
        {
            var sql = from a in idal.IReceiptRegisterDAL.SelectAll()
                      where a.WhCode == searchEntity.WhCode
                      select new ReceiptRegisterResult
                      {
                          Id = a.Id,
                          ReceiptId = a.ReceiptId,
                          ReceiptType = a.ReceiptType,
                          WhCode = a.WhCode,
                          ClientId = a.ClientId,
                          ClientCode = a.ClientCode,
                          Status =
                           a.Status == "N" ? "未释放" :
                           a.Status == "U" ? "未收货" :
                           a.Status == "A" ? "正在收货" :
                           a.Status == "P" ? "暂停收货" :
                           a.Status == "C" ? "完成收货" : null,
                          ProcessName = a.ProcessName,
                          TruckNumber = a.TruckNumber,
                          PhoneNumber = a.PhoneNumber,
                          RegisterDate = a.RegisterDate,
                          LocationId = a.LocationId,
                          ArriveDate = a.PrintDate,
                          CreateUser = a.CreateUser,
                          CreateDate = a.CreateDate,
                          DSFlag = a.DSFLag == 0 ? "普通" :
                          a.DSFLag == null ? "普通" :
                           a.DSFLag == 1 ? "直装" : null,
                          SumQty = a.SumQty,
                          SumCBM = a.SumCBM ?? 0,
                          BkDate = a.BkDate,
                          GreenPassFlag = a.GreenPassFLag ?? 0,
                          GreenPassFlagShow = a.GreenPassFLag == 0 ? "" :
                          a.GreenPassFLag == null ? "" :
                          a.GreenPassFLag == 1 ? "是" : "",
                          TruckCountShow = a.TruckCount ?? 0,
                          DNCountShow = a.DNCount ?? 0,
                          FaxInCountShow = a.FaxInCount ?? 0,
                          FaxOutCountShow = a.FaxOutCount ?? 0,
                          BillCountShow = a.BillCount ?? 0,
                          ResetCountShow = a.ResetCount ?? 0,
                          GoodType = a.GoodType
                      };

            if (!string.IsNullOrEmpty(searchEntity.ReceiptId))
                sql = sql.Where(u => u.ReceiptId.Contains(searchEntity.ReceiptId));
            if (searchEntity.ClientId > 0)
                sql = sql.Where(u => u.ClientId == searchEntity.ClientId);
            if (!string.IsNullOrEmpty(searchEntity.TruckNumber))
                sql = sql.Where(u => u.TruckNumber.Contains(searchEntity.TruckNumber));
            if (!string.IsNullOrEmpty(searchEntity.PhoneNumber))
                sql = sql.Where(u => u.PhoneNumber.Contains(searchEntity.PhoneNumber));

            if (searchEntity.ReceiptType != "Com")
            {
                sql = sql.Where(u => u.ReceiptType == searchEntity.ReceiptType);
            }
            if (!string.IsNullOrEmpty(searchEntity.Status))
            {
                sql = sql.Where(u => u.Status == searchEntity.Status);
            }

            if (searchEntity.BeginCreateDate != null)
            {
                sql = sql.Where(u => u.CreateDate >= searchEntity.BeginCreateDate);
            }
            if (searchEntity.EndCreateDate != null)
            {
                sql = sql.Where(u => u.CreateDate <= searchEntity.EndCreateDate);
            }
            if (!string.IsNullOrEmpty(searchEntity.CreateUser))
            {
                sql = sql.Where(u => u.CreateUser == searchEntity.CreateUser);
            }

            total = sql.Count();
            sql = sql.OrderByDescending(u => u.ReceiptId).ThenBy(u => u.ClientCode);
            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);
            return sql.ToList();

        }

        //添加 收货登记头表 给第三方导入使用，状态为已释放未收货
        public ReceiptRegister AddReceiptRegister(ReceiptRegister entity)
        {
            if (entity.ClientId == 0 || entity.ClientCode == "" || entity.WhCode == "" || entity.LocationId == "" || entity.LocationId == null)
            {
                return null;
            }
            entity.ReceiptId = "EI" + DI.IDGenerator.NewId;
            entity.WhCode = entity.WhCode;
            entity.Status = "U";
            entity.DSFLag = 0;
            entity.LoadMasterId = 0;
            entity.OutBoundOrderId = 0;
            entity.HoldOutBoundOrderId = 0;
            entity.RegisterDate = DateTime.Now;
            entity.CreateDate = DateTime.Now;

            idal.IReceiptRegisterDAL.Add(entity);
            idal.IReceiptRegisterDAL.SaveChanges();
            return entity;
        }

        //添加 收货登记头表 WMS使用 状态为 未释放 
        public ReceiptRegister AddReceiptRegister1(ReceiptRegister entity)
        {
            if (entity.ClientId == 0 || entity.ClientCode == "" || entity.WhCode == "" || entity.LocationId == "" || entity.LocationId == null)
            {
                return null;
            }

            ////删除45天前 未收货的登记信息 
            //DateTime? getbegin = DateTime.Now.AddDays(-45);
            //List<ReceiptRegister> checkStatusReg = idal.IReceiptRegisterDAL.SelectBy(u => u.WhCode == entity.WhCode && u.Status == "U" && u.CreateDate <= getbegin);
            //if (checkStatusReg.Count > 0)
            //{
            //    foreach (var item in checkStatusReg)
            //    {
            //        item.UpdateUser = "1008";
            //        DelReceiptRegister(item);
            //    }
            //}

            //验证是否 存在 客户名、车牌号、库区 已录入但未释放的收货登记头
            List<ReceiptRegister> checkReg = idal.IReceiptRegisterDAL.SelectBy(u => u.ClientCode == entity.ClientCode && u.WhCode == entity.WhCode && u.LocationId == entity.LocationId && u.TruckNumber == entity.TruckNumber && u.Status == "N" && u.ReceiptType == entity.ReceiptType);
            if (checkReg.Count > 0)
            {
                ReceiptRegister reg = checkReg.First();

                TimeSpan d3 = DateTime.Now.Subtract(Convert.ToDateTime(reg.RegisterDate));
                int getDay = d3.Days;
                if (getDay > 2)
                {
                    idal.IReceiptRegisterDAL.DeleteBy(u => u.Id == reg.Id);
                    idal.IReceiptRegisterDetailDAL.DeleteBy(u => u.WhCode == reg.WhCode && u.ReceiptId == reg.ReceiptId);

                    entity.ReceiptId = "EI" + DI.IDGenerator.NewId;
                    entity.WhCode = entity.WhCode;
                    entity.Status = "N";
                    entity.DSFLag = 0;
                    entity.LoadMasterId = 0;
                    entity.OutBoundOrderId = 0;
                    entity.HoldOutBoundOrderId = 0;
                    entity.RegisterDate = DateTime.Now;
                    entity.CreateDate = DateTime.Now;

                    idal.IReceiptRegisterDAL.Add(entity);
                    idal.IReceiptRegisterDAL.SaveChanges();
                    return entity;
                }
                else
                {
                    return reg;
                }
            }
            else
            {
                entity.ReceiptId = "EI" + DI.IDGenerator.NewId;
                entity.WhCode = entity.WhCode;
                entity.Status = "N";
                entity.DSFLag = 0;
                entity.LoadMasterId = 0;
                entity.OutBoundOrderId = 0;
                entity.HoldOutBoundOrderId = 0;
                entity.RegisterDate = DateTime.Now;
                entity.CreateDate = DateTime.Now;

                idal.IReceiptRegisterDAL.Add(entity);
                idal.IReceiptRegisterDAL.SaveChanges();
                return entity;
            }
        }

        //添加 收货登记明细  给第三方导入使用
        public string AddReceiptRegisterDetail(List<ReceiptRegisterInsert> listEntity)
        {
            if (listEntity == null)
            {
                return "数据有误，请重新操作！";
            }

            ReceiptRegisterInsert regInsert = listEntity.First();
            ReceiptRegister reg = idal.IReceiptRegisterDAL.SelectBy(u => u.ReceiptId == regInsert.ReceiptId && u.WhCode == regInsert.WhCode).First();
            if (reg.Status != "U")
            {
                return "收货批次状态有误，请重新查询！";
            }

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

            //修改收货批次流程
            ReceiptRegisterInsert recInsert = listEntity.First();
            ReceiptRegister recReg = new ReceiptRegister();
            recReg.ReceiptId = recInsert.ReceiptId;
            recReg.WhCode = recInsert.WhCode;
            recReg.ProcessId = recInsert.ProcessId;
            recReg.ProcessName = recInsert.ProcessName;
            EditReceiptRegister(recReg, new string[] { "ProcessId", "ProcessName" });

            //修改收货批次状态 及 登记总数量
            UpdateRegReceipt(reg.WhCode, reg.ReceiptId);
            return "Y";
        }

        //添加 收货登记明细 WMS使用
        public string AddReceiptRegisterDetail1(List<ReceiptRegisterInsert> listEntity)
        {
            if (listEntity == null)
            {
                return "数据有误，请重新操作！";
            }

            ReceiptRegisterInsert regInsert = listEntity.First();
            ReceiptRegister reg = idal.IReceiptRegisterDAL.SelectBy(u => u.ReceiptId == regInsert.ReceiptId && u.WhCode == regInsert.WhCode).First();
            if (reg.Status != "N")
            {
                return "收货批次状态有误，请重新查询！";
            }

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

            //修改收货批次流程
            ReceiptRegisterInsert recInsert = listEntity.First();
            ReceiptRegister recReg = new ReceiptRegister();
            recReg.ReceiptId = recInsert.ReceiptId;
            recReg.WhCode = recInsert.WhCode;
            recReg.ProcessId = recInsert.ProcessId;
            recReg.ProcessName = recInsert.ProcessName;
            EditReceiptRegister1(recReg, new string[] { "ProcessId", "ProcessName" });
            return "Y";
        }

        //修改收货登记  给第三方导入使用
        public string EditReceiptRegister(ReceiptRegister entity, params string[] modifiedProNames)
        {
            if (entity == null || entity.ReceiptId == "" || entity.ReceiptId == null)
            {
                return "数据有误，请重新操作！";
            }
            ReceiptRegister reg = idal.IReceiptRegisterDAL.SelectBy(u => u.ReceiptId == entity.ReceiptId && u.WhCode == entity.WhCode).First();
            if (reg.Status != "U")
            {
                return "收货批次状态有误，请重新查询！";
            }

            entity.UpdateDate = DateTime.Now;
            idal.IReceiptRegisterDAL.UpdateBy(entity, u => u.ReceiptId == entity.ReceiptId && u.WhCode == entity.WhCode, modifiedProNames);
            idal.IReceiptRegisterDAL.SaveChanges();
            return "Y";
        }

        //修改收货登记  WMS使用
        public string EditReceiptRegister1(ReceiptRegister entity, params string[] modifiedProNames)
        {
            if (entity == null || entity.ReceiptId == "" || entity.ReceiptId == null)
            {
                return "数据有误，请重新操作！";
            }
            ReceiptRegister reg = idal.IReceiptRegisterDAL.SelectBy(u => u.ReceiptId == entity.ReceiptId && u.WhCode == entity.WhCode).First();
            //if (reg.Status != "N" && reg.Status != "U")
            //{
            //    return "收货批次状态有误，请重新查询！";
            //}

            if ((entity.LocationId ?? "") != "")
            {
                TranLog tl = new TranLog();
                tl.TranType = "75";
                tl.Description = "收货登记修改";
                tl.TranDate = DateTime.Now;
                tl.TranUser = entity.UpdateUser;
                tl.WhCode = entity.WhCode;
                tl.ReceiptId = entity.ReceiptId;
                tl.Remark = "库区" + reg.LocationId + "车牌" + reg.TruckNumber + "手机" + reg.PhoneNumber;
                idal.ITranLogDAL.Add(tl);
            }
            entity.UpdateDate = DateTime.Now;
            idal.IReceiptRegisterDAL.UpdateBy(entity, u => u.ReceiptId == entity.ReceiptId && u.WhCode == entity.WhCode, modifiedProNames);
            idal.IReceiptRegisterDAL.SaveChanges();



            //修改对接GMS信息
            if (modifiedProNames.Count() != 2) { 
            GMSManager gms = new GMSManager();
            gms.UpdateTruckInfoByReceiptRegister(entity, modifiedProNames);
            }
            return "Y";
        }

        //修改收货登记明细  WMS使用
        public string EditReceiptRegisterDetail(ReceiptRegisterDetailEdit entity)
        {
            if (entity == null)
            {
                return "数据有误，请重新操作！";
            }

            ReceiptRegister reg = idal.IReceiptRegisterDAL.SelectBy(u => u.ReceiptId == entity.ReceiptId && u.WhCode == entity.WhCode).First();
            if (reg.Status != "N")
            {
                return "收货批次状态有误，请重新查询！";
            }

            InBoundOrderDetail inOrderDetail = idal.IInBoundOrderDetailDAL.SelectBy(u => u.Id == entity.InOrderDetailId).First();
            if (inOrderDetail.RegQty < entity.DiffQty)
            {
                return "登记数量超支无法删除，请联系IT处理！";
            }
            inOrderDetail.RegQty = inOrderDetail.RegQty - entity.DiffQty;
            inOrderDetail.UpdateUser = entity.UpdateUser;
            inOrderDetail.UpdateDate = entity.UpdateDate;
            idal.IInBoundOrderDetailDAL.UpdateBy(inOrderDetail, u => u.Id == entity.InOrderDetailId, "RegQty");

            ReceiptRegisterDetail recDetail = new ReceiptRegisterDetail();
            recDetail.RegQty = entity.RegQty;
            recDetail.UpdateUser = entity.UpdateUser;
            recDetail.UpdateDate = entity.UpdateDate;

            idal.IReceiptRegisterDetailDAL.UpdateBy(recDetail, u => u.Id == entity.Id, "RegQty");
            idal.IReceiptRegisterDetailDAL.SaveChanges();
            return "Y";
        }

        //删除收货登记明细  WMS使用
        public string DelReceiptRegisterDetail(ReceiptRegisterDetailEdit entity)
        {
            if (entity == null || entity.Id == 0)
            {
                return "数据有误，请重新操作！";
            }

            ReceiptRegisterDetail regDetail = idal.IReceiptRegisterDetailDAL.SelectBy(u => u.Id == entity.Id).First();

            ReceiptRegister reg = idal.IReceiptRegisterDAL.SelectBy(u => u.WhCode == regDetail.WhCode && u.ReceiptId == regDetail.ReceiptId).First();
            if (reg.Status != "N")
            {
                return "收货批次状态有误，请重新查询！";
            }

            idal.IReceiptRegisterDetailDAL.DeleteBy(u => u.Id == entity.Id);

            InBoundOrderDetail inOrderDetail = idal.IInBoundOrderDetailDAL.SelectBy(u => u.Id == entity.InOrderDetailId).First();
            inOrderDetail.RegQty = inOrderDetail.RegQty - entity.RegQty;
            inOrderDetail.UpdateUser = entity.UpdateUser;
            inOrderDetail.UpdateDate = entity.UpdateDate;
            idal.IInBoundOrderDetailDAL.UpdateBy(inOrderDetail, u => u.Id == entity.InOrderDetailId, "RegQty");
            idal.IInBoundOrderDetailDAL.SaveChanges();
            return "Y";
        }

        //删除收货登记  WMS使用
        public string DelReceiptRegister(ReceiptRegister entity)
        {
            if (entity == null || entity.ReceiptId == "" || entity.ReceiptId == null)
            {
                return "数据有误，请重新操作！";
            }
            List<ReceiptRegister> regL;
            regL = idal.IReceiptRegisterDAL.SelectBy(u => u.ReceiptId == entity.ReceiptId && u.WhCode == entity.WhCode);

            if (regL.Count == 0)
            {
                return "未查询到该批次号码!";
            }

            ReceiptRegister reg = regL.First();

            if (reg.Status != "N" && reg.Status != "U")
            {
                return "收货批次状态有误，请重新查询！";
            }

            List<ReceiptRegisterDetail> listReg = (from a in idal.IReceiptRegisterDetailDAL.SelectAll()
                                                   where a.ReceiptId == entity.ReceiptId && a.WhCode == entity.WhCode
                                                   select a).ToList();
            string result = "";
            if (listReg.Count > 0)
            {
                int[] inorderDetailIdArr = (from a in listReg select a.InOrderDetailId).Distinct().ToArray();

                List<InBoundOrderDetail> inOrderDetailList = idal.IInBoundOrderDetailDAL.SelectBy(u => inorderDetailIdArr.Contains(u.Id));

                foreach (var item in listReg)
                {
                    List<InBoundOrderDetail> getinOrderDetailList = inOrderDetailList.Where(u => u.Id == item.InOrderDetailId).ToList();

                    if (getinOrderDetailList.Count == 0)
                    {
                        continue;
                    }
                    InBoundOrderDetail inOrderDetail = getinOrderDetailList.First();
                    if (inOrderDetail.RegQty < item.RegQty)
                    {
                        result = "登记数量超支无法删除，请联系IT处理！";
                        break;
                    }
                    inOrderDetail.RegQty = inOrderDetail.RegQty - item.RegQty;
                    inOrderDetail.UpdateUser = entity.UpdateUser;
                    inOrderDetail.UpdateDate = entity.UpdateDate;
                    idal.IInBoundOrderDetailDAL.UpdateBy(inOrderDetail, u => u.Id == item.InOrderDetailId, "RegQty");
                }
            }
            if (result != "")
            {
                return result;
            }

            TranLog tl = new TranLog();
            tl.TranType = "70";
            tl.Description = "删除收货登记";
            tl.TranDate = DateTime.Now;
            tl.TranUser = entity.UpdateUser;
            tl.WhCode = entity.WhCode;
            tl.ReceiptId = entity.ReceiptId;
            idal.ITranLogDAL.Add(tl);

            idal.IReceiptRegisterDAL.DeleteBy(u => u.ReceiptId == entity.ReceiptId && u.WhCode == entity.WhCode);
            idal.IReceiptRegisterDetailDAL.DeleteBy(u => u.ReceiptId == entity.ReceiptId && u.WhCode == entity.WhCode);
            idal.IReceiptRegisterDAL.SaveChanges();



            //删除关联GMS信息
            GMSManager GMS = new GMSManager();
            GMS.DeleteTruckInfoByReceiptRegister(entity.ReceiptId, entity.WhCode);




            return "Y";
        }

        //验证收货批次流程是否选择有误
        public string CheckReceiptProssName(ReceiptRegisterInsert entity)
        {
            if (entity == null || entity.ReceiptId == "" || entity.ReceiptId == null)
            {
                return "数据有误，请重新操作！";
            }
            string result = "";
            ReceiptRegister recReg = idal.IReceiptRegisterDAL.SelectBy(u => u.ReceiptId == entity.ReceiptId && u.WhCode == entity.WhCode).First();
            if (recReg.ProcessId.ToString() != "")
            {
                if (recReg.ProcessId != entity.ProcessId || recReg.ProcessName != entity.ProcessName)
                {
                    result = "流程选择有误，已有流程：" + recReg.ProcessName + "，当前所选：" + entity.ProcessName;
                }
            }
            return result;
        }


        #endregion


        #region 1.1收货登记查询（整出）

        //添加进仓单 查询(整出)  
        //对应RegInBoundOrderController中的 List_CFS 方法
        public List<InBoundOrderDetailResult> RegInBoundOrderListCFS(InBoundOrderDetailSearch searchEntity, string[] soNumberList, string[] poNumberList, string[] skuNumberList, out int total, out string str)
        {
            var sql = from a in idal.IInBoundSODAL.SelectAll()
                      where a.WhCode == searchEntity.WhCode
                      join b in idal.IInBoundOrderDAL.SelectAll()
                      on new { B = a.Id } equals new { B = (int)b.SoId }
                      join d in idal.IInBoundOrderDetailDAL.SelectAll()
                      on new { B = b.Id } equals new { B = d.PoId }
                      join c in idal.IItemMasterDAL.SelectAll()
                      on new { B = d.ItemId } equals new { B = c.Id }
                      where b.OrderType == searchEntity.OrderType
                      select new InBoundOrderDetailResult
                      {
                          Id = d.Id,
                          WhCode = d.WhCode,
                          ClientId = a.ClientId,
                          ClientCode = a.ClientCode,
                          ProcessId = b.ProcessId,
                          ProcessName = b.ProcessName,
                          UnitId = d.UnitId,
                          UnitName = d.UnitName == "none" ? "" : d.UnitName,
                          SoNumber = a.SoNumber,
                          CustomerPoNumber = b.CustomerPoNumber,
                          AltItemNumber = c.AltItemNumber,
                          PoId = d.PoId,
                          ItemId = d.ItemId,
                          Style1 = c.Style1,
                          Style2 = c.Style2,
                          Style3 = c.Style3,
                          Qty = (d.Qty - d.RegQty),
                          Weight = d.Weight,
                          CBM = d.CBM
                      };

            if (soNumberList != null)
                sql = sql.Where(u => soNumberList.Contains(u.SoNumber));
            if (poNumberList != null)
                sql = sql.Where(u => poNumberList.Contains(u.CustomerPoNumber));
            if (skuNumberList != null)
                sql = sql.Where(u => skuNumberList.Contains(u.AltItemNumber));

            if (searchEntity.ClientId > 0)
            {
                sql = sql.Where(u => u.ClientId == searchEntity.ClientId);
            }
            if (searchEntity.ProcessId > 0)
            {
                sql = sql.Where(u => u.ProcessId == searchEntity.ProcessId);
            }
            sql = sql.Where(u => u.Qty > 0);

            total = sql.Count();
            str = "";
            if (total > 0)
            {
                str = "{\"可登记数量\":\"" + sql.Sum(u => u.Qty).ToString() + "\"}";
            }
            sql = sql.OrderBy(u => u.SoNumber).ThenBy(u => u.CustomerPoNumber).ThenBy(u => u.AltItemNumber);
            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);
            return sql.ToList();
        }

        //显示收货批次明细 （整出）
        //对应RegInBoundOrderController中的 ReceiptRegisterDetailList 方法
        public List<ReceiptRegisterDetailResult> ReceiptRegisterDetailList(ReceiptRegisterDetailSearch searchEntity, out int total, out string str)
        {
            var sql = from a in idal.IReceiptRegisterDetailDAL.SelectAll()
                      where a.ReceiptId == searchEntity.ReceiptId && a.WhCode == searchEntity.WhCode
                      join b in idal.IInBoundOrderDAL.SelectAll()
                      on new { A = a.PoId } equals new { A = b.Id }
                      join c in idal.IItemMasterDAL.SelectAll()
                      on new { A = a.ItemId } equals new { A = c.Id }
                      join d in idal.IInBoundSODAL.SelectAll()
                      on b.SoId equals d.Id
                      select new ReceiptRegisterDetailResult
                      {
                          Id = a.Id,
                          InOrderDetailId = a.InOrderDetailId,
                          SoNumber = d.SoNumber,
                          CustomerPoNumber = b.CustomerPoNumber,
                          AltItemNumber = c.AltItemNumber,
                          Style1 = c.Style1,
                          Style2 = c.Style2,
                          Style3 = c.Style3,
                          RegQty = a.RegQty,
                          UnitName = a.UnitName == "none" ? "" : a.UnitName
                      };
            total = sql.Count();
            str = "";
            if (total > 0)
            {
                str = "{\"登记数量\":\"" + sql.Sum(u => u.RegQty).ToString() + "\"}";
            }
            sql = sql.OrderBy(u => u.SoNumber).ThenBy(u => u.CustomerPoNumber).ThenBy(u => u.AltItemNumber);
            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);
            return sql.ToList();
        }

        #endregion


        #region 1.2 收货登记查询（非整出）

        //添加进仓单 查询(非整出)  
        //对应RegInBoundOrderController中的 AddInBoundOrder 方法
        public List<InBoundOrderDetailResult> RegInBoundOrderListDC(InBoundOrderDetailSearch searchEntity, string[] poNumberList, string[] skuNumberList, out int total, out string str)
        {
            var sql = from a in idal.IInBoundOrderDAL.SelectAll()
                      where a.WhCode == searchEntity.WhCode && a.OrderType == searchEntity.OrderType
                      join d in idal.IInBoundOrderDetailDAL.SelectAll()
                      on new { C = a.Id } equals new { C = d.PoId }
                      join c in idal.IItemMasterDAL.SelectAll()
                      on new { B = d.ItemId } equals new { B = c.Id }
                      select new InBoundOrderDetailResult
                      {
                          Id = d.Id,
                          WhCode = d.WhCode,
                          ClientId = a.ClientId,
                          ClientCode = a.ClientCode,
                          ProcessId = a.ProcessId,
                          ProcessName = a.ProcessName,
                          UnitId = d.UnitId,
                          UnitName = d.UnitName == "none" ? "" : d.UnitName,
                          CustomerPoNumber = a.CustomerPoNumber,
                          AltItemNumber = c.AltItemNumber,
                          PoId = d.PoId,
                          ItemId = d.ItemId,
                          Style1 = c.Style1,
                          Style2 = c.Style2,
                          Style3 = c.Style3,
                          Qty = (d.Qty - d.RegQty),
                          Weight = d.Weight,
                          CBM = d.CBM
                      };

            if (poNumberList != null)
                sql = sql.Where(u => poNumberList.Contains(u.CustomerPoNumber));
            if (skuNumberList != null)
                sql = sql.Where(u => skuNumberList.Contains(u.AltItemNumber));

            if (searchEntity.ClientId > 0)
            {
                sql = sql.Where(u => u.ClientId == searchEntity.ClientId);
            }
            if (searchEntity.ProcessId > 0)
            {
                sql = sql.Where(u => u.ProcessId == searchEntity.ProcessId);
            }
            sql = sql.Where(u => u.Qty > 0);

            total = sql.Count();
            str = "{\"可登记数量\":\"" + sql.Sum(u => u.Qty).ToString() + "\"}";

            sql = sql.OrderByDescending(u => u.Id);
            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);
            return sql.ToList();
        }

        //显示收货批次明细(非整出)
        //对应RegInBoundOrderController中的 ReceiptRegisterDetailPartList 方法
        public List<ReceiptRegisterDetailResult> ReceiptRegisterDetailPartList(ReceiptRegisterDetailSearch searchEntity, out int total, out string str)
        {
            var sql = from a in idal.IReceiptRegisterDetailDAL.SelectAll()
                      where a.ReceiptId == searchEntity.ReceiptId && a.WhCode == searchEntity.WhCode
                      join b in idal.IInBoundOrderDAL.SelectAll()
                      on new { A = a.PoId } equals new { A = b.Id }
                      join c in idal.IItemMasterDAL.SelectAll()
                      on new { A = a.ItemId } equals new { A = c.Id }
                      select new ReceiptRegisterDetailResult
                      {
                          Id = a.Id,
                          InOrderDetailId = a.InOrderDetailId,
                          CustomerPoNumber = b.CustomerPoNumber,
                          AltItemNumber = c.AltItemNumber,
                          Style1 = c.Style1,
                          Style2 = c.Style2,
                          Style3 = c.Style3,
                          RegQty = a.RegQty,
                          UnitName = a.UnitName == "none" ? "" : a.UnitName
                      };
            total = sql.Count();
            str = "";
            if (total > 0)
            {
                str = "{\"登记数量\":\"" + sql.Sum(u => u.RegQty).ToString() + "\"}";
            }
            sql = sql.OrderBy(u => u.CustomerPoNumber).ThenBy(u => u.AltItemNumber);
            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);
            return sql.ToList();
        }

        #endregion


        #region 1.3收货登记查询（通用）

        //添加进仓单 查询(通用)  
        //对应RegInBoundOrderController中的 List_Com 方法
        public List<InBoundOrderDetailResult> RegInBoundOrderListCom(InBoundOrderDetailSearch searchEntity, string[] soNumberList, string[] poNumberList, string[] skuNumberList, out int total, out string str)
        {
            var sql = from a in idal.IInBoundOrderDAL.SelectAll()
                      where a.WhCode == searchEntity.WhCode
                      join b in idal.IInBoundOrderDetailDAL.SelectAll()
                            on new { a.Id }
                        equals new { Id = b.PoId }
                      join c in idal.IItemMasterDAL.SelectAll()
                            on new { b.ItemId }
                        equals new { ItemId = c.Id }
                      join d in idal.IInBoundSODAL.SelectAll()
                            on new { SoId = (Int32)a.SoId }
                        equals new { SoId = d.Id } into d_join
                      from d in d_join.DefaultIfEmpty()
                      select new InBoundOrderDetailResult
                      {
                          Id = b.Id,        //ID为明细主键ID
                          WhCode = b.WhCode,
                          ClientId = a.ClientId,
                          ClientCode = a.ClientCode,
                          ProcessId = a.ProcessId,
                          ProcessName = a.ProcessName,
                          UnitId = b.UnitId,
                          UnitName = b.UnitName == "none" ? "" : b.UnitName,
                          SoNumber = d.SoNumber,
                          CustomerPoNumber = a.CustomerPoNumber,
                          AltItemNumber = c.AltItemNumber,
                          PoId = b.PoId,
                          ItemId = b.ItemId,
                          Style1 = c.Style1,
                          Style2 = c.Style2,
                          Style3 = c.Style3,
                          Qty = (b.Qty - b.RegQty),
                          Weight = b.Weight,
                          CBM = b.CBM
                      };

            if (soNumberList != null)
                sql = sql.Where(u => soNumberList.Contains(u.SoNumber));
            if (poNumberList != null)
                sql = sql.Where(u => poNumberList.Contains(u.CustomerPoNumber));
            if (skuNumberList != null)
                sql = sql.Where(u => skuNumberList.Contains(u.AltItemNumber));

            if (searchEntity.ClientId > 0)
            {
                sql = sql.Where(u => u.ClientId == searchEntity.ClientId);
            }
            if (searchEntity.ProcessId > 0)
            {
                sql = sql.Where(u => u.ProcessId == searchEntity.ProcessId);
            }
            sql = sql.Where(u => u.Qty > 0);

            total = sql.Count();
            str = "{\"可登记数量\":\"" + sql.Sum(u => u.Qty).ToString() + "\"}";
            sql = sql.OrderByDescending(u => u.Id).ThenBy(u => u.SoNumber).ThenBy(u => u.CustomerPoNumber).ThenBy(u => u.AltItemNumber);
            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);
            return sql.ToList();
        }

        //显示收货批次明细 （通用）
        //对应RegInBoundOrderController中的 ReceiptRegisterDetailListCom 方法
        public List<ReceiptRegisterDetailResult> ReceiptRegisterDetailListCom(ReceiptRegisterDetailSearch searchEntity, out int total, out string str)
        {
            var sql = from a in idal.IReceiptRegisterDetailDAL.SelectAll()
                      where a.ReceiptId == searchEntity.ReceiptId && a.WhCode == searchEntity.WhCode
                      join b in idal.IInBoundOrderDAL.SelectAll()
                      on new { A = a.PoId } equals new { A = b.Id }
                      join c in idal.IItemMasterDAL.SelectAll()
                      on new { A = a.ItemId } equals new { A = c.Id }
                      join d in idal.IInBoundSODAL.SelectAll()
                      on b.SoId equals d.Id into temp1
                      from ddb in temp1.DefaultIfEmpty()
                      select new ReceiptRegisterDetailResult
                      {
                          Id = a.Id,
                          InOrderDetailId = a.InOrderDetailId,
                          SoNumber = ddb.SoNumber,
                          CustomerPoNumber = b.CustomerPoNumber,
                          AltItemNumber = c.AltItemNumber,
                          Style1 = c.Style1,
                          Style2 = c.Style2,
                          Style3 = c.Style3,
                          RegQty = a.RegQty,
                          UnitName = a.UnitName == "none" ? "" : a.UnitName
                      };

            total = sql.Count();
            str = "";
            if (total > 0)
            {
                str = "{\"登记数量\":\"" + sql.Sum(u => u.RegQty).ToString() + "\"}";
            }

            sql = sql.OrderBy(u => u.SoNumber).ThenBy(u => u.CustomerPoNumber).ThenBy(u => u.AltItemNumber);
            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);
            return sql.ToList();
        }

        #endregion


        //更新收货批次状态及 登记总数量、总立方，同时 更新车辆排队放车信息
        public string UpdateRegReceipt(string whCode, string receiptId)
        {
            if (string.IsNullOrEmpty(whCode) || string.IsNullOrEmpty(receiptId))
            {
                return "收货批次号有误！";
            }

            List<ReceiptRegister> regList = idal.IReceiptRegisterDAL.SelectBy(u => u.WhCode == whCode && u.ReceiptId == receiptId);

            List<ReceiptRegisterDetail> regDetailList = idal.IReceiptRegisterDetailDAL.SelectBy(u => u.WhCode == whCode && u.ReceiptId == receiptId);
            if (regDetailList.Count > 0 && regList.Count > 0)
            {
                int sumQty = regDetailList.Sum(u => u.RegQty);
                decimal? sumCBM = 0;
                foreach (var item in regDetailList)
                {
                    InBoundOrderDetail fir = idal.IInBoundOrderDetailDAL.SelectBy(u => u.Id == item.InOrderDetailId).First();

                    decimal? cbm = (fir.CBM ?? 0) / (fir.Qty == 0 ? 1 : fir.Qty);
                    sumCBM += cbm * item.RegQty;
                }

                ReceiptRegister reg = regList.First();
                reg.SumQty = sumQty;
                reg.SumCBM = sumCBM;
                reg.Status = "U";
                reg.TruckStatus = 0;

                //先检查 车辆排队系统是否删除过
                List<ReceiptTruck> regTruckList = idal.IReceiptTruckDAL.SelectBy(u => u.WhCode == whCode && u.TruckNumber == reg.TruckNumber);
                if (regTruckList.Count > 0)
                {
                    ReceiptTruck truck = regTruckList.First();

                    //24小时内的再次登记 放车时间有效
                    TimeSpan d3 = DateTime.Now.Subtract(Convert.ToDateTime(truck.CreateDate));
                    int getDay = d3.Hours;
                    if (getDay <= 24)
                    {
                        reg.TruckStatus = truck.TruckStatus;
                        reg.ParkingUser = truck.ParkingUser;
                        reg.ParkingDate = truck.ParkingDate;
                        reg.StorageDate = truck.StorageDate;
                    }

                    idal.IReceiptTruckDAL.DeleteBy(u => u.WhCode == whCode && u.TruckNumber == reg.TruckNumber);
                }
                else
                {
                    List<ReceiptRegister> regList1 = idal.IReceiptRegisterDAL.SelectBy(u => u.WhCode == whCode && u.TruckNumber == reg.TruckNumber && (u.TruckStatus == 10 || u.TruckStatus == 20));

                    if (regList1.Count > 0)
                    {
                        ReceiptRegister reg1 = regList1.First();

                        reg.TruckStatus = reg1.TruckStatus;
                        reg.ParkingUser = reg1.ParkingUser;
                        reg.ParkingDate = reg1.ParkingDate;
                        reg.StorageDate = reg1.StorageDate;
                    }
                }

                idal.IReceiptRegisterDAL.UpdateBy(reg, u => u.WhCode == whCode && u.ReceiptId == receiptId, new string[] { "SumQty", "SumCBM", "Status", "TruckStatus", "ParkingUser", "ParkingDate", "StorageDate" });

                idal.SaveChanges();
            }


            //插入GMS排队
            GMSManager gm = new GMSManager();
            gm.WmsCreateGms(whCode, receiptId);

            return "Y";
        }


        //释放收货批次号 验证 是否有直装
        public string ReleaseReceipt(string whCode, string receiptId)
        {
            if (string.IsNullOrEmpty(whCode) || string.IsNullOrEmpty(receiptId))
            {
                return "收货批次号有误！";
            }

            using (TransactionScope trans = new TransactionScope())
            {
                try
                {
                    List<ReceiptRegister> regList = idal.IReceiptRegisterDAL.SelectBy(u => u.WhCode == whCode && u.ReceiptId == receiptId);
                    if (regList.Count == 0)
                    {
                        return "收货批次号有误！";
                    }
                    ReceiptRegister reg = regList.First();
                    if (reg.Status != "N")
                    {
                        return "收货批次号状态有误！";
                    }

                    List<ReceiptRegisterDetail> regDetailList = idal.IReceiptRegisterDetailDAL.SelectBy(u => u.WhCode == whCode && u.ReceiptId == receiptId);
                    if (regDetailList.Count == 0)
                    {
                        return "收货批次号明细有误！";
                    }

                    #region 释放优化

                    int[] inorderDetailId = (from a in regDetailList select a.InOrderDetailId).ToArray();

                    //得到直装的InBoundOrderDetailId
                    var s = from a in (
                                    (from a in idal.IOutBoundOrderDetailDAL.SelectAll()
                                     join b in idal.IOutBoundOrderDAL.SelectAll()
                                     on new { OutBoundOrderId = a.OutBoundOrderId } equals new { OutBoundOrderId = b.Id }
                                     join c in idal.ILoadDetailDAL.SelectAll()
                                     on new { Id = b.Id } equals new { Id = (int)c.OutBoundOrderId } into extent3_join
                                     from c in extent3_join.DefaultIfEmpty()
                                     where b.StatusId != 40 && b.StatusId >= 20 && b.DSFLag == 1
                                     select new
                                     {
                                         OutBoundOrderId = a.OutBoundOrderId,
                                         LoadMasterId = c.LoadMasterId,
                                         InBoundOrderDetailId = a.InBoundOrderDetailId ?? 0,
                                         ClientId = b.ClientId,
                                         ClientCode = b.ClientCode,
                                         a.Qty
                                     }))
                            join b in (
                                        (from a0 in idal.IReceiptRegisterDAL.SelectAll()
                                         join b1 in idal.IReceiptRegisterDetailDAL.SelectAll()
                                        on new { a0.WhCode, a0.ReceiptId } equals new { b1.WhCode, b1.ReceiptId } into b1_join
                                         from b1 in b1_join.DefaultIfEmpty()
                                         where a0.Status != "C"
                                         group new { a0, b1 } by new
                                         {
                                             a0.LoadMasterId,
                                             a0.OutBoundOrderId,
                                             b1.InOrderDetailId
                                         } into g
                                         select new
                                         {
                                             LoadMasterId = g.Key.LoadMasterId,
                                             OutBoundOrderId = g.Key.OutBoundOrderId,
                                             InOrderDetailId = g.Key.InOrderDetailId,
                                             RegQty = g.Sum(p => p.b1.RegQty)
                                         }))
                             on new { a.LoadMasterId, OutBoundOrderId = (int?)a.OutBoundOrderId, a.InBoundOrderDetailId }
                                equals new { b.LoadMasterId, OutBoundOrderId = b.OutBoundOrderId, InBoundOrderDetailId = b.InOrderDetailId }
                             into b_join
                            from b in b_join.DefaultIfEmpty()
                            where (a.Qty - ((int?)b.RegQty ?? (int?)0)) > 0
                            select new DSReceiptRegisterResult
                            {
                                OutBoundOrderId = a.OutBoundOrderId,
                                LoadMasterId = (int)a.LoadMasterId,
                                InBoundOrderDetailId = a.InBoundOrderDetailId,
                                ClientId = a.ClientId,
                                ClientCode = a.ClientCode,
                                Qty = (int)(a.Qty - ((Int32?)b.RegQty ?? (Int32?)0))
                            };

                    List<DSReceiptRegisterResult> sql = s.ToList();
                    //验证是否有直装
                    sql = sql.Where(u => inorderDetailId.Contains(u.InBoundOrderDetailId)).ToList();

                    //如果有直装收货
                    if (sql.Count() > 0)
                    {
                        List<ReceiptRegister> receiptRegisterList = new List<ReceiptRegister>();        //要保存的登记头表
                        List<ReceiptRegisterDetail> receiptRegisterDetailList = new List<ReceiptRegisterDetail>();  //要保存的登记明细表
                        List<ReceiptRegisterDetail> deleteList = new List<ReceiptRegisterDetail>(); //最后要删除的收货登记
                        foreach (var item in sql)
                        {
                            if (receiptRegisterList.Where(u => u.LoadMasterId == item.LoadMasterId && u.OutBoundOrderId == item.OutBoundOrderId).Count() == 0)
                            {
                                ReceiptRegister entity = new ReceiptRegister();
                                entity.ReceiptId = "EI" + DI.IDGenerator.NewId;
                                entity.WhCode = whCode;
                                entity.ClientId = item.ClientId;
                                entity.ClientCode = item.ClientCode;
                                entity.RegisterDate = DateTime.Now;
                                entity.ReceiptType = reg.ReceiptType;
                                entity.ProcessId = reg.ProcessId;
                                entity.ProcessName = reg.ProcessName;
                                entity.LocationId = reg.LocationId;
                                entity.TransportType = reg.ReceiptId;
                                entity.TruckNumber = reg.TruckNumber;
                                entity.ArriveDate = reg.ArriveDate;
                                entity.BkDate = reg.BkDate;
                                entity.Status = "N";
                                entity.DSFLag = 1;
                                entity.LoadMasterId = item.LoadMasterId;
                                entity.OutBoundOrderId = item.OutBoundOrderId;
                                entity.HoldOutBoundOrderId = 0;
                                entity.CreateUser = reg.CreateUser;
                                entity.CreateDate = DateTime.Now;
                                receiptRegisterList.Add(entity);

                                ReceiptRegisterDetail getReceiptRegisterDetail = regDetailList.Where(u => u.InOrderDetailId == item.InBoundOrderDetailId).First();

                                //验证登记明细数量 是否被登记过
                                if (deleteList.Where(u => u.Id == getReceiptRegisterDetail.Id).Count() > 0)
                                {
                                    ReceiptRegisterDetail oldreg = deleteList.Where(u => u.Id == getReceiptRegisterDetail.Id).First();
                                    getReceiptRegisterDetail.RegQty = oldreg.RegQty;
                                    if (getReceiptRegisterDetail.RegQty > 1)
                                    {
                                        deleteList.Remove(oldreg);
                                    }
                                }
                                if (getReceiptRegisterDetail.RegQty < 1)
                                {
                                    continue;
                                }

                                //为防止登记明细数量 被重复使用 需记录下被登记过的数量
                                ReceiptRegisterDetail deleteRegDetail = new ReceiptRegisterDetail();
                                deleteRegDetail.Id = getReceiptRegisterDetail.Id;

                                ReceiptRegisterDetail receiptRegisterDetail = new ReceiptRegisterDetail();
                                receiptRegisterDetail.WhCode = whCode;
                                receiptRegisterDetail.ReceiptId = entity.ReceiptId;
                                receiptRegisterDetail.InOrderDetailId = getReceiptRegisterDetail.InOrderDetailId;
                                receiptRegisterDetail.CustomerPoNumber = getReceiptRegisterDetail.CustomerPoNumber;
                                receiptRegisterDetail.AltItemNumber = getReceiptRegisterDetail.AltItemNumber;
                                receiptRegisterDetail.PoId = getReceiptRegisterDetail.PoId;
                                receiptRegisterDetail.ItemId = getReceiptRegisterDetail.ItemId;
                                receiptRegisterDetail.UnitName = getReceiptRegisterDetail.UnitName;
                                receiptRegisterDetail.UnitId = getReceiptRegisterDetail.UnitId;
                                if (getReceiptRegisterDetail.RegQty >= item.Qty)
                                {
                                    receiptRegisterDetail.RegQty = item.Qty;
                                    deleteRegDetail.RegQty = getReceiptRegisterDetail.RegQty - item.Qty;
                                }
                                else if (getReceiptRegisterDetail.RegQty < item.Qty)
                                {
                                    receiptRegisterDetail.RegQty = getReceiptRegisterDetail.RegQty;
                                    deleteRegDetail.RegQty = 0;
                                }
                                receiptRegisterDetail.CreateUser = getReceiptRegisterDetail.CreateUser;
                                receiptRegisterDetail.CreateDate = getReceiptRegisterDetail.CreateDate;

                                receiptRegisterDetailList.Add(receiptRegisterDetail);
                                deleteList.Add(deleteRegDetail);
                            }
                            else
                            {
                                ReceiptRegister entity = receiptRegisterList.Where(u => u.LoadMasterId == item.LoadMasterId && u.OutBoundOrderId == item.OutBoundOrderId).First();

                                ReceiptRegisterDetail getReceiptRegisterDetail = regDetailList.Where(u => u.InOrderDetailId == item.InBoundOrderDetailId).First();
                                //验证登记明细数量 是否被登记过
                                if (deleteList.Where(u => u.Id == getReceiptRegisterDetail.Id).Count() > 0)
                                {
                                    ReceiptRegisterDetail oldreg = deleteList.Where(u => u.Id == getReceiptRegisterDetail.Id).First();
                                    getReceiptRegisterDetail.RegQty = oldreg.RegQty;
                                    if (getReceiptRegisterDetail.RegQty > 1)
                                    {
                                        deleteList.Remove(oldreg);
                                    }
                                }
                                if (getReceiptRegisterDetail.RegQty < 1)
                                {
                                    continue;
                                }

                                //为防止登记明细数量 被重复使用 需记录下被登记过的数量
                                ReceiptRegisterDetail deleteRegDetail = new ReceiptRegisterDetail();
                                deleteRegDetail.Id = getReceiptRegisterDetail.Id;

                                ReceiptRegisterDetail receiptRegisterDetail = new ReceiptRegisterDetail();
                                receiptRegisterDetail.WhCode = whCode;
                                receiptRegisterDetail.ReceiptId = entity.ReceiptId;
                                receiptRegisterDetail.InOrderDetailId = getReceiptRegisterDetail.InOrderDetailId;
                                receiptRegisterDetail.CustomerPoNumber = getReceiptRegisterDetail.CustomerPoNumber;
                                receiptRegisterDetail.AltItemNumber = getReceiptRegisterDetail.AltItemNumber;
                                receiptRegisterDetail.PoId = getReceiptRegisterDetail.PoId;
                                receiptRegisterDetail.ItemId = getReceiptRegisterDetail.ItemId;
                                receiptRegisterDetail.UnitName = getReceiptRegisterDetail.UnitName;
                                receiptRegisterDetail.UnitId = getReceiptRegisterDetail.UnitId;

                                if (getReceiptRegisterDetail.RegQty >= item.Qty)
                                {
                                    receiptRegisterDetail.RegQty = item.Qty;
                                    deleteRegDetail.RegQty = getReceiptRegisterDetail.RegQty - item.Qty;
                                }
                                else if (getReceiptRegisterDetail.RegQty < item.Qty)
                                {
                                    receiptRegisterDetail.RegQty = getReceiptRegisterDetail.RegQty;
                                    deleteRegDetail.RegQty = 0;
                                }

                                receiptRegisterDetail.CreateUser = getReceiptRegisterDetail.CreateUser;
                                receiptRegisterDetail.CreateDate = getReceiptRegisterDetail.CreateDate;

                                receiptRegisterDetailList.Add(receiptRegisterDetail);

                                deleteList.Add(deleteRegDetail);
                            }
                        }

                        idal.IReceiptRegisterDAL.Add(receiptRegisterList);
                        idal.IReceiptRegisterDetailDAL.Add(receiptRegisterDetailList);

                        //修改收货登记明细数量
                        foreach (var item in deleteList)
                        {
                            idal.IReceiptRegisterDetailDAL.UpdateBy(item, u => u.Id == item.Id, new string[] { "RegQty" });
                        }
                        idal.SaveChanges();

                        //修改直装订单状态和登记数量
                        foreach (var item in receiptRegisterList)
                        {
                            UpdateRegReceipt(item.WhCode, item.ReceiptId);
                        }
                        //删除原收货批次号下 数量为0的数据
                        idal.IReceiptRegisterDetailDAL.DeleteBy(u => u.WhCode == whCode && u.ReceiptId == receiptId && u.RegQty == 0);
                        idal.SaveChanges();

                        if (idal.IReceiptRegisterDetailDAL.SelectBy(u => u.WhCode == whCode && u.ReceiptId == receiptId && u.RegQty != 0).Count > 0)
                        {
                            UpdateRegReceipt(reg.WhCode, reg.ReceiptId);
                            trans.Complete();
                            return "Y$" + (receiptRegisterList.Count + 1);
                        }
                        else
                        {
                            idal.IReceiptRegisterDAL.DeleteByExtended(u => u.WhCode == whCode && u.ReceiptId == receiptId);
                            trans.Complete();
                            return "Y$" + (receiptRegisterList.Count);
                        }
                    }
                    else
                    {
                        UpdateRegReceipt(whCode, receiptId);
                        trans.Complete();
                        return "Y1$1";
                    }

                    

                    #endregion
                }
                catch
                {
                    trans.Dispose();//出现异常，事务手动释放
                    return "释放异常，请重新提交！";
                }
            }
        }

        //释放成功后 根据车牌号查询列表
        public List<ReceiptRegisterResult> DSReceiptRegisterList(ReceiptRegisterSearch searchEntity, out int total)
        {
            var sql = from a in idal.IReceiptRegisterDAL.SelectAll()
                      where a.WhCode == searchEntity.WhCode && a.ReceiptId == searchEntity.ReceiptId || a.TransportType == searchEntity.ReceiptId
                      select new ReceiptRegisterResult
                      {
                          Id = a.Id,
                          ReceiptId = a.ReceiptId,
                          DSFlag = a.DSFLag == 0 ? "普通" :
                           a.DSFLag == 1 ? "直装" : null,
                          Status =
                           a.Status == "N" ? "未释放" :
                           a.Status == "U" ? "未收货" :
                           a.Status == "A" ? "正在收货" :
                           a.Status == "C" ? "完成收货" : null,
                          ProcessName = a.ProcessName,
                          TruckNumber = a.TruckNumber,
                          LocationId = a.LocationId,
                          SumQty = a.SumQty
                      };

            total = sql.Count();
            sql = sql.OrderByDescending(u => u.ReceiptId);
            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);
            return sql.ToList();
        }

        //撤销释放收货登记   WMS使用
        public string RollbackReceiptId(ReceiptRegister entity)
        {
            if (entity.Id == 0)
            {
                return "数据有误，请重新操作！";
            }
            ReceiptRegister reg = idal.IReceiptRegisterDAL.SelectBy(u => u.Id == entity.Id).First();
            if (reg.Status != "U" || reg.DSFLag == 1)
            {
                return "收货批次状态有误，请重新查询！";
            }

            string result = "";
            List<ReceiptRegisterDetail> listReg = (from a in idal.IReceiptRegisterDetailDAL.SelectAll()
                                                   where a.ReceiptId == entity.ReceiptId && a.WhCode == entity.WhCode
                                                   select a).ToList();
            foreach (var item in listReg)
            {
                InBoundOrderDetail inOrderDetail = idal.IInBoundOrderDetailDAL.SelectBy(u => u.Id == item.InOrderDetailId).First();
                if (inOrderDetail.RegQty < item.RegQty)
                {
                    result = "登记数量超支无法撤销，请联系IT处理！";
                    break;
                }
                inOrderDetail.RegQty = inOrderDetail.RegQty - item.RegQty;
                inOrderDetail.UpdateUser = entity.UpdateUser;
                inOrderDetail.UpdateDate = DateTime.Now;
                idal.IInBoundOrderDetailDAL.UpdateBy(inOrderDetail, u => u.Id == item.InOrderDetailId, "RegQty");
            }
            if (result != "")
            {
                return result;
            }

            entity.SumCBM = 0;
            entity.SumQty = 0;
            entity.Status = "N";
            entity.PrintDate = null;
            entity.UpdateUser = entity.UpdateUser;
            entity.UpdateDate = DateTime.Now;
            idal.IReceiptRegisterDAL.UpdateBy(entity, u => u.Id == reg.Id, new string[] { "SumCBM", "SumQty", "Status", "UpdateUser", "UpdateDate", "PrintDate" });

            //5.插入记录
            TranLog tl = new TranLog();
            tl.TranType = "760";
            tl.Description = "撤销释放收货登记";
            tl.TranDate = DateTime.Now;
            tl.TranUser = entity.UpdateUser;
            tl.WhCode = reg.WhCode;
            tl.ReceiptId = reg.ReceiptId;
            tl.ClientCode = reg.ClientCode;
            idal.ITranLogDAL.Add(tl);

            idal.IReceiptRegisterDAL.SaveChanges();

            GMSManager GMS = new GMSManager();
            GMS.DeleteTruckInfoByReceiptRegister(reg.ReceiptId, reg.WhCode);



            return "Y";
        }


        //收货操作单模版
        public string PrintInTempalte(string whCode, string receiptId)
        {
            ReceiptRegister reg = idal.IReceiptRegisterDAL.SelectBy(u => u.WhCode == whCode && u.ReceiptId == receiptId).First();

            //是否直装
            if (reg.DSFLag == 1)
            {
                CRTemplate cr = idal.ICRTemplateDAL.SelectBy(u => u.WhCode == whCode && u.Type == "InDS").First();

                return cr.Url + cr.TemplateName + ".aspx";
            }
            else
            {
                List<CRTemplate> list = (from a in idal.IReceiptRegisterDAL.SelectAll()
                                         join b in idal.IFlowHeadDAL.SelectAll()
                                         on a.ProcessId equals b.Id
                                         where a.WhCode == whCode && a.ReceiptId == receiptId
                                         join c in idal.ICRTemplateDAL.SelectAll()
                                         on new { A = b.WhCode, B = b.InTemplate } equals new { A = c.WhCode, B = c.TemplateName } into temp1
                                         from c in temp1.DefaultIfEmpty()
                                         select c).ToList();
                if (list.Count == 0)
                {
                    return "";
                }

                CRTemplate cr = list.First();

                return cr.Url + cr.TemplateName + ".aspx";
            }
        }

        //更新收货批次联单数
        public string UpdateRegReceiptBillCount(ReceiptRegister entity)
        {
            if (string.IsNullOrEmpty(entity.WhCode) || string.IsNullOrEmpty(entity.ReceiptId))
            {
                return "收货批次号有误！";
            }

            idal.IReceiptRegisterDAL.UpdateBy(entity, u => u.WhCode == entity.WhCode && u.ReceiptId == entity.ReceiptId, new string[] { "BillCount" });

            idal.SaveChanges();

            return "Y";
        }
    }
}

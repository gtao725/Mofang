using MODEL_MSSQL;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMS.BLLClass;
using WMS.IBLL;

namespace WMS.BLL
{
    public class TruckManager : ITruckManager
    {
        IDAL.IDALSession idal = BLL.BLLHelper.GetDal();
        RecHelper recHelper = new RecHelper();
        EclInBoundManager oms = new EclInBoundManager();

        private static object o = new object();

        #region 车辆排队管理

        //货代下拉菜单列表
        public IEnumerable<WhAgent> WhAgentListSelect(string whCode)
        {
            var sql = from a in idal.IWhAgentDAL.SelectAll()
                      where a.WhCode == whCode
                      select a;
            sql = sql.OrderBy(u => u.AgentCode);
            return sql.AsEnumerable();
        }

        //车辆排队列表
        public List<ReceiptRegisterResult> TruckQueryList(ReceiptRegisterSearch searchEntity, out int total)
        {
            var sql = (from a in idal.IReceiptRegisterDAL.SelectAll()
                       join c in idal.IR_WhClient_WhAgentDAL.SelectAll()
                       on a.ClientId equals c.ClientId
                       join d in idal.IWhAgentDAL.SelectAll()
                       on c.AgentId equals d.Id
                       where a.WhCode == searchEntity.WhCode && a.Status != "N" && (a.TruckNumber ?? "") != ""
                       select new ReceiptRegisterResult
                       {
                           Action = "",
                           Action1 = "",
                           Action2 = "",
                           Id = a.Id,
                           ReceiptId = a.ReceiptId,
                           ClientCode = a.ClientCode,
                           Status =
                            a.Status == "N" ? "未释放" :
                            a.Status == "U" ? "未收货" :
                            a.Status == "A" ? "正在收货" :
                            a.Status == "P" ? "暂停收货" :
                            a.Status == "C" ? "完成收货" : null,
                           TruckStatus = a.TruckStatus ?? 0,
                           TruckStatusShow =
                            a.TruckStatus == null ? "等待" :
                            a.TruckStatus == 0 ? "等待" :
                            a.TruckStatus == 10 ? "放车" :
                            a.TruckStatus == 20 ? "在库" :
                            a.TruckStatus == 30 ? "离开" : null,
                           TruckNumber = a.TruckNumber,
                           PhoneNumber = a.PhoneNumber,
                           RegisterDate = a.RegisterDate,
                           LocationId = a.LocationId,
                           ArriveDate = a.ArriveDate,
                           CreateUser = a.CreateUser,
                           CreateDate = a.CreateDate,
                           DSFlag = a.DSFLag == 0 ? "普通" :
                           a.DSFLag == null ? "普通" :
                            a.DSFLag == 1 ? "直装" : null,
                           SumQty = a.SumQty,
                           SumCBM = a.SumCBM,
                           BkDate = a.BkDate,
                           BkDateBegin = a.BkDateBegin,
                           BkDateEnd = a.BkDateEnd,
                           QueueUpFLag = a.QueueUpFLag == 0 ? "" :
                            a.QueueUpFLag == null ? "" :
                            a.QueueUpFLag == 1 ? "是" : null,
                           QueueUpUser = a.QueueUpUser ?? "",
                           QueueUpRemark = a.QueueUpRemark,
                           BaoAnRemark = a.BaoAnRemark,
                           ParkingUser = a.ParkingUser ?? "",
                           ParkingDate = a.ParkingDate,
                           StorageDate = a.StorageDate,
                           DepartureDate = a.DepartureDate,
                           AgentCode = d.AgentCode,
                           GreenPassFlagShow = a.GreenPassFLag == 1 ? "是" : ""
                       }).Distinct();
            if (searchEntity.BeginCreateDate != null)
            {
                if (searchEntity.TruckStatus == "未离库")
                {
                    DateTime? getbegin = Convert.ToDateTime(searchEntity.BeginCreateDate).AddDays(-15);
                    sql = sql.Where(u => u.CreateDate >= getbegin);
                    sql = sql.Where(u => u.TruckStatus < 30);
                }
                else if (searchEntity.TruckStatus == "等待")
                {
                    DateTime? getbegin = Convert.ToDateTime(searchEntity.BeginCreateDate).AddDays(-15);
                    sql = sql.Where(u => u.CreateDate >= getbegin);
                    sql = sql.Where(u => (u.TruckStatus ?? 0) == 0);
                }
                else if (searchEntity.TruckStatus == "放车")
                {
                    sql = sql.Where(u => u.CreateDate >= searchEntity.BeginCreateDate);
                    sql = sql.Where(u => u.TruckStatus == 10);
                }
                else if (searchEntity.TruckStatus == "在库")
                {
                    sql = sql.Where(u => u.CreateDate >= searchEntity.BeginCreateDate);
                    sql = sql.Where(u => u.TruckStatus == 20);
                }
                else if (searchEntity.TruckStatus == "离开")
                {
                    sql = sql.Where(u => u.CreateDate >= searchEntity.BeginCreateDate);
                    sql = sql.Where(u => u.TruckStatus == 30);
                }
            }

            List<ReceiptRegisterResult> sqlList = sql.ToList();

            List<WhUser> userList = (from a in idal.IWhUserDAL.SelectAll()
                                     join b in idal.IWhInfoDAL.SelectAll()
                                     on a.CompanyId equals b.CompanyId
                                     where b.WhCode == searchEntity.WhCode
                                     select a).ToList();

            List<ReceiptRegisterResult> list1 = new List<ReceiptRegisterResult>();
            foreach (var item in sqlList)
            {
                ReceiptRegisterResult work = item;

                List<WhUser> userCheck = userList.Where(u => u.UserName == item.QueueUpUser).ToList();

                if (userCheck.Count > 0)
                {
                    WhUser user = userCheck.First();
                    work.QueueUpUser = user.UserNameCN;
                }

                List<WhUser> userCheck1 = userList.Where(u => u.UserName == item.ParkingUser).ToList();
                if (userCheck1.Count > 0)
                {
                    WhUser user = userCheck1.First();
                    work.ParkingUser = user.UserNameCN;
                }
                if (sqlList.Where(u => u.TruckNumber == item.TruckNumber).Count() > 1)
                {
                    work.OneTruckMoreNumber = "是";
                }

                list1.Add(work);
            }

            List<ReceiptRegisterResult> list = new List<ReceiptRegisterResult>();

            if (searchEntity.TruckStatus == "未离库" || searchEntity.TruckStatus == "等待")
            {
                ////得到主管插队列表
                //List<ReceiptRegisterResult> getQueueList = list1.Where(u => u.QueueUpFLag != "" && u.TruckStatus == 0).OrderBy(u => u.BkDateBegin).ThenBy(u => u.CreateDate).ToList();
                ////得到预约列表
                //List<ReceiptRegisterResult> getBkList = list1.Where(u => (u.BkDate ?? "") != "" && u.TruckStatus == 0).OrderBy(u => u.BkDateBegin).ThenBy(u => u.CreateDate).ToList();
                ////得到件数列表
                //List<ReceiptRegisterResult> getSmallQtyList = list1.Where(u => u.SumQty <= 50 && u.TruckStatus == 0).OrderBy(u => u.CreateDate).ToList();

                //按照库区排队
                string[] locationIdList = (from a in list1.OrderBy(u => u.LocationId)
                                           select a.LocationId).Distinct().ToArray();

                foreach (var location in locationIdList)
                {
                    int seq = 1;
                    //foreach (var item in getQueueList.Where(u => u.LocationId == location))
                    //{
                    //    if (list.Where(u => u.WhCode == item.WhCode && u.ReceiptId == item.ReceiptId).Count() == 0)
                    //    {
                    //        ReceiptRegisterResult work = item;
                    //        work.Sequence = seq;
                    //        seq++;
                    //        list.Add(item);
                    //    }

                    //}

                    //foreach (var item in getBkList.Where(u => u.LocationId == location))
                    //{
                    //    if (list.Where(u => u.WhCode == item.WhCode && u.ReceiptId == item.ReceiptId).Count() == 0)
                    //    {
                    //        ReceiptRegisterResult work = item;
                    //        work.Sequence = seq;
                    //        seq++;
                    //        list.Add(item);
                    //    }
                    //}

                    //foreach (var item in getSmallQtyList.Where(u => u.LocationId == location))
                    //{
                    //    if (list.Where(u => u.WhCode == item.WhCode && u.ReceiptId == item.ReceiptId).Count() == 0)
                    //    {
                    //        ReceiptRegisterResult work = item;
                    //        work.Sequence = seq;
                    //        seq++;
                    //        list.Add(item);
                    //    }
                    //}

                    foreach (var item in list1.Where(u => u.TruckStatus == 0 && u.LocationId == location).OrderBy(u => u.CreateDate))
                    {
                        if (list.Where(u => u.WhCode == item.WhCode && u.ReceiptId == item.ReceiptId).Count() == 0)
                        {
                            ReceiptRegisterResult work = item;
                            work.Sequence = seq;
                            seq++;
                            list.Add(item);
                        }
                    }

                    foreach (var item in list1.Where(u => u.TruckStatus > 0 && u.LocationId == location).OrderBy(u => u.CreateDate))
                    {
                        if (list.Where(u => u.WhCode == item.WhCode && u.ReceiptId == item.ReceiptId).Count() == 0)
                        {
                            ReceiptRegisterResult work = item;
                            list.Add(item);
                        }
                    }
                }
            }
            else
            {
                list = list1.ToList();
            }

            if (!string.IsNullOrEmpty(searchEntity.ReceiptId))
                list = list.Where(u => u.ReceiptId == searchEntity.ReceiptId).ToList();
            if (!string.IsNullOrEmpty(searchEntity.ClientCode))
                list = list.Where(u => u.ClientCode == searchEntity.ClientCode).ToList();
            if (!string.IsNullOrEmpty(searchEntity.AgentCode))
                list = list.Where(u => u.AgentCode == searchEntity.AgentCode).ToList();

            if (!string.IsNullOrEmpty(searchEntity.TruckNumber))
                list = list.Where(u => u.TruckNumber == searchEntity.TruckNumber).ToList();

            if (!string.IsNullOrEmpty(searchEntity.LocationId))
                list = list.Where(u => u.LocationId == searchEntity.LocationId).ToList();

            if (searchEntity.EndCreateDate != null)
            {
                list = list.Where(u => u.CreateDate <= searchEntity.EndCreateDate).ToList();
            }

            total = list.Count();
            list = list.OrderBy(u => u.Sequence).ThenBy(u => u.LocationId).ToList();
            list = list.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize).ToList();
            return list;
        }

        //放车
        public string EditTruckStatus(string whCode, string receiptId, string userName)
        {
            lock (o)
            {
                ////是否需要 检查排队车辆少于多少时 理货员只能抢几个的设置？

                //List<ReceiptTruck> checkTruckCountList = idal.IReceiptTruckDAL.SelectBy(u => u.CreateUser == userName);
                //if (checkTruckCountList.Count >= 3)
                //{
                //    return "当前用户在库车辆已达上限，无法继续放车！";
                //}

                //List<ReceiptTruck> checkReceiptList = idal.IReceiptTruckDAL.SelectBy(u => u.WhCode == whCode && u.ReceiptId == receiptId);
                //if (checkReceiptList.Count > 0)
                //{
                //    return "当前车辆已被放车，请重新放车！";
                //}

                List<ReceiptRegister> ReceiptRegisterList = idal.IReceiptRegisterDAL.SelectBy(u => u.WhCode == whCode && u.ReceiptId == receiptId);
                if (ReceiptRegisterList.Count == 0)
                {
                    return "收货批次未找到，请查看交易记录是否被删除！";
                }

                ReceiptRegister reg = ReceiptRegisterList.First();
                if ((reg.TruckStatus ?? 0) == 0)
                {
                    reg.TruckStatus = 10;       //10为放车状态
                    reg.ParkingUser = userName;
                    reg.ParkingDate = DateTime.Now;
                    idal.IReceiptRegisterDAL.UpdateBy(reg, u => u.Id == reg.Id, new string[] { "TruckStatus", "ParkingUser", "ParkingDate" });

                    ReceiptRegister reg1 = new ReceiptRegister();
                    reg1.TruckStatus = 10;       //10为放车状态
                    reg1.ParkingUser = userName;
                    reg1.ParkingDate = DateTime.Now;

                    idal.IReceiptRegisterDAL.UpdateBy(reg1, u => u.WhCode == reg.WhCode && (u.TruckStatus ?? 0) == 0 && u.ReceiptId != reg.ReceiptId && u.TruckNumber == reg.TruckNumber, new string[] { "TruckStatus", "ParkingUser", "ParkingDate" });

                    ////工人与收货批次关系表
                    //ReceiptTruck truck = new ReceiptTruck();
                    //truck.WhCode = whCode;
                    //truck.ReceiptId = receiptId;
                    //truck.CreateUser = userName;
                    //truck.CreateDate = DateTime.Now;
                    //idal.IReceiptTruckDAL.Add(truck);
                }
                else if ((reg.TruckStatus ?? 0) == 10)
                {
                    return "当前车辆已放车，请重新查询！";
                }
                else if ((reg.TruckStatus ?? 0) == 20)
                {
                    return "当前车辆已在库，请重新查询！";
                }
                else if ((reg.TruckStatus ?? 0) == 30)
                {
                    return "当前车辆已离库，请重新查询！";
                }

                idal.SaveChanges();
                return "Y";
            }
        }

        //车辆插队
        public string QueueUpTruck(string whCode, string receiptId, string userName, string remark)
        {
            lock (o)
            {
                List<ReceiptTruck> checkReceiptList = idal.IReceiptTruckDAL.SelectBy(u => u.WhCode == whCode && u.ReceiptId == receiptId);
                if (checkReceiptList.Count > 0)
                {
                    return "当前车辆已被放车，请重新查询！";
                }

                List<ReceiptRegister> ReceiptRegisterList = idal.IReceiptRegisterDAL.SelectBy(u => u.WhCode == whCode && u.ReceiptId == receiptId);
                if (ReceiptRegisterList.Count == 0)
                {
                    return "收货批次未找到，请查看交易记录是否误删除操作！";
                }

                ReceiptRegister reg = ReceiptRegisterList.First();
                if ((reg.QueueUpFLag ?? 0) == 0)
                {
                    reg.QueueUpFLag = 1;
                    reg.QueueUpUser = userName;
                    reg.QueueUpRemark = remark;

                    idal.IReceiptRegisterDAL.UpdateBy(reg, u => u.Id == reg.Id, new string[] { "QueueUpFLag", "QueueUpUser", "QueueUpRemark" });
                }
                else
                {
                    return "当前车辆已被插队，请重新查询！";
                }

                idal.SaveChanges();
                return "Y";
            }
        }

        //删除收货登记 排队系统使用
        public string DelReceiptRegisterByTruck(ReceiptRegister entity)
        {
            if (entity == null || entity.ReceiptId == "" || entity.ReceiptId == null)
            {
                return "数据有误，请重新操作！";
            }
            List<ReceiptRegister> regL = idal.IReceiptRegisterDAL.SelectBy(u => u.ReceiptId == entity.ReceiptId && u.WhCode == entity.WhCode);

            if (regL.Count == 0)
            {
                return "系统未查询到该批次！";
            }

            ReceiptRegister reg = regL.First();

            if (reg.Status != "N" && reg.Status != "U")
            {
                return "收货批次状态有误，请重新查询！";
            }

            //删除前 插入车辆表,记录放车人 时间等，再次登记时 就可以重复使用了
            ReceiptTruck truck = new ReceiptTruck();
            truck.WhCode = reg.WhCode;
            truck.ReceiptId = reg.ReceiptId;
            truck.TruckNumber = reg.TruckNumber;
            truck.TruckStatus = reg.TruckStatus;
            truck.ParkingUser = reg.ParkingUser;
            truck.ParkingDate = reg.ParkingDate;
            truck.StorageDate = reg.StorageDate;
            truck.CreateDate = reg.CreateDate;
            truck.CreateUser = reg.CreateUser;
            idal.IReceiptTruckDAL.Add(truck);

            List<ReceiptRegisterDetail> listReg = (from a in idal.IReceiptRegisterDetailDAL.SelectAll()
                                                   where a.ReceiptId == entity.ReceiptId && a.WhCode == entity.WhCode
                                                   select a).ToList();
            string result = "";
            if (listReg.Count > 0)
            {
                foreach (var item in listReg)
                {
                    InBoundOrderDetail inOrderDetail = idal.IInBoundOrderDetailDAL.SelectBy(u => u.Id == item.InOrderDetailId).First();
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
            tl.Description = "仓库删除收货登记";
            tl.TranDate = DateTime.Now;
            tl.TranUser = entity.UpdateUser;
            tl.WhCode = entity.WhCode;
            tl.ReceiptId = entity.ReceiptId;
            tl.Remark = "排队系统功能删除";
            idal.ITranLogDAL.Add(tl);

            idal.IReceiptRegisterDAL.DeleteBy(u => u.ReceiptId == entity.ReceiptId && u.WhCode == entity.WhCode);
            idal.IReceiptRegisterDetailDAL.DeleteBy(u => u.ReceiptId == entity.ReceiptId && u.WhCode == entity.WhCode);
            idal.IReceiptRegisterDAL.SaveChanges();
            return "Y";
        }

        #endregion
    }
}

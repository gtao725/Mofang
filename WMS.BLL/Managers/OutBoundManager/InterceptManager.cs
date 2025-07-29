using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMS.IBLL;
using MODEL_MSSQL;
using WMS.BLLClass;
using System.Transactions;

namespace WMS.BLL
{
    public class InterceptManager : IInterceptManager
    {
        IDAL.IDALSession idal = BLL.BLLHelper.GetDal();
        private static object o = new object();

        //订单拦截
        public string OutBoundOrderIntercept(string whCode, string customerOutPoNumber, string clientCode, string userName)
        {
            lock (o)
            {
                //验证订单是否存在,不存在直接返回
                List<OutBoundOrder> OutBoundOrderList = idal.IOutBoundOrderDAL.SelectBy(u => u.WhCode == whCode && u.CustomerOutPoNumber == customerOutPoNumber && u.ClientCode == clientCode);
                if (OutBoundOrderList.Count == 0)
                {
                    //添加拦截日志
                    TranLog tl = new TranLog();
                    tl.TranType = "-10";
                    tl.Description = "订单拦截";
                    tl.TranDate = DateTime.Now;
                    tl.TranUser = userName;
                    tl.WhCode = whCode;
                    tl.ClientCode = clientCode;
                    tl.CustomerOutPoNumber = customerOutPoNumber;
                    tl.Remark = "订单不存在或有误";
                    idal.ITranLogDAL.Add(tl);
                    idal.SaveChanges();

                    return "订单不存在或有误！";
                }

                OutBoundOrder entity = OutBoundOrderList.First();

                string remark1 = "原状态：" + entity.StatusId + entity.StatusName;

                //添加拦截日志
                TranLog tlog = new TranLog();
                tlog.TranType = "-10";
                tlog.Description = "订单拦截";
                tlog.TranDate = DateTime.Now;
                tlog.TranUser = userName;
                tlog.WhCode = whCode;
                tlog.ClientCode = clientCode;
                tlog.CustomerOutPoNumber = customerOutPoNumber;
                tlog.Remark = "开始执行拦截方法";
                idal.ITranLogDAL.Add(tlog);
                idal.SaveChanges();

                List<FlowHead> FlowHeadList = idal.IFlowHeadDAL.SelectBy(u => u.Id == entity.ProcessId);
                if (FlowHeadList.Count == 0)
                {
                    //添加拦截日志
                    TranLog tl = new TranLog();
                    tl.TranType = "-10";
                    tl.Description = "订单拦截";
                    tl.TranDate = DateTime.Now;
                    tl.TranUser = userName;
                    tl.WhCode = whCode;
                    tl.ClientCode = clientCode;
                    tl.CustomerOutPoNumber = customerOutPoNumber;
                    tl.Remark = "订单未找到相关流程";
                    idal.ITranLogDAL.Add(tl);
                    idal.SaveChanges();

                    return "订单未找到相关流程！";
                }

                FlowHead flowHead = FlowHeadList.First();

                if (flowHead.InterceptFlag == 0 || flowHead.InterceptFlag == null)
                {
                    //添加拦截日志
                    TranLog tl = new TranLog();
                    tl.TranType = "-10";
                    tl.Description = "订单拦截";
                    tl.TranDate = DateTime.Now;
                    tl.TranUser = userName;
                    tl.WhCode = whCode;
                    tl.ClientCode = clientCode;
                    tl.CustomerOutPoNumber = customerOutPoNumber;
                    tl.Remark = "订单所选流程为不可拦截";
                    idal.ITranLogDAL.Add(tl);
                    idal.SaveChanges();

                    return "订单所选流程为不可拦截！";
                }
                //拦截标识1 为 订单拦截
                if (flowHead.InterceptFlag == 1)
                {
                    #region 订单拦截

                    //验证订单
                    if (entity.StatusId != -10 && entity.StatusId < 1)
                    {
                        //添加拦截日志
                        TranLog tl1 = new TranLog();
                        tl1.TranType = "-10";
                        tl1.Description = "订单拦截";
                        tl1.TranDate = DateTime.Now;
                        tl1.TranUser = userName;
                        tl1.WhCode = whCode;
                        tl1.ClientCode = clientCode;
                        tl1.CustomerOutPoNumber = customerOutPoNumber;
                        tl1.Remark = "订单状态未知";
                        idal.ITranLogDAL.Add(tl1);
                        idal.SaveChanges();

                        return "订单状态未知！";
                    }

                    //1.验证订单是否 已拦截
                    if (entity.StatusId == -10)
                    {
                        //添加拦截日志
                        TranLog tl1 = new TranLog();
                        tl1.TranType = "-10";
                        tl1.Description = "订单拦截";
                        tl1.TranDate = DateTime.Now;
                        tl1.TranUser = userName;
                        tl1.WhCode = whCode;
                        tl1.ClientCode = clientCode;
                        tl1.CustomerOutPoNumber = customerOutPoNumber;
                        tl1.Remark = "订单已拦截";
                        idal.ITranLogDAL.Add(tl1);
                        idal.SaveChanges();

                        return "订单已拦截！";
                    }

                    //2.验证订单是否 为 未生成LOAD
                    //5	草稿
                    //10  已确认订单
                    if (entity.StatusId == 5 || entity.StatusId == 10)
                    {
                        UpdateOutBoundOrderStatus(userName, "已拦截已处理", entity);
                    }

                    //3.验证订单是否 为 已生成LOAD
                    //15	已生成Load
                    if (entity.StatusId == 15)
                    {
                        List<LoadDetail> LoadDetailList = idal.ILoadDetailDAL.SelectBy(u => u.OutBoundOrderId == entity.Id);
                        LoadDetail loadDetail = LoadDetailList.First();

                        List<LoadDetail> checkLoadCount = idal.ILoadDetailDAL.SelectBy(u => u.LoadMasterId == loadDetail.LoadMasterId);

                        LoadMaster loadMaster = idal.ILoadMasterDAL.SelectBy(u => u.Id == loadDetail.LoadMasterId).First();
                        if (idal.IPickTaskDetailDAL.SelectBy(u => u.WhCode == whCode && u.LoadId == loadMaster.LoadId).Count() > 0)
                        {
                            UpdateOutBoundOrderStatus(userName, "已拦截待处理", entity);

                            List<SortTaskDetail> SortTaskDetailList = idal.ISortTaskDetailDAL.SelectBy(u => u.WhCode == whCode && u.OutPoNumber == entity.OutPoNumber);
                            foreach (var item in SortTaskDetailList)
                            {
                                SortTaskDetail sortTaskDetail = new SortTaskDetail();
                                sortTaskDetail.HoldQty = item.PlanQty;
                                sortTaskDetail.HoldFlag = 1;
                                sortTaskDetail.UpdateDate = DateTime.Now;
                                sortTaskDetail.UpdateUser = userName;
                                idal.ISortTaskDetailDAL.UpdateBy(sortTaskDetail, u => u.Id == item.Id, new string[] { "HoldQty", "HoldFlag", "UpdateDate", "UpdateUser" });
                            }
                        }
                        else
                        {
                            UpdateOutBoundOrderStatus(userName, "已拦截已处理", entity);

                            if (checkLoadCount.Count == 1)
                            {
                                idal.ILoadMasterDAL.DeleteBy(u => u.Id == loadDetail.LoadMasterId);
                            }
                            idal.ILoadDetailDAL.DeleteBy(u => u.OutBoundOrderId == entity.Id);
                        }
                    }

                    //4.验证订单是否 为 已释放 或 已备货
                    //20	已释放Load
                    //25	已备货
                    if (entity.StatusId == 20 || entity.StatusId == 25)
                    {
                        UpdateOutBoundOrderStatus(userName, "已拦截待处理", entity);

                        List<SortTaskDetail> SortTaskDetailList = idal.ISortTaskDetailDAL.SelectBy(u => u.WhCode == whCode && u.OutPoNumber == entity.OutPoNumber);
                        foreach (var item in SortTaskDetailList)
                        {
                            SortTaskDetail sortTaskDetail = new SortTaskDetail();
                            sortTaskDetail.HoldQty = item.PlanQty;
                            sortTaskDetail.HoldFlag = 1;
                            sortTaskDetail.UpdateDate = DateTime.Now;
                            sortTaskDetail.UpdateUser = userName;
                            idal.ISortTaskDetailDAL.UpdateBy(sortTaskDetail, u => u.Id == item.Id, new string[] { "HoldQty", "HoldFlag", "UpdateDate", "UpdateUser" });
                        }
                    }

                    //5.验证订单是否 为 已分拣
                    //30	已分拣
                    if (entity.StatusId == 30)
                    {
                        UpdateOutBoundOrderStatus(userName, "已拦截待处理", entity);

                        List<PackTask> PackTaskList = idal.IPackTaskDAL.SelectBy(u => u.WhCode == whCode && u.OutPoNumber == entity.OutPoNumber);
                        if (PackTaskList.Count != 0)
                        {
                            PackTask packTask = PackTaskList.First();
                            packTask.Status = -10;
                            packTask.UpdateDate = DateTime.Now;
                            packTask.UpdateUser = userName;
                            idal.IPackTaskDAL.UpdateBy(packTask, u => u.Id == packTask.Id, new string[] { "Status", "UpdateDate", "UpdateUser" });

                            List<PackHead> PackHeadList = idal.IPackHeadDAL.SelectBy(u => u.PackTaskId == packTask.Id);
                            foreach (var item in PackHeadList)
                            {
                                PackHead packHead = new PackHead();
                                packHead.Status = -10;
                                packHead.UpdateDate = DateTime.Now;
                                packHead.UpdateUser = userName;
                                idal.IPackHeadDAL.UpdateBy(packHead, u => u.Id == item.Id, new string[] { "Status", "UpdateDate", "UpdateUser" });
                            }
                        }

                        List<SortTaskDetail> SortTaskDetailList = idal.ISortTaskDetailDAL.SelectBy(u => u.WhCode == whCode && u.OutPoNumber == entity.OutPoNumber);
                        foreach (var item in SortTaskDetailList)
                        {
                            SortTaskDetail sortTaskDetail = new SortTaskDetail();
                            sortTaskDetail.HoldQty = item.PlanQty;
                            sortTaskDetail.HoldFlag = 1;
                            sortTaskDetail.UpdateDate = DateTime.Now;
                            sortTaskDetail.UpdateUser = userName;
                            idal.ISortTaskDetailDAL.UpdateBy(sortTaskDetail, u => u.Id == item.Id, new string[] { "HoldQty", "HoldFlag", "UpdateDate", "UpdateUser" });
                        }

                        List<TransferHead> transfHeadList = (from a in idal.ITransferHeadDAL.SelectAll()
                                                             join b in idal.ITransferTaskDAL.SelectAll()
                                                             on a.TransferTaskId equals b.Id
                                                             where b.Status != 30 && a.WhCode == whCode && a.OutPoNumber == entity.OutPoNumber
                                                             select a).ToList();
                        if (transfHeadList.Count != 0)
                        {
                            TransferHead tran = transfHeadList.First();
                            tran.Status = -10;
                            tran.UpdateDate = DateTime.Now;
                            tran.UpdateUser = userName;
                            idal.ITransferHeadDAL.UpdateBy(tran, u => u.Id == tran.Id, new string[] { "Status", "UpdateDate", "UpdateUser" });
                        }
                    }

                    //6.验证订单是否 为 已包装
                    //35	已包装
                    if (entity.StatusId == 35)
                    {
                        List<TransferHead> checkList = (from a in idal.ITransferHeadDAL.SelectAll()
                                                        join b in idal.ITransferTaskDAL.SelectAll()
                                                        on a.TransferTaskId equals b.Id
                                                        where b.Status == 30 && a.WhCode == whCode && a.OutPoNumber == entity.OutPoNumber
                                                        select a).ToList();
                        if (checkList.Count != 0)
                        {
                            //添加拦截日志
                            TranLog tl2 = new TranLog();
                            tl2.TranType = "-10";
                            tl2.Description = "订单拦截";
                            tl2.TranDate = DateTime.Now;
                            tl2.TranUser = userName;
                            tl2.WhCode = entity.WhCode;
                            tl2.ClientCode = entity.ClientCode;
                            tl2.OutPoNumber = entity.OutPoNumber;
                            tl2.CustomerOutPoNumber = entity.CustomerOutPoNumber;
                            tl2.Remark = "订单已发货，拦截失败";
                            idal.ITranLogDAL.Add(tl2);
                            idal.SaveChanges();

                            return "订单已发货，拦截失败！";
                        }
                        else
                        {
                            UpdateOutBoundOrderStatus(userName, "已拦截待处理", entity);

                            //拦截交接
                            List<TransferHead> TransferHeadList = idal.ITransferHeadDAL.SelectBy(u => u.WhCode == whCode && u.OutPoNumber == entity.OutPoNumber);
                            if (TransferHeadList.Count != 0)
                            {
                                foreach (var item in TransferHeadList)
                                {
                                    TransferHead transferHead = new TransferHead();
                                    transferHead.Status = -10;
                                    transferHead.UpdateDate = DateTime.Now;
                                    transferHead.UpdateUser = userName;
                                    idal.ITransferHeadDAL.UpdateBy(transferHead, u => u.Id == item.Id, new string[] { "Status", "UpdateDate", "UpdateUser" });
                                }
                            }

                            //拦截包装
                            List<PackTask> PackTaskList = idal.IPackTaskDAL.SelectBy(u => u.WhCode == whCode && u.OutPoNumber == entity.OutPoNumber);
                            if (PackTaskList.Count != 0)
                            {
                                PackTask packTask = PackTaskList.First();

                                List<PackHead> PackHeadList = idal.IPackHeadDAL.SelectBy(u => u.PackTaskId == packTask.Id);
                                foreach (var item in PackHeadList)
                                {
                                    PackHead packHead = new PackHead();
                                    packHead.Status = -10;
                                    packHead.UpdateDate = DateTime.Now;
                                    packHead.UpdateUser = userName;
                                    idal.IPackHeadDAL.UpdateBy(packHead, u => u.Id == item.Id, new string[] { "Status", "UpdateDate", "UpdateUser" });
                                }
                            }

                            //拦截分拣
                            List<SortTaskDetail> SortTaskDetailList = idal.ISortTaskDetailDAL.SelectBy(u => u.WhCode == whCode && u.OutPoNumber == entity.OutPoNumber);
                            foreach (var item in SortTaskDetailList)
                            {
                                SortTaskDetail sortTaskDetail = new SortTaskDetail();
                                sortTaskDetail.HoldQty = item.PlanQty;
                                sortTaskDetail.HoldFlag = 1;
                                sortTaskDetail.UpdateDate = DateTime.Now;
                                sortTaskDetail.UpdateUser = userName;
                                idal.ISortTaskDetailDAL.UpdateBy(sortTaskDetail, u => u.Id == item.Id, new string[] { "HoldQty", "HoldFlag", "UpdateDate", "UpdateUser" });
                            }
                        }
                    }

                    //7.验证订单是否 为 已发货
                    // 40  已发货
                    if (entity.StatusId == 40)
                    {
                        //添加拦截日志
                        TranLog tl2 = new TranLog();
                        tl2.TranType = "-10";
                        tl2.Description = "订单拦截";
                        tl2.TranDate = DateTime.Now;
                        tl2.TranUser = userName;
                        tl2.WhCode = entity.WhCode;
                        tl2.ClientCode = entity.ClientCode;
                        tl2.OutPoNumber = entity.OutPoNumber;
                        tl2.CustomerOutPoNumber = entity.CustomerOutPoNumber;
                        tl2.Remark = "订单已发货，拦截失败";
                        idal.ITranLogDAL.Add(tl2);
                        idal.SaveChanges();

                        return "订单已发货，拦截失败！";
                    }

                    //添加拦截日志
                    TranLog tl = new TranLog();
                    tl.TranType = "-10";
                    tl.Description = "订单拦截";
                    tl.TranDate = DateTime.Now;
                    tl.TranUser = userName;
                    tl.WhCode = entity.WhCode;
                    tl.ClientCode = entity.ClientCode;
                    tl.OutPoNumber = entity.OutPoNumber;
                    tl.CustomerOutPoNumber = entity.CustomerOutPoNumber;
                    tl.Remark = "订单拦截成功" + remark1;
                    idal.ITranLogDAL.Add(tl);
                    idal.SaveChanges();

                    return "Y";

                    #endregion
                }
                else if (flowHead.InterceptFlag == 2)  //拦截标识2 为 Load拦截
                {
                    //添加拦截日志
                    TranLog tl1 = new TranLog();
                    tl1.TranType = "-10";
                    tl1.Description = "订单拦截";
                    tl1.TranDate = DateTime.Now;
                    tl1.TranUser = userName;
                    tl1.WhCode = whCode;
                    tl1.ClientCode = clientCode;
                    tl1.CustomerOutPoNumber = customerOutPoNumber;
                    tl1.Remark = "Load拦截暂未完成";
                    idal.ITranLogDAL.Add(tl1);
                    idal.SaveChanges();

                    return "Load拦截暂未完成！";
                }
                else
                {
                    //添加拦截日志
                    TranLog tl1 = new TranLog();
                    tl1.TranType = "-10";
                    tl1.Description = "订单拦截";
                    tl1.TranDate = DateTime.Now;
                    tl1.TranUser = userName;
                    tl1.WhCode = whCode;
                    tl1.ClientCode = clientCode;
                    tl1.CustomerOutPoNumber = customerOutPoNumber;
                    tl1.Remark = "没有相关的拦截动作";
                    idal.ITranLogDAL.Add(tl1);
                    idal.SaveChanges();

                    return "没有相关的拦截动作！";
                }
            }
        }

        //更改订单状态
        public void UpdateOutBoundOrderStatus(string userName, string statusName, OutBoundOrder entity)
        {
            //更新出库订单状态为已分拣
            entity.NowProcessId = 0;
            entity.StatusId = -10;
            entity.StatusName = statusName;
            entity.UpdateUser = userName;
            entity.UpdateDate = DateTime.Now;
            idal.IOutBoundOrderDAL.UpdateBy(entity, u => u.Id == entity.Id, new string[] { "NowProcessId", "StatusId", "StatusName", "UpdateUser", "UpdateDate" });
        }


        //拦截订单查询 
        public List<OutBoundOrderResult1> InterceptOutBoundOrderList(OutBoundOrderSearch1 searchEntity, out int total)
        {
            var sql = from a in idal.IOutBoundOrderDAL.SelectAll()
                      join d in idal.IOutBoundOrderDetailDAL.SelectAll()
                      on a.Id equals d.OutBoundOrderId
                      join b in idal.IFlowHeadDAL.SelectAll()
                      on a.ProcessId equals b.Id
                      join c in idal.ILoadDetailDAL.SelectAll()
                      on a.Id equals c.OutBoundOrderId into temp1
                      from c in temp1.DefaultIfEmpty()
                      join e in idal.ILoadMasterDAL.SelectAll()
                      on c.LoadMasterId equals e.Id into temp2
                      from e in temp2.DefaultIfEmpty()
                      where a.WhCode == searchEntity.WhCode && a.StatusId == -10 && e.Status1 == "C"
                      group new { a, b, d, c } by new
                      {
                          a.Id,
                          a.ClientId,
                          a.ClientCode,
                          a.OutPoNumber,
                          a.CustomerOutPoNumber,
                          a.ReceiptId,
                          a.ProcessId,
                          a.StatusName,
                          a.StatusId,
                          b.FlowName,
                          a.OrderType,
                          a.CreateDate,
                          d.AltItemNumber,
                          c.OutBoundOrderId
                      } into g
                      select new OutBoundOrderResult1
                      {
                          Id = g.Key.Id,
                          ClientId = g.Key.ClientId,
                          ClientCode = g.Key.ClientCode,
                          OutPoNumber = g.Key.OutPoNumber,
                          CustomerOutPoNumber = g.Key.CustomerOutPoNumber,
                          ReceiptId = g.Key.ReceiptId ?? "",
                          ProcessId = g.Key.ProcessId,
                          StatusId = g.Key.StatusId,
                          StatusName = g.Key.StatusName,
                          OrderSource = g.Key.OrderType,
                          CreateDate = g.Key.CreateDate,
                          FlowName = g.Key.FlowName,
                          AltItemNumber = g.Key.AltItemNumber,
                          StatusId1 = (g.Key.OutBoundOrderId ?? 0) == 0 ? "未释放前已处理" : "已释放仓库已备货需检出",
                          StatusId2 = (g.Key.OutBoundOrderId ?? 0) == 0 ? 1 : 2,
                          SumQty = g.Sum(p => p.d.Qty)
                      };

            if (searchEntity.ClientId != 0)
                sql = sql.Where(u => u.ClientId == searchEntity.ClientId);
            if (!string.IsNullOrEmpty(searchEntity.CustomerOutPoNumber))
                sql = sql.Where(u => u.CustomerOutPoNumber == searchEntity.CustomerOutPoNumber);
            if (!string.IsNullOrEmpty(searchEntity.OutPoNumber))
                sql = sql.Where(u => u.OutPoNumber == searchEntity.OutPoNumber);
            if (!string.IsNullOrEmpty(searchEntity.StatusName))
                sql = sql.Where(u => u.StatusName == searchEntity.StatusName);
            if (!string.IsNullOrEmpty(searchEntity.ProcessName))
                sql = sql.Where(u => u.FlowName.Contains(searchEntity.ProcessName));

            if (searchEntity.BeginCreateDate != null)
            {
                sql = sql.Where(u => u.CreateDate >= searchEntity.BeginCreateDate);
            }
            if (searchEntity.EndCreateDate != null)
            {
                sql = sql.Where(u => u.CreateDate <= searchEntity.EndCreateDate);
            }
            if (searchEntity.StatusId2 != 0)
                sql = sql.Where(u => u.StatusId2 == searchEntity.StatusId2);

            List<OutBoundOrderResult1> list = sql.ToList();

            List<OutBoundOrderResult1> list1 = new List<OutBoundOrderResult1>();
            foreach (var item in list)
            {
                if (list1.Where(u => u.Id == item.Id).Count() == 0)
                {
                    OutBoundOrderResult1 newresult = item;
                    list1.Add(newresult);
                }
                else
                {
                    OutBoundOrderResult1 getModel = list1.Where(u => u.Id == item.Id).First();
                    list1.Remove(getModel);

                    OutBoundOrderResult1 newresult = item;
                    newresult.AltItemNumber = newresult.AltItemNumber + "," + getModel.AltItemNumber;
                    newresult.SumQty = newresult.SumQty + getModel.SumQty;
                    list1.Add(newresult);
                }

            }

            if (!string.IsNullOrEmpty(searchEntity.AltItemNumber))
                list1 = list1.Where(u => u.AltItemNumber.Contains(searchEntity.AltItemNumber)).ToList();

            total = list1.Count;
            list1 = list1.OrderBy(u => u.Id).ToList();
            list1 = list1.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize).ToList();
            return list1.ToList();
        }

        //得到客户下的所有收货流程
        public IEnumerable<FlowHead> ClientFlowNameSelect(string whCode, int clientId)
        {
            var sql = from a in idal.IFlowHeadDAL.SelectAll()
                      join b in idal.IR_Client_FlowRuleDAL.SelectAll()
                      on new { Id = a.Id } equals new { Id = (Int32)b.BusinessFlowGroupId } into b_join
                      from b in b_join.DefaultIfEmpty()
                      where b.ClientId == clientId && b.Type == "InBound" && b.WhCode == whCode
                      select a;
            return sql.AsEnumerable();

        }

        //重新生成收货操作单
        public string AddReceiptIdByInterceptOrder(int outBoundOrderId, int processId, string processName, string recLocationId, string userName, string abLocation)
        {
            List<OutBoundOrder> OutBoundOrderList = idal.IOutBoundOrderDAL.SelectBy(u => u.Id == outBoundOrderId);
            List<OutBoundOrderDetail> OutBoundOrderDetailList = idal.IOutBoundOrderDetailDAL.SelectBy(u => u.OutBoundOrderId == outBoundOrderId);
            if (OutBoundOrderList.Count == 0 || OutBoundOrderDetailList.Count == 0)
            {
                return "生成失败，出库订单信息有误！";
            }

            List<LoadMaster> loadMasterList = (from a in idal.IOutBoundOrderDAL.SelectAll()
                                               join b in idal.ILoadDetailDAL.SelectAll()
                                               on a.Id equals b.OutBoundOrderId
                                               join c in idal.ILoadMasterDAL.SelectAll()
                                               on b.LoadMasterId equals c.Id
                                               where a.Id == outBoundOrderId
                                               select c).ToList();

            if (loadMasterList.Count == 0)
            {
                return "订单未找到Load信息，请重新查询！";
            }
            else
            {
                LoadMaster load = loadMasterList.First();
                if (load.Status1 == "U")
                {
                    return "Load还未开始备货，无法生成拦截收货单！";
                }
            }

            #region 根据出库订单 重新构造预录入所需数据
            OutBoundOrder outBoundOrder = OutBoundOrderList.First();

            if ((outBoundOrder.ReceiptId == null ? "" : outBoundOrder.ReceiptId) != "")
            {
                return "订单已生成过收货批次号，请查询！";
            }

            string orderType = "";
            if (OutBoundOrderDetailList.Where(u => (u.SoNumber == null ? "" : u.SoNumber) != "").Count() > 0)
            {
                orderType = "CFS";
            }
            else
            {
                orderType = "DC";
            }

            List<InBoundOrderInsert> InBoundOrderInsertList = new List<InBoundOrderInsert>();

            List<InBoundOrderDetailInsert> orderDetailList = new List<InBoundOrderDetailInsert>();

            foreach (var item in OutBoundOrderDetailList)
            {
                if (InBoundOrderInsertList.Where(u => u.ClientCode == outBoundOrder.ClientCode && u.OrderType == orderType && u.SoNumber == outBoundOrder.CustomerOutPoNumber).Count() == 0)
                {
                    InBoundOrderInsert entity = new InBoundOrderInsert();
                    entity.WhCode = outBoundOrder.WhCode;
                    entity.ClientId = outBoundOrder.ClientId;
                    entity.ClientCode = outBoundOrder.ClientCode;
                    entity.ProcessId = processId;
                    entity.ProcessName = processName;
                    entity.OrderType = orderType;
                    entity.SoNumber = outBoundOrder.CustomerOutPoNumber;

                    InBoundOrderDetailInsert orderDetail = new InBoundOrderDetailInsert();
                    orderDetail.CustomerPoNumber = outBoundOrder.CustomerOutPoNumber;
                    orderDetail.AltItemNumber = item.AltItemNumber;
                    orderDetail.ItemId = item.ItemId;
                    orderDetail.UnitId = (Int32)item.UnitId;
                    orderDetail.UnitName = item.UnitName;

                    orderDetail.Qty = item.Qty;
                    orderDetail.CreateUser = userName;
                    orderDetailList.Add(orderDetail);

                    entity.InBoundOrderDetailInsert = orderDetailList;
                    InBoundOrderInsertList.Add(entity);
                }
                else
                {
                    InBoundOrderInsert entity = InBoundOrderInsertList.Where(u => u.ClientCode == outBoundOrder.ClientCode && u.OrderType == orderType && u.SoNumber == outBoundOrder.CustomerOutPoNumber).First();

                    InBoundOrderInsertList.Remove(entity);

                    InBoundOrderInsert newentity = entity;

                    if (orderDetailList.Where(u => u.CustomerPoNumber == outBoundOrder.CustomerOutPoNumber && u.AltItemNumber == item.AltItemNumber && u.ItemId == item.ItemId).Count() == 0)
                    {
                        InBoundOrderDetailInsert orderDetail = new InBoundOrderDetailInsert();
                        orderDetail.CustomerPoNumber = outBoundOrder.CustomerOutPoNumber;
                        orderDetail.AltItemNumber = item.AltItemNumber;
                        orderDetail.ItemId = item.ItemId;
                        orderDetail.UnitId = (Int32)item.UnitId;
                        orderDetail.UnitName = item.UnitName;

                        orderDetail.Qty = item.Qty;
                        orderDetail.CreateUser = userName;
                        orderDetailList.Add(orderDetail);

                        newentity.InBoundOrderDetailInsert = orderDetailList;
                        InBoundOrderInsertList.Add(newentity);
                    }
                    else
                    {
                        InBoundOrderDetailInsert oldorderDetail = orderDetailList.Where(u => u.CustomerPoNumber == outBoundOrder.CustomerOutPoNumber && u.AltItemNumber == item.AltItemNumber && u.ItemId == item.ItemId).First();

                        InBoundOrderDetailInsert neworderDetail = oldorderDetail;

                        orderDetailList.Remove(oldorderDetail);

                        neworderDetail.Qty = oldorderDetail.Qty + item.Qty;
                        orderDetailList.Add(neworderDetail);

                        newentity.InBoundOrderDetailInsert = orderDetailList;
                        InBoundOrderInsertList.Add(newentity);
                    }
                }
            }
            #endregion

            InBoundOrderManager inBoundOrderManager = new InBoundOrderManager();

            #region 添加预录入
            string result = "";
            foreach (var entity in InBoundOrderInsertList)
            {
                if (result != "")
                {
                    break;
                }
                foreach (var item in entity.InBoundOrderDetailInsert)
                {
                    InBoundOrder inBoundOrder = new InBoundOrder();
                    if (orderType == "CFS")
                    {
                        //添加InBoundSO 
                        InBoundSO inBoundSO = inBoundOrderManager.InsertInBoundSO(entity, item);

                        //添加InBoundOrder  
                        //判断客户PO是否存在
                        List<InBoundOrder> listInBoundOrder = idal.IInBoundOrderDAL.SelectBy(u => u.CustomerPoNumber == item.CustomerPoNumber && u.WhCode == entity.WhCode && u.SoId == inBoundSO.Id);

                        inBoundOrder = inBoundOrderManager.InsertInBoundOrder(entity, item, inBoundSO, listInBoundOrder);
                    }
                    else
                    {
                        //添加InBoundOrder  
                        //判断客户PO是否存在
                        List<InBoundOrder> listInBoundOrder = idal.IInBoundOrderDAL.SelectBy(u => u.CustomerPoNumber == item.CustomerPoNumber && u.WhCode == entity.WhCode && u.ClientId == entity.ClientId);

                        inBoundOrder = inBoundOrderManager.InsertInBoundOrder(entity, item, null, listInBoundOrder);
                    }

                    idal.SaveChanges();

                    ItemMaster itemMaster = idal.IItemMasterDAL.SelectBy(u => u.Id == item.ItemId).First();

                    //添加InBoundOrderDetail
                    int insertResult = inBoundOrderManager.InsertInBoundOrderDetail(entity, item, inBoundOrder, itemMaster);

                    if (insertResult != 1 && insertResult != 2)
                    {
                        result = "保存出错！";
                        break;
                    }
                }
            }

            if (result != "")
            {
                return result;
            }
            #endregion

            idal.IInBoundOrderDetailDAL.SaveChanges();

            #region 添加收货登记及明细
            //添加收货登记
            RegInBoundOrderManager rm = new RegInBoundOrderManager();

            ReceiptRegister rr = new ReceiptRegister();
            rr.ReceiptId = "EI" + DI.IDGenerator.NewId;
            rr.WhCode = outBoundOrder.WhCode;
            rr.ClientCode = outBoundOrder.ClientCode;
            rr.ClientId = outBoundOrder.ClientId;
            rr.RegisterDate = DateTime.Now;
            rr.ReceiptType = "Com";

            rr.LocationId = recLocationId;
            rr.ProcessId = processId;
            rr.ProcessName = processName;
            rr.HoldOutBoundOrderId = outBoundOrder.Id;          //加入拦截订单的ID

            rr.TruckNumber = "";
            rr.CreateDate = DateTime.Now;
            rr.CreateUser = userName;
            rr.Status = "U";
            idal.IReceiptRegisterDAL.Add(rr);
            idal.IReceiptRegisterDAL.SaveChanges();

            List<ReceiptRegisterInsert> rrilist = new List<ReceiptRegisterInsert>();

            //添加收货登记明细
            foreach (var entity in InBoundOrderInsertList)
            {
                foreach (var item in entity.InBoundOrderDetailInsert)
                {
                    InBoundOrder ibod = idal.IInBoundOrderDAL.SelectBy(u => u.WhCode == entity.WhCode && u.ClientId == outBoundOrder.ClientId && u.CustomerPoNumber == item.CustomerPoNumber).First();

                    List<InBoundOrderDetail> iboddetail = idal.IInBoundOrderDetailDAL.SelectBy(u => u.PoId == ibod.Id && u.ItemId == item.ItemId);

                    ReceiptRegisterInsert recReg = new ReceiptRegisterInsert();
                    recReg.WhCode = entity.WhCode;
                    recReg.ReceiptId = rr.ReceiptId;
                    recReg.InBoundOrderDetailId = iboddetail.First().Id;
                    recReg.CustomerPoNumber = item.CustomerPoNumber;
                    recReg.AltItemNumber = item.AltItemNumber;
                    recReg.PoId = ibod.Id;
                    recReg.ItemId = item.ItemId;
                    recReg.UnitName = item.UnitName;
                    recReg.UnitId = item.UnitId;
                    recReg.ProcessName = processName;
                    recReg.ProcessId = processId;
                    recReg.RegQty = item.Qty;
                    recReg.CreateUser = userName;
                    recReg.CreateDate = DateTime.Now;
                    rrilist.Add(recReg);
                }
            }

            rm.AddReceiptRegisterDetail(rrilist);
            #endregion

            //最后修改 出库订单中的收获批次号
            outBoundOrder.ReceiptId = rr.ReceiptId;
            outBoundOrder.StatusName = "已拦截重新生成";
            idal.IOutBoundOrderDAL.UpdateBy(outBoundOrder, u => u.Id == outBoundOrder.Id, new string[] { "ReceiptId", "StatusName" });

            idal.IInBoundOrderDetailDAL.SaveChanges();

            InterceptReceiptInsert(outBoundOrderId, abLocation, userName);
            return "Y";
        }


        //批量生成拦截收货操作单
        public string AddReceiptIdByInterceptOrderList(int[] outBoundOrderIdList, string whCode, string userName, string abLocation)
        {
            lock (o)
            {
                var sql7 = from a in idal.IWhLocationDAL.SelectAll()
                           join b in idal.ILocationTypeDAL.SelectAll() on new { LocationTypeId = a.LocationTypeId } equals new { LocationTypeId = b.Id }
                           where
                             b.TypeName == "AB" &&
                             a.WhCode == whCode && a.LocationId == abLocation
                           select new
                           {
                               a.LocationId
                           };
                if (sql7.Count() == 0)
                {
                    return "未检测到该异常库位，请去基础数据管理-储位管理中调整异常库位！";
                }

                //托盘不存在 优先创建托盘号
                if (idal.IPallateDAL.SelectBy(u => u.WhCode == whCode && u.HuId == abLocation).Count == 0)
                {
                    Pallate pallate = new Pallate();
                    pallate.WhCode = whCode;
                    pallate.HuId = abLocation;
                    pallate.TypeId = 1;
                    pallate.Status = "U";
                    idal.IPallateDAL.Add(pallate);
                }
                if (idal.IPallateDAL.SelectBy(u => u.WhCode == whCode && u.HuId == "L" + abLocation + "AB").Count == 0)
                {
                    Pallate pallate = new Pallate();
                    pallate.WhCode = whCode;
                    pallate.HuId = "L" + abLocation + "AB";
                    pallate.TypeId = 1;
                    pallate.Status = "U";
                    idal.IPallateDAL.Add(pallate);
                }

                List<OutBoundOrder> OutBoundOrderList = idal.IOutBoundOrderDAL.SelectBy(u => outBoundOrderIdList.Contains(u.Id));

                OutBoundOrderList = OutBoundOrderList.Where(u => (u.ReceiptId ?? "") == "").ToList();

                int[] clientIdArr = (from a in OutBoundOrderList
                                     select a.ClientId).Distinct().ToArray();

                List<FlowHeadResult> flowHeadList = (from a in idal.IFlowHeadDAL.SelectAll()
                                                     join b in idal.IR_Client_FlowRuleDAL.SelectAll()
                                                     on new { Id = a.Id } equals new { Id = (Int32)b.BusinessFlowGroupId } into b_join
                                                     from b in b_join.DefaultIfEmpty()
                                                     where clientIdArr.Contains(b.ClientId) && b.Type == "InBound" && b.WhCode == whCode
                                                     select new FlowHeadResult
                                                     {
                                                         Id = a.Id,
                                                         FlowName = a.FlowName,
                                                         ClientId = b.ClientId,
                                                         RId = b.Id
                                                     }).OrderBy(u => u.RId).ToList();

                List<WhZoneResult> whZoneList = (from a in idal.IWhClientDAL.SelectAll()
                                                 join b in idal.IZoneDAL.SelectAll() on new { ZoneId = (Int32)a.ZoneId } equals new { ZoneId = b.Id } into b_join
                                                 from b in b_join.DefaultIfEmpty()
                                                 where a.WhCode == whCode && b.RegFlag == 1 && clientIdArr.Contains(a.Id)
                                                 select new WhZoneResult
                                                 {
                                                     Id = b.Id,
                                                     ZoneName = b.ZoneName,
                                                     ClientId = a.Id
                                                 }).OrderByDescending(u => u.Id).ToList();
                int count = 0;
                string result = "";
                foreach (var item in outBoundOrderIdList)
                {
                    List<OutBoundOrder> getOutBoundOrder = OutBoundOrderList.Where(u => u.Id == item).ToList();
                    if (getOutBoundOrder.Count == 0)
                    {
                        continue;
                    }

                    OutBoundOrder OutBoundOrderFirst = getOutBoundOrder.First();

                    List<FlowHeadResult> getFlowHead = flowHeadList.Where(u => u.ClientId == OutBoundOrderFirst.ClientId).ToList();
                    if (getFlowHead.Count == 0)
                    {
                        continue;
                    }
                    FlowHeadResult flowHeadFirst = getFlowHead.First();

                    List<WhZoneResult> getWhZone = whZoneList.Where(u => u.ClientId == OutBoundOrderFirst.ClientId).ToList();
                    if (getWhZone.Count == 0)
                    {
                        continue;
                    }
                    WhZoneResult WhZoneResultFirst = getWhZone.First();

                    result = AddReceiptIdByInterceptOrder(item, (Int32)flowHeadFirst.Id, flowHeadFirst.FlowName, WhZoneResultFirst.ZoneName, userName, abLocation);
                    if (result != "Y")
                    {
                        break;
                    }
                    else
                    {
                        count++;
                    }
                }

                if (result != "Y")
                {
                    return "成功生成部分：" + count + "个批次并自动收货！其余订单因以下原因停止生成：" + result;
                }

                return "Y成功生成" + count + "个批次并自动收货！";
            }
        }

        //订单处理完成 确认
        public string CheckInterceptOrder(int outBoundOrderId, string userName)
        {
            return "该功能系统已自动优化！";

            List<OutBoundOrder> OutBoundOrderList = idal.IOutBoundOrderDAL.SelectBy(u => u.Id == outBoundOrderId);
            if (OutBoundOrderList.Count == 0)
            {
                return "未找到出库订单信息！";
            }

            List<OutBoundOrderDetail> OutBoundOrderDetailList = idal.IOutBoundOrderDetailDAL.SelectBy(u => u.OutBoundOrderId == outBoundOrderId);
            if (OutBoundOrderDetailList.Count == 0)
            {
                return "未找到出库订单明细信息！";
            }

            OutBoundOrder outBoundOrder = OutBoundOrderList.First();

            List<ReceiptRegister> ReceiptRegisterList = idal.IReceiptRegisterDAL.SelectBy(u => u.WhCode == outBoundOrder.WhCode && u.ReceiptId == outBoundOrder.ReceiptId);
            if (ReceiptRegisterList.Count == 0)
            {
                return "确认失败，收货登记信息有误！";
            }

            ReceiptRegister receiptRegister = ReceiptRegisterList.First();
            if (receiptRegister.Status != "C")
            {
                return "确认失败，重收批次号还未完成收货！";
            }

            //查询重新收货信息
            List<Receipt> ReceiptList = idal.IReceiptDAL.SelectBy(u => u.WhCode == outBoundOrder.WhCode && u.ReceiptId == outBoundOrder.ReceiptId);
            if (ReceiptList.Count == 0)
            {
                return "确认失败，重收数量为0！";
            }

            List<InterceptOrderRecResult> recResultList = (from receipt in ReceiptList
                                                           group receipt by new
                                                           {
                                                               receipt.AltItemNumber,
                                                               receipt.ItemId,
                                                               receipt.UnitId,
                                                               receipt.UnitName
                                                           } into g
                                                           select new InterceptOrderRecResult
                                                           {
                                                               AltItemNumber = g.Key.AltItemNumber,
                                                               ItemId = (Int32)g.Key.ItemId,
                                                               UnitId = (Int32)g.Key.UnitId,
                                                               UnitName = g.Key.UnitName,
                                                               Qty = (Int32)g.Sum(p => p.Qty)
                                                           }).ToList();

            string result = "";
            foreach (var item in OutBoundOrderDetailList)
            {
                if (recResultList.Where(u => u.AltItemNumber == item.AltItemNumber && u.ItemId == item.ItemId && u.UnitId == item.UnitId && u.UnitName == item.UnitName).Count() != 0)
                {
                    if (recResultList.Where(u => u.AltItemNumber == item.AltItemNumber && u.ItemId == item.ItemId && u.UnitId == item.UnitId && u.UnitName == item.UnitName).Count() > 1)
                    {
                        result = "确认失败，款号" + item.AltItemNumber + "重收数据有误！";
                        break;
                    }
                    else
                    {
                        InterceptOrderRecResult recResult = recResultList.Where(u => u.AltItemNumber == item.AltItemNumber && u.ItemId == item.ItemId && u.UnitId == item.UnitId && u.UnitName == item.UnitName).First();
                        if (recResult.Qty != item.Qty)
                        {
                            result = "确认失败，款号" + item.AltItemNumber + "重收数量与订单不一致！";
                            break;
                        }
                    }
                }
                else
                {
                    result = "确认失败，款号" + item.AltItemNumber + "未找到重收数量！";
                    break;
                }
            }
            if (result != "")
            {
                return result;
            }


            //最后修改 出库订单的状态
            outBoundOrder.StatusName = "已拦截已处理";
            outBoundOrder.UpdateUser = userName;
            outBoundOrder.UpdateDate = DateTime.Now;
            idal.IOutBoundOrderDAL.UpdateBy(outBoundOrder, u => u.Id == outBoundOrder.Id, new string[] { "StatusName", "UpdateUser", "UpdateDate" });

            idal.IInBoundOrderDetailDAL.SaveChanges();

            return "Y";
        }


        //通过系统出库订单号查找收货批次号
        public string GetInterceptOrderReceiptId(string OutPoNumber, string whCode)
        {
            string result = "";
            List<OutBoundOrder> list = idal.IOutBoundOrderDAL.SelectBy(u => u.WhCode == whCode && u.OutPoNumber == OutPoNumber);
            if (list.Count == 0)
            {
                return "未找到出货订单信息！";
            }
            else
            {
                OutBoundOrder first = list.First();
                if (string.IsNullOrEmpty(first.ReceiptId))
                {
                    result = "该拦截订单未生成收货单，请先生成！";
                }
                else
                {
                    result = "Y$" + first.ReceiptId;
                }
            }

            return result;
        }


        //拦截订单一键完成收货至异常库位
        public string InterceptReceiptInsert(int outBoundOrderId, string abLocation, string userName)
        {
            OutBoundOrder getOutBoundFirst = idal.IOutBoundOrderDAL.SelectBy(u => u.Id == outBoundOrderId).First();
            if ((getOutBoundFirst.ReceiptId ?? "") == "")
            {
                return "未生成收货操作单无法收货！";
            }

            int getSumRegQty = idal.IReceiptRegisterDetailDAL.SelectBy(u => u.ReceiptId == getOutBoundFirst.ReceiptId && u.WhCode == getOutBoundFirst.WhCode).Sum(u => u.RegQty);
            int getSumOutQty = idal.IOutBoundOrderDetailDAL.SelectBy(u => u.OutBoundOrderId == getOutBoundFirst.Id).Sum(u => u.Qty);

            if (getSumRegQty != getSumOutQty)
            {
                return "登记数量与拦截订单数量不符！";
            }

            try
            {
                #region 拦截订单一键收货优化

                ReceiptInsert entity = new ReceiptInsert();
                entity.ReceiptId = getOutBoundFirst.ReceiptId;
                entity.WhCode = getOutBoundFirst.WhCode;
                entity.CreateUser = userName;

                var sql = from a in idal.IReceiptRegisterDAL.SelectAll()
                          join b in idal.IReceiptRegisterDetailDAL.SelectAll()
                                on new { a.WhCode, a.ReceiptId }
                            equals new { b.WhCode, b.ReceiptId }
                          join c in idal.IInBoundOrderDetailDAL.SelectAll() on new { InOrderDetailId = b.InOrderDetailId } equals new { InOrderDetailId = c.Id }
                          join d in idal.IInBoundOrderDAL.SelectAll() on new { PoId = c.PoId } equals new { PoId = d.Id }
                          join e in idal.IInBoundSODAL.SelectAll() on new { SoId = (Int32)d.SoId } equals new { SoId = e.Id } into e_join
                          from e in e_join.DefaultIfEmpty()
                          join f in idal.IItemMasterDAL.SelectAll() on new { ItemId = c.ItemId } equals new { ItemId = f.Id }
                          where
                            a.ReceiptId == entity.ReceiptId &&
                            a.WhCode == entity.WhCode
                          group new { a, e, d, f, b } by new
                          {
                              a.ClientId,
                              a.ClientCode,
                              a.ReceiptId,
                              a.ProcessId,
                              e.SoNumber,
                              d.CustomerPoNumber,
                              f.AltItemNumber,
                              b.ItemId,
                              b.RegQty,
                              b.UnitName
                          } into g
                          select new
                          {
                              ClientId = g.Key.ClientId,
                              ClientCode = g.Key.ClientCode,
                              ReceiptId = g.Key.ReceiptId,
                              ProcessId = (Int32?)g.Key.ProcessId,
                              SoNumber = g.Key.SoNumber ?? "",
                              CustomerPoNumber = g.Key.CustomerPoNumber ?? "",
                              AltItemNumber = g.Key.AltItemNumber,
                              ItemId = g.Key.ItemId,
                              RegQty = g.Key.RegQty,
                              UnitName = g.Key.UnitName
                          };

                int count = 0;

                entity.WorkloadAccountModel = null;
                entity.HoldMasterModel = null;

                var sql2 = from a in idal.IReceiptRegisterDAL.SelectAll()
                           join b in idal.IZoneDAL.SelectAll()
                                 on new { a.WhCode, a.LocationId }
                             equals new { b.WhCode, LocationId = b.ZoneName }
                           join c in idal.IWhLocationDAL.SelectAll() on new { Id = b.Id } equals new { Id = (Int32)c.ZoneId }
                           where
                             c.LocationTypeId == 2 &&
                             a.WhCode == entity.WhCode &&
                             a.ReceiptId == entity.ReceiptId
                           select new LocationResult
                           {
                               Location = c.LocationId
                           };
                if (sql2.Count() == 0)
                {
                    return "未找到收货批次对应的收货门区！";
                }

                LocationResult firstLocation = sql2.ToList().First();
                string location = firstLocation.Location;

                //得到出货订单的LotNumber 与数量
                List<OutBoundOrderDetail> outBoundDetailList = idal.IOutBoundOrderDetailDAL.SelectBy(u => u.OutBoundOrderId == outBoundOrderId);
                List<OutBoundOrderDetail> checkAddDetailList = new List<OutBoundOrderDetail>();

                List<RecModeldetail> RecModeldetail = new List<RecModeldetail>();
                foreach (var item in sql)
                {
                    if (count == 0)
                    {
                        entity.ClientId = item.ClientId;
                        entity.ClientCode = item.ClientCode;
                        entity.ReceiptDate = DateTime.Now;
                        entity.SoNumber = item.SoNumber;
                        entity.CustomerPoNumber = item.CustomerPoNumber;
                        entity.ProcessId = item.ProcessId;
                        entity.HuId = "L" + abLocation + "AB";
                        entity.Location = location;
                    }

                    int qtyResult = item.RegQty;

                    //循环出库订单明细 填充收货实体
                    foreach (var outDetail in outBoundDetailList.Where(u => u.ItemId == item.ItemId))
                    {
                        if (qtyResult <= 0)
                        {
                            break;
                        }
                        else
                        {
                            int? sqlDetailQty = outDetail.Qty;
                            if (checkAddDetailList.Where(u => u.Id == outDetail.Id).Count() > 0)
                            {
                                int getQty = checkAddDetailList.Where(u => u.Id == outDetail.Id).Sum(u => u.Qty);
                                sqlDetailQty = outDetail.Qty - getQty;
                            }
                            if (sqlDetailQty <= 0)
                            {
                                continue;
                            }

                            RecModeldetail recModelDetail = new RecModeldetail();
                            recModelDetail.AltItemNumber = item.AltItemNumber;
                            recModelDetail.ItemId = item.ItemId;
                            recModelDetail.UnitId = 0;
                            recModelDetail.UnitName = item.UnitName;

                            if (qtyResult >= sqlDetailQty)
                            {
                                recModelDetail.Qty = (Int32)sqlDetailQty;
                                qtyResult = qtyResult - recModelDetail.Qty;
                            }
                            else
                            {
                                recModelDetail.Qty = qtyResult;
                                qtyResult = 0;
                            }

                            if (recModelDetail.Qty <= 0)
                            {
                                continue;
                            }

                            recModelDetail.Length = 0;
                            recModelDetail.Width = 0;
                            recModelDetail.Height = 0;
                            recModelDetail.Weight = 0;
                            recModelDetail.LotNumber1 = outDetail.LotNumber1 ?? "";
                            recModelDetail.LotNumber2 = outDetail.LotNumber2 ?? "";

                            //24年调整：拦截订单的Lotdate时间为2023-01-01，退货的Lotdate时间为2022-01-01
                            recModelDetail.LotDate = Convert.ToDateTime("2023-01-01 00:00:00");

                            recModelDetail.SerialNumberInModel = null;
                            RecModeldetail.Add(recModelDetail);

                            checkAddDetailList.Add(outDetail);
                        }

                        count++;
                    }
                }

                List<RecModeldetail> RecModeldetail1 = new List<RecModeldetail>();
                foreach (var item in RecModeldetail)
                {
                    if (RecModeldetail1.Where(u => u.ItemId == item.ItemId && u.UnitId == item.UnitId && u.Length == item.Length && u.Width == item.Width && u.Height == item.Height && u.LotNumber1 == item.LotNumber1 && u.LotNumber2 == item.LotNumber2).Count() == 0)
                    {
                        RecModeldetail1.Add(item);
                    }
                    else
                    {
                        RecModeldetail oldEntity = RecModeldetail1.Where(u => u.ItemId == item.ItemId && u.UnitId == item.UnitId && u.Length == item.Length && u.Width == item.Width && u.Height == item.Height && u.LotNumber1 == item.LotNumber1 && u.LotNumber2 == item.LotNumber2).First();
                        RecModeldetail1.Remove(oldEntity);

                        RecModeldetail newEntity = oldEntity;
                        newEntity.Qty = oldEntity.Qty + item.Qty;
                        RecModeldetail1.Add(newEntity);
                    }
                }

                entity.RecModeldetail = RecModeldetail1;

                //收货方法
                ReceiptInsertByOther(entity);

                //上架方法
                InterceptNoHuIdByHuDetail(entity.WhCode, entity.Location, abLocation, entity.HuId, entity.CreateUser);

                RecManager recManager = new RecManager();
                recManager.RecComplete(entity.ReceiptId, entity.WhCode, entity.CreateUser);

                #endregion

                return "Y";
            }
            catch (Exception e)
            {
                string ss = e.InnerException.Message;
                return "拦截订单一键收货异常，请重新提交！";
            }

        }

        //拦截或退货收货方法
        public string ReceiptInsertByOther(ReceiptInsert entity)
        {
            if (entity.RecModeldetail == null)
            {
                return "错误！没有货物明细！";
            }

            RecHelper recHelper = new RecHelper();

            //1.首先验证全部数据是否满足
            if (!recHelper.CheckReceiptId(entity.ReceiptId, entity.WhCode))
            {
                return "错误！收货批次号有误或不存在！";
            }

            if (!recHelper.CheckSoPo(entity.ReceiptId, entity.WhCode, entity.SoNumber, entity.CustomerPoNumber))
            {
                return "错误！SO或PO输入有误！";
            }

            bool recDetailResult = true;

            List<RecModeldetail> listRecDetail = new List<RecModeldetail>();
            foreach (var item in entity.RecModeldetail)
            {
                if (recDetailResult)
                {
                    if ((from a in listRecDetail where a.ItemId == item.ItemId && a.UnitId == item.UnitId && a.Length == item.Length && a.Width == item.Width && a.Height == item.Height && a.LotNumber1 == item.LotNumber1 && a.LotNumber2 == item.LotNumber2 select a).Count() == 0)
                    {
                        RecModeldetail recDetail = new RecModeldetail();
                        recDetail.ItemId = item.ItemId;
                        recDetail.UnitId = item.UnitId;
                        recDetail.Length = item.Length;
                        recDetail.Width = item.Width;
                        recDetail.Height = item.Height;
                        recDetail.LotNumber1 = item.LotNumber1;
                        recDetail.LotNumber2 = item.LotNumber2;
                        recDetail.LotDate = item.LotDate;
                        recDetail.Attribute1 = item.Attribute1;
                        listRecDetail.Add(recDetail);
                    }
                    else
                    {
                        recDetailResult = false;
                    }
                }
            }
            if (recDetailResult == false)
            {
                return "错误！货物明细重复或异常！";
            }

            string result = "";
            List<int> ItemList = new List<int>();

            List<string> checksearNumberList = new List<string>();
            foreach (var item in entity.RecModeldetail)
            {
                if (!ItemList.Contains(item.ItemId))
                {
                    ItemList.Add(item.ItemId);
                }

                string s = CheckSerialNumberList(entity, item, checksearNumberList);
                if (s != "Y")
                {
                    result = s;
                    break;
                }
            }

            if (result != "")
            {
                return result;
            }


            if (!recHelper.CheckSku(entity.ReceiptId, entity.WhCode, ItemList, entity.CustomerPoNumber))
            {
                return "错误！款号有误或不存在！";
            }
            if (!recHelper.CheckSku(entity.WhCode, ItemList))
            {
                return "错误！款号有误或不存在！";
            }

            //验证该收货 是否选择 收货单位(可变)流程    
            List<BusinessFlowHead> checkRFRule = (from c in idal.IReceiptRegisterDAL.SelectAll()
                                                  join a in idal.IFlowDetailDAL.SelectAll() on new { ProcessId = (Int32)c.ProcessId } equals new { ProcessId = a.FlowHeadId } into a_join
                                                  from a in a_join.DefaultIfEmpty()
                                                  join b in idal.IBusinessFlowHeadDAL.SelectAll() on new { BusinessObjectGroupId = (Int32)a.BusinessObjectGroupId } equals new { BusinessObjectGroupId = b.GroupId } into b_join
                                                  from b in b_join.DefaultIfEmpty()
                                                  where c.ReceiptId == entity.ReceiptId && c.WhCode == entity.WhCode
                                                  select b).ToList();

            //1.如果选择了正常的收货单位 则需要验证款号对应的单位
            //绑定了后台数据的主键ID， 请勿随意更改
            if (checkRFRule.Where(u => u.FlowRuleId == 16).Count() > 0)
            {
                if (!recHelper.CheckUnit(entity.WhCode, ItemList, entity))
                {
                    return "错误！款号对应的单位有误！";
                }
            }

            if (recHelper.CheckSkuId(entity.WhCode, entity.RecModeldetail, entity.ClientId) == false)
            {
                return "错误！款号所扫描的ID有误！";
            }

            using (TransactionScope trans = new TransactionScope())
            {
                try
                {

                    int SoId = 0;
                    if (!string.IsNullOrEmpty(entity.SoNumber))
                    {
                        List<InBoundSO> soList = idal.IInBoundSODAL.SelectBy(u => u.SoNumber == entity.SoNumber && u.WhCode == entity.WhCode && u.ClientId == entity.ClientId);
                        SoId = soList.First().Id;
                    }

                    int PoId = 0;
                    if (!string.IsNullOrEmpty(entity.SoNumber))
                    {
                        List<InBoundOrder> orderList = idal.IInBoundOrderDAL.SelectBy(u => u.SoId == SoId && u.WhCode == entity.WhCode && u.CustomerPoNumber == entity.CustomerPoNumber && u.ClientId == entity.ClientId);
                        PoId = orderList.First().Id;
                    }
                    else
                    {
                        List<InBoundOrder> orderList = idal.IInBoundOrderDAL.SelectBy(u => u.WhCode == entity.WhCode && u.CustomerPoNumber == entity.CustomerPoNumber && u.ClientId == entity.ClientId);
                        PoId = orderList.First().Id;
                    }

                    //2.验证实收数量 预收数量与登记数量是否有差异

                    //1.如果选择了正常的收货单位 则需要验证款号对应的单位
                    //绑定了后台数据的主键ID， 请勿随意更改
                    if (checkRFRule.Where(u => u.FlowRuleId == 16).Count() > 0)
                    {
                        string checkReceiptQtyResult = CheckReceiptQty(entity, PoId);
                        if (checkReceiptQtyResult != "")
                        {
                            return checkReceiptQtyResult;
                        }
                    }
                    else
                    {
                        //款号可变形的验证
                        string checkReceiptQtyResult = CheckReceiptQtyByItemBX(entity, PoId);
                        if (checkReceiptQtyResult != "")
                        {
                            return checkReceiptQtyResult;
                        }
                    }

                    //3.开始插入数据
                    List<ReceiptRegister> regList = idal.IReceiptRegisterDAL.SelectBy(u => u.ReceiptId == entity.ReceiptId && u.WhCode == entity.WhCode);
                    ReceiptRegister reg = regList.First();

                    Receipt rec = null;
                    decimal? qty = 0;    //总数量
                    decimal? cbm = 0;    //总体积
                    decimal? weight = 0; //总重量


                    List<InBoundOrderDetail> inboundOrderDetailList = idal.IInBoundOrderDetailDAL.SelectBy(u => u.WhCode == entity.WhCode && u.PoId == PoId);
                    List<ReceiptRegisterDetail> receiptRegDetailList = idal.IReceiptRegisterDetailDAL.SelectBy(u => u.ReceiptId == entity.ReceiptId && u.WhCode == entity.WhCode && u.PoId == PoId);

                    foreach (var item in entity.RecModeldetail)
                    {
                        //插入实收表
                        rec = new Receipt();
                        rec.WhCode = entity.WhCode;
                        rec.RegId = reg.Id;
                        rec.ReceiptId = entity.ReceiptId;
                        rec.ClientId = entity.ClientId;
                        rec.ClientCode = entity.ClientCode;
                        rec.ReceiptDate = DateTime.Now;
                        if (entity.HoldMasterModel != null)
                        {
                            rec.Status = "H";
                            rec.HoldReason = entity.HoldMasterModel.HoldReason;
                        }
                        else
                        {
                            rec.Status = "A";
                        }
                        rec.SoNumber = entity.SoNumber;
                        rec.PoId = PoId;
                        rec.CustomerPoNumber = entity.CustomerPoNumber;
                        rec.HuId = entity.HuId;
                        rec.LotFlag = entity.LotFlag;
                        rec.CreateUser = entity.CreateUser;
                        rec.CreateDate = DateTime.Now;

                        rec.AltItemNumber = item.AltItemNumber;
                        rec.ItemId = item.ItemId;
                        rec.UnitId = item.UnitId;
                        rec.UnitName = item.UnitName;
                        rec.Qty = item.Qty;
                        rec.Length = item.Length / 100;
                        rec.Width = item.Width / 100;
                        rec.Height = item.Height / 100;
                        rec.Weight = item.Weight;
                        rec.LotNumber1 = item.LotNumber1;
                        rec.LotNumber2 = item.LotNumber2;
                        rec.LotDate = item.LotDate;
                        rec.Attribute1 = item.Attribute1;

                        rec.Custom1 = item.Custom1;
                        rec.Custom2 = item.Custom2;
                        rec.Custom3 = item.Custom3;

                        idal.IReceiptDAL.Add(rec);

                        qty += item.Qty;
                        cbm += (item.UnitName.Contains("ECH") ? 0 : item.Qty) * (item.Length / 100) * (item.Height / 100) * (item.Width / 100);
                        weight += item.Weight;

                        ItemMaster getItemMaster = idal.IItemMasterDAL.SelectBy(u => u.Id == item.ItemId).First();
                        //2.如果选择了正常的收货单位  
                        //验证单位 且修改单位
                        if (checkRFRule.Where(u => u.FlowRuleId == 16).Count() > 0)
                        {
                            if (getItemMaster.UnitFlag == 0 && getItemMaster.UnitName == "none")
                            {
                                ItemMaster item1 = new ItemMaster();
                                item1.UnitName = item.UnitName;
                                item1.UpdateUser = entity.CreateUser;
                                item1.UpdateDate = DateTime.Now;
                                idal.IItemMasterDAL.UpdateBy(item1, u => u.Id == item.ItemId, new string[] { "UnitName", "UpdateUser", "UpdateDate" });
                            }

                            InBoundOrderDetail inOrderDetail = inboundOrderDetailList.Where(u => u.ItemId == item.ItemId).First();

                            if (inOrderDetail.UnitName == "none")
                            {
                                inOrderDetail.UnitId = item.UnitId;
                                inOrderDetail.UnitName = item.UnitName;
                                inOrderDetail.UpdateUser = entity.CreateUser;
                                inOrderDetail.UpdateDate = DateTime.Now;
                                idal.IInBoundOrderDetailDAL.UpdateBy(inOrderDetail, u => u.Id == inOrderDetail.Id, new string[] { "UnitId", "UnitName", "UpdateUser", "UpdateDate" });
                            }

                            ReceiptRegisterDetail receiptRegDetail = receiptRegDetailList.Where(u => u.ItemId == item.ItemId).First();

                            if (receiptRegDetail.UnitName == "none")
                            {
                                receiptRegDetail.UnitId = item.UnitId;
                                receiptRegDetail.UnitName = item.UnitName;
                                receiptRegDetail.UpdateUser = entity.CreateUser;
                                receiptRegDetail.UpdateDate = DateTime.Now;
                                idal.IReceiptRegisterDetailDAL.UpdateBy(receiptRegDetail, u => u.Id == receiptRegDetail.Id, new string[] { "UnitId", "UnitName", "UpdateUser", "UpdateDate" });
                            }
                        }
                        else
                        {
                            ItemMaster item1 = idal.IItemMasterDAL.SelectBy(u => u.Id == item.ItemId).First();

                            item1.UnitName = item.UnitName;
                            item1.UpdateUser = entity.CreateUser;
                            item1.UpdateDate = DateTime.Now;
                            idal.IItemMasterDAL.UpdateBy(item1, u => u.Id == item.ItemId, new string[] { "UnitName", "UpdateUser", "UpdateDate" });

                            InBoundOrderDetail inOrderDetail = inboundOrderDetailList.Where(u => u.ItemId == item.ItemId).First();

                            inOrderDetail.UnitId = item.UnitId;
                            inOrderDetail.UnitName = item.UnitName;
                            inOrderDetail.UpdateUser = entity.CreateUser;
                            inOrderDetail.UpdateDate = DateTime.Now;
                            idal.IInBoundOrderDetailDAL.UpdateBy(inOrderDetail, u => u.Id == inOrderDetail.Id, new string[] { "UnitId", "UnitName", "UpdateUser", "UpdateDate" });

                            ReceiptRegisterDetail receiptRegDetail = receiptRegDetailList.Where(u => u.ItemId == item.ItemId).First();

                            receiptRegDetail.UnitId = item.UnitId;
                            receiptRegDetail.UnitName = item.UnitName;
                            receiptRegDetail.UpdateUser = entity.CreateUser;
                            receiptRegDetail.UpdateDate = DateTime.Now;
                            idal.IReceiptRegisterDetailDAL.UpdateBy(receiptRegDetail, u => u.Id == receiptRegDetail.Id, new string[] { "UnitId", "UnitName", "UpdateUser", "UpdateDate" });
                        }

                        if (item.SerialNumberInModel != null)
                        {
                            SerialNumberInsert(entity, PoId, item);     //添加采集箱号
                        }
                    }

                    //更改收货批次号状态
                    ReceiptRegister receiptRegister = idal.IReceiptRegisterDAL.SelectBy(u => u.ReceiptId == entity.ReceiptId && u.WhCode == entity.WhCode).First();
                    if (receiptRegister.Status == "U" || receiptRegister.Status == "P")
                    {
                        if (receiptRegister.BeginReceiptDate == null)
                        {
                            ReceiptRegister receiptRegister1 = new ReceiptRegister();
                            receiptRegister1.BeginReceiptDate = DateTime.Now;
                            idal.IReceiptRegisterDAL.UpdateBy(receiptRegister1, u => u.ReceiptId == entity.ReceiptId && u.WhCode == entity.WhCode, new string[] { "BeginReceiptDate" });
                        }
                        receiptRegister.Status = "A";

                        if (receiptRegister.TransportType == null || receiptRegister.TransportType == "")
                        {
                            receiptRegister.TransportType = entity.TransportType;
                            receiptRegister.TransportTypeExtend = entity.TransportTypeExtend;
                            receiptRegister.UpdateUser = entity.CreateUser;
                            receiptRegister.UpdateDate = DateTime.Now;
                            idal.IReceiptRegisterDAL.UpdateBy(receiptRegister, u => u.ReceiptId == entity.ReceiptId && u.WhCode == entity.WhCode, new string[] { "Status", "TransportType", "UpdateUser", "UpdateDate", "TransportTypeExtend" });
                        }
                        else
                        {
                            receiptRegister.UpdateUser = entity.CreateUser;
                            receiptRegister.UpdateDate = DateTime.Now;
                            idal.IReceiptRegisterDAL.UpdateBy(receiptRegister, u => u.ReceiptId == entity.ReceiptId && u.WhCode == entity.WhCode, new string[] { "Status", "UpdateUser", "UpdateDate" });
                        }

                        //正在收货时 验证 是否是拦截订单
                        if (receiptRegister.HoldOutBoundOrderId != 0)
                        {
                            List<OutBoundOrder> OutBoundOrderList = idal.IOutBoundOrderDAL.SelectBy(u => u.Id == receiptRegister.HoldOutBoundOrderId);
                            if (OutBoundOrderList.Count != 0)
                            {
                                OutBoundOrder outBoundOrder = OutBoundOrderList.First();

                                outBoundOrder.StatusName = "已拦截正在收货";
                                idal.IOutBoundOrderDAL.UpdateBy(outBoundOrder, u => u.Id == outBoundOrder.Id, new string[] { "StatusName" });
                            }
                        }

                    }

                    HuMasterInsert(entity);    //添加库存

                    AddReceiptTranLog(entity);//添加收货TranLog

                    idal.IReceiptDAL.SaveChanges();

                    //收货时 插入TCR记录
                    #region 
                    //1.验证收货批次 是否有货损托盘
                    List<Receipt> checkReceiptTCRList = idal.IReceiptDAL.SelectBy(u => u.ReceiptId == entity.ReceiptId && u.WhCode == entity.WhCode && u.Status == "H" && u.HuId == entity.HuId);
                    if (checkReceiptTCRList.Count > 0)
                    {
                        //如果有TCR 批量插入
                        List<PhotoMaster> photoList = new List<PhotoMaster>();
                        foreach (var item in checkReceiptTCRList)
                        {
                            PhotoMaster photoMaster = new PhotoMaster();

                            photoMaster.PhotoId = 0;

                            photoMaster.WhCode = item.WhCode;
                            photoMaster.ClientCode = item.ClientCode;
                            photoMaster.Number = item.ReceiptId;
                            photoMaster.Number2 = item.SoNumber;
                            photoMaster.Number3 = item.CustomerPoNumber;
                            photoMaster.Number4 = item.AltItemNumber;

                            photoMaster.PoId = item.PoId;
                            photoMaster.ItemId = item.ItemId;
                            photoMaster.UnitName = item.UnitName;
                            photoMaster.Qty = item.Qty;
                            photoMaster.RegQty = 0;
                            photoMaster.HuId = item.HuId;
                            photoMaster.HoldReason = item.HoldReason;
                            photoMaster.TCRStatus = "未处理";
                            photoMaster.TCRProcessMode = "";

                            photoMaster.SettlementMode = "";
                            photoMaster.SumPrice = 0;
                            photoMaster.DeliveryDate = item.ReceiptDate;
                            photoMaster.Type = "in";

                            photoMaster.Status = 0;

                            photoMaster.CheckStatus1 = "N";
                            photoMaster.CheckStatus2 = "N";
                            photoMaster.CreateUser = item.CreateUser;
                            photoMaster.CreateDate = DateTime.Now;
                            photoList.Add(photoMaster);
                        }

                        idal.IPhotoMasterDAL.Add(photoList);
                    }
                    #endregion
                    //TCR结束

                    idal.IReceiptDAL.SaveChanges();
                    trans.Complete();
                    return "Y";
                }
                catch
                {
                    trans.Dispose();//出现异常，事务手动释放
                    return "操作异常请重试！";
                }
            }
        }


        //Location为当前库位,DestLoc为上架库位
        public string InterceptNoHuIdByHuDetail(string WhCode, string Location, string DestLoc, string HuId, string User)
        {
            try
            {
                //验证HuMaster 
                HuMaster huMaster = new HuMaster();
                List<HuMaster> GethuMasterList = idal.IHuMasterDAL.SelectBy(u => u.HuId == DestLoc && u.WhCode == WhCode);
                if (GethuMasterList.Count == 0)
                {
                    //如果库位托盘不存在，直接修改原托盘为库位托盘
                    huMaster = idal.IHuMasterDAL.SelectBy(u => u.HuId == HuId && u.WhCode == WhCode).First();
                    huMaster.HuId = DestLoc;
                    huMaster.Location = DestLoc;
                    huMaster.UpdateUser = User;
                    huMaster.UpdateDate = DateTime.Now;
                    idal.IHuMasterDAL.UpdateBy(huMaster, u => u.Id == huMaster.Id, new string[] { "HuId", "Location", "UpdateUser", "UpdateDate" });
                }
                else
                {
                    //如果库位托盘存在，删除原托盘
                    idal.IHuMasterDAL.DeleteBy(u => u.WhCode == WhCode && u.HuId == HuId);
                    huMaster = GethuMasterList.First();
                }

                //得到托盘库存
                List<HuDetail> huDetailList = idal.IHuDetailDAL.SelectBy(u => u.HuId == HuId && u.WhCode == WhCode);

                #region 添加上架工作量
                List<WorkloadAccount> addWorkList = new List<WorkloadAccount>();
                foreach (var huDetail in huDetailList)
                {
                    //得到原始数据 进行日志添加
                    TranLog tranLog = new TranLog();
                    tranLog.TranType = "105";
                    tranLog.Description = "摆货操作";
                    tranLog.TranDate = DateTime.Now;
                    tranLog.TranUser = User;
                    tranLog.WhCode = WhCode;
                    tranLog.ClientCode = huDetail.ClientCode;
                    tranLog.SoNumber = huDetail.SoNumber;
                    tranLog.CustomerPoNumber = huDetail.CustomerPoNumber;
                    tranLog.AltItemNumber = huDetail.AltItemNumber;
                    tranLog.ItemId = huDetail.ItemId;
                    tranLog.UnitID = huDetail.UnitId;
                    tranLog.UnitName = huDetail.UnitName;
                    tranLog.TranQty = huDetail.Qty;
                    tranLog.TranQty2 = huDetail.Qty;
                    tranLog.HuId = huDetail.HuId;
                    tranLog.Length = huDetail.Length;
                    tranLog.Width = huDetail.Width;
                    tranLog.Height = huDetail.Height;
                    tranLog.Weight = huDetail.Weight;
                    tranLog.LotNumber1 = huDetail.LotNumber1;
                    tranLog.LotNumber2 = huDetail.LotNumber2;
                    tranLog.LotDate = huDetail.LotDate;
                    tranLog.ReceiptId = huDetail.ReceiptId;
                    tranLog.ReceiptDate = huDetail.ReceiptDate;
                    tranLog.Location = Location;
                    tranLog.Location2 = DestLoc;
                    tranLog.HoldId = huMaster.HoldId;
                    tranLog.HoldReason = huMaster.HoldReason;
                    idal.ITranLogDAL.Add(tranLog);

                    //插入工人工作量
                    if (addWorkList.Where(u => u.WhCode == WhCode && u.HuId == huDetail.HuId).Count() == 0)
                    {
                        WorkloadAccount work = new WorkloadAccount();
                        work.WhCode = WhCode;
                        work.ReceiptId = huDetail.ReceiptId;
                        work.ClientId = huDetail.ClientId;
                        work.ClientCode = huDetail.ClientCode;
                        work.HuId = huDetail.HuId;
                        work.WorkType = "叉车工";
                        work.UserCode = User;
                        work.LotFlag = 0;
                        work.EchFlag = (huDetail.UnitName.Contains("ECH") ? 1 : 0);
                        work.Qty = (Int32)huDetail.Qty;
                        work.CBM = huDetail.Length * huDetail.Width * huDetail.Height * huDetail.Qty;
                        work.Weight = huDetail.Weight;
                        work.ReceiptDate = DateTime.Now;
                        addWorkList.Add(work);
                    }
                    else
                    {
                        WorkloadAccount getModel = addWorkList.Where(u => u.WhCode == WhCode && u.HuId == huDetail.HuId).First();
                        addWorkList.Remove(getModel);

                        WorkloadAccount work = new WorkloadAccount();
                        work.WhCode = WhCode;
                        work.ReceiptId = huDetail.ReceiptId;
                        work.ClientId = huDetail.ClientId;
                        work.ClientCode = huDetail.ClientCode;
                        work.HuId = huDetail.HuId;
                        work.WorkType = "叉车工";
                        work.UserCode = User;
                        work.LotFlag = 0;

                        if (getModel.EchFlag == 1)
                        {
                            work.EchFlag = 1;
                        }
                        else
                        {
                            work.EchFlag = (huDetail.UnitName.Contains("ECH") ? 1 : 0);
                        }

                        work.Qty = (Int32)huDetail.Qty + getModel.Qty;
                        work.CBM = (huDetail.Length * huDetail.Width * huDetail.Height * huDetail.Qty) + getModel.CBM;
                        work.Weight = huDetail.Weight + getModel.Weight;
                        work.ReceiptDate = DateTime.Now;
                        addWorkList.Add(work);
                    }
                }
                idal.IWorkloadAccountDAL.Add(addWorkList);
                #endregion

                //库位变更时间
                InVentoryManager inVentoryManager = new InVentoryManager();
                inVentoryManager.WhLocationEditChangeTime(WhCode, DestLoc);

                //需修改托盘的List
                List<HuDetail> EditList = new List<HuDetail>();
                //需删除托盘的List
                List<HuDetail> DelList = new List<HuDetail>();
                //得到库位托盘库存
                List<HuDetail> GetHuDetailList = idal.IHuDetailDAL.SelectBy(u => u.HuId == DestLoc && u.WhCode == WhCode);
                if (GetHuDetailList.Count == 0)
                {
                    //如果以库位为托盘的库存不存在
                    //直接把原托盘变更为库位托盘
                    foreach (var huDetail in huDetailList)
                    {
                        huDetail.HuId = DestLoc;
                        huDetail.UpdateUser = User;
                        huDetail.UpdateDate = DateTime.Now;
                        idal.IHuDetailDAL.UpdateBy(huDetail, u => u.Id == huDetail.Id, new string[] { "HuId", "UpdateUser", "UpdateDate" });
                    }
                }
                else
                {
                    //以库位为托盘的库存存在
                    //证明 库位托盘有货 需要合并库存
                    foreach (var item in huDetailList)
                    {
                        if (GetHuDetailList.Where(u => u.ClientId == item.ClientId && u.CustomerPoNumber == item.CustomerPoNumber && u.AltItemNumber == item.AltItemNumber && u.ItemId == item.ItemId && u.UnitName == item.UnitName && (u.LotNumber1 == item.LotNumber1 || (u.LotNumber1 == null ? "" : u.LotNumber1) == (item.LotNumber1 == null ? "" : item.LotNumber1)) && (u.LotNumber2 == item.LotNumber2 || (u.LotNumber2 == null ? "" : u.LotNumber2) == (item.LotNumber2 == null ? "" : item.LotNumber2)) && u.LotDate == item.LotDate).Count() == 0)
                        {
                            //如果库位托盘不存在该款号，直接修改原托盘库存信息为库位托盘
                            //原托盘明细修改为库位托盘明细： HuId变更为Location
                            EditList.Add(item);
                        }
                        else
                        {
                            //如果库位托盘存在该款号，需增加库位托盘数量后删除原托盘明细
                            //库位托盘数量++
                            //原托盘明细删除
                            DelList.Add(item);
                        }
                    }

                    foreach (var huDetail in EditList)
                    {
                        huDetail.HuId = DestLoc;
                        huDetail.UpdateUser = User;
                        huDetail.UpdateDate = DateTime.Now;
                        idal.IHuDetailDAL.UpdateBy(huDetail, u => u.Id == huDetail.Id, new string[] { "HuId", "UpdateUser", "UpdateDate" });
                    }

                    List<HuDetail> checkList = new List<HuDetail>();
                    foreach (var item in DelList)
                    {
                        HuDetail getHuDetail = GetHuDetailList.Where(u => u.ClientId == item.ClientId && u.CustomerPoNumber == item.CustomerPoNumber && u.AltItemNumber == item.AltItemNumber && u.ItemId == item.ItemId && u.UnitName == item.UnitName && (u.LotNumber1 == item.LotNumber1 || (u.LotNumber1 == null ? "" : u.LotNumber1) == (item.LotNumber1 == null ? "" : item.LotNumber1)) && (u.LotNumber2 == item.LotNumber2 || (u.LotNumber2 == null ? "" : u.LotNumber2) == (item.LotNumber2 == null ? "" : item.LotNumber2)) && u.LotDate == item.LotDate).First();

                        if (checkList.Where(u => u.Id == getHuDetail.Id).Count() == 0)
                        {
                            getHuDetail.Qty += item.Qty;
                            getHuDetail.UpdateUser = User;
                            getHuDetail.UpdateDate = DateTime.Now;
                            idal.IHuDetailDAL.UpdateBy(getHuDetail, u => u.Id == getHuDetail.Id, new string[] { "Qty", "UpdateUser", "UpdateDate" });
                            checkList.Add(getHuDetail);
                        }
                        else
                        {
                            HuDetail oldHudetail = checkList.Where(u => u.Id == getHuDetail.Id).First();
                            checkList.Remove(oldHudetail);

                            HuDetail newHudetail = oldHudetail;
                            newHudetail.Qty = newHudetail.Qty + item.Qty;
                            newHudetail.UpdateUser = User;
                            newHudetail.UpdateDate = DateTime.Now;
                            idal.IHuDetailDAL.UpdateBy(newHudetail, u => u.Id == newHudetail.Id, new string[] { "Qty", "UpdateUser", "UpdateDate" });
                            checkList.Add(newHudetail);
                        }

                        idal.IHuDetailDAL.DeleteBy(u => u.Id == item.Id);
                    }
                }

                idal.SaveChanges();
                return "Y";
            }
            catch
            {
                return "操作异常请重试！";
            }
        }

        //验证实收数量与登记数量
        public string CheckReceiptQty(ReceiptInsert entity, int PoId)
        {
            bool CheckResult = true;            //验证结果
            string mess = "";

            try
            {
                //得到登记数据
                var regSql = from receiptregisterdetail in idal.IReceiptRegisterDetailDAL.SelectAll()
                             where
                               receiptregisterdetail.ReceiptId == entity.ReceiptId &&
                               receiptregisterdetail.WhCode == entity.WhCode &&
                               receiptregisterdetail.PoId == PoId
                             select new CheckRecModel
                             {
                                 ItemId = receiptregisterdetail.ItemId,
                                 UnitName = receiptregisterdetail.UnitName,
                                 Qty = receiptregisterdetail.RegQty
                             };

                if (regSql.Count() == 0)
                {
                    return "错误！没有找到收货登记信息！";
                }

                List<CheckRecModel> listRegModel = new List<CheckRecModel>();    //登记数据
                List<CheckRecModel> listRfModel = new List<CheckRecModel>();     //预收数据
                List<CheckRecModel> listRecModel = new List<CheckRecModel>();    //实收数据

                //查询数据库得到实收数据
                var recSql = from receipt in idal.IReceiptDAL.SelectAll()
                             where
                               receipt.ReceiptId == entity.ReceiptId &&
                               receipt.WhCode == entity.WhCode &&
                               receipt.PoId == PoId &&
                               receipt.SoNumber == (entity.SoNumber ?? "")
                             group receipt by new
                             {
                                 receipt.ItemId,
                                 receipt.UnitName
                             } into g
                             select new CheckRecModel
                             {
                                 ItemId = g.Key.ItemId,
                                 UnitName = g.Key.UnitName,
                                 Qty = g.Sum(p => p.Qty)
                             };

                //循环实体
                foreach (var item in entity.RecModeldetail)
                {
                    if (item.Qty != 0)
                    {
                        if (item.SerialNumberInModel != null)
                        {
                            if (item.Qty != item.SerialNumberInModel.Count && item.SerialNumberInModel.Count != 0)
                            {
                                mess = "错误！托盘所收数量与扫描序列号数量不符！";
                                break;
                            }
                        }
                    }

                    //得到登记数量
                    var sql_reg = from a in listRegModel
                                  where a.ItemId == item.ItemId
                                  select a;
                    if (sql_reg.Count() == 0)
                    {
                        var reg1 = (from a in regSql
                                    where a.ItemId == item.ItemId && a.UnitName == "none"
                                    select a);
                        CheckRecModel model = new CheckRecModel();
                        if (reg1.Count() > 0)
                        {
                            model = (from a in regSql
                                     where a.ItemId == item.ItemId && a.UnitName == "none"
                                     select a).First();
                        }
                        else
                        {
                            model = (from a in regSql
                                     where a.ItemId == item.ItemId && a.UnitName == item.UnitName
                                     select a).First();
                        }

                        listRegModel.Add(model);
                    }

                    //得到预收数量总和
                    var sql = from a in listRfModel
                              where a.ItemId == item.ItemId && a.UnitName == item.UnitName
                              select a;
                    if (sql.Count() == 0)
                    {
                        CheckRecModel checkRecModel = new CheckRecModel();
                        checkRecModel.ItemId = item.ItemId;
                        checkRecModel.UnitName = item.UnitName;
                        checkRecModel.Qty = item.Qty;
                        listRfModel.Add(checkRecModel);
                    }
                    else
                    {
                        CheckRecModel model = listRfModel.Where(u => u.ItemId == item.ItemId).First();
                        listRfModel.Remove(model);
                        CheckRecModel checkRecModel = new CheckRecModel();
                        checkRecModel.ItemId = item.ItemId;
                        checkRecModel.UnitName = item.UnitName;
                        checkRecModel.Qty = item.Qty + model.Qty;
                        listRfModel.Add(checkRecModel);
                    }

                    //得到实收数量与预收数量的总和
                    var sql_rec1 = from a in recSql
                                   where a.ItemId == item.ItemId && a.UnitName == item.UnitName
                                   select a;
                    var sql_rec2 = from a in listRecModel
                                   where a.ItemId == item.ItemId && a.UnitName == item.UnitName
                                   select a;
                    if (sql_rec2.Count() == 0)
                    {
                        if (sql_rec1.Count() > 0)
                        {
                            CheckRecModel model = sql_rec1.Where(u => u.ItemId == item.ItemId).First();
                            model.Qty = model.Qty + item.Qty;
                            listRecModel.Add(model);
                        }
                    }
                    else
                    {
                        CheckRecModel getRecModel = sql_rec2.Where(u => u.ItemId == item.ItemId).First();
                        listRecModel.Remove(getRecModel);
                        CheckRecModel model = new CheckRecModel();
                        model.ItemId = item.ItemId;
                        model.UnitName = item.UnitName;
                        model.Qty = item.Qty + getRecModel.Qty;
                        listRecModel.Add(model);
                    }
                }
                if (mess != "")
                {
                    return mess;
                }

                if (listRfModel.Where(u => u.Qty == 0).Count() > 0)
                {
                    mess = "错误！托盘所收数量必须大于0！";
                }
                if (mess != "")
                {
                    return mess;
                }

                //如果有实收List 表示有实收 
                //比较实收 与登记
                if (listRecModel.Count > 0)
                {
                    foreach (var item in listRecModel)
                    {
                        if (CheckResult)
                        {
                            int getQty = (from a in listRegModel where a.ItemId == item.ItemId && a.UnitName == item.UnitName select a).First().Qty;
                            if (item.Qty > getQty)
                            {
                                CheckResult = false;
                                mess = "错误！托盘所收数量大于登记数量，请删除重收！";
                            }
                        }
                    }
                }

                if (mess != "")
                {
                    return mess;
                }

                //比较 预收与登记
                foreach (var item in listRfModel)
                {
                    if (CheckResult)
                    {
                        var s = (from a in listRegModel where a.ItemId == item.ItemId && a.UnitName == "none" select a);
                        if (s.Count() > 0)
                        {
                            if (item.Qty > s.First().Qty)
                            {
                                CheckResult = false;
                                mess = "错误！托盘所收数量大于登记数量，请删除重收！";
                            }
                        }
                        else
                        {
                            var s1 = (from a in listRegModel where a.ItemId == item.ItemId && a.UnitName == item.UnitName select a);
                            if (s1.Count() == 0)
                            {
                                CheckResult = false;
                                mess = "错误！托盘所收单位与收货登记单位不一致，请删除重收！";
                            }
                            else
                            {
                                int getQty = (from a in listRegModel where a.ItemId == item.ItemId && a.UnitName == item.UnitName select a).First().Qty;
                                if (item.Qty > getQty)
                                {
                                    CheckResult = false;
                                    mess = "错误！托盘所收数量大于登记数量，请删除重收！";
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                mess = "错误！数据比较异常！";
            }
            if (mess != "")
            {
                return mess;
            }

            //直装收货 需要验证 所收Lot 是否等于直装出库单Lot



            return "";
        }


        //验证实收数量与登记数量 款号可变形
        public string CheckReceiptQtyByItemBX(ReceiptInsert entity, int PoId)
        {
            bool CheckResult = true;            //验证结果
            string mess = "";

            try
            {
                //得到登记数据
                var regSql = from receiptregisterdetail in idal.IReceiptRegisterDetailDAL.SelectAll()
                             where
                               receiptregisterdetail.ReceiptId == entity.ReceiptId &&
                               receiptregisterdetail.WhCode == entity.WhCode &&
                               receiptregisterdetail.PoId == PoId
                             select new CheckRecModel
                             {
                                 ItemId = receiptregisterdetail.ItemId,
                                 UnitName = receiptregisterdetail.UnitName,
                                 Qty = receiptregisterdetail.RegQty
                             };

                if (regSql.Count() == 0)
                {
                    return "错误！没有找到收货登记信息！";
                }

                List<CheckRecModel> listRegModel = new List<CheckRecModel>();    //登记数据
                List<CheckRecModel> listRfModel = new List<CheckRecModel>();     //预收数据
                List<CheckRecModel> listRecModel = new List<CheckRecModel>();    //实收数据

                //查询数据库得到实收数据
                var recSql = from receipt in idal.IReceiptDAL.SelectAll()
                             where
                               receipt.ReceiptId == entity.ReceiptId &&
                               receipt.WhCode == entity.WhCode &&
                               receipt.PoId == PoId &&
                               receipt.SoNumber == (entity.SoNumber ?? "")
                             group receipt by new
                             {
                                 receipt.ItemId
                             } into g
                             select new CheckRecModel
                             {
                                 ItemId = g.Key.ItemId,
                                 Qty = g.Sum(p => p.Qty)
                             };

                //循环实体
                foreach (var item in entity.RecModeldetail)
                {
                    if (item.Qty != 0)
                    {
                        if (item.SerialNumberInModel != null)
                        {
                            if (item.Qty != item.SerialNumberInModel.Count && item.SerialNumberInModel.Count != 0)
                            {
                                mess = "错误！托盘所收数量与扫描序列号数量不符！";
                                break;
                            }
                        }
                    }

                    //得到登记数量
                    var sql_reg = from a in listRegModel
                                  where a.ItemId == item.ItemId
                                  select a;
                    if (sql_reg.Count() == 0)
                    {
                        var reg1 = (from a in regSql
                                    where a.ItemId == item.ItemId && a.UnitName == "none"
                                    select a);
                        CheckRecModel model = new CheckRecModel();
                        if (reg1.Count() > 0)
                        {
                            model = (from a in regSql
                                     where a.ItemId == item.ItemId && a.UnitName == "none"
                                     select a).First();
                        }
                        else
                        {
                            model = (from a in regSql
                                     where a.ItemId == item.ItemId
                                     select a).First();
                        }

                        listRegModel.Add(model);
                    }

                    //得到预收数量总和
                    var sql = from a in listRfModel
                              where a.ItemId == item.ItemId
                              select a;
                    if (sql.Count() == 0)
                    {
                        CheckRecModel checkRecModel = new CheckRecModel();
                        checkRecModel.ItemId = item.ItemId;
                        checkRecModel.Qty = item.Qty;
                        listRfModel.Add(checkRecModel);
                    }
                    else
                    {
                        CheckRecModel model = listRfModel.Where(u => u.ItemId == item.ItemId).First();
                        listRfModel.Remove(model);
                        CheckRecModel checkRecModel = new CheckRecModel();
                        checkRecModel.ItemId = item.ItemId;
                        checkRecModel.Qty = item.Qty + model.Qty;
                        listRfModel.Add(checkRecModel);
                    }

                    //得到实收数量与预收数量的总和
                    var sql_rec1 = from a in recSql
                                   where a.ItemId == item.ItemId
                                   select a;
                    var sql_rec2 = from a in listRecModel
                                   where a.ItemId == item.ItemId
                                   select a;
                    if (sql_rec2.Count() == 0)
                    {
                        if (sql_rec1.Count() > 0)
                        {
                            CheckRecModel model = sql_rec1.Where(u => u.ItemId == item.ItemId).First();
                            model.Qty = model.Qty + item.Qty;
                            listRecModel.Add(model);
                        }
                    }
                    else
                    {
                        CheckRecModel getRecModel = sql_rec2.Where(u => u.ItemId == item.ItemId).First();
                        listRecModel.Remove(getRecModel);
                        CheckRecModel model = new CheckRecModel();
                        model.ItemId = item.ItemId;
                        model.Qty = item.Qty + getRecModel.Qty;
                        listRecModel.Add(model);
                    }
                }
                if (mess != "")
                {
                    return mess;
                }

                if (listRfModel.Where(u => u.Qty == 0).Count() > 0)
                {
                    mess = "错误！托盘所收数量必须大于0！";
                }
                if (mess != "")
                {
                    return mess;
                }

                //如果有实收List 表示有实收 
                //比较实收 与登记
                if (listRecModel.Count > 0)
                {
                    foreach (var item in listRecModel)
                    {
                        if (CheckResult)
                        {
                            int getQty = (from a in listRegModel where a.ItemId == item.ItemId select a).First().Qty;
                            if (item.Qty > getQty)
                            {
                                CheckResult = false;
                                mess = "错误！托盘所收数量大于登记数量，请删除重收！";
                            }
                        }
                    }
                }

                if (mess != "")
                {
                    return mess;
                }

                //比较 预收与登记
                foreach (var item in listRfModel)
                {
                    if (CheckResult)
                    {
                        var s = (from a in listRegModel where a.ItemId == item.ItemId && a.UnitName == "none" select a);
                        if (s.Count() > 0)
                        {
                            if (item.Qty > s.First().Qty)
                            {
                                CheckResult = false;
                                mess = "错误！托盘所收数量大于登记数量，请删除重收！";
                            }
                        }
                        else
                        {
                            var s1 = (from a in listRegModel where a.ItemId == item.ItemId select a);
                            if (s1.Count() == 0)
                            {
                                CheckResult = false;
                                mess = "错误！托盘所收数据与收货登记数据不一致，请删除重收！";
                            }
                            else
                            {
                                int getQty = (from a in listRegModel where a.ItemId == item.ItemId select a).First().Qty;
                                if (item.Qty > getQty)
                                {
                                    CheckResult = false;
                                    mess = "错误！托盘所收数量大于登记数量，请删除重收！";
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                mess = "错误！数据比较异常！";
            }
            if (mess != "")
            {
                return mess;
            }

            //直装收货 需要验证 所收Lot 是否等于直装出库单Lot

            return "";
        }

        public void HuMasterInsert(ReceiptInsert entity)
        {
            var sql = idal.IHuMasterDAL.SelectBy(u => u.HuId == entity.HuId && u.WhCode == entity.WhCode).ToList();
            if (sql.Count() == 0)
            {
                HuMaster hu = new HuMaster();
                hu.WhCode = entity.WhCode;
                hu.HuId = entity.HuId;
                hu.Type = "M";
                if (entity.HoldMasterModel != null)
                {
                    hu.Status = "H";
                    hu.HoldId = entity.HoldMasterModel.HoldId;
                    hu.HoldReason = entity.HoldMasterModel.HoldReason;
                }
                else
                {
                    hu.Status = "A";
                }

                if (entity.HuHeight != null)
                {
                    hu.HuHeight = entity.HuHeight;
                }
                if (entity.HuLength != null)
                {
                    hu.HuLength = entity.HuLength;
                }
                if (entity.HuWidth != null)
                {
                    hu.HuWidth = entity.HuWidth;
                }
                if (entity.HuWeight != null)
                {
                    hu.HuWeight = entity.HuWeight;
                }

                hu.Location = entity.Location;
                hu.TransactionFlag = 0;
                hu.ReceiptId = "";
                hu.ReceiptDate = DateTime.Now;
                hu.CreateUser = entity.CreateUser;
                hu.CreateDate = DateTime.Now;
                idal.IHuMasterDAL.Add(hu);
            }

            HuDetailInsert(entity);
        }

        public void HuDetailInsert(ReceiptInsert entity)
        {
            List<HuDetail> addList = new List<HuDetail>();
            foreach (var item in entity.RecModeldetail)
            {
                HuDetail hu = new HuDetail();
                hu.WhCode = entity.WhCode;
                hu.HuId = entity.HuId;
                hu.ClientId = entity.ClientId;
                hu.ClientCode = entity.ClientCode;
                hu.SoNumber = entity.SoNumber;
                hu.CustomerPoNumber = entity.CustomerPoNumber;
                hu.ReceiptId = entity.ReceiptId;
                hu.ReceiptDate = DateTime.Now;
                hu.CreateUser = entity.CreateUser;
                hu.CreateDate = DateTime.Now;
                hu.AltItemNumber = item.AltItemNumber;
                hu.ItemId = item.ItemId;
                hu.UnitId = item.UnitId;
                hu.UnitName = item.UnitName;
                hu.Qty = item.Qty;
                hu.PlanQty = 0;
                hu.Length = item.Length / 100;
                hu.Width = item.Width / 100;
                hu.Height = item.Height / 100;
                hu.Weight = item.Weight;
                hu.LotNumber1 = item.LotNumber1;
                hu.LotNumber2 = item.LotNumber2;
                hu.LotDate = item.LotDate;
                hu.Attribute1 = item.Attribute1;

                addList.Add(hu);
            }

            idal.IHuDetailDAL.Add(addList);
            idal.IReceiptDAL.SaveChanges();
        }

        //验证收货扫描序列号
        private string CheckSerialNumberList(ReceiptInsert entity, RecModeldetail recDetail, List<string> searNumber)
        {
            string result = "";

            if (recDetail.SerialNumberInModel != null)
            {
                //1.先验证 是否重复
                foreach (var ser in recDetail.SerialNumberInModel)
                {
                    if (ser != null)
                    {
                        if (searNumber.Contains(ser.CartonId) == true)
                        {
                            result = "序列号扫描重复！";
                            break;
                        }
                        else
                        {
                            searNumber.Add(ser.CartonId);
                        }
                    }
                }
                if (result != "")
                {
                    return result;
                }

                //2.验证是否在同一收货批次下 存在重复
                if (searNumber.Count > 0)
                {
                    List<SerialNumberIn> list = idal.ISerialNumberInDAL.SelectBy(u => u.WhCode == entity.WhCode && u.ReceiptId == entity.ReceiptId);

                    int errorCount = list.Where(u => searNumber.Contains(u.CartonId)).Count();
                    if (errorCount > 0)
                    {
                        result = list.Where(u => searNumber.Contains(u.CartonId)).First().CartonId + "等" + errorCount + "个SN已扫描!";
                        //result = "序列号已存在！";
                    }

                    if (result != "")
                    {
                        return result;
                    }
                }
            }
            return "Y";
        }

        private void SerialNumberInsert(ReceiptInsert entity, int PoId, RecModeldetail recDetail)
        {
            foreach (var ser in recDetail.SerialNumberInModel)
            {
                if (ser != null)
                {
                    //插入采集箱号表
                    SerialNumberIn serial = new SerialNumberIn();
                    serial.WhCode = entity.WhCode;
                    serial.ReceiptId = entity.ReceiptId;
                    serial.ClientId = entity.ClientId;
                    serial.ClientCode = entity.ClientCode;
                    serial.SoNumber = entity.SoNumber;
                    serial.CustomerPoNumber = entity.CustomerPoNumber;
                    serial.AltItemNumber = recDetail.AltItemNumber;
                    serial.PoId = PoId;
                    serial.ItemId = recDetail.ItemId;
                    serial.CartonId = ser.CartonId;
                    serial.HuId = entity.HuId;
                    serial.Length = recDetail.Length / 100;
                    serial.Width = recDetail.Width / 100;
                    serial.Height = recDetail.Height / 100;
                    serial.Weight = recDetail.Weight;
                    serial.LotNumber1 = recDetail.LotNumber1;
                    serial.LotNumber2 = recDetail.LotNumber2;
                    serial.LotDate = recDetail.LotDate;
                    serial.CreateUser = entity.CreateUser;
                    serial.CreateDate = DateTime.Now;
                    serial.ToOutStatus = 1;

                    idal.ISerialNumberInDAL.Add(serial);
                }
            }
        }

        public void AddReceiptTranLog(ReceiptInsert entity)
        {
            foreach (var item in entity.RecModeldetail)
            {
                TranLog tl = new TranLog();
                tl.TranType = "900";
                tl.Description = "拦截订单快捷收货";
                tl.TranDate = DateTime.Now;
                tl.TranUser = entity.CreateUser;
                tl.WhCode = entity.WhCode;
                tl.ClientCode = entity.ClientCode;
                tl.SoNumber = entity.SoNumber;
                tl.CustomerPoNumber = entity.CustomerPoNumber;
                tl.PoID = entity.PoId;
                tl.AltItemNumber = item.AltItemNumber;
                tl.ItemId = item.ItemId;
                tl.UnitID = item.UnitId;
                tl.UnitName = item.UnitName;
                tl.Status = entity.Status;
                tl.TranQty2 = item.Qty;
                tl.HuId = entity.HuId;
                tl.LotFlag = entity.LotFlag;
                tl.Length = item.Length / 100;
                tl.Width = item.Width / 100;
                tl.Height = item.Height / 100;
                tl.Weight = item.Weight;
                tl.LotNumber1 = item.LotNumber1;
                tl.LotNumber2 = item.LotNumber2;
                tl.LotDate = item.LotDate;
                tl.ReceiptId = entity.ReceiptId;
                tl.ReceiptDate = DateTime.Now;
                tl.Location = entity.Location;
                tl.HoldId = entity.HoldMasterModel == null ? 0 : entity.HoldMasterModel.HoldId;
                tl.HoldReason = entity.HoldMasterModel == null ? null : entity.HoldMasterModel.HoldReason;

                idal.ITranLogDAL.Add(tl);
            }
        }


        //拦截订单批量回库
        public string InterceptOrderBatchReturnToWarehouse(int outBoundOrderId, string whCode, string userName, string abLocation)
        {
            var sql7 = from a in idal.IWhLocationDAL.SelectAll()
                       join b in idal.ILocationTypeDAL.SelectAll() on new { LocationTypeId = a.LocationTypeId } equals new { LocationTypeId = b.Id }
                       where
                         b.TypeName == "AB" &&
                         a.WhCode == whCode && a.LocationId == abLocation
                       select new
                       {
                           a.LocationId
                       };
            if (sql7.Count() == 0)
            {
                return "未检测到该异常库位，请去基础数据管理-储位管理中调整异常库位！";
            }

            //托盘不存在 优先创建托盘号
            if (idal.IPallateDAL.SelectBy(u => u.WhCode == whCode && u.HuId == abLocation).Count == 0)
            {
                Pallate pallate = new Pallate();
                pallate.WhCode = whCode;
                pallate.HuId = abLocation;
                pallate.TypeId = 1;
                pallate.Status = "U";
                idal.IPallateDAL.Add(pallate);
            }

            List<OutBoundOrder> OutBoundOrderList = idal.IOutBoundOrderDAL.SelectBy(u => u.Id == outBoundOrderId);
            if (OutBoundOrderList.Where(u => u.StatusId != -10).Count() > 0)
            {
                return "拦截订单状态有误，请重新查询后再次操作！";
            }
            if (OutBoundOrderList.Where(u => u.StatusName != "已拦截待处理").Count() > 0)
            {
                return "拦截订单状态有误，请重新查询后再次操作！";
            }

            List<LoadMasterResult> LoadMasterList = (from a in idal.ILoadMasterDAL.SelectAll()
                                                     join b in idal.ILoadDetailDAL.SelectAll() on new { Id = a.Id } equals new { Id = (Int32)b.LoadMasterId } into b_join
                                                     from b in b_join.DefaultIfEmpty()
                                                     join c in idal.IOutBoundOrderDAL.SelectAll() on new { OutBoundOrderId = (Int32)b.OutBoundOrderId } equals new { OutBoundOrderId = c.Id } into c_join
                                                     from c in c_join.DefaultIfEmpty()
                                                     where c.Id == outBoundOrderId
                                                     select new LoadMasterResult
                                                     {
                                                         LoadId = a.LoadId,
                                                         ClientId = c.ClientId,
                                                         ClientCode = c.ClientCode,
                                                         OutBoundOrderId = c.Id
                                                     }).ToList();

            //得到Load号
            LoadMasterResult LoadMasterResultEveryOne = LoadMasterList.First();
            string LoadId = LoadMasterResultEveryOne.LoadId;

            List<TranLog> tranLogList = new List<TranLog>();

            //得到订单多条明细及Lot
            List<OutBoundOrderDetail> OutBoundOrderDetailEveryOneList = idal.IOutBoundOrderDetailDAL.SelectBy(u => u.OutBoundOrderId == outBoundOrderId).ToList();
            //得到订单主表
            OutBoundOrder OutBoundOrderEveryOne = OutBoundOrderList.First();

            List<HuDetail> huDetailCheck = new List<HuDetail>();
            List<HuDetail> addHuDetailCheck = new List<HuDetail>();


            List<HuDetail> CheckList = new List<HuDetail>();
            //处理订单明细库存
            foreach (var item1 in OutBoundOrderDetailEveryOneList)
            {
                int ResultQty = item1.Qty;

                List<HuDetail> sqlDetailList = idal.IHuDetailDAL.SelectBy(u => u.HuId == LoadMasterResultEveryOne.LoadId && u.WhCode == whCode && u.ClientCode == LoadMasterResultEveryOne.ClientCode && u.AltItemNumber == item1.AltItemNumber && u.ItemId == item1.ItemId && u.UnitName == item1.UnitName && (u.LotNumber1 ?? "") == (item1.LotNumber1 ?? "") && (u.LotNumber2 ?? "") == (item1.LotNumber2 ?? ""));

                #region 库存扣减
                foreach (var sqlDetail in sqlDetailList)
                {
                    if (ResultQty == 0)
                    {
                        break;
                    }

                    if (huDetailCheck.Where(u => u.Id == sqlDetail.Id).Count() == 0)
                    {
                        if (sqlDetail.Qty >= ResultQty)
                        {
                            HuDetail huDetail = new HuDetail();
                            huDetail.Id = sqlDetail.Id;
                            huDetail.HuId = sqlDetail.HuId;
                            huDetail.WhCode = sqlDetail.WhCode;
                            huDetail.ClientCode = sqlDetail.ClientCode;
                            huDetail.ClientId = sqlDetail.ClientId;

                            huDetail.SoNumber = OutBoundOrderEveryOne.AltCustomerOutPoNumber;
                            huDetail.Attribute1 = OutBoundOrderEveryOne.OutPoNumber;
                            huDetail.CustomerPoNumber = OutBoundOrderEveryOne.CustomerOutPoNumber;
                            huDetail.ReceiptId = LoadMasterResultEveryOne.LoadId;

                            huDetail.AltItemNumber = sqlDetail.AltItemNumber;
                            huDetail.ItemId = sqlDetail.ItemId;
                            huDetail.UnitId = sqlDetail.UnitId;
                            huDetail.UnitName = sqlDetail.UnitName;
                            huDetail.LotNumber1 = sqlDetail.LotNumber1;
                            huDetail.LotNumber2 = sqlDetail.LotNumber2;
                            //24年调整：拦截订单的Lotdate时间为2023-01-01，退货的Lotdate时间为2022-01-01
                            huDetail.LotDate = Convert.ToDateTime("2023-01-01 00:00:00");
                            huDetail.Qty = sqlDetail.Qty - ResultQty;
                            huDetailCheck.Add(huDetail);

                            //插入tranLog
                            TranLog tl = new TranLog();
                            tl.TranType = "131";
                            tl.Description = "拦截订单匹配库存删除";
                            tl.TranDate = DateTime.Now;
                            tl.TranUser = userName;
                            tl.WhCode = sqlDetail.WhCode;
                            tl.ClientCode = sqlDetail.ClientCode;
                            tl.SoNumber = OutBoundOrderEveryOne.AltCustomerOutPoNumber;
                            tl.CustomerPoNumber = sqlDetail.CustomerPoNumber;
                            tl.AltItemNumber = sqlDetail.AltItemNumber;
                            tl.ItemId = sqlDetail.ItemId;
                            tl.UnitID = sqlDetail.UnitId;
                            tl.UnitName = sqlDetail.UnitName;
                            tl.TranQty = sqlDetail.Qty;
                            tl.TranQty2 = ResultQty;
                            tl.HuId = sqlDetail.HuId;
                            tl.Length = 0;
                            tl.Width = 0;
                            tl.Height = 0;
                            tl.Weight = 0;
                            tl.LotNumber1 = sqlDetail.LotNumber1;
                            tl.LotNumber2 = sqlDetail.LotNumber2;
                            tl.LotDate = sqlDetail.LotDate;
                            tl.ReceiptId = sqlDetail.HuId;
                            tl.ReceiptDate = DateTime.Now;
                            tl.OutPoNumber = OutBoundOrderEveryOne.OutPoNumber;
                            tl.CustomerOutPoNumber = OutBoundOrderEveryOne.CustomerOutPoNumber;
                            tl.LoadId = LoadMasterResultEveryOne.LoadId;
                            tl.Remark = "库存剩余数" + huDetail.Qty;
                            tranLogList.Add(tl);

                            ResultQty = 0;
                        }
                        else
                        {
                            HuDetail huDetail = new HuDetail();
                            huDetail.Id = sqlDetail.Id;
                            huDetail.HuId = sqlDetail.HuId;
                            huDetail.WhCode = sqlDetail.WhCode;
                            huDetail.ClientCode = sqlDetail.ClientCode;
                            huDetail.ClientId = sqlDetail.ClientId;

                            huDetail.SoNumber = OutBoundOrderEveryOne.AltCustomerOutPoNumber;
                            huDetail.Attribute1 = OutBoundOrderEveryOne.OutPoNumber;
                            huDetail.CustomerPoNumber = OutBoundOrderEveryOne.CustomerOutPoNumber;
                            huDetail.ReceiptId = LoadMasterResultEveryOne.LoadId;

                            huDetail.AltItemNumber = sqlDetail.AltItemNumber;
                            huDetail.ItemId = sqlDetail.ItemId;
                            huDetail.UnitId = sqlDetail.UnitId;
                            huDetail.UnitName = sqlDetail.UnitName;
                            huDetail.LotNumber1 = sqlDetail.LotNumber1;
                            huDetail.LotNumber2 = sqlDetail.LotNumber2;
                            //24年调整：拦截订单的Lotdate时间为2023-01-01，退货的Lotdate时间为2022-01-01
                            huDetail.LotDate = Convert.ToDateTime("2023-01-01 00:00:00");
                            huDetail.Qty = 0;
                            huDetailCheck.Add(huDetail);

                            //插入tranLog
                            TranLog tl = new TranLog();
                            tl.TranType = "131";
                            tl.Description = "拦截订单匹配库存删除";
                            tl.TranDate = DateTime.Now;
                            tl.TranUser = userName;
                            tl.WhCode = sqlDetail.WhCode;
                            tl.ClientCode = sqlDetail.ClientCode;
                            tl.SoNumber = OutBoundOrderEveryOne.AltCustomerOutPoNumber;
                            tl.CustomerPoNumber = sqlDetail.CustomerPoNumber;
                            tl.AltItemNumber = sqlDetail.AltItemNumber;
                            tl.ItemId = sqlDetail.ItemId;
                            tl.UnitID = sqlDetail.UnitId;
                            tl.UnitName = sqlDetail.UnitName;
                            tl.TranQty = sqlDetail.Qty;
                            tl.TranQty2 = sqlDetail.Qty;
                            tl.HuId = sqlDetail.HuId;
                            tl.Length = 0;
                            tl.Width = 0;
                            tl.Height = 0;
                            tl.Weight = 0;
                            tl.LotNumber1 = sqlDetail.LotNumber1;
                            tl.LotNumber2 = sqlDetail.LotNumber2;
                            tl.LotDate = sqlDetail.LotDate;
                            tl.ReceiptId = sqlDetail.HuId;
                            tl.ReceiptDate = DateTime.Now;
                            tl.OutPoNumber = OutBoundOrderEveryOne.OutPoNumber;
                            tl.CustomerOutPoNumber = OutBoundOrderEveryOne.CustomerOutPoNumber;
                            tl.LoadId = LoadMasterResultEveryOne.LoadId;
                            tl.Remark = "库存剩余数" + huDetail.Qty;
                            tranLogList.Add(tl);

                            ResultQty = ResultQty - sqlDetail.Qty;
                        }
                    }
                    else
                    {
                        HuDetail huDetail = huDetailCheck.Where(u => u.Id == sqlDetail.Id).First();
                        if (huDetail.Qty == 0)
                        {
                            continue;
                        }
                        huDetailCheck.Remove(huDetail);

                        if (huDetail.Qty >= ResultQty)
                        {
                            HuDetail newhuDetail = huDetail;
                            newhuDetail.Qty = newhuDetail.Qty - ResultQty;
                            huDetailCheck.Add(newhuDetail);

                            //插入tranLog
                            TranLog tl = new TranLog();
                            tl.TranType = "131";
                            tl.Description = "拦截订单匹配库存删除";
                            tl.TranDate = DateTime.Now;
                            tl.TranUser = userName;
                            tl.WhCode = huDetail.WhCode;
                            tl.ClientCode = huDetail.ClientCode;
                            tl.SoNumber = OutBoundOrderEveryOne.AltCustomerOutPoNumber;
                            tl.CustomerPoNumber = huDetail.CustomerPoNumber;
                            tl.AltItemNumber = huDetail.AltItemNumber;
                            tl.ItemId = huDetail.ItemId;
                            tl.UnitID = huDetail.UnitId;
                            tl.UnitName = huDetail.UnitName;
                            tl.TranQty = huDetail.Qty;
                            tl.TranQty2 = ResultQty;
                            tl.HuId = huDetail.HuId;
                            tl.Length = 0;
                            tl.Width = 0;
                            tl.Height = 0;
                            tl.Weight = 0;
                            tl.LotNumber1 = huDetail.LotNumber1;
                            tl.LotNumber2 = huDetail.LotNumber2;
                            tl.LotDate = huDetail.LotDate;
                            tl.ReceiptId = huDetail.HuId;
                            tl.ReceiptDate = DateTime.Now;
                            tl.OutPoNumber = OutBoundOrderEveryOne.OutPoNumber;
                            tl.CustomerOutPoNumber = OutBoundOrderEveryOne.CustomerOutPoNumber;
                            tl.LoadId = LoadMasterResultEveryOne.LoadId;
                            tl.Remark = "库存剩余数" + newhuDetail.Qty;
                            tranLogList.Add(tl);

                            ResultQty = 0;
                        }
                        else
                        {
                            HuDetail newhuDetail = huDetail;
                            newhuDetail.Qty = 0;
                            huDetailCheck.Add(newhuDetail);

                            //插入tranLog
                            TranLog tl = new TranLog();
                            tl.TranType = "131";
                            tl.Description = "拦截订单匹配库存删除";
                            tl.TranDate = DateTime.Now;
                            tl.TranUser = userName;
                            tl.WhCode = huDetail.WhCode;
                            tl.ClientCode = huDetail.ClientCode;
                            tl.SoNumber = OutBoundOrderEveryOne.AltCustomerOutPoNumber;
                            tl.CustomerPoNumber = huDetail.CustomerPoNumber;
                            tl.AltItemNumber = huDetail.AltItemNumber;
                            tl.ItemId = huDetail.ItemId;
                            tl.UnitID = huDetail.UnitId;
                            tl.UnitName = huDetail.UnitName;
                            tl.TranQty = huDetail.Qty;
                            tl.TranQty2 = huDetail.Qty;
                            tl.HuId = huDetail.HuId;
                            tl.Length = 0;
                            tl.Width = 0;
                            tl.Height = 0;
                            tl.Weight = 0;
                            tl.LotNumber1 = huDetail.LotNumber1;
                            tl.LotNumber2 = huDetail.LotNumber2;
                            tl.LotDate = huDetail.LotDate;
                            tl.ReceiptId = huDetail.HuId;
                            tl.ReceiptDate = DateTime.Now;
                            tl.OutPoNumber = OutBoundOrderEveryOne.OutPoNumber;
                            tl.CustomerOutPoNumber = OutBoundOrderEveryOne.CustomerOutPoNumber;
                            tl.LoadId = LoadMasterResultEveryOne.LoadId;
                            tl.Remark = "库存剩余数" + newhuDetail.Qty;
                            tranLogList.Add(tl);

                            ResultQty = ResultQty - huDetail.Qty;

                        }
                    }               
                }
                #endregion


                #region 根据拦截出库订单增加库存明细
                if (addHuDetailCheck.Where(u => u.CustomerPoNumber == OutBoundOrderEveryOne.CustomerOutPoNumber && u.ClientCode == OutBoundOrderEveryOne.ClientCode && u.WhCode == item1.WhCode && u.AltItemNumber == item1.AltItemNumber && u.ItemId == item1.ItemId && u.UnitName == item1.UnitName && (u.LotNumber1 ?? "") == (item1.LotNumber1 ?? "") && (u.LotNumber2 ?? "") == (item1.LotNumber2 ?? "")).Count() == 0)
                {
                    HuDetail hu = new HuDetail();
                    hu.WhCode = item1.WhCode;
                    hu.HuId = abLocation;
                    hu.ClientId = OutBoundOrderEveryOne.ClientId;
                    hu.ClientCode = OutBoundOrderEveryOne.ClientCode;

                    hu.SoNumber = OutBoundOrderEveryOne.AltCustomerOutPoNumber;
                    hu.Attribute1 = OutBoundOrderEveryOne.OutPoNumber;
                    hu.CustomerPoNumber = OutBoundOrderEveryOne.CustomerOutPoNumber;
                    hu.ReceiptId = LoadMasterResultEveryOne.LoadId;

                    hu.ReceiptDate = DateTime.Now;
                    hu.CreateUser = userName;
                    hu.CreateDate = DateTime.Now;
                    hu.AltItemNumber = item1.AltItemNumber;
                    hu.ItemId = item1.ItemId;
                    hu.UnitId = item1.UnitId;
                    hu.UnitName = item1.UnitName;
                    hu.Qty = item1.Qty;
                    hu.PlanQty = 0;
                    hu.Length = 0;
                    hu.Width = 0;
                    hu.Height = 0;
                    hu.Weight = 0;
                    hu.LotNumber1 = item1.LotNumber1;
                    hu.LotNumber2 = item1.LotNumber2;
                    //24年调整：拦截订单的Lotdate时间为2023-01-01，退货的Lotdate时间为2022-01-01
                    hu.LotDate = Convert.ToDateTime("2023-01-01 00:00:00");
                    addHuDetailCheck.Add(hu);
                }
                else
                {
                    HuDetail oldHuDetail = addHuDetailCheck.Where(u => u.CustomerPoNumber == OutBoundOrderEveryOne.CustomerOutPoNumber && u.ClientCode == OutBoundOrderEveryOne.ClientCode && u.WhCode == item1.WhCode && u.AltItemNumber == item1.AltItemNumber && u.ItemId == item1.ItemId && u.UnitName == item1.UnitName && (u.LotNumber1 ?? "") == (item1.LotNumber1 ?? "") && (u.LotNumber2 ?? "") == (item1.LotNumber2 ?? "")).First();
                    addHuDetailCheck.Remove(oldHuDetail);

                    HuDetail InsertNewHuDetail = oldHuDetail;
                    InsertNewHuDetail.Qty = InsertNewHuDetail.Qty + item1.Qty;
                    addHuDetailCheck.Add(InsertNewHuDetail);
                }
                #endregion

            }

            //int s = huDetailCheck.Count();
            //int s1 = addHuDetailCheck.Count();

            //return "Y";

            var sql = idal.IHuMasterDAL.SelectBy(u => u.HuId == abLocation && u.WhCode == whCode).ToList();
            if (sql.Count() == 0)
            {
                HuMaster hu = new HuMaster();
                hu.WhCode = whCode;
                hu.HuId = abLocation;
                hu.Type = "M";
                hu.Status = "A";
                hu.Location = abLocation;
                hu.TransactionFlag = 0;
                hu.ReceiptId = "";
                hu.CreateUser = userName;
                hu.CreateDate = DateTime.Now;
                idal.IHuMasterDAL.Add(hu);
            }

            //原库存数量进行删减或清除处理
            if (huDetailCheck.Count > 0)
            {
                foreach (var itemhuDetail in huDetailCheck)
                {
                    //插入tranLog
                    TranLog tl = new TranLog();
                    tl.TranType = "131";
                    tl.Description = "拦截订单库存处理";
                    tl.TranDate = DateTime.Now;
                    tl.TranUser = userName;
                    tl.WhCode = itemhuDetail.WhCode;
                    tl.ClientCode = itemhuDetail.ClientCode;
                    tl.SoNumber = itemhuDetail.SoNumber;
                    tl.CustomerPoNumber = itemhuDetail.CustomerPoNumber;
                    tl.AltItemNumber = itemhuDetail.AltItemNumber;
                    tl.ItemId = itemhuDetail.ItemId;
                    tl.UnitID = itemhuDetail.UnitId;
                    tl.UnitName = itemhuDetail.UnitName;
                    tl.TranQty = itemhuDetail.Qty;
                    tl.TranQty2 = itemhuDetail.Qty;
                    tl.HuId = itemhuDetail.HuId;
                    tl.Length = 0;
                    tl.Width = 0;
                    tl.Height = 0;
                    tl.Weight = 0;
                    tl.LotNumber1 = itemhuDetail.LotNumber1;
                    tl.LotNumber2 = itemhuDetail.LotNumber2;
                    tl.LotDate = itemhuDetail.LotDate;
                    tl.ReceiptId = itemhuDetail.ReceiptId;
                    tl.ReceiptDate = DateTime.Now;
                    tl.OutPoNumber = itemhuDetail.Attribute1;
                    tl.CustomerOutPoNumber = itemhuDetail.CustomerPoNumber;
                    tl.LoadId = itemhuDetail.HuId;
                    tl.Remark = "库存更新为" + itemhuDetail.Qty;
                    tranLogList.Add(tl);

                    if (itemhuDetail.Qty == 0)
                    {
                        idal.IHuDetailDAL.DeleteBy(u => u.Id == itemhuDetail.Id);
                    }
                    else
                    {
                        HuDetail setHuDetail = new HuDetail();
                        setHuDetail.Id = itemhuDetail.Id;
                        setHuDetail.Qty = itemhuDetail.Qty;
                        setHuDetail.UpdateUser = userName;
                        setHuDetail.UpdateDate = DateTime.Now;

                        idal.IHuDetailDAL.UpdateBy(setHuDetail, u => u.Id == setHuDetail.Id, new string[] { "Qty", "UpdateDate", "UpdateUser" });
                    }
                }
            }

            //添加新库存数量
            if (addHuDetailCheck.Count > 0)
            {
                foreach (var itemhuDetail in addHuDetailCheck)
                {
                    //插入tranLog
                    TranLog tl = new TranLog();
                    tl.TranType = "132";
                    tl.Description = "拦截订单库存回库至异常库位";
                    tl.TranDate = DateTime.Now;
                    tl.TranUser = userName;
                    tl.WhCode = itemhuDetail.WhCode;
                    tl.ClientCode = itemhuDetail.ClientCode;
                    tl.SoNumber = itemhuDetail.SoNumber;
                    tl.CustomerPoNumber = itemhuDetail.CustomerPoNumber;
                    tl.AltItemNumber = itemhuDetail.AltItemNumber;
                    tl.ItemId = itemhuDetail.ItemId;
                    tl.UnitID = itemhuDetail.UnitId;
                    tl.UnitName = itemhuDetail.UnitName;
                    tl.TranQty = itemhuDetail.Qty;
                    tl.TranQty2 = itemhuDetail.Qty;
                    tl.HuId = itemhuDetail.HuId;
                    tl.Length = 0;
                    tl.Width = 0;
                    tl.Height = 0;
                    tl.Weight = 0;
                    tl.LotNumber1 = itemhuDetail.LotNumber1;
                    tl.LotNumber2 = itemhuDetail.LotNumber2;
                    tl.LotDate = itemhuDetail.LotDate;
                    tl.ReceiptId = itemhuDetail.ReceiptId;
                    tl.ReceiptDate = DateTime.Now;
                    tl.OutPoNumber = itemhuDetail.Attribute1;
                    tl.CustomerOutPoNumber = itemhuDetail.CustomerPoNumber;
                    tl.LoadId = itemhuDetail.ReceiptId;
                    tl.Remark = "库存数量增加" + itemhuDetail.Qty;
                    tranLogList.Add(tl);
                }

                idal.IHuDetailDAL.Add(addHuDetailCheck);
            }

            idal.ITranLogDAL.Add(tranLogList);

            //更新订单状态
            foreach (var item in OutBoundOrderList)
            {
                item.StatusName = "已拦截已处理";
                item.InterceptFlag = 1;
                item.ReceiptId = abLocation;
                item.UpdateUser = userName;
                item.UpdateDate = DateTime.Now;

                idal.IOutBoundOrderDAL.UpdateBy(item, u => u.Id == item.Id, new string[] { "StatusName", "InterceptFlag", "ReceiptId", "UpdateUser", "UpdateDate" });

                ////订单已拦截已处理，InterceptFlag：订单拦截处理标识，1标识系统已处理可删除拦截订单分拣及包装数据
                //idal.IOutBoundOrderDAL.UpdateByExtended(u => outBoundOrderIdList.Contains(u.Id), t => new OutBoundOrder { StatusName = "已拦截已处理", InterceptFlag = 1, ReceiptId = abLocation, UpdateUser = userName, UpdateDate = DateTime.Now });    
            }

            idal.IOutBoundOrderDAL.SaveChanges();


            return "Y";

        }

    }
}

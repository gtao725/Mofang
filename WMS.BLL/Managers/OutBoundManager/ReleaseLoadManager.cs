using MODEL_MSSQL;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using WMS.BLLClass;
using WMS.IBLL;

namespace WMS.BLL
{
    public class ReleaseLoadManager : IReleaseLoadManager
    {
        IDAL.IDALSession idal = BLL.BLLHelper.GetDal();
        private static object o = new object();

        ShipHelper shipHelper = new ShipHelper();

        SortTaskManager sortTaskManager = new SortTaskManager();


        //释放Load
        //最后释放完成时 需要同时创建分拣任务
        public string CheckReleaseLoad(string loadId, string whCode, string userName)
        {
            if (loadId == "" || whCode == "" || userName == "" || loadId == null || whCode == null || userName == null)
            {
                return "数据有误，请重新操作！";
            }
            lock (o)
            {
                return "该释放方法已禁用，请联系IT处理！";
            }
        }

        //撤销释放
        //需要验证是否备货 和 是否分拣
        public string RollbackLoad(string loadId, string whCode, string userName)
        {
            using (TransactionScope trans = new TransactionScope())
            {
                try
                {
                    #region 撤销释放优化
                    if (loadId == null || loadId == "" || whCode == "" || whCode == null || userName == null || userName == "")
                    {
                        return "数据有误，请重新操作！";
                    }
                    string result = "";
                    LoadMaster loadMaster = idal.ILoadMasterDAL.SelectBy(u => u.LoadId == loadId && u.WhCode == whCode).First();
                    if (loadMaster.Status1 != "U" || loadMaster.Status2 != "U")
                    {
                        return "该Load:" + loadId + "已备货或已分拣，无法撤销释放！";
                    }

                    List<PickTaskDetail> pickList = idal.IPickTaskDetailDAL.SelectBy(u => u.WhCode == whCode && u.LoadId == loadId && u.DSFLag != 1);
                    if (pickList.Where(u => u.Status != "U").Count() > 0)
                    {
                        return "该Load:" + loadId + "正在备货，无法撤销释放！";
                    }

                    List<SortTask> sortList = idal.ISortTaskDAL.SelectBy(u => u.WhCode == whCode && u.LoadId == loadId);
                    if (sortList.Where(u => u.Status != "U").Count() > 0)
                    {
                        return "该Load:" + loadId + "正在分拣，无法撤销释放！";
                    }

                    List<ReceiptRegister> regList = idal.IReceiptRegisterDAL.SelectBy(u => u.WhCode == whCode && u.LoadMasterId == loadMaster.Id && u.DSFLag == 1);
                    if (regList.Count > 0)
                    {
                        return "该Load:" + loadId + "的直装订单已生成收货操作单，无法撤销释放！<br />请道口删除直装收货操作单后再次撤销！";
                    }

                    List<TranLog> tranLogList = new List<TranLog>();

                    int?[] getHuDetailId = (from a in pickList
                                            select a.HuDetailId).ToArray().Distinct().ToArray();

                    List<HuDetail> huDetailList1 = idal.IHuDetailDAL.SelectBy(u => getHuDetailId.Contains(u.Id));
                    if (huDetailList1.Count == 0)
                    {
                        if (pickList.Where(u => u.DSFLag == 0).Count() > 0)
                        {
                            return "库存不存在，撤销释放失败！";
                        }
                    }
                    //撤销SN状态
                    if (SerialNumberInToOutRollBack(whCode, loadId, userName) != "Y")
                        return "撤销释放删除SN异常，请重试！";

                    List<HuDetail> checkHuList = new List<HuDetail>();

                    //撤销备货任务
                    foreach (var item in pickList)
                    {
                        List<HuDetail> huDetailList = huDetailList1.Where(u => u.HuId == item.HuId && u.WhCode == item.WhCode && u.Id == item.HuDetailId).ToList();

                        //List<HuDetail> huDetailList = idal.IHuDetailDAL.SelectBy(u => u.Id == item.HuDetailId);

                        if (huDetailList.Count == 0)
                        {
                            result = "库存对应托盘：" + item.HuId + "不存在，撤销释放失败！";
                            break;
                        }
                        HuDetail huDetail = huDetailList.First();
                        if (checkHuList.Where(u => u.Id == huDetail.Id).Count() > 0)
                        {
                            huDetail.PlanQty = checkHuList.Where(u => u.Id == huDetail.Id).First().PlanQty;
                        }
                        if (huDetail.PlanQty < 0)
                        {
                            result = "撤销托盘：" + item.HuId + "数量异常，撤销释放失败！";
                            break;
                        }

                        TranLog tl = new TranLog();
                        tl.TranType = "31";
                        tl.Description = "撤销释放";
                        tl.TranDate = DateTime.Now;
                        tl.TranUser = userName;
                        tl.WhCode = whCode;
                        tl.ClientCode = huDetail.ClientCode;
                        tl.SoNumber = huDetail.SoNumber;
                        tl.CustomerPoNumber = huDetail.CustomerPoNumber;
                        tl.AltItemNumber = huDetail.AltItemNumber;
                        tl.ItemId = huDetail.ItemId;
                        tl.UnitID = huDetail.UnitId;
                        tl.UnitName = huDetail.UnitName;
                        tl.ReceiptId = huDetail.ReceiptId;
                        tl.ReceiptDate = huDetail.ReceiptDate;
                        tl.TranQty = huDetail.PlanQty;
                        tl.HuId = huDetail.HuId;
                        tl.Length = huDetail.Length;
                        tl.Width = huDetail.Width;
                        tl.Height = huDetail.Height;
                        tl.Weight = huDetail.Weight;
                        tl.LotNumber1 = huDetail.LotNumber1;
                        tl.LotNumber2 = huDetail.LotNumber2;
                        tl.LotDate = huDetail.LotDate;
                        tl.LoadId = item.LoadId;
                        tl.Remark = "锁定数量-" + item.Qty;
                        tl.TranQty2 = huDetail.PlanQty - item.Qty;
                        tranLogList.Add(tl);

                        if (checkHuList.Where(u => u.Id == huDetail.Id).Count() == 0)
                        {
                            HuDetail newHu = new HuDetail();
                            newHu.Id = huDetail.Id;
                            newHu.PlanQty = huDetail.PlanQty - item.Qty;
                            checkHuList.Add(newHu);

                            huDetail.PlanQty = huDetail.PlanQty - item.Qty;

                        }
                        else
                        {
                            HuDetail oldHu = checkHuList.Where(u => u.Id == huDetail.Id).First();
                            checkHuList.Remove(oldHu);

                            HuDetail newHu = new HuDetail();
                            newHu.Id = huDetail.Id;
                            newHu.PlanQty = oldHu.PlanQty - item.Qty;
                            checkHuList.Add(newHu);

                            huDetail.PlanQty = oldHu.PlanQty - item.Qty;
                        }
                    }
                    if (result != "")
                    {
                        return result;
                    }

                    //执行调整库存
                    foreach (var item in checkHuList)
                    {
                        idal.IHuDetailDAL.UpdateByExtended(u => u.Id == item.Id, t => new HuDetail { PlanQty = item.PlanQty, UpdateUser = userName, UpdateDate = DateTime.Now });
                    }

                    //删除释放
                    idal.IPickTaskDetailDAL.DeleteByExtended(u => u.WhCode == whCode && u.LoadId == loadId && u.DSFLag != 1);

                    //保存日志
                    idal.ITranLogDAL.Add(tranLogList);
                    //撤销直装
                    idal.IPickTaskDetailDAL.DeleteByExtended(u => u.WhCode == whCode && u.LoadId == loadId && u.DSFLag == 1);

                    //撤销分拣任务
                    idal.ISortTaskDAL.DeleteByExtended(u => u.LoadId == loadId && u.WhCode == whCode);
                    idal.ISortTaskDetailDAL.DeleteByExtended(u => u.LoadId == loadId && u.WhCode == whCode);


                    //撤销已保存的出货数量
                    loadMaster.SumCBM = 0;
                    loadMaster.SumQty = 0;
                    loadMaster.DSSumQty = 0;
                    loadMaster.SumWeight = 0;
                    loadMaster.EchQty = 0;
                    loadMaster.Status0 = "U";   //释放状态
                    loadMaster.ReleaseDate = null;   //释放时间
                    loadMaster.UpdateUser = userName;
                    loadMaster.UpdateDate = DateTime.Now;
                    idal.ILoadMasterDAL.UpdateBy(loadMaster, u => u.Id == loadMaster.Id, new string[] { "Status0", "ReleaseDate", "UpdateUser", "UpdateDate", "SumCBM", "SumQty", "DSSumQty", "SumWeight", "EchQty" });

                    //修改OutOrder状态
                    List<OutBoundOrderResult> list = (from a in idal.ILoadDetailDAL.SelectAll()
                                                      where a.LoadMasterId == loadMaster.Id
                                                      select new OutBoundOrderResult
                                                      {
                                                          Id = (Int32)a.OutBoundOrderId
                                                      }).ToList();

                    int[] outBoundOrderIdArr = (from a in list
                                                select a.Id).ToList().Distinct().ToArray();

                    List<OutBoundOrder> getList1 = idal.IOutBoundOrderDAL.SelectBy(u => outBoundOrderIdArr.Contains(u.Id));

                    List<TranLog> tranAddList = new List<TranLog>();
                    foreach (var item in list)
                    {
                        if (getList1.Where(u => u.Id == item.Id).Count() == 0)
                        {
                            continue;
                        }

                        OutBoundOrder entity = getList1.Where(u => u.Id == item.Id).First();
                        if (entity.StatusId > 0)
                        {
                            FlowHelper flowHelper = new FlowHelper(entity, "OutBound");
                            FlowDetail flowDetail = flowHelper.GetPreviousFlowDetail();
                            if (flowDetail != null && flowDetail.StatusId != 0)
                            {
                                string remark1 = "原状态：" + entity.StatusId + entity.StatusName;

                                entity.NowProcessId = flowDetail.FlowRuleId;
                                entity.StatusId = flowDetail.StatusId;
                                entity.StatusName = flowDetail.StatusName;
                                entity.UpdateUser = userName;
                                entity.UpdateDate = DateTime.Now;

                                //更新订单状态，插入日志
                                TranLog tlorder = new TranLog();
                                tlorder.TranType = "32";
                                tlorder.Description = "更新订单状态";
                                tlorder.TranDate = DateTime.Now;
                                tlorder.TranUser = userName;
                                tlorder.WhCode = whCode;
                                tlorder.LoadId = loadMaster.LoadId;
                                tlorder.CustomerOutPoNumber = entity.CustomerOutPoNumber;
                                tlorder.OutPoNumber = entity.OutPoNumber;
                                tlorder.Remark = remark1 + "变更为：" + entity.StatusId + entity.StatusName;
                                tranAddList.Add(tlorder);

                            }
                            else
                            {
                                return "错误！获取订单状态有误！";
                            }
                        }
                        else
                        {
                            InterceptManager intercept = new InterceptManager();
                            intercept.UpdateOutBoundOrderStatus(userName, "已拦截已处理", entity);
                            idal.ILoadDetailDAL.DeleteByExtended(u => u.OutBoundOrderId == entity.Id);
                        }
                    }
                    #endregion

                    idal.ITranLogDAL.Add(tranAddList);
                    idal.IPickTaskDetailDAL.SaveChanges();
                    trans.Complete();
                    return "Y";

                }
                catch
                {
                    trans.Dispose();//出现异常，事务手动释放
                    return "撤销释放异常，请重试！";
                }
            }
        }



        //释放Load
        //最后释放完成时 需要同时创建分拣任务
        public string CheckReleaseLoad(string loadId, string whCode, string userName, string getType)
        {
            if (loadId == "" || whCode == "" || userName == "" || loadId == null || whCode == null || userName == null)
            {
                return "数据有误，请重新操作！";
            }
            lock (o)
            {
                using (TransactionScope trans = new TransactionScope())
                {
                    try
                    {
                        #region 释放优化

                        List<LoadMaster> loadMasterList = idal.ILoadMasterDAL.SelectBy(u => u.WhCode == whCode && u.LoadId == loadId);
                        if (loadMasterList.Count == 0)
                        {
                            return "当前Load：" + loadId + "有误，请检查！";
                        }

                        LoadMaster loadMaster = loadMasterList.First();
                        if (loadMaster.ShipMode == "集装箱")
                        {
                            List<LoadContainerExtend> LoadContainerExtendList = idal.ILoadContainerExtendDAL.SelectBy(u => u.LoadId == loadId && u.WhCode == whCode);

                            if (LoadContainerExtendList.Count == 0)
                            {
                                return "当前Load：" + loadId + "未维护箱封号信息！";
                            }
                            else
                            {
                                LoadContainerExtend loadContainerExtend = LoadContainerExtendList.First();
                                if ((loadContainerExtend.ContainerNumber == null ? "" : loadContainerExtend.ContainerNumber) == "" || (loadContainerExtend.SealNumber == null ? "" : loadContainerExtend.SealNumber) == "")
                                {
                                    return "当前Load：" + loadId + "箱封号信息有误，请重新查询！";
                                }
                            }
                        }

                        List<LoadDetail> loadDetailList = idal.ILoadDetailDAL.SelectBy(u => u.LoadMasterId == loadMaster.Id);
                        if (loadDetailList.Count == 0)
                        {
                            return "当前Load：" + loadId + "未添加明细，请检查！";
                        }
                        if (loadMaster.Status0 == "C")
                        {
                            return "Load:" + loadMaster.LoadId + "状态有误，请重新查询！";
                        }

                        List<OutBoundOrder> getOutBoundOrderList = new List<OutBoundOrder>();
                        string clientCodeArr = "";
                        int clientCodeQty = 0;
                        //验证Load下的订单类型是否一致，不一致提示异常
                        if (1 == 1)
                        {
                            List<OutBoundOrderResult> checklist = (from a in idal.ILoadDetailDAL.SelectAll()
                                                                   where a.LoadMasterId == loadMaster.Id
                                                                   select new OutBoundOrderResult
                                                                   {
                                                                       Id = (Int32)a.OutBoundOrderId
                                                                   }).ToList();

                            int[] getOutBoundOrderIdArr = (from a in checklist
                                                           select a.Id).ToList().Distinct().ToArray();

                            getOutBoundOrderList = idal.IOutBoundOrderDAL.SelectBy(u => getOutBoundOrderIdArr.Contains(u.Id));

                            string[] getClientCodeArr = (from a in getOutBoundOrderList
                                                         select a.ClientCode).ToList().Distinct().ToArray();

                            foreach (var item in getClientCodeArr)
                            {
                                clientCodeArr += item + ",";
                            }

                            clientCodeArr = clientCodeArr.Substring(0, clientCodeArr.Length - 1);
                            clientCodeQty = getClientCodeArr.Count();

                            string[] checkOrderType = (from a in getOutBoundOrderList
                                                       where (a.OrderType ?? "") != ""
                                                       select (a.OrderType ?? "")).ToList().Distinct().ToArray();

                            if (checkOrderType.Count() > 1)
                            {
                                return "Load:" + loadMaster.LoadId + "中订单存在多种类型无法释放至一个Load中！";
                            }
                        }


                        //得到当前LOAD所选流程
                        List<FlowDetail> flowDetailList = (from a in idal.IFlowDetailDAL.SelectAll()
                                                           where a.FlowHeadId == loadMaster.ProcessId
                                                           select a).ToList();
                        if (flowDetailList.Where(u => u.Type == "ReleaseType").Count() == 0)
                        {
                            return "Load:" + loadMaster.LoadId + "没有选择释放方式！";
                        }
                        if (flowDetailList.Where(u => u.Type == "Release").Count() == 0)
                        {
                            return "Load:" + loadMaster.LoadId + "没有选择释放条件！";
                        }

                        #region bosch释放定制部分 现已注释

                        ////如果是博士 边备边包流程，需要验证 收件信息
                        //if (flowDetailList.Where(u => u.Type == "Picking" && u.Mark == "5").Count() > 0)
                        //{
                        //    //1.得到Load下所有出库订单
                        //    List<OutBoundOrder> getOutBoundOrderList = (from a in idal.IOutBoundOrderDAL.SelectAll()
                        //                                                join b in idal.ILoadDetailDAL.SelectAll()
                        //                                                on a.Id equals b.OutBoundOrderId
                        //                                                where b.LoadMasterId == loadMaster.Id
                        //                                                select a).ToList();

                        //    //2.取第一个订单的地址 与其它地址比较，如果存在不一致的收件信息 就错误提示
                        //    OutBoundOrder firstOutbound = getOutBoundOrderList.First();
                        //    if (getOutBoundOrderList.Where(u => u.buy_name != firstOutbound.buy_name || u.buy_company != firstOutbound.buy_company || u.address != firstOutbound.address).Count() > 0)
                        //    {
                        //        return "该出货流程对应的Load收件人信息必须一致，请删除不一致的订单！";
                        //    }
                        //}

                        #endregion

                        //验证Load是全部直装 还是部分直装
                        List<DSReleaseLoad> checkDSCount = (from a in idal.ILoadMasterDAL.SelectAll()
                                                            join b in idal.ILoadDetailDAL.SelectAll() on new { Id = a.Id } equals new { Id = (Int32)b.LoadMasterId } into b_join
                                                            from b in b_join.DefaultIfEmpty()
                                                            join c in idal.IOutBoundOrderDAL.SelectAll() on new { OutBoundOrderId = (Int32)b.OutBoundOrderId } equals new { OutBoundOrderId = c.Id } into c_join
                                                            from c in c_join.DefaultIfEmpty()
                                                            where
                                                              c.DSFLag != 1 && a.Id == loadMaster.Id && a.WhCode == whCode
                                                            select new DSReleaseLoad
                                                            {
                                                                ClientId = c.Id
                                                            }).ToList();

                        //删除释放异常明细列表，每次释放强制删除一次
                        idal.IReleaseLoadDetailDAL.DeleteBy(u => u.WhCode == whCode && u.LoadId == loadId);

                        //验证Load 如果全部是直装 
                        if (checkDSCount.Count() == 0)
                        {
                            #region Load是直装Load
                            //Load全部直装 直接添加直装备货任务
                            List<DSReleaseLoad> DSOutBoundList = (from a in idal.IOutBoundOrderDetailDAL.SelectAll()
                                                                  join b in idal.IOutBoundOrderDAL.SelectAll() on new { OutBoundOrderId = a.OutBoundOrderId } equals new { OutBoundOrderId = b.Id }
                                                                  join c in idal.ILoadDetailDAL.SelectAll() on new { OutBoundOrderId = a.OutBoundOrderId } equals new { OutBoundOrderId = (Int32)c.OutBoundOrderId }
                                                                  join d in idal.ILoadMasterDAL.SelectAll() on new { LoadMasterId = (Int32)c.LoadMasterId } equals new { LoadMasterId = d.Id }
                                                                  where d.LoadId == loadId && d.WhCode == whCode && a.DSFLag == 1
                                                                  group new { a, d, b } by new
                                                                  {
                                                                      a.WhCode,
                                                                      d.LoadId,
                                                                      b.ClientId,
                                                                      b.ClientCode,
                                                                      a.SoNumber,
                                                                      a.CustomerPoNumber,
                                                                      a.AltItemNumber,
                                                                      a.ItemId,
                                                                      a.UnitId,
                                                                      a.UnitName,
                                                                      a.LotNumber1,
                                                                      a.LotNumber2,
                                                                      a.LotDate,
                                                                      a.Length,
                                                                      a.Weight,
                                                                      a.Width,
                                                                      a.Height,
                                                                      userName
                                                                  } into g
                                                                  select new DSReleaseLoad
                                                                  {
                                                                      WhCode = g.Key.WhCode,
                                                                      LoadId = g.Key.LoadId,
                                                                      ClientId = g.Key.ClientId,
                                                                      ClientCode = g.Key.ClientCode,
                                                                      SoNumber = g.Key.SoNumber,
                                                                      CustomerPoNumber = g.Key.CustomerPoNumber,
                                                                      AltItemNumber = g.Key.AltItemNumber,
                                                                      ItemId = g.Key.ItemId,
                                                                      UnitId = g.Key.UnitId,
                                                                      UnitName = g.Key.UnitName,
                                                                      Qty = g.Sum(p => p.a.Qty),
                                                                      LotNumber1 = g.Key.LotNumber1,
                                                                      LotNumber2 = g.Key.LotNumber2,
                                                                      LotDate = (DateTime?)g.Key.LotDate,
                                                                      Length = g.Key.Length,
                                                                      Weight = g.Key.Weight,
                                                                      Width = g.Key.Width,
                                                                      Height = g.Key.Height,
                                                                      UserName = g.Key.userName
                                                                  }).ToList();
                            if (DSOutBoundList.Count > 0)
                            {
                                List<PickTaskDetail> addpickTask = new List<PickTaskDetail>();
                                foreach (var item in DSOutBoundList)
                                {
                                    PickTaskDetail detail = new PickTaskDetail();
                                    detail.WhCode = item.WhCode;
                                    detail.LoadId = loadId;
                                    detail.HuDetailId = 0;
                                    detail.HuId = "直装";
                                    detail.ClientCode = item.ClientCode;
                                    detail.Location = item.ClientCode;
                                    detail.SoNumber = item.SoNumber;
                                    detail.CustomerPoNumber = item.CustomerPoNumber;
                                    detail.AltItemNumber = item.AltItemNumber;
                                    detail.ItemId = (Int32)item.ItemId;
                                    detail.UnitId = item.UnitId;
                                    detail.UnitName = item.UnitName;
                                    detail.Qty = (Int32)item.Qty;
                                    detail.DSFLag = 1;
                                    detail.PickQty = 0;
                                    detail.Length = (item.Length ?? 0);
                                    detail.Width = (item.Width ?? 0);
                                    detail.Height = (item.Height ?? 0);
                                    detail.Weight = (item.Weight ?? 0);
                                    detail.LotNumber1 = item.LotNumber1;
                                    detail.LotNumber2 = item.LotNumber2;
                                    detail.LotDate = item.LotDate;
                                    detail.Status = "U";
                                    detail.Status1 = "U";
                                    detail.CreateUser = userName;
                                    detail.CreateDate = DateTime.Now;
                                    addpickTask.Add(detail);
                                }
                                idal.IPickTaskDetailDAL.Add(addpickTask);
                            }
                            #endregion
                        }
                        else
                        {
                            #region Load部分直装 验证非直装库存 添加直装备货任务

                            //得到释放的条件 是先进先出 还是后进先出等
                            string mark = flowDetailList.Where(u => u.Type == "Release").First().Mark;
                            if (mark == "1" || mark == "2" || mark == "3" || mark == "4" || mark == "5" || mark == "6" || mark == "7" || mark == "9" || mark == "11" || mark == "12")
                            {
                                #region 根据释放条件条件验证库存

                                //1. 验证库存是否满足所需
                                CheckReleaseLoadResult checkReleaseLoadResult = CheckInventory(mark, loadId, whCode, userName);
                                if (checkReleaseLoadResult.Result != "")
                                {
                                    //验证 释放异常时是否新增了释放明细，如果新增了释放明细 必须Complete
                                    if (checkReleaseLoadResult.CheckStatusResult == "Y")
                                    {
                                        trans.Complete();
                                    }
                                    return checkReleaseLoadResult.Result;
                                }

                                List<ReleaseLoad> entity = checkReleaseLoadResult.ReleaseLoadList;
                                List<HuDetailResult> List = checkReleaseLoadResult.HuDetailResultList;

                                if (List.Count == 0)
                                {
                                    return "库存检索列表数量为0，释放异常！";
                                }

                                //2. 查看当前是按Load释放 还是按订单释放
                                List<ReleaseLoad> forEntity = null;     //插入备货任务表时  需要使用的实体
                                string ReleaseType = flowDetailList.Where(u => u.Type == "ReleaseType").First().Mark;
                                //by Load释放
                                if (ReleaseType == "1")
                                {
                                    forEntity = entity;
                                }
                                else if (ReleaseType == "2")  //by 订单释放
                                {
                                    forEntity = (from a in idal.IOutBoundOrderDetailDAL.SelectAll()
                                                 join b in idal.IOutBoundOrderDAL.SelectAll() on new { OutBoundOrderId = a.OutBoundOrderId } equals new { OutBoundOrderId = b.Id }
                                                 join c in idal.ILoadDetailDAL.SelectAll() on new { OutBoundOrderId = a.OutBoundOrderId } equals new { OutBoundOrderId = (Int32)c.OutBoundOrderId }
                                                 join d in idal.ILoadMasterDAL.SelectAll() on new { LoadMasterId = (Int32)c.LoadMasterId } equals new { LoadMasterId = d.Id }
                                                 where d.LoadId == loadId && d.WhCode == whCode && a.DSFLag != 1
                                                 select new ReleaseLoad
                                                 {
                                                     WhCode = a.WhCode,
                                                     LoadId = d.LoadId,
                                                     ClientId = b.ClientId,
                                                     ClientCode = b.ClientCode,
                                                     OutBoundOrderId = a.OutBoundOrderId,
                                                     SoNumber = a.SoNumber,
                                                     CustomerPoNumber = a.CustomerPoNumber,
                                                     AltItemNumber = a.AltItemNumber,
                                                     ItemId = a.ItemId,
                                                     UnitId = a.UnitId,
                                                     UnitName = a.UnitName,
                                                     Qty = a.Qty,
                                                     LotNumber1 = a.LotNumber1 ?? "",
                                                     LotNumber2 = a.LotNumber2 ?? "",
                                                     LotDate = a.LotDate,
                                                     Length = a.Length,
                                                     Weight = a.Weight,
                                                     Width = a.Width,
                                                     Height = a.Height,
                                                     UserName = userName,
                                                     Sequence = a.Sequence
                                                 }).ToList();
                                }

                                //by订单List 
                                List<HuDetailResult> orderList = new List<HuDetailResult>();

                                //by订单验证同一托盘是否被重复释放
                                List<PickTaskDetail> checkHuDetailListByOrder = new List<PickTaskDetail>();

                                //日志记录
                                List<TranLog> tranLogList = new List<TranLog>();

                                //得到客户释放规则 //1 按默认流程释放 2开启优先拣货区 3开启优先高位区
                                ReleaseLoad getFirstReLoad = forEntity.First();
                                WhClient getClient = idal.IWhClientDAL.SelectBy(u => u.Id == getFirstReLoad.ClientId).First();

                                List<HuDetail> huDetailCheck = new List<HuDetail>();

                                //3. 开始插入 备货任务表
                                foreach (var item in forEntity)
                                {
                                    //-----------------------先进先出、后进先出排序规则----------------

                                    #region 库存排序
                                    List<HuDetailResult> ListWhere = new List<HuDetailResult>();
                                    //等于1,9 为先进先出  
                                    //等于2 为后进先出
                                    //等于3 为先进先出无视SOPO
                                    //等于4 为后进先出无视SOPO
                                    //等于5 为后进先出无视SO
                                    //等于6 为后进先出无视SO
                                    //等于7 为先进先出无视SOPO按Lodate倒序
                                    //等于10 为先进先出无视SOPOLotData优先LotData
                                    //等于11 为定制流程 无视SOPOLotNumberLotDate指定库位ROC1
                                    //等于12 为先进先出无视SOPO按Lodate升序

                                    if (mark == "1" || mark == "9")
                                    {
                                        ListWhere = List.Where(u => u.SoNumber == item.SoNumber && u.CustomerPoNumber == item.CustomerPoNumber && u.AltItemNumber == item.AltItemNumber && u.ItemId == item.ItemId && u.UnitName == item.UnitName && u.UnitId == item.UnitId && (u.LotNumber1 ?? "") == (item.LotNumber1 ?? "") && (u.LotNumber2 ?? "") == (item.LotNumber2 ?? "")).OrderBy(u => u.ReceiptDate).ToList();
                                    }
                                    else if (mark == "2")
                                    {
                                        ListWhere = List.Where(u => u.SoNumber == item.SoNumber && u.CustomerPoNumber == item.CustomerPoNumber && u.AltItemNumber == item.AltItemNumber && u.ItemId == item.ItemId && u.UnitName == item.UnitName && u.UnitId == item.UnitId && (u.LotNumber1 ?? "") == (item.LotNumber1 ?? "") && (u.LotNumber2 ?? "") == (item.LotNumber2 ?? "")).OrderByDescending(u => u.ReceiptDate).ToList();
                                    }
                                    else if (mark == "3")
                                    {
                                        ListWhere = List.Where(u => u.AltItemNumber == item.AltItemNumber && u.ItemId == item.ItemId && u.UnitName == item.UnitName && u.UnitId == item.UnitId && (u.LotNumber1 ?? "") == (item.LotNumber1 ?? "") && (u.LotNumber2 ?? "") == (item.LotNumber2 ?? "")).OrderBy(u => u.ReceiptDate).ThenBy(u => u.Location).ThenBy(u => u.HuId).ToList();
                                    }
                                    else if (mark == "4")
                                    {
                                        ListWhere = List.Where(u => u.AltItemNumber == item.AltItemNumber && u.ItemId == item.ItemId && u.UnitName == item.UnitName && u.UnitId == item.UnitId && (u.LotNumber1 ?? "") == (item.LotNumber1 ?? "") && (u.LotNumber2 ?? "") == (item.LotNumber2 ?? "")).OrderByDescending(u => u.ReceiptDate).ToList();
                                    }
                                    else if (mark == "5")
                                    {
                                        ListWhere = List.Where(u => u.CustomerPoNumber == item.CustomerPoNumber && u.AltItemNumber == item.AltItemNumber && u.ItemId == item.ItemId && u.UnitName == item.UnitName && u.UnitId == item.UnitId && (u.LotNumber1 ?? "") == (item.LotNumber1 ?? "") && (u.LotNumber2 ?? "") == (item.LotNumber2 ?? "")).OrderBy(u => u.ReceiptDate).ToList();
                                    }
                                    else if (mark == "6")
                                    {
                                        ListWhere = List.Where(u => u.CustomerPoNumber == item.CustomerPoNumber && u.AltItemNumber == item.AltItemNumber && u.ItemId == item.ItemId && u.UnitName == item.UnitName && u.UnitId == item.UnitId && (u.LotNumber1 ?? "") == (item.LotNumber1 ?? "") && (u.LotNumber2 ?? "") == (item.LotNumber2 ?? "")).OrderByDescending(u => u.ReceiptDate).ToList();
                                    }
                                    else if (mark == "7")
                                    {
                                        ListWhere = List.Where(u => u.AltItemNumber == item.AltItemNumber && u.ItemId == item.ItemId && u.UnitName == item.UnitName && u.UnitId == item.UnitId && (u.LotNumber1 ?? "") == (item.LotNumber1 ?? "") && (u.LotNumber2 ?? "") == (item.LotNumber2 ?? "")).OrderByDescending(u => u.LotDate).ThenBy(u => u.LocationTypeDetailId).ThenBy(u => u.ReceiptDate).ThenBy(u => u.Location).ThenBy(u => u.HuId).ToList();
                                    }
                                    else if (mark == "10")
                                    {
                                        ListWhere = List.Where(u => u.AltItemNumber == item.AltItemNumber && u.ItemId == item.ItemId && u.UnitName == item.UnitName && u.UnitId == item.UnitId && (u.LotNumber1 ?? "") == (item.LotNumber1 ?? "") && (u.LotNumber2 ?? "") == (item.LotNumber2 ?? "")).OrderBy(u => u.LotDate).ThenBy(u => u.LocationTypeDetailId).ThenBy(u => u.ReceiptDate).ThenBy(u => u.Location).ThenBy(u => u.HuId).ToList();
                                    }
                                    else if (mark == "11")
                                    {
                                        ListWhere = List.Where(u => u.AltItemNumber == item.AltItemNumber && u.ItemId == item.ItemId && u.UnitName == item.UnitName && u.UnitId == item.UnitId).OrderBy(u => u.LotDate).ThenBy(u => u.LotNumber1).ThenBy(u => u.Location).ThenBy(u => u.HuId).ToList();
                                    }
                                    else if (mark == "12")
                                    {
                                        ListWhere = List.Where(u => u.AltItemNumber == item.AltItemNumber && u.ItemId == item.ItemId && u.UnitName == item.UnitName && u.UnitId == item.UnitId && (u.LotNumber1 ?? "") == (item.LotNumber1 ?? "") && (u.LotNumber2 ?? "") == (item.LotNumber2 ?? "")).OrderBy(u => u.LotDate).ThenBy(u => u.LocationTypeDetailId).ThenBy(u => u.ReceiptDate).ThenBy(u => u.Location).ThenBy(u => u.HuId).ToList();
                                    }


                                    //最后检测下客户是否选择了释放排序规则，1：按流程默认2：LotDate升序优先拣货区3：LotDate升序优先高位区4：LotDate倒序优先拣货区5：LotDate倒序优先高位区6：优先拣货区无视LotDate7：优先高位区无视LotDate
                                    if (getClient.ReleaseRule == 2)
                                    {
                                        ListWhere = ListWhere.OrderBy(u => u.LotDate).ThenBy(u => u.LocationTypeDetailId).ThenBy(u => u.ReceiptDate).ThenBy(u => u.Location).ThenBy(u => u.HuId).ToList();
                                    }
                                    else if (getClient.ReleaseRule == 3)
                                    {
                                        ListWhere = ListWhere.OrderBy(u => u.LotDate).ThenByDescending(u => u.LocationTypeDetailId).ThenBy(u => u.ReceiptDate).ThenBy(u => u.Location).ThenBy(u => u.HuId).ToList();
                                    }
                                    else if (getClient.ReleaseRule == 4)
                                    {
                                        ListWhere = ListWhere.OrderByDescending(u => u.LotDate).ThenByDescending(u => u.LocationTypeDetailId).ThenBy(u => u.ReceiptDate).ThenBy(u => u.Location).ThenBy(u => u.HuId).ToList();
                                    }
                                    else if (getClient.ReleaseRule == 5)
                                    {
                                        ListWhere = ListWhere.OrderByDescending(u => u.LotDate).ThenByDescending(u => u.LocationTypeDetailId).ThenBy(u => u.ReceiptDate).ThenBy(u => u.Location).ThenBy(u => u.HuId).ToList();
                                    }
                                    else if (getClient.ReleaseRule == 6)
                                    {
                                        ListWhere = ListWhere.OrderBy(u => u.LocationTypeDetailId).ThenByDescending(u => u.LotDate).ThenBy(u => u.ReceiptDate).ThenBy(u => u.Location).ThenBy(u => u.HuId).ToList();
                                    }
                                    else if (getClient.ReleaseRule == 7)
                                    {
                                        ListWhere = ListWhere.OrderByDescending(u => u.LocationTypeDetailId).ThenByDescending(u => u.LotDate).ThenBy(u => u.ReceiptDate).ThenBy(u => u.Location).ThenBy(u => u.HuId).ToList();
                                    }

                                    #endregion  结束排序

                                    ///--------开始插入备货任务表--------------------
                                    int qtyResult = (Int32)item.Qty;  //得到出库订单的数量

                                    foreach (var sqlDetail in ListWhere)    //得到经过筛选后的库存
                                    {
                                        if (qtyResult <= 0)
                                        {
                                            break;
                                        }
                                        else
                                        {
                                            int? sqlDetailQty = sqlDetail.Qty;
                                            if (checkHuDetailListByOrder.Where(u => u.HuDetailId == sqlDetail.Id).Count() > 0)
                                            {
                                                int getQty = checkHuDetailListByOrder.Where(u => u.HuDetailId == sqlDetail.Id).Sum(u => u.Qty);
                                                sqlDetailQty = sqlDetail.Qty - getQty;
                                            }
                                            if (sqlDetailQty <= 0)
                                            {
                                                continue;
                                            }

                                            PickTaskDetail detail = new PickTaskDetail();
                                            if (ReleaseType == "2")  //by 订单释放
                                                detail.OutBoundOrderId = item.OutBoundOrderId;

                                            detail.WhCode = sqlDetail.WhCode;
                                            detail.LoadId = loadId;
                                            detail.HuDetailId = sqlDetail.Id;
                                            detail.ClientCode = sqlDetail.ClientCode;
                                            detail.HuId = sqlDetail.HuId;
                                            detail.Location = sqlDetail.Location;
                                            detail.SoNumber = sqlDetail.SoNumber;
                                            detail.CustomerPoNumber = sqlDetail.CustomerPoNumber;
                                            detail.AltItemNumber = sqlDetail.AltItemNumber;
                                            detail.ItemId = (Int32)sqlDetail.ItemId;
                                            detail.UnitId = sqlDetail.UnitId;
                                            detail.UnitName = sqlDetail.UnitName;
                                            if (qtyResult >= sqlDetailQty - (sqlDetail.PlanQty ?? 0))
                                            {
                                                detail.Qty = (Int32)sqlDetailQty - (sqlDetail.PlanQty ?? 0);  //释放数量
                                                qtyResult = qtyResult - detail.Qty;    //剩余的未释放数量
                                            }
                                            else
                                            {
                                                detail.Qty = qtyResult;
                                                qtyResult = 0;
                                            }
                                            detail.PickQty = 0;
                                            detail.Length = (sqlDetail.Length ?? 0);
                                            detail.Width = (sqlDetail.Width ?? 0);
                                            detail.Height = (sqlDetail.Height ?? 0);
                                            detail.Weight = (sqlDetail.Weight ?? 0);
                                            detail.LotNumber1 = sqlDetail.LotNumber1;
                                            detail.LotNumber2 = sqlDetail.LotNumber2;
                                            detail.LotDate = sqlDetail.LotDate;
                                            detail.Sequence = item.Sequence;
                                            detail.Status = "U";
                                            detail.Status1 = "U";
                                            detail.CreateUser = userName;
                                            detail.CreateDate = DateTime.Now;
                                            detail.ReceiptDate = sqlDetail.ReceiptDate;
                                            checkHuDetailListByOrder.Add(detail);

                                            //2016年修改  --张雨佳
                                            //by 订单释放修改
                                            //如果多个订单包含同一个款号 因这里没有实时更新 导致只会更新最后一次出库订单的数量
                                            //现增加List累加后 最后执行一次保存就没有问题
                                            if (ReleaseType == "2")
                                            {
                                                if (orderList.Where(u => u.Id == sqlDetail.Id).Count() == 0)
                                                {
                                                    HuDetailResult en = new HuDetailResult();
                                                    en.Id = sqlDetail.Id;
                                                    en.PlanQty = detail.Qty;
                                                    orderList.Add(en);
                                                }
                                                else
                                                {
                                                    HuDetailResult en = orderList.Where(u => u.Id == sqlDetail.Id).First();
                                                    HuDetailResult en1 = new HuDetailResult();
                                                    en1.Id = sqlDetail.Id;
                                                    en1.PlanQty = en.PlanQty + detail.Qty;
                                                    orderList.Add(en1);
                                                    orderList.Remove(en);
                                                }
                                            }
                                            else
                                            {
                                                //更新by Load 情况的 库存数量
                                                TranLog tl = new TranLog();
                                                tl.TranType = "30";
                                                tl.Description = "释放Load";
                                                tl.TranDate = DateTime.Now;
                                                tl.TranUser = userName;
                                                tl.WhCode = whCode;
                                                tl.ClientCode = sqlDetail.ClientCode;
                                                tl.SoNumber = sqlDetail.SoNumber;
                                                tl.CustomerPoNumber = sqlDetail.CustomerPoNumber;
                                                tl.AltItemNumber = sqlDetail.AltItemNumber;
                                                tl.Location = sqlDetail.Location;
                                                tl.ItemId = sqlDetail.ItemId;
                                                tl.UnitID = sqlDetail.UnitId;
                                                tl.UnitName = sqlDetail.UnitName;
                                                tl.ReceiptDate = sqlDetail.ReceiptDate;

                                                tl.HuId = sqlDetail.HuId;
                                                tl.Length = sqlDetail.Length;
                                                tl.Width = sqlDetail.Width;
                                                tl.Height = sqlDetail.Height;
                                                tl.Weight = sqlDetail.Weight;
                                                tl.LotNumber1 = sqlDetail.LotNumber1;
                                                tl.LotNumber2 = sqlDetail.LotNumber2;
                                                tl.LotDate = sqlDetail.LotDate;
                                                tl.LoadId = loadId;
                                                tl.Remark = "锁定数量+" + detail.Qty;

                                                //更新by Load 情况的 库存数量
                                                if (huDetailCheck.Where(u => u.Id == sqlDetail.Id).Count() == 0)
                                                {
                                                    HuDetail huDetail = new HuDetail();
                                                    huDetail.Id = sqlDetail.Id;
                                                    huDetail.PlanQty = (sqlDetail.PlanQty ?? 0) + detail.Qty;
                                                    huDetailCheck.Add(huDetail);

                                                    tl.TranQty = (sqlDetail.PlanQty ?? 0);
                                                    tl.TranQty2 = huDetail.PlanQty;
                                                }
                                                else
                                                {
                                                    HuDetail huDetail = huDetailCheck.Where(u => u.Id == sqlDetail.Id).First();
                                                    huDetailCheck.Remove(huDetail);
                                                    tl.TranQty = (huDetail.PlanQty ?? 0);

                                                    HuDetail newhuDetail = huDetail;
                                                    newhuDetail.PlanQty = newhuDetail.PlanQty + detail.Qty;
                                                    huDetailCheck.Add(newhuDetail);

                                                    tl.TranQty2 = newhuDetail.PlanQty;
                                                }

                                                tranLogList.Add(tl);
                                            }
                                        }
                                    }
                                }

                                //更新by Load 情况的 库存数量
                                if (huDetailCheck.Count > 0)
                                {
                                    foreach (var itemhuDetail in huDetailCheck)
                                    {
                                        itemhuDetail.UpdateUser = userName;
                                        itemhuDetail.UpdateDate = DateTime.Now;
                                        idal.IHuDetailDAL.UpdateBy(itemhuDetail, u => u.Id == itemhuDetail.Id, new string[] { "PlanQty", "UpdateUser", "UpdateDate" });
                                    }
                                }

                                List<PickTaskDetail> PickTaskDetailList = new List<PickTaskDetail>();
                                if (ReleaseType == "2")  //by 订单释放
                                {
                                    foreach (var item in checkHuDetailListByOrder.Where(u => u.Qty > 0))
                                    {
                                        if (PickTaskDetailList.Where(u => u.HuDetailId == item.HuDetailId && u.OutBoundOrderId == item.OutBoundOrderId).Count() == 0)
                                        {
                                            PickTaskDetail detail = new PickTaskDetail();
                                            detail.OutBoundOrderId = item.OutBoundOrderId;
                                            detail.WhCode = item.WhCode;
                                            detail.LoadId = item.LoadId;
                                            detail.HuDetailId = item.HuDetailId;
                                            detail.ClientCode = item.ClientCode;
                                            detail.HuId = item.HuId;
                                            detail.Location = item.Location;
                                            detail.SoNumber = item.SoNumber;
                                            detail.CustomerPoNumber = item.CustomerPoNumber;
                                            detail.AltItemNumber = item.AltItemNumber;
                                            detail.ItemId = item.ItemId;
                                            detail.UnitId = item.UnitId;
                                            detail.UnitName = item.UnitName;
                                            detail.Qty = item.Qty;
                                            detail.PickQty = 0;
                                            detail.Length = (item.Length ?? 0);
                                            detail.Width = (item.Width ?? 0);
                                            detail.Height = (item.Height ?? 0);
                                            detail.Weight = (item.Weight ?? 0);
                                            detail.LotNumber1 = item.LotNumber1;
                                            detail.LotNumber2 = item.LotNumber2;
                                            detail.LotDate = item.LotDate;
                                            detail.Sequence = item.Sequence;
                                            detail.Status = "U";
                                            detail.Status1 = "U";
                                            detail.CreateUser = item.CreateUser;
                                            detail.CreateDate = DateTime.Now;
                                            detail.ReceiptDate = item.ReceiptDate;
                                            PickTaskDetailList.Add(detail);
                                        }
                                        else
                                        {
                                            PickTaskDetail en = PickTaskDetailList.Where(u => u.HuDetailId == item.HuDetailId && u.OutBoundOrderId == item.OutBoundOrderId).First();
                                            PickTaskDetail en1 = en;
                                            en1.Qty = en.Qty + item.Qty;

                                            PickTaskDetailList.Remove(en);
                                            PickTaskDetailList.Add(en1);
                                        }
                                    }
                                    //添加备货任务信息
                                    idal.IPickTaskDetailDAL.Add(PickTaskDetailList.Where(u => u.Qty > 0));
                                }
                                else
                                {
                                    //添加备货任务信息
                                    idal.IPickTaskDetailDAL.Add(checkHuDetailListByOrder.Where(u => u.Qty > 0));
                                }


                                //更新 by订单情况的 库存数量
                                if (orderList != null)
                                {
                                    foreach (var item in orderList)
                                    {
                                        HuDetail huDetail = idal.IHuDetailDAL.SelectBy(u => u.Id == item.Id).First();

                                        TranLog tl = new TranLog();
                                        tl.TranType = "30";
                                        tl.Description = "释放Load";
                                        tl.TranDate = DateTime.Now;
                                        tl.TranUser = userName;
                                        tl.WhCode = whCode;
                                        tl.ClientCode = huDetail.ClientCode;
                                        tl.SoNumber = huDetail.SoNumber;
                                        tl.CustomerPoNumber = huDetail.CustomerPoNumber;
                                        tl.AltItemNumber = huDetail.AltItemNumber;
                                        tl.ItemId = huDetail.ItemId;
                                        tl.UnitID = huDetail.UnitId;
                                        tl.UnitName = huDetail.UnitName;
                                        tl.ReceiptId = huDetail.ReceiptId;
                                        tl.ReceiptDate = huDetail.ReceiptDate;
                                        tl.TranQty = (huDetail.PlanQty ?? 0);
                                        tl.HuId = huDetail.HuId;
                                        tl.Length = huDetail.Length;
                                        tl.Width = huDetail.Width;
                                        tl.Height = huDetail.Height;
                                        tl.Weight = huDetail.Weight;
                                        tl.LotNumber1 = huDetail.LotNumber1;
                                        tl.LotNumber2 = huDetail.LotNumber2;
                                        tl.LotDate = huDetail.LotDate;
                                        tl.LoadId = loadId;
                                        tl.Remark = "锁定数量+" + item.PlanQty;

                                        huDetail.PlanQty = item.PlanQty + (huDetail.PlanQty ?? 0);
                                        huDetail.UpdateUser = userName;
                                        huDetail.UpdateDate = DateTime.Now;
                                        idal.IHuDetailDAL.UpdateBy(huDetail, u => u.Id == item.Id, new string[] { "PlanQty", "UpdateUser", "UpdateDate" });

                                        tl.TranQty2 = huDetail.PlanQty;
                                        tranLogList.Add(tl);
                                    }
                                }
                                //保存释放日志
                                idal.ITranLogDAL.Add(tranLogList);

                                //3.1验证Load是否有直装部分 有直装 添加直装备货任务
                                List<DSReleaseLoad> DSOutBoundList = (from a in idal.IOutBoundOrderDetailDAL.SelectAll()
                                                                      join b in idal.IOutBoundOrderDAL.SelectAll() on new { OutBoundOrderId = a.OutBoundOrderId } equals new { OutBoundOrderId = b.Id }
                                                                      join c in idal.ILoadDetailDAL.SelectAll() on new { OutBoundOrderId = a.OutBoundOrderId } equals new { OutBoundOrderId = (Int32)c.OutBoundOrderId }
                                                                      join d in idal.ILoadMasterDAL.SelectAll() on new { LoadMasterId = (Int32)c.LoadMasterId } equals new { LoadMasterId = d.Id }
                                                                      where d.LoadId == loadId && d.WhCode == whCode && a.DSFLag == 1
                                                                      group new { a, d, b } by new
                                                                      {
                                                                          a.WhCode,
                                                                          d.LoadId,
                                                                          b.ClientId,
                                                                          b.ClientCode,
                                                                          a.SoNumber,
                                                                          a.CustomerPoNumber,
                                                                          a.AltItemNumber,
                                                                          a.ItemId,
                                                                          a.UnitId,
                                                                          a.UnitName,
                                                                          a.LotNumber1,
                                                                          a.LotNumber2,
                                                                          a.LotDate,
                                                                          a.Length,
                                                                          a.Weight,
                                                                          a.Width,
                                                                          a.Height,
                                                                          userName
                                                                      } into g
                                                                      select new DSReleaseLoad
                                                                      {
                                                                          WhCode = g.Key.WhCode,
                                                                          LoadId = g.Key.LoadId,
                                                                          ClientId = g.Key.ClientId,
                                                                          ClientCode = g.Key.ClientCode,
                                                                          SoNumber = g.Key.SoNumber,
                                                                          CustomerPoNumber = g.Key.CustomerPoNumber,
                                                                          AltItemNumber = g.Key.AltItemNumber,
                                                                          ItemId = g.Key.ItemId,
                                                                          UnitId = g.Key.UnitId,
                                                                          UnitName = g.Key.UnitName,
                                                                          Qty = g.Sum(p => p.a.Qty),
                                                                          LotNumber1 = g.Key.LotNumber1 ?? "",
                                                                          LotNumber2 = g.Key.LotNumber2 ?? "",
                                                                          LotDate = (DateTime?)g.Key.LotDate,
                                                                          Length = g.Key.Length,
                                                                          Weight = g.Key.Weight,
                                                                          Width = g.Key.Width,
                                                                          Height = g.Key.Height,
                                                                          UserName = g.Key.userName
                                                                      }).ToList();
                                if (DSOutBoundList.Count > 0)
                                {
                                    List<PickTaskDetail> addpickTask = new List<PickTaskDetail>();
                                    foreach (var item in DSOutBoundList)
                                    {
                                        PickTaskDetail detail = new PickTaskDetail();
                                        detail.WhCode = item.WhCode;
                                        detail.LoadId = loadId;
                                        detail.HuDetailId = 0;
                                        detail.HuId = "直装";
                                        detail.ClientCode = item.ClientCode;
                                        detail.Location = item.ClientCode;
                                        detail.SoNumber = item.SoNumber;
                                        detail.CustomerPoNumber = item.CustomerPoNumber;
                                        detail.AltItemNumber = item.AltItemNumber;
                                        detail.ItemId = (Int32)item.ItemId;
                                        detail.UnitId = item.UnitId;
                                        detail.UnitName = item.UnitName;
                                        detail.Qty = (Int32)item.Qty;
                                        detail.DSFLag = 1;

                                        detail.PickQty = 0;
                                        detail.Length = (item.Length ?? 0);
                                        detail.Width = (item.Width ?? 0);
                                        detail.Height = (item.Height ?? 0);
                                        detail.Weight = (item.Weight ?? 0);
                                        detail.LotNumber1 = item.LotNumber1;
                                        detail.LotNumber2 = item.LotNumber2;
                                        detail.LotDate = item.LotDate;
                                        detail.Status = "U";
                                        detail.Status1 = "U";
                                        detail.CreateUser = userName;
                                        detail.CreateDate = DateTime.Now;
                                        addpickTask.Add(detail);
                                    }
                                    idal.IPickTaskDetailDAL.Add(addpickTask);
                                }

                                #endregion
                            }
                            else if (mark == "8")
                            {
                                #region 根据订单自动验证及生成备货任务

                                //1. 验证库存是否满足所需
                                CheckReleaseLoadResult checkReleaseLoadResult = CheckInventory1(mark, loadId, whCode, userName);
                                if (checkReleaseLoadResult.Result != "")
                                {
                                    return checkReleaseLoadResult.Result;
                                }

                                List<ReleaseLoad> entity = checkReleaseLoadResult.ReleaseLoadList3;
                                List<HuDetailResult> List = checkReleaseLoadResult.HuDetailResultList3;

                                List<ReleaseLoad> entity2 = checkReleaseLoadResult.ReleaseLoadList2;
                                List<HuDetailResult> List2 = checkReleaseLoadResult.HuDetailResultList2;

                                List<ReleaseLoad> entity4 = checkReleaseLoadResult.ReleaseLoadList4;
                                List<HuDetailResult> List4 = checkReleaseLoadResult.HuDetailResultList4;

                                List<ReleaseLoad> entity3 = checkReleaseLoadResult.ReleaseLoadList;
                                List<HuDetailResult> List3 = checkReleaseLoadResult.HuDetailResultList;

                                if (List.Count == 0 && List2.Count == 0 && List3.Count == 0 && List4.Count == 0)
                                {
                                    return "Load订单明细未找到有效库存，请检查！";
                                }

                                string markCount = "SOPO";
                                //2. 查看当前是按Load释放 还是按订单释放
                                List<ReleaseLoad> forEntity = null;     //插入备货任务表时  需要使用的实体
                                string ReleaseType = flowDetailList.Where(u => u.Type == "ReleaseType").First().Mark;
                                //by Load释放
                                if (ReleaseType == "1")
                                {
                                    forEntity = entity;
                                }
                                else if (ReleaseType == "2")  //by 订单释放
                                {
                                    forEntity = (from a in idal.IOutBoundOrderDetailDAL.SelectAll()
                                                 join b in idal.IOutBoundOrderDAL.SelectAll() on new { OutBoundOrderId = a.OutBoundOrderId } equals new { OutBoundOrderId = b.Id }
                                                 join c in idal.ILoadDetailDAL.SelectAll() on new { OutBoundOrderId = a.OutBoundOrderId } equals new { OutBoundOrderId = (Int32)c.OutBoundOrderId }
                                                 join d in idal.ILoadMasterDAL.SelectAll() on new { LoadMasterId = (Int32)c.LoadMasterId } equals new { LoadMasterId = d.Id }
                                                 where d.LoadId == loadId && d.WhCode == whCode && a.DSFLag != 1 && (a.SoNumber ?? "") != "" && (a.CustomerPoNumber ?? "") != ""
                                                 select new ReleaseLoad
                                                 {
                                                     WhCode = a.WhCode,
                                                     LoadId = d.LoadId,
                                                     ClientId = b.ClientId,
                                                     ClientCode = b.ClientCode,
                                                     OutBoundOrderId = a.OutBoundOrderId,
                                                     SoNumber = a.SoNumber,
                                                     CustomerPoNumber = a.CustomerPoNumber,
                                                     AltItemNumber = a.AltItemNumber,
                                                     ItemId = a.ItemId,
                                                     UnitId = a.UnitId,
                                                     UnitName = a.UnitName,
                                                     Qty = a.Qty,
                                                     LotNumber1 = a.LotNumber1,
                                                     LotNumber2 = a.LotNumber2,
                                                     LotDate = a.LotDate,
                                                     Length = a.Length,
                                                     Weight = a.Weight,
                                                     Width = a.Width,
                                                     Height = a.Height,
                                                     UserName = userName,
                                                     Sequence = a.Sequence
                                                 }).ToList();
                                }

                                //by订单验证同一托盘是否被重复释放
                                List<PickTaskDetail> checkHuDetailList = new List<PickTaskDetail>();

                                if (List.Count > 0)
                                {
                                    AddPickTaskDetailTo(loadId, whCode, userName, List, markCount, forEntity, ReleaseType, checkHuDetailList);
                                }

                                markCount = "PO";
                                //2. 查看当前是按Load释放 还是按订单释放
                                forEntity = null;     //插入备货任务表时  需要使用的实体

                                //by Load释放
                                if (ReleaseType == "1")
                                {
                                    forEntity = entity2;
                                }
                                else if (ReleaseType == "2")  //by 订单释放
                                {
                                    forEntity = (from a in idal.IOutBoundOrderDetailDAL.SelectAll()
                                                 join b in idal.IOutBoundOrderDAL.SelectAll() on new { OutBoundOrderId = a.OutBoundOrderId } equals new { OutBoundOrderId = b.Id }
                                                 join c in idal.ILoadDetailDAL.SelectAll() on new { OutBoundOrderId = a.OutBoundOrderId } equals new { OutBoundOrderId = (Int32)c.OutBoundOrderId }
                                                 join d in idal.ILoadMasterDAL.SelectAll() on new { LoadMasterId = (Int32)c.LoadMasterId } equals new { LoadMasterId = d.Id }
                                                 where d.LoadId == loadId && d.WhCode == whCode && a.DSFLag != 1 && (a.SoNumber ?? "") == "" && (a.CustomerPoNumber ?? "") != ""
                                                 select new ReleaseLoad
                                                 {
                                                     WhCode = a.WhCode,
                                                     LoadId = d.LoadId,
                                                     ClientId = b.ClientId,
                                                     ClientCode = b.ClientCode,
                                                     OutBoundOrderId = a.OutBoundOrderId,
                                                     SoNumber = a.SoNumber,
                                                     CustomerPoNumber = a.CustomerPoNumber,
                                                     AltItemNumber = a.AltItemNumber,
                                                     ItemId = a.ItemId,
                                                     UnitId = a.UnitId,
                                                     UnitName = a.UnitName,
                                                     Qty = a.Qty,
                                                     LotNumber1 = a.LotNumber1,
                                                     LotNumber2 = a.LotNumber2,
                                                     LotDate = a.LotDate,
                                                     Length = a.Length,
                                                     Weight = a.Weight,
                                                     Width = a.Width,
                                                     Height = a.Height,
                                                     UserName = userName,
                                                     Sequence = a.Sequence
                                                 }).ToList();
                                }

                                if (List2.Count > 0)
                                {
                                    AddPickTaskDetailTo(loadId, whCode, userName, List2, markCount, forEntity, ReleaseType, checkHuDetailList);
                                }

                                markCount = "SO";
                                //2. 查看当前是按Load释放 还是按订单释放
                                forEntity = null;     //插入备货任务表时  需要使用的实体

                                //by Load释放
                                if (ReleaseType == "1")
                                {
                                    forEntity = entity4;
                                }
                                else if (ReleaseType == "2")  //by 订单释放
                                {
                                    forEntity = (from a in idal.IOutBoundOrderDetailDAL.SelectAll()
                                                 join b in idal.IOutBoundOrderDAL.SelectAll() on new { OutBoundOrderId = a.OutBoundOrderId } equals new { OutBoundOrderId = b.Id }
                                                 join c in idal.ILoadDetailDAL.SelectAll() on new { OutBoundOrderId = a.OutBoundOrderId } equals new { OutBoundOrderId = (Int32)c.OutBoundOrderId }
                                                 join d in idal.ILoadMasterDAL.SelectAll() on new { LoadMasterId = (Int32)c.LoadMasterId } equals new { LoadMasterId = d.Id }
                                                 where d.LoadId == loadId && d.WhCode == whCode && a.DSFLag != 1 && (a.SoNumber ?? "") != "" && (a.CustomerPoNumber ?? "") == ""
                                                 select new ReleaseLoad
                                                 {
                                                     WhCode = a.WhCode,
                                                     LoadId = d.LoadId,
                                                     ClientId = b.ClientId,
                                                     ClientCode = b.ClientCode,
                                                     OutBoundOrderId = a.OutBoundOrderId,
                                                     SoNumber = a.SoNumber,
                                                     CustomerPoNumber = a.CustomerPoNumber,
                                                     AltItemNumber = a.AltItemNumber,
                                                     ItemId = a.ItemId,
                                                     UnitId = a.UnitId,
                                                     UnitName = a.UnitName,
                                                     Qty = a.Qty,
                                                     LotNumber1 = a.LotNumber1,
                                                     LotNumber2 = a.LotNumber2,
                                                     LotDate = a.LotDate,
                                                     Length = a.Length,
                                                     Weight = a.Weight,
                                                     Width = a.Width,
                                                     Height = a.Height,
                                                     UserName = userName,
                                                     Sequence = a.Sequence
                                                 }).ToList();
                                }

                                if (List4.Count > 0)
                                {
                                    AddPickTaskDetailTo(loadId, whCode, userName, List4, markCount, forEntity, ReleaseType, checkHuDetailList);
                                }

                                markCount = "";
                                //2. 查看当前是按Load释放 还是按订单释放
                                forEntity = null;     //插入备货任务表时  需要使用的实体

                                //by Load释放
                                if (ReleaseType == "1")
                                {
                                    forEntity = entity3;
                                }
                                else if (ReleaseType == "2")  //by 订单释放
                                {
                                    forEntity = (from a in idal.IOutBoundOrderDetailDAL.SelectAll()
                                                 join b in idal.IOutBoundOrderDAL.SelectAll() on new { OutBoundOrderId = a.OutBoundOrderId } equals new { OutBoundOrderId = b.Id }
                                                 join c in idal.ILoadDetailDAL.SelectAll() on new { OutBoundOrderId = a.OutBoundOrderId } equals new { OutBoundOrderId = (Int32)c.OutBoundOrderId }
                                                 join d in idal.ILoadMasterDAL.SelectAll() on new { LoadMasterId = (Int32)c.LoadMasterId } equals new { LoadMasterId = d.Id }
                                                 where d.LoadId == loadId && d.WhCode == whCode && a.DSFLag != 1 && (a.SoNumber ?? "") == "" && (a.CustomerPoNumber ?? "") == ""
                                                 select new ReleaseLoad
                                                 {
                                                     WhCode = a.WhCode,
                                                     LoadId = d.LoadId,
                                                     ClientId = b.ClientId,
                                                     ClientCode = b.ClientCode,
                                                     OutBoundOrderId = a.OutBoundOrderId,
                                                     SoNumber = a.SoNumber,
                                                     CustomerPoNumber = a.CustomerPoNumber,
                                                     AltItemNumber = a.AltItemNumber,
                                                     ItemId = a.ItemId,
                                                     UnitId = a.UnitId,
                                                     UnitName = a.UnitName,
                                                     Qty = a.Qty,
                                                     LotNumber1 = a.LotNumber1,
                                                     LotNumber2 = a.LotNumber2,
                                                     LotDate = a.LotDate,
                                                     Length = a.Length,
                                                     Weight = a.Weight,
                                                     Width = a.Width,
                                                     Height = a.Height,
                                                     UserName = userName,
                                                     Sequence = a.Sequence
                                                 }).ToList();
                                }

                                if (List3.Count > 0)
                                {
                                    AddPickTaskDetailTo(loadId, whCode, userName, List3, markCount, forEntity, ReleaseType, checkHuDetailList);
                                }

                                //3.1验证Load是否有直装部分 有直装 添加直装备货任务
                                List<DSReleaseLoad> DSOutBoundList = (from a in idal.IOutBoundOrderDetailDAL.SelectAll()
                                                                      join b in idal.IOutBoundOrderDAL.SelectAll() on new { OutBoundOrderId = a.OutBoundOrderId } equals new { OutBoundOrderId = b.Id }
                                                                      join c in idal.ILoadDetailDAL.SelectAll() on new { OutBoundOrderId = a.OutBoundOrderId } equals new { OutBoundOrderId = (Int32)c.OutBoundOrderId }
                                                                      join d in idal.ILoadMasterDAL.SelectAll() on new { LoadMasterId = (Int32)c.LoadMasterId } equals new { LoadMasterId = d.Id }
                                                                      where d.LoadId == loadId && d.WhCode == whCode && a.DSFLag == 1
                                                                      group new { a, d, b } by new
                                                                      {
                                                                          a.WhCode,
                                                                          d.LoadId,
                                                                          b.ClientId,
                                                                          b.ClientCode,
                                                                          a.SoNumber,
                                                                          a.CustomerPoNumber,
                                                                          a.AltItemNumber,
                                                                          a.ItemId,
                                                                          a.UnitId,
                                                                          a.UnitName,
                                                                          a.LotNumber1,
                                                                          a.LotNumber2,
                                                                          a.LotDate,
                                                                          a.Length,
                                                                          a.Weight,
                                                                          a.Width,
                                                                          a.Height,
                                                                          userName
                                                                      } into g
                                                                      select new DSReleaseLoad
                                                                      {
                                                                          WhCode = g.Key.WhCode,
                                                                          LoadId = g.Key.LoadId,
                                                                          ClientId = g.Key.ClientId,
                                                                          ClientCode = g.Key.ClientCode,
                                                                          SoNumber = g.Key.SoNumber,
                                                                          CustomerPoNumber = g.Key.CustomerPoNumber,
                                                                          AltItemNumber = g.Key.AltItemNumber,
                                                                          ItemId = g.Key.ItemId,
                                                                          UnitId = g.Key.UnitId,
                                                                          UnitName = g.Key.UnitName,
                                                                          Qty = g.Sum(p => p.a.Qty),
                                                                          LotNumber1 = g.Key.LotNumber1,
                                                                          LotNumber2 = g.Key.LotNumber2,
                                                                          LotDate = (DateTime?)g.Key.LotDate,
                                                                          Length = g.Key.Length,
                                                                          Weight = g.Key.Weight,
                                                                          Width = g.Key.Width,
                                                                          Height = g.Key.Height,
                                                                          UserName = g.Key.userName
                                                                      }).ToList();
                                if (DSOutBoundList.Count > 0)
                                {
                                    List<PickTaskDetail> addpickTask = new List<PickTaskDetail>();
                                    foreach (var item in DSOutBoundList)
                                    {
                                        PickTaskDetail detail = new PickTaskDetail();
                                        detail.WhCode = item.WhCode;
                                        detail.LoadId = loadId;
                                        detail.HuDetailId = 0;
                                        detail.HuId = "直装";
                                        detail.ClientCode = item.ClientCode;
                                        detail.Location = item.ClientCode;
                                        detail.SoNumber = item.SoNumber;
                                        detail.CustomerPoNumber = item.CustomerPoNumber;
                                        detail.AltItemNumber = item.AltItemNumber;
                                        detail.ItemId = (Int32)item.ItemId;
                                        detail.UnitId = item.UnitId;
                                        detail.UnitName = item.UnitName;
                                        detail.Qty = (Int32)item.Qty;
                                        detail.DSFLag = 1;

                                        detail.PickQty = 0;
                                        detail.Length = (item.Length ?? 0);
                                        detail.Width = (item.Width ?? 0);
                                        detail.Height = (item.Height ?? 0);
                                        detail.Weight = (item.Weight ?? 0);
                                        detail.LotNumber1 = item.LotNumber1;
                                        detail.LotNumber2 = item.LotNumber2;
                                        detail.LotDate = item.LotDate;
                                        detail.Status = "U";
                                        detail.Status1 = "U";
                                        detail.CreateUser = userName;
                                        detail.CreateDate = DateTime.Now;
                                        addpickTask.Add(detail);
                                    }
                                    idal.IPickTaskDetailDAL.Add(addpickTask);
                                }

                                #endregion
                            }
                            else
                            {
                                return "当前Load：" + loadId + "未找到释放流程！";
                            }

                            #endregion
                        }

                        //4. 修改Load状态
                        LoadMaster load = idal.ILoadMasterDAL.SelectBy(u => u.WhCode == whCode && u.LoadId == loadId).First();
                        load.Status0 = "C";
                        load.ClientQty = clientCodeQty;
                        load.ClientCode = clientCodeArr;
                        load.ReleaseDate = DateTime.Now;
                        load.UpdateUser = userName;
                        load.UpdateDate = DateTime.Now;
                        idal.ILoadMasterDAL.UpdateBy(load, u => u.Id == load.Id, new string[] { "Status0", "ClientQty", "ClientCode", "ReleaseDate", "UpdateUser", "UpdateDate" });


                        //5. 修改OutOrder状态

                        #region 释放后调整出库订单状态

                        List<OutBoundOrderResult> list = (from a in idal.ILoadDetailDAL.SelectAll()
                                                          where a.LoadMasterId == load.Id
                                                          select new OutBoundOrderResult
                                                          {
                                                              Id = (Int32)a.OutBoundOrderId
                                                          }).ToList();

                        int[] outBoundOrderIdArr = (from a in list
                                                    select a.Id).ToList().Distinct().ToArray();

                        List<OutBoundOrder> getList1 = idal.IOutBoundOrderDAL.SelectBy(u => outBoundOrderIdArr.Contains(u.Id));

                        List<OutBoundOrder> getList = getList1.Where(u => u.WhCode == whCode && u.StatusName == "已生成Load" && u.StatusId > 0).ToList();

                        List<TranLog> tranAddList = new List<TranLog>();
                        foreach (var item in list)
                        {
                            if (getList.Where(u => u.Id == item.Id).Count() == 0)
                            {
                                continue;
                            }
                            OutBoundOrder eneity = getList.Where(u => u.Id == item.Id).First();
                            if (eneity.StatusId > 0)
                            {
                                string remark1 = "原状态：" + eneity.StatusId + eneity.StatusName;

                                FlowHelper flowHelper = new FlowHelper(eneity, "OutBound");
                                FlowDetail flowDetail = flowHelper.GetNextFlowDetail();
                                if (flowDetail != null && flowDetail.StatusId != 0)
                                {
                                    eneity.NowProcessId = flowDetail.FlowRuleId;
                                    eneity.StatusId = flowDetail.StatusId;
                                    eneity.StatusName = flowDetail.StatusName;
                                    eneity.UpdateUser = userName;
                                    eneity.UpdateDate = DateTime.Now;

                                    //更新订单状态，插入日志
                                    TranLog tlorder = new TranLog();
                                    tlorder.TranType = "32";
                                    tlorder.Description = "更新订单状态";
                                    tlorder.TranDate = DateTime.Now;
                                    tlorder.TranUser = userName;
                                    tlorder.WhCode = whCode;
                                    tlorder.LoadId = loadMaster.LoadId;
                                    tlorder.CustomerOutPoNumber = eneity.CustomerOutPoNumber;
                                    tlorder.OutPoNumber = eneity.OutPoNumber;
                                    tlorder.Remark = remark1 + "变更为：" + eneity.StatusId + eneity.StatusName;
                                    tranAddList.Add(tlorder);

                                }
                                else
                                {
                                    return "错误！获取订单状态有误！";
                                }
                            }
                            else
                            {
                                InterceptManager intercept = new InterceptManager();
                                intercept.UpdateOutBoundOrderStatus(userName, "已拦截已处理", eneity);
                                idal.ILoadDetailDAL.DeleteByExtended(u => u.OutBoundOrderId == eneity.Id);
                            }
                        }
                        idal.ITranLogDAL.Add(tranAddList);

                        #endregion


                        //6. 创建分拣任务
                        string sortResult = sortTaskManager.CreateSortTask(loadId, whCode, userName);
                        if (sortResult != "Y")
                        {
                            return sortResult;
                        }

                        //只执行一次保存
                        idal.IHuDetailDAL.SaveChanges();

                        #region 释放成功后调整CBM挂衣等数据

                        //by yujia 18.02.26
                        //释放完成后更新LoadMaster的CBM，挂衣数，货物总数，货物总重量
                        //得到备货任务明细表数据
                        List<PickTaskDetail> getPickTaskDetail = idal.IPickTaskDetailDAL.SelectBy(u => u.WhCode == whCode && u.LoadId == loadId);

                        //释放成功后 检测备货数量是否大于库存锁定数量
                        int?[] huDetailId = (from a in getPickTaskDetail
                                             select a.HuDetailId).ToList().Distinct().ToArray();

                        List<HuDetail> checkHuDetail = idal.IHuDetailDAL.SelectBy(u => huDetailId.Contains(u.Id));
                        foreach (var item in huDetailId)
                        {
                            int getPickQty = Convert.ToInt32(getPickTaskDetail.Where(u => u.HuDetailId == item).Sum(u => u.Qty).ToString());

                            HuDetail firstHuDetail = checkHuDetail.Where(u => u.Id == item).First();
                            int getHuQty = Convert.ToInt32(firstHuDetail.PlanQty ?? 0);
                            if (getPickQty > getHuQty)
                            {
                                trans.Dispose();//出现异常，事务手动释放
                                return "出现异常!库存锁定小于备货数量!托盘:" + firstHuDetail.HuId + ",款号:" + firstHuDetail.AltItemNumber + "请马上联系处理!";
                            }
                        }

                        //计算立方时 剔除挂衣数
                        decimal sumCbm = Convert.ToDecimal(getPickTaskDetail.Where(u => !u.UnitName.Contains("ECH")).Sum(u => u.Qty * u.Length * u.Width * u.Height).ToString());
                        int sumQty = Convert.ToInt32(getPickTaskDetail.Sum(u => u.Qty).ToString());
                        int dssumQty = Convert.ToInt32(getPickTaskDetail.Where(u => u.DSFLag == 1).Sum(u => u.Qty).ToString());
                        decimal sumWeight = Convert.ToDecimal(getPickTaskDetail.Sum(u => u.Weight).ToString());
                        int echQty = Convert.ToInt32(getPickTaskDetail.Where(u => u.UnitName.Contains("ECH")).Sum(u => u.Qty).ToString());

                        load.SumCBM = sumCbm;
                        load.SumQty = sumQty;
                        load.DSSumQty = dssumQty;
                        load.SumWeight = sumWeight;
                        load.EchQty = echQty;
                        idal.ILoadMasterDAL.UpdateBy(load, u => u.Id == load.Id, new string[] { "SumCBM", "SumQty", "DSSumQty", "SumWeight", "EchQty" });

                        #endregion

                        #endregion

                        idal.ILoadMasterDAL.SaveChanges();

                        //释放完成后 修改备货任务表的备货指引顺序
                        EditPickingSequence(loadId, whCode, 0);

                        trans.Complete();
                        return "Y";
                    }
                    catch (Exception e)
                    {
                        string s = e.InnerException.Message;
                        trans.Dispose();//出现异常，事务手动释放
                        return "释放异常，请重试！";
                    }
                }
            }
        }

        //根据订单情况 SO PO 添加备货任务
        private void AddPickTaskDetailTo(string loadId, string whCode, string userName, List<HuDetailResult> List, string markCount, List<ReleaseLoad> forEntity, string ReleaseType, List<PickTaskDetail> checkHuDetailList)
        {
            //by订单List 
            List<HuDetailResult> orderList = new List<HuDetailResult>();

            //byLoadList 
            List<HuDetail> ByLoadList = new List<HuDetail>();

            //by订单验证同一托盘是否被重复释放
            List<PickTaskDetail> checkHuDetailListByOrder = new List<PickTaskDetail>();

            //3. 开始插入 备货任务表
            foreach (var item in forEntity)
            {
                //-----------------------先进先出、后进先出排序规则----------------

                #region 库存排序
                List<HuDetailResult> ListWhere = new List<HuDetailResult>();

                if (markCount == "SOPO")
                {
                    ListWhere = List.Where(u => u.SoNumber == item.SoNumber && u.CustomerPoNumber == item.CustomerPoNumber && u.AltItemNumber == item.AltItemNumber && u.ItemId == item.ItemId && u.UnitName == item.UnitName && u.UnitId == item.UnitId && u.LotNumber1 == item.LotNumber1 && u.LotNumber2 == item.LotNumber2 &&
                      u.LotDate == item.LotDate).OrderBy(u => u.LocationTypeDetailId).ThenBy(u => u.ReceiptDate).ToList();
                }
                else if (markCount == "PO")
                {
                    ListWhere = List.Where(u => u.CustomerPoNumber == item.CustomerPoNumber && u.AltItemNumber == item.AltItemNumber && u.ItemId == item.ItemId && u.UnitName == item.UnitName && u.UnitId == item.UnitId && u.LotNumber1 == item.LotNumber1 && u.LotNumber2 == item.LotNumber2 &&
                       u.LotDate == item.LotDate).OrderBy(u => u.LocationTypeDetailId).ThenBy(u => u.ReceiptDate).ToList();
                }
                else if (markCount == "SO")
                {
                    ListWhere = List.Where(u => u.SoNumber == item.SoNumber && u.AltItemNumber == item.AltItemNumber && u.ItemId == item.ItemId && u.UnitName == item.UnitName && u.UnitId == item.UnitId && u.LotNumber1 == item.LotNumber1 && u.LotNumber2 == item.LotNumber2 &&
                       u.LotDate == item.LotDate).OrderBy(u => u.LocationTypeDetailId).ThenBy(u => u.ReceiptDate).ToList();
                }
                else if (markCount == "")
                {
                    ListWhere = List.Where(u => u.AltItemNumber == item.AltItemNumber && u.ItemId == item.ItemId && u.UnitName == item.UnitName && u.UnitId == item.UnitId && u.LotNumber1 == item.LotNumber1 && u.LotNumber2 == item.LotNumber2 &&
                      u.LotDate == item.LotDate).OrderBy(u => u.LocationTypeDetailId).ThenBy(u => u.ReceiptDate).ToList();
                }

                #endregion  结束排序

                ///--------开始插入备货任务表--------------------
                int qtyResult = (Int32)item.Qty;  //得到出库订单的数量
                foreach (var sqlDetail in ListWhere)    //得到经过筛选后的库存
                {
                    if (qtyResult <= 0)
                    {
                        break;
                    }
                    else
                    {
                        int? sqlDetailQty = sqlDetail.Qty;
                        if (checkHuDetailList.Where(u => u.HuDetailId == sqlDetail.Id).Count() > 0)
                        {
                            int getQty = checkHuDetailList.Where(u => u.HuDetailId == sqlDetail.Id).Sum(u => u.Qty);
                            sqlDetailQty = sqlDetail.Qty - getQty;
                        }
                        if (sqlDetailQty <= 0)
                        {
                            continue;
                        }

                        if (checkHuDetailListByOrder.Where(u => u.HuDetailId == sqlDetail.Id).Count() > 0)
                        {
                            int getQty = checkHuDetailListByOrder.Where(u => u.HuDetailId == sqlDetail.Id).Sum(u => u.Qty);
                            sqlDetailQty = sqlDetail.Qty - getQty;
                        }
                        if (sqlDetailQty <= 0)
                        {
                            continue;
                        }

                        PickTaskDetail detail = new PickTaskDetail();
                        if (ReleaseType == "2")  //by 订单释放
                            detail.OutBoundOrderId = item.OutBoundOrderId;

                        detail.WhCode = sqlDetail.WhCode;
                        detail.LoadId = loadId;
                        detail.HuDetailId = sqlDetail.Id;
                        detail.ClientCode = sqlDetail.ClientCode;
                        detail.HuId = sqlDetail.HuId;
                        detail.Location = sqlDetail.Location;
                        detail.SoNumber = sqlDetail.SoNumber;
                        detail.CustomerPoNumber = sqlDetail.CustomerPoNumber;
                        detail.AltItemNumber = sqlDetail.AltItemNumber;
                        detail.ItemId = (Int32)sqlDetail.ItemId;
                        detail.UnitId = sqlDetail.UnitId;
                        detail.UnitName = sqlDetail.UnitName;
                        if (qtyResult >= sqlDetailQty - (sqlDetail.PlanQty ?? 0))
                        {
                            detail.Qty = (Int32)sqlDetailQty - (sqlDetail.PlanQty ?? 0);  //释放数量
                            qtyResult = qtyResult - detail.Qty;    //剩余的未释放数量
                        }
                        else
                        {
                            detail.Qty = qtyResult;
                            qtyResult = 0;
                        }

                        if (detail.Qty <= 0)
                        {
                            continue;
                        }
                        detail.PickQty = 0;
                        detail.Length = (sqlDetail.Length ?? 0);
                        detail.Width = (sqlDetail.Width ?? 0);
                        detail.Height = (sqlDetail.Height ?? 0);
                        detail.Weight = (sqlDetail.Weight ?? 0);
                        detail.LotNumber1 = sqlDetail.LotNumber1;
                        detail.LotNumber2 = sqlDetail.LotNumber2;
                        detail.LotDate = sqlDetail.LotDate;
                        detail.Sequence = item.Sequence;
                        detail.Status = "U";
                        detail.Status1 = "U";
                        detail.CreateUser = userName;
                        detail.CreateDate = DateTime.Now;
                        detail.ReceiptDate = sqlDetail.ReceiptDate;
                        checkHuDetailListByOrder.Add(detail);

                        checkHuDetailList.Add(detail);



                        //2016年修改  --张雨佳
                        //by 订单释放修改
                        //如果多个订单包含同一个款号 因这里没有实时更新 导致只会更新最后一次出库订单的数量
                        //现增加List累加后 最后执行一次保存就没有问题
                        if (ReleaseType == "2")
                        {
                            if (orderList.Where(u => u.Id == sqlDetail.Id).Count() == 0)
                            {
                                HuDetailResult en = new HuDetailResult();
                                en.Id = sqlDetail.Id;
                                en.PlanQty = detail.Qty;
                                orderList.Add(en);
                            }
                            else
                            {
                                HuDetailResult en = orderList.Where(u => u.Id == sqlDetail.Id).First();
                                HuDetailResult en1 = new HuDetailResult();
                                en1.Id = sqlDetail.Id;
                                en1.PlanQty = en.PlanQty + detail.Qty;
                                orderList.Add(en1);
                                orderList.Remove(en);
                            }
                        }
                        else
                        {
                            if (ByLoadList.Where(u => u.Id == sqlDetail.Id).Count() == 0)
                            {
                                //更新by Load 情况的 库存数量
                                HuDetail huDetail = new HuDetail();
                                huDetail.Id = sqlDetail.Id;
                                huDetail.PlanQty = (sqlDetail.PlanQty ?? 0) + detail.Qty;

                                ByLoadList.Add(huDetail);
                            }
                            else
                            {
                                HuDetail oldhuDetail = ByLoadList.Where(u => u.Id == sqlDetail.Id).First();

                                //更新by Load 情况的 库存数量
                                HuDetail huDetail = new HuDetail();
                                huDetail.Id = sqlDetail.Id;
                                huDetail.PlanQty = (sqlDetail.PlanQty ?? 0) + detail.Qty + oldhuDetail.PlanQty;

                                ByLoadList.Add(huDetail);
                                ByLoadList.Remove(oldhuDetail);
                            }

                        }
                    }
                }
            }

            List<PickTaskDetail> PickTaskDetailList = new List<PickTaskDetail>();
            if (ReleaseType == "2")  //by 订单释放
            {
                foreach (var item in checkHuDetailListByOrder.Where(u => u.Qty > 0))
                {
                    if (item.Qty <= 0)
                    {
                        continue;
                    }
                    if (PickTaskDetailList.Where(u => u.HuDetailId == item.HuDetailId && u.OutBoundOrderId == item.OutBoundOrderId).Count() == 0)
                    {
                        PickTaskDetail detail = new PickTaskDetail();
                        detail.OutBoundOrderId = item.OutBoundOrderId;
                        detail.WhCode = item.WhCode;
                        detail.LoadId = item.LoadId;
                        detail.HuDetailId = item.HuDetailId;
                        detail.ClientCode = item.ClientCode;
                        detail.HuId = item.HuId;
                        detail.Location = item.Location;
                        detail.SoNumber = item.SoNumber;
                        detail.CustomerPoNumber = item.CustomerPoNumber;
                        detail.AltItemNumber = item.AltItemNumber;
                        detail.ItemId = item.ItemId;
                        detail.UnitId = item.UnitId;
                        detail.UnitName = item.UnitName;
                        detail.Qty = item.Qty;
                        detail.PickQty = 0;
                        detail.Length = (item.Length ?? 0);
                        detail.Width = (item.Width ?? 0);
                        detail.Height = (item.Height ?? 0);
                        detail.Weight = (item.Weight ?? 0);
                        detail.LotNumber1 = item.LotNumber1;
                        detail.LotNumber2 = item.LotNumber2;
                        detail.LotDate = item.LotDate;
                        detail.Sequence = item.Sequence;
                        detail.Status = "U";
                        detail.Status1 = "U";
                        detail.CreateUser = item.CreateUser;
                        detail.CreateDate = DateTime.Now;
                        detail.ReceiptDate = item.ReceiptDate;
                        PickTaskDetailList.Add(detail);
                    }
                    else
                    {
                        PickTaskDetail en = PickTaskDetailList.Where(u => u.HuDetailId == item.HuDetailId && u.OutBoundOrderId == item.OutBoundOrderId).First();
                        PickTaskDetail en1 = en;
                        en1.Qty = en.Qty + item.Qty;

                        PickTaskDetailList.Remove(en);
                        PickTaskDetailList.Add(en1);
                    }
                }
                //添加备货任务信息
                idal.IPickTaskDetailDAL.Add(PickTaskDetailList.Where(u => u.Qty > 0));
            }
            else
            {
                //添加备货任务信息
                idal.IPickTaskDetailDAL.Add(checkHuDetailListByOrder.Where(u => u.Qty > 0));
            }

            //日志记录
            List<TranLog> tranLogList = new List<TranLog>();

            //更新 by订单情况的 库存数量
            if (orderList != null)
            {
                foreach (var item in orderList)
                {
                    HuDetail huDetail = idal.IHuDetailDAL.SelectBy(u => u.Id == item.Id).First();

                    TranLog tl = new TranLog();
                    tl.TranType = "30";
                    tl.Description = "释放Load";
                    tl.TranDate = DateTime.Now;
                    tl.TranUser = userName;
                    tl.WhCode = whCode;
                    tl.ClientCode = huDetail.ClientCode;
                    tl.SoNumber = huDetail.SoNumber;
                    tl.CustomerPoNumber = huDetail.CustomerPoNumber;
                    tl.AltItemNumber = huDetail.AltItemNumber;
                    tl.ItemId = huDetail.ItemId;
                    tl.UnitID = huDetail.UnitId;
                    tl.UnitName = huDetail.UnitName;
                    tl.ReceiptId = huDetail.ReceiptId;
                    tl.ReceiptDate = huDetail.ReceiptDate;
                    tl.TranQty = (huDetail.PlanQty ?? 0);
                    tl.HuId = huDetail.HuId;
                    tl.Length = huDetail.Length;
                    tl.Width = huDetail.Width;
                    tl.Height = huDetail.Height;
                    tl.Weight = huDetail.Weight;
                    tl.LotNumber1 = huDetail.LotNumber1;
                    tl.LotNumber2 = huDetail.LotNumber2;
                    tl.LotDate = huDetail.LotDate;
                    tl.LoadId = loadId;
                    tl.Remark = "锁定数量+" + (item.PlanQty - (huDetail.PlanQty ?? 0));

                    huDetail.PlanQty = item.PlanQty;
                    huDetail.UpdateUser = userName;
                    huDetail.UpdateDate = DateTime.Now;
                    idal.IHuDetailDAL.UpdateBy(huDetail, u => u.Id == item.Id, new string[] { "PlanQty", "UpdateUser", "UpdateDate" });

                    tl.TranQty2 = huDetail.PlanQty;
                    tranLogList.Add(tl);
                }
            }

            //更新 byLoad情况的 库存数量
            if (ByLoadList != null)
            {
                foreach (var item in ByLoadList)
                {
                    HuDetail huDetail = idal.IHuDetailDAL.SelectBy(u => u.Id == item.Id).First();

                    TranLog tl = new TranLog();
                    tl.TranType = "30";
                    tl.Description = "释放Load";
                    tl.TranDate = DateTime.Now;
                    tl.TranUser = userName;
                    tl.WhCode = whCode;
                    tl.ClientCode = huDetail.ClientCode;
                    tl.SoNumber = huDetail.SoNumber;
                    tl.CustomerPoNumber = huDetail.CustomerPoNumber;
                    tl.AltItemNumber = huDetail.AltItemNumber;
                    tl.ItemId = huDetail.ItemId;
                    tl.UnitID = huDetail.UnitId;
                    tl.UnitName = huDetail.UnitName;
                    tl.ReceiptId = huDetail.ReceiptId;
                    tl.ReceiptDate = huDetail.ReceiptDate;
                    tl.TranQty = (huDetail.PlanQty ?? 0);
                    tl.HuId = huDetail.HuId;
                    tl.Length = huDetail.Length;
                    tl.Width = huDetail.Width;
                    tl.Height = huDetail.Height;
                    tl.Weight = huDetail.Weight;
                    tl.LotNumber1 = huDetail.LotNumber1;
                    tl.LotNumber2 = huDetail.LotNumber2;
                    tl.LotDate = huDetail.LotDate;
                    tl.LoadId = loadId;
                    tl.Remark = "锁定数量+" + (item.PlanQty - (huDetail.PlanQty ?? 0));

                    huDetail.PlanQty = item.PlanQty;
                    huDetail.UpdateUser = userName;
                    huDetail.UpdateDate = DateTime.Now;
                    idal.IHuDetailDAL.UpdateBy(huDetail, u => u.Id == item.Id, new string[] { "PlanQty", "UpdateUser", "UpdateDate" });

                    tl.TranQty2 = huDetail.PlanQty;
                    tranLogList.Add(tl);
                }
            }

            //保存释放日志
            idal.ITranLogDAL.Add(tranLogList);
        }


        //验证库存是否满足释放条件
        //新增释放流程7时 需更该 完成收货方法(),完成封箱方法()
        public CheckReleaseLoadResult CheckInventory(string mark, string loadId, string whCode, string userName)
        {
            CheckReleaseLoadResult checkReleaseLoadResult = new CheckReleaseLoadResult();
            string result = "";
            List<ReleaseLoad> entity = new List<ReleaseLoad>();         //后面需使用的实体对象
            List<ReleaseLoad> entity1 = new List<ReleaseLoad>();         //后面需使用的实体对象

            List<HuDetailResult> List = new List<HuDetailResult>();     //库存需要比较的实体对象
            IQueryable<HuDetailResult> ListIQueryable = null;     //库存需要比较的实体对象
            //验证库存思路
            //1.通过Load号 拉取 出库订单信息 同时聚合同款SKU的数量，赋值为 实体1
            //2.用实体1 的SKU、Lot 条件 拉取对应的SKU Lot库存，赋值为 实体2
            //3. 循环 实体1  查询 实体2同一SKU Lot等 的数量是否满足

            //重要-----------------新增释放流程 如新增7时 需更该 完成收货方法(),完成封箱方法()
            //重要-----------------新增释放流程 如新增7时 需更该 完成收货方法(),完成封箱方法()
            //重要-----------------新增释放流程 如新增7时 需更该 完成收货方法(),完成封箱方法()

            #region 1. 释放流程为34 无视SO PO的释放条件
            if (mark == "3" || mark == "4")
            {
                //1.通过Load号   拉取 出库订单信息 聚合同款数量
                var sql_get1 = from a in idal.IOutBoundOrderDetailDAL.SelectAll()
                               join b in idal.IOutBoundOrderDAL.SelectAll() on new { OutBoundOrderId = a.OutBoundOrderId } equals new { OutBoundOrderId = b.Id }
                               join c in idal.ILoadDetailDAL.SelectAll() on new { OutBoundOrderId = a.OutBoundOrderId } equals new { OutBoundOrderId = (Int32)c.OutBoundOrderId }
                               join d in idal.ILoadMasterDAL.SelectAll() on new { LoadMasterId = (Int32)c.LoadMasterId } equals new { LoadMasterId = d.Id }
                               where d.LoadId == loadId && d.WhCode == whCode && a.DSFLag != 1
                               group new { a, d, b } by new
                               {
                                   a.WhCode,
                                   d.LoadId,
                                   b.ClientId,
                                   b.ClientCode,
                                   a.SoNumber,
                                   a.CustomerPoNumber,
                                   a.AltItemNumber,
                                   a.ItemId,
                                   a.UnitId,
                                   a.UnitName,
                                   a.LotNumber1,
                                   a.LotNumber2,
                                   a.LotDate,
                                   userName
                               } into g
                               select new ReleaseLoad
                               {
                                   WhCode = g.Key.WhCode,
                                   LoadId = g.Key.LoadId,
                                   ClientId = g.Key.ClientId,
                                   ClientCode = g.Key.ClientCode,
                                   SoNumber = g.Key.SoNumber,
                                   CustomerPoNumber = g.Key.CustomerPoNumber,
                                   AltItemNumber = g.Key.AltItemNumber,
                                   ItemId = g.Key.ItemId,
                                   UnitId = g.Key.UnitId,
                                   UnitName = g.Key.UnitName,
                                   Qty = g.Sum(p => p.a.Qty),
                                   LotNumber1 = g.Key.LotNumber1,
                                   LotNumber2 = g.Key.LotNumber2,
                                   LotDate = (DateTime?)g.Key.LotDate,
                                   UserName = g.Key.userName
                               };

                entity1 = (from a in idal.IOutBoundOrderDetailDAL.SelectAll()
                           join b in idal.IOutBoundOrderDAL.SelectAll() on new { OutBoundOrderId = a.OutBoundOrderId } equals new { OutBoundOrderId = b.Id }
                           join c in idal.ILoadDetailDAL.SelectAll() on new { OutBoundOrderId = a.OutBoundOrderId } equals new { OutBoundOrderId = (Int32)c.OutBoundOrderId }
                           join d in idal.ILoadMasterDAL.SelectAll() on new { LoadMasterId = (Int32)c.LoadMasterId } equals new { LoadMasterId = d.Id }
                           where d.LoadId == loadId && d.WhCode == whCode && a.DSFLag != 1
                           group new { a, d, b } by new
                           {
                               a.WhCode,
                               d.LoadId,
                               b.ClientId,
                               b.ClientCode,
                               a.SoNumber,
                               a.CustomerPoNumber,
                               a.AltItemNumber,
                               a.ItemId,
                               a.UnitId,
                               a.UnitName,
                               a.LotNumber1,
                               a.LotNumber2,
                               a.LotDate,
                               userName,
                               a.Sequence
                           } into g
                           select new ReleaseLoad
                           {
                               WhCode = g.Key.WhCode,
                               LoadId = g.Key.LoadId,
                               ClientId = g.Key.ClientId,
                               ClientCode = g.Key.ClientCode,
                               SoNumber = g.Key.SoNumber,
                               CustomerPoNumber = g.Key.CustomerPoNumber,
                               AltItemNumber = g.Key.AltItemNumber,
                               ItemId = g.Key.ItemId,
                               UnitId = g.Key.UnitId,
                               UnitName = g.Key.UnitName,
                               Qty = g.Sum(p => p.a.Qty),
                               Sequence = g.Key.Sequence,
                               LotNumber1 = g.Key.LotNumber1,
                               LotNumber2 = g.Key.LotNumber2,
                               LotDate = (DateTime?)g.Key.LotDate,
                               UserName = g.Key.userName
                           }).ToList();

                entity = sql_get1.ToList();     //赋值，后续使用

                //2. 用出库订单的款号 lot1 lot2 等 拉取库存对应的数据
                var sql = from a1 in sql_get1
                          join a in idal.IHuDetailDAL.SelectAll()
                          on new { X = a1.ClientId, A = a1.AltItemNumber, B = a1.ItemId, C = a1.UnitName, D = a1.UnitId, E = (a1.LotNumber1 ?? ""), F = (a1.LotNumber2 ?? "") } equals new { X = a.ClientId, A = a.AltItemNumber, B = a.ItemId, C = a.UnitName, D = a.UnitId, E = (a.LotNumber1 ?? ""), F = (a.LotNumber2 ?? "") }
                          join b in idal.IHuMasterDAL.SelectAll()
                                          on new { a.HuId, a.WhCode }
                                      equals new { b.HuId, b.WhCode }
                          join c in idal.IWhLocationDAL.SelectAll()
                                on new { b.Location, b.WhCode }
                         equals new { Location = c.LocationId, c.WhCode }
                          where
                            b.Type == "M" &&
                            b.Status == "A" &&
                            c.LocationTypeId == 1 &&
                            (a.Qty - (a.PlanQty ?? 0)) > 0
                          select new HuDetailResult
                          {
                              Id = a.Id,
                              HuId = a.HuId,
                              WhCode = a.WhCode,
                              ClientId = a.ClientId,
                              ClientCode = a.ClientCode,
                              SoNumber = a.SoNumber,
                              CustomerPoNumber = a.CustomerPoNumber,
                              AltItemNumber = a.AltItemNumber,
                              ReceiptDate = a.ReceiptDate,
                              PlanQty = a.PlanQty ?? 0,
                              Qty = a.Qty,
                              ItemId = a.ItemId,
                              UnitId = a.UnitId,
                              UnitName = a.UnitName,
                              Height = a.Height,
                              Length = a.Length,
                              Weight = a.Weight,
                              Width = a.Width,
                              LotNumber1 = a.LotNumber1,
                              LotNumber2 = a.LotNumber2,
                              LotDate = a.LotDate,
                              Location = b.Location,
                              LocationTypeId = c.LocationTypeId,
                              LocationTypeDetailId = ((c.LocationTypeDetailId ?? 0) == 0 ? 99 : c.LocationTypeDetailId)
                          };
                List = sql.Distinct().ToList();

                //2.1 如果没有库存 提示报错
                if (List.Count == 0)
                {
                    result = "释放有误库存不足，请检查托盘状态、库位、款号、属性123及Lot是否一致！";
                }

                if (result != "")
                {
                    checkReleaseLoadResult.CheckStatusResult = ReleaseLoadDetailAdd(loadId, whCode, mark);
                    checkReleaseLoadResult.Result = result;
                    return checkReleaseLoadResult;
                }

                //得到Load释放的 指定托盘表
                List<LoadHuIdExtend> loadHuIdList = idal.ILoadHuIdExtendDAL.SelectBy(u => u.WhCode == whCode && u.LoadId == loadId);
                if (loadHuIdList.Count > 0)
                {
                    string[] huid = (from a in loadHuIdList select a.HuId).ToArray();
                    List = List.Where(u => huid.Contains(u.HuId)).ToList();

                    if (List.Count == 0)
                    {
                        result = "指定托盘库存不足，请检查托盘状态及库位、款号、属性123及Lot是否一致！";
                    }
                }
                if (result != "")
                {
                    checkReleaseLoadResult.CheckStatusResult = ReleaseLoadDetailAdd(loadId, whCode, mark);
                    checkReleaseLoadResult.Result = result;
                    return checkReleaseLoadResult;
                }

                //3. 聚合 通过订单查询出的库存 的数量
                var sql1 = from hudetail in List
                           group hudetail by new
                           {
                               hudetail.WhCode,
                               hudetail.AltItemNumber,
                               hudetail.ItemId,
                               hudetail.UnitId,
                               hudetail.UnitName,
                               hudetail.LotNumber1,
                               hudetail.LotNumber2,
                               hudetail.LotDate
                           } into g
                           select new HuDetailResult
                           {
                               WhCode = g.Key.WhCode,
                               AltItemNumber = g.Key.AltItemNumber,
                               ItemId = g.Key.ItemId,
                               UnitId = g.Key.UnitId,
                               UnitName = g.Key.UnitName,
                               LotNumber1 = g.Key.LotNumber1,
                               LotNumber2 = g.Key.LotNumber2,
                               LotDate = g.Key.LotDate,
                               Qty = g.Sum(p => p.Qty - (p.PlanQty ?? 0))
                           };

                List<HuDetailResult> ListWhere = sql1.ToList(); //为什么没有直接在上面toList 是因为 方便调试

                foreach (var item in entity)    //循环出库订单数据
                {
                    //同一SKU lot条件下 数量是否满足
                    List<HuDetailResult> huDetailResultList = ListWhere.Where(u => u.WhCode == item.WhCode && u.AltItemNumber == item.AltItemNumber && u.ItemId == item.ItemId && u.UnitId == item.UnitId && u.UnitName == item.UnitName &&
                    (u.LotNumber1 == item.LotNumber1 || (u.LotNumber1 == null ? "" : u.LotNumber1) == (item.LotNumber1 == null ? "" : item.LotNumber1))
                    &&
                   (u.LotNumber2 == item.LotNumber2 || (u.LotNumber2 == null ? "" : u.LotNumber2) == (item.LotNumber2 == null ? "" : item.LotNumber2))).ToList();

                    if (huDetailResultList.Count == 0)
                    {
                        result += "SKU:" + item.AltItemNumber + "库存不足，请检查款号及Lot是否一致！";
                    }
                    else
                    {
                        int? sumQty = huDetailResultList.Sum(u => u.Qty);

                        if (sumQty < item.Qty)
                        {
                            result += "SKU:" + item.AltItemNumber + "库存小于出货数量！";
                        }
                    }
                }

                if (result != "")
                {
                    checkReleaseLoadResult.CheckStatusResult = ReleaseLoadDetailAdd(loadId, whCode, mark);
                    result = "库存不足！" + result;
                }

                checkReleaseLoadResult.Result = result;
                checkReleaseLoadResult.ReleaseLoadList = entity1;
                checkReleaseLoadResult.HuDetailResultList = List;
            }
            #endregion


            #region 2. 释放流程为129 带有SO PO 条件

            if (mark == "1" || mark == "2" || mark == "9")
            {
                //1.通过Load号   拉取 出库订单信息 聚合同款数量
                var sql_get1 = from a in idal.IOutBoundOrderDetailDAL.SelectAll()
                               join b in idal.IOutBoundOrderDAL.SelectAll() on new { OutBoundOrderId = a.OutBoundOrderId } equals new { OutBoundOrderId = b.Id }
                               join c in idal.ILoadDetailDAL.SelectAll() on new { OutBoundOrderId = a.OutBoundOrderId } equals new { OutBoundOrderId = (Int32)c.OutBoundOrderId }
                               join d in idal.ILoadMasterDAL.SelectAll() on new { LoadMasterId = (Int32)c.LoadMasterId } equals new { LoadMasterId = d.Id }
                               where d.LoadId == loadId && d.WhCode == whCode && a.DSFLag != 1
                               group new { a, d, b } by new
                               {
                                   a.WhCode,
                                   d.LoadId,
                                   b.ClientId,
                                   b.ClientCode,
                                   a.SoNumber,
                                   a.CustomerPoNumber,
                                   a.AltItemNumber,
                                   a.ItemId,
                                   a.UnitId,
                                   a.UnitName,
                                   a.LotNumber1,
                                   a.LotNumber2,
                                   a.LotDate,
                                   userName
                               } into g
                               select new ReleaseLoad
                               {
                                   WhCode = g.Key.WhCode,
                                   LoadId = g.Key.LoadId,
                                   ClientId = g.Key.ClientId,
                                   ClientCode = g.Key.ClientCode,
                                   SoNumber = g.Key.SoNumber,
                                   CustomerPoNumber = g.Key.CustomerPoNumber,
                                   AltItemNumber = g.Key.AltItemNumber,
                                   ItemId = g.Key.ItemId,
                                   UnitId = g.Key.UnitId,
                                   UnitName = g.Key.UnitName,
                                   Qty = g.Sum(p => p.a.Qty),
                                   LotNumber1 = g.Key.LotNumber1,
                                   LotNumber2 = g.Key.LotNumber2,
                                   LotDate = (DateTime?)g.Key.LotDate,
                                   UserName = g.Key.userName
                               };

                entity1 = (from a in idal.IOutBoundOrderDetailDAL.SelectAll()
                           join b in idal.IOutBoundOrderDAL.SelectAll() on new { OutBoundOrderId = a.OutBoundOrderId } equals new { OutBoundOrderId = b.Id }
                           join c in idal.ILoadDetailDAL.SelectAll() on new { OutBoundOrderId = a.OutBoundOrderId } equals new { OutBoundOrderId = (Int32)c.OutBoundOrderId }
                           join d in idal.ILoadMasterDAL.SelectAll() on new { LoadMasterId = (Int32)c.LoadMasterId } equals new { LoadMasterId = d.Id }
                           where d.LoadId == loadId && d.WhCode == whCode && a.DSFLag != 1
                           group new { a, d, b } by new
                           {
                               a.WhCode,
                               d.LoadId,
                               b.ClientId,
                               b.ClientCode,
                               a.SoNumber,
                               a.CustomerPoNumber,
                               a.AltItemNumber,
                               a.ItemId,
                               a.UnitId,
                               a.UnitName,
                               a.LotNumber1,
                               a.LotNumber2,
                               a.LotDate,

                               userName,
                               a.Sequence
                           } into g
                           select new ReleaseLoad
                           {
                               WhCode = g.Key.WhCode,
                               LoadId = g.Key.LoadId,
                               ClientId = g.Key.ClientId,
                               ClientCode = g.Key.ClientCode,
                               SoNumber = g.Key.SoNumber,
                               CustomerPoNumber = g.Key.CustomerPoNumber,
                               AltItemNumber = g.Key.AltItemNumber,
                               ItemId = g.Key.ItemId,
                               UnitId = g.Key.UnitId,
                               UnitName = g.Key.UnitName,
                               Qty = g.Sum(p => p.a.Qty),
                               Sequence = g.Key.Sequence,
                               LotNumber1 = g.Key.LotNumber1,
                               LotNumber2 = g.Key.LotNumber2,
                               LotDate = (DateTime?)g.Key.LotDate,

                               UserName = g.Key.userName
                           }).ToList();

                entity = sql_get1.ToList();      //赋值，后续使用

                //2. 用出库订单的款号 SO PO lot1 lot2 等 拉取库存对应的数据
                var sql = from a1 in sql_get1
                          join a in idal.IHuDetailDAL.SelectAll()
                          on new { X = a1.ClientId, A = a1.AltItemNumber, B = a1.ItemId, C = a1.UnitName, D = a1.UnitId, E = (a1.LotNumber1 ?? ""), F = (a1.LotNumber2 ?? ""), H = a1.SoNumber, I = a1.CustomerPoNumber } equals new { X = a.ClientId, A = a.AltItemNumber, B = a.ItemId, C = a.UnitName, D = a.UnitId, E = (a.LotNumber1 ?? ""), F = (a.LotNumber2 ?? ""), H = a.SoNumber, I = a.CustomerPoNumber }
                          join b in idal.IHuMasterDAL.SelectAll()
                                          on new { a.HuId, a.WhCode }
                                      equals new { b.HuId, b.WhCode }
                          join c in idal.IWhLocationDAL.SelectAll()
                                on new { b.Location, b.WhCode }
                            equals new { Location = c.LocationId, c.WhCode }
                          where
                            b.Type == "M" &&
                            b.Status == "A" &&
                            c.LocationTypeId == 1 &&
                            (a.Qty - (a.PlanQty ?? 0)) > 0
                          select new HuDetailResult
                          {
                              Id = a.Id,
                              HuId = a.HuId,
                              WhCode = a.WhCode,
                              ClientId = a.ClientId,
                              ClientCode = a.ClientCode,
                              SoNumber = a.SoNumber,
                              CustomerPoNumber = a.CustomerPoNumber,
                              AltItemNumber = a.AltItemNumber,
                              ReceiptDate = a.ReceiptDate,
                              PlanQty = a.PlanQty,
                              Qty = a.Qty,
                              ItemId = a.ItemId,
                              UnitId = a.UnitId,
                              UnitName = a.UnitName,
                              Height = a.Height,
                              Length = a.Length,
                              Weight = a.Weight,
                              Width = a.Width,
                              LotNumber1 = a.LotNumber1,
                              LotNumber2 = a.LotNumber2,
                              LotDate = a.LotDate,
                              Location = b.Location,
                              LocationTypeId = c.LocationTypeId,
                              LocationTypeDetailId = ((c.LocationTypeDetailId ?? 0) == 0 ? 99 : c.LocationTypeDetailId)
                          };
                ListIQueryable = sql.Distinct();
                List = ListIQueryable.ToList();

                if (List.Count == 0)
                {
                    result = "释放有误库存不足，请检查托盘状态及库位、款号、属性123及Lot是否一致！";
                }

                if (result != "")
                {
                    checkReleaseLoadResult.CheckStatusResult = ReleaseLoadDetailAdd(loadId, whCode, mark);
                    checkReleaseLoadResult.Result = result;
                    return checkReleaseLoadResult;
                }

                //得到Load释放的 指定托盘表
                List<LoadHuIdExtend> loadHuIdList = idal.ILoadHuIdExtendDAL.SelectBy(u => u.WhCode == whCode && u.LoadId == loadId);
                if (loadHuIdList.Count > 0)
                {
                    string[] huid = (from a in loadHuIdList select a.HuId).ToArray();
                    List = List.Where(u => huid.Contains(u.HuId)).ToList();

                    if (List.Count == 0)
                    {
                        result = "指定托盘库存不足，请检查托盘状态及库位、款号及Lot是否一致！";
                    }
                }
                if (result != "")
                {
                    checkReleaseLoadResult.CheckStatusResult = ReleaseLoadDetailAdd(loadId, whCode, mark);
                    checkReleaseLoadResult.Result = result;
                    return checkReleaseLoadResult;
                }

                //3. 聚合 通过订单查询出的库存 的数量
                var sql1 = from hudetail in List
                           group hudetail by new
                           {
                               hudetail.WhCode,
                               hudetail.SoNumber,
                               hudetail.CustomerPoNumber,
                               hudetail.AltItemNumber,
                               hudetail.ItemId,
                               hudetail.UnitId,
                               hudetail.UnitName,
                               hudetail.LotNumber1,
                               hudetail.LotNumber2,
                               hudetail.LotDate
                           } into g
                           select new HuDetailResult
                           {
                               WhCode = g.Key.WhCode,
                               SoNumber = g.Key.SoNumber,
                               CustomerPoNumber = g.Key.CustomerPoNumber,
                               AltItemNumber = g.Key.AltItemNumber,
                               ItemId = g.Key.ItemId,
                               UnitId = g.Key.UnitId,
                               UnitName = g.Key.UnitName,
                               LotNumber1 = g.Key.LotNumber1,
                               LotNumber2 = g.Key.LotNumber2,
                               LotDate = g.Key.LotDate,
                               Qty = g.Sum(p => p.Qty - (p.PlanQty ?? 0))
                           };

                List<HuDetailResult> ListWhere = sql1.ToList();
                foreach (var item in entity)
                {
                    List<HuDetailResult> huDetailResultList = ListWhere.Where(u => u.WhCode == item.WhCode && u.SoNumber == item.SoNumber && u.CustomerPoNumber == item.CustomerPoNumber && u.AltItemNumber == item.AltItemNumber && u.ItemId == item.ItemId && u.UnitId == item.UnitId && u.UnitName == item.UnitName &&
                   (u.LotNumber1 == item.LotNumber1 || (u.LotNumber1 == null ? "" : u.LotNumber1) == (item.LotNumber1 == null ? "" : item.LotNumber1))
                    &&
                   (u.LotNumber2 == item.LotNumber2 || (u.LotNumber2 == null ? "" : u.LotNumber2) == (item.LotNumber2 == null ? "" : item.LotNumber2))).ToList();

                    if (huDetailResultList.Count == 0)
                    {
                        result += "SKU:" + item.AltItemNumber + "库存不足，请检查SO、PO、款号、属性123及Lot是否一致！";
                    }
                    else
                    {
                        int? sumQty = huDetailResultList.Sum(u => u.Qty);

                        if (sumQty < item.Qty)
                        {
                            result += "SKU:" + item.AltItemNumber + "库存小于出货数量！";
                        }
                    }
                }
                if (result != "")
                {
                    checkReleaseLoadResult.CheckStatusResult = ReleaseLoadDetailAdd(loadId, whCode, mark);
                    result = "库存不足！" + result;
                }
                checkReleaseLoadResult.Result = result;
                checkReleaseLoadResult.ReleaseLoadList = entity1;
                checkReleaseLoadResult.HuDetailResultList = List;
            }
            #endregion


            #region 3. 释放流程为56 带有PO 条件

            if (mark == "5" || mark == "6")
            {
                //1.通过Load号   拉取 出库订单信息 聚合同款数量
                var sql_get1 = from a in idal.IOutBoundOrderDetailDAL.SelectAll()
                               join b in idal.IOutBoundOrderDAL.SelectAll() on new { OutBoundOrderId = a.OutBoundOrderId } equals new { OutBoundOrderId = b.Id }
                               join c in idal.ILoadDetailDAL.SelectAll() on new { OutBoundOrderId = a.OutBoundOrderId } equals new { OutBoundOrderId = (Int32)c.OutBoundOrderId }
                               join d in idal.ILoadMasterDAL.SelectAll() on new { LoadMasterId = (Int32)c.LoadMasterId } equals new { LoadMasterId = d.Id }
                               where d.LoadId == loadId && d.WhCode == whCode && a.DSFLag != 1
                               group new { a, d, b } by new
                               {
                                   a.WhCode,
                                   d.LoadId,
                                   b.ClientId,
                                   b.ClientCode,
                                   a.SoNumber,
                                   a.CustomerPoNumber,
                                   a.AltItemNumber,
                                   a.ItemId,
                                   a.UnitId,
                                   a.UnitName,
                                   a.LotNumber1,
                                   a.LotNumber2,
                                   a.LotDate,

                                   userName
                               } into g
                               select new ReleaseLoad
                               {
                                   WhCode = g.Key.WhCode,
                                   LoadId = g.Key.LoadId,
                                   ClientId = g.Key.ClientId,
                                   ClientCode = g.Key.ClientCode,
                                   SoNumber = g.Key.SoNumber,
                                   CustomerPoNumber = g.Key.CustomerPoNumber,
                                   AltItemNumber = g.Key.AltItemNumber,
                                   ItemId = g.Key.ItemId,
                                   UnitId = g.Key.UnitId,
                                   UnitName = g.Key.UnitName,
                                   Qty = g.Sum(p => p.a.Qty),
                                   LotNumber1 = g.Key.LotNumber1,
                                   LotNumber2 = g.Key.LotNumber2,
                                   LotDate = (DateTime?)g.Key.LotDate,

                                   UserName = g.Key.userName
                               };

                entity1 = (from a in idal.IOutBoundOrderDetailDAL.SelectAll()
                           join b in idal.IOutBoundOrderDAL.SelectAll() on new { OutBoundOrderId = a.OutBoundOrderId } equals new { OutBoundOrderId = b.Id }
                           join c in idal.ILoadDetailDAL.SelectAll() on new { OutBoundOrderId = a.OutBoundOrderId } equals new { OutBoundOrderId = (Int32)c.OutBoundOrderId }
                           join d in idal.ILoadMasterDAL.SelectAll() on new { LoadMasterId = (Int32)c.LoadMasterId } equals new { LoadMasterId = d.Id }
                           where d.LoadId == loadId && d.WhCode == whCode && a.DSFLag != 1
                           group new { a, d, b } by new
                           {
                               a.WhCode,
                               d.LoadId,
                               b.ClientId,
                               b.ClientCode,
                               a.SoNumber,
                               a.CustomerPoNumber,
                               a.AltItemNumber,
                               a.ItemId,
                               a.UnitId,
                               a.UnitName,
                               a.LotNumber1,
                               a.LotNumber2,
                               a.LotDate,

                               userName,
                               a.Sequence
                           } into g
                           select new ReleaseLoad
                           {
                               WhCode = g.Key.WhCode,
                               LoadId = g.Key.LoadId,
                               ClientId = g.Key.ClientId,
                               ClientCode = g.Key.ClientCode,
                               SoNumber = g.Key.SoNumber,
                               CustomerPoNumber = g.Key.CustomerPoNumber,
                               AltItemNumber = g.Key.AltItemNumber,
                               ItemId = g.Key.ItemId,
                               UnitId = g.Key.UnitId,
                               UnitName = g.Key.UnitName,
                               Qty = g.Sum(p => p.a.Qty),
                               Sequence = g.Key.Sequence,
                               LotNumber1 = g.Key.LotNumber1,
                               LotNumber2 = g.Key.LotNumber2,
                               LotDate = (DateTime?)g.Key.LotDate,

                               UserName = g.Key.userName
                           }).ToList();

                entity = sql_get1.ToList();      //赋值，后续使用

                //2. 用出库订单的款号 SO PO lot1 lot2 等 拉取库存对应的数据
                var sql = from a1 in sql_get1
                          join a in idal.IHuDetailDAL.SelectAll()
                          on new { X = a1.ClientId, A = a1.AltItemNumber, B = a1.ItemId, C = a1.UnitName, D = a1.UnitId, E = (a1.LotNumber1 ?? ""), F = (a1.LotNumber2 ?? ""), I = a1.CustomerPoNumber } equals new { X = a.ClientId, A = a.AltItemNumber, B = a.ItemId, C = a.UnitName, D = a.UnitId, E = (a.LotNumber1 ?? ""), F = (a.LotNumber2 ?? ""), I = a.CustomerPoNumber }
                          join b in idal.IHuMasterDAL.SelectAll()
                                          on new { a.HuId, a.WhCode }
                                      equals new { b.HuId, b.WhCode }
                          join c in idal.IWhLocationDAL.SelectAll()
                                on new { b.Location, b.WhCode }
                            equals new { Location = c.LocationId, c.WhCode }
                          where
                            b.Type == "M" &&
                            b.Status == "A" &&
                            c.LocationTypeId == 1 &&
                            (a.Qty - (a.PlanQty ?? 0)) > 0
                          select new HuDetailResult
                          {
                              Id = a.Id,
                              HuId = a.HuId,
                              WhCode = a.WhCode,
                              ClientId = a.ClientId,
                              ClientCode = a.ClientCode,
                              SoNumber = a.SoNumber,
                              CustomerPoNumber = a.CustomerPoNumber,
                              AltItemNumber = a.AltItemNumber,
                              ReceiptDate = a.ReceiptDate,
                              PlanQty = a.PlanQty,
                              Qty = a.Qty,
                              ItemId = a.ItemId,
                              UnitId = a.UnitId,
                              UnitName = a.UnitName,
                              Height = a.Height,
                              Length = a.Length,
                              Weight = a.Weight,
                              Width = a.Width,
                              LotNumber1 = a.LotNumber1,
                              LotNumber2 = a.LotNumber2,
                              LotDate = a.LotDate,
                              Location = b.Location,
                              LocationTypeId = c.LocationTypeId,
                              LocationTypeDetailId = ((c.LocationTypeDetailId ?? 0) == 0 ? 99 : c.LocationTypeDetailId)
                          };
                List = sql.Distinct().ToList();

                if (List.Count == 0)
                {
                    result = "释放有误库存不足，请检查托盘状态及库位、款号、属性123及Lot是否一致！";
                }

                if (result != "")
                {
                    checkReleaseLoadResult.CheckStatusResult = ReleaseLoadDetailAdd(loadId, whCode, mark);
                    checkReleaseLoadResult.Result = result;
                    return checkReleaseLoadResult;
                }

                //得到Load释放的 指定托盘表
                List<LoadHuIdExtend> loadHuIdList = idal.ILoadHuIdExtendDAL.SelectBy(u => u.WhCode == whCode && u.LoadId == loadId);
                if (loadHuIdList.Count > 0)
                {
                    string[] huid = (from a in loadHuIdList select a.HuId).ToArray();
                    List = List.Where(u => huid.Contains(u.HuId)).ToList();

                    if (List.Count == 0)
                    {
                        result = "指定托盘库存不足，请检查托盘状态及库位、款号及Lot是否一致！";
                    }
                }
                if (result != "")
                {
                    checkReleaseLoadResult.CheckStatusResult = ReleaseLoadDetailAdd(loadId, whCode, mark);
                    checkReleaseLoadResult.Result = result;
                    return checkReleaseLoadResult;
                }

                //3. 聚合 通过订单查询出的库存 的数量
                var sql1 = from hudetail in List
                           group hudetail by new
                           {
                               hudetail.WhCode,
                               hudetail.CustomerPoNumber,
                               hudetail.AltItemNumber,
                               hudetail.ItemId,
                               hudetail.UnitId,
                               hudetail.UnitName,
                               hudetail.LotNumber1,
                               hudetail.LotNumber2,
                               hudetail.LotDate
                           } into g
                           select new HuDetailResult
                           {
                               WhCode = g.Key.WhCode,
                               CustomerPoNumber = g.Key.CustomerPoNumber,
                               AltItemNumber = g.Key.AltItemNumber,
                               ItemId = g.Key.ItemId,
                               UnitId = g.Key.UnitId,
                               UnitName = g.Key.UnitName,
                               LotNumber1 = g.Key.LotNumber1,
                               LotNumber2 = g.Key.LotNumber2,
                               LotDate = g.Key.LotDate,
                               Qty = g.Sum(p => p.Qty - (p.PlanQty ?? 0))
                           };

                List<HuDetailResult> ListWhere = sql1.ToList();
                foreach (var item in entity)
                {
                    List<HuDetailResult> huDetailResultList = ListWhere.Where(u => u.WhCode == item.WhCode && u.CustomerPoNumber == item.CustomerPoNumber && u.AltItemNumber == item.AltItemNumber && u.ItemId == item.ItemId && u.UnitId == item.UnitId && u.UnitName == item.UnitName &&
                   (u.LotNumber1 == item.LotNumber1 || (u.LotNumber1 == null ? "" : u.LotNumber1) == (item.LotNumber1 == null ? "" : item.LotNumber1))
                    &&
                   (u.LotNumber2 == item.LotNumber2 || (u.LotNumber2 == null ? "" : u.LotNumber2) == (item.LotNumber2 == null ? "" : item.LotNumber2))).ToList();

                    if (huDetailResultList.Count == 0)
                    {
                        result += "SKU:" + item.AltItemNumber + "库存不足，请检查PO、款号及Lot是否一致！";
                    }
                    else
                    {
                        int? sumQty = huDetailResultList.Sum(u => u.Qty);

                        if (sumQty < item.Qty)
                        {
                            result += "SKU:" + item.AltItemNumber + "库存小于出货数量！";
                        }
                    }
                }
                if (result != "")
                {
                    checkReleaseLoadResult.CheckStatusResult = ReleaseLoadDetailAdd(loadId, whCode, mark);
                    result = "库存不足！" + result;
                }
                checkReleaseLoadResult.Result = result;
                checkReleaseLoadResult.ReleaseLoadList = entity1;
                checkReleaseLoadResult.HuDetailResultList = List;
            }
            #endregion


            #region 4 释放流程为7和12 无视SO PO的优先捡货区的释放条件
            if (mark == "7" || mark == "12")
            {
                //1.通过Load号   拉取 出库订单信息 聚合同款数量
                var sql_get1 = from a in idal.IOutBoundOrderDetailDAL.SelectAll()
                               join b in idal.IOutBoundOrderDAL.SelectAll() on new { OutBoundOrderId = a.OutBoundOrderId } equals new { OutBoundOrderId = b.Id }
                               join c in idal.ILoadDetailDAL.SelectAll() on new { OutBoundOrderId = a.OutBoundOrderId } equals new { OutBoundOrderId = (Int32)c.OutBoundOrderId }
                               join d in idal.ILoadMasterDAL.SelectAll() on new { LoadMasterId = (Int32)c.LoadMasterId } equals new { LoadMasterId = d.Id }
                               where d.LoadId == loadId && d.WhCode == whCode && a.DSFLag != 1
                               group new { a, d, b } by new
                               {
                                   a.WhCode,
                                   d.LoadId,
                                   b.ClientId,
                                   b.ClientCode,
                                   a.SoNumber,
                                   a.CustomerPoNumber,
                                   a.AltItemNumber,
                                   a.ItemId,
                                   a.UnitId,
                                   a.UnitName,
                                   a.LotNumber1,
                                   a.LotNumber2,
                                   a.LotDate,
                                   userName
                               } into g
                               select new ReleaseLoad
                               {
                                   WhCode = g.Key.WhCode,
                                   LoadId = g.Key.LoadId,
                                   ClientId = g.Key.ClientId,
                                   ClientCode = g.Key.ClientCode,
                                   SoNumber = g.Key.SoNumber,
                                   CustomerPoNumber = g.Key.CustomerPoNumber,
                                   AltItemNumber = g.Key.AltItemNumber,
                                   ItemId = g.Key.ItemId,
                                   UnitId = g.Key.UnitId,
                                   UnitName = g.Key.UnitName,
                                   Qty = g.Sum(p => p.a.Qty),
                                   LotNumber1 = g.Key.LotNumber1,
                                   LotNumber2 = g.Key.LotNumber2,
                                   LotDate = (DateTime?)g.Key.LotDate,

                                   UserName = g.Key.userName
                               };
                entity1 = (from a in idal.IOutBoundOrderDetailDAL.SelectAll()
                           join b in idal.IOutBoundOrderDAL.SelectAll() on new { OutBoundOrderId = a.OutBoundOrderId } equals new { OutBoundOrderId = b.Id }
                           join c in idal.ILoadDetailDAL.SelectAll() on new { OutBoundOrderId = a.OutBoundOrderId } equals new { OutBoundOrderId = (Int32)c.OutBoundOrderId }
                           join d in idal.ILoadMasterDAL.SelectAll() on new { LoadMasterId = (Int32)c.LoadMasterId } equals new { LoadMasterId = d.Id }
                           where d.LoadId == loadId && d.WhCode == whCode && a.DSFLag != 1
                           group new { a, d, b } by new
                           {
                               a.WhCode,
                               d.LoadId,
                               b.ClientId,
                               b.ClientCode,
                               a.SoNumber,
                               a.CustomerPoNumber,
                               a.AltItemNumber,
                               a.ItemId,
                               a.UnitId,
                               a.UnitName,
                               a.LotNumber1,
                               a.LotNumber2,
                               a.LotDate,

                               userName,
                               a.Sequence
                           } into g
                           select new ReleaseLoad
                           {
                               WhCode = g.Key.WhCode,
                               LoadId = g.Key.LoadId,
                               ClientId = g.Key.ClientId,
                               ClientCode = g.Key.ClientCode,
                               SoNumber = g.Key.SoNumber,
                               CustomerPoNumber = g.Key.CustomerPoNumber,
                               AltItemNumber = g.Key.AltItemNumber,
                               ItemId = g.Key.ItemId,
                               UnitId = g.Key.UnitId,
                               UnitName = g.Key.UnitName,
                               Qty = g.Sum(p => p.a.Qty),
                               Sequence = g.Key.Sequence,
                               LotNumber1 = g.Key.LotNumber1,
                               LotNumber2 = g.Key.LotNumber2,
                               LotDate = (DateTime?)g.Key.LotDate,

                               UserName = g.Key.userName
                           }).ToList();

                entity = sql_get1.ToList();     //赋值，后续使用

                //2. 用出库订单的款号 lot1 lot2 等 拉取库存对应的数据
                var sql = from a1 in sql_get1
                          join a in idal.IHuDetailDAL.SelectAll()
                          on new { X = a1.ClientId, A = a1.AltItemNumber, B = a1.ItemId, C = a1.UnitName, D = a1.UnitId, E = (a1.LotNumber1 ?? ""), F = (a1.LotNumber2 ?? "") } equals new { X = a.ClientId, A = a.AltItemNumber, B = a.ItemId, C = a.UnitName, D = a.UnitId, E = (a.LotNumber1 ?? ""), F = (a.LotNumber2 ?? "") }
                          join b in idal.IHuMasterDAL.SelectAll()
                                          on new { a.HuId, a.WhCode }
                                      equals new { b.HuId, b.WhCode }
                          join c in idal.IWhLocationDAL.SelectAll()
                                on new { b.Location, b.WhCode }
                         equals new { Location = c.LocationId, c.WhCode }
                          where
                            b.Type == "M" &&
                            b.Status == "A" &&
                            c.LocationTypeId == 1 &&
                            (a.Qty - (a.PlanQty ?? 0)) > 0
                          select new HuDetailResult
                          {
                              Id = a.Id,
                              HuId = a.HuId,
                              WhCode = a.WhCode,
                              ClientId = a.ClientId,
                              ClientCode = a.ClientCode,
                              SoNumber = a.SoNumber,
                              CustomerPoNumber = a.CustomerPoNumber,
                              AltItemNumber = a.AltItemNumber,
                              ReceiptDate = a.ReceiptDate,
                              PlanQty = a.PlanQty ?? 0,
                              Qty = a.Qty,
                              ItemId = a.ItemId,
                              UnitId = a.UnitId,
                              UnitName = a.UnitName,
                              Height = a.Height,
                              Length = a.Length,
                              Weight = a.Weight,
                              Width = a.Width,
                              LotNumber1 = a.LotNumber1,
                              LotNumber2 = a.LotNumber2,
                              LotDate = a.LotDate,
                              Location = b.Location,
                              LocationTypeId = c.LocationTypeId,
                              LocationTypeDetailId = ((c.LocationTypeDetailId ?? 0) == 0 ? 99 : c.LocationTypeDetailId)
                          };
                List = sql.Distinct().ToList();

                //2.1 如果没有库存 提示报错
                if (List.Count == 0)
                {
                    result = "释放有误库存不足，请检查托盘状态及库位、款号、属性123及Lot是否一致！";
                }

                if (result != "")
                {
                    checkReleaseLoadResult.CheckStatusResult = ReleaseLoadDetailAdd(loadId, whCode, mark);
                    checkReleaseLoadResult.Result = result;
                    return checkReleaseLoadResult;
                }

                //得到Load释放的 指定托盘表
                List<LoadHuIdExtend> loadHuIdList = idal.ILoadHuIdExtendDAL.SelectBy(u => u.WhCode == whCode && u.LoadId == loadId);
                if (loadHuIdList.Count > 0)
                {
                    string[] huid = (from a in loadHuIdList select a.HuId).ToArray();
                    List = List.Where(u => huid.Contains(u.HuId)).ToList();

                    if (List.Count == 0)
                    {
                        result = "指定托盘库存不足，请检查托盘状态及库位、款号及Lot是否一致！";
                    }
                }
                if (result != "")
                {
                    checkReleaseLoadResult.CheckStatusResult = ReleaseLoadDetailAdd(loadId, whCode, mark);
                    checkReleaseLoadResult.Result = result;
                    return checkReleaseLoadResult;
                }

                //3. 聚合 通过订单查询出的库存 的数量
                var sql1 = from hudetail in List
                           group hudetail by new
                           {
                               hudetail.WhCode,
                               hudetail.AltItemNumber,
                               hudetail.ItemId,
                               hudetail.UnitId,
                               hudetail.UnitName,
                               hudetail.LotNumber1,
                               hudetail.LotNumber2,
                               hudetail.LotDate
                           } into g
                           select new HuDetailResult
                           {
                               WhCode = g.Key.WhCode,
                               AltItemNumber = g.Key.AltItemNumber,
                               ItemId = g.Key.ItemId,
                               UnitId = g.Key.UnitId,
                               UnitName = g.Key.UnitName,
                               LotNumber1 = g.Key.LotNumber1,
                               LotNumber2 = g.Key.LotNumber2,
                               LotDate = g.Key.LotDate,
                               Qty = g.Sum(p => p.Qty - (p.PlanQty ?? 0))
                           };

                List<HuDetailResult> ListWhere = sql1.ToList(); //为什么没有直接在上面toList 是因为 方便调试

                foreach (var item in entity)    //循环出库订单数据
                {
                    //同一SKU lot条件下 数量是否满足
                    List<HuDetailResult> huDetailResultList = ListWhere.Where(u => u.WhCode == item.WhCode && u.AltItemNumber == item.AltItemNumber && u.ItemId == item.ItemId && u.UnitId == item.UnitId && u.UnitName == item.UnitName &&
                    (u.LotNumber1 == item.LotNumber1 || (u.LotNumber1 == null ? "" : u.LotNumber1) == (item.LotNumber1 == null ? "" : item.LotNumber1))
                    &&
                   (u.LotNumber2 == item.LotNumber2 || (u.LotNumber2 == null ? "" : u.LotNumber2) == (item.LotNumber2 == null ? "" : item.LotNumber2))).ToList();

                    if (huDetailResultList.Count == 0)
                    {
                        result += "SKU:" + item.AltItemNumber + "库存不足，请检查款号及Lot是否一致！";
                    }
                    else
                    {
                        int? sumQty = huDetailResultList.Sum(u => u.Qty);

                        if (sumQty < item.Qty)
                        {
                            result += "SKU:" + item.AltItemNumber + "库存小于出货数量！";
                        }
                    }
                }
                if (result != "")
                {
                    checkReleaseLoadResult.CheckStatusResult = ReleaseLoadDetailAdd(loadId, whCode, mark);
                    result = "库存不足！" + result;
                }
                checkReleaseLoadResult.Result = result;
                checkReleaseLoadResult.ReleaseLoadList = entity1;
                checkReleaseLoadResult.HuDetailResultList = List;
            }
            #endregion


            #region 5 释放流程为9 验证扫描箱号(SOPO先进先出) 这里只是验证箱号部分

            if (mark == "9" && result == "")
            {
                //checkReleaseLoadResult.Result = result;
                //checkReleaseLoadResult.ReleaseLoadList = entity1;
                string res = CheckSerialNumberIn(loadId, whCode, userName, checkReleaseLoadResult.ReleaseLoadList, ListIQueryable);
                if (res == "") UrlEdiTaskInsert(loadId, whCode, userName);
                checkReleaseLoadResult.Result = res;
                // checkReleaseLoadResult.HuDetailResultList = List;
            }

            #endregion


            #region 6. 释放流程为11 无视SO PO、LotNumber、LotDate 指定库位ROC1 的释放条件
            if (mark == "11")
            {
                //1.通过Load号   拉取 出库订单信息 聚合同款数量
                var sql_get1 = from a in idal.IOutBoundOrderDetailDAL.SelectAll()
                               join b in idal.IOutBoundOrderDAL.SelectAll() on new { OutBoundOrderId = a.OutBoundOrderId } equals new { OutBoundOrderId = b.Id }
                               join c in idal.ILoadDetailDAL.SelectAll() on new { OutBoundOrderId = a.OutBoundOrderId } equals new { OutBoundOrderId = (Int32)c.OutBoundOrderId }
                               join d in idal.ILoadMasterDAL.SelectAll() on new { LoadMasterId = (Int32)c.LoadMasterId } equals new { LoadMasterId = d.Id }
                               where d.LoadId == loadId && d.WhCode == whCode && a.DSFLag != 1
                               group new { a, d, b } by new
                               {
                                   a.WhCode,
                                   d.LoadId,
                                   b.ClientId,
                                   b.ClientCode,
                                   a.SoNumber,
                                   a.CustomerPoNumber,
                                   a.AltItemNumber,
                                   a.ItemId,
                                   a.UnitId,
                                   a.UnitName,
                                   a.LotNumber1,
                                   a.LotNumber2,
                                   a.LotDate,
                                   userName
                               } into g
                               select new ReleaseLoad
                               {
                                   WhCode = g.Key.WhCode,
                                   LoadId = g.Key.LoadId,
                                   ClientId = g.Key.ClientId,
                                   ClientCode = g.Key.ClientCode,
                                   SoNumber = g.Key.SoNumber,
                                   CustomerPoNumber = g.Key.CustomerPoNumber,
                                   AltItemNumber = g.Key.AltItemNumber,
                                   ItemId = g.Key.ItemId,
                                   UnitId = g.Key.UnitId,
                                   UnitName = g.Key.UnitName,
                                   Qty = g.Sum(p => p.a.Qty),
                                   LotNumber1 = g.Key.LotNumber1,
                                   LotNumber2 = g.Key.LotNumber2,
                                   LotDate = (DateTime?)g.Key.LotDate,
                                   UserName = g.Key.userName
                               };

                entity1 = (from a in idal.IOutBoundOrderDetailDAL.SelectAll()
                           join b in idal.IOutBoundOrderDAL.SelectAll() on new { OutBoundOrderId = a.OutBoundOrderId } equals new { OutBoundOrderId = b.Id }
                           join c in idal.ILoadDetailDAL.SelectAll() on new { OutBoundOrderId = a.OutBoundOrderId } equals new { OutBoundOrderId = (Int32)c.OutBoundOrderId }
                           join d in idal.ILoadMasterDAL.SelectAll() on new { LoadMasterId = (Int32)c.LoadMasterId } equals new { LoadMasterId = d.Id }
                           where d.LoadId == loadId && d.WhCode == whCode && a.DSFLag != 1
                           group new { a, d, b } by new
                           {
                               a.WhCode,
                               d.LoadId,
                               b.ClientId,
                               b.ClientCode,
                               a.SoNumber,
                               a.CustomerPoNumber,
                               a.AltItemNumber,
                               a.ItemId,
                               a.UnitId,
                               a.UnitName,
                               a.LotNumber1,
                               a.LotNumber2,
                               a.LotDate,
                               userName,
                               a.Sequence
                           } into g
                           select new ReleaseLoad
                           {
                               WhCode = g.Key.WhCode,
                               LoadId = g.Key.LoadId,
                               ClientId = g.Key.ClientId,
                               ClientCode = g.Key.ClientCode,
                               SoNumber = g.Key.SoNumber,
                               CustomerPoNumber = g.Key.CustomerPoNumber,
                               AltItemNumber = g.Key.AltItemNumber,
                               ItemId = g.Key.ItemId,
                               UnitId = g.Key.UnitId,
                               UnitName = g.Key.UnitName,
                               Qty = g.Sum(p => p.a.Qty),
                               Sequence = g.Key.Sequence,
                               LotNumber1 = g.Key.LotNumber1,
                               LotNumber2 = g.Key.LotNumber2,
                               LotDate = (DateTime?)g.Key.LotDate,
                               UserName = g.Key.userName
                           }).ToList();

                entity = sql_get1.ToList();     //赋值，后续使用

                //2. 用出库订单的款号 等 拉取库存对应的数据
                var sql = from a1 in sql_get1
                          join a in idal.IHuDetailDAL.SelectAll()
                          on new { X = a1.ClientId, A = a1.AltItemNumber, B = a1.ItemId, C = a1.UnitName, D = a1.UnitId } equals new { X = a.ClientId, A = a.AltItemNumber, B = a.ItemId, C = a.UnitName, D = a.UnitId }
                          join b in idal.IHuMasterDAL.SelectAll()
                                          on new { a.HuId, a.WhCode }
                                      equals new { b.HuId, b.WhCode }
                          join c in idal.IWhLocationDAL.SelectAll()
                                on new { b.Location, b.WhCode }
                         equals new { Location = c.LocationId, c.WhCode }
                          where
                            b.Type == "M" &&
                            b.Status == "A" &&
                            b.Location == "ROC1" &&
                            (a.Qty - (a.PlanQty ?? 0)) > 0
                          select new HuDetailResult
                          {
                              Id = a.Id,
                              HuId = a.HuId,
                              WhCode = a.WhCode,
                              ClientId = a.ClientId,
                              ClientCode = a.ClientCode,
                              SoNumber = a.SoNumber,
                              CustomerPoNumber = a.CustomerPoNumber,
                              AltItemNumber = a.AltItemNumber,
                              ReceiptDate = a.ReceiptDate,
                              PlanQty = a.PlanQty ?? 0,
                              Qty = a.Qty,
                              ItemId = a.ItemId,
                              UnitId = a.UnitId,
                              UnitName = a.UnitName,
                              Height = a.Height,
                              Length = a.Length,
                              Weight = a.Weight,
                              Width = a.Width,
                              LotNumber1 = a.LotNumber1,
                              LotNumber2 = a.LotNumber2,
                              LotDate = a.LotDate,
                              Location = b.Location,
                              LocationTypeId = c.LocationTypeId,
                              LocationTypeDetailId = ((c.LocationTypeDetailId ?? 0) == 0 ? 99 : c.LocationTypeDetailId)
                          };
                List = sql.Distinct().ToList();

                //2.1 如果没有库存 提示报错
                if (List.Count == 0)
                {
                    result = "释放有误库存不足，请检查托盘状态、库位、款号、属性123是否一致！";
                }

                if (result != "")
                {
                    checkReleaseLoadResult.CheckStatusResult = ReleaseLoadDetailAdd(loadId, whCode, mark);
                    checkReleaseLoadResult.Result = result;
                    return checkReleaseLoadResult;
                }

                //得到Load释放的 指定托盘表
                List<LoadHuIdExtend> loadHuIdList = idal.ILoadHuIdExtendDAL.SelectBy(u => u.WhCode == whCode && u.LoadId == loadId);
                if (loadHuIdList.Count > 0)
                {
                    string[] huid = (from a in loadHuIdList select a.HuId).ToArray();
                    List = List.Where(u => huid.Contains(u.HuId)).ToList();

                    if (List.Count == 0)
                    {
                        result = "指定托盘库存不足，请检查托盘状态及库位、款号、属性123是否一致！";
                    }
                }
                if (result != "")
                {
                    checkReleaseLoadResult.CheckStatusResult = ReleaseLoadDetailAdd(loadId, whCode, mark);
                    checkReleaseLoadResult.Result = result;
                    return checkReleaseLoadResult;
                }

                //3. 聚合 通过订单查询出的库存 的数量
                var sql1 = from hudetail in List
                           group hudetail by new
                           {
                               hudetail.WhCode,
                               hudetail.AltItemNumber,
                               hudetail.ItemId,
                               hudetail.UnitId,
                               hudetail.UnitName,
                               hudetail.LotNumber1,
                               hudetail.LotNumber2,
                               hudetail.LotDate
                           } into g
                           select new HuDetailResult
                           {
                               WhCode = g.Key.WhCode,
                               AltItemNumber = g.Key.AltItemNumber,
                               ItemId = g.Key.ItemId,
                               UnitId = g.Key.UnitId,
                               UnitName = g.Key.UnitName,
                               LotNumber1 = g.Key.LotNumber1,
                               LotNumber2 = g.Key.LotNumber2,
                               LotDate = g.Key.LotDate,
                               Qty = g.Sum(p => p.Qty - (p.PlanQty ?? 0))
                           };

                List<HuDetailResult> ListWhere = sql1.ToList(); //为什么没有直接在上面toList 是因为 方便调试

                foreach (var item in entity)    //循环出库订单数据
                {
                    //同一SKU lot条件下 数量是否满足
                    List<HuDetailResult> huDetailResultList = ListWhere.Where(u => u.WhCode == item.WhCode && u.AltItemNumber == item.AltItemNumber && u.ItemId == item.ItemId && u.UnitId == item.UnitId && u.UnitName == item.UnitName).ToList();

                    if (huDetailResultList.Count == 0)
                    {
                        result += "SKU:" + item.AltItemNumber + "库存不足，请检查款号是否一致！";
                    }
                    else
                    {
                        int? sumQty = huDetailResultList.Sum(u => u.Qty);

                        if (sumQty < item.Qty)
                        {
                            result += "SKU:" + item.AltItemNumber + "库存小于出货数量！";
                        }
                    }
                }

                if (result != "")
                {
                    checkReleaseLoadResult.CheckStatusResult = ReleaseLoadDetailAdd(loadId, whCode, mark);
                    result = "库存不足！" + result;
                }
                checkReleaseLoadResult.Result = result;
                checkReleaseLoadResult.ReleaseLoadList = entity1;
                checkReleaseLoadResult.HuDetailResultList = List;
            }
            #endregion


            return checkReleaseLoadResult;
        }


        public string CheckSerialNumberIn(string loadId, string whCode, string userName, List<ReleaseLoad> ReleaseLoadList, IQueryable<HuDetailResult> ListIQueryable)
        {


            var huSql = from a in ListIQueryable
                        join b in idal.ISerialNumberInDAL.SelectAll() on new { a.WhCode, a.ClientCode, a.SoNumber, a.CustomerPoNumber, a.HuId, a.ItemId }
                        equals new { b.WhCode, b.ClientCode, b.SoNumber, b.CustomerPoNumber, b.HuId, b.ItemId }
                        where b.ToOutStatus == 1
                        select new
                        {
                            Id = b.Id,
                            WhCode = b.WhCode,
                            LoadId = loadId,
                            ClientId = b.ClientId,
                            ClientCode = b.ClientCode,
                            SoNumber = b.SoNumber,
                            CustomerPoNumber = b.CustomerPoNumber,
                            HuId = b.HuId,
                            ItemId = b.ItemId,
                            AltItemNumber = b.AltItemNumber,
                            CartonId = b.CartonId,
                            Length = b.Length,
                            Width = b.Width,
                            Height = b.Height,
                            Weight = b.Weight,
                            CreateUser = userName
                        };
            int huQty = huSql.Count();
            int loadQty = (Int32)ReleaseLoadList.Sum(s => s.Qty);

            List<HuDetailResult> List = ListIQueryable.ToList();

            List<SerialNumberOut> SNList = new List<SerialNumberOut>();
            foreach (var item in huSql.OrderBy(u => u.Id).ToList())
            {
                SerialNumberOut SN = new SerialNumberOut();
                SN.WhCode = item.WhCode;
                SN.LoadId = item.LoadId;
                SN.ClientId = item.ClientId;
                SN.ClientCode = item.ClientCode;
                SN.CustomerPoNumber = item.CustomerPoNumber;
                SN.HuId = item.HuId;
                SN.SoNumber = item.SoNumber;
                SN.ItemId = item.ItemId;
                SN.AltItemNumber = item.AltItemNumber;
                SN.CartonId = item.CartonId;
                SN.Length = item.Length;
                SN.Width = item.Width;
                SN.Height = item.Height;
                SN.Weight = item.Weight;
                SN.CreateUser = item.CreateUser;
                SN.CreateDate = DateTime.Now;
                SNList.Add(SN);
            }
            ////插入采集箱号表
            //SerialNumberOut serial = new SerialNumberOut();
            //serial.WhCode = huDetail.WhCode;
            //serial.ClientId = huDetail.ClientId;
            //serial.LoadId = LoadId;
            //serial.ClientCode = huDetail.ClientCode;
            //serial.SoNumber = huDetail.SoNumber;
            //serial.CustomerPoNumber = huDetail.CustomerPoNumber;
            //serial.AltItemNumber = huDetail.AltItemNumber;
            //serial.ItemId = huDetail.ItemId;
            //serial.HuId = huDetail.HuId;
            //serial.Length = huDetail.Length;
            //serial.Width = huDetail.Width;
            //serial.Height = huDetail.Height;
            //serial.Weight = huDetail.Weight;
            //serial.LotNumber1 = huDetail.LotNumber1;
            //serial.LotNumber2 = huDetail.LotNumber2;
            //serial.LotDate = huDetail.LotDate;
            //serial.CreateUser = userName;
            //serial.CreateDate = DateTime.Now;
            //serial.CartonId = item.CartonId;

            if (loadQty > huQty)
                return "LOAD共:" + loadQty + " 可用扫描数:" + huQty;
            else if (loadQty == huQty)
                return SerialNumberInToOutTotal(loadId, whCode, userName, List, SNList);
            else
                return SerialNumberInToOut(loadId, whCode, userName, ReleaseLoadList, List, SNList);
        }


        public string SerialNumberInToOutTotal(string loadId, string whCode, string userName, List<HuDetailResult> List, List<SerialNumberOut> serialNumberOutList)
        {
            SerialNumberIn editStatus = new SerialNumberIn();
            editStatus.ToOutStatus = 0;
            editStatus.UpdateUser = userName;
            editStatus.UpdateDate = DateTime.Now;
            foreach (var item in List)
            {
                idal.ISerialNumberInDAL.UpdateBy(editStatus, u => u.WhCode == whCode && u.ClientCode == item.ClientCode
                    && u.HuId == item.HuId && u.ItemId == item.ItemId && u.SoNumber == item.SoNumber && u.CustomerPoNumber == item.CustomerPoNumber, new string[] { "ToOutStatus", "UpdateUser", "UpdateDate" });

            }
            idal.ISerialNumberOutDAL.Add(serialNumberOutList);
            idal.SaveChanges();
            return "";
        }


        public string SerialNumberInToOut(string loadId, string whCode, string userName, List<ReleaseLoad> ReleaseLoadList, List<HuDetailResult> List, List<SerialNumberOut> serialNumberOutList)
        {

            List<SerialNumberOut> snListTotal = new List<SerialNumberOut>();


            List<PickTaskDetail> checkHuDetailListByOrder = new List<PickTaskDetail>();
            foreach (var item in ReleaseLoadList)
            {

                List<HuDetailResult> ListWhere = new List<HuDetailResult>();
                ListWhere = List.Where(u => u.SoNumber == item.SoNumber && u.CustomerPoNumber == item.CustomerPoNumber && u.AltItemNumber == item.AltItemNumber && u.ItemId == item.ItemId && u.UnitName == item.UnitName && u.UnitId == item.UnitId && u.LotNumber1 == item.LotNumber1 && u.LotNumber2 == item.LotNumber2 &&
                u.LotDate == item.LotDate).OrderBy(u => u.ReceiptDate).ToList();

                int qtyResult = (Int32)item.Qty;  //得到出库订单的数量
                foreach (var sqlDetail in ListWhere)    //得到经过筛选后的库存
                {
                    if (qtyResult <= 0)
                    {
                        break;
                    }
                    else
                    {
                        int? sqlDetailQty = sqlDetail.Qty;
                        if (checkHuDetailListByOrder.Where(u => u.HuDetailId == sqlDetail.Id).Count() > 0)
                        {
                            int getQty = checkHuDetailListByOrder.Where(u => u.HuDetailId == sqlDetail.Id).Sum(u => u.Qty);
                            sqlDetailQty = sqlDetail.Qty - getQty;
                        }
                        if (sqlDetailQty <= 0)
                        {
                            continue;
                        }


                        PickTaskDetail detail = new PickTaskDetail();
                        detail.HuDetailId = sqlDetail.Id;
                        detail.HuId = sqlDetail.HuId;

                        if (qtyResult >= sqlDetailQty - (sqlDetail.PlanQty ?? 0))
                        {
                            detail.Qty = (Int32)sqlDetailQty - (sqlDetail.PlanQty ?? 0);  //释放数量
                            qtyResult = qtyResult - detail.Qty;    //剩余的未释放数量
                        }
                        else
                        {
                            detail.Qty = qtyResult;
                            qtyResult = 0;
                        }
                        List<SerialNumberOut> snList = (serialNumberOutList.Where(u => u.WhCode == whCode && u.ClientCode == sqlDetail.ClientCode
                             && u.HuId == sqlDetail.HuId && u.SoNumber == sqlDetail.SoNumber && u.CustomerPoNumber == sqlDetail.CustomerPoNumber).Take(detail.Qty)).ToList();
                        snListTotal.AddRange(snList);

                        serialNumberOutList.RemoveAll(u => snList.Select(x => x.CartonId).Contains(u.CartonId));

                        checkHuDetailListByOrder.Add(detail);


                    }
                }
            }


            SerialNumberIn editStatus = new SerialNumberIn();
            editStatus.ToOutStatus = 0;
            editStatus.UpdateUser = userName;
            editStatus.UpdateDate = DateTime.Now;
            foreach (var item in snListTotal)
            {
                idal.ISerialNumberInDAL.UpdateBy(editStatus, u => u.WhCode == whCode && u.ClientCode == item.ClientCode
                    && u.HuId == item.HuId && u.SoNumber == item.SoNumber && u.CustomerPoNumber == item.CustomerPoNumber && u.CartonId == item.CartonId, new string[] { "ToOutStatus", "UpdateUser", "UpdateDate" });
            }
            idal.ISerialNumberOutDAL.Add(snListTotal);
            idal.SaveChanges();
            return "";
        }


        public string SerialNumberInToOutRollBack(string whCode, string loadId, string userName)
        {

            var sql = from a in idal.ISerialNumberOutDAL.SelectAll()
                      where a.WhCode == whCode && a.LoadId == loadId
                      select new { a.ClientCode, a.SoNumber, a.CustomerPoNumber, a.AltItemNumber, a.CartonId };

            //执行调整库存
            foreach (var item in sql.ToList())
            {
                idal.ISerialNumberInDAL.UpdateByExtended(u => u.WhCode == whCode && u.ClientCode == item.ClientCode & u.SoNumber == item.SoNumber && u.CustomerPoNumber == item.CustomerPoNumber && u.AltItemNumber == item.AltItemNumber && u.CartonId == item.CartonId, t => new SerialNumberIn { ToOutStatus = 1, UpdateUser = userName, UpdateDate = DateTime.Now });
            }

            // 
            idal.ISerialNumberOutDAL.DeleteByExtended(u => u.WhCode == whCode && u.LoadId == loadId);

            return "Y";
        }


        public void UrlEdiTaskInsert(string LoadId, string WhCode, string CreateUser)
        {
            UrlEdi url = idal.IUrlEdiDAL.SelectBy(u => u.Id == 17).First();
            UrlEdiTask uet = new UrlEdiTask();
            uet.WhCode = WhCode;
            uet.Type = "OMS";
            uet.Url = url.Url + "&WhCode=" + WhCode;
            uet.Field = url.Field;
            uet.Mark = LoadId;
            uet.HttpType = url.HttpType;
            uet.Status = 1;
            uet.CreateDate = DateTime.Now;
            idal.IUrlEdiTaskDAL.Add(uet);
        }


        //按订单SOPO 验证库存是否满足释放条件
        public CheckReleaseLoadResult CheckInventory1(string mark, string loadId, string whCode, string userName)
        {
            CheckReleaseLoadResult checkReleaseLoadResult = new CheckReleaseLoadResult();
            string result = "";
            List<ReleaseLoad> entity = new List<ReleaseLoad>();         //后面需使用的实体对象
            List<ReleaseLoad> entity1 = new List<ReleaseLoad>();         //后面需使用的实体对象

            List<HuDetailResult> List = new List<HuDetailResult>();     //库存需要比较的实体对象

            List<ReleaseLoad> entity2 = new List<ReleaseLoad>();         //后面需使用的实体对象
            List<ReleaseLoad> entity22 = new List<ReleaseLoad>();         //后面需使用的实体对象
            List<HuDetailResult> List2 = new List<HuDetailResult>();     //库存需要比较的实体对象

            List<ReleaseLoad> entity3 = new List<ReleaseLoad>();         //后面需使用的实体对象
            List<ReleaseLoad> entity33 = new List<ReleaseLoad>();         //后面需使用的实体对象
            List<HuDetailResult> List3 = new List<HuDetailResult>();     //库存需要比较的实体对象 

            List<ReleaseLoad> entity4 = new List<ReleaseLoad>();         //后面需使用的实体对象
            List<ReleaseLoad> entity44 = new List<ReleaseLoad>();         //后面需使用的实体对象
            List<HuDetailResult> List4 = new List<HuDetailResult>();     //库存需要比较的实体对象  

            List<HuDetailResult> checkHuDetailList = new List<HuDetailResult>();    //记录库存是否已被验证过

            #region 5. 释放流程为8 根据出库订单验证SO PO 条件

            if (mark == "8")
            {
                //1.通过Load号   拉取 出库订单信息 聚合同款数量

                #region 1.验证  SO、PO 不为空的订单

                //1.通过Load号   拉取 出库订单信息 聚合同款数量
                var sql_get3 = from a in idal.IOutBoundOrderDetailDAL.SelectAll()
                               join b in idal.IOutBoundOrderDAL.SelectAll() on new { OutBoundOrderId = a.OutBoundOrderId } equals new { OutBoundOrderId = b.Id }
                               join c in idal.ILoadDetailDAL.SelectAll() on new { OutBoundOrderId = a.OutBoundOrderId } equals new { OutBoundOrderId = (Int32)c.OutBoundOrderId }
                               join d in idal.ILoadMasterDAL.SelectAll() on new { LoadMasterId = (Int32)c.LoadMasterId } equals new { LoadMasterId = d.Id }
                               where d.LoadId == loadId && d.WhCode == whCode && a.DSFLag != 1 && (a.SoNumber ?? "") != "" && (a.CustomerPoNumber ?? "") != ""
                               group new { a, d, b } by new
                               {
                                   a.WhCode,
                                   d.LoadId,
                                   b.ClientId,
                                   a.SoNumber,
                                   a.CustomerPoNumber,
                                   a.AltItemNumber,
                                   a.ItemId,
                                   a.UnitId,
                                   a.UnitName,
                                   a.LotNumber1,
                                   a.LotNumber2,
                                   a.LotDate,
                                   userName
                               } into g
                               select new ReleaseLoad
                               {
                                   WhCode = g.Key.WhCode,
                                   LoadId = g.Key.LoadId,
                                   ClientId = g.Key.ClientId,
                                   SoNumber = g.Key.SoNumber,
                                   CustomerPoNumber = g.Key.CustomerPoNumber,
                                   AltItemNumber = g.Key.AltItemNumber,
                                   ItemId = g.Key.ItemId,
                                   UnitId = g.Key.UnitId,
                                   UnitName = g.Key.UnitName,
                                   Qty = g.Sum(p => p.a.Qty),
                                   LotNumber1 = g.Key.LotNumber1,
                                   LotNumber2 = g.Key.LotNumber2,
                                   LotDate = (DateTime?)g.Key.LotDate,
                                   UserName = g.Key.userName
                               };

                entity3 = (from a in idal.IOutBoundOrderDetailDAL.SelectAll()
                           join b in idal.IOutBoundOrderDAL.SelectAll() on new { OutBoundOrderId = a.OutBoundOrderId } equals new { OutBoundOrderId = b.Id }
                           join c in idal.ILoadDetailDAL.SelectAll() on new { OutBoundOrderId = a.OutBoundOrderId } equals new { OutBoundOrderId = (Int32)c.OutBoundOrderId }
                           join d in idal.ILoadMasterDAL.SelectAll() on new { LoadMasterId = (Int32)c.LoadMasterId } equals new { LoadMasterId = d.Id }
                           where d.LoadId == loadId && d.WhCode == whCode && a.DSFLag != 1 && (a.SoNumber ?? "") != "" && (a.CustomerPoNumber ?? "") != ""
                           group new { a, d, b } by new
                           {
                               a.WhCode,
                               d.LoadId,
                               b.ClientId,
                               a.SoNumber,
                               a.CustomerPoNumber,
                               a.AltItemNumber,
                               a.ItemId,
                               a.UnitId,
                               a.UnitName,
                               a.LotNumber1,
                               a.LotNumber2,
                               a.LotDate,

                               userName,
                               a.Sequence
                           } into g
                           select new ReleaseLoad
                           {
                               WhCode = g.Key.WhCode,
                               LoadId = g.Key.LoadId,
                               ClientId = g.Key.ClientId,
                               SoNumber = g.Key.SoNumber,
                               CustomerPoNumber = g.Key.CustomerPoNumber,
                               AltItemNumber = g.Key.AltItemNumber,
                               ItemId = g.Key.ItemId,
                               UnitId = g.Key.UnitId,
                               UnitName = g.Key.UnitName,
                               Qty = g.Sum(p => p.a.Qty),
                               Sequence = g.Key.Sequence,
                               LotNumber1 = g.Key.LotNumber1,
                               LotNumber2 = g.Key.LotNumber2,
                               LotDate = (DateTime?)g.Key.LotDate,

                               UserName = g.Key.userName
                           }).ToList();

                entity33 = sql_get3.ToList();      //赋值，后续使用

                //得到Load释放的 指定托盘表
                List<LoadHuIdExtend> loadHuIdList = idal.ILoadHuIdExtendDAL.SelectBy(u => u.WhCode == whCode && u.LoadId == loadId);

                if (entity33.Count > 0)
                {
                    //2. 用出库订单的款号 SO PO lot1 lot2 等 拉取库存对应的数据
                    var sql3 = from a1 in sql_get3
                               join a in idal.IHuDetailDAL.SelectAll()
                               on new { X = a1.ClientId, A = a1.AltItemNumber, B = a1.ItemId, C = a1.UnitName, D = a1.UnitId, E = (a1.LotNumber1 ?? ""), F = (a1.LotNumber2 ?? ""), G = a1.LotDate, H = a1.SoNumber, I = a1.CustomerPoNumber } equals new { X = a.ClientId, A = a.AltItemNumber, B = a.ItemId, C = a.UnitName, D = a.UnitId, E = (a.LotNumber1 ?? ""), F = (a.LotNumber2 ?? ""), G = a.LotDate, H = a.SoNumber, I = a.CustomerPoNumber }
                               join b in idal.IHuMasterDAL.SelectAll()
                                               on new { a.HuId, a.WhCode }
                                           equals new { b.HuId, b.WhCode }
                               join c in idal.IWhLocationDAL.SelectAll()
                                     on new { b.Location, b.WhCode }
                                 equals new { Location = c.LocationId, c.WhCode }
                               where
                                 b.Type == "M" &&
                                 b.Status == "A" &&
                                 c.LocationTypeId == 1 &&
                                 (a.Qty - (a.PlanQty ?? 0)) > 0
                               select new HuDetailResult
                               {
                                   Id = a.Id,
                                   HuId = a.HuId,
                                   WhCode = a.WhCode,
                                   ClientId = a.ClientId,
                                   ClientCode = a.ClientCode,
                                   SoNumber = a.SoNumber,
                                   CustomerPoNumber = a.CustomerPoNumber,
                                   AltItemNumber = a.AltItemNumber,
                                   ReceiptDate = a.ReceiptDate,
                                   PlanQty = a.PlanQty,
                                   Qty = a.Qty,
                                   ItemId = a.ItemId,
                                   UnitId = a.UnitId,
                                   UnitName = a.UnitName,
                                   Height = a.Height,
                                   Length = a.Length,
                                   Weight = a.Weight,
                                   Width = a.Width,
                                   LotNumber1 = a.LotNumber1,
                                   LotNumber2 = a.LotNumber2,
                                   LotDate = a.LotDate,
                                   Location = b.Location,
                                   LocationTypeId = c.LocationTypeId,
                                   LocationTypeDetailId = ((c.LocationTypeDetailId ?? 0) == 0 ? 99 : c.LocationTypeDetailId)
                               };
                    List3 = sql3.Distinct().ToList();

                    if (List3.Count == 0)
                    {
                        result = "释放有误库存不足，请检查托盘状态及库位、款号、属性123及Lot是否一致！";
                    }

                    if (result != "")
                    {
                        checkReleaseLoadResult.Result = result;
                        return checkReleaseLoadResult;
                    }

                    if (loadHuIdList.Count > 0)
                    {
                        string[] huid = (from a in loadHuIdList select a.HuId).ToArray();
                        List3 = List3.Where(u => huid.Contains(u.HuId)).ToList();

                        if (List3.Count == 0)
                        {
                            result = "指定托盘库存不足，请检查托盘状态及库位、款号及Lot是否一致！";
                        }
                    }
                    if (result != "")
                    {
                        checkReleaseLoadResult.Result = result;
                        return checkReleaseLoadResult;
                    }

                    //3. 聚合 通过订单查询出的库存 的数量
                    var sql33 = from hudetail in List3
                                group hudetail by new
                                {
                                    hudetail.WhCode,
                                    hudetail.SoNumber,
                                    hudetail.CustomerPoNumber,
                                    hudetail.AltItemNumber,
                                    hudetail.ItemId,
                                    hudetail.UnitId,
                                    hudetail.UnitName,
                                    hudetail.LotNumber1,
                                    hudetail.LotNumber2,
                                    hudetail.LotDate
                                } into g
                                select new HuDetailResult
                                {
                                    WhCode = g.Key.WhCode,
                                    SoNumber = g.Key.SoNumber,
                                    CustomerPoNumber = g.Key.CustomerPoNumber,
                                    AltItemNumber = g.Key.AltItemNumber,
                                    ItemId = g.Key.ItemId,
                                    UnitId = g.Key.UnitId,
                                    UnitName = g.Key.UnitName,
                                    LotNumber1 = g.Key.LotNumber1,
                                    LotNumber2 = g.Key.LotNumber2,
                                    LotDate = g.Key.LotDate,
                                    Qty = g.Sum(p => p.Qty - (p.PlanQty ?? 0))
                                };

                    List<HuDetailResult> ListWhere3 = sql33.ToList();
                    foreach (var item in entity33)
                    {
                        List<HuDetailResult> huDetailResultList = ListWhere3.Where(u => u.WhCode == item.WhCode && u.SoNumber == item.SoNumber && u.CustomerPoNumber == item.CustomerPoNumber && u.AltItemNumber == item.AltItemNumber && u.ItemId == item.ItemId && u.UnitId == item.UnitId && u.UnitName == item.UnitName &&
                       (u.LotNumber1 == item.LotNumber1 || (u.LotNumber1 == null ? "" : u.LotNumber1) == (item.LotNumber1 == null ? "" : item.LotNumber1))
                        &&
                       (u.LotNumber2 == item.LotNumber2 || (u.LotNumber2 == null ? "" : u.LotNumber2) == (item.LotNumber2 == null ? "" : item.LotNumber2)) &&
                        u.LotDate == item.LotDate).ToList();

                        if (huDetailResultList.Count == 0)
                        {
                            if (loadHuIdList.Count > 0)
                            {
                                result += "指定托盘";
                            }

                            result += "SKU:" + item.AltItemNumber + "库存不足，请检查SO、PO、款号及Lot是否一致！";
                        }
                        else
                        {
                            if (huDetailResultList.First().Qty < item.Qty)
                            {
                                if (loadHuIdList.Count > 0)
                                {
                                    result += "指定托盘";
                                }
                                result += "SKU:" + item.AltItemNumber + "库存小于出货数量！";
                            }
                            else
                            {
                                HuDetailResult hudetail = new HuDetailResult();
                                hudetail.SoNumber = item.SoNumber;
                                hudetail.CustomerPoNumber = item.CustomerPoNumber;
                                hudetail.AltItemNumber = item.AltItemNumber;
                                hudetail.ItemId = item.ItemId;
                                hudetail.UnitName = item.UnitName;
                                hudetail.UnitId = item.UnitId;
                                hudetail.LotNumber1 = item.LotNumber1 ?? "";
                                hudetail.LotNumber2 = item.LotNumber2 ?? "";
                                hudetail.LotDate = item.LotDate;
                                hudetail.Qty = item.Qty;
                                checkHuDetailList.Add(hudetail);
                            }
                        }
                    }
                }
                if (result != "")
                {
                    checkReleaseLoadResult.Result = result;
                    return checkReleaseLoadResult;
                }

                checkReleaseLoadResult.Result = result;
                checkReleaseLoadResult.ReleaseLoadList3 = entity3;
                checkReleaseLoadResult.HuDetailResultList3 = List3;

                #endregion


                #region 2.验证 SO为空 PO不为空的订单
                //1.通过Load号   拉取 出库订单信息 聚合同款数量
                var sql_get2 = from a in idal.IOutBoundOrderDetailDAL.SelectAll()
                               join b in idal.IOutBoundOrderDAL.SelectAll() on new { OutBoundOrderId = a.OutBoundOrderId } equals new { OutBoundOrderId = b.Id }
                               join c in idal.ILoadDetailDAL.SelectAll() on new { OutBoundOrderId = a.OutBoundOrderId } equals new { OutBoundOrderId = (Int32)c.OutBoundOrderId }
                               join d in idal.ILoadMasterDAL.SelectAll() on new { LoadMasterId = (Int32)c.LoadMasterId } equals new { LoadMasterId = d.Id }
                               where d.LoadId == loadId && d.WhCode == whCode && a.DSFLag != 1 && (a.SoNumber ?? "") == "" && (a.CustomerPoNumber ?? "") != ""
                               group new { a, d, b } by new
                               {
                                   a.WhCode,
                                   d.LoadId,
                                   b.ClientId,
                                   a.SoNumber,
                                   a.CustomerPoNumber,
                                   a.AltItemNumber,
                                   a.ItemId,
                                   a.UnitId,
                                   a.UnitName,
                                   a.LotNumber1,
                                   a.LotNumber2,
                                   a.LotDate,

                                   userName
                               } into g
                               select new ReleaseLoad
                               {
                                   WhCode = g.Key.WhCode,
                                   LoadId = g.Key.LoadId,
                                   ClientId = g.Key.ClientId,
                                   SoNumber = g.Key.SoNumber,
                                   CustomerPoNumber = g.Key.CustomerPoNumber,
                                   AltItemNumber = g.Key.AltItemNumber,
                                   ItemId = g.Key.ItemId,
                                   UnitId = g.Key.UnitId,
                                   UnitName = g.Key.UnitName,
                                   Qty = g.Sum(p => p.a.Qty),
                                   LotNumber1 = g.Key.LotNumber1,
                                   LotNumber2 = g.Key.LotNumber2,
                                   LotDate = (DateTime?)g.Key.LotDate,
                                   UserName = g.Key.userName
                               };

                entity2 = (from a in idal.IOutBoundOrderDetailDAL.SelectAll()
                           join b in idal.IOutBoundOrderDAL.SelectAll() on new { OutBoundOrderId = a.OutBoundOrderId } equals new { OutBoundOrderId = b.Id }
                           join c in idal.ILoadDetailDAL.SelectAll() on new { OutBoundOrderId = a.OutBoundOrderId } equals new { OutBoundOrderId = (Int32)c.OutBoundOrderId }
                           join d in idal.ILoadMasterDAL.SelectAll() on new { LoadMasterId = (Int32)c.LoadMasterId } equals new { LoadMasterId = d.Id }
                           where d.LoadId == loadId && d.WhCode == whCode && a.DSFLag != 1 && (a.SoNumber ?? "") == "" && (a.CustomerPoNumber ?? "") != ""
                           group new { a, d, b } by new
                           {
                               a.WhCode,
                               d.LoadId,
                               b.ClientId,
                               a.SoNumber,
                               a.CustomerPoNumber,
                               a.AltItemNumber,
                               a.ItemId,
                               a.UnitId,
                               a.UnitName,
                               a.LotNumber1,
                               a.LotNumber2,
                               a.LotDate,

                               userName,
                               a.Sequence
                           } into g
                           select new ReleaseLoad
                           {
                               WhCode = g.Key.WhCode,
                               LoadId = g.Key.LoadId,
                               ClientId = g.Key.ClientId,
                               SoNumber = g.Key.SoNumber,
                               CustomerPoNumber = g.Key.CustomerPoNumber,
                               AltItemNumber = g.Key.AltItemNumber,
                               ItemId = g.Key.ItemId,
                               UnitId = g.Key.UnitId,
                               UnitName = g.Key.UnitName,
                               Qty = g.Sum(p => p.a.Qty),
                               Sequence = g.Key.Sequence,
                               LotNumber1 = g.Key.LotNumber1,
                               LotNumber2 = g.Key.LotNumber2,
                               LotDate = (DateTime?)g.Key.LotDate,

                               UserName = g.Key.userName
                           }).ToList();

                entity22 = sql_get2.ToList();      //赋值，后续使用

                if (entity22.Count > 0)
                {

                    //2. 用出库订单的款号 SO PO lot1 lot2 等 拉取库存对应的数据
                    var sql2 = from a1 in sql_get2
                               join a in idal.IHuDetailDAL.SelectAll()
                              on new { X = a1.ClientId, A = a1.AltItemNumber, B = a1.ItemId, C = a1.UnitName, D = a1.UnitId, E = (a1.LotNumber1 ?? ""), F = (a1.LotNumber2 ?? ""), G = a1.LotDate, I = a1.CustomerPoNumber } equals new { X = a.ClientId, A = a.AltItemNumber, B = a.ItemId, C = a.UnitName, D = a.UnitId, E = (a.LotNumber1 ?? ""), F = (a.LotNumber2 ?? ""), G = a.LotDate, I = a.CustomerPoNumber }
                               join b in idal.IHuMasterDAL.SelectAll()
                                               on new { a.HuId, a.WhCode }
                                           equals new { b.HuId, b.WhCode }
                               join c in idal.IWhLocationDAL.SelectAll()
                                     on new { b.Location, b.WhCode }
                                 equals new { Location = c.LocationId, c.WhCode }
                               where
                                 b.Type == "M" &&
                                 b.Status == "A" &&
                                 c.LocationTypeId == 1 &&
                                 (a.Qty - (a.PlanQty ?? 0)) > 0
                               select new HuDetailResult
                               {
                                   Id = a.Id,
                                   HuId = a.HuId,
                                   WhCode = a.WhCode,
                                   ClientId = a.ClientId,
                                   ClientCode = a.ClientCode,
                                   SoNumber = a.SoNumber,
                                   CustomerPoNumber = a.CustomerPoNumber,
                                   AltItemNumber = a.AltItemNumber,
                                   ReceiptDate = a.ReceiptDate,
                                   PlanQty = a.PlanQty,
                                   Qty = a.Qty,
                                   ItemId = a.ItemId,
                                   UnitId = a.UnitId,
                                   UnitName = a.UnitName,
                                   Height = a.Height,
                                   Length = a.Length,
                                   Weight = a.Weight,
                                   Width = a.Width,
                                   LotNumber1 = a.LotNumber1,
                                   LotNumber2 = a.LotNumber2,
                                   LotDate = a.LotDate,
                                   Location = b.Location,
                                   LocationTypeId = c.LocationTypeId,
                                   LocationTypeDetailId = ((c.LocationTypeDetailId ?? 0) == 0 ? 99 : c.LocationTypeDetailId)
                               };
                    List2 = sql2.Distinct().ToList();

                    if (List2.Count == 0)
                    {
                        result = "释放有误库存不足，请检查托盘状态及库位、款号、属性123及Lot是否一致！";
                    }

                    if (result != "")
                    {
                        checkReleaseLoadResult.Result = result;
                        return checkReleaseLoadResult;
                    }

                    //得到Load释放的 指定托盘表

                    if (loadHuIdList.Count > 0)
                    {
                        string[] huid = (from a in loadHuIdList select a.HuId).ToArray();
                        List2 = List2.Where(u => huid.Contains(u.HuId)).ToList();

                        if (List2.Count == 0)
                        {
                            result = "指定托盘库存不足，请检查托盘状态及库位、款号及Lot是否一致！";
                        }
                    }
                    if (result != "")
                    {
                        checkReleaseLoadResult.Result = result;
                        return checkReleaseLoadResult;
                    }

                    //3. 聚合 通过订单查询出的库存 的数量
                    var sql22 = from hudetail in List2
                                group hudetail by new
                                {
                                    hudetail.WhCode,
                                    hudetail.CustomerPoNumber,
                                    hudetail.AltItemNumber,
                                    hudetail.ItemId,
                                    hudetail.UnitId,
                                    hudetail.UnitName,
                                    hudetail.LotNumber1,
                                    hudetail.LotNumber2,
                                    hudetail.LotDate
                                } into g
                                select new HuDetailResult
                                {
                                    WhCode = g.Key.WhCode,
                                    CustomerPoNumber = g.Key.CustomerPoNumber,
                                    AltItemNumber = g.Key.AltItemNumber,
                                    ItemId = g.Key.ItemId,
                                    UnitId = g.Key.UnitId,
                                    UnitName = g.Key.UnitName,
                                    LotNumber1 = g.Key.LotNumber1,
                                    LotNumber2 = g.Key.LotNumber2,
                                    LotDate = g.Key.LotDate,
                                    Qty = g.Sum(p => p.Qty - (p.PlanQty ?? 0))
                                };

                    List<HuDetailResult> ListWhere2 = sql22.ToList();
                    foreach (var item in entity22)
                    {
                        List<HuDetailResult> huDetailResultList = ListWhere2.Where(u => u.WhCode == item.WhCode && u.CustomerPoNumber == item.CustomerPoNumber && u.AltItemNumber == item.AltItemNumber && u.ItemId == item.ItemId && u.UnitId == item.UnitId && u.UnitName == item.UnitName &&
                       (u.LotNumber1 == item.LotNumber1 || (u.LotNumber1 == null ? "" : u.LotNumber1) == (item.LotNumber1 == null ? "" : item.LotNumber1))
                        &&
                       (u.LotNumber2 == item.LotNumber2 || (u.LotNumber2 == null ? "" : u.LotNumber2) == (item.LotNumber2 == null ? "" : item.LotNumber2)) &&
                        u.LotDate == item.LotDate).ToList();

                        if (huDetailResultList.Count == 0)
                        {
                            if (loadHuIdList.Count > 0)
                            {
                                result += "指定托盘";
                            }
                            result += "SKU:" + item.AltItemNumber + "库存不足，请检查PO、款号及Lot是否一致！";
                        }
                        else
                        {
                            //如果库存已记录过  比较剩余可用数量与现需出货数量
                            if (checkHuDetailList.Where(u => u.CustomerPoNumber == item.CustomerPoNumber && u.AltItemNumber == item.AltItemNumber && u.ItemId == item.ItemId && u.UnitId == item.UnitId && u.UnitName == item.UnitName &&
                       (u.LotNumber1 == item.LotNumber1 || (u.LotNumber1 == null ? "" : u.LotNumber1) == (item.LotNumber1 == null ? "" : item.LotNumber1))
                        &&
                       (u.LotNumber2 == item.LotNumber2 || (u.LotNumber2 == null ? "" : u.LotNumber2) == (item.LotNumber2 == null ? "" : item.LotNumber2)) &&
                        u.LotDate == item.LotDate).Count() > 0)
                            {
                                HuDetailResult fir = checkHuDetailList.Where(u => u.CustomerPoNumber == item.CustomerPoNumber && u.AltItemNumber == item.AltItemNumber && u.ItemId == item.ItemId && u.UnitId == item.UnitId && u.UnitName == item.UnitName &&
                         (u.LotNumber1 == item.LotNumber1 || (u.LotNumber1 == null ? "" : u.LotNumber1) == (item.LotNumber1 == null ? "" : item.LotNumber1))
                          &&
                         (u.LotNumber2 == item.LotNumber2 || (u.LotNumber2 == null ? "" : u.LotNumber2) == (item.LotNumber2 == null ? "" : item.LotNumber2)) &&
                          u.LotDate == item.LotDate).First();

                                if (huDetailResultList.First().Qty < fir.Qty + item.Qty)
                                {
                                    if (loadHuIdList.Count > 0)
                                    {
                                        result += "指定托盘";
                                    }
                                    result += "SKU:" + item.AltItemNumber + "库存小于出货数量！";
                                }
                                else
                                {
                                    HuDetailResult hudetail1 = new HuDetailResult();
                                    hudetail1.SoNumber = item.SoNumber;
                                    hudetail1.CustomerPoNumber = item.CustomerPoNumber;
                                    hudetail1.AltItemNumber = item.AltItemNumber;
                                    hudetail1.ItemId = item.ItemId;
                                    hudetail1.UnitName = item.UnitName;
                                    hudetail1.UnitId = item.UnitId;
                                    hudetail1.LotNumber1 = item.LotNumber1 ?? "";
                                    hudetail1.LotNumber2 = item.LotNumber2 ?? "";
                                    hudetail1.LotDate = item.LotDate;
                                    hudetail1.Qty = fir.Qty + item.Qty;
                                    checkHuDetailList.Add(hudetail1);
                                    checkHuDetailList.Remove(fir);
                                }
                            }
                            else
                            {
                                if (huDetailResultList.First().Qty < item.Qty)
                                {
                                    if (loadHuIdList.Count > 0)
                                    {
                                        result += "指定托盘";
                                    }
                                    result += "SKU:" + item.AltItemNumber + "库存小于出货数量！";
                                }
                                else
                                {
                                    HuDetailResult hudetail = new HuDetailResult();
                                    hudetail.SoNumber = item.SoNumber;
                                    hudetail.CustomerPoNumber = item.CustomerPoNumber;
                                    hudetail.AltItemNumber = item.AltItemNumber;
                                    hudetail.ItemId = item.ItemId;
                                    hudetail.UnitName = item.UnitName;
                                    hudetail.UnitId = item.UnitId;
                                    hudetail.LotNumber1 = item.LotNumber1 ?? "";
                                    hudetail.LotNumber2 = item.LotNumber2 ?? "";
                                    hudetail.LotDate = item.LotDate;
                                    hudetail.Qty = item.Qty;
                                    checkHuDetailList.Add(hudetail);
                                }
                            }
                        }
                    }
                }
                if (result != "")
                {
                    checkReleaseLoadResult.Result = result;
                    return checkReleaseLoadResult;
                }
                checkReleaseLoadResult.Result = result;
                checkReleaseLoadResult.ReleaseLoadList2 = entity2;
                checkReleaseLoadResult.HuDetailResultList2 = List2;

                #endregion


                #region 3.验证 SO不为空 PO为空的订单
                //1.通过Load号   拉取 出库订单信息 聚合同款数量
                var sql_get4 = from a in idal.IOutBoundOrderDetailDAL.SelectAll()
                               join b in idal.IOutBoundOrderDAL.SelectAll() on new { OutBoundOrderId = a.OutBoundOrderId } equals new { OutBoundOrderId = b.Id }
                               join c in idal.ILoadDetailDAL.SelectAll() on new { OutBoundOrderId = a.OutBoundOrderId } equals new { OutBoundOrderId = (Int32)c.OutBoundOrderId }
                               join d in idal.ILoadMasterDAL.SelectAll() on new { LoadMasterId = (Int32)c.LoadMasterId } equals new { LoadMasterId = d.Id }
                               where d.LoadId == loadId && d.WhCode == whCode && a.DSFLag != 1 && (a.SoNumber ?? "") != "" && (a.CustomerPoNumber ?? "") == ""
                               group new { a, d, b } by new
                               {
                                   a.WhCode,
                                   d.LoadId,
                                   b.ClientId,
                                   a.SoNumber,
                                   a.CustomerPoNumber,
                                   a.AltItemNumber,
                                   a.ItemId,
                                   a.UnitId,
                                   a.UnitName,
                                   a.LotNumber1,
                                   a.LotNumber2,
                                   a.LotDate,

                                   userName
                               } into g
                               select new ReleaseLoad
                               {
                                   WhCode = g.Key.WhCode,
                                   LoadId = g.Key.LoadId,
                                   ClientId = g.Key.ClientId,
                                   SoNumber = g.Key.SoNumber,
                                   CustomerPoNumber = g.Key.CustomerPoNumber,
                                   AltItemNumber = g.Key.AltItemNumber,
                                   ItemId = g.Key.ItemId,
                                   UnitId = g.Key.UnitId,
                                   UnitName = g.Key.UnitName,
                                   Qty = g.Sum(p => p.a.Qty),
                                   LotNumber1 = g.Key.LotNumber1,
                                   LotNumber2 = g.Key.LotNumber2,
                                   LotDate = (DateTime?)g.Key.LotDate,
                                   UserName = g.Key.userName
                               };

                entity4 = (from a in idal.IOutBoundOrderDetailDAL.SelectAll()
                           join b in idal.IOutBoundOrderDAL.SelectAll() on new { OutBoundOrderId = a.OutBoundOrderId } equals new { OutBoundOrderId = b.Id }
                           join c in idal.ILoadDetailDAL.SelectAll() on new { OutBoundOrderId = a.OutBoundOrderId } equals new { OutBoundOrderId = (Int32)c.OutBoundOrderId }
                           join d in idal.ILoadMasterDAL.SelectAll() on new { LoadMasterId = (Int32)c.LoadMasterId } equals new { LoadMasterId = d.Id }
                           where d.LoadId == loadId && d.WhCode == whCode && a.DSFLag != 1 && (a.SoNumber ?? "") != "" && (a.CustomerPoNumber ?? "") == ""
                           group new { a, d, b } by new
                           {
                               a.WhCode,
                               d.LoadId,
                               b.ClientId,
                               a.SoNumber,
                               a.CustomerPoNumber,
                               a.AltItemNumber,
                               a.ItemId,
                               a.UnitId,
                               a.UnitName,
                               a.LotNumber1,
                               a.LotNumber2,
                               a.LotDate,

                               userName,
                               a.Sequence
                           } into g
                           select new ReleaseLoad
                           {
                               WhCode = g.Key.WhCode,
                               LoadId = g.Key.LoadId,
                               ClientId = g.Key.ClientId,
                               SoNumber = g.Key.SoNumber,
                               CustomerPoNumber = g.Key.CustomerPoNumber,
                               AltItemNumber = g.Key.AltItemNumber,
                               ItemId = g.Key.ItemId,
                               UnitId = g.Key.UnitId,
                               UnitName = g.Key.UnitName,
                               Qty = g.Sum(p => p.a.Qty),
                               Sequence = g.Key.Sequence,
                               LotNumber1 = g.Key.LotNumber1,
                               LotNumber2 = g.Key.LotNumber2,
                               LotDate = (DateTime?)g.Key.LotDate,

                               UserName = g.Key.userName
                           }).ToList();

                entity44 = sql_get4.ToList();      //赋值，后续使用

                if (entity44.Count > 0)
                {

                    //2. 用出库订单的款号 SO PO lot1 lot2 等 拉取库存对应的数据
                    var sql4 = from a1 in sql_get4
                               join a in idal.IHuDetailDAL.SelectAll()
                              on new { X = a1.ClientId, A = a1.AltItemNumber, B = a1.ItemId, C = a1.UnitName, D = a1.UnitId, E = (a1.LotNumber1 ?? ""), F = (a1.LotNumber2 ?? ""), G = a1.LotDate, I = a1.SoNumber } equals new { X = a.ClientId, A = a.AltItemNumber, B = a.ItemId, C = a.UnitName, D = a.UnitId, E = (a.LotNumber1 ?? ""), F = (a.LotNumber2 ?? ""), G = a.LotDate, I = a.SoNumber }
                               join b in idal.IHuMasterDAL.SelectAll()
                                               on new { a.HuId, a.WhCode }
                                           equals new { b.HuId, b.WhCode }
                               join c in idal.IWhLocationDAL.SelectAll()
                                     on new { b.Location, b.WhCode }
                                 equals new { Location = c.LocationId, c.WhCode }
                               where
                                 b.Type == "M" &&
                                 b.Status == "A" &&
                                 c.LocationTypeId == 1 &&
                                 (a.Qty - (a.PlanQty ?? 0)) > 0
                               select new HuDetailResult
                               {
                                   Id = a.Id,
                                   HuId = a.HuId,
                                   WhCode = a.WhCode,
                                   ClientId = a.ClientId,
                                   ClientCode = a.ClientCode,
                                   SoNumber = a.SoNumber,
                                   CustomerPoNumber = a.CustomerPoNumber,
                                   AltItemNumber = a.AltItemNumber,
                                   ReceiptDate = a.ReceiptDate,
                                   PlanQty = a.PlanQty,
                                   Qty = a.Qty,
                                   ItemId = a.ItemId,
                                   UnitId = a.UnitId,
                                   UnitName = a.UnitName,
                                   Height = a.Height,
                                   Length = a.Length,
                                   Weight = a.Weight,
                                   Width = a.Width,
                                   LotNumber1 = a.LotNumber1,
                                   LotNumber2 = a.LotNumber2,
                                   LotDate = a.LotDate,
                                   Location = b.Location,
                                   LocationTypeId = c.LocationTypeId,
                                   LocationTypeDetailId = ((c.LocationTypeDetailId ?? 0) == 0 ? 99 : c.LocationTypeDetailId)
                               };
                    List4 = sql4.Distinct().ToList();

                    if (List4.Count == 0)
                    {
                        result = "释放有误库存不足，请检查托盘状态及库位、款号、属性123及Lot是否一致！";
                    }

                    if (result != "")
                    {
                        checkReleaseLoadResult.Result = result;
                        return checkReleaseLoadResult;
                    }

                    //得到Load释放的 指定托盘表

                    if (loadHuIdList.Count > 0)
                    {
                        string[] huid = (from a in loadHuIdList select a.HuId).ToArray();
                        List4 = List4.Where(u => huid.Contains(u.HuId)).ToList();

                        if (List4.Count == 0)
                        {
                            result = "指定托盘库存不足，请检查托盘状态及库位、款号及Lot是否一致！";
                        }
                    }
                    if (result != "")
                    {
                        checkReleaseLoadResult.Result = result;
                        return checkReleaseLoadResult;
                    }

                    //3. 聚合 通过订单查询出的库存 的数量
                    var sql44 = from hudetail in List4
                                group hudetail by new
                                {
                                    hudetail.WhCode,
                                    hudetail.SoNumber,
                                    hudetail.AltItemNumber,
                                    hudetail.ItemId,
                                    hudetail.UnitId,
                                    hudetail.UnitName,
                                    hudetail.LotNumber1,
                                    hudetail.LotNumber2,
                                    hudetail.LotDate
                                } into g
                                select new HuDetailResult
                                {
                                    WhCode = g.Key.WhCode,
                                    SoNumber = g.Key.SoNumber,
                                    AltItemNumber = g.Key.AltItemNumber,
                                    ItemId = g.Key.ItemId,
                                    UnitId = g.Key.UnitId,
                                    UnitName = g.Key.UnitName,
                                    LotNumber1 = g.Key.LotNumber1,
                                    LotNumber2 = g.Key.LotNumber2,
                                    LotDate = g.Key.LotDate,
                                    Qty = g.Sum(p => p.Qty - (p.PlanQty ?? 0))
                                };

                    List<HuDetailResult> ListWhere4 = sql44.ToList();
                    foreach (var item in entity44)
                    {
                        List<HuDetailResult> huDetailResultList = ListWhere4.Where(u => u.WhCode == item.WhCode && u.SoNumber == item.SoNumber && u.AltItemNumber == item.AltItemNumber && u.ItemId == item.ItemId && u.UnitId == item.UnitId && u.UnitName == item.UnitName &&
                       (u.LotNumber1 == item.LotNumber1 || (u.LotNumber1 == null ? "" : u.LotNumber1) == (item.LotNumber1 == null ? "" : item.LotNumber1))
                        &&
                       (u.LotNumber2 == item.LotNumber2 || (u.LotNumber2 == null ? "" : u.LotNumber2) == (item.LotNumber2 == null ? "" : item.LotNumber2)) &&
                        u.LotDate == item.LotDate).ToList();

                        if (huDetailResultList.Count == 0)
                        {
                            if (loadHuIdList.Count > 0)
                            {
                                result += "指定托盘";
                            }
                            result += "SKU:" + item.AltItemNumber + "库存不足，请检查PO、款号及Lot是否一致！";
                        }
                        else
                        {
                            //如果库存已记录过  比较剩余可用数量与现需出货数量
                            if (checkHuDetailList.Where(u => u.SoNumber == item.SoNumber && u.AltItemNumber == item.AltItemNumber && u.ItemId == item.ItemId && u.UnitId == item.UnitId && u.UnitName == item.UnitName &&
                       (u.LotNumber1 == item.LotNumber1 || (u.LotNumber1 == null ? "" : u.LotNumber1) == (item.LotNumber1 == null ? "" : item.LotNumber1))
                        &&
                       (u.LotNumber2 == item.LotNumber2 || (u.LotNumber2 == null ? "" : u.LotNumber2) == (item.LotNumber2 == null ? "" : item.LotNumber2)) &&
                        u.LotDate == item.LotDate).Count() > 0)
                            {
                                HuDetailResult fir = checkHuDetailList.Where(u => u.SoNumber == item.SoNumber && u.AltItemNumber == item.AltItemNumber && u.ItemId == item.ItemId && u.UnitId == item.UnitId && u.UnitName == item.UnitName &&
                         (u.LotNumber1 == item.LotNumber1 || (u.LotNumber1 == null ? "" : u.LotNumber1) == (item.LotNumber1 == null ? "" : item.LotNumber1))
                          &&
                         (u.LotNumber2 == item.LotNumber2 || (u.LotNumber2 == null ? "" : u.LotNumber2) == (item.LotNumber2 == null ? "" : item.LotNumber2)) &&
                          u.LotDate == item.LotDate).First();

                                if (huDetailResultList.First().Qty < fir.Qty + item.Qty)
                                {
                                    if (loadHuIdList.Count > 0)
                                    {
                                        result += "指定托盘";
                                    }
                                    result += "SKU:" + item.AltItemNumber + "库存小于出货数量！";
                                }
                                else
                                {
                                    HuDetailResult hudetail1 = new HuDetailResult();
                                    hudetail1.SoNumber = item.SoNumber;
                                    hudetail1.CustomerPoNumber = item.CustomerPoNumber;
                                    hudetail1.AltItemNumber = item.AltItemNumber;
                                    hudetail1.ItemId = item.ItemId;
                                    hudetail1.UnitName = item.UnitName;
                                    hudetail1.UnitId = item.UnitId;
                                    hudetail1.LotNumber1 = item.LotNumber1 ?? "";
                                    hudetail1.LotNumber2 = item.LotNumber2 ?? "";
                                    hudetail1.LotDate = item.LotDate;
                                    hudetail1.Qty = fir.Qty + item.Qty;
                                    checkHuDetailList.Add(hudetail1);
                                    checkHuDetailList.Remove(fir);
                                }
                            }
                            else
                            {
                                if (huDetailResultList.First().Qty < item.Qty)
                                {
                                    if (loadHuIdList.Count > 0)
                                    {
                                        result += "指定托盘";
                                    }
                                    result += "SKU:" + item.AltItemNumber + "库存小于出货数量！";
                                }
                                else
                                {
                                    HuDetailResult hudetail = new HuDetailResult();
                                    hudetail.SoNumber = item.SoNumber;
                                    hudetail.CustomerPoNumber = item.CustomerPoNumber;
                                    hudetail.AltItemNumber = item.AltItemNumber;
                                    hudetail.ItemId = item.ItemId;
                                    hudetail.UnitName = item.UnitName;
                                    hudetail.UnitId = item.UnitId;
                                    hudetail.LotNumber1 = item.LotNumber1 ?? "";
                                    hudetail.LotNumber2 = item.LotNumber2 ?? "";
                                    hudetail.LotDate = item.LotDate;
                                    hudetail.Qty = item.Qty;
                                    checkHuDetailList.Add(hudetail);
                                }
                            }
                        }
                    }
                }
                if (result != "")
                {
                    checkReleaseLoadResult.Result = result;
                    return checkReleaseLoadResult;
                }
                checkReleaseLoadResult.Result = result;
                checkReleaseLoadResult.ReleaseLoadList4 = entity4;
                checkReleaseLoadResult.HuDetailResultList4 = List4;

                #endregion


                #region 4.验证 SO、PO均为空的情况

                var sql_get1 = from a in idal.IOutBoundOrderDetailDAL.SelectAll()
                               join b in idal.IOutBoundOrderDAL.SelectAll() on new { OutBoundOrderId = a.OutBoundOrderId } equals new { OutBoundOrderId = b.Id }
                               join c in idal.ILoadDetailDAL.SelectAll() on new { OutBoundOrderId = a.OutBoundOrderId } equals new { OutBoundOrderId = (Int32)c.OutBoundOrderId }
                               join d in idal.ILoadMasterDAL.SelectAll() on new { LoadMasterId = (Int32)c.LoadMasterId } equals new { LoadMasterId = d.Id }
                               where d.LoadId == loadId && d.WhCode == whCode && a.DSFLag != 1 && (a.SoNumber ?? "") == "" && (a.CustomerPoNumber ?? "") == ""
                               group new { a, d, b } by new
                               {
                                   a.WhCode,
                                   d.LoadId,
                                   b.ClientId,
                                   a.SoNumber,
                                   a.CustomerPoNumber,
                                   a.AltItemNumber,
                                   a.ItemId,
                                   a.UnitId,
                                   a.UnitName,
                                   a.LotNumber1,
                                   a.LotNumber2,
                                   a.LotDate,
                                   userName
                               } into g
                               select new ReleaseLoad
                               {
                                   WhCode = g.Key.WhCode,
                                   LoadId = g.Key.LoadId,
                                   ClientId = g.Key.ClientId,
                                   SoNumber = g.Key.SoNumber,
                                   CustomerPoNumber = g.Key.CustomerPoNumber,
                                   AltItemNumber = g.Key.AltItemNumber,
                                   ItemId = g.Key.ItemId,
                                   UnitId = g.Key.UnitId,
                                   UnitName = g.Key.UnitName,
                                   Qty = g.Sum(p => p.a.Qty),
                                   LotNumber1 = g.Key.LotNumber1,
                                   LotNumber2 = g.Key.LotNumber2,
                                   LotDate = (DateTime?)g.Key.LotDate,
                                   UserName = g.Key.userName
                               };

                entity1 = (from a in idal.IOutBoundOrderDetailDAL.SelectAll()
                           join b in idal.IOutBoundOrderDAL.SelectAll() on new { OutBoundOrderId = a.OutBoundOrderId } equals new { OutBoundOrderId = b.Id }
                           join c in idal.ILoadDetailDAL.SelectAll() on new { OutBoundOrderId = a.OutBoundOrderId } equals new { OutBoundOrderId = (Int32)c.OutBoundOrderId }
                           join d in idal.ILoadMasterDAL.SelectAll() on new { LoadMasterId = (Int32)c.LoadMasterId } equals new { LoadMasterId = d.Id }
                           where d.LoadId == loadId && d.WhCode == whCode && a.DSFLag != 1 && (a.SoNumber ?? "") == "" && (a.CustomerPoNumber ?? "") == ""
                           group new { a, d, b } by new
                           {
                               a.WhCode,
                               d.LoadId,
                               b.ClientId,
                               a.SoNumber,
                               a.CustomerPoNumber,
                               a.AltItemNumber,
                               a.ItemId,
                               a.UnitId,
                               a.UnitName,
                               a.LotNumber1,
                               a.LotNumber2,
                               a.LotDate,
                               userName,
                               a.Sequence
                           } into g
                           select new ReleaseLoad
                           {
                               WhCode = g.Key.WhCode,
                               LoadId = g.Key.LoadId,
                               ClientId = g.Key.ClientId,
                               SoNumber = g.Key.SoNumber,
                               CustomerPoNumber = g.Key.CustomerPoNumber,
                               AltItemNumber = g.Key.AltItemNumber,
                               ItemId = g.Key.ItemId,
                               UnitId = g.Key.UnitId,
                               UnitName = g.Key.UnitName,
                               Qty = g.Sum(p => p.a.Qty),
                               Sequence = g.Key.Sequence,
                               LotNumber1 = g.Key.LotNumber1,
                               LotNumber2 = g.Key.LotNumber2,
                               LotDate = (DateTime?)g.Key.LotDate,
                               UserName = g.Key.userName
                           }).ToList();

                entity = sql_get1.ToList();     //赋值，后续使用

                if (entity.Count > 0)
                {

                    //2. 用出库订单的款号 lot1 lot2 等 拉取库存对应的数据
                    var sql = from a1 in sql_get1
                              join a in idal.IHuDetailDAL.SelectAll()
                              on new { X = a1.ClientId, A = a1.AltItemNumber, B = a1.ItemId, C = a1.UnitName, D = a1.UnitId, E = (a1.LotNumber1 ?? ""), F = (a1.LotNumber2 ?? ""), G = a1.LotDate } equals new { X = a.ClientId, A = a.AltItemNumber, B = a.ItemId, C = a.UnitName, D = a.UnitId, E = (a.LotNumber1 ?? ""), F = (a.LotNumber2 ?? ""), G = a.LotDate }
                              join b in idal.IHuMasterDAL.SelectAll()
                                              on new { a.HuId, a.WhCode }
                                          equals new { b.HuId, b.WhCode }
                              join c in idal.IWhLocationDAL.SelectAll()
                                    on new { b.Location, b.WhCode }
                             equals new { Location = c.LocationId, c.WhCode }
                              where
                                b.Type == "M" &&
                                b.Status == "A" &&
                                c.LocationTypeId == 1 &&
                                (a.Qty - (a.PlanQty ?? 0)) > 0
                              select new HuDetailResult
                              {
                                  Id = a.Id,
                                  HuId = a.HuId,
                                  WhCode = a.WhCode,
                                  ClientId = a.ClientId,
                                  ClientCode = a.ClientCode,
                                  SoNumber = a.SoNumber,
                                  CustomerPoNumber = a.CustomerPoNumber,
                                  AltItemNumber = a.AltItemNumber,
                                  ReceiptDate = a.ReceiptDate,
                                  PlanQty = a.PlanQty ?? 0,
                                  Qty = a.Qty,
                                  ItemId = a.ItemId,
                                  UnitId = a.UnitId,
                                  UnitName = a.UnitName,
                                  Height = a.Height,
                                  Length = a.Length,
                                  Weight = a.Weight,
                                  Width = a.Width,
                                  LotNumber1 = a.LotNumber1,
                                  LotNumber2 = a.LotNumber2,
                                  LotDate = a.LotDate,
                                  Location = b.Location,
                                  LocationTypeId = c.LocationTypeId,
                                  LocationTypeDetailId = ((c.LocationTypeDetailId ?? 0) == 0 ? 99 : c.LocationTypeDetailId)
                              };
                    List = sql.Distinct().ToList();

                    //2.1 如果没有库存 提示报错
                    if (List.Count == 0)
                    {
                        result = "释放有误库存不足，请检查托盘状态及库位、款号、属性123及Lot是否一致！";
                    }

                    if (result != "")
                    {
                        checkReleaseLoadResult.Result = result;
                        return checkReleaseLoadResult;
                    }

                    //得到Load释放的 指定托盘表
                    if (loadHuIdList.Count > 0)
                    {
                        string[] huid = (from a in loadHuIdList select a.HuId).ToArray();
                        List = List.Where(u => huid.Contains(u.HuId)).ToList();

                        if (List.Count == 0)
                        {
                            result = "指定托盘库存不足，请检查托盘状态及库位、款号及Lot是否一致！";
                        }
                    }
                    if (result != "")
                    {
                        checkReleaseLoadResult.Result = result;
                        return checkReleaseLoadResult;
                    }

                    //3. 聚合 通过订单查询出的库存 的数量
                    var sql1 = from hudetail in List
                               group hudetail by new
                               {
                                   hudetail.WhCode,
                                   hudetail.AltItemNumber,
                                   hudetail.ItemId,
                                   hudetail.UnitId,
                                   hudetail.UnitName,
                                   hudetail.LotNumber1,
                                   hudetail.LotNumber2,
                                   hudetail.LotDate
                               } into g
                               select new HuDetailResult
                               {
                                   WhCode = g.Key.WhCode,
                                   AltItemNumber = g.Key.AltItemNumber,
                                   ItemId = g.Key.ItemId,
                                   UnitId = g.Key.UnitId,
                                   UnitName = g.Key.UnitName,
                                   LotNumber1 = g.Key.LotNumber1,
                                   LotNumber2 = g.Key.LotNumber2,
                                   LotDate = g.Key.LotDate,
                                   Qty = g.Sum(p => p.Qty - (p.PlanQty ?? 0))
                               };

                    List<HuDetailResult> ListWhere = sql1.ToList(); //为什么没有直接在上面toList 是因为 方便调试

                    foreach (var item in entity)    //循环出库订单数据
                    {
                        //同一SKU lot条件下 数量是否满足
                        List<HuDetailResult> huDetailResultList = ListWhere.Where(u => u.WhCode == item.WhCode && u.AltItemNumber == item.AltItemNumber && u.ItemId == item.ItemId && u.UnitId == item.UnitId && u.UnitName == item.UnitName &&
                        (u.LotNumber1 == item.LotNumber1 || (u.LotNumber1 == null ? "" : u.LotNumber1) == (item.LotNumber1 == null ? "" : item.LotNumber1))
                        &&
                       (u.LotNumber2 == item.LotNumber2 || (u.LotNumber2 == null ? "" : u.LotNumber2) == (item.LotNumber2 == null ? "" : item.LotNumber2)) &&
                        u.LotDate == item.LotDate).ToList();

                        if (huDetailResultList.Count == 0)
                        {
                            if (loadHuIdList.Count > 0)
                            {
                                result += "指定托盘";
                            }
                            result += "SKU:" + item.AltItemNumber + "库存不足，请检查款号及Lot是否一致！";
                        }
                        else
                        {
                            //如果库存已记录过  比较剩余可用数量与现需出货数量
                            if (checkHuDetailList.Where(u => u.AltItemNumber == item.AltItemNumber && u.ItemId == item.ItemId && u.UnitId == item.UnitId && u.UnitName == item.UnitName &&
                       (u.LotNumber1 == item.LotNumber1 || (u.LotNumber1 == null ? "" : u.LotNumber1) == (item.LotNumber1 == null ? "" : item.LotNumber1))
                        &&
                       (u.LotNumber2 == item.LotNumber2 || (u.LotNumber2 == null ? "" : u.LotNumber2) == (item.LotNumber2 == null ? "" : item.LotNumber2)) &&
                        u.LotDate == item.LotDate).Count() > 0)
                            {
                                HuDetailResult fir = checkHuDetailList.Where(u => u.AltItemNumber == item.AltItemNumber && u.ItemId == item.ItemId && u.UnitId == item.UnitId && u.UnitName == item.UnitName &&
                         (u.LotNumber1 == item.LotNumber1 || (u.LotNumber1 == null ? "" : u.LotNumber1) == (item.LotNumber1 == null ? "" : item.LotNumber1))
                          &&
                         (u.LotNumber2 == item.LotNumber2 || (u.LotNumber2 == null ? "" : u.LotNumber2) == (item.LotNumber2 == null ? "" : item.LotNumber2)) &&
                          u.LotDate == item.LotDate).First();

                                if (huDetailResultList.First().Qty < fir.Qty + item.Qty)
                                {
                                    if (loadHuIdList.Count > 0)
                                    {
                                        result += "指定托盘";
                                    }
                                    result += "SKU:" + item.AltItemNumber + "库存小于出货数量！";
                                }
                                else
                                {
                                    HuDetailResult hudetail1 = new HuDetailResult();
                                    hudetail1.SoNumber = item.SoNumber;
                                    hudetail1.CustomerPoNumber = item.CustomerPoNumber;
                                    hudetail1.AltItemNumber = item.AltItemNumber;
                                    hudetail1.ItemId = item.ItemId;
                                    hudetail1.UnitName = item.UnitName;
                                    hudetail1.UnitId = item.UnitId;
                                    hudetail1.LotNumber1 = item.LotNumber1 ?? "";
                                    hudetail1.LotNumber2 = item.LotNumber2 ?? "";
                                    hudetail1.LotDate = item.LotDate;
                                    hudetail1.Qty = fir.Qty + item.Qty;
                                    checkHuDetailList.Add(hudetail1);
                                    checkHuDetailList.Remove(fir);
                                }
                            }
                            else
                            {
                                if (huDetailResultList.First().Qty < item.Qty)
                                {
                                    if (loadHuIdList.Count > 0)
                                    {
                                        result += "指定托盘";
                                    }
                                    result += "SKU:" + item.AltItemNumber + "库存小于出货数量！";
                                }
                                else
                                {
                                    HuDetailResult hudetail = new HuDetailResult();
                                    hudetail.SoNumber = item.SoNumber;
                                    hudetail.CustomerPoNumber = item.CustomerPoNumber;
                                    hudetail.AltItemNumber = item.AltItemNumber;
                                    hudetail.ItemId = item.ItemId;
                                    hudetail.UnitName = item.UnitName;
                                    hudetail.UnitId = item.UnitId;
                                    hudetail.LotNumber1 = item.LotNumber1 ?? "";
                                    hudetail.LotNumber2 = item.LotNumber2 ?? "";
                                    hudetail.LotDate = item.LotDate;
                                    hudetail.Qty = item.Qty;
                                    checkHuDetailList.Add(hudetail);
                                }
                            }
                        }
                    }
                }

                if (result != "")
                {
                    checkReleaseLoadResult.Result = result;
                    return checkReleaseLoadResult;
                }

                checkReleaseLoadResult.Result = result;
                checkReleaseLoadResult.ReleaseLoadList = entity1;
                checkReleaseLoadResult.HuDetailResultList = List;

                #endregion

            }


            #endregion

            return checkReleaseLoadResult;
        }


        //释放后填充备货顺序，作为扫描枪引导使用,FieldOrderById默认传值为0
        public string EditPickingSequence(string loadId, string whCode, int FieldOrderById)
        {
            List<LoadMaster> LoadMasterList = idal.ILoadMasterDAL.SelectBy(u => u.WhCode == whCode && u.LoadId == loadId);
            if (LoadMasterList.Count == 0)
            {
                return "未找到Load信息！";
            }

            List<string> columnName = new List<string>();
            List<string> columnNameOrderByDesc = new List<string>();
            if (FieldOrderById == 0)
            {
                //通过Load号取得该流程的排序规则
                columnName = (from a in idal.ILoadMasterDAL.SelectAll()
                              join b in idal.IFlowHeadDAL.SelectAll()
                              on a.ProcessId equals b.Id
                              join c in idal.IFieldOrderByDAL.SelectAll()
                              on b.FieldOrderById equals c.Id
                              where a.WhCode == whCode && a.LoadId == loadId
                              select c.ColumnName).ToList();

                columnNameOrderByDesc = (from a in idal.ILoadMasterDAL.SelectAll()
                                         join b in idal.IFlowHeadDAL.SelectAll()
                                         on a.ProcessId equals b.Id
                                         join c in idal.IFieldOrderByDAL.SelectAll()
                                         on b.FieldOrderById equals c.Id
                                         where a.WhCode == whCode && a.LoadId == loadId && (c.ColumnNameByDesc ?? "") != ""
                                         select c.ColumnNameByDesc).ToList();
            }
            else
            {
                columnName = (from a in idal.IFieldOrderByDAL.SelectAll()
                              where a.Id == FieldOrderById
                              select a.ColumnName).ToList();

                columnNameOrderByDesc = (from a in idal.IFieldOrderByDAL.SelectAll()
                                         where a.Id == FieldOrderById && (a.ColumnNameByDesc ?? "") != ""
                                         select a.ColumnNameByDesc).ToList();
            }

            if (columnName.Count == 0)
            {
                return "未匹配到排序规则！";
            }

            //取得Sequence,SoNumber,CustomerPoNumber,AltItemNumber,Style1,Style2,Style3,Location
            string getColumnName = columnName.First();

            if (getColumnName.IndexOf(",") == -1)
            {
                return "未匹配到排序规则！";
            }

            //分割字段，取得数组
            string[] columnArr = getColumnName.Split(',');

            var getpropertyInfo = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArr[0]);
            if (getpropertyInfo == null)
            {
                return "未匹配到排序规则！";
            }

            List<PickTaskDetailPickingSequenceResult> List = (from a in idal.IPickTaskDetailDAL.SelectAll()
                                                              join b in idal.IItemMasterDAL.SelectAll()
                                                              on a.ItemId equals b.Id
                                                              where a.WhCode == whCode && a.LoadId == loadId
                                                              select new PickTaskDetailPickingSequenceResult
                                                              {
                                                                  Id = a.Id,
                                                                  WhCode = a.WhCode,
                                                                  LoadId = a.LoadId,
                                                                  HuId = a.HuId,
                                                                  Location = a.Location,
                                                                  SoNumber = a.SoNumber,
                                                                  CustomerPoNumber = a.CustomerPoNumber,
                                                                  AltItemNumber = a.AltItemNumber,
                                                                  Style1 = b.Style1,
                                                                  Style2 = b.Style2,
                                                                  Style3 = b.Style3,
                                                                  Length = a.Length,
                                                                  Width = a.Width,
                                                                  Height = a.Height,
                                                                  LotNumber1 = a.LotNumber1,
                                                                  LotNumber2 = a.LotNumber2,
                                                                  LotDate = a.LotDate,
                                                                  Sequence = a.Sequence
                                                              }).ToList();

            List<PickTaskDetailPickingSequenceResult> List1 = new List<PickTaskDetailPickingSequenceResult>();

            if (columnArr.Length == 1)
            {
                var propertyInfo = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArr[0]);
                List1 = List.OrderBy(u => propertyInfo.GetValue(u, null)).ToList();

                //对释放字段进行倒序验证并排序
                List1 = ReleaseColumnNameOrderByDesc(columnNameOrderByDesc, columnArr, List1);

            }
            else if (columnArr.Length == 2)
            {
                var propertyInfo = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArr[0]);
                var propertyInfo1 = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArr[1]);

                List1 = List.OrderBy(u => propertyInfo.GetValue(u, null)).ThenBy(u => propertyInfo1.GetValue(u, null)).ToList();

                //对释放字段进行倒序验证并排序
                List1 = ReleaseColumnNameOrderByDesc(columnNameOrderByDesc, columnArr, List1);
            }
            else if (columnArr.Length == 3)
            {
                var propertyInfo = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArr[0]);
                var propertyInfo1 = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArr[1]);
                var propertyInfo2 = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArr[2]);

                List1 = List.OrderBy(u => propertyInfo.GetValue(u, null)).ThenBy(u => propertyInfo1.GetValue(u, null)).ThenBy(u => propertyInfo2.GetValue(u, null)).ToList();

                //对释放字段进行倒序验证并排序
                List1 = ReleaseColumnNameOrderByDesc(columnNameOrderByDesc, columnArr, List1);
            }
            else if (columnArr.Length == 4)
            {
                var propertyInfo = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArr[0]);
                var propertyInfo1 = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArr[1]);
                var propertyInfo2 = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArr[2]);
                var propertyInfo3 = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArr[3]);

                List1 = List.OrderBy(u => propertyInfo.GetValue(u, null)).ThenBy(u => propertyInfo1.GetValue(u, null)).ThenBy(u => propertyInfo2.GetValue(u, null)).ThenBy(u => propertyInfo3.GetValue(u, null)).ToList();

                //对释放字段进行倒序验证并排序
                List1 = ReleaseColumnNameOrderByDesc(columnNameOrderByDesc, columnArr, List1);
            }
            else if (columnArr.Length == 5)
            {
                var propertyInfo = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArr[0]);
                var propertyInfo1 = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArr[1]);
                var propertyInfo2 = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArr[2]);
                var propertyInfo3 = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArr[3]);
                var propertyInfo4 = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArr[4]);

                List1 = List.OrderBy(u => propertyInfo.GetValue(u, null)).ThenBy(u => propertyInfo1.GetValue(u, null)).ThenBy(u => propertyInfo2.GetValue(u, null)).ThenBy(u => propertyInfo3.GetValue(u, null)).ThenBy(u => propertyInfo4.GetValue(u, null)).ToList();

                //对释放字段进行倒序验证并排序
                List1 = ReleaseColumnNameOrderByDesc(columnNameOrderByDesc, columnArr, List1);
            }
            else if (columnArr.Length == 6)
            {
                var propertyInfo = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArr[0]);
                var propertyInfo1 = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArr[1]);
                var propertyInfo2 = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArr[2]);
                var propertyInfo3 = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArr[3]);
                var propertyInfo4 = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArr[4]);
                var propertyInfo5 = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArr[5]);

                List1 = List.OrderBy(u => propertyInfo.GetValue(u, null)).ThenBy(u => propertyInfo1.GetValue(u, null)).ThenBy(u => propertyInfo2.GetValue(u, null)).ThenBy(u => propertyInfo3.GetValue(u, null)).ThenBy(u => propertyInfo4.GetValue(u, null)).ThenBy(u => propertyInfo5.GetValue(u, null)).ToList();

                //对释放字段进行倒序验证并排序
                List1 = ReleaseColumnNameOrderByDesc(columnNameOrderByDesc, columnArr, List1);
            }
            else if (columnArr.Length == 7)
            {
                var propertyInfo = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArr[0]);
                var propertyInfo1 = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArr[1]);
                var propertyInfo2 = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArr[2]);
                var propertyInfo3 = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArr[3]);
                var propertyInfo4 = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArr[4]);
                var propertyInfo5 = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArr[5]);
                var propertyInfo6 = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArr[6]);

                List1 = List.OrderBy(u => propertyInfo.GetValue(u, null)).ThenBy(u => propertyInfo1.GetValue(u, null)).ThenBy(u => propertyInfo2.GetValue(u, null)).ThenBy(u => propertyInfo3.GetValue(u, null)).ThenBy(u => propertyInfo4.GetValue(u, null)).ThenBy(u => propertyInfo5.GetValue(u, null)).ThenBy(u => propertyInfo6.GetValue(u, null)).ToList();

                //对释放字段进行倒序验证并排序
                List1 = ReleaseColumnNameOrderByDesc(columnNameOrderByDesc, columnArr, List1);
            }
            else if (columnArr.Length == 8)
            {
                var propertyInfo = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArr[0]);
                var propertyInfo1 = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArr[1]);
                var propertyInfo2 = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArr[2]);
                var propertyInfo3 = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArr[3]);
                var propertyInfo4 = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArr[4]);
                var propertyInfo5 = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArr[5]);
                var propertyInfo6 = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArr[6]);
                var propertyInfo7 = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArr[7]);

                List1 = List.OrderBy(u => propertyInfo.GetValue(u, null)).ThenBy(u => propertyInfo1.GetValue(u, null)).ThenBy(u => propertyInfo2.GetValue(u, null)).ThenBy(u => propertyInfo3.GetValue(u, null)).ThenBy(u => propertyInfo4.GetValue(u, null)).ThenBy(u => propertyInfo5.GetValue(u, null)).ThenBy(u => propertyInfo6.GetValue(u, null)).ThenBy(u => propertyInfo7.GetValue(u, null)).ToList();

                //对释放字段进行倒序验证并排序
                List1 = ReleaseColumnNameOrderByDesc(columnNameOrderByDesc, columnArr, List1);

                //通过反射来实现
                //var param = "Address";
                //var propertyInfo = typeof(Student).GetProperty(param);
                //var orderByAddress = items.OrderBy(x => propertyInfo.GetValue(x, null));
            }
            else if (columnArr.Length == 9)
            {
                var propertyInfo = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArr[0]);
                var propertyInfo1 = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArr[1]);
                var propertyInfo2 = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArr[2]);
                var propertyInfo3 = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArr[3]);
                var propertyInfo4 = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArr[4]);
                var propertyInfo5 = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArr[5]);
                var propertyInfo6 = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArr[6]);
                var propertyInfo7 = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArr[7]);
                var propertyInfo8 = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArr[8]);

                List1 = List.OrderBy(u => propertyInfo.GetValue(u, null)).ThenBy(u => propertyInfo1.GetValue(u, null)).ThenBy(u => propertyInfo2.GetValue(u, null)).ThenBy(u => propertyInfo3.GetValue(u, null)).ThenBy(u => propertyInfo4.GetValue(u, null)).ThenBy(u => propertyInfo5.GetValue(u, null)).ThenBy(u => propertyInfo6.GetValue(u, null)).ThenBy(u => propertyInfo7.GetValue(u, null)).ThenBy(u => propertyInfo8.GetValue(u, null)).ToList();

                //对释放字段进行倒序验证并排序
                List1 = ReleaseColumnNameOrderByDesc(columnNameOrderByDesc, columnArr, List1);
            }
            else if (columnArr.Length == 10)
            {
                var propertyInfo = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArr[0]);
                var propertyInfo1 = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArr[1]);
                var propertyInfo2 = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArr[2]);
                var propertyInfo3 = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArr[3]);
                var propertyInfo4 = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArr[4]);
                var propertyInfo5 = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArr[5]);
                var propertyInfo6 = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArr[6]);
                var propertyInfo7 = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArr[7]);
                var propertyInfo8 = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArr[8]);
                var propertyInfo9 = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArr[9]);

                List1 = List.OrderBy(u => propertyInfo.GetValue(u, null)).ThenBy(u => propertyInfo1.GetValue(u, null)).ThenBy(u => propertyInfo2.GetValue(u, null)).ThenBy(u => propertyInfo3.GetValue(u, null)).ThenBy(u => propertyInfo4.GetValue(u, null)).ThenBy(u => propertyInfo5.GetValue(u, null)).ThenBy(u => propertyInfo6.GetValue(u, null)).ThenBy(u => propertyInfo7.GetValue(u, null)).ThenBy(u => propertyInfo8.GetValue(u, null)).ThenBy(u => propertyInfo9.GetValue(u, null)).ToList();

                //对释放字段进行倒序验证并排序
                List1 = ReleaseColumnNameOrderByDesc(columnNameOrderByDesc, columnArr, List1);
            }
            else if (columnArr.Length == 11)
            {
                var propertyInfo = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArr[0]);
                var propertyInfo1 = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArr[1]);
                var propertyInfo2 = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArr[2]);
                var propertyInfo3 = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArr[3]);
                var propertyInfo4 = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArr[4]);
                var propertyInfo5 = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArr[5]);
                var propertyInfo6 = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArr[6]);
                var propertyInfo7 = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArr[7]);
                var propertyInfo8 = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArr[8]);
                var propertyInfo9 = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArr[9]);
                var propertyInfo10 = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArr[10]);

                List1 = List.OrderBy(u => propertyInfo.GetValue(u, null)).ThenBy(u => propertyInfo1.GetValue(u, null)).ThenBy(u => propertyInfo2.GetValue(u, null)).ThenBy(u => propertyInfo3.GetValue(u, null)).ThenBy(u => propertyInfo4.GetValue(u, null)).ThenBy(u => propertyInfo5.GetValue(u, null)).ThenBy(u => propertyInfo6.GetValue(u, null)).ThenBy(u => propertyInfo7.GetValue(u, null)).ThenBy(u => propertyInfo8.GetValue(u, null)).ThenBy(u => propertyInfo9.GetValue(u, null)).ThenBy(u => propertyInfo10.GetValue(u, null)).ToList();

                //对释放字段进行倒序验证并排序
                List1 = ReleaseColumnNameOrderByDesc(columnNameOrderByDesc, columnArr, List1);
            }

            int count = 0;
            foreach (var item in List1)
            {
                count++;
                idal.IPickTaskDetailDAL.UpdateByExtended(u => u.Id == item.Id, t => new PickTaskDetail { PickingSequence = count });
            }

            return "Y";
        }

        private static List<PickTaskDetailPickingSequenceResult> ReleaseColumnNameOrderByDesc(List<string> columnNameOrderByDesc, string[] columnArr, List<PickTaskDetailPickingSequenceResult> List1)
        {
            //倒序字段列表
            if (columnNameOrderByDesc.Count > 0)
            {
                string[] columnArrOrderByDesc = columnNameOrderByDesc.First().Split(',');

                if (columnArr.Length == 1)
                {
                    #region 1个字段排序
                    var propertyInfo = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArr[0]);

                    if (columnArrOrderByDesc.Length == 1)
                    {
                        var propertyInfoOrderByDesc = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArrOrderByDesc[0]);

                        if (columnArr[0] == columnArrOrderByDesc[0])
                        {
                            List1 = List1.OrderByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ToList();
                        }
                    }

                    #endregion
                }

                if (columnArr.Length == 2)
                {
                    #region 2个字段排序
                    var propertyInfo = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArr[0]);
                    var propertyInfo1 = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArr[1]);

                    if (columnArrOrderByDesc.Length == 1)
                    {
                        var propertyInfoOrderByDesc = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArrOrderByDesc[0]);

                        if (columnArr[0] == columnArrOrderByDesc[0])
                        {
                            List1 = List1.OrderByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenBy(u => propertyInfo1.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[1] == columnArrOrderByDesc[0])
                        {
                            List1 = List1.OrderBy(u => propertyInfo.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ToList();
                        }
                    }
                    else if (columnArrOrderByDesc.Length == 2)
                    {
                        var propertyInfoOrderByDesc = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArrOrderByDesc[0]);
                        var propertyInfoOrderByDesc1 = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArrOrderByDesc[1]);

                        //第一个和第二个是需要倒序的字段
                        if (columnArr[0] == columnArrOrderByDesc[0] && columnArr[1] == columnArrOrderByDesc[1])
                        {
                            List1 = List1.OrderByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc1.GetValue(u, null)).ToList();
                        }
                    }
                    #endregion
                }

                if (columnArr.Length == 3)
                {
                    #region 3个字段排序
                    var propertyInfo = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArr[0]);
                    var propertyInfo1 = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArr[1]);
                    var propertyInfo2 = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArr[2]);

                    if (columnArrOrderByDesc.Length == 1)
                    {
                        var propertyInfoOrderByDesc = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArrOrderByDesc[0]);

                        if (columnArr[0] == columnArrOrderByDesc[0])
                        {
                            List1 = List1.OrderByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenBy(u => propertyInfo1.GetValue(u, null)).ThenBy(u => propertyInfo2.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[1] == columnArrOrderByDesc[0])
                        {
                            List1 = List1.OrderBy(u => propertyInfo.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenBy(u => propertyInfo2.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[2] == columnArrOrderByDesc[0])
                        {
                            List1 = List1.OrderBy(u => propertyInfo.GetValue(u, null)).ThenBy(u => propertyInfo1.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ToList();
                        }
                    }
                    else if (columnArrOrderByDesc.Length == 2)
                    {
                        var propertyInfoOrderByDesc = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArrOrderByDesc[0]);
                        var propertyInfoOrderByDesc1 = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArrOrderByDesc[1]);

                        //第一个和第二个是需要倒序的字段
                        if (columnArr[0] == columnArrOrderByDesc[0] && columnArr[1] == columnArrOrderByDesc[1])
                        {
                            List1 = List1.OrderByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc1.GetValue(u, null)).ThenBy(u => propertyInfo2.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[0] == columnArrOrderByDesc[0] && columnArr[2] == columnArrOrderByDesc[1])
                        {
                            List1 = List1.OrderByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenBy(u => propertyInfo1.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc1.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[1] == columnArrOrderByDesc[0] && columnArr[2] == columnArrOrderByDesc[1])
                        {
                            //第二个和第三个是需要倒序的字段
                            List1 = List1.OrderBy(u => propertyInfo.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc1.GetValue(u, null)).ToList();
                        }
                    }
                    #endregion
                }

                if (columnArr.Length == 4)
                {
                    #region 4个字段排序
                    var propertyInfo = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArr[0]);
                    var propertyInfo1 = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArr[1]);
                    var propertyInfo2 = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArr[2]);
                    var propertyInfo3 = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArr[3]);

                    if (columnArrOrderByDesc.Length == 1)
                    {
                        var propertyInfoOrderByDesc = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArrOrderByDesc[0]);

                        if (columnArr[0] == columnArrOrderByDesc[0])
                        {
                            List1 = List1.OrderByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenBy(u => propertyInfo1.GetValue(u, null)).ThenBy(u => propertyInfo2.GetValue(u, null)).ThenBy(u => propertyInfo3.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[1] == columnArrOrderByDesc[0])
                        {
                            List1 = List1.OrderBy(u => propertyInfo.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenBy(u => propertyInfo2.GetValue(u, null)).ThenBy(u => propertyInfo3.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[2] == columnArrOrderByDesc[0])
                        {
                            List1 = List1.OrderBy(u => propertyInfo.GetValue(u, null)).ThenBy(u => propertyInfo1.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenBy(u => propertyInfo3.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[3] == columnArrOrderByDesc[0])
                        {
                            List1 = List1.OrderBy(u => propertyInfo.GetValue(u, null)).ThenBy(u => propertyInfo1.GetValue(u, null)).ThenBy(u => propertyInfo2.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ToList();
                        }
                    }
                    else if (columnArrOrderByDesc.Length == 2)
                    {
                        var propertyInfoOrderByDesc = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArrOrderByDesc[0]);
                        var propertyInfoOrderByDesc1 = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArrOrderByDesc[1]);

                        //第一个和第二个是需要倒序的字段
                        if (columnArr[0] == columnArrOrderByDesc[0] && columnArr[1] == columnArrOrderByDesc[1])
                        {
                            List1 = List1.OrderByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc1.GetValue(u, null)).ThenBy(u => propertyInfo2.GetValue(u, null)).ThenBy(u => propertyInfo3.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[0] == columnArrOrderByDesc[0] && columnArr[2] == columnArrOrderByDesc[1])
                        {
                            List1 = List1.OrderByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenBy(u => propertyInfo1.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc1.GetValue(u, null)).ThenBy(u => propertyInfo3.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[0] == columnArrOrderByDesc[0] && columnArr[3] == columnArrOrderByDesc[1])
                        {
                            List1 = List1.OrderByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenBy(u => propertyInfo1.GetValue(u, null)).ThenBy(u => propertyInfo2.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc1.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[1] == columnArrOrderByDesc[0] && columnArr[2] == columnArrOrderByDesc[1])
                        {
                            //第二个和第三个是需要倒序的字段
                            List1 = List1.OrderBy(u => propertyInfo.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc1.GetValue(u, null)).ThenBy(u => propertyInfo3.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[1] == columnArrOrderByDesc[0] && columnArr[3] == columnArrOrderByDesc[1])
                        {
                            List1 = List1.OrderBy(u => propertyInfo.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenBy(u => propertyInfo2.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc1.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[2] == columnArrOrderByDesc[0] && columnArr[3] == columnArrOrderByDesc[1])
                        {
                            //第三个和第四个是需要倒序的字段
                            List1 = List1.OrderBy(u => propertyInfo.GetValue(u, null)).ThenBy(u => propertyInfo1.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc1.GetValue(u, null)).ToList();
                        }

                    }
                    #endregion
                }

                if (columnArr.Length == 5)
                {
                    #region 5个字段排序
                    var propertyInfo = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArr[0]);
                    var propertyInfo1 = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArr[1]);
                    var propertyInfo2 = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArr[2]);
                    var propertyInfo3 = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArr[3]);
                    var propertyInfo4 = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArr[4]);

                    if (columnArrOrderByDesc.Length == 1)
                    {
                        var propertyInfoOrderByDesc = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArrOrderByDesc[0]);

                        if (columnArr[0] == columnArrOrderByDesc[0])
                        {
                            List1 = List1.OrderByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenBy(u => propertyInfo1.GetValue(u, null)).ThenBy(u => propertyInfo2.GetValue(u, null)).ThenBy(u => propertyInfo3.GetValue(u, null)).ThenBy(u => propertyInfo4.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[1] == columnArrOrderByDesc[0])
                        {
                            List1 = List1.OrderBy(u => propertyInfo.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenBy(u => propertyInfo2.GetValue(u, null)).ThenBy(u => propertyInfo3.GetValue(u, null)).ThenBy(u => propertyInfo4.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[2] == columnArrOrderByDesc[0])
                        {
                            List1 = List1.OrderBy(u => propertyInfo.GetValue(u, null)).ThenBy(u => propertyInfo1.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenBy(u => propertyInfo3.GetValue(u, null)).ThenBy(u => propertyInfo4.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[3] == columnArrOrderByDesc[0])
                        {
                            List1 = List1.OrderBy(u => propertyInfo.GetValue(u, null)).ThenBy(u => propertyInfo1.GetValue(u, null)).ThenBy(u => propertyInfo2.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenBy(u => propertyInfo4.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[4] == columnArrOrderByDesc[0])
                        {
                            List1 = List1.OrderBy(u => propertyInfo.GetValue(u, null)).ThenBy(u => propertyInfo1.GetValue(u, null)).ThenBy(u => propertyInfo2.GetValue(u, null)).ThenBy(u => propertyInfo3.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ToList();
                        }
                    }
                    else if (columnArrOrderByDesc.Length == 2)
                    {
                        var propertyInfoOrderByDesc = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArrOrderByDesc[0]);
                        var propertyInfoOrderByDesc1 = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArrOrderByDesc[1]);

                        //第一个和第二个是需要倒序的字段
                        if (columnArr[0] == columnArrOrderByDesc[0] && columnArr[1] == columnArrOrderByDesc[1])
                        {
                            List1 = List1.OrderByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc1.GetValue(u, null)).ThenBy(u => propertyInfo2.GetValue(u, null)).ThenBy(u => propertyInfo3.GetValue(u, null)).ThenBy(u => propertyInfo4.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[0] == columnArrOrderByDesc[0] && columnArr[2] == columnArrOrderByDesc[1])
                        {
                            List1 = List1.OrderByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenBy(u => propertyInfo1.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc1.GetValue(u, null)).ThenBy(u => propertyInfo3.GetValue(u, null)).ThenBy(u => propertyInfo4.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[0] == columnArrOrderByDesc[0] && columnArr[3] == columnArrOrderByDesc[1])
                        {
                            List1 = List1.OrderByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenBy(u => propertyInfo1.GetValue(u, null)).ThenBy(u => propertyInfo2.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc1.GetValue(u, null)).ThenBy(u => propertyInfo4.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[0] == columnArrOrderByDesc[0] && columnArr[4] == columnArrOrderByDesc[1])
                        {
                            List1 = List1.OrderByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenBy(u => propertyInfo1.GetValue(u, null)).ThenBy(u => propertyInfo2.GetValue(u, null)).ThenBy(u => propertyInfo3.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc1.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[1] == columnArrOrderByDesc[0] && columnArr[2] == columnArrOrderByDesc[1])
                        {
                            //第二个和第三个是需要倒序的字段
                            List1 = List1.OrderBy(u => propertyInfo.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc1.GetValue(u, null)).ThenBy(u => propertyInfo3.GetValue(u, null)).ThenBy(u => propertyInfo4.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[1] == columnArrOrderByDesc[0] && columnArr[3] == columnArrOrderByDesc[1])
                        {
                            List1 = List1.OrderBy(u => propertyInfo.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenBy(u => propertyInfo2.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc1.GetValue(u, null)).ThenBy(u => propertyInfo4.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[1] == columnArrOrderByDesc[0] && columnArr[4] == columnArrOrderByDesc[1])
                        {
                            List1 = List1.OrderBy(u => propertyInfo.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenBy(u => propertyInfo2.GetValue(u, null)).ThenBy(u => propertyInfo3.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc1.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[2] == columnArrOrderByDesc[0] && columnArr[3] == columnArrOrderByDesc[1])
                        {
                            //第三个和第四个是需要倒序的字段
                            List1 = List1.OrderBy(u => propertyInfo.GetValue(u, null)).ThenBy(u => propertyInfo1.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc1.GetValue(u, null)).ThenBy(u => propertyInfo4.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[2] == columnArrOrderByDesc[0] && columnArr[4] == columnArrOrderByDesc[1])
                        {
                            List1 = List1.OrderBy(u => propertyInfo.GetValue(u, null)).ThenBy(u => propertyInfo1.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenBy(u => propertyInfo3.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc1.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[3] == columnArrOrderByDesc[0] && columnArr[4] == columnArrOrderByDesc[1])
                        {
                            List1 = List1.OrderBy(u => propertyInfo.GetValue(u, null)).ThenBy(u => propertyInfo1.GetValue(u, null)).ThenBy(u => propertyInfo2.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc1.GetValue(u, null)).ToList();
                        }
                    }
                    #endregion
                }

                if (columnArr.Length == 6)
                {
                    #region 6个字段排序
                    var propertyInfo = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArr[0]);
                    var propertyInfo1 = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArr[1]);
                    var propertyInfo2 = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArr[2]);
                    var propertyInfo3 = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArr[3]);
                    var propertyInfo4 = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArr[4]);
                    var propertyInfo5 = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArr[5]);

                    if (columnArrOrderByDesc.Length == 1)
                    {
                        var propertyInfoOrderByDesc = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArrOrderByDesc[0]);

                        if (columnArr[0] == columnArrOrderByDesc[0])
                        {
                            List1 = List1.OrderByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenBy(u => propertyInfo1.GetValue(u, null)).ThenBy(u => propertyInfo2.GetValue(u, null)).ThenBy(u => propertyInfo3.GetValue(u, null)).ThenBy(u => propertyInfo4.GetValue(u, null)).ThenBy(u => propertyInfo5.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[1] == columnArrOrderByDesc[0])
                        {
                            List1 = List1.OrderBy(u => propertyInfo.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenBy(u => propertyInfo2.GetValue(u, null)).ThenBy(u => propertyInfo3.GetValue(u, null)).ThenBy(u => propertyInfo4.GetValue(u, null)).ThenBy(u => propertyInfo5.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[2] == columnArrOrderByDesc[0])
                        {
                            List1 = List1.OrderBy(u => propertyInfo.GetValue(u, null)).ThenBy(u => propertyInfo1.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenBy(u => propertyInfo3.GetValue(u, null)).ThenBy(u => propertyInfo4.GetValue(u, null)).ThenBy(u => propertyInfo5.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[3] == columnArrOrderByDesc[0])
                        {
                            List1 = List1.OrderBy(u => propertyInfo.GetValue(u, null)).ThenBy(u => propertyInfo1.GetValue(u, null)).ThenBy(u => propertyInfo2.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenBy(u => propertyInfo4.GetValue(u, null)).ThenBy(u => propertyInfo5.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[4] == columnArrOrderByDesc[0])
                        {
                            List1 = List1.OrderBy(u => propertyInfo.GetValue(u, null)).ThenBy(u => propertyInfo1.GetValue(u, null)).ThenBy(u => propertyInfo2.GetValue(u, null)).ThenBy(u => propertyInfo3.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenBy(u => propertyInfo5.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[5] == columnArrOrderByDesc[0])
                        {
                            List1 = List1.OrderBy(u => propertyInfo.GetValue(u, null)).ThenBy(u => propertyInfo1.GetValue(u, null)).ThenBy(u => propertyInfo2.GetValue(u, null)).ThenBy(u => propertyInfo3.GetValue(u, null)).ThenBy(u => propertyInfo4.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ToList();
                        }
                    }
                    else if (columnArrOrderByDesc.Length == 2)
                    {
                        var propertyInfoOrderByDesc = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArrOrderByDesc[0]);
                        var propertyInfoOrderByDesc1 = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArrOrderByDesc[1]);

                        //第一个和第二个是需要倒序的字段
                        if (columnArr[0] == columnArrOrderByDesc[0] && columnArr[1] == columnArrOrderByDesc[1])
                        {
                            List1 = List1.OrderByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc1.GetValue(u, null)).ThenBy(u => propertyInfo2.GetValue(u, null)).ThenBy(u => propertyInfo3.GetValue(u, null)).ThenBy(u => propertyInfo4.GetValue(u, null)).ThenBy(u => propertyInfo5.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[0] == columnArrOrderByDesc[0] && columnArr[2] == columnArrOrderByDesc[1])
                        {
                            List1 = List1.OrderByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenBy(u => propertyInfo1.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc1.GetValue(u, null)).ThenBy(u => propertyInfo3.GetValue(u, null)).ThenBy(u => propertyInfo4.GetValue(u, null)).ThenBy(u => propertyInfo5.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[0] == columnArrOrderByDesc[0] && columnArr[3] == columnArrOrderByDesc[1])
                        {
                            List1 = List1.OrderByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenBy(u => propertyInfo1.GetValue(u, null)).ThenBy(u => propertyInfo2.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc1.GetValue(u, null)).ThenBy(u => propertyInfo4.GetValue(u, null)).ThenBy(u => propertyInfo5.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[0] == columnArrOrderByDesc[0] && columnArr[4] == columnArrOrderByDesc[1])
                        {
                            List1 = List1.OrderByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenBy(u => propertyInfo1.GetValue(u, null)).ThenBy(u => propertyInfo2.GetValue(u, null)).ThenBy(u => propertyInfo3.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc1.GetValue(u, null)).ThenBy(u => propertyInfo5.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[0] == columnArrOrderByDesc[0] && columnArr[5] == columnArrOrderByDesc[1])
                        {
                            List1 = List1.OrderByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenBy(u => propertyInfo1.GetValue(u, null)).ThenBy(u => propertyInfo2.GetValue(u, null)).ThenBy(u => propertyInfo3.GetValue(u, null)).ThenBy(u => propertyInfo4.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc1.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[1] == columnArrOrderByDesc[0] && columnArr[2] == columnArrOrderByDesc[1])
                        {
                            //第二个和第三个是需要倒序的字段
                            List1 = List1.OrderBy(u => propertyInfo.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc1.GetValue(u, null)).ThenBy(u => propertyInfo3.GetValue(u, null)).ThenBy(u => propertyInfo4.GetValue(u, null)).ThenBy(u => propertyInfo5.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[1] == columnArrOrderByDesc[0] && columnArr[3] == columnArrOrderByDesc[1])
                        {
                            List1 = List1.OrderBy(u => propertyInfo.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenBy(u => propertyInfo2.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc1.GetValue(u, null)).ThenBy(u => propertyInfo4.GetValue(u, null)).ThenBy(u => propertyInfo5.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[1] == columnArrOrderByDesc[0] && columnArr[4] == columnArrOrderByDesc[1])
                        {
                            List1 = List1.OrderBy(u => propertyInfo.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenBy(u => propertyInfo2.GetValue(u, null)).ThenBy(u => propertyInfo3.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc1.GetValue(u, null)).ThenBy(u => propertyInfo5.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[1] == columnArrOrderByDesc[0] && columnArr[5] == columnArrOrderByDesc[1])
                        {
                            List1 = List1.OrderBy(u => propertyInfo.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenBy(u => propertyInfo2.GetValue(u, null)).ThenBy(u => propertyInfo3.GetValue(u, null)).ThenBy(u => propertyInfo4.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc1.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[2] == columnArrOrderByDesc[0] && columnArr[3] == columnArrOrderByDesc[1])
                        {
                            //第三个和第四个是需要倒序的字段
                            List1 = List1.OrderBy(u => propertyInfo.GetValue(u, null)).ThenBy(u => propertyInfo1.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc1.GetValue(u, null)).ThenBy(u => propertyInfo4.GetValue(u, null)).ThenBy(u => propertyInfo5.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[2] == columnArrOrderByDesc[0] && columnArr[4] == columnArrOrderByDesc[1])
                        {
                            List1 = List1.OrderBy(u => propertyInfo.GetValue(u, null)).ThenBy(u => propertyInfo1.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenBy(u => propertyInfo3.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc1.GetValue(u, null)).ThenBy(u => propertyInfo5.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[2] == columnArrOrderByDesc[0] && columnArr[5] == columnArrOrderByDesc[1])
                        {
                            List1 = List1.OrderBy(u => propertyInfo.GetValue(u, null)).ThenBy(u => propertyInfo1.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenBy(u => propertyInfo3.GetValue(u, null)).ThenBy(u => propertyInfo4.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc1.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[3] == columnArrOrderByDesc[0] && columnArr[4] == columnArrOrderByDesc[1])
                        {
                            List1 = List1.OrderBy(u => propertyInfo.GetValue(u, null)).ThenBy(u => propertyInfo1.GetValue(u, null)).ThenBy(u => propertyInfo2.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc1.GetValue(u, null)).ThenBy(u => propertyInfo5.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[3] == columnArrOrderByDesc[0] && columnArr[5] == columnArrOrderByDesc[1])
                        {
                            List1 = List1.OrderBy(u => propertyInfo.GetValue(u, null)).ThenBy(u => propertyInfo1.GetValue(u, null)).ThenBy(u => propertyInfo2.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenBy(u => propertyInfo4.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc1.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[4] == columnArrOrderByDesc[0] && columnArr[5] == columnArrOrderByDesc[1])
                        {
                            List1 = List1.OrderBy(u => propertyInfo.GetValue(u, null)).ThenBy(u => propertyInfo1.GetValue(u, null)).ThenBy(u => propertyInfo2.GetValue(u, null)).ThenBy(u => propertyInfo3.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc1.GetValue(u, null)).ToList();
                        }
                    }
                    #endregion
                }

                if (columnArr.Length == 7)
                {
                    #region 7个字段排序
                    var propertyInfo = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArr[0]);
                    var propertyInfo1 = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArr[1]);
                    var propertyInfo2 = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArr[2]);
                    var propertyInfo3 = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArr[3]);
                    var propertyInfo4 = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArr[4]);
                    var propertyInfo5 = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArr[5]);
                    var propertyInfo6 = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArr[6]);

                    if (columnArrOrderByDesc.Length == 1)
                    {
                        var propertyInfoOrderByDesc = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArrOrderByDesc[0]);

                        if (columnArr[0] == columnArrOrderByDesc[0])
                        {
                            List1 = List1.OrderByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenBy(u => propertyInfo1.GetValue(u, null)).ThenBy(u => propertyInfo2.GetValue(u, null)).ThenBy(u => propertyInfo3.GetValue(u, null)).ThenBy(u => propertyInfo4.GetValue(u, null)).ThenBy(u => propertyInfo5.GetValue(u, null)).ThenBy(u => propertyInfo6.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[1] == columnArrOrderByDesc[0])
                        {
                            List1 = List1.OrderBy(u => propertyInfo.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenBy(u => propertyInfo2.GetValue(u, null)).ThenBy(u => propertyInfo3.GetValue(u, null)).ThenBy(u => propertyInfo4.GetValue(u, null)).ThenBy(u => propertyInfo5.GetValue(u, null)).ThenBy(u => propertyInfo6.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[2] == columnArrOrderByDesc[0])
                        {
                            List1 = List1.OrderBy(u => propertyInfo.GetValue(u, null)).ThenBy(u => propertyInfo1.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenBy(u => propertyInfo3.GetValue(u, null)).ThenBy(u => propertyInfo4.GetValue(u, null)).ThenBy(u => propertyInfo5.GetValue(u, null)).ThenBy(u => propertyInfo6.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[3] == columnArrOrderByDesc[0])
                        {
                            List1 = List1.OrderBy(u => propertyInfo.GetValue(u, null)).ThenBy(u => propertyInfo1.GetValue(u, null)).ThenBy(u => propertyInfo2.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenBy(u => propertyInfo4.GetValue(u, null)).ThenBy(u => propertyInfo5.GetValue(u, null)).ThenBy(u => propertyInfo6.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[4] == columnArrOrderByDesc[0])
                        {
                            List1 = List1.OrderBy(u => propertyInfo.GetValue(u, null)).ThenBy(u => propertyInfo1.GetValue(u, null)).ThenBy(u => propertyInfo2.GetValue(u, null)).ThenBy(u => propertyInfo3.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenBy(u => propertyInfo5.GetValue(u, null)).ThenBy(u => propertyInfo6.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[5] == columnArrOrderByDesc[0])
                        {
                            List1 = List1.OrderBy(u => propertyInfo.GetValue(u, null)).ThenBy(u => propertyInfo1.GetValue(u, null)).ThenBy(u => propertyInfo2.GetValue(u, null)).ThenBy(u => propertyInfo3.GetValue(u, null)).ThenBy(u => propertyInfo4.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenBy(u => propertyInfo6.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[6] == columnArrOrderByDesc[0])
                        {
                            List1 = List1.OrderBy(u => propertyInfo.GetValue(u, null)).ThenBy(u => propertyInfo1.GetValue(u, null)).ThenBy(u => propertyInfo2.GetValue(u, null)).ThenBy(u => propertyInfo3.GetValue(u, null)).ThenBy(u => propertyInfo4.GetValue(u, null)).ThenBy(u => propertyInfo5.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ToList();
                        }
                    }
                    else if (columnArrOrderByDesc.Length == 2)
                    {
                        var propertyInfoOrderByDesc = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArrOrderByDesc[0]);
                        var propertyInfoOrderByDesc1 = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArrOrderByDesc[1]);

                        //第一个和第二个是需要倒序的字段
                        if (columnArr[0] == columnArrOrderByDesc[0] && columnArr[1] == columnArrOrderByDesc[1])
                        {
                            List1 = List1.OrderByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc1.GetValue(u, null)).ThenBy(u => propertyInfo2.GetValue(u, null)).ThenBy(u => propertyInfo3.GetValue(u, null)).ThenBy(u => propertyInfo4.GetValue(u, null)).ThenBy(u => propertyInfo5.GetValue(u, null)).ThenBy(u => propertyInfo6.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[0] == columnArrOrderByDesc[0] && columnArr[2] == columnArrOrderByDesc[1])
                        {
                            List1 = List1.OrderByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenBy(u => propertyInfo1.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc1.GetValue(u, null)).ThenBy(u => propertyInfo3.GetValue(u, null)).ThenBy(u => propertyInfo4.GetValue(u, null)).ThenBy(u => propertyInfo5.GetValue(u, null)).ThenBy(u => propertyInfo6.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[0] == columnArrOrderByDesc[0] && columnArr[3] == columnArrOrderByDesc[1])
                        {
                            List1 = List1.OrderByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenBy(u => propertyInfo1.GetValue(u, null)).ThenBy(u => propertyInfo2.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc1.GetValue(u, null)).ThenBy(u => propertyInfo4.GetValue(u, null)).ThenBy(u => propertyInfo5.GetValue(u, null)).ThenBy(u => propertyInfo6.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[0] == columnArrOrderByDesc[0] && columnArr[4] == columnArrOrderByDesc[1])
                        {
                            List1 = List1.OrderByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenBy(u => propertyInfo1.GetValue(u, null)).ThenBy(u => propertyInfo2.GetValue(u, null)).ThenBy(u => propertyInfo3.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc1.GetValue(u, null)).ThenBy(u => propertyInfo5.GetValue(u, null)).ThenBy(u => propertyInfo6.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[0] == columnArrOrderByDesc[0] && columnArr[5] == columnArrOrderByDesc[1])
                        {
                            List1 = List1.OrderByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenBy(u => propertyInfo1.GetValue(u, null)).ThenBy(u => propertyInfo2.GetValue(u, null)).ThenBy(u => propertyInfo3.GetValue(u, null)).ThenBy(u => propertyInfo4.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc1.GetValue(u, null)).ThenBy(u => propertyInfo6.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[0] == columnArrOrderByDesc[0] && columnArr[6] == columnArrOrderByDesc[1])
                        {
                            List1 = List1.OrderByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenBy(u => propertyInfo1.GetValue(u, null)).ThenBy(u => propertyInfo2.GetValue(u, null)).ThenBy(u => propertyInfo3.GetValue(u, null)).ThenBy(u => propertyInfo4.GetValue(u, null)).ThenBy(u => propertyInfo5.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc1.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[1] == columnArrOrderByDesc[0] && columnArr[2] == columnArrOrderByDesc[1])
                        {
                            //第二个和第三个是需要倒序的字段
                            List1 = List1.OrderBy(u => propertyInfo.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc1.GetValue(u, null)).ThenBy(u => propertyInfo3.GetValue(u, null)).ThenBy(u => propertyInfo4.GetValue(u, null)).ThenBy(u => propertyInfo5.GetValue(u, null)).ThenBy(u => propertyInfo6.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[1] == columnArrOrderByDesc[0] && columnArr[3] == columnArrOrderByDesc[1])
                        {
                            List1 = List1.OrderBy(u => propertyInfo.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenBy(u => propertyInfo2.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc1.GetValue(u, null)).ThenBy(u => propertyInfo4.GetValue(u, null)).ThenBy(u => propertyInfo5.GetValue(u, null)).ThenBy(u => propertyInfo6.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[1] == columnArrOrderByDesc[0] && columnArr[4] == columnArrOrderByDesc[1])
                        {
                            List1 = List1.OrderBy(u => propertyInfo.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenBy(u => propertyInfo2.GetValue(u, null)).ThenBy(u => propertyInfo3.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc1.GetValue(u, null)).ThenBy(u => propertyInfo5.GetValue(u, null)).ThenBy(u => propertyInfo6.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[1] == columnArrOrderByDesc[0] && columnArr[5] == columnArrOrderByDesc[1])
                        {
                            List1 = List1.OrderBy(u => propertyInfo.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenBy(u => propertyInfo2.GetValue(u, null)).ThenBy(u => propertyInfo3.GetValue(u, null)).ThenBy(u => propertyInfo4.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc1.GetValue(u, null)).ThenBy(u => propertyInfo6.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[1] == columnArrOrderByDesc[0] && columnArr[6] == columnArrOrderByDesc[1])
                        {
                            List1 = List1.OrderBy(u => propertyInfo.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenBy(u => propertyInfo2.GetValue(u, null)).ThenBy(u => propertyInfo3.GetValue(u, null)).ThenBy(u => propertyInfo4.GetValue(u, null)).ThenBy(u => propertyInfo5.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc1.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[2] == columnArrOrderByDesc[0] && columnArr[3] == columnArrOrderByDesc[1])
                        {
                            //第三个和第四个是需要倒序的字段
                            List1 = List1.OrderBy(u => propertyInfo.GetValue(u, null)).ThenBy(u => propertyInfo1.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc1.GetValue(u, null)).ThenBy(u => propertyInfo4.GetValue(u, null)).ThenBy(u => propertyInfo5.GetValue(u, null)).ThenBy(u => propertyInfo6.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[2] == columnArrOrderByDesc[0] && columnArr[4] == columnArrOrderByDesc[1])
                        {
                            List1 = List1.OrderBy(u => propertyInfo.GetValue(u, null)).ThenBy(u => propertyInfo1.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenBy(u => propertyInfo3.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc1.GetValue(u, null)).ThenBy(u => propertyInfo5.GetValue(u, null)).ThenBy(u => propertyInfo6.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[2] == columnArrOrderByDesc[0] && columnArr[5] == columnArrOrderByDesc[1])
                        {
                            List1 = List1.OrderBy(u => propertyInfo.GetValue(u, null)).ThenBy(u => propertyInfo1.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenBy(u => propertyInfo3.GetValue(u, null)).ThenBy(u => propertyInfo4.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc1.GetValue(u, null)).ThenBy(u => propertyInfo6.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[2] == columnArrOrderByDesc[0] && columnArr[6] == columnArrOrderByDesc[1])
                        {
                            List1 = List1.OrderBy(u => propertyInfo.GetValue(u, null)).ThenBy(u => propertyInfo1.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenBy(u => propertyInfo3.GetValue(u, null)).ThenBy(u => propertyInfo4.GetValue(u, null)).ThenBy(u => propertyInfo5.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc1.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[3] == columnArrOrderByDesc[0] && columnArr[4] == columnArrOrderByDesc[1])
                        {
                            List1 = List1.OrderBy(u => propertyInfo.GetValue(u, null)).ThenBy(u => propertyInfo1.GetValue(u, null)).ThenBy(u => propertyInfo2.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc1.GetValue(u, null)).ThenBy(u => propertyInfo5.GetValue(u, null)).ThenBy(u => propertyInfo6.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[3] == columnArrOrderByDesc[0] && columnArr[5] == columnArrOrderByDesc[1])
                        {
                            List1 = List1.OrderBy(u => propertyInfo.GetValue(u, null)).ThenBy(u => propertyInfo1.GetValue(u, null)).ThenBy(u => propertyInfo2.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenBy(u => propertyInfo4.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc1.GetValue(u, null)).ThenBy(u => propertyInfo6.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[3] == columnArrOrderByDesc[0] && columnArr[6] == columnArrOrderByDesc[1])
                        {
                            List1 = List1.OrderBy(u => propertyInfo.GetValue(u, null)).ThenBy(u => propertyInfo1.GetValue(u, null)).ThenBy(u => propertyInfo2.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenBy(u => propertyInfo4.GetValue(u, null)).ThenBy(u => propertyInfo5.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc1.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[4] == columnArrOrderByDesc[0] && columnArr[5] == columnArrOrderByDesc[1])
                        {
                            List1 = List1.OrderBy(u => propertyInfo.GetValue(u, null)).ThenBy(u => propertyInfo1.GetValue(u, null)).ThenBy(u => propertyInfo2.GetValue(u, null)).ThenBy(u => propertyInfo3.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc1.GetValue(u, null)).ThenBy(u => propertyInfo6.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[4] == columnArrOrderByDesc[0] && columnArr[6] == columnArrOrderByDesc[1])
                        {
                            List1 = List1.OrderBy(u => propertyInfo.GetValue(u, null)).ThenBy(u => propertyInfo1.GetValue(u, null)).ThenBy(u => propertyInfo2.GetValue(u, null)).ThenBy(u => propertyInfo3.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenBy(u => propertyInfo5.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc1.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[5] == columnArrOrderByDesc[0] && columnArr[6] == columnArrOrderByDesc[1])
                        {
                            List1 = List1.OrderBy(u => propertyInfo.GetValue(u, null)).ThenBy(u => propertyInfo1.GetValue(u, null)).ThenBy(u => propertyInfo2.GetValue(u, null)).ThenBy(u => propertyInfo3.GetValue(u, null)).ThenBy(u => propertyInfo4.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc1.GetValue(u, null)).ToList();
                        }
                    }
                    #endregion
                }

                if (columnArr.Length == 8)
                {
                    #region 8个字段排序
                    var propertyInfo = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArr[0]);
                    var propertyInfo1 = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArr[1]);
                    var propertyInfo2 = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArr[2]);
                    var propertyInfo3 = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArr[3]);
                    var propertyInfo4 = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArr[4]);
                    var propertyInfo5 = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArr[5]);
                    var propertyInfo6 = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArr[6]);
                    var propertyInfo7 = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArr[7]);

                    if (columnArrOrderByDesc.Length == 1)
                    {
                        var propertyInfoOrderByDesc = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArrOrderByDesc[0]);

                        if (columnArr[0] == columnArrOrderByDesc[0])
                        {
                            List1 = List1.OrderByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenBy(u => propertyInfo1.GetValue(u, null)).ThenBy(u => propertyInfo2.GetValue(u, null)).ThenBy(u => propertyInfo3.GetValue(u, null)).ThenBy(u => propertyInfo4.GetValue(u, null)).ThenBy(u => propertyInfo5.GetValue(u, null)).ThenBy(u => propertyInfo6.GetValue(u, null)).ThenBy(u => propertyInfo7.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[1] == columnArrOrderByDesc[0])
                        {
                            List1 = List1.OrderBy(u => propertyInfo.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenBy(u => propertyInfo2.GetValue(u, null)).ThenBy(u => propertyInfo3.GetValue(u, null)).ThenBy(u => propertyInfo4.GetValue(u, null)).ThenBy(u => propertyInfo5.GetValue(u, null)).ThenBy(u => propertyInfo6.GetValue(u, null)).ThenBy(u => propertyInfo7.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[2] == columnArrOrderByDesc[0])
                        {
                            List1 = List1.OrderBy(u => propertyInfo.GetValue(u, null)).ThenBy(u => propertyInfo1.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenBy(u => propertyInfo3.GetValue(u, null)).ThenBy(u => propertyInfo4.GetValue(u, null)).ThenBy(u => propertyInfo5.GetValue(u, null)).ThenBy(u => propertyInfo6.GetValue(u, null)).ThenBy(u => propertyInfo7.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[3] == columnArrOrderByDesc[0])
                        {
                            List1 = List1.OrderBy(u => propertyInfo.GetValue(u, null)).ThenBy(u => propertyInfo1.GetValue(u, null)).ThenBy(u => propertyInfo2.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenBy(u => propertyInfo4.GetValue(u, null)).ThenBy(u => propertyInfo5.GetValue(u, null)).ThenBy(u => propertyInfo6.GetValue(u, null)).ThenBy(u => propertyInfo7.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[4] == columnArrOrderByDesc[0])
                        {
                            List1 = List1.OrderBy(u => propertyInfo.GetValue(u, null)).ThenBy(u => propertyInfo1.GetValue(u, null)).ThenBy(u => propertyInfo2.GetValue(u, null)).ThenBy(u => propertyInfo3.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenBy(u => propertyInfo5.GetValue(u, null)).ThenBy(u => propertyInfo6.GetValue(u, null)).ThenBy(u => propertyInfo7.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[5] == columnArrOrderByDesc[0])
                        {
                            List1 = List1.OrderBy(u => propertyInfo.GetValue(u, null)).ThenBy(u => propertyInfo1.GetValue(u, null)).ThenBy(u => propertyInfo2.GetValue(u, null)).ThenBy(u => propertyInfo3.GetValue(u, null)).ThenBy(u => propertyInfo4.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenBy(u => propertyInfo6.GetValue(u, null)).ThenBy(u => propertyInfo7.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[6] == columnArrOrderByDesc[0])
                        {
                            List1 = List1.OrderBy(u => propertyInfo.GetValue(u, null)).ThenBy(u => propertyInfo1.GetValue(u, null)).ThenBy(u => propertyInfo2.GetValue(u, null)).ThenBy(u => propertyInfo3.GetValue(u, null)).ThenBy(u => propertyInfo4.GetValue(u, null)).ThenBy(u => propertyInfo5.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenBy(u => propertyInfo7.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[7] == columnArrOrderByDesc[0])
                        {
                            List1 = List1.OrderBy(u => propertyInfo.GetValue(u, null)).ThenBy(u => propertyInfo1.GetValue(u, null)).ThenBy(u => propertyInfo2.GetValue(u, null)).ThenBy(u => propertyInfo3.GetValue(u, null)).ThenBy(u => propertyInfo4.GetValue(u, null)).ThenBy(u => propertyInfo5.GetValue(u, null)).ThenBy(u => propertyInfo6.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ToList();
                        }
                    }
                    else if (columnArrOrderByDesc.Length == 2)
                    {
                        var propertyInfoOrderByDesc = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArrOrderByDesc[0]);
                        var propertyInfoOrderByDesc1 = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArrOrderByDesc[1]);

                        //第一个和第二个是需要倒序的字段
                        if (columnArr[0] == columnArrOrderByDesc[0] && columnArr[1] == columnArrOrderByDesc[1])
                        {
                            List1 = List1.OrderByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc1.GetValue(u, null)).ThenBy(u => propertyInfo2.GetValue(u, null)).ThenBy(u => propertyInfo3.GetValue(u, null)).ThenBy(u => propertyInfo4.GetValue(u, null)).ThenBy(u => propertyInfo5.GetValue(u, null)).ThenBy(u => propertyInfo6.GetValue(u, null)).ThenBy(u => propertyInfo7.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[0] == columnArrOrderByDesc[0] && columnArr[2] == columnArrOrderByDesc[1])
                        {
                            List1 = List1.OrderByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenBy(u => propertyInfo1.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc1.GetValue(u, null)).ThenBy(u => propertyInfo3.GetValue(u, null)).ThenBy(u => propertyInfo4.GetValue(u, null)).ThenBy(u => propertyInfo5.GetValue(u, null)).ThenBy(u => propertyInfo6.GetValue(u, null)).ThenBy(u => propertyInfo7.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[0] == columnArrOrderByDesc[0] && columnArr[3] == columnArrOrderByDesc[1])
                        {
                            List1 = List1.OrderByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenBy(u => propertyInfo1.GetValue(u, null)).ThenBy(u => propertyInfo2.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc1.GetValue(u, null)).ThenBy(u => propertyInfo4.GetValue(u, null)).ThenBy(u => propertyInfo5.GetValue(u, null)).ThenBy(u => propertyInfo6.GetValue(u, null)).ThenBy(u => propertyInfo7.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[0] == columnArrOrderByDesc[0] && columnArr[4] == columnArrOrderByDesc[1])
                        {
                            List1 = List1.OrderByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenBy(u => propertyInfo1.GetValue(u, null)).ThenBy(u => propertyInfo2.GetValue(u, null)).ThenBy(u => propertyInfo3.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc1.GetValue(u, null)).ThenBy(u => propertyInfo5.GetValue(u, null)).ThenBy(u => propertyInfo6.GetValue(u, null)).ThenBy(u => propertyInfo7.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[0] == columnArrOrderByDesc[0] && columnArr[5] == columnArrOrderByDesc[1])
                        {
                            List1 = List1.OrderByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenBy(u => propertyInfo1.GetValue(u, null)).ThenBy(u => propertyInfo2.GetValue(u, null)).ThenBy(u => propertyInfo3.GetValue(u, null)).ThenBy(u => propertyInfo4.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc1.GetValue(u, null)).ThenBy(u => propertyInfo6.GetValue(u, null)).ThenBy(u => propertyInfo7.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[0] == columnArrOrderByDesc[0] && columnArr[6] == columnArrOrderByDesc[1])
                        {
                            List1 = List1.OrderByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenBy(u => propertyInfo1.GetValue(u, null)).ThenBy(u => propertyInfo2.GetValue(u, null)).ThenBy(u => propertyInfo3.GetValue(u, null)).ThenBy(u => propertyInfo4.GetValue(u, null)).ThenBy(u => propertyInfo5.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc1.GetValue(u, null)).ThenBy(u => propertyInfo7.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[0] == columnArrOrderByDesc[0] && columnArr[7] == columnArrOrderByDesc[1])
                        {
                            List1 = List1.OrderByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenBy(u => propertyInfo1.GetValue(u, null)).ThenBy(u => propertyInfo2.GetValue(u, null)).ThenBy(u => propertyInfo3.GetValue(u, null)).ThenBy(u => propertyInfo4.GetValue(u, null)).ThenBy(u => propertyInfo5.GetValue(u, null)).ThenBy(u => propertyInfo6.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc1.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[1] == columnArrOrderByDesc[0] && columnArr[2] == columnArrOrderByDesc[1])
                        {
                            //第二个和第三个是需要倒序的字段
                            List1 = List1.OrderBy(u => propertyInfo.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc1.GetValue(u, null)).ThenBy(u => propertyInfo3.GetValue(u, null)).ThenBy(u => propertyInfo4.GetValue(u, null)).ThenBy(u => propertyInfo5.GetValue(u, null)).ThenBy(u => propertyInfo6.GetValue(u, null)).ThenBy(u => propertyInfo7.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[1] == columnArrOrderByDesc[0] && columnArr[3] == columnArrOrderByDesc[1])
                        {
                            List1 = List1.OrderBy(u => propertyInfo.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenBy(u => propertyInfo2.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc1.GetValue(u, null)).ThenBy(u => propertyInfo4.GetValue(u, null)).ThenBy(u => propertyInfo5.GetValue(u, null)).ThenBy(u => propertyInfo6.GetValue(u, null)).ThenBy(u => propertyInfo7.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[1] == columnArrOrderByDesc[0] && columnArr[4] == columnArrOrderByDesc[1])
                        {
                            List1 = List1.OrderBy(u => propertyInfo.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenBy(u => propertyInfo2.GetValue(u, null)).ThenBy(u => propertyInfo3.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc1.GetValue(u, null)).ThenBy(u => propertyInfo5.GetValue(u, null)).ThenBy(u => propertyInfo6.GetValue(u, null)).ThenBy(u => propertyInfo7.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[1] == columnArrOrderByDesc[0] && columnArr[5] == columnArrOrderByDesc[1])
                        {
                            List1 = List1.OrderBy(u => propertyInfo.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenBy(u => propertyInfo2.GetValue(u, null)).ThenBy(u => propertyInfo3.GetValue(u, null)).ThenBy(u => propertyInfo4.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc1.GetValue(u, null)).ThenBy(u => propertyInfo6.GetValue(u, null)).ThenBy(u => propertyInfo7.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[1] == columnArrOrderByDesc[0] && columnArr[6] == columnArrOrderByDesc[1])
                        {
                            List1 = List1.OrderBy(u => propertyInfo.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenBy(u => propertyInfo2.GetValue(u, null)).ThenBy(u => propertyInfo3.GetValue(u, null)).ThenBy(u => propertyInfo4.GetValue(u, null)).ThenBy(u => propertyInfo5.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc1.GetValue(u, null)).ThenBy(u => propertyInfo7.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[1] == columnArrOrderByDesc[0] && columnArr[7] == columnArrOrderByDesc[1])
                        {
                            List1 = List1.OrderBy(u => propertyInfo.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenBy(u => propertyInfo2.GetValue(u, null)).ThenBy(u => propertyInfo3.GetValue(u, null)).ThenBy(u => propertyInfo4.GetValue(u, null)).ThenBy(u => propertyInfo5.GetValue(u, null)).ThenBy(u => propertyInfo6.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc1.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[2] == columnArrOrderByDesc[0] && columnArr[3] == columnArrOrderByDesc[1])
                        {
                            //第三个和第四个是需要倒序的字段
                            List1 = List1.OrderBy(u => propertyInfo.GetValue(u, null)).ThenBy(u => propertyInfo1.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc1.GetValue(u, null)).ThenBy(u => propertyInfo4.GetValue(u, null)).ThenBy(u => propertyInfo5.GetValue(u, null)).ThenBy(u => propertyInfo6.GetValue(u, null)).ThenBy(u => propertyInfo7.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[2] == columnArrOrderByDesc[0] && columnArr[4] == columnArrOrderByDesc[1])
                        {
                            List1 = List1.OrderBy(u => propertyInfo.GetValue(u, null)).ThenBy(u => propertyInfo1.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenBy(u => propertyInfo3.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc1.GetValue(u, null)).ThenBy(u => propertyInfo5.GetValue(u, null)).ThenBy(u => propertyInfo6.GetValue(u, null)).ThenBy(u => propertyInfo7.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[2] == columnArrOrderByDesc[0] && columnArr[5] == columnArrOrderByDesc[1])
                        {
                            List1 = List1.OrderBy(u => propertyInfo.GetValue(u, null)).ThenBy(u => propertyInfo1.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenBy(u => propertyInfo3.GetValue(u, null)).ThenBy(u => propertyInfo4.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc1.GetValue(u, null)).ThenBy(u => propertyInfo6.GetValue(u, null)).ThenBy(u => propertyInfo7.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[2] == columnArrOrderByDesc[0] && columnArr[6] == columnArrOrderByDesc[1])
                        {
                            List1 = List1.OrderBy(u => propertyInfo.GetValue(u, null)).ThenBy(u => propertyInfo1.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenBy(u => propertyInfo3.GetValue(u, null)).ThenBy(u => propertyInfo4.GetValue(u, null)).ThenBy(u => propertyInfo5.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc1.GetValue(u, null)).ThenBy(u => propertyInfo7.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[2] == columnArrOrderByDesc[0] && columnArr[7] == columnArrOrderByDesc[1])
                        {
                            List1 = List1.OrderBy(u => propertyInfo.GetValue(u, null)).ThenBy(u => propertyInfo1.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenBy(u => propertyInfo3.GetValue(u, null)).ThenBy(u => propertyInfo4.GetValue(u, null)).ThenBy(u => propertyInfo5.GetValue(u, null)).ThenByDescending(u => propertyInfo6.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc1.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[3] == columnArrOrderByDesc[0] && columnArr[4] == columnArrOrderByDesc[1])
                        {
                            List1 = List1.OrderBy(u => propertyInfo.GetValue(u, null)).ThenBy(u => propertyInfo1.GetValue(u, null)).ThenBy(u => propertyInfo2.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc1.GetValue(u, null)).ThenBy(u => propertyInfo5.GetValue(u, null)).ThenBy(u => propertyInfo6.GetValue(u, null)).ThenBy(u => propertyInfo7.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[3] == columnArrOrderByDesc[0] && columnArr[5] == columnArrOrderByDesc[1])
                        {
                            List1 = List1.OrderBy(u => propertyInfo.GetValue(u, null)).ThenBy(u => propertyInfo1.GetValue(u, null)).ThenBy(u => propertyInfo2.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenBy(u => propertyInfo4.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc1.GetValue(u, null)).ThenBy(u => propertyInfo6.GetValue(u, null)).ThenBy(u => propertyInfo7.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[3] == columnArrOrderByDesc[0] && columnArr[6] == columnArrOrderByDesc[1])
                        {
                            List1 = List1.OrderBy(u => propertyInfo.GetValue(u, null)).ThenBy(u => propertyInfo1.GetValue(u, null)).ThenBy(u => propertyInfo2.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenBy(u => propertyInfo4.GetValue(u, null)).ThenBy(u => propertyInfo5.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc1.GetValue(u, null)).ThenBy(u => propertyInfo7.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[3] == columnArrOrderByDesc[0] && columnArr[7] == columnArrOrderByDesc[1])
                        {
                            List1 = List1.OrderBy(u => propertyInfo.GetValue(u, null)).ThenBy(u => propertyInfo1.GetValue(u, null)).ThenBy(u => propertyInfo2.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenBy(u => propertyInfo4.GetValue(u, null)).ThenBy(u => propertyInfo5.GetValue(u, null)).ThenBy(u => propertyInfo6.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc1.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[4] == columnArrOrderByDesc[0] && columnArr[5] == columnArrOrderByDesc[1])
                        {
                            List1 = List1.OrderBy(u => propertyInfo.GetValue(u, null)).ThenBy(u => propertyInfo1.GetValue(u, null)).ThenBy(u => propertyInfo2.GetValue(u, null)).ThenBy(u => propertyInfo3.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc1.GetValue(u, null)).ThenBy(u => propertyInfo6.GetValue(u, null)).ThenBy(u => propertyInfo7.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[4] == columnArrOrderByDesc[0] && columnArr[6] == columnArrOrderByDesc[1])
                        {
                            List1 = List1.OrderBy(u => propertyInfo.GetValue(u, null)).ThenBy(u => propertyInfo1.GetValue(u, null)).ThenBy(u => propertyInfo2.GetValue(u, null)).ThenBy(u => propertyInfo3.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenBy(u => propertyInfo5.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc1.GetValue(u, null)).ThenBy(u => propertyInfo7.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[4] == columnArrOrderByDesc[0] && columnArr[7] == columnArrOrderByDesc[1])
                        {
                            List1 = List1.OrderBy(u => propertyInfo.GetValue(u, null)).ThenBy(u => propertyInfo1.GetValue(u, null)).ThenBy(u => propertyInfo2.GetValue(u, null)).ThenBy(u => propertyInfo3.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenBy(u => propertyInfo5.GetValue(u, null)).ThenBy(u => propertyInfo6.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc1.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[5] == columnArrOrderByDesc[0] && columnArr[6] == columnArrOrderByDesc[1])
                        {
                            List1 = List1.OrderBy(u => propertyInfo.GetValue(u, null)).ThenBy(u => propertyInfo1.GetValue(u, null)).ThenBy(u => propertyInfo2.GetValue(u, null)).ThenBy(u => propertyInfo3.GetValue(u, null)).ThenBy(u => propertyInfo4.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc1.GetValue(u, null)).ThenBy(u => propertyInfo7.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[5] == columnArrOrderByDesc[0] && columnArr[7] == columnArrOrderByDesc[1])
                        {
                            List1 = List1.OrderBy(u => propertyInfo.GetValue(u, null)).ThenBy(u => propertyInfo1.GetValue(u, null)).ThenBy(u => propertyInfo2.GetValue(u, null)).ThenBy(u => propertyInfo3.GetValue(u, null)).ThenBy(u => propertyInfo4.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenBy(u => propertyInfo6.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc1.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[6] == columnArrOrderByDesc[0] && columnArr[7] == columnArrOrderByDesc[1])
                        {
                            List1 = List1.OrderBy(u => propertyInfo.GetValue(u, null)).ThenBy(u => propertyInfo1.GetValue(u, null)).ThenBy(u => propertyInfo2.GetValue(u, null)).ThenBy(u => propertyInfo3.GetValue(u, null)).ThenBy(u => propertyInfo4.GetValue(u, null)).ThenBy(u => propertyInfo5.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc1.GetValue(u, null)).ToList();
                        }
                    }
                    #endregion
                }

                if (columnArr.Length > 8)
                {
                    #region 大于8个字段排序
                    var propertyInfo = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArr[0]);
                    var propertyInfo1 = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArr[1]);
                    var propertyInfo2 = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArr[2]);
                    var propertyInfo3 = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArr[3]);
                    var propertyInfo4 = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArr[4]);
                    var propertyInfo5 = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArr[5]);
                    var propertyInfo6 = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArr[6]);
                    var propertyInfo7 = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArr[7]);

                    if (columnArrOrderByDesc.Length == 1)
                    {
                        var propertyInfoOrderByDesc = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArrOrderByDesc[0]);

                        if (columnArr[0] == columnArrOrderByDesc[0])
                        {
                            List1 = List1.OrderByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenBy(u => propertyInfo1.GetValue(u, null)).ThenBy(u => propertyInfo2.GetValue(u, null)).ThenBy(u => propertyInfo3.GetValue(u, null)).ThenBy(u => propertyInfo4.GetValue(u, null)).ThenBy(u => propertyInfo5.GetValue(u, null)).ThenBy(u => propertyInfo6.GetValue(u, null)).ThenBy(u => propertyInfo7.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[1] == columnArrOrderByDesc[0])
                        {
                            List1 = List1.OrderBy(u => propertyInfo.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenBy(u => propertyInfo2.GetValue(u, null)).ThenBy(u => propertyInfo3.GetValue(u, null)).ThenBy(u => propertyInfo4.GetValue(u, null)).ThenBy(u => propertyInfo5.GetValue(u, null)).ThenBy(u => propertyInfo6.GetValue(u, null)).ThenBy(u => propertyInfo7.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[2] == columnArrOrderByDesc[0])
                        {
                            List1 = List1.OrderBy(u => propertyInfo.GetValue(u, null)).ThenBy(u => propertyInfo1.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenBy(u => propertyInfo3.GetValue(u, null)).ThenBy(u => propertyInfo4.GetValue(u, null)).ThenBy(u => propertyInfo5.GetValue(u, null)).ThenBy(u => propertyInfo6.GetValue(u, null)).ThenBy(u => propertyInfo7.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[3] == columnArrOrderByDesc[0])
                        {
                            List1 = List1.OrderBy(u => propertyInfo.GetValue(u, null)).ThenBy(u => propertyInfo1.GetValue(u, null)).ThenBy(u => propertyInfo2.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenBy(u => propertyInfo4.GetValue(u, null)).ThenBy(u => propertyInfo5.GetValue(u, null)).ThenBy(u => propertyInfo6.GetValue(u, null)).ThenBy(u => propertyInfo7.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[4] == columnArrOrderByDesc[0])
                        {
                            List1 = List1.OrderBy(u => propertyInfo.GetValue(u, null)).ThenBy(u => propertyInfo1.GetValue(u, null)).ThenBy(u => propertyInfo2.GetValue(u, null)).ThenBy(u => propertyInfo3.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenBy(u => propertyInfo5.GetValue(u, null)).ThenBy(u => propertyInfo6.GetValue(u, null)).ThenBy(u => propertyInfo7.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[5] == columnArrOrderByDesc[0])
                        {
                            List1 = List1.OrderBy(u => propertyInfo.GetValue(u, null)).ThenBy(u => propertyInfo1.GetValue(u, null)).ThenBy(u => propertyInfo2.GetValue(u, null)).ThenBy(u => propertyInfo3.GetValue(u, null)).ThenBy(u => propertyInfo4.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenBy(u => propertyInfo6.GetValue(u, null)).ThenBy(u => propertyInfo7.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[6] == columnArrOrderByDesc[0])
                        {
                            List1 = List1.OrderBy(u => propertyInfo.GetValue(u, null)).ThenBy(u => propertyInfo1.GetValue(u, null)).ThenBy(u => propertyInfo2.GetValue(u, null)).ThenBy(u => propertyInfo3.GetValue(u, null)).ThenBy(u => propertyInfo4.GetValue(u, null)).ThenBy(u => propertyInfo5.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenBy(u => propertyInfo7.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[7] == columnArrOrderByDesc[0])
                        {
                            List1 = List1.OrderBy(u => propertyInfo.GetValue(u, null)).ThenBy(u => propertyInfo1.GetValue(u, null)).ThenBy(u => propertyInfo2.GetValue(u, null)).ThenBy(u => propertyInfo3.GetValue(u, null)).ThenBy(u => propertyInfo4.GetValue(u, null)).ThenBy(u => propertyInfo5.GetValue(u, null)).ThenBy(u => propertyInfo6.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ToList();
                        }
                    }
                    else if (columnArrOrderByDesc.Length == 2)
                    {
                        var propertyInfoOrderByDesc = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArrOrderByDesc[0]);
                        var propertyInfoOrderByDesc1 = typeof(PickTaskDetailPickingSequenceResult).GetProperty(columnArrOrderByDesc[1]);

                        //第一个和第二个是需要倒序的字段
                        if (columnArr[0] == columnArrOrderByDesc[0] && columnArr[1] == columnArrOrderByDesc[1])
                        {
                            List1 = List1.OrderByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc1.GetValue(u, null)).ThenBy(u => propertyInfo2.GetValue(u, null)).ThenBy(u => propertyInfo3.GetValue(u, null)).ThenBy(u => propertyInfo4.GetValue(u, null)).ThenBy(u => propertyInfo5.GetValue(u, null)).ThenBy(u => propertyInfo6.GetValue(u, null)).ThenBy(u => propertyInfo7.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[0] == columnArrOrderByDesc[0] && columnArr[2] == columnArrOrderByDesc[1])
                        {
                            List1 = List1.OrderByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenBy(u => propertyInfo1.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc1.GetValue(u, null)).ThenBy(u => propertyInfo3.GetValue(u, null)).ThenBy(u => propertyInfo4.GetValue(u, null)).ThenBy(u => propertyInfo5.GetValue(u, null)).ThenBy(u => propertyInfo6.GetValue(u, null)).ThenBy(u => propertyInfo7.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[0] == columnArrOrderByDesc[0] && columnArr[3] == columnArrOrderByDesc[1])
                        {
                            List1 = List1.OrderByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenBy(u => propertyInfo1.GetValue(u, null)).ThenBy(u => propertyInfo2.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc1.GetValue(u, null)).ThenBy(u => propertyInfo4.GetValue(u, null)).ThenBy(u => propertyInfo5.GetValue(u, null)).ThenBy(u => propertyInfo6.GetValue(u, null)).ThenBy(u => propertyInfo7.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[0] == columnArrOrderByDesc[0] && columnArr[4] == columnArrOrderByDesc[1])
                        {
                            List1 = List1.OrderByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenBy(u => propertyInfo1.GetValue(u, null)).ThenBy(u => propertyInfo2.GetValue(u, null)).ThenBy(u => propertyInfo3.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc1.GetValue(u, null)).ThenBy(u => propertyInfo5.GetValue(u, null)).ThenBy(u => propertyInfo6.GetValue(u, null)).ThenBy(u => propertyInfo7.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[0] == columnArrOrderByDesc[0] && columnArr[5] == columnArrOrderByDesc[1])
                        {
                            List1 = List1.OrderByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenBy(u => propertyInfo1.GetValue(u, null)).ThenBy(u => propertyInfo2.GetValue(u, null)).ThenBy(u => propertyInfo3.GetValue(u, null)).ThenBy(u => propertyInfo4.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc1.GetValue(u, null)).ThenBy(u => propertyInfo6.GetValue(u, null)).ThenBy(u => propertyInfo7.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[0] == columnArrOrderByDesc[0] && columnArr[6] == columnArrOrderByDesc[1])
                        {
                            List1 = List1.OrderByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenBy(u => propertyInfo1.GetValue(u, null)).ThenBy(u => propertyInfo2.GetValue(u, null)).ThenBy(u => propertyInfo3.GetValue(u, null)).ThenBy(u => propertyInfo4.GetValue(u, null)).ThenBy(u => propertyInfo5.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc1.GetValue(u, null)).ThenBy(u => propertyInfo7.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[0] == columnArrOrderByDesc[0] && columnArr[7] == columnArrOrderByDesc[1])
                        {
                            List1 = List1.OrderByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenBy(u => propertyInfo1.GetValue(u, null)).ThenBy(u => propertyInfo2.GetValue(u, null)).ThenBy(u => propertyInfo3.GetValue(u, null)).ThenBy(u => propertyInfo4.GetValue(u, null)).ThenBy(u => propertyInfo5.GetValue(u, null)).ThenBy(u => propertyInfo6.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc1.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[1] == columnArrOrderByDesc[0] && columnArr[2] == columnArrOrderByDesc[1])
                        {
                            //第二个和第三个是需要倒序的字段
                            List1 = List1.OrderBy(u => propertyInfo.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc1.GetValue(u, null)).ThenBy(u => propertyInfo3.GetValue(u, null)).ThenBy(u => propertyInfo4.GetValue(u, null)).ThenBy(u => propertyInfo5.GetValue(u, null)).ThenBy(u => propertyInfo6.GetValue(u, null)).ThenBy(u => propertyInfo7.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[1] == columnArrOrderByDesc[0] && columnArr[3] == columnArrOrderByDesc[1])
                        {
                            List1 = List1.OrderBy(u => propertyInfo.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenBy(u => propertyInfo2.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc1.GetValue(u, null)).ThenBy(u => propertyInfo4.GetValue(u, null)).ThenBy(u => propertyInfo5.GetValue(u, null)).ThenBy(u => propertyInfo6.GetValue(u, null)).ThenBy(u => propertyInfo7.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[1] == columnArrOrderByDesc[0] && columnArr[4] == columnArrOrderByDesc[1])
                        {
                            List1 = List1.OrderBy(u => propertyInfo.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenBy(u => propertyInfo2.GetValue(u, null)).ThenBy(u => propertyInfo3.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc1.GetValue(u, null)).ThenBy(u => propertyInfo5.GetValue(u, null)).ThenBy(u => propertyInfo6.GetValue(u, null)).ThenBy(u => propertyInfo7.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[1] == columnArrOrderByDesc[0] && columnArr[5] == columnArrOrderByDesc[1])
                        {
                            List1 = List1.OrderBy(u => propertyInfo.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenBy(u => propertyInfo2.GetValue(u, null)).ThenBy(u => propertyInfo3.GetValue(u, null)).ThenBy(u => propertyInfo4.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc1.GetValue(u, null)).ThenBy(u => propertyInfo6.GetValue(u, null)).ThenBy(u => propertyInfo7.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[1] == columnArrOrderByDesc[0] && columnArr[6] == columnArrOrderByDesc[1])
                        {
                            List1 = List1.OrderBy(u => propertyInfo.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenBy(u => propertyInfo2.GetValue(u, null)).ThenBy(u => propertyInfo3.GetValue(u, null)).ThenBy(u => propertyInfo4.GetValue(u, null)).ThenBy(u => propertyInfo5.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc1.GetValue(u, null)).ThenBy(u => propertyInfo7.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[1] == columnArrOrderByDesc[0] && columnArr[7] == columnArrOrderByDesc[1])
                        {
                            List1 = List1.OrderBy(u => propertyInfo.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenBy(u => propertyInfo2.GetValue(u, null)).ThenBy(u => propertyInfo3.GetValue(u, null)).ThenBy(u => propertyInfo4.GetValue(u, null)).ThenBy(u => propertyInfo5.GetValue(u, null)).ThenBy(u => propertyInfo6.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc1.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[2] == columnArrOrderByDesc[0] && columnArr[3] == columnArrOrderByDesc[1])
                        {
                            //第三个和第四个是需要倒序的字段
                            List1 = List1.OrderBy(u => propertyInfo.GetValue(u, null)).ThenBy(u => propertyInfo1.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc1.GetValue(u, null)).ThenBy(u => propertyInfo4.GetValue(u, null)).ThenBy(u => propertyInfo5.GetValue(u, null)).ThenBy(u => propertyInfo6.GetValue(u, null)).ThenBy(u => propertyInfo7.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[2] == columnArrOrderByDesc[0] && columnArr[4] == columnArrOrderByDesc[1])
                        {
                            List1 = List1.OrderBy(u => propertyInfo.GetValue(u, null)).ThenBy(u => propertyInfo1.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenBy(u => propertyInfo3.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc1.GetValue(u, null)).ThenBy(u => propertyInfo5.GetValue(u, null)).ThenBy(u => propertyInfo6.GetValue(u, null)).ThenBy(u => propertyInfo7.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[2] == columnArrOrderByDesc[0] && columnArr[5] == columnArrOrderByDesc[1])
                        {
                            List1 = List1.OrderBy(u => propertyInfo.GetValue(u, null)).ThenBy(u => propertyInfo1.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenBy(u => propertyInfo3.GetValue(u, null)).ThenBy(u => propertyInfo4.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc1.GetValue(u, null)).ThenBy(u => propertyInfo6.GetValue(u, null)).ThenBy(u => propertyInfo7.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[2] == columnArrOrderByDesc[0] && columnArr[6] == columnArrOrderByDesc[1])
                        {
                            List1 = List1.OrderBy(u => propertyInfo.GetValue(u, null)).ThenBy(u => propertyInfo1.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenBy(u => propertyInfo3.GetValue(u, null)).ThenBy(u => propertyInfo4.GetValue(u, null)).ThenBy(u => propertyInfo5.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc1.GetValue(u, null)).ThenBy(u => propertyInfo7.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[2] == columnArrOrderByDesc[0] && columnArr[7] == columnArrOrderByDesc[1])
                        {
                            List1 = List1.OrderBy(u => propertyInfo.GetValue(u, null)).ThenBy(u => propertyInfo1.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenBy(u => propertyInfo3.GetValue(u, null)).ThenBy(u => propertyInfo4.GetValue(u, null)).ThenBy(u => propertyInfo5.GetValue(u, null)).ThenByDescending(u => propertyInfo6.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc1.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[3] == columnArrOrderByDesc[0] && columnArr[4] == columnArrOrderByDesc[1])
                        {
                            List1 = List1.OrderBy(u => propertyInfo.GetValue(u, null)).ThenBy(u => propertyInfo1.GetValue(u, null)).ThenBy(u => propertyInfo2.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc1.GetValue(u, null)).ThenBy(u => propertyInfo5.GetValue(u, null)).ThenBy(u => propertyInfo6.GetValue(u, null)).ThenBy(u => propertyInfo7.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[3] == columnArrOrderByDesc[0] && columnArr[5] == columnArrOrderByDesc[1])
                        {
                            List1 = List1.OrderBy(u => propertyInfo.GetValue(u, null)).ThenBy(u => propertyInfo1.GetValue(u, null)).ThenBy(u => propertyInfo2.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenBy(u => propertyInfo4.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc1.GetValue(u, null)).ThenBy(u => propertyInfo6.GetValue(u, null)).ThenBy(u => propertyInfo7.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[3] == columnArrOrderByDesc[0] && columnArr[6] == columnArrOrderByDesc[1])
                        {
                            List1 = List1.OrderBy(u => propertyInfo.GetValue(u, null)).ThenBy(u => propertyInfo1.GetValue(u, null)).ThenBy(u => propertyInfo2.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenBy(u => propertyInfo4.GetValue(u, null)).ThenBy(u => propertyInfo5.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc1.GetValue(u, null)).ThenBy(u => propertyInfo7.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[3] == columnArrOrderByDesc[0] && columnArr[7] == columnArrOrderByDesc[1])
                        {
                            List1 = List1.OrderBy(u => propertyInfo.GetValue(u, null)).ThenBy(u => propertyInfo1.GetValue(u, null)).ThenBy(u => propertyInfo2.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenBy(u => propertyInfo4.GetValue(u, null)).ThenBy(u => propertyInfo5.GetValue(u, null)).ThenBy(u => propertyInfo6.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc1.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[4] == columnArrOrderByDesc[0] && columnArr[5] == columnArrOrderByDesc[1])
                        {
                            List1 = List1.OrderBy(u => propertyInfo.GetValue(u, null)).ThenBy(u => propertyInfo1.GetValue(u, null)).ThenBy(u => propertyInfo2.GetValue(u, null)).ThenBy(u => propertyInfo3.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc1.GetValue(u, null)).ThenBy(u => propertyInfo6.GetValue(u, null)).ThenBy(u => propertyInfo7.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[4] == columnArrOrderByDesc[0] && columnArr[6] == columnArrOrderByDesc[1])
                        {
                            List1 = List1.OrderBy(u => propertyInfo.GetValue(u, null)).ThenBy(u => propertyInfo1.GetValue(u, null)).ThenBy(u => propertyInfo2.GetValue(u, null)).ThenBy(u => propertyInfo3.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenBy(u => propertyInfo5.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc1.GetValue(u, null)).ThenBy(u => propertyInfo7.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[4] == columnArrOrderByDesc[0] && columnArr[7] == columnArrOrderByDesc[1])
                        {
                            List1 = List1.OrderBy(u => propertyInfo.GetValue(u, null)).ThenBy(u => propertyInfo1.GetValue(u, null)).ThenBy(u => propertyInfo2.GetValue(u, null)).ThenBy(u => propertyInfo3.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenBy(u => propertyInfo5.GetValue(u, null)).ThenBy(u => propertyInfo6.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc1.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[5] == columnArrOrderByDesc[0] && columnArr[6] == columnArrOrderByDesc[1])
                        {
                            List1 = List1.OrderBy(u => propertyInfo.GetValue(u, null)).ThenBy(u => propertyInfo1.GetValue(u, null)).ThenBy(u => propertyInfo2.GetValue(u, null)).ThenBy(u => propertyInfo3.GetValue(u, null)).ThenBy(u => propertyInfo4.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc1.GetValue(u, null)).ThenBy(u => propertyInfo7.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[5] == columnArrOrderByDesc[0] && columnArr[7] == columnArrOrderByDesc[1])
                        {
                            List1 = List1.OrderBy(u => propertyInfo.GetValue(u, null)).ThenBy(u => propertyInfo1.GetValue(u, null)).ThenBy(u => propertyInfo2.GetValue(u, null)).ThenBy(u => propertyInfo3.GetValue(u, null)).ThenBy(u => propertyInfo4.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenBy(u => propertyInfo6.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc1.GetValue(u, null)).ToList();
                        }
                        else if (columnArr[6] == columnArrOrderByDesc[0] && columnArr[7] == columnArrOrderByDesc[1])
                        {
                            List1 = List1.OrderBy(u => propertyInfo.GetValue(u, null)).ThenBy(u => propertyInfo1.GetValue(u, null)).ThenBy(u => propertyInfo2.GetValue(u, null)).ThenBy(u => propertyInfo3.GetValue(u, null)).ThenBy(u => propertyInfo4.GetValue(u, null)).ThenBy(u => propertyInfo5.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc.GetValue(u, null)).ThenByDescending(u => propertyInfoOrderByDesc1.GetValue(u, null)).ToList();
                        }
                    }
                    #endregion
                }


            }

            return List1;
        }


        //释放有误时 添加释放异常明细至列表中 前端显示明细列表
        public string ReleaseLoadDetailAdd(string loadId, string whCode, string mark)
        {
            List<ReleaseLoad> entityList = new List<ReleaseLoad>();
            List<HuDetailResult> List = new List<HuDetailResult>();
            List<HuDetailResult> List1 = new List<HuDetailResult>();

            var sql_get1 = from a in idal.IOutBoundOrderDetailDAL.SelectAll()
                           join b in idal.IOutBoundOrderDAL.SelectAll() on new { OutBoundOrderId = a.OutBoundOrderId } equals new { OutBoundOrderId = b.Id }
                           join c in idal.ILoadDetailDAL.SelectAll() on new { OutBoundOrderId = a.OutBoundOrderId } equals new { OutBoundOrderId = (Int32)c.OutBoundOrderId }
                           join d in idal.ILoadMasterDAL.SelectAll() on new { LoadMasterId = (Int32)c.LoadMasterId } equals new { LoadMasterId = d.Id }
                           where d.LoadId == loadId && d.WhCode == whCode && a.DSFLag != 1
                           group new { a, d, b } by new
                           {
                               a.WhCode,
                               d.LoadId,
                               b.ClientId,
                               b.ClientCode,
                               a.SoNumber,
                               a.CustomerPoNumber,
                               a.AltItemNumber,
                               a.ItemId,
                               a.UnitId,
                               a.UnitName,
                               a.LotNumber1,
                               a.LotNumber2,
                               a.LotDate
                           } into g
                           select new ReleaseLoad
                           {
                               WhCode = g.Key.WhCode,
                               LoadId = g.Key.LoadId,
                               ClientId = g.Key.ClientId,
                               ClientCode = g.Key.ClientCode,
                               SoNumber = g.Key.SoNumber ?? "",
                               CustomerPoNumber = g.Key.CustomerPoNumber ?? "",
                               AltItemNumber = g.Key.AltItemNumber,
                               ItemId = g.Key.ItemId,
                               UnitId = g.Key.UnitId,
                               UnitName = g.Key.UnitName ?? "",
                               Qty = g.Sum(p => p.a.Qty),
                               LotNumber1 = g.Key.LotNumber1 ?? "",
                               LotNumber2 = g.Key.LotNumber2 ?? "",
                               LotDate = g.Key.LotDate
                           };

            entityList = sql_get1.ToList();

            string[] skuArr = (from a in entityList select a.AltItemNumber).Distinct().ToArray();
            string[] clientCodeArr = (from a in entityList select a.ClientCode).Distinct().ToArray();

            if (mark == "11")
            {
                #region 流程11 指定库位ROC1 库位匹配

                List<LoadHuIdExtend> loadHuIdList = idal.ILoadHuIdExtendDAL.SelectBy(u => u.WhCode == whCode && u.LoadId == loadId);
                if (loadHuIdList.Count > 0)
                {
                    string[] huid = (from a in loadHuIdList select a.HuId).Distinct().ToArray();

                    var sql = from a in idal.IHuDetailDAL.SelectAll()
                              join b in idal.IHuMasterDAL.SelectAll()
                              on new { a.HuId, a.WhCode } equals new { b.HuId, b.WhCode }
                              join c in idal.IWhLocationDAL.SelectAll()
                              on new { b.Location, b.WhCode } equals new { Location = c.LocationId, c.WhCode }
                              where b.Type == "M" && b.Status == "A" && c.LocationTypeId == 1 && (a.Qty - (a.PlanQty ?? 0)) > 0
                              && skuArr.Contains(a.AltItemNumber) && clientCodeArr.Contains(a.ClientCode) && huid.Contains(a.HuId) &&
                            b.Location == "ROC1"
                              group new { a, c, b } by new
                              {
                                  a.WhCode,
                                  a.ClientId,
                                  a.ClientCode,
                                  a.SoNumber,
                                  a.CustomerPoNumber,
                                  a.AltItemNumber,
                                  a.ItemId,
                                  a.UnitId,
                                  a.UnitName,
                                  a.LotNumber1,
                                  a.LotNumber2,
                                  a.LotDate
                              } into g
                              select new HuDetailResult
                              {
                                  WhCode = g.Key.WhCode,
                                  ClientId = g.Key.ClientId,
                                  ClientCode = g.Key.ClientCode,
                                  SoNumber = g.Key.SoNumber ?? "",
                                  CustomerPoNumber = g.Key.CustomerPoNumber ?? "",
                                  AltItemNumber = g.Key.AltItemNumber,
                                  Qty = g.Sum(u => u.a.Qty - (u.a.PlanQty ?? 0)),
                                  ItemId = g.Key.ItemId,
                                  UnitId = g.Key.UnitId,
                                  UnitName = g.Key.UnitName ?? "",
                                  LotNumber1 = g.Key.LotNumber1 ?? "",
                                  LotNumber2 = g.Key.LotNumber2 ?? "",
                                  LotDate = g.Key.LotDate
                              };

                    List = sql.ToList();
                }
                else
                {
                    var sql = from a in idal.IHuDetailDAL.SelectAll()
                              join b in idal.IHuMasterDAL.SelectAll()
                              on new { a.HuId, a.WhCode } equals new { b.HuId, b.WhCode }
                              join c in idal.IWhLocationDAL.SelectAll()
                              on new { b.Location, b.WhCode } equals new { Location = c.LocationId, c.WhCode }
                              where b.Type == "M" && b.Status == "A" && c.LocationTypeId == 1 && (a.Qty - (a.PlanQty ?? 0)) > 0
                              && skuArr.Contains(a.AltItemNumber) && clientCodeArr.Contains(a.ClientCode) &&
                            b.Location == "ROC1"
                              group new { a, c, b } by new
                              {
                                  a.WhCode,
                                  a.ClientId,
                                  a.ClientCode,
                                  a.SoNumber,
                                  a.CustomerPoNumber,
                                  a.AltItemNumber,
                                  a.ItemId,
                                  a.UnitId,
                                  a.UnitName,
                                  a.LotNumber1,
                                  a.LotNumber2,
                                  a.LotDate
                              } into g
                              select new HuDetailResult
                              {
                                  WhCode = g.Key.WhCode,
                                  ClientId = g.Key.ClientId,
                                  ClientCode = g.Key.ClientCode,
                                  SoNumber = g.Key.SoNumber ?? "",
                                  CustomerPoNumber = g.Key.CustomerPoNumber ?? "",
                                  AltItemNumber = g.Key.AltItemNumber,
                                  Qty = g.Sum(u => u.a.Qty - (u.a.PlanQty ?? 0)),
                                  ItemId = g.Key.ItemId,
                                  UnitId = g.Key.UnitId,
                                  UnitName = g.Key.UnitName ?? "",
                                  LotNumber1 = g.Key.LotNumber1 ?? "",
                                  LotNumber2 = g.Key.LotNumber2 ?? "",
                                  LotDate = g.Key.LotDate
                              };

                    List = sql.ToList();
                }

                #endregion
            }
            else
            {
                #region 正常库存匹配

                List<LoadHuIdExtend> loadHuIdList = idal.ILoadHuIdExtendDAL.SelectBy(u => u.WhCode == whCode && u.LoadId == loadId);
                if (loadHuIdList.Count > 0)
                {
                    string[] huid = (from a in loadHuIdList select a.HuId).Distinct().ToArray();

                    var sql = from a in idal.IHuDetailDAL.SelectAll()
                              join b in idal.IHuMasterDAL.SelectAll()
                              on new { a.HuId, a.WhCode } equals new { b.HuId, b.WhCode }
                              join c in idal.IWhLocationDAL.SelectAll()
                              on new { b.Location, b.WhCode } equals new { Location = c.LocationId, c.WhCode }
                              where b.Type == "M" && b.Status == "A" && c.LocationTypeId == 1 && (a.Qty - (a.PlanQty ?? 0)) > 0
                              && skuArr.Contains(a.AltItemNumber) && clientCodeArr.Contains(a.ClientCode) && a.WhCode == whCode && huid.Contains(a.HuId)
                              group new { a, c, b } by new
                              {
                                  a.WhCode,
                                  a.ClientId,
                                  a.ClientCode,
                                  a.SoNumber,
                                  a.CustomerPoNumber,
                                  a.AltItemNumber,
                                  a.ItemId,
                                  a.UnitId,
                                  a.UnitName,
                                  a.LotNumber1,
                                  a.LotNumber2,
                                  a.LotDate
                              } into g
                              select new HuDetailResult
                              {
                                  WhCode = g.Key.WhCode,
                                  ClientId = g.Key.ClientId,
                                  ClientCode = g.Key.ClientCode,
                                  SoNumber = g.Key.SoNumber ?? "",
                                  CustomerPoNumber = g.Key.CustomerPoNumber ?? "",
                                  AltItemNumber = g.Key.AltItemNumber,
                                  Qty = g.Sum(u => u.a.Qty - (u.a.PlanQty ?? 0)),
                                  ItemId = g.Key.ItemId,
                                  UnitId = g.Key.UnitId,
                                  UnitName = g.Key.UnitName ?? "",
                                  LotNumber1 = g.Key.LotNumber1 ?? "",
                                  LotNumber2 = g.Key.LotNumber2 ?? "",
                                  LotDate = g.Key.LotDate
                              };

                    List = sql.ToList();
                }
                else
                {
                    var sql = from a in idal.IHuDetailDAL.SelectAll()
                              join b in idal.IHuMasterDAL.SelectAll()
                              on new { a.HuId, a.WhCode } equals new { b.HuId, b.WhCode }
                              join c in idal.IWhLocationDAL.SelectAll()
                              on new { b.Location, b.WhCode } equals new { Location = c.LocationId, c.WhCode }
                              where b.Type == "M" && b.Status == "A" && c.LocationTypeId == 1 && (a.Qty - (a.PlanQty ?? 0)) > 0
                              && skuArr.Contains(a.AltItemNumber) && clientCodeArr.Contains(a.ClientCode) && a.WhCode == whCode
                              group new { a, c, b } by new
                              {
                                  a.WhCode,
                                  a.ClientId,
                                  a.ClientCode,
                                  a.SoNumber,
                                  a.CustomerPoNumber,
                                  a.AltItemNumber,
                                  a.ItemId,
                                  a.UnitId,
                                  a.UnitName,
                                  a.LotNumber1,
                                  a.LotNumber2,
                                  a.LotDate
                              } into g
                              select new HuDetailResult
                              {
                                  WhCode = g.Key.WhCode,
                                  ClientId = g.Key.ClientId,
                                  ClientCode = g.Key.ClientCode,
                                  SoNumber = g.Key.SoNumber ?? "",
                                  CustomerPoNumber = g.Key.CustomerPoNumber ?? "",
                                  AltItemNumber = g.Key.AltItemNumber,
                                  Qty = g.Sum(u => u.a.Qty - (u.a.PlanQty ?? 0)),
                                  ItemId = g.Key.ItemId,
                                  UnitId = g.Key.UnitId,
                                  UnitName = g.Key.UnitName ?? "",
                                  LotNumber1 = g.Key.LotNumber1 ?? "",
                                  LotNumber2 = g.Key.LotNumber2 ?? "",
                                  LotDate = g.Key.LotDate
                              };

                    List = sql.ToList();
                }

                #endregion
            }

            List<ReleaseLoadDetail> resultList = new List<ReleaseLoadDetail>();

            //entityList = entityList.Where(u => u.AltItemNumber == "AW15521").ToList();
            foreach (var item in entityList)
            {
                #region 循环匹配出货明细与库存

                if (List.Where(u => u.AltItemNumber == item.AltItemNumber && u.ClientId == item.ClientId).Count() == 0)
                {
                    var sql2 = from a in idal.IHuDetailDAL.SelectAll()
                               join b in idal.IHuMasterDAL.SelectAll()
                               on new { a.HuId, a.WhCode } equals new { b.HuId, b.WhCode }
                               join c in idal.IWhLocationDAL.SelectAll()
                               on new { b.Location, b.WhCode } equals new { Location = c.LocationId, c.WhCode }
                               where skuArr.Contains(a.AltItemNumber) && clientCodeArr.Contains(a.ClientCode) && a.WhCode == whCode
                                && (a.Qty - (a.PlanQty ?? 0)) > 0 && !a.HuId.Contains("LD")
                               group new { a, c, b } by new
                               {
                                   a.WhCode,
                                   a.ClientId,
                                   a.ClientCode,
                                   a.SoNumber,
                                   a.CustomerPoNumber,
                                   a.AltItemNumber,
                                   a.ItemId,
                                   a.UnitId,
                                   a.UnitName,
                                   a.LotNumber1,
                                   a.LotNumber2,
                                   a.LotDate,
                                   b.Type,
                                   b.Status,
                                   c.LocationTypeId
                               } into g
                               select new HuDetailResult1
                               {
                                   WhCode = g.Key.WhCode,
                                   ClientId = g.Key.ClientId,
                                   ClientCode = g.Key.ClientCode,
                                   SoNumber = g.Key.SoNumber ?? "",
                                   CustomerPoNumber = g.Key.CustomerPoNumber ?? "",
                                   AltItemNumber = g.Key.AltItemNumber,
                                   Qty = g.Sum(u => u.a.Qty - (u.a.PlanQty ?? 0)),
                                   ItemId = g.Key.ItemId,
                                   UnitId = g.Key.UnitId,
                                   UnitName = g.Key.UnitName ?? "",
                                   LotNumber1 = g.Key.LotNumber1 ?? "",
                                   LotNumber2 = g.Key.LotNumber2 ?? "",
                                   LotDate = g.Key.LotDate,
                                   Type = g.Key.Type,
                                   Status = g.Key.Status,
                                   LocationTypeId = g.Key.LocationTypeId
                               };

                    List<HuDetailResult1> checkInventory = sql2.ToList();
                    ReleaseLoadDetail entity = new ReleaseLoadDetail();

                    if (checkInventory.Count == 0)
                    {
                        entity.Description = "该款号无有效库存，库存中无该款号信息";
                    }
                    else
                    {
                        if (checkInventory.Where(u => u.LocationTypeId == 2).Count() > 0)
                        {
                            entity.Description = "该款号无有效库存，库存托盘未上架";
                        }
                        else if (checkInventory.Where(u => u.Status == "H").Count() > 0)
                        {
                            entity.Description = "该款号无有效库存，库存托盘存在冻结";
                        }
                        else if (checkInventory.Where(u => u.Type == "R").Count() > 0)
                        {
                            entity.Description = "该款号无有效库存，库存托盘正在收货中";
                        }
                        else if (checkInventory.Where(u => u.LocationTypeId == 5).Count() > 0)
                        {
                            entity.Description = "该款号无有效库存，库存托盘在货损库位";
                        }
                        else if (checkInventory.Where(u => u.LocationTypeId == 6).Count() > 0)
                        {
                            entity.Description = "该款号无有效库存，库存托盘在异常库位";
                        }
                        else
                        {
                            entity.Description = "该款号无有效库存，库存托盘存在异常情况";
                        }
                    }

                    if (mark == "1" || mark == "2" || mark == "9")
                    {
                        entity.ReleaseMark = "匹配SKU-ItemId-单位-SO-PO-Lot1-Lot2";
                    }
                    else if (mark == "3" || mark == "4")
                    {
                        entity.ReleaseMark = "匹配SKU-ItemId-单位-Lot1-Lot2";
                    }
                    else if (mark == "5" || mark == "6")
                    {
                        entity.ReleaseMark = "匹配SKU-ItemId-单位-PO-Lot1-Lot2";
                    }
                    else if (mark == "7")
                    {
                        entity.ReleaseMark = "匹配SKU-ItemId-单位-Lot1-Lot2";
                    }
                    else if (mark == "11")
                    {
                        entity.ReleaseMark = "指定库位ROC1";
                    }

                    entity.WhCode = whCode;
                    entity.LoadId = loadId;
                    entity.ClientCode = item.ClientCode;
                    entity.Qty = 0;
                    entity.OutSoNumber = item.SoNumber;
                    entity.OutCustomerPoNumber = item.CustomerPoNumber;
                    entity.OutAltItemNumber = item.AltItemNumber;
                    entity.OutItemId = item.ItemId;
                    entity.OutUnitName = item.UnitName;
                    entity.OutUnitId = item.UnitId;
                    entity.OutQty = item.Qty;
                    entity.OutLotNumber1 = item.LotNumber1;
                    entity.OutLotNumber2 = item.LotNumber2;
                    entity.OutLotDate = item.LotDate;
                    resultList.Add(entity);
                    continue;
                }
                else
                {
                    int sumHuQty = Convert.ToInt32(List.Where(u => u.AltItemNumber == item.AltItemNumber && u.ClientId == item.ClientId).Sum(u => u.Qty).ToString());

                    if (sumHuQty < item.Qty)
                    {
                        ReleaseLoadDetail entity = new ReleaseLoadDetail();
                        entity.Description = "该行根据释放条件匹配SKU时有效库存不足：有效库存数量" + sumHuQty + "出货数量" + item.Qty;

                        if (mark == "1" || mark == "2" || mark == "9")
                        {
                            entity.ReleaseMark = "匹配SKU-ItemId-单位-SO-PO-Lot1-Lot2";
                        }
                        else if (mark == "3" || mark == "4")
                        {
                            entity.ReleaseMark = "匹配SKU-ItemId-单位-Lot1-Lot2";
                        }
                        else if (mark == "5" || mark == "6")
                        {
                            entity.ReleaseMark = "匹配SKU-ItemId-单位-PO-Lot1-Lot2";
                        }
                        else if (mark == "7")
                        {
                            entity.ReleaseMark = "匹配SKU-ItemId-单位-Lot1-Lot2";
                        }
                        else if (mark == "11")
                        {
                            entity.ReleaseMark = "指定库位ROC1";
                        }

                        entity.WhCode = whCode;
                        entity.LoadId = loadId;
                        entity.ClientCode = item.ClientCode;
                        entity.Qty = sumHuQty;
                        entity.OutSoNumber = item.SoNumber;
                        entity.OutCustomerPoNumber = item.CustomerPoNumber;
                        entity.OutAltItemNumber = item.AltItemNumber;
                        entity.OutItemId = item.ItemId;
                        entity.OutUnitName = item.UnitName;
                        entity.OutUnitId = item.UnitId;
                        entity.OutQty = item.Qty;
                        entity.OutLotNumber1 = item.LotNumber1;
                        entity.OutLotNumber2 = item.LotNumber2;
                        entity.OutLotDate = item.LotDate;
                        resultList.Add(entity);
                        continue;
                    }
                    else
                    {

                        List1.Clear();
                        List1 = List.Where(u => u.AltItemNumber == item.AltItemNumber && u.ClientId == item.ClientId && u.ItemId == item.ItemId).ToList();

                        #region 匹配款号ID

                        if (List1.Count == 0)
                        {
                            ReleaseLoadDetail entity = new ReleaseLoadDetail();
                            entity.Description = "该行根据释放条件匹配至ItemId时无有效库存,ItemId：" + item.ItemId;

                            if (mark == "1" || mark == "2" || mark == "9")
                            {
                                entity.ReleaseMark = "匹配SKU-ItemId-单位-SO-PO-Lot1-Lot2";
                            }
                            else if (mark == "3" || mark == "4")
                            {
                                entity.ReleaseMark = "匹配SKU-ItemId-单位-Lot1-Lot2";
                            }
                            else if (mark == "5" || mark == "6")
                            {
                                entity.ReleaseMark = "匹配SKU-ItemId-单位-PO-Lot1-Lot2";
                            }
                            else if (mark == "7")
                            {
                                entity.ReleaseMark = "匹配SKU-ItemId-单位-Lot1-Lot2";
                            }
                            else if (mark == "11")
                            {
                                entity.ReleaseMark = "指定库位ROC1";
                            }
                            entity.WhCode = whCode;
                            entity.LoadId = loadId;
                            entity.ClientCode = item.ClientCode;
                            entity.OutSoNumber = item.SoNumber;
                            entity.OutCustomerPoNumber = item.CustomerPoNumber;
                            entity.OutAltItemNumber = item.AltItemNumber;
                            entity.OutItemId = item.ItemId;
                            entity.OutUnitName = item.UnitName;
                            entity.OutUnitId = item.UnitId;
                            entity.OutQty = item.Qty;
                            entity.OutLotNumber1 = item.LotNumber1;
                            entity.OutLotNumber2 = item.LotNumber2;
                            entity.OutLotDate = item.LotDate;
                            resultList.Add(entity);
                            continue;
                        }
                        else
                        {
                            sumHuQty = Convert.ToInt32(List.Where(u => u.AltItemNumber == item.AltItemNumber && u.ClientId == item.ClientId && u.ItemId == item.ItemId).Sum(u => u.Qty).ToString());

                            if (sumHuQty < item.Qty)
                            {
                                ReleaseLoadDetail entity = new ReleaseLoadDetail();
                                entity.Description = "该行根据释放条件匹配至ItemId时有效库存不足,ItemId：" + item.ItemId + "有效库存数量" + sumHuQty + "出货数量" + item.Qty;

                                if (mark == "1" || mark == "2" || mark == "9")
                                {
                                    entity.ReleaseMark = "匹配SKU-ItemId-单位-SO-PO-Lot1-Lot2";
                                }
                                else if (mark == "3" || mark == "4")
                                {
                                    entity.ReleaseMark = "匹配SKU-ItemId-单位-Lot1-Lot2";
                                }
                                else if (mark == "5" || mark == "6")
                                {
                                    entity.ReleaseMark = "匹配SKU-ItemId-单位-PO-Lot1-Lot2";
                                }
                                else if (mark == "7")
                                {
                                    entity.ReleaseMark = "匹配SKU-ItemId-单位-Lot1-Lot2";
                                }
                                else if (mark == "11")
                                {
                                    entity.ReleaseMark = "指定库位ROC1";
                                }

                                entity.WhCode = whCode;
                                entity.LoadId = loadId;
                                entity.ClientCode = item.ClientCode;
                                entity.Qty = sumHuQty;
                                entity.OutSoNumber = item.SoNumber;
                                entity.OutCustomerPoNumber = item.CustomerPoNumber;
                                entity.OutAltItemNumber = item.AltItemNumber;
                                entity.OutItemId = item.ItemId;
                                entity.OutUnitName = item.UnitName;
                                entity.OutUnitId = item.UnitId;
                                entity.OutQty = item.Qty;
                                entity.OutLotNumber1 = item.LotNumber1;
                                entity.OutLotNumber2 = item.LotNumber2;
                                entity.OutLotDate = item.LotDate;
                                resultList.Add(entity);
                                continue;
                            }
                        }
                        #endregion

                        List1.Clear();
                        List1 = List.Where(u => u.AltItemNumber == item.AltItemNumber && u.UnitName == item.UnitName && u.ClientId == item.ClientId && u.ItemId == item.ItemId).ToList();

                        #region 匹配单位

                        if (List1.Count == 0)
                        {
                            ReleaseLoadDetail entity = new ReleaseLoadDetail();
                            entity.Description = "该行根据释放条件匹配至单位时无有效库存,出货单位：" + item.UnitName;

                            if (mark == "1" || mark == "2" || mark == "9")
                            {
                                entity.ReleaseMark = "匹配SKU-ItemId-单位-SO-PO-Lot1-Lot2";
                            }
                            else if (mark == "3" || mark == "4")
                            {
                                entity.ReleaseMark = "匹配SKU-ItemId-单位-Lot1-Lot2";
                            }
                            else if (mark == "5" || mark == "6")
                            {
                                entity.ReleaseMark = "匹配SKU-ItemId-单位-PO-Lot1-Lot2";
                            }
                            else if (mark == "7")
                            {
                                entity.ReleaseMark = "匹配SKU-ItemId-单位-Lot1-Lot2";
                            }
                            else if (mark == "11")
                            {
                                entity.ReleaseMark = "指定库位ROC1";
                            }
                            entity.WhCode = whCode;
                            entity.LoadId = loadId;
                            entity.ClientCode = item.ClientCode;
                            entity.OutSoNumber = item.SoNumber;
                            entity.OutCustomerPoNumber = item.CustomerPoNumber;
                            entity.OutAltItemNumber = item.AltItemNumber;
                            entity.OutItemId = item.ItemId;
                            entity.OutUnitName = item.UnitName;
                            entity.OutUnitId = item.UnitId;
                            entity.OutQty = item.Qty;
                            entity.OutLotNumber1 = item.LotNumber1;
                            entity.OutLotNumber2 = item.LotNumber2;
                            entity.OutLotDate = item.LotDate;
                            resultList.Add(entity);
                            continue;
                        }
                        else
                        {
                            sumHuQty = Convert.ToInt32(List.Where(u => u.AltItemNumber == item.AltItemNumber && u.UnitName == item.UnitName && u.ClientId == item.ClientId).Sum(u => u.Qty).ToString());

                            if (sumHuQty < item.Qty)
                            {
                                ReleaseLoadDetail entity = new ReleaseLoadDetail();
                                entity.Description = "该行根据释放条件匹配至单位时有效库存不足,出货单位：" + item.UnitName + "有效库存数量" + sumHuQty + "出货数量" + item.Qty;

                                if (mark == "1" || mark == "2" || mark == "9")
                                {
                                    entity.ReleaseMark = "匹配SKU-ItemId-单位-SO-PO-Lot1-Lot2";
                                }
                                else if (mark == "3" || mark == "4")
                                {
                                    entity.ReleaseMark = "匹配SKU-ItemId-单位-Lot1-Lot2";
                                }
                                else if (mark == "5" || mark == "6")
                                {
                                    entity.ReleaseMark = "匹配SKU-ItemId-单位-PO-Lot1-Lot2";
                                }
                                else if (mark == "7")
                                {
                                    entity.ReleaseMark = "匹配SKU-ItemId-单位-Lot1-Lot2";
                                }
                                else if (mark == "11")
                                {
                                    entity.ReleaseMark = "指定库位ROC1";
                                }

                                entity.WhCode = whCode;
                                entity.LoadId = loadId;
                                entity.ClientCode = item.ClientCode;
                                entity.Qty = sumHuQty;
                                entity.OutSoNumber = item.SoNumber;
                                entity.OutCustomerPoNumber = item.CustomerPoNumber;
                                entity.OutAltItemNumber = item.AltItemNumber;
                                entity.OutItemId = item.ItemId;
                                entity.OutUnitName = item.UnitName;
                                entity.OutUnitId = item.UnitId;
                                entity.OutQty = item.Qty;
                                entity.OutLotNumber1 = item.LotNumber1;
                                entity.OutLotNumber2 = item.LotNumber2;
                                entity.OutLotDate = item.LotDate;
                                resultList.Add(entity);
                                continue;
                            }
                        }
                        #endregion


                        if (mark == "1" || mark == "2" || mark == "9")
                        {
                            #region 匹配SO PO

                            List1.Clear();
                            List1 = List.Where(u => u.AltItemNumber == item.AltItemNumber && u.UnitName == item.UnitName && u.SoNumber == item.SoNumber && u.ClientId == item.ClientId && u.ItemId == item.ItemId).ToList();

                            if (List1.Count == 0)
                            {
                                ReleaseLoadDetail entity = new ReleaseLoadDetail();
                                entity.Description = "该行根据释放条件匹配至SO时有效库存不足,SO：" + item.SoNumber;

                                if (mark == "1" || mark == "2" || mark == "9")
                                {
                                    entity.ReleaseMark = "匹配SKU-ItemId-单位-SO-PO-Lot1-Lot2";
                                }
                                else if (mark == "3" || mark == "4")
                                {
                                    entity.ReleaseMark = "匹配SKU-ItemId-单位-Lot1-Lot2";
                                }
                                else if (mark == "5" || mark == "6")
                                {
                                    entity.ReleaseMark = "匹配SKU-ItemId-单位-PO-Lot1-Lot2";
                                }
                                else if (mark == "7")
                                {
                                    entity.ReleaseMark = "匹配SKU-ItemId-单位-Lot1-Lot2";
                                }
                                else if (mark == "11")
                                {
                                    entity.ReleaseMark = "指定库位ROC1";
                                }

                                entity.WhCode = whCode;
                                entity.LoadId = loadId;
                                entity.ClientCode = item.ClientCode;
                                entity.OutSoNumber = item.SoNumber;
                                entity.OutCustomerPoNumber = item.CustomerPoNumber;
                                entity.OutAltItemNumber = item.AltItemNumber;
                                entity.OutItemId = item.ItemId;
                                entity.OutUnitName = item.UnitName;
                                entity.OutUnitId = item.UnitId;
                                entity.OutQty = item.Qty;
                                entity.OutLotNumber1 = item.LotNumber1;
                                entity.OutLotNumber2 = item.LotNumber2;
                                entity.OutLotDate = item.LotDate;
                                resultList.Add(entity);
                                continue;
                            }
                            else
                            {
                                sumHuQty = Convert.ToInt32(List.Where(u => u.AltItemNumber == item.AltItemNumber && u.UnitName == item.UnitName && u.SoNumber == item.SoNumber && u.ClientId == item.ClientId && u.ItemId == item.ItemId).Sum(u => u.Qty).ToString());

                                if (sumHuQty < item.Qty)
                                {
                                    ReleaseLoadDetail entity = new ReleaseLoadDetail();
                                    entity.Description = "该行根据释放条件匹配至SO时有效库存不足,SO：" + item.SoNumber + "有效库存数量" + sumHuQty + "出货数量" + item.Qty;

                                    if (mark == "1" || mark == "2" || mark == "9")
                                    {
                                        entity.ReleaseMark = "匹配SKU-ItemId-单位-SO-PO-Lot1-Lot2";
                                    }
                                    else if (mark == "3" || mark == "4")
                                    {
                                        entity.ReleaseMark = "匹配SKU-ItemId-单位-Lot1-Lot2";
                                    }
                                    else if (mark == "5" || mark == "6")
                                    {
                                        entity.ReleaseMark = "匹配SKU-ItemId-单位-PO-Lot1-Lot2";
                                    }
                                    else if (mark == "7")
                                    {
                                        entity.ReleaseMark = "匹配SKU-ItemId-单位-Lot1-Lot2";
                                    }
                                    else if (mark == "11")
                                    {
                                        entity.ReleaseMark = "指定库位ROC1";
                                    }

                                    entity.WhCode = whCode;
                                    entity.LoadId = loadId;
                                    entity.ClientCode = item.ClientCode;
                                    entity.Qty = sumHuQty;
                                    entity.OutSoNumber = item.SoNumber;
                                    entity.OutCustomerPoNumber = item.CustomerPoNumber;
                                    entity.OutAltItemNumber = item.AltItemNumber;
                                    entity.OutItemId = item.ItemId;
                                    entity.OutUnitName = item.UnitName;
                                    entity.OutUnitId = item.UnitId;
                                    entity.OutQty = item.Qty;
                                    entity.OutLotNumber1 = item.LotNumber1;
                                    entity.OutLotNumber2 = item.LotNumber2;
                                    entity.OutLotDate = item.LotDate;
                                    resultList.Add(entity);
                                    continue;
                                }
                            }

                            List1.Clear();
                            List1 = List.Where(u => u.AltItemNumber == item.AltItemNumber && u.UnitName == item.UnitName && u.SoNumber == item.SoNumber && u.CustomerPoNumber == item.CustomerPoNumber && u.ClientId == item.ClientId && u.ItemId == item.ItemId).ToList();

                            if (List1.Count == 0)
                            {
                                ReleaseLoadDetail entity = new ReleaseLoadDetail();
                                entity.Description = "该行根据释放条件匹配至PO时无有效库存,PO：" + item.CustomerPoNumber;

                                if (mark == "1" || mark == "2" || mark == "9")
                                {
                                    entity.ReleaseMark = "匹配SKU-ItemId-单位-SO-PO-Lot1-Lot2";
                                }
                                else if (mark == "3" || mark == "4")
                                {
                                    entity.ReleaseMark = "匹配SKU-ItemId-单位-Lot1-Lot2";
                                }
                                else if (mark == "5" || mark == "6")
                                {
                                    entity.ReleaseMark = "匹配SKU-ItemId-单位-PO-Lot1-Lot2";
                                }
                                else if (mark == "7")
                                {
                                    entity.ReleaseMark = "匹配SKU-ItemId-单位-Lot1-Lot2";
                                }
                                else if (mark == "11")
                                {
                                    entity.ReleaseMark = "指定库位ROC1";
                                }

                                entity.WhCode = whCode;
                                entity.LoadId = loadId;
                                entity.ClientCode = item.ClientCode;
                                entity.OutSoNumber = item.SoNumber;
                                entity.OutCustomerPoNumber = item.CustomerPoNumber;
                                entity.OutAltItemNumber = item.AltItemNumber;
                                entity.OutItemId = item.ItemId;
                                entity.OutUnitName = item.UnitName;
                                entity.OutUnitId = item.UnitId;
                                entity.OutQty = item.Qty;
                                entity.OutLotNumber1 = item.LotNumber1;
                                entity.OutLotNumber2 = item.LotNumber2;
                                entity.OutLotDate = item.LotDate;
                                resultList.Add(entity);
                                continue;
                            }
                            else
                            {
                                sumHuQty = Convert.ToInt32(List.Where(u => u.AltItemNumber == item.AltItemNumber && u.UnitName == item.UnitName && u.SoNumber == item.SoNumber && u.CustomerPoNumber == item.CustomerPoNumber && u.ClientId == item.ClientId && u.ItemId == item.ItemId).Sum(u => u.Qty).ToString());

                                if (sumHuQty < item.Qty)
                                {
                                    ReleaseLoadDetail entity = new ReleaseLoadDetail();
                                    entity.Description = "该行根据释放条件匹配至PO时有效库存不足,PO：" + item.CustomerPoNumber + "有效库存数量" + sumHuQty + "出货数量" + item.Qty;

                                    if (mark == "1" || mark == "2" || mark == "9")
                                    {
                                        entity.ReleaseMark = "匹配SKU-ItemId-单位-SO-PO-Lot1-Lot2";
                                    }
                                    else if (mark == "3" || mark == "4")
                                    {
                                        entity.ReleaseMark = "匹配SKU-ItemId-单位-Lot1-Lot2";
                                    }
                                    else if (mark == "5" || mark == "6")
                                    {
                                        entity.ReleaseMark = "匹配SKU-ItemId-单位-PO-Lot1-Lot2";
                                    }
                                    else if (mark == "7")
                                    {
                                        entity.ReleaseMark = "匹配SKU-ItemId-单位-Lot1-Lot2";
                                    }
                                    else if (mark == "11")
                                    {
                                        entity.ReleaseMark = "指定库位ROC1";
                                    }

                                    entity.WhCode = whCode;
                                    entity.LoadId = loadId;
                                    entity.ClientCode = item.ClientCode;
                                    entity.Qty = sumHuQty;
                                    entity.OutSoNumber = item.SoNumber;
                                    entity.OutCustomerPoNumber = item.CustomerPoNumber;
                                    entity.OutAltItemNumber = item.AltItemNumber;
                                    entity.OutItemId = item.ItemId;
                                    entity.OutUnitName = item.UnitName;
                                    entity.OutUnitId = item.UnitId;
                                    entity.OutQty = item.Qty;
                                    entity.OutLotNumber1 = item.LotNumber1;
                                    entity.OutLotNumber2 = item.LotNumber2;
                                    entity.OutLotDate = item.LotDate;
                                    resultList.Add(entity);
                                    continue;
                                }
                            }

                            #endregion

                        }
                        else if (mark == "5" || mark == "6")
                        {
                            #region 匹配PO

                            List1.Clear();
                            List1 = List.Where(u => u.AltItemNumber == item.AltItemNumber && u.UnitName == item.UnitName && u.LotNumber1 == item.LotNumber1 && u.LotNumber2 == item.LotNumber2 && u.CustomerPoNumber == item.CustomerPoNumber && u.ClientId == item.ClientId && u.ItemId == item.ItemId).ToList();

                            if (List1.Count == 0)
                            {
                                ReleaseLoadDetail entity = new ReleaseLoadDetail();
                                entity.Description = "该行根据释放条件匹配至PO时无有效库存,PO：" + item.CustomerPoNumber;

                                if (mark == "1" || mark == "2" || mark == "9")
                                {
                                    entity.ReleaseMark = "匹配SKU-ItemId-单位-SO-PO-Lot1-Lot2";
                                }
                                else if (mark == "3" || mark == "4")
                                {
                                    entity.ReleaseMark = "匹配SKU-ItemId-单位-Lot1-Lot2";
                                }
                                else if (mark == "5" || mark == "6")
                                {
                                    entity.ReleaseMark = "匹配SKU-ItemId-单位-PO-Lot1-Lot2";
                                }
                                else if (mark == "7")
                                {
                                    entity.ReleaseMark = "匹配SKU-ItemId-单位-Lot1-Lot2";
                                }
                                else if (mark == "11")
                                {
                                    entity.ReleaseMark = "指定库位ROC1";
                                }

                                entity.WhCode = whCode;
                                entity.LoadId = loadId;
                                entity.ClientCode = item.ClientCode;
                                entity.OutSoNumber = item.SoNumber;
                                entity.OutCustomerPoNumber = item.CustomerPoNumber;
                                entity.OutAltItemNumber = item.AltItemNumber;
                                entity.OutItemId = item.ItemId;
                                entity.OutUnitName = item.UnitName;
                                entity.OutUnitId = item.UnitId;
                                entity.OutQty = item.Qty;
                                entity.OutLotNumber1 = item.LotNumber1;
                                entity.OutLotNumber2 = item.LotNumber2;
                                entity.OutLotDate = item.LotDate;
                                resultList.Add(entity);
                                continue;
                            }
                            else
                            {
                                sumHuQty = Convert.ToInt32(List.Where(u => u.AltItemNumber == item.AltItemNumber && u.UnitName == item.UnitName && u.LotNumber1 == item.LotNumber1 && u.LotNumber2 == item.LotNumber2 && u.CustomerPoNumber == item.CustomerPoNumber && u.ClientId == item.ClientId && u.ItemId == item.ItemId).Sum(u => u.Qty).ToString());

                                if (sumHuQty < item.Qty)
                                {
                                    ReleaseLoadDetail entity = new ReleaseLoadDetail();
                                    entity.Description = "该行根据释放条件匹配至PO时有效库存不足,PO：" + item.CustomerPoNumber + "有效库存数量" + sumHuQty + "出货数量" + item.Qty;

                                    if (mark == "1" || mark == "2" || mark == "9")
                                    {
                                        entity.ReleaseMark = "匹配SKU-ItemId-单位-SO-PO-Lot1-Lot2";
                                    }
                                    else if (mark == "3" || mark == "4")
                                    {
                                        entity.ReleaseMark = "匹配SKU-ItemId-单位-Lot1-Lot2";
                                    }
                                    else if (mark == "5" || mark == "6")
                                    {
                                        entity.ReleaseMark = "匹配SKU-ItemId-单位-PO-Lot1-Lot2";
                                    }
                                    else if (mark == "7")
                                    {
                                        entity.ReleaseMark = "匹配SKU-ItemId-单位-Lot1-Lot2";
                                    }
                                    else if (mark == "11")
                                    {
                                        entity.ReleaseMark = "指定库位ROC1";
                                    }
                                    entity.WhCode = whCode;
                                    entity.LoadId = loadId;
                                    entity.ClientCode = item.ClientCode;
                                    entity.Qty = sumHuQty;
                                    entity.OutSoNumber = item.SoNumber;
                                    entity.OutCustomerPoNumber = item.CustomerPoNumber;
                                    entity.OutAltItemNumber = item.AltItemNumber;
                                    entity.OutItemId = item.ItemId;
                                    entity.OutUnitName = item.UnitName;
                                    entity.OutUnitId = item.UnitId;
                                    entity.OutQty = item.Qty;
                                    entity.OutLotNumber1 = item.LotNumber1;
                                    entity.OutLotNumber2 = item.LotNumber2;
                                    entity.OutLotDate = item.LotDate;
                                    resultList.Add(entity);
                                    continue;
                                }
                            }

                            #endregion
                        }


                        if (mark == "1" || mark == "2" || mark == "3" || mark == "4" || mark == "5" || mark == "6" || mark == "7" || mark == "8" || mark == "9")
                        {
                            #region 匹配Lot1 Lot2 

                            List1.Clear();
                            List1 = List.Where(u => u.AltItemNumber == item.AltItemNumber && u.UnitName == item.UnitName && u.LotNumber1 == item.LotNumber1 && u.ClientId == item.ClientId && u.ItemId == item.ItemId).ToList();

                            if (List1.Count == 0)
                            {
                                ReleaseLoadDetail entity = new ReleaseLoadDetail();
                                entity.Description = "该行根据释放条件匹配至Lot1时无有效库存,Lot1：" + item.LotNumber1;

                                if (mark == "1" || mark == "2" || mark == "9")
                                {
                                    entity.ReleaseMark = "匹配SKU-ItemId-单位-SO-PO-Lot1-Lot2";
                                }
                                else if (mark == "3" || mark == "4")
                                {
                                    entity.ReleaseMark = "匹配SKU-ItemId-单位-Lot1-Lot2";
                                }
                                else if (mark == "5" || mark == "6")
                                {
                                    entity.ReleaseMark = "匹配SKU-ItemId-单位-PO-Lot1-Lot2";
                                }
                                else if (mark == "7")
                                {
                                    entity.ReleaseMark = "匹配SKU-ItemId-单位-Lot1-Lot2";
                                }
                                else if (mark == "11")
                                {
                                    entity.ReleaseMark = "指定库位ROC1";
                                }

                                entity.WhCode = whCode;
                                entity.LoadId = loadId;
                                entity.ClientCode = item.ClientCode;
                                entity.OutSoNumber = item.SoNumber;
                                entity.OutCustomerPoNumber = item.CustomerPoNumber;
                                entity.OutAltItemNumber = item.AltItemNumber;
                                entity.OutItemId = item.ItemId;
                                entity.OutUnitName = item.UnitName;
                                entity.OutUnitId = item.UnitId;
                                entity.OutQty = item.Qty;
                                entity.OutLotNumber1 = item.LotNumber1;
                                entity.OutLotNumber2 = item.LotNumber2;
                                entity.OutLotDate = item.LotDate;
                                resultList.Add(entity);
                                continue;
                            }
                            else
                            {
                                sumHuQty = Convert.ToInt32(List.Where(u => u.AltItemNumber == item.AltItemNumber && u.UnitName == item.UnitName && u.LotNumber1 == item.LotNumber1 && u.ClientId == item.ClientId && u.ItemId == item.ItemId).Sum(u => u.Qty).ToString());

                                if (sumHuQty < item.Qty)
                                {
                                    ReleaseLoadDetail entity = new ReleaseLoadDetail();
                                    entity.Description = "该行根据释放条件匹配至Lot1时有效库存不足,Lot1：" + item.LotNumber1 + "有效库存数量" + sumHuQty + "出货数量" + item.Qty;

                                    if (mark == "1" || mark == "2" || mark == "9")
                                    {
                                        entity.ReleaseMark = "匹配SKU-ItemId-单位-SO-PO-Lot1-Lot2";
                                    }
                                    else if (mark == "3" || mark == "4")
                                    {
                                        entity.ReleaseMark = "匹配SKU-ItemId-单位-Lot1-Lot2";
                                    }
                                    else if (mark == "5" || mark == "6")
                                    {
                                        entity.ReleaseMark = "匹配SKU-ItemId-单位-PO-Lot1-Lot2";
                                    }
                                    else if (mark == "7")
                                    {
                                        entity.ReleaseMark = "匹配SKU-ItemId-单位-Lot1-Lot2";
                                    }
                                    else if (mark == "11")
                                    {
                                        entity.ReleaseMark = "指定库位ROC1";
                                    }

                                    entity.WhCode = whCode;
                                    entity.LoadId = loadId;
                                    entity.ClientCode = item.ClientCode;
                                    entity.Qty = sumHuQty;
                                    entity.OutSoNumber = item.SoNumber;
                                    entity.OutCustomerPoNumber = item.CustomerPoNumber;
                                    entity.OutAltItemNumber = item.AltItemNumber;
                                    entity.OutItemId = item.ItemId;
                                    entity.OutUnitName = item.UnitName;
                                    entity.OutUnitId = item.UnitId;
                                    entity.OutQty = item.Qty;
                                    entity.OutLotNumber1 = item.LotNumber1;
                                    entity.OutLotNumber2 = item.LotNumber2;
                                    entity.OutLotDate = item.LotDate;
                                    resultList.Add(entity);
                                    continue;
                                }
                            }


                            List1.Clear();
                            List1 = List.Where(u => u.AltItemNumber == item.AltItemNumber && u.UnitName == item.UnitName && u.LotNumber1 == item.LotNumber1 && u.LotNumber2 == item.LotNumber2 && u.ClientId == item.ClientId && u.ItemId == item.ItemId).ToList();

                            if (List1.Count == 0)
                            {
                                ReleaseLoadDetail entity = new ReleaseLoadDetail();
                                entity.Description = "该行根据释放条件匹配至Lot2时无有效库存,Lot2：" + item.LotNumber2;

                                if (mark == "1" || mark == "2" || mark == "9")
                                {
                                    entity.ReleaseMark = "匹配SKU-ItemId-单位-SO-PO-Lot1-Lot2";
                                }
                                else if (mark == "3" || mark == "4")
                                {
                                    entity.ReleaseMark = "匹配SKU-ItemId-单位-Lot1-Lot2";
                                }
                                else if (mark == "5" || mark == "6")
                                {
                                    entity.ReleaseMark = "匹配SKU-ItemId-单位-PO-Lot1-Lot2";
                                }
                                else if (mark == "7")
                                {
                                    entity.ReleaseMark = "匹配SKU-ItemId-单位-Lot1-Lot2";
                                }
                                else if (mark == "11")
                                {
                                    entity.ReleaseMark = "指定库位ROC1";
                                }

                                entity.WhCode = whCode;
                                entity.LoadId = loadId;
                                entity.ClientCode = item.ClientCode;
                                entity.OutSoNumber = item.SoNumber;
                                entity.OutCustomerPoNumber = item.CustomerPoNumber;
                                entity.OutAltItemNumber = item.AltItemNumber;
                                entity.OutItemId = item.ItemId;
                                entity.OutUnitName = item.UnitName;
                                entity.OutUnitId = item.UnitId;
                                entity.OutQty = item.Qty;
                                entity.OutLotNumber1 = item.LotNumber1;
                                entity.OutLotNumber2 = item.LotNumber2;
                                entity.OutLotDate = item.LotDate;
                                resultList.Add(entity);
                                continue;
                            }
                            else
                            {
                                sumHuQty = Convert.ToInt32(List.Where(u => u.AltItemNumber == item.AltItemNumber && u.UnitName == item.UnitName && u.LotNumber1 == item.LotNumber1 && u.LotNumber2 == item.LotNumber2 && u.ClientId == item.ClientId && u.ItemId == item.ItemId).Sum(u => u.Qty).ToString());

                                if (sumHuQty < item.Qty)
                                {
                                    ReleaseLoadDetail entity = new ReleaseLoadDetail();
                                    entity.Description = "该行根据释放条件匹配至Lot2时有效库存不足,Lot2：" + item.LotNumber2 + "有效库存数量" + sumHuQty + "出货数量" + item.Qty;

                                    if (mark == "1" || mark == "2" || mark == "9")
                                    {
                                        entity.ReleaseMark = "匹配SKU-ItemId-单位-SO-PO-Lot1-Lot2";
                                    }
                                    else if (mark == "3" || mark == "4")
                                    {
                                        entity.ReleaseMark = "匹配SKU-ItemId-单位-Lot1-Lot2";
                                    }
                                    else if (mark == "5" || mark == "6")
                                    {
                                        entity.ReleaseMark = "匹配SKU-ItemId-单位-PO-Lot1-Lot2";
                                    }
                                    else if (mark == "7")
                                    {
                                        entity.ReleaseMark = "匹配SKU-ItemId-单位-Lot1-Lot2";
                                    }
                                    else if (mark == "11")
                                    {
                                        entity.ReleaseMark = "指定库位ROC1";
                                    }

                                    entity.WhCode = whCode;
                                    entity.LoadId = loadId;
                                    entity.ClientCode = item.ClientCode;
                                    entity.Qty = sumHuQty;
                                    entity.OutSoNumber = item.SoNumber;
                                    entity.OutCustomerPoNumber = item.CustomerPoNumber;
                                    entity.OutAltItemNumber = item.AltItemNumber;
                                    entity.OutItemId = item.ItemId;
                                    entity.OutUnitName = item.UnitName;
                                    entity.OutUnitId = item.UnitId;
                                    entity.OutQty = item.Qty;
                                    entity.OutLotNumber1 = item.LotNumber1;
                                    entity.OutLotNumber2 = item.LotNumber2;
                                    entity.OutLotDate = item.LotDate;
                                    resultList.Add(entity);
                                    continue;
                                }
                            }
                            #endregion
                        }

                    }
                }

                #endregion
            }

            idal.IReleaseLoadDetailDAL.Add(resultList);
            idal.IReleaseLoadDetailDAL.SaveChanges();
            return "Y";

        }

    }


}

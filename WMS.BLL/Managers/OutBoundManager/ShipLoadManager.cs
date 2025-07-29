using MODEL_MSSQL;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using WMS.BLLClass;
using WMS.IBLL;
using Newtonsoft.Json;

namespace WMS.BLL
{
    public class ShipLoadManager : IShipLoadManager
    {

        IDAL.IDALSession idal = BLL.BLLHelper.GetDal();
        private static object o = new object();

        public static object o1 = new object();
        public static object o2 = new object();

        ShipHelper shipHelper = new ShipHelper();

        //后台执行备货
        public string adminSetPickingLoad(string LoadId, string whCode, string userName, string Location)
        {
            List<PickTaskDetail> pickList = idal.IPickTaskDetailDAL.SelectBy(u => u.LoadId == LoadId && u.WhCode == whCode && u.Status == "U").ToList();
            List<string> checkList = new List<string>();

            List<LoadMaster> loadList = idal.ILoadMasterDAL.SelectBy(u => u.WhCode == whCode && u.LoadId == LoadId);
            LoadMaster load = loadList.First();

            FlowDetail flowDetail = idal.IFlowDetailDAL.SelectBy(u => u.FlowHeadId == load.ProcessId && u.Type == "Picking").First();

            if (flowDetail.Mark == "1") //1为ByLoad自动装箱
            {
                foreach (var pick in pickList)
                {
                    if (checkList.Where(u => u == pick.HuId).Count() > 0)
                    {
                        continue;
                    }
                    PickingLoadDC(LoadId, whCode, userName, pick.HuId, "TEST0007", Location);

                    //PickingLoad(pick.LoadId, pick.WhCode, userName, pick.HuId, "TEST0007", Location);
                    checkList.Add(pick.HuId);
                }
            }
            else if (flowDetail.Mark == "3")    //1为ByLoad不自动装箱
            {
                foreach (var pick in pickList)
                {
                    if (checkList.Where(u => u == pick.HuId).Count() > 0)
                    {
                        continue;
                    }
                    PickingLoadByBegin(LoadId, whCode, userName, pick.HuId, "TEST0007", Location);

                    checkList.Add(pick.HuId);
                }
            }
            else if (flowDetail.Mark == "4")    //4为ByLoad自动分拣
            {
                foreach (var pick in pickList)
                {
                    if (checkList.Where(u => u == pick.HuId).Count() > 0)
                    {
                        continue;
                    }
                    PickingSortingByLoadBegin(LoadId, whCode, userName, pick.HuId, "TEST0007", Location);

                    checkList.Add(pick.HuId);
                }
            }

            return "Y";
        }

        //后台执行装箱
        public string adminSetPackingLoad(string LoadId, string whCode, string userName)
        {
            List<PickTaskDetail> pickList = idal.IPickTaskDetailDAL.SelectBy(u => u.LoadId == LoadId && u.WhCode == whCode && u.Status1 == "U").ToList();
            foreach (var pick in pickList)
            {
                PackingLoad(pick.LoadId, pick.WhCode, pick.HuId, userName);
            }

            return "Y";
        }


        //检查Load及托盘 是否拆托
        public string CheckPickingLoad(string LoadId, string whCode, string HuId)
        {
            if (LoadId == null || LoadId == "" || whCode == "" || whCode == null || HuId == null || HuId == "")
            {
                return "数据有误，请重新操作！";
            }
            List<PickTaskDetail> pickList = idal.IPickTaskDetailDAL.SelectBy(u => u.WhCode == whCode && u.LoadId == LoadId && u.HuId == HuId && u.Status == "U");
            if (pickList.Count == 0)
            {
                return "该Load:" + LoadId + "对应的备货托盘：" + HuId + "状态有误或不存在，请检查备货任务！";
            }

            string result = "";
            List<HuDetail> huDetailList = idal.IHuDetailDAL.SelectBy(u => u.WhCode == whCode && u.HuId == HuId);

            int pickQty = pickList.Sum(u => u.Qty);
            int inventoryQty = huDetailList.Sum(u => u.Qty);

            if (pickQty == inventoryQty)
            {
                foreach (var item in pickList)
                {
                    if (result == "")
                    {
                        HuDetail huDetail = huDetailList.Where(u => u.Id == item.HuDetailId).First();
                        if (huDetail.Qty < item.Qty)
                        {
                            result = "错误！库存不足！";
                            continue;
                        }
                        if (huDetail.Qty > item.Qty)
                        {
                            result = "N";
                            continue;
                        }
                    }
                }
            }
            else
            {
                result = "N";
            }

            if (result == "")
            {
                return "Y";
            }
            else
            {
                return result;
            }

        }

        //备货
        public string PickingLoad(string LoadId, string whCode, string userName, string HuId, string PutHuId, string Location)
        {
            if (LoadId == null || LoadId == "" || whCode == "" || whCode == null || HuId == null || HuId == "" || userName == null || userName == "" || Location == null || Location == "")
            {
                return "数据有误，请重新操作！";
            }

            List<LoadMaster> loadList = idal.ILoadMasterDAL.SelectBy(u => u.WhCode == whCode && u.LoadId == LoadId);
            if (loadList.Count == 0)
            {
                return "未找到Load信息！";
            }


            LoadMaster load = loadList.First();

            FlowDetail flowDetail = idal.IFlowDetailDAL.SelectBy(u => u.FlowHeadId == load.ProcessId && u.Type == "Picking").First();

            string result = "";
            if (flowDetail.Mark == "1") //1为ByLoad自动装箱
            {
                result = PickingLoadDC(LoadId, whCode, userName, HuId, PutHuId, Location);
            }
            else if (flowDetail.Mark == "3")    //1为ByLoad不自动装箱
            {
                result = PickingLoadByBegin(LoadId, whCode, userName, HuId, PutHuId, Location);
            }
            else if (flowDetail.Mark == "4")    //4为ByLoad自动分拣
            {
                result = PickingSortingByLoadBegin(LoadId, whCode, userName, HuId, PutHuId, Location);
            }
            else if (flowDetail.Mark == "7")    //7为ByLoad不自动装箱,托盘备货后可重复使用继续备货
            {
                result = PickingLoadByBegin(LoadId, whCode, userName, HuId, PutHuId, Location, "1");
            }

            return result;

        }


        //ByLoad备货 不自动装箱
        public string PickingLoadByBegin(string LoadId, string whCode, string userName, string HuId, string PutHuId, string Location)
        {
            if (LoadId == null || LoadId == "" || whCode == "" || whCode == null || HuId == null || HuId == "" || userName == null || userName == "" || Location == null || Location == "")
            {
                return "数据有误，请重新操作！";
            }
            using (TransactionScope trans = new TransactionScope())
            {
                try
                {
                    #region 备货优化

                    if (!shipHelper.CheckLoadStatus(whCode, LoadId))
                    {
                        return "该Load:" + LoadId + "状态有误，请检查！";
                    }

                    if (!shipHelper.CheckOutLocation(whCode, Location))
                    {
                        return "错误！备货门区有误！";
                    }

                    if (!shipHelper.CheckHuStatusByPickTask(whCode, HuId, LoadId))
                    {
                        return "该Load:" + LoadId + "对应的备货托盘：" + HuId + "状态有误或不存在，请检查备货任务！";
                    }

                    if (!shipHelper.CheckHuStatus(whCode, HuId))
                    {
                        return "库存托盘:" + HuId + "状态有误，请检查！";
                    }
                    if (!shipHelper.CheckLocationStatusByHuId(whCode, HuId))
                    {
                        return "库存托盘:" + HuId + "对应的库位状态有误，请检查！";
                    }

                    string mess = CheckPickingLoad(LoadId, whCode, HuId);
                    //既不是拆托 也不是整出 表示有误
                    if (mess != "N" && mess != "Y")
                    {
                        return mess;
                    }
                    //拆托还要验证新托盘
                    if (mess == "N")
                    {
                        if (!shipHelper.CheckPlt(whCode, PutHuId))
                        {
                            return "错误！托盘" + PutHuId + "不存在或已使用！";
                        }
                    }

                    LoadMaster load = idal.ILoadMasterDAL.SelectBy(u => u.WhCode == whCode && u.LoadId == LoadId).First();
                    //-----------25 为已备货
                    FlowDetail flowDetail = idal.IFlowDetailDAL.SelectBy(u => u.FlowHeadId == load.ProcessId && u.Type == "Picking").First();

                    //修改Load状态
                    if (load.Status1 == "U")
                    {
                        load.Location = Location;
                        load.Status1 = "A";
                        load.UpdateUser = userName;
                        load.UpdateDate = DateTime.Now;
                        load.BeginPickDate = DateTime.Now;
                        idal.ILoadMasterDAL.UpdateBy(load, u => u.Id == load.Id, new string[] { "Status1", "UpdateUser", "UpdateDate", "BeginPickDate", "Location" });

                        //修改OutOrder状态
                        List<OutBoundOrderResult> list = (from a in idal.ILoadDetailDAL.SelectAll()
                                                          where a.LoadMasterId == load.Id
                                                          select new OutBoundOrderResult
                                                          {
                                                              Id = (Int32)a.OutBoundOrderId
                                                          }).ToList();
                        foreach (var item in list)
                        {
                            OutBoundOrder outBoundOrder = idal.IOutBoundOrderDAL.SelectBy(u => u.Id == item.Id).First();
                            if (outBoundOrder.StatusId > 15 && outBoundOrder.StatusId < 30) //订单状态必须为草稿以上
                            {
                                string remark1 = "原状态：" + outBoundOrder.StatusId + outBoundOrder.StatusName;

                                outBoundOrder.NowProcessId = flowDetail.FlowRuleId;
                                outBoundOrder.StatusId = flowDetail.StatusId;
                                outBoundOrder.StatusName = "正在备货";
                                outBoundOrder.UpdateUser = userName;
                                outBoundOrder.UpdateDate = DateTime.Now;
                                idal.IOutBoundOrderDAL.UpdateBy(outBoundOrder, u => u.Id == item.Id, new string[] { "NowProcessId", "StatusId", "StatusName", "UpdateUser", "UpdateDate" });

                                //更新订单状态，插入日志
                                TranLog tl = new TranLog();
                                tl.TranType = "32";
                                tl.Description = "更新订单状态";
                                tl.TranDate = DateTime.Now;
                                tl.TranUser = userName;
                                tl.WhCode = whCode;
                                tl.LoadId = LoadId;
                                tl.CustomerOutPoNumber = outBoundOrder.CustomerOutPoNumber;
                                tl.OutPoNumber = outBoundOrder.OutPoNumber;
                                tl.Remark = remark1 + "变更为：" + outBoundOrder.StatusId + outBoundOrder.StatusName;
                                idal.ITranLogDAL.Add(tl);
                            }

                        }
                    }

                    //1.修改库存托盘状态 及 实现托盘拆托
                    List<PickTaskDetail> pickList = idal.IPickTaskDetailDAL.SelectBy(u => u.WhCode == whCode && u.LoadId == LoadId && u.HuId == HuId && u.Status == "U");

                    List<WorkloadAccount> addWorkList = new List<WorkloadAccount>();
                    //如果是拆托
                    if (mess == "N")
                    {
                        List<HuDetail> huDetailList = idal.IHuDetailDAL.SelectBy(u => u.WhCode == whCode && u.HuId == HuId);
                        int count = 0;
                        foreach (var item in pickList)
                        {
                            HuDetail huDetail = huDetailList.Where(u => u.Id == item.HuDetailId).First();
                            if (count == 0)
                            {
                                HuMaster huMaster = new HuMaster();
                                huMaster.WhCode = whCode;
                                huMaster.HuId = PutHuId;
                                huMaster.LoadId = LoadId;
                                huMaster.Type = "M";
                                huMaster.Status = "A";
                                huMaster.Location = Location;
                                huMaster.ReceiptId = huDetail.ReceiptId;
                                huMaster.ReceiptDate = DateTime.Now;
                                huMaster.TransactionFlag = 1;
                                huMaster.CreateUser = userName;
                                huMaster.CreateDate = DateTime.Now;
                                idal.IHuMasterDAL.Add(huMaster);
                            }

                            HuDetail entity = new HuDetail();
                            entity.WhCode = whCode;
                            entity.HuId = PutHuId;
                            entity.ClientId = huDetail.ClientId;
                            entity.ClientCode = huDetail.ClientCode;
                            entity.SoNumber = huDetail.SoNumber;
                            entity.CustomerPoNumber = huDetail.CustomerPoNumber;
                            entity.AltItemNumber = huDetail.AltItemNumber;
                            entity.ItemId = huDetail.ItemId;
                            entity.UnitId = huDetail.UnitId;
                            entity.UnitName = huDetail.UnitName;
                            entity.ReceiptId = huDetail.ReceiptId;
                            entity.Qty = item.Qty;
                            entity.PlanQty = 0;
                            entity.ReceiptDate = huDetail.ReceiptDate;
                            entity.Length = huDetail.Length;
                            entity.Width = huDetail.Width;
                            entity.Height = huDetail.Height;
                            entity.Weight = huDetail.Weight;
                            entity.LotNumber1 = huDetail.LotNumber1;
                            entity.LotNumber2 = huDetail.LotNumber2;
                            entity.LotDate = huDetail.LotDate;
                            entity.CreateUser = userName;
                            entity.CreateDate = DateTime.Now;
                            idal.IHuDetailDAL.Add(entity);

                            //1.2 插入备货记录
                            AddPickingTranLog1(huDetail, userName, whCode, item.Location, "", LoadId, PutHuId, item.Qty);

                            HuDetail entity1 = new HuDetail();
                            entity1.Qty = huDetail.Qty - item.Qty;
                            entity1.PlanQty = huDetail.PlanQty - item.Qty;
                            entity1.UpdateUser = userName;
                            entity1.UpdateDate = DateTime.Now;
                            if (entity1.Qty == 0)
                            {
                                idal.IHuDetailDAL.DeleteBy(u => u.Id == huDetail.Id);

                            }
                            else
                            {
                                idal.IHuDetailDAL.UpdateBy(entity1, u => u.Id == huDetail.Id, new string[] { "Qty", "PlanQty", "UpdateUser", "UpdateDate" });
                            }
                            //1.2 插入备货记录 
                            AddPickingTranLog(entity, userName, whCode, item.Location, Location, LoadId, "");

                            //插入工人工作量
                            if (addWorkList.Where(u => u.WhCode == whCode && u.HuId == entity.HuId).Count() == 0)
                            {
                                WorkloadAccount work = new WorkloadAccount();
                                work.WhCode = whCode;
                                work.ClientId = entity.ClientId;
                                work.ClientCode = entity.ClientCode;
                                work.LoadId = LoadId;
                                work.HuId = entity.HuId;
                                work.WorkType = "叉车工";
                                work.UserCode = userName;
                                work.LotFlag = 0;
                                work.EchFlag = (entity.UnitName.Contains("ECH") ? 1 : 0);
                                work.Qty = (Int32)entity.Qty;
                                work.CBM = entity.Length * entity.Width * entity.Height * entity.Qty;
                                work.Weight = entity.Weight;
                                work.ReceiptDate = DateTime.Now;
                                addWorkList.Add(work);
                            }
                            else
                            {
                                WorkloadAccount getModel = addWorkList.Where(u => u.WhCode == whCode && u.HuId == entity.HuId).First();
                                addWorkList.Remove(getModel);

                                WorkloadAccount work = new WorkloadAccount();
                                work.WhCode = whCode;
                                work.ClientId = entity.ClientId;
                                work.ClientCode = entity.ClientCode;
                                work.LoadId = LoadId;
                                work.HuId = entity.HuId;
                                work.WorkType = "叉车工";
                                work.UserCode = userName;
                                work.LotFlag = 0;

                                if (getModel.EchFlag == 1)
                                {
                                    work.EchFlag = 1;
                                }
                                else
                                {
                                    work.EchFlag = (entity.UnitName.Contains("ECH") ? 1 : 0);
                                }

                                work.Qty = (Int32)entity.Qty + getModel.Qty;
                                work.CBM = (entity.Length * entity.Width * entity.Height * entity.Qty) + getModel.CBM;
                                work.Weight = entity.Weight + getModel.Weight;
                                work.ReceiptDate = DateTime.Now;
                                addWorkList.Add(work);
                            }

                            count++;
                        }

                        idal.IWorkloadAccountDAL.Add(addWorkList);

                        //1.3 修改备货任务托盘
                        PickTaskDetail pick = new PickTaskDetail();
                        pick.Status = "C";
                        pick.HuId = PutHuId;
                        pick.UpdateUser = userName;
                        pick.UpdateDate = DateTime.Now;
                        idal.IPickTaskDetailDAL.UpdateBy(pick, u => u.LoadId == LoadId && u.WhCode == whCode && u.HuId == HuId, new string[] { "Status", "HuId", "UpdateUser", "UpdateDate" });

                        //1.4 修改原托盘拆托Flag为1
                        HuMaster huma = new HuMaster();
                        huma.TransactionFlag = 1;
                        huma.UpdateUser = userName;
                        huma.UpdateDate = DateTime.Now;
                        idal.IHuMasterDAL.UpdateBy(huma, u => u.WhCode == whCode && u.HuId == HuId, new string[] { "TransactionFlag", "UpdateUser", "UpdateDate" });
                    }
                    else if (mess == "Y")   //整出
                    {
                        HuMaster huMaster = idal.IHuMasterDAL.SelectBy(u => u.WhCode == whCode && u.HuId == HuId).First();
                        //1.1 插入备货记录
                        List<HuDetail> huDetailList = idal.IHuDetailDAL.SelectBy(u => u.WhCode == whCode && u.HuId == HuId);
                        foreach (var item in huDetailList)
                        {
                            AddPickingTranLog(item, userName, whCode, huMaster.Location, Location, LoadId, "");

                            //插入工人工作量
                            if (addWorkList.Where(u => u.WhCode == whCode && u.HuId == item.HuId).Count() == 0)
                            {
                                WorkloadAccount work = new WorkloadAccount();
                                work.WhCode = whCode;
                                work.ClientId = item.ClientId;
                                work.ClientCode = item.ClientCode;
                                work.LoadId = LoadId;
                                work.HuId = item.HuId;
                                work.WorkType = "叉车工";
                                work.UserCode = userName;
                                work.LotFlag = 0;
                                work.EchFlag = (item.UnitName.Contains("ECH") ? 1 : 0);
                                work.Qty = (Int32)item.Qty;
                                work.CBM = item.Length * item.Width * item.Height * item.Qty;
                                work.Weight = item.Weight;
                                work.ReceiptDate = DateTime.Now;
                                addWorkList.Add(work);
                            }
                            else
                            {
                                WorkloadAccount getModel = addWorkList.Where(u => u.WhCode == whCode && u.HuId == item.HuId).First();
                                addWorkList.Remove(getModel);

                                WorkloadAccount work = new WorkloadAccount();
                                work.WhCode = whCode;
                                work.ClientId = item.ClientId;
                                work.ClientCode = item.ClientCode;
                                work.LoadId = LoadId;
                                work.HuId = item.HuId;
                                work.WorkType = "叉车工";
                                work.UserCode = userName;
                                work.LotFlag = 0;

                                if (getModel.EchFlag == 1)
                                {
                                    work.EchFlag = 1;
                                }
                                else
                                {
                                    work.EchFlag = (item.UnitName.Contains("ECH") ? 1 : 0);
                                }

                                work.Qty = (Int32)item.Qty + getModel.Qty;
                                work.CBM = (item.Length * item.Width * item.Height * item.Qty) + getModel.CBM;
                                work.Weight = item.Weight + getModel.Weight;
                                work.ReceiptDate = DateTime.Now;
                                addWorkList.Add(work);
                            }
                        }

                        idal.IWorkloadAccountDAL.Add(addWorkList);

                        //1.2 修改托盘信息
                        huMaster.LoadId = LoadId;
                        huMaster.Location = Location;
                        huMaster.UpdateUser = userName;
                        huMaster.UpdateDate = DateTime.Now;
                        idal.IHuMasterDAL.UpdateBy(huMaster, u => u.Id == huMaster.Id, new string[] { "LoadId", "Location", "UpdateUser", "UpdateDate" });

                        HuDetail huDetail = new HuDetail();
                        huDetail.PlanQty = 0;
                        huDetail.UpdateUser = userName;
                        huDetail.UpdateDate = DateTime.Now;
                        idal.IHuDetailDAL.UpdateBy(huDetail, u => u.WhCode == huMaster.WhCode && u.HuId == huMaster.HuId, new string[] { "PlanQty", "UpdateUser", "UpdateDate" });

                        //1.3 修改备货任务状态
                        PickTaskDetail pick = new PickTaskDetail();
                        pick.Status = "C";
                        pick.UpdateUser = userName;
                        pick.UpdateDate = DateTime.Now;
                        idal.IPickTaskDetailDAL.UpdateBy(pick, u => u.LoadId == LoadId && u.WhCode == whCode && u.HuId == HuId, new string[] { "Status", "UpdateUser", "UpdateDate" });
                    }

                    idal.IPickTaskDetailDAL.SaveChanges();

                    //检查是否全部完成备货
                    var sql4 = from a in idal.IPickTaskDetailDAL.SelectAll()
                               where a.LoadId == LoadId && a.WhCode == whCode && a.Status == "U" && a.HuId != HuId
                               select a;
                    if (sql4.Count() == 0)
                    {
                        load.Status1 = "C";
                        load.UpdateUser = userName;
                        load.UpdateDate = DateTime.Now;
                        load.EndPickDate = DateTime.Now;
                        idal.ILoadMasterDAL.UpdateBy(load, u => u.Id == load.Id, new string[] { "Status1", "UpdateUser", "UpdateDate", "EndPickDate" });

                        //修改OutOrder状态
                        List<OutBoundOrderResult> list = (from a in idal.ILoadDetailDAL.SelectAll()
                                                          where a.LoadMasterId == load.Id
                                                          select new OutBoundOrderResult
                                                          {
                                                              Id = (Int32)a.OutBoundOrderId
                                                          }).ToList();
                        foreach (var item in list)
                        {
                            OutBoundOrder outBoundOrder = idal.IOutBoundOrderDAL.SelectBy(u => u.Id == item.Id).First();
                            //只更新正在备货的订单 
                            if (outBoundOrder.StatusName == "正在备货" && outBoundOrder.StatusId >= 15 && outBoundOrder.StatusId < 30)
                            {
                                string remark1 = "原状态：" + outBoundOrder.StatusId + outBoundOrder.StatusName;

                                outBoundOrder.NowProcessId = flowDetail.FlowRuleId;
                                outBoundOrder.StatusId = flowDetail.StatusId;
                                outBoundOrder.StatusName = flowDetail.StatusName;
                                outBoundOrder.UpdateUser = userName;
                                outBoundOrder.UpdateDate = DateTime.Now;
                                idal.IOutBoundOrderDAL.UpdateBy(outBoundOrder, u => u.Id == item.Id, new string[] { "NowProcessId", "StatusId", "StatusName", "UpdateUser", "UpdateDate" });

                                //更新订单状态，插入日志
                                TranLog tl = new TranLog();
                                tl.TranType = "32";
                                tl.Description = "更新订单状态";
                                tl.TranDate = DateTime.Now;
                                tl.TranUser = userName;
                                tl.WhCode = whCode;
                                tl.LoadId = LoadId;
                                tl.CustomerOutPoNumber = outBoundOrder.CustomerOutPoNumber;
                                tl.OutPoNumber = outBoundOrder.OutPoNumber;
                                tl.Remark = remark1 + "变更为：" + outBoundOrder.StatusId + outBoundOrder.StatusName;
                                idal.ITranLogDAL.Add(tl);
                            }
                        }
                    }



                    //----------------begin 
                    //备货时 同时插入分拣任务表中的PickQty (备货数量)
                    //1.首先 根据备货任务表的款号等 查询出 分拣任务表的对应数据
                    List<SortTaskDetail> resultList = new List<SortTaskDetail>();   //最终结果

                    List<SortTaskDetail> checkResultList = new List<SortTaskDetail>();
                    foreach (var item in pickList)
                    {
                        List<SortTaskDetail> sortTaskDetailList = idal.ISortTaskDetailDAL.SelectBy(u => u.AltItemNumber == item.AltItemNumber && u.ItemId == item.ItemId && u.LoadId == item.LoadId && u.WhCode == item.WhCode && u.PlanQty != u.PickQty).OrderBy(u => u.GroupId).ToList();

                        int resultQty = item.Qty;
                        foreach (var detail in sortTaskDetailList)
                        {
                            if (checkResultList.Where(u => u.Id == detail.Id).Count() > 0)
                            {
                                detail.PickQty = checkResultList.Where(u => u.Id == detail.Id).First().PickQty;
                            }
                            if (detail.PickQty == detail.PlanQty)
                            {
                                continue;
                            }

                            if (checkResultList.Where(u => u.Id == detail.Id).Count() > 0)
                            {
                                SortTaskDetail old = checkResultList.Where(u => u.Id == detail.Id).First();
                                checkResultList.Remove(old);
                            }

                            if (detail.PickQty + resultQty > detail.PlanQty)
                            {
                                resultQty = resultQty - (detail.PlanQty - (Int32)detail.PickQty);

                                SortTaskDetail entity = new SortTaskDetail();
                                entity.Id = detail.Id;
                                entity.PickQty = detail.PlanQty;
                                checkResultList.Add(entity);

                                SortTaskDetail editentity = detail;
                                editentity.PickQty = entity.PickQty;

                                continue;
                            }
                            else
                            {
                                SortTaskDetail entity = new SortTaskDetail();
                                entity.Id = detail.Id;
                                entity.PickQty = detail.PickQty + resultQty;
                                checkResultList.Add(entity);

                                SortTaskDetail editentity = detail;
                                editentity.PickQty = entity.PickQty;

                                break;
                            }
                        }
                    }

                    //更新分拣表的备货数量
                    foreach (var item in checkResultList)
                    {
                        idal.ISortTaskDetailDAL.UpdateByExtended(u => u.Id == item.Id, t => new SortTaskDetail { PickQty = item.PickQty, UpdateUser = userName, UpdateDate = DateTime.Now });
                    }

                    //--------------- end

                    #endregion

                    idal.SaveChanges();
                    trans.Complete();
                    return "Y";
                }
                catch (Exception e)
                {
                    string s = e.InnerException.Message;
                    if (s.Length > 10)
                    {
                        s = s.Substring(0, 10);
                    }
                    trans.Dispose();//出现异常，事务手动释放
                    return "备货异常！" + s;
                }
            }
        }


        //ByLoad备货 不自动装箱，托盘备货后可重复使用
        public string PickingLoadByBegin(string LoadId, string whCode, string userName, string HuId, string PutHuId, string Location, string type)
        {
            if (LoadId == null || LoadId == "" || whCode == "" || whCode == null || HuId == null || HuId == "" || userName == null || userName == "" || Location == null || Location == "")
            {
                return "数据有误，请重新操作！";
            }
            using (TransactionScope trans = new TransactionScope())
            {
                try
                {
                    #region 备货优化

                    if (!shipHelper.CheckLoadStatus(whCode, LoadId))
                    {
                        return "该Load:" + LoadId + "状态有误，请检查！";
                    }

                    if (!shipHelper.CheckOutLocation(whCode, Location))
                    {
                        return "错误！备货门区有误！";
                    }

                    if (!shipHelper.CheckHuStatusByPickTask(whCode, HuId, LoadId))
                    {
                        return "该Load:" + LoadId + "对应的备货托盘：" + HuId + "状态有误或不存在，请检查备货任务！";
                    }

                    if (!shipHelper.CheckHuStatus(whCode, HuId))
                    {
                        return "库存托盘:" + HuId + "状态有误，请检查！";
                    }
                    if (!shipHelper.CheckLocationStatusByHuId(whCode, HuId))
                    {
                        return "库存托盘:" + HuId + "对应的库位状态有误，请检查！";
                    }

                    string mess = CheckPickingLoad(LoadId, whCode, HuId);
                    //既不是拆托 也不是整出 表示有误
                    if (mess != "N" && mess != "Y")
                    {
                        return mess;
                    }
                    if (mess == "Y")
                    {
                        if (!string.IsNullOrEmpty(PutHuId))
                        {
                            if (HuId != PutHuId)
                            {
                                return "备货托盘:" + HuId + "为整出请使用原托盘号！";
                            }
                        }
                        else
                        {
                            PutHuId = HuId;
                        }
                    }

                    LoadMaster load = idal.ILoadMasterDAL.SelectBy(u => u.WhCode == whCode && u.LoadId == LoadId).First();
                    //-----------25 为已备货
                    FlowDetail flowDetail = idal.IFlowDetailDAL.SelectBy(u => u.FlowHeadId == load.ProcessId && u.Type == "Picking").First();

                    //修改Load状态
                    if (load.Status1 == "U")
                    {
                        load.Location = Location;
                        load.Status1 = "A";
                        load.UpdateUser = userName;
                        load.UpdateDate = DateTime.Now;
                        load.BeginPickDate = DateTime.Now;
                        idal.ILoadMasterDAL.UpdateBy(load, u => u.Id == load.Id, new string[] { "Status1", "UpdateUser", "UpdateDate", "BeginPickDate", "Location" });

                        //修改OutOrder状态
                        List<OutBoundOrderResult> list = (from a in idal.ILoadDetailDAL.SelectAll()
                                                          where a.LoadMasterId == load.Id
                                                          select new OutBoundOrderResult
                                                          {
                                                              Id = (Int32)a.OutBoundOrderId
                                                          }).ToList();
                        foreach (var item in list)
                        {
                            OutBoundOrder outBoundOrder = idal.IOutBoundOrderDAL.SelectBy(u => u.Id == item.Id).First();
                            if (outBoundOrder.StatusId > 15 && outBoundOrder.StatusId < 30) //订单状态必须为草稿以上
                            {
                                string remark1 = "原状态：" + outBoundOrder.StatusId + outBoundOrder.StatusName;

                                outBoundOrder.NowProcessId = flowDetail.FlowRuleId;
                                outBoundOrder.StatusId = flowDetail.StatusId;
                                outBoundOrder.StatusName = "正在备货";
                                outBoundOrder.UpdateUser = userName;
                                outBoundOrder.UpdateDate = DateTime.Now;
                                idal.IOutBoundOrderDAL.UpdateBy(outBoundOrder, u => u.Id == item.Id, new string[] { "NowProcessId", "StatusId", "StatusName", "UpdateUser", "UpdateDate" });

                                //更新订单状态，插入日志
                                TranLog tl = new TranLog();
                                tl.TranType = "32";
                                tl.Description = "更新订单状态";
                                tl.TranDate = DateTime.Now;
                                tl.TranUser = userName;
                                tl.WhCode = whCode;
                                tl.LoadId = LoadId;
                                tl.CustomerOutPoNumber = outBoundOrder.CustomerOutPoNumber;
                                tl.OutPoNumber = outBoundOrder.OutPoNumber;
                                tl.Remark = remark1 + "变更为：" + outBoundOrder.StatusId + outBoundOrder.StatusName;
                                idal.ITranLogDAL.Add(tl);
                            }

                        }
                    }

                    //1.修改库存托盘状态 及 实现托盘拆托
                    List<PickTaskDetail> pickList = idal.IPickTaskDetailDAL.SelectBy(u => u.WhCode == whCode && u.LoadId == LoadId && u.HuId == HuId && u.Status == "U");

                    List<WorkloadAccount> addWorkList = new List<WorkloadAccount>();
                    //如果是拆托
                    if (mess == "N")
                    {
                        List<HuMaster> huMasterList = idal.IHuMasterDAL.SelectBy(u => u.WhCode == whCode && u.HuId == PutHuId);
                        if (huMasterList.Count == 0)
                        {
                            HuMaster huMaster = new HuMaster();
                            huMaster.WhCode = whCode;
                            huMaster.HuId = PutHuId;
                            huMaster.LoadId = LoadId;
                            huMaster.Type = "M";
                            huMaster.Status = "A";
                            huMaster.Location = Location;
                            huMaster.ReceiptId = "";
                            huMaster.ReceiptDate = DateTime.Now;
                            huMaster.TransactionFlag = 1;
                            huMaster.CreateUser = userName;
                            huMaster.CreateDate = DateTime.Now;
                            idal.IHuMasterDAL.Add(huMaster);
                        }

                        List<HuDetail> huDetailList = idal.IHuDetailDAL.SelectBy(u => u.WhCode == whCode && u.HuId == HuId);
                        int count = 0;
                        foreach (var item in pickList)
                        {
                            HuDetail huDetail = huDetailList.Where(u => u.Id == item.HuDetailId).First();

                            HuDetail entity = new HuDetail();
                            entity.WhCode = whCode;
                            entity.HuId = PutHuId;
                            entity.ClientId = huDetail.ClientId;
                            entity.ClientCode = huDetail.ClientCode;
                            entity.SoNumber = huDetail.SoNumber;
                            entity.CustomerPoNumber = huDetail.CustomerPoNumber;
                            entity.AltItemNumber = huDetail.AltItemNumber;
                            entity.ItemId = huDetail.ItemId;
                            entity.UnitId = huDetail.UnitId;
                            entity.UnitName = huDetail.UnitName;
                            entity.ReceiptId = huDetail.ReceiptId;
                            entity.Qty = item.Qty;
                            entity.PlanQty = 0;
                            entity.ReceiptDate = DateTime.Now;
                            entity.Length = huDetail.Length;
                            entity.Width = huDetail.Width;
                            entity.Height = huDetail.Height;
                            entity.Weight = huDetail.Weight;
                            entity.LotNumber1 = huDetail.LotNumber1;
                            entity.LotNumber2 = huDetail.LotNumber2;
                            entity.LotDate = huDetail.LotDate;
                            entity.CreateUser = userName;
                            entity.CreateDate = DateTime.Now;
                            idal.IHuDetailDAL.Add(entity);

                            //1.2 插入备货记录
                            AddPickingTranLog1(huDetail, userName, whCode, item.Location, "", LoadId, PutHuId, item.Qty);

                            HuDetail entity1 = new HuDetail();
                            entity1.Qty = huDetail.Qty - item.Qty;
                            entity1.PlanQty = huDetail.PlanQty - item.Qty;
                            entity1.UpdateUser = userName;
                            entity1.UpdateDate = DateTime.Now;
                            if (entity1.Qty == 0)
                            {
                                idal.IHuDetailDAL.DeleteBy(u => u.Id == huDetail.Id);

                            }
                            else
                            {
                                idal.IHuDetailDAL.UpdateBy(entity1, u => u.Id == huDetail.Id, new string[] { "Qty", "PlanQty", "UpdateUser", "UpdateDate" });
                            }
                            //1.2 插入备货记录 
                            AddPickingTranLog(entity, userName, whCode, item.Location, Location, LoadId, "");

                            //插入工人工作量
                            if (addWorkList.Where(u => u.WhCode == whCode && u.HuId == entity.HuId).Count() == 0)
                            {
                                WorkloadAccount work = new WorkloadAccount();
                                work.WhCode = whCode;
                                work.ClientId = entity.ClientId;
                                work.ClientCode = entity.ClientCode;
                                work.LoadId = LoadId;
                                work.HuId = entity.HuId;
                                work.WorkType = "叉车工";
                                work.UserCode = userName;
                                work.LotFlag = 0;
                                work.EchFlag = (entity.UnitName.Contains("ECH") ? 1 : 0);
                                work.Qty = (Int32)entity.Qty;
                                work.CBM = entity.Length * entity.Width * entity.Height * entity.Qty;
                                work.Weight = entity.Weight;
                                work.ReceiptDate = DateTime.Now;
                                addWorkList.Add(work);
                            }
                            else
                            {
                                WorkloadAccount getModel = addWorkList.Where(u => u.WhCode == whCode && u.HuId == entity.HuId).First();
                                addWorkList.Remove(getModel);

                                WorkloadAccount work = new WorkloadAccount();
                                work.WhCode = whCode;
                                work.ClientId = entity.ClientId;
                                work.ClientCode = entity.ClientCode;
                                work.LoadId = LoadId;
                                work.HuId = entity.HuId;
                                work.WorkType = "叉车工";
                                work.UserCode = userName;
                                work.LotFlag = 0;

                                if (getModel.EchFlag == 1)
                                {
                                    work.EchFlag = 1;
                                }
                                else
                                {
                                    work.EchFlag = (entity.UnitName.Contains("ECH") ? 1 : 0);
                                }

                                work.Qty = (Int32)entity.Qty + getModel.Qty;
                                work.CBM = (entity.Length * entity.Width * entity.Height * entity.Qty) + getModel.CBM;
                                work.Weight = entity.Weight + getModel.Weight;
                                work.ReceiptDate = DateTime.Now;
                                addWorkList.Add(work);
                            }

                            count++;
                        }

                        idal.IWorkloadAccountDAL.Add(addWorkList);

                        //1.3 修改备货任务托盘
                        PickTaskDetail pick = new PickTaskDetail();
                        pick.Status = "C";
                        pick.HuId = PutHuId;
                        pick.UpdateUser = userName;
                        pick.UpdateDate = DateTime.Now;
                        idal.IPickTaskDetailDAL.UpdateBy(pick, u => u.LoadId == LoadId && u.WhCode == whCode && u.HuId == HuId, new string[] { "Status", "HuId", "UpdateUser", "UpdateDate" });

                        //1.4 修改原托盘拆托Flag为1
                        HuMaster huma = new HuMaster();
                        huma.TransactionFlag = 1;
                        huma.UpdateUser = userName;
                        huma.UpdateDate = DateTime.Now;
                        idal.IHuMasterDAL.UpdateBy(huma, u => u.WhCode == whCode && u.HuId == HuId, new string[] { "TransactionFlag", "UpdateUser", "UpdateDate" });
                    }
                    else if (mess == "Y")   //整出
                    {
                        HuMaster huMaster = idal.IHuMasterDAL.SelectBy(u => u.WhCode == whCode && u.HuId == HuId).First();
                        //1.1 插入备货记录
                        List<HuDetail> huDetailList = idal.IHuDetailDAL.SelectBy(u => u.WhCode == whCode && u.HuId == HuId);
                        foreach (var item in huDetailList)
                        {
                            AddPickingTranLog(item, userName, whCode, huMaster.Location, Location, LoadId, "");

                            //插入工人工作量
                            if (addWorkList.Where(u => u.WhCode == whCode && u.HuId == item.HuId).Count() == 0)
                            {
                                WorkloadAccount work = new WorkloadAccount();
                                work.WhCode = whCode;
                                work.ClientId = item.ClientId;
                                work.ClientCode = item.ClientCode;
                                work.LoadId = LoadId;
                                work.HuId = item.HuId;
                                work.WorkType = "叉车工";
                                work.UserCode = userName;
                                work.LotFlag = 0;
                                work.EchFlag = (item.UnitName.Contains("ECH") ? 1 : 0);
                                work.Qty = (Int32)item.Qty;
                                work.CBM = item.Length * item.Width * item.Height * item.Qty;
                                work.Weight = item.Weight;
                                work.ReceiptDate = DateTime.Now;
                                addWorkList.Add(work);
                            }
                            else
                            {
                                WorkloadAccount getModel = addWorkList.Where(u => u.WhCode == whCode && u.HuId == item.HuId).First();
                                addWorkList.Remove(getModel);

                                WorkloadAccount work = new WorkloadAccount();
                                work.WhCode = whCode;
                                work.ClientId = item.ClientId;
                                work.ClientCode = item.ClientCode;
                                work.LoadId = LoadId;
                                work.HuId = item.HuId;
                                work.WorkType = "叉车工";
                                work.UserCode = userName;
                                work.LotFlag = 0;

                                if (getModel.EchFlag == 1)
                                {
                                    work.EchFlag = 1;
                                }
                                else
                                {
                                    work.EchFlag = (item.UnitName.Contains("ECH") ? 1 : 0);
                                }

                                work.Qty = (Int32)item.Qty + getModel.Qty;
                                work.CBM = (item.Length * item.Width * item.Height * item.Qty) + getModel.CBM;
                                work.Weight = item.Weight + getModel.Weight;
                                work.ReceiptDate = DateTime.Now;
                                addWorkList.Add(work);
                            }
                        }

                        idal.IWorkloadAccountDAL.Add(addWorkList);

                        //1.2 修改托盘信息
                        huMaster.LoadId = LoadId;
                        huMaster.Location = Location;
                        huMaster.UpdateUser = userName;
                        huMaster.UpdateDate = DateTime.Now;
                        idal.IHuMasterDAL.UpdateBy(huMaster, u => u.Id == huMaster.Id, new string[] { "LoadId", "Location", "UpdateUser", "UpdateDate" });

                        HuDetail huDetail = new HuDetail();
                        huDetail.PlanQty = 0;
                        huDetail.UpdateUser = userName;
                        huDetail.UpdateDate = DateTime.Now;
                        idal.IHuDetailDAL.UpdateBy(huDetail, u => u.WhCode == huMaster.WhCode && u.HuId == huMaster.HuId, new string[] { "PlanQty", "UpdateUser", "UpdateDate" });

                        //1.3 修改备货任务状态
                        PickTaskDetail pick = new PickTaskDetail();
                        pick.Status = "C";
                        pick.UpdateUser = userName;
                        pick.UpdateDate = DateTime.Now;
                        idal.IPickTaskDetailDAL.UpdateBy(pick, u => u.LoadId == LoadId && u.WhCode == whCode && u.HuId == HuId, new string[] { "Status", "UpdateUser", "UpdateDate" });
                    }

                    idal.IPickTaskDetailDAL.SaveChanges();

                    //检查是否全部完成备货
                    var sql4 = from a in idal.IPickTaskDetailDAL.SelectAll()
                               where a.LoadId == LoadId && a.WhCode == whCode && a.Status == "U" && a.HuId != HuId
                               select a;
                    if (sql4.Count() == 0)
                    {
                        load.Status1 = "C";
                        load.UpdateUser = userName;
                        load.UpdateDate = DateTime.Now;
                        load.EndPickDate = DateTime.Now;
                        idal.ILoadMasterDAL.UpdateBy(load, u => u.Id == load.Id, new string[] { "Status1", "UpdateUser", "UpdateDate", "EndPickDate" });

                        //修改OutOrder状态
                        List<OutBoundOrderResult> list = (from a in idal.ILoadDetailDAL.SelectAll()
                                                          where a.LoadMasterId == load.Id
                                                          select new OutBoundOrderResult
                                                          {
                                                              Id = (Int32)a.OutBoundOrderId
                                                          }).ToList();
                        foreach (var item in list)
                        {
                            OutBoundOrder outBoundOrder = idal.IOutBoundOrderDAL.SelectBy(u => u.Id == item.Id).First();
                            //只更新正在备货的订单 
                            if (outBoundOrder.StatusName == "正在备货" && outBoundOrder.StatusId >= 15 && outBoundOrder.StatusId < 30)
                            {
                                string remark1 = "原状态：" + outBoundOrder.StatusId + outBoundOrder.StatusName;

                                outBoundOrder.NowProcessId = flowDetail.FlowRuleId;
                                outBoundOrder.StatusId = flowDetail.StatusId;
                                outBoundOrder.StatusName = flowDetail.StatusName;
                                outBoundOrder.UpdateUser = userName;
                                outBoundOrder.UpdateDate = DateTime.Now;
                                idal.IOutBoundOrderDAL.UpdateBy(outBoundOrder, u => u.Id == item.Id, new string[] { "NowProcessId", "StatusId", "StatusName", "UpdateUser", "UpdateDate" });

                                //更新订单状态，插入日志
                                TranLog tl = new TranLog();
                                tl.TranType = "32";
                                tl.Description = "更新订单状态";
                                tl.TranDate = DateTime.Now;
                                tl.TranUser = userName;
                                tl.WhCode = whCode;
                                tl.LoadId = LoadId;
                                tl.CustomerOutPoNumber = outBoundOrder.CustomerOutPoNumber;
                                tl.OutPoNumber = outBoundOrder.OutPoNumber;
                                tl.Remark = remark1 + "变更为：" + outBoundOrder.StatusId + outBoundOrder.StatusName;
                                idal.ITranLogDAL.Add(tl);
                            }
                        }
                    }



                    //----------------begin 
                    //备货时 同时插入分拣任务表中的PickQty (备货数量)
                    //1.首先 根据备货任务表的款号等 查询出 分拣任务表的对应数据
                    List<SortTaskDetail> resultList = new List<SortTaskDetail>();   //最终结果

                    List<SortTaskDetail> checkResultList = new List<SortTaskDetail>();
                    foreach (var item in pickList)
                    {
                        List<SortTaskDetail> sortTaskDetailList = idal.ISortTaskDetailDAL.SelectBy(u => u.AltItemNumber == item.AltItemNumber && u.ItemId == item.ItemId && u.LoadId == item.LoadId && u.WhCode == item.WhCode && u.PlanQty != u.PickQty).OrderBy(u => u.GroupId).ToList();

                        int resultQty = item.Qty;
                        foreach (var detail in sortTaskDetailList)
                        {
                            if (checkResultList.Where(u => u.Id == detail.Id).Count() > 0)
                            {
                                detail.PickQty = checkResultList.Where(u => u.Id == detail.Id).First().PickQty;
                            }
                            if (detail.PickQty == detail.PlanQty)
                            {
                                continue;
                            }

                            if (checkResultList.Where(u => u.Id == detail.Id).Count() > 0)
                            {
                                SortTaskDetail old = checkResultList.Where(u => u.Id == detail.Id).First();
                                checkResultList.Remove(old);
                            }

                            if (detail.PickQty + resultQty > detail.PlanQty)
                            {
                                resultQty = resultQty - (detail.PlanQty - (Int32)detail.PickQty);

                                SortTaskDetail entity = new SortTaskDetail();
                                entity.Id = detail.Id;
                                entity.PickQty = detail.PlanQty;
                                checkResultList.Add(entity);

                                SortTaskDetail editentity = detail;
                                editentity.PickQty = entity.PickQty;

                                continue;
                            }
                            else
                            {
                                SortTaskDetail entity = new SortTaskDetail();
                                entity.Id = detail.Id;
                                entity.PickQty = detail.PickQty + resultQty;
                                checkResultList.Add(entity);

                                SortTaskDetail editentity = detail;
                                editentity.PickQty = entity.PickQty;

                                break;
                            }
                        }
                    }

                    //更新分拣表的备货数量
                    foreach (var item in checkResultList)
                    {
                        idal.ISortTaskDetailDAL.UpdateByExtended(u => u.Id == item.Id, t => new SortTaskDetail { PickQty = item.PickQty, UpdateUser = userName, UpdateDate = DateTime.Now });
                    }

                    //--------------- end

                    #endregion

                    idal.SaveChanges();
                    trans.Complete();
                    return "Y";
                }
                catch (Exception e)
                {
                    string s = e.InnerException.Message;
                    if (s.Length > 10)
                    {
                        s = s.Substring(0, 10);
                    }
                    trans.Dispose();//出现异常，事务手动释放
                    return "备货异常！" + s;
                }
            }
        }


        //ByLoad备货后 自动装箱
        public string PickingLoadDC(string LoadId, string whCode, string userName, string HuId, string PutHuId, string Location)
        {
            using (TransactionScope trans = new TransactionScope())
            {
                try
                {
                    #region-------开始备货

                    if (LoadId == null || LoadId == "" || whCode == "" || whCode == null || HuId == null || HuId == "" || userName == null || userName == "" || Location == null || Location == "")
                    {
                        return "数据有误，请重新操作！";
                    }

                    if (!shipHelper.CheckLoadStatus(whCode, LoadId))
                    {
                        return "该Load:" + LoadId + "状态有误，请检查！";
                    }

                    if (!shipHelper.CheckOutLocation(whCode, Location))
                    {
                        return "错误！备货门区有误！";
                    }

                    if (!shipHelper.CheckHuStatusByPickTask(whCode, HuId, LoadId))
                    {
                        return "该Load:" + LoadId + "对应的备货托盘：" + HuId + "状态有误或不存在，请检查备货任务！";
                    }

                    if (!shipHelper.CheckHuStatus(whCode, HuId))
                    {
                        return "库存托盘:" + HuId + "状态有误，请检查！";
                    }
                    if (!shipHelper.CheckLocationStatusByHuId(whCode, HuId))
                    {
                        return "库存托盘:" + HuId + "对应的库位状态有误，请检查！";
                    }

                    string mess = CheckPickingLoad(LoadId, whCode, HuId);
                    //既不是拆托 也不是整出 表示有误
                    if (mess != "N" && mess != "Y")
                    {
                        return mess;
                    }
                    //拆托还要验证新托盘
                    if (mess == "N")
                    {
                        if (!shipHelper.CheckPlt(whCode, PutHuId))
                        {
                            return "错误！托盘" + PutHuId + "不存在或已使用！";
                        }
                    }

                    LoadMaster load = idal.ILoadMasterDAL.SelectBy(u => u.WhCode == whCode && u.LoadId == LoadId).First();
                    //-----------25 为已备货
                    FlowDetail flowDetail = idal.IFlowDetailDAL.SelectBy(u => u.FlowHeadId == load.ProcessId && u.Type == "Picking").First();

                    //订单状态修改列表
                    List<OutBoundOrder> OutBoundOrderUpdateList = new List<OutBoundOrder>();

                    //修改Load状态
                    if (load.Status1 == "U")
                    {
                        load.Location = Location;
                        load.Status1 = "A";
                        load.UpdateUser = userName;
                        load.UpdateDate = DateTime.Now;
                        load.BeginPickDate = DateTime.Now;
                        idal.ILoadMasterDAL.UpdateBy(load, u => u.Id == load.Id, new string[] { "Status1", "UpdateUser", "UpdateDate", "BeginPickDate", "Location" });

                        //修改OutOrder状态
                        List<OutBoundOrderResult> list = (from a in idal.ILoadDetailDAL.SelectAll()
                                                          where a.LoadMasterId == load.Id
                                                          select new OutBoundOrderResult
                                                          {
                                                              Id = (Int32)a.OutBoundOrderId
                                                          }).ToList();

                        int[] OutBoundOrderId = (from a in list
                                                 select a.Id).ToList().Distinct().ToArray();

                        List<OutBoundOrder> OutBoundOrderList = idal.IOutBoundOrderDAL.SelectBy(u => OutBoundOrderId.Contains(u.Id));

                        foreach (var item in list)
                        {
                            OutBoundOrder outBoundOrder = OutBoundOrderList.Where(u => u.Id == item.Id).First();

                            if (outBoundOrder.StatusId > 15 && outBoundOrder.StatusId < 30) //订单状态必须为草稿以上
                            {
                                string remark1 = "原状态：" + outBoundOrder.StatusId + outBoundOrder.StatusName;

                                outBoundOrder.NowProcessId = flowDetail.FlowRuleId;
                                outBoundOrder.StatusId = flowDetail.StatusId;
                                outBoundOrder.StatusName = "正在备货";
                                outBoundOrder.UpdateUser = userName;
                                outBoundOrder.UpdateDate = DateTime.Now;

                                //取消单次更新，变更为批量Sql执行更新
                                OutBoundOrderUpdateList.Add(outBoundOrder);
                                //idal.IOutBoundOrderDAL.UpdateBy(outBoundOrder, u => u.Id == item.Id, new string[] { "NowProcessId", "StatusId", "StatusName", "UpdateUser", "UpdateDate" });

                                //更新订单状态，插入日志
                                TranLog tl = new TranLog();
                                tl.TranType = "32";
                                tl.Description = "备货更新订单状态";
                                tl.TranDate = DateTime.Now;
                                tl.TranUser = userName;
                                tl.WhCode = whCode;
                                tl.LoadId = LoadId;
                                tl.CustomerOutPoNumber = outBoundOrder.CustomerOutPoNumber;
                                tl.OutPoNumber = outBoundOrder.OutPoNumber;
                                tl.Remark = remark1 + "变更为：" + outBoundOrder.StatusId + outBoundOrder.StatusName;
                                idal.ITranLogDAL.Add(tl);
                            }

                        }
                    }

                    //修改出库订单状态为正在备货
                    foreach (var outBoundOrderUpdate in OutBoundOrderUpdateList)
                    {
                        idal.IOutBoundOrderDAL.UpdateByExtended(u => u.Id == outBoundOrderUpdate.Id, t => new OutBoundOrder { NowProcessId = outBoundOrderUpdate.NowProcessId, StatusId = outBoundOrderUpdate.StatusId, StatusName = outBoundOrderUpdate.StatusName, UpdateUser = userName, UpdateDate = DateTime.Now });
                    }

                    //1.修改库存托盘状态 及 实现托盘拆托
                    List<PickTaskDetail> pickList = idal.IPickTaskDetailDAL.SelectBy(u => u.WhCode == whCode && u.LoadId == LoadId && u.HuId == HuId && u.Status == "U");

                    List<WorkloadAccount> addWorkList = new List<WorkloadAccount>();
                    //如果是拆托
                    if (mess == "N")
                    {
                        List<HuDetail> huDetailList = idal.IHuDetailDAL.SelectBy(u => u.WhCode == whCode && u.HuId == HuId);
                        int count = 0;
                        foreach (var item in pickList)
                        {
                            HuDetail huDetail = huDetailList.Where(u => u.Id == item.HuDetailId).First();
                            if (count == 0)
                            {
                                HuMaster huMaster = new HuMaster();
                                huMaster.WhCode = whCode;
                                huMaster.HuId = PutHuId;
                                huMaster.LoadId = LoadId;
                                huMaster.Type = "M";
                                huMaster.Status = "A";
                                huMaster.Location = Location;
                                huMaster.ReceiptId = huDetail.ReceiptId;
                                huMaster.ReceiptDate = DateTime.Now;
                                huMaster.TransactionFlag = 1;
                                huMaster.CreateUser = userName;
                                huMaster.CreateDate = DateTime.Now;
                                idal.IHuMasterDAL.Add(huMaster);
                            }

                            HuDetail entity = new HuDetail();
                            entity.WhCode = whCode;
                            entity.HuId = PutHuId;
                            entity.ClientId = huDetail.ClientId;
                            entity.ClientCode = huDetail.ClientCode;
                            entity.SoNumber = huDetail.SoNumber;
                            entity.CustomerPoNumber = huDetail.CustomerPoNumber;
                            entity.AltItemNumber = huDetail.AltItemNumber;
                            entity.ItemId = huDetail.ItemId;
                            entity.UnitId = huDetail.UnitId;
                            entity.UnitName = huDetail.UnitName;
                            entity.ReceiptId = huDetail.ReceiptId;
                            entity.Qty = item.Qty;
                            entity.PlanQty = 0;
                            entity.ReceiptDate = DateTime.Now;
                            entity.Length = huDetail.Length;
                            entity.Width = huDetail.Width;
                            entity.Height = huDetail.Height;
                            entity.Weight = huDetail.Weight;
                            entity.LotNumber1 = huDetail.LotNumber1;
                            entity.LotNumber2 = huDetail.LotNumber2;
                            entity.LotDate = huDetail.LotDate;
                            entity.CreateUser = userName;
                            entity.CreateDate = DateTime.Now;
                            idal.IHuDetailDAL.Add(entity);

                            //1.2 插入备货记录
                            AddPickingTranLog1(huDetail, userName, whCode, item.Location, "", LoadId, PutHuId, item.Qty);

                            HuDetail entity1 = new HuDetail();
                            entity1.Qty = huDetail.Qty - item.Qty;
                            entity1.PlanQty = huDetail.PlanQty - item.Qty;
                            entity1.UpdateUser = userName;
                            entity1.UpdateDate = DateTime.Now;
                            if (entity1.Qty == 0)
                            {
                                idal.IHuDetailDAL.DeleteBy(u => u.Id == huDetail.Id);

                                //1.3 插入备货删除记录
                                AddPickingTranLog2(huDetail, userName, whCode, item.Location, "", LoadId, PutHuId, item.Qty);
                            }
                            else
                            {
                                idal.IHuDetailDAL.UpdateBy(entity1, u => u.Id == huDetail.Id, new string[] { "Qty", "PlanQty", "UpdateUser", "UpdateDate" });
                            }
                            //1.2 插入备货记录 
                            AddPickingTranLog(entity, userName, whCode, item.Location, Location, LoadId, "");

                            //插入工人工作量
                            if (addWorkList.Where(u => u.WhCode == whCode && u.HuId == entity.HuId).Count() == 0)
                            {
                                WorkloadAccount work = new WorkloadAccount();
                                work.WhCode = whCode;
                                work.ClientId = entity.ClientId;
                                work.ClientCode = entity.ClientCode;
                                work.LoadId = LoadId;
                                work.HuId = entity.HuId;
                                work.WorkType = "叉车工";
                                work.UserCode = userName;
                                work.LotFlag = 0;
                                work.EchFlag = (entity.UnitName.Contains("ECH") ? 1 : 0);
                                work.Qty = (Int32)entity.Qty;
                                work.CBM = entity.Length * entity.Width * entity.Height * entity.Qty;
                                work.Weight = entity.Weight;
                                work.ReceiptDate = DateTime.Now;
                                addWorkList.Add(work);
                            }
                            else
                            {
                                WorkloadAccount getModel = addWorkList.Where(u => u.WhCode == whCode && u.HuId == entity.HuId).First();
                                addWorkList.Remove(getModel);

                                WorkloadAccount work = new WorkloadAccount();
                                work.WhCode = whCode;
                                work.ClientId = entity.ClientId;
                                work.ClientCode = entity.ClientCode;
                                work.LoadId = LoadId;
                                work.HuId = entity.HuId;
                                work.WorkType = "叉车工";
                                work.UserCode = userName;
                                work.LotFlag = 0;

                                if (getModel.EchFlag == 1)
                                {
                                    work.EchFlag = 1;
                                }
                                else
                                {
                                    work.EchFlag = (entity.UnitName.Contains("ECH") ? 1 : 0);
                                }

                                work.Qty = (Int32)entity.Qty + getModel.Qty;
                                work.CBM = (entity.Length * entity.Width * entity.Height * entity.Qty) + getModel.CBM;
                                work.Weight = entity.Weight + getModel.Weight;
                                work.ReceiptDate = DateTime.Now;
                                addWorkList.Add(work);
                            }

                            count++;
                        }

                        idal.IWorkloadAccountDAL.Add(addWorkList);

                        //1.3 修改备货任务托盘
                        PickTaskDetail pick = new PickTaskDetail();
                        pick.Status = "C";
                        pick.HuId = PutHuId;
                        pick.UpdateUser = userName;
                        pick.UpdateDate = DateTime.Now;
                        idal.IPickTaskDetailDAL.UpdateBy(pick, u => u.LoadId == LoadId && u.WhCode == whCode && u.HuId == HuId, new string[] { "Status", "HuId", "UpdateUser", "UpdateDate" });

                        //1.4 修改原托盘拆托Flag为1
                        HuMaster huma = new HuMaster();
                        huma.TransactionFlag = 1;
                        huma.UpdateUser = userName;
                        huma.UpdateDate = DateTime.Now;
                        idal.IHuMasterDAL.UpdateBy(huma, u => u.WhCode == whCode && u.HuId == HuId, new string[] { "TransactionFlag", "UpdateUser", "UpdateDate" });
                    }
                    else if (mess == "Y")   //整出
                    {
                        HuMaster huMaster = idal.IHuMasterDAL.SelectBy(u => u.WhCode == whCode && u.HuId == HuId).First();
                        //1.1 插入备货记录
                        List<HuDetail> huDetailList = idal.IHuDetailDAL.SelectBy(u => u.WhCode == whCode && u.HuId == HuId);
                        foreach (var item in huDetailList)
                        {
                            AddPickingTranLog(item, userName, whCode, huMaster.Location, Location, LoadId, "");

                            //插入工人工作量
                            if (addWorkList.Where(u => u.WhCode == whCode && u.HuId == item.HuId).Count() == 0)
                            {
                                WorkloadAccount work = new WorkloadAccount();
                                work.WhCode = whCode;
                                work.ClientId = item.ClientId;
                                work.ClientCode = item.ClientCode;
                                work.LoadId = LoadId;
                                work.HuId = item.HuId;
                                work.WorkType = "叉车工";
                                work.UserCode = userName;
                                work.LotFlag = 0;
                                work.EchFlag = (item.UnitName.Contains("ECH") ? 1 : 0);
                                work.Qty = (Int32)item.Qty;
                                work.CBM = item.Length * item.Width * item.Height * item.Qty;
                                work.Weight = item.Weight;
                                work.ReceiptDate = DateTime.Now;
                                addWorkList.Add(work);
                            }
                            else
                            {
                                WorkloadAccount getModel = addWorkList.Where(u => u.WhCode == whCode && u.HuId == item.HuId).First();
                                addWorkList.Remove(getModel);

                                WorkloadAccount work = new WorkloadAccount();
                                work.WhCode = whCode;
                                work.ClientId = item.ClientId;
                                work.ClientCode = item.ClientCode;
                                work.LoadId = LoadId;
                                work.HuId = item.HuId;
                                work.WorkType = "叉车工";
                                work.UserCode = userName;
                                work.LotFlag = 0;

                                if (getModel.EchFlag == 1)
                                {
                                    work.EchFlag = 1;
                                }
                                else
                                {
                                    work.EchFlag = (item.UnitName.Contains("ECH") ? 1 : 0);
                                }

                                work.Qty = (Int32)item.Qty + getModel.Qty;
                                work.CBM = (item.Length * item.Width * item.Height * item.Qty) + getModel.CBM;
                                work.Weight = item.Weight + getModel.Weight;
                                work.ReceiptDate = DateTime.Now;
                                addWorkList.Add(work);
                            }
                        }

                        idal.IWorkloadAccountDAL.Add(addWorkList);

                        //1.2 修改托盘信息
                        huMaster.LoadId = LoadId;
                        huMaster.Location = Location;
                        huMaster.UpdateUser = userName;
                        huMaster.UpdateDate = DateTime.Now;
                        idal.IHuMasterDAL.UpdateBy(huMaster, u => u.Id == huMaster.Id, new string[] { "LoadId", "Location", "UpdateUser", "UpdateDate" });

                        HuDetail huDetail = new HuDetail();
                        huDetail.PlanQty = 0;
                        huDetail.UpdateUser = userName;
                        huDetail.UpdateDate = DateTime.Now;
                        idal.IHuDetailDAL.UpdateBy(huDetail, u => u.WhCode == huMaster.WhCode && u.HuId == huMaster.HuId, new string[] { "PlanQty", "UpdateUser", "UpdateDate" });

                        //1.3 修改备货任务状态
                        PickTaskDetail pick = new PickTaskDetail();
                        pick.Status = "C";
                        pick.UpdateUser = userName;
                        pick.UpdateDate = DateTime.Now;
                        idal.IPickTaskDetailDAL.UpdateBy(pick, u => u.LoadId == LoadId && u.WhCode == whCode && u.HuId == HuId, new string[] { "Status", "UpdateUser", "UpdateDate" });
                    }

                    idal.SaveChanges();

                    //检查是否全部完成备货
                    var sql4 = from a in idal.IPickTaskDetailDAL.SelectAll()
                               where a.LoadId == LoadId && a.WhCode == whCode && a.Status == "U" && a.HuId != HuId
                               select a;
                    if (sql4.Count() == 0)
                    {
                        load.Status1 = "C";
                        load.UpdateUser = userName;
                        load.UpdateDate = DateTime.Now;
                        load.EndPickDate = DateTime.Now;
                        idal.ILoadMasterDAL.UpdateBy(load, u => u.Id == load.Id, new string[] { "Status1", "UpdateUser", "UpdateDate", "EndPickDate" });

                        //修改OutOrder状态
                        List<OutBoundOrderResult> list = (from a in idal.ILoadDetailDAL.SelectAll()
                                                          where a.LoadMasterId == load.Id
                                                          select new OutBoundOrderResult
                                                          {
                                                              Id = (Int32)a.OutBoundOrderId
                                                          }).ToList();

                        int[] OutBoundOrderId = (from a in list
                                                 select a.Id).ToList().Distinct().ToArray();

                        List<OutBoundOrder> OutBoundOrderList = idal.IOutBoundOrderDAL.SelectBy(u => OutBoundOrderId.Contains(u.Id));

                        foreach (var item in list)
                        {
                            OutBoundOrder outBoundOrder = OutBoundOrderList.Where(u => u.Id == item.Id).First();

                            if (outBoundOrder.StatusName == "正在备货" && outBoundOrder.StatusId >= 15 && outBoundOrder.StatusId < 30)
                            {
                                string remark1 = "原状态：" + outBoundOrder.StatusId + outBoundOrder.StatusName;

                                outBoundOrder.StatusName = flowDetail.StatusName;
                                outBoundOrder.UpdateUser = userName;
                                outBoundOrder.UpdateDate = DateTime.Now;

                                idal.IOutBoundOrderDAL.UpdateByExtended(u => u.Id == outBoundOrder.Id, t => new OutBoundOrder { StatusName = flowDetail.StatusName, UpdateUser = userName, UpdateDate = DateTime.Now });

                                //更新订单状态，插入日志
                                TranLog tl = new TranLog();
                                tl.TranType = "32";
                                tl.Description = "备货更新订单状态";
                                tl.TranDate = DateTime.Now;
                                tl.TranUser = userName;
                                tl.WhCode = whCode;
                                tl.LoadId = LoadId;
                                tl.CustomerOutPoNumber = outBoundOrder.CustomerOutPoNumber;
                                tl.OutPoNumber = outBoundOrder.OutPoNumber;
                                tl.Remark = remark1 + "变更为：" + outBoundOrder.StatusId + outBoundOrder.StatusName;
                                idal.ITranLogDAL.Add(tl);
                            }
                        }
                    }

                    //----------------begin 
                    //备货时 同时插入分拣任务表中的PickQty (备货数量)
                    //1.首先 根据备货任务表的款号等 查询出 分拣任务表的对应数据
                    List<SortTaskDetail> resultList = new List<SortTaskDetail>();   //最终结果

                    List<SortTaskDetail> checkResultList = new List<SortTaskDetail>();
                    foreach (var item in pickList)
                    {
                        List<SortTaskDetail> sortTaskDetailList = idal.ISortTaskDetailDAL.SelectBy(u => u.AltItemNumber == item.AltItemNumber && u.ItemId == item.ItemId && u.LoadId == item.LoadId && u.WhCode == item.WhCode && u.PlanQty != u.PickQty).OrderBy(u => u.GroupId).ToList();

                        int resultQty = item.Qty;
                        foreach (var detail in sortTaskDetailList)
                        {
                            if (checkResultList.Where(u => u.Id == detail.Id).Count() > 0)
                            {
                                detail.PickQty = checkResultList.Where(u => u.Id == detail.Id).First().PickQty;
                            }
                            if (detail.PickQty == detail.PlanQty)
                            {
                                continue;
                            }

                            if (checkResultList.Where(u => u.Id == detail.Id).Count() > 0)
                            {
                                SortTaskDetail old = checkResultList.Where(u => u.Id == detail.Id).First();
                                checkResultList.Remove(old);
                            }

                            if (detail.PickQty + resultQty > detail.PlanQty)
                            {
                                resultQty = resultQty - (detail.PlanQty - (Int32)detail.PickQty);

                                SortTaskDetail entity = new SortTaskDetail();
                                entity.Id = detail.Id;
                                entity.PickQty = detail.PlanQty;
                                checkResultList.Add(entity);

                                SortTaskDetail editentity = detail;
                                editentity.PickQty = entity.PickQty;

                                continue;
                            }
                            else
                            {
                                SortTaskDetail entity = new SortTaskDetail();
                                entity.Id = detail.Id;
                                entity.PickQty = detail.PickQty + resultQty;
                                checkResultList.Add(entity);

                                SortTaskDetail editentity = detail;
                                editentity.PickQty = entity.PickQty;

                                break;
                            }
                        }
                    }

                    //更新分拣表的备货数量
                    foreach (var item in checkResultList)
                    {
                        idal.ISortTaskDetailDAL.UpdateByExtended(u => u.Id == item.Id, t => new SortTaskDetail { PickQty = item.PickQty, UpdateUser = userName, UpdateDate = DateTime.Now });
                    }

                    #endregion

                    idal.IPickTaskDetailDAL.SaveChanges();

                    string huId = "";

                    if (mess == "N")
                    {
                        huId = PutHuId;
                    }
                    else if (mess == "Y")
                    {
                        huId = HuId;
                    }

                    #region--------开始装箱

                    if (huId != "")
                    {
                        List<HuMaster> huMasterList = (from a in idal.IHuMasterDAL.SelectAll()
                                                       where (a.HuId == LoadId || a.HuId == huId) && a.WhCode == whCode
                                                       select a).ToList();
                        if (huMasterList.Where(u => u.HuId == LoadId && u.WhCode == whCode).Count() == 0)
                        {
                            HuMaster huMaster = new HuMaster();
                            string loc = huMasterList.Where(u => u.HuId == huId && u.WhCode == whCode).First().Location;
                            huMaster.WhCode = whCode;
                            huMaster.HuId = LoadId;
                            huMaster.Type = "O";
                            huMaster.Status = "A";
                            huMaster.Location = loc;
                            huMaster.ReceiptDate = DateTime.Now;
                            huMaster.LoadId = LoadId;
                            huMaster.CreateUser = userName;
                            huMaster.CreateDate = DateTime.Now;
                            idal.IHuMasterDAL.Add(huMaster);
                        }

                        List<HuDetail> List = idal.IHuDetailDAL.SelectBy(u => u.WhCode == whCode && (u.HuId == huId || u.HuId == LoadId));

                        List<HuDetail> huDetailList = List.Where(u => u.HuId == huId && u.WhCode == whCode).ToList();
                        if (huDetailList.Count == 0)
                        {
                            //插入装箱记录
                            TranLog tl = new TranLog();
                            tl.TranType = "211";
                            tl.Description = "自动装箱异常";
                            tl.TranDate = DateTime.Now;
                            tl.TranUser = userName;
                            tl.WhCode = whCode;
                            tl.HuId = huId;
                            tl.Remark = "装箱未找到库存托盘";
                            tl.LoadId = LoadId;
                            idal.ITranLogDAL.Add(tl);
                            idal.SaveChanges();

                            trans.Dispose();//出现异常，事务手动释放
                            return "备货异常，请重新提交！";
                        }

                        List<FlowDetail> getFlowDetailList = (from a in idal.ILoadMasterDAL.SelectAll()
                                                              join b in idal.IFlowDetailDAL.SelectAll()
                                                              on a.ProcessId equals b.FlowHeadId
                                                              where a.LoadId == LoadId && a.WhCode == whCode && b.Type == "Release"
                                                              select b).ToList();
                        FlowDetail getMark = new FlowDetail();
                        if (getFlowDetailList.Count > 0)
                        {
                            getMark = getFlowDetailList.First();
                        }

                        List<HuDetail> loadHuList = List.Where(u => u.HuId == LoadId && u.WhCode == whCode).ToList();
                        foreach (var item in huDetailList)
                        {
                            //插入装箱记录
                            TranLog tl = new TranLog();
                            tl.TranType = "210";
                            tl.Description = "装箱";
                            tl.TranDate = DateTime.Now;
                            tl.TranUser = userName;
                            tl.WhCode = item.WhCode;
                            tl.ClientCode = item.ClientCode;
                            tl.SoNumber = item.SoNumber;
                            tl.CustomerPoNumber = item.CustomerPoNumber;
                            tl.AltItemNumber = item.AltItemNumber;
                            tl.ItemId = item.ItemId;
                            tl.UnitID = item.UnitId;
                            tl.UnitName = item.UnitName;
                            tl.TranQty = item.Qty;
                            tl.TranQty2 = item.PlanQty;
                            tl.HuId = item.HuId;
                            tl.Length = item.Length;
                            tl.Width = item.Width;
                            tl.Height = item.Height;
                            tl.Weight = item.Weight;
                            tl.LotNumber1 = item.LotNumber1;
                            tl.LotNumber2 = item.LotNumber2;
                            tl.LotDate = item.LotDate;
                            tl.ReceiptId = item.ReceiptId;
                            tl.ReceiptDate = item.ReceiptDate;
                            tl.Location = huMasterList.Where(u => (u.HuId == LoadId || u.HuId == huId) && u.WhCode == whCode).First().Location;
                            tl.LoadId = LoadId;
                            idal.ITranLogDAL.Add(tl);

                            if (loadHuList.Count == 0)
                            {
                                if (getMark.Mark == "1" || getMark.Mark == "2" || getMark.Mark == "9")
                                {
                                    //添加库存新明细
                                    addHudetail(LoadId, userName, item);
                                }
                                else
                                {
                                    addHudetail1(LoadId, userName, item);
                                }
                            }
                            else
                            {
                                //如果是CFS，需要保留SOPO等信息
                                if (getMark.Mark == "1" || getMark.Mark == "2" || getMark.Mark == "9")
                                {
                                    if (loadHuList.Where(u => u.HuId == LoadId && u.WhCode == item.WhCode && u.SoNumber == item.SoNumber && u.CustomerPoNumber == item.CustomerPoNumber && u.AltItemNumber == item.AltItemNumber && u.ItemId == item.ItemId && u.UnitId == item.UnitId && u.UnitName == item.UnitName
                                                            &&
                                                               (u.Length == item.Length || (u.Length == null ? 0 : u.Length) == (item.Length == null ? 0 : item.Length)) &&
                                                               (u.Width == item.Width || (u.Width == null ? 0 : u.Width) == (item.Width == null ? 0 : item.Width)) &&
                                                               (u.Height == item.Height || (u.Height == null ? 0 : u.Height) == (item.Height == null ? 0 : item.Height)) &&
                                                               (u.Weight == item.Weight || (u.Weight == null ? 0 : u.Weight) == (item.Weight == null ? 0 : item.Weight)) &&
                                                              (u.LotNumber1 == item.LotNumber1 || (u.LotNumber1 == null ? "" : u.LotNumber1) == (item.LotNumber1 == null ? "" : item.LotNumber1))
                                                               &&
                                                              (u.LotNumber2 == item.LotNumber2 || (u.LotNumber2 == null ? "" : u.LotNumber2) == (item.LotNumber2 == null ? "" : item.LotNumber2)) &&
                                                               u.LotDate == item.LotDate
                                                           ).Count() == 0)
                                    {
                                        //添加库存新明细
                                        addHudetail(LoadId, userName, item);
                                    }
                                    else
                                    {
                                        HuDetail entity = loadHuList.Where(u => u.HuId == LoadId && u.WhCode == item.WhCode && u.SoNumber == item.SoNumber && u.CustomerPoNumber == item.CustomerPoNumber && u.AltItemNumber == item.AltItemNumber && u.ItemId == item.ItemId && u.UnitId == item.UnitId && u.UnitName == item.UnitName
                                         &&
                                        (u.Length == item.Length || (u.Length == null ? 0 : u.Length) == (item.Length == null ? 0 : item.Length)) &&
                                        (u.Width == item.Width || (u.Width == null ? 0 : u.Width) == (item.Width == null ? 0 : item.Width)) &&
                                        (u.Height == item.Height || (u.Height == null ? 0 : u.Height) == (item.Height == null ? 0 : item.Height)) &&
                                        (u.Weight == item.Weight || (u.Weight == null ? 0 : u.Weight) == (item.Weight == null ? 0 : item.Weight)) &&
                                      (u.LotNumber1 == item.LotNumber1 || (u.LotNumber1 == null ? "" : u.LotNumber1) == (item.LotNumber1 == null ? "" : item.LotNumber1))
                                        &&
                                       (u.LotNumber2 == item.LotNumber2 || (u.LotNumber2 == null ? "" : u.LotNumber2) == (item.LotNumber2 == null ? "" : item.LotNumber2)) &&
                                        u.LotDate == item.LotDate
                                        ).First();
                                        entity.Qty = entity.Qty + item.Qty;
                                        idal.IHuDetailDAL.UpdateBy(entity, u => u.Id == entity.Id, new string[] { "Qty" });
                                    }
                                }
                                else
                                {
                                    //电商则不需要SOPO信息，根据款号聚合装箱
                                    if (loadHuList.Where(u => u.HuId == LoadId && u.WhCode == item.WhCode && u.AltItemNumber == item.AltItemNumber && u.ItemId == item.ItemId && u.UnitId == item.UnitId && u.UnitName == item.UnitName && (u.LotNumber1 == item.LotNumber1 || (u.LotNumber1 == null ? "" : u.LotNumber1) == (item.LotNumber1 == null ? "" : item.LotNumber1)) && (u.LotNumber2 == item.LotNumber2 || (u.LotNumber2 == null ? "" : u.LotNumber2) == (item.LotNumber2 == null ? "" : item.LotNumber2))).Count() == 0)
                                    {
                                        //添加库存新明细
                                        addHudetail1(LoadId, userName, item);
                                    }
                                    else
                                    {
                                        HuDetail entity = loadHuList.Where(u => u.HuId == LoadId && u.WhCode == item.WhCode && u.AltItemNumber == item.AltItemNumber && u.ItemId == item.ItemId && u.UnitId == item.UnitId && u.UnitName == item.UnitName
                                         && (u.LotNumber1 == item.LotNumber1 || (u.LotNumber1 == null ? "" : u.LotNumber1) == (item.LotNumber1 == null ? "" : item.LotNumber1)) && (u.LotNumber2 == item.LotNumber2 || (u.LotNumber2 == null ? "" : u.LotNumber2) == (item.LotNumber2 == null ? "" : item.LotNumber2))).First();
                                        entity.Qty = entity.Qty + item.Qty;
                                        idal.IHuDetailDAL.UpdateBy(entity, u => u.Id == entity.Id, new string[] { "Qty" });
                                    }
                                }
                            }
                            idal.IHuDetailDAL.DeleteBy(u => u.Id == item.Id);
                        }
                        idal.IHuMasterDAL.DeleteBy(u => u.WhCode == whCode && u.HuId == huId);


                        //1.修改Load的 装箱状态及 装箱开始时间
                        if (load.Status3 == "U")
                        {
                            load.Status3 = "A";
                            load.BeginPackDate = DateTime.Now;
                            load.UpdateUser = userName;
                            load.UpdateDate = DateTime.Now;
                            idal.ILoadMasterDAL.UpdateBy(load, u => u.Id == load.Id, new string[] { "Status3", "BeginPackDate", "UpdateUser", "UpdateDate" });
                        }

                        //2.修改备货任务的 装箱状态
                        PickTaskDetail editStatus1 = new PickTaskDetail();
                        editStatus1.Status1 = "C";
                        editStatus1.UpdateUser = userName;
                        editStatus1.UpdateDate = DateTime.Now;
                        idal.IPickTaskDetailDAL.UpdateBy(editStatus1, u => u.WhCode == whCode && u.LoadId == LoadId && u.HuId == huId, new string[] { "Status1", "UpdateUser", "UpdateDate" });

                        idal.IPickTaskDetailDAL.SaveChanges();

                        //3.修改Load的 装箱结束时间
                        List<PickTaskDetail> sql1 = (from a in idal.IPickTaskDetailDAL.SelectAll()
                                                     where a.WhCode == whCode && a.LoadId == LoadId && a.Status1 == "U"
                                                     select a).ToList();
                        if (sql1.Count == 0)
                        {
                            load.Status3 = "C";
                            load.EndPackDate = DateTime.Now;
                            load.UpdateUser = userName;
                            load.UpdateDate = DateTime.Now;
                            idal.ILoadMasterDAL.UpdateBy(load, u => u.Id == load.Id, new string[] { "Status3", "EndPackDate", "UpdateUser", "UpdateDate" });
                        }
                    }

                    #endregion

                    idal.ILoadMasterDAL.SaveChanges();
                    trans.Complete();
                    return "Y";
                }
                catch
                {
                    trans.Dispose();//出现异常，事务手动释放
                    return "数据异常,自动备货装箱失败!";
                }
            }

        }


        //ByLoad备货后 自动装箱自动分拣
        public string PickingSortingByLoadBegin(string LoadId, string whCode, string userName, string HuId, string PutHuId, string Location)
        {
            if (LoadId == null || LoadId == "" || whCode == "" || whCode == null || HuId == null || HuId == "" || userName == null || userName == "" || Location == null || Location == "")
            {
                return "数据有误，请重新操作！";
            }

            if (!shipHelper.CheckLoadStatus(whCode, LoadId))
            {
                return "该Load:" + LoadId + "状态有误，请检查！";
            }

            if (!shipHelper.CheckOutLocation(whCode, Location))
            {
                return "错误！备货门区有误！";
            }

            if (!shipHelper.CheckHuStatusByPickTask(whCode, HuId, LoadId))
            {
                return "该Load:" + LoadId + "对应的备货托盘：" + HuId + "状态有误或不存在，请检查备货任务！";
            }

            if (!shipHelper.CheckHuStatus(whCode, HuId))
            {
                return "库存托盘:" + HuId + "状态有误，请检查！";
            }
            if (!shipHelper.CheckLocationStatusByHuId(whCode, HuId))
            {
                return "库存托盘:" + HuId + "对应的库位状态有误，请检查！";
            }

            string mess = CheckPickingLoad(LoadId, whCode, HuId);
            //既不是拆托 也不是整出 表示有误
            if (mess != "N" && mess != "Y")
            {
                return mess;
            }
            //拆托还要验证新托盘
            if (mess == "N")
            {
                if (!shipHelper.CheckPlt(whCode, PutHuId))
                {
                    return "错误！托盘" + PutHuId + "不存在或已使用！";
                }
            }

            LoadMaster load = idal.ILoadMasterDAL.SelectBy(u => u.WhCode == whCode && u.LoadId == LoadId).First();
            //-----------25 为已备货
            FlowDetail flowDetail = idal.IFlowDetailDAL.SelectBy(u => u.FlowHeadId == load.ProcessId && u.Type == "Picking").First();

            //订单状态修改列表
            List<OutBoundOrder> OutBoundOrderUpdateList = new List<OutBoundOrder>();

            List<TranLog> tranLogAddList = new List<TranLog>();
            List<TranLog> tranLogAddList1 = new List<TranLog>();

            using (TransactionScope trans = new TransactionScope())
            {
                try
                {
                    //修改Load状态
                    if (load.Status1 == "U")
                    {
                        load.Location = Location;
                        load.Status1 = "A";
                        load.UpdateUser = userName;
                        load.UpdateDate = DateTime.Now;
                        load.BeginPickDate = DateTime.Now;
                        idal.ILoadMasterDAL.UpdateBy(load, u => u.Id == load.Id, new string[] { "Status1", "UpdateUser", "UpdateDate", "BeginPickDate", "Location" });

                        //修改OutOrder状态
                        List<OutBoundOrderResult> list = (from a in idal.ILoadDetailDAL.SelectAll()
                                                          where a.LoadMasterId == load.Id
                                                          select new OutBoundOrderResult
                                                          {
                                                              Id = (Int32)a.OutBoundOrderId
                                                          }).ToList();

                        int[] OutBoundOrderId = (from a in list
                                                 select a.Id).ToList().Distinct().ToArray();

                        List<OutBoundOrder> OutBoundOrderList = idal.IOutBoundOrderDAL.SelectBy(u => OutBoundOrderId.Contains(u.Id));

                        foreach (var item in list)
                        {
                            OutBoundOrder outBoundOrder = OutBoundOrderList.Where(u => u.Id == item.Id).First();

                            if (outBoundOrder.StatusId > 15 && outBoundOrder.StatusId < 30) //订单状态必须为草稿以上
                            {
                                string remark1 = "原状态：" + outBoundOrder.StatusId + outBoundOrder.StatusName;

                                //订单流程变更为 25 已备货
                                outBoundOrder.NowProcessId = flowDetail.FlowRuleId;
                                outBoundOrder.StatusId = flowDetail.StatusId;
                                outBoundOrder.StatusName = "正在备货";
                                outBoundOrder.UpdateUser = userName;
                                outBoundOrder.UpdateDate = DateTime.Now;

                                //取消单次更新，变更为批量Sql执行更新
                                OutBoundOrderUpdateList.Add(outBoundOrder);

                                //更新订单状态，插入日志
                                TranLog tl = new TranLog();
                                tl.TranType = "32";
                                tl.Description = "备货更新订单状态";
                                tl.TranDate = DateTime.Now;
                                tl.TranUser = userName;
                                tl.WhCode = whCode;
                                tl.LoadId = LoadId;
                                tl.CustomerOutPoNumber = outBoundOrder.CustomerOutPoNumber;
                                tl.OutPoNumber = outBoundOrder.OutPoNumber;
                                tl.Remark = remark1 + "变更为：" + outBoundOrder.StatusId + outBoundOrder.StatusName;
                                tranLogAddList.Add(tl);

                            }
                        }
                    }

                    //修改出库订单状态为正在备货
                    foreach (var outBoundOrderUpdate in OutBoundOrderUpdateList)
                    {
                        idal.IOutBoundOrderDAL.UpdateByExtended(u => u.Id == outBoundOrderUpdate.Id, t => new OutBoundOrder { NowProcessId = outBoundOrderUpdate.NowProcessId, StatusId = outBoundOrderUpdate.StatusId, StatusName = outBoundOrderUpdate.StatusName, UpdateUser = userName, UpdateDate = DateTime.Now });
                    }

                    //1.修改库存托盘状态 及 实现托盘拆托
                    List<PickTaskDetail> pickList = idal.IPickTaskDetailDAL.SelectBy(u => u.WhCode == whCode && u.LoadId == LoadId && u.HuId == HuId && u.Status == "U");

                    //如果是拆托
                    if (mess == "N")
                    {
                        List<HuDetail> huDetailList = idal.IHuDetailDAL.SelectBy(u => u.WhCode == whCode && u.HuId == HuId);
                        int count = 0;
                        foreach (var item in pickList)
                        {
                            //1.如果是拆托 验证拆托托盘是否存在
                            HuDetail huDetail = huDetailList.Where(u => u.Id == item.HuDetailId).First();
                            if (count == 0)
                            {
                                HuMaster huMaster = new HuMaster();
                                huMaster.WhCode = whCode;
                                huMaster.HuId = PutHuId;
                                huMaster.LoadId = LoadId;
                                huMaster.Type = "M";
                                huMaster.Status = "A";
                                huMaster.Location = Location;
                                huMaster.ReceiptId = huDetail.ReceiptId;
                                huMaster.ReceiptDate = DateTime.Now;
                                huMaster.TransactionFlag = 1;
                                huMaster.CreateUser = userName;
                                huMaster.CreateDate = DateTime.Now;
                                idal.IHuMasterDAL.Add(huMaster);
                            }
                            //1.1加入拆托明细 锁定数量为0
                            HuDetail entity = new HuDetail();
                            entity.WhCode = whCode;
                            entity.HuId = PutHuId;
                            entity.ClientId = huDetail.ClientId;
                            entity.ClientCode = huDetail.ClientCode;
                            entity.SoNumber = huDetail.SoNumber;
                            entity.CustomerPoNumber = huDetail.CustomerPoNumber;
                            entity.AltItemNumber = huDetail.AltItemNumber;
                            entity.ItemId = huDetail.ItemId;
                            entity.UnitId = huDetail.UnitId;
                            entity.UnitName = huDetail.UnitName;
                            entity.ReceiptId = huDetail.ReceiptId;
                            entity.Qty = item.Qty;
                            entity.PlanQty = 0;
                            entity.ReceiptDate = DateTime.Now;
                            entity.Length = huDetail.Length;
                            entity.Width = huDetail.Width;
                            entity.Height = huDetail.Height;
                            entity.Weight = huDetail.Weight;
                            entity.LotNumber1 = huDetail.LotNumber1;
                            entity.LotNumber2 = huDetail.LotNumber2;
                            entity.LotDate = huDetail.LotDate;
                            entity.CreateUser = userName;
                            entity.CreateDate = DateTime.Now;
                            idal.IHuDetailDAL.Add(entity);

                            //1.2 插入备货记录
                            AddPickingTranLog1(huDetail, userName, whCode, item.Location, "", LoadId, PutHuId, item.Qty);

                            //1.3 修改原托盘数量
                            HuDetail entity1 = new HuDetail();
                            entity1.Qty = huDetail.Qty - item.Qty;
                            entity1.PlanQty = huDetail.PlanQty - item.Qty;
                            entity1.UpdateUser = userName;
                            entity1.UpdateDate = DateTime.Now;
                            if (entity1.Qty == 0)
                            {
                                idal.IHuDetailDAL.DeleteBy(u => u.Id == huDetail.Id);

                                //1.3 插入备货删除记录
                                AddPickingTranLog2(huDetail, userName, whCode, item.Location, "", LoadId, PutHuId, item.Qty);
                            }
                            else
                            {
                                idal.IHuDetailDAL.UpdateBy(entity1, u => u.Id == huDetail.Id, new string[] { "Qty", "PlanQty", "UpdateUser", "UpdateDate" });
                            }
                            //1.2 插入备货记录 
                            AddPickingTranLog(entity, userName, whCode, item.Location, Location, LoadId, "");
                            count++;
                        }

                        //1.3 修改备货任务托盘
                        PickTaskDetail pick = new PickTaskDetail();
                        pick.Status = "C";
                        pick.HuId = PutHuId;
                        pick.UpdateUser = userName;
                        pick.UpdateDate = DateTime.Now;
                        idal.IPickTaskDetailDAL.UpdateBy(pick, u => u.LoadId == LoadId && u.WhCode == whCode && u.HuId == HuId, new string[] { "Status", "HuId", "UpdateUser", "UpdateDate" });

                        //1.4 修改原托盘拆托Flag为1
                        HuMaster huma = new HuMaster();
                        huma.TransactionFlag = 1;
                        huma.UpdateUser = userName;
                        huma.UpdateDate = DateTime.Now;
                        idal.IHuMasterDAL.UpdateBy(huma, u => u.WhCode == whCode && u.HuId == HuId, new string[] { "TransactionFlag", "UpdateUser", "UpdateDate" });
                    }
                    else if (mess == "Y")   //整出
                    {
                        HuMaster huMaster = idal.IHuMasterDAL.SelectBy(u => u.WhCode == whCode && u.HuId == HuId).First();
                        //1.1 插入备货记录
                        List<HuDetail> huDetailList = idal.IHuDetailDAL.SelectBy(u => u.WhCode == whCode && u.HuId == HuId);
                        foreach (var item in huDetailList)
                        {
                            AddPickingTranLog(item, userName, whCode, huMaster.Location, Location, LoadId, "");
                        }

                        //1.2 修改托盘信息
                        huMaster.LoadId = LoadId;
                        huMaster.Location = Location;
                        huMaster.UpdateUser = userName;
                        huMaster.UpdateDate = DateTime.Now;
                        idal.IHuMasterDAL.UpdateBy(huMaster, u => u.Id == huMaster.Id, new string[] { "LoadId", "Location", "UpdateUser", "UpdateDate" });

                        HuDetail huDetail = new HuDetail();
                        huDetail.PlanQty = 0;
                        huDetail.UpdateUser = userName;
                        huDetail.UpdateDate = DateTime.Now;
                        idal.IHuDetailDAL.UpdateBy(huDetail, u => u.WhCode == huMaster.WhCode && u.HuId == huMaster.HuId, new string[] { "PlanQty", "UpdateUser", "UpdateDate" });

                        //1.3 修改备货任务状态
                        PickTaskDetail pick = new PickTaskDetail();
                        pick.Status = "C";
                        pick.UpdateUser = userName;
                        pick.UpdateDate = DateTime.Now;
                        idal.IPickTaskDetailDAL.UpdateBy(pick, u => u.LoadId == LoadId && u.WhCode == whCode && u.HuId == HuId, new string[] { "Status", "UpdateUser", "UpdateDate" });
                    }

                    //修改Load的分拣开始时间
                    if (load.Status2 == "U")
                    {
                        load.Status2 = "A";
                        load.BeginSortDate = DateTime.Now;
                        idal.ILoadMasterDAL.UpdateBy(load, u => u.Id == load.Id, new string[] { "Status2", "BeginSortDate" });
                    }

                    idal.SaveChanges();

                    //检查是否全部完成备货
                    var sql4 = from a in idal.IPickTaskDetailDAL.SelectAll()
                               where a.LoadId == LoadId && a.WhCode == whCode && a.Status == "U" && a.HuId != HuId
                               select a;
                    if (sql4.Count() == 0)
                    {
                        load.Status1 = "C";
                        load.UpdateUser = userName;
                        load.UpdateDate = DateTime.Now;
                        load.EndPickDate = DateTime.Now;
                        idal.ILoadMasterDAL.UpdateBy(load, u => u.Id == load.Id, new string[] { "Status1", "UpdateUser", "UpdateDate", "EndPickDate" });

                        //修改OutOrder状态
                        List<OutBoundOrderResult> list = (from a in idal.ILoadDetailDAL.SelectAll()
                                                          where a.LoadMasterId == load.Id
                                                          select new OutBoundOrderResult
                                                          {
                                                              Id = (Int32)a.OutBoundOrderId
                                                          }).ToList();

                        int[] OutBoundOrderId = (from a in list
                                                 select a.Id).ToList().Distinct().ToArray();

                        List<OutBoundOrder> OutBoundOrderList = idal.IOutBoundOrderDAL.SelectBy(u => OutBoundOrderId.Contains(u.Id));

                        foreach (var item in list)
                        {
                            OutBoundOrder outBoundOrder = OutBoundOrderList.Where(u => u.Id == item.Id).First();

                            if (outBoundOrder.StatusName == "正在备货" && outBoundOrder.StatusId >= 15 && outBoundOrder.StatusId < 30)
                            {
                                string remark1 = "原状态：" + outBoundOrder.StatusId + outBoundOrder.StatusName;

                                outBoundOrder.StatusName = flowDetail.StatusName;
                                outBoundOrder.UpdateUser = userName;
                                outBoundOrder.UpdateDate = DateTime.Now;

                                idal.IOutBoundOrderDAL.UpdateByExtended(u => u.Id == outBoundOrder.Id, t => new OutBoundOrder { StatusName = flowDetail.StatusName, UpdateUser = userName, UpdateDate = DateTime.Now });

                                //更新订单状态，插入日志
                                TranLog tl = new TranLog();
                                tl.TranType = "32";
                                tl.Description = "备货更新订单状态";
                                tl.TranDate = DateTime.Now;
                                tl.TranUser = userName;
                                tl.WhCode = whCode;
                                tl.LoadId = LoadId;
                                tl.CustomerOutPoNumber = outBoundOrder.CustomerOutPoNumber;
                                tl.OutPoNumber = outBoundOrder.OutPoNumber;
                                tl.Remark = remark1 + "变更为：" + outBoundOrder.StatusId + outBoundOrder.StatusName;
                                tranLogAddList.Add(tl);
                            }
                        }
                    }

                    //----------------begin 
                    //备货时 同时插入分拣任务表中的PickQty (备货数量)  Qty (分拣数量)
                    //1.首先 根据备货任务表的款号等 查询出 分拣任务表的对应数据

                    List<SortTaskDetail> checkResultList = new List<SortTaskDetail>();
                    foreach (var item in pickList)
                    {
                        List<SortTaskDetail> sortTaskDetailList = idal.ISortTaskDetailDAL.SelectBy(u => u.AltItemNumber == item.AltItemNumber && u.ItemId == item.ItemId && u.LoadId == item.LoadId && u.WhCode == item.WhCode && u.PlanQty != u.PickQty).OrderBy(u => u.GroupId).ToList();

                        int resultQty = item.Qty;
                        foreach (var detail in sortTaskDetailList)
                        {
                            if (checkResultList.Where(u => u.Id == detail.Id).Count() > 0)
                            {
                                detail.PickQty = checkResultList.Where(u => u.Id == detail.Id).First().PickQty;
                            }
                            if (detail.PickQty == detail.PlanQty)
                            {
                                continue;
                            }

                            if (checkResultList.Where(u => u.Id == detail.Id).Count() > 0)
                            {
                                SortTaskDetail old = checkResultList.Where(u => u.Id == detail.Id).First();
                                checkResultList.Remove(old);
                            }

                            if (detail.PickQty + resultQty > detail.PlanQty)
                            {
                                resultQty = resultQty - (detail.PlanQty - (Int32)detail.PickQty);

                                SortTaskDetail entity = new SortTaskDetail();
                                entity.Id = detail.Id;
                                entity.PickQty = detail.PlanQty;
                                entity.Qty = detail.PlanQty;
                                checkResultList.Add(entity);

                                SortTaskDetail editentity = detail;
                                editentity.PickQty = entity.PickQty;

                                continue;
                            }
                            else
                            {
                                SortTaskDetail entity = new SortTaskDetail();
                                entity.Id = detail.Id;
                                entity.PickQty = detail.PickQty + resultQty;
                                entity.Qty = (Int32)detail.PickQty + resultQty;
                                checkResultList.Add(entity);

                                SortTaskDetail editentity = detail;
                                editentity.PickQty = entity.PickQty;

                                break;
                            }
                        }
                    }

                    //更新分拣表的备货数量
                    foreach (var item in checkResultList)
                    {
                        idal.ISortTaskDetailDAL.UpdateByExtended(u => u.Id == item.Id, t => new SortTaskDetail { PickQty = item.PickQty, Qty = (Int32)item.Qty, UpdateUser = userName, UpdateDate = DateTime.Now });
                    }

                    //--------------- end

                    string result1 = "";
                    //装箱
                    if (mess == "N")
                    {
                        result1 = PackingLoad(LoadId, whCode, PutHuId, userName);
                    }
                    else if (mess == "Y")
                    {
                        result1 = PackingLoad(LoadId, whCode, HuId, userName);
                    }
                    if (result1 != "Y")
                    {
                        trans.Dispose();//出现异常，事务手动释放
                        return "备货异常,请重新提交！";
                    }

                    idal.ITranLogDAL.Add(tranLogAddList);
                    idal.SaveChanges();
                    //-------------------------------------------------自动分拣----------------------------------------------------------
                    //-------------------------------------------------自动分拣----------------------------------------------------------
                    //-------------------------------------------------自动分拣----------------------------------------------------------
                    //-------------------------------------------------自动分拣----------------------------------------------------------
                    //-------------------------------------------------自动分拣----------------------------------------------------------
                    //-------------------------------------------------自动分拣----------------------------------------------------------
                    //-------------------------------------------------自动分拣----------------------------------------------------------

                    //验证是否有出库订单完成分拣
                    var sql1 = from a in (
                                               (from sorttaskdetail in idal.ISortTaskDetailDAL.SelectAll()
                                                where
                                                  sorttaskdetail.LoadId == LoadId &&
                                                  sorttaskdetail.WhCode == whCode &&
                                                  sorttaskdetail.GroupNumber == ""
                                                group sorttaskdetail by new
                                                {
                                                    sorttaskdetail.GroupId,
                                                    sorttaskdetail.OutPoNumber
                                                } into g
                                                select new
                                                {
                                                    GroupId = (Int32?)g.Key.GroupId,
                                                    g.Key.OutPoNumber,
                                                    PlanQty = (Int32?)g.Sum(p => p.PlanQty),
                                                    Qty = (Int32?)g.Sum(p => p.Qty)
                                                }))
                               where a.PlanQty == a.Qty
                               select new SortTaskDetailResult
                               {
                                   GroupId = a.GroupId,
                                   OutPoNumber = a.OutPoNumber,
                                   PlanQty = a.PlanQty,
                                   Qty = a.Qty
                               };

                    List<SortTaskDetailResult> groupIdList = sql1.ToList();
                    if (groupIdList.Count() > 0)
                    {
                        List<SortTaskDetail> sortList = idal.ISortTaskDetailDAL.SelectBy(u => u.LoadId == LoadId && u.WhCode == whCode);

                        string[] OutPoNumberArr = (from a in groupIdList
                                                   select a.OutPoNumber).ToList().Distinct().ToArray();

                        List<OutBoundOrder> OutBoundOrderList = idal.IOutBoundOrderDAL.SelectBy(u => u.WhCode == whCode && OutPoNumberArr.Contains(u.OutPoNumber));

                        string[] CustomerOutPoNumberArr = (from a in OutBoundOrderList
                                                           select a.CustomerOutPoNumber).ToList().Distinct().ToArray();

                        List<SortTaskDetail> sortTaskDetailUpdateList = new List<SortTaskDetail>();


                        OutBoundOrder firstOutBoundOrder = OutBoundOrderList.First();

                        //如果检查到流程中有包装流程 
                        List<FlowDetail> checkFlowDetailList = idal.IFlowDetailDAL.SelectBy(u => u.FlowHeadId == firstOutBoundOrder.ProcessId && u.Type == "PackingType");
                        //如果检查到流程中有分拣流程 
                        List<FlowDetail> checkSortingFlowDetailList = idal.IFlowDetailDAL.SelectBy(u => u.FlowHeadId == firstOutBoundOrder.ProcessId && u.Type == "Sorting");

                        string sortingMark = "";
                        if (checkSortingFlowDetailList.Count > 0)
                        {
                            sortingMark = checkSortingFlowDetailList.First().Mark;
                        }

                        //得到Load包装任务表
                        List<PackTask> packTaskList = idal.IPackTaskDAL.SelectBy(u => u.WhCode == whCode && u.LoadId == LoadId);

                        //得到出库订单Json表
                        List<PackTaskJson> packTaskJsonList = idal.IPackTaskJsonDAL.SelectBy(u => u.WhCode == whCode && CustomerOutPoNumberArr.Contains(u.CustomerOutPoNumber));

                        List<PackTask> addPackTaskList = new List<PackTask>();
                        foreach (var item in groupIdList)
                        {
                            OutBoundOrder outBoundOrder = OutBoundOrderList.Where(u => u.WhCode == whCode && u.OutPoNumber == item.OutPoNumber).First();

                            //更新分拣明细中的框号
                            SortTaskDetail sortTaskDetail = new SortTaskDetail();
                            sortTaskDetail.LoadId = LoadId;
                            sortTaskDetail.WhCode = whCode;
                            sortTaskDetail.GroupId = item.GroupId;

                            if (sortingMark == "1")
                            {
                                sortTaskDetail.GroupNumber = LoadId + item.GroupId.ToString();
                            }
                            else if (sortingMark == "2")
                            {
                                sortTaskDetail.GroupNumber = outBoundOrder.CustomerOutPoNumber;
                            }
                            else if (sortingMark == "3")
                            {
                                sortTaskDetail.GroupNumber = outBoundOrder.AltCustomerOutPoNumber;
                            }
                            else
                            {
                                sortTaskDetail.GroupNumber = LoadId + item.GroupId.ToString();
                            }

                            sortTaskDetail.UpdateUser = userName;
                            sortTaskDetail.UpdateDate = DateTime.Now;
                            sortTaskDetailUpdateList.Add(sortTaskDetail);

                            //idal.ISortTaskDetailDAL.UpdateBy(sortTaskDetail, u => u.LoadId == LoadId && u.WhCode == whCode && u.GroupId == item.GroupId, new string[] { "GroupNumber", "UpdateUser", "UpdateDate" });

                            //-----订单完成分拣绑定框号时 发现被拦截
                            //1.更改订单状态为已拦截待处理
                            if (sortList.Where(u => u.HoldFlag == 1 && u.OutPoNumber == item.OutPoNumber).Count() > 0)
                            {
                                if (checkFlowDetailList.Count > 0)
                                {
                                    List<PackTask> checkpackTaskList = packTaskList.Where(u => u.WhCode == whCode && u.LoadId == LoadId && u.OutPoNumber == outBoundOrder.OutPoNumber && u.SortGroupId == item.GroupId).ToList();
                                    if (checkpackTaskList.Count == 0)
                                    {
                                        PackTask packTask = new PackTask();
                                        packTask.WhCode = whCode;
                                        packTask.LoadId = LoadId;
                                        packTask.SortGroupId = item.GroupId;                                //分拣组号

                                        if (sortingMark == "1")
                                        {
                                            packTask.SortGroupNumber = LoadId + item.GroupId.ToString();
                                        }
                                        else if (sortingMark == "2")
                                        {
                                            packTask.SortGroupNumber = outBoundOrder.CustomerOutPoNumber;
                                        }
                                        else if (sortingMark == "3")
                                        {
                                            packTask.SortGroupNumber = outBoundOrder.AltCustomerOutPoNumber;
                                        }
                                        else
                                        {
                                            packTask.SortGroupNumber = LoadId + item.GroupId.ToString();
                                        }

                                        packTask.CustomerOutPoNumber = outBoundOrder.CustomerOutPoNumber;  //客户出库订单号
                                        packTask.OutPoNumber = outBoundOrder.OutPoNumber;                   //系统出库订单号
                                        packTask.Status = -10;                                              //状态为-10：订单被拦截
                                        packTask.CreateUser = userName;
                                        packTask.CreateDate = DateTime.Now;
                                        addPackTaskList.Add(packTask);
                                    }
                                }
                            }
                            else
                            {
                                if (checkFlowDetailList.Count > 0)
                                {
                                    List<PackTask> checkpackTaskList = packTaskList.Where(u => u.WhCode == whCode && u.LoadId == LoadId && u.OutPoNumber == outBoundOrder.OutPoNumber && u.SortGroupId == item.GroupId).ToList();
                                    if (checkpackTaskList.Count == 0)
                                    {
                                        List<PackTaskJson> getPackTaskJson = packTaskJsonList.Where(u => u.WhCode == whCode && u.CustomerOutPoNumber == outBoundOrder.CustomerOutPoNumber).ToList();
                                        if (getPackTaskJson.Count == 0)
                                        {
                                            PackTask packTask = new PackTask();
                                            packTask.WhCode = whCode;
                                            packTask.LoadId = LoadId;
                                            packTask.SortGroupId = item.GroupId;                                //分拣组号

                                            if (sortingMark == "1")
                                            {
                                                packTask.SortGroupNumber = LoadId + item.GroupId.ToString();
                                            }
                                            else if (sortingMark == "2")
                                            {
                                                packTask.SortGroupNumber = outBoundOrder.CustomerOutPoNumber;
                                            }
                                            else if (sortingMark == "3")
                                            {
                                                packTask.SortGroupNumber = outBoundOrder.AltCustomerOutPoNumber;
                                            }
                                            else
                                            {
                                                packTask.SortGroupNumber = LoadId + item.GroupId.ToString();
                                            }

                                            packTask.CustomerOutPoNumber = outBoundOrder.CustomerOutPoNumber;  //客户出库订单号
                                            packTask.OutPoNumber = outBoundOrder.OutPoNumber;                   //系统出库订单号
                                            packTask.Status = 0;                                              //状态为0：任务初始化
                                            packTask.CreateUser = userName;
                                            packTask.CreateDate = DateTime.Now;
                                            addPackTaskList.Add(packTask);
                                        }
                                        else
                                        {
                                            PackTask packTask = new PackTask();

                                            PackTaskJson packJson = getPackTaskJson.First();

                                            PackTaskJsonEntity packTaskJsonEntity = JsonConvert.DeserializeObject<PackTaskJsonEntity>(packJson.Json);

                                            packTask.TransportType = packTaskJsonEntity.TransportType;
                                            packTask.express_code = packTaskJsonEntity.express_code;
                                            packTask.express_type = packTaskJsonEntity.express_type;
                                            packTask.express_type_zh = packTaskJsonEntity.express_type_zh;
                                            packTask.SingleFlag = packTaskJsonEntity.SingleFlag;
                                            if (packTask.SingleFlag == 1)
                                            {
                                                packTask.PackQty = 1;
                                            }

                                            packTask.SinglePlaneTemplate = packTaskJsonEntity.SinglePlaneTemplate;
                                            packTask.PackingListTemplate = packTaskJsonEntity.PackingListTemplate;
                                            packTask.PackMoreFlag = packTaskJsonEntity.PackMoreFlag;
                                            packTask.ZdFlag = packTaskJsonEntity.ZdFlag;

                                            packTask.WhCode = whCode;
                                            packTask.LoadId = LoadId;
                                            packTask.SortGroupId = item.GroupId;                                //分拣组号

                                            if (sortingMark == "1")
                                            {
                                                packTask.SortGroupNumber = LoadId + item.GroupId.ToString();
                                            }
                                            else if (sortingMark == "2")
                                            {
                                                packTask.SortGroupNumber = outBoundOrder.CustomerOutPoNumber;
                                            }
                                            else if (sortingMark == "3")
                                            {
                                                packTask.SortGroupNumber = outBoundOrder.AltCustomerOutPoNumber;
                                            }
                                            else
                                            {
                                                packTask.SortGroupNumber = LoadId + item.GroupId.ToString();
                                            }

                                            packTask.CustomerOutPoNumber = outBoundOrder.CustomerOutPoNumber;  //客户出库订单号
                                            packTask.OutPoNumber = outBoundOrder.OutPoNumber;                  //系统出库订单号
                                            packTask.Status = 10;                                               //状态为10：已获取物流信息
                                            packTask.CreateUser = userName;
                                            packTask.CreateDate = DateTime.Now;
                                            addPackTaskList.Add(packTask);
                                        }

                                    }
                                }

                                if (checkSortingFlowDetailList.Count > 0)
                                {
                                    if (outBoundOrder.StatusId > 15 && outBoundOrder.StatusId < 30) //订单状态必须为草稿以上
                                    {
                                        FlowDetail sortFlowDetail = checkSortingFlowDetailList.First();
                                        string remark1 = "原状态：" + outBoundOrder.StatusId + outBoundOrder.StatusName;

                                        outBoundOrder.NowProcessId = sortFlowDetail.FlowRuleId;
                                        outBoundOrder.StatusId = sortFlowDetail.StatusId;
                                        outBoundOrder.StatusName = sortFlowDetail.StatusName;
                                        outBoundOrder.UpdateUser = userName;
                                        outBoundOrder.UpdateDate = DateTime.Now;

                                        //OutBoundOrderUpdateList.Clear();

                                        ////取消单次更新，变更为批量Sql执行更新
                                        //OutBoundOrderUpdateList.Add(outBoundOrder);

                                        idal.IOutBoundOrderDAL.UpdateByExtended(u => u.Id == outBoundOrder.Id, t => new OutBoundOrder { NowProcessId = sortFlowDetail.FlowRuleId, StatusId = sortFlowDetail.StatusId, StatusName = sortFlowDetail.StatusName, UpdateUser = userName, UpdateDate = DateTime.Now });

                                        //更新订单状态，插入日志
                                        TranLog tl = new TranLog();
                                        tl.TranType = "32";
                                        tl.Description = "分拣更新订单状态";
                                        tl.TranDate = DateTime.Now;
                                        tl.TranUser = userName;
                                        tl.WhCode = whCode;
                                        tl.LoadId = LoadId;
                                        tl.CustomerOutPoNumber = outBoundOrder.CustomerOutPoNumber;
                                        tl.OutPoNumber = outBoundOrder.OutPoNumber;
                                        tl.Remark = remark1 + "变更为：" + outBoundOrder.StatusId + outBoundOrder.StatusName;
                                        tranLogAddList1.Add(tl);
                                    }

                                }
                            }
                        }

                        idal.IPackTaskDAL.Add(addPackTaskList);

                        //更新 分拣任务状态
                        if (sortList.Where(u => u.HoldFlag != 1).Where(u => u.PlanQty != u.PickQty).Count() == 0)
                        {
                            SortTask sortTask = new SortTask();
                            sortTask.Status = "C";
                            sortTask.UpdateUser = userName;
                            sortTask.UpdateDate = DateTime.Now;
                            idal.ISortTaskDAL.UpdateBy(sortTask, u => u.LoadId == LoadId && u.WhCode == whCode, new string[] { "Status", "UpdateUser", "UpdateDate" });

                            //更新分拣完成时间
                            LoadMaster loadMaster = new LoadMaster();
                            loadMaster.Status2 = "C";
                            loadMaster.EndSortDate = DateTime.Now;
                            idal.ILoadMasterDAL.UpdateBy(loadMaster, u => u.LoadId == LoadId && u.WhCode == whCode, new string[] { "Status2", "EndSortDate" });
                        }

                        //批量更新分拣表的分拣框号
                        foreach (var item in sortTaskDetailUpdateList)
                        {
                            idal.ISortTaskDetailDAL.UpdateByExtended(u => u.LoadId == item.LoadId && u.WhCode == item.WhCode && u.GroupId == item.GroupId, t => new SortTaskDetail { GroupNumber = item.GroupNumber, UpdateUser = userName, UpdateDate = DateTime.Now });
                        }

                    }

                    idal.ITranLogDAL.Add(tranLogAddList1);

                    idal.SaveChanges();
                    trans.Complete();
                    return "Y";
                }
                catch (Exception e)
                {
                    trans.Dispose();//出现异常，事务手动释放
                    return "备货异常，请重新提交！";
                }
            }
        }


        //By订单备货后自动装箱自动分拣自动包装
        public string PickingSortingPackingByOrerBegin(List<PickTaskDetailResult> entityList, string whCode, string packGroupNumber, string Location, string userName)
        {
            string result = "";
            if (entityList.Count == 0)
            {
                return "数据有误，请重新操作！";
            }

            string[] loadid = (from a in entityList select a.LoadId).Distinct().ToArray();

            if (loadid.Length > 1)
            {
                return "错误！备货选择了多个Load号！";
            }

            int?[] outOrderNumber = (from a in entityList select a.OutBoundOrderId).Distinct().ToArray();
            if (outOrderNumber.Length > 1)
            {
                return "错误！备货选择了多个订单号！";
            }

            if (!shipHelper.CheckOutLocation(whCode, Location))
            {
                return "错误！备货门区有误！";
            }
            int i = 0;
            //批量验证 数据是否满足
            foreach (var item in entityList)
            {
                List<PickTaskDetail> picktaskList = idal.IPickTaskDetailDAL.SelectBy(u => u.Id == item.Id);
                if (picktaskList.Count == 0)
                {
                    result = "未找到备货任务明细！";
                    break;
                }
                PickTaskDetail picktask = picktaskList.First();

                //第一次 验证出货流程是否符合
                if (i == 0)
                {
                    List<LoadMaster> loadMasterList = idal.ILoadMasterDAL.SelectBy(u => u.WhCode == whCode && u.LoadId == picktask.LoadId);
                    if (loadMasterList.Count == 0)
                    {
                        return "当前Load：" + picktask.LoadId + "信息有误，请检查！";
                    }
                    LoadMaster loadMaster = loadMasterList.First();
                    List<FlowDetail> flowDetailList = (from a in idal.IFlowDetailDAL.SelectAll()
                                                       where a.FlowHeadId == loadMaster.ProcessId
                                                       select a).ToList();
                    string mark = flowDetailList.Where(u => u.Type == "Picking").First().Mark;
                    if (mark != "5")
                    {
                        result = "该Load：" + picktask.LoadId + "不属于该出货流程，无法备货！";
                        break;
                    }
                    i++;
                }

                if (picktask.Status == "C")
                {
                    result = "该备货托盘" + picktask.HuId + "状态有误或不存在，请检查备货任务！";
                    break;
                }

                if (item.PickQty < 1)
                {
                    result = "备货数量必须大于0！";
                    break;
                }

                if ((picktask.Qty - (picktask.PickQty ?? 0)) < item.PickQty)
                {
                    result = "备货数量大于可用数量，请检查备货任务！";
                    break;
                }

                if (!shipHelper.CheckHuStatus(whCode, picktask.HuId))
                {
                    result = "库存托盘:" + picktask.HuId + "状态有误，请检查！";
                    break;
                }
                if (!shipHelper.CheckLocationStatusByHuId(whCode, picktask.HuId))
                {
                    result = "库存托盘:" + picktask.HuId + "对应的库位状态有误，请检查！";
                    break;
                }

                List<PackHead> checkPackGroupNumber = (from a in idal.IPackTaskDAL.SelectAll()
                                                       join b in idal.IPackHeadDAL.SelectAll()
                                                       on a.Id equals b.PackTaskId
                                                       where a.WhCode == whCode && a.LoadId != picktask.LoadId && b.PackNumber.StartsWith(packGroupNumber)
                                                       select b).ToList();
                if (checkPackGroupNumber.Count > 0)
                {
                    result = "包装框号已被其它Load使用，请检查！";
                    break;
                }

                OutBoundOrder getoutBoundOrder = idal.IOutBoundOrderDAL.SelectBy(u => u.Id == picktask.OutBoundOrderId).First();
                packGroupNumber = packGroupNumber + "_" + getoutBoundOrder.CustomerOutPoNumber;

                List<PackHead> checkPackGroupNumber1 = (from a in idal.IPackTaskDAL.SelectAll()
                                                        join b in idal.IPackHeadDAL.SelectAll()
                                                        on a.Id equals b.PackTaskId
                                                        where a.WhCode == whCode && a.LoadId == picktask.LoadId && b.PackNumber.Contains(packGroupNumber)
                                                        select b).ToList();

                if (checkPackGroupNumber1.Where(u => u.Status == 10).Count() > 0)
                {
                    result = "包装框号已称重，无法使用！";
                    break;
                }

                //验证扫描序列号规则是否满足
                List<SortTaskDetail> taskDetailList = idal.ISortTaskDetailDAL.SelectBy(u => u.ItemId == picktask.ItemId && u.WhCode == whCode && u.LoadId == picktask.LoadId && u.OutPoNumber == getoutBoundOrder.OutPoNumber).ToList();

                foreach (var item1 in taskDetailList)
                {
                    if (result != "")
                    {
                        break;
                    }
                    //比较款号中是否有需要验证扫描序列号
                    if (item1.ScanFlag == 1)
                    {
                        if ((item1.ScanRule == null ? "" : item1.ScanRule) != "")
                        {
                            if (Regex.IsMatch(item1.ScanRule, "^[0-9]*$") == true)
                            {
                                int sCheck = Convert.ToInt32(item1.ScanRule);
                                if (sCheck > 0)
                                {
                                    if (item.PackScanNumber != null)
                                    {
                                        foreach (var checkScanNumber in item.PackScanNumber)
                                        {
                                            if (checkScanNumber.ScanNumber.Length != sCheck)
                                            {
                                                result = "款号:" + item.AltItemNumber + "扫描序列号长度不符！";
                                                break;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        result = "款号:" + item.AltItemNumber + "扫描序列号长度不符！";
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                result = "款号:" + item.AltItemNumber + "序列号长度维护有误！";
                                break;
                            }
                        }
                    }
                }
                if (result != "")
                {
                    return result;
                }

                string result1 = "";

                List<PackScanNumber> checkPackPackScanNumberList = (from a in idal.IPackTaskDAL.SelectAll()
                                                                    join b in idal.IPackHeadDAL.SelectAll() on new { Id = a.Id } equals new { Id = (Int32)b.PackTaskId } into b_join
                                                                    from b in b_join.DefaultIfEmpty()
                                                                    join c in idal.IPackDetailDAL.SelectAll() on new { Id = b.Id } equals new { Id = (Int32)c.PackHeadId } into c_join
                                                                    from c in c_join.DefaultIfEmpty()
                                                                    join d in idal.IPackScanNumberDAL.SelectAll() on new { Id = c.Id } equals new { Id = (Int32)d.PackDetailId } into d_join
                                                                    from d in d_join.DefaultIfEmpty()
                                                                    where
                                                                      a.LoadId == picktask.LoadId &&
                                                                      a.WhCode == whCode &&
                                                                      a.OutPoNumber == getoutBoundOrder.OutPoNumber &&
                                                                      (d.ScanNumber ?? "") != ""
                                                                    select d).ToList();
                List<PackScanNumberInsert> checkRFPackScanNumberList = new List<PackScanNumberInsert>();
                int count = 0;
                if (checkPackPackScanNumberList.Count > 0)
                {
                    if (item.PackScanNumber != null)
                    {
                        foreach (var item3 in item.PackScanNumber)
                        {
                            if (checkPackPackScanNumberList.Where(u => u.ScanNumber == item3.ScanNumber).Count() > 0)
                            {
                                if (count >= 2)
                                {
                                    result += "等";
                                    break;
                                }
                                else
                                {
                                    result += item3.ScanNumber + " ";
                                    count++;
                                }
                            }
                            if (checkRFPackScanNumberList.Where(u => u.ScanNumber == item3.ScanNumber).Count() > 0)
                            {
                                if (count >= 2)
                                {
                                    result1 += "等";
                                    break;
                                }
                                else
                                {
                                    result1 += item3.ScanNumber + " ";
                                    count++;
                                }
                            }
                            else
                            {
                                PackScanNumberInsert packScan = new PackScanNumberInsert();
                                packScan.ScanNumber = item3.ScanNumber;
                                checkRFPackScanNumberList.Add(packScan);
                            }
                        }
                    }
                }
                if (result + "" != "")
                {
                    result = "序列号已存在：" + result;
                }
                if (result1 + "" != "")
                {
                    result = "序列号重复：" + result1;
                }
            }
            if (result != "")
            {
                return result;
            }

            using (TransactionScope trans = new TransactionScope())
            {
                try
                {
                    //循环 开始备货 分拣 包装

                    //1.修改 备货任务数量
                    foreach (var item in entityList)
                    {
                        PickTaskDetail picktask = idal.IPickTaskDetailDAL.SelectBy(u => u.Id == item.Id).First();
                        picktask.PickQty = (picktask.PickQty ?? 0) + item.PickQty;

                        if (picktask.PickQty == picktask.Qty)
                        {
                            picktask.Status = "C";
                        }
                        else
                        {
                            picktask.Status = "A";
                        }

                        idal.IPickTaskDetailDAL.UpdateBy(picktask, u => u.Id == item.Id, new string[] { "PickQty", "Status" });
                    }

                    //2.修改Load状态 及 订单状态

                    PickTaskDetailResult pickFirst = entityList.First();

                    LoadMaster load = idal.ILoadMasterDAL.SelectBy(u => u.WhCode == whCode && u.LoadId == pickFirst.LoadId).First();
                    //-----------25 为已备货
                    FlowDetail flowDetail = idal.IFlowDetailDAL.SelectBy(u => u.FlowHeadId == load.ProcessId && u.Type == "Picking").First();

                    if (load.Status1 == "U")
                    {
                        load.Location = Location;
                        load.Status1 = "A";
                        load.Status2 = "A";
                        load.Status3 = "A";
                        load.UpdateUser = userName;
                        load.UpdateDate = DateTime.Now;
                        load.BeginPickDate = DateTime.Now;
                        load.BeginSortDate = DateTime.Now;
                        load.BeginPackDate = DateTime.Now;
                        idal.ILoadMasterDAL.UpdateBy(load, u => u.Id == load.Id, new string[] { "Status1", "UpdateUser", "UpdateDate", "BeginPickDate", "Location", "Status2", "Status3", "BeginSortDate", "BeginPackDate" });

                        //修改OutOrder状态
                        List<OutBoundOrderResult> list = (from a in idal.ILoadDetailDAL.SelectAll()
                                                          where a.LoadMasterId == load.Id
                                                          select new OutBoundOrderResult
                                                          {
                                                              Id = (Int32)a.OutBoundOrderId
                                                          }).ToList();
                        foreach (var item in list)
                        {
                            OutBoundOrder outBoundOrder = idal.IOutBoundOrderDAL.SelectBy(u => u.Id == item.Id).First();
                            if (outBoundOrder.StatusId > 15 && outBoundOrder.StatusId < 30) //订单状态必须为草稿以上
                            {
                                string remark1 = "原状态：" + outBoundOrder.StatusId + outBoundOrder.StatusName;

                                outBoundOrder.NowProcessId = flowDetail.FlowRuleId;
                                outBoundOrder.StatusId = flowDetail.StatusId;
                                outBoundOrder.StatusName = "正在备货";
                                outBoundOrder.UpdateUser = userName;
                                outBoundOrder.UpdateDate = DateTime.Now;
                                idal.IOutBoundOrderDAL.UpdateBy(outBoundOrder, u => u.Id == item.Id, new string[] { "NowProcessId", "StatusId", "StatusName", "UpdateUser", "UpdateDate" });

                                //更新订单状态，插入日志
                                TranLog tl = new TranLog();
                                tl.TranType = "32";
                                tl.Description = "更新订单状态";
                                tl.TranDate = DateTime.Now;
                                tl.TranUser = userName;
                                tl.WhCode = whCode;
                                tl.LoadId = load.LoadId;
                                tl.CustomerOutPoNumber = outBoundOrder.CustomerOutPoNumber;
                                tl.OutPoNumber = outBoundOrder.OutPoNumber;
                                tl.Remark = remark1 + "变更为：" + outBoundOrder.StatusId + outBoundOrder.StatusName;
                                idal.ITranLogDAL.Add(tl);
                            }
                        }
                    }
                    idal.IPickTaskDetailDAL.SaveChanges();

                    //系统自动取得拆托托盘
                    List<string> PutHuIdList = (from a in idal.IPallateDAL.SelectAll()
                                                join b in idal.IHuDetailDAL.SelectAll()
                                                on new { A = a.WhCode, B = a.HuId } equals new { A = b.WhCode, B = b.HuId } into temp1
                                                from b in temp1.DefaultIfEmpty()
                                                where a.WhCode == whCode && (b.HuId ?? "") == ""
                                                select a.HuId
                                      ).Take(1).ToList();

                    string PutHuId = PutHuIdList.First();

                    List<PickTaskDetail> checkHuList = new List<PickTaskDetail>();
                    //验证是否有托盘 完成备货
                    foreach (var picktaskDetail in entityList)
                    {
                        PickTaskDetail picktask = idal.IPickTaskDetailDAL.SelectBy(u => u.Id == picktaskDetail.Id).First();
                        List<PickTaskDetail> list = idal.IPickTaskDetailDAL.SelectBy(u => u.LoadId == picktask.LoadId && u.WhCode == whCode && u.HuId == picktask.HuId && u.PickQty != u.Qty);

                        //如果有托盘 备货数量等于已备货数量
                        //验证该备货托盘 是否全部款号均完成备货，如果完成备货 执行备货装箱
                        if (list.Count == 0)
                        {

                            #region -----------------------------备货及装箱-----------------------------------

                            //防止多次对同一托盘进行装箱
                            if (checkHuList.Where(u => u.HuId == picktask.HuId).Count() == 0)
                            {
                                PickTaskDetail checkHu = new PickTaskDetail();
                                checkHu.HuId = picktask.HuId;
                                checkHuList.Add(checkHu);

                                string LoadId = picktask.LoadId;
                                string HuId = picktask.HuId;

                                string mess = "";   //是否是拆托 N是拆托 Y不是拆托

                                //执行装箱拆托 
                                List<PickTaskDetail> pickList = idal.IPickTaskDetailDAL.SelectBy(u => u.WhCode == whCode && u.LoadId == LoadId && u.HuId == HuId);

                                List<HuDetail> huDetailList = idal.IHuDetailDAL.SelectBy(u => u.WhCode == whCode && u.HuId == HuId);

                                int pickQty = pickList.Sum(u => u.Qty);
                                int inventoryQty = huDetailList.Sum(u => u.Qty);

                                if (pickQty == inventoryQty)
                                {
                                    foreach (var item in pickList)
                                    {
                                        if (mess == "")
                                        {
                                            HuDetail huDetail = huDetailList.Where(u => u.Id == item.HuDetailId).First();
                                            if (huDetail.Qty > item.Qty)
                                            {
                                                mess = "N";
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    mess = "N";
                                }
                                if (mess == "")
                                {
                                    mess = "Y";
                                }

                                //如果是拆托
                                if (mess == "N")
                                {
                                    int count = 0;
                                    foreach (var item in pickList)
                                    {
                                        //1.如果是拆托 验证拆托托盘是否存在
                                        HuDetail huDetail = huDetailList.Where(u => u.Id == item.HuDetailId).First();
                                        if (count == 0)
                                        {
                                            HuMaster huMaster = new HuMaster();
                                            huMaster.WhCode = whCode;
                                            huMaster.HuId = PutHuId;
                                            huMaster.LoadId = LoadId;
                                            huMaster.Type = "M";
                                            huMaster.Status = "A";
                                            huMaster.Location = Location;
                                            huMaster.ReceiptId = huDetail.ReceiptId;
                                            huMaster.ReceiptDate = DateTime.Now;
                                            huMaster.TransactionFlag = 1;
                                            huMaster.CreateUser = userName;
                                            huMaster.CreateDate = DateTime.Now;
                                            idal.IHuMasterDAL.Add(huMaster);
                                        }
                                        //1.1加入拆托明细 锁定数量为0
                                        HuDetail entity = new HuDetail();
                                        entity.WhCode = whCode;
                                        entity.HuId = PutHuId;
                                        entity.ClientId = huDetail.ClientId;
                                        entity.ClientCode = huDetail.ClientCode;
                                        entity.SoNumber = huDetail.SoNumber;
                                        entity.CustomerPoNumber = huDetail.CustomerPoNumber;
                                        entity.AltItemNumber = huDetail.AltItemNumber;
                                        entity.ItemId = huDetail.ItemId;
                                        entity.UnitId = huDetail.UnitId;
                                        entity.UnitName = huDetail.UnitName;
                                        entity.ReceiptId = huDetail.ReceiptId;
                                        entity.Qty = item.Qty;
                                        entity.PlanQty = 0;
                                        entity.ReceiptDate = DateTime.Now;
                                        entity.Length = huDetail.Length;
                                        entity.Width = huDetail.Width;
                                        entity.Height = huDetail.Height;
                                        entity.Weight = huDetail.Weight;
                                        entity.LotNumber1 = huDetail.LotNumber1;
                                        entity.LotNumber2 = huDetail.LotNumber2;
                                        entity.LotDate = huDetail.LotDate;
                                        entity.CreateUser = userName;
                                        entity.CreateDate = DateTime.Now;
                                        idal.IHuDetailDAL.Add(entity);

                                        //1.2 插入备货记录
                                        AddPickingTranLog1(huDetail, userName, whCode, item.Location, "", LoadId, PutHuId, item.Qty);

                                        //1.3 修改原托盘数量
                                        HuDetail entity1 = new HuDetail();
                                        entity1.Qty = huDetail.Qty - item.Qty;
                                        entity1.PlanQty = huDetail.PlanQty - item.Qty;
                                        entity1.UpdateUser = userName;
                                        entity1.UpdateDate = DateTime.Now;
                                        if (entity1.Qty == 0)
                                        {
                                            idal.IHuDetailDAL.DeleteBy(u => u.Id == huDetail.Id);

                                            //1.3 插入备货删除记录
                                            AddPickingTranLog2(huDetail, userName, whCode, item.Location, "", LoadId, PutHuId, item.Qty);
                                        }
                                        else
                                        {
                                            idal.IHuDetailDAL.UpdateBy(entity1, u => u.Id == huDetail.Id, new string[] { "Qty", "PlanQty", "UpdateUser", "UpdateDate" });
                                        }
                                        //1.2 插入备货记录 
                                        AddPickingTranLog(entity, userName, whCode, item.Location, Location, LoadId, "");
                                        count++;
                                    }

                                    //1.3 修改备货任务托盘
                                    PickTaskDetail pick = new PickTaskDetail();
                                    pick.HuId = PutHuId;
                                    pick.UpdateUser = userName;
                                    pick.UpdateDate = DateTime.Now;
                                    idal.IPickTaskDetailDAL.UpdateBy(pick, u => u.LoadId == LoadId && u.WhCode == whCode && u.HuId == HuId, new string[] { "HuId", "UpdateUser", "UpdateDate" });

                                    //1.4 修改原托盘拆托Flag为1
                                    HuMaster huma = new HuMaster();
                                    huma.TransactionFlag = 1;
                                    huma.UpdateUser = userName;
                                    huma.UpdateDate = DateTime.Now;
                                    idal.IHuMasterDAL.UpdateBy(huma, u => u.WhCode == whCode && u.HuId == HuId, new string[] { "TransactionFlag", "UpdateUser", "UpdateDate" });

                                }
                                else if (mess == "Y")   //整出
                                {
                                    HuMaster huMaster = idal.IHuMasterDAL.SelectBy(u => u.WhCode == whCode && u.HuId == HuId).First();
                                    //1.1 插入备货记录
                                    foreach (var item in huDetailList)
                                    {
                                        AddPickingTranLog(item, userName, whCode, huMaster.Location, Location, LoadId, "");
                                    }

                                    //1.2 修改托盘信息
                                    huMaster.LoadId = LoadId;
                                    huMaster.Location = Location;
                                    huMaster.UpdateUser = userName;
                                    huMaster.UpdateDate = DateTime.Now;
                                    idal.IHuMasterDAL.UpdateBy(huMaster, u => u.Id == huMaster.Id, new string[] { "LoadId", "Location", "UpdateUser", "UpdateDate" });

                                    HuDetail huDetail = new HuDetail();
                                    huDetail.PlanQty = 0;
                                    huDetail.UpdateUser = userName;
                                    huDetail.UpdateDate = DateTime.Now;
                                    idal.IHuDetailDAL.UpdateBy(huDetail, u => u.WhCode == huMaster.WhCode && u.HuId == huMaster.HuId, new string[] { "PlanQty", "UpdateUser", "UpdateDate" });
                                }

                                idal.IHuDetailDAL.SaveChanges();

                                string result1 = "";
                                //装箱
                                if (mess == "N")
                                    result1 = PackingLoad(LoadId, whCode, PutHuId, userName);
                                else if (mess == "Y")
                                    result1 = PackingLoad(LoadId, whCode, HuId, userName);
                                if (result1 != "Y")
                                {
                                    trans.Dispose();//出现异常，事务手动释放
                                    return "备货异常,请重新提交！";
                                }
                            }
                            #endregion
                        }

                        //增加分拣表备货数量、分拣数量、 包装数量、插入包装任务等

                        //----------------begin 
                        //备货时 同时插入分拣任务表中的PickQty (备货数量)  Qty (分拣数量)
                        //1.首先 根据备货任务表的款号等 查询出 分拣任务表的对应数据

                        OutBoundOrder outBoundOrder = idal.IOutBoundOrderDAL.SelectBy(u => u.Id == picktask.OutBoundOrderId).First();

                        //如果检查到流程中有分拣流程 
                        List<FlowDetail> checkSortingFlowDetailList = idal.IFlowDetailDAL.SelectBy(u => u.FlowHeadId == outBoundOrder.ProcessId && u.Type == "Sorting");

                        string sortingMark = "";
                        if (checkSortingFlowDetailList.Count > 0)
                        {
                            sortingMark = checkSortingFlowDetailList.First().Mark;
                        }

                        List<SortTaskDetail> sortTaskDetailList = idal.ISortTaskDetailDAL.SelectBy(u => u.AltItemNumber == picktask.AltItemNumber && u.ItemId == picktask.ItemId && u.LoadId == picktask.LoadId && u.WhCode == whCode && u.PlanQty != u.PickQty && u.OutPoNumber == outBoundOrder.OutPoNumber).OrderBy(u => u.GroupId).ToList();

                        int? sortGroupId = 0;
                        int resultQty = (Int32)picktaskDetail.PickQty;
                        foreach (var detail in sortTaskDetailList)
                        {
                            if (detail.PickQty + resultQty > detail.PlanQty)
                            {
                                resultQty = resultQty - (detail.PlanQty - (Int32)detail.PickQty);

                                SortTaskDetail entity = new SortTaskDetail();

                                if (sortingMark == "1")
                                {
                                    entity.GroupNumber = detail.LoadId + detail.GroupId.ToString();
                                }
                                else if (sortingMark == "2")
                                {
                                    entity.GroupNumber = outBoundOrder.CustomerOutPoNumber;
                                }
                                else if (sortingMark == "3")
                                {
                                    entity.GroupNumber = outBoundOrder.AltCustomerOutPoNumber;
                                }
                                else
                                {
                                    entity.GroupNumber = detail.LoadId + detail.GroupId.ToString();
                                }

                                entity.PackQty = detail.PlanQty;
                                entity.PickQty = detail.PlanQty;
                                entity.Qty = (Int32)detail.PlanQty;
                                entity.UpdateUser = userName;
                                entity.UpdateDate = DateTime.Now;
                                idal.ISortTaskDetailDAL.UpdateBy(entity, u => u.Id == detail.Id, new string[] { "GroupNumber", "PackQty", "PickQty", "Qty", "UpdateUser", "UpdateDate" });
                                sortGroupId = detail.GroupId;
                                continue;
                            }
                            else
                            {
                                SortTaskDetail entity = new SortTaskDetail();

                                if (sortingMark == "1")
                                {
                                    entity.GroupNumber = detail.LoadId + detail.GroupId.ToString();
                                }
                                else if (sortingMark == "2")
                                {
                                    entity.GroupNumber = outBoundOrder.CustomerOutPoNumber;
                                }
                                else if (sortingMark == "3")
                                {
                                    entity.GroupNumber = outBoundOrder.AltCustomerOutPoNumber;
                                }
                                else
                                {
                                    entity.GroupNumber = detail.LoadId + detail.GroupId.ToString();
                                }

                                entity.PackQty = detail.PackQty + resultQty;
                                entity.PickQty = detail.PickQty + resultQty;
                                entity.Qty = (Int32)detail.PickQty + resultQty;
                                entity.UpdateUser = userName;
                                entity.UpdateDate = DateTime.Now;
                                idal.ISortTaskDetailDAL.UpdateBy(entity, u => u.Id == detail.Id, new string[] { "GroupNumber", "PackQty", "PickQty", "Qty", "UpdateUser", "UpdateDate" });
                                sortGroupId = detail.GroupId;
                                break;
                            }
                        }
                        //--------------- end
                        idal.ISortTaskDetailDAL.SaveChanges();

                        //插入包装任务信息
                        //如果检查到没有包装任务 添加包装任务

                        List<PackTask> checkPackTaskList = idal.IPackTaskDAL.SelectBy(u => u.WhCode == whCode && u.LoadId == pickFirst.LoadId && u.CustomerOutPoNumber == outBoundOrder.CustomerOutPoNumber && u.OutPoNumber == outBoundOrder.OutPoNumber);

                        PackTask packTask = new PackTask();
                        if (checkPackTaskList.Count == 0)
                        {
                            List<PackTaskJson> getPackTaskJson = idal.IPackTaskJsonDAL.SelectBy(u => u.WhCode == whCode && u.CustomerOutPoNumber == outBoundOrder.CustomerOutPoNumber);
                            if (getPackTaskJson.Count == 0)
                            {
                                packTask.WhCode = whCode;
                                packTask.LoadId = pickFirst.LoadId;
                                packTask.SortGroupId = sortGroupId; //分拣组号

                                if (sortingMark == "1")
                                {
                                    packTask.SortGroupNumber = pickFirst.LoadId + sortGroupId.ToString(); //分拣框号
                                }
                                else if (sortingMark == "2")
                                {
                                    packTask.SortGroupNumber = outBoundOrder.CustomerOutPoNumber;
                                }
                                else if (sortingMark == "3")
                                {
                                    packTask.SortGroupNumber = outBoundOrder.AltCustomerOutPoNumber;
                                }
                                else
                                {
                                    packTask.SortGroupNumber = pickFirst.LoadId + sortGroupId.ToString(); //分拣框号
                                }

                                packTask.CustomerOutPoNumber = outBoundOrder.CustomerOutPoNumber;  //客户出库订单号
                                packTask.OutPoNumber = outBoundOrder.OutPoNumber;      //系统出库订单号
                                packTask.Status = 0;                        //状态为-10：订单被拦截
                                packTask.CreateUser = userName;
                                packTask.CreateDate = DateTime.Now;
                            }
                            else
                            {
                                PackTaskJson packJson = getPackTaskJson.First();

                                PackTaskJsonEntity packTaskJsonEntity = JsonConvert.DeserializeObject<PackTaskJsonEntity>(packJson.Json);

                                packTask.TransportType = packTaskJsonEntity.TransportType;
                                packTask.express_code = packTaskJsonEntity.express_code;
                                packTask.express_type = packTaskJsonEntity.express_type;
                                packTask.express_type_zh = packTaskJsonEntity.express_type_zh;
                                packTask.SingleFlag = packTaskJsonEntity.SingleFlag;

                                if (packTask.SingleFlag == 1)
                                {
                                    packTask.PackQty = 1;
                                }

                                packTask.SinglePlaneTemplate = packTaskJsonEntity.SinglePlaneTemplate;
                                packTask.PackingListTemplate = packTaskJsonEntity.PackingListTemplate;

                                packTask.WhCode = whCode;
                                packTask.LoadId = pickFirst.LoadId;
                                packTask.SortGroupId = sortGroupId; //分拣组号

                                if (sortingMark == "1")
                                {
                                    packTask.SortGroupNumber = pickFirst.LoadId + sortGroupId.ToString(); //分拣框号
                                }
                                else if (sortingMark == "2")
                                {
                                    packTask.SortGroupNumber = outBoundOrder.CustomerOutPoNumber;
                                }
                                else if (sortingMark == "3")
                                {
                                    packTask.SortGroupNumber = outBoundOrder.AltCustomerOutPoNumber;
                                }
                                else
                                {
                                    packTask.SortGroupNumber = pickFirst.LoadId + sortGroupId.ToString(); //分拣框号
                                }

                                packTask.CustomerOutPoNumber = outBoundOrder.CustomerOutPoNumber;  //客户出库订单号
                                packTask.OutPoNumber = outBoundOrder.OutPoNumber;      //系统出库订单号      
                                packTask.Status = 10;                        //状态为-10：订单被拦截
                                packTask.CreateUser = userName;
                                packTask.CreateDate = DateTime.Now;
                            }

                            idal.IPackTaskDAL.Add(packTask);
                            idal.IPackTaskDAL.SaveChanges();
                        }
                        else
                        {
                            packTask = checkPackTaskList.First();
                        }

                        //2.----------插入包装头表
                        PackHead packHead = new PackHead();
                        List<PackHead> checkPackHeadCount = idal.IPackHeadDAL.SelectBy(u => u.PackTaskId == packTask.Id && u.WhCode == packTask.WhCode && u.PackNumber == packGroupNumber).ToList();
                        if (checkPackHeadCount.Count == 0)
                        {
                            List<PackHead> packHeadList = idal.IPackHeadDAL.SelectBy(u => u.PackTaskId == packTask.Id && u.WhCode == packTask.WhCode).ToList();
                            packHead.WhCode = packTask.WhCode;
                            packHead.PackTaskId = packTask.Id;
                            if (packHeadList.Count == 0)
                            {
                                packHead.PackGroupId = 1;
                            }
                            else
                            {
                                packHead.PackGroupId = Convert.ToInt32(packHeadList.Max(u => u.PackGroupId).ToString()) + 1;
                            }
                            packHead.PackNumber = packGroupNumber;
                            packHead.Status = 0;
                            packHead.ExpressStatus = "N";
                            packHead.CreateUser = userName;
                            packHead.CreateDate = DateTime.Now;
                            idal.IPackHeadDAL.Add(packHead);
                            idal.IPackHeadDAL.SaveChanges();        //保存一次 取得包装头表主键ID 
                        }
                        else
                        {
                            packHead = checkPackHeadCount.First();
                        }

                        //3.----------插入包装明细表

                        PackDetail packDetail = new PackDetail();
                        List<PackDetail> checkPackDetailCount = idal.IPackDetailDAL.SelectBy(u => u.PackHeadId == packHead.Id && u.WhCode == packTask.WhCode && u.ItemId == picktask.ItemId).ToList();
                        if (checkPackDetailCount.Count == 0)
                        {
                            packDetail.WhCode = packTask.WhCode;
                            packDetail.PackHeadId = packHead.Id;
                            packDetail.ItemId = picktask.ItemId;
                            packDetail.Qty = resultQty;
                            packDetail.CreateUser = userName;
                            packDetail.CreateDate = DateTime.Now;
                            idal.IPackDetailDAL.Add(packDetail);
                        }
                        else
                        {
                            packDetail = checkPackDetailCount.First();

                            PackDetail update = new PackDetail();
                            update.Qty = resultQty + packDetail.Qty;
                            update.UpdateUser = userName;
                            update.UpdateDate = DateTime.Now;

                            idal.IPackDetailDAL.UpdateBy(update, u => u.Id == packDetail.Id, new string[] { "Qty", "UpdateUser", "UpdateDate" });
                        }

                        //3.1---------插入包装扫描表
                        if (picktaskDetail.PackScanNumber != null)
                        {
                            idal.IPackDetailDAL.SaveChanges();        //保存一次 取得包装明细表主键ID
                            foreach (var item1 in picktaskDetail.PackScanNumber)
                            {
                                PackScanNumber packScanNumber = new PackScanNumber();
                                packScanNumber.WhCode = packTask.WhCode;
                                packScanNumber.PackDetailId = packDetail.Id;
                                packScanNumber.ScanNumber = item1.ScanNumber;
                                packScanNumber.CreateUser = userName;
                                packScanNumber.CreateDate = DateTime.Now;
                                idal.IPackScanNumberDAL.Add(packScanNumber);
                            }
                        }

                        idal.IPackDetailDAL.SaveChanges();

                        //验证 分拣明细中 订单包装是否全部完成
                        List<SortTaskDetail> checkAllQty = idal.ISortTaskDetailDAL.SelectBy(u => u.WhCode == whCode && u.LoadId == pickFirst.LoadId && u.GroupId == packTask.SortGroupId && u.GroupNumber == packTask.SortGroupNumber && u.PlanQty != u.PackQty).ToList();
                        if (checkAllQty.Count == 0)
                        {
                            //如果完成包装 更新出库订单状态
                            List<OutBoundOrder> eneityList = idal.IOutBoundOrderDAL.SelectBy(u => u.WhCode == packTask.WhCode && u.OutPoNumber == packTask.OutPoNumber);
                            if (eneityList.Count != 0)
                            {
                                OutBoundOrder outBoundOrder1 = eneityList.First();

                                if (outBoundOrder1.StatusId > 15 && outBoundOrder1.StatusId < 40) //订单状态必须为草稿以上
                                {
                                    FlowDetail flowDetail1 = idal.IFlowDetailDAL.SelectBy(u => u.FlowHeadId == outBoundOrder1.ProcessId && u.Type == "PackingType").First();

                                    if (flowDetail1 != null && flowDetail1.StatusId != 0)
                                    {
                                        string remark1 = "原状态：" + outBoundOrder.StatusId + outBoundOrder.StatusName;

                                        //更新出库订单状态为已包装
                                        outBoundOrder1.NowProcessId = flowDetail1.FlowRuleId;
                                        outBoundOrder1.StatusId = flowDetail1.StatusId;
                                        outBoundOrder1.StatusName = flowDetail1.StatusName;
                                        outBoundOrder1.UpdateUser = userName;
                                        outBoundOrder1.UpdateDate = DateTime.Now;
                                        idal.IOutBoundOrderDAL.UpdateBy(outBoundOrder1, u => u.Id == outBoundOrder1.Id, new string[] { "NowProcessId", "StatusId", "StatusName", "UpdateUser", "UpdateDate" });

                                        //更新订单状态，插入日志
                                        TranLog tl = new TranLog();
                                        tl.TranType = "32";
                                        tl.Description = "更新订单状态";
                                        tl.TranDate = DateTime.Now;
                                        tl.TranUser = userName;
                                        tl.WhCode = whCode;
                                        tl.LoadId = pickFirst.LoadId;
                                        tl.CustomerOutPoNumber = outBoundOrder.CustomerOutPoNumber;
                                        tl.OutPoNumber = outBoundOrder.OutPoNumber;
                                        tl.Remark = remark1 + "变更为：" + outBoundOrder.StatusId + outBoundOrder.StatusName;
                                        idal.ITranLogDAL.Add(tl);
                                    }
                                }
                            }
                        }
                    }

                    //检查是否全部完成备货
                    List<PickTaskDetail> sql4 = (from a in idal.IPickTaskDetailDAL.SelectAll()
                                                 where a.LoadId == pickFirst.LoadId && a.WhCode == whCode && a.Status == "U" || a.Status == "A"
                                                 select a).ToList();
                    if (sql4.Count() == 0)
                    {
                        load.Status1 = "C";
                        load.Status2 = "C";
                        load.Status3 = "C";
                        load.UpdateUser = userName;
                        load.UpdateDate = DateTime.Now;
                        load.EndPickDate = DateTime.Now;
                        load.EndSortDate = DateTime.Now;
                        load.EndPackDate = DateTime.Now;
                        idal.ILoadMasterDAL.UpdateBy(load, u => u.Id == load.Id, new string[] { "Status1", "Status2", "Status3", "UpdateUser", "UpdateDate", "EndPickDate", "EndSortDate", "EndPackDate" });

                        //更新包装任务状态 为 已完成
                        PackTask setPackTask = new PackTask();
                        setPackTask.Status = 30;
                        setPackTask.UpdateUser = userName;
                        setPackTask.UpdateDate = DateTime.Now;
                        idal.IPackTaskDAL.UpdateBy(setPackTask, u => u.WhCode == whCode && u.LoadId == pickFirst.LoadId, new string[] { "Status", "UpdateUser", "UpdateDate" });
                    }

                    idal.ILoadMasterDAL.SaveChanges();
                    trans.Complete();
                    return "Y";
                }
                catch
                {
                    trans.Dispose();
                    return "备货异常，请重新提交！";
                }
            }
        }


        //By订单备货后自动装箱自动分拣
        public string PickingSortingByOrerBegin(List<PickTaskDetailResult> entityList, string whCode, string packGroupNumber, string Location, string userName)
        {
            string result = "";
            if (entityList.Count == 0)
            {
                return "数据有误，请重新操作！";
            }

            string[] loadid = (from a in entityList select a.LoadId).Distinct().ToArray();

            if (loadid.Length > 1)
            {
                return "错误！备货选择了多个Load号！";
            }

            int?[] outOrderNumber = (from a in entityList select a.OutBoundOrderId).Distinct().ToArray();
            if (outOrderNumber.Length > 1)
            {
                return "错误！备货选择了多个订单号！";
            }

            if (!shipHelper.CheckOutLocation(whCode, Location))
            {
                return "错误！备货门区有误！";
            }
            int i = 0;
            //批量验证 数据是否满足
            foreach (var item in entityList)
            {
                List<PickTaskDetail> picktaskList = idal.IPickTaskDetailDAL.SelectBy(u => u.Id == item.Id);
                if (picktaskList.Count == 0)
                {
                    result = "未找到备货任务明细！";
                    break;
                }
                PickTaskDetail picktask = picktaskList.First();

                //第一次 验证出货流程是否符合
                if (i == 0)
                {
                    List<LoadMaster> loadMasterList = idal.ILoadMasterDAL.SelectBy(u => u.WhCode == whCode && u.LoadId == picktask.LoadId);
                    if (loadMasterList.Count == 0)
                    {
                        return "当前Load：" + picktask.LoadId + "信息有误，请检查！";
                    }
                    LoadMaster loadMaster = loadMasterList.First();
                    List<FlowDetail> flowDetailList = (from a in idal.IFlowDetailDAL.SelectAll()
                                                       where a.FlowHeadId == loadMaster.ProcessId
                                                       select a).ToList();
                    string mark = flowDetailList.Where(u => u.Type == "Picking").First().Mark;
                    if (mark != "6")
                    {
                        result = "该Load：" + picktask.LoadId + "不属于该出货流程，无法备货！";
                        break;
                    }
                    i++;
                }

                if (picktask.Status == "C")
                {
                    result = "该备货托盘" + picktask.HuId + "状态有误或不存在，请检查备货任务！";
                    break;
                }

                if (item.PickQty < 1)
                {
                    result = "备货数量必须大于0！";
                    break;
                }

                if ((picktask.Qty - (picktask.PickQty ?? 0)) < item.PickQty)
                {
                    result = "备货数量大于可用数量，请检查备货任务！";
                    break;
                }

                if (!shipHelper.CheckHuStatus(whCode, picktask.HuId))
                {
                    result = "库存托盘:" + picktask.HuId + "状态有误，请检查！";
                    break;
                }
                if (!shipHelper.CheckLocationStatusByHuId(whCode, picktask.HuId))
                {
                    result = "库存托盘:" + picktask.HuId + "对应的库位状态有误，请检查！";
                    break;
                }

                OutBoundOrder getoutBoundOrder = idal.IOutBoundOrderDAL.SelectBy(u => u.Id == picktask.OutBoundOrderId).First();
                packGroupNumber = packGroupNumber + "_" + getoutBoundOrder.CustomerOutPoNumber;

                //验证扫描序列号规则是否满足
                List<SortTaskDetail> taskDetailList = idal.ISortTaskDetailDAL.SelectBy(u => u.ItemId == picktask.ItemId && u.WhCode == whCode && u.LoadId == picktask.LoadId && u.OutPoNumber == getoutBoundOrder.OutPoNumber).ToList();

                foreach (var item1 in taskDetailList)
                {
                    if (result != "")
                    {
                        break;
                    }
                    //比较款号中是否有需要验证扫描序列号
                    if (item1.ScanFlag == 1)
                    {
                        if ((item1.ScanRule == null ? "" : item1.ScanRule) != "")
                        {
                            if (Regex.IsMatch(item1.ScanRule, "^[0-9]*$") == true)
                            {
                                int sCheck = Convert.ToInt32(item1.ScanRule);
                                if (sCheck > 0)
                                {
                                    if (item.PackScanNumber != null)
                                    {
                                        foreach (var checkScanNumber in item.PackScanNumber)
                                        {
                                            if (checkScanNumber.ScanNumber.Length != sCheck)
                                            {
                                                result = "款号:" + item.AltItemNumber + "扫描序列号长度不符！";
                                                break;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        result = "款号:" + item.AltItemNumber + "扫描序列号长度不符！";
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                result = "款号:" + item.AltItemNumber + "序列号长度维护有误！";
                                break;
                            }
                        }
                    }
                }
                if (result != "")
                {
                    return result;
                }

                string result1 = "";

                List<PackScanNumber> checkPackPackScanNumberList = (from a in idal.IPackTaskDAL.SelectAll()
                                                                    join b in idal.IPackHeadDAL.SelectAll() on new { Id = a.Id } equals new { Id = (Int32)b.PackTaskId } into b_join
                                                                    from b in b_join.DefaultIfEmpty()
                                                                    join c in idal.IPackDetailDAL.SelectAll() on new { Id = b.Id } equals new { Id = (Int32)c.PackHeadId } into c_join
                                                                    from c in c_join.DefaultIfEmpty()
                                                                    join d in idal.IPackScanNumberDAL.SelectAll() on new { Id = c.Id } equals new { Id = (Int32)d.PackDetailId } into d_join
                                                                    from d in d_join.DefaultIfEmpty()
                                                                    where
                                                                      a.LoadId == picktask.LoadId &&
                                                                      a.WhCode == whCode &&
                                                                      a.OutPoNumber == getoutBoundOrder.OutPoNumber &&
                                                                      (d.ScanNumber ?? "") != ""
                                                                    select d).ToList();
                List<PackScanNumberInsert> checkRFPackScanNumberList = new List<PackScanNumberInsert>();
                int count = 0;
                if (checkPackPackScanNumberList.Count > 0)
                {
                    if (item.PackScanNumber != null)
                    {
                        foreach (var item3 in item.PackScanNumber)
                        {
                            if (checkPackPackScanNumberList.Where(u => u.ScanNumber == item3.ScanNumber).Count() > 0)
                            {
                                if (count >= 2)
                                {
                                    result += "等";
                                    break;
                                }
                                else
                                {
                                    result += item3.ScanNumber + " ";
                                    count++;
                                }
                            }
                            if (checkRFPackScanNumberList.Where(u => u.ScanNumber == item3.ScanNumber).Count() > 0)
                            {
                                if (count >= 2)
                                {
                                    result1 += "等";
                                    break;
                                }
                                else
                                {
                                    result1 += item3.ScanNumber + " ";
                                    count++;
                                }
                            }
                            else
                            {
                                PackScanNumberInsert packScan = new PackScanNumberInsert();
                                packScan.ScanNumber = item3.ScanNumber;
                                checkRFPackScanNumberList.Add(packScan);
                            }
                        }
                    }
                }
                if (result + "" != "")
                {
                    result = "序列号已存在：" + result;
                }
                if (result1 + "" != "")
                {
                    result = "序列号重复：" + result1;
                }
            }
            if (result != "")
            {
                return result;
            }

            using (TransactionScope trans = new TransactionScope())
            {
                try
                {
                    //循环 开始备货 分拣 包装

                    //1.修改 备货任务数量
                    foreach (var item in entityList)
                    {
                        PickTaskDetail picktask = idal.IPickTaskDetailDAL.SelectBy(u => u.Id == item.Id).First();
                        picktask.PickQty = (picktask.PickQty ?? 0) + item.PickQty;

                        if (picktask.PickQty == picktask.Qty)
                        {
                            picktask.Status = "C";
                        }
                        else
                        {
                            picktask.Status = "A";
                        }

                        idal.IPickTaskDetailDAL.UpdateBy(picktask, u => u.Id == item.Id, new string[] { "PickQty", "Status" });
                    }

                    //2.修改Load状态 及 订单状态

                    PickTaskDetailResult pickFirst = entityList.First();

                    LoadMaster load = idal.ILoadMasterDAL.SelectBy(u => u.WhCode == whCode && u.LoadId == pickFirst.LoadId).First();
                    //-----------25 为已备货
                    FlowDetail flowDetail = idal.IFlowDetailDAL.SelectBy(u => u.FlowHeadId == load.ProcessId && u.Type == "Picking").First();

                    if (load.Status1 == "U")
                    {
                        load.Location = Location;
                        load.Status1 = "A";
                        load.Status2 = "A";
                        load.Status3 = "A";
                        load.UpdateUser = userName;
                        load.UpdateDate = DateTime.Now;
                        load.BeginPickDate = DateTime.Now;
                        load.BeginSortDate = DateTime.Now;
                        load.BeginPackDate = DateTime.Now;
                        idal.ILoadMasterDAL.UpdateBy(load, u => u.Id == load.Id, new string[] { "Status1", "UpdateUser", "UpdateDate", "BeginPickDate", "Location", "Status2", "Status3", "BeginSortDate", "BeginPackDate" });

                        //修改OutOrder状态
                        List<OutBoundOrderResult> list = (from a in idal.ILoadDetailDAL.SelectAll()
                                                          where a.LoadMasterId == load.Id
                                                          select new OutBoundOrderResult
                                                          {
                                                              Id = (Int32)a.OutBoundOrderId
                                                          }).ToList();
                        foreach (var item in list)
                        {
                            OutBoundOrder outBoundOrder = idal.IOutBoundOrderDAL.SelectBy(u => u.Id == item.Id).First();
                            if (outBoundOrder.StatusId > 15 && outBoundOrder.StatusId < 30) //订单状态必须为草稿以上
                            {
                                string remark1 = "原状态：" + outBoundOrder.StatusId + outBoundOrder.StatusName;

                                outBoundOrder.NowProcessId = flowDetail.FlowRuleId;
                                outBoundOrder.StatusId = flowDetail.StatusId;
                                outBoundOrder.StatusName = "正在备货";
                                outBoundOrder.UpdateUser = userName;
                                outBoundOrder.UpdateDate = DateTime.Now;
                                idal.IOutBoundOrderDAL.UpdateBy(outBoundOrder, u => u.Id == item.Id, new string[] { "NowProcessId", "StatusId", "StatusName", "UpdateUser", "UpdateDate" });

                                //更新订单状态，插入日志
                                TranLog tl = new TranLog();
                                tl.TranType = "32";
                                tl.Description = "更新订单状态";
                                tl.TranDate = DateTime.Now;
                                tl.TranUser = userName;
                                tl.WhCode = whCode;
                                tl.LoadId = load.LoadId;
                                tl.CustomerOutPoNumber = outBoundOrder.CustomerOutPoNumber;
                                tl.OutPoNumber = outBoundOrder.OutPoNumber;
                                tl.Remark = remark1 + "变更为：" + outBoundOrder.StatusId + outBoundOrder.StatusName;
                                idal.ITranLogDAL.Add(tl);
                            }
                        }
                    }
                    idal.IPickTaskDetailDAL.SaveChanges();

                    //系统自动取得拆托托盘
                    List<string> PutHuIdList = (from a in idal.IPallateDAL.SelectAll()
                                                join b in idal.IHuDetailDAL.SelectAll()
                                                on new { A = a.WhCode, B = a.HuId } equals new { A = b.WhCode, B = b.HuId } into temp1
                                                from b in temp1.DefaultIfEmpty()
                                                where a.WhCode == whCode && (b.HuId ?? "") == ""
                                                select a.HuId
                                      ).Take(1).ToList();

                    string PutHuId = PutHuIdList.First();

                    List<PickTaskDetail> checkHuList = new List<PickTaskDetail>();
                    //验证是否有托盘 完成备货
                    foreach (var picktaskDetail in entityList)
                    {
                        PickTaskDetail picktask = idal.IPickTaskDetailDAL.SelectBy(u => u.Id == picktaskDetail.Id).First();
                        List<PickTaskDetail> list = idal.IPickTaskDetailDAL.SelectBy(u => u.LoadId == picktask.LoadId && u.WhCode == whCode && u.HuId == picktask.HuId && u.PickQty != u.Qty);

                        //如果有托盘 备货数量等于已备货数量
                        //验证该备货托盘 是否全部款号均完成备货，如果完成备货 执行备货装箱
                        if (list.Count == 0)
                        {

                            #region -----------------------------备货及装箱-----------------------------------

                            //防止多次对同一托盘进行装箱
                            if (checkHuList.Where(u => u.HuId == picktask.HuId).Count() == 0)
                            {
                                PickTaskDetail checkHu = new PickTaskDetail();
                                checkHu.HuId = picktask.HuId;
                                checkHuList.Add(checkHu);

                                string LoadId = picktask.LoadId;
                                string HuId = picktask.HuId;

                                string mess = "";   //是否是拆托 N是拆托 Y不是拆托

                                //执行装箱拆托 
                                List<PickTaskDetail> pickList = idal.IPickTaskDetailDAL.SelectBy(u => u.WhCode == whCode && u.LoadId == LoadId && u.HuId == HuId);

                                List<HuDetail> huDetailList = idal.IHuDetailDAL.SelectBy(u => u.WhCode == whCode && u.HuId == HuId);

                                int pickQty = pickList.Sum(u => u.Qty);
                                int inventoryQty = huDetailList.Sum(u => u.Qty);

                                if (pickQty == inventoryQty)
                                {
                                    foreach (var item in pickList)
                                    {
                                        if (mess == "")
                                        {
                                            HuDetail huDetail = huDetailList.Where(u => u.Id == item.HuDetailId).First();
                                            if (huDetail.Qty > item.Qty)
                                            {
                                                mess = "N";
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    mess = "N";
                                }
                                if (mess == "")
                                {
                                    mess = "Y";
                                }

                                //如果是拆托
                                if (mess == "N")
                                {
                                    int count = 0;
                                    foreach (var item in pickList)
                                    {
                                        //1.如果是拆托 验证拆托托盘是否存在
                                        HuDetail huDetail = huDetailList.Where(u => u.Id == item.HuDetailId).First();
                                        if (count == 0)
                                        {
                                            HuMaster huMaster = new HuMaster();
                                            huMaster.WhCode = whCode;
                                            huMaster.HuId = PutHuId;
                                            huMaster.LoadId = LoadId;
                                            huMaster.Type = "M";
                                            huMaster.Status = "A";
                                            huMaster.Location = Location;
                                            huMaster.ReceiptId = huDetail.ReceiptId;
                                            huMaster.ReceiptDate = DateTime.Now;
                                            huMaster.TransactionFlag = 1;
                                            huMaster.CreateUser = userName;
                                            huMaster.CreateDate = DateTime.Now;
                                            idal.IHuMasterDAL.Add(huMaster);
                                        }
                                        //1.1加入拆托明细 锁定数量为0
                                        HuDetail entity = new HuDetail();
                                        entity.WhCode = whCode;
                                        entity.HuId = PutHuId;
                                        entity.ClientId = huDetail.ClientId;
                                        entity.ClientCode = huDetail.ClientCode;
                                        entity.SoNumber = huDetail.SoNumber;
                                        entity.CustomerPoNumber = huDetail.CustomerPoNumber;
                                        entity.AltItemNumber = huDetail.AltItemNumber;
                                        entity.ItemId = huDetail.ItemId;
                                        entity.UnitId = huDetail.UnitId;
                                        entity.UnitName = huDetail.UnitName;
                                        entity.ReceiptId = huDetail.ReceiptId;
                                        entity.Qty = item.Qty;
                                        entity.PlanQty = 0;
                                        entity.ReceiptDate = DateTime.Now;
                                        entity.Length = huDetail.Length;
                                        entity.Width = huDetail.Width;
                                        entity.Height = huDetail.Height;
                                        entity.Weight = huDetail.Weight;
                                        entity.LotNumber1 = huDetail.LotNumber1;
                                        entity.LotNumber2 = huDetail.LotNumber2;
                                        entity.LotDate = huDetail.LotDate;
                                        entity.CreateUser = userName;
                                        entity.CreateDate = DateTime.Now;
                                        idal.IHuDetailDAL.Add(entity);

                                        //1.2 插入备货记录
                                        AddPickingTranLog1(huDetail, userName, whCode, item.Location, "", LoadId, PutHuId, item.Qty);

                                        //1.3 修改原托盘数量
                                        HuDetail entity1 = new HuDetail();
                                        entity1.Qty = huDetail.Qty - item.Qty;
                                        entity1.PlanQty = huDetail.PlanQty - item.Qty;
                                        entity1.UpdateUser = userName;
                                        entity1.UpdateDate = DateTime.Now;
                                        if (entity1.Qty == 0)
                                        {
                                            idal.IHuDetailDAL.DeleteBy(u => u.Id == huDetail.Id);

                                            //1.3 插入备货删除记录
                                            AddPickingTranLog2(huDetail, userName, whCode, item.Location, "", LoadId, PutHuId, item.Qty);
                                        }
                                        else
                                        {
                                            idal.IHuDetailDAL.UpdateBy(entity1, u => u.Id == huDetail.Id, new string[] { "Qty", "PlanQty", "UpdateUser", "UpdateDate" });
                                        }
                                        //1.2 插入备货记录 
                                        AddPickingTranLog(entity, userName, whCode, item.Location, Location, LoadId, "");
                                        count++;
                                    }

                                    //1.3 修改备货任务托盘
                                    PickTaskDetail pick = new PickTaskDetail();
                                    pick.HuId = PutHuId;
                                    pick.UpdateUser = userName;
                                    pick.UpdateDate = DateTime.Now;
                                    idal.IPickTaskDetailDAL.UpdateBy(pick, u => u.LoadId == LoadId && u.WhCode == whCode && u.HuId == HuId, new string[] { "HuId", "UpdateUser", "UpdateDate" });

                                    //1.4 修改原托盘拆托Flag为1
                                    HuMaster huma = new HuMaster();
                                    huma.TransactionFlag = 1;
                                    huma.UpdateUser = userName;
                                    huma.UpdateDate = DateTime.Now;
                                    idal.IHuMasterDAL.UpdateBy(huma, u => u.WhCode == whCode && u.HuId == HuId, new string[] { "TransactionFlag", "UpdateUser", "UpdateDate" });

                                }
                                else if (mess == "Y")   //整出
                                {
                                    HuMaster huMaster = idal.IHuMasterDAL.SelectBy(u => u.WhCode == whCode && u.HuId == HuId).First();
                                    //1.1 插入备货记录
                                    foreach (var item in huDetailList)
                                    {
                                        AddPickingTranLog(item, userName, whCode, huMaster.Location, Location, LoadId, "");
                                    }

                                    //1.2 修改托盘信息
                                    huMaster.LoadId = LoadId;
                                    huMaster.Location = Location;
                                    huMaster.UpdateUser = userName;
                                    huMaster.UpdateDate = DateTime.Now;
                                    idal.IHuMasterDAL.UpdateBy(huMaster, u => u.Id == huMaster.Id, new string[] { "LoadId", "Location", "UpdateUser", "UpdateDate" });

                                    HuDetail huDetail = new HuDetail();
                                    huDetail.PlanQty = 0;
                                    huDetail.UpdateUser = userName;
                                    huDetail.UpdateDate = DateTime.Now;
                                    idal.IHuDetailDAL.UpdateBy(huDetail, u => u.WhCode == huMaster.WhCode && u.HuId == huMaster.HuId, new string[] { "PlanQty", "UpdateUser", "UpdateDate" });
                                }

                                idal.IHuDetailDAL.SaveChanges();

                                string result1 = "";
                                //装箱
                                if (mess == "N")
                                    result1 = PackingLoad(LoadId, whCode, PutHuId, userName);
                                else if (mess == "Y")
                                    result1 = PackingLoad(LoadId, whCode, HuId, userName);
                                if (result1 != "Y")
                                {
                                    trans.Dispose();//出现异常，事务手动释放
                                    return "备货异常,请重新提交！";
                                }
                            }
                            #endregion
                        }

                        //增加分拣表备货数量、分拣数量、 插入包装任务等

                        //----------------begin 
                        //备货时 同时插入分拣任务表中的PickQty (备货数量)  Qty (分拣数量)
                        //1.首先 根据备货任务表的款号等 查询出 分拣任务表的对应数据

                        OutBoundOrder outBoundOrder = idal.IOutBoundOrderDAL.SelectBy(u => u.Id == picktask.OutBoundOrderId).First();

                        //如果检查到流程中有分拣流程 
                        List<FlowDetail> checkSortingFlowDetailList = idal.IFlowDetailDAL.SelectBy(u => u.FlowHeadId == outBoundOrder.ProcessId && u.Type == "Sorting");

                        string sortingMark = "";
                        if (checkSortingFlowDetailList.Count > 0)
                        {
                            sortingMark = checkSortingFlowDetailList.First().Mark;
                        }

                        List<SortTaskDetail> sortTaskDetailList = idal.ISortTaskDetailDAL.SelectBy(u => u.AltItemNumber == picktask.AltItemNumber && u.ItemId == picktask.ItemId && u.LoadId == picktask.LoadId && u.WhCode == whCode && u.PlanQty != u.PickQty && u.OutPoNumber == outBoundOrder.OutPoNumber).OrderBy(u => u.GroupId).ToList();

                        int? sortGroupId = 0;
                        int resultQty = (Int32)picktaskDetail.PickQty;
                        foreach (var detail in sortTaskDetailList)
                        {
                            if (detail.PickQty + resultQty > detail.PlanQty)
                            {
                                resultQty = resultQty - (detail.PlanQty - (Int32)detail.PickQty);

                                SortTaskDetail entity = new SortTaskDetail();

                                if (sortingMark == "1")
                                {
                                    entity.GroupNumber = detail.LoadId + detail.GroupId.ToString();
                                }
                                else if (sortingMark == "2")
                                {
                                    entity.GroupNumber = outBoundOrder.CustomerOutPoNumber;
                                }
                                else if (sortingMark == "3")
                                {
                                    entity.GroupNumber = outBoundOrder.AltCustomerOutPoNumber;
                                }
                                else
                                {
                                    entity.GroupNumber = detail.LoadId + detail.GroupId.ToString();
                                }

                                entity.PickQty = detail.PlanQty;
                                entity.Qty = (Int32)detail.PlanQty;
                                entity.UpdateUser = userName;
                                entity.UpdateDate = DateTime.Now;
                                idal.ISortTaskDetailDAL.UpdateBy(entity, u => u.Id == detail.Id, new string[] { "GroupNumber", "PickQty", "Qty", "UpdateUser", "UpdateDate" });
                                sortGroupId = detail.GroupId;
                                continue;
                            }
                            else
                            {
                                SortTaskDetail entity = new SortTaskDetail();

                                if (sortingMark == "1")
                                {
                                    entity.GroupNumber = detail.LoadId + detail.GroupId.ToString();
                                }
                                else if (sortingMark == "2")
                                {
                                    entity.GroupNumber = outBoundOrder.CustomerOutPoNumber;
                                }
                                else if (sortingMark == "3")
                                {
                                    entity.GroupNumber = outBoundOrder.AltCustomerOutPoNumber;
                                }
                                else
                                {
                                    entity.GroupNumber = detail.LoadId + detail.GroupId.ToString();
                                }

                                entity.PickQty = detail.PickQty + resultQty;
                                entity.Qty = (Int32)detail.PickQty + resultQty;
                                entity.UpdateUser = userName;
                                entity.UpdateDate = DateTime.Now;
                                idal.ISortTaskDetailDAL.UpdateBy(entity, u => u.Id == detail.Id, new string[] { "GroupNumber", "PickQty", "Qty", "UpdateUser", "UpdateDate" });
                                sortGroupId = detail.GroupId;
                                break;
                            }
                        }
                        //--------------- end
                        idal.ISortTaskDetailDAL.SaveChanges();

                        //插入包装任务信息
                        //如果检查到没有包装任务 添加包装任务

                        List<PackTask> checkPackTaskList = idal.IPackTaskDAL.SelectBy(u => u.WhCode == whCode && u.LoadId == pickFirst.LoadId && u.CustomerOutPoNumber == outBoundOrder.CustomerOutPoNumber && u.OutPoNumber == outBoundOrder.OutPoNumber);

                        PackTask packTask = new PackTask();
                        if (checkPackTaskList.Count == 0)
                        {
                            List<PackTaskJson> getPackTaskJson = idal.IPackTaskJsonDAL.SelectBy(u => u.WhCode == whCode && u.CustomerOutPoNumber == outBoundOrder.CustomerOutPoNumber);
                            if (getPackTaskJson.Count == 0)
                            {
                                packTask.WhCode = whCode;
                                packTask.LoadId = pickFirst.LoadId;
                                packTask.SortGroupId = sortGroupId; //分拣组号

                                if (sortingMark == "1")
                                {
                                    packTask.SortGroupNumber = pickFirst.LoadId + sortGroupId.ToString(); //分拣框号
                                }
                                else if (sortingMark == "2")
                                {
                                    packTask.SortGroupNumber = outBoundOrder.CustomerOutPoNumber;
                                }
                                else if (sortingMark == "3")
                                {
                                    packTask.SortGroupNumber = outBoundOrder.AltCustomerOutPoNumber;
                                }
                                else
                                {
                                    packTask.SortGroupNumber = pickFirst.LoadId + sortGroupId.ToString(); //分拣框号
                                }

                                packTask.CustomerOutPoNumber = outBoundOrder.CustomerOutPoNumber;  //客户出库订单号
                                packTask.OutPoNumber = outBoundOrder.OutPoNumber;      //系统出库订单号
                                packTask.Status = 0;                        //状态为-10：订单被拦截
                                packTask.CreateUser = userName;
                                packTask.CreateDate = DateTime.Now;
                            }
                            else
                            {
                                PackTaskJson packJson = getPackTaskJson.First();

                                PackTaskJsonEntity packTaskJsonEntity = JsonConvert.DeserializeObject<PackTaskJsonEntity>(packJson.Json);

                                packTask.TransportType = packTaskJsonEntity.TransportType;
                                packTask.express_code = packTaskJsonEntity.express_code;
                                packTask.express_type = packTaskJsonEntity.express_type;
                                packTask.express_type_zh = packTaskJsonEntity.express_type_zh;
                                packTask.SingleFlag = packTaskJsonEntity.SingleFlag;

                                if (packTask.SingleFlag == 1)
                                {
                                    packTask.PackQty = 1;
                                }
                                packTask.SinglePlaneTemplate = packTaskJsonEntity.SinglePlaneTemplate;
                                packTask.PackingListTemplate = packTaskJsonEntity.PackingListTemplate;

                                packTask.WhCode = whCode;
                                packTask.LoadId = pickFirst.LoadId;
                                packTask.SortGroupId = sortGroupId; //分拣组号

                                if (sortingMark == "1")
                                {
                                    packTask.SortGroupNumber = pickFirst.LoadId + sortGroupId.ToString(); //分拣框号
                                }
                                else if (sortingMark == "2")
                                {
                                    packTask.SortGroupNumber = outBoundOrder.CustomerOutPoNumber;
                                }
                                else if (sortingMark == "3")
                                {
                                    packTask.SortGroupNumber = outBoundOrder.AltCustomerOutPoNumber;
                                }
                                else
                                {
                                    packTask.SortGroupNumber = pickFirst.LoadId + sortGroupId.ToString(); //分拣框号
                                }

                                packTask.CustomerOutPoNumber = outBoundOrder.CustomerOutPoNumber;  //客户出库订单号
                                packTask.OutPoNumber = outBoundOrder.OutPoNumber;      //系统出库订单号      
                                packTask.Status = 10;                        //状态为-10：订单被拦截
                                packTask.CreateUser = userName;
                                packTask.CreateDate = DateTime.Now;
                            }

                            idal.IPackTaskDAL.Add(packTask);
                            idal.IPackTaskDAL.SaveChanges();
                        }
                    }

                    //检查是否全部完成备货
                    List<PickTaskDetail> sql4 = (from a in idal.IPickTaskDetailDAL.SelectAll()
                                                 where a.LoadId == pickFirst.LoadId && a.WhCode == whCode && a.Status == "U" || a.Status == "A"
                                                 select a).ToList();
                    if (sql4.Count() == 0)
                    {
                        load.Status1 = "C";
                        load.Status2 = "C";
                        load.Status3 = "C";
                        load.UpdateUser = userName;
                        load.UpdateDate = DateTime.Now;
                        load.EndPickDate = DateTime.Now;
                        load.EndSortDate = DateTime.Now;
                        idal.ILoadMasterDAL.UpdateBy(load, u => u.Id == load.Id, new string[] { "Status1", "Status2", "Status3", "UpdateUser", "UpdateDate", "EndPickDate", "EndSortDate" });
                    }

                    idal.ILoadMasterDAL.SaveChanges();
                    trans.Complete();
                    return "Y";
                }
                catch
                {
                    trans.Dispose();
                    return "备货异常，请重新提交！";
                }
            }
        }


        #region 插入备货TranLog 和 备货工作量
        public void AddPickingTranLog(HuDetail huDetail, string userName, string whCode, string location, string putLocation, string loadId, string huId2)
        {
            TranLog tl = new TranLog();
            tl.TranType = "203";
            tl.Description = "备货";
            tl.TranDate = DateTime.Now;
            tl.TranUser = userName;
            tl.WhCode = huDetail.WhCode;
            tl.ClientCode = huDetail.ClientCode;
            tl.SoNumber = huDetail.SoNumber;
            tl.CustomerPoNumber = huDetail.CustomerPoNumber;
            tl.AltItemNumber = huDetail.AltItemNumber;
            tl.ItemId = huDetail.ItemId;
            tl.UnitID = huDetail.UnitId;
            tl.UnitName = huDetail.UnitName;
            tl.TranQty = huDetail.Qty;
            tl.TranQty2 = huDetail.PlanQty;
            tl.HuId = huDetail.HuId;
            tl.HuId2 = huId2;
            tl.Length = huDetail.Length;
            tl.Width = huDetail.Width;
            tl.Height = huDetail.Height;
            tl.Weight = huDetail.Weight;
            tl.LotNumber1 = huDetail.LotNumber1;
            tl.LotNumber2 = huDetail.LotNumber2;
            tl.LotDate = huDetail.LotDate;
            tl.ReceiptId = huDetail.ReceiptId;
            tl.ReceiptDate = huDetail.ReceiptDate;
            tl.Location = location;
            tl.Location2 = putLocation;
            tl.LoadId = loadId;

            //WorkloadAccount work = new WorkloadAccount();
            //work.WhCode = whCode;
            //work.ClientId = huDetail.ClientId;
            //work.ClientCode = huDetail.ClientCode;
            //work.LoadId = loadId;
            //work.HuId = huDetail.HuId;
            //work.WorkType = "叉车工";
            //work.UserCode = userName;
            //work.LotFlag = 0;
            //work.EchFlag = (huDetail.UnitName == "ECH" ? 1 : 0);
            //work.Qty = (Int32)tl.TranQty;
            //work.CBM = tl.Length * tl.Width * tl.Height * tl.TranQty;
            //work.Weight = tl.Weight;
            //work.ReceiptDate = DateTime.Now;
            //idal.IWorkloadAccountDAL.Add(work);

            //WorkloadAccount work1 = new WorkloadAccount();
            //work1.WhCode = whCode;
            //work1.LoadId = loadId;
            //work1.HuId = huDetail.HuId;
            //work1.WorkType = "理货员";
            //work1.UserCode = userName;
            //work1.LotFlag = 0;
            //work1.EchFlag = 0;
            //work1.Qty = (Int32)tl.TranQty;
            //work1.CBM = tl.Length * tl.Width * tl.Height * tl.TranQty;
            //work1.Weight = tl.Weight;
            //work1.ReceiptDate = DateTime.Now;
            //idal.IWorkloadAccountDAL.Add(work1);

            idal.ITranLogDAL.Add(tl);
        }


        public void AddPickingTranLog1(HuDetail huDetail, string userName, string whCode, string location, string putLocation, string loadId, string huId2, int jiaoyiQty)
        {
            TranLog tl = new TranLog();
            tl.TranType = "202";
            tl.Description = "备货拆托";
            tl.TranDate = DateTime.Now;
            tl.TranUser = userName;
            tl.WhCode = huDetail.WhCode;
            tl.ClientCode = huDetail.ClientCode;
            tl.SoNumber = huDetail.SoNumber;
            tl.CustomerPoNumber = huDetail.CustomerPoNumber;
            tl.AltItemNumber = huDetail.AltItemNumber;
            tl.ItemId = huDetail.ItemId;
            tl.UnitID = huDetail.UnitId;
            tl.UnitName = huDetail.UnitName;
            tl.TranQty = huDetail.Qty;
            tl.TranQty2 = jiaoyiQty;
            tl.HuId = huDetail.HuId;
            tl.HuId2 = huId2;
            tl.Length = huDetail.Length;
            tl.Width = huDetail.Width;
            tl.Height = huDetail.Height;
            tl.Weight = huDetail.Weight;
            tl.LotNumber1 = huDetail.LotNumber1;
            tl.LotNumber2 = huDetail.LotNumber2;
            tl.LotDate = huDetail.LotDate;
            tl.ReceiptId = huDetail.ReceiptId;
            tl.ReceiptDate = huDetail.ReceiptDate;
            tl.Location = location;
            tl.Location2 = putLocation;
            tl.LoadId = loadId;
            idal.ITranLogDAL.Add(tl);
        }

        public void AddPickingTranLog2(HuDetail huDetail, string userName, string whCode, string location, string putLocation, string loadId, string huId2, int jiaoyiQty)
        {
            TranLog tl = new TranLog();
            tl.TranType = "203";
            tl.Description = "备货拆托删除";
            tl.TranDate = DateTime.Now;
            tl.TranUser = userName;
            tl.WhCode = huDetail.WhCode;
            tl.ClientCode = huDetail.ClientCode;
            tl.SoNumber = huDetail.SoNumber;
            tl.CustomerPoNumber = huDetail.CustomerPoNumber;
            tl.AltItemNumber = huDetail.AltItemNumber;
            tl.ItemId = huDetail.ItemId;
            tl.UnitID = huDetail.UnitId;
            tl.UnitName = huDetail.UnitName;
            tl.TranQty = huDetail.Qty;
            tl.TranQty2 = jiaoyiQty;
            tl.HuId = huDetail.HuId;
            tl.HuId2 = huId2;
            tl.Length = huDetail.Length;
            tl.Width = huDetail.Width;
            tl.Height = huDetail.Height;
            tl.Weight = huDetail.Weight;
            tl.LotNumber1 = huDetail.LotNumber1;
            tl.LotNumber2 = huDetail.LotNumber2;
            tl.LotDate = huDetail.LotDate;
            tl.ReceiptId = huDetail.ReceiptId;
            tl.ReceiptDate = huDetail.ReceiptDate;
            tl.Location = location;
            tl.Location2 = putLocation;
            tl.LoadId = loadId;
            idal.ITranLogDAL.Add(tl);
        }

        #endregion


        //装箱插入装卸工 工作量
        public string PackingLoad(string LoadId, string whCode, string huId, string Position, string userName, List<WorkloadAccountModel> WorkloadAccountModel, List<HuDetailRemained> HuDetailRemained)
        {
            using (TransactionScope trans = new TransactionScope())
            {
                try
                {
                    #region 装箱优化

                    List<HuDetail> HuDetailList = idal.IHuDetailDAL.SelectBy(u => u.WhCode == whCode && u.HuId == huId);

                    if (HuDetailList.Count == 0)
                        return "托盘库存不存在,装箱失败！";

                    HuDetail getHuDetailClient = HuDetailList.First();
                    //需要添加扫描序列号
                    if (HuDetailRemained != null)
                    {
                        //var  aaa= from a in  HuDetailRemained[0].SerialNumberModel
                        List<string> cartonIds = new List<string>();
                        foreach (var item in HuDetailRemained)
                        {
                            foreach (var item0 in item.SerialNumberModel)
                            {
                                cartonIds.Add(item0.CartonId);
                            }
                        }
                        List<string> reCounts = idal.ISerialNumberOutDAL.SelectBy(u => u.LoadId == LoadId && u.WhCode == whCode && cartonIds.Contains(u.CartonId)).Select(u => u.CartonId).ToList();

                        if (reCounts.Count > 0)
                            return reCounts[0] + "等重复！";
                        var HuDetailListRemained = from a in HuDetailList
                                                   join b in HuDetailRemained on a.Id equals b.Id
                                                   select new
                                                   {
                                                       huDetailId = a.Id,
                                                       WhCode = a.WhCode,
                                                       ClientId = a.ClientId,
                                                       ClientCode = a.ClientCode,
                                                       SoNumber = a.SoNumber,
                                                       CustomerPoNumber = a.CustomerPoNumber,
                                                       AltItemNumber = a.AltItemNumber,
                                                       ItemId = a.ItemId,
                                                       HuId = a.HuId,
                                                       Length = a.Length,
                                                       Width = a.Width,
                                                       Height = a.Height,
                                                       Weight = a.Weight,
                                                       LotNumber1 = a.LotNumber1,
                                                       LotNumber2 = a.LotNumber2,
                                                       LotDate = a.LotDate,
                                                       SerialNumberModel = b.SerialNumberModel
                                                   };
                        List<SerialNumberOut> serialNumberOutList = new List<SerialNumberOut>();
                        foreach (var huDetail in HuDetailListRemained)
                        {
                            foreach (var item in huDetail.SerialNumberModel)
                            {
                                //插入采集箱号表
                                SerialNumberOut serial = new SerialNumberOut();
                                serial.WhCode = huDetail.WhCode;
                                serial.ClientId = huDetail.ClientId;
                                serial.LoadId = LoadId;
                                serial.ClientCode = huDetail.ClientCode;
                                serial.SoNumber = huDetail.SoNumber;
                                serial.CustomerPoNumber = huDetail.CustomerPoNumber;
                                serial.AltItemNumber = huDetail.AltItemNumber;
                                serial.ItemId = huDetail.ItemId;
                                serial.HuId = huDetail.HuId;
                                serial.Length = huDetail.Length;
                                serial.Width = huDetail.Width;
                                serial.Height = huDetail.Height;
                                serial.Weight = huDetail.Weight;
                                serial.LotNumber1 = huDetail.LotNumber1;
                                serial.LotNumber2 = huDetail.LotNumber2;
                                serial.LotDate = huDetail.LotDate;
                                serial.CreateUser = userName;
                                serial.CreateDate = DateTime.Now;
                                serial.CartonId = item.CartonId;
                                serialNumberOutList.Add(serial);

                                ////删除关系表 暂时不启用
                                //idal.IR_SerialNumberInOutDAL.DeleteBy(u => u.WhCode == huDetail.WhCode
                                //&& u.ClientCode == huDetail.ClientCode && u.SoNumber == huDetail.SoNumber
                                //&& u.CustomerPoNumber == u.CustomerPoNumber && u.ItemId == huDetail.ItemId
                                //&& u.CartonId == item.CartonId
                                //);
                            }
                        }

                        //批量添加扫描
                        idal.ISerialNumberOutDAL.Add(serialNumberOutList);

                    }
                    //装箱位置
                    if (!string.IsNullOrEmpty(Position))
                    {

                        PickTaskDetail editPosition = new PickTaskDetail();
                        editPosition.Position = Position;
                        idal.IPickTaskDetailDAL.UpdateBy(editPosition, u => u.WhCode == whCode && u.LoadId == LoadId && u.HuId == huId, new string[] { "Position" });


                        List<PickTaskDetail> pickTaskDetailList = idal.IPickTaskDetailDAL.SelectBy(u => u.LoadId == LoadId && u.WhCode == whCode && u.HuId == huId);
                        foreach (PickTaskDetail item in pickTaskDetailList)
                        {
                            OutBoundOrderDetail editOrderDetailPosition = new OutBoundOrderDetail();
                            editOrderDetailPosition.StowPosition = Position;
                            idal.IOutBoundOrderDetailDAL.UpdateBy(editOrderDetailPosition,
                                    u => u.OutBoundOrderId == item.OutBoundOrderId && u.SoNumber == item.SoNumber && u.CustomerPoNumber == item.CustomerPoNumber
                                    && u.ItemId == item.ItemId && u.UnitName == item.UnitName
                                    , new string[] { "StowPosition" });
                        }


                    }


                    string result = PackingLoad(LoadId, whCode, huId, userName);
                    if (result != "Y")
                    {
                        return result;
                    }

                    decimal? qty = 0;
                    decimal? cbm = 0;
                    decimal? weight = 0;
                    int EchFlag = 0;
                    foreach (var huDetail in HuDetailList)
                    {
                        qty = qty + huDetail.Qty;    //总数量
                        cbm = cbm + huDetail.Qty * huDetail.Length * huDetail.Height * huDetail.Width;    //总体积
                        weight = weight + huDetail.Weight * huDetail.Qty; //总重量
                        if (huDetail.UnitName.Contains("ECH"))
                            EchFlag = 1;

                    }

                    //插入理货员工作量
                    WorkloadAccount workl = new WorkloadAccount();
                    workl.WhCode = whCode;
                    workl.ClientId = getHuDetailClient.ClientId;
                    workl.ClientCode = getHuDetailClient.ClientCode;
                    workl.LoadId = LoadId;
                    workl.HuId = huId;
                    workl.WorkType = "理货员";
                    workl.UserCode = userName;
                    workl.LotFlag = 0;
                    workl.EchFlag = EchFlag;
                    workl.CBM = cbm;
                    workl.Qty = Convert.ToDecimal(qty);
                    workl.Weight = weight;
                    workl.ReceiptDate = DateTime.Now;
                    idal.IWorkloadAccountDAL.Add(workl);

                    if (WorkloadAccountModel != null)
                    {
                        //先得到工种
                        List<string> list = WorkloadAccountModel.Select(u => u.WorkType).Distinct().ToList();

                        Hashtable hs = new Hashtable();
                        for (int i = 0; i < list.Count; i++)
                        {
                            string workType = list[i].ToString();
                            int workCount = WorkloadAccountModel.Where(u => u.WorkType == workType).Count();
                            hs.Add(workType, workCount);    //把工种做为唯一键
                        }

                        //插入工人工作量表
                        foreach (var workItem in WorkloadAccountModel)
                        {
                            WorkloadAccount work = new WorkloadAccount();
                            work.WhCode = whCode;
                            work.ClientId = getHuDetailClient.ClientId;
                            work.ClientCode = getHuDetailClient.ClientCode;
                            work.LoadId = LoadId;
                            work.HuId = huId;
                            work.WorkType = workItem.WorkType;
                            work.UserCode = workItem.UserCode;
                            work.LotFlag = 0;
                            work.EchFlag = EchFlag;
                            work.ReceiptDate = DateTime.Now;

                            if (hs.ContainsKey(workItem.WorkType))
                            {
                                int count = Convert.ToInt32(hs[workItem.WorkType]);
                                decimal? avgCbm = cbm / count;
                                decimal? avgQty = qty / count;
                                decimal? avgWeight = weight / count;
                                work.CBM = avgCbm;
                                work.Qty = Convert.ToDecimal(avgQty);
                                work.Weight = avgWeight;
                            }
                            idal.IWorkloadAccountDAL.Add(work);
                        }
                    }

                    #endregion

                    idal.SaveChanges();
                    trans.Complete();
                    return "Y";
                }
                catch
                {
                    trans.Dispose();//出现异常，事务手动释放
                    return "装箱异常，请重新提交！";
                }
            }
        }

        public string PackingLoadWork(string LoadId, string whCode, string huId, string userName, List<WorkloadAccountModel> WorkloadAccountModel, List<HuDetail> HuDetailList)
        {

            //先得到工种
            List<string> list = WorkloadAccountModel.Select(u => u.WorkType).Distinct().ToList();

            Hashtable hs = new Hashtable();
            for (int i = 0; i < list.Count; i++)
            {
                string workType = list[i].ToString();
                int workCount = WorkloadAccountModel.Where(u => u.WorkType == workType).Count();
                hs.Add(workType, workCount);    //把工种做为唯一键
            }

            HuDetail huDetail = HuDetailList.First();

            decimal? qty = huDetail.Qty;    //总数量
            decimal? cbm = huDetail.Qty * (huDetail.Length / 100) * (huDetail.Height / 100) * (huDetail.Width / 100);    //总体积
            decimal? weight = huDetail.Weight; //总重量

            //插入工人工作量表
            foreach (var workItem in WorkloadAccountModel)
            {
                WorkloadAccount work = new WorkloadAccount();
                work.WhCode = whCode;
                work.ClientId = huDetail.ClientId;
                work.ClientCode = huDetail.ClientCode;
                work.LoadId = LoadId;
                work.HuId = huId;
                work.WorkType = workItem.WorkType;
                work.UserCode = workItem.UserCode;
                work.LotFlag = 0;
                work.EchFlag = 0;
                work.ReceiptDate = DateTime.Now;

                if (hs.ContainsKey(workItem.WorkType))
                {
                    int count = Convert.ToInt32(hs[workItem.WorkType]);
                    decimal? avgCbm = qty / count;
                    decimal? avgQty = cbm / count;
                    decimal? avgWeight = weight / count;
                    work.CBM = avgCbm;
                    work.Qty = Convert.ToDecimal(avgQty);
                    work.Weight = avgWeight;
                }
                idal.IWorkloadAccountDAL.Add(work);
            }
            return "";
        }

        public string ShippingLoadAddEchBa(string LoadId, string whCode, int Ba)
        {

            List<WorkloadAccount> WorkloadAccountL = idal.IWorkloadAccountDAL.SelectBy(u => u.WhCode == whCode && u.LoadId == LoadId);

            LoadMaster loadMaster = new LoadMaster();
            loadMaster.BaQty = Ba;
            idal.ILoadMasterDAL.UpdateBy(loadMaster, u => u.LoadId == LoadId && u.WhCode == whCode, new string[] { "BaQty" });

            WorkloadAccount Lh0 = WorkloadAccountL.Where(u => u.WorkType == "理货员").First();

            if (WorkloadAccountL.Where(u => u.WorkType == "理货员").Count() > 0)
            {
                WorkloadAccount Lh = new WorkloadAccount();
                Lh.WhCode = Lh0.WhCode;
                Lh.ClientId = Lh0.ClientId;
                Lh.ClientCode = Lh0.ClientCode;
                Lh.LoadId = Lh0.LoadId;
                Lh.UserCode = Lh0.UserCode;
                Lh.LotFlag = Lh0.LotFlag;
                Lh.WorkType = Lh0.WorkType;
                Lh.Qty = Lh0.Qty;
                Lh.BaQty = Ba;
                Lh.HuId = "";
                Lh.CBM = 0;
                Lh.EchFlag = 1;
                Lh.Weight = 0;
                Lh.ReceiptDate = DateTime.Now;
                idal.IWorkloadAccountDAL.Add(Lh);
            }

            if (WorkloadAccountL.Where(u => u.WorkType == "装卸工").Count() > 0)
            {
                List<string> ZXL = new List<string>();
                foreach (var item in WorkloadAccountL.Where(u => u.WorkType == "装卸工"))
                {
                    string zxworkcode = item.UserCode;
                    if (!ZXL.Contains(zxworkcode))
                    {
                        ZXL.Add(zxworkcode);

                    }

                }

                int workcount = ZXL.Count();
                decimal danba = Convert.ToDecimal(Ba) / workcount;


                foreach (var item in ZXL)
                {
                    WorkloadAccount Lh = new WorkloadAccount();
                    Lh.WhCode = Lh0.WhCode;
                    Lh.ClientId = Lh0.ClientId;
                    Lh.ClientCode = Lh0.ClientCode;
                    Lh.LoadId = Lh0.LoadId;
                    Lh.LotFlag = Lh0.LotFlag;
                    Lh.Qty = Lh0.Qty;
                    Lh.HuId = "";
                    Lh.WorkType = "装卸工";
                    Lh.EchFlag = 1;
                    Lh.CBM = 0;
                    Lh.Weight = 0;
                    Lh.UserCode = item;
                    Lh.BaQty = danba;
                    Lh.ReceiptDate = DateTime.Now;
                    idal.IWorkloadAccountDAL.Add(Lh);
                }
            }

            idal.SaveChanges();
            return "";
        }

        //装箱
        public string PackingLoad(string LoadId, string whCode, string huId, string userName)
        {
            using (TransactionScope trans = new TransactionScope())
            {
                try
                {
                    #region 装箱优化
                    if (LoadId == null || LoadId == "" || whCode == "" || whCode == null || huId == null || huId == "" || userName == null || userName == "")
                    {
                        return "数据有误，请重新操作！";
                    }
                    List<PickTaskDetail> sql = (from a in idal.IPickTaskDetailDAL.SelectAll()
                                                where a.WhCode == whCode && a.HuId == huId && a.LoadId == LoadId && a.Status == "C" && a.Status1 == "U"
                                                select a).ToList();
                    if (sql.Count == 0)
                    {
                        return "该Load:" + LoadId + "对应的托盘" + huId + "不存在或状态有误！";
                    }

                    List<HuMaster> huMasterList = (from a in idal.IHuMasterDAL.SelectAll()
                                                   where (a.HuId == LoadId || a.HuId == huId) && a.WhCode == whCode
                                                   select a).ToList();
                    if (huMasterList.Where(u => u.HuId == LoadId && u.WhCode == whCode).Count() == 0)
                    {
                        HuMaster huMaster = new HuMaster();
                        string loc = huMasterList.Where(u => u.HuId == huId && u.WhCode == whCode).First().Location;
                        huMaster.WhCode = whCode;
                        huMaster.HuId = LoadId;
                        huMaster.Type = "O";
                        huMaster.Status = "A";
                        huMaster.Location = loc;
                        huMaster.ReceiptDate = DateTime.Now;
                        huMaster.LoadId = LoadId;
                        huMaster.CreateUser = userName;
                        huMaster.CreateDate = DateTime.Now;
                        idal.IHuMasterDAL.Add(huMaster);
                    }

                    List<HuDetail> List = idal.IHuDetailDAL.SelectBy(u => u.WhCode == whCode && (u.HuId == huId || u.HuId == LoadId));

                    List<HuDetail> huDetailList = List.Where(u => u.HuId == huId && u.WhCode == whCode).ToList();
                    if (huDetailList.Count == 0)
                    {
                        return "该Load:" + LoadId + "对应的托盘" + huId + "库存不存在！";
                    }

                    List<FlowDetail> getFlowDetailList = (from a in idal.ILoadMasterDAL.SelectAll()
                                                          join b in idal.IFlowDetailDAL.SelectAll()
                                                          on a.ProcessId equals b.FlowHeadId
                                                          where a.LoadId == LoadId && a.WhCode == whCode && b.Type == "Release"
                                                          select b).ToList();
                    FlowDetail getMark = new FlowDetail();
                    if (getFlowDetailList.Count > 0)
                    {
                        getMark = getFlowDetailList.First();
                    }

                    List<HuDetail> loadHuList = List.Where(u => u.HuId == LoadId && u.WhCode == whCode).ToList();
                    foreach (var item in huDetailList)
                    {
                        //插入装箱记录
                        TranLog tl = new TranLog();
                        tl.TranType = "210";
                        tl.Description = "装箱";
                        tl.TranDate = DateTime.Now;
                        tl.TranUser = userName;
                        tl.WhCode = item.WhCode;
                        tl.ClientCode = item.ClientCode;
                        tl.SoNumber = item.SoNumber;
                        tl.CustomerPoNumber = item.CustomerPoNumber;
                        tl.AltItemNumber = item.AltItemNumber;
                        tl.ItemId = item.ItemId;
                        tl.UnitID = item.UnitId;
                        tl.UnitName = item.UnitName;
                        tl.TranQty = item.Qty;
                        tl.TranQty2 = item.PlanQty;
                        tl.HuId = item.HuId;
                        tl.Length = item.Length;
                        tl.Width = item.Width;
                        tl.Height = item.Height;
                        tl.Weight = item.Weight;
                        tl.LotNumber1 = item.LotNumber1;
                        tl.LotNumber2 = item.LotNumber2;
                        tl.LotDate = item.LotDate;
                        tl.ReceiptId = item.ReceiptId;
                        tl.ReceiptDate = item.ReceiptDate;

                        tl.Location = huMasterList.Where(u => (u.HuId == LoadId || u.HuId == huId) && u.WhCode == whCode).First().Location;
                        tl.LoadId = LoadId;
                        //装箱位置放在Remark
                        tl.Remark = sql.First().Position;
                        idal.ITranLogDAL.Add(tl);

                        if (loadHuList.Count == 0)
                        {
                            if (getMark.Mark == "1" || getMark.Mark == "2" || getMark.Mark == "9")
                            {
                                //添加库存新明细
                                addHudetail(LoadId, userName, item);
                            }
                            else
                            {
                                addHudetail1(LoadId, userName, item);
                            }
                        }
                        else
                        {
                            //如果是CFS，需要保留SOPO等信息
                            if (getMark.Mark == "1" || getMark.Mark == "2" || getMark.Mark == "9")
                            {
                                if (loadHuList.Where(u => u.HuId == LoadId && u.WhCode == item.WhCode && u.SoNumber == item.SoNumber && u.CustomerPoNumber == item.CustomerPoNumber && u.AltItemNumber == item.AltItemNumber && u.ItemId == item.ItemId && u.UnitId == item.UnitId && u.UnitName == item.UnitName
                                                        &&
                                                           (u.Length == item.Length || (u.Length == null ? 0 : u.Length) == (item.Length == null ? 0 : item.Length)) &&
                                                           (u.Width == item.Width || (u.Width == null ? 0 : u.Width) == (item.Width == null ? 0 : item.Width)) &&
                                                           (u.Height == item.Height || (u.Height == null ? 0 : u.Height) == (item.Height == null ? 0 : item.Height)) &&
                                                           (u.Weight == item.Weight || (u.Weight == null ? 0 : u.Weight) == (item.Weight == null ? 0 : item.Weight)) &&
                                                          (u.LotNumber1 == item.LotNumber1 || (u.LotNumber1 == null ? "" : u.LotNumber1) == (item.LotNumber1 == null ? "" : item.LotNumber1))
                                                           &&
                                                          (u.LotNumber2 == item.LotNumber2 || (u.LotNumber2 == null ? "" : u.LotNumber2) == (item.LotNumber2 == null ? "" : item.LotNumber2)) &&
                                                           u.LotDate == item.LotDate
                                                       ).Count() == 0)
                                {
                                    //添加库存新明细
                                    addHudetail(LoadId, userName, item);
                                }
                                else
                                {
                                    HuDetail entity = loadHuList.Where(u => u.HuId == LoadId && u.WhCode == item.WhCode && u.SoNumber == item.SoNumber && u.CustomerPoNumber == item.CustomerPoNumber && u.AltItemNumber == item.AltItemNumber && u.ItemId == item.ItemId && u.UnitId == item.UnitId && u.UnitName == item.UnitName
                                     &&
                                    (u.Length == item.Length || (u.Length == null ? 0 : u.Length) == (item.Length == null ? 0 : item.Length)) &&
                                    (u.Width == item.Width || (u.Width == null ? 0 : u.Width) == (item.Width == null ? 0 : item.Width)) &&
                                    (u.Height == item.Height || (u.Height == null ? 0 : u.Height) == (item.Height == null ? 0 : item.Height)) &&
                                    (u.Weight == item.Weight || (u.Weight == null ? 0 : u.Weight) == (item.Weight == null ? 0 : item.Weight)) &&
                                  (u.LotNumber1 == item.LotNumber1 || (u.LotNumber1 == null ? "" : u.LotNumber1) == (item.LotNumber1 == null ? "" : item.LotNumber1))
                                    &&
                                   (u.LotNumber2 == item.LotNumber2 || (u.LotNumber2 == null ? "" : u.LotNumber2) == (item.LotNumber2 == null ? "" : item.LotNumber2)) &&
                                    u.LotDate == item.LotDate
                                    ).First();
                                    entity.Qty = entity.Qty + item.Qty;
                                    idal.IHuDetailDAL.UpdateBy(entity, u => u.Id == entity.Id, new string[] { "Qty" });
                                }
                            }
                            else
                            {
                                //电商则不需要SOPO信息，根据款号聚合装箱
                                if (loadHuList.Where(u => u.HuId == LoadId && u.WhCode == item.WhCode && u.AltItemNumber == item.AltItemNumber && u.ItemId == item.ItemId && u.UnitId == item.UnitId && u.UnitName == item.UnitName && (u.LotNumber1 == item.LotNumber1 || (u.LotNumber1 == null ? "" : u.LotNumber1) == (item.LotNumber1 == null ? "" : item.LotNumber1)) && (u.LotNumber2 == item.LotNumber2 || (u.LotNumber2 == null ? "" : u.LotNumber2) == (item.LotNumber2 == null ? "" : item.LotNumber2))).Count() == 0)
                                {
                                    //添加库存新明细
                                    addHudetail1(LoadId, userName, item);
                                }
                                else
                                {
                                    HuDetail entity = loadHuList.Where(u => u.HuId == LoadId && u.WhCode == item.WhCode && u.AltItemNumber == item.AltItemNumber && u.ItemId == item.ItemId && u.UnitId == item.UnitId && u.UnitName == item.UnitName
                                     && (u.LotNumber1 == item.LotNumber1 || (u.LotNumber1 == null ? "" : u.LotNumber1) == (item.LotNumber1 == null ? "" : item.LotNumber1)) && (u.LotNumber2 == item.LotNumber2 || (u.LotNumber2 == null ? "" : u.LotNumber2) == (item.LotNumber2 == null ? "" : item.LotNumber2))).First();
                                    entity.Qty = entity.Qty + item.Qty;
                                    idal.IHuDetailDAL.UpdateBy(entity, u => u.Id == entity.Id, new string[] { "Qty" });
                                }
                            }
                        }
                        idal.IHuDetailDAL.DeleteBy(u => u.Id == item.Id);
                    }
                    idal.IHuMasterDAL.DeleteBy(u => u.WhCode == whCode && u.HuId == huId);


                    //1.修改Load的 装箱状态及 装箱开始时间

                    LoadMaster load = idal.ILoadMasterDAL.SelectBy(u => u.WhCode == whCode && u.LoadId == LoadId).First();

                    if (load.Status3 == "U")
                    {
                        load.Status3 = "A";
                        load.BeginPackDate = DateTime.Now;
                        load.UpdateUser = userName;
                        load.UpdateDate = DateTime.Now;
                        idal.ILoadMasterDAL.UpdateBy(load, u => u.Id == load.Id, new string[] { "Status3", "BeginPackDate", "UpdateUser", "UpdateDate" });
                    }

                    //2.修改备货任务的 装箱状态
                    PickTaskDetail editStatus1 = new PickTaskDetail();
                    editStatus1.Status1 = "C";
                    editStatus1.UpdateUser = userName;
                    editStatus1.UpdateDate = DateTime.Now;
                    idal.IPickTaskDetailDAL.UpdateBy(editStatus1, u => u.WhCode == whCode && u.LoadId == LoadId && u.HuId == huId, new string[] { "Status1", "UpdateUser", "UpdateDate" });

                    idal.IPickTaskDetailDAL.SaveChanges();

                    //3.修改Load的 装箱结束时间
                    var sql1 = from a in idal.IPickTaskDetailDAL.SelectAll()
                               where a.WhCode == whCode && a.LoadId == LoadId && a.Status1 == "U"
                               select a;
                    if (sql1.Count() == 0)
                    {
                        load.Status3 = "C";
                        load.EndPackDate = DateTime.Now;
                        load.UpdateUser = userName;
                        load.UpdateDate = DateTime.Now;
                        idal.ILoadMasterDAL.UpdateBy(load, u => u.Id == load.Id, new string[] { "Status3", "EndPackDate", "UpdateUser", "UpdateDate" });
                    }

                    #endregion

                    idal.ILoadMasterDAL.SaveChanges();
                    trans.Complete();
                    return "Y";
                }
                catch
                {
                    trans.Dispose();//出现异常，事务手动释放
                    return "装箱异常，请重新提交！";
                }
            }
        }

        //添加库存新明细
        private void addHudetail(string LoadId, string userName, HuDetail item)
        {
            HuDetail huDetail = new HuDetail();
            huDetail.WhCode = item.WhCode;
            huDetail.HuId = LoadId;
            huDetail.ClientId = item.ClientId;
            huDetail.ClientCode = item.ClientCode;
            huDetail.SoNumber = item.SoNumber;
            huDetail.CustomerPoNumber = item.CustomerPoNumber;
            huDetail.AltItemNumber = item.AltItemNumber;
            huDetail.ItemId = item.ItemId;
            huDetail.UnitId = item.UnitId;
            huDetail.UnitName = item.UnitName;
            huDetail.ReceiptId = item.ReceiptId;
            huDetail.Qty = item.Qty;
            huDetail.PlanQty = 0;
            huDetail.ReceiptDate = DateTime.Now;
            huDetail.Length = item.Length;
            huDetail.Width = item.Width;
            huDetail.Height = item.Height;
            huDetail.Weight = item.Weight;
            huDetail.LotNumber1 = item.LotNumber1;
            huDetail.LotNumber2 = item.LotNumber2;
            huDetail.LotDate = item.LotDate;
            huDetail.CreateUser = userName;
            huDetail.CreateDate = DateTime.Now;
            idal.IHuDetailDAL.Add(huDetail);
        }

        private void addHudetail1(string LoadId, string userName, HuDetail item)
        {
            HuDetail huDetail = new HuDetail();
            huDetail.WhCode = item.WhCode;
            huDetail.HuId = LoadId;
            huDetail.ClientId = item.ClientId;
            huDetail.ClientCode = item.ClientCode;
            huDetail.SoNumber = "";
            huDetail.CustomerPoNumber = "";
            huDetail.AltItemNumber = item.AltItemNumber;
            huDetail.ItemId = item.ItemId;
            huDetail.UnitId = item.UnitId;
            huDetail.UnitName = item.UnitName;
            huDetail.ReceiptId = "";
            huDetail.Qty = item.Qty;
            huDetail.PlanQty = 0;
            huDetail.ReceiptDate = DateTime.Now;
            huDetail.Length = 0;
            huDetail.Width = 0;
            huDetail.Height = 0;
            huDetail.Weight = 0;
            huDetail.LotNumber1 = item.LotNumber1;
            huDetail.LotNumber2 = item.LotNumber2;
            huDetail.CreateUser = userName;
            huDetail.CreateDate = DateTime.Now;
            idal.IHuDetailDAL.Add(huDetail);
        }


        //集装箱 封箱
        public string ShippingLoad(string loadId, string whCode, string userName, string containerNumber, string sealNumber, int baQty)
        {
            List<LoadContainerExtend> LoadContainerExtendList = idal.ILoadContainerExtendDAL.SelectBy(u => u.LoadId == loadId && u.WhCode == whCode);
            if (LoadContainerExtendList.Count == 0)
            {
                return "未找到箱封号信息！";
            }
            if (containerNumber != "")
            {
                if (LoadContainerExtendList.Where(u => u.ContainerNumber == containerNumber).Count() == 0)
                {
                    return "箱号有误！";
                }
            }

            if (sealNumber != "")
            {
                if (LoadContainerExtendList.Where(u => u.SealNumber == sealNumber).Count() == 0)
                {
                    return "封号有误！";
                }
            }

            if (baQty > 23)
            {
                return "挂衣把数不能大于23!";
            }

            ////计算出货基础费用
            //LoadManager loadManager = new LoadManager();
            //loadManager.LoadChargeAdd(loadId, whCode, userName);

            string result = ShippingLoad(loadId, whCode, userName);
            if (result != "Y")
            {
                return result;
            }

            //添加挂衣把数计件
            if (baQty > 0)
                ShippingLoadAddEchBa(loadId, whCode, baQty);

            //idal.IWorkloadAccountDAL.UpdateByExtended(u => u.WhCode == whCode && u.LoadId == loadId , u => new WorkloadAccount() { BaQty = baQty });

            return "Y";
        }


        //集装箱 封箱
        public string ShippingLoad(string loadId, string whCode, string userName, string containerNumber, string sealNumber)
        {
            List<LoadContainerExtend> LoadContainerExtendList = idal.ILoadContainerExtendDAL.SelectBy(u => u.LoadId == loadId && u.WhCode == whCode);
            if (LoadContainerExtendList.Count == 0)
            {
                return "未找到箱封号信息！";
            }
            if (containerNumber != "")
            {
                if (LoadContainerExtendList.Where(u => u.ContainerNumber == containerNumber).Count() == 0)
                {
                    return "箱号有误！";
                }
            }

            if (sealNumber != "")
            {
                if (LoadContainerExtendList.Where(u => u.SealNumber == sealNumber).Count() == 0)
                {
                    return "封号有误！";
                }
            }

            string result = ShippingLoad(loadId, whCode, userName);
            if (result != "Y")
            {
                return result;
            }

            return "Y";
        }

        //DC BTB封箱
        public string ShippingLoad(string loadId, string whCode, string userName)
        {
            if (loadId == null || loadId == "" || whCode == "" || whCode == null || userName == null || userName == "")
            {
                return "数据有误，请重新操作！";
            }

            using (TransactionScope trans = new TransactionScope())
            {
                try
                {
                    #region 封箱优化
                    List<PickTaskDetail> pickList = (from a in idal.IPickTaskDetailDAL.SelectAll()
                                                     where a.LoadId == loadId && a.WhCode == whCode
                                                     select a).ToList();
                    if (pickList.Where(u => u.Status == "U").Count() > 0)
                    {
                        return "该Load:" + loadId + "未完成备货，请检查备货任务！";
                    }

                    if (pickList.Where(u => u.Status1 == "U").Count() > 0)
                    {
                        return "该Load:" + loadId + "未完成装箱，请检查备货任务！";
                    }

                    List<LoadMaster> loadMasterList = (from a in idal.ILoadMasterDAL.SelectAll()
                                                       where a.LoadId == loadId && a.WhCode == whCode
                                                       select a).ToList();
                    if (loadMasterList.Count == 0)
                    {
                        return "该Load:" + loadId + "无效，请检查Load及Load明细！";
                    }

                    List<OutBoundOrderDetail> outBountOrderDetailList = (from a in idal.IOutBoundOrderDetailDAL.SelectAll()
                                                                         join b in idal.ILoadDetailDAL.SelectAll()
                                                                         on a.OutBoundOrderId equals b.OutBoundOrderId
                                                                         join c in idal.ILoadMasterDAL.SelectAll()
                                                                         on b.LoadMasterId equals c.Id
                                                                         where c.LoadId == loadId && c.WhCode == whCode
                                                                         select a).ToList();
                    if (outBountOrderDetailList.Count == 0)
                    {
                        return "该Load:" + loadId + "未找到订单明细，请检查！";
                    }
                    List<HuDetail> huDetailList = (from a in idal.IHuDetailDAL.SelectAll()
                                                   where a.WhCode == whCode && a.HuId == loadId
                                                   select a).ToList();
                    if (huDetailList.Count == 0)
                    {
                        return "该Load:" + loadId + "未找到装箱明细，请检查！";
                    }

                    string result = ""; //比较结果

                    LoadMaster loadMaster = loadMasterList.First();

                    //验证是否是集装箱发货流程
                    List<FlowDetail> flowDetail1List = idal.IFlowDetailDAL.SelectBy(u => u.FlowHeadId == loadMaster.ProcessId && u.Type == "Shipping");
                    if (flowDetail1List.Count == 0)
                    {
                        result = "该Load:" + loadId + "出货流程中未维护封箱流程！";
                    }
                    else
                    {
                        FlowDetail flow1 = flowDetail1List.First();
                        if (!flow1.Mark.Contains("BTB"))
                        {
                            result = "该Load:" + loadId + "不属于BTB发货流程！";
                        }
                    }
                    if (result != "")
                    {
                        return result;
                    }

                    //得到释放流程 查看是否启用了SO PO的条件匹配
                    List<FlowDetail> flowDetailList = (from a in idal.IFlowDetailDAL.SelectAll()
                                                       where a.FlowHeadId == loadMaster.ProcessId
                                                       select a).ToList();

                    //得到释放的条件类型 先进先出等
                    FlowDetail flowDetail = flowDetailList.Where(u => u.Type == "Release").First();

                    //修改OutOrder 为已发货状态
                    List<OutBoundOrderResult> list = (from a in idal.ILoadDetailDAL.SelectAll()
                                                      where a.LoadMasterId == loadMaster.Id
                                                      select new OutBoundOrderResult
                                                      {
                                                          Id = (Int32)a.OutBoundOrderId
                                                      }).ToList();
                    FlowDetail flowDetail1 = flowDetail1List.First();
                    foreach (var item in list)
                    {
                        OutBoundOrder outBoundOrder = idal.IOutBoundOrderDAL.SelectBy(u => u.Id == item.Id).First();
                        if (outBoundOrder.StatusId >= 20)
                        {
                            string remark1 = "原状态：" + outBoundOrder.StatusId + outBoundOrder.StatusName;

                            outBoundOrder.NowProcessId = flowDetail1.FlowRuleId;
                            outBoundOrder.StatusId = flowDetail1.StatusId;
                            outBoundOrder.StatusName = flowDetail1.StatusName;
                            outBoundOrder.UpdateUser = userName;
                            outBoundOrder.UpdateDate = DateTime.Now;
                            idal.IOutBoundOrderDAL.UpdateBy(outBoundOrder, u => u.Id == item.Id, new string[] { "NowProcessId", "StatusId", "StatusName", "UpdateUser", "UpdateDate" });

                            //更新订单状态，插入日志
                            TranLog tlorder = new TranLog();
                            tlorder.TranType = "32";
                            tlorder.Description = "更新订单状态";
                            tlorder.TranDate = DateTime.Now;
                            tlorder.TranUser = userName;
                            tlorder.WhCode = whCode;
                            tlorder.LoadId = loadMaster.LoadId;
                            tlorder.CustomerOutPoNumber = outBoundOrder.CustomerOutPoNumber;
                            tlorder.OutPoNumber = outBoundOrder.OutPoNumber;
                            tlorder.Remark = remark1 + "变更为：" + outBoundOrder.StatusId + outBoundOrder.StatusName;
                            idal.ITranLogDAL.Add(tlorder);
                        }
                    }

                    //修改出库订单部分信息
                    //by yujia 18.07.26
                    //封箱时 出库订单更新 立方数，最早收货时间，最晚出货时间
                    //得到备货任务明细表数据
                    List<PickTaskDetail> getPickTaskDetail = idal.IPickTaskDetailDAL.SelectBy(u => u.WhCode == whCode && u.LoadId == loadId);
                    foreach (var item in list)
                    {
                        List<OutBoundOrderDetail> outBoundOrderList = idal.IOutBoundOrderDetailDAL.SelectBy(u => u.OutBoundOrderId == item.Id);

                        foreach (var item1 in outBoundOrderList)
                        {
                            if (flowDetail.Mark == "1" || flowDetail.Mark == "2") //12表示有SO PO释放
                            {
                                List<PickTaskDetail> get1 = getPickTaskDetail.Where(u => u.WhCode == item1.WhCode && u.SoNumber == item1.SoNumber && u.CustomerPoNumber == item1.CustomerPoNumber && u.AltItemNumber == item1.AltItemNumber && u.ItemId == item1.ItemId && u.UnitId == item1.UnitId && u.UnitName == item1.UnitName).ToList();

                                DateTime? minReceipt = get1.Min(u => u.ReceiptDate);

                                decimal? sumCbm = get1.Where(u => !u.UnitName.Contains("ECH")).Sum(u => u.Qty * u.Length * u.Width * u.Height);

                                OutBoundOrderDetail setEntity = new OutBoundOrderDetail();
                                setEntity.TotalCbm = sumCbm;
                                setEntity.MinReceiptDate = minReceipt;
                                setEntity.MaxShipDate = DateTime.Now;

                                idal.IOutBoundOrderDetailDAL.UpdateBy(setEntity, u => u.Id == item1.Id, new string[] { "TotalCbm", "MinReceiptDate", "MaxShipDate" });
                            }
                            else if (flowDetail.Mark == "3" || flowDetail.Mark == "4" || flowDetail.Mark == "7")    //347表示无视SO PO释放
                            {
                                List<PickTaskDetail> get1 = getPickTaskDetail.Where(u => u.WhCode == item1.WhCode && u.AltItemNumber == item1.AltItemNumber && u.ItemId == item1.ItemId && u.UnitId == item1.UnitId && u.UnitName == item1.UnitName).ToList();

                                DateTime? minReceipt = get1.Min(u => u.ReceiptDate);

                                decimal? sumCbm = get1.Where(u => !u.UnitName.Contains("ECH")).Sum(u => u.Qty * u.Length * u.Width * u.Height);

                                OutBoundOrderDetail setEntity = new OutBoundOrderDetail();
                                setEntity.TotalCbm = sumCbm;
                                setEntity.MinReceiptDate = minReceipt;
                                setEntity.MaxShipDate = DateTime.Now;

                                idal.IOutBoundOrderDetailDAL.UpdateBy(setEntity, u => u.Id == item1.Id, new string[] { "TotalCbm", "MinReceiptDate", "MaxShipDate" });
                            }
                            else if (flowDetail.Mark == "5" || flowDetail.Mark == "6") //56表示 PO释放
                            {
                                List<PickTaskDetail> get1 = getPickTaskDetail.Where(u => u.WhCode == item1.WhCode && u.CustomerPoNumber == item1.CustomerPoNumber && u.AltItemNumber == item1.AltItemNumber && u.ItemId == item1.ItemId && u.UnitId == item1.UnitId && u.UnitName == item1.UnitName).ToList();

                                DateTime? minReceipt = get1.Min(u => u.ReceiptDate);

                                decimal? sumCbm = get1.Where(u => !u.UnitName.Contains("ECH")).Sum(u => u.Qty * u.Length * u.Width * u.Height);

                                OutBoundOrderDetail setEntity = new OutBoundOrderDetail();
                                setEntity.TotalCbm = sumCbm;
                                setEntity.MinReceiptDate = minReceipt;
                                setEntity.MaxShipDate = DateTime.Now;

                                idal.IOutBoundOrderDetailDAL.UpdateBy(setEntity, u => u.Id == item1.Id, new string[] { "TotalCbm", "MinReceiptDate", "MaxShipDate" });
                            }
                            else
                            {
                                //新增加的流程 默认按SOPO处理
                                List<PickTaskDetail> get1 = getPickTaskDetail.Where(u => u.WhCode == item1.WhCode && u.SoNumber == item1.SoNumber && u.CustomerPoNumber == item1.CustomerPoNumber && u.AltItemNumber == item1.AltItemNumber && u.ItemId == item1.ItemId && u.UnitId == item1.UnitId && u.UnitName == item1.UnitName).ToList();

                                DateTime? minReceipt = get1.Min(u => u.ReceiptDate);

                                decimal? sumCbm = get1.Where(u => !u.UnitName.Contains("ECH")).Sum(u => u.Qty * u.Length * u.Width * u.Height);

                                OutBoundOrderDetail setEntity = new OutBoundOrderDetail();
                                setEntity.TotalCbm = sumCbm;
                                setEntity.MinReceiptDate = minReceipt;
                                setEntity.MaxShipDate = DateTime.Now;

                                idal.IOutBoundOrderDetailDAL.UpdateBy(setEntity, u => u.Id == item1.Id, new string[] { "TotalCbm", "MinReceiptDate", "MaxShipDate" });
                            }
                        }
                    }

                    //插入封箱记录
                    TranLog tl = new TranLog();
                    tl.TranType = "250";
                    tl.Description = "封箱";
                    tl.TranDate = DateTime.Now;
                    tl.TranUser = userName;
                    tl.WhCode = whCode;
                    tl.LoadId = loadId;

                    idal.ITranLogDAL.Add(tl);

                    loadMaster.UpdateUser = userName;
                    loadMaster.UpdateDate = DateTime.Now;
                    loadMaster.ShipDate = DateTime.Now;
                    idal.ILoadMasterDAL.UpdateBy(loadMaster, u => u.Id == loadMaster.Id, new string[] { "UpdateUser", "UpdateDate", "ShipDate" });

                    UrlEdiTaskInsert(loadId, whCode, userName);

                    //如果是集装箱提货、需删除收货CartonId方便下次二次进仓
                    //2021年10月26日增加
                    List<LoadContainerExtend> getLoadContainerList = idal.ILoadContainerExtendDAL.SelectBy(u => u.WhCode == whCode && u.LoadId == loadId);
                    if (getLoadContainerList.Count > 0)
                    {
                        LoadContainerExtend firstLoadContainer = getLoadContainerList.First();
                        if (firstLoadContainer.ChuCangFS == "提货")
                        {
                            List<SerialNumberOut> getSerialNumberOutList = idal.ISerialNumberOutDAL.SelectBy(u => u.WhCode == whCode && u.LoadId == loadId);
                            if (getSerialNumberOutList.Count > 0)
                            {
                                int?[] itemIdArr = (from a in getSerialNumberOutList select a.ItemId).ToList().Distinct().ToArray();

                                string[] cartonIdArr = (from a in getSerialNumberOutList select a.CartonId).ToList().Distinct().ToArray();

                                string[] soArr = (from a in getSerialNumberOutList select a.SoNumber).ToList().Distinct().ToArray();

                                string[] poArr = (from a in getSerialNumberOutList select a.CustomerPoNumber).ToList().Distinct().ToArray();

                                idal.ISerialNumberInDAL.DeleteByExtended(u => itemIdArr.Contains(u.ItemId) && u.WhCode == whCode && soArr.Contains(u.SoNumber) && poArr.Contains(u.CustomerPoNumber) && cartonIdArr.Contains(u.CartonId));
                            }
                        }
                    }

                    idal.IPickTaskDetailDAL.SaveChanges();

                    //删除装箱单所选库存表
                    List<LoadContainerExtend> loadContainerList = idal.ILoadContainerExtendDAL.SelectBy(u => u.LoadId == loadId && u.WhCode == whCode);
                    if (loadContainerList.Count > 0)
                    {
                        LoadContainerExtend loadContainer = loadContainerList.First();
                        idal.ILoadContainerExtendHuDetailDAL.DeleteByExtended(u => u.LoadContainerId == loadContainer.Id && u.WhCode == whCode);
                    }

                    //删除库存
                    idal.IHuDetailDAL.DeleteByExtended(u => u.HuId == loadId && u.WhCode == whCode);
                    idal.IHuMasterDAL.DeleteByExtended(u => u.HuId == loadId && u.WhCode == whCode);

                    //删除备货任务
                    idal.IPickTaskDetailDAL.DeleteByExtended(u => u.LoadId == loadId && u.WhCode == whCode);

                    //删除分拣任务
                    idal.ISortTaskDAL.DeleteByExtended(u => u.LoadId == loadId && u.WhCode == whCode);
                    idal.ISortTaskDetailDAL.DeleteByExtended(u => u.LoadId == loadId && u.WhCode == whCode);

                    #endregion

                    trans.Complete();
                    return "Y";
                }
                catch
                {
                    trans.Dispose();//出现异常，事务手动释放
                    return "封箱异常，请重新提交！";
                }
            }
        }



        //ByLoad备货后自动装箱
        public string PickingSortingByLoad1(string LoadId, string whCode, string userName, string HuId, string PutHuId, string Location)
        {
            if (LoadId == null || LoadId == "" || whCode == "" || whCode == null || HuId == null || HuId == "" || userName == null || userName == "" || Location == null || Location == "")
            {
                return "数据有误，请重新操作！";
            }

            if (!shipHelper.CheckLoadStatus(whCode, LoadId))
            {
                return "该Load:" + LoadId + "状态有误，请检查！";
            }

            if (!shipHelper.CheckOutLocation(whCode, Location))
            {
                return "错误！备货门区有误！";
            }

            if (!shipHelper.CheckHuStatusByPickTask(whCode, HuId, LoadId))
            {
                return "该Load:" + LoadId + "对应的备货托盘：" + HuId + "状态有误或不存在，请检查备货任务！";
            }

            if (!shipHelper.CheckHuStatus(whCode, HuId))
            {
                return "库存托盘:" + HuId + "状态有误，请检查！";
            }
            if (!shipHelper.CheckLocationStatusByHuId(whCode, HuId))
            {
                return "库存托盘:" + HuId + "对应的库位状态有误，请检查！";
            }

            string mess = CheckPickingLoad(LoadId, whCode, HuId);
            //既不是拆托 也不是整出 表示有误
            if (mess != "N" && mess != "Y")
            {
                return mess;
            }
            //拆托还要验证新托盘
            if (mess == "N")
            {
                if (!shipHelper.CheckPlt(whCode, PutHuId))
                {
                    return "错误！托盘" + PutHuId + "不存在或已使用！";
                }
            }

            LoadMaster load = idal.ILoadMasterDAL.SelectBy(u => u.WhCode == whCode && u.LoadId == LoadId).First();
            //-----------25 为已备货
            FlowDetail flowDetail = idal.IFlowDetailDAL.SelectBy(u => u.FlowHeadId == load.ProcessId && u.Type == "Picking").First();

            //订单状态修改列表
            List<OutBoundOrder> OutBoundOrderUpdateList = new List<OutBoundOrder>();

            List<TranLog> tranLogAddList = new List<TranLog>();
            List<TranLog> tranLogAddList1 = new List<TranLog>();

            using (TransactionScope trans = new TransactionScope())
            {
                try
                {
                    //修改Load状态
                    if (load.Status1 == "U")
                    {
                        load.Location = Location;
                        load.Status1 = "A";
                        load.UpdateUser = userName;
                        load.UpdateDate = DateTime.Now;
                        load.BeginPickDate = DateTime.Now;
                        idal.ILoadMasterDAL.UpdateBy(load, u => u.Id == load.Id, new string[] { "Status1", "UpdateUser", "UpdateDate", "BeginPickDate", "Location" });

                        //修改OutOrder状态
                        List<OutBoundOrderResult> list = (from a in idal.ILoadDetailDAL.SelectAll()
                                                          where a.LoadMasterId == load.Id
                                                          select new OutBoundOrderResult
                                                          {
                                                              Id = (Int32)a.OutBoundOrderId
                                                          }).ToList();

                        int[] OutBoundOrderId = (from a in list
                                                 select a.Id).ToList().Distinct().ToArray();

                        List<OutBoundOrder> OutBoundOrderList = idal.IOutBoundOrderDAL.SelectBy(u => OutBoundOrderId.Contains(u.Id));

                        foreach (var item in list)
                        {
                            OutBoundOrder outBoundOrder = OutBoundOrderList.Where(u => u.Id == item.Id).First();

                            if (outBoundOrder.StatusId > 15 && outBoundOrder.StatusId < 30) //订单状态必须为草稿以上
                            {
                                string remark1 = "原状态：" + outBoundOrder.StatusId + outBoundOrder.StatusName;

                                //订单流程变更为 25 已备货
                                outBoundOrder.NowProcessId = flowDetail.FlowRuleId;
                                outBoundOrder.StatusId = flowDetail.StatusId;
                                outBoundOrder.StatusName = "正在备货";
                                outBoundOrder.UpdateUser = userName;
                                outBoundOrder.UpdateDate = DateTime.Now;

                                ////取消单次更新，变更为批量Sql执行更新
                                //OutBoundOrderUpdateList.Add(outBoundOrder);

                                //更新订单状态，插入日志
                                TranLog tl = new TranLog();
                                tl.TranType = "32";
                                tl.Description = "备货更新订单状态";
                                tl.TranDate = DateTime.Now;
                                tl.TranUser = userName;
                                tl.WhCode = whCode;
                                tl.LoadId = LoadId;
                                tl.CustomerOutPoNumber = outBoundOrder.CustomerOutPoNumber;
                                tl.OutPoNumber = outBoundOrder.OutPoNumber;
                                tl.Remark = remark1 + "变更为：" + outBoundOrder.StatusId + outBoundOrder.StatusName;
                                tranLogAddList.Add(tl);

                            }
                        }
                    }

                    ////修改出库订单状态为正在备货
                    //foreach (var outBoundOrderUpdate in OutBoundOrderUpdateList)
                    //{
                    //    idal.IOutBoundOrderDAL.UpdateByExtended(u => u.Id == outBoundOrderUpdate.Id, t => new OutBoundOrder { NowProcessId = outBoundOrderUpdate.NowProcessId, StatusId = outBoundOrderUpdate.StatusId, StatusName = outBoundOrderUpdate.StatusName, UpdateUser = userName, UpdateDate = DateTime.Now });
                    //}

                    //1.修改库存托盘状态 及 实现托盘拆托
                    List<PickTaskDetail> pickList = idal.IPickTaskDetailDAL.SelectBy(u => u.WhCode == whCode && u.LoadId == LoadId && u.HuId == HuId && u.Status == "U");

                    //如果是拆托
                    if (mess == "N")
                    {
                        List<HuDetail> huDetailList = idal.IHuDetailDAL.SelectBy(u => u.WhCode == whCode && u.HuId == HuId);
                        int count = idal.IHuMasterDAL.SelectBy(u => u.WhCode == whCode && u.HuId == HuId).Count();
                        foreach (var item in pickList)
                        {
                            //1.如果是拆托 验证拆托托盘是否存在
                            HuDetail huDetail = huDetailList.Where(u => u.Id == item.HuDetailId).First();
                            if (count == 0)
                            {
                                HuMaster huMaster = new HuMaster();
                                huMaster.WhCode = whCode;
                                huMaster.HuId = PutHuId;
                                huMaster.LoadId = LoadId;
                                huMaster.Type = "M";
                                huMaster.Status = "A";
                                huMaster.Location = Location;
                                huMaster.ReceiptId = huDetail.ReceiptId;
                                huMaster.ReceiptDate = DateTime.Now;
                                huMaster.TransactionFlag = 1;
                                huMaster.CreateUser = userName;
                                huMaster.CreateDate = DateTime.Now;
                                idal.IHuMasterDAL.Add(huMaster);
                            }
                            //1.1加入拆托明细 锁定数量为0
                            HuDetail entity = new HuDetail();
                            entity.WhCode = whCode;
                            entity.HuId = PutHuId;
                            entity.ClientId = huDetail.ClientId;
                            entity.ClientCode = huDetail.ClientCode;
                            entity.SoNumber = huDetail.SoNumber;
                            entity.CustomerPoNumber = huDetail.CustomerPoNumber;
                            entity.AltItemNumber = huDetail.AltItemNumber;
                            entity.ItemId = huDetail.ItemId;
                            entity.UnitId = huDetail.UnitId;
                            entity.UnitName = huDetail.UnitName;
                            entity.ReceiptId = huDetail.ReceiptId;
                            entity.Qty = item.Qty;
                            entity.PlanQty = 0;
                            entity.ReceiptDate = DateTime.Now;
                            entity.Length = huDetail.Length;
                            entity.Width = huDetail.Width;
                            entity.Height = huDetail.Height;
                            entity.Weight = huDetail.Weight;
                            entity.LotNumber1 = huDetail.LotNumber1;
                            entity.LotNumber2 = huDetail.LotNumber2;
                            entity.LotDate = huDetail.LotDate;
                            entity.CreateUser = userName;
                            entity.CreateDate = DateTime.Now;
                            idal.IHuDetailDAL.Add(entity);

                            //1.2 插入备货记录
                            AddPickingTranLog1(huDetail, userName, whCode, item.Location, "", LoadId, PutHuId, item.Qty);

                            //1.3 修改原托盘数量
                            HuDetail entity1 = new HuDetail();
                            entity1.Qty = huDetail.Qty - item.Qty;
                            entity1.PlanQty = huDetail.PlanQty - item.Qty;
                            entity1.UpdateUser = userName;
                            entity1.UpdateDate = DateTime.Now;
                            if (entity1.Qty == 0)
                            {
                                idal.IHuDetailDAL.DeleteBy(u => u.Id == huDetail.Id);

                                //1.3 插入备货删除记录
                                AddPickingTranLog2(huDetail, userName, whCode, item.Location, "", LoadId, PutHuId, item.Qty);
                            }
                            else
                            {
                                idal.IHuDetailDAL.UpdateBy(entity1, u => u.Id == huDetail.Id, new string[] { "Qty", "PlanQty", "UpdateUser", "UpdateDate" });
                            }
                            //1.2 插入备货记录 
                            AddPickingTranLog(entity, userName, whCode, item.Location, Location, LoadId, "");
                            count++;
                        }

                        //1.3 修改备货任务托盘
                        PickTaskDetail pick = new PickTaskDetail();
                        pick.Status = "C";
                        pick.HuId = PutHuId;
                        pick.UpdateUser = userName;
                        pick.UpdateDate = DateTime.Now;
                        idal.IPickTaskDetailDAL.UpdateBy(pick, u => u.LoadId == LoadId && u.WhCode == whCode && u.HuId == HuId, new string[] { "Status", "HuId", "UpdateUser", "UpdateDate" });

                        //1.4 修改原托盘拆托Flag为1
                        HuMaster huma = new HuMaster();
                        huma.TransactionFlag = 1;
                        huma.UpdateUser = userName;
                        huma.UpdateDate = DateTime.Now;
                        idal.IHuMasterDAL.UpdateBy(huma, u => u.WhCode == whCode && u.HuId == HuId, new string[] { "TransactionFlag", "UpdateUser", "UpdateDate" });
                    }
                    else if (mess == "Y")   //整出
                    {
                        HuMaster huMaster = idal.IHuMasterDAL.SelectBy(u => u.WhCode == whCode && u.HuId == HuId).First();
                        //1.1 插入备货记录
                        List<HuDetail> huDetailList = idal.IHuDetailDAL.SelectBy(u => u.WhCode == whCode && u.HuId == HuId);
                        foreach (var item in huDetailList)
                        {
                            AddPickingTranLog(item, userName, whCode, huMaster.Location, Location, LoadId, "");
                        }

                        //1.2 修改托盘信息
                        huMaster.LoadId = LoadId;
                        huMaster.Location = Location;
                        huMaster.UpdateUser = userName;
                        huMaster.UpdateDate = DateTime.Now;
                        idal.IHuMasterDAL.UpdateBy(huMaster, u => u.Id == huMaster.Id, new string[] { "LoadId", "Location", "UpdateUser", "UpdateDate" });

                        HuDetail huDetail = new HuDetail();
                        huDetail.PlanQty = 0;
                        huDetail.UpdateUser = userName;
                        huDetail.UpdateDate = DateTime.Now;
                        idal.IHuDetailDAL.UpdateBy(huDetail, u => u.WhCode == huMaster.WhCode && u.HuId == huMaster.HuId, new string[] { "PlanQty", "UpdateUser", "UpdateDate" });

                        //1.3 修改备货任务状态
                        PickTaskDetail pick = new PickTaskDetail();
                        pick.Status = "C";
                        pick.UpdateUser = userName;
                        pick.UpdateDate = DateTime.Now;
                        idal.IPickTaskDetailDAL.UpdateBy(pick, u => u.LoadId == LoadId && u.WhCode == whCode && u.HuId == HuId, new string[] { "Status", "UpdateUser", "UpdateDate" });
                    }

                    //修改Load的分拣开始时间
                    if (load.Status2 == "U")
                    {
                        load.Status2 = "A";
                        load.BeginSortDate = DateTime.Now;
                        idal.ILoadMasterDAL.UpdateBy(load, u => u.Id == load.Id, new string[] { "Status2", "BeginSortDate" });
                    }

                    idal.SaveChanges();

                    //检查是否全部完成备货
                    var sql4 = from a in idal.IPickTaskDetailDAL.SelectAll()
                               where a.LoadId == LoadId && a.WhCode == whCode && a.Status == "U" && a.HuId != HuId
                               select a;
                    if (sql4.Count() == 0)
                    {
                        load.Status1 = "C";
                        load.UpdateUser = userName;
                        load.UpdateDate = DateTime.Now;
                        load.EndPickDate = DateTime.Now;
                        idal.ILoadMasterDAL.UpdateBy(load, u => u.Id == load.Id, new string[] { "Status1", "UpdateUser", "UpdateDate", "EndPickDate" });

                        //修改OutOrder状态
                        List<OutBoundOrderResult> list = (from a in idal.ILoadDetailDAL.SelectAll()
                                                          where a.LoadMasterId == load.Id
                                                          select new OutBoundOrderResult
                                                          {
                                                              Id = (Int32)a.OutBoundOrderId
                                                          }).ToList();

                        int[] OutBoundOrderId = (from a in list
                                                 select a.Id).ToList().Distinct().ToArray();

                        List<OutBoundOrder> OutBoundOrderList = idal.IOutBoundOrderDAL.SelectBy(u => OutBoundOrderId.Contains(u.Id));

                        foreach (var item in list)
                        {
                            OutBoundOrder outBoundOrder = OutBoundOrderList.Where(u => u.Id == item.Id).First();

                            if (outBoundOrder.StatusName == "正在备货" && outBoundOrder.StatusId >= 15 && outBoundOrder.StatusId < 30)
                            {
                                string remark1 = "原状态：" + outBoundOrder.StatusId + outBoundOrder.StatusName;

                                outBoundOrder.StatusName = flowDetail.StatusName;
                                outBoundOrder.UpdateUser = userName;
                                outBoundOrder.UpdateDate = DateTime.Now;

                                //idal.IOutBoundOrderDAL.UpdateByExtended(u => u.Id == outBoundOrder.Id, t => new OutBoundOrder { StatusName = flowDetail.StatusName, UpdateUser = userName, UpdateDate = DateTime.Now });

                                //更新订单状态，插入日志
                                TranLog tl = new TranLog();
                                tl.TranType = "32";
                                tl.Description = "备货更新订单状态";
                                tl.TranDate = DateTime.Now;
                                tl.TranUser = userName;
                                tl.WhCode = whCode;
                                tl.LoadId = LoadId;
                                tl.CustomerOutPoNumber = outBoundOrder.CustomerOutPoNumber;
                                tl.OutPoNumber = outBoundOrder.OutPoNumber;
                                tl.Remark = remark1 + "变更为：" + outBoundOrder.StatusId + outBoundOrder.StatusName;
                                tranLogAddList.Add(tl);
                            }
                        }
                    }

                    //----------------begin 
                    //备货时 同时插入分拣任务表中的PickQty (备货数量)  Qty (分拣数量)
                    //1.首先 根据备货任务表的款号等 查询出 分拣任务表的对应数据

                    List<SortTaskDetail> checkResultList = new List<SortTaskDetail>();
                    foreach (var item in pickList)
                    {
                        List<SortTaskDetail> sortTaskDetailList = idal.ISortTaskDetailDAL.SelectBy(u => u.AltItemNumber == item.AltItemNumber && u.ItemId == item.ItemId && u.LoadId == item.LoadId && u.WhCode == item.WhCode && u.PlanQty != u.PickQty).OrderBy(u => u.GroupId).ToList();

                        int resultQty = item.Qty;
                        foreach (var detail in sortTaskDetailList)
                        {
                            if (checkResultList.Where(u => u.Id == detail.Id).Count() > 0)
                            {
                                detail.PickQty = checkResultList.Where(u => u.Id == detail.Id).First().PickQty;
                            }
                            if (detail.PickQty == detail.PlanQty)
                            {
                                continue;
                            }

                            if (checkResultList.Where(u => u.Id == detail.Id).Count() > 0)
                            {
                                SortTaskDetail old = checkResultList.Where(u => u.Id == detail.Id).First();
                                checkResultList.Remove(old);
                            }

                            if (detail.PickQty + resultQty > detail.PlanQty)
                            {
                                resultQty = resultQty - (detail.PlanQty - (Int32)detail.PickQty);

                                SortTaskDetail entity = new SortTaskDetail();
                                entity.Id = detail.Id;
                                entity.PickQty = detail.PlanQty;
                                entity.Qty = detail.PlanQty;
                                checkResultList.Add(entity);

                                SortTaskDetail editentity = detail;
                                editentity.PickQty = entity.PickQty;

                                continue;
                            }
                            else
                            {
                                SortTaskDetail entity = new SortTaskDetail();
                                entity.Id = detail.Id;
                                entity.PickQty = detail.PickQty + resultQty;
                                entity.Qty = (Int32)detail.PickQty + resultQty;
                                checkResultList.Add(entity);

                                SortTaskDetail editentity = detail;
                                editentity.PickQty = entity.PickQty;

                                break;
                            }
                        }
                    }

                    //更新分拣表的备货数量
                    foreach (var item in checkResultList)
                    {
                        idal.ISortTaskDetailDAL.UpdateByExtended(u => u.Id == item.Id, t => new SortTaskDetail { PickQty = item.PickQty, Qty = (Int32)item.Qty, UpdateUser = userName, UpdateDate = DateTime.Now });
                    }

                    //--------------- end

                    string result1 = "";
                    //装箱
                    if (mess == "N")
                    {
                        result1 = PackingLoad(LoadId, whCode, PutHuId, userName);
                    }
                    else if (mess == "Y")
                    {
                        result1 = PackingLoad(LoadId, whCode, HuId, userName);
                    }
                    if (result1 != "Y")
                    {
                        trans.Dispose();//出现异常，事务手动释放
                        return "备货异常,请重新提交！";
                    }

                    idal.ITranLogDAL.Add(tranLogAddList);
                    idal.SaveChanges();
                    trans.Complete();
                    return "Y";
                }
                catch (Exception e)
                {
                    trans.Dispose();//出现异常，事务手动释放
                    return "备货异常，请重新提交！";
                }
            }
        }

        public string SortLoad(string LoadId, string whCode, string userName)
        {

            List<TranLog> tranLogAddList1 = new List<TranLog>();

            //验证是否有出库订单完成分拣
            var sql1 = from a in (
                                       (from sorttaskdetail in idal.ISortTaskDetailDAL.SelectAll()
                                        where
                                          sorttaskdetail.LoadId == LoadId &&
                                          sorttaskdetail.WhCode == whCode &&
                                          sorttaskdetail.GroupNumber == ""
                                        group sorttaskdetail by new
                                        {
                                            sorttaskdetail.GroupId,
                                            sorttaskdetail.OutPoNumber
                                        } into g
                                        select new
                                        {
                                            GroupId = (Int32?)g.Key.GroupId,
                                            g.Key.OutPoNumber,
                                            PlanQty = (Int32?)g.Sum(p => p.PlanQty),
                                            Qty = (Int32?)g.Sum(p => p.Qty)
                                        }))
                       where a.PlanQty == a.Qty
                       select new SortTaskDetailResult
                       {
                           GroupId = a.GroupId,
                           OutPoNumber = a.OutPoNumber,
                           PlanQty = a.PlanQty,
                           Qty = a.Qty
                       };

            List<SortTaskDetailResult> groupIdList = sql1.ToList();
            if (groupIdList.Count() > 0)
            {
                List<SortTaskDetail> sortList = idal.ISortTaskDetailDAL.SelectBy(u => u.LoadId == LoadId && u.WhCode == whCode);

                string[] OutPoNumberArr = (from a in groupIdList
                                           select a.OutPoNumber).ToList().Distinct().ToArray();

                List<OutBoundOrder> OutBoundOrderList = idal.IOutBoundOrderDAL.SelectBy(u => u.WhCode == whCode && OutPoNumberArr.Contains(u.OutPoNumber));

                string[] CustomerOutPoNumberArr = (from a in OutBoundOrderList
                                                   select a.CustomerOutPoNumber).ToList().Distinct().ToArray();

                List<SortTaskDetail> sortTaskDetailUpdateList = new List<SortTaskDetail>();


                OutBoundOrder firstOutBoundOrder = OutBoundOrderList.First();

                //如果检查到流程中有包装流程 
                List<FlowDetail> checkFlowDetailList = idal.IFlowDetailDAL.SelectBy(u => u.FlowHeadId == firstOutBoundOrder.ProcessId && u.Type == "PackingType");
                //如果检查到流程中有分拣流程 
                List<FlowDetail> checkSortingFlowDetailList = idal.IFlowDetailDAL.SelectBy(u => u.FlowHeadId == firstOutBoundOrder.ProcessId && u.Type == "Sorting");

                string sortingMark = "";
                if (checkSortingFlowDetailList.Count > 0)
                {
                    sortingMark = checkSortingFlowDetailList.First().Mark;
                }

                //得到Load包装任务表
                List<PackTask> packTaskList = idal.IPackTaskDAL.SelectBy(u => u.WhCode == whCode && u.LoadId == LoadId);

                //得到出库订单Json表
                List<PackTaskJson> packTaskJsonList = idal.IPackTaskJsonDAL.SelectBy(u => u.WhCode == whCode && CustomerOutPoNumberArr.Contains(u.CustomerOutPoNumber));

                List<PackTask> addPackTaskList = new List<PackTask>();
                foreach (var item in groupIdList)
                {
                    OutBoundOrder outBoundOrder = OutBoundOrderList.Where(u => u.WhCode == whCode && u.OutPoNumber == item.OutPoNumber).First();

                    //更新分拣明细中的框号
                    SortTaskDetail sortTaskDetail = new SortTaskDetail();
                    sortTaskDetail.LoadId = LoadId;
                    sortTaskDetail.WhCode = whCode;
                    sortTaskDetail.GroupId = item.GroupId;

                    if (sortingMark == "1")
                    {
                        sortTaskDetail.GroupNumber = LoadId + item.GroupId.ToString();
                    }
                    else if (sortingMark == "2")
                    {
                        sortTaskDetail.GroupNumber = outBoundOrder.CustomerOutPoNumber;
                    }
                    else if (sortingMark == "3")
                    {
                        sortTaskDetail.GroupNumber = outBoundOrder.AltCustomerOutPoNumber;
                    }
                    else
                    {
                        sortTaskDetail.GroupNumber = LoadId + item.GroupId.ToString();
                    }

                    sortTaskDetail.UpdateUser = userName;
                    sortTaskDetail.UpdateDate = DateTime.Now;
                    sortTaskDetailUpdateList.Add(sortTaskDetail);

                    //idal.ISortTaskDetailDAL.UpdateBy(sortTaskDetail, u => u.LoadId == LoadId && u.WhCode == whCode && u.GroupId == item.GroupId, new string[] { "GroupNumber", "UpdateUser", "UpdateDate" });

                    //-----订单完成分拣绑定框号时 发现被拦截
                    //1.更改订单状态为已拦截待处理
                    if (sortList.Where(u => u.HoldFlag == 1 && u.OutPoNumber == item.OutPoNumber).Count() > 0)
                    {
                        if (checkFlowDetailList.Count > 0)
                        {
                            List<PackTask> checkpackTaskList = packTaskList.Where(u => u.WhCode == whCode && u.LoadId == LoadId && u.OutPoNumber == outBoundOrder.OutPoNumber && u.SortGroupId == item.GroupId).ToList();
                            if (checkpackTaskList.Count == 0)
                            {
                                PackTask packTask = new PackTask();
                                packTask.WhCode = whCode;
                                packTask.LoadId = LoadId;
                                packTask.SortGroupId = item.GroupId;                                //分拣组号

                                if (sortingMark == "1")
                                {
                                    packTask.SortGroupNumber = LoadId + item.GroupId.ToString();
                                }
                                else if (sortingMark == "2")
                                {
                                    packTask.SortGroupNumber = outBoundOrder.CustomerOutPoNumber;
                                }
                                else if (sortingMark == "3")
                                {
                                    packTask.SortGroupNumber = outBoundOrder.AltCustomerOutPoNumber;
                                }
                                else
                                {
                                    packTask.SortGroupNumber = LoadId + item.GroupId.ToString();
                                }

                                packTask.CustomerOutPoNumber = outBoundOrder.CustomerOutPoNumber;  //客户出库订单号
                                packTask.OutPoNumber = outBoundOrder.OutPoNumber;                   //系统出库订单号
                                packTask.Status = -10;                                              //状态为-10：订单被拦截
                                packTask.CreateUser = userName;
                                packTask.CreateDate = DateTime.Now;
                                addPackTaskList.Add(packTask);
                            }
                        }
                    }
                    else
                    {
                        if (checkFlowDetailList.Count > 0)
                        {
                            List<PackTask> checkpackTaskList = packTaskList.Where(u => u.WhCode == whCode && u.LoadId == LoadId && u.OutPoNumber == outBoundOrder.OutPoNumber && u.SortGroupId == item.GroupId).ToList();
                            if (checkpackTaskList.Count == 0)
                            {
                                List<PackTaskJson> getPackTaskJson = packTaskJsonList.Where(u => u.WhCode == whCode && u.CustomerOutPoNumber == outBoundOrder.CustomerOutPoNumber).ToList();
                                if (getPackTaskJson.Count == 0)
                                {
                                    PackTask packTask = new PackTask();
                                    packTask.WhCode = whCode;
                                    packTask.LoadId = LoadId;
                                    packTask.SortGroupId = item.GroupId;                                //分拣组号

                                    if (sortingMark == "1")
                                    {
                                        packTask.SortGroupNumber = LoadId + item.GroupId.ToString();
                                    }
                                    else if (sortingMark == "2")
                                    {
                                        packTask.SortGroupNumber = outBoundOrder.CustomerOutPoNumber;
                                    }
                                    else if (sortingMark == "3")
                                    {
                                        packTask.SortGroupNumber = outBoundOrder.AltCustomerOutPoNumber;
                                    }
                                    else
                                    {
                                        packTask.SortGroupNumber = LoadId + item.GroupId.ToString();
                                    }

                                    packTask.CustomerOutPoNumber = outBoundOrder.CustomerOutPoNumber;  //客户出库订单号
                                    packTask.OutPoNumber = outBoundOrder.OutPoNumber;                   //系统出库订单号
                                    packTask.Status = 0;                                              //状态为0：任务初始化
                                    packTask.CreateUser = userName;
                                    packTask.CreateDate = DateTime.Now;
                                    addPackTaskList.Add(packTask);
                                }
                                else
                                {
                                    PackTask packTask = new PackTask();

                                    PackTaskJson packJson = getPackTaskJson.First();

                                    PackTaskJsonEntity packTaskJsonEntity = JsonConvert.DeserializeObject<PackTaskJsonEntity>(packJson.Json);

                                    packTask.TransportType = packTaskJsonEntity.TransportType;
                                    packTask.express_code = packTaskJsonEntity.express_code;
                                    packTask.express_type = packTaskJsonEntity.express_type;
                                    packTask.express_type_zh = packTaskJsonEntity.express_type_zh;
                                    packTask.SingleFlag = packTaskJsonEntity.SingleFlag;
                                    if (packTask.SingleFlag == 1)
                                    {
                                        packTask.PackQty = 1;
                                    }

                                    packTask.SinglePlaneTemplate = packTaskJsonEntity.SinglePlaneTemplate;
                                    packTask.PackingListTemplate = packTaskJsonEntity.PackingListTemplate;
                                    packTask.PackMoreFlag = packTaskJsonEntity.PackMoreFlag;
                                    packTask.ZdFlag = packTaskJsonEntity.ZdFlag;

                                    packTask.WhCode = whCode;
                                    packTask.LoadId = LoadId;
                                    packTask.SortGroupId = item.GroupId;                                //分拣组号

                                    if (sortingMark == "1")
                                    {
                                        packTask.SortGroupNumber = LoadId + item.GroupId.ToString();
                                    }
                                    else if (sortingMark == "2")
                                    {
                                        packTask.SortGroupNumber = outBoundOrder.CustomerOutPoNumber;
                                    }
                                    else if (sortingMark == "3")
                                    {
                                        packTask.SortGroupNumber = outBoundOrder.AltCustomerOutPoNumber;
                                    }
                                    else
                                    {
                                        packTask.SortGroupNumber = LoadId + item.GroupId.ToString();
                                    }

                                    packTask.CustomerOutPoNumber = outBoundOrder.CustomerOutPoNumber;  //客户出库订单号
                                    packTask.OutPoNumber = outBoundOrder.OutPoNumber;                  //系统出库订单号
                                    packTask.Status = 10;                                               //状态为10：已获取物流信息
                                    packTask.CreateUser = userName;
                                    packTask.CreateDate = DateTime.Now;
                                    addPackTaskList.Add(packTask);
                                }

                            }
                        }

                        if (checkSortingFlowDetailList.Count > 0)
                        {
                            if (outBoundOrder.StatusId > 15 && outBoundOrder.StatusId < 30) //订单状态必须为草稿以上
                            {
                                FlowDetail sortFlowDetail = checkSortingFlowDetailList.First();
                                string remark1 = "原状态：" + outBoundOrder.StatusId + outBoundOrder.StatusName;

                                outBoundOrder.NowProcessId = sortFlowDetail.FlowRuleId;
                                outBoundOrder.StatusId = sortFlowDetail.StatusId;
                                outBoundOrder.StatusName = sortFlowDetail.StatusName;
                                outBoundOrder.UpdateUser = userName;
                                outBoundOrder.UpdateDate = DateTime.Now;

                                //OutBoundOrderUpdateList.Clear();

                                ////取消单次更新，变更为批量Sql执行更新
                                //OutBoundOrderUpdateList.Add(outBoundOrder);

                                idal.IOutBoundOrderDAL.UpdateByExtended(u => u.Id == outBoundOrder.Id, t => new OutBoundOrder { NowProcessId = sortFlowDetail.FlowRuleId, StatusId = sortFlowDetail.StatusId, StatusName = sortFlowDetail.StatusName, UpdateUser = userName, UpdateDate = DateTime.Now });

                                //更新订单状态，插入日志
                                TranLog tl = new TranLog();
                                tl.TranType = "32";
                                tl.Description = "分拣更新订单状态";
                                tl.TranDate = DateTime.Now;
                                tl.TranUser = userName;
                                tl.WhCode = whCode;
                                tl.LoadId = LoadId;
                                tl.CustomerOutPoNumber = outBoundOrder.CustomerOutPoNumber;
                                tl.OutPoNumber = outBoundOrder.OutPoNumber;
                                tl.Remark = remark1 + "变更为：" + outBoundOrder.StatusId + outBoundOrder.StatusName;
                                tranLogAddList1.Add(tl);
                            }

                        }
                    }
                }

                idal.IPackTaskDAL.Add(addPackTaskList);

                //更新 分拣任务状态
                if (sortList.Where(u => u.HoldFlag != 1).Where(u => u.PlanQty != u.PickQty).Count() == 0)
                {
                    SortTask sortTask = new SortTask();
                    sortTask.Status = "C";
                    sortTask.UpdateUser = userName;
                    sortTask.UpdateDate = DateTime.Now;
                    idal.ISortTaskDAL.UpdateBy(sortTask, u => u.LoadId == LoadId && u.WhCode == whCode, new string[] { "Status", "UpdateUser", "UpdateDate" });

                    //更新分拣完成时间
                    LoadMaster loadMaster = new LoadMaster();
                    loadMaster.Status2 = "C";
                    loadMaster.EndSortDate = DateTime.Now;
                    idal.ILoadMasterDAL.UpdateBy(loadMaster, u => u.LoadId == LoadId && u.WhCode == whCode, new string[] { "Status2", "EndSortDate" });
                }

                //批量更新分拣表的分拣框号
                foreach (var item in sortTaskDetailUpdateList)
                {
                    idal.ISortTaskDetailDAL.UpdateByExtended(u => u.LoadId == item.LoadId && u.WhCode == item.WhCode && u.GroupId == item.GroupId, t => new SortTaskDetail { GroupNumber = item.GroupNumber, UpdateUser = userName, UpdateDate = DateTime.Now });
                }

            }

            idal.ITranLogDAL.Add(tranLogAddList1);
            idal.SaveChanges();
            return "Y";
        }


        #region 集装箱出货返EDI

        //插入EDI任务表
        public void UrlEdiTaskInsert(string LoadId, string WhCode, string CreateUser)
        {
            List<FlowHead> getflowHeadList = (from a in idal.ILoadMasterDAL.SelectAll()
                                              join b in idal.IFlowHeadDAL.SelectAll()
                                              on a.ProcessId equals b.Id
                                              where a.WhCode == WhCode && a.LoadId == LoadId
                                              select b).ToList();

            if (getflowHeadList.Where(u => (u.UrlEdiId ?? 0) != 0).Count() > 0)
            {
                FlowHead first = getflowHeadList.Where(u => (u.UrlEdiId ?? 0) != 0).First();

                List<UrlEdi> urlList = idal.IUrlEdiDAL.SelectBy(u => u.Id == first.UrlEdiId).ToList();
                if (urlList.Count > 0)
                {
                    UrlEdi url = urlList.First();

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
            }
            if (getflowHeadList.Where(u => (u.UrlEdiId2 ?? 0) != 0).Count() > 0)
            {
                FlowHead first = getflowHeadList.Where(u => (u.UrlEdiId2 ?? 0) != 0).First();
                List<UrlEdi> urlList = idal.IUrlEdiDAL.SelectBy(u => u.Id == first.UrlEdiId2).ToList();

                if (urlList.Count > 0)
                {
                    UrlEdi url = urlList.First();

                    UrlEdiTask uet1 = new UrlEdiTask();
                    uet1.WhCode = WhCode;
                    uet1.Type = "OMS";
                    uet1.Url = url.Url + "&WhCode=" + WhCode;
                    uet1.Field = url.Field;
                    uet1.Mark = LoadId;
                    uet1.HttpType = url.HttpType;
                    uet1.Status = 1;
                    uet1.CreateDate = DateTime.Now;
                    idal.IUrlEdiTaskDAL.Add(uet1);
                }
            }

            if (getflowHeadList.Where(u => (u.UrlEdiId3 ?? 0) != 0).Count() > 0)
            {
                FlowHead first = getflowHeadList.Where(u => (u.UrlEdiId3 ?? 0) != 0).First();
                List<UrlEdi> urlList = idal.IUrlEdiDAL.SelectBy(u => u.Id == first.UrlEdiId3).ToList();

                if (urlList.Count > 0)
                {
                    UrlEdi url = urlList.First();

                    UrlEdiTask uet2 = new UrlEdiTask();
                    uet2.WhCode = WhCode;
                    uet2.Type = "OMS";
                    uet2.Url = url.Url + "&WhCode=" + WhCode;
                    uet2.Field = url.Field;
                    uet2.Mark = LoadId;
                    uet2.HttpType = url.HttpType;
                    uet2.Status = 1;
                    uet2.CreateDate = DateTime.Now;
                    idal.IUrlEdiTaskDAL.Add(uet2);
                }
            }

        }
        #endregion



    }
}

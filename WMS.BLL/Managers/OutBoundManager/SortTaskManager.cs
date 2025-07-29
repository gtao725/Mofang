using MODEL_MSSQL;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using WMS.BLLClass;
using WMS.IBLL;
using Newtonsoft.Json;

namespace WMS.BLL
{
    public class SortTaskManager : ISortTaskManager
    {

        IDAL.IDALSession idal = BLL.BLLHelper.GetDal();
        private static object o = new object();

        ShipHelper shipHelper = new ShipHelper();


        //创建分拣任务
        public string CreateSortTask(string loadId, string whCode, string userName)
        {
            if (loadId == null || loadId == "" || whCode == "" || whCode == null || userName == null || userName == "")
            {
                return "数据有误，请重新操作！";
            }

            List<SortTask> sortTaskList = idal.ISortTaskDAL.SelectBy(u => u.LoadId == loadId && u.WhCode == whCode);
            if (sortTaskList.Count > 0)
            {
                return "该Load:" + loadId + "已创建分拣任务！";
            }

            SortTask sortTask = new SortTask();
            sortTask.WhCode = whCode;
            sortTask.LoadId = loadId;
            sortTask.Status = "U";
            sortTask.CreateUser = userName;
            sortTask.CreateDate = DateTime.Now;
            idal.ISortTaskDAL.Add(sortTask);

            List<SortTaskDetailResult> SortTaskDetailResultList = (from a in idal.ILoadMasterDAL.SelectAll()
                                                                   join b in idal.ILoadDetailDAL.SelectAll() on new { Id = a.Id } equals new { Id = (Int32)b.LoadMasterId } into b_join
                                                                   from b in b_join.DefaultIfEmpty()
                                                                   join c in idal.IOutBoundOrderDAL.SelectAll() on new { OutBoundOrderId = (Int32)b.OutBoundOrderId } equals new { OutBoundOrderId = c.Id } into c_join
                                                                   from c in c_join.DefaultIfEmpty()
                                                                   join d in idal.IOutBoundOrderDetailDAL.SelectAll() on new { Id = c.Id } equals new { Id = d.OutBoundOrderId } into d_join
                                                                   from d in d_join.DefaultIfEmpty()
                                                                   join e in idal.IItemMasterDAL.SelectAll()
                                                                    on new { Id = d.ItemId } equals new { Id = e.Id } into e_join
                                                                   from e in e_join.DefaultIfEmpty()
                                                                   where a.LoadId == loadId && a.WhCode == whCode
                                                                   group new { a, c, d, e } by new
                                                                   {
                                                                       a.LoadId,
                                                                       a.WhCode,
                                                                       c.OutPoNumber,
                                                                       d.AltItemNumber,
                                                                       e.EAN,
                                                                       d.ItemId,
                                                                       e.HandFlag,
                                                                       e.ScanFlag,
                                                                       e.ScanRule
                                                                   } into g
                                                                   select new SortTaskDetailResult
                                                                   {
                                                                       LoadId = g.Key.LoadId,
                                                                       WhCode = g.Key.WhCode,
                                                                       OutPoNumber = g.Key.OutPoNumber,
                                                                       AltItemNumber = g.Key.AltItemNumber,
                                                                       EAN = g.Key.EAN,
                                                                       ItemId = g.Key.ItemId,
                                                                       HandFlag = g.Key.HandFlag ?? 0,
                                                                       ScanFlag = g.Key.ScanFlag ?? 0,
                                                                       ScanRule = g.Key.ScanRule ?? "",
                                                                       PlanQty = g.Sum(p => p.d.Qty)
                                                                   }).ToList();
            if (SortTaskDetailResultList.Count == 0)
            {
                return "创建分拣任务失败，请检查订单明细！";
            }

            int groupId = 0;
            Hashtable hs = new Hashtable();
            foreach (var item in SortTaskDetailResultList)
            {
                SortTaskDetail entity = new SortTaskDetail();
                entity.WhCode = item.WhCode;
                entity.LoadId = item.LoadId;
                if (hs.ContainsKey(item.OutPoNumber) == false)
                {
                    groupId++;
                    hs.Add(item.OutPoNumber, groupId);
                    entity.GroupId = groupId;
                }
                else
                {
                    entity.GroupId = Convert.ToInt32(hs[item.OutPoNumber].ToString());
                }

                entity.OutPoNumber = item.OutPoNumber;
                entity.AltItemNumber = item.AltItemNumber;
                entity.EAN = item.EAN;
                entity.ItemId = item.ItemId;
                entity.ScanFlag = (Int32)item.ScanFlag;
                if (string.IsNullOrEmpty(item.ScanRule))
                    entity.ScanRule = "0";
                else
                    entity.ScanRule = item.ScanRule;
                entity.PlanQty = (Int32)item.PlanQty;
                entity.PickQty = 0;
                entity.Qty = 0;
                entity.PackQty = 0;
                entity.TransferQty = 0;
                entity.HoldQty = 0;
                entity.HoldFlag = 0;
                entity.GroupNumber = "";
                if (entity.ScanFlag == 1)
                {
                    if (entity.PlanQty == 1)
                    {
                        entity.HandFlag = 0;
                    }
                    else
                    {
                        entity.HandFlag = 1;
                    }
                }
                else
                {
                    entity.HandFlag = (Int32)item.HandFlag;
                }
                entity.CreateUser = userName;
                entity.CreateDate = DateTime.Now;
                idal.ISortTaskDetailDAL.Add(entity);
            }

            return "Y";
        }




        //分拣工作台--------------------------------------------------
        //拉取 分拣头信息
        public List<SortTask> GetSortTaskList(string loadId, string whCode)
        {
            return idal.ISortTaskDAL.SelectBy(u => u.LoadId == loadId && u.WhCode == whCode && u.Status != "C").ToList();
        }

        //拉取 分拣明细信息
        public List<SortTaskDetail> GetSortTaskDetailList(string loadId, string whCode)
        {
            return idal.ISortTaskDetailDAL.SelectBy(u => u.LoadId == loadId && u.WhCode == whCode).ToList();
        }


        //拉取 分拣明细信息
        public List<SortTaskDetailResult> GetSumSortTaskDetailList(string loadId, string whCode)
        {
            var sql = from sorttaskdetail in idal.ISortTaskDetailDAL.SelectAll()
                      where
                        sorttaskdetail.LoadId == loadId &&
                        sorttaskdetail.WhCode == whCode
                      group sorttaskdetail by new
                      {
                          sorttaskdetail.LoadId
                      } into g
                      select new SortTaskDetailResult
                      {
                          LoadId = g.Key.LoadId,
                          PlanQty = (Int32?)g.Sum(p => p.PlanQty),
                          Qty = (Int32?)g.Sum(p => p.Qty),
                          HoldQty = (Int32?)g.Sum(p => p.HoldQty ?? 0),
                      };
            return sql.ToList();
        }

        //实时拉取分拣冻结数量
        public List<SortTaskDetailResult> GetHoldQtySortTaskDetailList(string loadId, string whCode)
        {
            var sql = from sorttaskdetail in idal.ISortTaskDetailDAL.SelectAll()
                      where
                        sorttaskdetail.LoadId == loadId &&
                        sorttaskdetail.WhCode == whCode && sorttaskdetail.PlanQty != sorttaskdetail.Qty && sorttaskdetail.HoldFlag == 1
                      group sorttaskdetail by new
                      {
                          sorttaskdetail.LoadId
                      } into g
                      select new SortTaskDetailResult
                      {
                          LoadId = g.Key.LoadId,
                          HoldQty = (Int32?)g.Sum(p => p.HoldQty ?? 0),
                      };
            return sql.ToList();
        }


        //根据不同的分拣款号输入类型 得到款号ID
        public int GetItemIdBySort(string loadId, string whCode, string itemString)
        {
            int itemId = 0;
            List<SortTaskDetail> sortTaskDetailList = idal.ISortTaskDetailDAL.SelectBy(u => u.LoadId == loadId && u.WhCode == whCode);
            if (sortTaskDetailList.Count != 0)
            {
                if (sortTaskDetailList.Where(u => u.EAN == itemString).Count() == 0)
                {
                    if (sortTaskDetailList.Where(u => u.AltItemNumber == itemString).Count() == 0)
                    {
                        int changeResult = Convert.ToInt32(itemString);
                        if (sortTaskDetailList.Where(u => u.ItemId == changeResult).Count() == 0)
                        {
                            itemId = 0;
                        }
                        else
                        {
                            itemId = (Int32)sortTaskDetailList.Where(u => u.ItemId == changeResult).First().ItemId;
                        }
                    }
                    else
                    {
                        itemId = (Int32)sortTaskDetailList.Where(u => u.AltItemNumber == itemString).First().ItemId;
                    }
                }
                else
                {
                    itemId = (Int32)sortTaskDetailList.Where(u => u.EAN == itemString).First().ItemId;
                }
            }
            return itemId;
        }

        //分拣工作台
        public string SortTaskScanning(string loadId, string whCode, string userName, string itemString, int Qty)
        {
            if (loadId == null || loadId == "" || whCode == "" || whCode == null || userName == null || userName == "")
            {
                return "数据有误，请重新操作！";
            }

            List<SortTask> sortTaskList = idal.ISortTaskDAL.SelectBy(u => u.LoadId == loadId && u.WhCode == whCode);
            if (sortTaskList.Where(u => u.Status == "C").Count() > 0)
            {
                return "该Load:" + loadId + "已完成分拣！";
            }

            int itemId = GetItemIdBySort(loadId, whCode, itemString);
            if (itemId == 0)
            {
                return "该款号:" + itemString + "有误！";
            }

            if (sortTaskList.Where(u => u.Status == "U").Count() > 0)
            {
                SortTask sortTask = sortTaskList.First();
                sortTask.Status = "A";
                sortTask.UpdateUser = userName;
                sortTask.UpdateDate = DateTime.Now;
                idal.ISortTaskDAL.UpdateBy(sortTask, u => u.Id == sortTask.Id, new string[] { "Status", "UpdateUser", "UpdateDate" });

                //更新分拣时间
                LoadMaster loadMaster = new LoadMaster();
                loadMaster.Status2 = "A";
                loadMaster.BeginSortDate = DateTime.Now;
                idal.ILoadMasterDAL.UpdateBy(loadMaster, u => u.LoadId == loadId && u.WhCode == whCode, new string[] { "Status2", "BeginSortDate" });
            }

            List<SortTaskDetail> sortTaskDetailList = idal.ISortTaskDetailDAL.SelectBy(u => u.LoadId == loadId && u.WhCode == whCode && u.PickQty != u.Qty && u.PickQty != 0 && u.ItemId == itemId);
            if (sortTaskDetailList.Count == 0)
            {
                return "未找到分拣明细！";
            }
            else
            {
                SortTaskDetail sortTaskDetail = sortTaskDetailList.First();
                if (sortTaskDetail.HandFlag == 0)
                {
                    sortTaskDetail.Qty = sortTaskDetail.Qty + Qty;
                }
                else
                {
                    sortTaskDetail.Qty = sortTaskDetail.Qty + Qty;
                }

                if (sortTaskDetail.Qty > sortTaskDetail.PickQty)
                {
                    SortTaskDetail sortTaskDetail1 = sortTaskDetailList.First();
                    return "可用数量为" + (sortTaskDetail1.PlanQty - sortTaskDetail1.PickQty) + "！";
                }

                sortTaskDetail.UpdateUser = userName;
                sortTaskDetail.UpdateDate = DateTime.Now;

                idal.ISortTaskDetailDAL.UpdateBy(sortTaskDetail, u => u.Id == sortTaskDetail.Id, new string[] { "Qty", "UpdateUser", "UpdateDate" });


                idal.ISortTaskDetailDAL.SaveChanges();


                var sql1 = from a in (
                                        (from sorttaskdetail in idal.ISortTaskDetailDAL.SelectAll()
                                         where
                                           sorttaskdetail.LoadId == loadId &&
                                           sorttaskdetail.WhCode == whCode &&
                                           sorttaskdetail.GroupNumber == ""
                                         group sorttaskdetail by new
                                         {
                                             sorttaskdetail.GroupId
                                         } into g
                                         select new
                                         {
                                             GroupId = (Int32?)g.Key.GroupId,
                                             PlanQty = (Int32?)g.Sum(p => p.PlanQty),
                                             Qty = (Int32?)g.Sum(p => p.Qty)
                                         }))
                           where a.PlanQty == a.Qty
                           select new SortTaskDetailResult
                           {
                               GroupId = a.GroupId,
                               PlanQty = a.PlanQty,
                               Qty = a.Qty
                           };

                List<SortTaskDetailResult> groupIdList = sql1.ToList();
                if (groupIdList.Count == 0)
                {
                    return "Y" + "组号：" + sortTaskDetail.GroupId;
                }
                else
                {
                    if (groupIdList.Where(u => u.GroupId == sortTaskDetail.GroupId).Count() > 0)
                    {
                        return "C" + sortTaskDetail.GroupId + "$组号：" + sortTaskDetail.GroupId;
                    }
                    else
                    {
                        return "Y" + "组号：" + sortTaskDetail.GroupId;
                    }

                }
            }

        }


        //完成分拣后  更新分拣订单号状态 及 框号信息
        //增加包装数据
        public string UpdateSortTaskGroupNumber(string loadId, string whCode, int groupId, string groupNumber, string userName)
        {
            if (groupId == 0 || groupNumber == "" || userName == "")
            {
                return "数据有误，请重新操作！";
            }

            groupNumber = groupNumber.Trim();

            string result = "";
            List<SortTaskDetail> CheckSortTaskDetail = idal.ISortTaskDetailDAL.SelectBy(u => u.LoadId == loadId && u.WhCode == whCode);

            List<SortTaskDetail> sortTaskDetailList = CheckSortTaskDetail.Where(u => u.GroupId == groupId).ToList();
            if (sortTaskDetailList.Count == 0)
            {
                return "未找到明细！组号：" + groupId;
            }

            //绑定框号时 需要验证框号是否存在分拣表中
            List<SortTaskDetail> checkGroupNumberList = idal.ISortTaskDetailDAL.SelectBy(u => u.WhCode == whCode && u.GroupNumber == groupNumber);
            string checkGroupNumber = "";
            if (checkGroupNumberList.Count > 0)
            {
                foreach (var item in checkGroupNumberList)
                {
                    if (item.PlanQty != item.PackQty && item.PlanQty != 0)
                    {
                        checkGroupNumber = "框号已被使用，请更换！";
                        break;
                    }
                }
            }

            if (checkGroupNumber != "")
            {
                return checkGroupNumber;
            }

            //绑定框号时 需要验证框号是否 在包装中被拦截
            List<PackTask> packTaskList = idal.IPackTaskDAL.SelectBy(u => u.WhCode == whCode && u.SortGroupNumber == groupNumber);
            if (packTaskList.Where(u => u.Status == -10).Count() > 0)
            {
                return "框号被冻结，因上个订单被拦截！";
            }

            SortTaskDetail sort = sortTaskDetailList.First();
            List<OutBoundOrder> eneityList = idal.IOutBoundOrderDAL.SelectBy(u => u.WhCode == whCode && u.OutPoNumber == sort.OutPoNumber);

            //-----订单完成分拣绑定框号时 发现被拦截
            //1.更改订单状态为已拦截待处理
            if (sortTaskDetailList.Where(u => u.HoldFlag == 1).Count() > 0)
            {
                //1.1 验证订单信息是否存在
                if (eneityList.Count != 0)
                {
                    OutBoundOrder eneity = eneityList.First();

                    //更新分拣明细中的框号
                    SortTaskDetail sortTaskDetail = new SortTaskDetail();
                    sortTaskDetail.GroupNumber = groupNumber;
                    sortTaskDetail.UpdateUser = userName;
                    sortTaskDetail.UpdateDate = DateTime.Now;
                    idal.ISortTaskDetailDAL.UpdateBy(sortTaskDetail, u => u.LoadId == loadId && u.WhCode == whCode && u.GroupId == groupId, new string[] { "GroupNumber", "UpdateUser", "UpdateDate" });


                    //如果检查到流程中有包装流程 就添加
                    //添加包装数据
                    List<FlowDetail> checkFlowDetailList = idal.IFlowDetailDAL.SelectBy(u => u.FlowHeadId == eneity.ProcessId && u.Type == "PackingType");
                    if (checkFlowDetailList.Count > 0)
                    {
                        List<PackTask> checkpackTaskList = idal.IPackTaskDAL.SelectBy(u => u.WhCode == whCode && u.LoadId == loadId && u.OutPoNumber == eneity.OutPoNumber && u.SortGroupId == groupId);
                        if (checkpackTaskList.Count == 0)
                        {
                            PackTask packTask = new PackTask();
                            packTask.WhCode = whCode;
                            packTask.LoadId = loadId;
                            packTask.SortGroupId = groupId; //分拣组号
                            packTask.SortGroupNumber = groupNumber; //分拣框号
                            packTask.CustomerOutPoNumber = eneity.CustomerOutPoNumber;  //客户出库订单号
                            packTask.OutPoNumber = eneity.OutPoNumber;      //系统出库订单号
                            packTask.Status = -10;                        //状态为-10：订单被拦截
                            packTask.CreateUser = userName;
                            packTask.CreateDate = DateTime.Now;
                            idal.IPackTaskDAL.Add(packTask);
                        }
                    }
                }

                //更新 分拣任务状态
                if (CheckSortTaskDetail.Where(u => u.HoldFlag != 1).Where(u => u.PlanQty != u.Qty).Count() == 0)
                {
                    SortTask sortTask = new SortTask();
                    sortTask.Status = "C";
                    sortTask.UpdateUser = userName;
                    sortTask.UpdateDate = DateTime.Now;
                    idal.ISortTaskDAL.UpdateBy(sortTask, u => u.LoadId == loadId && u.WhCode == whCode, new string[] { "Status", "UpdateUser", "UpdateDate" });

                    //更新分拣完成时间
                    LoadMaster loadMaster = new LoadMaster();
                    loadMaster.Status2 = "C";
                    loadMaster.EndSortDate = DateTime.Now;
                    idal.ILoadMasterDAL.UpdateBy(loadMaster, u => u.LoadId == loadId && u.WhCode == whCode, new string[] { "Status2", "EndSortDate" });
                }

                idal.ISortTaskDetailDAL.SaveChanges();
                return "H框号对应订单被拦截！";
            }
            else
            {
                using (TransactionScope trans = new TransactionScope())
                {
                    try
                    {
                        #region 分拣功能优化
                        if (eneityList.Count != 0)
                        {
                            OutBoundOrder eneity = eneityList.First();

                            if (eneity.StatusId > 15 && eneity.StatusId < 35) //订单状态必须为草稿以上
                            {
                                FlowDetail flowDetail = idal.IFlowDetailDAL.SelectBy(u => u.FlowHeadId == eneity.ProcessId && u.Type == "Sorting").First();

                                if (flowDetail != null && flowDetail.StatusId != 0)
                                {
                                    string remark1 = "原状态：" + eneity.StatusId + eneity.StatusName;

                                    //更新出库订单状态为已分拣
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
                                    tlorder.TranUser = userName;
                                    tlorder.WhCode = whCode;
                                    tlorder.LoadId = loadId;
                                    tlorder.CustomerOutPoNumber = eneity.CustomerOutPoNumber;
                                    tlorder.OutPoNumber = eneity.OutPoNumber;
                                    tlorder.Remark = remark1 + "变更为：" + eneity.StatusId + eneity.StatusName;
                                    idal.ITranLogDAL.Add(tlorder);


                                    //更新分拣明细中的框号
                                    SortTaskDetail sortTaskDetail = new SortTaskDetail();
                                    sortTaskDetail.GroupNumber = groupNumber;
                                    sortTaskDetail.UpdateUser = userName;
                                    sortTaskDetail.UpdateDate = DateTime.Now;
                                    idal.ISortTaskDetailDAL.UpdateBy(sortTaskDetail, u => u.LoadId == loadId && u.WhCode == whCode && u.GroupId == groupId, new string[] { "GroupNumber", "UpdateUser", "UpdateDate" });


                                    //如果检查到流程中有包装流程 就添加
                                    //添加包装数据
                                    List<FlowDetail> checkFlowDetailList = idal.IFlowDetailDAL.SelectBy(u => u.FlowHeadId == eneity.ProcessId && u.Type == "PackingType");
                                    if (checkFlowDetailList.Count > 0)
                                    {
                                        List<PackTask> checkpackTaskList = idal.IPackTaskDAL.SelectBy(u => u.WhCode == whCode && u.LoadId == loadId && u.OutPoNumber == eneity.OutPoNumber && u.SortGroupId == groupId);
                                        if (checkpackTaskList.Count == 0)
                                        {
                                            List<PackTaskJson> getPackTaskJson = idal.IPackTaskJsonDAL.SelectBy(u => u.WhCode == whCode && u.CustomerOutPoNumber == eneity.CustomerOutPoNumber);
                                            if (getPackTaskJson.Count == 0)
                                            {
                                                PackTask packTask = new PackTask();
                                                packTask.WhCode = whCode;
                                                packTask.LoadId = loadId;
                                                packTask.SortGroupId = groupId; //分拣组号
                                                packTask.SortGroupNumber = groupNumber; //分拣框号
                                                packTask.CustomerOutPoNumber = eneity.CustomerOutPoNumber;  //客户出库订单号
                                                packTask.OutPoNumber = eneity.OutPoNumber;      //系统出库订单号
                                                packTask.Status = 0;                        //状态为0：未获取物流信息
                                                packTask.CreateUser = userName;
                                                packTask.CreateDate = DateTime.Now;
                                                idal.IPackTaskDAL.Add(packTask);
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
                                                packTask.LoadId = loadId;
                                                packTask.SortGroupId = groupId; //分拣组号
                                                packTask.SortGroupNumber = groupNumber; //分拣框号
                                                packTask.CustomerOutPoNumber = eneity.CustomerOutPoNumber;  //客户出库订单号
                                                packTask.OutPoNumber = eneity.OutPoNumber;      //系统出库订单号
                                                packTask.Status = 10;                        //状态为0：未获取物流信息
                                                packTask.CreateUser = userName;
                                                packTask.CreateDate = DateTime.Now;
                                                idal.IPackTaskDAL.Add(packTask);
                                            }

                                        }
                                    }
                                }
                                else
                                {
                                    result = "错误！获取订单状态有误！";
                                }
                            }
                        }

                        if (result != "")
                        {
                            return result;
                        }

                        //更新 分拣任务状态
                        if (CheckSortTaskDetail.Where(u => u.PlanQty != u.Qty).Count() == 0)
                        {
                            SortTask sortTask = new SortTask();
                            sortTask.Status = "C";
                            sortTask.UpdateUser = userName;
                            sortTask.UpdateDate = DateTime.Now;
                            idal.ISortTaskDAL.UpdateBy(sortTask, u => u.LoadId == loadId && u.WhCode == whCode, new string[] { "Status", "UpdateUser", "UpdateDate" });

                            //更新分拣完成时间
                            LoadMaster loadMaster = new LoadMaster();
                            loadMaster.Status2 = "C";
                            loadMaster.EndSortDate = DateTime.Now;
                            idal.ILoadMasterDAL.UpdateBy(loadMaster, u => u.LoadId == loadId && u.WhCode == whCode, new string[] { "Status2", "EndSortDate" });
                        }
                        #endregion

                        idal.ISortTaskDetailDAL.SaveChanges();
                        trans.Complete();

                        return "Y绑定成功！";
                    }
                    catch
                    {
                        trans.Dispose();//出现异常，事务手动释放
                        return "分拣异常，请重新提交！";
                    }
                }
            }

        }



        //分拣工作台 查询分拣明细
        public List<SortTaskDetailSelectResult> SelectSortTaskDetailList(SortTaskDetailSearch searchEntity, out int total)
        {
            var sql = from a in idal.ISortTaskDAL.SelectAll()
                      join b in idal.ISortTaskDetailDAL.SelectAll()
                            on new { a.LoadId, a.WhCode }
                        equals new { b.LoadId, b.WhCode } into b_join
                      from b in b_join.DefaultIfEmpty()
                      join c in idal.IOutBoundOrderDAL.SelectAll()
                            on new { b.OutPoNumber, a.WhCode }
                         equals new { c.OutPoNumber, c.WhCode } into c_join
                      from c in c_join.DefaultIfEmpty()
                      where a.WhCode == searchEntity.WhCode && b.PlanQty == b.PickQty && b.PackQty == 0
                      select new SortTaskDetailSelectResult
                      {
                          LoadId = a.LoadId,
                          Status = c.StatusName,
                          GroupId = b.GroupId,
                          OutPoNumber = b.OutPoNumber,
                          AltItemNumber = b.AltItemNumber,
                          EAN = b.EAN,
                          PlanQty = b.PlanQty,
                          Qty = b.Qty,
                          GroupNumber = b.GroupNumber,
                          CreateDate = a.CreateDate
                      };
            if (!string.IsNullOrEmpty(searchEntity.LoadId))
            {
                sql = sql.Where(u => u.LoadId == searchEntity.LoadId);
            }
            if (!string.IsNullOrEmpty(searchEntity.AltItemNumber))
            {
                sql = sql.Where(u => u.AltItemNumber == searchEntity.AltItemNumber);
            }
            if (!string.IsNullOrEmpty(searchEntity.SortGroupNumber))
            {
                sql = sql.Where(u => u.GroupNumber == searchEntity.SortGroupNumber);
            }
            if (searchEntity.CreateDateBegin != null)
            {
                sql = sql.Where(u => u.CreateDate >= searchEntity.CreateDateBegin);
            }
            if (searchEntity.CreateDateEnd != null)
            {
                sql = sql.Where(u => u.CreateDate <= searchEntity.CreateDateEnd);
            }


            total = sql.Count();
            sql = sql.OrderBy(u => u.LoadId).ThenBy(u => u.GroupId).ThenBy(u => u.OutPoNumber);
            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);
            return sql.ToList();
        }


        //更新框号信息
        public string UpdateSortTaskDetail(string loadId, string whCode, int groupId, string groupNumber, string userName)
        {
            if (groupId == 0 || groupNumber == "" || userName == "")
            {
                return "数据有误，请重新操作！";
            }

            string result = "";
            List<SortTaskDetail> CheckSortTaskDetail = idal.ISortTaskDetailDAL.SelectBy(u => u.LoadId == loadId && u.WhCode == whCode);

            List<SortTaskDetail> sortTaskDetailList = CheckSortTaskDetail.Where(u => u.GroupId == groupId).ToList();
            if (sortTaskDetailList.Count == 0)
            {
                return "未找到明细！组号：" + groupId;
            }

            //绑定框号时 需要验证框号是否存在分拣表中
            List<SortTaskDetail> checkGroupNumberList = idal.ISortTaskDetailDAL.SelectBy(u => u.WhCode == whCode && u.GroupNumber == groupNumber);
            string checkGroupNumber = "";
            if (checkGroupNumberList.Count > 0)
            {
                foreach (var item in checkGroupNumberList)
                {
                    if (item.PlanQty != item.PackQty && item.PlanQty != 0)
                    {
                        checkGroupNumber = "框号已被使用，请更换！";
                        break;
                    }
                }
            }
            if (checkGroupNumber != "")
            {
                return checkGroupNumber;
            }

            //绑定框号时 需要验证框号是否 在包装中被拦截
            List<PackTask> packTaskList = idal.IPackTaskDAL.SelectBy(u => u.WhCode == whCode && u.SortGroupNumber == groupNumber);
            if (packTaskList.Where(u => u.Status == -10).Count() > 0)
            {
                return "框号被冻结，因上个订单被拦截！";
            }

            using (TransactionScope trans = new TransactionScope())
            {
                try
                {
                    #region 分拣优化

                    SortTaskDetail sort = sortTaskDetailList.First();

                    List<OutBoundOrder> eneityList = idal.IOutBoundOrderDAL.SelectBy(u => u.WhCode == whCode && u.OutPoNumber == sort.OutPoNumber);

                    //-----订单完成分拣绑定框号时 发现被拦截
                    //1.更改订单状态为已拦截待处理
                    if (sortTaskDetailList.Where(u => u.HoldFlag == 1).Count() > 0)
                    {
                        //1.1 验证订单信息是否存在
                        if (eneityList.Count != 0)
                        {
                            OutBoundOrder eneity = eneityList.First();

                            //如果检查到流程中有包装流程 就添加
                            //添加包装数据
                            List<FlowDetail> checkFlowDetailList = idal.IFlowDetailDAL.SelectBy(u => u.FlowHeadId == eneity.ProcessId && u.Type == "PackingType");
                            if (checkFlowDetailList.Count > 0)
                            {
                                //重复绑定框号时 需要验证
                                List<PackTask> checkPackTaskList = idal.IPackTaskDAL.SelectBy(u => u.WhCode == whCode && u.LoadId == loadId && u.SortGroupId == groupId && u.SortGroupNumber == sort.GroupNumber && u.CustomerOutPoNumber == eneity.CustomerOutPoNumber);
                                if (checkPackTaskList.Count > 0)
                                {
                                    if (checkPackTaskList.Where(u => u.Status == -10 || u.Status == 0).Count() > 0)
                                    {
                                        PackTask packTask = checkPackTaskList.Where(u => u.Status == -10 || u.Status == 0).First();
                                        packTask.SortGroupNumber = groupNumber;
                                        packTask.UpdateUser = userName;
                                        packTask.UpdateDate = DateTime.Now;
                                        idal.IPackTaskDAL.UpdateBy(packTask, u => u.Id == packTask.Id, new string[] { "SortGroupNumber", "UpdateUser", "UpdateDate" });
                                    }
                                    else
                                    {
                                        return "分拣框号被拦截，无需修改，请查询！";
                                    }
                                }
                                else
                                {
                                    PackTask packTask = new PackTask();
                                    packTask.WhCode = whCode;
                                    packTask.LoadId = loadId;
                                    packTask.SortGroupId = groupId; //分拣组号
                                    packTask.SortGroupNumber = groupNumber; //分拣框号
                                    packTask.CustomerOutPoNumber = eneity.CustomerOutPoNumber;  //客户出库订单号
                                    packTask.OutPoNumber = eneity.OutPoNumber;      //系统出库订单号
                                    packTask.Status = -10;                        //状态为-10：订单被拦截
                                    packTask.CreateUser = userName;
                                    packTask.CreateDate = DateTime.Now;
                                    idal.IPackTaskDAL.Add(packTask);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (eneityList.Count != 0)
                        {
                            OutBoundOrder eneity = eneityList.First();

                            if (eneity.StatusId > 15 && eneity.StatusId < 35) //订单状态必须为草稿以上
                            {
                                FlowDetail flowDetail = idal.IFlowDetailDAL.SelectBy(u => u.FlowHeadId == eneity.ProcessId && u.Type == "Sorting").First();

                                if (flowDetail != null && flowDetail.StatusId != 0)
                                {
                                    string remark1 = "原状态：" + eneity.StatusId + eneity.StatusName;

                                    //更新出库订单状态为已分拣
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
                                    tlorder.TranUser = userName;
                                    tlorder.WhCode = whCode;
                                    tlorder.LoadId = loadId;
                                    tlorder.CustomerOutPoNumber = eneity.CustomerOutPoNumber;
                                    tlorder.OutPoNumber = eneity.OutPoNumber;
                                    tlorder.Remark = remark1 + "变更为：" + eneity.StatusId + eneity.StatusName;
                                    idal.ITranLogDAL.Add(tlorder);


                                    //如果检查到流程中有包装流程 就添加
                                    //添加包装数据
                                    List<FlowDetail> checkFlowDetailList = idal.IFlowDetailDAL.SelectBy(u => u.FlowHeadId == eneity.ProcessId && u.Type == "PackingType");
                                    if (checkFlowDetailList.Count > 0)
                                    {
                                        //重复绑定框号时 需要验证
                                        List<PackTask> checkPackTaskList = idal.IPackTaskDAL.SelectBy(u => u.WhCode == whCode && u.LoadId == loadId && u.SortGroupId == groupId && u.SortGroupNumber == sort.GroupNumber && u.CustomerOutPoNumber == eneity.CustomerOutPoNumber);

                                        List<PackTaskJson> getPackTaskJson = idal.IPackTaskJsonDAL.SelectBy(u => u.WhCode == whCode && u.CustomerOutPoNumber == eneity.CustomerOutPoNumber);

                                        PackTaskJson packJson = new PackTaskJson();
                                        PackTaskJsonEntity packTaskJsonEntity = new PackTaskJsonEntity();

                                        if (checkPackTaskList.Count > 0)
                                        {
                                            if (getPackTaskJson.Count > 0)
                                            {
                                                packJson = getPackTaskJson.First();
                                                packTaskJsonEntity = JsonConvert.DeserializeObject<PackTaskJsonEntity>(packJson.Json);
                                            }

                                            if (checkPackTaskList.Where(u => u.Status == -10 || u.Status == 0).Count() > 0)
                                            {
                                                PackTask packTask = checkPackTaskList.Where(u => u.Status == -10 || u.Status == 0).First();
                                                packTask.SortGroupNumber = groupNumber;
                                                packTask.UpdateUser = userName;
                                                packTask.UpdateDate = DateTime.Now;

                                                packTask.TransportType = packTaskJsonEntity.TransportType;
                                                packTask.express_code = packTaskJsonEntity.express_code;
                                                packTask.express_type = packTaskJsonEntity.express_type;
                                                packTask.express_type_zh = packTaskJsonEntity.express_type_zh;
                                                packTask.PackMoreFlag = packTaskJsonEntity.PackMoreFlag;
                                                packTask.SingleFlag = packTaskJsonEntity.SingleFlag;
                                                if (packTask.SingleFlag == 1)
                                                {
                                                    packTask.PackQty = 1;
                                                }
                                                packTask.ZdFlag = packTaskJsonEntity.ZdFlag;
                                                packTask.SinglePlaneTemplate = packTaskJsonEntity.SinglePlaneTemplate;
                                                packTask.PackingListTemplate = packTaskJsonEntity.PackingListTemplate;
                                                if (packTask.Status == 0)
                                                {
                                                    packTask.Status = 10;
                                                    idal.IPackTaskDAL.UpdateBy(packTask, u => u.Id == packTask.Id, new string[] { "SortGroupNumber", "UpdateUser", "UpdateDate", "TransportType", "express_code", "express_type", "express_type_zh", "SingleFlag", "ZdFlag", "SinglePlaneTemplate", "PackingListTemplate", "Status", "PackMoreFlag", "PackQty" });
                                                }
                                                else
                                                {
                                                    idal.IPackTaskDAL.UpdateBy(packTask, u => u.Id == packTask.Id, new string[] { "SortGroupNumber", "UpdateUser", "UpdateDate", "TransportType", "express_code", "express_type", "express_type_zh", "SingleFlag", "ZdFlag", "SinglePlaneTemplate", "PackingListTemplate", "PackMoreFlag", "PackQty" });
                                                }
                                            }
                                            else
                                            {
                                                PackTask packTask = checkPackTaskList.First();
                                                if (packTask.Status == 30)
                                                {
                                                    return "分拣框号已完成包装，无法修改！";
                                                }
                                                else if (packTask.Status == 10 || packTask.Status == 20)
                                                {
                                                    List<PackHead> checklist = idal.IPackHeadDAL.SelectBy(u => u.PackTaskId == packTask.Id);
                                                    if (checklist.Count > 0)
                                                    {
                                                        return "分拣框号存在包装信息，如需修改请删除所有包装信息！";
                                                    }
                                                    else
                                                    {
                                                        packTask.SortGroupNumber = groupNumber;
                                                        packTask.UpdateUser = userName;
                                                        packTask.UpdateDate = DateTime.Now;

                                                        packTask.TransportType = packTaskJsonEntity.TransportType;
                                                        packTask.express_code = packTaskJsonEntity.express_code;
                                                        packTask.express_type = packTaskJsonEntity.express_type;
                                                        packTask.express_type_zh = packTaskJsonEntity.express_type_zh;
                                                        packTask.PackMoreFlag = packTaskJsonEntity.PackMoreFlag;
                                                        packTask.SingleFlag = packTaskJsonEntity.SingleFlag;
                                                        if (packTask.SingleFlag == 1)
                                                        {
                                                            packTask.PackQty = 1;
                                                        }
                                                        packTask.ZdFlag = packTaskJsonEntity.ZdFlag;
                                                        packTask.SinglePlaneTemplate = packTaskJsonEntity.SinglePlaneTemplate;
                                                        packTask.PackingListTemplate = packTaskJsonEntity.PackingListTemplate;

                                                        idal.IPackTaskDAL.UpdateBy(packTask, u => u.Id == packTask.Id, new string[] { "SortGroupNumber", "UpdateUser", "UpdateDate", "TransportType", "express_code", "express_type", "express_type_zh", "SingleFlag", "ZdFlag", "SinglePlaneTemplate", "PackingListTemplate", "PackMoreFlag", "PackQty" });

                                                    }
                                                }

                                            }
                                        }
                                        else
                                        {

                                            if (getPackTaskJson.Count == 0)
                                            {
                                                PackTask packTask = new PackTask();
                                                packTask.WhCode = whCode;
                                                packTask.LoadId = loadId;
                                                packTask.SortGroupId = groupId; //分拣组号
                                                packTask.SortGroupNumber = groupNumber; //分拣框号
                                                packTask.CustomerOutPoNumber = eneity.CustomerOutPoNumber;  //客户出库订单号
                                                packTask.OutPoNumber = eneity.OutPoNumber;      //系统出库订单号
                                                packTask.Status = 0;                        //状态为0：未获取物流信息
                                                packTask.CreateUser = userName;
                                                packTask.CreateDate = DateTime.Now;
                                                idal.IPackTaskDAL.Add(packTask);
                                            }
                                            else
                                            {
                                                PackTask packTask = new PackTask();

                                                packTask.TransportType = packTaskJsonEntity.TransportType;
                                                packTask.express_code = packTaskJsonEntity.express_code;
                                                packTask.express_type = packTaskJsonEntity.express_type;
                                                packTask.express_type_zh = packTaskJsonEntity.express_type_zh;
                                                packTask.SingleFlag = packTaskJsonEntity.SingleFlag;
                                                if (packTask.SingleFlag == 1)
                                                {
                                                    packTask.PackQty = 1;
                                                }
                                                packTask.PackMoreFlag = packTaskJsonEntity.PackMoreFlag;
                                                packTask.ZdFlag = packTaskJsonEntity.ZdFlag;
                                                packTask.SinglePlaneTemplate = packTaskJsonEntity.SinglePlaneTemplate;
                                                packTask.PackingListTemplate = packTaskJsonEntity.PackingListTemplate;

                                                packTask.WhCode = whCode;
                                                packTask.LoadId = loadId;
                                                packTask.SortGroupId = groupId; //分拣组号
                                                packTask.SortGroupNumber = groupNumber; //分拣框号
                                                packTask.CustomerOutPoNumber = eneity.CustomerOutPoNumber;  //客户出库订单号
                                                packTask.OutPoNumber = eneity.OutPoNumber;      //系统出库订单号
                                                packTask.Status = 10;                        //状态为0：未获取物流信息
                                                packTask.CreateUser = userName;
                                                packTask.CreateDate = DateTime.Now;
                                                idal.IPackTaskDAL.Add(packTask);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    result = "错误！获取订单状态有误！";
                                }
                            }
                        }
                    }

                    if (result != "")
                    {
                        return result;
                    }

                    //更新分拣明细中的框号
                    SortTaskDetail sortTaskDetail = new SortTaskDetail();
                    sortTaskDetail.GroupNumber = groupNumber;
                    sortTaskDetail.UpdateUser = userName;
                    sortTaskDetail.UpdateDate = DateTime.Now;
                    idal.ISortTaskDetailDAL.UpdateBy(sortTaskDetail, u => u.LoadId == loadId && u.WhCode == whCode && u.GroupId == groupId, new string[] { "GroupNumber", "UpdateUser", "UpdateDate" });

                    //更新 分拣任务状态
                    if (CheckSortTaskDetail.Where(u => u.HoldFlag != 1).Where(u => u.PlanQty != u.Qty).Count() == 0)
                    {
                        SortTask sortTask = new SortTask();
                        sortTask.Status = "C";
                        sortTask.UpdateUser = userName;
                        sortTask.UpdateDate = DateTime.Now;
                        idal.ISortTaskDAL.UpdateBy(sortTask, u => u.LoadId == loadId && u.WhCode == whCode, new string[] { "Status", "UpdateUser", "UpdateDate" });

                        //更新分拣完成时间
                        LoadMaster loadMaster = new LoadMaster();
                        loadMaster.Status2 = "C";
                        loadMaster.EndSortDate = DateTime.Now;
                        idal.ILoadMasterDAL.UpdateBy(loadMaster, u => u.LoadId == loadId && u.WhCode == whCode, new string[] { "Status2", "EndSortDate" });
                    }

                    #endregion

                    idal.ISortTaskDetailDAL.SaveChanges();
                    trans.Complete();
                    return "Y";
                }
                catch
                {
                    trans.Dispose();//出现异常，事务手动释放
                    return "分拣异常，请重新提交！";
                }
            }

        }


        public string AddPackTask(string loadId, string whCode, string userName)
        {
            List<SortTaskDetail> list = idal.ISortTaskDetailDAL.SelectBy(u => u.WhCode == whCode && u.LoadId == loadId).OrderByDescending(u => u.GroupId).Skip((200) * (10 - 1)).Take(200).ToList();

            List<PackTask> ListAdd = new List<PackTask>();

            string[] OutPoNumber = (from a in list select a.OutPoNumber).ToList().Distinct().ToArray();

            List<OutBoundOrder> outBoundList = idal.IOutBoundOrderDAL.SelectBy(u => u.WhCode == whCode && OutPoNumber.Contains(u.OutPoNumber));

            foreach (var sort in list)
            {
                if (ListAdd.Where(u => u.WhCode == whCode && u.LoadId == loadId && u.OutPoNumber == sort.OutPoNumber && u.SortGroupId == sort.GroupId).Count() == 0)
                {
                    List<PackTask> checkpackTaskList = idal.IPackTaskDAL.SelectBy(u => u.WhCode == whCode && u.LoadId == loadId && u.OutPoNumber == sort.OutPoNumber && u.SortGroupId == sort.GroupId);

                    OutBoundOrder outBound = outBoundList.Where(u => u.OutPoNumber == sort.OutPoNumber).First();

                    if (checkpackTaskList.Count == 0)
                    {
                        List<PackTaskJson> getPackTaskJson = idal.IPackTaskJsonDAL.SelectBy(u => u.WhCode == whCode && u.CustomerOutPoNumber == outBound.CustomerOutPoNumber);
                        if (getPackTaskJson.Count == 0)
                        {
                            PackTask packTask = new PackTask();
                            packTask.WhCode = whCode;
                            packTask.LoadId = loadId;
                            packTask.SortGroupId = sort.GroupId; //分拣组号
                            packTask.SortGroupNumber = sort.GroupNumber; //分拣框号
                            packTask.CustomerOutPoNumber = outBound.CustomerOutPoNumber;  //客户出库订单号
                            packTask.OutPoNumber = outBound.OutPoNumber;      //系统出库订单号
                            packTask.Status = 0;                        //状态为0：未获取物流信息
                            packTask.CreateUser = userName;
                            packTask.CreateDate = DateTime.Now;
                            ListAdd.Add(packTask);
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
                            packTask.LoadId = loadId;
                            packTask.SortGroupId = sort.GroupId; //分拣组号
                            packTask.SortGroupNumber = sort.GroupNumber; //分拣框号
                            packTask.CustomerOutPoNumber = outBound.CustomerOutPoNumber;  //客户出库订单号
                            packTask.OutPoNumber = outBound.OutPoNumber;      //系统出库订单号
                            packTask.Status = 10;                        //状态为0：未获取物流信息
                            packTask.CreateUser = userName;
                            packTask.CreateDate = DateTime.Now;
                            ListAdd.Add(packTask);
                        }

                    }

                }
            }

            idal.IPackTaskDAL.Add(ListAdd);
            idal.IPackTaskDAL.SaveChanges();
            return "Y";

        }
    }

}

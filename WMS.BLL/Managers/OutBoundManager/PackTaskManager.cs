using MODEL_MSSQL;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Transactions;
using WMS.BLLClass;
using WMS.Express;
using WMS.IBLL;

namespace WMS.BLL
{
    public class PackTaskManager : IPackTaskManager
    {

        IDAL.IDALSession idal = BLL.BLLHelper.GetDal();
        private static object o = new object();

        private static object o1 = new object();

        private static object o2 = new object();

        private static object o3 = new object();

        private static List<StaticLoadPackTaskDetail> staticLoadPackTaskDetailList = new List<StaticLoadPackTaskDetail>();
        public static List<StaticLoadPackCount> staticLoadPackCountList = new List<StaticLoadPackCount>();

        public static List<PackTask> staticPackTaskList = new List<PackTask>();
        public static List<StaticLoadPackCount> staticPackTaskStatusList = new List<StaticLoadPackCount>();

        public List<WhClient> WhClientListSelect(string whCode)
        {
            var sql = from a in idal.IWhClientDAL.SelectAll()
                      where a.Status == "Active" && a.WhCode == whCode
                      select a;
            sql = sql.OrderBy(u => u.ClientCode);
            return sql.ToList();
        }

        #region 单品包装

        //验证 Load号是否正确
        public List<PackTask> GetPackTaskListByLoad(string loadId, string whCode)
        {
            List<PackTask> PackTaskList = idal.IPackTaskDAL.SelectBy(u => u.LoadId == loadId && u.WhCode == whCode && u.Status == 0).ToList();

            if (PackTaskList.Count > 0)
            {
                string[] CustomerOutPoNumber = (from a in PackTaskList
                                                select a.CustomerOutPoNumber).ToList().Distinct().ToArray();
                //2.一次性拉取订单Json列表
                List<PackTaskJson> packTaskJsonList = (from a in idal.IPackTaskJsonDAL.SelectAll()
                                                       where a.WhCode == whCode && CustomerOutPoNumber.Contains(a.CustomerOutPoNumber)
                                                       select a).ToList();
                foreach (var item in PackTaskList)
                {
                    if (packTaskJsonList.Where(u => u.CustomerOutPoNumber == item.CustomerOutPoNumber).Count() > 0)
                    {
                        PackTaskJson packJson = packTaskJsonList.Where(u => u.CustomerOutPoNumber == item.CustomerOutPoNumber).First();

                        PackTaskJsonEntity packTaskJsonEntity = JsonConvert.DeserializeObject<PackTaskJsonEntity>(packJson.Json);

                        PackTask pack = item;
                        pack.Status = 10;
                        pack.TransportType = packTaskJsonEntity.TransportType;
                        pack.express_code = packTaskJsonEntity.express_code;
                        pack.express_type = packTaskJsonEntity.express_type;
                        pack.express_type_zh = packTaskJsonEntity.express_type_zh;
                        pack.PackMoreFlag = packTaskJsonEntity.PackMoreFlag;
                        pack.SingleFlag = packTaskJsonEntity.SingleFlag;
                        if (pack.SingleFlag == 1)
                        {
                            pack.PackQty = 1;
                        }
                        pack.ZdFlag = packTaskJsonEntity.ZdFlag;
                        pack.SinglePlaneTemplate = packTaskJsonEntity.SinglePlaneTemplate;
                        pack.PackingListTemplate = packTaskJsonEntity.PackingListTemplate;

                        idal.IPackTaskDAL.UpdateBy(pack, u => u.Id == pack.Id, new string[] { "Status", "TransportType", "express_code", "express_type", "express_type_zh", "SingleFlag", "ZdFlag", "SinglePlaneTemplate", "PackingListTemplate", "PackMoreFlag", "PackQty" });
                    }
                }
                idal.SaveChanges();
            }

            List<PackTask> PackTaskList1 = idal.IPackTaskDAL.SelectBy(u => u.LoadId == loadId && u.WhCode == whCode && (u.TransportType ?? "") == "").ToList();

            if (PackTaskList1.Count > 0)
            {
                string[] CustomerOutPoNumber = (from a in PackTaskList1
                                                select a.CustomerOutPoNumber).ToList().Distinct().ToArray();
                //2.一次性拉取订单Json列表
                List<PackTaskJson> packTaskJsonList = (from a in idal.IPackTaskJsonDAL.SelectAll()
                                                       where a.WhCode == whCode && CustomerOutPoNumber.Contains(a.CustomerOutPoNumber)
                                                       select a).ToList();
                foreach (var item in PackTaskList1)
                {
                    if (packTaskJsonList.Where(u => u.CustomerOutPoNumber == item.CustomerOutPoNumber).Count() > 0)
                    {
                        PackTaskJson packJson = packTaskJsonList.Where(u => u.CustomerOutPoNumber == item.CustomerOutPoNumber).First();

                        PackTaskJsonEntity packTaskJsonEntity = JsonConvert.DeserializeObject<PackTaskJsonEntity>(packJson.Json);

                        PackTask pack = item;
                        pack.TransportType = packTaskJsonEntity.TransportType;
                        pack.express_code = packTaskJsonEntity.express_code;
                        pack.express_type = packTaskJsonEntity.express_type;
                        pack.express_type_zh = packTaskJsonEntity.express_type_zh;
                        pack.PackMoreFlag = packTaskJsonEntity.PackMoreFlag;
                        pack.SingleFlag = packTaskJsonEntity.SingleFlag;
                        if (pack.SingleFlag == 1)
                        {
                            pack.PackQty = 1;
                        }
                        pack.ZdFlag = packTaskJsonEntity.ZdFlag;
                        pack.SinglePlaneTemplate = packTaskJsonEntity.SinglePlaneTemplate;
                        pack.PackingListTemplate = packTaskJsonEntity.PackingListTemplate;

                        idal.IPackTaskDAL.UpdateBy(pack, u => u.Id == pack.Id, new string[] { "TransportType", "express_code", "express_type", "express_type_zh", "SingleFlag", "ZdFlag", "SinglePlaneTemplate", "PackingListTemplate", "PackMoreFlag", "PackQty" });
                    }
                }

                idal.SaveChanges();
            }

            return idal.IPackTaskDAL.SelectBy(u => u.LoadId == loadId && u.WhCode == whCode && (u.Status == 10 || u.Status == 20) && u.SingleFlag == 1).ToList();
        }

        //通过 当前包装扫描的Load号 获得 其分拣明细
        public List<SortTaskDetail> GetSortTaskDetailByLoad(string loadId, string whCode)
        {
            var sql = from a in idal.IPackTaskDAL.SelectAll()
                      where a.SingleFlag == 1 && a.LoadId == loadId && a.WhCode == whCode
                      join sorttaskdetail in idal.ISortTaskDetailDAL.SelectAll()
                      on new { A = a.LoadId, B = a.WhCode, C = a.OutPoNumber } equals new { A = sorttaskdetail.LoadId, B = sorttaskdetail.WhCode, C = sorttaskdetail.OutPoNumber }
                      where sorttaskdetail.PlanQty != sorttaskdetail.PackQty
                      select sorttaskdetail;

            return sql.ToList();
        }

        public List<SortTaskDetail> GetSortTaskDetailByLoadOrder(string loadId, string whCode)
        {
            var sql = from a in idal.IPackTaskDAL.SelectAll()
                      where a.SingleFlag != 1 && a.LoadId == loadId && a.WhCode == whCode
                      join sorttaskdetail in idal.ISortTaskDetailDAL.SelectAll()
                      on new { A = a.LoadId, B = a.WhCode, C = a.OutPoNumber } equals new { A = sorttaskdetail.LoadId, B = sorttaskdetail.WhCode, C = sorttaskdetail.OutPoNumber }
                      where sorttaskdetail.PlanQty != sorttaskdetail.PackQty
                      select sorttaskdetail;

            return sql.ToList();
        }

        public string DoTaskTest(string loadId, string whCode, string altItemNumber, string EAN)
        {

            var sql = from a in idal.ISortTaskDetailDAL.SelectAll()
                          //where a.PlanQty != a.PackQty && (a.AltItemNumber == altItemNumber || a.EAN == EAN)
                      where a.PlanQty != a.PackQty && (a.EAN == EAN)
                          && a.LoadId == loadId && a.WhCode == whCode
                      select new PackDetailInsert
                      {
                          AltItemNumber = altItemNumber,
                          EAN = EAN,
                          Qty = a.Qty,
                          ItemId = a.ItemId
                      };

            PackTaskInsert packT = new PackTaskInsert();
            packT.LoadId = loadId;
            packT.WhCode = whCode;
            packT.userName = "1536";
            packT.PackDetail = sql.Take(1).ToList();

            //idal.IHuDetailDAL.DeleteByExtended(u =>  u.Id== 671);

            //return "111";
            //Name aa = new Name();
            //aa.Name1 = "测试";

            //NameAdd b = new NameAdd();
            //b.NameId = aa.Id;
            //b.Add = "测试地址";
            //// a.NameAdds = b;

            //idal.INameDAL.Add(aa);
            //idal.INameAddDAL.Add(b);
            //idal.SaveChanges();
            //return "11";

            string res = PackTaskInsertByLoad(packT);
            if (!string.IsNullOrEmpty(res))
            {
                string status = res.Substring(0, 1);
                if (status == "Y")
                {
                    string packHeadIdString = res.Split('$')[0];
                    string packHeadId = packHeadIdString.Substring(1, packHeadIdString.Length - 1);

                    // GetExpressNumber(  packHeadId, string userName)
                    return GetExpressNumber(Convert.ToInt32(packHeadId), "1536", "");
                }
                else
                    return res;
            }
            else
                return res;

        }
        public string TaskLoadStaticClear(string loadId, string whCode)
        {
            lock (o3)
            {
                //删除LOAD的状态数据
                if (staticLoadPackCountList != null)
                {
                    if (staticLoadPackCountList.Count > 0)
                    {
                        staticLoadPackCountList.RemoveAll(u => u.LoadId == loadId && u.WhCode == whCode);
                    }
                }

                if (staticLoadPackTaskDetailList != null)
                {
                    if (staticLoadPackTaskDetailList.Count > 0)
                    {
                        staticLoadPackTaskDetailList.RemoveAll(u => u.WhCode == whCode && u.LoadId == loadId);
                    }
                }

            }
            return "Y";
        }

        //拉取 计划包装总数与已包装总数 
        public List<SortTaskDetailResult> GetSumSortTaskDetailListByLoad(string loadId, string whCode)
        {
            var sql = from a in
                        (from a in idal.ISortTaskDetailDAL.SelectAll()
                         join b in idal.IPackTaskDAL.SelectAll()
                               on new { a.WhCode, a.LoadId, a.OutPoNumber }
                           equals new { b.WhCode, b.LoadId, b.OutPoNumber }
                         where
                           a.LoadId == loadId &&
                           a.WhCode == whCode
                         select new
                         {
                             a.PlanQty,
                             a.PackQty,
                             holdQty = a.PackQty == 0 && a.HoldFlag == 1 ? a.PlanQty : 0,
                             Dummy = "x"
                         })
                      group a by new { a.Dummy } into g
                      select new SortTaskDetailResult
                      {
                          PlanQty = (Int32?)g.Sum(p => p.PlanQty),
                          PackQty = (Int32?)g.Sum(p => p.PackQty),
                          HoldQty = (System.Int32?)g.Sum(p => p.holdQty)
                      };
            return sql.ToList();
        }


        //单品 包装工作台
        public string PackTaskInsertByLoad(PackTaskInsert entity)
        {
            if (entity.LoadId == null || entity.LoadId == "" || entity.WhCode == "" || entity.WhCode == null)
            {
                return "数据有误，请重新操作！";
            }

            if (entity.PackDetail == null)
            {
                return "明细数据有误，请重新操作！";
            }

            //  using (TransactionScope trans = new TransactionScope())
            //    {
            try
            {
                #region 开始包装

                //包装开始前 验证备货装箱是否完成
                var sql = (from a in idal.IPickTaskDetailDAL.SelectAll()
                           where a.LoadId == entity.LoadId && a.WhCode == entity.WhCode
                           && a.Status == "C" && a.Status1 == "U"
                           select a);
                if (sql.Count() > 0)
                {
                    return "该Load:" + entity.LoadId + "自动装箱有误，请查看备货任务后手动装箱！";
                }

                //单品包装验证逻辑
                //2.  Load有多条包装信息时
                //2.1 首先取得当前扫描的单品包装的款号
                PackDetailInsert getPackDetailInsert = entity.PackDetail.First();

                ////2.2 通过扫描的款号 查询出 分拣表中 同一Load 同一款号 包装数量不等于计划数量 的信息list
                //List<SortTaskDetail> getSortTaskDetailList = idal.ISortTaskDetailDAL.SelectBy(u => u.LoadId == entity.LoadId && u.WhCode == entity.WhCode && u.ItemId == getPackDetailInsert.ItemId && u.PlanQty != u.PackQty).OrderBy(u => u.GroupId).ToList();

                //if (getSortTaskDetailList.Count == 0)
                //{
                //    return "没有可用的包装信息！";
                //}

                ////2.3 提取list中 没有拦截标志的信息
                //if (getSortTaskDetailList.Where(u => u.HoldFlag != 1).Count() == 0)
                //{
                //    return "包装被拦截！";
                //}


                //2.2 通过扫描的款号 查询出 分拣表中 同一Load 同一款号 包装数量不等于计划数量 的信息list
                List<SortTaskDetail> sortTaskDetailList = idal.ISortTaskDetailDAL.SelectBy(u => u.LoadId == entity.LoadId && u.WhCode == entity.WhCode && u.ItemId == getPackDetailInsert.ItemId && u.PlanQty != u.PackQty).OrderBy(u => u.GroupId).ToList();


                ////////增加分拣数据到明细里
                //if (loadSortTaskDetailList.Where(u => u.WhCode == entity.WhCode && u.LoadId == entity.LoadId && u.ItemId == getPackDetailInsert.ItemId).Count() == 0)
                //    loadSortTaskDetailList.AddRange(sortTaskDetailList);

                if (sortTaskDetailList.Count == 0)
                {
                    return "没有可用的包装信息！";
                }

                //2.3 提取list中 没有拦截标志的信息
                if (sortTaskDetailList.Where(u => u.HoldFlag != 1).Count() == 0)
                {
                    return "包装被拦截！";
                }


                //2.4 通过list 查询 包装任务表中的 是单品标志且状态正常的 信息
                List<PackTask> getpacktaskSql = (from a in idal.ISortTaskDetailDAL.SelectAll()
                                                 join b in idal.IPackTaskDAL.SelectAll()
                                                       on new { a.WhCode, a.LoadId, a.GroupId, a.GroupNumber, a.OutPoNumber }
                                                   equals new { b.WhCode, b.LoadId, GroupId = b.SortGroupId, GroupNumber = b.SortGroupNumber, b.OutPoNumber }
                                                 where
                                                   a.WhCode == entity.WhCode &&
                                                   a.LoadId == entity.LoadId &&
                                                   a.ItemId == getPackDetailInsert.ItemId &&
                                                   a.PlanQty != a.PackQty && a.HoldFlag == 0 &&
                                                   b.SingleFlag == 1 && (b.Status == 10 || b.Status == 20)
                                                 orderby a.GroupId
                                                 select b).ToList();

                //判断2个状态是不是都没有,如果是0表示第一次插进来
                try
                {
                    var doStatusData = staticLoadPackCountList.Where(u => u.WhCode == entity.WhCode && u.LoadId == entity.LoadId && u.ItemId == getPackDetailInsert.ItemId).ToList();
                    var doData = staticLoadPackTaskDetailList.Where(u => u.WhCode == entity.WhCode && u.LoadId == entity.LoadId && u.ItemId == getPackDetailInsert.ItemId).ToList();


                    if (doData.Count() == 0 && doStatusData.Count() == 0)
                    {
                        List<StaticLoadPackTaskDetail> sqlInsert = (from a in sortTaskDetailList
                                                                    join b in getpacktaskSql
                                                                          on new { a.WhCode, a.LoadId, a.GroupId, a.GroupNumber, a.OutPoNumber }
                                                                      equals new { b.WhCode, b.LoadId, GroupId = b.SortGroupId, GroupNumber = b.SortGroupNumber, b.OutPoNumber }
                                                                    where a.HoldFlag == 0
                                                                    orderby a.GroupId
                                                                    select new StaticLoadPackTaskDetail
                                                                    {
                                                                        WhCode = a.WhCode,
                                                                        LoadId = a.LoadId,
                                                                        OutPoNumber = a.OutPoNumber,
                                                                        GroupId = (Int32)a.GroupId,
                                                                        GroupNumber = a.GroupNumber,
                                                                        PackNumber = a.OutPoNumber,
                                                                        ItemId = a.ItemId,
                                                                        packTaskId = b.Id
                                                                    }).ToList();
                        //新增明细数据到包装选择器里面
                        staticLoadPackTaskDetailList.AddRange(sqlInsert);

                        //新增状态LoadId,ItemId对应的状态到 包装状态明细里
                        StaticLoadPackCount aa = new StaticLoadPackCount();
                        aa.LoadId = entity.LoadId;
                        aa.WhCode = entity.WhCode;
                        aa.ItemId = getPackDetailInsert.ItemId;
                        aa.CountRows = sqlInsert.Count();
                        aa.status = "N";
                        staticLoadPackCountList.Add(aa);
                    }
                    //曾经包装过doStatusData.Count(),但是已经没数据doData.Count() == 0 了
                    else if (doData.Count() == 0 && doStatusData.Count() != 0)
                    {

                        Task<string> task = Task<string>.Run(() =>
                        {
                            System.Threading.Thread.Sleep(2000);
                            staticLoadPackCountList.RemoveAll(u => u.LoadId == entity.LoadId && u.WhCode == entity.WhCode && u.ItemId == getPackDetailInsert.ItemId);
                            return System.Threading.Thread.CurrentThread.ManagedThreadId.ToString();
                        });
                        //会等到任务执行完之后执行
                        task.GetAwaiter().OnCompleted(() =>
                        {
                            Console.WriteLine(task.Result);
                        });


                        return "没有可用的包装信息！2秒后系统自动刷新数据,请稍后再试!";
                    }
                }
                catch (Exception)
                {
                    TaskLoadStaticClear(entity.LoadId, entity.WhCode);
                    return "没有可用的包装信息！2秒后系统自动刷新数据,请稍后再试!";
                }

                //   return doData.Count() + "-" + doStatusData.Count();


                //分配GroupId明细数据 updateBy youpingshen
                //PackTask packTask = new PackTask();
                StaticLoadPackTaskDetail packTask = new StaticLoadPackTaskDetail();
                lock (o3)
                {

                    packTask = staticLoadPackTaskDetailList.Where(u => u.WhCode == entity.WhCode && u.LoadId == entity.LoadId && u.ItemId == getPackDetailInsert.ItemId).First();
                    entity.OutPoNumber = packTask.OutPoNumber;
                    entity.GroupId = (Int32)packTask.GroupId;
                    entity.GroupNumber = packTask.GroupNumber;
                    entity.PackNumber = packTask.OutPoNumber;
                    staticLoadPackTaskDetailList.Remove(packTask);

                    //修改状态变量的数量及状态
                    foreach (StaticLoadPackCount item in staticLoadPackCountList.Where(u => u.WhCode == packTask.WhCode && u.LoadId == packTask.LoadId && u.ItemId == packTask.ItemId))
                    {
                        item.CountRows = item.CountRows - 1;
                        //款号没数量的时候修改状态
                        if (item.CountRows == 0)
                            item.status = "Y";
                    }

                    // HoldFlag
                }

                int HoldFlagCount = sortTaskDetailList.Where(u => u.WhCode == entity.WhCode && u.LoadId == entity.LoadId && u.GroupId == entity.GroupId && u.GroupNumber == entity.GroupNumber && u.HoldFlag == 1).Count();
                if (HoldFlagCount == 1)
                    return "订单:" + entity.OutPoNumber + "已经拦截!请再次扫描获取新订单!";


                //List<PackTask> getPackTaskList = getpacktaskSql.ToList();

                //if (getPackTaskList.Count == 0)
                //{
                //    return "没有可用的包装信息！";
                //}

                //PackTask packTask = new PackTask();

                //lock (o3)
                //{
                //    packTask=getPackTaskList.First();
                //    entity.OutPoNumber = packTask.OutPoNumber;
                //    entity.GroupId = (Int32)packTask.SortGroupId;
                //    entity.GroupNumber = packTask.SortGroupNumber;
                //    entity.PackNumber = packTask.OutPoNumber;

                //}


                //PackTask packTask = getPackTaskList.First();
                //entity.OutPoNumber = packTask.OutPoNumber;
                //entity.GroupId = (Int32)packTask.SortGroupId;
                //entity.GroupNumber = packTask.SortGroupNumber;
                //entity.PackNumber = packTask.OutPoNumber;

                string result = "";

                //1.获取数据库中的分拣数据
                // List<SortTaskDetail> sortTaskDetailList = GetSortTaskDetail(entity.LoadId, entity.WhCode, entity.GroupId, entity.GroupNumber).ToList();

                sortTaskDetailList = sortTaskDetailList.Where(u => u.WhCode == entity.WhCode && u.LoadId == entity.LoadId && u.GroupId == entity.GroupId && u.GroupNumber == entity.GroupNumber).ToList();

                List<SortTaskDetail> resultList = new List<SortTaskDetail>();
                //2.提交的数据 与 数据库数据 进行比较验证 
                foreach (var item in entity.PackDetail)
                {
                    if (result != "")
                    {
                        break;
                    }
                    if (item.Qty < 1)
                    {
                        result = "数量不能小于1！";
                        break;
                    }
                    List<SortTaskDetail> taskDetailList = sortTaskDetailList.Where(u => u.ItemId == item.ItemId).ToList();
                    int sumPackQty = Convert.ToInt32(taskDetailList.Sum(u => u.PackQty).ToString());
                    int sumPlanQty = Convert.ToInt32(taskDetailList.Sum(u => u.PlanQty).ToString());

                    //比较总数量
                    if (item.Qty + sumPackQty > sumPlanQty)
                    {
                        result = "款号:" + item.AltItemNumber + "可用数量为:" + (sumPlanQty - sumPackQty);
                        break;
                    }
                    else
                    {
                        int? qty = item.Qty;
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
                                                        result = "款号:" + item.AltItemNumber + "扫描长度不符！";
                                                        break;
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                result = "款号:" + item.AltItemNumber + "扫描长度不符！";
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

                            if (item1.PackQty + qty > item1.PlanQty)
                            {
                                SortTaskDetail updateEntity = new SortTaskDetail();
                                updateEntity.Id = item1.Id;
                                updateEntity.PackQty = item1.PlanQty;
                                resultList.Add(updateEntity);
                                qty = qty - item1.PlanQty;
                                continue;
                            }
                            else
                            {
                                SortTaskDetail updateEntity = new SortTaskDetail();
                                updateEntity.Id = item1.Id;
                                updateEntity.PackQty = item1.PackQty + qty;
                                resultList.Add(updateEntity);
                                break;
                            }
                        }
                    }
                }
                if (result + "" != "")
                {
                    return result;
                }

                //0. 更新Load的开始包装时间
                LoadMaster load = idal.ILoadMasterDAL.SelectBy(u => u.LoadId == entity.LoadId && u.WhCode == entity.WhCode).First();

                List<PackHead> getOne = (from a in idal.IPackHeadDAL.SelectAll()
                                         join b in idal.IPackTaskDAL.SelectAll()
                                         on a.PackTaskId equals b.Id
                                         where b.LoadId == entity.LoadId && b.WhCode == entity.WhCode
                                         select a).Take(1).ToList();
                if (getOne.Count == 0)
                {
                    load.BeginPackDate = DateTime.Now;
                    load.UpdateUser = entity.userName;
                    load.UpdateDate = DateTime.Now;
                    idal.ILoadMasterDAL.UpdateBy(load, u => u.Id == load.Id, new string[] { "BeginPackDate", "UpdateUser", "UpdateDate" });
                }

                //---------插入包装数据
                //1.---------先反更新分拣明细已包装数量
                foreach (var item in resultList)
                {
                    SortTaskDetail updateEntity = new SortTaskDetail();
                    updateEntity.PackQty = item.PackQty;
                    updateEntity.UpdateUser = entity.userName;
                    updateEntity.UpdateDate = DateTime.Now;
                    idal.ISortTaskDetailDAL.UpdateBy(updateEntity, u => u.Id == item.Id, new string[] { "PackQty", "UpdateUser", "UpdateDate" });
                }

                //2.----------插入包装头表
                PackHead packHead = new PackHead();
                List<PackHead> checkPackHeadCount = idal.IPackHeadDAL.SelectBy(u => u.PackTaskId == packTask.packTaskId && u.WhCode == packTask.WhCode && u.PackNumber == entity.PackNumber && u.Status == 10).ToList();
                if (checkPackHeadCount.Count == 0)
                {
                    List<PackHead> packHeadList = idal.IPackHeadDAL.SelectBy(u => u.PackTaskId == packTask.packTaskId && u.WhCode == packTask.WhCode).ToList();
                    packHead.WhCode = packTask.WhCode;
                    packHead.PackTaskId = packTask.packTaskId;
                    if (packHeadList.Count == 0)
                    {
                        packHead.PackGroupId = 1;
                    }
                    else
                    {
                        packHead.PackGroupId = Convert.ToInt32(packHeadList.Max(u => u.PackGroupId).ToString()) + 1;
                    }
                    packHead.PackNumber = entity.PackNumber;
                    packHead.Status = 10;
                    packHead.CreateUser = entity.userName;
                    packHead.CreateDate = DateTime.Now;
                    idal.IPackHeadDAL.Add(packHead);
                }
                else
                {
                    packHead = checkPackHeadCount.First();
                    PackHead update = new PackHead();
                    update.Status = 10;
                    update.UpdateUser = entity.userName;
                    update.UpdateDate = DateTime.Now;
                    idal.IPackHeadDAL.UpdateBy(update, u => u.Id == packHead.Id, new string[] { "Status", "UpdateUser", "UpdateDate" });

                    if (!string.IsNullOrEmpty(packHead.ExpressNumber))
                    {
                        result = "请重新再扫描下款号！";
                    }
                }

                if (result + "" != "")
                {
                    return result;
                }

                //   idal.SaveChanges();        //保存一次 取得包装头表主键ID 


                //3.----------插入包装明细表
                foreach (var item in entity.PackDetail)
                {
                    PackDetail packDetail = new PackDetail();
                    packDetail.WhCode = packTask.WhCode;
                    packDetail.PackHeadId = packHead.Id;
                    packDetail.ItemId = item.ItemId;
                    packDetail.Qty = (Int32)item.Qty;
                    packDetail.CreateUser = entity.userName;
                    packDetail.CreateDate = DateTime.Now;
                    idal.IPackDetailDAL.Add(packDetail);

                    //3.1---------插入包装扫描表
                    if (item.PackScanNumber != null)
                    {
                        idal.IPackHeadDAL.SaveChanges();        //保存一次 取得包装明细表主键ID
                        foreach (var item1 in item.PackScanNumber)
                        {
                            PackScanNumber packScanNumber = new PackScanNumber();
                            packScanNumber.WhCode = packTask.WhCode;
                            packScanNumber.PackDetailId = packDetail.Id;
                            packScanNumber.ScanNumber = item1.ScanNumber;
                            packScanNumber.CreateUser = entity.userName;
                            packScanNumber.CreateDate = DateTime.Now;
                            idal.IPackScanNumberDAL.Add(packScanNumber);
                        }
                    }
                }
                //提交一次,不然分拣明细状态更新不出来
                idal.SaveChanges();

                //验证 分拣明细中 包装是否全部完成
                List<SortTaskDetail> checkAllQty = GetSortTaskDetail(entity.LoadId, entity.WhCode, entity.GroupId).ToList();
                if (checkAllQty.Count == 0)
                {
                    //如果全部完成 更新出库订单状态
                    List<OutBoundOrder> eneityList = idal.IOutBoundOrderDAL.SelectBy(u => u.WhCode == packTask.WhCode && u.OutPoNumber == packTask.OutPoNumber);
                    if (eneityList.Count > 0)
                    {
                        OutBoundOrder outBoundOrder = eneityList.First();
                        if (outBoundOrder.StatusId != -10)
                        {
                            FlowDetail flowDetail = idal.IFlowDetailDAL.SelectBy(u => u.FlowHeadId == outBoundOrder.ProcessId && u.Type == "PackingType").First();

                            if (flowDetail != null && flowDetail.StatusId != 0)
                            {
                                string remark1 = "原状态：" + outBoundOrder.StatusId + outBoundOrder.StatusName;

                                //更新出库订单状态为已包装
                                outBoundOrder.NowProcessId = flowDetail.FlowRuleId;
                                outBoundOrder.StatusId = flowDetail.StatusId;
                                outBoundOrder.StatusName = flowDetail.StatusName;
                                outBoundOrder.UpdateUser = entity.userName;
                                outBoundOrder.UpdateDate = DateTime.Now;
                                idal.IOutBoundOrderDAL.UpdateBy(outBoundOrder, u => u.Id == outBoundOrder.Id, new string[] { "NowProcessId", "StatusId", "StatusName", "UpdateUser", "UpdateDate" });

                                //更新订单状态，插入日志
                                TranLog tlorder = new TranLog();
                                tlorder.TranType = "32";
                                tlorder.Description = "更新订单状态";
                                tlorder.TranDate = DateTime.Now;
                                tlorder.TranUser = entity.userName;
                                tlorder.WhCode = packTask.WhCode;
                                tlorder.LoadId = entity.LoadId;
                                tlorder.CustomerOutPoNumber = outBoundOrder.CustomerOutPoNumber;
                                tlorder.OutPoNumber = outBoundOrder.OutPoNumber;
                                tlorder.Remark = remark1 + "变更为：" + outBoundOrder.StatusId + outBoundOrder.StatusName;
                                idal.ITranLogDAL.Add(tlorder);
                            }
                        }
                    }

                    //更新包装任务状态 为 已完成
                    PackTask setPackTask = new PackTask();
                    setPackTask.Status = 30;
                    setPackTask.UpdateUser = entity.userName;
                    setPackTask.UpdateDate = DateTime.Now;
                    idal.IPackTaskDAL.UpdateBy(setPackTask, u => u.Id == packTask.packTaskId, new string[] { "Status", "UpdateUser", "UpdateDate" });

                }


                //6. 更新Load的完成包装时间
                List<PackTask> checkPackTaskList1 = idal.IPackTaskDAL.SelectBy(u => u.LoadId == entity.LoadId && u.WhCode == entity.WhCode);
                if (checkPackTaskList1.Where(u => u.Status == 0 || u.Status == 10 || u.Status == 20).Count() == 0)
                {
                    load.EndPackDate = DateTime.Now;
                    load.UpdateUser = entity.userName;
                    load.UpdateDate = DateTime.Now;
                    //删除LOAD的状态数据
                    staticLoadPackCountList.RemoveAll(u => u.LoadId == entity.LoadId && u.WhCode == entity.WhCode);

                    idal.ILoadMasterDAL.UpdateBy(load, u => u.Id == load.Id, new string[] { "EndPackDate", "UpdateUser", "UpdateDate" });
                }
                #endregion

                idal.SaveChanges();
                //  trans.Complete();
                return "Y" + packHead.Id + "$" + packTask.packTaskId;
            }
            catch (Exception ex)
            {
                //trans.Dispose();//出现异常，事务手动释放

                return ex.ToString();
                // return "包装异常，请重新提交！";
            }
            // }
        }

        public PackTask GetPackTaskById(int packTaskId)
        {
            return idal.IPackTaskDAL.SelectBy(u => u.Id == packTaskId).First();
        }

        #endregion



        //验证 分拣框号状态
        public string CheckPackTaskStatus(string sortGroupNumber, string whCode)
        {
            List<PackTask> PackTaskList = idal.IPackTaskDAL.SelectBy(u => u.SortGroupNumber == sortGroupNumber && u.WhCode == whCode).ToList();
            if (PackTaskList.Count == 0)
            {
                return "分拣框号不存在或有误！";
            }

            if (PackTaskList.Where(u => u.Status == -10).Count() > 0)
            {
                return "分拣框号对应订单被拦截！";
            }

            if (PackTaskList.Where(u => u.Status == 0).Count() > 0)
            {
                PackTask pack = PackTaskList.First();

                List<PackTaskJson> packTaskJsonList = (from a in idal.IPackTaskJsonDAL.SelectAll()
                                                       where a.WhCode == pack.WhCode && a.CustomerOutPoNumber == pack.CustomerOutPoNumber
                                                       select a).ToList();
                if (packTaskJsonList.Count == 0)
                {
                    return "该Load其余订单未成功抛转地址信息，请联系客服处理！";
                }

                PackTaskJson packJson = packTaskJsonList.First();
                PackTaskJsonEntity packTaskJsonEntity = JsonConvert.DeserializeObject<PackTaskJsonEntity>(packJson.Json);

                pack.Status = 10;
                pack.TransportType = packTaskJsonEntity.TransportType;
                pack.express_code = packTaskJsonEntity.express_code;
                pack.express_type = packTaskJsonEntity.express_type;
                pack.express_type_zh = packTaskJsonEntity.express_type_zh;
                pack.PackMoreFlag = packTaskJsonEntity.PackMoreFlag;
                pack.SingleFlag = packTaskJsonEntity.SingleFlag;
                if (pack.SingleFlag == 1)
                {
                    pack.PackQty = 1;
                }
                pack.ZdFlag = packTaskJsonEntity.ZdFlag;
                pack.SinglePlaneTemplate = packTaskJsonEntity.SinglePlaneTemplate;
                pack.PackingListTemplate = packTaskJsonEntity.PackingListTemplate;

                idal.IPackTaskDAL.UpdateBy(pack, u => u.Id == pack.Id, new string[] { "Status", "TransportType", "express_code", "express_type", "express_type_zh", "SingleFlag", "ZdFlag", "SinglePlaneTemplate", "PackingListTemplate", "PackMoreFlag", "PackQty" });
                idal.SaveChanges();
            }

            List<PackTask> PackTaskList1 = idal.IPackTaskDAL.SelectBy(u => u.SortGroupNumber == sortGroupNumber && u.WhCode == whCode && (u.TransportType ?? "") == "").ToList();
            if (PackTaskList1.Count() > 0)
            {
                PackTask pack = PackTaskList1.First();

                List<PackTaskJson> packTaskJsonList = (from a in idal.IPackTaskJsonDAL.SelectAll()
                                                       where a.WhCode == pack.WhCode && a.CustomerOutPoNumber == pack.CustomerOutPoNumber
                                                       select a).ToList();
                if (packTaskJsonList.Count == 0)
                {
                    return "该Load其余订单未成功抛转地址信息，请联系客服处理！";
                }

                PackTaskJson packJson = packTaskJsonList.First();
                PackTaskJsonEntity packTaskJsonEntity = JsonConvert.DeserializeObject<PackTaskJsonEntity>(packJson.Json);

                pack.TransportType = packTaskJsonEntity.TransportType;
                pack.express_code = packTaskJsonEntity.express_code;
                pack.express_type = packTaskJsonEntity.express_type;
                pack.express_type_zh = packTaskJsonEntity.express_type_zh;
                pack.PackMoreFlag = packTaskJsonEntity.PackMoreFlag;
                pack.SingleFlag = packTaskJsonEntity.SingleFlag;
                if (pack.SingleFlag == 1)
                {
                    pack.PackQty = 1;
                }
                pack.ZdFlag = packTaskJsonEntity.ZdFlag;
                pack.SinglePlaneTemplate = packTaskJsonEntity.SinglePlaneTemplate;
                pack.PackingListTemplate = packTaskJsonEntity.PackingListTemplate;

                idal.IPackTaskDAL.UpdateBy(pack, u => u.Id == pack.Id, new string[] { "TransportType", "express_code", "express_type", "express_type_zh", "SingleFlag", "ZdFlag", "SinglePlaneTemplate", "PackingListTemplate", "PackMoreFlag", "PackQty" });
                idal.SaveChanges();
            }


            if (idal.IPackTaskDAL.SelectBy(u => u.SortGroupNumber == sortGroupNumber && u.WhCode == whCode && (u.Status == 10 || u.Status == 20)).Count == 0)
            {
                return "分拣框号没有可包装的订单信息！";
            }

            return "Y";
        }

        //验证 Load号是否正确
        public List<PackTask> GetPackTaskListByLoadOrder(string loadId, string whCode)
        {
            List<PackTask> PackTaskList = idal.IPackTaskDAL.SelectBy(u => u.LoadId == loadId && u.WhCode == whCode && u.Status == 0).ToList();

            if (PackTaskList.Count > 0)
            {
                string[] CustomerOutPoNumber = (from a in PackTaskList
                                                select a.CustomerOutPoNumber).ToList().Distinct().ToArray();
                //2.一次性拉取订单Json列表
                List<PackTaskJson> packTaskJsonList = (from a in idal.IPackTaskJsonDAL.SelectAll()
                                                       where a.WhCode == whCode && CustomerOutPoNumber.Contains(a.CustomerOutPoNumber)
                                                       select a).ToList();
                foreach (var item in PackTaskList)
                {
                    if (packTaskJsonList.Where(u => u.CustomerOutPoNumber == item.CustomerOutPoNumber).Count() > 0)
                    {
                        PackTaskJson packJson = packTaskJsonList.Where(u => u.CustomerOutPoNumber == item.CustomerOutPoNumber).First();

                        PackTaskJsonEntity packTaskJsonEntity = JsonConvert.DeserializeObject<PackTaskJsonEntity>(packJson.Json);

                        PackTask pack = item;
                        pack.Status = 10;
                        pack.TransportType = packTaskJsonEntity.TransportType;
                        pack.express_code = packTaskJsonEntity.express_code;
                        pack.express_type = packTaskJsonEntity.express_type;
                        pack.express_type_zh = packTaskJsonEntity.express_type_zh;
                        pack.PackMoreFlag = packTaskJsonEntity.PackMoreFlag;
                        pack.SingleFlag = packTaskJsonEntity.SingleFlag;
                        if (pack.SingleFlag == 1)
                        {
                            pack.PackQty = 1;
                        }
                        pack.ZdFlag = packTaskJsonEntity.ZdFlag;
                        pack.SinglePlaneTemplate = packTaskJsonEntity.SinglePlaneTemplate;
                        pack.PackingListTemplate = packTaskJsonEntity.PackingListTemplate;

                        idal.IPackTaskDAL.UpdateBy(pack, u => u.Id == pack.Id, new string[] { "Status", "TransportType", "express_code", "express_type", "express_type_zh", "SingleFlag", "ZdFlag", "SinglePlaneTemplate", "PackingListTemplate", "PackMoreFlag", "PackQty" });
                    }
                }
                idal.SaveChanges();
            }

            List<PackTask> PackTaskList1 = idal.IPackTaskDAL.SelectBy(u => u.LoadId == loadId && u.WhCode == whCode && (u.TransportType ?? "") == "").ToList();

            if (PackTaskList1.Count > 0)
            {
                string[] CustomerOutPoNumber = (from a in PackTaskList1
                                                select a.CustomerOutPoNumber).ToList().Distinct().ToArray();
                //2.一次性拉取订单Json列表
                List<PackTaskJson> packTaskJsonList = (from a in idal.IPackTaskJsonDAL.SelectAll()
                                                       where a.WhCode == whCode && CustomerOutPoNumber.Contains(a.CustomerOutPoNumber)
                                                       select a).ToList();
                foreach (var item in PackTaskList1)
                {
                    if (packTaskJsonList.Where(u => u.CustomerOutPoNumber == item.CustomerOutPoNumber).Count() > 0)
                    {
                        PackTaskJson packJson = packTaskJsonList.Where(u => u.CustomerOutPoNumber == item.CustomerOutPoNumber).First();

                        PackTaskJsonEntity packTaskJsonEntity = JsonConvert.DeserializeObject<PackTaskJsonEntity>(packJson.Json);

                        PackTask pack = item;
                        pack.TransportType = packTaskJsonEntity.TransportType;
                        pack.express_code = packTaskJsonEntity.express_code;
                        pack.express_type = packTaskJsonEntity.express_type;
                        pack.express_type_zh = packTaskJsonEntity.express_type_zh;
                        pack.PackMoreFlag = packTaskJsonEntity.PackMoreFlag;
                        pack.SingleFlag = packTaskJsonEntity.SingleFlag;
                        if (pack.SingleFlag == 1)
                        {
                            pack.PackQty = 1;
                        }
                        pack.ZdFlag = packTaskJsonEntity.ZdFlag;
                        pack.SinglePlaneTemplate = packTaskJsonEntity.SinglePlaneTemplate;
                        pack.PackingListTemplate = packTaskJsonEntity.PackingListTemplate;

                        idal.IPackTaskDAL.UpdateBy(pack, u => u.Id == pack.Id, new string[] { "TransportType", "express_code", "express_type", "express_type_zh", "SingleFlag", "ZdFlag", "SinglePlaneTemplate", "PackingListTemplate", "PackMoreFlag", "PackQty" });
                    }
                }

                idal.SaveChanges();
            }

            return idal.IPackTaskDAL.SelectBy(u => u.LoadId == loadId && u.WhCode == whCode && (u.Status == 10 || u.Status == 20) && u.SingleFlag != 1).ToList();
        }



        //验证 分拣框号是否正确
        public List<PackTask> GetPackTaskList(string sortGroupNumber, string whCode)
        {
            return idal.IPackTaskDAL.SelectBy(u => u.SortGroupNumber == sortGroupNumber && u.WhCode == whCode && (u.Status == 10 || u.Status == 20)).ToList();
        }


        //验证 包装箱号是否已存在
        public List<PackHead> CheckPackNumber(PackHead entity)
        {
            return idal.IPackHeadDAL.SelectBy(u => u.PackNumber == entity.PackNumber && u.WhCode == entity.WhCode && u.PackTaskId == entity.PackTaskId).ToList();
        }

        //通过包装任务Id 取得当前包装的最大组号
        public int GetPackHeadGroupId(int packTaskId)
        {
            List<PackHead> list = idal.IPackHeadDAL.SelectBy(u => u.PackTaskId == packTaskId);
            if (list.Count == 0)
            {
                return 1;
            }
            else
            {
                int? maxGroupId = list.Max(u => u.PackGroupId);
                return Convert.ToInt32(maxGroupId) + 1;
            }
        }


        //通过Load得到订单渠道
        public string GetOrderType(string loadId, string whCode)
        {
            var sql = (from a in idal.ILoadMasterDAL.SelectAll()
                       join b in idal.ILoadDetailDAL.SelectAll() on new { Id = a.Id } equals new { Id = (Int32)b.LoadMasterId } into b_join
                       from b in b_join.DefaultIfEmpty()
                       join c in idal.IOutBoundOrderDAL.SelectAll() on new { OutBoundOrderId = (Int32)b.OutBoundOrderId } equals new { OutBoundOrderId = c.Id } into c_join
                       from c in c_join.DefaultIfEmpty()
                       where a.WhCode == whCode && a.LoadId == loadId
                       select c.OrderType).Distinct();
            return sql.First();
        }


        //通过 当前包装扫描的框号 获得 其分拣明细
        public List<SortTaskDetail> GetSortTaskDetail(string loadId, string whCode, int sortGroupId)
        {
            return idal.ISortTaskDetailDAL.SelectBy(u => u.WhCode == whCode && u.LoadId == loadId && u.GroupId == sortGroupId && u.PlanQty != u.PackQty).ToList();
        }


        //通过 当前包装扫描的框号 获得 其流程中的包装类型
        public List<FlowDetail> GetPackingType(string loadId, string whCode)
        {
            var sql = from a in idal.ILoadMasterDAL.SelectAll()
                      join b in idal.IFlowDetailDAL.SelectAll()
                      on a.ProcessId equals b.FlowHeadId
                      where a.WhCode == whCode && a.LoadId == loadId
                      select b;

            return sql.ToList();
        }

        //得到包装扫描多种耗材
        public List<FlowDetail> GetScanningConsumables(string loadId, string whCode)
        {
            var sql = from a in idal.ILoadMasterDAL.SelectAll()
                      join b in idal.IFlowDetailDAL.SelectAll()
                      on a.ProcessId equals b.FlowHeadId
                      where a.WhCode == whCode && a.LoadId == loadId && b.Type == "ScanningConsumables"
                      select b;

            return sql.ToList();
        }

        //拉取 计划包装总数与已包装总数 
        public List<SortTaskDetailResult> GetSumSortTaskDetailList(string loadId, string whCode, int sortGroupId, string sortGroupNumber)
        {
            var sql = from a in
                      (from a in idal.ISortTaskDetailDAL.SelectAll()
                       where
                         a.LoadId == loadId &&
                         a.WhCode == whCode &&
                         a.GroupId == sortGroupId &&
                         a.GroupNumber == sortGroupNumber
                       select new
                       {
                           a.PlanQty,
                           a.PackQty,
                           holdQty = a.PackQty == 0 && a.HoldFlag == 1 ? a.PlanQty : 0,
                           Dummy = "x"
                       })
                      group a by new { a.Dummy } into g
                      select new SortTaskDetailResult
                      {
                          PlanQty = (Int32?)g.Sum(p => p.PlanQty),
                          PackQty = (Int32?)g.Sum(p => p.PackQty),
                          HoldQty = (System.Int32?)g.Sum(p => p.holdQty)
                      };
            return sql.ToList();
        }


        //多品包装工作台 //再次确认包装号码后 提交数据 
        public string PackTaskInsert(PackTaskInsert entity)
        {
            if (entity.LoadId == null || entity.LoadId == "" || entity.WhCode == "" || entity.WhCode == null || entity.GroupNumber == null || entity.GroupNumber == "")
            {
                return "数据有误，请重新操作！";
            }
            if (entity.PackDetail == null)
            {
                return "明细数据有误，请重新操作！";
            }

            entity.GroupNumber = entity.GroupNumber.Trim();

            try
            {
                #region 多品包装开始
                //包装开始前 验证备货装箱是否完成
                List<PickTaskDetail> pickList = (from a in idal.IPickTaskDetailDAL.SelectAll()
                                                 where a.LoadId == entity.LoadId && a.WhCode == entity.WhCode
                                                 select a).ToList();
                if (pickList.Where(u => u.Status == "C" && u.Status1 == "U").Count() > 0)
                {
                    return "该Load:" + entity.LoadId + "自动装箱有误，请查看备货任务后手动装箱！";
                }

                List<PackTask> checkPackStatus = idal.IPackTaskDAL.SelectBy(u => u.LoadId == entity.LoadId && u.WhCode == entity.WhCode && u.SortGroupId == entity.GroupId && u.SortGroupNumber == entity.GroupNumber).ToList();
                if (checkPackStatus.Where(u => u.Status == -10).Count() > 0)
                {
                    return "包装被拦截！";
                }
                if (checkPackStatus.Where(u => u.Status == 30).Count() > 0)
                {
                    return "包装已完成！";
                }

                List<SortTaskDetail> checkSortTaskDetailStatus = idal.ISortTaskDetailDAL.SelectBy(u => u.WhCode == entity.WhCode && u.LoadId == entity.LoadId && u.GroupId == entity.GroupId && u.GroupNumber == entity.GroupNumber).ToList();
                if (checkSortTaskDetailStatus.Where(u => u.HoldFlag == 1).Count() > 0)
                {
                    return "包装被拦截！";
                }

                PackTask GetPackTask = checkPackStatus.First();
                string result = "";
                string result1 = "";
                //验证序列号是否重复
                List<PackScanNumber> checkPackPackScanNumberList = (from a in idal.IPackTaskDAL.SelectAll()
                                                                    join b in idal.IPackHeadDAL.SelectAll() on new { Id = a.Id } equals new { Id = (Int32)b.PackTaskId } into b_join
                                                                    from b in b_join.DefaultIfEmpty()
                                                                    join c in idal.IPackDetailDAL.SelectAll() on new { Id = b.Id } equals new { Id = (Int32)c.PackHeadId } into c_join
                                                                    from c in c_join.DefaultIfEmpty()
                                                                    join d in idal.IPackScanNumberDAL.SelectAll() on new { Id = c.Id } equals new { Id = (Int32)d.PackDetailId } into d_join
                                                                    from d in d_join.DefaultIfEmpty()
                                                                    where
                                                                      a.LoadId == entity.LoadId &&
                                                                      a.WhCode == entity.WhCode &&
                                                                      a.OutPoNumber == GetPackTask.OutPoNumber &&
                                                                      (d.ScanNumber ?? "") != ""
                                                                    select d).ToList();
                List<PackScanNumberInsert> checkRFPackScanNumberList = new List<PackScanNumberInsert>();
                int count = 0;
                if (checkPackPackScanNumberList.Count > 0)
                {
                    foreach (var item2 in entity.PackDetail)
                    {
                        if (item2.PackScanNumber != null)
                        {
                            foreach (var item3 in item2.PackScanNumber)
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
                }
                if (result + "" != "")
                {
                    return "序列号已存在：" + result;
                }
                if (result1 + "" != "")
                {
                    return "序列号重复：" + result1;
                }

                //1.获取数据库中的分拣数据
                List<SortTaskDetail> sortTaskDetailList = GetSortTaskDetail(entity.LoadId, entity.WhCode, entity.GroupId).ToList();

                List<SortTaskDetail> resultList = new List<SortTaskDetail>();

                //2.提交的数据 与 数据库数据 进行比较验证 
                foreach (var item in entity.PackDetail)
                {
                    if (result != "")
                    {
                        break;
                    }
                    if (item.Qty < 1)
                    {
                        result = "数量不能小于1！";
                        break;
                    }
                    List<SortTaskDetail> taskDetailList = sortTaskDetailList.Where(u => u.ItemId == item.ItemId).ToList();
                    int sumPackQty = Convert.ToInt32(taskDetailList.Sum(u => u.PackQty).ToString());
                    int sumPlanQty = Convert.ToInt32(taskDetailList.Sum(u => u.PlanQty).ToString());
                    if (item.Qty + sumPackQty > sumPlanQty)
                    {
                        result = "款号:" + item.AltItemNumber + "可用数量为:" + (sumPlanQty - sumPackQty);
                        break;
                    }
                    else
                    {
                        int? qty = item.Qty;
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
                                                        result = "款号:" + item.AltItemNumber + "扫描长度不符！";
                                                        break;
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                result = "款号:" + item.AltItemNumber + "扫描长度不符！";
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

                            if (item1.PackQty + qty > item1.PlanQty)
                            {
                                SortTaskDetail updateEntity = new SortTaskDetail();
                                updateEntity.Id = item1.Id;
                                updateEntity.PackQty = item1.PlanQty;
                                resultList.Add(updateEntity);
                                qty = qty - item1.PlanQty;
                                continue;
                            }
                            else
                            {
                                SortTaskDetail updateEntity = new SortTaskDetail();
                                updateEntity.Id = item1.Id;
                                updateEntity.PackQty = item1.PackQty + qty;
                                resultList.Add(updateEntity);
                                break;
                            }
                        }
                    }
                }
                if (result + "" != "")
                {
                    return result;
                }

                //多品包装如果前台已获取快递单号，后台包装时只能包一个包裹
                List<PackTaskJson> packTaskJsonList = (from a in idal.IPackTaskJsonDAL.SelectAll()
                                                       where a.WhCode == GetPackTask.WhCode && a.CustomerOutPoNumber == GetPackTask.CustomerOutPoNumber
                                                       select a).ToList();
                if (packTaskJsonList.Count > 0)
                {
                    PackTaskJson packJson = packTaskJsonList.First();
                    PackTaskJsonEntity packTaskJsonEntity = JsonConvert.DeserializeObject<PackTaskJsonEntity>(packJson.Json);

                    if (packTaskJsonEntity.TransportType == "快递")
                    {
                        if (!string.IsNullOrEmpty(packTaskJsonEntity.express_type_zh))
                        {
                            if ((packTaskJsonEntity.ExpressNumber ?? "") != "")
                            {
                                int checkCount = 0;
                                foreach (var item in entity.PackDetail)
                                {
                                    List<SortTaskDetail> taskDetailList = sortTaskDetailList.Where(u => u.ItemId == item.ItemId).ToList();
                                    int sumPackQty = Convert.ToInt32(taskDetailList.Sum(u => u.PackQty).ToString());
                                    int sumPlanQty = Convert.ToInt32(taskDetailList.Sum(u => u.PlanQty).ToString());
                                    if (item.Qty + sumPackQty == sumPlanQty)
                                    {
                                        checkCount++;
                                    }
                                }

                                if (checkCount != entity.PackDetail.Count)
                                {
                                    result = "订单已前台获取快递单号，当前只能包一个包裹，请一次包完所有货物！";
                                }
                            }
                        }
                    }
                }
                if (result + "" != "")
                {
                    return result;
                }


                //0. 更新Load的开始包装时间
                LoadMaster load = idal.ILoadMasterDAL.SelectBy(u => u.LoadId == entity.LoadId && u.WhCode == entity.WhCode).First();

                List<PackHead> getOne = (from a in idal.IPackHeadDAL.SelectAll()
                                         join b in idal.IPackTaskDAL.SelectAll()
                                         on a.PackTaskId equals b.Id
                                         where b.LoadId == entity.LoadId && b.WhCode == entity.WhCode
                                         select a).ToList();
                if (getOne.Count == 0)
                {
                    load.BeginPackDate = DateTime.Now;
                    load.UpdateUser = entity.userName;
                    load.UpdateDate = DateTime.Now;
                    idal.ILoadMasterDAL.UpdateBy(load, u => u.Id == load.Id, new string[] { "BeginPackDate", "UpdateUser", "UpdateDate" });
                }

                //---------插入包装数据
                PackTask packTask = checkPackStatus.First();    //取得包装任务头表
                                                                //1.---------先反更新分拣明细已包装数量
                foreach (var item in resultList)
                {
                    SortTaskDetail updateEntity = new SortTaskDetail();
                    updateEntity.PackQty = item.PackQty;
                    updateEntity.UpdateUser = entity.userName;
                    updateEntity.UpdateDate = DateTime.Now;
                    idal.ISortTaskDetailDAL.UpdateBy(updateEntity, u => u.Id == item.Id, new string[] { "PackQty", "UpdateUser", "UpdateDate" });
                }

                //2.----------插入包装头表
                PackHead packHead = new PackHead();
                List<PackHead> checkPackHeadCount = idal.IPackHeadDAL.SelectBy(u => u.PackTaskId == packTask.Id && u.WhCode == packTask.WhCode && u.PackNumber == entity.PackNumber && u.Status == 10).ToList();
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
                    packHead.PackNumber = entity.PackNumber;
                    packHead.Status = 10;
                    packHead.CreateUser = entity.userName;
                    packHead.CreateDate = DateTime.Now;
                    idal.IPackHeadDAL.Add(packHead);
                }
                else
                {
                    packHead = checkPackHeadCount.First();
                    PackHead update = new PackHead();
                    update.Status = 10;
                    update.UpdateUser = entity.userName;
                    update.UpdateDate = DateTime.Now;
                    idal.IPackHeadDAL.UpdateBy(update, u => u.Id == packHead.Id, new string[] { "Status", "UpdateUser", "UpdateDate" });
                }

                idal.SaveChanges();        //保存一次 取得包装头表主键ID 

                //3.----------插入包装明细表
                foreach (var item in entity.PackDetail)
                {
                    PackDetail packDetail = new PackDetail();
                    packDetail.WhCode = packTask.WhCode;
                    packDetail.PackHeadId = packHead.Id;
                    packDetail.ItemId = item.ItemId;
                    packDetail.Qty = (Int32)item.Qty;
                    packDetail.CreateUser = entity.userName;
                    packDetail.CreateDate = DateTime.Now;
                    idal.IPackDetailDAL.Add(packDetail);

                    //3.1---------插入包装扫描表
                    if (item.PackScanNumber != null)
                    {
                        idal.IPackHeadDAL.SaveChanges();        //保存一次 取得包装明细表主键ID
                        foreach (var item1 in item.PackScanNumber)
                        {
                            PackScanNumber packScanNumber = new PackScanNumber();
                            packScanNumber.WhCode = packTask.WhCode;
                            packScanNumber.PackDetailId = packDetail.Id;
                            packScanNumber.ScanNumber = item1.ScanNumber;
                            packScanNumber.CreateUser = entity.userName;
                            packScanNumber.CreateDate = DateTime.Now;
                            idal.IPackScanNumberDAL.Add(packScanNumber);
                        }
                    }
                }

                //验证 分拣明细中 包装是否全部完成
                List<SortTaskDetail> checkAllQty = GetSortTaskDetail(entity.LoadId, entity.WhCode, entity.GroupId).ToList();
                if (checkAllQty.Count == 0)
                {
                    //如果全部完成 更新出库订单状态
                    List<OutBoundOrder> eneityList = idal.IOutBoundOrderDAL.SelectBy(u => u.WhCode == packTask.WhCode && u.OutPoNumber == packTask.OutPoNumber);
                    if (eneityList.Count != 0)
                    {
                        OutBoundOrder outBoundOrder = eneityList.First();
                        if (outBoundOrder.StatusId != -10)
                        {
                            FlowDetail flowDetail = idal.IFlowDetailDAL.SelectBy(u => u.FlowHeadId == outBoundOrder.ProcessId && u.Type == "PackingType").First();

                            if (flowDetail != null && flowDetail.StatusId != 0)
                            {
                                string remark1 = "原状态：" + outBoundOrder.StatusId + outBoundOrder.StatusName;

                                //更新出库订单状态为已包装
                                outBoundOrder.NowProcessId = flowDetail.FlowRuleId;
                                outBoundOrder.StatusId = flowDetail.StatusId;
                                outBoundOrder.StatusName = flowDetail.StatusName;
                                outBoundOrder.UpdateUser = entity.userName;
                                outBoundOrder.UpdateDate = DateTime.Now;
                                idal.IOutBoundOrderDAL.UpdateBy(outBoundOrder, u => u.Id == outBoundOrder.Id, new string[] { "NowProcessId", "StatusId", "StatusName", "UpdateUser", "UpdateDate" });

                                //更新订单状态，插入日志
                                TranLog tlorder = new TranLog();
                                tlorder.TranType = "32";
                                tlorder.Description = "更新订单状态";
                                tlorder.TranDate = DateTime.Now;
                                tlorder.TranUser = entity.userName;
                                tlorder.WhCode = entity.WhCode;
                                tlorder.LoadId = entity.LoadId;
                                tlorder.CustomerOutPoNumber = outBoundOrder.CustomerOutPoNumber;
                                tlorder.OutPoNumber = outBoundOrder.OutPoNumber;
                                tlorder.Remark = remark1 + "变更为：" + outBoundOrder.StatusId + outBoundOrder.StatusName;
                                idal.ITranLogDAL.Add(tlorder);
                            }
                        }
                    }

                    //更新包装任务状态 为 已完成
                    PackTask setPackTask = new PackTask();
                    setPackTask.Status = 30;
                    setPackTask.UpdateUser = entity.userName;
                    setPackTask.UpdateDate = DateTime.Now;
                    idal.IPackTaskDAL.UpdateBy(setPackTask, u => u.Id == packTask.Id, new string[] { "Status", "UpdateUser", "UpdateDate" });
                }

                //6. 更新Load的完成包装时间
                List<PackTask> checkPackTaskList1 = idal.IPackTaskDAL.SelectBy(u => u.LoadId == entity.LoadId && u.WhCode == entity.WhCode);
                if (checkPackTaskList1.Where(u => u.Status == 0 || u.Status == 10 || u.Status == 20).Count() == 0)
                {
                    load.EndPackDate = DateTime.Now;
                    load.UpdateUser = entity.userName;
                    load.UpdateDate = DateTime.Now;
                    idal.ILoadMasterDAL.UpdateBy(load, u => u.Id == load.Id, new string[] { "EndPackDate", "UpdateUser", "UpdateDate" });
                }
                #endregion

                idal.SaveChanges();

                return "Y" + packHead.Id;
            }
            catch
            {
                return "包装异常，请重新提交！";
            }


        }

        //检测耗材
        public string checkLoss(string whCode, string lossCode)
        {
            //检测耗材是否存在
            List<Loss> lostList = idal.ILossDAL.SelectBy(u => u.WhCode == whCode && u.LossCode == lossCode);
            if (lostList.Count == 0)
            {
                return "包装耗材不存在，请先添加！";
            }
            else
            {
                Loss loss = lostList.First();
                if (loss.Qty == 0)
                {
                    return "包装耗材数量不足，请先添加！";
                }
            }

            return "Y";
        }

        //修改包装耗材等信息
        public string UpdatePackHead(PackHead entity)
        {
            List<PackHead> packHeadList = idal.IPackHeadDAL.SelectBy(u => u.Id == entity.Id);
            if (packHeadList.Count == 0)
            {
                return "包装明细不存在！";
            }

            if (packHeadList.Where(u => (u.TransferHeadId ?? "") != "").Count() > 0)
            {
                return "包装正在交接，请先删除交接！";
            }


            PackTask pack = (from a in idal.IPackHeadDAL.SelectAll()
                             join b in idal.IPackTaskDAL.SelectAll()
                             on a.PackTaskId equals b.Id
                             where a.Id == entity.Id
                             select b).ToList().First();

            List<OutBoundOrder> outboundList = idal.IOutBoundOrderDAL.SelectBy(u => u.WhCode == pack.WhCode && u.OutPoNumber == pack.OutPoNumber);
            if (outboundList.Count > 0)
            {
                OutBoundOrder outBoundOrder = outboundList.First();

                //得到是否扫描多种耗材
                List<FlowDetail> getScanningList = GetScanningConsumables(pack.LoadId, pack.WhCode);

                if (getScanningList.Count > 0)
                {
                    #region 如果是多种耗材

                    List<string> lossArr = new List<string>();
                    string[] exp_arr = null;
                    if (!string.IsNullOrEmpty(entity.PackCarton))
                    {
                        exp_arr = entity.PackCarton.Replace("\r\n", ",").Split(',');
                    }

                    if (exp_arr != null)
                    {
                        foreach (var item in exp_arr)
                        {
                            if (!string.IsNullOrEmpty(item))
                            {
                                lossArr.Add(item);
                            }
                        }
                    }

                    List<Loss> lostList1 = idal.ILossDAL.SelectBy(u => u.WhCode == entity.WhCode && lossArr.Contains(u.LossCode) && u.ClientCode == outBoundOrder.ClientCode);
                    if (lostList1.Count > 0)
                    {
                        List<Loss> sumLossList = new List<Loss>();
                        foreach (var item in lossArr)
                        {
                            if (lostList1.Where(u => u.LossCode == item && u.ClientCode == outBoundOrder.ClientCode && u.WhCode == entity.WhCode).Count() > 0)
                            {
                                Loss getFirst = lostList1.Where(u => u.LossCode == item && u.ClientCode == outBoundOrder.ClientCode && u.WhCode == entity.WhCode).First();

                                if (sumLossList.Where(u => u.Id == getFirst.Id).Count() == 0)
                                {
                                    Loss loss = new Loss();
                                    loss.Id = getFirst.Id;
                                    loss.Qty = 1;
                                    sumLossList.Add(loss);
                                }
                                else
                                {
                                    Loss first = sumLossList.Where(u => u.Id == getFirst.Id).First();

                                    sumLossList.Remove(first);

                                    Loss newloss = first;
                                    newloss.Qty = newloss.Qty + 1;
                                    sumLossList.Add(newloss);
                                }
                            }
                        }

                        int[] idarr = (from a in sumLossList
                                       select a.Id).ToList().Distinct().ToArray();
                        List<Loss> getLoss1 = idal.ILossDAL.SelectBy(u => idarr.Contains(u.Id));

                        List<string> list1 = (from a in idal.IPackDetailDAL.SelectAll()
                                              join b in idal.IItemMasterDAL.SelectAll()
                                              on a.ItemId equals b.Id
                                              where a.PackHeadId == entity.Id
                                              select b.AltItemNumber).ToList();

                        string altitemnumber = "";
                        if (list1.Count > 0)
                        {
                            foreach (var item in list1.Distinct())
                            {
                                altitemnumber += item + ",";
                            }
                        }

                        if (altitemnumber.Length > 100)
                        {
                            altitemnumber = altitemnumber.Substring(0, 99);
                        }

                        List<TranLog> tranLogList = new List<TranLog>();
                        foreach (var item in sumLossList)
                        {
                            Loss getLoss = getLoss1.Where(u => u.Id == item.Id).First();
                            getLoss.Qty = getLoss.Qty - item.Qty;
                            idal.ILossDAL.UpdateBy(getLoss, u => u.Id == getLoss.Id, new string[] { "Qty" });

                            //添加日志
                            TranLog tl = new TranLog();
                            tl.TranType = "505";
                            tl.Description = "包装耗材";
                            tl.TranDate = DateTime.Now;
                            tl.TranUser = entity.UpdateUser;
                            tl.WhCode = getLoss.WhCode;
                            tl.ClientCode = getLoss.ClientCode;
                            tl.Length = entity.Length;
                            tl.Width = entity.Width;
                            tl.Height = entity.Height;
                            tl.Weight = entity.Weight;
                            tl.TranQty = -item.Qty;
                            tl.SoNumber = getLoss.LossCode;
                            tl.CustomerPoNumber = getLoss.LossCode;
                            tl.AltItemNumber = getLoss.LossCode;
                            tl.OutPoNumber = pack.OutPoNumber;
                            tl.CustomerOutPoNumber = pack.CustomerOutPoNumber;
                            tl.HoldReason = pack.AltCustomerOutPoNumber;
                            tl.LoadId = pack.LoadId;
                            tl.Remark = altitemnumber;
                            tranLogList.Add(tl);
                        }

                        idal.ITranLogDAL.Add(tranLogList);
                    }

                    #endregion
                }
                else
                {
                    //通过包装头确认是否是单品包装
                    //单品包装第一次扫描耗材时 需要更新ItemMaster中的耗材名称CartonName，第二次工作台单品包装时就会默认耗材而不用扫描耗材
                    if (pack.SingleFlag == 1)
                    {
                        PackHead firstPackHead = packHeadList.First();
                        List<PackDetail> packDetailList = idal.IPackDetailDAL.SelectBy(u => u.PackHeadId == firstPackHead.Id);
                        if (packDetailList.Count == 1)
                        {
                            PackDetail firstPackDetail = packDetailList.First();
                            if (firstPackDetail.Qty == 1)
                            {
                                int itemId = Convert.ToInt32(firstPackDetail.ItemId);
                                ItemMaster editItemMaster = idal.IItemMasterDAL.SelectBy(u => u.Id == itemId).First();
                                if (string.IsNullOrEmpty(editItemMaster.CartonName))
                                {
                                    editItemMaster.Id = itemId;
                                    editItemMaster.CartonName = entity.PackCarton.Trim();
                                    idal.IItemMasterDAL.UpdateBy(editItemMaster, u => u.Id == itemId, new string[] { "CartonName" });
                                }
                            }
                        }
                    }

                    //检测耗材是否存在
                    List<Loss> lostList = idal.ILossDAL.SelectBy(u => u.WhCode == entity.WhCode && u.LossCode == entity.PackCarton && u.ClientCode == outBoundOrder.ClientCode);
                    if (lostList.Count > 0)
                    {
                        Loss loss = lostList.First();
                        loss.Qty = loss.Qty - 1;
                        idal.ILossDAL.UpdateBy(loss, u => u.Id == loss.Id, new string[] { "Qty" });

                        if ((entity.Length ?? 0) == 0)
                        {
                            entity.Length = loss.Length;
                            entity.Width = loss.Width;
                            entity.Height = loss.Height;
                        }

                        if ((entity.Weight ?? 0) == 0)
                        {
                            entity.Weight = loss.Weight;
                        }

                        List<string> list1 = (from a in idal.IPackDetailDAL.SelectAll()
                                              join b in idal.IItemMasterDAL.SelectAll()
                                              on a.ItemId equals b.Id
                                              where a.PackHeadId == entity.Id
                                              select b.AltItemNumber).ToList();

                        string altitemnumber = "";
                        if (list1.Count > 0)
                        {
                            foreach (var item in list1.Distinct())
                            {
                                altitemnumber += item + ",";
                            }
                        }

                        if (altitemnumber.Length > 100)
                        {
                            altitemnumber = altitemnumber.Substring(0, 99);
                        }

                        //添加日志
                        TranLog tl = new TranLog();
                        tl.TranType = "505";
                        tl.Description = "包装耗材";
                        tl.TranDate = DateTime.Now;
                        tl.TranUser = entity.UpdateUser;
                        tl.WhCode = loss.WhCode;
                        tl.ClientCode = loss.ClientCode;
                        tl.Length = entity.Length;
                        tl.Width = entity.Width;
                        tl.Height = entity.Height;
                        tl.Weight = entity.Weight;
                        tl.TranQty = -1;
                        tl.SoNumber = entity.PackCarton;
                        tl.CustomerPoNumber = entity.PackCarton;
                        tl.AltItemNumber = entity.PackCarton;
                        tl.OutPoNumber = pack.OutPoNumber;
                        tl.CustomerOutPoNumber = pack.CustomerOutPoNumber;
                        tl.HoldReason = pack.AltCustomerOutPoNumber;
                        tl.LoadId = pack.LoadId;
                        tl.Remark = altitemnumber;
                        idal.ITranLogDAL.Add(tl);

                    }
                }
            }

            entity.ExpressStatus = "N";
            entity.UpdateDate = DateTime.Now;

            if (entity.ExpressNumber != null && entity.ExpressNumber != "")
            {
                idal.IPackHeadDAL.UpdateBy(entity, u => u.Id == entity.Id, new string[] { "Length", "Width", "Height", "Weight", "PackCarton", "ExpressStatus", "ExpressNumber", "UpdateUser", "UpdateDate" });
            }
            else
            {
                idal.IPackHeadDAL.UpdateBy(entity, u => u.Id == entity.Id, new string[] { "Length", "Width", "Height", "Weight", "PackCarton", "ExpressStatus", "UpdateUser", "UpdateDate" });
            }

            idal.IPackHeadDAL.SaveChanges();
            return "Y";

        }

        //修改包裹总数量
        /// <summary>
        /// 为子母单追加包裹总数量
        /// add by yangxin 2024-05-29
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public string UpdatePackTask(PackTask entity)
        {
            List<PackTask> packTaskList = idal.IPackTaskDAL.SelectBy(u => u.Id == entity.Id);
            if (packTaskList.Count == 0)
            {
                return "包装任务不存在！";
            }

            if (entity.PackQty != null)
            {
                idal.IPackTaskDAL.UpdateBy(entity, u => u.Id == entity.Id, new string[] { "PackQty" });
                idal.IPackTaskDAL.SaveChanges();
            }

            return "Y";

        }

        //删除包装头信息    //参数 只需要 包装头ID 及 包装任务ID
        public int DeletePackHead(PackHead entity)
        {
            using (TransactionScope trans = new TransactionScope())
            {
                try
                {
                    PackHead packHead = idal.IPackHeadDAL.SelectBy(u => u.Id == entity.Id).First();
                    if (!string.IsNullOrEmpty(packHead.ExpressNumber))
                    {
                        return 0;
                    }

                    //通过 包装任务ID    获取 包装任务表信息
                    PackTask packTask = idal.IPackTaskDAL.SelectBy(u => u.Id == entity.PackTaskId).First();

                    //通过 包装任务实体   获取 分拣明细数据
                    List<SortTaskDetail> sortTaskDetailList = idal.ISortTaskDetailDAL.SelectBy(u => u.WhCode == packTask.WhCode && u.LoadId == packTask.LoadId && u.GroupId == packTask.SortGroupId && u.GroupNumber == packTask.SortGroupNumber);

                    //通过 包装头ID  获取包装明细信息
                    List<PackDetail> packDetailList = idal.IPackDetailDAL.SelectBy(u => u.PackHeadId == entity.Id);

                    //1.反更新分拣数量
                    foreach (var item in packDetailList)
                    {
                        SortTaskDetail sortTaskDetail = sortTaskDetailList.Where(u => u.ItemId == item.ItemId).First();
                        sortTaskDetail.PackQty = sortTaskDetail.PackQty - item.Qty;
                        sortTaskDetail.UpdateUser = entity.UpdateUser;
                        sortTaskDetail.UpdateDate = DateTime.Now;
                        idal.ISortTaskDetailDAL.UpdateBy(sortTaskDetail, u => u.Id == sortTaskDetail.Id, new string[] { "PackQty", "UpdateUser", "UpdateDate" });

                        //3.删除对应的包装扫描信息
                        idal.IPackScanNumberDAL.DeleteBy(u => u.PackDetailId == item.Id);

                        //4.删除包装明细信息
                        idal.IPackDetailDAL.DeleteBy(u => u.Id == item.Id);
                    }

                    //2.更新包装任务的状态
                    packTask.Status = 20;
                    packTask.UpdateUser = entity.UpdateUser;
                    packTask.UpdateDate = DateTime.Now;
                    idal.IPackTaskDAL.UpdateBy(packTask, u => u.Id == packTask.Id, new string[] { "Status", "UpdateUser", "UpdateDate" });


                    //2.1 更新客户出库订单状态
                    List<OutBoundOrder> eneityList = idal.IOutBoundOrderDAL.SelectBy(u => u.WhCode == packTask.WhCode && u.OutPoNumber == packTask.OutPoNumber);
                    if (eneityList.Count > 0)
                    {
                        OutBoundOrder outBoundOrder = eneityList.First();

                        if (outBoundOrder.StatusId == 35)
                        {
                            FlowHelper flowHelper = new FlowHelper(outBoundOrder, "OutBound");
                            FlowDetail flowDetail = flowHelper.GetPreviousFlowDetail();
                            if (flowDetail != null && flowDetail.StatusId != 0)
                            {
                                string remark1 = "原状态：" + outBoundOrder.StatusId + outBoundOrder.StatusName;

                                outBoundOrder.NowProcessId = flowDetail.FlowRuleId;
                                outBoundOrder.StatusId = flowDetail.StatusId;
                                outBoundOrder.StatusName = flowDetail.StatusName;
                                outBoundOrder.UpdateUser = entity.UpdateUser;
                                outBoundOrder.UpdateDate = DateTime.Now;
                                idal.IOutBoundOrderDAL.UpdateBy(outBoundOrder, u => u.Id == outBoundOrder.Id, new string[] { "NowProcessId", "StatusId", "StatusName", "UpdateUser", "UpdateDate" });

                                //更新订单状态，插入日志
                                TranLog tlorder = new TranLog();
                                tlorder.TranType = "32";
                                tlorder.Description = "更新订单状态";
                                tlorder.TranDate = DateTime.Now;
                                tlorder.TranUser = entity.UpdateUser;
                                tlorder.WhCode = packTask.WhCode;
                                tlorder.LoadId = packTask.LoadId;
                                tlorder.CustomerOutPoNumber = outBoundOrder.CustomerOutPoNumber;
                                tlorder.OutPoNumber = outBoundOrder.OutPoNumber;
                                tlorder.Remark = remark1 + "变更为：" + outBoundOrder.StatusId + outBoundOrder.StatusName;
                                idal.ITranLogDAL.Add(tlorder);


                                //更新订单状态，插入日志
                                TranLog tlorder1 = new TranLog();
                                tlorder1.TranType = "33";
                                tlorder1.Description = "删除包装信息";
                                tlorder1.TranDate = DateTime.Now;
                                tlorder1.TranUser = entity.UpdateUser;
                                tlorder1.WhCode = packTask.WhCode;
                                tlorder1.LoadId = packTask.LoadId;
                                tlorder1.CustomerOutPoNumber = outBoundOrder.CustomerOutPoNumber;
                                tlorder1.OutPoNumber = outBoundOrder.OutPoNumber;
                                tlorder1.Remark = remark1 + "变更为：" + outBoundOrder.StatusId + outBoundOrder.StatusName;
                                idal.ITranLogDAL.Add(tlorder1);

                            }
                        }
                    }

                    //5.删除包装头信息
                    idal.IPackHeadDAL.DeleteBy(u => u.Id == entity.Id);

                    idal.IPackHeadDAL.SaveChanges();
                    trans.Complete();
                    return 1;
                }
                catch
                {
                    trans.Dispose();//出现异常，事务手动释放
                    return 0;
                }
            }
        }

        //查询包装明细信息列表
        public List<PackDetailSearchResult> GetPackDetailSearchResult(int packHeadId, out int total)
        {
            var sql = from a in idal.IPackHeadDAL.SelectAll()
                      join b in idal.IPackDetailDAL.SelectAll()
                      on new { Id = a.Id } equals new { Id = (Int32)b.PackHeadId } into b_join
                      from b in b_join.DefaultIfEmpty()
                      join c in idal.IItemMasterDAL.SelectAll()
                      on b.ItemId equals c.Id
                      where a.Id == packHeadId
                      select new PackDetailSearchResult
                      {
                          PackDetailId = b.Id,
                          PackNumber = a.PackNumber,
                          AltItemNumber = c.AltItemNumber,
                          PlanQty = b.PlanQty,
                          Qty = b.Qty,
                          CreateUser = b.CreateUser,
                          CreateDate = b.CreateDate
                      };

            total = sql.Count();
            return sql.ToList();
        }

        //查询包装扫描明细
        public List<PackPackScanNumberResult> ScanNumberDetailList(int packDetailId, out int total)
        {
            var sql = from a in idal.IPackScanNumberDAL.SelectAll()
                      where a.PackDetailId == packDetailId
                      select new PackPackScanNumberResult
                      {
                          PackScanNumberId = a.Id,
                          ScanNumber = a.ScanNumber
                      };

            total = sql.Count();

            return sql.ToList();
        }

        //查询包装信息列表
        public List<PackTaskSearchResult> GetPackTaskSearchResult(PackTaskSearch searchEntity, out int total, string[] expressNumberArr, string[] customerOutPoNumberArr)
        {
            var sql = (from b in idal.IPackHeadDAL.SelectAll()
                       join a in idal.IPackTaskDAL.SelectAll()
                             on new { Id = (Int32)b.PackTaskId }
                         equals new { a.Id } into b_join
                       from a in b_join.DefaultIfEmpty()
                       join c in (
                           (from sorttaskdetail in idal.ISortTaskDetailDAL.SelectAll()
                            where sorttaskdetail.WhCode == searchEntity.WhCode
                            group sorttaskdetail by new
                            {
                                sorttaskdetail.WhCode,
                                sorttaskdetail.LoadId,
                                sorttaskdetail.GroupId,
                                sorttaskdetail.GroupNumber
                            } into g
                            select new
                            {
                                g.Key.WhCode,
                                g.Key.LoadId,
                                GroupId = g.Key.GroupId,
                                g.Key.GroupNumber,
                                planQty = g.Sum(p => p.PlanQty),
                                packQty = g.Sum(p => p.PackQty)
                            }))
                             on new { a.WhCode, a.LoadId, a.SortGroupId, a.SortGroupNumber }
                         equals new { c.WhCode, c.LoadId, SortGroupId = c.GroupId, SortGroupNumber = c.GroupNumber } into c_join
                       from c in c_join.DefaultIfEmpty()
                       join d in idal.IPackDetailDAL.SelectAll()
                       on b.Id equals d.PackHeadId into d_join
                       from d in d_join.DefaultIfEmpty()
                       join e in idal.IItemMasterDAL.SelectAll()
                       on d.ItemId equals e.Id
                       join f in
                          (from packDetail in idal.IPackDetailDAL.SelectAll()
                           group packDetail by new
                           {
                               packDetail.PackHeadId
                           } into g
                           select new
                           {
                               g.Key.PackHeadId,
                               packQty = g.Sum(p => p.Qty)
                           })
                      on b.Id equals f.PackHeadId into f_join
                       from f in f_join.DefaultIfEmpty()
                       join g in idal.IOutBoundOrderDAL.SelectAll()
                     on new { A = a.WhCode, B = a.OutPoNumber } equals new { A = g.WhCode, B = g.OutPoNumber }
                       join h in idal.IFlowHeadDAL.SelectAll()
                       on g.ProcessId equals h.Id
                       where a.WhCode == searchEntity.WhCode && g.ClientCode != "Bosch" && (a.SingleFlag ?? 0) != 2
                       select new PackTaskSearchResult
                       {
                           PackTaskId = a.Id,
                           LoadId = a.LoadId,
                           SortGroupNumber = a.SortGroupNumber,
                           CustomerOutPoNumber = a.CustomerOutPoNumber,
                           OutPoNumber = a.OutPoNumber,
                           ExpressCode = a.express_code,
                           ClientCode = g.ClientCode,
                           PackHeadId = b.Id,
                           PackGroupId = b.PackGroupId,
                           PackNumber = b.PackNumber,
                           ExpressNumber = b.ExpressNumber ?? "",
                           ExpressStatus = b.ExpressStatus,
                           ExpressMessage = b.ExpressMessage,
                           Length = b.Length,
                           Width = b.Width,
                           Height = b.Height,
                           Weight = b.Weight,
                           PackCarton = b.PackCarton,
                           Status =
                           b.Status == -10 ? "被拦截" :
                           b.Status == 0 ? "未包装" :
                           b.Status == 10 ? "正常" :
                           b.Status == 20 ? "已交接" : null,
                           planQty = c.planQty,
                           packQty = c.packQty,//add by yangxin 20240530 包裹总数量
                           packNowQty = f.packQty,
                           Qty = d.Qty,
                           CreateDate = b.CreateDate,
                           UpdateDate = b.UpdateDate,
                           UpdateUser = b.UpdateUser,
                           AltItemNumber = e.AltItemNumber,
                           SinglePlaneTemplate = a.SinglePlaneTemplate,
                           PackingListTemplate = a.PackingListTemplate,
                           AltCustomerOutPoNumber = g.AltCustomerOutPoNumber,
                           OrderDate = g.CreateDate,
                           OrderType = g.OrderType,
                           OrderSource = g.OrderSource,
                           ProcessName = h.FlowName,
                           ItemName = e.ItemName ?? ""
                       }).Distinct();

            if (!string.IsNullOrEmpty(searchEntity.ClientCode))
                sql = sql.Where(u => u.ClientCode == searchEntity.ClientCode);
            if (!string.IsNullOrEmpty(searchEntity.SortGroupNumber))
                sql = sql.Where(u => u.SortGroupNumber == searchEntity.SortGroupNumber);
            if (!string.IsNullOrEmpty(searchEntity.AltItemNumber))
                sql = sql.Where(u => u.AltItemNumber == searchEntity.AltItemNumber);

            if (!string.IsNullOrEmpty(searchEntity.LoadId))
                sql = sql.Where(u => u.LoadId.Contains(searchEntity.LoadId));

            if (expressNumberArr != null)
                sql = sql.Where(u => expressNumberArr.Contains(u.ExpressNumber));

            if (customerOutPoNumberArr != null)
                sql = sql.Where(u => customerOutPoNumberArr.Contains(u.CustomerOutPoNumber));

            if (!string.IsNullOrEmpty(searchEntity.UpdateUser))
                sql = sql.Where(u => u.UpdateUser == searchEntity.UpdateUser);

            if (!string.IsNullOrEmpty(searchEntity.Status))
            {
                sql = sql.Where(u => u.Status == searchEntity.Status);
            }

            if (!string.IsNullOrEmpty(searchEntity.ExpressNumberIsNull))
            {
                if (searchEntity.ExpressNumberIsNull == "有")
                {
                    sql = sql.Where(u => u.ExpressNumber != "");
                }
                else
                {
                    sql = sql.Where(u => u.ExpressNumber == "");
                }
            }

            if (searchEntity.BeginCreateDate != null)
            {
                sql = sql.Where(u => u.CreateDate >= searchEntity.BeginCreateDate);
            }
            if (searchEntity.EndCreateDate != null)
            {
                sql = sql.Where(u => u.CreateDate < searchEntity.EndCreateDate);
            }


            if (searchEntity.BeginOrderDate != null)
            {
                sql = sql.Where(u => u.OrderDate >= searchEntity.BeginOrderDate);
            }
            if (searchEntity.EndOrderDate != null)
            {
                sql = sql.Where(u => u.OrderDate < searchEntity.EndOrderDate);
            }

            if (!string.IsNullOrEmpty(searchEntity.AltCustomerOutPoNumber))
                sql = sql.Where(u => u.AltCustomerOutPoNumber == (searchEntity.AltCustomerOutPoNumber));

            total = sql.Count();
            sql = sql.OrderByDescending(u => u.UpdateDate).Skip((searchEntity.pageSize) * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);

            return sql.ToList();
        }

        //随箱单报表查询
        public List<PackTaskCryReport> GetCryReportPackTask(int packHeadId, string whCode, string userName, int type)
        {
            var sql = from a in idal.IPackTaskDAL.SelectAll()
                      join b in idal.IPackHeadDAL.SelectAll() on new { Id = a.Id } equals new { Id = (Int32)b.PackTaskId } into b_join
                      from b in b_join.DefaultIfEmpty()
                      join c in idal.IPackDetailDAL.SelectAll() on new { Id = b.Id } equals new { Id = (Int32)c.PackHeadId } into c_join
                      from c in c_join.DefaultIfEmpty()
                      join d in idal.IItemMasterDAL.SelectAll() on new { ItemId = (Int32)c.ItemId } equals new { ItemId = d.Id } into d_join
                      from d in d_join.DefaultIfEmpty()
                      join e in idal.IOutBoundOrderDAL.SelectAll()
                            on new { a.WhCode, a.OutPoNumber }
                        equals new { e.WhCode, e.OutPoNumber } into e_join
                      from e in e_join.DefaultIfEmpty()
                      join f in idal.IOutBoundOrderDetailDAL.SelectAll()
                            on new { e.Id, ItemId = (Int32)c.ItemId }
                        equals new { Id = f.OutBoundOrderId, ItemId = f.ItemId } into f_join
                      from f in f_join.DefaultIfEmpty()
                      where
                        b.Id == packHeadId
                      group new { a, d, c } by new
                      {
                          a.WhCode,
                          a.LoadId,
                          a.OutPoNumber,
                          a.CustomerOutPoNumber,
                          a.AltCustomerOutPoNumber,
                          a.d_company,
                          a.d_contact,
                          a.d_tel,
                          a.d_address,
                          d.Id,
                          d.Description,
                          d.Remark1,
                          a.OrderCreateDate,
                          f.Price,
                          d.AltItemNumber,
                          d.EAN
                      } into g
                      select new PackTaskCryReport
                      {
                          WhCode = g.Key.WhCode,
                          LoadId = g.Key.LoadId,
                          OutPoNumber = g.Key.OutPoNumber,
                          CustomerPoNumber = g.Key.CustomerOutPoNumber,

                          ItemId = (Int32?)g.Key.Id,
                          AltItemNumber = g.Key.AltItemNumber,
                          Description = g.Key.Description,
                          Remark1 = g.Key.Remark1,
                          Qty = (Int32?)g.Sum(p => p.c.Qty),

                          Price = g.Key.Price ?? 0,
                          PackGroupId = 0,
                          TotalPrice = (Int32?)g.Sum(p => p.c.Qty) * g.Key.Price ?? 0,
                          Length = 0,
                          Height = 0,
                          Width = 0,
                          Weight = 0,
                          EAN = g.Key.EAN
                      };

            List<PackTaskCryReport> sqlList = sql.ToList();
            PackTaskCryReport first = sqlList.First();

            //得到订单Json列表
            //1.得到订单数组
            string[] CustomerOutPoNumber = (from a in sqlList
                                            select a.CustomerPoNumber).ToList().Distinct().ToArray();
            //2.一次性拉取订单Json列表
            List<PackTaskJson> packTaskJsonList = (from a in idal.IPackTaskJsonDAL.SelectAll()
                                                   where a.WhCode == first.WhCode && CustomerOutPoNumber.Contains(a.CustomerOutPoNumber)
                                                   select a).ToList();

            List<PackTaskCryReport> sqlResult = new List<PackTaskCryReport>();
            foreach (var item in sqlList)
            {
                PackTaskJson packJson = new PackTaskJson();
                PackTaskJsonEntity packTaskJsonEntity = new PackTaskJsonEntity();

                if (packTaskJsonList.Where(u => u.CustomerOutPoNumber == item.CustomerPoNumber).Count() > 0)
                {
                    packJson = packTaskJsonList.Where(u => u.CustomerOutPoNumber == item.CustomerPoNumber).First();

                    packTaskJsonEntity = JsonConvert.DeserializeObject<PackTaskJsonEntity>(packJson.Json);
                }
                if (packTaskJsonEntity == null)
                {
                    continue;
                }

                //CNUNICEF 客户 订单类型为CNUNICEF_ct09 的 代表不打印随箱单
                if (packTaskJsonEntity.BusinessMode == "CNUNICEF_ct09" || packTaskJsonEntity.BusinessMode == "CNUNICEF_TM")
                {
                    continue;
                }

                PackTaskCryReport packCry = new PackTaskCryReport();
                packCry.WhCode = item.WhCode;
                packCry.LoadId = item.LoadId;
                packCry.OutPoNumber = item.OutPoNumber;
                packCry.CustomerPoNumber = item.CustomerPoNumber;
                packCry.PingTaiNumberBar = null;
                packCry.PingTaiNumber = packTaskJsonEntity.AltCustomerOutPoNumber ?? "";
                packCry.ClientCode = packTaskJsonEntity.ClientCode ?? "";

                if (!string.IsNullOrEmpty(packTaskJsonEntity.CompanyName))
                {
                    packCry.CustomerPo = packTaskJsonEntity.CompanyName ?? "";
                }
                else
                {
                    packCry.CustomerPo = packTaskJsonEntity.customerPo ?? "";
                }

                packCry.d_company = packTaskJsonEntity.d_company ?? "";
                packCry.d_contact = packTaskJsonEntity.d_contact ?? "";
                packCry.d_tel = packTaskJsonEntity.d_tel ?? "";
                packCry.d_address = packTaskJsonEntity.d_address ?? "";
                packCry.CreateDate = packTaskJsonEntity.OrderCreateDate ?? DateTime.Now;

                packCry.ItemId = item.ItemId;
                packCry.AltItemNumber = item.AltItemNumber;
                packCry.Description = item.Description;
                packCry.Remark1 = item.Remark1;
                packCry.Qty = item.Qty;
                packCry.EAN = item.EAN;

                packCry.Price = item.Price ?? 0;
                packCry.PackGroupId = 0;
                packCry.TotalPrice = item.TotalPrice ?? 0;
                packCry.Length = 0;
                packCry.Height = 0;
                packCry.Width = 0;
                packCry.Weight = 0;
                sqlResult.Add(packCry);
            }

            return sqlResult;
        }

        //电子面单报表查询
        public List<PackTaskCryReportExpress> GetCryReportExpressPackTask(int packHeadId, string whCode, string userName, int type, string content)
        {
            var sql = from a in idal.IPackTaskDAL.SelectAll()
                      join b in idal.IPackHeadDAL.SelectAll() on new { Id = a.Id } equals new { Id = (Int32)b.PackTaskId } into b_join
                      from b in b_join.DefaultIfEmpty()
                      where b.Id == packHeadId
                      select new PackTaskCryReportExpress
                      {
                          WhCode = a.WhCode,
                          LoadId = a.LoadId,
                          OutPoNumber = a.OutPoNumber,
                          CustomerOutPoNumber = a.CustomerOutPoNumber,
                          express_type_zh = a.express_type_zh,
                          express_code = a.express_code,
                          PackQty = a.PackQty ?? 0,
                          dest_code = a.dest_code ?? "",
                          PackGroupId = b.PackGroupId,
                          ExpressNumber = b.ExpressNumber,
                          ExpressNumberParent = b.ExpressNumberParent ?? "",
                          ExpressNumberBar = null,
                          packHeadId = b.Id,
                          Weight = b.Weight ?? 0,
                          DNBar = null
                      };

            List<PackTaskCryReportExpress> sqlList = sql.ToList();
            if (sqlList.Count == 0)
            {
                return null;
            }

            PackTaskCryReportExpress first = sqlList.First();

            //得到订单Json列表
            //1.得到订单数组
            string[] CustomerOutPoNumber = (from a in sqlList
                                            select a.CustomerOutPoNumber).ToList().Distinct().ToArray();
            //2.一次性拉取订单Json列表
            List<PackTaskJson> packTaskJsonList = (from a in idal.IPackTaskJsonDAL.SelectAll()
                                                   where a.WhCode == first.WhCode && CustomerOutPoNumber.Contains(a.CustomerOutPoNumber)
                                                   select a).ToList();

            List<PackHeadJson> packHeadJsonList = new List<PackHeadJson>();
            //3.一次性拉取订单Json列表
            //如果不是子母单
            if (string.IsNullOrEmpty(first.ExpressNumberParent))
            {
                packHeadJsonList = (from a in idal.IPackHeadJsonDAL.SelectAll()
                                    where a.WhCode == first.WhCode && a.ExpressNumber == first.ExpressNumber
                                    select a).ToList();
            }
            else
            {
                packHeadJsonList = (from a in idal.IPackHeadJsonDAL.SelectAll()
                                    where a.WhCode == first.WhCode && a.ExpressNumber == first.ExpressNumberParent
                                    select a).ToList();
            }


            List<PackTaskCryReportExpress> sqlResult = new List<PackTaskCryReportExpress>();
            foreach (var item in sqlList)
            {
                PackTaskJson packJson = new PackTaskJson();
                PackTaskJsonEntity packTaskJsonEntity = new PackTaskJsonEntity();

                if (packTaskJsonList.Where(u => u.CustomerOutPoNumber == item.CustomerOutPoNumber).Count() > 0)
                {
                    packJson = packTaskJsonList.Where(u => u.CustomerOutPoNumber == item.CustomerOutPoNumber).First();

                    packTaskJsonEntity = JsonConvert.DeserializeObject<PackTaskJsonEntity>(packJson.Json);
                }
                if (packTaskJsonEntity == null)
                {
                    continue;
                }

                PackHeadJson packHeadJson = new PackHeadJson();
                PackHeadJsonEntity packHeadJsonEntity = new PackHeadJsonEntity();

                PackHeadJsonEntityZTO packHeadJsonEntityZTO = new PackHeadJsonEntityZTO();

                PackTaskCryReportExpress eneity = new PackTaskCryReportExpress();
                eneity.WhCode = item.WhCode;
                eneity.LoadId = item.LoadId;
                eneity.express_code = item.express_code;
                eneity.CustomerOutPoNumber = item.CustomerOutPoNumber;
                eneity.express_type_zh = item.express_type_zh ?? "";

                if (item.PackQty == null)
                {
                    eneity.PackQty = 0;
                }
                else
                {
                    eneity.PackQty = item.PackQty ?? 0;
                }

                string remark = "";
                List<PackDetail> packDetailList = idal.IPackDetailDAL.SelectBy(u => u.PackHeadId == packHeadId);
                int?[] itemIdArr = (from a in packDetailList
                                    select a.ItemId).ToList().Distinct().ToArray();

                List<ItemMaster> itemMasterList = idal.IItemMasterDAL.SelectBy(u => itemIdArr.Contains(u.Id));

                foreach (var packDetail in packDetailList)
                {
                    ItemMaster getItem = itemMasterList.Where(u => u.Id == packDetail.ItemId).First();
                    remark += getItem.AltItemNumber + "*" + packDetail.Qty + ",";
                }

                eneity.Remark = "客户订单号:" + item.CustomerOutPoNumber;

                //取得顺丰加密信息
                if (eneity.express_code == "SF")
                {
                    if (packHeadJsonList.Count > 0)
                    {
                        //如果不是子母单
                        if (string.IsNullOrEmpty(item.ExpressNumberParent))
                        {
                            packHeadJson = packHeadJsonList.Where(u => u.ExpressNumber == item.ExpressNumber).First();
                            packHeadJsonEntity = JsonConvert.DeserializeObject<PackHeadJsonEntity>(packHeadJson.Json);
                        }
                        else
                        {
                            packHeadJson = packHeadJsonList.Where(u => u.ExpressNumber == item.ExpressNumberParent).First();
                            packHeadJsonEntity = JsonConvert.DeserializeObject<PackHeadJsonEntity>(packHeadJson.Json);

                        }

                        if (packHeadJsonEntity != null)
                        {
                            eneity.proCode = packHeadJsonEntity.proCode;
                            if (string.IsNullOrEmpty(eneity.proCode))
                            {
                                eneity.proName = packHeadJsonEntity.proName;
                            }
                            eneity.destRouteLabel = packHeadJsonEntity.destRouteLabel;
                            eneity.destTeamCode = packHeadJsonEntity.destTeamCode;
                            eneity.codingMapping = packHeadJsonEntity.codingMapping;
                            eneity.xbFlag = packHeadJsonEntity.xbFlag;
                            eneity.codingMappingOut = packHeadJsonEntity.codingMappingOut;
                            eneity.printIcon = packHeadJsonEntity.printIcon;
                            if (!string.IsNullOrEmpty(item.ExpressNumberParent))
                            {
                                eneity.twoDimensionCode = packHeadJsonEntity.twoDimensionCode.Replace(item.ExpressNumberParent, item.ExpressNumber);
                            }
                            else
                            {
                                eneity.twoDimensionCode = packHeadJsonEntity.twoDimensionCode;
                            }
                        }
                    }
                }

                //取得中通加密信息
                if (eneity.express_code == "ZTO")
                {
                    if (packHeadJsonList.Count > 0)
                    {
                        packHeadJson = packHeadJsonList.Where(u => u.ExpressNumber == item.ExpressNumber).First();
                        packHeadJsonEntityZTO = JsonConvert.DeserializeObject<PackHeadJsonEntityZTO>(packHeadJson.Json);

                        if (packHeadJsonEntityZTO != null)
                        {
                            eneity.bagAddr = packHeadJsonEntityZTO.bagAddr;
                        }
                    }
                }

                if (packTaskJsonEntity.CustomerOutPoNumber != null)
                {
                    eneity.j_company = packTaskJsonEntity.j_company ?? "";
                    eneity.j_contact = packTaskJsonEntity.j_contact ?? "";
                    eneity.j_tel = packTaskJsonEntity.j_tel ?? "";
                    eneity.j_province = packTaskJsonEntity.j_province ?? "";
                    eneity.j_city = packTaskJsonEntity.j_city ?? "";
                    eneity.j_county = packTaskJsonEntity.j_county ?? "";
                    eneity.j_address = packTaskJsonEntity.j_address ?? "";
                    eneity.d_company = packTaskJsonEntity.d_company ?? "";
                    eneity.d_contact = packTaskJsonEntity.d_contact ?? "";
                    eneity.d_tel = packTaskJsonEntity.d_tel ?? "";

                    eneity.ClientCode = packTaskJsonEntity.ClientCode;

                    if (eneity.express_code == "SF")
                    {
                        if (eneity.d_tel.Length == 11)
                        {
                            eneity.d_tel = eneity.d_tel.Substring(0, 1) + "*" + eneity.d_tel.Substring(7, 4);
                        }
                        else if (eneity.d_tel.Length == 16)
                        {
                            eneity.d_tel = eneity.d_tel;
                        }
                        else if (eneity.d_tel.Length > 4)
                        {
                            eneity.d_tel = "*" + eneity.d_tel.Substring(eneity.d_tel.Length - 4, 4);
                        }
                        else
                        {
                            eneity.d_tel = eneity.d_tel;
                        }
                    }

                    eneity.d_Province = packTaskJsonEntity.d_Province ?? "";
                    eneity.d_city = packTaskJsonEntity.d_city ?? "";
                    eneity.d_address = packTaskJsonEntity.d_address ?? "";
                    eneity.custid = packTaskJsonEntity.custid ?? "";
                    eneity.cod = packTaskJsonEntity.cod.ToString() ?? "";
                    eneity.j_name = packTaskJsonEntity.j_name ?? "";
                    eneity.form_code = packTaskJsonEntity.form_code ?? "";
                    eneity.DN = packTaskJsonEntity.AltCustomerOutPoNumber ?? "";
                    eneity.CustomerPo = packTaskJsonEntity.customerPo ?? "";
                    eneity.PayMethodShow = packTaskJsonEntity.payMethod == "2" ? "到付" : "寄付月结";

                    eneity.AirFlag = packTaskJsonEntity.AirFlag == "0" ? "不可航空" :
                                    packTaskJsonEntity.AirFlag == "1" ? "可航空" : "不可航空";

                    eneity.Remark += " 订单编号:" + packTaskJsonEntity.AltCustomerOutPoNumber;
                }

                eneity.Remark += " " + remark.Substring(0, remark.Length - 1);

                eneity.dest_code = item.dest_code ?? "";
                eneity.PackGroupId = item.PackGroupId;
                eneity.packHeadId = item.packHeadId;

                if (!string.IsNullOrEmpty(item.ExpressNumberParent))
                {
                    eneity.ExpressNumberParent = "母：" + item.ExpressNumberParent;
                    eneity.ExpressNumberParentShow = "1/x";
                }

                eneity.ExpressNumber = item.ExpressNumber;
                eneity.ExpressNumberBar = null;
                eneity.Weight = item.Weight ?? 0;
                eneity.DNBar = null;

                sqlResult.Add(eneity);
            }

            List<OutBoundOrder> list = idal.IOutBoundOrderDAL.SelectBy(u => u.WhCode == first.WhCode && u.OutPoNumber == first.OutPoNumber);
            string clientCode = "";
            if (list.Count > 0)
            {
                OutBoundOrder firstOutbound = list.First();
                clientCode = firstOutbound.ClientCode;
            }

            //添加日志
            TranLog tl = new TranLog();
            tl.TranType = "507";
            if (type == 0)
            {
                tl.Description = "自动打印快递单";
            }
            else
            {
                tl.Description = "手动打印快递单";
            }

            tl.TranDate = DateTime.Now;
            tl.TranUser = userName;
            tl.WhCode = whCode;
            tl.ClientCode = clientCode;
            tl.OutPoNumber = first.OutPoNumber;
            tl.CustomerOutPoNumber = first.CustomerOutPoNumber;
            tl.LoadId = first.LoadId;
            tl.Remark = first.ExpressNumber + content;
            idal.ITranLogDAL.Add(tl);
            idal.SaveChanges();

            return sqlResult;
        }

        //云打印电子面单数据查询
        public List<PackTaskCryReportYunPrintData> GetCryReportYunPrintData(int packHeadId, string whCode, string userName, int type, string content)
        {
            var sql = from a in idal.IPackTaskDAL.SelectAll()
                      join b in idal.IPackHeadDAL.SelectAll() on new { Id = a.Id } equals new { Id = (Int32)b.PackTaskId } into b_join
                      from b in b_join.DefaultIfEmpty()
                      where b.Id == packHeadId
                      select new PackTaskCryReportYunPrintData
                      {
                          WhCode = a.WhCode,
                          LoadId = a.LoadId,
                          OutPoNumber = a.OutPoNumber,
                          CustomerOutPoNumber = a.CustomerOutPoNumber,
                          CustomerPo = a.AltCustomerOutPoNumber,
                          express_code = a.express_code,
                          PackGroupId = b.PackGroupId,
                          PackQty = a.PackQty ?? 0
                      };

            List<PackTaskCryReportYunPrintData> sqlList = sql.ToList();
            if (sqlList.Count == 0)
            {
                return null;
            }

            PackTaskCryReportYunPrintData first = sqlList.First();

            //得到订单Json列表
            //1.得到订单数组
            string[] CustomerOutPoNumber = (from a in sqlList
                                            select a.CustomerOutPoNumber).ToList().Distinct().ToArray();
            //2.一次性拉取订单Json列表
            List<PackTaskJson> packTaskJsonList = (from a in idal.IPackTaskJsonDAL.SelectAll()
                                                   where a.WhCode == first.WhCode && CustomerOutPoNumber.Contains(a.CustomerOutPoNumber)
                                                   select a).ToList();

            List<PackTaskCryReportYunPrintData> sqlResult = new List<PackTaskCryReportYunPrintData>();

            string remark = "";
            List<PackDetail> packDetailList = idal.IPackDetailDAL.SelectBy(u => u.PackHeadId == packHeadId);
            int?[] itemIdArr = (from a in packDetailList
                                select a.ItemId).ToList().Distinct().ToArray();

            List<ItemMaster> itemMasterList = idal.IItemMasterDAL.SelectBy(u => itemIdArr.Contains(u.Id));

            foreach (var packDetail in packDetailList)
            {
                ItemMaster getItem = itemMasterList.Where(u => u.Id == packDetail.ItemId).First();
                remark += getItem.AltItemNumber + "*" + packDetail.Qty + ",";
            }
            if (remark != "")
            {
                remark = remark.Substring(0, remark.Length - 1);
            }


            foreach (var item in sqlList)
            {
                PackTaskJson packJson = new PackTaskJson();
                PackTaskJsonEntity packTaskJsonEntity = new PackTaskJsonEntity();

                if (packTaskJsonList.Where(u => u.CustomerOutPoNumber == item.CustomerOutPoNumber).Count() > 0)
                {
                    packJson = packTaskJsonList.Where(u => u.CustomerOutPoNumber == item.CustomerOutPoNumber).First();

                    packTaskJsonEntity = JsonConvert.DeserializeObject<PackTaskJsonEntity>(packJson.Json);
                }
                if (packTaskJsonEntity == null)
                {
                    continue;
                }

                PackTaskCryReportYunPrintData eneity = new PackTaskCryReportYunPrintData();
                eneity.WhCode = item.WhCode;
                eneity.LoadId = item.LoadId;
                eneity.CustomerOutPoNumber = item.CustomerOutPoNumber;
                eneity.CustomerPo = packTaskJsonEntity.AltCustomerOutPoNumber;
                eneity.ECustomerPo = packTaskJsonEntity.CustomerRef;
                eneity.express_code = packTaskJsonEntity.express_code;
                eneity.PackGroupId = item.PackGroupId;
                eneity.Base64CryDate = packTaskJsonEntity.Base64CryDate;
                eneity.TrackNo = packTaskJsonEntity.TrackNo;
                eneity.EdiJson = packTaskJsonEntity.EdiJson;
                eneity.Sign = packTaskJsonEntity.Sign;
                eneity.ClientCode = packTaskJsonEntity.ClientCode;

                if (item.PackQty == 0)
                {
                    eneity.PackQty = 0;
                }
                else
                {
                    eneity.PackQty = item.PackQty;
                }

                eneity.j_company = packTaskJsonEntity.j_company ?? "";
                eneity.j_contact = packTaskJsonEntity.j_contact ?? "";
                eneity.j_tel = packTaskJsonEntity.j_tel ?? "";
                eneity.j_province = packTaskJsonEntity.j_province ?? "";
                eneity.j_city = packTaskJsonEntity.j_city ?? "";
                eneity.j_county = packTaskJsonEntity.j_county ?? "";
                eneity.j_address = packTaskJsonEntity.j_address ?? "";
                eneity.OutOrderSourceToCloudPrint = packTaskJsonEntity.OutOrderSourceToCloudPrint ?? "";

                List<ItemMaster> getpackDetailList = (from a in idal.IPackDetailDAL.SelectAll()
                                                      join b in idal.IItemMasterDAL.SelectAll()
                                                      on a.ItemId equals b.Id
                                                      where a.PackHeadId == packHeadId
                                                      && (b.InStallService ?? 0) == 1
                                                      select b).ToList();
                if (getpackDetailList.Count > 0 && packTaskJsonEntity.installServiceFlag == "1")
                {
                    eneity.ServiceCode = "IN103";
                }
                else
                {
                    eneity.ServiceCode = "";
                }

                eneity.ProductInfo = remark;

                sqlResult.Add(eneity);
            }

            List<OutBoundOrder> list = idal.IOutBoundOrderDAL.SelectBy(u => u.WhCode == first.WhCode && u.OutPoNumber == first.OutPoNumber);
            string clientCode = "";
            if (list.Count > 0)
            {
                OutBoundOrder firstOutbound = list.First();
                clientCode = firstOutbound.ClientCode;
            }

            //添加日志
            TranLog tl = new TranLog();
            tl.TranType = "820";
            if (type == 0)
            {
                tl.Description = "自动云打印快递单";
            }
            else
            {
                tl.Description = "手动云打印快递单";
            }

            tl.TranDate = DateTime.Now;
            tl.TranUser = userName;
            tl.WhCode = whCode;
            tl.ClientCode = clientCode;
            tl.OutPoNumber = first.OutPoNumber;
            tl.CustomerOutPoNumber = first.CustomerOutPoNumber;
            tl.LoadId = first.LoadId;
            tl.Remark = "云打印仅获取打印数据" + content;
            idal.ITranLogDAL.Add(tl);
            idal.SaveChanges();

            return sqlResult;
        }

        //获取快递单
        public string GetExpressNumber(int packHeadId, string userName, string content)
        {
            using (TransactionScope trans = new TransactionScope())
            {
                try
                {
                    PackHead packHead = idal.IPackHeadDAL.SelectBy(u => u.Id == packHeadId).First();

                    List<PackTask> packTaskList = idal.IPackTaskDAL.SelectBy(u => u.Id == packHead.PackTaskId);
                    if (packTaskList.Count == 0)
                    {
                        return "ERR$未找到包装信息！";
                    }
                    PackTask pack = packTaskList.First();

                    List<PackTaskJson> packTaskJsonList = (from a in idal.IPackTaskJsonDAL.SelectAll()
                                                           where a.WhCode == pack.WhCode && a.CustomerOutPoNumber == pack.CustomerOutPoNumber
                                                           select a).ToList();
                    if (packTaskJsonList.Count == 0)
                    {
                        return "ERR$订单未更新地址信息！";
                    }

                    PackTaskJson packJson = packTaskJsonList.First();
                    PackTaskJsonEntity packTaskJsonEntity = JsonConvert.DeserializeObject<PackTaskJsonEntity>(packJson.Json);

                    if (pack.TransportType != "快递")
                    {
                        return "ERR$当前包装运输方式不属于快递！";
                    }

                    if ((packTaskJsonEntity.j_address ?? "") == "" || (packTaskJsonEntity.j_contact ?? "") == "")
                    {
                        return "ERR$当前包装未维护寄件人或寄件地址！";
                    }

                    if ((packTaskJsonEntity.custid ?? "") == "")
                    {
                        return "ERR$该客户没有月结帐号，无法获取快递单！";
                    }

                    if ((packHead.ExpressNumber == null ? "" : packHead.ExpressNumber) != "")
                    {
                        return "OK$快递单已经获取！";
                    }

                    packTaskJsonEntity.d_contact = packTaskJsonEntity.d_contact.Replace("'", " ");
                    packTaskJsonEntity.d_contact = packTaskJsonEntity.d_contact.Replace("&", " ");
                    packTaskJsonEntity.d_contact = packTaskJsonEntity.d_contact.Replace("$", " ");
                    packTaskJsonEntity.d_contact = packTaskJsonEntity.d_contact.Replace("*", " ");
                    packTaskJsonEntity.d_contact = packTaskJsonEntity.d_contact.Replace("<", " ");
                    packTaskJsonEntity.d_contact = packTaskJsonEntity.d_contact.Replace(">", " ");
                    packTaskJsonEntity.d_contact = packTaskJsonEntity.d_contact.Replace(@"\r\n", " ").Replace(@"\r", " ").Replace(@"\n", " ");

                    packTaskJsonEntity.d_address = packTaskJsonEntity.d_address.Replace('&', ' ');
                    packTaskJsonEntity.d_address = packTaskJsonEntity.d_address.Replace('$', ' ');
                    packTaskJsonEntity.d_address = packTaskJsonEntity.d_address.Replace('"', ' ');
                    packTaskJsonEntity.d_address = packTaskJsonEntity.d_address.Replace(',', ' ');
                    packTaskJsonEntity.d_address = packTaskJsonEntity.d_address.Replace('，', ' ');
                    packTaskJsonEntity.d_address = packTaskJsonEntity.d_address.Replace('<', ' ');
                    packTaskJsonEntity.d_address = packTaskJsonEntity.d_address.Replace('>', ' ');
                    packTaskJsonEntity.d_address = packTaskJsonEntity.d_address.Replace('（', ' ');
                    packTaskJsonEntity.d_address = packTaskJsonEntity.d_address.Replace('）', ' ');
                    packTaskJsonEntity.d_address = packTaskJsonEntity.d_address.Replace("'", " ");
                    packTaskJsonEntity.d_address = packTaskJsonEntity.d_address.Replace("*", " ");
                    packTaskJsonEntity.d_address = packTaskJsonEntity.d_address.Replace("?", " ");
                    packTaskJsonEntity.d_address = packTaskJsonEntity.d_address.Replace("？", " ");

                    packTaskJsonEntity.d_address = packTaskJsonEntity.d_address.Replace(@"\r\n", " ").Replace(@"\r", " ").Replace(@"\n", " ");

                    ExpressResult result = new ExpressResult();

                    //如果前打单已经获取过快递单号，直接更新
                    if ((packTaskJsonEntity.ExpressNumber ?? "") != "")
                    {
                        //更新包装头 的快递单号和获取状态
                        PackHead entity = new PackHead();
                        entity.ExpressNumber = packTaskJsonEntity.ExpressNumber;
                        entity.ExpressMessage = "前打单快递单获取成功！";
                        entity.UpdateUser = userName;
                        entity.UpdateDate = DateTime.Now;

                        //如果电子面单Code不为空 反更新包装任务表
                        if (!string.IsNullOrEmpty(packTaskJsonEntity.dest_code))
                        {
                            PackTask editPackTask = new PackTask();
                            editPackTask.dest_code = packTaskJsonEntity.dest_code;
                            idal.IPackTaskDAL.UpdateBy(editPackTask, u => u.Id == pack.Id, new string[] { "dest_code" });

                            entity.ExpressStatus = "Y";
                        }
                        else
                        {
                            entity.ExpressStatus = "N";
                        }

                        idal.IPackHeadDAL.UpdateBy(entity, u => u.Id == packHeadId, new string[] { "ExpressNumber", "ExpressMessage", "ExpressStatus", "UpdateUser", "UpdateDate" });

                        result.Status = "OK";
                        result.Message = "前打单获取快递单成功！";
                    }
                    else
                    {
                        //开始后打单
                        int? PackGroupId = packHead.PackGroupId;
                        if (!string.IsNullOrEmpty(packHead.ExpressMessage))
                        {
                            if (packHead.ExpressMessage.Contains("重复下单"))
                            {
                                packHead.PackGroupId = packHead.PackGroupId + 1;
                                PackGroupId = packHead.PackGroupId;
                                idal.IPackHeadDAL.UpdateBy(packHead, u => u.Id == packHead.Id, new string[] { "PackGroupId" });
                                idal.SaveChanges();
                            }
                        }

                        //顺丰快递
                        if (pack.express_code == "SF")
                        {
                            //检测是否是子母单 1是子母单
                            if ((pack.ZdFlag ?? 0) == 1)
                            {
                                List<PackHead> checkCount = idal.IPackHeadDAL.SelectBy(u => u.PackTaskId == pack.Id).OrderBy(u => u.Id).ToList();
                                if (checkCount.Count > 200)
                                {
                                    return "ERR$子母单获取数已达上限！";
                                }
                            }
                        }

                        ExpressManager express = new ExpressManager();

                        //顺丰快递
                        if (pack.express_code == "SF")
                        {
                            //得到款号是否需要顺丰的送装服务
                            List<ItemMaster> packDetailList = (from a in idal.IPackDetailDAL.SelectAll()
                                                               join b in idal.IItemMasterDAL.SelectAll()
                                                               on a.ItemId equals b.Id
                                                               where a.PackHeadId == packHeadId
                                                               && (b.InStallService ?? 0) == 1
                                                               select b).ToList();
                            if (packDetailList.Count > 0 && packTaskJsonEntity.installServiceFlag == "1")
                            {
                                SFExpressAPIModel sfModel = new SFExpressAPIModel();

                                sfModel.orderId = pack.OutPoNumber + "-" + PackGroupId;
                                sfModel.waybillNo = packTaskJsonEntity.ExpressNumber ?? "";
                                if (string.IsNullOrEmpty(sfModel.waybillNo))
                                {
                                    sfModel.isGenBillNo = 1;
                                }

                                sfModel.sendCompany = packTaskJsonEntity.j_company;
                                sfModel.sendContact = packTaskJsonEntity.j_contact;
                                sfModel.sendTel = packTaskJsonEntity.j_tel;
                                sfModel.sendMobile = "";
                                sfModel.sendProvince = packTaskJsonEntity.j_province;
                                sfModel.sendCity = packTaskJsonEntity.j_city;
                                sfModel.sendCounty = packTaskJsonEntity.j_county;
                                sfModel.sendAddress = packTaskJsonEntity.j_address;

                                sfModel.deliveryCompany = packTaskJsonEntity.d_company;
                                sfModel.deliveryContact = packTaskJsonEntity.d_contact;
                                if (packTaskJsonEntity.d_tel.Length == 11)
                                {
                                    sfModel.deliveryMobile = packTaskJsonEntity.d_tel;
                                    sfModel.deliveryTel = "";
                                }
                                else if (packTaskJsonEntity.d_tel.Length > 4)
                                {
                                    sfModel.deliveryMobile = "";
                                    sfModel.deliveryTel = packTaskJsonEntity.d_tel;
                                }
                                else
                                {
                                    sfModel.deliveryMobile = "";
                                    sfModel.deliveryTel = packTaskJsonEntity.d_tel;
                                }

                                sfModel.deliveryProvince = packTaskJsonEntity.d_Province;
                                sfModel.deliveryCity = packTaskJsonEntity.d_city;
                                sfModel.deliveryCounty = "";
                                sfModel.deliveryAddress = packTaskJsonEntity.d_address;

                                sfModel.parcelQuantity = 1;
                                sfModel.cargoName = packTaskJsonEntity.j_name;
                                sfModel.customId = packTaskJsonEntity.custid;
                                sfModel.payMethod = 1;
                                sfModel.isDoCall = 0;

                                List<cargoList> carList = new List<cargoList>();
                                cargoList car = new cargoList();
                                car.name = packTaskJsonEntity.j_name;
                                carList.Add(car);
                                sfModel.cargoList = carList;

                                List<AdditionServices> addserList = new List<AdditionServices>();
                                AdditionServices addser = new AdditionServices();
                                addser.name = "HIN";

                                value5 value5 = new value5();
                                value5.serviceType = 2;

                                List<serviceItemInfos> serinfoList = new List<serviceItemInfos>();
                                serviceItemInfos serinfo = new serviceItemInfos();
                                serinfo.count = 1;
                                serinfo.standServiceCode = packDetailList.First().AltItemNumber;
                                serinfo.standServiceName = "";
                                serinfo.cusServiceCode = "";
                                serinfo.cusServiceName = "";
                                serinfoList.Add(serinfo);

                                value5.serviceItemInfos = serinfoList;
                                addser.value5 = value5;
                                addserList.Add(addser);
                                sfModel.AdditionServices = addserList;

                                result = express.GetSFExpressAPI(sfModel);
                                UpdateExpressNumber(packHeadId, userName, pack, result, content);

                                ////检测是否是子母单 1是子母单 0不是子母单
                                //if ((packTaskJsonEntity.ZdFlag ?? 0) == 1)
                                //{
                                //    //订单的第一票快递单号必须为母单，所以验证是否已经获取过快递单号了
                                //    List<PackHead> checkCount = idal.IPackHeadDAL.SelectBy(u => u.PackTaskId == pack.Id && u.Id != packHeadId && (u.ExpressNumber ?? "") != "").OrderBy(u => u.Id).ToList();
                                //    if (checkCount.Count > 0)
                                //    {
                                //        //如果获取过快递单号，取第一个快递单号的 出库订单（必须是第一个订单 母单的订单号） 追加子单，否则获取失败，上面有orderBy
                                //        int? GetPackGroupId = checkCount.First().PackGroupId;

                                //        SFExpressAPIZDModel zdModel = new SFExpressAPIZDModel();
                                //        zdModel.orderId = pack.OutPoNumber + "-" + GetPackGroupId;
                                //        zdModel.count = 1;

                                //        result = express.GetZDSFExpressAPI(zdModel);
                                //        UpdateExpressNumber(packHeadId, userName, pack, result, result.ZDExpressResult);
                                //    }
                                //    else
                                //    {
                                //        //没有获取过快递单号，第一单必须使用母单获取
                                //        result = express.GetSFExpressAPI(sfModel);
                                //        UpdateExpressNumber(packHeadId, userName, pack, result);
                                //    }
                                //}
                                //else
                                //{
                                //    result = express.GetSFExpressAPI(sfModel);
                                //    UpdateExpressNumber(packHeadId, userName, pack, result);
                                //}
                            }
                            else
                            {
                                SFExpressModel sfModel = new SFExpressModel();
                                sfModel.OrderId = pack.OutPoNumber + "-" + PackGroupId;
                                sfModel.CompanyCode = packTaskJsonEntity.companyCode;
                                sfModel.j_company = packTaskJsonEntity.j_company;
                                sfModel.j_contact = packTaskJsonEntity.j_contact;
                                sfModel.j_tel = packTaskJsonEntity.j_tel;
                                sfModel.j_province = packTaskJsonEntity.j_province;
                                sfModel.j_city = packTaskJsonEntity.j_city;
                                sfModel.j_county = packTaskJsonEntity.j_county;
                                sfModel.j_address = packTaskJsonEntity.j_address;
                                sfModel.d_company = packTaskJsonEntity.d_company;
                                sfModel.d_contact = packTaskJsonEntity.d_contact;
                                sfModel.d_tel = packTaskJsonEntity.d_tel;
                                sfModel.d_Province = packTaskJsonEntity.d_Province;
                                sfModel.d_city = packTaskJsonEntity.d_city;
                                sfModel.d_address = packTaskJsonEntity.d_address;
                                sfModel.express_type = packTaskJsonEntity.express_type;
                                sfModel.parcel_quantity = 1;
                                sfModel.Weight = packHead.Weight ?? 0;
                                sfModel.payMethod = packTaskJsonEntity.payMethod;
                                if (string.IsNullOrEmpty(sfModel.payMethod))
                                {
                                    sfModel.payMethod = "1";
                                }

                                sfModel.custid = packTaskJsonEntity.custid;
                                sfModel.cod = Convert.ToDecimal(packTaskJsonEntity.cod);
                                sfModel.j_name = packTaskJsonEntity.j_name;
                                sfModel.issureFlag = (packTaskJsonEntity.issureFlag == null ? "" : packTaskJsonEntity.issureFlag);
                                sfModel.issureMoney = (packTaskJsonEntity.issureMoney == null ? 0 : packTaskJsonEntity.issureMoney);

                                //检测是否是子母单 1是子母单 0不是子母单
                                if ((pack.ZdFlag ?? 0) == 1)
                                {
                                    //订单的第一票快递单号必须为母单，所以验证是否已经获取过快递单号了
                                    List<PackHead> checkCount = idal.IPackHeadDAL.SelectBy(u => u.PackTaskId == pack.Id && u.Id != packHeadId && (u.ExpressNumber ?? "") != "").OrderBy(u => u.Id).ToList();
                                    if (checkCount.Count > 0)
                                    {
                                        //如果获取过快递单号，取第一个快递单号的 出库订单（必须是第一个订单 母单的订单号） 追加子单，否则获取失败，上面有orderBy
                                        int? GetPackGroupId = checkCount.First().PackGroupId;
                                        sfModel.OrderId = pack.OutPoNumber + "-" + GetPackGroupId;

                                        result = express.GetZDExpress(sfModel);
                                        UpdateExpressNumber(packHeadId, userName, pack, result, result.ZDExpressResult);
                                    }
                                    else
                                    {
                                        //没有获取过快递单号，第一单必须使用母单获取
                                        result = express.GetExpress(sfModel);
                                        UpdateExpressNumber(packHeadId, userName, pack, result, content);
                                    }
                                }
                                else
                                {

                                    result = express.GetExpress(sfModel);
                                    UpdateExpressNumber(packHeadId, userName, pack, result, content);
                                }
                            }

                        }
                        else if (pack.express_code == "YTO")            //圆通快递
                        {
                            YTExpressModel ytModel = new YTExpressModel();
                            ytModel.OrderId = pack.OutPoNumber + "-" + PackGroupId;
                            ytModel.CompanyCode = packTaskJsonEntity.companyCode;
                            ytModel.custid = packTaskJsonEntity.custid;
                            ytModel.j_province = packTaskJsonEntity.j_province;
                            ytModel.j_city = packTaskJsonEntity.j_city;
                            ytModel.j_county = packTaskJsonEntity.j_county;
                            ytModel.j_address = packTaskJsonEntity.j_address;
                            ytModel.j_contact = packTaskJsonEntity.j_contact;
                            ytModel.j_tel = packTaskJsonEntity.j_tel;
                            ytModel.d_company = packTaskJsonEntity.d_company;
                            ytModel.d_contact = packTaskJsonEntity.d_contact;
                            ytModel.d_tel = packTaskJsonEntity.d_tel;
                            ytModel.d_Province = packTaskJsonEntity.d_Province;
                            ytModel.d_city = packTaskJsonEntity.d_city;
                            ytModel.d_address = packTaskJsonEntity.d_address;
                            ytModel.parcel_quantity = 1;
                            ytModel.item_name = packTaskJsonEntity.j_name;
                            ytModel.Checkword = packTaskJsonEntity.Checkword;

                            result = express.GetExpress(ytModel);

                            UpdateExpressNumber(packHeadId, userName, pack, result, content);
                        }
                        else if (pack.express_code == "ZTO")            //中通快递
                        {
                            ZTOExpressModel ztModel = new ZTOExpressModel();
                            ztModel.partnerOrderCode = pack.OutPoNumber + "-" + PackGroupId;

                            ztModel.companyid = packTaskJsonEntity.companyCode;
                            ztModel.companypwd = packTaskJsonEntity.companyPwd;
                            ztModel.appserect = packTaskJsonEntity.custid;
                            ztModel.key = packTaskJsonEntity.Checkword;
                            ztModel.orderType = packTaskJsonEntity.express_type;


                            senderInfo send = new senderInfo();
                            send.senderName = packTaskJsonEntity.j_contact;
                            send.senderProvince = packTaskJsonEntity.j_province;
                            send.senderCity = packTaskJsonEntity.j_city;
                            send.senderDistrict = " ";
                            send.senderAddress = packTaskJsonEntity.j_address;
                            send.senderPhone = packTaskJsonEntity.j_tel;
                            ztModel.senderInfo = send;

                            receiveInfo rece = new receiveInfo();
                            rece.receiverName = packTaskJsonEntity.d_contact;
                            rece.receiverProvince = packTaskJsonEntity.d_Province;
                            rece.receiverCity = packTaskJsonEntity.d_city;
                            rece.receiverDistrict = " ";
                            rece.receiverAddress = packTaskJsonEntity.d_address;
                            rece.receiverMobile = packTaskJsonEntity.d_tel;
                            ztModel.receiveInfo = rece;

                            result = express.GetExpress(ztModel);

                            UpdateExpressNumber(packHeadId, userName, pack, result, content);
                        }
                        else
                        {
                            return "ERR$当前快递公司未对接快递单信息！";
                        }
                    }

                    idal.SaveChanges();
                    trans.Complete();

                    string result2 = "";
                    if (result.Message.Contains("重复下单"))
                    {
                        result2 = "操作异常请重新获取！";
                    }
                    //else if (result.Message.Contains("CODE错误"))
                    //{
                    //    result2 = "地址异常请修改订单地址后重新获取！";
                    //}
                    else
                    {
                        result2 = result.Message;
                    }

                    return result.Status + "$" + result2;
                }
                catch
                {
                    trans.Dispose();//出现异常，事务手动释放
                    return "ERR$获取快递单异常，请重新提交！";
                }
            }
        }

        //更新快递单号信息
        private void UpdateExpressNumber(int packHeadId, string userName, PackTask pack, ExpressResult result, string content)
        {
            List<OutBoundOrder> list = idal.IOutBoundOrderDAL.SelectBy(u => u.WhCode == pack.WhCode && u.OutPoNumber == pack.OutPoNumber);
            string clientCode = "";
            if (list.Count > 0)
            {
                OutBoundOrder first = list.First();
                clientCode = first.ClientCode;
            }

            if (result.Status == "OK")
            {
                //更新包装头 的快递单号和获取状态
                PackHead entity = new PackHead();
                if (result.MailNo.IndexOf(",") > -1)
                {
                    result.MailNo = result.MailNo.Substring(0, result.MailNo.IndexOf(","));
                }
                entity.ExpressNumber = result.MailNo;
                entity.ExpressMessage = result.Message + content;
                entity.UpdateUser = userName;
                entity.UpdateDate = DateTime.Now;

                //如果电子面单Code不为空 反更新包装任务表
                if (!string.IsNullOrEmpty(result.DestCode) && result.DestCode != null)
                {
                    PackTask editPackTask = new PackTask();
                    editPackTask.dest_code = result.DestCode;
                    idal.IPackTaskDAL.UpdateBy(editPackTask, u => u.Id == pack.Id, new string[] { "dest_code" });
                }

                entity.ExpressStatus = "Y";
                AddPackHeadJson(pack.WhCode, result);

                //添加日志
                TranLog tl = new TranLog();
                tl.TranType = "506";
                tl.Description = "包装获取快递单";
                tl.TranDate = DateTime.Now;
                tl.TranUser = userName;
                tl.WhCode = pack.WhCode;
                tl.ClientCode = clientCode;
                tl.Remark = result.MailNo + content;
                tl.OutPoNumber = pack.OutPoNumber;
                tl.CustomerOutPoNumber = pack.CustomerOutPoNumber;
                tl.HoldReason = pack.AltCustomerOutPoNumber;
                tl.LoadId = pack.LoadId;
                idal.ITranLogDAL.Add(tl);

                idal.IPackHeadDAL.UpdateBy(entity, u => u.Id == packHeadId, new string[] { "ExpressNumber", "ExpressMessage", "ExpressStatus", "UpdateUser", "UpdateDate" });
            }
            else if (result.Status == "ERR")
            {
                PackHead firstPackHead = idal.IPackHeadDAL.SelectBy(u => u.Id == packHeadId).First();
                if ((firstPackHead.GetExpressNumberErrorFlag ?? 0) == 0)
                {
                    List<FlowHead> getflowHeadList = (from a in idal.ILoadMasterDAL.SelectAll()
                                                      join b in idal.IFlowHeadDAL.SelectAll()
                                                      on a.ProcessId equals b.Id
                                                      where a.WhCode == pack.WhCode && a.LoadId == pack.LoadId
                                                      select b).ToList();

                    if (getflowHeadList.Where(u => (u.UrlEdiId3 ?? 0) != 0).Count() > 0)
                    {
                        FlowHead first = getflowHeadList.Where(u => (u.UrlEdiId3 ?? 0) != 0).First();
                        List<UrlEdi> urlList = idal.IUrlEdiDAL.SelectBy(u => u.Id == first.UrlEdiId3).ToList();

                        if (urlList.Count > 0)
                        {
                            UrlEdi url = urlList.First();

                            UrlEdiTask uet2 = new UrlEdiTask();
                            uet2.WhCode = pack.WhCode;
                            uet2.Type = "OMS";
                            uet2.Url = url.Url + "&WhCode=" + pack.WhCode;
                            uet2.Field = url.Field;
                            uet2.Mark = packHeadId.ToString();
                            uet2.HttpType = url.HttpType;
                            uet2.Status = 1;
                            uet2.CreateDate = DateTime.Now;
                            idal.IUrlEdiTaskDAL.Add(uet2);
                        }
                    }
                }

                //失败 更新获取状态
                PackHead entity = new PackHead();
                entity.GetExpressNumberErrorFlag = 1;
                entity.ExpressStatus = "N";
                entity.ExpressMessage = result.Message;

                if (result.Message.Contains("重复下单"))
                {
                    entity.ExpressMessage = "操作异常请重新获取！";
                }
                //else if (result.Message.Contains("CODE错误"))
                //{
                //    entity.ExpressMessage = "地址异常请修改订单地址后重新获取！";
                //}

                if (entity.ExpressMessage.Length > 200)
                {
                    entity.ExpressMessage = entity.ExpressMessage.Substring(0, 150);
                }

                entity.UpdateUser = userName;
                entity.UpdateDate = DateTime.Now;

                idal.IPackHeadDAL.UpdateBy(entity, u => u.Id == packHeadId, new string[] { "ExpressStatus", "GetExpressNumberErrorFlag", "ExpressMessage", "UpdateUser", "UpdateDate" });
            }
        }


        //云打印更新快递单号信息
        public void UpdateExpressNumberByYunPrint(int packHeadId, string userName, string message)
        {
            PackHead packHead = idal.IPackHeadDAL.SelectBy(u => u.Id == packHeadId).First();
            PackTask pack = idal.IPackTaskDAL.SelectBy(u => u.Id == packHead.PackTaskId).First();

            List<OutBoundOrder> list = idal.IOutBoundOrderDAL.SelectBy(u => u.WhCode == pack.WhCode && u.OutPoNumber == pack.OutPoNumber);
            string clientCode = "";
            if (list.Count > 0)
            {
                OutBoundOrder first = list.First();
                clientCode = first.ClientCode;
            }

            //添加日志
            TranLog tl = new TranLog();
            tl.TranType = "506";
            tl.Description = "云打印获取快递单";
            tl.TranDate = DateTime.Now;
            tl.TranUser = userName;
            tl.WhCode = pack.WhCode;
            tl.ClientCode = clientCode;
            tl.OutPoNumber = pack.OutPoNumber;
            tl.CustomerOutPoNumber = pack.CustomerOutPoNumber;
            tl.HoldReason = pack.AltCustomerOutPoNumber;
            tl.LoadId = pack.LoadId;

            if (message.Split('$')[0] == "Y")
            {
                //更新包装头 的快递单号和获取状态
                PackHead entity = new PackHead();
                entity.ExpressNumber = message.Split('$')[1];
                entity.ExpressMessage = message.Split('$')[2];
                entity.ExpressNumberParent = message.Split('$')[3];

                entity.ExpressStatus = "Y";
                entity.UpdateUser = userName;
                entity.UpdateDate = DateTime.Now;

                tl.Remark = message.Split('$')[1] + message.Split('$')[2];

                idal.IPackHeadDAL.UpdateBy(entity, u => u.Id == packHeadId, new string[] { "ExpressNumber", "ExpressMessage", "ExpressStatus", "UpdateUser", "UpdateDate", "ExpressNumberParent" });
            }
            else
            {
                //失败 更新获取状态
                PackHead entity = new PackHead();
                entity.ExpressStatus = "N";
                entity.ExpressMessage = message.Split('$')[1];
                if (entity.ExpressMessage.Length > 200)
                {
                    entity.ExpressMessage = "云打印获取失败！" + entity.ExpressMessage.Substring(0, 150);
                }

                entity.UpdateUser = userName;
                entity.UpdateDate = DateTime.Now;

                if (entity.ExpressMessage.Length > 100)
                {
                    tl.Remark = message.Split('$')[1].Substring(0, 50);
                }
                else
                {
                    tl.Remark = message.Split('$')[1];
                }

                idal.IPackHeadDAL.UpdateBy(entity, u => u.Id == packHeadId, new string[] { "ExpressStatus", "ExpressMessage", "UpdateUser", "UpdateDate" });
            }

            idal.ITranLogDAL.Add(tl);

            idal.SaveChanges();
        }



        //手动更新快递单号信息
        public string UpdateExpressNumberByWork(int packHeadId, string userName, string message)
        {
            PackHead packHead = idal.IPackHeadDAL.SelectBy(u => u.Id == packHeadId).First();
            PackTask pack = idal.IPackTaskDAL.SelectBy(u => u.Id == packHead.PackTaskId).First();

            List<OutBoundOrder> list = idal.IOutBoundOrderDAL.SelectBy(u => u.WhCode == pack.WhCode && u.OutPoNumber == pack.OutPoNumber);
            string clientCode = "";
            if (list.Count > 0)
            {
                OutBoundOrder first = list.First();
                clientCode = first.ClientCode;
            }

            //添加日志
            TranLog tl = new TranLog();
            tl.TranType = "516";
            tl.Description = "手动更新快递单";
            tl.TranDate = DateTime.Now;
            tl.TranUser = userName;
            tl.WhCode = pack.WhCode;
            tl.ClientCode = clientCode;
            tl.OutPoNumber = pack.OutPoNumber;
            tl.CustomerOutPoNumber = pack.CustomerOutPoNumber;
            tl.HoldReason = pack.AltCustomerOutPoNumber;
            tl.LoadId = pack.LoadId;

            if (message.Split('$')[0] == "Y")
            {
                //更新包装头 的快递单号和获取状态
                PackHead entity = new PackHead();
                entity.ExpressNumber = message.Split('$')[1];

                List<PackHead> checkList = idal.IPackHeadDAL.SelectBy(u => u.ExpressNumber == entity.ExpressNumber && u.WhCode == pack.WhCode);
                if (checkList.Count > 0)
                {
                    return "快递单号" + entity.ExpressNumber + "已使用，无法再次使用！";
                }

                entity.ExpressMessage = message.Split('$')[2];
                entity.ExpressStatus = "Y";
                entity.UpdateUser = userName;
                entity.UpdateDate = DateTime.Now;

                tl.Remark = message.Split('$')[1] + message.Split('$')[2];

                idal.IPackHeadDAL.UpdateBy(entity, u => u.Id == packHeadId, new string[] { "ExpressNumber", "ExpressMessage", "ExpressStatus", "UpdateUser", "UpdateDate" });
            }
            else
            {
                //失败 更新获取状态
                PackHead entity = new PackHead();
                entity.ExpressStatus = "N";
                entity.ExpressMessage = message.Split('$')[1];
                if (entity.ExpressMessage.Length > 200)
                {
                    entity.ExpressMessage = "更新快递单失败！" + entity.ExpressMessage.Substring(0, 150);
                }

                entity.UpdateUser = userName;
                entity.UpdateDate = DateTime.Now;

                tl.Remark = message.Split('$')[1];

                idal.IPackHeadDAL.UpdateBy(entity, u => u.Id == packHeadId, new string[] { "ExpressStatus", "ExpressMessage", "UpdateUser", "UpdateDate" });
            }

            idal.ITranLogDAL.Add(tl);
            idal.SaveChanges();
            return "Y";
        }


        //插入顺丰快递加密单其它信息
        public void AddPackHeadJson(string whcode, ExpressResult result)
        {
            if (result.sFGetDetailModel != null)
            {
                //得到顺丰密单明细信息
                string json = JsonConvert.SerializeObject(result.sFGetDetailModel);

                PackHeadJson packHeadJson = new PackHeadJson();
                packHeadJson.WhCode = whcode;
                packHeadJson.ExpressNumber = result.MailNo;
                packHeadJson.Json = json;
                packHeadJson.CreateDate = DateTime.Now;
                idal.IPackHeadJsonDAL.Add(packHeadJson);
                idal.SaveChanges();
            }

            if (result.zTOGetDetailModel != null)
            {
                //得到中通明细信息
                string json = JsonConvert.SerializeObject(result.zTOGetDetailModel);

                PackHeadJson packHeadJson = new PackHeadJson();
                packHeadJson.WhCode = whcode;
                packHeadJson.ExpressNumber = result.MailNo;
                packHeadJson.Json = json;
                packHeadJson.CreateDate = DateTime.Now;
                idal.IPackHeadJsonDAL.Add(packHeadJson);
                idal.SaveChanges();
            }

        }

        //更新子母单快递单号信息
        private void UpdateExpressNumber(int packHeadId, string userName, PackTask pack, ExpressResult result, List<ZDExpressResult> ZDExpressResultList)
        {
            List<OutBoundOrder> list = idal.IOutBoundOrderDAL.SelectBy(u => u.WhCode == pack.WhCode && u.OutPoNumber == pack.OutPoNumber);
            string clientCode = "";
            if (list.Count > 0)
            {
                OutBoundOrder first = list.First();
                clientCode = first.ClientCode;
            }

            if (result.Status == "OK")
            {
                string ExpressNumber = "";
                foreach (var item in ZDExpressResultList)
                {
                    ExpressNumber = item.MailNo_ZD;
                }
                //更新包装头 的快递单号和获取状态
                PackHead entity = new PackHead();
                entity.ExpressNumber = ExpressNumber;
                entity.ExpressNumberParent = result.MailNo;
                entity.ExpressMessage = result.Message;
                entity.UpdateUser = userName;
                entity.UpdateDate = DateTime.Now;
                entity.ExpressStatus = "Y";

                AddPackHeadJson(pack.WhCode, result);

                //添加日志
                TranLog tl = new TranLog();
                tl.TranType = "506";
                tl.Description = "包装获取快递单";
                tl.TranDate = DateTime.Now;
                tl.TranUser = userName;
                tl.WhCode = pack.WhCode;
                tl.ClientCode = clientCode;
                tl.Remark = ExpressNumber;
                tl.OutPoNumber = pack.OutPoNumber;
                tl.CustomerOutPoNumber = pack.CustomerOutPoNumber;
                tl.HoldReason = pack.AltCustomerOutPoNumber;
                tl.LoadId = pack.LoadId;
                idal.ITranLogDAL.Add(tl);

                idal.IPackHeadDAL.UpdateBy(entity, u => u.Id == packHeadId, new string[] { "ExpressNumber", "ExpressNumberParent", "ExpressMessage", "ExpressStatus", "UpdateUser", "UpdateDate" });
            }
            else if (result.Status == "ERR")
            {
                PackHead firstPackHead = idal.IPackHeadDAL.SelectBy(u => u.Id == packHeadId).First();
                if ((firstPackHead.GetExpressNumberErrorFlag ?? 0) == 0)
                {
                    List<FlowHead> getflowHeadList = (from a in idal.ILoadMasterDAL.SelectAll()
                                                      join b in idal.IFlowHeadDAL.SelectAll()
                                                      on a.ProcessId equals b.Id
                                                      where a.WhCode == pack.WhCode && a.LoadId == pack.LoadId
                                                      select b).ToList();

                    if (getflowHeadList.Where(u => (u.UrlEdiId3 ?? 0) != 0).Count() > 0)
                    {
                        FlowHead first = getflowHeadList.Where(u => (u.UrlEdiId3 ?? 0) != 0).First();
                        List<UrlEdi> urlList = idal.IUrlEdiDAL.SelectBy(u => u.Id == first.UrlEdiId3).ToList();

                        if (urlList.Count > 0)
                        {
                            UrlEdi url = urlList.First();

                            UrlEdiTask uet2 = new UrlEdiTask();
                            uet2.WhCode = pack.WhCode;
                            uet2.Type = "OMS";
                            uet2.Url = url.Url + "&WhCode=" + pack.WhCode;
                            uet2.Field = url.Field;
                            uet2.Mark = packHeadId.ToString();
                            uet2.HttpType = url.HttpType;
                            uet2.Status = 1;
                            uet2.CreateDate = DateTime.Now;
                            idal.IUrlEdiTaskDAL.Add(uet2);
                        }
                    }
                }

                //失败 更新获取状态
                PackHead entity = new PackHead();
                entity.ExpressStatus = "N";
                entity.GetExpressNumberErrorFlag = 1;
                entity.ExpressMessage = result.Message;
                if (entity.ExpressMessage.Length > 200)
                {
                    entity.ExpressMessage = entity.ExpressMessage.Substring(0, 150);
                }
                entity.UpdateUser = userName;
                entity.UpdateDate = DateTime.Now;

                idal.IPackHeadDAL.UpdateBy(entity, u => u.Id == packHeadId, new string[] { "ExpressStatus", "GetExpressNumberErrorFlag", "ExpressMessage", "UpdateUser", "UpdateDate" });

            }
        }

        //出货进度查询
        public List<LoadProcedureResult> GetLoadProcedureList(LoadProcedureSearch searchEntity, out int total)
        {
            var sql = (from a in idal.ISortTaskDetailDAL.SelectAll()
                       join b in idal.IOutBoundOrderDAL.SelectAll()
                       on new { a.OutPoNumber, a.WhCode }
                        equals new { b.OutPoNumber, b.WhCode } into b_join
                       from b in b_join.DefaultIfEmpty()
                       join c in idal.IPackTaskDAL.SelectAll()
                       on new { a.OutPoNumber, a.WhCode, a.LoadId, a.GroupId } equals new { c.OutPoNumber, c.WhCode, c.LoadId, GroupId = c.SortGroupId } into c_join
                       from c in c_join.DefaultIfEmpty()
                       join d in idal.IPackHeadDAL.SelectAll() on new { Id = c.Id } equals new { Id = (Int32)d.PackTaskId } into d_join
                       from d in d_join.DefaultIfEmpty()
                       join h in idal.IFlowHeadDAL.SelectAll()
                       on b.ProcessId equals h.Id
                       join e in idal.IItemMasterDAL.SelectAll()
                       on a.ItemId equals e.Id
                       where a.WhCode == searchEntity.WhCode && a.PlanQty != a.TransferQty
                       select new LoadProcedureResult
                       {
                           LoadId = a.LoadId,
                           ClientCode = b.ClientCode,
                           AltCustomerOutPoNumber = b.AltCustomerOutPoNumber,
                           CustomerOutPoNumber = b.CustomerOutPoNumber,
                           StatusName = b.StatusName,
                           AltItemNumber = a.AltItemNumber,
                           ItemName = e.ItemName ?? "",
                           EAN = a.EAN,
                           PlanQty = a.PlanQty,
                           PickQty = a.PickQty,
                           Qty = a.Qty,
                           PackQty = a.PackQty,
                           TransferQty = a.TransferQty,
                           SortGroupNumber = a.GroupNumber,
                           UpdateUser = a.UpdateUser,
                           UpdateDate = a.UpdateDate,
                           CreateDate = a.CreateDate,
                           ExpressNumber = d.ExpressNumber,
                           ExpressMessage = d.ExpressMessage,
                           OrderDate = b.CreateDate,
                           OrderType = b.OrderType,
                           OrderSource = b.OrderSource,
                           ProcessName = h.FlowName
                       }).Distinct();

            if (!string.IsNullOrEmpty(searchEntity.LoadId))
                sql = sql.Where(u => u.LoadId.Contains(searchEntity.LoadId));
            if (!string.IsNullOrEmpty(searchEntity.ClientCode))
                sql = sql.Where(u => u.ClientCode == searchEntity.ClientCode);

            if (!string.IsNullOrEmpty(searchEntity.CustomerOutPoNumber))
            {
                sql = sql.Where(u => u.CustomerOutPoNumber.Contains(searchEntity.CustomerOutPoNumber));
            }

            if (!string.IsNullOrEmpty(searchEntity.AltCustomerOutPoNumber))
            {
                sql = sql.Where(u => u.AltCustomerOutPoNumber.Contains(searchEntity.AltCustomerOutPoNumber));
            }
            if (!string.IsNullOrEmpty(searchEntity.EAN))
            {
                sql = sql.Where(u => u.EAN.Contains(searchEntity.EAN));
            }

            if (!string.IsNullOrEmpty(searchEntity.StatusName))
            {
                sql = sql.Where(u => u.StatusName.Contains(searchEntity.StatusName));
            }

            if (!string.IsNullOrEmpty(searchEntity.AltItemNumber))
            {
                sql = sql.Where(u => u.AltItemNumber.Contains(searchEntity.AltItemNumber));
            }
            if (searchEntity.BeginCreateDate != null)
            {
                sql = sql.Where(u => u.CreateDate >= searchEntity.BeginCreateDate);
            }
            if (searchEntity.EndCreateDate != null)
            {
                sql = sql.Where(u => u.CreateDate < searchEntity.EndCreateDate);
            }

            if (searchEntity.BeginOrderDate != null)
            {
                sql = sql.Where(u => u.OrderDate >= searchEntity.BeginOrderDate);
            }
            if (searchEntity.EndOrderDate != null)
            {
                sql = sql.Where(u => u.OrderDate < searchEntity.EndOrderDate);
            }

            total = sql.Count();
            sql = sql.OrderBy(u => u.CreateDate).Skip((searchEntity.pageSize) * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);

            return sql.ToList();

        }


        //获取DM条码打印
        public PackTaskResult GetBarCodeList(int packHeadId)
        {
            List<PackTaskResult> sql = (from a in idal.IPackHeadDAL.SelectAll()
                                        join b in idal.IPackTaskDAL.SelectAll() on new { PackTaskId = (Int32)a.PackTaskId } equals new { PackTaskId = b.Id } into b_join
                                        from b in b_join.DefaultIfEmpty()
                                        join c in idal.IPackDetailDAL.SelectAll() on new { Id = a.Id } equals new { Id = (Int32)c.PackHeadId } into c_join
                                        from c in c_join.DefaultIfEmpty()
                                        where a.Id == packHeadId
                                        group new { b, c } by new
                                        {
                                            b.WhCode,
                                            b.CustomerOutPoNumber
                                        } into g
                                        select new PackTaskResult
                                        {
                                            WhCode = g.Key.WhCode,
                                            CustomerPoNumber = g.Key.CustomerOutPoNumber,
                                            BarcodePrintCount = g.Sum(p => p.c.Qty)
                                        }).ToList();

            PackTaskResult sqlResult = new PackTaskResult();
            if (sql.Count > 0)
            {
                PackTaskResult packTaskResult = sql.First();

                List<PackTaskJson> packTaskJsonList = (from a in idal.IPackTaskJsonDAL.SelectAll()
                                                       where a.WhCode == packTaskResult.WhCode && a.CustomerOutPoNumber == packTaskResult.CustomerPoNumber
                                                       select a).ToList();
                if (packTaskJsonList.Count == 0)
                {
                    return null;
                }
                else
                {
                    PackTaskJson packJson = packTaskJsonList.First();
                    PackTaskJsonEntity packTaskJsonEntity = JsonConvert.DeserializeObject<PackTaskJsonEntity>(packJson.Json);

                    if (packTaskJsonEntity.ClientCode == "DM" || packTaskJsonEntity.ClientCode == "dmTest")
                    {
                        sqlResult.customer_Po = packTaskJsonEntity.customerPo;
                        sqlResult.outorder_number_alt = packTaskJsonEntity.AltCustomerOutPoNumber;
                        sqlResult.BarcodePrintCount = packTaskResult.BarcodePrintCount;
                        sqlResult.customer_Po_Bar = null;
                        sqlResult.outorder_number_alt_Bar = null;
                        sqlResult.SortGroupId = 0;
                        sqlResult.SortGroupNumber = "DM_barCode";

                        return sqlResult;
                    }
                    else if (packTaskJsonEntity.ClientCode == "Christofle")
                    {
                        string remark = "";
                        List<PackDetail> packDetailList = idal.IPackDetailDAL.SelectBy(u => u.PackHeadId == packHeadId);
                        int?[] itemIdArr = (from a in packDetailList
                                            select a.ItemId).ToList().Distinct().ToArray();

                        List<ItemMaster> itemMasterList = idal.IItemMasterDAL.SelectBy(u => itemIdArr.Contains(u.Id));

                        foreach (var packDetail in packDetailList)
                        {
                            ItemMaster getItem = itemMasterList.Where(u => u.Id == packDetail.ItemId).First();
                            remark += getItem.AltItemNumber + "*" + packDetail.Qty + ",";
                        }

                        sqlResult.customer_Po = "";
                        sqlResult.outorder_number_alt = packTaskJsonEntity.AltCustomerOutPoNumber;
                        sqlResult.BarcodePrintCount = 1;
                        sqlResult.customer_Po_Bar = null;
                        sqlResult.outorder_number_alt_Bar = null;
                        sqlResult.SortGroupId = 0;
                        sqlResult.SortGroupNumber = "CHR_barCode";
                        sqlResult.Remark = remark.Substring(0, remark.Length - 1);

                        return sqlResult;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            else
            {
                return null;
            }
        }


        //验证是否已打印过包装面单
        public string CheckPackPrintDate(int packHeadId)
        {
            string result = "";
            List<PackHead> list = idal.IPackHeadDAL.SelectBy(u => u.Id == packHeadId);
            if (list.Count == 0)
            {
                return "未找到包装信息！";
            }
            else
            {
                if (list.Where(u => (u.PrintDate == null ? "" : (u.PrintDate.ToString() ?? "")) != "").Count() == 0)
                {
                    PackHead packHead = list.First();

                    result = "Y";
                    packHead.PrintDate = DateTime.Now;

                    idal.IPackHeadDAL.UpdateBy(packHead, u => u.Id == packHead.Id, new string[] { "PrintDate" });
                    idal.SaveChanges();
                }
                else
                {
                    result = "面单已打印过！";
                }
            }

            return result;
        }

        //日志添加
        public string TranLogAdd(TranLog entity)
        {
            if ((entity.HoldId ?? 0) != 0)
            {
                var sql = from a in idal.IPackTaskDAL.SelectAll()
                          join b in idal.IPackHeadDAL.SelectAll() on new { Id = a.Id } equals new { Id = (Int32)b.PackTaskId } into b_join
                          from b in b_join.DefaultIfEmpty()
                          where b.Id == entity.HoldId
                          select new PackTaskCryReportExpress
                          {
                              WhCode = a.WhCode,
                              LoadId = a.LoadId,
                              CustomerOutPoNumber = a.CustomerOutPoNumber,
                              PackGroupId = b.PackGroupId,
                              ExpressNumber = b.ExpressNumber,
                              ExpressNumberBar = null,
                              Remark = a.OutPoNumber,
                              Weight = b.Weight ?? 0,
                              DNBar = null
                          };

                List<PackTaskCryReportExpress> sqlList = sql.ToList();
                if (sqlList.Count > 0)
                {
                    PackTaskCryReportExpress first = sqlList.First();

                    entity.OutPoNumber = first.Remark;
                    entity.CustomerOutPoNumber = first.CustomerOutPoNumber;
                    entity.LoadId = first.LoadId;
                    entity.Remark = first.ExpressNumber;
                }
            }

            entity.TranDate = DateTime.Now;
            idal.ITranLogDAL.Add(entity);
            idal.SaveChanges();
            return "Y";
        }


        //通过包装头得到款号耗材及重量
        public List<ItemMaster> GetItemMasterByPackHeadId(int packHeadId)
        {
            List<PackDetail> list = idal.IPackDetailDAL.SelectBy(u => u.PackHeadId == packHeadId);
            if (list.Count == 0)
            {
                return null;
            }

            PackDetail packdetail = list.First();
            return idal.IItemMasterDAL.SelectBy(u => u.Id == packdetail.ItemId).ToList();
        }


        //批量获取顺丰加密数据
        public List<PackHeadJson> GetPackHeadJsonByExpress(string whCode, string[] expressNumber)
        {
            var sql = idal.IPackHeadJsonDAL.SelectBy(u => u.WhCode == whCode && expressNumber.Contains(u.ExpressNumber));
            return sql.ToList();
        }



        public string GetSFExpressDownUrlAPI()
        {
            ExpressManager express = new ExpressManager();

            SFExpressDownUrlModel sfModel = new SFExpressDownUrlModel();

            sfModel.templateCode = "fm_76130_standard_YOGJWQZ849YE";
            sfModel.version = "2.0";
            sfModel.fileType = "pdf";
            sfModel.sync = "true";
            sfModel.customTemplateCode = "fm_76130_standard_custom_10036305045_1";
            //021-88117110

            List<documents> List = new List<documents>();
            documents car = new documents();
            car.masterWaybillNo = "SF3101400836180";
            car.waybillNoCheckType = "2";
            car.waybillNoCheckValue = "117110";

            customData cusdate = new customData();
            cusdate.Remark = "客户订单号：";
            car.customData = cusdate;
            List.Add(car);

            sfModel.documents = List;

            //string orderId = pack.OutPoNumber + "-" + PackGroupId;
            string orderId = "OMS24112716115213767" + "-" + 1;
            ExpressResult result = new ExpressResult();
            result = express.GetSFExpressDownUrlAPI(sfModel, orderId);

            return result.Message;
        }


        #region  力士乐包装工作台

        //验证 包装框号状态
        public string BoschCheckPackTaskStatus(string sortGroupNumber, string whCode)
        {
            List<PackTask> PackTaskList = (from a in idal.IPackHeadDAL.SelectAll()
                                           join b in idal.IPackTaskDAL.SelectAll()
                                           on a.PackTaskId equals b.Id
                                           where a.WhCode == whCode && a.PackNumber.StartsWith(sortGroupNumber) && a.PackNumber.Contains("_")
                                           select b).ToList();

            if (PackTaskList.Count == 0)
            {
                return "包装框号有误！";
            }

            if (PackTaskList.Where(u => u.Status == -10).Count() > 0)
            {
                return "包装框号对应订单被拦截！";
            }

            //if (PackTaskList.Where(u => (u.Json ?? "") == "").Count() > 0)
            //{
            //    return "包装框号对应订单信息未更新！";
            //}

            //if (PackTaskList.Where(u => u.Status == 30).Count() > 0)
            //{
            //    return "包装框号已完成包装！";
            //}

            List<PackHead> PackHeadList = (from a in idal.IPackHeadDAL.SelectAll()
                                           join b in idal.IPackTaskDAL.SelectAll()
                                           on a.PackTaskId equals b.Id
                                           where a.WhCode == whCode && a.PackNumber.StartsWith(sortGroupNumber) && a.PackNumber.Contains("_")
                                           select a).ToList();

            if (PackHeadList.Where(u => u.Status == 0).Count() == 0)
            {
                return "包装框号已称重！";
            }

            return "Y";
        }

        //获取包装任务信息
        public PackTask GetBoschPackTask(string sortGroupNumber, string whCode)
        {
            List<PackTask> PackTaskList = (from a in idal.IPackHeadDAL.SelectAll()
                                           join b in idal.IPackTaskDAL.SelectAll()
                                           on a.PackTaskId equals b.Id
                                           where a.WhCode == whCode && a.PackNumber.StartsWith(sortGroupNumber) && b.Status >= 10 && a.Status == 0 && a.PackNumber.Contains("_")
                                           select b).ToList();
            if (PackTaskList.Count == 0)
            {
                return null;
            }

            return PackTaskList.First();
        }

        //获取包装框号等信息
        public PackHead GetBoschPackHead(string sortGroupNumber, string whCode)
        {
            List<PackHead> PackTaskList = (from a in idal.IPackHeadDAL.SelectAll()
                                           join b in idal.IPackTaskDAL.SelectAll()
                                           on a.PackTaskId equals b.Id
                                           where a.WhCode == whCode && a.PackNumber.StartsWith(sortGroupNumber) && b.Status >= 10 && a.Status == 0 && a.PackNumber.Contains("_")
                                           select a).ToList();
            if (PackTaskList.Count == 0)
            {
                return null;
            }

            return PackTaskList.First();
        }

        //博士更新重量 耗材等信息
        public string UpdateBoschPackHead(PackHead entity)
        {
            List<PackHead> packHeadList = idal.IPackHeadDAL.SelectBy(u => u.Id == entity.Id);
            if (packHeadList.Count == 0)
            {
                return "包装明细不存在！";
            }

            if (packHeadList.Where(u => (u.TransferHeadId ?? "") != "").Count() > 0)
            {
                return "包装正在交接，请先删除交接！";
            }

            string getpackNumber = packHeadList.First().PackNumber;
            string packNumber = getpackNumber.Substring(0, getpackNumber.IndexOf('_'));

            //检测耗材是否存在
            List<Loss> lostList = idal.ILossDAL.SelectBy(u => u.WhCode == entity.WhCode && u.LossCode == entity.PackCarton);
            if (lostList.Count != 0)
            {
                Loss loss = lostList.First();
                loss.Qty = loss.Qty - 1;
                idal.ILossDAL.UpdateBy(loss, u => u.Id == loss.Id, new string[] { "Qty" });

                PackTask pack = (from a in idal.IPackHeadDAL.SelectAll()
                                 join b in idal.IPackTaskDAL.SelectAll()
                                 on a.PackTaskId equals b.Id
                                 where a.Id == entity.Id
                                 select b).ToList().Distinct().ToList().First();

                //添加日志
                TranLog tl = new TranLog();
                tl.TranType = "505";
                tl.Description = "包装耗材";
                tl.TranDate = DateTime.Now;
                tl.TranUser = entity.UpdateUser;
                tl.WhCode = loss.WhCode;
                tl.ClientCode = loss.ClientCode;
                tl.Length = entity.Length;
                tl.Width = entity.Width;
                tl.Height = entity.Height;
                tl.Weight = entity.Weight;
                tl.TranQty = -1;
                tl.SoNumber = entity.PackCarton;
                tl.CustomerPoNumber = entity.PackCarton;
                tl.AltItemNumber = entity.PackCarton;
                tl.OutPoNumber = pack.OutPoNumber;
                tl.CustomerOutPoNumber = pack.CustomerOutPoNumber;
                tl.HoldReason = pack.AltCustomerOutPoNumber;
                tl.LoadId = pack.LoadId;
                idal.ITranLogDAL.Add(tl);

            }
            entity.Status = 10;
            entity.UpdateDate = DateTime.Now;

            idal.IPackHeadDAL.UpdateBy(entity, u => u.WhCode == entity.WhCode && u.PackNumber.StartsWith(packNumber) && u.PackNumber.Contains("_"), new string[] { "Length", "Width", "Height", "Weight", "PackCarton", "Status", "UpdateUser", "UpdateDate" });

            idal.IPackHeadDAL.SaveChanges();


            //检测该Load是否全部完成称重，如果全部完成称重，检查总重量
            PackTask packTask = (from a in idal.IPackTaskDAL.SelectAll()
                                 join b in idal.IPackHeadDAL.SelectAll()
                                 on a.Id equals b.PackTaskId
                                 where b.Id == entity.Id
                                 select a).ToList().Distinct().ToList().First();

            //获取分拣信息
            List<SortTaskDetail> checkSortTaskDetailList = idal.ISortTaskDetailDAL.SelectBy(u => u.WhCode == packTask.WhCode && u.LoadId == packTask.LoadId && u.PlanQty != u.PackQty);

            //备货数量不等于包装数量 的情况为0行
            if (checkSortTaskDetailList.Count == 0)
            {
                List<PackHead> checkPackHeadList = (from a in idal.IPackTaskDAL.SelectAll()
                                                    join b in idal.IPackHeadDAL.SelectAll()
                                                    on a.Id equals b.PackTaskId
                                                    where a.WhCode == packTask.WhCode && a.LoadId == packTask.LoadId
                                                    select b).ToList().Distinct().ToList();

                //都已经称重后 根据重量 修改出货类型
                if (checkPackHeadList.Where(u => u.Status == 0).Count() == 0 && checkPackHeadList.Count > 0)
                {
                    decimal? sumWeight = 0;

                    List<string> packNumberlist = new List<string>();
                    foreach (var item in checkPackHeadList)
                    {
                        string getpackNumber1 = item.PackNumber;
                        string packNumber1 = getpackNumber1.Substring(0, getpackNumber1.IndexOf('_'));

                        if (packNumberlist.Contains(packNumber1) == false)
                        {
                            packNumberlist.Add(packNumber1);
                            sumWeight += item.Weight ?? 0;
                        }
                    }

                    //重量小于50KG 变更为快递
                    if (sumWeight < 50)
                    {
                        if (packTask.TransportType == "物流")
                        {
                            PackTask editPack = new PackTask();
                            editPack.TransportType = "快递";
                            editPack.express_code = "SF";
                            editPack.express_type = "2";
                            editPack.express_type_zh = "顺丰隔日";
                            idal.IPackTaskDAL.UpdateBy(editPack, u => u.WhCode == packTask.WhCode && u.LoadId == packTask.LoadId, new string[] { "TransportType", "express_code", "express_type", "express_type_zh" });
                        }
                    }
                    else
                    {
                        //重量大于等于50KG 变更为物流
                        if (packTask.TransportType == "快递")
                        {
                            List<PackTaskJson> packTaskJsonList = (from a in idal.IPackTaskJsonDAL.SelectAll()
                                                                   where a.WhCode == packTask.WhCode && a.CustomerOutPoNumber == packTask.CustomerOutPoNumber
                                                                   select a).ToList();
                            if (packTaskJsonList.Count > 0)
                            {
                                PackTaskJson packJson = packTaskJsonList.First();

                                PackTaskJsonEntity packTaskJsonEntity = JsonConvert.DeserializeObject<PackTaskJsonEntity>(packJson.Json);

                                PackTask editPack = new PackTask();
                                editPack.TransportType = packTaskJsonEntity.TransportType;
                                editPack.express_code = packTaskJsonEntity.express_code;
                                editPack.express_type = "";
                                editPack.express_type_zh = "";
                                idal.IPackTaskDAL.UpdateBy(editPack, u => u.WhCode == packTask.WhCode && u.LoadId == packTask.LoadId, new string[] { "TransportType", "express_code", "express_type", "express_type_zh" });
                            }

                        }
                    }
                }
            }

            idal.IPackHeadDAL.SaveChanges();
            return "Y";
        }

        //博士更新包装框号
        public string UpdateBoschPackNumber(int packHeadId, string setpackNumber)
        {
            List<PackHead> packHeadList = idal.IPackHeadDAL.SelectBy(u => u.Id == packHeadId);
            if (packHeadList.Count == 0)
            {
                return "包装明细不存在！";
            }

            if (packHeadList.Where(u => (u.TransferHeadId ?? "") != "").Count() > 0)
            {
                return "包装正在交接，请先删除交接！";
            }

            try
            {
                string getpackNumber = packHeadList.First().PackNumber;
                string packNumber = getpackNumber.Substring(0, getpackNumber.IndexOf('_'));

                string outPoNumber = getpackNumber.Substring(getpackNumber.IndexOf('_'), getpackNumber.Length - getpackNumber.IndexOf('_'));

                PackHead entity = new PackHead();
                entity.PackNumber = setpackNumber + outPoNumber;


                idal.IPackHeadDAL.UpdateBy(entity, u => u.Id == packHeadId, new string[] { "PackNumber" });
            }
            catch
            {
                return "获取包装框号信息异常！";
            }

            idal.IPackHeadDAL.SaveChanges();
            return "Y";

        }


        //博士随箱单报表字段查询
        public List<BoschPackTaskCryReport> GetBoschCryReportPackTask(int packHeadId)
        {
            List<BoschPackTaskCryReport> getFirstPackList = (from a in idal.IPackTaskDAL.SelectAll()
                                                             join b in idal.IPackHeadDAL.SelectAll() on new { Id = a.Id } equals new { Id = (Int32)b.PackTaskId } into b_join
                                                             from b in b_join.DefaultIfEmpty()
                                                             where b.Id == packHeadId
                                                             select new BoschPackTaskCryReport
                                                             {
                                                                 WhCode = a.WhCode,
                                                                 PackNumber = b.PackNumber
                                                             }).ToList();
            //取得第一个包装框号
            BoschPackTaskCryReport getFirstPack = getFirstPackList.First();

            string getpackNumber = getFirstPack.PackNumber;
            string packNumber = getpackNumber.Substring(0, getpackNumber.IndexOf('_'));


            //通过包装框号简写 查询其剩余同名的包装框号
            List<BoschPackTaskCryReport> getJson = (from a in idal.IPackTaskDAL.SelectAll()
                                                    join b in idal.IPackHeadDAL.SelectAll() on new { Id = a.Id } equals new { Id = (Int32)b.PackTaskId } into b_join
                                                    from b in b_join.DefaultIfEmpty()
                                                    where a.WhCode == getFirstPack.WhCode && b.PackNumber.StartsWith(packNumber) && b.PackNumber.Contains("_")
                                                    select new BoschPackTaskCryReport
                                                    {
                                                        WhCode = a.WhCode,
                                                        LoadId = a.LoadId,
                                                        OutPoNumber = a.OutPoNumber,
                                                        CustomerPoNumber = a.CustomerOutPoNumber
                                                    }).ToList();

            List<BoschPackTaskCryReport> list = new List<BoschPackTaskCryReport>();
            if (getJson.Count > 0)
            {
                var sql2 = (from a in idal.IPackTaskDAL.SelectAll()
                            join b in idal.IPackHeadDAL.SelectAll() on new { Id = a.Id } equals new { Id = (Int32)b.PackTaskId } into b_join
                            from b in b_join.DefaultIfEmpty()
                            join c in idal.IPackDetailDAL.SelectAll() on new { Id = b.Id } equals new { Id = (Int32)c.PackHeadId } into c_join
                            from c in c_join.DefaultIfEmpty()
                            join d in idal.IItemMasterDAL.SelectAll() on new { ItemId = (Int32)c.ItemId } equals new { ItemId = d.Id } into d_join
                            from d in d_join.DefaultIfEmpty()
                            join e in idal.IOutBoundOrderDAL.SelectAll()
                                  on new { a.WhCode, a.OutPoNumber }
                              equals new { e.WhCode, e.OutPoNumber } into e_join
                            from e in e_join.DefaultIfEmpty()
                            join h in idal.ILossDAL.SelectAll()
                                 on new { a = b.PackCarton, b = b.WhCode } equals new { a = h.LossCode, b = h.WhCode } into h_join
                            from h in h_join.DefaultIfEmpty()
                            where b.PackNumber.StartsWith(packNumber) && b.PackNumber.Contains("_") && a.WhCode == getFirstPack.WhCode
                            group new { a, d, c } by new
                            {
                                a.LoadId,
                                a.OutPoNumber,
                                a.CustomerOutPoNumber,

                                //a.AltCustomerOutPoNumber,
                                //a.d_company,
                                //a.d_contact,
                                //a.d_tel,
                                //a.d_address,

                                d.Id,
                                d.Description,
                                d.AltItemNumber,
                                d.UnitName,
                                b.PackNumber,
                                h.Length,
                                h.Width,
                                h.Height,
                                b.Weight,
                                b.PackCarton,
                                len = b.Length,
                                wid = b.Width,
                                hei = b.Height
                            } into g
                            select new PackTaskCryReport
                            {
                                LoadId = g.Key.LoadId,
                                OutPoNumber = g.Key.OutPoNumber,
                                CustomerPoNumber = g.Key.CustomerOutPoNumber,

                                //PingTaiNumber = g.Key.AltCustomerOutPoNumber,
                                //d_company = g.Key.d_company,
                                //d_contact = g.Key.d_contact,
                                //d_tel = g.Key.d_tel,
                                //d_address = g.Key.d_address,

                                ItemId = (Int32?)g.Key.Id,
                                AltItemNumber = g.Key.AltItemNumber,
                                Qty = (Int32?)g.Sum(p => p.c.Qty),
                                UnitName = g.Key.UnitName,
                                PackNumber = g.Key.PackNumber,
                                Length = (g.Key.len ?? 0) == 0 ? g.Key.Length : g.Key.len,
                                Width = (g.Key.wid ?? 0) == 0 ? g.Key.Width : g.Key.wid,
                                Height = (g.Key.hei ?? 0) == 0 ? g.Key.Height : g.Key.hei,
                                Weight = g.Key.Weight,
                                PackCarton = g.Key.PackCarton,
                                CreateDate = g.Max(p => p.c.CreateDate)
                            }).Distinct();

                List<PackTaskCryReport> sql = sql2.ToList();

                //单独得到合同号即DN号累加值
                List<BoschPackEntity> checkList = new List<BoschPackEntity>();
                string HeTongNo = "";
                string DNNo = "";

                //得到订单Json列表
                //1.得到订单数组
                string[] CustomerOutPoNumber = (from a in getJson
                                                select a.CustomerPoNumber).ToList().Distinct().ToArray();
                //2.一次性拉取订单Json列表
                List<PackTaskJson> packTaskJsonList = (from a in idal.IPackTaskJsonDAL.SelectAll()
                                                       where a.WhCode == getFirstPack.WhCode && CustomerOutPoNumber.Contains(a.CustomerOutPoNumber)
                                                       select a).ToList();

                //通过Json取得所有订单的合同号、DN号
                foreach (var item in getJson)
                {
                    if (packTaskJsonList.Where(u => u.CustomerOutPoNumber == item.CustomerPoNumber).Count() > 0)
                    {
                        PackTaskJson packJson = packTaskJsonList.Where(u => u.CustomerOutPoNumber == item.CustomerPoNumber).First();
                        PackTaskJsonEntity packTaskJsonEntity = JsonConvert.DeserializeObject<PackTaskJsonEntity>(packJson.Json);

                        BoschPackEntity boschPack = packTaskJsonEntity.boschPackEntity;

                        if (checkList.Where(u => u.HeTongNo == boschPack.HeTongNo).Count() == 0)
                        {
                            HeTongNo += boschPack.HeTongNo + ",";
                        }

                        if (checkList.Where(u => u.DNNo == boschPack.DNNo).Count() == 0)
                        {
                            DNNo += boschPack.DNNo + ",";
                        }

                        checkList.Add(boschPack);
                    }
                }

                //最终显示列表
                int LineNumber = 1;
                int i = 0;
                foreach (var item in sql)
                {
                    PackTaskJsonEntity packTaskJsonEntity = new PackTaskJsonEntity();

                    if (packTaskJsonList.Where(u => u.CustomerOutPoNumber == item.CustomerPoNumber).Count() > 0)
                    {
                        PackTaskJson packJson = packTaskJsonList.Where(u => u.CustomerOutPoNumber == item.CustomerPoNumber).First();
                        packTaskJsonEntity = JsonConvert.DeserializeObject<PackTaskJsonEntity>(packJson.Json);
                    }

                    BoschPackTaskCryReport entity = new BoschPackTaskCryReport();
                    entity.LoadId = item.LoadId;
                    entity.CustomerPoNumber = item.CustomerPoNumber;
                    entity.OutPoNumber = item.OutPoNumber;

                    entity.PingTaiNumber = packTaskJsonEntity.AltCustomerOutPoNumber;
                    entity.d_company = packTaskJsonEntity.d_company;
                    entity.d_contact = packTaskJsonEntity.d_contact;
                    entity.d_tel = packTaskJsonEntity.d_tel;
                    entity.d_address = packTaskJsonEntity.d_address;

                    entity.dest_code = item.dest_code;
                    entity.AltItemNumber = item.AltItemNumber;
                    entity.Qty = item.Qty;
                    entity.UnitName = "PC";
                    entity.PackNumber = packNumber;
                    entity.CreateDate = item.CreateDate;

                    if (i == 0)
                    {
                        entity.Weight = Math.Round(Convert.ToDouble(item.Weight ?? 0), 2).ToString();
                        entity.PackCarton = item.PackCarton;
                        entity.Length = Math.Round(Convert.ToDouble(item.Length ?? 0), 2).ToString();
                        entity.Width = Math.Round(Convert.ToDouble(item.Width ?? 0), 2).ToString();
                        entity.Height = Math.Round(Convert.ToDouble(item.Height ?? 0), 2).ToString();
                    }
                    else
                    {
                        entity.Weight = "";
                        entity.PackCarton = "";
                        entity.Length = "";
                        entity.Width = "";
                        entity.Height = "";
                    }

                    BoschPackEntity boschPack = packTaskJsonEntity.boschPackEntity;

                    entity.OutOrderNumber = boschPack.OutOrderNumber;
                    entity.HeTongNo = HeTongNo.Substring(0, HeTongNo.Length - 1);
                    entity.DNNo = DNNo.Substring(0, DNNo.Length - 1);

                    entity.PackingListTemplate = boschPack.PackingListTemplate;
                    entity.PrintCount = boschPack.PrintCount.ToString();

                    if (boschPack.BoschPackEntityDetail.Where(u => u.ItemNumber == item.AltItemNumber).Count() > 0)
                    {
                        BoschPackEntityDetail entityDetail = boschPack.BoschPackEntityDetail.Where(u => u.ItemNumber == item.AltItemNumber).First();

                        entity.LineNumber = LineNumber.ToString();
                        entity.ItemNumber = entityDetail.ItemNumber;
                        entity.EngLishDescription = entityDetail.Description;
                        entity.SaleNo = entityDetail.SaleNo;
                        entity.SaleNoZd = entityDetail.SaleNoZd;

                        if ((entityDetail.ClientItemNumber ?? "") + "" != "")
                        {
                            entity.ClientItemNumber = "Cust.Mat.No:" + entityDetail.ClientItemNumber ?? "";
                        }
                        else
                        {
                            entity.ClientItemNumber = "";
                        }

                        entity.ItemLength = entityDetail.Length.ToString();
                    }

                    list.Add(entity);
                    i++;
                    LineNumber++;
                }
            }

            return list;
        }


        //博士力士乐查询包装信息列表
        public List<BoschPackTaskSearchResult> GetBoschPackTaskSearchResult(PackTaskSearch searchEntity, out int total)
        {
            var sql = from b in idal.IPackHeadDAL.SelectAll()
                      join a in idal.IPackTaskDAL.SelectAll()
                            on new { Id = (Int32)b.PackTaskId }
                        equals new { a.Id } into b_join
                      from a in b_join.DefaultIfEmpty()
                      join c in (
                          (from sorttaskdetail in idal.ISortTaskDetailDAL.SelectAll()
                           where sorttaskdetail.WhCode == searchEntity.WhCode
                           group sorttaskdetail by new
                           {
                               sorttaskdetail.WhCode,
                               sorttaskdetail.LoadId,
                               sorttaskdetail.GroupId,
                               sorttaskdetail.GroupNumber
                           } into g
                           select new
                           {
                               g.Key.WhCode,
                               g.Key.LoadId,
                               GroupId = g.Key.GroupId,
                               g.Key.GroupNumber,
                               planQty = g.Sum(p => p.PlanQty),
                               packQty = g.Sum(p => p.PackQty)
                           }))
                            on new { a.WhCode, a.LoadId, a.SortGroupId, a.SortGroupNumber }
                        equals new { c.WhCode, c.LoadId, SortGroupId = c.GroupId, SortGroupNumber = c.GroupNumber } into c_join
                      from c in c_join.DefaultIfEmpty()
                      join d in idal.IPackDetailDAL.SelectAll()
                      on b.Id equals d.PackHeadId into d_join
                      from d in d_join.DefaultIfEmpty()
                      join e in idal.IItemMasterDAL.SelectAll()
                      on d.ItemId equals e.Id
                      join h in idal.ILossDAL.SelectAll()
                      on new { a = b.PackCarton, b = b.WhCode } equals new { a = h.LossCode, b = h.WhCode } into h_join
                      from h in h_join.DefaultIfEmpty()
                      join f in
                         (from packDetail in idal.IPackDetailDAL.SelectAll()
                          group packDetail by new
                          {
                              packDetail.PackHeadId
                          } into g
                          select new
                          {
                              g.Key.PackHeadId,
                              packQty = g.Sum(p => p.Qty)
                          })
                     on b.Id equals f.PackHeadId into f_join
                      from f in f_join.DefaultIfEmpty()
                      join g in idal.IOutBoundOrderDAL.SelectAll()
                      on new { A = a.WhCode, B = a.OutPoNumber } equals new { A = g.WhCode, B = g.OutPoNumber }
                      where a.WhCode == searchEntity.WhCode && g.ClientCode == "Bosch"
                      select new BoschPackTaskSearchResult
                      {
                          PackTaskId = a.Id,
                          LoadId = a.LoadId,
                          SortGroupNumber = a.SortGroupNumber,

                          //AltCustomerOutPoNumber = a.AltCustomerOutPoNumber,
                          ExpressCode = a.express_code ?? "",

                          CustomerOutPoNumber = a.CustomerOutPoNumber,
                          OutPoNumber = a.OutPoNumber,

                          PackHeadId = b.Id,
                          PackGroupId = b.PackGroupId,
                          PackNumber = b.PackNumber,
                          ExpressNumber = b.ExpressNumber,
                          ExpressStatus = b.ExpressStatus,
                          ExpressMessage = b.ExpressMessage,
                          Length = (b.Length ?? 0) == 0 ? h.Length : b.Length,
                          Width = (b.Width ?? 0) == 0 ? h.Width : b.Width,
                          Height = (b.Height ?? 0) == 0 ? h.Height : b.Height,
                          Weight = b.Weight ?? 0,
                          PackCarton = b.PackCarton,
                          Status =
                          b.Status == -10 ? "被拦截" :
                          b.Status == 0 ? "未包装" :
                          b.Status == 10 ? "正常" :
                          b.Status == 20 ? "交接中" : null,
                          planQty = c.planQty,
                          packQty = c.packQty,
                          packNowQty = f.packQty,
                          CreateDate = a.CreateDate,
                          UpdateDate = b.CreateDate,
                          AltItemNumber = e.AltItemNumber,

                          //d_addressDetail = a.d_Province + " " + a.d_address + " " + a.d_contact,
                          //SinglePlaneTemplate = a.SinglePlaneTemplate,
                          //PackingListTemplate = a.PackingListTemplate

                      };

            if (!string.IsNullOrEmpty(searchEntity.SortGroupNumber))
                sql = sql.Where(u => u.PackNumber.StartsWith(searchEntity.SortGroupNumber));
            if (!string.IsNullOrEmpty(searchEntity.LoadId))
                sql = sql.Where(u => u.LoadId == searchEntity.LoadId);
            if (!string.IsNullOrEmpty(searchEntity.ExpressNumber))
                sql = sql.Where(u => u.ExpressNumber == searchEntity.ExpressNumber);

            if (searchEntity.BeginCreateDate != null)
            {
                sql = sql.Where(u => u.CreateDate >= searchEntity.BeginCreateDate);
            }
            if (searchEntity.EndCreateDate != null)
            {
                sql = sql.Where(u => u.CreateDate <= searchEntity.EndCreateDate);
            }

            List<BoschPackTaskSearchResult> sqlList = sql.ToList();

            //得到订单Json列表
            //1.得到订单数组
            string[] CustomerOutPoNumber = (from a in sqlList
                                            select a.CustomerOutPoNumber).ToList().Distinct().ToArray();
            //2.一次性拉取订单Json列表
            List<PackTaskJson> packTaskJsonList = (from a in idal.IPackTaskJsonDAL.SelectAll()
                                                   where a.WhCode == searchEntity.WhCode && CustomerOutPoNumber.Contains(a.CustomerOutPoNumber)
                                                   select a).ToList();

            List<BoschPackTaskSearchResult> list = new List<BoschPackTaskSearchResult>();
            foreach (var item in sqlList)
            {
                PackTaskJsonEntity packTaskJsonEntity = new PackTaskJsonEntity();

                if (packTaskJsonList.Where(u => u.CustomerOutPoNumber == item.CustomerOutPoNumber).Count() > 0)
                {
                    PackTaskJson packJson = packTaskJsonList.Where(u => u.CustomerOutPoNumber == item.CustomerOutPoNumber).First();
                    packTaskJsonEntity = JsonConvert.DeserializeObject<PackTaskJsonEntity>(packJson.Json);
                }

                BoschPackEntity boschPack = packTaskJsonEntity.boschPackEntity;

                string packNumber = item.PackNumber.Substring(0, item.PackNumber.IndexOf('_'));
                if (list.Where(u => u.PackHeadId == item.PackHeadId).Count() == 0)
                {
                    BoschPackTaskSearchResult result = new BoschPackTaskSearchResult();
                    result.PackTaskId = item.PackTaskId;
                    result.LoadId = item.LoadId;
                    result.SortGroupNumber = item.SortGroupNumber;
                    result.CustomerOutPoNumber = item.CustomerOutPoNumber;

                    result.AltCustomerOutPoNumber = packTaskJsonEntity.AltCustomerOutPoNumber;

                    result.OutPoNumber = item.OutPoNumber;
                    result.ExpressCode = item.ExpressCode;
                    result.PackHeadId = item.PackHeadId;
                    result.PackGroupId = item.PackGroupId;
                    result.PackNumber = packNumber;
                    result.ExpressNumber = item.ExpressNumber;
                    result.ExpressStatus = item.ExpressStatus;
                    result.ExpressMessage = item.ExpressMessage;
                    result.Length = item.Length;
                    result.Width = item.Width;
                    result.Height = item.Height;
                    result.Weight = item.Weight;
                    result.PackCarton = item.PackCarton;
                    result.Status = item.Status;
                    result.planQty = item.planQty;
                    result.packQty = item.packQty;
                    result.packNowQty = item.packNowQty;
                    result.CreateDate = item.CreateDate;
                    result.UpdateDate = item.UpdateDate;
                    result.AltItemNumber = item.AltItemNumber;

                    result.d_addressDetail = (packTaskJsonEntity.d_Province ?? "") + " " + (packTaskJsonEntity.d_address ?? "") + " " + (packTaskJsonEntity.d_contact ?? "");
                    result.SinglePlaneTemplate = packTaskJsonEntity.SinglePlaneTemplate;
                    result.PackingListTemplate = boschPack.PackingListTemplate;

                    result.HeTongNo = boschPack.HeTongNo;
                    result.DNNo = boschPack.DNNo;

                    if (boschPack.BoschPackEntityDetail.Where(u => u.ItemNumber == item.AltItemNumber).Count() > 0)
                    {
                        BoschPackEntityDetail entityDetail = boschPack.BoschPackEntityDetail.Where(u => u.ItemNumber == item.AltItemNumber).First();

                        result.LineNumber = entityDetail.LineNumber;
                        result.ItemNumber = entityDetail.ItemNumber;
                        result.EngLishDescription = entityDetail.Description;
                        result.ClientItemNumber = entityDetail.ClientItemNumber;
                        result.SaleNo = entityDetail.SaleNo;
                        result.SaleNoZd = entityDetail.SaleNoZd;

                        result.ItemLength = entityDetail.Length.ToString();
                    }

                    list.Add(result);
                }
                else
                {
                    BoschPackTaskSearchResult getModel = list.Where(u => u.PackHeadId == item.PackHeadId).First();
                    list.Remove(getModel);

                    BoschPackTaskSearchResult result = new BoschPackTaskSearchResult();
                    result.PackTaskId = item.PackTaskId;
                    result.LoadId = item.LoadId;
                    result.SortGroupNumber = item.SortGroupNumber;
                    result.CustomerOutPoNumber = item.CustomerOutPoNumber;

                    result.AltCustomerOutPoNumber = packTaskJsonEntity.AltCustomerOutPoNumber;

                    result.OutPoNumber = item.OutPoNumber;
                    result.ExpressCode = item.ExpressCode;
                    result.PackHeadId = item.PackHeadId;
                    result.PackGroupId = item.PackGroupId;
                    result.PackNumber = packNumber;
                    result.ExpressNumber = item.ExpressNumber;
                    result.ExpressStatus = item.ExpressStatus;
                    result.ExpressMessage = item.ExpressMessage;
                    result.Length = item.Length;
                    result.Width = item.Width;
                    result.Height = item.Height;
                    result.Weight = item.Weight;
                    result.PackCarton = item.PackCarton;
                    result.Status = item.Status;
                    result.planQty = item.planQty;
                    result.packQty = item.packQty;
                    result.packNowQty = item.packNowQty;
                    result.CreateDate = item.CreateDate;
                    result.UpdateDate = item.UpdateDate;

                    result.d_addressDetail = (packTaskJsonEntity.d_Province ?? "") + " " + (packTaskJsonEntity.d_address ?? "") + " " + (packTaskJsonEntity.d_contact ?? "");
                    result.SinglePlaneTemplate = packTaskJsonEntity.SinglePlaneTemplate;
                    result.PackingListTemplate = boschPack.PackingListTemplate;

                    result.AltItemNumber = getModel.AltItemNumber + "," + item.AltItemNumber;

                    result.HeTongNo = boschPack.HeTongNo;
                    result.DNNo = boschPack.DNNo;

                    if (boschPack.BoschPackEntityDetail.Where(u => u.ItemNumber == item.AltItemNumber).Count() > 0)
                    {
                        BoschPackEntityDetail entityDetail = boschPack.BoschPackEntityDetail.Where(u => u.ItemNumber == item.AltItemNumber).First();

                        result.SaleNo = entityDetail.SaleNo;
                        result.SaleNoZd = entityDetail.SaleNoZd;

                        result.LineNumber = getModel.LineNumber + "," + entityDetail.LineNumber;
                        result.ItemNumber = getModel.ItemNumber + "," + entityDetail.ItemNumber;
                        result.EngLishDescription = getModel.EngLishDescription + "," + entityDetail.Description;
                        result.ClientItemNumber = getModel.ClientItemNumber + "," + entityDetail.ClientItemNumber;

                        result.ItemLength = getModel.ItemLength + "," + entityDetail.Length.ToString();
                    }

                    list.Add(result);
                }
            }

            if (!string.IsNullOrEmpty(searchEntity.AltItemNumber))
            {
                list = list.Where(u => u.AltItemNumber.Contains(searchEntity.AltItemNumber)).ToList();
            }

            total = list.Count;
            list = list.OrderByDescending(u => u.UpdateDate).ToList();
            list = list.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize).ToList();
            return list;
        }


        //博士获取快递单
        public string GetBoschExpressNumber(int packHeadId, string userName)
        {
            PackHead packHead = idal.IPackHeadDAL.SelectBy(u => u.Id == packHeadId).First();

            List<PackTask> packTaskList = idal.IPackTaskDAL.SelectBy(u => u.Id == packHead.PackTaskId);
            if (packTaskList.Count == 0)
            {
                return "ERR$未找到包装信息！";
            }
            PackTask pack = packTaskList.First();

            List<PackTaskJson> packTaskJsonList = (from a in idal.IPackTaskJsonDAL.SelectAll()
                                                   where a.WhCode == pack.WhCode && a.CustomerOutPoNumber == pack.CustomerOutPoNumber
                                                   select a).ToList();
            if (packTaskJsonList.Count == 0)
            {
                return "ERR$订单未更新地址信息！";
            }

            PackTaskJson packJson = packTaskJsonList.First();
            PackTaskJsonEntity packTaskJsonEntity = JsonConvert.DeserializeObject<PackTaskJsonEntity>(packJson.Json);

            if (pack.TransportType != "快递")
            {
                return "ERR$当前包装运输方式不属于快递！";
            }

            if ((packTaskJsonEntity.j_address ?? "") == "" || (packTaskJsonEntity.j_contact ?? "") == "")
            {
                return "ERR$当前包装未维护寄件人或寄件地址！";
            }
            if ((packTaskJsonEntity.custid ?? "") == "")
            {
                return "ERR$该客户没有月结帐号，无法获取快递单！";
            }

            List<PackHead> checkWeightList = (from a in idal.IPackTaskDAL.SelectAll()
                                              join b in idal.IPackHeadDAL.SelectAll()
                                              on a.Id equals b.PackTaskId
                                              where a.WhCode == pack.WhCode && a.LoadId == pack.LoadId
                                              select b).ToList();
            if (checkWeightList.Where(u => u.Status == 0).Count() > 0)
            {
                return "ERR$该Load未完成包装称重！";
            }
            decimal? sumWeight = 0;

            List<string> packNumberlist = new List<string>();
            foreach (var item in checkWeightList)
            {
                string getpackNumber = item.PackNumber;
                string packNumber = getpackNumber.Substring(0, getpackNumber.IndexOf('_'));

                if (packNumberlist.Contains(packNumber) == false)
                {
                    packNumberlist.Add(packNumber);
                    sumWeight += item.Weight ?? 0;
                }
            }

            if (sumWeight >= 50)
            {
                return "ERR$该Load总重大于等于50KG，无法获取快递单！";
            }

            packTaskJsonEntity.d_contact = packTaskJsonEntity.d_contact.Replace("'", " ");
            packTaskJsonEntity.d_contact = packTaskJsonEntity.d_contact.Replace("&", " ");
            packTaskJsonEntity.d_contact = packTaskJsonEntity.d_contact.Replace("$", " ");
            packTaskJsonEntity.d_contact = packTaskJsonEntity.d_contact.Replace("*", " ");
            packTaskJsonEntity.d_contact = packTaskJsonEntity.d_contact.Replace("<", " ");
            packTaskJsonEntity.d_contact = packTaskJsonEntity.d_contact.Replace(">", " ");
            packTaskJsonEntity.d_contact = packTaskJsonEntity.d_contact.Replace(@"\r\n", " ").Replace(@"\r", " ").Replace(@"\n", " ");

            packTaskJsonEntity.d_address = packTaskJsonEntity.d_address.Replace('&', ' ');
            packTaskJsonEntity.d_address = packTaskJsonEntity.d_address.Replace('$', ' ');
            packTaskJsonEntity.d_address = packTaskJsonEntity.d_address.Replace('"', ' ');
            packTaskJsonEntity.d_address = packTaskJsonEntity.d_address.Replace(',', ' ');
            packTaskJsonEntity.d_address = packTaskJsonEntity.d_address.Replace('，', ' ');
            packTaskJsonEntity.d_address = packTaskJsonEntity.d_address.Replace('<', ' ');
            packTaskJsonEntity.d_address = packTaskJsonEntity.d_address.Replace('>', ' ');
            packTaskJsonEntity.d_address = packTaskJsonEntity.d_address.Replace('（', ' ');
            packTaskJsonEntity.d_address = packTaskJsonEntity.d_address.Replace('）', ' ');
            packTaskJsonEntity.d_address = packTaskJsonEntity.d_address.Replace("'", " ");
            packTaskJsonEntity.d_address = packTaskJsonEntity.d_address.Replace("*", " ");
            packTaskJsonEntity.d_address = packTaskJsonEntity.d_address.Replace("?", " ");
            packTaskJsonEntity.d_address = packTaskJsonEntity.d_address.Replace("？", " ");

            packTaskJsonEntity.d_address = packTaskJsonEntity.d_address.Replace(@"\r\n", " ").Replace(@"\r", " ").Replace(@"\n", " ");

            if ((packHead.ExpressNumber == null ? "" : packHead.ExpressNumber) != "")
            {
                return "OK$快递单已经获取！";
            }

            ExpressManager express = new ExpressManager();

            ExpressResult result = new ExpressResult();
            //顺丰快递
            if (pack.express_code == "SF")
            {
                SFExpressModel sfModel = new SFExpressModel();
                sfModel.OrderId = pack.OutPoNumber + "-" + packHead.PackGroupId;
                sfModel.CompanyCode = packTaskJsonEntity.companyCode;
                sfModel.j_company = packTaskJsonEntity.j_company;
                sfModel.j_contact = packTaskJsonEntity.j_contact;
                sfModel.j_tel = packTaskJsonEntity.j_tel;
                sfModel.j_province = packTaskJsonEntity.j_province;
                sfModel.j_city = packTaskJsonEntity.j_city;
                sfModel.j_county = packTaskJsonEntity.j_county;
                sfModel.j_address = packTaskJsonEntity.j_address;
                sfModel.d_company = packTaskJsonEntity.d_company;
                sfModel.d_contact = packTaskJsonEntity.d_contact;
                sfModel.d_tel = packTaskJsonEntity.d_tel;
                sfModel.d_Province = packTaskJsonEntity.d_Province;
                sfModel.d_city = packTaskJsonEntity.d_city;
                sfModel.d_address = packTaskJsonEntity.d_address;
                sfModel.express_type = pack.express_type;
                sfModel.parcel_quantity = 1;
                sfModel.custid = packTaskJsonEntity.custid;
                sfModel.cod = Convert.ToDecimal(packTaskJsonEntity.cod);
                sfModel.j_name = packTaskJsonEntity.j_name;

                result = express.GetExpress(sfModel);
                UpdateBoschExpressNumber(packHead, userName, pack, result);

            }
            else if (pack.express_code == "YTO")            //圆通快递
            {
                YTExpressModel ytModel = new YTExpressModel();
                ytModel.OrderId = pack.OutPoNumber + "-" + packHead.PackGroupId;
                ytModel.CompanyCode = packTaskJsonEntity.companyCode;
                ytModel.custid = packTaskJsonEntity.custid;
                ytModel.j_province = packTaskJsonEntity.j_province;
                ytModel.j_city = packTaskJsonEntity.j_city;
                ytModel.j_county = packTaskJsonEntity.j_county;
                ytModel.j_address = packTaskJsonEntity.j_address;
                ytModel.j_contact = packTaskJsonEntity.j_contact;
                ytModel.j_tel = packTaskJsonEntity.j_tel;
                ytModel.d_company = packTaskJsonEntity.d_company;
                ytModel.d_contact = packTaskJsonEntity.d_contact;
                ytModel.d_tel = packTaskJsonEntity.d_tel;
                ytModel.d_Province = packTaskJsonEntity.d_Province;
                ytModel.d_city = packTaskJsonEntity.d_city;
                ytModel.d_address = packTaskJsonEntity.d_address;
                ytModel.parcel_quantity = 1;
                ytModel.item_name = packTaskJsonEntity.j_name;
                ytModel.Checkword = packTaskJsonEntity.Checkword;

                result = express.GetExpress(ytModel);

                UpdateBoschExpressNumber(packHead, userName, pack, result);
            }
            else
            {
                return "ERR$当前快递公司未对接快递单信息！";
            }

            idal.SaveChanges();
            return result.Status + "$" + result.Message;

        }


        //博士更新快递单号信息
        private void UpdateBoschExpressNumber(PackHead packHead, string userName, PackTask pack, ExpressResult result)
        {

            string getpackNumber = packHead.PackNumber;
            string packNumber = getpackNumber.Substring(0, getpackNumber.IndexOf('_'));

            if (result.Status == "OK")
            {
                //更新包装头 的快递单号和获取状态
                PackHead entity = new PackHead();
                entity.ExpressNumber = result.MailNo;
                entity.ExpressMessage = result.Message;
                entity.UpdateUser = userName;
                entity.UpdateDate = DateTime.Now;

                //如果电子面单Code不为空 反更新包装任务表
                if (!string.IsNullOrEmpty(result.DestCode) && result.DestCode != null)
                {
                    PackTask editPackTask = new PackTask();
                    editPackTask.dest_code = result.DestCode;
                    idal.IPackTaskDAL.UpdateBy(editPackTask, u => u.WhCode == pack.WhCode && u.LoadId == pack.LoadId && (u.dest_code ?? "") == "", new string[] { "dest_code" });

                    entity.ExpressStatus = "Y";
                }
                else
                {
                    entity.ExpressStatus = "N";
                }

                idal.IPackHeadDAL.UpdateBy(entity, u => u.WhCode == packHead.WhCode && u.PackNumber.StartsWith(packNumber) && u.PackNumber.Contains("_"), new string[] { "ExpressNumber", "ExpressMessage", "ExpressStatus", "UpdateUser", "UpdateDate" });
            }
            else if (result.Status == "ERR")
            {
                //失败 更新获取状态
                PackHead entity = new PackHead();
                entity.ExpressNumber = result.MailNo ?? "";
                entity.ExpressStatus = "N";
                entity.ExpressMessage = result.Message;
                if (entity.ExpressMessage.Length > 200)
                {
                    entity.ExpressMessage = entity.ExpressMessage.Substring(0, 150);
                }

                entity.UpdateUser = userName;
                entity.UpdateDate = DateTime.Now;

                idal.IPackHeadDAL.UpdateBy(entity, u => u.WhCode == packHead.WhCode && u.PackNumber.StartsWith(packNumber) && u.PackNumber.Contains("_"), new string[] { "ExpressStatus", "ExpressMessage", "UpdateUser", "UpdateDate" });
            }
        }


        //博士电子面单报表查询
        public List<PackTaskCryReportExpress> GetBoschCryReportExpressPackTask(int packHeadId)
        {
            PackHead packHead = idal.IPackHeadDAL.SelectBy(u => u.Id == packHeadId).First();
            string getpackNumber = packHead.PackNumber;
            string packNumber = getpackNumber.Substring(0, getpackNumber.IndexOf('_'));

            //通过包装框号简写 查询其剩余同名的包装框号
            List<BoschPackTaskCryReport> getJson = (from a in idal.IPackTaskDAL.SelectAll()
                                                    join b in idal.IPackHeadDAL.SelectAll() on new { Id = a.Id } equals new { Id = (Int32)b.PackTaskId } into b_join
                                                    from b in b_join.DefaultIfEmpty()
                                                    where a.WhCode == packHead.WhCode && b.PackNumber.StartsWith(packNumber) && b.PackNumber.Contains("_")
                                                    select new BoschPackTaskCryReport
                                                    {
                                                        WhCode = a.WhCode,
                                                        LoadId = a.LoadId,
                                                        OutPoNumber = a.OutPoNumber,
                                                        CustomerPoNumber = a.CustomerOutPoNumber
                                                    }).ToList();

            //得到订单Json列表
            //1.得到订单数组
            string[] CustomerOutPoNumber = (from a in getJson
                                            select a.CustomerPoNumber).ToList().Distinct().ToArray();
            //2.一次性拉取订单Json列表
            List<PackTaskJson> packTaskJsonList = (from a in idal.IPackTaskJsonDAL.SelectAll()
                                                   where a.WhCode == packHead.WhCode && CustomerOutPoNumber.Contains(a.CustomerOutPoNumber)
                                                   select a).ToList();

            List<BoschPackEntity> checkList = new List<BoschPackEntity>();

            string DNNo = "";
            foreach (var item in getJson)
            {
                if (packTaskJsonList.Where(u => u.CustomerOutPoNumber == item.CustomerPoNumber).Count() > 0)
                {
                    PackTaskJson packJson = packTaskJsonList.Where(u => u.CustomerOutPoNumber == item.CustomerPoNumber).First();
                    PackTaskJsonEntity packTaskJsonEntity = JsonConvert.DeserializeObject<PackTaskJsonEntity>(packJson.Json);

                    BoschPackEntity boschPack = packTaskJsonEntity.boschPackEntity;

                    if (checkList.Where(u => u.DNNo == boschPack.DNNo).Count() == 0)
                    {
                        DNNo += boschPack.DNNo + ",";
                    }

                    checkList.Add(boschPack);
                }
            }

            if (DNNo != "")
            {
                DNNo = DNNo.Substring(0, DNNo.Length - 1);
            }

            List<PackTaskCryReportExpress> sql1 = (from a in idal.IPackTaskDAL.SelectAll()
                                                   join b in idal.IPackHeadDAL.SelectAll() on new { Id = a.Id } equals new { Id = (Int32)b.PackTaskId } into b_join
                                                   from b in b_join.DefaultIfEmpty()
                                                   where b.Id == packHeadId
                                                   select new PackTaskCryReportExpress
                                                   {
                                                       LoadId = a.LoadId,
                                                       CustomerOutPoNumber = a.CustomerOutPoNumber,

                                                       //j_company = a.j_company,
                                                       //j_contact = a.j_contact,
                                                       //j_tel = a.j_tel,
                                                       //j_province = a.j_province,
                                                       //j_city = a.j_city,
                                                       //j_county = a.j_county,
                                                       //j_address = a.j_address,
                                                       //d_company = a.d_company,
                                                       //d_contact = a.d_contact,
                                                       //d_tel = a.d_tel,
                                                       //d_Province = a.d_Province,
                                                       //d_city = a.d_city,
                                                       //d_address = a.d_address,
                                                       //custid = a.custid,
                                                       //cod = a.cod.ToString(),
                                                       //j_name = a.j_name,
                                                       //form_code = a.form_code,
                                                       //AirFlag = a.AirFlag == "0" ? "不可航空" :
                                                       //a.AirFlag == "1" ? "可航空" : null,
                                                       //DN = a.AltCustomerOutPoNumber,

                                                       express_type_zh = a.express_type_zh,
                                                       dest_code = a.dest_code ?? "",

                                                       PackGroupId = b.PackGroupId,
                                                       ExpressNumber = b.ExpressNumber,
                                                       ExpressNumberBar = null,
                                                       Remark = a.Remark,
                                                       Weight = b.Weight ?? 0,
                                                       DNBar = null
                                                   }).ToList();

            List<PackTaskCryReportExpress> sql = new List<PackTaskCryReportExpress>();
            foreach (var item in sql1)
            {
                if (packTaskJsonList.Where(u => u.CustomerOutPoNumber == item.CustomerOutPoNumber).Count() > 0)
                {
                    PackTaskJson packJson = packTaskJsonList.Where(u => u.CustomerOutPoNumber == item.CustomerOutPoNumber).First();
                    PackTaskJsonEntity packTaskJsonEntity = JsonConvert.DeserializeObject<PackTaskJsonEntity>(packJson.Json);

                    item.j_company = packTaskJsonEntity.j_company;
                    item.j_contact = packTaskJsonEntity.j_contact;
                    item.j_tel = packTaskJsonEntity.j_tel;
                    item.j_province = packTaskJsonEntity.j_province;
                    item.j_city = packTaskJsonEntity.j_city;
                    item.j_county = packTaskJsonEntity.j_county;
                    item.j_address = packTaskJsonEntity.j_address;
                    item.d_company = packTaskJsonEntity.d_company;
                    item.d_contact = packTaskJsonEntity.d_contact;
                    item.d_tel = packTaskJsonEntity.d_tel;
                    item.d_Province = packTaskJsonEntity.d_Province;
                    item.d_city = packTaskJsonEntity.d_city;
                    item.d_address = packTaskJsonEntity.d_address;
                    item.custid = packTaskJsonEntity.custid;
                    item.cod = packTaskJsonEntity.cod;
                    item.j_name = packTaskJsonEntity.j_name;
                    item.form_code = packTaskJsonEntity.form_code;
                    item.AirFlag = packTaskJsonEntity.AirFlag == "0" ? "不可航空" :
                                    packTaskJsonEntity.AirFlag == "1" ? "可航空" : "不可航空";
                    item.DN = packTaskJsonEntity.AltCustomerOutPoNumber;
                }

                item.DN = DNNo;
                sql.Add(item);
            }

            return sql;
        }




        #endregion



        #region 特殊操作台

        //备货Load查询
        public List<LoadMasterResult> WorkPickLoadList(LoadMasterSearch searchEntity, out int total)
        {
            var sql = from a in idal.ILoadMasterDAL.SelectAll()
                      join b in idal.ILoadDetailDAL.SelectAll() on new { Id = a.Id } equals new { Id = (Int32)b.LoadMasterId } into b_join
                      from b in b_join.DefaultIfEmpty()
                      join c in idal.IOutBoundOrderDAL.SelectAll() on new { OutBoundOrderId = (Int32)b.OutBoundOrderId } equals new { OutBoundOrderId = c.Id } into c_join
                      from c in c_join.DefaultIfEmpty()
                      join d in idal.ILoadContainerExtendDAL.SelectAll()
                            on new { a.LoadId, a.WhCode }
                        equals new { d.LoadId, d.WhCode } into d_join
                      from d in d_join.DefaultIfEmpty()
                      join e in idal.ILoadContainerTypeDAL.SelectAll()
                            on d.ContainerType equals e.ContainerType into temp4
                      from e in temp4.DefaultIfEmpty()
                      where a.WhCode == searchEntity.WhCode
                      group new { a, c, d } by new
                      {
                          a.Id,
                          a.LoadId,
                          a.Status0,
                          a.Status1,
                          a.Status2,
                          a.ProcessName,
                          a.Remark,
                          c.ClientId,
                          c.ClientCode,
                          c.CustomerOutPoNumber,
                          a.ShipDate,
                          a.CreateUser,
                          a.CreateDate,
                          a.SumQty,
                          a.DSSumQty,
                          a.SumCBM,
                          a.SumWeight,
                          a.EchQty
                      } into g
                      select new LoadMasterResult
                      {
                          Id = g.Key.Id,
                          ClientCode = g.Key.ClientCode,
                          LoadId = g.Key.LoadId,
                          CustomerOutPoNumber = g.Key.CustomerOutPoNumber,
                          Status0 =
                           g.Key.Status0 == "U" ? "未释放" :
                           g.Key.Status0 == "C" ? "已释放" : null,
                          Status1 =
                           g.Key.Status1 == "U" ? "未备货" :
                           g.Key.Status1 == "A" ? "正在备货" :
                           g.Key.Status1 == "C" ? "完成备货" : null,
                          Status2 =
                           g.Key.Status2 == "U" ? "未分拣" :
                           g.Key.Status2 == "A" ? "正在分拣" :
                           g.Key.Status2 == "C" ? "完成分拣" : null,

                          ShipStatus = g.Key.ShipDate == null ? "未封箱" : "已封箱",
                          ProcessName = g.Key.ProcessName,

                          CreateUser = g.Key.CreateUser,
                          CreateDate = g.Key.CreateDate,
                          Remark = g.Key.Remark ?? "",
                          SumQty = g.Key.SumQty,
                          DSSumQty = g.Key.DSSumQty,
                          EchQty = g.Key.EchQty,
                          SumCBM = g.Key.SumCBM,
                          SumWeight = g.Key.SumWeight
                      };

            if (!string.IsNullOrEmpty(searchEntity.LoadId))
                sql = sql.Where(u => u.LoadId == searchEntity.LoadId);
            if (!string.IsNullOrEmpty(searchEntity.ShipMode))
                sql = sql.Where(u => u.ShipMode == searchEntity.ShipMode);
            if (!string.IsNullOrEmpty(searchEntity.Status0))
                sql = sql.Where(u => u.Status0 == searchEntity.Status0);
            if (!string.IsNullOrEmpty(searchEntity.Status1))
                sql = sql.Where(u => u.Status1 == searchEntity.Status1);
            if (!string.IsNullOrEmpty(searchEntity.Status3))
                sql = sql.Where(u => u.Status3 == searchEntity.Status3);
            if (!string.IsNullOrEmpty(searchEntity.ShipStatus))
                sql = sql.Where(u => u.ShipStatus == searchEntity.ShipStatus);
            if (searchEntity.BeginShipDate != null)
            {
                sql = sql.Where(u => u.ShipDate >= searchEntity.BeginShipDate);
            }
            if (searchEntity.EndShipDate != null)
            {
                sql = sql.Where(u => u.ShipDate <= searchEntity.EndShipDate);
            }
            if (searchEntity.BeginCreateDate != null)
            {
                sql = sql.Where(u => u.CreateDate >= searchEntity.BeginCreateDate);
            }
            if (searchEntity.EndCreateDate != null)
            {
                sql = sql.Where(u => u.CreateDate <= searchEntity.EndCreateDate);
            }

            if (!string.IsNullOrEmpty(searchEntity.ContainerNumber))
                sql = sql.Where(u => u.ContainerNumber == searchEntity.ContainerNumber);


            List<LoadMasterResult> list = new List<LoadMasterResult>();
            foreach (var item in sql)
            {
                if (list.Where(u => u.Id == item.Id).Count() == 0)
                {
                    LoadMasterResult loadMaster = new LoadMasterResult();
                    loadMaster.Action1 = "";
                    loadMaster.Action2 = "";
                    loadMaster.Id = item.Id;
                    loadMaster.ClientCode = item.ClientCode ?? "";
                    loadMaster.LoadId = item.LoadId;
                    loadMaster.CustomerOutPoNumber = item.CustomerOutPoNumber ?? "";

                    loadMaster.Status0 = item.Status0;
                    loadMaster.Status1 = item.Status1;
                    loadMaster.Status2 = item.Status2;

                    loadMaster.ShipStatus = item.ShipStatus;
                    loadMaster.ProcessId = item.ProcessId;
                    loadMaster.ProcessName = item.ProcessName;


                    loadMaster.CreateUser = item.CreateUser;
                    loadMaster.CreateDate = item.CreateDate;
                    loadMaster.Remark = item.Remark;
                    loadMaster.SumQty = item.SumQty;
                    loadMaster.DSSumQty = item.DSSumQty;
                    loadMaster.EchQty = item.EchQty;
                    loadMaster.SumCBM = item.SumCBM;
                    loadMaster.SumWeight = item.SumWeight;
                    list.Add(loadMaster);
                }
                else
                {
                    LoadMasterResult getModel = list.Where(u => u.Id == item.Id).First();
                    list.Remove(getModel);

                    LoadMasterResult loadMaster = new LoadMasterResult();
                    loadMaster.Action1 = "";
                    loadMaster.Action2 = "";
                    loadMaster.Id = item.Id;
                    loadMaster.ClientCode = getModel.ClientCode ?? "" + "," + item.ClientCode ?? "";
                    loadMaster.LoadId = item.LoadId;

                    if (getModel.CustomerOutPoNumber.Length > 50)
                    {
                        if (getModel.CustomerOutPoNumber.IndexOf("...") > 0)
                        {
                            loadMaster.CustomerOutPoNumber = getModel.CustomerOutPoNumber;
                        }
                        else
                        {
                            loadMaster.CustomerOutPoNumber = getModel.CustomerOutPoNumber + "...";
                        }
                    }
                    else
                    {
                        loadMaster.CustomerOutPoNumber = getModel.CustomerOutPoNumber + "," + item.CustomerOutPoNumber;
                    }

                    loadMaster.Status0 = item.Status0;
                    loadMaster.Status1 = item.Status1;
                    loadMaster.Status2 = item.Status2;

                    loadMaster.ShipStatus = item.ShipStatus;
                    loadMaster.ProcessId = item.ProcessId;
                    loadMaster.ProcessName = item.ProcessName;

                    loadMaster.CreateUser = item.CreateUser;
                    loadMaster.CreateDate = item.CreateDate;
                    loadMaster.Remark = item.Remark;
                    loadMaster.SumQty = item.SumQty;
                    loadMaster.DSSumQty = item.DSSumQty;
                    loadMaster.EchQty = item.EchQty;
                    loadMaster.SumCBM = item.SumCBM;
                    loadMaster.SumWeight = item.SumWeight;
                    list.Add(loadMaster);
                }
            }
            if (!string.IsNullOrEmpty(searchEntity.ClientCode))
            {
                list = list.Where(u => u.ClientCode.Contains(searchEntity.ClientCode)).ToList();
            }
            if (!string.IsNullOrEmpty(searchEntity.CustomerOutPoNumber))
            {
                list = list.Where(u => u.CustomerOutPoNumber.Contains(searchEntity.CustomerOutPoNumber)).ToList();
            }

            total = list.Count;
            list = list.OrderByDescending(u => u.Id).ToList();
            return list;
        }


        //拉取 计划备货总数 
        public List<PickTaskDetailSumQtyResult> GetSumQtyPickTaskDetailList(string loadId, string whCode)
        {
            var sql = from sorttaskdetail in idal.IPickTaskDetailDAL.SelectAll()
                      where
                        sorttaskdetail.LoadId == loadId &&
                        sorttaskdetail.WhCode == whCode
                      group sorttaskdetail by new
                      {
                          sorttaskdetail.LoadId
                      } into g
                      select new PickTaskDetailSumQtyResult
                      {
                          PlanQty = g.Sum(p => p.Qty)
                      };
            return sql.ToList();
        }

        //拉取 已备货总数 
        public List<PickTaskDetailSumQtyResult> GetSumPickQtyPickTaskDetailList(string loadId, string whCode)
        {
            var sql = from sorttaskdetail in idal.IPickTaskDetailDAL.SelectAll()
                      where
                        sorttaskdetail.LoadId == loadId &&
                        sorttaskdetail.WhCode == whCode && sorttaskdetail.Status == "C"
                      group sorttaskdetail by new
                      {
                          sorttaskdetail.LoadId
                      } into g
                      select new PickTaskDetailSumQtyResult
                      {
                          PickQty = g.Sum(p => p.Qty)
                      };
            return sql.ToList();
        }

        //拉取备货任务明细
        public List<PickTaskDetail> GetPickTaskDetailList(string loadId, string whCode)
        {
            List<PickTaskDetail> sql = (from sorttaskdetail in idal.IPickTaskDetailDAL.SelectAll()
                                        where
                                          sorttaskdetail.LoadId == loadId &&
                                          sorttaskdetail.WhCode == whCode
                                        select sorttaskdetail).ToList();

            foreach (var item in sql)
            {
                if (item.Status == "U")
                {
                    item.Status = "未备货";
                }
                else if (item.Status == "C" && item.Status1 == "C")
                {
                    item.Status = "已备货";
                }
                else if (item.Status == "C" && item.Status1 == "U")
                {
                    item.Status = "备货异常";
                }
            }
            return sql;

        }


        #endregion



        #region 款号组合特殊包装台

        public List<PackTask> GetPackTaskListByCombinationAltItemNumber(string loadId, string whCode)
        {
            List<PackTask> PackTaskList = idal.IPackTaskDAL.SelectBy(u => u.LoadId == loadId && u.WhCode == whCode && u.Status == 0).ToList();

            if (PackTaskList.Count > 0)
            {
                string[] CustomerOutPoNumber = (from a in PackTaskList
                                                select a.CustomerOutPoNumber).ToList().Distinct().ToArray();
                //2.一次性拉取订单Json列表
                List<PackTaskJson> packTaskJsonList = (from a in idal.IPackTaskJsonDAL.SelectAll()
                                                       where a.WhCode == whCode && CustomerOutPoNumber.Contains(a.CustomerOutPoNumber)
                                                       select a).ToList();
                foreach (var item in PackTaskList)
                {
                    if (packTaskJsonList.Where(u => u.CustomerOutPoNumber == item.CustomerOutPoNumber).Count() > 0)
                    {
                        PackTaskJson packJson = packTaskJsonList.Where(u => u.CustomerOutPoNumber == item.CustomerOutPoNumber).First();

                        PackTaskJsonEntity packTaskJsonEntity = JsonConvert.DeserializeObject<PackTaskJsonEntity>(packJson.Json);

                        PackTask pack = item;
                        pack.Status = 10;
                        pack.TransportType = packTaskJsonEntity.TransportType;
                        pack.express_code = packTaskJsonEntity.express_code;
                        pack.express_type = packTaskJsonEntity.express_type;
                        pack.express_type_zh = packTaskJsonEntity.express_type_zh;
                        pack.PackMoreFlag = packTaskJsonEntity.PackMoreFlag;
                        pack.SingleFlag = packTaskJsonEntity.SingleFlag;
                        if (pack.SingleFlag == 1)
                        {
                            pack.PackQty = 1;
                        }
                        pack.ZdFlag = packTaskJsonEntity.ZdFlag;
                        pack.SinglePlaneTemplate = packTaskJsonEntity.SinglePlaneTemplate;
                        pack.PackingListTemplate = packTaskJsonEntity.PackingListTemplate;

                        idal.IPackTaskDAL.UpdateBy(pack, u => u.Id == pack.Id, new string[] { "Status", "TransportType", "express_code", "express_type", "express_type_zh", "SingleFlag", "ZdFlag", "SinglePlaneTemplate", "PackingListTemplate", "PackMoreFlag", "PackQty" });
                    }
                }
                idal.SaveChanges();
            }

            List<PackTask> PackTaskList1 = idal.IPackTaskDAL.SelectBy(u => u.LoadId == loadId && u.WhCode == whCode && (u.TransportType ?? "") == "").ToList();

            if (PackTaskList1.Count > 0)
            {
                string[] CustomerOutPoNumber = (from a in PackTaskList1
                                                select a.CustomerOutPoNumber).ToList().Distinct().ToArray();
                //2.一次性拉取订单Json列表
                List<PackTaskJson> packTaskJsonList = (from a in idal.IPackTaskJsonDAL.SelectAll()
                                                       where a.WhCode == whCode && CustomerOutPoNumber.Contains(a.CustomerOutPoNumber)
                                                       select a).ToList();
                foreach (var item in PackTaskList1)
                {
                    if (packTaskJsonList.Where(u => u.CustomerOutPoNumber == item.CustomerOutPoNumber).Count() > 0)
                    {
                        PackTaskJson packJson = packTaskJsonList.Where(u => u.CustomerOutPoNumber == item.CustomerOutPoNumber).First();

                        PackTaskJsonEntity packTaskJsonEntity = JsonConvert.DeserializeObject<PackTaskJsonEntity>(packJson.Json);

                        PackTask pack = item;
                        pack.TransportType = packTaskJsonEntity.TransportType;
                        pack.express_code = packTaskJsonEntity.express_code;
                        pack.express_type = packTaskJsonEntity.express_type;
                        pack.express_type_zh = packTaskJsonEntity.express_type_zh;
                        pack.PackMoreFlag = packTaskJsonEntity.PackMoreFlag;
                        pack.SingleFlag = packTaskJsonEntity.SingleFlag;
                        if (pack.SingleFlag == 1)
                        {
                            pack.PackQty = 1;
                        }
                        pack.ZdFlag = packTaskJsonEntity.ZdFlag;
                        pack.SinglePlaneTemplate = packTaskJsonEntity.SinglePlaneTemplate;
                        pack.PackingListTemplate = packTaskJsonEntity.PackingListTemplate;

                        idal.IPackTaskDAL.UpdateBy(pack, u => u.Id == pack.Id, new string[] { "TransportType", "express_code", "express_type", "express_type_zh", "SingleFlag", "ZdFlag", "SinglePlaneTemplate", "PackingListTemplate", "PackMoreFlag", "PackQty" });
                    }
                }

                idal.SaveChanges();
            }

            return idal.IPackTaskDAL.SelectBy(u => u.LoadId == loadId && u.WhCode == whCode && (u.Status == 10 || u.Status == 20) && u.SingleFlag == 2).ToList();
        }


        //拉取 计划包装总数与已包装总数 
        public List<SortTaskDetailResult> GetSumListByCombinationAltItemNumber(string loadId, string whCode)
        {
            var sql = from a in
                        (from a in idal.ISortTaskDetailDAL.SelectAll()
                         join b in idal.IPackTaskDAL.SelectAll()
                               on new { a.WhCode, a.LoadId, a.OutPoNumber }
                           equals new { b.WhCode, b.LoadId, b.OutPoNumber }
                         where
                           a.LoadId == loadId &&
                           a.WhCode == whCode &&
                           b.SingleFlag == 2
                         select new
                         {
                             a.PlanQty,
                             a.PackQty,
                             holdQty = a.PackQty == 0 && a.HoldFlag == 1 ? a.PlanQty : 0,
                             Dummy = "x"
                         })
                      group a by new { a.Dummy } into g
                      select new SortTaskDetailResult
                      {
                          PlanQty = (Int32?)g.Sum(p => p.PlanQty),
                          PackQty = (Int32?)g.Sum(p => p.PackQty),
                          HoldQty = (System.Int32?)g.Sum(p => p.holdQty)
                      };
            return sql.ToList();
        }

        //通过 当前包装扫描的Load号 获得 其分拣明细
        public List<SortTaskDetail> GetSortTaskDetailByCombinationAltItemNumber(string loadId, string whCode)
        {
            var sql = from a in idal.IPackTaskDAL.SelectAll()
                      where a.SingleFlag == 2 && a.LoadId == loadId && a.WhCode == whCode
                      join sorttaskdetail in idal.ISortTaskDetailDAL.SelectAll()
                      on new { A = a.LoadId, B = a.WhCode, C = a.OutPoNumber } equals new { A = sorttaskdetail.LoadId, B = sorttaskdetail.WhCode, C = sorttaskdetail.OutPoNumber }
                      where sorttaskdetail.PlanQty != sorttaskdetail.PackQty
                      select sorttaskdetail;

            return sql.ToList();
        }

        //UCF随箱单报表查询
        public List<PackTaskCryReport> UCFGetCryReportPackTask(int packHeadId, string whCode, string userName, int type)
        {
            var sql = from a in idal.IPackTaskDAL.SelectAll()
                      join b in idal.IPackHeadDAL.SelectAll() on new { Id = a.Id } equals new { Id = (Int32)b.PackTaskId }
                      where
                        b.Id == packHeadId
                      group new { a } by new
                      {
                          a.WhCode,
                          a.LoadId,
                          a.OutPoNumber,
                          a.CustomerOutPoNumber
                      } into g
                      select new PackTaskCryReport
                      {
                          WhCode = g.Key.WhCode,
                          LoadId = g.Key.LoadId,
                          OutPoNumber = g.Key.OutPoNumber,
                          CustomerPoNumber = g.Key.CustomerOutPoNumber
                      };

            List<PackTaskCryReport> sqlList = sql.ToList();
            PackTaskCryReport first = sqlList.First();

            //得到订单Json列表
            //1.得到订单数组
            string[] CustomerOutPoNumber = (from a in sqlList
                                            select a.CustomerPoNumber).ToList().Distinct().ToArray();
            //2.一次性拉取订单Json列表
            List<PackTaskJson> packTaskJsonList = (from a in idal.IPackTaskJsonDAL.SelectAll()
                                                   where a.WhCode == first.WhCode && CustomerOutPoNumber.Contains(a.CustomerOutPoNumber)
                                                   select a).ToList();

            List<PackTaskCryReport> sqlResult = new List<PackTaskCryReport>();
            foreach (var item in sqlList)
            {
                PackTaskJson packJson = new PackTaskJson();
                PackTaskJsonEntity packTaskJsonEntity = new PackTaskJsonEntity();

                if (packTaskJsonList.Where(u => u.CustomerOutPoNumber == item.CustomerPoNumber).Count() > 0)
                {
                    packJson = packTaskJsonList.Where(u => u.CustomerOutPoNumber == item.CustomerPoNumber).First();

                    packTaskJsonEntity = JsonConvert.DeserializeObject<PackTaskJsonEntity>(packJson.Json);
                }
                if (packTaskJsonEntity == null)
                {
                    continue;
                }

                PackTaskCryReport packCry = new PackTaskCryReport();
                packCry.WhCode = item.WhCode;
                packCry.LoadId = item.LoadId;
                packCry.OutPoNumber = item.OutPoNumber;
                packCry.CustomerPoNumber = item.CustomerPoNumber;
                packCry.CustomerName = packTaskJsonEntity.CustomerName ?? "";
                packCry.CustomerRef = packTaskJsonEntity.CustomerRef ?? "";
                packCry.BusinessMode = packTaskJsonEntity.BusinessMode ?? "";
                packCry.d_contact = packTaskJsonEntity.d_contact ?? "";
                packCry.d_tel = packTaskJsonEntity.d_tel ?? "";
                packCry.d_address = packTaskJsonEntity.d_address ?? "";
                packCry.CreateDate = packTaskJsonEntity.OrderCreateDate ?? DateTime.Now;
                packCry.Price = item.Price ?? 0;
                packCry.PackGroupId = 0;
                packCry.TotalPrice = item.TotalPrice ?? 0;
                packCry.Length = 0;
                packCry.Height = 0;
                packCry.Width = 0;
                packCry.Weight = 0;
                packCry.ItemId = 0;
                packCry.Qty = 1;

                sqlResult.Add(packCry);
            }

            return sqlResult;
        }

        //包装任意订单中的某一个款号时，自动包装完该订单的剩余款号
        public string PackTaskInsertByCombinationAltItemNumber(PackTaskInsert entity)
        {
            if (entity.LoadId == null || entity.LoadId == "" || entity.WhCode == "" || entity.WhCode == null)
            {
                return "数据有误，请重新操作！";
            }

            if (entity.PackDetail == null)
            {
                return "明细数据有误，请重新操作！";
            }

            try
            {
                #region 开始包装

                //包装开始前 验证备货装箱是否完成
                var sql = (from a in idal.IPickTaskDetailDAL.SelectAll()
                           where a.LoadId == entity.LoadId && a.WhCode == entity.WhCode
                           && a.Status == "C" && a.Status1 == "U"
                           select a);
                if (sql.Count() > 0)
                {
                    return "该Load:" + entity.LoadId + "自动装箱有误，请查看备货任务后手动装箱！";
                }

                //组合包装验证逻辑
                //2.  Load有多条包装信息时
                //2.1 首先取得当前扫描的单品包装的款号
                PackDetailInsert getPackDetailInsert = entity.PackDetail.First();

                //2.2 通过扫描的款号 查询出 分拣表中 同一Load 同一款号 包装数量不等于计划数量 的信息list
                List<SortTaskDetail> sortTaskDetailList = idal.ISortTaskDetailDAL.SelectBy(u => u.LoadId == entity.LoadId && u.WhCode == entity.WhCode && u.ItemId == getPackDetailInsert.ItemId && u.PlanQty != u.PackQty).OrderBy(u => u.GroupId).ToList();

                if (sortTaskDetailList.Count == 0)
                {
                    return "没有可用的包装信息！";
                }

                //2.3 提取list中 没有拦截标志的信息
                if (sortTaskDetailList.Where(u => u.HoldFlag != 1).Count() == 0)
                {
                    return "包装被拦截！";
                }

                //SingleFlag == 1 单品，SingleFlag == 2 组合

                var doStatusData = staticPackTaskStatusList.Where(u => u.WhCode == entity.WhCode && u.LoadId == entity.LoadId).ToList();
                var doData = staticPackTaskList.Where(u => u.WhCode == entity.WhCode && u.LoadId == entity.LoadId).ToList();

                if (doData.Count() == 0 && doStatusData.Count == 0)
                {
                    //2.4 通过list 查询 包装任务表中的 是组合标志且状态正常的 信息
                    List<PackTask> getpacktaskSql = (from a in idal.ISortTaskDetailDAL.SelectAll()
                                                     join b in idal.IPackTaskDAL.SelectAll()
                                                           on new { a.WhCode, a.LoadId, a.GroupId, a.GroupNumber, a.OutPoNumber }
                                                       equals new { b.WhCode, b.LoadId, GroupId = b.SortGroupId, GroupNumber = b.SortGroupNumber, b.OutPoNumber }
                                                     where
                                                       a.WhCode == entity.WhCode &&
                                                       a.LoadId == entity.LoadId &&
                                                       a.ItemId == getPackDetailInsert.ItemId &&
                                                       a.PlanQty != a.PackQty && a.HoldFlag == 0 &&
                                                       b.SingleFlag == 2 && (b.Status == 10 || b.Status == 20)
                                                     orderby a.GroupId
                                                     select b).ToList();
                    //新增明细数据到包装选择器里面
                    staticPackTaskList.AddRange(getpacktaskSql);

                    //新增状态LoadId,ItemId对应的状态到 包装状态明细里
                    StaticLoadPackCount aa = new StaticLoadPackCount();
                    aa.LoadId = entity.LoadId;
                    aa.WhCode = entity.WhCode;
                    staticPackTaskStatusList.Add(aa);
                }
                else if (doData.Count() == 0 && doStatusData.Count() != 0)
                {
                    Task<string> task = Task<string>.Run(() =>
                    {
                        System.Threading.Thread.Sleep(2000);
                        staticPackTaskStatusList.RemoveAll(u => u.LoadId == entity.LoadId && u.WhCode == entity.WhCode);
                        return System.Threading.Thread.CurrentThread.ManagedThreadId.ToString();
                    });
                    //会等到任务执行完之后执行
                    task.GetAwaiter().OnCompleted(() =>
                    {
                        Console.WriteLine(task.Result);
                    });

                    return "没有可用的包装信息！2秒后系统自动刷新数据!";
                }

                PackTask packTask = new PackTask();

                lock (o3)
                {
                    packTask = staticPackTaskList.Where(u => u.WhCode == entity.WhCode && u.LoadId == entity.LoadId).First();

                    entity.OutPoNumber = packTask.OutPoNumber;
                    entity.GroupId = (Int32)packTask.SortGroupId;
                    entity.GroupNumber = packTask.SortGroupNumber;
                    entity.PackNumber = packTask.CustomerOutPoNumber;

                    staticPackTaskList.Remove(packTask);
                }

                List<SortTaskDetail> resultList = idal.ISortTaskDetailDAL.SelectBy(u => u.WhCode == entity.WhCode && u.LoadId == entity.LoadId && u.OutPoNumber == packTask.OutPoNumber && u.GroupId == packTask.SortGroupId && u.GroupNumber == packTask.SortGroupNumber).ToList();

                int HoldFlagCount = resultList.Where(u => u.HoldFlag == 1).Count();
                if (HoldFlagCount >= 1)
                    return "订单:" + entity.OutPoNumber + "已经拦截!请再次扫描获取新订单!";

                string result = "";

                //0. 更新Load的开始包装时间
                LoadMaster load = idal.ILoadMasterDAL.SelectBy(u => u.LoadId == entity.LoadId && u.WhCode == entity.WhCode).First();

                List<PackHead> getOne = (from a in idal.IPackHeadDAL.SelectAll()
                                         join b in idal.IPackTaskDAL.SelectAll()
                                         on a.PackTaskId equals b.Id
                                         where b.LoadId == entity.LoadId && b.WhCode == entity.WhCode
                                         select a).Take(1).ToList();
                if (getOne.Count == 0)
                {
                    load.BeginPackDate = DateTime.Now;
                    load.UpdateUser = entity.userName;
                    load.UpdateDate = DateTime.Now;
                    idal.ILoadMasterDAL.UpdateBy(load, u => u.Id == load.Id, new string[] { "BeginPackDate", "UpdateUser", "UpdateDate" });
                }

                //---------插入包装数据
                //1.---------先反更新分拣明细已包装数量
                foreach (var item in resultList)
                {
                    SortTaskDetail updateEntity = new SortTaskDetail();
                    updateEntity.PackQty = item.PlanQty;
                    updateEntity.UpdateUser = entity.userName;
                    updateEntity.UpdateDate = DateTime.Now;
                    idal.ISortTaskDetailDAL.UpdateBy(updateEntity, u => u.Id == item.Id, new string[] { "PackQty", "UpdateUser", "UpdateDate" });
                }

                //2.----------插入包装头表
                PackHead packHead = new PackHead();
                List<PackHead> checkPackHeadCount = idal.IPackHeadDAL.SelectBy(u => u.PackTaskId == packTask.Id && u.WhCode == packTask.WhCode && u.PackNumber == entity.PackNumber && u.Status == 10).ToList();
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
                    packHead.PackNumber = entity.PackNumber;
                    packHead.Status = 10;
                    packHead.CreateUser = entity.userName;
                    packHead.CreateDate = DateTime.Now;
                    idal.IPackHeadDAL.Add(packHead);
                }
                else
                {
                    packHead = checkPackHeadCount.First();
                    PackHead update = new PackHead();
                    update.Status = 10;
                    update.UpdateUser = entity.userName;
                    update.UpdateDate = DateTime.Now;
                    idal.IPackHeadDAL.UpdateBy(update, u => u.Id == packHead.Id, new string[] { "Status", "UpdateUser", "UpdateDate" });

                    if (!string.IsNullOrEmpty(packHead.ExpressNumber))
                    {
                        result = "请重新再扫描下款号！";
                    }
                }

                if (result + "" != "")
                {
                    return result;
                }

                //3.----------插入包装明细表
                foreach (var item in resultList)
                {
                    PackDetail packDetail = new PackDetail();
                    packDetail.WhCode = packTask.WhCode;
                    packDetail.PackHeadId = packHead.Id;
                    packDetail.ItemId = item.ItemId;
                    packDetail.Qty = (Int32)item.Qty;
                    packDetail.CreateUser = entity.userName;
                    packDetail.CreateDate = DateTime.Now;
                    idal.IPackDetailDAL.Add(packDetail);
                }
                //提交一次,不然分拣明细状态更新不出来
                idal.SaveChanges();

                //验证 分拣明细中 包装是否全部完成
                List<SortTaskDetail> checkAllQty = GetSortTaskDetail(entity.LoadId, entity.WhCode, entity.GroupId).ToList();
                if (checkAllQty.Count == 0)
                {
                    //如果全部完成 更新出库订单状态
                    List<OutBoundOrder> eneityList = idal.IOutBoundOrderDAL.SelectBy(u => u.WhCode == packTask.WhCode && u.OutPoNumber == packTask.OutPoNumber);
                    if (eneityList.Count > 0)
                    {
                        OutBoundOrder outBoundOrder = eneityList.First();
                        if (outBoundOrder.StatusId != -10)
                        {
                            FlowDetail flowDetail = idal.IFlowDetailDAL.SelectBy(u => u.FlowHeadId == outBoundOrder.ProcessId && u.Type == "PackingType").First();

                            if (flowDetail != null && flowDetail.StatusId != 0)
                            {
                                string remark1 = "原状态：" + outBoundOrder.StatusId + outBoundOrder.StatusName;

                                //更新出库订单状态为已包装
                                outBoundOrder.NowProcessId = flowDetail.FlowRuleId;
                                outBoundOrder.StatusId = flowDetail.StatusId;
                                outBoundOrder.StatusName = flowDetail.StatusName;
                                outBoundOrder.UpdateUser = entity.userName;
                                outBoundOrder.UpdateDate = DateTime.Now;
                                idal.IOutBoundOrderDAL.UpdateBy(outBoundOrder, u => u.Id == outBoundOrder.Id, new string[] { "NowProcessId", "StatusId", "StatusName", "UpdateUser", "UpdateDate" });

                                //更新订单状态，插入日志
                                TranLog tlorder = new TranLog();
                                tlorder.TranType = "32";
                                tlorder.Description = "更新订单状态";
                                tlorder.TranDate = DateTime.Now;
                                tlorder.TranUser = entity.userName;
                                tlorder.WhCode = packTask.WhCode;
                                tlorder.LoadId = entity.LoadId;
                                tlorder.CustomerOutPoNumber = outBoundOrder.CustomerOutPoNumber;
                                tlorder.OutPoNumber = outBoundOrder.OutPoNumber;
                                tlorder.Remark = remark1 + "变更为：" + outBoundOrder.StatusId + outBoundOrder.StatusName;
                                idal.ITranLogDAL.Add(tlorder);
                            }
                        }
                    }

                    //更新包装任务状态 为 已完成
                    PackTask setPackTask = new PackTask();
                    setPackTask.Status = 30;
                    setPackTask.UpdateUser = entity.userName;
                    setPackTask.UpdateDate = DateTime.Now;
                    idal.IPackTaskDAL.UpdateBy(setPackTask, u => u.Id == packTask.Id, new string[] { "Status", "UpdateUser", "UpdateDate" });
                }

                //6. 更新Load的完成包装时间
                List<PackTask> checkPackTaskList1 = idal.IPackTaskDAL.SelectBy(u => u.LoadId == entity.LoadId && u.WhCode == entity.WhCode);
                if (checkPackTaskList1.Where(u => u.Status == 0 || u.Status == 10 || u.Status == 20).Count() == 0)
                {
                    load.EndPackDate = DateTime.Now;
                    load.UpdateUser = entity.userName;
                    load.UpdateDate = DateTime.Now;

                    //删除LOAD的状态数据
                    staticLoadPackCountList.RemoveAll(u => u.LoadId == entity.LoadId && u.WhCode == entity.WhCode);

                    idal.ILoadMasterDAL.UpdateBy(load, u => u.Id == load.Id, new string[] { "EndPackDate", "UpdateUser", "UpdateDate" });
                }
                #endregion

                idal.SaveChanges();

                return "Y" + packHead.Id + "$" + packTask.Id;
            }
            catch (Exception ex)
            {
                return ex.ToString();
                // return "包装异常，请重新提交！";
            }

        }


        //查询包装信息列表
        public List<PackTaskSearchResult> GetPackTaskSearchResultByCombinationAltItemNumber(PackTaskSearch searchEntity, out int total, string[] expressNumberArr, string[] customerOutPoNumberArr)
        {
            var sql = (from b in idal.IPackHeadDAL.SelectAll()
                       join a in idal.IPackTaskDAL.SelectAll()
                             on new { Id = (Int32)b.PackTaskId }
                         equals new { a.Id } into b_join
                       from a in b_join.DefaultIfEmpty()
                       join c in (
                           (from sorttaskdetail in idal.ISortTaskDetailDAL.SelectAll()
                            where sorttaskdetail.WhCode == searchEntity.WhCode
                            group sorttaskdetail by new
                            {
                                sorttaskdetail.WhCode,
                                sorttaskdetail.LoadId,
                                sorttaskdetail.GroupId,
                                sorttaskdetail.GroupNumber
                            } into g
                            select new
                            {
                                g.Key.WhCode,
                                g.Key.LoadId,
                                GroupId = g.Key.GroupId,
                                g.Key.GroupNumber,
                                planQty = g.Sum(p => p.PlanQty),
                                packQty = g.Sum(p => p.PackQty)
                            }))
                             on new { a.WhCode, a.LoadId, a.SortGroupId, a.SortGroupNumber }
                         equals new { c.WhCode, c.LoadId, SortGroupId = c.GroupId, SortGroupNumber = c.GroupNumber } into c_join
                       from c in c_join.DefaultIfEmpty()
                       join d in idal.IPackDetailDAL.SelectAll()
                       on b.Id equals d.PackHeadId into d_join
                       from d in d_join.DefaultIfEmpty()
                       join e in idal.IItemMasterDAL.SelectAll()
                       on d.ItemId equals e.Id
                       join f in
                          (from packDetail in idal.IPackDetailDAL.SelectAll()
                           group packDetail by new
                           {
                               packDetail.PackHeadId
                           } into g
                           select new
                           {
                               g.Key.PackHeadId,
                               packQty = g.Sum(p => p.Qty)
                           })
                      on b.Id equals f.PackHeadId into f_join
                       from f in f_join.DefaultIfEmpty()
                       join g in idal.IOutBoundOrderDAL.SelectAll()
                     on new { A = a.WhCode, B = a.OutPoNumber } equals new { A = g.WhCode, B = g.OutPoNumber }
                       where a.WhCode == searchEntity.WhCode && (a.SingleFlag ?? 0) == 2
                       select new PackTaskSearchResult
                       {
                           PackTaskId = a.Id,
                           LoadId = a.LoadId,
                           ClientCode = g.ClientCode,
                           SortGroupNumber = a.SortGroupNumber,
                           CustomerOutPoNumber = a.CustomerOutPoNumber,
                           AltCustomerOutPoNumber = g.AltCustomerOutPoNumber,
                           OutPoNumber = a.OutPoNumber,
                           ExpressCode = a.express_code,
                           PackHeadId = b.Id,
                           PackGroupId = b.PackGroupId,
                           PackNumber = b.PackNumber,
                           ExpressNumber = b.ExpressNumber ?? "",
                           ExpressStatus = b.ExpressStatus,
                           ExpressMessage = b.ExpressMessage,
                           Length = b.Length,
                           Width = b.Width,
                           Height = b.Height,
                           Weight = b.Weight,
                           PackCarton = b.PackCarton,
                           Status =
                           b.Status == -10 ? "被拦截" :
                           b.Status == 0 ? "未包装" :
                           b.Status == 10 ? "正常" :
                           b.Status == 20 ? "已交接" : null,
                           planQty = c.planQty,
                           packQty = c.packQty,
                           packNowQty = f.packQty,
                           CreateDate = b.CreateDate,
                           UpdateDate = b.UpdateDate,
                           UpdateUser = b.UpdateUser,
                           SinglePlaneTemplate = a.SinglePlaneTemplate,
                           PackingListTemplate = a.PackingListTemplate,
                           OrderDate = g.CreateDate,
                           OrderSource = g.OrderSource,
                           OrderType = g.OrderType
                       }).Distinct();

            if (!string.IsNullOrEmpty(searchEntity.ClientCode))
                sql = sql.Where(u => u.ClientCode == searchEntity.ClientCode);
            if (!string.IsNullOrEmpty(searchEntity.SortGroupNumber))
                sql = sql.Where(u => u.SortGroupNumber == searchEntity.SortGroupNumber);

            if (!string.IsNullOrEmpty(searchEntity.LoadId))
                sql = sql.Where(u => u.LoadId.Contains(searchEntity.LoadId));

            if (expressNumberArr != null)
                sql = sql.Where(u => expressNumberArr.Contains(u.ExpressNumber));

            if (customerOutPoNumberArr != null)
                sql = sql.Where(u => customerOutPoNumberArr.Contains(u.CustomerOutPoNumber));

            if (!string.IsNullOrEmpty(searchEntity.UpdateUser))
                sql = sql.Where(u => u.UpdateUser == searchEntity.UpdateUser);

            if (!string.IsNullOrEmpty(searchEntity.Status))
            {
                sql = sql.Where(u => u.Status == searchEntity.Status);
            }

            if (!string.IsNullOrEmpty(searchEntity.ExpressNumberIsNull))
            {
                if (searchEntity.ExpressNumberIsNull == "有")
                {
                    sql = sql.Where(u => u.ExpressNumber != "");
                }
                else
                {
                    sql = sql.Where(u => u.ExpressNumber == "");
                }
            }

            if (searchEntity.BeginCreateDate != null)
            {
                sql = sql.Where(u => u.CreateDate >= searchEntity.BeginCreateDate);
            }
            if (searchEntity.EndCreateDate != null)
            {
                sql = sql.Where(u => u.CreateDate <= searchEntity.EndCreateDate);
            }

            if (searchEntity.BeginOrderDate != null)
            {
                sql = sql.Where(u => u.OrderDate >= searchEntity.BeginOrderDate);
            }
            if (searchEntity.EndOrderDate != null)
            {
                sql = sql.Where(u => u.OrderDate <= searchEntity.EndOrderDate);
            }

            if (!string.IsNullOrEmpty(searchEntity.AltCustomerOutPoNumber))
                sql = sql.Where(u => u.AltCustomerOutPoNumber == (searchEntity.AltCustomerOutPoNumber));

            total = sql.Count();
            sql = sql.OrderByDescending(u => u.UpdateDate);
            sql = sql.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize);

            return sql.ToList();
        }


        //查询固定组合款号包装信息列表-UNICEF
        public List<PackTaskSearchResult> GetPackTaskSearchResultByCombinationAltItemNumberUCF(PackTaskSearch searchEntity, out int total, string[] expressNumberArr)
        {
            var sql = from a in idal.IPackTaskDAL.SelectAll()
                      join b in idal.ISortTaskDetailDAL.SelectAll()
                            on new { a.WhCode, a.LoadId, a.SortGroupId, a.OutPoNumber }
                        equals new { b.WhCode, b.LoadId, SortGroupId = b.GroupId, b.OutPoNumber }
                      join c in idal.IPackHeadDAL.SelectAll() on new { Id = a.Id } equals new { Id = (Int32)c.PackTaskId } into c_join
                      from c in c_join.DefaultIfEmpty()
                      join d in idal.IItemMasterDAL.SelectAll() on new { ItemId = (Int32)b.ItemId } equals new { ItemId = d.Id }
                      join e in idal.IOutBoundOrderDAL.SelectAll()
                            on new { b.WhCode, b.OutPoNumber }
                        equals new { e.WhCode, e.OutPoNumber }
                      where a.WhCode == searchEntity.WhCode && e.ClientCode == "CNUNICEF" && (a.SingleFlag ?? 0) == 2
                      select new PackTaskSearchResult
                      {
                          PackTaskId = a.Id,
                          LoadId = a.LoadId,
                          CustomerOutPoNumber = a.CustomerOutPoNumber,
                          ExpressCode = a.express_code,
                          ClientCode = e.ClientCode,
                          PackHeadId = c.Id,
                          PackGroupId = b.GroupId,
                          ExpressNumber = c.ExpressNumber ?? "",
                          ExpressMessage = c.ExpressMessage,
                          PackCarton = c.PackCarton,
                          Status = e.StatusName,
                          CreateDate = a.CreateDate,
                          AltItemNumber = d.AltItemNumber,
                          AltCustomerOutPoNumber = e.AltCustomerOutPoNumber,
                          PrintDate = c.PrintDate == null ? "" : c.PrintDate.ToString()
                      };

            if (!string.IsNullOrEmpty(searchEntity.LoadId))
                sql = sql.Where(u => u.LoadId == searchEntity.LoadId);

            if (expressNumberArr != null)
                sql = sql.Where(u => expressNumberArr.Contains(u.ExpressNumber));

            List<PackTaskSearchResult> list = new List<PackTaskSearchResult>();
            foreach (var item in sql)
            {
                if (list.Where(u => u.PackTaskId == item.PackTaskId).Count() == 0)
                {
                    list.Add(item);
                }
                else
                {
                    PackTaskSearchResult oldentity = list.Where(u => u.PackTaskId == item.PackTaskId).First();
                    list.Remove(oldentity);

                    PackTaskSearchResult newentity = oldentity;
                    newentity.AltItemNumber = newentity.AltItemNumber + "," + item.AltItemNumber;
                    list.Add(newentity);
                }
            }

            total = list.Count;
            list = list.OrderBy(u => u.LoadId).ThenBy(u => u.PackGroupId).Skip((searchEntity.pageSize) * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize).ToList();

            return list;
        }


        //一键完成包装-UNICEF
        public string PackTaskInsertByCombinationAltItemNumberUCF(int?[] packTaskId, string userName)
        {
            List<PackTask> PackTaskList = idal.IPackTaskDAL.SelectBy(u => packTaskId.Contains(u.Id) && u.SingleFlag == 2 && (u.Status == 10 || u.Status == 20));

            List<PackTaskInsert> entityList = (from a in idal.IPackTaskDAL.SelectAll()
                                               join b in idal.ISortTaskDetailDAL.SelectAll()
                     on new { a.WhCode, a.LoadId, a.SortGroupId, a.OutPoNumber }
                 equals new { b.WhCode, b.LoadId, SortGroupId = b.GroupId, b.OutPoNumber }
                                               where packTaskId.Contains(a.Id) && a.SingleFlag == 2 && (a.Status == 10 || a.Status == 20)
                                               && b.PlanQty != b.PackQty && b.HoldFlag != 1
                                               select new PackTaskInsert
                                               {
                                                   LoadId = a.LoadId,
                                                   WhCode = a.WhCode,
                                                   OutPoNumber = a.OutPoNumber,
                                                   GroupId = (Int32)a.SortGroupId,
                                                   GroupNumber = a.SortGroupNumber,
                                                   PackNumber = a.CustomerOutPoNumber,
                                                   userName = userName
                                               }).Distinct().OrderBy(u => u.GroupId).ToList();
            if (entityList.Count == 0)
            {
                return "该Load未找到可包装信息！";
            }

            PackTaskInsert entityFirst = entityList.First();
            List<PackDetailInsert> getPackDetailInsertList = (from a in idal.ISortTaskDetailDAL.SelectAll()
                                                              join b in idal.IItemMasterDAL.SelectAll()
                                                              on a.ItemId equals b.Id
                                                              where a.LoadId == entityFirst.LoadId && a.WhCode == entityFirst.WhCode && a.GroupId == entityFirst.GroupId && a.OutPoNumber == entityFirst.OutPoNumber
                                                              select new PackDetailInsert
                                                              {
                                                                  ItemId = a.ItemId,
                                                                  AltItemNumber = a.AltItemNumber,
                                                                  EAN = a.EAN,
                                                                  Qty = a.PlanQty,
                                                                  CartonName = b.CartonName,
                                                                  Weight = b.Weight
                                                              }).ToList();

            //UNICEF默认重量0.3
            decimal sumWeight = getPackDetailInsertList.Sum(u => u.Weight);
            if (sumWeight < (decimal)0.3)
            {
                sumWeight = (decimal)0.3;
            }

            PackDetailInsert packDetailInsertFirst = getPackDetailInsertList.OrderByDescending(u => u.Weight).ToList().First();

            //包装开始前 验证备货装箱是否完成
            var sql = (from a in idal.IPickTaskDetailDAL.SelectAll()
                       where a.LoadId == entityFirst.LoadId && a.WhCode == entityFirst.WhCode
                       && a.Status == "C" && a.Status1 == "U"
                       select a);
            if (sql.Count() > 0)
            {
                return "该Load:" + entityFirst.LoadId + "自动装箱有误，请查看备货任务后手动装箱！";
            }

            using (TransactionScope trans = new TransactionScope())
            {
                try
                {
                    #region 开始包装

                    //0. 更新Load的开始包装时间
                    LoadMaster load = idal.ILoadMasterDAL.SelectBy(u => u.LoadId == entityFirst.LoadId && u.WhCode == entityFirst.WhCode).First();

                    List<PackHead> getOne = (from a in idal.IPackHeadDAL.SelectAll()
                                             join b in idal.IPackTaskDAL.SelectAll()
                                             on a.PackTaskId equals b.Id
                                             where b.LoadId == entityFirst.LoadId && b.WhCode == entityFirst.WhCode
                                             select a).Take(1).ToList();
                    if (getOne.Count == 0)
                    {
                        load.BeginPackDate = DateTime.Now;
                        load.UpdateUser = userName;
                        load.UpdateDate = DateTime.Now;
                        idal.ILoadMasterDAL.UpdateBy(load, u => u.Id == load.Id, new string[] { "BeginPackDate", "UpdateUser", "UpdateDate" });
                    }

                    foreach (var entity in entityList)
                    {
                        //SingleFlag == 1 单品，SingleFlag == 2 组合

                        PackTask packTask = PackTaskList.Where(u => u.LoadId == entity.LoadId && u.WhCode == entity.WhCode && u.OutPoNumber == entity.OutPoNumber && u.SortGroupId == entity.GroupId).First();

                        string result = "";

                        //---------插入包装数据
                        //1.---------先反更新分拣明细已包装数量
                        foreach (var item in getPackDetailInsertList)
                        {
                            SortTaskDetail updateEntity = new SortTaskDetail();
                            updateEntity.LoadId = entity.LoadId;
                            updateEntity.WhCode = entity.WhCode;
                            updateEntity.OutPoNumber = entity.OutPoNumber;
                            updateEntity.GroupId = entity.GroupId;

                            updateEntity.ItemId = item.ItemId;
                            updateEntity.PackQty = item.Qty;
                            updateEntity.UpdateUser = entity.userName;
                            updateEntity.UpdateDate = DateTime.Now;
                            idal.ISortTaskDetailDAL.UpdateBy(updateEntity, u => u.LoadId == entity.LoadId && u.WhCode == entity.WhCode && u.OutPoNumber == entity.OutPoNumber && u.GroupId == entity.GroupId && u.ItemId == item.ItemId, new string[] { "PackQty", "UpdateUser", "UpdateDate" });
                        }

                        //2.----------插入包装头表
                        PackHead packHead = new PackHead();
                        List<PackHead> checkPackHeadCount = idal.IPackHeadDAL.SelectBy(u => u.PackTaskId == packTask.Id && u.WhCode == packTask.WhCode && u.PackNumber == entity.PackNumber && u.Status == 10).ToList();
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
                            packHead.PackNumber = entity.PackNumber;
                            packHead.Status = 10;
                            packHead.PackCarton = packDetailInsertFirst.CartonName ?? "";
                            packHead.Weight = sumWeight;
                            packHead.CreateUser = entity.userName;
                            packHead.CreateDate = DateTime.Now;
                            idal.IPackHeadDAL.Add(packHead);
                        }
                        else
                        {
                            packHead = checkPackHeadCount.First();
                            PackHead update = new PackHead();
                            update.Status = 10;
                            update.UpdateUser = entity.userName;
                            update.UpdateDate = DateTime.Now;
                            idal.IPackHeadDAL.UpdateBy(update, u => u.Id == packHead.Id, new string[] { "Status", "UpdateUser", "UpdateDate" });

                            if (!string.IsNullOrEmpty(packHead.ExpressNumber))
                            {
                                result = "请重新再扫描下款号！";
                            }
                        }

                        if (result + "" != "")
                        {
                            continue;
                        }

                        //3.----------插入包装明细表
                        foreach (var item in getPackDetailInsertList)
                        {
                            PackDetail packDetail = new PackDetail();
                            packDetail.WhCode = packTask.WhCode;
                            packDetail.PackHeadId = packHead.Id;
                            packDetail.ItemId = item.ItemId;
                            packDetail.Qty = (Int32)item.Qty;
                            packDetail.CreateUser = entity.userName;
                            packDetail.CreateDate = DateTime.Now;
                            idal.IPackDetailDAL.Add(packDetail);
                        }

                        //更新包装任务状态 为 已完成
                        PackTask setPackTask = new PackTask();
                        setPackTask.Status = 30;
                        setPackTask.UpdateUser = entity.userName;
                        setPackTask.UpdateDate = DateTime.Now;
                        idal.IPackTaskDAL.UpdateBy(setPackTask, u => u.Id == packTask.Id, new string[] { "Status", "UpdateUser", "UpdateDate" });

                        //提交一次,不然分拣明细状态更新不出来
                        idal.SaveChanges();
                    }

                    //批量一次性更新订单状态
                    string[] outPoNumberarr = (from a in PackTaskList
                                               select a.OutPoNumber).ToList().Distinct().ToArray();

                    List<OutBoundOrder> outBoundOrderList = idal.IOutBoundOrderDAL.SelectBy(u => outPoNumberarr.Contains(u.OutPoNumber) && u.WhCode == entityFirst.WhCode);


                    List<TranLog> tranLogAdd = new List<TranLog>();
                    if (outBoundOrderList.Count > 0)
                    {
                        OutBoundOrder firstOrder = outBoundOrderList.First();

                        FlowDetail flowDetail = idal.IFlowDetailDAL.SelectBy(u => u.FlowHeadId == firstOrder.ProcessId && u.Type == "PackingType").First();

                        foreach (var item in outBoundOrderList)
                        {
                            if (item.StatusId != -10)
                            {
                                if (flowDetail != null && flowDetail.StatusId != 0)
                                {
                                    OutBoundOrder outBoundOrder = item;
                                    string remark1 = "原状态：" + outBoundOrder.StatusId + outBoundOrder.StatusName;

                                    //更新出库订单状态为已包装
                                    outBoundOrder.NowProcessId = flowDetail.FlowRuleId;
                                    outBoundOrder.StatusId = flowDetail.StatusId;
                                    outBoundOrder.StatusName = flowDetail.StatusName;
                                    outBoundOrder.UpdateUser = userName;
                                    outBoundOrder.UpdateDate = DateTime.Now;
                                    //使用对象实体 也能直接变更订单状态，无需UpdateBy
                                    //idal.IOutBoundOrderDAL.UpdateBy(outBoundOrder, u => u.Id == outBoundOrder.Id, new string[] { "NowProcessId", "StatusId", "StatusName", "UpdateUser", "UpdateDate" });

                                    //更新订单状态，插入日志
                                    TranLog tlorder = new TranLog();
                                    tlorder.TranType = "32";
                                    tlorder.Description = "更新订单状态";
                                    tlorder.TranDate = DateTime.Now;
                                    tlorder.TranUser = userName;
                                    tlorder.WhCode = entityFirst.WhCode;
                                    tlorder.LoadId = entityFirst.LoadId;
                                    tlorder.CustomerOutPoNumber = outBoundOrder.CustomerOutPoNumber;
                                    tlorder.OutPoNumber = outBoundOrder.OutPoNumber;
                                    tlorder.Remark = remark1 + "变更为：" + outBoundOrder.StatusId + outBoundOrder.StatusName;
                                    tranLogAdd.Add(tlorder);
                                }
                            }
                        }
                    }

                    idal.ITranLogDAL.Add(tranLogAdd);

                    //6. 更新Load的完成包装时间
                    List<PackTask> checkPackTaskList1 = idal.IPackTaskDAL.SelectBy(u => u.LoadId == entityFirst.LoadId && u.WhCode == entityFirst.WhCode);
                    if (checkPackTaskList1.Where(u => u.Status == 0 || u.Status == 10 || u.Status == 20).Count() == 0)
                    {
                        load.EndPackDate = DateTime.Now;
                        load.UpdateUser = userName;
                        load.UpdateDate = DateTime.Now;

                        idal.ILoadMasterDAL.UpdateBy(load, u => u.Id == load.Id, new string[] { "EndPackDate", "UpdateUser", "UpdateDate" });
                    }

                    #endregion

                    idal.SaveChanges();
                    trans.Complete();
                    return "Y";
                }
                catch (Exception e)
                {
                    string s = e.InnerException.Message;
                    trans.Dispose();//出现异常，事务手动释放
                                    // Monitor.Exit(o1);
                    return "一键包装异常，请重新提交！";
                }

            }
        }



        //UCF随箱单报表查询-批量
        public List<PackTaskCryReport> UCFGetCryReportPackTask(int[] packHeadId, string whCode, string userName, int type)
        {
            List<PackTaskCryReport> sqlList = new List<PackTaskCryReport>();
            if (type == 0)
            {
                var sql = from a in idal.IPackTaskDAL.SelectAll()
                          join b in idal.IPackHeadDAL.SelectAll() on new { Id = a.Id } equals new { Id = (Int32)b.PackTaskId }
                          where packHeadId.Contains(b.Id) && b.PrintDate == null
                          group new { a } by new
                          {
                              a.WhCode,
                              a.LoadId,
                              a.OutPoNumber,
                              a.CustomerOutPoNumber
                          } into g
                          select new PackTaskCryReport
                          {
                              WhCode = g.Key.WhCode,
                              LoadId = g.Key.LoadId,
                              OutPoNumber = g.Key.OutPoNumber,
                              CustomerPoNumber = g.Key.CustomerOutPoNumber
                          };
                sqlList = sql.OrderBy(u => u.OutPoNumber).ToList();
            }
            else
            {
                var sql = from a in idal.IPackTaskDAL.SelectAll()
                          join b in idal.IPackHeadDAL.SelectAll() on new { Id = a.Id } equals new { Id = (Int32)b.PackTaskId }
                          where packHeadId.Contains(b.Id)
                          group new { a } by new
                          {
                              a.WhCode,
                              a.LoadId,
                              a.OutPoNumber,
                              a.CustomerOutPoNumber
                          } into g
                          select new PackTaskCryReport
                          {
                              WhCode = g.Key.WhCode,
                              LoadId = g.Key.LoadId,
                              OutPoNumber = g.Key.OutPoNumber,
                              CustomerPoNumber = g.Key.CustomerOutPoNumber
                          };
                sqlList = sql.OrderBy(u => u.OutPoNumber).ToList();
            }

            if (sqlList.Count == 0)
            {
                return sqlList;
            }

            PackTaskCryReport first = sqlList.First();

            //得到订单Json列表
            //1.得到订单数组
            string[] CustomerOutPoNumber = (from a in sqlList
                                            select a.CustomerPoNumber).ToList().Distinct().ToArray();
            //2.一次性拉取订单Json列表
            List<PackTaskJson> packTaskJsonList = (from a in idal.IPackTaskJsonDAL.SelectAll()
                                                   where a.WhCode == first.WhCode && CustomerOutPoNumber.Contains(a.CustomerOutPoNumber)
                                                   select a).ToList();

            List<PackTaskCryReport> sqlResult = new List<PackTaskCryReport>();
            foreach (var item in sqlList)
            {
                if (sqlResult.Where(u => u.CustomerPoNumber == item.CustomerPoNumber).Count() > 0)
                {
                    continue;
                }

                PackTaskJson packJson = new PackTaskJson();
                PackTaskJsonEntity packTaskJsonEntity = new PackTaskJsonEntity();

                if (packTaskJsonList.Where(u => u.CustomerOutPoNumber == item.CustomerPoNumber).Count() > 0)
                {
                    packJson = packTaskJsonList.Where(u => u.CustomerOutPoNumber == item.CustomerPoNumber).First();
                    packTaskJsonEntity = JsonConvert.DeserializeObject<PackTaskJsonEntity>(packJson.Json);
                }

                PackTaskCryReport packCry = new PackTaskCryReport();
                packCry.WhCode = item.WhCode;
                packCry.LoadId = item.LoadId;
                packCry.OutPoNumber = item.OutPoNumber;
                packCry.CustomerPoNumber = item.CustomerPoNumber;

                if (packTaskJsonEntity != null)
                {
                    packCry.CustomerName = packTaskJsonEntity.CustomerName ?? "";
                    packCry.CustomerRef = packTaskJsonEntity.CustomerRef ?? "";
                    packCry.BusinessMode = packTaskJsonEntity.BusinessMode ?? "";
                    packCry.d_contact = packTaskJsonEntity.d_contact ?? "";
                    packCry.d_tel = packTaskJsonEntity.d_tel ?? "";
                    packCry.d_address = packTaskJsonEntity.d_address ?? "";
                    packCry.CreateDate = packTaskJsonEntity.OrderCreateDate ?? DateTime.Now;
                }

                packCry.Price = item.Price ?? 0;
                packCry.PackGroupId = 0;
                packCry.TotalPrice = item.TotalPrice ?? 0;
                packCry.Length = 0;
                packCry.Height = 0;
                packCry.Width = 0;
                packCry.Weight = 0;
                packCry.ItemId = 0;
                packCry.Qty = 1;

                sqlResult.Add(packCry);
            }

            return sqlResult.OrderBy(u => u.OutPoNumber).ToList();


        }

        //快递面单报表查询-批量
        public List<PackTaskCryReportExpress> GetCryReportExpressPackTask(int?[] packHeadId, string whCode, string userName, int type)
        {
            List<PackTaskCryReportExpress> sqlList = new List<PackTaskCryReportExpress>();
            if (type == 0)
            {
                var sql = from a in idal.IPackTaskDAL.SelectAll()
                          join b in idal.IPackHeadDAL.SelectAll() on new { Id = a.Id } equals new { Id = (Int32)b.PackTaskId } into b_join
                          from b in b_join.DefaultIfEmpty()
                          where packHeadId.Contains(b.Id) && b.PrintDate == null
                          select new PackTaskCryReportExpress
                          {
                              WhCode = a.WhCode,
                              LoadId = a.LoadId,
                              OutPoNumber = a.OutPoNumber,
                              CustomerOutPoNumber = a.CustomerOutPoNumber,
                              express_type_zh = a.express_type_zh,
                              express_code = a.express_code,
                              PackQty = a.PackQty ?? 0,
                              dest_code = a.dest_code ?? "",
                              PackGroupId = b.PackGroupId,
                              ExpressNumber = b.ExpressNumber,
                              ExpressNumberParent = b.ExpressNumberParent ?? "",
                              ExpressNumberBar = null,
                              packHeadId = b.Id,
                              Weight = b.Weight ?? 0,
                              DNBar = null
                          };

                sqlList = sql.OrderBy(u => u.OutPoNumber).ToList();
            }
            else
            {
                var sql = from a in idal.IPackTaskDAL.SelectAll()
                          join b in idal.IPackHeadDAL.SelectAll() on new { Id = a.Id } equals new { Id = (Int32)b.PackTaskId } into b_join
                          from b in b_join.DefaultIfEmpty()
                          where packHeadId.Contains(b.Id)
                          select new PackTaskCryReportExpress
                          {
                              WhCode = a.WhCode,
                              LoadId = a.LoadId,
                              OutPoNumber = a.OutPoNumber,
                              CustomerOutPoNumber = a.CustomerOutPoNumber,
                              express_type_zh = a.express_type_zh,
                              express_code = a.express_code,
                              PackQty = a.PackQty ?? 0,
                              dest_code = a.dest_code ?? "",
                              PackGroupId = b.PackGroupId,
                              ExpressNumber = b.ExpressNumber,
                              ExpressNumberParent = b.ExpressNumberParent ?? "",
                              ExpressNumberBar = null,
                              packHeadId = b.Id,
                              Weight = b.Weight ?? 0,
                              DNBar = null
                          };

                sqlList = sql.OrderBy(u => u.OutPoNumber).ToList();
            }

            if (sqlList.Count == 0)
            {
                return sqlList;
            }

            PackTaskCryReportExpress first = sqlList.First();

            //得到订单Json列表
            //1.得到订单数组
            string[] CustomerOutPoNumber = (from a in sqlList
                                            select a.CustomerOutPoNumber).ToList().Distinct().ToArray();
            //2.一次性拉取订单Json列表
            List<PackTaskJson> packTaskJsonList = (from a in idal.IPackTaskJsonDAL.SelectAll()
                                                   where a.WhCode == first.WhCode && CustomerOutPoNumber.Contains(a.CustomerOutPoNumber)
                                                   select a).ToList();

            string[] ExpressNumberArr = (from a in sqlList
                                         select a.ExpressNumber).ToList().Distinct().ToArray();

            List<PackHeadJson> packHeadJsonList = new List<PackHeadJson>();
            //3.一次性拉取订单Json列表

            packHeadJsonList = (from a in idal.IPackHeadJsonDAL.SelectAll()
                                where a.WhCode == first.WhCode && ExpressNumberArr.Contains(a.ExpressNumber)
                                select a).ToList();


            List<PackDetail> getPackDetailList = idal.IPackDetailDAL.SelectBy(u => packHeadId.Contains(u.PackHeadId));
            int?[] getitemIdArr = (from a in getPackDetailList
                                   select a.ItemId).ToList().Distinct().ToArray();

            List<ItemMaster> getItemMasterList = idal.IItemMasterDAL.SelectBy(u => getitemIdArr.Contains(u.Id));

            List<PackTaskCryReportExpress> sqlResult = new List<PackTaskCryReportExpress>();
            foreach (var item in sqlList)
            {
                if (sqlResult.Where(u => u.CustomerOutPoNumber == item.CustomerOutPoNumber).Count() > 0)
                {
                    continue;
                }
                PackTaskJson packJson = new PackTaskJson();
                PackTaskJsonEntity packTaskJsonEntity = new PackTaskJsonEntity();

                if (packTaskJsonList.Where(u => u.CustomerOutPoNumber == item.CustomerOutPoNumber).Count() > 0)
                {
                    packJson = packTaskJsonList.Where(u => u.CustomerOutPoNumber == item.CustomerOutPoNumber).First();
                    packTaskJsonEntity = JsonConvert.DeserializeObject<PackTaskJsonEntity>(packJson.Json);
                }

                PackHeadJson packHeadJson = new PackHeadJson();
                PackHeadJsonEntity packHeadJsonEntity = new PackHeadJsonEntity();

                PackHeadJsonEntityZTO packHeadJsonEntityZTO = new PackHeadJsonEntityZTO();

                PackTaskCryReportExpress eneity = new PackTaskCryReportExpress();
                eneity.WhCode = item.WhCode;
                eneity.LoadId = item.LoadId;
                eneity.express_code = item.express_code;
                eneity.CustomerOutPoNumber = item.CustomerOutPoNumber;
                eneity.express_type_zh = item.express_type_zh ?? "";

                if (item.PackQty == null)
                {
                    eneity.PackQty = 0;
                }
                else
                {
                    eneity.PackQty = item.PackQty ?? 0;
                }

                if (packTaskJsonEntity != null)
                {
                    eneity.SinglePlaneTemplate = packTaskJsonEntity.SinglePlaneTemplate;
                }
                else
                {
                    eneity.SinglePlaneTemplate = item.express_code + "_bz";
                }

                string remark = "";
                List<PackDetail> packDetailList = getPackDetailList.Where(u => u.PackHeadId == item.packHeadId).ToList();
                int?[] itemIdArr = (from a in packDetailList
                                    select a.ItemId).ToList().Distinct().ToArray();

                List<ItemMaster> itemMasterList = getItemMasterList.Where(u => itemIdArr.Contains(u.Id)).ToList();

                foreach (var packDetail in packDetailList)
                {
                    ItemMaster getItem = itemMasterList.Where(u => u.Id == packDetail.ItemId).First();
                    remark += getItem.AltItemNumber + "*" + packDetail.Qty + ",";
                }

                eneity.Remark = "客户订单号:" + item.CustomerOutPoNumber;

                //取得顺丰加密信息
                if (eneity.express_code == "SF")
                {
                    if (packHeadJsonList.Where(u => u.ExpressNumber == item.ExpressNumber).Count() > 0)
                    {
                        //如果不是子母单
                        if (string.IsNullOrEmpty(item.ExpressNumberParent))
                        {
                            packHeadJson = packHeadJsonList.Where(u => u.ExpressNumber == item.ExpressNumber).First();
                            packHeadJsonEntity = JsonConvert.DeserializeObject<PackHeadJsonEntity>(packHeadJson.Json);

                        }
                        else
                        {
                            packHeadJson = packHeadJsonList.Where(u => u.ExpressNumber == item.ExpressNumberParent).First();
                            packHeadJsonEntity = JsonConvert.DeserializeObject<PackHeadJsonEntity>(packHeadJson.Json);
                        }

                        if (packHeadJsonEntity != null)
                        {
                            eneity.proCode = packHeadJsonEntity.proCode;
                            if (string.IsNullOrEmpty(eneity.proCode))
                            {
                                eneity.proName = packHeadJsonEntity.proName;
                            }
                            eneity.destRouteLabel = packHeadJsonEntity.destRouteLabel;
                            eneity.destTeamCode = packHeadJsonEntity.destTeamCode;
                            eneity.codingMapping = packHeadJsonEntity.codingMapping;
                            eneity.xbFlag = packHeadJsonEntity.xbFlag;
                            eneity.codingMappingOut = packHeadJsonEntity.codingMappingOut;
                            eneity.printIcon = packHeadJsonEntity.printIcon;
                            if (!string.IsNullOrEmpty(item.ExpressNumberParent))
                            {
                                eneity.twoDimensionCode = packHeadJsonEntity.twoDimensionCode.Replace(item.ExpressNumberParent, item.ExpressNumber);
                            }
                            else
                            {
                                eneity.twoDimensionCode = packHeadJsonEntity.twoDimensionCode;
                            }
                        }
                    }
                }

                //取得中通加密信息
                if (eneity.express_code == "ZTO")
                {
                    if (packHeadJsonList.Where(u => u.ExpressNumber == item.ExpressNumber).Count() > 0)
                    {
                        packHeadJson = packHeadJsonList.Where(u => u.ExpressNumber == item.ExpressNumber).First();
                        packHeadJsonEntityZTO = JsonConvert.DeserializeObject<PackHeadJsonEntityZTO>(packHeadJson.Json);
                    }

                    if (packHeadJsonEntityZTO != null)
                    {
                        eneity.bagAddr = packHeadJsonEntityZTO.bagAddr;
                    }
                }

                if (!string.IsNullOrEmpty(packTaskJsonEntity.CustomerOutPoNumber))
                {
                    eneity.j_company = packTaskJsonEntity.j_company ?? "";
                    eneity.j_contact = packTaskJsonEntity.j_contact ?? "";
                    eneity.j_tel = packTaskJsonEntity.j_tel ?? "";
                    eneity.j_province = packTaskJsonEntity.j_province ?? "";
                    eneity.j_city = packTaskJsonEntity.j_city ?? "";
                    eneity.j_county = packTaskJsonEntity.j_county ?? "";
                    eneity.j_address = packTaskJsonEntity.j_address ?? "";
                    eneity.d_company = packTaskJsonEntity.d_company ?? "";
                    eneity.d_contact = packTaskJsonEntity.d_contact ?? "";
                    eneity.d_tel = packTaskJsonEntity.d_tel ?? "";

                    eneity.PayMethodShow = packTaskJsonEntity.payMethod == "2" ? "到付" : "寄付月结";

                    if (eneity.express_code == "SF")
                    {
                        if (eneity.d_tel.Length == 11)
                        {
                            eneity.d_tel = eneity.d_tel.Substring(0, 3) + "****" + eneity.d_tel.Substring(7, 4);
                        }
                        else if (eneity.d_tel.Length == 16)
                        {
                            eneity.d_tel = eneity.d_tel;
                        }
                        else if (eneity.d_tel.Length > 4)
                        {
                            eneity.d_tel = "******" + eneity.d_tel.Substring(eneity.d_tel.Length - 4, 4);
                        }
                        else
                        {
                            eneity.d_tel = eneity.d_tel;
                        }
                    }

                    eneity.d_Province = packTaskJsonEntity.d_Province ?? "";
                    eneity.d_city = packTaskJsonEntity.d_city ?? "";
                    eneity.d_address = packTaskJsonEntity.d_address ?? "";
                    eneity.custid = packTaskJsonEntity.custid ?? "";
                    eneity.cod = packTaskJsonEntity.cod.ToString() ?? "";
                    eneity.j_name = packTaskJsonEntity.j_name ?? "";
                    eneity.form_code = packTaskJsonEntity.form_code ?? "";
                    eneity.DN = packTaskJsonEntity.AltCustomerOutPoNumber ?? "";
                    eneity.CustomerPo = packTaskJsonEntity.customerPo ?? "";

                    eneity.AirFlag = packTaskJsonEntity.AirFlag == "0" ? "不可航空" :
                                    packTaskJsonEntity.AirFlag == "1" ? "可航空" : "不可航空";

                    eneity.Remark += " 订单编号:" + packTaskJsonEntity.AltCustomerOutPoNumber;
                }

                eneity.Remark += " " + remark.Substring(0, remark.Length - 1);

                eneity.dest_code = item.dest_code ?? "";
                eneity.PackGroupId = item.PackGroupId;
                eneity.packHeadId = item.packHeadId;

                if (!string.IsNullOrEmpty(item.ExpressNumberParent))
                {
                    eneity.ExpressNumberParent = "母单号 " + item.ExpressNumberParent;
                    eneity.ExpressNumberParentShow = "1/x";
                }

                eneity.ExpressNumber = item.ExpressNumber;
                eneity.ExpressNumberBar = null;
                eneity.Weight = item.Weight ?? 0;
                eneity.DNBar = null;

                sqlResult.Add(eneity);
            }

            if (type == 0)
            {
                foreach (var item in packHeadId)
                {
                    idal.IPackHeadDAL.UpdateByExtended(u => u.Id == item, t => new PackHead { PrintDate = DateTime.Now });
                }
            }

            return sqlResult.OrderBy(u => u.OutPoNumber).ToList();
        }


        #endregion

    }
}
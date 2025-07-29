using MODEL_MSSQL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using WMS.BLLClass;
using WMS.Express;
using WMS.IBLL;

namespace WMS.BLL
{
    public class TransferTaskManager : ITransferTaskManager
    {
        IDAL.IDALSession idal = BLL.BLLHelper.GetDal();
        private static object o = new object();

        public static object o1 = new object();

        public static object o2 = new object();
        public static object o3 = new object();

        //获取交接框号已扫描的快递单列表
        public List<TransferHeadResult> GetExpressNumberList(string transferNumber, string whCode)
        {
            var sql = (from a in idal.ITransferTaskDAL.SelectAll()
                       join b in idal.ITransferHeadDAL.SelectAll()
                             on new { a.Id, a.WhCode }
                         equals new { Id = (Int32)b.TransferTaskId, b.WhCode }
                       where a.TransferNumber == transferNumber && a.WhCode == whCode && a.Status != 30
                       select new TransferHeadResult
                       {
                           TransferTaskId = ((Int32?)a.Id ?? (Int32?)0),
                           Id = ((Int32?)b.Id ?? (Int32?)0),
                           ExpressNumber = b.ExpressNumber,
                           CreateUser = b.CreateUser,
                           CreateDate = b.CreateDate
                       }).Distinct();

            return sql.ToList();
        }


        //获取交接框号已扫描的出库单列表
        public List<TransferHeadResult> GetCustomerOutPoNumberList(string transferNumber, string whCode)
        {
            var sql = from a in idal.ITransferTaskDAL.SelectAll()
                      join b in idal.ITransferHeadDAL.SelectAll()
                            on new { a.Id, a.WhCode }
                        equals new { Id = (Int32)b.TransferTaskId, b.WhCode }
                      where a.TransferNumber == transferNumber && a.WhCode == whCode && a.Status != 30
                      group b by new
                      {
                          b.TransferTaskId,
                          b.CustomerOutPoNumber,
                          b.PackNumber,
                          b.CreateUser
                      } into g
                      select new TransferHeadResult
                      {
                          TransferTaskId = g.Key.TransferTaskId,
                          CustomerOutPoNumber = g.Key.CustomerOutPoNumber,
                          PackNumber = g.Key.PackNumber,
                          CreateUser = g.Key.CreateUser,
                          CreateDate = g.Min(p => p.CreateDate)
                      };

            return sql.ToList();
        }


        //单品交接 
        //工作台中 扫描快递单号后 验证是否存在 及插入数据等
        public string TransferTaskInsert(string transferNumber, string userName, string expressNumber, string whCode)
        {
            lock (o3)
            {
                using (TransactionScope trans = new TransactionScope())
                {
                    try
                    {
                        #region 开始交接

                        List<TransferTask> checkTransferHeadList = (from a in idal.ITransferHeadDAL.SelectAll()
                                                                    join b in idal.ITransferTaskDAL.SelectAll()
                                                                    on a.TransferTaskId equals b.Id
                                                                    where a.WhCode == whCode && a.ExpressNumber == expressNumber
                                                                    select b).ToList();
                        if (checkTransferHeadList.Count > 0)
                        {
                            TransferTask transferTaskFirst = checkTransferHeadList.First();
                            if (transferTaskFirst.Status == 30)
                            {
                                return "J快递单号已交接！";
                            }
                            else
                            {
                                return "C快递单号扫描重复！";
                            }
                        }

                        List<PackHead> packHeadList = idal.IPackHeadDAL.SelectBy(u => u.ExpressNumber == expressNumber && u.WhCode == whCode);

                        if (packHeadList.Count == 0)
                        {
                            return "N快递单号不存在！";
                        }
                        else
                        {
                            if (packHeadList.Where(u => (u.TransferHeadId ?? "") != "").Count() > 0)
                            {
                                return "S快递单号已扫描！";
                            }
                        }

                        if (packHeadList.Where(u => u.Status == -10).Count() > 0)
                        {
                            return "L快递单号被拦截：" + expressNumber;
                        }


                        PackHead packHead = packHeadList.First();

                        PackTask packTask = idal.IPackTaskDAL.SelectBy(u => u.Id == packHead.PackTaskId).First();

                        //1.根据输入的交接框号 验证是否已创建了交接任务
                        List<TransferTask> TransferTaskList = idal.ITransferTaskDAL.SelectBy(u => u.TransferNumber == transferNumber && u.WhCode == whCode);
                        TransferTask transferTask = new TransferTask();
                        if (TransferTaskList.Count == 0)
                        {
                            transferTask.WhCode = whCode;
                            transferTask.TransferId = "TF" + DI.IDGenerator.NewId;
                            transferTask.TransferNumber = transferNumber;
                            transferTask.TransportType = packTask.TransportType;
                            transferTask.express_code = packTask.express_code;
                            transferTask.express_type = packTask.express_type;
                            transferTask.express_type_zh = packTask.express_type_zh;
                            transferTask.Status = 0;
                            transferTask.CreateUser = userName;
                            transferTask.CreateDate = DateTime.Now;
                            idal.ITransferTaskDAL.Add(transferTask);
                        }
                        else
                        {
                            if (TransferTaskList.Where(u => u.Status == 0).Count() == 0)
                            {
                                transferTask.WhCode = whCode;
                                transferTask.TransferId = "TF" + DI.IDGenerator.NewId;
                                transferTask.TransferNumber = transferNumber;
                                transferTask.TransportType = packTask.TransportType;
                                transferTask.express_code = packTask.express_code;
                                transferTask.express_type = packTask.express_type;
                                transferTask.express_type_zh = packTask.express_type_zh;
                                transferTask.Status = 0;
                                transferTask.CreateUser = userName;
                                transferTask.CreateDate = DateTime.Now;
                                idal.ITransferTaskDAL.Add(transferTask);
                            }
                            else
                            {
                                transferTask = TransferTaskList.Where(u => u.Status == 0).First();
                                List<TransferHead> GetTransferHeadCount = idal.ITransferHeadDAL.SelectBy(u => u.TransferTaskId == transferTask.Id);

                                if (GetTransferHeadCount.Count == 0)
                                {
                                    transferTask.express_code = packTask.express_code;
                                    transferTask.express_type = packTask.express_type;
                                    transferTask.express_type_zh = packTask.express_type_zh;
                                    idal.ITransferTaskDAL.UpdateBy(transferTask, u => u.Id == transferTask.Id, new string[] { "express_code", "express_type", "express_type_zh" });
                                }
                            }
                        }

                        //2. 如果创建过交接任务 比较多次输入的快递公司是否为同一个
                        if (packTask.express_code != transferTask.express_code)
                        {
                            return "N快递单对应的快递公司或出库方式必须相同！";
                        }

                        idal.ITransferTaskDAL.SaveChanges();

                        //3.根据快递单号 获取包装信息后 插入交接表
                        TransferHead transferHead = new TransferHead();
                        transferHead.WhCode = whCode;
                        transferHead.TransferTaskId = transferTask.Id;
                        transferHead.LoadId = packTask.LoadId;
                        transferHead.SortGroupId = packTask.SortGroupId;
                        transferHead.SortGroupNumber = packTask.SortGroupNumber;
                        transferHead.CustomerOutPoNumber = packTask.CustomerOutPoNumber;
                        transferHead.OutPoNumber = packTask.OutPoNumber;
                        transferHead.PackGroupId = packHead.PackGroupId;
                        transferHead.PackNumber = packHead.PackNumber;
                        transferHead.ExpressNumber = packHead.ExpressNumber;
                        transferHead.Length = packHead.Length;
                        transferHead.Width = packHead.Width;
                        transferHead.Height = packHead.Height;
                        transferHead.Weight = packHead.Weight;
                        transferHead.PackCarton = packHead.PackCarton;
                        transferHead.Status = 10;
                        transferHead.CreateUser = userName;
                        transferHead.CreateDate = DateTime.Now;
                        idal.ITransferHeadDAL.Add(transferHead);
                        idal.ITransferHeadDAL.SaveChanges();

                        //修改包装
                        PackHead updatePackHead = new PackHead();
                        updatePackHead.TransferHeadId = transferHead.Id.ToString();
                        updatePackHead.UpdateUser = userName;
                        updatePackHead.UpdateDate = DateTime.Now;
                        idal.IPackHeadDAL.UpdateBy(updatePackHead, u => u.Id == packHead.Id, new string[] { "TransferHeadId", "UpdateUser", "UpdateDate" });

                        List<TransferDetail> transferDetailList = new List<TransferDetail>();
                        List<TransferScanNumber> transferScanNumberList = new List<TransferScanNumber>();

                        //4.获取包装款号明细 插入交接表
                        List<PackDetail> packDetailList = idal.IPackDetailDAL.SelectBy(u => u.PackHeadId == packHead.Id);
                        foreach (var item in packDetailList)
                        {
                            ItemMaster itemMaster = idal.IItemMasterDAL.SelectBy(u => u.Id == item.ItemId).First();
                            TransferDetail transferDetail = new TransferDetail();
                            transferDetail.WhCode = whCode;
                            transferDetail.TransferHeadId = transferHead.Id;
                            transferDetail.ItemId = item.ItemId;
                            transferDetail.AltItemNumber = itemMaster.AltItemNumber;
                            transferDetail.PlanQty = item.PlanQty;
                            transferDetail.Qty = item.Qty;
                            transferDetail.CreateUser = userName;
                            transferDetail.CreateDate = DateTime.Now;
                            transferDetailList.Add(transferDetail);

                            //5.获取包装款号扫描 插入交接表
                            List<PackScanNumber> packScanNumberList = idal.IPackScanNumberDAL.SelectBy(u => u.PackDetailId == item.Id);
                            foreach (var item1 in packScanNumberList)
                            {
                                TransferScanNumber transferScanNumber = new TransferScanNumber();
                                transferScanNumber.WhCode = whCode;
                                transferScanNumber.TransferHeadId = transferHead.Id;
                                transferScanNumber.ItemId = item.ItemId;
                                transferScanNumber.AltItemNumber = itemMaster.AltItemNumber;
                                transferScanNumber.ScanNumber = item1.ScanNumber;
                                transferScanNumber.CreateUser = userName;
                                transferScanNumber.CreateDate = DateTime.Now;
                                transferScanNumberList.Add(transferScanNumber);
                            }
                        }

                        idal.ITransferDetailDAL.Add(transferDetailList);
                        idal.ITransferScanNumberDAL.Add(transferScanNumberList);
                        #endregion

                        idal.ITransferScanNumberDAL.SaveChanges();
                        trans.Complete();
                        return "Y" + transferTask.Id;
                    }
                    catch
                    {
                        trans.Dispose();//出现异常，事务手动释放
                        return "N交接异常，请重新提交！";
                    }
                }
            }
        }

        //---------------------------------------------确认交接执行方法

        #region 原交接执行方法 
        public string BeginTransferTask1(int transferId, string userName, int fayunQty)
        {
            List<TransferTask> TransferTaskList = idal.ITransferTaskDAL.SelectBy(u => u.Id == transferId);
            if (TransferTaskList.Count == 0)
            {
                return "未找到交接信息，请查询！";
            }

            TransferTask transferTask = TransferTaskList.First();
            if (transferTask.Status == -10)
            {
                return "交接任务被拦截，请查询！";
            }
            if (transferTask.Status == 30)
            {
                return "当前交接任务已完成交接！";
            }

            //拦截交接中的快递单
            List<TransferHead> TransferHeadList = idal.ITransferHeadDAL.SelectBy(u => u.TransferTaskId == transferTask.Id);

            string checkResult = "";
            foreach (var item in TransferHeadList.Where(u => u.Status == -10))
            {
                checkResult += "快递单号：" + item.ExpressNumber + "被拦截！";
            }
            if (checkResult != "")
            {
                return checkResult;
            }

            List<TransferHeadDetailResult> getCount = (from transferhead in
                                                       (from transferhead in idal.ITransferHeadDAL.SelectAll()
                                                        where
                                                          transferhead.TransferTaskId == transferId
                                                        select new
                                                        {
                                                            Dummy = "x"
                                                        })
                                                       group transferhead by new { transferhead.Dummy } into g
                                                       select new TransferHeadDetailResult
                                                       {
                                                           SumQty = g.Count()
                                                       }).ToList();
            if (getCount.Count == 0)
            {
                return "没有交接信息！";
            }

            if (getCount.First().SumQty != fayunQty)
            {
                return "发运数量与系统数量不一致！";
            }

            //并发锁
            lock (o1)
            {
                using (TransactionScope trans = new TransactionScope())
                {
                    try
                    {
                        List<TranLog> tlList = new List<TranLog>();

                        //添加交接日志
                        TranLog tl2 = new TranLog();
                        tl2.TranType = "405";
                        tl2.Description = "开始验证交接";
                        tl2.TranDate = DateTime.Now;
                        tl2.TranUser = userName;
                        tl2.WhCode = transferTask.WhCode;
                        tl2.Remark = "交接号：" + transferTask.TransferId;

                        idal.ITranLogDAL.Add(tl2);
                        idal.SaveChanges();
                        //tlList.Add(tl2);

                        //验证分拣表数量 与 实际交接数量 是否一致
                        string result = "";
                        List<TransferHeadDetailResult> checkList = (from c in idal.ITransferTaskDAL.SelectAll()
                                                                    join a in idal.ITransferHeadDAL.SelectAll() on new { Id = c.Id } equals new { Id = (Int32)a.TransferTaskId } into a_join
                                                                    from a in a_join.DefaultIfEmpty()
                                                                    join b in idal.ITransferDetailDAL.SelectAll()
                                                                          on new { a.Id }
                                                                      equals new { Id = (Int32)b.TransferHeadId } into b_join
                                                                    from b in b_join.DefaultIfEmpty()
                                                                    where c.Id == transferTask.Id
                                                                    group new { a, b } by new
                                                                    {
                                                                        a.WhCode,
                                                                        a.LoadId,
                                                                        a.SortGroupId,
                                                                        a.SortGroupNumber,
                                                                        a.OutPoNumber,
                                                                        b.ItemId
                                                                    } into g
                                                                    select new TransferHeadDetailResult
                                                                    {
                                                                        WhCode = g.Key.WhCode,
                                                                        LoadId = g.Key.LoadId,
                                                                        SortGroupId = g.Key.SortGroupId,
                                                                        SortGroupNumber = g.Key.SortGroupNumber,
                                                                        OutPoNumber = g.Key.OutPoNumber,
                                                                        ItemId = g.Key.ItemId,
                                                                        SumQty = g.Sum(p => p.b.Qty)
                                                                    }).ToList();
                        if (checkList.Count == 0)
                        {
                            result = "未找到可交接的信息，无法交接！";
                            return result;
                        }

                        //验证交接数量是否超支
                        List<SortTaskDetail> checkSortList = new List<SortTaskDetail>();

                        List<SortTaskDetail> getsortTaskDetailList1 = (from a in checkList
                                                                       join b in idal.ISortTaskDetailDAL.SelectAll()
                                                                       on new { A = a.WhCode, B = a.LoadId } equals new { A = b.WhCode, B = b.LoadId }
                                                                       select b).ToList();

                        foreach (var item in checkList)
                        {
                            List<SortTaskDetail> checksortTaskDetailList = getsortTaskDetailList1.Where(u => u.WhCode == item.WhCode && u.LoadId == item.LoadId && u.GroupId == item.SortGroupId && u.GroupNumber == item.SortGroupNumber && u.OutPoNumber == item.OutPoNumber && u.ItemId == item.ItemId && u.TransferQty < u.PlanQty).ToList();
                            if (checksortTaskDetailList.Count == 0)
                            {
                                result = "没有需要交接的信息！";
                                break;
                            }
                            else
                            {
                                SortTaskDetail so = checksortTaskDetailList.First();
                                int? transferQty = so.TransferQty;
                                if (checkSortList.Where(u => u.Id == so.Id).Count() > 0)
                                {
                                    transferQty = transferQty + checkSortList.Where(u => u.Id == so.Id).First().TransferQty;
                                }

                                if (transferQty + item.SumQty > so.PlanQty)
                                {
                                    result = "交接数量与系统所需数量不符，无法交接！";
                                    break;
                                }
                                else
                                {
                                    if (checkSortList.Where(u => u.Id == so.Id).Count() == 0)
                                    {
                                        SortTaskDetail newsort = new SortTaskDetail();
                                        newsort.Id = so.Id;
                                        newsort.TransferQty = item.SumQty;
                                        checkSortList.Add(newsort);
                                    }
                                    else
                                    {
                                        SortTaskDetail old = checkSortList.Where(u => u.Id == so.Id).First();

                                        SortTaskDetail newsort = new SortTaskDetail();
                                        newsort.Id = so.Id;
                                        newsort.TransferQty = old.TransferQty + item.SumQty;
                                        checkSortList.Add(newsort);
                                        checkSortList.Remove(old);
                                    }
                                }
                            }
                        }
                        if (result + "" != "")
                        {
                            return result;
                        }

                        //得到交接任务头
                        List<TransferHead> getTransferHeadList = (from a in idal.ITransferHeadDAL.SelectAll()
                                                                  where a.TransferTaskId == transferTask.Id
                                                                  select a).ToList();

                        List<TransferDetail> gettransferDetailList1 = (from a in idal.ITransferHeadDAL.SelectAll()
                                                                       join b in idal.ITransferDetailDAL.SelectAll()
                                                                       on a.Id equals b.TransferHeadId
                                                                       where a.TransferTaskId == transferTask.Id
                                                                       select b).ToList();

                        //更新分拣表中的 已交接数量
                        foreach (var transferHead in getTransferHeadList)
                        {
                            if (result != "")
                            {
                                break;
                            }

                            List<TransferDetail> transferDetailList = gettransferDetailList1.Where(u => u.TransferHeadId == transferHead.Id).ToList();

                            foreach (var item in transferDetailList)
                            {
                                List<SortTaskDetail> sortTaskDetailList = idal.ISortTaskDetailDAL.SelectBy(u => u.WhCode == transferHead.WhCode && u.LoadId == transferHead.LoadId && u.GroupId == transferHead.SortGroupId && u.GroupNumber == transferHead.SortGroupNumber && u.OutPoNumber == transferHead.OutPoNumber && u.ItemId == item.ItemId && u.TransferQty < u.PlanQty).ToList();

                                if (sortTaskDetailList.Count == 0)
                                {
                                    result = "查找分拣的交接信息出现异常，无法交接！";
                                    break;
                                }

                                SortTaskDetail sortTaskDetail = sortTaskDetailList.First();

                                if (sortTaskDetail.TransferQty + item.Qty > sortTaskDetail.PlanQty)
                                {
                                    result = "Load：" + sortTaskDetail.LoadId + " SKU:" + sortTaskDetail.AltItemNumber + " 分拣与交接数量不符，无法交接！";
                                    break;
                                }
                                else
                                {
                                    idal.ISortTaskDetailDAL.UpdateByExtended(u => u.Id == sortTaskDetail.Id, t => new SortTaskDetail { TransferQty = sortTaskDetail.TransferQty + item.Qty, UpdateUser = userName, UpdateDate = DateTime.Now });

                                    sortTaskDetail.TransferQty = sortTaskDetail.TransferQty + item.Qty;
                                    sortTaskDetail.UpdateUser = userName;
                                    sortTaskDetail.UpdateDate = DateTime.Now;
                                }
                            }
                        }
                        if (result + "" != "")
                        {
                            return result;
                        }

                        transferTask.Status = 30;
                        transferTask.UpdateUser = userName;
                        transferTask.UpdateDate = DateTime.Now;
                        idal.ITransferTaskDAL.UpdateBy(transferTask, u => u.Id == transferTask.Id, new string[] { "Status", "UpdateUser", "UpdateDate" });

                        //添加交接日志
                        TranLog tl3 = new TranLog();
                        tl3.TranType = "410";
                        tl3.Description = "开始执行交接";
                        tl3.TranDate = DateTime.Now;
                        tl3.TranUser = userName;
                        tl3.WhCode = transferTask.WhCode;
                        tl3.Remark = transferTask.TransferId;

                        idal.ITranLogDAL.Add(tl3);
                        idal.SaveChanges();
                        //tlList.Add(tl3);


                        //交接时 验证 同一托盘多条明细中的第一行是否已扣减库存
                        //如果已扣减，应跳过该行 继续下一行
                        List<HuDetail> checkHuList = new List<HuDetail>();

                        foreach (var item in getTransferHeadList)
                        {
                            List<TransferDetail> transferDetailList = idal.ITransferDetailDAL.SelectBy(u => u.TransferHeadId == item.Id);

                            //根据交接数量 减去对应库存
                            foreach (var TransferDetail in transferDetailList)
                            {
                                int tranQty = TransferDetail.Qty;   //得到交接明细数量

                                //得到库存装箱数量
                                List<HuDetail> huDetailList = idal.IHuDetailDAL.SelectBy(u => u.WhCode == transferTask.WhCode && u.HuId == item.LoadId && u.ItemId == TransferDetail.ItemId);
                                if (huDetailList.Count != 0)
                                {
                                    foreach (var huDetail in huDetailList)
                                    {
                                        //交接明细数量 库存已扣减 循环断开
                                        if (tranQty == 0)
                                        {
                                            break;
                                        }
                                        if (checkHuList.Where(u => u.Id == huDetail.Id).Count() > 0)
                                        {
                                            huDetail.Qty = checkHuList.Where(u => u.Id == huDetail.Id).First().Qty;
                                        }
                                        if (huDetail.Qty < 1)
                                        {
                                            continue;
                                        }

                                        TranLog tl = new TranLog();
                                        tl.TranType = "400";
                                        tl.Description = "交接删除库存";
                                        tl.TranDate = DateTime.Now;
                                        tl.TranUser = userName;
                                        tl.WhCode = transferTask.WhCode;
                                        tl.ClientCode = huDetail.ClientCode;
                                        tl.SoNumber = huDetail.SoNumber;
                                        tl.CustomerPoNumber = huDetail.CustomerPoNumber;
                                        tl.AltItemNumber = huDetail.AltItemNumber;
                                        tl.ItemId = huDetail.ItemId;
                                        tl.UnitID = huDetail.UnitId;
                                        tl.UnitName = huDetail.UnitName;
                                        tl.ReceiptId = huDetail.ReceiptId;
                                        tl.ReceiptDate = huDetail.ReceiptDate;
                                        tl.TranQty = huDetail.Qty;
                                        tl.HuId = huDetail.HuId;
                                        tl.Length = huDetail.Length;
                                        tl.Width = huDetail.Width;
                                        tl.Height = huDetail.Height;
                                        tl.Weight = huDetail.Weight;
                                        tl.LotNumber1 = huDetail.LotNumber1;
                                        tl.LotNumber2 = huDetail.LotNumber2;
                                        tl.LotDate = huDetail.LotDate;
                                        tl.CustomerOutPoNumber = item.CustomerOutPoNumber;
                                        tl.OutPoNumber = item.OutPoNumber;
                                        tl.LoadId = item.LoadId;
                                        tl.Remark = transferTask.TransferId;
                                        //idal.IHuDetailDAL.UpdateBy(huDetail, u => u.Id == huDetail.Id, new string[] { "Qty", "UpdateUser", "UpdateDate" });

                                        if (huDetail.Qty < tranQty)
                                        {
                                            tranQty = tranQty - huDetail.Qty;

                                            idal.IHuDetailDAL.UpdateByExtended(u => u.Id == huDetail.Id, t => new HuDetail { Qty = 0, UpdateUser = userName, UpdateDate = DateTime.Now });

                                            huDetail.Qty = 0;
                                            huDetail.UpdateUser = userName;
                                            huDetail.UpdateDate = DateTime.Now;

                                            tl.TranQty2 = huDetail.Qty;
                                        }
                                        else
                                        {
                                            idal.IHuDetailDAL.UpdateByExtended(u => u.Id == huDetail.Id, t => new HuDetail { Qty = huDetail.Qty - tranQty, UpdateUser = userName, UpdateDate = DateTime.Now });

                                            huDetail.Qty = huDetail.Qty - tranQty;
                                            huDetail.UpdateUser = userName;
                                            huDetail.UpdateDate = DateTime.Now;
                                            tranQty = 0;    //装箱库存已减去交接数量

                                            tl.TranQty2 = tranQty;
                                        }

                                        idal.ITranLogDAL.Add(tl);
                                        idal.SaveChanges();
                                        //tlList.Add(tl);

                                        if (checkHuList.Where(u => u.Id == huDetail.Id).Count() == 0)
                                        {
                                            HuDetail newHu = new HuDetail();
                                            newHu.Id = huDetail.Id;
                                            newHu.Qty = huDetail.Qty;
                                            checkHuList.Add(newHu);
                                        }
                                        else
                                        {
                                            HuDetail oldHu = checkHuList.Where(u => u.Id == huDetail.Id).First();
                                            checkHuList.Remove(oldHu);

                                            HuDetail newHu = new HuDetail();
                                            newHu.Id = huDetail.Id;
                                            newHu.Qty = huDetail.Qty;
                                            checkHuList.Add(newHu);
                                        }

                                    }
                                }
                            }
                        }

                        idal.SaveChanges();

                        //添加交接日志
                        TranLog tl5 = new TranLog();
                        tl5.TranType = "415";
                        tl5.Description = "交接删除分拣包装";
                        tl5.TranDate = DateTime.Now;
                        tl5.TranUser = userName;
                        tl5.WhCode = transferTask.WhCode;
                        tl5.Remark = transferTask.TransferId;

                        idal.ITranLogDAL.Add(tl5);
                        idal.SaveChanges();
                        //tlList.Add(tl5);

                        List<SortTaskDetailResult> SortTaskDetailResult = new List<SortTaskDetailResult>();

                        string[] loadIdList = (from a in getTransferHeadList
                                               select a.LoadId).Distinct().ToArray();
                        //验证Load是否全部交接
                        foreach (var loadId in loadIdList)
                        {
                            List<SortTaskDetail> GetSortTaskDetailList = idal.ISortTaskDetailDAL.SelectBy(u => u.WhCode == transferTask.WhCode && u.LoadId == loadId);

                            //根据Load号 取得  出货流程信息
                            FlowDetail flowDetail = (from a in idal.IFlowDetailDAL.SelectAll()
                                                     join b in idal.ILoadMasterDAL.SelectAll()
                                                     on a.FlowHeadId equals b.ProcessId
                                                     where a.Type == "Shipping" && b.WhCode == transferTask.WhCode && b.LoadId == loadId
                                                     select a).First();

                            //如果有分拣数据
                            if (GetSortTaskDetailList.Count != 0)
                            {
                                //验证分拣数量是否全部分拣
                                if (GetSortTaskDetailList.Where(u => u.HoldFlag == 0).Where(u => u.PlanQty != u.TransferQty).Count() == 0)
                                {
                                    //删除Load指定托盘数据
                                    idal.ILoadHuIdExtendDAL.DeleteByExtended(u => u.LoadId == loadId && u.WhCode == transferTask.WhCode);

                                    //删除备货任务
                                    idal.IPickTaskDetailDAL.DeleteByExtended(u => u.LoadId == loadId && u.WhCode == transferTask.WhCode);
                                }

                                //更新封箱时间         
                                idal.ILoadMasterDAL.UpdateByExtended(u => u.WhCode == transferTask.WhCode && u.LoadId == loadId, t => new LoadMaster { ShipDate = DateTime.Now, UpdateUser = userName, UpdateDate = DateTime.Now });

                                if (GetSortTaskDetailList.Where(u => u.PlanQty != u.PackQty || u.PlanQty != u.TransferQty).Count() == 0)
                                {
                                    //根据Load号全部删除 分拣 包装 库存数据
                                    CompleteTransfer(userName, transferTask.WhCode, loadId, flowDetail, transferTask.TransferId);
                                }
                                else
                                {
                                    //部分交接 删除正常订单的 分拣 包装 库存数据

                                    //1.首先得到包装数量等于计划数量 且交接数量等于计划数量的 Load和出库订单号

                                    List<SortTaskDetail> GetSortTaskDetailListByHoldFlag = GetSortTaskDetailList.Where(u => u.HoldFlag == 0).Where(u => u.PlanQty == u.PackQty && u.PlanQty == u.TransferQty).ToList();
                                    if (GetSortTaskDetailListByHoldFlag.Count() > 0)
                                    {
                                        List<SortTaskDetailResult> SortTaskDetailResult1 = (from a in GetSortTaskDetailListByHoldFlag
                                                                                            group new { a } by new
                                                                                            {
                                                                                                a.WhCode,
                                                                                                a.LoadId,
                                                                                                a.OutPoNumber
                                                                                            } into g
                                                                                            select new SortTaskDetailResult
                                                                                            {
                                                                                                LoadId = g.Key.LoadId,
                                                                                                WhCode = g.Key.WhCode,
                                                                                                OutPoNumber = g.Key.OutPoNumber
                                                                                            }).ToList();

                                        //2.再验证Load下的出库订单是否全部包装
                                        foreach (var item in SortTaskDetailResult1)
                                        {
                                            List<SortTaskDetail> checkSortTaskDetail = idal.ISortTaskDetailDAL.SelectBy(u => u.WhCode == item.WhCode && u.LoadId == item.LoadId && u.OutPoNumber == item.OutPoNumber);
                                            if (checkSortTaskDetail.Where(u => u.PlanQty != u.PackQty || u.PlanQty != u.TransferQty).Count() > 0)
                                            {
                                                continue;
                                                //SortTaskDetailResult.Remove(item);
                                            }
                                            else
                                            {
                                                SortTaskDetailResult.Add(item);
                                            }
                                        }

                                        foreach (var sortTaskDetail in SortTaskDetailResult)
                                        {
                                            OutBoundOrder outOrder = new OutBoundOrder();
                                            outOrder.NowProcessId = flowDetail.FlowRuleId;
                                            outOrder.StatusId = flowDetail.StatusId;
                                            outOrder.StatusName = flowDetail.StatusName;
                                            outOrder.UpdateUser = userName;
                                            outOrder.UpdateDate = DateTime.Now;

                                            //更新订单状态
                                            idal.IOutBoundOrderDAL.UpdateBy(outOrder, u => u.WhCode == sortTaskDetail.WhCode && u.OutPoNumber == sortTaskDetail.OutPoNumber && u.StatusId != -10, new string[] { "NowProcessId", "StatusId", "StatusName", "UpdateUser", "UpdateDate" });


                                            //更新订单状态，插入日志
                                            TranLog tlorder = new TranLog();
                                            tlorder.TranType = "32";
                                            tlorder.Description = "更新订单状态";
                                            tlorder.TranDate = DateTime.Now;
                                            tlorder.TranUser = userName;
                                            tlorder.WhCode = sortTaskDetail.WhCode;
                                            tlorder.LoadId = sortTaskDetail.LoadId;
                                            tlorder.OutPoNumber = sortTaskDetail.OutPoNumber;
                                            tlorder.Remark = "变更为：" + flowDetail.StatusId + flowDetail.StatusName;
                                            idal.ITranLogDAL.Add(tlorder);


                                            //删除包装 ---------------------------------
                                            //得到包装任务
                                            List<PackTask> packTaskList = idal.IPackTaskDAL.SelectBy(u => u.WhCode == sortTaskDetail.WhCode && u.LoadId == sortTaskDetail.LoadId && u.OutPoNumber == sortTaskDetail.OutPoNumber);
                                            foreach (var packTask in packTaskList)
                                            {
                                                //得到包装任务头
                                                List<PackHead> packHeadList = idal.IPackHeadDAL.SelectBy(u => u.PackTaskId == packTask.Id);
                                                foreach (var packHead in packHeadList)
                                                {
                                                    //得到包装明细
                                                    List<PackDetail> packDetailList = idal.IPackDetailDAL.SelectBy(u => u.PackHeadId == packHead.Id);
                                                    foreach (var packDetail in packDetailList)
                                                    {

                                                        //删除明细对应的扫描
                                                        idal.IPackScanNumberDAL.DeleteByExtended(u => u.PackDetailId == packDetail.Id);
                                                        idal.IPackDetailDAL.DeleteByExtended(u => u.Id == packDetail.Id);
                                                    }
                                                    idal.IPackHeadDAL.DeleteByExtended(u => u.Id == packHead.Id);
                                                }
                                                idal.IPackTaskDAL.DeleteByExtended(u => u.Id == packTask.Id);
                                            }

                                            //再删除分拣
                                            idal.ISortTaskDetailDAL.DeleteByExtended(u => u.WhCode == sortTaskDetail.WhCode && u.LoadId == sortTaskDetail.LoadId && u.OutPoNumber == sortTaskDetail.OutPoNumber);


                                            //删除库存
                                            List<HuDetail> checkHuDetailList = idal.IHuDetailDAL.SelectBy(u => u.WhCode == sortTaskDetail.WhCode && u.HuId == sortTaskDetail.LoadId && u.Qty != 0);
                                            if (checkHuDetailList.Count == 0)
                                            {
                                                idal.IHuMasterDAL.DeleteByExtended(u => u.WhCode == sortTaskDetail.WhCode && u.HuId == sortTaskDetail.LoadId);
                                            }
                                            idal.IHuDetailDAL.DeleteByExtended(u => u.WhCode == sortTaskDetail.WhCode && u.HuId == sortTaskDetail.LoadId && u.Qty == 0);

                                        }
                                    }
                                }
                            }
                        }

                        //添加交接日志
                        TranLog tl4 = new TranLog();
                        tl4.TranType = "420";
                        tl4.Description = "完成执行交接";
                        tl4.TranDate = DateTime.Now;
                        tl4.TranUser = userName;
                        tl4.WhCode = transferTask.WhCode;
                        tl4.Remark = transferTask.TransferId;

                        idal.ITranLogDAL.Add(tl4);
                        idal.SaveChanges();
                        //tlList.Add(tl4);

                        // idal.ITranLogDAL.Add(tlList);

                        //插入EDI任务数据
                        EclOutBoundManager eom = new EclOutBoundManager();
                        eom.UrlEdiTaskInsertOut(transferTask.TransferId, transferTask.WhCode, userName);
                        idal.SaveChanges();
                        trans.Complete();
                        return "Y";
                    }
                    catch
                    {
                        trans.Dispose();//出现异常，事务手动释放
                        return "交接异常，请重新提交！";
                    }
                }

            }
        }

        #endregion


        #region 新交接执行方法

        //大批量Load交接方法 当检测到交接数量等于分拣数量时 校验款号成功后 批量删除库存
        //public string BeginTransferTask(int transferId, string userName, int fayunQty)
        //{
        //    List<TransferTask> TransferTaskList = idal.ITransferTaskDAL.SelectBy(u => u.Id == transferId);
        //    if (TransferTaskList.Count == 0)
        //    {
        //        return "未找到交接信息，请查询！";
        //    }

        //    TransferTask transferTask = TransferTaskList.First();
        //    if (transferTask.Status == -10)
        //    {
        //        return "交接任务被拦截，请查询！";
        //    }
        //    if (transferTask.Status == 30)
        //    {
        //        return "当前交接任务已完成交接！";
        //    }

        //    //拦截交接中的快递单
        //    List<TransferHead> TransferHeadList = idal.ITransferHeadDAL.SelectBy(u => u.TransferTaskId == transferTask.Id);

        //    string checkResult = "";
        //    foreach (var item in TransferHeadList.Where(u => u.Status == -10))
        //    {
        //        checkResult += "快递单号：" + item.ExpressNumber + "被拦截！";
        //    }
        //    if (checkResult != "")
        //    {
        //        return checkResult;
        //    }

        //    List<TransferHeadDetailResult> getCount = (from transferhead in
        //                                               (from transferhead in idal.ITransferHeadDAL.SelectAll()
        //                                                where
        //                                                  transferhead.TransferTaskId == transferId
        //                                                select new
        //                                                {
        //                                                    Dummy = "x"
        //                                                })
        //                                               group transferhead by new { transferhead.Dummy } into g
        //                                               select new TransferHeadDetailResult
        //                                               {
        //                                                   SumQty = g.Count()
        //                                               }).ToList();
        //    if (getCount.Count == 0)
        //    {
        //        return "没有交接信息！";
        //    }

        //    if (getCount.First().SumQty != fayunQty)
        //    {
        //        return "发运数量与交接数量不一致！发运数量：" + fayunQty + " 交接数量：" + getCount.First().SumQty;
        //    }

        //    //并发锁
        //    lock (o1)
        //    {
        //        using (TransactionScope trans = new TransactionScope())
        //        {
        //            try
        //            {
        //                List<TranLog> tlList = new List<TranLog>();

        //                //添加交接日志
        //                TranLog tl2 = new TranLog();
        //                tl2.TranType = "405";
        //                tl2.Description = "开始验证交接";
        //                tl2.TranDate = DateTime.Now;
        //                tl2.TranUser = userName;
        //                tl2.WhCode = transferTask.WhCode;
        //                tl2.Remark = transferTask.TransferId;
        //                idal.ITranLogDAL.Add(tl2);
        //                idal.SaveChanges();

        //                int isAllShipFlag = 0;  //交接是否是整出 0否 1是，0非整出时 会循环扣除库存

        //                //1.验证分拣表数量 与 实际交接数量 是否一致

        //                //得到交接明细信息
        //                string result = "";
        //                List<TransferHeadDetailResult> checkList = (from c in idal.ITransferTaskDAL.SelectAll()
        //                                                            join a in idal.ITransferHeadDAL.SelectAll() on new { Id = c.Id } equals new { Id = (Int32)a.TransferTaskId } into a_join
        //                                                            from a in a_join.DefaultIfEmpty()
        //                                                            join b in idal.ITransferDetailDAL.SelectAll()
        //                                                                  on new { a.Id }
        //                                                              equals new { Id = (Int32)b.TransferHeadId } into b_join
        //                                                            from b in b_join.DefaultIfEmpty()
        //                                                            where c.Id == transferTask.Id
        //                                                            group new { a, b } by new
        //                                                            {
        //                                                                a.WhCode,
        //                                                                a.LoadId,
        //                                                                a.SortGroupId,
        //                                                                a.SortGroupNumber,
        //                                                                a.OutPoNumber,
        //                                                                b.ItemId
        //                                                            } into g
        //                                                            select new TransferHeadDetailResult
        //                                                            {
        //                                                                WhCode = g.Key.WhCode,
        //                                                                LoadId = g.Key.LoadId,
        //                                                                SortGroupId = g.Key.SortGroupId,
        //                                                                SortGroupNumber = g.Key.SortGroupNumber,
        //                                                                OutPoNumber = g.Key.OutPoNumber,
        //                                                                ItemId = g.Key.ItemId,
        //                                                                SumQty = g.Sum(p => p.b.Qty)
        //                                                            }).ToList();
        //                if (checkList.Count == 0)
        //                {
        //                    result = "未找到可交接的信息，无法交接！";
        //                    return result;
        //                }

        //                //得到当前交接的Load
        //                string[] getloadIdList = (from a in checkList
        //                                          select a.LoadId).Distinct().ToArray();

        //                //验证集合
        //                List<SortTaskDetail> checkSortList = new List<SortTaskDetail>();

        //                //得到分拣数据
        //                List<SortTaskDetail> getsortTaskDetailList1 = (from b in idal.ISortTaskDetailDAL.SelectAll()
        //                                                               join a in idal.ITransferHeadDAL.SelectAll()
        //                                                               on new { A = b.WhCode, B = b.LoadId } equals new { A = a.WhCode, B = a.LoadId } into temp1
        //                                                               from ab in temp1.DefaultIfEmpty()
        //                                                               where ab.TransferTaskId == transferTask.Id
        //                                                               select b).ToList().Distinct().ToList();

        //                int checkAllShipCount = 0;  //是完整交接的Load数

        //                //循环Load，验证交接的每个Load 是否都是完整的
        //                foreach (var loadid in getloadIdList)
        //                {
        //                    int? checkQty = checkList.Where(u => u.LoadId == loadid).Sum(u => u.SumQty);
        //                    int? checkQty1 = getsortTaskDetailList1.Where(u => u.LoadId == loadid && u.HoldFlag == 0).Sum(u => (u.PlanQty - u.TransferQty));
        //                    if (checkQty > checkQty1)
        //                    {
        //                        string getResult = "";
        //                        foreach (var itemCheck in checkList)
        //                        {
        //                            if (getsortTaskDetailList1.Where(u => u.LoadId == itemCheck.LoadId && u.WhCode == itemCheck.WhCode && u.HoldFlag == 0 && u.ItemId == itemCheck.ItemId && u.OutPoNumber == itemCheck.OutPoNumber && u.GroupId == itemCheck.SortGroupId).Count() == 0)
        //                            {
        //                                getResult = " 系统出库单号:" + itemCheck.OutPoNumber + " 分拣组号:" + itemCheck.SortGroupId + " 款号ID:" + itemCheck.ItemId + "异常!";
        //                                break;
        //                            }
        //                        }
        //                        result = "Load:" + loadid + "交接总数大于系统所需总数！系统所需总数：" + checkQty1 + " 交接总数：" + checkQty + getResult;
        //                        break;
        //                    }
        //                    else
        //                    {
        //                        int? trqty = checkList.Where(u => u.LoadId == loadid).Sum(u => u.SumQty);
        //                        int? trqty1 = getsortTaskDetailList1.Where(u => u.LoadId == loadid).Sum(u => u.PlanQty);

        //                        //如果验证时 交接数量等于分拣计划数量
        //                        if (trqty == trqty1)
        //                        {
        //                            checkAllShipCount++;
        //                        }

        //                        foreach (var item in checkList.Where(u => u.LoadId == loadid))
        //                        {
        //                            List<SortTaskDetail> checksortTaskDetailList = getsortTaskDetailList1.Where(u => u.WhCode == item.WhCode && u.LoadId == item.LoadId && u.GroupId == item.SortGroupId && u.GroupNumber == item.SortGroupNumber && u.OutPoNumber == item.OutPoNumber && u.ItemId == item.ItemId && u.TransferQty < u.PlanQty).ToList();
        //                            if (checksortTaskDetailList.Count == 0)
        //                            {
        //                                result = "Load:" + loadid + "未找到分拣明细信息！";
        //                                break;
        //                            }
        //                            else
        //                            {
        //                                SortTaskDetail so = checksortTaskDetailList.First();
        //                                int? transferQty = so.TransferQty;
        //                                if (checkSortList.Where(u => u.Id == so.Id).Count() > 0)
        //                                {
        //                                    transferQty = transferQty + checkSortList.Where(u => u.Id == so.Id).First().TransferQty;
        //                                }

        //                                if (transferQty + item.SumQty > so.PlanQty)
        //                                {
        //                                    result = "交接数量大于系统所需数量,无法交接！Load:" + item.LoadId + " 系统出库单号:" + item.OutPoNumber + " 分拣组号:" + item.SortGroupId + " 款号:" + item.AltItemNumber + " 系统所需数量：" + so.PlanQty + " 交接数量:" + transferQty + item.SumQty;
        //                                    break;
        //                                }
        //                                else
        //                                {
        //                                    if (checkSortList.Where(u => u.Id == so.Id).Count() == 0)
        //                                    {
        //                                        SortTaskDetail newsort = new SortTaskDetail();
        //                                        newsort.Id = so.Id;
        //                                        newsort.TransferQty = item.SumQty;
        //                                        checkSortList.Add(newsort);
        //                                    }
        //                                    else
        //                                    {
        //                                        SortTaskDetail old = checkSortList.Where(u => u.Id == so.Id).First();

        //                                        SortTaskDetail newsort = new SortTaskDetail();
        //                                        newsort.Id = so.Id;
        //                                        newsort.TransferQty = old.TransferQty + item.SumQty;
        //                                        checkSortList.Add(newsort);
        //                                        checkSortList.Remove(old);
        //                                    }
        //                                }
        //                            }
        //                        }
        //                    }
        //                }

        //                if (result + "" != "")
        //                {
        //                    return result;
        //                }

        //                //添加交接日志
        //                TranLog tl1 = new TranLog();
        //                tl1.TranType = "406";
        //                tl1.Description = "完成验证交接";
        //                tl1.TranDate = DateTime.Now;
        //                tl1.TranUser = userName;
        //                tl1.WhCode = transferTask.WhCode;
        //                tl1.Remark = transferTask.TransferId;
        //                idal.ITranLogDAL.Add(tl1);
        //                idal.SaveChanges();

        //                //验证交接数量正确后，比较 完整Load数 是否等于 所选交接的Load数
        //                if (checkAllShipCount == getloadIdList.Count())
        //                {
        //                    isAllShipFlag = 1;  //变更为 整出，不验证库存
        //                }

        //                transferTask.Status = 30;
        //                transferTask.UpdateUser = userName;
        //                transferTask.UpdateDate = DateTime.Now;
        //                idal.ITransferTaskDAL.UpdateBy(transferTask, u => u.Id == transferTask.Id, new string[] { "Status", "UpdateUser", "UpdateDate" });

        //                //添加交接日志
        //                TranLog tl3 = new TranLog();
        //                tl3.TranType = "410";
        //                tl3.Description = "开始执行交接";
        //                tl3.TranDate = DateTime.Now;
        //                tl3.TranUser = userName;
        //                tl3.WhCode = transferTask.WhCode;
        //                tl3.Remark = transferTask.TransferId;
        //                idal.ITranLogDAL.Add(tl3);
        //                idal.SaveChanges();

        //                //一次性拉取所有Load的出货流程 Id状态明细
        //                List<FlowDetailResult> getflowDetailList = (from a in idal.IFlowDetailDAL.SelectAll()
        //                                                            join b in idal.ILoadMasterDAL.SelectAll()
        //                                                            on a.FlowHeadId equals b.ProcessId
        //                                                            where a.Type == "Shipping" && b.WhCode == transferTask.WhCode && getloadIdList.Contains(b.LoadId)
        //                                                            select new FlowDetailResult
        //                                                            {
        //                                                                FlowRuleId = a.FlowRuleId,
        //                                                                StatusId = a.StatusId,
        //                                                                StatusName = a.StatusName,
        //                                                                LoadId = b.LoadId
        //                                                            }).ToList().Distinct().ToList();

        //                //整出 删除交接Load的库存
        //                if (isAllShipFlag == 1)
        //                {
        //                    foreach (var loadId in getloadIdList)
        //                    {
        //                        //删除Load指定托盘数据
        //                        idal.ILoadHuIdExtendDAL.DeleteByExtended(u => u.LoadId == loadId && u.WhCode == transferTask.WhCode);

        //                        //删除备货任务
        //                        idal.IPickTaskDetailDAL.DeleteByExtended(u => u.LoadId == loadId && u.WhCode == transferTask.WhCode);

        //                        //1.根据Load号 取得  出货流程信息
        //                        FlowDetailResult flowDetailResult = getflowDetailList.Where(u => u.LoadId == loadId).First();

        //                        FlowDetail flowDetail = new FlowDetail();
        //                        flowDetail.FlowRuleId = Convert.ToInt32(flowDetailResult.FlowRuleId);
        //                        flowDetail.StatusId = flowDetailResult.StatusId;
        //                        flowDetail.StatusName = flowDetailResult.StatusName;

        //                        //根据Load号全部删除 分拣 包装 库存数据
        //                        CompleteTransfer(userName, transferTask.WhCode, loadId, flowDetail, transferTask.TransferId);

        //                        //更新封箱时间         
        //                        idal.ILoadMasterDAL.UpdateByExtended(u => u.WhCode == transferTask.WhCode && u.LoadId == loadId, t => new LoadMaster { ShipDate = DateTime.Now, UpdateUser = userName, UpdateDate = DateTime.Now });
        //                    }
        //                }
        //                else
        //                {
        //                    //部分交接
        //                    int[] SortTaskDetailId = (from a in checkSortList
        //                                              select a.Id).ToList().Distinct().ToArray();

        //                    //1.更新分拣表中的交接数量
        //                    List<SortTaskDetail> getSortTaskDetailList = (from b in idal.ISortTaskDetailDAL.SelectAll()
        //                                                                  where SortTaskDetailId.Contains(b.Id)
        //                                                                  select b).ToList();

        //                    foreach (var sortTaskDetail in checkSortList)
        //                    {
        //                        SortTaskDetail sortTaskDetail1 = getSortTaskDetailList.Where(u => u.Id == sortTaskDetail.Id).First();

        //                        idal.ISortTaskDetailDAL.UpdateByExtended(u => u.Id == sortTaskDetail.Id, t => new SortTaskDetail { TransferQty = sortTaskDetail.TransferQty + sortTaskDetail1.TransferQty, UpdateUser = userName, UpdateDate = DateTime.Now });

        //                        sortTaskDetail1.TransferQty = sortTaskDetail.TransferQty + sortTaskDetail1.TransferQty;
        //                        sortTaskDetail1.UpdateUser = userName;
        //                        sortTaskDetail1.UpdateDate = DateTime.Now;
        //                    }

        //                    //2.删除部分库存

        //                    //得到交接任务头
        //                    List<TransferHead> getTransferHeadList = (from a in idal.ITransferHeadDAL.SelectAll()
        //                                                              where a.TransferTaskId == transferTask.Id
        //                                                              select a).ToList().Distinct().ToList();

        //                    List<TransferDetail> gettransferDetailList1 = (from a in idal.ITransferHeadDAL.SelectAll()
        //                                                                   join b in idal.ITransferDetailDAL.SelectAll()
        //                                                                   on a.Id equals b.TransferHeadId
        //                                                                   where a.TransferTaskId == transferTask.Id
        //                                                                   select b).ToList().Distinct().ToList();

        //                    //一次性拉取库存信息
        //                    List<HuDetail> getHuDetailList = (from a in idal.IHuDetailDAL.SelectAll()
        //                                                      join b in idal.ITransferHeadDAL.SelectAll()
        //                                                           on new { a.WhCode, a.HuId }
        //                                                       equals new { b.WhCode, HuId = b.LoadId }
        //                                                      where b.TransferTaskId == transferTask.Id
        //                                                      select a).ToList().Distinct().ToList();

        //                    List<TransferDetail> checkTransferDetailChongfu = new List<TransferDetail>();

        //                    //交接时 验证 同一托盘多条明细中的第一行是否已扣减库存
        //                    //如果已扣减，应跳过该行 继续下一行
        //                    List<HuDetail> checkHuList = new List<HuDetail>();

        //                    #region 扣除库存方法 较难理解
        //                    foreach (var item in getTransferHeadList)
        //                    {
        //                        List<TransferDetail> transferDetailList = gettransferDetailList1.Where(u => u.TransferHeadId == item.Id).ToList();

        //                        //根据交接数量 减去对应库存
        //                        foreach (var TransferDetail in transferDetailList)
        //                        {
        //                            //如果 交接款号明细ID已经执行过一次删除库存，就跳过
        //                            if (checkTransferDetailChongfu.Where(u => u.Id == TransferDetail.Id).Count() > 0)
        //                            {
        //                                continue;
        //                            }
        //                            else
        //                            {
        //                                //第一次 加入交接款号明细对象
        //                                checkTransferDetailChongfu.Add(TransferDetail);
        //                            }

        //                            int tranQty = TransferDetail.Qty;   //得到交接明细数量

        //                            //得到库存装箱数量
        //                            List<HuDetail> huDetailList = getHuDetailList.Where(u => u.WhCode == transferTask.WhCode && u.HuId == item.LoadId && u.ItemId == TransferDetail.ItemId).ToList();
        //                            if (huDetailList.Count != 0)
        //                            {
        //                                foreach (var huDetail in huDetailList)
        //                                {
        //                                    //交接明细数量 库存已扣减 循环断开
        //                                    if (tranQty == 0)
        //                                    {
        //                                        break;
        //                                    }
        //                                    if (checkHuList.Where(u => u.Id == huDetail.Id).Count() > 0)
        //                                    {
        //                                        huDetail.Qty = checkHuList.Where(u => u.Id == huDetail.Id).First().Qty;
        //                                    }
        //                                    if (huDetail.Qty < 1)
        //                                    {
        //                                        continue;
        //                                    }

        //                                    TranLog tl = new TranLog();
        //                                    tl.TranType = "400";
        //                                    tl.Description = "部分交接删除库存";
        //                                    tl.TranDate = DateTime.Now;
        //                                    tl.TranUser = userName;
        //                                    tl.WhCode = transferTask.WhCode;
        //                                    tl.ClientCode = huDetail.ClientCode;
        //                                    tl.SoNumber = huDetail.SoNumber;
        //                                    tl.CustomerPoNumber = huDetail.CustomerPoNumber;
        //                                    tl.AltItemNumber = huDetail.AltItemNumber;
        //                                    tl.ItemId = huDetail.ItemId;
        //                                    tl.UnitID = huDetail.UnitId;
        //                                    tl.UnitName = huDetail.UnitName;
        //                                    tl.ReceiptId = huDetail.ReceiptId;
        //                                    tl.ReceiptDate = huDetail.ReceiptDate;
        //                                    tl.TranQty = huDetail.Qty;
        //                                    tl.HuId = huDetail.HuId;
        //                                    tl.Length = huDetail.Length;
        //                                    tl.Width = huDetail.Width;
        //                                    tl.Height = huDetail.Height;
        //                                    tl.Weight = huDetail.Weight;
        //                                    tl.LotNumber1 = huDetail.LotNumber1;
        //                                    tl.LotNumber2 = huDetail.LotNumber2;
        //                                    tl.LotDate = huDetail.LotDate;
        //                                    tl.CustomerOutPoNumber = item.CustomerOutPoNumber;
        //                                    tl.OutPoNumber = item.OutPoNumber;
        //                                    tl.LoadId = item.LoadId;
        //                                    tl.Remark = transferTask.TransferId;

        //                                    //交接数量 大于库存托盘的数量时
        //                                    //需要把当前托盘库存扣除为0 然后继续扣除下一个托盘
        //                                    if (huDetail.Qty < tranQty)
        //                                    {
        //                                        tl.TranQty2 = huDetail.Qty;

        //                                        tranQty = tranQty - huDetail.Qty;
        //                                        huDetail.Qty = 0;
        //                                    }
        //                                    else
        //                                    {
        //                                        tl.TranQty2 = tranQty;

        //                                        huDetail.Qty = huDetail.Qty - tranQty;
        //                                        tranQty = 0;    //交接数量已扣减   
        //                                    }

        //                                    //记录下当前托盘 已扣除过的数量 不能在下个交接明细中继续扣除
        //                                    //如：PLTA00001 SKU1 数量1 在交接1中被扣除了，但 交接2循环进来就不能再扣减它了
        //                                    if (checkHuList.Where(u => u.Id == huDetail.Id).Count() == 0)
        //                                    {
        //                                        HuDetail newHu = new HuDetail();
        //                                        newHu.Id = huDetail.Id;
        //                                        newHu.Qty = huDetail.Qty;
        //                                        checkHuList.Add(newHu);
        //                                    }
        //                                    else
        //                                    {
        //                                        HuDetail oldHu = checkHuList.Where(u => u.Id == huDetail.Id).First();
        //                                        checkHuList.Remove(oldHu);

        //                                        HuDetail newHu = new HuDetail();
        //                                        newHu.Id = huDetail.Id;
        //                                        newHu.Qty = huDetail.Qty;
        //                                        checkHuList.Add(newHu);
        //                                    }

        //                                    //记录交易日志
        //                                    tlList.Add(tl);
        //                                }
        //                            }
        //                        }
        //                    }
        //                    #endregion

        //                    //return "Y";

        //                    idal.ITranLogDAL.Add(tlList);
        //                    idal.SaveChanges();

        //                    //一次性删除库存为0的数据
        //                    int[] delHuDetailId = (from a in checkHuList
        //                                           where a.Qty == 0
        //                                           select a.Id).ToArray();

        //                    if (delHuDetailId.Count() > 0)
        //                    {
        //                        idal.IHuDetailDAL.DeleteByExtended(u => delHuDetailId.Contains(u.Id));
        //                    }

        //                    //执行调整库存
        //                    foreach (var item in checkHuList.Where(u => u.Qty > 0))
        //                    {
        //                        idal.IHuDetailDAL.UpdateByExtended(u => u.Id == item.Id, t => new HuDetail { Qty = item.Qty, UpdateUser = userName, UpdateDate = DateTime.Now });
        //                    }

        //                    //添加交接日志
        //                    TranLog tl5 = new TranLog();
        //                    tl5.TranType = "415";
        //                    tl5.Description = "部分交接删除分拣包装信息";
        //                    tl5.TranDate = DateTime.Now;
        //                    tl5.TranUser = userName;
        //                    tl5.WhCode = transferTask.WhCode;
        //                    tl5.Remark = transferTask.TransferId;
        //                    idal.ITranLogDAL.Add(tl5);

        //                    //3.部分交接 删除正常订单的 分拣 包装数据

        //                    List<SortTaskDetailResult> SortTaskDetailResultList = new List<SortTaskDetailResult>();

        //                    //拉取分拣明细
        //                    List<SortTaskDetail> GetSortTaskDetailListCheck = idal.ISortTaskDetailDAL.SelectBy(u => u.WhCode == transferTask.WhCode && getloadIdList.Contains(u.LoadId));

        //                    //拉取分拣Load\订单
        //                    List<SortTaskDetailResult> getLoadOutPoNumberList = (from a in idal.ISortTaskDetailDAL.SelectAll()
        //                                                                         where a.WhCode == transferTask.WhCode && getloadIdList.Contains(a.LoadId) && a.HoldFlag == 0 && a.PlanQty == a.PackQty && a.PlanQty == a.TransferQty
        //                                                                         group new { a } by new
        //                                                                         {
        //                                                                             a.WhCode,
        //                                                                             a.LoadId,
        //                                                                             a.OutPoNumber
        //                                                                         } into g
        //                                                                         select new SortTaskDetailResult
        //                                                                         {
        //                                                                             LoadId = g.Key.LoadId,
        //                                                                             WhCode = g.Key.WhCode,
        //                                                                             OutPoNumber = g.Key.OutPoNumber
        //                                                                         }).ToList();


        //                    //拉取待删除Load的所有包装明细信息
        //                    List<PackTaskDeleteResult> getPackALlDetail = (from a in idal.IPackTaskDAL.SelectAll()
        //                                                                   join b in idal.IPackHeadDAL.SelectAll() on new { Id = a.Id } equals new { Id = (Int32)b.PackTaskId } into b_join
        //                                                                   from b in b_join.DefaultIfEmpty()
        //                                                                   join c in idal.IPackDetailDAL.SelectAll() on new { Id = b.Id } equals new { Id = (Int32)c.PackHeadId } into c_join
        //                                                                   from c in c_join.DefaultIfEmpty()
        //                                                                   where getloadIdList.Contains(a.LoadId) && a.WhCode == transferTask.WhCode
        //                                                                   select new PackTaskDeleteResult
        //                                                                   {
        //                                                                       WhCode = a.WhCode,
        //                                                                       LoadId = a.LoadId,
        //                                                                       OutPoNumber = a.OutPoNumber,
        //                                                                       PackTaskId = (Int32?)a.Id,
        //                                                                       PackHeadId = (Int32?)b.Id,
        //                                                                       PackDetailId = (Int32?)c.Id
        //                                                                   }).ToList();

        //                    List<TranLog> trList = new List<TranLog>();
        //                    foreach (var loadId in getloadIdList)
        //                    {
        //                        //1.根据Load号 取得  出货流程信息
        //                        FlowDetailResult flowDetailResult = getflowDetailList.Where(u => u.LoadId == loadId).First();

        //                        FlowDetail flowDetail = new FlowDetail();
        //                        flowDetail.FlowRuleId = Convert.ToInt32(flowDetailResult.FlowRuleId);
        //                        flowDetail.StatusId = flowDetailResult.StatusId;
        //                        flowDetail.StatusName = flowDetailResult.StatusName;

        //                        List<SortTaskDetailResult> SortTaskDetailResult1 = getLoadOutPoNumberList.Where(u => u.LoadId == loadId).ToList();

        //                        //2.验证Load下的出库订单是否是全部包装了
        //                        //如果 快递单交接了部分款号，是不能清除分拣表的
        //                        foreach (var item in SortTaskDetailResult1)
        //                        {
        //                            List<SortTaskDetail> checkSortTaskDetail = GetSortTaskDetailListCheck.Where(u => u.WhCode == item.WhCode && u.LoadId == item.LoadId && u.OutPoNumber == item.OutPoNumber).ToList();
        //                            if (checkSortTaskDetail.Where(u => u.PlanQty != u.PackQty || u.PlanQty != u.TransferQty).Count() > 0)
        //                            {
        //                                continue;
        //                            }
        //                            else
        //                            {
        //                                SortTaskDetailResultList.Add(item);
        //                            }
        //                        }

        //                        if (SortTaskDetailResultList.Count > 0)
        //                        {
        //                            string[] OutPoNumberarr = (from a in SortTaskDetailResultList select a.OutPoNumber).ToList().Distinct().ToArray();

        //                            //更新订单状态
        //                            idal.IOutBoundOrderDAL.UpdateByExtended(u => u.WhCode == transferTask.WhCode && OutPoNumberarr.Contains(u.OutPoNumber), t => new OutBoundOrder { NowProcessId = flowDetail.FlowRuleId, StatusId = flowDetail.StatusId, StatusName = flowDetail.StatusName, UpdateUser = userName, UpdateDate = DateTime.Now });

        //                            foreach (var sortTaskDetail in SortTaskDetailResultList)
        //                            {
        //                                //更新订单状态，插入日志
        //                                TranLog tlorder = new TranLog();
        //                                tlorder.TranType = "32";
        //                                tlorder.Description = "部分交接更新订单状态";
        //                                tlorder.TranDate = DateTime.Now;
        //                                tlorder.TranUser = userName;
        //                                tlorder.WhCode = sortTaskDetail.WhCode;
        //                                tlorder.LoadId = sortTaskDetail.LoadId;
        //                                tlorder.OutPoNumber = sortTaskDetail.OutPoNumber;
        //                                tlorder.Remark = "变更为：" + flowDetail.StatusId + flowDetail.StatusName;
        //                                trList.Add(tlorder);
        //                            }

        //                            //删除包装 ---------------------------------

        //                            //得到包装任务
        //                            List<PackTaskDeleteResult> packTaskList = getPackALlDetail.Where(u => u.WhCode == transferTask.WhCode && u.LoadId == loadId && OutPoNumberarr.Contains(u.OutPoNumber)).ToList();

        //                            int?[] packTaskId = (from a in packTaskList select a.PackTaskId).ToList().Distinct().ToArray();

        //                            int?[] packHeadId = (from a in packTaskList select a.PackHeadId).ToList().Distinct().ToArray();

        //                            int?[] packDetaild = (from a in packTaskList select a.PackDetailId).ToList().Distinct().ToArray();

        //                            if (packDetaild.Count() > 0)
        //                            {
        //                                //删除明细对应的扫描
        //                                idal.IPackScanNumberDAL.DeleteByExtended(u => packDetaild.Contains(u.PackDetailId));

        //                                //删除包装明细
        //                                idal.IPackDetailDAL.DeleteByExtended(u => packDetaild.Contains(u.Id));
        //                            }

        //                            if (packHeadId.Count() > 0)
        //                            {
        //                                //删除包装头
        //                                idal.IPackHeadDAL.DeleteByExtended(u => packHeadId.Contains(u.Id));
        //                            }

        //                            if (packTaskId.Count() > 0)
        //                            {
        //                                //删除包装任务
        //                                idal.IPackTaskDAL.DeleteByExtended(u => packTaskId.Contains(u.Id));
        //                            }

        //                            //再删除分拣
        //                            idal.ISortTaskDetailDAL.DeleteByExtended(u => u.WhCode == transferTask.WhCode && u.LoadId == loadId && OutPoNumberarr.Contains(u.OutPoNumber));
        //                        }
        //                    }

        //                    idal.ITranLogDAL.Add(trList);

        //                    ////添加交接日志
        //                    //TranLog tl6 = new TranLog();
        //                    //tl6.TranType = "416";
        //                    //tl6.Description = "完成部分交接删除分拣包装信息";
        //                    //tl6.TranDate = DateTime.Now;
        //                    //tl6.TranUser = userName;
        //                    //tl6.WhCode = transferTask.WhCode;
        //                    //tl6.Remark = transferTask.TransferId;
        //                    //idal.ITranLogDAL.Add(tl6);

        //                    idal.SaveChanges();

        //                    //再次检查库存，并更新封箱时间
        //                    List<HuDetail> getNewHuDetailList = idal.IHuDetailDAL.SelectBy(u => u.WhCode == transferTask.WhCode && getloadIdList.Contains(u.HuId));

        //                    List<SortTaskDetail> GetSortTaskDetailListCheck1 = idal.ISortTaskDetailDAL.SelectBy(u => u.WhCode == transferTask.WhCode && getloadIdList.Contains(u.LoadId));

        //                    //最后 验证Load是否还有数据，如果没有 清除备货任务表
        //                    foreach (var loadId in getloadIdList)
        //                    {
        //                        List<HuDetail> checkHuDetailList = getNewHuDetailList.Where(u => u.WhCode == transferTask.WhCode && u.HuId == loadId && u.Qty != 0).ToList();
        //                        if (checkHuDetailList.Count == 0)
        //                        {
        //                            idal.IHuMasterDAL.DeleteByExtended(u => u.WhCode == transferTask.WhCode && u.HuId == loadId);
        //                        }

        //                        //更新封箱时间         
        //                        idal.ILoadMasterDAL.UpdateByExtended(u => u.WhCode == transferTask.WhCode && u.LoadId == loadId, t => new LoadMaster { ShipDate = DateTime.Now, UpdateUser = userName, UpdateDate = DateTime.Now });

        //                        if (GetSortTaskDetailListCheck1.Where(u => u.LoadId == loadId).Count() == 0)
        //                        {
        //                            //删除分拣任务
        //                            idal.ISortTaskDAL.DeleteByExtended(u => u.WhCode == transferTask.WhCode && u.LoadId == loadId);

        //                            //删除Load指定托盘数据
        //                            idal.ILoadHuIdExtendDAL.DeleteByExtended(u => u.LoadId == loadId && u.WhCode == transferTask.WhCode);

        //                            //删除备货任务数据
        //                            idal.IPickTaskDetailDAL.DeleteByExtended(u => u.LoadId == loadId && u.WhCode == transferTask.WhCode);
        //                        }
        //                    }
        //                }

        //                //添加交接日志
        //                TranLog tl4 = new TranLog();
        //                tl4.TranType = "420";
        //                tl4.Description = "完成执行交接";
        //                tl4.TranDate = DateTime.Now;
        //                tl4.TranUser = userName;
        //                tl4.WhCode = transferTask.WhCode;
        //                tl4.Remark = transferTask.TransferId;

        //                idal.ITranLogDAL.Add(tl4);
        //                idal.SaveChanges();

        //                //插入EDI任务数据
        //                EclOutBoundManager eom = new EclOutBoundManager();
        //                eom.UrlEdiTaskInsertOut(transferTask.TransferId, transferTask.WhCode, userName);
        //                idal.SaveChanges();


        //                trans.Complete();
        //                return "Y";
        //            }
        //            catch (Exception e)
        //            {
        //                string s = e.InnerException.Message;
        //                trans.Dispose();//出现异常，事务手动释放
        //                return "交接异常，请重新提交！";
        //            }
        //        }

        //    }
        //}

        #endregion

        #region 新交接执行方法

        //大批量Load交接方法 当检测到交接数量等于分拣数量时 校验款号成功后 批量删除库存
        public string BeginTransferTask(int transferId, string userName, int fayunQty)
        {


            //if (Monitor.TryEnter(o1, 1000))
            //{
            //    Thread.Sleep(10000);
            //    Monitor.Exit(o1);
            //    return "交接完成,请稍后！";
            //}
            //else
            //{

            //    return "有其他交接正在进行交接,请稍后！";
            //}


            List<TransferTask> TransferTaskList = idal.ITransferTaskDAL.SelectBy(u => u.Id == transferId);
            if (TransferTaskList.Count == 0)
            {
                return "未找到交接信息，请查询！";
            }

            TransferTask transferTask = TransferTaskList.First();
            if (transferTask.Status == -10)
            {
                return "交接任务被拦截，请查询！";
            }
            if (transferTask.Status == 30)
            {
                return "当前交接任务已完成交接！";
            }

            //拦截交接中的快递单
            List<TransferHead> TransferHeadList = idal.ITransferHeadDAL.SelectBy(u => u.TransferTaskId == transferTask.Id);

            string checkResult = "";
            foreach (var item in TransferHeadList.Where(u => u.Status == -10))
            {
                checkResult += "快递单号：" + item.ExpressNumber + "被拦截！";
            }
            if (checkResult != "")
            {
                return checkResult;
            }

            List<TransferHeadDetailResult> getCount = (from transferhead in
                                                       (from transferhead in idal.ITransferHeadDAL.SelectAll()
                                                        where
                                                          transferhead.TransferTaskId == transferId
                                                        select new
                                                        {
                                                            Dummy = "x"
                                                        })
                                                       group transferhead by new { transferhead.Dummy } into g
                                                       select new TransferHeadDetailResult
                                                       {
                                                           SumQty = g.Count()
                                                       }).ToList();
            if (getCount.Count == 0)
            {
                return "没有交接信息！";
            }

            if (getCount.First().SumQty != fayunQty)
            {
                return "发运数量与交接数量不一致！发运数量：" + fayunQty + " 交接数量：" + getCount.First().SumQty;
            }


            //并发锁
            lock (o1)
            //   if (Monitor.TryEnter(o1, 1000))
            {
                using (TransactionScope trans = new TransactionScope())
                {
                    try
                    {
                        List<TranLog> tlList = new List<TranLog>();

                        //添加交接日志
                        //TranLog tl2 = new TranLog();
                        //tl2.TranType = "405";
                        //tl2.Description = "开始验证交接";
                        //tl2.TranDate = DateTime.Now;
                        //tl2.TranUser = userName;
                        //tl2.WhCode = transferTask.WhCode;
                        //tl2.Remark = transferTask.TransferId;
                        //idal.ITranLogDAL.Add(tl2);
                        // idal.SaveChanges();

                        int isAllShipFlag = 0;  //交接是否是整出 0否 1是，0非整出时 会循环扣除库存

                        //1.验证分拣表数量 与 实际交接数量 是否一致

                        //得到交接明细信息
                        string result = "";
                        List<TransferHeadDetailResult> checkList = (from c in idal.ITransferTaskDAL.SelectAll()
                                                                    join a in idal.ITransferHeadDAL.SelectAll() on new { Id = c.Id } equals new { Id = (Int32)a.TransferTaskId } into a_join
                                                                    from a in a_join.DefaultIfEmpty()
                                                                    join b in idal.ITransferDetailDAL.SelectAll()
                                                                          on new { a.Id }
                                                                      equals new { Id = (Int32)b.TransferHeadId } into b_join
                                                                    from b in b_join.DefaultIfEmpty()
                                                                    where c.Id == transferTask.Id
                                                                    group new { a, b } by new
                                                                    {
                                                                        a.WhCode,
                                                                        a.LoadId,
                                                                        a.SortGroupId,
                                                                        a.SortGroupNumber,
                                                                        a.OutPoNumber,
                                                                        b.ItemId
                                                                    } into g
                                                                    select new TransferHeadDetailResult
                                                                    {
                                                                        WhCode = g.Key.WhCode,
                                                                        LoadId = g.Key.LoadId,
                                                                        SortGroupId = g.Key.SortGroupId,
                                                                        SortGroupNumber = g.Key.SortGroupNumber,
                                                                        OutPoNumber = g.Key.OutPoNumber,
                                                                        ItemId = g.Key.ItemId,
                                                                        SumQty = g.Sum(p => p.b.Qty)
                                                                    }).ToList();
                        if (checkList.Count == 0)
                        {
                            result = "未找到可交接的信息，无法交接！";
                            return result;
                        }

                        //得到当前交接的Load
                        string[] getloadIdList = (from a in checkList
                                                  select a.LoadId).Distinct().ToArray();

                        //验证集合
                        List<SortTaskDetail> checkSortList = new List<SortTaskDetail>();

                        int checkAllShipCount = 0;  //是完整交接的Load数

                        //循环Load，验证交接的每个Load 是否都是完整的
                        foreach (var loadid in getloadIdList)
                        {
                            int? checkQty = checkList.Where(u => u.LoadId == loadid).Sum(u => u.SumQty);

                            //得到分拣数据
                            List<SortTaskDetail> getsortTaskDetailList1 = idal.ISortTaskDetailDAL.SelectBy(u => u.WhCode == transferTask.WhCode && u.LoadId == loadid);

                            int? checkQty1 = getsortTaskDetailList1.Where(u => u.LoadId == loadid && u.HoldFlag == 0).Sum(u => (u.PlanQty - u.TransferQty));
                            if (checkQty > checkQty1)
                            {
                                string getResult = "";
                                foreach (var itemCheck in checkList)
                                {
                                    if (getsortTaskDetailList1.Where(u => u.LoadId == itemCheck.LoadId && u.WhCode == itemCheck.WhCode && u.HoldFlag == 0 && u.ItemId == itemCheck.ItemId && u.OutPoNumber == itemCheck.OutPoNumber && u.GroupId == itemCheck.SortGroupId).Count() == 0)
                                    {
                                        getResult = " 系统出库单号:" + itemCheck.OutPoNumber + " 分拣组号:" + itemCheck.SortGroupId + " 款号ID:" + itemCheck.ItemId + "异常!";
                                        break;
                                    }
                                }
                                result = "Load:" + loadid + "交接总数大于系统所需总数！系统所需总数：" + checkQty1 + " 交接总数：" + checkQty + getResult;
                                break;
                            }
                            else
                            {
                                int? trqty = checkList.Where(u => u.LoadId == loadid).Sum(u => u.SumQty);
                                int? trqty1 = getsortTaskDetailList1.Where(u => u.LoadId == loadid).Sum(u => u.PlanQty);

                                //如果验证时 交接数量等于分拣计划数量
                                if (trqty == trqty1)
                                {
                                    checkAllShipCount++;
                                }

                                foreach (var item in checkList.Where(u => u.LoadId == loadid))
                                {
                                    List<SortTaskDetail> checksortTaskDetailList = getsortTaskDetailList1.Where(u => u.WhCode == item.WhCode && u.LoadId == item.LoadId && u.GroupId == item.SortGroupId && u.GroupNumber == item.SortGroupNumber && u.OutPoNumber == item.OutPoNumber && u.ItemId == item.ItemId && u.TransferQty < u.PlanQty).ToList();
                                    if (checksortTaskDetailList.Count == 0)
                                    {
                                        result = "Load:" + loadid + "未找到分拣明细信息！";
                                        break;
                                    }
                                    else
                                    {
                                        SortTaskDetail so = checksortTaskDetailList.First();
                                        int? transferQty = so.TransferQty;
                                        if (checkSortList.Where(u => u.Id == so.Id).Count() > 0)
                                        {
                                            transferQty = transferQty + checkSortList.Where(u => u.Id == so.Id).First().TransferQty;
                                        }

                                        if (transferQty + item.SumQty > so.PlanQty)
                                        {
                                            result = "交接数量大于系统所需数量,无法交接！Load:" + item.LoadId + " 系统出库单号:" + item.OutPoNumber + " 分拣组号:" + item.SortGroupId + " 款号:" + item.AltItemNumber + " 系统所需数量：" + so.PlanQty + " 交接数量:" + transferQty + item.SumQty;
                                            break;
                                        }
                                        else
                                        {
                                            if (checkSortList.Where(u => u.Id == so.Id).Count() == 0)
                                            {
                                                SortTaskDetail newsort = new SortTaskDetail();
                                                newsort.Id = so.Id;
                                                newsort.TransferQty = item.SumQty;
                                                checkSortList.Add(newsort);
                                            }
                                            else
                                            {
                                                SortTaskDetail old = checkSortList.Where(u => u.Id == so.Id).First();

                                                SortTaskDetail newsort = new SortTaskDetail();
                                                newsort.Id = so.Id;
                                                newsort.TransferQty = old.TransferQty + item.SumQty;
                                                checkSortList.Add(newsort);
                                                checkSortList.Remove(old);
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if (result + "" != "")
                        {
                            return result;
                        }

                        //添加交接日志
                        //TranLog tl1 = new TranLog();
                        //tl1.TranType = "406";
                        //tl1.Description = "完成验证交接";
                        //tl1.TranDate = DateTime.Now;
                        //tl1.TranUser = userName;
                        //tl1.WhCode = transferTask.WhCode;
                        //tl1.Remark = transferTask.TransferId;
                        //idal.ITranLogDAL.Add(tl1);
                        // idal.SaveChanges();

                        //验证交接数量正确后，比较 完整Load数 是否等于 所选交接的Load数
                        if (checkAllShipCount == getloadIdList.Count())
                        {
                            isAllShipFlag = 1;  //变更为 整出，不验证库存
                        }

                        //string s = isAllShipFlag.ToString();
                        //return "Y";

                        transferTask.Status = 30;
                        transferTask.UpdateUser = userName;
                        transferTask.UpdateDate = DateTime.Now;
                        idal.ITransferTaskDAL.UpdateBy(transferTask, u => u.Id == transferTask.Id, new string[] { "Status", "UpdateUser", "UpdateDate" });

                        //添加交接日志
                        //TranLog tl3 = new TranLog();
                        //tl3.TranType = "410";
                        //tl3.Description = "开始执行交接";
                        //tl3.TranDate = DateTime.Now;
                        //tl3.TranUser = userName;
                        //tl3.WhCode = transferTask.WhCode;
                        //tl3.Remark = transferTask.TransferId;
                        //idal.ITranLogDAL.Add(tl3);
                        // idal.SaveChanges();

                        //一次性拉取所有Load的出货流程 Id状态明细
                        List<FlowDetailResult> getflowDetailList = (from a in idal.IFlowDetailDAL.SelectAll()
                                                                    join b in idal.ILoadMasterDAL.SelectAll()
                                                                    on a.FlowHeadId equals b.ProcessId
                                                                    where a.Type == "Shipping" && b.WhCode == transferTask.WhCode && getloadIdList.Contains(b.LoadId)
                                                                    select new FlowDetailResult
                                                                    {
                                                                        FlowRuleId = a.FlowRuleId,
                                                                        StatusId = a.StatusId,
                                                                        StatusName = a.StatusName,
                                                                        LoadId = b.LoadId
                                                                    }).ToList().Distinct().ToList();

                        //整出 删除交接Load的库存
                        if (isAllShipFlag == 1)
                        {
                            foreach (var loadId in getloadIdList)
                            {
                                //删除Load指定托盘数据
                                idal.ILoadHuIdExtendDAL.DeleteByExtended(u => u.LoadId == loadId && u.WhCode == transferTask.WhCode);

                                //删除备货任务
                                idal.IPickTaskDetailDAL.DeleteByExtended(u => u.LoadId == loadId && u.WhCode == transferTask.WhCode);

                                //1.根据Load号 取得  出货流程信息
                                FlowDetailResult flowDetailResult = getflowDetailList.Where(u => u.LoadId == loadId).First();

                                FlowDetail flowDetail = new FlowDetail();
                                flowDetail.FlowRuleId = Convert.ToInt32(flowDetailResult.FlowRuleId);
                                flowDetail.StatusId = flowDetailResult.StatusId;
                                flowDetail.StatusName = flowDetailResult.StatusName;

                                //根据Load号全部删除 分拣 包装 库存数据
                                CompleteTransfer(userName, transferTask.WhCode, loadId, flowDetail, transferTask.TransferId);

                                //更新封箱时间         
                                idal.ILoadMasterDAL.UpdateByExtended(u => u.WhCode == transferTask.WhCode && u.LoadId == loadId, t => new LoadMaster { ShipDate = DateTime.Now, UpdateUser = userName, UpdateDate = DateTime.Now });
                            }
                        }
                        else
                        {
                            //部分交接
                            int[] SortTaskDetailId = (from a in checkSortList
                                                      select a.Id).ToList().Distinct().ToArray();

                            //1.更新分拣表中的交接数量
                            List<SortTaskDetail> getSortTaskDetailList = (from b in idal.ISortTaskDetailDAL.SelectAll()
                                                                          where SortTaskDetailId.Contains(b.Id)
                                                                          select b).ToList();

                            foreach (var sortTaskDetail in checkSortList)
                            {
                                SortTaskDetail sortTaskDetail1 = getSortTaskDetailList.Where(u => u.Id == sortTaskDetail.Id).First();

                                idal.ISortTaskDetailDAL.UpdateByExtended(u => u.Id == sortTaskDetail.Id, t => new SortTaskDetail { TransferQty = sortTaskDetail.TransferQty + sortTaskDetail1.TransferQty, UpdateUser = userName, UpdateDate = DateTime.Now });

                                sortTaskDetail1.TransferQty = sortTaskDetail.TransferQty + sortTaskDetail1.TransferQty;
                                sortTaskDetail1.UpdateUser = userName;
                                sortTaskDetail1.UpdateDate = DateTime.Now;
                            }

                            //2.删除部分库存

                            //得到交接任务头
                            List<TransferHead> getTransferHeadList = (from a in idal.ITransferHeadDAL.SelectAll()
                                                                      where a.TransferTaskId == transferTask.Id
                                                                      select a).ToList().Distinct().ToList();

                            List<TransferDetail> gettransferDetailList1 = (from a in idal.ITransferHeadDAL.SelectAll()
                                                                           join b in idal.ITransferDetailDAL.SelectAll()
                                                                           on a.Id equals b.TransferHeadId
                                                                           where a.TransferTaskId == transferTask.Id
                                                                           select b).ToList().Distinct().ToList();

                            //一次性拉取库存信息
                            List<HuDetail> getHuDetailList = (from a in idal.IHuDetailDAL.SelectAll()
                                                              join b in idal.ITransferHeadDAL.SelectAll()
                                                                   on new { a.WhCode, a.HuId }
                                                               equals new { b.WhCode, HuId = b.LoadId }
                                                              where b.TransferTaskId == transferTask.Id
                                                              select a).ToList().Distinct().ToList();

                            List<TransferDetail> checkTransferDetailChongfu = new List<TransferDetail>();

                            //交接时 验证 同一托盘多条明细中的第一行是否已扣减库存
                            //如果已扣减，应跳过该行 继续下一行
                            List<HuDetail> checkHuList = new List<HuDetail>();

                            #region 扣除库存方法 较难理解
                            foreach (var item in getTransferHeadList)
                            {
                                List<TransferDetail> transferDetailList = gettransferDetailList1.Where(u => u.TransferHeadId == item.Id).ToList();

                                //根据交接数量 减去对应库存
                                foreach (var TransferDetail in transferDetailList)
                                {
                                    //如果 交接款号明细ID已经执行过一次删除库存，就跳过
                                    if (checkTransferDetailChongfu.Where(u => u.Id == TransferDetail.Id).Count() > 0)
                                    {
                                        continue;
                                    }
                                    else
                                    {
                                        //第一次 加入交接款号明细对象
                                        checkTransferDetailChongfu.Add(TransferDetail);
                                    }

                                    int tranQty = TransferDetail.Qty;   //得到交接明细数量

                                    //得到库存装箱数量
                                    List<HuDetail> huDetailList = getHuDetailList.Where(u => u.WhCode == transferTask.WhCode && u.HuId == item.LoadId && u.ItemId == TransferDetail.ItemId).ToList();
                                    if (huDetailList.Count != 0)
                                    {
                                        foreach (var huDetail in huDetailList)
                                        {
                                            //交接明细数量 库存已扣减 循环断开
                                            if (tranQty == 0)
                                            {
                                                break;
                                            }
                                            if (checkHuList.Where(u => u.Id == huDetail.Id).Count() > 0)
                                            {
                                                huDetail.Qty = checkHuList.Where(u => u.Id == huDetail.Id).First().Qty;
                                            }
                                            if (huDetail.Qty < 1)
                                            {
                                                continue;
                                            }

                                            TranLog tl = new TranLog();
                                            tl.TranType = "400";
                                            tl.Description = "部分交接删除库存";
                                            tl.TranDate = DateTime.Now;
                                            tl.TranUser = userName;
                                            tl.WhCode = transferTask.WhCode;
                                            tl.ClientCode = huDetail.ClientCode;
                                            tl.SoNumber = huDetail.SoNumber;
                                            tl.CustomerPoNumber = huDetail.CustomerPoNumber;
                                            tl.AltItemNumber = huDetail.AltItemNumber;
                                            tl.ItemId = huDetail.ItemId;
                                            tl.UnitID = huDetail.UnitId;
                                            tl.UnitName = huDetail.UnitName;
                                            tl.ReceiptId = huDetail.ReceiptId;
                                            tl.ReceiptDate = huDetail.ReceiptDate;
                                            tl.TranQty = huDetail.Qty;
                                            tl.HuId = huDetail.HuId;
                                            tl.Length = huDetail.Length;
                                            tl.Width = huDetail.Width;
                                            tl.Height = huDetail.Height;
                                            tl.Weight = huDetail.Weight;
                                            tl.LotNumber1 = huDetail.LotNumber1;
                                            tl.LotNumber2 = huDetail.LotNumber2;
                                            tl.LotDate = huDetail.LotDate;
                                            tl.CustomerOutPoNumber = item.CustomerOutPoNumber;
                                            tl.OutPoNumber = item.OutPoNumber;
                                            tl.LoadId = item.LoadId;
                                            tl.Remark = transferTask.TransferId;

                                            //交接数量 大于库存托盘的数量时
                                            //需要把当前托盘库存扣除为0 然后继续扣除下一个托盘
                                            if (huDetail.Qty < tranQty)
                                            {
                                                tl.TranQty2 = huDetail.Qty;

                                                tranQty = tranQty - huDetail.Qty;
                                                huDetail.Qty = 0;
                                            }
                                            else
                                            {
                                                tl.TranQty2 = tranQty;

                                                huDetail.Qty = huDetail.Qty - tranQty;
                                                tranQty = 0;    //交接数量已扣减   
                                            }

                                            //记录下当前托盘 已扣除过的数量 不能在下个交接明细中继续扣除
                                            //如：PLTA00001 SKU1 数量1 在交接1中被扣除了，但 交接2循环进来就不能再扣减它了
                                            if (checkHuList.Where(u => u.Id == huDetail.Id).Count() == 0)
                                            {
                                                HuDetail newHu = new HuDetail();
                                                newHu.Id = huDetail.Id;
                                                newHu.Qty = huDetail.Qty;
                                                checkHuList.Add(newHu);
                                            }
                                            else
                                            {
                                                HuDetail oldHu = checkHuList.Where(u => u.Id == huDetail.Id).First();
                                                checkHuList.Remove(oldHu);

                                                HuDetail newHu = new HuDetail();
                                                newHu.Id = huDetail.Id;
                                                newHu.Qty = huDetail.Qty;
                                                checkHuList.Add(newHu);
                                            }

                                            //记录交易日志
                                            tlList.Add(tl);
                                        }
                                    }
                                }
                            }
                            #endregion

                            //return "Y";

                            idal.ITranLogDAL.Add(tlList);
                            //  idal.SaveChanges();

                            //一次性删除库存为0的数据
                            int[] delHuDetailId = (from a in checkHuList
                                                   where a.Qty == 0
                                                   select a.Id).ToArray();

                            if (delHuDetailId.Count() > 0)
                            {
                                idal.IHuDetailDAL.DeleteByExtended(u => delHuDetailId.Contains(u.Id));
                            }



                            // int[] itemIds = checkHuList.Where(u => u.Qty > 0).Select(g=>g.Id).ToList().Distinct().ToArray();

                            //执行调整库存
                            foreach (var item in checkHuList.Where(u => u.Qty > 0))
                            {
                                idal.IHuDetailDAL.UpdateByExtended(u => u.Id == item.Id, t => new HuDetail { Qty = item.Qty, UpdateUser = userName, UpdateDate = DateTime.Now });
                            }



                            ////添加交接日志
                            //TranLog tl5 = new TranLog();
                            //tl5.TranType = "415";
                            //tl5.Description = "部分交接删除分拣包装信息";
                            //tl5.TranDate = DateTime.Now;
                            //tl5.TranUser = userName;
                            //tl5.WhCode = transferTask.WhCode;
                            //tl5.Remark = transferTask.TransferId;
                            //idal.ITranLogDAL.Add(tl5);

                            //3.部分交接 删除正常订单的 分拣 包装数据

                            List<SortTaskDetailResult> SortTaskDetailResultList = new List<SortTaskDetailResult>();

                            //拉取分拣明细
                            List<SortTaskDetail> GetSortTaskDetailListCheck = idal.ISortTaskDetailDAL.SelectBy(u => u.WhCode == transferTask.WhCode && getloadIdList.Contains(u.LoadId));

                            //拉取分拣Load\订单
                            List<SortTaskDetailResult> getLoadOutPoNumberList = (from a in idal.ISortTaskDetailDAL.SelectAll()
                                                                                 where a.WhCode == transferTask.WhCode && getloadIdList.Contains(a.LoadId) && a.HoldFlag == 0 && a.PlanQty == a.PackQty && a.PlanQty == a.TransferQty
                                                                                 group new { a } by new
                                                                                 {
                                                                                     a.WhCode,
                                                                                     a.LoadId,
                                                                                     a.OutPoNumber
                                                                                 } into g
                                                                                 select new SortTaskDetailResult
                                                                                 {
                                                                                     LoadId = g.Key.LoadId,
                                                                                     WhCode = g.Key.WhCode,
                                                                                     OutPoNumber = g.Key.OutPoNumber
                                                                                 }).ToList();


                            //拉取待删除Load的所有包装明细信息
                            List<PackTaskDeleteResult> getPackALlDetail = (from a in idal.IPackTaskDAL.SelectAll()
                                                                           join b in idal.IPackHeadDAL.SelectAll() on new { Id = a.Id } equals new { Id = (Int32)b.PackTaskId } into b_join
                                                                           from b in b_join.DefaultIfEmpty()
                                                                           join c in idal.IPackDetailDAL.SelectAll() on new { Id = b.Id } equals new { Id = (Int32)c.PackHeadId } into c_join
                                                                           from c in c_join.DefaultIfEmpty()
                                                                           where getloadIdList.Contains(a.LoadId) && a.WhCode == transferTask.WhCode
                                                                           select new PackTaskDeleteResult
                                                                           {
                                                                               WhCode = a.WhCode,
                                                                               LoadId = a.LoadId,
                                                                               OutPoNumber = a.OutPoNumber,
                                                                               PackTaskId = (Int32?)a.Id,
                                                                               PackHeadId = (Int32?)b.Id,
                                                                               PackDetailId = (Int32?)c.Id
                                                                           }).ToList();

                            List<TranLog> trList = new List<TranLog>();
                            foreach (var loadId in getloadIdList)
                            {
                                //1.根据Load号 取得  出货流程信息
                                FlowDetailResult flowDetailResult = getflowDetailList.Where(u => u.LoadId == loadId).First();

                                FlowDetail flowDetail = new FlowDetail();
                                flowDetail.FlowRuleId = Convert.ToInt32(flowDetailResult.FlowRuleId);
                                flowDetail.StatusId = flowDetailResult.StatusId;
                                flowDetail.StatusName = flowDetailResult.StatusName;

                                List<SortTaskDetailResult> SortTaskDetailResult1 = getLoadOutPoNumberList.Where(u => u.LoadId == loadId).ToList();

                                //2.验证Load下的出库订单是否是全部包装了
                                //如果 快递单交接了部分款号，是不能清除分拣表的
                                foreach (var item in SortTaskDetailResult1)
                                {
                                    List<SortTaskDetail> checkSortTaskDetail = GetSortTaskDetailListCheck.Where(u => u.WhCode == item.WhCode && u.LoadId == item.LoadId && u.OutPoNumber == item.OutPoNumber).ToList();
                                    if (checkSortTaskDetail.Where(u => u.PlanQty != u.PackQty || u.PlanQty != u.TransferQty).Count() > 0)
                                    {
                                        continue;
                                    }
                                    else
                                    {
                                        SortTaskDetailResultList.Add(item);
                                    }
                                }

                                if (SortTaskDetailResultList.Count > 0)
                                {
                                    string[] OutPoNumberarr = (from a in SortTaskDetailResultList select a.OutPoNumber).ToList().Distinct().ToArray();

                                    //更新订单状态
                                    idal.IOutBoundOrderDAL.UpdateByExtended(u => u.WhCode == transferTask.WhCode && OutPoNumberarr.Contains(u.OutPoNumber) && u.StatusId != -10, t => new OutBoundOrder { NowProcessId = flowDetail.FlowRuleId, StatusId = flowDetail.StatusId, StatusName = flowDetail.StatusName, UpdateUser = userName, UpdateDate = DateTime.Now });

                                    foreach (var sortTaskDetail in SortTaskDetailResultList)
                                    {
                                        //更新订单状态，插入日志
                                        TranLog tlorder = new TranLog();
                                        tlorder.TranType = "32";
                                        tlorder.Description = "部分交接更新订单状态";
                                        tlorder.TranDate = DateTime.Now;
                                        tlorder.TranUser = userName;
                                        tlorder.WhCode = sortTaskDetail.WhCode;
                                        tlorder.LoadId = sortTaskDetail.LoadId;
                                        tlorder.OutPoNumber = sortTaskDetail.OutPoNumber;
                                        tlorder.Remark = "变更为：" + flowDetail.StatusId + flowDetail.StatusName;
                                        trList.Add(tlorder);
                                    }

                                    //删除包装 ---------------------------------

                                    //得到包装任务
                                    List<PackTaskDeleteResult> packTaskList = getPackALlDetail.Where(u => u.WhCode == transferTask.WhCode && u.LoadId == loadId && OutPoNumberarr.Contains(u.OutPoNumber)).ToList();

                                    int?[] packTaskId = (from a in packTaskList select a.PackTaskId).ToList().Distinct().ToArray();

                                    int?[] packHeadId = (from a in packTaskList select a.PackHeadId).ToList().Distinct().ToArray();

                                    int?[] packDetaild = (from a in packTaskList select a.PackDetailId).ToList().Distinct().ToArray();

                                    if (packDetaild.Count() > 0)
                                    {
                                        //删除明细对应的扫描
                                        idal.IPackScanNumberDAL.DeleteByExtended(u => packDetaild.Contains(u.PackDetailId));

                                        //删除包装明细
                                        idal.IPackDetailDAL.DeleteByExtended(u => packDetaild.Contains(u.Id));
                                    }

                                    if (packHeadId.Count() > 0)
                                    {
                                        //删除包装头
                                        idal.IPackHeadDAL.DeleteByExtended(u => packHeadId.Contains(u.Id));
                                    }

                                    if (packTaskId.Count() > 0)
                                    {
                                        //删除包装任务
                                        idal.IPackTaskDAL.DeleteByExtended(u => packTaskId.Contains(u.Id));
                                    }

                                    //再删除分拣
                                    idal.ISortTaskDetailDAL.DeleteByExtended(u => u.WhCode == transferTask.WhCode && u.LoadId == loadId && OutPoNumberarr.Contains(u.OutPoNumber));
                                }
                            }

                            idal.ITranLogDAL.Add(trList);

                            ////添加交接日志
                            //TranLog tl6 = new TranLog();
                            //tl6.TranType = "416";
                            //tl6.Description = "完成部分交接删除分拣包装信息";
                            //tl6.TranDate = DateTime.Now;
                            //tl6.TranUser = userName;
                            //tl6.WhCode = transferTask.WhCode;
                            //tl6.Remark = transferTask.TransferId;
                            //idal.ITranLogDAL.Add(tl6);

                            //   idal.SaveChanges();

                            //再次检查库存，并更新封箱时间
                            List<HuDetail> getNewHuDetailList = idal.IHuDetailDAL.SelectBy(u => u.WhCode == transferTask.WhCode && getloadIdList.Contains(u.HuId));

                            List<SortTaskDetail> GetSortTaskDetailListCheck1 = idal.ISortTaskDetailDAL.SelectBy(u => u.WhCode == transferTask.WhCode && getloadIdList.Contains(u.LoadId));

                            //最后 验证Load是否还有数据，如果没有 清除备货任务表
                            foreach (var loadId in getloadIdList)
                            {
                                List<HuDetail> checkHuDetailList = getNewHuDetailList.Where(u => u.WhCode == transferTask.WhCode && u.HuId == loadId && u.Qty != 0).ToList();
                                if (checkHuDetailList.Count == 0)
                                {
                                    idal.IHuMasterDAL.DeleteByExtended(u => u.WhCode == transferTask.WhCode && u.HuId == loadId);
                                }

                                //更新封箱时间         
                                idal.ILoadMasterDAL.UpdateByExtended(u => u.WhCode == transferTask.WhCode && u.LoadId == loadId, t => new LoadMaster { ShipDate = DateTime.Now, UpdateUser = userName, UpdateDate = DateTime.Now });

                                if (GetSortTaskDetailListCheck1.Where(u => u.LoadId == loadId).Count() == 0)
                                {
                                    //删除分拣任务
                                    idal.ISortTaskDAL.DeleteByExtended(u => u.WhCode == transferTask.WhCode && u.LoadId == loadId);

                                    //删除Load指定托盘数据
                                    idal.ILoadHuIdExtendDAL.DeleteByExtended(u => u.LoadId == loadId && u.WhCode == transferTask.WhCode);

                                    //删除备货任务数据
                                    idal.IPickTaskDetailDAL.DeleteByExtended(u => u.LoadId == loadId && u.WhCode == transferTask.WhCode);
                                }
                            }
                        }

                        ////添加交接日志
                        //TranLog tl4 = new TranLog();
                        //tl4.TranType = "420";
                        //tl4.Description = "完成执行交接";
                        //tl4.TranDate = DateTime.Now;
                        //tl4.TranUser = userName;
                        //tl4.WhCode = transferTask.WhCode;
                        //tl4.Remark = transferTask.TransferId;

                        //idal.ITranLogDAL.Add(tl4);
                        // idal.SaveChanges();

                        //插入EDI任务数据
                        EclOutBoundManager eom = new EclOutBoundManager();
                        eom.UrlEdiTaskInsertOut(transferTask.TransferId, transferTask.WhCode, userName);
                        idal.SaveChanges();

                        // Monitor.Exit(o1);
                        trans.Complete();
                        return "Y";
                    }
                    catch (Exception e)
                    {
                        string s = e.InnerException.Message;
                        trans.Dispose();//出现异常，事务手动释放
                                        // Monitor.Exit(o1);
                        return "交接异常，请重新提交！";
                    }
                    //}
                    //else
                    //{

                    //    return "有其他交接正在进行交接,请稍后！";
                    //}

                }
            }
        }



        public string BeginTransferTask333(int transferId, string userName, int fayunQty)
        {

            List<TransferTask> TransferTaskList = idal.ITransferTaskDAL.SelectBy(u => u.Id == transferId);
            if (TransferTaskList.Count == 0)
            {
                return "未找到交接信息，请查询！";
            }

            TransferTask transferTask = TransferTaskList.First();
            if (transferTask.Status == -10)
            {
                return "交接任务被拦截，请查询！";
            }
            if (transferTask.Status == 30)
            {
                return "当前交接任务已完成交接！";
            }

            //拦截交接中的快递单
            List<TransferHead> TransferHeadList = idal.ITransferHeadDAL.SelectBy(u => u.TransferTaskId == transferTask.Id);

            string checkResult = "";
            foreach (var item in TransferHeadList.Where(u => u.Status == -10))
            {
                checkResult += "快递单号：" + item.ExpressNumber + "被拦截！";
            }
            if (checkResult != "")
            {
                return checkResult;
            }

            List<TransferHeadDetailResult> getCount = (from transferhead in
                                                       (from transferhead in idal.ITransferHeadDAL.SelectAll()
                                                        where
                                                          transferhead.TransferTaskId == transferId
                                                        select new
                                                        {
                                                            Dummy = "x"
                                                        })
                                                       group transferhead by new { transferhead.Dummy } into g
                                                       select new TransferHeadDetailResult
                                                       {
                                                           SumQty = g.Count()
                                                       }).ToList();
            if (getCount.Count == 0)
            {
                return "没有交接信息！";
            }

            if (getCount.First().SumQty != fayunQty)
            {
                return "发运数量与交接数量不一致！发运数量：" + fayunQty + " 交接数量：" + getCount.First().SumQty;
            }

            //并发锁
            lock (o1)
            {
                using (TransactionScope trans = new TransactionScope())
                {
                    try
                    {
                        List<TranLog> tlList = new List<TranLog>();

                        int isAllShipFlag = 0;  //交接是否是整出 0否 1是，0非整出时 会循环扣除库存

                        //1.验证分拣表数量 与 实际交接数量 是否一致

                        //得到交接明细信息
                        string result = "";
                        List<TransferHeadDetailResult> checkList = (from c in idal.ITransferTaskDAL.SelectAll()
                                                                    join a in idal.ITransferHeadDAL.SelectAll() on new { Id = c.Id } equals new { Id = (Int32)a.TransferTaskId } into a_join
                                                                    from a in a_join.DefaultIfEmpty()
                                                                    join b in idal.ITransferDetailDAL.SelectAll()
                                                                          on new { a.Id }
                                                                      equals new { Id = (Int32)b.TransferHeadId } into b_join
                                                                    from b in b_join.DefaultIfEmpty()
                                                                    where c.Id == transferTask.Id
                                                                    group new { a, b } by new
                                                                    {
                                                                        a.WhCode,
                                                                        a.LoadId,
                                                                        a.SortGroupId,
                                                                        a.SortGroupNumber,
                                                                        a.OutPoNumber,
                                                                        b.ItemId
                                                                    } into g
                                                                    select new TransferHeadDetailResult
                                                                    {
                                                                        WhCode = g.Key.WhCode,
                                                                        LoadId = g.Key.LoadId,
                                                                        SortGroupId = g.Key.SortGroupId,
                                                                        SortGroupNumber = g.Key.SortGroupNumber,
                                                                        OutPoNumber = g.Key.OutPoNumber,
                                                                        ItemId = g.Key.ItemId,
                                                                        SumQty = g.Sum(p => p.b.Qty)
                                                                    }).ToList();
                        if (checkList.Count == 0)
                        {
                            result = "未找到可交接的信息，无法交接！";
                            return result;
                        }

                        //得到当前交接的Load
                        string[] getloadIdList = (from a in checkList
                                                  select a.LoadId).Distinct().ToArray();

                        //验证集合
                        List<SortTaskDetail> checkSortList = new List<SortTaskDetail>();

                        //得到分拣数据
                        List<SortTaskDetail> getsortTaskDetailList1 = (from b in idal.ISortTaskDetailDAL.SelectAll()
                                                                       join a in idal.ITransferHeadDAL.SelectAll()
                                                                       on new { A = b.WhCode, B = b.LoadId } equals new { A = a.WhCode, B = a.LoadId } into temp1
                                                                       from ab in temp1.DefaultIfEmpty()
                                                                       where ab.TransferTaskId == transferTask.Id
                                                                       select b).ToList().Distinct().ToList();

                        int checkAllShipCount = 0;  //是完整交接的Load数

                        //循环Load，验证交接的每个Load 是否都是完整的
                        foreach (var loadid in getloadIdList)
                        {
                            int? checkQty = checkList.Where(u => u.LoadId == loadid).Sum(u => u.SumQty);
                            int? checkQty1 = getsortTaskDetailList1.Where(u => u.LoadId == loadid && u.HoldFlag == 0).Sum(u => (u.PlanQty - u.TransferQty));
                            if (checkQty > checkQty1)
                            {
                                string getResult = "";
                                foreach (var itemCheck in checkList)
                                {
                                    if (getsortTaskDetailList1.Where(u => u.LoadId == itemCheck.LoadId && u.WhCode == itemCheck.WhCode && u.HoldFlag == 0 && u.ItemId == itemCheck.ItemId && u.OutPoNumber == itemCheck.OutPoNumber && u.GroupId == itemCheck.SortGroupId).Count() == 0)
                                    {
                                        getResult = " 系统出库单号:" + itemCheck.OutPoNumber + " 分拣组号:" + itemCheck.SortGroupId + " 款号ID:" + itemCheck.ItemId + "异常!";
                                        break;
                                    }
                                }
                                result = "Load:" + loadid + "交接总数大于系统所需总数！系统所需总数：" + checkQty1 + " 交接总数：" + checkQty + getResult;
                                break;
                            }
                            else
                            {
                                int? trqty = checkList.Where(u => u.LoadId == loadid).Sum(u => u.SumQty);
                                int? trqty1 = getsortTaskDetailList1.Where(u => u.LoadId == loadid).Sum(u => u.PlanQty);

                                //如果验证时 交接数量等于分拣计划数量
                                if (trqty == trqty1)
                                {
                                    checkAllShipCount++;
                                }

                                foreach (var item in checkList.Where(u => u.LoadId == loadid))
                                {
                                    List<SortTaskDetail> checksortTaskDetailList = getsortTaskDetailList1.Where(u => u.WhCode == item.WhCode && u.LoadId == item.LoadId && u.GroupId == item.SortGroupId && u.GroupNumber == item.SortGroupNumber && u.OutPoNumber == item.OutPoNumber && u.ItemId == item.ItemId && u.TransferQty < u.PlanQty).ToList();
                                    if (checksortTaskDetailList.Count == 0)
                                    {
                                        result = "Load:" + loadid + "未找到分拣明细信息！";
                                        break;
                                    }
                                    else
                                    {
                                        SortTaskDetail so = checksortTaskDetailList.First();
                                        int? transferQty = so.TransferQty;
                                        if (checkSortList.Where(u => u.Id == so.Id).Count() > 0)
                                        {
                                            transferQty = transferQty + checkSortList.Where(u => u.Id == so.Id).First().TransferQty;
                                        }

                                        if (transferQty + item.SumQty > so.PlanQty)
                                        {
                                            result = "交接数量大于系统所需数量,无法交接！Load:" + item.LoadId + " 系统出库单号:" + item.OutPoNumber + " 分拣组号:" + item.SortGroupId + " 款号:" + item.AltItemNumber + " 系统所需数量：" + so.PlanQty + " 交接数量:" + transferQty + item.SumQty;
                                            break;
                                        }
                                        else
                                        {
                                            if (checkSortList.Where(u => u.Id == so.Id).Count() == 0)
                                            {
                                                SortTaskDetail newsort = new SortTaskDetail();
                                                newsort.Id = so.Id;
                                                newsort.TransferQty = item.SumQty;
                                                checkSortList.Add(newsort);
                                            }
                                            else
                                            {
                                                SortTaskDetail old = checkSortList.Where(u => u.Id == so.Id).First();

                                                SortTaskDetail newsort = new SortTaskDetail();
                                                newsort.Id = so.Id;
                                                newsort.TransferQty = old.TransferQty + item.SumQty;
                                                checkSortList.Add(newsort);
                                                checkSortList.Remove(old);
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if (result + "" != "")
                        {
                            return result;
                        }

                        //验证交接数量正确后，比较 完整Load数 是否等于 所选交接的Load数
                        if (checkAllShipCount == getloadIdList.Count())
                        {
                            isAllShipFlag = 1;  //变更为 整出，不验证库存
                        }

                        transferTask.Status = 30;
                        transferTask.UpdateUser = userName;
                        transferTask.UpdateDate = DateTime.Now;
                        idal.ITransferTaskDAL.UpdateBy(transferTask, u => u.Id == transferTask.Id, new string[] { "Status", "UpdateUser", "UpdateDate" });

                        //一次性拉取所有Load的出货流程 Id状态明细
                        List<FlowDetailResult> getflowDetailList = (from a in idal.IFlowDetailDAL.SelectAll()
                                                                    join b in idal.ILoadMasterDAL.SelectAll()
                                                                    on a.FlowHeadId equals b.ProcessId
                                                                    where a.Type == "Shipping" && b.WhCode == transferTask.WhCode && getloadIdList.Contains(b.LoadId)
                                                                    select new FlowDetailResult
                                                                    {
                                                                        FlowRuleId = a.FlowRuleId,
                                                                        StatusId = a.StatusId,
                                                                        StatusName = a.StatusName,
                                                                        LoadId = b.LoadId
                                                                    }).ToList().Distinct().ToList();

                        //整出 删除交接Load的库存
                        if (isAllShipFlag == 1)
                        {
                            foreach (var loadId in getloadIdList)
                            {
                                //删除Load指定托盘数据
                                idal.ILoadHuIdExtendDAL.DeleteByExtended(u => u.LoadId == loadId && u.WhCode == transferTask.WhCode);

                                //删除备货任务
                                idal.IPickTaskDetailDAL.DeleteByExtended(u => u.LoadId == loadId && u.WhCode == transferTask.WhCode);

                                //1.根据Load号 取得  出货流程信息
                                FlowDetailResult flowDetailResult = getflowDetailList.Where(u => u.LoadId == loadId).First();

                                FlowDetail flowDetail = new FlowDetail();
                                flowDetail.FlowRuleId = Convert.ToInt32(flowDetailResult.FlowRuleId);
                                flowDetail.StatusId = flowDetailResult.StatusId;
                                flowDetail.StatusName = flowDetailResult.StatusName;

                                //根据Load号全部删除 分拣 包装 库存数据
                                CompleteTransfer(userName, transferTask.WhCode, loadId, flowDetail, transferTask.TransferId);

                                //更新封箱时间         
                                idal.ILoadMasterDAL.UpdateByExtended(u => u.WhCode == transferTask.WhCode && u.LoadId == loadId, t => new LoadMaster { ShipDate = DateTime.Now, UpdateUser = userName, UpdateDate = DateTime.Now });
                            }
                        }
                        else
                        {
                            //部分交接
                            int[] SortTaskDetailId = (from a in checkSortList
                                                      select a.Id).ToList().Distinct().ToArray();

                            //1.更新分拣表中的交接数量
                            List<SortTaskDetail> getSortTaskDetailList = (from b in idal.ISortTaskDetailDAL.SelectAll()
                                                                          where SortTaskDetailId.Contains(b.Id)
                                                                          select b).ToList();

                            foreach (var sortTaskDetail in checkSortList)
                            {
                                SortTaskDetail sortTaskDetail1 = getSortTaskDetailList.Where(u => u.Id == sortTaskDetail.Id).First();

                                idal.ISortTaskDetailDAL.UpdateByExtended(u => u.Id == sortTaskDetail.Id, t => new SortTaskDetail { TransferQty = sortTaskDetail.TransferQty + sortTaskDetail1.TransferQty, UpdateUser = userName, UpdateDate = DateTime.Now });

                                sortTaskDetail1.TransferQty = sortTaskDetail.TransferQty + sortTaskDetail1.TransferQty;
                                sortTaskDetail1.UpdateUser = userName;
                                sortTaskDetail1.UpdateDate = DateTime.Now;
                            }

                            List<SortTaskDetailResult> SortTaskDetailResultList = new List<SortTaskDetailResult>();

                            //拉取分拣明细
                            List<SortTaskDetail> GetSortTaskDetailListCheck = idal.ISortTaskDetailDAL.SelectBy(u => u.WhCode == transferTask.WhCode && getloadIdList.Contains(u.LoadId));

                            //拉取分拣Load\订单
                            List<SortTaskDetailResult> getLoadOutPoNumberList = (from a in idal.ISortTaskDetailDAL.SelectAll()
                                                                                 where a.WhCode == transferTask.WhCode && getloadIdList.Contains(a.LoadId) && a.HoldFlag == 0 && a.PlanQty == a.PackQty && a.PlanQty == a.TransferQty
                                                                                 group new { a } by new
                                                                                 {
                                                                                     a.WhCode,
                                                                                     a.LoadId,
                                                                                     a.OutPoNumber
                                                                                 } into g
                                                                                 select new SortTaskDetailResult
                                                                                 {
                                                                                     LoadId = g.Key.LoadId,
                                                                                     WhCode = g.Key.WhCode,
                                                                                     OutPoNumber = g.Key.OutPoNumber
                                                                                 }).ToList();


                            //拉取待删除Load的所有包装明细信息
                            List<PackTaskDeleteResult> getPackALlDetail = (from a in idal.IPackTaskDAL.SelectAll()
                                                                           join b in idal.IPackHeadDAL.SelectAll() on new { Id = a.Id } equals new { Id = (Int32)b.PackTaskId } into b_join
                                                                           from b in b_join.DefaultIfEmpty()
                                                                           join c in idal.IPackDetailDAL.SelectAll() on new { Id = b.Id } equals new { Id = (Int32)c.PackHeadId } into c_join
                                                                           from c in c_join.DefaultIfEmpty()
                                                                           where getloadIdList.Contains(a.LoadId) && a.WhCode == transferTask.WhCode
                                                                           select new PackTaskDeleteResult
                                                                           {
                                                                               WhCode = a.WhCode,
                                                                               LoadId = a.LoadId,
                                                                               OutPoNumber = a.OutPoNumber,
                                                                               PackTaskId = (Int32?)a.Id,
                                                                               PackHeadId = (Int32?)b.Id,
                                                                               PackDetailId = (Int32?)c.Id
                                                                           }).ToList();

                            List<TranLog> trList = new List<TranLog>();
                            foreach (var loadId in getloadIdList)
                            {
                                //1.根据Load号 取得  出货流程信息
                                FlowDetailResult flowDetailResult = getflowDetailList.Where(u => u.LoadId == loadId).First();

                                FlowDetail flowDetail = new FlowDetail();
                                flowDetail.FlowRuleId = Convert.ToInt32(flowDetailResult.FlowRuleId);
                                flowDetail.StatusId = flowDetailResult.StatusId;
                                flowDetail.StatusName = flowDetailResult.StatusName;

                                List<SortTaskDetailResult> SortTaskDetailResult1 = getLoadOutPoNumberList.Where(u => u.LoadId == loadId).ToList();

                                //2.验证Load下的出库订单是否是全部包装了
                                //如果 快递单交接了部分款号，是不能清除分拣表的
                                foreach (var item in SortTaskDetailResult1)
                                {
                                    List<SortTaskDetail> checkSortTaskDetail = GetSortTaskDetailListCheck.Where(u => u.WhCode == item.WhCode && u.LoadId == item.LoadId && u.OutPoNumber == item.OutPoNumber).ToList();
                                    if (checkSortTaskDetail.Where(u => u.PlanQty != u.PackQty || u.PlanQty != u.TransferQty).Count() > 0)
                                    {
                                        continue;
                                    }
                                    else
                                    {
                                        SortTaskDetailResultList.Add(item);
                                    }
                                }

                                if (SortTaskDetailResultList.Count > 0)
                                {
                                    string[] OutPoNumberarr = (from a in SortTaskDetailResultList select a.OutPoNumber).ToList().Distinct().ToArray();

                                    //更新订单状态
                                    idal.IOutBoundOrderDAL.UpdateByExtended(u => u.WhCode == transferTask.WhCode && OutPoNumberarr.Contains(u.OutPoNumber) && u.StatusId != -10, t => new OutBoundOrder { NowProcessId = flowDetail.FlowRuleId, StatusId = flowDetail.StatusId, StatusName = flowDetail.StatusName, UpdateUser = userName, UpdateDate = DateTime.Now });

                                    foreach (var sortTaskDetail in SortTaskDetailResultList)
                                    {
                                        //更新订单状态，插入日志
                                        TranLog tlorder = new TranLog();
                                        tlorder.TranType = "32";
                                        tlorder.Description = "部分交接更新订单状态";
                                        tlorder.TranDate = DateTime.Now;
                                        tlorder.TranUser = userName;
                                        tlorder.WhCode = sortTaskDetail.WhCode;
                                        tlorder.LoadId = sortTaskDetail.LoadId;
                                        tlorder.OutPoNumber = sortTaskDetail.OutPoNumber;
                                        tlorder.Remark = "变更为：" + flowDetail.StatusId + flowDetail.StatusName;
                                        trList.Add(tlorder);
                                    }
                                }
                            }

                            idal.ITranLogDAL.Add(trList);

                        }


                        //插入EDI任务数据
                        EclOutBoundManager eom = new EclOutBoundManager();
                        eom.UrlEdiTaskInsertOut(transferTask.TransferId, transferTask.WhCode, userName);
                        idal.SaveChanges();


                        trans.Complete();
                        return "Y";
                    }
                    catch (Exception e)
                    {
                        string s = e.InnerException.Message;
                        trans.Dispose();//出现异常，事务手动释放

                        return "交接异常，请重新提交！";
                    }

                }
            }
        }

        #endregion

        //---------------------------------------------

        //完成交接删除分拣 包装 库存信息
        private void CompleteTransfer(string userName, string whCode, string loadId, FlowDetail flowDetail, string TransferId)
        {
            //更新订单状态，插入日志
            TranLog tranLog1 = new TranLog();
            tranLog1.TranType = "400";
            tranLog1.Description = "整出交接删除库存";
            tranLog1.TranDate = DateTime.Now;
            tranLog1.TranUser = userName;
            tranLog1.WhCode = whCode;
            tranLog1.LoadId = loadId;
            tranLog1.Remark = TransferId;
            idal.ITranLogDAL.Add(tranLog1);

            //删除库存
            idal.IHuDetailDAL.DeleteByExtended(u => u.HuId == loadId && u.WhCode == whCode);
            idal.IHuMasterDAL.DeleteByExtended(u => u.HuId == loadId && u.WhCode == whCode);

            //添加交接日志
            TranLog tl5 = new TranLog();
            tl5.TranType = "415";
            tl5.Description = "整出交接删除分拣包装";
            tl5.TranDate = DateTime.Now;
            tl5.TranUser = userName;
            tl5.WhCode = whCode;
            tl5.LoadId = loadId;
            tl5.Remark = TransferId;
            idal.ITranLogDAL.Add(tl5);

            //删除分拣任务及明细
            idal.ISortTaskDAL.DeleteByExtended(u => u.WhCode == whCode && u.LoadId == loadId);
            idal.ISortTaskDetailDAL.DeleteByExtended(u => u.WhCode == whCode && u.LoadId == loadId);

            //删除包装 ---------------------------------

            //得到包装任务
            List<PackTaskDeleteResult> packTaskList = (from a in idal.IPackTaskDAL.SelectAll()
                                                       join b in idal.IPackHeadDAL.SelectAll() on new { Id = a.Id } equals new { Id = (Int32)b.PackTaskId } into b_join
                                                       from b in b_join.DefaultIfEmpty()
                                                       join c in idal.IPackDetailDAL.SelectAll() on new { Id = b.Id } equals new { Id = (Int32)c.PackHeadId } into c_join
                                                       from c in c_join.DefaultIfEmpty()
                                                       where a.LoadId == loadId && a.WhCode == whCode
                                                       select new PackTaskDeleteResult
                                                       {
                                                           WhCode = a.WhCode,
                                                           LoadId = a.LoadId,
                                                           OutPoNumber = a.OutPoNumber,
                                                           PackTaskId = (Int32?)a.Id,
                                                           PackHeadId = (Int32?)b.Id,
                                                           PackDetailId = (Int32?)c.Id
                                                       }).ToList();

            int?[] packTaskId = (from a in packTaskList select a.PackTaskId).ToList().Distinct().ToArray();

            int?[] packHeadId = (from a in packTaskList select a.PackHeadId).ToList().Distinct().ToArray();

            int?[] packDetaild = (from a in packTaskList select a.PackDetailId).ToList().Distinct().ToArray();

            //删除明细对应的扫描
            idal.IPackScanNumberDAL.DeleteByExtended(u => packDetaild.Contains(u.PackDetailId));

            //删除包装明细
            idal.IPackDetailDAL.DeleteByExtended(u => packDetaild.Contains(u.Id));

            //删除包装头
            idal.IPackHeadDAL.DeleteByExtended(u => packHeadId.Contains(u.Id));

            //删除包装任务
            idal.IPackTaskDAL.DeleteByExtended(u => packTaskId.Contains(u.Id));


            //修改OutOrder状态
            List<OutBoundOrderResult> list = (from a in idal.ILoadDetailDAL.SelectAll()
                                              join b in idal.ILoadMasterDAL.SelectAll()
                                              on a.LoadMasterId equals b.Id
                                              where b.WhCode == whCode && b.LoadId == loadId
                                              select new OutBoundOrderResult
                                              {
                                                  Id = (Int32)a.OutBoundOrderId
                                              }).ToList();

            int[] OutPoNumberarr = (from a in list select a.Id).ToList().Distinct().ToArray();

            //更新订单状态
            idal.IOutBoundOrderDAL.UpdateByExtended(u => OutPoNumberarr.Contains(u.Id) && u.StatusId > 25, t => new OutBoundOrder { NowProcessId = flowDetail.FlowRuleId, StatusId = flowDetail.StatusId, StatusName = flowDetail.StatusName, UpdateUser = userName, UpdateDate = DateTime.Now });

            List<TranLog> trList = new List<TranLog>();
            foreach (var outBoundOrder in list)
            {
                //更新订单状态，插入日志
                TranLog tlorder = new TranLog();
                tlorder.TranType = "32";
                tlorder.Description = "整出交接更新订单状态";
                tlorder.TranDate = DateTime.Now;
                tlorder.TranUser = userName;
                tlorder.WhCode = whCode;
                tlorder.LoadId = loadId;
                tlorder.OutPoNumber = outBoundOrder.Id.ToString();
                tlorder.Remark = "变更为：" + flowDetail.StatusId + flowDetail.StatusName;
                trList.Add(tlorder);
            }
            idal.ITranLogDAL.Add(trList);

        }


        //多品交接
        //交接工作台 按出库订单交接
        public string TransferTaskOutBoundOrderInsert(string transferNumber, string userName, string loadId, string whCode, string workFlag)
        {
            lock (o2)
            {
                using (TransactionScope trans = new TransactionScope())
                {
                    try
                    {
                        #region 开始交接

                        List<SortTaskDetail> CheckSortTaskDetailList = idal.ISortTaskDetailDAL.SelectBy(u => u.WhCode == whCode && u.LoadId == loadId && u.HoldFlag == 0);

                        if (CheckSortTaskDetailList.Where(u => u.PlanQty != u.Qty).Count() > 0)
                        {
                            return "Load未完成备货分拣，请检查！";
                        }

                        if (CheckSortTaskDetailList.Where(u => u.PlanQty != u.PackQty).Count() > 0)
                        {
                            return "Load未完成包装，请检查！";
                        }


                        List<PackTask> PackTaskList = (from a in idal.IPackTaskDAL.SelectAll()
                                                       join b in idal.IPackHeadDAL.SelectAll()
                                                       on a.Id equals b.PackTaskId
                                                       where a.WhCode == whCode && a.LoadId == loadId && (b.TransferHeadId ?? "") == ""
                                                       select a).ToList();

                        if (PackTaskList.Count == 0)
                        {
                            return "Load有误或没有可交接的信息！";
                        }

                        List<PackHead> CheckPackHeadList = (from a in idal.IPackTaskDAL.SelectAll()
                                                            join b in idal.IPackHeadDAL.SelectAll()
                                                            on a.Id equals b.PackTaskId
                                                            where a.WhCode == whCode && a.LoadId == loadId && (b.TransferHeadId ?? "") == ""
                                                            select b).ToList();

                        if (CheckPackHeadList.Where(u => u.Status == 0).Count() > 0)
                        {
                            return "Load存在未称重的包装，请检查！";
                        }


                        List<LoadMaster> loadMasterList = idal.ILoadMasterDAL.SelectBy(u => u.LoadId == loadId && u.WhCode == whCode);
                        if (loadMasterList.Count > 0)
                        {
                            LoadMaster load = loadMasterList.First();
                            List<FlowDetail> flowDetailList = idal.IFlowDetailDAL.SelectBy(u => u.FlowHeadId == load.ProcessId);

                            FlowDetail detail = flowDetailList.Where(u => u.Type == "Shipping").First();
                            if (detail.FunctionName.Contains("集装箱发货"))
                            {
                                return "Load号的发货方式不属于交接！";
                            }
                        }

                        List<SortTaskDetailResult> SortTaskDetailList1 = (from sorttaskdetail in idal.ISortTaskDetailDAL.SelectAll()
                                                                          where
                                                                            sorttaskdetail.PlanQty == sorttaskdetail.PackQty
                                                                            && sorttaskdetail.LoadId == loadId
                                                                            && sorttaskdetail.WhCode == whCode
                                                                          group sorttaskdetail by new
                                                                          {
                                                                              sorttaskdetail.LoadId,
                                                                              sorttaskdetail.GroupId,
                                                                              sorttaskdetail.OutPoNumber,
                                                                              sorttaskdetail.GroupNumber
                                                                          } into g
                                                                          select new SortTaskDetailResult
                                                                          {
                                                                              LoadId = g.Key.LoadId,
                                                                              GroupId = (Int32?)g.Key.GroupId,
                                                                              OutPoNumber = g.Key.OutPoNumber,
                                                                              GroupNumber = g.Key.GroupNumber
                                                                          }).ToList();

                        if (SortTaskDetailList1.Count() == 0)
                        {
                            return "没有可交接的包装！";
                        }

                        List<SortTaskDetailResult> SortTaskDetailList = new List<SortTaskDetailResult>();

                        //验证是否有订单未完成包装
                        List<SortTaskDetail> getSortTaskDetailList = idal.ISortTaskDetailDAL.SelectBy(u => u.WhCode == whCode && u.LoadId == loadId);

                        string result = "";
                        foreach (var item in SortTaskDetailList1)
                        {
                            List<SortTaskDetail> sortTaskDetailList = getSortTaskDetailList.Where(u => u.WhCode == whCode && u.LoadId == item.LoadId && u.GroupId == item.GroupId && u.GroupNumber == item.GroupNumber && u.OutPoNumber == item.OutPoNumber && u.PlanQty != u.PackQty).ToList();
                            if (sortTaskDetailList.Count > 0)
                            {
                                result = "Load存在订单未完成包装！";
                                break;
                            }
                            else
                            {
                                SortTaskDetailList.Add(item);
                            }
                        }

                        if (result != "")
                        {
                            return result;
                        }

                        PackTask packTask = PackTaskList.First();

                        //1.根据输入的交接框号 验证是否已创建了交接任务
                        List<TransferTask> TransferTaskList = idal.ITransferTaskDAL.SelectBy(u => u.TransferNumber == transferNumber && u.WhCode == whCode);
                        TransferTask transferTask = new TransferTask();
                        if (TransferTaskList.Count == 0)
                        {
                            transferTask.WhCode = whCode;
                            transferTask.TransferId = "TF" + DI.IDGenerator.NewId;
                            transferTask.TransferNumber = transferNumber;
                            transferTask.TransportType = packTask.TransportType;
                            transferTask.express_code = packTask.express_code;
                            transferTask.express_type = packTask.express_type;
                            transferTask.express_type_zh = packTask.express_type_zh;
                            transferTask.Status = 0;
                            transferTask.CreateUser = userName;
                            transferTask.CreateDate = DateTime.Now;
                            idal.ITransferTaskDAL.Add(transferTask);
                        }
                        else
                        {
                            if (TransferTaskList.Where(u => u.Status == 0).Count() == 0)
                            {
                                transferTask.WhCode = whCode;
                                transferTask.TransferId = "TF" + DI.IDGenerator.NewId;
                                transferTask.TransferNumber = transferNumber;
                                transferTask.TransportType = packTask.TransportType;
                                transferTask.express_code = packTask.express_code;
                                transferTask.express_type = packTask.express_type;
                                transferTask.express_type_zh = packTask.express_type_zh;
                                transferTask.Status = 0;
                                transferTask.CreateUser = userName;
                                transferTask.CreateDate = DateTime.Now;
                                idal.ITransferTaskDAL.Add(transferTask);
                            }
                            else
                            {
                                transferTask = TransferTaskList.Where(u => u.Status == 0).First();
                                List<TransferHead> GetTransferHeadCount = idal.ITransferHeadDAL.SelectBy(u => u.TransferTaskId == transferTask.Id);

                                if (GetTransferHeadCount.Count == 0)
                                {
                                    transferTask.express_code = packTask.express_code;
                                    transferTask.express_type = packTask.express_type;
                                    transferTask.express_type_zh = packTask.express_type_zh;
                                    idal.ITransferTaskDAL.UpdateBy(transferTask, u => u.Id == transferTask.Id, new string[] { "express_code", "express_type", "express_type_zh" });
                                }
                            }
                        }

                        //2. 如果创建过交接任务 比较多次输入的 出库方式/快递公司 是否一致
                        if (packTask.express_code != transferTask.express_code)
                        {
                            return "出库单对应的快递公司或出库方式必须相同！";
                        }
                        idal.ITransferTaskDAL.SaveChanges();

                        #endregion

                        if (string.IsNullOrEmpty(workFlag))
                        {
                            #region 扫描枪使用交接时，需要用到以下插入

                            List<TransferHead> transferHeadList = new List<TransferHead>();
                            List<TransferDetail> transferDetailList = new List<TransferDetail>();
                            List<TransferScanNumber> transferScanNumberList = new List<TransferScanNumber>();

                            int count = 0;

                            List<PackTask> getPackTaskList = idal.IPackTaskDAL.SelectBy(u => u.WhCode == whCode && u.LoadId == loadId);

                            List<PackHead> headListResult = new List<PackHead>();
                            foreach (var sortTask in SortTaskDetailList)
                            {
                                PackTask pack = getPackTaskList.Where(u => u.WhCode == whCode && u.LoadId == sortTask.LoadId && u.SortGroupId == sortTask.GroupId && u.SortGroupNumber == sortTask.GroupNumber && u.OutPoNumber == sortTask.OutPoNumber).First();

                                if (pack.express_code != transferTask.express_code)
                                {
                                    continue;
                                }

                                if (pack.Status == -10 || pack.Status == 0)
                                {
                                    continue;
                                }
                                else
                                {
                                    //3.根据出库单号 获取包装信息后 插入交接表
                                    List<PackHead> PackHeadList = idal.IPackHeadDAL.SelectBy(u => u.PackTaskId == pack.Id && u.WhCode == pack.WhCode && u.Status != -10 && (u.TransferHeadId ?? "") == "");

                                    //4.获取包装款号明细 插入交接表
                                    List<PackDetail> packDetailList = (from a in idal.IPackTaskDAL.SelectAll()
                                                                       join b in idal.IPackHeadDAL.SelectAll()
                                                                       on a.Id equals b.PackTaskId
                                                                       join c in idal.IPackDetailDAL.SelectAll()
                                                                       on b.Id equals c.PackHeadId
                                                                       where a.Id == pack.Id && b.Status != -10 && (b.TransferHeadId ?? "") == "" && a.WhCode == pack.WhCode
                                                                       select c).Distinct().ToList();

                                    List<ItemMaster> itemMasterList = (from a in idal.IPackTaskDAL.SelectAll()
                                                                       join b in idal.IPackHeadDAL.SelectAll()
                                                                       on a.Id equals b.PackTaskId
                                                                       join c in idal.IPackDetailDAL.SelectAll()
                                                                       on b.Id equals c.PackHeadId
                                                                       join d in idal.IItemMasterDAL.SelectAll()
                                                                       on c.ItemId equals d.Id
                                                                       where a.Id == pack.Id && b.Status != -10 && (b.TransferHeadId ?? "") == "" && a.WhCode == pack.WhCode
                                                                       select d).Distinct().ToList();

                                    //5.获取包装款号扫描 插入交接表
                                    List<PackScanNumber> packScanNumberList = (from a in idal.IPackTaskDAL.SelectAll()
                                                                               join b in idal.IPackHeadDAL.SelectAll()
                                                                               on a.Id equals b.PackTaskId
                                                                               join c in idal.IPackDetailDAL.SelectAll()
                                                                               on b.Id equals c.PackHeadId
                                                                               join d in idal.IPackScanNumberDAL.SelectAll()
                                                                               on c.Id equals d.PackDetailId
                                                                               where a.Id == pack.Id && b.Status != -10 && (b.TransferHeadId ?? "") == "" && a.WhCode == pack.WhCode
                                                                               select d).Distinct().ToList();

                                    foreach (var packHead in PackHeadList)
                                    {
                                        if (result != "")
                                        {
                                            break;
                                        }

                                        string TransferSystemNumber = "TS" + DI.IDGenerator.NewId;

                                        TransferHead transferHead = new TransferHead();
                                        transferHead.WhCode = whCode;
                                        transferHead.TransferTaskId = transferTask.Id;
                                        transferHead.TransferSystemNumber = TransferSystemNumber;
                                        transferHead.LoadId = pack.LoadId;
                                        transferHead.SortGroupId = pack.SortGroupId;
                                        transferHead.SortGroupNumber = pack.SortGroupNumber;
                                        transferHead.CustomerOutPoNumber = pack.CustomerOutPoNumber;
                                        transferHead.OutPoNumber = pack.OutPoNumber;
                                        transferHead.PackGroupId = packHead.PackGroupId;
                                        transferHead.PackNumber = packHead.PackNumber;
                                        transferHead.ExpressNumber = packHead.ExpressNumber;
                                        transferHead.Length = packHead.Length;
                                        transferHead.Width = packHead.Width;
                                        transferHead.Height = packHead.Height;
                                        transferHead.Weight = packHead.Weight;
                                        transferHead.PackCarton = packHead.PackCarton;
                                        transferHead.Status = 10;
                                        transferHead.CreateUser = userName;
                                        transferHead.CreateDate = DateTime.Now;
                                        transferHeadList.Add(transferHead);

                                        //idal.ITransferHeadDAL.SaveChanges();

                                        //修改包装
                                        PackHead updatePackHead = new PackHead();
                                        updatePackHead.Id = packHead.Id;
                                        updatePackHead.TransferSystemNumber = TransferSystemNumber;
                                        updatePackHead.UpdateUser = userName;
                                        updatePackHead.UpdateDate = DateTime.Now;
                                        headListResult.Add(updatePackHead);
                                        //idal.IPackHeadDAL.UpdateBy(updatePackHead, u => u.Id == packHead.Id, new string[] { "TransferSystemNumber", "UpdateUser", "UpdateDate" });

                                        foreach (var item in packDetailList.Where(u => u.PackHeadId == packHead.Id))
                                        {
                                            List<ItemMaster> getitemMaster = itemMasterList.Where(u => u.Id == item.ItemId).ToList();
                                            if (getitemMaster.Count == 0)
                                            {
                                                result = "校验款号基础数据有误，款号ID不存在！";
                                                break;
                                            }

                                            ItemMaster itemMaster = getitemMaster.First();

                                            TransferDetail transferDetail = new TransferDetail();
                                            transferDetail.WhCode = whCode;
                                            transferDetail.TransferHeadId = 0;
                                            transferDetail.TransferSystemNumber = TransferSystemNumber;
                                            transferDetail.ItemId = item.ItemId;
                                            transferDetail.AltItemNumber = itemMaster.AltItemNumber;
                                            transferDetail.PlanQty = item.PlanQty;
                                            transferDetail.Qty = item.Qty;
                                            transferDetail.CreateUser = userName;
                                            transferDetail.CreateDate = DateTime.Now;
                                            transferDetailList.Add(transferDetail);

                                            foreach (var item1 in packScanNumberList.Where(u => u.PackDetailId == item.Id))
                                            {
                                                TransferScanNumber transferScanNumber = new TransferScanNumber();
                                                transferScanNumber.WhCode = whCode;
                                                transferScanNumber.TransferHeadId = 0;
                                                transferScanNumber.TransferSystemNumber = TransferSystemNumber;
                                                transferScanNumber.ItemId = item.ItemId;
                                                transferScanNumber.AltItemNumber = itemMaster.AltItemNumber;
                                                transferScanNumber.ScanNumber = item1.ScanNumber;
                                                transferScanNumber.CreateUser = userName;
                                                transferScanNumber.CreateDate = DateTime.Now;
                                                transferScanNumberList.Add(transferScanNumber);
                                            }
                                        }

                                        count++;
                                    }
                                }
                            }
                            if (result != "")
                            {
                                return result;
                            }

                            if (count == 0)
                            {
                                return "没有需要交接的包装！";
                            }

                            idal.ITransferHeadDAL.Add(transferHeadList);
                            idal.ITransferDetailDAL.Add(transferDetailList);
                            idal.ITransferScanNumberDAL.Add(transferScanNumberList);

                            idal.SaveChanges();

                            foreach (var item in headListResult)
                            {
                                idal.IPackHeadDAL.UpdateByExtended(u => u.Id == item.Id, t => new PackHead { TransferSystemNumber = item.TransferSystemNumber.ToString(), UpdateUser = item.UpdateUser, UpdateDate = item.UpdateDate });

                            }

                            string[] TransferSystemNumberList = (from a in transferHeadList
                                                                 select a.TransferSystemNumber).ToList().Distinct().ToArray();

                            //根据TransferSystemNumber 修改交接详细表、交接扫描表、包装Head表 中的TransferHeadId
                            List<TransferHead> getTransferHeadList = (from b in idal.ITransferHeadDAL.SelectAll()
                                                                      where b.WhCode == whCode && TransferSystemNumberList.Contains(b.TransferSystemNumber)
                                                                      select b).ToList();
                            foreach (var item in transferHeadList)
                            {
                                TransferHead getTransferHead = getTransferHeadList.Where(u => u.WhCode == item.WhCode && u.TransferSystemNumber == item.TransferSystemNumber).First();

                                idal.ITransferDetailDAL.UpdateByExtended(u => u.WhCode == item.WhCode && u.TransferSystemNumber == item.TransferSystemNumber, t => new TransferDetail { TransferHeadId = getTransferHead.Id });

                                idal.ITransferScanNumberDAL.UpdateByExtended(u => u.WhCode == item.WhCode && u.TransferSystemNumber == item.TransferSystemNumber, t => new TransferScanNumber { TransferHeadId = getTransferHead.Id });

                                idal.IPackHeadDAL.UpdateByExtended(u => u.WhCode == item.WhCode && u.TransferSystemNumber == item.TransferSystemNumber, t => new PackHead { TransferHeadId = getTransferHead.Id.ToString() });
                            }
                            #endregion
                        }

                        trans.Complete();
                        return "Y" + transferTask.Id;
                    }
                    catch
                    {
                        trans.Dispose();//出现异常，事务手动释放
                        return "交接异常，请重新提交！";
                    }
                }
            }
        }


        //根据交接框号 删除交接
        public string TransferTaskDelete(int transferTaskId)
        {
            using (TransactionScope trans = new TransactionScope())
            {
                try
                {
                    List<TransferTask> TransferTaskList = idal.ITransferTaskDAL.SelectBy(u => u.Id == transferTaskId);
                    if (TransferTaskList.Count == 0)
                    {
                        return "未找到交接信息，请查询！";
                    }

                    TransferTask transferTask = TransferTaskList.First();
                    if (transferTask.Status == 30)
                    {
                        return "当前交接任务已完成交接！";
                    }

                    //拦截交接中的快递单
                    List<TransferHead> TransferHeadList = idal.ITransferHeadDAL.SelectBy(u => u.TransferTaskId == transferTask.Id);

                    if (TransferHeadList.Count != 0)
                    {
                        string checkResult = "";
                        foreach (var item in TransferHeadList.Where(u => u.Status == -10))
                        {
                            checkResult += "快递单号：" + item.ExpressNumber + "被拦截！";
                        }
                        if (checkResult != "")
                        {
                            return checkResult;
                        }

                        string result = "";
                        foreach (var item in TransferHeadList)
                        {
                            result = TransferHeadDelete(item.Id);
                            if (result != "Y")
                            {
                                break;
                            }
                        }
                        if (result != "Y")
                        {
                            return result;
                        }
                    }

                    //添加交接日志
                    TranLog tl4 = new TranLog();
                    tl4.TranType = "430";
                    tl4.Description = "删除交接";
                    tl4.TranDate = DateTime.Now;
                    tl4.TranUser = "仓库删除";
                    tl4.WhCode = transferTask.WhCode;
                    tl4.Remark = "交接号：" + transferTask.TransferId;
                    idal.ITranLogDAL.Add(tl4);

                    idal.ITransferTaskDAL.DeleteBy(u => u.Id == transferTaskId);
                    idal.ITransferTaskDAL.SaveChanges();
                    trans.Complete();
                    return "Y";
                }
                catch
                {
                    trans.Dispose();//出现异常，事务手动释放
                    return "删除异常，请重新提交！";
                }
            }
        }


        //根据客户出库单号  删除交接
        public string TransferHeadDeleteByOrder(int transferTaskId, string customerOutPoNumber)
        {
            List<TransferTask> TransferTaskList = idal.ITransferTaskDAL.SelectBy(u => u.Id == transferTaskId);
            if (TransferTaskList.Count == 0)
            {
                return "未找到交接信息，请查询！";
            }

            TransferTask transferTask = TransferTaskList.First();
            if (transferTask.Status == 30)
            {
                return "当前交接任务已完成交接！";
            }

            List<TransferHead> TransferHeadList = idal.ITransferHeadDAL.SelectBy(u => u.TransferTaskId == transferTaskId && u.CustomerOutPoNumber == customerOutPoNumber);

            if (TransferHeadList.Count != 0)
            {
                string result = "";
                foreach (var item in TransferHeadList)
                {
                    result = TransferHeadDelete(item.Id);
                    if (result != "Y")
                    {
                        break;
                    }
                }
                if (result != "Y")
                {
                    return result;
                }
            }

            return "Y";
        }



        /// <summary>
        /// /-----------------------------------交接信息查询部分
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>

        //查询交接任务
        public List<TransferTaskResult> GetTransferTaskList(TransferTaskSearch entity, out int total)
        {
            var sql = from a in idal.ITransferTaskDAL.SelectAll()
                      join b in (
                          (from transferhead in idal.ITransferHeadDAL.SelectAll()
                           where transferhead.WhCode == entity.WhCode
                           group transferhead by new
                           {
                               transferhead.TransferTaskId
                           } into g
                           select new
                           {
                               TransferTaskId = (Int32?)g.Key.TransferTaskId,
                               expressCount = (Int32?)g.Count(p => p.ExpressNumber != null)
                           })) on new { Id = a.Id } equals new { Id = (Int32)b.TransferTaskId } into b_join
                      from b in b_join.DefaultIfEmpty()
                      join c in (
                          (from a0 in idal.ITransferHeadDAL.SelectAll()
                           join b1 in idal.ITransferDetailDAL.SelectAll() on new { Id = a0.Id } equals new { Id = (Int32)b1.TransferHeadId } into b1_join
                           from b1 in b1_join.DefaultIfEmpty()
                           where a0.WhCode == entity.WhCode
                           group new { a0, b1 } by new
                           {
                               a0.TransferTaskId
                           } into g
                           select new
                           {
                               TransferTaskId = (Int32?)g.Key.TransferTaskId,
                               qty = (Int32?)g.Sum(p => p.b1.Qty)
                           })) on new { Id = a.Id } equals new { Id = (Int32)c.TransferTaskId } into c_join
                      from c in c_join.DefaultIfEmpty()
                      where a.WhCode == entity.WhCode
                      select new TransferTaskResult
                      {
                          Id = a.Id,
                          TransferId = a.TransferId,
                          TransferNumber = a.TransferNumber,
                          express_code = a.express_code,
                          express_type_zh = a.express_type_zh,
                          Status0 =
                          a.Status == -10 ? "被拦截" :
                          a.Status == 0 ? "未交接" :
                          a.Status == 10 ? "正在交接" :
                          a.Status == 30 ? "交接完成" : null,
                          CreateUser = a.CreateUser,
                          CreateDate = a.CreateDate,
                          UpdateUser = a.UpdateUser,
                          UpdateDate = a.UpdateDate,
                          ExpressCount = b.expressCount,
                          SumQty = c.qty
                      };

            sql = sql.Where(u => (u.ExpressCount ?? 0) > 0);

            if (!string.IsNullOrEmpty(entity.CreateUser))
                sql = sql.Where(u => u.CreateUser == entity.CreateUser);
            if (!string.IsNullOrEmpty(entity.TransferId))
            {
                sql = sql.Where(u => u.TransferId == entity.TransferId);
            }
            if (!string.IsNullOrEmpty(entity.TransferNumber))
            {
                sql = sql.Where(u => u.TransferNumber == entity.TransferNumber);
            }
            if (entity.CreateDateBegin != null)
            {
                sql = sql.Where(u => u.CreateDate >= entity.CreateDateBegin);
            }
            if (entity.CreateDateEnd != null)
            {
                sql = sql.Where(u => u.CreateDate < entity.CreateDateEnd);
            }

            if (!string.IsNullOrEmpty(entity.LoadId) || !string.IsNullOrEmpty(entity.ExpressNumber) || !string.IsNullOrEmpty(entity.AltItemNumber))
            {
                var sql1 = (from a in idal.ITransferTaskDAL.SelectAll()
                            join b in idal.ITransferHeadDAL.SelectAll() on new { Id = a.Id } equals new { Id = (Int32)b.TransferTaskId }
                            join c in idal.ITransferDetailDAL.SelectAll() on new { Id = b.Id } equals new { Id = (Int32)c.TransferHeadId }
                            where a.WhCode == entity.WhCode
                            select new
                            {
                                a.TransferId,
                                a.TransferNumber,
                                a.CreateDate,
                                b.LoadId,
                                b.ExpressNumber,
                                c.AltItemNumber
                            }).Distinct();
                if (!string.IsNullOrEmpty(entity.TransferId))
                {
                    sql1 = sql1.Where(u => u.TransferId == entity.TransferId);
                }
                if (!string.IsNullOrEmpty(entity.TransferNumber))
                {
                    sql1 = sql1.Where(u => u.TransferNumber == entity.TransferNumber);
                }
                if (!string.IsNullOrEmpty(entity.LoadId))
                {
                    sql1 = sql1.Where(u => u.LoadId == entity.LoadId);
                }
                if (!string.IsNullOrEmpty(entity.ExpressNumber))
                {
                    sql1 = sql1.Where(u => u.ExpressNumber.Contains(entity.ExpressNumber));
                }
                if (!string.IsNullOrEmpty(entity.AltItemNumber))
                {
                    sql1 = sql1.Where(u => u.AltItemNumber.Contains(entity.AltItemNumber));
                }
                if (entity.CreateDateBegin != null)
                {
                    sql1 = sql1.Where(u => u.CreateDate >= entity.CreateDateBegin);
                }
                if (entity.CreateDateEnd != null)
                {
                    sql1 = sql1.Where(u => u.CreateDate < entity.CreateDateEnd);
                }

                string[] transferid = (from a in sql1
                                       select a.TransferId).Distinct().ToArray();

                sql = sql.Where(u => transferid.Contains(u.TransferId));
            }
            total = sql.Count();
            sql = sql.OrderByDescending(u => u.CreateDate);
            sql = sql.Skip(entity.pageSize * (entity.pageIndex - 1)).Take(entity.pageSize);

            return sql.ToList();
        }

        //查询交接任务
        public List<TransferTaskResult> GetTransferTaskOrderList(TransferTaskSearch entity, out int total)
        {
            var sql = from a in idal.ITransferTaskDAL.SelectAll()
                      join b in (
                          (from transferhead in idal.ITransferHeadDAL.SelectAll()
                           where transferhead.WhCode == entity.WhCode
                           group transferhead by new
                           {
                               transferhead.TransferTaskId
                           } into g
                           select new
                           {
                               TransferTaskId = (Int32?)g.Key.TransferTaskId,
                               expressCount = (Int32?)g.Count(p => p.ExpressNumber != null)
                           })) on new { Id = a.Id } equals new { Id = (Int32)b.TransferTaskId } into b_join
                      from b in b_join.DefaultIfEmpty()
                      join c in (
                          (from a0 in idal.ITransferHeadDAL.SelectAll()
                           join b1 in idal.ITransferDetailDAL.SelectAll() on new { Id = a0.Id } equals new { Id = (Int32)b1.TransferHeadId } into b1_join
                           from b1 in b1_join.DefaultIfEmpty()
                           where a0.WhCode == entity.WhCode
                           group new { a0, b1 } by new
                           {
                               a0.TransferTaskId
                           } into g
                           select new
                           {
                               TransferTaskId = (Int32?)g.Key.TransferTaskId,
                               qty = (Int32?)g.Sum(p => p.b1.Qty)
                           })) on new { Id = a.Id } equals new { Id = (Int32)c.TransferTaskId } into c_join
                      from c in c_join.DefaultIfEmpty()
                      where a.WhCode == entity.WhCode
                      select new TransferTaskResult
                      {
                          Id = a.Id,
                          TransferId = a.TransferId,
                          TransferNumber = a.TransferNumber,
                          Status0 =
                          a.Status == -10 ? "被拦截" :
                          a.Status == 0 ? "未交接" :
                          a.Status == 10 ? "正在交接" :
                          a.Status == 30 ? "交接完成" : null,
                          CreateUser = a.CreateUser,
                          CreateDate = a.CreateDate,
                          UpdateUser = a.UpdateUser,
                          UpdateDate = a.UpdateDate,
                          ExpressCount = b.expressCount,
                          SumQty = c.qty
                      };

            sql = sql.Where(u => (u.ExpressCount ?? 0) == 0);

            if (!string.IsNullOrEmpty(entity.CreateUser))
                sql = sql.Where(u => u.CreateUser == entity.CreateUser);
            if (!string.IsNullOrEmpty(entity.TransferId))
                sql = sql.Where(u => u.TransferId == entity.TransferId);
            if (!string.IsNullOrEmpty(entity.TransferNumber))
                sql = sql.Where(u => u.TransferNumber == entity.TransferNumber);
            if (entity.CreateDateBegin != null)
            {
                sql = sql.Where(u => u.CreateDate >= entity.CreateDateBegin);
            }
            if (entity.CreateDateEnd != null)
            {
                sql = sql.Where(u => u.CreateDate < entity.CreateDateEnd);
            }

            if (!string.IsNullOrEmpty(entity.LoadId) || !string.IsNullOrEmpty(entity.ExpressNumber) || !string.IsNullOrEmpty(entity.AltItemNumber))
            {
                var sql1 = (from a in idal.ITransferTaskDAL.SelectAll()
                            join b in idal.ITransferHeadDAL.SelectAll() on new { Id = a.Id } equals new { Id = (Int32)b.TransferTaskId }
                            join c in idal.ITransferDetailDAL.SelectAll() on new { Id = b.Id } equals new { Id = (Int32)c.TransferHeadId }
                            where a.WhCode == entity.WhCode
                            select new
                            {
                                a.TransferId,
                                a.TransferNumber,
                                a.CreateDate,
                                b.LoadId,
                                b.ExpressNumber,
                                c.AltItemNumber
                            }).Distinct();
                if (!string.IsNullOrEmpty(entity.TransferId))
                {
                    sql1 = sql1.Where(u => u.TransferId == entity.TransferId);
                }
                if (!string.IsNullOrEmpty(entity.TransferNumber))
                {
                    sql1 = sql1.Where(u => u.TransferNumber == entity.TransferNumber);
                }
                if (!string.IsNullOrEmpty(entity.LoadId))
                {
                    sql1 = sql1.Where(u => u.LoadId == entity.LoadId);
                }
                if (!string.IsNullOrEmpty(entity.ExpressNumber))
                {
                    sql1 = sql1.Where(u => u.ExpressNumber.Contains(entity.ExpressNumber));
                }
                if (!string.IsNullOrEmpty(entity.AltItemNumber))
                {
                    sql1 = sql1.Where(u => u.AltItemNumber.Contains(entity.AltItemNumber));
                }
                if (entity.CreateDateBegin != null)
                {
                    sql1 = sql1.Where(u => u.CreateDate >= entity.CreateDateBegin);
                }
                if (entity.CreateDateEnd != null)
                {
                    sql1 = sql1.Where(u => u.CreateDate < entity.CreateDateEnd);
                }

                string[] transferid = (from a in sql1
                                       select a.TransferId).Distinct().ToArray();

                sql = sql.Where(u => transferid.Contains(u.TransferId));
            }

            total = sql.Count();
            sql = sql.OrderByDescending(u => u.CreateDate);
            sql = sql.Skip(entity.pageSize * (entity.pageIndex - 1)).Take(entity.pageSize);
            return sql.ToList();
        }

        //查询 交接任务的交接明细
        public List<TransferHeadDetailResult> GetTransferTaskDetailList(TransferHeadDetailSearch searchEntity, out int total)
        {
            var sql = from a in idal.ITransferTaskDAL.SelectAll()
                      join b in idal.ITransferHeadDAL.SelectAll()
                      on a.Id equals b.TransferTaskId
                      join c in idal.ITransferDetailDAL.SelectAll()
                      on b.Id equals c.TransferHeadId
                      where a.Id == searchEntity.Id
                      select new TransferHeadDetailResult
                      {
                          Id = b.Id,
                          TransferNumber = a.TransferNumber,
                          LoadId = b.LoadId,
                          SortGroupNumber = b.SortGroupNumber,
                          CustomerOutPoNumber = b.CustomerOutPoNumber,
                          OutPoNumber = b.OutPoNumber,
                          PackGroupNumber = b.PackNumber,
                          ExpressNumber = b.ExpressNumber,
                          AltItemNumber = c.AltItemNumber,
                          Qty = c.Qty,
                          Length = b.Length,
                          Width = b.Width,
                          Height = b.Height,
                          Weight = b.Weight,
                          PackCarton = b.PackCarton,
                          Status1 =
                          b.Status == -10 ? "被拦截" :
                          b.Status == 10 ? "正常" : null,
                          CreateUser = b.CreateUser,
                          CreateDate = b.CreateDate
                      };

            List<TransferHeadDetailResult> list = new List<TransferHeadDetailResult>();
            foreach (var item in sql)
            {
                if (list.Where(u => u.Id == item.Id).Count() == 0)
                {
                    TransferHeadDetailResult result = new TransferHeadDetailResult();
                    result.Id = item.Id;
                    result.TransferNumber = item.TransferNumber;
                    result.LoadId = item.LoadId;
                    result.SortGroupNumber = item.SortGroupNumber;
                    result.CustomerOutPoNumber = item.CustomerOutPoNumber;
                    result.OutPoNumber = item.OutPoNumber;
                    result.PackGroupNumber = item.PackGroupNumber ?? "";
                    result.ExpressNumber = item.ExpressNumber ?? "";
                    result.AltItemNumber = item.AltItemNumber;
                    result.Qty = item.Qty;
                    result.Length = item.Length;
                    result.Width = item.Width;
                    result.Height = item.Height;
                    result.Weight = item.Weight;
                    result.PackCarton = item.PackCarton;
                    result.Status1 = item.Status1;
                    result.CreateUser = item.CreateUser;
                    result.CreateDate = item.CreateDate;
                    list.Add(result);
                }
                else
                {
                    TransferHeadDetailResult getModel = list.Where(u => u.Id == item.Id).First();
                    list.Remove(getModel);

                    TransferHeadDetailResult result = new TransferHeadDetailResult();
                    result.Id = item.Id;
                    result.TransferNumber = item.TransferNumber;
                    result.LoadId = item.LoadId;
                    result.SortGroupNumber = item.SortGroupNumber;
                    result.CustomerOutPoNumber = item.CustomerOutPoNumber;
                    result.OutPoNumber = item.OutPoNumber;
                    result.PackGroupNumber = item.PackGroupNumber ?? "";
                    result.ExpressNumber = item.ExpressNumber ?? "";
                    result.Length = item.Length;
                    result.Width = item.Width;
                    result.Height = item.Height;
                    result.Weight = item.Weight;
                    result.PackCarton = item.PackCarton;
                    result.Status1 = item.Status1;
                    result.CreateUser = item.CreateUser;
                    result.CreateDate = item.CreateDate;
                    result.Qty = getModel.Qty + item.Qty;
                    result.AltItemNumber = getModel.AltItemNumber + "," + item.AltItemNumber;
                    list.Add(result);
                }
            }

            if (!string.IsNullOrEmpty(searchEntity.ExpressNumber))
            {
                list = list.Where(u => (u.ExpressNumber == null ? "" : u.ExpressNumber).Contains(searchEntity.ExpressNumber)).ToList();
            }
            if (!string.IsNullOrEmpty(searchEntity.LoadId))
            {
                list = list.Where(u => u.LoadId == searchEntity.LoadId).ToList();
            }
            if (!string.IsNullOrEmpty(searchEntity.AltItemNumber))
            {
                list = list.Where(u => u.AltItemNumber.Contains(searchEntity.AltItemNumber)).ToList();
            }
            if (!string.IsNullOrEmpty(searchEntity.PackCarton))
            {
                list = list.Where(u => u.PackCarton == searchEntity.PackCarton).ToList();
            }

            total = list.Count;
            list = list.OrderByDescending(u => u.Id).ToList();
            list = list.Skip(searchEntity.pageSize * (searchEntity.pageIndex - 1)).Take(searchEntity.pageSize).ToList();
            return list;
        }


        //删除交接头信息
        public string TransferHeadDelete(int id)
        {
            List<TransferHead> transferHeadList = idal.ITransferHeadDAL.SelectBy(u => u.Id == id);
            if (transferHeadList.Count == 0)
            {
                return "信息不存在，请重新查询交接信息！";
            }

            TransferHead transferHead = transferHeadList.First();

            List<TransferTask> transferTaskList = idal.ITransferTaskDAL.SelectBy(u => u.Id == transferHead.TransferTaskId);
            if (transferTaskList.Count == 0)
            {
                return "信息不存在，请重新查询交接信息！";
            }

            List<OutBoundOrder> checkOutBoundOrder = idal.IOutBoundOrderDAL.SelectBy(u => u.WhCode == transferHead.WhCode && u.OutPoNumber == transferHead.OutPoNumber && u.StatusId == 40);
            if (checkOutBoundOrder.Count > 0)
            {
                OutBoundOrder outOrder = checkOutBoundOrder.First();
                return "出货订单" + outOrder.CustomerOutPoNumber + "已发货，无法删除该交接！";
            }

            TransferTask transferTask = transferTaskList.First();
            if (transferTask.Status == 30)
            {
                return "当前交接任务已完成交接！";
            }

            //string checkResult = "";
            //foreach (var item in transferHeadList.Where(u => u.Status == -10))
            //{
            //    checkResult += "快递单号：" + item.ExpressNumber + "被拦截！";
            //}
            //if (checkResult != "")
            //{
            //    return checkResult;
            //}

            TransferDelete(transferHeadList);
            idal.ITransferTaskDAL.SaveChanges();

            return "Y";
        }


        //删除交接明细方法
        public void TransferDelete(List<TransferHead> transferHeadList)
        {
            foreach (var transferHead in transferHeadList)
            {
                string tHeadId = transferHead.Id.ToString();

                //修改包装
                List<PackHead> updatePackHeadList = idal.IPackHeadDAL.SelectBy(u => u.WhCode == transferHead.WhCode && u.ExpressNumber == transferHead.ExpressNumber && u.TransferHeadId == tHeadId);
                if (updatePackHeadList.Count > 0)
                {
                    PackHead updatePackHead = updatePackHeadList.First();
                    updatePackHead.TransferHeadId = "";
                    idal.IPackHeadDAL.UpdateBy(updatePackHead, u => u.Id == updatePackHead.Id, new string[] { "TransferHeadId" });
                }

                //删除明细对应的扫描
                idal.ITransferScanNumberDAL.DeleteBy(u => u.TransferHeadId == transferHead.Id);

                idal.ITransferDetailDAL.DeleteBy(u => u.TransferHeadId == transferHead.Id);

                idal.ITransferHeadDAL.DeleteBy(u => u.Id == transferHead.Id);
            }
        }


        public TransferTaskResultEcl GetTransferTaskEclResult(string whCode, string transferId)
        {
            TransferTask transferTask = idal.ITransferTaskDAL.SelectBy(u => u.WhCode == whCode && u.TransferId == transferId).First();

            TransferTaskResultEcl transferTaskResult = new TransferTaskResultEcl();
            transferTaskResult.WhCode = transferTask.WhCode;
            transferTaskResult.TransferId = transferTask.TransferId;
            transferTaskResult.TransferNumber = transferTask.TransferNumber;
            transferTaskResult.TransportType = transferTask.TransportType;

            transferTaskResult.express_code = transferTask.express_code;
            transferTaskResult.express_type = transferTask.express_type;
            transferTaskResult.express_type_zh = transferTask.express_type_zh;
            transferTaskResult.Status = transferTask.Status;
            transferTaskResult.CreateUser = transferTask.CreateUser;
            transferTaskResult.CreateDate = transferTask.CreateDate;

            List<TransferHeadResultEcl> transferHeadResultEclList = new List<TransferHeadResultEcl>();

            List<TransferHead> transferHeadList = idal.ITransferHeadDAL.SelectBy(u => u.TransferTaskId == transferTask.Id);
            foreach (var item in transferHeadList)
            {
                TransferHeadResultEcl transferHeadResultEcl = new TransferHeadResultEcl();
                transferHeadResultEcl.WhCode = item.WhCode;
                transferHeadResultEcl.TransferTaskId = item.TransferTaskId;
                transferHeadResultEcl.LoadId = item.LoadId;
                transferHeadResultEcl.SortGroupId = item.SortGroupId;
                transferHeadResultEcl.SortGroupNumber = item.SortGroupNumber;
                transferHeadResultEcl.CustomerOutPoNumber = item.CustomerOutPoNumber;
                transferHeadResultEcl.OutPoNumber = item.OutPoNumber;
                transferHeadResultEcl.PackGroupId = item.PackGroupId;
                transferHeadResultEcl.PackGroupNumber = item.PackNumber;
                transferHeadResultEcl.ExpressNumber = item.ExpressNumber;
                transferHeadResultEcl.ExpressNumberParent = item.ExpressNumberParent; //add by yangxin 2024-05-28 
                transferHeadResultEcl.Length = item.Length;
                transferHeadResultEcl.Width = item.Width;
                transferHeadResultEcl.Height = item.Height;
                transferHeadResultEcl.Weight = item.Weight;
                transferHeadResultEcl.PackCarton = item.PackCarton;

                List<TransferDetail> transferDetailList = idal.ITransferDetailDAL.SelectBy(u => u.TransferHeadId == item.Id);
                List<TransferScanNumber> transferScanNumberList = idal.ITransferScanNumberDAL.SelectBy(u => u.TransferHeadId == item.Id);

                List<TransferDetailResultEcl> transferDetailResultEclList = new List<TransferDetailResultEcl>();
                List<TransferScanNumberResultEcl> transferScanNumberResultEclList = new List<TransferScanNumberResultEcl>();

                foreach (var transferDetail in transferDetailList)
                {
                    ItemMaster itemMaster = idal.IItemMasterDAL.SelectBy(u => u.Id == transferDetail.ItemId).First();
                    TransferDetailResultEcl transferDetailResultEcl = new TransferDetailResultEcl();
                    transferDetailResultEcl.WhCode = transferDetail.WhCode;
                    transferDetailResultEcl.TransferHeadId = transferDetail.TransferHeadId;
                    transferDetailResultEcl.ItemId = transferDetail.ItemId;
                    transferDetailResultEcl.AltItemNumber = itemMaster.AltItemNumber;
                    transferDetailResultEcl.PlanQty = transferDetail.PlanQty;
                    transferDetailResultEcl.Qty = transferDetail.Qty;
                    transferDetailResultEclList.Add(transferDetailResultEcl);
                }

                foreach (var transferScanNumber in transferScanNumberList)
                {
                    ItemMaster itemMaster = idal.IItemMasterDAL.SelectBy(u => u.Id == transferScanNumber.ItemId).First();
                    TransferScanNumberResultEcl transferScanNumberResultEcl = new TransferScanNumberResultEcl();
                    transferScanNumberResultEcl.WhCode = transferScanNumber.WhCode;
                    transferScanNumberResultEcl.TransferHeadId = transferScanNumber.TransferHeadId;
                    transferScanNumberResultEcl.ItemId = transferScanNumber.ItemId;
                    transferScanNumberResultEcl.AltItemNumber = itemMaster.AltItemNumber;
                    transferScanNumberResultEcl.ScanNumber = transferScanNumber.ScanNumber;
                    transferScanNumberResultEclList.Add(transferScanNumberResultEcl);
                }

                transferHeadResultEcl.transferDetailResultEcl = transferDetailResultEclList;
                transferHeadResultEcl.transferScanNumberResultEcl = transferScanNumberResultEclList;
                transferHeadResultEclList.Add(transferHeadResultEcl);
            }

            transferTaskResult.transferHeadResultEcl = transferHeadResultEclList;

            return transferTaskResult;
        }


        //交接单水晶报表
        public List<TransferReportResult> GetTransferReportList(int tid)
        {
            List<TransferReportResult> sql1 = (from a in idal.ITransferTaskDAL.SelectAll()
                                               join b in idal.ITransferHeadDAL.SelectAll() on new { Id = a.Id } equals new { Id = (Int32)b.TransferTaskId } into b_join
                                               from b in b_join.DefaultIfEmpty()
                                               join c in idal.ITransferDetailDAL.SelectAll() on new { Id = b.Id } equals new { Id = (Int32)c.TransferHeadId } into c_join
                                               from c in c_join.DefaultIfEmpty()
                                               join d in idal.IWhUserDAL.SelectAll() on new { UpdateUser = a.UpdateUser } equals new { UpdateUser = d.UserCode } into d_join
                                               from d in d_join.DefaultIfEmpty()
                                               where a.Id == tid
                                               group new { a, b, c, d } by new
                                               {
                                                   a.TransferId,
                                                   a.TransportType,
                                                   a.express_code,
                                                   b.CustomerOutPoNumber,
                                                   b.LoadId,
                                                   b.PackNumber,
                                                   b.ExpressNumber,
                                                   b.Weight,
                                                   d.UserNameCN
                                               } into g
                                               select new TransferReportResult
                                               {
                                                   TransferId = g.Key.TransferId,
                                                   TransportType = g.Key.TransportType,
                                                   express_code = g.Key.express_code,
                                                   LoadId = g.Key.LoadId,
                                                   CustomerOutPoNumber = g.Key.CustomerOutPoNumber,
                                                   PackNumber = g.Key.PackNumber,
                                                   ExpressNumber = g.Key.ExpressNumber,
                                                   Weight = ((Decimal?)g.Key.Weight ?? (Decimal?)0),
                                                   Qty = g.Sum(p => ((Int32?)p.c.Qty ?? (Int32?)0)),
                                                   TransferUserName = g.Key.UserNameCN
                                               }).ToList();
            int packQty = 0;
            List<TransferReportResult> sql = new List<TransferReportResult>();
            foreach (var item in sql1)
            {
                string getpackNumber = item.PackNumber;
                if (getpackNumber.IndexOf('_') >= 0)
                {
                    string packNumber = getpackNumber.Substring(0, getpackNumber.IndexOf('_'));

                    item.PackNumber = packNumber;
                    if (sql.Where(u => u.PackNumber.StartsWith(packNumber)).Count() > 0)
                    {
                        item.Weight = 0;
                    }
                    else
                    {
                        packQty++;
                    }
                }
                else
                {
                    packQty++;
                }
                sql.Add(item);
            }

            List<TransferReportResult> sql3 = new List<TransferReportResult>();
            foreach (var item in sql)
            {
                item.PackQty = packQty;
                sql3.Add(item);
            }

            sql3 = sql3.OrderBy(u => u.LoadId).ThenBy(u => u.CustomerOutPoNumber).ThenBy(u => u.PackNumber).ToList();
            return sql3;
        }


        //快递单未交接查询
        public List<UnTransferExpressNumberResult> GetUnTransferExpressNumberList(UnTransferExpressNumberSearch entity, out int total)
        {
            var sql = from a in idal.IPackHeadDAL.SelectAll()
                      join b in idal.IPackTaskDAL.SelectAll()
                      on new { PackTaskId = (Int32)a.PackTaskId } equals new { PackTaskId = b.Id } into b_join
                      from b in b_join.DefaultIfEmpty()
                      join c in idal.IOutBoundOrderDAL.SelectAll()
                      on new { a = b.WhCode, b = b.OutPoNumber } equals new { a = c.WhCode, b = c.OutPoNumber }
                      where (a.TransferHeadId ?? "") == "" && (a.ExpressNumber ?? "") != ""
                      select new UnTransferExpressNumberResult
                      {
                          LoadId = b.LoadId,
                          ClientCode = c.ClientCode,
                          SortGroupNumber = b.SortGroupNumber,
                          PackGroupNumber = a.PackNumber,
                          ExpressNumber = a.ExpressNumber,
                          Weight = a.Weight,
                          PackCarton = a.PackCarton,
                          Status =
                          a.Status == -10 ? "被拦截" :
                          a.Status == 0 ? "未包装" :
                          a.Status == 10 ? "包装完成" :
                          a.Status == 20 ? "交接中" : null,
                          CreateUser = a.CreateUser,
                          CreateDate = a.CreateDate
                      };
            if (!string.IsNullOrEmpty(entity.ExpressNumber))
                sql = sql.Where(u => u.ExpressNumber == entity.ExpressNumber);
            if (!string.IsNullOrEmpty(entity.ClientCode))
                sql = sql.Where(u => u.ClientCode == entity.ClientCode);
            if (entity.CreateDateBegin != null)
            {
                sql = sql.Where(u => u.CreateDate >= entity.CreateDateBegin);
            }
            if (entity.CreateDateEnd != null)
            {
                sql = sql.Where(u => u.CreateDate < entity.CreateDateEnd);
            }

            total = sql.Count();
            sql = sql.OrderByDescending(u => u.CreateDate);
            sql = sql.Skip(entity.pageSize * (entity.pageIndex - 1)).Take(entity.pageSize);
            return sql.ToList();
        }


    }
}

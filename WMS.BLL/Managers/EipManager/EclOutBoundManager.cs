using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MODEL_MSSQL;
using WMS.BLLClass;
using WMS.IBLL;
using System.Collections;
using WMS.Express;
using System.Transactions;

namespace WMS.BLL
{
    public class EclOutBoundManager : IEclOutBoundManager
    {
        IDAL.IDALSession idal = BLL.BLLHelper.GetDal();
        IBLL.IOutBoundOrderManager wmsoutBound = new BLL.OutBoundOrderManager();

        #region 1.电商订单导入直接生成LOAD
        public string OutBoundLoadAddEcl(EclLoadModel entity)
        {
            OutBoundOrderManager obom = new OutBoundOrderManager();
            List<LoadDetail> loadDetailList = new List<LoadDetail>();
            Hashtable iu = new Hashtable(); //创建一个Hashtable 用于存放item 及uom

            LoadManager loadManager = new LoadManager();
            ReleaseLoadManager releaseLoadManager = new ReleaseLoadManager();

            List<ItemMaster> il;
            List<WhClient> wcl;
            int ClientId;
            string ReleaseResult;
            string Result;

            using (TransactionScope trans = new TransactionScope())
            {
                try
                {

                    #region 查找客户ID

                    wcl = idal.IWhClientDAL.SelectBy(u => u.ClientCode == entity.ClientCode && u.WhCode == entity.WhCode);
                    if (wcl.Count() > 0)
                    {
                        ClientId = wcl.First().Id;
                    }
                    else
                    {
                        return "失败!客户不存在!";
                    }

                    #endregion

                    #region 检查数据完整并更新itemID及UOM
                    if (entity.EclLoadId == "" || entity.EclLoadId == null)
                    {
                        return "失败!ECLLoadID!";
                    }
                    if (entity.EclOutOrderList.Count() == 0)
                    {
                        return "失败!无订单!";
                    }
                    foreach (var item in entity.EclOutOrderList)
                    {

                        if (idal.IOutBoundOrderDAL.SelectBy(u => u.CustomerOutPoNumber == item.outorder_number && u.ClientId == ClientId && u.WhCode == entity.WhCode).Count() > 0)
                        {
                            return "失败!" + item.outorder_number + "已经存在,无法导入!";
                        }
                        if (item.EclOutOrderDetailList.Count() == 0)
                        {
                            return "失败!" + item.outorder_number + "存在空明细订单!";
                        }
                        foreach (var item2 in item.EclOutOrderDetailList)
                        {

                            if (item2.qty == 0 || item2.item_number == "" || item2.item_number == null || item2.qty == null)
                            {
                                return "失败!" + item2.outorder_number + "明细缺少数据!";
                            }

                            //检查ITEM是否存在集合中
                            if (!iu.Contains(item2.item_number))
                            {
                                il = idal.IItemMasterDAL.SelectBy(u => u.AltItemNumber == item2.item_number && u.WhCode == entity.WhCode && u.ClientId == ClientId).OrderBy(u=>u.Id).ToList();
                                //判断查询结果输了
                                if (il.Count() > 0)
                                {
                                    //判断是否有UOM
                                    if (il.First().UnitName == "" || il.First().UnitName == null)
                                    {
                                        return "失败!" + item2.outorder_number + ":" + item2.item_number + "UOM为空!可能没有收货";
                                    }

                                    //更新所有这个ITEM的ID和UOM
                                    foreach (var oo in entity.EclOutOrderList)
                                    {
                                        foreach (var ooo in oo.EclOutOrderDetailList)
                                        {
                                            if (ooo.item_number == item2.item_number)
                                            {
                                                ooo.item_id = il.First().Id;
                                                ooo.uom = il.First().UnitName;
                                            }
                                        }
                                    }

                                    //插入集合
                                    iu.Add(item2.item_number, il.First().UnitName);
                                    //更新Model中的uom
                                    item2.uom = il.First().UnitName;
                                }
                                else
                                {
                                    return "失败!" + item2.outorder_number + ":" + item2.item_number + "不存在WMS中!";
                                }
                            }

                        }
                    }

                    #endregion

                    #region 创建Load表头

                    LoadMaster load = new LoadMaster();
                    load.ProcessId = entity.ProcessId;
                    load.ShipMode = "交接单";
                    load.ProcessName = entity.ProcessName;
                    load.WhCode = entity.WhCode;
                    load.CreateUser = entity.CreateUser;


                    load = loadManager.LoadMasterAdd(load);
                    idal.SaveChanges();

                    #endregion

                    #region 导入出库订单,并组织load明细
                    foreach (var item in entity.EclOutOrderList)
                    {

                        #region 添加OutBoundOrder  
                        OutBoundOrder outBoundOrder = new OutBoundOrder();
                        LoadDetail loadDetail = new LoadDetail();

                        outBoundOrder.WhCode = entity.WhCode;
                        outBoundOrder.OutPoNumber = "SA" + DI.IDGenerator.NewId;
                        outBoundOrder.CustomerOutPoNumber = item.outorder_number;
                        outBoundOrder.ClientId = ClientId;
                        outBoundOrder.ClientCode = entity.ClientCode;
                        outBoundOrder.ProcessId = entity.ProcessId;
                        outBoundOrder.NowProcessId = 3;
                        outBoundOrder.StatusId = 10;
                        outBoundOrder.StatusName = "已确认订单";


                        outBoundOrder.OrderSource = "ECL";
                        outBoundOrder.CreateUser = entity.CreateUser;
                        outBoundOrder.CreateDate = DateTime.Now;
                        outBoundOrder = idal.IOutBoundOrderDAL.Add(outBoundOrder);
                        idal.IOutBoundOrderDAL.SaveChanges();

                        //组织loaddetal明细
                        loadDetail.LoadMasterId = load.Id;
                        loadDetail.OutBoundOrderId = outBoundOrder.Id;
                        loadDetail.CreateUser = entity.CreateUser;
                        loadDetail.CreateDate = DateTime.Now;

                        loadDetailList.Add(loadDetail);
                        #endregion

                        #region 添加InBoundOrderDetail

                        foreach (var item2 in item.EclOutOrderDetailList)
                        {
                            OutBoundOrderDetail outBoundOrderDetail = new OutBoundOrderDetail();


                            outBoundOrderDetail.WhCode = entity.WhCode;
                            outBoundOrderDetail.OutBoundOrderId = outBoundOrder.Id;
                            outBoundOrderDetail.ItemId = (int)item2.item_id;
                            outBoundOrderDetail.AltItemNumber = item2.item_number;
                            outBoundOrderDetail.UnitName = item2.uom;
                            outBoundOrderDetail.UnitId = 0;
                            outBoundOrderDetail.Qty = item2.qty;
                            outBoundOrderDetail.LotNumber1 = item2.lot_number;
                            outBoundOrderDetail.CreateUser = entity.CreateUser;
                            outBoundOrderDetail.CreateDate = DateTime.Now;

                            idal.IOutBoundOrderDetailDAL.Add(outBoundOrderDetail);

                        }


                        #endregion
                    }


                    #endregion

                    #region 导入LOAD明细

                    loadManager.LoadDetailAdd(loadDetailList);

                    #endregion

                    #region 释放load
                    ReleaseResult = releaseLoadManager.CheckReleaseLoad(load.LoadId, load.WhCode, entity.CreateUser);
                    #endregion

                    if (ReleaseResult == "Y")
                    {
                        Result = ReleaseResult + "$" + load.LoadId;
                    }
                    else
                    {

                        Result = "N$" + ReleaseResult + "$" + load.LoadId;
                    }

                    idal.SaveChanges();
                    trans.Complete();
                    return Result;
                }
                catch
                {
                    trans.Dispose();//出现异常，事务手动释放
                    return "系统出现异常，请重新提交！";
                }
            }

        }

        public string OMSOutBoundOrderDel(string whCode, string customerOutPoNumber, string clientCode, string userName)
        {
            List<OutBoundOrder> OutBoundOrderList = idal.IOutBoundOrderDAL.SelectBy(u => u.WhCode == whCode && u.CustomerOutPoNumber == customerOutPoNumber && u.ClientCode == clientCode);
            return wmsoutBound.OutBoundOrderDel(OutBoundOrderList.First().Id);
        }

        public string OutBoundOrderAddOMS(EclOutOrderModel entity)
        {
            using (TransactionScope trans = new TransactionScope())
            {
                try
                {

                    #region 验证订单流程是否选择错误

                    List<FlowHead> flowHeadList = idal.IFlowHeadDAL.SelectBy(u => u.Id == entity.ProcessId);
                    if (flowHeadList.Count == 0)
                    {
                        return "失败，订单流程不存在！";
                    }
                    else
                    {
                        if (flowHeadList.Where(u => u.FlowName.Contains("单品")).Count() > 0)
                        {
                            int sumQty = 0;
                            foreach (var item2 in entity.EclOutOrderDetailList)
                            {
                                sumQty += item2.qty;
                            }

                            if (sumQty > 1)
                            {
                                return "失败，该订单应设置为多品流程！";
                            }
                        }
                        else if (flowHeadList.Where(u => u.FlowName.Contains("多品")).Count() > 0)
                        {
                            int sumQty = 0;
                            foreach (var item2 in entity.EclOutOrderDetailList)
                            {
                                sumQty += item2.qty;
                            }

                            if (sumQty == 1)
                            {
                                return "失败，该订单应设置为单品流程！";
                            }
                        }
                    }

                    #endregion

                    #region 查找客户ID
                    List<WhClient> clientL = new List<WhClient>();
                    int ClientId = 0;
                    clientL = idal.IWhClientDAL.SelectBy(u => u.ClientCode == entity.ClientCode && u.WhCode == entity.WhCode);
                    if (clientL.Count() > 0)
                    {
                        ClientId = clientL.First().Id;
                    }
                    else
                    {
                        return "失败!客户不存在!";
                    }

                    #endregion
                    #region 完善Detail数据

                    Hashtable iu = new Hashtable(); //创建一个Hashtable 用于存放item 及uom
                    List<ItemMaster> il;

                    if (idal.IOutBoundOrderDAL.SelectBy(u => u.AltCustomerOutPoNumber == entity.outorder_number_alt && u.ClientId == ClientId && u.WhCode == entity.WhCode).Count() > 0)
                    {
                        return "失败!" + entity.outorder_number + "已经存在,无法导入!";
                    }
                    if (entity.EclOutOrderDetailList.Count() == 0)
                    {
                        return "失败!" + entity.outorder_number + "为空明细订单!";
                    }
                    foreach (var item2 in entity.EclOutOrderDetailList)
                    {
                        //检查数量及款号
                        if (item2.qty == 0 || item2.item_number == "" || item2.item_number == null)
                        {
                            return "失败!" + entity.outorder_number + "明细缺少数据!";
                        }

                        //检查ITEM是否存在集合中
                        if (!iu.Contains(item2.item_number))
                        {
                            il = idal.IItemMasterDAL.SelectBy(u => u.AltItemNumber == item2.item_number && u.WhCode == entity.WhCode && u.ClientId == ClientId && (u.Style1 == null ? "" : u.Style1) == (item2.Style1 == null ? "" : item2.Style1)).OrderBy(u => u.Id).ToList();
                            //判断查询结果输了
                            if (il.Count() > 0)
                            {
                                //判断是否有UOM
                                if (il.First().UnitName == "" || il.First().UnitName == null)
                                {
                                    return "失败!" + entity.outorder_number + ":" + item2.item_number + "UOM为空!可能没有收货";
                                }

                                //更新所有这个ITEM的ID和UOM
                                foreach (var oo in entity.EclOutOrderDetailList)
                                {

                                    if (oo.item_number == item2.item_number)
                                    {
                                        oo.item_id = il.First().Id;
                                        oo.uom = il.First().UnitName;
                                    }

                                }

                                //插入集合
                                iu.Add(item2.item_number, il.First().UnitName);
                                //更新Model中的uom
                                item2.uom = il.First().UnitName;
                            }
                            else
                            {
                                return "失败!" + entity.outorder_number + ":" + item2.item_number + "不存在WMS中!";
                            }
                        }

                    }



                    #endregion
                    #region 添加OutBoundOrder  
                    OutBoundOrder outBoundOrder = new OutBoundOrder();
                    outBoundOrder.WhCode = entity.WhCode;
                    outBoundOrder.OutPoNumber = "OMS" + DI.IDGenerator.NewId;
                    outBoundOrder.CustomerOutPoNumber = entity.outorder_number;
                    outBoundOrder.AltCustomerOutPoNumber = entity.outorder_number_alt;
                    outBoundOrder.ClientId = ClientId;
                    outBoundOrder.ClientCode = entity.ClientCode;
                    outBoundOrder.ProcessId = entity.ProcessId;
                    outBoundOrder.NowProcessId = 3;
                    outBoundOrder.StatusId = 10;
                    outBoundOrder.StatusName = "已确认订单";
                    outBoundOrder.OrderSource = entity.OrderSource;
                    outBoundOrder.OrderType = entity.OrderType;
                    outBoundOrder.CreateUser = entity.CreateUser;
                    outBoundOrder.CreateDate = DateTime.Now;
                    outBoundOrder = idal.IOutBoundOrderDAL.Add(outBoundOrder);
                    idal.IOutBoundOrderDAL.SaveChanges();

                    #endregion
                    #region 添加OutBoundOrderDetail

                    foreach (var item2 in entity.EclOutOrderDetailList)
                    {
                        OutBoundOrderDetail outBoundOrderDetail = new OutBoundOrderDetail();

                        outBoundOrderDetail.WhCode = entity.WhCode;
                        outBoundOrderDetail.OutBoundOrderId = outBoundOrder.Id;
                        outBoundOrderDetail.ItemId = (int)item2.item_id;
                        outBoundOrderDetail.AltItemNumber = item2.item_number;

                        outBoundOrderDetail.UnitName = item2.uom;
                        outBoundOrderDetail.UnitId = 0;
                        outBoundOrderDetail.Qty = item2.qty;
                        outBoundOrderDetail.Price = item2.price;
                        outBoundOrderDetail.LotNumber1 = item2.lot_number;
                        outBoundOrderDetail.CreateUser = entity.CreateUser;
                        outBoundOrderDetail.CreateDate = DateTime.Now;

                        idal.IOutBoundOrderDetailDAL.Add(outBoundOrderDetail);
                    }


                    #endregion

                    idal.SaveChanges();
                    trans.Complete();
                    return "Y";
                }
                catch
                {
                    trans.Dispose();//出现异常，事务手动释放
                    return "系统出现异常，请重新提交！";
                }
            }
        }

        public string BoschOutBoundOrderAddOMS(EclOutOrderModel entity)
        {

            using (TransactionScope trans = new TransactionScope())
            {
                try
                {

                    #region 查找客户ID
                    List<WhClient> clientL = new List<WhClient>();
                    int ClientId = 0;
                    clientL = idal.IWhClientDAL.SelectBy(u => u.ClientCode == entity.ClientCode && u.WhCode == entity.WhCode);
                    if (clientL.Count() > 0)
                    {
                        ClientId = clientL.First().Id;
                    }
                    else
                    {
                        return "失败!客户不存在!";
                    }

                    #endregion
                    #region 完善Detail数据

                    Hashtable iu = new Hashtable(); //创建一个Hashtable 用于存放item 及uom
                    List<ItemMaster> il;

                    if (idal.IOutBoundOrderDAL.SelectBy(u => u.AltCustomerOutPoNumber == entity.outorder_number_alt && u.ClientId == ClientId && u.WhCode == entity.WhCode).Count() > 0)
                    {
                        return "失败!" + entity.outorder_number + "已经存在,无法导入!";
                    }
                    if (entity.EclOutOrderDetailList.Count() == 0)
                    {
                        return "失败!" + entity.outorder_number + "为空明细订单!";
                    }
                    foreach (var item2 in entity.EclOutOrderDetailList)
                    {
                        //检查数量及款号
                        if (item2.qty == 0 || item2.item_number == "" || item2.item_number == null)
                        {
                            return "失败!" + entity.outorder_number + "明细缺少数据!";
                        }

                        //检查ITEM是否存在集合中
                        if (!iu.Contains(item2.item_number))
                        {
                            il = idal.IItemMasterDAL.SelectBy(u => u.AltItemNumber == item2.item_number && u.WhCode == entity.WhCode && u.ClientId == ClientId).OrderBy(u => u.Id).ToList();
                            //判断查询结果输了
                            if (il.Count() > 0)
                            {
                                //判断是否有UOM
                                if (il.First().UnitName == "" || il.First().UnitName == null)
                                {
                                    return "失败!" + entity.outorder_number + ":" + item2.item_number + "UOM为空!可能没有收货";
                                }

                                //更新所有这个ITEM的ID和UOM
                                foreach (var oo in entity.EclOutOrderDetailList)
                                {

                                    if (oo.item_number == item2.item_number)
                                    {
                                        oo.item_id = il.First().Id;
                                        oo.uom = il.First().UnitName;
                                    }

                                }

                                //插入集合
                                iu.Add(item2.item_number, il.First().UnitName);
                                //更新Model中的uom
                                item2.uom = il.First().UnitName;
                            }
                            else
                            {
                                return "失败!" + entity.outorder_number + ":" + item2.item_number + "不存在WMS中!";
                            }
                        }

                    }



                    #endregion
                    #region 添加OutBoundOrder  
                    OutBoundOrder outBoundOrder = new OutBoundOrder();
                    outBoundOrder.WhCode = entity.WhCode;
                    outBoundOrder.OutPoNumber = "OMS" + DI.IDGenerator.NewId;
                    outBoundOrder.CustomerOutPoNumber = entity.outorder_number;
                    outBoundOrder.AltCustomerOutPoNumber = entity.outorder_number_alt;
                    outBoundOrder.ClientId = ClientId;
                    outBoundOrder.ClientCode = entity.ClientCode;
                    outBoundOrder.ProcessId = entity.ProcessId;
                    outBoundOrder.NowProcessId = 3;
                    outBoundOrder.StatusId = 10;
                    outBoundOrder.StatusName = "已确认订单";

                    outBoundOrder.buy_name = entity.buy_name;
                    outBoundOrder.buy_company = entity.buy_company;
                    outBoundOrder.address = entity.address;

                    outBoundOrder.OrderSource = entity.OrderSource;
                    outBoundOrder.OrderType = entity.OrderType;
                    outBoundOrder.CreateUser = entity.CreateUser;
                    outBoundOrder.CreateDate = DateTime.Now;
                    outBoundOrder = idal.IOutBoundOrderDAL.Add(outBoundOrder);
                    idal.IOutBoundOrderDAL.SaveChanges();

                    #endregion
                    #region 添加OutBoundOrderDetail

                    foreach (var item2 in entity.EclOutOrderDetailList)
                    {
                        OutBoundOrderDetail outBoundOrderDetail = new OutBoundOrderDetail();

                        outBoundOrderDetail.WhCode = entity.WhCode;
                        outBoundOrderDetail.OutBoundOrderId = outBoundOrder.Id;
                        outBoundOrderDetail.ItemId = (int)item2.item_id;
                        outBoundOrderDetail.AltItemNumber = item2.item_number;
                        outBoundOrderDetail.CustomerPoNumber = item2.outorder_number;
                        outBoundOrderDetail.SoNumber = item2.so_number;

                        outBoundOrderDetail.UnitName = item2.uom;
                        outBoundOrderDetail.UnitId = 0;
                        outBoundOrderDetail.Qty = item2.qty;
                        outBoundOrderDetail.LotNumber1 = item2.lot_number;
                        outBoundOrderDetail.CreateUser = entity.CreateUser;
                        outBoundOrderDetail.CreateDate = DateTime.Now;

                        idal.IOutBoundOrderDetailDAL.Add(outBoundOrderDetail);
                    }

                    #endregion

                    idal.SaveChanges();
                    trans.Complete();
                    return "Y";
                }
                catch
                {
                    trans.Dispose();//出现异常，事务手动释放
                    return "系统出现异常，请重新提交！";
                }
            }
        }

        //批量导入OMS订单
        public List<EclOutOrderModelResult> OutBoundOrderAddOMSBatch(List<EclOutOrderModel> entityList)
        {
            using (TransactionScope trans = new TransactionScope())
            {
                try
                {
                    EclOutOrderModel first = entityList.First();

                    //流程验证
                    int[] processIdArr = (from a in entityList
                                          select a.ProcessId).ToList().Distinct().ToArray();

                    List<FlowHead> flowHeadList = idal.IFlowHeadDAL.SelectBy(u => processIdArr.Contains(u.Id));

                    //客户验证
                    string[] clientCodeArr = (from a in entityList
                                              select a.ClientCode).ToList().Distinct().ToArray();
                    List<WhClient> clientList = idal.IWhClientDAL.SelectBy(u => clientCodeArr.Contains(u.ClientCode) && u.WhCode == first.WhCode);


                    //订单验证
                    string[] outOrderNumberArr = (from a in entityList
                                                  select a.outorder_number_alt).ToList().Distinct().ToArray();
                    List<OutBoundOrder> outOrderNumberList = idal.IOutBoundOrderDAL.SelectBy(u => outOrderNumberArr.Contains(u.AltCustomerOutPoNumber) && u.WhCode == first.WhCode);

                    List<string> stringList = new List<string>();
                    foreach (var entity in entityList)
                    {
                        foreach (var item2 in entity.EclOutOrderDetailList)
                        {
                            stringList.Add(item2.item_number);
                        }
                    }
                    //款号验证
                    List<ItemMaster> itemMasterList = idal.IItemMasterDAL.SelectBy(u => u.WhCode == first.WhCode && stringList.Contains(u.AltItemNumber)).OrderBy(u => u.Id).ToList();

                    string checkresult = "";
                    List<EclOutOrderModelResult> entityResult = new List<EclOutOrderModelResult>();

                    List<OutBoundOrder> outBoundOrderAddList = new List<OutBoundOrder>();
                    List<OutBoundOrderDetail> outBoundOrderDetailAddList = new List<OutBoundOrderDetail>();

                    List<OutBoundOrderDetail> outBoundOrderDetailAddCheckList = new List<OutBoundOrderDetail>();
                    foreach (var entity in entityList)
                    {
                        checkresult = "";
                        if (entityResult.Where(u => u.outorder_number == entity.outorder_number).Count() > 0)
                        {
                            continue;
                        }

                        if (clientList.Where(u => u.ClientCode == entity.ClientCode).Count() == 0)
                        {
                            EclOutOrderModelResult result = new EclOutOrderModelResult();
                            result.outorder_number = entity.outorder_number;
                            result.outorder_number_alt = entity.outorder_number_alt;
                            result.status = "1";
                            result.remark = "失败，客户不存在WMS中！";
                            entityResult.Add(result);
                            continue;
                        }

                        if (outOrderNumberList.Where(u => u.AltCustomerOutPoNumber == entity.outorder_number_alt && u.ClientCode == entity.ClientCode).Count() > 0)
                        {
                            EclOutOrderModelResult result = new EclOutOrderModelResult();
                            result.outorder_number = entity.outorder_number;
                            result.outorder_number_alt = entity.outorder_number_alt;
                            result.status = "1";
                            result.remark = "失败，订单已存在WMS中！";
                            entityResult.Add(result);
                            continue;
                        }

                        if (entity.EclOutOrderDetailList.Count() == 0)
                        {
                            EclOutOrderModelResult result = new EclOutOrderModelResult();
                            result.outorder_number = entity.outorder_number;
                            result.outorder_number_alt = entity.outorder_number_alt;
                            result.status = "1";
                            result.remark = "失败，订单明细为空！";
                            entityResult.Add(result);
                            continue;
                        }
                        else
                        {
                            if (entity.EclOutOrderDetailList.Where(u => u.qty == 0 || (u.item_number ?? "") == "").Count() > 0)
                            {
                                EclOutOrderModelResult result = new EclOutOrderModelResult();
                                result.outorder_number = entity.outorder_number;
                                result.outorder_number_alt = entity.outorder_number_alt;
                                result.status = "1";
                                result.remark = "失败，订单明细缺少数量或款号数据！";
                                entityResult.Add(result);
                                continue;
                            }
                        }

                        if (flowHeadList.Where(u => u.Id == entity.ProcessId).Count() == 0)
                        {
                            EclOutOrderModelResult result = new EclOutOrderModelResult();
                            result.outorder_number = entity.outorder_number;
                            result.outorder_number_alt = entity.outorder_number_alt;
                            result.status = "1";
                            result.remark = "失败，订单流程不存在WMS中！";
                            entityResult.Add(result);
                            continue;
                        }
                        else
                        {
                            if (flowHeadList.Where(u => u.FlowName.Contains("单品") && u.Id == entity.ProcessId).Count() > 0)
                            {
                                int sumQty = 0;
                                foreach (var item2 in entity.EclOutOrderDetailList)
                                {
                                    sumQty += item2.qty;
                                }

                                if (sumQty > 1)
                                {
                                    EclOutOrderModelResult result = new EclOutOrderModelResult();
                                    result.outorder_number = entity.outorder_number;
                                    result.outorder_number_alt = entity.outorder_number_alt;
                                    result.status = "1";
                                    result.remark = "失败，订单应设置为多品流程！";
                                    entityResult.Add(result);
                                    continue;
                                }
                            }
                            else if (flowHeadList.Where(u => u.FlowName.Contains("多品") && u.Id == entity.ProcessId).Count() > 0)
                            {
                                int sumQty = 0;
                                foreach (var item2 in entity.EclOutOrderDetailList)
                                {
                                    sumQty += item2.qty;
                                }

                                if (sumQty == 1)
                                {
                                    EclOutOrderModelResult result = new EclOutOrderModelResult();
                                    result.outorder_number = entity.outorder_number;
                                    result.outorder_number_alt = entity.outorder_number_alt;
                                    result.status = "1";
                                    result.remark = "失败，订单应设置为单品流程！";
                                    entityResult.Add(result);
                                    continue;
                                }
                            }
                        }

                        string outPoNumber = "OMS" + DI.IDGenerator.NewId;

                        outBoundOrderDetailAddCheckList.Clear();

                        foreach (var item2 in entity.EclOutOrderDetailList)
                        {
                            List<ItemMaster> itemList = itemMasterList.Where(u => u.AltItemNumber == item2.item_number && u.ClientCode == entity.ClientCode && (u.Style1 == null ? "" : u.Style1) == (item2.Style1 == null ? "" : item2.Style1)).ToList();

                            if (itemList.Count == 0)
                            {
                                EclOutOrderModelResult result = new EclOutOrderModelResult();
                                result.outorder_number = entity.outorder_number;
                                result.outorder_number_alt = entity.outorder_number_alt;
                                result.status = "1";
                                result.remark = "失败，款号不存在WMS中！";
                                checkresult = "失败，款号不存在WMS中！";
                                entityResult.Add(result);
                                break;
                            }
                            else
                            {
                                if (itemList.Where(u => (u.UnitName ?? "") == "").Count() > 0)
                                {
                                    EclOutOrderModelResult result = new EclOutOrderModelResult();
                                    result.outorder_number = entity.outorder_number;
                                    result.outorder_number_alt = entity.outorder_number_alt;
                                    result.status = "1";
                                    result.remark = "失败，款号单位异常！";
                                    checkresult = "失败，款号单位异常！";
                                    entityResult.Add(result);
                                    break;
                                }
                                else
                                {
                                    ItemMaster itemFirst = itemList.First();

                                    OutBoundOrderDetail outBoundOrderDetail = new OutBoundOrderDetail();
                                    outBoundOrderDetail.WhCode = entity.WhCode;
                                    outBoundOrderDetail.OutBoundOrderId = 0;
                                    outBoundOrderDetail.OutPoNumber = outPoNumber;
                                    outBoundOrderDetail.ItemId = itemFirst.Id;
                                    outBoundOrderDetail.AltItemNumber = item2.item_number;
                                    outBoundOrderDetail.UnitName = (itemFirst.UnitName ?? "") == "" ? item2.uom : itemFirst.UnitName;
                                    outBoundOrderDetail.UnitId = 0;
                                    outBoundOrderDetail.Qty = item2.qty;
                                    outBoundOrderDetail.Price = item2.price;
                                    outBoundOrderDetail.LotNumber1 = item2.lot_number;
                                    outBoundOrderDetail.CreateUser = entity.CreateUser;
                                    outBoundOrderDetail.CreateDate = DateTime.Now;

                                    outBoundOrderDetailAddCheckList.Add(outBoundOrderDetail);
                                }
                            }
                        }
                        if (checkresult != "")
                        {
                            continue;
                        }

                        //如果明细数量一致，证明明细验证通过
                        if (outBoundOrderDetailAddCheckList.Count == entity.EclOutOrderDetailList.Count)
                        {
                            foreach (var item in outBoundOrderDetailAddCheckList)
                            {
                                outBoundOrderDetailAddList.Add(item);
                            }
                        }
                        else
                        {
                            EclOutOrderModelResult result = new EclOutOrderModelResult();
                            result.outorder_number = entity.outorder_number;
                            result.outorder_number_alt = entity.outorder_number_alt;
                            result.status = "1";
                            result.remark = "失败，订单明细匹配出不一致！";
                            entityResult.Add(result);
                            continue;
                        }

                        //能走到最下面 证明订单没有问题，可新增
                        WhClient client = clientList.Where(u => u.ClientCode == entity.ClientCode).First();

                        OutBoundOrder outBoundOrder = new OutBoundOrder();
                        outBoundOrder.WhCode = entity.WhCode;
                        outBoundOrder.OutPoNumber = outPoNumber;
                        outBoundOrder.CustomerOutPoNumber = entity.outorder_number;
                        outBoundOrder.AltCustomerOutPoNumber = entity.outorder_number_alt;
                        outBoundOrder.ClientId = client.Id;
                        outBoundOrder.ClientCode = entity.ClientCode;
                        outBoundOrder.ProcessId = entity.ProcessId;
                        outBoundOrder.NowProcessId = 3;
                        outBoundOrder.StatusId = 10;
                        outBoundOrder.StatusName = "已确认订单";
                        outBoundOrder.OrderSource = entity.OrderSource;
                        outBoundOrder.OrderType = entity.OrderType;
                        outBoundOrder.CreateUser = entity.CreateUser;
                        outBoundOrder.CreateDate = DateTime.Now;

                        outBoundOrderAddList.Add(outBoundOrder);

                        EclOutOrderModelResult result1 = new EclOutOrderModelResult();
                        result1.outorder_number = entity.outorder_number;
                        result1.outorder_number_alt = entity.outorder_number_alt;
                        result1.status = "0";
                        result1.remark = "订单导入成功！";
                        entityResult.Add(result1);
                    }

                    if (outBoundOrderAddList.Count > 0)
                    {
                        idal.IOutBoundOrderDAL.Add(outBoundOrderAddList);
                        idal.IOutBoundOrderDetailDAL.Add(outBoundOrderDetailAddList);

                        idal.SaveChanges();

                        //以下更新已在EIP-OMS功能中批量更新实现
                        //string[] OutPoNumberList = (from a in outBoundOrderDetailAddList
                        //                            select a.OutPoNumber).ToList().Distinct().ToArray();

                        ////根据OutPoNumber 修改订单明细Id
                        //List<OutBoundOrder> getOutBoundOrderList = idal.IOutBoundOrderDAL.SelectBy(u => u.WhCode == first.WhCode && OutPoNumberList.Contains(u.OutPoNumber)).ToList();

                        //foreach (var item in outBoundOrderAddList)
                        //{
                        //    OutBoundOrder getOutBoundOrder = getOutBoundOrderList.Where(u => u.WhCode == item.WhCode && u.OutPoNumber == item.OutPoNumber).First();

                        //    idal.IOutBoundOrderDetailDAL.UpdateByExtended(u => u.WhCode == item.WhCode && u.OutPoNumber == item.OutPoNumber, t => new OutBoundOrderDetail { OutBoundOrderId = getOutBoundOrder.Id });
                        //}
                    }

                    trans.Complete();
                    return entityResult;
                }
                catch
                {
                    trans.Dispose();//出现异常，事务手动释放
                    return null;
                }
            }
        }

        public void UrlEdiTaskInsertOut(string TransferCode, string WhCode, string CreateUser)
        {
            //TransferTask rr = idal.ITransferTaskDAL.SelectBy(u => u.TransferId == TransferCode && u.WhCode == WhCode).First();

            List<string> getLoadId = (from a in idal.ITransferTaskDAL.SelectAll()
                                      join b in idal.ITransferHeadDAL.SelectAll()
                                      on a.Id equals b.TransferTaskId
                                      where a.TransferId == TransferCode && a.WhCode == WhCode
                                      select b.LoadId).Take(5).ToList();

            List<FlowHead> getflowHeadList = (from a in idal.ILoadMasterDAL.SelectAll()
                                              join b in idal.IFlowHeadDAL.SelectAll()
                                              on a.ProcessId equals b.Id
                                              where a.WhCode == WhCode && getLoadId.Contains(a.LoadId)
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
                    uet.Mark = TransferCode;
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
                    uet1.Mark = TransferCode;
                    uet1.HttpType = url.HttpType;
                    uet1.Status = 1;
                    uet1.CreateDate = DateTime.Now;
                    idal.IUrlEdiTaskDAL.Add(uet1);
                }
            }

            //if (rr.WhCode == "02" || rr.WhCode == "40")
            //{

            //    UrlEdiTask uet = new UrlEdiTask();
            //    uet.Status = 1;
            //    uet.Type = "OMS";
            //    uet.Url = "http://10.88.88.90/net/oms/out_order.aspx?actionType=WmsOutUpdate&WhCode=" + WhCode;
            //    uet.Field = "TransferCode";
            //    uet.Mark = TransferCode;
            //    uet.HttpType = "Get";

            //    idal.IUrlEdiTaskDAL.Add(uet);

            //}
        }

        #endregion

        //手动获取快递单
        public string GetExpressNumber(string express_code, SFExpressModel sfModel, YTExpressModel ytModel, string whCode, ZTOExpressModel ztModel)
        {
            ExpressResult result = new ExpressResult();

            ExpressManager express = new ExpressManager();
            PackTaskManager packManager = new PackTaskManager();

            #region 获取快递
            //顺丰快递
            if (express_code == "SF")
            {
                if ((sfModel.j_address ?? "") == "" || (sfModel.j_contact ?? "") == "")
                {
                    return "ERR$当前未维护寄件人或寄件地址！";
                }

                if ((sfModel.d_Province ?? "") == "" || (sfModel.d_city ?? "") == "")
                {
                    return "ERR$当前包装未维护收件人省份！";
                }

                sfModel.d_contact = sfModel.d_contact.Replace("'", " ");
                sfModel.d_contact = sfModel.d_contact.Replace("&", " ");
                sfModel.d_contact = sfModel.d_contact.Replace("$", " ");
                sfModel.d_contact = sfModel.d_contact.Replace("*", " ");
                sfModel.d_contact = sfModel.d_contact.Replace("<", " ");
                sfModel.d_contact = sfModel.d_contact.Replace(">", " ");
                sfModel.d_contact = sfModel.d_contact.Replace(@"\r\n", " ").Replace(@"\r", " ").Replace(@"\n", " ");

                sfModel.d_address = sfModel.d_address.Replace('&', ' ');
                sfModel.d_address = sfModel.d_address.Replace('$', ' ');
                sfModel.d_address = sfModel.d_address.Replace('"', ' ');
                sfModel.d_address = sfModel.d_address.Replace(',', ' ');
                sfModel.d_address = sfModel.d_address.Replace('，', ' ');
                sfModel.d_address = sfModel.d_address.Replace('<', ' ');
                sfModel.d_address = sfModel.d_address.Replace('>', ' ');
                sfModel.d_address = sfModel.d_address.Replace('（', ' ');
                sfModel.d_address = sfModel.d_address.Replace('）', ' ');
                sfModel.d_address = sfModel.d_address.Replace("'", " ");
                sfModel.d_address = sfModel.d_address.Replace("*", " ");
                sfModel.d_address = sfModel.d_address.Replace("?", " ");
                sfModel.d_address = sfModel.d_address.Replace("？", " ");

                sfModel.d_address = sfModel.d_address.Replace(@"\r\n", " ").Replace(@"\r", " ").Replace(@"\n", " ");

                result = express.GetExpress(sfModel);
                packManager.AddPackHeadJson(whCode, result);
            }
            else if (express_code == "YTO")
            {
                if ((ytModel.j_address ?? "") == "" || (ytModel.j_contact ?? "") == "")
                {
                    return "ERR$当前未维护寄件人或寄件地址！";
                }

                if ((ytModel.d_Province ?? "") == "" || (ytModel.d_city ?? "") == "")
                {
                    return "ERR$当前包装未维护收件人省份！";
                }

                ytModel.d_contact = ytModel.d_contact.Replace("'", " ");
                ytModel.d_contact = ytModel.d_contact.Replace("&", " ");
                ytModel.d_contact = ytModel.d_contact.Replace("$", " ");
                ytModel.d_contact = ytModel.d_contact.Replace("*", " ");
                ytModel.d_contact = ytModel.d_contact.Replace("<", " ");
                ytModel.d_contact = ytModel.d_contact.Replace(">", " ");
                ytModel.d_contact = ytModel.d_contact.Replace(@"\r\n", " ").Replace(@"\r", " ").Replace(@"\n", " ");

                ytModel.d_address = ytModel.d_address.Replace('&', ' ');
                ytModel.d_address = ytModel.d_address.Replace('$', ' ');
                ytModel.d_address = ytModel.d_address.Replace('"', ' ');
                ytModel.d_address = ytModel.d_address.Replace(',', ' ');
                ytModel.d_address = ytModel.d_address.Replace('，', ' ');
                ytModel.d_address = ytModel.d_address.Replace('<', ' ');
                ytModel.d_address = ytModel.d_address.Replace('>', ' ');
                ytModel.d_address = ytModel.d_address.Replace('（', ' ');
                ytModel.d_address = ytModel.d_address.Replace('）', ' ');
                ytModel.d_address = ytModel.d_address.Replace("'", " ");
                ytModel.d_address = ytModel.d_address.Replace("*", " ");
                ytModel.d_address = ytModel.d_address.Replace("?", " ");
                ytModel.d_address = ytModel.d_address.Replace("？", " ");

                ytModel.d_address = ytModel.d_address.Replace(@"\r\n", " ").Replace(@"\r", " ").Replace(@"\n", " ");

                result = express.GetExpress(ytModel);
            }
            else if (express_code == "ZTO")
            {
                ztModel.receiveInfo.receiverName = ztModel.receiveInfo.receiverName.Replace('&', ' ').Replace("&", " ").Replace("$", " ").Replace(@"\r\n", " ").Replace(@"\r", " ").Replace(@"\n", " ");
                ztModel.receiveInfo.receiverAddress = ztModel.receiveInfo.receiverAddress.Replace('&', ' ').Replace("&", " ").Replace("$", " ").Replace("?", " ").Replace("？", " ").Replace(@"\r\n", " ").Replace(@"\r", " ").Replace(@"\n", " ");

                result = express.GetExpress(ztModel);
            }
            else
            {
                return "ERR$当前快递公司未对接快递单信息！";
            }
            #endregion



            //正确返回OK$快递单号

            return result.Status + "$" + result.Message + "$" + result.MailNo + "$" + result.DestCode;

        }
    }
}

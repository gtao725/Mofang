using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMS.BLLClass;
using WMS.IBLL;
using MODEL_MSSQL;
using System.Collections;

namespace WMS.BLL
{
    public class BsOutBoundManager : IBsOutBoundManager
    {
        IDAL.IDALSession idal = BLL.BLLHelper.GetDal();
        public string OutBoundLoadAddBs(BsLoadModel entity)
        {
            OutBoundOrderManager obom = new OutBoundOrderManager();
            List<LoadDetail> loadDetailList = new List<LoadDetail>();
            Hashtable iu = new Hashtable(); //创建一个Hashtable 用于存放item 及uom

            LoadManager loadManager = new LoadManager();
            ReleaseLoadManager releaseLoadManager = new ReleaseLoadManager();

            string ShipMode = "集装箱";
            List<ItemMaster> il;
            List<WhClient> wcl;
            int ClientId;
            String TempItemNumber;

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

            if (entity.ClientCode == "Joules")
            {
                ShipMode = "快递";
            }

            #endregion

            #region 检查数据完整并更新itemID及UOM
            if (entity.BsLoadId == "" || entity.BsLoadId == null)
            {
                return "失败!ECLLoadID!";
            }
            if (entity.BsOutOrderList.Count() == 0)
            {
                return "失败!无订单!";
            }
            foreach (var item in entity.BsOutOrderList)
            {

                if (idal.IOutBoundOrderDAL.SelectBy(u => u.CustomerOutPoNumber == item.outorder_number && u.ClientId == ClientId).Count() > 0)
                {
                    return "失败!" + item.outorder_number + "已经存在,无法导入!";
                }
                if (item.BsOutOrderDetailList.Count() == 0)
                {
                    return "失败!" + item.outorder_number + "存在空明细订单!";
                }
                foreach (var item2 in item.BsOutOrderDetailList)
                {

                    if (item2.qty == 0 || item2.item_number == "" || item2.item_number == null || item2.qty == null)
                    {
                        return "失败!" + item2.outorder_number + "明细缺少数据!";
                    }

                    TempItemNumber = item2.item_number + (item2.Style1 == null ? "" : item2.Style1) + (item2.Style2 == null ? "" : item2.Style2) + (item2.Style3 == null ? "" : item2.Style3);

                    //检查ITEM是否存在集合中
                    if (!iu.Contains(TempItemNumber))
                    {
                        il = idal.IItemMasterDAL.SelectBy(u => u.AltItemNumber == item2.item_number && u.WhCode == entity.WhCode && u.ClientId == ClientId && (u.Style1 == null ? "" : u.Style1) == (item2.Style1 == null ? "" : item2.Style1) && (u.Style2 == null ? "" : u.Style2) == (item2.Style2 == null ? "" : item2.Style2) && (u.Style3 == null ? "" : u.Style3) == (item2.Style3 == null ? "" : item2.Style3)).OrderBy(u => u.Id).ToList();
                        //判断查询结果输了
                        if (il.Count() > 0)
                        {
                            //判断是否有UOM
                            if (il.First().UnitName == "" || il.First().UnitName == null || il.First().UnitName == "none")
                            {
                                return "失败!" + item2.outorder_number + ":" + item2.item_number + "UOM为空!可能没有收货";
                            }

                            //更新所有这个ITEM的ID和UOM
                            foreach (var oo in entity.BsOutOrderList)
                            {
                                foreach (var ooo in oo.BsOutOrderDetailList)
                                {
                                    if (ooo.item_number == item2.item_number && ooo.Style1 == item2.Style1 && ooo.Style2 == item2.Style2 && ooo.Style3 == item2.Style3)
                                    {
                                        ooo.item_id = il.First().Id;
                                        ooo.uom = il.First().UnitName;
                                    }
                                }
                            }

                            //插入集合
                            iu.Add(TempItemNumber, il.First().UnitName);
                            //更新Model中的uom
                            //item2.uom = il.First().UnitName;
                        }
                        else
                        {
                            return "失败!" + item2.outorder_number + ":" + item2.item_number + "不存在WMS中!";
                        }
                    }

                }
            }

            #endregion

            if (entity.ProcessId == 0)
            {
                return "失败!缺少出货流程!";
            }


            #region 创建Load表头

            LoadMaster load = new LoadMaster();
            load.ProcessId = entity.ProcessId;
            load.ShipMode = ShipMode;
            load.ProcessName = entity.ProcessName;
            load.WhCode = entity.WhCode;
            load.CreateUser = entity.CreateUser;


            load = loadManager.LoadMasterAdd(load);
            idal.SaveChanges();

            #endregion

            #region 导入出库订单,并组织load明细
            foreach (var item in entity.BsOutOrderList)
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
                outBoundOrder.OrderSource = "Bs";
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

                foreach (var item2 in item.BsOutOrderDetailList)
                {
                    OutBoundOrderDetail outBoundOrderDetail = new OutBoundOrderDetail();

                    outBoundOrderDetail.WhCode = entity.WhCode;
                    outBoundOrderDetail.OutBoundOrderId = outBoundOrder.Id;
                    outBoundOrderDetail.ItemId = (int)item2.item_id;
                    outBoundOrderDetail.AltItemNumber = item2.item_number;
                    outBoundOrderDetail.SoNumber = item2.SoNumber;
                    outBoundOrderDetail.CustomerPoNumber = item2.CustomerPoNumber;
                    outBoundOrderDetail.UnitName = item2.uom;
                    outBoundOrderDetail.UnitId = 0;
                    outBoundOrderDetail.Qty = item2.qty;
                    outBoundOrderDetail.LotNumber1 = item2.lot_number;
                    outBoundOrderDetail.CreateUser = entity.CreateUser;
                    outBoundOrderDetail.CreateDate = DateTime.Now;
                    outBoundOrderDetail.Sequence = item2.LoadSeq;

                    idal.IOutBoundOrderDetailDAL.Add(outBoundOrderDetail);

                }


                #endregion
            }


            #endregion

            #region 导入LOAD明细

            loadManager.LoadDetailAdd(loadDetailList);

            #endregion

            //#region 释放load
            ////ReleaseResult = releaseLoadManager.CheckReleaseLoad(load.LoadId, load.WhCode, entity.CreateUser);
            //#endregion

            //if (ReleaseResult == "Y")
            //{
            //    Result = ReleaseResult + "$" + load.LoadId;
            //}
            //else {

            //    Result = "N$" + ReleaseResult + "$" + load.LoadId;
            //}

            idal.SaveChanges();

            return "Y$" + load.LoadId;
        }

        #region 检查出库订单是否存在
        public string OutBoundOrderCheck(string CustomerOutPoNumber)
        {
            List<OutBoundOrder> orderlist = idal.IOutBoundOrderDAL.SelectBy(u => u.WhCode == "03" && u.CustomerOutPoNumber == CustomerOutPoNumber);
            if (orderlist.Count() > 0)
            {
                return "订单在WMS系统存在不能删除！";
            }
            else
            {
                return "Y";
            }
        }

        public string EditLoadShipMode(string LoadId, string WhCode, string ShipMode)
        {
            if (ShipMode == "散货")
            {
                MODEL_MSSQL.LoadMaster ld = new LoadMaster();
                ld.ShipMode = "货箱";
                idal.ILoadMasterDAL.UpdateBy(ld, u => u.LoadId == LoadId && u.WhCode == WhCode, new string[] { "ShipMode" });
                idal.SaveChanges();
            }

            return "Y";
        }

        #endregion
    }
}

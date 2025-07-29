using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MODEL_MSSQL;
using WMS.BLLClass;
using WMS.IBLL;

namespace WMS.BLL
{
    public class EclInBoundManager : IEclInBoundManager
    {
        IDAL.IDALSession idal = BLL.BLLHelper.GetDal();

        InBoundOrderManager ibo = new InBoundOrderManager();
        RegInBoundOrderManager ribo = new RegInBoundOrderManager();
        RootManager rm = new RootManager();

        #region 4.电商订单导入
        public string InBoundOrderListAddEcl(InBoundOrderInsert entity)
        {
            string result = "";     //执行总结果
            string CustomerPoNumber = "";
            RegInBoundOrderManager rm = new RegInBoundOrderManager();
            int ClientId = idal.IWhClientDAL.SelectBy(u => u.ClientCode == entity.ClientCode && u.WhCode == entity.WhCode).First().Id;

            entity.ClientId = ClientId;

            #region 判断订单号码是否存在
            if (entity.InBoundOrderDetailInsert.Count() == 0)
            {
                return "失败!无明细!";
            }
            else
            {
                CustomerPoNumber = entity.InBoundOrderDetailInsert.First().CustomerPoNumber;
            }

            if (idal.IInBoundOrderDAL.SelectBy(u => u.CustomerPoNumber == CustomerPoNumber && u.ClientCode == entity.ClientCode && u.WhCode == entity.WhCode).Count() > 0)
            {
                return "重复导入!";
            };

            #endregion


            List<string> poList = new List<string>();
            List<string> skuList = new List<string>();

            List<InBoundOrder> InBoundOrderAddList = new List<InBoundOrder>();
            List<ItemMaster> ItemMasterAddList = new List<ItemMaster>();

            List<InBoundOrder> checkInBoundOrderAddResult = new List<InBoundOrder>();
            List<ItemMaster> checkItemMasterAddResult = new List<ItemMaster>();

            InBoundSO inBoundSO = new InBoundSO();

            if (!string.IsNullOrEmpty(entity.SoNumber))
            {
                //添加InBoundSO 
                List<InBoundSO> listInBoundSO = idal.IInBoundSODAL.SelectBy(u => u.SoNumber == entity.SoNumber && u.WhCode == entity.WhCode && u.ClientCode == entity.ClientCode);

                //如果SO不存在 就添加
                if (listInBoundSO.Count == 0)
                {
                    inBoundSO.WhCode = entity.WhCode;
                    inBoundSO.SoNumber = entity.SoNumber;
                    inBoundSO.ClientCode = entity.ClientCode;
                    inBoundSO.ClientId = (int)entity.ClientId;              //添加新数据 必须赋予客户ID
                    inBoundSO.CreateUser = entity.CreateUser;
                    inBoundSO.CreateDate = DateTime.Now;
                    idal.IInBoundSODAL.Add(inBoundSO);
                    idal.IInBoundSODAL.SaveChanges();
                }
                else
                {
                    //存在，就获取
                    inBoundSO = listInBoundSO.First();
                }
            }

            //批量导入预录入PO

            if (!string.IsNullOrEmpty(entity.SoNumber))
            {
                string[] getPoArr = (from a in entity.InBoundOrderDetailInsert select a.CustomerPoNumber).ToList().Distinct().ToArray();

                List<InBoundOrder> checkPoArr = idal.IInBoundOrderDAL.SelectBy(u => getPoArr.Contains(u.CustomerPoNumber) && u.WhCode == entity.WhCode && u.ClientCode == entity.ClientCode && u.SoId == inBoundSO.Id);

                string[] getSkuArr = (from a in entity.InBoundOrderDetailInsert select a.AltItemNumber).ToList().Distinct().ToArray();

                List<ItemMaster> checkSkuArr = idal.IItemMasterDAL.SelectBy(u => u.WhCode == entity.WhCode && getSkuArr.Contains(u.AltItemNumber) && u.ClientId == entity.ClientId).OrderBy(u => u.Id).ToList();

                foreach (var item in entity.InBoundOrderDetailInsert)
                {
                    if (checkInBoundOrderAddResult.Where(u => u.SoId == inBoundSO.Id && u.CustomerPoNumber == item.CustomerPoNumber).Count() == 0)
                    {
                        if (checkPoArr.Where(u => u.CustomerPoNumber == item.CustomerPoNumber && u.WhCode == entity.WhCode && u.ClientCode == entity.ClientCode && u.SoId == inBoundSO.Id).Count() == 0)
                        {
                            InBoundOrder inBoundOrder = new InBoundOrder();
                            inBoundOrder.WhCode = entity.WhCode;
                            inBoundOrder.SoId = inBoundSO.Id;
                            inBoundOrder.PoNumber = "ST" + DI.IDGenerator.NewId;
                            inBoundOrder.CustomerPoNumber = item.CustomerPoNumber;
                            inBoundOrder.AltCustomerPoNumber = item.AltCustomerPoNumber;
                            inBoundOrder.ClientId = (int)entity.ClientId;
                            inBoundOrder.ClientCode = entity.ClientCode;
                            inBoundOrder.OrderType = entity.OrderType;
                            inBoundOrder.ProcessId = entity.ProcessId;
                            inBoundOrder.ProcessName = entity.ProcessName;
                            inBoundOrder.OrderSource = "ECL";
                            inBoundOrder.CreateUser = item.CreateUser;
                            inBoundOrder.CreateDate = DateTime.Now;
                            InBoundOrderAddList.Add(inBoundOrder);
                        }
                        poList.Add(item.CustomerPoNumber);

                        InBoundOrder inboundResult = new InBoundOrder();
                        inboundResult.SoId = inBoundSO.Id;
                        inboundResult.CustomerPoNumber = item.CustomerPoNumber;
                        checkInBoundOrderAddResult.Add(inboundResult);
                    }

                    if (checkItemMasterAddResult.Where(u => u.AltItemNumber == item.AltItemNumber && (u.Style1 == null ? "" : u.Style1) == (item.Style1 == null ? "" : item.Style1) && (u.Style2 == null ? "" : u.Style2) == (item.Style2 == null ? "" : item.Style2) && (u.Style3 == null ? "" : u.Style3) == (item.Style3 == null ? "" : item.Style3) && u.ClientId == entity.ClientId).Count() == 0)
                    {
                        if (checkSkuArr.Where(u => u.WhCode == entity.WhCode && u.AltItemNumber == item.AltItemNumber && (u.Style1 == null ? "" : u.Style1) == (item.Style1 == null ? "" : item.Style1) && (u.Style2 == null ? "" : u.Style2) == (item.Style2 == null ? "" : item.Style2) && (u.Style3 == null ? "" : u.Style3) == (item.Style3 == null ? "" : item.Style3) && u.ClientId == entity.ClientId).Count() == 0)
                        {
                            ItemMaster itemMaster = new ItemMaster();
                            itemMaster.ItemNumber = "SYS" + DI.IDGenerator.NewId;
                            itemMaster.WhCode = entity.WhCode;
                            itemMaster.AltItemNumber = item.AltItemNumber;
                            itemMaster.ClientId = (int)entity.ClientId;
                            itemMaster.ClientCode = entity.ClientCode;
                            itemMaster.Style1 = item.Style1 ?? "";
                            itemMaster.Style2 = item.Style2 ?? "";
                            itemMaster.Style3 = item.Style3 ?? "";
                            itemMaster.UnitFlag = 0;
                            if (item.UnitName == "" || item.UnitName == null)
                            {
                                itemMaster.UnitName = "none";
                            }
                            else
                            {
                                itemMaster.UnitName = item.UnitName;
                            }

                            itemMaster.CreateUser = item.CreateUser;
                            itemMaster.CreateDate = DateTime.Now;
                            ItemMasterAddList.Add(itemMaster);   //款号不存在就新增
                        }
                        skuList.Add(item.AltItemNumber);

                        ItemMaster itemResult = new ItemMaster();
                        itemResult.ClientId = (int)entity.ClientId;
                        itemResult.AltItemNumber = item.AltItemNumber;
                        itemResult.Style1 = item.Style1 ?? "";
                        itemResult.Style2 = item.Style2 ?? "";
                        itemResult.Style3 = item.Style3 ?? "";
                        checkItemMasterAddResult.Add(itemResult);
                    }

                }
            }
            else
            {
                string[] getPoArr = (from a in entity.InBoundOrderDetailInsert select a.CustomerPoNumber).ToList().Distinct().ToArray();

                List<InBoundOrder> checkPoArr = idal.IInBoundOrderDAL.SelectBy(u => getPoArr.Contains(u.CustomerPoNumber) && u.WhCode == entity.WhCode && u.ClientCode == entity.ClientCode && u.SoId == null);

                string[] getSkuArr = (from a in entity.InBoundOrderDetailInsert select a.AltItemNumber).ToList().Distinct().ToArray();

                List<ItemMaster> checkSkuArr = idal.IItemMasterDAL.SelectBy(u => u.WhCode == entity.WhCode && getSkuArr.Contains(u.AltItemNumber) && u.ClientId == entity.ClientId).OrderBy(u => u.Id).ToList();

                foreach (var item in entity.InBoundOrderDetailInsert)
                {
                    if (checkInBoundOrderAddResult.Where(u => u.SoId == null && u.CustomerPoNumber == item.CustomerPoNumber).Count() == 0)
                    {
                        if (checkPoArr.Where(u => u.CustomerPoNumber == item.CustomerPoNumber && u.WhCode == entity.WhCode && u.ClientCode == entity.ClientCode && u.SoId == null).Count() == 0)
                        {
                            InBoundOrder inBoundOrder = new InBoundOrder();
                            inBoundOrder.WhCode = entity.WhCode;
                            inBoundOrder.PoNumber = "ST" + DI.IDGenerator.NewId;
                            inBoundOrder.CustomerPoNumber = item.CustomerPoNumber;
                            inBoundOrder.AltCustomerPoNumber = item.AltCustomerPoNumber;
                            inBoundOrder.ClientId = (int)entity.ClientId;
                            inBoundOrder.ClientCode = entity.ClientCode;
                            inBoundOrder.OrderType = entity.OrderType;
                            inBoundOrder.ProcessId = entity.ProcessId;
                            inBoundOrder.ProcessName = entity.ProcessName;
                            inBoundOrder.OrderSource = "ECL";
                            inBoundOrder.CreateUser = item.CreateUser;
                            inBoundOrder.CreateDate = DateTime.Now;
                            InBoundOrderAddList.Add(inBoundOrder);
                        }

                        poList.Add(item.CustomerPoNumber);

                        InBoundOrder inboundResult = new InBoundOrder();
                        inboundResult.SoId = null;
                        inboundResult.CustomerPoNumber = item.CustomerPoNumber;
                        checkInBoundOrderAddResult.Add(inboundResult);
                    }

                    if (checkItemMasterAddResult.Where(u => u.AltItemNumber == item.AltItemNumber && (u.Style1 == null ? "" : u.Style1) == (item.Style1 == null ? "" : item.Style1) && (u.Style2 == null ? "" : u.Style2) == (item.Style2 == null ? "" : item.Style2) && (u.Style3 == null ? "" : u.Style3) == (item.Style3 == null ? "" : item.Style3) && u.ClientId == entity.ClientId).Count() == 0)
                    {
                        if (checkSkuArr.Where(u => u.WhCode == entity.WhCode && u.AltItemNumber == item.AltItemNumber && (u.Style1 == null ? "" : u.Style1) == (item.Style1 == null ? "" : item.Style1) && (u.Style2 == null ? "" : u.Style2) == (item.Style2 == null ? "" : item.Style2) && (u.Style3 == null ? "" : u.Style3) == (item.Style3 == null ? "" : item.Style3) && u.ClientId == entity.ClientId).Count() == 0)
                        {
                            ItemMaster itemMaster = new ItemMaster();
                            itemMaster.ItemNumber = "SYS" + DI.IDGenerator.NewId;
                            itemMaster.WhCode = entity.WhCode;
                            itemMaster.AltItemNumber = item.AltItemNumber;
                            itemMaster.ClientId = (int)entity.ClientId;
                            itemMaster.ClientCode = entity.ClientCode;
                            itemMaster.Style1 = item.Style1 ?? "";
                            itemMaster.Style2 = item.Style2 ?? "";
                            itemMaster.Style3 = item.Style3 ?? "";
                            itemMaster.UnitFlag = 0;
                            if (item.UnitName == "" || item.UnitName == null)
                            {
                                itemMaster.UnitName = "none";
                            }
                            else
                            {
                                itemMaster.UnitName = item.UnitName;
                            }

                            itemMaster.CreateUser = item.CreateUser;
                            itemMaster.CreateDate = DateTime.Now;
                            ItemMasterAddList.Add(itemMaster);   //款号不存在就新增
                        }
                        skuList.Add(item.AltItemNumber);

                        ItemMaster itemResult = new ItemMaster();
                        itemResult.ClientId = (int)entity.ClientId;
                        itemResult.AltItemNumber = item.AltItemNumber;
                        itemResult.Style1 = item.Style1 ?? "";
                        itemResult.Style2 = item.Style2 ?? "";
                        itemResult.Style3 = item.Style3 ?? "";
                        checkItemMasterAddResult.Add(itemResult);
                    }
                }
            }


            if (InBoundOrderAddList.Count > 0)
            {
                idal.IInBoundOrderDAL.Add(InBoundOrderAddList);
                idal.IInBoundSODAL.SaveChanges();
            }

            if (ItemMasterAddList.Count > 0)
            {
                idal.IItemMasterDAL.Add(ItemMasterAddList);
                idal.IItemMasterDAL.SaveChanges();
            }

            List<InBoundOrder> listInBoundOrder = idal.IInBoundOrderDAL.SelectBy(u => poList.Contains(u.CustomerPoNumber) && u.WhCode == entity.WhCode);

            List<ItemMaster> listItemMaster = idal.IItemMasterDAL.SelectBy(u => skuList.Contains(u.AltItemNumber) && u.WhCode == entity.WhCode).OrderBy(u => u.Id).ToList();

            foreach (var item in entity.InBoundOrderDetailInsert)
            {
                InBoundOrder inBoundOrder = new InBoundOrder();

                if (!string.IsNullOrEmpty(entity.SoNumber))
                {
                    inBoundOrder = listInBoundOrder.Where(u => u.CustomerPoNumber == item.CustomerPoNumber && u.ClientCode == entity.ClientCode && u.SoId == inBoundSO.Id).First();
                }
                else
                {
                    inBoundOrder = listInBoundOrder.Where(u => u.CustomerPoNumber == item.CustomerPoNumber && u.ClientCode == entity.ClientCode && u.SoId == null).First();
                }

                //判断款号是否存在
                ItemMaster itemMaster = listItemMaster.Where(u => u.WhCode == entity.WhCode && u.AltItemNumber == item.AltItemNumber && (u.Style1 == null ? "" : u.Style1) == (item.Style1 == null ? "" : item.Style1) && (u.Style2 == null ? "" : u.Style2) == (item.Style2 == null ? "" : item.Style2) && (u.Style3 == null ? "" : u.Style3) == (item.Style3 == null ? "" : item.Style3) && u.ClientId == entity.ClientId).First();

                //添加InBoundOrderDetail
                ibo.InsertInBoundOrderDetail(entity, item, inBoundOrder, itemMaster);
            }

            idal.IInBoundOrderDetailDAL.SaveChanges();

            #region 创建收货登记表头
            ReceiptRegister rr = new ReceiptRegister();
            rr.ClientCode = entity.ClientCode;
            rr.ClientId = ClientId;
            rr.CreateDate = DateTime.Now;
            rr.CreateUser = "ECL";
            rr.LocationId = "D01";
            rr.ProcessId = entity.ProcessId;
            rr.ProcessName = entity.ProcessName;
            rr.WhCode = entity.WhCode;
            rr.Status = "U";
            rr.ReceiptType = "Com";
            rr.RegisterDate = DateTime.Now;
            rr.TruckNumber = "";
            //rr.ArriveDate = Convert.ToDateTime(Request["txt_ArriveDate"]);

            rr = rm.AddReceiptRegister(rr);
            #endregion

            #region 添加收货操作单明细
            List<ReceiptRegisterInsert> rrilist = new List<ReceiptRegisterInsert>();
            ReceiptRegisterInsert rri;
            ItemMaster sku;
            InBoundOrder ibod = idal.IInBoundOrderDAL.SelectBy(u => u.WhCode == entity.WhCode && u.ClientId == ClientId && u.CustomerPoNumber == CustomerPoNumber).First();
            List<InBoundOrderDetail> iboddetail = idal.IInBoundOrderDetailDAL.SelectBy(u => u.PoId == ibod.Id);

            foreach (var item in iboddetail)
            {
                sku = idal.IItemMasterDAL.SelectBy(u => u.Id == item.ItemId).First();

                rri = new ReceiptRegisterInsert();
                rri.ReceiptId = rr.ReceiptId;
                rri.WhCode = item.WhCode;
                rri.InBoundOrderDetailId = item.Id;
                rri.CustomerPoNumber = CustomerPoNumber;
                rri.AltItemNumber = sku.AltItemNumber;
                rri.PoId = item.PoId;
                rri.ItemId = item.ItemId;
                rri.UnitId = (int)item.UnitId;
                rri.ProcessName = rr.ProcessName;
                rri.ProcessId = (int)rr.ProcessId;

                if (sku.UnitName == "" || sku.UnitName == null)
                {
                    rri.UnitName = "none";
                }
                else
                {
                    rri.UnitName = sku.UnitName;
                }
                rri.RegQty = item.Qty;
                rri.CreateUser = "ECL";
                rri.CreateDate = DateTime.Now;

                rrilist.Add(rri);

            }

            rm.AddReceiptRegisterDetail(rrilist);
            idal.IInBoundOrderDetailDAL.SaveChanges();
            #endregion

            return "Y$" + rr.ReceiptId;
        }
        #endregion

        #region OMS订单导入调用收货登记
        public string InBoundOrderListAddOms(InBoundOrderInsert entity)
        {
            string result = "";     //执行总结果
            int clientId = 0;

            if (entity == null || entity.InBoundOrderDetailInsert == null || entity.InBoundOrderDetailInsert.Count == 0)
            {
                return "数据有误，请重新操作！";
            }


            if (entity.ClientCode == null || entity.ClientCode == null)
            {
                return "客户为空！";
            }
            else
            {
                List<WhClient> clientL = idal.IWhClientDAL.SelectBy(u => u.ClientCode == entity.ClientCode && u.WhCode == entity.WhCode).ToList();
                if (clientL.Count > 0)
                {
                    clientId = clientL.First().Id;
                    entity.ClientId = clientId;
                }
                else
                {
                    return "客户不存在！";
                }
            }


            #region 判断PO重复
            List<string> tempPO = new List<string>();
            foreach (var item in entity.InBoundOrderDetailInsert)
            {
                if (!tempPO.Equals(item.CustomerPoNumber))
                {
                    tempPO.Add(item.CustomerPoNumber);
                }
            }
            string[] tempPo = tempPO.ToArray();
            int PoCz = idal.IInBoundOrderDAL.SelectBy(u => u.ClientCode == entity.ClientCode && u.SoId == null && tempPo.Contains(u.CustomerPoNumber)).Count();
            if (PoCz != 0)
            {
                return "PO重复导入";
            }
            #endregion


            List<string> poList = new List<string>();
            List<string> skuList = new List<string>();

            List<InBoundOrder> InBoundOrderAddList = new List<InBoundOrder>();
            List<ItemMaster> ItemMasterAddList = new List<ItemMaster>();

            List<InBoundOrder> checkInBoundOrderAddResult = new List<InBoundOrder>();
            List<ItemMaster> checkItemMasterAddResult = new List<ItemMaster>();

            InBoundSO inBoundSO = new InBoundSO();

            if (!string.IsNullOrEmpty(entity.SoNumber))
            {
                //添加InBoundSO 
                List<InBoundSO> listInBoundSO = idal.IInBoundSODAL.SelectBy(u => u.SoNumber == entity.SoNumber && u.WhCode == entity.WhCode && u.ClientCode == entity.ClientCode);

                //如果SO不存在 就添加
                if (listInBoundSO.Count == 0)
                {
                    inBoundSO.WhCode = entity.WhCode;
                    inBoundSO.SoNumber = entity.SoNumber;
                    inBoundSO.ClientCode = entity.ClientCode;
                    inBoundSO.ClientId = (int)entity.ClientId;              //添加新数据 必须赋予客户ID
                    inBoundSO.CreateUser = entity.CreateUser;
                    inBoundSO.CreateDate = DateTime.Now;
                    idal.IInBoundSODAL.Add(inBoundSO);
                    idal.IInBoundSODAL.SaveChanges();
                }
                else
                {
                    //存在，就获取
                    inBoundSO = listInBoundSO.First();
                }
            }

            //批量导入预录入PO

            if (!string.IsNullOrEmpty(entity.SoNumber))
            {
                string[] getPoArr = (from a in entity.InBoundOrderDetailInsert select a.CustomerPoNumber).ToList().Distinct().ToArray();

                List<InBoundOrder> checkPoArr = idal.IInBoundOrderDAL.SelectBy(u => getPoArr.Contains(u.CustomerPoNumber) && u.WhCode == entity.WhCode && u.ClientCode == entity.ClientCode && u.SoId == inBoundSO.Id);

                string[] getSkuArr = (from a in entity.InBoundOrderDetailInsert select a.AltItemNumber).ToList().Distinct().ToArray();

                List<ItemMaster> checkSkuArr = idal.IItemMasterDAL.SelectBy(u => u.WhCode == entity.WhCode && getSkuArr.Contains(u.AltItemNumber) && u.ClientId == entity.ClientId).OrderBy(u => u.Id).ToList();

                foreach (var item in entity.InBoundOrderDetailInsert)
                {
                    if (checkInBoundOrderAddResult.Where(u => u.SoId == inBoundSO.Id && u.CustomerPoNumber == item.CustomerPoNumber).Count() == 0)
                    {
                        if (checkPoArr.Where(u => u.CustomerPoNumber == item.CustomerPoNumber && u.WhCode == entity.WhCode && u.ClientCode == entity.ClientCode && u.SoId == inBoundSO.Id).Count() == 0)
                        {
                            InBoundOrder inBoundOrder = new InBoundOrder();
                            inBoundOrder.WhCode = entity.WhCode;
                            inBoundOrder.SoId = inBoundSO.Id;
                            inBoundOrder.PoNumber = "ST" + DI.IDGenerator.NewId;
                            inBoundOrder.CustomerPoNumber = item.CustomerPoNumber;
                            inBoundOrder.AltCustomerPoNumber = item.AltCustomerPoNumber;
                            inBoundOrder.ClientId = (int)entity.ClientId;
                            inBoundOrder.ClientCode = entity.ClientCode;
                            inBoundOrder.OrderType = entity.OrderType;
                            inBoundOrder.ProcessId = entity.ProcessId;
                            inBoundOrder.ProcessName = entity.ProcessName;
                            inBoundOrder.OrderSource = "WMS";
                            inBoundOrder.CreateUser = item.CreateUser;
                            inBoundOrder.CreateDate = DateTime.Now;
                            InBoundOrderAddList.Add(inBoundOrder);
                        }
                        poList.Add(item.CustomerPoNumber);

                        InBoundOrder inboundResult = new InBoundOrder();
                        inboundResult.SoId = inBoundSO.Id;
                        inboundResult.CustomerPoNumber = item.CustomerPoNumber;
                        checkInBoundOrderAddResult.Add(inboundResult);
                    }

                    if (checkItemMasterAddResult.Where(u => u.AltItemNumber == item.AltItemNumber && (u.Style1 == null ? "" : u.Style1) == (item.Style1 == null ? "" : item.Style1) && (u.Style2 == null ? "" : u.Style2) == (item.Style2 == null ? "" : item.Style2) && (u.Style3 == null ? "" : u.Style3) == (item.Style3 == null ? "" : item.Style3) && u.ClientId == entity.ClientId).Count() == 0)
                    {
                        if (checkSkuArr.Where(u => u.WhCode == entity.WhCode && u.AltItemNumber == item.AltItemNumber && (u.Style1 == null ? "" : u.Style1) == (item.Style1 == null ? "" : item.Style1) && (u.Style2 == null ? "" : u.Style2) == (item.Style2 == null ? "" : item.Style2) && (u.Style3 == null ? "" : u.Style3) == (item.Style3 == null ? "" : item.Style3) && u.ClientId == entity.ClientId).Count() == 0)
                        {
                            ItemMaster itemMaster = new ItemMaster();
                            itemMaster.ItemNumber = "SYS" + DI.IDGenerator.NewId;
                            itemMaster.WhCode = entity.WhCode;
                            itemMaster.AltItemNumber = item.AltItemNumber;
                            itemMaster.ClientId = (int)entity.ClientId;
                            itemMaster.ClientCode = entity.ClientCode;
                            itemMaster.Style1 = item.Style1 ?? "";
                            itemMaster.Style2 = item.Style2 ?? "";
                            itemMaster.Style3 = item.Style3 ?? "";
                            itemMaster.UnitFlag = 0;
                            if (item.UnitName == "" || item.UnitName == null)
                            {
                                itemMaster.UnitName = "none";
                            }
                            else
                            {
                                itemMaster.UnitName = item.UnitName;
                            }

                            itemMaster.CreateUser = item.CreateUser;
                            itemMaster.CreateDate = DateTime.Now;
                            ItemMasterAddList.Add(itemMaster);   //款号不存在就新增
                        }
                        skuList.Add(item.AltItemNumber);

                        ItemMaster itemResult = new ItemMaster();
                        itemResult.ClientId = (int)entity.ClientId;
                        itemResult.AltItemNumber = item.AltItemNumber;
                        itemResult.Style1 = item.Style1 ?? "";
                        itemResult.Style2 = item.Style2 ?? "";
                        itemResult.Style3 = item.Style3 ?? "";
                        checkItemMasterAddResult.Add(itemResult);
                    }

                }
            }
            else
            {
                string[] getPoArr = (from a in entity.InBoundOrderDetailInsert select a.CustomerPoNumber).ToList().Distinct().ToArray();

                List<InBoundOrder> checkPoArr = idal.IInBoundOrderDAL.SelectBy(u => getPoArr.Contains(u.CustomerPoNumber) && u.WhCode == entity.WhCode && u.ClientCode == entity.ClientCode && u.SoId == null);

                string[] getSkuArr = (from a in entity.InBoundOrderDetailInsert select a.AltItemNumber).ToList().Distinct().ToArray();

                List<ItemMaster> checkSkuArr = idal.IItemMasterDAL.SelectBy(u => u.WhCode == entity.WhCode && getSkuArr.Contains(u.AltItemNumber) && u.ClientId == entity.ClientId).OrderBy(u => u.Id).ToList();

                foreach (var item in entity.InBoundOrderDetailInsert)
                {
                    if (checkInBoundOrderAddResult.Where(u => u.SoId == null && u.CustomerPoNumber == item.CustomerPoNumber).Count() == 0)
                    {
                        if (checkPoArr.Where(u => u.CustomerPoNumber == item.CustomerPoNumber && u.WhCode == entity.WhCode && u.ClientCode == entity.ClientCode && u.SoId == null).Count() == 0)
                        {
                            InBoundOrder inBoundOrder = new InBoundOrder();
                            inBoundOrder.WhCode = entity.WhCode;
                            inBoundOrder.PoNumber = "ST" + DI.IDGenerator.NewId;
                            inBoundOrder.CustomerPoNumber = item.CustomerPoNumber;
                            inBoundOrder.AltCustomerPoNumber = item.AltCustomerPoNumber;
                            inBoundOrder.ClientId = (int)entity.ClientId;
                            inBoundOrder.ClientCode = entity.ClientCode;
                            inBoundOrder.OrderType = entity.OrderType;
                            inBoundOrder.ProcessId = entity.ProcessId;
                            inBoundOrder.ProcessName = entity.ProcessName;
                            inBoundOrder.OrderSource = "WMS";
                            inBoundOrder.CreateUser = item.CreateUser;
                            inBoundOrder.CreateDate = DateTime.Now;
                            InBoundOrderAddList.Add(inBoundOrder);
                        }

                        poList.Add(item.CustomerPoNumber);

                        InBoundOrder inboundResult = new InBoundOrder();
                        inboundResult.SoId = null;
                        inboundResult.CustomerPoNumber = item.CustomerPoNumber;
                        checkInBoundOrderAddResult.Add(inboundResult);
                    }

                    if (checkItemMasterAddResult.Where(u => u.AltItemNumber == item.AltItemNumber && (u.Style1 == null ? "" : u.Style1) == (item.Style1 == null ? "" : item.Style1) && (u.Style2 == null ? "" : u.Style2) == (item.Style2 == null ? "" : item.Style2) && (u.Style3 == null ? "" : u.Style3) == (item.Style3 == null ? "" : item.Style3) && u.ClientId == entity.ClientId).Count() == 0)
                    {
                        if (checkSkuArr.Where(u => u.WhCode == entity.WhCode && u.AltItemNumber == item.AltItemNumber && (u.Style1 == null ? "" : u.Style1) == (item.Style1 == null ? "" : item.Style1) && (u.Style2 == null ? "" : u.Style2) == (item.Style2 == null ? "" : item.Style2) && (u.Style3 == null ? "" : u.Style3) == (item.Style3 == null ? "" : item.Style3) && u.ClientId == entity.ClientId).Count() == 0)
                        {
                            ItemMaster itemMaster = new ItemMaster();
                            itemMaster.ItemNumber = "SYS" + DI.IDGenerator.NewId;
                            itemMaster.WhCode = entity.WhCode;
                            itemMaster.AltItemNumber = item.AltItemNumber;
                            itemMaster.ClientId = (int)entity.ClientId;
                            itemMaster.ClientCode = entity.ClientCode;
                            itemMaster.Style1 = item.Style1 ?? "";
                            itemMaster.Style2 = item.Style2 ?? "";
                            itemMaster.Style3 = item.Style3 ?? "";
                            itemMaster.UnitFlag = 0;
                            if (item.UnitName == "" || item.UnitName == null)
                            {
                                itemMaster.UnitName = "none";
                            }
                            else
                            {
                                itemMaster.UnitName = item.UnitName;
                            }

                            itemMaster.CreateUser = item.CreateUser;
                            itemMaster.CreateDate = DateTime.Now;
                            ItemMasterAddList.Add(itemMaster);   //款号不存在就新增
                        }
                        skuList.Add(item.AltItemNumber);

                        ItemMaster itemResult = new ItemMaster();
                        itemResult.ClientId = (int)entity.ClientId;
                        itemResult.AltItemNumber = item.AltItemNumber;
                        itemResult.Style1 = item.Style1 ?? "";
                        itemResult.Style2 = item.Style2 ?? "";
                        itemResult.Style3 = item.Style3 ?? "";
                        checkItemMasterAddResult.Add(itemResult);
                    }
                }
            }


            if (InBoundOrderAddList.Count > 0)
            {
                idal.IInBoundOrderDAL.Add(InBoundOrderAddList);
                idal.IInBoundSODAL.SaveChanges();
            }

            if (ItemMasterAddList.Count > 0)
            {
                idal.IItemMasterDAL.Add(ItemMasterAddList);
                idal.IItemMasterDAL.SaveChanges();
            }

            List<InBoundOrder> listInBoundOrder = idal.IInBoundOrderDAL.SelectBy(u => poList.Contains(u.CustomerPoNumber) && u.WhCode == entity.WhCode);

            List<ItemMaster> listItemMaster = idal.IItemMasterDAL.SelectBy(u => skuList.Contains(u.AltItemNumber) && u.WhCode == entity.WhCode).OrderBy(u => u.Id).ToList();


            foreach (var item in entity.InBoundOrderDetailInsert)
            {
                InBoundOrder inBoundOrder = new InBoundOrder();

                if (!string.IsNullOrEmpty(entity.SoNumber))
                {
                    inBoundOrder = listInBoundOrder.Where(u => u.CustomerPoNumber == item.CustomerPoNumber && u.ClientCode == entity.ClientCode && u.SoId == inBoundSO.Id).First();
                }
                else
                {
                    inBoundOrder = listInBoundOrder.Where(u => u.CustomerPoNumber == item.CustomerPoNumber && u.ClientCode == entity.ClientCode && u.SoId == null).First();
                }

                //判断款号是否存在
                ItemMaster itemMaster = listItemMaster.Where(u => u.WhCode == entity.WhCode && u.AltItemNumber == item.AltItemNumber && (u.Style1 == null ? "" : u.Style1) == (item.Style1 == null ? "" : item.Style1) && (u.Style2 == null ? "" : u.Style2) == (item.Style2 == null ? "" : item.Style2) && (u.Style3 == null ? "" : u.Style3) == (item.Style3 == null ? "" : item.Style3) && u.ClientId == entity.ClientId).First();

                //添加InBoundOrderDetail
                ibo.InsertInBoundOrderDetail(entity, item, inBoundOrder, itemMaster);
                //result = "Y";
            }

            result = AddReceiptBs(entity.InBoundOrderDetailInsert.First().CustomerPoNumber, entity.ClientCode, (int)entity.ClientId, "D01", entity.WhCode, entity.CreateUser);

            if (result != "" && result != null)
            {
                //更新车牌
                if (entity.TruckNumber != "" && entity.TruckNumber != null)
                {
                    ReceiptRegister rr = new ReceiptRegister();
                    rr.ReceiptId = result;
                    rr.WhCode = entity.WhCode;
                    rr.TruckNumber = entity.TruckNumber;
                    ribo.EditReceiptRegister(rr, new string[] { "TruckNumber" });

                }

                return "Y$" + result;
            }
            else
            {
                return "N";
            }
        }
        #endregion

        #region OMS收货登记

        public string AddReceiptBs(string InBoundNumber, string ClientCode, int ClientId, string LocationId, string WhCode, string User)
        {


            //查询入库订单表头并校验流程是否相同
            List<InBoundOrder> InBoundOrderL = idal.IInBoundOrderDAL.SelectBy(u => u.CustomerPoNumber == InBoundNumber && u.ClientCode == ClientCode && u.WhCode == WhCode);
            int ProcessId = 0;
            foreach (var item in InBoundOrderL)
            {
                if (ProcessId == 0)
                {
                    ProcessId = (int)item.ProcessId;
                }
                else
                {
                    if (ProcessId != item.ProcessId)
                    {
                        return "所选SO包含有不同操作流程!";
                    }
                }
            }
            //查询入库单明细并校验是否存在登记情况
            List<int> PoIdL = new List<int>();
            foreach (var item in InBoundOrderL)
            {
                PoIdL.Add(item.Id);
            }
            List<InBoundOrderDetail> InBoundOrderDetailL = idal.IInBoundOrderDetailDAL.SelectBy(u => PoIdL.Contains((int)u.PoId) && u.WhCode == WhCode);


            //创建收货登记表头
            ReceiptRegister receiptRegister = new ReceiptRegister();
            receiptRegister.WhCode = WhCode;
            receiptRegister.ClientCode = ClientCode;
            receiptRegister.ClientId = ClientId;
            receiptRegister.LocationId = LocationId;
            receiptRegister.ReceiptType = "OMS";
            receiptRegister = ribo.AddReceiptRegister(receiptRegister);
            idal.SaveChanges();
            //添加收货登记明细
            List<ReceiptRegisterInsert> ReceiptRegisterInsertL = new List<ReceiptRegisterInsert>();
            foreach (var item in InBoundOrderDetailL)
            {
                ReceiptRegisterInsert itemR = new ReceiptRegisterInsert();

                itemR.InBoundOrderDetailId = item.Id;
                itemR.ReceiptId = receiptRegister.ReceiptId;
                itemR.WhCode = item.WhCode;
                //itemR.CustomerPoNumber = idal.IInBoundOrderDAL.SelectBy(u=>u.Id==item.PoId).First().CustomerPoNumber;
                //itemR.AltItemNumber = idal.IItemMasterDAL.SelectBy(u=>u.Id==item.ItemId).First().AltItemNumber;
                itemR.PoId = item.PoId;
                itemR.ItemId = item.ItemId;
                itemR.RegQty = item.Qty - item.RegQty;
                itemR.CreateUser = User;
                itemR.CreateDate = System.DateTime.Now;
                item.UnitId = item.UnitId;
                itemR.UnitName = item.UnitName;
                itemR.ProcessId = (int)InBoundOrderL.First().ProcessId;
                itemR.ProcessName = InBoundOrderL.First().ProcessName;

                if (itemR.RegQty > 0)
                {
                    ReceiptRegisterInsertL.Add(itemR);
                }

            }
            //维护ReceiptRegisterInsertL中的CustomerPoNumber和AltItemNumber
            string CustomerPoNumber;
            string AltItemNumber;
            foreach (var item in ReceiptRegisterInsertL)
            {
                if (item.CustomerPoNumber == null || item.CustomerPoNumber == "")
                {
                    CustomerPoNumber = idal.IInBoundOrderDAL.SelectBy(u => u.Id == item.PoId).First().CustomerPoNumber;
                    foreach (var item1 in ReceiptRegisterInsertL)
                    {
                        if (item1.PoId == item.PoId)
                        {
                            item1.CustomerPoNumber = CustomerPoNumber;
                        }
                    }
                }

                if (item.AltItemNumber == null || item.AltItemNumber == "")
                {
                    AltItemNumber = idal.IItemMasterDAL.SelectBy(u => u.Id == item.ItemId).First().AltItemNumber;
                    foreach (var item1 in ReceiptRegisterInsertL)
                    {
                        if (item1.ItemId == item.ItemId)
                        {
                            item1.AltItemNumber = AltItemNumber;
                        }
                    }
                }
            }

            ribo.AddReceiptRegisterDetail(ReceiptRegisterInsertL);

            return receiptRegister.ReceiptId;
        }

        #endregion





        #region OMSDMSSID订单导入调用收货登记
        public string InBoundOrderListAddOmsSSID(InBoundOrderInsert entity)
        {
            string result = "";     //执行总结果
            int clientId = 0;



            if (entity == null || entity.InBoundOrderDetailInsert == null || entity.InBoundOrderDetailInsert.Count == 0)
            {
                return "数据有误，请重新操作！";
            }

            foreach (var item in entity.InBoundOrderDetailInsert)
            {

                if (item.SSID == null || item.SSID == "")
                {
                    return "明细无SSID!";
                }
            }

            if (entity.ClientCode == null || entity.ClientCode == null)
            {
                return "客户为空！";
            }
            else
            {
                List<WhClient> clientL = idal.IWhClientDAL.SelectBy(u => u.ClientCode == entity.ClientCode && u.WhCode == entity.WhCode).ToList();
                if (clientL.Count > 0)
                {
                    clientId = clientL.First().Id;
                    entity.ClientId = clientId;
                }
                else
                {
                    return "客户不存在！";
                }
            }


            #region 判断PO重复
            List<string> tempPO = new List<string>();
            foreach (var item in entity.InBoundOrderDetailInsert)
            {
                if (!tempPO.Equals(item.CustomerPoNumber))
                {
                    tempPO.Add(item.CustomerPoNumber);
                }
            }
            string[] tempPo = tempPO.ToArray();

            //获取SO
            int soID = 0;
            List<InBoundSO> Lso = idal.IInBoundSODAL.SelectBy(u => u.SoNumber == entity.SoNumber && u.WhCode == "02");
            if (Lso.Count > 0)
            {
                soID = Lso.First().Id;
            }

            int PoCz = idal.IInBoundOrderDAL.SelectBy(u => u.ClientCode == entity.ClientCode && u.SoId == soID && tempPo.Contains(u.CustomerPoNumber)).Count();
            if (PoCz != 0)
            {
                return "SOPO重复导入";
            }
            #endregion


            List<string> poList = new List<string>();
            List<string> skuList = new List<string>();

            List<InBoundOrder> InBoundOrderAddList = new List<InBoundOrder>();
            List<ItemMaster> ItemMasterAddList = new List<ItemMaster>();

            List<InBoundOrder> checkInBoundOrderAddResult = new List<InBoundOrder>();
            List<ItemMaster> checkItemMasterAddResult = new List<ItemMaster>();

            InBoundSO inBoundSO = new InBoundSO();

            if (!string.IsNullOrEmpty(entity.SoNumber))
            {
                //添加InBoundSO 
                List<InBoundSO> listInBoundSO = idal.IInBoundSODAL.SelectBy(u => u.SoNumber == entity.SoNumber && u.WhCode == entity.WhCode && u.ClientCode == entity.ClientCode);

                //如果SO不存在 就添加
                if (listInBoundSO.Count == 0)
                {
                    inBoundSO.WhCode = entity.WhCode;
                    inBoundSO.SoNumber = entity.SoNumber;
                    inBoundSO.ClientCode = entity.ClientCode;
                    inBoundSO.ClientId = (int)entity.ClientId;              //添加新数据 必须赋予客户ID
                    inBoundSO.CreateUser = entity.CreateUser;
                    inBoundSO.CreateDate = DateTime.Now;
                    idal.IInBoundSODAL.Add(inBoundSO);
                    idal.IInBoundSODAL.SaveChanges();
                }
                else
                {
                    //存在，就获取
                    inBoundSO = listInBoundSO.First();
                }
            }

            //批量导入预录入PO

            if (!string.IsNullOrEmpty(entity.SoNumber))
            {

                string[] getPoArr = (from a in entity.InBoundOrderDetailInsert select a.CustomerPoNumber).ToList().Distinct().ToArray();

                List<InBoundOrder> checkPoArr = idal.IInBoundOrderDAL.SelectBy(u => getPoArr.Contains(u.CustomerPoNumber) && u.WhCode == entity.WhCode && u.ClientCode == entity.ClientCode && u.SoId == inBoundSO.Id);

                string[] getSkuArr = (from a in entity.InBoundOrderDetailInsert select a.AltItemNumber).ToList().Distinct().ToArray();

                List<ItemMaster> checkSkuArr = idal.IItemMasterDAL.SelectBy(u => u.WhCode == entity.WhCode && getSkuArr.Contains(u.AltItemNumber) && u.ClientId == entity.ClientId).OrderBy(u => u.Id).ToList();

                foreach (var item in entity.InBoundOrderDetailInsert)
                {
                    if (checkInBoundOrderAddResult.Where(u => u.SoId == inBoundSO.Id && u.CustomerPoNumber == item.CustomerPoNumber).Count() == 0)
                    {
                        if (checkPoArr.Where(u => u.CustomerPoNumber == item.CustomerPoNumber && u.WhCode == entity.WhCode && u.ClientCode == entity.ClientCode && u.SoId == inBoundSO.Id).Count() == 0)
                        {
                            InBoundOrder inBoundOrder = new InBoundOrder();
                            inBoundOrder.WhCode = entity.WhCode;
                            inBoundOrder.SoId = inBoundSO.Id;
                            inBoundOrder.PoNumber = "ST" + DI.IDGenerator.NewId;
                            inBoundOrder.CustomerPoNumber = item.CustomerPoNumber;
                            inBoundOrder.AltCustomerPoNumber = item.AltCustomerPoNumber;
                            inBoundOrder.ClientId = (int)entity.ClientId;
                            inBoundOrder.ClientCode = entity.ClientCode;
                            inBoundOrder.OrderType = entity.OrderType;
                            inBoundOrder.ProcessId = entity.ProcessId;
                            inBoundOrder.ProcessName = entity.ProcessName;
                            inBoundOrder.OrderSource = "WMS";
                            inBoundOrder.CreateUser = item.CreateUser;
                            inBoundOrder.CreateDate = DateTime.Now;
                            InBoundOrderAddList.Add(inBoundOrder);
                        }
                        poList.Add(item.CustomerPoNumber);

                        InBoundOrder inboundResult = new InBoundOrder();
                        inboundResult.SoId = inBoundSO.Id;
                        inboundResult.CustomerPoNumber = item.CustomerPoNumber;
                        checkInBoundOrderAddResult.Add(inboundResult);
                    }

                    if (checkItemMasterAddResult.Where(u => u.AltItemNumber == item.AltItemNumber && (u.Style1 == null ? "" : u.Style1) == (item.Style1 == null ? "" : item.Style1) && (u.Style2 == null ? "" : u.Style2) == (item.Style2 == null ? "" : item.Style2) && (u.Style3 == null ? "" : u.Style3) == (item.Style3 == null ? "" : item.Style3) && u.ClientId == entity.ClientId).Count() == 0)
                    {
                        if (checkSkuArr.Where(u => u.WhCode == entity.WhCode && u.AltItemNumber == item.AltItemNumber && (u.Style1 == null ? "" : u.Style1) == (item.Style1 == null ? "" : item.Style1) && (u.Style2 == null ? "" : u.Style2) == (item.Style2 == null ? "" : item.Style2) && (u.Style3 == null ? "" : u.Style3) == (item.Style3 == null ? "" : item.Style3) && u.ClientId == entity.ClientId).Count() == 0)
                        {
                            ItemMaster itemMaster = new ItemMaster();
                            itemMaster.ItemNumber = "SYS" + DI.IDGenerator.NewId;
                            itemMaster.WhCode = entity.WhCode;
                            itemMaster.AltItemNumber = item.AltItemNumber;
                            itemMaster.ClientId = (int)entity.ClientId;
                            itemMaster.ClientCode = entity.ClientCode;
                            itemMaster.Style1 = item.Style1 ?? "";
                            itemMaster.Style2 = item.Style2 ?? "";
                            itemMaster.Style3 = item.Style3 ?? "";
                            itemMaster.UnitFlag = 0;
                            if (item.UnitName == "" || item.UnitName == null)
                            {
                                itemMaster.UnitName = "none";
                            }
                            else
                            {
                                itemMaster.UnitName = item.UnitName;
                            }

                            itemMaster.CreateUser = item.CreateUser;
                            itemMaster.CreateDate = DateTime.Now;
                            ItemMasterAddList.Add(itemMaster);   //款号不存在就新增
                        }
                        skuList.Add(item.AltItemNumber);

                        ItemMaster itemResult = new ItemMaster();
                        itemResult.ClientId = (int)entity.ClientId;
                        itemResult.AltItemNumber = item.AltItemNumber;
                        itemResult.Style1 = item.Style1 ?? "";
                        itemResult.Style2 = item.Style2 ?? "";
                        itemResult.Style3 = item.Style3 ?? "";
                        checkItemMasterAddResult.Add(itemResult);
                    }

                }
            }
            else
            {
                string[] getPoArr = (from a in entity.InBoundOrderDetailInsert select a.CustomerPoNumber).ToList().Distinct().ToArray();

                List<InBoundOrder> checkPoArr = idal.IInBoundOrderDAL.SelectBy(u => getPoArr.Contains(u.CustomerPoNumber) && u.WhCode == entity.WhCode && u.ClientCode == entity.ClientCode && u.SoId == null);

                string[] getSkuArr = (from a in entity.InBoundOrderDetailInsert select a.AltItemNumber).ToList().Distinct().ToArray();

                List<ItemMaster> checkSkuArr = idal.IItemMasterDAL.SelectBy(u => u.WhCode == entity.WhCode && getSkuArr.Contains(u.AltItemNumber) && u.ClientId == entity.ClientId).OrderBy(u => u.Id).ToList();

                foreach (var item in entity.InBoundOrderDetailInsert)
                {
                    if (checkInBoundOrderAddResult.Where(u => u.SoId == null && u.CustomerPoNumber == item.CustomerPoNumber).Count() == 0)
                    {
                        if (checkPoArr.Where(u => u.CustomerPoNumber == item.CustomerPoNumber && u.WhCode == entity.WhCode && u.ClientCode == entity.ClientCode && u.SoId == null).Count() == 0)
                        {
                            InBoundOrder inBoundOrder = new InBoundOrder();
                            inBoundOrder.WhCode = entity.WhCode;
                            inBoundOrder.PoNumber = "ST" + DI.IDGenerator.NewId;
                            inBoundOrder.CustomerPoNumber = item.CustomerPoNumber;
                            inBoundOrder.AltCustomerPoNumber = item.AltCustomerPoNumber;
                            inBoundOrder.ClientId = (int)entity.ClientId;
                            inBoundOrder.ClientCode = entity.ClientCode;
                            inBoundOrder.OrderType = entity.OrderType;
                            inBoundOrder.ProcessId = entity.ProcessId;
                            inBoundOrder.ProcessName = entity.ProcessName;
                            inBoundOrder.OrderSource = "WMS";
                            inBoundOrder.CreateUser = item.CreateUser;
                            inBoundOrder.CreateDate = DateTime.Now;
                            InBoundOrderAddList.Add(inBoundOrder);
                        }

                        poList.Add(item.CustomerPoNumber);

                        InBoundOrder inboundResult = new InBoundOrder();
                        inboundResult.SoId = null;
                        inboundResult.CustomerPoNumber = item.CustomerPoNumber;
                        checkInBoundOrderAddResult.Add(inboundResult);
                    }

                    if (checkItemMasterAddResult.Where(u => u.AltItemNumber == item.AltItemNumber && (u.Style1 == null ? "" : u.Style1) == (item.Style1 == null ? "" : item.Style1) && (u.Style2 == null ? "" : u.Style2) == (item.Style2 == null ? "" : item.Style2) && (u.Style3 == null ? "" : u.Style3) == (item.Style3 == null ? "" : item.Style3) && u.ClientId == entity.ClientId).Count() == 0)
                    {
                        if (checkSkuArr.Where(u => u.WhCode == entity.WhCode && u.AltItemNumber == item.AltItemNumber && (u.Style1 == null ? "" : u.Style1) == (item.Style1 == null ? "" : item.Style1) && (u.Style2 == null ? "" : u.Style2) == (item.Style2 == null ? "" : item.Style2) && (u.Style3 == null ? "" : u.Style3) == (item.Style3 == null ? "" : item.Style3) && u.ClientId == entity.ClientId).Count() == 0)
                        {
                            ItemMaster itemMaster = new ItemMaster();
                            itemMaster.ItemNumber = "SYS" + DI.IDGenerator.NewId;
                            itemMaster.WhCode = entity.WhCode;
                            itemMaster.AltItemNumber = item.AltItemNumber;
                            itemMaster.ClientId = (int)entity.ClientId;
                            itemMaster.ClientCode = entity.ClientCode;
                            itemMaster.Style1 = item.Style1 ?? "";
                            itemMaster.Style2 = item.Style2 ?? "";
                            itemMaster.Style3 = item.Style3 ?? "";
                            itemMaster.UnitFlag = 0;
                            if (item.UnitName == "" || item.UnitName == null)
                            {
                                itemMaster.UnitName = "none";
                            }
                            else
                            {
                                itemMaster.UnitName = item.UnitName;
                            }

                            itemMaster.CreateUser = item.CreateUser;
                            itemMaster.CreateDate = DateTime.Now;
                            ItemMasterAddList.Add(itemMaster);   //款号不存在就新增
                        }
                        skuList.Add(item.AltItemNumber);

                        ItemMaster itemResult = new ItemMaster();
                        itemResult.ClientId = (int)entity.ClientId;
                        itemResult.AltItemNumber = item.AltItemNumber;
                        itemResult.Style1 = item.Style1 ?? "";
                        itemResult.Style2 = item.Style2 ?? "";
                        itemResult.Style3 = item.Style3 ?? "";
                        checkItemMasterAddResult.Add(itemResult);
                    }
                }
            }


            if (InBoundOrderAddList.Count > 0)
            {
                idal.IInBoundOrderDAL.Add(InBoundOrderAddList);
                idal.IInBoundSODAL.SaveChanges();
            }

            if (ItemMasterAddList.Count > 0)
            {
                idal.IItemMasterDAL.Add(ItemMasterAddList);
                idal.IItemMasterDAL.SaveChanges();
            }

            List<InBoundOrder> listInBoundOrder = idal.IInBoundOrderDAL.SelectBy(u => poList.Contains(u.CustomerPoNumber) && u.WhCode == entity.WhCode);

            List<ItemMaster> listItemMaster = idal.IItemMasterDAL.SelectBy(u => skuList.Contains(u.AltItemNumber) && u.WhCode == entity.WhCode).OrderBy(u => u.Id).ToList();


            foreach (var item in entity.InBoundOrderDetailInsert)
            {
                InBoundOrder inBoundOrder = new InBoundOrder();

                if (!string.IsNullOrEmpty(entity.SoNumber))
                {
                    inBoundOrder = listInBoundOrder.Where(u => u.CustomerPoNumber == item.CustomerPoNumber && u.ClientCode == entity.ClientCode && u.SoId == inBoundSO.Id).First();
                }
                else
                {
                    inBoundOrder = listInBoundOrder.Where(u => u.CustomerPoNumber == item.CustomerPoNumber && u.ClientCode == entity.ClientCode && u.SoId == null).First();
                }

                //判断款号是否存在
                ItemMaster itemMaster = listItemMaster.Where(u => u.WhCode == entity.WhCode && u.AltItemNumber == item.AltItemNumber && (u.Style1 == null ? "" : u.Style1) == (item.Style1 == null ? "" : item.Style1) && (u.Style2 == null ? "" : u.Style2) == (item.Style2 == null ? "" : item.Style2) && (u.Style3 == null ? "" : u.Style3) == (item.Style3 == null ? "" : item.Style3) && u.ClientId == entity.ClientId).First();

                //添加InBoundOrderDetail
                ibo.InsertInBoundOrderDetail(entity, item, inBoundOrder, itemMaster);
                //result = "Y";
            }

            //return "Y$ssssss";

            result = AddReceiptBsSSID(entity.SoNumber, entity.ClientCode, (int)entity.ClientId, "D01", entity.WhCode, entity.CreateUser);

            if (result != "" && result != null)
            {
                //更新车牌
                if (entity.TruckNumber != "" && entity.TruckNumber != null)
                {
                    ReceiptRegister rr = new ReceiptRegister();
                    rr.ReceiptId = result;
                    rr.WhCode = entity.WhCode;
                    rr.TruckNumber = entity.TruckNumber;
                    ribo.EditReceiptRegister(rr, new string[] { "TruckNumber" });

                }

                return "Y$" + result;
            }
            else
            {
                return "N";
            }
        }
        #endregion

        #region OMSDMSSID收货登记

        public string AddReceiptBsSSID(string SoNumber, string ClientCode, int ClientId, string LocationId, string WhCode, string User)
        {
            int soID = 0;
            List<InBoundSO> Lso = idal.IInBoundSODAL.SelectBy(u => u.SoNumber == SoNumber && u.WhCode == "02");
            if (Lso.Count > 0)
            {
                soID = Lso.First().Id;
            }

            //查询入库订单表头并校验流程是否相同
            List<InBoundOrder> InBoundOrderL = idal.IInBoundOrderDAL.SelectBy(u => u.SoId == soID && u.ClientCode == ClientCode && u.WhCode == WhCode);
            int ProcessId = 0;
            foreach (var item in InBoundOrderL)
            {
                if (ProcessId == 0)
                {
                    ProcessId = (int)item.ProcessId;
                }
                else
                {
                    if (ProcessId != item.ProcessId)
                    {
                        return "所选SO包含有不同操作流程!";
                    }
                }
            }
            //查询入库单明细并校验是否存在登记情况
            List<int> PoIdL = new List<int>();
            foreach (var item in InBoundOrderL)
            {
                PoIdL.Add(item.Id);
            }
            List<InBoundOrderDetail> InBoundOrderDetailL = idal.IInBoundOrderDetailDAL.SelectBy(u => PoIdL.Contains((int)u.PoId) && u.WhCode == WhCode);


            //创建收货登记表头
            ReceiptRegister receiptRegister = new ReceiptRegister();
            receiptRegister.WhCode = WhCode;
            receiptRegister.ClientCode = ClientCode;
            receiptRegister.ClientId = ClientId;
            receiptRegister.LocationId = LocationId;
            receiptRegister.ReceiptType = "OMS";
            receiptRegister = ribo.AddReceiptRegister(receiptRegister);
            idal.SaveChanges();
            //添加收货登记明细
            List<ReceiptRegisterInsert> ReceiptRegisterInsertL = new List<ReceiptRegisterInsert>();
            foreach (var item in InBoundOrderDetailL)
            {
                ReceiptRegisterInsert itemR = new ReceiptRegisterInsert();

                itemR.InBoundOrderDetailId = item.Id;
                itemR.ReceiptId = receiptRegister.ReceiptId;
                itemR.WhCode = item.WhCode;
                //itemR.CustomerPoNumber = idal.IInBoundOrderDAL.SelectBy(u=>u.Id==item.PoId).First().CustomerPoNumber;
                //itemR.AltItemNumber = idal.IItemMasterDAL.SelectBy(u=>u.Id==item.ItemId).First().AltItemNumber;
                itemR.PoId = item.PoId;
                itemR.ItemId = item.ItemId;
                itemR.RegQty = item.Qty - item.RegQty;
                itemR.CreateUser = User;
                itemR.CreateDate = System.DateTime.Now;
                item.UnitId = item.UnitId;
                itemR.UnitName = item.UnitName;
                itemR.ProcessId = (int)InBoundOrderL.First().ProcessId;
                itemR.ProcessName = InBoundOrderL.First().ProcessName;

                if (itemR.RegQty > 0)
                {
                    ReceiptRegisterInsertL.Add(itemR);
                }

            }
            //维护ReceiptRegisterInsertL中的CustomerPoNumber和AltItemNumber
            string CustomerPoNumber;
            string AltItemNumber;
            foreach (var item in ReceiptRegisterInsertL)
            {
                if (item.CustomerPoNumber == null || item.CustomerPoNumber == "")
                {
                    CustomerPoNumber = idal.IInBoundOrderDAL.SelectBy(u => u.Id == item.PoId).First().CustomerPoNumber;
                    foreach (var item1 in ReceiptRegisterInsertL)
                    {
                        if (item1.PoId == item.PoId)
                        {
                            item1.CustomerPoNumber = CustomerPoNumber;
                        }
                    }
                }

                if (item.AltItemNumber == null || item.AltItemNumber == "")
                {
                    AltItemNumber = idal.IItemMasterDAL.SelectBy(u => u.Id == item.ItemId).First().AltItemNumber;
                    foreach (var item1 in ReceiptRegisterInsertL)
                    {
                        if (item1.ItemId == item.ItemId)
                        {
                            item1.AltItemNumber = AltItemNumber;
                        }
                    }
                }
            }

            ribo.AddReceiptRegisterDetail(ReceiptRegisterInsertL);

            return receiptRegister.ReceiptId;
        }

        #endregion




        public void UrlEdiTaskInsertRec(string ReceiptId, string WhCode, string CreateUser)
        {
            //ReceiptRegister rr = idal.IReceiptRegisterDAL.SelectBy(u => u.ReceiptId == ReceiptId && u.WhCode == WhCode).First();

            List<FlowHead> getflowHeadList = (from a in idal.IReceiptRegisterDAL.SelectAll()
                                              join b in idal.IFlowHeadDAL.SelectAll()
                                              on a.ProcessId equals b.Id
                                              where a.WhCode == WhCode && a.ReceiptId == ReceiptId
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
                    uet.Mark = ReceiptId;
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
                    uet1.Mark = ReceiptId;
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
                    uet2.Mark = ReceiptId;
                    uet2.HttpType = url.HttpType;
                    uet2.Status = 1;
                    uet2.CreateDate = DateTime.Now;
                    idal.IUrlEdiTaskDAL.Add(uet2);
                }
            }

            //if (rr.ReceiptType == "Ecl")
            //{

            //    UrlEdiTask uet = new UrlEdiTask();
            //    uet.Status = 1;
            //    uet.Type = "ECL";
            //    uet.Url = "http://10.88.88.90/net/ecl/in_order_rec_comfirm.aspx?actionType=WmsRecUpdate&WhCode=" + WhCode;
            //    uet.Field = "ReceiptId";
            //    uet.Mark = ReceiptId;
            //    uet.HttpType = "Get";

            //    idal.IUrlEdiTaskDAL.Add(uet);

            //}

            //if (rr.ReceiptType == "OMS")
            //{

            //    UrlEdiTask uet = new UrlEdiTask();
            //    uet.Status = 1;
            //    uet.Type = "OMS";
            //    uet.Url = "http://10.88.88.90/net/oms/in_order.aspx?actionType=WmsRecUpdate&WhCode=" + WhCode;
            //    uet.Field = "ReceiptId";
            //    uet.Mark = ReceiptId;
            //    uet.HttpType = "Get";

            //    idal.IUrlEdiTaskDAL.Add(uet);

            //}
        }

        #region 基础数据维护

        //新增款号
        public string ItemMasterAddOms(List<ItemMaster> iml)
        {

            foreach (var item in iml)
            {
                rm.ItemMaterAdd(item);
            }
            idal.SaveChanges();

            return "1";
        }

        //修改款号

        public string ItemMasterUpdateOms(ItemMaster im)
        {

            rm.ItemMaterUpdate(im);

            idal.SaveChanges();

            return "1";
        }
        #endregion
    }
}

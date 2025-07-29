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


    public class BsInBoundManager : IBsInBoundManager
    {
        IDAL.IDALSession idal = BLL.BLLHelper.GetDal();
        InBoundOrderManager ibo = new InBoundOrderManager();
        RegInBoundOrderManager ribo = new RegInBoundOrderManager();

        #region 1.保税区EIP订单导入

        public string InBoundOrderListAddBs(InBoundOrderInsert entity)
        {
            string result = "";     //执行总结果
            int clientId = 0;

            if (entity == null || entity.InBoundOrderDetailInsert == null)
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

            //if (entity.SoNumber != null && entity.SoNumber != "")
            //{

            //    int SoCz = idal.IInBoundSODAL.SelectBy(u => u.SoNumber == entity.SoNumber && u.ClientCode == entity.ClientCode).Count();
            //    if (SoCz != 0)
            //    {
            //        return "SO重复导入!";
            //    }
            //}

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
                result = "Y";
            }

            return result;
        }


        #endregion

        #region 2.保税区EIP收货登记
        public string AddReceiptBs(string[] SO, string ClientCode, string LocationId, string WhCode, string User)
        {
            WhClient Client = new WhClient();

            List<string> SO1 = SO.ToList();
            SO1 = SO1.Distinct<string>().ToList();


            // string [] so3= SO1.Distinct().ToArray();
            //查询客户名
            List<WhClient> ClientL = idal.IWhClientDAL.SelectBy(u => u.ClientCode == ClientCode && u.WhCode == WhCode);
            if (ClientL.Count() == 0)
            {
                return "WMS中客户不存在";
            }
            else
            {
                Client = ClientL.First();
            }


            //查询SO
            List<InBoundSO> SoL = idal.IInBoundSODAL.SelectBy(u => u.WhCode == WhCode && u.ClientCode == ClientCode && SO.Contains(u.SoNumber));
            if (SoL.Count() != SO1.Count())
            {
                return "所选SO与WMS_SO数量不符";
            }

            List<int> SoIdL = new List<int>();
            foreach (var item in SoL)
            {
                SoIdL.Add(item.Id);
            }
            //查询入库订单表头并校验流程是否相同
            List<InBoundOrder> InBoundOrderL = idal.IInBoundOrderDAL.SelectBy(u => SoIdL.Contains((int)u.SoId) && u.WhCode == WhCode);
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
            //foreach (var item in InBoundOrderDetailL)
            //{
            //    if (item.Qty-item.RegQty <= 0)
            //    {
            //        //InBoundOrderDetailL.
            //    }
            //}

            //创建收货登记表头
            ReceiptRegister receiptRegister = new ReceiptRegister();
            receiptRegister.WhCode = WhCode;
            receiptRegister.ClientCode = ClientCode;
            receiptRegister.ClientId = Client.Id;
            receiptRegister.LocationId = LocationId;
            receiptRegister.ReceiptType = "BS";
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

            return "Y$" + receiptRegister.ReceiptId;
        }

        #endregion
    }
}

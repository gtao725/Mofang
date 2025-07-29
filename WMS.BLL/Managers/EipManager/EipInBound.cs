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
    public class EipInBound : IEipInBound
    {
        IDAL.IDALSession idal = BLL.BLLHelper.GetDal();
        IBLL.ILoadManager lm = new BLL.LoadManager();
        InBoundOrderManager ibo = new InBoundOrderManager();
        RegInBoundOrderManager ribo = new RegInBoundOrderManager();
        IBLL.IOutBoundOrderManager obom = new OutBoundOrderManager();
        RootManager rm = new RootManager();

        public List<InBoundOrderInsert> ImportsInBoundOrderTransformation(List<ImportsInBoundOrderInsert> entity)
        {
            List<InBoundOrderInsert> iboL = new List<InBoundOrderInsert>();

            foreach (var item in entity)
            {
                if (iboL.Where(u => u.SoNumber == item.SoNumber).Count() == 0)
                {
                    InBoundOrderInsert ibo = new InBoundOrderInsert();
                    ibo.WhCode = item.whCode;
                    ibo.SoNumber = item.SoNumber;

                    ibo.ProcessId = item.ProcessId;
                    ibo.ProcessName = item.ProcessName;

                    ibo.ClientCode = item.ClientCode;
                    ibo.CreateUser = item.CreateUser;
                    ibo.OrderType = item.OrderType;

                    List<InBoundOrderDetailInsert> list = new List<InBoundOrderDetailInsert>();
                    //循环找SO的明细赋值
                    foreach (var item2 in entity.Where(u => u.SoNumber == item.SoNumber))
                    {
                        InBoundOrderDetailInsert ibod = new InBoundOrderDetailInsert();

                        ibod.AltItemNumber = item2.AltItemNumber;
                        ibod.CBM = item2.CBM;
                        ibod.CreateUser = item2.CreateUser;
                        ibod.CustomerPoNumber = item2.CustomerPoNumber;
                        ibod.Qty = (int)item2.Qty;
                        ibod.Style1 = item2.Style1;
                        ibod.Style2 = item2.Style2;
                        ibod.Style3 = item2.Style3;
                        ibod.UnitName = item2.UnitName;
                        ibod.Weight = item2.Weight;
                        list.Add(ibod);     
                    }

                    ibo.InBoundOrderDetailInsert = list;
                    iboL.Add(ibo);
                }
            }
            return iboL;
        }


        public string EipInsertInBound(List<InBoundOrderInsert> entity)
        {
            string ClientCode = entity.First().ClientCode;
            string WhCode = entity.First().WhCode;
            int clientId;
            int ZoneId;
            int SoId = 0;
            int PoId;
            int ItemId;
            string ZoneName = "暂无";
            List<ReceiptRegisterInsert> rrl = new List<ReceiptRegisterInsert>();
            ReceiptRegisterInsert rri;
            InBoundOrderDetail iod;

            if (ClientCode == null || ClientCode == "")
            {
                return "客户为空！";
            }
            else
            {
                List<WhClient> clientL = idal.IWhClientDAL.SelectBy(u => u.ClientCode == ClientCode && u.WhCode == WhCode).ToList();
                if (clientL.Count > 0)
                {
                    clientId = clientL.First().Id;
                    ZoneId = clientL.First().ZoneId.Value;
                    if (ZoneId != 0)
                    {
                        ZoneName = idal.IZoneDAL.SelectBy(u => u.Id == ZoneId).First().ZoneName;
                    }
                }
                else
                {
                    return "客户不存在！";
                }
            }

            ReceiptRegister rr = new ReceiptRegister();
            rr.ClientId = clientId;
            rr.ClientCode = ClientCode;
            rr.WhCode = WhCode;
            rr.LocationId = ZoneName;
            rr.ReceiptType = "";

            ReceiptRegister Receipt = ribo.AddReceiptRegister(rr);


            foreach (var item in entity)
            {
                item.ClientId = clientId;
                ibo.InBoundOrderListAddCommon(item);

                if (item.SoNumber != "" && item.SoNumber != null)
                {
                    SoId = idal.IInBoundSODAL.SelectBy(u => u.ClientCode == item.ClientCode && u.WhCode == item.WhCode && u.SoNumber == item.SoNumber).First().Id;

                }

                foreach (var itemdetail in item.InBoundOrderDetailInsert)
                {

                    ItemId = idal.IItemMasterDAL.SelectBy(u => u.WhCode == item.WhCode && u.AltItemNumber == itemdetail.AltItemNumber && u.Style1 == (itemdetail.Style1 ?? "") && u.Style2 == (itemdetail.Style2 ?? "") && u.Style3 == (itemdetail.Style3 ?? "") && u.ClientCode == item.ClientCode).OrderBy(u => u.Id).ToList().First().Id;
                    if (item.SoNumber != "" && item.SoNumber != null)
                    {
                        PoId = idal.IInBoundOrderDAL.SelectBy(u => u.WhCode == item.WhCode && u.ClientCode == item.ClientCode && u.SoId == SoId && u.CustomerPoNumber == itemdetail.CustomerPoNumber).First().Id;

                    }
                    else
                    {
                        PoId = idal.IInBoundOrderDAL.SelectBy(u => u.WhCode == item.WhCode && u.ClientCode == item.ClientCode && u.SoId == null && u.CustomerPoNumber == itemdetail.CustomerPoNumber).First().Id;
                    }


                    rri = new ReceiptRegisterInsert();

                    iod = idal.IInBoundOrderDetailDAL.SelectBy(u => u.PoId == PoId && u.ItemId == ItemId).First();

                    rri.InBoundOrderDetailId = iod.Id;
                    rri.ReceiptId = Receipt.ReceiptId;
                    rri.WhCode = item.WhCode;
                    rri.CustomerPoNumber = itemdetail.CustomerPoNumber;
                    rri.AltItemNumber = itemdetail.AltItemNumber;
                    rri.PoId = PoId;
                    rri.ItemId = ItemId;
                    rri.RegQty = itemdetail.Qty;
                    rri.CreateUser = item.CreateUser;
                    rri.CreateDate = DateTime.Now;
                    rri.UnitId = itemdetail.UnitId;
                    rri.UnitName = itemdetail.UnitName;
                    rri.ProcessId = item.ProcessId.Value;
                    rri.ProcessName = item.ProcessName;
                    rrl.Add(rri);
                }
            }

            ribo.AddReceiptRegisterDetail(rrl);

            return "Y$" + Receipt.ReceiptId;
        }

        public string LoadMasterDel(string LoadId, string Whcode, string User)
        {
            string res = "";
            List<LoadMaster> lml = idal.ILoadMasterDAL.SelectBy(u => u.LoadId == LoadId && u.WhCode == Whcode);
            if (lml.Count > 0)
            {
                int loadmasterId = lml.First().Id;
                List<LoadDetail> ldl = idal.ILoadDetailDAL.SelectBy(u => u.LoadMasterId == loadmasterId);

                res = lm.LoadMasterDel(loadmasterId);
                if (res == "Y")
                {
                    foreach (var item in ldl)
                    {
                        obom.OutBoundOrderDel((int)item.OutBoundOrderId);
                    }

                }
                return res;

            }
            else
            {
                return "未查询到该LOAD";
            }

        }

        public string CheckRegInBoundSo(string SoNumber, string Whcode, string ClientCode)
        {
            string res = "Y";

            List<InBoundOrderDetailResult> list = (from feedetail in
                                                    (from inbouso in idal.IInBoundSODAL.SelectAll()
                                                     join inbouorder in idal.IInBoundOrderDAL.SelectAll()
                                                     on inbouso.Id equals inbouorder.SoId
                                                     join inorderDetail in idal.IInBoundOrderDetailDAL.SelectAll()
                                                     on inbouorder.Id equals inorderDetail.PoId
                                                     where inbouso.WhCode == Whcode && inbouso.SoNumber == SoNumber && inbouso.ClientCode == ClientCode && inorderDetail.RegQty > 0
                                                     select new
                                                     {
                                                         Dummy = "x"
                                                     })
                                                   group feedetail by new { feedetail.Dummy } into g
                                                   select new InBoundOrderDetailResult
                                                   {
                                                       Qty = g.Count()
                                                   }).ToList();
            if (list.Count == 0)
            {
                res = "Y";
            }
            else
            {
                res = "已登记";
            }
            return res;
        }

        public string ImportsGWI(List<GwiDetailInsert> entity)
        {
            DamcoGRNHead gh = new DamcoGRNHead();
            DamcoGRNDetail gd;


            string ClientCode = entity.First().ClientCode;
            string WhCode = entity.First().WhCode;
            string SO = entity.First().SO;
            string SoCode = entity.First().SoCode;
            string SoDigit = entity.First().SoDigit;
            string CreateUser = entity.First().CreateUser;
            string SiteId = entity.First().SiteId;
            string CustomerId = entity.First().CustomerId;


            List<DamcoGRNHead> dghl =  idal.IDamcoGRNHeadDAL.SelectBy(u => u.SoNumber == SO && u.WhCode== WhCode&& u.ClientCode== ClientCode) ;
            int total = dghl.Count();
            if (total > 0)
            {
                DamcoGRNHead dgh = dghl.First();
                if(dgh.WmsCbm!=null)
                {
                    return "已收货,无法更新GWI！";
                }

            }


            if (ClientCode == null || ClientCode == "")
            {
                return "客户为空！";
            }
            if (WhCode == null || WhCode == "")
            {
                return "仓库Code为空！";
            }
            if (SO == null || SO == "")
            {
                return "SO为空！";
            }

            //idal.IDamcoGRNHeadDAL.DeleteBy(u => u.SoNumber == SO && u.WhCode == WhCode && u.ClientCode == ClientCode);
            //idal.IDamcoGRNDetailDAL.DeleteBy(u => u.SoNumber == SO && u.WhCode == WhCode && u.ClientCode == ClientCode);

            idal.IDamcoGRNHeadDAL.DeleteBy(u => u.SoNumber == SO);
            idal.IDamcoGRNDetailDAL.DeleteBy(u => u.SoNumber == SO);

            gh.SoNumber = SO;
            gh.ClientCode = ClientCode;
            gh.WhCode = WhCode;
            gh.CreateDate = DateTime.Now;
            gh.SoCode = SoCode;
            gh.SoDigit= SoDigit;
            gh.CreateUser = CreateUser;
            gh.SiteId = SiteId;
            gh.CustomerId = CustomerId;

            idal.IDamcoGRNHeadDAL.Add(gh);

            foreach (var item in entity)
            {
                gd = new DamcoGRNDetail();
                gd.ClientCode = ClientCode;
                gd.WhCode = WhCode;
                gd.SoNumber = item.SO;
                gd.LN = item.LN;
                gd.PoNumber = item.PO;
                gd.AltItemNumber = item.SKU;
                gd.Style = item.STYLE;
                gd.GWI_Qty = item.GWI_Qty;
                gd.GWI_Pcs = item.GWI_Pcs;
                gd.GWI_Cbm = item.GWI_Cbm;
                gd.GWI_Kgs = item.GWI_Kgs;
                gd.GWI_nwKgs = item.GWI_nwKgs;
                gd.SoCode = SoCode;
                gd.SoDigit = SoDigit;

                idal.IDamcoGRNDetailDAL.Add(gd);
            }

            idal.SaveChanges();
            return "Y";
        }


    }
}

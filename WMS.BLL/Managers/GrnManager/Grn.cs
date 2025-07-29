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
    public class Grn : IGrn
    {
        IDAL.IDALSession idal = BLL.BLLHelper.GetDal();
        public string CheckSoLN(string sonumber)
        {
            List<string> soL = new List<string>();
            int soqty = idal.IDamcoGRNHeadDAL.SelectBy(u => u.SoNumber == sonumber).Count();
            string res = "";
            if (soqty == 0)
            {
                res = sonumber + "在GWI中不存在";
            }
            else if (soqty > 1)
            {
                res = sonumber + "重复";
            }
            else
            {
                res = "Y";
            }

            if (res == "Y")
            {
                int sodetailqty = idal.IDamcoGRNDetailDAL.SelectBy(u => u.SoNumber == sonumber).Count();
                if (sodetailqty == 0)
                {
                    res = sonumber + "明细在GWI中不存在";
                }
            }
            return res;
        }

        public List<GrnSoUpdateSearch> GetReceipSo(string receipid)
        {
            List<GrnSoUpdateSearch> soL = new List<GrnSoUpdateSearch>();
            List<Receipt> so = idal.IReceiptDAL.SelectBy(u => u.ReceiptId == receipid);

            foreach (var item in so)
            {
                if (soL.Where(u => u.WhCode == item.WhCode && u.ClientCode == item.ClientCode && u.SoNumber == item.SoNumber).Count() == 0)
                {
                    GrnSoUpdateSearch gs = new GrnSoUpdateSearch();
                    gs.ClientCode = item.ClientCode;
                    gs.WhCode = item.WhCode;
                    gs.SoNumber = item.SoNumber;

                    if (item.SoNumber != "")
                        soL.Add(gs);
                }
            }

            return soL;
        }

        public string GrnAutoUpdate(string sonumber, string Whcode, string ClientCode)
        {
            DamcoGrnRule rule = null;
            List<DamcoGrnRule> li = idal.IDamcoGrnRuleDAL.SelectBy(u => u.ClientCode == ClientCode && u.WhCode == Whcode);
            if (li.Count() > 0)
            {
                rule = li.First();
            }
            else
            {
                return "Grn更新失败此客户无GrnRule配置";
            }

            List<DamcoGRNDetail> dl = idal.IDamcoGRNDetailDAL.SelectBy(u => u.SoNumber == sonumber && u.ClientCode == rule.ClientCode && u.WhCode == rule.WhCode).ToList();


            //if (rule.AvgFlag == 1)
            //{
            //    List<HuDetail> hl = idal.IHuDetailDAL.SelectBy(u => u.SoNumber == sonumber && u.ClientCode == rule.ClientCode && u.WhCode == rule.WhCode).ToList();
            //    GrnAvgUpdate(sonumber, rule, dl, hl);
            //    return "";
            //}



            GrnAutoUpdateCbm(sonumber, rule, dl);
            GrnAutoUpdateKgs(sonumber, rule, dl);
            GrnAutoUpdateReceiptDate(sonumber, rule, dl);
            idal.SaveChanges();

            return "Y";

        }

        public string GrnAvgUpdate(string sonumber, DamcoGrnRule rule, List<DamcoGRNDetail> dl, List<HuDetail> hl)
        {
            string error_str = "";
            var wmsPsku = hl.GroupBy(u => new { u.SoNumber, u.CustomerPoNumber, u.AltItemNumber }).Select(g => (new { SoNumber1 = g.Count(), SoNumber = g.Key.SoNumber, CustomerPoNumber = g.Key.CustomerPoNumber, AltItemNumber = g.Key.AltItemNumber, Qty = g.Sum(item => item.Qty), Cbm = g.Sum(item => item.Qty * item.Length * item.Width * item.Height) })).ToList();
            var wmsPpo = hl.GroupBy(u => new { u.SoNumber, u.CustomerPoNumber }).Select(g => (new { SoNumber1 = g.Count(), SoNumber = g.Key.SoNumber, CustomerPoNumber = g.Key.CustomerPoNumber, Qty = g.Sum(item => item.Qty), Cbm = g.Sum(item => item.Qty * item.Length * item.Width * item.Height) })).ToList();
            var gwiPsku = dl.GroupBy(u => new { u.SoNumber, u.PoNumber, u.AltItemNumber }).Select(g => (new { SoNumber1 = g.Count(), SoNumber = g.Key.SoNumber, CustomerPoNumber = g.Key.PoNumber, AltItemNumber = g.Key.AltItemNumber, Qty = g.Sum(item => item.GWI_Qty) })).ToList();
            var gwiPpo = dl.GroupBy(u => new { u.SoNumber, u.PoNumber }).Select(g => (new { SoNumber1 = g.Count(), SoNumber = g.Key.SoNumber, CustomerPoNumber = g.Key.PoNumber, Qty = g.Sum(item => item.GWI_Qty) })).ToList();

            if (wmsPsku.Count != gwiPsku.Count)
                error_str = "SKU不匹配";
            if (error_str == "")
            {

                return "";
            }


            return "";
        }

        public string GrnAutoUpdateCbm(string sonumber, DamcoGrnRule rule, List<DamcoGRNDetail> dl)
        {
            if (rule.ClientCode == "HM")
            {
                double? wmscbm = 0;
                double? gwicbm = 0;
                List<DamcoGRNHead> dgh = idal.IDamcoGRNHeadDAL.SelectBy(u => u.SoNumber == sonumber).ToList();
                if (dgh.Count > 0)
                {
                    wmscbm = dgh.First().WmsCbm;
                }

                foreach (var item in dl)
                {
                    gwicbm = gwicbm + item.GWI_Cbm;
                }

                if (gwicbm > 0 && wmscbm > 0)
                {
                    double cha = Math.Abs(Convert.ToDouble(gwicbm - wmscbm));

                    if (gwicbm <= 1 && wmscbm <= 1)
                        rule.CBMSource = "GWI";
                    if (gwicbm > 1 && cha < 0.5)
                        rule.CBMSource = "GWI";
                    if (gwicbm > 1 && cha >= 0.5)
                        rule.CBMSource = "WMS";
                }

            }

            foreach (var item in dl)
            {

                DamcoGRNDetail dr = new DamcoGRNDetail();
                dr.Id = item.Id;
                if (rule.CBMSource == "WMS")
                    dr.GRN_Cbm = item.WMS_Cbm;
                if (rule.CBMSource == "GWI")
                    dr.GRN_Cbm = decimal.Parse(item.GWI_Cbm.ToString());
                dr.GRN_Qty = item.WMS_Qty;
                idal.IDamcoGRNDetailDAL.UpdateBy(dr, u => u.Id == dr.Id, new string[] { "GRN_Cbm", "GRN_Qty" });

            }
            return "Y";

        }

        public string GrnAutoUpdateKgs(string sonumber, DamcoGrnRule rule, List<DamcoGRNDetail> dl)
        {
            foreach (var item in dl)
            {

                DamcoGRNDetail dr = new DamcoGRNDetail();
                dr.Id = item.Id;
                if (rule.KgsSource == "WMS")
                    dr.GRN_Kgs = item.WMS_Kgs;
                if (rule.KgsSource == "GWI")
                    dr.GRN_Kgs = item.GWI_Kgs;
                idal.IDamcoGRNDetailDAL.UpdateBy(dr, u => u.Id == dr.Id, new string[] { "GRN_Kgs" });

            }
            return "Y";
        }

        public string GrnAutoUpdateReceiptDate(string sonumber, DamcoGrnRule rule, List<DamcoGRNDetail> dl)
        {
            string ReceiptId;
            DateTime? RegDate = null;
            Receipt r = null;
            if (rule.ReceiptDateSource == "10")
            {
                List<Receipt> rrl = idal.IReceiptDAL.SelectBy(u => u.SoNumber == sonumber).Take(1).ToList();
                if (rrl.Count > 0)
                {
                    r = rrl.First();
                }
                if (r != null)
                {
                    ReceiptId = r.ReceiptId;
                    List<ReceiptRegister> rl = idal.IReceiptRegisterDAL.SelectBy(u => u.ReceiptId == ReceiptId);
                    if (rl.Count > 0)
                    {
                        DateTime RegDate1 = (DateTime)(rl.First().CreateDate);
                        string sss = RegDate1.ToString("yyyy-MM-dd 10:00");
                        RegDate = DateTime.Parse(sss);
                    }
                }
            }


            foreach (var item in dl)
            {

                DamcoGRNDetail dr = new DamcoGRNDetail();
                dr.Id = item.Id;
                //if (rule.ReceiptDateSource == "1")
                //{
                dr.GRN_ReceiptDate = item.WMS_ReceiptDate;

                if (rule.ReceiptDateSource == "10" && RegDate != null)
                    dr.GRN_ReceiptDate = RegDate;

                idal.IDamcoGRNDetailDAL.UpdateBy(dr, u => u.Id == dr.Id, new string[] { "GRN_ReceiptDate" });

            }
            return "Y";
        }

        public string SetGrn(string receiptid)
        {
            List<GrnSoUpdateSearch> sol = GetReceipSo(receiptid);
            string res = "";
            foreach (var item in sol)
            {
                res = CheckSoLN(item.SoNumber);
                if (res == "Y")
                {
                    UpdateGrnWmsData(item.SoNumber, item.ClientCode, item.WhCode, "sys");
                }
            }

            return "Y";
        }


        //更新WMS总数量CBM到GRN头表,查找客户RULE 判断级别更新detail
        public string UpdateGrnWmsData(string sonumber, string clientcode, string whcode, string User)
        {
            if (sonumber == null || sonumber == "")
                return "SO为空";

            //获取clientRule
            DamcoGrnRule rule = null;
            List<DamcoGrnRule> li = idal.IDamcoGrnRuleDAL.SelectBy(u => u.ClientCode == clientcode && u.WhCode == whcode);
            if (li.Count() > 0)
            {
                rule = li.First();
            }


            //List<Receipt> rl = idal.IReceiptDAL.SelectBy(u => u.SoNumber == sonumber);

            //SO_PO_SKU_STYLE
            var sql = from a in idal.IHuDetailDAL.SelectAll()
                      join b in idal.IItemMasterDAL.SelectAll()
                            on new { a.ItemId }
                        equals new { ItemId = b.Id } into b_join
                      from b in b_join.DefaultIfEmpty()
                      where
                        a.SoNumber == sonumber && a.ClientCode == clientcode && a.WhCode == whcode
                      group new { a, b } by new
                      {
                          a.SoNumber,
                          a.CustomerPoNumber,
                          a.AltItemNumber,
                          a.UnitName,
                          b.Style1
                      } into g
                      select new
                      {
                          g.Key.SoNumber,
                          g.Key.CustomerPoNumber,
                          g.Key.AltItemNumber,
                          UnitName = g.Key.UnitName,
                          Style1 = g.Key.Style1,
                          ReceiptDate = (DateTime?)g.Max(p => p.a.ReceiptDate),
                          Qty = (Int32?)g.Sum(p => p.a.Qty),
                          CBM = Math.Round((System.Double)g.Sum(p => p.a.Qty * p.a.Width * p.a.Length * p.a.Height), 6),
                          Kgs = (System.Decimal?)g.Sum(p => p.a.Qty * p.a.Weight)
                      };

            //SO_PO 
            var sql_PO_Level = from a in idal.IHuDetailDAL.SelectAll()
                               join b in idal.IItemMasterDAL.SelectAll()
                                     on new { a.ItemId }
                                 equals new { ItemId = b.Id } into b_join
                               from b in b_join.DefaultIfEmpty()
                               where
                                 a.SoNumber == sonumber && a.ClientCode == clientcode && a.WhCode == whcode
                               group new { a, b } by new
                               {
                                   a.SoNumber,
                                   a.CustomerPoNumber
                               } into g
                               select new
                               {
                                   g.Key.SoNumber,
                                   g.Key.CustomerPoNumber,
                                   ReceiptDate = (DateTime?)g.Max(p => p.a.ReceiptDate),
                                   Qty = (Int32?)g.Sum(p => p.a.Qty),
                                   CBM = Math.Round((System.Double)g.Sum(p => p.a.Qty * p.a.Width * p.a.Length * p.a.Height), 6),
                                   Kgs = (System.Decimal?)g.Sum(p => p.a.Qty * p.a.Weight)
                               };



            int i = 0;
            int? soqty = 0;
            decimal? WmsCbm = 0;


            if (rule != null && rule.MatchLevel == "SO_PO")
            {
                foreach (var item in sql_PO_Level)
                {
                    soqty = soqty + item.Qty;
                    WmsCbm = WmsCbm + (decimal?)item.CBM;
                    int lastId = 0;

                    List<DamcoGRNDetail> dl = idal.IDamcoGRNDetailDAL.SelectBy(u => u.SoNumber == item.SoNumber && u.PoNumber == item.CustomerPoNumber ).ToList();
                    if (dl.Count() == 1)
                    {
                        DamcoGRNDetail dr = new DamcoGRNDetail();
                        dr.Id = dl.First().Id;
                        dr.WMS_Cbm = decimal.Parse(item.CBM.ToString());
                        dr.WMS_Kgs = double.Parse(item.Kgs.ToString());
                        dr.WMS_Qty = item.Qty;
                        dr.WMS_ReceiptDate = item.ReceiptDate;
                        dr.UpdateDate = System.DateTime.Now;
                        dr.UpdateUser = User;
                        idal.IDamcoGRNDetailDAL.UpdateBy(dr, u => u.Id == dr.Id, new string[] { "WMS_Cbm", "WMS_Kgs", "WMS_Qty", "WMS_ReceiptDate", "UpdateDate", "UpdateUser" });
                        i++;
                    }
                    else if (dl.Count() > 1)
                    {
                        int GwiTotal = Convert.ToInt32(dl.Sum(u => u.GWI_Qty));
                        if (GwiTotal == item.Qty)
                        {
                            foreach (var gd in dl)
                            {
                                double xs = (double)gd.GWI_Qty / (double)item.Qty;
                                lastId = gd.Id;
                                DamcoGRNDetail dr = new DamcoGRNDetail();
                                dr.Id = gd.Id;
                                dr.WMS_Cbm = Convert.ToDecimal((double.Parse(item.CBM.ToString()) * xs).ToString("f3"));
                                dr.WMS_Kgs = double.Parse(item.Kgs.ToString()) * xs;
                                dr.WMS_Qty = gd.GWI_Qty;
                                dr.WMS_ReceiptDate = item.ReceiptDate;
                                dr.UpdateDate = System.DateTime.Now;
                                dr.UpdateUser = User;
                                idal.IDamcoGRNDetailDAL.UpdateBy(dr, u => u.Id == dr.Id, new string[] { "WMS_Cbm", "WMS_Kgs", "WMS_Qty", "WMS_ReceiptDate", "UpdateDate", "UpdateUser" });
                                i++;
                            }

                            double aa = Convert.ToDouble(idal.IDamcoGRNDetailDAL.SelectBy(u => u.SoNumber == item.SoNumber && u.PoNumber == item.CustomerPoNumber ).Sum(u => u.WMS_Cbm));
                            decimal bb = Convert.ToDecimal(item.CBM.ToString("f3")) - Convert.ToDecimal(aa.ToString("f3"));
                            DamcoGRNDetail lastdr = idal.IDamcoGRNDetailDAL.SelectBy(u => u.Id == lastId).First();
                            lastdr.WMS_Cbm = lastdr.WMS_Cbm + bb;
                            idal.IDamcoGRNDetailDAL.UpdateBy(lastdr, u => u.Id == lastdr.Id, new string[] { "WMS_Cbm" });
                        }




                    }

                }
            }
            else if (rule != null && rule.MatchLevel == "SO_PO_SKU_STYLE")
            {
                foreach (var item in sql)
                {
                    soqty = soqty + item.Qty;
                    WmsCbm = WmsCbm + (decimal?)item.CBM;
                    int lastId = 0;

                    List<DamcoGRNDetail> dl = idal.IDamcoGRNDetailDAL.SelectBy(u => u.SoNumber == item.SoNumber && u.PoNumber == item.CustomerPoNumber && u.AltItemNumber == item.AltItemNumber && (u.Style ?? "") == (item.Style1 ?? "")).ToList();
                    if (dl.Count() == 1)
                    {
                        //dl.First().Id
                        DamcoGRNDetail dr = new DamcoGRNDetail();
                        dr.Id = dl.First().Id;
                        dr.WMS_Cbm = decimal.Parse(item.CBM.ToString());
                        dr.WMS_Kgs = double.Parse(item.Kgs.ToString());
                        dr.WMS_Qty = item.Qty;
                        dr.WMS_ReceiptDate = item.ReceiptDate;
                        dr.UpdateDate = System.DateTime.Now;
                        dr.UpdateUser = User;
                        idal.IDamcoGRNDetailDAL.UpdateBy(dr, u => u.Id == dr.Id, new string[] { "WMS_Cbm", "WMS_Kgs", "WMS_Qty", "WMS_ReceiptDate", "UpdateDate", "UpdateUser" });
                        i++;
                    }
                    else if (dl.Count() > 1)
                    {
                        int GwiTotal = Convert.ToInt32(dl.Sum(u => u.GWI_Qty));
                        if (GwiTotal == item.Qty)
                        {
                            foreach (var gd in dl)
                            {
                                double xs = (double)gd.GWI_Qty / (double)item.Qty;
                                lastId = gd.Id;
                                DamcoGRNDetail dr = new DamcoGRNDetail();
                                dr.Id = gd.Id;
                                dr.WMS_Cbm = Convert.ToDecimal((double.Parse(item.CBM.ToString()) * xs).ToString("f3"));
                                dr.WMS_Kgs = double.Parse(item.Kgs.ToString()) * xs;
                                dr.WMS_Qty = gd.GWI_Qty;
                                dr.WMS_ReceiptDate = item.ReceiptDate;
                                dr.UpdateDate = System.DateTime.Now;
                                dr.UpdateUser = User;
                                idal.IDamcoGRNDetailDAL.UpdateBy(dr, u => u.Id == dr.Id, new string[] { "WMS_Cbm", "WMS_Kgs", "WMS_Qty", "WMS_ReceiptDate", "UpdateDate", "UpdateUser" });
                                i++;
                            }

                            double aa = Convert.ToDouble(idal.IDamcoGRNDetailDAL.SelectBy(u => u.SoNumber == item.SoNumber && u.PoNumber == item.CustomerPoNumber && u.AltItemNumber == item.AltItemNumber && u.Style == item.Style1).Sum(u => u.WMS_Cbm));
                            decimal bb = Convert.ToDecimal(item.CBM.ToString("f3")) - Convert.ToDecimal(aa.ToString("f3"));
                            DamcoGRNDetail lastdr = idal.IDamcoGRNDetailDAL.SelectBy(u => u.Id == lastId).First();
                            lastdr.WMS_Cbm = lastdr.WMS_Cbm + bb;
                            idal.IDamcoGRNDetailDAL.UpdateBy(lastdr, u => u.Id == lastdr.Id, new string[] { "WMS_Cbm" });
                        }




                    }

                }

            }


            

            if (soqty > 0)
            {
                List<DamcoGRNHead> dhl = idal.IDamcoGRNHeadDAL.SelectBy(u => u.WhCode == whcode && u.ClientCode == clientcode && u.SoNumber == sonumber).ToList();
                DamcoGRNHead dh = new DamcoGRNHead();
                if (dhl.Count() > 0)
                    dh = dhl.First();
                dh.WmsQty = soqty;
                dh.WmsCbm = Convert.ToDouble(WmsCbm);
                dh.UpdateDate = System.DateTime.Now;
                dh.UpdateUser = User;

                idal.IDamcoGRNHeadDAL.UpdateBy(dh, u => u.SoNumber == sonumber && u.WhCode == whcode && u.ClientCode == clientcode, new string[] { "WmsQty", "WmsCbm", "UpdateDate", "UpdateUser" });


            }

            idal.SaveChanges();
            if (i == 0 && User != "WmsAuto")
            {

                return sonumber + "GRNDetail更新失败:GWI与WMS不匹配";
            }

            return "Y";
        }


        public string SendGRN(string sonumber, string Whcode, string ClientCode, string user)
        {
            List<DamcoGRNHead> dhl = idal.IDamcoGRNHeadDAL.SelectBy(u => u.SoNumber == sonumber && u.WhCode == Whcode && u.ClientCode == ClientCode).ToList();
            if (dhl.Count() == 0)
            {
                return "发送失败" + sonumber + "无DamcoGRNHead数据";
            }
            DamcoGRNHead dh = dhl.First();



            List<DamcoGrnRule> drl = idal.IDamcoGrnRuleDAL.SelectBy(u => u.WhCode == Whcode && u.ClientCode == ClientCode).ToList();
            if (drl.Count() == 0)
            {
                UpdateGrnHeadRemark(dh.Id, "该客户无Rule数据");
                return "发送失败" + ClientCode + "无Rule数据";
            }
            DamcoGrnRule dr = drl.First();


            List<DamcoGRNDetail> dl = idal.IDamcoGRNDetailDAL.SelectBy(u => u.SoNumber == sonumber && u.WhCode == Whcode && u.ClientCode == ClientCode).ToList();
            if (dl.Count() == 0)
            {
                UpdateGrnHeadRemark(dh.Id, "无GWI明细数据");
                return "发送失败" + sonumber + "无GWI明细数据";
            }

            int error_qty = 0;
            int? tol_qty = 0;
            decimal? tol_cbm = 0;
            int? tol_gwi_qty = 0;
            double? tol_gwi_cbm = 0;
            int onlyheadflag = 0;
            foreach (var item in dl)
            {
                if (item.GRN_Cbm == 0 || item.GRN_Cbm == null)
                    error_qty++;
                if (item.GRN_Qty == 0 || item.GRN_Qty == null)
                    error_qty++;
                if (item.GRN_Qty !=item.GWI_Qty)
                    error_qty++;
                if (item.GRN_ReceiptDate == null)
                    error_qty++;
                tol_qty = tol_qty + item.GRN_Qty;
                tol_cbm = tol_cbm + item.GRN_Cbm;
                tol_gwi_qty = tol_gwi_qty + item.GWI_Qty;
                tol_gwi_cbm = tol_gwi_cbm + item.GWI_Cbm;
            }
            if (error_qty > 0 && dr.TotalCheck != 1)
            {
                UpdateGrnHeadRemark(dh.Id, "发送检测:WMS实收明细与GWILN不符");
                return "发送失败" + sonumber + "GRNdetail中GRN数据缺失,可能是WMS实收明细与GWILN不符";
            }

            if ((dh.WmsCbm == null || dh.WmsQty == null) && user == "WmsAuto")
            {
                UpdateGrnHeadRemark(dh.Id, "数据异常,头数据无WMS立方和总数量");
                return "发送失败" + sonumber + "GrnHead无WMS立方和总数量";
            }

            if (tol_gwi_qty != dh.WmsQty)
            {
                UpdateGrnHeadRemark(dh.Id, "实收总数量与GWI不符");
                return "发送失败" + sonumber + "实收总数量与GWI不符";
            }

            if (tol_gwi_qty == dh.WmsQty && tol_gwi_qty != tol_qty)
            {
                if (dr.TotalCheck == 1)
                {
                    onlyheadflag = 1;
                }
                else
                {
                    UpdateGrnHeadRemark(dh.Id, "数据错误!,实收总数与GWI相符,但GRNLN数据不完整,");
                    return "发送失败" + sonumber + "总数对,GwiLN无法对应";
                }

            }

            if (dr.AutoSend != 1 && user == "WmsAuto")
            {
                UpdateGrnHeadRemark(dh.Id, "客户设置不自动发送");
                return "发送失败:" + sonumber + "客户设置不自动发送";
            }

            //判断是否发送差异过大提示邮件
            if (dr.DifferenceRate != 0 && user == "WmsAuto" && dr.DifferentialMailFlag == 1)
            {
                double l = (double)(dh.WmsCbm / tol_gwi_cbm);
                if (l > 1)
                    l = l - 1.0;
                else
                    l = 1.0 - l;

                if (Math.Abs(l * 100) > dr.DifferenceRate && dr.MailTo + "" != "")
                {
                    UrlEdiTask uet1 = new UrlEdiTask();
                    uet1.WhCode = Whcode;
                    uet1.Type = "GRN";
                    uet1.Url = "http://10.88.88.90/net/mf_cfs/grnsendmail.aspx?actionType=InGrnSendMail&WhCode=" + Whcode;
                    uet1.Field = "SO";
                    uet1.Mark = sonumber;
                    uet1.HttpType = "Get";
                    uet1.Status = 1;
                    uet1.CreateDate = DateTime.Now;
                    idal.IUrlEdiTaskDAL.Add(uet1);
                    idal.SaveChanges();
                }
            }

            //是否拦截
            if (dr.DifferenceRate != 0 && user == "WmsAuto" && dr.DifferentialInterceptionFlag==1)
            {
                double l = (double)(dh.WmsCbm / tol_gwi_cbm);
                if (l > 1)
                    l = l - 1.0;
                else
                    l = 1.0 - l;

                if (Math.Abs(l * 100) > dr.DifferenceRate)
                {
                    UpdateGrnHeadRemark(dh.Id, "立方差异大于客户设置百分之" + dr.DifferenceRate);
                    return "发送失败:" + sonumber + "立方差异大于预设值";
                }
            }

         



            DamcoGRNHead dhnew = new DamcoGRNHead();
            dhnew.Id = dh.Id;
            dhnew.SendTime = System.DateTime.Now;
            dhnew.Status = "待发送";
            dhnew.SendType = "REC";
            //dhnew.SendType = (dh.SendType == null ? "REC" : "COR");
            dhnew.UpdateUser = user;
            dhnew.UpdateDate = System.DateTime.Now;
            dhnew.Remark = "";
            idal.IDamcoGRNHeadDAL.UpdateBy(dhnew, u => u.SoNumber == sonumber && u.WhCode == Whcode && u.ClientCode == ClientCode, new string[] { "SendTime", "Status", "SendType", "UpdateUser", "UpdateDate", "Remark" });


            UrlEdiTask uet = new UrlEdiTask();
            uet.WhCode = Whcode;
            uet.Type = "GRN";


            if (dr.Sendtype == "SFTP")
            {
                uet.Url = "http://10.88.88.90/NET/MF_CFS/GrnSendKmart.aspx?actionType=InGrnSend&onlyheadflag=" + onlyheadflag + "&WhCode=" + Whcode;
            }else if (dr.Sendtype == "NSCP")
            {
                uet.Url = "http://10.88.88.90/NET/MF_CFS/GrnSend_NSCP.aspx?actionType=CreateJson&s_type=" + onlyheadflag + "&WhCode=" + Whcode;
            }
            else
            {
                uet.Url = "http://10.88.88.90/NET/MF_CFS/GrnSend.aspx?actionType=InGrnSend&onlyheadflag=" + onlyheadflag + "&WhCode=" + Whcode;
            }



            uet.Field = "SO";
            uet.Mark = sonumber;
            uet.HttpType = "Get";
            uet.Status = 1;
            uet.CreateDate = DateTime.Now;
            idal.IUrlEdiTaskDAL.Add(uet);
            idal.SaveChanges();

            return "Y";
        }

        public string AutoSendGRN(string receiptid, string Whcode, string user)//收货引用主方法
        {
            string res = "";
            List<GrnSoUpdateSearch> sol = new List<GrnSoUpdateSearch>();
            ReceiptRegister rr = idal.IReceiptRegisterDAL.SelectBy(u => u.ReceiptId == receiptid && u.WhCode == Whcode).First();
            List<DamcoGrnRule> drl = idal.IDamcoGrnRuleDAL.SelectBy(u => u.WhCode == Whcode && u.ClientCode == rr.ClientCode);
            if (drl.Count() == 1)
            {
                sol = GetReceipSo(receiptid);
            }
            else if (drl.Count() > 1)
            {
                res = "客户GrnRule重复维护";

                return res;
            }
            else
            {
                res = "客户GrnRule未维护";
                return res;
            }





            //添加日志
            TranLog tl1 = new TranLog();
            tl1.TranType = "650";
            tl1.Description = "收货GRN开始执行";
            tl1.TranDate = DateTime.Now;
            tl1.TranUser = user;
            tl1.WhCode = Whcode;
            tl1.ReceiptId = receiptid;
            idal.ITranLogDAL.Add(tl1);

            foreach (var item in sol)
            {
                //添加日志
                TranLog tl2 = new TranLog();
                tl2.TranType = "650";
                tl2.Description = "收货GRN开始验证SO";
                tl2.TranDate = DateTime.Now;
                tl2.TranUser = user;
                tl2.WhCode = Whcode;
                tl2.SoNumber = item.SoNumber;
                tl2.ReceiptId = receiptid;
                idal.ITranLogDAL.Add(tl2);

                string sores;
                sores = UpdateGrnWmsData(item.SoNumber, item.ClientCode, item.WhCode, user);
                if (sores == "Y")
                    sores = GrnAutoUpdate(item.SoNumber, item.WhCode, item.ClientCode);
                if (sores == "Y")
                    sores = SendGRN(item.SoNumber, item.WhCode, item.ClientCode, user);
                //添加日志
                TranLog tl3 = new TranLog();
                tl3.TranType = "650";
                tl3.Description = "收货GRN完成验证SO";
                tl3.TranDate = DateTime.Now;
                tl3.TranUser = user;
                tl3.WhCode = Whcode;
                tl3.SoNumber = item.SoNumber;
                tl3.ReceiptId = receiptid;
                idal.ITranLogDAL.Add(tl3);
                res = res + sores;
            }

            //添加日志
            TranLog tl = new TranLog();
            tl.TranType = "650";
            tl.Description = "收货GRN完成执行";
            tl.TranDate = DateTime.Now;
            tl.TranUser = user;
            tl.WhCode = Whcode;
            tl.ReceiptId = receiptid;

            if (res.Length > 100)
            {
                tl.Remark = res.Substring(0, 99);
            }
            else
            {
                tl.Remark = res;
            }
            idal.ITranLogDAL.Add(tl);

            idal.SaveChanges();
            return res;
        }

        public string UpdateGrnDetail(int detailId, DateTime? GRN_ReceiptDate, int GRN_Qty, double? GRN_Cbm, double? GRN_Kgs)
        {
            DamcoGRNDetail dg = new DamcoGRNDetail();
            dg.Id = detailId;
            if (GRN_ReceiptDate != null)
                dg.GRN_ReceiptDate = GRN_ReceiptDate;
            if (GRN_Qty != 0)
                dg.GRN_Qty = GRN_Qty;
            if (GRN_Cbm != null)
                dg.GRN_Cbm = decimal.Parse(GRN_Cbm.ToString());
            if (GRN_Kgs != null)
                dg.GRN_Kgs = GRN_Kgs;
            idal.IDamcoGRNDetailDAL.UpdateBy(dg, u => u.Id == detailId, new string[] { "GRN_ReceiptDate", "GRN_Qty", "GRN_Cbm", "GRN_Kgs" });
            idal.SaveChanges();
            return "Y";
        }

        public String UpdateGrnHeadRemark(int HeadId, string remark)
        {
            DamcoGRNHead dh = new DamcoGRNHead();
            dh.Id = HeadId;
            dh.Remark = remark;
            idal.IDamcoGRNHeadDAL.UpdateBy(dh, u => u.Id == dh.Id, new string[] { "Remark" });
            idal.SaveChanges();
            return "Y";
        }


    }
}

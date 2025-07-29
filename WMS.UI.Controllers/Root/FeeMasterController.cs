using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace WMS.UI.Controllers
{
    public class FeeMasterController : Controller
    {
        WCF.InBoundService.InBoundServiceClient cf = new WCF.InBoundService.InBoundServiceClient();
        WCF.RootService.RootServiceClient cf1 = new WCF.RootService.RootServiceClient();

        Helper Helper = new Helper();
        EIP.EIP EIP = new EIP.EIP();
        [DefaultRequest]
        public ActionResult Index()
        {
            ViewData["WhClientList"] = from r in cf.WhClientListSelect(Session["whCode"].ToString())
                                       select new SelectListItem()
                                       {
                                           Text = r.ClientCode,     //text
                                           Value = r.Id.ToString()
                                       };

            ViewData["zoneLocationList"] = from r in cf.RecZoneSelect(Session["whCode"].ToString(), 0)
                                           select new SelectListItem()
                                           {
                                               Text = r.ZoneName,     //text
                                               Value = r.ZoneName.ToString()
                                           };

            return View();
        }


        [DefaultRequest]
        public ActionResult IndexDK()
        {
            ViewData["WhClientList"] = from r in cf.WhClientListSelect(Session["whCode"].ToString())
                                       select new SelectListItem()
                                       {
                                           Text = r.ClientCode,     //text
                                           Value = r.Id.ToString()
                                       };

            ViewData["zoneLocationList"] = from r in cf.RecZoneSelect(Session["whCode"].ToString(), 0)
                                           select new SelectListItem()
                                           {
                                               Text = r.ZoneName,     //text
                                               Value = r.ZoneName.ToString()
                                           };

            return View();
        }

        //通过选择的客户得到收货区域
        [HttpPost]
        public ActionResult GetRecZoneNameList()
        {
            IEnumerable<WCF.InBoundService.WhZoneResult> list = cf.RecZoneSelect(Session["whCode"].ToString(), Convert.ToInt32(Request["txt_WhClient"]));
            if (list != null)
            {
                var sql = from r in list
                          select new
                          {
                              Value = r.ZoneName.ToString(),
                              Text = r.ZoneName    //text
                          };
                return Json(sql);
            }
            else
            {
                return null;
            }
        }

        //得到收货区域
        [HttpPost]
        public ActionResult GetRecZoneNameList1()
        {
            IEnumerable<WCF.InBoundService.WhZoneResult> list = cf.RecZoneSelect(Session["whCode"].ToString(), 0);
            if (list != null)
            {
                var sql = from r in list
                          select new
                          {
                              Value = r.ZoneName.ToString(),
                              Text = r.ZoneName    //text
                          };
                return Json(sql);
            }
            else
            {
                return null;
            }
        }

        //查询
        public ActionResult List()
        {
            WCF.RootService.FeeMasterSearch entity = new WCF.RootService.FeeMasterSearch();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.WhCode = Session["whCode"].ToString();
            entity.FeeNumber = Request["FeeNumber"].Trim();
            entity.Type = Request["type"].Trim();
            entity.Status = Request["status"].Trim();
            entity.ClientCode = Request["clientCode"].Trim();

            if (Request["createUser"].Trim() != "")
            {
                entity.CreateUser = Session["userName"].ToString();
            }

            if (Request["BeginCreateDate"] != "")
            {
                entity.BeginCreateDate = Convert.ToDateTime(Request["BeginCreateDate"]);
            }
            else
            {
                entity.BeginCreateDate = null;
            }

            if (Request["EndCreateDate"] != "")
            {
                entity.EndCreateDate = Convert.ToDateTime(Request["EndCreateDate"]).AddDays(1);
            }
            else
            {
                entity.EndCreateDate = null;
            }

            string soNumber = Request["soNumber"];
            string[] soNumberList = null;
            if (!string.IsNullOrEmpty(soNumber))
            {
                string po_temp = soNumber.Replace("\r\n", "@");    //把so中的空格 替换为@符号
                soNumberList = po_temp.Split('@');           //把SO 按照@分割，放在数组
            }

            int total = 0;
            string str = "";
            List<WCF.RootService.FeeMaster> list = cf1.FeeMaseterList(entity, soNumberList, out total, out str).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Id", "操作");
            fieldsName.Add("FeeNumber", "系统编号");
            fieldsName.Add("Type", "类型");
            fieldsName.Add("ClientCode", "客户名");
            fieldsName.Add("SoNumber", "SO号");
            fieldsName.Add("Status", "状态");

            fieldsName.Add("LoadId", "Load号");
            fieldsName.Add("LocationId", "收货区域");

            fieldsName.Add("LuruFee", "录入总价");
            fieldsName.Add("Description", "备注");

            fieldsName.Add("ReceiptId", "收货批次号");
            fieldsName.Add("OperationBeginDate", "开始操作时间");
            fieldsName.Add("OperationEndDate", "结束操作时间");

            fieldsName.Add("CreateUser", "创建人");
            fieldsName.Add("CreateDate", "创建时间");

            //fieldsName.Add("YuShouFee", "预收款");
            //fieldsName.Add("YuShouUser", "预收人");
            //fieldsName.Add("JieSuanFee", "结算款");
            //fieldsName.Add("JieSuanUser", "结算人");
            //fieldsName.Add("BillingType", "开票类型");

            //fieldsName.Add("InvoiceType", "发票机号");
            //fieldsName.Add("NoNumber", "发票代码");
            //fieldsName.Add("InvoiceNumber", "发票号码");
            //fieldsName.Add("InvoiceTopContent", "开票抬头");

            return Content(EIP.EipListJson(list, total, fieldsName, "Id:110,FeeNumber:140,ClientCode:110,Status:65,CreateDate:120,CreateUser:50,LocationId:70,SoNumber:140,ReceiptId:110,LoadId:140,Description:200,Type:65,OperationBeginDate:135,OperationEndDate:135,default:80"));
        }

        //新增TCR正常费用
        public ActionResult AddFeeMaster()
        {
            WCF.RootService.FeeMaster entity = new WCF.RootService.FeeMaster();
            entity.WhCode = Session["whCode"].ToString();
            entity.ClientCode = Request["clientCode"].Trim();
            entity.SoNumber = Request["txt_SoNumber"].Trim();
            entity.LoadId = Request["txt_Load"].Trim();
            entity.LocationId = Request["txt_recZone"].Trim();
            entity.Description = Request["txt_Description"].Trim();

            entity.CreateUser = Session["userName"].ToString();

            WCF.RootService.FeeMaster result = cf1.FeeMaseterAdd(entity);
            if (result != null)
            {
                return Helper.RedirectAjax("ok", "添加成功！", result, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "对不起，添加失败！", null, "");
            }
        }

        //客服添加现场操作特殊费用
        public void AddFeeMasterSpecial()
        {
            WCF.RootService.FeeMaster entity = new WCF.RootService.FeeMaster();
            entity.WhCode = Session["whCode"].ToString();
            entity.ClientCode = Request["clientCode"].Trim();
            entity.ReceiptId = Request["txt_ReceiptId1"].Trim();

            entity.LocationId = Request["txt_recZone1"].Trim();
            entity.Description = Request["txt_Description1"].Trim();

            try
            {
                entity.LuruFee = Convert.ToDecimal(Request["txt_LuruFee1"].Trim());
            }
            catch
            {
                Response.Write("特殊操作费用必须为数值类型！");
                return;
            }

            entity.CreateUser = Session["userName"].ToString();


            string result = cf1.FeeMaseterSpecialAdd(entity, 0);
            Response.Write(result);
        }


        //客服添加出货提箱特殊费用
        public void AddFeeMasterOutLoad()
        {
            WCF.RootService.FeeMaster entity = new WCF.RootService.FeeMaster();
            entity.WhCode = Session["whCode"].ToString();
            entity.ClientCode = Request["clientCode"].Trim();
            entity.LoadId = Request["txt_Load_load"].Trim();

            entity.LocationId = Request["txt_recZone_load"].Trim();
            entity.Description = Request["txt_Description_load"].Trim();

            try
            {
                entity.LuruFee = Convert.ToDecimal(Request["txt_LuruFee_load"].Trim());
            }
            catch
            {
                Response.Write("出货提货费用必须为数值类型！");
                return;
            }

            entity.CreateUser = Session["userName"].ToString();


            string result = cf1.FeeMasterOutLoadAdd(entity);
            Response.Write(result);
        }



        //道口添加现场操作特殊费用，只能夜班时间使用
        public void AddFeeMasterSpecialRec()
        {

            WCF.RootService.FeeMaster entity = new WCF.RootService.FeeMaster();
            entity.WhCode = Session["whCode"].ToString();
            entity.ClientCode = Request["clientCode"].Trim();
            entity.ReceiptId = Request["txt_ReceiptId1"].Trim();

            entity.LocationId = Request["txt_recZone1"].Trim();
            entity.Description = Request["txt_Description1"].Trim();

            try
            {
                entity.LuruFee = Convert.ToDecimal(Request["txt_LuruFee1"].Trim());
            }
            catch
            {
                Response.Write("特殊操作费用必须为数值类型！");
                return;
            }

            entity.CreateUser = Session["userName"].ToString();

            string result = cf1.FeeMaseterSpecialAdd(entity, 0);
            Response.Write(result);

        }


        //删除费用
        public ActionResult DelFeeMaster()
        {
            string result = cf1.FeeMaseterDel(Request["feeNumber"], Session["whCode"].ToString(), Session["userName"].ToString());
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "删除成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }
        }

        //确认订单完成
        public ActionResult EditFeeMasterStatus()
        {
            WCF.RootService.FeeMaster entity = new WCF.RootService.FeeMaster();
            entity.Id = Convert.ToInt32(Request["id"]);

            string result = cf1.FeeMaseterEditStatus(entity);
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "确认成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }
        }

        //撤销确认订单
        public ActionResult EditFeeMasterStatus1()
        {
            WCF.RootService.FeeMaster entity = new WCF.RootService.FeeMaster();
            entity.Id = Convert.ToInt32(Request["id"]);

            string result = cf1.FeeMaseterEditStatus1(entity);
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "撤销成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }
        }

        //订单确认作废
        public ActionResult EditFeeMasterStatus2()
        {
            WCF.RootService.FeeMaster entity = new WCF.RootService.FeeMaster();
            entity.Id = Convert.ToInt32(Request["id"]);
            entity.Description = Request["edit_description_status"];
            entity.CreateUser = Session["userName"].ToString();

            string result = cf1.FeeMaseterEditStatus2(entity);
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "订单作废成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }
        }

        //修改费用
        public ActionResult EditFeeMaster()
        {
            WCF.RootService.FeeMaster entity = new WCF.RootService.FeeMaster();
            entity.Id = Convert.ToInt32(Request["id"].Trim());

            entity.SoNumber = Request["edit_soNumber"].Trim();
            entity.Description = Request["edit_description"].Trim();

            entity.UpdateUser = Session["userName"].ToString();
            entity.UpdateDate = DateTime.Now;

            string result = cf1.FeeMaseterEdit(entity);
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "修改成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }
        }

        //修改费用
        public ActionResult EditFeeMasterRemark()
        {
            WCF.RootService.FeeMaster entity = new WCF.RootService.FeeMaster();
            entity.Id = Convert.ToInt32(Request["id"].Trim());

            entity.Description = Request["edit_remark"].Trim();
            try
            {
                entity.LuruFee = Convert.ToDecimal(Request["edit_luruFee"].Trim());
            }
            catch (Exception)
            {
                return Helper.RedirectAjax("err", "总价请填写数值！", null, "");
            }
           

            entity.UpdateUser = Session["userName"].ToString();
            entity.UpdateDate = DateTime.Now;

            string result = cf1.FeeMasterRemarkEdit(entity);
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "修改成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }
        }

        //修改费用明细
        public ActionResult EditFeeDetail()
        {
            WCF.RootService.FeeDetail entity = new WCF.RootService.FeeDetail();
            entity.Id = Convert.ToInt32(Request["id"].Trim());

            entity.SoNumber = Request["Editdetail_soNumber"].Trim();
            entity.CustomerPoNumber = Request["Editdetail_poNumber"].Trim();
            entity.AltItemNumber = Request["Editdetail_altNumber"].Trim();

            entity.HuId = Request["Editdetail_huid"].Trim();
            entity.LocationId = Request["Editdetail_locationId"].Trim();
            entity.Description = Request["Editdetail_description"].Trim();

            entity.ChangDiHours = Convert.ToInt32(Request["Editdetail_ChangdiHours"].Trim());

            if (Request["Editdetail_changDiFee"].Trim() == "" || Request["Editdetail_changDiFee"].Trim() == null)
            {
                entity.ChangDiFee = 0;
            }
            else
            {
                entity.ChangDiFee = Convert.ToDecimal(Request["Editdetail_changDiFee"].Trim());
            }
            if (Request["Editdetail_peopleFee"].Trim() == "" || Request["Editdetail_peopleFee"].Trim() == null)
            {
                entity.PeopleFee = 0;
            }
            else
            {
                entity.PeopleFee = Convert.ToDecimal(Request["Editdetail_peopleFee"].Trim());
            }
            if (Request["Editdetail_truckFee"].Trim() == "" || Request["Editdetail_truckFee"].Trim() == null)
            {
                entity.TruckFee = 0;
            }
            else
            {
                entity.TruckFee = Convert.ToDecimal(Request["Editdetail_truckFee"].Trim());
            }
            if (Request["Editdetail_daDanFee"].Trim() == "" || Request["Editdetail_daDanFee"].Trim() == null)
            {
                entity.DaDanFee = 0;
            }
            else
            {
                entity.DaDanFee = Convert.ToDecimal(Request["Editdetail_daDanFee"].Trim());
            }
            if (Request["Editdetail_otherFee"].Trim() == "" || Request["Editdetail_otherFee"].Trim() == null)
            {
                entity.OtherFee = 0;
            }
            else
            {
                entity.OtherFee = Convert.ToDecimal(Request["Editdetail_otherFee"].Trim());
            }

            try
            {
                entity.Price = Convert.ToDecimal(Request["Editdetail_price"].Trim());
                entity.Qty = Convert.ToInt32(Request["Editdetail_qty"].Trim());
                entity.TotalPrice = entity.Price * entity.Qty;
            }
            catch
            {
                return Helper.RedirectAjax("err", "单价和数量必须为数值类型！", null, "");
            }

            entity.UpdateUser = Session["userName"].ToString();

            string result = cf1.FeeDetailEdit(entity);
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "修改成功！", result, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }
        }

        //删除费用明细
        public ActionResult FeeDetailDel()
        {
            string result = cf1.FeeDetailDel(Convert.ToInt32(Request["id"]), Session["userName"].ToString());
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "删除成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }
        }

        //显示费用明细
        public ActionResult FeeDetailList()
        {
            WCF.RootService.FeeDetailSearch entity = new WCF.RootService.FeeDetailSearch();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.WhCode = Session["whCode"].ToString();
            entity.FeeNumber = Request["feeNumber"];
            entity.SoNumber = Request["searchDetail_soNumber"];
            entity.PoNumber = Request["searchDetail_poNumber"];
            entity.HuId = Request["searchDetail_huId"];
            entity.LocationId = Request["searchDetail_locationId"];

            int total = 0;
            string str = "";
            List<WCF.RootService.FeeDetail> list = cf1.FeeDetailList(entity, out total, out str).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Id", "操作");
            fieldsName.Add("FeeNumber", "系统编号");
            fieldsName.Add("SoNumber", "SO号");
            fieldsName.Add("CustomerPoNumber", "PO号");
            fieldsName.Add("AltItemNumber", "款号");
            fieldsName.Add("HuId", "托盘");
            fieldsName.Add("LocationId", "库位");
            fieldsName.Add("TCRProcessMode", "TCR处理方式");
            fieldsName.Add("Price", "单价");
            fieldsName.Add("Qty", "数量");
            fieldsName.Add("TotalPrice", "总价");
            fieldsName.Add("ChangDiFee", "场地费");
            fieldsName.Add("ChangDiHours", "场地预估小时");
            fieldsName.Add("PeopleFee", "人员监管费");
            fieldsName.Add("TruckFee", "车辆管理费");
            fieldsName.Add("DaDanFee", "打单费");
            fieldsName.Add("OtherFee", "其他费");
            fieldsName.Add("Description", "备注");
            fieldsName.Add("CreateUser", "创建人");
            fieldsName.Add("CreateDate", "创建时间");
            fieldsName.Add("UpdateUser", "修改人");
            fieldsName.Add("UpdateDate", "修改时间");

            return Content(EIP.EipListJson(list, total, fieldsName, "Id:60,SoNumber:120,TCRProcessMode:100,Price:60,Qty:60,TotalPrice:60,Description:120,CreateDate:120,UpdateDate:120,default:80", null, "", 200, str));
        }

        //新增费用明细
        public ActionResult AddFeeDetail()
        {
            WCF.RootService.FeeDetail entity = new WCF.RootService.FeeDetail();
            entity.WhCode = Session["whCode"].ToString();
            entity.FeeNumber = Request["feeNumber"].Trim();

            entity.SoNumber = Request["detail_soNumber"].Trim();
            entity.CustomerPoNumber = Request["detail_poNumber"].Trim();
            entity.AltItemNumber = Request["detail_altNumber"].Trim();

            entity.TCRProcessMode = Request["detail_TCRProcessMode"].Trim();
            entity.HuId = Request["detail_huid"].Trim();
            entity.LocationId = Request["detail_locationId"].Trim();
            entity.Description = Request["detail_description"].Trim();

            entity.ChangDiHours = Convert.ToInt32(Request["sel_ChangdiHours"].Trim());

            if (Request["detail_changDiFee"].Trim() == "" || Request["detail_changDiFee"].Trim() == null)
            {
                entity.ChangDiFee = 0;
            }
            else
            {
                entity.ChangDiFee = Convert.ToDecimal(Request["detail_changDiFee"].Trim());
            }
            if (Request["detail_peopleFee"].Trim() == "" || Request["detail_peopleFee"].Trim() == null)
            {
                entity.PeopleFee = 0;
            }
            else
            {
                entity.PeopleFee = Convert.ToDecimal(Request["detail_peopleFee"].Trim());
            }
            if (Request["detail_truckFee"].Trim() == "" || Request["detail_truckFee"].Trim() == null)
            {
                entity.TruckFee = 0;
            }
            else
            {
                entity.TruckFee = Convert.ToDecimal(Request["detail_truckFee"].Trim());
            }
            if (Request["detail_daDanFee"].Trim() == "" || Request["detail_daDanFee"].Trim() == null)
            {
                entity.DaDanFee = 0;
            }
            else
            {
                entity.DaDanFee = Convert.ToDecimal(Request["detail_daDanFee"].Trim());
            }
            if (Request["detail_otherFee"].Trim() == "" || Request["detail_otherFee"].Trim() == null)
            {
                entity.OtherFee = 0;
            }
            else
            {
                entity.OtherFee = Convert.ToDecimal(Request["detail_otherFee"].Trim());
            }

            try
            {
                entity.Price = Convert.ToDecimal(Request["detail_price"].Trim());
                entity.Qty = Convert.ToInt32(Request["detail_qty"].Trim());
                entity.TotalPrice = entity.Price * entity.Qty;
            }
            catch
            {
                return Helper.RedirectAjax("err", "单价和数量必须为数值类型！", null, "");
            }


            entity.CreateUser = Session["userName"].ToString();

            string result = cf1.FeeDetailAdd(entity);
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "添加成功！", result, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }
        }

        [HttpGet]
        public ActionResult SelectHuDetailList()
        {
            WCF.RootService.FeeDetailSearch entity = new WCF.RootService.FeeDetailSearch();
            entity.WhCode = Session["whCode"].ToString();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);

            entity.WhCode = Session["whCode"].ToString();
            entity.ClientCode = Request["txt_clientCode"].Trim();
            entity.ReceiptId = Request["byposku_receiptId"].Trim();


            string so_area = Request["so_area"];
            string[] soNumberList = null;
            if (!string.IsNullOrEmpty(so_area))
            {
                string so_temp = so_area.Replace("\r\n", "@");    //把so中的空格 替换为@符号
                soNumberList = so_temp.Split('@');           //把SO 按照@分割，放在数组
            }

            string po_area = Request["po_area"];
            string[] poNumberList = null;
            if (!string.IsNullOrEmpty(po_area))
            {
                string so_temp = po_area.Replace("\r\n", "@");    //把so中的空格 替换为@符号
                poNumberList = so_temp.Split('@');           //把SO 按照@分割，放在数组
            }

            string sku_area = Request["sku_area"];
            string[] skuNumberList = null;
            if (!string.IsNullOrEmpty(sku_area))
            {
                string so_temp = sku_area.Replace("\r\n", "@");    //把so中的空格 替换为@符号
                skuNumberList = so_temp.Split('@');           //把SO 按照@分割，放在数组
            }

            string hu_area = Request["huid_area"];
            string[] huList = null;
            if (!string.IsNullOrEmpty(hu_area))
            {
                string so_temp = hu_area.Replace("\r\n", "@");    //把so中的空格 替换为@符号
                huList = so_temp.Split('@');           //把SO 按照@分割，放在数组
            }

            int total = 0;
            string str = "";
            List<WCF.RootService.FeeDetailHuDetailListResult> list = cf1.HuDetailListByPOSKU(entity, soNumberList, poNumberList, skuNumberList, huList, out total, out str).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Action", "操作");
            fieldsName.Add("ClientCode", "客户名");
            fieldsName.Add("ReceiptId", "收货批次号");
            fieldsName.Add("HuId", "托盘");
            fieldsName.Add("LocationId", "库位");
            fieldsName.Add("SoNumber", "SO号");
            fieldsName.Add("CustomerPoNumber", "PO");
            fieldsName.Add("AltItemNumber", "款号");
            fieldsName.Add("Qty", "库存数量");
            fieldsName.Add("HoldReason", "锁定原因");

            return Content(EIP.EipListJson(list, total, fieldsName, "ClientCode:110,SoNumber:130,CustomerPoNumber:130,AltItemNumber:130,ReceiptId:130,default:70", null, "", 50, str));
        }

        [HttpPost]
        public ActionResult AddFeeDetailList()
        {
            string feeNumber = Request["feeNumber"];
            string[] soNumber = Request.Form.GetValues("soNumber");
            string[] poNumber = Request.Form.GetValues("poNumber");
            string[] altNumber = Request.Form.GetValues("altNumber");
            string[] qty = Request.Form.GetValues("qty");
            string[] huid = Request.Form.GetValues("huid");
            string[] locationId = Request.Form.GetValues("locationId");

            decimal price = Convert.ToDecimal(Request["price"]);
            string TCRProcessMode = Request["TCRProcessMode"];

            string checkResult = "";
            List<WCF.RootService.FeeDetail> list = new List<WCF.RootService.FeeDetail>();
            for (int i = 0; i < soNumber.Length; i++)
            {
                WCF.RootService.FeeDetail entity = new WCF.RootService.FeeDetail();
                entity.WhCode = Session["whCode"].ToString();
                entity.FeeNumber = feeNumber;

                entity.SoNumber = soNumber[i];
                entity.CustomerPoNumber = poNumber[i];
                entity.AltItemNumber = altNumber[i];
                entity.Qty = Convert.ToInt32(qty[i]);
                entity.HuId = huid[i];
                entity.LocationId = locationId[i];

                entity.TCRProcessMode = TCRProcessMode;
                entity.Description = "";

                entity.ChangDiFee = 0;
                entity.PeopleFee = 0;
                entity.TruckFee = 0;
                entity.DaDanFee = 0;
                entity.OtherFee = 0;
                entity.CreateUser = Session["userName"].ToString();
                try
                {
                    entity.Price = price;
                    entity.TotalPrice = entity.Price * entity.Qty;
                }
                catch
                {
                    checkResult = "单价数值异常！";
                    break;
                }
                list.Add(entity);
            }
            if (checkResult != "")
            {
                return Helper.RedirectAjax("err", checkResult, null, "");
            }

            string result = cf1.FeeDetailAddList(list.ToArray());
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "保存成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }
        }

        [HttpPost]
        public ActionResult onkeyGetHuDetail()
        {
            WCF.RootService.FeeDetailHuDetailListResult entity = cf1.GetHuDetailByHuId(Request["detail_huid"], Session["whCode"].ToString(), Request["hid_clientCode"]);

            if (entity != null)
            {
                return Helper.RedirectAjax("ok", "检索成功！", entity, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "该托盘的库存信息存在多行或未找到！", null, "");
            }
        }

    }
}

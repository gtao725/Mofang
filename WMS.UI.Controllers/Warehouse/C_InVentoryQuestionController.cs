using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace WMS.UI.Controllers
{
    public class C_InVentoryQuestionController : Controller
    {
        WCF.InBoundService.InBoundServiceClient inboundcf = new WCF.InBoundService.InBoundServiceClient();
        WCF.RecService.RecServiceClient recService = new WCF.RecService.RecServiceClient();
        WCF.InVentoryService.InVentoryServiceClient cf = new WCF.InVentoryService.InVentoryServiceClient();
        Helper Helper = new Helper();
        EIP.EIP EIP = new EIP.EIP();

        [DefaultRequest]
        public ActionResult Index()
        {
            ViewData["WhClientList"] = from r in inboundcf.WhClientListSelect(Session["whCode"].ToString())
                                       select new SelectListItem()
                                       {
                                           Text = r.ClientCode,     //text
                                           Value = r.Id.ToString()
                                       };
            return View();
        }

        [DefaultRequest]
        public ActionResult Index1()
        {
            ViewData["WhClientList"] = from r in inboundcf.WhClientListSelect(Session["whCode"].ToString())
                                       select new SelectListItem()
                                       {
                                           Text = r.ClientCode,     //text
                                           Value = r.Id.ToString()
                                       };
            return View();
        }

        [HttpGet]
        //库存问题信息查询
        public ActionResult List()
        {
            WCF.InVentoryService.InVentorySearch entity = new WCF.InVentoryService.InVentorySearch();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.WhCode = Session["whCode"].ToString();
            entity.ReceiptId = Request["receipt_id"].Trim();

            entity.SoNumber = Request["so_like"].Trim();
            entity.LotNumber1 = Request["lotNumber1"].Trim();
            entity.HoldReason = Request["holdReason"].Trim();

            entity.Type = Request["typeSelect"];

            string soNumber = Request["so_number"];
            string[] soNumberList = null;
            List<string> List1 = new List<string>();
            if (!string.IsNullOrEmpty(soNumber))
            {
                string temp = soNumber.Replace("\r\n", "@");    //把so中的空格 替换为@符号
                soNumberList = temp.Split('@');           //把SO 按照@分割，放在数组
                foreach (var item in soNumberList)
                {
                    if (!string.IsNullOrEmpty(item))
                    {
                        List1.Add(item);
                    }
                }
            }

            string poNumber = Request["customer_po"];
            string[] poNumberList = null;
            List<string> List2 = new List<string>();
            if (!string.IsNullOrEmpty(poNumber))
            {
                string po_temp = poNumber.Replace("\r\n", "@");    //把so中的空格 替换为@符号
                poNumberList = po_temp.Split('@');           //把SO 按照@分割，放在数组
                foreach (var item in poNumberList)
                {
                    if (!string.IsNullOrEmpty(item))
                    {
                        List2.Add(item);
                    }
                }
            }

            string altItemNumber = Request["altItemNumber"];
            string[] itemNumberList = null;
            List<string> List3 = new List<string>();
            if (!string.IsNullOrEmpty(altItemNumber))
            {
                string temp = altItemNumber.Replace("\r\n", "@");    //把so中的空格 替换为@符号
                itemNumberList = temp.Split('@');           //把SO 按照@分割，放在数组
                foreach (var item in itemNumberList)
                {
                    if (!string.IsNullOrEmpty(item))
                    {
                        List3.Add(item);
                    }
                }
            }

            string style1 = Request["style1"];
            string[] style1List = null;
            List<string> List4 = new List<string>();
            if (!string.IsNullOrEmpty(style1))
            {
                string temp = style1.Replace("\r\n", "@");    //把so中的空格 替换为@符号
                style1List = temp.Split('@');           //把SO 按照@分割，放在数组
                foreach (var item in style1List)
                {
                    if (!string.IsNullOrEmpty(item))
                    {
                        List4.Add(item);
                    }
                }
            }

            string HuId = Request["hu_id"];
            string[] HuIdList = null;
            List<string> List5 = new List<string>();
            if (!string.IsNullOrEmpty(HuId))
            {
                string po_temp = HuId.Replace("\r\n", "@");    //把so中的空格 替换为@符号
                HuIdList = po_temp.Split('@');           //把SO 按照@分割，放在数组
                foreach (var item in HuIdList)
                {
                    if (!string.IsNullOrEmpty(item))
                    {
                        List5.Add(item);
                    }
                }
            }

            if (Request["WhClientId"] != "")
            {
                entity.ClientId = Convert.ToInt32(Request["WhClientId"]);
            }
            else
            {
                entity.ClientId = 0;
            }
            if (Request["BeginReceiptDate"] != "")
            {
                entity.BeginReceiptDate = Convert.ToDateTime(Request["BeginReceiptDate"]);
            }
            else
            {
                entity.BeginReceiptDate = null;
            }
            if (Request["EndReceiptDate"] != "")
            {
                entity.EndReceiptDate = Convert.ToDateTime(Request["EndReceiptDate"]).AddDays(1);
            }
            else
            {
                entity.EndReceiptDate = null;
            }

            entity.LocationId = Request["locationId"].Trim();
            entity.LocationId1 = Request["locationId1"].Trim();
            entity.LocationTypeId = Convert.ToInt32(Request["locationTypeId"]);

            int total = 0;
            string str = "";
            List<WCF.InVentoryService.InVentoryResult> list = cf.C_InVentoryQuestionList(entity, List1.ToArray(), List2.ToArray(), List3.ToArray(), List4.ToArray(), List5.ToArray(), out total, out str).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("HuDetailId", "操作");
            fieldsName.Add("HuMasterId", "HuMasterId");
            fieldsName.Add("WhCode", "仓库");
            fieldsName.Add("HuId", "托盘");
            fieldsName.Add("TypeShow", "库存类型");
            fieldsName.Add("StatusShow", "货物状态");
            fieldsName.Add("LocationShow", "库位状态");
            fieldsName.Add("PickDetailShow", "释放状态");
            fieldsName.Add("Type", "类型");
            fieldsName.Add("Status", "状态");
            fieldsName.Add("Location", "库位");
            fieldsName.Add("AltItemNumber", "款号");
            fieldsName.Add("Qty", "库存数量");
            fieldsName.Add("PlanQty", "锁定数量");
            fieldsName.Add("HoldReason", "冻结原因");
            fieldsName.Add("ClientId", "客户ID");
            fieldsName.Add("ClientCode", "客户");
            fieldsName.Add("Doi", "库存天数");
            fieldsName.Add("ReceiptDate", "收货时间");
            fieldsName.Add("SoNumber", "SO号");
            fieldsName.Add("CustomerPoNumber", "PO号");
            fieldsName.Add("ItemId", "ItemId");
            fieldsName.Add("Style1", "属性1");
            fieldsName.Add("Style2", "属性2");
            fieldsName.Add("Style3", "属性3");
            fieldsName.Add("UnitName", "单位名");
            fieldsName.Add("UnitNameShow", "单位名称");
            fieldsName.Add("ReceiptId", "收货批次号");
            fieldsName.Add("Length", "长");
            fieldsName.Add("Width", "宽");
            fieldsName.Add("Height", "高");
            fieldsName.Add("Weight", "重量");
            fieldsName.Add("CBM", "立方");
            fieldsName.Add("LotNumber1", "Lot1");
            fieldsName.Add("LotNumber2", "Lot2");
            fieldsName.Add("LotDate", "lotDate");

            return Content(EIP.EipListJson(list, total, fieldsName, "HuDetailId:80,TypeShow:65,StatusShow:65,PickDetailShow:65,ReceiptId:130,Doi:65,ReceiptDate:125,Type:45,Qty:60,PlanQty:60,Length:60,Width:60,Height:60,Weight:60,LotNumber1:80,LotNumber2:80,CBM:60,Status:45,HuId:130,SoNumber:130,CustomerPoNumber:130,AltItemNumber:130,UnitName:60,LotDate:130,LocationShow:100,default:90", null, "", 50, str));
        }

        [HttpGet]
        //库存问题信息修改
        public ActionResult EditDetail()
        {
            int DetailId = Convert.ToInt32(Request["edit_DetailId"]);
            int MasterId = Convert.ToInt32(Request["edit_MasterId"]);

            WCF.InVentoryService.HuDetailResult entity = new WCF.InVentoryService.HuDetailResult();
            entity.Id = DetailId;
            entity.HuMasterId = MasterId;
            entity.Location = Request["edit_location"].Trim();
            entity.Qty = Convert.ToInt32(Request["edit_qty"]);
            entity.WhCode = Session["whCode"].ToString();
            if (Request["edit_length"] != "" && Request["edit_length"] != "null")
            {
                entity.Length = Convert.ToDecimal(Request["edit_length"]);
            }
            else
            {
                entity.Length = 0;
            }
            if (Request["edit_width"] != "" && Request["edit_width"] != "null")
            {
                entity.Width = Convert.ToDecimal(Request["edit_width"]);
            }
            else
            {
                entity.Width = 0;
            }
            if (Request["edit_height"] != "" && Request["edit_height"] != "null")
            {
                entity.Height = Convert.ToDecimal(Request["edit_height"]);
            }
            else
            {
                entity.Height = 0;
            }
            if (Request["edit_weight"] != "" && Request["edit_weight"] != "null")
            {
                entity.Weight = Convert.ToDecimal(Request["edit_weight"]);
            }
            else
            {
                entity.Weight = 0;
            }
            if (Request["edit_Lot1"] == "null")
            {
                entity.LotNumber1 = "";
            }
            else
            {
                entity.LotNumber1 = Request["edit_Lot1"].Trim();
            }

            if (Request["edit_Lot2"] == "null")
            {
                entity.LotNumber2 = "";
            }
            else
            {
                entity.LotNumber2 = Request["edit_Lot2"].Trim();
            }
            if (Request["edit_LotDate"] == "null" || string.IsNullOrEmpty(Request["edit_LotDate"]))
            {
                entity.LotDate = null;
            }
            else
            {
                try
                {
                    DateTime s = Convert.ToDateTime(Request["edit_LotDate"].Trim());
                    entity.LotDate = s;
                }
                catch (Exception)
                {
                    return Helper.RedirectAjax("err", "日期格式不正确！", null, "");
                }
            }

            entity.UserName = Session["userName"].ToString();
            string result = cf.HuMasterHuDetailEdit(entity);
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "信息修改成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }
        }


        [HttpGet]
        //托盘移库
        public ActionResult EditDetail1()
        {
            string result = cf.HuIdRemoveLocation(Session["whCode"].ToString(), Request["old_location"], Request["edit_location"].Trim(), Request["old_huId"], Session["userName"].ToString());
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "托盘移库成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }
        }

        [HttpGet]
        //查询客户异常原因
        public ActionResult HoldMasterListByRec()
        {
            WCF.RecService.HoldMasterSearch entity = new WCF.RecService.HoldMasterSearch();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.WhCode = Session["whCode"].ToString();
            entity.ClientId = Convert.ToInt32(Request["clientId"]);
            int total = 0;
            WCF.RecService.HoldMaster[] list = recService.HoldMasterListByRec(entity, out total);

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Id", "操作");
            fieldsName.Add("WhCode", "仓库");
            fieldsName.Add("ClientId", "客户ID");
            fieldsName.Add("ClientCode", "客户Code");
            fieldsName.Add("HoldReason", "异常原因");

            return Content(EIP.EipListJson(list.ToList(), total, fieldsName, "HoldReason:150,default:90"));
        }

        [HttpGet]
        //托盘冻结
        public ActionResult FrozenHuId()
        {
            string HuId = Request["HuId"].Trim();

            WCF.InVentoryService.HuMaster master = new WCF.InVentoryService.HuMaster();
            master.HuId = HuId;
            master.WhCode = Session["whCode"].ToString();
            master.Status = "H";
            master.HoldId = 0;
            master.HoldReason = Request["holdReason"].ToString();
            master.UpdateUser = Session["userName"].ToString();
            master.UpdateDate = DateTime.Now;

            master.HoldReason = master.HoldReason.Substring(0, master.HoldReason.Length - 1);

            // int result = cf.HuMasterEdit(master, new string[] { "Status", "HoldId", "HoldReason", "UpdateUser", "UpdateDate" });
            string result = cf.PltHoldEdit(master);

            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "托盘冻结成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "对不起，冻结失败！", null, "");
            }
        }

        [HttpGet]
        //解冻托盘
        public ActionResult RelieveHuId()
        {
            string HuId = Request["HuId"].Trim();

            WCF.InVentoryService.HuMaster master = new WCF.InVentoryService.HuMaster();
            master.HuId = HuId;
            master.WhCode = Session["whCode"].ToString();
            master.Status = "A";
            master.HoldId = 0;
            master.HoldReason = "";
            master.UpdateUser = Session["userName"].ToString();
            master.UpdateDate = DateTime.Now;

            // int result = cf.HuMasterEdit(master, new string[] { "Status", "HoldId", "HoldReason", "UpdateUser", "UpdateDate" });
            string result = cf.PltHoldEdit(master);

            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "托盘解冻成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "对不起，解冻失败！", null, "");
            }
        }

        [HttpPost]
        //批量解冻托盘
        public ActionResult BatchRelieveHuId()
        {
            string[] HuIdList = Request.Form.GetValues("HuId");
            string[] HuStatusList = Request.Form.GetValues("HuStatus");

            string strHuIds = "";
            for (int i = 0; i < HuIdList.Length; i++)
            {
                if (HuStatusList[i] == "H")
                {

                    WCF.InVentoryService.HuMaster master = new WCF.InVentoryService.HuMaster();
                    master.HuId = HuIdList[i];
                    master.WhCode = Session["whCode"].ToString();
                    master.Status = "A";
                    master.HoldId = 0;
                    master.HoldReason = "";
                    master.UpdateUser = Session["userName"].ToString();
                    master.UpdateDate = DateTime.Now;

                    // int result = cf.HuMasterEdit(master, new string[] { "Status", "HoldId", "HoldReason", "UpdateUser", "UpdateDate" });
                    string result = cf.PltHoldEdit(master);

                    if (result != "Y")
                    {
                        strHuIds += Environment.NewLine + HuIdList[i];
                    }
                    else
                    {

                    }
                }
            }
            if (strHuIds == "")
            {
                return Helper.RedirectAjax("ok", "修改成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "对不起，以下托盘解冻失败:" + strHuIds, null, "");
            }

        }



        [HttpPost]
        //异常库位批量上架
        public ActionResult BatchPutHuIdABLocation()
        {
            string[] ClientCodeList = Request.Form.GetValues("ClientCode");
            string[] ItemNumberList = Request.Form.GetValues("ItemNumber");
            string[] HuDetailIdList = Request.Form.GetValues("HuDetailId");
            string[] LotNumber1List = Request.Form.GetValues("LotNumber1");
            string[] LotNumber2List = Request.Form.GetValues("LotNumber2");
            string[] LotDateList = Request.Form.GetValues("LotDate");

            string BatchPutHuId_location = Request["BatchPutHuId_location"].Trim();

            string[] CheckClientCodeList = (from a in ClientCodeList select a).ToList().Distinct().ToArray();
            if (CheckClientCodeList.Count() > 1)
            {
                return Helper.RedirectAjax("err", "仅允许单客户批量上架，请选择客户查询后再次操作！", null, "");
            }

            string[] CheckItemNumberList = (from a in ItemNumberList select a).ToList().Distinct().ToArray();
            if (CheckItemNumberList.Count() > 1)
            {
                return Helper.RedirectAjax("err", "仅允许单款号批量上架，请输入款号查询后再次操作！", null, "");
            }


            string[] CheckLotNumber1List = (from a in LotNumber1List select a).ToList().Distinct().ToArray();
            if (CheckLotNumber1List.Count() > 1)
            {
                return Helper.RedirectAjax("err", "仅允许单款号单Lot1批量上架，请输入Lot1查询后再次操作！", null, "");
            }


            string[] CheckLotNumber2List = (from a in LotNumber2List select a).ToList().Distinct().ToArray();
            if (CheckLotNumber2List.Count() > 1)
            {
                return Helper.RedirectAjax("err", "仅允许单款号单Lot2批量上架，请输入Lot2查询后再次操作！", null, "");
            }

            string[] CheckLotDateList = (from a in LotDateList select a).ToList().Distinct().ToArray();
            if (CheckLotDateList.Count() > 1)
            {
                return Helper.RedirectAjax("err", "仅允许单款号单LotDate批量上架，请输入LotDate查询后再次操作！", null, "");
            }

            int[] idArr = Array.ConvertAll(HuDetailIdList, int.Parse);

            string strHuIds = cf.BatchPutHuIdABLocation(idArr, Session["whCode"].ToString(), BatchPutHuId_location, Session["userName"].ToString());

            if (strHuIds == "Y")
            {
                return Helper.RedirectAjax("ok", "批量上架成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "批量上架失败！", null, "");
            }
        }



        [HttpPost]
        //批量调整库存Lot
        public ActionResult BatchEditHuIdLot()
        {
            string[] HuIdList = Request.Form.GetValues("HuDetailId");
            string qty = Request["batchedit_qty"].Trim();
            string huId = Request["batchedit_huId"].Trim();

            string lot1 = Request["batchedit_lot1"].Trim();
            string lot2 = Request["batchedit_lot2"].Trim();
            string lotdate = Request["batchedit_lotdate"].Trim();
            string lotdate1 = "";
            int qty1 = 0;
            try
            {
                int s = Convert.ToInt32(qty);
                if (s <= 0)
                {
                    return Helper.RedirectAjax("err", "需调整数量请填写数字并大于0！", null, "");
                }

                qty1 = s;
            }
            catch
            {
                return Helper.RedirectAjax("err", "需调整数量请填写数字并大于0！", null, "");
            }

            try
            {
                if (!string.IsNullOrEmpty(lotdate))
                {
                    DateTime s = Convert.ToDateTime(lotdate);
                    lotdate1 = lotdate;
                }
            }
            catch
            {
                return Helper.RedirectAjax("err", "需调整数量请填写数字！", null, "");
            }

            int[] idArr = Array.ConvertAll(HuIdList, int.Parse);

            string result = cf.BatchEditHuIdLot(idArr, Session["whCode"].ToString(), huId, qty1, lot1, lot2, lotdate1, Session["userName"].ToString());

            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "调整成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }

        }

    }
}

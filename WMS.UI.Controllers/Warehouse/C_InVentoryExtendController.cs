using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace WMS.UI.Controllers
{
    public class C_InVentoryExtendController : Controller
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
            fieldsName.Add("HuId", "SN号");
            fieldsName.Add("TypeShow", "库存类型");
            fieldsName.Add("StatusShow", "库存状态");
            fieldsName.Add("LocationShow", "库位状态");
            fieldsName.Add("PickDetailShow", "释放状态");
            fieldsName.Add("Type", "类型");
            fieldsName.Add("Status", "状态");
            fieldsName.Add("Location", "库位");
            fieldsName.Add("Qty", "库存数量");
            fieldsName.Add("PlanQty", "锁定数量");
            fieldsName.Add("HoldReason", "冻结原因");
            fieldsName.Add("ClientId", "客户ID");
            fieldsName.Add("ClientCode", "客户");
            fieldsName.Add("Doi", "库存天数");
            fieldsName.Add("ReceiptDate", "收货时间");
            fieldsName.Add("SoNumber", "SO号");
            fieldsName.Add("CustomerPoNumber", "PO号");
            fieldsName.Add("AltItemNumber", "款号");
            fieldsName.Add("Style1", "属性1");
            fieldsName.Add("Style2", "属性2");
            fieldsName.Add("Style3", "属性3");
            fieldsName.Add("UnitName", "单位名");
            fieldsName.Add("ReceiptId", "收货批次号");
            fieldsName.Add("Length", "长");
            fieldsName.Add("Width", "宽");
            fieldsName.Add("Height", "高");
            fieldsName.Add("Weight", "重量");
            fieldsName.Add("CBM", "立方");
            fieldsName.Add("LotNumber1", "Lot1");
            fieldsName.Add("LotNumber2", "Lot2");
            fieldsName.Add("LotDate", "lotDate");

            return Content(EIP.EipListJson(list, total, fieldsName, "HuDetailId:80,TypeShow:65,StatusShow:65,PickDetailShow:65,ReceiptId:130,Doi:65,ReceiptDate:125,Type:45,Qty:60,PlanQty:60,Length:60,Width:60,Height:60,Weight:60,LotNumber1:80,LotNumber2:80,CBM:60,Status:45,HuId:100,SoNumber:130,CustomerPoNumber:130,AltItemNumber:130,UnitName:60,LotDate:130,default:90", null, "", 50, str));
        }

        [HttpGet]
        //库存问题信息修改
        public ActionResult EditDetail()
        {
            int DetailId = Convert.ToInt32(Request["edit_DetailId"]);
            int MasterId = Convert.ToInt32(Request["edit_MasterId"]);

            string result = cf.EditInventoryExtendHuId(DetailId, MasterId, Request["edit_sn"].Trim());
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "SN信息修改成功！", null, "");
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
            string strHuIds = "";
            for (int i = 0; i < HuIdList.Length; i++)
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
            if (strHuIds == "")
            {
                return Helper.RedirectAjax("ok", "修改成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "对不起，以下托盘解冻失败:" + strHuIds, null, "");
            }



        }


        [DefaultRequest]
        public ActionResult IndexCreateInvMove()
        {
            ViewBag.whCode = Session["whCode"].ToString();
            ViewBag.CrSaveAddress = ConfigurationManager.AppSettings["CrAddress98"].ToString();
            return View();
        }



        [HttpGet]
        //库存整理单查询
        public ActionResult ListInvMove()
        {
            WCF.InVentoryService.InvMoveDetailSearch entity = new WCF.InVentoryService.InvMoveDetailSearch();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.WhCode = Session["whCode"].ToString();


            //  entity.SoNumber = Request["so_like"].Trim();
            //  entity.CustomerPoNumber = Request["customer_po"].Trim();
            //  entity.AltItemNumber = Request["altItemNumber"].Trim();
            entity.MoveNum = Request["MoveNum"];
            entity.DetailFlag = Convert.ToInt32(Request["DetailFlag"]);
            //entity.CreateDate =  Request["CreateDate"];


            if (Request["BeginDate"] != "" && entity.DetailFlag == 0)
            {
                entity.BeginDate = Convert.ToDateTime(Request["BeginDate"]).AddDays(-1);
            }

            if (Request["EndDate"] != "" && entity.DetailFlag == 0)
            {
                entity.EndDate = Convert.ToDateTime(Request["EndDate"]);
            }


            int total = 0;

            List<WCF.InVentoryService.InvMoveDetailResult> list = cf.ListInvMove(entity, out total).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("MoveNumEdit", "操作");
            if (entity.DetailFlag == 0) {  
                fieldsName.Add("MoveNum", "整理单号");
            }

            else if (entity.DetailFlag == 1)
            {
                fieldsName.Add("ClientCode", "客户");
                fieldsName.Add("SoNumber", "SO");
                fieldsName.Add("AltItemNumber", "SKU");
                fieldsName.Add("HuId", "托盘号");
                fieldsName.Add("Location", "起始储位");
                fieldsName.Add("DesZoneName", "目标库位");
            }
            fieldsName.Add("CreateUser", "创建人");
            fieldsName.Add("CreateDate", "创建时间");


            return Content(EIP.EipListJson(list, total, fieldsName, "MoveNum:160,HuId:160,ClientCode:85,CreateDate:130,default:80", null, "", 50));
        }

        [HttpGet]
        //库存整理单查询
        public ActionResult InvMoveCreate()
        {


            string WhCode = Session["whCode"].ToString();

            string userName = Session["userName"].ToString();

            string res = cf.CreateInvMove(WhCode, userName);

            if (res.Split('$')[0] == "Y")
            {
                return Helper.RedirectAjax("ok", res.Split('$')[1], null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", res.Split('$')[1], null, "");
            }



           
        }

    }
}

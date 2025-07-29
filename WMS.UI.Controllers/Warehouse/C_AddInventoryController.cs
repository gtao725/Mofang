using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace WMS.UI.Controllers
{
    public class C_AddInventoryController : Controller
    {
        WCF.InVentoryService.InVentoryServiceClient cf = new WCF.InVentoryService.InVentoryServiceClient();

        WCF.InBoundService.InBoundServiceClient cfinbound = new WCF.InBoundService.InBoundServiceClient();

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
            return View();
        }

        //库存问题信息查询
        public ActionResult List()
        {
            WCF.InVentoryService.InVentorySearch entity = new WCF.InVentoryService.InVentorySearch();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.WhCode = Session["whCode"].ToString();
            entity.ReceiptId = Request["receipt_id"].Trim();
            entity.HuId = Request["hu_id"].Trim();
            entity.SoNumber = Request["so_number"].Trim();
            entity.CustomerPoNumber = Request["customer_po"].Trim();
            entity.HoldReason = Request["holdReason"].Trim();
            string[] soNumberList = null;
            string[] poNumberList = null;
            string[] itemNumberList = null;
            string[] huIdList = null;
            string[] styleList = null;

            if (Request["WhClientId"] != "")
            {
                entity.ClientId = Convert.ToInt32(Request["WhClientId"]);
            }
            else
            {
                entity.ClientId = 0;
            }
            entity.LocationId = Request["locationId"].Trim();
            entity.LocationTypeId = Convert.ToInt32(Request["locationTypeId"]);

            int total = 0;
            string str = "";
            List<WCF.InVentoryService.InVentoryResult> list = cf.C_InVentoryQuestionList(entity, soNumberList, poNumberList, itemNumberList, styleList, huIdList, out total, out str).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("HuDetailId", "操作");
            fieldsName.Add("HuMasterId", "HuMasterId");
            fieldsName.Add("WhCode", "仓库");
            fieldsName.Add("HuId", "托盘");
            fieldsName.Add("TypeShow", "库存类型");
            fieldsName.Add("StatusShow", "库存状态");
            fieldsName.Add("Type", "类型");
            fieldsName.Add("Status", "状态");
            fieldsName.Add("Location", "库位");
            fieldsName.Add("Qty", "库存数量");
            fieldsName.Add("PlanQty", "锁定数量");
            fieldsName.Add("HoldReason", "冻结原因");
            fieldsName.Add("ClientCode", "客户");
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

            return Content(EIP.EipListJson(list, total, fieldsName, "HuDetailId:60,TypeShow:70,StatusShow:70,ReceiptId:130,ReceiptDate:130,Type:45,Qty:60,PlanQty:60,Length:60,Width:60,Height:60,Weight:60,CBM:60,Status:45,HuId:110,default:90", null, "", 50, str));
        }


        [HttpPost]
        //新增库存
        public ActionResult AddInventory()
        {
            WCF.InVentoryService.HuDetailInsert entity = new WCF.InVentoryService.HuDetailInsert();
            entity.WhCode = Session["whCode"].ToString();
            entity.ReceiptId = Request["txt_ReceiptId"].Trim();
            entity.HuId = Request["txt_HuId"].Trim();
            if (Request["txt_ClientCode"] != "")
            {
                entity.ClientId = Convert.ToInt32(Request["txt_ClientCode"]);
            }
            else
            {
                entity.ClientId = 0;
            }
            entity.ClientCode = Request["clientCode"].Trim();
            entity.SoNumber = Request["txt_SoNumber"].Trim();
            entity.CustomerPoNumber = Request["txt_CustomerPoNumber"].Trim();
            entity.AltItemNumber = Request["txt_AltItemNumber"].Trim();
            entity.Qty = Convert.ToInt32(Request["txt_Qty"]);

            entity.UnitName = Request["unitName"].Trim();
            entity.UnitId = Convert.ToInt32(Request["sel_unitName"]);

            if (Request["txt_Length"] != "")
            {
                entity.Length = Convert.ToDecimal(Request["txt_Length"]);
            }
            else
            {
                entity.Length = 0;
            }
            if (Request["txt_Width"] != "")
            {
                entity.Width = Convert.ToDecimal(Request["txt_Width"]);
            }
            else
            {
                entity.Width = 0;
            }
            if (Request["txt_Height"] != "")
            {
                entity.Height = Convert.ToDecimal(Request["txt_Height"]);
            }
            else
            {
                entity.Height = 0;
            }
            if (Request["txt_Weight"] != "")
            {
                entity.Weight = Convert.ToDecimal(Request["txt_Weight"]);
            }
            else
            {
                entity.Weight = 0;
            }
            entity.LotNumber1 = Request["txt_LotNumber1"].Trim();
            entity.LotNumber2 = Request["txt_LotNumber2"].Trim();
            if (Request["txt_LotDate"] != "")
            {
                entity.LotDate = Convert.ToDateTime(Request["txt_LotDate"]);
            }
            else
            {
                entity.LotDate = null;
            }
            entity.Location = Request["txt_Location"].Trim();
            entity.UserName = Session["userName"].ToString();
            string result = cf.AddInventory(entity);
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "新增库存成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }

        }

        WCF.InBoundService.InBoundServiceClient inboundCF = new WCF.InBoundService.InBoundServiceClient();
        //通过输入的款号 属性 得到单位列表
        [HttpPost]
        public ActionResult GetUnitSelList()
        {
            WCF.InBoundService.UnitsSearch entity = new WCF.InBoundService.UnitsSearch();
            entity.WhCode = Session["whCode"].ToString();
            entity.ClientId = Convert.ToInt32(Request["txt_ClientCode"]);
            entity.AltItemNumber = Request["txt_AltItemNumber"].Trim();

            IEnumerable<WCF.InBoundService.UnitsResult> list = inboundCF.GetUnitSelList(entity);
            if (list != null)
            {
                var sql = from r in inboundCF.GetUnitSelList(entity)
                          select new
                          {
                              Value = r.Id.ToString(),
                              Text = r.UnitName    //text
                          };
                return Json(sql);
            }
            else
            {


                var sql = from r in cfinbound.UnitDefaultListSelect(Session["whCode"].ToString())
                          select new
                          {
                              Value = 0,
                              Text = r.UnitName    //text
                          };
                return Json(sql);
            }
        }

    }
}

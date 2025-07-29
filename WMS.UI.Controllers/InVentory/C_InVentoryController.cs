using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace WMS.UI.Controllers
{
    public class C_InVentoryController : Controller
    {
        WCF.InVentoryService.InVentoryServiceClient cf = new WCF.InVentoryService.InVentoryServiceClient();
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

        public ActionResult List()
        {
            WCF.InVentoryService.InVentorySearch entity = new WCF.InVentoryService.InVentorySearch();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.WhCode = Session["whCode"].ToString();
            entity.SoNumber = Request["so_number"];
            entity.CustomerPoNumber = Request["customer_po"];
            entity.AltItemNumber = Request["alt_item_number"];

            if (Request["WhClientId"] == "")
            {
                entity.ClientId = 0;
            }
            else
            {
                entity.ClientId = Convert.ToInt32(Request["WhClientId"]);
            }

            int total = 0;
            List<WCF.InVentoryService.InVentoryResult> list = cf.C_InVentoryList(entity, out total).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("WhCode", "仓库");
            fieldsName.Add("ClientCode", "客户");
            fieldsName.Add("ReceiptDate", "收货时间");
            fieldsName.Add("HuId", "托盘");
            fieldsName.Add("Location", "库位");
            fieldsName.Add("Type", "类型");
            fieldsName.Add("Status", "状态");
            fieldsName.Add("HoldReason", "冻结原因");
            fieldsName.Add("SoNumber", "SO号");
            fieldsName.Add("CustomerPoNumber", "PO号");
            fieldsName.Add("AltItemNumber", "款号");
            fieldsName.Add("Style1", "属性1");
            fieldsName.Add("Style2", "属性2");
            fieldsName.Add("Style3", "属性3");
            fieldsName.Add("Qty", "库存数量");
            fieldsName.Add("UnitName", "单位名");
            fieldsName.Add("CBM", "体积");
            fieldsName.Add("LotNumber1", "Lot1");
            fieldsName.Add("LotNumber2", "Lot2");
            fieldsName.Add("LotDate", "LotDate");

            return Content(EIP.EipListJson(list, total, fieldsName, "ReceiptDate:130,Qty:60,CBM:60,default:90"));
        }

    }
}

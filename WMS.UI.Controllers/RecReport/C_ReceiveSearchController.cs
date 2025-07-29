using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace WMS.UI.Controllers
{
    public class C_ReceiveSearchController : Controller
    {
        WCF.RecReportService.RecReportServiceClient cf = new WCF.RecReportService.RecReportServiceClient();
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
            WCF.RecReportService.ReceiptReportSearch entity = new WCF.RecReportService.ReceiptReportSearch();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.WhCode = Session["whCode"].ToString();
            entity.ReceiptId = Request["ReceiptId"];

            if (Request["WhClientId"] == "")
            {
                entity.ClientId = 0;
            }
            else
            {
                entity.ClientId = Convert.ToInt32(Request["WhClientId"]);
            }

            int total = 0;
            List<WCF.RecReportService.ReceiptReportResult> list = cf.C_ReceiveList(entity, out total).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("WhCode", "仓库");
            fieldsName.Add("ReceiptId", "收货批次号");
            fieldsName.Add("ClientCode", "客户");
            fieldsName.Add("ReceiptDate", "收货时间");
            fieldsName.Add("SoNumber", "SO号");
            fieldsName.Add("CustomerPoNumber", "PO号");
            fieldsName.Add("AltItemNumber", "款号");
            fieldsName.Add("Style1", "属性1");
            fieldsName.Add("HuId", "托盘");
            fieldsName.Add("Qty", "实收数量");
            fieldsName.Add("UnitName", "单位名");
            fieldsName.Add("Length", "长");
            fieldsName.Add("Width", "宽");
            fieldsName.Add("Height", "高");
            fieldsName.Add("Weight", "重量");
            fieldsName.Add("CBM", "体积");

            return Content(EIP.EipListJson(list, total, fieldsName, "ReceiptId:130,ReceiptDate:130,HuId:70,Qty:60,Length:60,Width:60,Height:60,Weight:60,CBM:60,default:90"));
        }

    }
}

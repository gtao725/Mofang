using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace WMS.UI.Controllers
{
    public class C_RecCompleteController : Controller
    {
        WCF.RecService.RecServiceClient cf = new WCF.RecService.RecServiceClient();
        Helper Helper = new Helper();
        EIP.EIP EIP = new EIP.EIP();

        [DefaultRequest]
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult List()
        {
            WCF.RecService.ReceiptSearch entity = new WCF.RecService.ReceiptSearch();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.WhCode = Session["whCode"].ToString();
            entity.ReceiptId = Request["receipt_id"].Trim();

            int total = 0;
            string str = "";
            List<WCF.RecService.ReceiptDetailCompleteResult> list = cf.C_RecCompleteList(entity, out total, out str).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();

            fieldsName.Add("Action", "操作");
            fieldsName.Add("ClientCode", "客户");
            fieldsName.Add("ReceiptId", "收货批次号");
            fieldsName.Add("LocationId", "库区");
            fieldsName.Add("Status", "状态");
            fieldsName.Add("SoNumber", "SO号");
            fieldsName.Add("PoNumber", "PO号");
            fieldsName.Add("AltItemNumber", "款号");

            fieldsName.Add("RecQty", "实收数量");
            fieldsName.Add("RegQty", "登记数量");

            return Content(EIP.EipListJson(list, total, fieldsName, "Action:40,LocationId:60,Status:70,RecQty:70,RegQty:70,default:130", null, "", 50, str));
        }


        public ActionResult EditDetail()
        {
            string result = cf.CheckRecComplete1(Request["ReceiptId"].Trim(), Session["whCode"].ToString(), Session["userName"].ToString());
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "强制收货完成！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }

        }
    }
}

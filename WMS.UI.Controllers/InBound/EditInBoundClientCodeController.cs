using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace WMS.UI.Controllers
{
    public class EditInBoundClientCodeController : Controller
    {
        WCF.InBoundService.InBoundServiceClient cf = new WCF.InBoundService.InBoundServiceClient();
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

        [HttpGet]
        public ActionResult List()
        {

            WCF.InBoundService.EditInBoundSearch entity = new WCF.InBoundService.EditInBoundSearch();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.WhCode = Session["whCode"].ToString();
            entity.ReceiptId = Request["receipt_id"];
            entity.SoNumber = Request["so_number"];

            int total = 0;
            List<WCF.InBoundService.EditInBoundResult> list = cf.EditInBoundClientCodeList(entity, out total).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("ReceiptId", "收货批次号");
            fieldsName.Add("ClientCode", "客户名");
            fieldsName.Add("SoNumber", "SO号");
            fieldsName.Add("RegQty", "登记数量");
            fieldsName.Add("RecQty", "实收数量");
            fieldsName.Add("Status", "状态");
            fieldsName.Add("RegisterDate", "登记时间");
            fieldsName.Add("ReceiptDate", "收货时间");

            return Content(EIP.EipListJson(list, total, fieldsName, "Action:60,Status:70,RegQty:70,RecQty:70,default:110"));
        }

        //修改收货客户名
        [HttpPost]
        public ActionResult EditDetailClientCode()
        {
            string[] soList = Request.Form.GetValues("soNumber");

            WCF.InBoundService.EditInBoundResult entity = new WCF.InBoundService.EditInBoundResult();
            entity.ClientCode = Request["clientCode"].Replace(@"""", "").Replace(@"'", "").Replace(@" ", "").Trim();
            entity.ClientId = Convert.ToInt32(Request["clientId"]);
            entity.ReceiptId = Request["receiptId"].Replace(@"""", "").Replace(@"'", "").Replace(@" ", "").Trim();
            entity.WhCode = Session["whCode"].ToString();
            entity.UserCode = Session["userName"].ToString();

            string result = cf.EditDetailClientCode(entity, soList.Distinct().ToArray());

            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "修改成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }
        }



    }
}

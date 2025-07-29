using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace WMS.UI.Controllers
{
    public class EditInBoundSoNumberController : Controller
    {
        WCF.InBoundService.InBoundServiceClient cf = new WCF.InBoundService.InBoundServiceClient();
        Helper Helper = new Helper();
        EIP.EIP EIP = new EIP.EIP();

        [DefaultRequest]
        public ActionResult Index()
        {
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

        //修改SO
        [HttpPost]
        public ActionResult EditDetailSoNumber()
        {
            string[] soList = Request.Form.GetValues("soNumber");

            WCF.InBoundService.EditInBoundResult entity = new WCF.InBoundService.EditInBoundResult();
            entity.SoNumber = Request["edit_SoNumber"].Replace(@"""", "").Replace(@"'", "").Replace(@" ", "").Trim();
            entity.ReceiptId = Request["receiptId"].Replace(@"""", "").Replace(@"'", "").Replace(@" ", "").Trim();
            entity.WhCode = Session["whCode"].ToString();
            entity.UserCode = Session["userName"].ToString();

            string result = cf.EditDetailSoNumber(entity, soList.Distinct().ToArray());

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

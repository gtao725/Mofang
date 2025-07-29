using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace WMS.UI.Controllers
{
    public class EditInBoundAltItemByHuController : Controller
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
            entity.HuId = Request["hu_id"];

            int total = 0;
            List<WCF.InBoundService.ReceiptResult> list = cf.EditInBoundAltItemByHuList(entity, out total).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("ReceiptId", "收货批次号");
            fieldsName.Add("ClientCode", "客户名");
            fieldsName.Add("HuId", "托盘号");
            fieldsName.Add("SoNumber", "SO号");
            fieldsName.Add("CustomerPoNumber", "PO号");
            fieldsName.Add("AltItemNumber", "款号");
            fieldsName.Add("Qty", "实收数量");
            fieldsName.Add("Status", "状态");
            fieldsName.Add("UnitName", "单位名");
            fieldsName.Add("UnitNameShow", "单位");

            return Content(EIP.EipListJson(list, total, fieldsName, "WhCode:40,ReceiptId:120,ClientCode:100,Qty:60,UnitName:60,UnitNameShow:60,HuId:100,Status:100,default:130"));
        }

        //修改SO
        [HttpPost]
        public ActionResult EditDetailAltItemNumber()
        {
            string[] soList = Request.Form.GetValues("soNumber");
            string[] poList = Request.Form.GetValues("poNumber");
            string[] itemList = Request.Form.GetValues("itemNumber");
            string[] huList = Request.Form.GetValues("huId");

            WCF.InBoundService.EditInBoundResult entity = new WCF.InBoundService.EditInBoundResult();
            entity.AltItemNumber = Request["edit_AltItemNumber"].Replace(@"""", "").Replace(@"'", "").Replace(@" ", "").Trim();
            entity.ReceiptId = Request["receiptId"].Replace(@"""", "").Replace(@"'", "").Replace(@" ", "").Trim();
            entity.WhCode = Session["whCode"].ToString();
            entity.UserCode = Session["userName"].ToString();

            string result = cf.EditDetailAltItemByHu(entity, soList.Distinct().ToArray(), poList.Distinct().ToArray(), itemList.Distinct().ToArray(), huList.ToArray());

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

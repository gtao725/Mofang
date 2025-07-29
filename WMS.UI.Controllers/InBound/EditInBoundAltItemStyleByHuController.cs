using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace WMS.UI.Controllers
{
    public class EditInBoundAltItemStyleByHuController : Controller
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
            List<WCF.InBoundService.ReceiptResult> list = cf.EditInBoundAltItemStyleByHuList(entity, out total).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("ReceiptId", "收货批次号");
            fieldsName.Add("ClientCode", "客户名");
            fieldsName.Add("HuId", "托盘号");
            fieldsName.Add("SoNumber", "SO号");
            fieldsName.Add("CustomerPoNumber", "PO号");
            fieldsName.Add("AltItemNumber", "款号");

            fieldsName.Add("Style1", "属性1");
            fieldsName.Add("Style2", "属性2");
            fieldsName.Add("Style3", "属性3");

            fieldsName.Add("Qty", "实收数量");
            fieldsName.Add("Status", "状态");
            fieldsName.Add("UnitName", "单位名");
            fieldsName.Add("UnitNameShow", "单位");

            return Content(EIP.EipListJson(list, total, fieldsName, "WhCode:40,ReceiptId:120,ClientCode:100,Qty:60,UnitName:60,UnitNameShow:60,HuId:100,Status:100,Style1:70,Style2:70,Style3:70,default:130"));
        }

        //修改
        [HttpPost]
        public ActionResult EditDetailAltItemNumber()
        {
            string[] soList = Request.Form.GetValues("soNumber");
            string[] poList = Request.Form.GetValues("poNumber");
            string[] itemList = Request.Form.GetValues("itemNumber");
            string[] huList = Request.Form.GetValues("huId");

            string[] style1List = Request.Form.GetValues("style1");
            string[] style2List = Request.Form.GetValues("style2");
            string[] style3List = Request.Form.GetValues("style3");

            WCF.InBoundService.EditInBoundResult entity = new WCF.InBoundService.EditInBoundResult();
            entity.Style1 = Request["edit_style1"].Replace(@"""", "").Replace(@"'", "").Replace(@" ", "").Trim();
            entity.Style2 = Request["edit_style2"].Replace(@"""", "").Replace(@"'", "").Replace(@" ", "").Trim();
            entity.Style3 = Request["edit_style3"].Replace(@"""", "").Replace(@"'", "").Replace(@" ", "").Trim();

            entity.ReceiptId = Request["receiptId"];
            entity.WhCode = Session["whCode"].ToString();
            entity.UserCode = Session["userName"].ToString();

            List<string> styleList1 = new List<string>();
            List<string> styleList2 = new List<string>();
            List<string> styleList3 = new List<string>();

            foreach (var item in style1List)
            {
                if (string.IsNullOrEmpty(item) || item == "null")
                {
                    styleList1.Add("");
                }
                else
                {
                    styleList1.Add(item);
                }
            }
            foreach (var item in style2List)
            {

                if (string.IsNullOrEmpty(item) || item == "null")
                {
                    styleList2.Add("");
                }
                else
                {
                    styleList2.Add(item);
                }
            }
            foreach (var item in style3List)
            {
                if (string.IsNullOrEmpty(item) || item == "null")
                {
                    styleList3.Add("");
                }
                else
                {
                    styleList3.Add(item);
                }
            }

            string result = cf.EditDetailAltItemStyleByHu(entity, soList.Distinct().ToArray(), poList.Distinct().ToArray(), itemList.Distinct().ToArray(), styleList1.ToArray(), styleList2.ToArray(), styleList3.ToArray(), huList.ToArray());

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

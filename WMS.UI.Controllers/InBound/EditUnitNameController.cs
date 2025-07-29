using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace WMS.UI.Controllers
{
    public class EditUnitNameController : Controller
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

            

            ViewData["unitNameList"] = from r in cf.UnitDefaultListSelect(Session["whCode"].ToString())
                                       select new SelectListItem()
                                       {
                                           Text = r.UnitNameCN,    //text
                                           Value = r.UnitName.ToString()
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

            int total = 0;
            List<WCF.InBoundService.ReceiptResult> list = cf.EditUnitNameList(entity, out total).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("ReceiptId", "收货批次号");
            fieldsName.Add("ClientCode", "客户名");
            fieldsName.Add("SoNumber", "SO号");
            fieldsName.Add("CustomerPoNumber", "PO号");
            fieldsName.Add("AltItemNumber", "款号");
            fieldsName.Add("Qty", "实收数量");
            fieldsName.Add("UnitName", "单位名");
            fieldsName.Add("UnitNameShow", "单位");

            return Content(EIP.EipListJson(list, total, fieldsName, "WhCode:40,ReceiptId:120,ClientCode:100,Qty:60,UnitName:60,UnitNameShow:60,default:130"));
        }

        //修改收货客户名
        [HttpPost]
        public ActionResult EditUnitName()
        {
            string[] soList = Request.Form.GetValues("soNumber");
            string[] poList = Request.Form.GetValues("poNumber");
            string[] itemList = Request.Form.GetValues("itemNumber");
            string[] unitNameList = Request.Form.GetValues("unitName");

            WCF.InBoundService.EditInBoundResult entity = new WCF.InBoundService.EditInBoundResult();
            entity.UnitName = Request["unitName"].Replace(@"""", "").Replace(@"'", "").Replace(@" ", "").Trim();
            entity.ReceiptId = Request["receiptId"].Replace(@"""", "").Replace(@"'", "").Replace(@" ", "").Trim();
            entity.WhCode = Session["whCode"].ToString();
            entity.UserCode = Session["userName"].ToString();

            string result = cf.EditDetailUnitName(entity, soList.Distinct().ToArray(), poList.Distinct().ToArray(), itemList.Distinct().ToArray(), unitNameList.ToArray());

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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace WMS.UI.Controllers
{
    public class C_RecWebController : Controller
    {
        WCF.RecService.RecServiceClient recService = new WCF.RecService.RecServiceClient();
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

        [HttpPost]
        //添加收货信息
        public ActionResult ReceiptInsert(WCF.RecService.ReceiptInsert entity)
        {
            //return Json(entity);
            entity.WhCode = Session["whCode"].ToString();
            entity.ReceiptDate = DateTime.Now;
            entity.Status = "A";
            entity.CreateUser = Session["userName"].ToString();
            entity.CreateDate = DateTime.Now;

            string result = recService.ReceiptInsert(entity);
            if (result == "")
            {
                return Helper.RedirectAjax("ok", "添加成功！", null, "");
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
            entity.ClientId = Convert.ToInt32(Request["txt_ClientCode"]);
            int total = 0;
            WCF.RecService.HoldMaster[] list = recService.HoldMasterListByRec(entity, out total);

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Id", "操作");
            fieldsName.Add("WhCode", "仓库");
            fieldsName.Add("ClientId", "客户ID");
            fieldsName.Add("ClientCode", "客户Code");
            fieldsName.Add("HoldReason", "异常原因");

            return Content(EIP.EipListJson(list.ToList(), total, fieldsName, "Action:60,HoldReason:150,default:90"));
        }
    }
}

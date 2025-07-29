using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace WMS.UI.Controllers
{
    public class ClientExtendController : Controller
    {
        WCF.InBoundService.InBoundServiceClient inboundcf = new WCF.InBoundService.InBoundServiceClient();
        WCF.RootService.RootServiceClient cf = new WCF.RootService.RootServiceClient();
        Helper Helper = new Helper();
        EIP.EIP EIP = new EIP.EIP();

        [DefaultRequest]
        public ActionResult Index()
        {

            ViewData["WhClientList"] = from r in inboundcf.WhClientListSelect(Session["whCode"].ToString())
                                       select new SelectListItem()
                                       {
                                           Text = r.ClientCode,     //text
                                           Value = r.Id.ToString()
                                       };

            return View();
        }

        //客户列表
        [HttpGet]
        public ActionResult List()
        {
            WCF.RootService.WhClientSearch entity = new WCF.RootService.WhClientSearch();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.WhCode = Session["whCode"].ToString();
            if (!string.IsNullOrEmpty(Request["search_WhClient"]))
            {
                entity.ClientId = Convert.ToInt32(Request["search_WhClient"]);
            }

            int total = 0;
            List<WCF.RootService.WhClientExtendResult> list = cf.WhClientExtendList(entity, out total).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Id", "操作");
            fieldsName.Add("ClientCode", "客户名");
            fieldsName.Add("ClientId", "ClientId");
            fieldsName.Add("InvClearUpSkuMaxQty", "库存SKU警戒数");
            fieldsName.Add("NotOnlySkuPutawayQty", "混合区上架SKU警戒数");
            fieldsName.Add("RegularExpression", "SN验证规则正则");
 
            return Content(EIP.EipListJson(list, total, fieldsName, "Id:60,default:150"));
        }

        //新增客户
        [HttpGet]
        public ActionResult AddClientExtend()
        {
            WCF.RootService.WhClientExtend entity = new WCF.RootService.WhClientExtend();
            entity.WhCode = Session["whCode"].ToString();
            entity.ClientId = Convert.ToInt32(Request["txt_WhClient"]);
            entity.InvClearUpSkuMaxQty = Convert.ToInt32(Request["txt_InvClearUpSkuMaxQty"]);
            entity.NotOnlySkuPutawayQty = Convert.ToInt32(Request["txt_NotOnlySkuPutawayQty"]);
            entity.RegularExpression = Request["txt_RegularExpression"].Trim();
            entity.CreateUser = Session["userName"].ToString();

            WCF.RootService.WhClientExtend result = cf.WhClientExtendAdd(entity);
            if (result != null)
            {
                return Helper.RedirectAjax("ok", "添加成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "对不起，添加失败！", null, "");
            }
        }

        [HttpGet]
        public ActionResult DelWhClientExtend()
        {
            int id = Convert.ToInt32(Request["id"]);
            List<int?> list = new List<int?>();
            list.Add(id);

            string result = cf.WhClientExtendBatchDel(list.ToArray());
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "删除成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }

        }

        [HttpGet]
        public ActionResult EditWhClientExtend()
        {
            WCF.RootService.WhClientExtend entity = new WCF.RootService.WhClientExtend();
            entity.Id = Convert.ToInt32(Request["id"].Trim());
            entity.InvClearUpSkuMaxQty = Convert.ToInt32(Request["edit_InvClearUpSkuMaxQty"].Trim());
            entity.NotOnlySkuPutawayQty = Convert.ToInt32(Request["edit_NotOnlySkuPutawayQty"].Trim());
            entity.RegularExpression = Request["edit_RegularExpression"].Trim();

            entity.UpdateUser = Session["userNameCN"].ToString();

            int result = cf.WhClientExtendEdit(entity);
            if (result > 0)
            {
                return Helper.RedirectAjax("ok", "修改成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "修改失败！", null, "");
            }
        }


    }
}

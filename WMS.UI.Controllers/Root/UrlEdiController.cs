using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace WMS.UI.Controllers
{
    public class UrlEdiController : Controller
    {
        WCF.RootService.RootServiceClient cf = new WCF.RootService.RootServiceClient();
        Helper Helper = new Helper();

        EIP.EIP EIP = new EIP.EIP();

        [DefaultRequest]
        public ActionResult Index()
        {
            ViewData["UrlEdiSelect"] = from r in cf.UrlEdiSelect(Session["whCode"].ToString())
                                             select new SelectListItem()
                                             {
                                                 Text = r.Field,     //text
                                                 Value = r.Field.ToString()
                                             };

            return View();
        }

        //流程列表
        [HttpGet]
        public ActionResult List()
        {
            WCF.RootService.UrlEdiSearch entity = new WCF.RootService.UrlEdiSearch();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.WhCode = Session["whCode"].ToString();
            entity.Url = Request["Url"];

            int total = 0;
            List<WCF.RootService.UrlEdi> list = cf.UrlEdiList(entity, out total).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();

            fieldsName.Add("Id", "操作");
            fieldsName.Add("Url", "Url路径");
            fieldsName.Add("UrlName", "Edi中文名称");
            fieldsName.Add("Field", "类型");
            fieldsName.Add("HttpType", "Http方式");

            return Content(EIP.EipListJson(list, total, fieldsName, "Id:70,Url:200,default:150"));
        }

        //新增流程
        [HttpGet]
        public ActionResult AddUrlEdi()
        {
            WCF.RootService.UrlEdi entity = new WCF.RootService.UrlEdi();
            entity.WhCode = Session["whCode"].ToString();
            entity.Url = Request["txt_Url"].Trim();
            entity.UrlName = Request["txt_UrlName"] ;
            entity.Field = Request["txt_UrlEdiSelect"].Trim();        

            string result = cf.UrlEdiAdd(entity);
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "添加成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "对不起，添加失败！", null, "");
            }
        }

        //修改流程
        [HttpGet]
         public ActionResult UrlEdiEdit()
        {
      
            WCF.RootService.UrlEdi entity = new WCF.RootService.UrlEdi();
            entity.WhCode= Session["whCode"].ToString();
            entity.Id = Convert.ToInt32(Request["Id"]);
            entity.Url = Request["edit_url"].Trim();
           
            string result = cf.UrlEdiEdit(entity);
            if (result =="Y")
            {
                return Helper.RedirectAjax("ok", "信息修改成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "对不起，修改失败！"+result, null, "");
            }
        }

   
        //删除
        [HttpGet]
        public ActionResult UrlEdidel()
        {
            int id = Convert.ToInt32(Request["Id"].ToString());
            int result = cf.UrlEdidel(id);
            if (result > 0)
            {
                return Helper.RedirectAjax("ok", "删除成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "对不起，删除失败！", null, "");
            }
        }

    }
}

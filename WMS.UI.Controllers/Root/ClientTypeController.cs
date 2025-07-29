using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace WMS.UI.Controllers
{
    public class ClientTypeController : Controller
    {
        WCF.RootService.RootServiceClient cf = new WCF.RootService.RootServiceClient();
        Helper Helper = new Helper();

        EIP.EIP EIP = new EIP.EIP();

        [DefaultRequest]
        public ActionResult Index()
        {

            return View();
        }

        //流程列表
        [HttpGet]
        public ActionResult List()
        {
            WCF.RootService.WhClientTypeSearch entity = new WCF.RootService.WhClientTypeSearch();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.WhCode = Session["whCode"].ToString();
            entity.ClientType = Request["clientType"];

            int total = 0;
            List<WCF.RootService.WhClientType> list = cf.WhClientTypeList(entity, out total).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();

            fieldsName.Add("Id", "操作");
            fieldsName.Add("ClientType", "客户类型");
            fieldsName.Add("CreateUser", "创建人");
            fieldsName.Add("CreateDate", "创建时间");

            return Content(EIP.EipListJson(list, total, fieldsName, "Id:70,default:150"));
        }

        //新增流程
        [HttpGet]
        public ActionResult AddClientType()
        {
            WCF.RootService.WhClientType entity = new WCF.RootService.WhClientType();
            entity.WhCode = Session["whCode"].ToString();
            entity.ClientType = Request["txt_clientType"].Trim();
            entity.CreateUser= Session["userName"].ToString();

            string result = cf.WhClientTypeAdd(entity);
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
         public ActionResult ClientTypeEdit()
        {
      
            WCF.RootService.WhClientType entity = new WCF.RootService.WhClientType();
            entity.Id = Convert.ToInt32(Request["Id"]);
            entity.ClientType = Request["edit_clientType"].Trim();
           
            string result = cf.WhClientTypeEdit(entity);
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
        public ActionResult ClientTypedel()
        {
            int id = Convert.ToInt32(Request["Id"].ToString());
            int result = cf.WhClientTypeDel(id);
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

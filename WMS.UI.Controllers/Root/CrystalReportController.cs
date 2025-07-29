using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace WMS.UI.Controllers
{
    public class CrystalReportController : Controller
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
            WCF.RootService.CRReportSearch entity = new WCF.RootService.CRReportSearch();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.WhCode = Session["whCode"].ToString();
            entity.Description = Request["Description"];
            entity.TemplateName = Request["TemplateName"];
            entity.Type = Request["sel_type"];

            int total = 0;
            List<WCF.RootService.CRTemplate> list = cf.GetCRTemplate(entity, out total).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();


            fieldsName.Add("Id", "操作");
            fieldsName.Add("Type", "类型");
            fieldsName.Add("Url", "Url路径");
            fieldsName.Add("TemplateName", "水晶报表文件名");
            fieldsName.Add("Description", "中文描述");
            fieldsName.Add("CreateUser", "创建人");
            fieldsName.Add("CreateDate", "创建时间");
            fieldsName.Add("UpdateUser", "更新人");
            fieldsName.Add("UpdateDate", "更新时间");

            return Content(EIP.EipListJson(list, total, fieldsName, "Id:70,Type:70,CreateUser:70,UpdateUser:70,Url:200,Description:150,default:125"));
        }

        //新增流程
        [HttpGet]
        public ActionResult AddCrystalReport()
        {
            WCF.RootService.CRTemplate entity = new WCF.RootService.CRTemplate();
            entity.WhCode = Session["whCode"].ToString();
            entity.Url = Request["txt_Url"].Trim();
            entity.TemplateName =Request["txt_TemplateName"] ;
            entity.CreateUser = Session["userName"].ToString();
            entity.CreateDate = DateTime.Now;
            entity.Type = Request["txt_type"].Trim();
            entity.Description = Request["txt_Description"].Trim();
             
            string result = cf.CrystallReportAdd(entity);
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "添加成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "对不起，添加失败，单据已存在！", null, "");
            }
        }

        //修改流程
        [HttpGet]
         public ActionResult CrystalReportEdit()
        {
      
            WCF.RootService.CRTemplate entity = new WCF.RootService.CRTemplate();
            entity.WhCode= Session["whCode"].ToString();
            entity.Id = Convert.ToInt32(Request["Id"]);
            entity.Url = Request["edit_url"].Trim();
            entity.TemplateName = Request["edit_TemplateName"].Trim();
            entity.Description = Request["edit_Description"].Trim();
            entity.UpdateUser = Session["userName"].ToString();
            //entity.UpdateDate = DateTime.Now;
            string result = cf.CrystallReportEdit(entity);
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
        public ActionResult CrystallReportdel()
        {
            int id = Convert.ToInt32(Request["Id"].ToString());
            int result = cf.CrystallReportdel(id);
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

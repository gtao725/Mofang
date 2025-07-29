using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace WMS.UI.Controllers
{
    public class NightTimeController : Controller
    {
        WCF.RootService.RootServiceClient cf = new WCF.RootService.RootServiceClient();
        Helper Helper = new Helper();

        EIP.EIP EIP = new EIP.EIP();

        [DefaultRequest]
        public ActionResult Index()
        {

            return View();
        }

        //查询列表
        [HttpGet]
        public ActionResult List()
        {
            WCF.RootService.NightTimeSearch entity = new WCF.RootService.NightTimeSearch();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.WhCode = Session["whCode"].ToString();

            int total = 0;
            List<WCF.RootService.NightTime> list = cf.NightTimeList(entity, out total).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();

            fieldsName.Add("Id", "操作");
            fieldsName.Add("NightBegin", "夜班开始时间");
            fieldsName.Add("NightEnd", "夜班结束时间");
            fieldsName.Add("CreateUser", "创建人");
            fieldsName.Add("CreateDate", "创建时间");

            return Content(EIP.EipListJson(list, total, fieldsName, "Id:70,CreateUser:70,default:130"));
        }

        //新增流程
        [HttpGet]
        public ActionResult AddNightTime()
        {
            WCF.RootService.NightTime entity = new WCF.RootService.NightTime();
            entity.WhCode = Session["whCode"].ToString();
            entity.NightBegin = Request["txt_nightTimeBegin"].Trim();
            entity.NightEnd = Request["txt_nightTimeEnd"].Trim();

            entity.CreateUser= Session["userName"].ToString();

            string result = cf.NightTimeAdd(entity);
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
         public ActionResult NightTimeEdit()
        {
      
            WCF.RootService.NightTime entity = new WCF.RootService.NightTime();
            entity.Id = Convert.ToInt32(Request["Id"]);
            entity.NightBegin = Request["edit_nightTimeBegin"].Trim();
            entity.NightEnd = Request["edit_nightTimeEnd"].Trim();

            string result = cf.NightTimeEdit(entity);
            if (result =="Y")
            {
                return Helper.RedirectAjax("ok", "信息修改成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "对不起，修改失败！", null, "");
            }
        }

   
        //删除
        [HttpGet]
        public ActionResult NightTimedel()
        {
            int id = Convert.ToInt32(Request["Id"].ToString());
            int result = cf.NightTimeDel(id);
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

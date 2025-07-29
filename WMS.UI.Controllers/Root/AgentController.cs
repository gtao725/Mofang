using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace WMS.UI.Controllers
{
    public class AgentController : Controller
    {
        WCF.RootService.RootServiceClient cf = new WCF.RootService.RootServiceClient();
        Helper Helper = new Helper();
        EIP.EIP EIP = new EIP.EIP();
        [DefaultRequest]
        public ActionResult Index()
        {
            return View();
        }

        //代理列表
        [HttpGet]
        public ActionResult List()
        {
            WCF.RootService.WhAgentSearch entity = new WCF.RootService.WhAgentSearch();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.WhCode = Session["whCode"].ToString();
            entity.AgentName = Request["agent_name"].Trim();
            entity.AgentCode = Request["agent_code"].Trim();
            entity.AgentType = Request["agent_type"].Trim();
            int total = 0;
            List<WCF.RootService.WhAgent> list = cf.WhAgentList(entity, out total).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Id", "操作");
            fieldsName.Add("WhCode", "仓库");
            fieldsName.Add("AgentName", "代理名");
            fieldsName.Add("AgentCode", "代理Code");
            fieldsName.Add("AgentType", "代理类型");
            fieldsName.Add("CreateUser", "创建人");
            fieldsName.Add("CreateDate", "创建时间");
            fieldsName.Add("UpdateUser", "修改人");
            fieldsName.Add("UpdateDate", "修改时间");

            return Content(EIP.EipListJson(list, total, fieldsName, "Id:60,CreateDate:130,default:80"));
        }

        //新增代理
        [HttpGet]
        public ActionResult AddAgent()
        {
            WCF.RootService.WhAgent entity = new WCF.RootService.WhAgent();

            entity.WhCode = Session["whCode"].ToString();
            entity.AgentName = Request["txt_agentName"].Trim();
            entity.AgentCode = Request["txt_agentCode"].Trim();
            entity.AgentType = Request["txt_agentType"].Trim();
            entity.CreateUser = Session["userName"].ToString();
            entity.CreateDate = DateTime.Now;

            WCF.RootService.WhAgent result = cf.WhAgentAdd(entity);
            if (result != null)
            {
                return Helper.RedirectAjax("ok", "添加成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "对不起，添加失败，代理Code已存在！", null, "");
            }
        }


        //修改代理信息
        [HttpGet]
        public ActionResult WhAgentEdit()
        {
            WCF.RootService.WhAgent entity = new WCF.RootService.WhAgent();
            entity.Id = Convert.ToInt32(Request["Id"]);
            entity.WhCode = Request["WhCode"];
            entity.AgentName = Request["AgentName"].Trim();
            entity.AgentCode = Request["AgentCode"].Trim();
            entity.UpdateUser = Session["userName"].ToString();
            entity.UpdateDate = DateTime.Now;

            int result = cf.WhAgentEdit(entity, new string[] { "AgentName", "AgentCode", "UpdateUser", "UpdateDate" });
            if (result > 0)
            {
                return Helper.RedirectAjax("ok", "修改成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "对不起，修改失败，代理Code已存在！", null, "");
            }
        }

    }
}

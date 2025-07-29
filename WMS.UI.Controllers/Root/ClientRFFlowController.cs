using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace WMS.UI.Controllers
{
    public class ClientRFFlowController : Controller
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
            WCF.RootService.BusinessFlowGroupSearch entity = new WCF.RootService.BusinessFlowGroupSearch();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.WhCode = Session["whCode"].ToString();
            entity.FlowName = Request["flowName"].Trim();
            int total = 0;
            List<WCF.RootService.BusinessFlowGroupResult> list = cf.ClientRFFlowList(entity, out total).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Id", "操作");
            fieldsName.Add("FlowName", "流程名");
            fieldsName.Add("Remark", "流程备注");
            fieldsName.Add("CreateUser", "创建人");
            fieldsName.Add("CreateDate", "创建时间");
            fieldsName.Add("UpdateUser", "修改人");
            fieldsName.Add("UpdateDate", "修改时间");

            return Content(EIP.EipListJson(list, total, fieldsName, "Id:90,FlowName:160,Remark:200,CreateDate:130,UpdateDate:130,default:80"));
        }

        //新增流程
        [HttpGet]
        public ActionResult AddClientRFFlow()
        {
            WCF.RootService.BusinessFlowGroup entity = new WCF.RootService.BusinessFlowGroup();
            entity.WhCode = Session["whCode"].ToString();
            entity.FlowName = Request["txt_FlowName"].Trim();
            entity.CreateUser = Session["userName"].ToString();
            entity.CreateDate = DateTime.Now;
            entity.Type = "InBound";
            entity.Remark = Request["txt_remark"].Trim();

            WCF.RootService.BusinessFlowGroup result = cf.AddClientRFFlow(entity);
            if (result != null)
            {
                return Helper.RedirectAjax("ok", "添加成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "对不起，添加失败，流程名已存在！", null, "");
            }
        }

        //修改流程
        [HttpGet]
        public ActionResult ClientRFFlowEdit()
        {
            WCF.RootService.BusinessFlowGroup entity = new WCF.RootService.BusinessFlowGroup();
            entity.Id = Convert.ToInt32(Request["Id"]);
            entity.FlowName = Request["FlowName"].Trim();
            entity.Remark = Request["Remark"].Trim();
            entity.UpdateUser = Session["userName"].ToString();
            entity.UpdateDate = DateTime.Now;
            int result = cf.ClientRFFlowEdit(entity, new string[] { "FlowName", "Remark", "UpdateUser", "UpdateDate" });
            if (result > 0)
            {
                return Helper.RedirectAjax("ok", "信息修改成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "对不起，修改失败，流程名已存在！", null, "");
            }
        }

        //流程明细列表
        [HttpGet]
        public ActionResult RFFlowRuleDetailList()
        {
            WCF.RootService.BusinessFlowHeadSearch entity = new WCF.RootService.BusinessFlowHeadSearch();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.BusinessFlowGroupId = Convert.ToInt32(Request["BusinessFlowGroupId"]);
            int total = 0;
            WCF.RootService.BusinessFlowHeadResult[] list = cf.RFFlowRuleDetailList(entity, out total);

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Id", "操作");
            fieldsName.Add("FunctionName", "流程名");
            fieldsName.Add("Description", "功能描述");
            fieldsName.Add("SelectRuleDescription", "选择流程描述");
            fieldsName.Add("BusinessFlowHeadId", "BusinessFlowHeadId");
            fieldsName.Add("FlowRuleId", "FlowRuleId");
            fieldsName.Add("GroupId", "GroupId");

            return Content(EIP.EipListJson(list.ToList(), total, fieldsName, "FunctionName:220,Description:250,SelectRuleDescription:300,default:170"));
        }


        //新增客户流程配置
        [HttpPost]
        public ActionResult AddClientFlowDetail()
        {
            string[] flowRuleId = Request.Form.GetValues("flowRuleId");
            string[] funcName = Request.Form.GetValues("funcName");
            string[] headId = Request.Form.GetValues("headId");
            string[] groupId = Request.Form.GetValues("groupId");

            WCF.RootService.BusinessFlowHeadInsert entity = new WCF.RootService.BusinessFlowHeadInsert();
            entity.busGroupId = Convert.ToInt32(Request["busGroupId"]);

            List<WCF.RootService.FlowRuleModel> flowList = new List<WCF.RootService.FlowRuleModel>();
            for (int i = 0; i < flowRuleId.Length; i++)
            {
                WCF.RootService.FlowRuleModel flow = new WCF.RootService.FlowRuleModel();
                flow.Id = Convert.ToInt32(flowRuleId[i].ToString());
                flow.FunctionName = funcName[i].ToString();
                flow.BusinessFlowHeadId = Convert.ToInt32(headId[i].ToString());
                if (groupId[i] == "null" || groupId[i] == "")
                {
                    flow.GroupId = 0;
                }
                else
                {
                    flow.GroupId = Convert.ToInt32(groupId[i].ToString());
                }
                flowList.Add(flow);
            }
            entity.FlowRuleModel = flowList.ToArray();

            string result = cf.AddClientFlowDetail(entity);
            if (result == "")
            {
                return Helper.RedirectAjax("ok", "添加成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }
        }
    }
}

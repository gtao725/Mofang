using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace WMS.UI.Controllers
{
    public class FlowRuleController : Controller
    {
        WCF.AdminService.AdminServiceClient cf = new WCF.AdminService.AdminServiceClient();
        Helper Helper = new Helper();
        EIP.EIP EIP = new EIP.EIP();

        [DefaultRequest]
        public ActionResult Index()
        {
            ViewData["FlowNameSelect"] = from r in cf.FlowNameSelect(Session["whCode"].ToString())
                                         select new SelectListItem()
                                         {
                                             Text = r.FlowName,     //text
                                             Value = r.Id.ToString()
                                         };
            return View();
        }

        /// <summary>
        /// 查询流程规则
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult List()
        {
            WCF.AdminService.FlowRuleSearch entity = new WCF.AdminService.FlowRuleSearch();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.FunctionName = Request["functionName"];

            int total = 0;
            List<WCF.AdminService.FlowRuleResult> list = cf.FlowRuleList(entity, out total).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Id", "操作");
            fieldsName.Add("FunctionId", "流程ID");
            fieldsName.Add("FunctionName", "流程名");
            fieldsName.Add("StatusName", "状态名");
            fieldsName.Add("Description", "功能说明");
            fieldsName.Add("GroupId", "组号");
            fieldsName.Add("FunctionFlag", "是否功能");
            fieldsName.Add("RequiredFlag", "是否必填");
            fieldsName.Add("RelyId", "依赖ID");
            fieldsName.Add("FlowName", "对应RF流程");
            fieldsName.Add("BusinessObjectGroupId", "对应流程ID");
            fieldsName.Add("SelectRuleDescription", "选择功能描述");
            fieldsName.Add("RollbackFlag", "RollbackFlag");
            fieldsName.Add("RollbackFlagShow", "可否回滚");

            return Content(EIP.EipListJson(list, total, fieldsName, "StatusName:100,FunctionName:200,Description:220,BusinessObjectHeadId:90,FlowName:130,SelectRuleDescription:200,default:60"));
        }


        /// <summary>
        /// 新增流程规则对象
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult AddFlorRule()
        {
            WCF.AdminService.FlowRule entity = new WCF.AdminService.FlowRule();
            entity.FunctionId = Convert.ToInt32(Request["txt_id"]);
            entity.FunctionName = Request["txt_functionName"];
            entity.StatusName = Request["txt_StatusName"];
            entity.RuleType = "OutBound";
            entity.Description = Request["txt_description"];
            if (Request["txt_groupId"] != "")
            {
                entity.GroupId = Convert.ToInt32(Request["txt_groupId"]);
            }
            else
            {
                entity.GroupId = 0;
            }

            entity.FunctionFlag = Convert.ToInt32(Request["txt_functionFlag"]);
            entity.RequiredFlag = Convert.ToInt32(Request["txt_requiredFlag"]);
            if (Request["txt_relyId"] != "")
            {
                entity.RelyId = Convert.ToInt32(Request["txt_relyId"]);
            }
            else
            {
                entity.RelyId = 0;
            }

            if (Request["txt_businessObjectGroupId"] != "")
            {
                entity.BusinessObjectGroupId = Convert.ToInt32(Request["txt_businessObjectGroupId"]);
            }
            else
            {
                entity.BusinessObjectGroupId = 0;
            }
            entity.SelectRuleDescription = Request["txt_selectRuleDescription"];
            entity.RollbackFlag = Convert.ToInt32(Request["txt_rollbackFlag"]);

            WCF.AdminService.FlowRule result = cf.AddFlowRule(entity);
            if (result != null)
            {
                return Helper.RedirectAjax("ok", "添加成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "对不起，添加失败，流程名已存在！", null, "");
            }
        }


        /// <summary>
        /// 修改流程规则对象
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult EditFlorRule()
        {
            WCF.AdminService.FlowRule entity = new WCF.AdminService.FlowRule();
            entity.Id= Convert.ToInt32(Request["Id"]);
            entity.FunctionId = Convert.ToInt32(Request["edit_Id"]);
            entity.FunctionName = Request["edit_FunctionName"];
            entity.StatusName = Request["edit_StatusName"];
            entity.Description = Request["edit_Description"];
            if (Request["edit_GroupId"] != "")
            {
                entity.GroupId = Convert.ToInt32(Request["edit_GroupId"]);
            }

            entity.FunctionFlag = Convert.ToInt32(Request["edit_FunctionFlag"]);
            entity.RequiredFlag = Convert.ToInt32(Request["edit_RequiredFlag"]);
            if (Request["edit_RelyId"] != "")
            {
                entity.RelyId = Convert.ToInt32(Request["edit_RelyId"]);
            }
            else
            {
                entity.RelyId = 0;
            }
            if (Request["edit_BusinessObjectGroupId"] != "")
            {
                entity.BusinessObjectGroupId = Convert.ToInt32(Request["edit_BusinessObjectGroupId"]);
            }
            entity.SelectRuleDescription = Request["edit_SelectRuleDescription"];
            entity.RollbackFlag = Convert.ToInt32(Request["edit_RollbackFlag"]);

            int result = cf.EditFlowRule(entity);
            if (result > 0)
            {
                return Helper.RedirectAjax("ok", "修改成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "对不起，修改失败！", null, "");
            }
        }

        //删除流程规则对象
        [HttpGet]
        public ActionResult FlowRuleDel()
        {
            int id = Convert.ToInt32(Request["Id"]);
            int result = cf.FlowRuleDel(id);
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

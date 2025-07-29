using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace WMS.UI.Controllers
{
    public class RFFlowRuleController : Controller
    {
        WCF.AdminService.AdminServiceClient cf = new WCF.AdminService.AdminServiceClient();
        Helper Helper = new Helper();
        EIP.EIP EIP = new EIP.EIP();

        [DefaultRequest]
        public ActionResult Index()
        {

            return View();
        }

        /// <summary>
        /// 查询流程规则
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult List()
        {
            WCF.AdminService.RFFlowRuleSearch entity = new WCF.AdminService.RFFlowRuleSearch();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.FunctionName = Request["functionName"];

            int total = 0;
            List<WCF.AdminService.RFFlowRuleResult> list = cf.RFFlowRuleList(entity, out total).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Id", "操作");
            fieldsName.Add("FunctionId", "流程ID");
            fieldsName.Add("FunctionName", "流程名");
            fieldsName.Add("Description", "功能说明");
            fieldsName.Add("GroupId", "组号");
            fieldsName.Add("FunctionFlag", "是否功能");
            fieldsName.Add("RequiredFlag", "是否必填");
            fieldsName.Add("RelyId", "依赖ID");
            fieldsName.Add("BusinessObjectHeadId", "对应程序ID");
            fieldsName.Add("SelectRuleDescription", "选择功能描述");

            return Content(EIP.EipListJson(list, total, fieldsName, "FunctionName:110,Description:200,BusinessObjectHeadId:90,SelectRuleDescription:200,default:60"));
        }


        /// <summary>
        /// 新增流程规则对象
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult AddFlorRule()
        {
            WCF.AdminService.RFFlowRule entity = new WCF.AdminService.RFFlowRule();
            entity.FunctionId = Convert.ToInt32(Request["txt_id"]);
            entity.FunctionName = Request["txt_functionName"];
            entity.RuleType = "InBound";
            entity.Description = Request["txt_description"];
            if (Request["txt_groupId"] != "")
            {
                entity.GroupId = Convert.ToInt32(Request["txt_groupId"]);
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
            if (Request["txt_businessObjectHeadId"] != "")
            {
                entity.BusinessObjectHeadId = Convert.ToInt32(Request["txt_businessObjectHeadId"]);
            }
            entity.SelectRuleDescription = Request["txt_selectRuleDescription"];

            WCF.AdminService.RFFlowRule result = cf.AddRFFlorRule(entity);
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
            WCF.AdminService.RFFlowRule entity = new WCF.AdminService.RFFlowRule();
            entity.Id = Convert.ToInt32(Request["Id"]);
            entity.FunctionId = Convert.ToInt32(Request["edit_Id"]);
            entity.FunctionName = Request["edit_FunctionName"];
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

            if (Request["edit_BusinessObjectHeadId"] != "")
            {
                entity.BusinessObjectHeadId = Convert.ToInt32(Request["edit_BusinessObjectHeadId"]);
            }
            entity.SelectRuleDescription = Request["edit_SelectRuleDescription"];

            int result = cf.EditRFFlowRule(entity);
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
            int result = cf.RFFlowRuleDel(id);
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

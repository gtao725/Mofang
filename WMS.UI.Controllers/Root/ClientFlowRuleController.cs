using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace WMS.UI.Controllers
{
    public class ClientFlowRuleController : Controller
    {
        WCF.RootService.RootServiceClient cf = new WCF.RootService.RootServiceClient();
        Helper Helper = new Helper();

        EIP.EIP EIP = new EIP.EIP();

        [DefaultRequest]
        public ActionResult Index()
        {
            ViewData["FieldOrderBySelect"] = from r in cf.FieldOrderBySelect(Session["whCode"].ToString())
                                             select new SelectListItem()
                                             {
                                                 Text = r.Description,     //text
                                                 Value = r.Id.ToString()
                                             };

            ViewData["InTempSelect"] = from r in cf.TempSelect("In", Session["whCode"].ToString())
                                       select new SelectListItem()
                                       {
                                           Text = r.Description,     //text
                                           Value = r.TemplateName.ToString()
                                       };

            ViewData["OutTempSelect"] = from r in cf.TempSelect("Out", Session["whCode"].ToString())
                                        select new SelectListItem()
                                        {
                                            Text = r.Description,     //text
                                            Value = r.TemplateName.ToString()
                                        };

            ViewData["RecPZTempSelect"] = from r in cf.TempSelect("Rec", Session["whCode"].ToString())
                                          select new SelectListItem()
                                          {
                                              Text = r.Description,     //text
                                              Value = r.TemplateName.ToString()
                                          };

            ViewData["UrlEdiSelect"] = from r in cf.UrlEdiSelect(Session["whCode"].ToString())
                                       select new SelectListItem()
                                       {
                                           Text = r.UrlName,     //text
                                           Value = r.Id.ToString()
                                       };


            return View();
        }

        //流程列表
        [HttpGet]
        public ActionResult List()
        {
            WCF.RootService.FlowHeadSearch entity = new WCF.RootService.FlowHeadSearch();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.WhCode = Session["whCode"].ToString();
            entity.FlowName = Request["flowName"];
            entity.Type = Request["txt_type"];

            int total = 0;
            List<WCF.RootService.FlowHeadResult> list = cf.ClientFlowRuleList(entity, out total).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Id", "操作");
            fieldsName.Add("FlowName", "流程名");
            fieldsName.Add("Type", "类型");
            fieldsName.Add("TypeName", "类型名称");
            fieldsName.Add("InterceptFlag", "是否可拦截");
            fieldsName.Add("FlowOrderBy", "流程排序");
            fieldsName.Add("FieldOrderById", "排序ID");
            fieldsName.Add("OrderByDescription", "报表排序");
            fieldsName.Add("InTemplateShow", "收货操作单");
            fieldsName.Add("PZTemplateShow", "收货凭证");
            fieldsName.Add("CheckAllHuWeightShow", "收货托盘是否称重");

            fieldsName.Add("OutTemplateShow", "出货操作单");
            fieldsName.Add("UrlNameShow", "Edi任务名称");
            fieldsName.Add("UrlNameShow2", "Edi任务名称2");
            fieldsName.Add("UrlNameShow3", "Edi任务名称3");


            fieldsName.Add("Remark", "流程备注");
            fieldsName.Add("CreateUser", "创建人");
            fieldsName.Add("CreateDate", "创建时间");
            fieldsName.Add("UpdateUser", "修改人");
            fieldsName.Add("UpdateDate", "修改时间");
            fieldsName.Add("InTemplate", "inTemplate");
            fieldsName.Add("PZTemplate", "pZTemplate");
            fieldsName.Add("OutTemplate", "outTemplate");
            fieldsName.Add("UrlEdiId", "UrlEdi主键");
            fieldsName.Add("UrlEdiId2", "UrlEdi主键2");
            fieldsName.Add("UrlEdiId3", "UrlEdi主键3");
            fieldsName.Add("CheckAllHuWeightFlag", "checkAllHuWeightFlag");

            return Content(EIP.EipListJson(list, total, fieldsName, "Id:70,FlowName:160,UrlNameShow:140,UrlNameShow2:140,UrlNameShow3:140,Remark:200,OrderByDescription:130,CreateDate:130,UpdateDate:130,ContractName:120,CheckAllHuWeightShow:130,default:75"));
        }

        //新增流程
        [HttpGet]
        public ActionResult AddClientFlowRule()
        {
            WCF.RootService.FlowHead entity = new WCF.RootService.FlowHead();
            entity.WhCode = Session["whCode"].ToString();
            entity.FlowName = Request["txt_FlowName"].Trim();
            entity.InterceptFlag = Convert.ToInt32(Request["txt_interceptFlag"]);
            entity.CreateUser = Session["userName"].ToString();
            entity.CreateDate = DateTime.Now;
            entity.Type = Request["txt_Type"].Trim();
            entity.Remark = Request["txt_remark"].Trim();
            if (Request["txt_FieldOrderBy"] != "")
            {
                entity.FieldOrderById = Convert.ToInt32(Request["txt_FieldOrderBy"]);
            }
            else
            {
                entity.FieldOrderById = 0;
            }
            if (entity.Type == "InBound")
            {
                if (Request["txt_inTemplate"].Trim() == "")
                {
                    return Helper.RedirectAjax("err", "收货操作单不能为空！", null, "");
                }
                if (Request["txt_pzTemplate"].Trim() == "")
                {
                    return Helper.RedirectAjax("err", "收货凭证不能为空！", null, "");
                }

                entity.InTemplate = Request["txt_inTemplate"].Trim();
                entity.PZTemplate = Request["txt_pzTemplate"].Trim();
            }
            else if (entity.Type == "OutBound")
            {
                if (Request["txt_outTemplate"].Trim() == "")
                {
                    return Helper.RedirectAjax("err", "出货操作单不能为空！", null, "");
                }
                entity.OutTemplate = Request["txt_outTemplate"].Trim();
            }
            else
            {
                return Helper.RedirectAjax("err", "流程类型不能为空！", null, "");
            }
            if (Request["txt_urlEdiId"] != "")
            {
                entity.UrlEdiId = Convert.ToInt32(Request["txt_urlEdiId"]);
            }
            else
            {
                entity.UrlEdiId = 0;
            }

            if (Request["txt_checkAllHuWeightFlag"] != "")
            {
                entity.CheckAllHuWeightFlag = Convert.ToInt32(Request["txt_checkAllHuWeightFlag"]);
            }
            else
            {
                entity.CheckAllHuWeightFlag = 0;
            }

            WCF.RootService.FlowHead result = cf.AddClientFlowRule(entity);
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
        public ActionResult ClientFlowRuleEdit()
        {
            WCF.RootService.FlowHead entity = new WCF.RootService.FlowHead();
            entity.Id = Convert.ToInt32(Request["Id"]);
            entity.FlowName = Request["FlowName"].Trim();
            entity.Remark = Request["Remark"].Trim();

            if (Request["FlowOrderBy"] != "")
            {
                entity.FlowOrderBy = Convert.ToInt32(Request["FlowOrderBy"]);
            }
            else
            {
                entity.FlowOrderBy = 0;
            }

            if (Request["orderById"] != "")
            {
                entity.FieldOrderById = Convert.ToInt32(Request["orderById"]);
            }
            else
            {
                entity.FieldOrderById = 0;
            }
            if (Request["edit_urlEdiId"] != "")
            {
                entity.UrlEdiId = Convert.ToInt32(Request["edit_urlEdiId"]);
            }
            else
            {
                entity.UrlEdiId = 0;
            }

            if (Request["edit_urlEdiId2"] != "")
            {
                entity.UrlEdiId2 = Convert.ToInt32(Request["edit_urlEdiId2"]);
            }
            else
            {
                entity.UrlEdiId2 = 0;
            }

            if (Request["edit_urlEdiId3"] != "")
            {
                entity.UrlEdiId3 = Convert.ToInt32(Request["edit_urlEdiId3"]);
            }
            else
            {
                entity.UrlEdiId3 = 0;
            }

            if (Request["edit_checkAllHuWeightFlag"] != "")
            {
                if (Request["edit_checkAllHuWeightFlag"] == "null")
                {
                    entity.CheckAllHuWeightFlag = 0;
                }
                else
                {
                    entity.CheckAllHuWeightFlag = Convert.ToInt32(Request["edit_checkAllHuWeightFlag"]);
                }
            }
            else
            {
                entity.CheckAllHuWeightFlag = 0;
            }

            int result = 0;
            if (Request["inTemplate"] != null)
            {
                entity.InTemplate = Request["inTemplate"].Trim();
                entity.PZTemplate = Request["pzTemplate"].Trim();

                entity.UpdateUser = Session["userName"].ToString();
                entity.UpdateDate = DateTime.Now;
                result = cf.ClientFlowRuleEdit(entity, new string[] { "InTemplate", "PZTemplate", "FlowName", "Remark", "FieldOrderById", "UrlEdiId", "UrlEdiId2", "UrlEdiId3", "UpdateUser", "UpdateDate", "FlowOrderBy", "CheckAllHuWeightFlag" });
            }
            if (Request["outTemplate"] != null)
            {
                entity.OutTemplate = Request["outTemplate"].Trim();

                entity.UpdateUser = Session["userName"].ToString();
                entity.UpdateDate = DateTime.Now;
                result = cf.ClientFlowRuleEdit(entity, new string[] { "OutTemplate", "FlowName", "Remark", "FieldOrderById", "UrlEdiId", "UrlEdiId2", "UrlEdiId3", "UpdateUser", "UpdateDate", "FlowOrderBy" });
            }

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
        public ActionResult FlowRuleDetailList()
        {
            WCF.RootService.FlowDetailSearch entity = new WCF.RootService.FlowDetailSearch();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.FlowHeadId = Convert.ToInt32(Request["FlowHeadId"]);
            int total = 0;
            WCF.RootService.FlowDetailResult[] list = cf.FlowRuleDetailList(entity, out total);

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("FlowRuleId", "操作");
            fieldsName.Add("FlowHeadId", "FlowHeadId");
            fieldsName.Add("FunctionName", "流程名");
            fieldsName.Add("Description", "功能描述");
            fieldsName.Add("GroupId", "GroupId");
            fieldsName.Add("SelectRuleDescription", "选择流程描述");
            fieldsName.Add("BusinessObjectGroupId", "BusinessObjectGroupId");
            fieldsName.Add("RollbackFlag", "RollbackFlag");
            fieldsName.Add("StatusId", "StatusId");
            fieldsName.Add("StatusName", "状态名");
            fieldsName.Add("Type", "Type");
            fieldsName.Add("Mark", "Mark");

            return Content(EIP.EipListJson(list.ToList(), total, fieldsName, "FunctionName:220,Description:160,RollbackFlagShow:70,SelectRuleDescription:200,default:100"));
        }


        //新增客户流程配置
        [HttpPost]
        public ActionResult AddClientFlowDetail()
        {
            string[] flowRuleId = Request.Form.GetValues("flowRuleId");
            string[] funcName = Request.Form.GetValues("funcName");
            string[] businessObjectGroupId = Request.Form.GetValues("businessObjectGroupId");
            string[] rollbackFlag = Request.Form.GetValues("rollbackFlag");
            string[] groupId = Request.Form.GetValues("groupId");
            string[] statusId = Request.Form.GetValues("statusId");
            string[] statusName = Request.Form.GetValues("statusName");
            string[] typeName = Request.Form.GetValues("typeName");
            string[] mark = Request.Form.GetValues("mark");


            WCF.RootService.FlowHeadInsert entity = new WCF.RootService.FlowHeadInsert();
            entity.FlowHeadId = Convert.ToInt32(Request["FlowHeadId"]);

            List<WCF.RootService.FlowDetailModel> flowList = new List<WCF.RootService.FlowDetailModel>();
            for (int i = 0; i < flowRuleId.Length; i++)
            {
                WCF.RootService.FlowDetailModel flow = new WCF.RootService.FlowDetailModel();
                flow.FlowRuleId = Convert.ToInt32(flowRuleId[i].ToString());
                flow.FunctionName = funcName[i].ToString();
                if (businessObjectGroupId[i] == "null" || businessObjectGroupId[i] == "")
                {
                    flow.BusinessObjectGroupId = 0;
                }
                else
                {
                    flow.BusinessObjectGroupId = Convert.ToInt32(businessObjectGroupId[i].ToString());
                }
                if (groupId[i] == "null" || groupId[i] == "")
                {
                    flow.GroupId = 0;
                }
                else
                {
                    flow.GroupId = Convert.ToInt32(groupId[i].ToString());
                }
                if (rollbackFlag[i] == "null" || rollbackFlag[i] == "")
                {
                    flow.RollbackFlag = 0;
                }
                else
                {
                    flow.RollbackFlag = Convert.ToInt32(rollbackFlag[i].ToString());
                }
                flow.StatusId = Convert.ToInt32(statusId[i].ToString());
                flow.StatusName = statusName[i].ToString();
                flow.Type = typeName[i].ToString();
                flow.Mark = mark[i].ToString();

                flowList.Add(flow);
            }
            entity.FlowDetailModel = flowList.ToArray();

            string result = cf.AddClientOutFlowDetail(entity);
            if (result == "")
            {
                return Helper.RedirectAjax("ok", "添加成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }
        }



        //---------------------------------  收货流程管理
        //根据当前收货流程查询出未选择的RF流程
        [HttpGet]
        public ActionResult ClientFlowNameUnselected()
        {
            WCF.RootService.BusinessFlowGroupSearch entity = new WCF.RootService.BusinessFlowGroupSearch();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.WhCode = Session["whCode"].ToString();
            entity.FlowHeadId = Convert.ToInt32(Request["Id"]);
            entity.Type = Request["FlowType"];
            entity.FlowName = Request["FlowName"].Trim();
            int total = 0;
            WCF.RootService.BusinessFlowGroupResult[] list = cf.ClientFlowNameUnselected(entity, out total);

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Id", "Id");
            fieldsName.Add("FlowName", "流程名");
            fieldsName.Add("Remark", "流程备注");

            return Content(EIP.EipListJson(list.ToList(), total, fieldsName, "default:140"));
        }

        //根据当前收货流程查询出已选择的RF流程
        [HttpGet]
        public ActionResult ClientFlowNameSelected()
        {
            WCF.RootService.BusinessFlowGroupSearch entity = new WCF.RootService.BusinessFlowGroupSearch();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.FlowHeadId = Convert.ToInt32(Request["Id"]);
            entity.Type = Request["FlowType"];
            entity.WhCode = Session["whCode"].ToString();
            int total = 0;
            WCF.RootService.BusinessFlowGroupResult[] list = cf.ClientFlowNameSelected(entity, out total);

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Id", "操作");
            fieldsName.Add("FlowName", "流程名");
            fieldsName.Add("Remark", "流程备注");

            return Content(EIP.EipListJson(list.ToList(), total, fieldsName, "Id:60,default:140"));
        }


        //新增客户流程关系
        [HttpPost]
        public ActionResult AddClientFlowRuleDetail()
        {
            string[] Pow_Id = Request.Form.GetValues("chx_Pos");
            int FlowHeadId = Convert.ToInt32(Request["Hid_PId"]);            //当前选择的流程ID

            List<WCF.RootService.FlowDetail> list = new List<WCF.RootService.FlowDetail>();
            for (int i = 0; i < Pow_Id.Length; i++)
            {
                WCF.RootService.FlowDetail entity = new WCF.RootService.FlowDetail();
                entity.FlowHeadId = FlowHeadId;
                entity.BusinessObjectGroupId = Convert.ToInt32(Pow_Id[i].ToString());
                entity.Type = "InBound";
                entity.FunctionName = "InBound";
                entity.OrderId = 1000;
                list.Add(entity);
            }

            int result = cf.AddClientFlowRuleDetail(list.ToArray());
            if (result > 0)
            {
                return Helper.RedirectAjax("ok", "添加成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "对不起，添加失败！", null, "");
            }
        }

        //删除客户的某个流程
        [HttpGet]
        public ActionResult ClientFlowNameDel()
        {
            int id = Convert.ToInt32(Request["Id"].ToString());
            int result = cf.ClientFlowNameDel(id);
            if (result > 0)
            {
                return Helper.RedirectAjax("ok", "取消成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "对不起，取消失败！", null, "");
            }
        }

    }
}

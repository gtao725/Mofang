using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace WMS.UI.Controllers
{
    public class ClientController : Controller
    {
        WCF.RootService.RootServiceClient cf = new WCF.RootService.RootServiceClient();
        Helper Helper = new Helper();
        EIP.EIP EIP = new EIP.EIP();

        [DefaultRequest]
        public ActionResult Index()
        {
            ViewData["ZoneSelect"] = from r in cf.ZoneSelect(Session["whCode"].ToString())
                                     select new SelectListItem()
                                     {
                                         Text = r.ZoneName,     //text
                                         Value = r.Id.ToString()
                                     };

            ViewData["WhClientTypeList"] = from r in cf.WhClientTypeListSelect(Session["whCode"].ToString())
                                           select new SelectListItem()
                                           {
                                               Text = r.ClientType,     //text
                                               Value = r.ClientType.ToString()
                                           };

            ViewData["NightTimeList"] = from r in cf.NightTimeListSelect(Session["whCode"].ToString())
                                        select new SelectListItem()
                                        {
                                            Text = r.NightBegin + "-" + r.NightEnd,     //text
                                            Value = r.NightBegin + "-" + r.NightEnd
                                        };

            ViewData["ContractNameSelect"] = from r in cf.ContractNameListSelect(Session["whCode"].ToString())
                                             select new SelectListItem()
                                             {
                                                 Text = r,     //text
                                                 Value = r
                                             };

            ViewData["ContractNameOutSelect"] = from r in cf.ContractFormOutListSelect(Session["whCode"].ToString())
                                                select new SelectListItem()
                                                {
                                                    Text = r,     //text
                                                    Value = r
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
            entity.ClientName = Request["client_name"].Trim();
            entity.ClientCode = Request["client_code"].Trim();
            entity.ClientCodeOrderBy = Request["ClientCodeOrderBy"].Trim();
            int total = 0;
            List<WCF.RootService.WhClientResult> list = cf.WhClientList(entity, out total).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Id", "操作");
            fieldsName.Add("WhCode", "仓库");
            fieldsName.Add("ClientName", "客户名");
            fieldsName.Add("ClientCode", "客户Code");
            fieldsName.Add("Status", "状态");
            fieldsName.Add("WarnCBM", "警戒立方");
            fieldsName.Add("ZoneName", "默认区域");
            fieldsName.Add("Passageway", "绿通名称");
            fieldsName.Add("ZoneId", "区域ID");
            fieldsName.Add("ClientType", "客户类型");
            fieldsName.Add("NightTime", "夜班区间");
            fieldsName.Add("ContractName", "收货合同名称");
            fieldsName.Add("ContractNameOut", "出货合同名称");

            return Content(EIP.EipListJson(list, total, fieldsName, "Id:150,Status:100,WarnCBM:70,ZoneName:80,Passageway:100,ClientType:100,default:120"));
        }

        //新增客户
        [HttpGet]
        public ActionResult AddClient()
        {
            WCF.RootService.WhClient entity = new WCF.RootService.WhClient();

            entity.WhCode = Session["whCode"].ToString();
            entity.ClientName = Request["txt_clientName"].Trim();
            entity.ClientCode = Request["txt_clientCode"].Trim();
            entity.Status = "Active";
            entity.CreateUser = Session["userName"].ToString();
            entity.CreateDate = DateTime.Now;
            entity.ClientType = Request["txt_WhClientType"].Trim();
            entity.Passageway = "绿色通道";
            entity.ReleaseRule = 1;//1 按默认流程释放 2开启优先拣货区 3关闭优先

            if (Request["txt_zone"] == "")
            {
                entity.ZoneId = 0;
            }
            else
            {
                entity.ZoneId = Convert.ToInt32(Request["txt_zone"]);
            }
            if (Request["txt_warnCBM"] == "")
            {
                entity.WarnCBM = 0;
            }
            else
            {
                entity.WarnCBM = Convert.ToDecimal(Request["txt_warnCBM"]);
            }

            WCF.RootService.WhClient result = cf.WhClientAdd(entity);
            if (result != null)
            {
                return Helper.RedirectAjax("ok", "添加成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "对不起，添加失败，客户Code已存在！", null, "");
            }
        }

        //修改客户
        [HttpGet]
        public ActionResult WhClientEdit()
        {
            WCF.RootService.WhClient entity = new WCF.RootService.WhClient();
            entity.Id = Convert.ToInt32(Request["Id"]);
            entity.WhCode = Request["WhCode"];
            entity.ClientName = Request["ClientName"].Trim();
            entity.ClientCode = Request["ClientCode"].Trim();
            entity.Status = Request["Status"];
            entity.ClientType = Request["edit_WhClientType"].Trim();

            entity.Passageway = Request["edit_Passageway"].Trim();

            if (string.IsNullOrEmpty(Request["ZoneId"]))
            {
                entity.ZoneId = 0;
            }
            else
            {
                entity.ZoneId = Convert.ToInt32(Request["ZoneId"]);
            }

            if (Request["edit_warnCBM"] == "")
            {
                entity.WarnCBM = 0;
            }
            else
            {
                entity.WarnCBM = Convert.ToDecimal(Request["edit_warnCBM"]);
            }

            entity.NightTime = Request["edit_NightTime"].Trim();
            entity.ContractName = Request["edit_ContractName"].Trim();
            entity.ContractNameOut = Request["edit_ContractNameOut"].Trim();

            entity.UpdateUser = Session["userName"].ToString();
            entity.UpdateDate = DateTime.Now;
            int result = cf.WhClientEdit(entity, new string[] { "ClientCode", "ClientName", "WarnCBM", "Status", "ZoneId", "UpdateUser", "UpdateDate", "ClientType", "Passageway", "NightTime", "ContractName", "ContractNameOut" });
            if (result > 0)
            {
                return Helper.RedirectAjax("ok", "信息修改成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "对不起，修改失败，客户Code已存在！", null, "");
            }
        }

        //修改客户默认库区
        [HttpGet]
        public ActionResult WhClientEdit1()
        {
            WCF.RootService.WhClient entity = new WCF.RootService.WhClient();
            entity.Id = Convert.ToInt32(Request["Id"]);
            entity.WhCode = Request["WhCode"];

            entity.ZoneId = Convert.ToInt32(Request["ZoneId"]);
            entity.UpdateUser = Session["userName"].ToString();
            entity.UpdateDate = DateTime.Now;
            int result = cf.WhClientEdit(entity, new string[] { "ZoneId", "UpdateUser", "UpdateDate" });
            if (result > 0)
            {
                return Helper.RedirectAjax("ok", "修改成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "对不起，修改失败！", null, "");
            }
        }

        //根据当前客户查询出未选择的货代
        [HttpGet]
        public ActionResult WhAgentUnselected()
        {
            WCF.RootService.WhAgentSearch entity = new WCF.RootService.WhAgentSearch();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.WhCode = Session["whCode"].ToString();
            entity.ClientId = Convert.ToInt32(Request["Id"]);
            entity.AgentName = Request["AgentName"];
            entity.AgentCode = Request["AgentCode"];
            entity.AgentType = Request["AgentType"];
            int total = 0;
            WCF.RootService.WhAgentResult[] list = cf.WhAgentUnselected(entity, out total);

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Id", "Id");
            fieldsName.Add("WhCode", "仓库");
            fieldsName.Add("AgentName", "代理名");
            fieldsName.Add("AgentCode", "代理Code");
            fieldsName.Add("AgentType", "代理类型");

            return Content(EIP.EipListJson(list.ToList(), total, fieldsName, "WhCode:60,default:90"));
        }

        //根据当前客户查询出已选择的货代
        [HttpGet]
        public ActionResult WhAgentSelected()
        {
            WCF.RootService.WhAgentSearch entity = new WCF.RootService.WhAgentSearch();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.WhCode = Session["whCode"].ToString();
            entity.ClientId = Convert.ToInt32(Request["Id"]);
            int total = 0;
            WCF.RootService.WhAgentResult[] list = cf.WhAgentSelected(entity, out total);

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Action", "操作");
            fieldsName.Add("Id", "Id");
            fieldsName.Add("WhCode", "仓库");
            fieldsName.Add("AgentName", "代理名");
            fieldsName.Add("AgentCode", "代理Code");
            fieldsName.Add("AgentType", "代理类型");

            return Content(EIP.EipListJson(list.ToList(), total, fieldsName, "Action:60,default:90"));
        }


        //添加客户代理关系
        [HttpPost]
        public ActionResult WhAgentWhClientListAdd()
        {
            string[] Pow_Id = Request.Form.GetValues("chx_Pos");

            string Hid_PId = Request["Hid_PId"];            //当前选择的客户ID

            List<WCF.RootService.R_WhClient_WhAgent> list = new List<WCF.RootService.R_WhClient_WhAgent>();
            for (int i = 0; i < Pow_Id.Length; i++)
            {
                int AgentId = Convert.ToInt32(Pow_Id[i].Split('-')[0]);
                string AgentType = Pow_Id[i].Split('-')[1];

                WCF.RootService.R_WhClient_WhAgent entity = new WCF.RootService.R_WhClient_WhAgent();
                entity.ClientId = Convert.ToInt32(Hid_PId);
                entity.AgentId = AgentId;
                entity.AgentType = AgentType;
                entity.CreateUser = Session["userName"].ToString();
                entity.CreateDate = DateTime.Now;
                list.Add(entity);
            }

            int result = cf.WhAgentWhClientListAdd(list.ToArray());
            if (result > 0)
            {
                return Helper.RedirectAjax("ok", "添加成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "对不起，添加失败！", null, "");
            }
        }

        //删除客户的某个代理
        [HttpGet]
        public ActionResult WhAgentWhClientDelByAgentClientId()
        {
            int id = Convert.ToInt32(Request["Id"]);
            int Agent_Id = Convert.ToInt32(Request["Agent_Id"]);
            WCF.RootService.R_WhClient_WhAgent entity = new WCF.RootService.R_WhClient_WhAgent();
            entity.ClientId = id;
            entity.AgentId = Agent_Id;
            int result = cf.WhAgentWhClientDel(entity);
            if (result > 0)
            {
                return Helper.RedirectAjax("ok", "取消成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "对不起，取消失败！", null, "");
            }
        }


        //客户异常原因查询列表
        [HttpGet]
        public ActionResult HoldMasterListByClient()
        {
            WCF.RootService.HoldMasterSearch entity = new WCF.RootService.HoldMasterSearch();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.WhCode = Session["whCode"].ToString();
            entity.ClientId = Convert.ToInt32(Request["ClientId"]);
            int total = 0;
            WCF.RootService.HoldMaster[] list = cf.HoldMasterListByClient(entity, out total);

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Id", "操作");
            fieldsName.Add("WhCode", "仓库");
            fieldsName.Add("ClientId", "客户ID");
            fieldsName.Add("ClientCode", "客户Code");
            fieldsName.Add("ReasonType", "异常类型");
            fieldsName.Add("HoldReason", "异常原因");
            fieldsName.Add("Sequence", "显示顺序");

            return Content(EIP.EipListJson(list.ToList(), total, fieldsName, "Action:60,HoldReason:150,Sequence:120,default:90"));
        }


        //新增客户异常原因
        [HttpPost]
        public ActionResult HoldMasterAdd()
        {
            WCF.RootService.HoldMaster entity = new WCF.RootService.HoldMaster();
            entity.WhCode = Session["whCode"].ToString();
            entity.ClientId = Convert.ToInt32(Request["ClientId"]);
            entity.ClientCode = Request["clientCode"].Trim();
            entity.HoldReason = Request["txt_holdReason"].Trim();
            entity.CreateUser = Session["userName"].ToString();
            entity.CreateDate = DateTime.Now;
            entity.ReasonType = Request["txt_holdReasonType"];

            WCF.RootService.HoldMaster result = cf.HoldMasterAdd(entity);
            if (result != null)
            {
                return Helper.RedirectAjax("ok", "添加成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "对不起，添加失败！", null, "");
            }
        }


        //客户异常原因修改
        [HttpGet]
        public ActionResult HoldMasterEdit()
        {
            WCF.RootService.HoldMaster entity = new WCF.RootService.HoldMaster();
            entity.Id = Convert.ToInt32(Request["id"]);
            entity.HoldReason = Request["holdReason"].Trim();

            if (Request["sequence"] == "")
            {
                entity.Sequence = 0;
            }
            else
            {
                entity.Sequence = Convert.ToInt32(Request["sequence"]);
            }

            entity.UpdateUser = Session["userName"].ToString();
            entity.UpdateDate = DateTime.Now;
            int result = cf.HoldMasterEdit(entity, new string[] { "HoldReason", "Sequence", "UpdateUser", "UpdateDate" });
            if (result > 0)
            {
                return Helper.RedirectAjax("ok", "修改成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "对不起，修改失败！", null, "");
            }
        }

        //删除客户异常原因
        [HttpGet]
        public ActionResult HoldMasterDelById()
        {
            int id = Convert.ToInt32(Request["Id"]);
            int result = cf.HoldMasterDelById(id);
            if (result > 0)
            {
                return Helper.RedirectAjax("ok", "删除成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "删除失败！", null, "");
            }
        }


        //---------------------------------  客户流程管理
        //根据当前客户查询出未选择的流程
        [HttpGet]
        public ActionResult FlowNameUnselected()
        {
            WCF.RootService.FlowHeadSearch entity = new WCF.RootService.FlowHeadSearch();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.WhCode = Session["whCode"].ToString();
            entity.ClientId = Convert.ToInt32(Request["Id"]);
            entity.Type = Request["Type"];
            entity.FlowName = Request["FlowName"];
            int total = 0;
            WCF.RootService.FlowHeadResult[] list = cf.FlowNameUnselected(entity, out total);

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Id", "Id");
            fieldsName.Add("FlowName", "流程名");
            fieldsName.Add("Remark", "流程备注");

            return Content(EIP.EipListJson(list.ToList(), total, fieldsName, "default:140"));
        }

        //根据当前客户查询出已选择的流程
        [HttpGet]
        public ActionResult FlowNameSelected()
        {
            WCF.RootService.FlowHeadSearch entity = new WCF.RootService.FlowHeadSearch();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.WhCode = Session["whCode"].ToString();
            entity.ClientId = Convert.ToInt32(Request["Id"]);
            entity.Type = Request["Type"];
            int total = 0;
            WCF.RootService.FlowHeadResult[] list = cf.FlowNameSelected(entity, out total);

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Id", "操作");
            fieldsName.Add("FlowName", "流程名");
            fieldsName.Add("Remark", "流程备注");

            return Content(EIP.EipListJson(list.ToList(), total, fieldsName, "Id:60,default:140"));
        }

        //新增客户流程关系
        [HttpPost]
        public ActionResult AddFlowRule()
        {
            string[] Pow_Id = Request.Form.GetValues("chx_Pos");
            int Hid_PId = Convert.ToInt32(Request["Hid_PId"]);            //当前选择的客户ID

            List<WCF.RootService.R_Client_FlowRule> list = new List<WCF.RootService.R_Client_FlowRule>();
            for (int i = 0; i < Pow_Id.Length; i++)
            {
                WCF.RootService.R_Client_FlowRule entity = new WCF.RootService.R_Client_FlowRule();
                entity.WhCode = Session["whCode"].ToString();
                entity.ClientId = Hid_PId;
                entity.BusinessFlowGroupId = Convert.ToInt32(Pow_Id[i].ToString());
                entity.Type = Request["Type"];
                list.Add(entity);
            }

            int result = cf.AddFlowRule(list.ToArray());
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
        public ActionResult ClientFlowRuleDel()
        {
            int id = Convert.ToInt32(Request["Id"].ToString());
            int result = cf.ClientFlowRuleDel(id);
            if (result > 0)
            {
                return Helper.RedirectAjax("ok", "取消成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "对不起，取消失败！", null, "");
            }
        }


        //修改夜班区间
        [HttpPost]
        public ActionResult EditDetailNightTime()
        {
            string[] clientIdList = Request.Form.GetValues("clientId");
            string nightTime = Request["nithgTime"].Trim();

            for (int i = 0; i < clientIdList.Length; i++)
            {
                WCF.RootService.WhClient entity = new WCF.RootService.WhClient();
                entity.Id = Convert.ToInt32(clientIdList[i]);

                entity.NightTime = nightTime;
                cf.WhClientEdit(entity, new string[] { "NightTime" });
            }

            return Helper.RedirectAjax("ok", "修改成功！", null, "");

        }


        //修改收货合同
        [HttpPost]
        public ActionResult EditDetailContractName()
        {
            string[] clientIdList = Request.Form.GetValues("clientId");
            string contractName = Request["contractName"].Trim();

            for (int i = 0; i < clientIdList.Length; i++)
            {
                WCF.RootService.WhClient entity = new WCF.RootService.WhClient();
                entity.Id = Convert.ToInt32(clientIdList[i]);

                entity.ContractName = contractName;
                cf.WhClientEdit(entity, new string[] { "ContractName" });
            }

            return Helper.RedirectAjax("ok", "修改成功！", null, "");

        }

        //修改出货合同
        [HttpPost]
        public ActionResult EditDetailContractNameOut()
        {
            string[] clientIdList = Request.Form.GetValues("clientId");
            string contractName = Request["contractName"].Trim();

            for (int i = 0; i < clientIdList.Length; i++)
            {
                WCF.RootService.WhClient entity = new WCF.RootService.WhClient();
                entity.Id = Convert.ToInt32(clientIdList[i]);

                entity.ContractNameOut = contractName;
                cf.WhClientEdit(entity, new string[] { "ContractNameOut" });
            }

            return Helper.RedirectAjax("ok", "修改成功！", null, "");

        }


        [HttpGet]
        public ActionResult ClientEditUnActive()
        {
            WCF.RootService.WhClient entity = new WCF.RootService.WhClient();
            entity.Id = Convert.ToInt32(Request["Id"]);
            entity.Status = "UnActive";
            entity.UpdateUser = Session["userName"].ToString();
            entity.UpdateDate = DateTime.Now;

            int result = cf.WhClientEdit(entity, new string[] { "Status", "UpdateUser", "UpdateDate" });
            if (result > 0)
            {
                return Helper.RedirectAjax("ok", "客户失效成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "对不起，操作失败！", null, "");
            }
        }

        [HttpGet]
        public ActionResult ClientEditActive()
        {
            WCF.RootService.WhClient entity = new WCF.RootService.WhClient();
            entity.Id = Convert.ToInt32(Request["Id"]);
            entity.Status = "Active";
            entity.UpdateUser = Session["userName"].ToString();
            entity.UpdateDate = DateTime.Now;

            int result = cf.WhClientEdit(entity, new string[] { "Status", "UpdateUser", "UpdateDate" });
            if (result > 0)
            {
                return Helper.RedirectAjax("ok", "重新启用客户成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "对不起，操作失败！", null, "");
            }
        }

    }
}

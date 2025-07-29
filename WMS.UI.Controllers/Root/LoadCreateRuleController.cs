using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace WMS.UI.Controllers
{
    public class LoadCreateRuleController : Controller
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
            WCF.RootService.LoadCreateRuleSearch entity = new WCF.RootService.LoadCreateRuleSearch();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.WhCode = Session["whCode"].ToString();
            entity.RuleName = Request["RuleName"].Trim();
            entity.Status = Request["Status"];
            int total = 0;
            List<WCF.RootService.LoadCreateRuleResult> list = cf.GetLoadCreateRuleList(entity, out total).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Id", "操作");
            fieldsName.Add("RuleName", "规则名");
            fieldsName.Add("Description", "规则备注");
            fieldsName.Add("OrderQty", "上限订单数");
            fieldsName.Add("Qty", "上限总个数");
            fieldsName.Add("ShipMode", "出库方式");
            fieldsName.Add("Status", "状态");
            fieldsName.Add("CreateUser", "创建人");
            fieldsName.Add("CreateDate", "创建时间");

            return Content(EIP.EipListJson(list, total, fieldsName, "Id:90,RuleName:150,Description:200,CreateDate:130,default:80"));
        }

        //新增生成规则
        [HttpGet]
        public ActionResult LoadCreateRuleAdd()
        {
            WCF.RootService.LoadCreateRule entity = new WCF.RootService.LoadCreateRule();
            entity.WhCode = Session["whCode"].ToString();
            entity.RuleName = Request["txt_RuleName"].Trim();
            entity.Description = Request["txt_Description"].Trim();
            entity.Status = Request["txt_Status"].Trim();
            entity.ShipMode = Request["txt_shipMode"].Trim();
            if (Request["txt_OrderQty"] == "")
            {
                entity.OrderQty = 0;
            }
            else
            {
                entity.OrderQty = Convert.ToInt32(Request["txt_OrderQty"]);
            }
            if (Request["txt_Qty"] == "")
            {
                entity.Qty = 0;
            }
            else
            {
                entity.Qty = Convert.ToInt32(Request["txt_Qty"]);
            }

            entity.CreateUser = Session["userName"].ToString();

            WCF.RootService.LoadCreateRule result = cf.LoadCreateRuleAdd(entity);
            if (result != null)
            {
                return Helper.RedirectAjax("ok", "添加成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "对不起，添加失败，规则名已存在！", null, "");
            }
        }

        //修改规则
        [HttpGet]
        public ActionResult LoadCreateRuleEdit()
        {
            WCF.RootService.LoadCreateRule entity = new WCF.RootService.LoadCreateRule();
            entity.Id = Convert.ToInt32(Request["Id"]);
            entity.Description = Request["edit_Description"].Trim();
            if (Request["edit_OrderQty"] == "")
            {
                entity.OrderQty = 0;
            }
            else
            {
                entity.OrderQty = Convert.ToInt32(Request["edit_OrderQty"]);
            }
            if (Request["edit_Qty"] == "")
            {
                entity.Qty = 0;
            }
            else
            {
                entity.Qty = Convert.ToInt32(Request["edit_Qty"]);
            }

            entity.RuleName = Request["edit_RuleName"];
            entity.Status = Request["edit_Status"];
            entity.UpdateUser = Session["userName"].ToString();
            int result = cf.LoadCreateRuleEdit(entity);
            if (result > 0)
            {
                return Helper.RedirectAjax("ok", "信息修改成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "对不起，修改失败！", null, "");
            }
        }

        [HttpGet]
        public ActionResult LoadCreateFlowNameUnselected()
        {
            WCF.RootService.LoadCreateRuleSearch entity = new WCF.RootService.LoadCreateRuleSearch();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.WhCode = Session["whCode"].ToString();
            entity.Id = Convert.ToInt32(Request["Id"]);
            entity.FlowName = Request["outFlowName"].Trim();

            int total = 0;
            WCF.RootService.BusinessFlowGroupResult[] list = cf.LoadCreateFlowNameUnselected(entity, out total);

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Id", "Id");
            fieldsName.Add("FlowName", "流程名");
            fieldsName.Add("Remark", "流程备注");

            return Content(EIP.EipListJson(list.ToList(), total, fieldsName, "default:140"));
        }

        [HttpGet]
        public ActionResult LoadCreateFlowNameSelected()
        {
            WCF.RootService.LoadCreateRuleSearch entity = new WCF.RootService.LoadCreateRuleSearch();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.Id = Convert.ToInt32(Request["Id"]);
            int total = 0;
            WCF.RootService.BusinessFlowGroupResult[] list = cf.LoadCreateFlowNameSelected(entity, out total);

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Id", "操作");
            fieldsName.Add("FlowName", "流程名");
            fieldsName.Add("Remark", "流程备注");

            return Content(EIP.EipListJson(list.ToList(), total, fieldsName, "Id:60,default:140"));
        }


        //新增客户流程关系
        [HttpPost]
        public ActionResult R_LoadRule_FlowHeadListAdd()
        {
            string[] Pow_Id = Request.Form.GetValues("chx_Pos");
            int RuleId = Convert.ToInt32(Request["Hid_PId"]);            //当前选择的流程ID

            List<WCF.RootService.R_LoadRule_FlowHead> list = new List<WCF.RootService.R_LoadRule_FlowHead>();
            for (int i = 0; i < Pow_Id.Length; i++)
            {
                WCF.RootService.R_LoadRule_FlowHead entity = new WCF.RootService.R_LoadRule_FlowHead();
                entity.RuleId = RuleId;
                entity.FlowHeadId = Convert.ToInt32(Pow_Id[i].ToString());
                entity.CreateUser = Session["userName"].ToString();
                list.Add(entity);
            }

            int result = cf.R_LoadRule_FlowHeadListAdd(list.ToArray());
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
        public ActionResult R_LoadRule_FlowHeadDel()
        {
            int id = Convert.ToInt32(Request["Id"].ToString());
            int result = cf.R_LoadRule_FlowHeadDel(id);
            if (result > 0)
            {
                return Helper.RedirectAjax("ok", "取消成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "对不起，取消失败！", null, "");
            }
        }


        //自动生成Load
        [HttpPost]
        public void BeginLoadCreate()
        {
            WCF.RootService.LoadCreateRuleInsert entity = new WCF.RootService.LoadCreateRuleInsert();
            try
            {
                if (Request["BeginDate1"].Trim() != "")
                {
                    entity.BeginDate = Convert.ToDateTime(Request["BeginDate1"] + " " + Request["BeginDate2"] + ":00:00");

                    int date2 = Convert.ToInt32(Request["EndDate2"]);
                    if (date2 >= 24)
                    {
                        entity.EndDate = Convert.ToDateTime(Request["EndDate1"] + " " + "00:00:00").AddDays(1);
                    }
                    else
                    {
                        entity.EndDate = Convert.ToDateTime(Request["EndDate1"] + " " + Request["EndDate2"] + ":00:00");
                    }

                }
                else
                {
                    entity.BeginDate = null;
                    entity.EndDate = null;
                }
            }
            catch
            {
                Response.Write("日期格式有误，请检查！");
                return;
            }
            if (entity.EndDate <= entity.BeginDate)
            {
                Response.Write("结束时间不能小于等于开始时间！");
                return;
            }

            string[] flowHeadId = Request.Form.GetValues("flowHeadId");
            string[] client = Request.Form.GetValues("client");
            string[] orderSource = Request.Form.GetValues("orderSource");
            string[] orderType = Request.Form.GetValues("orderType");

            for (int i = 0; i < flowHeadId.Length; i++)
            {
                entity.WhCode = Session["whCode"].ToString();
                entity.ClientCode = client[i];
                entity.UserName = Session["userName"].ToString();
                entity.LoadCount = 5;
                entity.RuleId = Convert.ToInt32(flowHeadId[i]);
                entity.OrderSource = orderSource[i];
                entity.OrderType = orderType[i];

                string result = cf.BeginLoadCreate(entity);

                Response.Write(result);
            }
        }

        [HttpGet]
        public ActionResult GetOrderQtyList()
        {
            WCF.RootService.LoadCreateRuleSearch entity = new WCF.RootService.LoadCreateRuleSearch();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.WhCode = Session["whCode"].ToString();
            entity.OrderSource = Request["txt_orderSourse"];
            entity.ClientCode = Request["clientCode"];

            try
            {
                if (Request["BeginDate1"].Trim() != "")
                {
                    entity.BeginDate = Convert.ToDateTime(Request["BeginDate1"] + " " + Request["BeginDate2"] + ":00:00");

                    int date2 = Convert.ToInt32(Request["EndDate2"]);
                    if (date2 >= 24)
                    {
                        entity.EndDate = Convert.ToDateTime(Request["EndDate1"] + " " + "00:00:00").AddDays(1);
                    }
                    else
                    {
                        entity.EndDate = Convert.ToDateTime(Request["EndDate1"] + " " + Request["EndDate2"] + ":00:00");
                    }

                }
                else
                {
                    entity.BeginDate = null;
                    entity.EndDate = null;
                }
            }
            catch
            {
                return Helper.RedirectAjax("err", "日期格式有误请重新选择！", null, "");
            }
            if (entity.EndDate <= entity.BeginDate)
            {
                return Helper.RedirectAjax("err", "结束时间不能小于等于开始时间！", null, "");
            }

            int total = 0;
            List<WCF.RootService.LoadCreateRuleResult> list = cf.GetOrderQtyList(entity, out total).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Id", "操作");
            fieldsName.Add("RuleName", "流程名");
            fieldsName.Add("ClientCode", "客户名");
            fieldsName.Add("OrderQty", "订单总数");
            fieldsName.Add("OrderSource", "订单来源");
            fieldsName.Add("OrderType", "订单类型");

            return Content(EIP.EipListJson(list, total, fieldsName, "Id:45,RuleName:150,ClientCode:125,default:90"));
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace WMS.UI.Controllers
{
    public class C_WorkloadAccountController : Controller
    {
        WCF.RootService.RootServiceClient cf = new WCF.RootService.RootServiceClient();
        Helper Helper = new Helper();
        EIP.EIP EIP = new EIP.EIP();

        [DefaultRequest]
        public ActionResult Index()
        {

            ViewData["WorktTypeList"] = from r in cf.WorkTypeList(Session["whCode"].ToString()).Where(u => u.WorkType != "")
                                        select new SelectListItem()
                                        {
                                            Text = r.WorkType,     //text
                                            Value = r.WorkType.ToString()
                                        };

            ViewData["WorktTypeList1"] = from r in cf.WorkTypeList(Session["whCode"].ToString()).Where(u => u.WorkType != "理货员")
                                         select new SelectListItem()
                                         {
                                             Text = r.WorkType,     //text
                                             Value = r.WorkType.ToString()
                                         };

            return View();
        }

        [DefaultRequest]
        public ActionResult Index1()
        {
            ViewData["WorktTypeList"] = from r in cf.WorkTypeList(Session["whCode"].ToString()).Where(u => u.WorkType != "")
                                        select new SelectListItem()
                                        {
                                            Text = r.WorkType,     //text
                                            Value = r.WorkType.ToString()
                                        };

            ViewData["WorktTypeList1"] = from r in cf.WorkTypeList(Session["whCode"].ToString()).Where(u => u.WorkType != "理货员")
                                         select new SelectListItem()
                                         {
                                             Text = r.WorkType,     //text
                                             Value = r.WorkType.ToString()
                                         };

            return View();
        }

        public ActionResult List()
        {
            WCF.RootService.WorkloadAccountSearch entity = new WCF.RootService.WorkloadAccountSearch();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.WhCode = Session["whCode"].ToString();
            entity.ReceiptId = Request["receipt_id"].Trim();
            entity.WorkType = Request["WorktType"].Trim();
            entity.UserCode = Request["UserCode"].Trim();

            if (Request["BeginCreateDate"] != "")
            {
                entity.BeginCreateDate = Convert.ToDateTime(Request["BeginCreateDate"]);
            }
            else
            {
                entity.BeginCreateDate = null;
            }
            if (Request["EndCreateDate"] != "")
            {
                entity.EndCreateDate = Convert.ToDateTime(Request["EndCreateDate"]).AddDays(1);
            }
            else
            {
                entity.EndCreateDate = null;
            }

            int total = 0;
            List<WCF.RootService.WorkloadAccountResult> list = cf.InWorkloadAccountList(entity, out total).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Action", "操作");
            fieldsName.Add("ReceiptId", "收货批次号");
            fieldsName.Add("ReceiptDate", "收货时间");
            fieldsName.Add("UserCode", "工号");
            fieldsName.Add("UserNameCN", "员工姓名");
            fieldsName.Add("WorkType", "工作类型");
            fieldsName.Add("cbm", "立方");

            return Content(EIP.EipListJson(list, total, fieldsName, "Action:60,ReceiptId:120,ReceiptDate:120,cbm:60,default:90", null));
        }

        //修改工人工作量
        [HttpPost]
        public ActionResult EditDetailUserCode()
        {
            string[] userCode = Request.Form.GetValues("userCode");
            string[] receiptId = Request.Form.GetValues("receiptId");
            string[] workType = Request.Form.GetValues("workType");
            string editUserCode = Request["edit_userCode"];

            List<WCF.RootService.WorkloadAccountResult> list = new List<WCF.RootService.WorkloadAccountResult>();
            for (int i = 0; i < userCode.Length; i++)
            {
                WCF.RootService.WorkloadAccountResult entity = new WCF.RootService.WorkloadAccountResult();
                entity.UserCode = userCode[i];
                entity.ReceiptId = receiptId[i];
                entity.WorkType = workType[i];
                entity.WhCode = Session["whCode"].ToString();
                entity.UpdateUser = Session["userName"].ToString();
                list.Add(entity);
            }

            string result = cf.EditWorkloadAccount(list.ToArray(), editUserCode);
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "批量修改成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }
        }

        //批量修改工人工作量
        [HttpPost]
        public ActionResult EditDetailUserCodeList()
        {
            string[] userCode = Request.Form.GetValues("userCode");
            string[] receiptId = Request.Form.GetValues("receiptId");
            string[] workType = Request.Form.GetValues("workType");
            string editUserCode = Request["edit_userCode"];

            List<WCF.RootService.WorkloadAccountResult> list = new List<WCF.RootService.WorkloadAccountResult>();
            for (int i = 0; i < userCode.Length; i++)
            {
                WCF.RootService.WorkloadAccountResult entity = new WCF.RootService.WorkloadAccountResult();
                entity.UserCode = userCode[i];
                entity.ReceiptId = receiptId[i];
                entity.WorkType = workType[i];
                entity.WhCode = Session["whCode"].ToString();
                entity.UpdateUser = Session["userName"].ToString();
                list.Add(entity);
            }

            string result = cf.EditWorkloadAccountList(list.ToArray(), editUserCode);
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "批量修改成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }
        }

        [HttpGet]
        public ActionResult EditDetail()
        {
            string editUserCode = Request["edit_userCode"];
            string userCode = Request["userCode"];
            string receiptId = Request["receiptId"];
            string workType = Request["workType"];
            string cbm = Request["edit_cbm"];

            List<WCF.RootService.WorkloadAccountResult> list = new List<WCF.RootService.WorkloadAccountResult>();

            WCF.RootService.WorkloadAccountResult entity = new WCF.RootService.WorkloadAccountResult();
            entity.UserCode = userCode;
            entity.ReceiptId = receiptId;
            entity.WorkType = workType;

            if (cbm != "undefined")
            {
                entity.cbm = Convert.ToDecimal(cbm);
            }

            entity.WhCode = Session["whCode"].ToString();
            entity.UpdateUser = Session["userName"].ToString();
            list.Add(entity);

            string result = "";
            if (workType != "理货员")
            {
                result = cf.EditWorkloadAccountList(list.ToArray(), editUserCode);
            }
            else
            {
                result = cf.EditWorkloadAccount(list.ToArray(), editUserCode);
            }

            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "修改成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }

        }

        //批量删除工人工作量
        [HttpPost]
        public ActionResult DelDetailUserCode()
        {
            string[] userCode = Request.Form.GetValues("userCode");
            string[] receiptId = Request.Form.GetValues("receiptId");
            string[] workType = Request.Form.GetValues("workType");
            string editUserCode = "";

            List<WCF.RootService.WorkloadAccountResult> list = new List<WCF.RootService.WorkloadAccountResult>();
            for (int i = 0; i < userCode.Length; i++)
            {
                WCF.RootService.WorkloadAccountResult entity = new WCF.RootService.WorkloadAccountResult();
                entity.UserCode = userCode[i];
                entity.ReceiptId = receiptId[i];
                entity.WorkType = workType[i];
                entity.WhCode = Session["whCode"].ToString();
                entity.UpdateUser = Session["userName"].ToString();
                list.Add(entity);
            }

            string result = cf.DelWorkloadAccount(list.ToArray(), editUserCode);
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "批量删除成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }
        }

        [HttpGet]
        public ActionResult DelDetail()
        {
            string editUserCode = "";
            string userCode = Request["userCode"];
            string receiptId = Request["receiptId"];
            string workType = Request["workType"];

            List<WCF.RootService.WorkloadAccountResult> list = new List<WCF.RootService.WorkloadAccountResult>();

            WCF.RootService.WorkloadAccountResult entity = new WCF.RootService.WorkloadAccountResult();
            entity.UserCode = userCode;
            entity.ReceiptId = receiptId;
            entity.WorkType = workType;
            entity.WhCode = Session["whCode"].ToString();
            entity.UpdateUser = Session["userName"].ToString();
            list.Add(entity);

            string result = cf.DelWorkloadAccount(list.ToArray(), editUserCode);
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "删除成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }

        }

        //新增工人工作量
        [HttpGet]
        public ActionResult AddDetail()
        {
            string editUserCode = "";
            string userCode = Request["add_UserCode"];
            string receiptId = Request["add_receipt_id"];
            string workType = Request["add_WorktType"];

            List<WCF.RootService.WorkloadAccountResult> list = new List<WCF.RootService.WorkloadAccountResult>();

            WCF.RootService.WorkloadAccountResult entity = new WCF.RootService.WorkloadAccountResult();
            entity.UserCode = userCode;
            entity.ReceiptId = receiptId;
            entity.WorkType = workType;
            entity.WhCode = Session["whCode"].ToString();
            entity.UpdateUser = Session["userName"].ToString();
            list.Add(entity);

            string result = cf.AddWorkloadAccount(list.ToArray(), editUserCode);
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "添加成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }

        }



        //------------------------出货工作量修改

        public ActionResult List1()
        {
            WCF.RootService.WorkloadAccountSearch entity = new WCF.RootService.WorkloadAccountSearch();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.WhCode = Session["whCode"].ToString();
            entity.LoadId = Request["receipt_id"].Trim();
            entity.WorkType = Request["WorktType"].Trim();
            entity.UserCode = Request["UserCode"].Trim();

            if (Request["BeginCreateDate"] != "")
            {
                entity.BeginCreateDate = Convert.ToDateTime(Request["BeginCreateDate"]);
            }
            else
            {
                entity.BeginCreateDate = null;
            }
            if (Request["EndCreateDate"] != "")
            {
                entity.EndCreateDate = Convert.ToDateTime(Request["EndCreateDate"]).AddDays(1);
            }
            else
            {
                entity.EndCreateDate = null;
            }

            int total = 0;
            List<WCF.RootService.WorkloadAccountResult> list = cf.OutWorkloadAccountList(entity, out total).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Action", "操作");
            fieldsName.Add("LoadId", "Load号");
            fieldsName.Add("ReceiptDate", "出货时间");
            fieldsName.Add("UserCode", "工号");
            fieldsName.Add("UserNameCN", "员工姓名");
            fieldsName.Add("WorkType", "工作类型");
            fieldsName.Add("cbm", "立方");
            fieldsName.Add("baQty", "把数");

            return Content(EIP.EipListJson(list, total, fieldsName, "Action:60,LoadId:120,ReceiptDate:120,cbm:60,default:90", null));
        }

        //修改工人工作量
        [HttpPost]
        public ActionResult EditDetailUserCode1()
        {
            string[] userCode = Request.Form.GetValues("userCode");
            string[] receiptId = Request.Form.GetValues("receiptId");
            string[] workType = Request.Form.GetValues("workType");
            string editUserCode = Request["edit_userCode"];

            List<WCF.RootService.WorkloadAccountResult> list = new List<WCF.RootService.WorkloadAccountResult>();
            for (int i = 0; i < userCode.Length; i++)
            {
                WCF.RootService.WorkloadAccountResult entity = new WCF.RootService.WorkloadAccountResult();
                entity.UserCode = userCode[i];
                entity.LoadId = receiptId[i];
                entity.WorkType = workType[i];
                entity.WhCode = Session["whCode"].ToString();
                entity.UpdateUser = Session["userName"].ToString();
                list.Add(entity);
            }

            string result = cf.EditWorkloadAccount1(list.ToArray(), editUserCode);
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "批量修改成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }
        }

        //批量修改出货工人工作量
        [HttpPost]
        public ActionResult EditDetailUserCode1List()
        {
            string[] userCode = Request.Form.GetValues("userCode");
            string[] receiptId = Request.Form.GetValues("receiptId");
            string[] workType = Request.Form.GetValues("workType");
            string editUserCode = Request["edit_userCode"];

            List<WCF.RootService.WorkloadAccountResult> list = new List<WCF.RootService.WorkloadAccountResult>();
            for (int i = 0; i < userCode.Length; i++)
            {
                WCF.RootService.WorkloadAccountResult entity = new WCF.RootService.WorkloadAccountResult();
                entity.UserCode = userCode[i];
                entity.LoadId = receiptId[i];
                entity.WorkType = workType[i];
                entity.WhCode = Session["whCode"].ToString();
                entity.UpdateUser = Session["userName"].ToString();
                list.Add(entity);
            }

            string result = cf.EditWorkloadAccount1List(list.ToArray(), editUserCode);
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "批量修改成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }
        }

        [HttpGet]
        public ActionResult EditDetail1()
        {
            string editUserCode = Request["edit_userCode"];
            string userCode = Request["userCode"];
            string receiptId = Request["receiptId"];
            string workType = Request["workType"];
            string cbm = Request["edit_cbm"];
            string baqty = Request["edit_baqty"];


            List<WCF.RootService.WorkloadAccountResult> list = new List<WCF.RootService.WorkloadAccountResult>();

            WCF.RootService.WorkloadAccountResult entity = new WCF.RootService.WorkloadAccountResult();
            entity.UserCode = userCode;
            entity.LoadId = receiptId;
            entity.WorkType = workType;
            if (cbm != "undefined")
            {
                entity.cbm = Convert.ToDecimal(cbm);
            }

            if (baqty != "undefined")
            {
                entity.baQty = Convert.ToDecimal(baqty);
            }
            entity.WhCode = Session["whCode"].ToString();
            entity.UpdateUser = Session["userName"].ToString();

            list.Add(entity);

            string result = "";

            if (workType != "理货员")
            {
                result = cf.EditWorkloadAccount1List(list.ToArray(), editUserCode);
            }
            else
            {
                result = cf.EditWorkloadAccount1(list.ToArray(), editUserCode);
            }

            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "修改成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }

        }

        //批量删除工人工作量
        [HttpPost]
        public ActionResult DelDetailUserCode1()
        {
            string[] userCode = Request.Form.GetValues("userCode");
            string[] receiptId = Request.Form.GetValues("receiptId");
            string[] workType = Request.Form.GetValues("workType");
            string editUserCode = "";

            List<WCF.RootService.WorkloadAccountResult> list = new List<WCF.RootService.WorkloadAccountResult>();
            for (int i = 0; i < userCode.Length; i++)
            {
                WCF.RootService.WorkloadAccountResult entity = new WCF.RootService.WorkloadAccountResult();
                entity.UserCode = userCode[i];
                entity.LoadId = receiptId[i];
                entity.WorkType = workType[i];
                entity.WhCode = Session["whCode"].ToString();
                entity.UpdateUser = Session["userName"].ToString();
                list.Add(entity);
            }

            string result = cf.DelWorkloadAccount1(list.ToArray(), editUserCode);
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "批量删除成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }
        }

        [HttpGet]
        public ActionResult DelDetail1()
        {
            string editUserCode = "";
            string userCode = Request["userCode"];
            string receiptId = Request["receiptId"];
            string workType = Request["workType"];

            List<WCF.RootService.WorkloadAccountResult> list = new List<WCF.RootService.WorkloadAccountResult>();

            WCF.RootService.WorkloadAccountResult entity = new WCF.RootService.WorkloadAccountResult();
            entity.UserCode = userCode;
            entity.LoadId = receiptId;
            entity.WorkType = workType;
            entity.WhCode = Session["whCode"].ToString();
            entity.UpdateUser = Session["userName"].ToString();
            list.Add(entity);

            string result = cf.DelWorkloadAccount1(list.ToArray(), editUserCode);
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "删除成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }

        }

        //新增工人工作量
        [HttpGet]
        public ActionResult AddDetail1()
        {
            string editUserCode = "";
            string userCode = Request["add_UserCode"];
            string receiptId = Request["add_receipt_id"];
            string workType = Request["add_WorktType"];

            List<WCF.RootService.WorkloadAccountResult> list = new List<WCF.RootService.WorkloadAccountResult>();

            WCF.RootService.WorkloadAccountResult entity = new WCF.RootService.WorkloadAccountResult();
            entity.UserCode = userCode;
            entity.LoadId = receiptId;
            entity.WorkType = workType;
            entity.WhCode = Session["whCode"].ToString();
            entity.UpdateUser = Session["userName"].ToString();
            list.Add(entity);

            string result = cf.AddWorkloadAccount1(list.ToArray(), editUserCode);
            if (result == "Y")
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

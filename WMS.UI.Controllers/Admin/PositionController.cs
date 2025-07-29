using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using WMS.UI.Controllers.Model;

namespace WMS.UI.Controllers
{
    public class PositionController : Controller
    {
        WCF.AdminService.AdminServiceClient cf = new WCF.AdminService.AdminServiceClient();
        Helper Helper = new Helper();
        EIP.EIP EIP = new EIP.EIP();
        [DefaultRequest]
        public ActionResult Index()
        {
            //List<WCF.AdminService.WhMenuResult> listWhMenu = (List<WCF.AdminService.WhMenuResult>)Session["userMenu"];
            //ViewData["Select"] = from r in listWhMenu
            //               where r.ParentMenuId == 2
            //               select new SelectListItem()
            //               {
            //                   Text = r.MenuNameCN,     //text
            //                   Value = r.Id.ToString()
            //               };


            return View();
            // return View();

        }

        public ActionResult Test()
        {

            TestModel model = new TestModel();

            TryUpdateModel(model);

            if (ModelState.IsValid)
            {
                return Helper.RedirectAjax("ok", "添加成功！", null, "");
            }
            else
            {
                //这里，前端页面不用改，所以我直接利用第一个例子中的前端页面
                return Helper.RedirectAjax("err", Helper.CheckModelErrors(this), null, "");
            }

        }

        public ActionResult vendor(TestModel model)
        {



            WCF.AdminService.WhPositionSearch whPositionSearch = new WCF.AdminService.WhPositionSearch();
            whPositionSearch.pageIndex = 1;
            whPositionSearch.pageSize = 50;
            //whPositionSearch.WhCode = "01";
            //whPositionSearch.PositionName = Request["pos_name"];
            //whPositionSearch.PositionNameCN = Request["pos_nameCN"];
            int total = 0;
            WCF.AdminService.WhPosition[] list = cf.WhPositionList(whPositionSearch, out total);

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Id", "VENDOR_ID");
            fieldsName.Add("PositionName", "供应商");


            return Content(EIP.EipListJson(list.ToList(), total, fieldsName, "PositionName:120,default:200"));


            // return View();

        }
        /// <summary>
        /// 查询所有职位信息
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult List()
        {
            WCF.AdminService.WhPositionSearch whPositionSearch = new WCF.AdminService.WhPositionSearch();
            whPositionSearch.pageIndex = Convert.ToInt32(Request["eip_page"]);
            whPositionSearch.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            whPositionSearch.CompanyId = Convert.ToInt32(Session["whCompany"].ToString());
            whPositionSearch.PositionName = Request["pos_name"];
            whPositionSearch.PositionNameCN = Request["pos_nameCN"];
            int total = 0;
            WCF.AdminService.WhPosition[] list = cf.WhPositionList(whPositionSearch, out total);

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Id", "操作");
            fieldsName.Add("CompanyId", "公司ID");
            fieldsName.Add("PositionName", "职位英文简称");
            fieldsName.Add("PositionNameCN", "职位中文名");
            fieldsName.Add("Status", "状态");
            fieldsName.Add("WorkPowerFlag", "工作台特殊权限");
            fieldsName.Add("CreateUser", "创建人");
            fieldsName.Add("CreateDate", "创建时间");
            fieldsName.Add("UpdateUser", "修改人");
            fieldsName.Add("UpdateDate", "修改时间");

            return Content(EIP.EipListJson(list.ToList(), total, fieldsName, "WhCode:60,CreateDate:130,WorkPowerFlag:130,default:90"));

        }

        /// <summary>
        /// 新增职位
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult WhPositionAdd()
        {
            WCF.AdminService.WhPosition whPosition = new WCF.AdminService.WhPosition();
            whPosition.CompanyId = Convert.ToInt32(Session["whCompany"].ToString());
            whPosition.PositionName = Request["txt_posname"];
            whPosition.PositionNameCN = Request["txt_posnameCN"];

            if (string.IsNullOrEmpty(Request["txt_WorkPowerFlag"]))
            {
                whPosition.WorkPowerFlag = 0;
            }
            else
            {
                whPosition.WorkPowerFlag = Convert.ToInt32(Request["txt_WorkPowerFlag"]);
            }
            whPosition.Status = "Active";
            whPosition.CreateUser = Session["userName"].ToString();
            whPosition.CreateDate = DateTime.Now;

            WCF.AdminService.WhPosition result = cf.WhPositionAdd(whPosition);
            if (result != null)
            {
                return Helper.RedirectAjax("ok", "添加成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "添加失败，职位英文简称已存在！", null, "");
            }
        }

        //修改职位状态
        [HttpGet]
        public ActionResult WhPositionEdit()
        {

            WCF.AdminService.WhPosition entity = new WCF.AdminService.WhPosition();
            entity.Id = Convert.ToInt32(Request["Id"]);
            entity.Status = Request["Status"];
            entity.WorkPowerFlag = Convert.ToInt32(Request["WorkPowerFlag"]);
            entity.UpdateUser = Session["userName"].ToString();
            entity.UpdateDate = DateTime.Now;
            int result = cf.WhPositionEdit(entity, new string[] { "Status", "WorkPowerFlag", "UpdateUser", "UpdateDate" });
            if (result > 0)
            {
                return Helper.RedirectAjax("ok", "信息修改成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "对不起，修改失败！", null, "");
            }
        }

        /// <summary>
        /// 根据当前职位查询出未选择的权限信息
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult WhPowerUnselected()
        {
            WCF.AdminService.WhPowerSearch whPowerSearch = new WCF.AdminService.WhPowerSearch();
            whPowerSearch.pageIndex = Convert.ToInt32(Request["eip_page"]);
            whPowerSearch.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            whPowerSearch.CompanyId = Convert.ToInt32(Session["whCompany"].ToString());
            whPowerSearch.PositionName = Request["PosName"];
            whPowerSearch.PowerType = Request["PowType"];
            whPowerSearch.PowerName = Request["PowName"];
            int total = 0;
            WCF.AdminService.WhPowerResult[] list = cf.WhPowerUnselected(whPowerSearch, out total);

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Id", "Id");
            fieldsName.Add("CompanyId", "公司ID");
            fieldsName.Add("PowerName", "权限名");
            fieldsName.Add("PowerType", "权限类型");
            fieldsName.Add("PowerDescription", "说明");

            return Content(EIP.EipListJson(list.ToList(), total, fieldsName, "WhCode:60,PowerName:140,default:90"));
        }

        /// <summary>
        /// 根据当前职位查询已有权限
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult WhPowerSelected()
        {
            WCF.AdminService.WhPositionWhPowerSearch whPositionWhPowerSearch = new WCF.AdminService.WhPositionWhPowerSearch();
            whPositionWhPowerSearch.pageIndex = Convert.ToInt32(Request["eip_page"]);
            whPositionWhPowerSearch.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            whPositionWhPowerSearch.CompanyId = Convert.ToInt32(Session["whCompany"].ToString());
            whPositionWhPowerSearch.PositionName = Request["PosName"];

            int total = 0;
            List<WCF.AdminService.WhPositionWhPowerResult> list = cf.WhPowerSelected(whPositionWhPowerSearch, out total).ToList();
            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Id", "Id");
            fieldsName.Add("Action", "操作");
            fieldsName.Add("CompanyId", "公司ID");
            fieldsName.Add("PositionName", "职位英文简称");
            fieldsName.Add("PositionNameCN", "职位中文名");
            fieldsName.Add("PowerName", "权限名");
            fieldsName.Add("PowerType", "权限类型");
            fieldsName.Add("PowerDescription", "说明");

            return Content(EIP.EipListJson(list, total, fieldsName, "Action:50,WhCode:60,PowerName:140,default:90"));
        }

        /// <summary>
        /// 添加职位权限关系
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult WhPositionPowerListAdd()
        {
            string[] Pow_Id = Request.Form.GetValues("chx_Pow");

            string Hid_PId = Request["Hid_PId"];            //当前选择的职位ID
            string Hid_PosName = Request["Hid_PosName"];    //当前选择的职位名

            List<WCF.AdminService.WhPositionPower> list = new List<WCF.AdminService.WhPositionPower>();
            for (int i = 0; i < Pow_Id.Length; i++)
            {
                int PId = Convert.ToInt32(Pow_Id[i].Split('-')[0]);
                string PowName = Pow_Id[i].Split('-')[1];

                WCF.AdminService.WhPositionPower whPositionPower = new WCF.AdminService.WhPositionPower();
                whPositionPower.CompanyId = Convert.ToInt32(Session["whCompany"].ToString());
                whPositionPower.PositionId = Convert.ToInt32(Hid_PId);
                whPositionPower.PositionName = Hid_PosName;
                whPositionPower.PowerId = Convert.ToInt32(PId);
                whPositionPower.PowerName = PowName;
                whPositionPower.PowerVal = "";
                whPositionPower.CreateUser = Session["userName"].ToString();
                whPositionPower.CreateDate = DateTime.Now;
                list.Add(whPositionPower);
            }

            int result = cf.WhPositionPowerListAdd(list.ToArray());
            if (result > 0)
            {
                return Helper.RedirectAjax("ok", "添加成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "添加失败，权限已存在！", null, "");
            }
        }

        /// <summary>
        /// 取消职位的某个权限
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult WhPositionPowerDelById()
        {
            int id = Convert.ToInt32(Request["Id"]);
            int result = cf.WhPositionPowerDelById(id);
            if (result > 0)
            {
                return Helper.RedirectAjax("ok", "取消成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "取消失败！", null, "");
            }
        }
    }
}

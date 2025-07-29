using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace WMS.UI.Controllers
{
    public class UserInfoController : Controller
    {
        WCF.AdminService.AdminServiceClient cf = new WCF.AdminService.AdminServiceClient();
        Helper Helper = new Helper();
        EIP.EIP EIP = new EIP.EIP();

        [DefaultRequest]
        public ActionResult Index()
        {
            //string aaaa1 = Session["whCompany"].ToString();

            ViewData["WhCodeNameSelect"] = from r in cf.WhInfoList(Convert.ToInt32(Session["whCompany"].ToString()), Session["userName"].ToString())
                                           select new SelectListItem()
                                           {
                                               Text = r.WhName,     //text
                                               Value = r.WhName
                                           };

            ViewData["WhCodeSelect"] = from r in cf.WhInfoList(Convert.ToInt32(Session["whCompany"].ToString()), Session["userName"].ToString())
                                       select new SelectListItem()
                                       {
                                           Text = r.WhName,     //text
                                           Value = r.Id.ToString()
                                       };

            ViewData["WhPositionSelect"] = from r in cf.WhPositionSelect(Session["whCode"].ToString())
                                           select new SelectListItem()
                                           {
                                               Text = r.PositionNameCN,    //text
                                               Value = r.PositionNameCN.ToString()
                                           };



            return View();
        }

        /// <summary>
        /// 新增用户
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Check]
        public ActionResult AddUser()
        {
            WCF.AdminService.WhUser whUser = new WCF.AdminService.WhUser();

            whUser.CompanyId = Convert.ToInt32(Session["whCompany"].ToString());
            whUser.UserName = Request["txt_userCode"];
            whUser.PassWord = Request["txt_password"];
            whUser.UserNameCN = Request["txt_usernameCN"];
            whUser.UserCode = Request["txt_userCode"];
            whUser.Status = "Active";
            whUser.CreateUser = Session["userName"].ToString();
            whUser.CreateDate = DateTime.Now;

            WCF.AdminService.WhUser whUserResult = cf.WhUserAdd(whUser);
            if (whUserResult != null)
            {
                return Helper.RedirectAjax("ok", "添加成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "对不起，添加失败，工号已存在！", null, "");
            }
        }

        /// <summary>
        /// 查询所有用户信息
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult List()
        {
            WCF.AdminService.WhUserSearch whUserSearch = new WCF.AdminService.WhUserSearch();
            whUserSearch.pageIndex = Convert.ToInt32(Request["eip_page"]);
            whUserSearch.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            whUserSearch.CompanyId = Convert.ToInt32(Session["whCompany"].ToString());

            whUserSearch.UserNameCN = Request["user_nameCN"];
            whUserSearch.UserCode = Request["user_code"];
            whUserSearch.PositionNameCN = Request["PositionName"];
            whUserSearch.WhCodeName = Request["WhCodeName"];

            int total = 0;
            List<WCF.AdminService.WhUserResult> list = cf.WhUserList(whUserSearch, out total).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Id", "操作");
            fieldsName.Add("CompanyId", "公司ID");
            fieldsName.Add("UserCode", "工号");
            fieldsName.Add("UserNameCN", "姓名");
            fieldsName.Add("PassWord", "密码");
            fieldsName.Add("Status", "状态");
            fieldsName.Add("PositionNameCN", "职位名");
            fieldsName.Add("WhName", "仓库");
            fieldsName.Add("CreateUser", "创建人");
            fieldsName.Add("CreateDate", "创建时间");

            return Content(EIP.EipListJson(list, total, fieldsName, "Id:110,UserNameCN:110,PassWord:90,CreateDate:120,PositionNameCN:200,WhName:200,Status:100,default:80"));
        }

        /// <summary>
        /// 根据当前用户查询出未选择职位
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult WhPositionUnselected()
        {
            WCF.AdminService.WhPositionSearch whPositionSearch = new WCF.AdminService.WhPositionSearch();
            whPositionSearch.pageIndex = Convert.ToInt32(Request["eip_page"]);
            whPositionSearch.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            whPositionSearch.CompanyId = Convert.ToInt32(Session["whCompany"].ToString());
            whPositionSearch.UserName = Request["UserName"];
            int total = 0;
            WCF.AdminService.WhPositionResult[] list = cf.WhPositionUnselected(whPositionSearch, out total);

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Id", "Id");
            fieldsName.Add("CompanyId", "公司ID");
            fieldsName.Add("PositionName", "职位名");
            fieldsName.Add("PositionNameCN", "职位中文名");

            return Content(EIP.EipListJson(list.ToList(), total, fieldsName, "default:90"));
        }

        /// <summary>
        /// 根据当前用户查询出已有职位
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult WhPositionSelected()
        {
            WCF.AdminService.WhUserWhPositionSearch whUserWhPositionSearch = new WCF.AdminService.WhUserWhPositionSearch();
            whUserWhPositionSearch.pageIndex = Convert.ToInt32(Request["eip_page"]);
            whUserWhPositionSearch.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            whUserWhPositionSearch.CompanyId = Convert.ToInt32(Session["whCompany"].ToString());
            whUserWhPositionSearch.UserName = Request["UserName"];

            int total = 0;
            List<WCF.AdminService.WhUserWhPositionResult> list = cf.WhPositionSelected(whUserWhPositionSearch, out total).ToList();
            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Id", "Id");
            fieldsName.Add("Action", "操作");
            fieldsName.Add("CompanyId", "公司ID");
            fieldsName.Add("PositionName", "职位名");
            fieldsName.Add("PositionNameCN", "职位中文名");

            return Content(EIP.EipListJson(list, total, fieldsName, "Action:50,default:90"));
        }

        /// <summary>
        /// 取消用户的某个职位
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult WhUserPositionDeleteById()
        {
            int id = Convert.ToInt32(Request["Id"]);
            int result = cf.WhUserPositionDeleteById(id);
            if (result > 0)
            {
                return Helper.RedirectAjax("ok", "取消成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "对不起，取消失败！", null, "");
            }

        }


        /// <summary>
        /// 根据当前用户查询出未选择仓库
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult WhInfoUnselected()
        {
            WCF.AdminService.WhInfoSearch whInfoSearch = new WCF.AdminService.WhInfoSearch();
            whInfoSearch.pageIndex = Convert.ToInt32(Request["eip_page"]);
            whInfoSearch.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            whInfoSearch.CompanyId = Convert.ToInt32(Session["whCompany"].ToString());
            whInfoSearch.UserName = Request["UserName"];
            int total = 0;
            WCF.AdminService.WhInfoResult[] list = cf.WhInfoUnselected(whInfoSearch, out total);

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Id", "Id");
            fieldsName.Add("CompanyId", "公司ID");
            fieldsName.Add("WhCode", "仓库ID");
            fieldsName.Add("WhName", "仓库名");

            return Content(EIP.EipListJson(list.ToList(), total, fieldsName, "default:90"));
        }

        /// <summary>
        /// 根据当前用户查询出已有仓库
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult WhInfoSelected()
        {
            WCF.AdminService.WhInfoSearch whInfoSearch = new WCF.AdminService.WhInfoSearch();
            whInfoSearch.pageIndex = Convert.ToInt32(Request["eip_page"]);
            whInfoSearch.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            whInfoSearch.CompanyId = Convert.ToInt32(Session["whCompany"].ToString());
            whInfoSearch.UserName = Request["UserName"];

            int total = 0;
            List<WCF.AdminService.WhInfoWhUserResult> list = cf.WhInfoSelected(whInfoSearch, out total).ToList();
            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Id", "Id");
            fieldsName.Add("Action", "操作");
            fieldsName.Add("CompanyId", "公司ID");
            fieldsName.Add("WhCode", "仓库ID");
            fieldsName.Add("WhName", "仓库名");

            return Content(EIP.EipListJson(list, total, fieldsName, "Action:50,default:90"));
        }

        /// <summary>
        /// 取消用户的某个仓库
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult WhInfoWhUserDel()
        {
            int id = Convert.ToInt32(Request["Id"]);
            int result = cf.WhInfoWhUserDel(id);
            if (result > 0)
            {
                return Helper.RedirectAjax("ok", "取消成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "对不起，取消失败！", null, "");
            }

        }

        /// <summary>
        /// 添加用户职位关系
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult WhUserPositionListAdd()
        {
            string[] Pos_Id = Request.Form.GetValues("chx_Pos");

            string Hid_UId = Request["Hid_UId"];            //当前选择的用户ID
            string Hid_UserName = Request["Hid_UserName"];  //当前选择的用户登录名

            List<WCF.AdminService.WhUserPosition> list = new List<WCF.AdminService.WhUserPosition>();
            for (int i = 0; i < Pos_Id.Length; i++)
            {
                int PId = Convert.ToInt32(Pos_Id[i].Split('-')[0]);
                string PosName = Pos_Id[i].Split('-')[1];

                WCF.AdminService.WhUserPosition whUserPosition = new WCF.AdminService.WhUserPosition();
                whUserPosition.CompanyId = Convert.ToInt32(Session["whCompany"].ToString());
                whUserPosition.UserId = Convert.ToInt32(Hid_UId);
                whUserPosition.UserName = Hid_UserName;
                whUserPosition.PositionId = Convert.ToInt32(PId);
                whUserPosition.PositionName = PosName;
                whUserPosition.CreateUser = Session["userName"].ToString();
                whUserPosition.CreateDate = DateTime.Now;
                list.Add(whUserPosition);
            }

            int result = cf.WhUserPositionListAdd(list.ToArray());
            if (result > 0)
            {
                return Helper.RedirectAjax("ok", "添加成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "对不起，添加失败，职位已存在！", null, "");
            }
        }


        /// <summary>
        /// 添加用户仓库关系
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult R_WhInfo_WhUserAdd()
        {
            string[] Pos_Id = Request.Form.GetValues("chx_Pos");

            string Hid_UId = Request["Hid_UId"];            //当前选择的用户ID

            List<WCF.AdminService.R_WhInfo_WhUser> list = new List<WCF.AdminService.R_WhInfo_WhUser>();
            for (int i = 0; i < Pos_Id.Length; i++)
            {
                WCF.AdminService.R_WhInfo_WhUser whUserPosition = new WCF.AdminService.R_WhInfo_WhUser();

                whUserPosition.UserId = Convert.ToInt32(Hid_UId);
                whUserPosition.WhCodeId = Convert.ToInt32(Convert.ToInt32(Pos_Id[i]));
                whUserPosition.CreateUser = Session["userName"].ToString();
                whUserPosition.CreateDate = DateTime.Now;
                list.Add(whUserPosition);
            }

            int result = cf.R_WhInfo_WhUserAdd(list.ToArray());
            if (result > 0)
            {
                return Helper.RedirectAjax("ok", "添加成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "对不起，添加失败！", null, "");
            }
        }

        /// <summary>
        /// 密码初始化--------根据当前年月日初始化
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult UserPwdInit()
        {
            int id = Convert.ToInt32(Request["Id"]);
            WCF.AdminService.WhUser whUser = new WCF.AdminService.WhUser();
            whUser.Id = id;
            //whUser.PassWord = DateTime.Now.ToString("yyyyMMdd");
            whUser.PassWord = "123456";
            int result = cf.WhUserPwdInit(whUser);
            if (result > 0)
            {
                return Helper.RedirectAjax("ok", "密码初始化成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "对不起，密码初始化失败！", null, "");
            }
        }

        /// <summary>
        /// 修改用户信息
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult WhUserEdit()
        {
            int id = Convert.ToInt32(Request["Id"]);
            WCF.AdminService.WhUser whUser = new WCF.AdminService.WhUser();
            whUser.Id = id;
            whUser.CompanyId = Convert.ToInt32(Request["whCompanyId"]);
            whUser.UserName = Request["UserName"];
            whUser.UserCode = Request["UserName"];
            whUser.UserNameCN = Request["UserNameCN"];
            whUser.Status = Request["Status"];
            whUser.UpdateUser = Session["userName"].ToString();
            whUser.UpdateDate = DateTime.Now;
            int result = cf.WhUserEdit(whUser, new string[] { "UserName", "UserCode", "UserNameCN", "Status", "UpdateUser", "UpdateDate" });
            if (result > 0)
            {
                return Helper.RedirectAjax("ok", "信息修改成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "对不起，修改失败，工号已存在！", null, "");
            }
        }


        [HttpGet]
        public ActionResult UserEditUnActive()
        {
            int id = Convert.ToInt32(Request["Id"]);
            WCF.AdminService.WhUser whUser = new WCF.AdminService.WhUser();
            whUser.Id = id;
            whUser.Status = "UnActive";
            whUser.UpdateUser = Session["userName"].ToString();
            whUser.UpdateDate = DateTime.Now;
            int result = cf.WhUserEdit(whUser, new string[] { "Status", "UpdateUser", "UpdateDate" });

            if (result > 0)
            {
                return Helper.RedirectAjax("ok", "失效用户成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "对不起，操作失败！", null, "");
            }
        }

        [HttpGet]
        public ActionResult UserEditActive()
        {
            int id = Convert.ToInt32(Request["Id"]);
            WCF.AdminService.WhUser whUser = new WCF.AdminService.WhUser();
            whUser.Id = id;
            whUser.Status = "Active";
            whUser.UpdateUser = Session["userName"].ToString();
            whUser.UpdateDate = DateTime.Now;
            int result = cf.WhUserEdit(whUser, new string[] { "Status", "UpdateUser", "UpdateDate" });

            if (result > 0)
            {
                return Helper.RedirectAjax("ok", "重新启用用户成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "对不起，操作失败！", null, "");
            }
        }


        [HttpPost]
        public ActionResult R_WhUser_WhInfo_BatchEdit()
        {
            string[] userId = Request.Form.GetValues("userId");
            string whcodeName = Request["whcodeName"].Trim();

            List<WCF.AdminService.R_WhInfo_WhUser> list = new List<WCF.AdminService.R_WhInfo_WhUser>();
            for (int i = 0; i < userId.Length; i++)
            {
                WCF.AdminService.R_WhInfo_WhUser entity = new WCF.AdminService.R_WhInfo_WhUser();
                entity.UserId = Convert.ToInt32(userId[i]);
                entity.WhCodeId = Convert.ToInt32(whcodeName);
                list.Add(entity);
            }

            cf.R_WhInfo_WhUserAdd(list.ToArray());

            return Helper.RedirectAjax("ok", "批量添加仓库成功！", null, "");

        }

        [HttpGet]
        public ActionResult GetUserList()
        {
            WCF.AdminService.WhUserSearch whUserSearch = new WCF.AdminService.WhUserSearch();
            whUserSearch.pageIndex = Convert.ToInt32(Request["eip_page"]);
            whUserSearch.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            whUserSearch.CompanyId = Convert.ToInt32(Session["whCompany"].ToString());

            whUserSearch.UserNameCN = Request["txt_searchUserName"];
            whUserSearch.UserCode = Request["txt_searchUserCode"];
            whUserSearch.PositionNameCN = Request["txt_searchPositionName"];
            whUserSearch.WhCodeName = Request["txt_searchWhCodeName"];

            int total = 0;
            List<WCF.AdminService.WhUserResult> list = cf.WhUserList(whUserSearch, out total).ToList();
            int userId = Convert.ToInt32(Request["Hid_UserId"]);
            list = list.Where(u => u.Id != userId).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Id", "操作");

            fieldsName.Add("UserCode", "工号");
            fieldsName.Add("UserNameCN", "姓名");

            fieldsName.Add("PositionNameCN", "职位名");
            fieldsName.Add("WhName", "仓库");

            return Content(EIP.EipListJson(list, total, fieldsName, "Id:90,UserNameCN:110,PassWord:90,CreateDate:120,PositionNameCN:200,WhName:200,Status:100,default:80"));
        }


        [HttpGet]
        public ActionResult CopyWhUserPosition()
        {
            int userId = Convert.ToInt32(Request["Id"]);
            int copyUserId = Convert.ToInt32(Request["CopyId"]);
            int companyId = Convert.ToInt32(Session["whCompany"].ToString());

            string result = cf.CopyWhUserPosition(userId, copyUserId, companyId);

            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "权限复制成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "对不起，操作失败！", null, "");
            }
        }

        [HttpGet]
        public ActionResult EditUserCheckFlag()
        {
            int CheckFlag = Convert.ToInt32(Request["edit_CheckFlag"]);
            int result = cf.WhUserCheckFlagEdit(CheckFlag);

            if (result > 0)
            {
                return Helper.RedirectAjax("ok", "操作成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "对不起，操作失败！", null, "");
            }
        }


        [HttpPost]
        public void GetUserCheckFlag()
        {
            WCF.AdminService.WhUserSearch whUserSearch = new WCF.AdminService.WhUserSearch();
            whUserSearch.pageIndex = 1;
            whUserSearch.pageSize = 50;
            whUserSearch.CompanyId = Convert.ToInt32(Session["whCompany"].ToString());

            int total = 0;
            List<WCF.AdminService.WhUserResult> list = cf.WhUserList(whUserSearch, out total).ToList();

            WCF.AdminService.WhUserResult first = list.First();
            if ((first.CheckFlag ?? 0) == 0)
            {
                Response.Write("关");
            }
            else
            {
                Response.Write("开");
            }

        }

    }
}

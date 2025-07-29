using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using WMS.UI.Controllers.Common;
using WMS.UI.Controllers.Model;

namespace WMS.UI.Controllers
{
    //public class InputValidationModelBinder : DefaultModelBinder
    //{
    //    protected override void OnModelUpdated(ControllerContext controllerContext, ModelBindingContext bindingContext)
    //    {
    //        var modelState = controllerContext.Controller.ViewData.ModelState;
    //        var valueProvider = controllerContext.Controller.ValueProvider;

    //        var keysWithNoIncomingValue = modelState.Keys.Where(x => !valueProvider.ContainsPrefix(x));
    //        foreach (var key in keysWithNoIncomingValue)
    //            modelState[key].Errors.Clear();
    //    }
    //}

    public class Helper
    {
        const string UserIdKey = "userId";
        const string UserNameKey = "userName";
        const string CompanyKey = "whCompany";
        const string WhNameKey = "whName";
        const string UserWhCodeKey = "whCode";
        const string UserNameCNKey = "userNameCN";
        const string UserPermissionKey = "userPermission";
        const string UserMenuKey = "userMenu";
        const string PassWordKey = "passWord";
        const string IsAdminKey = "isAdmin";

        WCF.AdminService.AdminServiceClient cf = new WCF.AdminService.AdminServiceClient();

        System.Web.SessionState.HttpSessionState Session
        {
            get
            {
                return HttpContext.Current.Session;
            }

        }


        #region 2.1 当前用户对象 +MODEL.Ou_UserInfo Usr
        // <summary>
        /// 当前用户对象
        /// </summary>
        WCF.AdminService.WhUser Usr { get; set; }
        //{
        //    get
        //    {
        //        return Session[UserIdKey] as WCF.AdminService.WhUser;
        //    }
        //    set
        //    {
        //        Session[UserIdKey] = value;
        //    }
        //}
        #endregion

        #region 0.3 用户权限 +List<MODEL.Ou_Permission> UsrPermission
        // <summary>
        /// 用户权限
        /// </summary>
        public List<WCF.AdminService.WhPositionPowerMVCResult> UsrPermission
        {
            get
            {
                return Session[UserPermissionKey] as List<WCF.AdminService.WhPositionPowerMVCResult>;
            }
            set
            {
                Session[UserPermissionKey] = value;
            }
        }
        #endregion

        #region 0.4 用户权限菜单 +List<WCF.AdminService.WhMenu> UserMenu
        // <summary>
        /// 用户权限菜单
        /// </summary>
        public List<WCF.AdminService.WhMenuResult> UserMenu
        {
            get
            {
                return Session[UserMenuKey] as List<WCF.AdminService.WhMenuResult>;
            }
            set
            {
                Session[UserMenuKey] = value;
            }
        }
        #endregion


        /// <summary>
        /// 管理员登录方法
        /// </summary>
        /// <param name="usrPara"></param>
        public bool LoginUser(WCF.AdminService.WhUser usrPara, int IsAlways, string WhCode = null)
        {
            MD5 md5Hasher = MD5.Create();
            byte[] data = md5Hasher.ComputeHash(Encoding.GetEncoding("UTF-8").GetBytes(usrPara.PassWord));
            usrPara.PassWord = BitConverter.ToString(data);

            //WCF.AdminService.AdminServiceClient cf = new WCF.AdminService.AdminServiceClient();
            //  WCF.AdminService.AdminServiceClient cf = new WCF.AdminService.AdminServiceClient();
            WCF.AdminService.WhUser usr = cf.LoginIn(usrPara);

            if (usr != null)
            {
                //2.1 保存 用户数据(Session or Cookie)
                Usr = usr;
                //如果选择了复选框，则要使用cookie保存数据
                if (IsAlways == 1)
                {
                    ////2.1.2将用户id加密成字符串
                    //string strCookieValue = SecurityHelper.EncryptUserInfo(usr.Id.ToString());
                    ////2.1.3创建cookie
                    //HttpCookie cookie = new HttpCookie(UserIdKey, strCookieValue);
                    //cookie.Expires = DateTime.Now.AddDays(1);
                    ////cookie.Path = "/user/";
                    //HttpContext.Current.Response.Cookies.Add(cookie);
                    cookieSet(UserIdKey, usr.Id.ToString());
                    cookieSet(UserNameKey, usr.UserName.ToString());
                    // cookieSet(UserWhCodeKey, "01");
                    cookieSet(CompanyKey, usr.CompanyId.ToString());
                    cookieSet(UserNameCNKey, usr.UserNameCN.ToString());
                    cookieSet(PassWordKey, usr.PassWord.ToString());
                }

                Session[UserIdKey] = usr.Id.ToString();
                Session[UserNameKey] = usr.UserName.ToString();

                Session[CompanyKey] = usr.CompanyId.ToString();
                Session[UserNameCNKey] = usr.UserNameCN.ToString();
               

                //区分超级管理员
                if (usr.CompanyId.ToString() == "0")
                {
                    Session[IsAdminKey] = "Y";
                }
                else
                    Session[IsAdminKey] = "N";

                //WhCode不为空直接重新设置Company 
                if (!string.IsNullOrEmpty(WhCode))
                {

                    Session[UserWhCodeKey] = WhCode;


                    var sql = from r in cf.WhInfoList(Convert.ToInt32(Session[CompanyKey]), Session[UserNameKey].ToString())
                              select new WCF.AdminService.WhInfoResult()
                              {
                                  WhCode = r.WhCode,
                                  CompanyId = r.CompanyId
                              };

                    Session[CompanyKey] = sql.Where(u => u.WhCode == WhCode).ToList()[0].CompanyId;
                    usr.CompanyId = Convert.ToInt32(Session[CompanyKey]);
                    //string aa1 = Session[CompanyKey].ToString();
                    //Session[UserWhCodeKey] = usrPara.w
                }
                SetUserPower(usr);
                return true;
            }
            return false;
        }


        public bool changeWhcompany(int changeWhcompany)
        {
            if ((Session[IsAdminKey] == null ? "" : Session[IsAdminKey].ToString()) == "Y" && changeWhcompany > 0)
            {
                Session[CompanyKey] = changeWhcompany;
                WCF.AdminService.WhUser usrPara = new WCF.AdminService.WhUser();
                usrPara.UserName = Session[UserNameKey].ToString();
                usrPara.CompanyId = changeWhcompany;
                SetUserPower(usrPara);
                return true;
            }
            else
                return false;
        }
        public void SetUserPower(WCF.AdminService.WhUser user)
        {

            // 查询当前用户的 权限，并将权限 存入 Session 中
            UsrPermission = GetUserPermission(user);
            // 查询当前用户的菜单 并将菜单 存入 Session 中
            UserMenu = GetUserMenu(user);
        }

        //设置WhCode
        public bool setWhCode(string whCode, int IsAlways)
        {
            if (Session[UserIdKey] != null && whCode != null)
            {
                //再次验证是否有WhCode的权限
                var sql = (from r in cf.WhInfoList(Convert.ToInt32(Session[CompanyKey].ToString()), Session[UserNameKey].ToString())
                          select new WCF.AdminService.WhInfoResult()
                          {
                              WhCode = r.WhCode,
                              CompanyId = r.CompanyId,
                              WhName=r.WhName
                          }).ToList();
                int counts = sql.Where(u => u.WhCode == whCode).Count();
                if (counts > 0)
                {
                    Session[UserWhCodeKey] = whCode;
                    Session[CompanyKey] = sql.Where(u => u.WhCode == whCode).ToList()[0].CompanyId;
                    Session[WhNameKey] = sql.Where(u => u.WhCode == whCode).ToList()[0].WhName;


                   // string aa = Session[CompanyKey].ToString();

                    SetUserPower(new WCF.AdminService.WhUser { CompanyId = Convert.ToInt32(Session[CompanyKey].ToString()), UserName = Session[UserNameKey].ToString() });
                    if (IsAlways == 1) cookieSet(UserWhCodeKey, whCode);
                    return true;
                }
                return false;
            }
            else
                return false;

        }

        public void cookieSet(string cookieKey, string cookieValue)
        {

            HttpCookie cookie = new HttpCookie(cookieKey, SecurityHelper.EncryptUserInfo(cookieValue));
            //有效时间
            cookie.Expires = DateTime.Now.AddDays(1);
            HttpContext.Current.Response.Cookies.Add(cookie);
        }


        #region 2.0 根据用户查询用户权限 
        /// <summary>
        /// 根据用户id查询用户权限
        /// </summary>
        /// <param name="usrId"></param>
        public List<WCF.AdminService.WhPositionPowerMVCResult> GetUserPermission(WCF.AdminService.WhUser ur)
        {
            // WCF.AdminService.AdminServiceClient cf = new WCF.AdminService.AdminServiceClient();
            WCF.AdminService.WhPositionPowerMVCSearch whUser = new WCF.AdminService.WhPositionPowerMVCSearch();
            whUser.CompanyId = ur.CompanyId;
            whUser.UserName = ur.UserName;
            return cf.WhPowerMVCList(whUser).ToList();
        }
        #endregion

        #region 2.1 根据用户查询用户菜单
        /// <summary>
        /// 根据用户对象查询用户菜单
        /// </summary>
        /// <param name="usrId"></param>
        public List<WCF.AdminService.WhMenuResult> GetUserMenu(WCF.AdminService.WhUser ur)
        {
            WCF.AdminService.AdminServiceClient cf = new WCF.AdminService.AdminServiceClient();

            return cf.WhUserMenuGet(new WCF.AdminService.WhUser { CompanyId = ur.CompanyId, UserName = ur.UserName }).OrderBy(u => u.ParentMenuId).OrderBy(u => u.MenuSort).ToList();
        }
        #endregion

        #region 2.2 判断当前用户是否登陆 +bool IsLogin()
        /// <summary>
        /// 判断当前用户是否登陆 而且
        /// </summary>
        /// <returns></returns>
        public bool IsLogin()
        {
            //1.验证用户是否登陆(Session && Cookie)
            if (Session[UserNameKey] == null)
            {
                if (HttpContext.Current.Request.Cookies[UserNameKey] == null)
                {
                    return false;
                }
                else//如果有cookie则从cookie中获取用户id并查询相关数据存入 Session
                {
                    //获取COOKIE字段
                    string UserName = HttpContext.Current.Request.Cookies[UserNameKey].Value;
                    UserName = SecurityHelper.DecryptUserInfo(UserName);
                    string WhCode = HttpContext.Current.Request.Cookies[UserWhCodeKey].Value;
                    WhCode = SecurityHelper.DecryptUserInfo(WhCode);
                    string PassWord = HttpContext.Current.Request.Cookies[PassWordKey].Value;
                    PassWord = SecurityHelper.DecryptUserInfo(PassWord);
                    string Company = HttpContext.Current.Request.Cookies[CompanyKey].Value;
                    Company = SecurityHelper.DecryptUserInfo(Company);

                    //生成验证对象
                    WCF.AdminService.WhUser usr = new WCF.AdminService.WhUser();
                    usr.CompanyId = Convert.ToInt32(Company);
                    usr.UserName = UserName;
                    usr.PassWord = PassWord;
                    //验证用户,设置全局用户Usr
                    Usr = cf.LoginIn(usr);


                    if (Usr != null)
                    {
                        //设置Session
                        Session[UserIdKey] = usr.Id.ToString();
                        Session[UserNameKey] = usr.UserName.ToString();
                        Session[CompanyKey] = usr.CompanyId.ToString();
                        Session[UserNameCNKey] = usr.UserNameCN.ToString();
                        //设置权限
                        UsrPermission = GetUserPermission(Usr);

                    }

                }
            }
            return true;
        }
        #endregion

        #region  2.3 判断当前用户 是否有 访问当前页面的权限 +bool HasPemission
        /// <summary>
        /// 2.3 判断当前用户 是否有 访问当前页面的权限
        /// </summary> 
        /// <param name="areaName"></param>
        /// <param name="controllerName"></param>
        /// <param name="actionName"></param>
        /// <param name="httpMethod"></param>
        /// <returns></returns>
        public bool HasPemission(string areaName, string controllerName, string actionName, string httpMethod)
        {
            var listP = from per in UsrPermission
                        where
                            string.Equals(per.AreaName, areaName, StringComparison.CurrentCultureIgnoreCase) &&
                            string.Equals(per.ControllerName, controllerName, StringComparison.CurrentCultureIgnoreCase) &&
                            string.Equals(per.ActionName, actionName, StringComparison.CurrentCultureIgnoreCase) && (
                                per.HttpMethod == 3 ||//如果数据库保存的权限 请求方式 =3 代表允许 get/post请求
                                per.HttpMethod == (httpMethod.ToLower() == "get" ? 1 : 2)
                            )
                        select per;
            return listP.Count() > 0;
            //return true;
        }
        #endregion

        #region 3.1 生成 Json 格式的返回值 +ActionResult RedirectAjax(string statu, string msg, object data, string backurl)
        /// <summary>
        /// 生成 Json 格式的返回值
        /// </summary>
        /// <param name="statu"></param>
        /// <param name="msg"></param>
        /// <param name="data"></param>
        /// <param name="backurl"></param>
        /// <returns></returns>
        public ActionResult RedirectAjax(string statu, string msg, object data, string backurl)
        {
            AjaxMsgModel ajax = new AjaxMsgModel()
            {
                Statu = statu,
                Msg = msg,
                Data = data,
                BackUrl = backurl
            };
            JsonResult res = new JsonResult();
            res.Data = ajax;
            res.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            return res;
        }
        #endregion

        #region 3.2 重定向方法 根据Action方法特性  +ActionResult Redirect(string url, ActionDescriptor action)
        /// <summary>
        /// 重定向方法 有两种情况：如果是Ajax请求，则返回 Json字符串；如果是普通请求，则 返回重定向命令
        /// </summary>
        /// <returns></returns>
        public ActionResult Redirect(string url, ActionDescriptor action)
        {
            //如果 超链接或表单 没有权限访问，则返回 302重定向命令 
            if (action.IsDefined(typeof(DefaultRequestAttribute), true) || action.ControllerDescriptor.IsDefined(typeof(DefaultRequestAttribute), true))
            {
                return new RedirectResult(url);
            }
            else//如果不是Ajax请求没有权限，就返回 Json消息
            {
                return RedirectAjax("nologin", "您没有登陆或没有权限访问此页面~~", null, null);
            }
        }
        #endregion


        public string CheckModelErrors(Controller CheckController)
        {

            StringBuilder sbErrors = new StringBuilder();
            foreach (var item in CheckController.ModelState)
            {
                if (item.Value.Errors.Count > 0)
                {
                    for (int i = item.Value.Errors.Count - 1; i >= 0; i--)
                    {
                        sbErrors.Append(item.Key);
                        sbErrors.Append(":");
                        sbErrors.Append(item.Value.Errors[i].ErrorMessage);
                        sbErrors.Append("<br/>");
                    }
                }
            }
            return sbErrors.ToString();

        }

    }



}

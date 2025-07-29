using System.Collections.Generic;
using System.Linq;
using WMS.UI.Controllers;
using WMS.UI.Controllers.Common;

namespace WMS.UI.Controllers.Filters
{
   

    /// <summary>
    /// 管理员 身份验证 过滤器
    /// </summary>
    public class ActionExcutedAttribute : System.Web.Mvc.AuthorizeAttribute
    {
        #region 1.0 验证方法 - 在 ActionExcuting过滤器之前执行
        /// <summary>
        /// 验证方法 - 在 ActionExcuting过滤器之前执行
        /// </summary>
        /// <param name="filterContext"></param>
        public override void OnAuthorization(System.Web.Mvc.AuthorizationContext filterContext)
        {
            Helper helper = new Helper();
            //获取区域名
            string strArea = filterContext.RouteData.DataTokens.Keys.Contains("area") ? filterContext.RouteData.DataTokens["area"].ToString().ToLower() : "";
            //要验证的区域名 集合
            List<string> listAreaLimite = new List<string>() { "admin", "rec", "root" ,"outbound"};
            //1.如果请求的 Admin 区域里的 控制器类和方法，那么就要验证权限
            if (!string.IsNullOrEmpty(strArea) && listAreaLimite.Contains(strArea))//监测区域名 是否为 admin
            {
                //2.检查 被请求的 方法 和 控制器是否有 CheckAttribute 标签，如果有,则验证,如果没有，则不验证；
                if (filterContext.ActionDescriptor.IsDefined(typeof(CheckAttribute), true)||
                    filterContext.ActionDescriptor.ControllerDescriptor.IsDefined(typeof(CheckAttribute), true))
                {
                    #region 1.0验证用户是否登陆(Session && Cookie)
                    //1.验证用户是否登陆(Session && Cookie)
                    if (!helper.IsLogin())
                    {
                        filterContext.Result = helper.Redirect("/admin/admin/index", filterContext.ActionDescriptor);
                    }
                    #endregion
                    #region //2.0验证登陆用户 是否有访问该页面的权限
                    else
                    {
                        //2.获取 登陆用户权限
                        string strAreaName = filterContext.RouteData.DataTokens["area"].ToString().ToLower();
                        string strContrllerName = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName.ToLower();
                        string strActionName = filterContext.ActionDescriptor.ActionName.ToLower();
                        string strHttpMethod = filterContext.HttpContext.Request.HttpMethod;
                        //filterContext.HttpContext.Response.Cache.SetCacheability(System.Web.HttpCacheability.NoCache);

                        if (!helper.HasPemission(strAreaName, strContrllerName, strActionName, strHttpMethod))
                        {
                            filterContext.Result = helper.Redirect("/admin/admin/index?msg=noPermission", filterContext.ActionDescriptor);
                        }
                    }
                    #endregion
                }

            }
        }
        #endregion
    }
}

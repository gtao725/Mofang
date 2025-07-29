using System.Web.Mvc;
using System.Web.Routing;

namespace WMS.UI
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            // ModelBinders.Binders.DefaultBinder = new Controllers.InputValidationModelBinder();

            //注册全局的自定义MVC异常过滤器
            GlobalFilters.Filters.Add(new MVCExceptionFilterAttribution());
        }
    }
}

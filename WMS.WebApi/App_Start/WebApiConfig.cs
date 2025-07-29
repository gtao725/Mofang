using System.Web.Http;
using WMS.WebApi.Common;

namespace WMS.WebApi
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API 配置和服务
     


            //配置返回格式
            config.Filters.Add(new ApiResultAttribute());
            //配置异常返回格式
            config.Filters.Add(new ApiErrorHandleAttribute());
            // Web API 路由
            config.MapHttpAttributeRoutes();
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{action}/{id}",
                defaults: new { controller = "Test", action = "List",id = RouteParameter.Optional }
            );
        }
    }
}

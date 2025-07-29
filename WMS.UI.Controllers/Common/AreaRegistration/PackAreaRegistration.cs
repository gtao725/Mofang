using System.Web.Mvc;

namespace WMS.UI.Areas.Pack
{
    public class PacAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            { 
                return "Pack";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "Pack_default",
                "Pack/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional },
                 namespaces: new string[1] { "WMS.UI.Controllers" }
            );
        }
    }
}
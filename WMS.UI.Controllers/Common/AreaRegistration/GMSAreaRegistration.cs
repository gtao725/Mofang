using System.Web.Mvc;

namespace WMS.UI.Areas.GMS
{
    public class GMSAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            { 
                return "GMS";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "GMS_default",
                "GMS/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional },
                 namespaces: new string[1] { "WMS.UI.Controllers" }
            );
        }
    }
}
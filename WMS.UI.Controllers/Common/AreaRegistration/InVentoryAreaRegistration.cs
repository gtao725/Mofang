using System.Web.Mvc;

namespace WMS.UI.Areas.InVentory
{
    public class InVentoryAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "InVentory";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "InVentory_default",
                "InVentory/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional },
                 namespaces: new string[1] { "WMS.UI.Controllers" }
            );
        }
    }
}
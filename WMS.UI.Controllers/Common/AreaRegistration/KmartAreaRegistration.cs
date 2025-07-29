using System.Web.Mvc;

namespace WMS.UI.Areas.InBound
{
    public class KmartAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Kmart";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "Kmart_default",
                "Kmart/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional },
                 namespaces: new string[1] { "WMS.UI.Controllers" }
            );
        }
    }
}
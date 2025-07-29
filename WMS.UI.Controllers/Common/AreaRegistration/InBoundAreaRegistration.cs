using System.Web.Mvc;

namespace WMS.UI.Areas.InBound
{
    public class InBoundAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "InBound";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "InBound_default",
                "InBound/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional },
                 namespaces: new string[1] { "WMS.UI.Controllers" }
            );
        }
    }
}
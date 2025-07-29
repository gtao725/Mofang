using System.Web.Mvc;

namespace WMS.UI.Areas.OutBound
{
    public class OutBoundAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "OutBound";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "OutBound_default",
                "OutBound/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional },
                 namespaces: new string[1] { "WMS.UI.Controllers" }
            );
        }
    }
}
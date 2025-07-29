using System.Web.Mvc;

namespace WMS.UI.Areas.Root
{
    public class RootAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Root";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "Root_default",
                "Root/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional },
                 namespaces: new string[1] { "WMS.UI.Controllers" }
            );
        }
    }
}

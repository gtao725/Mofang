using System.Web.Mvc;

namespace WMS.UI.Areas.Rec
{
    public class RecAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Rec";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "Rec_default",
                "Rec/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional },
                 namespaces: new string[1] { "WMS.UI.Controllers" }
            );
        }
    }
}
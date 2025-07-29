using System.Web.Mvc;

namespace WMS.UI.Areas.RecReport
{
    public class RecReportAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "RecReport";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "RecReport_default",
                "RecReport/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional },
                 namespaces: new string[1] { "WMS.UI.Controllers" }
            );
        }
    }
}
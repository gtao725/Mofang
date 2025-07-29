using System.Web;
using System.Web.Mvc;


namespace WMS.UI
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new Controllers.Filters.ActionExcutedAttribute());
        }
    }
}
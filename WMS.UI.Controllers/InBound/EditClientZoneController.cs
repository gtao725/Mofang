using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace WMS.UI.Controllers
{
    public class EditClientZoneController : Controller
    {
        WCF.RootService.RootServiceClient cf = new WCF.RootService.RootServiceClient();
        Helper Helper = new Helper();
        EIP.EIP EIP = new EIP.EIP();

        [DefaultRequest]
        public ActionResult Index()
        {
            ViewData["ZoneSelect"] = from r in cf.ZoneSelect(Session["whCode"].ToString())
                                     select new SelectListItem()
                                     {
                                         Text = r.ZoneName,     //text
                                         Value = r.Id.ToString()
                                     };

            return View();
        }

      



    }
}

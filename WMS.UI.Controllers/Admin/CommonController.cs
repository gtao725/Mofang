using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using WMS.EIP;
using WMS.UI.Controllers.Model;

namespace WMS.UI.Controllers
{

    public class CommonController : Controller
    {
        WCF.AdminService.AdminServiceClient cf = new WCF.AdminService.AdminServiceClient();
        WCF.RootService.RootServiceClient rootcf = new WCF.RootService.RootServiceClient();
        Helper Helper = new Helper();

        [DefaultRequestAttribute]

        public ActionResult highcharts()
        {

            string whCompany = Session["whCompany"] == null ? "" : Session["whCompany"].ToString();
            string WhCode = Session["whCode"].ToString();

            rootcf.LocRateList(WhCode);

            string Htmldetail = "";
            foreach (var item in rootcf.InvRateList(WhCode))
            {
                Htmldetail += "<tr><th>" + item.Code + "</th><td align='center'>" + item.Value1 + "</td><td align='center'>" + item.Value2 + "</td></tr>";
            }

            ViewData["Htmldetail"] = Htmldetail;

            return View();
            // return View(new TestModel());
        }

        public ActionResult highcharts1()
        {

            string whCompany = Session["whCompany"] == null ? "" : Session["whCompany"].ToString();
            string WhCode = Session["whCode"].ToString();

            rootcf.LocRateList(WhCode);

            string Htmldetail = "";
            string Htmldetail1 = "";
            foreach (var item in rootcf.LocRateList(WhCode))
            {
                Htmldetail += "<tr><th>" + item.Code + "</th><td align='center'>" + item.Value1 + "</td><td align='center'>" + item.Value2 + "</td></tr>";
                Htmldetail1 += "<tr><th>" + item.Code + "</th><td align='center'>" + item.Value1 + "</td><td align='center'>" + item.Value2 + "</td><td align='center'>" + ((int)((float.Parse(item.Value1) / float.Parse(item.Value2)) * 100)).ToString() + "%" + "</td></tr>";
            }
            ViewData["Htmldetail"] = Htmldetail;
            ViewData["Htmldetail1"] = Htmldetail1;

            return View();
            // return View(new TestModel());
        }

        public ActionResult highcharts2()
        {

            string whCompany = Session["whCompany"] == null ? "" : Session["whCompany"].ToString();
            string WhCode = Session["whCode"].ToString();

            string Htmldetail = "";
            string Htmldetail1 = "";
            int count = 0;
            foreach (var item in rootcf.ClientInvRateList(WhCode))
            {
                if (count < 10)
                {
                    Htmldetail += "<tr><th>" + item.ClientCode + "</th><td align='center'>" + item.Value1 + "</td><td align='center'>" + item.Value2 + "</td></tr>";
                }
                count++;

                Htmldetail1 += "<tr><td align='center'>" + item.ClientCode + "</td><td align='center'>" + item.Value1 + "</td><td align='center'>" + item.Value2 + "</td><td align='center'>" + item.Value3 + "</td><td align='center'>" + item.Value4 + "</td></tr>";
            }
            ViewData["Htmldetail"] = Htmldetail;
            ViewData["Htmldetail1"] = Htmldetail1;

            return View();
            // return View(new TestModel());
        }

        public ActionResult Index()
        {

            string whCompany = Session["whCompany"] == null ? "" : Session["whCompany"].ToString();


            ViewData["ifPWUnActive"] = Request["ifPWUnActive"] == null ? "N" : Request["ifPWUnActive"];

            List<WCF.AdminService.WhCompany> whCompanySelect = cf.WhCompanyList().ToList();
            var sqlwhInfoSelect = from r in whCompanySelect
                                  select new SelectListItem()
                                  {
                                      Value = r.Id.ToString(),
                                      Text = r.CompanyName,
                                      Selected = r.Id == Convert.ToInt32(whCompany)
                                  };
            if (Session["isAdmin"] != null)
                ViewData["whInfoSelect"] = sqlwhInfoSelect.ToList();
            else
                ViewData["whInfoSelect"] = null;

            ViewData["isAdmin"] = Session["isAdmin"] == null ? "" : Session["isAdmin"].ToString();

            ViewData["whName"] = Session["whName"];
            ViewData["userNameCN"] = Session["userNameCN"].ToString();

            List<WCF.AdminService.WhMenuResult> listWhMenu = (List<WCF.AdminService.WhMenuResult>)Session["userMenu"];
            var sql = from r in listWhMenu
                      where r.ParentMenuId == 0
                      select new MenuTreeModel()
                      {
                          id = r.Id,
                          text = r.MenuNameCN,     //text
                          iconCls = r.MenuIcon
                      };
            return View(new MenuTreeModel() { children = sql.ToList() });
            // return View(new TestModel());
        }
        public ActionResult downApp()
        {

            return View();
        }
        /// <summary>
        /// 查询所有用户信息
        /// </summary>
        /// <returns></returns>
        public ActionResult GetTree(int id)
        {


            List<WCF.AdminService.WhMenuResult> listWhMenu = (List<WCF.AdminService.WhMenuResult>)Session["userMenu"];
            //var aa = listWhMenu.Where(u =>u.ParentMenuId==id).Count();

            if (Session["userMenu"] != null)
            {
                var sql = from r in listWhMenu
                          where r.ParentMenuId == id
                          select new MenuTreeModel()
                          {
                              id = r.Id,
                              text = r.MenuNameCN,     //text
                              attributes = r.MenuUrl,
                              iconCls = r.MenuIcon,
                              state = (listWhMenu.Where(u => u.ParentMenuId == r.Id).Count() > 0) ? "closed" : "open",
                              children = TreeChildren((int)r.Id)
                          };

                return Json(sql);
            }
            else
            {
                return Json("0", JsonRequestBehavior.AllowGet);
            }

        }

        List<MenuTreeModel> TreeChildren(int id)
        {
            List<WCF.AdminService.WhMenuResult> listWhMenu = (List<WCF.AdminService.WhMenuResult>)Session["userMenu"];
            var sql = from r in listWhMenu
                      where r.ParentMenuId == id
                      select new MenuTreeModel()
                      {
                          id = r.Id,
                          text = r.MenuNameCN,     //text
                          attributes = r.MenuUrl,
                          iconCls = r.MenuIcon,
                          state = (listWhMenu.Where(u => u.ParentMenuId == r.Id).Count() > 0) ? "closed" : "open",
                      };

            return sql.ToList();
        }

        public ActionResult changeWhcompany()
        {
            int changeWhcompany = Convert.ToInt32(Request["changeWhcompany"]);


            if (Helper.changeWhcompany(changeWhcompany))
            {
                return Helper.RedirectAjax("ok", "", null, "/Admin/Common/index");
            }
            else
            {
                //这里，前端页面不用改，所以我直接利用第一个例子中的前端页面
                return Helper.RedirectAjax("err", "切换失败!", null, "");
            }

        }


    }
}

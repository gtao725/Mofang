using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace WMS.UI.Controllers
{
    public class MenuController : Controller
    {
        WCF.AdminService.AdminServiceClient cf = new WCF.AdminService.AdminServiceClient();
        Helper Helper = new Helper();
        EIP.EIP EIP = new EIP.EIP();
        [DefaultRequest]
        public ActionResult Index()
        {
            ViewData["MenuNameCN"] = from r in cf.MenuNameSelect(Convert.ToInt32(Session["whCompany"].ToString()))
                                     select new SelectListItem()
                                     {
                                         Text = r.MenuNameCN,     //text
                                         Value = r.Id.ToString()
                                     };
            return View();
        }

        //查询所有菜单信息
        [HttpGet]
        public ActionResult List()
        {
            WCF.AdminService.WhMenuSearch search = new WCF.AdminService.WhMenuSearch();
            search.pageIndex = Convert.ToInt32(Request["eip_page"]);
            search.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            search.CompanyId = Convert.ToInt32(Session["whCompany"].ToString());
            search.MenuNameCN = Request["menu_nameCN"];
            search.ParentId = Request["parentId"];
            int total = 0;
            List<WCF.AdminService.WhMenuResult> list = cf.WhMenuList(search, out total).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Id", "操作");
            fieldsName.Add("WhCode", "仓库");
            fieldsName.Add("MenuName", "菜单英文简称");
            fieldsName.Add("MenuNameCN", "菜单中文名");
            fieldsName.Add("MenuUrl", "Url地址");
            fieldsName.Add("MenuIcon", "菜单图标");
            fieldsName.Add("MenuSort", "菜单排序");
            fieldsName.Add("ParentMenuId", "父级菜单ID");
            fieldsName.Add("ParentMenuName", "父级菜单");
            fieldsName.Add("PowerId", "权限ID");
            fieldsName.Add("PowerName", "权限名");
            fieldsName.Add("CreateUser", "创建人");
            fieldsName.Add("CreateDate", "创建时间");

            return Content(EIP.EipListJson(list, total, fieldsName, "MenuName:100,MenuNameCN:110,MenuUrl:150,MenuIcon:70,MenuSort:60,PowerName:150,CreateDate:125,ParentMenuName:100,CreateUser:60,default:80"));
        }

        //新增菜单
        [HttpGet]
        public ActionResult AddMenu()
        {
            WCF.AdminService.WhMenu entity = new WCF.AdminService.WhMenu();
            entity.CompanyId = Convert.ToInt32(Session["whCompany"].ToString());
            entity.MenuName = "test";
            entity.MenuNameCN = Request["txt_menu_nameCN"];
            entity.MenuUrl = Request["txt_url"];
            entity.MenuIcon = Request["txt_icon"].ToString();
            entity.MenuSort = Convert.ToInt32(Request["txt_menuOrderBy"].ToString());

            string parentMenuId = Request["txt_MenuNameCN"];
            if (parentMenuId + "" != "")
            {
                entity.ParentMenuId = Convert.ToInt32(parentMenuId);
            }
            else
            {
                entity.ParentMenuId = 0;
            }
            entity.CreateUser = Session["userName"].ToString();
            entity.CreateDate = DateTime.Now;
            WCF.AdminService.WhMenu result = cf.WhMenuAdd(entity);
            if (result != null)
            {
                return Helper.RedirectAjax("ok", "添加成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "添加失败，菜单中文名已存在！", null, "");
            }
        }

        //删除菜单
        [HttpGet]
        public ActionResult WhMenuDelById()
        {
            int id = Convert.ToInt32(Request["Id"]);
            int result = cf.WhMenuDelById(id);
            if (result > 0)
            {
                return Helper.RedirectAjax("ok", "删除成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "删除失败！", null, "");
            }
        }

        //当前菜单未选择的权限
        [HttpGet]
        public ActionResult WhMenuUnselected()
        {
            WCF.AdminService.WhMenuSearch search = new WCF.AdminService.WhMenuSearch();
            search.pageIndex = Convert.ToInt32(Request["eip_page"]);
            search.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            search.CompanyId = Convert.ToInt32(Session["whCompany"].ToString());
            search.MenuId = Convert.ToInt32(Request["MenuId"]);
            search.PowerName = Request["PowerName"];
            search.PowerType = Request["PowerType"];
            int total = 0;
            WCF.AdminService.WhPower[] list = cf.WhMenuUnselected(search, out total);

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Id", "Id");
            fieldsName.Add("WhCode", "仓库");
            fieldsName.Add("PowerName", "权限名");
            fieldsName.Add("PowerDescription", "说明");
            fieldsName.Add("PowerType", "权限类型");
            fieldsName.Add("CreateUser", "创建人");
            fieldsName.Add("CreateDate", "创建时间");
            fieldsName.Add("UpdateUser", "修改人");
            fieldsName.Add("UpdateDate", "修改时间");

            return Content(EIP.EipListJson(list.ToList(), total, fieldsName, "WhCode:60,PowerName:140,default:90"));
        }

        //当前菜单已选择的权限
        [HttpGet]
        public ActionResult WhMenuSelected()
        {
            WCF.AdminService.WhMenuSearch search = new WCF.AdminService.WhMenuSearch();
            search.pageIndex = Convert.ToInt32(Request["eip_page"]);
            search.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            search.CompanyId = Convert.ToInt32(Session["whCompany"].ToString());
            search.MenuId = Convert.ToInt32(Request["MenuId"]);
            int total = 0;
            WCF.AdminService.WhPower[] list = cf.WhMenuSelected(search, out total);

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Id", "操作");
            fieldsName.Add("WhCode", "仓库");
            fieldsName.Add("PowerName", "权限名");
            fieldsName.Add("PowerDescription", "说明");
            fieldsName.Add("PowerType", "权限类型");
            fieldsName.Add("CreateUser", "创建人");
            fieldsName.Add("CreateDate", "创建时间");
            fieldsName.Add("UpdateUser", "修改人");
            fieldsName.Add("UpdateDate", "修改时间");

            return Content(EIP.EipListJson(list.ToList(), total, fieldsName, "Id:40,WhCode:60,PowerName:140,default:90"));
        }

        //删除菜单的某个权限,即根据菜单ID 修改权限为空
        [HttpGet]
        public ActionResult WhMenuUpdateById()
        {
            int id = Convert.ToInt32(Request["Id"]);
            WCF.AdminService.WhMenu entity = new WCF.AdminService.WhMenu();
            entity.UpdateUser = Session["userName"].ToString();
            entity.UpdateDate = DateTime.Now;

            int result1 = cf.WhMenuUpdateById(entity, id, new string[] { "PowerId", "PowerName", "UpdateUser", "UpdateDate" });
            if (result1 > 0)
            {
                return Helper.RedirectAjax("ok", "取消成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "取消失败！", null, "");
            }
        }

        //菜单添加权限
        [HttpPost]
        public ActionResult WhMenuAddPower()
        {
            string[] Pow_Id = Request.Form.GetValues("chx_Pow");

            int MenuId = Convert.ToInt32(Request["MenuId"]);            //当前选择的菜单ID

            int result = 0;
            List<WCF.AdminService.WhPositionPower> list = new List<WCF.AdminService.WhPositionPower>(); ;
            for (int i = 0; i < Pow_Id.Length; i++)
            {
                WCF.AdminService.WhMenu entity = new WCF.AdminService.WhMenu();
                entity.PowerId = Convert.ToInt32(Pow_Id[i].Split('-')[0]);
                entity.PowerName = Pow_Id[i].Split('-')[1];
                entity.UpdateUser = Session["userName"].ToString();
                entity.UpdateDate = DateTime.Now;

                result += cf.WhMenuUpdateById(entity, MenuId, new string[] { "PowerId", "PowerName", "UpdateUser", "UpdateDate" });
            }

            if (result > 0)
            {
                return Helper.RedirectAjax("ok", "添加成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "添加失败！", null, "");
            }
        }


        //修改菜单信息
        [HttpGet]
        public ActionResult WhMenuEdit()
        {
            string menuSort = Request["MenuSort"];
            string menuParentId = Request["MenuParentId"];

            WCF.AdminService.WhMenu whMenu = new WCF.AdminService.WhMenu();
            if (menuParentId + "" != "")
            {
                whMenu.ParentMenuId = Convert.ToInt32(menuParentId);
            }
            else
            {
                whMenu.ParentMenuId = 0;
            }
            if (menuSort + "" != "")
            {
                whMenu.MenuSort = Convert.ToInt32(menuSort);
            }
            else
            {
                whMenu.MenuSort = 0;
            }
            whMenu.Id = Convert.ToInt32(Request["Id"]);
            whMenu.CompanyId = Convert.ToInt32(Session["whCompany"].ToString());
            whMenu.MenuName = Request["MenuName"];
            whMenu.MenuNameCN = Request["MenuNameCN"];
            whMenu.MenuUrl = Request["MenuUrl"];
            whMenu.MenuIcon = Request["MenuIcon"];
            whMenu.UpdateUser = Session["userName"].ToString();
            whMenu.UpdateDate = DateTime.Now;
            int result = cf.WhMenuEdit(whMenu, new string[] { "MenuName", "MenuNameCN", "MenuUrl", "MenuIcon", "ParentMenuId", "MenuSort", "UpdateUser", "UpdateDate" });
            if (result > 0)
            {
                return Helper.RedirectAjax("ok", "信息修改成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "对不起，修改失败，菜单中文名已存在！", null, "");
            }
        }

    }
}

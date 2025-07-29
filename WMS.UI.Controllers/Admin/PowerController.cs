using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace WMS.UI.Controllers
{
    public class PowerController : Controller
    {
        WCF.AdminService.AdminServiceClient cf = new WCF.AdminService.AdminServiceClient();
        Helper Helper = new Helper();
        EIP.EIP EIP = new EIP.EIP();

        [DefaultRequest]
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 查询所有权限信息
        /// </summary>
        /// <returns></returns>

        [HttpGet]
        public ActionResult List()
        {
            WCF.AdminService.WhPowerSearch whPowerSearch = new WCF.AdminService.WhPowerSearch();
            whPowerSearch.pageIndex = Convert.ToInt32(Request["eip_page"]);
            whPowerSearch.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            whPowerSearch.CompanyId = Convert.ToInt32(Session["whCompany"].ToString());
            whPowerSearch.PowerName = Request["power_name"];
            whPowerSearch.PowerType = Request["power_type"];
            int total = 0;
            List<WCF.AdminService.WhPower> list = cf.WhPowerList(whPowerSearch, out total).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Id", "操作");
            fieldsName.Add("CompanyId", "公司ID");
            fieldsName.Add("PowerName", "权限名");
            fieldsName.Add("PowerDescription", "说明");
            fieldsName.Add("PowerType", "权限类型");
            fieldsName.Add("CreateUser", "创建人");
            fieldsName.Add("CreateDate", "创建时间");
            fieldsName.Add("UpdateUser", "修改人");
            fieldsName.Add("UpdateDate", "修改时间");

            return Content(EIP.EipListJson(list, total, fieldsName, "PowerName:160,PowerDescription:150,CreateDate:130,default:80"));
        }

        /// <summary>
        /// 新增权限
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult AddPower()
        {
            WCF.AdminService.WhPower whPower = new WCF.AdminService.WhPower();
            whPower.CompanyId = Convert.ToInt32(Session["whCompany"].ToString());
            whPower.PowerName = Request["txt_powername"];
            whPower.PowerDescription = Request["txt_description"];
            whPower.PowerType = Request["txt_powertype"];
            whPower.CreateUser = Session["userName"].ToString();
            whPower.CreateDate = DateTime.Now;
            WCF.AdminService.WhPower result = cf.WhPowerAdd(whPower);
            if (result != null)
            {
                return Helper.RedirectAjax("ok", "添加成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "添加失败，权限已存在！", null, "");
            }
        }


        /// <summary>
        /// 根据当前权限ID 查询出未选择的控制
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult WhPowerMVCUnselected()
        {
            WCF.AdminService.WhPositionPowerMVCSearch search = new WCF.AdminService.WhPositionPowerMVCSearch();
            search.pageIndex = Convert.ToInt32(Request["eip_page"]);
            search.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            search.CompanyId = Convert.ToInt32(Session["whCompany"].ToString());
            search.PowerId = Convert.ToInt32(Request["PowerId"]);
            search.AreaName = Request["AreaName"];
            int total = 0;
            WCF.AdminService.WhPositionPowerMVCResult[] list = cf.WhPowerMVCUnselected(search, out total);

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Id", "Id");
            fieldsName.Add("CompanyId", "公司ID");
            fieldsName.Add("ParentId", "ParentId");
            fieldsName.Add("AreaName", "域");
            fieldsName.Add("ControllerName", "控制器");
            fieldsName.Add("ActionName", "方法");
            fieldsName.Add("HttpMethod", "传值类型");
            fieldsName.Add("PowerId", "PowerId");
            fieldsName.Add("PowerName", "权限名");
            fieldsName.Add("Description", "说明");

            return Content(EIP.EipListJson(list.ToList(), total, fieldsName, "WhCode:60,default:90"));
        }

        /// <summary>
        /// 根据权限ID 查询出已选择的控制
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult WhPoweMVCSelected()
        {
            WCF.AdminService.WhPositionPowerMVCSearch search = new WCF.AdminService.WhPositionPowerMVCSearch();
            search.pageIndex = Convert.ToInt32(Request["eip_page"]);
            search.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            search.CompanyId = Convert.ToInt32(Session["whCompany"].ToString());
            search.PowerId = Convert.ToInt32(Request["PowerId"]);
            int total = 0;
            WCF.AdminService.WhPositionPowerMVCResult[] list = cf.WhPoweMVCSelected(search, out total);

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Action", "操作");
            fieldsName.Add("Id", "Id");
            fieldsName.Add("CompanyId", "公司ID");
            fieldsName.Add("ParentId", "ParentId");
            fieldsName.Add("AreaName", "域");
            fieldsName.Add("ControllerName", "控制器");
            fieldsName.Add("ActionName", "方法");
            fieldsName.Add("HttpMethod", "传值类型");
            fieldsName.Add("PowerId", "PowerId");
            fieldsName.Add("PowerName", "权限名");
            fieldsName.Add("Description", "说明");

            return Content(EIP.EipListJson(list.ToList(), total, fieldsName, "Action:40,WhCode:60,default:90"));
        }

        /// <summary>
        /// 取消权限的某个控制
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult WhPowerMVCDel()
        {
            int id = Convert.ToInt32(Request["Id"]);
            WCF.AdminService.WhPositionPowerMVC whPositionPowerMVC = new WCF.AdminService.WhPositionPowerMVC();
            whPositionPowerMVC.UpdateUser = Session["userName"].ToString();
            whPositionPowerMVC.UpdateDate = DateTime.Now;

            int result1 = cf.PowerMVCUpdateById(whPositionPowerMVC, id, new string[] { "PowerId", "PowerName", "UpdateUser", "UpdateDate" });
            if (result1 > 0)
            {
                return Helper.RedirectAjax("ok", "取消成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "取消失败！", null, "");
            }
        }


        /// <summary>
        /// 添加权限对应的MVC关系
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult WhPowerMVCListAdd()
        {
            string[] Pow_Id = Request.Form.GetValues("chx_Pow");

            string Hid_PId = Request["Hid_PId"];            //当前选择的权限ID
            string Hid_PosName = Request["Hid_PosName"];    //当前选择的权限名

            int result = 0;
            List<WCF.AdminService.WhPositionPower> list = new List<WCF.AdminService.WhPositionPower>(); ;
            for (int i = 0; i < Pow_Id.Length; i++)
            {
                WCF.AdminService.WhPositionPowerMVC whPositionPowerMVC = new WCF.AdminService.WhPositionPowerMVC();
                whPositionPowerMVC.PowerId = Convert.ToInt32(Hid_PId);
                whPositionPowerMVC.PowerName = Hid_PosName;
                whPositionPowerMVC.UpdateUser = Session["userName"].ToString();
                whPositionPowerMVC.UpdateDate = DateTime.Now;

                result = result + cf.PowerMVCUpdateById(whPositionPowerMVC, Convert.ToInt32(Pow_Id[i]), new string[] { "PowerId", "PowerName", "UpdateUser", "UpdateDate" });
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

        /// <summary>
        /// 修改权限信息
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult WhPowerEdit()
        {
            WCF.AdminService.WhPower entity = new WCF.AdminService.WhPower();
            entity.Id = Convert.ToInt32(Request["Id"]);
            entity.CompanyId = Convert.ToInt32(Session["whCompany"].ToString());
            entity.PowerDescription = Request["Description"];
            entity.UpdateUser = Session["userName"].ToString();
            entity.UpdateDate = DateTime.Now;

            int result = cf.WhPowerEdit(entity, new string[] { "PowerDescription", "UpdateUser", "UpdateDate" });
            if (result > 0)
            {
                return Helper.RedirectAjax("ok", "信息修改成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "对不起，修改失败，权限名已存在！", null, "");
            }
        }

        /// <summary>
        /// MVC 域、control及方法同步至数据表WhPositionPowerMVC
        /// </summary>
        /// <returns></returns>
        public ActionResult Sync(AuthorizationContext filterContext)
        {
            List<string> listAreaLimite = new List<string>() { "Admin", "Rec", "Root", "OutBound" };

            int result = 0;

            foreach (var areaName in listAreaLimite)
            {
                string MainPath = Server.MapPath("../../WMS.UI.Controllers/" + areaName);
                MainPath = MainPath.Replace("\\", "/");

                string path = MainPath.Substring(0, MainPath.IndexOf('.'));
                string pathDll = path;
                path = path + ".UI.Controllers/" + areaName;

                DirectoryInfo dir = new DirectoryInfo(path);
                FileInfo[] inf = dir.GetFiles();

                List<WCF.AdminService.WhPositionPowerMVC> list = new List<WCF.AdminService.WhPositionPowerMVC>();
                foreach (FileInfo finf in inf)
                {
                    if (finf.Extension.Equals(".cs"))
                    {
                        //得到control类的名称
                        string name = finf.Name.Substring(0, finf.Name.IndexOf('.'));

                        //加载程序集(dll文件地址)，使用Assembly类   
                        Assembly assembly = Assembly.LoadFile(pathDll + ".UI/Bin/WMS.UI.Controllers.dll");
                        //获取类型，参数（命名空间+类）   
                        Type t = assembly.GetType("WMS.UI.Controllers." + name);
                        //获取所有方法
                        MethodInfo[] mi = t.GetMethods();

                        string controlName = name.Replace("Controller", "");    //替换control类的名称

                        //遍历mi对象数组
                        foreach (MethodInfo m in mi)
                        {
                            if (m.Name.Substring(0, 3) != "get" && m.Name.Substring(0, 3) != "set" && m.Name != "Dispose" && m.Name != "ToString" && m.Name != "Equals" && m.Name != "GetHashCode" && m.Name != "GetType" && m.Name != "Sync")
                            {
                                foreach (Attribute attr in m.GetCustomAttributes())     //循环方法的特性
                                {
                                    //mess = mess + "方法名：" + m.Name + "特性：" + attr + "<br/>";
                                    if (attr.ToString() == "WMS.UI.Controllers.CheckAttribute")
                                    {
                                        WCF.AdminService.WhPositionPowerMVC mvc = new WCF.AdminService.WhPositionPowerMVC();
                                        mvc.CompanyId = Convert.ToInt32(Session["whCompany"].ToString());
                                        mvc.AreaName = areaName;
                                        mvc.ControllerName = controlName;
                                        mvc.ActionName = m.Name;
                                        mvc.HttpMethod = 3;
                                        mvc.CreateUser = Session["userName"].ToString();
                                        mvc.CreateDate = DateTime.Now;
                                        list.Add(mvc);
                                    }
                                }
                            }
                        }
                    }
                }
                result = cf.Sync(list.ToArray());
            }

            if (result > 0)
            {
                return Helper.RedirectAjax("ok", "同步成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "同步失败，请检查代码！", null, "");
            }
        }

    }
}

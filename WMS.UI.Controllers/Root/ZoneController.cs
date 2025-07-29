using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace WMS.UI.Controllers
{
    public class ZoneController : Controller
    {
        WCF.RootService.RootServiceClient cf = new WCF.RootService.RootServiceClient();
        Helper Helper = new Helper();
        EIP.EIP EIP = new EIP.EIP();
        [DefaultRequest]
        public ActionResult Index()
        {
            ViewData["ZoneParentSelect"] = from r in cf.WhZoneParentSelect(Session["whCode"].ToString())
                                           select new SelectListItem()
                                           {
                                               Text = r.ZoneName,     //text
                                               Value = r.Id.ToString()
                                           };
            return View();
        }

        //区域列表
        [HttpGet]
        public ActionResult List()
        {
            WCF.RootService.WhZoneSearch entity = new WCF.RootService.WhZoneSearch();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.WhCode = Session["whCode"].ToString();
            entity.ZoneName = Request["zone_name"].Trim();

            int total = 0;
            List<WCF.RootService.WhZoneResult> list = cf.WhZoneList(entity, out total).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Id", "操作");
            fieldsName.Add("ZoneName", "区域");
            fieldsName.Add("ZoneCBM", "仓库立方");
            fieldsName.Add("Description", "说明");
            fieldsName.Add("ParentId", "父级区域ID");
            fieldsName.Add("ParentZoneName", "父级区域");
            fieldsName.Add("RegShow", "道口登记显示");
            fieldsName.Add("RegFlag", "道口登记Flag");
            return Content(EIP.EipListJson(list, total, fieldsName, "Id:90,default:120"));
        }

        //新增区域
        [HttpGet]
        public ActionResult AddZone()
        {
            WCF.RootService.Zone entity = new WCF.RootService.Zone();
            entity.WhCode = Session["whCode"].ToString();
            entity.ZoneName = Request["txt_zone"].Trim();

            if (Request["txt_warnCBM"] == "")
            {
                entity.ZoneCBM = 0;
            }
            else
            {
                entity.ZoneCBM = Convert.ToDecimal(Request["txt_warnCBM"]);
            }

            entity.Description = Request["txt_description"].Trim();
            if (Request["txt_upId"] == "")
            {
                entity.UpId = 0;
            }
            else
            {
                entity.UpId = Convert.ToInt32(Request["txt_upId"]);
            }
            entity.RegFlag = Convert.ToInt32(Request["sel_RegFlag"]);
            entity.CreateUser = Session["userName"].ToString();
            entity.CreateDate = DateTime.Now;

            WCF.RootService.Zone result = cf.WhZoneAdd(entity);
            if (result != null)
            {
                return Helper.RedirectAjax("ok", "添加成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "对不起，添加失败，区域已存在！", null, "");
            }
        }


        //根据当前区域查询出未选择的库位信息
        //对应 ZoneController中的 LocationUnselected 方法
        [HttpGet]
        public ActionResult LocationUnselected()
        {
            WCF.RootService.ZoneLocationSearch entity = new WCF.RootService.ZoneLocationSearch();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.Location = Request["Location"].Trim();
            entity.WhCode = Session["whCode"].ToString();
            entity.ZoneId = Convert.ToInt32(Request["ZoneId"]);
            entity.Location = Request["Location"].Trim();
            int total = 0;
            WCF.RootService.ZoneLocationResult[] list = cf.LocationUnselected(entity, out total);

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Location", "储位库区");
            fieldsName.Add("WhCode", "仓库");

            return Content(EIP.EipListJson(list.ToList(), total, fieldsName, "WhCode:60,default:90"));
        }


        //根据当前区域查询出已选择的库位信息
        //对应 ZoneController中的 LocationSelected 方法
        [HttpGet]
        public ActionResult LocationSelected()
        {
            WCF.RootService.ZoneLocationSearch entity = new WCF.RootService.ZoneLocationSearch();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.WhCode = Session["whCode"].ToString();
            entity.ZoneId = Convert.ToInt32(Request["ZoneId"]);

            int total = 0;
            WCF.RootService.ZoneLocationResult[] list = cf.LocationSelected(entity, out total);

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Id", "操作");
            fieldsName.Add("Location", "储位库区");
            fieldsName.Add("WhCode", "仓库");

            return Content(EIP.EipListJson(list.ToList(), total, fieldsName, "Id:45,WhCode:60,default:90"));
        }


        //新增区域对应库位关系
        [HttpPost]
        public ActionResult ZoneLocationAdd()
        {
            string[] Location = Request.Form.GetValues("chx_Pow");

            List<WCF.RootService.ZoneLocation> list = new List<WCF.RootService.ZoneLocation>();

            for (int i = 0; i < Location.Length; i++)
            {
                WCF.RootService.ZoneLocation entity = new WCF.RootService.ZoneLocation();
                entity.WhCode = Session["whCode"].ToString();
                entity.ZoneId = Convert.ToInt32(Request["ZoneId"]);
                entity.Location = Location[i].ToString().Trim();
                entity.CreateUser = Session["userName"].ToString();
                entity.CreateDate = DateTime.Now;
                list.Add(entity);
            }

            int result = cf.ZoneLocationAdd(list.ToArray());
            if (result > 0)
            {
                return Helper.RedirectAjax("ok", "添加成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "对不起，添加失败！", null, "");
            }
        }


        //取消区域库位关系
        [HttpGet]
        public ActionResult ZoneLocationById()
        {
            int id = Convert.ToInt32(Request["Id"]);
            int result = cf.ZoneLocationDelById(id);
            return Helper.RedirectAjax("ok", "删除成功！", null, "");

        }

        //修改区域的父级信息
        [HttpGet]
        public ActionResult WhZoneParentEdit()
        {
            int id = Convert.ToInt32(Request["Id"]);
            WCF.RootService.Zone entity = new WCF.RootService.Zone();
            entity.Id = id;
            if (Request["parent_id"] == "")
            {
                entity.UpId = 0;
            }
            else
            {
                entity.UpId = Convert.ToInt32(Request["parent_id"]);
            }
            if (Request["edit_warnCBM"] == "")
            {
                entity.ZoneCBM = 0;
            }
            else
            {
                entity.ZoneCBM = Convert.ToDecimal(Request["edit_warnCBM"]);
            }

            if (Request["edit_RegFlag"] != "")
            {
                if (Request["edit_RegFlag"] == "null")
                {
                    entity.RegFlag = 0;
                }
                else
                {
                    entity.RegFlag = Convert.ToInt32(Request["edit_RegFlag"]);
                }
            }


            entity.UpdateUser = Session["userName"].ToString();
            entity.UpdateDate = DateTime.Now;

            int result = cf.WhZoneParentEdit(entity, new string[] { "UpId", "RegFlag", "ZoneCBM", "UpdateUser", "UpdateDate" });
            if (result > 0)
            {
                return Helper.RedirectAjax("ok", "修改成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "对不起，修改失败！", null, "");
            }
        }

        [HttpGet]
        public ActionResult DelZone()
        {
            int id = Convert.ToInt32(Request["Id"]);
            List<int?> list = new List<int?>();
            list.Add(id);

            string result = cf.WhZoneBatchDel(list.ToArray());
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "删除成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }
        }

        public ActionResult imports()
        {
            string[] zone = Request.Form.GetValues("区域");
            string[] zonecbm = Request.Form.GetValues("仓库立方");
            string[] description = Request.Form.GetValues("说明");
            string[] upzonename = Request.Form.GetValues("父级区域");
            string[] regflag = Request.Form.GetValues("道口登记显示");

            Hashtable hash = new Hashtable();
            string mess = "";

            if (zone == null)
            {
                return Helper.RedirectAjax("err", "请先复制数据再导入！", null, "");
            }
            if (zone.Length > 1000)
            {
                return Helper.RedirectAjax("err", "每次导入量不能超过1000条！", null, "");
            }

            Hashtable data = new Hashtable();   //去excel重复的库位
            string error = "";
            for (int i = 0; i < zone.Length; i++)
            {
                if (regflag.Length > 0)
                {
                    if (regflag[i] != "Y" && regflag[i] != "N")
                    {
                        error = "道口登记显示应填写Y/N";
                    }
                }
            }

            if (error != "")
            {
                return Helper.RedirectAjax("err", error, null, "");
            }

            List<WCF.RootService.ZoneResult> list = new List<WCF.RootService.ZoneResult>();
            for (int i = 0; i < zone.Length; i++)
            {
                if (!hash.ContainsValue(zone[i].ToString()))
                {
                    hash.Add(i, zone[i].ToString());
                    WCF.RootService.ZoneResult entity = new WCF.RootService.ZoneResult();
                    entity.WhCode = Session["whCode"].ToString();
                    entity.ZoneName = zone[i].ToString().Trim();
                    entity.Description = description[i].ToString().Trim();
                    entity.UpZoneName = upzonename[i].ToString().Trim();
                    entity.RegFlag = regflag[i].ToString();
                    entity.CreateUser = Session["userName"].ToString();
                    if (!string.IsNullOrEmpty(zonecbm[i]))
                    {
                        entity.ZoneCBM = Convert.ToDecimal(zonecbm[i].ToString());
                    }

                    list.Add(entity);
                }
                else
                {
                    mess += "区域重复：" + zone[i].ToString() + "<br/>";
                }
            }

            if (mess != "")
            {
                return Helper.RedirectAjax("err", "导入失败！<br/>" + mess, null, "");
            }

            string result = cf.ZoneImports(list.ToArray());
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "导入成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }

        }

        [HttpPost]
        public ActionResult BatchDelZone()
        {
            string[] idarr = Request.Form.GetValues("idarr");

            List<int?> list = new List<int?>();
            foreach (var item in idarr)
            {
                list.Add(Convert.ToInt32(item));
            }
            string result = cf.WhZoneBatchDel(list.ToArray());
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "删除成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }
        }

    }
}

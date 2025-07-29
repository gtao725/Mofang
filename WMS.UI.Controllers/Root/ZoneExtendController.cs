using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace WMS.UI.Controllers
{
    public class ZoneExtendController : Controller
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
            List<WCF.RootService.ZoneExtendResult> list = cf.ZonesExtendList(entity, out total).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Id", "操作");
            fieldsName.Add("ZoneName", "区域");
            fieldsName.Add("ZoneId", "ZoneId");
            fieldsName.Add("ZoneOrderBy", "排列号");
            fieldsName.Add("OnlySkuFlag", "OnlySkuFlag");
            fieldsName.Add("OnlySkuShow", "是否混合区");
            fieldsName.Add("MaxLocationIdQty", "最大库位数");
            fieldsName.Add("MaxPallateQty", "最大托盘数");
            return Content(EIP.EipListJson(list, total, fieldsName, "Id:60,default:80"));
        }

        //新增区域
        [HttpGet]
        public ActionResult AddZonesExtendAdd()
        {
            WCF.RootService.ZonesExtend entity = new WCF.RootService.ZonesExtend();
            entity.WhCode = Session["whCode"].ToString();
            entity.ZoneId = Convert.ToInt32(Request["txt_zone_name"].Trim());

            entity.ZoneOrderBy = Convert.ToInt32(Request["txt_zoneOrderBy"]);
            entity.OnlySkuFlag = Convert.ToInt32(Request["sel_OnlySkuFlag"]);
            entity.MaxLocationIdQty = Convert.ToInt32(Request["txt_maxLocationIdQty"]);
            entity.MaxPallateQty = Convert.ToInt32(Request["txt_maxPallateQty"]);

            entity.CreateUser = Session["userName"].ToString();

            WCF.RootService.ZonesExtend result = cf.ZonesExtendAdd(entity);
            if (result != null)
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
        public ActionResult DelZoneExtend()
        {
            int id = Convert.ToInt32(Request["Id"]);
            int result = cf.ZonesExtendDel(id);
            return Helper.RedirectAjax("ok", "删除成功！", null, "");

        }

        [HttpGet]
        public ActionResult EditZoneExtend()
        {
            WCF.RootService.ZonesExtend entity = new WCF.RootService.ZonesExtend();
            entity.Id = Convert.ToInt32(Request["id"].Trim());
            entity.ZoneOrderBy = Convert.ToInt32(Request["edit_ZoneOrderBy"].Trim());
            entity.OnlySkuFlag = Convert.ToInt32(Request["edit_OnlySkuFlag"].Trim());
            entity.MaxLocationIdQty = Convert.ToInt32(Request["edit_MaxLocationIdQty"].Trim());
            entity.MaxPallateQty = Convert.ToInt32(Request["edit_MaxPallateQty"].Trim());

            entity.UpdateUser = Session["userNameCN"].ToString();

            int result = cf.ZonesExtendEdit(entity);
            if (result > 0)
            {
                return Helper.RedirectAjax("ok", "修改成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "修改失败！", null, "");
            }
        }

        [HttpPost]
        public ActionResult BatchDelZoneExtend()
        {
            string[] idarr = Request.Form.GetValues("idarr");

            List<int?> list = new List<int?>();
            foreach (var item in idarr)
            {
                list.Add(Convert.ToInt32(item));
            }
            string result = cf.ZonesExtendBatchDel(list.ToArray());
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
            string[] zoneorderby = Request.Form.GetValues("排列号");
            string[] onlyskuflag = Request.Form.GetValues("是否混合区");
            string[] maxlocationqty = Request.Form.GetValues("最大库位数");
            string[] maxpallateqty = Request.Form.GetValues("最大托盘数");

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
                if (onlyskuflag.Length > 0)
                {
                    if (onlyskuflag[i] != "Y" && onlyskuflag[i] != "N")
                    {
                        error = "是否混合区应填写Y/N";
                    }
                }
            }

            if (error != "")
            {
                return Helper.RedirectAjax("err", error, null, "");
            }

            List<WCF.RootService.ZoneExtendResult> list = new List<WCF.RootService.ZoneExtendResult>();
            for (int i = 0; i < zone.Length; i++)
            {
                if (!hash.ContainsValue(zone[i].ToString()))
                {
                    hash.Add(i, zone[i].ToString());
                    WCF.RootService.ZoneExtendResult entity = new WCF.RootService.ZoneExtendResult();
                    entity.WhCode = Session["whCode"].ToString();
                    entity.ZoneName = zone[i].ToString().Trim();

                    if (!string.IsNullOrEmpty(zoneorderby[i]))
                    {
                        entity.ZoneOrderBy = Convert.ToInt32(zoneorderby[i].ToString());
                    }
                    else
                    {
                        entity.ZoneOrderBy = 0;
                    }

                    entity.OnlySkuShow = onlyskuflag[i].ToString().Trim();

                    if (!string.IsNullOrEmpty(maxlocationqty[i]))
                    {
                        entity.MaxLocationIdQty = Convert.ToInt32(maxlocationqty[i].ToString());
                    }
                    else
                    {
                        entity.MaxLocationIdQty = 0;
                    }

                    if (!string.IsNullOrEmpty(maxpallateqty[i]))
                    {
                        entity.MaxPallateQty = Convert.ToInt32(maxpallateqty[i].ToString());
                    }
                    else
                    {
                        entity.MaxPallateQty = 0;
                    }

                    entity.CreateUser = Session["userName"].ToString();

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

            string result = cf.ZoneExtendImports(list.ToArray());
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "导入成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }

        }

    }
}

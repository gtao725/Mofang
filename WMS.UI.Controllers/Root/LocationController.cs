using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace WMS.UI.Controllers
{
    public class LocationController : Controller
    {
        WCF.RootService.RootServiceClient cf = new WCF.RootService.RootServiceClient();
        Helper Helper = new Helper();
        EIP.EIP EIP = new EIP.EIP();
        [DefaultRequest]
        public ActionResult Index()
        {
            ViewData["LocationType"] = from r in cf.LocationTypeSelect()
                                       select new SelectListItem()
                                       {
                                           Text = r.Description,     //text
                                           Value = r.Id.ToString()
                                       };

            ViewData["ZoneSelect"] = from r in cf.ZoneSelect(Session["whCode"].ToString())
                                     select new SelectListItem()
                                     {
                                         Text = r.ZoneName,     //text
                                         Value = r.Id.ToString()
                                     };

            ViewData["LocationSelect"] = from r in cf.LocationSelect(Session["whCode"].ToString())
                                         select new SelectListItem()
                                         {
                                             Text = r.Location,     //text
                                             Value = r.Location.ToString()
                                         };

            return View();
        }

        //储位列表
        [HttpGet]
        public ActionResult List()
        {
            WCF.RootService.WhLocationSearch entity = new WCF.RootService.WhLocationSearch();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.WhCode = Session["whCode"].ToString();
            entity.LocationId = Request["location_name"].Trim();
            entity.LocationTypeId = Request["SelLocationType"];

            if (!string.IsNullOrEmpty(Request["SelZone"]))
            {
                entity.ZoneId = Convert.ToInt32(Request["SelZone"]);
            }

            int total = 0;
            List<WCF.RootService.WhLocationResult> list = cf.WhLocationList(entity, out total).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Id", "操作");
            fieldsName.Add("WhCode", "仓库");
            fieldsName.Add("LocationId", "储位");
            fieldsName.Add("MaxPltQty", "最大托盘数");
            fieldsName.Add("ZoneId", "区域ID");
            fieldsName.Add("ZoneName", "区域");
            fieldsName.Add("Location", "库区");
            fieldsName.Add("LocationTypeId", "储位类型ID");
            fieldsName.Add("LocationDescription", "储位类型");
            fieldsName.Add("CreateUser", "创建人");
            fieldsName.Add("CreateDate", "创建时间");

            return Content(EIP.EipListJson(list, total, fieldsName, "Id:60,CreateDate:130,default:80"));
        }

        //批量导入储位
        public ActionResult imports()
        {
            string[] location = Request.Form.GetValues("储位");
            string[] zoneName = Request.Form.GetValues("区域");
            string[] MaxPltQty = Request.Form.GetValues("最大托盘数");
            Hashtable hash = new Hashtable();
            string mess = "";

            if (location == null)
            {
                return Helper.RedirectAjax("err", "请先复制数据再导入！", null, "");
            }
            if (location.Length > 1000)
            {
                return Helper.RedirectAjax("err", "每次导入量不能超过1000条！", null, "");
            }

            List<WCF.RootService.LocationResult> list = new List<WCF.RootService.LocationResult>();
            for (int i = 0; i < location.Length; i++)
            {
                if (!hash.ContainsValue(location[i].ToString()))
                {
                    hash.Add(i, location[i].ToString());
                    WCF.RootService.LocationResult entity = new WCF.RootService.LocationResult();
                    entity.WhCode = Session["whCode"].ToString();
                    entity.LocationId = location[i].ToString().Trim();
                    entity.ZoneName = zoneName[i].ToString().Trim();
                    entity.Location = zoneName[i].ToString().Trim();
                    entity.CreateUser = Session["userName"].ToString();
                    if (MaxPltQty != null)
                    {
                        if (!string.IsNullOrEmpty(MaxPltQty[i]))
                            entity.MaxPltQty = Convert.ToInt32(MaxPltQty[i]);
                    }
                    else
                    {
                        entity.MaxPltQty = 0;
                    }

                    list.Add(entity);
                }
                else
                {
                    mess += "储位重复：" + location[i].ToString() + "<br/>";
                }
            }

            if (mess != "")
            {
                return Helper.RedirectAjax("err", "导入失败！<br/>" + mess, null, "");
            }

            string result = cf.LocationImports(list.ToArray());
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "导入成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }

        }

        [HttpGet]
        public ActionResult AddRecLocation()
        {
            WCF.RootService.WhLocation entity = new WCF.RootService.WhLocation();
            entity.WhCode = Session["whCode"].ToString();
            entity.LocationId = Request["txt_reclocation"];
            entity.Status = "A";
            if (Request["txt_zone"] == "")
            {
                entity.ZoneId = 0;
            }
            else
            {
                entity.ZoneId = Convert.ToInt32(Request["txt_zone"]);
            }

            entity.LocationTypeId = 2;
            entity.CreateUser = Session["userName"].ToString();
            entity.CreateDate = DateTime.Now;

            WCF.RootService.WhLocation result = cf.LocationAdd(entity);
            if (result != null)
            {
                return Helper.RedirectAjax("ok", "添加成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "对不起，添加失败，储位名已存在！", null, "");
            }
        }

        [HttpGet]
        public ActionResult AddOutLocation()
        {
            WCF.RootService.WhLocation entity = new WCF.RootService.WhLocation();
            entity.WhCode = Session["whCode"].ToString();
            entity.LocationId = Request["txt_outlocation"].Trim();
            entity.Status = "A";
            if (Request["txt_zone"] == "")
            {
                entity.ZoneId = 0;
            }
            else
            {
                entity.ZoneId = Convert.ToInt32(Request["txt_zone"]);
            }
            entity.LocationTypeId = 3;
            entity.CreateUser = Session["userName"].ToString();
            entity.CreateDate = DateTime.Now;

            WCF.RootService.WhLocation result = cf.LocationAdd(entity);
            if (result != null)
            {
                return Helper.RedirectAjax("ok", "添加成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "对不起，添加失败，储位名已存在！", null, "");
            }
        }

        //按照规则生成储位
        [HttpGet]
        public ActionResult AddLocation()
        {
            string locationResult = "";

            string beginLocationArray = Request["txt_beginLocation"];
            char[] beginLocationColumnArray = Request["txt_beginLocationColumn1"].ToCharArray();
            char[] endLocationColumnArray = Request["txt_endLocationColumn1"].ToCharArray();
            char[] beginLocationColumnArray2 = Request["txt_beginLocationColumn2"].ToCharArray();
            char[] endLocationColumnArray2 = Request["txt_endLocationColumn2"].ToCharArray();

            int beginLocationRow = Convert.ToInt32(Request["txt_beginLocationRow"]);
            int endLocationRow = Convert.ToInt32(Request["txt_endLocationRow"]);

            int LocationFloor = Convert.ToInt32(Request["txt_LocationFloor"]);
            int LocationPcs = Convert.ToInt32(Request["txt_LocationPcs"]);

            if (beginLocationColumnArray.Length != endLocationColumnArray.Length)
            {
                return Helper.RedirectAjax("err", "通道长度必须一致：" + Request["txt_beginLocationColumn1"] + "-" + Request["txt_endLocationColumn1"], null, "");
            }
            if (beginLocationColumnArray2.Length != endLocationColumnArray2.Length)
            {
                return Helper.RedirectAjax("err", "通道长度必须一致：" + Request["txt_beginLocationColumn2"] + "-" + Request["txt_endLocationColumn2"], null, "");
            }

            int CheckBegin = 0, CheckEnd = 0;
            //验证通道格式 65-91为A-Z 48-57为0-9
            for (int i = 0; i < beginLocationColumnArray.Length; i++)
            {
                if (beginLocationColumnArray[i] != endLocationColumnArray[i])
                {
                    CheckBegin = 1;
                }
                if (Convert.ToInt32(beginLocationColumnArray[i]) >= 65 && Convert.ToInt32(beginLocationColumnArray[i]) <= 91)
                {
                    if (Convert.ToInt32(endLocationColumnArray[i]) >= 65 && Convert.ToInt32(endLocationColumnArray[i]) <= 91)
                    {
                        if (Convert.ToInt32(beginLocationColumnArray[i]) > Convert.ToInt32(endLocationColumnArray[i]))
                        {
                            locationResult = "通道位数必须由小到大：" + beginLocationColumnArray[i].ToString() + "-" + endLocationColumnArray[i].ToString();
                        }
                    }
                    else
                    {
                        locationResult = "通道格式不正确：" + Request["txt_beginLocationColumn1"] + "-" + Request["txt_endLocationColumn1"];
                    }
                }
                else
                {
                    if (Convert.ToInt32(endLocationColumnArray[i]) >= 48 && Convert.ToInt32(endLocationColumnArray[i]) <= 57)
                    {
                        if (Convert.ToInt32(beginLocationColumnArray[i]) > Convert.ToInt32(endLocationColumnArray[i]))
                        {
                            locationResult = "通道位数必须由小到大：" + beginLocationColumnArray[i].ToString() + "-" + endLocationColumnArray[i].ToString();
                        }
                    }
                    else
                    {
                        locationResult = "通道格式不正确：" + Request["txt_beginLocationColumn1"] + "-" + Request["txt_endLocationColumn1"];
                    }
                }
            }
            if (locationResult != "")
            {
                return Helper.RedirectAjax("err", locationResult, null, "");
            }

            //验证通道格式 65-91为A-Z 48-57为0-9
            for (int i = 0; i < beginLocationColumnArray2.Length; i++)
            {
                if (beginLocationColumnArray2[i] != endLocationColumnArray2[i])
                {
                    CheckEnd = 1;
                }
                if (Convert.ToInt32(beginLocationColumnArray2[i]) >= 65 && Convert.ToInt32(beginLocationColumnArray2[i]) <= 91)
                {
                    if (Convert.ToInt32(endLocationColumnArray2[i]) >= 65 && Convert.ToInt32(endLocationColumnArray2[i]) <= 91)
                    {
                        if (Convert.ToInt32(beginLocationColumnArray2[i]) > Convert.ToInt32(endLocationColumnArray2[i]))
                        {
                            locationResult = "通道位数必须由小到大：" + beginLocationColumnArray2[i].ToString() + "-" + endLocationColumnArray2[i].ToString();
                        }
                    }
                    else
                    {
                        locationResult = "通道格式不正确：" + Request["txt_beginLocationColumn2"] + "-" + Request["txt_endLocationColumn2"];
                    }
                }
                else
                {
                    if (Convert.ToInt32(endLocationColumnArray2[i]) >= 48 && Convert.ToInt32(endLocationColumnArray2[i]) <= 57)
                    {
                        if (Convert.ToInt32(beginLocationColumnArray2[i]) > Convert.ToInt32(endLocationColumnArray2[i]))
                        {
                            locationResult = "通道位数必须由小到大：" + beginLocationColumnArray2[i].ToString() + "-" + endLocationColumnArray2[i].ToString();
                        }
                    }
                    else
                    {
                        locationResult = "通道格式不正确：" + Request["txt_beginLocationColumn2"] + "-" + Request["txt_endLocationColumn2"];
                    }
                }
            }
            if (locationResult != "")
            {
                return Helper.RedirectAjax("err", locationResult, null, "");
            }

            //验证通道起始值填写 是否为多次循环
            if (CheckBegin != 0 && CheckEnd != 0)
            {
                return Helper.RedirectAjax("err", "通道格式有误！<br/>举例：A01-A11 或01A-11A！<br/> 或：1A-9A", null, "");
            }

            if (beginLocationColumnArray.Length > 1)
            {
                for (int i = 0; i < beginLocationColumnArray.Length; i++)
                {
                    if (Convert.ToInt32(beginLocationColumnArray[i]) >= 65)
                    {
                        locationResult = "填写字母后不能再填写任何内容！" + Request["txt_beginLocationColumn1"];
                    }
                }
            }
            if (locationResult != "")
            {
                return Helper.RedirectAjax("err", locationResult, null, "");
            }

            if (beginLocationColumnArray2.Length > 1)
            {
                for (int i = 0; i < beginLocationColumnArray2.Length; i++)
                {
                    if (Convert.ToInt32(beginLocationColumnArray2[i]) >= 65)
                    {
                        locationResult = "填写字母后不能再填写任何内容！" + Request["txt_beginLocationColumn2"];
                    }
                }
            }
            if (locationResult != "")
            {
                return Helper.RedirectAjax("err", locationResult, null, "");
            }

            if (Convert.ToInt32(beginLocationColumnArray[0]) >= 48 && Convert.ToInt32(beginLocationColumnArray[0]) <= 57)
            {
                if (Convert.ToInt32(Request["txt_endLocationColumn1"]) - Convert.ToInt32(Request["txt_beginLocationColumn1"]) > 10)
                {
                    return Helper.RedirectAjax("err", "每次生成的通道数不能超过10！", null, "");
                }
            }
            else
            {
                if (Convert.ToInt32(endLocationColumnArray[0]) - Convert.ToInt32(beginLocationColumnArray[0]) > 10)
                {
                    return Helper.RedirectAjax("err", "每次生成的通道数不能超过10！", null, "");
                }
            }

            if (beginLocationColumnArray2.Length > 0)
            {
                if (Convert.ToInt32(beginLocationColumnArray2[0]) >= 48 && Convert.ToInt32(beginLocationColumnArray2[0]) <= 57)
                {
                    if (Convert.ToInt32(Request["txt_endLocationColumn2"]) - Convert.ToInt32(Request["txt_beginLocationColumn2"]) > 10)
                    {
                        return Helper.RedirectAjax("err", "每次生成的通道数不能超过10！", null, "");
                    }
                }
                else
                {
                    if (Convert.ToInt32(endLocationColumnArray2[0]) - Convert.ToInt32(beginLocationColumnArray2[0]) > 10)
                    {
                        return Helper.RedirectAjax("err", "每次生成的通道数不能超过10！", null, "");
                    }
                }
            }

            if (endLocationRow - beginLocationRow > 20)
            {
                return Helper.RedirectAjax("err", "每次生成的列数不能超过20！", null, "");
            }
            if (LocationFloor > 15)
            {
                return Helper.RedirectAjax("err", "每次生成的层数不能超过15！", null, "");
            }

            if (CheckBegin == 0 && CheckEnd == 0)
            {
                CheckBegin = 1;
            }

            int result = cf.AddLocation(beginLocationArray.Trim(), Request["txt_beginLocationColumn1"].Trim(), Request["txt_endLocationColumn1"].Trim(), Request["txt_beginLocationColumn2"].Trim(), Request["txt_endLocationColumn2"].Trim(), beginLocationRow, endLocationRow, LocationFloor, LocationPcs, CheckBegin, Session["whCode"].ToString(), Session["userName"].ToString());
            if (result > 0)
            {
                return Helper.RedirectAjax("ok", "添加成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "对不起，添加失败，储位名已存在！", null, "");
            }
        }

        //批量修改储位
        [HttpPost]
        public ActionResult EditOutLocation()
        {
            int locationTypeId = Convert.ToInt32(Request["locationTypeId"]);
            string[] id = Request.Form.GetValues("id");
            List<WCF.RootService.WhLocation> list = new List<WCF.RootService.WhLocation>();

            for (int i = 0; i < id.Length; i++)
            {
                WCF.RootService.WhLocation entity = new WCF.RootService.WhLocation();
                entity.Id = Convert.ToInt32(id[i].ToString());
                entity.LocationTypeId = locationTypeId;
                list.Add(entity);
            }
            string result = cf.LocationEdit(list.ToArray());

            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "修改成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "修改失败！", null, "");
            }
        }

        [HttpGet]
        public ActionResult DelLocation()
        {
            int id = Convert.ToInt32(Request["Id"]);
            List<int?> list = new List<int?>();
            list.Add(id);

            string result = cf.WhLocationBatchDel(list.ToArray());
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "删除成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }
        }

        [HttpPost]
        public ActionResult BatchDelLocation()
        {
            string[] idarr = Request.Form.GetValues("idarr");

            List<int?> list = new List<int?>();
            foreach (var item in idarr)
            {
                list.Add(Convert.ToInt32(item));
            }
            string result = cf.WhLocationBatchDel(list.ToArray());
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "删除成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }
        }

        [HttpGet]
        public ActionResult EditLocationId()
        {
            int id = Convert.ToInt32(Request["Id"]);
            WCF.RootService.WhLocation entity = new WCF.RootService.WhLocation();
            entity.Id = id;
            entity.WhCode = Session["whCode"].ToString();
            if (Request["edit_ZoneId"] == "")
            {
                entity.ZoneId = 0;
            }
            else
            {
                entity.ZoneId = Convert.ToInt32(Request["edit_ZoneId"]);
            }

            if (Request["edit_LocationTypeId"] == "")
            {
                entity.LocationTypeId = 0;
            }
            else
            {
                entity.LocationTypeId = Convert.ToInt32(Request["edit_LocationTypeId"]);
            }

            entity.LocationId = Request["edit_LocationId"];
            entity.Location = Request["edit_Location"];
            entity.UpdateUser = Session["userName"].ToString();

            int result = cf.WhLocationEdit(entity);
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
        public ActionResult SetAbLocationId()
        {
            string abLocationId = Request["txt_setABLocationId"];
            string result = cf.SetWhLocationLookUp(Session["whCode"].ToString(), abLocationId);

            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "设置成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }
        }

        [HttpPost]
        public void GetAbLocationId()
        {
            string abLocationId = cf.GetWhLocationLookUp(Session["whCode"].ToString());

            Response.Write(abLocationId);

        }

    }
}

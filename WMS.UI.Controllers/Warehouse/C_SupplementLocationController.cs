using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace WMS.UI.Controllers
{
    public class C_SupplementLocationController : Controller
    {
        WCF.InBoundService.InBoundServiceClient inboundcf = new WCF.InBoundService.InBoundServiceClient();
        WCF.RootService.RootServiceClient cf = new WCF.RootService.RootServiceClient();

        Helper Helper = new Helper();
        EIP.EIP EIP = new EIP.EIP();

        [DefaultRequest]
        public ActionResult Index()
        {
            ViewData["WhClientList"] = from r in inboundcf.WhClientListSelect(Session["whCode"].ToString())
                                       select new SelectListItem()
                                       {
                                           Text = r.ClientCode,     //text
                                           Value = r.ClientCode
                                       };
            return View();
        }

        [HttpGet]
        public ActionResult List()
        {
            WCF.RootService.WhLocationSearch entity = new WCF.RootService.WhLocationSearch();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.WhCode = Session["whCode"].ToString();
            entity.LocationId = Request["locationId"];

            int total = 0;
            List<WCF.RootService.WhLocationResult> list = cf.SupplementLocationList(entity, out total).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Id", "批量删除");
            fieldsName.Add("LocationId", "捡货库位");
            fieldsName.Add("Status", "状态");
            fieldsName.Add("LocationDescription", "储位类型");
            fieldsName.Add("CreateUser", "创建人");
            fieldsName.Add("CreateDate", "创建时间");

            return Content(EIP.EipListJson(list, total, fieldsName, "Id:60,CreateDate:120,default:100"));
        }


        [HttpPost]
        public ActionResult ImportSupplementLocation()
        {
            string[] locationId = Request.Form.GetValues("库位");

            if (locationId == null)
            {
                return Helper.RedirectAjax("err", "请先复制数据再导入！", null, "");
            }
            if (locationId.Count() > 500)
            {
                return Helper.RedirectAjax("err", "每次导入量不能超过500条！", null, "");
            }

            Hashtable data = new Hashtable();   //去excel重复的库位

            //清除excel表中的数据
            string errorItemNumber = ""; //插入失败的款号
            int k = 0;
            for (int i = 0; i < locationId.Length; i++)
            {
                if (!data.ContainsValue(locationId[i].ToString()))//Ecxel是否存在重复的值 不存在 add 
                {
                    data.Add(k, locationId[i].ToString());
                    k++;
                }
                else
                {
                    errorItemNumber = "库位:" + locationId[i].ToString();
                }
            }

            if (errorItemNumber != "")
            {
                return Helper.RedirectAjax("err", "导入数据重复！" + errorItemNumber, null, "");
            }

            List<WCF.RootService.WhLocation> entityList = new List<WCF.RootService.WhLocation>();

            //构造批量导入实体
            for (int i = 0; i < locationId.Length; i++)
            {
                WCF.RootService.WhLocation entity = new WCF.RootService.WhLocation();
                entity.WhCode = Session["whCode"].ToString();
                entity.LocationId = locationId[i].Trim();
                entity.Status = "A";
                entity.LocationTypeId = 1;
                entity.LocationTypeDetailId = 1;
                entity.CreateUser = Session["userName"].ToString();
                entityList.Add(entity);
            }

            int result = cf.ImportSupplementLocation(entityList.ToArray());
            if (result > 0)
            {
                return Helper.RedirectAjax("ok", "导入成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "导入失败！", null, "");
            }

        }

        [HttpPost]
        public ActionResult SupplementLocationDel()
        {
            string[] Pow_Id = Request.Form.GetValues("chx_Pow");

            List<WCF.RootService.WhLocation> entityList = new List<WCF.RootService.WhLocation>();
            for (int i = 0; i < Pow_Id.Length; i++)
            {
                int Id = Convert.ToInt32(Pow_Id[i].Split('-')[0]);
                string LocationId = Pow_Id[i].Split('-')[1];

                WCF.RootService.WhLocation entity = new WCF.RootService.WhLocation();
                entity.WhCode= Session["whCode"].ToString();
                entity.Id = Convert.ToInt32(Id);
                entity.LocationId = LocationId;

                entityList.Add(entity);
            }

            string result = cf.SupplementLocationDel(entityList.ToArray());
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "批量删除成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }
        }


    }
}

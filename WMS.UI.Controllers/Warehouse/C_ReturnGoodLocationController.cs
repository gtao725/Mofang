using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace WMS.UI.Controllers
{
    public class C_ReturnGoodLocationController : Controller
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

        [DefaultRequest]
        public ActionResult Index1()
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
            List<WCF.RootService.WhLocationResult> list = cf.ReturnGoodLocationList(entity, out total).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Id", "批量删除");
            fieldsName.Add("LocationId", "退货上架库位");
            fieldsName.Add("Status", "状态");
            fieldsName.Add("LocationDescription", "储位类型");

            return Content(EIP.EipListJson(list, total, fieldsName, "Id:60,CreateDate:120,default:120"));
        }


        [HttpPost]
        public ActionResult ImportReturnGoodLocation()
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
                entity.LocationTypeDetailId = 2;
                entity.CreateUser = Session["userName"].ToString();
                entityList.Add(entity);
            }

            int result = cf.ImportReturnGoodLocation(entityList.ToArray());
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
        public ActionResult ReturnGoodLocationDel()
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

            string result = cf.ReturnGoodLocationDel(entityList.ToArray());
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "批量删除成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }
        }


        [HttpGet]
        //库存问题信息查询
        public ActionResult List1()
        {
            WCF.RootService.R_Location_ItemSearch entity = new WCF.RootService.R_Location_ItemSearch();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.WhCode = Session["whCode"].ToString();
            entity.ClientCode = Request["WhClientId"];
            entity.LocationId = Request["locationId"].Trim();
            entity.AltItemNumber = Request["altItemNumber"].Trim();
            entity.Status = Request["Status"];

            int total = 0;
            List<WCF.RootService.R_Location_ItemResult> list = cf.R_Location_ItemRGList(entity, out total).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Id", "操作");
            fieldsName.Add("ClientCode", "客户名");
            fieldsName.Add("LocationId", "退货上架库位");
            fieldsName.Add("AltItemNumber", "款号");
            fieldsName.Add("MaxQty", "上限数量");
            fieldsName.Add("LotNumber1", "Lot1");
            fieldsName.Add("Status", "状态");
            fieldsName.Add("StatusShow", "状态名");
            fieldsName.Add("CreateUser", "创建人");
            fieldsName.Add("CreateDate", "创建时间");
            fieldsName.Add("UpdateUser", "更新人");
            fieldsName.Add("UpdateDate", "更新时间");

            return Content(EIP.EipListJson(list, total, fieldsName, "Id:60,MinQty:70,MaxQty:70,UnitName:65,AltItemNumber:120,CreateDate:120,default:100"));
        }



        [HttpPost]
        public ActionResult ImportsR_Location_Item_RG()
        {
            string[] clientCode = Request.Form.GetValues("客户名");
            string[] locationId = Request.Form.GetValues("退货上架库位");
            string[] altItemNumber = Request.Form.GetValues("款号");
            string[] lot1 = Request.Form.GetValues("lot1");
            string[] maxQty = Request.Form.GetValues("上限数量");

            if (clientCode == null)
            {
                return Helper.RedirectAjax("err", "请先复制数据再导入！", null, "");
            }
            if (clientCode.Count() > 500)
            {
                return Helper.RedirectAjax("err", "每次导入量不能超过500条！", null, "");
            }

            if (clientCode.Count() != locationId.Count() || locationId.Count() != altItemNumber.Count() || altItemNumber.Count() != lot1.Count() || maxQty.Count() != clientCode.Count())
            {
                return Helper.RedirectAjax("err", "数据出现异常，请检查数据行数是否一致！", null, "");
            }

            Hashtable data = new Hashtable();   //去excel重复的库位

            //清除excel表中的数据
            string errorItemNumber = ""; //插入失败的款号
            int k = 0;
            for (int i = 0; i < altItemNumber.Length; i++)
            {
                if (!data.ContainsValue(clientCode[i].ToString().Trim() + "-" + locationId[i].ToString() + "-" + altItemNumber[i].ToString() + "-" + lot1[i].ToString().Trim()))//Ecxel是否存在重复的值 不存在 add 
                {
                    data.Add(k, clientCode[i].ToString().Trim() + "-" + locationId[i].ToString() + "-" + altItemNumber[i].ToString() + "-" + lot1[i].ToString().Trim());
                    k++;
                }
                else
                {
                    errorItemNumber = "数据:" + clientCode[i].ToString().Trim() + "-" + locationId[i].ToString() + "-" + altItemNumber[i].ToString() + "-" + lot1[i].ToString().Trim()  + "-" + maxQty[i].ToString().Trim();
                }
            }

            if (errorItemNumber != "")
            {
                return Helper.RedirectAjax("err", "导入数据重复！" + errorItemNumber, null, "");
            }

            List<WCF.RootService.R_Location_Item_RG> entityList = new List<WCF.RootService.R_Location_Item_RG>();

            //构造批量导入实体
            for (int i = 0; i < clientCode.Length; i++)
            {
                WCF.RootService.R_Location_Item_RG entity = new WCF.RootService.R_Location_Item_RG();
                entity.WhCode = Session["whCode"].ToString();
                entity.ClientCode = clientCode[i].Trim();
                entity.LocationId = locationId[i].Trim();
                entity.AltItemNumber = altItemNumber[i].Trim();
                entity.LotNumber1 = lot1[i].Trim();
                entity.MinQty = 0;
                entity.MaxQty = Convert.ToInt32(maxQty[i]);
                entity.CreateUser = Session["userName"].ToString();
                entityList.Add(entity);
            }

            string result = cf.R_Location_ItemRGAdd(entityList.ToArray());
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "导入成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }

        }


        //编辑信息
        [HttpGet]
        public ActionResult EditAddR_Location_Item_RG()
        {
            WCF.RootService.R_Location_Item_RG entity = new WCF.RootService.R_Location_Item_RG();
            entity.Id = Convert.ToInt32(Request["Id"].Trim());
            entity.MaxQty = Convert.ToInt32(Request["edit_MaxQty"].Trim());
            entity.Status = Request["edit_Status"].Trim();
            entity.UpdateUser = Session["userName"].ToString();
            int result = cf.R_Location_ItemRGEdit(entity);
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
        public ActionResult R_Location_Item_RGDel()
        {
            string[] Pow_Id = Request.Form.GetValues("chx_Pow");

            List<WCF.RootService.R_Location_Item_RG> entityList = new List<WCF.RootService.R_Location_Item_RG>();
            for (int i = 0; i < Pow_Id.Length; i++)
            {
                WCF.RootService.R_Location_Item_RG entity = new WCF.RootService.R_Location_Item_RG();
                entity.Id = Convert.ToInt32(Pow_Id[i]);

                entityList.Add(entity);
            }

            string result = cf.R_Location_Item_RG_Del(entityList.ToArray());
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

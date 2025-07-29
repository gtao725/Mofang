using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace WMS.UI.Controllers
{
    public class C_SupplementController : Controller
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
        //库存问题信息查询
        public ActionResult List()
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
            List<WCF.RootService.R_Location_ItemResult> list = cf.R_Location_ItemList(entity, out total).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Id", "操作");
            fieldsName.Add("ClientCode", "客户名");
            fieldsName.Add("LocationId", "捡货库位");
            fieldsName.Add("AltItemNumber", "款号");
            fieldsName.Add("MinQty", "警戒数量");
            fieldsName.Add("MaxQty", "上限数量");
            fieldsName.Add("UnitName", "单位");
            fieldsName.Add("LotNumber1", "Lot1");
            fieldsName.Add("LotNumber2", "Lot2");
            fieldsName.Add("Status", "状态");
            fieldsName.Add("StatusShow", "状态名");
            fieldsName.Add("CreateUser", "创建人");
            fieldsName.Add("CreateDate", "创建时间");
            fieldsName.Add("UpdateUser", "更新人");
            fieldsName.Add("UpdateDate", "更新时间");

            return Content(EIP.EipListJson(list, total, fieldsName, "Id:60,MinQty:70,MaxQty:70,UnitName:65,AltItemNumber:120,CreateDate:120,default:100"));
        }


        //新增补货库位信息
        [HttpGet]
        public ActionResult AddR_Location_Item()
        {
            List<WCF.RootService.R_Location_Item> list = new List<WCF.RootService.R_Location_Item>();

            WCF.RootService.R_Location_Item entity = new WCF.RootService.R_Location_Item();
            entity.WhCode = Session["whCode"].ToString();
            entity.ClientCode = Request["txt_clientCode"].Trim();
            entity.LocationId = Request["txt_LocationId"].Trim();
            entity.AltItemNumber = Request["txt_AltItemNumber"].Trim();
            entity.MinQty = Convert.ToInt32(Request["txt_MinQty"].Trim());
            entity.MaxQty = Convert.ToInt32(Request["txt_MaxQty"].Trim());
            entity.LotNumber1 = Request["txt_LotNumber1"].Trim();
            entity.LotNumber2 = Request["txt_LotNumber2"].Trim();
            if (Request["txt_LotDate"].Trim() != "")
            {
                entity.LotDate = Convert.ToDateTime(Request["txt_LotDate"].Trim());
            }

            entity.Status = Request["Status"].Trim();
            entity.CreateUser = Session["userName"].ToString();
            list.Add(entity);

            string result = cf.R_Location_ItemAdd(list.ToArray());
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "添加成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }
        }



        [HttpPost]
        public ActionResult ImportsR_Location_Item()
        {
            string[] clientCode = Request.Form.GetValues("客户名");
            string[] locationId = Request.Form.GetValues("库位");
            string[] altItemNumber = Request.Form.GetValues("款号");
            string[] lot1 = Request.Form.GetValues("lot1");
            string[] minQty = Request.Form.GetValues("警戒数量");
            string[] maxQty = Request.Form.GetValues("上限数量");

            if (clientCode == null)
            {
                return Helper.RedirectAjax("err", "请先复制数据再导入！", null, "");
            }
            if (clientCode.Count() > 500)
            {
                return Helper.RedirectAjax("err", "每次导入量不能超过500条！", null, "");
            }

            if (clientCode.Count() != locationId.Count() || locationId.Count() != altItemNumber.Count() || altItemNumber.Count() != lot1.Count() || lot1.Count() != minQty.Count() || minQty.Count() != maxQty.Count() || maxQty.Count() != clientCode.Count())
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
                    errorItemNumber = "数据:" + clientCode[i].ToString().Trim() + "-" + locationId[i].ToString() + "-" + altItemNumber[i].ToString() + "-" + lot1[i].ToString().Trim() + "-" + minQty[i].ToString().Trim() + "-" + maxQty[i].ToString().Trim();
                }
            }

            if (errorItemNumber != "")
            {
                return Helper.RedirectAjax("err", "导入数据重复！" + errorItemNumber, null, "");
            }

            List<WCF.RootService.R_Location_Item> entityList = new List<WCF.RootService.R_Location_Item>();

            //构造批量导入实体
            for (int i = 0; i < clientCode.Length; i++)
            {
                WCF.RootService.R_Location_Item entity = new WCF.RootService.R_Location_Item();
                entity.WhCode = Session["whCode"].ToString();
                entity.ClientCode = clientCode[i].Trim();
                entity.LocationId = locationId[i].Trim();
                entity.AltItemNumber = altItemNumber[i].Trim();
                entity.LotNumber1 = lot1[i].Trim();
                entity.MinQty = Convert.ToInt32(minQty[i]);
                entity.MaxQty = Convert.ToInt32(maxQty[i]);
                entity.CreateUser = Session["userName"].ToString();
                entityList.Add(entity);
            }

            string result = cf.R_Location_ItemAdd(entityList.ToArray());
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "导入成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }

        }


        //新增补货库位信息
        [HttpGet]
        public ActionResult EditAddR_Location_Item()
        {


            WCF.RootService.R_Location_Item entity = new WCF.RootService.R_Location_Item();
            entity.Id = Convert.ToInt32(Request["Id"].Trim());
            entity.MinQty = Convert.ToInt32(Request["edit_MinQty"].Trim());
            entity.MaxQty = Convert.ToInt32(Request["edit_MaxQty"].Trim());
            entity.Status = Request["edit_Status"].Trim();
            entity.UpdateUser = Session["userName"].ToString();
            int result = cf.R_Location_ItemEdit(entity);
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
        public ActionResult SupplementLocationDel()
        {
            string[] Pow_Id = Request.Form.GetValues("chx_Pow");

            List<WCF.RootService.R_Location_Item> entityList = new List<WCF.RootService.R_Location_Item>();
            for (int i = 0; i < Pow_Id.Length; i++)
            { 
                WCF.RootService.R_Location_Item entity = new WCF.RootService.R_Location_Item();
                entity.Id = Convert.ToInt32(Pow_Id[i]);
   
                entityList.Add(entity);
            }

            string result = cf.R_Location_Item_Del(entityList.ToArray());
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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace WMS.UI.Controllers
{
    public class C_LossController : Controller
    {
        WCF.RootService.RootServiceClient cf = new WCF.RootService.RootServiceClient();
        WCF.InBoundService.InBoundServiceClient incf = new WCF.InBoundService.InBoundServiceClient();

        Helper Helper = new Helper();
        EIP.EIP EIP = new EIP.EIP();

        [DefaultRequest]
        public ActionResult Index()
        {
            ViewData["WhClientList"] = from r in incf.WhClientListSelect(Session["whCode"].ToString())
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
            WCF.RootService.LossSearch entity = new WCF.RootService.LossSearch();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.WhCode = Session["whCode"].ToString();
            entity.LossCode = Request["LossCode"].Trim();
            entity.ClientCode = Request["ClientCode"].Trim();

            int total = 0;
            List<WCF.RootService.Loss> list = cf.LossList(entity, out total).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Id", "操作");
            fieldsName.Add("ClientCode", "客户名");
            fieldsName.Add("LossCode", "耗材Code");
            fieldsName.Add("LossDescription", "耗材类型");
            fieldsName.Add("Qty", "库存数量");
            fieldsName.Add("Length", "长");
            fieldsName.Add("Width", "宽");
            fieldsName.Add("Height", "高");
            fieldsName.Add("Weight", "重量");
            fieldsName.Add("WorkLevel", "工作台耗材扫描");
            fieldsName.Add("CreateUser", "创建人");
            fieldsName.Add("CreateDate", "创建时间");
            fieldsName.Add("UpdateUser", "更新人");
            fieldsName.Add("UpdateDate", "更新时间");

            return Content(EIP.EipListJson(list, total, fieldsName, "Id:70,LossDescription:120,CreateDate:120,UpdateDate:120,WorkLevel:150,default:100"));
        }

        [HttpGet]
        public ActionResult LossAdd()
        {
            WCF.RootService.Loss entity = new WCF.RootService.Loss();
            entity.WhCode = Session["whCode"].ToString();
            entity.ClientCode = Request["txt_ClientCode"].Trim();
            entity.LossCode = Request["txt_LossCode"].Trim();
            entity.LossDescription = Request["txt_LossDescription"].Trim();
            if (Request["txt_Length"] != "")
            {
                entity.Length = Convert.ToDecimal(Request["txt_Length"]);
            }
            else
            {
                entity.Length = 0;
            }

            if (Request["txt_Width"] != "")
            {
                entity.Width = Convert.ToDecimal(Request["txt_Width"]);
            }
            else
            {
                entity.Width = 0;
            }

            if (Request["txt_Height"] != "")
            {
                entity.Height = Convert.ToDecimal(Request["txt_Height"]);
            }
            else
            {
                entity.Height = 0;
            }

            if (Request["txt_Weight"] != "")
            {
                entity.Weight = Convert.ToDecimal(Request["txt_Weight"]);
            }
            else
            {
                entity.Weight = 0;
            }

            entity.WorkLevel= Convert.ToInt32(Request["txt_WorkLevel"]);
            entity.Qty = Convert.ToInt32(Request["txt_Qty"]);
            entity.CreateUser = Session["userName"].ToString();
            string result = cf.LossAdd(entity);

            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "添加成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }
        }



        [HttpGet]
        public ActionResult LossDel()
        {
            int result = cf.LossDel(Convert.ToInt32(Request["Id"]));
            if (result > 0)
            {
                return Helper.RedirectAjax("ok", "删除成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "删除失败！", null, "");
            }
        }


        [HttpGet]
        public ActionResult EditDetail()
        {
            WCF.RootService.Loss entity = new WCF.RootService.Loss();
            entity.WhCode = Session["whCode"].ToString();
            entity.Id = Convert.ToInt32(Request["edit_Id"]);
            entity.ClientCode = Request["edit_clientCode"].Trim();
            entity.LossDescription = Request["edit_lossDescription"].Trim();
            entity.Length = Convert.ToDecimal(Request["edit_length"]);
            entity.Width = Convert.ToDecimal(Request["edit_width"]);
            entity.Height = Convert.ToDecimal(Request["edit_height"]);
            entity.Weight = Convert.ToDecimal(Request["edit_weight"]);
            entity.Qty = Convert.ToInt32(Request["edit_qty"]);
            entity.WorkLevel= Convert.ToInt32(Request["edit_WorkLevel"]);
            entity.UpdateUser = Session["userName"].ToString();

            int result = cf.LossEdit(entity);

            if (result > 0)
            {
                return Helper.RedirectAjax("ok", "修改成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "修改失败", null, "");
            }
        }

    }
}

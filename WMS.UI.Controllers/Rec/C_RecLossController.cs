using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace WMS.UI.Controllers
{
    public class C_RecLossController : Controller
    {
        WCF.RecService.RecServiceClient cf = new WCF.RecService.RecServiceClient();
        WCF.InBoundService.InBoundServiceClient incf = new WCF.InBoundService.InBoundServiceClient();

        Helper Helper = new Helper();
        EIP.EIP EIP = new EIP.EIP();

        [DefaultRequest]
        public ActionResult Index()
        {
            ViewData["RecLossTypeList"] = from r in cf.RecLossTypeListSelect(Session["whCode"].ToString())
                                          select new SelectListItem()
                                          {
                                              Text = r.RecLossType1,     //text
                                              Value = r.Id.ToString()
                                          };
            return View();
        }

        [HttpGet]
        public ActionResult List()
        {
            WCF.RecService.RecLossSearch entity = new WCF.RecService.RecLossSearch();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.WhCode = Session["whCode"].ToString();
            entity.RecLossName = Request["RecLossName"].Trim();
            entity.Status = Request["Status"].Trim();
            if (!string.IsNullOrEmpty(Request["RecLossType"]))
            {
                entity.RecLossTypeId = Convert.ToInt32(Request["RecLossType"]);
            }

            int total = 0;
            List<WCF.RecService.RecLossResult> list = cf.RecLossList(entity, out total).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Id", "操作");
            fieldsName.Add("RecLossName", "收货耗材名称");
            fieldsName.Add("RecLossDescription", "耗材说明");
            fieldsName.Add("RecLossType", "收货耗材科目");
            fieldsName.Add("Price", "单价");
            fieldsName.Add("RecSort", "排序");
            fieldsName.Add("Status", "状态");
            fieldsName.Add("CreateUser", "创建人");
            fieldsName.Add("CreateDate", "创建时间");
            fieldsName.Add("UpdateUser", "更新人");
            fieldsName.Add("UpdateDate", "更新时间");

            return Content(EIP.EipListJson(list, total, fieldsName, "Id:40,Price:80,CreateDate:120,UpdateDate:120,default:120"));
        }

        [HttpGet]
        public ActionResult LossAdd()
        {
            WCF.RecService.RecLoss entity = new WCF.RecService.RecLoss();
            entity.WhCode = Session["whCode"].ToString();
            entity.RecLossName = Request["txt_RecLossName"].Trim();
            entity.RecLossDescription = Request["txt_RecLossDescription"].Trim();
            entity.CreateUser = Session["userName"].ToString();
            entity.RecLossTypeId = Convert.ToInt32(Request["txt_RecLossType"]);

            try
            {
                entity.Price = Convert.ToDecimal(Request["txt_Price"].Trim());
            }
            catch (Exception)
            {
                return Helper.RedirectAjax("err", "单价格式不正确，请输入数字！", null, "");
            }

            try
            {
                entity.RecSort = Convert.ToInt32(Request["txt_RecSort"].Trim());
            }
            catch (Exception)
            {
                return Helper.RedirectAjax("err", "排序格式不正确，请输入数字！", null, "");
            }


            WCF.RecService.RecLoss result = cf.RecLossAdd(entity);

            if (result.Id > 0)
            {
                return Helper.RedirectAjax("ok", "添加成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "添加失败，请检查收货耗材名称是否已存在！", null, "");
            }
        }


        [HttpGet]
        public ActionResult EditDetail()
        {
            WCF.RecService.RecLoss entity = new WCF.RecService.RecLoss();
            entity.Id = Convert.ToInt32(Request["edit_Id"]);
            entity.RecLossDescription = Request["edit_lossDescription"].Trim();
            entity.Status = Request["edit_Status"].Trim();

            try
            {
                entity.Price = Convert.ToDecimal(Request["edit_price"].Trim());
            }
            catch (Exception)
            {
                return Helper.RedirectAjax("err", "单价格式不正确，请输入数字！", null, "");
            }


            try
            {
                entity.RecSort = Convert.ToInt32(Request["edit_recSort"].Trim());
            }
            catch (Exception)
            {
                return Helper.RedirectAjax("err", "排序格式不正确，请输入数字！", null, "");
            }


            entity.UpdateUser = Session["userName"].ToString();

            string result = cf.RecLossEdit(entity);

            if (result == "Y")
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

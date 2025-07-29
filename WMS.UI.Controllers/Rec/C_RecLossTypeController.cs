using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace WMS.UI.Controllers
{
    public class C_RecLossTypeController : Controller
    {
        WCF.RecService.RecServiceClient cf = new WCF.RecService.RecServiceClient();
        WCF.InBoundService.InBoundServiceClient incf = new WCF.InBoundService.InBoundServiceClient();

        Helper Helper = new Helper();
        EIP.EIP EIP = new EIP.EIP();

        [DefaultRequest]
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult List()
        {
            WCF.RecService.RecLossTypeSearch entity = new WCF.RecService.RecLossTypeSearch();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.WhCode = Session["whCode"].ToString();
            entity.RecLossType = Request["RecLossType"].Trim();
            entity.Status = Request["Status"].Trim();

            int total = 0;
            List<WCF.RecService.RecLossType> list = cf.RecLossTypeList(entity, out total).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Id", "操作");
            fieldsName.Add("RecLossType1", "收货耗材科目");
            fieldsName.Add("Status", "状态");
            fieldsName.Add("CreateUser", "创建人");
            fieldsName.Add("CreateDate", "创建时间");
            fieldsName.Add("UpdateUser", "更新人");
            fieldsName.Add("UpdateDate", "更新时间");

            return Content(EIP.EipListJson(list, total, fieldsName, "Id:70,RecLossType1:140,CreateDate:120,UpdateDate:120,default:100"));
        }

        [HttpGet]
        public ActionResult LossAdd()
        {
            WCF.RecService.RecLossType entity = new WCF.RecService.RecLossType();
            entity.WhCode = Session["whCode"].ToString();
            entity.RecLossType1 = Request["txt_RecLossType"].Trim();
            entity.CreateUser = Session["userName"].ToString();

            WCF.RecService.RecLossType result = cf.RecLossTypeAdd(entity);

            if (result.Id > 0)
            {
                return Helper.RedirectAjax("ok", "添加成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "添加失败，请检查科目是否存在！", null, "");
            }
        }


        [HttpGet]
        public ActionResult EditDetail()
        {
            WCF.RecService.RecLossType entity = new WCF.RecService.RecLossType();
            entity.WhCode = Session["whCode"].ToString();
            entity.Id = Convert.ToInt32(Request["edit_Id"]);
            entity.Status = Request["edit_Status"].Trim();
            entity.UpdateUser = Session["userName"].ToString();

            string result = cf.RecLossTypeEdit(entity);

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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace WMS.UI.Controllers
{
    public class ContractFormExtendController : Controller
    {
        WCF.InBoundService.InBoundServiceClient inboundcf = new WCF.InBoundService.InBoundServiceClient();
        WCF.RootService.RootServiceClient cf = new WCF.RootService.RootServiceClient();

        Helper Helper = new Helper();
        EIP.EIP EIP = new EIP.EIP();
        [DefaultRequest]
        public ActionResult Index()
        {

            return View();
        }

        //查询列表
        [HttpGet]
        public ActionResult List()
        {
            WCF.RootService.ContractFormExtendSearch entity = new WCF.RootService.ContractFormExtendSearch();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.WhCode = Session["whCode"].ToString();

            entity.ChargeName = Request["txt_ChargeName"].Trim();

            int total = 0;
            List<WCF.RootService.ContractFormExtend> list = cf.ContractFormExtendList(entity, out total).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Id", "操作");
            fieldsName.Add("ChargeName", "收费项目名");
            fieldsName.Add("Price", "单价");

            return Content(EIP.EipListJson(list, total, fieldsName, "Id:60,default:130"));
        }

        //修改信息
        [HttpGet]
        public ActionResult Edit()
        {
            WCF.RootService.ContractFormExtend entity = new WCF.RootService.ContractFormExtend();
            entity.Id = Convert.ToInt32(Request["Id"]);

            string errorResult1 = "";
            try
            {
                entity.Price = Convert.ToDecimal(Request["edit_price"].Trim());
            }
            catch
            {
                errorResult1 = "数据异常:" + Request["edit_price"].Trim();
            }
            if (errorResult1 != "")
            {
                return Helper.RedirectAjax("err", "格式不正确！" + errorResult1, null, "");
            }

            string result = cf.ContractFormExtendEdit(entity);
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "修改成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "对不起，修改失败！", null, "");
            }
        }


    }
}

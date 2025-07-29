using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace WMS.UI.Controllers
{
    public class HeavyPalletController : Controller
    {
        WCF.InBoundService.InBoundServiceClient cf = new WCF.InBoundService.InBoundServiceClient();
        Helper Helper = new Helper();
        EIP.EIP EIP = new EIP.EIP();
        [DefaultRequest]

        public ActionResult Index()
        {
            ViewBag.PhotoServerAddress = ConfigurationManager.AppSettings["PhotoServerAddress"].ToString();
            ViewBag.PhotoSaveAddress = ConfigurationManager.AppSettings["PhotoSaveAddress"].ToString();
            ViewBag.WMSUrl = ConfigurationManager.AppSettings["WMSUrl"].ToString();
            ViewBag.CrSaveAddress = ConfigurationManager.AppSettings["CrAddress98"].ToString();

            ViewBag.userName = Session["userName"].ToString();
            ViewBag.whCode = Session["whCode"].ToString();

            return View();
        }

        //查询收货异常报表
        [HttpGet]
        public ActionResult List()
        {
            WCF.InBoundService.KmartHeavyPalletSearch entity = new WCF.InBoundService.KmartHeavyPalletSearch();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.WhCode = Session["whCode"].ToString();
            entity.SoNumber = Request["SoNumber"].ToString();
            entity.CustomerPoNumber = Request["CustomerPoNumber"].ToString();
            entity.AltItemNumber = Request["AltItemNumber"].ToString();
            entity.HuId = Request["HuId"].ToString();
            entity.ReceiptId = Request["ReceiptId"].ToString();
            entity.ClientCode = "Kmart";

            if (!string.IsNullOrEmpty(Request["BeginCreateDate"]))
            {
                entity.BeginReceiptDate = Convert.ToDateTime(Request["BeginCreateDate"]);
            }
            if (!string.IsNullOrEmpty(Request["EndCreateDate"]))
            {
                entity.EndReceiptDate = Convert.ToDateTime(Request["EndCreateDate"]).AddDays(1);
            }

            if (!string.IsNullOrEmpty(Request["BeginConsDate"]))
            {
                entity.BeginConsDate = Convert.ToDateTime(Request["BeginConsDate"]);
            }
            if (!string.IsNullOrEmpty(Request["EndConsDate"]))
            {
                entity.EndConsDate = Convert.ToDateTime(Request["EndConsDate"]);
            }

            string soNumber = Request["so_number"];
            string[] soNumberList = null;
            if (!string.IsNullOrEmpty(soNumber))
            {
                string temp = soNumber.Replace("\r\n", "@");    //把so中的空格 替换为@符号
                soNumberList = temp.Split('@');           //把SO 按照@分割，放在数组
            }

            int total = 0;
            List<WCF.InBoundService.KmartHeavyPalletResult> list = cf.KmartHeavyPalletList(entity, soNumberList, out total).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Show", "操作");
            fieldsName.Add("ReceiptId", "收货批次号");
            fieldsName.Add("HuId", "托盘");
            fieldsName.Add("LocationId", "库位");
            fieldsName.Add("ConsDate", "[ConsDate]");
            fieldsName.Add("DCDD", "[DCDD]");
            fieldsName.Add("Supplier", "[Vendor]");
            fieldsName.Add("SoNumber", "[Booking no]");
            fieldsName.Add("PoNumber", "[Brand PoNo.]");
            fieldsName.Add("ItemNumber", "[Style Number/KeyCode]");
            fieldsName.Add("PlaceOfDelivery", "[POD]");
            fieldsName.Add("BookedCarton", "Box_QTY");
            fieldsName.Add("Qty", "[Received Box QTY]");
            fieldsName.Add("recCBM", "[Receive TTL Vol]");
            fieldsName.Add("Length", "长");
            fieldsName.Add("Width", "宽");
            fieldsName.Add("Height", "高");
            fieldsName.Add("Show1", "费用");

            return Content(EIP.EipListJson(list, total, fieldsName, "Show:60,ReceiptId:130,default:120", null, "", 50, ""));

        }

        //修改
        [HttpGet]
        public ActionResult EditHeavyPallet()
        {
            WCF.InBoundService.KmartHeavyPalletResult entity = new WCF.InBoundService.KmartHeavyPalletResult();
            entity.WhCode = Session["whCode"].ToString();
            entity.HuId = Request["huId"];

            try
            {
                entity.Length = Convert.ToDecimal(Request["edit_HuLength"]);
                entity.Width = Convert.ToDecimal(Request["edit_HuWidth"]);
                entity.Height = Convert.ToDecimal(Request["edit_HuHeight"]);
            }
            catch (Exception)
            {
                return Helper.RedirectAjax("err", "长宽高必须为数值", null, "");
            }

            entity.UpdateUser = Session["userName"].ToString();

            string result = cf.KmartHeavyPalletEdit(entity);
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "信息修改成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }

        }




    }
}

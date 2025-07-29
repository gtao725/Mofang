using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;


namespace WMS.UI.Controllers
{
    public class HuAllWeightController : Controller
    {

        WCF.RootService.RootServiceClient cf = new WCF.RootService.RootServiceClient();

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
            WCF.RootService.HuMasterSearch1 entity = new WCF.RootService.HuMasterSearch1();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.WhCode = Session["whCode"].ToString();
            entity.SoNumber = Request["SoNumber"].ToString();
            entity.CustomerPoNumber = Request["CustomerPoNumber"].ToString();
            entity.AltItemNumber = Request["AltItemNumber"].ToString();
            entity.HuId = Request["HuId"].ToString();
            entity.ReceiptId = Request["ReceiptId"].ToString();
            entity.ClientCode= Request["clientCode"].ToString(); 

            if (!string.IsNullOrEmpty(Request["BeginCreateDate"]))
            {
                entity.BeginReceiptDate = Convert.ToDateTime(Request["BeginCreateDate"]);
            }
            if (!string.IsNullOrEmpty(Request["EndCreateDate"]))
            {
                entity.EndReceiptDate = Convert.ToDateTime(Request["EndCreateDate"]).AddDays(1);
            }

            string soNumber = Request["so_number"];
            string[] soNumberList = null;
            if (!string.IsNullOrEmpty(soNumber))
            {
                string temp = soNumber.Replace("\r\n", "@");    //把so中的空格 替换为@符号
                soNumberList = temp.Split('@');           //把SO 按照@分割，放在数组
            }

            int total = 0;
            List<WCF.RootService.HuMasterResult1> list = cf.HuMasterHeavyPalletList(entity, soNumberList, out total).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Show", "操作");
            fieldsName.Add("ClientCode", "客户名");
            fieldsName.Add("ReceiptId", "收货批次号");
            fieldsName.Add("HuId", "托盘");
            fieldsName.Add("LocationId", "库位"); 
            fieldsName.Add("SoNumber", "SoNumber");
            fieldsName.Add("PoNumber", "PoNumber");
            fieldsName.Add("ItemNumber", "款号");
            fieldsName.Add("Qty", "数量");
            fieldsName.Add("recCBM", "总立方");
            fieldsName.Add("Length", "托盘长");
            fieldsName.Add("Width", "托盘宽");
            fieldsName.Add("Height", "托盘高");
            fieldsName.Add("HuWeight", "托盘重量");

            return Content(EIP.EipListJson(list, total, fieldsName, "Show:60,ReceiptId:130,ClientCode:100,Qty:50,recCBM:70,Length:60,Width:60,Height:60,HuWeight:60,default:120", null, "", 50, ""));

        }

        //修改
        [HttpGet]
        public ActionResult EditHeavyPallet()
        {
            WCF.RootService.HuMasterResult1 entity = new WCF.RootService.HuMasterResult1();
            entity.WhCode = Session["whCode"].ToString();
            entity.HuId = Request["huId"];

            try
            {
                entity.Length = Convert.ToDecimal(Request["edit_HuLength"]);
                entity.Width = Convert.ToDecimal(Request["edit_HuWidth"]);
                entity.Height = Convert.ToDecimal(Request["edit_HuHeight"]);
                entity.HuWeight = Convert.ToDecimal(Request["edit_HuWeight"]);
            }
            catch (Exception)
            {
                return Helper.RedirectAjax("err", "长宽高重量必须为数值", null, "");
            }

            entity.UpdateUser = Session["userName"].ToString();

            string result = cf.HuMasterHeavyPalletEdit(entity);
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

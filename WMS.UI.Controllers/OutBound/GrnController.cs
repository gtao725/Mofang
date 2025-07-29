using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace WMS.UI.Controllers
{
    public class GrnController : Controller
    {
        WCF.InBoundService.InBoundServiceClient inboundcf = new WCF.InBoundService.InBoundServiceClient();
        WCF.OutBoundService.OutBoundServiceClient cf = new WCF.OutBoundService.OutBoundServiceClient();
        WCF.RecService.RecServiceClient rc = new WCF.RecService.RecServiceClient();

        Helper Helper = new Helper();
        EIP.EIP EIP = new EIP.EIP();

        [DefaultRequest]
        public ActionResult Index()
        {
            ViewData["CreateUserName"] = from r in cf.CreateUserSelect(Session["whCode"].ToString())
                                         select new SelectListItem()
                                         {
                                             Text = r.CreateUserName,     //text
                                             Value = r.CreateUser
                                         };

            ViewData["LoadContainerTypeSelect"] = from r in cf.LoadContainerTypeSelect()
                                                  select new SelectListItem()
                                                  {
                                                      Text = r.ContainerName,     //text
                                                      Value = r.ContainerType
                                                  };
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
            WCF.OutBoundService.LoadContainerSearch entity = new WCF.OutBoundService.LoadContainerSearch();
            WCF.RecService.GrnHeadSearch entity1 = new WCF.RecService.GrnHeadSearch();

            entity1.WhCode = Session["whCode"].ToString();
            entity1.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity1.pageSize = Convert.ToInt32(Request["eip_page_size"]);

            string SO = Request["SO"];
            string[] SOl = null;
            if (!string.IsNullOrEmpty(SO))
            {
                string so_temp = SO.Replace("\r\n", "@");    //把so中的空格 替换为@符号
                SOl = so_temp.Split('@');           //把SO 按照@分割，放在数组
            }


            entity1.receiptid = Request["receiptid"].Trim();
            entity1.ClientCode = Request["WhClientId"].Trim();
            entity1.SoL = SOl;


            //entity1.So = Request["SO"].Trim();
            //string poNumber = Request["containerNumber"];
            //string[] poNumberList = null;
            //if (!string.IsNullOrEmpty(poNumber))
            //{
            //    string po_temp = poNumber.Replace("\r\n", "@");    //把so中的空格 替换为@符号
            //    poNumberList = po_temp.Split('@');           //把SO 按照@分割，放在数组
            //}

            if (Request["BeginCreateDate"] != "")
            {
                entity1.BeginCreateDate = Convert.ToDateTime(Request["BeginCreateDate"]);
            }
            else
            {
                entity1.BeginCreateDate = null;
            }

            if (Request["EndCreateDate"] != "")
            {
                entity1.EndCreateDate = Convert.ToDateTime(Request["EndCreateDate"]).AddDays(1);
            }
            else
            {
                entity1.EndCreateDate = null;
            }

            int total = 0;

            List<WCF.RecService.GrnHeadResult> list = rc.GrnHeadList(entity1, out total).ToList();
           // List<WCF.OutBoundService.LoadContainerResult> list = cf.LoadContainerList(entity, poNumberList, out total).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("ClientCode", "客户");
            fieldsName.Add("SoNumber", "SO");
            fieldsName.Add("SendType", "发送类型");
            fieldsName.Add("Status", "状态");
            fieldsName.Add("SendTime", "触发时间");
            fieldsName.Add("GWI_Qty", "GWI数量");
            fieldsName.Add("GWI_Cbm", "GWI立方");
            fieldsName.Add("GWI_Kgs", "GWI重量");
            fieldsName.Add("WmsQty", "WMS数量");
            fieldsName.Add("WmsCbm", "WMS立方");
            fieldsName.Add("GRN_Qty", "GRN数量");
            fieldsName.Add("GRN_Cbm", "GRN立方");
            fieldsName.Add("GRN_Kgs", "GRN重量");
            fieldsName.Add("CreateDate", "EDI创建时间");

           
            return Content(EIP.EipListJson(list, total, fieldsName, "default:90"));
        }

        [HttpGet]
        public ActionResult SOList()
        {
            //WCF.OutBoundService.LoadContainerSearch entity = new WCF.OutBoundService.LoadContainerSearch();
            WCF.RecService.GrnHeadSearch entity1 = new WCF.RecService.GrnHeadSearch();

            entity1.WhCode = Session["whCode"].ToString();
            entity1.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity1.pageSize = Convert.ToInt32(Request["eip_page_size"]);

            //entity1.receiptid = Request["receiptid"].Trim();
            //entity1.ClientCode = Request["WhClientId"].Trim();
            entity1.So = Request["SO"].Trim();
            entity1.ClientCode = Request["ClientCode"].Trim();
            //string poNumber = Request["containerNumber"];
            //string[] poNumberList = null;
            //if (!string.IsNullOrEmpty(poNumber))
            //{
            //    string po_temp = poNumber.Replace("\r\n", "@");    //把so中的空格 替换为@符号
            //    poNumberList = po_temp.Split('@');           //把SO 按照@分割，放在数组
            //}

            //if (Request["BeginCreateDate"] != "")
            //{
            //    entity1.BeginCreateDate = Convert.ToDateTime(Request["BeginCreateDate"]);
            //}
            //else
            //{
            //    entity1.BeginCreateDate = null;
            //}

            //if (Request["EndCreateDate"] != "")
            //{
            //    entity1.EndCreateDate = Convert.ToDateTime(Request["EndCreateDate"]).AddDays(1);
            //}
            //else
            //{
            //    entity1.EndCreateDate = null;
            //}

            int total = 0;

            List<WCF.RecService.DamcoGRNDetail> list = rc.GrnSOList(entity1, out total).ToList();
            // List<WCF.OutBoundService.LoadContainerResult> list = cf.LoadContainerList(entity, poNumberList, out total).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Id", "Id");
            fieldsName.Add("SoNumber", "SO");
            fieldsName.Add("LN", "LN");
            fieldsName.Add("PoNumber", "PO");
            fieldsName.Add("AltItemNumber", "SKU");
            fieldsName.Add("Style", "属性");
            fieldsName.Add("GRN_ReceiptDate", "GRN收货时间");
            fieldsName.Add("GRN_Qty", "GRN数量");
            fieldsName.Add("GRN_Cbm", "GRN立方");
            fieldsName.Add("GRN_Kgs", "GRN重量");
            fieldsName.Add("GWI_Qty", "GWI数量");
            fieldsName.Add("GWI_Cbm", "GWI立方");
            fieldsName.Add("GWI_Kgs", "GWI重量");
            fieldsName.Add("WMS_ReceiptDate", "WMS收货时间");
            fieldsName.Add("WMS_Qty", "WMS数量");
            fieldsName.Add("WMS_Cbm", "WMS立方");
            fieldsName.Add("WMS_Kgs", "WMS重量");

            fieldsName.Add("UpdateDate", "更新时间");
            fieldsName.Add("UpdateUser", "更新人");

            return Content(EIP.EipListJson(list, total, fieldsName, "default:90"));
        }

        [HttpGet]
        public string RefreshWmsData()
        {
            string WhCode = Session["whCode"].ToString();
            string ClientCode = Request["ClientCode"].Trim();
            string SO = Request["SO"].Trim();
            string USER= Session["userName"].ToString();

            return rc.UpdateGrnWmsData(SO, ClientCode, WhCode, USER);
        }

        [HttpGet]
        public string GrnAutoUpdate()
        {
            string WhCode = Session["whCode"].ToString();
            string ClientCode = Request["ClientCode"].Trim();
            string SO = Request["SO"].Trim();
            string USER = Session["userName"].ToString();

            return rc.GrnAutoUpdate(SO, WhCode,ClientCode);
        }

        [HttpPost]
        public string SaveDetail()
        {
            string[] Id = Request["detailid"].Split(',');
            string[] GRN_ReceiptDate = Request["GRN_ReceiptDate"].Split(',');
            string[] GRN_Qty = Request["GRN_Qty"].Split(',');
            string[] GRN_Cbm = Request["GRN_Cbm"].Split(',');
            string[] GRN_Kgs = Request["GRN_Kgs"].Split(',');

            //DateTime? GRN_ReceiptDate, int GRN_Qty, double? GRN_Cbm, double? GRN_Kgs
            int i = 0;
            foreach (var item in Id)
            {
                rc.UpdateGrnDetail(int.Parse(item), DateTime.Parse(GRN_ReceiptDate[i]), int.Parse(GRN_Qty[i]), double.Parse(GRN_Cbm[i]), double.Parse(GRN_Kgs[i]));
                i++;
            }
            return "Y";
        }
        

        [HttpGet]
        public string SendGRN()
        {
            string WhCode = Session["whCode"].ToString();
            string ClientCode = Request["ClientCode"].Trim();
            string SO = Request["SO"].Trim();
            string USER = Session["userName"].ToString();

            return rc.SendGRN(SO, WhCode, ClientCode, USER);
        }

    }
}

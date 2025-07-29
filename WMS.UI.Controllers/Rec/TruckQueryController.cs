using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace WMS.UI.Controllers
{
    public class TruckQueryController : Controller
    {
        WCF.RecService.RecServiceClient recService = new WCF.RecService.RecServiceClient();
        WCF.InBoundService.InBoundServiceClient cf = new WCF.InBoundService.InBoundServiceClient();
        Helper Helper = new Helper();
        EIP.EIP EIP = new EIP.EIP();

        [DefaultRequest]
        public ActionResult Index()
        {
            ViewData["WhAgentList"] = from r in cf.WhAgentListSelect(Session["whCode"].ToString())
                                      select new SelectListItem()
                                      {
                                          Text = r.AgentCode,     //text
                                          Value = r.AgentCode.ToString()
                                      };

            ViewData["WhClientList"] = from r in cf.WhClientListSelect(Session["whCode"].ToString())
                                       select new SelectListItem()
                                       {
                                           Text = r.ClientCode,     //text
                                           Value = r.ClientCode.ToString()
                                       };

            ViewData["ZoneList"] = from r in cf.RecZoneSelect(Session["whCode"].ToString(), 0)
                                   select new SelectListItem()
                                   {
                                       Text = r.ZoneName,    //text
                                       Value = r.ZoneName.ToString()
                                   };

            return View();
        }


        [DefaultRequest]
        public ActionResult IndexDaoKou()
        {
            ViewData["WhAgentList"] = from r in cf.WhAgentListSelect(Session["whCode"].ToString())
                                      select new SelectListItem()
                                      {
                                          Text = r.AgentCode,     //text
                                          Value = r.AgentCode.ToString()
                                      };

            ViewData["WhClientList"] = from r in cf.WhClientListSelect(Session["whCode"].ToString())
                                       select new SelectListItem()
                                       {
                                           Text = r.ClientCode,     //text
                                           Value = r.ClientCode.ToString()
                                       };

            ViewData["ZoneList"] = from r in cf.RecZoneSelect(Session["whCode"].ToString(), 0)
                                   select new SelectListItem()
                                   {
                                       Text = r.ZoneName,    //text
                                       Value = r.ZoneName.ToString()
                                   };

            return View();
        }


        public ActionResult List()
        {
            //string s = "2018-07-07 04:00-20:00";
            //string a = s.Substring(0, 13);

            //string b = s.Substring(0, 11);
            //string b1 = s.Substring(17, 2);

            //DateTime s1 = Convert.ToDateTime(s.Substring(0, 13) + ":00:00");
            //if (s.Substring(17, 2) == "24")
            //{
            //    DateTime t1 = Convert.ToDateTime(s.Substring(0, 11) + " 23:59:59");
            //}
            //else
            //{
            //    DateTime t1 = Convert.ToDateTime(s.Substring(0, 11) + s.Substring(17, 2) + ":00:00");
            //}

            WCF.InBoundService.ReceiptRegisterSearch entity = new WCF.InBoundService.ReceiptRegisterSearch();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.WhCode = Session["whCode"].ToString();
            entity.ReceiptId = Request["ReceiptId"].Trim();
            entity.TruckNumber = Request["TruckNumber"].Trim();

            entity.AgentCode = Request["AgentCode"].Trim();
            entity.ClientCode = Request["ClientCode"].Trim();
            entity.TruckStatus = Request["truckStatus"].Trim();
            entity.LocationId = Request["LocationId"].Trim();

            if (Request["BeginCreateDate"] != "")
            {
                entity.BeginCreateDate = Convert.ToDateTime(Request["BeginCreateDate"]);
            }
            else
            {
                entity.BeginCreateDate = null;
            }

            if (Request["EndCreateDate"] != "")
            {
                entity.EndCreateDate = Convert.ToDateTime(Request["EndCreateDate"]).AddDays(1);
            }
            else
            {
                entity.EndCreateDate = null;
            }

            int total = 0;
            List<WCF.InBoundService.ReceiptRegisterResult> list = cf.TruckQueryList(entity, out total).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Id", "操作");
            fieldsName.Add("Action", "插队");
            fieldsName.Add("Action1", "放车");
            fieldsName.Add("ReceiptId", "收货批次号");
            fieldsName.Add("ClientCode", "客户名");
            fieldsName.Add("AgentCode", "货代");
            fieldsName.Add("LocationId", "库区");
            fieldsName.Add("GreenPassFlagShow", "绿色通道");  
            fieldsName.Add("Sequence", "排位");
            fieldsName.Add("TruckStatusShow", "排队状态");
            fieldsName.Add("Status", "批次状态");
            fieldsName.Add("SumQty", "登记数量");
            fieldsName.Add("SumCBM", "登记立方");
            fieldsName.Add("OneTruckMoreNumber", "一车多单");
            fieldsName.Add("TruckNumber", "车牌号");
            fieldsName.Add("DSFlag", "类型");
            fieldsName.Add("PhoneNumber", "手机号");
            fieldsName.Add("CreateDate", "创建时间");
            fieldsName.Add("BkDate", "预约时间");
            fieldsName.Add("QueueUpFLag", "是否插队");
            fieldsName.Add("QueueUpUser", "主管插队");
            fieldsName.Add("QueueUpRemark", "备注");
            fieldsName.Add("BaoAnRemark", "保安备注不在");
            fieldsName.Add("ParkingUser", "放车人员");
            fieldsName.Add("ParkingDate", "放车时间");
            fieldsName.Add("StorageDate", "入库时间");
            fieldsName.Add("DepartureDate", "离库时间");
            fieldsName.Add("CreateUser", "创建人");


            return Content(EIP.EipListJson(list, total, fieldsName, "Id:70,Action:45,Action1:45,ReceiptId:120,ClientCode:100,AgentCode:100,LocationId:45,Sequence:45,DSFlag:45,CreateDate:120,ArriveDate:120,TruckNumber:120,CreateUser:50,PhoneNumber:95,BkDate:140,ParkingDate:120,StorageDate:120,DepartureDate:120,BaoAnRemark:90,QueueUpRemark:70,default:65"));
        }

        //车辆放车
        [HttpGet]
        public ActionResult UpdateTruckStatu()
        {
            string result = cf.EditTruckStatus(Session["whCode"].ToString(), Request["receiptId"].Trim(), Session["userName"].ToString());
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "放车成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }
        }

        //车辆插队
        [HttpGet]
        public ActionResult QueueUpTruck()
        {
            string result = cf.QueueUpTruck(Session["whCode"].ToString(), Request["receiptId"].Trim(), Session["userName"].ToString(), Request["QueueUpRemark"].Trim());
            if (result == "Y")
            {
                cf.EditTruckStatus(Session["whCode"].ToString(), Request["receiptId"].Trim(), Session["userName"].ToString());
                return Helper.RedirectAjax("ok", "插队放车成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }
        }

        //删除收货登记 
        public ActionResult DelReceiptRegister()
        {
            WCF.InBoundService.ReceiptRegister entity = new WCF.InBoundService.ReceiptRegister();
            entity.ReceiptId = Request["ReceiptId"];
            entity.WhCode = Session["whCode"].ToString();
            entity.UpdateUser = Session["userName"].ToString();
            entity.UpdateDate = DateTime.Now;

            string result = cf.DelReceiptRegisterByTruck(entity);
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "删除成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }
        }

    }
}

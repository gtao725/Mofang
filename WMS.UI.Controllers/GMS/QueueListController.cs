using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace WMS.UI.Controllers
{
    public class QueueListController : Controller
    {
        Helper Helper = new Helper();
        EIP.EIP EIP = new EIP.EIP();
        WCF.GMSService.GMSServiceClient cf = new WCF.GMSService.GMSServiceClient();
        [DefaultRequest]
        public ActionResult Index()
        {
            WCF.InBoundService.InBoundServiceClient cf1 = new WCF.InBoundService.InBoundServiceClient();
            WCF.RootService.RootServiceClient ZoneCF = new WCF.RootService.RootServiceClient();
            ViewData["whCode"] = Session["whCode"].ToString();
            ViewData["WhClientList"] = from r in cf1.WhClientListSelect(Session["whCode"].ToString())
                                       select new SelectListItem()
                                       {
                                           Text = r.ClientCode,     //text
                                           Value = r.ClientCode.ToString()
                                       };

            List<WCF.RootService.WhZoneResult> list = ZoneCF.WhZoneParentSelect(Session["whCode"].ToString()).ToList().Where(u => u.RegFlag == 1).ToList();

            ViewData["ZoneParentSelect"] = from r in list
                                           select new SelectListItem()
                                           {
                                               Text = r.ZoneName,     //text
                                               Value = r.ZoneName.ToString()
                                           };
            ViewData["BookChannel"] = from r in cf.BookChannelSelect(Session["whCode"].ToString())
                                      select new SelectListItem()
                                      {
                                          Text = r.Description,     //text
                                          Value = r.Description
                                      };
            return View();
        }


        [DefaultRequest]
        public ActionResult Index1()
        {
            WCF.InBoundService.InBoundServiceClient cf1 = new WCF.InBoundService.InBoundServiceClient();
            WCF.RootService.RootServiceClient ZoneCF = new WCF.RootService.RootServiceClient();
            ViewData["whCode"] = Session["whCode"].ToString();
            ViewData["WhClientList"] = from r in cf1.WhClientListSelect(Session["whCode"].ToString())
                                       select new SelectListItem()
                                       {
                                           Text = r.ClientCode,     //text
                                           Value = r.ClientCode.ToString()
                                       };

            List<WCF.RootService.WhZoneResult> list = ZoneCF.WhZoneParentSelect(Session["whCode"].ToString()).ToList().Where(u => u.RegFlag == 1).ToList();

            ViewData["ZoneParentSelect"] = from r in list
                                           select new SelectListItem()
                                           {
                                               Text = r.ZoneName,     //text
                                               Value = r.ZoneName.ToString()
                                           };
            ViewData["BookChannel"] = from r in cf.BookChannelSelect(Session["whCode"].ToString())
                                      select new SelectListItem()
                                      {
                                          Text = r.Description,     //text
                                          Value = r.Description
                                      };
            return View();
        }

        [DefaultRequest]
        public ActionResult LockIndex()
        {
            ViewData["whCode"] = Session["whCode"].ToString();
            return View();
        }

        //放车
        [HttpGet]
        public void ReleaseTruck()
        {
            try
            {
                string TruckNumber = Request["TruckNumber"].ToString();
                string UnloadingArea = Request["UnloadingArea"].ToString();
                string WhCode = Request["WhCode"].ToString();
                string Id = Request["Id"];
                string JumpingRemark = Request["JumpingRemark"];
                string smallloadArea = Request["smallloadArea"];
                string UserName = "";
                try
                {
                    UserName = Session["userName"].ToString();
                }
                catch (Exception ex)
                {
                    Response.Write("登录已失效，请退出重新登录！");
                    return;
                }
                WCF.GMSService.QueueParam queueParam = new WCF.GMSService.QueueParam();
                queueParam.Id = Id;
                queueParam.JumpingRemark = JumpingRemark;
                queueParam.UserName = UserName;
                queueParam.TruckNumber = TruckNumber;
                queueParam.UnloadingArea = UnloadingArea;
                queueParam.WhCode = WhCode;
                queueParam.smallloadArea = smallloadArea;
                string result = cf.ReleaseTruck(queueParam);
                Response.Write(result);
            }catch (Exception ex)
            {
                Response.Write("操作异常"+ex.Message.ToString());
            }
            
        }
        //删除排队车辆信息
        [HttpGet]
        public void DeleteTruckInfo()
        {
            try
            {
                string Id = Request["Id"];
                string TruckNumber = Request["TruckNumber"].ToString();
                string UserName = "";
                try
                {
                    UserName = Session["userName"].ToString();
                }
                catch (Exception ex)
                {
                    Response.Write("登录已失效，请退出重新登录！");
                    return;
                }
                WCF.GMSService.QueueParam queueParam = new WCF.GMSService.QueueParam();
                queueParam.Id = Id;
                queueParam.UserName = UserName;
                queueParam.TruckNumber = TruckNumber;
                string result = cf.DeleteTruckInfo(queueParam);
                Response.Write(result);
            }
            catch (Exception ex)
            {
                Response.Write(ex.ToString());
            }

        }
        //删除车牌排队明细信息
        [HttpGet]
        public void DeleteTruckDetail()
        {
            try
            {
                string Id = Request["Id"];
                //WCF.GMSService.GMSServiceClient cf = new WCF.GMSService.GMSServiceClient();
                string UserName = "";
                try
                {
                    UserName = Session["userName"].ToString();
                }
                catch (Exception ex)
                {
                    Response.Write("登录已失效，请退出重新登录！");
                    return;
                }
                WCF.GMSService.QueueParam queueParam = new WCF.GMSService.QueueParam();
                queueParam.Id = Id;
                queueParam.UserName = UserName;
                string result = cf.DeleteTruckDetail(queueParam);
                Response.Write(result);
            }
            catch (Exception ex)
            {
                Response.Write(ex.ToString());
            }
        }
        //新增车辆表头信息
        [HttpGet]
        public void AddTruckQueueHead()
        {
            try
            {
                WCF.GMSService.TruckQueueHeadParam entity = new WCF.GMSService.TruckQueueHeadParam();
                entity.WhCode = Session["whCode"].ToString();
                entity.TruckNumber = Request["txt_TruckNumber"].Trim().ToUpper();
                entity.PhoneNumber = Request["txt_PhoneNumber"].Trim();
                entity.TruckLength = Request["txt_TruckLength"].Trim();
                entity.GreenPassFlag = Convert.ToInt32(Request["txt_GreenPassFlag"]);
                string UserName = "";
                try
                {
                    UserName = Session["userName"].ToString();
                }
                catch (Exception ex)
                {
                    Response.Write("登录已失效，请退出重新登录！");
                    return;
                }
                entity.CreateUser = UserName;
                entity.BookOrigin = "USER";
                entity.TruckStatus = "1";
                string result = cf.AddTruckQueueHead(entity);
                Response.Write(result);
            }
            catch (Exception ex)
            {
                Response.Write(ex.ToString());
            }
        }
        //新增车辆明细信息
        [HttpGet]
        public void AddTruckQueueDetail()
        {
            try
            {
                WCF.GMSService.TruckQueueDetailParam entity = new WCF.GMSService.TruckQueueDetailParam();
                entity.ReceiptId = Request["txt_ReceiptId"].Trim();
                entity.WhCode = Session["whCode"].ToString();
                entity.UnloadingArea = Request["txt_UnloadingArea"].Trim();
                entity.ClientCode = Request["txt_ClientCode"].Trim();
                string txtQty = "0", txtCBM= "0",txtWeight="0";
                if (Request["txt_Qty"]+""!=""&&Request["txt_Qty"] != null)
                {
                    txtQty = Request["txt_Qty"].Trim();
                }
                if (Request["txt_CBM"] != null&& Request["txt_CBM"]+""!="")
                {
                    txtCBM = Request["txt_CBM"].Trim();
                }
                if(Request["txt_Weight"] != null&& Request["txt_Weight"]+""!="")
                {
                    txtWeight = Request["txt_Weight"].Trim();
                }
                string UserName = "";
                try
                {
                    UserName = Session["userName"].ToString();
                }
                catch (Exception ex)
                {
                    Response.Write("登录已失效，请退出重新登录！");
                    return;
                }
                entity.Qty = Convert.ToInt32(txtQty);
                entity.CBM = Convert.ToDecimal(txtCBM);
                entity.Weight = Convert.ToDecimal(txtWeight);
                entity.GoodsType = Request["txt_GoodsType"];
                entity.OverSizeFlag = Convert.ToInt32(Request["txt_OverSizeFlag"]);
                entity.NoticeFlag = Convert.ToInt32(Request["txt_NoticeFlag"]);
                entity.BookChannel = Request["txt_BookChannel"];
                entity.RegisterDate = Request["txt_RegisterDate"];
                entity.BookOrigin = "USER";
                entity.CreateUser= UserName;
                entity.SeeFlag = 0;
                entity.HeadId = Convert.ToInt32(Request["Id"]);
                string result = cf.AddTruckQueueDetail(entity);
                Response.Write(result);
            }
            catch (Exception ex)
            {
                Response.Write(ex.ToString());
            }
        }
        //修改车辆信息表头
        [HttpGet]
        public void UpdateTruckQueueHeader()
        {
            try
            {
                var Id = Request["Id"];
                var TruckNumber = Request["edit_TruckNumber"].Trim().ToUpper();
                var PhoneNumber = Request["edit_PhoneNumber"].Trim();
                var TruckLength = Request["edit_TruckLength"].Trim();
                var BookOrigin = Request["edit_BookOrigin"].Trim();
                var GreenPassFlag = Request["edit_GreenPassFlag"].Trim();
                string UserName = "";
                try
                {
                    UserName = Session["userName"].ToString();
                }
                catch (Exception ex)
                {
                    Response.Write("登录已失效，请退出重新登录！");
                    return;
                }
                WCF.GMSService.TruckQueueHeadParam entity = new WCF.GMSService.TruckQueueHeadParam();
                entity.TruckNumber = TruckNumber;
                entity.PhoneNumber = PhoneNumber;
                entity.TruckLength = TruckLength;
                entity.BookOrigin = BookOrigin;
                entity.GreenPassFlag = int.Parse(GreenPassFlag);
                entity.Id = int.Parse(Id);
                entity.UpdateUser= UserName;
                string result = cf.UpdateTruckQueueHeader(entity);
                Response.Write(result);
            }
            catch (Exception ex)
            {
                Response.Write(ex.ToString());
            }
        }
        //修改车辆明细信息
        [HttpGet]
        public void UpdateTruckQueueDetail()
        {
            try
            {
                var Id = Request["Id"];
                WCF.GMSService.TruckQueueDetailParam entity = new WCF.GMSService.TruckQueueDetailParam();
                entity.Id= int.Parse(Id);
                entity.ReceiptId = Request["edit_ReceiptId"];
                entity.UnloadingArea = Request["edit_UnloadingArea"];
                entity.ClientCode = Request["edit_ClientCode"];
                string txtQty = "0", txtCBM = "0", txtWeight = "0";
                if (Request["edit_Qty"] + "" != "" && Request["edit_Qty"] != null)
                {
                    txtQty = Request["edit_Qty"].Trim();
                }
                if (Request["edit_CBM"] != null && Request["edit_CBM"] + "" != "")
                {
                    txtCBM = Request["edit_CBM"].Trim();
                }
                if (Request["edit_Weight"] != null && Request["edit_Weight"] + "" != "")
                {
                    txtWeight = Request["edit_Weight"].Trim();
                }
                string UserName = "";
                try
                {
                    UserName = Session["userName"].ToString();
                }
                catch (Exception ex)
                {
                    Response.Write("登录已失效，请退出重新登录！");
                    return;
                }
                entity.Qty = int.Parse(txtQty);
                entity.CBM = decimal.Parse(txtCBM);
                entity.Weight = decimal.Parse(txtWeight);
                entity.GoodsType = Request["edit_GoodsType"];
                entity.RegisterDate = Request["edit_RegisterDate"];
                entity.BkIsValid = int.Parse(Request["edit_BkIsValid"]);
                entity.OverSizeFlag = int.Parse(Request["edit_OverSizeFlag"]);
                entity.NoticeFlag = int.Parse(Request["edit_NoticeFlag"]);
                entity.BookOrigin = Request["edit_BookOrigin"];
                entity.UpdateUser= UserName;
                string result = cf.UpdateTruckQueueDetail(entity);
                Response.Write(result);
            }
            catch (Exception ex)
            {
                Response.Write(ex.ToString());
            }
        }
        //获取客户
        [HttpPost]
        public ActionResult GetClientList()
        {
            WCF.InBoundService.InBoundServiceClient cf1 = new WCF.InBoundService.InBoundServiceClient();
            var sql= from r in cf1.WhClientListSelect(Session["whCode"].ToString())
                     select new  
                     {
                         Value = r.ClientCode,
                         Text = r.ClientCode    //text
                     };
            return Json(sql);
        }
        //超时锁定车辆解锁
        [HttpGet]
        public void unlockTruck()
        {
            string TruckNumber = Request["TruckNumber"].ToString();
            string WhCode = Request["WhCode"].ToString();
            string Id = Request["Id"];
            string UserName = "";
            try
            {
                UserName = Session["userName"].ToString();
            }
            catch (Exception ex)
            {
                Response.Write("登录已失效，请退出重新登录！");
                return;
            }
            WCF.GMSService.QueueParam queueParam = new WCF.GMSService.QueueParam();
            queueParam.Id = Id;
            queueParam.UserName = UserName;
            queueParam.TruckNumber = TruckNumber;
            queueParam.WhCode = WhCode;
            string result = cf.unlockTruck(queueParam);
            Response.Write(result);
        }
        //入库
        [HttpGet]
        public void getInGate()
        {
            try
            {
                string WhCode = Request["WhCode"];
                string TruckNumber = Request["TruckNumber"].ToString();
                //WCF.GMSService.GMSServiceClient cf = new WCF.GMSService.GMSServiceClient();
                //string UserName = Session["userName"].ToString();
                WCF.GMSService.ReceiptParam receiptParam = new WCF.GMSService.ReceiptParam();
                receiptParam.WhCode = WhCode;
                //queueParam.UserName = UserName;
                receiptParam.TruckNumber = TruckNumber;
                string result = cf.getInGate(receiptParam);
                Response.Write(result);
            }
            catch (Exception ex)
            {
                Response.Write(ex.ToString());
            }

        }
        //离库
        [HttpGet]
        public void leaveGate()
        {
            try
            {
                string WhCode = Request["WhCode"];
                string TruckNumber = Request["TruckNumber"].ToString();
                //string UserName = Session["userName"].ToString();
                WCF.GMSService.ReceiptParam receiptParam = new WCF.GMSService.ReceiptParam();
                receiptParam.WhCode = WhCode;
                //queueParam.UserName = UserName;
                receiptParam.TruckNumber = TruckNumber;
                string result = cf.leaveGate(receiptParam);
                Response.Write(result);
            }
            catch (Exception ex)
            {
                Response.Write(ex.ToString());
            }

        }
        //获取卸货小库区
        [HttpPost]
        public ActionResult GetSmallLoadAreaList()
        {
            string UnloadingArea = Request["UnloadingArea"].ToString();
            var sql = from r in cf.GetSmallLoadAreaList(Session["whCode"].ToString(), UnloadingArea)
                      select new
                      {
                          Value = r.ZoneName,
                          Text = r.ZoneName.ToString()    //text
                      };
            return Json(sql);
        }
    }
}

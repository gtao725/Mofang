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
    public class RegInBoundOrderController : Controller
    {
        WCF.InBoundService.InBoundServiceClient cf = new WCF.InBoundService.InBoundServiceClient();
        Helper Helper = new Helper();
        EIP.EIP EIP = new EIP.EIP();
        [DefaultRequest]
        //页面显示 （通用）
        public ActionResult Index()
        {
            ViewBag.whCode = Session["whCode"].ToString();

            ViewBag.CrSaveAddress = ConfigurationManager.AppSettings["CrAddress98"].ToString();

            ViewData["WhClientList"] = from r in cf.WhClientListSelect(Session["whCode"].ToString())
                                       select new SelectListItem()
                                       {
                                           Text = r.ClientCode,     //text
                                           Value = r.Id.ToString()
                                       };

            ViewData["zoneLocationList"] = from r in cf.RecZoneSelect(Session["whCode"].ToString(), 0)
                                           select new SelectListItem()
                                           {
                                               Text = r.ZoneName,     //text
                                               Value = r.ZoneName.ToString()
                                           };

            return View();
        }

        [DefaultRequest]
        //页面显示 （整出）
        public ActionResult IndexAll()
        {
            ViewBag.whCode = Session["whCode"].ToString();

            ViewBag.CrSaveAddress = ConfigurationManager.AppSettings["CrAddress98"].ToString();

            ViewData["WhClientList"] = from r in cf.WhClientListSelect(Session["whCode"].ToString())
                                       select new SelectListItem()
                                       {
                                           Text = r.ClientCode,     //text
                                           Value = r.Id.ToString()
                                       };

            ViewData["zoneLocationList"] = from r in cf.RecZoneSelect(Session["whCode"].ToString(), 0)
                                           select new SelectListItem()
                                           {
                                               Text = r.ZoneName,     //text
                                               Value = r.ZoneName.ToString()
                                           };

            return View();
        }

        [DefaultRequest]
        //页面显示 （非整出）
        public ActionResult IndexPart()
        {
            ViewBag.whCode = Session["whCode"].ToString();

            ViewBag.CrSaveAddress = ConfigurationManager.AppSettings["CrAddress98"].ToString();

            ViewData["WhClientList"] = from r in cf.WhClientListSelect(Session["whCode"].ToString())
                                       select new SelectListItem()
                                       {
                                           Text = r.ClientCode,     //text
                                           Value = r.Id.ToString()
                                       };
            ViewData["zoneLocationList"] = from r in cf.RecZoneSelect(Session["whCode"].ToString(), 0)
                                           select new SelectListItem()
                                           {
                                               Text = r.ZoneName,     //text
                                               Value = r.ZoneName.ToString()
                                           };

            return View();
        }

        //通过客户 得到客户流程列表
        [HttpPost]
        public ActionResult GetFlowNameSelList()
        {
            int clientId = Convert.ToInt32(Request["Hid_clientId"]);

            var sql = from r in cf.RegClientFlowNameSelect(clientId)
                      select new
                      {
                          Value = r.Id.ToString(),
                          Text = r.FlowName  //text
                      };
            return Json(sql);

        }

        //通过选择的客户得到收货区域
        [HttpPost]
        public ActionResult GetRecZoneNameList()
        {
            IEnumerable<WCF.InBoundService.WhZoneResult> list = cf.RecZoneSelect(Session["whCode"].ToString(), Convert.ToInt32(Request["txt_WhClient"]));
            if (list.Count() > 0)
            {
                var sql = from r in list
                          select new
                          {
                              Value = r.ZoneName.ToString(),
                              Text = r.ZoneName    //text
                          };
                return Json(sql);
            }
            else
            {
                return null;
            }
        }

        //得到收货区域
        [HttpPost]
        public ActionResult GetRecZoneNameList1()
        {
            IEnumerable<WCF.InBoundService.WhZoneResult> list = cf.RecZoneSelect(Session["whCode"].ToString(), 0);
            if (list != null)
            {
                var sql = from r in list
                          select new
                          {
                              Value = r.ZoneName.ToString(),
                              Text = r.ZoneName    //text
                          };
                return Json(sql);
            }
            else
            {
                return null;
            }
        }

        //显示收货批次列表 （通用）
        public ActionResult List()
        {
            WCF.InBoundService.ReceiptRegisterSearch entity = new WCF.InBoundService.ReceiptRegisterSearch();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.WhCode = Session["whCode"].ToString();
            entity.ReceiptId = Request["ReceiptId"].Trim();
            entity.TruckNumber = Request["TruckNumber"].Trim();
            entity.PhoneNumber = Request["PhoneNumber"].Trim();
            entity.ReceiptType = Request["ReceiptType"].Trim();
            entity.Status = Request["status"].Trim();
            if (Request["WhClientId"] == "")
            {
                entity.ClientId = 0;
            }
            else
            {
                entity.ClientId = Convert.ToInt32(Request["WhClientId"]);
            }
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
            if (Request["createUser"].Trim() != "")
            {
                entity.CreateUser = Session["userName"].ToString();
            }

            int total = 0;
            List<WCF.InBoundService.ReceiptRegisterResult> list = cf.ReceiptRegisterList(entity, out total).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Id", "操作");
            fieldsName.Add("WhCode", "仓库");
            fieldsName.Add("ReceiptId", "收货批次号");
            fieldsName.Add("ClientId", "客户ID");
            fieldsName.Add("ClientCode", "客户名");
            fieldsName.Add("LocationId", "收货区域");
            fieldsName.Add("Status", "状态");
            fieldsName.Add("DSFlag", "类型");

            fieldsName.Add("GreenPassFlagShow", "绿色通道");
            fieldsName.Add("SumQty", "原登记数量");
            fieldsName.Add("SumCBM", "原登记立方");
            fieldsName.Add("TruckNumber", "车牌号");
            fieldsName.Add("GoodType", "货物类型");

            fieldsName.Add("PhoneNumber", "手机号");
            fieldsName.Add("BkDate", "预约时间");

            fieldsName.Add("TruckCountShow", "车辆数量");
            fieldsName.Add("DNCountShow", "DN数");
            fieldsName.Add("FaxInCountShow", "传真收份数");
            fieldsName.Add("FaxOutCountShow", "传真发份数");
            fieldsName.Add("BillCountShow", "联单数");
            fieldsName.Add("ResetCountShow", "排队重置次数");

            fieldsName.Add("ArriveDate", "车辆到达时间");
            fieldsName.Add("ProcessName", "所选流程");
            fieldsName.Add("CreateUser", "创建人");
            fieldsName.Add("CreateDate", "创建时间");
            fieldsName.Add("GreenPassFlag", "GreenPassFlag");

            return Content(EIP.EipListJson(list, total, fieldsName, "Id:135,ReceiptId:110,ClientCode:110,Status:60,DSFlag:40,SumQty:70,SumCBM:70,ProcessName:110,CreateDate:110,ArriveDate:110,TruckNumber:90,CreateUser:50,BkDate:120,LocationId:65,GreenPassFlagShow:63,TruckCountShow:63,GoodType:63,ResetCountShow:90,DNCountShow:70,default:80"));
        }

        //新增收货登记 主表（通用）
        public ActionResult AddReceiptRegister()
        {
            WCF.InBoundService.ReceiptRegister entity = new WCF.InBoundService.ReceiptRegister();
            entity.WhCode = Session["whCode"].ToString();
            entity.ClientId = Convert.ToInt32(Request["txt_WhClient"]);
            entity.ClientCode = Request["clientCode"].Trim();
            entity.ReceiptType = Request["ReceiptType"].Trim();
            entity.TruckNumber = Request["txt_TruckNumber"].Trim().ToUpper();
            entity.PhoneNumber = Request["txt_PhoneNumber"].Trim();
            entity.LocationId = Request["txt_recZone"].Trim();
            entity.ArriveDate = Convert.ToDateTime(Request["txt_ArriveDate"]);
            entity.CreateUser = Session["userName"].ToString();
            entity.GoodType = Request["txt_GoodType"].Trim();

            if (Request["txt_TruckCount"] != null)
            {
                if (Request["txt_TruckCount"] != "")
                {
                    entity.TruckCount = Convert.ToInt32(Request["txt_TruckCount"]);
                }
                else
                {
                    entity.TruckCount = 1;
                }
            }
            else
            {
                entity.TruckCount = 1;
            }

            if (Request["txt_DNCount"] != null)
            {
                if (Request["txt_DNCount"] != "")
                {
                    entity.DNCount = Convert.ToInt32(Request["txt_DNCount"]);
                }
                else
                {
                    entity.DNCount = 1;
                }
            }
            else
            {
                entity.DNCount = 1;
            }

            if (Request["txt_FaxInCount"] != null)
            {
                if (Request["txt_FaxInCount"] != "")
                {
                    entity.FaxInCount = Convert.ToInt32(Request["txt_FaxInCount"]);
                }
                else
                {
                    entity.FaxInCount = 0;
                }
            }
            else
            {
                entity.FaxInCount = 0;
            }


            if (Request["txt_FaxOutCount"] != null)
            {
                if (Request["txt_FaxOutCount"] != "")
                {
                    entity.FaxOutCount = Convert.ToInt32(Request["txt_FaxOutCount"]);
                }
                else
                {
                    entity.FaxOutCount = 0;
                }
            }
            else
            {
                entity.FaxOutCount = 0;
            }


            if (Request["txt_BillCount"] != null)
            {
                if (Request["txt_BillCount"] != "")
                {
                    entity.BillCount = Convert.ToInt32(Request["txt_BillCount"]);
                }
                else
                {
                    entity.BillCount = 0;
                }
            }
            else
            {
                entity.BillCount = 0;
            }


            if (Request["txt_GreenPassFlag"] != null)
            {
                if (Request["txt_GreenPassFlag"] != "")
                {
                    entity.GreenPassFLag = Convert.ToInt32(Request["txt_GreenPassFlag"].Trim());
                }
            }

            if (Request["txt_bkDate"] != null)
            {
                if (Request["txt_bkDate"] != "")
                {
                    string s = Convert.ToDateTime(Request["txt_ArriveDate"]).ToString("yyyy-MM-dd");
                    entity.BkDate = s + " " + Request["txt_bkDate"];
                }
            }

            string checkTruck = "";
            if (!string.IsNullOrEmpty(entity.TruckNumber))
            {
                //char[] c = entity.TruckNumber.Substring(0, 1).ToCharArray();
                //if (c[0] >= 0x4e00 && c[0] <= 0x9fbb)
                //{

                //}
                //else
                //{
                //    checkTruck = "车牌号异常，格式应为：沪AXXXXX 汉字+6位！";
                //}

                //if (entity.TruckNumber.Substring(1, entity.TruckNumber.Length - 1).Length != 6)
                //{
                //    checkTruck = "车牌号异常，格式应为：沪AXXXXX 汉字+6位！";
                //}

                //if (!Regex.IsMatch(entity.TruckNumber.Substring(1, entity.TruckNumber.Length - 1), @"(?i)^[0-9A-Z]+$"))
                //{
                //    checkTruck = "车牌号异常，格式应为：沪AXXXXX 汉字+6位！";
                //}
            }

            if (checkTruck != "")
            {
                return Helper.RedirectAjax("err", checkTruck, null, "");
            }
            else
            {
                WCF.InBoundService.ReceiptRegister result = cf.AddReceiptRegister1(entity);
                if (result != null)
                {
                    return Helper.RedirectAjax("ok", "添加成功！", result, "");
                }
                else
                {
                    return Helper.RedirectAjax("err", "对不起，添加失败！", null, "");
                }
            }
        }

        //新增收货登记明细 （通用）
        public ActionResult AddReceiptRegisterDetail()
        {
            string[] Id = Request.Form.GetValues("idarr");
            string[] CustomerPoNumber = Request.Form.GetValues("poarr");
            string[] AltItemNumber = Request.Form.GetValues("skuarr");
            string[] PoId = Request.Form.GetValues("poIdarr");
            string[] ItemId = Request.Form.GetValues("itemIdarr");
            string[] UnitId = Request.Form.GetValues("unitIdarr");
            string[] UnitName = Request.Form.GetValues("unitNamearr");
            string[] RegQty = Request.Form.GetValues("qtyarr");
            string[] ProcessId = Request.Form.GetValues("proarr");
            string[] ProcessName = Request.Form.GetValues("proNamearr");

            string checkProcessId = ProcessId[0];
            string checkResult = "";
            for (int i = 0; i < ProcessId.Length; i++)
            {
                if (checkProcessId != ProcessId[i])
                {
                    checkResult = "所选流程不一致，请重新选择！";
                }
            }
            if (checkResult != "")
            {
                return Helper.RedirectAjax("err", checkResult, null, "");
            }

            WCF.InBoundService.ReceiptRegisterInsert recEntity = new WCF.InBoundService.ReceiptRegisterInsert();
            recEntity.ReceiptId = Request["ReceiptId"].Trim();
            recEntity.WhCode = Session["whCode"].ToString();
            recEntity.ProcessName = ProcessName[0];
            if (checkProcessId != "")
            {
                recEntity.ProcessId = Convert.ToInt32(checkProcessId);
            }
            else
            {
                recEntity.ProcessId = 0;
            }
            //验证收货批次流程
            string resultCheckProssName = cf.CheckReceiptProssName(recEntity);
            if (resultCheckProssName != "")
            {
                return Helper.RedirectAjax("err", resultCheckProssName, null, "");
            }


            List<WCF.InBoundService.ReceiptRegisterInsert> list = new List<WCF.InBoundService.ReceiptRegisterInsert>();

            for (int i = 0; i < Id.Length; i++)
            {
                WCF.InBoundService.ReceiptRegisterInsert entity = new WCF.InBoundService.ReceiptRegisterInsert();
                entity.WhCode = Session["whCode"].ToString();
                entity.ReceiptId = Request["ReceiptId"].Trim();
                entity.InBoundOrderDetailId = Convert.ToInt32(Id[i].ToString());
                entity.CustomerPoNumber = CustomerPoNumber[i].Trim().ToString();
                entity.AltItemNumber = AltItemNumber[i].Trim().ToString();
                entity.PoId = Convert.ToInt32(PoId[i].ToString());
                entity.ItemId = Convert.ToInt32(ItemId[i].ToString());
                entity.UnitId = Convert.ToInt32(UnitId[i].ToString());
                entity.ProcessName = ProcessName[0];
                if (checkProcessId != "")
                {
                    entity.ProcessId = Convert.ToInt32(checkProcessId);
                }
                else
                {
                    entity.ProcessId = 0;
                }

                if (UnitName[i].ToString() == "")
                {
                    entity.UnitName = "none";
                }
                else
                {
                    entity.UnitName = UnitName[i].Trim().ToString();
                }
                entity.RegQty = Convert.ToInt32(RegQty[i].ToString());
                entity.CreateUser = Session["userName"].ToString();
                entity.CreateDate = DateTime.Now;
                list.Add(entity);
            }
            string result = cf.AddReceiptRegisterDetail(list.ToArray());
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "添加成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }
        }

        //删除收货登记 （通用）
        public ActionResult DelReceiptRegister()
        {
            WCF.InBoundService.ReceiptRegister entity = new WCF.InBoundService.ReceiptRegister();
            entity.ReceiptId = Request["ReceiptId"];
            entity.WhCode = Session["whCode"].ToString();
            entity.UpdateUser = Session["userName"].ToString();
            entity.UpdateDate = DateTime.Now;

            string result = cf.DelReceiptRegister(entity);
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "删除成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }
        }

        //修改收货登记 （通用）
        public ActionResult EditReceiptRegister()
        {
            WCF.InBoundService.ReceiptRegister entity = new WCF.InBoundService.ReceiptRegister();
            entity.ReceiptId = Request["ReceiptId"].Trim();
            entity.WhCode = Session["whCode"].ToString();
            entity.LocationId = Request["LocationId"].Trim();
            entity.TruckNumber = Request["TruckNumber"].Trim().ToUpper();
            entity.PhoneNumber = Request["Phone"].Trim();
            entity.UpdateUser = Session["userName"].ToString();
            entity.UpdateDate = DateTime.Now;

            entity.GoodType = Request["GoodType"].Trim();

            string s = Request["GreenPassFlag"].Trim();
            if (Request["GreenPassFlag"] != "null")
            {
                if (Request["GreenPassFlag"] != "")
                {
                    entity.GreenPassFLag = Convert.ToInt32(Request["GreenPassFlag"].Trim());
                }
                else
                {
                    entity.GreenPassFLag = 0;
                }
            }
            else
            {
                entity.GreenPassFLag = 0;
            }

            if (Request["TruckCount"] != "null")
            {
                if (Request["TruckCount"] != "")
                {
                    entity.TruckCount = Convert.ToInt32(Request["TruckCount"]);
                }
                else
                {
                    entity.TruckCount = 1;
                }
            }
            else
            {
                entity.TruckCount = 1;
            }

            if (Request["DNCount"] != "null")
            {
                if (Request["DNCount"] != "")
                {
                    entity.DNCount = Convert.ToInt32(Request["DNCount"]);
                }
                else
                {
                    entity.DNCount = 1;
                }
            }
            else
            {
                entity.DNCount = 1;
            }

            if (Request["FaxInCount"] != "null")
            {
                if (Request["FaxInCount"] != "")
                {
                    entity.FaxInCount = Convert.ToInt32(Request["FaxInCount"]);
                }
                else
                {
                    entity.FaxInCount = 0;
                }
            }
            else
            {
                entity.FaxInCount = 0;
            }


            if (Request["FaxOutCount"] != "null")
            {
                if (Request["FaxOutCount"] != "")
                {
                    entity.FaxOutCount = Convert.ToInt32(Request["FaxOutCount"]);
                }
                else
                {
                    entity.FaxOutCount = 0;
                }
            }
            else
            {
                entity.FaxOutCount = 0;
            }


            if (Request["BillCount"] != "null")
            {
                if (Request["BillCount"] != "")
                {
                    entity.BillCount = Convert.ToInt32(Request["BillCount"]);
                }
                else
                {
                    entity.BillCount = 0;
                }
            }
            else
            {
                entity.BillCount = 0;
            }

            if (Request["ResetCount"] != "null")
            {
                if (Request["ResetCount"] != "")
                {
                    entity.ResetCount = Convert.ToInt32(Request["ResetCount"]);
                }
                else
                {
                    entity.ResetCount = 0;
                }
            }
            else
            {
                entity.ResetCount = 0;
            }


            if (Request["CBM"] != "null")
            {
                if (Request["CBM"] != "")
                {
                    entity.SumCBM = Convert.ToDecimal(Request["CBM"]);
                }
                else
                {
                    entity.SumCBM = 0;
                }
            }
            else
            {
                entity.SumCBM = 0;
            }


            string checkTruck = "";
            if (!string.IsNullOrEmpty(entity.TruckNumber))
            {
                //char[] c = entity.TruckNumber.Substring(0, 1).ToCharArray();
                //if (c[0] >= 0x4e00 && c[0] <= 0x9fbb)
                //{

                //}
                //else
                //{
                //    checkTruck = "车牌号异常，格式应为：沪AXXXXX 汉字+6位！";
                //}

                //if (entity.TruckNumber.Substring(1, entity.TruckNumber.Length - 1).Length != 6)
                //{
                //    checkTruck = "车牌号异常，格式应为：沪AXXXXX 汉字+6位！";
                //}

                //if (!Regex.IsMatch(entity.TruckNumber.Substring(1, entity.TruckNumber.Length - 1), @"(?i)^[0-9A-Z]+$"))
                //{
                //    checkTruck = "车牌号异常，格式应为：沪AXXXXX 汉字+6位！";
                //}
            }

            if (checkTruck != "")
            {
                return Helper.RedirectAjax("err", checkTruck, null, "");
            }
            else
            {
                string result = cf.EditReceiptRegister(entity, new string[] { "LocationId", "TruckNumber", "PhoneNumber", "UpdateUser", "UpdateDate", "GreenPassFLag", "TruckCount", "FaxInCount", "FaxOutCount", "BillCount", "ResetCount", "DNCount", "GoodType", "SumCBM" });
                if (result == "Y")
                {
                    return Helper.RedirectAjax("ok", "修改成功！", null, "");
                }
                else
                {
                    return Helper.RedirectAjax("err", result, null, "");
                }
            }

        }

        //修改收货登记明细 （通用）
        public ActionResult EditReceiptRegisterDetail()
        {
            WCF.InBoundService.ReceiptRegisterDetailEdit entity = new WCF.InBoundService.ReceiptRegisterDetailEdit();
            entity.Id = Convert.ToInt32(Request["id"]);
            entity.InOrderDetailId = Convert.ToInt32(Request["inOrderDetailId"]);
            entity.ReceiptId = Request["receiptId"].Trim();
            entity.WhCode = Session["whCode"].ToString();
            entity.RegQty = Convert.ToInt32(Request["editQty"]);
            entity.DiffQty = Convert.ToInt32(Request["regQty"]) - Convert.ToInt32(Request["editQty"]);
            entity.UpdateUser = Session["userName"].ToString();
            entity.UpdateDate = DateTime.Now;

            string result = cf.EditReceiptRegisterDetail(entity);
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "修改成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }
        }

        //删除收货批次明细 （通用）
        public ActionResult DelReceiptRegisterDetail()
        {
            WCF.InBoundService.ReceiptRegisterDetailEdit entity = new WCF.InBoundService.ReceiptRegisterDetailEdit();
            entity.Id = Convert.ToInt32(Request["id"]);
            entity.InOrderDetailId = Convert.ToInt32(Request["inOrderDetailId"]);
            entity.RegQty = Convert.ToInt32(Request["regQty"]);
            entity.UpdateUser = Session["userName"].ToString();
            entity.UpdateDate = DateTime.Now;
            entity.ReceiptId = Request["receiptId"].Trim();
            entity.WhCode = Session["whCode"].ToString();

            string result = cf.DelReceiptRegisterDetail(entity);
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "删除成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }
        }

        public ActionResult BatchDelReceiptRegisterDetail()
        {
            string[] Id = Request.Form.GetValues("idarr");
            string[] inOrderDetailId = Request.Form.GetValues("inOrderDetailIdarr");
            string[] regQty = Request.Form.GetValues("regQtyarr");
            string[] receiptId = Request.Form.GetValues("receiptIdarr");

            string result = "";
            for (int i = 0; i < Id.Length; i++)
            {
                WCF.InBoundService.ReceiptRegisterDetailEdit entity = new WCF.InBoundService.ReceiptRegisterDetailEdit();
                entity.Id = Convert.ToInt32(Id[i]);
                entity.InOrderDetailId = Convert.ToInt32(inOrderDetailId[i]);
                entity.RegQty = Convert.ToInt32(regQty[i]);
                entity.UpdateUser = Session["userName"].ToString();
                entity.UpdateDate = DateTime.Now;
                entity.ReceiptId = receiptId[i];
                entity.WhCode = Session["whCode"].ToString();
                result = cf.DelReceiptRegisterDetail(entity);
            }

            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "删除成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }
        }

        //添加进仓单 查询（通用）
        public ActionResult List_Com()
        {
            WCF.InBoundService.InBoundOrderDetailSearch entity = new WCF.InBoundService.InBoundOrderDetailSearch();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.WhCode = Session["whCode"].ToString();
            entity.ClientId = Convert.ToInt32(Request["Hid_clientId"]);

            string soNumber = Request["so_area"];
            string[] soNumberList = null;
            if (!string.IsNullOrEmpty(soNumber))
            {
                string so_temp = soNumber.Replace("\r\n", "@");    //把so中的空格 替换为@符号
                soNumberList = so_temp.Split('@');           //把SO 按照@分割，放在数组
            }
            string poNumber = Request["po_area"];
            string[] poNumberList = null;
            if (!string.IsNullOrEmpty(poNumber))
            {
                string po_temp = poNumber.Replace("\r\n", "@");    //把so中的空格 替换为@符号
                poNumberList = po_temp.Split('@');           //把PO 按照@分割，放在数组
            }

            string skuNumber = Request["sku_area"];
            string[] skuNumberList = null;
            if (!string.IsNullOrEmpty(skuNumber))
            {
                string sku_temp = skuNumber.Replace("\r\n", "@");    //把so中的空格 替换为@符号
                skuNumberList = sku_temp.Split('@');           //把SO 按照@分割，放在数组
            }


            if (Request["sel_flowName"] != "")
            {
                entity.ProcessId = Convert.ToInt32(Request["sel_flowName"]);
            }
            else
            {
                entity.ProcessId = 0;
            }
            int total = 0;
            string str = "";
            List<WCF.InBoundService.InBoundOrderDetailResult> list = cf.RegInBoundOrderListCom(entity, soNumberList, poNumberList, skuNumberList, out total, out str).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Id", "操作");
            fieldsName.Add("ClientId", "客户ID");
            fieldsName.Add("ClientCode", "客户名");
            fieldsName.Add("ProcessId", "流程ID");
            fieldsName.Add("ProcessName", "流程名称");
            fieldsName.Add("UnitId", "单位ID");
            fieldsName.Add("SoNumber", "SO号");
            fieldsName.Add("CustomerPoNumber", "PO号");
            fieldsName.Add("AltItemNumber", "款号");
            fieldsName.Add("PoId", "POId");
            fieldsName.Add("ItemId", "ItemId");
            fieldsName.Add("Qty", "可登记数量");
            fieldsName.Add("Style1", "属性1");
            fieldsName.Add("Style2", "属性2");
            fieldsName.Add("Style3", "属性3");
            fieldsName.Add("UnitName", "单位名称");
            fieldsName.Add("Weight", "重量");
            fieldsName.Add("CBM", "体积");

            return Content(EIP.EipListJson(list, total, fieldsName, "Id:60,ClientCode:120,ProcessName:140,SoNumber:120,CustomerPoNumber:120,AltItemNumber:120,default:65", null, "", 200, str));
        }

        //显示收货批次明细 （通用）
        public ActionResult ReceiptRegisterDetailListCom()
        {
            WCF.InBoundService.ReceiptRegisterDetailSearch entity = new WCF.InBoundService.ReceiptRegisterDetailSearch();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.WhCode = Session["whCode"].ToString();
            entity.ReceiptId = Request["ReceiptId"];

            int total = 0;
            string str = "";
            List<WCF.InBoundService.ReceiptRegisterDetailResult> list = cf.ReceiptRegisterDetailListCom(entity, out total, out str).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Id", "操作");
            fieldsName.Add("InOrderDetailId", "预录明细ID");
            fieldsName.Add("SoNumber", "SO号");
            fieldsName.Add("CustomerPoNumber", "PO号");
            fieldsName.Add("AltItemNumber", "款号");
            fieldsName.Add("Style1", "属性1");
            fieldsName.Add("Style2", "属性2");
            fieldsName.Add("Style3", "属性3");
            fieldsName.Add("RegQty", "登记数量");
            fieldsName.Add("UnitName", "单位名称");


            return Content(EIP.EipListJson(list, total, fieldsName, "Id:60,SoNumber:140,CustomerPoNumber:140,AltItemNumber:140,RegQty:60,UnitName:60,default:90", null, "", 200, str));
        }




        //添加进仓单 查询（整出）
        public ActionResult List_CFS()
        {
            WCF.InBoundService.InBoundOrderDetailSearch entity = new WCF.InBoundService.InBoundOrderDetailSearch();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.WhCode = Session["whCode"].ToString();
            entity.OrderType = "CFS";
            entity.ClientId = Convert.ToInt32(Request["Hid_clientId"]);

            string soNumber = Request["so_area"];
            string poNumber = Request["po_area"];
            string skuNumber = Request["sku_area"];

            string[] soNumberList = null;
            if (!string.IsNullOrEmpty(soNumber))
            {
                string so_temp = soNumber.Replace("\r\n", "@");    //把so中的空格 替换为@符号
                soNumberList = so_temp.Split('@');           //把SO 按照@分割，放在数组
            }

            string[] poNumberList = null;
            if (!string.IsNullOrEmpty(poNumber))
            {
                string po_temp = poNumber.Replace("\r\n", "@");    //把so中的空格 替换为@符号
                poNumberList = po_temp.Split('@');           //把SO 按照@分割，放在数组
            }

            string[] skuNumberList = null;
            if (!string.IsNullOrEmpty(skuNumber))
            {
                string sku_temp = skuNumber.Replace("\r\n", "@");    //把so中的空格 替换为@符号
                skuNumberList = sku_temp.Split('@');           //把SO 按照@分割，放在数组
            }

            if (Request["sel_flowName"] != "")
            {
                entity.ProcessId = Convert.ToInt32(Request["sel_flowName"]);
            }
            else
            {
                entity.ProcessId = 0;
            }
            int total = 0;
            string str = "";
            List<WCF.InBoundService.InBoundOrderDetailResult> list = cf.RegInBoundOrderListCFS(entity, soNumberList, poNumberList, skuNumberList, out total, out str).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Id", "操作");
            fieldsName.Add("ClientId", "客户ID");
            fieldsName.Add("ClientCode", "客户名");
            fieldsName.Add("ProcessId", "流程ID");
            fieldsName.Add("ProcessName", "流程名称");
            fieldsName.Add("UnitId", "单位ID");
            fieldsName.Add("SoNumber", "SO号");
            fieldsName.Add("CustomerPoNumber", "PO号");
            fieldsName.Add("AltItemNumber", "款号");
            fieldsName.Add("PoId", "POId");
            fieldsName.Add("ItemId", "ItemId");
            fieldsName.Add("Qty", "可登记数量");
            fieldsName.Add("Style1", "属性1");
            fieldsName.Add("Style2", "属性2");
            fieldsName.Add("Style3", "属性3");
            fieldsName.Add("UnitName", "单位名称");
            fieldsName.Add("Weight", "重量");
            fieldsName.Add("CBM", "体积");

            return Content(EIP.EipListJson(list, total, fieldsName, "Id:60,ClientCode:120,ProcessName:140,SoNumber:120,CustomerPoNumber:120,AltItemNumber:120,default:65", null, "", 200, str));
        }


        //显示收货批次明细 （整出）
        public ActionResult ReceiptRegisterDetailList()
        {
            WCF.InBoundService.ReceiptRegisterDetailSearch entity = new WCF.InBoundService.ReceiptRegisterDetailSearch();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.WhCode = Session["whCode"].ToString();
            entity.ReceiptId = Request["ReceiptId"];

            int total = 0;
            string str = "";
            List<WCF.InBoundService.ReceiptRegisterDetailResult> list = cf.ReceiptRegisterDetailList(entity, out total, out str).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Id", "操作");
            fieldsName.Add("InOrderDetailId", "预录明细ID");
            fieldsName.Add("SoNumber", "SO号");
            fieldsName.Add("CustomerPoNumber", "PO号");
            fieldsName.Add("AltItemNumber", "款号");
            fieldsName.Add("Style1", "属性1");
            fieldsName.Add("Style2", "属性2");
            fieldsName.Add("Style3", "属性3");
            fieldsName.Add("RegQty", "登记数量");
            fieldsName.Add("UnitName", "单位名称");

            return Content(EIP.EipListJson(list, total, fieldsName, "Id:70,default:90", null, "", 200, str));
        }



        //添加进仓单 查询（非整出）
        public ActionResult List_DC()
        {
            WCF.InBoundService.InBoundOrderDetailSearch entity = new WCF.InBoundService.InBoundOrderDetailSearch();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.WhCode = Session["whCode"].ToString();
            entity.OrderType = "DC";
            entity.ClientId = Convert.ToInt32(Request["Hid_clientId"]);

            string poNumber = Request["po_area"];
            string[] poNumberList = null;
            if (!string.IsNullOrEmpty(poNumber))
            {
                string po_temp = poNumber.Replace("\r\n", "@");    //把so中的空格 替换为@符号
                poNumberList = po_temp.Split('@');           //把SO 按照@分割，放在数组
            }

            string skuNumber = Request["sku_area"];
            string[] skuNumberList = null;
            if (!string.IsNullOrEmpty(skuNumber))
            {
                string sku_temp = skuNumber.Replace("\r\n", "@");    //把so中的空格 替换为@符号
                skuNumberList = sku_temp.Split('@');           //把SO 按照@分割，放在数组
            }

            if (Request["sel_flowName"] != "")
            {
                entity.ProcessId = Convert.ToInt32(Request["sel_flowName"]);
            }
            else
            {
                entity.ProcessId = 0;
            }
            int total = 0;
            string str = "";
            List<WCF.InBoundService.InBoundOrderDetailResult> list = cf.RegInBoundOrderListDC(entity, poNumberList, skuNumberList, out total, out str).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Id", "操作");
            fieldsName.Add("ClientId", "客户ID");
            fieldsName.Add("ClientCode", "客户名");
            fieldsName.Add("ProcessId", "流程ID");
            fieldsName.Add("ProcessName", "流程名称");
            fieldsName.Add("UnitId", "单位ID");   
            fieldsName.Add("CustomerPoNumber", "PO号");
            fieldsName.Add("AltItemNumber", "款号");
            fieldsName.Add("PoId", "POId");
            fieldsName.Add("ItemId", "ItemId");
            fieldsName.Add("Qty", "可登记数量");
            fieldsName.Add("Style1", "属性1");
            fieldsName.Add("Style2", "属性2");
            fieldsName.Add("Style3", "属性3");
            fieldsName.Add("UnitName", "单位名称");
            fieldsName.Add("Weight", "重量");
            fieldsName.Add("CBM", "体积");

            return Content(EIP.EipListJson(list, total, fieldsName, "Id:60,ClientCode:120,ProcessName:140,CustomerPoNumber:120,AltItemNumber:120,default:65", null, "", 200, str));
        }


        //显示收货批次明细 (非整出)
        public ActionResult ReceiptRegisterDetailPartList()
        {
            WCF.InBoundService.ReceiptRegisterDetailSearch entity = new WCF.InBoundService.ReceiptRegisterDetailSearch();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.WhCode = Session["whCode"].ToString();
            entity.ReceiptId = Request["ReceiptId"];

            int total = 0;
            string str = "";
            List<WCF.InBoundService.ReceiptRegisterDetailResult> list = cf.ReceiptRegisterDetailPartList(entity, out total, out str).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Id", "操作");
            fieldsName.Add("InOrderDetailId", "预录明细ID");
            fieldsName.Add("CustomerPoNumber", "PO号");
            fieldsName.Add("AltItemNumber", "款号");
            fieldsName.Add("Style1", "属性1");
            fieldsName.Add("Style2", "属性2");
            fieldsName.Add("Style3", "属性3");
            fieldsName.Add("RegQty", "登记数量");
            fieldsName.Add("UnitName", "单位名称");

            return Content(EIP.EipListJson(list, total, fieldsName, "Id:70,default:90", null, "", 200, str));
        }

        //释放收货批次号
        [HttpPost]
        public void ReleaseReceipt()
        {
            string receiptId = Request["receiptId"];
            string result = cf.ReleaseReceipt(Session["whCode"].ToString(), receiptId);
            Response.Write(result);
        }

        //释放成功后 显示列表
        public ActionResult DSReceiptRegisterList()
        {
            WCF.InBoundService.ReceiptRegisterSearch entity = new WCF.InBoundService.ReceiptRegisterSearch();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.WhCode = Session["whCode"].ToString();
            entity.ReceiptId = Request["ReceiptId"].Trim();

            int total = 0;
            List<WCF.InBoundService.ReceiptRegisterResult> list = cf.DSReceiptRegisterList(entity, out total).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Id", "操作");
            fieldsName.Add("ReceiptId", "收货批次号");
            fieldsName.Add("DSFlag", "类型");
            fieldsName.Add("LocationId", "收货区域");
            fieldsName.Add("Status", "状态");
            fieldsName.Add("SumQty", "总数量");
            fieldsName.Add("ProcessName", "所选流程");
            fieldsName.Add("TruckNumber", "车牌号");

            return Content(EIP.EipListJson(list, total, fieldsName, "ReceiptId:110,Id:90,CreateDate:115,ArriveDate:115,default:80"));
        }

        [HttpGet]
        public ActionResult RollbackReceiptId()
        {
            int id = Convert.ToInt32(Request["Id"]);

            WCF.InBoundService.ReceiptRegister reg = new WCF.InBoundService.ReceiptRegister();
            reg.Id = id;
            reg.UpdateUser = Session["userName"].ToString();

            string result = cf.RollbackReceiptId(reg);
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "撤销成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "撤销失败！", null, "");
            }
        }


        //打印收货操作单
        [HttpGet]
        public void PrintReceiptId()
        {
            string result = cf.PrintInTempalte(Session["whCode"].ToString(), Request["ReceiptId"]);
            Response.Write(result);
        }
    }
}

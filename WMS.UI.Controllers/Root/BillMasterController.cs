using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace WMS.UI.Controllers
{
    public class BillMasterController : Controller
    {
        WCF.InBoundService.InBoundServiceClient cf = new WCF.InBoundService.InBoundServiceClient();
        WCF.RootService.RootServiceClient cf1 = new WCF.RootService.RootServiceClient();
        WCF.OutBoundService.OutBoundServiceClient outboundcf = new WCF.OutBoundService.OutBoundServiceClient();

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

            return View();
        }

        //查询
        public ActionResult List()
        {
            WCF.RootService.BillMasterSearch entity = new WCF.RootService.BillMasterSearch();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.WhCode = Session["whCode"].ToString();

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

            if (Request["createUser"] != "")
            {
                entity.CreateUser = Session["userName"].ToString();
            }

            int total = 0;
            List<WCF.RootService.BillMaster> list = cf1.BillMasterList(entity, out total).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Id", "操作");
            fieldsName.Add("BLNumber", "系统编号");
            fieldsName.Add("Status", "状态");
            fieldsName.Add("Description", "备注");

            fieldsName.Add("CreateUser", "创建人");
            fieldsName.Add("CreateDate", "创建时间");

            return Content(EIP.EipListJson(list, total, fieldsName, "Id:110,BLNumber:120,Description:150,Status:70,CreateDate:130,CreateUser:50,default:80"));
        }

        //新增
        public ActionResult AddFeeMaster()
        {
            WCF.RootService.BillMaster entity = new WCF.RootService.BillMaster();
            entity.WhCode = Session["whCode"].ToString();
            entity.Description = Request["txt_Description"].Trim();

            entity.CreateUser = Session["userName"].ToString();

            WCF.RootService.BillMaster result = cf1.BillMasterAdd(entity);
            if (result != null)
            {
                return Helper.RedirectAjax("ok", "添加成功！", result, "");
            }
            else
            {
                return Helper.RedirectAjax("err", "对不起，添加失败！", null, "");
            }
        }

        public ActionResult FeeDetailAddList()
        {
            WCF.RootService.BillDetailSearch entity = new WCF.RootService.BillDetailSearch();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.WhCode = Session["whCode"].ToString();

            entity.AgentCode = Request["fee_agentCode"];
            entity.ClientCode = Request["fee_clientCode"];

            entity.FeeStatus = Request["fee_LoadChargeStatus"];

            string so_area = Request["fee_loadId_area"];
            string[] loadList = null;
            if (!string.IsNullOrEmpty(so_area))
            {
                string so_temp = so_area.Replace("\r\n", "@");    //把so中的空格 替换为@符号
                loadList = so_temp.Split('@');           //把SO 按照@分割，放在数组
            }

            string po_area = Request["fee_containerNumber_area"];
            string[] containerList = null;
            if (!string.IsNullOrEmpty(po_area))
            {
                string so_temp = po_area.Replace("\r\n", "@");    //把so中的空格 替换为@符号
                containerList = so_temp.Split('@');           //把SO 按照@分割，放在数组
            }

            if (Request["fee_createDateBegin"] != "")
            {
                entity.BeginCreateDate = Convert.ToDateTime(Request["fee_createDateBegin"]);
            }
            else
            {
                entity.BeginCreateDate = null;
            }

            if (Request["fee_createDateEnd"] != "")
            {
                entity.EndCreateDate = Convert.ToDateTime(Request["fee_createDateEnd"]).AddDays(1);
            }
            else
            {
                entity.EndCreateDate = null;
            }

            int total = 0;
            List<WCF.RootService.BillDetailResult> list = cf1.GetLoadMasterList(entity, loadList, containerList, out total).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();

            fieldsName.Add("Action", "操作");
            fieldsName.Add("AgentCode", "代理名");
            fieldsName.Add("ClientCode", "客户名");

            fieldsName.Add("LoadId", "Load");
            fieldsName.Add("ContainerNumber", "箱号");
            fieldsName.Add("ContainerType", "箱型");

            fieldsName.Add("SumCBM", "总立方");
            fieldsName.Add("SumQty", "总数量");
            fieldsName.Add("Status", "状态");
            fieldsName.Add("FeeStatus", "费用状态");
            fieldsName.Add("CreateDate", "ETD日期");

            return Content(EIP.EipListJson(list, total, fieldsName, "Action:40,LoadId:125,ContainerNumber:105,AgentCode:100,ClientCode:100,ContainerType:45,CreateDate:130,default:60"));
        }

        [HttpPost]
        public ActionResult BillDetailAdd()
        {
            string so_area = Request["loadId"];
            string[] loadList = null;
            if (!string.IsNullOrEmpty(so_area))
            {
                string so_temp = so_area.Replace(",", "@");    //把so中的空格 替换为@符号
                loadList = so_temp.Split('@');           //把SO 按照@分割，放在数组
            }

            string result = cf1.BillDetailAdd(Session["whCode"].ToString(), Request["BLNumber"], Session["userName"].ToString(), loadList);
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "添加成功！", result, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }
        }


        public ActionResult showFeeDetailAddList()
        {
            WCF.RootService.BillDetailSearch entity = new WCF.RootService.BillDetailSearch();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.WhCode = Session["whCode"].ToString();
            entity.BLNumber = Request["BLNumber"];

            entity.LoadId = Request["txt_detail_loadId"];
            entity.ContainerNumber = Request["txt_detail_containerNumber"];
            entity.SoNumber= Request["txt_detail_soNumber"].Trim();

            if (Request["txt_detail_ETD_begin"] != "")
            {
                entity.BeginETD = Convert.ToDateTime(Request["txt_detail_ETD_begin"]);
            }
            else
            {
                entity.BeginETD = null;
            }
            if (Request["txt_detail_ETD_end"] != "")
            {
                entity.EndETD = Convert.ToDateTime(Request["txt_detail_ETD_end"]).AddDays(1);
            }
            else
            {
                entity.EndETD = null;
            }


            List<string> chargeName = new List<string>();
            chargeName.Add("装箱费");
            chargeName.Add("进港费");

            List<string> clientCodeNotIn = new List<string>();
            clientCodeNotIn.Add("DAMCO");
            clientCodeNotIn.Add("HM");

            entity.ReportAgentType = "Damco";
            entity.ReportType = "装箱费";
            entity.OceScmType = "SCM";
            entity.NotLoadFlag = 0;

            int total = 0;
            List<WCF.RootService.BillDetailRepostResult> list = cf1.DamcoBillDetailList(entity, chargeName.ToArray(), null, clientCodeNotIn.ToArray(), out total).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();

            fieldsName.Add("LoadId", "操作");
            fieldsName.Add("origin", "origin/丹马士分公司");
            fieldsName.Add("invoice_name", "invoice_name");
            fieldsName.Add("invoice_no", "invoice_no/发票号");
            fieldsName.Add("due_date", "due_date/到期付款日");
            fieldsName.Add("consignee_name", "consignee_name/客人名称");
            fieldsName.Add("sending_date", "Sending_Date/发账单日期");
            fieldsName.Add("etd_show", "etd/开航日期");
            fieldsName.Add("booking_no", "Booking_NO/SO/订舱号");
            fieldsName.Add("cbl", "cbl/SWB/提单号/海运单号");
            fieldsName.Add("container_no", "container_no/箱号");
            fieldsName.Add("container_size", "container_size/柜型");
            fieldsName.Add("tixiangdian", "提空箱（外高桥WGQ/洋山YS）");
            fieldsName.Add("jingangdian", "进港码头（外高桥WGQ/洋山YS）");
            fieldsName.Add("portsurtcase", "提箱-进港");
            fieldsName.Add("quantity", "quantity/数量");
            fieldsName.Add("charge_item", "charge_item/费用名称");
            fieldsName.Add("charge_code", "charge_code/费用代码");
            fieldsName.Add("unit_price", "unit_price/单价");
            fieldsName.Add("invoice_amount", "invoice_amount/总价");
            fieldsName.Add("no_vat_amount", "no_vat amount/不含税价");
            fieldsName.Add("currency", "currency/币种");
            fieldsName.Add("vat_rate", "vat rate/税率");
            fieldsName.Add("fapiao_type", "Fapiao Type/发票类型(增值税专用发票，增值税普票等)");
            fieldsName.Add("booking_damco_pic", "booking_Damco_PIC/丹马士订舱人/操作人");
            fieldsName.Add("remark", "Remark备注");

            fieldsName.Add("fcr", "FCR/HBL/提单收据");
            fieldsName.Add("application_form", "Application_form的编号");
            fieldsName.Add("oce", "oce/scm");
            fieldsName.Add("warhouse_pack_fee", "仓库装箱收入CFS");
            fieldsName.Add("truck_pack_fee", "运输收入CFS");
            fieldsName.Add("warhouse_daidian_fee", "仓库代垫收入CFS");
            fieldsName.Add("truck_daidian_fee", "运输代垫收入CFS");
            fieldsName.Add("warhouse_unload_fee", "卸货费CFS");
            fieldsName.Add("demurrageCharge_fee", "超期堆存费"); 
            fieldsName.Add("other_fee", "修改改挂收入LX");
            fieldsName.Add("yard_fee", "堆场收入");
            fieldsName.Add("yardZhongKong_fee", "堆场中控收入");
            fieldsName.Add("truck_pack_outSourcing_fee", "仓库运输收入(外包)");
            fieldsName.Add("agent_fee", "货代收入");
            fieldsName.Add("difference", "差额");
            fieldsName.Add("locationId", "库位");
            fieldsName.Add("customerName", "客服");

            return Content(EIP.EipListJson(list, total, fieldsName, "LoadId:60,default:110"));
        }

        public ActionResult showFeeDetailAddList1()
        {
            WCF.RootService.BillDetailSearch entity = new WCF.RootService.BillDetailSearch();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.WhCode = Session["whCode"].ToString();
            entity.BLNumber = Request["BLNumber"];

            entity.LoadId = Request["txt_detail_loadId"];
            entity.ContainerNumber = Request["txt_detail_containerNumber"];
            entity.SoNumber = Request["txt_detail_soNumber"].Trim();

            if (Request["txt_detail_ETD_begin"] != "")
            {
                entity.BeginETD = Convert.ToDateTime(Request["txt_detail_ETD_begin"]);
            }
            else
            {
                entity.BeginETD = null;
            }
            if (Request["txt_detail_ETD_end"] != "")
            {
                entity.EndETD = Convert.ToDateTime(Request["txt_detail_ETD_end"]).AddDays(1);
            }
            else
            {
                entity.EndETD = null;
            }

            List<string> chargeName = new List<string>();
            chargeName.Add("装箱费");
            chargeName.Add("进港费");

            List<string> clientCodeNotIn = new List<string>();
            clientCodeNotIn.Add("DAMCO");
            clientCodeNotIn.Add("HM");

            entity.ReportAgentType = "Damco";
            entity.ReportType = "特费";
            entity.OceScmType = "SCM";
            entity.NotLoadFlag = 0;

            int total = 0;
            List<WCF.RootService.BillDetailRepostResult> list = cf1.DamcoBillDetailList(entity, chargeName.ToArray(), null, clientCodeNotIn.ToArray(), out total).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();

            fieldsName.Add("LoadId", "操作");
            fieldsName.Add("origin", "origin/丹马士分公司");
            fieldsName.Add("invoice_name", "invoice_name");
            fieldsName.Add("invoice_no", "invoice_no/发票号");
            fieldsName.Add("due_date", "due_date/到期付款日");
            fieldsName.Add("consignee_name", "consignee_name/客人名称");
            fieldsName.Add("sending_date", "Sending_Date/发账单日期");
            fieldsName.Add("etd_show", "etd/开航日期");
            fieldsName.Add("booking_no", "Booking_NO/SO/订舱号");
            fieldsName.Add("cbl", "cbl/SWB/提单号/海运单号");
            fieldsName.Add("container_no", "container_no/箱号");
            fieldsName.Add("container_size", "container_size/柜型");
            fieldsName.Add("tixiangdian", "提空箱（外高桥WGQ/洋山YS）");
            fieldsName.Add("jingangdian", "进港码头（外高桥WGQ/洋山YS）");
            fieldsName.Add("portsurtcase", "提箱-进港");
            fieldsName.Add("quantity", "quantity/数量");
            fieldsName.Add("charge_item", "charge_item/费用名称");
            fieldsName.Add("charge_code", "charge_code/费用代码");
            fieldsName.Add("unit_price", "unit_price/含税单价");
            fieldsName.Add("invoice_amount", "invoice_amount/总价");
            fieldsName.Add("no_vat_amount", "no_vat amount/不含税价");
            fieldsName.Add("currency", "currency/币种");
            fieldsName.Add("vat_rate", "vat rate/税率");
            fieldsName.Add("fapiao_type", "Fapiao Type/发票类型(增值税专用发票，增值税普票等)");
            fieldsName.Add("booking_damco_pic", "booking_Damco_PIC/丹马士订舱人/操作人");
            fieldsName.Add("remark", "Remark备注");

            fieldsName.Add("fcr", "FCR/HBL/提单收据");
            fieldsName.Add("application_form", "Application_form的编号");
            fieldsName.Add("oce", "oce/scm");
            fieldsName.Add("warhouse_pack_fee", "仓库装箱收入CFS");
            fieldsName.Add("truck_pack_fee", "运输收入CFS");
            fieldsName.Add("warhouse_daidian_fee", "仓库代垫收入CFS");
            fieldsName.Add("truck_daidian_fee", "运输代垫收入CFS");
            fieldsName.Add("warhouse_unload_fee", "卸货费CFS");
            fieldsName.Add("demurrageCharge_fee", "超期堆存费");
            fieldsName.Add("other_fee", "修改改挂收入LX");
            fieldsName.Add("yard_fee", "堆场收入");
            fieldsName.Add("yardZhongKong_fee", "堆场中控收入");
            fieldsName.Add("truck_pack_outSourcing_fee", "仓库运输收入(外包)");
            fieldsName.Add("agent_fee", "货代收入");
            fieldsName.Add("difference", "差额");
            fieldsName.Add("locationId", "库位");
            fieldsName.Add("customerName", "客服");

            return Content(EIP.EipListJson(list, total, fieldsName, "LoadId:60,default:110"));
        }

        public ActionResult showFeeDetailAddList2()
        {
            WCF.RootService.BillDetailSearch entity = new WCF.RootService.BillDetailSearch();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.WhCode = Session["whCode"].ToString();
            entity.BLNumber = Request["BLNumber"];

            entity.LoadId = Request["txt_detail_loadId"];
            entity.ContainerNumber = Request["txt_detail_containerNumber"];
            entity.SoNumber = Request["txt_detail_soNumber"].Trim();

            if (Request["txt_detail_ETD_begin"] != "")
            {
                entity.BeginETD = Convert.ToDateTime(Request["txt_detail_ETD_begin"]);
            }
            else
            {
                entity.BeginETD = null;
            }
            if (Request["txt_detail_ETD_end"] != "")
            {
                entity.EndETD = Convert.ToDateTime(Request["txt_detail_ETD_end"]).AddDays(1);
            }
            else
            {
                entity.EndETD = null;
            }

            entity.ReportAgentType = "Damco";
            entity.ReportType = "特费";
            entity.OceScmType = "";
            entity.NotLoadFlag = 1;

            int total = 0;
            List<WCF.RootService.BillDetailRepostResult> list = cf1.DamcoBillDetailSOList(entity, null, null, null, out total).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();

            fieldsName.Add("Id", "操作1");
            fieldsName.Add("origin", "origin/丹马士分公司");
            fieldsName.Add("invoice_name", "invoice_name");
            fieldsName.Add("invoice_no", "invoice_no/发票号");
            fieldsName.Add("due_date", "due_date/到期付款日");
            fieldsName.Add("consignee_name", "consignee_name/客人名称");
            fieldsName.Add("sending_date", "Sending_Date/发账单日期");
            fieldsName.Add("etd_show", "etd/开航日期");
            fieldsName.Add("booking_no", "Booking_NO/SO/订舱号");
            fieldsName.Add("cbl", "cbl/SWB/提单号/海运单号");
            fieldsName.Add("container_no", "container_no/箱号");
            fieldsName.Add("container_size", "container_size/柜型");
            fieldsName.Add("tixiangdian", "提空箱（外高桥WGQ/洋山YS）");
            fieldsName.Add("jingangdian", "进港码头（外高桥WGQ/洋山YS）");
            fieldsName.Add("portsurtcase", "提箱-进港");
            fieldsName.Add("quantity", "quantity/数量");
            fieldsName.Add("charge_item", "charge_item/费用名称");
            fieldsName.Add("charge_code", "charge_code/费用代码");
            fieldsName.Add("unit_price", "unit_price/含税单价");
            fieldsName.Add("invoice_amount", "invoice_amount/总价");
            fieldsName.Add("no_vat_amount", "no_vat amount/不含税价");
            fieldsName.Add("currency", "currency/币种");
            fieldsName.Add("vat_rate", "vat rate/税率");
            fieldsName.Add("fapiao_type", "Fapiao Type/发票类型(增值税专用发票，增值税普票等)");
            fieldsName.Add("booking_damco_pic", "booking_Damco_PIC/丹马士订舱人/操作人");
            fieldsName.Add("remark", "Remark备注");

            fieldsName.Add("fcr", "FCR/HBL/提单收据");
            fieldsName.Add("application_form", "Application_form的编号");
            fieldsName.Add("oce", "oce/scm");
            fieldsName.Add("warhouse_pack_fee", "仓库装箱收入CFS");
            fieldsName.Add("truck_pack_fee", "运输收入CFS");
            fieldsName.Add("warhouse_daidian_fee", "仓库代垫收入CFS");
            fieldsName.Add("truck_daidian_fee", "运输代垫收入CFS");
            fieldsName.Add("warhouse_unload_fee", "卸货费CFS");
            fieldsName.Add("other_fee", "修改改挂收入");
            fieldsName.Add("yard_fee", "堆场收入");
            fieldsName.Add("difference", "差额");
            fieldsName.Add("locationId", "库位");
            fieldsName.Add("customerName", "客服");

            return Content(EIP.EipListJson(list, total, fieldsName, "Id:40,default:110"));
        }

        [HttpGet]
        public ActionResult BillDetailDel()
        {
            string result = cf1.BillDetailDelByLoad(Session["whCode"].ToString(), Request["BLNumber"], Session["userName"].ToString(), Request["LoadId"]);
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "删除成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }
        }

        [HttpGet]
        public ActionResult BillMasterEdit()
        {
            string result = cf1.BillMasterEdit(Session["whCode"].ToString(), Request["BLNumber"], "C");
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "确认成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }
        }

        [HttpGet]
        public ActionResult BillFeeDetailDelAll()
        {
            string result = cf1.BillFeeDetailDelAll(Session["whCode"].ToString(), Request["BLNumber"]);
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "删除成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }
        }


        //查询SO特费列表
        public ActionResult GetLoadChargeSOList()
        {
            WCF.OutBoundService.LoadChargeDetailSearch entity = new WCF.OutBoundService.LoadChargeDetailSearch();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.WhCode = Session["whCode"].ToString();
            entity.SoNumber = Request["txt_sea_SoNumber"];
            entity.BillNumber = Request["txt_sea_BillNumber"];
            entity.ContainerNumber = Request["txt_sea_ContainerNumber"];
            entity.ClientCode = Request["txt_sea_clientCode"];
            entity.SoStatus = "正常";

            if (Request["txt_sea_ETD_begin"] != "")
            {
                entity.BeginETD = Convert.ToDateTime(Request["txt_sea_ETD_begin"]);
            }
            else
            {
                entity.BeginETD = null;
            }
            if (Request["txt_sea_ETD_end"] != "")
            {
                entity.EndETD = Convert.ToDateTime(Request["txt_sea_ETD_end"]).AddDays(1);
            }
            else
            {
                entity.EndETD = null;
            }

            string so_area = Request["txt_sea_SoNumber_area"];
            string[] soList = null;
            if (!string.IsNullOrEmpty(so_area))
            {
                string so_temp = so_area.Replace("\r\n", "@");    //把so中的空格 替换为@符号
                soList = so_temp.Split('@');           //把SO 按照@分割，放在数组
            }

            int total = 0;
            List<WCF.OutBoundService.LoadChargeDetailResult> list = outboundcf.LoadChargeDetailSOList(entity, soList, out total).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Id", "操作");
            fieldsName.Add("ClientCode", "客户名");
            fieldsName.Add("ETDShow", "ETD时间");
            fieldsName.Add("SoNumber", "SO号");
            fieldsName.Add("UnitName", "提单号");
            fieldsName.Add("ContainerNumber", "箱号");
            fieldsName.Add("ContainerType", "箱型");
            fieldsName.Add("ChargeName", "科目");
            fieldsName.Add("CBM", "数量");
            fieldsName.Add("Price", "单价");
            fieldsName.Add("PriceTotal", "总价");
            fieldsName.Add("ChargeUnitName", "操作者");
            fieldsName.Add("LadderNumber", "FCR");
            fieldsName.Add("SoStatus", "状态");
            fieldsName.Add("Remark", "客服备注");

            return Content(EIP.EipListJson(list, total, fieldsName, "Id:45,ClientCode:130,ETD:80,SoNumber:130,CBM:45,Price:65,PriceTotal:65,Remark:150,default:90"));
        }


        [HttpPost]
        public ActionResult BillDetailAddSO()
        {
            string so_area = Request["id"];
            string[] loadList = null;
            if (!string.IsNullOrEmpty(so_area))
            {
                string so_temp = so_area.Replace(",", "@");    //把so中的空格 替换为@符号
                loadList = so_temp.Split('@');           //把SO 按照@分割，放在数组
            }

            string result = cf1.BillDetailAddSO(Session["whCode"].ToString(), Request["BLNumber"], Session["userName"].ToString(), Array.ConvertAll(loadList, int.Parse));

            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "添加成功！", result, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }
        }

        [HttpGet]
        public ActionResult DelBillDetailSO()
        {
            string result = cf1.BillDetailDelBySO(Session["whCode"].ToString(), Request["BLNumber"], Session["userName"].ToString(), Convert.ToInt32(Request["id"]));
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

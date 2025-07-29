
using System.Web.Mvc;
using WMS.EIP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Collections;
using System.Web;
using System.IO;

namespace WMS.UI.Controllers
{
    public class ForecastOrderController : Controller
    {
        WCF.InBoundService.InBoundServiceClient cf = new WCF.InBoundService.InBoundServiceClient();
        Helper Helper = new Helper();
        EIP.EIP EIP = new EIP.EIP();

        [DefaultRequest]
        public ActionResult Index()
        {

            return View();
        }



        //预录入查询明细列表
        [HttpGet]
        public ActionResult List()
        {
            WCF.InBoundService.ExcelImportInBoundSearch entity = new WCF.InBoundService.ExcelImportInBoundSearch();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.SoNumber = Request["SoNumber"].ToString();
            entity.PoNumber = Request["PoNumber"].Trim();
            entity.WhCode = Session["whCode"].ToString();
            entity.SystemNumber = Request["SystemNumber"].ToString();

            if (!string.IsNullOrEmpty(Request["BeginDate"]))
            {
                entity.BeginDate = Convert.ToDateTime(Request["BeginDate"]);
            }
            if (!string.IsNullOrEmpty(Request["EndDate"]))
            {
                entity.EndDate = Convert.ToDateTime(Request["EndDate"]);
            }

            if (!string.IsNullOrEmpty(Request["BeginConsDate"]))
            {
                entity.BeginConsDate = Convert.ToDateTime(Request["BeginConsDate"]);
            }
            if (!string.IsNullOrEmpty(Request["EndConsDate"]))
            {
                entity.EndConsDate = Convert.ToDateTime(Request["EndConsDate"]);
            }

            entity.ItemNumber = Request["ItemNumber"].Trim();
            entity.Supplier = Request["Supplier"].Trim();

            entity.Labeling = Request["Labeling"].Trim();
            entity.PlatHeavyCargo = Request["PlatHeavyCargo"].Trim();

            int total = 0;
            string str = "";
            List<WCF.InBoundService.ExcelImportInBound> list = cf.ExcelImportInBoundList(entity, out total, out str).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Id", "操作");
            fieldsName.Add("SystemNumber", "系统编号");
            fieldsName.Add("LabelingSend", "是否寄出");
            fieldsName.Add("ExpressNumber", "快递单号");
            fieldsName.Add("DoesTheFactoryConfirm", "[是否确认/是否贴好]");
            fieldsName.Add("Consignee", "Consignee收货人");
            fieldsName.Add("IncoTerm", "Inco term国际贸易术语");
            fieldsName.Add("OriginService", "Origin service起运港服务");
            fieldsName.Add("Supplier", "Supplier发货人/供应商");
            fieldsName.Add("SoNumber", "Booking no.进仓编号");
            fieldsName.Add("PoNumber", "PO number订单号");
            fieldsName.Add("KeycodeRatioPack", "Style number/Keycode款号");
            fieldsName.Add("RatioPack", "Ratio Pack # 比率包装号");
            fieldsName.Add("ItemNumber", "Keycode/Ratio Pack #款号");

            fieldsName.Add("DepartmentNo", "Department no 部门号");
            fieldsName.Add("SubBrand", "SUB BRAND 子品牌");
            fieldsName.Add("PlaceOfReceipt", "Place Of Receipt 起运港");
            fieldsName.Add("PlaceOfDelivery", "Place Of Delivery目的港");
            fieldsName.Add("BookedCarton", "Booked Carton 箱数");
            fieldsName.Add("BookedWeight", "Booked Weight重量");
            fieldsName.Add("BookedCBM", "Booked CBM体积");
            fieldsName.Add("Qty", "Booked Quantity 件数");
            fieldsName.Add("Height", "Height (CM) 高");
            fieldsName.Add("Width", "Width (CM)宽");
            fieldsName.Add("Length", "Length/Depth (CM) 长");

            fieldsName.Add("DistributionChannel", "Distribution Channel 分拨渠道");
            fieldsName.Add("ADDate", "AD date货物可供零售时间");
            fieldsName.Add("PriorityFlag", "Priority Flag 优先标志");
            fieldsName.Add("ConsDate", "Cons date 送货截止");
            fieldsName.Add("DCDD", "DCDD交货截止");
            fieldsName.Add("OverseasDecantFlag", "overseas decant Flag 海外预分拣标志");
            fieldsName.Add("Mixed", "[Mixed?混]");
            fieldsName.Add("PackingClusterGroup", "Packing Cluster Group 货物大类");
            fieldsName.Add("QC", "QC 质检");
            fieldsName.Add("Labeling", "Labeling 标签");
            fieldsName.Add("Remark", "Remark 备注");
            fieldsName.Add("PalletizationForHeavyCargo", "Palletization for Heavy Cargo(重货打托)");

            fieldsName.Add("CreateUser", "创建人");
            fieldsName.Add("CreateDate", "创建时间");
            fieldsName.Add("UpdateUser", "修改人");
            fieldsName.Add("UpdateDate", "修改时间");

            return Content(EIP.EipListJson(list, total, fieldsName, "Id:60,SoNumber:110,PoNumber:110,ItemNumber:110,default:150", null, "", 200, str));
        }


        public void import()
        {

            #region 1.选择Excel文件并验证

            //文件名
            string oldName = Request.Files["UploadFile"].FileName;
            string fileName = oldName.Substring(oldName.LastIndexOf('\\') + 1);
            string result = "";
            //上传的文件大小
            if (Request.Files[0].ContentLength > 40 * 1024 * 1024)
            {

                result = "文件大小不能超过40M！";
                Response.Write(result);
                return;
                //return Helper.RedirectAjax("err", result, null, "");
            }

            string Path = @"d:\file\" + fileName;
            Directory.CreateDirectory(@"d:\file\");

            HttpRequest request = System.Web.HttpContext.Current.Request;
            HttpFileCollection FileCollect = request.Files;

            for (int i = 0; i < FileCollect.Count; i++)
            {
                if (Directory.Exists(Path))
                {
                    Directory.Delete(Path);
                }
                FileCollect[i].SaveAs(Path);
            }

            //得到Excel的所有数据
            //DataTable dataTable = sourceTable(Path);
            NPOIExcelHelper helper = new NPOIExcelHelper();
            DataTable dataTable = helper.ExcelToDataTable(Path, null, true);    //取得Excel第一个文档的数据

            if (dataTable == null)
            {
                result = "Excel存在异常，请打开Excel后另存为一个新文件后再导入！";
                //return Helper.RedirectAjax("err", result, null, "");
            }
            if (result != "")
            {
                Response.Write(result);
                return;
                //return Helper.RedirectAjax("err", result, null, "");
            }
            #endregion

            #region 2.验证Excel列是否存在并符合要求

            //取得Excel列名
            List<string> tbList = new List<string>();
            for (int i = 0; i < dataTable.Columns.Count; i++)
            {
                tbList.Add(dataTable.Columns[i].ColumnName);
            }


            List<WCF.InBoundService.ExcelImportInBoundSo> entityList = new List<WCF.InBoundService.ExcelImportInBoundSo>();
            List<WCF.InBoundService.ExcelImportInBound> entityForecastList = new List<WCF.InBoundService.ExcelImportInBound>();

            List<string> soList = new List<string>();
            Hashtable data = new Hashtable();   //去excel重复的SO、PO、款号
            int k = 0; string errorItemNumber = ""; //插入失败的款号
            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                //判断进仓编号是否为空
                if (string.IsNullOrEmpty(dataTable.Rows[i]["Booking no.进仓编号"].ToString().Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace("'", "")))
                {
                    break;
                }
                if (string.IsNullOrEmpty(dataTable.Rows[i]["PO number订单号"].ToString().Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace("'", "")))
                {
                    break;
                }
                if (string.IsNullOrEmpty(dataTable.Rows[i]["Booked Carton 箱数"].ToString().Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace("'", "")))
                {
                    break;
                }
                //
                if (!data.ContainsValue(dataTable.Rows[i]["Booking no.进仓编号"].ToString().Trim() + "-" + dataTable.Rows[i]["PO number订单号"].ToString().Trim() + "-" + dataTable.Rows[i]["Keycode/Ratio Pack #款号"].ToString() + "-" + dataTable.Rows[i]["Keycode/Ratio Pack #款号"].ToString().Trim())) //Ecxel是否存在重复的值 不存在 add 
                {
                    data.Add(k, dataTable.Rows[i]["Booking no.进仓编号"].ToString().Trim() + "-" + dataTable.Rows[i]["PO number订单号"].ToString().Trim() + "-" + dataTable.Rows[i]["Keycode/Ratio Pack #款号"].ToString() + "-" + dataTable.Rows[i]["Keycode/Ratio Pack #款号"].ToString().Trim());
                    k++;
                }
                else
                {
                    errorItemNumber = "数据:" + dataTable.Rows[i]["Booking no.进仓编号"].ToString().Trim() + "-" + dataTable.Rows[i]["PO number订单号"].ToString().Trim() + "-" + dataTable.Rows[i]["Keycode/Ratio Pack #款号"].ToString() + "-" + dataTable.Rows[i]["Keycode/Ratio Pack #款号"].ToString().Trim();
                }
                if (soList.Contains(dataTable.Rows[i]["Booking no.进仓编号"].ToString()) == false)
                {
                    WCF.InBoundService.ExcelImportInBoundSo enity = new WCF.InBoundService.ExcelImportInBoundSo();
                    enity.WhCode = Session["whCode"].ToString();
                    enity.ClientCode = "Kmart";
                    enity.SoNumber = dataTable.Rows[i]["Booking no.进仓编号"].ToString();
                    entityList.Add(enity);

                    soList.Add(dataTable.Rows[i]["Booking no.进仓编号"].ToString());
                }
                //Booked Carton 箱数 必须为整数
                try
                {
                    int ss = Convert.ToInt32(dataTable.Rows[i]["Booked Carton 箱数"].ToString().Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace("'", ""));
                }
                catch (Exception)
                {
                    result = "格式有误必须为数字！Booked Carton 箱数:" + dataTable.Rows[i]["Booked Carton 箱数"].ToString().Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace("'", "");
                }

                //Booked CBM体积 必须为数值
                try
                {
                    decimal ss = Convert.ToDecimal(dataTable.Rows[i]["Booked Weight重量"].ToString().Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace("'", ""));
                }
                catch (Exception)
                {
                    result = "格式有误必须为数值！Booked Weight重量:" + dataTable.Rows[i]["Booked Weight重量"].ToString().Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace("'", "");
                }
                //Booked Quantity 件数 必须为数值
                try
                {
                    int ss = Convert.ToInt32(dataTable.Rows[i]["Booked Quantity 件数"].ToString().Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace("'", ""));
                }
                catch (Exception)
                {
                    result = "格式有误必须为数字！Booked Quantity 件数:" + dataTable.Rows[i]["Booked Quantity 件数"].ToString().Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace("'", "");
                }

                ////height(CM)高
                //try
                //{
                //    decimal ss = Convert.ToDecimal(dataTable.Rows[i]["Booked CBM体积"].ToString().Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace("'", ""));
                //}
                //catch (Exception)
                //{
                //    result = "格式有误必须为数字！Booked CBM体积:" + dataTable.Rows[i]["Booked CBM体积"].ToString().Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace("'", "");
                //}
                ////height(CM)高
                //try
                //{
                //    decimal ss = Convert.ToDecimal(dataTable.Rows[i]["Height (CM) 高"].ToString().Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace("'", ""));
                //}
                //catch (Exception)
                //{
                //    result = "格式有误必须为数字！Height (CM)高:" + dataTable.Rows[i]["Height (CM) 高"].ToString().Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace("'", "");
                //}
                ////Width(CM)宽
                //try
                //{
                //    decimal ss = Convert.ToDecimal(dataTable.Rows[i]["Width (CM)宽"].ToString().Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace("'", ""));
                //}
                //catch (Exception)
                //{
                //    result = "格式有误必须为数字！Width(CM)宽:" + dataTable.Rows[i]["Width (CM)宽"].ToString().Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace("'", "");
                //}
                //try
                //{
                //    decimal ss = Convert.ToDecimal(dataTable.Rows[i]["Length/Depth (CM) 长"].ToString().Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace("'", ""));
                //}
                //catch (Exception)
                //{
                //    result = "格式有误必须为数字！Length/Depth (CM) 长:" + dataTable.Rows[i]["Length/Depth (CM) 长"].ToString().Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace("'", "");
                //}

            }

            if (errorItemNumber != "")
            {
                Response.Write("导入数据重复！" + errorItemNumber);
                return;
            }
            #endregion

            if (result != "")
            {
                Response.Write(result);
                return;
            }

            #region 3.预录入

            List<WCF.InBoundService.InBoundOrderInsert> entityInBoundList = new List<WCF.InBoundService.InBoundOrderInsert>();

            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                if (string.IsNullOrEmpty(dataTable.Rows[i]["Booking no.进仓编号"].ToString().Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace("'", "")))
                {
                    break;
                }

                #region 构建foreCast导入实体

                WCF.InBoundService.ExcelImportInBound entityForecast = new WCF.InBoundService.ExcelImportInBound();
                entityForecast.WhCode = Session["whCode"].ToString();
                entityForecast.Consignee = dataTable.Rows[i]["Consignee 收货人"].ToString();
                entityForecast.IncoTerm = dataTable.Rows[i]["Inco term 国际贸易术语"].ToString();
                entityForecast.OriginService = dataTable.Rows[i]["Origin service 起运港服务"].ToString();
                entityForecast.Supplier = dataTable.Rows[i]["Supplier 发货人;供应商"].ToString();
                entityForecast.SoNumber = dataTable.Rows[i]["Booking no.进仓编号"].ToString().Trim().Replace(" ", "").Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "");
                entityForecast.PoNumber = dataTable.Rows[i]["PO number订单号"].ToString().Trim().Replace(" ", "").Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "");
                entityForecast.ItemNumber = dataTable.Rows[i]["Keycode/Ratio Pack #款号"].ToString().Trim().Replace(" ", "").Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "");
                entityForecast.RatioPack = dataTable.Rows[i]["Ratio Pack # 比率包装号"].ToString().Trim().Replace(" ", "").Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "");
                entityForecast.KeycodeRatioPack = dataTable.Rows[i]["Style number / Keycode 款号"].ToString().Trim().Replace(" ", "").Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "");
                entityForecast.DepartmentNo = dataTable.Rows[i]["Department no 部门号"].ToString();
                entityForecast.SubBrand = dataTable.Rows[i]["SUB BRAND 子品牌"].ToString();
                entityForecast.PlaceOfReceipt = dataTable.Rows[i]["Place Of Receipt 起运港"].ToString();
                entityForecast.PlaceOfDelivery = dataTable.Rows[i]["Place Of Delivery目的港"].ToString();
                entityForecast.BookedCarton = Convert.ToInt32(dataTable.Rows[i]["Booked Carton 箱数"].ToString());
                entityForecast.BookedWeight = dataTable.Rows[i]["Booked Weight重量"].ToString();
                entityForecast.BookedCBM = Convert.ToDecimal(dataTable.Rows[i]["Booked CBM体积"]);
                entityForecast.Qty = Convert.ToInt32(dataTable.Rows[i]["Booked Quantity 件数"]);
                entityForecast.Height = Convert.ToDecimal(dataTable.Rows[i]["Height (CM) 高"]);
                entityForecast.Width = Convert.ToDecimal(dataTable.Rows[i]["Width (CM)宽"]);
                entityForecast.Length = Convert.ToDecimal(dataTable.Rows[i]["Length/Depth (CM) 长"]);
                entityForecast.DistributionChannel = dataTable.Rows[i]["Distribution Channel 分拨渠道"].ToString();
                entityForecast.ADDate = dataTable.Rows[i]["AD date"].ToString();
                entityForecast.PriorityFlag = dataTable.Rows[i]["Priority Flag 优先标志"].ToString();

                entityForecast.ConsDate = ExcelStringToDatetime(dataTable.Rows[i]["Cons date 送货截止"]);
                entityForecast.DCDD = ExcelStringToDatetime(dataTable.Rows[i]["DCDD"]);

                entityForecast.OverseasDecantFlag = dataTable.Rows[i]["overseas decant Flag 海外预分拣标志"].ToString();
                entityForecast.Mixed = dataTable.Rows[i]["Mixed混"].ToString();
                entityForecast.PackingClusterGroup = dataTable.Rows[i]["Packing Cluster Group 货物大类"].ToString();
                entityForecast.QC = dataTable.Rows[i]["QC 质检"].ToString();
                entityForecast.Labeling = dataTable.Rows[i]["Labeling 标签"].ToString();
                entityForecast.Remark = dataTable.Rows[i]["Remark 备注"].ToString();
                entityForecast.PalletizationForHeavyCargo = dataTable.Rows[i]["Palletization for Heavy Cargo(重货打托)"].ToString().Trim().Replace(" ", "").Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "");

                entityForecast.CreateDate = DateTime.Now;
                entityForecast.CreateUser = Session["userName"].ToString();
                entityForecastList.Add(entityForecast);

                #endregion

                #region 构建预录入实体

                //if (entityInBoundList.Where(u => u.SoNumber == entityForecast.SoNumber && u.ClientCode == "Kmart").Count() == 0)
                //{
                //    WCF.InBoundService.InBoundOrderInsert entity = new WCF.InBoundService.InBoundOrderInsert();
                //    entity.WhCode = Session["whCode"].ToString();
                //    entity.ClientCode = "Kmart";
                //    entity.SoNumber = dataTable.Rows[i]["Booking no.进仓编号"].ToString().Trim().Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "");
                //    entity.OrderType = "CFS";

                //    if (entity.WhCode == "10")
                //    {
                //        entity.ProcessId = 61;
                //        entity.ProcessName = "收货立方验证流程";
                //    }
                //    else
                //    {
                //        entity.ProcessId = 120;
                //        entity.ProcessName = "收货立方验证流程";
                //    }

                //    List<WCF.InBoundService.InBoundOrderDetailInsert> orderDetailList = new List<WCF.InBoundService.InBoundOrderDetailInsert>();

                //    WCF.InBoundService.InBoundOrderDetailInsert orderDetail = new WCF.InBoundService.InBoundOrderDetailInsert();
                //    orderDetail.JsonId = i;
                //    orderDetail.CustomerPoNumber = dataTable.Rows[i]["PO number订单号"].ToString().Trim().Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "");
                //    orderDetail.AltItemNumber = dataTable.Rows[i]["Keycode/Ratio Pack #款号"].ToString().Trim().ToString().Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "");
                //    orderDetail.Style1 = dataTable.Rows[i]["Palletization for Heavy Cargo(重货打托)"].ToString().Trim().Replace(" ", "").Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "");
                //    orderDetail.Style2 = dataTable.Rows[i]["Labeling 标签"].ToString().Trim().Replace(" ", "").Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "");

                //    orderDetail.Style3 = dataTable.Rows[i]["Place Of Delivery目的港"].ToString().Trim().Replace(" ", "").Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "");

                //    if (string.IsNullOrEmpty(dataTable.Rows[i]["Booked Weight重量"].ToString()) == true)
                //    {
                //        orderDetail.Weight = 0;
                //    }
                //    else
                //    {
                //        orderDetail.Weight = Convert.ToDecimal((dataTable.Rows[i]["Booked Weight重量"].ToString().Trim().Replace(" ", "").Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "") ?? "0"));
                //    }
                //    if (string.IsNullOrEmpty(dataTable.Rows[i]["Booked CBM体积"].ToString()) == true)
                //    {
                //        orderDetail.CBM = 0;
                //    }
                //    else
                //    {
                //        orderDetail.CBM = Convert.ToDecimal(dataTable.Rows[i]["Booked CBM体积"].ToString().Trim().Replace(" ", "").Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", ""));
                //    }
                //    orderDetail.Qty = Convert.ToInt32(dataTable.Rows[i]["Booked Carton 箱数"].ToString().Trim().Replace(" ", "").Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", ""));
                //    orderDetail.CreateUser = Session["userName"].ToString();
                //    orderDetailList.Add(orderDetail);

                //    entity.InBoundOrderDetailInsert = orderDetailList.ToArray();
                //    entityInBoundList.Add(entity);
                //}
                //else
                //{
                //    WCF.InBoundService.InBoundOrderInsert oldentity = entityInBoundList.Where(u => u.SoNumber == entityForecast.SoNumber && u.ClientCode == "Kmart").First();
                //    entityInBoundList.Remove(oldentity);

                //    WCF.InBoundService.InBoundOrderInsert newentity = oldentity;

                //    List<WCF.InBoundService.InBoundOrderDetailInsert> orderDetailList = oldentity.InBoundOrderDetailInsert.ToList();
                //    //
                //    if (orderDetailList.Where(u => u.CustomerPoNumber == dataTable.Rows[i]["PO number订单号"].ToString().Trim().Replace(" ", "").Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "") && u.AltItemNumber == dataTable.Rows[i]["Keycode/Ratio Pack #款号"].ToString().Trim().Replace(" ", "").Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "") && u.Style1 == dataTable.Rows[i]["Palletization for Heavy Cargo(重货打托)"].ToString().Trim().Replace(" ", "").Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "")).Count() == 0)
                //    {
                //        WCF.InBoundService.InBoundOrderDetailInsert orderDetail = new WCF.InBoundService.InBoundOrderDetailInsert();
                //        orderDetail.JsonId = i;

                //        orderDetail.CustomerPoNumber = dataTable.Rows[i]["PO number订单号"].ToString().Trim().Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "");

                //        orderDetail.AltItemNumber = dataTable.Rows[i]["Keycode/Ratio Pack #款号"].ToString().Trim().ToString().Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "");

                //        orderDetail.Style1 = dataTable.Rows[i]["Palletization for Heavy Cargo(重货打托)"].ToString().Trim().Replace(" ", "").Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "");

                //        orderDetail.Style2 = dataTable.Rows[i]["Labeling 标签"].ToString().Trim().Replace(" ", "").Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "");

                //        orderDetail.Style3 = dataTable.Rows[i]["Place Of Delivery目的港"].ToString().Trim().Replace(" ", "").Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "");

                //        if (string.IsNullOrEmpty(dataTable.Rows[i]["Booked Weight重量"].ToString()) == true)
                //        {
                //            orderDetail.Weight = 0;
                //        }
                //        else
                //        {
                //            orderDetail.Weight = Convert.ToDecimal((dataTable.Rows[i]["Booked Weight重量"].ToString().Trim().Replace(" ", "").Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "") ?? "0"));
                //        }
                //        if (string.IsNullOrEmpty(dataTable.Rows[i]["Booked CBM体积"].ToString()) == true)
                //        {
                //            orderDetail.CBM = 0;
                //        }
                //        else
                //        {
                //            orderDetail.CBM = Convert.ToDecimal(dataTable.Rows[i]["Booked CBM体积"].ToString().Trim().Replace(" ", "").Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", ""));
                //        }
                //        orderDetail.Qty = Convert.ToInt32(dataTable.Rows[i]["Booked Carton 箱数"].ToString().Trim().Replace(" ", "").Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", ""));
                //        orderDetail.CreateUser = Session["userName"].ToString();
                //        orderDetailList.Add(orderDetail);
                //    }
                //    else
                //    {
                //        WCF.InBoundService.InBoundOrderDetailInsert oldorderDetail = orderDetailList.Where(u => u.CustomerPoNumber == dataTable.Rows[i]["PO number订单号"].ToString().Trim().Replace(" ", "").Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "") && u.AltItemNumber == dataTable.Rows[i]["Keycode/Ratio Pack #款号"].ToString().Trim().Replace(" ", "").Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "") && u.Style1 == dataTable.Rows[i]["Palletization for Heavy Cargo(重货打托)"].ToString().Trim().Replace(" ", "").Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "")).First();
                //        orderDetailList.Remove(oldorderDetail);
                //        WCF.InBoundService.InBoundOrderDetailInsert neworderDetail = oldorderDetail;
                //        neworderDetail.Qty = oldorderDetail.Qty + Convert.ToInt32(dataTable.Rows[i]["Booked Quantity 件数"].ToString().Trim().Replace(" ", "").Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", ""));
                //        orderDetailList.Add(neworderDetail);
                //    }

                //    newentity.InBoundOrderDetailInsert = orderDetailList.ToArray();
                //    entityInBoundList.Add(newentity);

                //}
                #endregion
            }

            string aa = cf.importForecast(entityForecastList.ToArray());
            //string resultInbound = cf.ImportsInBoundOrder(entityInBoundList.ToArray());

            string[] value = aa.Split('$');

            if (value[0] == "Y")
            {
                //最后再删除文件
                //Directory.Delete(@"d:\file\",true);

                Response.Write("Y$" + "导入成功！系统编号为：" + value[1] + "导入Carton总数:" + value[2]);
                return;
            }
            else
            {
                Response.Write(aa);
                return;
            }
        }


        #endregion


        //修改Forecast信息
        [HttpGet]
        public ActionResult Edit()
        {
            WCF.InBoundService.ExcelImportInBound entity = new WCF.InBoundService.ExcelImportInBound();
            entity.Id = Convert.ToInt32(Request["Id"]);
            entity.LabelingSend = Request["edit_LabelingSend"];
            entity.ExpressNumber = Request["edit_ExpressNumber"];
            entity.DoesTheFactoryConfirm = Request["edit_DoesTheFactoryConfirm"];
            entity.UpdateUser = Session["userName"].ToString();
            entity.UpdateDate = DateTime.Now;
            string result = cf.ExcelImportInBoundEdit(entity);
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "信息修改成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }
        }


        //删除SO
        [HttpGet]
        public ActionResult DeleteSO()
        {
            string whCode = Session["whCode"].ToString();
            string clientCode = "Kmart";
            string soNumber = Request["soNumber"];

            string result = cf.ExcelImportInBoundDelete(whCode, clientCode, soNumber);
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "删除成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }
        }


        public DateTime ExcelStringToDatetime(object objectvalue)
        {
            string[] value = objectvalue.ToString().Split('-');

            string year = "", month = "", day = "";
            day = value[0].ToString();
            if (value[1].ToString() == "1月")
            {
                month = "1";
            }
            else if (value[1].ToString() == "2月")
            {
                month = "2";
            }
            else if (value[1].ToString() == "3月")
            {
                month = "3";
            }
            else if (value[1].ToString() == "4月")
            {
                month = "4";
            }
            else if (value[1].ToString() == "5月")
            {
                month = "5";
            }
            else if (value[1].ToString() == "6月")
            {
                month = "6";
            }
            else if (value[1].ToString() == "7月")
            {
                month = "7";
            }
            else if (value[1].ToString() == "8月")
            {
                month = "8";
            }
            else if (value[1].ToString() == "9月")
            {
                month = "9";
            }
            else if (value[1].ToString() == "10月")
            {
                month = "10";
            }
            else if (value[1].ToString() == "11月")
            {
                month = "11";
            }
            else if (value[1].ToString() == "12月")
            {
                month = "12";
            }
            year = value[2].ToString();

            return Convert.ToDateTime(year + "-" + month + "-" + day);
        }




        [DefaultRequest]
        public ActionResult CottonIndex()
        {

            return View();
        }

        //预录入查询明细列表
        [HttpGet]
        public ActionResult CottonList()
        {
            WCF.InBoundService.ExcelImportInBoundSearch entity = new WCF.InBoundService.ExcelImportInBoundSearch();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.SoNumber = Request["SoNumber"].ToString();
            entity.PoNumber = Request["PoNumber"].Trim();
            entity.ItemNumber = Request["ItemNumber"].Trim();
            entity.WhCode = Session["whCode"].ToString();
            entity.SystemNumber = Request["SystemNumber"].ToString();

            entity.ASOS = Request["ASOS"].ToString();
            entity.ClothesHanger = Request["ClothesHanger"].ToString();
            entity.Hudson = Request["Hudson"].ToString();
            entity.ShipeeziSO = Request["ShipeeziSO"].ToString();

            if (!string.IsNullOrEmpty(Request["BeginDate"]))
            {
                entity.BeginDate = Convert.ToDateTime(Request["BeginDate"]);
            }
            if (!string.IsNullOrEmpty(Request["EndDate"]))
            {
                entity.EndDate = Convert.ToDateTime(Request["EndDate"]);
            }

            int total = 0;
            string str = "";
            List<WCF.InBoundService.ExcelImportInBoundCotton> list = cf.CottonExcelImportInBoundList(entity, out total, out str).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Id", "操作");
            fieldsName.Add("SystemNumber", "系统编号");

            fieldsName.Add("ShipeeziSO", "ShipeeziSO");
            fieldsName.Add("SoNumber", "SO");
            fieldsName.Add("PoNumber", "PoNumber");
            fieldsName.Add("ItemNumber", "SKU");
            fieldsName.Add("ColourCode", "ColourCode");
            fieldsName.Add("Qty", "CTN");

            fieldsName.Add("DC", "DC");
            fieldsName.Add("BookingDate", "BookingDate");
            fieldsName.Add("Consignee", "Consignee");
            fieldsName.Add("Shipper", "Shipper");

            fieldsName.Add("Brand", "Brand");
            fieldsName.Add("ERD", "ERD");
            fieldsName.Add("COGETD", "COGETD");
            fieldsName.Add("COGETA", "COGETA");
            fieldsName.Add("Weight", "Weight");
            fieldsName.Add("CBM", "CBM");
            fieldsName.Add("BookedQTY", "BookedQTY");
            fieldsName.Add("LOGITQTY", "LOGITQTY");
            fieldsName.Add("Size", "Size");
            fieldsName.Add("PlaceOfReceipt", "PlaceOfReceipt");
            fieldsName.Add("BookedPOR", "BookedPOR");
            fieldsName.Add("BookedPortOfDischarge", "BookedPortOfDischarge");
            fieldsName.Add("ASOS", "[ASOS / HUDSON BAY / MACY’S / BR]");
            fieldsName.Add("ClothesHanger", "[含衣架 (F/H)]");
            fieldsName.Add("Hudson", "[Hudson Bay 吊牌 (Y/N)]");

            fieldsName.Add("CreateUser", "创建人");
            fieldsName.Add("CreateDate", "创建时间");

            return Content(EIP.EipListJson(list, total, fieldsName, "Id:60,SystemNumber:143,default:130", null, "", 200, str));
        }


        public void CottonImport()
        {

            #region 1.选择Excel文件并验证

            //文件名
            string oldName = Request.Files["UploadFile"].FileName;
            string fileName = oldName.Substring(oldName.LastIndexOf('\\') + 1);
            string result = "";
            //上传的文件大小
            if (Request.Files[0].ContentLength > 40 * 1024 * 1024)
            {

                result = "文件大小不能超过40M！";
                Response.Write(result);
                return;
            }

            string Path = @"d:\file\" + fileName;
            Directory.CreateDirectory(@"d:\file\");

            HttpRequest request = System.Web.HttpContext.Current.Request;
            HttpFileCollection FileCollect = request.Files;

            for (int i = 0; i < FileCollect.Count; i++)
            {
                if (Directory.Exists(Path))
                {
                    Directory.Delete(Path);
                }
                FileCollect[i].SaveAs(Path);
            }

            //得到Excel的所有数据
            NPOIExcelHelper helper = new NPOIExcelHelper();
            DataTable dataTable = helper.ExcelToDataTable(Path, null, true);    //取得Excel第一个文档的数据

            if (dataTable == null)
            {
                result = "Excel存在异常，请检查列是否包含中文列、重复列、是否是第一页等！";
            }
            if (result != "")
            {
                Response.Write(result);
                return;
            }
            #endregion

            #region 2.验证Excel列是否存在并符合要求

            //取得Excel列名
            List<string> tbList = new List<string>();
            for (int i = 0; i < dataTable.Columns.Count; i++)
            {
                tbList.Add(dataTable.Columns[i].ColumnName);
            }


            List<WCF.InBoundService.ExcelImportInBoundCotton> entityForecastList = new List<WCF.InBoundService.ExcelImportInBoundCotton>();

            Hashtable data = new Hashtable();   //去excel重复的SO、PO、款号
            int k = 0; string errorItemNumber = ""; //插入失败的款号
            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                //判断进仓编号是否为空
                if (string.IsNullOrEmpty(dataTable.Rows[i]["Shipeezi SO"].ToString().Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace("'", "")))
                {
                    break;
                }
                if (string.IsNullOrEmpty(dataTable.Rows[i]["PO"].ToString().Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace("'", "")))
                {
                    break;
                }
                if (string.IsNullOrEmpty(dataTable.Rows[i]["CTN"].ToString().Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace("'", "")))
                {
                    break;
                }

                //CTN 必须为整数
                try
                {
                    int ss = Convert.ToInt32(dataTable.Rows[i]["CTN"].ToString().Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace("'", ""));
                }
                catch (Exception)
                {
                    result = "格式有误必须为数字！CTN:" + dataTable.Rows[i]["CTN"].ToString().Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace("'", "");
                }
            }

            #endregion

            if (result != "")
            {
                Response.Write(result);
                return;
            }

            #region 3.预录入

            List<WCF.InBoundService.InBoundOrderInsert> entityInBoundList = new List<WCF.InBoundService.InBoundOrderInsert>();

            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                if (string.IsNullOrEmpty(dataTable.Rows[i]["Shipeezi SO"].ToString().Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace("'", "")))
                {
                    break;
                }

                #region 构建foreCast导入实体

                WCF.InBoundService.ExcelImportInBoundCotton entityForecast = new WCF.InBoundService.ExcelImportInBoundCotton();
                entityForecast.WhCode = Session["whCode"].ToString();
                entityForecast.ClientCode = "Cotton_on";
                entityForecast.DC = dataTable.Rows[i]["DC"].ToString();
                entityForecast.PO = dataTable.Rows[i]["PO"].ToString();
                entityForecast.BookingDate = dataTable.Rows[i]["booking date"].ToString();
                entityForecast.Consignee = dataTable.Rows[i]["Consignee"].ToString();
                entityForecast.Shipper = dataTable.Rows[i]["Shipper"].ToString();
                entityForecast.ShipeeziSO = dataTable.Rows[i]["Shipeezi SO"].ToString();

                entityForecast.SoNumber = dataTable.Rows[i]["SO"].ToString().Trim().Replace(" ", "").Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "");
                entityForecast.PoNumber = dataTable.Rows[i]["PO"].ToString().Trim().Replace(" ", "").Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "");
                entityForecast.ItemNumber = dataTable.Rows[i]["ItemCode"].ToString().Trim().Replace(" ", "").Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "");
                entityForecast.Brand = dataTable.Rows[i]["Brand/O5-DivisionDesc"].ToString().Trim().Replace(" ", "").Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "");

                entityForecast.ERD = dataTable.Rows[i]["ERD"].ToString().Trim().Replace(" ", "").Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "");
                entityForecast.COGETD = dataTable.Rows[i]["COGETD"].ToString();
                entityForecast.COGETA = dataTable.Rows[i]["COGETA"].ToString();
                entityForecast.Qty = Convert.ToInt32(dataTable.Rows[i]["CTN"]);

                entityForecast.Weight = Convert.ToDecimal(dataTable.Rows[i]["Weight"]);
                entityForecast.CBM = Convert.ToDecimal(dataTable.Rows[i]["CBM"]);
                entityForecast.BookedQTY = Convert.ToInt32(dataTable.Rows[i]["BookedQTY"]);
                entityForecast.LOGITQTY = Convert.ToInt32(dataTable.Rows[i]["LOGITQTY"].ToString());
                entityForecast.ColourCode = dataTable.Rows[i]["ColourCode"].ToString().Trim().Replace(" ", "").Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "");
                entityForecast.Size = dataTable.Rows[i]["Size"].ToString().Trim().Replace(" ", "").Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "");

                entityForecast.PlaceOfReceipt = dataTable.Rows[i]["PlaceOfReceipt"].ToString();
                entityForecast.BookedPOR = dataTable.Rows[i]["BookedPOR"].ToString();
                entityForecast.BookedPortOfDischarge = dataTable.Rows[i]["BookedPortOfDischarge"].ToString();
                entityForecast.ASOS = dataTable.Rows[i]["ASOS / HUDSON BAY / MACY’S / BR"].ToString();
                entityForecast.ClothesHanger = dataTable.Rows[i]["含衣架 (F/H)"].ToString();
                entityForecast.Hudson = dataTable.Rows[i]["Hudson Bay 吊牌 (Y/N)"].ToString();

                entityForecast.CreateDate = DateTime.Now;
                entityForecast.CreateUser = Session["userName"].ToString();
                entityForecastList.Add(entityForecast);

                #endregion


            }

            #endregion

            string aa = cf.ExcelImportForecastCotton(entityForecastList.ToArray());

            string[] value = aa.Split('$');

            if (value[0] == "Y")
            {
                Response.Write("Y$" + "导入成功！系统编号为：" + value[1] + "导入Carton总数:" + value[2]);
                return;
            }
            else
            {
                Response.Write(aa);
                return;
            }
        }

        //修改Forecast信息
        [HttpGet]
        public ActionResult EditCottonDetail()
        {
            WCF.InBoundService.ExcelImportInBoundCotton entity = new WCF.InBoundService.ExcelImportInBoundCotton();
            entity.Id = Convert.ToInt32(Request["edit_id"]);
            entity.Qty = Convert.ToInt32(Request["edit_qty"]);
            entity.CreateUser = Session["userName"].ToString();

            string result = cf.EditForecastCotton(entity);
            if (result == "Y")
            {
                return Helper.RedirectAjax("ok", "信息修改成功！", null, "");
            }
            else
            {
                return Helper.RedirectAjax("err", result, null, "");
            }
        }





        [DefaultRequest]
        public ActionResult MosaicIndex()
        {

            return View();
        }

        //预录入查询明细列表
        [HttpGet]
        public ActionResult MosaicList()
        {
            WCF.InBoundService.ExcelImportInBoundSearch entity = new WCF.InBoundService.ExcelImportInBoundSearch();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.SoNumber = Request["SoNumber"].ToString();
            entity.PoNumber = Request["PoNumber"].Trim();
            entity.ItemNumber = Request["ItemNumber"].Trim();
            entity.WhCode = Session["whCode"].ToString();
            entity.SystemNumber = Request["SystemNumber"].ToString();
            entity.RatioorBulk = Request["RatioorBulk"].ToString();

            if (!string.IsNullOrEmpty(Request["BeginDate"]))
            {
                entity.BeginDate = Convert.ToDateTime(Request["BeginDate"]);
            }
            if (!string.IsNullOrEmpty(Request["EndDate"]))
            {
                entity.EndDate = Convert.ToDateTime(Request["EndDate"]);
            }

            if (!string.IsNullOrEmpty(Request["BeginConsDate"]))
            {
                entity.BeginConsDate = Convert.ToDateTime(Request["BeginConsDate"]);
            }
            if (!string.IsNullOrEmpty(Request["EndConsDate"]))
            {
                entity.EndConsDate = Convert.ToDateTime(Request["EndConsDate"]);
            }

            int total = 0;
            string str = "";
            List<WCF.InBoundService.ExcelImportInBoundCom> list = cf.MosaicExcelImportInBoundList(entity, out total, out str).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("Id", "操作");
            fieldsName.Add("SystemNumber", "系统编号");
            fieldsName.Add("SoNumber", "SO");
            fieldsName.Add("PoNumber", "PO");
            fieldsName.Add("ItemNumber", "SKU");
            fieldsName.Add("Style1", "属性1");
            fieldsName.Add("Qty", "数量");
            fieldsName.Add("Remark2", "Ratio or Bulk");
            fieldsName.Add("Remark3", "Brand");
            fieldsName.Add("Weight", "Booked Weight");
            fieldsName.Add("CBM", "Booked Volume");
            fieldsName.Add("CreateUser", "创建人");
            fieldsName.Add("CreateDate", "创建时间");

            return Content(EIP.EipListJson(list, total, fieldsName, "Id:60,Qty:50,CreateUser:60,ItemNumber:300,SystemNumber:143,Remark1:130,default:130", null, "", 200, str));
        }


        public void MosaicImport()
        {

            #region 1.选择Excel文件并验证

            //文件名
            string oldName = Request.Files["UploadFile"].FileName;
            string fileName = oldName.Substring(oldName.LastIndexOf('\\') + 1);
            string result = "";
            //上传的文件大小
            if (Request.Files[0].ContentLength > 40 * 1024 * 1024)
            {
                result = "文件大小不能超过40M！";
                Response.Write(result);
                return;
            }

            string Path = @"d:\file\" + fileName;
            Directory.CreateDirectory(@"d:\file\");

            HttpRequest request = System.Web.HttpContext.Current.Request;
            HttpFileCollection FileCollect = request.Files;

            for (int i = 0; i < FileCollect.Count; i++)
            {
                if (Directory.Exists(Path))
                {
                    Directory.Delete(Path);
                }
                FileCollect[i].SaveAs(Path);
            }

            //得到Excel的所有数据
            NPOIExcelHelper helper = new NPOIExcelHelper();
            DataTable dataTable = helper.ExcelToDataTable(Path, null, true);    //取得Excel第一个文档的数据

            if (dataTable == null)
            {
                result = "Excel存在异常，请检查列是否包含中文列、重复列、是否是第一页等！";
            }
            if (result != "")
            {
                Response.Write(result);
                return;
            }
            #endregion

            #region 2.验证Excel列是否存在并符合要求

            List<WCF.InBoundService.ExcelImportInBoundCom> entityForecastList = new List<WCF.InBoundService.ExcelImportInBoundCom>();

            Hashtable data = new Hashtable();   //去excel重复的SO、PO、款号

            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                //判断进仓编号是否为空
                if (string.IsNullOrEmpty(dataTable.Rows[i]["Shipper Booking Number"].ToString().Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace("'", "")))
                {
                    break;
                }
                if (string.IsNullOrEmpty(dataTable.Rows[i]["PO"].ToString().Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace("'", "")))
                {
                    break;
                }
                if (string.IsNullOrEmpty(dataTable.Rows[i]["Booked Carton"].ToString().Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace("'", "")))
                {
                    break;
                }

                //CTN 必须为整数
                try
                {
                    int ss = Convert.ToInt32(dataTable.Rows[i]["Booked Carton"].ToString().Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace("'", ""));
                }
                catch (Exception)
                {
                    result = "格式有误必须为数字！Booked Carton:" + dataTable.Rows[i]["Booked Carton"].ToString().Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace("'", "");
                }
            }

            #endregion

            if (result != "")
            {
                Response.Write(result);
                return;
            }

            #region 3.预录入

            List<WCF.InBoundService.InBoundOrderInsert> entityInBoundList = new List<WCF.InBoundService.InBoundOrderInsert>();

            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                if (string.IsNullOrEmpty(dataTable.Rows[i]["Shipper Booking Number"].ToString().Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace("'", "")))
                {
                    break;
                }

                if (string.IsNullOrEmpty(dataTable.Rows[i]["Booked Carton"].ToString().Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace("'", "")))
                {
                    continue;
                }

                #region 构建foreCast导入实体

                WCF.InBoundService.ExcelImportInBoundCom entityForecast = new WCF.InBoundService.ExcelImportInBoundCom();
                entityForecast.WhCode = Session["whCode"].ToString();
                entityForecast.ClientCode = "Mosaic";

                entityForecast.SoNumber = dataTable.Rows[i]["Shipper Booking Number"].ToString().Trim().Replace(" ", "").Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "");
                entityForecast.PoNumber = dataTable.Rows[i]["PO"].ToString().Trim().Replace(" ", "").Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "");
                entityForecast.ItemNumber = dataTable.Rows[i]["SKU"].ToString().Trim().Replace(" ", "").Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "").Replace(@".", "_");

                if (entityForecast.ItemNumber.Length > 48)
                {
                    entityForecast.ItemNumber = entityForecast.ItemNumber.Substring(0, 48);
                }
                entityForecast.Style1 = dataTable.Rows[i]["Place Of Delivery"].ToString().Trim().Replace(" ", "").Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "");

                if (!string.IsNullOrEmpty(dataTable.Rows[i]["FFD (Expected receipt date at origin)"].ToString().Trim().Replace(" ", "").Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "")))
                {
                    string ss = dataTable.Rows[i]["FFD (Expected receipt date at origin)"].ToString().Trim().Replace(" ", "").Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "").Replace("11月", "11").Replace("12月", "12").Replace("1月", "01").Replace("2月", "02").Replace("3月", "03").Replace("4月", "04").Replace("5月", "05").Replace("6月", "06").Replace("7月", "07").Replace("8月", "08").Replace("9月", "09").Replace("10月", "10");

                    string[] numberList = null;
                    if (!string.IsNullOrEmpty(ss))
                    {
                        numberList = ss.Split('-');           //按照-分割，放在数组
                    }

                    entityForecast.Remark1 = Convert.ToDateTime(numberList[2] + "/" + numberList[1] + "/" + numberList[0]);
                }

                entityForecast.Remark2 = dataTable.Rows[i]["Ratio or Bulk"].ToString();
                entityForecast.Remark3 = dataTable.Rows[i]["Brand"].ToString();

                entityForecast.Qty = Convert.ToInt32(dataTable.Rows[i]["Booked Carton"]);
                entityForecast.Weight = Convert.ToDecimal(dataTable.Rows[i]["Booked Weight"]);
                entityForecast.CBM = Convert.ToDecimal(dataTable.Rows[i]["Booked Volume"]);

                entityForecast.CreateDate = DateTime.Now;
                entityForecast.CreateUser = Session["userName"].ToString();
                entityForecastList.Add(entityForecast);

                #endregion

            }

            #endregion

            string result1 = cf.ExcelImportForecastCommon(entityForecastList.ToArray());

            string[] value = result1.Split('$');

            if (value[0] == "Y")
            {
                Response.Write("Y$" + "导入成功！系统编号为：" + value[1] + "导入总数:" + value[2]);
                return;
            }
            else
            {
                Response.Write(result1);
                return;
            }
        }


    }
}

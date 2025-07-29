using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WMS.EIP;

namespace WMS.UI.Controllers
{
    public class ForecastOutBoundController : Controller
    {
        WCF.InBoundService.InBoundServiceClient cf1 = new WCF.InBoundService.InBoundServiceClient();
        WCF.OutBoundService.OutBoundServiceClient cf = new WCF.OutBoundService.OutBoundServiceClient();

        Helper Helper = new Helper();
        EIP.EIP EIP = new EIP.EIP();
        [DefaultRequest]
        //页面显示 （通用）
        public ActionResult Index()
        {
            ViewData["WhClientList"] = from r in cf1.WhClientListSelect(Session["whCode"].ToString())
                                       select new SelectListItem()
                                       {
                                           Text = r.ClientCode,     //text
                                           Value = r.Id.ToString()
                                       };

            return View();
        }

        [HttpGet]
        public ActionResult List()
        {
            WCF.OutBoundService.ExcelImportOutBoundSearch entity = new WCF.OutBoundService.ExcelImportOutBoundSearch();
            entity.WhCode = Session["whCode"].ToString();
            entity.pageIndex = Convert.ToInt32(Request["eip_page"]);
            entity.pageSize = Convert.ToInt32(Request["eip_page_size"]);
            entity.SoNumber = Request["SoNumber"];
            entity.PoNumber = Request["PoNumber"];
            entity.ItemNumber = Request["ItemNumber"];
            entity.ContainerNumber = Request["ContainerNumber"];

            string str = "";
            int total = 0;
            List<WCF.OutBoundService.ExcelImportOutBound> list = cf.CottonExcelImportOutBoundList(entity, out total, out str).ToList();

            Dictionary<string, string> fieldsName = new Dictionary<string, string>();
            fieldsName.Add("ContainerNumber", "箱号");
            fieldsName.Add("ClientCode", "客户名");
            fieldsName.Add("SoNumber", "SO");
            fieldsName.Add("PoNumber", "PO");
            fieldsName.Add("ItemNumber", "SKU");
            fieldsName.Add("Style1", "属性1");
            fieldsName.Add("Qty", "Qty");

            return Content(EIP.EipListJson(list, total, fieldsName, "default:140", null, "", 200, str));
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

            ////取得Excel列名
            //List<string> tbList = new List<string>();
            //for (int i = 0; i < dataTable.Columns.Count; i++)
            //{
            //    tbList.Add(dataTable.Columns[i].ColumnName);
            //}

            List<WCF.InBoundService.ExcelImportInBoundCotton> entityForecastList = new List<WCF.InBoundService.ExcelImportInBoundCotton>();

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

            #region 3.数据处理

            List<WCF.OutBoundService.ExcelImportOutBound> entityBoundList = new List<WCF.OutBoundService.ExcelImportOutBound>();

            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                if (string.IsNullOrEmpty(dataTable.Rows[i]["Shipeezi SO"].ToString().Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace("'", "")))
                {
                    break;
                }

                if (Convert.ToInt32(dataTable.Rows[i]["CTN"].ToString().Trim().Replace(" ", "").Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "")) == 0)
                {
                    continue;
                }

                string so = dataTable.Rows[i]["Shipeezi SO"].ToString().Trim().Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "");
                string po = dataTable.Rows[i]["PO"].ToString().Trim().Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "");
                string item = dataTable.Rows[i]["SKU"].ToString().Trim().Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "");
                string style1 = dataTable.Rows[i]["SKU"].ToString().Trim().Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "");
                string container = dataTable.Rows[i]["CONTAINER "].ToString().Trim().Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", "");

                string sku = item.Substring(0, item.IndexOf("-"));
                string getlen = item.Substring(sku.Length + 1, item.Length - sku.Length - 1);

                style1 = getlen.Substring(0, getlen.IndexOf("-"));

                if (entityBoundList.Where(u => u.SoNumber == so && u.PoNumber == po && u.ItemNumber == sku && u.ContainerNumber == container && u.Style1 == style1).Count() == 0)
                {
                    WCF.OutBoundService.ExcelImportOutBound entity = new WCF.OutBoundService.ExcelImportOutBound();
                    entity.WhCode = Session["whCode"].ToString();
                    entity.ClientCode = "Cotton_on";
                    entity.ContainerNumber = container;
                    entity.SoNumber = so;
                    entity.PoNumber = po;
                    entity.ItemNumber = sku;
                    entity.Style1 = style1;

                    entity.Qty = Convert.ToInt32(dataTable.Rows[i]["CTN"].ToString().Trim().Replace(" ", "").Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", ""));

                    entityBoundList.Add(entity);
                }
                else
                {
                    WCF.OutBoundService.ExcelImportOutBound oldentity = entityBoundList.Where(u => u.SoNumber == so && u.PoNumber == po && u.ItemNumber == sku && u.ContainerNumber == container && u.Style1 == style1).First();
                    entityBoundList.Remove(oldentity);

                    WCF.OutBoundService.ExcelImportOutBound newentity = oldentity;
                    newentity.Qty = oldentity.Qty + Convert.ToInt32(dataTable.Rows[i]["CTN"].ToString().Trim().Replace(" ", "").Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace(@"""", "").Replace(@"'", ""));
                    entityBoundList.Add(newentity);
                }

            }

            #endregion

            string aa = cf.ExcelImportOutBoundCotton(entityBoundList.ToArray());

            string[] value = aa.Split('$');

            if (value[0] == "Y")
            {
                Response.Write("导入成功！");
                return;
            }
            else
            {
                Response.Write(aa);
                return;
            }
        }


    }
}

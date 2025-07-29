using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using WMS.Reports.Services.Helper;
using WMS.Reports.Services.Models;

namespace WMS.Reports.Services.Controllers
{
    public class SqlExportController : ApiController
    {
        NPOIExcelHelper NPOIExcelHelper = new NPOIExcelHelper();
        OpenDataHelper OpenDataHelper = new OpenDataHelper();

        [HttpGet, HttpPost]
        //public IHttpActionResult SqlToJson(ReportRequestDataModel requestDataModel)
        public void ExportExecl(ReportRequestDataModel requestDataModel)
        {
          //ReportRequestDataModel requestDataModel = SetModel();
            DataTable dt = OpenDataHelper.SqlToDataTable(requestDataModel.SqlName, requestDataModel.Sql, requestDataModel.SqlParameter, requestDataModel.ExecDbName, requestDataModel.OrderByPara, requestDataModel.ExecType);
            using (MemoryStream cc = NPOIExcelHelper.DataTableToExcel(dt))
            {
                HttpContext.Current.Response.Expires = 0;
                HttpContext.Current.Response.Clear();
                HttpContext.Current.Response.Buffer = true;
                HttpContext.Current.Response.Charset = "utf-8";
                HttpContext.Current.Response.ContentEncoding = System.Text.Encoding.UTF8;
                HttpContext.Current.Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                HttpContext.Current.Response.BinaryWrite(cc.ToArray());
                HttpContext.Current.Response.End();
            }
            dt.Dispose();
        }
        private ReportRequestDataModel SetModel()
        {

            ReportRequestDataModel requestDataModel = new ReportRequestDataModel();
            requestDataModel.SqlName = "托盘查询";
            requestDataModel.ExecDbName = "10#88#88#97.WMS";
            requestDataModel.Page = 1;
            requestDataModel.Size = 0;
            Hashtable SqlParameters = new Hashtable();
            SqlParameters.Add("@WhCode", "01");
            SqlParameters.Add("@HuId", "");
            requestDataModel.SqlParameter = SqlParameters;
            Dictionary<string, string> OrderByPara = new Dictionary<string, string>();
            OrderByPara.Add("托盘号", " asc");
            OrderByPara.Add("ReceiptDate", " desc");
            requestDataModel.OrderByPara = OrderByPara;

            return requestDataModel;

        }
    }
}

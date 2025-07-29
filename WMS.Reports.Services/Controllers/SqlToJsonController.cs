using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http;
using WMS.Reports.Services.Helper;
using WMS.Reports.Services.Models;

namespace WMS.Reports.Services.Controllers
{
    public class SqlToJsonController : ApiController
    {
        OpenDataHelper OpenDataHelper = new OpenDataHelper();
        [HttpGet,HttpPost]
        //public IHttpActionResult SqlToJson(ReportRequestDataModel requestDataModel)
        public HttpResponseMessage SqlToJson(ReportRequestDataModel requestDataModel)
        {


            //HttpContext.Current.Response.AddHeader("Content-Type", "application/json");
            //  ReportRequestDataModel requestDataModel = SetModel();
            //   return (JsonConvert.SerializeObject(requestDataModel, Formatting.None, new IsoDateTimeConverter { DateTimeFormat = "yyyy-MM-dd HH:mm:ss" }));


            // OpenDataHelper openDataHelper = new OpenDataHelper();
            if (requestDataModel != null)
                if (requestDataModel.ExecDbName != null)
                    if (requestDataModel.IfReturnSql)
                    {
                        string aa = OpenDataHelper.SqlReturn(requestDataModel.SqlName, requestDataModel.Sql, requestDataModel.SqlParameter, requestDataModel.ExecDbName, requestDataModel.Page, requestDataModel.Size, requestDataModel.OrderByPara, true, requestDataModel.SumPara, requestDataModel.ExecType);
                        return SetResponseMessage(aa);
                    }
                    else
                        return SetResponseMessage(OpenDataHelper.SqlToJson(requestDataModel.SqlName, requestDataModel.Sql, requestDataModel.SqlParameter, requestDataModel.ExecDbName, requestDataModel.Page, requestDataModel.Size, requestDataModel.OrderByPara, requestDataModel.SumPara, requestDataModel.ExecType));
                else
                    return SetResponseMessage(OpenDataHelper.StringToJson("SQL或DB名称异常"));
            else
                return SetResponseMessage(OpenDataHelper.StringToJson("没有数据可执行数据!"));
        }

        private HttpResponseMessage SetResponseMessage(string content)
        {
            return new HttpResponseMessage { Content = new StringContent(content, Encoding.GetEncoding("UTF-8"), "application/json") };
        }
        private ReportRequestDataModel SetModel() {

            ReportRequestDataModel requestDataModel = new ReportRequestDataModel();
            requestDataModel.SqlName = "托盘查询1";
           requestDataModel.ExecDbName = "10#88#88#97.WMS";
            requestDataModel.Page = 1;
            requestDataModel.Size = 30;
            Hashtable SqlParameters = new Hashtable();
            SqlParameters.Add("@WhCode", "01");
            SqlParameters.Add("@HuId", "");
            requestDataModel.SqlParameter = SqlParameters;
            Dictionary<string, string> OrderByPara = new Dictionary<string, string>();
            OrderByPara.Add("ReceiptId", " asc");
            OrderByPara.Add("CreateDate", " desc");
            requestDataModel.OrderByPara = OrderByPara;

            return requestDataModel;

        }
    }
}

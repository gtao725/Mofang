using MODEL_MSSQL;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WMS.BLL;
using WMS.BLLClass;
using WMS.IBLL;
using WMS.WebApi.Common;

namespace WMS.WebApi.Controllers
{
    public class RecOuterController : ApiController
    {
        Helper Helper = new Helper();
        [HttpGet]
        public object CheckRecLocation([FromUri]string WhCode, [FromUri]string Location)
        {
            IRecOuterManager aa = new RecOuterManager();
            if (aa.CheckRecLocation(WhCode, Location))
                return Helper.ResultData("Y", "", new { });
            else
                return Helper.ResultData("N", "收货区不存在!", new { });
        }

        //获取收货数据
        [HttpGet, HttpPost]
        public object GetReceiptData(ReceiptParam receiptParam)
        {
            IRecOuterManager recOuterManager = new RecOuterManager();
            var ReceiptData = recOuterManager.GetReceiptData(receiptParam);
            return Helper.ResultData("Y", "", ReceiptData);
        }

    }
}


using MODEL_MSSQL;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using WMS.BLL;
using WMS.BLLClass;
using WMS.IBLL;
using WMS.WebApi.Common;
using WMS.WebApi.Models;

namespace WMS.WebApi.Controllers
{
    public class TransferController : ApiController
    {
        Helper Helper = new Helper();

        [HttpGet] 
        public object GetTransferDetailCe([FromUri]string WhCode, [FromUri]string TransferNumber, [FromUri]int pageSize, [FromUri]int pageIndex)
        {

            ITransferTaskWinceManager aa = new TransferTaskWinceManager();
            int count = 0;
            List<TransferDetailCe> TransferDetailList = aa.GetTransferDetailCe(TransferNumber, WhCode, pageSize, pageIndex, out count);
            if (count != 0)
            {
                return Helper.ResultData("Y", count.ToString(), TransferDetailList);

            }else
                return Helper.ResultData("N", TransferNumber + "无交接明细!", new { });

        }

        [HttpGet]
        public object TransferTaskInsert([FromUri]string WhCode, [FromUri]string TransferNumber, [FromUri]string UserName, [FromUri]string ExpressNumber)
        {

            ITransferTaskWinceManager aa = new TransferTaskWinceManager();
            string res = aa.TransferTaskInsert(TransferNumber, UserName, ExpressNumber, WhCode);
            if (res.Substring(0, 1) == "Y")
            {
                int count = 0;
                List<TransferDetailCe> TransferDetailList = aa.GetTransferDetailCe(TransferNumber, WhCode,20,1, out count);
                return Helper.ResultData("Y", count.ToString(), TransferDetailList);
            }
            else
                return Helper.ResultData("N", res, new { });

        }

        [HttpGet]
        public object TransferTaskLoadInsert([FromUri]string WhCode, [FromUri]string TransferNumber, [FromUri]string UserName, [FromUri]string LoadId)
        {

            ITransferTaskWinceManager aa = new TransferTaskWinceManager();
            string res = aa.TransferTaskLoadInsert(TransferNumber, UserName, LoadId, WhCode);
            if (res.Substring(0, 1) == "Y")
            {
                int count = 0;
                List<TransferDetailCe> TransferDetailList = aa.GetTransferDetailCe(TransferNumber, WhCode, 20, 1, out count);
                return Helper.ResultData("Y", count.ToString(), TransferDetailList);
            }
            else
                return Helper.ResultData("N", res, new { });

        }
        [HttpGet]
        public object BeginTransferTask(  [FromUri]int transferId, [FromUri]string UserName, [FromUri] int fayunQty)
        {
 
            ITransferTaskWinceManager aa = new TransferTaskWinceManager();
            string res = aa.BeginTransferTask(transferId, UserName, fayunQty);
            if (res.Substring(0, 1) == "Y")
            {
                 return Helper.ResultData("Y", "", new { });
            }
            else
                return Helper.ResultData("N", res, new { });
        }

        [HttpGet]
        public object TransferTaskDelete([FromUri]int transferId)
        {

            ITransferTaskWinceManager aa = new TransferTaskWinceManager();
            string res = aa.TransferTaskDelete(transferId);
            if (res == "Y")
            {
                return Helper.ResultData("Y","", new { });
            }
            else
                return Helper.ResultData("N", res, new { });
        }
 
    }
}

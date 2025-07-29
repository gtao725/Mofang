
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
    public class SupplementController : ApiController
    {
        Helper Helper = new Helper();

        [HttpGet] 
        public object GetSupplementTask([FromUri]string WhCode, [FromUri]string TaskType)
        {

            IInventoryWinceManager aa = new InventoryWinceManager();
            string[] NotStatusArry = null;
            if (TaskType == "Down")
                NotStatusArry = new[] {"C","D"};
            else if (TaskType == "Up")
                NotStatusArry = new[] { "U","C"};
            List<SupplementTaskCe> supplementTaskList = aa.SupplementTaskCe(WhCode, NotStatusArry);
            if (supplementTaskList.Count()>0)
            {
                return Helper.ResultData("Y", "", supplementTaskList);

            }else
                return Helper.ResultData("N", "无补货任务!", new { });

        }

        [HttpGet]
        public object GetSupplementTaskDetail([FromUri]string SupplementNumber, [FromUri]string WhCode,[FromUri] string SupplementGroupNumber)
        {
            
            IInventoryWinceManager aa = new InventoryWinceManager();
            List<SupplementTaskDetailCe> supplementTaskDetailList = aa.SupplementTaskDetailCe(WhCode,SupplementNumber, SupplementGroupNumber);
            if (supplementTaskDetailList.Count() > 0)
            {
                return Helper.ResultData("Y", "", supplementTaskDetailList);

            }
            else
                return Helper.ResultData("N", "补货任务明细获取异常!", new { });

        }

        [HttpGet]
        public object  SupplementTaskDown([FromUri]string SupplementNumber, [FromUri]string SupplementGroupNumber, [FromUri] string HuId, [FromUri] string User, [FromUri]string WhCode)
        {

            IInventoryWinceManager aa = new InventoryWinceManager();
            string res= aa.SupplementTaskDown( SupplementNumber, SupplementGroupNumber,  HuId,  User,  WhCode);
            if (res=="Y")
            {
                return Helper.ResultData("Y", "", new { });
            }
            else
                return Helper.ResultData("N", res, new { });

        }
        [HttpGet]
        public object SupplementTaskUp([FromUri]string SupplementNumber, [FromUri]string SupplementGroupNumber, [FromUri] string PutLocationId, [FromUri] string User, [FromUri]string WhCode)
        {

            IInventoryWinceManager aa = new InventoryWinceManager();
            string res = aa.SupplementTaskUp(SupplementNumber, SupplementGroupNumber, PutLocationId, User, WhCode);
            if (res == "Y")
            {
                return Helper.ResultData("Y", "", new { });
            }
            else
                return Helper.ResultData("N", res, new { });

        }
    }
}

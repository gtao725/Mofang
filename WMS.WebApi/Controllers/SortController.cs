
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
    public class SortController : ApiController
    {
        Helper Helper = new Helper();

        [HttpGet] 
        public object CheckSortTask([FromUri]string WhCode, [FromUri]string LoadId)
        {

            ISortTaskWinceManager aa = new SortTaskWinceManager();
            List<SortTask> SortList = aa.GetSortTaskList(LoadId, WhCode);
            //分拣状态正确
            if (SortList.Count!=0)
            {
                IShipWinceManger res = new ShipWinceManger();
                //返回分拣明细和流程号
                return Helper.ResultData("Y", res.GetLoadProcessId(WhCode, LoadId).ToString(), aa.GetSortTaskDetailList(LoadId, WhCode));

            }else
                return Helper.ResultData("N", "出货计划:"+ LoadId + "不存在或已经分拣完成!", new { });

        }

        [HttpGet]
        public object SortTaskScanning([FromUri]string WhCode, [FromUri]string LoadId, [FromUri]string UserName, [FromUri]int ItemId, [FromUri]int Qty)
        {

            ISortTaskWinceManager aa = new SortTaskWinceManager();
            string res = aa.SortTaskScanning(LoadId, WhCode, UserName, ItemId.ToString(), Qty);
            //分拣状态正确
            if (res.Substring(0, 1) == "Y"|| res.Substring(0, 1) == "C")
            {
                return Helper.ResultData("Y", res, new { });
            }
            else
                return Helper.ResultData("N", res, new { });

        }

        [HttpGet]
        public object UpdateSortTaskGroupNumber([FromUri]string WhCode, [FromUri]string LoadId, [FromUri]string UserName, [FromUri]int GroupId, [FromUri]string GroupNumber)
        {

            ISortTaskWinceManager aa = new SortTaskWinceManager();
            string res = aa.UpdateSortTaskGroupNumber(LoadId, WhCode, GroupId, GroupNumber, UserName);
            if (res.Substring(0, 1) == "Y"|| res.Substring(0, 1) == "H")
            {
                if(res.Substring(0, 1) == "H")
                    return Helper.ResultData("Y", res.Substring(1, res.Length), new { });
                else
                    return Helper.ResultData("Y", "", new { });
            }
            else
                return Helper.ResultData("N", res, new { });

        }

        
    }
}


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
    public class PackController : ApiController
    {
        Helper Helper = new Helper();

        [HttpGet] 
        public object CheckPackSortGroupNumber([FromUri]string WhCode, [FromUri]string SortGroupNumber)
        {

            IPackWinceManager aa = new PackWinceManager();
            List<SortTaskDetailResult> STList = aa.CheckPackSortGroupNumber(SortGroupNumber, WhCode);
            if (STList!=null)
            {
                IShipWinceManger res = new ShipWinceManger();
                return Helper.ResultData("Y", res.GetLoadProcessId(WhCode, STList.First().LoadId).ToString(), STList);

            }else
                return Helper.ResultData("N", "包装框号:"+SortGroupNumber + "不存在!", new { });

        }

        [HttpGet]
        public object ChecPackNumber([FromUri]string WhCode, [FromUri] string LoadId,[FromUri]string SortGroupNumber, [FromUri]string PackNumber)
        {

            IPackWinceManager aa = new PackWinceManager();
            
            if (aa.ChecPackNumber(WhCode, LoadId, SortGroupNumber, PackNumber))
            {
                return Helper.ResultData("Y", "", new { });
            }
            else
                return Helper.ResultData("N", "包装号:" + PackNumber + " 重复!", new { });

        }

        // 托盘装箱完成
        [HttpGet,HttpPost]
        public object PackTaskInsert(PackTaskInsert packTaskInsert)
        {
            //string jsonstr = "{\"LoadId\":\"LD170726142710003\",\"WhCode\":\"03\",\"OutPoNumber\":null,\"GroupId\":1,\"GroupNumber\":\"TEST0001\",\"PackNumber\":\"111\",\"userName\":null,\"PackDetail\":[{\"ItemId\":78617,\"Qty\":2,\"EAN\":null,\"AltItemNumber\":\"5054411194587\",\"CreateDate\":null,\"PackScanNumber\":null},{\"ItemId\":78611,\"Qty\":1,\"EAN\":null,\"AltItemNumber\":\"5053251174933\",\"CreateDate\":null,\"PackScanNumber\":null}]}";
           // PackTaskInsert packTaskInsert = JsonConvert.DeserializeObject<PackTaskInsert>(jsonstr);


            if (packTaskInsert == null) {
                return Helper.ResultData("N", "数据异常!", new { });
            }
            IPackWinceManager aa = new PackWinceManager();
            string res = aa.PackTaskInsert(packTaskInsert);
            if (res.Substring(0, 1) == "Y")
                return Helper.ResultData("Y", res.Substring(1, res.Length-1), new { });
            else
                return Helper.ResultData("N", res, new { });
        }

   
        [HttpGet]
        public object UpdatePackHead([FromUri]int packHeadId, [FromUri]decimal? weight, [FromUri] string PackCartonNo, [FromUri] decimal? longth, [FromUri] decimal? width, [FromUri] decimal? height, [FromUri] string User)
        {
            IPackWinceManager aa = new PackWinceManager();
            if (aa.UpdatePackHead(packHeadId, weight, PackCartonNo, longth, width, height, User)=="Y")
                return Helper.ResultData("Y", "", new { });
            else
                return Helper.ResultData("N", "保存数据错误!", new { });
        }


    }
}

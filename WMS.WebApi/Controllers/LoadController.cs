
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
    public class LoadController : ApiController
    {
        Helper Helper = new Helper();

        [HttpGet, HttpPost]
        public object CheckLoad([FromUri]string WhCode, [FromUri]string LoadId)
        {

            ILoadWinceManger aa = new LoadWinceManger();
            if (aa.CheckLoad(WhCode, LoadId))
            {
                return Helper.ResultData("Y", "加载成功", aa.GetShipLoadDesModel(WhCode, LoadId));

            }else
                return Helper.ResultData("N", LoadId + " 不可装箱!", new { });
        }


        [HttpGet, HttpPost]
        public string test([FromUri]string WhCode)
        {


            return "http://localhost:9528/#/bigscreen/viewer?reportCode=IT_TEST";
        }

        [HttpGet]
        public object CheckPltScan([FromUri]string WhCode, [FromUri]string LoadId, [FromUri]string HuId)
        {

            ILoadWinceManger aa = new LoadWinceManger();
            if(aa.CheckPltScan(WhCode, LoadId, HuId))
                return Helper.ResultData("Y", "", new { });
            else
                return Helper.ResultData("N", "", new { });

        }
        [HttpGet, HttpPost]
        public object ToSerialNumberOut([FromUri]string WhCode, [FromUri]string LoadId)
        {

            ILoadWinceManger aa = new LoadWinceManger();
            string res = aa.ToSerialNumberOut(WhCode, LoadId);
            if (res=="Y")
            {
                return Helper.ResultData("Y", null, new { });
            }
            else
                return Helper.ResultData("N", res, new { });
        }
        [HttpGet, HttpPost]
        public object GetSerialNumber([FromUri]int HuDetailId)
        {

            ILoadWinceManger aa = new LoadWinceManger();
            List<string> res = aa.GetSerialNumber(HuDetailId);
            if (res.Count>0)
            {
                return Helper.ResultData("Y", null, res);
            }
            else
                return Helper.ResultData("N", "无可扫描的数据!", new { });
        }


        //获取建议托盘
        [HttpGet]
        public object LoadSugPlt([FromUri]string WhCode, [FromUri]string LoadId)
        {
            ILoadWinceManger aa = new LoadWinceManger();
            return Helper.ResultData("Y", aa.LoadSugPlt(WhCode, LoadId), new { });
 
        }
 

        //验证托盘是否符合装箱条件
        [HttpGet]
        public object CheckPltLoad( [FromUri]string WhCode, [FromUri]string LoadId, [FromUri]string HuId)
        {
            ILoadWinceManger aa = new LoadWinceManger();
            string res = aa.CheckPltLoad(WhCode, LoadId, HuId);
            if (res == "Y")
                return Helper.ResultData("Y", "", new { });
            else
                return Helper.ResultData("N", res, new { });
        }


        // 托盘装箱完成
        [HttpGet,HttpPost]
        public object LoadComplete(LoadPlt loadPlt)
        {
           // string jsonstr = "{\"LoadId\":\"LD170713092610001\",\"WhCode\":\"03\",\"HuId\":\"PLTA000001\",\"UserName\":\"1536\",\"WorkloadAccountModel\":[{\"WorkType\":\"装卸工\",\"UserCode\":\"1008\"}]}";
          //  LoadPlt loadPlt = JsonConvert.DeserializeObject<LoadPlt>(jsonstr);


            if (loadPlt == null) {
                return Helper.ResultData("N", "数据异常!", new { });
            }
            ILoadWinceManger aa = new LoadWinceManger();
            string res = aa.LoadComplete(loadPlt);
            if (res == "Y")
                return Helper.ResultData("Y", "", new { });
            else
                return Helper.ResultData("N", res, new { });
        }

        //验证是否完成装箱
        [HttpGet]
        public object CheckLoadIfComplete([FromUri]string WhCode, [FromUri]string LoadId)
        {
            ILoadWinceManger aa = new LoadWinceManger();
            
            if (aa.CheckLoadIfComplete(WhCode, LoadId))
                return Helper.ResultData("Y", "", new { });
            else
                return Helper.ResultData("N", "", new { });
        }


       
        [HttpGet]
        public object LoadIfComplete([FromUri]string WhCode, [FromUri]string LoadId, [FromUri]string item, [FromUri]string EAN)
        {
            IPackTaskManager aa = new PackTaskManager();
            string res = aa.DoTaskTest(LoadId, WhCode, item, EAN);
           // return Helper.ResultData("Y", res, new { });
            if ( res.Substring(0, 1) == "O")
                return Helper.ResultData("Y", "", new { });
            else
                return Helper.ResultData("N", res, new { });

 
          
        }


        [HttpGet]
        public object TransferTaskTest([FromUri]int transferId, [FromUri]int fayunQty)
        {
            ITransferTaskManager aa = new TransferTaskManager();
            string res = aa.BeginTransferTask(transferId, "1536", fayunQty);

            if (res == "Y")
                return Helper.ResultData("Y", "", new { });
            else
                return Helper.ResultData("N", res, new { });



        }


        [HttpGet]
        public object TaskLoadStaticClear([FromUri]string WhCode, [FromUri]string LoadId)
        {
            IPackTaskManager aa = new PackTaskManager();
            string res = aa.TaskLoadStaticClear(LoadId, WhCode);

            if (res == "Y")
                return Helper.ResultData("Y", LoadId + "缓存清除成功!", new { });
            else
                return Helper.ResultData("N", res, new { });



        }
 
    }
}

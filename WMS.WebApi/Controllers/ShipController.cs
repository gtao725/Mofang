using Newtonsoft.Json;
using System.Collections.Generic;
using System.Web.Http;
using WMS.BLL;
using WMS.BLLClass;
using WMS.IBLL;
using WMS.WebApi.Common;
using WMS.WebApi.Models;

namespace WMS.WebApi.Controllers
{
    public class ShipController : ApiController
    {
        Helper Helper = new Helper();
        #region wince ShipLoadIn 检测出货计划状态是否正确
        /// <summary>
        /// 检测出货计划状态是否正确
        /// </summary>
        /// <param name="LoadId">LoadId</param>
        /// <param name="WhCode">WhCode</param>
        /// <returns>正确的话返回Y和备货流程ID</returns>
        [HttpGet]
        public object CheckLoadStatus([FromUri]string LoadId, [FromUri]string WhCode)
        {
            IShipWinceManger res = new ShipWinceManger();
            if (res.CheckLoadStatus(WhCode, LoadId))
            {
                ShipPickDesModel shipPickDesModel = new ShipPickDesModel();
                shipPickDesModel.ProcessId = res.GetLoadProcessId(WhCode, LoadId);
                shipPickDesModel.LoadId = LoadId;
                shipPickDesModel.WhCode = WhCode;
                return Helper.ResultData("Y", "", shipPickDesModel);
            }
            else
                return Helper.ResultData("N", "出货计划已备货或不存在", new { });
        }
        #endregion
        /// <summary>
        /// 获取备货基本提示资料
        /// </summary>
        /// <param name="LoadId"></param>
        /// <param name="WhCode"></param>
        /// <returns></returns>
        [HttpGet]
        public object GetPickingRes([FromUri]string LoadId, [FromUri]string WhCode)
        {
            IShipWinceManger res = new ShipWinceManger();
            return Helper.ResultData("Y","",res.GetPickingRes(WhCode, LoadId));
        }
        /// <summary>
        /// 获取备货基本提示资料
        /// </summary>
        /// <param name="LoadId"></param>
        /// <param name="WhCode"></param>
        /// <param name="Index"></param>
        /// <returns></returns>
        [HttpGet]
        public object GetPickingRes([FromUri]string LoadId, [FromUri]string WhCode, [FromUri]int Index)
        {
            IShipWinceManger res = new ShipWinceManger();
            return Helper.ResultData("Y", "", res.GetPickingRes(WhCode, LoadId, Index));
        }

        /// <summary>
        /// 获取备货基本提示资料
        /// </summary>
        /// <param name="LoadId"></param>
        /// <param name="WhCode"></param>
        /// <param name="Index"></param>
        /// <returns></returns>
        [HttpGet]
        public object GetPickingResList([FromUri]string LoadId, [FromUri]string WhCode, [FromUri]int count)
        {
            IShipWinceManger res = new ShipWinceManger();
            return Helper.ResultData("Y", "", res.GetPickingResList(WhCode, LoadId, count));
        }
        //获取托盘总数量
        [HttpGet]
        public object GetPickingPltRes([FromUri]string WhCode, [FromUri]string HuId)
        {
            IShipWinceManger res = new ShipWinceManger();
            return Helper.ResultData("Y", "", res.GetPickingPltRes(WhCode, HuId));
        }

        /// <summary>
        /// 拆托检测
        /// </summary>
        /// <param name="LoadId"></param>
        /// <param name="WhCode"></param>
        /// <param name="HuId"></param>
        /// <returns></returns>
        [HttpGet,HttpPost]
        public object CheckPickingLoad(ApiRequestDataModel apiRequestDataModel)
        {
         // string jsonStr = "{\"recModel\":null,\"pallatesModel\":null,\"shipLoadPlt\":{\"LoadId\":\"LD170111142410002\",\"WhCode\":\"01\",\"UserName\":\"1012\",\"HuId\":\"PLTD247245\",\"PutHuId\":\"\",\"Location\":null}}";

        //    ApiRequestDataModel apiRequestDataModel= JsonConvert.DeserializeObject<ApiRequestDataModel>(jsonStr);

            IShipWinceManger res = new ShipWinceManger();
            string resStr = res.CheckPickingLoad(apiRequestDataModel.shipLoadPlt.WhCode, apiRequestDataModel.shipLoadPlt.LoadId, apiRequestDataModel.shipLoadPlt.HuId);
            //DC的前台会按照ShipSplit做相似匹配成ShipSplitDC,AGV为ShipSplitAGV,ShipLocationAGV
            if (resStr == "Y")
                return Helper.ResultData("Y","", new OpenFormModel { formName= "ShipLocation" });
            else if(resStr=="N")
                return Helper.ResultData("Y", resStr, new OpenFormModel { formName = "ShipSplit" });
            else
                return Helper.ResultData("N", resStr, new{ });

        }
        [HttpGet, HttpPost]
        public object CheckPickSplitPlt(ApiRequestDataModel apiRequestDataModel)
        {
            IShipWinceManger res = new ShipWinceManger();
           // string jsonStr = "{\"recModel\":null,\"pallatesModel\":null,\"shipLoadPlt\":{\"LoadId\":\"LD170111142410002\",\"WhCode\":\"01\",\"UserName\":\"1012\",\"HuId\":\"PLTD247247\",\"PutHuId\":\"L17911\",\"Location\":null}}";

           // ApiRequestDataModel apiRequestDataModel = JsonConvert.DeserializeObject<ApiRequestDataModel>(jsonStr);
            string resStr = res.CheckPickSplitPlt(apiRequestDataModel.shipLoadPlt.WhCode, apiRequestDataModel.shipLoadPlt.LoadId, apiRequestDataModel.shipLoadPlt.HuId, apiRequestDataModel.shipLoadPlt.PutHuId);
            if (resStr=="Y")
            {
                return Helper.ResultData("Y","", new { });
            }
            else
                return Helper.ResultData("N", resStr, new { });

        }

        [HttpGet]
        public object GetSerialNumberOut([FromUri]string WhCode , [FromUri]string LoadId, [FromUri]string HuId)
        {

            IShipWinceManger aa = new ShipWinceManger();
            List<string> res = aa.GetSerialNumberOut(WhCode, LoadId, HuId);
            if (res.Count > 0)
            {
                return Helper.ResultData("Y", null, res);
            }
            else
                return Helper.ResultData("N", "无可扫描的数据!", new { });
        }
        //备货
        [HttpPost]
        public object PickingLoad(ShipLoadPlt shipLoadPlt)
        {

            // string jsonStr = "{\"LoadId\":\"LD170111142410002\",\"WhCode\":\"01\",\"UserName\":\"1012\",\"HuId\":\"PLTD247245\",\"PutHuId\":\"\",\"Location\":\"A04\"}";

            //ShipLoadPlt shipLoadPlt = JsonConvert.DeserializeObject<ShipLoadPlt>(jsonStr);

            IShipLoadManager shipLoadManager = new ShipLoadManager();
            IShipWinceManger shipWince = new ShipWinceManger();
            //拆托自动不需要维护托盘情况
            if (shipLoadPlt.IfHavePutHuId == 1 && string.IsNullOrEmpty(shipLoadPlt.PutHuId)) {
               
                string PutHuId=shipWince.GetSysPlt(shipLoadPlt.WhCode);
                if(PutHuId==null)
                    return Helper.ResultData("N", "系统无可用的虚拟托盘!稍后再试!", new { });
                shipLoadPlt.PutHuId = shipWince.GetSysPlt(shipLoadPlt.WhCode);
            }
            string res = shipLoadManager.PickingLoad(shipLoadPlt.LoadId, shipLoadPlt.WhCode, shipLoadPlt.UserName, shipLoadPlt.HuId, shipLoadPlt.PutHuId, shipLoadPlt.Location);
            //转换SSCC的拆托托盘号
            if (shipLoadPlt.IfSerialNumberChange == 1 && !string.IsNullOrEmpty(shipLoadPlt.PutHuId)&& res=="Y")
            {
                res = shipWince.SerialNumberChange(shipLoadPlt.WhCode, shipLoadPlt.LoadId, shipLoadPlt.HuId, shipLoadPlt.PutHuId, shipLoadPlt.UserName);

            }

            if (res=="Y")
            {
                return Helper.ResultData("Y","", new { });
            }
            else
                return Helper.ResultData("N", res, new { });

        }


        /// <summary>
        /// 验证备货单是否全部完成
        /// </summary>
        /// <param name="WhCode"></param>
        /// <param name="LoadId"></param>
        /// <returns>没有全部完成的话返回到ShipPltIn 托盘输入界面</returns>
        [HttpGet,HttpPost]
        public object CheckPickingComplete(ApiRequestDataModel apiRequestDataModel )
        {
            IShipWinceManger res = new ShipWinceManger();
            if (res.CheckPickingComplete(apiRequestDataModel.shipLoadPlt.WhCode, apiRequestDataModel.shipLoadPlt.LoadId))
            {
                return Helper.ResultData("Y", "", new { });
            }
            else
                return Helper.ResultData("Y", "", new OpenFormModel { formName = "ShipPltIn" });

        }

        [HttpGet]
        public object GetPickingSplitDes([FromUri]string WhCode, [FromUri]string LoadId, [FromUri]string HuId)
        {
            IShipWinceManger res = new ShipWinceManger();

            List<ShipPickSplitDesModel> aa=res.GetPickingSplitDes(WhCode, LoadId, HuId);
            if (aa.Count>0)
            {
                return Helper.ResultData("Y", "", aa);
            }
            else
                return Helper.ResultData("N", "无拆托数据!系统异常", new { });

        }
        [HttpGet]
        public object DeliveryExceptionRegister([FromUri]string WhCode, [FromUri]string ExpressNumber, [FromUri]string user)
            {
            IOutBoundOrderManager res = new OutBoundOrderManager();

            string aa = res.DeliveryExceptionRegister(WhCode, ExpressNumber, user);
            if (aa=="Y")
                {
                return Helper.ResultData("Y", "", new { });
                }
            else
                return Helper.ResultData("N", "订单已扫描", new { });

            }

        }
}

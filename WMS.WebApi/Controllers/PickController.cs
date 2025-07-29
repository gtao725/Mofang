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
    public class PickController : ApiController
    {
        Helper Helper = new Helper();
        #region wince ShipLoadIn 检测出货计划状态是否正确
        /// <summary>
        /// 获取拣货的出货计划
        /// </summary>
        /// <param name="LoadId">LoadId</param>
        ///  <param name="UserName">UserName</param>
        /// <param name="WhCode">WhCode</param>
        /// <returns>正确的话返回Y和备货流程ID</returns>
        [HttpGet]
        public object GetPickLoad([FromUri]string LoadId, [FromUri]string UserName, [FromUri]string WhCode)
        {
            IPickWinceManger res = new PickWinceManager();
 

            List<string> listStrModel = res.PickLoadList(WhCode, LoadId, UserName);
            if(listStrModel.Count>0)
                return Helper.ResultData("Y", "", listStrModel);
            else
                return Helper.ResultData("N", "没有可拣货的出货计划!", "");

        }
        #endregion

        /// <summary>
        /// 获取拣货订单号
        /// </summary>
        /// <param name="LoadId"></param>
        /// <param name="WhCode"></param>
        /// <returns></returns>
        [HttpGet]
        public object GetPickTaskOrder([FromUri]string LoadId, [FromUri]string WhCode)
        {
            IPickWinceManger res = new PickWinceManager();
 
            List<string> listStrModel = res.GetPickTaskOrder(WhCode, LoadId);
            if (listStrModel.Count > 0)
                return Helper.ResultData("Y", "", listStrModel);
            else
                return Helper.ResultData("N", "没有可拣货的订单!", "");
        }
        /// <summary>
        /// 获取备货基本提示资料
        /// </summary>
        /// <param name="LoadId"></param>
        /// <param name="WhCode"></param>
        /// <param name="Index"></param>
        /// <returns></returns>
        [HttpGet]
        public object GetPickTaskDetail( [FromUri]string WhCode, [FromUri]string LoadId, [FromUri]int OutBoundOrderId)
        {
            IPickWinceManger res = new PickWinceManager();

            List<PickTaskDetailWince> listStrModel = res.GetPickTaskDetail(WhCode, LoadId, OutBoundOrderId);
            if (listStrModel.Count > 0)
                return Helper.ResultData("Y", "", listStrModel);
            else
                return Helper.ResultData("N", "获取拣货明细失败!", "");
        }

       

        //拣货
        [HttpPost]
        public object PickLoad(PickTaskDetailResult pickTaskDetailResult, [FromUri]string WhCode, [FromUri]string packGroupNumber, [FromUri]string Location, [FromUri]string userName)
        {

            //  public string PickingSortingPackingByOrerBegin(List<PickTaskDetailResult> entityList, string whCode, string packGroupNumber, string Location, string userName)

            List<PickTaskDetailResult> pickTaskDetailResultList = new List<PickTaskDetailResult>();
            pickTaskDetailResultList.Add(pickTaskDetailResult);

            IShipLoadManager shipLoadManager = new ShipLoadManager();
            string res = shipLoadManager.PickingSortingPackingByOrerBegin(pickTaskDetailResultList, WhCode, packGroupNumber, Location, userName);
            if (res=="Y")
            {
                //返货拣货明细
                IPickWinceManger pickWinceManger = new PickWinceManager();
                List<PickTaskDetailWince> pickTaskDetailWinceList = pickWinceManger.GetPickTaskDetail(WhCode, pickTaskDetailResult.LoadId,(int) pickTaskDetailResult.OutBoundOrderId);
                if (pickTaskDetailWinceList.Count > 0)
                    return Helper.ResultData("Y", "", pickTaskDetailWinceList);
                else
                    return Helper.ResultData("Y","", new { });
            }
            else
                return Helper.ResultData("N", res, new { });

        }


        
        

    }
}

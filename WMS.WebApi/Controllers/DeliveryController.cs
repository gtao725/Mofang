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
    public class DeliveryController : ApiController
    {
        Helper Helper = new Helper();

        /// <summary>
        /// 检测出货计划状态是否备货
        /// </summary>
        /// <param name="LoadId">LoadId</param>
        /// <param name="WhCode">WhCode</param>
        /// <returns>正确的话返回Y和出货流程ID</returns>
        [HttpGet]
        public object CheckDeliveryLoadStatus([FromUri]string LoadId, [FromUri]string WhCode)
        {
            IShipWinceManger res = new ShipWinceManger();
            if (res.CheckDeliveryLoadStatus(WhCode, LoadId))
            {
                return Helper.ResultData("Y", "", res.GetShipLoadDesHead(WhCode, LoadId));
            }
            else
                return Helper.ResultData("N", "出货计划不能封箱!", new { });
        }
 
       
        [HttpGet]
        public object ShippingLoad([FromUri]string LoadId, [FromUri]string WhCode, [FromUri]string userName, [FromUri]string  DeliveryOrderNumber)
        {
            IShipWinceManger res = new ShipWinceManger();
            string resStr = res.ShippingLoad(LoadId, WhCode, userName);
            if (resStr == "Y")
            {
                return Helper.ResultData("Y", "", new { });
            }
            else
                return Helper.ResultData("N", resStr, new { });

          
        }
        [HttpGet]
        public object ShippingLoadCustomer([FromUri]string LoadId, [FromUri]string WhCode, [FromUri]string userName, [FromUri]string DeliveryOrderNumber)
        {
            IShipWinceManger res = new ShipWinceManger();
            string resStr = res.ShippingLoadCustomer(LoadId, DeliveryOrderNumber, WhCode, userName);
            if (resStr == "Y")
            {
                return Helper.ResultData("Y", "", new { });
            }
            else
                return Helper.ResultData("N", resStr, new { });


        }

        
        [HttpGet]
        public object DeliveryQtyCheck([FromUri]string LoadId, [FromUri]string WhCode)
        {
            IShipWinceManger res = new ShipWinceManger();
      
            if (res.DeliveryQtyCheck(LoadId, WhCode))
            {
                return Helper.ResultData("Y", "Y", new { });
            }
            else
                return Helper.ResultData("N", "无需输入挂衣把数", new { });


        }


        [HttpGet]
        public object ShippingLoadCNSN([FromUri]string LoadId, [FromUri]string WhCode, [FromUri]string userName, [FromUri]string containerNumber, [FromUri]string sealNumber)
        {
            IShipWinceManger res = new ShipWinceManger();
            string resStr = res.ShippingLoad(LoadId, WhCode, userName, containerNumber, sealNumber);
            if (resStr == "Y")
            {

                return Helper.ResultData("Y", "", new { });
            }
            else
                return Helper.ResultData("N", resStr, new { });

            //ShippingLoad(string loadId, string whCode, string userName, string containerNumber, string sealNumber)
        }

        [HttpGet]
        public object ShippingLoadCNSN([FromUri]string LoadId, [FromUri]string WhCode, [FromUri]string userName, [FromUri]string containerNumber, [FromUri]string sealNumber, [FromUri]int baQty)
        {
            IShipWinceManger res = new ShipWinceManger();
            string resStr = res.ShippingLoad(LoadId, WhCode, userName, containerNumber, sealNumber, baQty);
            if (resStr == "Y")
            {
                return Helper.ResultData("Y", "", new { });
            }
            else
                return Helper.ResultData("N", resStr, new { });

            //ShippingLoad(string loadId, string whCode, string userName, string containerNumber, string sealNumber)
        }
        
    }
}

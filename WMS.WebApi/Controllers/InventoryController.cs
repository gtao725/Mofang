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
    public class InventoryController : ApiController
    {
        Helper Helper = new Helper();
        [HttpGet]
        public object CheckRecLocation([FromUri]string WhCode, [FromUri]string Location)
        {
            IRecWinceManager aa = new RecWinceManager();
            if (aa.CheckRecLocation(WhCode, Location))
                return Helper.ResultData("Y", "", new { });
            else
                return Helper.ResultData("N", "收货区不存在!", new { });
        }

        [HttpGet]
        public object CheckSpecialLocation([FromUri]string WhCode, [FromUri]string Location)
        {
            IRecWinceManager aa = new RecWinceManager();
            if (aa.CheckReturnLocation(WhCode, Location))
                return Helper.ResultData("Y", "", new { });
            else
                return Helper.ResultData("N", "异常库位不存在!", new { });
        }
        [HttpGet]
        public object IfPltStock([FromUri]string WhCode, [FromUri]string Location, [FromUri]string HuId)
        {
            IInventoryWinceManager aa = new InventoryWinceManager();
            string res = aa.IfPltStock(WhCode, Location, HuId);
            if (res == "Y")
                return Helper.ResultData("Y", "", new { });
            else

                return Helper.ResultData("N", res, new { });
        }

        /// <summary>
        /// 获取收货上架建议
        /// </summary>
        /// <param name="WhCode"></param>
        /// <param name="HuId"></param>
        /// <returns></returns>
        [HttpGet]
        public object GetRecStockSug([FromUri]string WhCode, [FromUri]string HuId)
        {
            IInventoryWinceManager aa = new InventoryWinceManager();
            return Helper.ResultData("Y", aa.GetRecStockSug(WhCode, HuId), aa.PltItemMoveList(WhCode, HuId));

        }

        [HttpGet]
        public object RecStockMove([FromUri]string WhCode, [FromUri]string Location, [FromUri] string DestLoc, [FromUri]string HuId, [FromUri] string User)
        {
            IInventoryWinceManager aa = new InventoryWinceManager();
            string res = aa.RecStockMove(WhCode, Location, DestLoc, HuId, User);
            if (res == "Y")
                return Helper.ResultData("Y", "", new { });
            else
                return Helper.ResultData("N", res, new { });
        }
        [HttpGet]
        public object StockMove([FromUri]string WhCode, [FromUri]string Location, [FromUri] string DestLoc, [FromUri]string HuId, [FromUri] string User)
        {
            IInventoryWinceManager aa = new InventoryWinceManager();
            string res = aa.StockMove(WhCode, Location, DestLoc, HuId, User);
            if (res == "Y")
                return Helper.ResultData("Y", "", new { });
            else
                return Helper.ResultData("N", res, new { });
        }
        [HttpGet]
        public object GetPltClient([FromUri]string WhCode, [FromUri]string HuId, [FromUri]string ActionType)
        {
            IInventoryWinceManager aa = new InventoryWinceManager();

            string checkRes = aa.IfHuIdLocked(WhCode, HuId);
            //获取资料的时候检查托盘是否已经冻结或者解锁
            if (ActionType == "Hold" && checkRes == "Y")
                //已经冻结的就不能再冻结了
                return Helper.ResultData("N", "托盘已冻结", new { });
            else if (ActionType == "UnHold" && checkRes == "N")
                //已经解锁的不能再解锁
                return Helper.ResultData("N", "托盘已解锁", new { });
            else if (checkRes != "Y" && checkRes != "N")
                return Helper.ResultData("N", checkRes, new { });

            int? res = aa.GetPltClient(WhCode, HuId);
            if (res != 0)
                return Helper.ResultData("Y", res.ToString(), new { });
            else
                return Helper.ResultData("N", "托盘客户异常", new { });
        }


        [HttpGet]
        public object PltLock([FromUri]string WhCode, [FromUri]string HuId, [FromUri] string User, [FromUri] int HoldId, [FromUri] string HoldReason, [FromUri] string TCRFlag)
        {

            HuMaster huMaster = new HuMaster();
            huMaster.HuId = HuId;
            huMaster.HoldId = HoldId;
            huMaster.HoldReason = HoldReason;
            huMaster.WhCode = WhCode;
            huMaster.UpdateDate = DateTime.Now;
            huMaster.UpdateUser = User;
            huMaster.Status = "H";

            IInventoryWinceManager bb = new InventoryWinceManager();
            string checkRes = bb.IfHuIdLocked(WhCode, HuId);
            //已经冻结的就不能再冻结了
            if (checkRes != "N")
                return Helper.ResultData("N", checkRes == "Y" ? "该托盘已经冻结!" : checkRes, new { });

            IInVentoryManager aa = new InVentoryManager();
            string res = aa.PltHoldEdit(huMaster);
            if (res == "Y")
                if (TCRFlag == "Y")
                {
                    res = aa.InventoryTCR(huMaster);
                    if (res == "Y")
                        return Helper.ResultData("Y", "", new { });
                    else
                        return Helper.ResultData("N", res, new { });
                }
                else
                    return Helper.ResultData("Y", "", new { });

            else
                return Helper.ResultData("N", res, new { });
        }

        [HttpGet]
        public object PltUnLock([FromUri]string WhCode, [FromUri]string HuId, [FromUri] string User)
        {
            HuMaster huMaster = new HuMaster();
            huMaster.HuId = HuId;
            huMaster.HoldId = null;
            huMaster.HoldReason = null;
            huMaster.WhCode = WhCode;
            huMaster.UpdateDate = DateTime.Now;
            huMaster.UpdateUser = User;
            huMaster.Status = "A";

            IInventoryWinceManager bb = new InventoryWinceManager();
            string checkRes = bb.IfHuIdLocked(WhCode, HuId);
            //已经解锁的不能再解锁
            if (checkRes != "Y")
                return Helper.ResultData("N", checkRes == "N" ? "该托盘已经解锁!" : checkRes, new { });

            IInVentoryManager aa = new InVentoryManager();
            string res = aa.PltHoldEdit(huMaster);
            if (res == "Y")
                return Helper.ResultData("Y", "", new { });
            else
                return Helper.ResultData("N", res, new { });
        }

        /// <summary>
        /// 检测托盘是否有库存,并返回托盘库位
        /// </summary>
        /// <param name="WhCode"></param>
        /// <param name="HuId"></param>
        /// <returns></returns>
        [HttpGet]
        public object IfPltHaveStock([FromUri]string WhCode, [FromUri]string HuId)
        {
            IInventoryWinceManager aa = new InventoryWinceManager();

            if (aa.IfPltHaveStock(WhCode, HuId))
                return Helper.ResultData("Y", aa.GetPltLocation(WhCode, HuId), new { });
            else
                return Helper.ResultData("N", "托盘无库存!", new { });
        }




        /// <summary>
        /// 检测盘点任务是否存在
        /// </summary>
        /// <param name="WhCode"></param>
        /// <param name="TaskNumber"></param>
        /// <returns>Y或错误结果</returns>
        [HttpGet]
        public object IfCycleTaskNumber([FromUri] string WhCode, [FromUri] string TaskNumber)
        {

            IInventoryWinceManager aa = new InventoryWinceManager();
            string res = aa.IfCycleTaskNumber(WhCode, TaskNumber);
            if (res == "Y")
                return Helper.ResultData("Y", aa.CycleTaskNumberOneByOneScanFlag(WhCode, TaskNumber).ToString(), new { });
            else
                return Helper.ResultData("N", res, new { });

        }

        /// <summary>
        /// 获取盘点任务建议库位,按照库位名顺序排序
        /// </summary>
        /// <param name="WhCode"></param>
        /// <param name="TaskNumber"></param>
        /// <returns>没有的话返回null</returns>
        [HttpGet]
        public object GetCTSugLoc([FromUri] string WhCode, [FromUri] string TaskNumber)
        {

            IInventoryWinceManager aa = new InventoryWinceManager();
            string res = aa.GetCTSugLoc(WhCode, TaskNumber);
            if (res != null)
                return Helper.ResultData("Y", res, new { });
            else
                return Helper.ResultData("Y", "无", new { });

        }
        /// <summary>
        /// 获取盘点任务剩余库位数
        /// </summary>
        /// <param name="WhCode"></param>
        /// <param name="TaskNumber"></param>
        /// <returns>没有的话返回0</returns>
        [HttpGet]
        public object GetLocRemainingQty([FromUri] string WhCode, [FromUri] string TaskNumber)
        {
            IInventoryWinceManager aa = new InventoryWinceManager();
            int res = aa.GetLocRemainingQty(WhCode, TaskNumber);
            if (res != 0)
                return Helper.ResultData("Y", res.ToString(), new { });
            else
                return Helper.ResultData("Y", "0", new { });

        }

        /// <summary>
        /// 检测托盘是否在盘点任务中
        /// </summary>
        /// <param name="WhCode"></param>
        /// <param name="TaskNumber"></param>
        /// <param name="LocationId"></param>
        /// <returns>Y或异常信息</returns>

        [HttpGet]
        public object IfLocInCycleTaskNumber([FromUri] string WhCode, [FromUri] string TaskNumber, [FromUri]string LocationId)
        {
            IInventoryWinceManager aa = new InventoryWinceManager();

            string res = aa.IfLocInCycleTaskNumber(WhCode, TaskNumber, LocationId);
            if (res == "Y")
                return Helper.ResultData("Y", "", new { });
            else
                return Helper.ResultData("N", res, new { });

        }

        /// <summary>
        /// 盘点插入
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        [HttpPost]
        public object CycleCountInsert(CycleCountInsertComplex entity)
        {

            IInventoryWinceManager aa = new InventoryWinceManager();

            // string jsonStr = "{\"TaskNumber\":\"PD170208144410001\",\"WhCode\":\"02\",\"LocationId\":\"100\",\"HuIdModel\":[{\"AltItemNumber\":null,\"HuId\":\"123\",\"Qty\":12},{\"AltItemNumber\":null,\"HuId\":\"1234\",\"Qty\":12}],\"CreateUser\":\"1012\",\"CreateDate\":\"02/08/2017 14:58:31\"}";

            // CycleCountInsertComplex entity = JsonConvert.DeserializeObject<CycleCountInsertComplex>(jsonStr);


            string res = aa.CycleCountInsert(entity);
            if (res == "Y")
                return Helper.ResultData("Y", "", new { });
            else
                return Helper.ResultData("N", res, new { });

        }

        /// <summary>
        /// 盘点插入
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        [HttpPost]
        public object CycleCountSkuInsert(CycleCountInsertComplexAddPo entity)
        {

            IInventoryWinceManager aa = new InventoryWinceManager();

            // string jsonStr = "{\"TaskNumber\":\"PD170208144410001\",\"WhCode\":\"02\",\"LocationId\":\"100\",\"HuIdModel\":[{\"AltItemNumber\":null,\"HuId\":\"123\",\"Qty\":12},{\"AltItemNumber\":null,\"HuId\":\"1234\",\"Qty\":12}],\"CreateUser\":\"1012\",\"CreateDate\":\"02/08/2017 14:58:31\"}";

            // CycleCountInsertComplex entity = JsonConvert.DeserializeObject<CycleCountInsertComplex>(jsonStr);


            string res = aa.CycleCountInsert(entity);
            if (res == "Y")
                return Helper.ResultData("Y", "", new { });
            else
                return Helper.ResultData("N", res, new { });

        }


        /// <summary>
        /// 盘点EAN转换成SKU
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        [HttpPost]
        public object CycleEANChangeToSKU(CycleCountInsertComplexAddPo entity)
        {

            IInventoryWinceManager aa = new InventoryWinceManager();

            CycleCountInsertComplexAddPo res = aa.CycleEANChangeToSKU(entity);
            if (res != null)
                return Helper.ResultData("Y", "", res);
            else
                return Helper.ResultData("N", "EAN转换错误", new { });

        }



        /// <summary>
        /// 盘点检测
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        [HttpPost]
        public object CheckCycleResult(CycleCountInsertComplexAddPo entity)
        {

            IInventoryWinceManager aa = new InventoryWinceManager();

            string res = aa.CheckCycleResult(entity);
            if (res == "Y")
                return Helper.ResultData("Y", "", new { });
            else
                return Helper.ResultData("N", res, new { });

        }
        /// <summary>
        /// 盘点完成
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>

        [HttpGet]
        public object CycleCountComplete([FromUri] string WhCode, [FromUri] string TaskNumber, [FromUri] string User)
        {
            IInventoryWinceManager aa = new InventoryWinceManager();
            string res = aa.CycleCountComplete(WhCode, TaskNumber, User);
            if (res == "Y")
                return Helper.ResultData("Y", "", new { });
            else
                return Helper.ResultData("N", res, new { });

        }

        [HttpGet]
        public object PltItemEnableMove([FromUri] string WhCode, [FromUri] string HuId)
        {
            IInventoryWinceManager aa = new InventoryWinceManager();
            string res = aa.PltItemEnableMove(WhCode, HuId);
            if (res == "Y")
                return Helper.ResultData("Y", "", new { });
            else
                return Helper.ResultData("N", res, new { });

        }

        [HttpGet]
        public object DestPltItemEnableMove([FromUri]string WhCode, [FromUri]string DestHuId)
        {
            IInventoryWinceManager aa = new InventoryWinceManager();
            string res = aa.DestPltItemEnableMove(WhCode, DestHuId, null);
            if (res == "Y")
                return Helper.ResultData("Y", "", new { });
            else if (res.Split('$')[0] == "N")
                return Helper.ResultData("Y", "N", new { });
            else
                return Helper.ResultData("N", res, new { });

        }
        [HttpGet]
        public object PltItemMoveList([FromUri] string WhCode, [FromUri] string HuId)
        {
            IInventoryWinceManager aa = new InventoryWinceManager();
            List<HuDetailResult> res = aa.PltItemMoveList(WhCode, HuId);
            if (res != null)
                return Helper.ResultData("Y", "", res);
            else
                return Helper.ResultData("N", "加载托盘明细失败", new { });

        }
        [HttpGet]
        public object PltItemMoveList([FromUri] string WhCode, [FromUri] string HuId, [FromUri] string SKU)
        {
            IInventoryWinceManager aa = new InventoryWinceManager();
            List<HuDetailResult> res = aa.PltItemMoveList(WhCode, HuId, SKU);
            if (res != null)
                return Helper.ResultData("Y", "", res);
            else
                return Helper.ResultData("N", "加载托盘明细失败", new { });

        }
        [HttpGet]
        public object PltItemMoveAction([FromUri] string WhCode, [FromUri] string HuId, [FromUri] string DestHuId, [FromUri] string DestLoc, [FromUri]int HuDetailId, [FromUri] int MoveQty, [FromUri] string User)
        {

            IInventoryWinceManager rm = new InventoryWinceManager();
            string res = rm.PltItemMoveAction(WhCode, HuId, DestHuId, DestLoc, HuDetailId, MoveQty, User);

            if (res == "Y")
                return Helper.ResultData("Y", "", new { });
            else
                return Helper.ResultData("N", res, new { });

        }

        //[HttpGet]
        //public object PltList([FromUri] string WhCode, [FromUri] string HuId)
        //{
        //    IInventoryWinceManager aa = new InventoryWinceManager();
        //    string loc = null;
        //    if (aa.IfPltHaveStock(WhCode, HuId))
        //    {
        //        loc = aa.GetPltLocation(WhCode, HuId);
        //        List<HuDetailResult> res = aa.PltItemMoveList(WhCode, HuId);
        //        return Helper.ResultData("Y", loc, res);
        //    }
        //    else
        //        return Helper.ResultData("N", "加载托盘明细失败", new { });

        //}

        [HttpGet]
        public object PltList([FromUri] string WhCode, [FromUri] string HuId)
        {
            IInventoryWinceManager aa = new InventoryWinceManager();
            string loc = null;
            if (aa.IfPltHaveStock(WhCode, HuId))
            {
                loc = aa.GetPltLocation(WhCode, HuId);
                IRecWinceManager bb = new RecWinceManager();
                List<HuDetailRemained> res = bb.RecScanRemainedPlt(WhCode, HuId);
                return Helper.ResultData("Y", loc, res);
            }
            else
                return Helper.ResultData("N", "加载托盘明细失败", new { });

        }

        [HttpGet]
        public object HuInfo([FromUri] string WhCode, [FromUri] string HuId)
        {
            IInventoryWinceManager aa = new InventoryWinceManager();
            string loc = null;
            if (aa.IfPltHaveStock(WhCode, HuId))
            {
                //loc = aa.GetPltLocation(WhCode, HuId);
                IRecWinceManager bb = new RecWinceManager();
                HuInfo res = aa.GetPlt(WhCode, HuId);
                return Helper.ResultData("Y", loc, res);
            }
            else
                return Helper.ResultData("N", "加载托盘明细失败", new { });

        }


        [HttpGet]
        public object PltLWH([FromUri] string WhCode, [FromUri] string HuId)
        {
            IInventoryWinceManager aa = new InventoryWinceManager();
            HuMasterResult Result = aa.GetPltLWH(WhCode, HuId);
            if (Result != null)
            {

                return Helper.ResultData("Y", "", Result);
            }
            else
                return Helper.ResultData("N", "托盘没有库存", new { });

        }

        [HttpGet]
        public object PltSetLWH([FromUri]string WhCode, [FromUri]string HuId, [FromUri]string HuLength, [FromUri]string HuWidth, [FromUri]string HuHeight, [FromUri] string User)
        {
            HuMasterResult huMasterResult = new HuMasterResult();
            huMasterResult.HuId = HuId;
            huMasterResult.WhCode = WhCode;
            huMasterResult.HuLength = Convert.ToDecimal(HuLength) / 100;
            huMasterResult.HuWidth = Convert.ToDecimal(HuWidth) / 100;
            huMasterResult.HuHeight = Convert.ToDecimal(HuHeight) / 100;

            IInventoryWinceManager bb = new InventoryWinceManager();
            string res = bb.SetPltLWH(huMasterResult, User);
            if (res == "Y")
                return Helper.ResultData("Y", "", new { });
            else
                return Helper.ResultData("N", res, new { });
        }
        [HttpGet]
        public object LocList([FromUri] string WhCode, [FromUri] string LocationId)
        {
            IInventoryWinceManager aa = new InventoryWinceManager();
            List<HuDetailResult> res = aa.LocationHuList(WhCode, LocationId);
            if (res == null)
            {
                return Helper.ResultData("N", "库位没有库存托盘!", new { });
            }

            if (res.Count() > 0)
            {
                int huCounts = res.Select(u => u.HuId).Distinct().Count();
                return Helper.ResultData("Y", huCounts.ToString(), res);
            }
            else
                return Helper.ResultData("N", "库位没有库存托盘!", new { });

        }



        [HttpGet]
        public object GetSpecialLocList([FromUri]string WhCode, [FromUri]string Location, [FromUri]string Sku)
        {
            IInventoryWinceManager aa = new InventoryWinceManager();

            List<HuDetailResult> res = aa.LocationHuList(WhCode, Location, Sku);
            if (res != null)
            {
                return Helper.ResultData("Y", "", res);
            }
            else
                return Helper.ResultData("N", "该款号没有库存!", new { });

        }

        [HttpGet]
        public object SNSearch([FromUri] string WhCode, [FromUri] string SN)
        {
            IInventoryWinceManager aa = new InventoryWinceManager();
            string res = aa.SNSearch(WhCode, SN);
            if (!string.IsNullOrEmpty(res))
            {
                return Helper.ResultData("Y", res, new { });
            }
            else
                return Helper.ResultData("N", "SN不存或者没有库存!", new { });

        }
        [HttpGet]
        public object GetItemZone([FromUri] string WhCode, [FromUri] string AltItemNumber)
        {
            IInventoryWinceManager aa = new InventoryWinceManager();

            List<ItemZone> res = aa.GetItemZone(WhCode, AltItemNumber);
            if (res != null)
                return Helper.ResultData("Y", "", res);
            else
                return Helper.ResultData("N", "此款号没有库存", new { });

        }
        [HttpGet]
        public object GetZoneLoc([FromUri] string WhCode, [FromUri] string ZoneName)
        {
            IInventoryWinceManager aa = new InventoryWinceManager();

            List<ItemZone> res = aa.GetZoneLoc(WhCode, ZoneName);
            if (res != null)
                if (res.Count() > 60)
                    return Helper.ResultData("N", "区域查询行数过多,请输入详细区域", new { });
                else
                    return Helper.ResultData("Y", "", res);
            else
                return Helper.ResultData("N", "此区域没有储位", new { });

        }
        [HttpGet]
        public object GetItemLoc([FromUri] string WhCode, [FromUri] string AltItemNumber)
        {
            IInventoryWinceManager aa = new InventoryWinceManager();

            List<ItemZone> res = aa.GetItemLoc(WhCode, AltItemNumber);
            if (res != null)
                return Helper.ResultData("Y", "", res);
            else
                return Helper.ResultData("N", "此款号没有库存", new { });

        }

        [HttpGet]
        public object CreateInvMove([FromUri] string WhCode, [FromUri] string user)
        {
            IInVentoryManager aa = new InVentoryManager();
            string res = aa.CreateInvMove(WhCode, user);

            return Helper.ResultData(res.Split('$')[0], res.Split('$')[1], new { });


        }



    }
}

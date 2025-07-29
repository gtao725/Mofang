using MODEL_MSSQL;
using System.Collections.Generic;
using WMS.BLLClass;

namespace WMS.IBLL
{
    public interface IInventoryWinceManager
    {

        /// <summary>
        /// 验证收货库存是否存在
        /// </summary>
        /// <param name="locationId"></param>
        /// <param name="whCode"></param>
        /// <returns></returns>
        bool CheckWhLocation(string WhCode, string Location);
        /// <summary>
        /// 检测托盘是否有未摆货数据在该库区
        /// </summary>
        /// <param name="WhCode"></param>
        /// <param name="Location">库区</param>
        /// <param name="HuId">托盘号</param>
        /// <returns></returns>
        string IfPltStock(string WhCode, string Location, string HuId);
        /// <summary>
        /// 上架
        /// </summary>
        /// <param name="WhCode"></param>
        /// <param name="Location">当前库位</param>
        /// <param name="DestLoc">目标库位</param>
        /// <param name="HuId">托盘号</param>
        /// <returns></returns>
        string RecStockMove(string WhCode, string Location, string DestLoc, string HuId, string User);

        /// <summary>
        /// 移库
        /// </summary>
        /// <param name="WhCode"></param>
        /// <param name="Location">当前库位</param>
        /// <param name="DestLoc">目标库位</param>
        /// <param name="HuId">托盘号</param>
        /// <returns></returns>
        string StockMove(string WhCode, string Location, string DestLoc, string HuId, string User);
        /// <summary>
        /// 获取托盘的客户ID
        /// </summary>
        /// <param name="WhCode"></param>
        /// <param name="HuId"></param>
        /// <returns></returns>
        int? GetPltClient(string WhCode, string HuId);
        /// <summary>
        /// 检测托盘是否锁定
        /// </summary>
        /// <param name="WhCode"></param>
        /// <param name="HuId"></param>
        /// <returns>Y是锁定,N是未锁定,否则就是异常原因</returns>
        /// 

        /// <summary>
        /// 获取托盘信息-用于锁定
        /// </summary>
        HuInfo GetPlt(string WhCode, string HuId);



        string IfHuIdLocked(string WhCode, string HuId);

        /// <summary>
        /// 检测托盘是否存在并且有货
        /// </summary>
        /// <param name="WhCode"></param>
        /// <param name="HuId"></param>
        /// <returns></returns>
        bool IfPltHaveStock(string WhCode, string HuId);

        /// <summary>
        /// 获取托盘长款高
        /// </summary>
        /// <param name="WhCode"></param>
        /// <param name="HuId"></param>
        /// <returns></returns>
        HuMasterResult GetPltLWH(string WhCode, string HuId);

        /// <summary>
        /// 修改托盘长款高
        /// </summary>
        string SetPltLWH(HuMasterResult huMasterResult, string User);
        /// <summary>
        /// 获取托盘的库位
        /// </summary>
        /// <param name="WhCode"></param>
        /// <param name="HuId"></param>
        /// <returns>没有话返回null</returns>
        string GetPltLocation(string WhCode, string HuId);
        /// <summary>
        /// 获取托盘上架建议内容
        /// </summary>
        /// <param name="WhCode"></param>
        /// <param name="HuId"></param>
        /// <returns></returns>
        string GetRecStockSug(string WhCode, string HuId);
        /// <summary>
        /// 验证当前托盘是否可以移货
        /// </summary>
        /// <param name="WhCode"></param>
        /// <param name="HuId"></param>
        /// <returns></returns>
        string PltItemEnableMove(string WhCode, string HuId);
        /// <summary>
        /// 验证目标托盘是否可以接受移货
        /// </summary>
        /// <param name="WhCode"></param>
        /// <param name="DestHuId">目标托盘</param>
        /// <param name="DestLoc">目标库位</param>
        /// <returns></returns>
        string DestPltItemEnableMove(string WhCode, string DestHuId,string DestLoc);
        List<HuDetailResult> PltItemMoveList(string WhCode, string HuId);
        List<HuDetailResult> PltItemMoveList(string WhCode, string HuId,string SKU);
        List<HuDetailResult>  LocationHuList(string WhCode, string LocationId);

        string SkuToItemId(string WhCode, string AltItemNumber);

        List<HuDetailResult> LocationHuList(string WhCode, string LocationId, string Sku);
        string SNSearch(string WhCode, string SN);

        List<ItemZone> GetItemZone(string WhCode, string AltItemNumber);
 

        List<ItemZone> GetZoneLoc(string WhCode, string ZoneName);
        List<ItemZone> GetItemLoc(string WhCode, string AltItemNumber);
        /// <summary>
        /// 移库操作
        /// </summary>
        /// <param name="WhCode"></param>
        /// <param name="HuId"></param>
        /// <param name="DestHuId"></param>
        /// <param name="HuDetailId"></param>
        /// <param name="MoveQty"></param>
        /// <param name="User"></param>
        /// <returns></returns>
        string PltItemMoveAction(string WhCode, string HuId, string DestHuId,string DestLoc, int HuDetailId, int MoveQty, string User);

        /// <summary>
        /// 获取待补货任务
        /// </summary>
        /// <param name="WhCode"></param>
        /// <returns></returns>
        List<SupplementTaskCe> SupplementTaskCe(string WhCode, string[] NotStatusArry);
        /// <summary>
        /// 补货任务明细
        /// </summary>
        /// <param name="SupplementNumber"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        List<SupplementTaskDetailCe> SupplementTaskDetailCe(string WhCode,string SupplementNumber, string SupplementGroupNumber);

        /// <summary>
        /// 补货下架
        /// </summary>
        /// <param name="SupplementNumber"></param>
        /// <param name="SupplementGroupNumber"></param>
        /// <param name="HuId"></param>
        /// <param name="user"></param>
        /// <param name="WhCode"></param>
        /// <returns></returns>
        string SupplementTaskDown(string SupplementNumber, string SupplementGroupNumber, string HuId, string User, string WhCode);

        string SupplementTaskUp(string SupplementNumber, string SupplementGroupNumber, string PutLocationId, string User, string WhCode);
       

        #region 3.0 盘点
        /// <summary>
        /// 检测盘点任务是否存在
        /// </summary>
        /// <param name="WhCode"></param>
        /// <param name="TaskNumber"></param>
        /// <returns>Y或错误结果</returns>
        string IfCycleTaskNumber(string WhCode, string TaskNumber);

        /// <summary>
        /// 获取是否逐件盘点的标记
        /// </summary>
        /// <param name="WhCode"></param>
        /// <param name="TaskNumber"></param>
        /// <returns></returns>
        int CycleTaskNumberOneByOneScanFlag(string WhCode, string TaskNumber);

        /// <summary>
        /// 获取盘点任务建议库位,按照库位名顺序排序
        /// </summary>
        /// <param name="WhCode"></param>
        /// <param name="TaskNumber"></param>
        /// <returns>没有的话返回null</returns>
        string GetCTSugLoc(string WhCode, string TaskNumber);

        /// <summary>
        /// 获取盘点任务剩余库位数
        /// </summary>
        /// <param name="WhCode"></param>
        /// <param name="TaskNumber"></param>
        /// <returns>没有的话返回0</returns>
        int GetLocRemainingQty(string WhCode, string TaskNumber);
        /// <summary>
        /// 检测托盘是否在盘点任务中
        /// </summary>
        /// <param name="WhCode"></param>
        /// <param name="TaskNumber"></param>
        /// <param name="LocationId"></param>
        /// <returns>Y或异常信息</returns>
        string IfLocInCycleTaskNumber(string WhCode, string TaskNumber, string LocationId);
        /// <summary>
        /// 盘点插入
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        string CycleCountInsert(CycleCountInsertComplex entity);
        /// <summary>
        /// 盘点插入
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        string CycleCountInsert(CycleCountInsertComplexAddPo entity);

        /// <summary>
        /// 盘点确认
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        string CheckCycleResult(CycleCountInsertComplexAddPo entity);
        /// <summary>
        /// 盘点EAN转成SKU
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        CycleCountInsertComplexAddPo CycleEANChangeToSKU(CycleCountInsertComplexAddPo entity);

        /// <summary>
        /// 盘点完成
        /// </summary>
        string CycleCountComplete(string WhCode, string TaskNumber, string User);
        #endregion
    }

}

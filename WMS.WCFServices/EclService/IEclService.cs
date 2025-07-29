using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using WMS.BLLClass;
using MODEL_MSSQL;

namespace WMS.WCFServices.EclService
{
    // 注意: 使用“重构”菜单上的“重命名”命令，可以同时更改代码和配置文件中的接口名“IEclService”。
    [ServiceContract]
    public interface IEclService
    {
        #region 1.入库订单导入

        [OperationContract]
        string InBoundOrderListAddEcl(InBoundOrderInsert entity);

        [OperationContract]
        string InBoundOrderListAddOms(InBoundOrderInsert entity);

        [OperationContract]
        string InBoundOrderListAddOmsSSID(InBoundOrderInsert entity);
        [OperationContract]
        //获取收货数据
        List<EclRecModel> GetRecInfoOms(String Receipt);
        #endregion

        #region 1.出库Load导入

        [OperationContract]

        string OutBoundLoadAddEcl(EclLoadModel entity);

        #endregion

        #region OMS出库订单管理
        [OperationContract]
        //出库订单导入WMS
        string OutBoundOrderAddOMS(EclOutOrderModel entity);

        [OperationContract]
        string BoschOutBoundOrderAddOMS(EclOutOrderModel entity);

        [OperationContract]
        string OutBoundOrderInterceptOMS(string whCode, string customerOutPoNumber, string clientCode, string userName);

        [OperationContract]
        string GetBoschPackEntity(BoschPackEntity entity);

        [OperationContract]
        //获取DM 包装实体
        string GetDMPackEntity(DMPackEntity entity);

        [OperationContract]
        //获取出库订单Json 实体
        string GetPackTaskJsonEntity(PackTaskJsonEntity entity);

        #endregion

        #region LOAD释放控制

        [OperationContract]
        //撤销释放
        string RollbackLoad(string loadId, string whCode, string userName);

        [OperationContract]
        //释放Load
        string CheckReleaseLoad(string loadId, string whCode, string userName);
        #endregion

        #region 款号维护

        [OperationContract]
        string ItemMasterAddOms(List<ItemMaster> iml);

        [OperationContract]
        string ItemMasterUpdateOms(ItemMaster im);
        #endregion

        #region  获取交接信息
        [OperationContract]
        TransferTaskResultEcl GetTransferTaskEclResult(string whCode, string transferId);
        #endregion

        #region 修改库存Lot

        [OperationContract]
        //调整库存Lot
        string EditHuIdLot(EditHuDetailLotEntity entity, string locationId, int qty, string lot1, string lot2, string lotdate);

        [OperationContract]
        //调整库存Lot
        List<EditHuDetailLotEntity> EditHuIdLot1(List<EditHuDetailLotEntity> entityList);

        #endregion

        [OperationContract]
        //批量导入OMS订单
        List<EclOutOrderModelResult> OutBoundOrderAddOMSBatch(List<EclOutOrderModel> entityList);

        [OperationContract]
        //手动获取快递单
        string GetExpressNumber(string express_code, SFExpressModel sfModel, YTExpressModel ytModel, string whCode, ZTOExpressModel ztModel);

        //批量导入预录入
        [OperationContract]
        string ImportsInBoundOrder(List<InBoundOrderInsert> entityList);


        #region  5.WMS工作台网页版

        //用户登录
        [OperationContract]
        WhUser LoginIn(WhUser whUser);

        [OperationContract]
        //通过公司ID 获取该公司的仓库数组
        List<WhInfoResult> WhInfoList(int companyId, string userName);

        #endregion


    }
}

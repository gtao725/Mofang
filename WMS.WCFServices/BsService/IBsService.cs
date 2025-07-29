using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using WMS.BLLClass;
using MODEL_MSSQL;

namespace WMS.WCFServices.BsService
{
    // 注意: 使用“重构”菜单上的“重命名”命令，可以同时更改代码和配置文件中的接口名“IBsService”。
    [ServiceContract]
    public interface IBsService
    {

        #region 1.入库订单导入

        [OperationContract]
        string InBoundOrderListAddBs(InBoundOrderInsert entity);

        #endregion

        #region 6.删除入库订单
        [OperationContract]
        string DeleteInorderBySO(string SoNumber, string WhCode, string ClientCode);
        #endregion


        #region 2.保税区EIP收货登记
        [OperationContract]
        string AddReceiptBs(string[] SO, string ClientCode, string LocationId, string WhCode, string User);
        #endregion

        #region 出库load导入
        [OperationContract]
        string OutBoundLoadAddBs(BsLoadModel entity);
        #endregion

        #region 维护集装箱扩展信息
        [OperationContract]
        string AddOutBoundContainer(LoadContainerExtend entity);
        #endregion

        #region 检查订单是否在WMS系统存在
        [OperationContract]
        string CheckOutBound(string CustomerOutPoNumber);
        #endregion
    }
}

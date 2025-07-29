using MODEL_MSSQL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using WMS.BLLClass;

namespace WMS.WCFServices.EipCfsService
{
    // 注意: 使用“重构”菜单上的“重命名”命令，可以同时更改代码和配置文件中的接口名“IService1”。
    [ServiceContract]
    public interface ICfsService
    {
        [OperationContract]
        string LoadContainerExtendAdd(LoadContainerExtend entity);

        [OperationContract]
        string EipInsertInBound(List<InBoundOrderInsert> entity);

        [OperationContract]
        string DelReceiptRegister(ReceiptRegister entity);

        [OperationContract]
        string LoadMasterDel(String LoadId, string Whcode, string User);
        [OperationContract]
        string OutBoundLoadAdd(BsLoadModel entity);



        #region 9.预约单批量导入预录入 同时生成收货操作单

        [OperationContract]
        string ImportsInBoundOrderAndReceiptByOrder(List<InBoundOrderInsert> entityList);


        #endregion

        #region 10.预录入批量导入
        [OperationContract]
        string ImportsInBoundOrder(List<ImportsInBoundOrderInsert> entity);

        #endregion

        #region 11.SO是否登记
        [OperationContract]
        string CheckRegInBoundSo(string SoNumber, string Whcode, string ClientCode);

        #endregion


        //重新生成收货费用
        [OperationContract]
        string DelReceiptCharge(string ReceiptId, string WhCode, string CreateUser);

        [OperationContract]
        //得到实际操作费用列表 
        List<FeeDetailResult1> getOperationFeeList(string feeNumber, string whCode, out int total);


        [OperationContract]
        //更新收货批次联单数
        string UpdateRegReceiptBillCount(ReceiptRegister entity);


        [OperationContract]
        //导入GWI创建GRN基础数据表
        string ImportsGWI(List<GwiDetailInsert> entity);


        [OperationContract]
        //计算箱单费用
        string AgainLoadCharge(string whCode, string loadId, string userName);


        [OperationContract]
        //确认箱单费用
        string LoadChargeEdit(string whCode, string loadId);


    }
}


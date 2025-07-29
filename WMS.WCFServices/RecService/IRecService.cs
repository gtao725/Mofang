using MODEL_MSSQL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using WMS.BLLClass;

namespace WMS.WCFServices.RecService
{
    // 注意: 使用“重构”菜单上的“重命名”命令，可以同时更改代码和配置文件中的接口名“IRecService”。
    [ServiceContract]
    public interface IRecService
    {
        [OperationContract]
        //收货信息更改 查询
        List<ReceiptResult> C_RecQuestionList(ReceiptSearch searchEntity, out int total,out string str);

        [OperationContract]
        List<ReceiptDetailCompleteResult> C_RecCompleteList(ReceiptSearch searchEntity, out int total, out string str);

        [OperationContract]
        //批量修改收货TCR异常原因 并冻结库存、修改TCR报表
        string FrozenEditList(string[] idArr, string whCode, string holdReason, string userName);

        [OperationContract]
        //批量清除收货TCR异常原因
        string FrozenDeleteList(string[] idArr, string whCode, string userName);

        [OperationContract]
        //批量修改收货分票
        string LotFlagEditList(string[] idArr, int lotFlag, string whCode, string userName);

        [OperationContract]
        //修改收货车型
        string ReceiptEditRegTransportType(string receiptId, string whCode, string transportType, string transportTypeExtend, string userName);

        [OperationContract]
        //修改收货信息
        string ReceiptEdit(Receipt entity);

        [OperationContract]
        //收货修改客户定制
        string ReceiptEditDetailCustom(Receipt entity);

        [OperationContract]
        //收货异常原因列表
        List<HoldMaster> HoldMasterListByRec(HoldMasterSearch searchEntity, out int total);


        [OperationContract]
        //插入收货信息
        string ReceiptInsert(ReceiptInsert entity);


        [OperationContract]
        //插入收货信息
        List<EclRecModel> GetRecInfoEcl(String Receipt);

        [OperationContract]
        //检查是否完成收货  返回Y 是全部完成  返回N是部分收货
        string CheckRecComplete1(string ReceiptId, string WhCode, string userName);

        [OperationContract]
        //分票类型下拉菜单列表
        IEnumerable<LookUp> LotFlagListSelect();

        [OperationContract]
        //Grn头查询
        List<GrnHeadResult> GrnHeadList(GrnHeadSearch search, out int total);

        //grnso明细
        [OperationContract]
        List<DamcoGRNDetail> GrnSOList(GrnHeadSearch search, out int total);

        [OperationContract]
        string UpdateGrnWmsData(string sonumber, string clientcode, string whcode, string User);

        [OperationContract]
        string GrnAutoUpdate(string sonumber, string Whcode, string ClientCode);

        [OperationContract]
        string SendGRN(string sonumber, string Whcode, string ClientCode, string user);
        [OperationContract]
        string AutoSendGRN(string receiptid, string Whcode, string user);

        [OperationContract]
        string UpdateGrnDetail(int detailId, DateTime? GRN_ReceiptDate, int GRN_Qty, double? GRN_Cbm, double? GRN_Kgs);


        #region 1.收货耗材科目
        [OperationContract]
        List<RecLossType> RecLossTypeList(RecLossTypeSearch searchEntity, out int total);

        [OperationContract]
        RecLossType RecLossTypeAdd(RecLossType entity);

        [OperationContract]
        string RecLossTypeEdit(RecLossType entity);

        #endregion


        #region 2.收货耗材

        [OperationContract]
        List<RecLossResult> RecLossList(RecLossSearch searchEntity, out int total);

        [OperationContract]
        RecLoss RecLossAdd(RecLoss entity);

        [OperationContract]
        string RecLossEdit(RecLoss entity);

        [OperationContract]
        //收货耗材科目下拉菜单列表
        IEnumerable<RecLossType> RecLossTypeListSelect(string whCode);


        #endregion

        [OperationContract]
        string getFileId(string userId);

    }
}

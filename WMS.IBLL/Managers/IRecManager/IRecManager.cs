using MODEL_MSSQL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMS.BLLClass;

namespace WMS.IBLL
{
    public interface IRecManager
    {
        //收货信息更改 查询
        List<ReceiptResult> C_RecQuestionList(ReceiptSearch searchEntity, out int total, out string str);

        List<ReceiptDetailCompleteResult> C_RecCompleteList(ReceiptSearch searchEntity, out int total, out string str);

        //批量修改收货TCR异常原因 并冻结库存、修改TCR报表
        string FrozenEditList(string[] idArr, string whCode, string holdReason, string userName);

        //批量清除收货TCR异常原因
        string FrozenDeleteList(string[] idArr, string whCode, string userName);

        //批量修改收货分票
        string LotFlagEditList(string[] idArr, int lotFlag, string whCode, string userName);

        //修改收货信息
        string ReceiptEdit(Receipt entity);

        //修改收货车型
        string ReceiptEditRegTransportType(string receiptId, string whCode, string transportType, string transportTypeExtend, string userName);

        //收货修改客户定制
        string ReceiptEditDetailCustom(Receipt entity);

        //收货异常原因列表
        List<HoldMaster> HoldMasterListByRec(HoldMasterSearch searchEntity, out int total);

        //插入收货信息
        string ReceiptInsert(ReceiptInsert entity);

        //插入收货信息
        string ReceiptInsert(List<ReceiptInsert> entityList);

        string CheckRecComplete(string ReceiptId, string WhCode);
        string PartRecComplete(string ReceiptId, string WhCode, string CreateUser);
        string RecComplete(string ReceiptId, string WhCode, string CreateUser);

        string RecCheckOverWeightList(List<ReceiptInsert> entity);
        
        string PauseRec(string ReceiptId, string WhCode, string CreateUser);


        //检查是否完成收货  返回Y 是全部完成  返回N是部分收货
        string CheckRecComplete1(string ReceiptId, string WhCode, string userName);

        //获取收货数据
        List<EclRecModel> GetRecInfoEcl(string ReceiptId);


        //拦截订单快捷收货,type 0代表单品，其余为多品
        string ReceiptByOutOrderIntercept(ReceiptInsert entity, int type);

        //删除收货费用后再次获取费用
        string DelReceiptCharge(string ReceiptId, string WhCode, string CreateUser);


        //分票类型下拉菜单列表
        IEnumerable<LookUp> LotFlagListSelect();


        #region 1.收货耗材科目

        List<RecLossType> RecLossTypeList(RecLossTypeSearch searchEntity, out int total);

        RecLossType RecLossTypeAdd(RecLossType entity);

        string RecLossTypeEdit(RecLossType entity);

        #endregion

        #region 2.收货耗材

        List<RecLossResult> RecLossList(RecLossSearch searchEntity, out int total);

        RecLoss RecLossAdd(RecLoss entity);

        string RecLossEdit(RecLoss entity);

        //收货耗材科目下拉菜单列表
        IEnumerable<RecLossType> RecLossTypeListSelect(string whCode);

        //获取photoId
        string getFileId(string userId);

        #endregion

    }
}

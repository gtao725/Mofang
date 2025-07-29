using MODEL_MSSQL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMS.BLLClass;

namespace WMS.IBLL
{
    public interface IRecWinceManager
    {


        //查询收货批次的基本资料
        ReceiptInsert GetReceipt(string ReceiptId, string WhCode);
        bool ReceiptDSCheck(string ReceiptId, string WhCode, string RecType);
        string GetRecDes(string ReceiptId, string WhCode);
        string RecConsumerCreate(string WhCode, string ReceiptId, int RecLossId, int Qty, string CreateUser);
        string RecCreateSkuBatch(ReceiptInsert rec, string eFlag);
        string ReceiptFastCheck(string ReceiptId, string WhCode);

        bool CheckRecLocation(string WhCode, string Location);
        bool CheckReturnLocation(string WhCode, string Location);
        bool CheckRecLocation(string WhCode, string Location, string ReceiptId);
        bool CheckSoPo(string ReceiptId, string WhCode, string SoNumber, string CustomerPoNumber);

        bool CheckPlt(string WhCode, string HuId);

        bool CheckSku(string ReceiptId, string WhCode, int ItemId, string CustomerPoNumber);
        IQueryable<RecSkuUnit> GetUnit(string ReceiptId, string WhCode, int ItemId);
        IQueryable<RecSkuUnit> GetUnitChange(string ReceiptId, string WhCode, int ItemId);
        string GetSkuAltItemNumber(int ItemId);
        List<HuDetailRemained> RecScanRemainedPlt(string WhCode, string HuId);
        List<HuDetailRemained> RecScanUPCPlt(string WhCode, string HuId);

        string RecScanRemainedComplete(HuDetailRemained huDetailRemained);


        string RecUPCComplete(HuDetailRemained huDetailRemained);


        string RecScanCheck(HuDetailRemained huDetailRemained);
        string RecUPCCheck(HuDetailRemained huDetailRemained);

        string RecScanCheck(List<SerialNumberInModel> serialNumberInModelList, int checkPartFlag, string ClientCode, string SoNumber, string CustomerPoNumber, string AltItemNumber, string WhCode);
        RecSkuDataCe GetRecSkuDataCe(int ItemId);
        List<RecSkuDataCe> GetRecSkuDataCeList(string ReceiptId, string SoNumber, string CustomerPoNumber, string WhCode);
        List<string> GetRecReturnOrder(string RecReturnOrderNumber, string WhCode);
        List<RecSoModel> GetRecSoList(string WhCode, string ReceiptId, string SoNumber);

        int GetSkuRecQty(string ReceiptId, string WhCode, List<int> ItemIdArry, string SoNumber, string CustomerPoNumber);
        int GetSkuRegQty(string ReceiptId, string WhCode, List<int> ItemIdArry, string SoNumber, string CustomerPoNumber);
        RecSkuDataCe GetSkuRegCBMWeight(string ReceiptId, string WhCode, int ItemId, string SoNumber, string CustomerPoNumber);

        //任意SKU转换成itemId
        string RecItemNumberToId(string ItemNumber, string WhCode, string ReceiptId, string CustomerPoNumber, string SoNumber);

        List<int> RecItemNumberToIds(string ItemNumber, string WhCode, string ReceiptId, string CustomerPoNumber, string SoNumber);
        string CheckRecCBMPercent(string ReceiptId, string WhCode, int Percent);
        string CheckRecCBMPercent(string ReceiptId, string WhCode);
        List<RecLotFlagDescription> GetRecLotFlag();

        List<UserPriceT> UserPriceT();


        List<UserShenT> UserShenT();

        List<RecConsumerGoodsModel> GetRecConsumerGoodsModelList(string WhCode);
        List<RecConsumerGoodsModel> GetRecConsumerGoodsModelList(string ReceiptId, string WhCode);

        string RecConsumerDelete(string ReceiptId, string WhCode, string userName);

        string RecReMarkIn(string ReceiptId, string WhCode, string userName, string recRemark);
        string RecEIAssign(string ReceiptId, string WhCode, string userName);

        string DcReturnExceptionInsert(DcReturnExceptionIn dcReturnExceptionIn);
        string EANGetItem(string WhCode, string EAN);


        List<WorkloadAccountModelCN> GetRecWorkloadAccountModelList(string ReceiptId, string WhCode);
        List<WorkloadAccountModelCN> GetOutWorkloadAccountModelList(string LoadId, string WhCode);

    }
}

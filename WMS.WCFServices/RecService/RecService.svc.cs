using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using MODEL_MSSQL;
using WMS.BLLClass;

namespace WMS.WCFServices.RecService
{
    // 注意: 使用“重构”菜单上的“重命名”命令，可以同时更改代码、svc 和配置文件中的类名“RecService”。
    // 注意: 为了启动 WCF 测试客户端以测试此服务，请在解决方案资源管理器中选择 RecService.svc 或 RecService.svc.cs，然后开始调试。
    public class RecService : IRecService
    {
        IBLL.IRecManager rec = new BLL.RecManager();
        IBLL.IGrnWeb grnweb = new BLL.GrnWeb();
        IBLL.IGrn grn = new BLL.Grn();
        public List<ReceiptResult> C_RecQuestionList(ReceiptSearch searchEntity, out int total, out string str)
        {
            return rec.C_RecQuestionList(searchEntity, out total, out str);
        }

        public List<ReceiptDetailCompleteResult> C_RecCompleteList(ReceiptSearch searchEntity, out int total, out string str)
        {
            return rec.C_RecCompleteList(searchEntity, out total, out str);
        }

        //批量修改收货TCR异常原因 并冻结库存、修改TCR报表
        public string FrozenEditList(string[] idArr, string whCode, string holdReason, string userName)
        {
            return rec.FrozenEditList(idArr, whCode, holdReason, userName);
        }

        //批量清除收货TCR异常原因
        public string FrozenDeleteList(string[] idArr, string whCode, string userName)
        {
            return rec.FrozenDeleteList(idArr, whCode, userName);
        }

        //批量修改收货分票
        public string LotFlagEditList(string[] idArr, int lotFlag, string whCode, string userName)
        {
            return rec.LotFlagEditList(idArr, lotFlag, whCode, userName);
        }

        //修改收货车型
        public string ReceiptEditRegTransportType(string receiptId, string whCode, string transportType, string transportTypeExtend, string userName)
        {
            return rec.ReceiptEditRegTransportType(receiptId, whCode, transportType, transportTypeExtend, userName);
        }

        public string ReceiptEdit(Receipt entity)
        {
            return rec.ReceiptEdit(entity);
        }

        //收货修改客户定制
        public string ReceiptEditDetailCustom(Receipt entity)
        {
            return rec.ReceiptEditDetailCustom(entity);
        }

        //收货异常原因列表
        public List<HoldMaster> HoldMasterListByRec(HoldMasterSearch searchEntity, out int total)
        {
            return rec.HoldMasterListByRec(searchEntity, out total);
        }

        //插入收货信息
        public string ReceiptInsert(ReceiptInsert entity)
        {
            return rec.ReceiptInsert(entity);
        }

        //电商获取收货数据
        public List<EclRecModel> GetRecInfoEcl(String Receipt)
        {

            return rec.GetRecInfoEcl(Receipt);
        }

        //检查是否完成收货  返回Y 是全部完成  返回N是部分收货
        public string CheckRecComplete1(string ReceiptId, string WhCode, string userName)
        {
            return rec.CheckRecComplete1(ReceiptId, WhCode, userName);
        }

        //分票类型下拉菜单列表
        public IEnumerable<LookUp> LotFlagListSelect()
        {
            return rec.LotFlagListSelect();
        }

        //grn头查询
        public List<GrnHeadResult> GrnHeadList(GrnHeadSearch search, out int total)
        {
            return grnweb.GrnHeadList(search, out total);
        }

        //grnso查询
        public List<DamcoGRNDetail> GrnSOList(GrnHeadSearch search, out int total)
        {
            return grnweb.GrnSOList(search, out total);
        }

        public string UpdateGrnWmsData(string sonumber, string clientcode, string whcode, string User)
        {

            return grn.UpdateGrnWmsData(sonumber, clientcode, whcode, User);
        }

        public string GrnAutoUpdate(string sonumber, string Whcode, string ClientCode)
        {

            return grn.GrnAutoUpdate(sonumber, Whcode, ClientCode);
        }

        public string SendGRN(string sonumber, string Whcode, string ClientCode, string user)
        {
            return grn.SendGRN(sonumber, Whcode, ClientCode, user);
        }

        public string AutoSendGRN(string receiptid, string Whcode, string user)
        {
            return grn.AutoSendGRN(receiptid, Whcode, user);
        }

        public string UpdateGrnDetail(int detailId, DateTime? GRN_ReceiptDate, int GRN_Qty, double? GRN_Cbm, double? GRN_Kgs)
        {
            return grn.UpdateGrnDetail(detailId, GRN_ReceiptDate, GRN_Qty, GRN_Cbm, GRN_Kgs);
        }


        #region 1.收货耗材科目

        public List<RecLossType> RecLossTypeList(RecLossTypeSearch searchEntity, out int total)
        {
            return rec.RecLossTypeList(searchEntity, out total);
        }

        public RecLossType RecLossTypeAdd(RecLossType entity)
        {
            return rec.RecLossTypeAdd(entity);
        }

        public string RecLossTypeEdit(RecLossType entity)
        {
            return rec.RecLossTypeEdit(entity);
        }

        #endregion

        #region 2.收货耗材

        public List<RecLossResult> RecLossList(RecLossSearch searchEntity, out int total)
        {
            return rec.RecLossList(searchEntity, out total);
        }

        public RecLoss RecLossAdd(RecLoss entity)
        {
            return rec.RecLossAdd(entity);
        }

        public string RecLossEdit(RecLoss entity)
        {
            return rec.RecLossEdit(entity);
        }

        //收货耗材科目下拉菜单列表
        public IEnumerable<RecLossType> RecLossTypeListSelect(string whCode)
        {
            return rec.RecLossTypeListSelect(whCode);
        }

        #endregion


        //收货耗材科目下拉菜单列表
        public string  getFileId(string userId)
        {
            return rec.getFileId(userId);
        }

    }
}

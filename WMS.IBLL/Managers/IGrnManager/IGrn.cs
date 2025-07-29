using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMS.BLLClass;
using MODEL_MSSQL;

namespace WMS.IBLL
{
    public interface IGrn
    {
        string CheckSoLN(String sonumber);

        List<GrnSoUpdateSearch> GetReceipSo(String receipid);
 
        string UpdateGrnWmsData(string sonumber, string clientcode, string whcode, string User);

        string SetGrn(string receiptid);

        string GrnAutoUpdate(string sonumber,string Whcode,string ClientCode );

        string GrnAutoUpdateCbm(string sonumber, DamcoGrnRule rule, List<DamcoGRNDetail> dl);

        string GrnAutoUpdateKgs(string sonumber, DamcoGrnRule rule, List<DamcoGRNDetail> dl);

        string GrnAutoUpdateReceiptDate(string sonumber, DamcoGrnRule rule, List<DamcoGRNDetail> dl);

        string SendGRN(string sonumber, string Whcode, string ClientCode, string user);

        string AutoSendGRN(string receiptid, string Whcode, string user);
        string UpdateGrnDetail(int detailId, DateTime? GRN_ReceiptDate, int GRN_Qty, double? GRN_Cbm, double? GRN_Kgs);

        String UpdateGrnHeadRemark(int HeadId, string remark);

    }
}

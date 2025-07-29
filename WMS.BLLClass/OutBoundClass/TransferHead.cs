using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.BLLClass
{
    public class TransferHeadResult
    {
        public string ExpressNumber { get; set; }
        public string CreateUser { get; set; }
        public DateTime? CreateDate { get; set; }
        public int? Id { get; set; }

        public int? TransferTaskId { get; set; }
        public string CustomerOutPoNumber { get; set; }
        public string PackNumber { get; set; }
    }


    public class TransferTaskResult
    {
        public int Id { get; set; }

        public string ExpressNumber { get; set; }
        public string Status0 { get; set; }
        public string TransferId { get; set; }
        public string TransferNumber { get; set; }
        public string express_code { get; set; }
        public string express_type_zh { get; set; }
        public string Status1 { get; set; }
        public string CreateUser { get; set; }
        public DateTime? CreateDate { get; set; }

        public string UpdateUser { get; set; }
        public DateTime? UpdateDate { get; set; }
        public int? ExpressCount { get; set; }
        public int? SumQty { get; set; }
    }

    public class TransferHeadDetailSearch : BaseSearch
    {
        public int Id { get; set; }
        public string ExpressNumber { get; set; }
        public string LoadId { get; set; }
        public string AltItemNumber { get; set; }
        public string PackCarton { get; set; }
    }
    public class TransferHeadDetailResult
    {
        public int Id { get; set; }

        public string TransferNumber { get; set; }
        public string LoadId { get; set; }
        public string SortGroupNumber { get; set; }
        public string CustomerOutPoNumber { get; set; }
        public string OutPoNumber { get; set; }
        public string PackGroupNumber { get; set; }
        public string ExpressNumber { get; set; }
        public string AltItemNumber { get; set; }
        public int Qty { get; set; }
        public decimal? Length { get; set; }
        public decimal? Width { get; set; }
        public decimal? Height { get; set; }
        public decimal? Weight { get; set; }
        public string PackCarton { get; set; }

        public string Status1 { get; set; }
        public string CreateUser { get; set; }
        public DateTime? CreateDate { get; set; }

        public string WhCode { get; set; }
        public int? SortGroupId { get; set; }
        public int? ItemId { get; set; }
        public int? SumQty { get; set; }
    }


    public class TransferTaskSearch : BaseSearch
    {
        public string WhCode { get; set; }
        public string TransferId { get; set; }
        public string TransferNumber { get; set; }
        public string ExpressNumber { get; set; }
        public string AltItemNumber { get; set; }
        public string LoadId { get; set; }
        public string CreateUser { get; set; }
        public DateTime? CreateDateBegin { get; set; }
        public DateTime? CreateDateEnd { get; set; }
    }


    public class TransferTaskResultEcl
    {
        public string WhCode { get; set; }
        public string TransferId { get; set; }
        public string TransferNumber { get; set; }
        public string TransportType { get; set; }
        public string express_code { get; set; }

        public string express_type { get; set; }
        public string express_type_zh { get; set; }
        public int? Status { get; set; }
        public string CreateUser { get; set; }
        public DateTime? CreateDate { get; set; }

        public List<TransferHeadResultEcl> transferHeadResultEcl;
    }

    public class TransferHeadResultEcl
    {
        public string WhCode { get; set; }
        public int? TransferTaskId { get; set; }
        public string LoadId { get; set; }
        public int? SortGroupId { get; set; }
        public string SortGroupNumber { get; set; }
        public string CustomerOutPoNumber { get; set; }
        public string OutPoNumber { get; set; }
        public int? PackGroupId { get; set; }
        public string PackGroupNumber { get; set; }
        public string ExpressNumber { get; set; }
        public string ExpressNumberParent { get; set; } //add by yangxin 2024-05-28 母单号
        public decimal? Length { get; set; }
        public decimal? Width { get; set; }
        public decimal? Height { get; set; }
        public decimal? Weight { get; set; }
        public string PackCarton { get; set; }

        public List<TransferDetailResultEcl> transferDetailResultEcl;

        public List<TransferScanNumberResultEcl> transferScanNumberResultEcl;
    }

    public class TransferDetailResultEcl
    {
        public string WhCode { get; set; }
        public int? TransferHeadId { get; set; }
        public int? ItemId { get; set; }
        public string AltItemNumber { get; set; }
        public int PlanQty { get; set; }
        public int Qty { get; set; }
    }


    public class TransferScanNumberResultEcl
    {
        public string WhCode { get; set; }
        public int? TransferHeadId { get; set; }
        public int? ItemId { get; set; }
        public string AltItemNumber { get; set; }
        public string ScanNumber { get; set; }
    }



    public class TransferReportResult
    {
        public string TransferId { get; set; }
        public string TransportType { get; set; }
        public string express_code { get; set; }

        public string LoadId { get; set; }
        public string CustomerOutPoNumber { get; set; }
        public string PackNumber { get; set; }
        public string ExpressNumber { get; set; }
        public decimal? Weight { get; set; }
        public int? Qty { get; set; }
        public int? PackQty { get; set; }
        public string TransferUserName { get; set; }
    }
}

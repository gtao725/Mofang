using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.BLLClass
{
    public class OutBoundOrderResult
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public string ClientCode { get; set; }
        public string OutPoNumber { get; set; }
        public string CustomerOutPoNumber { get; set; }
        public string AltCustomerOutPoNumber { get; set; }
        public string ReceiptId { get; set; }
        public int ProcessId { get; set; }
        public int NowProcessId { get; set; }
        public string NowProcessName { get; set; }
        public int? StatusId { get; set; }
        public string StatusName { get; set; }
        public string OrderSource { get; set; }
        public DateTime? PlanOutTime { get; set; }
        public string CreateUser { get; set; }
        public DateTime? CreateDate { get; set; }
        public string FlowName { get; set; }
        public string StatusId1 { get; set; }

        public int? RollbackFlag { get; set; }
        public string LoadId { get; set; }

        public int? OutBoundOrderId { get; set; }
        public string DSShow { get; set; }
        public int SumQty { get; set; }

        public string buy_name { get; set; }
        public string buy_company { get; set; }
        public string address { get; set; }

        public string StowPosition { get; set; }
    }

    public class OutBoundOrderSearch : BaseSearch
    {
        public string WhCode { get; set; }
        public int? ClientId { get; set; }
        public string OutPoNumber { get; set; }
        public string CustomerOutPoNumber { get; set; }
        public string AltCustomerOutPoNumber { get; set; }
        public int ProcessId { get; set; }
        public int LoadMasterId { get; set; }
        public int? StatusId { get; set; }
        public string StatusName { get; set; }
        public DateTime? BeginCreateDate { get; set; }
        public DateTime? EndCreateDate { get; set; }

        public string ProcessName { get; set; }

        public string OrderSource { get; set; }

    }

    public class ImportOutBoundOrder
    {
        public int ProcessId { get; set; }
        public string ProcessName { get; set; }
        public string WhCode { get; set; }
        public string ClientCode { get; set; }
        public string CustomerOutPoNumber { get; set; }

        public List<OutBoundOrderDetailModel> OutBoundOrderDetailModel;
    }


    public class OutBoundOrderResult1
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public string ClientCode { get; set; }
        public string OutPoNumber { get; set; }
        public string CustomerOutPoNumber { get; set; }
        public string AltCustomerOutPoNumber { get; set; }
        public string ReceiptId { get; set; }
        public int ProcessId { get; set; }
        public int NowProcessId { get; set; }
        public string NowProcessName { get; set; }
        public int? StatusId { get; set; }
        public string StatusName { get; set; }
        public string OrderSource { get; set; }
        public DateTime? PlanOutTime { get; set; }
        public string CreateUser { get; set; }
        public DateTime? CreateDate { get; set; }
        public string FlowName { get; set; }
        public string StatusId1 { get; set; }

        public int? RollbackFlag { get; set; }
        public string LoadId { get; set; }

        public int? OutBoundOrderId { get; set; }
        public string DSShow { get; set; }
        public int SumQty { get; set; }

        public string buy_name { get; set; }
        public string buy_company { get; set; }
        public string address { get; set; }

        public string StowPosition { get; set; }
        public string AltItemNumber { get; set; }
        public int? StatusId2 { get; set; }
    }

    public class OutBoundOrderSearch1 : BaseSearch
    {
        public string WhCode { get; set; }
        public int? ClientId { get; set; }
        public string OutPoNumber { get; set; }
        public string CustomerOutPoNumber { get; set; }
        public string AltCustomerOutPoNumber { get; set; }
        public string AltItemNumber { get; set; }
        public int ProcessId { get; set; }
        public int LoadMasterId { get; set; }
        public int? StatusId { get; set; }
        public string StatusName { get; set; }
        public DateTime? BeginCreateDate { get; set; }
        public DateTime? EndCreateDate { get; set; }

        public string ProcessName { get; set; }

        public int? StatusId2 { get; set; }
    }
}

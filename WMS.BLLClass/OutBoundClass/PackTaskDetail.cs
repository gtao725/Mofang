using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.BLLClass
{
    public class PackTaskInsert
    {
        public string LoadId { get; set; }
        public string WhCode { get; set; }
        public string OutPoNumber { get; set; }
        public int GroupId { get; set; }
        public string GroupNumber { get; set; }
        public string PackNumber { get; set; }
        public string userName { get; set; }

        public List<PackDetailInsert> PackDetail;
    }

    public class PackDetailInsert
    {
        public int? ItemId { get; set; }
        public int? Qty { get; set; }
        public string AltItemNumber { get; set; }
        public string EAN { get; set; }
        public DateTime? CreateDate { get; set; }
        public string CartonName { get; set; }
        public decimal Weight { get; set; }

        public List<PackScanNumberInsert> PackScanNumber;
    }
    public class PackScanNumberInsert
    {
        public string ScanNumber { get; set; }

    }


    public class StaticLoadPackTaskDetail
    {
        public  string LoadId { get; set; }
        public string WhCode { get; set; }
        public string OutPoNumber { get; set; }
        public int GroupId { get; set; }
        public string GroupNumber { get; set; }
        public string PackNumber { get; set; }
        public int? ItemId { get; set; }
        public int packTaskId { get; set; }
    }
    public class StaticLoadPackCount
    {
        public string LoadId { get; set; }
        public string WhCode { get; set; }
        public int? ItemId { get; set; }
        public int CountRows { get; set; }
        public string status { get; set; }
    }

}

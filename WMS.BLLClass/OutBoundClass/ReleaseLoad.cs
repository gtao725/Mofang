using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.BLLClass
{
    public class ReleaseLoad
    {
        public string WhCode { get; set; }
        public string LoadId { get; set; }
        public int? ClientId { get; set; }
        public string ClientCode { get; set; }
        public int? OutBoundOrderId { get; set; }
        public string SoNumber { get; set; }
        public string CustomerPoNumber { get; set; }
        public string AltItemNumber { get; set; }
        public int ItemId { get; set; }
        public string UnitName { get; set; }
        public Nullable<int> UnitId { get; set; }
        public int? Qty { get; set; }
        public Nullable<decimal> Length { get; set; }
        public Nullable<decimal> Width { get; set; }
        public Nullable<decimal> Height { get; set; }
        public Nullable<decimal> Weight { get; set; }
        public string LotNumber1 { get; set; }
        public string LotNumber2 { get; set; }
        public Nullable<System.DateTime> LotDate { get; set; }
        public string UserName { get; set; }
        public int? Sequence { get; set; }
    }

    public class CheckReleaseLoadResult
    {
        public string Result { get; set; }

        public List<ReleaseLoad> ReleaseLoadList;

        public List<HuDetailResult> HuDetailResultList;

        public List<ReleaseLoad> ReleaseLoadList2;

        public List<HuDetailResult> HuDetailResultList2;

        public List<ReleaseLoad> ReleaseLoadList3;

        public List<HuDetailResult> HuDetailResultList3;

        public List<ReleaseLoad> ReleaseLoadList4;

        public List<HuDetailResult> HuDetailResultList4;
        public string CheckStatusResult { get; set; }
    }


    public class DSReleaseLoad
    {
        public string WhCode { get; set; }
        public string LoadId { get; set; }
        public int? ClientId { get; set; }
        public string ClientCode { get; set; }
        public int? OutBoundOrderId { get; set; }
        public string SoNumber { get; set; }
        public string CustomerPoNumber { get; set; }
        public string AltItemNumber { get; set; }
        public int ItemId { get; set; }
        public string UnitName { get; set; }
        public Nullable<int> UnitId { get; set; }
        public int? Qty { get; set; }
        public Nullable<decimal> Length { get; set; }
        public Nullable<decimal> Width { get; set; }
        public Nullable<decimal> Height { get; set; }
        public Nullable<decimal> Weight { get; set; }
        public string LotNumber1 { get; set; }
        public string LotNumber2 { get; set; }
        public Nullable<System.DateTime> LotDate { get; set; }
        public string UserName { get; set; }

    }

    public class PickTaskDetailPickingSequenceResult
    {
        public int Id { get; set; }
        public string WhCode { get; set; }
        public string LoadId { get; set; }
        public string HuId { get; set; }
        public string Location { get; set; }
        public string SoNumber { get; set; }
        public string CustomerPoNumber { get; set; }
        public string AltItemNumber { get; set; }
        public string Style1 { get; set; }
        public string Style2 { get; set; }
        public string Style3 { get; set; }
        public decimal? Length { get; set; }
        public decimal? Width { get; set; }
        public decimal? Height { get; set; }
        public string LotNumber1 { get; set; }
        public string LotNumber2 { get; set; }
        public DateTime? LotDate { get; set; }
        public int? Sequence { get; set; }

    }


}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.BLLClass
{
    public class CycleCountMasterSearch : BaseSearch
    {
        public string WhCode { get; set; }
        public string TaskNumber { get; set; }
        public int? CreateType { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public DateTime? BeginCreateDate { get; set; }
        public DateTime? EndCreateDate { get; set; }
    }

    public class CycleCountMasterResult
    {
        public int Id { get; set; }
        public string TaskNumber { get; set; }
        public string Type { get; set; }
        public int? CreateType { get; set; }
        public string TypeDescription { get; set; }
        public string Status { get; set; }
        public string Description { get; set; }
        public string CreateUser { get; set; }
        public DateTime? CreateDate { get; set; }
        public string LocationNullShow { get; set; }
        public string OneByOneScanShow { get; set; }

        public string CompareStorageLocationHuShow { get; set; }
    }

    public class CycleCountMasterInsert
    {
        public string TaskNumber { get; set; }
        public string Type { get; set; }
        public int? LocationNullFlag { get; set; }
        public int? OneByOneScanFlag { get; set; }
        public string Description { get; set; }

        public int? CreateType { get; set; }
        public string TypeDescription { get; set; }
        public string WhCode { get; set; }
        public string BeginLocationId { get; set; }
        public string EndLocationId { get; set; }
        public string Location { get; set; }
        public string LocationColumn { get; set; }
        public int? LocationRowBegin { get; set; }
        public int? LocationRowEnd { get; set; }
        public string CreateUser { get; set; }
        public DateTime? CreateDate { get; set; }

        public int? CompareStorageLocationHu { get; set; }
    }

    public class CycleCountMasterSeacrh
    {
        public DateTime? BeginCreateDate { get; set; }
        public DateTime? EndCreateDate { get; set; }
    }



    public class CycleCountInsertComplex
    {
        public string TaskNumber { get; set; }
        public string WhCode { get; set; }
        public string LocationId { get; set; }

        public List<HuIdModel> HuIdModel;

        public string CreateUser { get; set; }
    }

    public class HuIdModel
    {
        public string AltItemNumber { get; set; }
        public string HuId { get; set; }
        public int Qty { get; set; }
    }


    public class CycleCountInsertComplexAddPo
    {
        public string TaskNumber { get; set; }
        public string WhCode { get; set; }
        public string LocationId { get; set; }

        public List<HuIdModelAddPo> HuIdModelAddPo;

        public string CreateUser { get; set; }
    }

    public class HuIdModelAddPo
    {
        public string HuId { get; set; }

        public List<PoModel> PoModel;
    }

    public class PoModel
    {
        public string CustomerPoNumber { get; set; }

        public List<HuDetailModel> HuDetailModel;
    }

    public class HuDetailModel
    {
        public string AltItemNumber { get; set; }
        public int Qty { get; set; }

    }
}

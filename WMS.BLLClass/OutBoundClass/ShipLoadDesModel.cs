using System.Collections.Generic;

namespace WMS.BLLClass
{
    public class ShipLoadDesModel
    {
        public string LoadId { get; set; }
        public string WhCode { get; set; }
        //箱号
        public string ContainerNumber { get; set; }
        //箱型 95代码
        public string ContainerType { get; set; }
        //箱型名称
        public string ContainerName { get; set; }
        //封号
        public string SealNumber { get; set; }
        //流程ID
        public int ProcessId { get; set; }
        //Load总数
        public int LoadTotalQty { get; set; }
        //Load已装数量
        public int LoadQty { get; set; }
        //Load已装数量
        public int LoadCBM { get; set; }
        //Load建议托盘
        //  public string SugPlt { get; set; }

    }
 

    public class LoadPlt
    {
        public string LoadId { get; set; }
        public string WhCode { get; set; }
        //托盘
        public string HuId { get; set; }
        public string Position { get; set; }
        public string UserName { get; set; }
        public List<WorkloadAccountModel> WorkloadAccountModel { get; set; }
        public List<HuDetailRemained> HuDetailRemained { get; set; }
    }
}

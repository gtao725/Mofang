namespace WMS.BLLClass
{
    public class ShipPickDesModel
    {
        public string LoadId { get; set; }
        public string WhCode { get; set; }
        //流程ID
        public int ProcessId { get; set; }
        public string HuId { get; set; }
        //pick未备托盘数
        public int UnPickPltQty { get; set; }
        //pick未备箱数,多单位时候显示字符串
        public string UnPickQty { get; set; }
        //pick已备托盘数
        public int PickPltQty { get; set; }
        //pick已备数量,多单位时候显示字符串
        public string PickQty { get; set; }
        //Load建议托盘
        public string SugPlt { get; set; }
        //Load建议库位
        public string SugLocation { get; set; }
        //托盘总数量,多单位时候显示字符串
        public string PltQty { get; set; }
        //拆托标记 Y表示拆托 
        public string SplitFlag { get; set; }
        public string Location { get; set; }

    }

    public class ShipPickSplitDesModel
    {
        public string LoadId { get; set; }
        public string AltItemNumber { get; set; }
        public string UnitName { get; set; }
        public int Qty { get; set; }
        public int? SplitQty { get; set; }

    }

    public class ShipPickTaskDetailOrderByModel
    {
        public string HuId { get; set; }
        public string Location { get; set; }
        public string AltItemNumber { get; set; }
        public string SoNumber { get; set; }
        public string CustomerPoNumber { get; set; }
        public string Style1{ get; set; }
        public string Style2 { get; set; }
        public string Style3 { get; set; }

 

    }

}

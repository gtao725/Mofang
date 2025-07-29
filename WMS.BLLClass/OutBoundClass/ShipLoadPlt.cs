
namespace WMS.BLLClass
{
   public class ShipLoadPlt
    {
        public string LoadId { get; set; }
        public string WhCode { get; set; }
        public string UserName { get; set; }
        public string HuId { get; set; }
        public string PutHuId { get; set; }
        public string Location { get; set; }
        public int IfHavePutHuId { get; set; }

        public int IfSerialNumberChange { get; set; }
    }
}

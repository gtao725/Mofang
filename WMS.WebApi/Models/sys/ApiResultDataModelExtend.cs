
using System.Collections.Generic;
using System.Linq;
using WMS.BLLClass;

namespace WMS.WebApi.Models
{
    /// <summary>
    /// 这里放属性必须带{ get; set; } 不然反射不到属性会失败      
    /// </summary>
    public partial class ApiResultDataModel
    {

        public List<string> listStrModel { get; set; }
        public ReceiptInsert recModel { get; set; }
        public List<ReceiptInsert> recModelList { get; set; }
        public BusinessFlowDetailList BusinessFlowDetailList { get; set; }
        public OpenFormModel openFormModel { get; set; }
     //   public BusinessObjectsModel BusinessObject { get; set; }
        public UserModel userModel { get; set; }
       // public BusinessObjectHeadModel BusinessObjectHead { get; set; }
        public ApiRequestDataModel apiRequestDataModel { get; set; }
        public List<RecSkuUnit> recSkuUnit { get; set; }
        public List<HoldMasterModel> holdMasterModel { get; set; }
        public ShipPickDesModel shipPickDesModel{ get; set; }
        public ShipLoadDesModel shipLoadDesModel { get; set; }
        public List<ShipPickDesModel> shipPickDesModelList { get; set; }
        public List<ShipPickSplitDesModel> shipPickSplitDesModel { get; set; }
        public CycleCountInsertComplex cycleCountInsertComplex { get; set; }
        public List<HuDetailResult> huDetailResult { get; set; }
        public RecSkuDataCe recSkuDataCe { get; set; }

        public List<RecSkuDataCe> recSkuDataCeList { get; set; }
        public CycleCountInsertComplexAddPo cycleCountInsertComplexAddPo { get; set; }
     //   public PackTaskInsert packTaskInsert { get; set; }
        public List<SortTaskDetailResult> sortTaskDetailList { get; set; }

        public List<TransferDetailCe> transferDetailCeList { get; set; }

        public List<SupplementTaskCe> supplementTaskCe { get; set; }

        public List<SupplementTaskDetailCe> supplementTaskDetailCe { get; set; }
        public List<HuDetailRemained> huDetailRemained { get; set; }

        public List<PickTaskDetailWince> pickTaskDetailWince { get; set; }
        public List<RecLotFlagDescription> recLotFlagDescription { get; set; }

        public List<RecConsumerGoodsModel> recConsumerGoodsModelList { get; set; }
       public List<ItemZone> itemZoneList { get; set; }
        public HuMasterResult huMasterResult { get; set; }
        public List<RecSoModel> listRecSoModel { get; set; }
        public List<TruckQueueListDetail> truckQueueListDetailModel { get; set; }
        public ReceiptData ReceiptData { get; set; }

        public  List<PhotoMasterResult> PhotoMasterResult  { get; set; }

        public HuInfo huInfo { get; set; }
        
        public List<WorkloadAccountModelCN> workloadAccountModel { get; set; }
    }
}

 using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MODEL_MSSQL;


namespace WMS.IBLL
{
	public partial interface IAddValueServiceService : IBaseBLL<AddValueService>
    {
	        //不分页查询
            AddValueService Select(int id);
    }

	public partial interface IBillDetailService : IBaseBLL<BillDetail>
    {
	        //不分页查询
            BillDetail Select(int id);
    }

	public partial interface IBillMasterService : IBaseBLL<BillMaster>
    {
	        //不分页查询
            BillMaster Select(int id);
    }

	public partial interface IBusinessFlowGroupService : IBaseBLL<BusinessFlowGroup>
    {
	        //不分页查询
            BusinessFlowGroup Select(int id);
    }

	public partial interface IBusinessFlowHeadService : IBaseBLL<BusinessFlowHead>
    {
	        //不分页查询
            BusinessFlowHead Select(int id);
    }

	public partial interface IBusinessObjectService : IBaseBLL<BusinessObject>
    {
	        //不分页查询
            BusinessObject Select(int id);
    }

	public partial interface IBusinessObjectDetailService : IBaseBLL<BusinessObjectDetail>
    {
	        //不分页查询
            BusinessObjectDetail Select(int id);
    }

	public partial interface IBusinessObjectHeadService : IBaseBLL<BusinessObjectHead>
    {
	        //不分页查询
            BusinessObjectHead Select(int id);
    }

	public partial interface IBusinessObjectItemService : IBaseBLL<BusinessObjectItem>
    {
	        //不分页查询
            BusinessObjectItem Select(int id);
    }

	public partial interface IContractFormService : IBaseBLL<ContractForm>
    {
	        //不分页查询
            ContractForm Select(int id);
    }

	public partial interface IContractFormExtendService : IBaseBLL<ContractFormExtend>
    {
	        //不分页查询
            ContractFormExtend Select(int id);
    }

	public partial interface IContractFormOutService : IBaseBLL<ContractFormOut>
    {
	        //不分页查询
            ContractFormOut Select(int id);
    }

	public partial interface ICRTemplateService : IBaseBLL<CRTemplate>
    {
	        //不分页查询
            CRTemplate Select(int id);
    }

	public partial interface ICycleCountCheckService : IBaseBLL<CycleCountCheck>
    {
	        //不分页查询
            CycleCountCheck Select(int id);
    }

	public partial interface ICycleCountDetailService : IBaseBLL<CycleCountDetail>
    {
	        //不分页查询
            CycleCountDetail Select(int id);
    }

	public partial interface ICycleCountInventoryService : IBaseBLL<CycleCountInventory>
    {
	        //不分页查询
            CycleCountInventory Select(int id);
    }

	public partial interface ICycleCountMasterService : IBaseBLL<CycleCountMaster>
    {
	        //不分页查询
            CycleCountMaster Select(int id);
    }

	public partial interface IDamcoGRNDetailService : IBaseBLL<DamcoGRNDetail>
    {
	        //不分页查询
            DamcoGRNDetail Select(int id);
    }

	public partial interface IDamcoGRNHeadService : IBaseBLL<DamcoGRNHead>
    {
	        //不分页查询
            DamcoGRNHead Select(int id);
    }

	public partial interface IDamcoGrnRuleService : IBaseBLL<DamcoGrnRule>
    {
	        //不分页查询
            DamcoGrnRule Select(int id);
    }

	public partial interface IDcReturnExceptionService : IBaseBLL<DcReturnException>
    {
	        //不分页查询
            DcReturnException Select(int id);
    }

	public partial interface IDcShipingExceptionService : IBaseBLL<DcShipingException>
    {
	        //不分页查询
            DcShipingException Select(int id);
    }

	public partial interface IExcelImportInBoundService : IBaseBLL<ExcelImportInBound>
    {
	        //不分页查询
            ExcelImportInBound Select(int id);
    }

	public partial interface IExcelImportInBoundComService : IBaseBLL<ExcelImportInBoundCom>
    {
	        //不分页查询
            ExcelImportInBoundCom Select(int id);
    }

	public partial interface IExcelImportInBoundCottonService : IBaseBLL<ExcelImportInBoundCotton>
    {
	        //不分页查询
            ExcelImportInBoundCotton Select(int id);
    }

	public partial interface IExcelImportOutBoundService : IBaseBLL<ExcelImportOutBound>
    {
	        //不分页查询
            ExcelImportOutBound Select(int id);
    }

	public partial interface IFeeDetailService : IBaseBLL<FeeDetail>
    {
	        //不分页查询
            FeeDetail Select(int id);
    }

	public partial interface IFeeHolidayService : IBaseBLL<FeeHoliday>
    {
	        //不分页查询
            FeeHoliday Select(int id);
    }

	public partial interface IFeeMasterService : IBaseBLL<FeeMaster>
    {
	        //不分页查询
            FeeMaster Select(int id);
    }

	public partial interface IFieldOrderByService : IBaseBLL<FieldOrderBy>
    {
	        //不分页查询
            FieldOrderBy Select(int id);
    }

	public partial interface IFlowDetailService : IBaseBLL<FlowDetail>
    {
	        //不分页查询
            FlowDetail Select(int id);
    }

	public partial interface IFlowHeadService : IBaseBLL<FlowHead>
    {
	        //不分页查询
            FlowHead Select(int id);
    }

	public partial interface IFlowRuleService : IBaseBLL<FlowRule>
    {
	        //不分页查询
            FlowRule Select(int id);
    }

	public partial interface IHeport_R_SerialNumberInOutService : IBaseBLL<Heport_R_SerialNumberInOut>
    {
	        //不分页查询
            Heport_R_SerialNumberInOut Select(int id);
    }

	public partial interface IHeportSerialNumberInService : IBaseBLL<HeportSerialNumberIn>
    {
	        //不分页查询
            HeportSerialNumberIn Select(int id);
    }

	public partial interface IHoldMasterService : IBaseBLL<HoldMaster>
    {
	        //不分页查询
            HoldMaster Select(int id);
    }

	public partial interface IHolidayService : IBaseBLL<Holiday>
    {
	        //不分页查询
            Holiday Select(int id);
    }

	public partial interface IHuDetailService : IBaseBLL<HuDetail>
    {
	        //不分页查询
            HuDetail Select(int id);
    }

	public partial interface IHuMasterService : IBaseBLL<HuMaster>
    {
	        //不分页查询
            HuMaster Select(int id);
    }

	public partial interface IInBoundOrderService : IBaseBLL<InBoundOrder>
    {
	        //不分页查询
            InBoundOrder Select(int id);
    }

	public partial interface IInBoundOrderDetailService : IBaseBLL<InBoundOrderDetail>
    {
	        //不分页查询
            InBoundOrderDetail Select(int id);
    }

	public partial interface IInBoundSOService : IBaseBLL<InBoundSO>
    {
	        //不分页查询
            InBoundSO Select(int id);
    }

	public partial interface IInvMoveService : IBaseBLL<InvMove>
    {
	        //不分页查询
            InvMove Select(int id);
    }

	public partial interface IInvMoveDetailService : IBaseBLL<InvMoveDetail>
    {
	        //不分页查询
            InvMoveDetail Select(int id);
    }

	public partial interface IItemMasterService : IBaseBLL<ItemMaster>
    {
	        //不分页查询
            ItemMaster Select(int id);
    }

	public partial interface IItemMasterColorCodeService : IBaseBLL<ItemMasterColorCode>
    {
	        //不分页查询
            ItemMasterColorCode Select(int id);
    }

	public partial interface IItemMasterExtendOMService : IBaseBLL<ItemMasterExtendOM>
    {
	        //不分页查询
            ItemMasterExtendOM Select(int id);
    }

	public partial interface IKmartWaitingReasonService : IBaseBLL<KmartWaitingReason>
    {
	        //不分页查询
            KmartWaitingReason Select(int id);
    }

	public partial interface ILoadChargeService : IBaseBLL<LoadCharge>
    {
	        //不分页查询
            LoadCharge Select(int id);
    }

	public partial interface ILoadChargeDaiDianService : IBaseBLL<LoadChargeDaiDian>
    {
	        //不分页查询
            LoadChargeDaiDian Select(int id);
    }

	public partial interface ILoadChargeDetailService : IBaseBLL<LoadChargeDetail>
    {
	        //不分页查询
            LoadChargeDetail Select(int id);
    }

	public partial interface ILoadChargeRuleService : IBaseBLL<LoadChargeRule>
    {
	        //不分页查询
            LoadChargeRule Select(int id);
    }

	public partial interface ILoadContainerDetailExtendService : IBaseBLL<LoadContainerDetailExtend>
    {
	        //不分页查询
            LoadContainerDetailExtend Select(int id);
    }

	public partial interface ILoadContainerExtendService : IBaseBLL<LoadContainerExtend>
    {
	        //不分页查询
            LoadContainerExtend Select(int id);
    }

	public partial interface ILoadContainerExtendHuDetailService : IBaseBLL<LoadContainerExtendHuDetail>
    {
	        //不分页查询
            LoadContainerExtendHuDetail Select(int id);
    }

	public partial interface ILoadContainerTypeService : IBaseBLL<LoadContainerType>
    {
	        //不分页查询
            LoadContainerType Select(int id);
    }

	public partial interface ILoadCreateRuleService : IBaseBLL<LoadCreateRule>
    {
	        //不分页查询
            LoadCreateRule Select(int id);
    }

	public partial interface ILoadDetailService : IBaseBLL<LoadDetail>
    {
	        //不分页查询
            LoadDetail Select(int id);
    }

	public partial interface ILoadHuIdExtendService : IBaseBLL<LoadHuIdExtend>
    {
	        //不分页查询
            LoadHuIdExtend Select(int id);
    }

	public partial interface ILoadMasterService : IBaseBLL<LoadMaster>
    {
	        //不分页查询
            LoadMaster Select(int id);
    }

	public partial interface ILocationTypeService : IBaseBLL<LocationType>
    {
	        //不分页查询
            LocationType Select(int id);
    }

	public partial interface ILocationTypesDetailService : IBaseBLL<LocationTypesDetail>
    {
	        //不分页查询
            LocationTypesDetail Select(int id);
    }

	public partial interface ILoginLogService : IBaseBLL<LoginLog>
    {
	        //不分页查询
            LoginLog Select(int id);
    }

	public partial interface ILookUpService : IBaseBLL<LookUp>
    {
	        //不分页查询
            LookUp Select(int id);
    }

	public partial interface ILossService : IBaseBLL<Loss>
    {
	        //不分页查询
            Loss Select(int id);
    }

	public partial interface INightTimeService : IBaseBLL<NightTime>
    {
	        //不分页查询
            NightTime Select(int id);
    }

	public partial interface IOMSInvChangeService : IBaseBLL<OMSInvChange>
    {
	        //不分页查询
            OMSInvChange Select(int id);
    }

	public partial interface IOutBoundOrderService : IBaseBLL<OutBoundOrder>
    {
	        //不分页查询
            OutBoundOrder Select(int id);
    }

	public partial interface IOutBoundOrderDetailService : IBaseBLL<OutBoundOrderDetail>
    {
	        //不分页查询
            OutBoundOrderDetail Select(int id);
    }

	public partial interface IPackDetailService : IBaseBLL<PackDetail>
    {
	        //不分页查询
            PackDetail Select(int id);
    }

	public partial interface IPackHeadService : IBaseBLL<PackHead>
    {
	        //不分页查询
            PackHead Select(int id);
    }

	public partial interface IPackHeadJsonService : IBaseBLL<PackHeadJson>
    {
	        //不分页查询
            PackHeadJson Select(int id);
    }

	public partial interface IPackScanNumberService : IBaseBLL<PackScanNumber>
    {
	        //不分页查询
            PackScanNumber Select(int id);
    }

	public partial interface IPackTaskService : IBaseBLL<PackTask>
    {
	        //不分页查询
            PackTask Select(int id);
    }

	public partial interface IPackTaskJsonService : IBaseBLL<PackTaskJson>
    {
	        //不分页查询
            PackTaskJson Select(int id);
    }

	public partial interface IPallateService : IBaseBLL<Pallate>
    {
	        //不分页查询
            Pallate Select(int id);
    }

	public partial interface IPallateTypeService : IBaseBLL<PallateType>
    {
	        //不分页查询
            PallateType Select(int id);
    }

	public partial interface IPhotoMasterService : IBaseBLL<PhotoMaster>
    {
	        //不分页查询
            PhotoMaster Select(int id);
    }

	public partial interface IPickTaskDetailService : IBaseBLL<PickTaskDetail>
    {
	        //不分页查询
            PickTaskDetail Select(int id);
    }

	public partial interface IPlatformUserInfoService : IBaseBLL<PlatformUserInfo>
    {
	        //不分页查询
            PlatformUserInfo Select(int id);
    }

	public partial interface IR_Client_FlowRuleService : IBaseBLL<R_Client_FlowRule>
    {
	        //不分页查询
            R_Client_FlowRule Select(int id);
    }

	public partial interface IR_LoadRule_FlowHeadService : IBaseBLL<R_LoadRule_FlowHead>
    {
	        //不分页查询
            R_LoadRule_FlowHead Select(int id);
    }

	public partial interface IR_Location_ItemService : IBaseBLL<R_Location_Item>
    {
	        //不分页查询
            R_Location_Item Select(int id);
    }

	public partial interface IR_Location_Item_RGService : IBaseBLL<R_Location_Item_RG>
    {
	        //不分页查询
            R_Location_Item_RG Select(int id);
    }

	public partial interface IR_SerialNumberInOutService : IBaseBLL<R_SerialNumberInOut>
    {
	        //不分页查询
            R_SerialNumberInOut Select(int id);
    }

	public partial interface IR_WhClient_WhAgentService : IBaseBLL<R_WhClient_WhAgent>
    {
	        //不分页查询
            R_WhClient_WhAgent Select(int id);
    }

	public partial interface IR_WhInfo_WhUserService : IBaseBLL<R_WhInfo_WhUser>
    {
	        //不分页查询
            R_WhInfo_WhUser Select(int id);
    }

	public partial interface IReceiptService : IBaseBLL<Receipt>
    {
	        //不分页查询
            Receipt Select(int id);
    }

	public partial interface IReceiptChargeService : IBaseBLL<ReceiptCharge>
    {
	        //不分页查询
            ReceiptCharge Select(int id);
    }

	public partial interface IReceiptChargeDetailService : IBaseBLL<ReceiptChargeDetail>
    {
	        //不分页查询
            ReceiptChargeDetail Select(int id);
    }

	public partial interface IReceiptPartialRegisterService : IBaseBLL<ReceiptPartialRegister>
    {
	        //不分页查询
            ReceiptPartialRegister Select(int id);
    }

	public partial interface IReceiptPartialRegisterDetailService : IBaseBLL<ReceiptPartialRegisterDetail>
    {
	        //不分页查询
            ReceiptPartialRegisterDetail Select(int id);
    }

	public partial interface IReceiptRegisterService : IBaseBLL<ReceiptRegister>
    {
	        //不分页查询
            ReceiptRegister Select(int id);
    }

	public partial interface IReceiptRegisterDetailService : IBaseBLL<ReceiptRegisterDetail>
    {
	        //不分页查询
            ReceiptRegisterDetail Select(int id);
    }

	public partial interface IReceiptRegisterExtendService : IBaseBLL<ReceiptRegisterExtend>
    {
	        //不分页查询
            ReceiptRegisterExtend Select(int id);
    }

	public partial interface IReceiptTruckService : IBaseBLL<ReceiptTruck>
    {
	        //不分页查询
            ReceiptTruck Select(int id);
    }

	public partial interface IRecLossService : IBaseBLL<RecLoss>
    {
	        //不分页查询
            RecLoss Select(int id);
    }

	public partial interface IRecLossTypeService : IBaseBLL<RecLossType>
    {
	        //不分页查询
            RecLossType Select(int id);
    }

	public partial interface IReleaseLoadDetailService : IBaseBLL<ReleaseLoadDetail>
    {
	        //不分页查询
            ReleaseLoadDetail Select(int id);
    }

	public partial interface IRFFlowRuleService : IBaseBLL<RFFlowRule>
    {
	        //不分页查询
            RFFlowRule Select(int id);
    }

	public partial interface ISerialNumberDetailService : IBaseBLL<SerialNumberDetail>
    {
	        //不分页查询
            SerialNumberDetail Select(int id);
    }

	public partial interface ISerialNumberInService : IBaseBLL<SerialNumberIn>
    {
	        //不分页查询
            SerialNumberIn Select(int id);
    }

	public partial interface ISerialNumberInOutExtendService : IBaseBLL<SerialNumberInOutExtend>
    {
	        //不分页查询
            SerialNumberInOutExtend Select(int id);
    }

	public partial interface ISerialNumberOutService : IBaseBLL<SerialNumberOut>
    {
	        //不分页查询
            SerialNumberOut Select(int id);
    }

	public partial interface ISortTaskService : IBaseBLL<SortTask>
    {
	        //不分页查询
            SortTask Select(int id);
    }

	public partial interface ISortTaskDetailService : IBaseBLL<SortTaskDetail>
    {
	        //不分页查询
            SortTaskDetail Select(int id);
    }

	public partial interface IStringrRgularService : IBaseBLL<StringrRgular>
    {
	        //不分页查询
            StringrRgular Select(int id);
    }

	public partial interface ISupplementTaskService : IBaseBLL<SupplementTask>
    {
	        //不分页查询
            SupplementTask Select(int id);
    }

	public partial interface ISupplementTaskDetailService : IBaseBLL<SupplementTaskDetail>
    {
	        //不分页查询
            SupplementTaskDetail Select(int id);
    }

	public partial interface ITCRProcessService : IBaseBLL<TCRProcess>
    {
	        //不分页查询
            TCRProcess Select(int id);
    }

	public partial interface ITranLogService : IBaseBLL<TranLog>
    {
	        //不分页查询
            TranLog Select(int id);
    }

	public partial interface ITransferDetailService : IBaseBLL<TransferDetail>
    {
	        //不分页查询
            TransferDetail Select(int id);
    }

	public partial interface ITransferHeadService : IBaseBLL<TransferHead>
    {
	        //不分页查询
            TransferHead Select(int id);
    }

	public partial interface ITransferScanNumberService : IBaseBLL<TransferScanNumber>
    {
	        //不分页查询
            TransferScanNumber Select(int id);
    }

	public partial interface ITransferTaskService : IBaseBLL<TransferTask>
    {
	        //不分页查询
            TransferTask Select(int id);
    }

	public partial interface ITruckQueueDetailService : IBaseBLL<TruckQueueDetail>
    {
	        //不分页查询
            TruckQueueDetail Select(int id);
    }

	public partial interface ITruckQueueHeadService : IBaseBLL<TruckQueueHead>
    {
	        //不分页查询
            TruckQueueHead Select(int id);
    }

	public partial interface ItruckQueueViewService : IBaseBLL<truckQueueView>
    {
	        //不分页查询
            truckQueueView Select(int id);
    }

	public partial interface IUnitService : IBaseBLL<Unit>
    {
	        //不分页查询
            Unit Select(int id);
    }

	public partial interface IUnitDefaultService : IBaseBLL<UnitDefault>
    {
	        //不分页查询
            UnitDefault Select(int id);
    }

	public partial interface IUrlEdiService : IBaseBLL<UrlEdi>
    {
	        //不分页查询
            UrlEdi Select(int id);
    }

	public partial interface IUrlEdiTaskService : IBaseBLL<UrlEdiTask>
    {
	        //不分页查询
            UrlEdiTask Select(int id);
    }

	public partial interface IUserPriceTService : IBaseBLL<UserPriceT>
    {
	        //不分页查询
            UserPriceT Select(int id);
    }

	public partial interface IUserShenTService : IBaseBLL<UserShenT>
    {
	        //不分页查询
            UserShenT Select(int id);
    }

	public partial interface IWhAgentService : IBaseBLL<WhAgent>
    {
	        //不分页查询
            WhAgent Select(int id);
    }

	public partial interface IWhClientService : IBaseBLL<WhClient>
    {
	        //不分页查询
            WhClient Select(int id);
    }

	public partial interface IWhClientExtendService : IBaseBLL<WhClientExtend>
    {
	        //不分页查询
            WhClientExtend Select(int id);
    }

	public partial interface IWhClientTypeService : IBaseBLL<WhClientType>
    {
	        //不分页查询
            WhClientType Select(int id);
    }

	public partial interface IWhCompanyService : IBaseBLL<WhCompany>
    {
	        //不分页查询
            WhCompany Select(int id);
    }

	public partial interface IWhInfoService : IBaseBLL<WhInfo>
    {
	        //不分页查询
            WhInfo Select(int id);
    }

	public partial interface IWhLevelService : IBaseBLL<WhLevel>
    {
	        //不分页查询
            WhLevel Select(int id);
    }

	public partial interface IWhLocationService : IBaseBLL<WhLocation>
    {
	        //不分页查询
            WhLocation Select(int id);
    }

	public partial interface IWhMenuService : IBaseBLL<WhMenu>
    {
	        //不分页查询
            WhMenu Select(int id);
    }

	public partial interface IWhPositionService : IBaseBLL<WhPosition>
    {
	        //不分页查询
            WhPosition Select(int id);
    }

	public partial interface IWhPositionPowerService : IBaseBLL<WhPositionPower>
    {
	        //不分页查询
            WhPositionPower Select(int id);
    }

	public partial interface IWhPositionPowerMVCService : IBaseBLL<WhPositionPowerMVC>
    {
	        //不分页查询
            WhPositionPowerMVC Select(int id);
    }

	public partial interface IWhPowerService : IBaseBLL<WhPower>
    {
	        //不分页查询
            WhPower Select(int id);
    }

	public partial interface IWhUserService : IBaseBLL<WhUser>
    {
	        //不分页查询
            WhUser Select(int id);
    }

	public partial interface IWhUserPositionService : IBaseBLL<WhUserPosition>
    {
	        //不分页查询
            WhUserPosition Select(int id);
    }

	public partial interface IWorkloadAccountService : IBaseBLL<WorkloadAccount>
    {
	        //不分页查询
            WorkloadAccount Select(int id);
    }

	public partial interface IZoneService : IBaseBLL<Zone>
    {
	        //不分页查询
            Zone Select(int id);
    }

	public partial interface IZoneLocationService : IBaseBLL<ZoneLocation>
    {
	        //不分页查询
            ZoneLocation Select(int id);
    }

	public partial interface IZonesExtendService : IBaseBLL<ZonesExtend>
    {
	        //不分页查询
            ZonesExtend Select(int id);
    }


}
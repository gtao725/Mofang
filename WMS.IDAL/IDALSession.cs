
 using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MODEL_MSSQL;

namespace WMS.IDAL
{

	public partial interface IDALSession
	{ 

		IAddValueServiceDAL IAddValueServiceDAL { get; }


		IBillDetailDAL IBillDetailDAL { get; }


		IBillMasterDAL IBillMasterDAL { get; }


		IBusinessFlowGroupDAL IBusinessFlowGroupDAL { get; }


		IBusinessFlowHeadDAL IBusinessFlowHeadDAL { get; }


		IBusinessObjectDAL IBusinessObjectDAL { get; }


		IBusinessObjectDetailDAL IBusinessObjectDetailDAL { get; }


		IBusinessObjectHeadDAL IBusinessObjectHeadDAL { get; }


		IBusinessObjectItemDAL IBusinessObjectItemDAL { get; }


		IContractFormDAL IContractFormDAL { get; }


		IContractFormExtendDAL IContractFormExtendDAL { get; }


		IContractFormOutDAL IContractFormOutDAL { get; }


		ICRTemplateDAL ICRTemplateDAL { get; }


		ICycleCountCheckDAL ICycleCountCheckDAL { get; }


		ICycleCountDetailDAL ICycleCountDetailDAL { get; }


		ICycleCountInventoryDAL ICycleCountInventoryDAL { get; }


		ICycleCountMasterDAL ICycleCountMasterDAL { get; }


		IDamcoGRNDetailDAL IDamcoGRNDetailDAL { get; }


		IDamcoGRNHeadDAL IDamcoGRNHeadDAL { get; }


		IDamcoGrnRuleDAL IDamcoGrnRuleDAL { get; }


		IDcReturnExceptionDAL IDcReturnExceptionDAL { get; }


		IDcShipingExceptionDAL IDcShipingExceptionDAL { get; }


		IExcelImportInBoundDAL IExcelImportInBoundDAL { get; }


		IExcelImportInBoundComDAL IExcelImportInBoundComDAL { get; }


		IExcelImportInBoundCottonDAL IExcelImportInBoundCottonDAL { get; }


		IExcelImportOutBoundDAL IExcelImportOutBoundDAL { get; }


		IFeeDetailDAL IFeeDetailDAL { get; }


		IFeeHolidayDAL IFeeHolidayDAL { get; }


		IFeeMasterDAL IFeeMasterDAL { get; }


		IFieldOrderByDAL IFieldOrderByDAL { get; }


		IFlowDetailDAL IFlowDetailDAL { get; }


		IFlowHeadDAL IFlowHeadDAL { get; }


		IFlowRuleDAL IFlowRuleDAL { get; }


		IHeport_R_SerialNumberInOutDAL IHeport_R_SerialNumberInOutDAL { get; }


		IHeportSerialNumberInDAL IHeportSerialNumberInDAL { get; }


		IHoldMasterDAL IHoldMasterDAL { get; }


		IHolidayDAL IHolidayDAL { get; }


		IHuDetailDAL IHuDetailDAL { get; }


		IHuMasterDAL IHuMasterDAL { get; }


		IInBoundOrderDAL IInBoundOrderDAL { get; }


		IInBoundOrderDetailDAL IInBoundOrderDetailDAL { get; }


		IInBoundSODAL IInBoundSODAL { get; }


		IInvMoveDAL IInvMoveDAL { get; }


		IInvMoveDetailDAL IInvMoveDetailDAL { get; }


		IItemMasterDAL IItemMasterDAL { get; }


		IItemMasterColorCodeDAL IItemMasterColorCodeDAL { get; }


		IItemMasterExtendOMDAL IItemMasterExtendOMDAL { get; }


		IKmartWaitingReasonDAL IKmartWaitingReasonDAL { get; }


		ILoadChargeDAL ILoadChargeDAL { get; }


		ILoadChargeDaiDianDAL ILoadChargeDaiDianDAL { get; }


		ILoadChargeDetailDAL ILoadChargeDetailDAL { get; }


		ILoadChargeRuleDAL ILoadChargeRuleDAL { get; }


		ILoadContainerDetailExtendDAL ILoadContainerDetailExtendDAL { get; }


		ILoadContainerExtendDAL ILoadContainerExtendDAL { get; }


		ILoadContainerExtendHuDetailDAL ILoadContainerExtendHuDetailDAL { get; }


		ILoadContainerTypeDAL ILoadContainerTypeDAL { get; }


		ILoadCreateRuleDAL ILoadCreateRuleDAL { get; }


		ILoadDetailDAL ILoadDetailDAL { get; }


		ILoadHuIdExtendDAL ILoadHuIdExtendDAL { get; }


		ILoadMasterDAL ILoadMasterDAL { get; }


		ILocationTypeDAL ILocationTypeDAL { get; }


		ILocationTypesDetailDAL ILocationTypesDetailDAL { get; }


		ILoginLogDAL ILoginLogDAL { get; }


		ILookUpDAL ILookUpDAL { get; }


		ILossDAL ILossDAL { get; }


		INightTimeDAL INightTimeDAL { get; }


		IOMSInvChangeDAL IOMSInvChangeDAL { get; }


		IOutBoundOrderDAL IOutBoundOrderDAL { get; }


		IOutBoundOrderDetailDAL IOutBoundOrderDetailDAL { get; }


		IPackDetailDAL IPackDetailDAL { get; }


		IPackHeadDAL IPackHeadDAL { get; }


		IPackHeadJsonDAL IPackHeadJsonDAL { get; }


		IPackScanNumberDAL IPackScanNumberDAL { get; }


		IPackTaskDAL IPackTaskDAL { get; }


		IPackTaskJsonDAL IPackTaskJsonDAL { get; }


		IPallateDAL IPallateDAL { get; }


		IPallateTypeDAL IPallateTypeDAL { get; }


		IPhotoMasterDAL IPhotoMasterDAL { get; }


		IPickTaskDetailDAL IPickTaskDetailDAL { get; }


		IPlatformUserInfoDAL IPlatformUserInfoDAL { get; }


		IR_Client_FlowRuleDAL IR_Client_FlowRuleDAL { get; }


		IR_LoadRule_FlowHeadDAL IR_LoadRule_FlowHeadDAL { get; }


		IR_Location_ItemDAL IR_Location_ItemDAL { get; }


		IR_Location_Item_RGDAL IR_Location_Item_RGDAL { get; }


		IR_SerialNumberInOutDAL IR_SerialNumberInOutDAL { get; }


		IR_WhClient_WhAgentDAL IR_WhClient_WhAgentDAL { get; }


		IR_WhInfo_WhUserDAL IR_WhInfo_WhUserDAL { get; }


		IReceiptDAL IReceiptDAL { get; }


		IReceiptChargeDAL IReceiptChargeDAL { get; }


		IReceiptChargeDetailDAL IReceiptChargeDetailDAL { get; }


		IReceiptPartialRegisterDAL IReceiptPartialRegisterDAL { get; }


		IReceiptPartialRegisterDetailDAL IReceiptPartialRegisterDetailDAL { get; }


		IReceiptRegisterDAL IReceiptRegisterDAL { get; }


		IReceiptRegisterDetailDAL IReceiptRegisterDetailDAL { get; }


		IReceiptRegisterExtendDAL IReceiptRegisterExtendDAL { get; }


		IReceiptTruckDAL IReceiptTruckDAL { get; }


		IRecLossDAL IRecLossDAL { get; }


		IRecLossTypeDAL IRecLossTypeDAL { get; }


		IReleaseLoadDetailDAL IReleaseLoadDetailDAL { get; }


		IRFFlowRuleDAL IRFFlowRuleDAL { get; }


		ISerialNumberDetailDAL ISerialNumberDetailDAL { get; }


		ISerialNumberInDAL ISerialNumberInDAL { get; }


		ISerialNumberInOutExtendDAL ISerialNumberInOutExtendDAL { get; }


		ISerialNumberOutDAL ISerialNumberOutDAL { get; }


		ISortTaskDAL ISortTaskDAL { get; }


		ISortTaskDetailDAL ISortTaskDetailDAL { get; }


		IStringrRgularDAL IStringrRgularDAL { get; }


		ISupplementTaskDAL ISupplementTaskDAL { get; }


		ISupplementTaskDetailDAL ISupplementTaskDetailDAL { get; }


		ITCRProcessDAL ITCRProcessDAL { get; }


		ITranLogDAL ITranLogDAL { get; }


		ITransferDetailDAL ITransferDetailDAL { get; }


		ITransferHeadDAL ITransferHeadDAL { get; }


		ITransferScanNumberDAL ITransferScanNumberDAL { get; }


		ITransferTaskDAL ITransferTaskDAL { get; }


		ITruckQueueDetailDAL ITruckQueueDetailDAL { get; }


		ITruckQueueHeadDAL ITruckQueueHeadDAL { get; }


		ItruckQueueViewDAL ItruckQueueViewDAL { get; }


		IUnitDAL IUnitDAL { get; }


		IUnitDefaultDAL IUnitDefaultDAL { get; }


		IUrlEdiDAL IUrlEdiDAL { get; }


		IUrlEdiTaskDAL IUrlEdiTaskDAL { get; }


		IUserPriceTDAL IUserPriceTDAL { get; }


		IUserShenTDAL IUserShenTDAL { get; }


		IWhAgentDAL IWhAgentDAL { get; }


		IWhClientDAL IWhClientDAL { get; }


		IWhClientExtendDAL IWhClientExtendDAL { get; }


		IWhClientTypeDAL IWhClientTypeDAL { get; }


		IWhCompanyDAL IWhCompanyDAL { get; }


		IWhInfoDAL IWhInfoDAL { get; }


		IWhLevelDAL IWhLevelDAL { get; }


		IWhLocationDAL IWhLocationDAL { get; }


		IWhMenuDAL IWhMenuDAL { get; }


		IWhPositionDAL IWhPositionDAL { get; }


		IWhPositionPowerDAL IWhPositionPowerDAL { get; }


		IWhPositionPowerMVCDAL IWhPositionPowerMVCDAL { get; }


		IWhPowerDAL IWhPowerDAL { get; }


		IWhUserDAL IWhUserDAL { get; }


		IWhUserPositionDAL IWhUserPositionDAL { get; }


		IWorkloadAccountDAL IWorkloadAccountDAL { get; }


		IZoneDAL IZoneDAL { get; }


		IZoneLocationDAL IZoneLocationDAL { get; }


		IZonesExtendDAL IZonesExtendDAL { get; }

}
}
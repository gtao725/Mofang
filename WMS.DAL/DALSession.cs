
 using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MODEL_MSSQL;
using WMS.IDAL;

namespace WMS.DAL
{

	public partial class DALSession:IDALSession
	{ 

		#region 01 数据接口 IAddValueServiceDAL
		IAddValueServiceDAL iAddValueServiceDAL;
		public IAddValueServiceDAL IAddValueServiceDAL 
		{ 
			get
			{
				if(iAddValueServiceDAL==null)
				{
					iAddValueServiceDAL=new AddValueServiceDAL();	
				}
				return iAddValueServiceDAL;
			}
		 }
		#endregion

		#region 02 数据接口 IBillDetailDAL
		IBillDetailDAL iBillDetailDAL;
		public IBillDetailDAL IBillDetailDAL 
		{ 
			get
			{
				if(iBillDetailDAL==null)
				{
					iBillDetailDAL=new BillDetailDAL();	
				}
				return iBillDetailDAL;
			}
		 }
		#endregion

		#region 03 数据接口 IBillMasterDAL
		IBillMasterDAL iBillMasterDAL;
		public IBillMasterDAL IBillMasterDAL 
		{ 
			get
			{
				if(iBillMasterDAL==null)
				{
					iBillMasterDAL=new BillMasterDAL();	
				}
				return iBillMasterDAL;
			}
		 }
		#endregion

		#region 04 数据接口 IBusinessFlowGroupDAL
		IBusinessFlowGroupDAL iBusinessFlowGroupDAL;
		public IBusinessFlowGroupDAL IBusinessFlowGroupDAL 
		{ 
			get
			{
				if(iBusinessFlowGroupDAL==null)
				{
					iBusinessFlowGroupDAL=new BusinessFlowGroupDAL();	
				}
				return iBusinessFlowGroupDAL;
			}
		 }
		#endregion

		#region 05 数据接口 IBusinessFlowHeadDAL
		IBusinessFlowHeadDAL iBusinessFlowHeadDAL;
		public IBusinessFlowHeadDAL IBusinessFlowHeadDAL 
		{ 
			get
			{
				if(iBusinessFlowHeadDAL==null)
				{
					iBusinessFlowHeadDAL=new BusinessFlowHeadDAL();	
				}
				return iBusinessFlowHeadDAL;
			}
		 }
		#endregion

		#region 06 数据接口 IBusinessObjectDAL
		IBusinessObjectDAL iBusinessObjectDAL;
		public IBusinessObjectDAL IBusinessObjectDAL 
		{ 
			get
			{
				if(iBusinessObjectDAL==null)
				{
					iBusinessObjectDAL=new BusinessObjectDAL();	
				}
				return iBusinessObjectDAL;
			}
		 }
		#endregion

		#region 07 数据接口 IBusinessObjectDetailDAL
		IBusinessObjectDetailDAL iBusinessObjectDetailDAL;
		public IBusinessObjectDetailDAL IBusinessObjectDetailDAL 
		{ 
			get
			{
				if(iBusinessObjectDetailDAL==null)
				{
					iBusinessObjectDetailDAL=new BusinessObjectDetailDAL();	
				}
				return iBusinessObjectDetailDAL;
			}
		 }
		#endregion

		#region 08 数据接口 IBusinessObjectHeadDAL
		IBusinessObjectHeadDAL iBusinessObjectHeadDAL;
		public IBusinessObjectHeadDAL IBusinessObjectHeadDAL 
		{ 
			get
			{
				if(iBusinessObjectHeadDAL==null)
				{
					iBusinessObjectHeadDAL=new BusinessObjectHeadDAL();	
				}
				return iBusinessObjectHeadDAL;
			}
		 }
		#endregion

		#region 09 数据接口 IBusinessObjectItemDAL
		IBusinessObjectItemDAL iBusinessObjectItemDAL;
		public IBusinessObjectItemDAL IBusinessObjectItemDAL 
		{ 
			get
			{
				if(iBusinessObjectItemDAL==null)
				{
					iBusinessObjectItemDAL=new BusinessObjectItemDAL();	
				}
				return iBusinessObjectItemDAL;
			}
		 }
		#endregion

		#region 10 数据接口 IContractFormDAL
		IContractFormDAL iContractFormDAL;
		public IContractFormDAL IContractFormDAL 
		{ 
			get
			{
				if(iContractFormDAL==null)
				{
					iContractFormDAL=new ContractFormDAL();	
				}
				return iContractFormDAL;
			}
		 }
		#endregion

		#region 11 数据接口 IContractFormExtendDAL
		IContractFormExtendDAL iContractFormExtendDAL;
		public IContractFormExtendDAL IContractFormExtendDAL 
		{ 
			get
			{
				if(iContractFormExtendDAL==null)
				{
					iContractFormExtendDAL=new ContractFormExtendDAL();	
				}
				return iContractFormExtendDAL;
			}
		 }
		#endregion

		#region 12 数据接口 IContractFormOutDAL
		IContractFormOutDAL iContractFormOutDAL;
		public IContractFormOutDAL IContractFormOutDAL 
		{ 
			get
			{
				if(iContractFormOutDAL==null)
				{
					iContractFormOutDAL=new ContractFormOutDAL();	
				}
				return iContractFormOutDAL;
			}
		 }
		#endregion

		#region 13 数据接口 ICRTemplateDAL
		ICRTemplateDAL iCRTemplateDAL;
		public ICRTemplateDAL ICRTemplateDAL 
		{ 
			get
			{
				if(iCRTemplateDAL==null)
				{
					iCRTemplateDAL=new CRTemplateDAL();	
				}
				return iCRTemplateDAL;
			}
		 }
		#endregion

		#region 14 数据接口 ICycleCountCheckDAL
		ICycleCountCheckDAL iCycleCountCheckDAL;
		public ICycleCountCheckDAL ICycleCountCheckDAL 
		{ 
			get
			{
				if(iCycleCountCheckDAL==null)
				{
					iCycleCountCheckDAL=new CycleCountCheckDAL();	
				}
				return iCycleCountCheckDAL;
			}
		 }
		#endregion

		#region 15 数据接口 ICycleCountDetailDAL
		ICycleCountDetailDAL iCycleCountDetailDAL;
		public ICycleCountDetailDAL ICycleCountDetailDAL 
		{ 
			get
			{
				if(iCycleCountDetailDAL==null)
				{
					iCycleCountDetailDAL=new CycleCountDetailDAL();	
				}
				return iCycleCountDetailDAL;
			}
		 }
		#endregion

		#region 16 数据接口 ICycleCountInventoryDAL
		ICycleCountInventoryDAL iCycleCountInventoryDAL;
		public ICycleCountInventoryDAL ICycleCountInventoryDAL 
		{ 
			get
			{
				if(iCycleCountInventoryDAL==null)
				{
					iCycleCountInventoryDAL=new CycleCountInventoryDAL();	
				}
				return iCycleCountInventoryDAL;
			}
		 }
		#endregion

		#region 17 数据接口 ICycleCountMasterDAL
		ICycleCountMasterDAL iCycleCountMasterDAL;
		public ICycleCountMasterDAL ICycleCountMasterDAL 
		{ 
			get
			{
				if(iCycleCountMasterDAL==null)
				{
					iCycleCountMasterDAL=new CycleCountMasterDAL();	
				}
				return iCycleCountMasterDAL;
			}
		 }
		#endregion

		#region 18 数据接口 IDamcoGRNDetailDAL
		IDamcoGRNDetailDAL iDamcoGRNDetailDAL;
		public IDamcoGRNDetailDAL IDamcoGRNDetailDAL 
		{ 
			get
			{
				if(iDamcoGRNDetailDAL==null)
				{
					iDamcoGRNDetailDAL=new DamcoGRNDetailDAL();	
				}
				return iDamcoGRNDetailDAL;
			}
		 }
		#endregion

		#region 19 数据接口 IDamcoGRNHeadDAL
		IDamcoGRNHeadDAL iDamcoGRNHeadDAL;
		public IDamcoGRNHeadDAL IDamcoGRNHeadDAL 
		{ 
			get
			{
				if(iDamcoGRNHeadDAL==null)
				{
					iDamcoGRNHeadDAL=new DamcoGRNHeadDAL();	
				}
				return iDamcoGRNHeadDAL;
			}
		 }
		#endregion

		#region 20 数据接口 IDamcoGrnRuleDAL
		IDamcoGrnRuleDAL iDamcoGrnRuleDAL;
		public IDamcoGrnRuleDAL IDamcoGrnRuleDAL 
		{ 
			get
			{
				if(iDamcoGrnRuleDAL==null)
				{
					iDamcoGrnRuleDAL=new DamcoGrnRuleDAL();	
				}
				return iDamcoGrnRuleDAL;
			}
		 }
		#endregion

		#region 21 数据接口 IDcReturnExceptionDAL
		IDcReturnExceptionDAL iDcReturnExceptionDAL;
		public IDcReturnExceptionDAL IDcReturnExceptionDAL 
		{ 
			get
			{
				if(iDcReturnExceptionDAL==null)
				{
					iDcReturnExceptionDAL=new DcReturnExceptionDAL();	
				}
				return iDcReturnExceptionDAL;
			}
		 }
		#endregion

		#region 22 数据接口 IDcShipingExceptionDAL
		IDcShipingExceptionDAL iDcShipingExceptionDAL;
		public IDcShipingExceptionDAL IDcShipingExceptionDAL 
		{ 
			get
			{
				if(iDcShipingExceptionDAL==null)
				{
					iDcShipingExceptionDAL=new DcShipingExceptionDAL();	
				}
				return iDcShipingExceptionDAL;
			}
		 }
		#endregion

		#region 23 数据接口 IExcelImportInBoundDAL
		IExcelImportInBoundDAL iExcelImportInBoundDAL;
		public IExcelImportInBoundDAL IExcelImportInBoundDAL 
		{ 
			get
			{
				if(iExcelImportInBoundDAL==null)
				{
					iExcelImportInBoundDAL=new ExcelImportInBoundDAL();	
				}
				return iExcelImportInBoundDAL;
			}
		 }
		#endregion

		#region 24 数据接口 IExcelImportInBoundComDAL
		IExcelImportInBoundComDAL iExcelImportInBoundComDAL;
		public IExcelImportInBoundComDAL IExcelImportInBoundComDAL 
		{ 
			get
			{
				if(iExcelImportInBoundComDAL==null)
				{
					iExcelImportInBoundComDAL=new ExcelImportInBoundComDAL();	
				}
				return iExcelImportInBoundComDAL;
			}
		 }
		#endregion

		#region 25 数据接口 IExcelImportInBoundCottonDAL
		IExcelImportInBoundCottonDAL iExcelImportInBoundCottonDAL;
		public IExcelImportInBoundCottonDAL IExcelImportInBoundCottonDAL 
		{ 
			get
			{
				if(iExcelImportInBoundCottonDAL==null)
				{
					iExcelImportInBoundCottonDAL=new ExcelImportInBoundCottonDAL();	
				}
				return iExcelImportInBoundCottonDAL;
			}
		 }
		#endregion

		#region 26 数据接口 IExcelImportOutBoundDAL
		IExcelImportOutBoundDAL iExcelImportOutBoundDAL;
		public IExcelImportOutBoundDAL IExcelImportOutBoundDAL 
		{ 
			get
			{
				if(iExcelImportOutBoundDAL==null)
				{
					iExcelImportOutBoundDAL=new ExcelImportOutBoundDAL();	
				}
				return iExcelImportOutBoundDAL;
			}
		 }
		#endregion

		#region 27 数据接口 IFeeDetailDAL
		IFeeDetailDAL iFeeDetailDAL;
		public IFeeDetailDAL IFeeDetailDAL 
		{ 
			get
			{
				if(iFeeDetailDAL==null)
				{
					iFeeDetailDAL=new FeeDetailDAL();	
				}
				return iFeeDetailDAL;
			}
		 }
		#endregion

		#region 28 数据接口 IFeeHolidayDAL
		IFeeHolidayDAL iFeeHolidayDAL;
		public IFeeHolidayDAL IFeeHolidayDAL 
		{ 
			get
			{
				if(iFeeHolidayDAL==null)
				{
					iFeeHolidayDAL=new FeeHolidayDAL();	
				}
				return iFeeHolidayDAL;
			}
		 }
		#endregion

		#region 29 数据接口 IFeeMasterDAL
		IFeeMasterDAL iFeeMasterDAL;
		public IFeeMasterDAL IFeeMasterDAL 
		{ 
			get
			{
				if(iFeeMasterDAL==null)
				{
					iFeeMasterDAL=new FeeMasterDAL();	
				}
				return iFeeMasterDAL;
			}
		 }
		#endregion

		#region 30 数据接口 IFieldOrderByDAL
		IFieldOrderByDAL iFieldOrderByDAL;
		public IFieldOrderByDAL IFieldOrderByDAL 
		{ 
			get
			{
				if(iFieldOrderByDAL==null)
				{
					iFieldOrderByDAL=new FieldOrderByDAL();	
				}
				return iFieldOrderByDAL;
			}
		 }
		#endregion

		#region 31 数据接口 IFlowDetailDAL
		IFlowDetailDAL iFlowDetailDAL;
		public IFlowDetailDAL IFlowDetailDAL 
		{ 
			get
			{
				if(iFlowDetailDAL==null)
				{
					iFlowDetailDAL=new FlowDetailDAL();	
				}
				return iFlowDetailDAL;
			}
		 }
		#endregion

		#region 32 数据接口 IFlowHeadDAL
		IFlowHeadDAL iFlowHeadDAL;
		public IFlowHeadDAL IFlowHeadDAL 
		{ 
			get
			{
				if(iFlowHeadDAL==null)
				{
					iFlowHeadDAL=new FlowHeadDAL();	
				}
				return iFlowHeadDAL;
			}
		 }
		#endregion

		#region 33 数据接口 IFlowRuleDAL
		IFlowRuleDAL iFlowRuleDAL;
		public IFlowRuleDAL IFlowRuleDAL 
		{ 
			get
			{
				if(iFlowRuleDAL==null)
				{
					iFlowRuleDAL=new FlowRuleDAL();	
				}
				return iFlowRuleDAL;
			}
		 }
		#endregion

		#region 34 数据接口 IHeport_R_SerialNumberInOutDAL
		IHeport_R_SerialNumberInOutDAL iHeport_R_SerialNumberInOutDAL;
		public IHeport_R_SerialNumberInOutDAL IHeport_R_SerialNumberInOutDAL 
		{ 
			get
			{
				if(iHeport_R_SerialNumberInOutDAL==null)
				{
					iHeport_R_SerialNumberInOutDAL=new Heport_R_SerialNumberInOutDAL();	
				}
				return iHeport_R_SerialNumberInOutDAL;
			}
		 }
		#endregion

		#region 35 数据接口 IHeportSerialNumberInDAL
		IHeportSerialNumberInDAL iHeportSerialNumberInDAL;
		public IHeportSerialNumberInDAL IHeportSerialNumberInDAL 
		{ 
			get
			{
				if(iHeportSerialNumberInDAL==null)
				{
					iHeportSerialNumberInDAL=new HeportSerialNumberInDAL();	
				}
				return iHeportSerialNumberInDAL;
			}
		 }
		#endregion

		#region 36 数据接口 IHoldMasterDAL
		IHoldMasterDAL iHoldMasterDAL;
		public IHoldMasterDAL IHoldMasterDAL 
		{ 
			get
			{
				if(iHoldMasterDAL==null)
				{
					iHoldMasterDAL=new HoldMasterDAL();	
				}
				return iHoldMasterDAL;
			}
		 }
		#endregion

		#region 37 数据接口 IHolidayDAL
		IHolidayDAL iHolidayDAL;
		public IHolidayDAL IHolidayDAL 
		{ 
			get
			{
				if(iHolidayDAL==null)
				{
					iHolidayDAL=new HolidayDAL();	
				}
				return iHolidayDAL;
			}
		 }
		#endregion

		#region 38 数据接口 IHuDetailDAL
		IHuDetailDAL iHuDetailDAL;
		public IHuDetailDAL IHuDetailDAL 
		{ 
			get
			{
				if(iHuDetailDAL==null)
				{
					iHuDetailDAL=new HuDetailDAL();	
				}
				return iHuDetailDAL;
			}
		 }
		#endregion

		#region 39 数据接口 IHuMasterDAL
		IHuMasterDAL iHuMasterDAL;
		public IHuMasterDAL IHuMasterDAL 
		{ 
			get
			{
				if(iHuMasterDAL==null)
				{
					iHuMasterDAL=new HuMasterDAL();	
				}
				return iHuMasterDAL;
			}
		 }
		#endregion

		#region 40 数据接口 IInBoundOrderDAL
		IInBoundOrderDAL iInBoundOrderDAL;
		public IInBoundOrderDAL IInBoundOrderDAL 
		{ 
			get
			{
				if(iInBoundOrderDAL==null)
				{
					iInBoundOrderDAL=new InBoundOrderDAL();	
				}
				return iInBoundOrderDAL;
			}
		 }
		#endregion

		#region 41 数据接口 IInBoundOrderDetailDAL
		IInBoundOrderDetailDAL iInBoundOrderDetailDAL;
		public IInBoundOrderDetailDAL IInBoundOrderDetailDAL 
		{ 
			get
			{
				if(iInBoundOrderDetailDAL==null)
				{
					iInBoundOrderDetailDAL=new InBoundOrderDetailDAL();	
				}
				return iInBoundOrderDetailDAL;
			}
		 }
		#endregion

		#region 42 数据接口 IInBoundSODAL
		IInBoundSODAL iInBoundSODAL;
		public IInBoundSODAL IInBoundSODAL 
		{ 
			get
			{
				if(iInBoundSODAL==null)
				{
					iInBoundSODAL=new InBoundSODAL();	
				}
				return iInBoundSODAL;
			}
		 }
		#endregion

		#region 43 数据接口 IInvMoveDAL
		IInvMoveDAL iInvMoveDAL;
		public IInvMoveDAL IInvMoveDAL 
		{ 
			get
			{
				if(iInvMoveDAL==null)
				{
					iInvMoveDAL=new InvMoveDAL();	
				}
				return iInvMoveDAL;
			}
		 }
		#endregion

		#region 44 数据接口 IInvMoveDetailDAL
		IInvMoveDetailDAL iInvMoveDetailDAL;
		public IInvMoveDetailDAL IInvMoveDetailDAL 
		{ 
			get
			{
				if(iInvMoveDetailDAL==null)
				{
					iInvMoveDetailDAL=new InvMoveDetailDAL();	
				}
				return iInvMoveDetailDAL;
			}
		 }
		#endregion

		#region 45 数据接口 IItemMasterDAL
		IItemMasterDAL iItemMasterDAL;
		public IItemMasterDAL IItemMasterDAL 
		{ 
			get
			{
				if(iItemMasterDAL==null)
				{
					iItemMasterDAL=new ItemMasterDAL();	
				}
				return iItemMasterDAL;
			}
		 }
		#endregion

		#region 46 数据接口 IItemMasterColorCodeDAL
		IItemMasterColorCodeDAL iItemMasterColorCodeDAL;
		public IItemMasterColorCodeDAL IItemMasterColorCodeDAL 
		{ 
			get
			{
				if(iItemMasterColorCodeDAL==null)
				{
					iItemMasterColorCodeDAL=new ItemMasterColorCodeDAL();	
				}
				return iItemMasterColorCodeDAL;
			}
		 }
		#endregion

		#region 47 数据接口 IItemMasterExtendOMDAL
		IItemMasterExtendOMDAL iItemMasterExtendOMDAL;
		public IItemMasterExtendOMDAL IItemMasterExtendOMDAL 
		{ 
			get
			{
				if(iItemMasterExtendOMDAL==null)
				{
					iItemMasterExtendOMDAL=new ItemMasterExtendOMDAL();	
				}
				return iItemMasterExtendOMDAL;
			}
		 }
		#endregion

		#region 48 数据接口 IKmartWaitingReasonDAL
		IKmartWaitingReasonDAL iKmartWaitingReasonDAL;
		public IKmartWaitingReasonDAL IKmartWaitingReasonDAL 
		{ 
			get
			{
				if(iKmartWaitingReasonDAL==null)
				{
					iKmartWaitingReasonDAL=new KmartWaitingReasonDAL();	
				}
				return iKmartWaitingReasonDAL;
			}
		 }
		#endregion

		#region 49 数据接口 ILoadChargeDAL
		ILoadChargeDAL iLoadChargeDAL;
		public ILoadChargeDAL ILoadChargeDAL 
		{ 
			get
			{
				if(iLoadChargeDAL==null)
				{
					iLoadChargeDAL=new LoadChargeDAL();	
				}
				return iLoadChargeDAL;
			}
		 }
		#endregion

		#region 50 数据接口 ILoadChargeDaiDianDAL
		ILoadChargeDaiDianDAL iLoadChargeDaiDianDAL;
		public ILoadChargeDaiDianDAL ILoadChargeDaiDianDAL 
		{ 
			get
			{
				if(iLoadChargeDaiDianDAL==null)
				{
					iLoadChargeDaiDianDAL=new LoadChargeDaiDianDAL();	
				}
				return iLoadChargeDaiDianDAL;
			}
		 }
		#endregion

		#region 51 数据接口 ILoadChargeDetailDAL
		ILoadChargeDetailDAL iLoadChargeDetailDAL;
		public ILoadChargeDetailDAL ILoadChargeDetailDAL 
		{ 
			get
			{
				if(iLoadChargeDetailDAL==null)
				{
					iLoadChargeDetailDAL=new LoadChargeDetailDAL();	
				}
				return iLoadChargeDetailDAL;
			}
		 }
		#endregion

		#region 52 数据接口 ILoadChargeRuleDAL
		ILoadChargeRuleDAL iLoadChargeRuleDAL;
		public ILoadChargeRuleDAL ILoadChargeRuleDAL 
		{ 
			get
			{
				if(iLoadChargeRuleDAL==null)
				{
					iLoadChargeRuleDAL=new LoadChargeRuleDAL();	
				}
				return iLoadChargeRuleDAL;
			}
		 }
		#endregion

		#region 53 数据接口 ILoadContainerDetailExtendDAL
		ILoadContainerDetailExtendDAL iLoadContainerDetailExtendDAL;
		public ILoadContainerDetailExtendDAL ILoadContainerDetailExtendDAL 
		{ 
			get
			{
				if(iLoadContainerDetailExtendDAL==null)
				{
					iLoadContainerDetailExtendDAL=new LoadContainerDetailExtendDAL();	
				}
				return iLoadContainerDetailExtendDAL;
			}
		 }
		#endregion

		#region 54 数据接口 ILoadContainerExtendDAL
		ILoadContainerExtendDAL iLoadContainerExtendDAL;
		public ILoadContainerExtendDAL ILoadContainerExtendDAL 
		{ 
			get
			{
				if(iLoadContainerExtendDAL==null)
				{
					iLoadContainerExtendDAL=new LoadContainerExtendDAL();	
				}
				return iLoadContainerExtendDAL;
			}
		 }
		#endregion

		#region 55 数据接口 ILoadContainerExtendHuDetailDAL
		ILoadContainerExtendHuDetailDAL iLoadContainerExtendHuDetailDAL;
		public ILoadContainerExtendHuDetailDAL ILoadContainerExtendHuDetailDAL 
		{ 
			get
			{
				if(iLoadContainerExtendHuDetailDAL==null)
				{
					iLoadContainerExtendHuDetailDAL=new LoadContainerExtendHuDetailDAL();	
				}
				return iLoadContainerExtendHuDetailDAL;
			}
		 }
		#endregion

		#region 56 数据接口 ILoadContainerTypeDAL
		ILoadContainerTypeDAL iLoadContainerTypeDAL;
		public ILoadContainerTypeDAL ILoadContainerTypeDAL 
		{ 
			get
			{
				if(iLoadContainerTypeDAL==null)
				{
					iLoadContainerTypeDAL=new LoadContainerTypeDAL();	
				}
				return iLoadContainerTypeDAL;
			}
		 }
		#endregion

		#region 57 数据接口 ILoadCreateRuleDAL
		ILoadCreateRuleDAL iLoadCreateRuleDAL;
		public ILoadCreateRuleDAL ILoadCreateRuleDAL 
		{ 
			get
			{
				if(iLoadCreateRuleDAL==null)
				{
					iLoadCreateRuleDAL=new LoadCreateRuleDAL();	
				}
				return iLoadCreateRuleDAL;
			}
		 }
		#endregion

		#region 58 数据接口 ILoadDetailDAL
		ILoadDetailDAL iLoadDetailDAL;
		public ILoadDetailDAL ILoadDetailDAL 
		{ 
			get
			{
				if(iLoadDetailDAL==null)
				{
					iLoadDetailDAL=new LoadDetailDAL();	
				}
				return iLoadDetailDAL;
			}
		 }
		#endregion

		#region 59 数据接口 ILoadHuIdExtendDAL
		ILoadHuIdExtendDAL iLoadHuIdExtendDAL;
		public ILoadHuIdExtendDAL ILoadHuIdExtendDAL 
		{ 
			get
			{
				if(iLoadHuIdExtendDAL==null)
				{
					iLoadHuIdExtendDAL=new LoadHuIdExtendDAL();	
				}
				return iLoadHuIdExtendDAL;
			}
		 }
		#endregion

		#region 60 数据接口 ILoadMasterDAL
		ILoadMasterDAL iLoadMasterDAL;
		public ILoadMasterDAL ILoadMasterDAL 
		{ 
			get
			{
				if(iLoadMasterDAL==null)
				{
					iLoadMasterDAL=new LoadMasterDAL();	
				}
				return iLoadMasterDAL;
			}
		 }
		#endregion

		#region 61 数据接口 ILocationTypeDAL
		ILocationTypeDAL iLocationTypeDAL;
		public ILocationTypeDAL ILocationTypeDAL 
		{ 
			get
			{
				if(iLocationTypeDAL==null)
				{
					iLocationTypeDAL=new LocationTypeDAL();	
				}
				return iLocationTypeDAL;
			}
		 }
		#endregion

		#region 62 数据接口 ILocationTypesDetailDAL
		ILocationTypesDetailDAL iLocationTypesDetailDAL;
		public ILocationTypesDetailDAL ILocationTypesDetailDAL 
		{ 
			get
			{
				if(iLocationTypesDetailDAL==null)
				{
					iLocationTypesDetailDAL=new LocationTypesDetailDAL();	
				}
				return iLocationTypesDetailDAL;
			}
		 }
		#endregion

		#region 63 数据接口 ILoginLogDAL
		ILoginLogDAL iLoginLogDAL;
		public ILoginLogDAL ILoginLogDAL 
		{ 
			get
			{
				if(iLoginLogDAL==null)
				{
					iLoginLogDAL=new LoginLogDAL();	
				}
				return iLoginLogDAL;
			}
		 }
		#endregion

		#region 64 数据接口 ILookUpDAL
		ILookUpDAL iLookUpDAL;
		public ILookUpDAL ILookUpDAL 
		{ 
			get
			{
				if(iLookUpDAL==null)
				{
					iLookUpDAL=new LookUpDAL();	
				}
				return iLookUpDAL;
			}
		 }
		#endregion

		#region 65 数据接口 ILossDAL
		ILossDAL iLossDAL;
		public ILossDAL ILossDAL 
		{ 
			get
			{
				if(iLossDAL==null)
				{
					iLossDAL=new LossDAL();	
				}
				return iLossDAL;
			}
		 }
		#endregion

		#region 66 数据接口 INightTimeDAL
		INightTimeDAL iNightTimeDAL;
		public INightTimeDAL INightTimeDAL 
		{ 
			get
			{
				if(iNightTimeDAL==null)
				{
					iNightTimeDAL=new NightTimeDAL();	
				}
				return iNightTimeDAL;
			}
		 }
		#endregion

		#region 67 数据接口 IOMSInvChangeDAL
		IOMSInvChangeDAL iOMSInvChangeDAL;
		public IOMSInvChangeDAL IOMSInvChangeDAL 
		{ 
			get
			{
				if(iOMSInvChangeDAL==null)
				{
					iOMSInvChangeDAL=new OMSInvChangeDAL();	
				}
				return iOMSInvChangeDAL;
			}
		 }
		#endregion

		#region 68 数据接口 IOutBoundOrderDAL
		IOutBoundOrderDAL iOutBoundOrderDAL;
		public IOutBoundOrderDAL IOutBoundOrderDAL 
		{ 
			get
			{
				if(iOutBoundOrderDAL==null)
				{
					iOutBoundOrderDAL=new OutBoundOrderDAL();	
				}
				return iOutBoundOrderDAL;
			}
		 }
		#endregion

		#region 69 数据接口 IOutBoundOrderDetailDAL
		IOutBoundOrderDetailDAL iOutBoundOrderDetailDAL;
		public IOutBoundOrderDetailDAL IOutBoundOrderDetailDAL 
		{ 
			get
			{
				if(iOutBoundOrderDetailDAL==null)
				{
					iOutBoundOrderDetailDAL=new OutBoundOrderDetailDAL();	
				}
				return iOutBoundOrderDetailDAL;
			}
		 }
		#endregion

		#region 70 数据接口 IPackDetailDAL
		IPackDetailDAL iPackDetailDAL;
		public IPackDetailDAL IPackDetailDAL 
		{ 
			get
			{
				if(iPackDetailDAL==null)
				{
					iPackDetailDAL=new PackDetailDAL();	
				}
				return iPackDetailDAL;
			}
		 }
		#endregion

		#region 71 数据接口 IPackHeadDAL
		IPackHeadDAL iPackHeadDAL;
		public IPackHeadDAL IPackHeadDAL 
		{ 
			get
			{
				if(iPackHeadDAL==null)
				{
					iPackHeadDAL=new PackHeadDAL();	
				}
				return iPackHeadDAL;
			}
		 }
		#endregion

		#region 72 数据接口 IPackHeadJsonDAL
		IPackHeadJsonDAL iPackHeadJsonDAL;
		public IPackHeadJsonDAL IPackHeadJsonDAL 
		{ 
			get
			{
				if(iPackHeadJsonDAL==null)
				{
					iPackHeadJsonDAL=new PackHeadJsonDAL();	
				}
				return iPackHeadJsonDAL;
			}
		 }
		#endregion

		#region 73 数据接口 IPackScanNumberDAL
		IPackScanNumberDAL iPackScanNumberDAL;
		public IPackScanNumberDAL IPackScanNumberDAL 
		{ 
			get
			{
				if(iPackScanNumberDAL==null)
				{
					iPackScanNumberDAL=new PackScanNumberDAL();	
				}
				return iPackScanNumberDAL;
			}
		 }
		#endregion

		#region 74 数据接口 IPackTaskDAL
		IPackTaskDAL iPackTaskDAL;
		public IPackTaskDAL IPackTaskDAL 
		{ 
			get
			{
				if(iPackTaskDAL==null)
				{
					iPackTaskDAL=new PackTaskDAL();	
				}
				return iPackTaskDAL;
			}
		 }
		#endregion

		#region 75 数据接口 IPackTaskJsonDAL
		IPackTaskJsonDAL iPackTaskJsonDAL;
		public IPackTaskJsonDAL IPackTaskJsonDAL 
		{ 
			get
			{
				if(iPackTaskJsonDAL==null)
				{
					iPackTaskJsonDAL=new PackTaskJsonDAL();	
				}
				return iPackTaskJsonDAL;
			}
		 }
		#endregion

		#region 76 数据接口 IPallateDAL
		IPallateDAL iPallateDAL;
		public IPallateDAL IPallateDAL 
		{ 
			get
			{
				if(iPallateDAL==null)
				{
					iPallateDAL=new PallateDAL();	
				}
				return iPallateDAL;
			}
		 }
		#endregion

		#region 77 数据接口 IPallateTypeDAL
		IPallateTypeDAL iPallateTypeDAL;
		public IPallateTypeDAL IPallateTypeDAL 
		{ 
			get
			{
				if(iPallateTypeDAL==null)
				{
					iPallateTypeDAL=new PallateTypeDAL();	
				}
				return iPallateTypeDAL;
			}
		 }
		#endregion

		#region 78 数据接口 IPhotoMasterDAL
		IPhotoMasterDAL iPhotoMasterDAL;
		public IPhotoMasterDAL IPhotoMasterDAL 
		{ 
			get
			{
				if(iPhotoMasterDAL==null)
				{
					iPhotoMasterDAL=new PhotoMasterDAL();	
				}
				return iPhotoMasterDAL;
			}
		 }
		#endregion

		#region 79 数据接口 IPickTaskDetailDAL
		IPickTaskDetailDAL iPickTaskDetailDAL;
		public IPickTaskDetailDAL IPickTaskDetailDAL 
		{ 
			get
			{
				if(iPickTaskDetailDAL==null)
				{
					iPickTaskDetailDAL=new PickTaskDetailDAL();	
				}
				return iPickTaskDetailDAL;
			}
		 }
		#endregion

		#region 80 数据接口 IPlatformUserInfoDAL
		IPlatformUserInfoDAL iPlatformUserInfoDAL;
		public IPlatformUserInfoDAL IPlatformUserInfoDAL 
		{ 
			get
			{
				if(iPlatformUserInfoDAL==null)
				{
					iPlatformUserInfoDAL=new PlatformUserInfoDAL();	
				}
				return iPlatformUserInfoDAL;
			}
		 }
		#endregion

		#region 81 数据接口 IR_Client_FlowRuleDAL
		IR_Client_FlowRuleDAL iR_Client_FlowRuleDAL;
		public IR_Client_FlowRuleDAL IR_Client_FlowRuleDAL 
		{ 
			get
			{
				if(iR_Client_FlowRuleDAL==null)
				{
					iR_Client_FlowRuleDAL=new R_Client_FlowRuleDAL();	
				}
				return iR_Client_FlowRuleDAL;
			}
		 }
		#endregion

		#region 82 数据接口 IR_LoadRule_FlowHeadDAL
		IR_LoadRule_FlowHeadDAL iR_LoadRule_FlowHeadDAL;
		public IR_LoadRule_FlowHeadDAL IR_LoadRule_FlowHeadDAL 
		{ 
			get
			{
				if(iR_LoadRule_FlowHeadDAL==null)
				{
					iR_LoadRule_FlowHeadDAL=new R_LoadRule_FlowHeadDAL();	
				}
				return iR_LoadRule_FlowHeadDAL;
			}
		 }
		#endregion

		#region 83 数据接口 IR_Location_ItemDAL
		IR_Location_ItemDAL iR_Location_ItemDAL;
		public IR_Location_ItemDAL IR_Location_ItemDAL 
		{ 
			get
			{
				if(iR_Location_ItemDAL==null)
				{
					iR_Location_ItemDAL=new R_Location_ItemDAL();	
				}
				return iR_Location_ItemDAL;
			}
		 }
		#endregion

		#region 84 数据接口 IR_Location_Item_RGDAL
		IR_Location_Item_RGDAL iR_Location_Item_RGDAL;
		public IR_Location_Item_RGDAL IR_Location_Item_RGDAL 
		{ 
			get
			{
				if(iR_Location_Item_RGDAL==null)
				{
					iR_Location_Item_RGDAL=new R_Location_Item_RGDAL();	
				}
				return iR_Location_Item_RGDAL;
			}
		 }
		#endregion

		#region 85 数据接口 IR_SerialNumberInOutDAL
		IR_SerialNumberInOutDAL iR_SerialNumberInOutDAL;
		public IR_SerialNumberInOutDAL IR_SerialNumberInOutDAL 
		{ 
			get
			{
				if(iR_SerialNumberInOutDAL==null)
				{
					iR_SerialNumberInOutDAL=new R_SerialNumberInOutDAL();	
				}
				return iR_SerialNumberInOutDAL;
			}
		 }
		#endregion

		#region 86 数据接口 IR_WhClient_WhAgentDAL
		IR_WhClient_WhAgentDAL iR_WhClient_WhAgentDAL;
		public IR_WhClient_WhAgentDAL IR_WhClient_WhAgentDAL 
		{ 
			get
			{
				if(iR_WhClient_WhAgentDAL==null)
				{
					iR_WhClient_WhAgentDAL=new R_WhClient_WhAgentDAL();	
				}
				return iR_WhClient_WhAgentDAL;
			}
		 }
		#endregion

		#region 87 数据接口 IR_WhInfo_WhUserDAL
		IR_WhInfo_WhUserDAL iR_WhInfo_WhUserDAL;
		public IR_WhInfo_WhUserDAL IR_WhInfo_WhUserDAL 
		{ 
			get
			{
				if(iR_WhInfo_WhUserDAL==null)
				{
					iR_WhInfo_WhUserDAL=new R_WhInfo_WhUserDAL();	
				}
				return iR_WhInfo_WhUserDAL;
			}
		 }
		#endregion

		#region 88 数据接口 IReceiptDAL
		IReceiptDAL iReceiptDAL;
		public IReceiptDAL IReceiptDAL 
		{ 
			get
			{
				if(iReceiptDAL==null)
				{
					iReceiptDAL=new ReceiptDAL();	
				}
				return iReceiptDAL;
			}
		 }
		#endregion

		#region 89 数据接口 IReceiptChargeDAL
		IReceiptChargeDAL iReceiptChargeDAL;
		public IReceiptChargeDAL IReceiptChargeDAL 
		{ 
			get
			{
				if(iReceiptChargeDAL==null)
				{
					iReceiptChargeDAL=new ReceiptChargeDAL();	
				}
				return iReceiptChargeDAL;
			}
		 }
		#endregion

		#region 90 数据接口 IReceiptChargeDetailDAL
		IReceiptChargeDetailDAL iReceiptChargeDetailDAL;
		public IReceiptChargeDetailDAL IReceiptChargeDetailDAL 
		{ 
			get
			{
				if(iReceiptChargeDetailDAL==null)
				{
					iReceiptChargeDetailDAL=new ReceiptChargeDetailDAL();	
				}
				return iReceiptChargeDetailDAL;
			}
		 }
		#endregion

		#region 91 数据接口 IReceiptPartialRegisterDAL
		IReceiptPartialRegisterDAL iReceiptPartialRegisterDAL;
		public IReceiptPartialRegisterDAL IReceiptPartialRegisterDAL 
		{ 
			get
			{
				if(iReceiptPartialRegisterDAL==null)
				{
					iReceiptPartialRegisterDAL=new ReceiptPartialRegisterDAL();	
				}
				return iReceiptPartialRegisterDAL;
			}
		 }
		#endregion

		#region 92 数据接口 IReceiptPartialRegisterDetailDAL
		IReceiptPartialRegisterDetailDAL iReceiptPartialRegisterDetailDAL;
		public IReceiptPartialRegisterDetailDAL IReceiptPartialRegisterDetailDAL 
		{ 
			get
			{
				if(iReceiptPartialRegisterDetailDAL==null)
				{
					iReceiptPartialRegisterDetailDAL=new ReceiptPartialRegisterDetailDAL();	
				}
				return iReceiptPartialRegisterDetailDAL;
			}
		 }
		#endregion

		#region 93 数据接口 IReceiptRegisterDAL
		IReceiptRegisterDAL iReceiptRegisterDAL;
		public IReceiptRegisterDAL IReceiptRegisterDAL 
		{ 
			get
			{
				if(iReceiptRegisterDAL==null)
				{
					iReceiptRegisterDAL=new ReceiptRegisterDAL();	
				}
				return iReceiptRegisterDAL;
			}
		 }
		#endregion

		#region 94 数据接口 IReceiptRegisterDetailDAL
		IReceiptRegisterDetailDAL iReceiptRegisterDetailDAL;
		public IReceiptRegisterDetailDAL IReceiptRegisterDetailDAL 
		{ 
			get
			{
				if(iReceiptRegisterDetailDAL==null)
				{
					iReceiptRegisterDetailDAL=new ReceiptRegisterDetailDAL();	
				}
				return iReceiptRegisterDetailDAL;
			}
		 }
		#endregion

		#region 95 数据接口 IReceiptRegisterExtendDAL
		IReceiptRegisterExtendDAL iReceiptRegisterExtendDAL;
		public IReceiptRegisterExtendDAL IReceiptRegisterExtendDAL 
		{ 
			get
			{
				if(iReceiptRegisterExtendDAL==null)
				{
					iReceiptRegisterExtendDAL=new ReceiptRegisterExtendDAL();	
				}
				return iReceiptRegisterExtendDAL;
			}
		 }
		#endregion

		#region 96 数据接口 IReceiptTruckDAL
		IReceiptTruckDAL iReceiptTruckDAL;
		public IReceiptTruckDAL IReceiptTruckDAL 
		{ 
			get
			{
				if(iReceiptTruckDAL==null)
				{
					iReceiptTruckDAL=new ReceiptTruckDAL();	
				}
				return iReceiptTruckDAL;
			}
		 }
		#endregion

		#region 97 数据接口 IRecLossDAL
		IRecLossDAL iRecLossDAL;
		public IRecLossDAL IRecLossDAL 
		{ 
			get
			{
				if(iRecLossDAL==null)
				{
					iRecLossDAL=new RecLossDAL();	
				}
				return iRecLossDAL;
			}
		 }
		#endregion

		#region 98 数据接口 IRecLossTypeDAL
		IRecLossTypeDAL iRecLossTypeDAL;
		public IRecLossTypeDAL IRecLossTypeDAL 
		{ 
			get
			{
				if(iRecLossTypeDAL==null)
				{
					iRecLossTypeDAL=new RecLossTypeDAL();	
				}
				return iRecLossTypeDAL;
			}
		 }
		#endregion

		#region 99 数据接口 IReleaseLoadDetailDAL
		IReleaseLoadDetailDAL iReleaseLoadDetailDAL;
		public IReleaseLoadDetailDAL IReleaseLoadDetailDAL 
		{ 
			get
			{
				if(iReleaseLoadDetailDAL==null)
				{
					iReleaseLoadDetailDAL=new ReleaseLoadDetailDAL();	
				}
				return iReleaseLoadDetailDAL;
			}
		 }
		#endregion

		#region 100 数据接口 IRFFlowRuleDAL
		IRFFlowRuleDAL iRFFlowRuleDAL;
		public IRFFlowRuleDAL IRFFlowRuleDAL 
		{ 
			get
			{
				if(iRFFlowRuleDAL==null)
				{
					iRFFlowRuleDAL=new RFFlowRuleDAL();	
				}
				return iRFFlowRuleDAL;
			}
		 }
		#endregion

		#region 101 数据接口 ISerialNumberDetailDAL
		ISerialNumberDetailDAL iSerialNumberDetailDAL;
		public ISerialNumberDetailDAL ISerialNumberDetailDAL 
		{ 
			get
			{
				if(iSerialNumberDetailDAL==null)
				{
					iSerialNumberDetailDAL=new SerialNumberDetailDAL();	
				}
				return iSerialNumberDetailDAL;
			}
		 }
		#endregion

		#region 102 数据接口 ISerialNumberInDAL
		ISerialNumberInDAL iSerialNumberInDAL;
		public ISerialNumberInDAL ISerialNumberInDAL 
		{ 
			get
			{
				if(iSerialNumberInDAL==null)
				{
					iSerialNumberInDAL=new SerialNumberInDAL();	
				}
				return iSerialNumberInDAL;
			}
		 }
		#endregion

		#region 103 数据接口 ISerialNumberInOutExtendDAL
		ISerialNumberInOutExtendDAL iSerialNumberInOutExtendDAL;
		public ISerialNumberInOutExtendDAL ISerialNumberInOutExtendDAL 
		{ 
			get
			{
				if(iSerialNumberInOutExtendDAL==null)
				{
					iSerialNumberInOutExtendDAL=new SerialNumberInOutExtendDAL();	
				}
				return iSerialNumberInOutExtendDAL;
			}
		 }
		#endregion

		#region 104 数据接口 ISerialNumberOutDAL
		ISerialNumberOutDAL iSerialNumberOutDAL;
		public ISerialNumberOutDAL ISerialNumberOutDAL 
		{ 
			get
			{
				if(iSerialNumberOutDAL==null)
				{
					iSerialNumberOutDAL=new SerialNumberOutDAL();	
				}
				return iSerialNumberOutDAL;
			}
		 }
		#endregion

		#region 105 数据接口 ISortTaskDAL
		ISortTaskDAL iSortTaskDAL;
		public ISortTaskDAL ISortTaskDAL 
		{ 
			get
			{
				if(iSortTaskDAL==null)
				{
					iSortTaskDAL=new SortTaskDAL();	
				}
				return iSortTaskDAL;
			}
		 }
		#endregion

		#region 106 数据接口 ISortTaskDetailDAL
		ISortTaskDetailDAL iSortTaskDetailDAL;
		public ISortTaskDetailDAL ISortTaskDetailDAL 
		{ 
			get
			{
				if(iSortTaskDetailDAL==null)
				{
					iSortTaskDetailDAL=new SortTaskDetailDAL();	
				}
				return iSortTaskDetailDAL;
			}
		 }
		#endregion

		#region 107 数据接口 IStringrRgularDAL
		IStringrRgularDAL iStringrRgularDAL;
		public IStringrRgularDAL IStringrRgularDAL 
		{ 
			get
			{
				if(iStringrRgularDAL==null)
				{
					iStringrRgularDAL=new StringrRgularDAL();	
				}
				return iStringrRgularDAL;
			}
		 }
		#endregion

		#region 108 数据接口 ISupplementTaskDAL
		ISupplementTaskDAL iSupplementTaskDAL;
		public ISupplementTaskDAL ISupplementTaskDAL 
		{ 
			get
			{
				if(iSupplementTaskDAL==null)
				{
					iSupplementTaskDAL=new SupplementTaskDAL();	
				}
				return iSupplementTaskDAL;
			}
		 }
		#endregion

		#region 109 数据接口 ISupplementTaskDetailDAL
		ISupplementTaskDetailDAL iSupplementTaskDetailDAL;
		public ISupplementTaskDetailDAL ISupplementTaskDetailDAL 
		{ 
			get
			{
				if(iSupplementTaskDetailDAL==null)
				{
					iSupplementTaskDetailDAL=new SupplementTaskDetailDAL();	
				}
				return iSupplementTaskDetailDAL;
			}
		 }
		#endregion

		#region 110 数据接口 ITCRProcessDAL
		ITCRProcessDAL iTCRProcessDAL;
		public ITCRProcessDAL ITCRProcessDAL 
		{ 
			get
			{
				if(iTCRProcessDAL==null)
				{
					iTCRProcessDAL=new TCRProcessDAL();	
				}
				return iTCRProcessDAL;
			}
		 }
		#endregion

		#region 111 数据接口 ITranLogDAL
		ITranLogDAL iTranLogDAL;
		public ITranLogDAL ITranLogDAL 
		{ 
			get
			{
				if(iTranLogDAL==null)
				{
					iTranLogDAL=new TranLogDAL();	
				}
				return iTranLogDAL;
			}
		 }
		#endregion

		#region 112 数据接口 ITransferDetailDAL
		ITransferDetailDAL iTransferDetailDAL;
		public ITransferDetailDAL ITransferDetailDAL 
		{ 
			get
			{
				if(iTransferDetailDAL==null)
				{
					iTransferDetailDAL=new TransferDetailDAL();	
				}
				return iTransferDetailDAL;
			}
		 }
		#endregion

		#region 113 数据接口 ITransferHeadDAL
		ITransferHeadDAL iTransferHeadDAL;
		public ITransferHeadDAL ITransferHeadDAL 
		{ 
			get
			{
				if(iTransferHeadDAL==null)
				{
					iTransferHeadDAL=new TransferHeadDAL();	
				}
				return iTransferHeadDAL;
			}
		 }
		#endregion

		#region 114 数据接口 ITransferScanNumberDAL
		ITransferScanNumberDAL iTransferScanNumberDAL;
		public ITransferScanNumberDAL ITransferScanNumberDAL 
		{ 
			get
			{
				if(iTransferScanNumberDAL==null)
				{
					iTransferScanNumberDAL=new TransferScanNumberDAL();	
				}
				return iTransferScanNumberDAL;
			}
		 }
		#endregion

		#region 115 数据接口 ITransferTaskDAL
		ITransferTaskDAL iTransferTaskDAL;
		public ITransferTaskDAL ITransferTaskDAL 
		{ 
			get
			{
				if(iTransferTaskDAL==null)
				{
					iTransferTaskDAL=new TransferTaskDAL();	
				}
				return iTransferTaskDAL;
			}
		 }
		#endregion

		#region 116 数据接口 ITruckQueueDetailDAL
		ITruckQueueDetailDAL iTruckQueueDetailDAL;
		public ITruckQueueDetailDAL ITruckQueueDetailDAL 
		{ 
			get
			{
				if(iTruckQueueDetailDAL==null)
				{
					iTruckQueueDetailDAL=new TruckQueueDetailDAL();	
				}
				return iTruckQueueDetailDAL;
			}
		 }
		#endregion

		#region 117 数据接口 ITruckQueueHeadDAL
		ITruckQueueHeadDAL iTruckQueueHeadDAL;
		public ITruckQueueHeadDAL ITruckQueueHeadDAL 
		{ 
			get
			{
				if(iTruckQueueHeadDAL==null)
				{
					iTruckQueueHeadDAL=new TruckQueueHeadDAL();	
				}
				return iTruckQueueHeadDAL;
			}
		 }
		#endregion

		#region 118 数据接口 ItruckQueueViewDAL
		ItruckQueueViewDAL itruckQueueViewDAL;
		public ItruckQueueViewDAL ItruckQueueViewDAL 
		{ 
			get
			{
				if(itruckQueueViewDAL==null)
				{
					itruckQueueViewDAL=new truckQueueViewDAL();	
				}
				return itruckQueueViewDAL;
			}
		 }
		#endregion

		#region 119 数据接口 IUnitDAL
		IUnitDAL iUnitDAL;
		public IUnitDAL IUnitDAL 
		{ 
			get
			{
				if(iUnitDAL==null)
				{
					iUnitDAL=new UnitDAL();	
				}
				return iUnitDAL;
			}
		 }
		#endregion

		#region 120 数据接口 IUnitDefaultDAL
		IUnitDefaultDAL iUnitDefaultDAL;
		public IUnitDefaultDAL IUnitDefaultDAL 
		{ 
			get
			{
				if(iUnitDefaultDAL==null)
				{
					iUnitDefaultDAL=new UnitDefaultDAL();	
				}
				return iUnitDefaultDAL;
			}
		 }
		#endregion

		#region 121 数据接口 IUrlEdiDAL
		IUrlEdiDAL iUrlEdiDAL;
		public IUrlEdiDAL IUrlEdiDAL 
		{ 
			get
			{
				if(iUrlEdiDAL==null)
				{
					iUrlEdiDAL=new UrlEdiDAL();	
				}
				return iUrlEdiDAL;
			}
		 }
		#endregion

		#region 122 数据接口 IUrlEdiTaskDAL
		IUrlEdiTaskDAL iUrlEdiTaskDAL;
		public IUrlEdiTaskDAL IUrlEdiTaskDAL 
		{ 
			get
			{
				if(iUrlEdiTaskDAL==null)
				{
					iUrlEdiTaskDAL=new UrlEdiTaskDAL();	
				}
				return iUrlEdiTaskDAL;
			}
		 }
		#endregion

		#region 123 数据接口 IUserPriceTDAL
		IUserPriceTDAL iUserPriceTDAL;
		public IUserPriceTDAL IUserPriceTDAL 
		{ 
			get
			{
				if(iUserPriceTDAL==null)
				{
					iUserPriceTDAL=new UserPriceTDAL();	
				}
				return iUserPriceTDAL;
			}
		 }
		#endregion

		#region 124 数据接口 IUserShenTDAL
		IUserShenTDAL iUserShenTDAL;
		public IUserShenTDAL IUserShenTDAL 
		{ 
			get
			{
				if(iUserShenTDAL==null)
				{
					iUserShenTDAL=new UserShenTDAL();	
				}
				return iUserShenTDAL;
			}
		 }
		#endregion

		#region 125 数据接口 IWhAgentDAL
		IWhAgentDAL iWhAgentDAL;
		public IWhAgentDAL IWhAgentDAL 
		{ 
			get
			{
				if(iWhAgentDAL==null)
				{
					iWhAgentDAL=new WhAgentDAL();	
				}
				return iWhAgentDAL;
			}
		 }
		#endregion

		#region 126 数据接口 IWhClientDAL
		IWhClientDAL iWhClientDAL;
		public IWhClientDAL IWhClientDAL 
		{ 
			get
			{
				if(iWhClientDAL==null)
				{
					iWhClientDAL=new WhClientDAL();	
				}
				return iWhClientDAL;
			}
		 }
		#endregion

		#region 127 数据接口 IWhClientExtendDAL
		IWhClientExtendDAL iWhClientExtendDAL;
		public IWhClientExtendDAL IWhClientExtendDAL 
		{ 
			get
			{
				if(iWhClientExtendDAL==null)
				{
					iWhClientExtendDAL=new WhClientExtendDAL();	
				}
				return iWhClientExtendDAL;
			}
		 }
		#endregion

		#region 128 数据接口 IWhClientTypeDAL
		IWhClientTypeDAL iWhClientTypeDAL;
		public IWhClientTypeDAL IWhClientTypeDAL 
		{ 
			get
			{
				if(iWhClientTypeDAL==null)
				{
					iWhClientTypeDAL=new WhClientTypeDAL();	
				}
				return iWhClientTypeDAL;
			}
		 }
		#endregion

		#region 129 数据接口 IWhCompanyDAL
		IWhCompanyDAL iWhCompanyDAL;
		public IWhCompanyDAL IWhCompanyDAL 
		{ 
			get
			{
				if(iWhCompanyDAL==null)
				{
					iWhCompanyDAL=new WhCompanyDAL();	
				}
				return iWhCompanyDAL;
			}
		 }
		#endregion

		#region 130 数据接口 IWhInfoDAL
		IWhInfoDAL iWhInfoDAL;
		public IWhInfoDAL IWhInfoDAL 
		{ 
			get
			{
				if(iWhInfoDAL==null)
				{
					iWhInfoDAL=new WhInfoDAL();	
				}
				return iWhInfoDAL;
			}
		 }
		#endregion

		#region 131 数据接口 IWhLevelDAL
		IWhLevelDAL iWhLevelDAL;
		public IWhLevelDAL IWhLevelDAL 
		{ 
			get
			{
				if(iWhLevelDAL==null)
				{
					iWhLevelDAL=new WhLevelDAL();	
				}
				return iWhLevelDAL;
			}
		 }
		#endregion

		#region 132 数据接口 IWhLocationDAL
		IWhLocationDAL iWhLocationDAL;
		public IWhLocationDAL IWhLocationDAL 
		{ 
			get
			{
				if(iWhLocationDAL==null)
				{
					iWhLocationDAL=new WhLocationDAL();	
				}
				return iWhLocationDAL;
			}
		 }
		#endregion

		#region 133 数据接口 IWhMenuDAL
		IWhMenuDAL iWhMenuDAL;
		public IWhMenuDAL IWhMenuDAL 
		{ 
			get
			{
				if(iWhMenuDAL==null)
				{
					iWhMenuDAL=new WhMenuDAL();	
				}
				return iWhMenuDAL;
			}
		 }
		#endregion

		#region 134 数据接口 IWhPositionDAL
		IWhPositionDAL iWhPositionDAL;
		public IWhPositionDAL IWhPositionDAL 
		{ 
			get
			{
				if(iWhPositionDAL==null)
				{
					iWhPositionDAL=new WhPositionDAL();	
				}
				return iWhPositionDAL;
			}
		 }
		#endregion

		#region 135 数据接口 IWhPositionPowerDAL
		IWhPositionPowerDAL iWhPositionPowerDAL;
		public IWhPositionPowerDAL IWhPositionPowerDAL 
		{ 
			get
			{
				if(iWhPositionPowerDAL==null)
				{
					iWhPositionPowerDAL=new WhPositionPowerDAL();	
				}
				return iWhPositionPowerDAL;
			}
		 }
		#endregion

		#region 136 数据接口 IWhPositionPowerMVCDAL
		IWhPositionPowerMVCDAL iWhPositionPowerMVCDAL;
		public IWhPositionPowerMVCDAL IWhPositionPowerMVCDAL 
		{ 
			get
			{
				if(iWhPositionPowerMVCDAL==null)
				{
					iWhPositionPowerMVCDAL=new WhPositionPowerMVCDAL();	
				}
				return iWhPositionPowerMVCDAL;
			}
		 }
		#endregion

		#region 137 数据接口 IWhPowerDAL
		IWhPowerDAL iWhPowerDAL;
		public IWhPowerDAL IWhPowerDAL 
		{ 
			get
			{
				if(iWhPowerDAL==null)
				{
					iWhPowerDAL=new WhPowerDAL();	
				}
				return iWhPowerDAL;
			}
		 }
		#endregion

		#region 138 数据接口 IWhUserDAL
		IWhUserDAL iWhUserDAL;
		public IWhUserDAL IWhUserDAL 
		{ 
			get
			{
				if(iWhUserDAL==null)
				{
					iWhUserDAL=new WhUserDAL();	
				}
				return iWhUserDAL;
			}
		 }
		#endregion

		#region 139 数据接口 IWhUserPositionDAL
		IWhUserPositionDAL iWhUserPositionDAL;
		public IWhUserPositionDAL IWhUserPositionDAL 
		{ 
			get
			{
				if(iWhUserPositionDAL==null)
				{
					iWhUserPositionDAL=new WhUserPositionDAL();	
				}
				return iWhUserPositionDAL;
			}
		 }
		#endregion

		#region 140 数据接口 IWorkloadAccountDAL
		IWorkloadAccountDAL iWorkloadAccountDAL;
		public IWorkloadAccountDAL IWorkloadAccountDAL 
		{ 
			get
			{
				if(iWorkloadAccountDAL==null)
				{
					iWorkloadAccountDAL=new WorkloadAccountDAL();	
				}
				return iWorkloadAccountDAL;
			}
		 }
		#endregion

		#region 141 数据接口 IZoneDAL
		IZoneDAL iZoneDAL;
		public IZoneDAL IZoneDAL 
		{ 
			get
			{
				if(iZoneDAL==null)
				{
					iZoneDAL=new ZoneDAL();	
				}
				return iZoneDAL;
			}
		 }
		#endregion

		#region 142 数据接口 IZoneLocationDAL
		IZoneLocationDAL iZoneLocationDAL;
		public IZoneLocationDAL IZoneLocationDAL 
		{ 
			get
			{
				if(iZoneLocationDAL==null)
				{
					iZoneLocationDAL=new ZoneLocationDAL();	
				}
				return iZoneLocationDAL;
			}
		 }
		#endregion

		#region 143 数据接口 IZonesExtendDAL
		IZonesExtendDAL iZonesExtendDAL;
		public IZonesExtendDAL IZonesExtendDAL 
		{ 
			get
			{
				if(iZonesExtendDAL==null)
				{
					iZonesExtendDAL=new ZonesExtendDAL();	
				}
				return iZonesExtendDAL;
			}
		 }
		#endregion
}
}
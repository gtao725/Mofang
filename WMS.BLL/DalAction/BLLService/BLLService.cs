
 using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MODEL_MSSQL;
using WMS.IBLL;

namespace WMS.BLL
{
	public partial class AddValueServiceService : BaseBLL<AddValueService>,IAddValueServiceService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().IAddValueServiceDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public AddValueService Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class BillDetailService : BaseBLL<BillDetail>,IBillDetailService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().IBillDetailDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public BillDetail Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class BillMasterService : BaseBLL<BillMaster>,IBillMasterService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().IBillMasterDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public BillMaster Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class BusinessFlowGroupService : BaseBLL<BusinessFlowGroup>,IBusinessFlowGroupService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().IBusinessFlowGroupDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public BusinessFlowGroup Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class BusinessFlowHeadService : BaseBLL<BusinessFlowHead>,IBusinessFlowHeadService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().IBusinessFlowHeadDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public BusinessFlowHead Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class BusinessObjectService : BaseBLL<BusinessObject>,IBusinessObjectService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().IBusinessObjectDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public BusinessObject Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class BusinessObjectDetailService : BaseBLL<BusinessObjectDetail>,IBusinessObjectDetailService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().IBusinessObjectDetailDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public BusinessObjectDetail Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class BusinessObjectHeadService : BaseBLL<BusinessObjectHead>,IBusinessObjectHeadService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().IBusinessObjectHeadDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public BusinessObjectHead Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class BusinessObjectItemService : BaseBLL<BusinessObjectItem>,IBusinessObjectItemService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().IBusinessObjectItemDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public BusinessObjectItem Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class ContractFormService : BaseBLL<ContractForm>,IContractFormService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().IContractFormDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public ContractForm Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class ContractFormExtendService : BaseBLL<ContractFormExtend>,IContractFormExtendService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().IContractFormExtendDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public ContractFormExtend Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class ContractFormOutService : BaseBLL<ContractFormOut>,IContractFormOutService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().IContractFormOutDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public ContractFormOut Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class CRTemplateService : BaseBLL<CRTemplate>,ICRTemplateService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().ICRTemplateDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public CRTemplate Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class CycleCountCheckService : BaseBLL<CycleCountCheck>,ICycleCountCheckService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().ICycleCountCheckDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public CycleCountCheck Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class CycleCountDetailService : BaseBLL<CycleCountDetail>,ICycleCountDetailService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().ICycleCountDetailDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public CycleCountDetail Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class CycleCountInventoryService : BaseBLL<CycleCountInventory>,ICycleCountInventoryService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().ICycleCountInventoryDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public CycleCountInventory Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class CycleCountMasterService : BaseBLL<CycleCountMaster>,ICycleCountMasterService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().ICycleCountMasterDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public CycleCountMaster Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class DamcoGRNDetailService : BaseBLL<DamcoGRNDetail>,IDamcoGRNDetailService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().IDamcoGRNDetailDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public DamcoGRNDetail Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class DamcoGRNHeadService : BaseBLL<DamcoGRNHead>,IDamcoGRNHeadService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().IDamcoGRNHeadDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public DamcoGRNHead Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class DamcoGrnRuleService : BaseBLL<DamcoGrnRule>,IDamcoGrnRuleService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().IDamcoGrnRuleDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public DamcoGrnRule Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class DcReturnExceptionService : BaseBLL<DcReturnException>,IDcReturnExceptionService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().IDcReturnExceptionDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public DcReturnException Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class DcShipingExceptionService : BaseBLL<DcShipingException>,IDcShipingExceptionService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().IDcShipingExceptionDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public DcShipingException Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class ExcelImportInBoundService : BaseBLL<ExcelImportInBound>,IExcelImportInBoundService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().IExcelImportInBoundDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public ExcelImportInBound Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class ExcelImportInBoundComService : BaseBLL<ExcelImportInBoundCom>,IExcelImportInBoundComService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().IExcelImportInBoundComDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public ExcelImportInBoundCom Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class ExcelImportInBoundCottonService : BaseBLL<ExcelImportInBoundCotton>,IExcelImportInBoundCottonService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().IExcelImportInBoundCottonDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public ExcelImportInBoundCotton Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class ExcelImportOutBoundService : BaseBLL<ExcelImportOutBound>,IExcelImportOutBoundService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().IExcelImportOutBoundDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public ExcelImportOutBound Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class FeeDetailService : BaseBLL<FeeDetail>,IFeeDetailService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().IFeeDetailDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public FeeDetail Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class FeeHolidayService : BaseBLL<FeeHoliday>,IFeeHolidayService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().IFeeHolidayDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public FeeHoliday Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class FeeMasterService : BaseBLL<FeeMaster>,IFeeMasterService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().IFeeMasterDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public FeeMaster Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class FieldOrderByService : BaseBLL<FieldOrderBy>,IFieldOrderByService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().IFieldOrderByDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public FieldOrderBy Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class FlowDetailService : BaseBLL<FlowDetail>,IFlowDetailService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().IFlowDetailDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public FlowDetail Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class FlowHeadService : BaseBLL<FlowHead>,IFlowHeadService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().IFlowHeadDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public FlowHead Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class FlowRuleService : BaseBLL<FlowRule>,IFlowRuleService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().IFlowRuleDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public FlowRule Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class Heport_R_SerialNumberInOutService : BaseBLL<Heport_R_SerialNumberInOut>,IHeport_R_SerialNumberInOutService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().IHeport_R_SerialNumberInOutDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public Heport_R_SerialNumberInOut Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class HeportSerialNumberInService : BaseBLL<HeportSerialNumberIn>,IHeportSerialNumberInService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().IHeportSerialNumberInDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public HeportSerialNumberIn Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class HoldMasterService : BaseBLL<HoldMaster>,IHoldMasterService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().IHoldMasterDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public HoldMaster Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class HolidayService : BaseBLL<Holiday>,IHolidayService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().IHolidayDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public Holiday Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class HuDetailService : BaseBLL<HuDetail>,IHuDetailService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().IHuDetailDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public HuDetail Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class HuMasterService : BaseBLL<HuMaster>,IHuMasterService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().IHuMasterDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public HuMaster Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class InBoundOrderService : BaseBLL<InBoundOrder>,IInBoundOrderService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().IInBoundOrderDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public InBoundOrder Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class InBoundOrderDetailService : BaseBLL<InBoundOrderDetail>,IInBoundOrderDetailService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().IInBoundOrderDetailDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public InBoundOrderDetail Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class InBoundSOService : BaseBLL<InBoundSO>,IInBoundSOService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().IInBoundSODAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public InBoundSO Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class InvMoveService : BaseBLL<InvMove>,IInvMoveService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().IInvMoveDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public InvMove Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class InvMoveDetailService : BaseBLL<InvMoveDetail>,IInvMoveDetailService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().IInvMoveDetailDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public InvMoveDetail Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class ItemMasterService : BaseBLL<ItemMaster>,IItemMasterService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().IItemMasterDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public ItemMaster Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class ItemMasterColorCodeService : BaseBLL<ItemMasterColorCode>,IItemMasterColorCodeService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().IItemMasterColorCodeDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public ItemMasterColorCode Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class ItemMasterExtendOMService : BaseBLL<ItemMasterExtendOM>,IItemMasterExtendOMService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().IItemMasterExtendOMDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public ItemMasterExtendOM Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class KmartWaitingReasonService : BaseBLL<KmartWaitingReason>,IKmartWaitingReasonService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().IKmartWaitingReasonDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public KmartWaitingReason Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class LoadChargeService : BaseBLL<LoadCharge>,ILoadChargeService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().ILoadChargeDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public LoadCharge Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class LoadChargeDaiDianService : BaseBLL<LoadChargeDaiDian>,ILoadChargeDaiDianService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().ILoadChargeDaiDianDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public LoadChargeDaiDian Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class LoadChargeDetailService : BaseBLL<LoadChargeDetail>,ILoadChargeDetailService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().ILoadChargeDetailDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public LoadChargeDetail Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class LoadChargeRuleService : BaseBLL<LoadChargeRule>,ILoadChargeRuleService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().ILoadChargeRuleDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public LoadChargeRule Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class LoadContainerDetailExtendService : BaseBLL<LoadContainerDetailExtend>,ILoadContainerDetailExtendService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().ILoadContainerDetailExtendDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public LoadContainerDetailExtend Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class LoadContainerExtendService : BaseBLL<LoadContainerExtend>,ILoadContainerExtendService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().ILoadContainerExtendDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public LoadContainerExtend Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class LoadContainerExtendHuDetailService : BaseBLL<LoadContainerExtendHuDetail>,ILoadContainerExtendHuDetailService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().ILoadContainerExtendHuDetailDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public LoadContainerExtendHuDetail Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class LoadContainerTypeService : BaseBLL<LoadContainerType>,ILoadContainerTypeService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().ILoadContainerTypeDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public LoadContainerType Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class LoadCreateRuleService : BaseBLL<LoadCreateRule>,ILoadCreateRuleService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().ILoadCreateRuleDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public LoadCreateRule Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class LoadDetailService : BaseBLL<LoadDetail>,ILoadDetailService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().ILoadDetailDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public LoadDetail Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class LoadHuIdExtendService : BaseBLL<LoadHuIdExtend>,ILoadHuIdExtendService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().ILoadHuIdExtendDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public LoadHuIdExtend Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class LoadMasterService : BaseBLL<LoadMaster>,ILoadMasterService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().ILoadMasterDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public LoadMaster Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class LocationTypeService : BaseBLL<LocationType>,ILocationTypeService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().ILocationTypeDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public LocationType Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class LocationTypesDetailService : BaseBLL<LocationTypesDetail>,ILocationTypesDetailService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().ILocationTypesDetailDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public LocationTypesDetail Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class LoginLogService : BaseBLL<LoginLog>,ILoginLogService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().ILoginLogDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public LoginLog Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class LookUpService : BaseBLL<LookUp>,ILookUpService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().ILookUpDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public LookUp Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class LossService : BaseBLL<Loss>,ILossService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().ILossDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public Loss Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class NightTimeService : BaseBLL<NightTime>,INightTimeService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().INightTimeDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public NightTime Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class OMSInvChangeService : BaseBLL<OMSInvChange>,IOMSInvChangeService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().IOMSInvChangeDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public OMSInvChange Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class OutBoundOrderService : BaseBLL<OutBoundOrder>,IOutBoundOrderService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().IOutBoundOrderDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public OutBoundOrder Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class OutBoundOrderDetailService : BaseBLL<OutBoundOrderDetail>,IOutBoundOrderDetailService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().IOutBoundOrderDetailDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public OutBoundOrderDetail Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class PackDetailService : BaseBLL<PackDetail>,IPackDetailService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().IPackDetailDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public PackDetail Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class PackHeadService : BaseBLL<PackHead>,IPackHeadService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().IPackHeadDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public PackHead Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class PackHeadJsonService : BaseBLL<PackHeadJson>,IPackHeadJsonService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().IPackHeadJsonDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public PackHeadJson Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class PackScanNumberService : BaseBLL<PackScanNumber>,IPackScanNumberService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().IPackScanNumberDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public PackScanNumber Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class PackTaskService : BaseBLL<PackTask>,IPackTaskService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().IPackTaskDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public PackTask Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class PackTaskJsonService : BaseBLL<PackTaskJson>,IPackTaskJsonService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().IPackTaskJsonDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public PackTaskJson Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class PallateService : BaseBLL<Pallate>,IPallateService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().IPallateDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public Pallate Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class PallateTypeService : BaseBLL<PallateType>,IPallateTypeService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().IPallateTypeDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public PallateType Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class PhotoMasterService : BaseBLL<PhotoMaster>,IPhotoMasterService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().IPhotoMasterDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public PhotoMaster Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class PickTaskDetailService : BaseBLL<PickTaskDetail>,IPickTaskDetailService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().IPickTaskDetailDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public PickTaskDetail Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class PlatformUserInfoService : BaseBLL<PlatformUserInfo>,IPlatformUserInfoService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().IPlatformUserInfoDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public PlatformUserInfo Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class R_Client_FlowRuleService : BaseBLL<R_Client_FlowRule>,IR_Client_FlowRuleService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().IR_Client_FlowRuleDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public R_Client_FlowRule Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class R_LoadRule_FlowHeadService : BaseBLL<R_LoadRule_FlowHead>,IR_LoadRule_FlowHeadService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().IR_LoadRule_FlowHeadDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public R_LoadRule_FlowHead Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class R_Location_ItemService : BaseBLL<R_Location_Item>,IR_Location_ItemService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().IR_Location_ItemDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public R_Location_Item Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class R_Location_Item_RGService : BaseBLL<R_Location_Item_RG>,IR_Location_Item_RGService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().IR_Location_Item_RGDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public R_Location_Item_RG Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class R_SerialNumberInOutService : BaseBLL<R_SerialNumberInOut>,IR_SerialNumberInOutService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().IR_SerialNumberInOutDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public R_SerialNumberInOut Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class R_WhClient_WhAgentService : BaseBLL<R_WhClient_WhAgent>,IR_WhClient_WhAgentService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().IR_WhClient_WhAgentDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public R_WhClient_WhAgent Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class R_WhInfo_WhUserService : BaseBLL<R_WhInfo_WhUser>,IR_WhInfo_WhUserService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().IR_WhInfo_WhUserDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public R_WhInfo_WhUser Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class ReceiptService : BaseBLL<Receipt>,IReceiptService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().IReceiptDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public Receipt Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class ReceiptChargeService : BaseBLL<ReceiptCharge>,IReceiptChargeService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().IReceiptChargeDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public ReceiptCharge Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class ReceiptChargeDetailService : BaseBLL<ReceiptChargeDetail>,IReceiptChargeDetailService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().IReceiptChargeDetailDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public ReceiptChargeDetail Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class ReceiptPartialRegisterService : BaseBLL<ReceiptPartialRegister>,IReceiptPartialRegisterService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().IReceiptPartialRegisterDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public ReceiptPartialRegister Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class ReceiptPartialRegisterDetailService : BaseBLL<ReceiptPartialRegisterDetail>,IReceiptPartialRegisterDetailService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().IReceiptPartialRegisterDetailDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public ReceiptPartialRegisterDetail Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class ReceiptRegisterService : BaseBLL<ReceiptRegister>,IReceiptRegisterService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().IReceiptRegisterDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public ReceiptRegister Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class ReceiptRegisterDetailService : BaseBLL<ReceiptRegisterDetail>,IReceiptRegisterDetailService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().IReceiptRegisterDetailDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public ReceiptRegisterDetail Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class ReceiptRegisterExtendService : BaseBLL<ReceiptRegisterExtend>,IReceiptRegisterExtendService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().IReceiptRegisterExtendDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public ReceiptRegisterExtend Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class ReceiptTruckService : BaseBLL<ReceiptTruck>,IReceiptTruckService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().IReceiptTruckDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public ReceiptTruck Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class RecLossService : BaseBLL<RecLoss>,IRecLossService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().IRecLossDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public RecLoss Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class RecLossTypeService : BaseBLL<RecLossType>,IRecLossTypeService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().IRecLossTypeDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public RecLossType Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class ReleaseLoadDetailService : BaseBLL<ReleaseLoadDetail>,IReleaseLoadDetailService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().IReleaseLoadDetailDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public ReleaseLoadDetail Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class RFFlowRuleService : BaseBLL<RFFlowRule>,IRFFlowRuleService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().IRFFlowRuleDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public RFFlowRule Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class SerialNumberDetailService : BaseBLL<SerialNumberDetail>,ISerialNumberDetailService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().ISerialNumberDetailDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public SerialNumberDetail Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class SerialNumberInService : BaseBLL<SerialNumberIn>,ISerialNumberInService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().ISerialNumberInDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public SerialNumberIn Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class SerialNumberInOutExtendService : BaseBLL<SerialNumberInOutExtend>,ISerialNumberInOutExtendService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().ISerialNumberInOutExtendDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public SerialNumberInOutExtend Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class SerialNumberOutService : BaseBLL<SerialNumberOut>,ISerialNumberOutService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().ISerialNumberOutDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public SerialNumberOut Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class SortTaskService : BaseBLL<SortTask>,ISortTaskService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().ISortTaskDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public SortTask Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class SortTaskDetailService : BaseBLL<SortTaskDetail>,ISortTaskDetailService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().ISortTaskDetailDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public SortTaskDetail Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class StringrRgularService : BaseBLL<StringrRgular>,IStringrRgularService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().IStringrRgularDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public StringrRgular Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class SupplementTaskService : BaseBLL<SupplementTask>,ISupplementTaskService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().ISupplementTaskDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public SupplementTask Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class SupplementTaskDetailService : BaseBLL<SupplementTaskDetail>,ISupplementTaskDetailService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().ISupplementTaskDetailDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public SupplementTaskDetail Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class TCRProcessService : BaseBLL<TCRProcess>,ITCRProcessService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().ITCRProcessDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public TCRProcess Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class TranLogService : BaseBLL<TranLog>,ITranLogService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().ITranLogDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public TranLog Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class TransferDetailService : BaseBLL<TransferDetail>,ITransferDetailService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().ITransferDetailDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public TransferDetail Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class TransferHeadService : BaseBLL<TransferHead>,ITransferHeadService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().ITransferHeadDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public TransferHead Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class TransferScanNumberService : BaseBLL<TransferScanNumber>,ITransferScanNumberService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().ITransferScanNumberDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public TransferScanNumber Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class TransferTaskService : BaseBLL<TransferTask>,ITransferTaskService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().ITransferTaskDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public TransferTask Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class TruckQueueDetailService : BaseBLL<TruckQueueDetail>,ITruckQueueDetailService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().ITruckQueueDetailDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public TruckQueueDetail Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class TruckQueueHeadService : BaseBLL<TruckQueueHead>,ITruckQueueHeadService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().ITruckQueueHeadDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public TruckQueueHead Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class truckQueueViewService : BaseBLL<truckQueueView>,ItruckQueueViewService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().ItruckQueueViewDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public truckQueueView Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class UnitService : BaseBLL<Unit>,IUnitService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().IUnitDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public Unit Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class UnitDefaultService : BaseBLL<UnitDefault>,IUnitDefaultService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().IUnitDefaultDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public UnitDefault Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class UrlEdiService : BaseBLL<UrlEdi>,IUrlEdiService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().IUrlEdiDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public UrlEdi Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class UrlEdiTaskService : BaseBLL<UrlEdiTask>,IUrlEdiTaskService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().IUrlEdiTaskDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public UrlEdiTask Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class UserPriceTService : BaseBLL<UserPriceT>,IUserPriceTService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().IUserPriceTDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public UserPriceT Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class UserShenTService : BaseBLL<UserShenT>,IUserShenTService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().IUserShenTDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public UserShenT Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class WhAgentService : BaseBLL<WhAgent>,IWhAgentService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().IWhAgentDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public WhAgent Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class WhClientService : BaseBLL<WhClient>,IWhClientService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().IWhClientDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public WhClient Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class WhClientExtendService : BaseBLL<WhClientExtend>,IWhClientExtendService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().IWhClientExtendDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public WhClientExtend Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class WhClientTypeService : BaseBLL<WhClientType>,IWhClientTypeService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().IWhClientTypeDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public WhClientType Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class WhCompanyService : BaseBLL<WhCompany>,IWhCompanyService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().IWhCompanyDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public WhCompany Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class WhInfoService : BaseBLL<WhInfo>,IWhInfoService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().IWhInfoDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public WhInfo Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class WhLevelService : BaseBLL<WhLevel>,IWhLevelService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().IWhLevelDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public WhLevel Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class WhLocationService : BaseBLL<WhLocation>,IWhLocationService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().IWhLocationDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public WhLocation Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class WhMenuService : BaseBLL<WhMenu>,IWhMenuService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().IWhMenuDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public WhMenu Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class WhPositionService : BaseBLL<WhPosition>,IWhPositionService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().IWhPositionDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public WhPosition Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class WhPositionPowerService : BaseBLL<WhPositionPower>,IWhPositionPowerService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().IWhPositionPowerDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public WhPositionPower Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class WhPositionPowerMVCService : BaseBLL<WhPositionPowerMVC>,IWhPositionPowerMVCService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().IWhPositionPowerMVCDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public WhPositionPowerMVC Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class WhPowerService : BaseBLL<WhPower>,IWhPowerService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().IWhPowerDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public WhPower Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class WhUserService : BaseBLL<WhUser>,IWhUserService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().IWhUserDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public WhUser Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class WhUserPositionService : BaseBLL<WhUserPosition>,IWhUserPositionService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().IWhUserPositionDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public WhUserPosition Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class WorkloadAccountService : BaseBLL<WorkloadAccount>,IWorkloadAccountService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().IWorkloadAccountDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public WorkloadAccount Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class ZoneService : BaseBLL<Zone>,IZoneService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().IZoneDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public Zone Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class ZoneLocationService : BaseBLL<ZoneLocation>,IZoneLocationService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().IZoneLocationDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public ZoneLocation Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }

	public partial class ZonesExtendService : BaseBLL<ZonesExtend>,IZonesExtendService
    {
	  public override void Setidal()
        {
            idal = BLLHelper.GetDal().IZonesExtendDAL;
        }

	   public override int DeleteById(int id) {
          idal.DeleteBy(u => u.Id == id);
		  return idal.SaveChanges();
      }

	   public override int DeleteByListId(List<int> delId) {
           idal.DeleteBy(u => delId.Contains(u.Id));
           return idal.SaveChanges();
       }

	      public ZonesExtend Select(int id)
       {
           return idal.SelectBy(u => u.Id == id).First();
       }
    }


}
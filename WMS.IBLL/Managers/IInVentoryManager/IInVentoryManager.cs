using MODEL_MSSQL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMS.BLLClass;

namespace WMS.IBLL
{
    public interface IInVentoryManager
    {
        //客户下拉菜单列表
        IEnumerable<WhClient> WhClientListSelect(string whCode);

        //库存查询
        //对应 C_InVentoryController 中的 List 方法
        List<InVentoryResult> C_InVentoryList(InVentorySearch searchEntity, out int total);

        //库存信息更改 查询
        //对应 C_InVentoryQuestionController 中的 List 方法
        List<InVentoryResult> C_InVentoryQuestionList(InVentorySearch searchEntity, string[] soNumber, string[] poNumber, string[] altNumber, string[] style1, string[] huId, out int total, out string str);

        //修改库存信息
        //对应 C_InVentoryQuestionController 中的 EditDetail 方法
        int HuMasterEdit(HuDetailResult entity, params string[] modifiedProNames);

        //修改库存信息
        //对应 C_InVentoryQuestionController 中的 HuMasterHuDetailEdit 方法
        string HuMasterHuDetailEdit(HuDetailResult entity);

        //托盘移库
        string HuIdRemoveLocation(string WhCode, string Location, string DestLoc, string HuId, string User);

        //新增库存
        //对应 C_AddInventoryController 中的 AddInventory 方法
        string AddInventory(HuDetailInsert entity);
        //冻结,解冻修改
        string PltHoldEdit(HuMaster entity);
        //创建收货TCR
        string InventoryTCR(HuMaster entity);

        //修改SN
        string EditInventoryExtendHuId(int huDetailId, int huMasterId, string huId);

        //创建库存整理单
        string CreateInvMove(string WhCode, string User);
        List<InvMoveDetailResult> ListInvMove(InvMoveDetailSearch searchEntity, out int total);

        //调整库存Lot
        string BatchEditHuIdLot(int[] huDetailId, string whCode, string locationId, int qty, string lot1, string lot2, string lotdate, string uesrName);

        //调整库存Lot
        string EditHuIdLot(EditHuDetailLotEntity entity, string locationId, int qty, string lot1, string lot2, string lotdate);

        //调整库存Lot
        List<EditHuDetailLotEntity> EditHuIdLot(List<EditHuDetailLotEntity> entityList);

        //异常库位批量上架
        string BatchPutHuIdABLocation(int[] huDetailId, string whCode, string locationId, string uesrName);

    }

}

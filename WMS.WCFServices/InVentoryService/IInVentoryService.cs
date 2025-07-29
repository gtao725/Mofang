using MODEL_MSSQL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using WMS.BLLClass;

namespace WMS.WCFServices.InVentoryService
{
    // 注意: 使用“重构”菜单上的“重命名”命令，可以同时更改代码和配置文件中的接口名“IInVentoryService”。
    [ServiceContract]
    public interface IInVentoryService
    {
        [OperationContract]
        //客户下拉菜单列表
        IEnumerable<WhClient> WhClientListSelect(string whCode);

        [OperationContract]
        //库存查询
        List<InVentoryResult> C_InVentoryList(InVentorySearch searchEntity, out int total);

        [OperationContract]
        //库存信息更改 查询
        List<InVentoryResult> C_InVentoryQuestionList(InVentorySearch searchEntity, string[] soNumber, string[] poNumber, string[] altNumber, string[] style1, string[] huId, out int total, out string str);

        [OperationContract]
        //修改库存信息
        int HuMasterEdit(HuDetailResult entity, params string[] modifiedProNames);

        [OperationContract]
        //修改库存信息
        string HuMasterHuDetailEdit(HuDetailResult entity);

        [OperationContract]
        //新增库存
        string AddInventory(HuDetailInsert entity);

        [OperationContract]
        //托盘移库
        string HuIdRemoveLocation(string WhCode, string Location, string DestLoc, string HuId, string User);

        [OperationContract]
        //冻结,解冻修改
        string PltHoldEdit(HuMaster huMaster);

        [OperationContract]
        //修改SN
        string EditInventoryExtendHuId(int huDetailId, int huMasterId, string huId);



        [OperationContract]
        //库存整理单查询
        List<InvMoveDetailResult> ListInvMove(InvMoveDetailSearch searchEntity, out int total);


        [OperationContract]
        string CreateInvMove(string WhCode, string User);

        [OperationContract]
        //调整库存Lot
        string BatchEditHuIdLot(int[] huDetailId, string whCode, string locationId, int qty, string lot1, string lot2, string lotdate, string uesrName);

        [OperationContract]
        //异常库位批量上架
        string BatchPutHuIdABLocation(int[] huDetailId, string whCode, string locationId, string uesrName);

    }
}

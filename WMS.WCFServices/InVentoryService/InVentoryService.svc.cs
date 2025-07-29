using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using MODEL_MSSQL;
using WMS.BLLClass;

namespace WMS.WCFServices.InVentoryService
{
    // 注意: 使用“重构”菜单上的“重命名”命令，可以同时更改代码、svc 和配置文件中的类名“InVentoryService”。
    // 注意: 为了启动 WCF 测试客户端以测试此服务，请在解决方案资源管理器中选择 InVentoryService.svc 或 InVentoryService.svc.cs，然后开始调试。
    public class InVentoryService : IInVentoryService
    {
        IBLL.IInVentoryManager inVentory = new BLL.InVentoryManager();

        public List<InVentoryResult> C_InVentoryList(InVentorySearch searchEntity, out int total)
        {
            return inVentory.C_InVentoryList(searchEntity, out total);
        }

        public List<InVentoryResult> C_InVentoryQuestionList(InVentorySearch searchEntity, string[] soNumber, string[] poNumber, string[] altNumber, string[] style1, string[] huId, out int total, out string str)
        {
            return inVentory.C_InVentoryQuestionList(searchEntity, soNumber, poNumber, altNumber, style1, huId, out total, out str);
        }

        public string HuMasterHuDetailEdit(HuDetailResult entity)
        {
            return inVentory.HuMasterHuDetailEdit(entity);
        }

        public int HuMasterEdit(HuDetailResult entity, params string[] modifiedProNames)
        {
            return inVentory.HuMasterEdit(entity, modifiedProNames);
        }

        public IEnumerable<WhClient> WhClientListSelect(string whCode)
        {
            return inVentory.WhClientListSelect(whCode);
        }

        //新增库存
        public string AddInventory(HuDetailInsert entity)
        {
            return inVentory.AddInventory(entity);
        }

        //托盘移库
        public string HuIdRemoveLocation(string WhCode, string Location, string DestLoc, string HuId, string User)
        {
            return inVentory.HuIdRemoveLocation(WhCode, Location, DestLoc, HuId, User);
        }

        public string PltHoldEdit(HuMaster entity)
        {
            return inVentory.PltHoldEdit(entity);
        }


        //修改SN
        public string EditInventoryExtendHuId(int huDetailId, int huMasterId, string huId)
        {
            return inVentory.EditInventoryExtendHuId(huDetailId, huMasterId, huId);
        }




        public List<InvMoveDetailResult> ListInvMove(InvMoveDetailSearch searchEntity, out int total)
        {
            return inVentory.ListInvMove(searchEntity, out total);
        }

        public string CreateInvMove(string WhCode, string User)
        {

            return inVentory.CreateInvMove(WhCode, User);
        }

        //调整库存Lot
        public string BatchEditHuIdLot(int[] huDetailId, string whCode, string locationId, int qty, string lot1, string lot2, string lotdate, string uesrName)
        {
            return inVentory.BatchEditHuIdLot(huDetailId, whCode, locationId, qty, lot1, lot2, lotdate, uesrName);
        }


        //异常库位批量上架
        public string BatchPutHuIdABLocation(int[] huDetailId, string whCode, string locationId, string uesrName)
        {
            return inVentory.BatchPutHuIdABLocation(huDetailId, whCode, locationId, uesrName);
        }

    }
}

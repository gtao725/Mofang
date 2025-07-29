using MODEL_MSSQL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using WMS.BLLClass;

namespace WMS.WCFServices.PackTaskService
{
    // 注意: 使用“重构”菜单上的“重命名”命令，可以同时更改代码、svc 和配置文件中的类名“PackTaskService”。
    // 注意: 为了启动 WCF 测试客户端以测试此服务，请在解决方案资源管理器中选择 PackTaskService.svc 或 PackTaskService.svc.cs，然后开始调试。
    public class PackTaskService : IPackTaskService
    {
        IBLL.IPackTaskManager packTask = new BLL.PackTaskManager();
        IBLL.IShipLoadManager shipLoad = new BLL.ShipLoadManager();
        IBLL.IRootManager root = new BLL.RootManager();

        public List<WhClient> WhClientListSelect(string whCode)
        {
            return packTask.WhClientListSelect(whCode);
        }


        #region 单品包装

        //验证 Load号是否正确
        public List<PackTask> GetPackTaskListByLoad(string loadId, string whCode)
        {
            return packTask.GetPackTaskListByLoad(loadId, whCode);
        }

        //通过 当前包装扫描的Load号 获得 其分拣明细
        public List<SortTaskDetail> GetSortTaskDetailByLoad(string loadId, string whCode)
        {
            return packTask.GetSortTaskDetailByLoad(loadId, whCode);
        }

        //拉取 计划包装总数与已包装总数 
        public List<SortTaskDetailResult> GetSumSortTaskDetailListByLoad(string loadId, string whCode)
        {
            return packTask.GetSumSortTaskDetailListByLoad(loadId, whCode);
        }

        public string PackTaskInsertByLoad(PackTaskInsert entity)
        {
            return packTask.PackTaskInsertByLoad(entity);
        }

        public PackTask GetPackTaskById(int packTaskId)
        {
            return packTask.GetPackTaskById(packTaskId);
        }

        #endregion


        //验证 Load号是否正确
        public List<PackTask> GetPackTaskListByLoadOrder(string loadId, string whCode)
        {
            return packTask.GetPackTaskListByLoadOrder(loadId, whCode);
        }

        public List<SortTaskDetail> GetSortTaskDetailByLoadOrder(string loadId, string whCode)
        {
            return packTask.GetSortTaskDetailByLoadOrder(loadId, whCode);
        }

        //验证 分拣框号状态
        public string CheckPackTaskStatus(string sortGroupNumber, string whCode)
        {
            return packTask.CheckPackTaskStatus(sortGroupNumber, whCode);
        }

        //验证 分拣框号是否正确
        public List<PackTask> GetPackTaskList(string sortGroupNumber, string whCode)
        {
            return packTask.GetPackTaskList(sortGroupNumber, whCode);
        }

        //通过包装任务Id 取得当前包装的最大组号
        public int GetPackHeadGroupId(int packTaskId)
        {
            return packTask.GetPackHeadGroupId(packTaskId);
        }

        //通过Load得到订单渠道
        public string GetOrderType(string loadId, string whCode)
        {
            return packTask.GetOrderType(loadId, whCode);
        }

        //通过 当前包装扫描的框号 获得 其分拣明细
        public List<SortTaskDetail> GetSortTaskDetail(string loadId, string whCode, int sortGroupId)
        {
            return packTask.GetSortTaskDetail(loadId, whCode, sortGroupId);
        }

        //通过 当前包装扫描的框号 获得 其流程中的包装类型
        public List<FlowDetail> GetPackingType(string loadId, string whCode)
        {
            return packTask.GetPackingType(loadId, whCode);
        }


        //得到包装扫描多种耗材
        public List<FlowDetail> GetScanningConsumables(string loadId, string whCode)
        {
            return packTask.GetScanningConsumables(loadId, whCode);
        }


        //拉取 计划包装总数与已包装总数 
        public List<SortTaskDetailResult> GetSumSortTaskDetailList(string loadId, string whCode, int sortGroupId, string sortGroupNumber)
        {
            return packTask.GetSumSortTaskDetailList(loadId, whCode, sortGroupId, sortGroupNumber);
        }


        //包装工作台
        //再次确认包装号码后 提交数据 
        public string PackTaskInsert(PackTaskInsert entity)
        {
            return packTask.PackTaskInsert(entity);
        }

        //修改包装头表信息
        public string UpdatePackHead(PackHead entity)
        {
            return packTask.UpdatePackHead(entity);
        }


        //修改包装头表信息
        public string UpdatePackTask(PackTask entity)
        {
            return packTask.UpdatePackTask(entity);
        }
        //验证 包装箱号是否已存在
        public List<PackHead> CheckPackNumber(PackHead entity)
        {
            return packTask.CheckPackNumber(entity);
        }

        //查询包装信息列表
        public List<PackTaskSearchResult> GetPackTaskSearchResult(PackTaskSearch searchEntity, out int total, string[] expressNumberArr, string[] customerOutPoNumberArr)
        {
            return packTask.GetPackTaskSearchResult(searchEntity, out total, expressNumberArr, customerOutPoNumberArr);
        }


        //查询包装扫描明细
        public List<PackPackScanNumberResult> ScanNumberDetailList(int packDetailId, out int total)
        {
            return packTask.ScanNumberDetailList(packDetailId, out total);
        }

        //删除包装头信息
        public int DeletePackHead(PackHead entity)
        {
            return packTask.DeletePackHead(entity);
        }

        //查询包装明细信息列表
        public List<PackDetailSearchResult> GetPackDetailSearchResult(int packHeadId, out int total)
        {
            return packTask.GetPackDetailSearchResult(packHeadId, out total);
        }

        //随箱单报表查询
        public List<PackTaskCryReport> GetCryReportPackTask(int packTaskId, string whCode, string userName, int type)
        {
            return packTask.GetCryReportPackTask(packTaskId, whCode, userName, type);
        }

        //电子面单报表查询
        public List<PackTaskCryReportExpress> GetCryReportExpressPackTask(int packTaskId, string whCode, string userName, int type, string content)
        {
            return packTask.GetCryReportExpressPackTask(packTaskId, whCode, userName, type, content);
        }

        //云打印电子面单数据查询
        public List<PackTaskCryReportYunPrintData> GetCryReportYunPrintData(int packHeadId, string whCode, string userName, int type, string content)
        {
            return packTask.GetCryReportYunPrintData(packHeadId, whCode, userName, type, content);
        }

        //更新快递单号信息
        public void UpdateExpressNumberByYunPrint(int packHeadId, string userName, string message)
        {
            packTask.UpdateExpressNumberByYunPrint(packHeadId, userName, message);
        }

        //手动更新快递单号信息
        public string UpdateExpressNumberByWork(int packHeadId, string userName, string message)
        {
            return packTask.UpdateExpressNumberByWork(packHeadId, userName, message);
        }

        //获取快递单 并更新 包装信息
        public string GetExpressNumber(int packHeadId, string userName, string content)
        {
            return packTask.GetExpressNumber(packHeadId, userName, content);
        }


        //检测耗材
        public string checkLoss(string whCode, string lossCode)
        {
            return packTask.checkLoss(whCode, lossCode);
        }

        //出货进度查询
        public List<LoadProcedureResult> GetLoadProcedureList(LoadProcedureSearch searchEntity, out int total)
        {
            return packTask.GetLoadProcedureList(searchEntity, out total);
        }


        //获取DM条码打印
        public PackTaskResult GetBarCodeList(int packHeadId)
        {
            return packTask.GetBarCodeList(packHeadId);
        }


        //验证是否已打印过包装面单
        public string CheckPackPrintDate(int packHeadId)
        {
            return packTask.CheckPackPrintDate(packHeadId);
        }

        //日志添加
        public string TranLogAdd(TranLog entity)
        {
            return packTask.TranLogAdd(entity);
        }

        //通过包装头得到款号耗材及重量
        public List<ItemMaster> GetItemMasterByPackHeadId(int packHeadId)
        {
            return packTask.GetItemMasterByPackHeadId(packHeadId);
        }


        //批量获取顺丰加密数据
        public List<PackHeadJson> GetPackHeadJsonByExpress(string whCode, string[] expressNumber)
        {
            return packTask.GetPackHeadJsonByExpress(whCode, expressNumber);
        }

        //耗材列表
        public List<Loss> LossList(LossSearch searchEntity, out int total)
        {
            return root.LossList(searchEntity, out total);
        }


        #region  力士乐包装工作台

        ////验证 包装框号状态
        //public string BoschCheckPackTaskStatus(string sortGroupNumber, string whCode)
        //{
        //    return packTask.BoschCheckPackTaskStatus(sortGroupNumber, whCode);
        //}

        ////获取包装任务信息
        //public PackTask GetBoschPackTask(string sortGroupNumber, string whCode)
        //{
        //    return packTask.GetBoschPackTask(sortGroupNumber, whCode);
        //}

        ////获取包装框号等信息
        //public PackHead GetBoschPackHead(string sortGroupNumber, string whCode)
        //{
        //    return packTask.GetBoschPackHead(sortGroupNumber, whCode);
        //}

        ////更新重量 耗材等信息
        //public string UpdateBoschPackHead(PackHead entity)
        //{
        //    return packTask.UpdateBoschPackHead(entity);
        //}

        ////更新包装框号
        //public string UpdateBoschPackNumber(int packHeadId, string setpackNumber)
        //{
        //    return packTask.UpdateBoschPackNumber(packHeadId, setpackNumber);
        //}

        ////随箱单报表字段查询
        //public List<BoschPackTaskCryReport> GetBoschCryReportPackTask(int packHeadId)
        //{
        //    return packTask.GetBoschCryReportPackTask(packHeadId);
        //}

        ////博士力士乐查询包装信息列表
        //public List<BoschPackTaskSearchResult> GetBoschPackTaskSearchResult(PackTaskSearch searchEntity, out int total)
        //{
        //    return packTask.GetBoschPackTaskSearchResult(searchEntity, out total);
        //}


        ////博士获取快递单
        //public string GetBoschExpressNumber(int packHeadId, string userName)
        //{
        //    return packTask.GetBoschExpressNumber(packHeadId, userName);
        //}

        ////博士电子面单报表查询
        //public List<PackTaskCryReportExpress> GetBoschCryReportExpressPackTask(int packHeadId)
        //{
        //    return packTask.GetBoschCryReportExpressPackTask(packHeadId);
        //}


        #endregion


        #region 特殊操作台

        //备货Load查询
        public List<LoadMasterResult> WorkPickLoadList(LoadMasterSearch searchEntity, out int total)
        {
            return packTask.WorkPickLoadList(searchEntity, out total);
        }

        //拉取 计划备货总数 
        public List<PickTaskDetailSumQtyResult> GetSumQtyPickTaskDetailList(string loadId, string whCode)
        {
            return packTask.GetSumQtyPickTaskDetailList(loadId, whCode);
        }

        public List<PickTaskDetailSumQtyResult> GetSumPickQtyPickTaskDetailList(string loadId, string whCode)
        {
            return packTask.GetSumPickQtyPickTaskDetailList(loadId, whCode);
        }

        //拉取备货任务明细
        public List<PickTaskDetail> GetPickTaskDetailList(string loadId, string whCode)
        {
            return packTask.GetPickTaskDetailList(loadId, whCode);
        }

        //备货
        public string PickingLoad(string LoadId, string whCode, string userName, string HuId, string PutHuId, string Location)
        {
            return shipLoad.PickingLoad(LoadId, whCode, userName, HuId, PutHuId, Location);
        }

        #endregion


        #region 款号组合特殊包装台


        public List<PackTask> GetPackTaskListByCombinationAltItemNumber(string loadId, string whCode)
        {
            return packTask.GetPackTaskListByCombinationAltItemNumber(loadId, whCode);
        }


        //拉取 计划包装总数与已包装总数 
        public List<SortTaskDetailResult> GetSumListByCombinationAltItemNumber(string loadId, string whCode)
        {
            return packTask.GetSumListByCombinationAltItemNumber(loadId, whCode);
        }



        //通过 当前包装扫描的Load号 获得 其分拣明细
        public List<SortTaskDetail> GetSortTaskDetailByCombinationAltItemNumber(string loadId, string whCode)
        {
            return packTask.GetSortTaskDetailByCombinationAltItemNumber(loadId, whCode);
        }


        //UCF随箱单报表查询
        public List<PackTaskCryReport> UCFGetCryReportPackTask(int packHeadId, string whCode, string userName, int type)
        {
            return packTask.UCFGetCryReportPackTask(packHeadId, whCode, userName, type);
        }


        //包装任意订单中的某一个款号时，自动包装完该订单的剩余款号
        public string PackTaskInsertByCombinationAltItemNumber(PackTaskInsert entity)
        {
            return packTask.PackTaskInsertByCombinationAltItemNumber(entity);
        }


        //查询包装信息列表
        public List<PackTaskSearchResult> GetPackTaskSearchResultByCombinationAltItemNumber(PackTaskSearch searchEntity, out int total, string[] expressNumberArr, string[] customerOutPoNumberArr)
        {
            return packTask.GetPackTaskSearchResultByCombinationAltItemNumber(searchEntity, out total, expressNumberArr, customerOutPoNumberArr);
        }

        //查询固定组合款号包装信息列表-UNICEF
        public List<PackTaskSearchResult> GetPackTaskSearchResultByCombinationAltItemNumberUCF(PackTaskSearch searchEntity, out int total, string[] expressNumberArr)
        {
            return packTask.GetPackTaskSearchResultByCombinationAltItemNumberUCF(searchEntity, out total, expressNumberArr);
        }

        //一键完成包装-UNICEF
        public string PackTaskInsertByCombinationAltItemNumberUCF(int?[] packTaskId, string userName)
        {
            return packTask.PackTaskInsertByCombinationAltItemNumberUCF(packTaskId, userName);
        }


        //UCF随箱单报表查询-批量
        public List<PackTaskCryReport> UCFGetCryReportPackTask1(int[] packHeadId, string whCode, string userName, int type)
        {
            return packTask.UCFGetCryReportPackTask(packHeadId, whCode, userName, type);
        }

        //快递面单报表查询-批量
        public List<PackTaskCryReportExpress> GetCryReportExpressPackTask1(int?[] packHeadId, string whCode, string userName, int type)
        {
            return packTask.GetCryReportExpressPackTask(packHeadId, whCode, userName, type);
        }

        #endregion

    }
}

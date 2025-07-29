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
    // 注意: 使用“重构”菜单上的“重命名”命令，可以同时更改代码和配置文件中的接口名“IPackTaskService”。
    [ServiceContract]
    public interface IPackTaskService
    {
        [OperationContract]
        List<WhClient> WhClientListSelect(string whCode);

        #region 单品包装

        [OperationContract]
        //验证 Load号是否正确
        List<PackTask> GetPackTaskListByLoad(string loadId, string whCode);

        [OperationContract]
        //通过 当前包装扫描的Load号 获得 其分拣明细
        List<SortTaskDetail> GetSortTaskDetailByLoad(string loadId, string whCode);


        [OperationContract]
        //拉取 计划包装总数与已包装总数 
        List<SortTaskDetailResult> GetSumSortTaskDetailListByLoad(string loadId, string whCode);


        [OperationContract]
        string PackTaskInsertByLoad(PackTaskInsert entity);

        [OperationContract]
        PackTask GetPackTaskById(int packTaskId);

        #endregion

        [OperationContract]
        //验证 分拣框号状态
        string CheckPackTaskStatus(string sortGroupNumber, string whCode);

        [OperationContract]
        //验证 Load号是否正确
        List<PackTask> GetPackTaskListByLoadOrder(string loadId, string whCode);

        [OperationContract]
        List<SortTaskDetail> GetSortTaskDetailByLoadOrder(string loadId, string whCode);

        [OperationContract]
        //验证 分拣框号是否正确
        List<PackTask> GetPackTaskList(string sortGroupNumber, string whCode);

        [OperationContract]
        //通过包装任务Id 取得当前包装的最大组号
        int GetPackHeadGroupId(int packTaskId);

        [OperationContract]
        //通过Load得到订单渠道
        string GetOrderType(string loadId, string whCode);

        [OperationContract]
        //通过 当前包装扫描的框号 获得 其分拣明细
        List<SortTaskDetail> GetSortTaskDetail(string loadId, string whCode, int sortGroupId);

        [OperationContract]
        //通过 当前包装扫描的框号 获得 其流程中的包装类型
        List<FlowDetail> GetPackingType(string loadId, string whCode);

        [OperationContract]
        //得到包装扫描多种耗材
        List<FlowDetail> GetScanningConsumables(string loadId, string whCode);


        [OperationContract]
        //拉取 计划包装总数与已包装总数 
        List<SortTaskDetailResult> GetSumSortTaskDetailList(string loadId, string whCode, int sortGroupId, string sortGroupNumber);

        [OperationContract]
        //包装工作台
        //再次确认包装号码后 提交数据 
        string PackTaskInsert(PackTaskInsert entity);

        [OperationContract]
        //修改包装头表信息
        string UpdatePackHead(PackHead entity);

        [OperationContract]
        //修改包裹总数量 add by yangxin 2024-05-29 子母单引用
        string UpdatePackTask(PackTask entity);

        [OperationContract]
        //验证 包装箱号是否已存在
        List<PackHead> CheckPackNumber(PackHead entity);

        [OperationContract]
        //查询包装信息列表
        List<PackTaskSearchResult> GetPackTaskSearchResult(PackTaskSearch searchEntity, out int total, string[] expressNumberArr, string[] customerOutPoNumberArr);

        [OperationContract]
        //查询包装扫描明细
        List<PackPackScanNumberResult> ScanNumberDetailList(int packDetailId, out int total);

        [OperationContract]
        //删除包装头信息
        int DeletePackHead(PackHead entity);

        [OperationContract]
        //查询包装明细信息列表
        List<PackDetailSearchResult> GetPackDetailSearchResult(int packHeadId, out int total);

        [OperationContract]
        //随箱单报表查询
        List<PackTaskCryReport> GetCryReportPackTask(int packTaskId, string whCode, string userName, int type);

        [OperationContract]
        //电子面单报表查询
        List<PackTaskCryReportExpress> GetCryReportExpressPackTask(int packTaskId, string whCode, string userName, int type,string content);

        [OperationContract]
        //云打印电子面单数据查询
        List<PackTaskCryReportYunPrintData> GetCryReportYunPrintData(int packHeadId, string whCode, string userName, int type, string content);

        [OperationContract]
        //更新快递单号信息
        void UpdateExpressNumberByYunPrint(int packHeadId, string userName, string message);

        [OperationContract]
        //手动更新快递单号信息
        string UpdateExpressNumberByWork(int packHeadId, string userName, string message);


        [OperationContract]
        //获取快递单 并更新 包装信息
        string GetExpressNumber(int packHeadId, string userName,string content);

        [OperationContract]
        //检测耗材
        string checkLoss(string whCode, string lossCode);

        [OperationContract]
        //出货进度查询
        List<LoadProcedureResult> GetLoadProcedureList(LoadProcedureSearch searchEntity, out int total);

        [OperationContract]
        //获取DM条码打印
        PackTaskResult GetBarCodeList(int packHeadId);

        [OperationContract]
        //验证是否已打印过包装面单
        string CheckPackPrintDate(int packHeadId);

        [OperationContract]
        //日志添加
        string TranLogAdd(TranLog entity);


        [OperationContract]
        //通过包装头得到款号耗材及重量
        List<ItemMaster> GetItemMasterByPackHeadId(int packHeadId);


        [OperationContract]
        //批量获取顺丰加密数据
        List<PackHeadJson> GetPackHeadJsonByExpress(string whCode, string[] expressNumber);

        [OperationContract]
        //耗材列表
        List<Loss> LossList(LossSearch searchEntity, out int total);


        #region  力士乐包装工作台

        //[OperationContract]
        ////验证 包装框号状态
        //string BoschCheckPackTaskStatus(string sortGroupNumber, string whCode);

        //[OperationContract]
        ////获取包装任务信息
        //PackTask GetBoschPackTask(string sortGroupNumber, string whCode);

        //[OperationContract]
        ////获取包装框号等信息
        //PackHead GetBoschPackHead(string sortGroupNumber, string whCode);

        //[OperationContract]
        ////更新重量 耗材等信息
        //string UpdateBoschPackHead(PackHead entity);

        //[OperationContract]
        ////更新包装框号
        //string UpdateBoschPackNumber(int packHeadId, string setpackNumber);

        //[OperationContract]
        ////随箱单报表字段查询
        //List<BoschPackTaskCryReport> GetBoschCryReportPackTask(int packHeadId);

        //[OperationContract]
        ////博士力士乐查询包装信息列表
        //List<BoschPackTaskSearchResult> GetBoschPackTaskSearchResult(PackTaskSearch searchEntity, out int total);

        //[OperationContract]
        ////博士获取快递单
        //string GetBoschExpressNumber(int packHeadId, string userName);

        //[OperationContract]
        ////博士电子面单报表查询
        //List<PackTaskCryReportExpress> GetBoschCryReportExpressPackTask(int packHeadId);

        #endregion


        #region 特殊操作台

        [OperationContract]
        //备货Load查询
        List<LoadMasterResult> WorkPickLoadList(LoadMasterSearch searchEntity, out int total);

        [OperationContract]
        //拉取 计划备货总数 
        List<PickTaskDetailSumQtyResult> GetSumQtyPickTaskDetailList(string loadId, string whCode);

        [OperationContract]
        List<PickTaskDetailSumQtyResult> GetSumPickQtyPickTaskDetailList(string loadId, string whCode);

        [OperationContract]
        //拉取备货任务明细
        List<PickTaskDetail> GetPickTaskDetailList(string loadId, string whCode);

        [OperationContract]
        //备货
        string PickingLoad(string LoadId, string whCode, string userName, string HuId, string PutHuId, string Location);

        #endregion


        #region 款号组合特殊包装台

        [OperationContract]
        List<PackTask> GetPackTaskListByCombinationAltItemNumber(string loadId, string whCode);

        [OperationContract]
        //拉取 计划包装总数与已包装总数 
        List<SortTaskDetailResult> GetSumListByCombinationAltItemNumber(string loadId, string whCode);

        [OperationContract]
        //通过 当前包装扫描的Load号 获得 其分拣明细
        List<SortTaskDetail> GetSortTaskDetailByCombinationAltItemNumber(string loadId, string whCode);

        [OperationContract]
        //UCF随箱单报表查询
        List<PackTaskCryReport> UCFGetCryReportPackTask(int packHeadId, string whCode, string userName, int type);

        [OperationContract]
        //包装任意订单中的某一个款号时，自动包装完该订单的剩余款号
        string PackTaskInsertByCombinationAltItemNumber(PackTaskInsert entity);

        [OperationContract]
        //查询包装信息列表
        List<PackTaskSearchResult> GetPackTaskSearchResultByCombinationAltItemNumber(PackTaskSearch searchEntity, out int total, string[] expressNumberArr, string[] customerOutPoNumberArr);

        [OperationContract]
        //查询固定组合款号包装信息列表-UNICEF
        List<PackTaskSearchResult> GetPackTaskSearchResultByCombinationAltItemNumberUCF(PackTaskSearch searchEntity, out int total, string[] expressNumberArr);

        [OperationContract]
        //一键完成包装-UNICEF
        string PackTaskInsertByCombinationAltItemNumberUCF(int?[] packTaskId, string userName);

        [OperationContract]
        //UCF随箱单报表查询-批量
        List<PackTaskCryReport> UCFGetCryReportPackTask1(int[] packHeadId, string whCode, string userName, int type);

        [OperationContract]
        //快递面单报表查询-批量
        List<PackTaskCryReportExpress> GetCryReportExpressPackTask1(int?[] packHeadId, string whCode, string userName, int type);

        #endregion


    }
}

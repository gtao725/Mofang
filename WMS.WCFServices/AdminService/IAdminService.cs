using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;

using MODEL_MSSQL;
using WMS.BLLClass;


namespace WMS.WCFServices.AdminService
{
    [ServiceContract]
    public interface IAdminService
    {
        //用户登录
        [OperationContract]
        WhUser LoginIn(WhUser whUser);

        //修改密码
        [OperationContract]
        int UserUpdatePwd(WhUser whUser);

        //超级管理员公司下拉菜单列表
        [OperationContract]
        IEnumerable<WhCompany> WhCompanyList();

        [OperationContract]
        //通过公司ID 获取该公司的仓库数组
        List<WhInfoResult> WhInfoList(int companyId, string userName);

        [OperationContract]
        //WMS工作台特殊权限
        List<WhPosition> GetWorkPowerByUser(string userCode);


        #region 1.用户管理

        [OperationContract]

        WhUser WhUserAdd(WhUser entity);

        [OperationContract]
        //新增用户时 同时新增用户与仓库关系
        int R_WhInfo_WhUserAdd(List<R_WhInfo_WhUser> entity);


        [OperationContract]
        List<WhUserResult> WhUserList(WhUserSearch whUserSearch, out int total);

        [OperationContract]
        List<WhPositionResult> WhPositionUnselected(WhPositionSearch whPositionSearch, out int total);

        [OperationContract]
        List<WhUserWhPositionResult> WhPositionSelected(WhUserWhPositionSearch whUserWhPositionSearch, out int total);

        [OperationContract]
        //根据当前用户查询出未选择仓库
        //对应UserInfoController中的 WhInfoUnselected 方法
        List<WhInfoResult> WhInfoUnselected(WhInfoSearch searchEntity, out int total);

        [OperationContract]
        //根据当前用户查询出已选择仓库
        //对应UserInfoController中的 WhInfoSelected 方法
        List<WhInfoWhUserResult> WhInfoSelected(WhInfoSearch searchEntity, out int total);


        [OperationContract]
        int WhUserPositionDeleteById(int id);

        [OperationContract]
        int WhInfoWhUserDel(int id);

        [OperationContract]
        int WhUserPositionListAdd(List<WhUserPosition> entity);

        [OperationContract]
        int WhUserPwdInit(WhUser entity);

        [OperationContract]
        int WhUserEdit(WhUser entity, params string[] modifiedProNames);


        [OperationContract]
        //修改用户对应的仓库
        int WhInfoWhUserEdit(R_WhInfo_WhUser entity, params string[] modifiedProNames);

        [OperationContract]
        //职位列表
        List<WhPosition> WhPositionSelect(string whCode);

        [OperationContract]
        //复制权限及仓库
        string CopyWhUserPosition(int userId, int copyUserId, int companyId);

        [OperationContract]
        //修改密码检测开关
        int WhUserCheckFlagEdit(int checkFlag);

        #endregion


        #region 2.职位管理

        [OperationContract]
        List<WhPosition> WhPositionList(WhPositionSearch whPositionSearch, out int total);

        [OperationContract]
        WhPosition WhPositionAdd(WhPosition entity);

        [OperationContract]
        int WhPositionEdit(WhPosition entity, params string[] modifiedProNames);

        [OperationContract]
        List<WhPowerResult> WhPowerUnselected(WhPowerSearch whPowerSearch, out int total);

        [OperationContract]
        List<WhPositionWhPowerResult> WhPowerSelected(WhPositionWhPowerSearch whPositionWhPowerSearch, out int total);

        [OperationContract]
        int WhPositionPowerListAdd(List<WhPositionPower> entity);

        [OperationContract]
        int WhPositionPowerDelById(int id);

        #endregion


        #region 3.权限管理

        [OperationContract]
        List<WhPower> WhPowerList(WhPowerSearch whPowerSearch, out int total);

        [OperationContract]
        WhPower WhPowerAdd(WhPower entity);

        [OperationContract]
        int WhPowerDelById(int id);

        [OperationContract]
        int PowerMVCUpdateByPowerId(WhPositionPowerMVC entity, int powerId, params string[] modifiedProNames);

        [OperationContract]
        List<WhPositionPowerMVCResult> WhPowerMVCUnselected(WhPositionPowerMVCSearch whPositionPowerMVCSearch, out int total);

        [OperationContract]
        List<WhPositionPowerMVCResult> WhPoweMVCSelected(WhPositionPowerMVCSearch whPositionPowerMVCSearch, out int total);

        [OperationContract]
        int PowerMVCUpdateById(WhPositionPowerMVC entity, int Id, params string[] modifiedProNames);

        [OperationContract]
        int Sync(List<WhPositionPowerMVC> entity);

        [OperationContract]
        int WhPowerEdit(WhPower enity, params string[] modifiedProNames);

        #endregion


        #region 4.菜单管理

        [OperationContract]
        List<WhMenuResult> WhMenuList(WhMenuSearch whMenuSearch, out int total);

        [OperationContract]
        WhMenu WhMenuAdd(WhMenu entity);

        [OperationContract]
        int WhMenuDelById(int id);

        [OperationContract]
        List<WhPower> WhMenuUnselected(WhMenuSearch search, out int total);

        [OperationContract]
        List<WhPower> WhMenuSelected(WhMenuSearch search, out int total);

        [OperationContract]
        int WhMenuUpdateById(WhMenu entity, int Id, params string[] modifiedProNames);

        [OperationContract]
        IEnumerable<WhMenuResult> MenuNameSelect(int CompanyId);

        [OperationContract]
        int WhMenuEdit(WhMenu whMenu, params string[] modifiedProNames);

        #endregion


        #region 5.登录权限

        //获取当前权限菜单信息
        [OperationContract]
        List<WhPositionPowerMVCResult> WhPowerMVCList(WhPositionPowerMVCSearch whPositionPowerMVCSearch);

        [OperationContract]
        List<WhMenuResult> WhUserMenuGet(WhUser whUser);

        #endregion


        #region 6.WinCE管理

        [OperationContract]
        //WinCE 基础数据管理
        List<BusinessObject> BusinessObjectList(BusinessObjectSearch searchEntity, out int total);

        [OperationContract]
        //业务中文名下拉列表
        IEnumerable<BusinessObject> BusObjectDesSelect();

        [OperationContract]
        //业务类型下拉列表
        IEnumerable<BusinessObjectResult> BusObjectTypeSelect();

        [OperationContract]
        //新增WinCE业务对象
        BusinessObject AddBusObject(BusinessObject entity);

        [OperationContract]
        //新增WinCE业务对象
        BusinessObjectItem AddBusObjectItem(BusinessObjectItem entity);

        [OperationContract]
        //WinCE 业务对象明细查询
        List<BusinessObjectItem> BusinessObjectItemList(BusinessObjectItemSearch searchEntity, out int total);

        [OperationContract]
        //取消业务对象明细
        int BusObjectItemDelById(int id);

        [OperationContract]
        //删除业务对象
        int BusObjectDel(int id);

        #endregion


        #region 7.流程管理

        [OperationContract]
        //流程规则查询
        List<RFFlowRuleResult> RFFlowRuleList(RFFlowRuleSearch searchEntity, out int total);

        [OperationContract]
        //新增流程规则对象
        RFFlowRule AddRFFlorRule(RFFlowRule entity);

        [OperationContract]
        //流程规则对象删除
        int RFFlowRuleDel(int id);

        [OperationContract]
        //修改流程规则对象
        int EditRFFlowRule(RFFlowRule entity);

        #endregion


        #region 8.出货流程管理
        [OperationContract]
        //流程规则查询
        List<FlowRuleResult> FlowRuleList(FlowRuleSearch searchEntity, out int total);

        [OperationContract]
        //新增流程规则对象
        FlowRule AddFlowRule(FlowRule entity);

        [OperationContract]
        //流程规则对象删除
        int FlowRuleDel(int id);

        [OperationContract]
        //修改流程规则对象
        int EditFlowRule(FlowRule entity);

        [OperationContract]
        //RF枪的出货流程下拉列表
        IEnumerable<BusinessFlowGroupResult> FlowNameSelect(string whCode);

        #endregion


        #region 9.仓库管理

        [OperationContract]
        //列表
        List<WhInfoResult1> WhInfoNameList(WhInfoSearch1 searchEntity, out int total);

        [OperationContract]
        //新增
        WhInfo WhInfoAdd(WhInfo entity);

        [OperationContract]
        //修改
        int WhInfoEdit(WhInfo entity);


        #endregion

    }
}

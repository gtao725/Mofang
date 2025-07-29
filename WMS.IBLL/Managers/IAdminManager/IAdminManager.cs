using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMS.BLLClass;
using MODEL_MSSQL;

namespace WMS.IBLL
{
    public interface IAdminManager
    {

        //验证当前登录用户
        WhUser LoginIn(WhUser whUser);

        //修改密码
        int UserUpdatePwd(WhUser whUser);

        //登录后 通过公司ID 获取该公司的仓库数组
        List<WhInfoResult> WhInfoList(int companyId, string userName);

        //超级管理员公司下拉菜单列表
        IEnumerable<WhCompany> WhCompanyList();

        //WMS工作台特殊权限
        List<WhPosition> GetWorkPowerByUser(string userCode);


        #region 1.用户管理

        //新增用户
        //对应UserInfoController中的 AddUser 方法
        WhUser WhUserAdd(WhUser entity);

        //批量新增用户与仓库关系
        int R_WhInfo_WhUserAdd(List<R_WhInfo_WhUser> entity);


        //验证用户是否存在
        int WhUserCheck(WhUser entity);

        WhUser WhUserInfoCheck(WhUser entity);

        //用户列表
        //对应UserInfoController中的 List 方法
        List<WhUserResult> WhUserList(WhUserSearch whUserSearch, out int total);

        //根据当前用户查询出未选择职位
        //对应UserInfoController中的 WhPositionUnselected 方法
        List<WhPositionResult> WhPositionUnselected(WhPositionSearch whPositionSearch, out int total);

        //根据当前用户查询出未选择职位
        //对应UserInfoController中的 WhPositionSelected 方法
        List<WhUserWhPositionResult> WhPositionSelected(WhUserWhPositionSearch whUserWhPositionSearch, out int total);


        //根据当前用户查询出未选择仓库
        //对应UserInfoController中的 WhInfoUnselected 方法
        List<WhInfoResult> WhInfoUnselected(WhInfoSearch searchEntity, out int total);

        //根据当前用户查询出已选择仓库
        //对应UserInfoController中的 WhInfoSelected 方法
        List<WhInfoWhUserResult> WhInfoSelected(WhInfoSearch searchEntity, out int total);


        //批量添加用户对应职位
        //对应UserInfoController中的 WhUserPositionListAdd 方法
        int WhUserPositionListAdd(List<WhUserPosition> entity);

        //用户密码初始化
        //对应UserInfoController中的 WhUserPwdInit 方法
        int WhUserPwdInit(WhUser entity);

        //用户信息修改
        //对应UserInfoController中的 WhUserEdit 方法
        int WhUserEdit(WhUser entity, params string[] modifiedProNames);

        //修改用户对应的仓库
        int WhInfoWhUserEdit(R_WhInfo_WhUser entity, params string[] modifiedProNames);

        //职位列表
        List<WhPosition> WhPositionSelect(string whCode);

        //复制权限及仓库
        string CopyWhUserPosition(int userId, int copyUserId, int companyId);

        //修改密码检测开关
        int WhUserCheckFlagEdit(int checkFlag);

        string ApiVersion();

        #endregion


        #region 2.职位管理

        //职位列表
        //对应PositionController中的 List 方法
        List<WhPosition> WhPositionList(WhPositionSearch whPositionSearch, out int total);

        //新增职位
        //对应PositionController中的 WhPositionAdd 方法
        WhPosition WhPositionAdd(WhPosition entity);

        //职位信息修改
        //对应PositionController中的 WhPositionEdit 方法
        int WhPositionEdit(WhPosition entity, params string[] modifiedProNames);

        //根据当前职位查询出未选择的权限信息
        //对应PositionController中的 WhPowerUnselected 方法
        List<WhPowerResult> WhPowerUnselected(WhPowerSearch whPowerSearch, out int total);

        //根据当前职位查询出已选择的权限信息
        //对应PositionController中的 WhPowerSelected 方法
        List<WhPositionWhPowerResult> WhPowerSelected(WhPositionWhPowerSearch whPositionWhPowerSearch, out int total);

        //批量添加职位权限关系
        //对应PositionController中的 WhPositionPowerListAdd 方法
        int WhPositionPowerListAdd(List<WhPositionPower> entity);

        #endregion


        #region 3.权限管理

        //权限列表
        //对应PowerController中的 List 方法
        List<WhPower> WhPowerList(WhPowerSearch whPowerSearch, out int total);

        //新增权限
        //对应PowerController中的 AddPower 方法
        WhPower WhPowerAdd(WhPower entity);

        //删除权限后 更新权限控制MVC 表
        //对应PowerController中的 WhPowerDelById 方法
        int PowerMVCUpdateByPowerId(WhPositionPowerMVC entity, int powerId, params string[] modifiedProNames);

        //根据权限查询出未选择的控制
        //对应PowerController中的 WhPowerMVCUnselected 方法
        List<WhPositionPowerMVCResult> WhPowerMVCUnselected(WhPositionPowerMVCSearch whPositionPowerMVCSearch, out int total);

        //根据权限查询出已选择的控制
        //对应PowerController中的 WhPoweMVCSelected 方法
        List<WhPositionPowerMVCResult> WhPoweMVCSelected(WhPositionPowerMVCSearch whPositionPowerMVCSearch, out int total);

        //取消权限的某个控制 即更新控制器
        //对应PowerController中的 WhPowerMVCDel 方法-------------以下方法同时被俩个方法调用
        //添加权限对应的MVC关系
        //对应PowerController中的 WhPowerMVCListAdd 方法
        int PowerMVCUpdateById(WhPositionPowerMVC entity, int Id, params string[] modifiedProNames);

        //MVC 域、control及方法同步至数据表WhPositionPowerMVC
        //对应PowerController中的 Sync 方法
        int Sync(List<WhPositionPowerMVC> entity);

        //修改权限信息
        //对应PowerController中的 WhPowerEdit 方法
        int WhPowerEdit(WhPower enity, params string[] modifiedProNames);

        #endregion


        #region 4.菜单管理

        //菜单列表
        //对应MenuController中的 List方法
        List<WhMenuResult> WhMenuList(WhMenuSearch whMenuSearch, out int total);

        //新增菜单，验证菜单中文名是否存在
        //对应MenuController中的 AddMenu方法
        WhMenu WhMenuAdd(WhMenu entity);

        //查询当前菜单未选择的权限
        //对应MenuController中的 WhMenuUnselected 方法
        List<WhPower> WhMenuUnselected(WhMenuSearch search, out int total);

        //查询当前菜单已选择的权限
        //对应MenuController中的 WhMenuSelected 方法
        List<WhPower> WhMenuSelected(WhMenuSearch search, out int total);

        //根据菜单ID 修改权限ID和权限名
        //对应MenuController中的 WhMenuUpdateById 方法        ------------注意以下方法同时被俩个方法调用
        //菜单批量添加权限
        //对应MenuController中的 WhMenuAddPower 方法
        int WhMenuUpdateById(WhMenu entity, int Id, params string[] modifiedProNames);

        //菜单名称下拉列表
        IEnumerable<WhMenuResult> MenuNameSelect(int CompanyId);

        //菜单信息修改
        //对应MenuController中的 WhMenuEdit 方法
        int WhMenuEdit(WhMenu whMenu, params string[] modifiedProNames);

        #endregion


        #region 5.登录权限

        //根据条件查询出控制权限
        List<WhPositionPowerMVCResult> WhPowerMVCList(WhPositionPowerMVCSearch whPositionPowerMVCSearch);

        //获取用户有权限的菜单
        List<WhMenuResult> WhUserMenuGet(WhUser whUser);

        #endregion


        #region 6.WinCE管理

        //WinCE 基础数据管理
        //对应 WinCEController 的 List  方法
        List<BusinessObject> BusinessObjectList(BusinessObjectSearch searchEntity, out int total);

        //业务中文名下拉列表
        IEnumerable<BusinessObject> BusObjectDesSelect();

        //业务类型下拉列表
        IEnumerable<BusinessObjectResult> BusObjectTypeSelect();

        //新增WinCE业务对象
        //对应 WinCEController中的 AddBusObject 方法
        BusinessObject AddBusObject(BusinessObject entity);

        //新增WinCE业务对象
        //对应 WinCEController中的 AddBusObject 方法
        BusinessObjectItem AddBusObjectItem(BusinessObjectItem entity);

        //WinCE 业务对象明细查询
        //对应 WinCEController 的 ObjectItemList  方法
        List<BusinessObjectItem> BusinessObjectItemList(BusinessObjectItemSearch searchEntity, out int total);

        //删除业务对象
        //对应 WinCEController 的 ObjectDel  方法
        int BusObjectDel(int id);

        #endregion


        #region 7.RF流程管理

        //流程规则查询
        //对应 FlowRuleController 的 List  方法
        List<RFFlowRuleResult> RFFlowRuleList(RFFlowRuleSearch searchEntity, out int total);


        //新增流程规则对象
        //对应 FlowRuleController中的 AddFlorRule 方法
        RFFlowRule AddRFFlorRule(RFFlowRule entity);

        //修改流程规则对象
        //对应 FlowRuleController中的 EditFlorRule 方法
        int EditRFFlowRule(RFFlowRule entity);

        #endregion


        #region 8.出货流程管理

        //流程规则查询
        //对应 FlowRuleController 的 List  方法
        List<FlowRuleResult> FlowRuleList(FlowRuleSearch searchEntity, out int total);

        //新增流程规则对象
        //对应 FlowRuleController中的 AddFlorRule 方法
        FlowRule AddFlowRule(FlowRule entity);

        //修改流程规则对象
        //对应 FlowRuleController中的 EditFlorRule 方法
        int EditFlowRule(FlowRule entity);

        //RF枪的出货流程下拉列表
        IEnumerable<BusinessFlowGroupResult> FlowNameSelect(string whCode);


        #endregion


        #region 9.仓库管理

        //列表
        List<WhInfoResult1> WhInfoNameList(WhInfoSearch1 searchEntity, out int total);

        //新增
        WhInfo WhInfoAdd(WhInfo entity);

        //修改
        int WhInfoEdit(WhInfo entity);


        #endregion
    }
}

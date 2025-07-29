using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using MODEL_MSSQL;
using WMS.BLLClass;

namespace WMS.WCFServices.AdminService
{
    // 注意: 使用“重构”菜单上的“重命名”命令，可以同时更改代码、svc 和配置文件中的类名“AdminService1”。
    // 注意: 为了启动 WCF 测试客户端以测试此服务，请在解决方案资源管理器中选择 AdminService1.svc 或 AdminService1.svc.cs，然后开始调试。
    public class AdminService : IAdminService
    {
        IBLL.IAdminManager admin = new BLL.AdminManager();

        IBLL.IWhUserService user = new BLL.WhUserService();
        IBLL.IWhUserPositionService userPosition = new BLL.WhUserPositionService();

        IBLL.IWhPositionService position = new BLL.WhPositionService();
        IBLL.IWhPositionPowerService positionPower = new BLL.WhPositionPowerService();

        IBLL.IWhPowerService power = new BLL.WhPowerService();
        IBLL.IWhMenuService menu = new BLL.WhMenuService();

        IBLL.IBusinessObjectItemService busObjItem = new BLL.BusinessObjectItemService();
        IBLL.IRFFlowRuleService RflowRule = new BLL.RFFlowRuleService();
        IBLL.IFlowRuleService flowRule = new BLL.FlowRuleService();

        IBLL.IR_WhInfo_WhUserService whInfoWhUser = new BLL.R_WhInfo_WhUserService();

        //用户登录
        public WhUser LoginIn(WhUser whUser)
        {
            return admin.LoginIn(whUser);
        }

        public int UserUpdatePwd(WhUser whUser)
        {
            return admin.UserUpdatePwd(whUser);
        }

        public IEnumerable<WhCompany> WhCompanyList()
        {
            return admin.WhCompanyList();
        }

        //通过公司ID 获取该公司的仓库数组
        public List<WhInfoResult> WhInfoList(int companyId, string userName)
        {
            return admin.WhInfoList(companyId, userName);
        }

        //WMS工作台特殊权限
        public List<WhPosition> GetWorkPowerByUser(string userCode)
        {
            return admin.GetWorkPowerByUser(userCode);
        }


        #region 1.用户管理

        public WhUser WhUserAdd(WhUser entity)
        {
            return admin.WhUserAdd(entity);
        }

        //新增用户时 同时新增用户与仓库关系
        public int R_WhInfo_WhUserAdd(List<R_WhInfo_WhUser> entity)
        {
            return admin.R_WhInfo_WhUserAdd(entity);
        }

        public List<WhUserResult> WhUserList(WhUserSearch whUserSearch, out int total)
        {
            return admin.WhUserList(whUserSearch, out total);
        }

        public List<WhPositionResult> WhPositionUnselected(WhPositionSearch whPositionSearch, out int total)
        {
            return admin.WhPositionUnselected(whPositionSearch, out total);
        }

        public List<WhUserWhPositionResult> WhPositionSelected(WhUserWhPositionSearch whUserWhPositionSearch, out int total)
        {
            return admin.WhPositionSelected(whUserWhPositionSearch, out total);
        }

        //根据当前用户查询出未选择仓库
        //对应UserInfoController中的 WhInfoUnselected 方法
        public List<WhInfoResult> WhInfoUnselected(WhInfoSearch searchEntity, out int total)
        {
            return admin.WhInfoUnselected(searchEntity, out total);
        }


        //根据当前用户查询出已选择仓库
        //对应UserInfoController中的 WhInfoSelected 方法
        public List<WhInfoWhUserResult> WhInfoSelected(WhInfoSearch searchEntity, out int total)
        {
            return admin.WhInfoSelected(searchEntity, out total);
        }

        //根据用户仓库关系ID 删除
        public int WhInfoWhUserDel(int id)
        {
            return whInfoWhUser.DeleteById(id);
        }



        public int WhUserPositionDeleteById(int id)
        {
            return userPosition.DeleteById(id);
        }

        public int WhUserPositionListAdd(List<WhUserPosition> entity)
        {
            return admin.WhUserPositionListAdd(entity);
        }

        public int WhUserPwdInit(WhUser entity)
        {
            return admin.WhUserPwdInit(entity);
        }

        public int WhUserEdit(WhUser entity, params string[] modifiedProNames)
        {
            return admin.WhUserEdit(entity, modifiedProNames);
        }

        //修改用户对应的仓库
        public int WhInfoWhUserEdit(R_WhInfo_WhUser entity, params string[] modifiedProNames)
        {
            return admin.WhInfoWhUserEdit(entity, modifiedProNames);
        }

        //职位列表
        public List<WhPosition> WhPositionSelect(string whCode)
        {
            return admin.WhPositionSelect(whCode);
        }

        //复制权限及仓库
        public string CopyWhUserPosition(int userId, int copyUserId, int companyId)
        {
            return admin.CopyWhUserPosition(userId, copyUserId, companyId);
        }

        //修改密码检测开关
        public int WhUserCheckFlagEdit(int checkFlag)
        {
            return admin.WhUserCheckFlagEdit(checkFlag);
        }

        #endregion


        #region 2.职位管理

        public List<WhPosition> WhPositionList(WhPositionSearch whPositionSearch, out int total)
        {
            return admin.WhPositionList(whPositionSearch, out total);
        }

        public WhPosition WhPositionAdd(WhPosition entity)
        {
            return admin.WhPositionAdd(entity);
        }

        public int WhPositionEdit(WhPosition entity, params string[] modifiedProNames)
        {
            return admin.WhPositionEdit(entity, modifiedProNames);
        }

        List<WhPowerResult> IAdminService.WhPowerUnselected(WhPowerSearch whPowerSearch, out int total)
        {
            return admin.WhPowerUnselected(whPowerSearch, out total);
        }

        public List<WhPositionWhPowerResult> WhPowerSelected(WhPositionWhPowerSearch whPositionWhPowerSearch, out int total)
        {
            return admin.WhPowerSelected(whPositionWhPowerSearch, out total);
        }

        public int WhPositionPowerListAdd(List<WhPositionPower> entity)
        {
            return admin.WhPositionPowerListAdd(entity);
        }

        public int WhPositionPowerDelById(int id)
        {
            return positionPower.DeleteById(id);
        }

        #endregion


        #region 3.权限管理

        public List<WhPower> WhPowerList(BLLClass.WhPowerSearch whPowerSearch, out int total)
        {
            return admin.WhPowerList(whPowerSearch, out total);
        }

        public WhPower WhPowerAdd(WhPower entity)
        {
            return admin.WhPowerAdd(entity);
        }

        public int WhPowerDelById(int id)
        {
            return power.DeleteById(id);
        }

        public int PowerMVCUpdateByPowerId(WhPositionPowerMVC entity, int powerId, params string[] modifiedProNames)
        {
            return admin.PowerMVCUpdateByPowerId(entity, powerId, modifiedProNames);
        }

        public List<WhPositionPowerMVCResult> WhPowerMVCUnselected(WhPositionPowerMVCSearch whPositionPowerMVCSearch, out int total)
        {
            return admin.WhPowerMVCUnselected(whPositionPowerMVCSearch, out total);
        }

        public List<WhPositionPowerMVCResult> WhPoweMVCSelected(WhPositionPowerMVCSearch whPositionPowerMVCSearch, out int total)
        {
            return admin.WhPoweMVCSelected(whPositionPowerMVCSearch, out total);
        }

        public int PowerMVCUpdateById(WhPositionPowerMVC entity, int Id, params string[] modifiedProNames)
        {
            return admin.PowerMVCUpdateById(entity, Id, modifiedProNames);
        }

        public int Sync(List<WhPositionPowerMVC> entity)
        {
            return admin.Sync(entity);
        }

        public int WhPowerEdit(WhPower enity, params string[] modifiedProNames)
        {
            return admin.WhPowerEdit(enity, modifiedProNames);
        }

        #endregion


        #region 4.菜单管理

        public List<WhMenuResult> WhMenuList(WhMenuSearch whMenuSearch, out int total)
        {
            return admin.WhMenuList(whMenuSearch, out total);
        }

        public WhMenu WhMenuAdd(WhMenu entity)
        {
            return admin.WhMenuAdd(entity);
        }

        public int WhMenuDelById(int id)
        {
            return menu.DeleteById(id);
        }

        public List<WhPower> WhMenuUnselected(WhMenuSearch search, out int total)
        {
            return admin.WhMenuUnselected(search, out total);
        }

        public List<WhPower> WhMenuSelected(WhMenuSearch search, out int total)
        {
            return admin.WhMenuSelected(search, out total);
        }

        public int WhMenuUpdateById(WhMenu entity, int Id, params string[] modifiedProNames)
        {
            return admin.WhMenuUpdateById(entity, Id, modifiedProNames);
        }


        public IEnumerable<WhMenuResult> MenuNameSelect(int CompanyId)
        {
            return admin.MenuNameSelect(CompanyId);
        }


        public int WhMenuEdit(WhMenu whMenu, params string[] modifiedProNames)
        {
            return admin.WhMenuEdit(whMenu, modifiedProNames);
        }

        #endregion


        #region 5.登录权限

        public List<WhPositionPowerMVCResult> WhPowerMVCList(WhPositionPowerMVCSearch whPositionPowerMVCSearch)
        {
            return admin.WhPowerMVCList(whPositionPowerMVCSearch);
        }

        public List<WhMenuResult> WhUserMenuGet(WhUser whUser)
        {
            return admin.WhUserMenuGet(whUser);
        }

        public IEnumerable<WhMenuResult> MenuNameSelect(string whCode)
        {
            throw new NotImplementedException();
        }

        #endregion


        #region 6.WinCE管理

        //WinCE 基础数据管理
        public List<BusinessObject> BusinessObjectList(BusinessObjectSearch searchEntity, out int total)
        {
            return admin.BusinessObjectList(searchEntity, out total);
        }

        //业务中文名下拉列表
        public IEnumerable<BusinessObject> BusObjectDesSelect()
        {
            return admin.BusObjectDesSelect();
        }


        //业务类型下拉列表
        public IEnumerable<BusinessObjectResult> BusObjectTypeSelect()
        {
            return admin.BusObjectTypeSelect();
        }

        //新增WinCE业务对象
        public BusinessObject AddBusObject(BusinessObject entity)
        {
            return admin.AddBusObject(entity);
        }

        //新增WinCE业务对象
        public BusinessObjectItem AddBusObjectItem(BusinessObjectItem entity)
        {
            return admin.AddBusObjectItem(entity);
        }

        //WinCE 业务对象明细查询
        public List<BusinessObjectItem> BusinessObjectItemList(BusinessObjectItemSearch searchEntity, out int total)
        {
            return admin.BusinessObjectItemList(searchEntity, out total);
        }


        //取消业务对象明细
        public int BusObjectItemDelById(int id)
        {
            return busObjItem.DeleteById(id);
        }

        //删除业务对象
        public int BusObjectDel(int id)
        {
            return admin.BusObjectDel(id);
        }
        #endregion


        #region 7.流程管理

        //流程规则查询

        public List<RFFlowRuleResult> RFFlowRuleList(RFFlowRuleSearch searchEntity, out int total)
        {
            return admin.RFFlowRuleList(searchEntity, out total);
        }

        //新增流程规则对象
        public RFFlowRule AddRFFlorRule(RFFlowRule entity)
        {
            return admin.AddRFFlorRule(entity);
        }


        //流程规则对象删除
        public int RFFlowRuleDel(int id)
        {
            return RflowRule.DeleteById(id);
        }

        //修改流程规则对象
        public int EditRFFlowRule(RFFlowRule entity)
        {
            return admin.EditRFFlowRule(entity);
        }

        #endregion


        #region 8.出货流程管理

        //流程规则查询

        public List<FlowRuleResult> FlowRuleList(FlowRuleSearch searchEntity, out int total)
        {
            return admin.FlowRuleList(searchEntity, out total);
        }

        //新增流程规则对象

        public FlowRule AddFlowRule(FlowRule entity)
        {
            return admin.AddFlowRule(entity);
        }

        //流程规则对象删除
        public int FlowRuleDel(int id)
        {
            return flowRule.DeleteById(id);
        }

        //修改流程规则对象

        public int EditFlowRule(FlowRule entity)
        {
            return admin.EditFlowRule(entity);
        }

        //RF枪的出货流程下拉列表
        public IEnumerable<BusinessFlowGroupResult> FlowNameSelect(string whCode)
        {
            return admin.FlowNameSelect(whCode);
        }


        #endregion


        #region 9.仓库管理

        //列表
        public List<WhInfoResult1> WhInfoNameList(WhInfoSearch1 searchEntity, out int total)
        {
            return admin.WhInfoNameList(searchEntity, out total);
        }

        //新增
        public WhInfo WhInfoAdd(WhInfo entity)
        {
            return admin.WhInfoAdd(entity);
        }

        //修改
        public int WhInfoEdit(WhInfo entity)
        {
            return admin.WhInfoEdit(entity);
        }


        #endregion

    }
}

using MODEL_MSSQL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using WMS.BLLClass;

namespace WMS.WCFServices.EclService
{
    // 注意: 使用“重构”菜单上的“重命名”命令，可以同时更改代码、svc 和配置文件中的类名“EclService”。
    // 注意: 为了启动 WCF 测试客户端以测试此服务，请在解决方案资源管理器中选择 EclService.svc 或 EclService.svc.cs，然后开始调试。
    public class EclService : IEclService
    {
        IBLL.IInBoundOrderManager inBound = new BLL.InBoundOrderManager();
        IBLL.IEclOutBoundManager outBound = new BLL.EclOutBoundManager();
        IBLL.IOutBoundOrderManager wmsoutBound = new BLL.OutBoundOrderManager();
        IBLL.IEclInBoundManager eclInBound = new BLL.EclInBoundManager();
        IBLL.IRecManager rec = new BLL.RecManager();
        IBLL.IInterceptManager im = new BLL.InterceptManager();
        IBLL.ITransferTaskManager ttm = new BLL.TransferTaskManager();
        IBLL.IReleaseLoadManager releaseLoad = new BLL.ReleaseLoadManager();
        IBLL.IInVentoryManager inventory = new BLL.InVentoryManager();
        IBLL.IAdminManager admin = new BLL.AdminManager();

        #region 1.入库订单导入

        public string InBoundOrderListAddEcl(InBoundOrderInsert entity)
        {
            return inBound.InBoundOrderListAddEcl(entity);
        }

        public string InBoundOrderListAddOms(InBoundOrderInsert entity)
        {
            return eclInBound.InBoundOrderListAddOms(entity);
        }

        public string InBoundOrderListAddOmsSSID(InBoundOrderInsert entity)
        {
            return eclInBound.InBoundOrderListAddOmsSSID(entity);
        }

        public List<EclRecModel> GetRecInfoOms(String Receipt)
        {
            return rec.GetRecInfoEcl(Receipt);
        }
        #endregion


        #region 2.出库Load导入并释放
        public string OutBoundLoadAddEcl(EclLoadModel entity)
        {
            return outBound.OutBoundLoadAddEcl(entity);
        }
        #endregion



        #region OMS出库订单管理
        //出库订单导入WMS
        public string OutBoundOrderAddOMS(EclOutOrderModel entity)
        {

            return outBound.OutBoundOrderAddOMS(entity);

        }

        public string BoschOutBoundOrderAddOMS(EclOutOrderModel entity)
        {
            return outBound.BoschOutBoundOrderAddOMS(entity);
        }

        public string OutBoundOrderInterceptOMS(string whCode, string customerOutPoNumber, string clientCode, string userName)
        {

            string Res = "";

            Res = im.OutBoundOrderIntercept(whCode, customerOutPoNumber, clientCode, userName);

            if (Res == "订单所选流程为不可拦截！")
            {

                Res = outBound.OMSOutBoundOrderDel(whCode, customerOutPoNumber, clientCode, userName);

            }
            return Res;

        }

        //获取bosch 包装实体
        public string GetBoschPackEntity(BoschPackEntity entity)
        {
            return "Y";
        }

        //获取DM 包装实体
        public string GetDMPackEntity(DMPackEntity entity)
        {
            return "Y";
        }

        //获取出库订单Json 实体
        public string GetPackTaskJsonEntity(PackTaskJsonEntity entity)
        {
            return "Y";
        }


        #endregion

        #region LOAD释放控制



        #region 3.Load撤销释放
        public string RollbackLoad(string loadId, string whCode, string userName)
        {
            return releaseLoad.RollbackLoad(loadId, whCode, userName);
        }
        #endregion

        #region 4.释放load
        public string CheckReleaseLoad(string loadId, string whCode, string userName)
        {
            return releaseLoad.CheckReleaseLoad(loadId, whCode, userName);
        }
        #endregion

        #endregion

        #region 款号维护
        public string ItemMasterAddOms(List<ItemMaster> iml)
        {

            return eclInBound.ItemMasterAddOms(iml);
        }


        public string ItemMasterUpdateOms(ItemMaster im)
        {

            return eclInBound.ItemMasterUpdateOms(im);
        }

        #endregion

        #region  获取交接信息

        public TransferTaskResultEcl GetTransferTaskEclResult(string whCode, string transferId)
        {

            return ttm.GetTransferTaskEclResult(whCode, transferId);
        }
        #endregion


        #region 修改库存Lot

        //调整库存Lot
        public string EditHuIdLot(EditHuDetailLotEntity entity, string locationId, int qty, string lot1, string lot2, string lotdate)
        {
            return inventory.EditHuIdLot(entity, locationId, qty, lot1, lot2, lotdate);
        }

        //调整库存Lot
        public List<EditHuDetailLotEntity> EditHuIdLot1(List<EditHuDetailLotEntity> entityList)
        {
            return inventory.EditHuIdLot(entityList);
        }

        #endregion


        public List<EclOutOrderModelResult> OutBoundOrderAddOMSBatch(List<EclOutOrderModel> entityList)
        {
            return outBound.OutBoundOrderAddOMSBatch(entityList);
        }

        //OMS手动获取快递单
        public string GetExpressNumber(string express_code, SFExpressModel sfModel, YTExpressModel ytModel, string whCode, ZTOExpressModel ztModel)
        {
            return outBound.GetExpressNumber(express_code, sfModel, ytModel, whCode, ztModel);
        }

        //批量导入预录入
        public string ImportsInBoundOrder(List<InBoundOrderInsert> entityList)
        {
            return inBound.ImportsInBoundOrder(entityList);
        }


        #region 5.WMS工作台网页版

        //用户登录
        public WhUser LoginIn(WhUser whUser)
        {
            return admin.LoginIn(whUser);
        }

        //通过公司ID 获取该公司的仓库数组
        public List<WhInfoResult> WhInfoList(int companyId, string userName)
        {
            return admin.WhInfoList(companyId, userName);
        }

        #endregion

    }
}

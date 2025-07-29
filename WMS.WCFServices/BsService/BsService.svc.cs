using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using WMS.BLLClass;
using MODEL_MSSQL;

namespace WMS.WCFServices.BsService
{
    // 注意: 使用“重构”菜单上的“重命名”命令，可以同时更改代码、svc 和配置文件中的类名“BsService”。
    // 注意: 为了启动 WCF 测试客户端以测试此服务，请在解决方案资源管理器中选择 BsService.svc 或 BsService.svc.cs，然后开始调试。
    public class BsService : IBsService
    {

        //IBLL.IInBoundOrderManager Iin = new BLL.IInBoundOrderManager();
        IBLL.IInBoundOrderManager inBound = new BLL.InBoundOrderManager();
        IBLL.IBsInBoundManager bsinBound = new BLL.BsInBoundManager();
        IBLL.IBsOutBoundManager bsoutbound = new BLL.BsOutBoundManager();
        IBLL.ILoadManager loadm = new BLL.LoadManager();
        //IBLL.IRegInBoundOrderManager inBoundReg = new BLL.RegInBoundOrderManager();

        #region 1.EIP入库订单导入

        public string InBoundOrderListAddBs(InBoundOrderInsert entity)
        {
            return bsinBound.InBoundOrderListAddBs(entity);
        }

        #endregion



        #region 6.删除入库订单
        public string DeleteInorderBySO(string SoNumber, string WhCode, string ClientCode) {

            return inBound.DeleteInorderBySO(SoNumber, WhCode, ClientCode);
        }
        #endregion

        #region 2.保税区EIP收货登记

        public string AddReceiptBs(string[] SO, string ClientCode, string LocationId, string WhCode, string User) {
            return bsinBound.AddReceiptBs(SO, ClientCode, LocationId, WhCode, User);
        }

        #endregion

        #region 出库load导入
        public string OutBoundLoadAddBs(BsLoadModel entity) {
            return bsoutbound.OutBoundLoadAddBs(entity);
        }
        #endregion

        #region 维护集装箱扩展信息

        public string AddOutBoundContainer(LoadContainerExtend entity) {
            bsoutbound.EditLoadShipMode(entity.LoadId, entity.WhCode, entity.ContainerType);
            return loadm.LoadContainerExtendAdd(entity);
        }
        #endregion

        #region 检查订单是否在WMS系统存在
        public string CheckOutBound(string CustomerOutPoNumber)
        {
            return bsoutbound.OutBoundOrderCheck(CustomerOutPoNumber);
        }
        #endregion

    }
}

using MODEL_MSSQL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using WMS.BLLClass;

namespace WMS.WCFServices.EipCfsService
{
    // 注意: 使用“重构”菜单上的“重命名”命令，可以同时更改代码、svc 和配置文件中的类名“Service1”。
    // 注意: 为了启动 WCF 测试客户端以测试此服务，请在解决方案资源管理器中选择 Service1.svc 或 Service1.svc.cs，然后开始调试。
    public class CfsService : ICfsService
    {

        IBLL.ILoadManager lm = new BLL.LoadManager();
        IBLL.IEipInBound ib = new BLL.EipInBound();
        IBLL.IInBoundOrderManager inBoundOrder = new BLL.InBoundOrderManager();
        IBLL.IRegInBoundOrderManager ri = new BLL.RegInBoundOrderManager();
        IBLL.IBsOutBoundManager bsoutbound = new BLL.BsOutBoundManager();
        IBLL.IRecManager rec = new BLL.RecManager();
        IBLL.IRootManager root = new BLL.RootManager();


        //收货导入订单直接收货登记
        public string EipInsertInBound(List<InBoundOrderInsert> entity)
        {
            return ib.EipInsertInBound(entity);
        }

        //导入装箱单
        public string LoadContainerExtendAdd(LoadContainerExtend entity)
        {
            LoadContainerExtend res = lm.LoadContainerAdd(entity);

            if (res == null)
            {
                return "0";
            }
            else
            {
                return res.Id.ToString();
            }
        }

        // 8.删除收货登记
        public string DelReceiptRegister(ReceiptRegister entity)
        {
            string res = ri.DelReceiptRegister(entity);
            if (res == "Y" || res == "未查询到该批次号码!")
                return "Y";
            else
                return "该收货批次无法撤销,可能已经开始收货,具体原因请查询WMS";
        }


        // 9.预约单批量导入预录入 同时生成收货操作单
        public string ImportsInBoundOrderAndReceiptByOrder(List<InBoundOrderInsert> entityList)
        {
            return inBoundOrder.ImportsInBoundOrderAndReceiptByOrder(entityList);
        }

        //删除LOAD
        public string LoadMasterDel(string LoadId, string Whcode, string User)
        {

            return ib.LoadMasterDel(LoadId, Whcode, User);
        }


        //10.出库load导入
        public string OutBoundLoadAdd(BsLoadModel entity)
        {
            return bsoutbound.OutBoundLoadAddBs(entity);
        }

        //批量导入预录入
        public string ImportsInBoundOrder(List<ImportsInBoundOrderInsert> entity)
        {
            List<InBoundOrderInsert> aa = ib.ImportsInBoundOrderTransformation(entity);

            return inBoundOrder.ImportsInBoundOrder(aa);
        }

        //重新生成收货费用
        public string DelReceiptCharge(string ReceiptId, string WhCode, string CreateUser)
        {
            return rec.DelReceiptCharge(ReceiptId, WhCode, CreateUser);
        }


        //得到实际操作费用列表 
        public List<FeeDetailResult1> getOperationFeeList(string feeNumber, string whCode, out int total)
        {
            return root.getOperationFeeList(feeNumber, whCode, out total);
        }

        //更新收货批次联单数
        public string UpdateRegReceiptBillCount(ReceiptRegister entity)
        {
            return ri.UpdateRegReceiptBillCount(entity);
        }

        //批量导入预录入
        public string CheckRegInBoundSo(string SoNumber, string Whcode, string ClientCode)
        {

            return ib.CheckRegInBoundSo(SoNumber, Whcode, ClientCode);

        }

        public string ImportsGWI(List<GwiDetailInsert> entity)
        {
            return ib.ImportsGWI(entity);
        }

        //计算箱单费用
        public string AgainLoadCharge(string whCode, string loadId, string userName)
        {
            return lm.AgainLoadCharge(whCode, loadId, userName);
        }

        //确认箱单费用
        public string LoadChargeEdit(string whCode, string loadId)
        {
            return lm.LoadChargeEdit(whCode, loadId, "C");
        }
    }
}
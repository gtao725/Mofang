using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MODEL_MSSQL;

namespace WMS.BLL
{

    public class FlowHelper
    {
        IDAL.IDALSession idal = BLL.BLLHelper.GetDal();
        public List<FlowDetail> FlowList; //完整流程
        public int FlowHeadId; //流程名
        public int NowProcessId; //当前流程环节ID
        public FlowDetail nowFlowDetail; //当前环节

        //构造方法
        public FlowHelper(OutBoundOrder entity, String Type)
        {

            OutBoundOrder Obo = new OutBoundOrder();
            InBoundOrder Ibo = new InBoundOrder();

            if (Type == "OutBound")
            {
                Obo = idal.IOutBoundOrderDAL.SelectBy(u => u.Id == entity.Id).First();
                FlowHeadId = Obo.ProcessId;
                NowProcessId = Obo.NowProcessId;
                nowFlowDetail = idal.IFlowDetailDAL.SelectBy(u => u.FlowRuleId == NowProcessId && u.FlowHeadId == FlowHeadId).First();
            }
            if (Type == "InBound")
            {
                //预留方便以后扩展收货流程
            }
            FlowList = idal.IFlowDetailDAL.SelectBy(u => u.FlowHeadId == FlowHeadId && u.StatusId != 0).OrderBy(u => u.OrderId).ToList();
        }

        //获取上一个流程环节对象
        public FlowDetail GetPreviousFlowDetail()
        {

            if (FlowList.Count > 0)
            {
                int indexList = FlowList.FindIndex(delegate (FlowDetail a) { return a.FlowRuleId == NowProcessId; });
                if (indexList > 0)
                {
                    return FlowList[indexList - 1];
                }
                else {
                    return null;
                }
            }
            else {
                return null;
            }

        }

        //获取下一个流程环节对象
        public FlowDetail GetNextFlowDetail()
        {
            if (FlowList.Count > 0)
            {
                int indexList = FlowList.FindIndex(delegate (FlowDetail a) { return a.FlowRuleId == NowProcessId; });
                if (indexList != FlowList.Count - 1)
                {
                    return FlowList[indexList + 1];
                }
                else {
                    return null;
                }
            }
            else {
                return null;
            }
        }





    }
}

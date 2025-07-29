using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MODEL_MSSQL;
using WMS.IBLL;

namespace WMS.BLL
{
    public class UserManager : IUserManager
    {
        IDAL.IDALSession dal = BLLHelper.GetDal();

       
        public string WhAgentWhClientAddComplex(WhAgent whAgent, WhClient whClient)
        {
            string result = "";
            //验证货代是否存在
            List<WhAgent> listWhAgent = dal.IWhAgentDAL.SelectBy(u => u.WhCode == whAgent.WhCode && u.AgentCode == whAgent.AgentCode && u.AgentType == whAgent.AgentType);
            if (listWhAgent.Count == 0)
            {
                dal.IWhAgentDAL.Add(whAgent);           //不存在就新增
                dal.IWhAgentDAL.SaveChanges();
            }
            else
            {
                whAgent = listWhAgent.First();
            }

            List<WhClient> listWhClient = dal.IWhClientDAL.SelectBy(u => u.WhCode == whClient.WhCode && u.ClientCode == whClient.ClientCode);
            if (listWhClient.Count == 0)
            {
                dal.IWhClientDAL.Add(whClient);
                dal.IWhAgentDAL.SaveChanges();
            }
            else
            {
                whClient = listWhClient.First();
            }

            List<R_WhClient_WhAgent> listR_WhClient_WhAgent = dal.IR_WhClient_WhAgentDAL.SelectBy(u => u.AgentId == whAgent.Id && u.AgentType == whAgent.AgentType && u.ClientId == whClient.Id);
            if (listR_WhClient_WhAgent.Count == 0)
            {
                R_WhClient_WhAgent r_WhClient_WhAgent = new R_WhClient_WhAgent();
                r_WhClient_WhAgent.AgentId = whAgent.Id;
                r_WhClient_WhAgent.AgentType = whAgent.AgentType;
                r_WhClient_WhAgent.ClientId = whClient.Id;
                dal.IR_WhClient_WhAgentDAL.Add(r_WhClient_WhAgent);
                dal.IR_WhClient_WhAgentDAL.SaveChanges();
                result = "添加成功！";
            }
            else
            {
                result = "该货代客户关系已存在！";
            }
            return result;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMS.BLLClass;

namespace WMS.IBLL
{
    public interface IPickWinceManger
    {
 
        List<string> PickLoadList(string WhCode, string LoadId,string UserName);
        List<string> GetPickTaskOrder(string WhCode, string LoadId);

        List<PickTaskDetailWince> GetPickTaskDetail(string WhCode, string LoadId, int OutBoundOrderId);

    }
}

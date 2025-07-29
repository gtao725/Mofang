
using System.Web.Http;
using WMS.BLL;
using WMS.BLLClass;
using WMS.WebApi.Common;

namespace WMS.WebApi.Controllers
{
    public class BusinessController : ApiController
    {
        Helper Helper = new Helper();
        [HttpGet]
        public object GetFlowDetail([FromUri]int ProcessId, [FromUri]string FlowDetailType)
        {

            IBLL.IBusiness iaa = new ApiBusiness();
            BusinessFlowDetailList res = iaa.GetFlowDetail(ProcessId, FlowDetailType);
            return Helper.ResultData("Y", "加载成功", res);
        }
 
        [HttpGet]
        public object GetFlowDetailAPP([FromUri]int ProcessId, [FromUri]string FlowDetailType)
        {

            IBLL.IBusiness iaa = new ApiBusiness();
            BusinessFlowDetailList res = iaa.GetFlowDetailAPP(ProcessId, FlowDetailType);
            return Helper.ResultData("Y", "加载成功", res);
        }
    }
}

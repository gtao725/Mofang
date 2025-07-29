using System.Net.Http;
using System.Web.Http.Filters;
using WMS.WebApi.Models;

namespace WMS.WebApi.Common
{
    public class ApiResultAttribute:ActionFilterAttribute
    {
        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            // 若发生例外则不在这边处理     
            if (actionExecutedContext.Exception != null)     return;

            base.OnActionExecuted(actionExecutedContext);
            ApiResultModel result = new ApiResultModel();

            // 取得由 API 返回的状态代码     
            result.Status = actionExecutedContext.ActionContext.Response.StatusCode;
            // 取得由 API 返回的资料
            result.Data = actionExecutedContext.ActionContext.Response.Content.ReadAsAsync<object>().Result;

            //actionExecutedContext.Response.Content=

            actionExecutedContext.Response = actionExecutedContext.Request.CreateResponse(result.Status, result, "application/json");
            // 重新封装回传格式     
            //actionExecutedContext.Response = actionExecutedContext.Request.CreateResponse(result.Status, result);

        }
    }
}
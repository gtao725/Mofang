
using System.Net.Http;
using System.Web.Http.Filters;
using System.Net;
using WMS.WebApi.Models;

namespace WMS.WebApi.Common
{
    public class ApiErrorHandleAttribute: ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext actionExecutedContext) {

            base.OnException(actionExecutedContext);
            // 取得发生例外时的错误讯息     
            var errorMessage = actionExecutedContext.Exception.Message;
            var result = new ApiResultModel()
            {
                Status = HttpStatusCode.BadRequest,
                ErrorMessage = errorMessage
            };
            // 重新打包回传的讯息     
            actionExecutedContext.Response = actionExecutedContext.Request.CreateResponse(result.Status, result);
        }

    }
}
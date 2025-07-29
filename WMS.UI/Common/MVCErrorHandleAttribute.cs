using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
 

namespace WMS.UI
{
    public class MVCExceptionFilterAttribution : HandleErrorAttribute
    {
        public override void OnException(ExceptionContext context)
        {

            base.OnException(context);
            // 取得发生例外时的错误讯息     
            var errorMessage = context.Exception.Message;
           // var aa = context.HttpContext.Response;
            context.ExceptionHandled = true;
            // context.Result = new ContentResult(){ Content = "MVC Error"};
            //var cc = context.HttpContext.Session["userName"];
            if (context.HttpContext.Session["userName"] == null)
                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            else
            {
                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Result = new ContentResult() { Content = errorMessage };
            }
            //  context.HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            // actionExecutedContext.Response = actionExecutedContext.Request.CreateResponse(HttpStatusCode.OK, result);
            //var result = new ApiResultModel()
            //{
            //    Status = ApiResultStatusEnum.SystemError,
            //    ErrorMessage = "系统异常!请联系技术部门!"
            //};
            ////Task<Stream> stream = actionContext.Request.Content.ReadAsStreamAsync();
            //Log log = new Log();
            //ApiLog apiLog = new ApiLog();
            //apiLog.ActionName = actionExecutedContext.ActionContext.ActionDescriptor.ControllerDescriptor.ControllerName + "/" + actionExecutedContext.ActionContext.ActionDescriptor.ActionName;
            //apiLog.Actiontype = "Error";
            //apiLog.Message = actionExecutedContext.Exception.Message.ToString();
            //apiLog.CreateTime = DateTime.Now;
            //log.InsertApiLog(apiLog);

            //if (errorMessage != "照片服务器异常")
            //    // 重新打包回传的讯息     
            //    actionExecutedContext.Response = actionExecutedContext.Request.CreateResponse(HttpStatusCode.OK, result);


        }
    }
}

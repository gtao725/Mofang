
using System.Linq;
using System.Web.Http;
namespace WMS.WebApi
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {

            GlobalConfiguration.Configure(WebApiConfig.Register);

        }
        /// <summary>
        /// 跨域设置
        /// </summary>
        public void Application_BeginRequest()
        {

            //OPTIONS请求方法的主要作用：
            //1、获取服务器支持的HTTP方法；也就是黑客经常用的方法。
            //2、用来检查服务器的性能。如Ajax进行跨域请求是的预检，需要想另外一个域名的资源发送OPTIONS请求头，用以判断发送的请求是否安全
            if (Request.Headers.AllKeys.Contains("Origin") && Request.HttpMethod == "OPTIONS")
            {
                //表示对输出的内容进行缓冲，执行page.Response.Flush()时，会等所有内容缓冲完毕，将内容发送到客户端
                //这样就不会出错，造成页面卡死状态，让用户无限制等下去
                Response.Flush();
            }
        }
    }
}

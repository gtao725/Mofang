using Newtonsoft.Json;
using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using WMS.EIP;

namespace WMS.Report.Controllers
{
    public class ExportController : Controller
    {
       
        public ActionResult ExportExecl()
        {
            //获取前台的标题作为文件名
            string eip_page_title = string.IsNullOrEmpty(HttpContext.Request["eip_page_title"])?"EIP汇出": HttpContext.Request["eip_page_title"];
            //获取全部的查询参数
            string PostDataStr = PostDataToString();
            //查询LIST的相对地址,可能带参数
            string exportUrl = HttpContext.Request["exportUrl"];
            //有自带参数时追加ExportExecl
            if (exportUrl.IndexOf('?') > 0)
                exportUrl = exportUrl + "&ExportExecl=Y";
            else
                exportUrl = exportUrl + "?ExportExecl=Y";
            //模拟请求获取现在Control层的查询条件,类型为ReportRequestDataModel 的JSON格式
           // string retStr = Post("http://" + HttpContext.Request.Url.Authority + exportUrl, PostDataStr);

            string scheme = HttpContext.Request.IsSecureConnection ? "https" : "http";
            string retStr = Post(scheme + "://" + HttpContext.Request.Url.Authority + exportUrl, PostDataStr);
            //请求到报表服务器的汇出地址,汇出文件
            return File(PostStream(EIP.EIP.ReportServer + "/api/SqlExport/ExportExecl", retStr), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", eip_page_title+".xlsx");
        }

        public Hashtable PostData() {


            Hashtable ht = new Hashtable();
            foreach (string key in HttpContext.Request.Form.AllKeys) {

                ht.Add(key, Request.Form[key]);
               // string str = Request.Form[key];

            }
            return ht;
        }
        private string Post(string url, string PostDataStr) {
 
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
            request.Method = "POST";
           // request.ContentType = "application/json";
            request.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
            SetWebRequest(request);
            if (!string.IsNullOrEmpty(PostDataStr))
            {
                byte[] data = Encoding.UTF8.GetBytes(PostDataStr);
                WriteRequestData(request, data);
            }
            return ReadResponse(request.GetResponse());
        }
        private  String PostDataToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (string key in HttpContext.Request.Form.AllKeys)
            {
                string keystr = key;
                string keyVal = Request.Form[key];

                sb.Append("&" + UrlEncode(key) + "=" +  Request.Form[key]);

            }
            return sb.ToString();
 
        }
        public  string UrlEncode(string str)
        {
            return UrlEncode(str, "UTF-8");
        }
        public  string UrlEncode(string str, string encode)
        {
            StringBuilder sb = new StringBuilder();
            byte[] byStr = System.Text.Encoding.GetEncoding(encode).GetBytes(str);
            for (int i = 0; i < byStr.Length; i++)
            {
                sb.Append(@"%" + Convert.ToString(byStr[i], 16));
            }
            return (sb.ToString());
        }
        private  Stream PostStream(string url, string PostData)
        {

            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/json";
            request.Proxy = null;
            byte[] data = Encoding.UTF8.GetBytes(PostData);
            WriteRequestData(request, data);
            WebResponse cc = request.GetResponse();
            return cc.GetResponseStream();
        }

        private  void SetWebRequest(HttpWebRequest request)
        {
            request.Credentials = CredentialCache.DefaultCredentials;
            request.Timeout = 30000;
        }
        private  void WriteRequestData(HttpWebRequest request, byte[] data)
        {
            request.ContentLength = data.Length;
            Stream writer = request.GetRequestStream();
            writer.Write(data, 0, data.Length);
            writer.Close();
        }
        private  String ReadResponse(WebResponse response)
        {
            StreamReader sr = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
            String retStr = sr.ReadToEnd();
            sr.Close();
            return retStr;
        }

    }
}

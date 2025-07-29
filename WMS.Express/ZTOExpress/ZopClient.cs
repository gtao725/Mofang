using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web;

namespace ZTOExpress
{
    public class ZopClient
    {
        public string appKey { get; }
        public string appSecret { get; }

        public ZopClient(string key, string secret)
        {
            this.appKey = key;
            this.appSecret = secret;
        }

        public string execute(ZopPublicRequest request)
        {
            string url = request.url;
            string strToDigest = request.body + appSecret;

            NameValueCollection headers = new NameValueCollection();
            headers.Add("x-appKey", appKey);
            headers.Add("x-datadigest", MD5ToBase64String(strToDigest));
            return HttpUtil.post(url, headers, request.jsonBody, request.timeout);
        }

        //MD5加密
        public static string MD5ToBase64String(string str)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] MD5 = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(str));//MD5(注意UTF8编码)
            string result = Convert.ToBase64String(MD5, 0, MD5.Length);//Base64                                                        
            return result;
        }
    }
}

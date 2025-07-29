using System.Collections.Specialized;

namespace ZTOExpress
{
    public class ZopPublicRequest
    {
        public string url { set; get; }
        public string body { set; get; }
        public string jsonBody { set; get; }

        public int timeout = 2000;

        public ZopPublicRequest()
        {

        }

    }
}
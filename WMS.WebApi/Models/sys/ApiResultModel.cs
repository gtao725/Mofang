using System.Net;

namespace WMS.WebApi.Models
{
    public class ApiResultModel {
        public HttpStatusCode Status { get; set; }
        public object Data { get; set; }
        public string ErrorMessage { get; set; }
    }
}
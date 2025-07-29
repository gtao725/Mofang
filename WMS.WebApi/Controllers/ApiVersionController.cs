using MODEL_MSSQL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.Http;
using WMS.BLL;
using WMS.BLLClass;
using WMS.IBLL;
using WMS.WebApi.Common;
using WMS.WebApi.Models;

namespace WMS.WebApi.Controllers
{
    public class ApiVersionController : ApiController
    {
        Helper Helper = new Helper();
        [HttpGet]
        public object GetVersion()
        {
            IAdminManager ia = new AdminManager();
            return Helper.ResultData("Y", ia.ApiVersion(), null, null);
        }

   
    }
}
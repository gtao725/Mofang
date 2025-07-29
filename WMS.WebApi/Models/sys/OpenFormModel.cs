using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WMS.WebApi.Models
{
    public class OpenFormModel
    {
        private string _formName;
        public string formName
        {
            get{return _formName; }
            set{_formName = value; }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace WMS.Report.Controllers.Model
{
    public class EveryDayRecOutModel
    {
        [Display(Name = "开始收出货时间")]
        [Required(ErrorMessage = "开始收出货时间不能为空!")]
        public DateTime BeginDate { get; set; }

        [Display(Name = "结束收出货时间")]
        [Required(ErrorMessage = "结束收出货时间不能为空!")]
        public DateTime EndDate { get; set; }
    }
}

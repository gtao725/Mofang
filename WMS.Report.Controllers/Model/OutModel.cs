using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace WMS.Report.Controllers.Model
{
    public class OutScanNumberModel
    {
        [Display(Name = "出货时间")]
        [Required(ErrorMessage = "开始出货时间不能为空!")]
        public DateTime OutDateBegin { get; set; }

        [Display(Name = "-")]
        [Required(ErrorMessage = "结束出货时间不能为空!")]
        public DateTime OutDateEnd { get; set; }
    }

    public class Load_StatusModel
    {
        [Display(Name = "创建时间")]
        [Required(ErrorMessage = "开始创建时间不能为空!")]
        public DateTime OutDateBegin { get; set; }

        [Display(Name = "创建时间")]
        [Required(ErrorMessage = "结束创建时间不能为空!")]
        public DateTime OutDateEnd { get; set; }
    }
}

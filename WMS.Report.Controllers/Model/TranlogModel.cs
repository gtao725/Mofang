using System;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WMS.Report.Controllers.Model
{
    public class TranlogModel
    {
        [Display(Name = "开始时间")]
        [Required(ErrorMessage = "开始时间不能为空!")]
        public DateTime RegisterDateBegin { get; set; }

        [Display(Name = "结束时间")]
        [Required(ErrorMessage = "结束时间不能为空!")]
        public DateTime RegisterDateEnd { get; set; }
    }

    public class WorkAccountModel
    {
        [Display(Name = "开始时间")]
        [Required(ErrorMessage = "开始时间不能为空!")]
        public DateTime ReceiptDateBegin { get; set; }

        [Display(Name = "结束时间")]
        [Required(ErrorMessage = "结束时间不能为空!")]
        public DateTime ReceiptDateEnd { get; set; }
    }
}

using System;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WMS.Report.Controllers.Model
{
    public class RecModel
    {
        [Display(Name = "开始登记时间")]
        [Required(ErrorMessage = "开始登记时间不能为空!")]
        public DateTime RegisterDateBegin { get; set; }

        [Display(Name = "结束登记时间")]
        [Required(ErrorMessage = "结束登记时间不能为空!")]
        public DateTime RegisterDateEnd { get; set; }

        [Display(Name = "开始收货时间")]
        [Required(ErrorMessage = "开始收货时间不能为空!")]
        public DateTime ReceiptDateBegin { get; set; }

        [Display(Name = "结束收货时间")]
        [Required(ErrorMessage = "结束收货时间不能为空!")]
        public DateTime ReceiptDateEnd { get; set; }
    }

    public class RecStatusModel
    {
        [Display(Name = "开始登记时间")]
        [Required(ErrorMessage = "开始登记时间不能为空!")]
        public DateTime RegisterDateBegin { get; set; }

        [Display(Name = "结束登记时间")]
        [Required(ErrorMessage = "结束登记时间不能为空!")]
        public DateTime RegisterDateEnd { get; set; }

        [Display(Name = "开始收货时间")]
        [Required(ErrorMessage = "开始收货时间不能为空!")]
        public DateTime ReceiptDateBegin { get; set; }

        [Display(Name = "结束收货时间")]
        [Required(ErrorMessage = "结束收货时间不能为空!")]
        public DateTime ReceiptDateEnd { get; set; }
    }

    public class RecDetailModel
    {
        [Display(Name = "开始收货时间")]
        [Required(ErrorMessage = "开始收货时间不能为空!")]
        public DateTime ReceiptDateBegin { get; set; }

        [Display(Name = "结束收货时间")]
        [Required(ErrorMessage = "结束收货时间不能为空!")]
        public DateTime ReceiptDateEnd { get; set; }
    }
}

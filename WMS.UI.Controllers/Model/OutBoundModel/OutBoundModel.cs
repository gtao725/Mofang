using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WMS.UI.Controllers.Model
{
    public class OutBoundOrderModel
    {
        //1. 预录入主页验证
        [Display(Name = "客户名")]
        [Required(ErrorMessage = "客户不能为空!")]
        public string txt_WhClient { get; set; }

        [Display(Name = "出库订单流程")]
        [Required(ErrorMessage = "出库订单流程不能为空!")]
        public string txt_OutFlowName { get; set; }


        [Display(Name = "客户出库单号")]
        [Required(ErrorMessage = "客户出库单号不能为空!")]
        public string txt_CustomerOutPoNumber { get; set; }

        [Display(Name = "计划出库时间")]
        [Required(ErrorMessage = "计划出库时间不能为空!")]
        public DateTime txt_PlanOutTime { get; set; }

        //2.预录入新增页面验证
        [Display(Name = "SKU")]
        [Required(ErrorMessage = "SKU不能为空!")]
        public string alt_item_number { get; set; }

        [Display(Name = "数量")]
        [Required(ErrorMessage = "数量不能为空!")]
        public int qty { get; set; }
    }

    public class LoadModel
    {
        //1. 预录入主页验证
        [Display(Name = "客户名")]
        [Required(ErrorMessage = "客户不能为空!")]
        public string txt_WhClient { get; set; }

        [Display(Name = "出货流程")]
        [Required(ErrorMessage = "出货流程不能为空!")]
        public string txt_FlowName { get; set; }



    }

}

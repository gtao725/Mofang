using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;


namespace WMS.UI.Controllers.Model
{
    public class RecCFSModel
    {
        //1. 预录入主页验证
        [Display(Name = "")]
        [Required(ErrorMessage = "客户不能为空!")]
        public string WhClientList { get; set; }

        [Display(Name = "流程")]
        [Required(ErrorMessage = "流程不能为空!")]
        public string ClientFLowNameList { get; set; }

        [Display(Name = "")]
        [Required(ErrorMessage = "进仓单不能为空!")]
        public string add_so { get; set; }


        //2.预录入新增页面验证
        [Display(Name = "数量")]
        [Required(ErrorMessage = "数量不能为空!")]
        public int qty { get; set; }

    }

    public class RecDCModel
    {
        //1. 预录入主页验证
        [Display(Name = "")]
        [Required(ErrorMessage = "客户不能为空!")]
        public string WhClientList { get; set; }

        [Display(Name = "流程")]
        [Required(ErrorMessage = "流程不能为空!")]
        public string ClientFLowNameList { get; set; }

        [Display(Name = "")]
        [Required(ErrorMessage = "PO不能为空!")]
        public string add_po { get; set; }

        //2.预录入新增页面验证
        [Display(Name = "数量")]
        [Required(ErrorMessage = "数量不能为空!")]
        public int qty { get; set; }

    }

    public class RecModel
    {
        //1. 预录入主页验证
        [Display(Name = "客户")]
        [Required(ErrorMessage = "客户不能为空!")]
        public string WhClientList { get; set; }


        //2.预录入新增页面验证
        [Display(Name = "数量")]
        [Required(ErrorMessage = "数量不能为空!")]
        public int qty { get; set; }

    }

    public class ReceiptModel
    {
        //1. 收货登记新增进仓单(整出)验证
        [Display(Name = "客户")]
        [Required(ErrorMessage = "客户不能为空!")]
        public string txt_WhClient { get; set; }

        [Display(Name = "客户")]
        [Required(ErrorMessage = "客户不能为空!")]
        public string txt_WhClient1 { get; set; }

        [Display(Name = "客户")]
        [Required(ErrorMessage = "客户不能为空!")]
        public string txt_WhClient2 { get; set; } 

        [Display(Name = "收货区域")]
        [Required(ErrorMessage = "收货区域不能为空!")]
        public string txt_recZone { get; set; }

        [Display(Name = "车辆到达时间")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd hh:mm:00}")]
        [Required(ErrorMessage = "车辆到达时间不能为空!")]
        public DateTime txt_ArriveDate { get; set; }

    }

    public class ReceiptPartModel
    {
        //1. 收货登记新增进仓单(整出)验证
        [Display(Name = "客户")]
        [Required(ErrorMessage = "客户不能为空!")]
        public string txt_WhClient { get; set; }

        [Display(Name = "车牌号")]
        [Required(ErrorMessage = "车牌号不能为空!")]
        public string txt_TruckNumber { get; set; }

        [Display(Name = "收货区域")]
        [Required(ErrorMessage = "收货区域不能为空!")]
        public string txt_recZone { get; set; }

        [Display(Name = "车辆到达时间")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd hh:mm:00}")]
        [Required(ErrorMessage = "车辆到达时间不能为空!")]
        public DateTime txt_ArriveDate { get; set; }

    }

    public class AddInVentoryModel
    {
        [Display(Name = "客户名")]
        [Required(ErrorMessage = "客户不能为空!")]
        public string txt_ClientCode { get; set; }

        [Display(Name = "托盘")]
        [Required(ErrorMessage = "托盘不能为空!")]
        public string txt_HuId { get; set; }

        [Display(Name = "库位")]
        [Required(ErrorMessage = "库位不能为空!")]
        public string txt_Location { get; set; }

        [Display(Name = "数量")]
        [Required(ErrorMessage = "数量不能为空!")]
        public int txt_Qty { get; set; }

    }
}

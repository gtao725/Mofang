using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Reflection;
//using WMS.DI;


namespace WMS.BLLClass
    {
    public class WhItemExtendOMSList
        {
         public List<WhItemExtendOMS> data { get; set; }
        }
    public class WhItemExtendOMS
        {
        [Required(ErrorMessage =  "仓库编码不能为空!")]
        public string WhCode { get; set; }//仓库编码
        [Required(ErrorMessage = "[客户]CODE不能为空!")]
        public string ClientCode { get; set; }//客户CODE

        [Required(ErrorMessage = "[款号]不能为空!")]
        public string AltItemNumber { get; set; }//款号
        [Required(ErrorMessage = "[描述]不能为空!")]
        public string Description { get; set; } //描述
        [Required(ErrorMessage = "[品名]不能为空!")]
        public string ItemName { get; set; }//品名
        public string EAN { get; set; }//EAN序列号
        [RegularExpression(pattern: @"^([1-9]\d*|[0]{1,1})$", ErrorMessage = "[长度]的值只能是非负整数")]
        public string Length { get; set; }//长度
        [RegularExpression(pattern: @"^([1-9]\d*|[0]{1,1})$", ErrorMessage = "[宽度]的值只能是非负整数")]
        public string Width { get; set; }//宽度
        [RegularExpression(pattern: @"^([1-9]\d*|[0]{1,1})$", ErrorMessage = "[高度]的值只能是非负整数")]
        public string Height { get; set; }//高度
        [RegularExpression(pattern: @"^\d+(\.\d+)?$",  ErrorMessage = "[重量]的值只能是数值型")]
        public string Weight { get; set; }//重量
        [RegularExpression(pattern: @"^\d+(\.\d+)?$", ErrorMessage = "[箱规]的值只能是数值型")]
        public string Pcs { get; set; }//箱规(件/箱)
        [RegularExpression(pattern: @"^[01]$", ErrorMessage = "[包装是否输入数量]的值只能是0或1(0:否,1:是)")]
        public string HandFlag { get; set; }//是否可以输入数量批量出货(1:是；0:否)

        [RegularExpression(pattern: @"^[01]$", ErrorMessage = "[是否送装]的值只能是0或1(0:否,1:是)")]
        public string InstallSevice { get; set; }//送装服务(1:是；0:否)
        public string Category { get; set; }//Category类别
        public string Style1 { get; set; }//属性1
        public string Style2 { get; set; }//属性2
        public string Style3 { get; set; }//属性3
        public string ClassName { get; set; }//类别1
        public string Style { get; set; }//尺码
        public string PackageStyle { get; set; }//长度
        public string Size { get; set; }//长度
        public string BoxCode { get; set; }//鞋盒编码
        public string OriginCountry { get; set; }//原产国
        public string UnitName { get; set; }//单位
        public string Matieral { get; set; }//材质
        public string Color { get; set; }//颜色
        public string CusItemNumber { get; set; }//客户款号
        public string CusStyle { get; set; }//客户自定义类型
        public string CreateUser { get; set; }//操作员
        public string Remark1 { get; set; }//备注1
        public string Remark2 { get; set; }//备注2
        public string Remark3 { get; set; }//备注3
        public string Remark4 { get; set; }//备注4
        public string Remark5 { get; set; }//备注5

        ///// <summary>
        ///// 必填校验
        ///// </summary>
        //public void Validate()
        //    {
        //    ValidationContext context = new ValidationContext(this, serviceProvider: null, items: null);
        //    List<ValidationResult> results = new List<ValidationResult>();
        //    bool isValid = Validator.TryValidateObject(this, context, results, true);

        //    if (isValid == false)
        //        {
        //        StringBuilder sbrErrors = new StringBuilder();
        //        foreach (var validationResult in results)
        //            {
        //            sbrErrors.Append(string.Format("{0} 字段必填！", validationResult.MemberNames.FirstOrDefault()));
        //            }
        //        throw new ValidationException(sbrErrors.ToString());
        //        }
        //    }
        }
   



    }




using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
 

namespace WMS.UI.Controllers.Model
{


 

    public class TestModel
    {

        //float-->Single
        //int-->Int32
        //Decimal==>Decimal
        //string==>String
        //DateTime==>DateTime



        [Display(Name = "系统账号")]
        [Required(ErrorMessage = "账号不能为空!")]
        public int AdministratorId { get; set; }
 

        [Display(Name = "用户名")]
        [Required(ErrorMessage = "必填")]
        [RegularExpression(@"^((0\d{2,5}-)|\(0\d{2,5}\))?\d{7,8}(-\d{3,4})?$", ErrorMessage = "电话格式不正确！\n 有效格式为：\n①本区7或8位号码[-3或4位分机号码,可选]\n②(3~5位区号)7或8位号码[-3或4位分机号码,可选]\n③3~5位区号-7或8位号码[-3或4位分机号码,可选]\n示例：023-12345678;(023)1234567-1234")]
        public string AdminName { get; set; }


        [Display(Name = "密码")]       
        [Required(ErrorMessage = "密码（必填）")]
        [StringLength(256, MinimumLength = 6, ErrorMessage = "6-20个字符。")]
        public string PassWord { get; set; }

        [Display(Name = "姓名")]
        [StringLength(20, ErrorMessage = "填写姓名可以更容易识别管理员。")]
        public string Name { get; set; }


        [Display(Name = "时间测试")]
        //[DisplayFormat(DataFormatString = "{0:yyyy-MM-dd hh:mm:00}")]
        [Required(ErrorMessage = "时间测试不能为空!")]
        public DateTime test { get; set; }

    }


//    public class ShowModel
//    {

//        //[DisplayName("Id")]
//        //public int Id { get; set; }

//        //[DisplayName("用户名")]
//        //[Required(ErrorMessage = "用户名不能为空")]
//        //[StringLength(10, ErrorMessage = "用户名长度不能超过10个字符")]
//        //public string UserName { get; set; }

//        //[DisplayName("密码")]
//        //[Required(ErrorMessage = "密码不能为空")]
//        //[DataType(DataType.Password)]
//        //[StringLength(20, ErrorMessage = "密码长度不能超过20个字符")]
//        //public string Password { get; set; }


//        //[Required]
//        //[Display(Name = "电子邮件地址")]
//        //[DataType(DataType.EmailAddress)]
//        //public string Email { get; set; }




//// 1.非空和数据类型验证
//        [Required]
//        [Display(Name = "用户名")]
//        public string UserName { get; set; }

//        [Required]
//        [DataType(DataType.Password)]
//        [Display(Name = "密码")]
//        public string Password { get; set; }

//        [Required]
//        [DataType(DataType.EmailAddress)]
//        [Display(Name = "电子邮件地址")]
//        public string Email { get; set; }

//        [DataType(DataType.Date)]
//        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
//        public DateTime UpdateDate { get; set; }


//        [DisplayName("备注")]
//        [DataType(DataType.MultilineText)]
//        public string Remark { get; set; }

////2.非空和字符长度验证
//        [Required(ErrorMessage = "用户名不能为空！")]
//        [DisplayName("用户名")] 
//         public string UserNameRequired { get; set; }

//        [DisplayName("密码")]
//        [StringLength(6, ErrorMessage = "密码长度不能超过6个字符！")]
//        public string PasswordLength { get; set; }
////3.值域验证
//        [DisplayName("年龄")]
        
//        [Range(1, int.MaxValue, ErrorMessage = "年龄不能小于1！")]
//        public int Age { get; set; }
////4.比较验证
//        [Required]
//        [DataType(DataType.Password)]
//        [DisplayName("密码")]
//        public string PasswordCompare { get; set; }

//        [DataType(DataType.Password)]
//        [DisplayName("确认密码")]
//        [Compare("PasswordCompare", ErrorMessage = "密码和确认密码不匹配！")]
//        public string ConfirmPassword { get; set; }

////5.正则表达式验证
//        [DisplayName("联系电话")]
//        [RegularExpression(@"^((0\d{2,5}-)|\(0\d{2,5}\))?\d{7,8}(-\d{3,4})?$", ErrorMessage = "电话格式不正确！\n 有效格式为：\n①本区7或8位号码[-3或4位分机号码,可选]\n②(3~5位区号)7或8位号码[-3或4位分机号码,可选]\n③3~5位区号-7或8位号码[-3或4位分机号码,可选]\n示例：023-12345678;(023)1234567-1234")]
//        public string Phone { get; set; }
//        [DisplayName("电子邮件")]
//        [RegularExpression(@"^\w+((-\w+)|(\.\w+))*\@[A-Za-z0-9]+((\.|-)[A-Za-z0-9]+)*\.[A-Za-z0-9]+$", ErrorMessage = "请输入正确的Email格式！\n示例：abc@123.com")]
//        public string EmailRegular { get; set; }
//        [DisplayName("网址")]
//        [RegularExpression(@"http(s)?://([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*)?", ErrorMessage = "请输入合法的网址!\n示例：https://abc.com;http://www.abc.cn")]
//        public string Httpaddress { get; set; }



////6.自定义验证
//        //[Required]
//        //[ValidatePasswordLength]
//        //[DataType(DataType.Password)]
//        //[DisplayName("密码")]
//        //public string Password { get; set; }
//        //[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
//        //public sealed class ValidatePasswordLengthAttribute : ValidationAttribute, IClientValidatable
//        //{
//        //    private const string _defaultErrorMessage = "'{0}' 必须至少包含 {1} 个字符。";
//        //    private readonly int _minCharacters = Membership.Provider.MinRequiredPasswordLength;

//        //        public ValidatePasswordLengthAttribute()
//        //            : base(_defaultErrorMessage)
//        //        {
//        //        }

//        //        public override string FormatErrorMessage(string name)
//        //        {
//        //            return String.Format(CultureInfo.CurrentCulture, ErrorMessageString,
//        //                name, _minCharacters);
//        //        }

//        //        public override bool IsValid(object value)
//        //        {
//        //            string valueAsString = value as string;
//        //            return (valueAsString != null && valueAsString.Length >= _minCharacters);
//        //        }

//        //        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
//        //        {
//        //            return new[]{
//        //            new ModelClientValidationStringLengthRule(FormatErrorMessage(metadata.GetDisplayName()), _minCharacters, int.MaxValue)
//        //        };
//        //    }
//        //}
 

//    }

}
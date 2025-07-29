using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WMS.DI
    {

    //[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    //public abstract class BaseAttribute : Attribute
    //    {
    //    public virtual string error { get; set; }
    //    public abstract bool Validate(object value);
    //    }

    ///// <summary>
    ///// 约束属性不能为空
    ///// </summary>
    ///// <summary>
    ///// 约束属性不能为空
    ///// </summary>
    //public class RequiredAttribute : BaseAttribute
    //    {
    //    public override string error
    //        {
    //        get
    //            {
    //            if (base.error != null)
    //                {
    //                return base.error;
    //                }
    //            return "属性不能为空";
    //            }
    //        set { base.error = value; }
    //        }
    //    public override bool Validate(object value)
    //        {
    //        return !(value == null);
    //        }
    //    }
    ///// <summary>
    ///// 约束字符串的长度范围
    ///// </summary>
    //public class StringRangeAttribute : BaseAttribute
    //    {
    //    public int min { get; set; }
    //    public int max { get; set; }
    //    public override string error
    //        {
    //        get
    //            {
    //            if (base.error != null)
    //                {
    //                return base.error;
    //                }
    //            return $"字符串长度范围{this.min}-{this.max}";
    //            }
    //        set { base.error = value; }
    //        }
    //    public override bool Validate(object value)
    //        {
    //        return value.ToString().Length >= this.min && value.ToString().Length <= this.max;
    //        }
    //    }

    ///// <summary>
    ///// 约束符合正则表达式
    ///// </summary>
    //public class RegexAttribute : BaseAttribute
    //    {
    //    public string regexText;
    //    public override bool Validate(object value)
    //        {
    //        var regex = new Regex(regexText);
    //        return regex.Match(value.ToString()).Success;
    //        }
    //    }


    public static class Authorize
        {

        public static string Validate<T>(this T t)
            {
            Type type = t.GetType();
           
            //获取所有属性
            PropertyInfo[] propertyInfos = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            List<string> errorList = new List<string>();
            foreach (PropertyInfo propertyInfo in propertyInfos)
                {
                if (propertyInfo.IsDefined(typeof(ValidationAttribute)))//如果属性上有定义该属性,此步没有构造出实例
                    {
                    foreach (ValidationAttribute attribute in propertyInfo.GetCustomAttributes(typeof(ValidationAttribute)))
                        {
                        if (!attribute.IsValid(propertyInfo.GetValue(t, null)))
                            {
                            errorList.Add(attribute.ErrorMessage);
                            }
                        }
                    }
                }
            return string.Join(",", errorList);
            
            }

        }
    }

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Web.Script.Serialization;
using WMS.WebApi.Models;
namespace WMS.WebApi.Common
{
    /// <summary>
    /// 
    /// </summary>
    public class Helper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="statu"></param>
        /// <param name="msg"></param>
        /// <param name="data"></param>
        /// <param name="dataName">data类型名称</param>
        /// <returns></returns>
        public  ApiResultDataModel  ResultData(string statu, string msg, object data,string dataName)
        {
            ApiResultDataModel resultData = new ApiResultDataModel();
            resultData.Statu = statu;
            resultData.Msg= msg;
            
            PropertyInfo[] ps = resultData.GetType().GetProperties();
            foreach (PropertyInfo i in ps)
            {
                //泛型T的类型与ApiResultDataModel 属性类型相同是反会DATA
                if (i.PropertyType.Name == dataName)
                {
                    i.SetValue(resultData, data, null); //给对应属性赋值
                    
                }
            }
            return resultData;
        }


        public  object ResultData<T>(string statu, string msg, T data) where T : class
        {
            
            
            dynamic d = new ExpandoObject();
            d.Statu = statu;
            d.Msg = msg;

            string typeName = typeof(T).Name;

            //resultData.Statu = statu;
            //resultData.Msg= msg;
            // object aa = new { Statu = statu, Msg=msg, Data = data };
            //if (typeName != "EmptyModel")
            //{
                ApiResultDataModel resultData = new ApiResultDataModel();
                resultData.Statu = statu.ToString();
                resultData.Msg = msg;
                PropertyInfo[] ps = resultData.GetType().GetProperties();
                foreach (PropertyInfo i in ps)
                {

                    //泛型T的类型与ApiResultDataModel 属性类型相同是返回该属性
                    //if (i.PropertyType.Name == typeof(T).Name)
                    if (i.PropertyType.FullName == typeof(T).FullName)
                    {
                        (d as ICollection<KeyValuePair<string, object>>).Add(new KeyValuePair<string, object>(i.Name, data));
                    }
                }

                //动态添加typeof(T).Name属性
               

            //}
            return  d;
        }
 
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.BLLClass
{
    public class WhItemResult
    {
        public int Id { get; set; }
        public string WhCode { get; set; }
        public string ItemNumber { get; set; }
        public string AltItemNumber { get; set; }
        public string Description { get; set; }//描述
        public string Style1 { get; set; }//属性1
        public string Style2 { get; set; }//属性2
        public string Style3 { get; set; }//属性3
        public int ClientId { get; set; }//客户表系统ID
        public string ClientCode { get; set; }//客户编码
        public string EAN { get; set; }
        public decimal Length { get; set; }//长
        public decimal Width { get; set; }//宽
        public decimal Height { get; set; }//高
        public decimal Weight { get; set; }//重量
        public int? HandFlag { get; set; }//包装是否可以输入数量
        public int? ScanFlag { get; set; }
        public string HandFlagShow { get; set; }
        public string ScanFlagShow { get; set; }
        public string ScanRule { get; set; }
        public int UnitFlag { get; set; }
        public int UnitId { get; set; }
        public int LocFlag { get; set; }
        public int LocOnHandFlag { get; set; }
        public int OnHandFlag { get; set; }
        public int OneItemLPFlag { get; set; }
        public int OneItemSizeLPFlag { get; set; }
        public string CreateUser { get; set; }//创建人
        public DateTime? CreateDate { get; set; }//创建时间
        public string ItemName { get; set; }//品名
        public string Category { get; set; } //Category
        public string Class { get; set; }//类别
        public string Style { get; set; }//款式
        public int? InstallService { get; set; }//送装服务
        public string Size { get; set; }//尺码
        public string Color { get; set; }//颜色
        public string Material { get; set; }//材质
        public string CusStyle { get; set; }//客户自定义类型
        public string PackageStyle { get; set; }//包装类型
        public string BoxCode { get; set; }//鞋盒编号
        public string OriginCountry { get; set; } //原产国
        public string UnitName { get; set; } //客户单位
        public DateTime? UpdateDate { get; set; }//修改时间
        public string UpdateUser { get; set; } //修改人
        public decimal? Pcs { get; set; } //箱规
        public string CusItemNumber { get; set; } //客户款号
        public string CartonName { get; set; } //包装耗材名称
    }

    public class WhItemSearch : BaseSearch
    {
        public string WhCode { get; set; }
        public string AltItemNumber { get; set; }
        public string UnitName { get; set; }

        public string ClientCode { get; set; }//客户编码
        public int? ClientId { get; set; }

        public string HandFlag { get; set; }
        public string ScanFlag { get; set; }

        public string ColorCode { get; set; }
    }

    public class ItemUnitResult
    {
        public int ItemId { get; set; }
        public string WhCode { get; set; }
        public string ItemNumber { get; set; }
        public string AltItemNumber { get; set; }
        public string Style1 { get; set; }
        public string Style2 { get; set; }
        public string Style3 { get; set; }
        public int ClientId { get; set; }
        public string ClientCode { get; set; }
        public string UnitName { get; set; }
        public int UnitFlag { get; set; }
        public int UnitId { get; set; }
        public int Proportion { get; set; }
    }

    public class ItemUnitSearch : BaseSearch
    {
        public string WhCode { get; set; }
        public string AltItemNumber { get; set; }
        public int? ClientId { get; set; }
    }
}

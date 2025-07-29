using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.BLLClass
{

    public class QueueParam
    {
        /// <summary>
        /// 头表Id
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 仓库编码
        /// </summary>
        public string WhCode { get; set; }
        /// <summary>
        /// 库区
        /// </summary>
        public string UnloadingArea { get; set; }
        /// <summary>
        /// 车牌号
        /// </summary>
        public string TruckNumber { get; set; }
        /// <summary>
        /// 插队卸货说明
        /// </summary>
        public string JumpingRemark { get; set; }
        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// 电话号码
        /// </summary>
        public string phoneNumber { get; set; }
        /// <summary>
        /// 通知类型
        /// </summary>
        public int type { get; set; }
        /// <summary>
        /// 预约通道
        /// </summary>
        public string BookChannel { get; set; }
        //当前页码
        public int pageNumber { get; set; }
        //小库区
        public string smallloadArea { get; set; }
    }
    //排队表头
    public class TruckQueueHeadParam
    {
        public int Id { get; set; }
        public string WhCode { get; set; }
        public string TruckNumber { get; set; }
        public string PhoneNumber { get; set; }
        public string TruckStatus { get; set; }
        public string TruckLength { get; set; }
        public string AllowTime { get; set; }
        public string AllowUser { get; set; }
        public string JumpingRemark { get; set; }
        public string EntryTime { get; set; }
        public string DepartureTime { get; set; }
        public string DepartureType { get; set; }
        public int? GreenPassFlag { get; set; }
        public string CreateUser { get; set; }
        public string CreateDate { get; set; }
        public string UpdateUser { get; set; }
        public string UpdateDate { get; set; }
        public string BookOrigin { get; set; }
        public string WMSWhCode { get; set; }
    }
    //排队表体
    public class TruckQueueDetailParam
    {
        public int Id { get; set; }
        public int HeadId { get; set; }
        public string WhCode { get; set; }
        public string ReceiptId { get; set; }
        public string UnloadingArea { get; set; }
        public string ClientCode { get; set; }
        public string BkNumber { get; set; }
        public int? Qty { get; set; }
        public decimal? CBM { get; set; }
        public decimal? Weight { get; set; }
        public string GoodsType { get; set; }
        public string BkDateBegin { get; set; }
        public string BkDateEnd { get; set; }
        public string RegisterDate { get; set; }
        public int BkIsValid { get; set; }
        public int SeeFlag { get; set; }
        public string SeeTime { get; set; }
        public string SeeUser { get; set; }
        public int OverSizeFlag { get; set; }
        public int? FeesStatus { get; set; }
        public string BookOrigin { get; set; }
        public string CreateUser { get; set; }
        public string CreateDate { get; set; }
        public string UpdateUser { get; set; }
        public string UpdateDate { get; set; }
        public int NoticeFlag { get; set; }
        public string BookChannel { get; set; }
    }
    //车辆排队信息实体
    public class TruckQueueInfo
    {
        public TruckQueueHeadParam truckQueueHeadParam { get; set; }
        public List<TruckQueueDetailParam> truckQueueDetailParamList { get; set; } //车辆排队明细
    }
    //车辆排队List
    public class TruckQueueList
    {
        public List<TruckQueueListDetail> DetailList { get; set; } //排队明细
    }
    public class TruckQueueListDetail
    {
        public int Id { get; set; }
        public int Seq { get; set; } //排序
        public string WhCode { get; set; }
        public string UnloadingArea { get; set; }
        public string TruckNumber { get; set; }
        public string PhoneNumber { get; set; }
        public string TruckLength { get; set; }
        
        public string Description { get; set; }
        public string ClientCode { get; set; }
        public string GreenPassFlag { get; set; }
        public decimal? CBM { get; set; }
        public int ResetCount { get; set; }
    }
    public class TruckGateIn
    {
        public string warehouseCode { get; set; }
        public string method { get; set; }
        public string truckNumber { get; set; }
        public string operatorUser { get; set; }
        public List<TruckGateInDetail> Details { get; set; }    
        
    }
    public class TruckGateInDetail
    {
        public string client { get; set; }
        public string receiptId { get; set; }
        public string GreenPassFlag { get; set; }
        public int ResetCount { get; set; }
    }
}

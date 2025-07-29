using MODEL_MSSQL;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection.Emit;
using System.Runtime.Remoting.Contexts;
using System.Runtime.Remoting.Messaging;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Transactions;
using WMS.BLLClass;
using WMS.DAL;
using WMS.Express;
using WMS.IBLL;

namespace WMS.BLL
{
    public class GMSManager : IGMSManager
    {
        IDAL.IDALSession idal = BLL.BLLHelper.GetDal();
        //CDM正式区
        public string CDMReqUrl = "http://10.88.88.108:5001/";
        //CDM UAT
        //public string CDMReqUrl = "https://cdmapi-uat.oceaneast-logistics.com/";
        //小程序正式区
        public string uniReqUrl = "https://wxapp.oceaneast-logistics.com/";
        //小程序UAT
        // public string uniReqUrl = "https://wxapp-uat.oceaneast-logistics.com/";

        //WMS登记后添加车辆
        public string WmsCreateGms(string WhCode, string ReceiptId)
        {
            if (WhCode == "03")
            {
                return "保税区暂时关闭";
            }

            //获取批次信息
            ReceiptRegister receiptRegister = null;
            List<ReceiptRegister> ReceiptRegisterList = idal.IReceiptRegisterDAL.SelectBy(u => u.WhCode == WhCode && u.ReceiptId == ReceiptId).ToList();
            if (ReceiptRegisterList.Count > 0)
            {
                receiptRegister = ReceiptRegisterList[0];
            }
            else
            {
                return "N$收货批次不存在";
            }


            GMSManager gm = new GMSManager();
            List<TruckQueueInfo> truckQueueInfo = new List<TruckQueueInfo>();
            TruckQueueInfo ti = new TruckQueueInfo();
            ti.truckQueueDetailParamList = new List<TruckQueueDetailParam>();
            ti.truckQueueHeadParam = new TruckQueueHeadParam();
            //ti.truckQueueHeadParam.Id            = 0;
            ti.truckQueueHeadParam.WhCode = WhCode;
            ti.truckQueueHeadParam.TruckNumber = receiptRegister.TruckNumber;
            ti.truckQueueHeadParam.PhoneNumber = receiptRegister.PhoneNumber;
            ti.truckQueueHeadParam.TruckStatus = "1";
            ti.truckQueueHeadParam.TruckLength = receiptRegister.TruckLength;
            ti.truckQueueHeadParam.AllowTime = null;
            ti.truckQueueHeadParam.AllowUser = null;
            ti.truckQueueHeadParam.JumpingRemark = null;
            ti.truckQueueHeadParam.EntryTime = null;
            ti.truckQueueHeadParam.DepartureTime = null;
            ti.truckQueueHeadParam.DepartureType = null;
            ti.truckQueueHeadParam.GreenPassFlag = receiptRegister.GreenPassFLag;
            ti.truckQueueHeadParam.CreateUser = "WMS";
            ti.truckQueueHeadParam.CreateDate = null;
            ti.truckQueueHeadParam.UpdateUser = null;
            ti.truckQueueHeadParam.UpdateDate = null;
            ti.truckQueueHeadParam.BookOrigin = "WMS";
            ti.truckQueueHeadParam.WMSWhCode = WhCode;
            TruckQueueDetailParam td = new TruckQueueDetailParam();
            //td.Id = null;
            //td.HeadId = null;
            td.WhCode = WhCode;
            td.ReceiptId = ReceiptId;
            td.UnloadingArea = receiptRegister.LocationId;
            td.ClientCode = receiptRegister.ClientCode;
            td.BkNumber = null;
            td.Qty = receiptRegister.SumQty;
            td.CBM = receiptRegister.SumCBM;
            td.Weight = 0;
            td.GoodsType = receiptRegister.GoodType;
            td.BkDateBegin = receiptRegister.BkDateBegin.ToString();
            td.BkDateEnd = receiptRegister.BkDateEnd.ToString();
            td.RegisterDate = receiptRegister.RegisterDate.ToString();
            td.BkIsValid = receiptRegister.RegisterDate < receiptRegister.BkDateEnd ? 1 : 0;
            td.SeeFlag = 0;
            td.SeeTime = null;
            td.SeeUser = null;
            td.OverSizeFlag = 0;
            td.FeesStatus = 0;
            td.BookOrigin = null;
            td.CreateUser = "WMS";
            td.CreateDate = null;
            td.UpdateUser = null;
            td.UpdateDate = null;
            td.NoticeFlag = 1;
            td.BookChannel = WhCode;
            ti.truckQueueDetailParamList.Add(td);
            truckQueueInfo.Add(ti);

            string res = gm.GetTruckQueueInfo(truckQueueInfo);


            return res;
        }



        //放车
        public string ReleaseTruck(QueueParam queueParam)
        {
            string res = "";
            try
            {
                if (queueParam.smallloadArea == null)
                {
                    queueParam.smallloadArea = queueParam.UnloadingArea;
                }
                int id = Convert.ToInt32(queueParam.Id);
                List<TruckQueueHead> truckQueueHeadList = idal.ITruckQueueHeadDAL.SelectBy(u => u.Id == id).ToList();
                TruckQueueHead truckQueueHead = truckQueueHeadList[0];
                List<TruckQueueDetail> truckQueueDetailList = idal.ITruckQueueDetailDAL.SelectBy(u => u.HeadId == truckQueueHead.Id && u.UnloadingArea == queueParam.UnloadingArea).ToList();
                TruckQueueDetail truckQueueDetail = truckQueueDetailList[0];
                int isFlag = 0;// 记录seeFlag是否存在不为1的个数
                int isNotice = 0;//记录批次是否需要通知存在1的则需通知
                int isOverSize = 0;//记录是否为超规货物
                for (int i = 0; i < truckQueueDetailList.Count; i++)
                {
                    if (truckQueueDetailList[i].SeeFlag != 1)
                    {
                        isFlag += 1;
                    }
                    if (truckQueueDetailList[i].NoticeFlag == 1)
                    {
                        isNotice += 1;
                    }
                    if (truckQueueDetailList[i].OverSizeFlag == 1)
                    {
                        isOverSize += 1;
                    }
                }
                if (isNotice > 0)
                {
                    queueParam.phoneNumber = truckQueueHead.PhoneNumber;
                    //queueParam.phoneNumber = "18814183946";
                    queueParam.BookChannel = truckQueueDetail.BookChannel;
                    if (isOverSize > 0)
                    {
                        queueParam.type = 1;//超规货物车辆
                    }
                    else
                    {
                        queueParam.type = 0;//标准车辆
                    }
                    if (isFlag > 0)
                    {
                        res = notification(queueParam);//放车通知
                        if (truckQueueHead.TruckStatus == "1")
                        {

                            UpdateTruckQueueHeader(queueParam, truckQueueHead);//更新表头信息，状态变为待入库
                            createTranLog(queueParam, truckQueueHead, truckQueueDetail, "1000");

                        }
                        else
                        {  //已入库车辆放车更新AMS信息

                            if (queueParam.WhCode == "03")
                                updateAMSData(id, queueParam.UnloadingArea, 1, queueParam.TruckNumber);

                            res += "该车辆其他库区已放车!请等待,或者先放下一部车!";

                        }

                        createTranLog(queueParam, truckQueueHead, truckQueueDetail, "1001");
                    }
                    else
                    {
                        res = "该车辆已放车，不要重复放车，请刷新页面查看！";
                        return res;
                    }
                }
                else
                {
                    res = "该车辆未预约，系统不会发送短信和语音信息给司机，请自行通知！";
                }
                //if (truckQueueHead.TruckStatus == "1" && isFlag > 0)  //当TruckStatus为1排队中,SeeFlag不为1
                //{
                //    if (isNotice > 0)
                //    {
                //        queueParam.phoneNumber = truckQueueHead.PhoneNumber;
                //        queueParam.BookChannel = truckQueueDetail.BookChannel;
                //        if (isOverSize > 0)
                //        {
                //            queueParam.type = 1;
                //            res = notification(queueParam);//超规货物车辆发送通知
                //        }
                //        else
                //        {
                //            queueParam.type = 0;
                //            res = notification(queueParam);//标准车发送通知
                //        }
                //    }
                //    else
                //    {
                //        res = "该车辆未预约，系统不会发送短信和语音信息给司机，请自行通知！";
                //    }
                //    UpdateTruckQueueHeader(queueParam, truckQueueHead);//更新表头信息，状态变为待入库
                //    createTranLog(queueParam, truckQueueHead, truckQueueDetail, "1000");
                //    createTranLog(queueParam, truckQueueHead, truckQueueDetail, "1001");
                //}
                //else if (truckQueueHead.TruckStatus != "1" && isFlag > 0) //已入仓在其他库区车辆放车
                //{
                //    createTranLog(queueParam, truckQueueHead, truckQueueDetail, "1001");
                //    res = "该车辆已在其他库区卸货";
                //    //已入库车辆放车更新AMS信息
                //    updateAMSData(id, queueParam.UnloadingArea, 1, queueParam.TruckNumber);
                //}
                //else
                //{
                //    res = "该车辆已放车，不要重复放车，请刷新页面查看！";
                //    return res;
                //}
                //更新该库区的明细SeeFlag
                truckQueueDetail.SeeFlag = 1;
                truckQueueDetail.UpdateUser = queueParam.UserName;
                truckQueueDetail.UpdateDate = DateTime.Now;
                truckQueueDetail.SeeUser = queueParam.UserName;
                truckQueueDetail.SeeTime = DateTime.Now;
                truckQueueDetail.SmallLoadingArea = queueParam.smallloadArea;
                idal.ITruckQueueDetailDAL.UpdateBy(truckQueueDetail, u => u.HeadId == truckQueueHead.Id && u.UnloadingArea == queueParam.UnloadingArea && u.WhCode == queueParam.WhCode, new string[] { "SeeFlag", "UpdateUser", "UpdateDate", "SeeUser", "SeeTime", "SmallLoadingArea" });
                idal.SaveChanges();
                return res;
            }
            catch (Exception ex)
            {
                res = ex.Message.ToString();
                return res;
            }
        }







        //更新表头信息
        public void UpdateTruckQueueHeader(QueueParam queueParam, TruckQueueHead truckQueueHead)
        {
            int id = Convert.ToInt32(queueParam.Id);
            truckQueueHead.TruckStatus = "2";//车辆状态变为待入库
            truckQueueHead.AllowTime = DateTime.Now;
            truckQueueHead.AllowUser = queueParam.UserName;
            truckQueueHead.UpdateUser = queueParam.UserName;
            truckQueueHead.UpdateDate = DateTime.Now;
            if (queueParam.JumpingRemark + "" != "")
            {
                truckQueueHead.JumpingRemark = queueParam.JumpingRemark;
            }
            idal.ITruckQueueHeadDAL.UpdateBy(truckQueueHead, u => u.Id == id, new string[] { "TruckStatus", "AllowTime", "AllowUser", "UpdateUser", "UpdateDate", "JumpingRemark" });
            //idal.SaveChanges();
        }
        public void createTranLog(QueueParam queueParam, TruckQueueHead truckQueueHead, TruckQueueDetail truckQueueDetail, string type)
        {
            TranLog tl = new TranLog();
            tl.LotNumber1 = truckQueueHead.TruckNumber;
            tl.LotNumber2 = truckQueueHead.PhoneNumber;
            tl.TranType = type;
            tl.TranDate = DateTime.Now;
            if (type == "1000") //放车通知Log
            {
                tl.Description = "车辆放车入库";
                tl.Remark = queueParam.UnloadingArea + "车辆放车入库";
                tl.TranUser = queueParam.UserName;
                tl.WhCode = queueParam.WhCode;
                tl.ClientCode = truckQueueDetail.ClientCode;
                tl.ReceiptId = truckQueueDetail.ReceiptId;
                tl.Location = queueParam.UnloadingArea;
            }
            else if (type == "1001")          //放车已阅Log
            {
                tl.Description = "放车业务确认";
                tl.Remark = queueParam.UnloadingArea + "放车业务确认";
                tl.TranUser = queueParam.UserName;
                tl.WhCode = queueParam.WhCode;
                tl.ClientCode = truckQueueDetail.ClientCode;
                tl.ReceiptId = truckQueueDetail.ReceiptId;
                tl.Location = queueParam.UnloadingArea;
            }
            else if (type == "1002" || type == "1004")  //删除排队明细
            {
                tl.Description = "车辆排队明细删除";
                tl.Remark = truckQueueDetail.UnloadingArea + "车辆排队明细删除" + truckQueueDetail.BkNumber;
                tl.TranUser = queueParam.UserName;
                tl.WhCode = truckQueueDetail.WhCode;
                tl.ClientCode = truckQueueDetail.ClientCode;
                tl.ReceiptId = truckQueueDetail.ReceiptId;
                tl.Location = truckQueueDetail.UnloadingArea + "通道：" + truckQueueDetail.BookChannel;
                tl.LotDate = truckQueueDetail.RegisterDate;
            }
            else if (type == "1003")  //删除排队表头信息
            {
                tl.Description = "车辆排队表头删除";
                tl.Remark = "车辆排队表头删除";
                tl.TranUser = queueParam.UserName;
                tl.WhCode = truckQueueHead.WhCode;
                tl.Location = truckQueueHead.BookOrigin;
            }
            else if (type == "1005")//修改表头log
            {
                tl.Description = "车辆表头信息修改";
                tl.Remark = "车辆表头信息修改";
                tl.TranUser = truckQueueHead.UpdateUser;
                tl.WhCode = truckQueueHead.WhCode;
                tl.Location = truckQueueHead.BookOrigin;
                tl.HoldReason = "车长" + truckQueueHead.TruckLength;
            }
            else if (type == "1006")
            {
                tl.Description = "车辆排队明细修改";
                tl.Remark = truckQueueDetail.UnloadingArea + "车辆排队明细修改" + truckQueueDetail.BkNumber;
                tl.TranUser = truckQueueDetail.UpdateUser;
                tl.WhCode = truckQueueDetail.WhCode;
                tl.ClientCode = truckQueueDetail.ClientCode;
                tl.ReceiptId = truckQueueDetail.ReceiptId;
                tl.Location = truckQueueDetail.UnloadingArea + "通道：" + truckQueueDetail.BookChannel;
                tl.LotDate = truckQueueDetail.RegisterDate;
            }
            else if (type == "1008")
            {
                tl.Description = "入库超时车辆解锁";
                tl.Remark = "入库超时锁定车辆解锁" + truckQueueDetail.BkNumber;
                tl.WhCode = queueParam.WhCode;
                tl.LoadId = queueParam.Id;
                tl.LotDate = truckQueueDetail.RegisterDate;
            }
            else if (type == "1009")
            {
                tl.Description = "入库车辆超时解锁明细";
                tl.Remark = "入库车辆超时解锁明细收货登记";
                tl.TranUser = queueParam.UserName;
                tl.WhCode = queueParam.WhCode;
                tl.Location = truckQueueDetail.UnloadingArea;
                tl.LotDate = truckQueueDetail.RegisterDate;
                tl.ReceiptId = truckQueueDetail.ReceiptId;
            }
            idal.ITranLogDAL.Add(tl);
            //   idal.SaveChanges();
        }
        //通知方法接口
        public string notification(QueueParam queueParam)
        {
            //type  1 超规  0 标准
            string res = "";
            Dictionary<string, object> dicHeader = new Dictionary<string, object>();
            dicHeader.Add("type", "SMS,TTS");
            Dictionary<string, object> VoiceInfoDetail = new Dictionary<string, object>();
            Dictionary<string, object> SmsInfoDetail = new Dictionary<string, object>();
            //语音
            VoiceInfoDetail.Add("phoneNumber", queueParam.phoneNumber);
            VoiceInfoDetail.Add("sourceFrom", queueParam.BookChannel);
            //短信
            SmsInfoDetail.Add("phoneNumber", queueParam.phoneNumber);
            SmsInfoDetail.Add("sourceFrom", queueParam.BookChannel);
            if (queueParam.type == 0)//标准货物进仓通知
            {
                VoiceInfoDetail.Add("template", "TTS_304770037");
                VoiceInfoDetail.Add("content", queueParam.TruckNumber + "|" + queueParam.smallloadArea);
                SmsInfoDetail.Add("template", "sms-tmpl-WWdFHk94108");
                SmsInfoDetail.Add("content", queueParam.TruckNumber + "|" + queueParam.smallloadArea);
            }
            else  //超规货物进仓通知
            {
                VoiceInfoDetail.Add("template", "TTS_304765039");
                VoiceInfoDetail.Add("content", queueParam.TruckNumber + "|" + queueParam.smallloadArea + "2楼");
                SmsInfoDetail.Add("template", "sms-tmpl-dXsZxV13892");
                SmsInfoDetail.Add("content", queueParam.TruckNumber + "|" + queueParam.smallloadArea + "|2楼");
            }
            dicHeader.Add("VoiceInfoDetail", VoiceInfoDetail);
            dicHeader.Add("SmsInfoDetail", SmsInfoDetail);
            string json = JsonConvert.SerializeObject(dicHeader);
            string strUrl = uniReqUrl + "NoticeInfo/NotificationInterface";
            //string strUrl = "http://localhost:3103/NoticeInfo/NotificationInterface";
            string response = CreatePostHttpResponse(strUrl, "POST", json);
            Dictionary<string, object> resultList = JsonConvert.DeserializeObject<Dictionary<string, object>>(response.ToString());
            int code = int.Parse(resultList["code"].ToString());
            if (code == 200)
            {
                Dictionary<string, object> resData = JsonConvert.DeserializeObject<Dictionary<string, object>>(resultList["data"].ToString());
                Dictionary<string, object> voiceRes = JsonConvert.DeserializeObject<Dictionary<string, object>>(resData["voiceRes"].ToString());
                Dictionary<string, object> smsRes = JsonConvert.DeserializeObject<Dictionary<string, object>>(resData["smsRes"].ToString());
                //res = "语音："+voiceRes["msg"].ToString()+";短信："+ smsRes["msg"].ToString();
                res = "操作成功";
            }
            else
            {
                res = "通知失败！";
            }
            return res;
        }

        //收货登记修改表头后更改对应GMS信息

        
        public string UpdateTruckInfoByReceiptRegister(ReceiptRegister entity, params string[] modifiedProNames)
        {
            try { 
                TruckQueueDetail tqd = null;
                List<TruckQueueDetail> tqd_l = idal.ITruckQueueDetailDAL.SelectBy(u => u.ReceiptId == entity.ReceiptId && u.WhCode == entity.WhCode);
                if (tqd_l.Count > 0)
                {
                    tqd = tqd_l.First();
                }
                int HeadId = (int)tqd.HeadId;
                TruckQueueHead tqh = idal.ITruckQueueHeadDAL.SelectBy(u => u.Id == HeadId && u.WhCode == entity.WhCode).First();

                //TruckNumber,PhoneNumber,GreenPassFLag,GoodType
                if (tqh.PhoneNumber != entity.PhoneNumber)
                {
                    tqh.PhoneNumber = entity.PhoneNumber;
                    idal.ITruckQueueHeadDAL.UpdateBy(tqh, u => u.Id == HeadId, new string[] { "PhoneNumber" });
                }
                if (tqh.GreenPassFlag != entity.GreenPassFLag)
                {
                    tqh.GreenPassFlag = entity.GreenPassFLag;
                    idal.ITruckQueueHeadDAL.UpdateBy(tqh, u => u.Id == HeadId, new string[] { "GreenPassFlag" });
                }
                if (tqd.GoodsType != entity.GoodType)
                {
                    tqd.GoodsType = entity.GoodType;
                    idal.ITruckQueueDetailDAL.UpdateBy(tqd, u => u.Id == HeadId, new string[] { "GoodsType" });
                }
                if (tqd.CBM != entity.SumCBM)
                {
                    tqd.CBM = entity.SumCBM;
                    idal.ITruckQueueDetailDAL.UpdateBy(tqd, u => u.Id == HeadId, new string[] { "CBM" });
                }
                if (tqh.TruckNumber != entity.TruckNumber || tqd.UnloadingArea != entity.LocationId)
                {
                    DeleteTruckInfoByReceiptRegister(entity.ReceiptId, entity.WhCode);
                    WmsCreateGms(entity.WhCode, entity.ReceiptId);

                    TruckQueueHead tqh_new = idal.ITruckQueueHeadDAL.SelectBy(u => u.TruckNumber == entity.TruckNumber && u.WhCode == entity.WhCode && u.TruckStatus=="1").First();
                    tqh_new.TruckStatus = tqh.TruckStatus;
                    tqh_new.AllowUser = tqh.AllowUser;
                    tqh_new.AllowTime = tqh.AllowTime;
                    tqh_new.JumpingRemark = tqh.JumpingRemark;
                    tqh_new.EntryTime = tqh.EntryTime;
                    tqh_new.TruckStatus = tqh.TruckStatus;
                    idal.ITruckQueueHeadDAL.UpdateBy(tqh_new, u => u.Id == HeadId, new string[] { "TruckStatus", "AllowUser", "AllowTime", "JumpingRemark", "EntryTime" });

                    int? SeeFlag_new = 0;
                    if (tqh.TruckStatus != "1")
                        SeeFlag_new = 1;

                    int HeadId_new = tqh_new.Id;
                    TruckQueueDetail truckQueueDetail = new TruckQueueDetail();
                    truckQueueDetail.SeeFlag = SeeFlag_new;
                    idal.ITruckQueueDetailDAL.UpdateBy(truckQueueDetail, u => u.HeadId == HeadId, new string[] { "SeeFlag" });



                }
                idal.SaveChanges();
            }
            catch (Exception E)
            {
                return "N";
            }

            return "Y";
        }



        //收货登记删除后调用删除GMS对应信息
        public string DeleteTruckInfoByReceiptRegister(string ReceiptId, string WhCode)
        {
            try
            {

                TruckQueueHead truckQueueHead = null;
                QueueParam qp = new QueueParam();
                int Id = 0;



                List<TruckQueueDetail> recDetailList = idal.ITruckQueueDetailDAL.SelectBy(u => u.ReceiptId == ReceiptId && u.WhCode == WhCode);
                if (recDetailList.Count > 0)
                {
                    Id = (int)recDetailList.First().HeadId;
                    truckQueueHead = idal.ITruckQueueHeadDAL.SelectBy(u => u.Id == Id).First();

                    qp.Id = truckQueueHead.Id.ToString();
                    qp.WhCode = truckQueueHead.WhCode;
                    qp.UnloadingArea = recDetailList.First().UnloadingArea;
                    qp.TruckNumber = truckQueueHead.TruckNumber;
                    qp.UserName = "WMS";

                }

                //int id = Convert.ToInt32(queueParam.Id);

                List<TruckQueueDetail> truckQueueDetailList = idal.ITruckQueueDetailDAL.SelectBy(u => u.HeadId == Id);


                if (truckQueueDetailList.Count == 1)
                {
                    // DeleteTruckInfoByReceipt(qp, ReceiptId);
                    DeleteTruckInfo(qp);
                }
                else if (truckQueueDetailList.Count > 1 && recDetailList.Count > 0)
                {
                    createTranLog(qp, truckQueueHead, truckQueueDetailList.First(), "1002");

                    List<TruckQueueDetail> truckQueueDetailListReceiptId = idal.ITruckQueueDetailDAL.SelectBy(u => u.HeadId == Id && u.ReceiptId== ReceiptId);
                    int GetId = truckQueueDetailListReceiptId.First().Id;
                    //删除明细信息
                    idal.ITruckQueueDetailDAL.DeleteBy(u => u.Id == GetId);
                    idal.SaveChanges();
                }


            }
            catch (Exception ex)
            {
                return "N";
            }

            return "Y";
        }







      




        ////删除车辆排队信息
        public string DeleteTruckInfo(QueueParam queueParam)
        {
            string res = "";
            try
            {
                int id = Convert.ToInt32(queueParam.Id);
                TruckQueueHead truckQueueHead = idal.ITruckQueueHeadDAL.SelectBy(u => u.Id == id && u.TruckNumber == queueParam.TruckNumber).First();
                if (truckQueueHead.TruckStatus != "5")
                {
                    List<TruckQueueDetail> truckQueueDetailList = idal.ITruckQueueDetailDAL.SelectBy(u => u.HeadId == id);
                    foreach (var item in truckQueueDetailList)
                    {
                        createTranLog(queueParam, truckQueueHead, item, "1002");
                        //删除明细信息
                        idal.ITruckQueueDetailDAL.DeleteBy(u => u.Id == item.Id);
                    }
                    //删除排队表头信息
                    createTranLog(queueParam, truckQueueHead, null, "1003");
                    idal.ITruckQueueHeadDAL.DeleteBy(u => u.Id == id && u.TruckNumber == queueParam.TruckNumber);
                    idal.SaveChanges();
                    res = "删除成功！";
                }
                else
                {
                    res = "已离库车辆不能删除信息！";
                }
                return res;
            }
            catch (Exception ex)
            {
                res = ex.Message.ToString();
                return res;
            }
        }
        //删除车牌排队明细信息
        public string DeleteTruckDetail(QueueParam queueParam)
        {
            string res = "";
            try
            {
                int id = Convert.ToInt32(queueParam.Id);
                TruckQueueDetail truckQueueDetail = idal.ITruckQueueDetailDAL.SelectBy(u => u.Id == id).First();
                TruckQueueHead truckQueueHead = idal.ITruckQueueHeadDAL.SelectBy(u => u.Id == truckQueueDetail.HeadId).First();
                if (truckQueueHead.TruckStatus != "5")
                {
                    createTranLog(queueParam, truckQueueHead, truckQueueDetail, "1004");
                    //删除明细信息
                    idal.ITruckQueueDetailDAL.DeleteBy(u => u.Id == id);
                    idal.SaveChanges();
                    res = "删除成功！";
                }
                else
                {
                    res = "已离库车辆不能删除信息！";
                }
                return res;
            }
            catch (Exception ex)
            {
                res = ex.Message.ToString();
                return res;
            }
        }
        public string AddTruckQueueHead(TruckQueueHeadParam truckQueueHeadParam)
        {
            string res = "";
            try
            {
                TruckQueueHead truckQueueHead = new TruckQueueHead();
                truckQueueHead.WhCode = truckQueueHeadParam.WhCode;
                truckQueueHead.TruckNumber = truckQueueHeadParam.TruckNumber;
                truckQueueHead.PhoneNumber = truckQueueHeadParam.PhoneNumber;
                truckQueueHead.TruckLength = truckQueueHeadParam.TruckLength;
                truckQueueHead.GreenPassFlag = truckQueueHeadParam.GreenPassFlag;
                truckQueueHead.BookOrigin = truckQueueHeadParam.BookOrigin;
                truckQueueHead.TruckStatus = truckQueueHeadParam.TruckStatus;
                truckQueueHead.CreateDate = DateTime.Now;
                truckQueueHead.CreateUser = truckQueueHeadParam.CreateUser;
                truckQueueHead.WMSWhCode = truckQueueHeadParam.WMSWhCode;
                idal.ITruckQueueHeadDAL.Add(truckQueueHead);
                idal.SaveChanges();
                res = "新增成功";
                return res;
            }
            catch (Exception ex)
            {
                res = ex.Message.ToString();
                return res;
            }
        }
        //获取预约通道
        public IEnumerable<LookUp> BookChannelSelect(string whCode)
        {
            var sql = from a in idal.ILookUpDAL.SelectAll()
                      where a.TableName == "TruckQueueDetail" && a.ColumnName == "BookChannel" && a.ColumnKey == whCode
                      select a;
            return sql.AsEnumerable();
        }
        ////新增车辆明细信息
        public string AddTruckQueueDetail(TruckQueueDetailParam truckQueueDetailParam)
        {
            string res = "";
            try
            {
                TruckQueueDetail truckQueueDetail = new TruckQueueDetail();
                truckQueueDetail.ReceiptId = truckQueueDetailParam.ReceiptId;
                truckQueueDetail.ClientCode = truckQueueDetailParam.ClientCode;
                truckQueueDetail.UnloadingArea = truckQueueDetailParam.UnloadingArea;
                truckQueueDetail.BookChannel = truckQueueDetailParam.BookChannel;
                truckQueueDetail.NoticeFlag = truckQueueDetailParam.NoticeFlag;
                truckQueueDetail.SeeFlag = truckQueueDetailParam.SeeFlag;
                truckQueueDetail.BookOrigin = truckQueueDetailParam.BookOrigin;
                truckQueueDetail.CBM = truckQueueDetailParam.CBM;
                truckQueueDetail.Weight = truckQueueDetailParam.Weight;
                truckQueueDetail.Qty = truckQueueDetailParam.Qty;
                truckQueueDetail.CreateUser = truckQueueDetailParam.CreateUser;
                if (truckQueueDetailParam.GoodsType != null && truckQueueDetailParam.GoodsType != "")
                {
                    truckQueueDetail.GoodsType = truckQueueDetailParam.GoodsType;
                }
                truckQueueDetail.OverSizeFlag = truckQueueDetailParam.OverSizeFlag;
                if (truckQueueDetailParam.RegisterDate != null && truckQueueDetailParam.RegisterDate != "")
                {
                    truckQueueDetail.RegisterDate = DateTime.Parse(truckQueueDetailParam.RegisterDate);
                }
                else
                {
                    truckQueueDetail.RegisterDate = DateTime.Now;
                }
                truckQueueDetail.WhCode = truckQueueDetailParam.WhCode;
                truckQueueDetail.HeadId = truckQueueDetailParam.HeadId;
                truckQueueDetail.CreateDate = DateTime.Now;
                if (truckQueueDetailParam.BkDateBegin != null && truckQueueDetailParam.BkDateBegin != "")
                {
                    truckQueueDetail.BkDateBegin = DateTime.Parse(truckQueueDetailParam.BkDateBegin);
                }
                if (truckQueueDetailParam.BkDateEnd != null && truckQueueDetailParam.BkDateEnd != "")
                {
                    truckQueueDetail.BkDateEnd = DateTime.Parse(truckQueueDetailParam.BkDateEnd);
                }
                if (truckQueueDetailParam.BkNumber != null)
                {
                    truckQueueDetail.BkNumber = truckQueueDetailParam.BkNumber;
                }
                truckQueueDetail.BkIsValid = truckQueueDetailParam.BkIsValid;
                idal.ITruckQueueDetailDAL.Add(truckQueueDetail);
                idal.SaveChanges();
                res = "明细新增成功";
            }
            catch (Exception ex)
            {
                res = ex.Message.ToString();
            }
            return res;
        }
        //修改车辆表头信息
        public string UpdateTruckQueueHeader(TruckQueueHeadParam truckQueueHeadParam)
        {
            string res = "";
            try
            {
                if (truckQueueHeadParam.Id == 0)//没有Id，则根据车牌号及车辆状态查询出Id，进行更新
                {
                    TruckQueueHead truckQueueHead1 = idal.ITruckQueueHeadDAL.SelectBy(u => u.TruckNumber == truckQueueHeadParam.TruckNumber && u.TruckStatus != "5").First();
                    truckQueueHeadParam.Id = truckQueueHead1.Id;
                }
                TruckQueueHead truckQueueHead = idal.ITruckQueueHeadDAL.SelectBy(u => u.Id == truckQueueHeadParam.Id).First();
                //if(truckQueueHead.BookOrigin=="USER" && truckQueueHead.TruckStatus == "1")
                //{
                createTranLog(null, truckQueueHead, null, "1005");
                if (truckQueueHeadParam.TruckNumber + "" != "")
                {
                    truckQueueHead.TruckNumber = truckQueueHeadParam.TruckNumber;
                }
                if (truckQueueHeadParam.PhoneNumber + "" != "")
                {
                    truckQueueHead.PhoneNumber = truckQueueHeadParam.PhoneNumber;
                }
                if (truckQueueHeadParam.TruckLength + "" != "")
                {
                    truckQueueHead.TruckLength = truckQueueHeadParam.TruckLength;
                }
                if (truckQueueHeadParam.BookOrigin + "" != "")
                {
                    truckQueueHead.BookOrigin = truckQueueHeadParam.BookOrigin;
                }
                if (truckQueueHead.GreenPassFlag + "" != "")
                {
                    truckQueueHead.GreenPassFlag = truckQueueHeadParam.GreenPassFlag;
                }
                truckQueueHead.UpdateUser = truckQueueHeadParam.UpdateUser;
                truckQueueHead.UpdateDate = DateTime.Now;
                idal.ITruckQueueHeadDAL.UpdateBy(truckQueueHead, u => u.Id == truckQueueHeadParam.Id, new string[] { "TruckNumber", "PhoneNumber", "TruckLength", "BookOrigin", "UpdateUser", "UpdateDate", "GreenPassFlag" });
                idal.SaveChanges();
                res = "修改成功！";
                //}
                //else
                //{
                //    res = "该车辆数据为EDI数据或车辆已入库，不可修改！";
                //}
            }
            catch (Exception ex)
            {
                res = ex.Message.ToString();
            }
            return res;
        }
        //修改车辆明细信息
        public string UpdateTruckQueueDetail(TruckQueueDetailParam truckQueueDetailParam)
        {
            string res = "";
            try
            {
                TruckQueueDetail truckQueueDetail = idal.ITruckQueueDetailDAL.SelectBy(u => u.Id == truckQueueDetailParam.Id).First();
                TruckQueueHead truckQueueHead = idal.ITruckQueueHeadDAL.SelectBy(u => u.Id == truckQueueDetail.HeadId).First();
                if (truckQueueHead.TruckStatus == "1")
                {
                    createTranLog(null, truckQueueHead, truckQueueDetail, "1006");
                    truckQueueDetail.ReceiptId = truckQueueDetailParam.ReceiptId;
                    truckQueueDetail.UnloadingArea = truckQueueDetailParam.UnloadingArea;
                    truckQueueDetail.ClientCode = truckQueueDetailParam.ClientCode;
                    truckQueueDetail.Qty = truckQueueDetailParam.Qty;
                    truckQueueDetail.CBM = truckQueueDetailParam.CBM;
                    truckQueueDetail.Weight = truckQueueDetailParam.Weight;
                    truckQueueDetail.GoodsType = truckQueueDetailParam.GoodsType;
                    truckQueueDetail.RegisterDate = DateTime.Parse(truckQueueDetailParam.RegisterDate);
                    truckQueueDetail.BkIsValid = truckQueueDetailParam.BkIsValid;
                    truckQueueDetail.NoticeFlag = truckQueueDetailParam.NoticeFlag;
                    truckQueueDetail.OverSizeFlag = truckQueueDetailParam.OverSizeFlag;
                    truckQueueDetail.BookOrigin = truckQueueDetailParam.BookOrigin;
                    truckQueueDetail.UpdateUser = truckQueueDetailParam.UpdateUser;
                    truckQueueDetail.UpdateDate = DateTime.Now;
                    idal.ITruckQueueDetailDAL.UpdateBy(truckQueueDetail, u => u.Id == truckQueueDetailParam.Id, new string[] { "ReceiptId", "UnloadingArea", "ClientCode", "Qty", "CBM", "Weight", "GoodsType", "RegisterDate", "BkIsValid", "NoticeFlag", "OverSizeFlag", "BookOrigin", "UpdateUser", "UpdateDate" });
                    idal.SaveChanges();
                    res = "修改成功！";
                }
                else
                {
                    res = "该车辆数据为EDI数据或车辆已入库，不可修改";
                }
            }
            catch (Exception ex)
            {
                res = ex.Message.ToString();
            }
            return res;
        }
        //车辆排队信息接收接口
        public string GetTruckQueueInfo(List<TruckQueueInfo> truckQueueInfoList)
        {
            string res = "";
            try
            {
                var TruckNumberEmpty = truckQueueInfoList
               .Where(dto => string.IsNullOrEmpty(dto.truckQueueHeadParam.TruckNumber))
               .Select(dto => dto.truckQueueHeadParam.PhoneNumber)
               .ToArray(); // 检查车牌号是否为空
                if (TruckNumberEmpty.Length > 0) res += "以下手机号对应的车牌号不存在: " + string.Join(", ", TruckNumberEmpty);
                // var TruckNumberInvalid = truckQueueInfoList
                //.Where(dto => !IsVehicleNumber(dto.truckQueueHeadParam.TruckNumber))
                //.Select(dto => dto.truckQueueHeadParam.TruckNumber)
                //.ToArray(); // 检查车牌号格式是否正确
                // if ( TruckNumberInvalid.Length>0) res += "以下车牌号格式错误: " + string.Join(", ", TruckNumberInvalid);
                foreach (var item in truckQueueInfoList)
                {
                    res += isTruckDetailExist(item.truckQueueDetailParamList);
                }
                if (res != "")
                {
                    return res;
                }
                foreach (var truckQueueInfo in truckQueueInfoList)
                {
                    List<TruckQueueHead> truckQueueHeadList = idal.ITruckQueueHeadDAL.SelectBy(u => u.TruckNumber == truckQueueInfo.truckQueueHeadParam.TruckNumber && u.TruckStatus != "5" && u.WhCode == truckQueueInfo.truckQueueHeadParam.WhCode).ToList();
                    TruckQueueHead truckQueueHead = new TruckQueueHead();
                    if (truckQueueHeadList.Count == 0)
                    {
                        truckQueueInfo.truckQueueHeadParam.TruckStatus = "1";
                        truckQueueInfo.truckQueueHeadParam.BookOrigin = "EDI";
                        res += AddTruckQueueHead(truckQueueInfo.truckQueueHeadParam);
                        if (res == "新增成功")
                        {
                            res = "";
                        }
                        truckQueueHead = idal.ITruckQueueHeadDAL.SelectBy(u => u.TruckNumber == truckQueueInfo.truckQueueHeadParam.TruckNumber && u.TruckStatus != "5" && u.WhCode == truckQueueInfo.truckQueueHeadParam.WhCode).ToList().First();
                    }
                    else
                    {
                        truckQueueHead = truckQueueHeadList[0];
                    }
                    res += addTruckQueueDetailList(truckQueueInfo.truckQueueDetailParamList, truckQueueHead, truckQueueHead.TruckStatus);
                    if (res == "明细新增成功")
                    {
                        res = "";
                    }
                }
                if (res == "")
                {
                    res = "接收成功！";
                }
            }
            catch (Exception ex)
            {
                res = ex.Message.ToString();
            }
            return res;
        }
        //新增车辆明细
        public string addTruckQueueDetailList(List<TruckQueueDetailParam> detailList, TruckQueueHead truckQueueHead, string TruckStatus)
        {
            string res = "";
            int HeadId = truckQueueHead.Id;
            try
            {
                if (detailList.Count > 0)
                {
                    foreach (var item in detailList)
                    {
                        //if (TruckStatus != "1" && TruckStatus != "5")
                        //{
                        //    List<TruckQueueDetail> truckQueueDetailList = idal.ITruckQueueDetailDAL.SelectBy(u => u.HeadId == HeadId && u.UnloadingArea==item.UnloadingArea && u.SeeFlag==1);
                        //}
                        List<TruckQueueDetail> truckQueueDetailList = idal.ITruckQueueDetailDAL.SelectBy(u => u.HeadId == HeadId && u.UnloadingArea == item.UnloadingArea && u.SeeFlag == 1);
                        item.BookOrigin = "EDI";
                        if (truckQueueDetailList.Count > 0)
                        {
                            item.SeeFlag = 1;
                            updateAMSData(truckQueueHead.Id, item.ReceiptId, 3, truckQueueHead.TruckNumber);
                        }

                        else item.SeeFlag = 0;
                        item.HeadId = HeadId;
                        item.NoticeFlag = 1;
                        AddTruckQueueDetail(item);
                        //if (truckQueueDetailList.Count == 0)
                        //{
                        //    AddTruckQueueDetail(item);
                        //}
                    }
                }
            }
            catch (Exception ex)
            {
                res = ex.Message.ToString();
            }
            return res;
        }
        public bool IsVehicleNumber(string vehicleNumber)
        {
            bool result = false;
            //string carnumRegex = @"([京津沪渝冀豫云辽黑湘皖鲁新苏浙赣鄂桂甘晋蒙陕吉闽贵粤青藏川宁琼使领A-Z]{1}[A-Z]{1}(([0-9]{5}[DF])|([DF]([A-HJ-NP-Z0-9])[0-9]{4})))|([京津沪渝冀豫云辽黑湘皖鲁新苏浙赣鄂桂甘晋蒙陕吉闽贵粤青藏川宁琼使领A-Z]{1}[A-Z]{1}[A-HJ-NP-Z0-9]{4}[A-HJ-NP-Z0-9学警港澳]{1})";
            string carnumRegex = "^([京津沪渝冀豫云辽黑湘皖鲁新苏浙赣鄂桂甘晋蒙陕吉闽贵粤青藏川宁琼使领A-Z]{1}[a-zA-Z](([DF]((?![IO])[a-zA-Z0-9](?![IO]))[0-9]{4})|([0-9]{5}[DF]))|[京津沪渝冀豫云辽黑湘皖鲁新苏浙赣鄂桂甘晋蒙陕吉闽贵粤青藏川宁琼使领A-Z]{1}[A-Z]{1}[A-Z0-9]{4}[A-Z0-9挂学警港澳]{1})$";
            result = Regex.IsMatch(vehicleNumber, carnumRegex);
            return result;
        }
        public string isTruckDetailExist(List<TruckQueueDetailParam> detailList)
        {
            string res = "";
            try
            {
                if (detailList.Count > 0)
                {
                    foreach (var item in detailList)
                    {
                        List<TruckQueueDetail> truckQueueDetailList = idal.ITruckQueueDetailDAL.SelectBy(u => u.ReceiptId == item.ReceiptId && u.BkNumber == item.BkNumber);
                        if (truckQueueDetailList.Count > 0)
                        {
                            res += "收货批次号：" + item.ReceiptId + "预约单号：" + item.BkNumber + "已存在，不能重复推送！\r";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                res = ex.Message.ToString();
            }
            return res;
        }
        //车辆排队查询列表
        public List<TruckQueueListDetail> getTruckQueueList(QueueParam queueParam)
        {
            TruckQueueList truckQueueList = new TruckQueueList();
            var sql = from a in idal.ItruckQueueViewDAL.SelectAll()
                      where a.WhCode == queueParam.WhCode && a.UnloadingArea == queueParam.UnloadingArea
                      select new TruckQueueListDetail
                      {
                          Id = a.Id,
                          TruckNumber = a.TruckNumber,
                          Seq = (int)a.seq,
                          WhCode = a.WhCode,
                          UnloadingArea = a.UnloadingArea,
                          PhoneNumber = a.PhoneNumber,
                          TruckLength = a.TruckLength,
                          Description = a.Description,
                          ClientCode = a.ClientCode,
                          GreenPassFlag = a.GreenPassFlag,
                          CBM = a.CBM
                      };
            int pageSize = 10;
            int startIndex = (queueParam.pageNumber - 1) * pageSize; // 计算开始位置
            var data = sql.ToList();
            var truckQueueListDetail = data.Skip(startIndex).Take(pageSize).ToList();
            return truckQueueListDetail;
        }


        //车辆排队查询列表
        public List<TruckQueueListDetail> getTruckQueueListTruck(QueueParam queueParam)
        {
            TruckQueueList truckQueueList = new TruckQueueList();
            var sql = from a in idal.ITruckQueueHeadDAL.SelectAll()
                      join b in idal.ITruckQueueDetailDAL.SelectAll()
                      on new { A = a.Id } equals new { A = (Int32)b.HeadId }
                      join c in idal.ILookUpDAL.SelectAll()
                       on new { A = a.TruckStatus } equals new { A = c.ColumnKey }
                      where a.WhCode == queueParam.WhCode
                      && c.TableName == "TruckQueueHead" && c.ColumnName == "TruckStatus"
                      && (a.TruckStatus == "2" || a.TruckStatus == "4")
                      && b.UnloadingArea == queueParam.UnloadingArea
                      group new { a, b, c } by new
                      {
                          a.Id,
                          a.WhCode,
                          b.UnloadingArea,
                          a.TruckNumber,
                          a.PhoneNumber,
                          c.Description,
                          b.ClientCode
                      } into g
                      select new TruckQueueListDetail
                      {
                          Id = g.Key.Id,
                          WhCode = g.Key.WhCode,
                          UnloadingArea = g.Key.UnloadingArea,
                          TruckNumber = g.Key.TruckNumber,
                          PhoneNumber = g.Key.PhoneNumber,
                          Description = g.Key.Description,
                          ClientCode = g.Key.ClientCode,
                          CBM = g.Sum(x => x.b.CBM)
                      };

            int pageSize = 10;
            int startIndex = (queueParam.pageNumber - 1) * pageSize; // 计算开始位置
            var data = sql.ToList();
            var truckQueueListDetail = data.Skip(startIndex).Take(pageSize).ToList();
            return truckQueueListDetail;
        }
        //超时锁定车辆解锁
        public string unlockTruck(QueueParam queueParam)
        {
            string res = "";
            try
            {
                int id = Convert.ToInt32(queueParam.Id);
                TruckQueueHead truckQueueHead = idal.ITruckQueueHeadDAL.SelectBy(u => u.Id == id).First();
                List<TruckQueueDetail> truckQueueDetailList = idal.ITruckQueueDetailDAL.SelectBy(u => u.HeadId == id);
                TruckQueueDetail truckQueueDetail = truckQueueDetailList[0];
                int resetCount = 1;
                if (truckQueueHead.ResetCount > 0) { resetCount = (int)truckQueueHead.ResetCount + 1; }
                //更新收货登记表时间
                foreach (var item in truckQueueDetailList)
                {
                    List<ReceiptRegister> ReceiptRegisterList = idal.IReceiptRegisterDAL.SelectBy(u => u.ReceiptId == item.ReceiptId && u.WhCode == queueParam.WhCode).ToList();
                    if (ReceiptRegisterList.Count > 0)
                    {
                        ReceiptRegister receiptRegister = idal.IReceiptRegisterDAL.SelectBy(u => u.ReceiptId == item.ReceiptId && u.WhCode == queueParam.WhCode).First();
                        receiptRegister.RegisterDate = DateTime.Now;
                        receiptRegister.UpdateDate = DateTime.Now;
                        receiptRegister.UpdateUser = queueParam.UserName;
                        receiptRegister.ResetCount = resetCount;
                        //createTranLog(queueParam, truckQueueHead, item, "1009");
                        //idal.IReceiptRegisterDAL.UpdateBy(receiptRegister, u => u.ReceiptId == item.ReceiptId && u.WhCode == queueParam.WhCode,new string[] { "RegisterDate", "UpdateDate", "UpdateUser", "ResetCount"});
                    }
                }
                //先插log再更新可以保留前一个登记时间
                createTranLog(queueParam, truckQueueHead, truckQueueDetail, "1008");
                //更新表头信息车辆状态变为待入库
                truckQueueHead.TruckStatus = "1";
                truckQueueHead.UpdateUser = queueParam.UserName;
                truckQueueHead.UpdateDate = DateTime.Now;
                truckQueueHead.ResetCount = resetCount;
                idal.ITruckQueueHeadDAL.UpdateBy(truckQueueHead, u => u.Id == id, new string[] { "TruckStatus", "UpdateUser", "UpdateDate", "ResetCount" });
                //更新该车辆明细SeeFlag以及登记时间
                truckQueueDetail.RegisterDate = DateTime.Now;
                truckQueueDetail.SeeFlag = 0;
                truckQueueDetail.UpdateUser = queueParam.UserName;
                truckQueueDetail.UpdateDate = DateTime.Now;
                truckQueueDetail.ResetCount = resetCount;
                idal.ITruckQueueDetailDAL.UpdateBy(truckQueueDetail, u => u.HeadId == id, new string[] { "SeeFlag", "UpdateUser", "UpdateDate", "RegisterDate", "ResetCount" });
                idal.SaveChanges();
                res = "解锁成功！";
            }
            catch (Exception ex)
            {
                res = ex.Message.ToString();
            }
            return res;
        }
        //车辆入场抬杆
        public string getInGate(ReceiptParam receiptParam)
        {
            string str = "";
            string res = "";
            TruckQueueHead truckQueueHead = idal.ITruckQueueHeadDAL.SelectBy(u => u.TruckNumber == receiptParam.TruckNumber && u.WhCode == receiptParam.WhCode && u.TruckStatus == "2").First();
            truckQueueHead.TruckStatus = "4";
            truckQueueHead.UpdateDate = DateTime.Now;
            truckQueueHead.EntryTime = DateTime.Now;
            idal.ITruckQueueHeadDAL.UpdateBy(truckQueueHead, u => u.Id == truckQueueHead.Id, new string[] { "TruckStatus", "UpdateDate", "EntryTime" });
            idal.SaveChanges();
            List<TruckQueueDetail> truckQueueDetailList = idal.ITruckQueueDetailDAL.SelectBy(u => u.HeadId == truckQueueHead.Id).ToList();
            //BMS接口头
            TruckGateIn truckGateIn = new TruckGateIn();
            truckGateIn.method = "GMS";
            truckGateIn.truckNumber = truckQueueHead.TruckNumber;
            truckGateIn.warehouseCode = truckQueueHead.WhCode;
            truckGateIn.operatorUser = truckQueueHead.AllowUser;
            //BMS接口明细
            foreach (var item in truckQueueDetailList)
            {
                TruckGateInDetail truckGateInDetail = new TruckGateInDetail();
                truckGateInDetail.client = item.ClientCode;
                truckGateInDetail.receiptId = item.ReceiptId;
                if (item.ResetCount > 0)
                {
                    truckGateInDetail.ResetCount = (int)item.ResetCount;
                }
                else
                {
                    truckGateInDetail.ResetCount = 0;
                }
                truckGateInDetail.GreenPassFlag = truckQueueHead.GreenPassFlag.ToString();
                if (truckGateIn.Details == null)
                    truckGateIn.Details = new List<TruckGateInDetail>();
                truckGateIn.Details.Add(truckGateInDetail);
            }
            var JsonData = JsonConvert.SerializeObject(truckGateIn);
            string strUrl = CDMReqUrl + "ebilling/updateBillInfo";//抬杆后调用费用接口
            string response = CreatePostHttpResponse(strUrl, "POST", JsonData);
            //正常入库更新AMS数据
            str = updateAMSData(truckQueueHead.Id, "", 0, receiptParam.TruckNumber);
            //更新CDM信息,只有预约数据可更新
            if (truckQueueHead.BookOrigin == "EDI")
            {
                res = updateCDMData(truckQueueHead, receiptParam.TruckNumber, 1);
                if (res != "CDM更新成功！")
                {
                    str = str + res;
                }
            }
            return str;
        }
        //入库或者放车后更新AMS状态
        public string updateAMSData(int Id, string UnloadingArea, int Flag, string truckNumber)
        {
            string res = "";
            //AMS接口头信息数据
            Dictionary<string, object> dicData = new Dictionary<string, object>();
            dicData.Add("truckStatus", "");
            dicData.Add("bkorders", "");
            dicData.Add("truckNumber", truckNumber);
            dicData.Add("receiptFlag", "U");
            dicData.Add("invoiceFlag", "N");
            ArrayList detailList = new ArrayList();
            List<TruckQueueDetail> truckQueueDetailList = null;
            string status = "55";
            if (Flag == 0)//正常抬杆入库车辆
            {
                truckQueueDetailList = idal.ITruckQueueDetailDAL.SelectBy(u => u.HeadId == Id && u.SeeFlag == 1).ToList();
            }
            else if (Flag == 1) //已入库车辆其他库区放车
            {
                truckQueueDetailList = idal.ITruckQueueDetailDAL.SelectBy(u => u.HeadId == Id && u.UnloadingArea == UnloadingArea).ToList();
            }
            else if (Flag == 2)//离库更新状态
            {
                truckQueueDetailList = idal.ITruckQueueDetailDAL.SelectBy(u => u.HeadId == Id).ToList();
                status = "100";
            }
            else if (Flag == 3) //新增登记明细更新状态
            {
                status = "55";
            }
            if (Flag != 3)
            {
                foreach (var item in truckQueueDetailList)
                {
                    Dictionary<string, object> detail = new Dictionary<string, object>();
                    detail.Add("receiptId", item.ReceiptId);
                    detail.Add("receiptStatus", status);
                    detail.Add("createUser", "SYSTEM");
                    detailList.Add(detail);
                }
            }
            else
            {
                Dictionary<string, object> detail = new Dictionary<string, object>();
                detail.Add("receiptId", UnloadingArea);
                detail.Add("receiptStatus", status);
                detail.Add("createUser", "SYSTEM");
                detailList.Add(detail);
            }

            dicData.Add("bkReceipts", detailList);
            ArrayList arrayList = new ArrayList();
            arrayList.Add(dicData);
            string json = JsonConvert.SerializeObject(arrayList);
            string strUrl = uniReqUrl + "bookOrderList/updateBkTruckInfo";
            //string strUrl = "http://localhost:3103/bookOrderList/updateBkTruckInfo";
            string response = CreatePostHttpResponse(strUrl, "POST", json);
            Dictionary<string, object> resultList = JsonConvert.DeserializeObject<Dictionary<string, object>>(response.ToString());
            int code = int.Parse(resultList["code"].ToString());
            if (code == 200)
            {
                res = "入库成功！";
            }
            else
            {
                res = "更新AMS失败！";
            }
            return res;
        }
        public string updateCDMData(TruckQueueHead truckQueueHead, string truckNumber, int Flag)
        {
            string res = "";
            //AMS接口头信息数据
            Dictionary<string, object> dicData = new Dictionary<string, object>();
            dicData.Add("UpdateTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            dicData.Add("flag", Flag.ToString());
            dicData.Add("truckNumber", truckNumber);
            dicData.Add("AllowTime", truckQueueHead.AllowTime);
            dicData.Add("AllowUser", truckQueueHead.AllowUser);
            ArrayList detailList = new ArrayList();
            List<string> truckQueueDetailList = null;
            //truckQueueDetailList = idal.ITruckQueueDetailDAL.SelectBy(u => u.HeadId == Id).ToList();
            truckQueueDetailList = (from a in idal.ITruckQueueDetailDAL.SelectAll()
                                    where a.HeadId == truckQueueHead.Id
                                    select a.BkNumber).Distinct().ToList();

            foreach (var item in truckQueueDetailList)
            {
                Dictionary<string, object> detail = new Dictionary<string, object>();
                detail.Add("bkorders", item);
                detailList.Add(detail);
            }
            dicData.Add("bkordersList", detailList);
            string json = JsonConvert.SerializeObject(dicData);
            string strUrl = CDMReqUrl + "bondedarea/HandleTrcukStatus";
            string response = CreatePostHttpResponse(strUrl, "POST", json);
            int code = 0;
            string message = "";
            try
            {
                Dictionary<string, object> resultList = JsonConvert.DeserializeObject<Dictionary<string, object>>(response.ToString());
                code = int.Parse(resultList["code"].ToString());
                message = resultList["message"].ToString();

            }
            catch (Exception ex)
            {
                message = "请求更新接口出现异常！";
            }

            if (code == 20000)
            {
                res = "CDM更新成功！";
            }
            else
            {
                res = "CDM " + message;
            }
            return res;
        }
        //车辆离库抬杆
        public string leaveGate(ReceiptParam receiptParam)
        {
            string str = "离库成功！";
            TruckQueueHead truckQueueHead = idal.ITruckQueueHeadDAL.SelectBy(u => u.TruckNumber == receiptParam.TruckNumber && u.WhCode == receiptParam.WhCode && u.TruckStatus == "4").First();
            //首先判断该车BMS是否都已完成缴费
            string res = "";
            res = checkBMSStatus(receiptParam);
            res = "Y";
            if (res == "Y") //收费完成可离库
            {
                truckQueueHead.TruckStatus = "5";
                truckQueueHead.UpdateDate = DateTime.Now;
                truckQueueHead.DepartureTime = DateTime.Now;
                idal.ITruckQueueHeadDAL.UpdateBy(truckQueueHead, u => u.Id == truckQueueHead.Id, new string[] { "TruckStatus", "UpdateDate", "DepartureTime" });
                idal.SaveChanges();
                if (truckQueueHead.BookOrigin == "EDI")
                {
                    //更新CDM信息
                    res = updateCDMData(truckQueueHead, receiptParam.TruckNumber, 2);
                    if (res != "CDM更新成功！")
                    {
                        str = str + res;
                    }
                }
                //更新AMS信息状态
                updateAMSData(truckQueueHead.Id, "", 2, receiptParam.TruckNumber);
            }
            else //收费未完成不可离库，进行提醒
            {
                str = res;
            }
            return str;
        }
        public string checkBMSStatus(ReceiptParam receiptParam)
        {
            string res = "";
            Dictionary<string, object> dicData = new Dictionary<string, object>();
            dicData.Add("warehouseCode", receiptParam.WhCode);
            dicData.Add("truckNumber", receiptParam.TruckNumber);
            string json = JsonConvert.SerializeObject(dicData);
            string strUrl = CDMReqUrl + "ebilling/getTruckStatus";
            string response = CreatePostHttpResponse(strUrl, "POST", json);
            Dictionary<string, object> resultList = JsonConvert.DeserializeObject<Dictionary<string, object>>(response.ToString());
            int code = int.Parse(resultList["code"].ToString());
            string message = resultList["message"].ToString();
            if (code == 20000)
            {
                Dictionary<string, object> data = JsonConvert.DeserializeObject<Dictionary<string, object>>(resultList["data"].ToString());
                if (data["code"].ToString() == "100" && data["Status"].ToString() == "Y")
                {
                    res = "Y";
                }
                else if (data["code"].ToString() == "100" && data["Status"].ToString() == "N")
                {
                    res = "未完成支付";
                }
                else
                {
                    res = data["message"].ToString();
                }
            }
            else
            {
                res = message;
            }
            return res;
        }
        //小库区下拉列表
        public IEnumerable<WhZoneResult> GetSmallLoadAreaList(string whCode, string UnloadingArea)
        {
            List<Zone> ZoneList = idal.IZoneDAL.SelectBy(u => u.ZoneName == UnloadingArea).ToList();
            if (ZoneList.Count > 0)
            {
                Zone zoneData = ZoneList[0];
                var sql = from a in idal.IZoneDAL.SelectAll()
                          where a.WhCode == whCode && a.UpId == zoneData.Id
                          select new WhZoneResult
                          {
                              Id = a.Id,
                              RegFlag = a.RegFlag,
                              ZoneName = a.ZoneName
                          };
                return sql.AsEnumerable();
            }
            else
            {
                var sql = from a in idal.IZoneDAL.SelectAll()
                          where a.WhCode == whCode && a.Id == 0
                          select new WhZoneResult
                          {
                              Id = a.Id,
                              RegFlag = a.RegFlag,
                              ZoneName = a.ZoneName
                          };
                return sql.AsEnumerable();
            }
        }
        #region 调取外部接口
        public string CreatePostHttpResponse(string url, string method, string json = "")
        {
            string content = string.Empty;
            HttpWebRequest request = null;
            //如果是发送HTTPS请求  
            if (url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
            {
                // 设置安全协议
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                request = WebRequest.Create(url) as HttpWebRequest;
                request.ProtocolVersion = HttpVersion.Version10;
            }
            else
            {
                request = WebRequest.Create(url) as HttpWebRequest;
            }

            request.ProtocolVersion = HttpVersion.Version10;
            request.Method = "POST";
            request.ContentType = "application/json";
            request.UserAgent = "EIP";

            // 设置安全协议
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

            HttpWebResponse response;
            try
            {
                byte[] postData = Encoding.UTF8.GetBytes(json);
                request.ContentLength = postData.Length;
                using (Stream stream = request.GetRequestStream())
                {
                    stream.Write(postData, 0, postData.Length);
                }

                response = (HttpWebResponse)request.GetResponse();
                if (response != null && response.StatusCode == HttpStatusCode.OK)
                {
                    using (Stream stream = response.GetResponseStream())
                    {
                        using (StreamReader sr = new StreamReader(stream))
                        {
                            content = sr.ReadToEnd();
                        }
                    }
                }
                else
                {
                    content = "error$接口不通";
                }
                response.Close();
            }
            catch (Exception ex)
            {
                content = ex.Message;
            }
            return content;
        }

        #endregion
    }
}
<%@ WebHandler Language="C#" Class="upload" %>

using System;
using System.Web;
using System.Web.SessionState;
using System.IO;
using System.Net;
using System.Text;
using System.Diagnostics;
public class upload : IHttpHandler, IRequiresSessionState
{
    NET.EIP eip = new NET.EIP();
    string userName = "";
    HttpContext current;
    public void ProcessRequest(HttpContext context)
    {
        current = context;
        if (current.Session["engName"] != null) userName = current.Session["engName"].ToString().ToUpper();
        string actionType = current.Request["actionType"];
        switch (actionType)
        {
            //上传    
            case "upload":
                if(userName!="")
                    Upload();
                End();
                break;
            case "Download":
                if(userName!="")
                    Download();
                End();
                break;        
                
                

        }
    }

 

    /// <summary>
    /// 上传
    /// </summary>
    protected void Upload()
    {
        string fileId = current.Request["fileId"];
        string CallBackUrl = current.Request["CallBackUrl"];
        
        eip.OpenData(common.OE_edi);
        if (fileId + "" == ""&& CallBackUrl+""!="") {
            string sql = "insert into ED.dbo.[ElectronicDocument](FileHost,FilePath,CallBackUrl) values('10.88.88.161','ElectronicDocument','" + CallBackUrl + "')  select @@identity FileId";
            fileId = eip.Exec(sql).Tables[0].Rows[0][0].ToString();
        }
        HttpFileCollection files= HttpContext.Current.Request.Files;
     //   string urlPath = @"E:\pdf\"+ fileId;
      //  string filePathName = string.Empty;
     //   string localPath = Path.Combine(HttpRuntime.AppDomainAppPath, urlPath);
       // string ex = Path.GetExtension(files[0].FileName);

        string destHost = "10.88.88.161";
        string destinationFile = "ElectronicDocument";
        string urlPath = "\\\\" + destHost + "\\" + destinationFile + "\\" + fileId;
        string filePathName = string.Empty;
        string localPath = Path.Combine(HttpRuntime.AppDomainAppPath, urlPath);

        if (Ping(destHost))
        {

            HostPW hp = getHostPW(destHost);
            if (Connect(destHost, hp.ud, hp.pw))
            {
                // filePathName = Guid.NewGuid().ToString("N") + ex;
                if (!Directory.Exists(urlPath))
                {
                    Directory.CreateDirectory(urlPath);
                }
                // file.SaveAs(Path.Combine(localPath, file.FileName));
                files[0].SaveAs(Path.Combine(localPath, files[0].FileName));

                if (CallBackUrl + "" != "")
                {
                    WebClient client = new WebClient();
                    client.Proxy = null;
                    client.Encoding = Encoding.UTF8;
                    string responseTest = client.DownloadString(CallBackUrl + "&fileId=" + fileId);
                    if (responseTest != "")
                    {
                        string sqlUpdate = " update ED.dbo.ElectronicDocument set CallBackResult='" + responseTest + "',UpdateDate=getdate()  where Id=" + fileId;
                        eip.Exec(sqlUpdate);
                    }
                }
            }
            else
                throw new System.IO.FileNotFoundException("打开文件服务器失败！");
        }else
            throw new System.IO.FileNotFoundException("连接文件服务器失败！");
             
    }
            public class HostPW
    {
        public string host { get; set; }
        public string ud { get; set; }
        public string pw { get; set; }
 
    }
            public HostPW getHostPW(string host) {

            HostPW hp = new HostPW();
            hp.host = host;
            switch (host)
            {

                case "10.88.88.90":
                    hp.ud = "administrator";
                    hp.pw = "1qaz2wsx,";
           
                    break;

                case "10.88.88.15":
                    hp.ud = "eip";
                    hp.pw = "1qaz2wsx,";
                    break;
                case "10.88.88.161":
                    hp.ud = "administrator";
                    hp.pw = "password1()";
                    break;
            }
            return hp;
        }
    public bool Ping(string remoteHost)
    {
        bool Flag = false;
        Process proc = new Process();
        try
        {
            proc.StartInfo.FileName = "cmd.exe";
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardInput = true;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.RedirectStandardError = true;
            proc.StartInfo.CreateNoWindow = true;
            proc.Start();
            string dosLine = @"ping -n 1 " + remoteHost;
            proc.StandardInput.WriteLine(dosLine);
            proc.StandardInput.WriteLine("exit");
            while (proc.HasExited == false)
            {
                proc.WaitForExit(500);
            }
            string pingResult = proc.StandardOutput.ReadToEnd();
            if (pingResult.IndexOf("(0% 丢失)") != -1)
            {
                Flag = true;
            }
            proc.StandardOutput.Close();
        }
        catch (Exception ex)
        {
        }
        finally
        {
            try
            {
                proc.Close();
                proc.Dispose();
            }
            catch
            {
            }
        }
        return Flag;
    }
    public bool Connect(string remoteHost, string userName, string passWord)
    {
        if (!Ping(remoteHost))
        {
            return false;
        }
        bool Flag = true;
        System.Diagnostics.Process proc = new System.Diagnostics.Process();
        try
        {
            proc.StartInfo.FileName = "cmd.exe";
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardInput = true;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.RedirectStandardError = true;
            proc.StartInfo.CreateNoWindow = true;
            proc.Start();
            string dosLine = @"net use \\" + remoteHost + " " + passWord + " " + " /user:" + userName + ">NUL";
            //  proc.StandardInput.WriteLine("net use * / del / y");
            proc.StandardInput.WriteLine(dosLine);
            proc.StandardInput.WriteLine("exit");
            while (proc.HasExited == false)
            {
                proc.WaitForExit(1000);
            }
            string errormsg = proc.StandardError.ReadToEnd();
            if (errormsg != "")
            {
                Flag = false;
            }
            proc.StandardError.Close();
        }
        catch (Exception ex)
        {
            Flag = false;
        }
        finally
        {
            try
            {
                proc.Close();
                proc.Dispose();
            }
            catch
            {
            }
        }
        return Flag;
    }
    /// <summary>
    /// 删除
    /// </summary>
    protected void Delete() {
        string id = current.Request["id"];                                                      //公共上传 Attch_Files 中 Id        
        string sql = "Delete Attch_Files Where Id = " + eip.SqlStr(id);
        eip.Exec(sql);
        current.Response.Write("删除成功!");
    }

    /// <summary>
    /// 下载
    /// </summary>
    protected void Download() {
        string fileId = current.Request["fileId"];
        string fileName =current.Request["fileName"];
      // current.Response.Write("http://10.88.88.161/FilesDownLoad/DownLoadZip??fileId=" + fileId + "&fileName=" + fileName);
        current.Response.Redirect("http://10.88.88.161/FilesDownLoad/DownLoadZip?fileId=" + fileId + "&fileName=" + fileName, false); 
    }

   

    public bool IsReusable {
        get {
            return false;
        }
    }

    void End()
    {
        eip.CloseData();
        eip = null;
        current.Response.End();
    }
}
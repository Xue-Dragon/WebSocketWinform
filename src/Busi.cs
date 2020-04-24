using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;
using System.Collections;
using FTFrame;
using FTFrame.Tool;
using FTFrame.DBClient;
using System.Drawing;
using DataSocket.Obj;
namespace DataSocket
{
    public class Busi
    {
        public static bool DataRuning = false;
        public static List<SetObj> SetList = new List<SetObj>();
        public static Socket ServerSocket = null;
        public static Socket ComSocket = null;
        public static Thread ListenThread = null;
        public static Thread ReceiveThread = null;
        public static AutoResetEvent ResetEvent =null;
        private static void SaveData(string HexString)
        {
            try
            {
                using(DB db=new DB())
                {
                    db.Open(Const.MySql_ConnStr);
                    if(HexString.StartsWith("2A"))//心跳包
                    {
                        Tool.Debug(HexString);
                        string _hexstr = ConvertTools.HexStringToString(HexString, Encoding.Default, 2);
                        Tool.Debug(_hexstr);
                        string[] item = _hexstr.Split(new string[] { ","},StringSplitOptions.RemoveEmptyEntries);
                        //*HQ,2194203104,V6,062848,V,3118.9668,N,12136.0744,E,0.00,0.00,230320,FFFFFBFF,460,00,6304,19500,898607B3151980910028,#
                        //*HQ,2194203104,V1,184553,V,3118.9668,N,12136.0744,E,0.00,0.00,220320,FFFFFBFF,460,00,6304,19500,12.80,31,0,4.18#
                        //头厂家,序号，      命令,时间,  A有效V无效,北纬NS，东经EW,速度,方向, 日期DMY
                        if(item[2]=="V1" || item[2] == "V6")
                        {
                            if(item[4]=="A")//有效定位
                            {
                                string _time = item[3];
                                string[] _wei = item[5].Split('.');
                                string[] _jing = item[7].Split('.');
                                string _date = item[11];
                                decimal sudu = decimal.Parse(item[9])* 1.852m;
                                decimal fangxiang = decimal.Parse(item[10]);
                                string datetime = "20" + _date.Substring(4, 2) + "-" + _date.Substring(2, 2) + "-" + _date.Substring(0, 2) + " " + _time.Substring(0, 2) + ":" + _time.Substring(2, 2) + ":" + _time.Substring(4, 2);
                                string gpsId = item[1];
                                decimal weiLeft = decimal.Parse(_wei[0].Substring(0, _wei[0].Length - 2));
                                decimal weiRight = decimal.Parse(item[5].Substring(_wei[0].Length - 2))/60m;
                                decimal jingLeft = decimal.Parse(_jing[0].Substring(0, _jing[0].Length - 2));
                                decimal jingRight = decimal.Parse(item[7].Substring(_jing[0].Length - 2)) / 60m;
                                SetObj so=CarObj(gpsId);
                                if(so != null)
                                {
                                    string sql = "insert into tb_gps(carFid,gpsId,gpstime,jingdu,weidu,sudu,fangxiang,addtime)";
                                    sql += "values(@carFid,@gpsId,@gpstime,@jingdu,@weidu,@sudu,@fangxiang,@addtime)";
                                    db.execSql(sql,new PR[] {
                                        new PR("@carFid",so.CarID),
                                        new PR("@gpsId",gpsId),
                                        new PR("@gpstime",DateTime.Parse(datetime).AddHours(8)),
                                        new PR("@jingdu",jingLeft+jingRight),
                                        new PR("@weidu",weiLeft+weiRight),
                                        new PR("@sudu",sudu),
                                        new PR("@fangxiang",fangxiang),
                                        new PR("@addtime",DateTime.Now)
                                    });
                                    Tool.ShowStat("【心跳】"+so.CarNumber+"  北纬："+(weiLeft + weiRight).ToString("0.000000") +"   东经："+(jingLeft + jingRight).ToString("0.000000") + "   速度：" + (sudu).ToString("0.00")+"   方向：" + (fangxiang).ToString("0.00"));
                                }
                            }
                        }
                    }
                    else if (HexString.StartsWith("24"))
                    {
                        //24  21  94  20  33  15  01  25  08  17  03  20  22  36  11  66  00  11  34  99  19  0C  00  00  00  FF  FF  FF  FF  00  CC  18  09  0D  00  00  00  01  CC  00  18  A0  4C  2C  CA
                        Tool.Debug(HexString);
                        string _jing = HexString.Substring(17 * 2, 5 * 2);
                        string binStr = ConvertTools.HexString2BinString(_jing.Substring(9, 1));
                        bool IsAvailable = (binStr.Substring(2, 1) == "1");
                        if (IsAvailable)
                        {
                            string gpsId = HexString.Substring(1 * 2, 5 * 2);
                            string _time = HexString.Substring(6 * 2, 3 * 2);
                            string _date = HexString.Substring(9 * 2, 3 * 2);
                            string datetime = "20" + _date.Substring(4, 2) + "-" + _date.Substring(2, 2) + "-" + _date.Substring(0, 2) + " " + _time.Substring(0, 2) + ":" + _time.Substring(2, 2) + ":" + _time.Substring(4, 2);
                            string _wei = HexString.Substring(12 * 2, 4 * 2);
                            decimal sudu = decimal.Parse(HexString.Substring(22 * 2, 3)) * 1.852m;
                            decimal fangxiang = decimal.Parse(HexString.Substring(23 * 2 + 1, 3));
                            decimal weiLeft = decimal.Parse(_wei.Substring(0, 2));
                            decimal weiRight = decimal.Parse(_wei.Substring(2, 2) + "." + _wei.Substring(4, 4)) / 60m;
                            decimal jingLeft = decimal.Parse(_jing.Substring(0, 3));
                            decimal jingRight = decimal.Parse(_jing.Substring(3, 2) + "." + _jing.Substring(5, 4)) / 60m;
                            SetObj so = CarObj(gpsId);
                            if (so != null)
                            {
                                string sql = "insert into tb_gps(carFid,gpsId,gpstime,jingdu,weidu,sudu,fangxiang,addtime)";
                                sql += "values(@carFid,@gpsId,@gpstime,@jingdu,@weidu,@sudu,@fangxiang,@addtime)";
                                db.execSql(sql, new PR[] {
                                        new PR("@carFid",so.CarID),
                                        new PR("@gpsId",gpsId),
                                        new PR("@gpstime",DateTime.Parse(datetime).AddHours(8)),
                                        new PR("@jingdu",jingLeft+jingRight),
                                        new PR("@weidu",weiLeft+weiRight),
                                        new PR("@sudu",sudu),
                                        new PR("@fangxiang",fangxiang),
                                        new PR("@addtime",DateTime.Now)
                                    });
                                Tool.ShowStat("【主送】" + so.CarNumber + "  北纬：" + (weiLeft + weiRight).ToString("0.000000") + "   东经：" + (jingLeft + jingRight).ToString("0.000000") + "   速度：" + (sudu).ToString("0.00") + "   方向：" + (fangxiang).ToString("0.00"));
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                Tool.ShowStat(ex.Message);
                Tool.ShowStat(ex.StackTrace.ToString());
                Tool.ShowStat(ex.Source.ToString());
            }
        }
        public static void SetInit()
        {
            SetList.Clear();
            using (DB db = new DB())
            {
                db.Open(Const.MySql_ConnStr);
                string sql = "select fid,gps_type,gps_id,car_number from tb_car_info where stat=1 and (gps_type=1 or gps_type=2)";
                using (DR dr = db.OpenRecord(sql))
                {
                    while (dr.Read())
                    {
                        SetList.Add(new SetObj()
                        {
                            CarID = dr.GetString(0),
                            GPSType = dr.GetInt16(1),
                            GPSID = dr.GetString(2),
                            CarNumber = dr.GetString(3)
                        });
                    }
                }
            }
            ResetEvent = new AutoResetEvent(false);
        }
        private static SetObj CarObj(string GpsId)
        {
            foreach(SetObj so in SetList)
            {
                if (so.GPSID == GpsId) return so;
            }
            return null;
        }
        public static void JobCloseAll()
        {
            try
            {
                if (ServerSocket != null) ServerSocket.Close();
            }
            catch (Exception ex)
            {
                Tool.ShowStat(ex.Message);
            }
            try
            {
                if (ComSocket != null) ComSocket.Close();
            }
            catch (Exception ex)
            {
                Tool.ShowStat(ex.Message);
            }
            try
            {
                if (ListenThread != null) ListenThread.Abort();
            }
            catch (Exception ex)
            {
                Tool.ShowStat(ex.Message);
            }
            try
            {
                if(ReceiveThread!=null) ReceiveThread.Abort();
            }
            catch (Exception ex)
            {
                Tool.ShowStat(ex.Message);
            }
        }
        public static void JobStartAll()
        {
            JobCloseAll();
            IPAddress ip = IPAddress.Parse(Const.ListenIP);
            ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            ListenThread = null;
            ComSocket = null;
            ReceiveThread = null;
            try
            {
                //绑定ip和端口
                ServerSocket.Bind(new IPEndPoint(ip, Const.ListenPort));
                //设置最多20个排队连接请求
                ServerSocket.Listen(20);
                Tool.ShowStat(((IPEndPoint)ServerSocket.LocalEndPoint).ToString());
                ListenThread = new Thread(new ThreadStart(delegate () {
                    while (ListenThread.ThreadState != ThreadState.Aborted && true)
                    {
                        //if (serverSocket.Available == 0) continue;
                        //监听到客户端的连接，获取双方通信socket
                        ComSocket = ServerSocket.Accept();
                        Tool.ShowStat(((IPEndPoint)ComSocket.LocalEndPoint).ToString());
                        ReceiveThread= new Thread(Receive);
                        ReceiveThread.Start(ComSocket);
                    }
                }));
                ListenThread.Start();
            }
            catch (Exception ex)
            {
                Tool.ShowStat("[JobStartAll]" + ex.Message);
            }
        }
        public static void SocketRestart()
        {
            Tool.ShowStat("[SocketRestart]");
            JobCloseAll();
            Busi.SetInit();
            Busi.JobStartAll();
        }
        private static void Receive(object socket)
        {
            try
            {
                Socket myClientSocket = (Socket)socket;
                while (true)
                {
                    byte[] buff = new byte[Const.BufferSize];
                    int r = myClientSocket.Receive(buff);
                    string str = ConvertTools.BytesToHexString(buff.Take(r).ToArray());
                    //ResetEvent.Set();
                    //Tool.Debug(str);
                    SaveData(str);
                }
            }
            catch (Exception ex)
            {
                Tool.ShowStat("[Receive]" + ex.Message);
                SocketRestart();
            }
        }
    }
}

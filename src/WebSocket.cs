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
using Fleck;
using Newtonsoft.Json;

namespace DataSocket
{
    public class WebSocket
    {
        public static List<IWebSocketConnection> AllSockets = new List<IWebSocketConnection>();
        public static WebSocketServer WSServer = null;
        public static void Start()
        {
            End();
            AllSockets.Clear();
            WSServer = new WebSocketServer("ws://0.0.0.0:1002");
            //WSServer.Certificate= new System.Security.Cryptography.X509Certificates.X509Certificate2(@"D:\tools\certificate\3065706_e.intellqc.com.pfx", "JezhAqvL");
            WSServer.Start(socket =>
            {
                socket.OnOpen = () =>
                {
                    //Console.WriteLine("Open!");
                    AllSockets.Add(socket);
                };
                socket.OnClose = () =>
                {
                    //Console.WriteLine("Close!");
                    AllSockets.Remove(socket);
                };
                socket.OnMessage = message =>
                {
                    PostBack(socket, message);
                    //Console.WriteLine(message);
                    //AllSockets.ToList().ForEach(s => s.Send("Echo: " + message));
                };
            });
        }
        public static void PostBack(IWebSocketConnection socket,string message)
        {
            //qc|car|6e09f918_d4ef_4f27_8ed3_ab4800f5f488     //取前最近36条数据
            //qc|car|6e09f918_d4ef_4f27_8ed3_ab4800f5f488|startTime  多个car用,隔开（多个car时同时若存在startTime，startTime也要用,隔开）  //取大于startTime的数据，不超过36条
            //返回为Json字符串 格式为
            /*
            [
            {"carId":"abc1","data":[
                    {"time":"2020-12-12 12:12:14","jingDu":123,"weiDu":123,"suDu":123,"fangXiang":123},
            {"time":"2020-12-12 12:12:13","jingDu":123,"weiDu":123,"suDu":123,"fangXiang":123},
            {"time":"2020-12-12 12:12:12","jingDu":123,"weiDu":123,"suDu":123,"fangXiang":123}
                                ]     },
            {"carId":"abc2","data":[
                                {"time":"2020-12-12 12:12:14","jingDu":123,"weiDu":123,"suDu":123,"fangXiang":123},
            {"time":"2020-12-12 12:12:13","jingDu":123,"weiDu":123,"suDu":123,"fangXiang":123},
            {"time":"2020-12-12 12:12:12","jingDu":123,"weiDu":123,"suDu":123,"fangXiang":123}
                                ]     }
            ]
             */
            if (message.StartsWith("qc|bigScreenAlert"))
            {
                socket.Send(Redis.Read("bigScreenAlert"));
            }
            if (message.StartsWith("qc|alert|"))
            {
                string[] item = message.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                string userId = item[2].Trim();
                socket.Send(Redis.Read("alert:" + userId));
            }
            if (message.StartsWith("qc|car|"))
            {
                string[] item = message.Split(new string[] { "|"},StringSplitOptions.RemoveEmptyEntries);
                string[] startTimes = null;
                if(item.Length>3)
                {
                    string _startTimes = item[3].Trim();
                    startTimes = _startTimes.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                }
                string carIDs = item[2].Trim();
                string[] cars = carIDs.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                string[] sqls = new string[cars.Length];
                for(int i=0;i<cars.Length;i++)
                {
                    if(startTimes==null)
                    {
                        sqls[i]= "select * from tb_gps where carFid='" + str.D2DD(cars[i]) + "' order by addtime desc  limit 36";
                    }
                    else
                    {
                        sqls[i] = "select * from tb_gps where carFid='" + str.D2DD(cars[i]) + "' and addtime>'" + str.D2DD(startTimes[i]) + "' order by addtime desc limit 36";
                    }
                }
                StringBuilder sb = new StringBuilder(100);
                sb.Append("[");
                using (DB db = new DB())
                {
                    db.Open(Const.MySql_ConnStr);
                    for (int i = 0; i < cars.Length; i++)
                    {
                        string sql = sqls[i];
                        if (i > 0) sb.Append(",");
                        sb.Append("{\"carId\":\""+ cars[i]+ "\",\"data\":[");
                        bool hasdata = false;
                        using (DR dr = db.OpenRecord(sql))
                        {
                            while (dr.Read())
                            {
                                if (hasdata) sb.Append(",");
                                GPSPoint gp = GPSChange.WGS84_to_GCJ02(Convert.ToDouble(dr.GetDecimal("weidu")), Convert.ToDouble(dr.GetDecimal("jingdu")));
                                sb.Append("{\"time\":\""+ FTFrame.Tool.str.GetDateTime(dr.GetDateTime("addtime")) + "\",\"jingDu\":" + gp.GetLng() + ",\"weiDu\":" + gp.GetLat() + ",\"suDu\":" + dr.GetDecimal("sudu") + ",\"fangXiang\":" + dr.GetDecimal("fangxiang") + "}");
                                hasdata = true;
                            }
                        }
                        sb.Append("]}");
                    }
                }
                sb.Append("]");
                socket.Send(sb.ToString());
            }
        }
        public static void Send(string sendVal)
        {
            foreach (var socket in AllSockets)
            {
                socket.Send(sendVal);
            }
        }
        public static void End()
        {
            if (AllSockets != null)
            {
                foreach (var socket in AllSockets)
                {
                    try
                    {
                        socket.Close();
                    }
                    catch (Exception ex)
                    {
                        Tool.ShowStat(ex.Message);
                    }
                }
            }

            if (WSServer != null)
            {
                try
                {
                    WSServer.Dispose();
                }
                catch (Exception ex)
                {
                    Tool.ShowStat(ex.Message);
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace DataSocket
{
    public class Func
    {
        private const string BaoTou = "00 00 00 06";
        private const string CongZhan = "01";
        private const string GongNengMa = "03";
        public static string YanZheng = "Not Set";
        public static List<float> ReadFloat(Socket socket,int startAddress,int readCount)
        {
            try
            {
                List<float> list = new List<float>();
                YanZheng = new Random().Next(0, 16 * 16 * 16 * 15).ToString("x4");
                string SendText = YanZheng + BaoTou + CongZhan + GongNengMa + startAddress.ToString("x4") + (readCount*2).ToString("x4");
                SendText = SendText.Replace(" ", "");
                socket.Send(ConvertTools.StringToHexByte(SendText));
                bool re = Const._autoResetEvent.WaitOne(3000);
                if (re)
                {
                    string str = Const.SocketReceiveHex;
                    string data = str.Substring(8*2);
                    string lendata = data.Substring(0, 2);
                    int len= Convert.ToInt32(lendata, 16);
                    data = data.Substring(2);
                    for(int i=1;i<= readCount;i++)
                    {
                        if (data.Length >= 8 * i)
                        {
                            string curdata = data.Substring(8 * (i - 1), 8);
                            list.Add(ConvertTools.ValueConvertToFloat(ConvertTools.StringToHexByte(curdata)));
                        }
                    }
                    return list;
                }
                else
                {
                    Tool.ShowStat("超过3秒未得到回复");
                    return null;
                }
            }
            catch(Exception ex)
            {
                Tool.ShowStat(ex.Message);
                return null;
            }
        }
    }
}

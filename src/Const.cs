using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace DataSocket
{
    public class Const
    {
        public static AutoResetEvent _autoResetEvent = new AutoResetEvent(false);
        public static string SocketReceiveHex = null;
        public static MainForm Main  = null;
        public static int BufferSize = 999;
        public static int ListenPort = 1001;
        public static string ListenIP = ConfigurationManager.AppSettings["ListenIP"];
        public static string MySql_ConnStr = ConfigurationManager.AppSettings["MySql_ConnStr"];
        public static bool IsDebug = false;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DataSocket
{
   public  class Tool
    {
         static int CurLogHeight=0;
        public static void ShowStat(string str)
        {
            if (CurLogHeight++ > 400)
            {
                 Const.Main.logBox.Clear();
                CurLogHeight = 0;
            }
             Const.Main.logBox.AppendText("\r\n["+DateTime.Now.ToString("yyMMddHHmmss")+"]"+str);
        }
        public static void ShowStatInvoke(string str)
        {
            MethodInvoker mi = new MethodInvoker(() =>
            {
                if (CurLogHeight++ > 400)
                {
                    Const.Main.logBox.Clear();
                    CurLogHeight = 0;
                }
                Const.Main.logBox.AppendText("\r\n[" + DateTime.Now.ToString("yyMMddHHmmss") + "]" + str);
            });
            Const.Main.BeginInvoke(mi);
        }
        public static void Debug(string str)
        {
            if (!Const.IsDebug) return;
            if (CurLogHeight++ > 400)
            {
                Const.Main.logBox.Clear();
                CurLogHeight = 0;
            }
            Const.Main.logBox.AppendText("\r\n[" + DateTime.Now.ToString("yyMMddHHmmss") + "]" + str);
        }
    }
}

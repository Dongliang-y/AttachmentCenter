using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Text;

namespace AttachmentCenter
{
    public class FsLog
    {
        /// <summary>
        /// 获取上传目录
        /// </summary>
        /// <returns></returns>
        public static string GetLogPath()
        {
            string rootdir = AppContext.BaseDirectory;
            DirectoryInfo Dir = Directory.GetParent(rootdir);
            string root = Dir.Parent.Parent.FullName;
            return root;
        }
        /// <summary>
        ///  写入日志
        /// </summary>
        /// <param name="userpath">传入的物理路径</param>
        /// <param name="username">用户名</param>
        /// <param name="ContentStr">操作</param>
        public static void SaveLogs( string logDic, string ContentStr)
        {
            string userpath = "D:\\publish";
            StreamWriter sw = null;
            DateTime date = DateTime.Now;
            //年-月
            string FileName = date.Year + "-" + date.Month;
            try
            {
                FileName = userpath + "Logs\\" + date.Year + "-" + date.Month + "\\" + date.Year + "-" + date.Month + "-" + date.Day + ".log";

                #region 检测日志目录是否存在
                if (!Directory.Exists(userpath + "Logs"))
                {
                    Directory.CreateDirectory(userpath + "Logs");
                }
                if (!Directory.Exists(userpath + "Logs\\" + date.Year + "-" + date.Month))
                {
                    Directory.CreateDirectory(userpath + "Logs\\" + date.Year + "-" + date.Month);
                }
                if (!File.Exists(FileName))

                    sw = File.CreateText(FileName);

                else
                {
                    sw = File.AppendText(FileName);
                }
                #endregion
;
                sw.WriteLine("content    :" + ContentStr);
                sw.WriteLine("Time       :" + System.DateTime.Now);
                sw.WriteLine("≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡\r");
                sw.Flush();
            }
            finally
            {
                if (sw != null)
                    sw.Close();
            }
        }
    }
}

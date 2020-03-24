/***********************************************************
**��Ŀ����:	                                                                  				   
**��������:	  ��ժҪ˵��
**��    ��: 	�׶���                                         			   
**�� �� ��:	1.0                                             			   
**�������ڣ� 2015/12/29 16:15:32
**�޸���ʷ��
************************************************************/

namespace AttachmentCenter.Loger
{
    using System;
    using System.IO;

    using log4net;
    using log4net.Config;
    using log4net.Repository;

    /// <summary>
    /// ��־��¼��
    /// ʹ��ǰ�ȼ������� ��
    /// </summary>
    public static class LogerHelper
    {
        /// <summary>
        /// loger reposityory
        /// </summary>
        public static ILoggerRepository LogRepository { get; } = LogManager.CreateRepository("NETCoreRepository");

        /// <summary>
        /// The loger.
        /// </summary>
        public static ILog Log{ get; set; } 

        /// <summary>
        /// ����
        /// </summary>
        static LogerHelper()
        {
            // ����log4net
            log4net.Config.XmlConfigurator.Configure(LogRepository, new System.IO.FileInfo(System.IO.Directory.GetCurrentDirectory() + "/Config/log4net.config"));

            // ����logʵ��
            Log = LogManager.GetLogger(LogRepository.Name, AppDomain.CurrentDomain.FriendlyName);
            var cfg = Log.Logger.Repository.ConfigurationMessages;

            Log.Info("LoadLogger");
        }

        /// <summary>
        /// ����
        /// </summary>
        /// <param name="msg">
        /// The msg.
        /// </param>
        /// <param name="logType">
        /// The log Type.
        /// </param>
        public static void Info(string msg, string logType = "")
        {
            Log.Info($"{logType} -{msg}");
        }

        /// <summary>
        /// ������־
        /// </summary>
        public static void Debug(string msg, string logType = "")
        {
            Log.Debug($"{logType} -{msg}");
        }

        /// <summary>
        /// �쳣��־
        /// </summary>
        public static void Error(string msg, string logType)
        {
            Log.Error($"{logType} -{msg}");
        }

        /// <summary>
        /// �쳣��־
        /// </summary>
        public static void Error(string msg, string logType, Exception ex)
        {
            Log.Error($"{logType} -{msg}", ex);
        }
        
        /// <summary>
        /// ����
        /// </summary>
        public static void Warn(string msg, string logType)
        {
            Log.Warn($"{logType} -{msg}");
        }
    }
}
/***********************************************************
**项目名称:	                                                                  				   
**功能描述:	  的摘要说明
**作    者: 	易栋梁                                         			   
**版 本 号:	1.0                                             			   
**创建日期： 2015/12/29 16:15:32
**修改历史：
************************************************************/

namespace AttachmentCenter.Loger
{
    using System;
    using System.IO;

    using log4net;
    using log4net.Config;
    using log4net.Repository;

    /// <summary>
    /// 日志记录类
    /// 使用前先加载配置 。
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
        /// 构造
        /// </summary>
        static LogerHelper()
        {
            // 配置log4net
            log4net.Config.XmlConfigurator.Configure(LogRepository, new System.IO.FileInfo(System.IO.Directory.GetCurrentDirectory() + "/Config/log4net.config"));

            // 创建log实例
            Log = LogManager.GetLogger(LogRepository.Name, AppDomain.CurrentDomain.FriendlyName);
            var cfg = Log.Logger.Repository.ConfigurationMessages;

            Log.Info("LoadLogger");
        }

        /// <summary>
        /// 警告
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
        /// 调试日志
        /// </summary>
        public static void Debug(string msg, string logType = "")
        {
            Log.Debug($"{logType} -{msg}");
        }

        /// <summary>
        /// 异常日志
        /// </summary>
        public static void Error(string msg, string logType)
        {
            Log.Error($"{logType} -{msg}");
        }

        /// <summary>
        /// 异常日志
        /// </summary>
        public static void Error(string msg, string logType, Exception ex)
        {
            Log.Error($"{logType} -{msg}", ex);
        }
        
        /// <summary>
        /// 警告
        /// </summary>
        public static void Warn(string msg, string logType)
        {
            Log.Warn($"{logType} -{msg}");
        }
    }
}
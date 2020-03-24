/*************************************************************************************
  * CLR版本：       4.0.30319.42000
  * 类 名 称：       FileProcess
  * 机器名称：       DESKTOP123
  * 命名空间：       AttachmentCenter.UtilsHelp
  * 文 件 名：       FileProcess
  * 创建时间：       2020-1-6 16:24:16
  * 作    者：          xxx
  * 说   明：。。。。。
  * 修改时间：
  * 修 改 人：
*************************************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Drawing;
using AttachmentCenter.Loger;

namespace AttachmentCenter.UtilsHelp
{
    public class ThumbnailConfig
    {
        /// <summary>
        /// 缩略图模式，0正方形 ，1 按比率缩写。
        /// </summary>
        public int ThumbnailType
        {
            get; set;
        } = 1;
        /// <summary>
        /// 压缩比，0 - 100
        /// </summary>
        public int Compress
        { get; set; } = 60;

        /// <summary>
        /// 最大边长
        /// </summary>
        public int MaxLength { get; set; } = 300;
    }
    public class FileProcess
    {
        public static string Process(FileInfo fiold, string waterText)
        {
            try
            {
                using (var fs = fiold.Open(FileMode.Open, FileAccess.Read))
                {
                    System.Drawing.Image img = System.Drawing.Image.FromStream(fs);
                }
            }
            catch (Exception ex)
            {
                return "";
            }

            LogerHelper.Debug("saveFileLogs", "后缀名：" + fiold.Extension + ":::文件名：" + fiold.Name);
            try
            {
                if (!string.IsNullOrWhiteSpace(waterText))
                {
                    string oldpath = fiold.FullName;
                    string newpath = fiold.FullName;
                    AttachmentCenter.UtilsHelp.Watermark.AddWaterText(fiold.FullName, waterText, newpath, 118, 18);
                    LogerHelper.Debug("saveFileLogs", ":::水印文件名：" + fiold.Name);
                }
            }
            catch (Exception e)
            {
                LogerHelper.Error(e.Message, "FileProcess", e);
                throw new Exception("水印添加失败！" + e.Message);
            }
            try
            {
                var tName = fiold.FullName.Insert(fiold.FullName.LastIndexOf('.'), "Thumbnail");
                ThumbnailConfig config = Controllers.ConfigHelper.GetSetting<ThumbnailConfig>("Thumbnail");
                switch (config.ThumbnailType)
                {
                    case 0:
                        {
                            Thumbnail.MakeSquareImage(fiold.FullName, tName, (int)config.MaxLength);
                            break;
                        }
                    case 1:
                        {
                            Thumbnail.MakeThumbnailImage(fiold.FullName, tName,config.MaxLength, config.MaxLength);
                            break;
                        }
                }
                return new FileInfo( tName).Name;
            }
            catch (Exception ex)
            {
                LogerHelper.Error(ex.Message, "FileProcess", ex);
            }
            return "";
        }
    }
}

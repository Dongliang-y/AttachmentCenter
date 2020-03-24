/*************************************************************************************
  * CLR版本：       4.0.30319.42000
  * 类 名 称：       StringEncode
  * 机器名称：       DESKTOP123
  * 命名空间：       AttachmentCenter.UtilsHelp
  * 文 件 名：       StringEncode
  * 创建时间：       2020-1-8 9:25:16
  * 作    者：          xxx
  * 说   明：。。。。。
  * 修改时间：
  * 修 改 人：
*************************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AttachmentCenter.UtilsHelp
{
    public class StringEncode
    {
        public static string EncodeMy(string url)
        {
            url = url.Replace("#", "%23");
            url = url.Replace("$", "%24");
            url = url.Replace("+", "%2B");
            url = url.Replace("&", "%26");
            url = url.Replace("@", "%40");
            url = url.Replace("?", "？");
            url = url.Replace("/", "%2F");
            url = url.Replace(",", "%2C");
            return url;
        }
    }
}

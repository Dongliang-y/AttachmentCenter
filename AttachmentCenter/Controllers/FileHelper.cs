using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AttachmentCenter.Controllers
{
    /// <summary>
    ///FileHelper 的摘要说明
    /// </summary>
    public static class FileHelper
    {
        /// <summary>
        /// 保存文件
        /// </summary>
        /// <param name="file"></param>
        /// <param name="fullPath"></param>
        public static void SaveFile(this IFormFile file, string fullPath)
        {
            using (System.IO.FileStream fi = new FileStream(fullPath, FileMode.OpenOrCreate))
            {
                file.CopyTo(fi);
             //   stream.
            }
        }
    
        /// <summary>
        /// 获取上传目录
        /// </summary>
        /// <returns></returns>
        public static string GetUploadPath(string virtualDic)
        {
           
            string rootdir = AppContext.BaseDirectory;
            DirectoryInfo Dir = Directory.GetParent(rootdir);
            string root = Dir.FullName;
#if (DEBUG)
            root=Dir.Parent.Parent.FullName;
#endif
            var fullPath = $"{root}/Upload/{virtualDic}";
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
            }
            return fullPath;
        }

        /// <summary>
        /// 获取临时目录
        /// </summary>
        /// <returns></returns>
        public static string GetTempPath()
        {
            string rootdir = AppContext.BaseDirectory;
            DirectoryInfo Dir = Directory.GetParent(rootdir);
            string root = Dir.FullName;
#if (DEBUG)
            root=Dir.Parent.Parent.FullName;
#endif
            var fullPath = $"{root}\\temp";
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
            }
            return fullPath;
        }
    }
}

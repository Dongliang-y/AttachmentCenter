using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AttachmentCenter.Controllers
{
    /// <summary>
    /// 返回的文件信息
    /// </summary>
    public class FileResultDto
    {
        /// <summary>
        /// 原始文件名
        /// </summary>
        public string OriginalName { get; set; }
        /// <summary>
        /// 保存的文件名
        /// </summary>
        public string SaveName { get; set; }
        /// <summary>
        /// Thumbnail
        /// </summary>
        public string ThumbnailName { get; set; }
    }
}

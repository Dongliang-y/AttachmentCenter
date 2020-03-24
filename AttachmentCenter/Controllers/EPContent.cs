/********************************************************************************

** auth： DongliangYi

** date： 2017/8/24 12:34:28

** desc： 尚未编写描述

** Ver.:  V1.0.0

*********************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace AttachmentCenter.Controllers
{
    /// <summary>
    /// 返回类型
    /// </summary>
    public class EPContent<T>
    {
        /// <summary>
        /// 数据
        /// </summary>
        public List<T> Datas { get; set; }
        /// <summary>
        /// 信息
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// 代码，true成功，false 失败
        /// </summary>
        [JsonProperty(PropertyName = "success")]
        public bool Code { get; set; }
        /// <summary>
        /// 数据列表内容对象
        /// </summary>
        public EPContent(string message, bool success, List<T> datas)
        {
            Message = message;
            Code = success;
            Datas = datas;
        }
    }
}
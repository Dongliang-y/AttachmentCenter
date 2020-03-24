using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AttachmentCenter.Controllers
{
    using System.IO;

    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Configuration.Json;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;

    public class ConfigHelper
    {
        /// <summary>
        /// 获取配置节点对象
        /// </summary>   
        public static T GetSetting<T>(string key, string fileName = "appsettings.json") where T : class, new()
        {
            IConfiguration config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .Add(new JsonConfigurationSource { Path = fileName, Optional = false, ReloadOnChange = true })
                .Build();
            var appconfig = new ServiceCollection()
                .AddOptions()
                .Configure<T>(config.GetSection(key))
                .BuildServiceProvider()
                .GetService<IOptions<T>>()
                .Value;
            return appconfig;
        }
    }
}

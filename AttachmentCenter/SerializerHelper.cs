/***
 *  基础设施
 */

namespace AttachmentCenter
{
    using System;
    using System.IO;
    using System.Net;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Text;
    using System.Xml.Serialization;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// IPEndPoint转换器,json序列化专用
    /// </summary>
    public class IPAddressConverter : JsonConverter
    {
        /// <summary>
        /// 能否转换
        /// </summary>
        /// <param name="objectType"></param>
        /// <returns></returns>
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(IPAddress));
        }

        /// <summary>
        /// 从json 读取
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="objectType"></param>
        /// <param name="existingValue"></param>
        /// <param name="serializer"></param>
        /// <returns></returns>
        public override object ReadJson(
            JsonReader reader,
            Type objectType,
            object existingValue,
            JsonSerializer serializer)
        {
            JToken token = JToken.Load(reader);
            return IPAddress.Parse(token.Value<string>());
        }

        /// <summary>
        /// 转换成json
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        /// <param name="serializer"></param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            IPAddress ip = (IPAddress)value;
            writer.WriteValue(ip.ToString());
        }
    }

    /// <summary>
    /// IPEndPoint转换器,json序列化专用
    /// </summary>
    public class IPEndPointConverter : JsonConverter
    {
        /// <summary>
        /// 能否转换
        /// </summary>
        /// <param name="objectType"></param>
        /// <returns></returns>
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(IPEndPoint));
        }

        /// <summary>
        /// 读取json 
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="objectType"></param>
        /// <param name="existingValue"></param>
        /// <param name="serializer"></param>
        /// <returns></returns>
        public override object ReadJson(
            JsonReader reader,
            Type objectType,
            object existingValue,
            JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);
            IPAddress address = jo["Address"].ToObject<IPAddress>(serializer);
            int port = jo["Port"].Value<int>();
            return new IPEndPoint(address, port);
        }

        /// <summary>
        /// 转换成json
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        /// <param name="serializer"></param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            IPEndPoint ep = (IPEndPoint)value;
            writer.WriteStartObject();
            writer.WritePropertyName("Address");
            serializer.Serialize(writer, ep.Address);
            writer.WritePropertyName("Port");
            writer.WriteValue(ep.Port);
            writer.WriteEndObject();
        }
    }

    /// <summary>
    ///     序列化
    /// </summary>
    public static class SerializerHelper
    {
        /// <summary>
        ///     把字节反序列化成相应的对象
        /// </summary>
        /// <param name="pBytes">字节流</param>
        /// <returns>object</returns>
        public static T FromByte<T>(byte[] pBytes)
        {
            object newOjb = null;
            if (pBytes == null)
                throw new ArgumentNullException("pBytes");
            var memory = new MemoryStream(pBytes);
            memory.Position = 0;
            var formatter = new BinaryFormatter();
            newOjb = formatter.Deserialize(memory);
            memory.Close();
            return (T)newOjb;
        }

        /// <summary>
        ///     byte反序列化成obj
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <returns>对象</returns>
        public static T FromByteFile<T>(string filePath)
        {
            if (filePath == null) throw new ArgumentNullException("filePath");
            if (!File.Exists(filePath))
                return default(T);
            var fs = new FileStream(filePath, FileMode.Open);
            var binFormat = new BinaryFormatter();

            var obj = (T)binFormat.Deserialize(fs);
            return obj;
        }

        /// 反序列化Json对象
        /// 
        /// 需要转换成的对象
        /// Json串
        /// 。net对象
        public static T FromJson<T>(string sJson)
        {
            try
            {
                JsonSerializerSettings settings = new JsonSerializerSettings();
                settings.Converters.Add(new IPAddressConverter());
                settings.Converters.Add(new IPEndPointConverter());
                settings.Formatting = Formatting.Indented;

                return JsonConvert.DeserializeObject<T>(sJson, settings);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        ///     xml反序列化成obj
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="xmlString">xmlstring</param>
        /// <returns>对象</returns>
        public static T FromXml<T>(string xmlString)
        {
            var dser = new XmlSerializer(typeof(T));

            // xmlString是你从数据库获取的字符串
            Stream xmlStream = new MemoryStream(Encoding.UTF8.GetBytes(xmlString));

            var obj = (T)dser.Deserialize(xmlStream);
            return obj;
        }

        /// <summary>
        ///  对象序列化成Byte
        /// </summary>
        /// <param name="obj">对象</param>
        /// <typeparam name="T">对象类型T</typeparam>
        /// <returns>byte[]</returns>
        public static byte[] ToByte<T>(T obj)
        {
            if (obj == null)
                return null;
            var memory = new MemoryStream();
            var formatter = new BinaryFormatter();
            formatter.Serialize(memory, obj);
            memory.Position = 0;
            var read = new byte[memory.Length];
            memory.Read(read, 0, read.Length);
            memory.Close();
            return read;
        }

        /// <summary>
        /// 对象序列化成二进制文件
        /// </summary>
        public static void ToByteFile<T>(T obj, string strFilePath)
        {
            if (!new FileInfo(strFilePath).Directory.Exists)
            {
                throw new Exception(string.Format("序列化失败，目录不存在-{0}", strFilePath));
            }

            using (var fs = new FileStream(strFilePath, FileMode.Create))
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(fs, obj);
            }
        }

        /// <summary>
        /// .net对象序列化为Json对象
        /// 
        /// 。net对象类型
        /// 需要序列化的。net对象
        /// Json对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string ToJson<T>(T obj)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.Converters.Add(new IPAddressConverter());
            settings.Converters.Add(new IPEndPointConverter());
            settings.Formatting = Formatting.Indented;

            var script = JsonConvert.SerializeObject(obj, settings);
            return script;
        }

        /// <summary>
        ///     对象序列化成xml
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="obj">对象</param>
        /// <returns>序列化结果</returns>
        public static string ToXml<T>(T obj)
        {
            var ser = new XmlSerializer(typeof(T));

            var ms = new MemoryStream();

            ser.Serialize(ms, obj);
            var xmlString = Encoding.UTF8.GetString(ms.ToArray());
            return xmlString;
        }
    }
}
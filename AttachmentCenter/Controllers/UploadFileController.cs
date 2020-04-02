//Dongliang Yi
//Date： 2018 -11-16 AM 11
//file upload Controller
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OfficeHelper;
using Microsoft.AspNetCore.StaticFiles;
using System.Net;
using AttachmentCenter.UtilsHelp;
using AttachmentCenter.Loger;

namespace AttachmentCenter.Controllers
{
    /// <summary>
    /// //file upload Controller
    /// </summary>
    //[PermissionRequired]
    [ApiController]
    [Route("[controller]")]
    public class UploadFileController : ControllerBase
    {
        /// <summary>
        /// file move lock
        /// </summary>
        private object fileMoveLock = new object();
        /// <summary>
        /// 获取文件
        /// </summary>
        /// <param name="saveName">文件名</param>
        /// <returns>文件会自动下载</returns>
        [HttpGet("Files/{saveName}")]
        public async Task<IActionResult> Files(string saveName)
        {
            saveName = saveName.ToLower();
            return await Get(saveName);
        }

        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="saveName">文件名</param>
        /// <returns>文件会自动下载</returns>
        [HttpDelete("Delete/{saveName}")]
        public IActionResult Delete(string saveName)
        {
            saveName = saveName.ToLower();
            DirectoryInfo directoryInfo = new DirectoryInfo(FileHelper.GetUploadPath(string.Empty));
            FileInfo foundFileInfo = directoryInfo.GetFiles(saveName, SearchOption.AllDirectories).FirstOrDefault();
            if (foundFileInfo == null)
            {
                foundFileInfo = directoryInfo.GetFiles(StringEncode.EncodeMy(saveName), SearchOption.AllDirectories).FirstOrDefault();
            }
            if (foundFileInfo == null)
            {
                foundFileInfo = directoryInfo.GetFiles(HttpUtility.UrlDecode(saveName), SearchOption.AllDirectories).FirstOrDefault();
            }
            if (foundFileInfo != null)
            {
                try
                {
                    foundFileInfo.Delete();
                    return new JsonResult("{\"datas\":\"\",\"message\": \"删除成功\",\"success\": true }");
                }
                catch(Exception ex)
                {
                    LogerHelper.Error(ex.ToString(), "Delete");
                    return new JsonResult("{\"datas\":\"\",\"message\": \"删除失败+"+ex.Message+"+\",\"success\": false }");
                }
            }
            return NotFound();
        }

        /// <summary>
        /// 获取文件
        /// </summary>
        /// <param name="fileName">文件名</param>
        /// <returns>文件会自动下载</returns>
        [HttpGet("Get")]
        public async Task<IActionResult> Get(string fileName)
        {
            fileName = fileName.ToLower();
            if (string.IsNullOrEmpty(fileName))
            {
                return NotFound();
            }
            try
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(FileHelper.GetUploadPath(string.Empty));
                FileInfo foundFileInfo = directoryInfo.GetFiles(fileName, SearchOption.AllDirectories).FirstOrDefault();
                if(foundFileInfo==null)
                {
                    foundFileInfo = directoryInfo.GetFiles(StringEncode.EncodeMy(fileName), SearchOption.AllDirectories).FirstOrDefault();
                }
                if (foundFileInfo == null)
                {
                    foundFileInfo = directoryInfo.GetFiles(HttpUtility.UrlDecode(fileName), SearchOption.AllDirectories).FirstOrDefault();
                }
                if (foundFileInfo != null)
                {
                    var memoryStream = new MemoryStream((int)foundFileInfo.Length);
                    using (var stream = new FileStream(foundFileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        await stream.CopyToAsync(memoryStream);
                    }

                    memoryStream.Seek(0, SeekOrigin.Begin);

                    // 获取文件的ContentType
                    var provider = new FileExtensionContentTypeProvider();

                    var memi = provider.Mappings[foundFileInfo.Extension];

                    var idx = fileName.IndexOf("_S_", StringComparison.Ordinal);
                    var srcName = fileName;

                    if (idx > 0)
                    {
                        srcName = srcName.Substring(0, idx) + foundFileInfo.Extension;
                    }

                    // 文件名必须编码，否则会有特殊字符(如中文)无法在此下载。
                    string encodeFilename = HttpUtility.UrlEncode(srcName, System.Text.Encoding.GetEncoding("UTF-8"));
                    if (!memi.Contains("image") && !memi.Contains("pdf")&&!VideoHelper.VideoExts.Contains(foundFileInfo.Extension.ToLower()))
                    {
                        Response.Headers.Add("Content-Disposition", "attachment;filename=\"" + encodeFilename + "\"");
                    }
                    Response.Headers.Add("Accept-Charset", "UTF-8");
                    Response.Headers.Add("Content-type", memi);
                    return new FileContentResult(memoryStream.GetBuffer(), memi);
                }
            }
            catch (Exception ex)
            {
                LogerHelper.Error(ex.ToString(), "Get");
                Response.WriteAsync("Message" + ex.ToString());
            }

            return NotFound();
        }



        /// <summary>
        /// Post file 
        /// </summary>
        /// <param name="virtualDic">Save directory </param>
        /// <param name="isapp">Save directory </param>
        /// <param name="createThumbnail">是否生成缩略图</param>
        /// <returns></returns>
        [HttpPost("Upload")]
        public EPContent<FileResultDto> Upload(string virtualDic, string waterText = "", bool createThumbnail=true)
        {
            int currentChunks;
            int.TryParse(Request.Form["chunk"].ToString(), out currentChunks);
            int chunksTotal;
            int.TryParse(Request.Form["chunks"].ToString(), out chunksTotal);
            createThumbnail = true;
            var fullPath = FileHelper.GetUploadPath(HttpUtility.UrlDecode(virtualDic));
            List<FileResultDto> files = new List<FileResultDto>();
            if (Request.Form.Files.Count > 0) //批量上传文件
            {
                try
                {
                    for (int j = 0; j < Request.Form.Files.Count; j++)
                    {
                        var uploadFile = Request.Form.Files[j];

                        int offset = currentChunks; //当前分块
                        int total = chunksTotal;//总的分块数量

                        //文件没有分块
                        if (total == 0)
                        {
                            var fileOldName = StringEncode.EncodeMy(uploadFile.FileName.ToLower());
                            System.IO.FileInfo fiold = new FileInfo(fileOldName);
                            //生成唯一的保存文件的名字和路径
                            var pointIdx = fiold.Name.LastIndexOf('.');
                            var newName = fiold.Name.Insert(pointIdx, $"_S_{ DateTime.Now.ToString("yyyyMMddHHmmssfff")}");
                            var newfilepath = $"{fullPath}\\{newName}";
                            LogerHelper.Debug($"saveFileLogs{newfilepath}", "Upload");

                            if (uploadFile.Length > 0)
                            {
                                uploadFile.SaveFile(newfilepath);
                                var thumbnailName= FileProcess.Process(new FileInfo(newfilepath), waterText);

                                var fileRst = new FileResultDto()
                                {
                                    OriginalName = uploadFile.FileName,
                                    SaveName = newName,
                                    ThumbnailName = thumbnailName,
                                    VirtulPath = $"/Upload/{ HttpUtility.UrlDecode(virtualDic) }/{newName}"
                                };
                                if (!string.IsNullOrEmpty(thumbnailName))
                                {
                                    fileRst.ThumbnailVirtulPath = $"/Upload/{ HttpUtility.UrlDecode(virtualDic) }/{thumbnailName}";
                                }

                                files.Add(fileRst);
                            }
                        }
                        else
                        {
                            // 分块上传，则从form里面取文件名，每次存储的都是分块，不是文件，files里面提交的也是文件块。
                            string name = Request.Form["name"].ToString().ToLower();
                            var hisName = name;
                            name = StringEncode.EncodeMy(name);
                            // 文件 分成多块上传
                            var fileName = WriteTempFile(uploadFile, offset, name);

                            if (total - offset == 1)
                            {
                                //生成唯一的保存文件的名字和路径
                                System.IO.FileInfo fiold = new FileInfo(name);
                                var pointIdx = fiold.Name.LastIndexOf('.');
                                var newName = fiold.Name.Insert(pointIdx, $"_S_{ DateTime.Now.ToString("yyyyMMddHHmmssfff")}");
                                var newfilepath = $"{fullPath}\\{newName}";

                                // 如果是最后一个分块文件 ，则把文件从临时文件夹中移到上传文件 夹中
                                FileInfo newFile = new FileInfo(newfilepath);
                                if (newFile.Exists)
                                {
                                    // 文件名存在则删除旧文件 
                                    newFile.Delete();
                                }

                                lock (fileMoveLock)
                                {
                                    System.IO.File.Move(fileName, newfilepath);
                                }

                                var thumbnailName = FileProcess.Process(new FileInfo(newfilepath), waterText);
                                var fileRst = new FileResultDto()
                                {
                                    OriginalName = hisName,
                                    SaveName = newName,
                                    ThumbnailName = thumbnailName,
                                    VirtulPath = $"/Upload/{ HttpUtility.UrlDecode(virtualDic) }/{newName}"
                                };
                                if(!string.IsNullOrEmpty( thumbnailName))
                                {
                                    fileRst.ThumbnailVirtulPath = $"/Upload/{ HttpUtility.UrlDecode(virtualDic) }/{thumbnailName}";
                                }
                                files.Add(fileRst);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Response.WriteAsync("Message" + ex.ToString());
                    LogerHelper.Error(ex.ToString(), "Upload");
                    return new EPContent<FileResultDto>(ex.Message, false, null);
                }
            }

            Response.Headers[Microsoft.Net.Http.Headers.HeaderNames.CacheControl] = "no-cache";
            LogerHelper.Debug(SerializerHelper.ToJson(files));
            return new EPContent<FileResultDto>(string.Empty, true, files);
        }
        /// <summary>
        /// 保存临时文件 
        /// </summary>
        /// <param name="uploadFile"></param>
        /// <param name="chunk"></param>
        /// <returns></returns>
        private string WriteTempFile(IFormFile uploadFile, int chunk, string name)
        {
            string tempDir = FileHelper.GetTempPath();
            string fullName = string.Format("{0}\\{1}.part", tempDir, name);
            if (chunk == 0)
            {
                //如果是第一个分块，则直接保存
                //如果是其他分块文件 ，则原来的分块文件，读取流，然后文件最后写入相应的字节
                using (FileStream fs = new FileStream(fullName, FileMode.Create))
                {
                    if (uploadFile.Length > 0)
                    {
                        int FileLen = (int)uploadFile.Length;
                        byte[] input = new byte[FileLen];

                        // Initialize the stream.
                        using (System.IO.Stream MyStream = uploadFile.OpenReadStream())
                        {
                            // Read the file into the byte array.
                            MyStream.Read(input, 0, FileLen);

                            var t = fs.WriteAsync(input, 0, FileLen);
                            t.Wait();
                            MyStream.Close();
                            MyStream.Dispose();
                            fs.Close();
                            fs.Dispose();
                        }
                    }
                }
            }
            else
            {
                //如果是其他分块文件 ，则原来的分块文件，读取流，然后文件最后写入相应的字节
                using (FileStream fs = new FileStream(fullName, FileMode.Append))
                {
                    if (uploadFile.Length > 0)
                    {
                        int FileLen = (int)uploadFile.Length;
                        byte[] input = new byte[FileLen];

                        // Initialize the stream.
                        using (System.IO.Stream MyStream = uploadFile.OpenReadStream())
                        {
                            // Read the file into the byte array.
                            MyStream.Read(input, 0, FileLen);
                            MyStream.Close();
                            MyStream.Dispose();

                            var t = fs.WriteAsync(input, 0, FileLen);
                            t.Wait();
                            fs.Close();
                            fs.Dispose();
                        }
                    }
                }
            }
            return fullName;
        }

        /*
                /// <summary>
                /// 转换Excel
                /// </summary>
                /// <param name="virtualUrl">文件相对路径</param>
                /// <returns></returns>
                [HttpGet]
                public HttpResponse ConvertExcel(string virtualUrl)
                {
                    if (!virtualUrl.StartsWith("/"))
                        virtualUrl = "/" + virtualUrl;
                    string filePath = HttpContext.Current.Server.MapPath(virtualUrl);

                    FileInfo fi = new FileInfo(filePath);
                    if (fi.Extension.ToLower() == ".xls" || fi.Extension.ToLower() == ".xlsx")
                    {
                        if (fi.Exists)
                        {
                            try
                            {
                                ConvertTask.ConvertXls(filePath);
                            }
                            catch (Exception ex)
                            {
                                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
                            }
                        }
                        else
                            return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, string.Format("文件不存在：{0}", virtualUrl));
                    }
                    else
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, string.Format("非Excel文件：{0}", virtualUrl));
                    }

                    return Request.CreateResponse(HttpStatusCode.OK);
                }


                /// <summary>
                /// 更新并转换Excel
                /// </summary>
                /// <param name="listAlert">更新列表</param>
                /// <param name="virtualUrl">文件相对路径</param>
                /// <returns></returns>
                [HttpPost]
                public HttpResponseMessage AlterAndConvertExcel(string virtualUrl)
                {

                    try
                    {

                        if (!virtualUrl.StartsWith("/"))
                            virtualUrl = "/" + virtualUrl;
                        List<AlertExcelJson> listAlert = null;
                        var dicPath =  HttpContext.Current.Server.MapPath(virtualUrl);

                        FileInfo fi = new FileInfo(dicPath);
                        if (!fi.Exists)
                        {
                            return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,string.Format("文件不存在：{0}",dicPath));
                        }
                        if (fi.Extension.ToLower() == ".xls" || fi.Extension.ToLower() == ".xlsx")
                        {
                            try
                            {
                                StreamReader sr = new StreamReader(HttpContext.Current.Request.InputStream);
                                var js = sr.ReadToEnd();
                                js = HttpUtility.UrlDecode(js);
                                listAlert = JSONHelper.FromJson<List<AlertExcelJson>>(js);
                                LogMy.Write(js);
                            }
                            catch (Exception ex)
                            {
                            }
                            AlertExcel.UpdateExcel(listAlert, dicPath);
                            return ConvertExcel(virtualUrl);
                        }
                        else
                        {
                            return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, string.Format("非Excel文件：{0}", dicPath));
                        }
                    }
                    catch (Exception ex)
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
                    }
                    return Request.CreateResponse(HttpStatusCode.OK);
                }
                /// <summary>
                /// 更新并转换Excel
                /// </summary>
                /// <param name="listAlert">更新列表</param>
                /// <param name="virtualUrl">文件相对路径</param>
                /// <returns></returns>
                [HttpPost]
                public HttpResponseMessage AlterExcel(string virtualUrl)
                {

                    try
                    {
                        if (!virtualUrl.StartsWith("/"))
                            virtualUrl = "/" + virtualUrl;
                        List<AlertExcelJson> listAlert = null;

                        string dicPath = HttpContext.Current.Server.MapPath(virtualUrl);
                        FileInfo fi = new FileInfo(dicPath);
                        if (!fi.Exists)
                        {
                            return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, string.Format("文件不存在：{0}", dicPath));
                        }
                        if (fi.Extension.ToLower() == ".xls" || fi.Extension.ToLower() == ".xlsx")
                        {
                            try
                            {
                                StreamReader sr = new StreamReader(HttpContext.Current.Request.InputStream);
                                var js = sr.ReadToEnd();
                                js = HttpUtility.UrlDecode(js);
                                listAlert = JSONHelper.FromJson<List<AlertExcelJson>>(js);
                            }
                            catch (Exception ex)
                            {
                            }
                            AlertExcel.UpdateExcel(listAlert, dicPath);
                        }
                        else
                        {
                            return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, string.Format("非Excel文件：{0}", dicPath));
                        }
                    }
                    catch (Exception ex)
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
                    }
                    return Request.CreateResponse(HttpStatusCode.OK);
                }
                /// <summary>
                /// 删除Excel
                /// </summary>
                /// <param name="virtualUrl">文件相对路径</param>
                /// <returns></returns>
                [HttpGet]
                [LocalFilter()]public HttpResponseMessage DeleteFile(string virtualUrl)
                {
                    try
                    {
                        if (!virtualUrl.StartsWith("/"))
                            virtualUrl = "/" + virtualUrl;
                        string dir = HttpContext.Current.Server.MapPath(virtualUrl);
                        if (System.IO.File.Exists(dir))
                        {
                            System.IO.File.Delete(dir);
                        }
                    }
                    catch (Exception ex)
                    {
                       return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
                    }
                    return  Request.CreateResponse(HttpStatusCode.OK);
                }
                */
    }
}

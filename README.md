# Readme
## 文件上传
1. 接口地址：
- url:服务器地址/UploadFile/Upload?virtualDic&waterText&createThumbnail=false
- virtualDic 参数是必须的，建议规划好文件夹结构例如：“virtualDic=xx项目/xxx模块”
- aterText: 图片水印，字符参数，可选。
- createThumbnail，是否启用，bool参数，可选的。 缩略图是生成正方形或者按比率缩放是在服务端配置文件里调整的。
- 返回的结构:

```
{
    {
    "datas": 
    [ //多文件上传，此处返回的即为多个对象。
        { 
            "originalName": "测试测试sdfsf+ _@#￥.png", 
            "saveName": "%E6%B5%8B%E8%AF%95%E6%B5%8B%E8%AF%95sdfsf%2B%20_%40%23%E F%BF%A5_S_20200107141957202.png" 
        } 
    ],
    "message": "",
    "success": true
    }
}
```


2. 简单文件上传：

可以用传统的file表单，可以用第三方的upload插件，只要满足提交到后台的文件时存储在 Request.Form.Files对象中即可（传统文件上传皆如此），
同时提交一个url参数“virtualDic=你文件存储到服务器的目标文件夹

3.多文件上传：

满足条件2的情况下，同时向服务器提交多个文件即可。

4.大文件分块上传：

满足条件2，多文件满足条件3，另外分块大小不能大于服务器设定的上传限制，整体大小不限。

  
```
额外Form参数：
name： 文件原始名称（每次分块上传的是分块名称，而且每个分块名称必须唯一，所以需要传递一个原始文件名）
chunk： 当前分块序号，int类型，索引从0开始。
chunks： 总的分块数，int类型。
```

5.水印和缩略图：

满足条件2，3 。水印和缩略图只针对图片有效。 单文件、多文件上传均生效、分块上传无效。
## 文件获取
1. 获取文件

- 风格1：http://xxxx/Upload/上传时指定的虚拟目录/上传时返回的saveName

- 风格2：http://xxxx/UploadFile/Get?FileName=xxx.jpg 
    GET:服务器地址/UploadFile/Files/“上传时返回的saveName”
- 缩略图文件获取：方式与上述两种风格相同， 文件名在saveName基础上追加Thumbnail。

- **举个栗子：**
服务器地址是:http://39.106.163.216:5001
上传原文件名是：ql-2.jpg
上传的目录是 project/Reports
服务器返回一个： "saveName": "ql-2_S_20200108155128215Thumbnail.jpg"  
原图地址1：http://39.106.163.216:5001/UploadFile/Get?FileName=ql-2_S_20200108155128215.jpg
缩略图地址1：http://39.106.163.216:5001/UploadFile/Get?FileName=ql-2_S_20200108155128215Thumbnail.jpg
原图地址2：http://39.106.163.216:5001/Upload/project/Reports/ql-2_S_20200108155128215.jpg
缩略图地址2：http://39.106.163.216:5001/Upload/project/Reports/ql-2_S_20200108155128215Thumbnail.jpg
**两者差别就是:**
第一个种不用带目录，浏览器能当附件下载（常用于附件)
第二种要带目录，支持在线播放（视频），在线预览

## 文件删除
6. 删除文件 DELETE: 

服务器地址/UploadFile/Delete/“上传时返回的saveName”

## 示例
[多文件上传示例](/Test.html)

[单文件上传示例](/UploadSingle.html)
## 更新日志：
2020 -1-9 ：Version Beta 1.0.2修复MP4在线播放的bug，处理rar文件下载后大小变化的BUG。
2020 -1-8 ：Version Beta 1.0.1增加缩略图功能、增加文件直接访问的方式、增加单个文件上传示例、增加首页说明文档。
## 问题反馈
qq 270535197
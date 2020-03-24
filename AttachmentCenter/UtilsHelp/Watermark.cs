using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AttachmentCenter.UtilsHelp
{
    public class Watermark
    {
        /// <summary>
        /// 图片加水印文字
        /// </summary>
        /// <param name="oldpath">旧图片地址</param>
        /// <param name="text">水印文字</param>
        /// <param name="newpath">新图片地址</param>
        /// <param name="Alpha">透明度</param>
        /// <param name="fontsize">字体大小</param>
        public static void AddWaterText(string oldpath, string text, string newpath, int Alpha, int fontsize)
        {
            //text = text + "版权所有";
            FileStream fs = new FileStream(oldpath, FileMode.Open);
            BinaryReader br = new BinaryReader(fs);
            byte[] bytes = br.ReadBytes((int)fs.Length);
            br.Close();
            fs.Close();
            MemoryStream ms = new MemoryStream(bytes);

            System.Drawing.Image imgPhoto = System.Drawing.Image.FromStream(ms);
            int imgPhotoWidth = imgPhoto.Width;
            int imgPhotoHeight = imgPhoto.Height;

            Bitmap bmPhoto = new Bitmap(imgPhotoWidth, imgPhotoHeight, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            bmPhoto.SetResolution(72, 72);
            Graphics gbmPhoto = Graphics.FromImage(bmPhoto);
            // gif背景色
            gbmPhoto.Clear(Color.FromName("white"));
            gbmPhoto.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
            gbmPhoto.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            gbmPhoto.DrawImage(imgPhoto, new Rectangle(0, 0, imgPhotoWidth, imgPhotoHeight), 0, 0, imgPhotoWidth, imgPhotoHeight, GraphicsUnit.Pixel);
            System.Drawing.Font font = null;
            System.Drawing.SizeF crSize = new SizeF();
            font = new Font("宋体", fontsize, FontStyle.Bold);
            // 测量指定区域
            crSize = gbmPhoto.MeasureString(text, font);
            float y = imgPhotoHeight - crSize.Height;
            float x = imgPhotoWidth - crSize.Width;
            System.Drawing.StringFormat StrFormat = new System.Drawing.StringFormat();
            StrFormat.Alignment = System.Drawing.StringAlignment.Center;

            // 画两次制造透明效果
            System.Drawing.SolidBrush semiTransBrush2 = new System.Drawing.SolidBrush(Color.FromArgb(Alpha, 56, 56, 56));
            gbmPhoto.DrawString(text, font, semiTransBrush2, x + 1, y + 1);

            System.Drawing.SolidBrush semiTransBrush = new System.Drawing.SolidBrush(Color.FromArgb(Alpha, 176, 176, 176));
            gbmPhoto.DrawString(text, font, semiTransBrush, x, y);

            bmPhoto.Save(newpath, Thumbnail.GetFormat(oldpath));
            gbmPhoto.Dispose();
            imgPhoto.Dispose();
            bmPhoto.Dispose();
        }
    }
}

/*
 *  author：symbolspace
 *  e-mail：symbolspace@outlook.com
 */
using System.Drawing;
using System.Drawing.Imaging;
#if !net20 && !netcore
using System.Windows.Forms;
#endif
namespace Symbol.Drawing {

    /// <summary>
    /// 图像水印
    /// </summary>
    public static class ImageWaterMark {

        #region methods

        #region WaterMark
        /// <summary>
        /// 为图像加上水印（忽略gif格式），直接在原始图像上做处理。
        /// </summary>
        /// <param name="image">需要处理的图像</param>
        /// <param name="context">水印上下文实例</param>
        /// <returns>返回是否处理过图像。</returns>
        public static bool WaterMark(
#if !net20
            this
#endif
            Image image, ImageWaterMarkContext context) {
            if (image == null || context == null)
                return false;
#pragma warning disable CA1416 // 验证平台兼容性
            //Gif不加水印的
            if (image.RawFormat == ImageFormat.Gif)
                return false;
            float width;//水印宽
            float height;//水印高
            float x = 0F;//坐标.x
            float y = 0F;//坐标.y
            Graphics g = Graphics.FromImage(image);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            Bitmap waterMarkImage;
            if (context.Image != null) {
                //图像水印
                width = context.Image.Width;
                height = context.Image.Height;
                waterMarkImage = context.Image;
            } else {
                //文本水印
                if (context.TextFont == null)
                    context.TextFont = ImageWaterMarkContext.DefaultTextFont;
                SizeF size;
                if (context.MeasureStringFunc != null)
                    size = context.MeasureStringFunc(context.Text, context.TextFont);
                else
#if net20 || netcore
                    size= g.MeasureString("|"+context.Text+"|", context.TextFont);
#else
                    size = TextRenderer.MeasureText(context.Text, context.TextFont);
#endif
                width = size.Width + 12;
                height = size.Height + 12;
                waterMarkImage = new Bitmap((int)width, (int)height);
                var g2 = Graphics.FromImage(waterMarkImage);
                g2.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                g2.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                g2.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                g2.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
                g2.Clear(context.TextBackColor);
                g2.DrawRectangle(new Pen(context.TextBorderColor, 3F), 1F, 1F, width - 2F, height - 2F);
                g2.DrawString(context.Text, context.TextFont, new SolidBrush(context.TextForeColor), 6, 6);
                g2.Dispose();
            }
            //水印超出范围
            if ((width + context.Margin.Left + context.Margin.Right) > image.Width || (height + context.Margin.Top + context.Margin.Bottom) > image.Height)
                return false;

            //推算坐标
            switch (context.Location) {
                case ImageWaterMarkLocation.TopLeft:
                    x = context.Margin.Left;
                    y = context.Margin.Top;
                    break;
                case ImageWaterMarkLocation.TopCenter:
                    x = (image.Width - width) / 2; y = context.Margin.Top;
                    break;
                case ImageWaterMarkLocation.TopRight:
                    x = image.Width - context.Margin.Right - width; y = context.Margin.Top;
                    break;
                case ImageWaterMarkLocation.MiddleLeft:
                    x = context.Margin.Left; y = (image.Height - height) / 2;
                    break;
                case ImageWaterMarkLocation.MiddleCenter:
                    x = (image.Width - width) / 2; y = (image.Height - height) / 2;
                    break;
                case ImageWaterMarkLocation.MiddleRight:
                    x = image.Width - context.Margin.Right - width; y = (image.Height - height) / 2;
                    break;
                case ImageWaterMarkLocation.BottomLeft:
                    x = context.Margin.Left; y = image.Height - height - context.Margin.Bottom;
                    break;
                case ImageWaterMarkLocation.BottomCenter:
                    x = (image.Width - width) / 2; y = image.Height - height - context.Margin.Bottom;
                    break;
                case ImageWaterMarkLocation.BottomRight:
                    x = image.Width - context.Margin.Right - width; y = image.Height - height - context.Margin.Bottom;
                    break;
            }
            if (x < 0F || y < 0F)
                return false;
            //图像水印
            waterMarkImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);
            ImageAttributes imageAttributes = new ImageAttributes();
            imageAttributes.SetRemapTable(new ColorMap[]{
                    new ColorMap(){
                        OldColor= Color.FromArgb(255,0,255,0),
                        NewColor= Color.FromArgb(0,0,0,0),
                    }
                }, ColorAdjustType.Bitmap);
            if (context.Opacity < 0F || context.Opacity > 1F)
                context.Opacity = 0.51F;
            imageAttributes.SetColorMatrix(new ColorMatrix(new float[][]{
                    new float[]{1.0F,0.0F,0.0F,0.0F,0.0F},
                    new float[]{0.0F,1.0F,0.0F,0.0F,0.0F},
                    new float[]{0.0F,0.0F,1.0F,0.0F,0.0F},
                    new float[]{0.0F,0.0F,0.0F,context.Opacity,0.0F},//65透明度
                    new float[]{0.0F,0.0F,0.0F,0.0F,1.0F},
                }), ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
            g.DrawImage(waterMarkImage, new Rectangle((int)x, (int)y, (int)width, (int)height), 0, 0, width, height, GraphicsUnit.Pixel, imageAttributes);

            g.Dispose();
            if (context.Image == null) {
                waterMarkImage.Dispose();
            }
#pragma warning restore CA1416 // 验证平台兼容性
            return true;
        }
#endregion

#endregion
    }

}
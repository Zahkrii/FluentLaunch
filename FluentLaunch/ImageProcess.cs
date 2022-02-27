using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FluentLaunch
{
    internal class ImageProcess
    {
        public static BitmapSource GetIcon(string path, int sizeX, int sizeY)
        {
            //选中文件中的图标总数
            var iconTotalCount = PrivateExtractIcons(path, 0, 0, 0, null, null, 0, 0);

            //用于接收获取到的图标指针
            IntPtr[] hIcons = new IntPtr[iconTotalCount];
            //对应的图标id
            int[] ids = new int[iconTotalCount];
            //成功获取到的图标个数
            var successCount = PrivateExtractIcons(path, 0, sizeX, sizeY, hIcons, ids, iconTotalCount, 0);
            if (successCount > 0)
            {
                using (var ico = Icon.FromHandle(hIcons[0]))
                {
                    using (var myIcon = ico.ToBitmap())
                    {
                        return CreateBitmapSourceFromGdiBitmap(myIcon);
                    }
                }
            }
            else
                return null;
        }

        public static string SaveIcon(string path, int sizeX, int sizeY, string name)
        {
            //指定存放图标的文件夹
            string folderToSave = Directory.GetCurrentDirectory() + "\\Assets\\resource";
            if (!Directory.Exists(folderToSave)) Directory.CreateDirectory(folderToSave);

            //选中文件中的图标总数
            var iconTotalCount = PrivateExtractIcons(path, 0, 0, 0, null, null, 0, 0);
            //用于接收获取到的图标指针
            IntPtr[] hIcons = new IntPtr[iconTotalCount];
            //对应的图标id
            int[] ids = new int[iconTotalCount];
            //成功获取到的图标个数
            var successCount = PrivateExtractIcons(path, 0, sizeX, sizeY, hIcons, ids, iconTotalCount, 0);
            if (successCount > 0)
            {
                using (var ico = Icon.FromHandle(hIcons[0]))
                {
                    using (var myIcon = ico.ToBitmap())
                    {
                        if (!File.Exists(folderToSave + "\\" + name + ".png"))
                            myIcon.Save(folderToSave + "\\" + name + ".png", ImageFormat.Png);
                        return folderToSave + "\\" + name + ".png";
                    }
                }
            }
            else
                return path;
        }

        /// <summary>
        /// Creates an array of handles to icons that are extracted from a specified file.
        /// This function extracts from executable (.exe), DLL (.dll), icon (.ico), cursor (.cur), animated cursor (.ani), and bitmap (.bmp) files.
        /// Extractions from Windows 3.x 16-bit executables (.exe or .dll) are also supported.
        /// Details: https://msdn.microsoft.com/en-us/library/windows/desktop/ms648075(v=vs.85).aspx
        /// </summary>
        /// <param name="lpszFile">文件名(可以是exe,dll,ico,cur,ani,bmp)</param>
        /// <param name="nIconIndex">从第几个图标开始获取</param>
        /// <param name="cxIcon">获取图标的尺寸x</param>
        /// <param name="cyIcon">获取图标的尺寸y</param>
        /// <param name="phicon">获取到的图标指针数组</param>
        /// <param name="piconid">图标对应的资源编号</param>
        /// <param name="nIcons">指定获取的图标数量，仅当文件类型为.exe 和 .dll时候可用</param>
        /// <param name="flags">标志，默认0就可以，具体可以看LoadImage函数</param>
        /// <returns>选中文件中的图标总数</returns>
        [DllImport("User32.dll")]
        public static extern int PrivateExtractIcons(
            string lpszFile,
            int nIconIndex,
            int cxIcon,
            int cyIcon,
            IntPtr[] phicon,
            int[] piconid,
            int nIcons,
            int flags
        );

        /// <summary>
        /// 内存回收
        /// Destroys an icon and frees any memory the icon occupied.
        /// Details:https://msdn.microsoft.com/en-us/library/windows/desktop/ms648063(v=vs.85).aspx
        /// </summary>
        /// <param name="hIcon">A handle to the icon to be destroyed. The icon must not be in use.</param>
        /// <returns>销毁结果</returns>
        [DllImport("User32.dll")]
        public static extern bool DestroyIcon(IntPtr hIcon);

        public static BitmapSource CreateBitmapSourceFromGdiBitmap(Bitmap bitmap)
        {
            if (bitmap == null)
                throw new ArgumentNullException("bitmap");

            var rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);

            var bitmapData = bitmap.LockBits(
                rect,
                ImageLockMode.ReadWrite,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            try
            {
                var size = (rect.Width * rect.Height) * 4;

                return BitmapSource.Create(
                    bitmap.Width,
                    bitmap.Height,
                    bitmap.HorizontalResolution,
                    bitmap.VerticalResolution,
                    PixelFormats.Bgra32,
                    null,
                    bitmapData.Scan0,
                    size,
                    bitmapData.Stride);
            }
            finally
            {
                bitmap.UnlockBits(bitmapData);
            }
        }
    }
}
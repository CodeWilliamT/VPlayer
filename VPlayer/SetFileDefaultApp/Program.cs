using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SetFileDefaultApp
{
    internal class Program
    {
        static List<string> supportedFiles = new List<string>()
        { ".asf", ".avi", ".wm", ".wmp", ".wmv",
            ".ram",".rm",".rmvb",".rpm",".rt",".smil",".scm",".m1v",".m2v",".m2p",".m2ts",".mp2v",".mpe",".mpeg",".mpeg1",".mpeg2",".mpg",".mpv2",".pva",".tp",
            ".tpr",".ts",".m4b",".m4r",".m4p",".m4v",".mp4",".mpeg4",".3g2",".3gp",".3gp2",".3gpp",".mov",".qt",".flv",".f4v",".swf",".hlv",".vob",
            ".amv",".csf",".divx",".evo",".mkv",".mod",".pmp",".vp6",".bik",".mts",".xlmv",".ogm",".ogv",".ogx",
            ".aac",".ac3",".acc",".aiff",".ape",".au",".cda",".dts",".flac",".m1a",".m2a",".m4a",".mka",".mp2",/*".mp3",*/".mpa",".mpc",".ra",".tta",".wav",".wma",".wv",".mid",".midi",
            ".ogg",".oga",".dvd",".vqf",".srt", 
            ".ass", ".sub", ".vtt",".ram"
        };
        static void Main(string[] args)
        {
            string AppPath = @"C:\Users\v-wetong\source\repos\VPlayer\VPlayer\VPlayer\bin\Debug\VPlayer.exe";//args[0];
            FileInfo appInfo=new FileInfo(AppPath);
            string IcoPath = appInfo.DirectoryName + "\\AppIcon.ico";
            //MessageBox.Show(AppPath+"\n"+IcoPath);
            try
            {
                foreach(string ex in supportedFiles)
                {
                    SetFileDefaultApp(ex, AppPath, IcoPath);
                }
            }
            catch(Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
        }
        [System.Runtime.InteropServices.DllImport("Shell32.dll")]
        private static extern int SHChangeNotify(int eventId, int flags, IntPtr item1, IntPtr item2);
        /// <summary>
        /// 设置文件默认打开程序 前提是程序支持参数启动打开文件
        /// 特殊说明:txt后缀比较特殊,还需要从注册表修改userchoie的键值才行
        /// </summary>
        /// <param name="fileExtension">文件拓展名 示例:'.slnc'</param>
        /// <param name="appPath">默认程序绝对路径 示例:'c:\\test.exe'</param>
        /// <param name="fileIconPath">文件默认图标绝对路径 示例:'c:\\test.ico'</param>
        private static void SetFileDefaultApp(string fileExtension, string appPath, string fileIconPath=null)
        {
            //slnc示例 注册表中tree node path
            //|-.slnc				默认		"slncfile"
            //|--slncfile
            //|---DefaultIcon		默认		"fileIconPath"			默认图标
            //|----shell
            //|-----open
            //|------command		默认		"fileExtension \"%1\""	默认打开程序路径
            var fileExtensionKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(fileExtension);
            if (fileExtensionKey != null)
                Microsoft.Win32.Registry.ClassesRoot.DeleteSubKeyTree(fileExtension, false);
            fileExtensionKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Software\\Classes\\" + fileExtension);
            if (fileExtensionKey != null)
                Microsoft.Win32.Registry.CurrentUser.DeleteSubKeyTree("Software\\Classes\\" + fileExtension, false);
            string filetype = $"{fileExtension.Substring(1)}_file";

            Registry.SetValue("HKEY_CLASSES_ROOT\\" + fileExtension, "", filetype);
            Registry.SetValue("HKEY_CURRENT_USER\\Software\\Classes\\"+ fileExtension, "", filetype);
            Registry.SetValue("HKEY_CLASSES_ROOT\\" + filetype+ "\\shell\\open\\command", "", "\"" + appPath + "\" \"%1\"");
            Registry.SetValue("HKEY_CURRENT_USER\\Software\\Classes\\" + filetype + "\\shell\\open\\command", "", "\"" + appPath + "\" \"%1\"");
            if (fileIconPath != null)
            {
                Registry.SetValue("HKEY_CURRENT_USER\\Software\\Classes\\" + filetype + "\\DefaultIcon", "", "\"" + fileIconPath + "\"");
                Registry.SetValue("HKEY_CLASSES_ROOT\\" + filetype + "\\DefaultIcon", "", "\"" + fileIconPath + "\"");
            }
            //this call notifies Windows that it needs to redo the file associations and icons
            SHChangeNotify(0x08000000, 0x2000, IntPtr.Zero, IntPtr.Zero);

            //var fileExtensionKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(fileExtension);
            //if (fileExtensionKey != null)
            //    Microsoft.Win32.Registry.ClassesRoot.DeleteSubKeyTree(fileExtension, false);
            //fileExtensionKey = Microsoft.Win32.Registry.ClassesRoot.CreateSubKey(fileExtension);
            //using (fileExtensionKey)
            //{
            //    var fileKeyName = $"{fileExtension.Substring(1)}_file";
            //    fileExtensionKey.SetValue("", fileKeyName, Microsoft.Win32.RegistryValueKind.String);//Class- .xxx- default=xxxfile
            //    using (var fileKey = Microsoft.Win32.Registry.ClassesRoot.CreateSubKey(fileKeyName))
            //    {
            //        if (fileIconPath != null)
            //        {
            //            using (var defaultIcon = fileKey.CreateSubKey("DefaultIcon"))
            //            {
            //                defaultIcon.SetValue("", "\""+fileIconPath+"\"");//Class- .xxx- xxxfile- default=IconPath
            //            }
            //        }
            //        using (var command = fileKey.CreateSubKey(fileKeyName+"\\shell\\open\\command"))
            //        {
            //            command.SetValue("", "\"" + appPath + "\" \"%1\"");//Class- .xxx- xxxfile- shell- open- command
            //        }
            //    }
            //}
        }
    }
}

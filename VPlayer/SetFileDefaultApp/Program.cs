using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SetFileDefaultApp
{
    internal class Program
    {
        static List<string> Extensions = new List<string>()
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
            string AppPath ="";
            int i=0;
            foreach(string a in args)
            {
                if (i > 0) AppPath += " ";
                AppPath += a;
                i++;
            }
            try
            {
                foreach(string ex in Extensions)
                {
                    SetFileDefaultApp(ex.ToLower(), AppPath);
                }

                //this call notifies Windows that it needs to redo the file associations and icons
                SHChangeNotify(0x08000000, 0x2000, IntPtr.Zero, IntPtr.Zero);
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
        /// <param name="friendlyAppName">文件默认图标绝对路径 示例:'test'</param>
        /// <param name="fileIconPath">文件默认图标绝对路径 示例:'c:\\test.ico'</param>
        private static void SetFileDefaultApp(string fileExtension, string appPath, string fileIconPath = null)
        {
            //slnc示例 注册表中tree node path
            //|-.slnc				默认		"slncfile"
            //|--slncfile
            //|---DefaultIcon		默认		"fileIconPath"			默认图标
            //|----shell
            //|-----open
            //|------command		默认		"fileExtension \"%1\""	默认打开程序路径
            FileInfo appInfo = new FileInfo(appPath);
            string friendlyAppName = appInfo.Name.Substring(0, appInfo.Name.Length - appInfo.Extension.Length);
            var fileExtensionKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(fileExtension);
            if (fileExtensionKey != null)
                Registry.ClassesRoot.DeleteSubKeyTree(fileExtension, false);
            fileExtensionKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Software\\Classes\\" + fileExtension);
            if (fileExtensionKey != null)
                Registry.CurrentUser.DeleteSubKeyTree("Software\\Classes\\" + fileExtension, false);

            string fileType = fileExtension.Substring(1);
            string fileTypeNodeName = fileType + "_aoto_file";
            string systemClassName = "HKEY_CLASSES_ROOT\\";
            string currentUserClassName = "HKEY_CURRENT_USER\\Software\\Classes\\";
            string currentUserClassApplicationsAppName = currentUserClassName+ "Applications\\"+appInfo.Name+"\\";

            string ApplicationAssociationToastsName = "HKEY_CURRENT_USER\\Software\\Microsoft\\Windows\\CurrentVersion\\ApplicationAssociationToasts\\";
            string User_Explorer = "Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\FileExts\\"+fileExtension;

            string soc = "\\shell\\open\\command";


            //Set ApplicationAssociationToast Associate Node & Path
            Registry.SetValue(ApplicationAssociationToastsName, fileTypeNodeName + fileExtension, 0);
            Registry.SetValue(ApplicationAssociationToastsName, "Applications\\" + friendlyAppName + "_" + fileExtension, 0);
            //Set ExtensionNode with OpenPathNode
            Registry.SetValue(systemClassName + fileExtension, "", fileTypeNodeName);
            Registry.SetValue(currentUserClassName + fileExtension, "", fileTypeNodeName);
            //Set OpenPathNode with OpenAppPath
            Registry.SetValue(systemClassName + fileTypeNodeName + soc, "", "\"" + appPath + "\" \"%1\"");
            Registry.SetValue(currentUserClassName + fileTypeNodeName + soc, "", "\"" + appPath + "\" \"%1\"");
            Registry.SetValue(currentUserClassApplicationsAppName + soc, "", "\"" + appPath + "\" \"%1\"");

            //Set OpenAppPath.FriendlyAppName with FriendlyAppName
            Registry.SetValue(systemClassName +"Local Settings\\Software\\Microsoft\\Windows\\Shell\\MuiCache", appPath + ".FriendlyAppName", friendlyAppName);
            Registry.SetValue(currentUserClassName + "\\Local Settings\\Software\\Microsoft\\Windows\\Shell\\MuiCache", appPath + ".FriendlyAppName", friendlyAppName);


            Registry.SetValue("HKEY_CURRENT_USER\\"+User_Explorer + "\\OpenWithList","a", friendlyAppName);
            Registry.SetValue("HKEY_CURRENT_USER\\"+User_Explorer + "\\OpenWithProgids",fileTypeNodeName, "0");


            //using (RegistryKey User_ExplorerKey = Registry.CurrentUser.OpenSubKey(User_Explorer))
            //{
            //    using (RegistryKey User_Choice = User_ExplorerKey.OpenSubKey("UserChoice"))
            //    {
            //        if (User_Choice != null) 
            //            User_ExplorerKey.DeleteSubKey("UserChoice");
            //        Registry.SetValue("HKEY_CURRENT_USER\\" + User_Explorer + "UserChoice", "ProgId", "Applications\\" + friendlyAppName);
            //    }
            //}

            //Set Icon Path
            if (fileIconPath != null)
            {
                Registry.SetValue("HKEY_CURRENT_USER\\Software\\Classes\\" + fileTypeNodeName + "\\DefaultIcon", "", "\"" + fileIconPath + "\"");
                Registry.SetValue("HKEY_CLASSES_ROOT\\" + fileTypeNodeName + "\\DefaultIcon", "", "\"" + fileIconPath + "\"");
            }

        }

    }
}

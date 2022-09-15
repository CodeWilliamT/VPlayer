using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SetExtensionDefautApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string AppPath = args[0];
            for(int i = 1; i < args.Length; i++)
            {
                SetFileDefaultApp(args[i], AppPath);
            }
        }

        /// <summary>
        /// 设置文件默认打开程序 前提是程序支持参数启动打开文件
        /// 特殊说明:txt后缀比较特殊,还需要从注册表修改userchoie的键值才行
        /// </summary>
        /// <param name="fileExtension">文件拓展名 示例:'.slnc'</param>
        /// <param name="appPath">默认程序绝对路径 示例:'c:\\test.exe'</param>
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
            var fileExtensionKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(fileExtension);
            if (fileExtensionKey != null)
                Microsoft.Win32.Registry.ClassesRoot.DeleteSubKeyTree(fileExtension, false);
            fileExtensionKey = Microsoft.Win32.Registry.ClassesRoot.CreateSubKey(fileExtension);
            using (fileExtensionKey)
            {
                var fileKeyName = $"{fileExtension.Substring(1)}file";
                fileExtensionKey.SetValue("", fileKeyName, Microsoft.Win32.RegistryValueKind.String);
                using (var fileKey = fileExtensionKey.CreateSubKey(fileKeyName))
                {
                    if (fileIconPath != null)
                    {
                        using (var defaultIcon = fileKey.CreateSubKey("DefaultIcon"))
                        {
                            defaultIcon.SetValue("", fileIconPath);
                        }
                    }
                    using (var shell = fileKey.CreateSubKey("shell"))
                    {
                        using (var open = shell.CreateSubKey("open"))
                        {
                            using (var command = open.CreateSubKey("command"))
                            {
                                command.SetValue("", $"{appPath} \"%1\"");
                            }
                        }
                    }
                }
            }
        }
    }
}

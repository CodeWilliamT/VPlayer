using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace VPlayer
{
    public class Program
    {
        public static string configPath = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Vplayer.exe.Config";
        public static string[] startStr;

        /// <summary>
        /// Application Entry Point.
        /// </summary>
        [System.STAThreadAttribute()]
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public static void Main(string[] args)
        {
            RenderOptions.ProcessRenderMode = RenderMode.Default;
            if (args.Length >= 1)
            {
                startStr = args;
            }
            AppDomain.CurrentDomain.SetData("APP_CONFIG_FILE", configPath);
            VPlayer.App app = new VPlayer.App();
            app.InitializeComponent();
            app.Run();
        }
    }
}

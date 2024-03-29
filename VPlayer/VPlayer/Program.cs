﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace VPlayer
{
    public class Program
    {
        public static string[] startStr;

        /// <summary>
        /// Application Entry Point.
        /// </summary>
        [System.STAThreadAttribute()]
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public static void Main(string[] args)
        {
            if (args.Length >= 1)
            {
                startStr = args;
            }
            VPlayer.App app = new VPlayer.App();
            app.InitializeComponent();
            app.Run();
        }
    }
}

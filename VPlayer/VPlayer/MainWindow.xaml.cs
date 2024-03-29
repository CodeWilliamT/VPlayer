﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;
using System.IO;
using System.Windows.Threading;
using System.Runtime.InteropServices;
using Utils;
using System.Threading;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Xml.Linq;
using System.Diagnostics;
using System.Globalization;

namespace VPlayer
{
    public struct LogInfo
    {
        public string logStr;
        public DateTime disposeTime;
    }
    public class FileRecord
    {
        public int ID;
        public FileInfo FileInfo;
        public int Position;
        public MediaFileNode mediaFileNode;
        List<FileInfo> List_SubFiles;
        public FileRecord(int id, string filename, MediaFileNode node)
        {
            ID = id;
            FileInfo = new FileInfo(filename);
            Position = 0;
            mediaFileNode = node;
        }
    }
    public class MediaFileNode : TreeViewItemBase
    {
        public MediaFileNode()
        {
            this.Childs = new ObservableCollection<MediaFileNode>();
        }

        public string Name { get; set; }
        public string FullName { get; set; }
        public int Level { get; set; }

        public System.Windows.Controls.ContextMenu ContextMenu { get; set; }

        public ObservableCollection<MediaFileNode> Childs { get; set; }
    }

    public class TreeViewItemBase : INotifyPropertyChanged
    {
        private bool isSelected;
        public bool IsSelected
        {
            get { return this.isSelected; }
            set
            {
                if (value != this.isSelected)
                {
                    this.isSelected = value;
                    NotifyPropertyChanged("IsSelected");
                }
            }
        }
        private bool isExpanded;
        public bool IsExpanded
        {
            get { return this.isExpanded; }
            set
            {
                if (value != this.isExpanded)
                {
                    this.isExpanded = value;
                    NotifyPropertyChanged("IsExpanded");
                }
            }
        }
        private bool isUsing;
        public bool IsUsing
        {
            get { return this.isUsing; }
            set
            {
                if (value != this.isUsing)
                {
                    this.isUsing = value;
                    NotifyPropertyChanged("IsUsing");
                }
            }
        }
        private bool isRecorded;
        public bool IsRecorded
        {
            get { return this.isRecorded; }
            set
            {
                if (value != this.isRecorded)
                {
                    this.isRecorded = value;
                    NotifyPropertyChanged("IsRecorded");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }
    }

    public static class VideoImageBrushs
    {
        public static ImageBrush Close = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Images/Close.png")));
        public static ImageBrush Last = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Images/Last.png")));
        public static ImageBrush Max = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Images/Max.png")));
        public static ImageBrush Min = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Images/Min.png")));
        public static ImageBrush Next = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Images/Next.png")));
        public static ImageBrush NotPin = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Images/NotPin.png")));
        public static ImageBrush OpenFile = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Images/OpenFile.png")));
        public static ImageBrush Pause = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Images/Pause.png")));
        public static ImageBrush Pin = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Images/Pin.png")));
        public static ImageBrush Return = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Images/Return.png")));
        public static ImageBrush Screen = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Images/Screen.png")));
        public static ImageBrush Silence = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Images/Silence.png")));
        public static ImageBrush Sound = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Images/Sound.png")));
        public static ImageBrush Start = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Images/Start.png")));
        public static ImageBrush Stop = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Images/Stop.png")));
        public static ImageBrush Sub = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Images/Sub.png")));
        public static ImageBrush ToLeft = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Images/ToLeft.png")));
        public static ImageBrush ToRight = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Images/ToRight.png")));
    }
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : VcreditWindowBehindCode
    {
        enum PlayOverActions { PlayNext = 0, PlayThis = 1, DoNothing = 2 }

        const string title = "VPlayer By WilliamT";
        const string configPath = ".\\Vplayer.exe.Config";
        double minWidth = 630;
        double minHeight = 250;

        double preWidth = 630;
        double preHeight = 250;
        double viewPercent = 1;
        bool isWindowMax;
        bool mourseVisible = true;
        int mourseVisibleDelay, mourseVisibleMax = 6;
        DateTime time_LastMouseDown;
        POINT pi;
        RotateTransform viewRotateTransform;
        DispatcherTimer timer_Process;
        DispatcherTimer timer_Time;
        DispatcherTimer timer_Sub;
        Queue<LogInfo> Queue_LogInfo;
        List<FileInfo> List_SubFiles;
        Dictionary<string, FileRecord> Dic_MediaFiles;
        List<SubtitlesParser.Classes.SubtitleItem> subItems;
        List<string> supportedVideos = new List<string>()
        { ".asf", ".avi", ".wm", ".wmp", ".wmv",
            ".ram",".rm",".rmvb",".rpm",".rt",".smil",".scm",".m1v",".m2v",".m2p",".m2ts",".mp2v",".mpe",".mpeg",".mpeg1",".mpeg2",".mpg",".mpv2",".pva",".tp",
            ".tpr",".ts",".m4b",".m4r",".m4p",".m4v",".mp4",".mpeg4",".3g2",".3gp",".3gp2",".3gpp",".mov",".qt",".flv",".f4v",".swf",".hlv",".vob",
            ".amv",".csf",".divx",".evo",".mkv",".mod",".pmp",".vp6",".bik",".mts",".xlmv",".ogm",".ogv",".ogx",
            ".aac",".ac3",".acc",".aiff",".ape",".au",".cda",".dts",".flac",".m1a",".m2a",".m4a",".mka",".mp2",/*".mp3",*/".mpa",".mpc",".ra",".tta",".wav",".wma",".wv",".mid",".midi",
            ".ogg",".oga",".dvd",".vqf"};

        List<string> supportedSubs = new List<string>()
        { ".srt", ".ass", ".sub", ".vtt",".ram"
        };
        MSPlayer player;
        public string defaultDirctory;
        public List<string> List_Dirctory;
        public int defaultVoice = 50;
        public string nowFileName = "";
        public string nowSubName = "";
        public int nowPosition = 0;
        public Size defaultSize;
        public bool isListPinning;
        public int PlayOverActionMode = 0;
        public MainWindow()
        {
            InitializeComponent();
            SetupUI();
            pi = new POINT();
        }
        public double defaultWidth
        {
            get
            {
                return defaultSize.Width;
            }
            set
            {
                defaultSize = new Size(value, defaultHeight);
            }
        }
        public double defaultHeight
        {
            get
            {
                return defaultSize.Height;
            }
            set
            {
                defaultSize = new Size(defaultWidth, value);
            }
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;

            public POINT(int x, int y)
            {
                this.X = x;
                this.Y = y;
            }
        }
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern bool GetCursorPos(out POINT pt);
        public void SetupUI()
        {
            //异常处理
            System.Windows.Forms.Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            System.Windows.Forms.Application.ThreadException += Application_ThreadException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            preWidth = formWindow.Width;
            preHeight = formWindow.Height;
            Title = title;
            isWindowMax = false;
            mourseVisible = true;
            mourseVisibleDelay = 0;
            ShowList(false);
            ShowVoiceConfig(false);
            SetTop(false);
            time_LastMouseDown = DateTime.Now;
            defaultSize = new Size(Width, Height);
            btnOpenFolder.Visibility = Visibility.Hidden;
            Slider_Process.Visibility = Visibility.Hidden;
            btnLeft.Visibility = Visibility.Hidden;
            btnRight.Visibility = Visibility.Hidden;
            Label_Process.Visibility = Visibility.Hidden;
            timer_Process = new DispatcherTimer();
            timer_Process.Interval = new TimeSpan(0, 0, 0, 0, 500);
            timer_Process.Tick += timer_Process_tick;
            timer_Sub = new DispatcherTimer();
            timer_Sub.Interval = new TimeSpan(0, 0, 0, 0, 200);
            timer_Sub.Tick += timer_Sub_tick;
            timer_Time = new DispatcherTimer();
            timer_Time.Interval = new TimeSpan(0, 0, 0, 0, 500);
            timer_Time.Tick += timer_Time_tick;
            timer_Time.Start();
            btnLast.IsEnabled = false;
            btnNext.IsEnabled = false;
            btnStop.IsEnabled = false;
            AppConfigHelper.LoadObj(this);
            if (defaultVoice == 0)
            {
                defaultVoice = 50;
                Slider_Voice.Value = 50;
            }
            defaultDirctory = defaultDirctory.Replace("\0", "");
            Queue_LogInfo = new Queue<LogInfo>();
            List_SubFiles = new List<FileInfo>();
            Dic_MediaFiles = new Dictionary<string, FileRecord>();
            //viewRotateTransform = new RotateTransform();
            //player.LayoutTransform= viewRotateTransform;
            RefreshFileTree();
            (menuPlayOverActions.Items[PlayOverActionMode] as System.Windows.Controls.MenuItem).IsChecked = true;
            PinList(isListPinning);
            player = new MSPlayer(PlayerElement);
        }

        #region UI Functions
        static DependencyObject VisualUpwardSearch<T>(DependencyObject source)
        {
            while (source != null && source.GetType() != typeof(T))
                source = VisualTreeHelper.GetParent(source);

            return source;
        }

        /// <summary>
        /// 删除文件夹节点下所有文件的播放记录
        /// </summary>
        /// <param name="node"></param>
        private void DeleteDirNodeRecord(MediaFileNode node)
        {
            try
            {
                foreach (MediaFileNode child in node.Childs)
                {
                    DeleteFileNodeRecord(child);
                }
            }
            catch
            {
                return;
            }
        }


        /// <summary>
        /// 删除文件节点的播放记录
        /// </summary>
        /// <param name="node"></param>
        private void DeleteFileNodeRecord(MediaFileNode node)
        {
            try
            {
                RemoveRecord(node.FullName);
                node.IsRecorded = false;
            }
            catch
            {
                return;
            }
        }

        private void AddContextMenuToDirNode(MediaFileNode customNode)
        {

            customNode.ContextMenu = new System.Windows.Controls.ContextMenu();
            System.Windows.Controls.MenuItem menuDeleteDirNodeRecord = new System.Windows.Controls.MenuItem();
            menuDeleteDirNodeRecord.Header = "删除目录记录";
            menuDeleteDirNodeRecord.Click += menuDeleteNodeRecord_Click;
            System.Windows.Controls.MenuItem menuOpenDirNodeFolder = new System.Windows.Controls.MenuItem();
            menuOpenDirNodeFolder.Header = "打开文件夹";
            menuOpenDirNodeFolder.Click += menuOpenNodeFolder_Click;

            System.Windows.Controls.MenuItem menuClearDirList = new System.Windows.Controls.MenuItem();
            menuClearDirList.Header = "清空播放记录";
            menuClearDirList.Click += menuClearList_Click;


            customNode.ContextMenu.Items.Add(menuDeleteDirNodeRecord);
            customNode.ContextMenu.Items.Add(menuOpenDirNodeFolder);
            customNode.ContextMenu.Items.Add(menuClearDirList);

        }

        private void AddContextMenuToFileNode(MediaFileNode customNode)
        {
            customNode.ContextMenu = new System.Windows.Controls.ContextMenu();
            System.Windows.Controls.MenuItem menuDeleteFileNodeRecord = new System.Windows.Controls.MenuItem();
            menuDeleteFileNodeRecord.Header = "删除播放记录";
            menuDeleteFileNodeRecord.Click += menuDeleteNodeRecord_Click;
            System.Windows.Controls.MenuItem menuOpenFileNodeFolder = new System.Windows.Controls.MenuItem();
            menuOpenFileNodeFolder.Header = "打开文件目录";
            menuOpenFileNodeFolder.Click += menuOpenNodeFolder_Click;

            System.Windows.Controls.MenuItem menuFileClearList = new System.Windows.Controls.MenuItem();
            menuFileClearList.Header = "清空播放记录";
            menuFileClearList.Click += menuClearList_Click;
            customNode.ContextMenu.Items.Add(menuDeleteFileNodeRecord);
            customNode.ContextMenu.Items.Add(menuOpenFileNodeFolder);
            customNode.ContextMenu.Items.Add(menuFileClearList);

        }

        /// <summary>
        /// 加文件夹节点到文件列表
        /// </summary>
        /// <param name="DirctoryName"></param>
        private void AddDirectoryToListFile(string DirctoryName)
        {
            try
            {
                if (List_Dirctory.Contains(DirctoryName)) return;
                MediaFileNode node = AddDirectoryToTreeView(DirctoryName);
                node.IsSelected = true;
                node.IsExpanded = true;
                List_Dirctory.Add(DirctoryName);
            }
            catch
            {
                return;
            }
        }

        /// <summary>
        /// 加文件夹节点到文件列表
        /// </summary>
        /// <param name="DirctoryName"></param>
        private MediaFileNode AddDirectoryToTreeView(string DirctoryName)
        {
            try
            {
                DirectoryInfo path = new DirectoryInfo(DirctoryName);
                FileInfo[] Dir = path.GetFiles("*", SearchOption.AllDirectories);
                MediaFileNode dirNode = new MediaFileNode() { Name = path.Name, FullName = path.FullName, Level = 0 };
                AddContextMenuToDirNode(dirNode);
                int i = Dic_MediaFiles.Count;
                foreach (FileInfo d in Dir)
                {
                    if (supportedVideos.Contains(d.Extension.ToLower()))
                    {
                        MediaFileNode fileNode = new MediaFileNode()
                        {
                            Name = d.FullName.Remove(0, path.FullName.Length + 1),
                            FullName = d.FullName,
                            Level = 1
                        };
                        AddContextMenuToFileNode(fileNode);
                        int record = LoadRecord(d.FullName);
                        fileNode.IsRecorded = record > 0;
                        dirNode.Childs.Add(fileNode);
                        Dic_MediaFiles[d.FullName]=new FileRecord(Dic_MediaFiles.Count, d.FullName,fileNode);
                        i++;
                    }
                }
                TreeView_File.Items.Add(dirNode);
                return dirNode;
            }
            catch
            {
                return null;
            }
        }



        /// <summary>
        /// 刷新文件列表树
        /// </summary>
        private void RefreshFileTree()
        {
            try
            {
                TreeView_File.Items.Clear();
                List_SubFiles.Clear();
                Dic_MediaFiles.Clear();
                MediaFileNode node;
                foreach (var d in List_Dirctory)
                {
                    if (!Directory.Exists(d))
                    {
                        continue;
                    }
                    node = AddDirectoryToTreeView(d);
                    node.IsExpanded = true;
                }
            }
            catch
            {
                return;
            }
        }

        /// <summary>
        /// 打印Log信息
        /// </summary>
        /// <param name="str"></param>
        private void LogInfo(string str)
        {
            if (Queue_LogInfo.Count >= 2)
            {
                textBlock_Log.Text = textBlock_Log.Text.Replace(Queue_LogInfo.Peek().logStr, "");
                Queue_LogInfo.Dequeue();
            }
            Queue_LogInfo.Enqueue(new LogInfo() { logStr = str+"\n", disposeTime = DateTime.Now.AddSeconds(4) });
            textBlock_Log.Text = textBlock_Log.Text + Queue_LogInfo.Last().logStr;
        }
        /// <summary>
        /// 播放媒体文件
        /// </summary>
        /// <param name="fileName"></param>
        private void OpenMediaFile(string fileName)
        {
            try
            {
                if (fileName == null || fileName == "") return;
                if (!File.Exists(fileName)) return;
                if (player.IsPlaying)
                {
                    if (player.Source != null)
                    {
                        Dic_MediaFiles[nowFileName].mediaFileNode.IsUsing = false;
                        Dic_MediaFiles[nowFileName].mediaFileNode.IsRecorded = true;
                        if (player.NaturalDuration.HasTimeSpan)
                        {
                            SaveRecord();
                            if (subItems != null && subItems.Count() > 0)
                                AppConfigHelper.SaveKey("FileRecordSub-" + nowFileName, nowSubName);
                        }
                    }
                    Slider_Process.Value = 0;
                    SetVideoMode(false);
                }
                PlayRecord(fileName);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.ToString());
            }
        }


        private void OpenFolder(string folderName)
        {
            try
            {
                DirectoryInfo Path = new DirectoryInfo(folderName);
                FileInfo[] Dir = Path.GetFiles("*", SearchOption.AllDirectories);

                List<FileInfo> List_MediaFilesTmp = new List<FileInfo>();
                List_SubFiles.Clear();

                foreach (FileInfo d in Dir)
                {
                    if (supportedVideos.Contains(d.Extension.ToLower()))
                    {
                        List_MediaFilesTmp.Add(d);
                    }
                    if (supportedSubs.Contains(d.Extension.ToLower()))
                    {
                        List_SubFiles.Add(d);
                    }
                }
                if (List_MediaFilesTmp.Count == 0)
                {
                    if (List_SubFiles.Count > 0)
                        OpenSubFile(List_SubFiles.First().FullName);
                }
                else
                {
                    int idx =Dic_MediaFiles.Count;
                    AddDirectoryToListFile(folderName);
                    if (Dic_MediaFiles.Count > idx)
                        OpenMediaFile(Dic_MediaFiles.ElementAt(idx).Key);
                }
            }
            catch
            {
                return;
            }
        }
        /// <summary>
        /// 载入字幕文件
        /// </summary>
        /// <param name="fileName"></param>
        private bool OpenSubFile(string fileName)
        {
            if (player.Source == null) return false;
            if (fileName == null || fileName == "") return false;
            if (!File.Exists(fileName)) return false;
            nowSubName = fileName;
            FileInfo fileInfo = new FileInfo(fileName);
            if (!supportedSubs.Contains(fileInfo.Extension.ToLower()))
            {
                return false;
            }
            try
            {
                if (subItems != null)
                    subItems.Clear();
                var subParse = new SubtitlesParser.Classes.Parsers.SubParser();
                using (StreamReader sr = new StreamReader(fileName))
                {
                    subItems = subParse.ParseStream(sr.BaseStream);
                }
                LogInfo("已加载字幕" + fileInfo.Name);
                return true;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.ToString());
            }
            return false;
        }
        /// <summary>
        /// 使得容器控件悬浮可见或长期可见
        /// </summary>
        /// <param name="panel">容器</param>
        /// <param name="flag">true移动至容器表面可见，false恢复至长期可见</param>
        private void SetPanelMotionVisible(System.Windows.Controls.Panel panel, bool flag)
        {
            if (flag)
            {
                panel.MouseEnter += Menu_MouseEnter;
                panel.MouseLeave += Menu_MouseLeave;
            }
            else
            {
                panel.MouseEnter -= Menu_MouseEnter;
                panel.MouseLeave -= Menu_MouseLeave;
            }
        }


        /// <summary>
        /// 独立挂起显示文件列表
        /// </summary>
        /// <param name="flag"></param>
        private void PinList(bool flag)
        {
            //if (isListPinning == flag) return;
            if (flag)
            {
                ShowList(true);
                isListPinning = true;
                btnPinList.Background = VideoImageBrushs.Pin;
                TreeView_File_SizeChanged(null, null);

                TreeView_File.Margin = new Thickness(TreeView_File.Margin.Left, -Grid_Center.Margin.Top, TreeView_File.Margin.Right, -Grid_Center.Margin.Bottom);
                GridSplitter_List.Margin = new Thickness(GridSplitter_List.Margin.Left, -Grid_Center.Margin.Top, GridSplitter_List.Margin.Right, -Grid_Center.Margin.Bottom);

                ShowList(true);
            }
            else
            {
                isListPinning = false;
                btnPinList.Background = VideoImageBrushs.NotPin;
                var binding = new System.Windows.Data.Binding("ActualWidth") { Source = this.Grid_Form };
                this.Grid_Main.SetBinding(Grid.WidthProperty, binding);

                if (player!=null&&player.Source != null)
                {
                    TreeView_File.Margin = new Thickness(TreeView_File.Margin.Left, -Grid_Center.Margin.Top, TreeView_File.Margin.Right, -Grid_Center.Margin.Bottom);
                    GridSplitter_List.Margin = new Thickness(GridSplitter_List.Margin.Left, -Grid_Center.Margin.Top, GridSplitter_List.Margin.Right, -Grid_Center.Margin.Bottom);
                }
                else
                {
                    TreeView_File.Margin = new Thickness(TreeView_File.Margin.Left, 0, TreeView_File.Margin.Right, 0);
                    GridSplitter_List.Margin = new Thickness(GridSplitter_List.Margin.Left, 0, GridSplitter_List.Margin.Right, 0);
                }
            }
        }


        /// <summary>
        /// 显示文件列表
        /// </summary>
        /// <param name="flag"></param>
        private void ShowList(bool flag)
        {
            if (flag)
            {
                btnPinList.Visibility = Visibility.Visible;
                TreeView_File.Visibility = Visibility.Visible;
                GridSplitter_List.Visibility = Visibility.Visible;

                btnShowList.Margin = new Thickness(0, 0, 0, 0);
                btnShowList.Content = ">";

                btnShowList.Opacity = 1;
                btnShowList.MouseEnter -= Control_MouseEnter;
                btnShowList.MouseLeave -= Control_MouseLeave;
                Grid_Menu.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                Grid_Menu.Width = formWindow.Width - (Grid_Center.ColumnDefinitions[1].Width.Value + Grid_Center.ColumnDefinitions[2].Width.Value + 12);
                Grid_Top.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                Grid_Top.Width = formWindow.Width - (Grid_Center.ColumnDefinitions[1].Width.Value + Grid_Center.ColumnDefinitions[2].Width.Value + 12);
                if (isListPinning)
                {
                    TreeView_File_SizeChanged(null, null);
                    Canvas_Top.Visibility = Visibility.Visible;
                    Grid_Menu.Visibility = Visibility.Visible;
                }
            }
            else
            {
                if (isListPinning)
                {
                    var binding = new System.Windows.Data.Binding("ActualWidth") { Source = this.Grid_Form };
                    this.Grid_Main.SetBinding(Grid.WidthProperty, binding);
                }
                if (player!=null&&player.Source!=null)
                {
                    btnShowList.Opacity = 0;
                    btnShowList.MouseEnter += Control_MouseEnter;
                    btnShowList.MouseLeave += Control_MouseLeave;
                    Canvas_Top.Visibility = Visibility.Visible;
                    Grid_Menu.Visibility = Visibility.Visible;
                }
                Grid_Menu.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
                Grid_Top.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
                Grid_Menu.Width = System.Double.NaN;
                Grid_Top.Width = System.Double.NaN;
                btnPinList.Visibility = Visibility.Hidden;
                TreeView_File.Visibility = Visibility.Hidden;
                GridSplitter_List.Visibility = Visibility.Hidden;
                btnShowList.Margin = new Thickness(0, 0, -(Grid_Center.ColumnDefinitions[1].Width.Value + Grid_Center.ColumnDefinitions[2].Width.Value), 0);

                btnShowList.Background = null;
                btnShowList.Content = "<";
            }
        }

        /// <summary>
        /// 显示音量控制器
        /// </summary>
        /// <param name="flag"></param>
        private void ShowVoiceConfig(bool flag)
        {

            if (flag)
            {
                Canvas_Voice.Visibility = Visibility.Visible;
                btnVoice.Tag = true;
            }
            else
            {
                Canvas_Voice.Visibility = Visibility.Hidden;
                btnVoice.Tag = false;
            }
        }
        private void SaveRecord()
        {
            if (player.Source != null)
                if (player.NaturalDuration.HasTimeSpan)
                {
                    AppConfigHelper.SaveKey("FileRecord-" + nowFileName, player.Position.TotalMilliseconds.ToString());
                    if (subItems != null && subItems.Count() > 0) AppConfigHelper.SaveKey("FileRecordSub-" + nowFileName, nowSubName);

                }
        }
        private int LoadRecord(string fileName)
        {
            return (int)AppConfigHelper.LoadDouble("FileRecord-" + fileName);
        }
        private void RemoveRecord(string fileName)
        {
            AppConfigHelper.RemoveKey("FileRecord-" + fileName);
        }
        private void PlayRecord(string fileName)
        {
            FileInfo fileInfo = new FileInfo(fileName);
            //判断后缀是否支持
            if (!supportedVideos.Contains(fileInfo.Extension.ToLower()))
            {
                return;
            }
            //更新媒体目录
            if (!Dic_MediaFiles.ContainsKey(fileInfo.FullName))
            {
                AddDirectoryToListFile(fileInfo.DirectoryName);
            }

            nowFileName = fileInfo.FullName;
            player.Open(fileName);
            Slider_Voice.Value = defaultVoice;
            player.Volume = 1.0 * defaultVoice / Slider_Voice.Maximum;
            label_NowFile.Content = fileInfo.Name;
            Dic_MediaFiles[nowFileName].mediaFileNode.IsUsing = true;

            SetVideoMode(true);
            DateTime t = DateTime.Now;


            while (!player.NaturalDuration.HasTimeSpan)//等待媒体开始播放
            {
                System.Threading.Thread.Sleep(50);
                if (DateTime.Now.Subtract(t).TotalMilliseconds > 5000)
                {
                    btnStop_Click(null, null);
                    SetVideoMode(false);
                    if (System.Windows.MessageBox.Show(
                "文件损坏，或者不支持此种视频格式。\n可能是未安装HEVC解码器，是否从微软商场安装？", "打开失败", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                        {
                            string url = "ms-windows-store://pdp/?ProductId=9n4wgh0z6vhq";
                            Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
                        }
                    }
                    return;
                }
            }

            nowPosition = LoadRecord(nowFileName);
            if (nowPosition + 1000 < player.NaturalDuration.TimeSpan.TotalMilliseconds)
                player.Position = new TimeSpan(0, 0, 0, 0, nowPosition);

            if (subItems != null)
            {
                subItems.Clear();
                tbSub.Text = "";
            }
            string subfile = AppConfigHelper.LoadKey("FileRecordSub-" + nowFileName);
            if (subfile != "")
            {
                if (OpenSubFile(subfile))
                    goto _end;
            }
            string fileNoExPath = fileInfo.Name.Remove(fileInfo.Name.Length - fileInfo.Extension.Length, fileInfo.Extension.Length);
            var Trans = new[] { "_Trans", "" };
            var mids = new[] { "", "_Subtitles01", "_Subtitles02" };
            foreach (var trans in Trans)
            {
                for (int i= 0;i<10;i++)
                {
                    string mid = i == 0 ? "" : ("_Subtitles0" + i);
                    foreach (var subEx in supportedSubs)
                    {
                        if (OpenSubFile(fileInfo.DirectoryName + "\\" + fileNoExPath + mid + trans + subEx))
                            goto _end;
                        if (OpenSubFile(fileInfo.DirectoryName + "\\" + "Subs" + trans + "\\" + fileNoExPath + mid + subEx))
                            goto _end;
                        if (OpenSubFile(fileInfo.DirectoryName + "\\" + "Sub" + trans + "\\" + fileNoExPath + mid + subEx))
                            goto _end;
                    }
                }
            }

        _end:
            //UI
            Slider_Process.Maximum = (int)player.NaturalDuration.TimeSpan.TotalSeconds;
            Slider_Process.Visibility = Visibility.Visible;
            btnLeft.Visibility = Visibility.Visible;
            btnRight.Visibility = Visibility.Visible;
            Label_Process.Visibility = Visibility.Visible;
            TreeView_File.Margin = new Thickness(TreeView_File.Margin.Left, -Grid_Center.Margin.Top, TreeView_File.Margin.Right, -Grid_Center.Margin.Bottom);
            GridSplitter_List.Margin = new Thickness(GridSplitter_List.Margin.Left, -Grid_Center.Margin.Top, GridSplitter_List.Margin.Right, -Grid_Center.Margin.Bottom);
            SetPanelMotionVisible(Grid_Top, true);
            SetPanelMotionVisible(Grid_Menu, true);
            Canvas_File.Visibility = Visibility.Hidden;
            btnStop.IsEnabled = true;
            Grid_Main.Focus();
            if (!isListPinning && btnShowList.Content.ToString() == ">")
                ShowList(true);

            if (Dic_MediaFiles[nowFileName].ID != 0)
                btnLast.IsEnabled = true;
            if (Dic_MediaFiles[nowFileName].ID != Dic_MediaFiles.Count - 1)
                btnNext.IsEnabled = true;
        }
        private void SetVideoMax(bool flag)
        {
            ControlTemplate customWindowTemplate = App.Current.Resources["CustomWindowTemplete"] as ControlTemplate;
            var grid = customWindowTemplate.FindName("ResizingGrid", this) as Grid;
            if (flag)
            {
                grid.RowDefinitions[0].Height = new GridLength(0);
                grid.RowDefinitions[2].Height = new GridLength(0);
                grid.ColumnDefinitions[0].Width = new GridLength(0);
                grid.ColumnDefinitions[2].Width = new GridLength(0);
                WindowState = WindowState.Maximized;
                btnScreen.Background = VideoImageBrushs.Return;
                menuScreen.Header = "退出全屏";
            }
            else
            {
                grid.RowDefinitions[0].Height = new GridLength(1);
                grid.RowDefinitions[2].Height = new GridLength(1);
                grid.ColumnDefinitions[0].Width = new GridLength(1);
                grid.ColumnDefinitions[2].Width = new GridLength(1);
                WindowState = WindowState.Normal;
                btnScreen.Background = VideoImageBrushs.Screen;
                menuScreen.Header = "全屏";
            }
        }
        private void SetWindowMax()
        {
            ControlTemplate customWindowTemplate = App.Current.Resources["CustomWindowTemplete"] as ControlTemplate;
            var grid = customWindowTemplate.FindName("ResizingGrid", this) as Grid;
            Rect rect = SystemParameters.WorkArea;
            if (!isWindowMax)
            {
                btnMax.Background = VideoImageBrushs.Return;
                grid.RowDefinitions[0].Height = new GridLength(0);
                grid.RowDefinitions[2].Height = new GridLength(0);
                grid.ColumnDefinitions[0].Width = new GridLength(0);
                grid.ColumnDefinitions[2].Width = new GridLength(0);
                WindowState = WindowState.Normal;
                this.Width = rect.Width + 22;
                this.Height = rect.Height + 22;
                this.Top = -11;
                this.Left = -11;
            }
            else
            {
                btnMax.Background = VideoImageBrushs.Max;
                grid.RowDefinitions[0].Height = new GridLength(1);
                grid.RowDefinitions[2].Height = new GridLength(1);
                grid.ColumnDefinitions[0].Width = new GridLength(1);
                grid.ColumnDefinitions[2].Width = new GridLength(1);
                WindowState = WindowState.Normal;
                this.Width = defaultWidth;
                this.Height = defaultHeight;
                this.Top = (rect.Height - defaultHeight) / 2;
                this.Left = (rect.Width - defaultWidth) / 2;

            }
            btnScreen.Background = VideoImageBrushs.Screen;
            isWindowMax = !isWindowMax;
        }

        private void Grid_Player_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            player.Width = Grid_Player.ActualWidth * viewPercent;
            player.Height = Grid_Player.ActualHeight * viewPercent;
        }





        /// <summary>
        /// 配置是否显示最前
        /// </summary>
        /// <param name="flag"></param>
        private void SetTop(bool flag)
        {

            Topmost = flag;
            menuTop.IsChecked = flag;
            if (flag)
            {
                btnTop.Background = VideoImageBrushs.Pin;
            }
            else
            {
                btnTop.Background = VideoImageBrushs.NotPin;
            }
        }
        /// <summary>
        /// 使得视频播放或暂停
        /// </summary>
        /// <param name="flag"></param>
        private void SetVideoMode(bool flag)
        {
            if (player.Source == null) return;
            player.IsPlaying = flag;
            if (flag)
            {
                timer_Process.Start();
                timer_Sub.Start();
                btnStart.Background = VideoImageBrushs.Pause;
            }
            else
            {
                timer_Process.Stop();
                timer_Sub.Stop();
                btnStart.Background = VideoImageBrushs.Start;
            }
        }


        #endregion
        /// <summary>
        /// 异常处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            var ex = e.Exception;
            if (ex != null)
            {
                System.Windows.MessageBox.Show(ex.ToString());
            }
        }
        /// <summary>
        /// 异常处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = (Exception)e.ExceptionObject;
            if (ex != null)
            {
                System.Windows.MessageBox.Show(ex.ToString());
            }

        }

        private void timer_Process_tick(object sender, EventArgs e)
        {
            if (player.Source == null) return;
            Slider_Process.Value = (int)player.Position.TotalSeconds;
            if (player.NaturalDuration.HasTimeSpan)
                Label_Process.Content = player.Position.ToString("hh\\:mm\\:ss") + "/" + player.NaturalDuration.TimeSpan.ToString("hh\\:mm\\:ss");

        }

        private void timer_Sub_tick(object sender, EventArgs e)
        {
            if (player.Source == null) return;
            if (subItems == null || subItems.Count() == 0) return;
            double msec = player.Position.TotalMilliseconds;
            StringBuilder s = new StringBuilder();
            foreach (var subItem in subItems)
            {
                if (msec > subItem.StartTime && msec < subItem.EndTime)
                {
                    s.Append(subItem.PlaintextLines[0]);
                    for (int i = 1; i < subItem.PlaintextLines.Count(); i++)
                    {
                        s.Append("\n" + subItem.PlaintextLines[i]);
                    }
                }
            }
            tbSub.Text = s.ToString();
        }
        private void timer_Time_tick(object sender, EventArgs e)
        {
            label_time.Content = DateTime.Now.ToString("HH:mm:ss");
            if (Queue_LogInfo.Count>0&& Queue_LogInfo.Peek().disposeTime< DateTime.Now)
            {
                textBlock_Log.Text = textBlock_Log.Text.Replace(Queue_LogInfo.Peek().logStr,"");
                Queue_LogInfo.Dequeue();
            }
            if (Grid_CenterLeft.IsMouseOver&& mourseVisible)
            {
                if (mourseVisibleDelay < mourseVisibleMax)
                    mourseVisibleDelay++;
                else
                {
                    ShowCursor(0);
                    mourseVisible = false;
                }
            }
        }
        #region 隐藏鼠标的方法 0/1 隐藏/显示
        [DllImport("user32.dll", EntryPoint = "ShowCursor", CharSet = CharSet.Auto)]
        public static extern void ShowCursor(int status);
        #endregion
        private void btnDown_Click(object sender, RoutedEventArgs e)
        {
            if (btnOpenFolder.Visibility == Visibility.Hidden)
            {
                btnOpenFolder.Visibility = Visibility.Visible;
            }
            else
            {
                btnOpenFolder.Visibility = Visibility.Hidden;
            }
        }

        private void formWindow_ContentRendered(object sender, EventArgs e)
        {
            if (Program.startStr != null)
                if (Program.startStr.Length > 0)
                {
                    OpenMediaFile(Program.startStr[0]);
                }
        }
        private void btnOpenFile_Click(object sender, RoutedEventArgs e)
        {
            timer_Time.Stop();
            OpenFileDialog ofd = new OpenFileDialog();
            if (Directory.Exists(defaultDirctory)) ofd.InitialDirectory = defaultDirctory;
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                OpenMediaFile(ofd.FileName);
            }
            timer_Time.Start();
        }
        private void btnOpenFolder_Click(object sender, RoutedEventArgs e)
        {
            timer_Time.Stop();
            FolderBrowserDialog Fbd = new FolderBrowserDialog();
            if (Directory.Exists(defaultDirctory)) Fbd.SelectedPath = defaultDirctory;
            if (Fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                OpenFolder(Fbd.SelectedPath);
            }
            timer_Time.Start();
        }

        private void btnSubOpen_Click(object sender, RoutedEventArgs e)
        {
            timer_Time.Stop();
            Canvas_Sub.Visibility = Visibility.Hidden;
            OpenFileDialog ofd = new OpenFileDialog();
            if (Directory.Exists(defaultDirctory)) ofd.InitialDirectory = defaultDirctory;

            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                OpenSubFile(ofd.FileName);
            }
            timer_Time.Start();
        }
        private void btnSubVisible_Click(object sender, RoutedEventArgs e)
        {
            if (tbSub.Visibility == Visibility.Visible)
            {
                LogInfo("已隐藏字幕");
                tbSub.Text = "显示字幕";
                menuSubVisible.IsChecked = true;
                tbSub.Visibility = Visibility.Hidden;
            }
            else
            {
                LogInfo("已隐藏字幕");
                tbSub.Text = "隐藏字幕";
                menuSubVisible.IsChecked = false;
                tbSub.Visibility = Visibility.Visible;
            }
            Canvas_Sub.Visibility = Visibility.Hidden;
        }

        private void menuOpenDir_Click(object sender, RoutedEventArgs e)
        {
            string startargs;
            FileInfo fi = new FileInfo(nowFileName);
            startargs = fi.DirectoryName;
            Process.Start(@"Explorer.exe", startargs);
        }

        private void menuPlayOverActions_Items_Checked(object sender, RoutedEventArgs e)
        {
            var current = sender as System.Windows.Controls.MenuItem;
            PlayOverActionMode = (int)(Enum.Parse(typeof(PlayOverActions), current.Tag.ToString()));
            foreach (var m in menuPlayOverActions.Items)
            {
                var menucheck = m as System.Windows.Controls.MenuItem;
                if (current.Name == menucheck.Name) continue;
                menucheck.IsChecked = false;
            }
        }

        private void Control_MouseEnter(object sender, EventArgs e)
        {
            ((System.Windows.Controls.Control)sender).Opacity = 1;

        }

        private void Control_MouseLeave(object sender, EventArgs e)
        {
            ((System.Windows.Controls.Control)sender).Opacity = 0;

        }

        private void Menu_MouseEnter(object sender, EventArgs e)
        {
            Canvas_Top.Opacity = 1;
            Grid_Menu.Opacity = 1;
            btnShowList.Opacity = 1;
            //panel.IsEnabled = true;

        }

        private void Menu_MouseLeave(object sender, EventArgs e)
        {
            Canvas_Top.Opacity = 0;
            Grid_Menu.Opacity = 0;
            if (TreeView_File.Visibility == Visibility.Hidden)
            {
                btnShowList.Opacity = 0;
            }
            Task.Run(new Action(() =>
            {
                Thread.Sleep(5000);
                Dispatcher.Invoke(() =>
                {
                    Canvas_Voice.Visibility = Visibility.Hidden;
                    Canvas_Sub.Visibility = Visibility.Hidden;
                });
            }));
            //panel.IsEnabled = false;

        }


        private void btnPinList_Click(object sender, RoutedEventArgs e)
        {
            PinList(!isListPinning);
        }


        private void btnShowList_Click(object sender, RoutedEventArgs e)
        {
            ShowList(btnShowList.Content.ToString() == "<");
        }


        private void btnVoice_Click(object sender, RoutedEventArgs e)
        {
            ShowVoiceConfig(!((bool)btnVoice.Tag));
        }

        private void btnMin_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void btnMax_Click(object sender, RoutedEventArgs e)
        {
            SetWindowMax();
        }
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            formWindow.Close();
        }

        private void btnTop_Click(object sender, RoutedEventArgs e)
        {
            SetTop(!Topmost);
        }


        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            if (player.Source == null)
            {
                if (Dic_MediaFiles == null) return;
                if (Dic_MediaFiles.Count == 0) return;
                OpenMediaFile(nowFileName);
                return;
            }
            SetVideoMode(!player.IsPlaying);
        }

        private void Player_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (player.Source == null) return;
            btnStart_Click(null, null);
        }


        private void Slider_Process_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            //拖动的时候
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (player.Source == null) return;
                timer_Process.Stop();
            }
        }
        private void Slider_Process_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (player.Source == null) return;
            timer_Process.Stop();
            player.Position = TimeSpan.FromSeconds(Slider_Process.Value);
            timer_Process.Start();
            LogInfo("跳至  " + player.Position.ToString("hh\\:mm\\:ss") + "(" + (Slider_Process.Value / Slider_Process.Maximum).ToString("P2") + ")");
            //SetVideoMode(true);
        }
        private void Slider_Vioce_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (player == null) return;
            if (player.Source == null) return;
            player.Volume = 1.0 * Slider_Voice.Value / Slider_Voice.Maximum;
            if (Slider_Voice.Value > 0) defaultVoice = (int)Slider_Voice.Value;
            LogInfo("音量:" + player.Volume.ToString("P0"));
        }



        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            if (player == null) return;
            if (player.Source == null) return;
            tbSub.Text = "";
            SetVideoMode(false);
            SaveRecord();
            player.Stop();
            player.Source = null;
            if (subItems != null)
            {
                subItems.Clear();
                tbSub.Text = "";
            }
            Label_Process.Content = "--:--:--/ --:--:--";
            label_NowFile.Content = "VPlayer";
            Slider_Process.Value = 0;
            player.Close();
            Slider_Process.Visibility = Visibility.Hidden;
            btnLeft.Visibility = Visibility.Hidden;
            btnRight.Visibility = Visibility.Hidden;
            Label_Process.Visibility = Visibility.Hidden;

            btnShowList.Opacity = 1;
            btnShowList.MouseEnter -= Control_MouseEnter;
            btnShowList.MouseLeave -= Control_MouseLeave;
            SetPanelMotionVisible(Grid_Top, false);
            SetPanelMotionVisible(Grid_Menu, false);
            Canvas_File.Visibility = Visibility.Visible;
            btnLast.IsEnabled = false;
            btnNext.IsEnabled = false;

            TreeView_File.Margin = new Thickness(TreeView_File.Margin.Left, 0, TreeView_File.Margin.Right, 0);
            GridSplitter_List.Margin = new Thickness(GridSplitter_List.Margin.Left, 0, GridSplitter_List.Margin.Right, 0);
        }

        private void formWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.System&& Keyboard.Modifiers == ModifierKeys.Alt)
            {
                switch (e.SystemKey)
                {
                    case Key.Left:
                        menuViewRClock90_Click(null, null);
                        break;
                    case Key.Right:
                        menuViewClock90_Click(null, null);
                        break;
                    case Key.Up:
                        menuViewZoomUp_Click(null, null);
                    break;
                    case Key.Down:
                        menuViewZoomDown_Click(null, null);
                        break;
                }
                return;
            }
            switch (e.Key)
            {
                case Key.End:
                    btnStop_Click(null, null);
                    break;
                case Key.F:
                    SetVideoMax(true);
                    break;
                case Key.Space:
                    {
                        if (player.Source == null) return;
                        btnStart_Click(null, null);
                    }
                    break;
                case Key.Up:
                    if (Keyboard.Modifiers==ModifierKeys.Control)
                        Slider_Speed.Value += Slider_Speed.Interval;
                    else
                        Slider_Voice.Value += Slider_Voice.Interval;
                    break;
                case Key.Down:
                    if (Keyboard.Modifiers == ModifierKeys.Control)
                        Slider_Speed.Value -= Slider_Speed.Interval;
                    else
                        Slider_Voice.Value -= Slider_Voice.Interval;
                    break;
                case Key.Escape:
                    SetVideoMax(false);
                    break;
                case Key.PageUp:
                    btnLast_Click(null, null);
                    break;
                case Key.PageDown:
                    btnNext_Click(null, null);
                    break;
                case Key.Left:
                    btnLeft_Click(null, null);
                    break;
                case Key.Right:
                    btnRight_Click(null, null);
                    break;
                    //default:
                    //    break;
            }
        }

        private void formWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveRecord();
            AppConfigHelper.SaveObj(this);
        }


        private void menuViewClock90_Click(object sender, RoutedEventArgs e)
        {
            viewRotateTransform.Angle = (viewRotateTransform.Angle + 90) % 360;
            LogInfo("媒体顺时针旋转90度");
        }

        private void menuViewRClock90_Click(object sender, RoutedEventArgs e)
        {
            viewRotateTransform.Angle = (viewRotateTransform.Angle - 90) % 360;
            LogInfo("媒体逆时针旋转90度");
        }

        private void menuViewZoomUp_Click(object sender, RoutedEventArgs e)
        {
            if (viewPercent + 0.1 > 4)
                return;
            viewPercent += 0.1;
            Grid_Player_SizeChanged(null, null);
        }

        private void menuViewZoomDown_Click(object sender, RoutedEventArgs e)
        {
            if (viewPercent - 0.1 <0)
                return;
            viewPercent -= 0.1;
            Grid_Player_SizeChanged(null, null);
        }

        private void menuViewLongerFit_Click(object sender, RoutedEventArgs e)
        {

            viewPercent = 1;
            Grid_Player_SizeChanged(null, null);
            //viewRotateTransform.Angle = 0;
        }
        private void menuViewHeightFit_Click(object sender, RoutedEventArgs e)
        {
            viewPercent = 1;
            double x = 1.0 * player.NaturalVideoWidth * Grid_Player.ActualHeight / (player.NaturalVideoHeight * Grid_Player.ActualWidth);
            if (x > 0.99)
                viewPercent = x;
            Grid_Player_SizeChanged(null, null);
            //viewRotateTransform.Angle = 0;
        }

        private void TreeView_File_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (TreeView_File.SelectedItem == null) return;
            string path = (TreeView_File.SelectedItem as MediaFileNode).FullName;
            FileInfo fileInfo = new FileInfo(path);
            if ((fileInfo.Attributes & FileAttributes.Directory) == 0)
            {
                if (supportedVideos.Contains(fileInfo.Extension.ToLower()))
                    OpenMediaFile(path);
            }
            else
            {
                OpenFolder(path);
            }
        }

        private void btnScreen_Click(object sender, RoutedEventArgs e)
        {
            SetVideoMax(WindowState == WindowState.Normal);
        }

        private void btnRight_Click(object sender, RoutedEventArgs e)
        {
            SetVideoMode(false);
            Slider_Process.Value += Slider_Process.Interval;
            player.Position = TimeSpan.FromSeconds(Slider_Process.Value);
            LogInfo("快进  " + player.Position.ToString("hh\\:mm\\:ss") + " (" + (Slider_Process.Value / Slider_Process.Maximum).ToString("P2") + ")");
            SetVideoMode(true);
        }

        private void btnLeft_Click(object sender, RoutedEventArgs e)
        {
            SetVideoMode(false);
            Slider_Process.Value -= Slider_Process.Interval;
            player.Position = TimeSpan.FromSeconds(Slider_Process.Value);
            LogInfo("快退  " + player.Position.ToString("hh\\:mm\\:ss") + " (" + (Slider_Process.Value / Slider_Process.Maximum).ToString("P2") + ")");
            SetVideoMode(true);
        }

        private void btnLast_Click(object sender, RoutedEventArgs e)
        {
            int id = Dic_MediaFiles[nowFileName].ID-1;
            if (id <0) return;
            OpenMediaFile(Dic_MediaFiles.ElementAt(id).Key);
        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            int id = Dic_MediaFiles[nowFileName].ID + 1;
            if (id ==Dic_MediaFiles.Count) return;
            OpenMediaFile(Dic_MediaFiles.ElementAt(id).Key);
        }


        private void btn_Silence_Click(object sender, RoutedEventArgs e)
        {
            if (player.Source == null) return;
            if (Slider_Voice.Value == 0)
            {
                Slider_Voice.Value = defaultVoice;
                btn_Silence.Background = VideoImageBrushs.Sound;
            }
            else
            {
                LogInfo("已静音");
                Slider_Voice.Value = 0;
                btn_Silence.Background = VideoImageBrushs.Silence;
            }
        }

        private void Slider_Speed_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (player == null) return;
            if (player.Source == null) return;
            player.SpeedRatio = (double)(Slider_Speed.Value / 10);
            textBlock_speed.Text = ((double)Slider_Speed.Value / 10).ToString("F1");
            LogInfo("倍速：  " + player.SpeedRatio.ToString("F1") + "倍速");
        }

        private void formWindow_DragEnter(object sender, System.Windows.DragEventArgs e)
        {

            if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
                e.Effects = System.Windows.DragDropEffects.Move;
            else e.Effects = System.Windows.DragDropEffects.None;
        }

        private void formWindow_PreviewDrop(object sender, System.Windows.DragEventArgs e)
        {
            string path = ((System.Array)e.Data.GetData(System.Windows.DataFormats.FileDrop)).GetValue(0).ToString();
            FileInfo fileInfo = new FileInfo(path);
            if ((fileInfo.Attributes & FileAttributes.Directory) == 0)
            {
                if (supportedVideos.Contains(fileInfo.Extension.ToLower()))
                    OpenMediaFile(path);
                else if (supportedSubs.Contains(fileInfo.Extension.ToLower()))
                    OpenSubFile(path);
            }
            else
            {
                OpenFolder(path);
            }

        }

        private void btnSub_Click(object sender, RoutedEventArgs e)
        {
            if (Canvas_Sub.Visibility == Visibility.Hidden)
            {
                Canvas_Sub.Visibility = Visibility.Visible;
            }
            else
            {
                Canvas_Sub.Visibility = Visibility.Hidden;
            }
        }

        private void btnView_Click(object sender, RoutedEventArgs e)
        {
            if (Canvas_View.Visibility == Visibility.Hidden)
            {
                Canvas_View.Visibility = Visibility.Visible;
            }
            else
            {
                Canvas_View.Visibility = Visibility.Hidden;
            }

        }

        private void Player_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            TreeView_File_SizeChanged(null, null);
            if (formWindow.Width < minWidth) formWindow.Width = minWidth;
            if (formWindow.Height < minHeight) formWindow.Height = minHeight;
            double sizePercent = formWindow.Height / preHeight;

            tbSub.FontSize = sizePercent * tbSub.FontSize;
            preWidth = formWindow.Width;
            preHeight = formWindow.Height;
            if (player == null) return;
            if (player.Source == null) return;
            double width_Percent = player.ActualWidth / player.NaturalVideoWidth;
            double height_Percent = player.ActualHeight / player.NaturalVideoHeight;
            double video_Percent = width_Percent * height_Percent;
            int mediaWidth = (int)player.ActualWidth, mediaHeight = (int)player.ActualHeight;
            string str = mediaWidth.ToString() + "×" + mediaHeight.ToString() + "(" + video_Percent.ToString("p") + ")";
            LogInfo(str);
        }

        private void TreeView_File_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (TreeView_File.Visibility == Visibility.Hidden)
                return;
            if (isListPinning)
            {
                Grid_Main.Width = formWindow.Width - (Grid_Center.ColumnDefinitions[1].Width.Value + Grid_Center.ColumnDefinitions[2].Width.Value + 12);
                Canvas_File.Margin=new Thickness(Grid_Main.Width / 2.0 - formWindow.Width/2.0, Canvas_File.Margin.Top,
                    Canvas_File.Margin.Right, Canvas_File.Margin.Bottom);
            }
            Grid_Top.Width = formWindow.Width - (Grid_Center.ColumnDefinitions[1].Width.Value + Grid_Center.ColumnDefinitions[2].Width.Value + 12);
            Grid_Menu.Width = formWindow.Width - (Grid_Center.ColumnDefinitions[1].Width.Value + Grid_Center.ColumnDefinitions[2].Width.Value + 12);
        }

        private void Grid_Form_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (DateTime.Now.Subtract(time_LastMouseDown).TotalMilliseconds > 300)
            {
                time_LastMouseDown = DateTime.Now;
            }
            else
            {
                SetVideoMax(WindowState == WindowState.Normal);
            }
            if (e.LeftButton == MouseButtonState.Pressed && WindowState == WindowState.Normal)
            {
                this.DragMove();
            }
        }


        private void Grid_Form_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!mourseVisible)
            {
                ShowCursor(1);
                mourseVisible = true;
                mourseVisibleDelay = 0;
            }
        }


        private void Player_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (viewPercent + 0.1 * e.Delta / 120 < 0|| viewPercent + 0.1 * e.Delta / 120 >4)
                    return;
                viewPercent += 0.1* e.Delta / 120;
                Grid_Player_SizeChanged(null, null);
                return;
            }
            int val = (int)Slider_Voice.Value + e.Delta*5 / 120;
            Slider_Voice.Value = val < 0 ? 0 : val > 100 ? 100 : val;
        }

        private void menuClearList_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher.InvokeAsync(() =>
            {

                if (File.Exists(configPath));
                    File.Delete(configPath);
                TreeView_File.Items.Clear();
                List_Dirctory.Clear();
                RefreshFileTree();
                LogInfo("已清空播放记录");
            });
        }


        private void menuSetDefault_Click(object sender, RoutedEventArgs e)
        {
            string startargs = System.Windows.Forms.Application.ExecutablePath;
            Process.Start(@"SetFileDefaultApp.exe", startargs);
            System.Windows.MessageBox.Show("已关联后缀名。\r\nWin10系统锁定了默认媒体应用注册表，需要用户到\r\n" +
                "Setting(设置)-Apps(应用)-\r\nDefault apps(默认应用)-Video player(媒体应用)下 再设置为本应用");
        }

        private void menuInstallHEVC_Click(object sender, RoutedEventArgs e)
        {
            if (System.Windows.MessageBox.Show(
        "是否从微软商场安装HEVC解码器？", "询问", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    string url = "ms-windows-store://pdp/?ProductId=9n4wgh0z6vhq";
                    Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
                }
            }
        }

        private void TreeViewItem_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var treeViewItem = VisualUpwardSearch<TreeViewItem>(e.OriginalSource as DependencyObject) as TreeViewItem;
            if (treeViewItem != null)
            {
                treeViewItem.Focus();
                e.Handled = true;
            }
        }

        private void menuDeleteNodeRecord_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher.InvokeAsync(() =>
            {
                MediaFileNode node = TreeView_File.SelectedItem as MediaFileNode;
                if (node.Level == 0)
                {
                    DeleteDirNodeRecord(node);
                    TreeView_File.Items.Remove(node);
                    List_Dirctory.Remove(node.FullName);
                    RefreshFileTree();
                }
                if (node.Level > 0)
                    DeleteFileNodeRecord(node);
            });
        }

        private void menuOpenNodeFolder_Click(object sender, RoutedEventArgs e)
        {
            string startargs;
            MediaFileNode node = TreeView_File.SelectedItem as MediaFileNode;
            startargs = node.Level == 0 ? node.FullName : (new FileInfo(node.FullName)).DirectoryName;
            Process.Start(@"Explorer.exe", startargs);
        }

        private void Player_MediaEnded(object sender, RoutedEventArgs e)
        {
            player.Position = TimeSpan.FromSeconds(0);
            switch (PlayOverActionMode)
            {
                case 0://playnext
                    btnNext_Click(null, null);
                    break;
                case 1://playthis
                    {
                        if (player.Source == null) return;
                        SetVideoMode(true);
                        break;
                    }
                case 2://donothing
                    break;
                default:
                    break;
            }
        }

    }
}

using CommonPlayer;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Utils;

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
    public partial class MainWindow : VcreditWindow
    {
        enum PlayOverActions { PlayNext = 0, PlayThis = 1, DoNothing = 2 }
        enum ZoomModes { Stetch = 0, Fill = 1}

        const string title = "VPlayer By WilliamT";
        string configPath = Program.configPath;
        double minWidth = 630;
        double minHeight = 250;

        double preWidth = 630;
        double preHeight = 250;
        double viewPercent = 1;
        int listWidth = 270;
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

        List<string> supportedSubs = new List<string>()
        { ".srt", ".ass", ".sub", ".vtt",".ram"
        };
        //MSPlayer player;
        LibVLCPlayer player;
        public string defaultDirctory;
        public List<string> List_Dirctory;
        public int defaultVoice = 50;
        public string nowFileName = "";
        public string nowSubName = "";
        public int nowPosition = 0;
        public Size defaultSize;
        public int PlayOverActionMode = 0;
        public int ZoomMode = 0;
        public MainWindow()
        {
            InitializeComponent();
            SetupEvents();
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
            //MaxWidth = SystemParameters.PrimaryScreenWidth+10;
            //MaxHeight = SystemParameters.PrimaryScreenHeight+10; 

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
            Canvas_Sub.Visibility = Visibility.Hidden;
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
            //player = new MSPlayer(PlayerElement);
            player = new LibVLCPlayer(PlayerElement);
            player.MediaEnded += Player_MediaEnded;
            (menuPlayOverActions.Items[PlayOverActionMode] as System.Windows.Controls.MenuItem).IsChecked = true;
            (menuZoomModes.Items[ZoomMode] as System.Windows.Controls.MenuItem).IsChecked = true;
        }

        public void SetupEvents()
        {
            //Exceptions
            System.Windows.Forms.Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            System.Windows.Forms.Application.ThreadException += Application_ThreadException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            //IO
            this.ContentRendered += formWindow_ContentRendered;
            this.Closing += formWindow_Closing;
            this.KeyDown += formWindow_KeyDown;
            Grid_Mask.DragEnter += formWindow_DragEnter;
            Grid_Mask.PreviewDrop += formWindow_PreviewDrop;

            //size
            Grid_Player.SizeChanged += Grid_Player_SizeChanged;
            Grid_Mask.SizeChanged += Grid_Mask_SizeChanged;

            //mouse
            Grid_Mask.PreviewMouseMove += Grid_Mask_MouseMove;
            Grid_Mask.MouseLeftButtonDown += Grid_Mask_MouseLeftButtonDown;
            Grid_Sub.MouseLeftButtonDown += Grid_Sub_MouseLeftButtonDown;
            Grid_Main.MouseWheel += Player_MouseWheel;
            menuOpenFile.Click += btnOpenFile_Click;
            menuOpenFolder.Click += btnOpenFolder_Click;
            menuSubOpen.Click += btnSubOpen_Click;
            menuSubVisible.Click += btnSubVisible_Click;
            menuViewClock90.Click += menuViewClock90_Click;
            menuViewRClock90.Click += menuViewRClock90_Click;
            menuViewZoomUp.Click += menuViewZoomUp_Click;
            menuViewZoomDown.Click += menuViewZoomDown_Click;
            menuOpenDir.Click += menuOpenDir_Click;
            menuScreen.Click += btnScreen_Click;
            menuTop.Click += btnTop_Click;
            menuSetDefault.Click += menuSetDefault_Click;
            menuInstallHEVC.Click += menuInstallHEVC_Click;
            btnTop.Click += btnTop_Click;
            btnMin.Click += btnMin_Click;
            btnMax.Click += btnMax_Click;
            btnClose.Click += btnClose_Click;
            btnOpenFile_Copy.Click += btnOpenFile_Click;
            btnSub.Click += btnSub_Click;
            btnScreen.Click += btnScreen_Click;
            btnVoice.Click += btnVoice_Click;

            btnLeft.Click += btnLeft_Click;
            btnRight.Click += btnRight_Click;

            btnStart.Click += btnStart_Click;
            btnLast.Click += btnLast_Click;
            btnNext.Click += btnNext_Click;
            btnStop.Click += btnStop_Click;
            btn_Silence.Click += btn_Silence_Click;
            btnSubOpen.Click += btnSubOpen_Click;
            btnSubVisible.Click += btnSubVisible_Click;
            btnShowList.Click += btnShowList_Click;
            TreeView_File.MouseDoubleClick += TreeView_File_MouseDoubleClick;
            menuDeleteNodeRecord.Click += menuDeleteNodeRecord_Click;
            menuOpenNodeFolder.Click += menuOpenNodeFolder_Click;
            menuClearList.Click += menuClearList_Click;
            btnOpenFile.Click += btnOpenFile_Click;
            btnDown.Click += btnDown_Click;
            btnOpenFolder.Click += btnOpenFolder_Click;

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
                FileInfo[] Dir = path.GetFiles("*", SearchOption.TopDirectoryOnly);
                MediaFileNode dirNode = new MediaFileNode() { Name = path.Name, FullName = path.FullName, Level = 0 };
                AddContextMenuToDirNode(dirNode);
                int i = Dic_MediaFiles.Count;
                foreach (FileInfo d in Dir)
                {
                    if (BasePlayer.SupportedVideos.Contains(d.Extension.ToLower()))
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
                Grid_Player_SizeChanged(null, null);
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
                FileInfo[] Dir = Path.GetFiles("*", SearchOption.TopDirectoryOnly);

                List<FileInfo> List_MediaFilesTmp = new List<FileInfo>();
                List_SubFiles.Clear();

                foreach (FileInfo d in Dir)
                {
                    if (BasePlayer.SupportedVideos.Contains(d.Extension.ToLower()))
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
        /// 配置画面缩放
        /// </summary>
        private void SetUp_ZoomMode()
        {
            if (player == null) return;
            switch (ZoomMode)
            {
                case 0://Stetch
                    viewPercent = 1;
                    break;
                case 1://Fill
                    {
                        viewPercent = 1;
                        if (Grid_Sub.ActualHeight < 0) break;
                        double x = (1.0 * player.NaturalVideoWidth / player.NaturalVideoHeight)/(1.0* Grid_Player.ActualWidth/ Grid_Player.ActualHeight);
                        if (x > 0.99)
                            viewPercent = x;
                    }
                    break;
                default:
                    break;
            }
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
        /// 显示文件列表
        /// </summary>
        /// <param name="flag"></param>
        private void ShowList(bool flag)
        {
            if (flag)
            {
                Grid_Center.ColumnDefinitions[1].Width = new GridLength(5);
                Grid_Center.ColumnDefinitions[2].Width = new GridLength(listWidth);
                TreeView_File.Visibility = Visibility.Visible;
                GridSplitter_List.Visibility = Visibility.Visible;

                btnShowList.Margin = new Thickness(0, 0, 0, 0);
                btnShowList.Content = ">";

                btnShowList.Opacity = 1;
                btnShowList.MouseEnter -= Control_MouseEnter;
                btnShowList.MouseLeave -= Control_MouseLeave;
            }
            else
            {
                if (player!=null&&player.Source!=null)
                {
                    btnShowList.Opacity = 0;
                    btnShowList.MouseEnter += Control_MouseEnter;
                    btnShowList.MouseLeave += Control_MouseLeave;
                    Canvas_Top.Visibility = Visibility.Visible;
                    Grid_Menu.Visibility = Visibility.Visible;
                }
                TreeView_File.Visibility = Visibility.Hidden;
                GridSplitter_List.Visibility = Visibility.Hidden;
                if(Grid_Center.ColumnDefinitions[2].Width.Value>0) listWidth = (int)Grid_Center.ColumnDefinitions[2].Width.Value;
                Grid_Center.ColumnDefinitions[1].Width = new GridLength(0);
                Grid_Center.ColumnDefinitions[2].Width = new GridLength(0);
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
            SaveRecord(player.Position.TotalMilliseconds);
        }
        private void SaveRecord(double ms)
        {
            if (player.Source != null)
                if (player.NaturalDuration.HasTimeSpan)
                {
                    AppConfigHelper.SaveKey("FileRecord-" + nowFileName, ms.ToString());
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
            if (!BasePlayer.SupportedVideos.Contains(fileInfo.Extension.ToLower()))
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
            player.Volume = (int)(100* defaultVoice / Slider_Voice.Maximum);
            label_NowFile.Content = fileInfo.Name;
            Dic_MediaFiles[nowFileName].mediaFileNode.IsUsing = true;

            SetVideoMode(true);
            DateTime t = DateTime.Now;
            while (!player.NaturalDuration.HasTimeSpan || player.NaturalDuration.TimeSpan.TotalMilliseconds==0)//等待媒体开始播放
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
            player.GetVideoResolution();
            //UI
            Slider_Process.Maximum = (int)player.NaturalDuration.TimeSpan.TotalSeconds;
            Slider_Process.Visibility = Visibility.Visible;
            btnLeft.Visibility = Visibility.Visible;
            btnRight.Visibility = Visibility.Visible;
            Label_Process.Visibility = Visibility.Visible;
            SetPanelMotionVisible(Grid_Top, true);
            SetPanelMotionVisible(Grid_Menu, true);
            Canvas_File.Visibility = Visibility.Hidden;
            btnStop.IsEnabled = true;
            Grid_Main.Focus();
            ShowList(btnShowList.Content.ToString() == ">");

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
                Grid_Form.Margin = new Thickness(-6, -6, -6, -6);
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
                Grid_Form.Margin = new Thickness(0, 0, 0, 0);
                grid.RowDefinitions[0].Height = new GridLength(3);
                grid.RowDefinitions[2].Height = new GridLength(3);
                grid.ColumnDefinitions[0].Width = new GridLength(3);
                grid.ColumnDefinitions[2].Width = new GridLength(3);
                WindowState = WindowState.Normal;
                btnScreen.Background = VideoImageBrushs.Screen;
                menuScreen.Header = "全屏";
            }
        }
        private void SetWindowMax()
        {
            WindowState = WindowState.Normal;
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
                this.Width = rect.Width + 22;
                this.Height = rect.Height + 22;
                this.Top = -11;
                this.Left = (this.Left>= rect.Width? rect.Width :0)- 11;
            }
            else
            {
                btnMax.Background = VideoImageBrushs.Max;
                grid.RowDefinitions[0].Height = new GridLength(2);
                grid.RowDefinitions[2].Height = new GridLength(2);
                grid.ColumnDefinitions[0].Width = new GridLength(2);
                grid.ColumnDefinitions[2].Width = new GridLength(2);
                this.Width = defaultWidth;
                this.Height = defaultHeight;
                this.Top = (rect.Height - defaultHeight) / 2;
                this.Left = (this.Left >= rect.Width ? rect.Width : 0) + (rect.Width - defaultWidth) / 2;

            }
            btnScreen.Background = VideoImageBrushs.Screen;
            isWindowMax = !isWindowMax;
        }

        private void Grid_Player_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SetUp_ZoomMode();
            if (Grid_Player.ActualWidth <= 0 || Grid_Player.ActualHeight <= 0) return;
            player.Scale = (float)viewPercent;
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
            if (Grid_Player.IsMouseOver&& mourseVisible)
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

        private void menuZoomModes_Items_Checked(object sender, RoutedEventArgs e)
        {
            var current = sender as System.Windows.Controls.MenuItem;
            ZoomMode = (int)(Enum.Parse(typeof(ZoomModes), current.Tag.ToString()));
            foreach (var m in menuZoomModes.Items)
            {
                var menucheck = m as System.Windows.Controls.MenuItem;
                if (current.Name == menucheck.Name) continue;
                menucheck.IsChecked = false;
            }
            Grid_Player_SizeChanged(null,null);
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
            player.Volume = (int)(100 * Slider_Voice.Value / Slider_Voice.Maximum);
            if (Slider_Voice.Value > 0) defaultVoice = (int)Slider_Voice.Value;
            LogInfo(string.Format("音量:{0}%", Slider_Voice.Value));
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

        private void TreeView_File_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (TreeView_File.SelectedItem == null) return;
            string path = (TreeView_File.SelectedItem as MediaFileNode).FullName;
            FileInfo fileInfo = new FileInfo(path);
            if ((fileInfo.Attributes & FileAttributes.Directory) == 0)
            {
                if (BasePlayer.SupportedVideos.Contains(fileInfo.Extension.ToLower()))
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
                if (BasePlayer.SupportedVideos.Contains(fileInfo.Extension.ToLower()))
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

        private void Grid_Mask_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            TreeView_File_SizeChanged(null, null);
            if (formWindow.Width < minWidth) { formWindow.Width = minWidth; return; }
            if (formWindow.Height < minHeight) {formWindow.Height = minHeight; return;}
            double sizePercent = formWindow.Height / preHeight;

            tbSub.FontSize = sizePercent * tbSub.FontSize;
            preWidth = formWindow.Width;
            preHeight = formWindow.Height;
            if (player == null) return;
            if (player.Source == null) return;
            double width_Percent = Math.Round(player.ActualWidth / player.NaturalVideoWidth,2);
            double height_Percent = Math.Round(player.ActualHeight / player.NaturalVideoHeight,2);
            double video_Percent = width_Percent * height_Percent;
            int mediaWidth = (int)Math.Round(player.ActualWidth),mediaHeight = (int)Math.Round(player.ActualHeight);
            string str = mediaWidth.ToString() + "×" + mediaHeight.ToString() + "(" + video_Percent.ToString("p") + ")";
            LogInfo(str);
        }

        private void TreeView_File_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (TreeView_File.Visibility == Visibility.Hidden)
                return;
        }

        private void Grid_Sub_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (DateTime.Now.Subtract(time_LastMouseDown).TotalMilliseconds > 300)
            {
                time_LastMouseDown = DateTime.Now;
            }
            else
            {
                SetVideoMax(WindowState == WindowState.Normal);
            }
            if (player.Source == null) return;
            btnStart_Click(null, null);
        }

        private void Grid_Mask_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && WindowState == WindowState.Normal)
            {
                this.DragMove();
            }
        }


        private void Grid_Mask_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
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

        private void Player_MediaEnded(object sender, EventArgs e)
        {
            SaveRecord(0);
            Dispatcher.BeginInvoke(new Action(() =>
            {
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
            }));
        }

    }
}

using CommonPlayer;
using LibVLCSharp.Shared;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Text.RegularExpressions;
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
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Encodings.Web;
using System.Xml.Linq;

namespace VPlayer
{
    public struct LogInfo
    {
        public string logStr;
        public DateTime disposeTime;
    }

    public class MediaFIle
    {
        public int ID;
        public string FilePath;
        public MediaNode mediaFileNode;
        public MediaFIle(int id, string filePath, MediaNode node)
        {
            ID = id;
            FilePath = filePath;
            mediaFileNode = node;
        }
    }

    public class FileRecord
    {
        public string FilePath { get; set; }
        public int Position { get; set; }
        public int AudioTrack { get; set; }
        public SubTrack SubTrack { get; set; }
        [JsonIgnore]
        public string Dirctory { get; set; }
        public FileRecord(string filePath)
        {
            FilePath = filePath;
            Dirctory = Path.GetDirectoryName(filePath);
            Position = 0;
            AudioTrack = 0;
            SubTrack = null;
        }
    }

    public class SubTrack
    {
        public int TrackID { get; set; }
        public int Order { get; set; }
        public string Name { get; set; }
        public string SubPath { get; set; }
        public SubTrack(int order,int trackID,string name,string subPath)
        {
            Order = order;
            TrackID = trackID;
            Name=name;
            SubPath = subPath;
        }
    }

    public class AppData
    {
        public int DefaultVoice { get; set; }
        public int PlayOverActionMode { get; set; }
        public bool UsingFormattedSub { get; set; }
        public int ZoomMode { get; set; }
        public string NowFileName { get; set; }
        public List<string> List_RootDirctory { get; set; }
        public Dictionary<string, FileRecord> FileRecords { get; set; }

        public AppData()
        {
            DefaultVoice = 50;
            PlayOverActionMode = 0;
            UsingFormattedSub = false;
            ZoomMode = 0;
            NowFileName = "";
            List_RootDirctory = new List<string>();
            FileRecords = new Dictionary<string, FileRecord>();

        }
    }

    public class MediaNode : TreeViewItemBase
    {
        public MediaNode()
        {
            this.Childs = new ObservableCollection<MediaNode>();
        }

        public string Name { get; set; }
        public string FullName { get; set; }
        public bool IsFile { get; set; }
        public MediaNode ParentNode;

        public System.Windows.Controls.ContextMenu ContextMenu { get; set; }

        public ObservableCollection<MediaNode> Childs { get; set; }
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

        private string toolTip;
        public string ToolTip
        {
            get { return this.toolTip; }
            set
            {
                if (value != this.toolTip)
                {
                    this.toolTip = value;
                    NotifyPropertyChanged("ToolTip");
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


        static JsonSerializerOptions options = new JsonSerializerOptions
        {
            WriteIndented = true,          // 格式化输出（缩进）
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping // 中文不转义
        };

        enum PlayOverActions { PlayNext = 0, PlayThis = 1, DoNothing = 2 }
        enum ZoomModes { Stetch = 0, Fill = 1 }

        const string title = "VPlayer By WilliamT";
        double minWidth = 630;
        double minHeight = 250;

        double preWidth = 630;
        double preHeight = 250;
        Size defaultSize;
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
        List<SubTrack> List_SubTracks;
        Dictionary<string, MediaFIle> MediaListFiles;
        List<SubtitlesParser.Classes.SubtitleItem> subItems;

        List<string> supportedSubs = new List<string>()
        { ".srt", ".ass", ".sub", ".vtt",".ram"
        };
        //MSPlayer player;
        LibVLCPlayer player;
        FileRecord nowFileRecord;
        //AppData
        public AppData appData;
        int defaultVoice = 50;
        int playOverActionMode = 0;
        bool usingFormattedSub = false;
        int zoomMode = 0;
        string nowFileName;
        List<string> list_RootDirctory;
        Dictionary<string, FileRecord> fileRecords;

        public MainWindow()
        {
            InitializeComponent();
            SetupEvents();
            SetupUI();
            pi = new POINT();
        }
        double defaultWidth
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
        double defaultHeight
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
            //AppConfigHelper.LoadObj(this);
            LoadConfig();
            if (defaultVoice == 0)
            {
                defaultVoice = 50;
                Slider_Voice.Value = 50;
            }
            Queue_LogInfo = new Queue<LogInfo>();
            List_SubTracks = new List<SubTrack>();
            MediaListFiles = new Dictionary<string, MediaFIle>();
            //viewRotateTransform = new RotateTransform();
            //player.LayoutTransform= viewRotateTransform;
            RefreshFileTree();
            //player = new MSPlayer(PlayerElement);
            player = new LibVLCPlayer(PlayerElement);
            player.MediaEnded += Player_MediaEnded;
            (menuPlayOverActions.Items[playOverActionMode] as System.Windows.Controls.MenuItem).IsChecked = true;
            menuSubFormatted.IsChecked = usingFormattedSub;
            (menuZoomModes.Items[zoomMode] as System.Windows.Controls.MenuItem).IsChecked = true;
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
            tbSub.SizeChanged += tbSub_SizeChanged;
            tbSub.IsVisibleChanged += tbSub_IsVisibleChanged;

            //mouse
            Grid_Mask.PreviewMouseMove += Grid_Mask_MouseMove;
            Grid_Mask.MouseLeftButtonDown += Grid_Mask_MouseLeftButtonDown;
            Grid_Sub.MouseLeftButtonDown += Grid_Sub_MouseLeftButtonDown;
            Grid_Main.MouseWheel += Player_MouseWheel;
            menuOpenFile.Click += btnOpenFile_Click;
            menuOpenFolder.Click += btnOpenFolder_Click;
            menuSubOpen.Click += btnSubOpen_Click;
            menuSubHide.Click += btnSubVisible_Click;
            menuSubFormatted.Click += menuSubFormatted_Click;

            menuViewClock90.Click += menuViewClock90_Click;
            menuViewRClock90.Click += menuViewRClock90_Click;
            menuViewZoomUp.Click += menuViewZoomUp_Click;
            menuViewZoomDown.Click += menuViewZoomDown_Click;
            menuOpenDir.Click += menuOpenDir_Click;
            menuScreen.Click += btnScreen_Click;
            menuTop.Click += btnTop_Click;
            menuSetDefault.Click += menuSetDefault_Click;
            menuInstallHEVC.Click += menuInstallHEVC_Click;
            menuCodecInfo.Click += menuCodecInfo_Click;
            btnTop.Click += btnTop_Click;
            btnMin.Click += btnMin_Click;
            btnMax.Click += btnMax_Click;
            btnClose.Click += btnClose_Click;
            btnOpenFile_Copy.Click += btnOpenFile_Click;
            btnScreen.Click += btnScreen_Click;
            btnVoice.Click += btnVoice_Click;

            btnLeft.Click += btnLeft_Click;
            btnRight.Click += btnRight_Click;

            btnStart.Click += btnStart_Click;
            btnLast.Click += btnLast_Click;
            btnNext.Click += btnNext_Click;
            btnStop.Click += btnStop_Click;
            btn_Silence.Click += btn_Silence_Click;;
            btnShowList.Click += btnShowList_Click;
            TreeView_File.MouseDoubleClick += TreeView_File_MouseDoubleClick;
            menuOpenNodeFolder.Click += menuOpenNodeFolder_Click;
            menuDeleteNodeRecord.Click += menuDeleteNodeRecord_Click;
            menuClearList.Click += menuClearList_Click;
            menuClearRecords.Click += menuClearRecords_Click;
            btnOpenFile.Click += btnOpenFile_Click;
            btnDown.Click += btnDown_Click;
            btnOpenFolder.Click += btnOpenFolder_Click;

        }

        private void tbSub_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            tbSubUp.Visibility = tbSub.Visibility;
            tbSubDown.Visibility = tbSub.Visibility;
            tbSubLeft.Visibility = tbSub.Visibility;
            tbSubRight.Visibility = tbSub.Visibility;
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
        private void DeleteDirNodeRecord(MediaNode node)
        {
            try
            {
                foreach (MediaNode child in node.Childs)
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
        private void DeleteFileNodeRecord(MediaNode node)
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

        private void AddContextMenuToDirNode(MediaNode customNode)
        {

            customNode.ContextMenu = new System.Windows.Controls.ContextMenu();

            System.Windows.Controls.MenuItem menuOpenDirNodeFolder = new System.Windows.Controls.MenuItem();
            menuOpenDirNodeFolder.Header = "打开文件夹";
            menuOpenDirNodeFolder.Click += menuOpenNodeFolder_Click;

            System.Windows.Controls.MenuItem menuDeleteDirNodeRecord = new System.Windows.Controls.MenuItem();
            menuDeleteDirNodeRecord.Header = "删除目录及记录";
            menuDeleteDirNodeRecord.Click += menuDeleteNodeRecord_Click;

            System.Windows.Controls.MenuItem menuClearRecords = new System.Windows.Controls.MenuItem();
            menuClearRecords.Header = "清空播放进度";
            menuClearRecords.Click += menuClearRecords_Click;

            System.Windows.Controls.MenuItem menuClearDirList = new System.Windows.Controls.MenuItem();
            menuClearDirList.Header = "清空播放列表";
            menuClearDirList.Click += menuClearList_Click;

            customNode.ContextMenu.Items.Add(menuOpenDirNodeFolder);
            customNode.ContextMenu.Items.Add(menuDeleteDirNodeRecord);
            customNode.ContextMenu.Items.Add(menuClearRecords);
            customNode.ContextMenu.Items.Add(menuClearDirList);

        }

        private void AddContextMenuToFileNode(MediaNode customNode)
        {
            customNode.ContextMenu = new System.Windows.Controls.ContextMenu();
            System.Windows.Controls.MenuItem menuOpenFileNodeFolder = new System.Windows.Controls.MenuItem();
            menuOpenFileNodeFolder.Header = "打开文件目录";
            menuOpenFileNodeFolder.Click += menuOpenNodeFolder_Click;

            System.Windows.Controls.MenuItem menuDeleteFileNodeRecord = new System.Windows.Controls.MenuItem();
            menuDeleteFileNodeRecord.Header = "删除该节点记录";
            menuDeleteFileNodeRecord.Click += menuDeleteNodeRecord_Click;


            System.Windows.Controls.MenuItem menuClearRecords = new System.Windows.Controls.MenuItem();
            menuClearRecords.Header = "清空播放进度";
            menuClearRecords.Click += menuClearRecords_Click;

            System.Windows.Controls.MenuItem menuFileClearList = new System.Windows.Controls.MenuItem();
            menuFileClearList.Header = "清空播放记录";
            menuFileClearList.Click += menuClearList_Click;

            customNode.ContextMenu.Items.Add(menuOpenFileNodeFolder);
            customNode.ContextMenu.Items.Add(menuDeleteFileNodeRecord);
            customNode.ContextMenu.Items.Add(menuClearRecords);
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
                
                MediaNode node = GeneralDirectoryNode(DirctoryName);
                if (node.Childs.Count == 0) return;
                TreeView_File.Items.Add(node);
                if (list_RootDirctory.Contains(DirctoryName)) return;
                list_RootDirctory.Add(DirctoryName);
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
        private MediaNode GeneralDirectoryNode(string DirctoryName)
        {
            try
            {
                DirectoryInfo path = new DirectoryInfo(DirctoryName);
                MediaNode rootNode = new MediaNode() { Name = path.Name, FullName = path.FullName,IsFile=false,ParentNode = null};
                AddContextMenuToDirNode(rootNode);

                FileInfo[] fileInfos = path.GetFiles("*", SearchOption.TopDirectoryOnly);
                DirectoryInfo[] dirInfos = path.GetDirectories("*", SearchOption.TopDirectoryOnly);
                foreach (FileInfo f in fileInfos)
                {
                    if (MediaListFiles.ContainsKey(f.FullName)) continue;
                    if (BasePlayer.SupportedVideos.Contains(f.Extension.ToLower()))
                    {
                        MediaNode fileNode = new MediaNode()
                        {
                            Name = f.Name,
                            FullName = f.FullName,
                            IsFile = true,
                            ToolTip = f.FullName,
                            ParentNode= rootNode,
                        }; 
                        AddContextMenuToFileNode(fileNode);
                        int record = 0;
                        if (fileRecords.ContainsKey(f.FullName))
                        {
                            record = fileRecords[f.FullName].Position;
                        }
                        fileNode.IsRecorded = record > 0;
                        rootNode.Childs.Add(fileNode);
                        MediaListFiles[f.FullName] = new MediaFIle(MediaListFiles.Count, f.FullName, fileNode);
                    }
                }
                foreach (DirectoryInfo d in dirInfos)
                {
                    MediaNode dirNode = GeneralDirectoryNode(d.FullName);
                    dirNode.ParentNode= rootNode;
                    if(dirNode.Childs.Count>0)
                        rootNode.Childs.Add(dirNode);
                }
                return rootNode;
            }
            catch
            {
                return null;
            }
        }

        private SubTrack TryAddSubTrack(SubTrack subTrack)
        {
            foreach (SubTrack st in List_SubTracks)
            {
                if (subTrack.TrackID == st.TrackID || (subTrack.SubPath!=""&&subTrack.SubPath == st.SubPath))
                {
                    return st;
                }
            }
            System.Windows.Controls.MenuItem SubTrackItem = new System.Windows.Controls.MenuItem();
            SubTrackItem.Header = string.Format("{0}:{1}", subTrack.TrackID, subTrack.Name);
            SubTrackItem.Tag = menuSubTracks.Items.Count;
            SubTrackItem.IsCheckable = true;
            SubTrackItem.Click += menuSubTrackDescription_Click;
            if (subTrack.SubPath != "")
                player.AddSubtile(subTrack.SubPath);;
            List_SubTracks.Add(subTrack);
            menuSubTracks.Items.Add(SubTrackItem);
            return subTrack;
        }

        /// <summary>
        /// 得到当前文件本地字幕名
        /// </summary>
        private SubTrack NowMediaMatchedSubName()
        {
            SubTrack MatchedTrack = null;

            FileInfo fileInfo = new FileInfo(nowFileName);
            List_SubTracks.Clear();
            menuSubTracks.Items.Clear();
            foreach(var st in player.SubtitleTrackDescription)
            {
                SubTrack subTrack = new SubTrack(List_SubTracks.Count, st.Id, st.Name, "");
                TryAddSubTrack(subTrack);
                if (st.Name != null)
                {
                    MatchedTrack = subTrack;
                }
            }

            string fileNoExPath = fileInfo.Name.Substring(0, fileInfo.Name.Length - fileInfo.Extension.Length);
            string cleanFileName = Regex.Replace(fileNoExPath, @"[^\p{L}\p{N}]", "");
            Match m = Regex.Match(cleanFileName, @"\d");
            int headnum = cleanFileName.Length;
            if (m.Success)
                headnum = Math.Min(m.Index+5, cleanFileName.Length);
            string filehead = cleanFileName.Substring(0, headnum);
            DirectoryInfo path = new DirectoryInfo(nowFileRecord.Dirctory);
            FileInfo[] Dir = path.GetFiles("*", SearchOption.AllDirectories);
            foreach (FileInfo d in Dir)
            {
                if (supportedSubs.Contains(d.Extension.ToLower()))
                {
                    string cleanSubName = Regex.Replace(d.Name.Substring(0,d.Name.Length-d.Extension.Length), @"[^\p{L}\p{N}]", "");
                    int hnum = Math.Min(cleanSubName.Length, headnum);
                    if (String.Equals(filehead, cleanSubName.Substring(0, hnum), StringComparison.OrdinalIgnoreCase))
                    {
                        SubTrack subTrack = new SubTrack(List_SubTracks.Count,List_SubTracks.Count == 0 ? 0 : List_SubTracks[List_SubTracks.Count - 1].TrackID + 1, d.Name, d.FullName);
                        TryAddSubTrack(subTrack);
                        if (MatchedTrack == null||cleanSubName.Contains(cleanFileName))
                        {
                            MatchedTrack = subTrack;
                        }
                    }
                }
            }
            if (nowFileRecord.SubTrack != null)
            {
                if (nowFileRecord.SubTrack.SubPath == null || File.Exists(nowFileRecord.SubTrack.SubPath))
                {
                    MatchedTrack = nowFileRecord.SubTrack;
                }
            }
            return MatchedTrack;
        }

        /// <summary>
        /// 刷新文件列表树
        /// </summary>
        private void RefreshFileTree()
        {
            try
            {
                TreeView_File.Items.Clear();
                MediaListFiles.Clear();
                MediaNode node;
                foreach (var d in list_RootDirctory)
                {
                    if (!Directory.Exists(d))
                    {
                        continue;
                    }
                    node = GeneralDirectoryNode(d);
                    if (node.Childs.Count == 0) return;
                    TreeView_File.Items.Add(node);
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
            Queue_LogInfo.Enqueue(new LogInfo() { logStr = str + "\n", disposeTime = DateTime.Now.AddSeconds(4) });
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
                if (player.Source != null)
                {
                    MediaListFiles[nowFileName].mediaFileNode.IsUsing = false;
                    MediaListFiles[nowFileName].mediaFileNode.IsRecorded = true;
                    if (player.NaturalDuration.HasTimeSpan)
                    {
                        nowFileRecord.Position = (int)player.Position.TotalMilliseconds;
                        if (!fileRecords.ContainsKey(nowFileName))
                        {
                            fileRecords.Add(nowFileName, nowFileRecord);
                        }
                    }
                }
                if (player.IsPlaying)
                {
                    SetVideoMode(false);
                }
                PlayRecord(fileName);
                Grid_Mask_SizeChanged(null, null);
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
                FileInfo[] Dir = Path.GetFiles("*", SearchOption.AllDirectories);

                List<FileInfo> List_MediaFilesTmp = new List<FileInfo>();

                foreach (FileInfo d in Dir)
                {
                    if (BasePlayer.SupportedVideos.Contains(d.Extension.ToLower()))
                    {
                        List_MediaFilesTmp.Add(d);
                    }
                    if (supportedSubs.Contains(d.Extension.ToLower()))
                    {
                        TryAddSubTrack(new SubTrack(List_SubTracks.Count,List_SubTracks.Count == 0 ? 0 : List_SubTracks[List_SubTracks.Count - 1].TrackID + 1, d.Name, d.FullName));
                    }
                }
                if (List_MediaFilesTmp.Count == 0)
                {
                    if (List_SubTracks.Count > 0)
                        OpenSubFile(List_SubTracks.First().SubPath);
                }
                else
                {
                    AddDirectoryToListFile(folderName);
                    OpenMediaFile(List_MediaFilesTmp.First().FullName);
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
        private void OpenSubFile(string filePath)
        {
            if (player.Source == null) return;
            if (filePath == null || filePath == "") return;
            if (!File.Exists(filePath)) return;
            FileInfo fileInfo = new FileInfo(filePath);
            if (!supportedSubs.Contains(fileInfo.Extension.ToLower()))
            {
                return;
            }
            SubTrack subtitleTrack = new SubTrack(List_SubTracks.Count,List_SubTracks.Count == 0?0:List_SubTracks[List_SubTracks.Count - 1].TrackID + 1, fileInfo.Name, fileInfo.FullName);
            subtitleTrack=TryAddSubTrack(subtitleTrack);
            OpenSubTrack(subtitleTrack);
            return;
        }

        /// <summary>
        /// 载入字幕文件
        /// </summary>
        /// <param name="fileName"></param>
        private bool OpenSubTrack(SubTrack subtrack)
        {
            try
            {
                nowFileRecord.SubTrack = subtrack;
                for (int i = 0; i < menuSubTracks.Items.Count; i++)
                {
                    if (i == subtrack.Order)
                    {
                        ((System.Windows.Controls.MenuItem)menuSubTracks.Items[i]).IsChecked = true;
                    }
                    else
                    {
                        ((System.Windows.Controls.MenuItem)menuSubTracks.Items[i]).IsChecked = false;
                    }
                }
                if(!usingFormattedSub&& subtrack.SubPath == "")
                {
                    SetSubFormatted(true);
                }

                if (usingFormattedSub)
                {
                    player.SubtitleTrack = subtrack.TrackID;
                }
                else
                {
                    player.SubtitleTrack = -1;
                    tbSub.Visibility = Visibility.Visible;

                    if (subItems != null)
                        subItems.Clear();
                    var subParse = new SubtitlesParser.Classes.Parsers.SubParser();
                    using (StreamReader sr = new StreamReader(subtrack.SubPath))
                    {
                        subItems = subParse.ParseStream(sr.BaseStream);
                    }
                }
                LogInfo("已加载字幕" + subtrack.Name);
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
            switch (zoomMode)
            {
                case 0://Stetch
                    viewPercent = 1;
                    break;
                case 1://Fill
                    {
                        viewPercent = 1;
                        if (Grid_Sub.ActualHeight < 0) break;
                        double x = (1.0 * player.NaturalVideoWidth / player.NaturalVideoHeight) / (1.0 * Grid_Player.ActualWidth / Grid_Player.ActualHeight);
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
                if (player != null && player.Source != null)
                {
                    btnShowList.Opacity = 0;
                    btnShowList.MouseEnter += Control_MouseEnter;
                    btnShowList.MouseLeave += Control_MouseLeave;
                    Canvas_Top.Visibility = Visibility.Visible;
                    Grid_Menu.Visibility = Visibility.Visible;
                }
                TreeView_File.Visibility = Visibility.Hidden;
                GridSplitter_List.Visibility = Visibility.Hidden;
                if (Grid_Center.ColumnDefinitions[2].Width.Value > 0) listWidth = (int)Grid_Center.ColumnDefinitions[2].Width.Value;
                Grid_Center.ColumnDefinitions[1].Width = new GridLength(0);
                Grid_Center.ColumnDefinitions[2].Width = new GridLength(0);
                btnShowList.Background = null;
                btnShowList.Content = "<";
            }
        }

        private void ShowAudioTrack()
        {
            menuAudioTrack.Items.Clear();
            if (player.AudioTrackDescription.Length > 0)
            {
                int i = 0;
                foreach (var item in player.AudioTrackDescription)
                {
                    System.Windows.Controls.MenuItem AudioTrackItem = new System.Windows.Controls.MenuItem();
                    AudioTrackItem.Header = string.Format("{0}:{1}", item.Id, item.Name);
                    AudioTrackItem.Tag = i++;
                    AudioTrackItem.IsCheckable = true;
                    AudioTrackItem.Click += menuAudioTrackDescription_Click;
                    menuAudioTrack.Items.Add(AudioTrackItem);
                }
                SetAudioTrack(nowFileRecord.AudioTrack);
            }
        }

        private void menuAudioTrackDescription_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.MenuItem AudioTrackItem = sender as System.Windows.Controls.MenuItem;
            int AudioTrackId = (int)AudioTrackItem.Tag;
            Dispatcher.InvokeAsync(() =>
            {
                SetAudioTrack(AudioTrackId);
            });
        }

        private void menuSubTrackDescription_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.MenuItem SubTrackItem = sender as System.Windows.Controls.MenuItem;
            int SubTrackId = (int)SubTrackItem.Tag;
            Dispatcher.InvokeAsync(() =>
            {
                OpenSubTrack(List_SubTracks[SubTrackId]);
            });
        }

        private void SetAudioTrack(int num)
        {
            if (num<0|| player.AudioTrack <0|| num >= menuAudioTrack.Items.Count|| player.AudioTrack>= menuAudioTrack.Items.Count) return;
            System.Windows.Controls.MenuItem LastAudioTrackItem = (System.Windows.Controls.MenuItem)menuAudioTrack.Items[player.AudioTrack];
            player.AudioTrack = num;
            nowFileRecord.AudioTrack = player.AudioTrack;
            System.Windows.Controls.MenuItem NowAudioTrackItem = (System.Windows.Controls.MenuItem)menuAudioTrack.Items[num];
            LastAudioTrackItem.IsChecked = false;
            NowAudioTrackItem.IsChecked = true;
            LogInfo("已加载音轨：" + NowAudioTrackItem.Header);
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
        private void SaveConfig()
        {
            appData.DefaultVoice = defaultVoice;
            appData.PlayOverActionMode = playOverActionMode;
            appData.UsingFormattedSub = usingFormattedSub;
            appData.ZoomMode = zoomMode;
            appData.NowFileName = nowFileName;
            appData.List_RootDirctory = list_RootDirctory;
            appData.FileRecords = fileRecords;
            string json = JsonSerializer.Serialize(appData, options);
            File.WriteAllText(Program.configPath, json);
        }

        private void LoadConfig()
        {
            
            if (File.Exists(Program.configPath))
                appData = JsonSerializer.Deserialize<AppData>(File.ReadAllText(Program.configPath), options);
            if (appData == null)
                appData = new AppData();

            defaultVoice=appData.DefaultVoice;
            playOverActionMode = appData.PlayOverActionMode;
            usingFormattedSub = appData.UsingFormattedSub;
            zoomMode = appData.ZoomMode;
            nowFileName = appData.NowFileName;
            list_RootDirctory = appData.List_RootDirctory;
            fileRecords = appData.FileRecords;
        }

        private void RemoveRecord(string fileName)
        {
            fileRecords.Remove(fileName);
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
            if (!MediaListFiles.ContainsKey(fileInfo.FullName))
            {
                AddDirectoryToListFile(fileInfo.DirectoryName);
            }
            
            player.Open(fileName);

            Slider_Voice.Value = defaultVoice;
            player.Volume = (int)(100* defaultVoice / Slider_Voice.Maximum);
            label_NowFile.Content = fileInfo.Name;
            MediaListFiles[fileName].mediaFileNode.IsUsing = true;

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

            MediaNode md;
            if (nowFileName != ""&& MediaListFiles.ContainsKey(nowFileName))
            {
                md = MediaListFiles[nowFileName].mediaFileNode.ParentNode;
                do
                {
                    md.IsExpanded = false;
                    md = md.ParentNode;
                }
                while (md!= null);
            }
            nowFileName = fileInfo.FullName;
            if (!fileRecords.ContainsKey(fileInfo.FullName))
            {
                fileRecords.Add(fileName, new FileRecord(fileName));
            }
            nowFileRecord = fileRecords[nowFileName];
            md = MediaListFiles[nowFileName].mediaFileNode.ParentNode;
            do
            {
                md.IsExpanded = true;
                md = md.ParentNode;
            }
            while (md!= null);
            ShowAudioTrack();
            if (nowFileRecord.Position + 1000 < player.NaturalDuration.TimeSpan.TotalMilliseconds)
                player.Position = new TimeSpan(0, 0, 0, 0, nowFileRecord.Position);

            if (subItems != null)
            {
                subItems.Clear();
                tbSub.Text = "";
                tbSubUp.Text = tbSub.Text;
                tbSubDown.Text = tbSub.Text;
                tbSubLeft.Text = tbSub.Text;
                tbSubRight.Text = tbSub.Text;
            }
            SubTrack subTrack = NowMediaMatchedSubName();
            if (subTrack != null)
            {
                OpenSubTrack(subTrack);
            }

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
            Grid_Mask.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#01000000"));
            ShowList(btnShowList.Content.ToString() == ">");

            if (MediaListFiles[nowFileName].ID != 0)
                btnLast.IsEnabled = true;
            if (MediaListFiles[nowFileName].ID != MediaListFiles.Count - 1)
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

        private void SetSubFormatted(bool flag)
        {
            if (flag)
            {
                LogInfo("切换为特效字幕");
                menuSubFormatted.IsChecked = true;
                usingFormattedSub = true;
                menuSubHide.IsChecked = false;
                tbSub.Visibility = Visibility.Hidden;
            }
            else
            {
                LogInfo("切换为非特效字幕");
                menuSubFormatted.IsChecked = false;
                usingFormattedSub = false;
                menuSubHide.IsChecked = false;
                tbSub.Visibility = Visibility.Visible;
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
            tbSubUp.Text = tbSub.Text;
            tbSubDown.Text = tbSub.Text;
            tbSubLeft.Text = tbSub.Text;
            tbSubRight.Text = tbSub.Text;
        }
        private void timer_Time_tick(object sender, EventArgs e)
        {
            label_time.Content = DateTime.Now.ToString("HH:mm:ss");
            if (Queue_LogInfo.Count>0&& Queue_LogInfo.Peek().disposeTime< DateTime.Now)
            {
                textBlock_Log.Text = textBlock_Log.Text.Replace(Queue_LogInfo.Peek().logStr,"");
                Queue_LogInfo.Dequeue();
            }
            if (Grid_Main.IsMouseOver&& mourseVisible)
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
            if (Directory.Exists(nowFileRecord.Dirctory)) ofd.InitialDirectory = nowFileRecord.Dirctory;
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
            if (Directory.Exists(nowFileRecord.Dirctory)) Fbd.SelectedPath = nowFileRecord.Dirctory;
            if (Fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                OpenFolder(Fbd.SelectedPath);
            }
            timer_Time.Start();
        }

        private void btnSubOpen_Click(object sender, RoutedEventArgs e)
        {
            timer_Time.Stop();
            OpenFileDialog ofd = new OpenFileDialog();
            if (Directory.Exists(nowFileRecord.Dirctory)) ofd.InitialDirectory = nowFileRecord.Dirctory;

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
                menuSubHide.IsChecked = true;
                tbSub.Visibility = Visibility.Hidden;
                player.SubtitleTrack = -1;
            }
            else
            {
                LogInfo("已显示字幕");
                menuSubHide.IsChecked = false;
                tbSub.Visibility = Visibility.Visible;
                OpenSubTrack(nowFileRecord.SubTrack);
            }
        }

        private void menuSubFormatted_Click(object sender, RoutedEventArgs e)
        {
            SetSubFormatted(!usingFormattedSub);
            OpenSubTrack(nowFileRecord.SubTrack);
        }

        private void menuOpenDir_Click(object sender, RoutedEventArgs e)
        {
            string startargs;
            startargs = nowFileRecord.Dirctory;
            Process.Start(@"Explorer.exe", startargs);
        }

        private void menuPlayOverActions_Items_Checked(object sender, RoutedEventArgs e)
        {
            var current = sender as System.Windows.Controls.MenuItem;
            playOverActionMode = (int)(Enum.Parse(typeof(PlayOverActions), current.Tag.ToString()));
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
            zoomMode = (int)(Enum.Parse(typeof(ZoomModes), current.Tag.ToString()));
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
                if (MediaListFiles == null) return;
                if (MediaListFiles.Count == 0) return;
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
            this.Focus();
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

            if (player.Source != null)
            {
                MediaListFiles[nowFileName].mediaFileNode.IsUsing = false;
                MediaListFiles[nowFileName].mediaFileNode.IsRecorded = true;
                if (player.NaturalDuration.HasTimeSpan)
                {
                    nowFileRecord.Position = (int)player.Position.TotalMilliseconds;
                    if (!fileRecords.ContainsKey(nowFileName))
                    {
                        fileRecords.Add(nowFileName, nowFileRecord);
                    }
                }
            }
            if (player.IsPlaying)
            {
                SetVideoMode(false);
            }
            player.Stop();
            player.Source = null;

            if (subItems != null)
            {
                subItems.Clear();
                tbSub.Text = "";
                tbSubUp.Text = tbSub.Text;
                tbSubDown.Text = tbSub.Text;
                tbSubLeft.Text = tbSub.Text;
                tbSubRight.Text = tbSub.Text;
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
            Grid_Mask.Background= new SolidColorBrush((Color)ColorConverter.ConvertFromString("#000000"));
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
                    if (Keyboard.Modifiers == ModifierKeys.Control)
                        Slider_Light.Value -= Slider_Light.Interval;
                    else
                        btnLeft_Click(null, null);
                    break;
                case Key.Right:
                    if (Keyboard.Modifiers == ModifierKeys.Control)
                        Slider_Light.Value += Slider_Light.Interval;
                    else
                        btnRight_Click(null, null);
                    break;
                    //default:
                    //    break;
            }
        }

        private void formWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (player.Source != null)
            {
                if (player.NaturalDuration.HasTimeSpan)
                {
                    nowFileRecord.Position = (int)player.Position.TotalMilliseconds;
                    if (!fileRecords.ContainsKey(nowFileName))
                    {
                        fileRecords.Add(nowFileName, nowFileRecord);
                    }
                }
            }
            list_RootDirctory.Sort();
            SaveConfig();
            //AppConfigHelper.SaveObj(this);
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
            string path = (TreeView_File.SelectedItem as MediaNode).FullName;
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
            int id = MediaListFiles[nowFileName].ID-1;
            if (id <0) return;
            OpenMediaFile(MediaListFiles.ElementAt(id).Key);
        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            int id = MediaListFiles[nowFileName].ID + 1;
            if (id ==MediaListFiles.Count) return;
            OpenMediaFile(MediaListFiles.ElementAt(id).Key);
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

        private void Slider_Light_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (player == null) return;
            if (player.Source == null) return;
            string light = ((int)(255.0 * (1.0 - (double)(Slider_Light.Value / 100)))).ToString("X2");
            Grid_Main.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(string.Format("#{0}000000", light))); 
            textBlock_Light.Text = ((double)Slider_Light.Value / 100).ToString("P2");
            LogInfo("亮度：  " + textBlock_Light.Text);
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
            this.Focus();
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


        private void menuClearRecords_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher.InvokeAsync(() =>
            {
                fileRecords.Clear();
                RefreshFileTree();
                LogInfo("已清空播放进度");
            });
        }

        private void menuClearList_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher.InvokeAsync(() =>
            {
                fileRecords.Clear();
                TreeView_File.Items.Clear();
                list_RootDirctory.Clear();
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

        private void menuCodecInfo_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.MessageBox.Show(
            player.CodecInfo, "媒体信息", MessageBoxButton.OK);
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
                MediaNode node = TreeView_File.SelectedItem as MediaNode;
                if (node.IsFile)
                    DeleteFileNodeRecord(node);
                else
                {
                    DeleteDirNodeRecord(node);
                    TreeView_File.Items.Remove(node);
                    list_RootDirctory.Remove(node.FullName);
                    RefreshFileTree();
                }
            });
        }

        private void menuOpenNodeFolder_Click(object sender, RoutedEventArgs e)
        {
            string startargs;
            MediaNode node = TreeView_File.SelectedItem as MediaNode;
            startargs = node.IsFile ? Path.GetDirectoryName(node.FullName) :node.FullName;
            Process.Start(@"Explorer.exe", startargs);
        }


        private void tbSub_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            tbSubUp.FontSize = tbSub.FontSize;
            tbSubDown.FontSize = tbSub.FontSize;
            tbSubLeft.FontSize = tbSub.FontSize;
            tbSubRight.FontSize = tbSub.FontSize;
        }

        private void Player_MediaEnded(object sender, EventArgs e)
        {
            nowFileRecord.Position = 0;
            Dispatcher.BeginInvoke(new Action(() =>
            {
                Slider_Process.Value = 0;
                switch (playOverActionMode)
                {
                    case 0://playnext
                        btnNext_Click(null, null);
                        break;
                    case 1://playthis
                        {
                            if (player.Source == null) return;
                            OpenMediaFile(nowFileName);
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

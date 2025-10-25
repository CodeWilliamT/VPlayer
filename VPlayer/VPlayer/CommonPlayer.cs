using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using LibVLCSharp;
using LibVLCSharp.WPF;
//using Vlc.DotNet.Wpf;
//using Vlc.DotNet.Core;
//using Vlc.DotNet.Core.Interops.Signatures;
using System.Threading;
using System.Windows.Input;
using System.Security.Policy;
using static System.Net.Mime.MediaTypeNames;
using LibVLCSharp.Shared;
using System.Numerics;

namespace CommonPlayer
{
    internal abstract class BasePlayer : FrameworkElement
    {
        protected FrameworkElement uie;
        public BasePlayer(FrameworkElement obj)
        {
            uie = obj;
        }
        public event EventHandler MediaEnded;

        protected void OnMediaEnded(object obj, EventArgs e)
        {
            MediaEnded?.Invoke(this, e);
        }
        //ui property

        public static List<string> SupportedVideos = new List<string>()
        {
            ".asf", ".avi", ".wm", ".wmp", ".wmv",
            ".ram",".rm",".rmvb",".rpm",".rt",".smil",".scm",".m1v",".m2v",".m2p",".m2ts",".mp2v",".mpe",".mpeg",".mpeg1",".mpeg2",".mpg",".mpv2",".pva",".tp",
            ".tpr",".ts",".m4b",".m4r",".m4p",".m4v",".mp4",".mpeg4",".3g2",".3gp",".3gp2",".3gpp",".mov",".qt",".flv",".f4v",".swf",".hlv",".vob",
            ".amv",".csf",".divx",".evo",".mkv",".mod",".pmp",".vp6",".bik",".mts",".xlmv",".ogm",".ogv",".ogx",
            ".aac",".ac3",".acc",".aiff",".ape",".au",".cda",".dts",".flac",".m1a",".m2a",".m4a",".mka",".mp2",/*".mp3",*/".mpa",".mpc",".ra",".tta",".wav",".wma",".wv",".mid",".midi",
            ".ogg",".oga",".dvd",".vqf"
        };
        public virtual double ActualHeight { get => uie.ActualHeight; }
        public double ActualWidth { get => uie.ActualWidth; }
        public double Height { get => uie.Height; set => uie.Height = value; }
        public double Width { get => uie.Width; set => uie.Width = value; }
        //player property
        public TimeSpan Position { get; set; }
        public bool IsPlaying { get; set; }
        public System.Windows.Duration NaturalDuration { get; }
        public int NaturalVideoWidth { get; }
        public int NaturalVideoHeight { get; }
        public Uri Source { get; set; }
        public double SpeedRatio { get; set; }
        public int Volume { get; set; }

        public abstract void Open(string fileName);
        public abstract void Play();
        public abstract void Pause();
        public abstract void Stop();
        public abstract void Close();
        public virtual void GetVideoResolution() { }

    }

    internal class LibVLCPlayer : BasePlayer
    {
        private LibVLC libvlc;
        private VideoView vlc;
        private LibVLCSharp.Shared.MediaPlayer player;
        private Media media;
        private DirectoryInfo libDirectory = new DirectoryInfo(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "VLC"));
        private int naturalVideoWidth = 1280;
        private int naturalVideoHeight = 720;

        public LibVLCPlayer(FrameworkElement obj) : base(obj)
        {
            vlc = uie as VideoView;
            //初始化播放器
            string[] options = new string[]{
            //":--no-overlay", // 关闭硬件加速
            //":--no-video-hwaccels", // 关闭硬件加速
            //":--no-ignore-config", // 不无视配置
            //":--avcodec-hw=none", // 禁用AVCodec硬件解码器,使用软件解码器进行播放。
            //":‌--ffmpeg-hw=none", // 禁用FFmpeg硬件解码器
            //":video-chroma=RV32", // 使用适合硬件加速的色深格式
            //":hwdec=auto", // 自动选择硬件解码器
            //":--no-qt-video-autoresize ",//不缩放界面至原生视频大小
            //":--no-media-library", // 不用媒体库
            //":network-caching=1000", // 设置网络缓存时间
            //":file-caching=3600000", // 设置文件缓存时间
            //":--caching=3600000", // 设置缓存时间
            //":vout-ios-color-gamut=BT2020", // 启用硬件加速（根据需要选择合适的选项）
            //"--vout=gl",//视频输出为glwin32
            //"--ffmpeg-hw", // 启用FFmpeg硬件解码器，利用GPU加速解码。 
            ":--avcodec-hw‌=any",//启用avcodec硬件解码器，支持Intel、NVIDIA等GPU加速。 
            //"--avcodec-hw=DXVA2",//‌NVIDIA GPU硬件加速
            //":directx-device=any", // 使用DirectX硬件加速（如果可用）
            //":qsv-async-depth=64", // 针对Intel Quick Sync Video的额外配置
            //":--avcodec-hw=vaapi", // 启用VA-API硬件解码器。VLC可以利用VA-API技术进行硬件解码，提高解码效率。
            //":vaapi-device=/dev/dri/renderD128", // 针对VA-API的额外配置（Linux专用）
            //":vaapi-display=:0", // 指定VA-API的显示设备（Linux专用）
            //":deinterlace=-1", // 启用去隔行处理以改善视频质量
            //":x264-params=qp=24", // 设置x264编码参数（如果需要）
            //":avcodec-skiploopfilter=nonkey", // 跳过非关键帧的循环过滤器以提高性能
            //":avcodec-skipframe=nonkey", // 跳过非关键帧以提高性能
            //":avcodec-skipidct=none", // 禁用IDCT以减少CPU使用率（在某些情况下）
            //":avcodec-lowres=1", // 降低分辨率以提高性能（在某些情况下）
            //":avcodec-dr=1", // 使用直接渲染以提高性能（在某些编解码器中）
            //":avcodec-hwaccel=auto", // 自动选择硬件加速编解码器（在某些情况下）
            //":avcodec-hwaccel-auto=1", // 启用自动硬件加速选择（在某些情况下）
            //":avcodec-hwaccel-device=/dev/dri/renderD128", // 指定硬件加速设备（Linux专用）
            //":qsv-async-depth=64", // Intel Quick Sync Video的额外配置（在某些情况下）
            //":qsv-device=auto", // 自动选择QSV设备（在某些情况下）
            //"--directx-use-sysmem", //  在系统内存中使用视频缓冲区
            //":qsv-fallback-to-sw=0", // 不回退到软件解码
            //"--no-sub-autoload", // 别自动加载字幕
            
            "--no-sub-autodetect-file" // 别自动加载字幕文件
            };
            libvlc = new LibVLC(options);
            player = new LibVLCSharp.Shared.MediaPlayer(libvlc);
            vlc.MediaPlayer = player;
            player.EndReached += TriggerMediaEnded;
        }
        public TimeSpan Position { get => TimeSpan.FromMilliseconds(player.Time); set => player.Time = (long)value.TotalMilliseconds; }
        public bool IsPlaying
        {
            get => player.IsPlaying;
            set
            {
                if (value)
                    Play();
                else
                    Pause();
            }
        }
        public System.Windows.Duration NaturalDuration { get => TimeSpan.FromMilliseconds(player.Length); }
        public int NaturalVideoWidth { get => naturalVideoWidth; }
        public int NaturalVideoHeight { get => naturalVideoHeight; }
        public Uri Source
        {
            get => (media == null) ? null : (new Uri(media.Mrl));
            set
            {
                if (value == null)
                {
                    media.Dispose();
                    media = null;
                }
                else
                    media = new Media(libvlc, value);
            }
        }
        public double SpeedRatio { get => player.Rate; set => player.SetRate((float)value); }
        public int Volume { get => player.Volume; set => player.Volume = value; }


        public override void Open(string fileName)
        {
            try
            {
                if (fileName == null || fileName == "") return;
                if (!File.Exists(fileName)) return;
                if (media != null)
                {
                    this.Stop();
                    this.Close();
                }
                FileInfo fileInfo = new FileInfo(fileName);
                //判断后缀是否支持
                if (!SupportedVideos.Contains(fileInfo.Extension.ToLower()))
                {
                    return;
                }
                media = new Media(libvlc, new Uri(fileName));
                player.Media = media;
                player.Play();
                Dispatcher.InvokeAsync(() => {
                    vlc.Opacity = 1;
                });

            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(String.Format("Can not open this File. {0}", ex.ToString()));
            }
        }
        public override void Play()
        {
            player.Play();
        }
        public override void Pause()
        {
            player.SetPause(true);
        }
        public override void Stop()
        {
            player.Media.Dispose();
            Dispatcher.InvokeAsync(() => {
                vlc.Opacity = 0;
            });
        }
        public override void Close()
        {
            if (media != null)
            {
                this.Stop();
            }
        }


        public override void GetVideoResolution()
        {
            if (player.VideoTrackCount == 0) return;
            MediaTrack[] tracks = media.Tracks; // 获取媒体轨道信息
            MediaTrack track = tracks.FirstOrDefault(t => t.TrackType == TrackType.Video); // 查找视频轨道
            VideoTrack videoTrack = track.Data.Video;
            naturalVideoWidth = (int)videoTrack.Height;
            naturalVideoHeight = (int)videoTrack.Height;
        }
        #region private fuc
        private void TriggerMediaEnded(object obj, EventArgs e)
        {
            OnMediaEnded(this, EventArgs.Empty);
        }
        #endregion
    }


    internal class MSPlayer : BasePlayer
    {
        private MediaElement player;

        public MSPlayer(FrameworkElement obj) : base(obj)
        {
            player = uie as MediaElement;
            player.MediaEnded += TriggerMediaEnded;
        }
        public TimeSpan Position { get => player.Position; set => player.Position = value; }
        public bool IsPlaying
        {
            get => GetMediaState(player) == MediaState.Play;
            set
            {
                if (value)
                    Play();
                else
                    Pause();
            }
        }
        public System.Windows.Duration NaturalDuration { get => player.NaturalDuration; }
        public int NaturalVideoWidth { get => player.NaturalVideoWidth; }
        public int NaturalVideoHeight { get => player.NaturalVideoHeight; }
        public Uri Source { get => player.Source; set => player.Source = value; }
        public double SpeedRatio { get => player.SpeedRatio; set => player.SpeedRatio = value; }
        public int Volume { get => (int)(player.Volume * 100); set => player.Volume = value / 100.0d; }
        public override void Open(string fileName)
        {
            try
            {
                if (fileName == null || fileName == "") return;
                if (!File.Exists(fileName)) return;
                if (player.Source != null)
                {
                    player.Stop();
                    player.Close();
                }
                FileInfo fileInfo = new FileInfo(fileName);
                //判断后缀是否支持
                if (!SupportedVideos.Contains(fileInfo.Extension.ToLower()))
                {
                    return;
                }
                player.Source = new Uri(fileName);
                player.LoadedBehavior = MediaState.Manual;
                player.Play();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(String.Format("Can not open this File. {0}", ex.ToString()));
            }
        }
        public override void Play()
        {
            player.Play();
        }
        public override void Pause()
        {
            player.Pause();
        }
        public override void Stop()
        {
            player.Stop();
        }
        public override void Close()
        {
            player.Close();
        }
        #region private fuc
        private MediaState GetMediaState(MediaElement myMedia)
        {
            FieldInfo hlp = typeof(MediaElement).GetField("_helper", BindingFlags.NonPublic | BindingFlags.Instance);
            object helperObject = hlp.GetValue(myMedia);
            FieldInfo stateField = helperObject.GetType().GetField("_currentState", BindingFlags.NonPublic | BindingFlags.Instance);
            MediaState state = (MediaState)stateField.GetValue(helperObject);
            return state;
        }
        private void TriggerMediaEnded(object obj, RoutedEventArgs e)
        {
            OnMediaEnded(this, EventArgs.Empty);
        }
        #endregion
    }

    //internal class VLCPlayer : BasePlayer
    //{
    //    private VlcControl vlc;
    //    private VlcMediaPlayer player;
    //    private VlcMedia media;
    //    private DirectoryInfo libDirectory = new DirectoryInfo(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "VLC"));
    //    private int naturalVideoWidth=1280;
    //    private int naturalVideoHeight=720;

    //    public VLCPlayer(FrameworkElement obj) : base(obj)
    //    {
    //        vlc = uie as VlcControl;
    //        //初始化播放器
    //        string[] options = new string[]{
    //        //":--no-overlay", // 关闭硬件加速
    //        //":--no-video-hwaccels", // 关闭硬件加速
    //        //":--no-ignore-config", // 不无视配置
    //        //":--avcodec-hw=none", // 禁用AVCodec硬件解码器,使用软件解码器进行播放。
    //        //":‌--ffmpeg-hw=none", // 禁用FFmpeg硬件解码器
    //        //":video-chroma=RV32", // 使用适合硬件加速的色深格式
    //        //":hwdec=auto", // 自动选择硬件解码器
    //        //":--no-qt-video-autoresize ",//不缩放界面至原生视频大小
    //        //":--no-media-library", // 不用媒体库
    //        //":network-caching=1000", // 设置网络缓存时间
    //        //":file-caching=3600000", // 设置文件缓存时间
    //        //":--caching=3600000", // 设置缓存时间
    //        //":vout-ios-color-gamut=BT2020", // 启用硬件加速（根据需要选择合适的选项）
    //        //"--vout=gl",//视频输出为glwin32
    //        //"--ffmpeg-hw", // 启用FFmpeg硬件解码器，利用GPU加速解码。 
    //        ":--avcodec-hw‌=any",//启用avcodec硬件解码器，支持Intel、NVIDIA等GPU加速。 
    //        //"--avcodec-hw=DXVA2",//‌NVIDIA GPU硬件加速
    //        //":directx-device=any", // 使用DirectX硬件加速（如果可用）
    //        //":qsv-async-depth=64", // 针对Intel Quick Sync Video的额外配置
    //        //":--avcodec-hw=vaapi", // 启用VA-API硬件解码器。VLC可以利用VA-API技术进行硬件解码，提高解码效率。
    //        //":vaapi-device=/dev/dri/renderD128", // 针对VA-API的额外配置（Linux专用）
    //        //":vaapi-display=:0", // 指定VA-API的显示设备（Linux专用）
    //        //":deinterlace=-1", // 启用去隔行处理以改善视频质量
    //        //":x264-params=qp=24", // 设置x264编码参数（如果需要）
    //        //":avcodec-skiploopfilter=nonkey", // 跳过非关键帧的循环过滤器以提高性能
    //        //":avcodec-skipframe=nonkey", // 跳过非关键帧以提高性能
    //        //":avcodec-skipidct=none", // 禁用IDCT以减少CPU使用率（在某些情况下）
    //        //":avcodec-lowres=1", // 降低分辨率以提高性能（在某些情况下）
    //        //":avcodec-dr=1", // 使用直接渲染以提高性能（在某些编解码器中）
    //        //":avcodec-hwaccel=auto", // 自动选择硬件加速编解码器（在某些情况下）
    //        //":avcodec-hwaccel-auto=1", // 启用自动硬件加速选择（在某些情况下）
    //        //":avcodec-hwaccel-device=/dev/dri/renderD128", // 指定硬件加速设备（Linux专用）
    //        //":qsv-async-depth=64", // Intel Quick Sync Video的额外配置（在某些情况下）
    //        //":qsv-device=auto", // 自动选择QSV设备（在某些情况下）
    //        //"--directx-use-sysmem", //  在系统内存中使用视频缓冲区
    //        //":qsv-fallback-to-sw=0", // 不回退到软件解码
    //        //"--no-sub-autoload", // 别自动加载字幕

    //        "--no-sub-autodetect-file" // 别自动加载字幕文件
    //        };
    //        vlc.SourceProvider.CreatePlayer(libDirectory, options);
    //        player = vlc.SourceProvider.MediaPlayer;
    //        player.EndReached += TriggerMediaEnded;
    //    }
    //    public TimeSpan Position { get => TimeSpan.FromMilliseconds(player.Time); set => player.Time = (long)value.TotalMilliseconds; }
    //    public bool IsPlaying
    //    {
    //        get => player.IsPlaying();
    //        set
    //        {
    //            if (value)
    //                Play();
    //            else
    //                Pause();
    //        }
    //    }
    //    public System.Windows.Duration NaturalDuration { get => TimeSpan.FromMilliseconds(player.Length); }
    //    public int NaturalVideoWidth { get => naturalVideoWidth; }
    //    public int NaturalVideoHeight { get => naturalVideoHeight; }
    //    public Uri Source 
    //    { 
    //        get => (media==null)? null:(new Uri(media.Mrl)); 
    //        set
    //        {
    //            if (value == null)
    //            {
    //                media.Dispose();
    //                media= null;
    //            }
    //            else
    //                player.SetMedia(value); 
    //        }  
    //    }
    //    public double SpeedRatio { get => player.Rate; set => player.Rate = (float)value; }
    //    public int Volume { get => player.Audio.Volume; set => player.Audio.Volume = value; }


    //    public override void Open(string fileName)
    //    {
    //        try
    //        {
    //            if (fileName == null || fileName == "") return;
    //            if (!File.Exists(fileName)) return;
    //            if (media != null)
    //            {
    //                this.Stop();
    //                this.Close();
    //            }
    //            FileInfo fileInfo = new FileInfo(fileName);
    //            //判断后缀是否支持
    //            if (!SupportedVideos.Contains(fileInfo.Extension.ToLower()))
    //            {
    //                return;
    //            }
    //            player.SetMedia(new Uri(fileName));
    //            player.Play();
    //            media = player.GetMedia();
    //            vlc.Opacity = 1;
    //        }
    //        catch (Exception ex)
    //        {
    //            System.Windows.MessageBox.Show(String.Format("Can not open this File. {0}", ex.ToString()));
    //        }
    //    }
    //    public override void Play()
    //    {
    //        player.Play();
    //    }
    //    public override void Pause()
    //    {
    //        player.SetPause(true);
    //    }
    //    public override void Stop()
    //    {
    //        media.Dispose();
    //        player.ResetMedia();
    //        vlc.Opacity = 0;
    //    }
    //    public override void Close()
    //    {
    //        if (media != null)
    //        {
    //            this.Stop();
    //        }
    //    }


    //    public override void GetVideoResolution()
    //    {
    //        Vlc.DotNet.Core.Interops.MediaTrack[] tracks = media.Tracks; // 获取媒体轨道信息
    //        var videoTrack = tracks.FirstOrDefault(t => t.Type == MediaTrackTypes.Video); // 查找视频轨道
    //        if (videoTrack == null) return;
    //        var videoTrackInfo = videoTrack.TrackInfo as Vlc.DotNet.Core.Interops.VideoTrack;
    //        if (videoTrack != null)
    //        {
    //            naturalVideoWidth = (int)videoTrackInfo.Width;
    //            naturalVideoHeight = (int)videoTrackInfo.Height;
    //        }
    //    }
    //    #region private fuc
    //    private void TriggerMediaEnded(object obj, VlcMediaPlayerEndReachedEventArgs e)
    //    {
    //        OnMediaEnded(this, EventArgs.Empty);
    //    }
    //    #endregion
    //}
}

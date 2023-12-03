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

namespace VPlayer
{
    internal abstract class BasePlayer:FrameworkElement
    {
        protected FrameworkElement uie;
        public BasePlayer(FrameworkElement obj)
        {
            uie = obj;
        }
        //ui property

        public double ActualHeight { get => uie.ActualHeight;}
        public double ActualWidth { get => uie.ActualWidth;}
        public double Height { get=>uie.Height; set => uie.Height=value; }
        public double Width { get => uie.Width; set => uie.Width = value; }
        //player property
        public TimeSpan Position { get; set; }
        public bool IsPlaying { get; set; }
        public System.Windows.Duration NaturalDuration { get; }
        public int NaturalVideoHeight { get; }
        public int NaturalVideoWidth { get; }
        public Uri Source { get; set; }
        public double SpeedRatio { get; set; }
        public double Volume { get; set; }
        public List<string> SupportedVideos = new List<string>()
        {
            ".asf", ".avi", ".wm", ".wmp", ".wmv",
            ".ram",".rm",".rmvb",".rpm",".rt",".smil",".scm",".m1v",".m2v",".m2p",".m2ts",".mp2v",".mpe",".mpeg",".mpeg1",".mpeg2",".mpg",".mpv2",".pva",".tp",
            ".tpr",".ts",".m4b",".m4r",".m4p",".m4v",".mp4",".mpeg4",".3g2",".3gp",".3gp2",".3gpp",".mov",".qt",".flv",".f4v",".swf",".hlv",".vob",
            ".amv",".csf",".divx",".evo",".mkv",".mod",".pmp",".vp6",".bik",".mts",".xlmv",".ogm",".ogv",".ogx",
            ".aac",".ac3",".acc",".aiff",".ape",".au",".cda",".dts",".flac",".m1a",".m2a",".m4a",".mka",".mp2",/*".mp3",*/".mpa",".mpc",".ra",".tta",".wav",".wma",".wv",".mid",".midi",
            ".ogg",".oga",".dvd",".vqf"
        };

        public abstract void Open(string fileName);
        public abstract void Play();
        public abstract void Pause();
        public abstract void Stop();
        public abstract void Close();

    }
    internal class MSPlayer : BasePlayer
    {
        private MediaElement player;

        public MSPlayer(FrameworkElement obj) : base(obj)
        {
            player = uie as MediaElement;
        }
        public TimeSpan Position { get=>player.Position; set=>player.Position=value; }
        public bool IsPlaying { get=> GetMediaState(player) == MediaState.Play; 
            set {
                if (value)
                    Play();
                else
                    Pause();
            }
        }
        public System.Windows.Duration NaturalDuration { get => player.NaturalDuration; }
        public int NaturalVideoHeight { get => player.NaturalVideoHeight;}
        public int NaturalVideoWidth { get => player.NaturalVideoWidth; }
        public Uri Source { get => player.Source; set => player.Source = value; }
        public double SpeedRatio { get => player.SpeedRatio; set => player.SpeedRatio = value; }
        public double Volume { get => player.Volume; set => player.Volume = value; }

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
        public override void Play() { 
            player.Play();
        }
        public override void Pause() { 
            player.Pause();
        }
        public override void Stop() { 
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
        #endregion
    }
}

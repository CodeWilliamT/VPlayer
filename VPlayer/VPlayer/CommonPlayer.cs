using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace VPlayer
{
    internal abstract class BasicPlayer
    {
        public BasicPlayer() {
            
        }
        public object playerObject;
        public Type playerType;

        public abstract void Open(FileInfo fi);
        public abstract void Play();
        public abstract void Pause();
        public abstract void Stop();

    }
    //internal class MSPlayer:BasicPlayer
    //{
    //    public MSPlayer()
    //    {
    //        playerObject=
    //    }
    //    public object playerObject;

    //    public abstract void Open(FileInfo fi);
    //    public abstract void Play();
    //    public abstract void Pause();
    //    public abstract void Stop();

    //}
}

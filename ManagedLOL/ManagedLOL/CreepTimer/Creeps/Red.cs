using System;
using System.Windows.Media.Imaging;

namespace ManagedLOL.CreepTimer.Creeps
{
    internal class Red : ICreep
    {
        public TimeSpan InitialTime
        {
            get { return new TimeSpan(0, 1, 55); }
        }

        public TimeSpan MaxTime
        {
            get { return new TimeSpan(0, 5, 0); }
        }

        public BitmapImage Image
        {
            get { return new BitmapImage(new Uri(@"pack://application:,,,/ManagedLOL;component/images/red.png")); }
        }
    }
}
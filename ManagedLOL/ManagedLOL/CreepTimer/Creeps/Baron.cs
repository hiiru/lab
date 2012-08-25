using System;
using System.Windows.Media.Imaging;

namespace ManagedLOL.CreepTimer.Creeps
{
    internal class Baron : ICreep
    {
        public TimeSpan InitialTime
        {
            get { return new TimeSpan(0, 15, 0); }
        }

        public TimeSpan MaxTime
        {
            get { return new TimeSpan(0, 7, 0); }
        }

        public BitmapImage Image
        {
            get { return new BitmapImage(new Uri(@"pack://application:,,,/ManagedLOL;component/images/baron.png")); }
        }
    }
}
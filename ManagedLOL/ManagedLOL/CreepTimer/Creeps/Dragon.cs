using System;
using System.Windows.Media.Imaging;

namespace ManagedLOL.CreepTimer.Creeps
{
    internal class Dragon : ICreep
    {
        public TimeSpan InitialTime
        {
            get { return new TimeSpan(0, 2, 30); }
        }

        public TimeSpan MaxTime
        {
            get { return new TimeSpan(0, 6, 0); }
        }

        public BitmapImage Image
        {
            get { return new BitmapImage(new Uri(@"pack://application:,,,/ManagedLOL;component/images/dragon.png")); }
        }
    }
}
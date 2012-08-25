using System;
using System.Windows.Media.Imaging;

namespace ManagedLOL.CreepTimer.Creeps
{
    internal class Wolf : ICreep
    {
        public TimeSpan InitialTime
        {
            get { return new TimeSpan(0, 1, 40); }
        }

        public TimeSpan MaxTime
        {
            get { return new TimeSpan(0, 1, 40); }
        }

        public BitmapImage Image
        {
            get { return new BitmapImage(new Uri(@"pack://application:,,,/ManagedLOL;component/images/wolf.png")); }
        }
    }
}
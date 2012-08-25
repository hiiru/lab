using System;
using System.Windows.Media.Imaging;

namespace ManagedLOL.CreepTimer.Creeps
{
    internal interface ICreep
    {
        TimeSpan InitialTime { get; }

        TimeSpan MaxTime { get; }

        BitmapImage Image { get; }
    }
}
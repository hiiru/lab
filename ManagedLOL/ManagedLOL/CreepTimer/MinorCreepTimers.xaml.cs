using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using ManagedLOL.CreepTimer.Creeps;

namespace ManagedLOL
{
    /// <summary>
    /// Interaction logic for MinorCreepTimers.xaml
    /// </summary>
    public partial class MinorCreepTimers : UserControl
    {
        private List<UserControl> creeps = new List<UserControl>();

        public MinorCreepTimers(bool reverse = false)
        {
            InitializeComponent();
            creeps.Add(new CreepControl<Blue>());
            creeps.Add(new CreepControl<Wolf>());
            creeps.Add(new CreepControl<Wraith>());
            creeps.Add(new CreepControl<Red>());
            creeps.Add(new CreepControl<Golem>());
            if (reverse)
            {
                this.Padding = new Thickness(5, 0, 1, 0);
                creeps.Reverse();
            }
            else
                this.Padding = new Thickness(1, 0, 5, 0);
            this.Width = 6;
            foreach (UserControl creep in creeps)
            {
                panel.Children.Add(creep);
                this.Width += creep.Width;
            }
        }
    }
}
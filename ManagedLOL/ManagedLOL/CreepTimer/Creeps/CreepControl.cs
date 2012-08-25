using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace ManagedLOL.CreepTimer.Creeps
{
    internal class CreepControl<T> : UserControl where T : ICreep, new()
    {
        private double _minOpacity = 0.6;
        private double _maxOpacity = 0.8;

        #region Constructor and UI Events

        private Label lblTimer;
        private Image imgIcon;

        public CreepControl()
        {
            this.Height = this.Width = 55;
            T creep = new T();

            //load creep image or fallback
            BitmapImage img = creep.Image ?? new BitmapImage(new Uri(@"pack://application:,,,/ManagedLOL;component/images/red.png"));
            _maxTime = creep.MaxTime;
            this.MouseDown += control_MouseDown;
            this.MouseDoubleClick += control_MouseDoubleClick;

            //this.Background = Brushes.Transparent;
            Grid grid = new Grid();
            imgIcon = new Image();
            imgIcon.Stretch = Stretch.Uniform;
            imgIcon.Source = img;
            imgIcon.Opacity = _maxOpacity;
            lblTimer = new Label();
            lblTimer.Visibility = System.Windows.Visibility.Hidden;
            lblTimer.FontSize = 18;
            lblTimer.Foreground = Brushes.OrangeRed;
            lblTimer.FontWeight = FontWeights.Bold;
            DropShadowEffect myDropShadowEffect = new DropShadowEffect();
            myDropShadowEffect.Color = Colors.Black;
            myDropShadowEffect.Direction = 320;
            myDropShadowEffect.ShadowDepth = 0;
            myDropShadowEffect.Opacity = 0.9;

            lblTimer.Effect = myDropShadowEffect;
            lblTimer.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Center;
            lblTimer.VerticalContentAlignment = System.Windows.VerticalAlignment.Center;
            lblTimer.Height = this.Height;
            lblTimer.Width = this.Width;
            grid.Children.Add(imgIcon);
            grid.Children.Add(lblTimer);
            this.AddChild(grid);
            Panel.SetZIndex(lblTimer, 2);
            Panel.SetZIndex(imgIcon, 1);
        }

        private void control_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Right)
                stop();
            else
                start();
        }

        private void control_MouseDoubleClick(object sender, RoutedEventArgs e)
        {
            this.start(true);
        }

        #endregion Constructor and UI Events

        private DispatcherTimer _timer = null;
        private TimeSpan _maxTime;

        public TimeSpan MaxTime { get { return _maxTime; } }

        private TimeSpan _timeLeft = new TimeSpan(0);

        public void start(bool force = false)
        {
            //avoid timer reset
            if (_timer != null)
            {
                if (force)
                {
                    _timer.Stop();
                    _timer = null;
                }
                else if (_timer.IsEnabled) return;
            }
            _timeLeft = new TimeSpan(MaxTime.Ticks);
            UpdateTimer();
            _timer = new DispatcherTimer();
            _timer.Interval = new TimeSpan(0, 0, 1);
            _timer.Tick += TimerTick;
            _timer.Start();
        }

        public void stop()
        {
            if (_timer != null)
            {
                _timer.Stop();
                _timer = null;
            }
            ClearTimer();
        }

        private void UpdateTimer()
        {
            if (!lblTimer.IsVisible)
            {
                lblTimer.Visibility = System.Windows.Visibility.Visible;
                imgIcon.Opacity = _minOpacity;
            }
            lblTimer.Content = string.Format("{0:mm}:{0:ss}", _timeLeft);
        }

        private void ClearTimer()
        {
            lblTimer.Visibility = System.Windows.Visibility.Hidden;
            imgIcon.Opacity = _maxOpacity;
        }

        private void TimerTick(object sender, EventArgs args)
        {
            _timeLeft = _timeLeft.Subtract(_timer.Interval);
            if (_timeLeft.TotalSeconds <= 0)
                stop();
            else
                UpdateTimer();
        }

        public TimeSpan TimeLeft { get { return _timeLeft; } }
    }
}
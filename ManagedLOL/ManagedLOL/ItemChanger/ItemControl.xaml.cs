using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ManagedLOL.Configuration;
using ManagedLOL.ItemChanger.Items;

namespace ManagedLOL.ItemChanger
{
    /// <summary>
    /// Interaction logic for ItemControl.xaml
    /// </summary>
    public partial class ItemControl : UserControl
    {
        public Item Item
        {
            get { return this.DataContext as Item; }
            set
            {
                if (IsMainSlot && value == null)
                {
                    value = new Item(0, "empty slot" + MainSlotNumber, SettingsHelper.LolPathImagesItems + @"\EmptyIcon.png");
                }
                this.DataContext = value; if (IsMainSlot) ttGrid.Content = value.Name;
            }
        }

        public bool IsMainSlot
        {
            get { return (bool)this.Resources["IsMainSlot"]; }
            set { this.Resources["IsMainSlot"] = value; SetMainSlot(value); }
        }

        public int MainSlotNumber { get; set; }

        private void SetMainSlot(bool visible)
        {
            if (visible)
            {
                this.BorderBrush = Brushes.DarkGoldenrod;
                this.BorderThickness = new System.Windows.Thickness(1);
                this.AllowDrop = true;
            }
            else
            {
                this.BorderBrush = null;
                this.BorderThickness = new System.Windows.Thickness(0);
                this.AllowDrop = false;
            }
        }

        public ItemControl()
        {
            this.Resources["MainSlot"] = SettingsHelper.LolPathImagesItems + @"\cornerMask.png";
            this.Resources["IsMainSlot"] = false;
            InitializeComponent();
        }

        private void Grid_MouseMove(object sender, MouseEventArgs e)
        {
            ttGrid.Placement = System.Windows.Controls.Primitives.PlacementMode.Relative;
            ttGrid.HorizontalOffset = e.GetPosition((IInputElement)sender).X + 10;
            ttGrid.VerticalOffset = e.GetPosition((IInputElement)sender).Y + 10;
        }

        private Point startPoint;

        private void UserControl_MouseMove(object sender, MouseEventArgs e)
        {
            // Get the current mouse position
            Point mousePos = e.GetPosition(null);
            Vector diff = startPoint - mousePos;

            if (e.LeftButton == MouseButtonState.Pressed &&
                (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
            {
                // Get the dragged ItemControl
                ItemControl itemControl = sender as ItemControl;

                // Find the data behind the ListViewItem
                Item item = itemControl.Item;

                // Initialize the drag & drop operation
                DataObject dragData = new DataObject("item", item);
                if (IsMainSlot)
                {
                    dragData.SetData("MainSlot", itemControl);
                }
                DragDrop.DoDragDrop(itemControl, dragData, IsMainSlot ? DragDropEffects.Move : DragDropEffects.Copy);
            }
        }

        private void UserControl_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Store the mouse position
            startPoint = e.GetPosition(null);
        }

        private void UserControl_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("item"))
            {
                if (e.Data.GetDataPresent("MainSlot"))
                {
                    ItemControl itemControl = e.Data.GetData("MainSlot") as ItemControl;
                    itemControl.Item = Item;

                    //ttGrid.Content = "";
                }
                Item = e.Data.GetData("item") as Item;
            }
            e.Effects = DragDropEffects.None;
        }

        private void UserControl_DragOver(object sender, DragEventArgs e)
        {
            if (!IsMainSlot && e.Data.GetDataPresent("item"))
            {
                Cursor = Cursors.No;
            }
        }
    }
}
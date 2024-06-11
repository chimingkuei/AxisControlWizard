using MotionControllers.Motion;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MotionControllers.UI
{
    /// <summary>
    /// UserControl1.xaml 的互動邏輯
    /// </summary>
    public partial class MotionControlSimple : UserControl
    {
        //TODO : DataContext 接收 MotionController 並新增CurrentAxis Property
        public MotionControlSimple()
        {
            InitializeComponent();
            dataGrid.AutoGenerateColumns = false;
        }

        private void Context_Delete(object sender, RoutedEventArgs e)
        {
            //Get the clicked MenuItem
            var menuItem = (MenuItem)sender;

            //Get the ContextMenu to which the menuItem belongs
            var contextMenu = (ContextMenu)menuItem.Parent;

            //Find the placementTarget
            var item = (DataGrid)contextMenu.PlacementTarget;

            //Get the underlying item, that you cast to your object that is bound
            //to the DataGrid (and has subject and state as property)
            var toDeleteFromBindedList = (NamedLocation)item.SelectedCells[0].Item;

            //Remove the toDeleteFromBindedList object from your ObservableCollection
            if(DataContext is AxisMotionController cntlr)
            {
              
                cntlr.PointTable.Remove(toDeleteFromBindedList);
            }
        }

        private void CreateNewAbsLocation(object sender, RoutedEventArgs e)
        {
            if (dataGrid.ItemsSource is KeyedLocations locations)
            {
                var cntlr = (DataContext as AxisMotionController);
                var item = new NamedLocation() { Name = "NewPosition", Location = cntlr.Position };
                locations.Add(item);
                dataGrid.ScrollIntoView(item);
            }
        }

        private void btnView_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is AxisMotionController cntlr)
                cntlr.MoveAbsolute((dataGrid.SelectedItem as NamedLocation).Location);
        }

        private async void ButtonMoveClicked(object sender, RoutedEventArgs e)
        {
            if (DataContext is AxisMotionController cntlr)
            {
                try
                {
                    
                    switch ((sender as Button).Name)
                    {
                        case "BTN_Home":
                            
                            _ = cntlr.MoveHome();
                            //cntlr.WaitMotionIOStatus(axisID, MotionIOStatus.ORG)
                            break;
                        //case "BTN_JOE_Foward":
                        //case "BTN_JOG_Backward":
                        //    this will implement in mouse preview(up, down) event
                        //     break;
                        case "BTN_REL_Forward":
                            cntlr.MoveRelative(cntlr.DistanceRelative);
                            break;
                        case "BTN_REL_Backward":
                            cntlr.MoveRelative(-cntlr.DistanceRelative);
                            break;
                    }
                }
                catch(Exception ex)
                {

                }
            }
            else
                MessageBox.Show("Needs to asign DataContext of this component.");
        }

        private void BTN_JOE_Foward_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && DataContext is AxisMotionController cntlr )
            {
                try
                {
                    if (sender == BTN_JOE_Foward)
                        cntlr.JogStart( AxisDirection.Plus);
                    else if (sender == BTN_JOG_Backward)
                        cntlr.JogStart( AxisDirection.Minus);
                }
                catch(Exception ex)
                {

                }
            }
        }

        private void BTN_JOE_Foward_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && DataContext is AxisMotionController cntlr)
            {
                try
                {
                    cntlr.StopJogMotion();
                }
                catch (Exception ex)
                {

                }
            }
        }
    }

   
}

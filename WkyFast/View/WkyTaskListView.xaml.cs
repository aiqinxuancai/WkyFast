using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace WkyFast.View
{
    /// <summary>
    /// WkyTaskListView.xaml 的交互逻辑
    /// </summary>
    public partial class WkyTaskListView : UserControl
    {
        public WkyTaskListView()
                    : this(new ObservableCollection<WkyApiSharp.Service.Model.RemoteDownloadList.Task>())
        { }

        public WkyTaskListView(ObservableCollection<WkyApiSharp.Service.Model.RemoteDownloadList.Task> viewModel)
        {
            InitializeComponent();


            //主动刷新？

            this.ViewModel = viewModel;
        }

        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(ObservableCollection<WkyApiSharp.Service.Model.RemoteDownloadList.Task>), typeof(WkyTaskListView));

        public ObservableCollection<WkyApiSharp.Service.Model.RemoteDownloadList.Task> ViewModel
        {
            get { return (ObservableCollection<WkyApiSharp.Service.Model.RemoteDownloadList.Task>)GetValue(ViewModelProperty); }
            set
            {
                SetValue(ViewModelProperty, value);
                if (value != null && value.Count > 0)
                {
                    MainDataGrid.SelectedItem = value.First();
                }
            }
        }

        private void UIElement_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (!e.Handled)
            {
                e.Handled = true;
                var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
                eventArg.RoutedEvent = MouseWheelEvent;
                eventArg.Source = sender;
                var parent = ((Control)sender).Parent as UIElement;
                parent?.RaiseEvent(eventArg);
            }
        }
    }
}

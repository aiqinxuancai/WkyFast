using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WkyFast.Service.Model;

namespace WkyFast.View
{
    /// <summary>
    /// WkyTaskListView.xaml 的交互逻辑
    /// </summary>
    public partial class WkySubscriptionListView : UserControl
    {
        public WkySubscriptionListView()
                    : this(new ObservableCollection<SubscriptionModel>())
        { }

        public WkySubscriptionListView(ObservableCollection<SubscriptionModel> viewModel)
        {
            InitializeComponent();


            //主动刷新？

            this.ViewModel = viewModel;
        }

        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(ObservableCollection<SubscriptionModel>), typeof(WkySubscriptionListView));

        public ObservableCollection<SubscriptionModel> ViewModel
        {
            get { return (ObservableCollection<SubscriptionModel>)GetValue(ViewModelProperty); }
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

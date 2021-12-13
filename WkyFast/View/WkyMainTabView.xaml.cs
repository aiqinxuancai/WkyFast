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
using WkyFast.View.Model;

namespace WkyFast.View
{
    /// <summary>
    /// WkyMainTabView.xaml 的交互逻辑
    /// </summary>
    public partial class WkyMainTabView : UserControl
    {
        MainTabItemModel? _lastSelectedConfig = null;

        public MainTabItemModel LastSelectedConfig
        {
            set
            {
                _lastSelectedConfig = value;
            }
            get
            {
                return _lastSelectedConfig;
            }
        }
        public WkyMainTabView()
            : this(new ObservableCollection<MainTabItemModel>())
        { }

        public WkyMainTabView(ObservableCollection<MainTabItemModel> viewModel)
        {
            InitializeComponent();

            this.ViewModel = viewModel;
        }

        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(ObservableCollection<MainTabItemModel>), typeof(WkyMainTabView));

        public ObservableCollection<MainTabItemModel> ViewModel
        {
            get { return (ObservableCollection<MainTabItemModel>)GetValue(ViewModelProperty); }
            set { 
                SetValue(ViewModelProperty, value);
                if (value != null && value.Count > 0)
                {
                    mainConfigViewDataGrid.SelectedItem = value.First();
                }
            }
        }


        public static readonly RoutedEvent OnConfigSelectedEvent = EventManager.RegisterRoutedEvent("OnConfigSelected", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(WkyMainTabView));

        public event RoutedEventHandler OnConfigSelected
        {
            add { AddHandler(OnConfigSelectedEvent, value); }
            remove { RemoveHandler(OnConfigSelectedEvent, value); }
        }

        void RaiseSelectedEvent(SelectionChangedEventArgs e)
        {
            var arg = new RoutedEventArgs(OnConfigSelectedEvent, e);
            RaiseEvent(arg);
        }


        private void mainConfigViewDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LastSelectedConfig = (MainTabItemModel)mainConfigViewDataGrid.SelectedItem;
            RaiseSelectedEvent(e);
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

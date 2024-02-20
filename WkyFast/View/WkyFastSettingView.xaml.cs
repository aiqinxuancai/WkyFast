using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
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
using WkyFast.Service;
using WkyFast.Utils;

namespace WkyFast.View
{
    /// <summary>
    /// WkyFastSetting.xaml 的交互逻辑
    /// </summary>
    public partial class WkyFastSettingView : Page
    {
        public WkyFastSettingView()
        {
            InitializeComponent();

            //AccountTextBlock.Text = Aria2ApiManager.Instance.API.User;

            //Aria2ApiManager.Instance.API.EventReceived
            //            .OfType<LoginResultEvent>()
            //            .Subscribe(async r =>
            //            {
            //                AccountTextBlock.Text = r.Account;
            //            });

        }

        private void BadgeNewVersion_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //跳转至最新
            BrowserHelper.OpenUrlBrowser("https://github.com/aiqinxuancai/WkyFast/releases/latest");
        }


        private async void LinkAIKEY_Click(object sender, RoutedEventArgs e)
        {
            BrowserHelper.OpenUrlBrowser("https://aikey.one/register?aff=qHFBWX");
        }

        private async void LinkAPI2D_Click(object sender, RoutedEventArgs e)
        {
            BrowserHelper.OpenUrlBrowser("https://api2d.com/r/211572");
        }

        private void HomePageTextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            BrowserHelper.OpenUrlBrowser("https://github.com/aiqinxuancai/WkyFast");
        }
    }
}

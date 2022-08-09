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
using WkyApiSharp.Events.Account;
using WkyFast.Service;

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

            AccountTextBlock.Text = WkyApiManager.Instance.API.User;

            WkyApiManager.Instance.EventReceived
                        .OfType<LoginResultEvent>()
                        .Subscribe(async r =>
                        {
                            AccountTextBlock.Text = r.Account;
                        });

        }

        private void AccountCardAction_Click(object sender, RoutedEventArgs e)
        {
            //询问登出
            //if (MessageBox.Show("是否登出账号？", "提示", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            //{
            //}

            MainWindow.Instance.ShowMessageBox("提示", "是否登出账号？", () => {
                MainWindow.Instance.ReLoginFunc();
                this.AccountTextBlock.Text = "-";
            }, () => {
                //没有操作
            });
        }

        public void OnLoginResult(LoginResultEvent e)
        {
            if (e.IsSuccess)
            {
                this.AccountTextBlock.Text = e.Account;
            }
        }

    }
}

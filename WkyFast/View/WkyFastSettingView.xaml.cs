using System;
using System.Collections.Generic;
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
        }

        private void AccountCardAction_Click(object sender, RoutedEventArgs e)
        {
            //询问登出
            //if (MessageBox.Show("是否登出账号？", "提示", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            //{
            //}

            MainWindow.Instance.ShowMessageBox("提示", "是否登出账号？", () => { 
                var user = WkyApiManager.Instance.API.User;
                user.


            }, () => {
                //没有操作
            });
        }
    }
}

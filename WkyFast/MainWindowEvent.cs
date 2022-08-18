using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Wpf.Ui.Common;

namespace WkyFast
{
    public partial class MainWindow
    {
        //注册事件，用于弹出提示等操作

        private Action _messageBoxLeft;
        private Action _messageBoxRight;


        public void ShowMessageBox(string title, string message, Action leftClick, Action rightClick, string buttonLeftName = "Yes", string buttonRightName = "No")
        {
            this.RootDialog.ButtonLeftName = buttonLeftName;
            this.RootDialog.ButtonRightName = buttonRightName;
            _messageBoxLeft = leftClick;
            _messageBoxRight = rightClick;
            this.RootDialog.ButtonLeftClick -= RootDialog_ButtonLeftClick;
            this.RootDialog.ButtonRightClick -= RootDialog_ButtonRightClick;
            this.RootDialog.ButtonLeftClick += RootDialog_ButtonLeftClick;
            this.RootDialog.ButtonRightClick += RootDialog_ButtonRightClick;
            this.RootDialog.Show(title, message);
        }

        private void RootDialog_ButtonRightClick(object sender, RoutedEventArgs e)
        {
            _messageBoxRight();
            this.RootDialog.Hide();
        }

        private void RootDialog_ButtonLeftClick(object sender, System.Windows.RoutedEventArgs e)
        {
            _messageBoxLeft();
            this.RootDialog.Hide();
        }


        /// <summary>
        /// 展示提示类
        /// </summary>
        public void ShowSnackbar(string title, string message, SymbolRegular icon = SymbolRegular.Info24)
        {
            this.RootSnackbar.Show(title, message, icon);
        }

    }
}


using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Shapes;
using Wpf.Ui.Controls;

namespace WkyFast.Dialogs
{

    public enum LoginDialogTapType
    {
        CreateAccount,
        Login
    }

    public delegate void LoginDialogDelegate(LoginDialog loginDialog, LoginDialogTapType type, string email, string password, bool savePassword, bool autoLogin);

    /// <summary>
    /// StartWindow.xaml 的交互逻辑
    /// </summary>
    public partial class LoginDialog : UserControl
    {
        private LoginDialogDelegate _loginDialogDelegate;

        public LoginDialog()
        {
            InitializeComponent();
        }

        public LoginDialog(LoginDialogDelegate loginDialogDelegate,
                            string InitialUsername,
                            string InitialPassword,
                            bool savePasswordChecked,
                            bool autoLoginChecked)
        {
            InitializeComponent();
            _loginDialogDelegate = loginDialogDelegate;
            EmailTextBox.Text = InitialUsername;
            PasswordTextBox.Text = InitialPassword;
            SavePasswordCheckBox.IsChecked = savePasswordChecked;
            AutoLoginCheckBox.IsChecked = autoLoginChecked;
            Debug.WriteLine("LoginDialog创建");
        }

        ~LoginDialog()
        {
            Debug.WriteLine("LoginDialog销毁");
        }

        public void UpdateDefaltData(LoginDialogDelegate loginDialogDelegate,
                            string InitialUsername,
                            string InitialPassword,
                            bool savePasswordChecked,
                            bool autoLoginChecked)
        {
            _loginDialogDelegate = loginDialogDelegate;
            EmailTextBox.Text = InitialUsername;
            PasswordTextBox.Text = InitialPassword;
            SavePasswordCheckBox.IsChecked = savePasswordChecked;
            AutoLoginCheckBox.IsChecked = autoLoginChecked;
            Debug.WriteLine("LoginDialog更新");
        }

        public void ShowLoading(bool isShow = true)
        {
            //展示在Loading的画面
            if (isShow)
            {
                this.LoadingView.Visibility = Visibility.Visible;
            }
            else
            {
                this.LoadingView.Visibility = Visibility.Collapsed;
            }

        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            _loginDialogDelegate?.Invoke(this, LoginDialogTapType.Login,
                EmailTextBox.Text,
                PasswordTextBox.Password,
                (bool)SavePasswordCheckBox.IsChecked,
                (bool)AutoLoginCheckBox.IsChecked);
        }


        private void CreateLabel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _loginDialogDelegate?.Invoke(this, LoginDialogTapType.CreateAccount,
                EmailTextBox.Text, 
                PasswordTextBox.Password, 
                (bool)SavePasswordCheckBox.IsChecked,
                (bool)AutoLoginCheckBox.IsChecked);
        }

        private void ExitLabel_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //通知退出

            //退出
            App.ExitWkyFast();
        }
    }
}

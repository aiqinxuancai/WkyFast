using MahApps.Metro.Controls;
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

namespace WkyFast.Window
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
    public partial class LoginDialog : MahApps.Metro.Controls.Dialogs.CustomDialog 
    {
        private LoginDialogDelegate _loginDialogDelegate;

        public LoginDialog(LoginDialogDelegate loginDialogDelegate,
                            string InitialUsername,
                            string InitialPassword,
                            bool savePasswordChecked,
                            bool autoLoginChecked)
        {
            _loginDialogDelegate = loginDialogDelegate;
            InitializeComponent();
            EmailTextBox.Text = InitialUsername;
            PasswordTextBox.Password = InitialPassword;
            SavePasswordCheckBox.IsChecked = savePasswordChecked;
            AutoLoginCheckBox.IsChecked = autoLoginChecked;
        }

        ~LoginDialog()
        {
            Debug.WriteLine("LoginDialog销毁");
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
            //退出
            Application.Current.Shutdown();
        }
    }
}

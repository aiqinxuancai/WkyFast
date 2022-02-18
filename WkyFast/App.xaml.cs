using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace WkyFast
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        public static void ExitWkyFast()
        {
            if (WkyFast.MainWindow.Instance != null)
            {
                WkyFast.MainWindow.Instance.Close();
            }
            App.Current.Shutdown();
        }
    }
}

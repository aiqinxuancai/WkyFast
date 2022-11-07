using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using WkyFast.Service;

namespace WkyFast
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        static App()
        {


            TextOptions.TextFormattingModeProperty.OverrideMetadata(typeof(Window),
                new FrameworkPropertyMetadata(TextFormattingMode.Display, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits));
        }
        public static void ExitWkyFast()
        {
            if (WkyFast.MainWindow.Instance != null)
            {
                WkyFast.MainWindow.Instance.Close();
            }
            App.Current.Shutdown();
        }


        App()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            DispatcherUnhandledException += Current_DispatcherUnhandledException;
        }


        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = (Exception)e.ExceptionObject;
            EasyLogManager.Logger.Error(ex);
            MessageBox.Show(ex?.Message + Environment.NewLine + ex?.InnerException?.ToString(), "Error#1", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            EasyLogManager.Logger.Error(e?.ToString());
            MessageBox.Show(e?.Exception?.Message + Environment.NewLine + e?.Exception?.InnerException?.ToString(), "Error#2", MessageBoxButton.OK, MessageBoxImage.Information);
            e.Handled = true;
        }
    }
}

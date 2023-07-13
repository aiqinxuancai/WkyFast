using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading;
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
using Wpf.Ui.Common;
using Wpf.Ui.Contracts;
using Wpf.Ui.Controls;
using Wpf.Ui.Services;

namespace WkyFast.UI.Test
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : INavigationWindow, IContentDialogService
    {
        public MainWindowViewModel ViewModel { get; }

        public INavigationView GetNavigation()
        {
            throw new NotImplementedException();
        }

        public bool Navigate(Type pageType)
        {
            throw new NotImplementedException();
        }

        public void SetServiceProvider(IServiceProvider serviceProvider)
        {
            throw new NotImplementedException();
        }

        public void SetPageService(IPageService pageService)
        {
            throw new NotImplementedException();
        }

        public void ShowWindow()
        {
            throw new NotImplementedException();
        }

        public void CloseWindow()
        {
            throw new NotImplementedException();
        }

        public void SetContentPresenter(ContentPresenter contentPresenter)
        {
            throw new NotImplementedException();
        }

        public ContentPresenter GetContentPresenter()
        {
            throw new NotImplementedException();
        }

        public Task<ContentDialogResult> ShowAlertAsync(string title, string message, string closeButtonText, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<ContentDialogResult> ShowSimpleDialogAsync(SimpleContentDialogCreateOptions options, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public MainWindow()
        {
            DataContext = this;



            Wpf.Ui.Appearance.Watcher.Watch(this);

            InitializeComponent();

            //Loaded += (_, _) => RootNavigation.Navigate(typeof(DashboardPage));

            
        }

        private ControlAppearance _snackbarAppearance = ControlAppearance.Secondary;


        private void SnackbarBtn_Click(object sender, RoutedEventArgs e)
        {
            //+_snackbar   null    Wpf.Ui.Controls.Snackbar

            //var service = App.GetService<ISnackbarService>();

            SnackbarService snackbarService = new SnackbarService();
            snackbarService.SetSnackbarPresenter(SnackbarPresenter);

            snackbarService.Show(
                "Don't Blame Yourself.",
                "No Witcher's Ever Died In His Bed.",
                _snackbarAppearance,
                new SymbolIcon(SymbolRegular.Fluent24),
                TimeSpan.FromSeconds(5)
            );
        }

        private async void DialogBtn_Click(object sender, RoutedEventArgs e)
        {
            var service = App.GetService<IContentDialogService>();

            service.SetContentPresenter(DialogPresenter);

            var result = await service.ShowSimpleDialogAsync(
                new SimpleContentDialogCreateOptions()
                {
                    Title = "Save your work?",
                    Content = "aaaa",
                    PrimaryButtonText = "Save",
                    SecondaryButtonText = "Don't Save",
                    CloseButtonText = "Cancel",
                }
            );
        }
    }
}

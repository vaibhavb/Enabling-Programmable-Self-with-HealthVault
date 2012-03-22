using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;

namespace MoodTracker
{
    public partial class HostedBrowser : PhoneApplicationPage
    {
        public HostedBrowser()
        {
            InitializeComponent();
            c_webBrowser.IsScriptEnabled = true;

            Loaded += new RoutedEventHandler(HealthVaultWebPage_Loaded);
            c_webBrowser.Navigated += new EventHandler<System.Windows.Navigation.NavigationEventArgs>(c_webBrowser_Navigated);
            c_webBrowser.Navigating += new EventHandler<NavigatingEventArgs>(c_webBrowser_Navigating);
        }

        void c_webBrowser_Navigating(object sender, NavigatingEventArgs e)
        {
            c_CurrentUrl.Text = e.Uri.AbsoluteUri;
        }

        void c_webBrowser_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            if (e.Uri.OriginalString.Contains("target=AppAuthSuccess"))
            {
                Uri pageUri = new Uri("/MyMood.xaml", UriKind.RelativeOrAbsolute);

                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    NavigationService.Navigate(pageUri);
                });
            }
        }

        void HealthVaultWebPage_Loaded(object sender, RoutedEventArgs e)
        {
            string url = App.HealthVaultShellUrl;

            c_webBrowser.Navigate(new Uri(url));
        }
    }
}
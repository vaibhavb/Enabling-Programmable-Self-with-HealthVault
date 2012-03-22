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
    public partial class Page1 : PhoneApplicationPage
    {
        public Page1()
        {
            InitializeComponent();
            textBlock1.Text = @"This application requires a connection to the Microsoft"+
                              @"HealthVault Service to store emotional state. You will need" +
                              @"to sign up for a free HealthVault account and authorize this" +
                              @"application to access emotional state in your HealthVault" +
                              @" record. Press Continue to begin the sign-up process.";
            button1.Click += new RoutedEventHandler(authenticateHealthVault);
        }

        void authenticateHealthVault(object sender, RoutedEventArgs e)
        {
            Uri pageUri = new Uri("/MyMood.xaml", UriKind.RelativeOrAbsolute);

            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                NavigationService.Navigate(pageUri);
            });
        }
    }
}
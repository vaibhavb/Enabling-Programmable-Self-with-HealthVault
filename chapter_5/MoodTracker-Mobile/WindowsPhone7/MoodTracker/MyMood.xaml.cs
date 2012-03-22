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
using System.Xml;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Net.NetworkInformation;
using Microsoft.Phone.Tasks;
using System.Windows.Navigation;
using System.Xml.Linq;
using System.Windows.Media.Imaging;
using System.IO;
using Microsoft.Health.Mobile;

namespace MoodTracker
{

    /// <summary>
    /// This is the main page which displays to the user if 
    /// they go through the HealthVault authentication and authorization.
    /// </summary>
    public partial class MainPage : PhoneApplicationPage
    {
        bool _addingRecord = false;
        List<string> _currentThingIds = new List<string>();

        // Constructor
        public MainPage()
        {
            InitializeComponent();
            //An application should check for network before proceeding
            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                SetErrorMesasge("No Network Available");
                App.Quit();
            }
            Loaded += new RoutedEventHandler(MainPage_Loaded);
        }

        void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            App.HealthVaultService.LoadSettings(App.SettingsFilename);
            App.HealthVaultService.BeginAuthenticationCheck(AuthenticationCompleted, 
                DoShellAuthentication);
            SetProgressBarVisibility(true);
        }

        void DoShellAuthentication(object sender, HealthVaultResponseEventArgs e)
        {
            SetProgressBarVisibility(false);

            App.HealthVaultService.SaveSettings(App.SettingsFilename);

            string url;

            if (_addingRecord)
            {
                url = App.HealthVaultService.GetUserAuthorizationUrl();
            }
            else
            {
                url = App.HealthVaultService.GetApplicationCreationUrl();
            }

            App.HealthVaultShellUrl = url;

            // If we are  using hosted browser via the hosted browser page
            Uri pageUri = new Uri("/HostedBrowser.xaml", UriKind.RelativeOrAbsolute);

            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                NavigationService.Navigate(pageUri);
            });

        }

        void AuthenticationCompleted(object sender, HealthVaultResponseEventArgs e)
        {
            SetProgressBarVisibility(false);

            if (e != null && e.ErrorText != null)
            {
                SetRecordName(e.ErrorText);
                return;
            }

            if (App.HealthVaultService.CurrentRecord == null)
            {
                App.HealthVaultService.CurrentRecord = App.HealthVaultService.Records[0];
            }

            App.HealthVaultService.SaveSettings(App.SettingsFilename);
            if (App.HealthVaultService.CurrentRecord != null)
            {
                SetRecordName(App.HealthVaultService.CurrentRecord.RecordName);
                // We are only interested in the last item
                HealthVaultMethods.GetThings(EmotionalStateModel.TypeId, 1, null, null, GetThingsCompleted);
                SetProgressBarVisibility(true);
            }
        }

        void SetRecordName(string recordName)
        {
            Dispatcher.BeginInvoke(() =>
            {
                c_RecordName.Text = recordName;
            });
        }

        void SetProgressBarVisibility(bool visible)
        {
            Dispatcher.BeginInvoke(() =>
            {
                c_progressBar.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
            });
        }

        void SetErrorMesasge(string message)
        {
            Dispatcher.BeginInvoke(() =>
                {
                    MessageBox.Show(message);
                });
        }

        void SetUserToast(string message)
        {
            Dispatcher.BeginInvoke(() =>
            {
                MessageBox.Show(message);
            });
        }

        void GetThingsCompleted(object sender, HealthVaultResponseEventArgs e)
        {
            SetProgressBarVisibility(false);

            if (e.ErrorText == null)
            {
                XElement responseNode = XElement.Parse(e.ResponseXml);
                // using LINQ to get the latest reading of emotional state
                XElement latestEmotion = (from thingNode in responseNode.Descendants("thing")
                                          orderby Convert.ToDateTime(thingNode.Element("eff-date").Value) descending
                                          select thingNode).FirstOrDefault<XElement>();

                if (latestEmotion != null)
                {
                    EmotionalStateModel emotionalState =
                        new EmotionalStateModel();
                    emotionalState.Parse(latestEmotion);

                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        c_LastUpdated.Text +=
                            string.Format("{0} - \nMood:{1}, Stress:{2}, Wellbeing:{3}",
                                emotionalState.When.ToString("MMM dd, yyyy"),
                                System.Enum.GetName(typeof(Mood), emotionalState.Mood),
                                System.Enum.GetName(typeof(Stress), emotionalState.Stress),
                                System.Enum.GetName(typeof(Wellbeing), emotionalState.Wellbeing));
                        this.DataContext = this;
                    });
                }
                else
                {
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        c_LastUpdated.Text = "No readings! Time to track mood.";
                        this.DataContext = this;
                    });
                }
            }
        }

        // Save the reading to HealthVault
        private void Btn_SaveReadingToHealthVault_Click(object sender, RoutedEventArgs e)
        {
            EmotionalStateModel model = new EmotionalStateModel();
            model.Mood = (Mood)c_MoodSlider.Value;
            model.Stress = (Stress)c_StressSlider.Value;
            model.Wellbeing = (Wellbeing)c_WellbeingSlider.Value;
            model.When = DateTime.Now;
            model.Note = GetNote();
            HealthVaultMethods.PutThings(model, PutThingsCompleted);
            SetProgressBarVisibility(true);
        }

        void PutThingsCompleted(object sender, HealthVaultResponseEventArgs e)
        {
            SetProgressBarVisibility(false);
            if (e.ErrorText != null)
            {
                SetErrorMesasge(e.ErrorText);
            }
            else
            {
                SetUserToast("Mood successfully saved!");
            }
        }

        public string GetSliderValue(Type t, Slider slider)
        {
            return System.Enum.GetName(
                t, (int)slider.Value);
        }

        private void c_MoodSlider_ValueChanged(object sender, 
			System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            Dispatcher.BeginInvoke(() =>
                {
                    string value = GetSliderValue(typeof(Mood), c_MoodSlider);
                    MoodSliderValue.Text = value;
                });
        }

        private void c_WellbeingSlider_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            Dispatcher.BeginInvoke(() =>
            {
                string value = GetSliderValue(typeof(Wellbeing), c_WellbeingSlider);
                WellbeingSliderValue.Text = value;
            });
        }

        private void c_StressSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Dispatcher.BeginInvoke(() =>
            {
                string value = GetSliderValue(typeof(Stress), c_StressSlider);
                StressSliderValue.Text = value;
            });
        }

        private string GetNote()
        {
            if (!string.IsNullOrEmpty(Txt_EmotionalState_Note.Text))
            {
                return Txt_EmotionalState_Note.Text;
            }
            else return "";
        }

        private void ApplicationBarIconButton_Click(object sender, EventArgs e)
        {
            Btn_SaveReadingToHealthVault_Click(sender, new RoutedEventArgs());
        }

        private void ApplicationBarIconButton_Click_2(object sender, EventArgs e)
        {
            Uri pageUri = new Uri("/MyMoodPlant.xaml", UriKind.RelativeOrAbsolute);
            NavigationService.Navigate(pageUri);
        }

        private void ApplicationBarIconButton_Click_1(object sender, EventArgs e)
        {
            Uri pageUri = new Uri("/MyHistory.xaml", UriKind.RelativeOrAbsolute);
            NavigationService.Navigate(pageUri);
        }

    }
}
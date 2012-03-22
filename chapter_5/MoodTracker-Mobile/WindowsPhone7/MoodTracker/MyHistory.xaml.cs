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
using System.Collections.ObjectModel;

using Microsoft.Health.Mobile;
using System.Xml.Linq;

namespace MoodTracker
{
    public partial class MyHistory : PhoneApplicationPage
    {
        private ObservableCollection<EmotionalStateModel> _emotions =
            new ObservableCollection<EmotionalStateModel>();
        public ObservableCollection<EmotionalStateModel> EmotionList
        {
            get
            {
                if (_emotions == null)
                    _emotions = new ObservableCollection<EmotionalStateModel>();
                return _emotions;
            }
        }
      

        public MyHistory()
        {
            InitializeComponent();
        }

        public DateTime BaseTimeForGraph
        {
            get;
            set;
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            App.HealthVaultService.LoadSettings(App.SettingsFilename);
            BaseTimeForGraph = DateTime.Now; 
            if (App.HealthVaultService.CurrentRecord != null)
            {
                SetRecordName(App.HealthVaultService.CurrentRecord.RecordName);
                RefreshGraph();
                SetProgressBarVisibility(true);
            }
            this.DataContext = this;
        }

        void RefreshGraph()
        {
            this.EmotionList.Clear();
            this.GraphLabel.Text = string.Format("Readings for last 7 Days from {0}",
                BaseTimeForGraph.ToString("MMM dd, yyyy"));
            // Get the last emotional state info and try to plot a graph 
            HealthVaultMethods.GetThings(EmotionalStateModel.TypeId, null,
                BaseTimeForGraph.Subtract(new TimeSpan(7, 0, 0, 0)),
                BaseTimeForGraph,
                GetThingsCompleted);
        }

        void GetThingsCompleted(object sender, HealthVaultResponseEventArgs e)
        {
            ObservableCollection<EmotionalStateModel> emotionList 
                = new ObservableCollection<EmotionalStateModel>();
            SetProgressBarVisibility(false);
            if (e.ErrorText == null)
            {
                XElement responseNode = XElement.Parse(e.ResponseXml);
                foreach (XElement thing in responseNode.Descendants("thing"))
                {
                    EmotionalStateModel emotionalState = new EmotionalStateModel();
                    emotionalState.Parse(thing);

                    Dispatcher.BeginInvoke( () => {
                        DoItemAdd(
                        emotionalState.When,
                        emotionalState.Mood,
                        emotionalState.Stress,
                        emotionalState.Wellbeing,
                        emotionalState.Note);
                    });
                }
            }
        }

        private void DoItemAdd(DateTime when, Mood mood, 
            Stress stress, Wellbeing wellbeing, String note)
        {
            EmotionalStateModel emotion = new EmotionalStateModel();
            emotion.When = when;
            emotion.Mood = mood;
            emotion.Stress = stress;
            emotion.Wellbeing = wellbeing;
            emotion.Note = note;
            this.EmotionList.Add(emotion);
        }

        void SetProgressBarVisibility(bool visible)
        {
            Dispatcher.BeginInvoke(() =>
            {
                c_progressBar.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
            });
        }

        void SetRecordName(string recordName)
        {
            Dispatcher.BeginInvoke(() =>
            {
                c_RecordName.Text = recordName;
            });
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button_Next.Visibility = System.Windows.Visibility.Visible;
            BaseTimeForGraph = BaseTimeForGraph.Subtract(new TimeSpan(7, 0, 0, 0));
            RefreshGraph();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            BaseTimeForGraph = BaseTimeForGraph.Add(new TimeSpan(7, 0, 0, 0));
            RefreshGraph();
            if (BaseTimeForGraph >= DateTime.Now.Subtract(new TimeSpan(7,0,0,0)))
            {
                Button_Next.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        private void ApplicationBar_Home_Click(object sender, EventArgs e)
        {
            Uri pageUri = new Uri("/MyMood.xaml", UriKind.RelativeOrAbsolute);
            NavigationService.Navigate(pageUri);
        }

        private void ApplicationBar_MoodPlant_Click(object sender, EventArgs e)
        {
            Uri pageUri = new Uri("/MyMoodPlant.xaml", UriKind.RelativeOrAbsolute);
            NavigationService.Navigate(pageUri);
        }
    }
}
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
using System.Windows.Media.Imaging;
using Microsoft.Health.Mobile;
using System.Xml.Linq;

namespace MoodTracker
{
    public partial class MyMoodPlant : PhoneApplicationPage
    {
        public MyMoodPlant()
        {
            InitializeComponent();
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            App.HealthVaultService.LoadSettings(App.SettingsFilename);
            if (App.HealthVaultService.CurrentRecord != null)
            {
                SetProgressBarVisibility(true);
                SetRecordName(App.HealthVaultService.CurrentRecord.RecordName);
                CalculateEmotionValue();
            }
            this.DataContext = this;
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

        void CalculateEmotionValue()
        {
            // Get the last emotional state info and try to plot a graph 
            HealthVaultMethods.GetThings(EmotionalStateModel.TypeId, null,
                baseTime.Subtract(new TimeSpan(28, 0, 0, 0)),
                baseTime,
                GetThingsCompleted);
        }

        private DateTime baseTime = DateTime.Now;
        
        private List<EmotionalStateModel> emotionList
                = new List<EmotionalStateModel>();

        void GetThingsCompleted(object sender, HealthVaultResponseEventArgs e)
        {
            
            SetProgressBarVisibility(false);
            if (e.ErrorText == null)
            {
                XElement responseNode = XElement.Parse(e.ResponseXml);
                foreach (XElement thing in responseNode.Descendants("thing"))
                {
                    EmotionalStateModel emotionalState = new EmotionalStateModel();
                    emotionalState.Parse(thing);
                    this.emotionList.Add(emotionalState);
                }
            }

            /*
             * Algorithm 
             * 1. Read last 1 month's readings
             * 2. Weight 50% to 4th week
             * 3. Weight 25% to 1-3 week
             * 4. Weight 25% to how many readings (Good is 4/wk)
             */
            DateTime time50p = baseTime.Subtract(new TimeSpan(7,0,0,0));
            DateTime time30p = baseTime.Subtract(new TimeSpan(14, 0, 0, 0));

            int m = 0; int s = 0; int w = 0;
            int c50 = 0; int c30 = 0; int c20 = 0;
            foreach (EmotionalStateModel emotion in emotionList)
            {
                if (emotion.When >= time50p)
                {
                    m += (int)emotion.Mood * 50 ;
                    s += (int)emotion.Stress * 50 ;
                    w += (int)emotion.Wellbeing * 50;
                    c50++;
                }
                else if (emotion.When >= time30p)
                {
                    m += (int)emotion.Mood * 30;
                    s += (int)emotion.Stress * 30 ;
                    w += (int)emotion.Wellbeing * 30;
                    c30++;
                }
                else
                {
                    m += (int)emotion.Mood * 20;
                    s += (int)emotion.Stress * 20;
                    w += (int)emotion.Wellbeing * 20;
                    c20++;
                }
            }

            // Final numbers
            int c = 50 * c50 + 30 * c30 + 20 * c20;
            m = m / c;
            s = s /c;
            w = w / c;

            EmotionalStateModel resultEmotion = new EmotionalStateModel();
            resultEmotion.Mood = (Mood) m;
            resultEmotion.Stress = (Stress) s;
            resultEmotion.Wellbeing = (Wellbeing) w;

            RenderPlantValue(resultEmotion);

        }

        void RenderPlantValue(EmotionalStateModel emotion)
        {
            Dispatcher.BeginInvoke(() =>
                {
                    c_vmudi_plant_wellbeing.Source = new BitmapImage(new Uri(
                           string.Format("Images/plant/myplant_wellbeing_{0}.png", emotion.Wellbeing.ToString().ToLower()),
                           UriKind.Relative));
                    c_vmudi_plant_mood.Source = new BitmapImage(new Uri(
                           string.Format("Images/plant/myplant_mood_{0}.png", emotion.Mood.ToString().ToLower()),
                           UriKind.Relative));
                    c_vmudi_plant_stress.Source = new BitmapImage(new Uri(
                           string.Format("Images/plant/myplant_stress_{0}.png", emotion.Stress.ToString().ToLower()),
                           UriKind.Relative));
                });
        }

        private void ApplicationBar_Home_Click(object sender, EventArgs e)
        {
            Uri pageUri = new Uri("/MyMood.xaml", UriKind.RelativeOrAbsolute);
            NavigationService.Navigate(pageUri);
        }

        private void ApplicationBar_History_Click(object sender, EventArgs e)
        {
            Uri pageUri = new Uri("/MyHistory.xaml", UriKind.RelativeOrAbsolute);
            NavigationService.Navigate(pageUri);
        }
    
    }
}
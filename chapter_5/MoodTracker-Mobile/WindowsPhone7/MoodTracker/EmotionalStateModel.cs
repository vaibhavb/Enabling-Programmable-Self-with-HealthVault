using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

using System.Xml.Linq;
using System.Linq;

namespace MoodTracker
{
    /// <summary>
    ///  Data Model for Emotional State.
    ///  This model should enable - 
    ///     1. Loading an emotional state from existing HealthVault XML
    ///     2. Creating an emotional state object to be store in HealthVault
    ///     3. Expose the important properties of emotional state available in HealthVault SDK
    /// </summary>
    public class EmotionalStateModel : HealthRecordItemModel
    {
        public readonly static string TypeId = "4b7971d6-e427-427d-bf2c-2fbcf76606b3";
        private string thingXml =
            @"<info><thing>
                <type-id>4b7971d6-e427-427d-bf2c-2fbcf76606b3</type-id>
                <thing-state>Active</thing-state>
                <flags>0</flags>
                <data-xml>
                   <emotion>
                    <when>
                        <date>
                            <y>{0}</y>
                            <m>{1}</m>
                            <d>{2}</d>
                        </date>
                        <time>
                            <h>{3}</h>
                            <m>{4}</m>
                            <s>{5}</s>
                            <f>{6}</f>
                        </time>
                    </when>
                    <mood>{7}</mood>
                    <stress>{8}</stress>
                    <wellbeing>{9}</wellbeing>
                  </emotion>
                  <common>
                    <source>Mood Tracker for Windows Phone</source>
                    <note>{10}</note>
                  </common>
                </data-xml>
            </thing></info>";

        public void Parse(XElement thingXml)
        {
            this.Mood = Mood.None;
            this.Stress = Stress.None;
            this.Wellbeing = Wellbeing.None;

            XElement emotionalState = thingXml.Descendants("data-xml").Descendants("emotion").First();

            this.When = Convert.ToDateTime(thingXml.Element("eff-date").Value);

            if (thingXml.Descendants("common") != null &&
                    (thingXml.Descendants("common").Descendants("note").Count() != 0))
            {
                this.Note = thingXml.Descendants("common").Descendants("note").First().Value;
            }

            if (emotionalState.Element("mood") != null)
            {
                try
                {
                    this.Mood = (Mood)System.Enum.Parse(typeof(Mood),
                        ((XElement)emotionalState.Element("mood")).Value, true);
                }
                catch (Exception) { }
            }
            if (emotionalState.Element("stress") != null)
            {
                try
                {
                    this.Stress = (Stress)System.Enum.Parse(typeof(Stress),
                        ((XElement)emotionalState.Element("stress")).Value, true);
                }
                catch (Exception) { }
            }
            if (emotionalState.Element("wellbeing") != null)
            {
                try
                {
                    this.Wellbeing = (Wellbeing)System.Enum.Parse(typeof(Wellbeing),
                    ((XElement)emotionalState.Element("wellbeing")).Value, true);
                }
                catch (Exception) { }
            }
        }

        /// <summary>
        /// Get the Xml representing this type
        /// </summary>
        /// <returns></returns>
        public override string GetXml()
        {
            return string.Format(
                thingXml,
                When.Year,
                When.Month,
                When.Day,
                When.Hour,
                When.Minute,
                When.Second,
                When.Millisecond,
                ((int)this.Mood).ToString(),
                ((int)this.Stress).ToString(),
                ((int)this.Wellbeing).ToString(),
                this.Note);
        }

        public String FormattedWhen
        {
            get{
                return String.Format("{0:dd/MM}",When);
            }
        }
        public DateTime When { get; set; }
        public Mood Mood { get; set; }
        public Stress Stress { get; set; }
        public Wellbeing Wellbeing { get; set; }
        public string Note { get; set; }
    }

    /// <summary>
    /// Mood Coding
    /// </summary>
    public enum Mood
    {
        /// <summary>
        /// The person's mood is unknown.
        /// </summary>
        /// 
        None = 0,

        /// <summary>
        /// The person is depressed.
        /// </summary>
        /// 
        Depressed = 1,

        /// <summary>
        /// The person is sad.
        /// </summary>
        /// 
        Sad = 2,

        /// <summary>
        /// The person's mood is neutral.
        /// </summary>
        /// 
        Neutral = 3,

        /// <summary>
        /// The person is happy.
        /// </summary>
        /// 
        Happy = 4,

        /// <summary>
        /// The person is elated.
        /// </summary>
        /// 
        Elated = 5
    }

    /// <summary>
    /// Represents Stress
    /// </summary>
    /// 
    public enum Stress
    {
        /// <summary>
        /// The relative rating is not known.
        /// </summary>
        /// 
        None = 0,

        /// <summary>
        /// The rating is very low.
        /// </summary>
        /// 
        VeryLow = 1,

        /// <summary>
        /// The rating is low.
        /// </summary>
        /// 
        Low = 2,

        /// <summary>
        /// The rating is moderate.
        /// </summary>
        /// 
        Moderate = 3,

        /// <summary>
        /// The rating is high.
        /// </summary>
        /// 
        High = 4,

        /// <summary>
        /// The rating is very high.
        /// </summary>
        /// 
        VeryHigh = 5
    }

    /// <summary>
    /// Describes the general wellbeing of a person from sick to healthy.
    /// </summary>
    /// 
    public enum Wellbeing
    {
        /// <summary>
        /// The wellbeing of the person is unknown.
        /// </summary>
        /// 
        None = 0,

        /// <summary>
        /// The person is sick.
        /// </summary>
        /// 
        Sick = 1,

        /// <summary>
        /// The person is not functioning at a normal level. 
        /// </summary>
        /// 
        Impaired = 2,

        /// <summary>
        /// The person is functioning at a normal level but might still have
        /// symptoms.
        /// </summary>
        /// 
        Able = 3,

        /// <summary>
        /// The person is functioning at a normal level without any
        /// symptoms.
        /// </summary>
        /// 
        Healthy = 4,

        /// <summary>
        /// The person is functioning beyond their normal level. 
        /// </summary>
        /// 
        Vigorous = 5
    }
}

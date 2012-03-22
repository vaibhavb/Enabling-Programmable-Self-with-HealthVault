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

namespace MoodTracker
{
    /// <summary>
    /// Models a HealthRecordItem
    /// </summary>
    public class HealthRecordItemModel
    {
        /// <summary>
        /// Returns the Xml represenation of the object
        /// </summary>
        /// <returns>Xml object representation</returns>
        public virtual string GetXml()
        {
            return "";
        }
    }
}

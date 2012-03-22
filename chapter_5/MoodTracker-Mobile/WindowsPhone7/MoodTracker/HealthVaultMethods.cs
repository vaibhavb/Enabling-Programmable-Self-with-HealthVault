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

using Microsoft.Health.Mobile;
using System.Xml.Linq;
using System.Globalization;
using System.Collections.Generic;

namespace MoodTracker
{
    public class HealthVaultMethods
    {
        /// <summary>
        /// Get things Method
        /// </summary>
        /// <param name="typeId">HealthVault Type Id</param>
        /// <param name="responseCallback">Response Handler for the Type</param>
        public static void GetThings(string typeId,
            int? maxItems,
            DateTime? effDateMin,
            DateTime? effDateMax,
            EventHandler<HealthVaultResponseEventArgs> responseCallback)
        {
            string thingXml = @"
            <info>
                <group {0}>
                    <filter>
                        <type-id>{1}</type-id>
                        <thing-state>Active</thing-state>
                        {2}
                        {3}
                    </filter>
                    <format>
                        <section>core</section>
                        <xml/>
                        <type-version-format>{1}</type-version-format>
                    </format>
                </group>
            </info>";

            XElement info = XElement.Parse(string.Format
                (thingXml,
                MaxItemsXml(maxItems),
                typeId,
                EffDateMinXml(effDateMin),
                EffDateMaxXml(effDateMax)));
            HealthVaultRequest request = new HealthVaultRequest("GetThings", "3", info, responseCallback);
            App.HealthVaultService.BeginSendRequest(request);
        }

        private static string MaxItemsXml(int? maxItems)
        {
            if (maxItems != null)
            {
                return string.Format("max='{0}'", maxItems);
            }
            return "";
        }


        private static string EffDateMinXml(DateTime? effDateMin)
        {
            if (effDateMin != null)
                return
                    string.Format(@"<eff-date-min>{0}</eff-date-min>",
                        effDateMin.Value.ToString("yyyy-MM-ddTHH:mm:ss.FFFZ",
                                    CultureInfo.InvariantCulture)
                        );
            else return "";
        }

        private static string EffDateMaxXml(DateTime? effDateMax)
        {
            if (effDateMax != null)
            {
                return string.Format(@"<eff-date-max>{0}</eff-date-max>",
                    effDateMax.Value.ToString("yyyy-MM-ddTHH:mm:ss.FFFZ",
                                    CultureInfo.InvariantCulture));
            }
            return "";
        }


        /// <summary>
        /// PutThings Method
        /// </summary>
        /// <param name="item">The health item to upload</param>
        /// <param name="responseCallback">Function to resolve callback</param>
        public static void PutThings(HealthRecordItemModel item,
            EventHandler<HealthVaultResponseEventArgs> responseCallback)
        {
            XElement info = XElement.Parse(item.GetXml());
            HealthVaultRequest request = new HealthVaultRequest("PutThings", "2", info, responseCallback);
            App.HealthVaultService.BeginSendRequest(request);
        }

        /// <summary>
        /// PutThings Method
        /// HACK: Need to fix hardcoding of the type
        /// </summary>
        /// <param name="item">The health item to upload</param>
        /// <param name="responseCallback">Function to resolve callback</param>
        public static void PutThings(IEnumerable<EmotionalStateModel> items,
            EventHandler<HealthVaultResponseEventArgs> responseCallback)
        {
            string itemXml = "";
            foreach (HealthRecordItemModel item in items)
            {
                itemXml += item.GetXml();
            }
            XElement info = XElement.Parse("<info>" + itemXml + "</info>");
            HealthVaultRequest request = new HealthVaultRequest("PutThings", "2", info, responseCallback);
            App.HealthVaultService.BeginSendRequest(request);
        }
    }
}

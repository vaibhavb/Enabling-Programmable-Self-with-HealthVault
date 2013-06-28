using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace HealthVault.Types
{
    //
    // DateTime is not allowed by WinRt. 
    // Nullable<T> not allowed by WinRT
    //
    public sealed class DateTimeValue : IHealthVaultType
    {
        public DateTimeValue()
        {
        }

        public DateTimeValue(string val)
        {
            this.Value = DateTimeOffset.Parse(val);
        }

        [XmlIgnore]
        public DateTimeOffset Value
        {
            get; set;
        }
        
        [XmlText]
        public string Text
        {
            get { return XmlConvert.ToString(this.Value);}
            set
            {
                this.Value = XmlConvert.ToDateTimeOffset(value);
            }
        }

        public void Validate()
        {            
        }
    }
}
// (c) Microsoft. All rights reserved

using System;
using System.Xml.Serialization;
using HealthVault.Foundation;

namespace HealthVault.Types
{
    public sealed class CodableValue : IHealthVaultTypeSerializable
    {
        private CodedValueCollection m_codes;

        public CodableValue()
        {
            Text = String.Empty;
        }

        public CodableValue(string text)
            : this(text, null)
        {
        }

        public CodableValue(string text, string code, string vocab)
            : this(text, new CodedValue(code, vocab))
        {
        }

        public CodableValue(string text, CodedValue code)
        {
            if (text == null)
            {
                throw new ArgumentException("text");
            }

            Text = text;
            AddCode(code);
        }

        [XmlElement("text", Order = 1)]
        public string Text { get; set; }

        [XmlElement("code", Order = 2)]
        public CodedValueCollection Codes
        {
            get
            {
                if (m_codes == null)
                {
                    m_codes = new CodedValueCollection();
                }
                return m_codes;
            }
            set { m_codes = value; }
        }

        [XmlIgnore]
        public bool HasText
        {
            get { return Text != null; }
        }

        [XmlIgnore]
        public bool HasCodes
        {
            get { return !m_codes.IsNullOrEmpty(); }
        }

        #region IHealthVaultTypeSerializable Members

        public string Serialize()
        {
            return this.ToXml();
        }

        public void Validate()
        {
            if (Text == null)
            {
                throw new ArgumentException("Text");   
            }
            if (HasCodes)
            {
                m_codes.ValidateOptional<CodedValue>("Codes");
            }
        }

        #endregion

        /// <summary>
        /// Does a trimmed case INSENSITIVE comparison.
        /// Does not word break etc. 
        /// </summary>
        public bool MatchesText(string text)
        {
            if (!HasText)
            {
                return false;
            }

            if (string.IsNullOrEmpty(text))
            {
                throw new ArgumentException("text");
            }

            return Text.SafeEquals(text.Trim(), StringComparison.CurrentCultureIgnoreCase);
        }

        public void Update(string updatedText, CodedValue code)
        {
            if (string.IsNullOrEmpty(updatedText))
            {
                throw new ArgumentException("newText");
            }

            if (MatchesText(updatedText))
            {
                // Same text. We'll keep any existing codes that merge in the new code
                if (code != null)
                {
                    Codes.AddIfDoesNotExist(code);
                }
            }
            else
            {
                // Text is different. 
                Replace(updatedText, code);
            }
        }

        public void Replace(string newText, CodedValue newCode)
        {
            if (string.IsNullOrEmpty(newText))
            {
                throw new ArgumentException("newText");
            }

            Text = newText;
            ClearCodes();
            AddCode(newCode);
        }

        public static CodableValue Deserialize(string xml)
        {
            return HealthVaultClient.Serializer.FromXml<CodableValue>(xml);
        }

        public override string ToString()
        {
            return Text;
        }

        private void AddCode(CodedValue code)
        {
            if (code != null)
            {
                Codes.Add(code);
            }
        }

        private void ClearCodes()
        {
            if (m_codes != null)
            {
                m_codes.Clear();
            }
        }

        public bool ShouldSerializeText()
        {
            return Text != null;
        }
    }
}
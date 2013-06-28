// (c) Microsoft. All rights reserved
namespace HealthVault.Foundation
{
    public class RecordReference : IValidatable
    {
        public RecordReference()
        {
        }

        public RecordReference(string personId, string recordId)
        {
            PersonId = personId;
            RecordId = recordId;
        }

        public string PersonId { get; set; }

        public string RecordId { get; set; }

        #region IValidatable Members

        public void Validate()
        {
            PersonId.ValidateRequired("PersonId");
            RecordId.ValidateRequired("RecordId");
        }

        #endregion
    }
}
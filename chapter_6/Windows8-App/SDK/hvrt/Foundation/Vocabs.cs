// (c) Microsoft. All rights reserved

using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using HealthVault.Types;
using Windows.Foundation;

namespace HealthVault.Foundation
{
    public sealed class Vocabs
    {
        private readonly HealthVaultApp m_app;

        internal Vocabs(HealthVaultApp app)
        {
            m_app = app;
        }

        public IAsyncOperation<IList<VocabCodeSet>> GetAsync(IList<VocabIdentifier> vocabIDs)
        {
            vocabIDs.ValidateRequired("vocabIDs");

            return AsyncInfo.Run<IList<VocabCodeSet>>(
                async cancelToken =>
                      {
                          var getParams = new VocabGetParams
                                          {
                                              VocabIDs = vocabIDs.ToArray()
                                          };

                          VocabGetResults result =
                              await m_app.Client.ServiceMethods.GetVocabularies<VocabGetResults>(getParams, cancelToken);
                          return result.Vocabs;
                      });
        }

        public IAsyncOperation<VocabQueryResult> SearchAsync(VocabIdentifier vocab, string searchText)
        {
            vocab.ValidateRequired("vocab");

            return AsyncInfo.Run(
                async cancelToken =>
                      {
                          //VocabQuery query = new VocabQuery(vocab, searchText);
                          var query = new object[]
                                      {
                                          vocab,
                                          new VocabSearch(searchText)
                                      };
                          VocabQueryResults results =
                              await m_app.Client.ServiceMethods.SearchVocabulary<VocabQueryResults>(query, cancelToken);

                          return results.HasMatches ? results.Matches : null;
                      });
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HealthVault.Foundation;
using HealthVault.Types;
using HealthVault.ItemTypes;
using Windows.Foundation;

namespace HealthVault.Store
{
    /// <summary>
    /// The SychronizedStore works with a Local and a Remote Store (HealthVault). 
    ///   -- It pulls data from the Remote Store into the Local Store
    ///   -- It pushes back data from the Local Store into the Remote Store, and handles any conflicts automatically. 
    ///   
    /// This interface abstracts the interactions of the SynchronizedStore with its remote store. 
    /// By abstracting this interface, we can more easily mock and test our code. 
    /// In the future, we can also add other useful features by injecting additional pre/post processing behavior to the pull/push operations . 
    /// </summary>
    public interface IRemoteItemStore
    {
        IRecord Record { get; set; }

        IAsyncOperation<IList<PendingItem>> GetKeysAndDateAsync(ItemFilter filter, int maxResults);
        IAsyncOperation<IList<RecordItem>> GetAllItemsAsync(ItemQuery query);
        IAsyncOperation<RecordItem> GetItemAsync(ItemKey key, ItemSectionType sections);
        IAsyncOperation<ItemKey> NewAsync(IItemDataTyped item);
        IAsyncOperation<ItemKey> PutAsync(IItemDataTyped item);
        IAsyncAction RemoveItemAsync(ItemKey key);
    }
    
    /// <summary>
    /// Merely forwards all calls to the IRecord object... which in turn calls HealthVault directly
    /// </summary>
    public sealed class RemoteItemStore : IRemoteItemStore
    {
        IRecord m_record;

        public RemoteItemStore(IRecord record)
        {
            this.Record = record;
        }

        public IRecord Record
        {
            get { return m_record;}
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("Record");
                }
                m_record = value;
            }
        }

        public IAsyncOperation<IList<PendingItem>> GetKeysAndDateAsync(ItemFilter filter, int maxResults)
        {
            return m_record.GetKeysAndDateAsync(new ItemFilter [] {filter}, maxResults);
        }

        public IAsyncOperation<IList<RecordItem>> GetAllItemsAsync(ItemQuery query)
        {
            return m_record.GetAllItemsAsync(query);
        }

        public IAsyncOperation<RecordItem> GetItemAsync(ItemKey key, ItemSectionType sections)
        {
            return m_record.GetItemAsync(key, sections);
        }

        public IAsyncOperation<ItemKey> NewAsync(IItemDataTyped item)
        {
            return m_record.NewAsync(item);
        }

        public IAsyncOperation<ItemKey> PutAsync(IItemDataTyped item)
        {
            return m_record.UpdateAsync(item);
        }

        public IAsyncAction RemoveItemAsync(ItemKey key)
        {
            return m_record.RemoveAsync(key);
        }
    }

    /// <summary>
    /// A simple Mock RemoteStore, that will throw exceptions during the pull/push process
    /// </summary>
    public sealed class TestRemoteStore : IRemoteItemStore
    {
        IRemoteItemStore m_innerStore;
        double m_errorProbability;
        Random m_rand;

        public TestRemoteStore(IRemoteItemStore innerStore)
        {
            m_innerStore = innerStore;
            m_errorProbability = 0.5;
            m_rand = new Random();
        }

        public double ErrorProbability
        {
            get { return m_errorProbability;}
            set
            {
                if (value > 1)
                {
                    throw new ArgumentException();
                }
                m_errorProbability = value;
            }
        }
        
        public ActionDelegate ErrorThrower
        {
            get; set;
        }

        public IRecord Record
        {
            get
            {
                return m_innerStore.Record;
            }
            set
            {
                m_innerStore.Record = value;
            }
        }

        public IAsyncOperation<IList<PendingItem>> GetKeysAndDateAsync(ItemFilter filter, int maxResults)
        {
            this.ProduceError();
            return m_innerStore.GetKeysAndDateAsync(filter, maxResults);
        }

        public IAsyncOperation<IList<RecordItem>> GetAllItemsAsync(ItemQuery query)
        {
            this.ProduceError();
            return m_innerStore.GetAllItemsAsync(query);
        }

        public IAsyncOperation<RecordItem> GetItemAsync(ItemKey key, ItemSectionType sections)
        {
            this.ProduceError();
            return m_innerStore.GetItemAsync(key, sections);
        }

        public IAsyncOperation<ItemKey> NewAsync(IItemDataTyped item)
        {
            this.ProduceError();
            return m_innerStore.NewAsync(item);
        }

        public IAsyncOperation<ItemKey> PutAsync(IItemDataTyped item)
        {
            this.ProduceError();
            return m_innerStore.PutAsync(item);
        }

        public IAsyncAction RemoveItemAsync(ItemKey key)
        {
            this.ProduceError();
            return m_innerStore.RemoveItemAsync(key);
        }

        void ProduceError()
        {
            bool callAction = false;
            lock(m_rand)
            {
                callAction = (m_rand.NextDouble() <= m_errorProbability);
            }
            
            if (callAction && this.ErrorThrower != null)
            {
                this.ErrorThrower();
            }
        }
        
        public static TestRemoteStore CreateServerErrorProducer(IRemoteItemStore inner)
        {
            return CreateServerErrorProducer(inner, 
                                        new ServerErrorNumber[] {ServerErrorNumber.Failed, ServerErrorNumber.RequestTimedOut});
        }

        public static TestRemoteStore CreateServerErrorProducer(IRemoteItemStore inner, IList<ServerErrorNumber> errors)
        {
            TestRemoteStore store = new TestRemoteStore(inner);
            Random rand = new Random();
            store.ErrorThrower = delegate()
            {
                ServerErrorNumber error = 0;
                lock(rand)
                {
                    error = errors[rand.Next(0, errors.Count - 1)];
                }
                ServerStatusCode code = (ServerStatusCode)((uint)error - (uint)HResults.ServerErrorBase);
                throw new ServerException(code);
            };

            return store;
        }
    }
}

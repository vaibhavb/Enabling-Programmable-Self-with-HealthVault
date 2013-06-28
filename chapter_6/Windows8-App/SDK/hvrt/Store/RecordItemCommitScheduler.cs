using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Windows.Networking.Connectivity;
using Windows.System.Threading;
using HealthVault.Foundation;

namespace HealthVault.Store
{
    /// <summary>
    /// Use this to push trigger periodic background commits of changes into HealthVault
    /// Typically, when you make a change, the SynchronizedStore will automatically initiate a push. 
    /// However, in case of network outages, or errors, you do want a background process mopping up...
    /// 
    /// To use, you must:
    ///     .IsEnabled = true   (disabled by default)
    ///     (Optional) Enable Network monitoring
    ///     (Optional) Set a timer period
    /// </summary>
    public sealed class RecordItemCommitScheduler
    {
        LocalRecordStoreTable m_recordStores;
        TimeSpan m_timerPeriod;
        ThreadPoolTimer m_timer;
        bool m_monitorNetwork;
        bool? m_networkAvailable;
        object m_lock;

        internal RecordItemCommitScheduler(LocalRecordStoreTable recordStores)
        {
            if (recordStores == null)
            {
                throw new ArgumentNullException("recordStores");
            } 
            
            this.IsEnabled = false;
            // Let the app decide whether they want to use a timer, network monitoring, or both
            // Turned off by default               
            m_monitorNetwork = false; 
            m_timerPeriod = TimeSpan.Zero;
            m_recordStores = recordStores;
            m_lock = new object();
        }
        
        /// <summary>
        /// DISABLED BY DEFAULT
        /// </summary>
        public bool IsEnabled { get; set;}

        public int FrequencyMilliseconds
        {
            get
            {
                lock(m_lock)
                {
                    return (int) m_timerPeriod.TotalMilliseconds;
                }
            }
            set
            {
                lock(m_lock)
                {
                    if (value <= 0)
                    {
                        m_timerPeriod = TimeSpan.Zero;
                    }
                    else
                    {
                        m_timerPeriod = TimeSpan.FromMilliseconds(value);
                    }
                    this.RestartTimer();
                }
            }
        }

        /// <summary>
        /// Disabled by default
        /// </summary>
        public bool MonitorNetworkChanges
        {
            get { return m_monitorNetwork;}
            set
            {
                if (value == m_monitorNetwork)
                {
                    return;
                }

                m_monitorNetwork = value;
                if (m_monitorNetwork)
                {
                    this.SubscribeToNetworkEvents();
                }
                else
                {
                    this.UnsubscribeToNetworkEvents();
                }
            }
        }

        void StartTimer()
        {
            lock(m_lock)
            {
                if (m_timer == null && m_timerPeriod.TotalMilliseconds > 0)
                {
                    m_timer = ThreadPoolTimer.CreatePeriodicTimer(this.OnTimerFired, m_timerPeriod);
                }
            }
        }

        void StopTimer()
        {
            lock(m_lock)
            {
                if (m_timer != null)
                {
                    m_timer.Cancel();
                    m_timer = null;
                }
            }
        }

        void RestartTimer()
        {
            this.StopTimer();
            this.StartTimer();
        }

        void OnTimerFired(ThreadPoolTimer timer)
        {
            Debug.WriteLine("Background commit timer fired.");
            this.CommitChanges();
        }
        
        void SubscribeToNetworkEvents()
        {
            NetworkInformation.NetworkStatusChanged += this.OnNetworkStatusChanged;            
        }

        void UnsubscribeToNetworkEvents()
        {
            NetworkInformation.NetworkStatusChanged -= this.OnNetworkStatusChanged;
        }

        void OnNetworkStatusChanged(object sender)
        {
            try
            {
                bool hasNetwork = HealthVaultApp.HasInternetAccess();
                if (m_networkAvailable != null && m_networkAvailable.Value == hasNetwork)
                {
                    return;
                }

                Debug.WriteLine("Network status changed.");

                m_networkAvailable = hasNetwork;
                if (hasNetwork)
                {
                    this.CommitChanges();
                }
                else
                {
                    this.StopTimer();
                }
            }
            catch
            {
            }
        }

        void CommitChanges()
        {
            Task.Run(async () =>
            {
                try
                {
                    this.StopTimer();

                    // Change commit code is reentrant. If commits are already in progress, this call will
                    // be ignored
                    if (this.IsEnabled && HealthVaultApp.HasInternetAccess())
                    {
                        await m_recordStores.CommitChangesAsync();
                    }
                }
                catch
                {
                }
                finally
                {
                    this.StartTimer();
                }
            });
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.UI.Core;

namespace HealthVault
{
    internal class UIThreadDispatcher
    {
        static UIThreadDispatcher s_dispatcher;

        static UIThreadDispatcher()
        {
            s_dispatcher = new UIThreadDispatcher();
        }

        public static UIThreadDispatcher Current
        {
            get { return s_dispatcher;}
        }

        CoreDispatcher m_dispatcher;
        //long m_debugCounter;

        public CoreDispatcher Dispatcher
        {
            get { return m_dispatcher;}
            set { m_dispatcher = value;}
        }
        
        public void Init()
        {
            if (m_dispatcher != null)
            {
                return;
            }

            CoreWindow mainWindow = CoreWindow.GetForCurrentThread();
            if (mainWindow != null)
            {
                m_dispatcher = mainWindow.Dispatcher;
            }
        }

        public async Task RunAsync(DispatchedHandler method)
        {
            CoreDispatcher dispatcher = m_dispatcher;
            if (dispatcher != null)
            {
                await dispatcher.RunAsync(CoreDispatcherPriority.Normal, method);
            }
        }
        
        public void Schedule(DispatchedHandler method)
        {
            CoreDispatcher dispatcher = m_dispatcher;
            if (dispatcher != null)
            {
                // Don't want to await the completion
                Task task = dispatcher.RunAsync(CoreDispatcherPriority.Normal, method).AsTask();
            }
        }

        public async Task RunAlwaysAsync(DispatchedHandler method)
        {
            CoreDispatcher dispatcher = m_dispatcher;
            if (dispatcher != null)
            {
                await dispatcher.RunAsync(CoreDispatcherPriority.Normal, method);
                
                //System.Threading.Interlocked.Increment(ref m_debugCounter);
            }
            else
            {
                method();
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace DistrEx.Communication.Proxy
{
    /// <summary>
    /// Wrapper class for WCF duplex channel that can disposed of without throwing exeptions
    /// </summary>
    /// <typeparam name="TService">service contract</typeparam>
    public class DuplexClient<TService> : IDisposable
        where TService : ICommunicationObject
    {
        public TService Channel { get; private set; }

        public DuplexClient(TService channel)
        {
            Channel = channel;
        }

        public void Dispose()
        {
            //Dispose() on ICommunicationObject can throw
            try
            {
                Channel.Close();
            }
            catch (CommunicationException)
            {
                Channel.Abort();
            }
            catch (TimeoutException)
            {
                Channel.Abort();
            }
        }
    }
}

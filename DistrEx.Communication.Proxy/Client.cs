using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace DistrEx.Communication.Proxy
{
    /// <summary>
    /// Wrapper class for WCF channel that can disposed of without throwing exeptions
    /// </summary>
    /// <typeparam name="TService">service contract</typeparam>
    public class Client<TService> : IDisposable
        where TService : ICommunicationObject
    {
        public TService Channel { get; private set; }

        public Client(TService channel)
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

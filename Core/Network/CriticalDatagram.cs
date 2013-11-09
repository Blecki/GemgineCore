using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gem.Network
{
    internal class CriticalDatagram
    {
        internal Client to;
        internal byte[] data;
        internal uint ackID;
        internal DateTime lastSendAttempt;
        internal uint sendAttempts;
        internal Action onSuccess = null;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gem.Network
{
    public interface ISyncable
    {
        void WriteFullSync(WriteOnlyDatagram datagram);
        void ReadFullSync(ReadOnlyDatagram datagram);
    }
}

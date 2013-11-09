using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gem.Network
{
    internal enum ClientToServerMessage
    {
        Join,
        Acknowledge,
        Datagram
    }

    internal enum ServerToClientMessage
    {
        Acknowledge,
        Keepalive,
        PeerJoined,
        PeerLeft,
        Datagram
    }

}

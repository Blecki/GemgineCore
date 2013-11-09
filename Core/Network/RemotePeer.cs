using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace Gem.Network
{
    public class RemotePeer
    {
        internal List<uint> receivedDatagrams = new List<uint>();

        internal enum DatagramResponse
        {
            Deliver,
            Ignore
        }

        internal DatagramResponse newDatagramReceived(uint ackID)
        {
            if (receivedDatagrams.Count == 0)
            {
                receivedDatagrams.Add(ackID);
                return DatagramResponse.Deliver;
            }

            if (ackID <= receivedDatagrams[0])
                return DatagramResponse.Ignore;
            if (receivedDatagrams.Contains(ackID))
                return DatagramResponse.Ignore;

            var index = receivedDatagrams.FindIndex((val) => { return val > ackID; });
            if (index == -1) index = receivedDatagrams.Count;
            receivedDatagrams.Insert(index, ackID);

            while (receivedDatagrams.Count > 1 && receivedDatagrams[0] == receivedDatagrams[1] - 1)
                receivedDatagrams.RemoveAt(0);

            return DatagramResponse.Deliver;
        }
    }

    
}

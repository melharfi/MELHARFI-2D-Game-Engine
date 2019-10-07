using System;

namespace MELHARFI
{
    namespace Lidgren.Network
    {
        public sealed class NetFragmentationInfo
        {
            public int TotalFragmentCount;
            public bool[] Received;
            public int TotalReceived;
            public int FragmentSize;
        }
    }
}
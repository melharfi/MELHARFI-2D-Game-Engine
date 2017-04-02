using System;

namespace MELHARFI
{
    namespace Lidgren.Network
    {
        public sealed class NetInfos
        {
            public string Username, Pseudo;
            public string Spirit = "neutre", Sexe;
            public Int16 Level, ClasseId, SpiritLvl, Orientation = 3;
            public bool Pvp;
            public int Timestamp;
        }
    }
}
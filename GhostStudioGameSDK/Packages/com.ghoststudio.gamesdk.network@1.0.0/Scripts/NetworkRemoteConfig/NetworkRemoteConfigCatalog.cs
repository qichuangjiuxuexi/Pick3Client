using System;

namespace AppBase.Network
{
    [Serializable]
    public class NetworkRemoteConfigCatalog
    {
        public long version;
        public string[] keys;
    }
}
using System;

namespace Clouds.Data
{
    [Serializable]
    public abstract class DynamicData
    {
        public string Name;
        public DynamicData(string name)
        {
            Name = name;
        }
    }
}

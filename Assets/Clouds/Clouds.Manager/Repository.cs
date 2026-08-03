using Clouds.Data;

namespace Clouds.Manager
{
    public static class Repository<T> where T : DynamicData, new()
    {
        private static T _data;
        private static bool _isLoaded;

        public static bool IsLoaded => _isLoaded;
        public static T Data => _isLoaded ? _data : Load();

        // Generic static fields are per-closed-type (Repository<PlayerData> vs Repository<SettingData> don't
        // share state), but that also means [RuntimeInitializeOnLoadMethod] can't target an open generic —
        // Unity has no way to know which T to reset. Stale cache across domain-reload-disabled Play sessions
        // is a known limitation here; SignalBus/PopupService/PoolService are concrete classes and don't have it.
        public static T Load(string filename = null)
        {
            filename ??= typeof(T).Name + ".json";
            LoadSaveService.LoadDataFromFile(filename, out T loaded);
            _data = loaded;
            _isLoaded = true;
            return _data;
        }

        public static void Save(string filename = null)
        {
            filename ??= typeof(T).Name + ".json";
            LoadSaveService.SaveDatatofile(filename, Data);
        }
    }
}

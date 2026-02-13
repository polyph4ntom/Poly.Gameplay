using Poly.Settings;
using UnityEngine;

namespace Poly.Path
{
    public static class FPolyPathAssembly
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        public static void LoadAssembly()
        {
            FPolyDevSettingsDatabase.Register<FPolyPathSubsystemSettings>("SETTINGS_Path");
        }
    }
}

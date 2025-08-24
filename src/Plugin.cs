using BepInEx;
using BepInEx.Logging;
using Raincord100k.Hooks;

namespace Raincord100k
{
    [BepInPlugin(MOD_ID, "Raincord 100k Gallery Region", "1.0")]
    class Plugin : BaseUnityPlugin
    {
        private const string MOD_ID = "raincord_100k";
        public static new ManualLogSource Logger;


        // Add hooks
        public void OnEnable()
        {
            Logger = base.Logger;

            try
            {
                On.RainWorld.OnModsInit += Extras.WrapInit(LoadResources);

                MenuHooks.Apply();
                DevToolsHooks.Apply();
                ObjectHooks.Apply();

                Constants.Register();
            }
            catch (System.Exception e)
            {
                Logger.LogError(e);
            }

        }

        public void OnDisable()
        {
            MenuHooks.Unapply();
            DevToolsHooks.UnApply();
            ObjectHooks.UnApply();
        }
        
        // Load any resources, such as sprites or sounds
        private void LoadResources(RainWorld rainWorld)
        {
        }
    }
}
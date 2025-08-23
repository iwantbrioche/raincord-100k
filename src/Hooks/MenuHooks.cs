namespace Raincord100k.Hooks
{
    internal static class MenuHooks
    {
        public static void Apply()
        {
            On.MultiplayerUnlocks.ClassUnlocked += NoPlayInArenaHook;
        }

        public static void Unapply()
        {
            On.MultiplayerUnlocks.ClassUnlocked -= NoPlayInArenaHook;
        }

        private static bool NoPlayInArenaHook(On.MultiplayerUnlocks.orig_ClassUnlocked orig, MultiplayerUnlocks self, SlugcatStats.Name classID)
        {
            return classID != Constants.Slugcat && orig(self, classID); // cannot play gallery slugcat in arena
        }
    }
}

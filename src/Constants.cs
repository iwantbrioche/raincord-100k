using DevInterface;
using System.Collections.Generic;

namespace Raincord100k
{
    public static class Constants
    {
        public static SlugcatStats.Name Slugcat = new("Raincord100k", false);

        public static SlugcatStats.Timeline TimelinePast    = new("Raincord100k_Past",    false);
        public static SlugcatStats.Timeline TimelinePresent = new("Raincord100k_Present", false);
        public static SlugcatStats.Timeline TimelineFuture  = new("Raincord100k_Future",  false);

        public static ObjectsPage.DevObjectCategories Raincord100kObjCategory;

        public static PlacedObject.Type ConfettiPlantPOType;
        public static AbstractPhysicalObject.AbstractObjectType ConfettiPlantAOType;

        /// <summary>
        /// Registers the ExtEnum values
        /// </summary>
        internal static void Register()
        {
            Raincord100kObjCategory ??= new("100k Gallery Region", true);

            ConfettiPlantPOType ??= new("ConfettiPlant", true);
            ConfettiPlantAOType ??= new AbstractPhysicalObject.AbstractObjectType("ConfettiPlant", true);
            // Add your PlacedObject.Types here
            POTypes.Add(ConfettiPlantPOType);
        }
        
        // probably don't need this, i just wanted it cuz i wanted to try out HashSets lol
        public static HashSet<PlacedObject.Type> POTypes = [];
    }
}

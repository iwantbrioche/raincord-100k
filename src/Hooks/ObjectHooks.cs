
using Raincord100k.Objects.ConfettiPlant;

namespace Raincord100k.Hooks
{
    internal static class ObjectHooks
    {
        public static void Apply()
        {
            // Abstract Hooks
            On.AbstractConsumable.IsTypeConsumable += AbstractConsumable_IsTypeConsumable;
            On.AbstractPhysicalObject.Realize += AbstractPhysicalObject_Realize;

            // Placed Object Hooks
            On.PlacedObject.GenerateEmptyData += PlacedObject_GenerateEmptyData;
            On.Room.Loaded += Room_Loaded;
        }

        public static void UnApply()
        {
            // Abstract Hooks
            On.AbstractConsumable.IsTypeConsumable -= AbstractConsumable_IsTypeConsumable;
            On.AbstractPhysicalObject.Realize -= AbstractPhysicalObject_Realize;

            // Placed Object Hooks
            On.PlacedObject.GenerateEmptyData -= PlacedObject_GenerateEmptyData;
            On.Room.Loaded -= Room_Loaded;
        }

        private static void PlacedObject_GenerateEmptyData(On.PlacedObject.orig_GenerateEmptyData orig, PlacedObject self)
        {
            if (self.type == Constants.ConfettiPlantPOType)
            {
                self.data = new PlacedObject.ConsumableObjectData(self);
            }
            else
            {
                orig(self);
            }
        }

        private static void Room_Loaded(On.Room.orig_Loaded orig, Room self)
        {
            bool firstTimeRealized = self.abstractRoom.firstTimeRealized;

            orig(self);

            if (self.game != null)
            {
                for (int i = 0; i < self.roomSettings.placedObjects.Count; i++)
                {
                    PlacedObject pObj = self.roomSettings.placedObjects[i];
                    if (pObj.active)
                    {
                        // Requires first time realized
                        if (firstTimeRealized)
                        {
                            bool spawnConsumable = self.game.session is not StoryGameSession session || !session.saveState.ItemConsumed(self.world, false, self.abstractRoom.index, i);
                            if (pObj.type == Constants.ConfettiPlantPOType && spawnConsumable)
                            {
                                self.abstractRoom.AddEntity(new AbstractConsumable(self.world, Constants.ConfettiPlantAOType, null, self.GetWorldCoordinate(pObj.pos),
                                    self.game.GetNewID(), self.abstractRoom.index, i, pObj.data as PlacedObject.ConsumableObjectData)
                                { isConsumed = false } );
                            }
                        }
                    }
                }
            }
        }
        private static void AbstractPhysicalObject_Realize(On.AbstractPhysicalObject.orig_Realize orig, AbstractPhysicalObject self)
        {
            if (self.type == Constants.ConfettiPlantAOType)
            {
                self.realizedObject = new ConfettiPlant(self);
            }
            orig(self);
        }
        private static bool AbstractConsumable_IsTypeConsumable(On.AbstractConsumable.orig_IsTypeConsumable orig, AbstractPhysicalObject.AbstractObjectType type)
        {
            return type == Constants.ConfettiPlantAOType || orig(type);
        }
    }
}

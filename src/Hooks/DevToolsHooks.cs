
using DevInterface;

namespace Raincord100k.Hooks
{
    internal static class DevToolsHooks
    {
        public static void Apply()
        {
            On.DevInterface.ObjectsPage.DevObjectGetCategoryFromPlacedType += ObjectsPage_DevObjectGetCategoryFromPlacedType;
            On.DevInterface.ObjectsPage.CreateObjRep += ObjectsPage_CreateObjRep;
        }

        public static void UnApply()
        {
            On.DevInterface.ObjectsPage.DevObjectGetCategoryFromPlacedType -= ObjectsPage_DevObjectGetCategoryFromPlacedType;
            On.DevInterface.ObjectsPage.CreateObjRep -= ObjectsPage_CreateObjRep;
        }

        private static void ObjectsPage_CreateObjRep(On.DevInterface.ObjectsPage.orig_CreateObjRep orig, ObjectsPage self, PlacedObject.Type tp, PlacedObject pObj)
        {
            PlacedObjectRepresentation rep = null;

            if (tp == Constants.ConfettiPlantPOType)
            {
                NullCheckPObj();
                rep = new ConsumableRepresentation(self.owner, tp.ToString() + "_Rep", self, pObj, tp.ToString());
            }

            if (rep != null)
            {
                self.tempNodes.Add(rep);
                self.subNodes.Add(rep);
            }
            else
            {
                orig(self, tp, pObj);
            }

            // Checks if the pObj is null and creates one if it is
            void NullCheckPObj()
            {
                if (pObj == null)
                {
                    var camPos = self.owner.room.game.cameras[0].pos;
                    pObj = new PlacedObject(tp, null)
                    {
                        pos = camPos + Vector2.Lerp(self.owner.mousePos, new Vector2(-683f, 384f), 0.25f) + Custom.DegToVec(Random.value * 360f) * 0.2f
                    };
                    pObj.pos = Custom.RestrictInRect(pObj.pos, new FloatRect(camPos.x, camPos.y, camPos.x + 1366f, camPos.y + 768f));
                    self.RoomSettings.placedObjects.Add(pObj);
                }
            }
        }

        private static ObjectsPage.DevObjectCategories ObjectsPage_DevObjectGetCategoryFromPlacedType(On.DevInterface.ObjectsPage.orig_DevObjectGetCategoryFromPlacedType orig, DevInterface.ObjectsPage self, PlacedObject.Type type)
        {
            if (Constants.POTypes.Contains(type))
            {
                return Constants.Raincord100kObjCategory;
            }
            return orig(self, type);
        }
    }
}

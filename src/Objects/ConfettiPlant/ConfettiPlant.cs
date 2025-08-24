
namespace Raincord100k.Objects.ConfettiPlant
{
    public class ConfettiPlant : PhysicalObject, IDrawable
    {
        public ConfettiPlant(AbstractPhysicalObject abstractObj) : base(abstractObj)
        {
            room = abstractObj.world.GetAbstractRoom(abstractObj.pos).realizedRoom;

            bodyChunks = new BodyChunk[1];
            bodyChunkConnections = [];

            bodyChunks[0] = new(this, 0, default, 10f, 1f);
            collisionLayer = 1;
            airFriction = 0.9f;
            gravity = 0.9f;
            bounce = 0.6f;
            surfaceFriction = 0.8f;
            waterFriction = 0.95f;
            buoyancy = 1.1f;

            bodyChunks[0].HardSetPosition(room.MiddleOfTile(abstractObj.pos));


            UnityEngine.Debug.Log("i alive");
        }
        public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[1];
            sLeaser.sprites[0] = new("Futile_White");

            AddToContainer(sLeaser, rCam, null);
        }
        public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            newContatiner ??= rCam.ReturnFContainer("Midground");

            newContatiner.AddChild(sLeaser.sprites[0]);
        }
        public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            Vector2 drawPos = Vector2.Lerp(bodyChunks[0].lastPos, bodyChunks[0].pos, timeStacker);

            sLeaser.sprites[0].SetPosition(drawPos - camPos);

            if (slatedForDeletetion || room != rCam.room)
            {
                sLeaser.CleanSpritesAndRemove();
            }
        }
        public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
        }

    }
}

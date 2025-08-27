
using MoreSlugcats;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Raincord100k.Objects.ConfettiPlant
{
    public class ConfettiPlant : PlayerCarryableItem, IDrawable, IHaveAStalk
    {
        public class Stalk : UpdatableAndDeletable, IDrawable
        {
            public struct String
            {
                public Vector2 goal;
                //public float snapPercent;
                //public bool failed;
                public int segmentIndex;
            }
            public ConfettiPlant nest;
            public SimpleSegment[] segments;
            public List<String> strings;
            public Vector2[] displacements;
            public Vector2 stuckPos;
            public float ropeLength;
            public float connRad;
            public int releaseCounter;
            public bool stringsBroke;
            public int randomSeed;

            public Stalk(ConfettiPlant nest, Room room, Vector2 nestPos)
            {
                this.nest = nest;
                this.room = room;
                randomSeed = nest.abstractPhysicalObject.ID.RandomSeed;
                stuckPos.x = nestPos.x;
                int x = room.GetTilePosition(nestPos).x;
                for (int i = room.GetTilePosition(nestPos).y; i < room.TileHeight; i++) 
                {
                    if (room.GetTile(x, i).Solid)
                    {
                        stuckPos.y = room.MiddleOfTile(x, i).y - 10f;
                        ropeLength = Mathf.Abs(stuckPos.y - nestPos.y);
                        break;
                    }
                }
                segments = new SimpleSegment[Math.Max(2, (int)(ropeLength / 6f))];
                for (int i = 0; i < segments.Length; i++)
                {
                    float t = (float)i / (float)(segments.Length - 1);
                    segments[i].pos = Vector2.Lerp(stuckPos, nestPos, t);
                    segments[i].lastPos = segments[i].pos;
                }
                connRad = ropeLength / Mathf.Pow(segments.Length, 1.1f);
                displacements = new Vector2[segments.Length];

                Random.State state = Random.state;
                Random.InitState(randomSeed);
                for (int i = 0; i < displacements.Length; i++)
                {
                    displacements[i] = Custom.DegToVec(Random.Range(75f, 135f) * Mathf.Sign(Random.value - 0.5f));
                }

                strings = [];
                int stringAmount = Math.Max(4, (int)(Random.value * 16f));
                for (int i = 0; i < stringAmount; i++)
                {
                    float f = (float)i / (float)(stringAmount - 1);
                    int index = Random.Range(1, segments.Length);
                    Vector2? ray = SharedPhysics.ExactTerrainRayTracePos(room, segments[index].pos, stuckPos + new Vector2((f - 0.5f) * 30f + Random.Range(-3f, 3f), 0f));
                    if (ray.HasValue)
                    {
                        strings.Add(new String() { goal = ray.Value, segmentIndex = index });
                    }
                }
                Random.state = state;
            }
            public override void Update(bool eu)
            {
                base.Update(eu);
                if (ropeLength == -1f)
                {
                    Destroy();
                    return;
                }
                for (int i = 0; i < segments.Length; i++)
                {
                    segments[i].lastPos = segments[i].pos;
                }

                ConnectSegments(dir: true);
                ConnectSegments(dir: false);
                for (int i = 0; i < segments.Length; i++)
                {
                    segments[i].pos += segments[i].vel;
                    segments[i].vel *= 0.8f;
                    segments[i].vel.y -= room.gravity;
                }
                ConnectSegments(dir: false);
                ConnectSegments(dir: true);
                if (releaseCounter > 0)
                {
                    releaseCounter--;
                }
                if (nest != null)
                {
                    nest.setRotation = Custom.DirVec(nest.firstChunk.pos, Vector2.Lerp(segments[segments.Length - 1].pos, segments[segments.Length - 2].pos, 0.5f)) * 0.25f;
                    if (!Custom.DistLess(nest.firstChunk.pos, stuckPos, ropeLength * 1.5f + 20f) || nest.slatedForDeletetion || nest.room != room || releaseCounter == 1)
                    {
                        nest.abstractConsumable.Consume();
                        room.PlaySound(SoundID.Lizard_Jaws_Shut_Miss_Creature, nest.firstChunk, loop: false, 0.8f, 1.6f + Random.value / 10f);
                        nest.stalk = null;
                        nest = null;
                    }
                }
            }
            public void ConnectSegments(bool dir)
            {
                int index = !dir ? segments.Length - 1 : 0;
                bool finished = false;
                while (!finished)
                {
                    Vector2 stalkDir;
                    if (index == 0)
                    {
                        if (!Custom.DistLess(segments[index].pos, stuckPos, connRad))
                        {
                            stalkDir = Custom.DirVec(segments[index].pos, stuckPos) * (Vector2.Distance(segments[index].pos, stuckPos) - connRad);
                            segments[index].pos += stalkDir;
                            segments[index].vel += stalkDir;
                        }
                    }
                    else
                    {
                        if (!Custom.DistLess(segments[index].pos, segments[index - 1].pos, connRad))
                        {
                            stalkDir = Custom.DirVec(segments[index].pos, segments[index - 1].pos) * (Vector2.Distance(segments[index].pos, segments[index - 1].pos) - connRad);
                            segments[index].pos += stalkDir * 0.5f;
                            segments[index].vel += stalkDir * 0.25f;
                            segments[index].pos += displacements[index] * 0.25f;
                            segments[index].vel += displacements[index] * 0.25f;
                            segments[index - 1].pos -= stalkDir * 0.5f;
                            segments[index - 1].vel -= stalkDir * 0.25f;
                            segments[index - 1].pos -= displacements[index] * 0.25f;
                            segments[index - 1].vel -= displacements[index] * 0.25f;
                        }
                        if (index == segments.Length - 1 && nest != null && !Custom.DistLess(segments[index].pos, nest.firstChunk.pos, connRad))
                        {
                            stalkDir = Custom.DirVec(segments[index].pos, nest.firstChunk.pos) * (Vector2.Distance(segments[index].pos, nest.firstChunk.pos) - connRad);
                            segments[index].pos += stalkDir * 0.5f;
                            segments[index].vel += stalkDir * 0.25f;
                            nest.firstChunk.pos -= stalkDir * 0.25f;
                            nest.firstChunk.vel -= stalkDir * 0.25f;
                        }
                    }
                    index += dir ? 1 : -1;
                    if (dir && index >= segments.Length)
                    {
                        finished = true;
                    }
                    else if (!dir && index < 0)
                    {
                        finished = true;
                    }
                }
            }
            public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
            {
                sLeaser.sprites = new FSprite[1 + strings.Count];
                sLeaser.sprites[0] = TriangleMesh.MakeLongMesh(segments.Length, pointyTip: false, customColor: false);
                for (int i = 0; i < strings.Count; i++)
                {
                    sLeaser.sprites[1 + i] = new("pixel")
                    {
                        anchorY = 0f
                    };
                }

                AddToContainer(sLeaser, rCam, null);
            }
            public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
            {
                newContatiner ??= rCam.ReturnFContainer("Background");

                for (int i = 0; i < sLeaser.sprites.Length; i++)
                {
                    sLeaser.sprites[i].RemoveFromContainer();
                    newContatiner.AddChild(sLeaser.sprites[i]);
                }
            }
            public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
            {
                Vector2 lastStalkPos = stuckPos;
                float lastWidth = 2f;
                for (int i = 0; i < segments.Length; i++)
                {
                    float t = (float)i / (float)(segments.Length - 1);
                    float width = Mathf.Lerp(0.5f, 1.5f, Mathf.InverseLerp(0.5f, 0f, t)) + Mathf.Lerp(1f, 3f, Mathf.Sin(Mathf.Pow(t, 3.5f) * Mathf.PI));
                    Vector2 stalkPos = segments[i].DrawPos(timeStacker);
                    if (i == segments.Length - 1 && nest != null)
                    {
                        stalkPos = Vector2.Lerp(nest.firstChunk.lastPos, nest.firstChunk.pos, timeStacker) + Custom.RotateAroundOrigo(new Vector2(0f, 4f), Custom.VecToDeg(Custom.DirVec(nest.firstChunk.pos, segments[i].pos)));
                    }
                    Vector2 normalized = (lastStalkPos - stalkPos).normalized;
                    Vector2 perpendicular = Custom.PerpendicularVector(normalized);
                    stalkPos = new Vector2(Mathf.Floor(stalkPos.x) + 0.5f, Mathf.Floor(stalkPos.y) + 0.5f);
                    (sLeaser.sprites[0] as TriangleMesh).MoveVertice(i * 4, lastStalkPos - perpendicular * lastWidth - camPos);
                    (sLeaser.sprites[0] as TriangleMesh).MoveVertice(i * 4 + 1, lastStalkPos + perpendicular * lastWidth - camPos);
                    (sLeaser.sprites[0] as TriangleMesh).MoveVertice(i * 4 + 2, stalkPos - perpendicular * width - camPos);
                    (sLeaser.sprites[0] as TriangleMesh).MoveVertice(i * 4 + 3, stalkPos + perpendicular * width - camPos);
                    lastStalkPos = stalkPos;
                    lastWidth = width;
                }
                Random.State state = Random.state;
                Random.InitState(randomSeed);
                for (int i = 0; i < strings.Count; i++)
                {
                    sLeaser.sprites[1 + i].SetPosition(segments[strings[i].segmentIndex].DrawPos(timeStacker) + new Vector2((Random.value - 0.5f) * 4f, 0f) - camPos);
                    sLeaser.sprites[1 + i].rotation = Custom.VecToDeg(Custom.DirVec(segments[strings[i].segmentIndex].DrawPos(timeStacker), strings[i].goal));
                    sLeaser.sprites[1 + i].scaleY = Mathf.Max(1f, Vector2.Distance(segments[strings[i].segmentIndex].DrawPos(timeStacker), strings[i].goal));
                }
                Random.state = state;


                if (slatedForDeletetion || room != rCam.room)
                {
                    sLeaser.CleanSpritesAndRemove();
                }
            }
            public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
            {
                sLeaser.sprites[0].color = Color.Lerp(palette.blackColor, Color.green, 0.15f);
                for (int i = 0; i < strings.Count; i++)
                {
                    sLeaser.sprites[1 + i].color = Color.Lerp(palette.blackColor, Color.green, 0.1f);
                }
            }
        }

        public Stalk stalk;
        private Vector2 rotation;
        private Vector2 lastRotation;
        private Vector2? setRotation;
        private Vector2 homePos;
        public AbstractConsumable abstractConsumable => abstractPhysicalObject as AbstractConsumable;
        public ConfettiPlant(AbstractPhysicalObject abstractObj) : base(abstractObj)
        {
            room = abstractObj.world.GetAbstractRoom(abstractObj.pos).realizedRoom;

            bodyChunks = new BodyChunk[1];
            bodyChunkConnections = [];

            bodyChunks[0] = new(this, 0, default, 9f, 0.4f);
            collisionLayer = 1;
            airFriction = 0.999f;
            gravity = 0.9f;
            bounce = 0.6f;
            surfaceFriction = 0.5f;
            waterFriction = 0.95f;
            buoyancy = 1.1f;

        }
        public override void Update(bool eu)
        {
            base.Update(eu);
            lastRotation = rotation;
            if (grabbedBy.Count > 0)
            {
                rotation = Custom.PerpendicularVector(Custom.DirVec(firstChunk.pos, grabbedBy[0].grabber.mainBodyChunk.pos));
                rotation.y = Mathf.Abs(rotation.y);
            }
            if (setRotation.HasValue)
            {
                rotation = setRotation.Value;
                setRotation = null;
            }
            if (firstChunk.ContactPoint.y < 0)
            {
                rotation = (rotation - Custom.PerpendicularVector(rotation) * 0.1f * firstChunk.vel.x).normalized;
                firstChunk.vel.x *= 0.8f;
            }
            if (!abstractConsumable.isConsumed)
            {
                //if (!Custom.DistLess(homePos, firstChunk.pos, 25f))
                //{
                //    Vector2 homeDir = Custom.DirVec(homePos, firstChunk.pos) * Vector2.Distance(homePos, firstChunk.pos);
                //    firstChunk.pos -= homeDir * 0.5f;
                //    firstChunk.vel -= homeDir * 0.5f;
                //}
            }

        }
        public override void PlaceInRoom(Room placeRoom)
        {
            base.PlaceInRoom(placeRoom);
            if (ModManager.MMF && room.game.IsArenaSession && (MMF.cfgSandboxItemStems.Value || room.game.GetArenaGameSession.chMeta != null) && room.game.GetArenaGameSession.counter < 10)
            {
                firstChunk.HardSetPosition(placeRoom.MiddleOfTile(abstractPhysicalObject.pos));
                homePos = placeRoom.MiddleOfTile(abstractPhysicalObject.pos);
                stalk = new(this, placeRoom, firstChunk.pos);
                placeRoom.AddObject(stalk);
                abstractConsumable.isConsumed = false;
            }
            else if (!abstractConsumable.isConsumed && abstractConsumable.placedObjectIndex >= 0 && abstractConsumable.placedObjectIndex < placeRoom.roomSettings.placedObjects.Count)
            {
                firstChunk.HardSetPosition(placeRoom.roomSettings.placedObjects[abstractConsumable.placedObjectIndex].pos);
                homePos = placeRoom.MiddleOfTile(abstractPhysicalObject.pos);
                stalk = new(this, placeRoom, firstChunk.pos);
                placeRoom.AddObject(stalk);
            }
            else
            {
                firstChunk.HardSetPosition(placeRoom.MiddleOfTile(abstractPhysicalObject.pos));
            }
        }
        public void DetatchStalk()
        {
            if (stalk != null && stalk.releaseCounter == 0)
            {
                stalk.releaseCounter = 2;
            }
        }
        public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[6];
            for (int i = 0; i < sLeaser.sprites.Length; i++)
            {
                if (i < 3)
                {
                    sLeaser.sprites[i] = new FSprite($"ConfettiBulbA{i}");
                }
                else
                {
                    sLeaser.sprites[i] = new CustomFSprite($"ConfettiBulbB{i % 3}");
                }

            }

            AddToContainer(sLeaser, rCam, null);
        }
        public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            newContatiner ??= rCam.ReturnFContainer("Items");

            for (int i = 0; i < sLeaser.sprites.Length; i++)
            {
                sLeaser.sprites[i].RemoveFromContainer();
                newContatiner.AddChild(sLeaser.sprites[i]);
            }
        }
        public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            UnityEngine.Debug.Log($"lastLastPos: {firstChunk.lastLastPos}");
            UnityEngine.Debug.Log($"lastPos: {firstChunk.lastPos}");
            UnityEngine.Debug.Log($"pos: {firstChunk.pos}");
            UnityEngine.Debug.Log($"vel: {firstChunk.vel}");

            Vector2 drawPos = Vector2.Lerp(bodyChunks[0].lastPos, bodyChunks[0].pos, timeStacker);
            Vector2 drawRot = Vector2.Lerp(lastRotation, rotation, timeStacker);
            UnityEngine.Debug.Log($"drawPos: {drawPos}");

            for (int i = 0; i < sLeaser.sprites.Length; i++)
            {
                if (i < 3)
                {
                    sLeaser.sprites[i].SetPosition(drawPos - camPos);
                    sLeaser.sprites[i].rotation = Custom.VecToDeg(drawRot);
                }
                else
                {
                    float x = sLeaser.sprites[i].localRect.x;
                    float y = sLeaser.sprites[i].localRect.y;
                    float width = sLeaser.sprites[i].localRect.width;
                    float height = sLeaser.sprites[i].localRect.height;
                    float rotDeg = Custom.VecToDeg(drawRot);
                    (sLeaser.sprites[i] as CustomFSprite).MoveVertice(0, drawPos + Custom.rotateVectorDeg(new Vector2(x, y + height), rotDeg) - camPos);
                    (sLeaser.sprites[i] as CustomFSprite).MoveVertice(1, drawPos + Custom.rotateVectorDeg(new Vector2(x + width, y + height), rotDeg) - camPos);
                    (sLeaser.sprites[i] as CustomFSprite).MoveVertice(2, drawPos + Custom.rotateVectorDeg(new Vector2(x + width, y), rotDeg) - camPos);
                    (sLeaser.sprites[i] as CustomFSprite).MoveVertice(3, drawPos + Custom.rotateVectorDeg(new Vector2(x, y), rotDeg) - camPos);

                }

            }

            if (slatedForDeletetion || room != rCam.room)
            {
                sLeaser.CleanSpritesAndRemove();
            }
        }
        public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            for (int i = 0; i < sLeaser.sprites.Length; i++)
            {
                if (i < 3)
                {
                    sLeaser.sprites[i].color = new Color(1f, 0.7f, 0f);
                }
                else
                {
                    (sLeaser.sprites[i] as CustomFSprite).verticeColors[0] = new Color(0.8f, 0f, 0f);
                    (sLeaser.sprites[i] as CustomFSprite).verticeColors[1] = new Color(0.8f, 0f, 0.8f);
                    (sLeaser.sprites[i] as CustomFSprite).verticeColors[2] = new Color(0f, 0f, 0.8f);
                    (sLeaser.sprites[i] as CustomFSprite).verticeColors[3] = new Color(0f, 0.8f, 0f);
                }

            }
        }
    }
}

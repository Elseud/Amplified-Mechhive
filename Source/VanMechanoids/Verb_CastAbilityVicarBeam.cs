using HotSwap;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;
using static UnityEngine.GraphicsBuffer;
using VanillaWeaponsExpandedLaser;
using System.Configuration;
using static UnityEngine.UI.Image;
using UnityEngine;
using VFECore;

namespace VanMechanoids
{
    [HotSwap.HotSwappable]
    public class VicarBeamHolder : MapComponent
    {
        public Dictionary<Pawn, BeamRenderer> beamUsers;
        public Dictionary<Pawn, Pawn> usersAndTargets;
        public bool tickHappened = false;

        public VicarBeamHolder(Map map)
            : base(map)
        {
            beamUsers = new Dictionary<Pawn, BeamRenderer>();
            usersAndTargets = new Dictionary<Pawn, Pawn>();
        }

        public override void MapComponentUpdate()
        {
            base.MapComponentUpdate();
            if (tickHappened)
            {
                tickHappened = false;
                foreach (Pawn pawn in beamUsers.Keys)
                {
                    beamUsers[pawn].Render(pawn.DrawPos, usersAndTargets[pawn].DrawPos);
                }
            }
        }

        public override void MapComponentTick()
        {
            base.MapComponentTick();
            tickHappened = true;
        }

        public void AddBeam(Pawn user, Pawn target)
        {
            BeamRenderer beam = ThingMaker.MakeThing(VM_DefOf.VM_VicarBeamGraphic) as BeamRenderer;
            beam.InitializeValues();
            beam.Render(user.DrawPos, target.DrawPos);
            GenSpawn.Spawn(beam, (target.DrawPos.Yto0() + Vector3.up * VM_DefOf.VM_VicarBeamGraphic.Altitude).ToIntVec3(), user.Map);
            beamUsers[user] = beam;
            usersAndTargets[user] = target;
        }

        public void DespawnBeam(BeamRenderer beam)
        {
            Pawn user = beamUsers.FirstOrDefault(x => x.Value == beam).Key;
            beamUsers.Remove(user);
            usersAndTargets.Remove(user);
            beam.Destroy();
        }

        public void DespawnBeam(Pawn user)
        {
            beamUsers[user].Destroy();
            beamUsers.Remove(user);
            usersAndTargets.Remove(user);
        }
    }

    [HotSwap.HotSwappable]
    public class Verb_CastAbilityVicarBeam : Verb_CastAbility
    {
        public bool cachedCanHit = false;
        public IntVec3 cachedPosition;
        public IntVec3 cachedRoot;

        protected override bool TryCastShot()
        {
            if (!base.TryCastShot())
            {
                return false;
            }

            CasterPawn.MapHeld.GetComponent<VicarBeamHolder>().AddBeam(CasterPawn, currentTarget.Thing as Pawn);

            return true;
        }

        public override bool ValidateTarget(LocalTargetInfo target, bool showMessages = true)
        {
            if (CasterPawn == null || target == null)
            {
                return false;
            }

            if (!target.HasThing)
            {
                return false;
            }

            if (!(target.Thing is Pawn))
            {
                return false;
            }

            return !CasterPawn.MapHeld.GetComponent<VicarBeamHolder>().beamUsers.ContainsKey(CasterPawn) && !CasterPawn.MapHeld.GetComponent<VicarBeamHolder>().usersAndTargets.ContainsValue(target.Thing as Pawn) && this.CanHitTarget(target);
        }

        public override bool CanHitTargetFrom(IntVec3 root, LocalTargetInfo targ)
        {
            if (CasterPawn == null)
            {
                return false;
            }

            if (cachedRoot != root || cachedPosition != targ.Cell)
            {
                cachedRoot = root;
                cachedPosition = targ.Cell;
                cachedCanHit = GenSight.LineOfSight(root, targ.Cell, CasterPawn.MapHeld);
            }

            return cachedCanHit;
        }
    }

    [HotSwap.HotSwappable]
    public class BeamRenderer : ThingWithComps
    {
        public Vector3 firstPoint;
        public Vector3 secondPoint;
        public Matrix4x4 matrix;
        public List<List<Material>> materials;
        public Material material;
        public int maxIterator = 0;
        public int textureIterator = 0;
        public int materialCounter = 0;
        public int maxMaterials = 0;
        public int maxSizes = 0;
        public int sizeCounter = 1;
        public float maxRange = 25.9f;
        public bool multipleTex = false;
        public bool finishedTextures = false;

        public void InitializeValues()
        {
            BeamExtension extension = def.GetModExtension<BeamExtension>();
            materials = new List<List<Material>>();

            maxSizes = extension.sizeIterations;
            maxMaterials = extension.textureAmount;
            maxRange = extension.maxRange;

            string texPath = def.graphicData.texPath;

            if (extension.textureAmount < 2)
            {
                material = MaterialPool.MatFrom(texPath, ShaderDatabase.MoteGlow);
            }
            else
            {
                multipleTex = true;
                for (int i = 0; i < maxSizes; i++)
                {
                    List<Material> sizeMaterials = new List<Material>();
                    for (int j = 0; j < maxMaterials; j++)
                    {
                        sizeMaterials.Add(MaterialPool.MatFrom(texPath + (i + 1) + (j + 1), ShaderDatabase.MoteGlow));
                    }
                    materials.Add(sizeMaterials);
                }
                maxIterator = extension.textureIterationSpeed;
            }
            finishedTextures = true;
        }

        public void Render(Vector3 firstPoint, Vector3 secondPoint)
        {
            this.firstPoint = firstPoint.Yto0();
            this.secondPoint = secondPoint.Yto0();
            matrix.SetTRS((firstPoint + secondPoint) / 2, Quaternion.LookRotation(secondPoint - firstPoint), new Vector3(def.graphicData.drawSize.x, 1f, (firstPoint - secondPoint).magnitude));
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
        }

        public override void Draw()
        {
            if (!finishedTextures)
            {
                return;
            }

            Graphics.DrawMesh(MeshPool.plane10, matrix, material, 0);
        }

        public override void Tick()
        {
            base.Tick();

            if (multipleTex)
            {
                textureIterator += 1;
                if (textureIterator > maxIterator)
                {
                    textureIterator = 0;
                    materialCounter++;
                    if (materialCounter >= maxMaterials)
                    {
                        materialCounter = 0;
                    }
                    material = materials[sizeCounter][materialCounter];
                }
            }

            if (this.IsHashIntervalTick(20))
            {
                if (maxSizes > 1)
                {
                    if (firstPoint == secondPoint)
                    {
                        sizeCounter = 0;
                    }
                    else
                    {
                        float distance = (firstPoint - secondPoint).magnitude;
                        if (distance > maxRange)
                        {
                            MapHeld.GetComponent<VicarBeamHolder>().DespawnBeam(this);
                            return;
                        }
                        sizeCounter = (int)Math.Min(Math.Ceiling(distance / (maxRange / maxSizes)) - 1, maxSizes);
                    }
                    material = materials[sizeCounter][materialCounter];
                }
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref firstPoint, "firstPoint");
            Scribe_Values.Look(ref secondPoint, "secondPoint");
            Scribe_Values.Look(ref maxIterator, "maxIterator");
            Scribe_Values.Look(ref maxMaterials, "maxMaterials");
            Scribe_Values.Look(ref maxSizes, "maxSizes");
            Scribe_Values.Look(ref multipleTex, "multipleTex");
        }
    }

    [HotSwap.HotSwappable]
    public class BeamExtension : DefModExtension
    {
        public float maxRange;
        public int textureIterationSpeed;
        public int sizeIterations;
        public int textureAmount = 1;
    }
}

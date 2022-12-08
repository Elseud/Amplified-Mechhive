using AthenaFramework;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using Verse;
using Verse.AI;
using Verse.Noise;
using static HarmonyLib.Code;
using HotSwap;

namespace AmplifiedMechhive
{
    [HotSwappable]
    public class Comp_BishopBlessing : ThingComp
    {
        public CompProperties_BishopBlessing Props => props as CompProperties_BishopBlessing;

        private Matrix4x4 matrix;
        private Color color = Color.white;

        public override void PostDraw()
        {
            base.PostDraw();
            matrix.SetTRS(parent.DrawPos, new Quaternion(), new Vector3(Props.range * 2 + 1, 1f, Props.range * 2 + 1));
            color.a = 1.149575f * (0.3f + (1f + (float)Math.Sin(Math.PI / 90 * Find.TickManager.TicksGame)) * 0.35f);
            Props.graphicData.Graphic.MatSingle.SetColor("_Color", color);
            Graphics.DrawMesh(MeshPool.plane10, matrix, Props.graphicData.Graphic.MatSingle, 0);
        }

        public override void CompTick()
        {
            base.CompTick();
            if (parent.IsHashIntervalTick(Props.updateFrequency))
            {
                Pawn pawn = parent as Pawn;
                if (pawn == null)
                {
                    return;
                }

                foreach (Pawn ally in PawnGroupUtility.GetNearbyAllies(pawn, Props.range))
                {
                    if (pawn == ally)
                    {
                        continue;
                    }

                    Hediff buffHediff = ally.health.hediffSet.GetFirstHediffOfDef(Props.hediffDef);
                    if (buffHediff != null)
                    {
                        continue;
                    }

                    HediffWithComps hediff = HediffMaker.MakeHediff(Props.hediffDef, ally) as HediffWithComps;
                    ally.health.AddHediff(hediff);
                    HediffComp_BishopBlessing blessingHediff = hediff.comps.OfType<HediffComp_BishopBlessing>().ToList()[0];
                    blessingHediff.bishopComp = this;
                    blessingHediff.bishop = parent as Pawn;
                }
            }
        }
    }

    public class CompProperties_BishopBlessing : CompProperties
    {
        public CompProperties_BishopBlessing()
        {
            compClass = typeof(Comp_BishopBlessing);
        }

        public HediffDef hediffDef;
        public float range;
        public int updateFrequency = 300;
        public GraphicData graphicData;
    }
}

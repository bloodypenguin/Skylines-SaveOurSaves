using System;
using System.Diagnostics;
using System.Reflection;
using ColossalFramework;
using ColossalFramework.Math;
using UnityEngine;

namespace SaveOurSaves
{
    public class BuildingManagerDetour : BuildingManager
    {
        private static RedirectCallsState _state1;
        private static RedirectCallsState _state2;
        private static bool deployed;

        public static void Deploy()
        {
            if (!deployed)
            {
                //                RedirectionHelper.RedirectCalls(
                //                    typeof(BuildingManager).GetMethod("UpdateBuildingRenderer", BindingFlags.Instance | BindingFlags.Public, null, new[]{typeof(ushort), typeof(bool)} , null),
                //                    typeof(BuildingManagerDetour).GetMethod("UpdateBuildingRenderer", BindingFlags.Instance | BindingFlags.Public, null, new[] { typeof(ushort), typeof(bool) }, null));
                //
                _state1 = RedirectionHelper.RedirectCalls(
                   typeof(BuildingManager).GetMethod("OverlapQuad", new[] { typeof(Quad2), typeof(float), typeof(float), typeof(ItemClass.Layer), typeof(ushort), typeof(ushort), typeof(ushort), typeof(ulong[]) }),
                   typeof(BuildingManagerDetour).GetMethod("OverlapQuad", new[] { typeof(Quad2), typeof(float), typeof(float), typeof(ItemClass.Layer), typeof(ushort), typeof(ushort), typeof(ushort), typeof(ulong[]) }));

                _state2 = RedirectionHelper.RedirectCalls(
                    typeof(BuildingManager).GetMethod("FindBuilding"),
                    typeof(BuildingManagerDetour).GetMethod("FindBuilding"));


                deployed = true;
            }
        }

        public static void Revert()
        {
            if (deployed)
            {
                RedirectionHelper.RevertRedirect(
                    typeof(BuildingManager).GetMethod("OverlapQuad", new[] { typeof(Quad2), typeof(float), typeof(float), typeof(ItemClass.Layer), typeof(ushort), typeof(ushort), typeof(ushort), typeof(ulong[]) }),
                    _state1);
                RedirectionHelper.RevertRedirect(
                    typeof(BuildingManager).GetMethod("FindBuilding"),
                    _state2);

                deployed = false;
            }
        }

        public void UpdateBuildingRenderer(ushort building, bool updateGroup)
        {
            var buildingObj = this.m_buildings.m_buffer[(int)building];
            if (buildingObj.Info == null)
            {
                this.m_buildings.m_buffer[(int)building].m_flags = Building.Flags.None;
            }
            this.UpdateBuildingRenderer(building, ref buildingObj, updateGroup);
        }

        public bool OverlapQuad(Quad2 quad, float minY, float maxY, ItemClass.Layer layers, ushort ignoreBuilding, ushort ignoreNode1, ushort ignoreNode2, ulong[] buildingMask)
        {
            var ignoreOverlap = typeof (BuildingManager).GetMethod("IgnoreOverlap",
                BindingFlags.NonPublic | BindingFlags.Instance);

            Vector2 vector2_1 = quad.Min();
            Vector2 vector2_2 = quad.Max();
            int num1 = Mathf.Max((int)(((double)vector2_1.x - 72.0) / 64.0 + 135.0), 0);
            int num2 = Mathf.Max((int)(((double)vector2_1.y - 72.0) / 64.0 + 135.0), 0);
            int num3 = Mathf.Min((int)(((double)vector2_2.x + 72.0) / 64.0 + 135.0), 269);
            int num4 = Mathf.Min((int)(((double)vector2_2.y + 72.0) / 64.0 + 135.0), 269);
            bool flag = false;
            for (int index1 = num2; index1 <= num4; ++index1)
            {
                for (int index2 = num1; index2 <= num3; ++index2)
                {
                    ushort num5 = this.m_buildingGrid[index1 * 270 + index2];
                    int num6 = 0;
                    while ((int)num5 != 0)
                    {
                        //mod: add null check
                        BuildingInfo info = this.m_buildings.m_buffer[(int)num5].Info;
                        if ((layers == ItemClass.Layer.None || (info!=null && (info.m_class.m_layer & layers) != ItemClass.Layer.None)) && (!(bool)ignoreOverlap.Invoke(this, new object[] { num5, ignoreBuilding, ignoreNode1, ignoreNode2 }) && this.m_buildings.m_buffer[(int)num5].OverlapQuad(num5, quad, minY, maxY)))
                        {
                            if (buildingMask == null)
                                return true;
                            buildingMask[(int)num5 >> 6] |= (ulong)(1L << (int)num5);
                            flag = true;
                        }
                        num5 = this.m_buildings.m_buffer[(int)num5].m_nextGridBuilding;
                        if (++num6 >= 49152)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                            break;
                        }
                    }
                }
            }
            return flag;
        }

        public ushort FindBuilding(Vector3 pos, float maxDistance, ItemClass.Service service, ItemClass.SubService subService, Building.Flags flagsRequired, Building.Flags flagsForbidden)
        {
            int num1 = Mathf.Max((int)(((double)pos.x - (double)maxDistance) / 64.0 + 135.0), 0);
            int num2 = Mathf.Max((int)(((double)pos.z - (double)maxDistance) / 64.0 + 135.0), 0);
            int num3 = Mathf.Min((int)(((double)pos.x + (double)maxDistance) / 64.0 + 135.0), 269);
            int num4 = Mathf.Min((int)(((double)pos.z + (double)maxDistance) / 64.0 + 135.0), 269);
            ushort num5 = (ushort)0;
            float num6 = maxDistance * maxDistance;
            for (int index1 = num2; index1 <= num4; ++index1)
            {
                for (int index2 = num1; index2 <= num3; ++index2)
                {
                    ushort num7 = this.m_buildingGrid[index1 * 270 + index2];
                    int num8 = 0;
                    while ((int)num7 != 0)
                    {
                        BuildingInfo info = this.m_buildings.m_buffer[(int)num7].Info;
                        //mod: add null check
                        if (info != null)
                        {
                            if ((info.m_class.m_service == service || service == ItemClass.Service.None) && (info.m_class.m_subService == subService || subService == ItemClass.SubService.None) && (this.m_buildings.m_buffer[(int)num7].m_flags & (flagsRequired | flagsForbidden)) == flagsRequired)
                            {
                                float num9 = Vector3.SqrMagnitude(pos - this.m_buildings.m_buffer[(int)num7].m_position);
                                if ((double)num9 < (double)num6)
                                {
                                    num5 = num7;
                                    num6 = num9;
                                }
                            }
                        }

                        num7 = this.m_buildings.m_buffer[(int)num7].m_nextGridBuilding;
                        if (++num8 >= 49152)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + System.Environment.StackTrace);
                            break;
                        }
                    }
                }
            }
            return num5;
        }
    }
}
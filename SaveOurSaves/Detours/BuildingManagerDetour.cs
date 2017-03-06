using ColossalFramework;
using SaveOurSaves.Redirection;
using UnityEngine;

namespace SaveOurSaves.Detours
{
    [TargetType(typeof(BuildingManager))]
    public class BuildingManagerDetour : BuildingManager
    {
        [RedirectMethod]
        public override bool CalculateGroupData(int groupX, int groupZ, int layer, ref int vertexCount,
            ref int triangleCount, ref int objectCount, ref RenderGroup.VertexArrays vertexArrays)
        {
            bool flag = false;
            int num1 = groupX * 270 / 45;
            int num2 = groupZ * 270 / 45;
            int num3 = (groupX + 1) * 270 / 45 - 1;
            int num4 = (groupZ + 1) * 270 / 45 - 1;
            for (int index1 = num2; index1 <= num4; ++index1)
            {
                for (int index2 = num1; index2 <= num3; ++index2)
                {
                    ushort buildingID = this.m_buildingGrid[index1 * 270 + index2];
                    int num5 = 0;
                    while ((int) buildingID != 0)
                    {
                        //add null check
                        //begin mod
                        if (this.m_buildings.m_buffer[(int) buildingID].Info != null)
                        {
                            if (this.m_buildings.m_buffer[(int) buildingID].CalculateGroupData(buildingID, layer,
                                ref vertexCount, ref triangleCount, ref objectCount, ref vertexArrays))
                                flag = true;
                        }
                        //end mod
                        buildingID = this.m_buildings.m_buffer[(int) buildingID].m_nextGridBuilding;
                        if (++num5 >= 49152)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core,
                                "Invalid list detected!\n" + System.Environment.StackTrace);
                            break;
                        }
                    }
                }
            }
            return flag;
        }

        [RedirectMethod]
        public override void PopulateGroupData(int groupX, int groupZ, int layer, ref int vertexIndex,
            ref int triangleIndex, Vector3 groupPosition, RenderGroup.MeshData data, ref Vector3 min, ref Vector3 max,
            ref float maxRenderDistance, ref float maxInstanceDistance, ref bool requireSurfaceMaps)
        {
            int num1 = groupX * 270 / 45;
            int num2 = groupZ * 270 / 45;
            int num3 = (groupX + 1) * 270 / 45 - 1;
            int num4 = (groupZ + 1) * 270 / 45 - 1;
            for (int index1 = num2; index1 <= num4; ++index1)
            {
                for (int index2 = num1; index2 <= num3; ++index2)
                {
                    ushort buildingID = this.m_buildingGrid[index1 * 270 + index2];
                    int num5 = 0;
                    while ((int) buildingID != 0)
                    {
                        //add null check
                        //begin mod
                        if (this.m_buildings.m_buffer[(int) buildingID].Info != null)
                        {
                            this.m_buildings.m_buffer[(int) buildingID].PopulateGroupData(buildingID, groupX, groupZ,
                                layer, ref vertexIndex, ref triangleIndex, groupPosition, data, ref min, ref max,
                                ref maxRenderDistance, ref maxInstanceDistance);
                        }
                        //end mod
                        buildingID = this.m_buildings.m_buffer[(int) buildingID].m_nextGridBuilding;
                        if (++num5 >= 49152)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core,
                                "Invalid list detected!\n" + System.Environment.StackTrace);
                            break;
                        }
                    }
                }
            }
        }

        [RedirectMethod]
        public ushort FindBuilding(Vector3 pos, float maxDistance, ItemClass.Service service, ItemClass.SubService subService, Building.Flags flagsRequired, Building.Flags flagsForbidden)
        {
            int num1 = Mathf.Max((int)(((double)pos.x - (double)maxDistance) / 64.0 + 135.0), 0);
            int num2 = Mathf.Max((int)(((double)pos.z - (double)maxDistance) / 64.0 + 135.0), 0);
            int num3 = Mathf.Min((int)(((double)pos.x + (double)maxDistance) / 64.0 + 135.0), 269);
            int num4 = Mathf.Min((int)(((double)pos.z + (double)maxDistance) / 64.0 + 135.0), 269);
            ushort num5 = 0;
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
                        //added null check
                        //begin mod
                        if (info!=null && (info.m_class.m_service == service || service == ItemClass.Service.None) && (info.m_class.m_subService == subService || subService == ItemClass.SubService.None) && (this.m_buildings.m_buffer[(int)num7].m_flags & (flagsRequired | flagsForbidden)) == flagsRequired)
                        {
                            //end mod
                            float num9 = Vector3.SqrMagnitude(pos - this.m_buildings.m_buffer[(int)num7].m_position);
                            if ((double)num9 < (double)num6)
                            {
                                num5 = num7;
                                num6 = num9;
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
using ColossalFramework;
using SaveOurSaves.Redirection;
using UnityEngine;

namespace SaveOurSaves.Detours
{
    [TargetType(typeof(NetManager))]
    public class NetManagerDetour : NetManager
    {
        [RedirectMethod]
        public override bool CalculateGroupData(int groupX, int groupZ, int layer, ref int vertexCount, ref int triangleCount, ref int objectCount, ref RenderGroup.VertexArrays vertexArrays)
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
                    ushort nodeID = this.m_nodeGrid[index1 * 270 + index2];
                    int num5 = 0;
                    while ((int)nodeID != 0)
                    {
                        //swallow exceptions
                        //begin mod
                        try
                        {
                            if (this.m_nodes.m_buffer[(int)nodeID].CalculateGroupData(nodeID, layer, ref vertexCount,
                                ref triangleCount, ref objectCount, ref vertexArrays))
                                flag = true;
                        }
                        catch
                        {
                            //swallow
                        }
                        //end mod
                        nodeID = this.m_nodes.m_buffer[(int)nodeID].m_nextGridNode;
                        if (++num5 >= 32768)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + System.Environment.StackTrace);
                            break;
                        }
                    }
                }
            }
            for (int index1 = num2; index1 <= num4; ++index1)
            {
                for (int index2 = num1; index2 <= num3; ++index2)
                {
                    ushort segmentID = this.m_segmentGrid[index1 * 270 + index2];
                    int num5 = 0;
                    while ((int)segmentID != 0)
                    {
                        //swallow exceptions
                        //begin mod
                        try
                        {
                            if (this.m_segments.m_buffer[(int)segmentID].CalculateGroupData(segmentID, layer, ref vertexCount, ref triangleCount, ref objectCount, ref vertexArrays))
                                flag = true;
                        }
                        catch
                        {
                            //swallow
                        }
                        //end mod
                        segmentID = this.m_segments.m_buffer[(int)segmentID].m_nextGridSegment;
                        if (++num5 >= 36864)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + System.Environment.StackTrace);
                            break;
                        }
                    }
                }
            }
            return flag;
        }

        [RedirectMethod]
        public override void PopulateGroupData(int groupX, int groupZ, int layer, ref int vertexIndex, ref int triangleIndex, Vector3 groupPosition, RenderGroup.MeshData data, ref Vector3 min, ref Vector3 max, ref float maxRenderDistance, ref float maxInstanceDistance, ref bool requireSurfaceMaps)
        {
            int num1 = groupX * 270 / 45;
            int num2 = groupZ * 270 / 45;
            int num3 = (groupX + 1) * 270 / 45 - 1;
            int num4 = (groupZ + 1) * 270 / 45 - 1;
            for (int index1 = num2; index1 <= num4; ++index1)
            {
                for (int index2 = num1; index2 <= num3; ++index2)
                {
                    ushort nodeID = this.m_nodeGrid[index1 * 270 + index2];
                    int num5 = 0;
                    while ((int)nodeID != 0)
                    {
                        //swallow exceptions
                        //begin mod
                        try
                        {
                            this.m_nodes.m_buffer[(int)nodeID].PopulateGroupData(nodeID, groupX, groupZ, layer, ref vertexIndex, ref triangleIndex, groupPosition, data, ref min, ref max, ref maxRenderDistance, ref maxInstanceDistance, ref requireSurfaceMaps);
                        }
                        catch
                        {
                            //swallow
                        }
                        //end mod
                        nodeID = this.m_nodes.m_buffer[(int)nodeID].m_nextGridNode;
                        if (++num5 >= 32768)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + System.Environment.StackTrace);
                            break;
                        }
                    }
                }
            }
            for (int index1 = num2; index1 <= num4; ++index1)
            {
                for (int index2 = num1; index2 <= num3; ++index2)
                {
                    ushort segmentID = this.m_segmentGrid[index1 * 270 + index2];
                    int num5 = 0;
                    while ((int)segmentID != 0)
                    {
                        //swallow exceptions
                        //begin mod
                        try
                        {
                            this.m_segments.m_buffer[(int)segmentID].PopulateGroupData(segmentID, groupX, groupZ, layer, ref vertexIndex, ref triangleIndex, groupPosition, data, ref min, ref max, ref maxRenderDistance, ref maxInstanceDistance, ref requireSurfaceMaps);
                        }
                        catch
                        {
                            //swallow
                        }
                        segmentID = this.m_segments.m_buffer[(int)segmentID].m_nextGridSegment;
                        if (++num5 >= 36864)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + System.Environment.StackTrace);
                            break;
                        }
                    }
                }
            }
        }
    }
}
using ColossalFramework;
using ColossalFramework.Math;
using SaveOurSaves.Redirection;
using UnityEngine;

namespace SaveOurSaves.Detours
{
    [TargetType(typeof(NetSegment))]
    public class NetSegmentDetour
    {
        [RedirectMethod]
        public static void CalculateCorner(NetInfo info, Vector3 startPos, Vector3 endPos, Vector3 startDir, Vector3 endDir, NetInfo extraInfo1, Vector3 extraEndPos1, Vector3 extraStartDir1, Vector3 extraEndDir1, NetInfo extraInfo2, Vector3 extraEndPos2, Vector3 extraStartDir2, Vector3 extraEndDir2, ushort ignoreSegmentID, ushort startNodeID, bool heightOffset, bool leftSide, out Vector3 cornerPos, out Vector3 cornerDirection, out bool smooth)
        {
            //add null check
            //begin mod
            if (info == null)
            {
                cornerPos = new Vector3();
                cornerDirection = new Vector3();
                smooth = false;
                return;
            }
            //end mod
            NetManager instance = Singleton<NetManager>.instance;
            Bezier3 bezier1 = new Bezier3();
            Bezier3 bezier2 = new Bezier3();
            NetNode.Flags flags = NetNode.Flags.End;
            ushort num1 = 0;
            if ((int)startNodeID != 0)
            {
                flags = instance.m_nodes.m_buffer[(int)startNodeID].m_flags;
                num1 = instance.m_nodes.m_buffer[(int)startNodeID].m_building;
            }
            cornerDirection = startDir;
            float num2 = !leftSide ? -info.m_halfWidth : info.m_halfWidth;
            smooth = (flags & NetNode.Flags.Middle) != NetNode.Flags.None;
            if (extraInfo1 != null)
                flags = (flags & NetNode.Flags.End) == NetNode.Flags.None || !info.IsCombatible(extraInfo1) || extraInfo2 != null ? flags & ~(NetNode.Flags.Middle | NetNode.Flags.Bend) | NetNode.Flags.Junction : ((double)startDir.x * (double)extraStartDir1.x + (double)startDir.z * (double)extraStartDir1.z >= -0.999000012874603 ? flags & ~NetNode.Flags.End | NetNode.Flags.Bend : flags & ~NetNode.Flags.End | NetNode.Flags.Middle);
            if ((flags & NetNode.Flags.Middle) != NetNode.Flags.None)
            {
                int num3 = extraInfo1 == null ? 0 : -1;
                int num4 = (int)startNodeID == 0 ? 0 : 8;
                for (int index = num3; index < num4; ++index)
                {
                    Vector3 vector3;
                    if (index == -1)
                    {
                        vector3 = extraStartDir1;
                    }
                    else
                    {
                        ushort segment = instance.m_nodes.m_buffer[(int)startNodeID].GetSegment(index);
                        if ((int)segment != 0 && (int)segment != (int)ignoreSegmentID)
                        {
                            ushort num5 = instance.m_segments.m_buffer[(int)segment].m_startNode;
                            vector3 = (int)startNodeID == (int)num5 ? instance.m_segments.m_buffer[(int)segment].m_startDirection : instance.m_segments.m_buffer[(int)segment].m_endDirection;
                        }
                        else
                            continue;
                    }
                    cornerDirection = VectorUtils.NormalizeXZ(cornerDirection - vector3);
                    break;
                }
            }
            Vector3 lhs = Vector3.Cross(cornerDirection, Vector3.up).normalized;
            if (info.m_twistSegmentEnds && (int)num1 != 0)
            {
                float f = Singleton<BuildingManager>.instance.m_buildings.m_buffer[(int)num1].m_angle;
                Vector3 rhs = new Vector3(Mathf.Cos(f), 0.0f, Mathf.Sin(f));
                lhs = (double)Vector3.Dot(lhs, rhs) < 0.0 ? -rhs : rhs;
            }
            bezier1.a = startPos + lhs * num2;
            bezier2.a = startPos - lhs * num2;
            cornerPos = bezier1.a;
            if ((flags & NetNode.Flags.Junction) != NetNode.Flags.None && info.m_clipSegmentEnds || (flags & (NetNode.Flags.Bend | NetNode.Flags.Outside)) != NetNode.Flags.None)
            {
                Vector3 vector3_1 = endDir;
                Vector3 normalized1 = Vector3.Cross(vector3_1, Vector3.up).normalized;
                bezier1.d = endPos - normalized1 * num2;
                bezier2.d = endPos + normalized1 * num2;
                NetSegment.CalculateMiddlePoints(bezier1.a, cornerDirection, bezier1.d, vector3_1, false, false, out bezier1.b, out bezier1.c);
                NetSegment.CalculateMiddlePoints(bezier2.a, cornerDirection, bezier2.d, vector3_1, false, false, out bezier2.b, out bezier2.c);
                Bezier2 bezier2_1 = Bezier2.XZ(bezier1);
                Bezier2 bezier2_2 = Bezier2.XZ(bezier2);
                float a1 = -1f;
                float num3 = -1f;
                bool flag = false;
                int num4 = extraInfo1 == null ? 0 : (extraInfo2 == null ? -1 : -2);
                int num5 = (int)startNodeID == 0 ? 0 : 8;
                float a2 = info.m_halfWidth * 0.5f;
                for (int index = num4; index < num5; ++index)
                {
                    NetInfo netInfo;
                    if (index == -2)
                    {
                        netInfo = extraInfo2;
                        Vector3 vector3_2 = extraEndPos2;
                        Vector3 vector3_3 = extraEndDir2;
                        if (vector3_2 == endPos && vector3_3 == endDir)
                            continue;
                    }
                    else if (index == -1)
                    {
                        netInfo = extraInfo1;
                        Vector3 vector3_2 = extraEndPos1;
                        Vector3 vector3_3 = extraEndDir1;
                        if (vector3_2 == endPos && vector3_3 == endDir)
                            continue;
                    }
                    else
                    {
                        ushort segment = instance.m_nodes.m_buffer[(int)startNodeID].GetSegment(index);
                        if ((int)segment != 0 && (int)segment != (int)ignoreSegmentID)
                            netInfo = instance.m_segments.m_buffer[(int)segment].Info;
                        else
                            continue;
                    }
                    if (netInfo != null)
                        a2 = Mathf.Max(a2, netInfo.m_halfWidth * 0.5f);
                }
                for (int index = num4; index < num5; ++index)
                {
                    NetInfo netInfo;
                    Vector3 vector3_2;
                    Vector3 vector3_3;
                    Vector3 vector3_4;
                    if (index == -2)
                    {
                        netInfo = extraInfo2;
                        vector3_2 = extraEndPos2;
                        vector3_3 = extraStartDir2;
                        vector3_4 = extraEndDir2;
                        if (vector3_2 == endPos && vector3_4 == endDir)
                            continue;
                    }
                    else if (index == -1)
                    {
                        netInfo = extraInfo1;
                        vector3_2 = extraEndPos1;
                        vector3_3 = extraStartDir1;
                        vector3_4 = extraEndDir1;
                        if (vector3_2 == endPos && vector3_4 == endDir)
                            continue;
                    }
                    else
                    {
                        ushort segment = instance.m_nodes.m_buffer[(int)startNodeID].GetSegment(index);
                        if ((int)segment != 0 && (int)segment != (int)ignoreSegmentID)
                        {
                            ushort num6 = instance.m_segments.m_buffer[(int)segment].m_startNode;
                            ushort num7 = instance.m_segments.m_buffer[(int)segment].m_endNode;
                            vector3_3 = instance.m_segments.m_buffer[(int)segment].m_startDirection;
                            vector3_4 = instance.m_segments.m_buffer[(int)segment].m_endDirection;
                            if ((int)startNodeID != (int)num6)
                            {
                                num7 = num6;
                                Vector3 vector3_5 = vector3_3;
                                vector3_3 = vector3_4;
                                vector3_4 = vector3_5;
                            }
                            netInfo = instance.m_segments.m_buffer[(int)segment].Info;
                            vector3_2 = instance.m_nodes.m_buffer[(int)num7].m_position;
                        }
                        else
                            continue;
                    }
                    if (netInfo != null)
                    {
                        if ((double)vector3_3.z * (double)cornerDirection.x - (double)vector3_3.x * (double)cornerDirection.z > 0.0 == leftSide)
                        {
                            Bezier3 bezier3 = new Bezier3();
                            float num6 = Mathf.Max(a2, netInfo.m_halfWidth);
                            if (!leftSide)
                                num6 = -num6;
                            Vector3 normalized2 = Vector3.Cross(vector3_3, Vector3.up).normalized;
                            bezier3.a = startPos - normalized2 * num6;
                            Vector3 normalized3 = Vector3.Cross(vector3_4, Vector3.up).normalized;
                            bezier3.d = vector3_2 + normalized3 * num6;
                            NetSegment.CalculateMiddlePoints(bezier3.a, vector3_3, bezier3.d, vector3_4, false, false, out bezier3.b, out bezier3.c);
                            Bezier2 b2 = Bezier2.XZ(bezier3);
                            float t1;
                            float t2;
                            if (bezier2_1.Intersect(b2, out t1, out t2, 6))
                                a1 = Mathf.Max(a1, t1);
                            else if (bezier2_1.Intersect(b2.a, b2.a - VectorUtils.XZ(vector3_3) * 16f, out t1, out t2, 6))
                                a1 = Mathf.Max(a1, t1);
                            else if (b2.Intersect(bezier2_1.d + (bezier2_1.d - bezier2_2.d) * 0.01f, bezier2_2.d, out t1, out t2, 6))
                                a1 = Mathf.Max(a1, 1f);
                            if ((double)cornerDirection.x * (double)vector3_3.x + (double)cornerDirection.z * (double)vector3_3.z >= -0.75)
                                flag = true;
                        }
                        else
                        {
                            Bezier3 bezier3 = new Bezier3();
                            float num6 = (float)((double)cornerDirection.x * (double)vector3_3.x + (double)cornerDirection.z * (double)vector3_3.z);
                            if ((double)num6 >= 0.0)
                            {
                                vector3_3.x -= (float)((double)cornerDirection.x * (double)num6 * 2.0);
                                vector3_3.z -= (float)((double)cornerDirection.z * (double)num6 * 2.0);
                            }
                            float num7 = Mathf.Max(a2, netInfo.m_halfWidth);
                            if (!leftSide)
                                num7 = -num7;
                            Vector3 normalized2 = Vector3.Cross(vector3_3, Vector3.up).normalized;
                            bezier3.a = startPos + normalized2 * num7;
                            Vector3 normalized3 = Vector3.Cross(vector3_4, Vector3.up).normalized;
                            bezier3.d = vector3_2 - normalized3 * num7;
                            NetSegment.CalculateMiddlePoints(bezier3.a, vector3_3, bezier3.d, vector3_4, false, false, out bezier3.b, out bezier3.c);
                            Bezier2 b2 = Bezier2.XZ(bezier3);
                            float t1;
                            float t2;
                            if (bezier2_2.Intersect(b2, out t1, out t2, 6))
                                num3 = Mathf.Max(num3, t1);
                            else if (bezier2_2.Intersect(b2.a, b2.a - VectorUtils.XZ(vector3_3) * 16f, out t1, out t2, 6))
                                num3 = Mathf.Max(num3, t1);
                            else if (b2.Intersect(bezier2_1.d, bezier2_2.d + (bezier2_2.d - bezier2_1.d) * 0.01f, out t1, out t2, 6))
                                num3 = Mathf.Max(num3, 1f);
                        }
                    }
                }
                if ((flags & NetNode.Flags.Junction) != NetNode.Flags.None)
                {
                    if (!flag)
                        a1 = Mathf.Max(a1, num3);
                }
                else if ((flags & NetNode.Flags.Bend) != NetNode.Flags.None && !flag)
                    a1 = Mathf.Max(a1, num3);
                float num8;
                if ((flags & NetNode.Flags.Outside) != NetNode.Flags.None)
                {
                    float num6 = 8640f;
                    Vector2 vector2_1 = new Vector2(-num6, -num6);
                    Vector2 vector2_2 = new Vector2(-num6, num6);
                    Vector2 vector2_3 = new Vector2(num6, num6);
                    Vector2 vector2_4 = new Vector2(num6, -num6);
                    float t1;
                    float t2;
                    if (bezier2_1.Intersect(vector2_1, vector2_2, out t1, out t2, 6))
                        a1 = Mathf.Max(a1, t1);
                    if (bezier2_1.Intersect(vector2_2, vector2_3, out t1, out t2, 6))
                        a1 = Mathf.Max(a1, t1);
                    if (bezier2_1.Intersect(vector2_3, vector2_4, out t1, out t2, 6))
                        a1 = Mathf.Max(a1, t1);
                    if (bezier2_1.Intersect(vector2_4, vector2_1, out t1, out t2, 6))
                        a1 = Mathf.Max(a1, t1);
                    num8 = Mathf.Clamp01(a1);
                }
                else
                {
                    if ((double)a1 < 0.0)
                        a1 = (double)info.m_halfWidth >= 4.0 ? bezier2_1.Travel(0.0f, 8f) : 0.0f;
                    float num6 = Mathf.Clamp01(a1);
                    float num7 = VectorUtils.LengthXZ(bezier1.Position(num6) - bezier1.a);
                    num8 = bezier2_1.Travel(num6, Mathf.Max(info.m_minCornerOffset - num7, 2f));
                    if (info.m_straightSegmentEnds)
                    {
                        if ((double)num3 < 0.0)
                            num3 = (double)info.m_halfWidth >= 4.0 ? bezier2_2.Travel(0.0f, 8f) : 0.0f;
                        float num9 = Mathf.Clamp01(num3);
                        float num10 = VectorUtils.LengthXZ(bezier2.Position(num9) - bezier2.a);
                        float b = bezier2_2.Travel(num9, Mathf.Max(info.m_minCornerOffset - num10, 2f));
                        num8 = Mathf.Max(num8, b);
                    }
                }
                float num11 = cornerDirection.y;
                cornerDirection = bezier1.Tangent(num8);
                cornerDirection.y = 0.0f;
                cornerDirection.Normalize();
                if (!info.m_flatJunctions)
                    cornerDirection.y = num11;
                cornerPos = bezier1.Position(num8);
                cornerPos.y = startPos.y;
            }
            if (!heightOffset || (int)startNodeID == 0)
                return;
            cornerPos.y += (float)instance.m_nodes.m_buffer[(int)startNodeID].m_heightOffset * (1f / 64f);
        }
    }
}
using ColossalFramework.Math;
using SaveOurSaves.Redirection;
using UnityEngine;

namespace SaveOurSaves.Detours
{
    [TargetType(typeof(PropInstance))]
    public struct PropInstanceDetour
    {
        [RedirectMethod]
        public static bool CalculateGroupData(PropInfo info, int layer, ref int vertexCount, ref int triangleCount, ref int objectCount, ref RenderGroup.VertexArrays vertexArrays)
        {
            //added null check
            //begin mod
            if (info == null)
            {
                return false;
            }
            //end mod
            if (info.m_prefabDataLayer == layer)
                return true;
            if (info.m_effectLayer != layer)
                return false;
            bool flag = false;
            for (int index = 0; index < info.m_effects.Length; ++index)
            {

                if (info.m_effects[index].m_effect.CalculateGroupData(layer, ref vertexCount, ref triangleCount, ref objectCount, ref vertexArrays))
                    flag = true;
            }
            return flag;
        }

        [RedirectMethod]
        public static void PopulateGroupData(ref PropInstance prop, ushort propID, int layer, ref int vertexIndex, ref int triangleIndex, Vector3 groupPosition, RenderGroup.MeshData data, ref Vector3 min, ref Vector3 max, ref float maxRenderDistance, ref float maxInstanceDistance)
        {
            if (prop.Blocked)
                return;
            PropInfo info = prop.Info;
            //add null check
            //begin mod
            if (info == null)
            {
                return;
            }
            //end mod
            Vector3 position = prop.Position;
            Randomizer r = new Randomizer((int)propID);
            float scale = info.m_minScale + (float)((double)r.Int32(10000U) * ((double)info.m_maxScale - (double)info.m_minScale) * 9.99999974737875E-05);
            float angle = prop.Angle;
            Color color = info.GetColor(ref r);
            PropInstance.PopulateGroupData(info, layer, new InstanceID()
            {
                Prop = propID
            }, position, scale, angle, color, ref vertexIndex, ref triangleIndex, groupPosition, data, ref min, ref max, ref maxRenderDistance, ref maxInstanceDistance);
        }

        [RedirectMethod]
        public static void PopulateGroupData(PropInfo info, int layer, InstanceID id, Vector3 position, float scale, float angle, Color color, ref int vertexIndex, ref int triangleIndex, Vector3 groupPosition, RenderGroup.MeshData data, ref Vector3 min, ref Vector3 max, ref float maxRenderDistance, ref float maxInstanceDistance)
        {
            //add null check
            //begin mod
            if (info == null)
            {
                return;
            }
            //end mod
            if (info.m_prefabDataLayer == layer)
            {
                float y = info.m_generatedInfo.m_size.y * scale;
                float num = (float)((double)Mathf.Max(info.m_generatedInfo.m_size.x, info.m_generatedInfo.m_size.z) * (double)scale * 0.5);
                min = Vector3.Min(min, position - new Vector3(num, 0.0f, num));
                max = Vector3.Max(max, position + new Vector3(num, y, num));
                maxRenderDistance = Mathf.Max(maxRenderDistance, info.m_maxRenderDistance);
                maxInstanceDistance = Mathf.Max(maxInstanceDistance, info.m_maxRenderDistance);
            }
            else
            {
                if (info.m_effectLayer != layer)
                    return;
                Matrix4x4 matrix4x4 = new Matrix4x4();
                matrix4x4.SetTRS(position, Quaternion.AngleAxis(angle * 57.29578f, Vector3.down), new Vector3(scale, scale, scale));
                for (int index = 0; index < info.m_effects.Length; ++index)
                {
                    Vector3 pos = matrix4x4.MultiplyPoint(info.m_effects[index].m_position);
                    Vector3 dir = matrix4x4.MultiplyVector(info.m_effects[index].m_direction);
                    info.m_effects[index].m_effect.PopulateGroupData(layer, id, pos, dir, ref vertexIndex, ref triangleIndex, groupPosition, data, ref min, ref max, ref maxRenderDistance, ref maxInstanceDistance);
                }
            }
        }
    }
}
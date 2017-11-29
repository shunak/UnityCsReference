// Unity C# reference source
// Copyright (c) Unity Technologies. For terms of use, see
// https://unity3d.com/legal/licenses/Unity_Reference_Only_License

using System;
using UnityEngine;
using UnityEngine.Bindings;
using UnityEngine.Collections;
using UnityEngine.Experimental.Graphics;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine.Jobs;
using UnityEngine.Scripting;
using Unity.Collections.LowLevel.Unsafe;

namespace UnityEngine.Experimental.U2D
{
    [NativeHeader("Runtime/2D/Common/SpriteDataAccess.h")]
    [RequiredByNativeCode]
    [NativeType(CodegenOptions.Custom, "ScriptingSpriteBone")]
    [StructLayout(LayoutKind.Sequential)]
    [Serializable]
    public struct SpriteBone
    {
        [NativeNameAttribute("name")]
        string m_Name;
        [NativeNameAttribute("position")]
        Vector3 m_Position;
        [NativeNameAttribute("rotation")]
        Quaternion m_Rotation;
        [NativeNameAttribute("length")]
        float m_Length;
        [NativeNameAttribute("parentId")]
        int m_ParentId;

        public string name { get { return m_Name; } set { m_Name = value; } }
        public Vector3 position { get { return m_Position; } set { m_Position = value; } }
        public Quaternion rotation { get { return m_Rotation; } set { m_Rotation = value; } }
        public float length { get { return m_Length; } set { m_Length = value; } }
        public int parentId { get { return m_ParentId; } set { m_ParentId = value; } }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct SpriteChannelInfo
    {
        [NativeNameAttribute("buffer")]
        IntPtr m_Buffer;
        [NativeNameAttribute("count")]
        int m_Count;
        [NativeNameAttribute("offset")]
        int m_Offset;
        [NativeNameAttribute("stride")]
        int m_Stride;

        public IntPtr buffer { get { return m_Buffer; } set { m_Buffer = value; } }
        public int count { get { return m_Count; } set { m_Count = value; } }
        public int offset { get { return m_Offset; } set { m_Offset = value; } }
        public int stride { get { return m_Stride; } set { m_Stride = value; } }
    }

    [NativeHeader("Runtime/Graphics/SpriteFrame.h")]
    [NativeHeader("Runtime/2D/Common/SpriteDataAccess.h")]
    public static class SpriteDataAccessExtensions
    {
        private static void CheckAttributeTypeMatchesAndThrow<T>(VertexAttribute channel)
        {
            var channelTypeMatches = false;
            switch (channel)
            {
                case VertexAttribute.Position:
                case VertexAttribute.Normal:
                    channelTypeMatches = typeof(T) == typeof(Vector3); break;
                case VertexAttribute.Color:
                    channelTypeMatches = typeof(T) == typeof(Color32); break;
                case VertexAttribute.TexCoord0:
                case VertexAttribute.TexCoord1:
                case VertexAttribute.TexCoord2:
                case VertexAttribute.TexCoord3:
                    channelTypeMatches = typeof(T) == typeof(Vector2); break;
                case VertexAttribute.Tangent:
                    channelTypeMatches = typeof(T) == typeof(Vector4); break;
                default:
                    throw new InvalidOperationException(String.Format("The requested channel '{0}' is unknown.", channel));
            }

            if (!channelTypeMatches)
                throw new InvalidOperationException(String.Format("The requested channel '{0}' does not match the return type {1}.", channel, typeof(T).Name));
        }

        public static NativeSlice<T> GetVertexAttribute<T>(this Sprite sprite, VertexAttribute channel) where T : struct
        {
            CheckAttributeTypeMatchesAndThrow<T>(channel);
            var info = GetChannelInfo(sprite, channel);
            return NativeSliceUnsafeUtility.ConvertExistingDataToNativeSlice<T>(info.buffer, info.offset, info.count, info.stride, sprite.GetSafetyHandle());
        }

        public static void SetVertexAttribute<T>(this Sprite sprite, VertexAttribute channel, NativeArray<T> src) where T : struct
        {
            CheckAttributeTypeMatchesAndThrow<T>(channel);
            SetChannelData(sprite, channel, src.UnsafeReadOnlyPtr);
        }

        public static NativeArray<Matrix4x4> GetBindPoses(this Sprite sprite)
        {
            var info = GetBindPoseInfo(sprite);
            return NativeArray<Matrix4x4>.ConvertExistingDataToNativeArrayInternal(info.buffer, info.count, sprite.GetSafetyHandle(), Allocator.Invalid);
        }

        public static void SetBindPoses(this Sprite sprite, NativeArray<Matrix4x4> src)
        {
            SetBindPoseData(sprite, src.UnsafeReadOnlyPtr, src.Length);
        }

        public static NativeArray<ushort> GetIndices(this Sprite sprite)
        {
            var info = GetIndicesInfo(sprite);
            return NativeArray<ushort>.ConvertExistingDataToNativeArrayInternal(info.buffer, info.count, sprite.GetSafetyHandle(), Allocator.Invalid);
        }

        public static void SetIndices(this Sprite sprite, NativeArray<ushort> src)
        {
            SetIndicesData(sprite, src.UnsafeReadOnlyPtr, src.Length);
        }

        public static NativeArray<BoneWeight> GetBoneWeights(this Sprite sprite)
        {
            var info = GetBoneWeightsInfo(sprite);
            return NativeArray<BoneWeight>.ConvertExistingDataToNativeArrayInternal(info.buffer, info.count, sprite.GetSafetyHandle(), Allocator.Invalid);
        }

        public static void SetBoneWeights(this Sprite sprite, NativeArray<BoneWeight> src)
        {
            SetBoneWeightsData(sprite, src.UnsafeReadOnlyPtr, src.Length);
        }

        public static SpriteBone[] GetBones(this Sprite sprite)
        {
            return GetBoneInfo(sprite);
        }

        public static void SetBones(this Sprite sprite, SpriteBone[] src)
        {
            SetBoneData(sprite, src);
        }

        // The only way to change the vertex count
        extern public static void SetVertexCount(this Sprite sprite, int count);
        extern public static int GetVertexCount(this Sprite sprite);

        // This lenght is not tied to vertexCount
        extern private static SpriteChannelInfo GetBindPoseInfo(Sprite sprite);
        extern private static void SetBindPoseData(Sprite sprite, IntPtr src, int count);

        extern private static SpriteChannelInfo GetIndicesInfo(Sprite sprite);
        extern private static void SetIndicesData(Sprite sprite, IntPtr src, int count);

        extern private static SpriteChannelInfo GetChannelInfo(Sprite sprite, VertexAttribute channel);
        extern private static void SetChannelData(Sprite sprite, VertexAttribute channel, IntPtr src);

        extern private static SpriteBone[] GetBoneInfo(Sprite sprite);
        extern private static void SetBoneData(Sprite sprite, SpriteBone[] src);

        extern private static SpriteChannelInfo GetBoneWeightsInfo(Sprite sprite);
        extern private static void SetBoneWeightsData(Sprite sprite, IntPtr src, int count);

        extern private static AtomicSafetyHandle GetSafetyHandle(this Sprite sprite);
    }

    [NativeHeader("Runtime/2D/Common/SpriteDataAccess.h")]
    [NativeHeader("Runtime/Graphics/Mesh/SpriteRenderer.h")]
    public static class SpriteRendererDataAccessExtensions
    {
        public static NativeArray<Vector3> GetDeformableVertices(this SpriteRenderer spriteRenderer)
        {
            var info = GetDeformableChannelInfo(spriteRenderer, VertexAttribute.Position);
            return NativeArray<Vector3>.ConvertExistingDataToNativeArrayInternal(info.buffer, info.count, spriteRenderer.GetSafetyHandle(), Allocator.Invalid);
        }

        extern public static void DeactivateDeformableBuffer(this SpriteRenderer renderer);

        // This method will be enabled when JobSystem Properly lands... The tests will follow as well
        // It is just a simple comment out because nothing else needs to be disabled.
        // extern public static void UpdateDeformableBuffer(this SpriteRenderer spriteRenderer, JobHandle fence);

        extern private static SpriteChannelInfo GetDeformableChannelInfo(this SpriteRenderer sprite, VertexAttribute channel);

        extern private static AtomicSafetyHandle GetSafetyHandle(this SpriteRenderer spriteRenderer);
    }
}
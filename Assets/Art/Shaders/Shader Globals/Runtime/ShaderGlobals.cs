using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ShaderGlobals
{
    enum ShaderGlobalType
    {
        Float,
        Integer,
        Color,
        Vector,
        Matrix,
        Texture,
    }

    [Serializable]
    abstract class BaseShaderGlobal
    {
        [SerializeField]
        internal string m_Name;

        internal abstract ShaderGlobalType Type { get; }
        internal abstract object ObjectValue { get; }

        public abstract void Apply();
        public abstract void Reset();
        internal abstract BaseShaderGlobal Copy();

        internal static BaseShaderGlobal Create(ShaderGlobalType type, string name, object valueHint = null)
        {
            return type switch
            {
                ShaderGlobalType.Float => new FloatShaderGlobal(name, valueHint),
                ShaderGlobalType.Integer => new IntegerShaderGlobal(name, valueHint),
                ShaderGlobalType.Color => new ColorShaderGlobal(name, valueHint),
                ShaderGlobalType.Vector => new VectorShaderGlobal(name, valueHint),
                ShaderGlobalType.Matrix => new MatrixShaderGlobal(name, valueHint),
                ShaderGlobalType.Texture => new TextureShaderGlobal(name, valueHint),
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }
    }

    [Serializable]
    abstract class ShaderGlobal<T> : BaseShaderGlobal
    {
        [SerializeField]
        internal T m_Value;

        internal override object ObjectValue => m_Value;

        protected ShaderGlobal(string name, object valueHint = null)
        {
            m_Name = name;

            if (valueHint != null)
            {
                try
                {
                    m_Value = (T) Convert.ChangeType(valueHint, typeof(T));
                }
                catch (Exception)
                {
                    m_Value = default;
                }
            }
        }
    }

    sealed class FloatShaderGlobal : ShaderGlobal<float>
    {
        internal override ShaderGlobalType Type => ShaderGlobalType.Float;

        public FloatShaderGlobal(string name, object valueHint = null)
            : base(name, valueHint) { }

        public override void Apply() => Shader.SetGlobalFloat(m_Name, m_Value);
        public override void Reset() => Shader.SetGlobalFloat(m_Name, default);
        internal override BaseShaderGlobal Copy() => new FloatShaderGlobal(m_Name, m_Value);
    }

    sealed class IntegerShaderGlobal : ShaderGlobal<int>
    {
        internal override ShaderGlobalType Type => ShaderGlobalType.Integer;

        public IntegerShaderGlobal(string name, object valueHint = null)
            : base(name, valueHint) { }

        public override void Apply() => Shader.SetGlobalInteger(m_Name, m_Value);
        public override void Reset() => Shader.SetGlobalInteger(m_Name, default);
        internal override BaseShaderGlobal Copy() => new IntegerShaderGlobal(m_Name, m_Value);
    }

    sealed class ColorShaderGlobal : ShaderGlobal<Color>
    {
        internal override ShaderGlobalType Type => ShaderGlobalType.Color;

        public ColorShaderGlobal(string name, object valueHint = null)
            : base(name, valueHint) { }

        public override void Apply() => Shader.SetGlobalColor(m_Name, m_Value);
        public override void Reset() => Shader.SetGlobalColor(m_Name, default);
        internal override BaseShaderGlobal Copy() => new ColorShaderGlobal(m_Name, m_Value);
    }

    sealed class VectorShaderGlobal : ShaderGlobal<Vector4>
    {
        internal override ShaderGlobalType Type => ShaderGlobalType.Vector;

        public VectorShaderGlobal(string name, object valueHint = null)
            : base(name, valueHint) { }

        public override void Apply() => Shader.SetGlobalVector(m_Name, m_Value);
        public override void Reset() => Shader.SetGlobalVector(m_Name, default);
        internal override BaseShaderGlobal Copy() => new VectorShaderGlobal(m_Name, m_Value);
    }

    sealed class MatrixShaderGlobal : ShaderGlobal<Matrix4x4>
    {
        internal override ShaderGlobalType Type => ShaderGlobalType.Matrix;

        public MatrixShaderGlobal(string name, object valueHint = null)
            : base(name, valueHint) { }

        public override void Apply() => Shader.SetGlobalMatrix(m_Name, m_Value);
        public override void Reset() => Shader.SetGlobalMatrix(m_Name, default);
        internal override BaseShaderGlobal Copy() => new MatrixShaderGlobal(m_Name, m_Value);
    }

    sealed class TextureShaderGlobal : ShaderGlobal<Texture>
    {
        internal override ShaderGlobalType Type => ShaderGlobalType.Texture;

        public TextureShaderGlobal(string name, object valueHint = null)
            : base(name, valueHint) { }

        public override void Apply() => Shader.SetGlobalTexture(m_Name, m_Value);
        public override void Reset() => Shader.SetGlobalTexture(m_Name, default);
        internal override BaseShaderGlobal Copy() => new TextureShaderGlobal(m_Name, m_Value);
    }

#if UNITY_EDITOR
    [CreateAssetMenu(fileName = "Shader Globals", menuName = "Shader Globals", order = 300)]
#endif
    sealed class ShaderGlobals : ScriptableObject
    {
        [NonSerialized]
        static List<BaseShaderGlobal> s_LastAppliedGlobals;

        [SerializeReference]
        public List<BaseShaderGlobal> shaderGlobals = new();

        void Apply()
        {
            foreach (var global in shaderGlobals)
            {
                global.Apply();
            }
        }

        internal void ClearValues()
        {
            foreach (var global in shaderGlobals)
            {
                global.Reset();
            }

            shaderGlobals.Clear();
        }

        void Reset()
        {
            SetGlobals();
        }

        void OnValidate()
        {
            shaderGlobals.RemoveAll(s => s == null);
            SetGlobals();
        }

#if UNITY_EDITOR
        [InitializeOnLoadMethod, InitializeOnEnterPlayMode]
#endif
        [RuntimeInitializeOnLoadMethod]
        static void SetGlobals()
        {
            if (s_LastAppliedGlobals is {Count: > 0})
            {
                foreach (var s in s_LastAppliedGlobals)
                {
                    s.Reset();
                }
            }

            s_LastAppliedGlobals ??= new List<BaseShaderGlobal>();
            s_LastAppliedGlobals.Clear();

            foreach (var globals in Resources.LoadAll<ShaderGlobals>(""))
            {
                globals.Apply();
                foreach (var global in globals.shaderGlobals)
                {
                    s_LastAppliedGlobals.Add(global.Copy());
                }
            }
        }
    }
}

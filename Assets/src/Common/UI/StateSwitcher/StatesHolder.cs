using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Common.UI.StateSwitcher
{
    public abstract class StatesHolder
    {
        public abstract void Apply(string stateName);
        public abstract bool IsActive();
        public abstract GameObject TargetGameObject { get; }
    }

    public abstract class StatesHolder<TTarget, TValue, TState> : StatesHolder where TState : AbstractState<TTarget, TValue>
    {
        public TTarget Target => _target;
        public IReadOnlyCollection<TState> States => _states;

        [SerializeField] private TTarget _target;
        [SerializeField] private TState[] _states;

        public override void Apply(string stateName)
        {
            TState state = null;
            for (var i = 0; i < _states.Length; i++)
            {
                if (_states[i].Name.Equals(stateName))
                {
                    state = _states[i];
                    break;
                }
            }

            if (state == null)
            {
#if UNITY_EDITOR
                Debug.LogWarningFormat("State not found {0} in {1}", stateName, GetType());
#endif
                return;
            }
            state.Apply(_target);
        }

        public TValue GetValue(string stateName)
        {
            var state = _states.FirstOrDefault(x => x.Name.Equals(stateName));
            if (state == null)
            {
                throw new ArgumentOutOfRangeException(stateName);
            }

            return state.Value;
        }

        public void SetValue(string stateName, TValue value)
        {
            var state = _states.FirstOrDefault(x => x.Name.Equals(stateName));
            if (state == null)
            {
                throw new ArgumentOutOfRangeException(stateName);
            }

            state.Value = value;
        }
    }

    public abstract class ComponentStatesHolder<TTarget, TValue, TState> : StatesHolder<TTarget, TValue, TState>
        where TTarget : Component
        where TState : AbstractState<TTarget, TValue>
    {
        public override bool IsActive()
        {
            return Target.gameObject.activeInHierarchy;
        }

        public override GameObject TargetGameObject => Target != null ? Target.gameObject : null;
    }

    /// <summary>
    /// Need for UnityEdtitor
    /// </summary>
    #region States Holders
    [Serializable] public class ColorStatesHolder : ComponentStatesHolder<Graphic, Color, ColorState> { }
    [Serializable] public class ColorSRStatesHolder : ComponentStatesHolder<SpriteRenderer, Color, ColorSRState> { }
    [Serializable] public class AlphaStatesHolder : ComponentStatesHolder<CanvasGroup, float, AlphaState> { }
    [Serializable] public class ScaleStateHolder : ComponentStatesHolder<RectTransform, Vector3, ScaleState> { }   
    [Serializable] public class TransformScaleStateHolder : ComponentStatesHolder<Transform, Vector3, TransformScaleState> { }
    [Serializable] public class MaterialParamStatesHolder : ComponentStatesHolder<Graphic, MaterialContainer, ShaderValueState> { }
    [Serializable] public class MaterialComponentStatesHolder : ComponentStatesHolder<Graphic, Material, MaterialState> { }
    [Serializable] public class FontStatesHolder : ComponentStatesHolder<Text, Font, FontState> { }
    [Serializable] public class ButtonInteractableStatesHolder : ComponentStatesHolder<Button, bool, ButtonInteractableState> { }
    [Serializable] public class FontStyleStatesHolder : ComponentStatesHolder<Text, FontStyle, FontStyleState> { }
    [Serializable] public class TextStatesHolder : ComponentStatesHolder<Text, string, TextState> { }
    [Serializable] public class TextFontDataStatesHolder : ComponentStatesHolder<Text, TextFontDataState.TextFontDataSettings, TextFontDataState> { }
    [Serializable] public class RotateStatesHolder : ComponentStatesHolder<Transform, Vector3, RotateState> { }
    [Serializable] public class RotationEulerAnglesStatesHolder : ComponentStatesHolder<Transform, Vector3, RotationEulerAnglesState> { }
    [Serializable] public class LayoutStatesHolder : ComponentStatesHolder<LayoutGroup, LayoutState.LayoutSettings, LayoutState> { }
    [Serializable] public class ParticleColorStatesHolder : ComponentStatesHolder<ParticleSystem, ParticleColors, ParticleColorState> { }
    [Serializable] public class RectTransformAnchorsStatesHolder : ComponentStatesHolder<UnityEngine.RectTransform, RectTransformAnchors, RectTransformAnchorsState> { }
    [Serializable] public class RectTransformSizeStates : ComponentStatesHolder<UnityEngine.RectTransform, RectTransformSize, RectTransformSizeDeltaState> { }
    [Serializable] public class TextAlignmentStatesHolder : ComponentStatesHolder<Text, TextAnchor, TextAlignmentState> { }
    [Serializable] public class ImageStateHolder : ComponentStatesHolder<Image, Sprite, ImageState> {}
    [Serializable]
    public class SpriteStateHolder : ComponentStatesHolder<SpriteRenderer, Sprite, SpriteState>
    {
        public override bool IsActive()
        {
            return true;
        }
    }

    [Serializable]
    public class EnableBehaviourStateHolder : ComponentStatesHolder<Behaviour, bool, EnableBehaviourState>
    {
        public override bool IsActive()
        {
            return true;
        }
    }
    
    [Serializable]
    public class LocalTransformPositionStatesHolder : ComponentStatesHolder<Transform, Vector3, LocalTransformPositionState>
    {
        public override bool IsActive()
        {
            return true;
        }
    }
    
    [Serializable]
    public class EnableStatesHolder : StatesHolder<GameObject, bool, EnableState>
    {
        public override bool IsActive()
        {
            return true;
        }
        
        public override GameObject TargetGameObject => Target != null? Target.gameObject : null;
    }
    
    #endregion

    [Serializable]
    public class ParticleColors
    {
        public Color color1 = Color.white;
        public Color color2 = Color.white;
    }
    
    [Serializable]
    public class RectTransformAnchors
    {
        public Vector2 anchoredPosition = Vector2.zero;
        public Vector2 anchorMin = Vector2.zero;
        public Vector2 anchorMax = Vector2.zero;
        public Vector2 pivot = Vector2.zero;
    }
    
    [Serializable]
    public class RectTransformSize
    {
        public enum StretchType
        {
            NoStretch,
            AutoStretch,
            AlwaysStretch 
        } 
        
        public StretchType stretchType = StretchType.AutoStretch;
        public Vector2 size;
        [Header("Only if stretch! Visible only in play mode!")]
        public float left;
        public float right;
        public float bottom;
        public float top;
    }
}
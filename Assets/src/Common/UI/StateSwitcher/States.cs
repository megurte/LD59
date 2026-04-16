using System;
using UnityEngine;
using UnityEngine.UI;

namespace Common.UI.StateSwitcher
{
    public interface IApplicableState<in TTarget>
    {
        void Apply(TTarget target);
    }

    [Serializable]
    public abstract class AbstractState<TTarget, TValue> : IApplicableState<TTarget>
    {
        public string Name
        {
            get { return _name; }
        }

        public TValue Value
        {
            get => _value;
            set => _value = value;
        }

        [SerializeField]
        private string _name;
        [SerializeField]
        private TValue _value;

        public abstract void Apply(TTarget target);
    }

    [Serializable]
    public class ColorState : AbstractState<Graphic, Color>
    {
        public override void Apply(Graphic target)
        {
            target.color = Value;
        }
    }
    
    [Serializable]
    public class ColorSRState : AbstractState<SpriteRenderer, Color>
    {
        public override void Apply(SpriteRenderer target)
        {
            target.color = Value;
        }
    }
    
    [Serializable]
    public class ParticleColorState : AbstractState<ParticleSystem, ParticleColors>
    {
        public override void Apply(ParticleSystem target)
        {
            if (target == null)
            {
                Debug.LogError("target(ParticleSystem) is null");
                return;
            }

            var main = target.main;
            main.startColor = new ParticleSystem.MinMaxGradient(Value.color1, Value.color2);
        }
    }

    [Serializable]
    public class SpriteState : AbstractState<SpriteRenderer, Sprite>
    {
        public override void Apply(SpriteRenderer target)
        {
            target.sprite = Value;
        }
    }
    
    [Serializable]
    public class ImageState : AbstractState<Image, Sprite>
    {
        public override void Apply(Image target)
        {
            target.sprite = Value;
        }
    }
    
    [Serializable]
    public class EnableBehaviourState : AbstractState<Behaviour, bool>
    {
        public override void Apply(Behaviour target)
        {
            target.enabled = Value;
        }
    }
        
    [Serializable]
    public class EnableState : AbstractState<GameObject, bool>
    {
        public override void Apply(GameObject target)
        {
            target.SetActive(Value);
        }
    }

    [Serializable]
    public class AlphaState : AbstractState<CanvasGroup, float>
    {
        public override void Apply(CanvasGroup target)
        {
            target.alpha = Value;
        }
    }

    [Serializable]
    public class LocalTransformPositionState : AbstractState<Transform, Vector3>
    {
        public override void Apply(Transform target)
        {
            target.localPosition = Value;
        }
    }
    
    [Serializable]
    public class ButtonInteractableState : AbstractState<UnityEngine.UI.Button, bool>
    {
        public override void Apply(UnityEngine.UI.Button target)
        {
            target.interactable = Value;
        }
    }
    
    [Serializable]
    public class RectTransformAnchorsState : AbstractState<UnityEngine.RectTransform, RectTransformAnchors>
    {
        public override void Apply(UnityEngine.RectTransform target)
        {
            target.anchorMin = Value.anchorMin;
            target.anchorMax = Value.anchorMax;
            target.anchoredPosition = Value.anchoredPosition;
            target.pivot = Value.pivot;
        }
    }
    
    [Serializable]
    public class RectTransformSizeDeltaState : AbstractState<UnityEngine.RectTransform, RectTransformSize>
    {
        public override void Apply(UnityEngine.RectTransform target)
        {
            if (Value.stretchType == RectTransformSize.StretchType.AlwaysStretch)
            {
                target.offsetMin = new Vector2(Value.left, Value.bottom);
                target.offsetMax = new Vector2(Value.right, Value.top);
                return;
            }

            var size = Value.size;
            target.sizeDelta = size;

            if (Math.Abs(target.anchorMin.x - target.anchorMax.x) > float.Epsilon
                && Value.stretchType == RectTransformSize.StretchType.AutoStretch)
            {
                target.offsetMin = new Vector2(Value.left, target.offsetMin.y);
                target.offsetMax = new Vector2(Value.right, target.offsetMax.y);
            }
            
            if (Math.Abs(target.anchorMin.y - target.anchorMax.y) > float.Epsilon && 
                Value.stretchType == RectTransformSize.StretchType.AutoStretch)
            {
                target.offsetMin = new Vector2(target.offsetMin.x, Value.bottom);
                target.offsetMax = new Vector2(target.offsetMax.x, Value.top);
            }
        }
    }

    [Serializable]
    public class ScaleState : AbstractState<RectTransform, Vector3>
    {
        public override void Apply(RectTransform target)
        {
            target.localScale = Value;
        }
    }
    
    [Serializable]
    public class TransformScaleState : AbstractState<Transform, Vector3>
    {
        public override void Apply(Transform target)
        {
            target.localScale = Value;
        }
    }

    [Serializable]
    public class ShaderValueState : AbstractState<Graphic, MaterialContainer>
    {
        public override void Apply(Graphic target)
        {
            target.material = new Material(target.material);
            target.material.SetFloat(Value.PropertyName, Value.PropertyValue);
            target.materialForRendering.SetFloat(Value.PropertyName, Value.PropertyValue);
        }
    }

    [Serializable]
    public class MaterialState : AbstractState<Graphic, Material>
    {
        public override void Apply(Graphic target)
        {
            target.material = Value == null ? Value : new Material(Value);
        }
    }

    [Serializable]
    public class FontState : AbstractState<Text, Font>
    {
        public override void Apply(Text target)
        {
            target.font = Value;
        }
    }
    
    [Serializable]
    public class FontStyleState : AbstractState<Text, FontStyle>
    {
        public override void Apply(Text target)
        {
            target.fontStyle = Value;
        }
    }

    
    [Serializable]
    public class TextState : AbstractState<Text, string>
    {
        public override void Apply(Text target)
        {
            target.text = Value;
        }
    }

    [Serializable]
    public class TextFontDataState : AbstractState<Text, TextFontDataState.TextFontDataSettings>
    {
        [Serializable]
        public class TextFontDataSettings
        {
            public bool BestFit;
            public int MinSize;
            public int MaxSize;
            public int FontSize;
        }

        public override void Apply(Text target)
        {
            target.resizeTextForBestFit = Value.BestFit;
            target.resizeTextMinSize = Value.MinSize;
            target.resizeTextMaxSize = Value.MaxSize;
            target.fontSize = Value.FontSize;
        }
    }

    [Serializable]
    public class RotateState : AbstractState<Transform, Vector3>
    {
        public override void Apply(Transform target)
        {
            target.Rotate(Value);
        }
    }
    
    [Serializable]
    public class RotationEulerAnglesState : AbstractState<Transform, Vector3>
    {
        public override void Apply(Transform target)
        {
            target.eulerAngles = Value;
        }
    }

    [Serializable]
    public class MaterialContainer
    {
        public string PropertyName;
        public float PropertyValue;
    }

    [Serializable]
    public class LayoutState : AbstractState<LayoutGroup, LayoutState.LayoutSettings>
    {
        [Serializable]
        public class LayoutSettings
        {
            public RectOffset Padding;
            public float Spacing;
            public TextAnchor Anchor;
        }

        public override void Apply(LayoutGroup target)
        {
            target.padding = Value.Padding;
            target.childAlignment = Value.Anchor;
            var horizontalOrVerticalLayoutGroup = target as HorizontalOrVerticalLayoutGroup;
            if (horizontalOrVerticalLayoutGroup != null)
            {
                horizontalOrVerticalLayoutGroup.spacing = Value.Spacing;
            }
        }
    }
    
    [Serializable]
    public class TextAlignmentState : AbstractState<Text, TextAnchor>
    {
        public override void Apply(Text target)
        {
            target.alignment = Value;
        }
    }
}
using EasyTextEffects.Editor.EditorDocumentation;
using EasyTextEffects.Editor.MyBoxCopy.Attributes;
using TMPro;
using UnityEngine;
using static EasyTextEffects.Editor.EditorDocumentation.FoldBoxAttribute;

namespace EasyTextEffects.Effects
{
    [CreateAssetMenu(fileName = "Color", menuName = "Easy Text Effects/1. Color", order = 1)]
    public class Effect_Color : TextEffectInstance
    {
        public enum ColorType
        {
            Gradient,
            BetweenTwoColors,
            OnlyAlpha,
            ColorToOriginal,
            OriginalToColor,
        }

        public enum GradientOrientation
        {
            Horizontal,
            HorizontalPerCharacter,
            Vertical,
        }

        [Space(10)]
        [Header("Color")]
        [FoldBox("Color", new[]
        {
            "Gradient: Applies a gradient horizontally across the text.",
            "Between Two Colors: Animates between two colors.",
            "Only Alpha: Animates only the alpha (transparency) of the text.",
            "Color To Original: Animates from a start color to the original color of the text.",
            "Original To Color: Animates from the original color of the text to an end color.",
        }, new[] { ContentType.Text })]
        public ColorType colorType;

        [ConditionalField(nameof(colorType), false, ColorType.BetweenTwoColors, ColorType.ColorToOriginal)]
        public Color startColor = Color.white;

        [ConditionalField(nameof(colorType), false, ColorType.BetweenTwoColors, ColorType.OriginalToColor)]
        public Color endColor = Color.white;


        [ConditionalField(nameof(colorType), false, ColorType.Gradient)]
        public Gradient gradient;

        [ConditionalField(nameof(colorType), false, ColorType.Gradient)]
        public GradientOrientation orientation;

        [ConditionalField(nameof(orientation), false, GradientOrientation.HorizontalPerCharacter,
            GradientOrientation.Vertical)]
        public int stride = 10;


        [ConditionalField(nameof(colorType), false, ColorType.OnlyAlpha)] [Range(0, 1)]
        public float startAlpha = 0;

        [ConditionalField(nameof(colorType), false, ColorType.OnlyAlpha)] [Range(0, 1)]
        public float endAlpha = 1;

        public override void ApplyEffect(TMP_TextInfo _textInfo, int _charIndex, int _startVertex = 0, int _endVertex = 3)
        {
            if (!CheckCanApplyEffect(_charIndex)) return;
            TMP_CharacterInfo charInfo = _textInfo.characterInfo[_charIndex];
            var materialIndex = charInfo.materialReferenceIndex;

            // Pre-calculate interpolation for uniform color types
            Color uniformColor = default;
            float uniformAlpha = default;
            
            if (colorType == ColorType.BetweenTwoColors)
            {
                uniformColor = Interpolate(startColor, endColor, _charIndex);
            }
            else if (colorType == ColorType.OnlyAlpha)
            {
                uniformAlpha = Interpolate(startAlpha, endAlpha, _charIndex);
            }

            for (var v = _startVertex; v <= _endVertex; v++)
            {
                var vertexIndex = charInfo.vertexIndex + v;
                Color color = _textInfo.meshInfo[materialIndex].colors32[vertexIndex];
                
                if (colorType == ColorType.Gradient)
                {
                    if (orientation == GradientOrientation.Horizontal)
                    {
                        var t = Interpolate(0, 1, _charIndex);
                        color = gradient.Evaluate(t);
                    }
                    if (orientation == GradientOrientation.HorizontalPerCharacter)
                    {
                        var start = Interpolate(0, 1, _charIndex);
                        var end = Interpolate(0, 1, _charIndex + stride);
                        var t = v == 0 || v == 1 ? start : end;
                        color = gradient.Evaluate(t);
                    }
                    if (orientation == GradientOrientation.Vertical)
                    {
                        var start = Interpolate(0, 1, _charIndex);
                        var end = Interpolate(0, 1, _charIndex + stride);
                        var t = v == 1 || v == 2 ? start : end;
                        color = gradient.Evaluate(t);
                    }
                }
                else if (colorType == ColorType.BetweenTwoColors)
                {
                    color = uniformColor;
                }
                else if (colorType == ColorType.OnlyAlpha)
                {
                    color.a = uniformAlpha;
                }
                else if (colorType == ColorType.ColorToOriginal)
                {
                    color = Interpolate(startColor, color, _charIndex);
                }
                else if (colorType == ColorType.OriginalToColor)
                {
                    color = Interpolate(color, endColor, _charIndex);
                }
                _textInfo.meshInfo[materialIndex].colors32[vertexIndex] = color;
            }
        }
    }
}
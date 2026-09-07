using TMPro;
using UnityEngine;

namespace EasyTextEffects.Effects
{
    [CreateAssetMenu(fileName = "Rotate", menuName = "Easy Text Effects/3. Rotate", order = 3)]
    public class Effect_Rotate : TextEffectInstance
    {
        [Space(10)]
        [Header("Rotate")]
        public Vector2 centerOffset = Vector2.zero;
        [Range(-360, 360)] public float startAngle = 90;
        [Range(-360, 360)] public float endAngle = 0;

        public override void ApplyEffect(TMP_TextInfo _textInfo, int _charIndex, int _startVertex = 0, int _endVertex = 3)
        {
            if (!CheckCanApplyEffect(_charIndex)) return;
            TMP_CharacterInfo charInfo = _textInfo.characterInfo[_charIndex];
            var materialIndex = charInfo.materialReferenceIndex;
            var verts = _textInfo.meshInfo[materialIndex].vertices;
            
            // Calculate angle and quaternion once per character instead of vertex
            var angle = Interpolate(startAngle, endAngle, _charIndex);
            Quaternion rotation = Quaternion.Euler(0, 0, angle);
            
            Vector3 center = CharCenter(charInfo, verts) + new Vector3(centerOffset.x, centerOffset.y, 0);
            
            for (var v = _startVertex; v <= _endVertex; v++)
            {
                var vertexIndex = charInfo.vertexIndex + v;
                Vector3 fromCenter = verts[vertexIndex] - center;
                verts[vertexIndex] = center + rotation * fromCenter;
            }
        }
    }
}
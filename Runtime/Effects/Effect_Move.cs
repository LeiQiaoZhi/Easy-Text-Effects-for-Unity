using TMPro;
using UnityEngine;

namespace EasyTextEffects.Effects
{
    [CreateAssetMenu(fileName = "Move", menuName = "Easy Text Effects/2. Move", order = 2)]
    public class Effect_Move : TextEffectInstance
    {
        [Space(10)]
        [Header("Move")]
        public Vector2 startOffset = -Vector2.up * 10;
        public Vector2 endOffset;

        public override void ApplyEffect(TMP_TextInfo _textInfo, int _charIndex, int _startVertex = 0, int _endVertex = 3)
        {
            if (!CheckCanApplyEffect(_charIndex)) return;
            TMP_CharacterInfo charInfo = _textInfo.characterInfo[_charIndex];
            var materialIndex = charInfo.materialReferenceIndex;
            var verts = _textInfo.meshInfo[materialIndex].vertices;
            
            // Allocate Vectors outside the loop
            Vector2 offset = Interpolate(startOffset, endOffset, _charIndex);
            Vector3 offset3D = new Vector3(offset.x, offset.y, 0);

            for (var v = _startVertex; v <= _endVertex; v++)
            {
                var vertexIndex = charInfo.vertexIndex + v;
                verts[vertexIndex] += offset3D;
            }
        }
    }
}
using EasyTextEffects.Editor.EditorDocumentation;
using EasyTextEffects.Editor.MyBoxCopy.Attributes;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

using static EasyTextEffects.Editor.EditorDocumentation.FoldBoxAttribute;

namespace EasyTextEffects.Effects
{
    public abstract class TextEffectInstance : TextEffect_Base
    {
        public enum AnimationType
        {
            Loop,
            LoopFixedDuration,
            PingPong,
            OneTime
        }

        [Space(10)] [Header("Type")] 
        [FoldBox("",
            new[]
            {
                "Packages/com.qiaozhilei.easy-text-effects/Documentation/Images/onetime.png, 200",
                "Packages/com.qiaozhilei.easy-text-effects/Documentation/Images/notonetime.png, 300",
                "Packages/com.qiaozhilei.easy-text-effects/Documentation/Images/fixed.png, 300",
                "Packages/com.qiaozhilei.easy-text-effects/Documentation/Images/loopvspingpong.png, 300",
            },
            new[] { ContentType.Image })]
        public AnimationType animationType = AnimationType.PingPong;

        [ConditionalField(nameof(animationType), false, AnimationType.LoopFixedDuration)]
        public float fixedDuration;


        [Space(10)]
        [FormerlySerializedAs("duration")]
        [Header("Timing")]
        [FoldBox("Timing Explained",
            new[] { "Packages/com.qiaozhilei.easy-text-effects/Documentation/Images/time.png, 300" },
            new[] { ContentType.Image })]
        public float durationPerChar = 0.5f;

        [FoldBox("Timing Explained",
            new[] { "Packages/com.qiaozhilei.easy-text-effects/Documentation/Images/time.png, 300" },
            new[] { ContentType.Image })]
        public float timeBetweenChars = 0.05f;

        [FoldBox("No Delay Explained",
            new[]
            {
                "If enabled, the effect will start immediately for all characters, instead of waiting for the previous character to finish.",
                "Packages/com.qiaozhilei.easy-text-effects/Documentation/Images/nodelay.png, 300"
            }, new[] { ContentType.Text, ContentType.Image })]
        public bool noDelayForChars;

        [FoldBox("Reverse Char Order Explained",
            new[]
            {
                "If enabled, the effect will start from the last character instead of the first. This is useful for exit animations.",
                "Packages/com.qiaozhilei.easy-text-effects/Documentation/Images/reverse.png, 300"
            }, new[] { ContentType.Text, ContentType.Image })]
        public bool reverseCharOrder;

        [Space(10)] [FormerlySerializedAs("curve")] [Header("Curve")]
        public AnimationCurve easingCurve = AnimationCurve.Linear(0, 0, 1, 1);
        public bool clampBetween0And1;


        protected float startTime;
        protected bool started;
        protected bool isComplete;
        private TextEffectEntry currentEntry;

        protected bool CheckCanApplyEffect(int _charIndex)
        {
            return started && _charIndex >= startCharIndex && _charIndex < startCharIndex + charLength;
        }

        public virtual void StartEffect()
        {
            currentEntry = entry;

            started = true;
            startTime = TimeUtil.GetTime();
            isComplete = false;

            if (animationType == AnimationType.OneTime || animationType == AnimationType.LoopFixedDuration)
            {
                easingCurve.preWrapMode = WrapMode.Clamp;
                easingCurve.postWrapMode = WrapMode.Clamp;
            }
            else if (animationType == AnimationType.Loop)
            {
                easingCurve.preWrapMode = noDelayForChars ? WrapMode.Loop : WrapMode.Clamp;
                easingCurve.postWrapMode = WrapMode.Loop;
            }
            else if (animationType == AnimationType.PingPong)
            {
                easingCurve.preWrapMode = noDelayForChars ? WrapMode.PingPong : WrapMode.Clamp;
                easingCurve.postWrapMode = WrapMode.PingPong;
            }
        }

        public virtual void StopEffect()
        {
            started = false;
            isComplete = true;
        }

        protected float Interpolate(float _start, float _end, int _charIndex)
        {
            var time = GetTimeForChar(_charIndex);
            // return Mathf.Lerp(_start, _end, easingCurve.Evaluate(time / durationPerChar));
            var t = easingCurve.Evaluate(time / durationPerChar);
            if (clampBetween0And1)
                t = Mathf.Clamp01(t);
            return _start * (1 - t) + _end * t;
        }

        protected Vector2 Interpolate(Vector2 _start, Vector2 _end, int _charIndex)
        {
            var time = GetTimeForChar(_charIndex);
            // return Vector2.Lerp(_start, _end, easingCurve.Evaluate(time / durationPerChar));
            var t = easingCurve.Evaluate(time / durationPerChar);
            if (clampBetween0And1)
                t = Mathf.Clamp01(t);
            return _start * (1 - t) + _end * t;
        }

        protected Color Interpolate(Color _start, Color _end, int _charIndex)
        {
            var time = GetTimeForChar(_charIndex);
            // return Color.Lerp(_start, _end, easingCurve.Evaluate(time / durationPerChar));
            var t = easingCurve.Evaluate(time / durationPerChar);
            if (clampBetween0And1)
                t = Mathf.Clamp01(t);
            return _start * (1 - t) + _end * t;
        }

        private float GetTimeForChar(int _charIndex)
        {
            var time = TimeUtil.GetTime();

            // Check completion for LoopFixedDuration
            if (animationType == AnimationType.LoopFixedDuration && time - startTime > fixedDuration)
            {
                startTime += fixedDuration;
                if (!isComplete)
                {
                    isComplete = true;
                    currentEntry?.InvokeCompleted();
                }
            }

            // Check completion for OneTime
            else if (animationType == AnimationType.OneTime && !isComplete)
            {
                float totalDuration = noDelayForChars ?
                    durationPerChar :
                    (durationPerChar + (timeBetweenChars * (charLength - 1)));

                if (time - startTime > totalDuration)
                {
                    isComplete = true;
                    currentEntry?.InvokeCompleted();
                }
            }


            var charOrder = _charIndex - startCharIndex;
            if (reverseCharOrder)
                charOrder = charLength - charOrder - 1;
            var charStartTime = startTime + timeBetweenChars * charOrder;
            return time - charStartTime;
        }

        public bool IsComplete() => isComplete;

        public virtual TextEffectInstance Instantiate()
        {
            return Instantiate(this);
        }
    }
}
using System.Collections.Generic;
using UnityEngine;

namespace EasyTextEffects.Samples
{
    public class SequenceManager : MonoBehaviour
    {
        public List<GameObject> slides;

        private int currentSlideIndex_ = -1;
        public List<float> slideDelays;

        void Start()
        {
            for (int i = 0; i < slides.Count; i++)
            {
                slides[i].SetActive(false);
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
            {
                NextSlide();
            }
        }

        public void NextSlide()
        {
            if (currentSlideIndex_ < slides.Count - 1)
            {
                if (currentSlideIndex_ >= 0)
                {
                    StopEffect(currentSlideIndex_);
                }

                currentSlideIndex_++;
                Invoke(nameof(StartCurrentEffect), slideDelays[currentSlideIndex_]);
            }
            else
            {
                StopEffect(slides.Count - 1);
                currentSlideIndex_ = -1;
            }
        }

        private void StopEffect(int _index)
        {
            GameObject currentSlide = slides[_index];
            var currentText = currentSlide.GetComponentInChildren<TextEffect>();
            if (currentText == null)
            {
                currentSlide.SetActive(false);
                return;
            }
            currentText.StopOnStartEffects();
            var effect = currentText.StartManualEffect("exit");
            if (effect == null)
            {
                currentSlide.SetActive(false);
                return;
            }
            effect.onEffectCompleted.AddListener(() => 
            {
                currentSlide.SetActive(false);
                effect.onEffectCompleted.RemoveAllListeners();
            });
        }

        private void StartEffect(int _index)
        {
            GameObject currentSlide = slides[_index];

            currentSlide.SetActive(true);
            var currentText = currentSlide.GetComponentInChildren<TextEffect>();
            if (currentText != null)
            {
                currentText.UpdateStyleInfos();
            }
        }

        private void StartCurrentEffect()
        {
            StartEffect(currentSlideIndex_);
        }
    }
}
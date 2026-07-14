using DG.Tweening;
using TMPro;
using UnityEngine;

namespace GASHAPWN.UI
{
    /// <summary>
    /// GUI Wrapper for feedback text of ControlsBindScreen
    /// </summary>
    public class ControlsBindFeedbackText : MonoBehaviour
    {

        [SerializeField] private TextMeshProUGUI feedbackText;
        [SerializeField] private GameObject feedbackTextControls;

        private CanvasGroup _cg;
        private FeedbackState _state = FeedbackState.Waiting;
        private Tween _flashTween;

        private void Awake()
        {
            _cg = GetComponent<CanvasGroup>();
            SetState(FeedbackState.Waiting, -1);
        }

        private void StartFlashing()
        {
            _flashTween?.Kill();

            _flashTween = _cg
                .DOFade(0.15f, 1f)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
        }

        public void SetState(FeedbackState state, int playerIndex)
        {
            switch (state)
            {
                case FeedbackState.Waiting:
                    feedbackText.gameObject.SetActive(true);
                    feedbackTextControls.gameObject.SetActive(false);
                    feedbackText.SetText("Waiting...");
                    StartFlashing();
                    _state = state;
                    break;
                case FeedbackState.Prompting:
                    feedbackText.gameObject.SetActive(false);
                    feedbackTextControls.gameObject.SetActive(true);
                    _state = state;
                    break;
                case FeedbackState.Connected:
                    feedbackText.gameObject.SetActive(true);
                    feedbackTextControls.gameObject.SetActive(false);
                    feedbackText.SetText($"Player {playerIndex + 1} Ready!");
                    _flashTween?.Kill();
                    _cg.alpha = 1;
                    _state = state;
                    break;
            }
        }

        public enum FeedbackState
        {
            Waiting,
            Prompting,
            Connected
        }
    }
}
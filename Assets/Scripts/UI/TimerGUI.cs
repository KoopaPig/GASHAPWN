using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GASHAPWN.UI {
    /// <summary>
    /// Controller for Timer Display
    /// </summary>
    public class TimerGUI : MonoBehaviour
    {
        [Header("Timer GUI Elements")]
            [Tooltip("Text for timer")]
            [SerializeField] private TextMeshProUGUI timerText;
            
            [Tooltip("Circle element of timer to radially un-fill")]
            [SerializeField] private Image timerCircle;

        [Header("Color Control")]
            [Tooltip("Base color of Timer Circle")]
            [SerializeField] private Color baseColor;

            [Tooltip("Red tone for Timer Circle when time is below red threshold")]
            [SerializeField] private Color redColor;

            [Tooltip("Seconds remaining at which Timer Circle turns red")]
            [SerializeField] private float redThreshold = 30f;

        [Header("Timer Animator")]
            [SerializeField] private Animator timerAnimator;

        // Keep track of remaining time
        private float remainingTime;
        // Store total time locally
        private float totalTime;
        // Reference to BattleManager
        private BattleManager battleManager;

        private void Awake()
        {
            battleManager = FindFirstObjectByType<BattleManager>();
        }

        private void Start()
        {
            remainingTime = battleManager.battleTime;
            totalTime = remainingTime;
            timerCircle.fillAmount = 1f;
        }

        private void Update()
        {
            remainingTime = battleManager.battleTime;

            int minutes = Mathf.FloorToInt(remainingTime / 60);
            int seconds = Mathf.FloorToInt(remainingTime % 60);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);

            // text and circle turns red if remaining time is less than some threshold
            if (remainingTime < redThreshold)
            {
                timerText.color = redColor;
                timerCircle.color = redColor;
            }
            else { 
                timerText.color = baseColor; 
                timerCircle.color = baseColor; 
            }

            if (remainingTime < redThreshold && remainingTime > 0)
            {
                timerAnimator.SetBool("isWarningEffect", true); // Start the warning effect
            }
            else
            {
                timerAnimator.SetBool("isWarningEffect", false); // Stop the warning effect
            }

            // proportionally fill the timer circle
            timerCircle.fillAmount = remainingTime / totalTime;

            if (remainingTime <= 0)
            {
                timerText.text = "Time Up!";
            }
        }
    }
}
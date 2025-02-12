using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GASHAPWN.UI {
    public class TimerGUI : MonoBehaviour
    {
        [Header("Timer GUI Elements")]
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private Image timerCircle;

        [Header("Color Control")]
        [SerializeField] private Color baseColor;
        [SerializeField] private Color redColor;
        [SerializeField] private float redThreshold = 30f;

        private float remainingTime;
        private float totalTime;

        [Header("Timer Animator")]
        [SerializeField] private Animator timerAnimator;

        private BattleManager battleManager;
        private void Start()
        {
            remainingTime = battleManager.battleTime;
            totalTime = remainingTime;
            timerCircle.fillAmount = 1f;
        }

        private void Awake()
        {
            battleManager = FindFirstObjectByType<BattleManager>();
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
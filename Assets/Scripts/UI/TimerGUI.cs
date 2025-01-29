using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GASHAPWN {
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

        private bool isTimerActive = true;
        [Header("Timer Animator")]
        [SerializeField] private Animator timerAnimator;

        /// <summary>
        /// Set Total Time from BattleManager
        /// </summary>
        /// <param name="time"></param>
        public void SetTotalTime(float time)
        {
            totalTime = time;
        }

        /// <summary>
        /// Set Remaining Time from BattleManager
        /// </summary>
        /// <param name="time"></param>

        public void SetRemainingTime(float time)
        {
            remainingTime = time;
        }

        private void Start()
        {
            timerCircle.fillAmount = 1f;
            isTimerActive = true;
            // TEMP DEBUG
            //remainingTime = 30;
            //totalTime = 180f;
        }

        private void Update()
        {

            // TEMP DEBUG
            //if (isTimerActive)
            //{
            //    if (remainingTime >= 0)
            //    {
            //        remainingTime -= Time.deltaTime;
            //    }
            //    else if (remainingTime < 0)
            //    {
            //        remainingTime = 0;
            //    }
            //}   
            // END TEMP DEBUG

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
                isTimerActive = false;
            }
        }
    }
}
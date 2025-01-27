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

        // Set Total Time from BattleManager
        public void SetTotalTime(float time)
        {
            totalTime = time;
        }

        // Set Remaining Time from BattleManager

        public void SetRemainingTime(float time)
        {
            remainingTime = time;
        }

        private void Start()
        {
            timerCircle.fillAmount = 1f;
            //remainingTime = 28;
            //totalTime = 180f;
        }

        private void Update()
        {
            int minutes = Mathf.FloorToInt(remainingTime / 60);
            int seconds = Mathf.FloorToInt(remainingTime % 60);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);

            // text turns red if remaining time is less than some threshold
            if (remainingTime < redThreshold) timerText.color = redColor;
            else timerText.color = baseColor;

            // proportionally fill the timer circle
            timerCircle.fillAmount = (totalTime - remainingTime) / totalTime;
        }
    }
}
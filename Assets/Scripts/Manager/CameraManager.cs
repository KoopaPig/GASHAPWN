using System.Collections;
using UnityEngine;

namespace GASHAPWN
{
    public class CameraManager : MonoBehaviour
    {
        [Header("Positions")]
        public Transform StartingPosition;
        public Transform EndingPosition;
        public Transform FocusDoorPosition;
        public Transform SpawnPosition;

        [Header("Cameras")]
        public Camera Maincam;
        public Camera Player1Cam;
        public Camera Player2Cam;

        public bool PathingEnabled;

        [Header("Player Capsule Prefabs")]
        public GameObject Player1Capsule;
        public GameObject Player2Capsule;

        private Figure WinningFigure;
        private string WinningFigureTag;

        private float time;
        private Coroutine waitCamCoroutine;

        private void Awake()
        {
            Maincam.transform.position = StartingPosition.position;
            Maincam.transform.rotation = StartingPosition.rotation;
        }

        private void Start()
        {
            BattleManager.Instance.ChangeToCountdown.AddListener(StartPath);
            BattleManager.Instance.OnWinningFigure.AddListener(SwitchCamera);
            BattleManager.Instance.ChangeToNewFigure.AddListener(SwitchToMain);
        }

        public void StartPath(BattleState state)
        {
            PathingEnabled = true;
        }

        public void SwitchCamera(string Tag, Figure figure)
        {
            waitCamCoroutine = StartCoroutine(WaitToSwitchCamera(Tag, figure, 3.0f));
        }

        public void SwitchToMain(BattleState state)
        {
            if (waitCamCoroutine != null) { StopCoroutine(waitCamCoroutine); }
            // Turn off player cameras
            if (Player1Cam.enabled) Player1Cam.enabled = false;
            else Player2Cam.enabled = false;

            // Translate main camera to focus on the door
            Maincam.transform.SetPositionAndRotation(FocusDoorPosition.position, FocusDoorPosition.rotation);

            // Spawn a player behind the door

            GameObject PlayerCapsule = null;
            if (WinningFigureTag == "Player1") PlayerCapsule = Player1Capsule;
            else if (WinningFigureTag == "Player2") PlayerCapsule = Player2Capsule;
            else Debug.Log("Unknown Tag");

            GameObject SpawnedCapsule = Instantiate(PlayerCapsule, SpawnPosition);
            Instantiate(WinningFigure.capsuleModelPrefab, SpawnedCapsule.transform);

            // Turn on main camera
            Maincam.enabled = true;
        }

        private float TimeFunc(float time)
        {
            if (time < 0.75f)
            {
                time += 0.3f * Time.deltaTime;
            }
            else time += 0.1f * Time.deltaTime;
            return time;
        }

        private void Update()
        {
            if (PathingEnabled && BattleManager.Instance.countDownTime > 0)
            {
                Maincam.transform.position = Vector3.Lerp(StartingPosition.position, EndingPosition.position, time);
                Maincam.transform.rotation = Quaternion.Lerp(StartingPosition.rotation, EndingPosition.rotation, time);
                time = TimeFunc(time);
            }
            else PathingEnabled = false;
        }

        private IEnumerator WaitToSwitchCamera(string tag, Figure figure, float waitDuration)
        {
            yield return new WaitForSeconds(waitDuration);
            WinningFigureTag = tag;
            WinningFigure = figure;
            if (tag == "Player1")
            {
                Maincam.enabled = false;
                Player1Cam.enabled = true;
            }
            else if (tag == "Player2")
            {
                Maincam.enabled = false;
                Player2Cam.enabled = true;
            }
            else Debug.LogError("Unknown Tag");

        }

        private void OnDisable()
        {
            if (BattleManager.Instance != null)
            {
                BattleManager.Instance.ChangeToCountdown.RemoveListener(StartPath);
                BattleManager.Instance.OnWinningFigure.RemoveListener(SwitchCamera);
                BattleManager.Instance.ChangeToNewFigure.RemoveListener(SwitchToMain);
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

namespace GASHAPWN
{
    // TODO: Move this back to a general manager position
    public class CameraManager : MonoBehaviour
    {
        [Header("Positions")]
        public Transform pathStart;
        public Transform pathEnd;
        public Transform machineCapsuleSpawnPos;

        //public bool PathingEnabled;

        [Header("Player Capsule Prefabs")]
        public GameObject Player1Capsule;
        public GameObject Player2Capsule;

        public float pathDuration = 5f;

        private Figure WinningFigure;
        private string WinningFigureTag;

        private float pathTimer = 0f;
        private Coroutine waitCamCoroutine;

        [Header("Cameras")]
        private bool isMoving = false;
        private CinemachineGroupFraming framing;

        [SerializeField] private CinemachineCamera introCam;
        [SerializeField] private CinemachineCamera battleCam;
        [SerializeField] private CinemachineCamera winCam;
        [SerializeField] private CinemachineCamera machineCam;
        
        // maybe a seperate intro cam

        // Store reference to activePlayers PlayerData Components for Camera Effects
        private List<PlayerData> subscribedPlayers = new();

        private void Awake()
        {
            battleCam.Priority = 5;
            winCam.Priority = 0;
            machineCam.Priority = 0;
            //introCam.Priority = 0;
        }

        private void Start()
        {
            BattleManager.Instance.ChangeToCountdown.AddListener(StartPath);
            BattleManager.Instance.OnWinner.AddListener(SwitchToWinCam);
            BattleManager.Instance.ChangeToNewFigure.AddListener(SwitchToMachineCam);
            BattleManager.Instance.ChangeToBattle.AddListener(HandleChangeToBattle);
            framing = battleCam.GetComponent<CinemachineGroupFraming>();
            framing.Damping = 0f;
            foreach (var player in BattleManager.Instance.GetActivePlayers())
            {
                var playerData = player.GetComponent<PlayerData>();
                playerData.OnDamage.AddListener(OnHit);
                subscribedPlayers.Add(playerData);
            }
        }

        public void StartPath(BattleState state)
        {
            isMoving = true;
        }

        public void SwitchToWinCam(GameObject player, string tag, Figure figure)
        {
            waitCamCoroutine = StartCoroutine(WaitToSwitchCamera(player.transform, tag, figure, 3.0f));
        }

        public void SwitchToMachineCam(BattleState state)
        {
            if (waitCamCoroutine != null) { StopCoroutine(waitCamCoroutine); }
            // Turn off winCam
            winCam.Priority = 0;
            machineCam.Priority = 30;

            // Spawn a player behind the door
            GameObject PlayerCapsule = null;
            if (WinningFigureTag == "Player1") PlayerCapsule = Player1Capsule;
            else if (WinningFigureTag == "Player2") PlayerCapsule = Player2Capsule;
            else Debug.Log("Unknown Tag");
            // Spawn in capsule and figure
            GameObject SpawnedCapsule = Instantiate(PlayerCapsule, machineCapsuleSpawnPos);
            SpawnedCapsule.GetComponent<PlayerAttachedFigure>().SetFigureInCapsule(WinningFigure, 2.5f);

            // Turn on main camera
            battleCam.enabled = true;
        }

        // Coroutine to control camera effect when OnDamage event is called
        public IEnumerator HitCamEffect(float duration)
        {
            if (battleCam == null) yield break;

            float halfDuration = duration / 2f;
            float timer = 0f;

            // get targetDutch value in range from -5 to 5
            float targetDutch = Random.Range(1f, 5f) * (Random.value > 0.5f ? 1 : -1);
            float startDutch = battleCam.Lens.Dutch;

            // get targetFOV value in range from -1 to 1
            float targetFOV = battleCam.Lens.FieldOfView + (Random.value > 0.5f ? 1 : -1);
            float startFOV = battleCam.Lens.FieldOfView;

            // Interpolate to target
            while (timer < halfDuration)
            {
                timer += Time.deltaTime;
                float t = timer / halfDuration;
                battleCam.Lens.Dutch = Mathf.Lerp(startDutch, targetDutch, 1 - Mathf.Pow(1 - t, 2));
                battleCam.Lens.FieldOfView = Mathf.Lerp(startFOV, targetFOV, 1 - Mathf.Pow(t, 2));
                yield return null;
            }

            // Interpolate back to 0
            timer = 0f;
            while (timer < halfDuration)
            {
                timer += Time.deltaTime;
                float t = timer / halfDuration;
                battleCam.Lens.Dutch = Mathf.Lerp(targetDutch, startDutch, 1 - Mathf.Pow(1 - t, 2));
                battleCam.Lens.FieldOfView = Mathf.Lerp(targetFOV, startFOV, 1 - Mathf.Pow(t, 2));
                yield return null;
            }
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

        // TODO: Get the move to position working

        //void Update()
        //{
        //    if (isMoving)
        //    {
        //        pathTimer += Time.deltaTime;
        //        float t = Mathf.Clamp01(pathTimer / pathDuration);

        //        Vector3 newPos = Vector3.Lerp(pathStart.position, pathEnd.position, t);

        //        if (t >= 1f)
        //            isMoving = false;
        //    }
        //}

        //private void Update()
        //{
        //    if (PathingEnabled && BattleManager.Instance.countDownTime > 0)
        //    {
        //        Maincam.transform.position = Vector3.Lerp(StartingPosition.position, EndingPosition.position, time);
        //        Maincam.transform.rotation = Quaternion.Lerp(StartingPosition.rotation, EndingPosition.rotation, time);
        //        time = TimeFunc(time);
        //    }
        //    else PathingEnabled = false;
        //}

        private IEnumerator WaitToSwitchCamera(Transform transform, string tag, Figure figure, float waitDuration)
        {
            yield return new WaitForSeconds(waitDuration);
            WinningFigureTag = tag;
            WinningFigure = figure;

            // Set winCam to proper transfrom and set active
            winCam.Follow = transform;
            winCam.LookAt = transform;
            winCam.Priority = 30;
        }

        private void HandleChangeToBattle(BattleState state)
        {
            if (framing != null) framing.Damping = 2f;
        }

        // Whenever damage is dealt, do HitCamEffect
        private void OnHit(int val)
        {
            StartCoroutine(HitCamEffect(0.65f));
        }

        private void OnDisable()
        {
            if (BattleManager.Instance != null)
            {
                BattleManager.Instance.ChangeToCountdown.RemoveListener(StartPath);
                BattleManager.Instance.OnWinner.RemoveListener(SwitchToWinCam);
                BattleManager.Instance.ChangeToNewFigure.RemoveListener(SwitchToMachineCam);
                BattleManager.Instance.ChangeToBattle.RemoveListener(HandleChangeToBattle);

                foreach (var playerData in subscribedPlayers)
                {
                    if (playerData != null) playerData.OnDamage.RemoveListener(OnHit);
                }
                subscribedPlayers.Clear();
            }
        }
    }
}

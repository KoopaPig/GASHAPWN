using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Splines;

namespace GASHAPWN
{
    public class CameraManager : MonoBehaviour
    {
        public bool isDebug = false;

        [Header("Cameras")]
        private CinemachineGroupFraming framing;
        private SplineAnimate dolly;

        [SerializeField] private CinemachineCamera introCam;
        [SerializeField] private CinemachineCamera battleCam;
        [SerializeField] private CinemachineCamera winCam;
        [SerializeField] private CinemachineCamera machineCam; // within GashaMachine
        [SerializeField] private CinemachineCamera debugCam;
        [SerializeField] private CinemachineTargetGroup targetGroup;

        [Header("Player Capsule Prefabs")]
        public GameObject Player1Capsule;
        public GameObject Player2Capsule;

        [Header("Positions")]
        public Transform machineCapsuleSpawnPos;

        private Figure WinningFigure;
        private string WinningFigureTag;

        private Coroutine waitCamCoroutine;

        // Store reference to activePlayers PlayerData Components for Camera Effects
        private List<PlayerData> subscribedPlayers = new();

        private void Awake()
        {
            introCam.Priority = 10;
            battleCam.Priority = 5;
            winCam.Priority = 0;
            machineCam.Priority = 0;
        }

        private void Start()
        {
            BattleManager.Instance.ChangeToCountdown.AddListener(StartPath);
            BattleManager.Instance.OnWinner.AddListener(SwitchToWinCam);
            BattleManager.Instance.ChangeToNewFigure.AddListener(SwitchToMachineCam);
            BattleManager.Instance.ChangeToBattle.AddListener(HandleChangeToBattle);

            framing = battleCam.GetComponent<CinemachineGroupFraming>();
            framing.Damping = 0f;

            dolly = introCam.GetComponent<SplineAnimate>();
            dolly.Duration = BattleManager.Instance.countDownTime;

            foreach (var player in BattleManager.Instance.GetActivePlayers())
            {
                var playerData = player.GetComponent<PlayerData>();
                playerData.OnDamage.AddListener(OnHit);
                subscribedPlayers.Add(playerData);
            }
        }

        private void Update()
        {
            // Switch to Debug Camera
            // 1 -- Player1
            // 2 -- Player2
            // 0 -- Back to previous cam
            if (isDebug)
            {
                List<GameObject> activePlayers = BattleManager.Instance.GetActivePlayers();
                if (Input.GetKeyDown(KeyCode.Alpha1) && activePlayers.Count > 0)
                {
                    debugCam.Priority = 100;
                    // Track Player1
                    Transform playerTransform = activePlayers[0].transform;
                    debugCam.Follow = playerTransform;
                    debugCam.LookAt = playerTransform;
                }
                if (Input.GetKeyDown(KeyCode.Alpha2) && activePlayers.Count > 0)
                {
                    debugCam.Priority = 100;
                    // Track Player2
                    Transform playerTransform = activePlayers[1].transform;
                    debugCam.Follow = playerTransform;
                    debugCam.LookAt = playerTransform;
                }
                if (Input.GetKeyDown(KeyCode.Alpha0))
                {
                    // '0' pressed, so switch off from this camera
                    debugCam.Priority = 0;
                }
            }
        }

        private void LateUpdate()
        {
            UpdateTargetGroup();
        }

        public void StartPath(BattleState state)
        {
            introCam.Priority = 30;
            dolly.Play();
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

        private IEnumerator WaitToSwitchCamera(Transform transform, string tag, Figure figure, float waitDuration)
        {
            yield return new WaitForSeconds(waitDuration);
            WinningFigureTag = tag;
            WinningFigure = figure;

            // Set winCam to proper transfrom and set active
            winCam.Follow = transform;
            //winCam.LookAt = transform;
            winCam.Priority = 30;
        }

        private void HandleChangeToBattle(BattleState state)
        {
            if (!dolly.IsPlaying) { introCam.Priority = 0; }
            if (framing != null) framing.Damping = 2f;
        }

        // Whenever damage is dealt, do HitCamEffect
        private void OnHit(int val)
        {
            StartCoroutine(HitCamEffect(0.65f));
        }

        // Make sure that MainCameraTargetGroup is only targetting active players
        private void UpdateTargetGroup()
        {
            if (BattleManager.Instance.GetActivePlayers() != null)
            {
                List<GameObject> activePlayers = BattleManager.Instance.GetActivePlayers();

                // Clean the list: remove destroyed/null entries
                activePlayers.RemoveAll(player => player == null);

                Dictionary<Transform, CinemachineTargetGroup.Target> currentTargets = new();

                foreach (var target in targetGroup.Targets)
                {
                    if (target.Object != null)
                        currentTargets[target.Object] = target;
                }

                List<CinemachineTargetGroup.Target> newTargetList = new();

                foreach (var player in activePlayers)
                {
                    Transform playerTransform = player.transform;
                    newTargetList.Add(new CinemachineTargetGroup.Target
                    {
                        Object = playerTransform,
                        Weight = 2f,
                        Radius = 30f
                    });
                }

                targetGroup.Targets = newTargetList;
            }
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

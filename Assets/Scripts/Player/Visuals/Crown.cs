using GASHAPWN;
using GASHAPWN.Utility;
using System.Collections;
using UnityEngine;

public class Crown : MonoBehaviour
{
    [Tooltip("Prefab object for crown")]
    [SerializeField] private GameObject crownPrefab;

    private Animator _crownAnimator;

    private GameObject _crownInstance;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        BattleManager.Instance.OnWinner.AddListener(InitializeCrown);
    }

    private void InitializeCrown(GameObject player, string name, Figure figure) => StartCoroutine(SpawnCrown(player, 1.5f));
    private IEnumerator SpawnCrown(GameObject player, float waitDuration)
    {
        yield return new WaitForSeconds(waitDuration);
        yield return StartCoroutine(PlayerHelpers.RotateUpDirectionCoroutine(player.GetComponent<Rigidbody>(), player.transform, Vector3.up, 0.25f));

        Renderer playerRenderer = player.GetComponentInChildren<Renderer>();

        Vector3 topOfPlayer = playerRenderer.bounds.center + player.transform.up * playerRenderer.bounds.extents.y;

        Quaternion crownRotation = Quaternion.LookRotation(player.transform.forward, player.transform.up);

        _crownInstance = Instantiate(crownPrefab, topOfPlayer, crownRotation, player.transform);
        _crownAnimator = _crownInstance.GetComponent<Animator>();

        int randomAnim = Random.Range(1, 2);
        _crownAnimator.SetBool("Spawn", true);
        _crownAnimator.SetInteger("RandomSpawn", randomAnim);
    }


    private void OnDisable()
    {
        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.OnWinner.RemoveListener(InitializeCrown);
        }
        Destroy(_crownInstance);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

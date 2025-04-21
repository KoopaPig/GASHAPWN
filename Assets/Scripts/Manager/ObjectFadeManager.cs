using System.Collections.Generic;
using GASHAPWN;
using Unity.VisualScripting;
using UnityEngine;

public class ObjectFadeManager : MonoBehaviour
{
    public static ObjectFadeManager Instance;
    
    // Stores all ObjectFade objects in scene
    private HashSet<ObjectFade> allFadables = new HashSet<ObjectFade>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void Register(ObjectFade fadeObj)
    {
        allFadables.Add(fadeObj);
    }

    public void Unregister(ObjectFade fadeObj)
    {
        allFadables.Remove(fadeObj);
    }

    private void Update()
    {


        if (Camera.main == null || BattleManager.Instance == null) return;

        HashSet<ObjectFade> objectsToFade = new HashSet<ObjectFade>();

        if (BattleManager.Instance.GetActivePlayers() != null)
        {
            foreach (var player in BattleManager.Instance.GetActivePlayers())
            {
                Vector3 origin = Camera.main.transform.position;
                Vector3 target = player.transform.position;
                Vector3 direction = (target - origin).normalized;
                float distance = Vector3.Distance(origin, target);

                RaycastHit[] hits = Physics.RaycastAll(origin, direction, distance);

                foreach (var hit in hits)
                {
                    var fade = hit.transform.GetComponent<ObjectFade>();
                    if (fade != null)
                    {
                        objectsToFade.Add(fade);
                    }
                }
            }
            // then set objects as faded if in "objectsToFade"
            foreach (var fade in allFadables)
            {
                bool shouldFade = objectsToFade.Contains(fade);
                fade.SetFaded(shouldFade);
            }
        } else
        {
            foreach (var fade in allFadables) fade.SetFaded(false);
        }



    }
}

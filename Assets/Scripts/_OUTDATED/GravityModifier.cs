using UnityEngine;
using System.Collections;

public class GravityModifier : MonoBehaviour
{
    [SerializeField] private float scalar = 1f;

    private ConstantForce cForce;
    private Vector3 forceDirection;

    void Start()
    {
        cForce = GetComponent<ConstantForce>();
        forceDirection = new Vector3(0, -1 * scalar, 0);
        cForce.force = forceDirection;
    }

    // Update is called once per frame
    void Update()
    {
        cForce.force = forceDirection;
    }
}

using Unity.VisualScripting;
using UnityEngine;

public class AnchorSpring : MonoBehaviour
{
    public Transform anchorPos;
    public Transform fishingIndicatorPos;
    public float tetherStrength = 5f;
    public float tetherDamping = 5f;

    public float tetherStrReleased = 2f;
    public float tetherDmpReleased = 1f;

    public float maxDistance = 2f;
    public bool isBobbing = false;

    Rigidbody rb;
    LineRenderer line;

    Transform currentAnchor;
    float currentTetherStr;
    float currentTetherDmp;
    

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        line = GetComponent<LineRenderer>();

        currentTetherStr = tetherStrength;
        currentTetherDmp = tetherDamping;

    }

    void FixedUpdate()
    {
        currentAnchor = isBobbing ? fishingIndicatorPos : anchorPos;

        Vector3 toAnchor = currentAnchor.position - transform.position;
        float distance = toAnchor.magnitude;

        if (distance > 0.01f)
        {
            Vector3 pullDir = toAnchor.normalized;

            float clampedDistance = Mathf.Min(distance, maxDistance);
            Vector3 pullForce = pullDir * currentTetherStr * clampedDistance;

            Vector3 dampingForce = -rb.linearVelocity * currentTetherDmp;

            rb.AddForce(pullForce + dampingForce);

            if (distance > maxDistance)
            {
                Vector3 clampedPos = currentAnchor.position - pullDir * maxDistance;
                rb.position = clampedPos;
                rb.linearVelocity = Vector3.zero;
            }
        }
    }
    void LateUpdate()
    {
        line.positionCount = 2;
        line.SetPosition(0, anchorPos.position);
        line.SetPosition(1, transform.position);
    }

    public void Release()
    {
        isBobbing = true;
        currentTetherStr = tetherStrReleased;
        currentTetherDmp = tetherDmpReleased;
    }

    public void Retract()
    {
        isBobbing = false;
        currentTetherStr = tetherStrength;
        currentTetherDmp = tetherDamping;
    }
}
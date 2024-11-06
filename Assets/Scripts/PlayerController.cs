using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed;
    public float turnSpeed;
    public float maxSpeed;

    public GameObject followCam;
    public GameObject fishingCam;
    public InputActionReference fishAction;

    bool isFishing = false;

    Vector3 moveDir;
    Rigidbody rb;

    public Animator fishingAnim;

    void Awake() {
        rb = GetComponent<Rigidbody>();
    }

    void Start()
    {
        
    }

    void Update()
    {
        transform.Rotate(0, moveDir.x * turnSpeed * Time.deltaTime, 0);
    }

    private void FixedUpdate()
    {
        rb.AddForce(moveDir.z * transform.forward * moveSpeed, ForceMode.VelocityChange);

        if (rb.linearVelocity.magnitude > maxSpeed)
        {
            var linearVelocity = rb.linearVelocity;
            linearVelocity.x = Mathf.Clamp(linearVelocity.x, -maxSpeed, maxSpeed);
            linearVelocity.z = Mathf.Clamp(linearVelocity.z, -maxSpeed, maxSpeed);
            rb.linearVelocity = linearVelocity;
        }
    }

    void OnMove(InputValue input) {
        var inputDir = input.Get<Vector2>();
        moveDir = new Vector3(inputDir.x, 0, inputDir.y);
    }

    void OnFish()
    {
        isFishing = !isFishing;
        followCam.SetActive(!isFishing);
        fishingCam.SetActive(isFishing);

        if (isFishing)
        {
            fishingAnim.SetTrigger("Fish");
        } else
        {
            fishingAnim.SetTrigger("Catch");
        }
        
    }
}

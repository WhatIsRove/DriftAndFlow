using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed;
    public float turnSpeed;
    public float maxSpeed;

    public GameObject turnObject;

    public GameObject followCam;
    public GameObject fishingCam;

    bool isFishing = false;
    bool isCatching = false;

    Vector3 moveDir;
    Rigidbody rb;

    public Animator fishingAnim;
    public Animator fishingUIAnim;
    public Animator HUDAnim;

    public Animator PopUpAnim;
    public GameObject popUpItem;

    public Item item;
    public Item otherItem;
    public GameObject inventory;

    void Awake() {
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        AudioManager.Instance.Play("Engine");
    }

    void Update()
    {
        transform.Rotate(0, moveDir.x * turnSpeed * Time.deltaTime, 0);

        if (moveDir.z > 0 || moveDir.z < 0)
        {
            if (!AudioManager.Instance.IsPlaying("EngineAccel"))
            {
                AudioManager.Instance.Stop("Engine");
                AudioManager.Instance.Play("EngineAccel");
            }
        } else
        {
            if (!AudioManager.Instance.IsPlaying("Engine"))
            {
                AudioManager.Instance.Stop("EngineAccel");
                AudioManager.Instance.Play("Engine");
            }
        }
    }

    private void OnDrawGizmos()
    {
        rb = GetComponent<Rigidbody>();
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, rb.transform.forward*5f);
        Gizmos.DrawRay(transform.position, rb.linearVelocity*5f);
    }

    private void FixedUpdate()
    {
        rb.AddForce(moveDir.z * (transform.forward) * moveSpeed, ForceMode.Acceleration);
        
        //if (rb.linearVelocity.magnitude > maxSpeed)
        //{
        //    var linearVelocity = rb.linearVelocity;
        //    linearVelocity.x = Mathf.Clamp(linearVelocity.x, -maxSpeed, maxSpeed);
        //    linearVelocity.z = Mathf.Clamp(linearVelocity.z, -maxSpeed, maxSpeed);
        //    rb.linearVelocity = linearVelocity;
        //}
    }

    void OnMove(InputValue input) {
        var inputDir = input.Get<Vector2>();
        moveDir = new Vector3(inputDir.x, 0, inputDir.y).normalized;
    }

    void OnTest()
    {
        InventoryManager.instance.AddItem(otherItem);
    }

    void OnFish()
    {

        if (!isFishing && !PopUpAnim.GetCurrentAnimatorStateInfo(0).IsName("PopUpOn"))
        {
            isFishing = true;
            followCam.SetActive(false);
            fishingCam.SetActive(true);

            HUDAnim.SetBool("isFishing", isFishing);
            fishingAnim.SetTrigger("Fish");

            FindObjectOfType<MinigameManager>().GenerateSectors();
            StartCoroutine(StartSpinner());
        } else
        {
            if (FindObjectOfType<MinigameManager>().CheckInput())
            {
                isFishing = false;
                

                followCam.SetActive(true);
                fishingCam.SetActive(false);

                fishingAnim.SetTrigger("Catch");
                HUDAnim.SetBool("isFishing", isFishing);

                isCatching = false;
                fishingUIAnim.SetBool("isCatching", isCatching);
                CatchFish();
            }
        }
        
    }

    void CatchFish()
    {
        AudioManager.Instance.Stop("Reel");
        InventoryManager.instance.AddItem(item);
        popUpItem.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = item.name;
        popUpItem.transform.GetChild(1).GetComponent<Image>().sprite = item.image;
        PopUpAnim.SetTrigger("popUp");
        AudioManager.Instance.Play("Collect");
    }

    IEnumerator StartSpinner()
    {
        yield return new WaitForSeconds(1);
        isCatching = true;
        fishingUIAnim.SetBool("isCatching", isCatching);
        AudioManager.Instance.Play("Reel");
    }

    void OnCatch()
    {
        

        //InventoryManager.instance.AddItem(item);

        //if (!isCatching)
        //{
        //    isCatching = true;
        //} else
        //{
        //    isCatching = false;
        //}

        //fishingUIAnim.SetBool("isCatching", isCatching);
    }

    void OnInventory()
    {
        if (!inventory.activeSelf)
        {
            inventory.SetActive(true);
        } else
        {
            inventory.SetActive(false);
        }
        
        InventoryManager.instance.ListItems();
    }
}

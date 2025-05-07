using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class PlayerController : MonoBehaviour
{
    [Header("Speed")]
    public float moveSpeed;
    public float slowSpeed;
    public float turnSpeed;
    //public float maxSpeed;

    float currentSpeed;

    public Vector2 clampHeight;

    [Header("Cameras")]
    public GameObject followCam;
    public GameObject fishingCam;

    bool isFishing = false;
    bool isCatching = false;

    Vector3 moveDir;
    Rigidbody rb;

    [Header("Animations")]
    Animator anim;
    public Animator fishingUIAnim;
    public Animator HUDAnim;

    public Animator PopUpAnim;
    public GameObject popUpItem;

    [Header("Items")]
    //These are test items
    //public Item item;
    //public Item otherItem;
    //public LootTable testTable;
    /* TODO: Rework items to randomize based on fishing holes and location
     * 
     */
    public LootTable basicTable;
    public LootTable temperateTable;
    public LootTable pollutedTable;
    public LootTable bloodSeaTable;
    public LootTable coldTable;
    public LootTable magicTable;


    [Header("UI Objects")]
    public GameObject inventory;

    public GameObject shop;
    public GameObject quests;

    public TextMeshProUGUI shopPopup;

    public bool isShop = false;

    [Header("Casting / Charging")]
    public InputActionReference castAction;

    public float minChargeTime = 0.5f;
    public float maxChargeTime = 3f;
    float castingChargeStartTime = 0;
    bool isCasting = false;
    public bool hasReleased = false;

    public float returnRate = 1f;

    float _chargeLevel;

    public GameObject bob;
    public Transform indicator;
    public float scatterRadius = 0.25f;


    public Animator fishAnim;
    public AnimationCurve fishApproachCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
    public AnimationCurve fishFleeCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
    public float approachDuration = 2f;
    float approachTimer = 0f;
    bool isApproaching = false;
    bool isAvailable = false;
    public float attractDuration = 3f;
    public float fleeRate = 3f;
    float attractTimer = 0f;

    public float fleeThreshold = 2.5f;
    bool isFleeing = false;
    Vector3 bobVelocity;
    Vector3 lastBobPosition;

    void Awake() {
        rb = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        GameManager.instance.CheckSave();
        AudioManager.Instance.Play("Engine");

        inventory = GameObject.Find("Inventory");
        shop = GameObject.Find("ShopInventory");
        quests = GameObject.Find("Quests");

        inventory.SetActive(false);
        shop.SetActive(false);
        quests.SetActive(false);
    }


    void Update()
    {
        if (GameManager.isPaused) return;

        //transform.Rotate(0, moveDir.x * turnSpeed * Time.deltaTime, 0);

        var clampedPos = transform.position;
        clampedPos.y = Mathf.Clamp(clampedPos.y, clampHeight.x, clampHeight.y);
        transform.position = clampedPos;

        //TODO: Blend Audio SFX
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


        //Blend Casting Charge Animation
        if (isCasting)
        {
            float chargeTime = Time.time - castingChargeStartTime;
            float chargeLevel = Mathf.Clamp01((float)chargeTime / maxChargeTime);
            _chargeLevel = chargeLevel;

            anim.SetFloat("ChargeLevel", chargeLevel);

        } 
        //Return to normal if released and not casting
        else if(!isCasting && !hasReleased && _chargeLevel > 0.01f)
        {
            float chargeLevel = _chargeLevel;

            chargeLevel = Mathf.Lerp(chargeLevel, 0, returnRate * Time.deltaTime);

            _chargeLevel = chargeLevel;

            anim.SetFloat("ChargeLevel", chargeLevel);
        } else if (_chargeLevel <= 0.01f)
        {
            _chargeLevel = 0;
            anim.SetFloat("ChargeLevel", _chargeLevel);
        }

        if (!isAvailable)
        {
            if (attractTimer < attractDuration)
            {
                attractTimer += Time.deltaTime;
            } else
            {
                isAvailable = true;
            }
        }

        if (isApproaching)
        {
            if (approachTimer < approachDuration)
            {
                approachTimer += Time.deltaTime;
            }

            
            float t = Mathf.Clamp01(approachTimer / approachDuration);
            fishAnim.SetFloat("Approach", fishApproachCurve.Evaluate(t));
            
            if (fishApproachCurve.Evaluate(t) <= 0)
            {
                isApproaching = false;
                Catching();
            }
        }

        if (isFleeing)
        {
            if (approachTimer > 0)
            {
                approachTimer -= fleeRate*Time.deltaTime;
            } else
            {
                isFleeing = false;
                fishAnim.SetBool("isFleeing", isFleeing);
            }

            float t = Mathf.Clamp01(approachTimer / approachDuration);
            fishAnim.SetFloat("Approach", fishFleeCurve.Evaluate(t));
        }

        if (isShop && !inventory.activeSelf && !shop.activeSelf && !quests.activeSelf)
        {
            shopPopup.text = "Press [E] to Open Shop";
        } else
        {
            shopPopup.text = "";
        }

    }

    private void FixedUpdate()
    {
        if (GameManager.isPaused) return;

        rb.MoveRotation(rb.rotation * Quaternion.Euler(0, moveDir.x * turnSpeed * Time.fixedDeltaTime, 0));

        Vector3 currentBobPosition = bob.transform.position;
        bobVelocity = (currentBobPosition - lastBobPosition) / Time.fixedDeltaTime;
        if (!isFleeing || isApproaching) lastBobPosition = currentBobPosition;

        rb.AddForce(moveDir.z * (transform.forward) * currentSpeed, ForceMode.Acceleration);

        if (hasReleased)
        {
            FindObjectOfType<AnchorSpring>().Release();

            if (isAvailable && !isApproaching && !isFleeing && !isFishing)
            {
                approachTimer = 0;
                isApproaching = true;
                fishAnim.transform.rotation = Quaternion.EulerAngles(0, Random.Range(0, 360), 0);
            }

            if (isApproaching && bobVelocity.magnitude > fleeThreshold && !isFishing)
            {
                StartCoroutine(Flee());
            }

            fishAnim.SetBool("isApproaching", isApproaching);
            fishAnim.transform.position = bob.transform.position;
        }

        if (isCasting || hasReleased)
        {
            currentSpeed = slowSpeed;
        } else { 
            currentSpeed = moveSpeed;
            FindObjectOfType<AnchorSpring>().Retract();
            //approachTimer = 0;

            if (isApproaching)
            {
                isApproaching = false;
                fishAnim.SetBool("isApproaching", isApproaching);

                isFleeing = true;
                fishAnim.SetBool("isFleeing", isFleeing);
            }
            

            isAvailable = false;
            attractTimer = 0;
        }
        

        //if (rb.linearVelocity.magnitude > maxSpeed)
        //{
        //    var linearVelocity = rb.linearVelocity;
        //    linearVelocity.x = Mathf.Clamp(linearVelocity.x, -maxSpeed, maxSpeed);
        //    linearVelocity.z = Mathf.Clamp(linearVelocity.z, -maxSpeed, maxSpeed);
        //    rb.linearVelocity = linearVelocity;
        //}
    }

    private void OnDrawGizmos()
    {
        rb = GetComponent<Rigidbody>();
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, rb.transform.forward*5f);
        Gizmos.DrawRay(transform.position, rb.linearVelocity*5f);
    }

    IEnumerator Flee()
    {
        isFleeing = true;
        fishAnim.SetBool("isFleeing", isFleeing);

        Bait currentBait = FindObjectOfType<InventoryManager>().currentEquippedBait;

        if (Random.value < 0.2f && currentBait.amount > 0)
        {
            currentBait.amount--;
        }

        yield return new WaitForSeconds(0.1f);
        
        isApproaching = false;
        fishAnim.SetBool("isApproaching", isApproaching);

        yield return new WaitForSeconds(approachDuration/ fleeRate);
        isAvailable = false;
        attractTimer = 0;

        

    }

    void OnMove(InputValue input) {
        if (GameManager.isPaused) return;

        var inputDir = input.Get<Vector2>();
        moveDir = new Vector3(inputDir.x, 0, inputDir.y);
    }

    //void OnTest()
    //{
    //    Debug.Log("Test");
    //    //InventoryManager.instance.AddItem(testTable.GetRandomItem());
    //    int seed = Random.Range(0, 100000);
    //    PlayerPrefs.SetInt("WorldSeed", seed);

    //    var noises = FindObjectsOfType<NoiseDensity>();
    //    var meshGens = FindObjectsOfType<MeshGenerator>();
    //    foreach (var noise in noises)
    //    {
    //        Debug.Log("Found noises");
    //        noise.seed = PlayerPrefs.GetInt("WorldSeed");
    //    }

    //    foreach (var mesh in meshGens)
    //    {
    //        mesh.Run();
    //    }
    //}


    //On Casting Rod
    private void OnEnable()
    {
        castAction.action.started += OnCastStarted;
        castAction.action.canceled += OnCastCanceled;
    }

    private void OnDisable()
    {
        castAction.action.started -= OnCastStarted;
        castAction.action.canceled -= OnCastCanceled;
    }

    private void OnCastStarted(InputAction.CallbackContext context)
    {
        if (GameManager.isPaused) return;

        if (FindObjectOfType<InventoryManager>().currentEquippedBait.amount <= 0 && (!inventory.activeSelf && !quests.activeSelf && !shop.activeSelf))
        {
            popUpItem.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = FindObjectOfType<InventoryManager>().currentEquippedBait.name;
            popUpItem.transform.GetChild(1).GetComponent<Image>().sprite = FindObjectOfType<InventoryManager>().currentEquippedBait.image;
            popUpItem.transform.parent.GetChild(1).GetComponent<TextMeshProUGUI>().text = "You're out of bait!";
            PopUpAnim.SetTrigger("popUp");
            //CancelRod();
            return;
        }

        if (inventory.activeSelf || shop.activeSelf || quests.activeSelf)
        {
            return;
        }

        if (!hasReleased && !isFishing)
        {
            castingChargeStartTime = Time.time;
            isCasting = true;
            anim.SetBool("isCasting", isCasting);
        }
        else if (isFishing)
        {
            Catching();
        }
    }

    private void OnCastCanceled(InputAction.CallbackContext context)
    {
        if (GameManager.isPaused) return;

        if (inventory.activeSelf || shop.activeSelf || quests.activeSelf || isFishing) return;
        if (FindObjectOfType<InventoryManager>().currentEquippedBait.amount <= 0)
        {
            CancelRod();
            return;
        }

        if (!hasReleased)
        {
            isCasting = false;
            anim.SetBool("isCasting", isCasting);

            if ((Time.time - castingChargeStartTime) >= minChargeTime)
            {
                ReleaseRod();
            }
            else
            {
                CancelRod();
            }
        }
        else
        {
            CancelRod();
        }
    }

    //private void OnEnable()
    //{
        

    //    castAction.action.started += context =>
    //    {
    //        if (GameManager.isPaused) return;

    //        if (inventory.activeSelf || shop.activeSelf || quests.activeSelf) {
    //            if (FindObjectOfType<InventoryManager>().currentEquippedBait.amount <= 0 && (!inventory.activeSelf && !quests.activeSelf && !shop.activeSelf))
    //            {
    //                popUpItem.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = FindObjectOfType<InventoryManager>().currentEquippedBait.name;
    //                popUpItem.transform.GetChild(1).GetComponent<Image>().sprite = FindObjectOfType<InventoryManager>().currentEquippedBait.image;
    //                popUpItem.transform.parent.GetChild(1).GetComponent<TextMeshProUGUI>().text = "You're out of bait!";
    //                PopUpAnim.SetTrigger("popUp");
    //                return;
    //            }
    //            return;
    //        }
    //        if (!hasReleased && !isFishing)
    //        {
    //            castingChargeStartTime = Time.time;
    //            isCasting = true;
    //            anim.SetBool("isCasting", isCasting);
    //        } else if (isFishing)
    //        {
    //            Catching();
    //        }

            
    //    };

    //    castAction.action.canceled += context =>
    //    {
    //        if (GameManager.isPaused) return;

    //        if (inventory.activeSelf || shop.activeSelf || quests.activeSelf || isFishing || FindObjectOfType<InventoryManager>().currentEquippedBait.amount <= 0) return;

    //        if (!hasReleased)
    //        {
    //            isCasting = false;
    //            anim.SetBool("isCasting", isCasting);

    //            //if charge was enough, release rod
    //            if ((Time.time - castingChargeStartTime) >= minChargeTime)
    //            {
    //                ReleaseRod();
    //            }
    //            else
    //            {
    //                CancelRod();
    //            }

    //        } else
    //        {
    //            CancelRod();
    //        }

    //    };
        
    //}

    //private void OnDisable()
    //{
    //    castAction.action.Reset();
    //}

    void ReleaseRod()
    {

        hasReleased = true;
        anim.SetBool("Released", hasReleased);

    }

    void Catching()
    {
        if (!isFishing && !PopUpAnim.GetCurrentAnimatorStateInfo(0).IsName("PopUpOn"))
        {
            isFishing = true;
            followCam.SetActive(false);
            fishingCam.SetActive(true);

            HUDAnim.SetBool("isFishing", isFishing);
            fishAnim.SetBool("isFishing", isFishing);
            //anim.SetTrigger("Fish");

            FindObjectOfType<MinigameManager>().GenerateSectors();
            StartCoroutine(StartSpinner());
            isApproaching = false;
            isFleeing = false;
        }
        else if(isFishing && FindObjectOfType<MinigameManager>().CheckInput() && !PopUpAnim.GetCurrentAnimatorStateInfo(0).IsName("PopUpOn"))
        {
            isFishing = false;

            followCam.SetActive(true);
            fishingCam.SetActive(false);

            //anim.SetTrigger("Catch");
            HUDAnim.SetBool("isFishing", isFishing);
            fishAnim.SetBool("isFishing", isFishing);


            isCatching = false;
            fishingUIAnim.SetBool("isCatching", isCatching);
            CatchFish();
        }
    }

    void CancelRod()
    {
        hasReleased = false;
        anim.SetBool("Released", hasReleased);
    }

    void CatchFish()
    {

        Item droppedItem = DetermineDrop();
        AudioManager.Instance.Stop("Reel");

        droppedItem.realPrice = (int)(droppedItem.goldValue * Random.Range(droppedItem.sizeRange.x, droppedItem.sizeRange.y));

        FindObjectOfType<InventoryManager>().AddItem(droppedItem);
        QuestManager.instance.IncrementQuests(droppedItem);

        popUpItem.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = droppedItem.itemName;
        popUpItem.transform.GetChild(1).GetComponent<Image>().sprite = droppedItem.image;
        popUpItem.transform.parent.GetChild(1).GetComponent<TextMeshProUGUI>().text = "You caught a..";
        PopUpAnim.SetTrigger("popUp");
        AudioManager.Instance.Play("Collect");
    }

    Item DetermineDrop()
    {
        Biome currentBiome = BiomeManager.instance.currentBiome;

        Bait currentBait = FindObjectOfType<InventoryManager>().currentEquippedBait;
        if (currentBait.amount > 0) currentBait.amount--;

        LootTable currentLootTable = null;

        switch (currentBiome)
        {
            case Biome.TEMPERATE:
                currentLootTable = temperateTable;
                break;
            case Biome.COLD:
                currentLootTable = coldTable;
                break;
            case Biome.POLLUTED:
                currentLootTable = pollutedTable;
                break;
            case Biome.BLOOD_SEA:
                currentLootTable = bloodSeaTable;
                break;
            case Biome.TWILIGHT_OCEAN:
                currentLootTable = magicTable;
                break;
        }

        if (currentBait.effectiveBiomes.Contains(currentBiome))
        {
            LootTable chosenTable = Random.value < 0.1 ? basicTable : currentLootTable;
            return chosenTable.GetRandomItem();
        } else
        {
            LootTable chosenTable = Random.value < 0.6 ? basicTable : currentLootTable;
            return chosenTable.GetRandomItem();
        }
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

    public void OnInventory()
    {
        if (GameManager.isPaused) return;

        if (!shop.activeSelf && !quests.activeSelf)
        {
            if (!inventory.activeSelf)
            {
                inventory.SetActive(true);
            }
            else
            {
                inventory.SetActive(false);
                FindObjectOfType<InventoryManager>().HideToolTip();
            }

            FindObjectOfType<InventoryManager>().ListItems();
        }
        
    }

    void OnInteract()
    {
        if (GameManager.isPaused) return;

        if (isShop && (!inventory.activeSelf && !quests.activeSelf))
        {
            if (!shop.activeSelf)
            {
                shop.SetActive(true);
            }
            else
            {
                shop.SetActive(false);
                FindObjectOfType<InventoryManager>().HideToolTip();
            }

            FindObjectOfType<InventoryManager>().ListShopItems();
        }
        

    }

    public void OnQuest()
    {
        if (GameManager.isPaused) return;

        if (!shop.activeSelf && !inventory.activeSelf)
        {
            if (!quests.activeSelf)
            {
                quests.SetActive(true);
            }
            else
            {
                quests.SetActive(false);
                FindObjectOfType<InventoryManager>().HideToolTip();
            }

            QuestManager.instance.ListQuests();
        }
        
    }
    
    public void OnPause()
    {
        GameManager.instance.PauseGame();
    }
}

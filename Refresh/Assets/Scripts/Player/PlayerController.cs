using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;
using TMPro;

public class PlayerController : MonoBehaviour
{
    [Header("Stats")]
    public float speed;
    public float defaultGravity;
    public float fallingGravity;
    public float coyoteTime;
    public int gravSwitches;
    private float jumpVel;
    public Color leadColor;
    public Color fireColor;
    public Color gravColor;

    [Header("State Info")]
    private float moveInput;
    public bool grounded;
    public bool coyoteTimeActive;
    public bool jumpBufferActive;
    public bool falling;
    public bool jumping; //active after jump begins until landed
    public bool jumpPressed;
    public int itemHeld; //0 = none, 1 = lead, 2 = fire, 3 = gravity, 4 = block
    public bool facingRight = true;
    private bool jumpTween;
    public float fallDistance = 0;
    private float initialFallY = 0;

    public bool overItem;
    public bool gravityCrystal;
    public bool lead;
    public bool fire;
    public bool block;
    public bool inCauldron;

    [Header("References")]
    private Controls controls;
    [HideInInspector] public Rigidbody2D rb;
    public GroundTrigger groundTrigger;
    [HideInInspector] public Tween myTween;
    public GameController gameController;
    private Transform fireLight;
    private TextMeshPro textPrompt;
    public Transform itemHeldTransform;
    private Transform itemOverTransform; //item you were last over or are over
    public GameObject blockPrefab;
    private LocalStorageManager localStorageManager;
    [HideInInspector] public SpriteRenderer sprite;
    [HideInInspector] public Animator animator;
    public GameObject audioPrefab;
    public TextMeshPro gravCounterText;
    public ParticleSystem cauldronBurst;

    [Header("Audio")]
    public AudioClip croak;
    public AudioClip jump;
    public AudioClip jumpLayer;
    public AudioClip gravSwitch;
    public AudioClip land;
    public AudioClip heavyLand;
    public AudioClip item;
    public AudioClip cauldron1;
    public AudioClip cauldron2;
    public AudioClip blockPlace;
    public AudioClip gravBreak;
    public AudioClip fireBreak;


    #region Initialization

    private void Awake()
    {
        //Initializations
        controls = new Controls();
        rb = GetComponent<Rigidbody2D>();
        fireLight = transform.Find("Fire Light");
        textPrompt = transform.Find("Text Prompt").GetComponent<TextMeshPro>();
        localStorageManager = gameController.gameObject.GetComponent<LocalStorageManager>();
        sprite = transform.Find("Sprite").GetComponent<SpriteRenderer>();
        animator = transform.Find("Sprite").GetComponent<Animator>();
        gravCounterText.text = gravSwitches.ToString();
    }

    private void OnEnable()
    {
        //Set and enable controls
        controls.Player.Enable();
        controls.Player.Movement.performed += OnMove;
        controls.Player.Movement.canceled += OnMove;
        controls.Player.Jump.performed += OnJump;
        controls.Player.Jump.canceled += OnJumpRelease;
        controls.Player.Softlock.performed += Softlock;
        controls.Player.Interact.performed += Item;
    }

    private void OnDisable()
    {
        //Disable controls when player is inactive
        controls.Player.Movement.performed -= OnMove;
        controls.Player.Movement.canceled -= OnMove;
        controls.Player.Jump.performed -= OnJump;
        controls.Player.Jump.canceled -= OnJumpRelease;
        controls.Player.Interact.performed -= Item;
        controls.Player.Softlock.performed -= Softlock;
        controls.Player.Disable();
    }

    #endregion

    #region Update

    //Handles physics updates
    private void FixedUpdate()
    {
        // horizontal movement
        rb.AddForce(new Vector2(moveInput * speed, 0));

        //jump physics
        if (jumpTween)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpVel);
        }

        //fall check
        if (gravityCrystal || rb.velocity.y < -0.1f)
        {
            falling = true;
            if (!lead)
                rb.gravityScale = fallingGravity;
            else
                rb.gravityScale = fallingGravity * 2;
        }
        else
        {
            falling = false;
            rb.gravityScale = defaultGravity;
        }
    }

    private void Update()
    {
        jumpPressed = controls.Player.Jump.IsPressed();

        //animator
        animator.SetBool("grounded", grounded);
        animator.SetBool("moving", moveInput != 0);

        //jump cut
        if (myTween != null && myTween.active && !jumpPressed && !falling)
            myTween.Kill();

        //fire light
        if (fire)
            fireLight.gameObject.SetActive(true);
        else
            fireLight.gameObject.SetActive(false);

        //fall distance
        if (falling && !grounded)
        {
            // If the player starts falling, store the initial Y position
            if (initialFallY == 0)
            {
                fallDistance = 0;
                initialFallY = transform.position.y;
            }
            else
            {
                float currentY = transform.position.y;
                fallDistance = initialFallY - currentY;
            }
        }
        else
            initialFallY = 0; // Reset initial fall position for the next fall
    }

    #endregion

    #region Controls

    //Called by script to force the held item to be placed
    public void ForcePlaceItem()
    {
        if (itemHeld != 0)
        {
            itemHeld = 0;
            itemHeldTransform.parent = null;
            itemHeldTransform.gameObject.SetActive(true);
            itemHeldTransform = null;
            fire = false;
            lead = false;
            gravityCrystal = false;
            block = false;
            Physics2D.gravity = new Vector2(0, -9.81f);
            sprite.flipY = false;
            sprite.transform.localPosition = new Vector3(0, 0.36f, 0);
        }
    }

    //Called when item input is triggered, handles placing and picking up items
    private void Item(InputAction.CallbackContext context)
    {
        if (!block && !overItem && itemHeld != 0 && grounded && !inCauldron)
        {
            //place regular item
            itemHeld = 0;
            itemHeldTransform.parent = null;
            itemHeldTransform.gameObject.SetActive(true);
            itemHeldTransform = null;
            fire = false;
            lead = false;
            gravityCrystal = false;
            block = false;
            Physics2D.gravity = new Vector2(0, -9.81f);
            sprite.flipY = false;
            sprite.transform.localPosition = new Vector3(0, 0.36f, 0);
            PlayAudio(item, 1, 1);
        }
        else if (itemHeld == 0 && grounded && overItem)
        {
            //pick up item
            itemHeldTransform = itemOverTransform;
            itemHeldTransform.parent = transform;
            itemHeldTransform.localPosition = new Vector3(0, 0, 0);
            itemHeldTransform.gameObject.SetActive(false);
            PlayAudio(item, 1, 1);
            myTween.Kill();
            rb.velocity = new Vector2(rb.velocity.x, 0);

            //set held item
            string itemName = itemHeldTransform.gameObject.name;
            switch (itemName)
            {
                case "Lead":
                    itemHeld = 1;
                    lead = true;
                    break;
                case "Fire":
                    itemHeld = 2;
                    fire = true;
                    break;
                case "Gravity Crystal":
                    itemHeld = 3;
                    gravityCrystal = true;
                    break;
                case "Block":
                    itemHeld = 4;
                    block = true;
                    break;
            }
        }
        else if (block && grounded)
        {
            //place the block item
            Vector3 spawnPos = facingRight ? new Vector3(transform.position.x + 1, transform.position.y, 0) : new Vector3(transform.position.x - 1, transform.position.y, 0);
            Instantiate(blockPrefab, spawnPos, Quaternion.identity, null);
            Destroy(itemHeldTransform.gameObject);
            PlayAudio(blockPlace, 1, 1);
            itemHeld = 0;
            block = false;
        }
        else if (inCauldron && itemHeld != 0 && !overItem && grounded)
        {
            //place item in cauldron
            itemHeldTransform.parent = null;
            itemHeldTransform.position = new Vector3(100, 100, 0);
            itemHeldTransform.gameObject.SetActive(true);

            itemHeldTransform = null;
            PlayAudio(cauldron1, 0.75f, 1);
            PlayAudio(cauldron2, 0.75f, 1);

            fire = false;
            gravityCrystal = false;
            lead = false;

            //cauldron effects
            switch (itemHeld)
            {
                case 1:
                    gameController.leadPlaced = true;
                    GameObject.Find("C Lead").GetComponent<SpriteRenderer>().enabled = true;
                    cauldronBurst.startColor = leadColor;
                    cauldronBurst.Play();
                    break;
                case 2:
                    gameController.firePlaced = true;
                    GameObject.Find("C Fire").GetComponent<SpriteRenderer>().enabled = true;
                    cauldronBurst.startColor = fireColor;
                    cauldronBurst.Play();
                    break;
                case 3:
                    gameController.crystalPlaced = true;
                    GameObject.Find("Crystal").GetComponent<SpriteRenderer>().enabled = true;
                    cauldronBurst.startColor = gravColor;
                    cauldronBurst.Play();
                    break;
            }

            itemHeld = 0;
        }
    }

    //Read movement value
    private void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<float>();

        if (moveInput > 0)
        {
            facingRight = true;
            sprite.flipX = false;
        }
        else if (moveInput < 0)
        {
            facingRight = false;
            sprite.flipX = true;
        }
    }

    //Called when jump button is pressed
    private void OnJump(InputAction.CallbackContext context)
    {
        if (!gravityCrystal)
        {
            if (grounded || coyoteTimeActive)
            {
                Jump();
            }
            else
            {
                //jump buffer
                jumpBufferActive = true;
            }
        }
        else if (grounded)
        {
            //gravity switch
            if (Physics2D.gravity.y < 0)
            {
                //reverse
                Physics2D.gravity = new Vector2(0, 9.81f);
                sprite.flipY = true;
                sprite.transform.localPosition = new Vector3(0, -0.36f, 0);
            }
            else
            {
                //normal
                Physics2D.gravity = new Vector2(0, -9.81f);
                sprite.flipY = false;
                sprite.transform.localPosition = new Vector3(0, 0.36f, 0);
            }

            gravSwitches -= 1;
            gravCounterText.text = gravSwitches.ToString();

            if (gravSwitches == -1)
            {
                //destroy crystal item after max uses reached
                Destroy(itemHeldTransform.gameObject);
                itemHeld = 0;
                gravityCrystal = false;
                Physics2D.gravity = new Vector2(0, -9.81f);
                sprite.flipY = false;
                sprite.transform.localPosition = new Vector3(0, 0.36f, 0);
                PlayAudio(gravBreak, 1, 1);
            }
            else
                PlayAudio(gravSwitch, 1, 1);
        }
    }

    //Jump tween and fx
    public void Jump()
    {
        fallDistance = 0;
        jumpVel = 30;
        float jumpTime = 0.575f;

        if (Random.Range(0, 2) == 1)
            PlayAudio(croak, 1, Random.Range(0.5f, 1.5f));
        PlayAudio(jump, 0.5f, 1.5f);
        PlayAudio(jumpLayer, 0.6f, 1);

        if (lead)
            jumpTime = 0.28f;

        jumping = true;
        jumpBufferActive = false;
        StopCoroutine(groundTrigger.CoyoteTime(0));
        coyoteTimeActive = false;

        myTween = DOTween.To(() => jumpVel, x => jumpVel = x, -1, jumpTime)
            .OnPlay(() => jumpTween = true)
            .SetEase(Ease.OutQuad)
            .OnComplete(() => jumpTween = false)
            .OnKill(() => jumpTween = false);
    }

    //Called when jump button is released
    private void OnJumpRelease(InputAction.CallbackContext context)
    {
        jumpBufferActive = false;
    }

    //Called when softlock is triggered, unsoftlocks the player if necessary
    public void Softlock(InputAction.CallbackContext context)
    {
        ForcePlaceItem();
        transform.position = new Vector3(0, 0, 0);

        gameController.crystalPlaced = false;
        gameController.firePlaced = false;
        gameController.leadPlaced = false;
        GameObject.Find("C Fire").GetComponent<SpriteRenderer>().enabled = false;
        GameObject.Find("C Lead").GetComponent<SpriteRenderer>().enabled = false;
        GameObject.Find("Crystal").GetComponent<SpriteRenderer>().enabled = false;
        GameObject.Find("Fire").transform.position = new Vector3(100, 100, 0);
        GameObject.Find("Lead").transform.position = new Vector3(100, 100, 0);
        GameObject.Find("Gravity Crystal").transform.position = new Vector3(100, 100, 0);
        GameObject.Find("Block").transform.position = new Vector3(100, 100, 0);
    }

    #endregion

    #region Collision

    private void OnTriggerStay2D(Collider2D collision)
    {
        //in shadow
        if (collision.CompareTag("Shadow"))
        {
            gameController.inShadow = true;
            localStorageManager.SetValue("gravitycrystal", BoolToInt(gravityCrystal).ToString());
            localStorageManager.SetValue("lead", BoolToInt(lead).ToString());
            localStorageManager.SetValue("block", BoolToInt(block).ToString());
        }

        if (collision.CompareTag("Item") && grounded && itemHeld == 0)
        {
            textPrompt.text = "Pick Up: (Down)";
            itemOverTransform = collision.transform;
            overItem = true;
        }

        if (collision.CompareTag("Cauldron") && grounded && itemHeld > 0 && itemHeld != 4)
            textPrompt.text = "Deposit: (Down)";
        else if (collision.CompareTag("Cauldron") && itemHeld == 0)
            textPrompt.text = "";
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Shadow"))
            gameController.inShadow = false;

        if (collision.CompareTag("Item") || collision.CompareTag("Cauldron"))
        {
            textPrompt.text = "";
            overItem = false;
        }
    }

    #endregion

    #region Utility

    public int BoolToInt(bool value)
    {
        if (value)
            return 1;
        else
            return 0;
    }

    public void PlayAudio(AudioClip clip, float volume, float pitch)
    {
        GameObject audioObject = Instantiate(audioPrefab);
        AudioSource audio = audioObject.GetComponent<AudioSource>();
        audio.volume = volume;
        audio.clip = clip;
        audio.pitch = pitch;
        audio.Play();
    }

    public void CameraShake()
    {
        float value = 5;
        DOTween.To(() => value, x => value = x, 4.9f, 0.1f)
            .OnUpdate(() => Camera.main.orthographicSize = value);
        DOTween.To(() => value, x => value = x, 5, 0.1f)
            .SetDelay(0.1f)
            .OnUpdate(() => Camera.main.orthographicSize = value)
            .OnComplete(() => Camera.main.orthographicSize = 5);
    }

    public void LoseControl()
    {
        controls.Player.Movement.performed -= OnMove;
        controls.Player.Movement.canceled -= OnMove;
        controls.Player.Jump.performed -= OnJump;
        controls.Player.Jump.canceled -= OnJumpRelease;
        controls.Player.Interact.performed -= Item;
        controls.Player.Softlock.performed -= Softlock;
        moveInput = 0;
        rb.velocity = new Vector2(0, 0);
    }
}

#endregion

using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using DG.Tweening;

public class GameController : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    private PlayerController playerController;
    private GameObject fireItem;
    private SpriteRenderer BG;
    private Color originalColor;
    public Color glitchColor;
    public Transform movingPlatform;
    private LocalStorageManager localStorageManager;
    public Sprite[] itemSprites = new Sprite[4];
    private Volume postProcessingVolume;
    private FilmGrain filmGrain;
    private ChromaticAberration chromaticAberration;
    private AudioSource audio;
    public GameObject audioPrefab;
    public AudioSource music;
    public AudioClip frogLaugh;
    public TextMeshPro glitchClock;
    private int glitchSeconds;
    public CanvasGroup winScreen;

    [Header("State Info")]
    public bool firePlaced;
    public bool crystalPlaced;
    public bool leadPlaced;
    [HideInInspector] public bool platformActive = false;
    [HideInInspector] public bool platformDone = false;
    [HideInInspector] public bool glitchPeriod;
    [HideInInspector] public bool inShadow;

    [Header("Display Hint")]
    public Transform locationDisplay;
    private TextMeshPro xPos;
    private TextMeshPro yPos;
    private SpriteRenderer itemDisplay;

    #region Initialization

    private void Awake()
    {
        playerController = player.GetComponent<PlayerController>();
        xPos = locationDisplay.Find("X Pos").GetComponent<TextMeshPro>();
        yPos = locationDisplay.Find("Y Pos").GetComponent<TextMeshPro>();
        fireItem = GameObject.Find("Fire");
        BG = GameObject.Find("BG").GetComponent<SpriteRenderer>();
        originalColor = BG.color;
        localStorageManager = gameObject.GetComponent<LocalStorageManager>();
        itemDisplay = GameObject.Find("Item Pic").GetComponent<SpriteRenderer>();
        postProcessingVolume = postProcessingVolume = GetComponent<Volume>();
        audio = GetComponent<AudioSource>();

        if (postProcessingVolume.profile.TryGet(out FilmGrain effect1))
        {
            filmGrain = effect1;
        }

        if (postProcessingVolume.profile.TryGet(out ChromaticAberration effect2))
        {
            chromaticAberration = effect2;
        }
    }

    void Start()
    {
        glitchSeconds = Random.Range(10, 60);
        StartCoroutine(GlitchTimer(glitchSeconds));
        StartCoroutine(GlitchClock(1));
        player.position = new Vector3(float.Parse(localStorageManager.GetValue("posx")), float.Parse(localStorageManager.GetValue("posy")), 0);
        playerController.gravityCrystal = IntToBool(int.Parse(localStorageManager.GetValue("gravitycrystal")));
        playerController.lead = IntToBool(int.Parse(localStorageManager.GetValue("lead")));
        playerController.block = IntToBool(int.Parse(localStorageManager.GetValue("block")));
        localStorageManager.SetValue("platformpos", "-13.5");

        GameObject item;

        if (playerController.lead)
        {
            item = GameObject.Find("Lead");
            InitPlayerHand(item, 1);
        }
        else if (playerController.fire)
        {
            item = GameObject.Find("Fire");
            InitPlayerHand(item, 2);
        }
        else if (playerController.gravityCrystal)
        {
            item = GameObject.Find("Gravity Crystal");
            InitPlayerHand(item, 3);
        }
        else if (playerController.block)
        {
            item = GameObject.Find("Block");
            InitPlayerHand(item, 4);
        }

        localStorageManager.SetValue("gravitycrystal", "0");
        localStorageManager.SetValue("lead", "0");
        localStorageManager.SetValue("block", "0");
    }

    private void InitPlayerHand(GameObject item, int ID)
    {
        playerController.itemHeldTransform = item.transform;
        playerController.itemHeld = ID;
        item.transform.parent = player;
        item.transform.localPosition = new Vector3(0, 0, 0);
        item.gameObject.SetActive(false);
    }

    #endregion

    #region Update

    void Update()
    {
        UpdateLocationDisplay();

        //check win condition
        if (leadPlaced && crystalPlaced && firePlaced)
        {
            StartCoroutine(WinSequence());
            firePlaced = false;
        }

        if (!platformDone)
        UpdatePlatform();
        
        //in light
        if (!inShadow || playerController.fire || Vector3.Distance(player.position, fireItem.transform.position) <= 4.5)
        {
            localStorageManager.SetValue("posx", player.position.x.ToString());
            localStorageManager.SetValue("posy", player.position.y.ToString());
            localStorageManager.SetValue("gravitycrystal", "0");
            localStorageManager.SetValue("lead", "0");
            localStorageManager.SetValue("block", "0");
        }
    }

    //Helper method to handle location display
    private void UpdateLocationDisplay()
    {
        xPos.text = float.Parse(localStorageManager.GetValue("posx")).ToString("F2");
        yPos.text = float.Parse(localStorageManager.GetValue("posy")).ToString("F2");

        if (!inShadow || playerController.fire || Vector3.Distance(player.position, fireItem.transform.position) <= 4.5)
        {
            itemDisplay.sprite = null;
        }
        else
        {
            itemDisplay.sprite = itemSprites[playerController.itemHeld];
        }
    }

    //Helper method to control the moving platform
    private void UpdatePlatform()
    {
        //potentially move if not at end pos
        if (movingPlatform.position.y < 3.5f)
        {
            //move if not in original pos
            if (movingPlatform.position.y > -13.5f || platformActive)
            {
                movingPlatform.GetComponent<AudioSource>().volume = 0.25f;
                platformActive = true;
                movingPlatform.Translate(new Vector3(0, 2, 0) * 1 * Time.deltaTime);

                localStorageManager.SetValue("platformpos", movingPlatform.position.y.ToString());
            }
        }
        else
        {
            movingPlatform.GetComponent<AudioSource>().volume = 0;
            platformDone = true;
        }

        //update pos between tabs which also triggers movement
        movingPlatform.position = new Vector3(20, float.Parse(localStorageManager.GetValue("platformpos")), 0);
    }

    #endregion

    #region Game State

    //glitch trigger
    public IEnumerator GlitchTimer(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        StartCoroutine(GlitchActive(2.5f));
    }

    //glitch effect coroutine
    public IEnumerator GlitchActive(float waitTime)
    {
        glitchPeriod = true;

        float value = 0;
        DOTween.To(() => value, x => value = x, 1, 1)
            .OnUpdate(() => GlitchFX(value));

        yield return new WaitForSeconds(waitTime);

        glitchPeriod = false;

        DOTween.To(() => value, x => value = x, 0, 1)
            .OnUpdate(() => GlitchFX(value));
    }

    //glitch visual counter
    public IEnumerator GlitchClock(float waitTime)
    {
        glitchClock.text = glitchSeconds.ToString();
        glitchSeconds -= 1;
        yield return new WaitForSeconds(waitTime);
        StartCoroutine(GlitchClock(1));
    }

    private void GlitchFX(float value)
    {
        audio.volume = value;
        chromaticAberration.intensity.value = value;
        filmGrain.intensity.value = value;
    }

    private IEnumerator WinSequence()
    {
        playerController.LoseControl();
        music.DOFade(0, 3);
        yield return new WaitForSeconds(3);

        PlayAudio(frogLaugh, 0.5f, 1);
        playerController.rb.bodyType = RigidbodyType2D.Kinematic;

        float value1 = player.position.y;
        float value2 = playerController.facingRight ? 45 : -45;
        float value3 = playerController.facingRight ? 110 : -110;

        DOTween.To(() => value1, x => value1 = x, player.position.y + 3, 3)
            .OnUpdate(() => player.position = new Vector3(player.position.x, value1, 0))
            .SetEase(Ease.OutQuad);

        DOTween.To(() => value2, x => value2 = x, value3, 6)
            .OnUpdate(() => player.Rotate(new Vector3(0, 0, value2) * Time.deltaTime))
            .SetEase(Ease.InQuad);

        yield return new WaitForSeconds(3);

        playerController.sprite.sortingLayerName = "BG";
        playerController.sprite.sortingOrder = 3;

        value1 = player.position.y;
        value2 = 0;

        DOTween.To(() => value1, x => value1 = x, player.position.y - 2, 2)
            .OnUpdate(() => player.position = new Vector3(player.position.x, value1, 0))
            .SetEase(Ease.InSine);

        DOTween.To(() => value2, x => value2 = x, 1, 2)
            .SetDelay(5)
            .OnUpdate(() => winScreen.alpha = value2)
            .SetEase(Ease.InSine);
    }

    //trigger moving platform
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && movingPlatform.position.y <= -13.5f)
        {
            movingPlatform.position = new Vector3(20, -13.49f, 0);
            localStorageManager.SetValue("platformpos", "-13.49");
        }
    }
    #endregion


    #region Utility

    public bool IntToBool(int value)
    {
        if (value == 0)
            return false;
        else
            return true;
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

    #endregion
}
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class EyeMovementAI : MonoBehaviour
{
    public const bool HEAVYFOOTSTEPS = true;

    // The ai level of the entity
    [Header("Basic ai")]
    public int aILevel;
    public float agroTimer;
    public float timeInOfficeTimer;
    public int timeUntilJumpscare;
    public bool inOffice;
    [Header("Advanced ai")]
    public int startingAILevel;
    public int forceAppearAtPower;
    public bool forceAppearReached = false;
    public int updateAIAtPowerLevelA;
    public int aiAtPowerLevelA;
    public bool powerLevelAReached = false;
    public int updateAIAtPowerLevelB;
    public int aiAtPowerLevelB;
    public bool powerLevelBReached = false;
    public int updateAIAtPowerLevelC;
    public int aiAtPowerLevelC;
    public bool powerLevelCReached = false;
    public int updateAIAtPowerLevelD;
    public int aiAtPowerLevelD;
    public bool powerLevelDReached = false;
    [Header("Movement animation")]
    public float lowPoint;
    public float endPoint;
    private float eyeX;
    private float eyeZ;
    private float startPoint;
    public bool movementAnimationActive;
    public bool exitAnimationActive;
    public float movementAnimationTimer;
    public float leaveOfficeSpeed = 10f;
    public Vector3 startPosition;
    [Header("Debug text ui")]
    public TextMeshProUGUI agroTimerText;
    public TextMeshProUGUI currentAILevelText;
    public TextMeshProUGUI inOfficeText;
    [Header("Sprite")]
    public GameObject entitySprite;
    [Header("Path split chances")]

    public MainMenu mainMenu;
    public TabletController tablet;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        aILevel = startingAILevel;
        agroTimer = 0;
        movementAnimationTimer = 0;
        inOffice = false;
        movementAnimationActive = false;
        exitAnimationActive = false;
        GameObject entitySprite = GameObject.Find("Eyesore");
        startPoint = GetYFromEquation(endPoint, lowPoint, 0);
        eyeX = entitySprite.transform.position.x;
        eyeZ = entitySprite.transform.position.z;
        startPosition = new Vector3(eyeX, startPoint, eyeZ);
    }

    // Update is called once per frame
    void Update()
    {
        var audioPlayer = FindAnyObjectByType<AudioController>();
        if (!inOffice && !movementAnimationActive && !exitAnimationActive)
        {
            agroTimer += Time.deltaTime;
        }
        else
        {
            agroTimer = 0;
        }
        if (inOffice)
        {
            timeInOfficeTimer += Time.deltaTime;
        }
        else
        {
            timeInOfficeTimer = 0;
        }
        if (movementAnimationActive == true)
        {
            PlayDropAnimation();
        }
        if (timeInOfficeTimer >= 5)
        {
            Debug.Log("JUMPSCARE 1");
            StartCoroutine(audioPlayer.Jumpscare("Eyesore"));
        }
        CheckForActive();
        CheckForAIUpdate();
        UpdateDebugInfo();
        PlayLeaveAnimation();
    }

    void CheckForActive()
    {
        var tablet = FindAnyObjectByType<TabletController>();
        if (aILevel == 0)
        {
            entitySprite.transform.position = startPosition;
        }
        if (agroTimer >= (26 - aILevel))
        {
            agroTimer = 0;
            movementAnimationActive = true;
        }
        if (tablet.tabletUp == false && inOffice)
        {
            exitAnimationActive = true;
            inOffice = false;
        }
    }
    // Plays the leave animation is Eyesore should be leaving
    void PlayLeaveAnimation()
    {
        if (exitAnimationActive)
        {
            entitySprite.transform.position = Vector3.MoveTowards(entitySprite.transform.position, startPosition, leaveOfficeSpeed * Time.deltaTime);
        }
        if (entitySprite.transform.position == startPosition)
        {
            exitAnimationActive = false;
        }
    }
    void PlayDropAnimation()
    {
        float pointDifference = startPoint - endPoint;
        float eyeYAxis;
        movementAnimationTimer += Time.deltaTime;
        eyeYAxis = GetYFromEquation(endPoint, lowPoint, movementAnimationTimer);
        entitySprite.transform.position = new Vector3(eyeX, eyeYAxis, eyeZ);
        if (movementAnimationTimer >= 1.25f)
        {
            movementAnimationActive = false;
            inOffice = true;
            movementAnimationTimer = 0;
        }
    }

    private float GetYFromEquation(float endY, float lowY, float x)
    {
        float outY = ((endY - lowY) * ((x - 1) * (x - 1)) / (0.25f * 0.25f)) + lowY;
        return outY;
    }

    void CheckForAIUpdate()
    {
        var battery = FindAnyObjectByType<BatteryDisplay>();
        if (battery.batteryAmount >= forceAppearAtPower && !forceAppearReached)
        {
            agroTimer = (26 - aILevel);
            forceAppearReached = true;
        }
        else if(battery.batteryAmount >= updateAIAtPowerLevelA && !powerLevelAReached)
        {
            aILevel = aiAtPowerLevelA;
            powerLevelAReached = true;
        }
        else if (battery.batteryAmount >= updateAIAtPowerLevelB && !powerLevelBReached)
        {
            aILevel = aiAtPowerLevelB;
            powerLevelBReached = true;
        }
        else if (battery.batteryAmount >= updateAIAtPowerLevelC && !powerLevelCReached)
        {
            aILevel = aiAtPowerLevelC;
            powerLevelCReached = true;
        }
        else if (battery.batteryAmount >= updateAIAtPowerLevelD && !powerLevelDReached)
        {
            aILevel = aiAtPowerLevelD;
            powerLevelDReached = true;
        }
    }

    void UpdateDebugInfo()
    {
        agroTimerText.text = agroTimer.ToString(format: "0.0000");
        currentAILevelText.text = aILevel.ToString();
        inOfficeText.text = inOffice.ToString();
    }
}

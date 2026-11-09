using UnityEngine;
using TMPro;

public class MalMovementAI : MonoBehaviour
{
    public const bool HEAVYFOOTSTEPS = false;

    // The ai level of the entity
    [Header("Basic ai")]
    public int aILevel;
    public float movementOpportunityTimerLength = 4.97f;
    // The movement opportunity timer
    private float movementOpportunityTimer;
    // If true = start the timer, if false = don't start the timer
    public bool movementOpportunityTimerActive = true;
    // If a movement opportunity was successful or not
    private bool movementOpportunitySuccess;
    public string currentPoint;
    public float jumpscareDrainAmount;
    [Header("Advanced ai")]
    public int startingAILevel;
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
    [Header("Debug text ui")]
    public TextMeshProUGUI movementOpportunityTimerText;
    public TextMeshProUGUI currentAILevelText;
    public TextMeshProUGUI movementOpportunitySuccessText;
    public TextMeshProUGUI currentPointText;
    [Header("Sprite")]
    public GameObject entitySprite;
    [Header("Path split chances")]
    // Chance to go from point 1 directly to point 3
    public int skipToPoint3Chance;
    // Chance to go from point 4 to point 4s
    public int goToPoint4sChance;
    // Chance to go from point 4s to point 4
    public int returnToPoint4Chance;

    public DoorController doorController;
    public MainMenu mainMenu;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        aILevel = startingAILevel;
        movementOpportunityTimer = movementOpportunityTimerLength;
        currentPoint = "1";
        GameObject entitySprite = GameObject.Find("Rampage");
    }

    // Update is called once per frame
    void Update()
    {
        var battery = FindAnyObjectByType<BatteryDisplay>();
        if (aILevel <= 0 || battery.batteryAmount <= 0)
        {
            currentPoint = "0";
            updatePosition(currentPoint);
            Debug.Log("0");
        }
        else if (currentPoint == "0")
        {
            currentPoint = "1";
            updatePosition(currentPoint);
            Debug.Log("1");
        }
        // Prevent the timer from happening if movementOpportunityTimerStart = false
        if (!movementOpportunityTimerActive) return;

        if (movementOpportunityTimer > 0)
        {
            movementOpportunityTimer -= Time.deltaTime;
        }
        else
        {
            movementOpportunityTimer = movementOpportunityTimerLength;
            executeMovementOpportunity();
        }
        checkForAIUpdate();
        UpdateDebugInfo();
    }
    void checkForAIUpdate()
    {
        var battery = FindAnyObjectByType<BatteryDisplay>();
        if (battery.batteryAmount >= updateAIAtPowerLevelA && !powerLevelAReached)
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
        movementOpportunityTimerText.text = movementOpportunityTimer.ToString(format: "0.0000");
        currentAILevelText.text = aILevel.ToString();
        movementOpportunitySuccessText.text = movementOpportunitySuccess.ToString();
        currentPointText.text = currentPoint.ToString();
    }

    void executeMovementOpportunity()
    {
        int low = 1;
        int high = 20;
        // Random number generated between 1 and 20
        int randomAINum = UnityEngine.Random.Range(low, high + 1);

        // If entity is outside of door and the door is closed on a movement opportunity, auto block
        if (currentPoint == "Final" && doorController.doorOpen == false)
        {
            blockedByDoor();
        }
        else if (randomAINum <= aILevel)
        {
            movementOpportunitySuccess = true;
            successfulMovementOpportunity();
        }
        else
        {
            movementOpportunitySuccess = false;
            updatePosition(currentPoint);
        }
    }

    void successfulMovementOpportunity()
    {
        var mainMenu = FindAnyObjectByType<MainMenu>();
        // Random number for multible possible directions
        int randomPercent = UnityEngine.Random.Range(1, 101);
        if (currentPoint == "1")
        {
            // The chance to go down one of the halls in point 1
            int point1HallSplitChance = (100 - skipToPoint3Chance) / 2;
            if (randomPercent <= skipToPoint3Chance)
            {
                currentPoint = "3";
            }
            else if (randomPercent <= skipToPoint3Chance + point1HallSplitChance)
            {
                currentPoint = "2A";
            }
            else
            {
                currentPoint = "2B";
            }
        }
        else if (currentPoint == "2A" || currentPoint == "2B")
        {
            currentPoint = "3";
        }
        else if (currentPoint == "3")
        {
            currentPoint = "4";
        }
        else if (currentPoint == "4")
        {
            if (randomPercent <= goToPoint4sChance)
            {
                currentPoint = "4s";
            }
            else
            {
                currentPoint = "Final";
            }
        }
        else if (currentPoint == "4s")
        {
            if (randomPercent <= returnToPoint4Chance)
            {
                currentPoint = "4";
            }
            else
            {
                currentPoint = "Final";
            }
        }
        else if (currentPoint == "Final")
        {
            if (doorController.doorOpen == false)
            {
                blockedByDoor();
            }
            else
            {
                Debug.Log("Jumpscare");
                jumpscare();
            }
        }
        else
        {
            Debug.LogWarning("Invalid point");
        }
        updatePosition(currentPoint);
    }

    void blockedByDoor()
    {
        var audioPlayer = FindAnyObjectByType<AudioController>();
        audioPlayer.PlayDoorThump();
        currentPoint = "1";
        updatePosition(currentPoint);
    }
    void jumpscare()
    {
        var audioPlayer = FindAnyObjectByType<AudioController>();
        var batteryScript = FindAnyObjectByType<BatteryDisplay>();

        batteryScript.batteryAmount -= jumpscareDrainAmount;
        audioPlayer.PowerDrain();
        currentPoint = "1";
        updatePosition(currentPoint);
    }
    void updatePosition(string currentPoint)
    {
        var audioPlayer = FindAnyObjectByType<AudioController>();
        if (movementOpportunitySuccess)
        {
            audioPlayer.playFootSteps(HEAVYFOOTSTEPS);
        }
        if (currentPoint == "0")
        {
            entitySprite.transform.position = new Vector3(-329.4571f, 15f, 562.9229f);
        }
        else if (currentPoint == "1")
        {
            entitySprite.transform.position = new Vector3(-329.4571f, 15f, 401.6519f);
        }
        else if (currentPoint == "2A")
        {
            entitySprite.transform.position = new Vector3(-515.9302f, 15f, 108.2468f);
        }
        else if (currentPoint == "2B")
        {
            entitySprite.transform.position = new Vector3(-143f, 15f, 108.2468f);
        }
        else if (currentPoint == "3")
        {
            entitySprite.transform.position = new Vector3(79.6f, 13.4f, 298.8f);
        }
        else if (currentPoint == "4")
        {
            entitySprite.transform.position = new Vector3(69.4f, 13.4f, 225.1f);
        }
        else if (currentPoint == "4s")
        {
            entitySprite.transform.position = new Vector3(-242.1016f, 15f, -51.6341f);
        }
        else if (currentPoint == "Final")
        {
            entitySprite.transform.position = new Vector3(67.5f, 13.4f, 64.89f);
        }
    }
}
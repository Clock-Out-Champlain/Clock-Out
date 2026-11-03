using UnityEngine;
using TMPro;

public class ClockMovementAI : MonoBehaviour
{
    public const bool HEAVYFOOTSTEPS = true;
    public Clock clock;
    float clockTimer;
    public int clockHours;
    public int clockMinutes;
    // The ai level of the entity
    public string currentPoint;
    [Header("Debug text ui")]
    public TextMeshProUGUI currentPointText;
    [Header("Sprite")]
    public GameObject entitySprite;

    public DoorController doorController;
    public MainMenu mainMenu;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentPoint = "0";
        GameObject entitySprite = GameObject.Find("Clock'N");
    }

    // Update is called once per frame
    void Update()
    {
        var clock = FindAnyObjectByType<Clock>();
        clockTimer = clock.clockTimer;
        clockHours = clock.clockHours;
        clockMinutes = clock.clockMinutes;
        CheckTime();
        UpdateDebugInfo();
    }
    void UpdateDebugInfo()
    {
        currentPointText.text = currentPoint.ToString();
    }
    void CheckTime()
    {
        var clock = FindAnyObjectByType<Clock>();
        int clockHourEnd = clock.clockHourEnd;
        if (clockHours >= clockHourEnd - 2 && currentPoint == "0")
        {
            UpdatePosition("4s");
            currentPoint = "4s";
        }
        else if (clockHours >= clockHourEnd - 1 && currentPoint == "4s")
        {
            UpdatePosition("Final");
            currentPoint = "Final";
        }
        else if (clockHours == clockHourEnd - 1 && clockMinutes >= 50 && currentPoint == "Final")
        {
            UpdatePosition("Office");
            currentPoint = "Office";
        }
    }
    void UpdatePosition(string currentPoint)
    {
        var audioPlayer = FindAnyObjectByType<AudioController>();
        audioPlayer.playFootSteps(HEAVYFOOTSTEPS);
        if (currentPoint == "0")
        {
            entitySprite.transform.position = new Vector3(-329.4571f, 15f, 562.9229f);
            entitySprite.transform.eulerAngles = new Vector3(0f, 0f, 0f);
        }
        /*else if (currentPoint == "1")
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
            entitySprite.transform.position = new Vector3(66.28302f, 13.4f, 309);
        }
        else if (currentPoint == "4")
        {
            entitySprite.transform.position = new Vector3(66.2f, 13.4f, 202.9f);
        }*/
        else if (currentPoint == "4s")
        {
            entitySprite.transform.position = new Vector3(78.4f, 13.9f, 134.4f);
            entitySprite.transform.eulerAngles = new Vector3(0f, 0f, 0f);
        }
        else if (currentPoint == "Final")
        {
            entitySprite.transform.position = new Vector3(67.8f, 31.85f, 40.43f);
            entitySprite.transform.eulerAngles = new Vector3(180f, 180f, 0f);
        }
        else if (currentPoint == "Office")
        {
            entitySprite.transform.position = new Vector3(67.8f, 38.1f, -9.57f);
            entitySprite.transform.eulerAngles = new Vector3(180f, 180f, 0f);
        }
    }
}

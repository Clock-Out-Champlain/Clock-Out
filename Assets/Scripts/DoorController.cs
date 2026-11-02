using UnityEngine;
using System.Collections;
using TMPro;

public class DoorController : MonoBehaviour
{
    [Header("Door Heat")]
    private float doorHeatTimer = 0;
    public int doorHeat;
    public int doorHeatIncreaseAfterSec;
    public int doorHeatMult;
    [Header("Door Heat Objects")]
    public GameObject heatLow;
    public GameObject heatMid;
    public GameObject heatHigh;
    [Header("Door Power Drain")]
    public bool doorPowerDrainEnabled = true;
    public float doorPowerDain = -0.001f;
    [Header("Misc")]
    public bool doorOpen = true;
    [Header("Door Open/Close")]
    public GameObject door;
    public float closeSpeed = 200f;
    public GameObject openPos;
    public GameObject closePos;
    [Header("Debug text ui")]
    public TextMeshProUGUI heatTimerText;

    private Transform target;
    private bool doorMoving = false;

    void Start()
    {
        target = openPos.transform;
    }

    void Update()
    {
        var batteryScript = FindAnyObjectByType<BatteryDisplay>();
        var audioPlayer = FindAnyObjectByType<AudioController>();
        if (Input.GetKeyDown(KeyCode.W))
        {
            if (doorMoving == false && batteryScript.batteryAmount > batteryScript.batteryMinValue)
            {
                audioPlayer.DoorButtonPress(true);
                if (doorOpen == true)
                {
                    doorOpen = false;
                    target = closePos.transform;
                }
                else
                {
                    doorOpen = true;
                    target = openPos.transform;
                }
                doorMoving = true;
            }
            else
            {
                audioPlayer.DoorButtonPress(false);
            }
        }
        if (doorOpen == false && batteryScript.batteryAmount <= batteryScript.batteryMinValue)
        {
            doorOpen = true;
            target = openPos.transform;
            audioPlayer.DoorButtonPress(true);
            doorMoving = true;
        }

        if (doorMoving == true)
        {
            float step = closeSpeed * Time.deltaTime;
            if (doorOpen == true)
            {
                door.transform.position = Vector3.MoveTowards(door.transform.position, target.position, step);
            }
            else if (doorOpen == false)
            {
                door.transform.position = Vector3.MoveTowards(door.transform.position, target.position, step);
            }
        }
        if (door.transform.position == target.position)
        {
            doorMoving = false;
        }
        // Drain power if door is closed
        if (doorPowerDrainEnabled && doorOpen == false)
        {
            batteryScript.ChangeBattery(doorPowerDain * doorHeat);
        }
        manageDoorHeat();
    }

    void manageDoorHeat()
    {
        // Manage the door heat timer
        if (doorHeatTimer < 0)
        {
            doorHeatTimer = 0;
        }
        else if (doorOpen)
        {
            doorHeatTimer -= Time.deltaTime;
        }
        else
        {
            doorHeatTimer += Time.deltaTime;
        }
        // Manage the door heat multiplier
        if (doorHeatTimer >= doorHeatIncreaseAfterSec * 3)
        {
            doorHeat = 4 * doorHeatMult;
            heatLow.SetActive(true);
            heatMid.SetActive(true);
            heatHigh.SetActive(true);
        }
        else if (doorHeatTimer >= doorHeatIncreaseAfterSec * 2)
        {
            doorHeat = 2 * doorHeatMult;
            heatLow.SetActive(true);
            heatMid.SetActive(true);
            heatHigh.SetActive(false);
        }
        else if (doorHeatTimer >= doorHeatIncreaseAfterSec)
        {
            doorHeat = doorHeatMult;
            heatLow.SetActive(true);
            heatMid.SetActive(false);
            heatHigh.SetActive(false);
        }
        else
        {
            doorHeat = 1;
            heatLow.SetActive(false);
            heatMid.SetActive(false);
            heatHigh.SetActive(false);
        }
        // Manage debug text
        heatTimerText.text = "Heat Timer: " + doorHeatTimer.ToString(format: "0.0000");
    }
}
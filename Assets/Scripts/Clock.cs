using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class Clock : MonoBehaviour
{
    public MainMenu mainMenu;
    public TextMeshPro clockText;
    public float clockTimer;
    public int clockHours;
    public int clockMinutes;
    public int clockHourStart = 6;
    public int clockHourEnd = 12;
    // How many seconds are in an hour
    public int hourLength = 60;
    // In hours
    public int nightLength;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        clockTimer = 0;
        clockHours = clockHourStart;
        clockMinutes = 0;
    }

    // Update is called once per frame
    void Update()
    {
        clockTimer += Time.deltaTime;
        clockHours = (int)(clockTimer / hourLength) + clockHourStart;
        clockMinutes = (int)(clockTimer % hourLength);
        updateClock();
        if (clockHours >= clockHourEnd)
        {
            SceneManager.LoadScene("DeathScreenClock");
        }

    }
    void updateClock()
    {
        clockText.text = clockHours + ":" + clockMinutes.ToString("00");
    }
}

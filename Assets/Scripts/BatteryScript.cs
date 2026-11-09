using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BatteryDisplay : MonoBehaviour
{
    public MainMenu mainMenu;
    public bool passivePowerDrainEnabled = true;
    public float passivePowerDrain = -0.000005f;
    public bool powerOutSoundPlayed = false;
    [Header("Battery Settings")]
    public float batteryAmount;
    public int batteryMinValue = 0;
    public int batteryMaxValue = 100;
    [Header("Battery text fields")]
    public TMP_Text debugBatteryText;
    public TMP_Text homeBatteryText;
    public TMP_Text mathBatteryText;

    private void Start()
    {
        UpdateBatteryDisplay();
    }

    private void Update()
    {
        var audioPlayer = FindAnyObjectByType<AudioController>();
        var mainMenu = FindAnyObjectByType<MainMenu>();
        if (passivePowerDrainEnabled)
        {
            ChangeBattery(passivePowerDrain);
        }
        if ( batteryAmount >= batteryMaxValue)
        {
            mainMenu.BackToMainMenu();
        }
        else if (batteryAmount <= 0 && !powerOutSoundPlayed)
        {
            audioPlayer.PowerOut();
            powerOutSoundPlayed = true;
        }
    }

    public void UpdateBatteryDisplay()
    {
        debugBatteryText.text = batteryAmount + "%";
        int percentage = Mathf.RoundToInt(batteryAmount);
        homeBatteryText.text = percentage + "%";
        mathBatteryText.text = percentage + "%";
    }

    public void ChangeBattery(float value)
    {
        batteryAmount += value;
        UpdateBatteryDisplay();
    }
}
using UnityEngine;

public class TabletController : MonoBehaviour
{
    public bool tabletUp = false;
    [Header("Debug")]
    public bool debugOnScreen = false;
    // If debug info can be turned on or off, false = debug always off
    public bool debugAllowed = true;
    public GameObject debugCanvas;
    [Header("Power drain")]
    public bool tabletPowerDrainEnabled = true;
    public float tabletPowerDrain = -0.0001f;
    [Header("Screens")]
    public GameObject mathCanvas;
    public GameObject tabletHomeCanvas;
    public GameObject CharacterCanvas;
    public GameObject radarCanvas;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        debugCanvas.SetActive(false);
        tabletHomeCanvas.SetActive(false);
        mathCanvas.SetActive(false);
        CharacterCanvas.SetActive(false);
        radarCanvas.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        var batteryScript = FindAnyObjectByType<BatteryDisplay>();
        // Turn on or off tablet
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (tabletUp && batteryScript.batteryAmount > batteryScript.batteryMinValue)
            {
                tabletUp = false;
                //turn off all tablet canvases
                tabletHomeCanvas.SetActive(false);
                mathCanvas.SetActive(false);
                CharacterCanvas.SetActive(false);
                radarCanvas.SetActive(false);
            }
            else
            {
                tabletUp = true;
                // turn on the home tablet canvas and turn off all non tablet canvases
                tabletHomeCanvas.SetActive(true);
                mathCanvas.SetActive(false);
                CharacterCanvas.SetActive(false);
                radarCanvas.SetActive(false);
            }
        }
        // Turn off tablet if there is no power
        if (batteryScript.batteryAmount <= batteryScript.batteryMinValue)
        {
            tabletUp = false;
            // turn off all tablet canvases
            tabletHomeCanvas.SetActive(false);
            mathCanvas.SetActive(false);
            CharacterCanvas.SetActive(false);
            radarCanvas.SetActive(false);
        }
        if (debugAllowed)
        {
            // Enable debug into when '`'('~') is pressed
            if (Input.GetKeyDown(KeyCode.BackQuote))
            {
                if (debugOnScreen)
                {
                    debugOnScreen = false;
                    debugCanvas.SetActive(false);
                }
                else
                {
                    debugOnScreen = true;
                    debugCanvas.SetActive(true);
                }
            }
        }
        if (tabletPowerDrainEnabled && tabletUp == true)
        {
            batteryScript.ChangeBattery(tabletPowerDrain);
        }
    }
}


using UnityEngine;

public class TabletController : MonoBehaviour
{
    public bool tabletUp = false;

    [Header("Debug")]
    public bool debugOnScreen = false;
    public bool debugAllowed = true;
    public GameObject debugCanvas;

    [Header("Power Drain")]
    public bool tabletPowerDrainEnabled = true;
    public float tabletPowerDrain = -0.0001f;

    [Header("Screens")]
    public GameObject mathCanvas;
    public GameObject tabletHomeCanvas;
    public GameObject CharacterCanvas;
    public GameObject radarCanvas;

    [Header("Simon Says")]
    public GameObject simonSaysCanvas;

    private BatteryDisplay batteryScript;

    private void Start()
    {
        batteryScript = FindAnyObjectByType<BatteryDisplay>();

        tabletUp = false;

        if (debugCanvas != null)
            debugCanvas.SetActive(false);

        HideTabletScreens();
    }

    private void Update()
    {
        if (batteryScript == null)
            batteryScript = FindAnyObjectByType<BatteryDisplay>();

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (tabletUp)
            {
                CloseTablet();
            }
            else if (batteryScript != null &&
                     batteryScript.batteryAmount >
                     batteryScript.batteryMinValue)
            {
                OpenTablet();
            }
        }

        if (batteryScript != null &&
            batteryScript.batteryAmount <= batteryScript.batteryMinValue)
        {
            CloseTablet();
        }

        if (debugAllowed &&
            Input.GetKeyDown(KeyCode.BackQuote))
        {
            debugOnScreen = !debugOnScreen;

            if (debugCanvas != null)
                debugCanvas.SetActive(debugOnScreen);
        }

        if (tabletPowerDrainEnabled &&
            tabletUp &&
            batteryScript != null)
        {
            batteryScript.ChangeBattery(
                tabletPowerDrain * Time.deltaTime);
        }
    }

    public void OpenTablet()
    {
        tabletUp = true;

        HideGameScreens();

        if (tabletHomeCanvas != null)
            tabletHomeCanvas.SetActive(true);
    }

    public void CloseTablet()
    {
        tabletUp = false;

        HideTabletScreens();
    }

    private void HideGameScreens()
    {
        if (mathCanvas != null)
            mathCanvas.SetActive(false);

        if (CharacterCanvas != null)
            CharacterCanvas.SetActive(false);

        if (radarCanvas != null)
            radarCanvas.SetActive(false);

        if (simonSaysCanvas != null)
            simonSaysCanvas.SetActive(false);
    }

    private void HideTabletScreens()
    {
        HideGameScreens();

        if (tabletHomeCanvas != null)
            tabletHomeCanvas.SetActive(false);
    }
}

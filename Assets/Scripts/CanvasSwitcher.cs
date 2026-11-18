
using UnityEngine;

public class CanvasSwitcher : MonoBehaviour
{
    [Header("Main Canvas")]
    public GameObject tabletHomeCanvas;

    [Header("Game Canvases")]
    public GameObject mathCanvas;
    public GameObject characterCanvas;
    public GameObject radarCanvas;
    public GameObject simonSaysCanvas;

    [Header("Game Controller")]
    public GamesScript gamesScript;

    private void Awake()
    {
        FindGamesScript();
    }

    private bool FindGamesScript()
    {
        if (gamesScript == null)
        {
            gamesScript = GetComponent<GamesScript>();
        }

        if (gamesScript == null)
        {
            gamesScript = FindAnyObjectByType<GamesScript>(
                FindObjectsInactive.Include
            );
        }

        if (gamesScript == null)
        {
            Debug.LogError(
                "CanvasSwitcher: No GamesScript found in the scene!"
            );
            return false;
        }

        return true;
    }

    public void OpenRandomGame()
    {
        if (!FindGamesScript())
            return;

        if (tabletHomeCanvas != null)
            tabletHomeCanvas.SetActive(false);

        gamesScript.StartRandomGame();
    }

    public void BackToTabletHome()
    {
        if (FindGamesScript())
            gamesScript.HideAllMiniGames();

        if (radarCanvas != null)
            radarCanvas.SetActive(false);

        if (tabletHomeCanvas != null)
            tabletHomeCanvas.SetActive(true);
    }

    public void OpenRadar()
    {
        if (FindGamesScript())
            gamesScript.HideAllMiniGames();

        if (tabletHomeCanvas != null)
            tabletHomeCanvas.SetActive(false);

        if (radarCanvas != null)
            radarCanvas.SetActive(true);
    }

    public void OpenMathGame()
    {
        if (!FindGamesScript())
            return;

        if (radarCanvas != null)
            radarCanvas.SetActive(false);

        if (tabletHomeCanvas != null)
            tabletHomeCanvas.SetActive(false);

        gamesScript.OpenMathGame();
    }

    public void OpenCharacterGame()
    {
        if (!FindGamesScript())
            return;

        if (radarCanvas != null)
            radarCanvas.SetActive(false);

        if (tabletHomeCanvas != null)
            tabletHomeCanvas.SetActive(false);

        gamesScript.OpenCharacterGame();
    }

    public void OpenSimonSaysGame()
    {
        if (!FindGamesScript())
            return;

        if (radarCanvas != null)
            radarCanvas.SetActive(false);

        if (tabletHomeCanvas != null)
            tabletHomeCanvas.SetActive(false);

        gamesScript.OpenSimonSaysGame();
    }
}

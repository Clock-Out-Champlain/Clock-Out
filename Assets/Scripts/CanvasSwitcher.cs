
using UnityEngine;

public class CanvasSwitcher : MonoBehaviour
{
    [Header("Main Canvas")]
    public GameObject tabletHomeCanvas;

    [Header("Game Canvases")]
    public GameObject mathCanvas;
    public GameObject characterCanvas;

    /*
    [Header("Simon Says")]
    public GameObject simonSaysCanvas;
    */


    // Randomly selects a game
    public void OpenRandomGame()
    {
        Debug.Log("OPEN RANDOM GAME WAS CALLED!");

        // Check that the required canvases exist
        if (mathCanvas == null || characterCanvas == null)
        {
            Debug.LogError("Math Canvas or Character Canvas is missing!");
            return;
        }

        // Hide all canvases first
        if (tabletHomeCanvas != null)
        {
            tabletHomeCanvas.SetActive(false);
        }

        mathCanvas.SetActive(false);
        characterCanvas.SetActive(false);

        /*
        simonSaysCanvas.SetActive(false);
        */

        // Randomly choose between Math and Character
        int randomGame = Random.Range(0, 2);

        switch (randomGame)
        {
            case 0:
                OpenMathGame();                
                Debug.Log("Random Game Selected: Math Game");
                break;

            case 1:
                characterCanvas.SetActive(true);
                Debug.Log("Random Game Selected: Character Game");
                break;

                /*
                case 2:
                    simonSaysCanvas.SetActive(true);
                    Debug.Log("Random Game Selected: Simon Says");
                    break;
                */
        }
    }


    // Return to tablet home
    public void BackToTabletHome()
    {
        mathCanvas.SetActive(false);
        characterCanvas.SetActive(false);

        /*
        simonSaysCanvas.SetActive(false);
        */

        if (tabletHomeCanvas != null)
        {
            tabletHomeCanvas.SetActive(true);
        }
    }


    // Open Math Game directly
    public void OpenMathGame()
    {
        tabletHomeCanvas.SetActive(false);
        characterCanvas.SetActive(false);
        mathCanvas.SetActive(true);

        /*
        simonSaysCanvas.SetActive(false);
        */
    }


    // Open Character Game directly
    public void OpenCharacterGame()
    {
        tabletHomeCanvas.SetActive(false);
        mathCanvas.SetActive(false);
        characterCanvas.SetActive(true);

        /*
        simonSaysCanvas.SetActive(false);
        */
    }


    /*
    // Open Simon Says directly
    public void OpenSimonSaysGame()
    {
        tabletHomeCanvas.SetActive(false);
        mathCanvas.SetActive(false);
        characterCanvas.SetActive(false);
        simonSaysCanvas.SetActive(true);
    }
    */
}

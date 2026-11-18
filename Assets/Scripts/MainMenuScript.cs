using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public MovementAI ram;
    public MalMovementAI mal;
    public EyeMovementAI eye;
    public DoorController door;

    // The difficulty selected by the player.
    // 0 = On The Clock
    // 1 = Beat The Clock
    // 2 = Overclocked
    public static int selectedDifficulty = 0;

    void Start()
    {
        if (SceneManager.GetActiveScene().name == "Office")
        {
            StartWithDifficulty(selectedDifficulty);
        }
    }

    // Opens the Difficulty Screen.
    public void StartGame()
    {
        SceneManager.LoadScene("Difficulty Screen");
    }

    // Opens the Settings Screen.
    // You can create this scene later.
    public void OpenSettings()
    {
        SceneManager.LoadScene("Settings");
    }

    // Closes the game.
    public void QuitGame()
    {
        Debug.Log("Quitting game!");

        Application.Quit();
    }

    // Returns to the Main Menu.
    public void BackToMainMenu()
    {
        selectedDifficulty = -1;
        SceneManager.LoadScene("Main Menu");
    }

    // Starts the game on On The Clock difficulty.
    public void SelectOnTheClock()
    {
        selectedDifficulty = 0;
        SceneManager.LoadScene("Office");
    }

    // Starts the game on Beat The Clock difficulty.
    public void SelectBeatTheClock()
    {
        selectedDifficulty = 1;
        SceneManager.LoadScene("Office");
    }

    // Starts the game on Overclocked difficulty.
    public void SelectOverclocked()
    {
        selectedDifficulty = 2;
        SceneManager.LoadScene("Office");
    }

    // Saves the chosen difficulty and loads the game.
    private void StartWithDifficulty(int difficulty)
    {
        var ram = FindAnyObjectByType<MovementAI>();
        var mal = FindAnyObjectByType<MalMovementAI>();
        var eye = FindAnyObjectByType<EyeMovementAI>();
        var door = FindAnyObjectByType<DoorController>();

       if (difficulty == 0)
        {
            Debug.Log("Selected difficulty: 0");
            ram.ChangeAIDifficulty(0);
            mal.ChangeAIDifficulty(0);
            eye.ChangeAIDifficulty(0);
            door.ChangeAIDifficulty(0);
        }
        else if (difficulty == 1)
        {
            Debug.Log("Selected difficulty: 1");
            ram.ChangeAIDifficulty(1);
            mal.ChangeAIDifficulty(1);
            eye.ChangeAIDifficulty(1);
            door.ChangeAIDifficulty(1);
        }
        else if (difficulty == 2)
        {
            Debug.Log("Selected difficulty: 2");
            ram.ChangeAIDifficulty(2);
            mal.ChangeAIDifficulty(2);
            eye.ChangeAIDifficulty(2);
            door.ChangeAIDifficulty(2);
        }
        else
        {
            Debug.LogError("Difficulty error");
        }
    }
}

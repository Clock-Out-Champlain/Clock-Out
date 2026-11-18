
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class GamesScript : MonoBehaviour
{
    // =========================================
    // SHARED TASK COUNTER
    // =========================================

    private static int sharedTasksComplete;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetSharedCounter()
    {
        sharedTasksComplete = 0;
    }

    [Header("Shared Task Counter")]
    public TMP_Text tasksCompleteText;
    public TMP_Text[] allTasksCompleteTexts;

    // =========================================
    // CANVASES
    // =========================================

    [Header("Mini Game Canvases")]
    public GameObject mathCanvas;
    public GameObject characterCanvas;
    public GameObject simonCanvas;

    private enum GameType
    {
        None,
        Math,
        Character,
        Simon
    }

    private GameType currentGame = GameType.None;

    private bool taskProcessing;

    private Coroutine nextGameCoroutine;
    private Coroutine simonSequenceCoroutine;

    // =========================================
    // MATH GAME
    // =========================================

    [Header("Math Game")]
    public TMP_Text mathQuestionText;
    public TMP_InputField mathAnswerInput;
    public TMP_Text mathFeedbackText;
    public TMP_Text mathScoreText;
    public Button mathSubmitButton;

    [Header("Math Battery")]
    public int mathBatteryGain = 5;
    public int mathBatteryLoss = -10;

    private int correctAnswer;
    private int mathCorrectCount;

    // =========================================
    // CHARACTER GAME
    // =========================================

    [Header("Character Game")]
    public TMP_Text sequenceText;
    public TMP_InputField characterAnswerInput;
    public TMP_Text characterTimerText;
    public TMP_Text characterFeedbackText;
    public Button characterSubmitButton;

    [Header("Character Difficulty")]
    public int startingSequenceLength = 5;
    public int maximumSequenceLength = 12;
    public float baseTime = 5f;
    public float timePerCharacter = 2f;

    [Header("Character Battery")]
    public int characterBatteryGain = 10;
    public int characterBatteryLoss = -10;

    private string currentSequence;
    private int sequenceLength;
    private float timeRemaining;
    private bool characterTaskActive;

    // =========================================
    // SIMON SAYS BUTTONS
    // =========================================

    [Header("Simon Says Individual Buttons")]
    public Button headButton;
    public Button leftArmButton;
    public Button chestButton;
    public Button rightArmButton;
    public Button leftLegButton;
    public Button rightLegButton;

    [Header("Simon Says Text")]
    public TMP_Text simonFeedbackText;
    public TMP_Text simonRoundText;
    public TMP_Text simonProgressText;

    [Header("Simon Says Difficulty")]
    public int simonStartingLength = 3;
    public int simonMaximumLength = 12;

    [Header("Simon Says Timing")]
    public float flashDuration = 0.5f;
    public float pauseBetweenFlashes = 0.3f;
    public float inactivityTime = 7f;
    public float playerFeedbackDuration = 0.2f;

    [Header("Simon Says Colors")]
    public Color highlightColor = Color.yellow;
    public Color pressedColor = Color.green;

    [Header("Simon Says Battery")]
    public int simonBatteryGain = 10;
    public int simonBatteryLoss = -10;

    private readonly List<int> simonSequence = new List<int>();

    private Button[] simonButtons;
    private Graphic[] simonGraphics;
    private Color[] originalSimonColors;
    private UnityAction[] simonButtonActions;

    private Coroutine[] buttonFlashCoroutines;

    // These names match the six button references above.
    private readonly string[] bodyPartNames =
    {
        "Head",
        "Left Arm",
        "Chest",
        "Right Arm",
        "Left Leg",
        "Right Leg"
    };

    private int simonPlayerProgress;
    private int simonSequenceLength;

    private bool simonAcceptingInput;
    private bool simonShowingSequence;

    private float simonInactivityTimer;

    // =========================================
    // BATTERY
    // =========================================

    private BatteryDisplay batteryScript;

    // =========================================
    // START
    // =========================================

    private void Start()
    {
        batteryScript = FindAnyObjectByType<BatteryDisplay>();

        sequenceLength = Mathf.Max(1, startingSequenceLength);
        simonSequenceLength = Mathf.Max(1, simonStartingLength);

        if (mathSubmitButton != null)
            mathSubmitButton.onClick.AddListener(CheckMathAnswer);

        if (mathAnswerInput != null)
            mathAnswerInput.onSubmit.AddListener(OnMathEnter);

        if (characterSubmitButton != null)
            characterSubmitButton.onClick.AddListener(CheckCharacterSequence);

        if (characterAnswerInput != null)
            characterAnswerInput.onSubmit.AddListener(OnCharacterEnter);

        InitializeSimonButtons();

        HideAllMiniGames();

        UpdateMathScore();
        UpdateTasksComplete();

        Debug.Log("GamesScript initialized.");
    }

    // =========================================
    // INITIALIZE SIMON BUTTONS
    // =========================================

    private void InitializeSimonButtons()
    {
        simonButtons = new Button[]
        {
            headButton,
            leftArmButton,
            chestButton,
            rightArmButton,
            leftLegButton,
            rightLegButton
        };

        simonGraphics = new Graphic[simonButtons.Length];
        originalSimonColors = new Color[simonButtons.Length];
        simonButtonActions = new UnityAction[simonButtons.Length];
        buttonFlashCoroutines = new Coroutine[simonButtons.Length];

        for (int i = 0; i < simonButtons.Length; i++)
        {
            if (simonButtons[i] == null)
            {
                Debug.LogError(
                    "Simon Says button is missing: " + bodyPartNames[i]);
                continue;
            }

            int index = i;

            simonGraphics[i] = simonButtons[i].targetGraphic;

            if (simonGraphics[i] == null)
                simonGraphics[i] = simonButtons[i].GetComponent<Graphic>();

            if (simonGraphics[i] != null)
            {
                originalSimonColors[i] = simonGraphics[i].color;
            }
            else
            {
                Debug.LogWarning(
                    bodyPartNames[i] + " has no target Graphic assigned.");
            }

            simonButtonActions[i] =
                () => OnSimonButtonPressed(index);

            simonButtons[i].onClick.AddListener(simonButtonActions[i]);

            simonButtons[i].interactable = false;
        }
    }

    // =========================================
    // HIDE ALL GAMES
    // =========================================

    public void HideAllMiniGames()
    {
        StopGameTransition();

        if (mathCanvas != null)
            mathCanvas.SetActive(false);

        if (characterCanvas != null)
            characterCanvas.SetActive(false);

        if (simonCanvas != null)
            simonCanvas.SetActive(false);

        currentGame = GameType.None;

        taskProcessing = false;
        characterTaskActive = false;

        simonAcceptingInput = false;
        simonShowingSequence = false;

        SetSimonButtonsInteractable(false);
        RestoreSimonColors();
    }

    // =========================================
    // RANDOM GAME SELECTOR
    // =========================================

    public void StartRandomGame()
    {
        StopGameTransition();

        List<GameType> availableGames = new List<GameType>();

        if (mathCanvas != null)
            availableGames.Add(GameType.Math);

        if (characterCanvas != null)
            availableGames.Add(GameType.Character);

        if (simonCanvas != null && AreAllSimonButtonsAssigned())
            availableGames.Add(GameType.Simon);

        if (availableGames.Count == 0)
        {
            Debug.LogError("No available mini-games!");
            return;
        }

        GameType previousGame = currentGame;

        if (availableGames.Count > 1)
            availableGames.Remove(previousGame);

        GameType selectedGame =
            availableGames[Random.Range(0, availableGames.Count)];

        HideAllMiniGames();

        currentGame = selectedGame;

        switch (currentGame)
        {
            case GameType.Math:
                mathCanvas.SetActive(true);
                GenerateMathQuestion();
                break;

            case GameType.Character:
                characterCanvas.SetActive(true);
                GenerateCharacterSequence();
                break;

            case GameType.Simon:
                simonCanvas.SetActive(true);
                GenerateSimonSequence();
                break;
        }

        Debug.Log("Selected game: " + currentGame);
    }

    private bool AreAllSimonButtonsAssigned()
    {
        if (simonButtons == null || simonButtons.Length != 6)
            return false;

        foreach (Button button in simonButtons)
        {
            if (button == null)
                return false;
        }

        return true;
    }

    // =========================================
    // DIRECT GAME OPENING
    // =========================================

    public void OpenMathGame()
    {
        HideAllMiniGames();

        currentGame = GameType.Math;

        if (mathCanvas != null)
        {
            mathCanvas.SetActive(true);
            GenerateMathQuestion();
        }
    }

    public void OpenCharacterGame()
    {
        HideAllMiniGames();

        currentGame = GameType.Character;

        if (characterCanvas != null)
        {
            characterCanvas.SetActive(true);
            GenerateCharacterSequence();
        }
    }

    public void OpenSimonSaysGame()
    {
        HideAllMiniGames();

        currentGame = GameType.Simon;

        if (simonCanvas != null)
        {
            simonCanvas.SetActive(true);
            GenerateSimonSequence();
        }
    }

    // =========================================
    // STOP COROUTINES
    // =========================================

    private void StopGameTransition()
    {
        if (nextGameCoroutine != null)
        {
            StopCoroutine(nextGameCoroutine);
            nextGameCoroutine = null;
        }

        if (simonSequenceCoroutine != null)
        {
            StopCoroutine(simonSequenceCoroutine);
            simonSequenceCoroutine = null;
        }

        if (buttonFlashCoroutines != null)
        {
            for (int i = 0; i < buttonFlashCoroutines.Length; i++)
            {
                if (buttonFlashCoroutines[i] != null)
                {
                    StopCoroutine(buttonFlashCoroutines[i]);
                    buttonFlashCoroutines[i] = null;
                }
            }
        }

        RestoreSimonColors();
    }

    // =========================================
    // FINISH TASK
    // =========================================

    private void FinishCurrentTask()
    {
        if (taskProcessing)
            return;

        taskProcessing = true;

        characterTaskActive = false;
        simonAcceptingInput = false;

        if (mathAnswerInput != null)
            mathAnswerInput.interactable = false;

        if (mathSubmitButton != null)
            mathSubmitButton.interactable = false;

        if (characterAnswerInput != null)
            characterAnswerInput.interactable = false;

        if (characterSubmitButton != null)
            characterSubmitButton.interactable = false;

        SetSimonButtonsInteractable(false);

        if (nextGameCoroutine != null)
            StopCoroutine(nextGameCoroutine);

        nextGameCoroutine = StartCoroutine(SwitchToNextGame());
    }

    private IEnumerator SwitchToNextGame()
    {
        yield return new WaitForSecondsRealtime(1.5f);

        nextGameCoroutine = null;

        StartRandomGame();
    }

    // =========================================
    // ENTER KEY
    // =========================================

    private void OnMathEnter(string value)
    {
        CheckMathAnswer();
    }

    private void OnCharacterEnter(string value)
    {
        CheckCharacterSequence();
    }

    // =========================================
    // MATH GAME
    // =========================================

    private void GenerateMathQuestion()
    {
        int number1;
        int number2;
        string symbol;

        switch (Random.Range(0, 4))
        {
            case 0:
                number1 = Random.Range(1, 51);
                number2 = Random.Range(1, 51);
                correctAnswer = number1 + number2;
                symbol = "+";
                break;

            case 1:
                number1 = Random.Range(1, 101);
                number2 = Random.Range(1, number1 + 1);
                correctAnswer = number1 - number2;
                symbol = "-";
                break;

            case 2:
                number1 = Random.Range(2, 13);
                number2 = Random.Range(2, 13);
                correctAnswer = number1 * number2;
                symbol = "×";
                break;

            default:
                number2 = Random.Range(2, 13);
                correctAnswer = Random.Range(1, 13);
                number1 = number2 * correctAnswer;
                symbol = "÷";
                break;
        }

        if (mathQuestionText != null)
            mathQuestionText.text =
                number1 + " " + symbol + " " + number2 + " = ?";

        if (mathAnswerInput != null)
        {
            mathAnswerInput.text = "";
            mathAnswerInput.interactable = true;
        }

        if (mathFeedbackText != null)
            mathFeedbackText.text = "Enter your answer.";

        if (mathSubmitButton != null)
            mathSubmitButton.interactable = true;

        UpdateMathScore();

        taskProcessing = false;
    }

    public void CheckMathAnswer()
    {
        if (currentGame != GameType.Math ||
            taskProcessing ||
            mathAnswerInput == null)
            return;

        if (!int.TryParse(mathAnswerInput.text.Trim(), out int answer))
        {
            if (mathFeedbackText != null)
                mathFeedbackText.text = "Please enter a whole number.";

            return;
        }

        if (answer == correctAnswer)
        {
            mathCorrectCount++;
            sharedTasksComplete++;

            ChangeSharedBattery(mathBatteryGain);

            if (mathFeedbackText != null)
                mathFeedbackText.text = "Correct! Battery increased.";
        }
        else
        {
            ChangeSharedBattery(mathBatteryLoss);

            if (mathFeedbackText != null)
                mathFeedbackText.text =
                    "Incorrect! Answer: " + correctAnswer;
        }

        UpdateMathScore();
        UpdateTasksComplete();

        FinishCurrentTask();
    }

    private void UpdateMathScore()
    {
        if (mathScoreText != null)
            mathScoreText.text = "Correct: " + mathCorrectCount;
    }

    public void RestartMathGame()
    {
        if (currentGame != GameType.Math)
            return;

        StopGameTransition();

        taskProcessing = false;

        GenerateMathQuestion();
    }

    // =========================================
    // CHARACTER GAME
    // =========================================

    private void GenerateCharacterSequence()
    {
        const string characters = "ABCDFGHIJKLMNOPQRSTUVXYZ";

        currentSequence = "";

        for (int i = 0; i < sequenceLength; i++)
        {
            currentSequence +=
                characters[Random.Range(0, characters.Length)];
        }

        if (sequenceText != null)
            sequenceText.text = currentSequence;

        if (characterAnswerInput != null)
        {
            characterAnswerInput.text = "";
            characterAnswerInput.interactable = true;
        }

        if (characterSubmitButton != null)
            characterSubmitButton.interactable = true;

        if (characterFeedbackText != null)
            characterFeedbackText.text = "";

        timeRemaining =
            baseTime + sequenceLength * timePerCharacter;

        characterTaskActive = true;
        taskProcessing = false;

        UpdateCharacterTimer();
    }

    public void CheckCharacterSequence()
    {
        if (currentGame != GameType.Character ||
            !characterTaskActive ||
            taskProcessing ||
            characterAnswerInput == null)
            return;

        string answer =
            characterAnswerInput.text.Trim().ToUpper();

        characterTaskActive = false;

        if (answer == currentSequence)
        {
            sharedTasksComplete++;

            sequenceLength = Mathf.Min(
                sequenceLength + 1,
                maximumSequenceLength);

            ChangeSharedBattery(characterBatteryGain);

            if (characterFeedbackText != null)
                characterFeedbackText.text = "Correct! Battery increased.";
        }
        else
        {
            ChangeSharedBattery(characterBatteryLoss);

            if (characterFeedbackText != null)
                characterFeedbackText.text = "Incorrect! Battery decreased.";
        }

        UpdateTasksComplete();

        FinishCurrentTask();
    }

    private void CharacterTimeOut()
    {
        if (!characterTaskActive || taskProcessing)
            return;

        characterTaskActive = false;

        ChangeSharedBattery(characterBatteryLoss);

        if (characterFeedbackText != null)
            characterFeedbackText.text = "Time's up! Battery decreased.";

        FinishCurrentTask();
    }

    private void UpdateCharacterTimer()
    {
        if (characterTimerText != null)
        {
            characterTimerText.text =
                "Time Left: " +
                Mathf.CeilToInt(Mathf.Max(0, timeRemaining));
        }
    }

    public void RestartCharacterTask()
    {
        if (currentGame != GameType.Character)
            return;

        StopGameTransition();

        sequenceLength = Mathf.Max(1, startingSequenceLength);
        taskProcessing = false;

        GenerateCharacterSequence();
    }

    // =========================================
    // SIMON SAYS - GENERATE SEQUENCE
    // =========================================

    private void GenerateSimonSequence()
    {
        if (!AreAllSimonButtonsAssigned())
        {
            Debug.LogError("One or more Simon Says buttons are missing.");
            return;
        }

        simonSequence.Clear();

        // Randomly choose from ALL SIX body parts.
        for (int i = 0; i < simonSequenceLength; i++)
        {
            simonSequence.Add(Random.Range(0, simonButtons.Length));
        }

        simonPlayerProgress = 0;

        simonAcceptingInput = false;
        simonShowingSequence = true;

        simonInactivityTimer = 0f;
        taskProcessing = false;

        if (simonFeedbackText != null)
            simonFeedbackText.text = "Watch carefully!";

        if (simonRoundText != null)
            simonRoundText.text =
                "Sequence Length: " + simonSequenceLength;

        UpdateSimonProgress();

        SetSimonButtonsInteractable(false);
        RestoreSimonColors();

        simonSequenceCoroutine = StartCoroutine(PlaySimonSequence());
    }

    // =========================================
    // SIMON SAYS - PLAY SEQUENCE
    // =========================================

    private IEnumerator PlaySimonSequence()
    {
        simonShowingSequence = true;
        simonAcceptingInput = false;

        SetSimonButtonsInteractable(false);

        if (simonFeedbackText != null)
            simonFeedbackText.text = "Watch the body parts!";

        yield return new WaitForSecondsRealtime(0.8f);

        for (int i = 0; i < simonSequence.Count; i++)
        {
            if (currentGame != GameType.Simon || taskProcessing)
            {
                simonSequenceCoroutine = null;
                yield break;
            }

            int index = simonSequence[i];

            // Every index from 0 through 5 is valid.
            yield return StartCoroutine(FlashSimonButton(index));

            yield return new WaitForSecondsRealtime(pauseBetweenFlashes);
        }

        if (currentGame != GameType.Simon || taskProcessing)
        {
            simonSequenceCoroutine = null;
            yield break;
        }

        simonShowingSequence = false;
        simonAcceptingInput = true;

        simonPlayerProgress = 0;
        simonInactivityTimer = 0f;

        UpdateSimonProgress();

        SetSimonButtonsInteractable(true);

        if (simonFeedbackText != null)
            simonFeedbackText.text = "Your turn! Repeat the sequence.";

        simonSequenceCoroutine = null;
    }

    // =========================================
    // SIMON SAYS - FLASH BUTTON
    // =========================================

    private IEnumerator FlashSimonButton(int index)
    {
        if (index < 0 || index >= simonButtons.Length)
            yield break;

        Graphic graphic = simonGraphics[index];

        if (graphic == null)
            yield break;

        graphic.color = highlightColor;

        yield return new WaitForSecondsRealtime(flashDuration);

        if (graphic != null)
            graphic.color = originalSimonColors[index];
    }

    // =========================================
    // SIMON SAYS - PLAYER INPUT
    // =========================================

    private void OnSimonButtonPressed(int index)
    {
        if (currentGame != GameType.Simon ||
            !simonAcceptingInput ||
            simonShowingSequence ||
            taskProcessing)
            return;

        // FIX:
        // Check against the SIX available buttons,
        // NOT against the current sequence length.
        if (index < 0 || index >= simonButtons.Length)
            return;

        if (simonPlayerProgress >= simonSequence.Count)
            return;

        simonInactivityTimer = 0f;

        Debug.Log("Simon Says button pressed: " + bodyPartNames[index]);

        if (buttonFlashCoroutines[index] != null)
        {
            StopCoroutine(buttonFlashCoroutines[index]);
            buttonFlashCoroutines[index] = null;
        }

        buttonFlashCoroutines[index] =
            StartCoroutine(PlayerButtonFeedback(index));

        if (simonSequence[simonPlayerProgress] == index)
        {
            simonPlayerProgress++;

            UpdateSimonProgress();

            if (simonFeedbackText != null)
            {
                simonFeedbackText.text =
                    "Correct! " + bodyPartNames[index];
            }

            if (simonPlayerProgress >= simonSequence.Count)
                CorrectSimonSequence();
        }
        else
        {
            IncorrectSimonSequence();
        }
    }

    // =========================================
    // SIMON SAYS - PLAYER BUTTON FEEDBACK
    // =========================================

    private IEnumerator PlayerButtonFeedback(int index)
    {
        if (index < 0 || index >= simonGraphics.Length)
            yield break;

        if (simonGraphics[index] == null)
            yield break;

        simonGraphics[index].color = pressedColor;

        yield return new WaitForSecondsRealtime(playerFeedbackDuration);

        if (simonGraphics[index] != null)
            simonGraphics[index].color = originalSimonColors[index];

        buttonFlashCoroutines[index] = null;
    }

    // =========================================
    // SIMON SAYS - CORRECT SEQUENCE
    // =========================================

    private void CorrectSimonSequence()
    {
        if (taskProcessing)
            return;

        simonAcceptingInput = false;

        SetSimonButtonsInteractable(false);

        simonSequenceLength = Mathf.Min(
            simonSequenceLength + 1,
            simonMaximumLength);

        ChangeSharedBattery(simonBatteryGain);

        sharedTasksComplete++;

        UpdateTasksComplete();

        if (simonFeedbackText != null)
            simonFeedbackText.text = "Perfect! Battery increased!";

        FinishCurrentTask();
    }

    // =========================================
    // SIMON SAYS - INCORRECT SEQUENCE
    // =========================================

    private void IncorrectSimonSequence()
    {
        if (taskProcessing)
            return;

        simonAcceptingInput = false;

        SetSimonButtonsInteractable(false);

        ChangeSharedBattery(simonBatteryLoss);

        if (simonFeedbackText != null)
            simonFeedbackText.text = "Incorrect! Battery decreased.";

        FinishCurrentTask();
    }

    // =========================================
    // SIMON SAYS - PROGRESS
    // =========================================

    private void UpdateSimonProgress()
    {
        if (simonProgressText != null)
        {
            simonProgressText.text =
                simonPlayerProgress + " / " + simonSequence.Count;
        }
    }

    // =========================================
    // SIMON SAYS - BUTTON INTERACTIVITY
    // =========================================

    private void SetSimonButtonsInteractable(bool state)
    {
        if (simonButtons == null)
            return;

        foreach (Button button in simonButtons)
        {
            if (button != null)
                button.interactable = state;
        }
    }

    // =========================================
    // SIMON SAYS - RESTORE COLORS
    // =========================================

    private void RestoreSimonColors()
    {
        if (simonGraphics == null || originalSimonColors == null)
            return;

        for (int i = 0; i < simonGraphics.Length; i++)
        {
            if (simonGraphics[i] != null)
                simonGraphics[i].color = originalSimonColors[i];
        }
    }

    // =========================================
    // SIMON SAYS - INACTIVITY TIMER
    // =========================================

    private void UpdateSimonInactivity()
    {
        if (currentGame != GameType.Simon ||
            !simonAcceptingInput ||
            simonShowingSequence ||
            taskProcessing)
            return;

        simonInactivityTimer += Time.unscaledDeltaTime;

        if (simonInactivityTimer >= inactivityTime)
        {
            simonAcceptingInput = false;

            simonPlayerProgress = 0;
            simonInactivityTimer = 0f;

            UpdateSimonProgress();

            if (simonFeedbackText != null)
                simonFeedbackText.text =
                    "Time's up! Watch the sequence again.";

            if (simonSequenceCoroutine != null)
                StopCoroutine(simonSequenceCoroutine);

            simonSequenceCoroutine = StartCoroutine(PlaySimonSequence());
        }
    }

    // =========================================
    // UPDATE
    // =========================================

    private void Update()
    {
        UpdateSimonInactivity();

        if (currentGame != GameType.Character ||
            !characterTaskActive ||
            taskProcessing)
            return;

        timeRemaining -= Time.unscaledDeltaTime;

        UpdateCharacterTimer();

        if (timeRemaining <= 0f)
            CharacterTimeOut();
    }

    // =========================================
    // SHARED TASK COUNTER
    // =========================================

    private void UpdateTasksComplete()
    {
        string counterText =
            "Tasks Complete: " + sharedTasksComplete;

        if (tasksCompleteText != null)
            tasksCompleteText.text = counterText;

        if (allTasksCompleteTexts != null)
        {
            foreach (TMP_Text textObject in allTasksCompleteTexts)
            {
                if (textObject != null)
                    textObject.text = counterText;
            }
        }

        GamesScript[] allGames =
    FindObjectsByType<GamesScript>(
        FindObjectsInactive.Include);

        foreach (GamesScript game in allGames)
        {
            if (game.tasksCompleteText != null)
                game.tasksCompleteText.text = counterText;

            if (game.allTasksCompleteTexts != null)
            {
                foreach (TMP_Text textObject in game.allTasksCompleteTexts)
                {
                    if (textObject != null)
                        textObject.text = counterText;
                }
            }
        }
    }

    // =========================================
    // BATTERY
    // =========================================

    private void ChangeSharedBattery(int amount)
    {
        if (batteryScript == null)
            batteryScript = FindAnyObjectByType<BatteryDisplay>();

        if (batteryScript != null)
            batteryScript.ChangeBattery(amount);
        else
            Debug.LogWarning("BatteryDisplay was not found!");
    }

    // =========================================
    // CLEANUP
    // =========================================

    private void OnDestroy()
    {
        if (mathSubmitButton != null)
            mathSubmitButton.onClick.RemoveListener(CheckMathAnswer);

        if (characterSubmitButton != null)
            characterSubmitButton.onClick.RemoveListener(CheckCharacterSequence);

        if (mathAnswerInput != null)
            mathAnswerInput.onSubmit.RemoveListener(OnMathEnter);

        if (characterAnswerInput != null)
            characterAnswerInput.onSubmit.RemoveListener(OnCharacterEnter);

        if (simonButtons != null && simonButtonActions != null)
        {
            for (int i = 0; i < simonButtons.Length; i++)
            {
                if (simonButtons[i] != null &&
                    simonButtonActions[i] != null)
                {
                    simonButtons[i].onClick.RemoveListener(
                        simonButtonActions[i]);
                }
            }
        }
    }
}

/*

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class GamesScript : MonoBehaviour
{
    // =========================
    // SHARED TASK COUNTER
    // =========================

    private static int sharedTasksComplete;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetSharedCounter()
    {
        sharedTasksComplete = 0;
    }

    private bool initialized;

    // =========================
    // MATH GAME
    // =========================

    [Header("Math Game")]
    public TMP_Text mathQuestionText;
    public TMP_InputField mathAnswerInput;
    public TMP_Text mathFeedbackText;
    public TMP_Text mathScoreText;
    public Button mathSubmitButton;

    [Header("Math Canvas")]
    public GameObject mathCanvas;

    public int mathBatteryGain = 5;
    public int mathBatteryLoss = -10;

    private int correctAnswer;
    private int mathCorrectCount;
    private bool mathWaitingForNextQuestion;
    private Coroutine nextMathQuestionCoroutine;

    // =========================
    // CHARACTER GAME
    // =========================

    [Header("Character Game")]
    public TMP_Text sequenceText;
    public TMP_InputField characterAnswerInput;
    public TMP_Text characterTimerText;
    public TMP_Text tasksCompleteText;
    public TMP_Text characterFeedbackText;
    public Button characterSubmitButton;

    [Header("Character Canvas")]
    public GameObject characterCanvas;

    public int characterBatteryGain = 10;
    public int characterBatteryLoss = -10;

    [Header("Character Difficulty")]
    public int startingSequenceLength = 5;
    public int maximumSequenceLength = 12;
    public float baseTime = 5f;
    public float timePerCharacter = 2f;

    private string currentSequence;
    private int sequenceLength;
    private float timeRemaining;

    private bool characterTaskActive;
    private bool characterProcessingAnswer;

    private Coroutine nextCharacterTaskCoroutine;

    // =========================
    // SHARED BATTERY
    // =========================

    private BatteryDisplay batteryScript;

    // =========================
    // START
    // =========================

    private void Start()
    {
        batteryScript = FindAnyObjectByType<BatteryDisplay>();

        // Math Game: submit with the button or by pressing Enter.
        if (mathSubmitButton != null)
            mathSubmitButton.onClick.AddListener(CheckMathAnswer);

        if (mathAnswerInput != null)
            mathAnswerInput.onSubmit.AddListener(OnMathEnter);

        // Character Game: submit with the button or by pressing Enter.
        if (characterSubmitButton != null)
            characterSubmitButton.onClick.AddListener(CheckCharacterSequence);

        if (characterAnswerInput != null)
            characterAnswerInput.onSubmit.AddListener(OnCharacterEnter);

        initialized = true;

        if (HasMathGame())
        {
            UpdateMathScore();
            GenerateMathQuestion();
        }

        if (HasCharacterGame())
        {
            if (sequenceLength <= 0)
                sequenceLength = startingSequenceLength;

            GenerateCharacterSequence();
        }

        UpdateTasksComplete();
    }

    // Called when Enter is pressed in the math input field.
    private void OnMathEnter(string value)
    {
        CheckMathAnswer();
    }

    // Called when Enter is pressed in the character input field.
    private void OnCharacterEnter(string value)
    {
        CheckCharacterSequence();
    }

    private bool HasMathGame()
    {
        return mathQuestionText != null ||
               mathAnswerInput != null ||
               mathSubmitButton != null;
    }

    private bool HasCharacterGame()
    {
        return sequenceText != null ||
               characterAnswerInput != null ||
               characterSubmitButton != null;
    }

    // =========================
    // IMPORTANT: REOPEN RESET
    // =========================

    private void OnEnable()
    {
        if (!initialized)
            return;

        // A previous question may have been interrupted.
        StopAllCoroutines();

        nextMathQuestionCoroutine = null;
        nextCharacterTaskCoroutine = null;

        if (HasMathGame())
        {
            mathWaitingForNextQuestion = false;
            GenerateMathQuestion();
        }

        if (HasCharacterGame())
        {
            characterTaskActive = false;
            characterProcessingAnswer = false;

            if (sequenceLength <= 0)
                sequenceLength = startingSequenceLength;

            GenerateCharacterSequence();
        }

        UpdateTasksComplete();
    }

    // =========================
    // IMPORTANT: CLOSE RESET
    // =========================

    private void OnDisable()
    {
        // Stop pending question transitions.
        StopAllCoroutines();

        nextMathQuestionCoroutine = null;
        nextCharacterTaskCoroutine = null;

        // Clear locked answer states.
        mathWaitingForNextQuestion = false;

        characterTaskActive = false;
        characterProcessingAnswer = false;

        // Do not apply a battery penalty when closing.
    }

    // =========================
    // UPDATE
    // =========================

    private void Update()
    {
        if (!HasCharacterGame())
            return;

        if (!characterTaskActive || characterProcessingAnswer)
            return;

        if (characterCanvas != null &&
            !characterCanvas.activeInHierarchy)
            return;

        timeRemaining -= Time.unscaledDeltaTime;

        UpdateCharacterTimer();

        if (timeRemaining <= 0f)
            CharacterTimeOut();
    }

    // =========================
    // MATH GAME
    // =========================

    private void GenerateMathQuestion()
    {
        int number1;
        int number2;
        string symbol;

        switch (Random.Range(0, 4))
        {
            case 0:
                number1 = Random.Range(1, 51);
                number2 = Random.Range(1, 51);
                correctAnswer = number1 + number2;
                symbol = "+";
                break;

            case 1:
                number1 = Random.Range(1, 101);
                number2 = Random.Range(1, number1 + 1);
                correctAnswer = number1 - number2;
                symbol = "-";
                break;

            case 2:
                number1 = Random.Range(2, 13);
                number2 = Random.Range(2, 13);
                correctAnswer = number1 * number2;
                symbol = "×";
                break;

            default:
                number2 = Random.Range(2, 13);
                correctAnswer = Random.Range(1, 13);
                number1 = number2 * correctAnswer;
                symbol = "÷";
                break;
        }

        if (mathQuestionText != null)
            mathQuestionText.text =
                number1 + " " + symbol + " " + number2 + " = ?";

        if (mathAnswerInput != null)
        {
            mathAnswerInput.text = "";
            mathAnswerInput.interactable = true;
        }

        if (mathFeedbackText != null)
            mathFeedbackText.text = "Enter your answer.";

        if (mathSubmitButton != null)
            mathSubmitButton.interactable = true;

        mathWaitingForNextQuestion = false;
    }

    public void CheckMathAnswer()
    {
        if (mathWaitingForNextQuestion ||
            mathAnswerInput == null)
            return;

        if (!int.TryParse(
            mathAnswerInput.text.Trim(),
            out int answer))
        {
            if (mathFeedbackText != null)
                mathFeedbackText.text =
                    "Please enter a whole number.";

            return;
        }

        mathWaitingForNextQuestion = true;

        mathAnswerInput.interactable = false;

        if (mathSubmitButton != null)
            mathSubmitButton.interactable = false;

        if (answer == correctAnswer)
        {
            mathCorrectCount++;
            sharedTasksComplete++;

            ChangeSharedBattery(mathBatteryGain);

            if (mathFeedbackText != null)
                mathFeedbackText.text =
                    "Correct! Battery increased.";
        }
        else
        {
            ChangeSharedBattery(mathBatteryLoss);

            if (mathFeedbackText != null)
                mathFeedbackText.text =
                    "Incorrect! Answer: " + correctAnswer;
        }

        UpdateMathScore();
        UpdateTasksComplete();

        if (nextMathQuestionCoroutine != null)
            StopCoroutine(nextMathQuestionCoroutine);

        nextMathQuestionCoroutine =
            StartCoroutine(NextMathQuestion());
    }

    private IEnumerator NextMathQuestion()
    {
        yield return new WaitForSecondsRealtime(1.5f);

        nextMathQuestionCoroutine = null;

        GenerateMathQuestion();
    }

    private void UpdateMathScore()
    {
        if (mathScoreText != null)
            mathScoreText.text =
                "Correct: " + mathCorrectCount;
    }

    public void RestartMathGame()
    {
        if (nextMathQuestionCoroutine != null)
            StopCoroutine(nextMathQuestionCoroutine);

        nextMathQuestionCoroutine = null;

        mathCorrectCount = 0;
        mathWaitingForNextQuestion = false;

        UpdateMathScore();
        GenerateMathQuestion();
    }

    // =========================
    // CHARACTER GAME
    // =========================

    private void GenerateCharacterSequence()
    {
        const string characters = "ABCDFGHIJKLMNOPQRSTUVXYZ";

        currentSequence = "";

        for (int i = 0; i < sequenceLength; i++)
        {
            currentSequence +=
                characters[Random.Range(0, characters.Length)];
        }

        if (sequenceText != null)
            sequenceText.text = currentSequence;

        if (characterAnswerInput != null)
        {
            characterAnswerInput.text = "";
            characterAnswerInput.interactable = true;
        }

        if (characterSubmitButton != null)
            characterSubmitButton.interactable = true;

        if (characterFeedbackText != null)
            characterFeedbackText.text = "";

        timeRemaining =
            baseTime + sequenceLength * timePerCharacter;

        characterTaskActive = true;
        characterProcessingAnswer = false;

        UpdateCharacterTimer();
    }

    public void CheckCharacterSequence()
    {
        if (!characterTaskActive ||
            characterProcessingAnswer ||
            characterAnswerInput == null)
            return;

        string answer =
            characterAnswerInput.text.Trim().ToUpper();

        characterTaskActive = false;
        characterProcessingAnswer = true;

        characterAnswerInput.interactable = false;

        if (characterSubmitButton != null)
            characterSubmitButton.interactable = false;

        if (answer == currentSequence)
        {
            sharedTasksComplete++;

            sequenceLength = Mathf.Min(
                sequenceLength + 1,
                maximumSequenceLength
            );

            ChangeSharedBattery(characterBatteryGain);

            if (characterFeedbackText != null)
                characterFeedbackText.text =
                    "Correct! Battery increased.";
        }
        else
        {
            ChangeSharedBattery(characterBatteryLoss);

            if (characterFeedbackText != null)
                characterFeedbackText.text =
                    "Incorrect! Try again.";
        }

        UpdateTasksComplete();

        if (nextCharacterTaskCoroutine != null)
            StopCoroutine(nextCharacterTaskCoroutine);

        nextCharacterTaskCoroutine =
            StartCoroutine(NextCharacterTask());
    }

    private void CharacterTimeOut()
    {
        if (!characterTaskActive ||
            characterProcessingAnswer)
            return;

        characterTaskActive = false;
        characterProcessingAnswer = true;

        if (characterAnswerInput != null)
            characterAnswerInput.interactable = false;

        if (characterSubmitButton != null)
            characterSubmitButton.interactable = false;

        ChangeSharedBattery(characterBatteryLoss);

        if (characterFeedbackText != null)
            characterFeedbackText.text =
                "Time's up! Battery decreased.";

        if (nextCharacterTaskCoroutine != null)
            StopCoroutine(nextCharacterTaskCoroutine);

        nextCharacterTaskCoroutine =
            StartCoroutine(NextCharacterTask());
    }

    private IEnumerator NextCharacterTask()
    {
        yield return new WaitForSecondsRealtime(1.5f);

        nextCharacterTaskCoroutine = null;

        GenerateCharacterSequence();
    }

    private void UpdateCharacterTimer()
    {
        if (characterTimerText != null)
        {
            characterTimerText.text =
                "Time Left: " +
                Mathf.CeilToInt(Mathf.Max(0, timeRemaining));
        }
    }

    private void UpdateTasksComplete()
    {
        // Update every active instance so both games
        // display the same completion count.

        GamesScript[] allGames =
     FindObjectsByType<GamesScript>();

        foreach (GamesScript game in allGames)
        {
            if (game.tasksCompleteText != null)
            {
                game.tasksCompleteText.text =
                    "Tasks Complete: " + sharedTasksComplete;
            }
        }
    }

    public void RestartCharacterTask()
    {
        if (nextCharacterTaskCoroutine != null)
            StopCoroutine(nextCharacterTaskCoroutine);

        nextCharacterTaskCoroutine = null;

        sequenceLength = startingSequenceLength;

        UpdateTasksComplete();
        GenerateCharacterSequence();
    }

    // =========================
    // SHARED BATTERY
    // =========================

    private void ChangeSharedBattery(int amount)
    {
        if (batteryScript == null)
        {
            batteryScript =
                FindAnyObjectByType<BatteryDisplay>();
        }

        if (batteryScript != null)
        {
            batteryScript.ChangeBattery(amount);
        }
        else
        {
            Debug.LogWarning(
                "BatteryDisplay was not found."
            );
        }
    }

    // =========================
    // CLEANUP
    // =========================

    private void OnDestroy()
    {
        if (mathSubmitButton != null)
            mathSubmitButton.onClick.RemoveListener(CheckMathAnswer);

        if (characterSubmitButton != null)
            characterSubmitButton.onClick.RemoveListener(CheckCharacterSequence);

        if (mathAnswerInput != null)
            mathAnswerInput.onSubmit.RemoveListener(OnMathEnter);

        if (characterAnswerInput != null)
            characterAnswerInput.onSubmit.RemoveListener(OnCharacterEnter);
    }
}*/
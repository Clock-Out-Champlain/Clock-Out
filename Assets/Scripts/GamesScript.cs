
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
        const string characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

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

        if (mathSubmitButton != null)
            mathSubmitButton.onClick.AddListener(CheckMathAnswer);

        if (characterSubmitButton != null)
            characterSubmitButton.onClick.AddListener(CheckCharacterSequence);

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
        // Allows you to press enter to input
        if (Input.GetKeyDown(KeyCode.Return) && mathCanvas.activeInHierarchy)
            CheckMathAnswer();
        if (Input.GetKeyDown(KeyCode.Return) && characterCanvas.activeInHierarchy)
            CheckCharacterSequence();

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
            FindObjectsOfType<GamesScript>();

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
    }
}*/
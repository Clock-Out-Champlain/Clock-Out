
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
}

/*
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class GamesScript : MonoBehaviour
{
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
    private int tasksComplete;
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

        UpdateMathScore();
        UpdateTasksComplete();

        GenerateMathQuestion();

        sequenceLength = startingSequenceLength;
        GenerateCharacterSequence();
    }

    // =========================
    // UPDATE
    // =========================

    private void Update()
    {
        if (!characterTaskActive || characterProcessingAnswer)
            return;

        if (characterCanvas != null &&
            !characterCanvas.activeInHierarchy)
            return;

        timeRemaining -= Time.unscaledDeltaTime;

        UpdateCharacterTimer();

        if (timeRemaining <= 0)
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
            mathQuestionText.text = number1 + " " + symbol + " " + number2 + " = ?";

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
        if (mathWaitingForNextQuestion || mathAnswerInput == null)
            return;

        if (!int.TryParse(mathAnswerInput.text.Trim(), out int answer))
        {
            if (mathFeedbackText != null)
                mathFeedbackText.text = "Please enter a whole number.";

            return;
        }

        mathWaitingForNextQuestion = true;
        mathAnswerInput.interactable = false;

        if (mathSubmitButton != null)
            mathSubmitButton.interactable = false;

        if (answer == correctAnswer)
        {
            mathCorrectCount++;
            tasksComplete++;

            ChangeSharedBattery(mathBatteryGain);

            if (mathFeedbackText != null)
                mathFeedbackText.text = "Correct! Battery increased.";
        }
        else
        {
            ChangeSharedBattery(mathBatteryLoss);

            if (mathFeedbackText != null)
                mathFeedbackText.text = "Incorrect! Answer: " + correctAnswer;
        }

        UpdateMathScore();
        UpdateTasksComplete();

        if (nextMathQuestionCoroutine != null)
            StopCoroutine(nextMathQuestionCoroutine);

        nextMathQuestionCoroutine = StartCoroutine(NextMathQuestion());
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
            mathScoreText.text = "Correct: " + mathCorrectCount;
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
            currentSequence += characters[Random.Range(0, characters.Length)];

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

        timeRemaining = baseTime + sequenceLength * timePerCharacter;

        characterTaskActive = true;
        characterProcessingAnswer = false;

        UpdateCharacterTimer();
    }

    public void CheckCharacterSequence()
    {
        if (!characterTaskActive || characterProcessingAnswer ||
            characterAnswerInput == null)
            return;

        string answer = characterAnswerInput.text.Trim().ToUpper();

        characterTaskActive = false;
        characterProcessingAnswer = true;

        characterAnswerInput.interactable = false;

        if (characterSubmitButton != null)
            characterSubmitButton.interactable = false;

        if (answer == currentSequence)
        {
            tasksComplete++;

            sequenceLength = Mathf.Min(
                sequenceLength + 1,
                maximumSequenceLength
            );

            ChangeSharedBattery(characterBatteryGain);

            if (characterFeedbackText != null)
                characterFeedbackText.text = "Correct! Battery increased.";
        }
        else
        {
            ChangeSharedBattery(characterBatteryLoss);

            if (characterFeedbackText != null)
                characterFeedbackText.text = "Incorrect! Try again.";
        }

        UpdateTasksComplete();

        if (nextCharacterTaskCoroutine != null)
            StopCoroutine(nextCharacterTaskCoroutine);

        nextCharacterTaskCoroutine = StartCoroutine(NextCharacterTask());
    }

    private void CharacterTimeOut()
    {
        if (!characterTaskActive || characterProcessingAnswer)
            return;

        characterTaskActive = false;
        characterProcessingAnswer = true;

        if (characterAnswerInput != null)
            characterAnswerInput.interactable = false;

        if (characterSubmitButton != null)
            characterSubmitButton.interactable = false;

        ChangeSharedBattery(characterBatteryLoss);

        if (characterFeedbackText != null)
            characterFeedbackText.text = "Time's up! Battery decreased.";

        nextCharacterTaskCoroutine = StartCoroutine(NextCharacterTask());
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
            characterTimerText.text = "Time Left: " +
                Mathf.CeilToInt(Mathf.Max(0, timeRemaining));
    }

    private void UpdateTasksComplete()
    {
        if (tasksCompleteText != null)
            tasksCompleteText.text = "Tasks Complete: " + tasksComplete;
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
    // SHARED BATTERY FUNCTION
    // =========================

    private void ChangeSharedBattery(int amount)
    {
        if (batteryScript == null)
        {
            batteryScript = FindAnyObjectByType<BatteryDisplay>();
        }

        if (batteryScript != null)
        {
            batteryScript.ChangeBattery(amount);
        }
        else
        {
            Debug.LogWarning("BatteryDisplay was not found.");
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
}
//Fix So that when you close when you anwser and its cheking the anwser is correct it dosent freze on both games and the timer closses so it dosnt contune if you close the tablet because right now when you close the tablet the timer contuines and you lose opwer also when you open and close the tablet you should always start on the home tablet screen because your sapose to get a random game each time and if you close the tablet on the game the question should get randimized for both



//using UnityEngine;
//using UnityEngine.UI;
//using TMPro;
//using System.Collections;
//using System.Collections.Generic;

//public class MathGameController : MonoBehaviour
//{
//    [Header("Math Question")]
//    public TMP_Text questionText;
//    public TMP_InputField answerInput;
//    public TMP_Text feedbackText;
//    public TMP_Text scoreText;

//    [Header("Buttons")]
//    public Button checkAnswerButton;

//    [Header("Battery Settings")]
//    public int batteryGain = 10;
//    public int batteryLoss = -10;

//    private int correctAnswer;
//    private int correctCount = 0;
//    private bool waitingForNextQuestion = false;
//    private bool gameOver = false;

//    private void Start()
//    {
//        // Connect the Check Answer button.
//        checkAnswerButton.onClick.AddListener(CheckAnswer);

//        // Show the starting score.
//        UpdateScore();

//        // Generate the first question.
//        GenerateQuestion();
//    }

//    private void OnDestroy()
//    {
//        if (checkAnswerButton != null)
//        {
//            checkAnswerButton.onClick.RemoveListener(CheckAnswer);
//        }
//    }

//    // Generate a random math question.
//    private void GenerateQuestion()
//    {
//        int number1;
//        int number2;
//        int operation = Random.Range(0, 4);
//        string symbol;

//        switch (operation)
//        {
//            case 0:
//                // Addition
//                number1 = Random.Range(1, 51);
//                number2 = Random.Range(1, 51);

//                correctAnswer = number1 + number2;
//                symbol = "+";
//                break;

//            case 1:
//                // Subtraction without negative answers
//                number1 = Random.Range(1, 101);
//                number2 = Random.Range(1, number1 + 1);

//                correctAnswer = number1 - number2;
//                symbol = "-";
//                break;

//            case 2:
//                // Multiplication
//                number1 = Random.Range(2, 13);
//                number2 = Random.Range(2, 13);

//                correctAnswer = number1 * number2;
//                symbol = "×";
//                break;

//            default:
//                // Division with whole-number answers
//                number2 = Random.Range(2, 13);
//                correctAnswer = Random.Range(1, 13);
//                number1 = number2 * correctAnswer;

//                symbol = "÷";
//                break;
//        }

//        questionText.text =
//            number1 + " " + symbol + " " + number2 + " = ?";

//        answerInput.text = "";
//        feedbackText.text = "Enter your answer.";

//        waitingForNextQuestion = false;
//        checkAnswerButton.interactable = true;
//        answerInput.interactable = true;
//    }

//    // Check the number entered by the player.
//    public void CheckAnswer()
//    {
//        var batteryScript = FindAnyObjectByType<BatteryDisplay>();
//        if (gameOver || waitingForNextQuestion)
//        {
//            return;
//        }

//        string playerInput = answerInput.text.Trim();

//        int playerAnswer;

//        if (!int.TryParse(playerInput, out playerAnswer))
//        {
//            feedbackText.text = "Please enter a whole number.";
//            return;
//        }

//        waitingForNextQuestion = true;

//        checkAnswerButton.interactable = false;
//        answerInput.interactable = false;

//        if (playerAnswer == correctAnswer)
//        {
//            correctCount++;

//            batteryScript.ChangeBattery(batteryGain);

//            feedbackText.text = "Correct! Battery increased.";
//        }
//        else
//        {
//            batteryScript.ChangeBattery(batteryLoss);

//            feedbackText.text =
//                "Incorrect! The answer was " + correctAnswer + ".";
//        }

//        UpdateScore();

//        // Wait briefly before displaying another question.
//        StartCoroutine(NextQuestion());
//    }

//    private IEnumerator NextQuestion()
//    {
//        yield return new WaitForSeconds(1.5f);

//        GenerateQuestion();
//    }

//    // Update the correct-answer counter.
//    private void UpdateScore()
//    {
//        scoreText.text = "Correct: " + correctCount;
//    }

//    // Restart the math challenge.
//    public void RestartGame()
//    {
//        StopAllCoroutines();

//        gameOver = false;
//        waitingForNextQuestion = false;
//        correctCount = 0;

//        // Keep the current battery level.
//        // The player must earn battery by answering correctly.

//        UpdateScore();
//        GenerateQuestion();
//    }
//}
*/
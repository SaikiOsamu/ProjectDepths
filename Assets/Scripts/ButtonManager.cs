using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ButtonManager : MonoBehaviour
{
    [SerializeField] Button[] buttons;
    [SerializeField] TextMeshProUGUI LoadText;
    [SerializeField] TextMeshProUGUI FirstDigit;
    [SerializeField] TextMeshProUGUI SecondDigit;
    [SerializeField] TextMeshProUGUI ThirdDigit;

    [SerializeField] private float codeTimeLimit = 5f; // Time limit in seconds to input the code

    private string targetCode = "";
    private string playerInput = "";
    private int currentDigitPosition = 0; // Track which digit we're currently inputting
    private Coroutine timeoutCoroutine; // Reference to track the timeout coroutine
    private Coroutine randomTriggerCoroutine; // Reference to track the random trigger coroutine
    private bool isCodeActive = false; // Flag to indicate if a code is currently active
    private bool isProcessingResult = false; // Flag to prevent input during result animation

    // Colors for feedback
    private Color correctColor = Color.green;
    private Color incorrectColor = Color.red;
    private Color defaultColor = Color.white;

    // Sound Variables
    [SerializeField] string passwordEnterSound = "PasswordEnter";
    [SerializeField] string passwordCorrectSound = "PasswordCorrect";
    [SerializeField] string passwordWrongSound = "PasswordWrong";

    AudioManager audioManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        // Get audio manager reference
        audioManager = AudioManager.instance;
        if (audioManager == null)
        {
            Debug.LogError("No audio manager found");
        }


        LoadText.text = "LOADING...";
        LoadText.color = defaultColor;
        FirstDigit.text = SecondDigit.text = ThirdDigit.text = "";

        for (int i = 0; i < buttons.Length; i++)
        {
            int digit = i;
            buttons[i].onClick.AddListener(() => OnDigitPressed(digit));
        }

        // Start the random trigger coroutine
        randomTriggerCoroutine = StartCoroutine(RandomTrigger());
    }

    IEnumerator RandomTrigger()
    {
        while (true)
        {
            if (!isCodeActive && !isProcessingResult) // Only generate a new code if no code is active and not processing
            {
                float waitTime = Random.Range(5f, 10f);
                yield return new WaitForSeconds(waitTime);

                // Double check before triggering a new code
                if (!isCodeActive && !isProcessingResult)
                {
                    StartCoroutine(TriggerPasswordEvent());
                }
            }
            else
            {
                // If a code is active or result is processing, just check again shortly
                yield return new WaitForSeconds(0.5f);
            }
        }
    }

    IEnumerator TriggerPasswordEvent()
    {
        // Set the flag to indicate a code is active
        isCodeActive = true;
        isProcessingResult = false;

        playerInput = "";
        targetCode = "";
        currentDigitPosition = 0; // Reset digit position
        FirstDigit.color = SecondDigit.color = ThirdDigit.color = defaultColor;

        for (int i = 0; i < 3; i++)
        {
            int digit = Random.Range(0, 10);
            targetCode += digit.ToString();

            if (i == 0) FirstDigit.text = digit.ToString();
            else if (i == 1) SecondDigit.text = digit.ToString();
            else ThirdDigit.text = digit.ToString();
        }

        LoadText.text = "";

        // Start the timeout countdown
        if (timeoutCoroutine != null)
        {
            StopCoroutine(timeoutCoroutine);
        }
        timeoutCoroutine = StartCoroutine(CodeTimeout());

        yield break;
    }

    // Coroutine for timing out if player doesn't complete the code
    IEnumerator CodeTimeout()
    {
        yield return new WaitForSeconds(codeTimeLimit);

        // Only expire if code entry hasn't been completed or failed yet
        if (isCodeActive && !isProcessingResult && targetCode != "" && currentDigitPosition < 3)
        {
            // Immediately mark as processing result to prevent further input
            isProcessingResult = true;
            isCodeActive = false;

            StartCoroutine(ExpiredInput());
        }
    }

    void OnDigitPressed(int digit)
    {
        // Prevent input if: code not active, processing result, or no target code
        if (targetCode == "" || !isCodeActive || isProcessingResult)
            return;

        audioManager.PlaySound(passwordEnterSound);

        // Get the expected digit at the current position
        char expectedDigit = targetCode[currentDigitPosition];
        bool isCorrect = (digit.ToString() == expectedDigit.ToString());

        if (isCorrect)
        {
            // Update the color of the current digit text to green
            if (currentDigitPosition == 0)
                FirstDigit.color = correctColor;
            else if (currentDigitPosition == 1)
                SecondDigit.color = correctColor;
            else if (currentDigitPosition == 2)
                ThirdDigit.color = correctColor;

            // Add the digit to player input
            playerInput += digit.ToString();

            // Move to the next digit
            currentDigitPosition++;

            // Check if the complete code has been entered correctly
            if (currentDigitPosition == 3)
            {
                // Set flags to prevent further input
                isProcessingResult = true;
                isCodeActive = false;

                // Stop the timeout coroutine since we completed the code
                if (timeoutCoroutine != null)
                {
                    StopCoroutine(timeoutCoroutine);
                    timeoutCoroutine = null;
                }

                StartCoroutine(CorrectInput());
            }
        }
        else
        {
            // Handle wrong input at the current digit position
            playerInput += digit.ToString(); // Still record what they pressed

            // Set flags to prevent further input
            isProcessingResult = true;
            isCodeActive = false;

            // Update the color of the current digit text to red
            if (currentDigitPosition == 0)
                FirstDigit.color = incorrectColor;
            else if (currentDigitPosition == 1)
                SecondDigit.color = incorrectColor;
            else if (currentDigitPosition == 2)
                ThirdDigit.color = incorrectColor;

            // Stop the timeout coroutine since we failed
            if (timeoutCoroutine != null)
            {
                StopCoroutine(timeoutCoroutine);
                timeoutCoroutine = null;
            }

            // Trigger wrong input sequence
            StartCoroutine(WrongInput());
        }
    }

    IEnumerator CorrectInput()
    {
        GameManager.instance.GainScore(100);

        for (int i = 0; i < 4; i++)
        {
            FirstDigit.color = SecondDigit.color = ThirdDigit.color = correctColor;
            yield return new WaitForSeconds(0.2f);
            FirstDigit.color = SecondDigit.color = ThirdDigit.color = Color.clear;
            yield return new WaitForSeconds(0.2f);
        }

        LoadText.color = correctColor;
        LoadText.text = "CORRECT!";

        audioManager.PlaySound(passwordCorrectSound);

        yield return new WaitForSeconds(2f);
        ResetCodeState();
    }

    IEnumerator WrongInput()
    {
        GameManager.instance.GainScore(-50);
        GameManager.instance.Punish();

        for (int i = 0; i < 4; i++)
        {
            FirstDigit.color = SecondDigit.color = ThirdDigit.color = incorrectColor;
            yield return new WaitForSeconds(0.2f);
            FirstDigit.color = SecondDigit.color = ThirdDigit.color = Color.clear;
            yield return new WaitForSeconds(0.2f);
        }

        LoadText.color = incorrectColor;
        LoadText.text = "FAIL";

        audioManager.PlaySound(passwordWrongSound);

        yield return new WaitForSeconds(2f);
        ResetCodeState();
    }

    IEnumerator ExpiredInput()
    {
        GameManager.instance.GainScore(-50); // Same penalty as wrong input
        GameManager.instance.Punish();

        for (int i = 0; i < 4; i++)
        {
            FirstDigit.color = SecondDigit.color = ThirdDigit.color = incorrectColor;
            yield return new WaitForSeconds(0.2f);
            FirstDigit.color = SecondDigit.color = ThirdDigit.color = Color.clear;
            yield return new WaitForSeconds(0.2f);
        }

        LoadText.color = incorrectColor;
        LoadText.text = "EXPIRED"; // Different message for timeout
        yield return new WaitForSeconds(2f);
        ResetCodeState();
    }

    // Helper method to reset all code-related state
    private void ResetCodeState()
    {
        LoadText.color = defaultColor;
        LoadText.text = "LOADING...";
        FirstDigit.text = SecondDigit.text = ThirdDigit.text = "";
        targetCode = "";
        playerInput = "";
        currentDigitPosition = 0;

        // Ensure timeout coroutine is stopped
        if (timeoutCoroutine != null)
        {
            StopCoroutine(timeoutCoroutine);
            timeoutCoroutine = null;
        }

        // Reset the code active flag, allowing new codes to be generated
        isCodeActive = false;
        isProcessingResult = false;
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnDestroy()
    {
        // Clean up any active coroutines when this object is destroyed
        if (timeoutCoroutine != null)
        {
            StopCoroutine(timeoutCoroutine);
        }

        if (randomTriggerCoroutine != null)
        {
            StopCoroutine(randomTriggerCoroutine);
        }
    }
}
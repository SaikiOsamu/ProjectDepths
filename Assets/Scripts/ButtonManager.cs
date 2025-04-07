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

    private string targetCode = "";
    private string playerInput = "";

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        LoadText.text = "LOADING...";
        LoadText.color = Color.white;
        FirstDigit.text = SecondDigit.text = ThirdDigit.text = "";

        for (int i = 0; i < buttons.Length; i++)
        {
            int digit = i;
            buttons[i].onClick.AddListener(() => OnDigitPressed(digit));
        }

        StartCoroutine(RandomTrigger());
    }

    IEnumerator RandomTrigger()
    {
        while (true)
        {
            float waitTime = Random.Range(5f, 10f);
            yield return new WaitForSeconds(waitTime);
            StartCoroutine(TriggerPasswordEvent());
        }
    }
    IEnumerator TriggerPasswordEvent()
    {
        playerInput = "";
        targetCode = "";
        FirstDigit.color = SecondDigit.color = ThirdDigit.color = Color.white;

        for (int i = 0; i < 3; i++)
        {
            int digit = Random.Range(0, 10);
            targetCode += digit.ToString();
            
            if (i == 0) FirstDigit.text = digit.ToString();
            else if (i == 1) SecondDigit.text = digit.ToString();
            else ThirdDigit.text = digit.ToString();
        }

        LoadText.text = "";
        yield break;
    }

    void OnDigitPressed(int digit)
    {
        if (targetCode == "") return;

        playerInput += digit.ToString();

        if (playerInput.Length == 3)
        {
            if (playerInput == targetCode)
            {
                StartCoroutine(CorrectInput());
            }
            else
            {
                StartCoroutine(WrongInput());
            }
        }
    }

    IEnumerator CorrectInput()
    {
        GameManager.instance.GainScore(100);
        Color green = Color.green;

        for (int i = 0; i < 4; i++)
        {
            FirstDigit.color = SecondDigit.color = ThirdDigit.color = green;
            yield return new WaitForSeconds(0.2f);
            FirstDigit.color = SecondDigit.color = ThirdDigit.color = Color.clear;
            yield return new WaitForSeconds(0.2f);
        }
        LoadText.color = green;
        LoadText.text = "CORRECT!";
        yield return new WaitForSeconds(2f);
        LoadText.color = Color.white;
        LoadText.text = "LOADING...";
        FirstDigit.text = SecondDigit.text = ThirdDigit.text = "";
        targetCode = "";
        playerInput = "";
        yield return null;
    }

    IEnumerator WrongInput()
    {
        GameManager.instance.GainScore(-50);
        GameManager.instance.Punish();
        Color red = Color.red;

        for (int i = 0; i < 4; i++)
        {
            FirstDigit.color = SecondDigit.color = ThirdDigit.color = red;
            yield return new WaitForSeconds(0.2f);
            FirstDigit.color = SecondDigit.color = ThirdDigit.color = Color.clear;
            yield return new WaitForSeconds(0.2f);
        }
        LoadText.color = red;
        LoadText.text = "FAIL";
        yield return new WaitForSeconds(2f);
        FirstDigit.text = SecondDigit.text = ThirdDigit.text = "";
        LoadText.color = Color.white;
        LoadText.text = "LOADING...";
        targetCode = "";
        playerInput = "";
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

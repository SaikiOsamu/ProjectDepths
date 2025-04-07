using UnityEngine;
using UnityEngine.UI;

public class ProgressBarController : MonoBehaviour
{
    public Scrollbar progressBar;
    public float fillDuration = 30f;

    private float timer = 0f;

    void Start()
    {
        UpdateProgressBar(0f);
    }

    void Update()
    {
        timer += Time.deltaTime;
        float progress = Mathf.Clamp01(timer / fillDuration);
        UpdateProgressBar(progress);

        if (progress >= 1f)
        {
            timer = 0f;
            UpdateProgressBar(0f);
            GameManager.instance.GainScore(100);
        }
    }

    void UpdateProgressBar(float progress)
    {
        progressBar.size = Mathf.Lerp(0f, 0.92f, progress);
    }
}

using UnityEngine;

public class LogoController : MonoBehaviour
{
    [Header("Logo GameObjects")]
    [SerializeField] private GameObject logo_01;
    [SerializeField] private GameObject logo_02;

    [Header("Toggle Settings")]
    [SerializeField] private bool toggled = true;

    void Start()
    {
        // Verify that both game objects are assigned
        if (logo_01 == null || logo_02 == null)
        {
            Debug.LogError("One or both logo GameObjects are not assigned to the LogoController on " + gameObject.name);
            return;
        }

        // Initial setup based on toggled state
        UpdateLogoVisibility();
    }

    void Update()
    {
        // Check if the toggled state has changed in the inspector
        UpdateLogoVisibility();
    }

    // Updates the visibility of the logos based on the toggled state
    private void UpdateLogoVisibility()
    {
        if (logo_01 == null || logo_02 == null)
            return;

        // Set the appropriate visibility based on toggled state
        logo_01.SetActive(toggled);
        logo_02.SetActive(!toggled);
    }

    // Public method to toggle the state
    public void Toggle()
    {
        toggled = !toggled;
        UpdateLogoVisibility();
    }

    // Public method to set the state directly
    public void SetToggled(bool state)
    {
        toggled = state;
        UpdateLogoVisibility();
    }
}
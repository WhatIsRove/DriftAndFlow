using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public GameObject continueButton;
    public GameObject continueDisabled;

    private void Start()
    {
        if (PlayerPrefs.HasKey("WorldSeed") && PlayerPrefs.GetInt("WorldSeed") != 0)
        {
            continueButton.SetActive(true);
            continueDisabled.SetActive(false);
        } else
        {
            continueButton.SetActive(false);
            continueDisabled.SetActive(true);
        }
    }
}

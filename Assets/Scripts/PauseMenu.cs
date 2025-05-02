using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public GameObject settingsMenu;

    public void Resume()
    {
        GameManager.instance.PauseGame();
    }

    public void SettingsMenu()
    {
        gameObject.SetActive(false);
        settingsMenu.SetActive(true);
    }

    public void QuitToMenu()
    {
        GameManager.instance.QuitToMenu();
    }

    public void QuitGame()
    {
        GameManager.instance.QuitGame();
    }
}

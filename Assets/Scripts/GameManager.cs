using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public GameObject pauseUI;
    public GameObject settingsUI;
    public static bool isPaused = false;

    bool newGame = true;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else Destroy(gameObject);
    }
    public void NewGame()
    {
        newGame = true;
        QuestManager.instance.ResetQuests();
        int seed = Random.Range(0, 100000);
        PlayerPrefs.SetInt("WorldSeed", seed);
        PlayerPrefs.SetInt("Gold", 0);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);


    }

    public void CheckSave()
    {
        if (SceneManager.GetActiveScene().buildIndex == 1)
        {
            pauseUI = GameObject.Find("PauseMenu");
            settingsUI = GameObject.Find("SettingsMenu");

            pauseUI.SetActive(false);
            settingsUI.SetActive(false);

            int seed;
            if (newGame)
            {
                newGame = false;
                QuestManager.instance.ResetQuests();
                seed = Random.Range(0, 100000);
                PlayerPrefs.SetInt("WorldSeed", seed);
            }
            else
            {
                seed = PlayerPrefs.GetInt("WorldSeed");
            }

            //var generator = FindObjectOfType<NoiseDensity>();
            var generators = FindObjectsOfType<NoiseDensity>();
            if (generators != null)
            {
                var meshGens = FindObjectsOfType<MeshGenerator>();

                foreach (var noiseGen in generators)
                {
                    noiseGen.seed = seed;
                }

                foreach (var meshGen in meshGens)
                {
                    meshGen.Run();
                }

            }

        }
    }

    public void StartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void QuitToMenu()
    {
        isPaused = false;
        Time.timeScale = 1f;
        AudioManager.Instance.Pause();


        SceneManager.LoadScene(0);
    }

    public void PauseGame()
    {
        if (pauseUI == null) return;

        isPaused = !isPaused;

        pauseUI.SetActive(isPaused);
        AudioManager.Instance.Pause();


        if (isPaused)
        {
            Time.timeScale = 0f;
        } else
        {
            settingsUI.SetActive(isPaused);
            Time.timeScale = 1f;
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}

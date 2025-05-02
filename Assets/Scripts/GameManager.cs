using System;
using System.Collections;
using TMPro;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public GameObject pauseUI;
    public GameObject settingsUI;
    public static bool isPaused = false;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else Destroy(gameObject);
    }

    private void Start()
    {
        //TODO: when saving and loading dont reset, or only reset on new game

        if (SceneManager.GetActiveScene().buildIndex == 1)
        {
            pauseUI = GameObject.Find("PauseMenu");
            settingsUI = GameObject.Find("SettingsMenu");                   

            pauseUI.SetActive(false);
            settingsUI.SetActive(false);

            //var generator = FindObjectOfType<NoiseDensity>();
            var generators = FindObjectsOfType<NoiseDensity>();
            if (generators != null)
            {
                int seed;
                if (PlayerPrefs.HasKey("WorldSeed"))
                {
                    seed = PlayerPrefs.GetInt("WorldSeed");
                }
                else
                {
                    QuestManager.instance.ResetQuests();
                    seed = Random.Range(0, 100000);
                    PlayerPrefs.SetInt("WorldSeed", seed);
                }

                foreach (var noiseGen in generators) {
                    noiseGen.seed = seed;
                }
                
            }

        }

    }

    public void NewGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        QuestManager.instance.ResetQuests();
        int seed = Random.Range(0, 100000);
        PlayerPrefs.SetInt("WorldSeed", seed);
        PlayerPrefs.SetInt("Gold", 0);
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

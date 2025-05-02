using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TMPro;
using UnityEngine;
using Color = UnityEngine.Color;
using Random = UnityEngine.Random;

public class BiomeManager : MonoBehaviour
{
    public Biome currentBiome;

    public Material waterMaterial;
    public Color[] biomeDeepColors;

    Color currentColor;
    Color targetColor;

    public float colorBlendRate = 10f;

    public static BiomeManager instance;

    List<BiomeEnter> currentBiomes = new();

    public TextMeshProUGUI biomeText;

    [Header("Biome Generator")]
    public GameObject biomeTriggerPrefab;
    public float gridSpacing = 200f;
    public float minDistance = 180f;
    public Vector2 mapSize = new Vector2(1000f, 1000f);
    public int maxAttempts = 100;

    public float minScale = 0.5f;
    public float maxScale = 2f;

    private List<Vector3> placedTriggers = new List<Vector3>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        currentBiome = Biome.TEMPERATE;

        PlaceBiomeTriggers();
    }

    private void Update()
    {
        currentColor = waterMaterial.GetColor("_Deep_Water_Color");
        targetColor = biomeDeepColors[(int)currentBiome];

        if (currentColor != targetColor)
        {
            Color newColor = Color.Lerp(currentColor, targetColor, colorBlendRate * Time.deltaTime);
            waterMaterial.SetColor("_Deep_Water_Color", newColor);
        }
    }

    public void SetBiome(int biomeIndex)
    {
        currentBiome = (Biome)biomeIndex;
        
    }

    public void EnterBiome(BiomeEnter biome)
    {
        if (!currentBiomes.Contains(biome))
        {
            currentBiomes.Add(biome);
        }

        CheckCurrentBiome();
    }

    public void ExitBiome(BiomeEnter biome)
    {
        if (currentBiomes.Contains(biome))
        {
            currentBiomes.Remove(biome);
        }

        CheckCurrentBiome();
    }

    void CheckCurrentBiome()
    {
        if (currentBiomes.Count > 0)
        {
            currentBiome = currentBiomes.First().localBiome;
            biomeText.text = "Biome: " + CultureInfo.CurrentCulture.TextInfo.ToTitleCase(currentBiome.ToString().Replace("_", " ").ToLower());
        }
        else currentBiome = 0;
        biomeText.text = "Biome: " + CultureInfo.CurrentCulture.TextInfo.ToTitleCase(currentBiome.ToString().Replace("_", " ").ToLower());
    }

    void PlaceBiomeTriggers()
    {
        // Adjust the starting point to the center of the map
        float startX = -mapSize.x / 2;
        float startZ = -mapSize.y / 2;

        for (float x = startX; x < mapSize.x / 2; x += gridSpacing)
        {
            for (float z = startZ; z < mapSize.y / 2; z += gridSpacing)
            {
                Vector3 potentialPosition = new Vector3(x, 0, z);
                bool validPosition = TryPlaceBiomeTrigger(potentialPosition);

                if (validPosition)
                {
                    GameObject newTrigger = Instantiate(biomeTriggerPrefab, potentialPosition, Quaternion.identity);
                    newTrigger.GetComponent<BiomeEnter>().localBiome = (Biome)Random.Range(0, (float)Enum.GetValues(typeof(Biome)).Cast<Biome>().Max()+1);
                    float randomScale = Random.Range(minScale, maxScale);
                    newTrigger.transform.localScale = transform.localScale * randomScale;

                    placedTriggers.Add(potentialPosition);
                }
            }
        }
    }

    bool TryPlaceBiomeTrigger(Vector3 position)
    {
        int attempts = 0;
        while (attempts < maxAttempts)
        {
            bool isValid = true;
            foreach (var placedPosition in placedTriggers)
            {
                float distance = Vector3.Distance(placedPosition, position);
                if (distance < minDistance)
                {
                    isValid = false;
                    break;
                }
            }

            if (isValid)
            {
                return true;
            }
            else
            {
                position = new Vector3(position.x + Random.Range(-gridSpacing / 2, gridSpacing / 2), 0, position.z + Random.Range(-gridSpacing / 2, gridSpacing / 2));
                attempts++;
            }
        }

        return false;
    }
}

public enum Biome
{
    TEMPERATE,
    COLD,
    BLOOD_SEA,
    POLLUTED,
    TWILIGHT_OCEAN
}
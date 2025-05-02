using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

public class MinigameManager : MonoBehaviour
{
    public Vector2 sectorAmountMinMax;
    public Vector2 sectorAngleRange;
    public List<GameObject> sectors;
    public GameObject sectorHolder;
    public GameObject pivot;

    public GameObject sectorPrefab;


    public void GenerateSectors()
    {
        if (sectors != null) {
            foreach (var sector in sectors) { 
                Destroy(sector);
            }
        }

        sectors = new List<GameObject>();
        for (int i = 0; i < Random.Range(sectorAmountMinMax.x, sectorAmountMinMax.y); i++)
        {
            GameObject sector = Instantiate(sectorPrefab, sectorHolder.transform);
            sector.GetComponent<Slider>().value = Random.Range(sectorAngleRange.x, sectorAngleRange.y);
            var sectorRot = sector.transform.localEulerAngles;
            sectorRot.z = Random.Range(40, 340);
            sector.transform.localEulerAngles = sectorRot;
            sectors.Add(sector);
        }
    }

    public bool CheckInput()
    {
        bool hasClicked = false;
        for (int i = 0; i < sectors.Count; i++)
        {
            if (!hasClicked && pivot.transform.localEulerAngles.z < sectors[i].transform.localEulerAngles.z && pivot.transform.localEulerAngles.z > sectors[i].transform.localEulerAngles.z - (sectors[i].GetComponent<Slider>().value))
            {
                if (sectors[i].activeSelf)
                {
                    sectors[i].SetActive(false);
                    hasClicked = true;
                }
            }

        }

        bool completed = true;
        for (int i = 0; i < sectors.Count; i++)
        {
            if (sectors[i].activeSelf)
            {
                completed = false;
            }
        }

        if (completed)
        {
            for (int i = 0; i < sectors.Count; i++)
            {
                Destroy(sectors[i], 5);
            }
            sectors.Clear();
            return true;
        }
        else return false;
        

        
    }
}

using UnityEngine;

public class BiomeEnter : MonoBehaviour
{
    [SerializeField]
    public Biome localBiome;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            BiomeManager.instance.EnterBiome(this);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            BiomeManager.instance.ExitBiome(this);
        }
    }
}

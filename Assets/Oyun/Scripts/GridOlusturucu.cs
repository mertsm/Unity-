using UnityEngine;

public class GridOlusturucu : MonoBehaviour
{
    public GameObject gridKaresiPrefab;
    public int genislik = 10;
    public int derinlik = 10;

    void Start()
    {
        // Eğer prefab atanmadıysa hata verip durduralım ki anlayalım
        if (gridKaresiPrefab == null)
        {
            Debug.LogError("HATA: Grid Karesi Prefabı atanmamış! OyunYoneticisi'ne bak.");
            return;
        }

        GameObject zeminParent = new GameObject("OlusturulanZemin");
        
        for (int x = 0; x < genislik; x++)
        {
            for (int z = 0; z < derinlik; z++)
            {
                // Yükseklik 0.1f olsun ki siyah zeminin (0) üstüne net çıksın
                Vector3 pozisyon = new Vector3(x, 0.001f, z); 

                GameObject yeniKare = Instantiate(gridKaresiPrefab, pozisyon, Quaternion.identity);
                yeniKare.transform.parent = zeminParent.transform;
                yeniKare.name = $"Kare_{x}_{z}";
            }
        }
    }
}
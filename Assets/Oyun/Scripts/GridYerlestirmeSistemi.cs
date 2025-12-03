using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class GridYerlestirmeSistemi : MonoBehaviour
{
    [Header("--- PREFABLAR ---")]
    public GameObject hayalet_1x1; public GameObject prefab_1x1;
    public GameObject hayalet_1x2; public GameObject prefab_1x2;
    public GameObject hayalet_1x3; public GameObject prefab_1x3;

    [Header("--- LEVEL AYARLARI ---")]
    public Vector3Int gridBoyutu = new Vector3Int(20, 10, 20);
    public Transform hedefYapiParent;

    [Header("--- GENEL ---")]
    public LayerMask insaatKatmanlari;

    private int[,,] gridMatrisi;
    private int[,,] hedefMatrisi;
    private Dictionary<Vector3Int, GameObject> blokKayitlari = new Dictionary<Vector3Int, GameObject>();

    private bool baslangicYapildi = false;
    private GameObject aktifHayalet;
    private GameObject aktifPrefab;
    private Camera anaKamera;
    private int simdikiUzunluk = 1;
    private bool yerlestirmeUygun = false;

    private Color uygunRenk = new Color(1f, 0.92f, 0.016f, 0.5f);
    private Color hataliRenk = new Color(1f, 0f, 0f, 0.5f);

    void Start() { }

    void Update()
    {
        if (!baslangicYapildi) { Baslat(); return; }

        if (EventSystem.current.IsPointerOverGameObject())
        {
            if (aktifHayalet != null && aktifHayalet.activeSelf) aktifHayalet.SetActive(false);
            return;
        }

        HayaletBloguYonet();

        if (Input.GetMouseButtonDown(0) && aktifHayalet != null && aktifHayalet.activeSelf && yerlestirmeUygun)
            BlokYerlestir();

        if (Input.GetMouseButtonDown(1)) BlokSil();

        if (Input.GetKeyDown(KeyCode.R) && aktifHayalet != null && aktifHayalet.activeSelf)
        {
            aktifHayalet.transform.Rotate(0, 90, 0);
            HayaletBloguYonet();
        }
    }

    void Baslat()
    {
        anaKamera = Camera.main;
        gridMatrisi = new int[gridBoyutu.x, gridBoyutu.y, gridBoyutu.z];
        HedefiAnalizEt();
        if (hayalet_1x1 != null && prefab_1x1 != null) BlokSec_1x1();
        baslangicYapildi = true;
    }

    void HayaletBloguYonet()
    {
        if (aktifHayalet == null) return;

        Ray isin = anaKamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit carpisma;

        if (Physics.Raycast(isin, out carpisma, 1000f, insaatKatmanlari))
        {
            // 1. ANCHOR (ÇAPA) BELİRLEME - DÜZELTİLDİ
            // Vuruş noktasına, yüzey normalinin yarısını ekliyoruz.
            // Bu bizi her zaman "yeni koyulacak karenin" tam merkezine atar.
            // Örn: Üste tıklarsan yukarı, yana tıklarsan yana atar.
            Vector3 hedefMerkez = carpisma.point + (carpisma.normal * 0.5f);

            Vector3Int anchorKoor = new Vector3Int(
                Mathf.RoundToInt(hedefMerkez.x),
                Mathf.FloorToInt(hedefMerkez.y), // Yükseklik hesabı artık kusursuz
                Mathf.RoundToInt(hedefMerkez.z)
            );

            // 2. SINIRLAMA (CLAMP)
            anchorKoor.x = Mathf.Clamp(anchorKoor.x, 0, gridBoyutu.x - 1);
            anchorKoor.y = Mathf.Clamp(anchorKoor.y, 0, gridBoyutu.y - 1); // Yükseklik sınırı da eklendi
            anchorKoor.z = Mathf.Clamp(anchorKoor.z, 0, gridBoyutu.z - 1);

            // 3. YER KONTROLÜ (Bu Anchor'dan başlayıp Yön boyunca uzanıyoruz)
            Vector3Int yonInt = Vector3Int.RoundToInt(aktifHayalet.transform.right);
            
            Vector3Int enIyiAnchor = anchorKoor;
            bool yerBulundu = false;

            // 0'dan başlayarak geriye doğru dene
            for (int offset = 0; offset < simdikiUzunluk; offset++)
            {
                Vector3Int testAnchor = anchorKoor - (yonInt * offset);
                if (AlanMusaitMi(testAnchor, yonInt))
                {
                    enIyiAnchor = testAnchor;
                    yerBulundu = true;
                    break;
                }
            }

            // 4. GÖRSEL POZİSYON HESABI
            // Matris koordinatını Dünya koordinatına çevir (Y + 0.5f farkı)
            Vector3 anchorDunyaPos = new Vector3(enIyiAnchor.x, enIyiAnchor.y + 0.5f, enIyiAnchor.z);
            
            // Bloğun görsel merkezini bulmak için kaydır
            Vector3 görselOfset = (Vector3)yonInt * ((simdikiUzunluk - 1) * 0.5f);
            Vector3 finalGorselPos = anchorDunyaPos + görselOfset;

            // Yer yoksa ilk (sığmayan) yerde dursun
            if (!yerBulundu)
            {
                Vector3 hataliAnchorDunyaPos = new Vector3(anchorKoor.x, anchorKoor.y + 0.5f, anchorKoor.z);
                finalGorselPos = hataliAnchorDunyaPos + görselOfset;
            }

            aktifHayalet.transform.position = finalGorselPos;

            // 5. RENK VE DURUM
            if (yerBulundu)
            {
                HayaletRenginiDegistir(uygunRenk);
                if (!aktifHayalet.activeSelf) aktifHayalet.SetActive(true);
                yerlestirmeUygun = true;
            }
            else
            {
                HayaletRenginiDegistir(hataliRenk);
                if (!aktifHayalet.activeSelf) aktifHayalet.SetActive(true);
                yerlestirmeUygun = false;
            }
        }
        else
        {
            if (aktifHayalet.activeSelf) aktifHayalet.SetActive(false);
        }
    }

    bool AlanMusaitMi(Vector3Int baslangic, Vector3Int yon)
    {
        for (int i = 0; i < simdikiUzunluk; i++)
        {
            Vector3Int bakilanYer = baslangic + (yon * i);

            if (bakilanYer.x < 0 || bakilanYer.x >= gridBoyutu.x ||
                bakilanYer.y < 0 || bakilanYer.y >= gridBoyutu.y ||
                bakilanYer.z < 0 || bakilanYer.z >= gridBoyutu.z) return false;

            if (gridMatrisi[bakilanYer.x, bakilanYer.y, bakilanYer.z] == 1) return false;
        }
        return true;
    }

    void BlokYerlestir()
    {
        GameObject yeniBlok = Instantiate(aktifPrefab, aktifHayalet.transform.position, aktifHayalet.transform.rotation);
        
        Vector3Int yonInt = Vector3Int.RoundToInt(yeniBlok.transform.right);
        Vector3 görselOfset = (Vector3)yonInt * ((simdikiUzunluk - 1) * 0.5f);
        Vector3 anchorDunyaPos = yeniBlok.transform.position - görselOfset;
        
        Vector3Int anchorKoor = new Vector3Int(
            Mathf.RoundToInt(anchorDunyaPos.x),
            Mathf.FloorToInt(anchorDunyaPos.y - 0.5f), 
            Mathf.RoundToInt(anchorDunyaPos.z)
        );

        for (int i = 0; i < simdikiUzunluk; i++)
        {
            Vector3Int p = anchorKoor + (yonInt * i);
            if (p.x >= 0 && p.x < gridBoyutu.x && p.y >= 0 && p.y < gridBoyutu.y && p.z >= 0 && p.z < gridBoyutu.z)
            {
                gridMatrisi[p.x, p.y, p.z] = 1;
                if (!blokKayitlari.ContainsKey(p)) blokKayitlari.Add(p, yeniBlok);
            }
        }
    }

    void BlokSil()
    {
        Ray isin = anaKamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit carpisma;
        if (Physics.Raycast(isin, out carpisma, 1000f, insaatKatmanlari))
        {
            if (carpisma.collider.CompareTag("BlokTag"))
            {
                GameObject silinecek = carpisma.collider.gameObject;
                if (carpisma.collider.transform.parent != null) silinecek = carpisma.collider.transform.parent.gameObject;

                List<Vector3Int> silinecekKoor = new List<Vector3Int>();
                foreach (var k in blokKayitlari) { if (k.Value == silinecek) silinecekKoor.Add(k.Key); }
                foreach (var k in silinecekKoor) { gridMatrisi[k.x, k.y, k.z] = 0; blokKayitlari.Remove(k); }
                Destroy(silinecek);
            }
        }
    }

    void HedefiAnalizEt()
    {
        hedefMatrisi = new int[gridBoyutu.x, gridBoyutu.y, gridBoyutu.z];
        if (hedefYapiParent == null) return;
        foreach (Transform child in hedefYapiParent)
        {
            Vector3Int yon = Vector3Int.RoundToInt(child.right);
            int len = Mathf.RoundToInt(child.localScale.x);
            Vector3 görselOfset = (Vector3)yon * ((len - 1) * 0.5f);
            Vector3 anchorDunya = child.position - görselOfset;
            Vector3Int anchor = new Vector3Int(Mathf.RoundToInt(anchorDunya.x), Mathf.FloorToInt(anchorDunya.y - 0.5f), Mathf.RoundToInt(anchorDunya.z));

            for (int i = 0; i < len; i++)
            {
                Vector3Int p = anchor + (yon * i);
                if (p.x >= 0 && p.x < gridBoyutu.x && p.y >= 0 && p.y < gridBoyutu.y && p.z >= 0 && p.z < gridBoyutu.z) 
                    hedefMatrisi[p.x, p.y, p.z] = 1;
            }
        }
        hedefYapiParent.gameObject.SetActive(false);
    }

    public void CevabiKontrolEt()
    {
        bool hata = false;
        for (int x = 0; x < gridBoyutu.x; x++)
            for (int y = 0; y < gridBoyutu.y; y++)
                for (int z = 0; z < gridBoyutu.z; z++)
                    if (gridMatrisi[x, y, z] != hedefMatrisi[x, y, z]) { hata = true; break; }
        Debug.Log(hata ? "YANLIŞ!" : "TEBRİKLER!");
    }

    void HayaletRenginiDegistir(Color renk) { foreach (MeshRenderer r in aktifHayalet.GetComponentsInChildren<MeshRenderer>()) r.material.color = renk; }
    void HayaletleriGizle() { if (hayalet_1x1) hayalet_1x1.SetActive(false); if (hayalet_1x2) hayalet_1x2.SetActive(false); if (hayalet_1x3) hayalet_1x3.SetActive(false); }
    public void BlokSec_1x1() { HayaletleriGizle(); aktifHayalet = hayalet_1x1; aktifPrefab = prefab_1x1; simdikiUzunluk = 1; HayaletBloguYonet(); }
    public void BlokSec_1x2() { HayaletleriGizle(); aktifHayalet = hayalet_1x2; aktifPrefab = prefab_1x2; simdikiUzunluk = 2; HayaletBloguYonet(); }
    public void BlokSec_1x3() { HayaletleriGizle(); aktifHayalet = hayalet_1x3; aktifPrefab = prefab_1x3; simdikiUzunluk = 3; HayaletBloguYonet(); }
}
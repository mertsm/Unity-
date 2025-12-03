using UnityEngine;

public class KameraSistemi : MonoBehaviour
{
    [Header("Hedef Noktalar (Boş Objeler)")]
    public Transform hedefISO;
    public Transform hedefON;
    public Transform hedefSAG;
    public Transform hedefUST;

    private Transform anaKameraTransform;

    void Start()
    {
        anaKameraTransform = Camera.main.transform;
        GorsunISO(); // Başlangıç açısı
    }

    // YENİ EKLENDİ: Test için klavye kısayolları
    void Update()
    {
        // 1 tuşuna basınca ISO (Çapraz)
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            GorsunISO();
        }
        // 2 tuşuna basınca ÖN
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            GorsunON();
        }
        // 3 tuşuna basınca SAĞ
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            GorsunSAG();
        }
        // 4 tuşuna basınca ÜST
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            GorsunUST();
        }
    }

    public void GorsunISO()
    {
        KamerayiTasi(hedefISO);
    }

    public void GorsunON()
    {
        KamerayiTasi(hedefON);
    }

    public void GorsunSAG()
    {
        KamerayiTasi(hedefSAG);
    }

    public void GorsunUST()
    {
        KamerayiTasi(hedefUST);
    }

    private void KamerayiTasi(Transform yeniHedef)
    {
        anaKameraTransform.position = yeniHedef.position;
        anaKameraTransform.rotation = yeniHedef.rotation;
    }
}
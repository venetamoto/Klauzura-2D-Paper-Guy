using UnityEngine;
using System.Collections;
using Unity.VisualScripting;
using UnityEditor.Build.Reporting;
using System.Net;

public class destrukčníSkript : MonoBehaviour
{
    public GameObject bomba;
    public GameObject bombaAnimace;
    public GameObject zed1;
    public GameObject zed12;
    public GameObject stul;
    public GameObject nuzky;
    public GameObject snura;
    public GameObject zed2;
    public GameObject mrakTransformatorPrefab;
    public GameObject cmarkaTransformatorPrefab;
    public static bool bombaSebrano = false;
    bool nuzkySebrano = false;
    public static bool ustrihnuto = false;
    bool ustrich = false;
    bool POCKEJ = false;
    bool bum = false;

    void Start()
    {
        bombaSebrano = false;
        nuzkySebrano = false;
        ustrihnuto = false;
        POCKEJ = false;
        bum = false;
    }

    void Update()
    {
        if (ustrihnuto == true)
        {
            GameObject SMAZTO3 = GameObject.Find("zeď 2");
            StartCoroutine(ČAS());
            if (POCKEJ == true) Destroy(SMAZTO3);
        }
        if (bum == true)
        {
            Destroy(zed1);
            Destroy(zed12);
            Destroy(stul);
            Instantiate(mrakTransformatorPrefab, transform.position, transform.rotation);
            Destroy(cmarkaTransformatorPrefab);
            GameObject SMAZTO4 = GameObject.Find("exploze(Clone)");
            Destroy(SMAZTO4);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.name == "bomba")
        {
            bombaSebrano = true;
            Destroy(bomba);
        }
        if ((collision.gameObject.name == "zeď1" || collision.gameObject.name == "zeď1(2)") && bombaSebrano == true)
        {
            Instantiate(bombaAnimace, transform.position, transform.rotation);
            StartCoroutine(Výbuch());
        }

        if (collision.gameObject.name == "nůžky")
        {
            GameObject SMAZTO = GameObject.Find("nůžky");
            Destroy(SMAZTO);
            nuzkySebrano = true;
        }
        if (collision.gameObject.name == "šňůra" && nuzkySebrano == true)
        {
            StartCoroutine(Ústřich());
            if (ustrich == true)
            {
                GameObject SMAZTO2 = GameObject.Find("šňůra");
                Destroy(SMAZTO2);
                ustrihnuto = true;
            }
        }
    } 

    public IEnumerator ČAS()
    {
        yield return new WaitForSeconds(1f);
        POCKEJ = true;
    }
    public IEnumerator Výbuch()
    {
        yield return new WaitForSeconds(1.9f);
        bum = true;
    }
    public IEnumerator Ústřich()
    {
        yield return new WaitForSeconds(1f);
        ustrich = true;
    }
}
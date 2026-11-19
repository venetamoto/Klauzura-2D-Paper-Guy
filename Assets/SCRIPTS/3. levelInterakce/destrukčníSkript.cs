using UnityEngine;
using System.Collections;

public class destrukčníSkript : MonoBehaviour
{
    public GameObject bomba;
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
    bool POCKEJ = false;

    void Start()
    {
        bombaSebrano = false;
        nuzkySebrano = false;
        ustrihnuto = false;
        POCKEJ = false;
    }

    void Update()
    {
        if (ustrihnuto == true)
        {
            GameObject SMAZTO3 = GameObject.Find("zeď 2");
            StartCoroutine(ČAS());
            if (POCKEJ == true) Destroy(SMAZTO3);
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
            Destroy(zed1);
            Destroy(zed12);
            Destroy(stul);
            Instantiate(mrakTransformatorPrefab, transform.position, transform.rotation);
            Destroy(cmarkaTransformatorPrefab);
        }

        if (collision.gameObject.name == "nůžky")
        {
            GameObject SMAZTO = GameObject.Find("nůžky");
            Destroy(SMAZTO);
            nuzkySebrano = true;
        }
        if (collision.gameObject.name == "šňůra" && nuzkySebrano == true)
        {
            GameObject SMAZTO2 = GameObject.Find("šňůra");
            Destroy(SMAZTO2);
            ustrihnuto = true;
        }
    } 

    public IEnumerator ČAS()
    {
        yield return new WaitForSeconds(1f);
        POCKEJ = true;
    }
}

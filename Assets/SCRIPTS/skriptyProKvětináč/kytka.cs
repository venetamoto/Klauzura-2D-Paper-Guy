using Unity.Collections;
using UnityEngine;

public class kytka : MonoBehaviour
{
    void Update()
    {
        if (květináč.kytka3prefab == true) transform.position = new Vector3(15.34f, -3.35f, 0f);
        if (květináč.kytka4prefab == true) transform.position = new Vector3(15.34f, 4.72f, 0f);
    }
}

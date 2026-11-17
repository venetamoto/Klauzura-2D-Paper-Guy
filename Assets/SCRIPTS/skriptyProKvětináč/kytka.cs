using Unity.Collections;
using UnityEngine;

public class kytka : MonoBehaviour
{
    [SerializeField]float yposition = 1f;
    void Update()
    {
        if (květináč.kytka3prefab == true) transform.position = new Vector3(15.34f, -3.35f, 0f);
        else if (květináč.kytka4prefab == true) transform.position = new Vector3(15.34f, yposition, 0f);
    }
}

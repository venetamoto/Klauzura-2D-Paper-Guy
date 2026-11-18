using UnityEngine;
using TMPro;

public class ukazateljídla : MonoBehaviour
{
    public TextMeshPro textik;

    void Update()
    {
        textik.text = květináč.prijateJidlo + "/11 jídla";
    }
}

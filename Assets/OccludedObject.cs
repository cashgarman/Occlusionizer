using UnityEngine;

public class OccludedObject : MonoBehaviour
{
    public Color color;

    void Start()
    {
        GetComponent<MeshRenderer>().material.color = color;
    }
}

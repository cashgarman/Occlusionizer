using UnityEngine;

public class OccludedObject : MonoBehaviour
{
    public Color color;

    void Start()
    {
        VisibilityCalculator.AddObject(this, color);
        GetComponent<MeshRenderer>().material.color = color;
    }
}

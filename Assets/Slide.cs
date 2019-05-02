using UnityEngine;

public class Slide : MonoBehaviour
{
    public Vector3 direction;
    public float speed;

    void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
    }
}

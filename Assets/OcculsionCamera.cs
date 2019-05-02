using UnityEngine;

public class OcculsionCamera : MonoBehaviour
{
    public Shader occulsionShader;

    void Start()
    {
//        GetComponent<Camera>().SetReplacementShader(occulsionShader, "occlusion");
        GetComponent<Camera>().depthTextureMode = DepthTextureMode.Depth;
    }
}

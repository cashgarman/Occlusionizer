using UnityEngine;

public class VisibilityCalculator : MonoBehaviour
{
    public ComputeShader visibilityComputeShader;
    public RenderTexture occludedRenderTarget;

    private ComputeBuffer visiblePixelsBuffer;
    private ComputeBuffer countBuffer;
    private int kernal;
    private uint xDimension;
    private uint yDimension;

    void Start()
    {
//        occludedRenderTarget = new RenderTexture(Screen.width, Screen.height, 24);
        
        kernal = visibilityComputeShader.FindKernel("CountVisiblePixels");
        visiblePixelsBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Counter);
        countBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.IndirectArguments);
        visibilityComputeShader.SetBuffer(kernal, "numVisiblePixels", visiblePixelsBuffer);
        visibilityComputeShader.SetTexture(kernal, "occludedTexture", occludedRenderTarget);
        visibilityComputeShader.GetKernelThreadGroupSizes(kernal, out xDimension, out yDimension, out _);
    }

    void Update()
    {
        visiblePixelsBuffer.SetCounterValue(0);
        
        visibilityComputeShader.Dispatch(kernal, (int)xDimension, (int)yDimension, 1);
        
        var count = new[] {0};
        ComputeBuffer.CopyCount(visiblePixelsBuffer, countBuffer, 0);
        countBuffer.GetData(count);
        
        Debug.Log($"Number of visible pixels: {count[0]}");
    }
}

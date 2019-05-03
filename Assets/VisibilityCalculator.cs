using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class VisibilityCalculator : MonoBehaviour
{
    public string computeShaderName;
    public RenderTexture occludedRenderTarget;
    public RenderTexture unoccludedRenderTarget;

    private ComputeBuffer visiblePixelsBuffer;
    private ComputeBuffer visibleCountBuffer;
    
    private ComputeBuffer referencePixelsBuffer;
    private ComputeBuffer referenceCountBuffer;
    
    private int visibleKernal;
    private int referenceKernal;
    private uint xDimension;
    private uint yDimension;
    private static readonly Dictionary<Color, OccludedObject> occludedObjects = new Dictionary<Color, OccludedObject>();

    private ComputeShader occludedShader;
    private ComputeShader unoccludedShader;
    public TextMeshProUGUI debugText;

    void Start()
    {
        occludedShader = (ComputeShader)Instantiate(Resources.Load(computeShaderName));
        unoccludedShader = (ComputeShader)Instantiate(Resources.Load(computeShaderName));
        
        // Initialize the visibility compute shader
        
        
        // Initialize the reference compute shader
        
    }

    void Update()
    {
        // Reset the count of pixels
        
        // Calculate the amount of visible pixels
        visibleKernal = occludedShader.FindKernel("CountVisiblePixels");
        visiblePixelsBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Counter);
        visiblePixelsBuffer.SetCounterValue(0);
        visibleCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.IndirectArguments);
        occludedShader.SetBuffer(visibleKernal, "numVisiblePixels", visiblePixelsBuffer);
        occludedShader.SetTexture(visibleKernal, "occludedBuffer", occludedRenderTarget);
        occludedShader.GetKernelThreadGroupSizes(visibleKernal, out xDimension, out yDimension, out _);
        occludedShader.Dispatch(visibleKernal, (int)xDimension, (int)yDimension, 1);
        
        // Calculate the total amount of pixels
        referenceKernal = unoccludedShader.FindKernel("CountReferencePixels");
        referencePixelsBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Counter);
        referencePixelsBuffer.SetCounterValue(0);
        referenceCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);
        unoccludedShader.SetBuffer(referenceKernal, "numReferencePixels", referencePixelsBuffer);
        unoccludedShader.SetTexture(referenceKernal, "unoccludedBuffer", unoccludedRenderTarget);
        unoccludedShader.GetKernelThreadGroupSizes(referenceKernal, out xDimension, out yDimension, out _);
        unoccludedShader.Dispatch(referenceKernal, (int)xDimension, (int)yDimension, 1);
        
        // Get the count of visible pixels
        var visiblePixels = new[] {0};
        ComputeBuffer.CopyCount(visiblePixelsBuffer, visibleCountBuffer, 0);
        visibleCountBuffer.GetData(visiblePixels);
        
        Debug.Log($"Number of visible pixels: {visiblePixels[0]}");

        // Get the total number of pixels
        var referencePixels = new[] {0};
        ComputeBuffer.CopyCount(referencePixelsBuffer, referenceCountBuffer, 0);
        referenceCountBuffer.GetData(referencePixels);
        
        Debug.Log($"Number of total pixels: {referencePixels[0]}");

        debugText.text = $"Visible: {visiblePixels[0]}, Total: {referencePixels[0]}";
    }

    public static void AddObject(OccludedObject obj, Color color)
    {
        occludedObjects.Add(color, obj);
    }

    private void OnDestroy()
    {
        visiblePixelsBuffer.Release();
        visibleCountBuffer.Release();
    
        referencePixelsBuffer.Release();
        referenceCountBuffer.Release();
    }
}

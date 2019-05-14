using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class VisibilityCalculator : MonoBehaviour
{
    public string occludedComputeShaderName;
    public string unoccludedComputeShaderName;
    
    public RenderTexture occludedRenderTarget;
    public RenderTexture unoccludedRenderTarget;

    private ComputeBuffer visiblePixelsBuffer;
    private ComputeBuffer visiblePixelsCountBuffer;
    
    private ComputeBuffer allPixelsBuffer;
    private ComputeBuffer allPixelsCountBuffer;
    
    private int visiblePixelsKernal;
    private int allPixelsKernal;
    private uint xDimension;
    private uint yDimension;
    private static readonly Dictionary<Color, OccludedObject> occludedObjects = new Dictionary<Color, OccludedObject>();

    private ComputeShader occludedShader;
    private ComputeShader unoccludedShader;
    public TextMeshProUGUI debugText;

    public MeshRenderer[] carParts;

    void Start()
    {
        occludedShader = (ComputeShader)Instantiate(Resources.Load(occludedComputeShaderName));
        unoccludedShader = (ComputeShader)Instantiate(Resources.Load(unoccludedComputeShaderName));
        
        // Initialize the visibility compute shader
        visiblePixelsKernal = occludedShader.FindKernel("CountVisiblePixels");
        visiblePixelsBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Counter);
        visiblePixelsCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.IndirectArguments);
        
        // Initialize the reference compute shader
        allPixelsKernal = unoccludedShader.FindKernel("CountAllPixels");
        allPixelsBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Counter);
        allPixelsCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);
        
        Update();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
            
        // Reset the count of pixels
        visiblePixelsBuffer.SetCounterValue(0);
        allPixelsBuffer.SetCounterValue(0);

        // Calculate the amount of visible pixels
        occludedShader.SetBuffer(visiblePixelsKernal, "numPixels", visiblePixelsBuffer);
        occludedShader.SetTexture(visiblePixelsKernal, "occludedBuffer", occludedRenderTarget);
        occludedShader.GetKernelThreadGroupSizes(visiblePixelsKernal, out xDimension, out yDimension, out _);
        occludedShader.Dispatch(visiblePixelsKernal, (int)xDimension, (int)yDimension, 1);
        
        // Calculate the total amount of pixels
        unoccludedShader.SetBuffer(allPixelsKernal, "numPixels", allPixelsBuffer);
        unoccludedShader.SetTexture(allPixelsKernal, "unoccludedBuffer", unoccludedRenderTarget);
        unoccludedShader.GetKernelThreadGroupSizes(allPixelsKernal, out xDimension, out yDimension, out _);
        unoccludedShader.Dispatch(allPixelsKernal, (int)xDimension, (int)yDimension, 1);
        
        // Get the count of visible pixels
        var visiblePixels = new[] {0};
        ComputeBuffer.CopyCount(visiblePixelsBuffer, visiblePixelsCountBuffer, 0);
        visiblePixelsCountBuffer.GetData(visiblePixels);
        
        Debug.Log($"Number of visible pixels: {visiblePixels[0]}");

        // Get the total number of pixels
        var allPixels = new[] {0};
        ComputeBuffer.CopyCount(allPixelsBuffer, allPixelsCountBuffer, 0);
        allPixelsCountBuffer.GetData(allPixels);    
        
        Debug.Log($"Number of total pixels: {allPixels[0]}");

        var amountVisible = (float) visiblePixels[0] / (float) allPixels[0];
        debugText.text = $"Percent Visible: {amountVisible * 100f:F0}%";

        foreach (var carPart in carParts)
        {
            carPart.material.color = Color.Lerp(Color.red, Color.green, amountVisible);
        }
    }

    public static void AddObject(OccludedObject obj, Color color)
    {
        occludedObjects.Add(color, obj);
    }

    private void OnDestroy()
    {
        visiblePixelsBuffer.Release();
        visiblePixelsCountBuffer.Release();
    
        allPixelsBuffer.Release();
        allPixelsCountBuffer.Release();
    }
}

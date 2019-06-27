using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrayShader : MonoBehaviour
{
    public ComputeShader shader;
    public int TexResolution = 2048;
    public Texture inputTexture;
    [HideInInspector]
    public float average_gray;
    Renderer rend;
    [HideInInspector]
    public RenderTexture myRt;
    RenderTexture tempRt;   
    ComputeBuffer totalGrayBuffer;

    int THREADSIZE = 8;
    bool pingpong;
    
    
    // Start is called before the first frame update
    void Start()
    {
        myRt = new RenderTexture(TexResolution, TexResolution, 24);
        myRt.enableRandomWrite = true;
        myRt.Create();

        tempRt = new RenderTexture(TexResolution, TexResolution, 24, RenderTextureFormat.ARGBFloat);
        tempRt.enableRandomWrite = true;
        tempRt.Create();

        rend = GetComponent<Renderer>();
        rend.enabled = true;

        totalGrayBuffer = new ComputeBuffer(1, sizeof(float), ComputeBufferType.Default);

        UpdateTextureFromCompute();
    }

    private void MipCompute(int stripNum)
    {
        int SUM_Handle = shader.FindKernel("Sum");
        pingpong = !pingpong;
        shader.SetBool("pingpong", pingpong);
        int max_offset = (int)Mathf.Min(THREADSIZE,TexResolution*THREADSIZE/stripNum);
        shader.SetInt("max_offset", max_offset);
        int group1D = (int)Mathf.Max(THREADSIZE, TexResolution / stripNum);
        shader.Dispatch(SUM_Handle, group1D, group1D, 1);
    }

    private float GetTotalGray()
    {
        int SUM_Handle = shader.FindKernel("Sum");

        shader.SetTexture(SUM_Handle, "TempBuffer", tempRt);
        shader.SetBuffer(SUM_Handle, "TotalGrayBuffer", totalGrayBuffer);
        pingpong = false;
        int mip_level = THREADSIZE;
        while (mip_level<=TexResolution)
        { 
            MipCompute(mip_level);
            mip_level *= THREADSIZE;
        }
        MipCompute(mip_level);

        float[] totalGray = new float[1] ;
        totalGrayBuffer.GetData(totalGray);
        float totalGraySum = totalGray[0];
        totalGraySum /= (TexResolution * TexResolution);
        return totalGraySum;
    }
    // Update is called once per frame
    public void UpdateTextureFromCompute()
    {
        int kernelHandle = shader.FindKernel("CSMain");
   
        shader.SetInt("textureWidth", myRt.width);
        shader.SetInt("textureHeight", myRt.height);
        shader.SetTexture(kernelHandle, "Result", myRt);

        shader.SetTexture(kernelHandle, "TempBuffer", tempRt);
        if (inputTexture)
            shader.SetTexture(kernelHandle, "Input", inputTexture);
        shader.Dispatch(kernelHandle, TexResolution / 8, TexResolution / 8, 1);

        average_gray = GetTotalGray();

        Debug.Log("input image averay gray:" + average_gray);
        rend.material.SetTexture("_MainTex", myRt);
    }


    void Update()
    {
        //UpdateTextureFromCompute();
    }

    void OnDestroy()
    {
        myRt.Release();
        totalGrayBuffer.Release();
    }
}

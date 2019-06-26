using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine;
using Unity.Collections.LowLevel.Unsafe;



public class Knitting : MonoBehaviour
{
    public ComputeShader shader;
   
    public int PinsNumber = 315;
    public int PathNumber = 2000;
    public GameObject inputTextureObject;
    RenderTexture tempRt;
    Renderer rend;
    RenderTexture myRt;
    ComputeBuffer pinsBuffer;
    ComputeBuffer pathBuffer;
    GrayShader inputImage;
    ComputeBuffer totalGrayBuffer;
    int THREADSIZE = 8;
    bool pingpong;
    float average_gray;
    float error;
    int TexResolution = 4;

    public bool running = true;
    bool writefile = true;
    public struct PinNode
    {
        public int x;
        public int y;
    }

    PinNode[] nodes;
    int[] Path;
    int[] CandicatePath;
    int count;


    private void InitNodes()
    {
        nodes = new PinNode[PinsNumber];
        float delta_angle = 2 * 3.1415f / PinsNumber;
        float radius = TexResolution * 0.5f;
        for (int i =0; i<PinsNumber;i++)
        {
            float x = radius + radius * Mathf.Sin(i * delta_angle)  ;
            float y = radius + radius * Mathf.Cos(i * delta_angle)  ;
            nodes[i] = new PinNode();
            nodes[i].x = (int)x;
            nodes[i].y = (int)y;
        }
        int NodeSize = System.Runtime.InteropServices.Marshal.SizeOf(typeof(PinNode));
      //  int NodeSize = sizeof(Node);

        Debug.Log("NodeSize:"+NodeSize);
        pinsBuffer = new ComputeBuffer(PinsNumber, NodeSize, ComputeBufferType.Default);
        pinsBuffer.SetData(nodes);
    }

    private void PathToCandicate()
    {
        CandicatePath = (int [])Path.Clone();
    }

    private void CandicateToPath()
    {
        Path = (int[])CandicatePath.Clone();
    }

    private void InitPath()
    {
        Path = new int[PathNumber];
        CandicatePath = new int[PathNumber];
        for (int i =0;i< PathNumber; i++)
        {
            Path[i] = Random.Range(0, PinsNumber);
        }
        PathToCandicate();
        int PathSize = System.Runtime.InteropServices.Marshal.SizeOf(typeof(int));
        pathBuffer = new ComputeBuffer(PathNumber, PathSize, ComputeBufferType.Default);
        pathBuffer.SetData(Path);
    }

    private void RandomPath()
    {
        PathToCandicate();
        int randonNode = Random.Range(0, PathNumber);
        CandicatePath[randonNode] = Random.Range(0, PinsNumber);
        pathBuffer.SetData(CandicatePath);
    }

    // Start is called before the first frame update
    void Start()
    {
        inputImage = inputTextureObject.GetComponent<GrayShader>();
        TexResolution = inputImage.TexResolution;

        myRt = new RenderTexture(TexResolution, TexResolution, 24);
        myRt.enableRandomWrite = true;
        myRt.isPowerOfTwo = true;
        myRt.filterMode = FilterMode.Trilinear;
        myRt.useMipMap = true;
        myRt.autoGenerateMips = false;
        myRt.Create();

        tempRt = new RenderTexture(TexResolution, TexResolution, 24, RenderTextureFormat.ARGBFloat);
        tempRt.enableRandomWrite = true;
        tempRt.Create();

        rend = GetComponent<Renderer>();
        rend.enabled = true;

        totalGrayBuffer = new ComputeBuffer(1, sizeof(float), ComputeBufferType.Default);

 
        error = 99999.0f;
        count = 0;
        InitNodes();
        InitPath();
        running = true;
    }

    private void MipCompute(int stripNum)
    {
        int SUM_Handle = shader.FindKernel("Sum");
        pingpong = !pingpong;
        shader.SetBool("pingpong", pingpong);
        int max_offset = (int)Mathf.Min(THREADSIZE, TexResolution * THREADSIZE / stripNum);
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
        while (mip_level <= TexResolution)
        {
            MipCompute(mip_level);
            mip_level *= THREADSIZE;
        }
        MipCompute(mip_level);

        float[] totalGray = new float[1];
        totalGrayBuffer.GetData(totalGray);
        float totalGraySum = totalGray[0];
        totalGraySum /= (TexResolution * TexResolution);
        return totalGraySum;
    }

    public void UpdateSegmentsDrawing()
    {

        int kernelHandle = shader.FindKernel("CSMain");
        int clearHandle = shader.FindKernel("Clear");
        int copyHandle = shader.FindKernel("Copy");
        int errorHandle = shader.FindKernel("Error");
        int blureHandle = shader.FindKernel("Blur");


        shader.SetInt("TextureSize", myRt.width);
        shader.SetTexture(clearHandle, "Result", myRt);
        shader.Dispatch(clearHandle, myRt.width / 8, myRt.width / 8, 1);

        //draw
        RandomPath();
        shader.SetBuffer(kernelHandle, "PinBuffer", pinsBuffer);
        shader.SetBuffer(kernelHandle, "PathBuffer", pathBuffer);
        shader.SetInt("UpLimit", PathNumber);
        shader.SetTexture(kernelHandle, "Result", myRt);
        shader.Dispatch(kernelHandle, PathNumber / 16, 1, 1);


        //blur
        shader.SetTexture(blureHandle, "Result", myRt);
        shader.Dispatch(blureHandle, myRt.width / 8, myRt.width / 8, 1);

        //copy
        shader.SetTexture(copyHandle, "TempBuffer", tempRt);
        shader.SetTexture(copyHandle, "Result", myRt);
        shader.Dispatch(copyHandle, myRt.width / 8, myRt.width / 8, 1);
        average_gray = GetTotalGray();

        //Debug.Log("input image2 gray:" + average_gray );
        //compute error
        shader.SetFloat("soureImageGray", inputImage.average_gray);
        shader.SetFloat("currentImageGray", average_gray);
        shader.SetTexture(errorHandle, "SourceImage", inputImage.myRt);
        shader.SetTexture(errorHandle, "Result", myRt);
        shader.SetTexture(errorHandle, "TempBuffer", tempRt);
        shader.Dispatch(errorHandle, myRt.width / 8, myRt.width / 8, 1);

        float current_error = GetTotalGray();
        Debug.Log("input image2 error:" + current_error+"previews error" + error);

        if (current_error< error)
        {
            count++;
            Debug.Log("update:"+ count);

            error = current_error;
            CandicateToPath();
        }
        myRt.GenerateMips();
        rend.material.SetTexture("_MainTex", myRt);
    }

    // Update is called once per frame
    void Update()
    {
        if(running)
        { 
            UpdateSegmentsDrawing();
            writefile = true;
        }
        else
        {
            if(writefile)
            {
                StreamWriter sw = new StreamWriter("segments.txt");
                foreach (int segmentid in Path)
                {
                    sw.Write("," + segmentid);
                }
                sw.WriteLine("");
                sw.WriteLine("finished");
                sw.Close();
                writefile = false;
            }
        }
    }

    void OnDestroy()
    {
        myRt.Release();
        pathBuffer.Release();
        pinsBuffer.Release();
    }
}

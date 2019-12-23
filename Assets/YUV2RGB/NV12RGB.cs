using System.IO;
using UnityEngine;

public class NV12RGB : MonoBehaviour
{
    public Material material;
    private int width = 320;
    private int height = 240;

    // Start is called before the first frame update
    void Start()
    {
        byte[] yuvData = File.ReadAllBytes(Application.dataPath + "/YUV2RGB/nv12.yuv");

        Debug.Log("yuvData.Length:" + yuvData.Length);
        //对于320*240分辨率的NV12格式
        //第1部分是Y数据
        byte[] yData = new byte[width * height];
        //第2部分是UV数据
        byte[] uvData = new byte[width * height *3 / 4]; 

        for (int i = 0; i < yuvData.Length; i++)
        {
            if (i < yData.Length)
            {
                yData[i] = yuvData[i];
            }
            if (i>= yData.Length)
            {
                uvData[i - yData.Length] = yuvData[i];
            } 
        }
        Debug.Log("yData[0]:" + yData[0]);
        Debug.Log("yData.Length:" + yData.Length);

        Debug.Log("uData[0]:" + uvData[0]);
        Debug.Log("uData.Length:" + uvData.Length); 

        Texture2D yTexture = new Texture2D(width, height, TextureFormat.Alpha8, false);
        yTexture.LoadRawTextureData(yData);
        yTexture.Apply();
        Debug.Log("yTexture[0]:" + yTexture.GetRawTextureData()[0]);
        Color colorY = yTexture.GetPixel(0, 0);
        Debug.Log("yTexture color a:" + colorY.a);

        Texture2D uvTexture = new Texture2D(width / 2, height / 2, TextureFormat.RGB24, false);
        uvTexture.LoadRawTextureData(uvData);
        uvTexture.Apply();
        Debug.Log("uTexture[0]:" + uvTexture.GetRawTextureData()[0]);
        Color colorUV = uvTexture.GetPixel(0, 0);
        Debug.Log("uTexture color a:" + colorUV.a); 

        material.SetTexture("_YTex", yTexture);
        material.SetTexture("_UVTex", uvTexture); 
    }
}
// YUV420
//YUV420p: I420、YV12
//YUV420sp: NV12、NV21
//I420格式
//一个6*4的图像，四种像素格式的存储方式如下：
//Y Y Y Y Y Y      Y Y Y Y Y Y      Y Y Y Y Y Y      Y Y Y Y Y Y
//Y Y Y Y Y Y      Y Y Y Y Y Y      Y Y Y Y Y Y      Y Y Y Y Y Y
//Y Y Y Y Y Y      Y Y Y Y Y Y      Y Y Y Y Y Y      Y Y Y Y Y Y
//Y Y Y Y Y Y      Y Y Y Y Y Y      Y Y Y Y Y Y      Y Y Y Y Y Y
//U U U U U U      V V V V V V      U V U V U V      V U V U V U
//V V V V V V      U U U U U U      U V U V U V      V U V U V U
// - I420 -          - YV12 -         - NV12 -         - NV21 - 
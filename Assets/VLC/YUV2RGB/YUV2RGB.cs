using System.IO;
using UnityEngine;

public class YUV2RGB : MonoBehaviour
{
    public Material material;
    private int width = 320;
    private int height = 240;

    // Start is called before the first frame update
    void Start()
    {
        byte[] yuvData = File.ReadAllBytes(Application.dataPath + "/YUV2RGB/yuv420.yuv");

        Debug.Log("yuvData.Length:" + yuvData.Length);
        //对于320*240分辨率的I420格式
        //第1部分是Y数据
        byte[] yData = new byte[width * height];
        //第2部分是U数据
        byte[] uData = new byte[width * height / 4];
        //第三部分是V数据
        byte[] vData = new byte[width * height / 4];

        for (int i = 0; i < yuvData.Length; i++)
        {
            if (i < yData.Length)
            {
                yData[i] = yuvData[i];
            }
            else if (i < yData.Length + uData.Length)
            {
                uData[i - yData.Length] = yuvData[i];
            }
            else
            {
                vData[i - yData.Length - uData.Length] = yuvData[i];
            }
        }
        Debug.Log("yData[0]:" + yData[0]);
        Debug.Log("yData.Length:" + yData.Length);

        Debug.Log("uData[0]:" + uData[0]);
        Debug.Log("uData.Length:" + uData.Length);

        Debug.Log("vData[0]:" + vData[0]);
        Debug.Log("vData.Length:" + vData.Length);

        Texture2D yTexture = new Texture2D(width, height, TextureFormat.Alpha8, false);
        yTexture.LoadRawTextureData(yData);
        yTexture.Apply();
        Debug.Log("yTexture[0]:" + yTexture.GetRawTextureData()[0]);
        Color colorY = yTexture.GetPixel(0, 0);
        Debug.Log("yTexture color a:" + colorY.a);

        Texture2D uTexture = new Texture2D(width / 4, height / 4, TextureFormat.Alpha8, false);
        uTexture.LoadRawTextureData(uData);
        uTexture.Apply();
        Debug.Log("uTexture[0]:" + uTexture.GetRawTextureData()[0]);
        Color colorU = uTexture.GetPixel(0, 0);
        Debug.Log("uTexture color a:" + colorU.a);

        Texture2D vTexture = new Texture2D(width / 4, height / 4, TextureFormat.Alpha8, false);
        vTexture.LoadRawTextureData(vData);
        vTexture.Apply();
        Debug.Log("vTexture[0]:" + vTexture.GetRawTextureData()[0]);
        Color colorV = vTexture.GetPixel(0, 0);
        Debug.Log("vTexture color a:" + colorV.a);

        material.SetTexture("_YTex", yTexture);
        material.SetTexture("_UTex", uTexture);
        material.SetTexture("_VTex", vTexture);
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
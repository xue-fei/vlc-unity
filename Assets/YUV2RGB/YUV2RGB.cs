using System.IO;
using UnityEngine;

public class YUV2RGB : MonoBehaviour
{
    public Material material;

    // Start is called before the first frame update
    void Start()
    {
        byte[] yuvData = File.ReadAllBytes(Application.dataPath + "/YUV2RGB/yuv420.yuv");
        Debug.Log("yuvData.Length:" + yuvData.Length);
        byte[] yData = new byte[320 * 240];
        for (int i = 0; i < yData.Length; i++)
        {
            yData[i] = yuvData[i];
        }
        Debug.Log("yData[0]:" + yData[0]);
        Debug.Log("yData.Length:" + yData.Length);

        byte[] uData = new byte[320 * 240 / 4];
        byte[] vData = new byte[320 * 240 / 4];
        int index = yData.Length;
        int uIndex = 0;
        int vIndex = 0;
        for (int i = yData.Length; i < yuvData.Length; i++)
        {
            if (i % 2 == 0)
            {
                vData[vIndex] = yuvData[index];
                vIndex++;
            }
            else
            {
                uData[uIndex] = yuvData[index];
                uIndex++; 
            }
            index++;
        }

        Debug.Log("uData[0]:" + uData[0]);
        Debug.Log("uData.Length:" + uData.Length);

        Debug.Log("vData[0]:" + vData[0]);
        Debug.Log("vData.Length:" + vData.Length);

        Texture2D yTexture = new Texture2D(320, 240, TextureFormat.Alpha8, false);
        yTexture.LoadRawTextureData(yData);
        yTexture.Apply();
        Debug.Log("yTexture[0]:" + yTexture.GetRawTextureData()[0]);
        Color colorY = yTexture.GetPixel(0, 0);
        Debug.Log("yTexture color a:" + colorY.a);

        Texture2D uTexture = new Texture2D(320 / 4, 240 / 4, TextureFormat.Alpha8, false);
        uTexture.LoadRawTextureData(uData);
        uTexture.Apply();
        Debug.Log("uTexture[0]:" + uTexture.GetRawTextureData()[0]);
        Color colorU = uTexture.GetPixel(0, 0);
        Debug.Log("uTexture color a:" + colorU.a);

        Texture2D vTexture = new Texture2D(320 / 4, 240 / 4, TextureFormat.Alpha8, false);
        vTexture.LoadRawTextureData(vData);
        vTexture.Apply();
        Debug.Log("vTexture[0]:" + vTexture.GetRawTextureData()[0]);
        Color colorV = vTexture.GetPixel(0, 0);
        Debug.Log("vTexture color a:" + colorV.a);

        material.SetTexture("_YTex", yTexture);
        material.SetTexture("_UTex", uTexture);
        material.SetTexture("_VTex", vTexture);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
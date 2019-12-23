using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class I420ToNV12 : MonoBehaviour
{
    private int width = 320;
    private int height = 240;
    // Use this for initialization
    void Start()
    {
        byte[] yuvData = File.ReadAllBytes(Application.dataPath + "/YUV2RGB/yuv420.yuv");
        Debug.Log("yuvData.Length:" + yuvData.Length);
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
        int j = 0;
        int k = 0; 
        //混合UV
        for (int i = yData.Length; i < uData.Length+ vData.Length; i++)
        {
            if(i%2==0)
            {
                yuvData[i] = uData[j];
                j++;
            }
            else
            {
                yuvData[i] = vData[k];
                k++;
            }
        }
        File.WriteAllBytes(Application.dataPath + "/YUV2RGB/nv12.yuv", yuvData);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
//YUV420
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
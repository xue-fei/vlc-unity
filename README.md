#vlc-unity

Unity调用VLC的库解码视频显示到3D场景中，支持常见本地视频和rtmp、rtsp等视频流。  

本工程仅为Demo，仅支持Windows/Linux/Android平台，做产品请各位商店寻找UMP插件   

Win和Linux选择x86_64   Android选ARM64   

Linux相关  
sudo apt install vlc  
修改/etc/profile，在文件末尾加上两行  
LD_LIBRARY_PATH=./  
export LD_LIBRARY_PATH  
source /etc/profile  

示例是播放湖南卫视的rtmp视频流——此视频流地址已不可用
![Image text](https://images.gitee.com/uploads/images/2019/0626/100814_893f7478_80624.jpeg)  

Android arm64测试  
![输入图片说明](Android%E6%B5%8B%E8%AF%95.jpg)

本地rtsp测试     
先启动mediamtx    
再用ffmpeg推流   
ffmpeg -re -i G:\MyProject\vlc-unity\Assets\StreamingAssets\test.mp4 -c copy -rtsp_transport tcp -f rtsp rtsp://127.0.0.1:8554/stream    
最后再播放   
![输入图片说明](%E6%9C%AC%E5%9C%B0rtsp%E6%B5%8B%E8%AF%95.png)

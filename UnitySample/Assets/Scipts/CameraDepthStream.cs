using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

#if ENABLE_WINMD_SUPPORT
using HL2UnityPlugin;
#endif

public class CameraDepthStream : MonoBehaviour
{
#if ENABLE_WINMD_SUPPORT
    HL2ResearchMode researchMode;
#endif
    
    private byte[] _longDepthFrameData = null;
    private ushort[] _testLongDepthFrameData = null;
    private Texture2D _longDepthTexture = null;
    private byte[] _longAbImageFrameData = null;
    private ushort[] _testLongABFrameData = null;
    private Texture2D _longABImageTexture = null;
    private int _numberOfImages = 0;
    public SendBytesToServer client;

    private void Awake()
    {
        _longDepthTexture = new Texture2D(320, 288, TextureFormat.R16, false);
        _longABImageTexture = new Texture2D(320, 288, TextureFormat.R16, false);
        
    }
    
    private void Start()
    {
#if ENABLE_WINMD_SUPPORT
        researchMode = new HL2ResearchMode();
        researchMode.InitializeLongDepthSensor();
        researchMode.InitializeSpatialCamerasFront();
        researchMode.StartLongDepthSensorLoop(false);
        researchMode.StartSpatialCamerasFrontLoop();
#if WINDOWS_UWP
        client.StartConnection();
#endif
#endif
    }

    private void LateUpdate()
    {
        GetDepthAndABImages();
#if WINDOWS_UWP
        if(_longDepthFrameData != null && _longAbImageFrameData != null)
        {
            //_longDepthTexture.LoadRawTextureData(_longDepthFrameData);
            //var bytes = _longDepthTexture.EncodeToPNG();

            client.Publish(_longDepthFrameData, _longAbImageFrameData);
        }
#endif
        //ExportDepthAndABImages();
    }

    private void GetDepthAndABImages()
    {
#if ENABLE_WINMD_SUPPORT
        if(researchMode.LongDepthMapTextureUpdated())
        {
            byte[] frameTexture = researchMode.GetLongDepthMapTextureBuffer();
            if(frameTexture.Length > 0)
            {
                if(_longDepthFrameData == null)
                {
                    _longDepthFrameData = frameTexture;
                }
                else
                {
                    System.Buffer.BlockCopy(frameTexture, 0, _longDepthFrameData, 0, _longDepthFrameData.Length);
                }
            }
        }
        
        if (researchMode.LongAbImageTextureUpdated())
        {  
            byte[] frameTexture = researchMode.GetLongAbImageTextureBuffer();
            if (frameTexture.Length > 0)
            {
                _longAbImageFrameData = frameTexture;
            }
            else
            {
                System.Buffer.BlockCopy(frameTexture, 0, _longAbImageFrameData, 0, _longAbImageFrameData.Length);
            }
        }
#endif
    }

    private void ExportDepthAndABImages()
    {
#if ENABLE_WINMD_SUPPORT
        var ab = researchMode.GetLongAbImageBuffer();
        var depth = researchMode.GetLongDepthMapBuffer();
#if WINDOWS_UWP
        if(client != null)
        {
            //client.SendUINT16Async(ab, depth);
        }
#endif
#endif
        /*
        if (_numberOfImages < 100)
        {
            if (_longDepthFrameData != null)
            {
                _longDepthTexture.LoadRawTextureData(_longDepthFrameData);
                var bytes = _longDepthTexture.EncodeToPNG();
                var dirPath = Application.persistentDataPath + "/SaveImages/";
                if(!Directory.Exists(dirPath)) 
                {
                    Directory.CreateDirectory(dirPath);
                }
                
                File.WriteAllBytes(dirPath + "Depth" + _numberOfImages + ".png", bytes);
            }

            if (_longDepthFrameData != null)
            {
                _longABImageTexture.LoadRawTextureData(_longAbImageFrameData);
                var bytes = _longABImageTexture.EncodeToPNG();
                var dirPath = Application.persistentDataPath + "/SaveImages/";
                if(!Directory.Exists(dirPath)) 
                {
                    Directory.CreateDirectory(dirPath);
                }
                File.WriteAllBytes(dirPath + "AB" + _numberOfImages + ".png", bytes);
            }

            _numberOfImages++;
        }
        */
    }
}

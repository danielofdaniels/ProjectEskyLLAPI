using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Runtime.InteropServices;


public class GetImageFromGPU : MonoBehaviour
{
    [DllImport("libProjectEskyLLAPIX", EntryPoint = "UnityPluginLoad")]
    public static extern int UnityPluginLoad();
    [DllImport("libProjectEskyLLAPIX", EntryPoint = "UnityPluginUnload")]
    public static extern int UnityPluginUnload();
    [DllImport("libProjectEskyLLAPIX", EntryPoint = "GetRenderEventFunc")]
    public static extern IntPtr GetRenderEventFunc();
    [DllImport("libProjectEskyLLAPIX", EntryPoint = "SetRenderTexturePointer")]
    public static extern int SetRenderTexturePointer(int Id, IntPtr texPointer);
    [DllImport("libProjectEskyLLAPIX", EntryPoint = "InitializeTrackerObject")]
    public static extern int InitializeTrackerObject(int Id);
    [DllImport("libProjectEskyLLAPIX", EntryPoint = "InitializeXCameraFeed")]
    public static extern int InitializeXCameraFeed(int Id);
    [DllImport("libProjectEskyLLAPIX", EntryPoint = "HookDeviceToIntel")]
    public static extern int HookDeviceToIntel(int Id);


    public int TrackerID;
    public RenderTexture tex;
    public UnityEngine.UI.RawImage rawImageMap;
    
    // TODO - Pull these in dynamically
    private const int width = 640;
    private const int height = 480;
    
    // TODO - Set to true once the camera is init
    bool canRenderImages = false;
    
    void Start()
    {
        TrackerID = 0;

        // Init the Tracker Object
        InitializeTrackerObject(TrackerID);
        HookDeviceToIntel(TrackerID);

        // Start the camera feed
        InitializeXCameraFeed(TrackerID);

        // Create a Unity side texture object
        tex = new RenderTexture(width, height, 0, RenderTextureFormat.BGRA32);
        tex.Create();
        // Link the C++ code to the texture pointer
        SetRenderTexturePointer(TrackerID, tex.GetNativeTexturePtr());
        // Link the unity world object
        if (rawImageMap != null)
        {
            rawImageMap.texture = tex;
            rawImageMap.gameObject.SetActive(true);
            print("Hooked into image object");
        }

        // TODO - Use a callback to do this
        canRenderImages = true;
        
        // Start the background process to update texture on the GPU
        StartCoroutine(WaitEndFrameCameraUpdate());

    }

    // Update is called once per frame
    void Update()
    {

    }

    IEnumerator WaitEndFrameCameraUpdate()
    {
        while (true)
        {
            yield return new WaitForEndOfFrame();
            if (canRenderImages)
            {
                GL.IssuePluginEvent(GetRenderEventFunc(), TrackerID);
                // GetRenderEventFunc gives unity OnRenderEvent()
                // OnRenderEvent() runs UpdatecameraTextureGPU() using the preloaded m_device and texture ptr
            }
        }
    }
}

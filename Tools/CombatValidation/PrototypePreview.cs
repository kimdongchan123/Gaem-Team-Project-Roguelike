using System;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public static class PrototypePreview
{
    public static void Capture(string name)
    {
        if(Array.IndexOf(Environment.GetCommandLineArgs(),"-prototype-preview")<0)return;
        Camera camera=Camera.main;if(camera==null)throw new Exception("Preview camera missing");
        var target=new RenderTexture(1280,720,24,RenderTextureFormat.ARGB32);target.Create();
        var oldTarget=camera.targetTexture;float oldAspect=camera.aspect;var oldActive=RenderTexture.active;
        var canvases=UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        var modes=new RenderMode[canvases.Length];var cameras=new Camera[canvases.Length];var distances=new float[canvases.Length];
        try
        {
            camera.targetTexture=target;camera.aspect=1280f/720f;
            for(int i=0;i<canvases.Length;i++)
            {
                modes[i]=canvases[i].renderMode;cameras[i]=canvases[i].worldCamera;distances[i]=canvases[i].planeDistance;
                if(modes[i]==RenderMode.ScreenSpaceOverlay){canvases[i].renderMode=RenderMode.ScreenSpaceCamera;canvases[i].worldCamera=camera;canvases[i].planeDistance=1f;}
            }
            Canvas.ForceUpdateCanvases();
            if(GraphicsSettings.currentRenderPipeline!=null)
                RenderPipeline.SubmitRenderRequest(camera,new UniversalRenderPipeline.SingleCameraRequest {destination=target});
            else camera.Render();
            RenderTexture.active=target;
            var image=new Texture2D(1280,720,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1280,720),0,0);image.Apply();
            string directory=Path.GetFullPath("Preview");Directory.CreateDirectory(directory);
            File.WriteAllBytes(Path.Combine(directory,name+".png"),ImageConversion.EncodeToPNG(image));UnityEngine.Object.DestroyImmediate(image);
            Debug.Log("PROTOTYPE_PREVIEW: "+Path.Combine(directory,name+".png"));
        }
        finally
        {
            for(int i=0;i<canvases.Length;i++){canvases[i].renderMode=modes[i];canvases[i].worldCamera=cameras[i];canvases[i].planeDistance=distances[i];}
            camera.targetTexture=oldTarget;camera.aspect=oldAspect;RenderTexture.active=oldActive;target.Release();UnityEngine.Object.DestroyImmediate(target);
        }
    }
}

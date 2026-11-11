using UnityEngine;

[DefaultExecutionOrder(500)]
public class PrototypeCameraFeedback : MonoBehaviour
{
    private Vector3 offset;
    private float strength;
    public static void Shake(float amount)
    {
        if(!PrototypeArt.Enabled||(PrototypeArt.Theme!=null&&!PrototypeArt.Theme.cameraShake))return;
        var camera=Camera.main;if(camera==null)return;
        var feedback=camera.GetComponent<PrototypeCameraFeedback>();
        if(feedback==null)feedback=camera.gameObject.AddComponent<PrototypeCameraFeedback>();
        feedback.strength=Mathf.Min(0.12f,Mathf.Max(feedback.strength,amount));
    }
    private void Update(){transform.position-=offset;offset=Vector3.zero;}
    private void LateUpdate()
    {
        if(Time.deltaTime<=0)return;
        strength=Mathf.MoveTowards(strength,0,Time.deltaTime*0.9f);
        offset=new Vector3(Mathf.Sin(Time.time*113),Mathf.Cos(Time.time*97),0)*strength;
        transform.position+=offset;
    }
    private void OnDisable(){transform.position-=offset;offset=Vector3.zero;strength=0;}
}

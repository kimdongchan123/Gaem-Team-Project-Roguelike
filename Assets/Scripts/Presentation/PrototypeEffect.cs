using UnityEngine;

public class PrototypeEffect : MonoBehaviour
{
    private SpriteRenderer sprite;
    private Vector2 velocity;
    private float age,duration;
    private Vector3 initialScale;
    private Color initialColor;
    private bool expand;
    private static int active;
    private static readonly System.Random random=new System.Random(416);
    public static void Emit(Sprite image,Vector2 position,Color color,Vector2 size,float lifetime,Vector2 speed,int layer,int order,bool grow=false,float rotation=0)
    {
        if(!PrototypeArt.Enabled||active>=160)return;
        var obj=new GameObject("Prototype Effect");obj.transform.position=position;obj.transform.localScale=new Vector3(size.x,size.y,1);
        obj.transform.rotation=Quaternion.Euler(0,0,rotation);
        var renderer=obj.AddComponent<SpriteRenderer>();renderer.sprite=image;renderer.color=color;renderer.sortingLayerID=layer;renderer.sortingOrder=order;
        var effect=obj.AddComponent<PrototypeEffect>();effect.sprite=renderer;effect.initialScale=obj.transform.localScale;
        effect.initialColor=color;effect.velocity=speed;effect.duration=Mathf.Max(0.02f,lifetime);effect.expand=grow;active++;
    }
    public static void Burst(Vector2 position,Color color,int count,int layer,int order)
    {
        for(int i=0;i<count;i++)
        {
            float angle=(float)random.NextDouble()*Mathf.PI*2,speed=0.5f+(float)random.NextDouble()*1.7f;
            Emit(PrototypeArt.Shape("pixel"),position,color,Vector2.one*(0.045f+(float)random.NextDouble()*0.055f),0.22f+(float)random.NextDouble()*0.18f,
                new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*speed,layer,order);
        }
    }
    public static void Impact(Vector2 position,Color color,int layer=0,int order=7)
    {
        Emit(PrototypeArt.Shape("ring"),position,color,Vector2.one*0.3f,0.18f,Vector2.zero,layer,order,true);
        Burst(position,color,5,layer,order);
    }
    private void Update()
    {
        age+=Time.deltaTime;float t=Mathf.Clamp01(age/duration);
        transform.position+=(Vector3)(velocity*Time.deltaTime);velocity*=Mathf.Exp(-5*Time.deltaTime);
        sprite.color=new Color(initialColor.r,initialColor.g,initialColor.b,initialColor.a*(1-t));
        transform.localScale=initialScale*(expand?1+t*2f:1-t*0.35f);
        if(age>=duration)Destroy(gameObject);
    }
    private void OnDestroy(){active=Mathf.Max(0,active-1);}
}

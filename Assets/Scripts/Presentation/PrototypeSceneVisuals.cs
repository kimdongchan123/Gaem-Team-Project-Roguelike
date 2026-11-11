using System.Collections.Generic;
using UnityEngine;

public class PrototypeSceneVisuals : MonoBehaviour
{
    private readonly List<SpriteRenderer> beacons=new List<SpriteRenderer>();
    public static void Install()
    {
        if(!PrototypeArt.Enabled||FindAnyObjectByType<PrototypeSceneVisuals>()!=null)return;
        new GameObject("Prototype Scene Presentation").AddComponent<PrototypeSceneVisuals>();
    }
    private void Start()
    {
        int floors=0;
        foreach(var original in FindObjectsByType<SpriteRenderer>(FindObjectsInactive.Include,FindObjectsSortMode.None))
        {
            string name=original.gameObject.name;
            if(name=="Floor"||name.StartsWith("Corridor_"))
            {
                DecorateSurface(original,false);floors++;
            }
            else if(name.StartsWith("Wall_")||name.StartsWith("CorridorWall")||name.StartsWith("Door_"))
                DecorateSurface(original,true);
            else if(name=="LobbyStatNPC")
            {
                original.sprite=PrototypeArt.Terminal();original.color=Color.white;
                beacons.Add(original);
            }
            else if(name=="ShopNPC")
            {
                original.sprite=PrototypeArt.Actor(true,0);original.color=new Color(1,0.82f,0.62f);
            }
            else if(name=="DungeonEntrance")
            {
                Bounds bounds=original.bounds;
                original.sprite=PrototypeArt.Tile(true);original.color=Color.white;
                var ring=Surface(original.transform,bounds,PrototypeArt.Shape("ring"),original.sortingLayerID,original.sortingOrder+1,false);
                ring.color=PrototypeArt.Cyan;beacons.Add(ring);
            }
        }
        if(floors==0&&gameObject.scene.name=="SampleScene")
        {
            Surface(transform,new Bounds(Vector3.zero,new Vector3(18,12,1)),PrototypeArt.Tile(false),0,-10,true);
        }
        if(Camera.main!=null)Camera.main.backgroundColor=new Color32(11,18,26,255);
    }
    private void DecorateSurface(SpriteRenderer original,bool wall)
    {
        Bounds bounds=original.bounds;
        if(bounds.size.x<0.001f||bounds.size.y<0.001f)
        {
            Vector3 scale=original.transform.lossyScale;
            bounds=new Bounds(original.transform.position,new Vector3(Mathf.Abs(scale.x)*original.size.x,Mathf.Abs(scale.y)*original.size.y,1));
        }
        int surfaceOrder=wall?original.sortingOrder:Mathf.Min(original.sortingOrder,-2);
        var renderer=Surface(original.transform,bounds,PrototypeArt.Tile(wall),original.sortingLayerID,surfaceOrder,true);
        renderer.sharedMaterial=original.sharedMaterial;renderer.maskInteraction=original.maskInteraction;
        original.enabled=false;
        if(!wall)
        {
            // Sparse floor vents are flat decals, not false obstacles.
            int count=Mathf.Clamp(Mathf.FloorToInt(bounds.size.x/3f),0,8);
            for(int i=0;i<count;i++)
            {
                Vector3 position=new Vector3(bounds.min.x+1.5f+i*3,bounds.min.y+0.35f,bounds.center.z);
                var vent=Surface(original.transform,new Bounds(position,new Vector3(0.7f,0.09f,1)),PrototypeArt.Shape("pixel"),original.sortingLayerID,surfaceOrder+1,false);
                vent.color=new Color(0.12f,0.2f,0.24f);
            }
        }
        else
        {
            Vector3 position=bounds.center;Vector3 size=bounds.size;
            if(size.x>size.y){position.y+=size.y*0.32f;size.y=0.045f;}else{position.x+=size.x*0.32f;size.x=0.045f;}
            var edge=Surface(original.transform,new Bounds(position,size),PrototypeArt.Shape("pixel"),original.sortingLayerID,original.sortingOrder+1,false);
            edge.color=original.name.StartsWith("Door_")?PrototypeArt.Amber:new Color(0.22f,0.42f,0.45f);
        }
    }
    private static SpriteRenderer Surface(Transform parent,Bounds bounds,Sprite sprite,int layer,int order,bool tiled)
    {
        var obj=new GameObject("Prototype Surface");obj.transform.SetParent(parent,false);
        obj.transform.SetPositionAndRotation(bounds.center,Quaternion.identity);
        Vector3 scale=parent.lossyScale;
        obj.transform.localScale=new Vector3(1/Mathf.Max(0.001f,Mathf.Abs(scale.x)),1/Mathf.Max(0.001f,Mathf.Abs(scale.y)),1);
        var renderer=obj.AddComponent<SpriteRenderer>();renderer.sprite=sprite;renderer.sortingLayerID=layer;renderer.sortingOrder=order;
        if(tiled){renderer.drawMode=SpriteDrawMode.Tiled;renderer.size=bounds.size;}
        else obj.transform.localScale=Vector3.Scale(obj.transform.localScale,new Vector3(bounds.size.x/sprite.bounds.size.x,bounds.size.y/sprite.bounds.size.y,1));
        return renderer;
    }
    private void Update()
    {
        float pulse=0.8f+Mathf.Sin(Time.time*2f)*0.2f;
        foreach(var beacon in beacons)if(beacon!=null)beacon.color=new Color(pulse,pulse,1);
    }
}

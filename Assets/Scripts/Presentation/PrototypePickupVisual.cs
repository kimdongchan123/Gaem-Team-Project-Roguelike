using UnityEngine;

public class PrototypePickupVisual : MonoBehaviour
{
    private Transform floating;
    private SpriteRenderer original;
    private float phase;
    private void Start()
    {
        var pickup=GetComponent<WorldPickup>();original=GetComponent<SpriteRenderer>();
        if(pickup==null||original==null)return;
        phase=transform.position.x*3+transform.position.y*7;
        var obj=new GameObject("Prototype Pickup");obj.transform.SetParent(transform,false);floating=obj.transform;
        floating.localScale=Vector3.one*1.35f;
        var renderer=obj.AddComponent<SpriteRenderer>();renderer.sortingOrder=original.sortingOrder;renderer.sortingLayerID=original.sortingLayerID;
        renderer.sprite=pickup.Weapon!=null?PrototypeArt.Weapon(pickup.Weapon.AttackType):PrototypeArt.Item(pickup.Consumable==null?0:pickup.Consumable.Effect==ConsumableEffect.Heal?1:2);
        original.enabled=false;
        var shadow=new GameObject("Prototype Pickup Shadow");shadow.transform.SetParent(transform,false);shadow.transform.localPosition=Vector3.down*0.45f;
        var shadowRenderer=shadow.AddComponent<SpriteRenderer>();shadowRenderer.sprite=PrototypeArt.Shape("shadow");shadowRenderer.sortingOrder=original.sortingOrder-1;shadowRenderer.sortingLayerID=original.sortingLayerID;
    }
    private void LateUpdate()
    {
        if(floating==null)return;
        floating.localPosition=Vector3.up*(0.25f+Mathf.Sin(Time.time*3f+phase)*0.15f);
        floating.rotation=Quaternion.Euler(0,0,Mathf.Sin(Time.time*2f+phase)*5f);
    }
}

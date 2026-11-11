using UnityEngine;

// Visual children animate independently: root transforms and gameplay colliders never squash or bob.
public class PrototypeActorVisual : MonoBehaviour
{
    private SpriteRenderer original,visual,shadow;
    private PlayerHealth playerHealth;
    private MonsterHealth monsterHealth;
    private PlayerMovement movement;
    private PlayerWeaponController controller;
    private Transform gun;
    private Vector3 gunRest,previousPosition,fit;
    private float stride,hitTime,recoil,trailTime;
    private bool player;
    private int variant;
    public SpriteRenderer Visual => visual;
    private void Awake()
    {
        original=GetComponent<SpriteRenderer>();
        if(original==null){enabled=false;return;}
        playerHealth=GetComponent<PlayerHealth>();monsterHealth=GetComponent<MonsterHealth>();movement=GetComponent<PlayerMovement>();
        controller=GetComponent<PlayerWeaponController>();player=playerHealth!=null;
        variant=GetComponent<SuicideMonster>()!=null?2:(GetComponent<RangeMonster>()!=null||GetComponent<MonsterRangedAttack>()!=null)?1:0;
        Vector2 reference=original.sprite!=null?original.sprite.bounds.size:Vector2.one;
        fit=new Vector3(Mathf.Max(reference.x,0.65f),Mathf.Max(reference.y,0.65f),1);
        if(!player)
        {
            Vector3 worldScale=transform.lossyScale;
            fit.x=Mathf.Max(fit.x,0.6f/Mathf.Max(0.001f,Mathf.Abs(worldScale.x)));
            fit.y=Mathf.Max(fit.y,0.6f/Mathf.Max(0.001f,Mathf.Abs(worldScale.y)));
        }
        visual=Child("Prototype Body",original.sortingOrder);visual.sprite=PrototypeArt.Actor(player,0,variant);
        visual.transform.localScale=fit;visual.sharedMaterial=original.sharedMaterial;
        shadow=Child("Prototype Shadow",original.sortingOrder-1);shadow.sprite=PrototypeArt.Shape("shadow");
        shadow.transform.localPosition=new Vector3(0,-fit.y*0.4f,0);shadow.transform.localScale=new Vector3(fit.x*0.9f,fit.x*0.9f,1);
        original.enabled=false;previousPosition=transform.position;
        if(playerHealth!=null)playerHealth.Damaged+=OnPlayerHit;
        if(monsterHealth!=null)monsterHealth.Damaged+=OnMonsterHit;
        if(controller!=null)controller.AttackPerformed+=OnAttack;
        gun=transform.Find("Weapon Aim Pivot/Test Weapon Visual");
        if(gun!=null){gunRest=new Vector3(0.23f,-0.08f,0);gun.localPosition=gunRest;gun.localScale=Vector3.one*0.72f;}
    }
    private SpriteRenderer Child(string name,int order)
    {
        var obj=new GameObject(name);obj.transform.SetParent(transform,false);
        obj.layer=gameObject.layer;
        var renderer=obj.AddComponent<SpriteRenderer>();renderer.sortingLayerID=original.sortingLayerID;renderer.sortingOrder=order;
        renderer.maskInteraction=original.maskInteraction;return renderer;
    }
    private void LateUpdate()
    {
        if(visual==null)return;
        float dt=Time.deltaTime;Vector2 delta=transform.position-previousPosition;previousPosition=transform.position;
        bool dead=playerHealth!=null&&playerHealth.IsDead;
        bool walking=delta.sqrMagnitude>0.000001f&&dt>0;
        stride+=walking?delta.magnitude*1.2f:dt*2f;
        int frame=walking?(int)(stride*0.9f)%4:0;
        visual.sprite=PrototypeArt.Actor(player,frame,variant);visual.flipX=original.flipX;
        if(!player&&walking)visual.flipX=delta.x<-0.001f;
        float bob=walking?Mathf.Abs(Mathf.Sin(stride))*0.045f:Mathf.Sin(Time.time*2.2f)*0.012f;
        visual.transform.localPosition=new Vector3(0,dead?-0.2f:bob,0);
        visual.transform.localRotation=Quaternion.Euler(0,0,dead?75f:walking?Mathf.Sin(stride)*3f:0f);
        hitTime=Mathf.Max(0,hitTime-dt);recoil=Mathf.Max(0,recoil-dt*7f);
        visual.color=dead?new Color(0.45f,0.52f,0.55f,0.7f):hitTime>0?new Color(1,0.6f,0.6f):Color.white;
        if(playerHealth!=null&&playerHealth.IsInvincible&&hitTime<=0)visual.color=new Color(1,1,1,Mathf.Sin(Time.time*45)>0?1:0.45f);
        float squash=hitTime>0?0.08f:walking?Mathf.Sin(stride*2)*0.025f:0;
        visual.transform.localScale=Vector3.Scale(fit,new Vector3(1+squash,1-squash,1));
        if(gun!=null)
        {
            gun.localPosition=gunRest+Vector3.left*recoil*0.12f;
            var renderer=gun.GetComponent<SpriteRenderer>();var weapon=controller.CurrentWeapon;
            renderer.enabled=weapon!=null&&!dead;
            if(weapon!=null)renderer.sprite=PrototypeArt.Weapon(weapon.AttackType);
            renderer.flipY=movement!=null&&movement.AimDirection.x<0;
        }
        if(movement!=null&&movement.IsDashing&&!dead&&Time.time>=trailTime)
        {
            trailTime=Time.time+0.035f;
            PrototypeEffect.Emit(visual.sprite,visual.transform.position,new Color(0.25f,0.95f,1,0.45f),
                new Vector2(transform.lossyScale.x*fit.x,transform.lossyScale.y*fit.y),0.16f,Vector2.zero,original.sortingLayerID,original.sortingOrder-1);
        }
    }
    private void OnAttack(WeaponData weapon,Vector2 origin,Vector2 direction)
    {
        recoil=1;
        float angle=Mathf.Atan2(direction.y,direction.x)*Mathf.Rad2Deg;
        if(weapon.AttackType==WeaponAttackType.Melee)
            PrototypeEffect.Emit(PrototypeArt.Shape("slash"),(Vector2)transform.position+direction*0.6f,PrototypeArt.Amber,Vector2.one*weapon.MeleeRange*1.4f,0.14f,Vector2.zero,original.sortingLayerID,original.sortingOrder+3,false,angle);
        else
            PrototypeEffect.Emit(PrototypeArt.Shape("flash"),origin,weapon.IsMagicWeapon?PrototypeArt.Cyan:PrototypeArt.Amber,new Vector2(0.4f,0.3f),0.065f,Vector2.zero,original.sortingLayerID,original.sortingOrder+3,false,angle);
    }
    private void OnPlayerHit(int damage)
    {
        Hit();PrototypeCameraFeedback.Shake(0.1f);
    }
    private void OnMonsterHit(float damage){Hit();}
    private void Hit()
    {
        hitTime=0.12f;PrototypeEffect.Burst(transform.position,player?PrototypeArt.Cyan:PrototypeArt.Red,6,original.sortingLayerID,original.sortingOrder+3);
    }
    private void OnDestroy()
    {
        if(playerHealth!=null)playerHealth.Damaged-=OnPlayerHit;
        if(monsterHealth!=null)monsterHealth.Damaged-=OnMonsterHit;
        if(controller!=null)controller.AttackPerformed-=OnAttack;
    }
}

using UnityEngine;

[CreateAssetMenu(menuName = "Presentation/Prototype Visual Theme")]
public class PrototypeVisualTheme : ScriptableObject
{
    [Tooltip("임시 그래픽 전체 사용 여부. 변경 후 플레이를 다시 시작하세요.")]
    public bool usePrototypeVisuals = true;
    public bool cameraShake = true;
    [Tooltip("비워두면 코드로 만든 임시 픽셀 그래픽을 사용합니다. 정식 스프라이트로 교체할 수 있습니다.")]
    public Sprite[] playerWalk;
    public Sprite[] monsterWalk;
    public Sprite floorTile;
    public Sprite wallTile;
}

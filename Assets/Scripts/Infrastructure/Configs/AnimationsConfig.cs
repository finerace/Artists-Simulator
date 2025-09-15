using DG.Tweening;
using UnityEngine;

namespace Game.Infrastructure.Configs
{
    [CreateAssetMenu(fileName = "AnimationsConfig", menuName = "Configs/AnimationsConfig", order = 1)]
    public class AnimationsConfig : ScriptableObject
    {
        [SerializeField] private int COMMON_DELAY = 500;
        
        [Space]
        
        [SerializeField] private float loadingMenuShowHideAnimationSpeed = 2f;
        
        [Header("Common")]
        
        [SerializeField] private Ease COMMON_ANIMATION_EASE = Ease.Linear;
        [SerializeField] private float COMMON_ANIMATION_TIME = 0.12f;
        [SerializeField] private float COMMON_ANIMATION_SCALE_DIFFERENCE = 0.9f;
        
        [Header("Camera Moving")]
        
        [SerializeField] private Ease CAMERA_ANIMATION_EASE = Ease.OutCubic;
        [SerializeField] private float CAMERA_ANIMATION_TIME = 0.75f;
        
        [Header("Items Shop")]
        
        [SerializeField] private Ease CELLS_ANIMATION_EASE = Ease.OutCubic;
        [SerializeField] private float CELLS_ANIMATION_TIME = 0.75f;
        [SerializeField] private int CELLS_COOLDOWN = 100;
        [SerializeField] private float CELLS_ICON_OBJECT_TURN_TIME = 2;
        [SerializeField] private float CELLS_ANIMATION_SCALE_DIFFERENCE = 0.5f;
        [SerializeField] private int CELLS_ICON_OBJECT_SCALE_SMOOTH = 18;
        [SerializeField] private int CELLS_UI_MESH_LAYER = 6;
        [SerializeField] private float CELLS_VERTICAL_GAP_MULTIPLIER = 0.25f;

        [Space] 
        
        [SerializeField] private Ease PANELS_ANIMATIONS_EASE = Ease.OutBack;
        [SerializeField] private float PANELS_ANIMATIONS_TIME = 0.75f;
        [SerializeField] private float PANELS_BIG_ANIMATIONS_SCALE_DIFFERENCE = 0.99f;
        [SerializeField] private float PANELS_SMALL_ANIMATIONS_SCALE_DIFFERENCE = 0.95f;
        
        [Header("Gameplay Menu")]
        
        [SerializeField] private Ease GAMEPLAY_MENU_BATTLE_FRAME_EASE = Ease.OutBack;
        [SerializeField] private float GAMEPLAY_MENU_BATTLE_FRAME_TIME = 0.75f;

        [Header("UI Characters Settings")]
        [Tooltip("Слой для UI персонажей")]
        [SerializeField] private int UI_CHARACTER_LAYER = 6;
        
        [Tooltip("Масштаб для персонажей в UI битвы")]
        [SerializeField] private float UI_CHARACTER_SCALE = 1f;
        
        [Space]
        [Tooltip("ID для персонажа игрока в UI битвы")]
        [SerializeField] private string UI_PLAYER_CHARACTER_ID = "PlayerBattleCharacter";

        [Tooltip("ID для персонажа противника в UI битвы")]
        [SerializeField] private string UI_ENEMY_CHARACTER_ID = "EnemyBattleCharacter";

        public int CommonDelay => COMMON_DELAY;

        public float LoadingMenuShowHideAnimationSpeed => loadingMenuShowHideAnimationSpeed;

        public Ease CommonAnimationEase => COMMON_ANIMATION_EASE;

        public float CommonAnimationTime => COMMON_ANIMATION_TIME;

        public float CommonAnimationScaleDifference => COMMON_ANIMATION_SCALE_DIFFERENCE;

        public Ease CameraAnimationEase => CAMERA_ANIMATION_EASE;

        public float CameraAnimationTime => CAMERA_ANIMATION_TIME;

        public Ease CellsAnimationEase => CELLS_ANIMATION_EASE;

        public float CellsAnimationTime => CELLS_ANIMATION_TIME;

        public int CellsCooldown => CELLS_COOLDOWN;

        public float CellsIconObjectTurnTime => CELLS_ICON_OBJECT_TURN_TIME;

        public float CellsAnimationScaleDifference => CELLS_ANIMATION_SCALE_DIFFERENCE;

        public int CellsIconObjectScaleSmooth => CELLS_ICON_OBJECT_SCALE_SMOOTH;

        public int CellsUIMeshLayer => CELLS_UI_MESH_LAYER;

        public float CellsVerticalGapMultiplier => CELLS_VERTICAL_GAP_MULTIPLIER;

        public Ease PanelsAnimationsEase => PANELS_ANIMATIONS_EASE;

        public float PanelsAnimationsTime => PANELS_ANIMATIONS_TIME;
        
        public float PanelsBigAnimationsScaleDifference => PANELS_BIG_ANIMATIONS_SCALE_DIFFERENCE;

        public float PanelsSmallAnimationsScaleDifference => PANELS_SMALL_ANIMATIONS_SCALE_DIFFERENCE;

        public Ease GameplayMenuBattleFrameEase => GAMEPLAY_MENU_BATTLE_FRAME_EASE;

        public float GameplayMenuBattleFrameTime => GAMEPLAY_MENU_BATTLE_FRAME_TIME;

        public int UICharacterLayer => UI_CHARACTER_LAYER;
        public float UICharacterScale => UI_CHARACTER_SCALE;
        public string UIPlayerCharacterId => UI_PLAYER_CHARACTER_ID;
        public string UIEnemyCharacterId => UI_ENEMY_CHARACTER_ID;
        
        [Header("Popup Colors")]
        [SerializeField] private Color popupInformationTitleColor = new Color(0.2f, 0.6f, 1f, 1f);
        [SerializeField] private Color popupInformationTextColor = new Color(0.1f, 0.1f, 0.1f, 1f);
        [SerializeField] private Color popupInformationButtonColor = new Color(0.2f, 0.6f, 1f, 1f);
        
        [SerializeField] private Color popupErrorTitleColor = new Color(0.9f, 0.3f, 0.3f, 1f);
        [SerializeField] private Color popupErrorTextColor = new Color(0.1f, 0.1f, 0.1f, 1f);
        [SerializeField] private Color popupErrorButtonColor = new Color(0.9f, 0.3f, 0.3f, 1f);
        
        [SerializeField] private Color popupInsufficientFundsTitleColor = new Color(1f, 0.6f, 0.2f, 1f);
        [SerializeField] private Color popupInsufficientFundsTextColor = new Color(0.1f, 0.1f, 0.1f, 1f);
        [SerializeField] private Color popupInsufficientFundsButtonColor = new Color(1f, 0.6f, 0.2f, 1f);
        
        public Color PopupInformationTitleColor => popupInformationTitleColor;
        public Color PopupInformationTextColor => popupInformationTextColor;
        public Color PopupInformationButtonColor => popupInformationButtonColor;
        
        public Color PopupErrorTitleColor => popupErrorTitleColor;
        public Color PopupErrorTextColor => popupErrorTextColor;
        public Color PopupErrorButtonColor => popupErrorButtonColor;
        
        public Color PopupInsufficientFundsTitleColor => popupInsufficientFundsTitleColor;
        public Color PopupInsufficientFundsTextColor => popupInsufficientFundsTextColor;
        public Color PopupInsufficientFundsButtonColor => popupInsufficientFundsButtonColor;
    }
}
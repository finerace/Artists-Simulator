using UnityEngine;

namespace Game.Infrastructure.Configs
{
    [CreateAssetMenu(fileName = "AssetsPathsConfig", menuName = "Configs/AssetsPathsConfig", order = 1)]
    public class AssetsPathsConfig : ScriptableObject
    {
        [SerializeField] private int BOOT_SCENE_ID = 0;
        [SerializeField] private int MAIN_SCENE_ID = 1;
        
        [Header("Menu IDs")]
        
        [SerializeField] private string LOADING_MENU_ID = "LoadingScreen";
        [SerializeField] private string MAIN_MENU_ID = "MainMenu";
        [SerializeField] private string SETTINGS_MENU_ID = "SettingsMenu";
        [SerializeField] private string CUSTOMISATION_MENU_ID = "ArtistsCustomisationMenu";
        [SerializeField] private string IMPROVEMENT_MENU_ID = "ImprovementMenu";
        [SerializeField] private string DONATE_MENU_ID = "DonateMenu";
        [SerializeField] private string LOCATION_IMPROVE_MENU_ID = "LocationCustomisationMenu";
        [SerializeField] private string DIFFICULTY_SWITCH_MENU_ID = "DifficultySwithMenu";
        [SerializeField] private string GAME_PLAY_MENU_ID = "GamePlayMenu";
        [SerializeField] private string ENEMY_SEARCH_MENU_ID = "EnemySearchMenu";
        [SerializeField] private string LEVEL_BONUSES_MENU_ID = "LevelBonusesMenu";
        [SerializeField] private string MAIN_LOCATION_ID = "MainLocation";
        [SerializeField] private string POPUP_WINDOW_ID = "PopupWindow";
        
        [Space]
        
        [SerializeField] private string customisationMenuShopCellID = "ShopCell";
        
        [Header("Characters")]
        
        [SerializeField] private string MALE_CHARACTER_ID = "Boy1";
        [SerializeField] private string FEMALE_CHARACTER_ID = "Girl1";

        [Header("Cameras")]
        
        [SerializeField] private string MAIN_CAMERA_NAME = "MainCamera";
        [SerializeField] private string CANVAS_CAMERA_NAME = "CanvasCamera";
        
        [Header("Other")]
        
        [SerializeField] private string PAINT_PATH_RENDERER_ID = "PaintPathRenderer";
        
        [Header("Popup Icons")]
        [SerializeField] private Sprite popupInformationIcon;
        [SerializeField] private Sprite popupErrorIcon;
        [SerializeField] private Sprite popupInsufficientFundsIcon;
        
        [Header("SaveService Settings")]
        [SerializeField] private float saveDebounceDelay = 0.2f;
        [SerializeField] private float saveMaxDelay = 5f;
        
        public string PaintPathRendererID => PAINT_PATH_RENDERER_ID;
        
        public int BootSceneID => BOOT_SCENE_ID;

        public int MainSceneID => MAIN_SCENE_ID;

        public string LoadingMenuID => LOADING_MENU_ID;

        public string MainMenuID => MAIN_MENU_ID;

        public string SettingsMenuID => SETTINGS_MENU_ID;

        public string CustomisationMenuID => CUSTOMISATION_MENU_ID;

        public string ImprovementMenuID => IMPROVEMENT_MENU_ID;

        public string DonateMenuID => DONATE_MENU_ID;

        public string LocationImproveMenuID => LOCATION_IMPROVE_MENU_ID;

        public string DifficultySwitchMenuID => DIFFICULTY_SWITCH_MENU_ID;

        public string GamePlayMenuID => GAME_PLAY_MENU_ID;

        public string LevelBonusesMenuID => LEVEL_BONUSES_MENU_ID;

        public string MainLocationID => MAIN_LOCATION_ID;
        
        public string PopupWindowID => POPUP_WINDOW_ID;

        public string CustomisationMenuShopCellID => customisationMenuShopCellID;

        public string MaleCharacterID => MALE_CHARACTER_ID;

        public string FemaleCharacterID => FEMALE_CHARACTER_ID;

        public string MainCameraName => MAIN_CAMERA_NAME;

        public string CanvasCameraName => CANVAS_CAMERA_NAME;
        
        public string EnemySearchMenuID => ENEMY_SEARCH_MENU_ID;
        
        public Sprite PopupInformationIcon => popupInformationIcon;
        public Sprite PopupErrorIcon => popupErrorIcon;
        public Sprite PopupInsufficientFundsIcon => popupInsufficientFundsIcon;

        public float SaveDebounceDelay => saveDebounceDelay;
        public float SaveMaxDelay => saveMaxDelay;

    }
}
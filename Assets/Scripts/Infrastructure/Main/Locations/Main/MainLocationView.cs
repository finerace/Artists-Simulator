using Game.Services.Core;
using Game.Services.Meta;
using UnityEngine;

namespace Game.Infrastructure.Main.Locations
{
    public class MainLocationView : MonoBehaviour
    {
        [Header("Camera Positions")]
        
        [SerializeField] private Transform mainMenuCameraPoint;
        
        [Space]
        
        [SerializeField] private Transform centerLocationImproveCameraPoint;

        [SerializeField] private Transform characterCustomisationCameraPoint;
        
        [Space]
        
        [SerializeField] private Transform gamePlayCameraPoint;

        [Header("Characters Positions")]
        
        [SerializeField] private Transform mainCharacterDefaultPoint;
        [SerializeField] private Transform mainCharacterShopPoint;

        [Header("Painting")] 
        
        [SerializeField] private PaintingSurface mainPaintingSurface;
        
        [Space]
        
        [SerializeField] private Transform brushT;
        [SerializeField] private Transform brushStartPoint;

        [Header("Location Improvements Points")]
        
        [SerializeField] private LocationImprovementItemData[] locImproveIds;
        [SerializeField] private Transform[] locImprovePoints;
        [SerializeField] private Transform[] locImprovementsCameraPoints;
        
        public Transform MainMenuCameraPoint => mainMenuCameraPoint;
        public Transform CenterLocationImproveCameraPoint => centerLocationImproveCameraPoint;

        public Transform MainCharacterDefaultPoint => mainCharacterDefaultPoint;

        public Transform MainCharacterShopPoint => mainCharacterShopPoint;

        public Transform CharacterCustomisationCameraPoint => characterCustomisationCameraPoint;

        public Transform GamePlayCameraPoint => gamePlayCameraPoint;

        public PaintingSurface MainPaintingSurface => mainPaintingSurface;
        
        public Transform BrushStartPoint => brushStartPoint;

        public Transform BrushT => brushT;

        public LocationImprovementItemData[] LocImproveIds => locImproveIds;

        public Transform[] LocImprovePoints => locImprovePoints;

        public Transform[] LocImprovementsCameraPoints => locImprovementsCameraPoints;
        
    }
}
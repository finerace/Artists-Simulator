using DG.Tweening;
using Game.Services.Core;
using UnityEngine;
using UnityEngine.Serialization;

namespace Game.Infrastructure.Configs
{
    [CreateAssetMenu(fileName = "PaintGameplaySystemsConfig", menuName = "Configs/PaintGameplaySystemsConfig", order = 1)]
    public class PaintGameplaySystemsConfig : ScriptableObject
    {
        [Header("Paint Settings")] 
        
        [SerializeField] private int paintRaycastDistance = 10;
        [SerializeField] private int textureSizeX = 342;
        [SerializeField] private int textureSizeY = 342;
        
        [SerializeField] private int paintDiameter = 25;
        [SerializeField] private float paintRateCof = 5;
        
        [SerializeField] private float scaleXCof = 2f;
        [SerializeField] private float scaleYCof = 2f;

        [SerializeField] private float minPaintMove = 1;
        
        [Header("Colors")] 
        
        [SerializeField] private Color startTextureColor;

        [Space] 
        
        [SerializeField] private float gradientChangeSpeed;
        [SerializeField] private Gradient commonColorGradient;
        
        [Header("Paths Gameplay Settings")]
        
        [SerializeField] private PaintPathData[] paintPathData;
        
        [Space]
        
        [SerializeField] private float paintPointsRegDistance = 0.1f;
        [SerializeField] private float maxAccuracyDistance = 1.0f;
        [SerializeField] private float closePathMultiplier = 10;
        [SerializeField] private float maxScorePerPoint  = 10;
        [SerializeField] private float maxScorePerUnitLightMode = 1;
        [SerializeField] private float maxScorePerUnitHardMode = 1;
        
        [FormerlySerializedAs("drawPathAccuracyPerPoint")]
        [Space]
        
        [SerializeField] private int pathAccuracy = 100;
        
        [Space]
        
        [SerializeField] private float farPenaltyMultiplier  = 2f;
        
        [Space]
        
        [SerializeField] private float progressMoveSpeed = 0.1f;

        [Space]
        
        [SerializeField] private float minDotProductThreshold = 0f;
        [SerializeField] private float minScoreThreshold = 0f;
        [SerializeField] private float lightModeScoreMultiplier = 1.5f;
        
        [Header("Accuracy Search Settings")]
        [SerializeField] private int baseSearchRange = 10;
        [SerializeField] private float speedSearchMultiplier = 20f;

        
        [Header("Brush Object Settings")] 
        
        [SerializeField] private float startPaintCooldown = 5f;
        [SerializeField] private float brushBackTime = 5f;

        //[SerializeField] private float brushUpDistance = 0.1f;

        [Header("Brush Animations")] 
        
        [SerializeField] private float brushMoveAnimationTime = 0.15f;

        [Space] 
        
        [SerializeField] private float rotationNormalPower = 2;
        
        [Space]
        
        [SerializeField] private float brushDistanceMoveAnimationTime = 1f;

        [SerializeField] private Ease brushMoveAnimationEase;
        [SerializeField] private Ease brushRotationAnimationEase;
        
        [Header("Progress Dragger Settings")]
        [SerializeField] private float dragBaseSensitivity = 1.0f;
        [SerializeField] private float dragDampeningFactor = 0.8f;
        [SerializeField] private float maxAccelerationRate = 3.0f;
        [SerializeField] private AnimationCurve speedResistanceCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        
        [Header("Line Renderer Animation Settings")]
        [SerializeField] private float lineRendererFadeInDuration = 0.5f;
        [SerializeField] private float lineRendererFadeOutDuration = 0.5f;
        [SerializeField] private Ease lineRendererFadeInEase = Ease.OutQuad;
        [SerializeField] private Ease lineRendererFadeOutEase = Ease.InQuad;
        
        public PaintPathData[] PaintPathData => paintPathData;
        
        public float ProgressMoveSpeed => progressMoveSpeed;

        public float PaintPointsRegDistance => paintPointsRegDistance;
        
        public float MaxAccuracyDistance => maxAccuracyDistance;

        public int PathAccuracy => pathAccuracy;

        public float ClosePathMultiplier => closePathMultiplier;

        public float MaxScorePerPoint => maxScorePerPoint;

        public float FarPenaltyMultiplier => farPenaltyMultiplier;
        
        public float GetMaxScorePerUnit(bool isLightMode) => isLightMode ? maxScorePerUnitLightMode : maxScorePerUnitHardMode;

        public int PaintRaycastDistance => paintRaycastDistance;

        public int TextureSizeX => textureSizeX;

        public int TextureSizeY => textureSizeY;
        
        public int PaintDiameter => paintDiameter;

        public float PaintRateCof => paintRateCof;

        public float ScaleXCof => scaleXCof;

        public float ScaleYCof => scaleYCof;

        public float MinPaintMove => minPaintMove;

        public Color StartTextureColor => startTextureColor;

        public float GradientChangeSpeed => gradientChangeSpeed;
        public Gradient CommonColorGradient => commonColorGradient;

        public float StartPaintCooldown => startPaintCooldown;

        public float BrushBackTime => brushBackTime;

        public float RotationNormalPower => rotationNormalPower;

        public float BrushDistanceMoveAnimationTime => brushDistanceMoveAnimationTime;
        
        public float BrushMoveAnimationTime => brushMoveAnimationTime;

        public Ease BrushMoveAnimationEase => brushMoveAnimationEase;

        public Ease BrushRotationAnimationEase => brushRotationAnimationEase;

        public float DragBaseSensitivity => dragBaseSensitivity;
        public float DragDampeningFactor => dragDampeningFactor;
        public float MaxAccelerationRate => maxAccelerationRate;
        public AnimationCurve SpeedResistanceCurve => speedResistanceCurve;

        public float LineRendererFadeInDuration => lineRendererFadeInDuration;
        public float LineRendererFadeOutDuration => lineRendererFadeOutDuration;
        public Ease LineRendererFadeInEase => lineRendererFadeInEase;
        public Ease LineRendererFadeOutEase => lineRendererFadeOutEase;

        public float MinDotProductThreshold => minDotProductThreshold;
        public float MinScoreThreshold => minScoreThreshold;
        public float LightModeScoreMultiplier => lightModeScoreMultiplier;
        
        public int BaseSearchRange => baseSearchRange;
        public float SpeedSearchMultiplier => speedSearchMultiplier;
    
    }
}
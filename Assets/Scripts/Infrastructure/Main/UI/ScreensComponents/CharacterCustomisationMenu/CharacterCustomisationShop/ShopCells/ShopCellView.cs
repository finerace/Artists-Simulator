using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game.Infrastructure.Configs;
using Game.Services.Meta;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Game.Additional.MagicAttributes;
using Game.Additional;

namespace Game.Infrastructure.Main.UI
{
    
    public class ShopCellView : MonoBehaviour, IShopCellView<CharacterItemData>, IColorizedShopCellView, IPointerClickHandler
    {
        [SerializeField] private RectTransform cellT;
        [SerializeField] private RectTransform cellBodyT;
        [SerializeField] private CanvasGroup itemCanvasGroup;
        
        [SerializeField] private RawImage iconImage;
        [SerializeField] private Image colorChangeIcon;
        
        [SerializeField] private GameObject iconImageObject;
        [SerializeField] private GameObject selectedImageObject;
        [SerializeField] private GameObject selectedRemovedImageObject;
        [SerializeField] private GameObject currentSelectedColorObject;
        [SerializeField] private GameObject lockedIconObject;
        
        private float icon3DScale;

        private AnimationsConfig animConfig;
        private CharacterItemData itemCurrent;
        private Vector3 defaultScale;
        private Transform iconObjectT;
        
        public event Action onClick;
        
        public RectTransform CellTransform => cellT;
        public CharacterItemData CurrentItem => itemCurrent;

        private void Awake()
        {
            animConfig = ConfigsProxy.AnimationsConfig;
            defaultScale = cellBodyT.localScale;
            itemCanvasGroup.alpha = 0;
        }
        
        public UniTask SetNewItem(CharacterItemData itemData)
        {
            itemCurrent = itemData;
            return UniTask.CompletedTask;
        }

        public void SetIcon2D(Texture iconTexture)
        {
            iconImageObject.SetActive(true);
            iconImage.texture = iconTexture;
        }

        public void SetIcon3D(Transform iconObject, Vector3 localPosition, float scale)
        {
            iconObjectT = iconObject;
            iconImageObject.SetActive(false);
            icon3DScale = scale;

            if (iconObjectT != null)
            {
                iconObject.gameObject.SetLayerSelfAndChilds(ConfigsProxy.AnimationsConfig.CellsUIMeshLayer);
                
                iconObjectT.SetParent(transform, false);
                iconObjectT.gameObject.SetActive(false);
                
                iconObjectT.localPosition = localPosition;
            }
        }
        
        private void StartIcon3DAnimation()
        {
            if (iconObjectT == null) return;
            
            iconObjectT.gameObject.SetActive(true);
            
            var iconObjectScaleSmooth = animConfig.CellsIconObjectScaleSmooth;
            var newObjectDefaultScale = Vector3.one * icon3DScale * iconObjectScaleSmooth;
            
            iconObjectT.localScale = Vector3.zero;
            iconObjectT.DOScale(newObjectDefaultScale, animConfig.CellsAnimationTime)
                .SetEase(animConfig.CellsAnimationEase);
            
            iconObjectT.rotation = Quaternion.identity;
            iconObjectT.DORotate(new Vector3(0, 360, 0), animConfig.CellsIconObjectTurnTime, 
                RotateMode.FastBeyond360).SetEase(Ease.Linear).SetLoops(-1);
        }
        
        public async UniTask StartShowAnimation()
        {
            itemCanvasGroup.alpha = 0;
            cellBodyT.localScale = defaultScale * (1 - animConfig.CellsAnimationScaleDifference);
            
            var scaleTask = cellBodyT.DOScale(defaultScale, animConfig.CellsAnimationTime)
                .SetEase(animConfig.CellsAnimationEase).AsyncWaitForCompletion().AsUniTask();
            var alphaTask = itemCanvasGroup.DOFade(1, animConfig.CellsAnimationTime)
                .SetEase(animConfig.CellsAnimationEase).AsyncWaitForCompletion().AsUniTask();
            
            if (iconObjectT != null)
                StartIcon3DAnimation();

            await UniTask.WhenAll(scaleTask, alphaTask);
        }
        
        public async UniTask StartHideAnimation()
        {
            var cellTargetScale = defaultScale * (1 - animConfig.CellsAnimationScaleDifference);

            var scaleTask = cellT.DOScale(cellTargetScale, animConfig.CellsAnimationTime)
                .SetEase(animConfig.CellsAnimationEase).AsyncWaitForCompletion().AsUniTask();
            var alphaTask = itemCanvasGroup.DOFade(0, animConfig.CellsAnimationTime)
                .SetEase(animConfig.CellsAnimationEase).AsyncWaitForCompletion().AsUniTask();

            if (iconObjectT != null)
            {
                iconObjectT.DOScale(Vector3.zero, animConfig.CellsAnimationTime)
                    .SetEase(animConfig.CellsAnimationEase);
            }

            await UniTask.WhenAll(scaleTask, alphaTask);
        }

        public void SetCellState(ShopCellFrameState frameState)
        {
            bool isSelected = frameState == ShopCellFrameState.Selected || 
                            frameState == ShopCellFrameState.SelectedLocked;
            bool isSelectedRemoved = frameState == ShopCellFrameState.SelectedRemoved || 
                                   frameState == ShopCellFrameState.SelectedRemovedLocked;
            bool isLocked = frameState == ShopCellFrameState.Locked || 
                          frameState == ShopCellFrameState.SelectedLocked || 
                          frameState == ShopCellFrameState.SelectedRemovedLocked;
            
            selectedImageObject.SetActive(isSelected);
            selectedRemovedImageObject.SetActive(isSelectedRemoved);
            lockedIconObject.SetActive(isLocked);
            currentSelectedColorObject.SetActive(isSelected || isSelectedRemoved);
            colorChangeIcon.gameObject.SetActive(isSelectedRemoved);
        }
        
        public void SetCellColorizeColor(Color color)
        {
            if (iconObjectT != null)
            {
                var colorizeView = iconObjectT.GetComponent<CharacterItemColorizeView>();
                if (colorizeView != null)
                    colorizeView.SetColor(color);
            }
            else
            {
                iconImage.color = color;
            }
            
            colorChangeIcon.color = color;
        }

        public void SetColorizeIconActive(bool isActive)
        {
            colorChangeIcon.gameObject.SetActive(isActive);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (cellBodyT == null) 
                return;

            DOTween.Kill(cellBodyT);
            Vector3 originalScale = cellBodyT.localScale;
            
            Sequence clickSequence = DOTween.Sequence();
            clickSequence.Append(cellBodyT.DOScale(originalScale * 0.95f, 0.05f));
            clickSequence.Append(cellBodyT.DOScale(originalScale, 0.05f));
            clickSequence.Play();
            
            onClick?.Invoke();
        }
    }
} 
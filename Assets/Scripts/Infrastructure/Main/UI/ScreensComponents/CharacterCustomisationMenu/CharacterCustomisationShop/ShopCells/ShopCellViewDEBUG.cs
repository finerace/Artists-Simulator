/*using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game.Additional;
using Game.Infrastructure.Configs;
using Game.Services.Meta;
using Game.Services.Common;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;
using Game.Additional.MagicAttributes;
using System.Threading;

namespace Game.Infrastructure.Main.UI
{
    
    public class ShopCellVie : MonoBehaviour, IPointerClickHandler
    {
        // === FIELDS & PROPERTIES ===
        
        private AnimationsConfig animConfig;
        private IAssetsService assetsService;
        
        [SerializeField] private RectTransform cellT;
        [SerializeField] private RectTransform cellBodyT;
        [SerializeField] private CanvasGroup itemCanvasGroup;
        
        [Space]
        
        [SerializeField] private RawImage iconImage;
        [SerializeField] private Image colorChangeIcon;
        
        [Space]
        
        [SerializeField] private GameObject iconImageObject;
        [SerializeField] private GameObject selectedImageObject;
        [SerializeField] private GameObject selectedRemovedImageObject;
        [SerializeField] private GameObject currentSelectedColorObject;
        [SerializeField] private GameObject lockedIconObject;

        [Space]
        
        [SerializeField] private CharacterItemData itemCurrent;

        public event Action onClick;
        
        private Transform iconObjectT;
        private Vector3 defaultScale;

        public RectTransform CellT => cellT;
        public CharacterItemData ItemCurrent => itemCurrent;

        // === DEPENDENCY INJECTION ===
        
        [Inject]
        private void Construct(IAssetsService assetsService)
        {
            this.assetsService = assetsService;
        }
        
        // === INITIALIZATION ===
        
        public void Awake()
        {
            animConfig = ConfigsProxy.AnimationsConfig;
            defaultScale = cellBodyT.localScale;

            itemCanvasGroup.alpha = 0;
        }

        public async UniTask SetNewItem(CharacterItemData itemData)
        {
            itemCurrent = itemData;
            
            if (!itemCurrent.IsIconObject)
            {
                Setup2DIcon();
                return;
            }
            
            if (string.IsNullOrEmpty(itemCurrent.ItemObjectAddressableId))
            {
                Debug.LogWarning($"Item marked as IconObject but has no AddressableId: {itemCurrent.ItemId}");
                return;
            }
            
            Setup3DIcon().Forget();
        }
        
        private async UniTask Setup3DIcon()
        {            
            CleanupExistingIcon();
            void CleanupExistingIcon()
            {
                iconImageObject.SetActive(false);

                if (iconObjectT != null)
                {
                    assetsService.ReleaseAsset(iconObjectT.gameObject);
                    iconObjectT = null;
                }
            }

            var loadedObject = await assetsService.GetAsset<Transform>(itemCurrent.ItemObjectAddressableId);
            
            if(cellT == null)
            {
                if(loadedObject != null)
                    assetsService.ReleaseAsset(loadedObject.gameObject);
                
                return;
            }
            
            ConfigureIconObject();
            void ConfigureIconObject()
            {
                iconObjectT = loadedObject;
                
                iconObjectT.SetParent(transform, false);
                
                iconObjectT.gameObject.SetLayerSelfAndChilds(animConfig.CellsUIMeshLayer);
                iconObjectT.localScale = Vector3.zero;
                iconObjectT.localPosition = ItemCurrent.IconObjectLocalPos;
                iconObjectT.rotation = Quaternion.identity;
            }
            
            StartIconAnimation();
            void StartIconAnimation()
            {
                var iconObjectScaleSmooth = animConfig.CellsIconObjectScaleSmooth;
                var newObjectDefaultScale = Vector3.one * itemCurrent.IconObjectScale * iconObjectScaleSmooth;

                iconObjectT.DOScale(newObjectDefaultScale, animConfig.CellsAnimationTime)
                    .SetEase(animConfig.CellsAnimationEase);
                
                iconObjectT.DORotate(new Vector3(0, 360, 0), animConfig.CellsIconObjectTurnTime, 
                    RotateMode.FastBeyond360).SetEase(Ease.Linear).SetLoops(-1);
            }
        }
        
        private void Setup2DIcon()
        {
            iconImageObject.SetActive(true);
            iconImage.texture = itemCurrent.ItemIcon;
        }

        // === ANIMATIONS ===
        
        public async UniTask StartShowAnimation()
        {
            itemCanvasGroup.alpha = 0;
            cellBodyT.localScale = defaultScale * (1 - animConfig.CellsAnimationScaleDifference);
            
            var scaleTask = cellBodyT.DOScale(defaultScale, animConfig.CellsAnimationTime)
                .SetEase(animConfig.CellsAnimationEase).AsyncWaitForCompletion().AsUniTask();
            var alphaTask = itemCanvasGroup.DOFade(1, animConfig.CellsAnimationTime)
                .SetEase(animConfig.CellsAnimationEase).AsyncWaitForCompletion().AsUniTask();
            
            await UniTask.WhenAll(scaleTask, alphaTask);
        }
        
        public void StartHideAnimation()
        {
            PrepareHideAnimations();
            void PrepareHideAnimations()
            {
                var cellTargetScale = defaultScale * (1 - animConfig.CellsAnimationScaleDifference);

                var scaleTween = cellT.DOScale(cellTargetScale, animConfig.CellsAnimationTime)
                    .SetEase(animConfig.CellsAnimationEase);
                
                var alphaTween = itemCanvasGroup.DOFade(0, animConfig.CellsAnimationTime)
                    .SetEase(animConfig.CellsAnimationEase);

                scaleTween.Pause();
                alphaTween.Pause();

                Handle3DIconHiding();
                void Handle3DIconHiding()
                {
                    if (itemCurrent.IsIconObject && iconObjectT != null)
                    {
                        iconObjectT.DOScale(Vector3.zero, animConfig.CellsAnimationTime)
                            .SetEase(animConfig.CellsAnimationEase);
                    }
                }

                scaleTween.Play();
                alphaTween.Play();
            }
        }

        // === UI STATE MANAGEMENT ===
        
        public void SetCellFrameState(ShopCellFrameState frameState)
        {
            switch (frameState)
            {
                case ShopCellFrameState.Idle:
                {
                    selectedImageObject.SetActive(false);
                    selectedRemovedImageObject.SetActive(false);
                    
                    break;
                }
                case ShopCellFrameState.Selected:
                {
                    selectedImageObject.SetActive(true);
                    selectedRemovedImageObject.SetActive(false);
                    
                    break;
                }

                case ShopCellFrameState.SelectedRemoved:
                {
                    selectedImageObject.SetActive(false);
                    selectedRemovedImageObject.SetActive(true);
                    
                    break;
                }
            }
        }
        
        public void SetCellSelectedColorActive(bool isActive)
        {
            currentSelectedColorObject.SetActive(isActive);
        }
        
        public void SetCellLockObjectActive(bool isActive)
        {
            lockedIconObject.SetActive(isActive);
        }
        
        public void SetCellColorChangeIconActive(bool isActive)
        {
            colorChangeIcon.gameObject.SetActive(isActive);
        }

        public void SetCellColorChangeIconColor(Color color)
        {
            colorChangeIcon.color = color;
        }
        
        public void SetCellColorizeColor(Color color)
        {
            if (itemCurrent.IsIconObject && iconObjectT != null)
            {
                var colorizeView = iconObjectT.GetComponent<CharacterItemColorizeView>();

                if (colorizeView != null)
                    colorizeView.SetColor(color);
            }
            else if (!itemCurrent.IsIconObject)
                iconImage.color = color;
        }
        
        // === EVENT HANDLERS ===
        
        public void OnPointerClick(PointerEventData eventData)
        {
            if (cellBodyT == null) return;

            PlayClickAnimation();
            void PlayClickAnimation()
            {
                DOTween.Kill(cellBodyT);
                
                Vector3 originalScale = cellBodyT.localScale;
                
                CreateClickSequence(originalScale);
                void CreateClickSequence(Vector3 scale)
                {
                    Sequence clickSequence = DOTween.Sequence();
                    clickSequence.Append(cellBodyT.DOScale(scale * 0.95f, 0.05f));
                    clickSequence.Append(cellBodyT.DOScale(scale, 0.05f));
                    clickSequence.Play();
                }
            }
            
            onClick?.Invoke();
        }

        // === CLEANUP ===
        private void OnDestroy()
        {
            if (iconObjectT != null)
            {
                assetsService.ReleaseAsset(iconObjectT.gameObject);
                iconObjectT = null;
            }
        }
    }
}
*/
using Cysharp.Threading.Tasks;
using Game.Additional;
using Game.Infrastructure.Configs;
using Game.Services.Meta;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using UnityEngine.Serialization;

namespace Game.Infrastructure.Main.UI.GamePlayUIState
{
    public class GamePlayMenuView : MonoBehaviour
    {
        // === SERIALIZED FIELDS ===
        
        [Header("Coins and Crystals")]
        [SerializeField] private RectTransform coinsShowPoint;
        [SerializeField] private RectTransform coinsHidePoint;
        [SerializeField] private RectTransform crystalsShowPoint;
        [SerializeField] private RectTransform crystalsHidePoint;
        [SerializeField] private RectTransform levelBarHidePoint;

        [Header("Main Gameplay Frame")]
        [SerializeField] private RectTransform mainGameplayFrame;
        
        [Header("Start/End Battle Frame")] 
        [SerializeField] private RectTransform battleFrame;
        [Space]
        [SerializeField] private RectTransform battleFrameLeft;
        [SerializeField] private RectTransform battleFrameRight;
        [Space]
        [SerializeField] private RectTransform battleFrameLeftShowPoint;
        [SerializeField] private RectTransform battleFrameRightShowPoint;
        [SerializeField] private RectTransform battleFrameLeftHidePoint;
        [SerializeField] private RectTransform battleFrameRightHidePoint;
        [Space]
        [SerializeField] private RectTransform winBattleFrame;
        [Space]
        [SerializeField] private RectTransform winBattleFramePlayerShowPoint;
        [SerializeField] private RectTransform winBattleFramePseudoPlayerShowPoint;
        [Space] 
        [SerializeField] private RectTransform battleStartedFrame;
        [SerializeField] private RectTransform battleEndedFrame;
        [Space]
        [SerializeField] private RectTransform battleFrameBorder;
        [SerializeField] private ParticleSystem[] borderParticleSystems;
        
        [FormerlySerializedAs("playerPos")]
        [Space]
        [Header("3D Characters")]
        [SerializeField] private RectTransform playerCharacterPoint;
        [SerializeField] private RectTransform enemyCharacterPoint;
        
        [Header("Close Button")]
        [SerializeField] private Button closeButton;
        
        [Header("Rewards Panel")]
        [SerializeField] private RectTransform rewardsPanel;
        [SerializeField] private TMP_Text coinsRewardText;
        [SerializeField] private TMP_Text gemsRewardText;
        [SerializeField] private TMP_Text experienceRewardText;

        // === PRIVATE FIELDS ===
        
        private Vector3 borderScale;
        private AnimationsConfig animConf;
        private CharacterCustomizationView playerCharacterCustomization;
        private CharacterCustomizationView enemyCharacterCustomization;
        private ICharactersServiceFacade charactersService;

        // === PROPERTIES ===
        
        public Button CloseButton => closeButton;
        public RectTransform CoinsShowPoint => coinsShowPoint;
        public RectTransform CoinsHidePoint => coinsHidePoint;
        public RectTransform CrystalsShowPoint => crystalsShowPoint;
        public RectTransform CrystalsHidePoint => crystalsHidePoint;
        public RectTransform LevelBarHidePoint => levelBarHidePoint;
        public RectTransform PlayerCharacterPoint => playerCharacterPoint;
        public RectTransform EnemyCharacterPoint => enemyCharacterPoint;

        // === INITIALIZATION ===
        
        private void Awake()
        {
            animConf = ConfigsProxy.AnimationsConfig;
            closeButton.gameObject.SetActive(false);
            rewardsPanel.gameObject.SetActive(false);
        }

        public async UniTask Initialize(ICharactersServiceFacade charactersService)
        {
            this.charactersService = charactersService;
            await CreateCharacters();
            
            if (playerCharacterCustomization != null)
                playerCharacterCustomization.gameObject.SetActive(false);
            if (enemyCharacterCustomization != null)
                enemyCharacterCustomization.gameObject.SetActive(false);
        }

        // === CHARACTER MANAGEMENT ===
        
        private async UniTask CreateCharacters()
        {
            var mainCharacter = charactersService.GetCharacter(ConfigsProxy.CharactersAndShopConfig.MainCharacterId);
            if (mainCharacter == null)
                return;
                
            var playerTemplate = mainCharacter.CustomizationTemplate;
            if (playerTemplate == null)
                return;
                
            playerCharacterCustomization = await charactersService.CreateCharacter(
                playerTemplate, 
                ConfigsProxy.AnimationsConfig.UIPlayerCharacterId);
                
            if (playerCharacterCustomization == null)
                return;
                
            var slotsToCopy = mainCharacter.GetSlotsData();
            await charactersService.ApplySlotDataToCharacter(playerCharacterCustomization, slotsToCopy);
            
            var randomGender = Random.value > 0.5f ? CharacterGender.Male : CharacterGender.Female;
            var enemyTemplate = charactersService.GetRandomTemplate(randomGender);
            if (enemyTemplate == null)
                return;
                
            enemyCharacterCustomization = await charactersService.CreateCharacter(
                enemyTemplate,
                ConfigsProxy.AnimationsConfig.UIEnemyCharacterId);
                
            if (enemyCharacterCustomization == null)
                return;
            
            await charactersService.ApplyRandomItemsToCharacter(enemyCharacterCustomization);
            
            ConfigureCharacterTransform(
                playerCharacterCustomization.gameObject, 
                playerCharacterPoint, 
                Vector3.zero, 
                Vector3.zero, 
                Vector3.one * animConf.UICharacterScale, 
                animConf.UICharacterLayer);
                
            ConfigureCharacterTransform(
                enemyCharacterCustomization.gameObject, 
                enemyCharacterPoint, 
                Vector3.zero, 
                Vector3.zero, 
                Vector3.one * animConf.UICharacterScale, 
                animConf.UICharacterLayer);
        }
        
        public void DestroyCharacters()
        {
            if (playerCharacterCustomization != null)
            {
                charactersService.DestroyCharacter(ConfigsProxy.AnimationsConfig.UIPlayerCharacterId);
                playerCharacterCustomization = null;
            }
            
            if (enemyCharacterCustomization != null)
            {
                charactersService.DestroyCharacter(ConfigsProxy.AnimationsConfig.UIEnemyCharacterId);
                enemyCharacterCustomization = null;
            }
        }
        
        private void ConfigureCharacterTransform(GameObject character, Transform parent, Vector3 localPosition, Vector3 localRotation, Vector3 localScale, int layerOverride = -1)
        {
            if (character == null)
                return;
            
            character.transform.SetParent(parent);
            character.transform.localPosition = localPosition;
            character.transform.localRotation = Quaternion.Euler(localRotation);
            character.transform.localScale = localScale;
            
            if (layerOverride >= 0)
            {
                SetLayerRecursively(character, layerOverride);
            }
        }
        
        private void SetLayerRecursively(GameObject obj, int layer)
        {
            if (obj == null)
                return;
            
            obj.layer = layer;
            
            foreach (Transform child in obj.transform)
            {
                SetLayerRecursively(child.gameObject, layer);
            }
        }
        
        // === BATTLE FRAME ANIMATIONS ===
        
        public async UniTask SetBattleFramesShow(bool isShow)
        {
            if(borderScale == Vector3.zero)
                borderScale = battleFrameBorder.localScale;
            
            battleFrameBorder.SetUIObjectShowAlpha(isShow,borderScale);
            
            if (isShow)
            {
                battleFrame.gameObject.SetActive(true);
                
                if (playerCharacterCustomization != null)
                    playerCharacterCustomization.gameObject.SetActive(true);
                if (enemyCharacterCustomization != null)
                    enemyCharacterCustomization.gameObject.SetActive(true);
                    
                playerCharacterCustomization.transform.localScale = Vector3.zero;
                enemyCharacterCustomization.transform.localScale = Vector3.zero;
            }
            
            SetParticlesShow(isShow);
            
            await UniTask.WhenAll(
                battleFrameLeft.AnchorMove(isShow ? battleFrameLeftShowPoint : battleFrameLeftHidePoint, 
                    animConf.GameplayMenuBattleFrameTime, animConf.GameplayMenuBattleFrameEase),
                
                battleFrameRight.AnchorMove(isShow ? battleFrameRightShowPoint : battleFrameRightHidePoint,
                    animConf.GameplayMenuBattleFrameTime, animConf.GameplayMenuBattleFrameEase),
                AnimateCharacters(isShow)
            );
            
            SetParticlesShow(isShow);
            
            if (!isShow)
            {
                battleFrame.gameObject.SetActive(false);
                
                if (playerCharacterCustomization != null)
                    playerCharacterCustomization.gameObject.SetActive(false);
                if (enemyCharacterCustomization != null)
                    enemyCharacterCustomization.gameObject.SetActive(false);
            }
        }
        
        public void SetBattleFramesShowInstant(bool isShow)
        {
            battleFrameLeft.position = isShow ? battleFrameLeftShowPoint.position : battleFrameLeftHidePoint.position;
            battleFrameRight.position = isShow ? battleFrameRightShowPoint.position : battleFrameRightHidePoint.position;
            
            battleFrameBorder.gameObject.SetActive(false);
            
            battleFrame.gameObject.SetActive(isShow);
            
            SetParticlesShow(isShow);
            
            if (isShow)
            {
                if (playerCharacterCustomization != null)
                {
                    playerCharacterCustomization.gameObject.SetActive(true);
                    playerCharacterCustomization.transform.localScale = Vector3.one * animConf.UICharacterScale;
                }
                if (enemyCharacterCustomization != null)
                {
                    enemyCharacterCustomization.gameObject.SetActive(true);
                    enemyCharacterCustomization.transform.localScale = Vector3.one * animConf.UICharacterScale;
                }
            }
            else
            {
                if (playerCharacterCustomization != null)
                    playerCharacterCustomization.gameObject.SetActive(false);
                if (enemyCharacterCustomization != null)
                    enemyCharacterCustomization.gameObject.SetActive(false);
            }
        }
        
        private async UniTask AnimateCharacters(bool isShow)
        {
            if (playerCharacterCustomization == null || enemyCharacterCustomization == null)
                return;
            
            if (playerCharacterCustomization.transform == null || enemyCharacterCustomization.transform == null)
                return;
            
            Vector3 targetScale = isShow ? Vector3.one * animConf.UICharacterScale : Vector3.zero;
            float duration = animConf.GameplayMenuBattleFrameTime;
            
            var sequence = DOTween.Sequence();
            
            sequence.Join(playerCharacterCustomization.transform.DOScale(targetScale, duration)
                .SetEase(animConf.GameplayMenuBattleFrameEase));
            sequence.Join(enemyCharacterCustomization.transform.DOScale(targetScale, duration)
                .SetEase(animConf.GameplayMenuBattleFrameEase));
            
            await sequence.AsyncWaitForCompletion();
        }
        
        private void SetParticlesShow(bool isShow)
        {
            foreach (var onOpenParticles in borderParticleSystems)
            {
                if(onOpenParticles == null)
                    continue;
                
                if (isShow)
                    onOpenParticles.Play();
                else
                    onOpenParticles.Stop();
            }
        }

        // === UI INTERACTIONS ===
        
        public async UniTask WaitForClose()
        {
            closeButton.gameObject.SetActive(true);
            
            var completionSource = new UniTaskCompletionSource<bool>();
            void OnClick() => completionSource.TrySetResult(true);
            
            closeButton.onClick.AddListener(OnClick);
            await completionSource.Task;
            closeButton.onClick.RemoveListener(OnClick);
            
            closeButton.gameObject.SetActive(false);
        }

        // === MAIN GAMEPLAY FRAME ===
        
        public UniTask SetMainGameplayFrameShow(bool isShow)
        {
            return mainGameplayFrame.SetUIObjectShowAlphaTask(isShow,Vector3.one);
        }

        public void SetMainGameplayFrameShowInstant(bool isShow)
        {
            mainGameplayFrame.gameObject.SetActive(isShow);
        }

        // === WIN BATTLE FRAME ===
        
        public void SetWinBattleFrameShow(bool isShow,bool isPlayerWin = true)
        {
            winBattleFrame.position = isPlayerWin ? winBattleFramePlayerShowPoint.position : 
                winBattleFramePseudoPlayerShowPoint.position;

            winBattleFrame.SetUIObjectShowAlpha(isShow,Vector3.one); 
        }

        // === REWARDS SYSTEM ===
        
        public async UniTask ShowRewards(int coins, int gems, int experience)
        {
            rewardsPanel.gameObject.SetActive(true);
            
            rewardsPanel.localScale = Vector3.one * animConf.PanelsSmallAnimationsScaleDifference;
            var canvasGroup = rewardsPanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = rewardsPanel.gameObject.AddComponent<CanvasGroup>();
            }
            canvasGroup.alpha = 0f;
            
            coinsRewardText.text = "0";
            gemsRewardText.text = "0";
            experienceRewardText.text = "0";
            
            var sequence = DOTween.Sequence();
            sequence.Join(canvasGroup.DOFade(1f, animConf.PanelsAnimationsTime).SetEase(animConf.PanelsAnimationsEase))
                    .Join(rewardsPanel.DOScale(Vector3.one, animConf.PanelsAnimationsTime).SetEase(animConf.PanelsAnimationsEase));
            
            await sequence.AsyncWaitForCompletion();
            
            sequence = DOTween.Sequence();
            
            if (coins > 0)
                sequence.Join(DOTween.To(() => 0, x => coinsRewardText.text = x.ToString(), coins, animConf.PanelsAnimationsTime)
                    .SetEase(animConf.PanelsAnimationsEase));
            
            if (gems > 0)
                sequence.Join(DOTween.To(() => 0, x => gemsRewardText.text = x.ToString(), gems, animConf.PanelsAnimationsTime)
                    .SetEase(animConf.PanelsAnimationsEase));
            
            if (experience > 0)
                sequence.Join(DOTween.To(() => 0, x => experienceRewardText.text = x.ToString(), experience, animConf.PanelsAnimationsTime)
                    .SetEase(animConf.PanelsAnimationsEase));
            
            await sequence.AsyncWaitForCompletion();
        }

        public async UniTask HideRewards()
        {
            if (!rewardsPanel.gameObject.activeSelf)
                return;
            
            var canvasGroup = rewardsPanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = rewardsPanel.gameObject.AddComponent<CanvasGroup>();
            }
            
            var sequence = DOTween.Sequence();
            sequence.Join(canvasGroup.DOFade(0f, animConf.PanelsAnimationsTime).SetEase(animConf.PanelsAnimationsEase))
                    .Join(rewardsPanel.DOScale(Vector3.one * animConf.PanelsSmallAnimationsScaleDifference, 
                        animConf.PanelsAnimationsTime).SetEase(animConf.PanelsAnimationsEase));
            
            await sequence.AsyncWaitForCompletion();
            rewardsPanel.gameObject.SetActive(false);
        }
    }
}
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using Game.Services.Meta;
using Game.Infrastructure.Configs;
using Zenject;

namespace Game.Infrastructure.Main.UI.ScreensComponents.CharacterCustomisationMenu
{
    public class CharacterRotationController : MonoBehaviour
    {
        [SerializeField] private Button leftRotateButton;
        [SerializeField] private Button rightRotateButton;
        [SerializeField] private float rotationSpeed = 90f;
        [SerializeField] private float rotationDuration = 0.3f;
        
        private ICharactersServiceFacade charactersService;
        private Transform characterTransform;
        private bool isRotatingLeft;
        private bool isRotatingRight;
        private float currentRotationVelocity;
        private float targetRotationSpeed;

        [Inject]
        public void Construct(ICharactersServiceFacade charactersService)
        {
            this.charactersService = charactersService;
        }

        private void Start()
        {
            InitializeCharacterReference();
            SetupButtons();
            SubscribeToEvents();
        }

        private void Update()
        {
            if (characterTransform == null) return;

            HandleRotation();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        private void InitializeCharacterReference()
        {
            UpdateCharacterReference();
        }

        private void SetupButtons()
        {
            SetupRotationButton(leftRotateButton, true);
            SetupRotationButton(rightRotateButton, false);
        }

        private void SetupRotationButton(Button button, bool isLeft)
        {
            if (button == null) return;

            EventTrigger trigger = GetOrAddEventTrigger(button);
            
            AddEventTriggerEntry(trigger, EventTriggerType.PointerDown, () => StartRotation(isLeft));
            AddEventTriggerEntry(trigger, EventTriggerType.PointerUp, () => StopRotation(isLeft));
            AddEventTriggerEntry(trigger, EventTriggerType.PointerExit, () => StopRotation(isLeft));
        }

        private EventTrigger GetOrAddEventTrigger(Button button)
        {
            EventTrigger trigger = button.GetComponent<EventTrigger>();
            if (trigger == null)
                trigger = button.gameObject.AddComponent<EventTrigger>();
            return trigger;
        }

        private void AddEventTriggerEntry(EventTrigger trigger, EventTriggerType eventType, System.Action callback)
        {
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = eventType;
            entry.callback.AddListener((data) => callback());
            trigger.triggers.Add(entry);
        }

        private void StartRotation(bool isLeft)
        {
            if (isLeft)
            {
                isRotatingLeft = true;
                targetRotationSpeed = -rotationSpeed;
            }
            else
            {
                isRotatingRight = true;
                targetRotationSpeed = rotationSpeed;
            }
        }
        
        private void StopRotation(bool isLeft)
        {
            if (isLeft)
            {
                isRotatingLeft = false;
            }
            else
            {
                isRotatingRight = false;
            }

            if (!isRotatingLeft && !isRotatingRight)
            {
                targetRotationSpeed = 0f;
            }
        }

        private void HandleRotation()
        {
            float easedSpeed = DOVirtual.EasedValue(currentRotationVelocity, targetRotationSpeed, 
                Time.deltaTime / rotationDuration, Ease.OutQuad);
            
            currentRotationVelocity = Mathf.Lerp(currentRotationVelocity, easedSpeed, Time.deltaTime / rotationDuration);
            
            if (Mathf.Abs(currentRotationVelocity) > 0.1f)
            {
                float rotationAmount = currentRotationVelocity * Time.deltaTime;
                characterTransform.Rotate(0, rotationAmount, 0);
            }
        }

        private void SubscribeToEvents()
        {
            charactersService.OnCharacterGenderSwapped += OnCharacterGenderSwapped;
        }

        private void UnsubscribeFromEvents()
        {
            if (charactersService != null)
                charactersService.OnCharacterGenderSwapped -= OnCharacterGenderSwapped;
        }

        private void OnCharacterGenderSwapped(string characterId, CharacterCustomizationView newCharacter)
        {
            if (characterId == ConfigsProxy.CharactersAndShopConfig.MainCharacterId)
            {
                UpdateCharacterReference();
            }
        }

        private void UpdateCharacterReference()
        {
            var character = charactersService.GetCharacter(ConfigsProxy.CharactersAndShopConfig.MainCharacterId);
            characterTransform = character?.transform;
        }
    }
} 
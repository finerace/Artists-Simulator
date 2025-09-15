using System;
using Game.Services.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Game.Services.Meta
{
    public class LocationImprovementBuyer : MonoBehaviour
    {
        private ILocationImprovementsService locationImprovementsService;
        private ILocalizationService localizationService;
        
        [SerializeField] private Button buyButton;

        [Space] 
        
        [SerializeField] private TMP_Text buttonText;
        [SerializeField] private TMP_Text priceText;
        [SerializeField] private TMP_Text nameText;

        [Space] 
        
        [SerializeField] private RectTransform pricePanel;
        [SerializeField] private RectTransform coinIcon;
        [SerializeField] private RectTransform crystalIcon;

        [Space] 
        
        [SerializeField] private LocationImprovementItemData targetItem;

        [Inject]
        private void Construct(ILocationImprovementsService locationImprovementsService,ILocalizationService localizationService)
        {
            this.locationImprovementsService = locationImprovementsService;
            this.localizationService = localizationService;
        }

        private void Start()
        {
            Initialize();
            
            buyButton.onClick.AddListener(Buy);
        }
        
        private void Initialize()
        {
            nameText.text = localizationService.GetText(targetItem.NameId);

            int price = 0;
            
            switch (targetItem.CurrencyType)
            {
                case CurrencyType.Coins:
                    price = targetItem.Price;
                    break;
                case CurrencyType.Crystals:
                    price = targetItem.Price;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            priceText.text = price.ToString();

            if(targetItem.CurrencyType == CurrencyType.Coins)
            {
                coinIcon.gameObject.SetActive(true);
                crystalIcon.gameObject.SetActive(false);
            }
            else
            {
                coinIcon.gameObject.SetActive(false);
                crystalIcon.gameObject.SetActive(true);
            }
            
            if (locationImprovementsService.IsLocationImproveUnlocked(targetItem.Id))
                gameObject.SetActive(false);
        }
        
        private void Buy()
        {
            if(locationImprovementsService.TryBuyLocationImprove(targetItem))
                gameObject.SetActive(false);
        }
    }
}
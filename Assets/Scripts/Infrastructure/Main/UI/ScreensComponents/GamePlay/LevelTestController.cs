using Game.Services.Meta;
using UnityEngine;
using Zenject;

namespace Game.Infrastructure.Main.UI
{
    public class LevelTestController : MonoBehaviour
    {
        [SerializeField] private int experienceToAdd = 100;
        [SerializeField] private int coinsToAdd = 50;
        [SerializeField] private int crystalsToAdd = 10;
        
        private IPlayerLevelService playerLevelService;
        private ICurrenciesService currenciesService;
        
        [Inject]
        private void Construct(IPlayerLevelService playerLevelService, ICurrenciesService currenciesService)
        {
            this.playerLevelService = playerLevelService;
            this.currenciesService = currenciesService;
        }
        
        private void Update()
        {
            // Управление опытом
            if (Input.GetKeyDown(KeyCode.X))
            {
                playerLevelService.AddExperience(experienceToAdd);
            }

            if (Input.GetKeyDown(KeyCode.C))
            {
                playerLevelService.LoseExperience(experienceToAdd);
            }
            
            // Управление монетами
            if (Input.GetKeyDown(KeyCode.V))
            {
                currenciesService.AddCoins(coinsToAdd);
            }
            
            if (Input.GetKeyDown(KeyCode.B))
            {
                currenciesService.TrySpendCoins(coinsToAdd);
            }
            
            // Управление кристаллами
            if (Input.GetKeyDown(KeyCode.N))
            {
                currenciesService.AddCrystals(crystalsToAdd);
            }
            
            if (Input.GetKeyDown(KeyCode.M))
            {
                currenciesService.TrySpendCrystals(crystalsToAdd);
            }
        }
    }
} 
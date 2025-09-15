using UnityEngine;

namespace Game.Infrastructure.Configs
{
    [CreateAssetMenu(fileName = "EconomicConfig", menuName = "Configs/EconomicConfig", order = 1)]
    public class EconomicConfig : ScriptableObject
    {
        [Header("Gameplay")] 
        
        [SerializeField] private int startMoney = 100;
        [SerializeField] private int startCrystals = 35;
        
        [Space]
        
        [SerializeField] private int colorizePrice = 50;

        public int StartMoney => startMoney;

        public int StartCrystals => startCrystals;
        
        public int ColorizePrice => colorizePrice;
    }
}

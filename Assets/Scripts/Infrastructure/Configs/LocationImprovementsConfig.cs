using Game.Services.Meta;
using UnityEngine;

namespace Game.Infrastructure.Configs
{
    
    [CreateAssetMenu(fileName = "LocationImprovementsConfig", menuName = "Configs/LocationImprovementsConfig", order = 1)]
    public class LocationImprovementsConfig : ScriptableObject
    {
        [SerializeField] private LocationImprovementItemData[] locationImprovementDatas;
        
        public LocationImprovementItemData[] LocationImprovementDatas => locationImprovementDatas;
    }
}
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Game.Services.Meta
{
    public interface ILocationImprovementsService
    {
        event Action<string> OnLocImproveUnlocked;
        
        UniTask Initialize(List<string> unlockedImprovements);
        UniTask BuildAllLocationImproves(bool buildUnlockedOnly = false);
        UniTask DestroyLocationImproves(bool destroyUnlocked = false);
        UniTask CreateLocationImprovement(string locImproveId);
        UniTask DestroyLocationImprovement(string locImproveId);
        bool TryBuyLocationImprove(LocationImprovementItemData locImproveItemData);
        bool IsLocationImproveUnlocked(string locImproveId);
    }
} 
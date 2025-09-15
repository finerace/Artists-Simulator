using Cysharp.Threading.Tasks;

namespace Game.Services.Common
{
    public interface ISaveLoadService
    {
        void Initialize();
        UniTask Save();
        UniTask Load();
    }
} 
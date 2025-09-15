using Game.Additional.MagicAttributes;

namespace Game.Services.Common
{
    
    public abstract class AssetService<T,T2>
    {
        protected readonly IAssetsService assetsService;
        protected T asset;
        
        protected AssetService(IAssetsService assetsService)
        {
            this.assetsService = assetsService;
        }
        
        public abstract T2 Initialize();
    
    }   
}
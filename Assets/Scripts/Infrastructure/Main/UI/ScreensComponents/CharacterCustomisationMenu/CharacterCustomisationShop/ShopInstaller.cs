using Game.Services.Meta;
using UnityEngine;
using Zenject;

namespace Game.Infrastructure.Main.UI
{
    public class ShopInstaller : MonoInstaller
    {
        public IShopPresenter ShopPresenter { get; private set; }
        
        [Header("Shop Components")]
        [SerializeField] private CharacterItemsShopView shopView;
        [SerializeField] private ColorSelector colorSelector;
        
        public UniversalMenuView UniversalMenuView => shopView;
        
        public override void InstallBindings()
        {
            // === MODEL COMPONENTS ===
            
            Container.Bind<IItemsProvider>()
                .To<ItemsProvider>()
                .AsSingle();

            // === VIEW COMPONENTS ===
            
            Container.Bind<ICharacterItemsShopView>()
                .FromInstance(shopView)
                .AsSingle();
                
            Container.Bind<IColorSelectWidget>()
                .FromInstance(colorSelector)
                .AsSingle();
                
            // === CELLS SYSTEM ===
            
            Container.Bind<ILayoutManager>()
                .To<GridLayoutManager>()
                .AsSingle();
                
            Container.Bind<IShopCellsFabric>()
                .To<ShopCellsFabric>()
                .AsSingle();
                
            // === SUB-PRESENTERS ===
            
            Container.Bind<IShopCellsPresenter<CharacterItemData>>()
                .To<ShopCellsPresenter>()
                .AsSingle();
                
            Container.Bind<IShopItemsSelectionPresenter<CharacterItemData, CharacterCustomizationView>>()
                .To<ShopItemsSelectionPresenter>()
                .AsSingle();
                
            // === MAIN PRESENTER ===
            
            Container.Bind<IShopPresenter>()
                .To<ShopPresenter>()
                .AsSingle()
                .WithArguments(Container.Resolve<ICharacterItemsShopService>());
                
            ShopPresenter = Container.Resolve<IShopPresenter>();
        }
    }
} 
using Cysharp.Threading.Tasks;
using Game.Additional.MagicAttributes;
using Game.Infrastructure.Configs;
using Game.Infrastructure.Main;
using Game.Services.Common.Logging;
using UnityEngine;
using Zenject;


public class GameBootstrapper : MonoBehaviour
{
    private GameStateMachine gameStateMachine;
    
    [SerializeField] private GameObject dontDestroyOnLoad;
    
    [Header("Configs")]
    
    [SerializeField] private EconomicConfig economicConfig;
    [SerializeField] private AssetsPathsConfig assetsPathsConfig;
    [SerializeField] private AnimationsConfig animationsConfig;
    [SerializeField] private CharacterShopConfig characterShopConfig;
    [SerializeField] private PaintGameplaySystemsConfig paintGameplaySystemsConfig;
    [SerializeField] private LocationImprovementsConfig locationImprovementsConfig;
    [SerializeField] private CompetitiveGameConfig competitiveGameConfig;
    [SerializeField] private LocalizationConfig localizationConfig;
    [SerializeField] private LoggingConfig loggingConfig;
    [SerializeField] private ExceptionHandlingConfig exceptionHandlingConfig;
    
    [Inject]
    private void Construct(GameStateMachine gameStateMachine)
    {
        this.gameStateMachine = gameStateMachine;
    }
    
    private void Awake()
    {
        DontDestroyOnLoad(dontDestroyOnLoad);
        
        ConfigsProxy.
            SetNewConfigs(
                economicConfig,
                assetsPathsConfig,
                animationsConfig,
                characterShopConfig,
                paintGameplaySystemsConfig,
                locationImprovementsConfig,
                competitiveGameConfig,
                localizationConfig,
                loggingConfig,
                exceptionHandlingConfig);
        
        Logs.Initialize();
    }
    
    private void Start()
    {
        gameStateMachine.Initialize();
        gameStateMachine.EnterState<BootState>().Forget();
    }
}

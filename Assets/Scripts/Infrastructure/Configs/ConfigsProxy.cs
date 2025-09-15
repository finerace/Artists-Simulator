namespace Game.Infrastructure.Configs
{
    static public class ConfigsProxy
    {
        private static EconomicConfig economicConfig;
        public static EconomicConfig EconomicConfig => economicConfig;
        
        
        private static AssetsPathsConfig assetsPathsConfig;
        public static AssetsPathsConfig AssetsPathsConfig => assetsPathsConfig;

        
        private static AnimationsConfig animationsConfig;
        public static AnimationsConfig AnimationsConfig => animationsConfig;
        
        private static CharacterShopConfig charactersAndShopConfig;
        public static CharacterShopConfig CharactersAndShopConfig => charactersAndShopConfig;
        

        private static PaintGameplaySystemsConfig paintGameplaySystemsConfig;
        public static PaintGameplaySystemsConfig PaintGameplaySystemsConfig => paintGameplaySystemsConfig;
        
        private static LocationImprovementsConfig locationImprovementsConfig;
        
        public static LocationImprovementsConfig LocationImprovementsConfig => locationImprovementsConfig;
        
        private static CompetitiveGameConfig competitiveGameConfig;
        public static CompetitiveGameConfig CompetitiveGameConfig => competitiveGameConfig;
        
        private static LocalizationConfig localizationConfig;
        public static LocalizationConfig LocalizationConfig => localizationConfig;
        
        private static LoggingConfig loggingConfig;
        public static LoggingConfig LoggingConfig => loggingConfig;
        
        private static ExceptionHandlingConfig exceptionHandlingConfig;
        public static ExceptionHandlingConfig ExceptionHandlingConfig => exceptionHandlingConfig;
        
        public static void SetNewConfigs(
            EconomicConfig economicConfig,
            AssetsPathsConfig assetsPathsConfig, 
            AnimationsConfig animationsConfig, 
            CharacterShopConfig characterShopConfig,
            PaintGameplaySystemsConfig paintGameplaySystemsConfig,
            LocationImprovementsConfig locationImprovementsConfig,
            CompetitiveGameConfig competitiveGameConfig,
            LocalizationConfig localizationConfig,
            LoggingConfig loggingConfig,
            ExceptionHandlingConfig exceptionHandlingConfig)
        {
            ConfigsProxy.economicConfig = economicConfig;
            ConfigsProxy.assetsPathsConfig = assetsPathsConfig;
            ConfigsProxy.animationsConfig = animationsConfig;
            
            ConfigsProxy.charactersAndShopConfig = characterShopConfig;
            charactersAndShopConfig.InitializeItemsDatabase();
            
            ConfigsProxy.paintGameplaySystemsConfig = paintGameplaySystemsConfig;
            ConfigsProxy.locationImprovementsConfig = locationImprovementsConfig;
            ConfigsProxy.competitiveGameConfig = competitiveGameConfig;
            
            ConfigsProxy.localizationConfig = localizationConfig;
            localizationConfig.Initialize();
            
            ConfigsProxy.loggingConfig = loggingConfig;
            ConfigsProxy.exceptionHandlingConfig = exceptionHandlingConfig;
        }

    }
}
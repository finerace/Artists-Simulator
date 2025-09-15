using System.Collections.Generic;
using Game.Additional.MagicAttributes;
using UnityEngine;

namespace Game.Infrastructure.Additionals
{
    
    public static class LanguageUtility
    {
        // Словарь соответствия ISO 639-1 кодов и SystemLanguage
        private static readonly Dictionary<string, SystemLanguage> IsoToSystemLanguage = new Dictionary<string, SystemLanguage>
        {
            {"af", SystemLanguage.Afrikaans},
            {"ar", SystemLanguage.Arabic},
            {"be", SystemLanguage.Belarusian},
            {"bg", SystemLanguage.Bulgarian},
            {"ca", SystemLanguage.Catalan},
            {"cs", SystemLanguage.Czech},
            {"da", SystemLanguage.Danish},
            {"de", SystemLanguage.German},
            {"el", SystemLanguage.Greek},
            {"en", SystemLanguage.English},
            {"es", SystemLanguage.Spanish},
            {"et", SystemLanguage.Estonian},
            {"fi", SystemLanguage.Finnish},
            {"fr", SystemLanguage.French},
            {"he", SystemLanguage.Hebrew},
            {"hi", SystemLanguage.Hindi},
            {"hu", SystemLanguage.Hungarian},
            {"id", SystemLanguage.Indonesian},
            {"it", SystemLanguage.Italian},
            {"ja", SystemLanguage.Japanese},
            {"ko", SystemLanguage.Korean},
            {"lt", SystemLanguage.Lithuanian},
            {"lv", SystemLanguage.Latvian},
            {"nl", SystemLanguage.Dutch},
            {"no", SystemLanguage.Norwegian},
            {"pl", SystemLanguage.Polish},
            {"pt", SystemLanguage.Portuguese},
            {"ro", SystemLanguage.Romanian},
            {"ru", SystemLanguage.Russian},
            {"sk", SystemLanguage.Slovak},
            {"sl", SystemLanguage.Slovenian},
            {"sr", SystemLanguage.SerboCroatian},
            {"sv", SystemLanguage.Swedish},
            {"th", SystemLanguage.Thai},
            {"tr", SystemLanguage.Turkish},
            {"uk", SystemLanguage.Ukrainian},
            {"vi", SystemLanguage.Vietnamese},
            {"zh", SystemLanguage.Chinese},
        };

        // Словарь соответствия SystemLanguage и ISO 639-1 кодов (обратное отображение)
        private static readonly Dictionary<SystemLanguage, string> SystemLanguageToIso = new Dictionary<SystemLanguage, string>();

        // Словарь с человекочитаемыми названиями языков
        private static readonly Dictionary<SystemLanguage, string> SystemLanguageToDisplayName = new Dictionary<SystemLanguage, string>
        {
            {SystemLanguage.Afrikaans, "Afrikaans"},
            {SystemLanguage.Arabic, "Arabic"},
            {SystemLanguage.Belarusian, "Belarusian"},
            {SystemLanguage.Bulgarian, "Bulgarian"},
            {SystemLanguage.Catalan, "Catalan"},
            {SystemLanguage.Czech, "Czech"},
            {SystemLanguage.Danish, "Danish"},
            {SystemLanguage.German, "German"},
            {SystemLanguage.Greek, "Greek"},
            {SystemLanguage.English, "English"},
            {SystemLanguage.Spanish, "Spanish"},
            {SystemLanguage.Estonian, "Estonian"},
            {SystemLanguage.Finnish, "Finnish"},
            {SystemLanguage.French, "French"},
            {SystemLanguage.Hebrew, "Hebrew"},
            {SystemLanguage.Hindi, "Hindi"},
            {SystemLanguage.Hungarian, "Hungarian"},
            {SystemLanguage.Indonesian, "Indonesian"},
            {SystemLanguage.Italian, "Italian"},
            {SystemLanguage.Japanese, "Japanese"},
            {SystemLanguage.Korean, "Korean"},
            {SystemLanguage.Lithuanian, "Lithuanian"},
            {SystemLanguage.Latvian, "Latvian"},
            {SystemLanguage.Dutch, "Dutch"},
            {SystemLanguage.Norwegian, "Norwegian"},
            {SystemLanguage.Polish, "Polish"},
            {SystemLanguage.Portuguese, "Portuguese"},
            {SystemLanguage.Romanian, "Romanian"},
            {SystemLanguage.Russian, "Russian"},
            {SystemLanguage.Slovak, "Slovak"},
            {SystemLanguage.Slovenian, "Slovenian"},
            {SystemLanguage.SerboCroatian, "Serbo-Croatian"},
            {SystemLanguage.Swedish, "Swedish"},
            {SystemLanguage.Thai, "Thai"},
            {SystemLanguage.Turkish, "Turkish"},
            {SystemLanguage.Ukrainian, "Ukrainian"},
            {SystemLanguage.Vietnamese, "Vietnamese"},
            {SystemLanguage.Chinese, "Chinese"},
            {SystemLanguage.Unknown, "Unknown"}
        };
        
        // Словарь с нативными названиями языков (названия на родном языке)
        private static readonly Dictionary<SystemLanguage, string> SystemLanguageToNativeName = new Dictionary<SystemLanguage, string>
        {
            {SystemLanguage.Afrikaans, "Afrikaans"},
            {SystemLanguage.Arabic, "العربية"},
            {SystemLanguage.Belarusian, "Беларуская"},
            {SystemLanguage.Bulgarian, "Български"},
            {SystemLanguage.Catalan, "Català"},
            {SystemLanguage.Czech, "Čeština"},
            {SystemLanguage.Danish, "Dansk"},
            {SystemLanguage.German, "Deutsch"},
            {SystemLanguage.Greek, "Ελληνικά"},
            {SystemLanguage.English, "English"},
            {SystemLanguage.Spanish, "Español"},
            {SystemLanguage.Estonian, "Eesti"},
            {SystemLanguage.Finnish, "Suomi"},
            {SystemLanguage.French, "Français"},
            {SystemLanguage.Hebrew, "עברית"},
            {SystemLanguage.Hindi, "हिन्दी"},
            {SystemLanguage.Hungarian, "Magyar"},
            {SystemLanguage.Indonesian, "Bahasa Indonesia"},
            {SystemLanguage.Italian, "Italiano"},
            {SystemLanguage.Japanese, "日本語"},
            {SystemLanguage.Korean, "한국어"},
            {SystemLanguage.Lithuanian, "Lietuvių"},
            {SystemLanguage.Latvian, "Latviešu"},
            {SystemLanguage.Dutch, "Nederlands"},
            {SystemLanguage.Norwegian, "Norsk"},
            {SystemLanguage.Polish, "Polski"},
            {SystemLanguage.Portuguese, "Português"},
            {SystemLanguage.Romanian, "Română"},
            {SystemLanguage.Russian, "Русский"},
            {SystemLanguage.Slovak, "Slovenčina"},
            {SystemLanguage.Slovenian, "Slovenščina"},
            {SystemLanguage.SerboCroatian, "Српско-хрватски"},
            {SystemLanguage.Swedish, "Svenska"},
            {SystemLanguage.Thai, "ไทย"},
            {SystemLanguage.Turkish, "Türkçe"},
            {SystemLanguage.Ukrainian, "Українська"},
            {SystemLanguage.Vietnamese, "Tiếng Việt"},
            {SystemLanguage.Chinese, "中文"},
            {SystemLanguage.Unknown, "Unknown"}
        };

        // Статический конструктор для инициализации обратного словаря
        static LanguageUtility()
        {
            foreach (var pair in IsoToSystemLanguage)
            {
                SystemLanguageToIso[pair.Value] = pair.Key;
            }
        }

        /// <summary>
        /// Конвертирует код ISO 639-1 в SystemLanguage
        /// </summary>
        /// <param name="isoCode">Код языка в формате ISO 639-1 (например, "en")</param>
        /// <returns>Соответствующий SystemLanguage или SystemLanguage.Unknown, если код не найден</returns>
        public static SystemLanguage IsoCodeToSystemLanguage(string isoCode)
        {
            if (string.IsNullOrEmpty(isoCode))
                return SystemLanguage.Unknown;

            isoCode = isoCode.ToLower();
            
            if (IsoToSystemLanguage.TryGetValue(isoCode, out SystemLanguage language))
                return language;

            // Проверка для двухкомпонентных кодов (например, "en-US")
            int dashIndex = isoCode.IndexOf('-');
            if (dashIndex > 0)
            {
                string primaryCode = isoCode.Substring(0, dashIndex);
                if (IsoToSystemLanguage.TryGetValue(primaryCode, out language))
                    return language;
            }

            return SystemLanguage.Unknown;
        }

        /// <summary>
        /// Конвертирует SystemLanguage в код ISO 639-1
        /// </summary>
        /// <param name="language">Язык SystemLanguage</param>
        /// <returns>Код ISO 639-1 или пустая строка, если язык не поддерживается</returns>
        public static string SystemLanguageToIsoCode(SystemLanguage language)
        {
            if (SystemLanguageToIso.TryGetValue(language, out string isoCode))
                return isoCode;

            return string.Empty;
        }

        /// <summary>
        /// Возвращает человекочитаемое название языка
        /// </summary>
        /// <param name="language">Язык SystemLanguage</param>
        /// <returns>Человекочитаемое название языка</returns>
        public static string GetDisplayName(SystemLanguage language)
        {
            if (SystemLanguageToDisplayName.TryGetValue(language, out string displayName))
                return displayName;

            return language.ToString();
        }
        
        /// <summary>
        /// Возвращает нативное название языка (на родном языке)
        /// </summary>
        /// <param name="language">Язык SystemLanguage</param>
        /// <returns>Название языка на родном языке</returns>
        public static string GetNativeName(SystemLanguage language)
        {
            if (SystemLanguageToNativeName.TryGetValue(language, out string nativeName))
                return nativeName;

            return language.ToString();
        }
        
        /// <summary>
        /// Возвращает нативное название языка по ISO коду
        /// </summary>
        /// <param name="isoCode">ISO код языка</param>
        /// <returns>Название языка на родном языке</returns>
        public static string GetNativeNameFromIsoCode(string isoCode)
        {
            SystemLanguage language = IsoCodeToSystemLanguage(isoCode);
            return GetNativeName(language);
        }

        /// <summary>
        /// Возвращает все поддерживаемые языки и их коды
        /// </summary>
        /// <returns>Словарь с языками и их ISO 639-1 кодами</returns>
        public static IReadOnlyDictionary<SystemLanguage, string> GetAllLanguages()
        {
            return SystemLanguageToIso;
        }
        
        /// <summary>
        /// Возвращает отображаемые имена для всех поддерживаемых языков
        /// </summary>
        /// <returns>Словарь с языками и их отображаемыми именами</returns>
        public static IReadOnlyDictionary<SystemLanguage, string> GetAllDisplayNames()
        {
            return SystemLanguageToDisplayName;
        }
        
        /// <summary>
        /// Возвращает нативные имена для всех поддерживаемых языков
        /// </summary>
        /// <returns>Словарь с языками и их нативными именами</returns>
        public static IReadOnlyDictionary<SystemLanguage, string> GetAllNativeNames()
        {
            return SystemLanguageToNativeName;
        }
    }
} 
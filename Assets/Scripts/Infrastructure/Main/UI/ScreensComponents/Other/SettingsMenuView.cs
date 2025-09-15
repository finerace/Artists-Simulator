using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Infrastructure.Main.UI
{
    public class SettingsMenuView : UniversalMenuView
    {
        [Header("Settings Controls")]
        [SerializeField] private Toggle soundToggle;
        [SerializeField] private Toggle musicToggle;
        [SerializeField] private TMP_Dropdown graphicsDropdown;
        [SerializeField] private TMP_Dropdown languageDropdown;

        public Toggle SoundToggle => soundToggle;
        public Toggle MusicToggle => musicToggle;
        public TMP_Dropdown GraphicsDropdown => graphicsDropdown;
        public TMP_Dropdown LanguageDropdown => languageDropdown;
    }
} 
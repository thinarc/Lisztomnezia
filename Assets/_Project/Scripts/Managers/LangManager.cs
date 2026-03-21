using UnityEngine.Localization.Settings;

namespace _Project.Scripts.Managers
{
    public class LangManager
    {
        public static LangManager Instance { get; private set; }

        public void Initialize(int lang)
        {
            Instance = this;
            
            LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[lang];
        }
    }
}
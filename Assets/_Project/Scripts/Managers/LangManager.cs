using UnityEngine.Localization.Settings;

namespace _Project.Scripts.Managers
{
    public static class LangManager
    {
        public static void InitLocale(int lang)
        {
            LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[lang];
        }
    }
}
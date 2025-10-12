using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;

public class LanguageSwitcherScript : MonoBehaviour
{
    [Tooltip("Список TextMeshPro элементов — один на каждый язык. " +
             "Порядок должен совпадать с порядком языков в LocalizationSettings.")]
    public List<TextMeshProUGUI> languageLabels;

    private int currentIndex = 0;

    private const string PlayerPrefsKey = "SelectedLanguageIndex";

    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    private void Start()
    {
        if (PlayerPrefs.HasKey(PlayerPrefsKey))
            currentIndex = PlayerPrefs.GetInt(PlayerPrefsKey);

        // Деактивируем все языки, кроме первого
        for (int i = 0; i < languageLabels.Count; i++)
            languageLabels[i].gameObject.SetActive(i == currentIndex);

        // Устанавливаем начальный язык
        if (LocalizationSettings.AvailableLocales.Locales.Count > 0)
        {
            LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[currentIndex];
        }
    }

    public void OnLanguageSelect()
    {
        CycleLanguage();
    }

    private void CycleLanguage()
    {
        // Деактивируем текущий
        languageLabels[currentIndex].gameObject.SetActive(false);

        // Следующий индекс
        currentIndex = (currentIndex + 1) % languageLabels.Count;

        // Активируем следующий
        languageLabels[currentIndex].gameObject.SetActive(true);

        SetLocale(currentIndex);

        PlayerPrefs.SetInt(PlayerPrefsKey, currentIndex);
        PlayerPrefs.Save();
    }

    private void SetLocale(int index)
    {
        var locales = LocalizationSettings.AvailableLocales.Locales;
        if (index < locales.Count)
        {
            LocalizationSettings.SelectedLocale = locales[index];
        }
        else
        {
            Debug.LogWarning("Индекс локали превышает количество доступных языков.");
        }
    }
}

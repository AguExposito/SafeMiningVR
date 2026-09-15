using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;
using XRStandardMenu;
public class TranslatableText : MonoBehaviour
{
    private TextMeshProUGUI tmpComponent;

    [Header("Settings for this text")]
    public string translationID;

    private void Start()
    {
        InitializeComponents();
        SubscribeToLanguageChanges();
        
        // Esperar a que el LanguageManager esté listo antes de actualizar el texto
        StartCoroutine(WaitForLanguageManagerAndUpdate());
    }

    private void OnDestroy()
    {
        UnsubscribeFromLanguageChanges();
    }

    private void InitializeComponents()
    {
        tmpComponent = GetComponent<TextMeshProUGUI>();
    }

    private void SubscribeToLanguageChanges()
    {
        if (ControlCenter.Instance == null || ControlCenter.Instance.languageManager == null)
        {
            return;
        }

        ControlCenter.Instance.languageManager.OnLanguageChanged += OnLanguageChanged;
    }

    private void UnsubscribeFromLanguageChanges()
    {
        if (ControlCenter.Instance == null) return;
        if (ControlCenter.Instance.languageManager != null)
        {
            ControlCenter.Instance.languageManager.OnLanguageChanged -= OnLanguageChanged;
        }
    }

    private void OnLanguageChanged(LanguageName newLanguage)
    {
        UpdateText();
    }

    private IEnumerator WaitForLanguageManagerAndUpdate()
    {
        while (ControlCenter.Instance == null ||
               ControlCenter.Instance.languageManager == null ||
               !ControlCenter.Instance.languageManager.resourcesLoaded.Value)
        {
            if (ControlCenter.Instance == null)
            {
                yield break;
            }

            yield return null;
        }

        UpdateText();
    }

    public void UpdateText()
    {
        if (!isActiveAndEnabled)
        {
            return;
        }

        if (translationID.StartsWith("$"))
        {
            ExceptionSystemTexts(translationID);
            return;
        }
        string translatedText = GetTranslatedText();

        if (tmpComponent != null)
        {
            tmpComponent.text = translatedText;
        }
    }
    void ExceptionSystemTexts(string id)
    {
        tmpComponent.text = id switch
        {
            "$version" => "V " + Application.version.ToString(),
            "$companyName" => Brand.CompanyName,
            "$productName" => Brand.ProductName,
            _ => "Not founded exception id string"
        };
    }

    private string GetTranslatedText()
    {
        // Product display name is centralized in BrandConfig (overrides lang XML simName).
        if (translationID == "simName")
        {
            return Brand.ProductName;
        }

        if (ControlCenter.Instance?.languageManager == null)
        {
            return $"ERROR: Missing Language Manager - ID: {translationID}";
        }

        if (!ControlCenter.Instance.languageManager.resourcesLoaded.Value)
        {
            return $"Loading translations... - ID: {translationID}";
        }

        return ControlCenter.Instance.languageManager.GetText(translationID);
    }

    public void ChangeTextLanguage()
    {
        UpdateText();
    }
}

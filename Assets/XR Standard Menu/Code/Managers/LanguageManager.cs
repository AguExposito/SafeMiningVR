using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Linq;
using System.Linq;
using UnityEngine.SceneManagement;
using System;
using System.IO;
using TMPro;
using NJ.Observable;
using NJ.Finder;

namespace XRStandardMenu
{
    public enum LanguageName { Spanish, English };

    [System.Serializable]
    public class Language
    {
        [SerializeField] public LanguageName language;
        [SerializeField] public string filename;
    }

    public class LanguageManager : MonoBehaviour
    {
        [Header("Default settings")]
        [SerializeField] private LanguageName _currentLanguage = LanguageName.Spanish;
        public Language[] languages = new Language[] 
        { 
            new Language() { language = LanguageName.Spanish, filename = "es" },
            new Language() { language = LanguageName.English, filename = "en" }
        };
        private Dictionary<string, Dictionary<string, string>> translations;

        [Header("UI References")]
        [SerializeField] private TMP_Dropdown languageDropdown;

        [Header("Internal variables")]
        public Observable<bool> resourcesLoaded = new Observable<bool>(false);

        public event Action<LanguageName> OnLanguageChanged;

        public LanguageName CurrentLanguage
        {
            get => _currentLanguage;
            set
            {
                if (_currentLanguage != value)
                {
                    _currentLanguage = value;
                    
                    // Guardar el idioma de forma persistente
                    var persistentData = ControlCenter.Instance.persistantDataManager.generalPersistantData;
                    persistentData.language = (int)value;
                    ControlCenter.Instance.persistantDataManager.generalPersistantData = persistentData;
                    
                    // Forzar el guardado inmediato
                    ControlCenter.Instance.persistantDataManager.SaveGeneralData();
                    
                    OnLanguageChanged?.Invoke(value);
                    LoadLanguageFromConfig();
                    Init();
                    StartCoroutine(SetupLanguageDropdownDelayed());
                    UpdateAllTranslatableTexts();
                }
            }
        }
        public void Initialize()
        {
            LoadLanguageFromConfig();
            Init();
        }

        private void LoadLanguageFromConfig()
        {
            _currentLanguage = (LanguageName)ControlCenter.Instance.persistantDataManager.generalPersistantData.language;
            Debug.Log($"Language loaded from config: {_currentLanguage}");
        }

        private void Init()
        {
            translations = new Dictionary<string, Dictionary<string, string>>();
            LoadLanguagesFromResources();
            LoadLanguagesFromConfig();

            if (translations.Count == 0)
            {
                Debug.LogError("No language files found in Assets/XR Standard Menu/Lang/ or configured manually");
                return;
            }

            resourcesLoaded.Value = true;
        }

        private void LoadLanguagesFromResources()
        {
            // Buscar archivos XML en Assets/XR Standard Menu/Lang/
            string langPath = Path.Combine(Application.dataPath, "XR Standard Menu", "Lang");

            if (!Directory.Exists(langPath))
            {
                Debug.LogError($"Language directory not found: {langPath}");
                return;
            }

            string[] xmlFiles = Directory.GetFiles(langPath, "*.xml", SearchOption.TopDirectoryOnly);

            foreach (string filePath in xmlFiles)
            {
                try
                {
                    string fileName = Path.GetFileName(filePath);
                    string xmlContent = File.ReadAllText(filePath);

                    // Intentar determinar el idioma por el nombre del archivo
                    LanguageName language = DetermineLanguageFromFilename(fileName);

                    Dictionary<string, string> collected = ParseXmlResource(xmlContent);
                    if (collected.Count > 0)
                    {
                        MergeLanguageEntries(language.ToString(), collected);
                        Debug.Log($"Loaded language file: {fileName} -> {language}");
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error parsing XML file {filePath}: {e.Message}");
                }
            }
        }

        private LanguageName DetermineLanguageFromFilename(string filename)
        {
            string lowerFilename = filename.ToLower();

            if (lowerFilename.Contains("spanish") || lowerFilename.Contains("español") || lowerFilename.Contains("es"))
                return LanguageName.Spanish;

            if (lowerFilename.Contains("english") || lowerFilename.Contains("inglés") || lowerFilename.Contains("en"))
                return LanguageName.English;

            // Por defecto, usar español
            return LanguageName.Spanish;
        }

        private void LoadLanguagesFromConfig()
        {
            foreach (Language language in languages)
            {
                if (string.IsNullOrEmpty(language.filename))
                {
                    Debug.LogError($"Language {language.language} has empty filename");
                    continue;
                }

                TextAsset resource = Resources.Load<TextAsset>(language.filename);
                if (resource == null)
                {
                    Debug.LogError($"Could not load resource: {language.filename}");
                    continue;
                }

                try
                {
                    Dictionary<string, string> collected = ParseXmlResource(resource.text);
                    if (collected.Count > 0)
                    {
                        MergeLanguageEntries(language.language.ToString(), collected);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error parsing XML for {language.language}: {e.Message}");
                }
            }
        }

        private void MergeLanguageEntries(string languageKey, Dictionary<string, string> entries)
        {
            if (!translations.TryGetValue(languageKey, out Dictionary<string, string> currentDictionary))
            {
                currentDictionary = new Dictionary<string, string>();
                translations[languageKey] = currentDictionary;
            }
            foreach (var item in entries)
            {
                currentDictionary[item.Key] = item.Value;
            }
        }

        private Dictionary<string, string> ParseXmlResource(string xmlContent)
        {
            Dictionary<string, string> collected = new Dictionary<string, string>();
            var parsed = XDocument.Parse(xmlContent);

            foreach (XElement element in parsed.Root.Elements())
            {
                var nameAttribute = element.Attribute("name");
                if (nameAttribute != null && !string.IsNullOrEmpty(nameAttribute.Value))
                {
                    collected[nameAttribute.Value] = element.Value;
                }
            }

            return collected;
        }

        public string GetText(string id)
        {
            return GetText(id, _currentLanguage);
        }

        public string GetText(string id, LanguageName language)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                Debug.LogWarning("Text ID is null or empty");
                return string.Empty;
            }
            id = id.Trim();

            // Verificar que las traducciones estén cargadas
            if (translations == null)
            {
                Debug.LogWarning("Translations not loaded yet, returning empty string");
                return string.Empty;
            }

            string languageKey = language.ToString();
            if (translations.TryGetValue(languageKey, out Dictionary<string, string> languageDict))
            {
                if (languageDict.TryGetValue(id, out string value))
                {
                    return value;
                }
                else
                {
                    Debug.LogWarning($"Text ID '{id}' not found in language '{language}'");
                }
            }
            else
            {
                Debug.LogWarning($"Language '{language}' not found in translations");
            }

            return id;
        }

        public bool HasText(string id)
        {
            return HasText(id, _currentLanguage);
        }

        public bool HasText(string id, LanguageName language)
        {
            if (string.IsNullOrEmpty(id)) return false;

            string languageKey = language.ToString();
            return translations.TryGetValue(languageKey, out Dictionary<string, string> languageDict) &&
                   languageDict.ContainsKey(id);
        }

        public void ChangeLanguage(LanguageName newLanguage)
        {
            CurrentLanguage = newLanguage;
            UpdateAllTranslatableTexts();
        }

        public void UpdateAllTranslatableTexts()
        {
            TranslatableText[] allTranslatableTexts = FindObjectsByType<TranslatableText>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID);

            foreach (TranslatableText translatableText in allTranslatableTexts)
            {
                if (translatableText != null)
                {
                    translatableText.UpdateText();
                }
            }

            Debug.Log($"Updated {allTranslatableTexts.Length} translatable text components");
        }

        private IEnumerator SetupLanguageDropdownDelayed()
        {
            // Esperar un frame para asegurar que todos los objetos estén inicializados
            yield return null;

            // Buscar el dropdown por nombre
            FindAndSetupLanguageDropdown();
        }

        private void FindAndSetupLanguageDropdown()
        {
            // Buscar el dropdown por nombre en toda la escena
            TMP_Dropdown[] foundDropdowns = ComponentFinder.FindAllComponentsInScene<TMP_Dropdown>();
            TMP_Dropdown foundedDropdown = null;
            foreach (TMP_Dropdown item in foundDropdowns)
            {
                if (item.gameObject.name == "LanguageDropdown")
                {
                    foundedDropdown = item;
                    break;
                }
            }

            if (foundedDropdown != null && foundedDropdown.gameObject.name == "LanguageDropdown")
            {
                languageDropdown = foundedDropdown;
                SetupLanguageDropdown();
                Debug.Log("LanguageDropdown encontrado y configurado automáticamente");
            }
            else
            {
                // Si no se encuentra, buscar por nombre específico
                GameObject dropdownObject = GameObject.Find("LanguageDropdown");
                if (dropdownObject != null)
                {
                    languageDropdown = dropdownObject.GetComponent<TMP_Dropdown>();
                    if (languageDropdown != null)
                    {
                        SetupLanguageDropdown();
                        Debug.Log("LanguageDropdown encontrado por nombre y configurado");
                    }
                    else
                    {
                        Debug.LogWarning("Objeto LanguageDropdown encontrado pero no tiene componente TMP_Dropdown");
                    }
                }
                else
                {
                    Debug.Log("LanguageDropdown no encontrado en la escena actual - LanguageManager funcionará sin dropdown");
                }
            }
        }

        private void SetupLanguageDropdown()
        {
            if (languageDropdown == null)
            {
                Debug.LogWarning("Language dropdown not assigned in LanguageManager");
                return;
            }

            // Limpiar opciones existentes
            languageDropdown.ClearOptions();

            // Crear lista de opciones con imágenes basada en los idiomas disponibles
            List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();
            foreach (LanguageName language in System.Enum.GetValues(typeof(LanguageName)))
            {
                string languageDisplayName = GetLanguageDisplayName(language);
                Sprite flagSprite = GetFlagSprite(language);
                
                TMP_Dropdown.OptionData optionData = new TMP_Dropdown.OptionData(languageDisplayName, flagSprite, new Color32(52,57,63,255));
                options.Add(optionData);
            }

            // Agregar opciones al dropdown
            languageDropdown.AddOptions(options);

            // Establecer el idioma actual como seleccionado
            languageDropdown.value = (int)_currentLanguage;

            // Suscribirse al evento de cambio
            languageDropdown.onValueChanged.RemoveAllListeners();
            languageDropdown.onValueChanged.AddListener(OnLanguageDropdownChanged);

            // Suscribirse a eventos del dropdown para configurar banderas cuando se abra
            SubscribeToDropdownEvents();

            // Configurar la bandera del item seleccionado (que está en el template)
            ConfigureSelectedItemFlag();

            Debug.Log($"Language dropdown configured with {options.Count} options");
        }

        private void SubscribeToDropdownEvents()
        {
            // Suscribirse a eventos del dropdown para detectar cuando se abre
            if (languageDropdown != null)
            {
                // Usar una corrutina que verifique periódicamente si el dropdown está abierto
                StartCoroutine(MonitorDropdownState());
            }
        }

        private System.Collections.IEnumerator MonitorDropdownState()
        {
            bool wasOpen = false;
            
            while (languageDropdown != null)
            {
                // Verificar si el dropdown está abierto buscando el Dropdown List
                Transform dropdownList = languageDropdown.transform.Find("Dropdown List");
                bool isOpen = dropdownList != null && dropdownList.gameObject.activeInHierarchy;
                
                // Si el dropdown se acaba de abrir, configurar las banderas
                if (isOpen && !wasOpen)
                {
                    Debug.Log("Dropdown opened, configuring flags");
                    yield return new WaitForEndOfFrame(); // Esperar un frame para que se genere completamente
                    ConfigureDropdownFlagImages();
                }
                
                wasOpen = isOpen;
                yield return new WaitForSeconds(0.1f); // Verificar cada 0.1 segundos
            }
        }

        private string GetLanguageDisplayName(LanguageName language)
        {
            switch (language)
            {
                case LanguageName.Spanish:
                    return "Español";
                case LanguageName.English:
                    return "English";
                default:
                    return language.ToString();
            }
        }

        private Sprite GetFlagSprite(LanguageName language)
        {
            if (SBD_Images.Instance == null)
            {
                Debug.LogWarning("SBD_Images.Instance is null, cannot get flag sprite");
                return null;
            }

            switch (language)
            {
                case LanguageName.Spanish:
                    return SBD_Images.Instance.UI_spanish_flag;
                case LanguageName.English:
                    return SBD_Images.Instance.UI_english_flag;
                default:
                    Debug.LogWarning($"No flag sprite defined for language: {language}");
                    return null;
            }
        }

        private void ConfigureDropdownFlagImages()
        {
            if (languageDropdown == null)
            {
                Debug.LogWarning("Language dropdown is null, cannot configure flag images");
                return;
            }

            // Ahora que sabemos que el dropdown está abierto, configurar directamente
            StartCoroutine(ConfigureFlagsAfterDropdownUpdate());
        }

        private System.Collections.IEnumerator ConfigureFlagsAfterDropdownUpdate()
        {
            // Esperar un frame para que el dropdown genere los items
            yield return null;

            // Buscar el "Dropdown List" que se genera dinámicamente en runtime
            Transform dropdownList = languageDropdown.transform.Find("Dropdown List");
            if (dropdownList == null)
            {
                Debug.LogWarning("Dropdown List not found");
                yield break;
            }

            // Buscar el viewport dentro del Dropdown List
            Transform viewport = dropdownList.Find("Viewport");
            if (viewport == null)
            {
                Debug.LogWarning("Dropdown viewport not found");
                yield break;
            }

            // Buscar el content dentro del viewport
            Transform content = viewport.Find("Content");
            if (content == null)
            {
                Debug.LogWarning("Dropdown content not found");
                yield break;
            }

            // Configurar las banderas en cada item
            // El dropdown tiene: 0=item desactivado, 1=Español, 2=Inglés
            LanguageName[] languages = (LanguageName[])System.Enum.GetValues(typeof(LanguageName));
            
            // Iterar sobre los items del dropdown (saltando el item desactivado en posición 0)
            for (int dropdownIndex = 1; dropdownIndex < content.childCount; dropdownIndex++)
            {
                int languageIndex = dropdownIndex - 1; // Convertir índice del dropdown a índice del enum
                
                if (languageIndex >= 0 && languageIndex < languages.Length)
                {
                    Transform item = content.GetChild(dropdownIndex);
                    
                    // Buscar el hijo llamado "flag"
                    Transform flagChild = item.Find("flag");
                    if (flagChild != null)
                    {
                        UnityEngine.UI.Image flagImage = flagChild.GetComponent<UnityEngine.UI.Image>();
                        if (flagImage != null)
                        {
                            Sprite flagSprite = GetFlagSprite(languages[languageIndex]);
                            if (flagSprite != null)
                            {
                                flagImage.sprite = flagSprite;
                                Debug.Log($"Flag configured for {languages[languageIndex]} in dropdown item {dropdownIndex} (language index {languageIndex})");
                            }
                            else
                            {
                                Debug.LogWarning($"No flag sprite found for {languages[languageIndex]}");
                            }
                        }
                        else
                        {
                            Debug.LogWarning($"Flag child found but no Image component in dropdown item {dropdownIndex}");
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"Flag child not found in dropdown item {dropdownIndex}");
                    }
                }
            }

            // También configurar la bandera del item seleccionado (el que se muestra cuando está cerrado)
            ConfigureSelectedItemFlag();
        }

        private void ConfigureSelectedItemFlag()
        {
            if (languageDropdown == null) return;

            // El item seleccionado está en el template, no en el Dropdown List
            // Buscar el template del dropdown
            Transform template = languageDropdown.transform.Find("Template");
            if (template == null)
            {
                Debug.LogWarning("Dropdown template not found");
                return;
            }

            // Buscar el viewport dentro del template
            Transform viewport = template.Find("Viewport");
            if (viewport == null)
            {
                Debug.LogWarning("Template viewport not found");
                return;
            }

            // Buscar el content dentro del viewport
            Transform content = viewport.Find("Content");
            if (content == null)
            {
                Debug.LogWarning("Template content not found");
                return;
            }

            // Buscar el primer item (que es el item seleccionado)
            Transform selectedItem = content.GetChild(0);
            if (selectedItem == null)
            {
                Debug.LogWarning("Selected item not found in template");
                return;
            }

            // Buscar el hijo llamado "flag" en el item seleccionado
            Transform flagChild = selectedItem.Find("flag");
            if (flagChild != null)
            {
                UnityEngine.UI.Image flagImage = flagChild.GetComponent<UnityEngine.UI.Image>();
                if (flagImage != null)
                {
                    Sprite flagSprite = GetFlagSprite(_currentLanguage);
                    if (flagSprite != null)
                    {
                        flagImage.sprite = flagSprite;
                        Debug.Log($"Flag configured for selected item: {_currentLanguage}");
                    }
                }
            }
            else
            {
                Debug.LogWarning("Flag child not found in selected item");
            }
        }

        private void OnLanguageDropdownChanged(int selectedIndex)
        {
            // En TMP_Dropdown, selectedIndex corresponde directamente a las opciones añadidas (0..n-1)
            if (selectedIndex >= 0 && selectedIndex < System.Enum.GetValues(typeof(LanguageName)).Length)
            {
                LanguageName selectedLanguage = (LanguageName)selectedIndex;

                if (selectedLanguage != _currentLanguage)
                {
                    Debug.Log($"Language changed from dropdown: {_currentLanguage} -> {selectedLanguage} (dropdown index {selectedIndex})");
                    ChangeLanguage(selectedLanguage);
                    
                    // Actualizar la bandera del item seleccionado después del cambio
                    StartCoroutine(UpdateSelectedFlagAfterChange());
                }
            }
        }

        private System.Collections.IEnumerator UpdateSelectedFlagAfterChange()
        {
            // Esperar un frame para que el cambio se complete
            yield return null;
            ConfigureSelectedItemFlag();
        }

        public void SetLanguageDropdown(TMP_Dropdown dropdown)
        {
            languageDropdown = dropdown;
            if (languageDropdown != null)
            {
                SetupLanguageDropdown();
            }
        }

        // Método público para buscar el dropdown cuando cambie de escena
        public void RefreshLanguageDropdown()
        {
            FindAndSetupLanguageDropdown();
        }

        // Método para suscribirse a cambios de escena
        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // Cuando se carga una nueva escena, buscar el dropdown nuevamente
            StartCoroutine(RefreshDropdownAfterSceneLoad());
        }

        private System.Collections.IEnumerator RefreshDropdownAfterSceneLoad()
        {
            // Esperar un frame para que la escena esté completamente cargada
            yield return null;
            FindAndSetupLanguageDropdown();
        }
    }
}
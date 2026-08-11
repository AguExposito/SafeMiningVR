using UnityEngine;

/// <summary>
/// Central branding asset. Edit this (or duplicate it per client) to rebrand the app.
/// Loaded from Resources/BrandConfig by default. Assign an override on ControlCenter if needed.
/// Project Settings (Player) only hold OS build identity — sync them from the Branding menu.
/// </summary>
[CreateAssetMenu(fileName = "BrandConfig", menuName = "XR Standard Menu/Brand Config")]
public class BrandConfig : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string companyName = "DefaultCompany";
    [SerializeField] private string productName = "XR Menu";

    [Header("Logo")]
    [SerializeField] private Sprite logo;

    [Header("Brand Colors")]
    [SerializeField] private Color primaryColor = new Color(0.12f, 0.45f, 0.58f, 1f);
    [SerializeField] private Color secondaryColor = new Color(0.18f, 0.20f, 0.22f, 1f);
    [SerializeField] private Color tertiaryColor = new Color(0.91f, 0.94f, 0.96f, 1f);

    public string CompanyName => companyName;
    public string ProductName => productName;
    public Sprite Logo => logo;
    public Color PrimaryColor => primaryColor;
    public Color SecondaryColor => secondaryColor;
    public Color TertiaryColor => tertiaryColor;

    public Color GetColor(BrandColorRole role)
    {
        return role switch
        {
            BrandColorRole.Primary => primaryColor,
            BrandColorRole.Secondary => secondaryColor,
            BrandColorRole.Tertiary => tertiaryColor,
            _ => Color.white
        };
    }

    public string GetText(BrandTextRole role)
    {
        return role switch
        {
            BrandTextRole.CompanyName => companyName,
            BrandTextRole.ProductName => productName,
            _ => string.Empty
        };
    }
}

public enum BrandColorRole
{
    None = 0,
    Primary = 1,
    Secondary = 2,
    Tertiary = 3
}

public enum BrandTextRole
{
    None = 0,
    CompanyName = 1,
    ProductName = 2
}

public enum BrandGraphicRole
{
    None = 0,
    Logo = 1
}

/// <summary>
/// Runtime access to the active <see cref="BrandConfig"/>.
/// </summary>
public static class Brand
{
    public const string ResourcesPath = "BrandConfig";

    private static BrandConfig overrideConfig;
    private static BrandConfig cached;

    public static BrandConfig Config
    {
        get
        {
            if (overrideConfig != null)
            {
                return overrideConfig;
            }
            if (cached == null)
            {
                cached = Resources.Load<BrandConfig>(ResourcesPath);
            }
            return cached;
        }
    }

    public static bool HasConfig => Config != null;

    public static string CompanyName => HasConfig ? Config.CompanyName : Application.companyName;
    public static string ProductName => HasConfig ? Config.ProductName : Application.productName;
    public static Sprite Logo => HasConfig ? Config.Logo : null;
    public static Color Primary => HasConfig ? Config.PrimaryColor : Color.white;
    public static Color Secondary => HasConfig ? Config.SecondaryColor : Color.gray;
    public static Color Tertiary => HasConfig ? Config.TertiaryColor : Color.white;

    public static void SetOverride(BrandConfig config)
    {
        overrideConfig = config;
        cached = null;
    }

    public static void ClearOverride()
    {
        overrideConfig = null;
        cached = null;
    }

    public static void ClearCache()
    {
        cached = null;
    }

    public static Color GetColor(BrandColorRole role)
    {
        return HasConfig ? Config.GetColor(role) : Color.white;
    }

    public static string GetText(BrandTextRole role)
    {
        if (!HasConfig)
        {
            return role switch
            {
                BrandTextRole.CompanyName => Application.companyName,
                BrandTextRole.ProductName => Application.productName,
                _ => string.Empty
            };
        }
        return Config.GetText(role);
    }
}

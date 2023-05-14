public class MultiLanguageString
{
    public string? EnglishText { get; set; }
    
    public MultiLanguageString(string text, Language language = 0)
    {
        switch (language)
        {
            case Language.English:
                EnglishText = text;
                break;
        }
    }

    public void SetLanguageText(string text, Language language = 0)
    {
        switch (language)
        {
            case Language.English:
                EnglishText = text;
                break;
        }
    }

    public string? ToString(Language language = 0)
    {
        switch (language)
        {
            case Language.English:
                return EnglishText;
            default: 
                return EnglishText;
        }
    }

    public override string? ToString()
    {
        return EnglishText;
    }

    public static implicit operator MultiLanguageString(string locKey)
    {
        return new MultiLanguageString(locKey);
    }
}

public enum Language
{
    English = 0
}
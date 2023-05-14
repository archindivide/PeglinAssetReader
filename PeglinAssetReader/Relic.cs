public class Relic
{
    public string LocKey { get; set; }
    public MultiLanguageString? Name { get; set; }
    public MultiLanguageString? Desc { get; set; }
    public MultiLanguageString? Desc2 { get; set; }
    public MultiLanguageString? Desc3 { get; set; }
    public MultiLanguageString? DescCombined
    {
        get
        {
            return Desc ?? Desc2 ?? Desc3;
        }
    }
    public string? SpriteGuid { get; set; }
    public string? ImageFileName { get; set; }
    public string? OutputImageFilePath { get; set; }
    public Relic(string locKey)
    {
        LocKey = locKey;
    }
}
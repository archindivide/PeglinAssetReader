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

    public void FixDescriptions(Dictionary<string,string> constants)
    {
        Desc = FixDescription(constants, Desc);
        Desc2 = FixDescription(constants, Desc2); 
        Desc3 = FixDescription(constants, Desc3);
    }

    public MultiLanguageString? FixDescription(Dictionary<string, string> constants, MultiLanguageString? description)
    {
        if(description?.EnglishText == null)
        {
            return null;
        }

        string english = description.EnglishText
            //replace images used in game with readable text
            .Replace("<sprite name=\"BOMB\">", "Bombs")
            .Replace("<sprite name=\"BOMB_REGULAR\">", "Bombs")
            .Replace("<sprite name=\"RIGGED_BOMB\">", "Red Bombs")
            .Replace("<sprite name=\"PEG_COIN\">", "Coins")
            .Replace("<sprite name=\"COIN_ONLY\">", "Coins")
            .Replace("<sprite name=\"GOLD\">", "Gold")
            .Replace("<sprite name=\"CRIT_PEG\">", "Critical Peg")
            .Replace("<sprite name=\"REFRESH_PEG\">", "Refresh Peg")
            .Replace("<sprite name=\"PEG_SHIELDED\">", "Shield Peg")
            .Replace("<sprite name=PEG>", "Peg")
            .Replace("<sprite name=\"PEG\">", "Peg")
            .Replace("<sprite name=\"PEG>", "Peg")
            .Replace("<sprite name=\"PEG_CLEARED\">", "Cleared Peg")
            //remove styles, maybe should be applied instead
            .Replace("<style=dmg_bonus>", "")
            .Replace("<style=dmg_negative>", "")
            .Replace("<style=morbid>", "")
            .Replace("<style=ballwark>", "")
            .Replace("<style=activate>", "")
            .Replace("<style=heal>", "")
            .Replace("<style=blind>", "")
            .Replace("<style=hit>", "")
            .Replace("<style=confuse>", "")
            .Replace("<style=damage>", "")
            .Replace("<style=echo>", "")
            .Replace("<style=multiball>", "")
            .Replace("<style=overflow>", "")
            .Replace("<style=finesse>", "")
            .Replace("<style=strength>", "")
            .Replace("<style=balance>", "")
            .Replace("<style=persist>", "")
            .Replace("<style=dexterity>", "")
            .Replace("<style=durable>", "")
            .Replace("<style=shield>", "")
            .Replace("<style=bramble>", "")
            .Replace("<style=Bramble>", "")
            .Replace("<style=poison>", "")
            .Replace("</style>", "")
            //manual fixes
            .Replace("{[STR_RELOAD_COUNT]} reloads", "reload")
            .Replace("Critical Peg Refresh Peg", "Critical Peg/Refresh Peg");

        foreach (var constant in constants)
        {
            if (english.Contains(constant.Key))
            {
                english = english.Replace($"{{[{constant.Key}]}}", constant.Value);
            }
        }

        //Will need to be done for each language

        MultiLanguageString result = new MultiLanguageString(english);

        return result;
    }
}
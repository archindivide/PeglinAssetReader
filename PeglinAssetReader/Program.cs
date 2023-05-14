using HandlebarsDotNet;
using System.ComponentModel.Design.Serialization;
using System.Configuration;
using System.Diagnostics;
using System.Text.RegularExpressions;

var peglinPath = ConfigurationManager.AppSettings["PeglinFolder"];
var assetRipperPath = ConfigurationManager.AppSettings["AssetRipperApplication"];
var assetRipperOutputPath = $"{Environment.CurrentDirectory}/asset-ripper-output";
var assetPath = $"{assetRipperOutputPath}/ExportedProject/Assets";
var languageFile = $"{assetRipperOutputPath}/ExportedProject/Assets/Resources/I2Languages.asset";
var assetFilePath = $"{assetPath}/MonoBehaviour";
var spriteMetaDataFilePath = $"{assetPath}/Sprite";
var spriteImageFilePath = $"{assetPath}/Texture2D";
var paramFile = $"{assetPath}/Scenes/Battle.unity";
var outputPath = $"{Environment.CurrentDirectory}/output";
var imageOutputPath = $"{outputPath}/img";

var ignoreKeys = new List<string>() 
{
    //"damage_bonus_plant_flat",
    //"damage_bonus_slime_flat",
    //"healing_slime",
    //"heal_on_reload_peg_num"
};

//Run AssetRipper to extract Peglin files from compiled code
if (Directory.Exists(assetRipperOutputPath) && Directory.GetFileSystemEntries(assetRipperOutputPath).Length > 0)
{
    Console.WriteLine("Skipping AssetRipper step because there are already files in the output path.");
}
else
{
    var proc = new Process();
    proc.StartInfo.FileName = assetRipperPath;
    proc.StartInfo.Arguments = $"{peglinPath} -o {assetRipperOutputPath} -q";
    proc.Start();
    proc.WaitForExit();
    var exitCode = proc.ExitCode;
    proc.Close();
}

//Parse params (for constants used in descriptions)
var paramText = File.ReadAllText(paramFile);
var startString = "_Params:";
var endString = "_IsGlobalManager";

var index = paramText.IndexOf(startString);
var nextIndex = index;
Dictionary<string, string> constants = new Dictionary<string, string>();

while ((index = nextIndex) > 0)
{
    var toParse = paramText.Substring(index, paramText.IndexOf(endString, index + 1) - index);
    var lines = toParse.Split('\n');

    var tempName = "";
    var tempValue = "";

    foreach(var line in lines)
    {
        if (line.Contains("Name"))
        {
            var split = line.Split(':');
            tempName = split[1].Trim();
        }
        else if (line.Contains("Value"))
        {
            var split = line.Split(':');
            tempValue = split[1].Trim();
            if(!constants.Any(c => c.Key == tempName))
            {
                constants.Add(tempName, tempValue);
            }
        }
    }

    nextIndex = paramText.IndexOf(startString, index + 1);
}

//Parse language file
var languageText = File.ReadAllText(languageFile);

if(languageText == null)
{
    Console.WriteLine("Language File not found or couldn't be read");
    return;
}

var relicIdentifierString = "- Term: Relics/";
index = languageText.IndexOf(relicIdentifierString);
nextIndex = index;

var relics = new List<Relic>();

while((index = nextIndex) < languageText.Length)
{
    nextIndex = languageText.IndexOf(relicIdentifierString, index + 1);
    if(nextIndex < 0)
    {
        nextIndex = languageText.Length;
    }
    var toParse = languageText.Substring(index, nextIndex - index);
    var lines = toParse.Split('\n');

    Regex resouceNameRegex = new Regex("Relics/(.*)");
    Match m = resouceNameRegex.Match(toParse);
    var resourceName = m.Groups[1].Value;

    var locKey = resourceName.Replace("_desc3", "").Replace("_desc2", "").Replace("_desc", "").Replace("_name", "");
    var currentRelic = relics.Where(r => r.LocKey == locKey).FirstOrDefault();

    if(ignoreKeys.Any(i => i == locKey))
    {
        continue;
    }

    if(currentRelic == null)
    {
        currentRelic = new Relic(locKey);
        relics.Add(currentRelic);
    }

    if (resourceName.EndsWith("_desc"))
    {
        currentRelic.Desc = lines[3].Substring(lines[3].IndexOf('-') + 2, lines[3].Length - (lines[3].IndexOf('-') + 2));
        //add parsing for other languages
    }
    else if (resourceName.EndsWith("_desc2"))
    {
        currentRelic.Desc2 = lines[3].Substring(lines[3].IndexOf('-') + 2, lines[3].Length - (lines[3].IndexOf('-') + 2));
        //add parsing for other languages
    }
    else if (resourceName.EndsWith("_desc3"))
    {
        currentRelic.Desc3 = lines[3].Substring(lines[3].IndexOf('-') + 2, lines[3].Length - (lines[3].IndexOf('-') + 2));
        //add parsing for other languages
    }
    else if (resourceName.EndsWith("_name"))
    {
        currentRelic.Name = lines[3].Substring(lines[3].IndexOf('-') + 2, lines[3].Length - (lines[3].IndexOf('-') + 2));
        //add parsing for other languages
    }
    else
    {
        Console.WriteLine($"Couldn't resolve resource: {resourceName}");
    }
}

//Load all the asset files
List<string> fileTexts = new List<string>();
var paths = Directory.GetFiles(assetFilePath);

foreach (var path in paths)
{
    fileTexts.Add(File.ReadAllText(path));
}

//Parse asset files for sprite guids
foreach(var relic in relics)
{
    var file = fileTexts.Where(t => t.Contains(relic.LocKey)).FirstOrDefault();
    if (file != null)
    {
        Regex regex = new Regex("sprite:.*guid: (.*),");
        Match m = regex.Match(file);
        relic.SpriteGuid = m.Groups[1].Value;
    }
}

fileTexts.Clear(); //no need to keep this in memory, might be used later

//Load all the sprite metadata files
paths = Directory.GetFiles(spriteMetaDataFilePath);
Dictionary<string,string> pathFileTexts = new Dictionary<string,string>();

foreach (var path in paths)
{
    pathFileTexts.Add(path, File.ReadAllText(path));
}

//Parse the sprite meta data files to get the image file names
foreach (var relic in relics.Where(r => r.SpriteGuid != null))
{
    var file = pathFileTexts.FirstOrDefault(t => t.Value.Contains(relic.SpriteGuid!));
    if (file.Value != null)
    {
        relic.ImageFileName = file.Key.Substring(file.Key.LastIndexOf('\\') + 1, file.Key.Length - (file.Key.LastIndexOf('\\') + 1)).Replace(".asset.meta", ".png");
    }
}

pathFileTexts.Clear(); //no need to keep this in memory, might be used later

//Create/clear output directory
if (!Directory.Exists(outputPath))
{
    Directory.CreateDirectory(outputPath);
}
else
{
    Directory.Delete(outputPath, true);
    Directory.CreateDirectory(outputPath);
}
if (!Directory.Exists(imageOutputPath))
{
    Directory.CreateDirectory(imageOutputPath);
}
else
{
    Directory.Delete(imageOutputPath, true);
    Directory.CreateDirectory(imageOutputPath);
}

//Move image files into output folder
foreach(var relic in relics.Where(r => r.ImageFileName != null))
{
    try
    {
        relic.OutputImageFilePath = $"{imageOutputPath}/{relic.ImageFileName}";
        File.Copy($"{spriteImageFilePath}/{relic.ImageFileName}", relic.OutputImageFilePath, true);
    }
    catch
    {
        //failed to find the file at the path or failed to copy it
        relic.ImageFileName = null;
        relic.OutputImageFilePath = null;
    }
}

//Identify relics that we couldn't link together to find an image file for, some of these are not real relics, old code, etc
Console.WriteLine("Malformed relics left out:");
Console.WriteLine();

foreach (var relic in relics.Where(r => r.ImageFileName == null))
{
    Console.WriteLine($"LocKey: {relic.LocKey}");
    Console.WriteLine($"Name: {relic.Name}");
    Console.WriteLine($"Desc: {relic.Desc}");
    Console.WriteLine($"Desc2: {relic.Desc2}");
    Console.WriteLine($"Desc3: {relic.Desc3}");
    Console.WriteLine($"SpriteGuid: {relic.SpriteGuid}");
    Console.WriteLine($"ImageFileName: {relic.ImageFileName}");

    Console.WriteLine();
}

//output a nice html page with sprites/descriptions
var outputSet = relics.Where(r => r.ImageFileName != null);

foreach(var relic in outputSet)
{
    relic.FixDescriptions(constants);
}

var template = Handlebars.Compile(File.ReadAllText($"{Environment.CurrentDirectory}/OutputTemplate.template"));

var data = new
{
    relics = outputSet,
};

var result = template(data);

File.WriteAllText($"{outputPath}/index.html", result);

Console.WriteLine("Outputs generated successfully!");
Console.WriteLine();
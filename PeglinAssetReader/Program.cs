using System.Configuration;
using System.Diagnostics;

//Run AssetRipper to extract Peglin files from compiled code
var peglinPath = ConfigurationManager.AppSettings["PeglinFolder"];
var assetRipperPath = ConfigurationManager.AppSettings["AssetRipperApplication"];
var outputPath = Environment.CurrentDirectory + "/output";

var proc = new Process();
proc.StartInfo.FileName = assetRipperPath;
proc.StartInfo.Arguments = $"{peglinPath} -o {outputPath}";
proc.Start();
proc.WaitForExit();
var exitCode = proc.ExitCode;
proc.Close();

Console.WriteLine(exitCode);

//Parse language file

//Parse asset files

//Compile asset/image/language info together into single object

//output a nice html page with sprites/descriptions
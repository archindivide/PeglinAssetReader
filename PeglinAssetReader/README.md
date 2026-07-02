To prep manual output, install latest version of AssetRipper and point it at 
the Peglin game folder install location, example location on my local machine:

C:\Program Files (x86)\Steam\steamapps\common\Peglin

After it finishes loading all the files, extract all files to an output folder
using the Extract Unity Project option. This is required due to using some game
logic to determine some of the output constants. 

Put this path in the App.config file and run the PeglinAssetReader.
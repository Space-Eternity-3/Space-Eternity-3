@echo off

xcopy /y/v .\SourcePack.jse3 .\Assets\Game\Other\TextAssets\SourcePack.jse3.txt

xcopy /y/v .\SourcePack.jse3 .\WindowsBuild\Datapacks\Default.jse3
xcopy /y/v .\SourcePack.jse3 .\LinuxBuild\Datapacks\Default.jse3
xcopy /y/v .\SourcePack.jse3 .\MacBuild\Datapacks\Default.jse3
xcopy /y/v .\SourcePack.jse3 .\DevBuild\Datapacks\Default.jse3
xcopy /y/v .\SourcePack.jse3 .\Datapacks\Default.jse3
xcopy /y/v .\SourcePack.jse3 .\ServerReady\Datapacks\Default.jse3
xcopy /y/v .\SourcePack.jse3 .\ServerReady\technical_data\DefaultDatapack.jse3

pause
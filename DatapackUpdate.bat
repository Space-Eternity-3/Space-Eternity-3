@echo off

xcopy /y/v .\SourcePack.jse3 .\Ready\TechnicalData\defaultdata.jse3
xcopy /y/v .\SourcePack.jse3 .\Ready\Datapacks\Default.jse3

xcopy /y/v .\SourcePack.jse3 .\LinuxBuild\TechnicalData\defaultdata.jse3
xcopy /y/v .\SourcePack.jse3 .\LinuxBuild\Datapacks\Default.jse3

xcopy /y/v .\SourcePack.jse3 .\MacBuild\TechnicalData\defaultdata.jse3
xcopy /y/v .\SourcePack.jse3 .\MacBuild\Datapacks\Default.jse3

xcopy /y/v .\SourcePack.jse3 .\DevBuild\TechnicalData\defaultdata.jse3
xcopy /y/v .\SourcePack.jse3 .\DevBuild\Datapacks\Default.jse3

xcopy /y/v .\SourcePack.jse3 .\TechnicalData\defaultdata.jse3
xcopy /y/v .\SourcePack.jse3 .\Datapacks\Default.jse3

xcopy /y/v .\SourcePack.jse3 .\ServerReady\Datapack.jse3

pause
@echo off

reg.exe query "HKLM\SOFTWARE\Microsoft\MSBuild\ToolsVersions\14.0" /v MSBuildToolsPath > nul 2>&1
if ERRORLEVEL 1 goto MissingMSBuildRegistry

for /f "skip=2 tokens=2,*" %%A in ('reg.exe query "HKLM\SOFTWARE\Microsoft\MSBuild\ToolsVersions\14.0" /v MSBuildToolsPath') do SET MSBUILDDIR=%%B

IF NOT EXIST "%MSBUILDDIR%" goto MissingMSBuildToolsPath
IF NOT EXIST "%MSBUILDDIR%msbuild.exe" goto MissingMSBuildExe

::~~~ Build section ~~~~~~~~~~~~~~~~~~~~
nuget.exe restore ClooToOpenGL.sln
if ERRORLEVEL 1 goto MissingNuget

"%MSBUILDDIR%msbuild.exe" ClooToOpenGL.sln /t:Clean;Build /p:Configuration=Release /p:DebugSymbols=false /p:DebugType=None /p:AllowedReferenceRelatedFileExtensions=none
cd ..\bin\Release\

cd ..\bin\Release
goto eof

::~~~ Errors section ~~~~~~~~~~~~~~~~~~~~
:MissingMSBuildRegistry
echo Cannot obtain path to MSBuild tools from registry
goto eof
:MissingMSBuildToolsPath
echo The MSBuild tools path from the registry '%MSBUILDDIR%' does not exist
goto eof
:MissingMSBuildExe
echo The MSBuild executable could not be found at '%MSBUILDDIR%'
goto eof
:MissingNuget
echo Nuget.exe does not exist. Install it first.
goto eof

:eof
echo .

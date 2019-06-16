@echo off
set PF32=%ProgramFiles(x86)%
if not exist "%PF32%" set PF32=%ProgramFiles%
"%PF32%\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\MSBuild"
echo:
pause
REM Clean all output folders, i.e. bin and obj.

for /d /r . %%d in (bin,obj) do @if exist "%%d" rd /s/q "%%d"

pause
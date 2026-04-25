@echo off
SETLOCAL EnableDelayedExpansion

goto filesExistCheck

:filesExistCheck
if not exist "CuoreUI.Winforms.nuspec" (
    echo "CuoreUI.Winforms.nuspec" or "nuget.exe" not found!
) else if not exist "nuget.exe" (
    echo "CuoreUI.Winforms.nuspec" or "nuget.exe" not found!
) else (
    goto revisionInput
)

:revisionInput
echo needed files are present
echo(
echo If this is a revision, provide the revision number: (default: 0)
set "revisionNumber=0"
set /p "revisionNumber= > "
goto scanNuspec

:scanNuspec
set /a lineNum=0
> tempfile.txt (
    for /f "usebackq tokens=* delims=" %%x in (CuoreUI.Winforms.nuspec) do (
    set /a lineNum+=1
    if !lineNum! equ 5 (

        rem Get date directly into variable
        for /f %%d in ('powershell -NoProfile -Command "Get-Date -Format yyyyMMdd"') do set dt=%%d

        rem Assign numeric components
        set "year=!dt:~0,4!"
        set "month=!dt:~4,2!"
        set "day=!dt:~6,2!"

        if !revisionNumber! equ 0 (
    echo     ^<version^>!year!.!month!.!day!^</version^>
) else (
    echo     ^<version^>!year!.!month!.!day!.!revisionNumber!^</version^>
)

	
    ) else (
        echo %%x
    )
)
)

move tempfile.txt CuoreUI.Winforms.nuspec

start "Title" nuget.exe pack
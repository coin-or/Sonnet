@echo off
SETLOCAL EnableDelayedExpansion

pushd "%CD%"
cd /d "%~dp0"
echo Output to "%~dp0THIRD-PARTY-LICENSE.txt"
call :DoWork > THIRD-PARTY-LICENSE.txt
if not %errorlevel%==0 ( popd && exit /b 1 )
dir "%~dp0THIRD-PARTY-LICENSE.txt" | find "THIRD"
popd

ENDLOCAL
goto :eof

:DoWork
echo SONNET THIRD PARTY LICENSE FILE
echo.
echo.
echo Sonnet includes a number of subcomponents with separate copyright notices and 
echo license terms. Sonnet does not necessarily use all the subcomponents referred 
echo to below. Your use of these subcomponents is subject to the terms and
echo conditions of the following licenses.
echo. 
echo.

if not exist ".coin-or\Dependencies" (
    echo ERROR: Dependencies not found! 1>&2
    exit /b 1
)
for /f "tokens=1,2,3" %%i in (.coin-or\Dependencies) do (
    @REM echo Taking dependency folder %%i 1>&2
    set _componentName= COIN-OR %%i
    set _componentLic=..\%%i\LICENSE
    if exist !_componentLic! ( call :WriteLicense ) else ( echo WARN: Not found !_componentLic! 1>&2 )
)

set _componentName=pthread-win32 for Windows (if applicable)
set _componentLic=..\..\pthreads\docs\license.md
if exist !_componentLic! ( call :WriteLicense ) else ( echo WARN: Not found !_componentLic! 1>&2 )

goto :eof

:WriteLicense
REM Uses _componentName and _componentLic

echo *****************************************************************************
echo ********* !_componentName! license section
echo *****************************************************************************
echo.
if not exist "!_componentLic!" ( 
  echo ERROR: !_componentLic! not found! 1>&2
  exit /b 1
)
type "!_componentLic!"
echo.
echo.
echo *****************************************************************************
echo ********* END OF !_componentName! license section
echo *****************************************************************************
echo.
goto :eof 
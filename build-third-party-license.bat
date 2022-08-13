@echo off
SETLOCAL EnableDelayedExpansion

call :DoWork > THIRD-PARTY-LICENSE.txt
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

set _componentName=COIN-OR BuildTools
set _componentLic=..\BuildTools\LICENSE
call :WriteLicense

set _componentName= COIN-OR Cbc
set _componentLic=..\Cbc\LICENSE
call :WriteLicense

set _componentName=COIN-OR Cgl
set _componentLic=..\Cgl\LICENSE
call :WriteLicense

set _componentName=COIN-OR Clp
set _componentLic=..\Clp\LICENSE
call :WriteLicense

set _componentName=COIN-OR Clp
set _componentLic=..\Clp\LICENSE
call :WriteLicense

set _componentName=COIN-OR Clp
set _componentLic=..\Clp\LICENSE
call :WriteLicense

set _componentName=pthread-win32 for Windows (if applicable)
set _componentLic=..\..\pthreads\docs\license.md
if exist !_componentLic! call :WriteLicense

goto :eof

:WriteLicense
REM Uses _componentName and _componentLic

echo *****************************************************************************
echo ********* !_componentName! license section
echo *****************************************************************************
echo.
if not exist "!_componentLic!" ( 
  echo ERROR: !_componentLic! not found!
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
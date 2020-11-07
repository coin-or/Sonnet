@REM This is the Appveyor 'after_build' script for SONNET 
@REM Tasks:
@REM  - Create the release package

cd C:\projects

mkdir release_package
cd release_package

mkdir x64
copy c:\projects\sonnet\MSVisualStudio\v16\Sonnet\bin\x64\Release\Sonnet.dll x64\
copy c:\projects\sonnet\MSVisualStudio\v16\Sonnet\bin\x64\Release\Sonnet.xml x64\
copy c:\projects\sonnet\MSVisualStudio\v16\Sonnet\bin\x64\Release\SonnetWrapper.dll x64\
mkdir x86
copy c:\projects\sonnet\MSVisualStudio\v16\Sonnet\bin\x86\Release\Sonnet.dll x86\
copy c:\projects\sonnet\MSVisualStudio\v16\Sonnet\bin\x86\Release\Sonnet.xml x86\
copy c:\projects\sonnet\MSVisualStudio\v16\Sonnet\bin\x86\Release\SonnetWrapper.dll x86\
copy c:\projects\sonnet\AUTHORS.txt .
copy c:\projects\sonnet\CHANGELOG.txt .
copy c:\projects\sonnet\INSTALL.txt .
copy c:\projects\sonnet\LICENSE.txt .
copy c:\projects\sonnet\README.txt .
copy c:\projects\sonnet\examples\Example5.cs .

@REM add recursively to archive
7z a -r ..\%APPVEYOR_PROJECT_NAME%-%VERSION%.zip *.*


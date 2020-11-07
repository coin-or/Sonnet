set newdir=newbins-%random%
mkdir %newdir%
cd %newdir%
mkdir x64
copy ..\..\MSVisualStudio\v16\Sonnet\bin\x64\Release\Sonnet.dll x64\
copy ..\..\MSVisualStudio\v16\Sonnet\bin\x64\Release\Sonnet.xml x64\
copy ..\..\MSVisualStudio\v16\Sonnet\bin\x64\Release\SonnetWrapper.dll x64\
mkdir x86
copy ..\..\MSVisualStudio\v16\Sonnet\bin\x86\Release\Sonnet.dll x86\
copy ..\..\MSVisualStudio\v16\Sonnet\bin\x86\Release\Sonnet.xml x86\
copy ..\..\MSVisualStudio\v16\Sonnet\bin\x86\Release\SonnetWrapper.dll x86\
copy ..\..\AUTHORS.txt .
copy ..\..\CHANGELOG.txt .
copy ..\..\INSTALL.txt .
copy ..\..\LICENSE.txt .
copy ..\..\README.txt .
copy ..\..\examples\Example5.cs .

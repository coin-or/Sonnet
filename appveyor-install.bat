@REM This is the Appveyor 'install' script for SONNET 
@REM Tasks:
@REM  - Clone the various coin dependencies

cd C:\projects

@REM Clone all required projects and checkout masters
@REM For all dependencies, take only latest 
git clone --depth 1 https://github.com/coin-or-tools/BuildTools.git -b master BuildTools
git clone --depth 1 https://github.com/coin-or/Cbc.git -b master Cbc
git clone --depth 1 https://github.com/coin-or/Clp.git -b master Clp
git clone --depth 1 https://github.com/coin-or/Cgl.git -b master Cgl
git clone --depth 1 https://github.com/coin-or/CoinUtils.git -b master CoinUtils
git clone --depth 1 https://github.com/coin-or/Osi.git -b master Osi


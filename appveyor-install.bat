@REM This is the Appveyor 'install' script for SONNET 
@REM Tasks:
@REM  - Clone the various coin dependencies

cd C:\projects

@REM Clone all required projects and checkout masters
@REM For all dependencies, take only latest 
@REM git clone --depth 1 https://github.com/coin-or-tools/BuildTools.git -b master BuildTools
@REM git clone --depth 1 https://github.com/coin-or/Cbc.git -b master Cbc
@REM git clone --depth 1 https://github.com/coin-or/Clp.git -b master Clp
@REM git clone --depth 1 https://github.com/coin-or/Cgl.git -b master Cgl
@REM git clone --depth 1 https://github.com/coin-or/CoinUtils.git -b master CoinUtils
@REM git clone --depth 1 https://github.com/coin-or/Osi.git -b master Osi
git clone --depth 1 https://github.com/jhmgoossens/BuildTools.git -b master BuildTools
git clone --depth 1 https://github.com/jhmgoossens/Cbc.git -b dev Cbc
git clone --depth 1 https://github.com/jhmgoossens/Clp.git -b dev Clp
git clone --depth 1 https://github.com/jhmgoossens/Cgl.git -b dev Cgl
git clone --depth 1 https://github.com/jhmgoossens/CoinUtils.git -b dev CoinUtils
git clone --depth 1 https://github.com/jhmgoossens/Osi.git -b dev Osi 


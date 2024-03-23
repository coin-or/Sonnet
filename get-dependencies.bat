@rem Read .coin-or\Dependencies and clone if the directory does not exist, or fetch and checkout.
@rem The Dependencies file has lines like <Directory>  <Repository>  <Branch/Tag>
@echo off

set depfile=.coin-or\Dependencies
if not exist %depfile% echo ERROR: File %depfile% not found. && exit /b 1

for /f "eol=# tokens=1,2,3" %%i in (%depfile%) do (
  echo.
  echo **********************************
  echo INFO: Directory: ..\%%i
  echo INFO: Repo: %%j
  echo INFO: Branch/Tag: %%k
  if not exist ..\%%i (
    echo INFO: Folder ..\%%i does not exist. Now cloning.
    git clone --depth 1 %%j -b %%k ..\%%i
    if errorlevel 1 pause
  ) else (
    echo INFO: Folder ..\%%i exists. Fetch and Checkout %%k.
    git -C ..\%%i fetch --all
    git -C ..\%%i checkout %%k
    if errorlevel 1 pause
    git -C ..\%%i status | find /i "HEAD detached"
    if errorlevel 1 (
      echo INFO: Not at detached head, so try to pull changes.
      git -C ..\%%i pull
      if errorlevel 1 pause
    )
  )
  echo INFO: Finished with ..\%%i
)

::charset: UTF-8
::author: raysco  date: 2018-9-23
@echo off
title Current - Directory: %~dp0

::此处添加你想要存放c-anls的根目录
set default="E:\download\imprc\imprc\"

::直接点击 cp-anls.bat即可将c-anls快速添加到根目录下


::关键代码 请谨慎修改
@ if exist %default% (
  @ if exist "%default%package\c-anls\" (
     echo.
     rd /s /q "%default%package\c-anls\" 
	 if errorlevel 0 echo - DELETED "c-anls" under %default%
  )
 for /r . %%i in (c-anls) do @if exist %%i ( 
	  xcopy %%i "%default%package\c-anls\" /e /f /h
	)
	if errorlevel 1 echo failed
	if errorlevel 0 echo operation finished!
) else (
 echo - target address incorrect!
)
echo.
pause



REM @echo off
REM echo.
REM echo - %~dp0
REM echo.
REM ::@for /d /r "./prj-vs-rc" %%i in (.vs,Debug,x64) do @if exist %%i ( rd /s /q %%i & echo kick - %%i) 
REM ::@for /r "./prj-vs-rc" %%i in (*.db) do (del /f /s /q %%i)
REM @for /d /r . %%i in (.vs,Debug) do @if exist %%i ( rd /s /q %%i & echo kick - %%i) 
REM @for /d /r "./prj-vs-rc" %%i in (x64) do @if exist %%i ( rd /s /q %%i & echo kick - %%i) 
REM @if exist x64 rd /s /q x64

REM @for /r . %%i in (*.db) do (del /f /s /q %%i)

REM echo.
REM echo - clean up!
REM echo.
REM pause
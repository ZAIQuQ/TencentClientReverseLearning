@echo off

start "crackme.exe" "crackme.exe"
:: 稍等2秒，防止crackme.exe未启动
ping 127.0.0.1 -n 2 > nul 

injection.exe crackme.exe

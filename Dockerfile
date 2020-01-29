FROM mcr.microsoft.com/windows/servercore:1709
ADD Hjerpbakk.Profilebot.Runner/bin/Release/ /
ENTRYPOINT Hjerpbakk.Profilebot.Runner.exe
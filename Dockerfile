FROM microsoft/windowsservercore
ADD Hjerpbakk.Profilebot.Runner/bin/Release/ /
ENTRYPOINT Hjerpbakk.Profilebot.Runner.exe
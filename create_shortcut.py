import os

target = r'C:\Users\utkug\.gemini\antigravity\scratch\OpenFMHub\publish\OpenFMHub.exe'

vbs = f'''
Set oWS = WScript.CreateObject("WScript.Shell")
strDesktop = oWS.SpecialFolders("Desktop")
sLinkFile = strDesktop & "\\OpenFM Hub.lnk"
Set oLink = oWS.CreateShortcut(sLinkFile)
oLink.TargetPath = "{target}"
oLink.WorkingDirectory = "C:\\Users\\utkug\\.gemini\\antigravity\\scratch\\OpenFMHub\\publish"
oLink.Description = "OpenFM Hub - FM 2026 Season Archive and Moneyball Companion"
oLink.Save
WScript.Echo "Saved to: " & sLinkFile
'''

with open('make_lnk.vbs', 'w', encoding='latin-1') as f:
    f.write(vbs)

os.system('cscript //nologo make_lnk.vbs')
if os.path.exists('make_lnk.vbs'):
    os.remove('make_lnk.vbs')

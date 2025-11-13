# MigrationBrowser
A helper browser that can be controlled via GPO to force certain URLs to open in private browser mode.

## Usage
| Command | Behavior |
| ------------------------------------------- | ------------------------------------------- |
| MigrationBrowser.exe | Opens Edge (normal) |
| MigrationBrowser.exe <url> | Opens Edge (InPrivate if URL matches pattern) |
| MigrationBrowser.exe --register | Registers + prompts to set as default broswer |
| MigrationBrowser.exe --register --silent | Registers silently + sets as default browser (no prompts) |

# Deployment Examples

## GPO Startup Script (Silent Deploy)
Create a .bat file and deploy via Computer Configuration > Policies > Windows Settings > Scripts (Startup):
bat
```
@echo off
"%~dp0MigrationBrowser.exe" --register --silent
```

## Add URL Patterns 
powershell
```
$key = "HKCU:\Software\MigrationBrowser\UrlPatterns"
New-Item -Path $key -Force | Out-Null
Set-ItemProperty -Path $key -Name "1" -Value "^https://intranet\.company\.com/.*"
```
GPO .pol
```
[Policies]
; GPO: MigrationBrowser - URL Patterns
; Scope: User Configuration
; Applies to: HKCU\Software\MigrationBrowser\UrlPatterns

[Registry]
; Create the key
HKEY_CURRENT_USER\Software\MigrationBrowser\UrlPatterns\**DeleteValues** = ""

; Add URL Pattern 1
HKEY_CURRENT_USER\Software\MigrationBrowser\UrlPatterns\1 = REG_SZ "^https://intranet\.company\.com/.*"

; Add URL Pattern 2
HKEY_CURRENT_USER\Software\MigrationBrowser\UrlPatterns\2 = REG_SZ "^https://legacy\.app\.internal/.*"
```


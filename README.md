# MigrationBrowser
A helper browser that can be controlled via GPO to force certain URLs to open in private browser mode.

During a migration phase where users are transitioning from legacy internal web applications to new environments, especially when changing identities, it is often necessary that certain URLs are opened in a private browsing mode to avoid issues with cached data, cookies, or session information.   While it is possible to setup desktop icons to open a web browser in private browsing mode, it requires the end-user to cut-and-paste URLs.
MigrationBrowser is a lightweight executable that can be set as the default web browser. It checks URLs against a list of patterns defined in the registry and opens matching URLs in InPrivate mode, while all other URLs open in normal mode.

It is intended to be installed via SCCM/MECM/GPO and configured via GPO registry settings.

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
## Update default browser
* DISM import: Dism /Online /Import-DefaultAppAssociations:C:\DefaultAssoc.xml
* For enterprise deployment use Group Policy / MDM:
* GPO: Computer Configuration > Administrative Templates > Windows Components > File Explorer > Set a default associations configuration file (point to the XML path).

* DefautAssoc.xml
```
<?xml version="1.0" encoding="utf-8"?>
<DefaultAssociations>
  <!-- Map protocols to your ProgId. Ensure the ProgId ("MigrationBrowser") is properly registered on target machines. -->
  <Association Identifier="http" ProgId="MigrationBrowser" ApplicationName="MigrationBrowser" />
  <Association Identifier="https" ProgId="MigrationBrowser" ApplicationName="MigrationBrowser" />
</DefaultAssociations>
```

# MigrationBrowser User Guide

## Overview

**MigrationBrowser** is a specialized browser launcher designed for enterprise environments undergoing migration. Its primary function is to intercept web links and intelligently decide whether to open them in a standard browser session or an **InPrivate/Incognito** session based on administrator-defined rules.

This is particularly useful when users are transitioning between legacy and new environments where identity conflicts, cached data, or session persistence might cause issues.

## How It Works

When MigrationBrowser is set as the default browser:
1. It intercepts the URL click.
2. It checks the URL against a list of **Regular Expression (Regex)** patterns defined in the Windows Registry.
3. **Match Found**: The URL opens in **Microsoft Edge (InPrivate mode)**.
4. **No Match**: The URL opens in **Microsoft Edge (Normal mode)**.

## Installation & Registration

MigrationBrowser is a standalone executable (`MigrationBrowser.exe`) that does not require a traditional installer. However, it must be registered with Windows to handle `http` and `https` links.

### Manual Registration (Interactive)
To register the application on a single machine:
1. Open a Command Prompt or PowerShell window.
2. Run the executable with the `--register` flag:
   ```cmd
   MigrationBrowser.exe --register
   ```
3. You will be prompted to open Windows Settings to set MigrationBrowser as the default browser.

### Silent Registration (Scripted/Enterprise)
For automated deployments (e.g., via SCCM, MECM, or GPO Startup Scripts), use the `--silent` flag to suppress user prompts:
```cmd
MigrationBrowser.exe --register --silent
```
*Note: This registers the application capabilities but cannot force it as the default browser on Windows 10/11 due to OS security restrictions. You must use a Default Association XML file for that (see Enterprise Deployment).*

## Configuration

Configuration is managed entirely via the Windows Registry in the **Current User** hive.

### Defining URL Patterns
Create registry values under the following key:
*   **Key**: `HKEY_CURRENT_USER\Software\MigrationBrowser\UrlPatterns`
*   **Value Name**: Any unique name (e.g., "1", "LegacyApp", "Intranet")
*   **Value Type**: `REG_SZ` (String)
*   **Value Data**: The Regular Expression pattern to match.

### Examples

| Scenario | Regex Pattern | Description |
| :--- | :--- | :--- |
| **Specific Domain** | `^https://legacy-app\.corp\.com/.*` | Matches any URL starting with this domain. |
| **Keyword in URL** | `.*login-v1.*` | Matches any URL containing "login-v1". |
| **Multiple Subdomains** | `^https://.*\.old-system\.net/` | Matches any subdomain of old-system.net. |

**PowerShell Example:**
```powershell
$key = "HKCU:\Software\MigrationBrowser\UrlPatterns"
New-Item -Path $key -Force | Out-Null
Set-ItemProperty -Path $key -Name "LegacyApp" -Value "^https://legacy\.app\.internal/.*"
```

## Enterprise Deployment

### 1. Deploy the Executable
Copy `MigrationBrowser.exe` to a stable location on the client machine (e.g., `C:\Program Files\MigrationBrowser\`).

### 2. Register the Application
Run the registration command via a startup script or deployment task:
```cmd
"C:\Program Files\MigrationBrowser\MigrationBrowser.exe" --register --silent
```

### 3. Configure Patterns (GPO)
Use Group Policy Preferences to push the registry keys to users.
*   **GPO Path**: User Configuration > Preferences > Windows Settings > Registry
*   **Action**: Update
*   **Hive**: HKEY_CURRENT_USER
*   **Key Path**: `Software\MigrationBrowser\UrlPatterns`
*   **Value Name**: `1` (or descriptive name)
*   **Value Data**: `^https://intranet\.company\.com/.*`

### 4. Set as Default Browser
Windows 10/11 prevents scripts from changing the default browser directly. You must use a **Default Associations Configuration File** via GPO.

1.  **Create `DefaultAssoc.xml`**:
    ```xml
    <?xml version="1.0" encoding="utf-8"?>
    <DefaultAssociations>
      <Association Identifier="http" ProgId="MigrationBrowser" ApplicationName="MigrationBrowser" />
      <Association Identifier="https" ProgId="MigrationBrowser" ApplicationName="MigrationBrowser" />
    </DefaultAssociations>
    ```
2.  **Apply GPO**:
    *   **Path**: Computer Configuration > Administrative Templates > Windows Components > File Explorer > Set a default associations configuration file.
    *   **Setting**: Enabled
    *   **File**: Path to your XML file (e.g., `\\sysvol\...\DefaultAssoc.xml` or a local path).

## Troubleshooting

### Browser Doesn't Open
*   **Check Edge Installation**: MigrationBrowser looks for Microsoft Edge at `HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\msedge.exe`. Ensure Edge is installed.
*   **Check Logs**: Run `MigrationBrowser.exe` manually from a terminal to see if any error messages appear.

### URLs Not Opening in Private Mode
*   **Verify Regex**: The pattern might be incorrect. Remember to escape special characters (e.g., use `\.` for a dot).
*   **Registry Location**: Ensure patterns are in `HKCU`, not `HKLM`.

### "Application not found" Error
*   Re-run the registration command: `MigrationBrowser.exe --register`.
*   Ensure the executable has not been moved after registration.
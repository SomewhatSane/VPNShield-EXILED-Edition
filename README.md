# VPNShield EXILED Edition
VPNShield EXILED Edition - A VPN Blocking Plugin for EXILED SCPSL servers. Originally a port of VPNShield by KarlOfDuty.

<h1>Installation</h1>
<p>Installation is simple. From the release zip, extract VPNShield.dll into your plugins folder and Newtonsoft.Json.dll into the dependencies folder inside of the plugins folder.

<h1>Configuration</h1>
<p>After starting up the plugin for the first time, a folder named VPNShield will be created in your plugins folder. This contains the main whitelists and blacklists that VPNShield uses for caching known good and bad IPs / accounts. The folder will contain 4 files:

- `VPNShield-WhitelistIPs.txt` - Contains IPs that have passed an IP check.
- `VPNShield-BlacklistIPs.txt` - Contains IPs that have failed an IP check.
- `VPNShield-WhitelistAccountAgeCheck.txt` - Contains Steam IDs that have passed an account age check.
- `VPNShield-WhitelistUserIDs.txt` - Contains SteamIDs that are allowed to bypass all checks.

If you would like to whitelist a user from account and VPN checking, you can place a user ID in the form `STEAMID64@steam`, `DISCORDID@discord`, `staffmember@nothwood` or any other user ID that is supported by the Remote Admin configuration. If you make any changes to any files, you can reload the VPNShield cache by running `vs_reload` in Remote Admin or `/vs_reload` in console (restarting your server also works).

<h1>Support</h1>
If you have any problems, you can contact me on Discord at SomewhatSane#0979.

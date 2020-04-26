# VPNShield EXILED Edition
VPNShield EXILED Edition - A VPN Blocking Plugin for EXILED SCPSL servers.<br><br>

vs_accountcheck - Boolean - Enable or Disable Steam account age checking - Default: False<br>
vs_vpncheck - Boolean - Enable or Disable VPN checking - Default: True<br>
<br>
vs_vpnapikey - String - IPHub.info API key for VPN checking - Default: null<br>
vs_vpnkickmessage - String - Kick message for VPN users - Default: "VPNs and Proxies are forbidden on this server."<br>
<br>
vs_steamapikey - String - Steam API key for account age checking - Default: null<br>
vs_accountminage - Integer - Minimum age of a Steam account that is allowed on the server (in days) - Default: 14<br>
vs_accountkickmessage - String - Kick message for Steam account too young - Default: "Your account must be atleast X day(s) old to play on this server."<br>
vs_verbose - Boolean - Verbose mode - Default: false<br>
<br>
vs_checkforupdates - Boolean - Enable checking for updates on server start - Default: true<br>
<br>
<br>
Commands
<br>
vs_reload - Reloads VPNShield configuration and whitelist (This is also done at WaitingForPlayers automatically)
<br>
<br>
User IDs can be added to 'VPNShield-WhitelistUserIDs.txt' (eg. '76561198123456789@steam' - 1 each line) to bypass VPN and account checks for those players.

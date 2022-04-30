# VPNShield EXILED Edition
VPNShield EXILED Edition - A VPN blocking plugin for EXILED SCPSL servers.<br><br>
Inspired by VPNShield by KarlOfDuty.

<h2>Installation</h2>

<p>Copy / Move VPNShield.dll to /EXILED/Plugins/ and LiteDB.dll to /EXILED/Plugins/dependencies/. Restart your server and configure in XXXX-config.yml.

<h2>Configuration</h2>

| Config setting | Type | Description | Default value |
| ------------- | ------------- | ------------- | ------------- |
| is_enabled | Boolean | Should VPNShield be enabled? | true |
| account_age_check | Boolean | Should account age checking be enabled? | false |
| account_playtime_check | Boolean | Should account playtime checking be enabled? | false |
| account_kick_private | Boolean | Should accounts that cannot be checked (eg. private Steam accounts) be kicked? | true |
| account_kick_on_steam_error | Boolean | Should accounts that cannot be checked due to a Steam API error be kicked? In most cases, you should keep this set to false. | false |
| steam_api_key | String | Steam API key for account age checking. | null |
| steam_min_age | Integer | Minimum Steam account age (if account age checking is enabled - in days). | 14 |
| steam_min_playtime | Integer | Minimum required SCPSL playtime required (if account playtime checking is enabled - in minutes). | 0 |
| account_age_check_kick_message | String |  Message shown to players who are kicked by an account age check. You may use %MINIMUMAGE% to insert the minimum age in days set into your kick message. | "Your account must be at least %MINIMUMAGE% day(s) old to play on this server." |
| account_playtime_check_kick_message | String | Message shown to players who are kicked by an account playtime check. You may use %MINIMUMPLAYTIME% to insert the minimum playtime in minutes set into your kick message. | "Your account must have played SCP: SL for atleast %MINIMUMPLAYTIME% minute(s) to play on this server." |
| account_private_kick_message | String | Message shown to players who are kicked because they account cannot be checked due to privacy settings. | "An account check could not be performed as your Steam profile is set to private. Please make your profile public and try connecting again!" |
| account_steam_error_kick_message | String | Message shown to players who are kicked as there was a Steam API error (only needed if account_kick_on_steam_error). | "An error occurred when trying to check your Steam account. Due to the policy set on this server, you were kicked. Please contact the server administration about this and try joining again later." |
| vpn_check | Boolean | Should VPN checking be enabled? | true |
| vpn_check_service | Integer | What VPN service would you like to use for VPN checking? (0 for IPHub, 1 for GetIPIntel) | 0 |
| iphub_api_key | String | IF vpn_check_service IS 0: IPHub API key for VPN checking. Get one for free at https://iphub.info . | null |
| iphub_strict_blocking | Boolean | IF vpn_check_service IS 0: IPHub supports 'strict blocking'. Should it be enabled? Strict blocking will catch more VPN / hosting IP addresses but may cause false positives. It is generally recommended to keep this disabled. | false |
| getipintel_subdomain | String | IF vpn_check_service IS 1: GetIPIntel.net API subdomain. If you have a specific plan, you can specify your subdomain. If you are using the free plan or don't know what this is, leave it as it is. | "check" |
| getipintel_contact_email_address | String | IF vpn_check_service IS 1: GetIPIntel.net Contact Email Address. The API requires a VALID contact email. | null |
| getipintel_optional_flags | String | IF vpn_check_service IS 1: GetIPIntel.net optional flags. Flags m, b, f and n are supported. If you don't know what this is, leave this blank / null or check the GetIPIntel website. | null |
| getipintel_max_score | Float | IF vpn_check_service IS 1: GetIPIntel.net max score. Any IP Address with a score above this value will be blocked. 0.995 is recommended by the API. | 0.995 |
| getipintel_block_mobile_isps | Boolean | IF vpn_check_service IS 1: Block mobile ISPs? (Players playing on mobile data providers such as 3, EE, Vodafone, T-Mobile, Verizon etc...) | false |
| vpn_check_kick_message | String | Message shown to players who are kicked by a VPN check or a mobile ISP check (if enabled). | "VPNs and proxies are forbidden on this server." |
| send_to_discord_webhook | Boolean | Send a message to Discord via webhooks when someone is kicked by VPNShield? | false |
| send_to_discord_webhook_url | String | Discord Webhook URL for send_to_discord_webhook (only needed if kick_to_discord is true). | null |
| check_for_updates | Boolean | Check for VPNShield updates on startup? | true |
| verbose_mode | Boolean | Verbose mode. Prints more console messages. | false |

<h2>Commands</h2>

| Command name | Permission node | Description | Usage |
| ------------- | ------------- | ------------- | ------------- |
| vs_blacklistip | VPNShield.blacklistip | Blacklist an IP address. | vs_blacklistip (add/remove) (ip) |
| vs_blacklistipsubnet | VPNShield.blacklistip.subnet | Blacklist an IP address subnet. Expects CIDR notation. | vs_blacklistipsubnet (add/remove) (ip subnet in CIDR notation)|
| vs_clearips | VPNShield.clear.ips | Clear all IP addresses from VPNShield's database. | vs_clearips |
| vs_clearsubnets | VPNShield.clear.subnets | Clear all IP subnets from VPNShield's database. | vs_clearsubnets |
| vs_clearuserids | VPNShield.clear.userids | Clear all user IDs from VPNShield's database. | vs_clearuserids |
| vs_getstatus | VPNShield.get.status | Get information that VPNShield has on an IP address or User ID. | vs_getstatus (ip/userid) |
| vs_getwhitelistedsubnets | VPNShield.get.whitelistedsubnets | Get a list of subnets that have been whitelisted. | vs_getwhitelistedsubnets |
| vs_whitelist | VPNShield.whitelist | Exempt players from VPNShield checks. | vs_whitelist (add/remove) (id) |
| vs_whitelistip | VPNShield.whitelist.ip | Whitelist an IP address. | vs_whitelistip (add/remove) (ip) |
| vs_whitelistipsubnet | VPNShield.whitelist.ipsubnet | Whitelist an IP address subnet. Expects CIDR notation. |  vs_whitelistipsubnet (add/remove) (ip subnet in CIDR notation) |

<h2>Data</h2>

All of VPNShield's data is stored in VPNShield/data.db. data.db can be opened by any LiteDB viewer such as LiteDB Studio.

<h2>Support</h2>

If you have any problems, you can contact me on Discord (SomewhatSane#0979) or via email (thisis AT somewhatsane DOT co DOT uk).

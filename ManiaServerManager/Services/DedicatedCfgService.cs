using ManiaServerManager.Models;

namespace ManiaServerManager.Services;

internal interface IDedicatedCfgService
{
    Task CreateTM2020ConfigAsync(string configDirectory, CancellationToken cancellationToken);
    Task CreateManiaPlanetConfigAsync(string configDirectory, CancellationToken cancellationToken);
    Task CreateTMFConfigAsync(string configDirectory, CancellationToken cancellationToken);
    Task CreateTMConfigAsync(string configDirectory, CancellationToken cancellationToken);
}

internal sealed class DedicatedCfgService : IDedicatedCfgService
{
    private readonly DedicatedCfg cfg;
    private readonly string dedicatedCfgFileName;

    public DedicatedCfgService(IConfiguration config)
    {
        cfg = config.Cfg;
        dedicatedCfgFileName = config.DedicatedCfgFileName;
    }

    public async Task CreateTM2020ConfigAsync(string configDirectory, CancellationToken cancellationToken)
    {
        var dedicatedCfg = $$"""
<?xml version="1.0" encoding="utf-8" ?>

<dedicated>
	<authorization_levels>
		<level>
			<name>{{cfg.AuthorizationSuperAdminName}}</name>
			<password>{{cfg.AuthorizationSuperAdminPassword}}</password>
		</level>
		<level>
			<name>{{cfg.AuthorizationAdminName}}</name>
			<password>{{cfg.AuthorizationAdminPassword}}</password>
		</level>
		<level>
			<name>{{cfg.AuthorizationUserName}}</name>
			<password>{{cfg.AuthorizationUserPassword}}</password>
		</level>
	</authorization_levels>
	
 	<masterserver_account>
		<login></login>
		<password></password>
	</masterserver_account>
	
	<server_options>
		<name></name>
		<comment>{{cfg.ServerComment}}</comment>
		<hide_server>{{(int)cfg.ServerHideServer}}</hide_server>					<!-- value is 0 (always shown), 1 (always hidden), 2 (hidden from nations) -->

		<max_players>{{cfg.ServerMaxPlayers}}</max_players>
		<password></password>
		
		<max_spectators>{{cfg.ServerMaxSpectators}}</max_spectators>
		<password_spectator>{{cfg.ServerPasswordSpectator}}</password_spectator>
	
		<keep_player_slots>{{cfg.ServerKeepPlayerSlots}}</keep_player_slots>			<!-- when a player changes to spectator, hould the server keep if player slots/scores etc.. or not. --> 	
		<ladder_mode>{{(int)cfg.ServerLadderMode}}</ladder_mode>				<!-- value between 'inactive', 'forced' (or '0', '1') -->
		
		<enable_p2p_upload>{{cfg.ServerEnableP2pUpload}}</enable_p2p_upload>
		<enable_p2p_download>{{cfg.ServerEnableP2pDownload}}</enable_p2p_download>
		
		<callvote_timeout>{{cfg.ServerCallVoteTimeout}}</callvote_timeout>
		<callvote_ratio>{{cfg.ServerCallVoteRatio}}</callvote_ratio>				<!-- default ratio. value in [0..1], or -1 to forbid. -->
		<callvote_ratios>
            {{string.Join("\n            ", cfg.ServerCallVoteRatios.Select(r => $"<voteratio command=\"{r.Command}\" ratio=\"{r.Ratio}\"/>"))}}
			<!-- commands can be "Ban", "Kick", "RestartMap", "NextMap", "SetModeScriptSettingsAndCommands"... -->
		</callvote_ratios>

		<allow_map_download>{{cfg.ServerAllowMapDownload}}</allow_map_download>
		<autosave_replays>{{cfg.ServerAutosaveReplays}}</autosave_replays>
		<autosave_validation_replays>{{cfg.ServerAutosaveValidationReplays}}</autosave_validation_replays>

		<referee_password>{{cfg.ServerRefereePassword}}</referee_password>
		<referee_validation_mode>{{(int)cfg.ServerRefereeValidationMode}}</referee_validation_mode>		<!-- value is 0 (only validate top3 players),  1 (validate all players) -->

		<use_changing_validation_seed>{{cfg.ServerUseChangingValidationSeed}}</use_changing_validation_seed>

		<disable_horns>{{cfg.ServerDisableHorns}}</disable_horns>
		<disable_profile_skins>{{cfg.ServerDisableProfileSkins}}</disable_profile_skins>        <!-- ignore player chosen skins and only use default country skin -->
		<clientinputs_maxlatency>{{cfg.ServerClientInputsMaxLatency}}</clientinputs_maxlatency>      <!-- players with c2s-latency greater than this value will experience difficulties playing, but a lower value reduce overall clients CPU usage (max 540ms) -->
	</server_options>
	
	<system_config>
		<connection_uploadrate>{{cfg.ConfigConnectionUploadRate}}</connection_uploadrate>						<!-- Kbits per second -->
		<connection_downloadrate>{{cfg.ConfigConnectionDownloadRate}}</connection_downloadrate>					<!-- Kbits per second -->

		<workerthreadcount>{{cfg.ConfigWorkerThreadCount}}</workerthreadcount>
		<packetassembly_multithread>{{cfg.ConfigPacketAssemblyMultithread}}</packetassembly_multithread>
		<packetassembly_packetsperframe>{{cfg.ConfigPacketAssemblyPacketsPerFrame}}</packetassembly_packetsperframe>                 <!-- Number of reduced SIMU-packets sent each frame (=10ms) -->
		<packetassembly_fullpacketsperframe>{{cfg.ConfigPacketAssemblyFullPacketsPerFrame}}</packetassembly_fullpacketsperframe>         <!-- Number of full SIMU-packets sent each frame (=10ms) -->
		<delayedvisuals_s2c_sendingrate>{{cfg.ConfigDelayedVisualsS2cSendingRate}}</delayedvisuals_s2c_sendingrate>                 <!-- proportion of frames when the server sends DELAYEDVISUAL-packets to everyone. 255 means every frame, 128 means every other frame, 64 means every fourth frame... -->

		<trustclientsimu_c2s_sendingrate>{{cfg.ConfigTrustClientSimuC2sSendingRate}}</trustclientsimu_c2s_sendingrate>               <!-- proportion of frames when the clients send TRUSTCLIENTSIMU-packets to the server.  255 means every frame, 128 means every other frame, 64 means every fourth frame... -->

		<allow_spectator_relays>{{cfg.ConfigAllowSpectatorRelays}}</allow_spectator_relays>

		<p2p_cache_size>{{cfg.ConfigP2pCacheSize}}</p2p_cache_size>

		<force_ip_address>{{cfg.ConfigForceIpAddress}}</force_ip_address>
		<server_port>{{cfg.ConfigServerPort}}</server_port>
		<client_port>{{cfg.ConfigClientPort}}</client_port>
		<bind_ip_address>{{cfg.ConfigBindIpAddress}}</bind_ip_address>
		<use_nat_upnp>{{cfg.ConfigUseNatUpnp}}</use_nat_upnp>

		<gsp_name>{{cfg.ConfigGspName}}</gsp_name>						<!-- Game Server Provider name and info url -->
		<gsp_url>{{cfg.ConfigGspUrl}}</gsp_url>						<!-- If you're a server hoster, you can use this to advertise your services -->

		<xmlrpc_port>{{cfg.ConfigXmlRpcPort}}</xmlrpc_port>
		<xmlrpc_allowremote>{{cfg.ConfigXmlRpcAllowRemote}}</xmlrpc_allowremote>			<!-- If you specify an ip adress here, it'll be the only accepted adress. this will improve security. -->

		
		<blacklist_url>{{cfg.ConfigBlacklistUrl}}</blacklist_url>
		<guestlist_filename>{{cfg.ConfigGuestlistFileName}}</guestlist_filename>
		<blacklist_filename>{{cfg.ConfigBlacklistFileName}}</blacklist_filename>
		
		<minimum_client_build>{{cfg.ConfigMinimumClientBuild}}</minimum_client_build>			<!-- Only accept updated client to a specific version. ex: 2011-10-06 -->

		<disable_coherence_checks>{{cfg.ConfigDisableCoherenceChecks}}</disable_coherence_checks>	<!-- disable internal checks to detect issues/cheats, and reject race times -->

		<disable_replay_recording>{{cfg.ConfigDisableReplayRecording}}</disable_replay_recording>	<!-- disable replay recording in memory during the game to lower memory usage. -->
		<save_all_individual_runs>{{cfg.ConfigSaveAllIndividualRuns}}</save_all_individual_runs>	<!-- Save all the ghosts from the match replay to individual ghost.gbx files, in folder {servername}/Autosaves/Runs_{mapname}/  -->

		<use_proxy>{{cfg.ConfigUseProxy}}</use_proxy>
		<proxy_url>{{cfg.ConfigProxyUrl}}</proxy_url>
	</system_config>
</dedicated>
""";

        await File.WriteAllTextAsync(Path.Combine(configDirectory, dedicatedCfgFileName), dedicatedCfg, cancellationToken);
    }

    public async Task CreateManiaPlanetConfigAsync(string configDirectory, CancellationToken cancellationToken)
    {
        var dedicatedCfg = $$"""
<?xml version="1.0" encoding="utf-8" ?>

<dedicated>
    <authorization_levels>
        <level>
            <name>{{cfg.AuthorizationSuperAdminName}}</name>
            <password>{{cfg.AuthorizationSuperAdminPassword}}</password>
        </level>
        <level>
            <name>{{cfg.AuthorizationAdminName}}</name>
            <password>{{cfg.AuthorizationAdminPassword}}</password>
        </level>
        <level>
            <name>{{cfg.AuthorizationUserName}}</name>
            <password>{{cfg.AuthorizationUserPassword}}</password>
        </level>
    </authorization_levels>
    
    <masterserver_account>
        <login></login>
        <password></password>
        <validation_key>{{cfg.AccountValidationKey}}</validation_key>
    </masterserver_account>
    
    <server_options>
        <name></name>
        <comment>{{cfg.ServerComment}}</comment>
        <hide_server>{{(int)cfg.ServerHideServer}}</hide_server>					<!-- value is 0 (always shown), 1 (always hidden), 2 (hidden from nations) -->

        <max_players>{{cfg.ServerMaxPlayers}}</max_players>
        <password></password>
        
        <max_spectators>{{cfg.ServerMaxSpectators}}</max_spectators>
        <password_spectator>{{cfg.ServerPasswordSpectator}}</password_spectator>
    
        <keep_player_slots>{{cfg.ServerKeepPlayerSlots}}</keep_player_slots>			<!-- when a player changes to spectator, hould the server keep if player slots/scores etc.. or not. --> 	
        <ladder_mode>{{(int)cfg.ServerLadderMode}}</ladder_mode>				<!-- value between 'inactive', 'forced' (or '0', '1') -->
        
        <enable_p2p_upload>{{cfg.ServerEnableP2pUpload}}</enable_p2p_upload>
        <enable_p2p_download>{{cfg.ServerEnableP2pDownload}}</enable_p2p_download>
        
        <callvote_timeout>{{cfg.ServerCallVoteTimeout}}</callvote_timeout>
        <callvote_ratio>{{cfg.ServerCallVoteRatio}}</callvote_ratio>				<!-- default ratio. value in [0..1], or -1 to forbid. -->
        <callvote_ratios>
            {{string.Join("\n            ", cfg.ServerCallVoteRatios.Select(r => $"<voteratio command=\"{r.Command}\" ratio=\"{r.Ratio}\"/>"))}}
            <!-- commands can be "Ban", "Kick", "RestartMap", "NextMap", "SetModeScriptSettingsAndCommands"... -->
        </callvote_ratios>

        <allow_map_download>{{cfg.ServerAllowMapDownload}}</allow_map_download>
        <autosave_replays>{{cfg.ServerAutosaveReplays}}</autosave_replays>
        <autosave_validation_replays>{{cfg.ServerAutosaveValidationReplays}}</autosave_validation_replays>

        <referee_password>{{cfg.ServerRefereePassword}}</referee_password>
        <referee_validation_mode>{{(int)cfg.ServerRefereeValidationMode}}</referee_validation_mode>		<!-- value is 0 (only validate top3 players),  1 (validate all players) -->

        <use_changing_validation_seed>{{cfg.ServerUseChangingValidationSeed}}</use_changing_validation_seed>

        <disable_horns>{{cfg.ServerDisableHorns}}</disable_horns>
        <clientinputs_maxlatency>{{cfg.ServerClientInputsMaxLatency}}</clientinputs_maxlatency>		<!-- 0 mean automatic adjustement -->
    </server_options>
    
    <system_config>
        <connection_uploadrate>{{cfg.ConfigConnectionUploadRate}}</connection_uploadrate>		<!-- Kbits per second -->
        <connection_downloadrate>{{cfg.ConfigConnectionDownloadRate}}</connection_downloadrate>		<!-- Kbits per second -->
        <packetassembly_threadcount>{{cfg.ConfigPacketAssemblyThreadCount}}</packetassembly_threadcount>     <!-- Number of threads used when assembling packets. Defaults to 1. --> 

        <allow_spectator_relays>{{cfg.ConfigAllowSpectatorRelays}}</allow_spectator_relays>

        <p2p_cache_size>{{cfg.ConfigP2pCacheSize}}</p2p_cache_size>

        <force_ip_address>{{cfg.ConfigForceIpAddress}}</force_ip_address>
        <server_port>{{cfg.ConfigServerPort}}</server_port>
        <server_p2p_port>{{cfg.ConfigServerP2pPort}}</server_p2p_port>
        <client_port>{{cfg.ConfigClientPort}}</client_port>
        <bind_ip_address>{{cfg.ConfigBindIpAddress}}</bind_ip_address>
        <use_nat_upnp>{{cfg.ConfigUseNatUpnp}}</use_nat_upnp>

        <gsp_name>{{cfg.ConfigGspName}}</gsp_name>						<!-- Game Server Provider name and info url -->
        <gsp_url>{{cfg.ConfigGspUrl}}</gsp_url>						<!-- If you're a server hoster, you can use this to advertise your services -->

        <xmlrpc_port>{{cfg.ConfigXmlRpcPort}}</xmlrpc_port>
        <xmlrpc_allowremote>{{cfg.ConfigXmlRpcAllowRemote}}</xmlrpc_allowremote>			<!-- If you specify an ip adress here, it'll be the only accepted adress. this will improve security. -->

        <scriptcloud_source>{{cfg.ConfigScriptCloudSource}}</scriptcloud_source>		<!-- Specify the cloud storage mode for Titles that use it. Can be "localdebug" or "xmlrpc" or "nadeocloud" (default). "nadeocloud" will work only if the creator of the title subscribed to the cloud service. -->

        
        <blacklist_url>{{cfg.ConfigBlacklistUrl}}</blacklist_url>
        <guestlist_filename>{{cfg.ConfigGuestlistFileName}}</guestlist_filename>
        <blacklist_filename>{{cfg.ConfigBlacklistFileName}}</blacklist_filename>
        
        <title>SMStorm</title>		<!-- SMStorm, TMCanyon, ... -->

        <minimum_client_build>{{cfg.ConfigMinimumClientBuild}}</minimum_client_build>			<!-- Only accept updated client to a specific version. ex: 2011-10-06 -->

        <disable_coherence_checks>{{cfg.ConfigDisableCoherenceChecks}}</disable_coherence_checks>	<!-- disable internal checks to detect issues/cheats, and reject race times -->

        <disable_replay_recording>{{cfg.ConfigDisableReplayRecording}}</disable_replay_recording>	<!-- disable replay recording in memory during the game to lower memory usage. -->
        <save_all_individual_runs>{{cfg.ConfigSaveAllIndividualRuns}}</save_all_individual_runs>	<!-- Save all the ghosts from the match replay to individual ghost.gbx files, in folder {servername}/Autosaves/Runs_{mapname}/  -->

        <use_proxy>{{cfg.ConfigUseProxy}}</use_proxy>
        <proxy_url>{{cfg.ConfigProxyUrl}}</proxy_url>
    </system_config>
</dedicated>
""";

        await File.WriteAllTextAsync(Path.Combine(configDirectory, dedicatedCfgFileName), dedicatedCfg, cancellationToken);
    }

    public async Task CreateTMFConfigAsync(string configDirectory, CancellationToken cancellationToken)
    {
        var dedicatedCfg = $$"""
<?xml version="1.0" encoding="utf-8" ?>

<dedicated>
	<authorization_levels>
		<level>
			<name>{{cfg.AuthorizationSuperAdminName}}</name>
			<password>{{cfg.AuthorizationSuperAdminPassword}}</password>
		</level>
		<level>
			<name>{{cfg.AuthorizationAdminName}}</name>
			<password>{{cfg.AuthorizationAdminPassword}}</password>
		</level>
		<level>
			<name>{{cfg.AuthorizationUserName}}</name>
			<password>{{cfg.AuthorizationUserPassword}}</password>
		</level>
	</authorization_levels>

 	<masterserver_account>
		<login></login>
		<password></password>
		<validation_key>{{cfg.AccountValidationKey}}</validation_key>
	</masterserver_account>

	<server_options>
		<name></name>
		<comment>{{cfg.ServerComment}}</comment>
		<hide_server>{{(int)cfg.ServerHideServer}}</hide_server>					<!-- value is 0 (always shown), 1 (always hidden), 2 (hidden from nations) -->

		<max_players>{{cfg.ServerMaxPlayers}}</max_players>
		<password></password>

		<max_spectators>{{cfg.ServerMaxSpectators}}</max_spectators>
		<password_spectator>{{cfg.ServerPasswordSpectator}}</password_spectator>

		<ladder_mode>{{(int)cfg.ServerLadderMode}}</ladder_mode>				<!-- value between 'inactive', 'forced' (or '0', '1') -->
		<ladder_serverlimit_min>{{cfg.ServerLadderServerLimitMin}}</ladder_serverlimit_min>		<!-- Those values will be clamped to the limits authorized on http://official.trackmania.com/tmf-ladderserver/ -->
		<ladder_serverlimit_max>{{cfg.ServerLadderServerLimitMax}}</ladder_serverlimit_max>		

		<enable_p2p_upload>{{cfg.ServerEnableP2pUpload}}</enable_p2p_upload>
		<enable_p2p_download>{{cfg.ServerEnableP2pDownload}}</enable_p2p_download>

		<callvote_timeout>{{cfg.ServerCallVoteTimeout}}</callvote_timeout>
		<callvote_ratio>{{cfg.ServerCallVoteRatio}}</callvote_ratio>				<!-- default ratio. value in [0..1], or -1 to forbid. -->
		<callvote_ratios>
            {{string.Join("\n            ", cfg.ServerCallVoteRatios.Select(r => $"<voteratio command=\"{r.Command}\" ratio=\"{r.Ratio}\"/>"))}}
			<!-- commands can be "Ban", "Kick", "ChallengeRestart", "NextChallenge", ... -->
		</callvote_ratios>

		<allow_challenge_download>{{cfg.ServerAllowMapDownload}}</allow_challenge_download>
		<autosave_replays>{{cfg.ServerAutosaveReplays}}</autosave_replays>
		<autosave_validation_replays>{{cfg.ServerAutosaveValidationReplays}}</autosave_validation_replays>

		<referee_password>{{cfg.ServerRefereePassword}}</referee_password>
		<referee_validation_mode>{{cfg.ServerRefereeValidationMode}}</referee_validation_mode>		<!-- value is 0 (only validate top3 players),  1 (validate all players) -->

		<use_changing_validation_seed>{{cfg.ServerUseChangingValidationSeed}}</use_changing_validation_seed>
	</server_options>

	<system_config>
		<connection_uploadrate>{{cfg.ConfigConnectionUploadRate}}</connection_uploadrate>		<!-- Kbps (kilo bits per second) -->
		<connection_downloadrate>{{cfg.ConfigConnectionDownloadRate}}</connection_downloadrate>		<!-- Kbps -->

		<force_ip_address>{{cfg.ConfigForceIpAddress}}</force_ip_address>
		<server_port>{{cfg.ConfigServerPort}}</server_port>
		<server_p2p_port>{{cfg.ConfigServerP2pPort}}</server_p2p_port>
		<client_port>{{cfg.ConfigClientPort}}</client_port>
		<bind_ip_address>{{cfg.ConfigBindIpAddress}}</bind_ip_address>
		<use_nat_upnp>{{cfg.ConfigUseNatUpnp}}</use_nat_upnp>

		<p2p_cache_size>{{cfg.ConfigP2pCacheSize}}</p2p_cache_size>

		<xmlrpc_port>{{cfg.ConfigXmlRpcPort}}</xmlrpc_port>
		<xmlrpc_allowremote>{{cfg.ConfigXmlRpcAllowRemote}}</xmlrpc_allowremote>			<!-- If you specify an ip adress here, it'll be the only accepted adress. this will improve security. -->

		<blacklist_url>{{cfg.ConfigBlacklistUrl}}</blacklist_url>
		<guestlist_filename>{{cfg.ConfigGuestlistFileName}}</guestlist_filename>
		<blacklist_filename>{{cfg.ConfigBlacklistFileName}}</blacklist_filename>

		<packmask>{{cfg.ConfigPackMask}}</packmask>

		<allow_spectator_relays>{{cfg.ConfigAllowSpectatorRelays}}</allow_spectator_relays>

		<!-- <minimum_client_build>2009-10-01</minimum_client_build> -->

		<!-- <disable_coherence_checks>laps</disable_coherence_checks> -->

		<use_proxy>{{cfg.ConfigUseProxy}}</use_proxy>
		<proxy_login>{{cfg.ConfigProxyLogin}}</proxy_login>
		<proxy_password>{{cfg.ConfigProxyPassword}}</proxy_password>
	</system_config>
</dedicated>
""";

        await File.WriteAllTextAsync(Path.Combine(configDirectory, dedicatedCfgFileName), dedicatedCfg, cancellationToken);
    }

    public async Task CreateTMConfigAsync(string configDirectory, CancellationToken cancellationToken)
    {
        var dedicatedCfg = $$"""
<?xml version="1.0" encoding="utf-8" ?>

<dedicated>
    <authorization_levels>
        <level>
        	<name>{{cfg.AuthorizationSuperAdminName}}</name>
        	<password>{{cfg.AuthorizationSuperAdminPassword}}</password>
        </level>
        <level>
        	<name>{{cfg.AuthorizationAdminName}}</name>
        	<password>{{cfg.AuthorizationAdminPassword}}</password>
        </level>
        <level>
        	<name>{{cfg.AuthorizationUserName}}</name>
        	<password>{{cfg.AuthorizationUserPassword}}</password>
        </level>
    </authorization_levels>

    <masterserver_account>
        <login></login>
        <password></password>
        <nation>{{cfg.AccountNation}}</nation>
    </masterserver_account>

    <server_options>
        <name></name>
        <comment>{{cfg.ServerComment}}</comment>
        <max_players>{{cfg.ServerMaxPlayers}}</max_players>
        <password></password>
        <max_spectators>{{cfg.ServerMaxSpectators}}</max_spectators>
        <password_spectator>{{cfg.ServerPasswordSpectator}}</password_spectator>
        <ladder_mode>{{cfg.ServerLadderMode.ToString().ToLowerInvariant()}}</ladder_mode> // value between 'inactive', 'normal' and 'forced' (or '0', '1', '2')
        <enable_p2p_upload>{{cfg.ServerEnableP2pUpload}}</enable_p2p_upload>
        <enable_p2p_download>{{cfg.ServerEnableP2pDownload}}</enable_p2p_download>
        <callvote_timeout>{{cfg.ServerCallVoteTimeout}}</callvote_timeout>
        <callvote_ratio>{{cfg.ServerCallVoteRatio}}</callvote_ratio>
        <allow_challenge_download>{{cfg.ServerAllowMapDownload}}</allow_challenge_download>
    </server_options>

    <system_config>
        <connection_type>{{cfg.ConfigConnectionType}}</connection_type>
        <server_port>{{cfg.ConfigServerPort}}</server_port>
        <server_p2p_port>{{cfg.ConfigServerP2pPort}}</server_p2p_port>
        <client_port>{{cfg.ConfigClientPort}}</client_port>
        <xmlrpc_port>{{cfg.ConfigXmlRpcPort}}</xmlrpc_port>
        <xmlrpc_allowremote>{{cfg.ConfigXmlRpcAllowRemote}}</xmlrpc_allowremote>	// if you specify an ip adress here, it'll be the only accepted adress. this will improve security.
        <bind_ip_address>{{cfg.ConfigBindIpAddress}}</bind_ip_address>
        <force_ip_address>{{cfg.ConfigForceIpAddress}}</force_ip_address>
        <use_proxy>{{cfg.ConfigUseProxy}}</use_proxy>
        <proxy_login>{{cfg.ConfigProxyLogin}}</proxy_login>
        <proxy_password>{{cfg.ConfigProxyPassword}}</proxy_password>
        <blacklist_url>{{cfg.ConfigBlacklistUrl}}</blacklist_url>
    </system_config>
</dedicated>
""";

        await File.WriteAllTextAsync(Path.Combine(configDirectory, "dedicated.cfg"), dedicatedCfg, cancellationToken);
    }
}

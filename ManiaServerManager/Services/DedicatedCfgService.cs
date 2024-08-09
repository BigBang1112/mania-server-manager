using ManiaServerManager.Server;

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
    private readonly ServerOptions serverOptions;

    public DedicatedCfgService(IConfiguration config)
    {
        serverOptions = new ServerOptions();
        config.GetSection("Server").Bind(serverOptions);
    }

    public async Task CreateTM2020ConfigAsync(string configDirectory, CancellationToken cancellationToken)
    {
        var config = $$"""
<?xml version="1.0" encoding="utf-8" ?>

<dedicated>
	<authorization_levels>
		<level>
			<name>SuperAdmin</name>
			<password>{{serverOptions.SuperAdminPassword}}</password>
		</level>
		<level>
			<name>Admin</name>
			<password>{{serverOptions.AdminPassword}}</password>
		</level>
		<level>
			<name>User</name>
			<password>{{serverOptions.UserPassword}}</password>
		</level>
	</authorization_levels>
	
 	<masterserver_account>
		<login></login>
		<password></password>
	</masterserver_account>
	
	<server_options>
		<name></name>
		<comment>{{serverOptions.Comment}}</comment>
		<hide_server>{{(int)serverOptions.HideServer}}</hide_server>					<!-- value is 0 (always shown), 1 (always hidden), 2 (hidden from nations) -->

		<max_players>{{serverOptions.MaxPlayers}}</max_players>
		<password></password>
		
		<max_spectators>{{serverOptions.MaxSpectators}}</max_spectators>
		<password_spectator></password_spectator>
	
		<keep_player_slots>{{serverOptions.KeepPlayerSlots}}</keep_player_slots>			<!-- when a player changes to spectator, hould the server keep if player slots/scores etc.. or not. --> 	
		<ladder_mode>{{(int)serverOptions.LadderMode}}</ladder_mode>				<!-- value between 'inactive', 'forced' (or '0', '1') -->
		
		<enable_p2p_upload>True</enable_p2p_upload>
		<enable_p2p_download>False</enable_p2p_download>
		
		<callvote_timeout>60000</callvote_timeout>
		<callvote_ratio>0.5</callvote_ratio>				<!-- default ratio. value in [0..1], or -1 to forbid. -->
		<callvote_ratios>
			<voteratio command="Ban" ratio="-1"/>
			<!-- commands can be "Ban", "Kick", "RestartMap", "NextMap", "SetModeScriptSettingsAndCommands"... -->
		</callvote_ratios>

		<allow_map_download>True</allow_map_download>
		<autosave_replays>False</autosave_replays>
		<autosave_validation_replays>False</autosave_validation_replays>

		<referee_password></referee_password>
		<referee_validation_mode>0</referee_validation_mode>		<!-- value is 0 (only validate top3 players),  1 (validate all players) -->

		<use_changing_validation_seed>False</use_changing_validation_seed>

		<disable_horns>False</disable_horns>
		<disable_profile_skins>False</disable_profile_skins>        <!-- ignore player chosen skins and only use default country skin -->
		<clientinputs_maxlatency>200</clientinputs_maxlatency>      <!-- players with c2s-latency greater than this value will experience difficulties playing, but a lower value reduce overall clients CPU usage (max 540ms) -->
	</server_options>
	
	<system_config>
		<connection_uploadrate>102400</connection_uploadrate>						<!-- Kbits per second -->
		<connection_downloadrate>102400</connection_downloadrate>					<!-- Kbits per second -->

		<workerthreadcount>2</workerthreadcount>
		<packetassembly_multithread>True</packetassembly_multithread>
		<packetassembly_packetsperframe>60</packetassembly_packetsperframe>                 <!-- Number of reduced SIMU-packets sent each frame (=10ms) -->
		<packetassembly_fullpacketsperframe>30</packetassembly_fullpacketsperframe>         <!-- Number of full SIMU-packets sent each frame (=10ms) -->
		<delayedvisuals_s2c_sendingrate>32</delayedvisuals_s2c_sendingrate>                 <!-- proportion of frames when the server sends DELAYEDVISUAL-packets to everyone. 255 means every frame, 128 means every other frame, 64 means every fourth frame... -->

		<trustclientsimu_c2s_sendingrate>64</trustclientsimu_c2s_sendingrate>               <!-- proportion of frames when the clients send TRUSTCLIENTSIMU-packets to the server.  255 means every frame, 128 means every other frame, 64 means every fourth frame... -->

		<allow_spectator_relays>False</allow_spectator_relays>

		<p2p_cache_size>600</p2p_cache_size>

		<force_ip_address></force_ip_address>
		<server_port>{{serverOptions.ServerPort}}</server_port>
		<client_port>0</client_port>
		<bind_ip_address></bind_ip_address>
		<use_nat_upnp></use_nat_upnp>

		<gsp_name></gsp_name>						<!-- Game Server Provider name and info url -->
		<gsp_url></gsp_url>						<!-- If you're a server hoster, you can use this to advertise your services -->

		<xmlrpc_port>{{serverOptions.XmlRpcPort}}</xmlrpc_port>
		<xmlrpc_allowremote>False</xmlrpc_allowremote>			<!-- If you specify an ip adress here, it'll be the only accepted adress. this will improve security. -->

		
		<blacklist_url></blacklist_url>
		<guestlist_filename></guestlist_filename>
		<blacklist_filename></blacklist_filename>
		
		<minimum_client_build></minimum_client_build>			<!-- Only accept updated client to a specific version. ex: 2011-10-06 -->

		<disable_coherence_checks>False</disable_coherence_checks>	<!-- disable internal checks to detect issues/cheats, and reject race times -->

		<disable_replay_recording>False</disable_replay_recording>	<!-- disable replay recording in memory during the game to lower memory usage. -->
		<save_all_individual_runs>False</save_all_individual_runs>	<!-- Save all the ghosts from the match replay to individual ghost.gbx files, in folder {servername}/Autosaves/Runs_{mapname}/  -->

		<use_proxy>False</use_proxy>
		<proxy_url></proxy_url>
	</system_config>
</dedicated>
""";

        await File.WriteAllTextAsync(Path.Combine(configDirectory, serverOptions.DedicatedCfg + ".txt"), config, cancellationToken);
    }

    public async Task CreateManiaPlanetConfigAsync(string configDirectory, CancellationToken cancellationToken)
    {
        var config = $$"""
<?xml version="1.0" encoding="utf-8" ?>

<dedicated>
    <authorization_levels>
        <level>
            <name>SuperAdmin</name>
            <password>{{serverOptions.SuperAdminPassword}}</password>
        </level>
        <level>
            <name>Admin</name>
            <password>{{serverOptions.AdminPassword}}</password>
        </level>
        <level>
            <name>User</name>
            <password>{{serverOptions.UserPassword}}</password>
        </level>
    </authorization_levels>
    
    <masterserver_account>
        <login></login>
        <password></password>
        <validation_key>{{serverOptions.ValidationKey}}</validation_key>
    </masterserver_account>
    
    <server_options>
        <name></name>
        <comment>{{serverOptions.Comment}}</comment>
        <hide_server>{{(int)serverOptions.HideServer}}</hide_server>					<!-- value is 0 (always shown), 1 (always hidden), 2 (hidden from nations) -->

        <max_players>{{serverOptions.MaxPlayers}}</max_players>
        <password></password>
        
        <max_spectators>{{serverOptions.MaxSpectators}}</max_spectators>
        <password_spectator>{{serverOptions.SpectatorPassword}}</password_spectator>
    
        <keep_player_slots>{{serverOptions.KeepPlayerSlots}}</keep_player_slots>			<!-- when a player changes to spectator, hould the server keep if player slots/scores etc.. or not. --> 	
        <ladder_mode>{{(int)serverOptions.LadderMode}}</ladder_mode>				<!-- value between 'inactive', 'forced' (or '0', '1') -->
        
        <enable_p2p_upload>True</enable_p2p_upload>
        <enable_p2p_download>False</enable_p2p_download>
        
        <callvote_timeout>60000</callvote_timeout>
        <callvote_ratio>0.5</callvote_ratio>				<!-- default ratio. value in [0..1], or -1 to forbid. -->
        <callvote_ratios>
            <voteratio command="Ban" ratio="-1"/>
            <!-- commands can be "Ban", "Kick", "RestartMap", "NextMap", "SetModeScriptSettingsAndCommands"... -->
        </callvote_ratios>

        <allow_map_download>True</allow_map_download>
        <autosave_replays>False</autosave_replays>
        <autosave_validation_replays>False</autosave_validation_replays>

        <referee_password></referee_password>
        <referee_validation_mode>0</referee_validation_mode>		<!-- value is 0 (only validate top3 players),  1 (validate all players) -->

        <use_changing_validation_seed>False</use_changing_validation_seed>

        <disable_horns>False</disable_horns>
        <clientinputs_maxlatency>0</clientinputs_maxlatency>		<!-- 0 mean automatic adjustement -->
    </server_options>
    
    <system_config>
        <connection_uploadrate>8000</connection_uploadrate>		<!-- Kbits per second -->
        <connection_downloadrate>8000</connection_downloadrate>		<!-- Kbits per second -->
        <packetassembly_threadcount>1</packetassembly_threadcount>     <!-- Number of threads used when assembling packets. Defaults to 1. --> 

        <allow_spectator_relays>False</allow_spectator_relays>

        <p2p_cache_size>600</p2p_cache_size>

        <force_ip_address></force_ip_address>
        <server_port>{{serverOptions.ServerPort}}</server_port>
        <server_p2p_port>{{serverOptions.ServerP2pPort}}</server_p2p_port>
        <client_port>0</client_port>
        <bind_ip_address></bind_ip_address>
        <use_nat_upnp></use_nat_upnp>

        <gsp_name></gsp_name>						<!-- Game Server Provider name and info url -->
        <gsp_url></gsp_url>						<!-- If you're a server hoster, you can use this to advertise your services -->

        <xmlrpc_port>{{serverOptions.XmlRpcPort}}</xmlrpc_port>
        <xmlrpc_allowremote>False</xmlrpc_allowremote>			<!-- If you specify an ip adress here, it'll be the only accepted adress. this will improve security. -->

        <scriptcloud_source>nadeocloud</scriptcloud_source>		<!-- Specify the cloud storage mode for Titles that use it. Can be "localdebug" or "xmlrpc" or "nadeocloud" (default). "nadeocloud" will work only if the creator of the title subscribed to the cloud service. -->

        
        <blacklist_url></blacklist_url>
        <guestlist_filename></guestlist_filename>
        <blacklist_filename></blacklist_filename>
        
        <title>SMStorm</title>		<!-- SMStorm, TMCanyon, ... -->

        <minimum_client_build></minimum_client_build>			<!-- Only accept updated client to a specific version. ex: 2011-10-06 -->

        <disable_coherence_checks>False</disable_coherence_checks>	<!-- disable internal checks to detect issues/cheats, and reject race times -->

        <disable_replay_recording>False</disable_replay_recording>	<!-- disable replay recording in memory during the game to lower memory usage. -->
        <save_all_individual_runs>False</save_all_individual_runs>	<!-- Save all the ghosts from the match replay to individual ghost.gbx files, in folder {servername}/Autosaves/Runs_{mapname}/  -->

        <use_proxy>False</use_proxy>
        <proxy_url></proxy_url>
    </system_config>
</dedicated>
""";

        await File.WriteAllTextAsync(Path.Combine(configDirectory, serverOptions.DedicatedCfg + ".txt"), config, cancellationToken);
    }

    public async Task CreateTMFConfigAsync(string configDirectory, CancellationToken cancellationToken)
    {
        var config = $$"""
<?xml version="1.0" encoding="utf-8" ?>

<dedicated>
	<authorization_levels>
		<level>
			<name>SuperAdmin</name>
			<password>{{serverOptions.SuperAdminPassword}}</password>
		</level>
		<level>
			<name>Admin</name>
			<password>{{serverOptions.AdminPassword}}</password>
		</level>
		<level>
			<name>User</name>
			<password>{{serverOptions.UserPassword}}</password>
		</level>
	</authorization_levels>

 	<masterserver_account>
		<login></login>
		<password></password>
		<validation_key>{{serverOptions.ValidationKey}}</validation_key>
	</masterserver_account>

	<server_options>
		<name></name>
		<comment>{{serverOptions.Comment}}</comment>
		<hide_server>{{(int)serverOptions.HideServer}}</hide_server>					<!-- value is 0 (always shown), 1 (always hidden), 2 (hidden from nations) -->

		<max_players>{{serverOptions.MaxPlayers}}</max_players>
		<password></password>

		<max_spectators>{{serverOptions.MaxSpectators}}</max_spectators>
		<password_spectator>{{serverOptions.SpectatorPassword}}</password_spectator>

		<ladder_mode>{{(int)serverOptions.LadderMode}}</ladder_mode>				<!-- value between 'inactive', 'forced' (or '0', '1') -->
		<ladder_serverlimit_min>0</ladder_serverlimit_min>		<!-- Those values will be clamped to the limits authorized on http://official.trackmania.com/tmf-ladderserver/ -->
		<ladder_serverlimit_max>50000</ladder_serverlimit_max>		

		<enable_p2p_upload>True</enable_p2p_upload>
		<enable_p2p_download>True</enable_p2p_download>

		<callvote_timeout>60000</callvote_timeout>
		<callvote_ratio>0.5</callvote_ratio>				<!-- default ratio. value in [0..1], or -1 to forbid. -->
		<callvote_ratios>
			<voteratio command="Ban" ratio="0.65"/>
			<!-- commands can be "Ban", "Kick", "ChallengeRestart", "NextChallenge", ... -->
		</callvote_ratios>

		<allow_challenge_download>True</allow_challenge_download>
		<autosave_replays>False</autosave_replays>
		<autosave_validation_replays>False</autosave_validation_replays>

		<referee_password></referee_password>
		<referee_validation_mode>0</referee_validation_mode>		<!-- value is 0 (only validate top3 players),  1 (validate all players) -->

		<use_changing_validation_seed>False</use_changing_validation_seed>
	</server_options>

	<system_config>
		<connection_uploadrate>512</connection_uploadrate>		<!-- Kbps (kilo bits per second) -->
		<connection_downloadrate>8192</connection_downloadrate>		<!-- Kbps -->

		<force_ip_address></force_ip_address>
		<server_port>{{serverOptions.ServerPort}}</server_port>
		<server_p2p_port>{{serverOptions.ServerP2pPort}}</server_p2p_port>
		<client_port>0</client_port>
		<bind_ip_address></bind_ip_address>
		<use_nat_upnp></use_nat_upnp>

		<p2p_cache_size>600</p2p_cache_size>

		<xmlrpc_port>{{serverOptions.XmlRpcPort}}</xmlrpc_port>
		<xmlrpc_allowremote>False</xmlrpc_allowremote>			<!-- If you specify an ip adress here, it'll be the only accepted adress. this will improve security. -->

		<blacklist_url></blacklist_url>
		<guestlist_filename></guestlist_filename>
		<blacklist_filename></blacklist_filename>

		<packmask>stadium</packmask>

		<allow_spectator_relays>False</allow_spectator_relays>

		<!-- <minimum_client_build>2009-10-01</minimum_client_build> -->

		<!-- <disable_coherence_checks>laps</disable_coherence_checks> -->

		<use_proxy>False</use_proxy>
		<proxy_login></proxy_login>
		<proxy_password></proxy_password>
	</system_config>
</dedicated>
""";

        await File.WriteAllTextAsync(Path.Combine(configDirectory, serverOptions.DedicatedCfg + ".txt"), config, cancellationToken);
    }

    public async Task CreateTMConfigAsync(string configDirectory, CancellationToken cancellationToken)
    {
        var config = $$"""
<?xml version="1.0" encoding="utf-8" ?>

<dedicated>
    <authorization_levels>
        <level>
        	<name>SuperAdmin</name>
        	<password>{{serverOptions.SuperAdminPassword}}</password>
        </level>
        <level>
        	<name>Admin</name>
        	<password>{{serverOptions.AdminPassword}}</password>
        </level>
        <level>
        	<name>User</name>
        	<password>{{serverOptions.UserPassword}}</password>
        </level>
    </authorization_levels>

    <masterserver_account>
        <login></login>
        <password></password>
        <nation></nation>
    </masterserver_account>

    <server_options>
        <name></name>
        <comment>{{serverOptions.Comment}}</comment>
        <max_players>{{serverOptions.MaxPlayers}}</max_players>
        <password></password>
        <max_spectators>{{serverOptions.MaxSpectators}}</max_spectators>
        <password_spectator>{{serverOptions.SpectatorPassword}}</password_spectator>
        <ladder_mode>normal</ladder_mode> // value between 'inactive', 'normal' and 'forced' (or '0', '1', '2')
        <enable_p2p_upload>True</enable_p2p_upload>
        <enable_p2p_download>True</enable_p2p_download>
        <callvote_timeout>60000</callvote_timeout>
        <callvote_ratio>0.5</callvote_ratio>
        <allow_challenge_download>True</allow_challenge_download>
    </server_options>

    <system_config>
        <connection_type>DSL_16384_4096</connection_type>
        <server_port>{{serverOptions.ServerPort}}</server_port>
        <server_p2p_port>{{serverOptions.ServerP2pPort}}</server_p2p_port>
        <client_port>0</client_port>
        <xmlrpc_port>{{serverOptions.XmlRpcPort}}</xmlrpc_port>
        <xmlrpc_allowremote>False</xmlrpc_allowremote>	// if you specify an ip adress here, it'll be the only accepted adress. this will improve security.
        <bind_ip_address></bind_ip_address>
        <force_ip_address></force_ip_address>
        <use_proxy>False</use_proxy>
        <proxy_login></proxy_login>
        <proxy_password></proxy_password>
        <blacklist_url></blacklist_url>
    </system_config>
</dedicated>
""";

        await File.WriteAllTextAsync(Path.Combine(configDirectory, "dedicated.cfg"), config, cancellationToken);
    }
}

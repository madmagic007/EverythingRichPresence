local module = {
    discordAppId = "772853901396279376",
    appName = "hl2",
    titleContains = "Garry's Mod",
    updateUrl = "https://github.com/madmagic007/EverythingRichPresence/raw/main/exampleModules/Garry's%20Mod%20by%20MadMagic.lua"
}

local addresses = {
    serverName = "client.dll+7BCD28",
    map = "engine.dll+4F5F90",
    gamemode = "filesystem_stdio.dll+C56E4"
}

local gamemodeMapping = {
    prop_hunt = "Prop Hunt",
    terrortown = "TTT (Trouble in Terrorist Town)"
}

RegisterModule(module, function()
    local serverName = Mem.readString(addresses.serverName)
    local gamemode =  Mem.readString(addresses.gamemode)
    local map = Mem.readString(addresses.map)


    print(serverName)

    local presence = {
        largeImageKey = "gmod",
        largeImageText = "Garry's Mod RPC By MadMagic",
        details = "In Main Menu"
    }

    if map ~= "" then
        local niceGamemode = gamemodeMapping[gamemode]
        if niceGamemode == nil then
            niceGamemode = gamemode
        end

        presence.details = "Playing on " .. serverName
        presence.state = "Playing " .. niceGamemode .. " on " .. map
        presence.smallImageKey = gamemode
        presence.smallImageText = niceGamemode
    end
    
    SetPresence(presence)
end)

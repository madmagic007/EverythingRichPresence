local module = {
    discordAppId = "772853901396279376",
    appName = "gmod",
    titleContains = "Garry's Mod",
    updateUrl = "https://github.com/madmagic007/EverythingRichPresence/raw/main/exampleModules/Garry's%20Mod%20by%20MadMagic.lua"
}

local addresses = {
    serverName = "client.dll+7F0FD0",
    map = "engine.dll+4F2740",
    gamemode = "client.dll+732558"
}

local gamemodeMapping = {
    prop_hunt = "Prop Hunt",
    terrortown = "TTT (Trouble in Terrorist Town)"
}

RegisterModule(module, function()
    local serverName = Mem.readString(addresses.serverName) 
    local gamemode =  Mem.readString(addresses.gamemode)
    local map = Mem.readString(addresses.map)

    print("serverName: " .. serverName)
    print("gamemode: " .. gamemode)
    print("map: " .. map)

    --disabling assets for discord issue
    local presence = {
        -- largeImageKey = "gmod",
        -- largeImageText = "Garry's Mod RPC By MadMagic",
        details = "In Main Menu"
    }

    if map ~= "" then
        local niceGamemode = gamemodeMapping[gamemode]
        if niceGamemode == nil then
            niceGamemode = gamemode
        end

        if serverName ~= "" then
            presence.details = "Playing on " .. serverName
            presence.state = "Playing " .. niceGamemode .. " on " .. map
        else
            presence.details = "Playing " .. niceGamemode
            presence.state = "Playing on " .. map
        end

        
        -- presence.smallImageKey = gamemode
        -- presence.smallImageText = niceGamemode
    end

    SetPresence(presence)
end)

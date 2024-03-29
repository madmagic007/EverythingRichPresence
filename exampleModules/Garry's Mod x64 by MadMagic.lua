local module = {
    discordAppId = "772853901396279376",
    appName = "gmod",
    titleContains = "x64",
    updateUrl = "https://raw.githubusercontent.com/madmagic007/EverythingRichPresence/main/exampleModules/Garry's%20Mod%20x64%20by%20MadMagic.lua"
}

local addresses = {
    serverName = "client.dll+9C1040",
    map = "engine.dll+5B5A78",
    gamemode = "filesystem_stdio.dll+FB028"
}

local gamemodeMapping = {
    prop_hunt = "Prop Hunt",
    terrortown = "TTT (Trouble in Terrorist Town)"
}

RegisterModule(module, function()
    local serverName = Mem.readString(addresses.serverName)
    local gamemode =  Mem.readString(addresses.gamemode)
    local map = Mem.readString(addresses.map)

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

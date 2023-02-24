local module = {
    appName = "Code",
    titleContains = "Visual",
    discordAppID = "642057918921048075",
    updateUrl = "https://raw.githubusercontent.com/madmagic007/EverythingRichPresence/main/examplemodule.lua"
}

RegisterModule(module, function()
    local fileName = string.gsub(Mem.readString("618801151380"), "Code", " - Visual Studio Code")
        
    SetPresence({
        state = fileName,
        details = "Editing a file",
        elapsed = true
    })
end)

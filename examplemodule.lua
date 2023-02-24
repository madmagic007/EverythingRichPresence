local module = {
    appName = "Code",
    titleContains = "Visual",
    discordAppID = "642057918921048075",
    updateUrl = "https://raw.githubusercontent.com/madmagic007/EverythingRichPresence/main/examplemodule.lua"
}

RegisterModule(module, function()
    print("Updated module is printing")
end)

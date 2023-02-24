local module = {
    appName = "Code",
    titleContains = "Visual",
    discordAppID = "642057918921048075"
}

RegisterModule(module, function()
    local fileName = string.gsub(Mem.readString("618801151380"), "Code", "")

    SetPresence({
        state = fileName,
        details = "Editing a file",
        elapsed = true
    })
end)

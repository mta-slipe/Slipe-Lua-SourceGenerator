function main()
    postPatches()
    
    initEvents()
    AnalyzerTest.Foo.Program.Main()
end

addEventHandler("onResourceStart", resourceRoot, main)
addEventHandler("onClientResourceStart", resourceRoot, main)
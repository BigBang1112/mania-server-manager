namespace ManiaServerManager.Models;

internal class UnusedContent
{
    public HashSet<string> Folders { get; set; } = [
        "TmDedicatedServer/RemoteControlExamples",
        "RemoteControlExamples"
    ];

    public HashSet<string> Files { get; set; } = [
        "ListCallbacks_2011-08-01.html",
        "ListCallbacks_2011-10-06.html",
        "ListCallbacks_2012-06-19.html",
        "ListCallbacks_2013-04-16.html",
        "CommandLine.html",
        "ListCallbacks.html",
        "ListMethods.html",
        "manialink_dedicatedserver.txt",
        "Readme_Dedicated.html",
        "RunTrackmaniaNations.sh",
        "RunTrackmaniaNations.bat",
        "ClientCommandLine.txt"
    ];
}

using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.IO;
using System.Linq;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using MemoriaAlphaSonnetv2.Windows;
using MemoriaAlphaSonnetv2.Services;
using FFXIVClientStructs.FFXIV.Client.Game;

namespace MemoriaAlphaSonnetv2;

public sealed class Plugin : IDalamudPlugin
{
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ITextureProvider TextureProvider { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] internal static IClientState ClientState { get; private set; } = null!;
    [PluginService] internal static IPlayerState PlayerState { get; private set; } = null!;
    [PluginService] internal static IDataManager DataManager { get; private set; } = null!;
    [PluginService] internal static IPluginLog Log { get; private set; } = null!;
    [PluginService] internal static IChatGui ChatGui { get; private set; } = null!;

    private const string CommandName = "/memalpha";
    
    public Configuration Configuration { get; init; }
    public readonly WindowSystem WindowSystem = new("MemoriaAlphaSonnetv2");
    
    private ConfigWindow ConfigWindow { get; init; }
    private MainWindow MainWindow { get; init; }
    
    // Services (initialized in constructor)
    private readonly TocService _tocService;
    private readonly QuestService _questService;

    public Plugin()
    {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        // You might normally want to embed resources and load them from the manifest stream
        var goatImagePath = Path.Combine(PluginInterface.AssemblyLocation.Directory?.FullName!, "goat.png");

        ConfigWindow = new ConfigWindow(this);
        MainWindow = new MainWindow(this, goatImagePath);

        WindowSystem.AddWindow(ConfigWindow);
        WindowSystem.AddWindow(MainWindow);

        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "Opens Memoria Alpha quest tracker"
        });

        // Progress report commands
        CommandManager.AddHandler("/overall", new CommandInfo(OnOverallCommand)
        {
            HelpMessage = "Shows overall quest completion progress"
        });

        CommandManager.AddHandler("/msq", new CommandInfo(OnMsqCommand)
        {
            HelpMessage = "Shows Main Scenario Quest progress"
        });

        CommandManager.AddHandler("/newera", new CommandInfo(OnNewEraCommand)
        {
            HelpMessage = "Shows Chronicles of a New Era progress"
        });

        CommandManager.AddHandler("/msqdebug", new CommandInfo(OnMsqDebugCommand)
        {
            HelpMessage = "Shows incomplete MSQ quests (for debugging)"
        });

        PluginInterface.UiBuilder.Draw += WindowSystem.Draw;
        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUi;
        PluginInterface.UiBuilder.OpenMainUi += ToggleMainUi;

        // Initialize services (loads data files eagerly)
        var pluginDirectory = PluginInterface.AssemblyLocation.Directory?.FullName!;
        _tocService = new TocService(ClientState, Log, pluginDirectory);
        _questService = new QuestService(Log, pluginDirectory);
        
        // Log milestone detection results
        var highestMilestone = _tocService.GetHighestCompletedMilestone();
        if (highestMilestone != null)
        {
            Log.Information($"[TOC] Highest completed MSQ milestone: Patch {highestMilestone}");
        }
        else
        {
            Log.Information("[TOC] No MSQ milestones completed yet (new character or stub IsQuestComplete)");
        }
        
        // Log quest data load summary
        Log.Information($"[QuestService] {_questService.Quests.Count} quests available");

        // Get version dynamically from assembly
        var version = GetType().Assembly.GetName().Version;
        Log.Information($"Memoria Alpha v{version?.Major}.{version?.Minor}.{version?.Build}.{version?.Revision ?? 0} loaded successfully");
    }

    public void Dispose()
    {
        PluginInterface.UiBuilder.Draw -= WindowSystem.Draw;
        PluginInterface.UiBuilder.OpenConfigUi -= ToggleConfigUi;
        PluginInterface.UiBuilder.OpenMainUi -= ToggleMainUi;
        
        WindowSystem.RemoveAllWindows();

        ConfigWindow.Dispose();
        MainWindow.Dispose();

        CommandManager.RemoveHandler(CommandName);
        CommandManager.RemoveHandler("/overall");
        CommandManager.RemoveHandler("/msq");
        CommandManager.RemoveHandler("/newera");
        CommandManager.RemoveHandler("/msqdebug");
        
        // NOTE: Neither TocService nor QuestService need explicit disposal
        // They only hold managed memory (List<T>) which GC handles automatically
    }

    private void OnCommand(string command, string args)
    {
        MainWindow.Toggle();
    }
    
    public void ToggleConfigUi() => ConfigWindow.Toggle();
    public void ToggleMainUi() => MainWindow.Toggle();

    /// <summary>
    /// Handler for /overall command - shows all quest progress
    /// </summary>
    private void OnOverallCommand(string command, string args)
    {
        var startCity = _questService.DetectStartingCity();
        var grandCompany = _questService.DetectGrandCompany();
        
        var filteredQuests = _questService.Quests
            .Where(q => string.IsNullOrEmpty(q.Start) || q.Start == startCity)
            .Where(q => string.IsNullOrEmpty(q.Gc) || q.Gc == grandCompany)
            .ToList();
        
        var total = filteredQuests.Count;
        var completed = filteredQuests.Count(q => q.IdArray.Any(id => QuestManager.IsQuestComplete(id)));
        var percentage = total > 0 ? (completed / (double)total) * 100 : 0;
        
        ChatGui.Print($"[Memoria Alpha] Overall: {completed}/{total} ({percentage:F2}%)");
    }

    /// <summary>
    /// Handler for /msq command - shows Main Scenario progress
    /// </summary>
    private void OnMsqCommand(string command, string args)
    {
        var startCity = _questService.DetectStartingCity();
        var grandCompany = _questService.DetectGrandCompany();
        
        var msqQuests = _questService.Quests
            .Where(q => q.Drawer == "1-msq")
            .Where(q => string.IsNullOrEmpty(q.Start) || q.Start == startCity)
            .Where(q => string.IsNullOrEmpty(q.Gc) || q.Gc == grandCompany)
            .ToList();
        
        var total = msqQuests.Count;
        var completed = msqQuests.Count(q => q.IdArray.Any(id => QuestManager.IsQuestComplete(id)));
        var percentage = total > 0 ? (completed / (double)total) * 100 : 0;
        
        ChatGui.Print($"[Memoria Alpha] Main Scenario: {completed}/{total} ({percentage:F2}%)");
    }

    /// <summary>
    /// Handler for /newera command - shows New Era progress
    /// </summary>
    private void OnNewEraCommand(string command, string args)
    {
        var startCity = _questService.DetectStartingCity();
        
        var newEraQuests = _questService.Quests
            .Where(q => q.Drawer == "2-NewEra")
            .Where(q => string.IsNullOrEmpty(q.Start) || q.Start == startCity)
            .ToList();
        
        var total = newEraQuests.Count;
        var completed = newEraQuests.Count(q => q.IdArray.Any(id => QuestManager.IsQuestComplete(id)));
        var percentage = total > 0 ? (completed / (double)total) * 100 : 0;
        
        ChatGui.Print($"[Memoria Alpha] Chronicles of a New Era: {completed}/{total} ({percentage:F2}%)");
    }

    /// <summary>
    /// Debug command: Lists first 10 incomplete MSQ quests
    /// </summary>
    private void OnMsqDebugCommand(string command, string args)
    {
        var startCity = _questService.DetectStartingCity();
        var grandCompany = _questService.DetectGrandCompany();
        
        var msqQuests = _questService.Quests
            .Where(q => q.Drawer == "1-msq")
            .Where(q => string.IsNullOrEmpty(q.Start) || q.Start == startCity)
            .Where(q => string.IsNullOrEmpty(q.Gc) || q.Gc == grandCompany)
            .ToList();
            
        var incomplete = msqQuests
            .Where(q => !q.IdArray.Any(id => QuestManager.IsQuestComplete(id)))
            .Take(10)
            .ToList();
        
        ChatGui.Print($"[Memoria Alpha] Starting City: {startCity}");
        ChatGui.Print($"[Memoria Alpha] Grand Company: {grandCompany}");
        ChatGui.Print($"[Memoria Alpha] Found {incomplete.Count} incomplete MSQ quests (showing first 10):");
        
        foreach (var quest in incomplete)
        {
            var idList = string.Join(", ", quest.IdArray);
            ChatGui.Print($"  - {quest.Title} (IDs: {idList}) | Patch: {quest.Expansion}");
        }
    }
}

using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.IO;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using MemoriaAlphaSonnetv2.Windows;
using MemoriaAlphaSonnetv2.Services;

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

    private const string CommandName = "/memalpha";
    
    public Configuration Configuration { get; init; }
    public readonly WindowSystem WindowSystem = new("MemoriaAlphaSonnetv2");
    
    private ConfigWindow ConfigWindow { get; init; }
    private MainWindow MainWindow { get; init; }
    
    // Services (initialized in constructor)
    private readonly TocService _tocService;
    private readonly QuestService _questService;  // 🆕 NEW: Add QuestService field

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

        PluginInterface.UiBuilder.Draw += WindowSystem.Draw;
        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUi;
        PluginInterface.UiBuilder.OpenMainUi += ToggleMainUi;

        // Initialize services (loads data files eagerly)
        var pluginDirectory = PluginInterface.AssemblyLocation.Directory?.FullName!;
        _tocService = new TocService(ClientState, Log, pluginDirectory);
        _questService = new QuestService(Log, pluginDirectory);  // 🆕 NEW: Initialize QuestService
        
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
        
        // 🆕 NEW: Log quest data load summary
        Log.Information($"[QuestService] {_questService.Quests.Count} quests available");

        Log.Information("Memoria Alpha v14.2.0.0 loaded successfully");
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
        
        // NOTE: Neither TocService nor QuestService need explicit disposal
        // They only hold managed memory (List<T>) which GC handles automatically
    }

    private void OnCommand(string command, string args)
    {
        MainWindow.Toggle();
    }
    
    public void ToggleConfigUi() => ConfigWindow.Toggle();
    public void ToggleMainUi() => MainWindow.Toggle();
}

using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.IO;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using MemoriaAlphaSonnetv2.Windows;
using MemoriaAlphaSonnetv2.Services;  // NEW: Import our Services namespace

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
    
    // NEW: TocService instance (initialized in constructor)
    private readonly TocService _tocService;

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

        // Tell the UI system that we want our windows to be drawn through the window system
        PluginInterface.UiBuilder.Draw += WindowSystem.Draw;

        // This adds a button to the plugin installer entry of this plugin which allows
        // toggling the display status of the configuration ui
        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUi;

        // Adds another button doing the same but for the main ui of the plugin
        PluginInterface.UiBuilder.OpenMainUi += ToggleMainUi;

        // NEW: Initialize TocService (loads toc.json eagerly)
        var pluginDirectory = PluginInterface.AssemblyLocation.Directory?.FullName!;
        _tocService = new TocService(ClientState, Log, pluginDirectory);
        
        // NEW: Check highest completed MSQ milestone on plugin load
        var highestMilestone = _tocService.GetHighestCompletedMilestone();
        if (highestMilestone != null)
        {
            Log.Information($"[TOC] Highest completed MSQ milestone: Patch {highestMilestone}");
        }
        else
        {
            Log.Information("[TOC] No MSQ milestones completed yet (new character or stub IsQuestComplete)");
        }

        // Log successful initialization
        Log.Information("Memoria Alpha v14.2.0.0 loaded successfully");
    }

    public void Dispose()
    {
        // Unregister all actions to not leak anything during disposal of plugin
        PluginInterface.UiBuilder.Draw -= WindowSystem.Draw;
        PluginInterface.UiBuilder.OpenConfigUi -= ToggleConfigUi;
        PluginInterface.UiBuilder.OpenMainUi -= ToggleMainUi;
        
        WindowSystem.RemoveAllWindows();

        ConfigWindow.Dispose();
        MainWindow.Dispose();

        CommandManager.RemoveHandler(CommandName);
        
        // NOTE: TocService doesn't need explicit disposal (no IDisposable resources)
        // It only holds managed memory (List<TocEntry>) which GC handles automatically
    }

    private void OnCommand(string command, string args)
    {
        // In response to the slash command, toggle the display status of our main ui
        MainWindow.Toggle();
    }
    
    public void ToggleConfigUi() => ConfigWindow.Toggle();
    public void ToggleMainUi() => MainWindow.Toggle();
}

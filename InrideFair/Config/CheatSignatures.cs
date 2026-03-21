namespace InrideFair.Config;

/// <summary>
/// Сигнатуры читов: процессы и файлы.
/// </summary>
public static class CheatSignatures
{
    /// <summary>
    /// Процессы читов.
    /// </summary>
    public static readonly string[] CheatProcesses =
    [
        "extreme injector", "extremeinjector", "dinjector", "loadlibrary", "manualmapinjector",
        "simpleinjector", "ghostinjector", "xenoinjector", "venominjector", "exloader",
        "exloader.exe", "exloader.net", "exhack", "exhack.exe", "ex-loader",
        "cheatengine.exe", "processhacker.exe", "systeminformer.exe", "memoryeditor.exe",
        "gameguardian.exe", "artmoney.exe", "scanmem", "lucamemoryeditor",
        "kdmapper.exe", "iqvw64e.sys", "gdrv.sys", "gdrvloader.exe", "rtcore64.sys",
        "capa.sys", "kprocesshacker.sys", "winio.sys", "giveio.sys",
        "nvidiawhac.exe", "wyvernkernel", "wyvern.exe", "kernel.sys",
        "osiris.dll", "osiris.exe", "neverlose.dll", "neverlose.exe", "neverlose.cc",
        "skeet.dll", "skeet.exe", "gamesense.exe", "gamesense.pub",
        "primordial.dll", "primordial.exe", "legendware.dll", "legendware.exe",
        "templeware.dll", "nexware.dll", "fatality.dll", "fatality.exe",
        "nemesis.dll", "nemesis.tech", "aimware.dll", "aimware.net",
        "onetap.dll", "onetap.exe", "onetap v3", "luna cheat", "luna.dll", "luna.exe",
        "aetherix", "crimsonmods", "midnight.cs2",
        "synapse.exe", "synapse x", "synapse z",
        "krnl.exe", "krnl.dll",
        "scriptware.exe", "script-ware.exe", "scriptware.dll",
        "fluxus.exe", "fluxus.dll",
        "oxygen.exe", "oxygen u",
        "velvet.dll", "velvet.exe",
        "electron.dll", "hydrogen.exe", "solara.exe", "nexus.exe", "arkos.exe",
        "oxygen u.dll", "apex-internal.dll", "apexrage", "apex dumper",
        "r5reloaded", "r5dumper", "apex external",
        "eft dumper", "eft internal", "eft external", "ponely eft", "spt dumper", "aki dumper",
        "valorant external", "valorant dumper", "vanguard bypass", "vgc.exe", "valorant internal",
        "warzone external", "warzone dumper", "mw2 internal", "cod dumper",
        "titan dumper", "il2cppdumper", "il2cpp dumper", "unitydumper", "ue4dumper",
        "unrealdumper", "unitydump.exe", "dumper-7", "dumper 8", "asmresolver",
        "skinchanger.exe", "skinchanger.dll", "mapchanger.exe", "cfgloader.exe",
        "configloader", "pasted.dll", "paste.dll", "exelix.dll", "moonware.dll",
        "midnight.dll", "shadow.dll", "ghost.dll", "phantom.dll", "darkness.dll",
        "evil.dll", "demon.dll", "angel.dll",
        "neverlose lua", "gamesense lua", "skeet lua",
        "cs2loader", "cs2-loader", "loader-cs2",
        "offsets.json", "offsets.ini", "signatures.json",
        // Новые процессы из базы
        "com.swiftsoft", "xone", "interium", "nix", "memesense", "mvploader",
        "sharkhack", "exhack", "neverkernel", "vredux", "mason", "predator",
        "aquila", "luno", "fecurity", "cartel", "aimstar", "tkazer", "naim",
        "pellix", "pussycat", "axios", "onemacro", "softhub", "proext",
        "sapphire", "interwebz", "plague", "vapehook", "smurfwrecker", "iniuria",
        "yeahnot", "legendware", "hauntedproject", "phoenixhack", "onebyteradar",
        "reborn", "onebyte", "ev0lve", "ghostware", "dexterion", "basicmultihack",
        "pudra", "icheat", "sneakys", "krazyhack", "muhprime", "drcheats",
        "rootcheat", "aeonix", "zedt.pw", "devcore", "legifard", "katebot",
        "imxnoobx", "w1nner", "ekknod", "neoxahack", "warware", "weave",
        "aimmy", "paradise", "xenon", "easysp", "en1gma", "Injector", "s1mple",
        "semirage", "invision", "undetek", "spurdo", "webradar", "valthrun",
        "midnight", "nixware"
    ];

    /// <summary>
    /// Файлы читов.
    /// </summary>
    public static readonly string[] CheatFiles =
    [
        "injector.exe", "loader.exe", "dumper.exe", "unpacker.exe",
        "mapchanger.exe", "skinchanger.exe", "cfgloader.exe",
        "extremeinjector.exe", "dinjector.exe", "ghostinjector.exe",
        "xenoinjector.exe", "venominjector.exe", "simpleinjector.exe",
        "cs2loader.exe", "cs2-loader.exe", "loader-cs2.exe",
        "gameloader.exe", "menuloader.exe", "dllloader.exe",
        "exloader.exe", "exhack.exe", "ex-loader.exe",
        "kdmapper.exe", "gdrvloader.exe", "wyvern.exe",
        "osiris.dll", "neverlose.dll", "primordial.dll", "paste.dll",
        "pasted.dll", "exelix.dll", "legendware.dll", "templeware.dll",
        "nexware.dll", "fatality.dll", "nemesis.dll", "aimware.dll",
        "onetap.dll", "skeet.dll", "gamesense.pub", "luna.dll",
        "aetherix.dll", "crimson.dll", "krnl.dll", "scriptware.dll",
        "fluxus.dll", "velvet.dll", "electron.dll", "oxygen u.dll",
        "solara.dll", "il2cppdumper.exe", "unitydumper.exe", "ue4dumper.exe",
        "dumper-7.exe", "dumper-8.exe", "asmresolver.dll",
        "apex-internal.dll", "apexrage.dll", "eft-dumper.exe", "ponely-eft.dll",
        "menu.dll", "config.dll", "lua.dll", "luajit.dll",
        "imgui.dll", "imgui-hook.dll", "directx-hook.dll",
        "esp-overlay.dll", "aimbot.dll", "triggerbot.dll",
        "offsets.json", "offsets.ini", "signatures.json",
        "config.json", "settings.json", "settings.dll",
        // Новые файлы
        "xone.exe", "xone.dll", "interium.exe", "interium.dll",
        "nix.dll", "nix.exe", "nixware.dll", "nixware.exe",
        "memesense.dll", "mvploader.exe", "sharkhack.dll",
        "neverkernel.sys", "vredux.dll", "predator.dll", "predator.exe",
        "aquila.dll", "luno.dll", "cartel.dll", "aimstar.dll",
        "tkazer.dll", "naim.dll", "pellix.dll", "axios.exe",
        "onemacro.exe", "softhub.dll", "proext.dll", "sapphire.dll",
        "vapehook.dll", "iniuria.dll", "hauntedproject.dll",
        "phoenixhack.dll", "onebyteradar.exe", "reborn.dll",
        "ev0lve.dll", "ghostware.dll", "dexterion.dll", "pudra.dll",
        "icheat.dll", "sneakys.dll", "drcheats.dll", "rootcheat.dll",
        "aeonix.dll", "devcore.dll", "imxnoobx.dll", "neoxahack.dll",
        "warware.dll", "weave.dll", "aimmy.exe", "paradise.dll",
        "xenon.dll", "en1gma.dll", "en1gma.exe", "osiris.exe",
        "invision.dll", "undetek.dll", "spurdo.exe", "spurdo.dll",
        "valthrun.dll", "midnight.dll", "midnight.exe",
        "token.ms", "schinese.bin", "russian.bin", "esp-icons.ttf",
        "message-bus.bin", "nl.log", "nl_cs2.log", ".ahk",
        "bhop.exe", "bhop.dll", "bunnyhop.dll", "espd2x.dll",
        "avira.dll", "pphud.dll", "primordial.dll", "nonagon.dll",
        "legit.dll", "hvh.dll", "aimbot.dll", "cs2.glow.dll", "invision.dll"
    ];

    /// <summary>
    /// Расширения архивов.
    /// </summary>
    public static readonly string[] ArchiveExtensions =
    [
        ".zip", ".rar", ".7z", ".tar", ".tar.gz", ".tgz", ".tar.bz2", ".tar.xz"
    ];

    /// <summary>
    /// Системные файлы (исключения).
    /// </summary>
    public static readonly string[] SystemFiles =
    [
        "steam_api.dll", "steam_api64.dll", "dbghelp.dll",
        "dinput8.dll", "dxgi.dll", "winmm.dll", "dsound.dll", "version.dll"
    ];

    /// <summary>
    /// Названия читов для анализа конфигов.
    /// </summary>
    public static readonly string[] CheatNames =
    [
        "neverlose", "osiris", "primordial", "gamesense", "skeet",
        "legendware", "templeware", "nexware", "fatality", "nemesis",
        "aimware", "onetap", "luna", "aetherix", "crimson", "exloader",
        "midnight", "xone", "nixware", "spurdo", "interium",
        "mvploader", "sharkhack", "vredux", "predator", "aquila",
        "luno", "cartel", "aimstar", "tkazer", "pellix", "axios",
        "onemacro", "softhub", "sapphire", "vapehook", "iniuria",
        "phoenixhack", "reborn", "ev0lve", "ghostware", "dexterion",
        "pudra", "icheat", "sneakys", "drcheats", "rootcheat",
        "aeonix", "imxnoobx", "neoxahack", "warware", "weave",
        "aimmy", "paradise", "xenon", "en1gma", "invision",
        "undetek", "valthrun", "memesense", "fecurity", "plague",
        "hauntedproject", "onebyte", "basicmultihack", "krazyhack",
        "muhprime", "zedt.pw", "devcore", "legifard", "katebot",
        "w1nner", "ekknod", "easysp", "s1mple", "semirage",
        "webradar", "nonagon"
    ];

    /// <summary>
    /// Поля читов для анализа конфигов.
    /// </summary>
    public static readonly string[] CheatFields =
    [
        "aimbot", "triggerbot", "bunnyhop", "antiaim", "resolver",
        "fake lag", "desync", "hitchance", "mindamage", "autowall",
        "esp", "glow", "chams", "skinchanger", "inventory", "fov",
        "recoil", "spread", "visibility",
        "legit", "hvh", "bhop", "macros", "macro", "ahk", "autohotkey",
        "injector", "dumper", "unpacker", "offsets", "signatures",
        "lua", "luajit", "imgui", "directx", "overlay", "radar",
        "minimap", "recoil control", "no spread", "no flash", "fullbright",
        "nightmode", "sound esp", "bomb timer", "player esp", "weapon esp",
        "loot esp", "snaplines", "health bar", "armor bar", "skeleton",
        "box esp", "name esp", "distance esp", "ammo esp", "visibility check",
        "silent aim", "rage aimbot", "legit aimbot", "smooth", "fovk",
        "silent", "autofire", "trigger", "auto trigger", "hitbox",
        "selection", "priority", "scale", "delay", "recoil based fov",
        "dormant", "fade", "visible only", "health based color",
        "armor based color", "weapon based color", "team check",
        "flash check", "smoke check", "scope check", "attacker", "target",
        "kill feed", "kill feed esp", "grenade prediction", "grenade trajectory",
        "nade predictor", "molotov timer", "smoke timer", "flash timer",
        "he timer", "decoy timer", "zeus range", "defuse kit", "buy menu",
        "loadout", "perk", "perk esp", "equipment esp", "utility esp",
        "projectile esp", "entity esp", "object esp", "vehicle esp",
        "animal esp", "bot esp", "npc esp", "player info", "weapon info",
        "ammo info", "health info", "armor info", "money info", "rank info",
        "stats info", "match info", "round info", "score info", "timer info",
        "bomb info", "hostage info", "objective esp", "capture zone",
        "control point", "payload", "cart", "bomb site", "plant timer",
        "defuse timer", "respawn timer", "buy timer", "freeze time",
        "round time", "match time", "overtime", "sudden death", "warmup",
        "practice", "scrim", "league", "premier", "competitive", "casual",
        "deathmatch", "arms race", "demolition", "flying scoutsman",
        "trivia", "custom", "workshop", "community", "valve", "official",
        "server", "browser", "favorites", "history", "dropdown", "slider",
        "checkbox", "button", "keybind", "hotkey", "bind", "menu key",
        "toggle key", "config system", "cloud config", "local config",
        "import", "export", "share", "upload", "download", "subscribe",
        "unsubscribe", "update", "changelog", "news", "announcement",
        "status", "status esp", "connection", "latency", "ping", "fps",
        "fps boost", "performance", "optimization", "undetectable",
        "legitbot", "ragebot", "misc", "miscellaneous", "visuals", "skins",
        "gloves", "stickers", "patterns", "float", "seed", "wear",
        "paint kit", "name tag", "inspect", "anime", "stattrak", "souvenir"
    ];

    /// <summary>
    /// Подозрительные поисковые запросы для браузеров.
    /// </summary>
    public static readonly string[] SuspiciousQueries =
    [
        "как скачать чит", "как установить чит", "чит бесплатно",
        "cheat download", "cheat free", "aimbot download", "wallhack download",
        "esp cheat", "triggerbot", "cs2 cheat", "csgo cheat", "valorant cheat",
        "apex cheat", "warzone cheat", "fortnite cheat", "tarkov cheat",
        "eft cheat", "rust cheat", "neverlose download", "osiris cheat",
        "primordial cheat", "skeet download", "gamesense download",
        "exloader", "krnl executor", "synapse x download", "fluxus download",
        "dll injector", "extreme injector", "kdmapper download",
        "vanguard bypass", "faceit bypass", "eac bypass", "battleeye bypass",
        "anticheat bypass", "skinchanger", "mapchanger", "inventory changer",
        // Сайты
        "doomxtf.com", "axios-macro.com", "midnight.im", "xone.fun",
        "blast.hk", "yougame.biz", "jestkii", "wh-satano", "cheatcsgo",
        "interium", "r8cheats", "ezcheats", "exloader", "cs-elect.ru",
        "extrimhack", "neverlose.cc", "gamesense", "legendware", "nixware",
        "phoenix-hack", "rf-cheats", "anyx.gg", "hackvshack.net", "ezyhack",
        "unknowncheats", "cheater.ninja", "insanitycheats.com", "cheater.fun",
        "100cheats.ru", "undetek.com", "cheater.world",
        "zelenka.guru/tags/cs2-cheat", "procheats", "hells-hack.com",
        "clickhack.ru", "procheat.pro", "420cheats.com", "cs2-cheat",
        "wh-satano.ru", "up-game.pro", "millex.xyz", "boohack.ru",
        "elitehacks.ru", "cheatcsgo.ru", "box-cheat.ru", "novamacro",
        "predator.systems", "mvploader", "securecheats", "darkaim",
        "invision.gg", "elitepvpers.com", "privatecheatz", "cosmocheats",
        "skycheats.com", "rockpapershotgun.com", "en1gma.tech", "lunocs2.ru",
        "abyss.gg", "ezcs.ru", "kitchenhack.ru", "ezyhack.ru", "extrimhack.ru",
        "dhjcheats.com", "aimcop.ru", "novamacro.xyz", "promacro.ru",
        "promacro.store", "botmek.ru", "topmacro.ru", "ggmacro.ru",
        "aimstar", "myhacks.store", "interium.ooo", "nixware.cc",
        "arayas-cheats.com", "x-cheats.com", "r8cheats.guru",
        "gamebreaker.ru", "cheatside.ru", "shadowcheat.pro", "h4ck.shop",
        "select-place.ru", "oplata.info", "Spurdo.me", "Chieftain"
    ];

    /// <summary>
    /// Легитимные процессы (исключения).
    /// </summary>
    public static readonly string[] LegitimateProcesses =
    [
        "csgo.exe", "cs2.exe", "cs2", "counter-strike", "csgo",
        "valorant.exe", "valorant", "vgc.exe", "apex legends.exe",
        "apex legends", "r5apex.exe", "steam.exe", "steam",
        "steamwebhelper.exe", "steamwebhelper", "explorer.exe",
        "explorer", "svchost.exe", "svchost", "bash", "zsh",
        "fish", "systemd", "init"
    ];

    /// <summary>
    /// Легитимные игровые пути (исключения).
    /// </summary>
    public static readonly string[] LegitimateGamePaths =
    [
        "steamapps\\common", "steam.exe", "battle.net", "epic games",
        "origin", "ubisoft", "riot games", "valorant", "csgo", "counter-strike"
    ];

    /// <summary>
    /// Подозрительные имена папок.
    /// </summary>
    public static readonly string[] SuspiciousFolderNames =
    [
        "neverlose", "osiris", "primordial", "skeet", "gamesense",
        "exloader", "exhack", "luna", "aetherix", "crimson",
        "cheat", "hack", "modmenu",
        "midnight", "xone", "nixware", "interium", "mvploader",
        "sharkhack", "vredux", "predator", "aquila", "luno",
        "cartel", "aimstar", "tkazer", "pellix", "axios",
        "onemacro", "softhub", "sapphire", "vapehook", "iniuria",
        "phoenixhack", "reborn", "ev0lve", "ghostware", "dexterion",
        "pudra", "icheat", "sneakys", "drcheats", "rootcheat",
        "aeonix", "imxnoobx", "neoxahack", "warware", "weave",
        "aimmy", "paradise", "xenon", "en1gma", "invision",
        "undetek", "valthrun", "memesense", "fecurity", "plague",
        "hauntedproject", "onebyte", "basicmultihack", "krazyhack",
        "muhprime", "zedt.pw", "devcore", "legifard", "katebot",
        "w1nner", "ekknod", "easysp", "s1mple", "semirage",
        "webradar", "nonagon", "legendware", "nexware", "fatality",
        "nemesis", "aimware", "onetap", "aetherix", "crimsonmods",
        "synapse", "krnl", "scriptware", "fluxus", "oxygen",
        "velvet", "electron", "hydrogen", "solara", "nexus", "arkos",
        "apexrage", "r5reloaded", "ponely", "spt", "aki",
        "il2cppdumper", "unitydumper", "ue4dumper", "dumper-7",
        "asmresolver", "skinchanger", "mapchanger", "cfgloader",
        "configloader", "pasted", "paste", "exelix", "moonware",
        "shadow", "ghost", "phantom", "darkness", "evil", "demon",
        "angel", "cs2loader", "gameloader", "menuloader", "dllloader"
    ];

    /// <summary>
    /// Ключевые слова загрузчиков.
    /// </summary>
    public static readonly string[] LoaderKeywords =
    [
        "loader", "injector", "inject", "load", "hook"
    ];

    /// <summary>
    /// Ключевые слова DLL.
    /// </summary>
    public static readonly string[] DllKeywords =
    [
        "menu", "config", "offsets", "signatures", "aimbot", "esp", "trigger"
    ];

    /// <summary>
    /// Конфиг файлы.
    /// </summary>
    public static readonly string[] ConfigFiles =
    [
        "offsets.json", "offsets.ini", "signatures.json", "config.json"
    ];

    /// <summary>
    /// Ключевые слова игр.
    /// </summary>
    public static readonly string[] GameKeywords =
    [
        "cs2", "csgo", "counter-strike", "valorant", "apex", "eft", "tarkov"
    ];

    /// <summary>
    /// Легитимные игровые директории.
    /// </summary>
    public static readonly string[] LegitimateGameDirKeywords =
    [
        "steamapps", "common"
    ];
}

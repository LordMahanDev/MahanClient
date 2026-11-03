using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32.SafeHandles;

class MahanSSApp
{
    static string LogPath, CrashLog;
    [STAThread]
    static void Main()
    {
        try
        {
            string exeDir = AppDomain.CurrentDomain.BaseDirectory;
            LogPath = Path.Combine(exeDir, "startup.log");
            CrashLog = Path.Combine(exeDir, "crash.log");
            try { File.WriteAllText(LogPath, DateTime.Now + " | start\n"); } catch { }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.ThreadException += (s, e) => LogCrash(e.Exception);
            AppDomain.CurrentDomain.UnhandledException += (s, e) => LogCrash(e.ExceptionObject as Exception);
            MainForm frm = null;
            try { frm = new MainForm(); } catch (Exception ex) { LogCrash(ex); return; }
            Application.Run(frm);
        }
        catch (Exception ex) { LogCrash(ex); }
    }
    static void LogCrash(Exception ex)
    {
        if (ex == null) return;
        try { File.AppendAllText(CrashLog, DateTime.Now + "\n" + ex + "\n\n"); } catch { }
        try { MessageBox.Show("Crash:\n" + ex.Message + "\n\nSee: " + CrashLog); } catch { }
    }
}

static class AppInfo
{
    public const string Name = "Mahan Client";
    public const string Version = "v19.2";
    public const string Dev = "Developed by Lord Mahan";
    public const string TelegramId = "@orwinner";
    public const string TelegramUrl = "https://t.me/orwinner";
}

static class Theme
{
    public static readonly Color BtnGray = Color.FromArgb(139, 139, 139);
    public static readonly Color BtnGrayDark = Color.FromArgb(107, 107, 107);
    public static readonly Color BtnGrayHover = Color.FromArgb(160, 160, 160);
    public static readonly Color BtnGreen = Color.FromArgb(62, 125, 62);
    public static readonly Color BtnGreenDark = Color.FromArgb(45, 96, 45);
    public static readonly Color BtnRed = Color.FromArgb(160, 32, 32);
    public static readonly Color BtnRedDark = Color.FromArgb(120, 24, 24);
    public static readonly Color BorderLight = Color.FromArgb(200, 200, 200);
    public static readonly Color BorderDark = Color.FromArgb(55, 55, 55);
    public static readonly Color Text = Color.White;
    public static readonly Color TextShadow = Color.FromArgb(63, 63, 63);
    public static readonly Color TextDim = Color.FromArgb(160, 160, 160);
    public static readonly Color MC_Green = Color.FromArgb(85, 255, 85);
    public static readonly Color MC_Yellow = Color.FromArgb(255, 255, 85);
    public static readonly Color MC_Red = Color.FromArgb(255, 85, 85);
    public static readonly Color MC_Gold = Color.FromArgb(255, 170, 0);
    public static readonly Color MC_Aqua = Color.FromArgb(85, 255, 255);
    public static Font Title, Body, Small, Tiny, Mono;
    static Theme()
    {
        try
        {
            Title = new Font("Consolas", 28f, FontStyle.Bold);
            Body = new Font("Consolas", 12f, FontStyle.Bold);
            Small = new Font("Consolas", 10f, FontStyle.Bold);
            Tiny = new Font("Consolas", 8f, FontStyle.Bold);
            Mono = new Font("Consolas", 10f, FontStyle.Bold);
        }
        catch { Title = SystemFonts.DefaultFont; Body = SystemFonts.DefaultFont; Small = SystemFonts.DefaultFont; Tiny = SystemFonts.DefaultFont; Mono = SystemFonts.DefaultFont; }
    }
}

class GamePanel : Panel
{
    public GamePanel()
    {
        SetStyle(ControlStyles.SupportsTransparentBackColor | ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw, true);
        BackColor = Color.Transparent;
        DoubleBuffered = true;
    }
}

class MCButton : Button
{
    public Color BaseColor = Theme.BtnGray;
    public Color BaseDark = Theme.BtnGrayDark;
    public Color HoverColor = Theme.BtnGrayHover;
    private bool hovered = false;
    public MCButton()
    {
        FlatStyle = FlatStyle.Flat; FlatAppearance.BorderSize = 0;
        ForeColor = Color.White; Font = Theme.Body;
        SetStyle(ControlStyles.SupportsTransparentBackColor | ControlStyles.UserPaint |
                 ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        BackColor = Color.Transparent; Cursor = Cursors.Hand; Height = 50;
    }
    protected override void OnMouseEnter(EventArgs e) { hovered = true; Invalidate(); base.OnMouseEnter(e); }
    protected override void OnMouseLeave(EventArgs e) { hovered = false; Invalidate(); base.OnMouseLeave(e); }
    protected override void OnPaint(PaintEventArgs e)
    {
        try
        {
            Graphics g = e.Graphics;
            Rectangle rect = ClientRectangle;
            Color c1 = hovered ? HoverColor : BaseColor;
            Color c2 = hovered ? ControlPaint.Dark(HoverColor, 0.15f) : BaseDark;
            if (hovered)
                using (var glow = new SolidBrush(Color.FromArgb(30, HoverColor)))
                    g.FillRectangle(glow, new Rectangle(-3, -3, Width + 6, Height + 6));
            using (var brush = new LinearGradientBrush(rect, c1, c2, 90f)) g.FillRectangle(brush, rect);
            using (var pen = new Pen(Theme.BorderLight, 2)) { g.DrawLine(pen, 0, 0, Width - 1, 0); g.DrawLine(pen, 0, 0, 0, Height - 1); }
            using (var pen = new Pen(Theme.BorderDark, 2)) { g.DrawLine(pen, 0, Height - 1, Width - 1, Height - 1); g.DrawLine(pen, Width - 1, 0, Width - 1, Height - 1); }
            using (var pen = new Pen(Color.Black, 1)) g.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
            StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            using (var shadow = new SolidBrush(Theme.TextShadow)) g.DrawString(Text, Font, shadow, new RectangleF(2, 2, Width, Height), sf);
            using (var tb = new SolidBrush(ForeColor)) g.DrawString(Text, Font, tb, new RectangleF(0, 0, Width, Height), sf);
        }
        catch { }
    }
}

class MCProgress : Panel
{
    public int Percent = 0;
    private int _displayed = 0;
    private System.Windows.Forms.Timer _timer;
    public MCProgress()
    {
        SetStyle(ControlStyles.SupportsTransparentBackColor | ControlStyles.UserPaint |
                 ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw, true);
        DoubleBuffered = true; BackColor = Color.Transparent; Height = 40;
        _timer = new System.Windows.Forms.Timer { Interval = 16 };
        _timer.Tick += (s, e) =>
        {
            if (_displayed != Percent)
            {
                int diff = Percent - _displayed;
                int step = Math.Max(1, Math.Abs(diff) / 8);
                _displayed = diff > 0 ? Math.Min(Percent, _displayed + step) : Math.Max(Percent, _displayed - step);
                Invalidate();
            }
            else _timer.Stop();
        };
    }
    public void SetPercent(int p) { Percent = Math.Max(0, Math.Min(100, p)); if (!_timer.Enabled) _timer.Start(); }
    protected override void OnPaint(PaintEventArgs e)
    {
        try
        {
            Graphics g = e.Graphics;
            Rectangle outer = new Rectangle(0, 8, Width - 1, Height - 16);
            using (var b = new SolidBrush(Color.FromArgb(22, 22, 22))) g.FillRectangle(b, outer);
            if (_displayed > 0)
            {
                int fillW = (int)((outer.Width - 4) * _displayed / 100.0);
                if (fillW > 2)
                {
                    Rectangle inner = new Rectangle(outer.X + 2, outer.Y + 2, fillW, outer.Height - 4);
                    using (var brush = new LinearGradientBrush(inner, Theme.MC_Green, Color.FromArgb(40, 160, 40), 90f))
                        g.FillRectangle(brush, inner);
                }
            }
            using (var pen = new Pen(Color.Black, 2)) g.DrawRectangle(pen, outer);
            StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            using (var shadow = new SolidBrush(Theme.TextShadow)) g.DrawString(Percent + "%", Theme.Body, shadow, new RectangleF(1, 1, Width, Height), sf);
            using (var tb = new SolidBrush(Color.White)) g.DrawString(Percent + "%", Theme.Body, tb, new RectangleF(0, 0, Width, Height), sf);
        }
        catch { }
    }
    protected override void Dispose(bool disposing) { if (disposing && _timer != null) { _timer.Dispose(); _timer = null; } base.Dispose(disposing); }
}

class FindingInfo
{
    public string FullPath = "", DisplayPath = "", Source = "", Note = "";
    public List<string> Matches = new List<string>();
    public DateTime? Created, Modified, Accessed, LastChange;
    public bool IsCheat;
    public bool IsRunning;
}

class MainForm : Form
{
    GamePanel menuPanel, scanPanel, resultsPanel;
    MCButton btnStart, btnExit, btnCancel, btnBack, btnSave;
    MCButton btnTaskMgr, btnFileLoc;
    Label lblLogo, lblVersion, lblDev, lblDesc;
    Label lblScanTitle, lblStep, lblStatus, lblVerdict;
    Label lblTelegramMenu, lblTelegramScan, lblTelegramResults;
    MCProgress progress;
    RichTextBox logBox, resultsBox;

    volatile bool cancelRequested = false;
    Thread scanThread;
    List<FindingInfo> currentFindings = new List<FindingInfo>();
    object findingsLock = new object();
    int totalCheats = 0, totalSuspicious = 0;
    string historyPath = "";
    DateTime scanStartTime;
    StreamWriter logWriter;
    HashSet<string> historySeen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    bool lastAccessEnabled = false;

    
    static readonly HashSet<string> ProxyProcessWhitelist = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "xray.exe", "xray",
        "v2ray.exe", "v2ray", "v2raya.exe",
        "nekoray.exe", "nekobox.exe",
        "hiddify.exe", "hiddify-core.exe",
        "clash.exe", "clash-meta.exe", "clash-verge.exe",
        "sing-box.exe", "singbox.exe",
        "trojan.exe", "trojan-go.exe",
        "shadowsocks.exe", "ss-local.exe", "sslocal.exe", "ss-server.exe",
        "warp-svc.exe", "wireguard.exe",
    };

    
    static readonly string[] ProxyPathSkip = {
        "\\v2ray\\", "\\v2raya\\", "\\xray-core\\", "\\xray-core-",
        "\\nekoray\\", "\\nekobox\\", "\\hiddify\\",
        "\\clash\\", "\\clash-meta\\", "\\clash-verge\\",
        "\\sing-box\\", "\\singbox\\",
        "\\shadowsocks\\", "\\trojan-go\\", "\\trojan\\",
        "\\warp\\", "\\wireguard\\",
    };

    [DllImport("kernel32.dll", SetLastError = true)] static extern IntPtr OpenProcess(uint a, bool i, int pid);
    [DllImport("kernel32.dll", SetLastError = true)] static extern bool ReadProcessMemory(IntPtr h, IntPtr addr, byte[] b, int n, out int r);
    [DllImport("kernel32.dll", SetLastError = true)] static extern int VirtualQueryEx(IntPtr h, IntPtr addr, out MBI m, uint l);
    [DllImport("kernel32.dll")] static extern bool CloseHandle(IntPtr h);
    [StructLayout(LayoutKind.Sequential)]
    struct MBI { public IntPtr BaseAddress, AllocationBase; public uint AllocationProtect; public IntPtr RegionSize; public uint State, Protect, Type; }
    [DllImport("advapi32.dll", SetLastError = true)] static extern bool OpenProcessToken(IntPtr h, uint acc, out IntPtr token);
    [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)] static extern bool LookupPrivilegeValue(string sys, string name, out LUID luid);
    [DllImport("advapi32.dll", SetLastError = true)] static extern bool AdjustTokenPrivileges(IntPtr token, bool dis, ref TOKEN_PRIVILEGES newState, uint len, IntPtr prev, IntPtr ret);
    [StructLayout(LayoutKind.Sequential)] struct LUID { public uint LowPart; public int HighPart; }
    [StructLayout(LayoutKind.Sequential)] struct LUID_AND_ATTRIBUTES { public LUID Luid; public uint Attributes; }
    [StructLayout(LayoutKind.Sequential)] struct TOKEN_PRIVILEGES { public uint PrivilegeCount; public LUID_AND_ATTRIBUTES Privileges; }
    const uint TOKEN_ADJUST_PRIVILEGES = 0x0020, TOKEN_QUERY = 0x0008, SE_PRIVILEGE_ENABLED = 0x0002;
    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)] static extern SafeFileHandle CreateFile(string n, uint acc, uint shr, IntPtr sa, uint cd, uint fa, IntPtr ht);
    [DllImport("kernel32.dll", SetLastError = true)] static extern bool DeviceIoControl(SafeFileHandle h, uint code, IntPtr inB, uint inS, IntPtr outB, uint outS, out uint ret, IntPtr ov);
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    struct USN_JOURNAL_DATA { public ulong UsnJournalID; public long FirstUsn, NextUsn, LowestValidUsn, MaxUsn; public ulong MaximumSize, AllocationDelta; }
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    struct READ_USN_JOURNAL_DATA { public long StartUsn; public uint ReasonMask, ReturnOnlyOnClose; public long Timeout, BytesToWaitFor; public ulong UsnJournalID; public ushort MinMajorVersion, MaxMajorVersion; }
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    struct MFT_ENUM_DATA { public ulong StartFileReferenceNumber; public long LowUsn, HighUsn; }
    const uint FSCTL_QUERY_USN_JOURNAL = 0x000900F4, FSCTL_READ_USN_JOURNAL = 0x000900BB;
    const uint FSCTL_CREATE_USN_JOURNAL = 0x000900E7, FSCTL_DELETE_USN_JOURNAL = 0x000900F8;
    const uint FSCTL_ENUM_USN_DATA = 0x000900B3, FSCTL_GET_NTFS_FILE_RECORD = 0x00090068;
    const uint GENERIC_READ = 0x80000000, GENERIC_WRITE = 0x40000000;
    const uint FILE_SHARE_READ = 1, FILE_SHARE_WRITE = 2, OPEN_EXISTING = 3;
    const uint USN_REASON_FILE_DELETE = 0x200, USN_REASON_RENAME_OLD_NAME = 0x1000, USN_REASON_RENAME_NEW_NAME = 0x2000;

    static readonly string[] StrongSigs = {
        
        "xray-fabric","xray-forge","xray-mod","xray.client",
        "advancedxray","advanced-xray","minecraft-xray",
        "xraymod","xray.mod","xray_mod",
        "com/xray/mods","net/xray/mods","org/xray/mods",
        "com/xray/minecraft","net/xray/minecraft","org/xray/minecraft",
        "XRayMod","XRayClient","XRayHack","XRayCheat",

        // بقیه strong sigs
        "net/wurstclient","org/wurstclient","com/meteordevelopment","net/ccbluex","club/fdpclient",
        "com/impactclient","com/aristois","com/kamiblue","com/rusherhack","com/phobos","com/konas",
        "com/novoline","com/salhack","fr1kin/forgehax","com/inertia","com/bleachhack","com/seppuku",
        "com/wolfram","com/sigma","com/flux","com/moonlight","com/ares","com/raven","com/rise",
        "com/tenacity","com/vape","com/entropy","com/sulfur","com/jessica","com/akira","com/noise",
        "com/nightx","com/azura","com/skiddermc","baritone/api","me/zeroeightysix",
        "WurstClient","MeteorClient","LiquidBounce","LiquidBouncePlus","ImpactClient","FutureClient",
        "KamiBlue","RusherHack","Novoline","SalHack","ForgeHax","BleachHack","SeppukuHack","WolframClient",
        "SigmaClient","FluxClient","MoonLightClient","AresClient","AresGhost","RavenB++","RiseClient",
        "TenacityClient","VapeV4","VapeLite","EntropyClient","SmartClient","SulfurClient","JessicaClient",
        "FDPClient","AkiraClient","AkiraGhost","NoiseClient","NightXClient","AzuraClient","Nursultan",
        "WildClient","CatLean","DeadcodeHack","DeltaClient","ExcellentClient","MyauVibe","MeowClient",
        "AugustusClient","AstralisClient","MadcatClient","Earthhack","CatalystClient","ValkyrieClient",
        "ApolloClient","ZephyrClient","BadmanClient","RadiumClient","ActiniumClient","CelsiusClient",
        "IrisClient","SurgeClient","UndisputedClient","NitroClient","AscendencyClient","NuvolaClient",
        "Wolfcock","ProtoHax","LatiteClient","FlarialClient","XeloClient","PraxClient","Horion","Borion",
        "OnixClient","JoivoClient","LuconiaClient","RuneClient","Skuxx","Unisteria","AstraClient",
        "DynamicClient","Silkcc","AchillesClient","LuminaClient","GasperClient","ToadClient","ZoominClient",
        "ItamiClient","WhiteoutClient","JigsawClient","HeraClient","RemixClient","SkidBounce","GoldBounce",
        "LeaderClient","CrossSine","MinusBounce","ChimeraClient","KawaiiClient","CattoClient","Pr3rox",
        "YuriClient","NodusClient","OyVeyClient","JexClient","FallenClient","NamiClient","ManloxxClient",
        "EssenceClient","Expens1veClient","WayneClient","MonotonClient",
        "BozeClient","boze.dev","net/boze","com/boze","Dqrkis","OrchardClient","ZenithMacros","Velaris",
        "NoxxClient","CalciumClient","ZelithClient","XenonV2","anamvmnt",
        "AutoLavaMixing","AutoTreeFarmer","AuctionSniper","RTPBaseFinder","FakeScoreboard","AutoBoneOrder",
        "SilentHomeSetter","TunnelBaseFinder","AmethystESP","BedrockHoleESP","TuffChunkFinder","SusChunkFinder",
        "LegitTridentFly","ShieldBreaker","SingleAnchor","PistonAura","SpawnerFinder","StashFinder",
        "HoleESP","PearlESP","TridentESP","PistonESP","BaseESP","LightESP","MobESP","SusESP",
        "AutoCart","SafeCart","CartTrap","AntiTrap","AnchorMacro","SpearSwap","automine","AHSniper",
        "FakePayments","FakeRoles","ManualTunnel","BreachSwap","Invtotem"
    };

    static readonly string[] SuspiciousKeywords = {
        "cheat","hack","wurst","meteor","vape","sigma","liquid","impact","aristois","futureclient",
        "novoline","entropy","riseclient","tenacity","salhack","seppuku","bleach","forgehax","rusher",
        "phobos","konas","pandora","astolfo","velaris","zelith","noxx","calcium","zenith","inertia",
        "wolfram","killaura","autocrystal","crystalaura","scaffold","triggerbot","aimbot",
        "aimassist","wallhack","tracers","nametags","esp","nofall","noslow","velocity","antikb",
        "antiknockback","bhop","speedhack","regen","blink","phase","noclip","nuker","tower","fastplace",
        "autoclicker","clicker","antiaim","autototem","bedaura","anchoraura","ghostclient","cheatclient",
        // ⭐ X-Ray ماینکرفت — فقط الگوهای اختصاصی
        "xray-mod","xray-fabric","xray-forge","xray-forge-mod",
        "xray-mod-fabric","xray-fabric-mod","xray-mod-forge",
        "xray_client","xrayclient","xray.client",
        "advancedxray","advanced-xray","advanced_xray",
        "minecraft-xray","minecraftxray","xray-mod-mc",
        "xrayhack","xray-cheat","xraycheat","xraymod",
        "xraymod","xray_mod"
    };

    static readonly string[] TargetExts = {
        ".jar",".json",".txt",".properties",".cfg",".yml",".yaml",".toml",".xml",
        ".exe",".dll",".bat",".sh",".ps1",".vbs",".js",".py",".mf",
        ".pf",".lnk",".class"
    };
    static readonly string[] TextExts = { ".json",".txt",".properties",".cfg",".yml",".yaml",".toml",".xml",".bat",".sh",".ps1",".vbs",".js",".py",".mf" };

    const int CHUNK = 1 << 20, MAX_DEPTH = 4, MAX_BIN_READ = 5 * 1024 * 1024;
    const int MAX_JAR_ENTRIES = 5000, MAX_MFT_RECORDS = 3000000;
    const int MAX_FILES_SAFETY = 200000;
    const long MAX_JAR_SIZE = 200L * 1024 * 1024, MAX_TEXT_SIZE = 20L * 1024 * 1024;

    static readonly HashSet<string> DirSkipNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase) {
        "windows","system volume information","recovery","perflogs","boot","msocache","config.msi",
        "$windows.~bt","$windows.~ws","winsxs","installer","assembly","servicing","node_modules",
        ".git",".svn",".hg",".idea",".vs",".vscode","__pycache__",".gradle",".m2",".nuget",".npm",".cache",
        "discord","slack","spotify","code","adobe","mozilla","google","opera","brave",
        "teams","zoom","skype","obs-studio","obs","steam","epic","unity","jetbrains",
        "pypa","python","go","cargo","rustup","bundle","composer","nugetfallbackfolder"
    };

    static readonly string[] JarPathSkip = {
        "\\libraries\\","/libraries/","\\versions\\","/versions/","\\assets\\","/assets/",
        "\\runtimes\\","/runtimes/","\\natives\\","/natives/","\\cache\\","/cache/",
        "\\.git\\","/.git/","\\logs\\","/logs/","\\crash-reports\\","/crash-reports/",
        "\\screenshots\\","/screenshots/","\\resourcepacks\\","/resourcepacks/",
        "\\shaderpacks\\","/shaderpacks/","\\saves\\","/saves/","\\processedmods\\","/processedmods/",
        "\\remappedjars\\","/remappedjars/","\\.fabric\\","/.fabric/"
    };

    public MainForm()
    {
        Text = AppInfo.Name + " " + AppInfo.Version;
        ClientSize = new Size(920, 680);
        MinimumSize = new Size(840, 620);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.FromArgb(26, 26, 26);
        ForeColor = Theme.Text;
        Font = Theme.Body;
        DoubleBuffered = true;
        try { Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath); } catch { }

        LoadBackgroundImage();
        InitializeMenuPanel();
        InitializeScanPanel();
        InitializeResultsPanel();

        menuPanel.Resize += (s, e) => LayoutMenuPanel();
        scanPanel.Resize += (s, e) => LayoutScanPanel();
        resultsPanel.Resize += (s, e) => LayoutResultsPanel();

        Load += (s, e) =>
        {
            try
            {
                ShowMenu();
                LayoutMenuPanel();
                LayoutScanPanel();
                LayoutResultsPanel();
            }
            catch (Exception ex) { MessageBox.Show("Load error: " + ex.Message); }
        };
    }

    void LoadBackgroundImage()
    {
        try
        {
            string exeDir = AppDomain.CurrentDomain.BaseDirectory;
            string[] candidates = { "mahan12.jpg", "mahan12.jpeg", "mahan12.png", "background.jpg", "background.png" };
            foreach (string name in candidates)
            {
                string p = Path.Combine(exeDir, name);
                if (!File.Exists(p)) continue;
                Image img = null;
                try { img = Image.FromFile(p); } catch { continue; }
                try
                {
                    Bitmap darkened = new Bitmap(img.Width, img.Height);
                    using (Graphics g = Graphics.FromImage(darkened))
                    {
                        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        g.DrawImage(img, 0, 0, img.Width, img.Height);
                        using (var overlay = new LinearGradientBrush(new Rectangle(0, 0, img.Width, img.Height),
                            Color.FromArgb(190, 5, 8, 15), Color.FromArgb(190, 15, 20, 35), 90f))
                            g.FillRectangle(overlay, 0, 0, img.Width, img.Height);
                    }
                    this.BackgroundImage = darkened;
                }
                catch { this.BackgroundImage = img; }
                this.BackgroundImageLayout = ImageLayout.Stretch;
                break;
            }
        }
        catch { }
    }

    void InitializeMenuPanel()
    {
        menuPanel = new GamePanel();
        menuPanel.Dock = DockStyle.Fill;

        lblLogo = new Label { Text = "MAHAN CLIENT", Font = Theme.Title, ForeColor = Theme.MC_Gold, TextAlign = ContentAlignment.MiddleCenter, BackColor = Color.Transparent, Size = new Size(800, 60) };
        menuPanel.Controls.Add(lblLogo);
        lblVersion = new Label { Text = "~ " + AppInfo.Version + " ~", Font = Theme.Small, ForeColor = Theme.MC_Green, TextAlign = ContentAlignment.MiddleCenter, BackColor = Color.Transparent, Size = new Size(800, 30) };
        menuPanel.Controls.Add(lblVersion);
        lblDev = new Label { Text = AppInfo.Dev, Font = Theme.Small, ForeColor = Theme.MC_Aqua, TextAlign = ContentAlignment.MiddleCenter, BackColor = Color.Transparent, Size = new Size(800, 30) };
        menuPanel.Controls.Add(lblDev);
        lblDesc = new Label { Text = "AppData + Temp + Prefetch + Recent + Tasklist", Font = Theme.Tiny, ForeColor = Theme.TextDim, TextAlign = ContentAlignment.MiddleCenter, BackColor = Color.Transparent, Size = new Size(800, 25) };
        menuPanel.Controls.Add(lblDesc);

        btnStart = new MCButton { Text = "START SCAN", BaseColor = Theme.BtnGreen, BaseDark = Theme.BtnGreenDark, HoverColor = Color.FromArgb(85, 160, 85), Size = new Size(300, 55), Font = Theme.Body };
        btnStart.Click += (s, e) => StartScan();
        menuPanel.Controls.Add(btnStart);
        btnExit = new MCButton { Text = "EXIT", BaseColor = Theme.BtnRed, BaseDark = Theme.BtnRedDark, HoverColor = Color.FromArgb(200, 50, 50), Size = new Size(300, 55), Font = Theme.Body };
        btnExit.Click += (s, e) => Close();
        menuPanel.Controls.Add(btnExit);
        lblTelegramMenu = MakeTelegramLabel();
        menuPanel.Controls.Add(lblTelegramMenu);

        Controls.Add(menuPanel);
    }

    Label MakeTelegramLabel()
    {
        Label lbl = new Label
        {
            Text = "Support: " + AppInfo.TelegramId,
            Font = new Font("Consolas", 10f, FontStyle.Bold),
            ForeColor = Color.FromArgb(150, 150, 150),
            BackColor = Color.Transparent, AutoSize = true, Cursor = Cursors.Hand
        };
        lbl.MouseEnter += (s, e) => { lbl.ForeColor = Theme.MC_Aqua; };
        lbl.MouseLeave += (s, e) => { lbl.ForeColor = Color.FromArgb(150, 150, 150); };
        lbl.Click += (s, e) => { try { Process.Start(AppInfo.TelegramUrl); } catch { } };
        return lbl;
    }

    void LayoutMenuPanel()
    {
        try
        {
            int w = menuPanel.ClientSize.Width, h = menuPanel.ClientSize.Height, cx = w / 2;
            lblLogo.Left = cx - lblLogo.Width / 2; lblLogo.Top = (int)(h * 0.14);
            lblVersion.Left = cx - lblVersion.Width / 2; lblVersion.Top = lblLogo.Top + lblLogo.Height + 5;
            lblDev.Left = cx - lblDev.Width / 2; lblDev.Top = lblVersion.Top + lblVersion.Height + 10;
            lblDesc.Left = cx - lblDesc.Width / 2; lblDesc.Top = lblDev.Top + lblDev.Height + 20;
            btnStart.Left = cx - btnStart.Width / 2; btnStart.Top = (int)(h * 0.55);
            btnExit.Left = cx - btnExit.Width / 2; btnExit.Top = btnStart.Top + btnStart.Height + 25;
            if (lblTelegramMenu != null) { lblTelegramMenu.Left = cx - lblTelegramMenu.Width / 2; lblTelegramMenu.Top = btnExit.Top + btnExit.Height + 30; lblTelegramMenu.BringToFront(); }
            menuPanel.Invalidate();
        }
        catch { }
    }

    void InitializeScanPanel()
    {
        scanPanel = new GamePanel();
        scanPanel.Dock = DockStyle.Fill;
        scanPanel.Visible = false;
        lblScanTitle = new Label { Text = "SCANNING IN PROGRESS...", Font = Theme.Title, ForeColor = Theme.MC_Gold, TextAlign = ContentAlignment.MiddleCenter, BackColor = Color.Transparent };
        scanPanel.Controls.Add(lblScanTitle);
        progress = new MCProgress(); scanPanel.Controls.Add(progress);
        lblStep = new Label { Text = "[0/8] Initializing...", Font = Theme.Small, ForeColor = Theme.MC_Aqua, TextAlign = ContentAlignment.MiddleCenter, BackColor = Color.Transparent };
        scanPanel.Controls.Add(lblStep);
        lblStatus = new Label { Text = "Preparing...", Font = Theme.Tiny, ForeColor = Theme.TextDim, TextAlign = ContentAlignment.MiddleCenter, BackColor = Color.Transparent };
        scanPanel.Controls.Add(lblStatus);
        logBox = new RichTextBox { Font = Theme.Mono, BackColor = Color.FromArgb(22, 22, 22), ForeColor = Theme.Text, BorderStyle = BorderStyle.None, ReadOnly = true, ScrollBars = RichTextBoxScrollBars.Vertical };
        scanPanel.Controls.Add(logBox);
        btnCancel = new MCButton { Text = "CANCEL", BaseColor = Theme.BtnRed, BaseDark = Theme.BtnRedDark, HoverColor = Color.FromArgb(200, 50, 50), Size = new Size(180, 50) };
        btnCancel.Click += (s, e) => { cancelRequested = true; btnCancel.Enabled = false; btnCancel.Text = "Cancelling..."; };
        scanPanel.Controls.Add(btnCancel);
        lblTelegramScan = MakeTelegramLabel();
        scanPanel.Controls.Add(lblTelegramScan);
        Controls.Add(scanPanel);
    }

    void LayoutScanPanel()
    {
        try
        {
            int w = scanPanel.ClientSize.Width, h = scanPanel.ClientSize.Height, cx = w / 2;
            int margin = 40, contentW = w - margin * 2;
            lblScanTitle.Width = contentW; lblScanTitle.Height = 60; lblScanTitle.Left = margin; lblScanTitle.Top = 25;
            progress.Left = margin; progress.Width = contentW; progress.Top = 95;
            lblStep.Width = contentW; lblStep.Height = 30; lblStep.Left = margin; lblStep.Top = 145;
            lblStatus.Width = contentW; lblStatus.Height = 25; lblStatus.Left = margin; lblStatus.Top = 175;
            logBox.Left = margin; logBox.Width = contentW; logBox.Top = 215; logBox.Height = Math.Max(100, h - 215 - 110);
            btnCancel.Left = cx - btnCancel.Width / 2; btnCancel.Top = h - btnCancel.Height - 30;
            if (lblTelegramScan != null) { lblTelegramScan.Left = w - lblTelegramScan.Width - 12; lblTelegramScan.Top = h - lblTelegramScan.Height - 8; lblTelegramScan.BringToFront(); }
        }
        catch { }
    }

    void InitializeResultsPanel()
    {
        resultsPanel = new GamePanel();
        resultsPanel.Dock = DockStyle.Fill;
        resultsPanel.Visible = false;
        lblVerdict = new Label { Text = "SCAN RESULTS", Font = Theme.Title, ForeColor = Theme.MC_Green, TextAlign = ContentAlignment.MiddleCenter, BackColor = Color.Transparent };
        resultsPanel.Controls.Add(lblVerdict);
        resultsBox = new RichTextBox { Font = Theme.Mono, BackColor = Color.FromArgb(22, 22, 22), ForeColor = Theme.Text, BorderStyle = BorderStyle.None, ReadOnly = true, ScrollBars = RichTextBoxScrollBars.Vertical };
        resultsPanel.Controls.Add(resultsBox);
        btnBack = new MCButton { Text = "< BACK TO MENU", BaseColor = Theme.BtnGray, BaseDark = Theme.BtnGrayDark, HoverColor = Theme.BtnGrayHover, Size = new Size(240, 50) };
        btnBack.Click += (s, e) => ShowMenu();
        resultsPanel.Controls.Add(btnBack);
        btnSave = new MCButton { Text = "SAVE REPORT", BaseColor = Theme.BtnGreen, BaseDark = Theme.BtnGreenDark, HoverColor = Color.FromArgb(85, 160, 85), Size = new Size(240, 50) };
        btnSave.Click += (s, e) => SaveReport();
        resultsPanel.Controls.Add(btnSave);

        btnTaskMgr = new MCButton { Text = "OPEN TASK MANAGER", BaseColor = Color.FromArgb(220, 100, 30), BaseDark = Color.FromArgb(160, 70, 20), HoverColor = Color.FromArgb(255, 130, 50), Size = new Size(240, 50), Font = Theme.Small, Visible = false };
        btnTaskMgr.Click += (s, e) => OpenTaskManager();
        resultsPanel.Controls.Add(btnTaskMgr);

        btnFileLoc = new MCButton { Text = "FILE LOCATION", BaseColor = Color.FromArgb(60, 100, 160), BaseDark = Color.FromArgb(40, 70, 120), HoverColor = Color.FromArgb(80, 130, 200), Size = new Size(240, 50), Font = Theme.Small, Visible = false };
        btnFileLoc.Click += (s, e) => OpenFirstCheatLocation();
        resultsPanel.Controls.Add(btnFileLoc);

        lblTelegramResults = MakeTelegramLabel();
        resultsPanel.Controls.Add(lblTelegramResults);
        Controls.Add(resultsPanel);
    }

    void LayoutResultsPanel()
    {
        try
        {
            int w = resultsPanel.ClientSize.Width, h = resultsPanel.ClientSize.Height, cx = w / 2;
            int margin = 40, contentW = w - margin * 2;
            lblVerdict.Width = contentW; lblVerdict.Height = 60; lblVerdict.Left = margin; lblVerdict.Top = 20;
            int row1Y = h - 75;
            int row2Y = row1Y - 60;
            resultsBox.Left = margin; resultsBox.Width = contentW; resultsBox.Top = 85;
            resultsBox.Height = Math.Max(100, row2Y - 85 - 15);
            btnBack.Left = cx - btnBack.Width - 10; btnBack.Top = row1Y;
            btnSave.Left = cx + 10; btnSave.Top = row1Y;
            if (btnTaskMgr != null) { btnTaskMgr.Left = cx - btnTaskMgr.Width - 10; btnTaskMgr.Top = row2Y; btnTaskMgr.BringToFront(); }
            if (btnFileLoc != null) { btnFileLoc.Left = cx + 10; btnFileLoc.Top = row2Y; btnFileLoc.BringToFront(); }
            if (lblTelegramResults != null) { lblTelegramResults.Left = w - lblTelegramResults.Width - 12; lblTelegramResults.Top = h - lblTelegramResults.Height - 8; lblTelegramResults.BringToFront(); }
        }
        catch { }
    }

    void ShowMenu() { menuPanel.Visible = true; scanPanel.Visible = false; resultsPanel.Visible = false; LayoutMenuPanel(); }
    void ShowScan() { menuPanel.Visible = false; scanPanel.Visible = true; resultsPanel.Visible = false; LayoutScanPanel(); }
    void ShowResults() { menuPanel.Visible = false; scanPanel.Visible = false; resultsPanel.Visible = true; LayoutResultsPanel(); }

    void UpdateProgressSafe(int pct, string step, string status)
    {
        if (IsDisposed || !IsHandleCreated) return;
        try { BeginInvoke(new Action(delegate { progress.SetPercent(pct); lblStep.Text = step; lblStatus.Text = status; })); } catch { }
    }

    void LogUI(string msg, Color color)
    {
        try { if (logWriter != null) { logWriter.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " | " + msg); logWriter.Flush(); } } catch { }
        if (IsDisposed || !IsHandleCreated) return;
        try
        {
            BeginInvoke(new Action(delegate
            {
                logBox.SelectionStart = logBox.TextLength;
                logBox.SelectionLength = 0;
                logBox.SelectionColor = color;
                logBox.AppendText(msg + "\n");
                logBox.ScrollToCaret();
            }));
        }
        catch { }
    }

    void AddFinding(FindingInfo fi)
    {
        lock (findingsLock) { currentFindings.Add(fi); if (fi.IsCheat) totalCheats++; else totalSuspicious++; }
    }

    void RunUnhideAttributes(List<string> paths)
    {
        foreach (string path in paths)
        {
            if (string.IsNullOrEmpty(path) || !Directory.Exists(path)) continue;
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo("attrib", "-h -s /s /d \"" + path + "\"")
                {
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    Verb = "runas"
                };
                using (Process p = Process.Start(psi)) { p.WaitForExit(15000); }
            }
            catch { }
        }
        try
        {
            ProcessStartInfo psi = new ProcessStartInfo("attrib", "-h -s \"C:\\$Recycle.Bin\"")
            { UseShellExecute = false, CreateNoWindow = true, Verb = "runas" };
            using (Process p = Process.Start(psi)) { p.WaitForExit(5000); }
        }
        catch { }
        LogUI("[OK] Unhid hidden/system files", Theme.MC_Green);
    }

    void RunTaskList()
    {
        LogUI("[*] Checking running processes with tasklist...", Theme.MC_Aqua);
        try
        {
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(
                "SELECT Name, ProcessId, ExecutablePath, CommandLine FROM Win32_Process"))
            {
                int flagged = 0;
                foreach (ManagementObject obj in searcher.Get())
                {
                    try
                    {
                        string name = obj["Name"] as string;
                        if (string.IsNullOrEmpty(name)) continue;
                        string lowerName = name.ToLowerInvariant();

                        // ⭐ Whitelist: skip known proxy/VPN/dev processes
                        if (ProxyProcessWhitelist.Contains(lowerName))
                            continue;

                        List<string> hits = GetSuspicious(name);
                        if (hits.Count == 0) continue;

                        string path = obj["ExecutablePath"] as string;
                        string cmdLine = obj["CommandLine"] as string;
                        if (string.IsNullOrEmpty(path)) path = "(no path)";

                        // ⭐ Whitelist: skip trusted paths
                        string lowerPath = path.ToLowerInvariant();
                        bool trusted =
                            lowerPath.StartsWith("c:\\windows\\") ||
                            lowerPath.StartsWith("c:\\program files\\") ||
                            lowerPath.StartsWith("c:\\program files (x86)\\") ||
                            lowerPath.Contains("\\microsoft\\") ||
                            lowerPath.Contains("\\nvidia\\") ||
                            lowerPath.Contains("\\intel\\") ||
                            lowerPath.Contains("\\amd\\");

                        // ⭐ Whitelist: skip proxy folder paths
                        if (!trusted)
                        {
                            foreach (string skip in ProxyPathSkip)
                            {
                                if (lowerPath.Contains(skip)) { trusted = true; break; }
                            }
                        }

                        if (trusted) continue;

                        flagged++;
                        FindingInfo fi = new FindingInfo();
                        fi.FullPath = path;
                        fi.DisplayPath = "Process: " + name + "  ->  " + path;
                        fi.Source = "Tasklist";
                        fi.Matches = hits;
                        fi.Note = string.IsNullOrEmpty(cmdLine) ? "" :
                                  (cmdLine.Length > 100 ? cmdLine.Substring(0, 100) + "..." : cmdLine);
                        fi.LastChange = DateTime.Now;
                        fi.Accessed = DateTime.Now;
                        fi.IsCheat = false;
                        fi.IsRunning = true;
                        AddFinding(fi);
                    }
                    catch { }
                }
                LogUI("[OK] Tasklist: " + flagged + " suspicious process(es)", flagged > 0 ? Theme.MC_Red : Theme.MC_Green);
            }
        }
        catch (Exception ex) { LogUI("[!] Tasklist failed: " + ex.Message, Theme.MC_Yellow); }
    }

    List<string> FindExtraFolders()
    {
        List<string> extras = new List<string>();
        try
        {
            string temp = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Temp");
            if (Directory.Exists(temp)) extras.Add(temp);
        }
        catch { }
        try
        {
            string prefetch = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Prefetch");
            if (Directory.Exists(prefetch)) extras.Add(prefetch);
        }
        catch { }
        try
        {
            string recent = Environment.GetFolderPath(Environment.SpecialFolder.Recent);
            if (Directory.Exists(recent)) extras.Add(recent);
        }
        catch { }
        try
        {
            string appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            if (Directory.Exists(appdata))
            {
                string[] suspiciousSubs = {
                    "cheats", ".cheats", "cheat", ".cheat",
                    "clients", ".clients", "client",
                    "hacks", ".hacks", "hack",
                    "mcpvp", "pvp", "minecraft-hacks", "mc-cheats"
                };
                foreach (string s in suspiciousSubs)
                {
                    string p = Path.Combine(appdata, s);
                    if (Directory.Exists(p) && !extras.Any(e => e.Equals(p, StringComparison.OrdinalIgnoreCase)))
                        extras.Add(p);
                }
            }
        }
        catch { }
        return extras;
    }

    void TryEnableLastAccessTracking()
    {
        try
        {
            ProcessStartInfo q = new ProcessStartInfo("fsutil", "behavior query disablelastaccess")
            { RedirectStandardOutput = true, UseShellExecute = false, CreateNoWindow = true };
            using (Process p = Process.Start(q))
            {
                string output = p.StandardOutput.ReadToEnd();
                p.WaitForExit(3000);
                if (output.Contains("= 0")) { lastAccessEnabled = true; LogUI("[OK] Last-access tracking enabled", Theme.MC_Green); return; }
            }
            ProcessStartInfo s = new ProcessStartInfo("fsutil", "behavior set disablelastaccess 0")
            { UseShellExecute = false, CreateNoWindow = true, Verb = "runas" };
            using (Process p = Process.Start(s))
            {
                p.WaitForExit(5000);
                if (p.ExitCode == 0) { lastAccessEnabled = true; LogUI("[OK] Enabled last-access tracking", Theme.MC_Green); }
            }
        }
        catch { LogUI("[!] Cannot enable last-access (need admin)", Theme.MC_Yellow); }
    }

    void StartScan()
    {
        cancelRequested = false; currentFindings.Clear(); totalCheats = 0; totalSuspicious = 0;
        logBox.Clear(); progress.SetPercent(0);
        btnCancel.Enabled = true; btnCancel.Text = "CANCEL";
        scanStartTime = DateTime.Now;
        ShowScan();
        scanThread = new Thread(new ThreadStart(ScanWorker)) { IsBackground = true };
        scanThread.Start();
    }

    void ScanWorker()
    {
        try
        {
            string appDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MahanClient");
            try { Directory.CreateDirectory(appDir); } catch { }
            historyPath = Path.Combine(appDir, "history.txt");
            try { logWriter = new StreamWriter(Path.Combine(appDir, "scan.log"), false, Encoding.UTF8); } catch { }

            LogUI(AppInfo.Name + " " + AppInfo.Version, Theme.MC_Gold);
            UpdateProgressSafe(1, "[0/8] Enabling privileges...", "Requesting admin privileges");
            EnablePrivileges();
            UpdateProgressSafe(2, "[0/8] Last-access tracking...", "fsutil");
            TryEnableLastAccessTracking();

            UpdateProgressSafe(3, "[1/8] Building signatures...", StrongSigs.Length + " sigs");
            BuildSignatures();
            LogUI("[OK] Loaded " + StrongSigs.Length + " signatures", Theme.MC_Green);
            LoadHistory();

            if (cancelRequested) { FinishScan(true); return; }

            Process[] procs = new Process[0];
            try { procs = Process.GetProcessesByName("javaw").Concat(Process.GetProcessesByName("java")).ToArray(); } catch { }

            UpdateProgressSafe(5, "[2/8] RAM scan...", procs.Length + " Java process(es)");
            if (procs.Length == 0) LogUI("[!] Minecraft not running - skipping RAM", Theme.MC_Yellow);
            else ScanRAMSafe(procs);
            if (cancelRequested) { FinishScan(true); return; }

            UpdateProgressSafe(12, "[3/8] Searching MC folders...", "Locating MC directories");
            List<string> recycleDirs = new List<string>();
            string cmdLine = procs.Length > 0 ? GetCmdLine(procs[0].Id) : null;
            List<string> folders = FindFolders(cmdLine, recycleDirs);
            LogUI("[OK] " + folders.Count + " MC folders, " + recycleDirs.Count + " recycle bins", Theme.MC_Green);

            UpdateProgressSafe(18, "[4/8] Adding extra folders...", "AppData + Temp + Prefetch + Recent");
            List<string> extraFolders = FindExtraFolders();
            foreach (string ef in extraFolders)
            {
                if (!folders.Any(f => f.Equals(ef, StringComparison.OrdinalIgnoreCase)))
                    folders.Add(ef);
                LogUI("      + Extra: " + ef, Theme.TextDim);
            }
            LogUI("[OK] Total folders to scan: " + folders.Count, Theme.MC_Green);

            if (cancelRequested) { FinishScan(true); return; }

            UpdateProgressSafe(22, "[5/8] Unhiding files...", "attrib -h -s");
            List<string> unhidePaths = new List<string>();
            try
            {
                string temp = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Temp");
                if (Directory.Exists(temp)) unhidePaths.Add(temp);
            } catch { }
            try
            {
                string recent = Environment.GetFolderPath(Environment.SpecialFolder.Recent);
                if (Directory.Exists(recent)) unhidePaths.Add(recent);
            } catch { }
            try
            {
                string prefetch = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Prefetch");
                if (Directory.Exists(prefetch)) unhidePaths.Add(prefetch);
            } catch { }
            RunUnhideAttributes(unhidePaths);

            if (cancelRequested) { FinishScan(true); return; }

            UpdateProgressSafe(25, "[6/8] Tasklist check...", "Scanning running processes");
            RunTaskList();

            if (cancelRequested) { FinishScan(true); return; }

            UpdateProgressSafe(30, "[7/8] Collecting files...", "Walking trees");
            List<string> allFiles = CollectFiles(folders, recycleDirs);
            LogUI("[OK] " + allFiles.Count + " files to inspect", Theme.MC_Green);
            if (cancelRequested) { FinishScan(true); return; }

            UpdateProgressSafe(40, "[7/8] Inspecting files...", allFiles.Count + " queued");
            InspectFiles(allFiles);
            if (cancelRequested) { FinishScan(true); return; }

            UpdateProgressSafe(70, "[8/8] USN Journal...", "Analyzing");
            ScanUSN();
            if (cancelRequested) { FinishScan(true); return; }

            UpdateProgressSafe(85, "[8/8] MFT Raw Scan...", "Recovering deleted");
            ScanMFT();
            if (cancelRequested) { FinishScan(true); return; }

            UpdateProgressSafe(98, "Finalizing...", "Saving");
            SaveHistory();
            FinishScan(false);
        }
        catch (Exception ex)
        {
            try { File.AppendAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "crash.log"), DateTime.Now + " | ScanWorker: " + ex + "\n"); } catch { }
            try { Invoke(new Action(delegate { MessageBox.Show("Scan error: " + ex.Message); })); } catch { }
            FinishScan(true);
        }
        finally { try { if (logWriter != null) { logWriter.Close(); logWriter = null; } } catch { } }
    }

    void EnablePrivileges()
    {
        string[] privs = { "SeDebugPrivilege","SeBackupPrivilege","SeRestorePrivilege","SeTakeOwnershipPrivilege","SeSecurityPrivilege" };
        IntPtr token;
        if (!OpenProcessToken(Process.GetCurrentProcess().Handle, TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, out token))
        { LogUI("[!] No admin", Theme.MC_Yellow); return; }
        int n = 0;
        try
        {
            foreach (string p in privs)
            {
                LUID l; if (!LookupPrivilegeValue(null, p, out l)) continue;
                TOKEN_PRIVILEGES tp = new TOKEN_PRIVILEGES();
                tp.PrivilegeCount = 1; tp.Privileges.Luid = l; tp.Privileges.Attributes = SE_PRIVILEGE_ENABLED;
                if (AdjustTokenPrivileges(token, false, ref tp, 0, IntPtr.Zero, IntPtr.Zero)) n++;
            }
        }
        finally { CloseHandle(token); }
        LogUI("[OK] " + n + "/" + privs.Length + " privileges", Theme.MC_Green);
    }

    class ACNode { public Dictionary<byte, ACNode> Next = new Dictionary<byte, ACNode>(); public ACNode Fail; public List<int> Output = new List<int>(); }
    ACNode acRoot; List<string> acPatterns;

    void BuildSignatures()
    {
        acPatterns = new List<string>(StrongSigs);
        acRoot = new ACNode();
        for (int i = 0; i < acPatterns.Count; i++)
        {
            ACNode cur = acRoot;
            foreach (byte b in Encoding.UTF8.GetBytes(acPatterns[i].ToLowerInvariant()))
            { ACNode nxt; if (!cur.Next.TryGetValue(b, out nxt)) { nxt = new ACNode(); cur.Next[b] = nxt; } cur = nxt; }
            cur.Output.Add(i);
        }
        Queue<ACNode> q = new Queue<ACNode>();
        foreach (ACNode ch in acRoot.Next.Values) { ch.Fail = acRoot; q.Enqueue(ch); }
        while (q.Count > 0)
        {
            ACNode cur = q.Dequeue();
            foreach (KeyValuePair<byte, ACNode> kv in cur.Next)
            {
                byte b = kv.Key; ACNode ch = kv.Value; ACNode f = cur.Fail;
                while (f != null && !f.Next.ContainsKey(b)) f = f.Fail;
                ch.Fail = (f == null) ? acRoot : (f.Next.ContainsKey(b) ? f.Next[b] : acRoot);
                ch.Output.AddRange(ch.Fail.Output); q.Enqueue(ch);
            }
        }
    }

    void SearchBytes(byte[] buf, int len, HashSet<string> found)
    {
        ACNode cur = acRoot;
        for (int i = 0; i < len; i++)
        {
            byte b = buf[i];
            if (b >= 'A' && b <= 'Z') b = (byte)(b + 32);
            while (cur != acRoot && !cur.Next.ContainsKey(b)) cur = cur.Fail;
            ACNode nxt;
            cur = cur.Next.TryGetValue(b, out nxt) ? nxt : acRoot;
            foreach (int idx in cur.Output) found.Add(acPatterns[idx]);
        }
    }

    void ScanRAMSafe(Process[] procs)
    {
        foreach (Process p in procs)
        {
           
            try
            {
                string procName = p.ProcessName.ToLowerInvariant();
                if (ProxyProcessWhitelist.Contains(procName) || ProxyProcessWhitelist.Contains(procName + ".exe"))
                {
                    LogUI("[OK] Skipping proxy process: " + p.ProcessName, Theme.TextDim);
                    continue;
                }
            }
            catch { }

            IntPtr h;
            try { h = OpenProcess(0x410, false, p.Id); } catch { continue; }
            if (h == IntPtr.Zero) { LogUI("[!] PID " + p.Id + ": denied", Theme.MC_Yellow); continue; }
            HashSet<string> found = new HashSet<string>();
            try { found = ScanRAM(h); } catch { } finally { try { CloseHandle(h); } catch { } }
            if (found.Count > 0)
            {
                LogUI("[!] PID " + p.Id + ": " + found.Count + " cheat(s)", Theme.MC_Red);
                foreach (string s in found)
                {
                    FindingInfo fi = new FindingInfo();
                    fi.FullPath = "PID " + p.Id; fi.DisplayPath = "Minecraft (PID " + p.Id + ")";
                    fi.Source = "RAM (RUNNING NOW)"; fi.Matches = new List<string> { s };
                    fi.LastChange = DateTime.Now; fi.Accessed = DateTime.Now;
                    fi.IsCheat = true; fi.IsRunning = true; AddFinding(fi);
                }
            }
            else LogUI("[OK] PID " + p.Id + ": clean", Theme.MC_Green);
        }
    }

    HashSet<string> ScanRAM(IntPtr h)
    {
        HashSet<string> found = new HashSet<string>();
        long addr = 0x10000;
        uint mbiSz = (uint)Marshal.SizeOf(typeof(MBI));
        while (true)
        {
            MBI m; int q;
            try { q = VirtualQueryEx(h, (IntPtr)addr, out m, mbiSz); } catch { break; }
            if (q == 0) break;
            long rs = (long)m.RegionSize; if (rs <= 0) break;
            uint pr = m.Protect;
            bool rd = (pr & 0x100) == 0 && pr != 0x01 && (pr & 0x66) != 0;
            bool wt = (m.Type & (0x20000 | 0x40000)) != 0;
            if (m.State == 0x1000 && rd && wt && rs > 0x1000)
            {
                long start = (long)m.BaseAddress, end = start + rs;
                List<long> chunks = new List<long>();
                for (long a = start; a < end; a += CHUNK) chunks.Add(a);
                Parallel.ForEach(chunks, ca =>
                {
                    byte[] buf = new byte[CHUNK];
                    int n = (int)Math.Min((long)CHUNK, end - ca);
                    int got;
                    try
                    {
                        if (ReadProcessMemory(h, (IntPtr)ca, buf, n, out got) && got > 0)
                        {
                            HashSet<string> loc = new HashSet<string>();
                            SearchBytes(buf, got, loc);
                            if (loc.Count > 0) lock (found) { foreach (string s in loc) found.Add(s); }
                        }
                    }
                    catch { }
                });
            }
            long next = (long)m.BaseAddress + rs;
            if (next <= addr) break;
            addr = next;
        }
        return found;
    }

    string GetCmdLine(int pid)
    {
        try
        {
            using (ManagementObjectSearcher s = new ManagementObjectSearcher("SELECT CommandLine FROM Win32_Process WHERE ProcessId = " + pid))
                foreach (ManagementObject o in s.Get())
                { object v = o["CommandLine"]; if (v != null) return v.ToString(); }
        }
        catch { }
        return null;
    }

    void AddFolderSafe(List<string> list, HashSet<string> seen, string path)
    {
        try
        {
            if (string.IsNullOrEmpty(path) || !Directory.Exists(path)) return;
            if ((File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0) return;
            path = Path.GetFullPath(path).TrimEnd('\\', '/');
            if (seen.Add(path)) list.Add(path);
        }
        catch { }
    }

    bool IsMCName(string l) { return l == ".minecraft" || l == ".fastclient" || l == ".lunarclient" || l == ".tlauncher" || l == ".modrinth" || l == ".technic" || l.Contains("minecraft"); }

    void RecurseFolders(string root, int depth, List<string> folders, HashSet<string> seen)
    {
        if (depth < 0) return;
        string[] subs;
        try { subs = Directory.GetDirectories(root); } catch { return; }
        foreach (string dir in subs)
        {
            try { if ((File.GetAttributes(dir) & FileAttributes.ReparsePoint) != 0) continue; } catch { continue; }
            string name;
            try { name = Path.GetFileName(dir); } catch { continue; }
            if (string.IsNullOrEmpty(name)) continue;
            string nl = name.ToLowerInvariant();
            if (nl.StartsWith("$") || DirSkipNames.Contains(nl)) continue;
            if (IsMCName(nl)) { AddFolderSafe(folders, seen, dir); continue; }
            RecurseFolders(dir, depth - 1, folders, seen);
        }
    }

    List<string> FindFolders(string cmdLine, List<string> recycleDirs)
    {
        List<string> folders = new List<string>();
        HashSet<string> seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        string[] names = { ".minecraft", ".fastclient", ".lunarclient", ".tlauncher", ".modrinth", ".technic" };
        try
        {
            if (!string.IsNullOrEmpty(cmdLine))
                foreach (string nm in names)
                {
                    int idx = 0;
                    while ((idx = cmdLine.IndexOf(nm, idx, StringComparison.OrdinalIgnoreCase)) >= 0)
                    {
                        int st = idx;
                        while (st > 0 && cmdLine[st - 1] != '"' && cmdLine[st - 1] != ' ' && cmdLine[st - 1] != ';' && cmdLine[st - 1] != '=') st--;
                        AddFolderSafe(folders, seen, (cmdLine.Substring(st, idx - st) + nm).Trim('"', ' ', '\''));
                        idx += nm.Length;
                    }
                }
        }
        catch { }
        string[] bases = {
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
        };
        foreach (string b in bases)
            if (!string.IsNullOrEmpty(b))
                foreach (string nm in names) AddFolderSafe(folders, seen, Path.Combine(b, nm));
        try
        {
            string up = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            foreach (string sub in new[] { "Downloads", "Games", "Game" })
                foreach (string nm in names) AddFolderSafe(folders, seen, Path.Combine(up, sub, nm));
        }
        catch { }
        try
        {
            foreach (DriveInfo d in DriveInfo.GetDrives())
            {
                try
                {
                    if (d.DriveType != DriveType.Fixed && d.DriveType != DriveType.Removable) continue;
                    if (!d.IsReady) continue;
                    RecurseFolders(d.RootDirectory.FullName, MAX_DEPTH, folders, seen);
                }
                catch { }
            }
        }
        catch { }
        try
        {
            foreach (DriveInfo d in DriveInfo.GetDrives())
            {
                try
                {
                    if (d.DriveType != DriveType.Fixed && d.DriveType != DriveType.Removable) continue;
                    if (!d.IsReady) continue;
                    string p = Path.Combine(d.RootDirectory.FullName, "$Recycle.Bin");
                    if (!Directory.Exists(p)) continue;
                    foreach (string sid in Directory.GetDirectories(p))
                        if (Path.GetFileName(sid).StartsWith("S-1-")) recycleDirs.Add(sid);
                }
                catch { }
            }
        }
        catch { }
        return folders;
    }

    bool IsTargetExt(string n)
    {
        if (string.IsNullOrEmpty(n)) return false;
        string l; try { l = n.ToLowerInvariant(); } catch { return false; }
        foreach (string e in TargetExts) if (l.EndsWith(e)) return true;
        return false;
    }

    bool ShouldSkipPath(string p)
    {
        if (string.IsNullOrEmpty(p)) return false;
        foreach (string s in JarPathSkip) if (p.Contains(s)) return true;
        return false;
    }

    List<string> CollectFiles(List<string> folders, List<string> recycleDirs)
    {
        List<string> all = new List<string>();
        foreach (string folder in folders)
        {
            try
            {
                foreach (string f in Directory.EnumerateFiles(folder, "*.*", SearchOption.AllDirectories))
                {
                    if (cancelRequested) return all;
                    if (all.Count >= MAX_FILES_SAFETY) return all;
                    string lo = f.ToLowerInvariant();
                    if (ShouldSkipPath(lo)) continue;
                    if (!IsTargetExt(f)) continue;
                    string ln = Path.GetFileName(folder).TrimStart('.').ToLowerInvariant();
                    if (!string.IsNullOrEmpty(ln) && Path.GetFileName(f).ToLowerInvariant().Contains(ln)) continue;
                    all.Add(f);
                }
            }
            catch { }
        }
        foreach (string rb in recycleDirs)
        {
            try
            {
                foreach (string f in Directory.EnumerateFiles(rb, "$R*", SearchOption.AllDirectories))
                {
                    if (cancelRequested) return all;
                    if (all.Count >= MAX_FILES_SAFETY) return all;
                    if (!IsTargetExt(f)) continue;
                    string fn = Path.GetFileName(f);
                    if (!fn.StartsWith("$R") && !fn.StartsWith("$r")) continue;
                    all.Add(f);
                }
            }
            catch { }
        }
        return all;
    }

    void InspectFiles(List<string> files)
    {
        int total = files.Count, done = 0;
        ParallelOptions po = new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount };
        Parallel.ForEach(files, po, f =>
        {
            if (cancelRequested) return;
            try
            {
                string fn = Path.GetFileName(f);
                HashSet<string> cH = InspectContent(f, fn);
                List<string> nH = GetSuspicious(fn);
                List<string> sH = new List<string>();
                foreach (string h in cH) if (StrongSigs.Any(s => s.Equals(h, StringComparison.OrdinalIgnoreCase))) sH.Add(h);
                if (sH.Count > 0 || nH.Count > 0 || cH.Count > 0)
                {
                    FindingInfo fi = new FindingInfo();
                    fi.FullPath = f; fi.DisplayPath = ShortenPath(f);
                    fi.Source = f.Contains("$Recycle") ? "Recycle Bin" : "Disk";
                    fi.IsCheat = sH.Count > 0;
                    if (sH.Count > 0) fi.Matches = sH.Take(5).ToList();
                    else fi.Matches = nH.Concat(cH).Distinct().Take(5).ToList();
                    try
                    {
                        FileInfo lf = new FileInfo(f);
                        if (lf.Exists)
                        {
                            fi.Created = lf.CreationTime; fi.Modified = lf.LastWriteTime;
                            DateTime la = lf.LastAccessTime;
                            if ((DateTime.Now - la).TotalSeconds > 60) fi.Accessed = la;
                        }
                    }
                    catch { }
                    AddFinding(fi);
                }
            }
            catch { }
            int cur = Interlocked.Increment(ref done);
            if (cur % 50 == 0 || cur == total)
                UpdateProgressSafe(40 + (int)(25.0 * cur / total), "[7/8] Inspecting...", cur + "/" + total);
        });
    }

    HashSet<string> InspectContent(string path, string fn)
    {
        HashSet<string> hits = new HashSet<string>();
        string lowerPath = path.ToLowerInvariant();

 (V2Ray, Xray-core, NekoRay, Hiddify, Clash, Sing-box, ...)
        foreach (string skip in ProxyPathSkip)
        {
            if (lowerPath.Contains(skip)) return hits;
        }

        if (lowerPath.EndsWith(".jar"))
        {
            try
            {
                FileInfo fi = new FileInfo(path);
                if (fi.Length > MAX_JAR_SIZE || fi.Length < 100) return hits;
                using (ZipArchive zip = ZipFile.OpenRead(path))
                {
                    StringBuilder sb = new StringBuilder();
                    int c = 0;
                    foreach (ZipArchiveEntry e in zip.Entries)
                    {
                        c++; if (c > MAX_JAR_ENTRIES) break;
                        string en; try { en = e.FullName.Replace('\\', '/'); } catch { continue; }
                        sb.Append(en).Append('\n');
                        string el = en.ToLowerInvariant();
                        if (el == "meta-inf/manifest.mf" || el == "fabric.mod.json" || el.EndsWith("/fabric.mod.json") ||
                            el == "meta-inf/mods.toml" || el.EndsWith("/mods.toml"))
                        {
                            try
                            {
                                using (Stream s = e.Open()) using (StreamReader r = new StreamReader(s))
                                { string ct = r.ReadToEnd(); if (ct != null && ct.Length < 200000) sb.Append(ct).Append('\n'); }
                            }
                            catch { }
                        }
                    }
                    if (sb.Length > 0) { byte[] data = Encoding.UTF8.GetBytes(sb.ToString()); SearchBytes(data, data.Length, hits); }
                }
            }
            catch { }
            return hits;
        }
        bool isT = false;
        string lo = fn.ToLowerInvariant();
        foreach (string e in TextExts) if (lo.EndsWith(e)) { isT = true; break; }
        long maxR = isT ? MAX_TEXT_SIZE : MAX_BIN_READ;
        try
        {
            FileInfo fi = new FileInfo(path);
            if (fi.Length < 20 || fi.Length > maxR * 4) return hits;
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 65536))
            {
                long tr = Math.Min(fs.Length, maxR);
                byte[] buf = new byte[tr];
                int t = 0;
                while (t < tr) { int r = fs.Read(buf, t, (int)(tr - t)); if (r <= 0) break; t += r; }
                SearchBytes(buf, t, hits);
                if (isT)
                {
                    try
                    {
                        string s16 = Encoding.Unicode.GetString(buf, 0, t);
                        byte[] b16 = Encoding.UTF8.GetBytes(s16);
                        SearchBytes(b16, b16.Length, hits);
                    }
                    catch { }
                }
            }
        }
        catch { }
        return hits;
    }

    List<string> GetSuspicious(string n)
    {
        List<string> hits = new List<string>();
        if (string.IsNullOrEmpty(n)) return hits;
        string l; try { l = n.ToLowerInvariant(); } catch { return hits; }
        foreach (string kw in SuspiciousKeywords) if (l.Contains(kw)) hits.Add(kw);
        return hits;
    }

    string ShortenPath(string full)
    {
        if (string.IsNullOrEmpty(full)) return full;
        try
        {
            string[] folders = { ".minecraft", ".fastclient", ".lunarclient", ".tlauncher", ".technic", ".modrinth" };
            foreach (string folder in folders)
            {
                int idx = full.IndexOf(folder + "\\", StringComparison.OrdinalIgnoreCase);
                if (idx < 0) idx = full.IndexOf(folder + "/", StringComparison.OrdinalIgnoreCase);
                if (idx >= 0)
                {
                    string rest = full.Substring(idx + folder.Length + 1);
                    string lbl = folder == ".minecraft" ? "<MC>" : folder == ".fastclient" ? "<FC>" : folder == ".lunarclient" ? "<LC>" : folder == ".tlauncher" ? "<TL>" : "<MR>";
                    return lbl + "\\" + rest;
                }
            }
            int rb = full.IndexOf("$Recycle.Bin", StringComparison.OrdinalIgnoreCase);
            if (rb >= 0) return "<RB>" + full.Substring(rb + "$Recycle.Bin".Length);
            string temp = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Temp");
            if (full.StartsWith(temp, StringComparison.OrdinalIgnoreCase))
                return "<TEMP>" + full.Substring(temp.Length);
            string recent = Environment.GetFolderPath(Environment.SpecialFolder.Recent);
            if (full.StartsWith(recent, StringComparison.OrdinalIgnoreCase))
                return "<RECENT>" + full.Substring(recent.Length);
            if (full.IndexOf("\\Prefetch\\", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                int idx = full.IndexOf("\\Prefetch\\", StringComparison.OrdinalIgnoreCase);
                return "<PREFETCH>" + full.Substring(idx + 9);
            }
        }
        catch { }
        return full;
    }

    class UsnRec { public string Name = ""; public uint Reason; public long FRN, TimeStamp; }

    void ScanUSN()
    {
        try
        {
            foreach (DriveInfo d in DriveInfo.GetDrives())
            {
                if (cancelRequested) return;
                try
                {
                    if (d.DriveType != DriveType.Fixed && d.DriveType != DriveType.Removable) continue;
                    if (!d.IsReady) continue;
                    if (d.DriveFormat != "NTFS") continue;
                    if (d.TotalSize < 200L * 1024 * 1024) continue;
                    LogUI("[USN] " + d.RootDirectory.FullName, Theme.MC_Aqua);
                    List<UsnRec> recs = ReadUSN(d.RootDirectory.FullName);
                    LogUI("[USN] " + recs.Count + " records", Theme.TextDim);
                    if (recs.Count == 0) continue;
                    Dictionary<long, string> pending = new Dictionary<long, string>();
                    foreach (UsnRec r in recs)
                    {
                        try
                        {
                            bool isD = (r.Reason & USN_REASON_FILE_DELETE) != 0;
                            bool isO = (r.Reason & USN_REASON_RENAME_OLD_NAME) != 0;
                            bool isN = (r.Reason & USN_REASON_RENAME_NEW_NAME) != 0;
                            if (isD && IsTargetExt(r.Name))
                            {
                                List<string> hits = GetSuspicious(r.Name);
                                if (hits.Count > 0)
                                {
                                    FindingInfo fi = new FindingInfo();
                                    fi.FullPath = r.Name; fi.DisplayPath = r.Name;
                                    fi.Source = "USN (deleted)"; fi.Matches = hits;
                                    fi.LastChange = FileTime(r.TimeStamp); fi.Accessed = FileTime(r.TimeStamp);
                                    AddFinding(fi);
                                }
                            }
                            if ((isO && isN) && IsTargetExt(r.Name))
                            {
                                List<string> hits = GetSuspicious(r.Name);
                                if (hits.Count > 0)
                                {
                                    FindingInfo fi = new FindingInfo();
                                    fi.FullPath = r.Name; fi.DisplayPath = r.Name;
                                    fi.Source = "USN (renamed)"; fi.Matches = hits;
                                    fi.LastChange = FileTime(r.TimeStamp); fi.Accessed = FileTime(r.TimeStamp);
                                    AddFinding(fi);
                                }
                            }
                            if (isO && !isN) pending[r.FRN] = r.Name;
                            else if (isN && !isO && pending.ContainsKey(r.FRN))
                            {
                                string oldN = pending[r.FRN]; string newN = r.Name;
                                pending.Remove(r.FRN);
                                if (oldN != newN && !string.IsNullOrEmpty(oldN) && !string.IsNullOrEmpty(newN))
                                {
                                    List<string> oH = GetSuspicious(oldN), nH = GetSuspicious(newN);
                                    if (oH.Count > 0 || nH.Count > 0)
                                    {
                                        FindingInfo fi = new FindingInfo();
                                        fi.FullPath = newN; fi.DisplayPath = newN;
                                        fi.Source = "USN (renamed)";
                                        fi.Matches = oH.Count > 0 ? oH : nH;
                                        fi.LastChange = FileTime(r.TimeStamp); fi.Accessed = FileTime(r.TimeStamp);
                                        fi.Note = oldN + " -> " + newN;
                                        AddFinding(fi);
                                    }
                                }
                            }
                        }
                        catch { }
                    }
                }
                catch { }
            }
        }
        catch { }
    }

    List<UsnRec> ReadUSN(string root)
    {
        List<UsnRec> recs = new List<UsnRec>();
        try
        {
            string vp = "\\\\.\\" + root.TrimEnd('\\');
            SafeFileHandle h = CreateFile(vp, GENERIC_READ, FILE_SHARE_READ | FILE_SHARE_WRITE, IntPtr.Zero, OPEN_EXISTING, 0, IntPtr.Zero);
            if (h.IsInvalid) return recs;
            try
            {
                USN_JOURNAL_DATA jd = new USN_JOURNAL_DATA();
                int jdSz = Marshal.SizeOf(typeof(USN_JOURNAL_DATA));
                IntPtr jdP = Marshal.AllocHGlobal(jdSz);
                try
                {
                    uint br;
                    if (!DeviceIoControl(h, FSCTL_QUERY_USN_JOURNAL, IntPtr.Zero, 0, jdP, (uint)jdSz, out br, IntPtr.Zero))
                    {
                        if (ForceResize(root, 1024L * 1024 * 1024))
                        { if (!DeviceIoControl(h, FSCTL_QUERY_USN_JOURNAL, IntPtr.Zero, 0, jdP, (uint)jdSz, out br, IntPtr.Zero)) return recs; }
                        else return recs;
                    }
                    jd = (USN_JOURNAL_DATA)Marshal.PtrToStructure(jdP, typeof(USN_JOURNAL_DATA));
                }
                finally { Marshal.FreeHGlobal(jdP); }
                if (jd.NextUsn == 0 || jd.FirstUsn >= jd.NextUsn) return recs;
                long su = jd.FirstUsn, eu = jd.NextUsn;
                int it = 0, ssz = Marshal.SizeOf(typeof(READ_USN_JOURNAL_DATA));
                int bsz = 1024 * 1024;
                IntPtr bp = Marshal.AllocHGlobal(bsz), sp = Marshal.AllocHGlobal(ssz);
                try
                {
                    while (su < eu && it < 5000 && !cancelRequested)
                    {
                        it++;
                        READ_USN_JOURNAL_DATA rd = new READ_USN_JOURNAL_DATA();
                        rd.StartUsn = su; rd.ReasonMask = 0xFFFFFFFF;
                        rd.UsnJournalID = jd.UsnJournalID; rd.MinMajorVersion = 2; rd.MaxMajorVersion = 3;
                        Marshal.StructureToPtr(rd, sp, false);
                        uint br;
                        if (!DeviceIoControl(h, FSCTL_READ_USN_JOURNAL, sp, (uint)ssz, bp, (uint)bsz, out br, IntPtr.Zero)) break;
                        if (br <= 8) break;
                        long nu = Marshal.ReadInt64(bp);
                        if (nu <= su) break;
                        int off = 8; long brL = (long)br;
                        while (off + 8 <= brL)
                        {
                            int rl;
                            try { rl = Marshal.ReadInt32(bp, off); } catch { break; }
                            if (rl <= 0 || (long)off + rl > brL) break;
                            if (rl < 60 || off + 60 > brL) { off += Math.Max(rl, 1); continue; }
                            try
                            {
                                short mv = Marshal.ReadInt16(bp, off + 4);
                                if (mv != 2 && mv != 3) { off += rl; continue; }
                                long frn = Marshal.ReadInt64(bp, off + 8);
                                long ts = Marshal.ReadInt64(bp, off + 32);
                                uint rs = (uint)Marshal.ReadInt32(bp, off + 40);
                                short nl = Marshal.ReadInt16(bp, off + 56);
                                short no = Marshal.ReadInt16(bp, off + 58);
                                if (nl > 0 && no > 0 && nl < 1000 && (long)off + no + nl <= brL)
                                {
                                    string n = null;
                                    try { n = Marshal.PtrToStringUni((IntPtr)((long)bp + off + no), nl / 2); } catch { }
                                    if (!string.IsNullOrEmpty(n))
                                    { UsnRec r = new UsnRec(); r.Name = n; r.Reason = rs; r.FRN = frn; r.TimeStamp = ts; recs.Add(r); }
                                }
                            }
                            catch { }
                            off += rl;
                        }
                        su = nu;
                    }
                }
                finally { Marshal.FreeHGlobal(bp); Marshal.FreeHGlobal(sp); }
            }
            finally { h.Close(); }
        }
        catch { }
        return recs;
    }

    bool ForceResize(string root, long sz)
    {
        try
        {
            string letter = root.TrimEnd('\\', ':');
            string p = "\\\\.\\" + letter + ":";
            SafeFileHandle h = CreateFile(p, GENERIC_READ | GENERIC_WRITE, FILE_SHARE_READ | FILE_SHARE_WRITE, IntPtr.Zero, OPEN_EXISTING, 0, IntPtr.Zero);
            if (h.IsInvalid) return false;
            try { uint br; DeviceIoControl(h, FSCTL_DELETE_USN_JOURNAL, IntPtr.Zero, 0, IntPtr.Zero, 0, out br, IntPtr.Zero); } finally { h.Close(); }
            SafeFileHandle h2 = CreateFile(p, GENERIC_READ | GENERIC_WRITE, FILE_SHARE_READ | FILE_SHARE_WRITE, IntPtr.Zero, OPEN_EXISTING, 0, IntPtr.Zero);
            if (h2.IsInvalid) return false;
            try
            {
                byte[] cd = new byte[16];
                BitConverter.GetBytes(sz).CopyTo(cd, 0);
                BitConverter.GetBytes(64L * 1024 * 1024).CopyTo(cd, 8);
                IntPtr dp = Marshal.AllocHGlobal(16);
                try { Marshal.Copy(cd, 0, dp, 16); uint br; return DeviceIoControl(h2, FSCTL_CREATE_USN_JOURNAL, dp, 16, IntPtr.Zero, 0, out br, IntPtr.Zero); }
                finally { Marshal.FreeHGlobal(dp); }
            }
            finally { h2.Close(); }
        }
        catch { return false; }
    }

    class MftE { public string Name = ""; public long ParentFRN, TimeStamp; }

    void ScanMFT()
    {
        try
        {
            foreach (DriveInfo d in DriveInfo.GetDrives())
            {
                if (cancelRequested) return;
                try
                {
                    if (d.DriveType != DriveType.Fixed) continue;
                    if (!d.IsReady) continue;
                    if (d.DriveFormat != "NTFS") continue;
                    if (d.TotalSize < 200L * 1024 * 1024) continue;
                    LogUI("[MFT] " + d.RootDirectory.FullName, Theme.MC_Aqua);
                    ScanMFTDrive(d.RootDirectory.FullName);
                }
                catch { }
            }
        }
        catch { }
    }

    void ScanMFTDrive(string root)
    {
        try
        {
            string vp = "\\\\.\\" + root.TrimEnd('\\');
            SafeFileHandle h = CreateFile(vp, GENERIC_READ, FILE_SHARE_READ | FILE_SHARE_WRITE, IntPtr.Zero, OPEN_EXISTING, 0, IntPtr.Zero);
            if (h.IsInvalid) { LogUI("[MFT] Cannot open volume", Theme.MC_Yellow); return; }
            try
            {
                USN_JOURNAL_DATA jd = new USN_JOURNAL_DATA();
                int jds = Marshal.SizeOf(typeof(USN_JOURNAL_DATA));
                IntPtr jdp = Marshal.AllocHGlobal(jds);
                try
                {
                    uint br;
                    if (!DeviceIoControl(h, FSCTL_QUERY_USN_JOURNAL, IntPtr.Zero, 0, jdp, (uint)jds, out br, IntPtr.Zero)) return;
                    jd = (USN_JOURNAL_DATA)Marshal.PtrToStructure(jdp, typeof(USN_JOURNAL_DATA));
                }
                finally { Marshal.FreeHGlobal(jdp); }
                MFT_ENUM_DATA md = new MFT_ENUM_DATA();
                md.StartFileReferenceNumber = 0; md.LowUsn = 0; md.HighUsn = long.MaxValue;
                int msz = Marshal.SizeOf(typeof(MFT_ENUM_DATA));
                IntPtr mp = Marshal.AllocHGlobal(msz);
                int bsz = 4 * 1024 * 1024;
                IntPtr bp = Marshal.AllocHGlobal(bsz);
                Dictionary<long, MftE> entries = new Dictionary<long, MftE>();
                List<long> susp = new List<long>();
                int total = 0, batches = 0;
                DateTime start = DateTime.Now;
                try
                {
                    while (total < MAX_MFT_RECORDS && !cancelRequested)
                    {
                        Marshal.StructureToPtr(md, mp, false);
                        uint br;
                        if (!DeviceIoControl(h, FSCTL_ENUM_USN_DATA, mp, (uint)msz, bp, (uint)bsz, out br, IntPtr.Zero)) break;
                        if (br <= 8) break;
                        batches++;
                        long next = Marshal.ReadInt64(bp);
                        int off = 8; long brL = (long)br;
                        while (off + 8 <= brL)
                        {
                            int rl;
                            try { rl = Marshal.ReadInt32(bp, off); } catch { break; }
                            if (rl <= 0 || (long)off + rl > brL) break;
                            if (rl < 60 || off + 60 > brL) { off += Math.Max(rl, 1); continue; }
                            try
                            {
                                short mv = Marshal.ReadInt16(bp, off + 4);
                                if (mv != 2) { off += rl; continue; }
                                long frn = Marshal.ReadInt64(bp, off + 8);
                                long pfrn = Marshal.ReadInt64(bp, off + 16);
                                long ts = Marshal.ReadInt64(bp, off + 32);
                                short nl = Marshal.ReadInt16(bp, off + 56);
                                short no = Marshal.ReadInt16(bp, off + 58);
                                if (nl > 0 && no > 0 && nl < 1000 && (long)off + no + nl <= brL)
                                {
                                    string n = null;
                                    try { n = Marshal.PtrToStringUni((IntPtr)((long)bp + off + no), nl / 2); } catch { }
                                    if (!string.IsNullOrEmpty(n))
                                    {
                                        MftE e = new MftE(); e.Name = n; e.ParentFRN = pfrn; e.TimeStamp = ts;
                                        entries[frn] = e; total++;
                                        string ln = n.ToLowerInvariant();
                                        if (ln.EndsWith(".jar") || GetSuspicious(n).Count > 0) susp.Add(frn);
                                    }
                                }
                            }
                            catch { }
                            off += rl;
                        }
                        md.StartFileReferenceNumber = (ulong)next;
                        if (next == 0) break;
                        if ((DateTime.Now - start).TotalSeconds > 300) break;
                        if (batches % 20 == 0) UpdateProgressSafe(85, "[8/8] MFT scan...", total + " records");
                    }
                }
                finally { Marshal.FreeHGlobal(bp); Marshal.FreeHGlobal(mp); }
                LogUI("[MFT] " + total + " records, " + susp.Count + " candidates", Theme.TextDim);
                int checked_ = 0, deleted = 0;
                foreach (long frn in susp)
                {
                    if (cancelRequested) return;
                    checked_++;
                    if (checked_ % 200 == 0) UpdateProgressSafe(90, "[8/8] MFT checking...", checked_ + "/" + susp.Count);
                    try
                    {
                        MftE e;
                        if (!entries.TryGetValue(frn, out e)) continue;
                        if (IsFileDeleted(h, frn) != true) continue;
                        List<string> hits = GetSuspicious(e.Name);
                        bool isJar = e.Name.ToLowerInvariant().EndsWith(".jar");
                        if (hits.Count == 0 && !isJar) continue;
                        deleted++;
                        string path = e.Name;
                        try { string r = ResolveMft(frn, entries); if (!string.IsNullOrEmpty(r)) path = r + "\\" + e.Name; } catch { }
                        FindingInfo fi = new FindingInfo();
                        fi.FullPath = path; fi.DisplayPath = path;
                        fi.Source = "MFT (deleted)";
                        fi.Matches = hits.Count > 0 ? hits : new List<string> { "(.jar file)" };
                        fi.LastChange = FileTime(e.TimeStamp); fi.Accessed = FileTime(e.TimeStamp);
                        fi.IsCheat = hits.Count > 0;
                        AddFinding(fi);
                    }
                    catch { }
                }
                LogUI("[MFT] Deleted suspicious: " + deleted, Theme.TextDim);
            }
            finally { h.Close(); }
        }
        catch { }
    }

    string ResolveMft(long frn, Dictionary<long, MftE> entries)
    {
        try
        {
            List<string> parts = new List<string>();
            long cur = frn; int depth = 0;
            while (cur != 5 && cur != 0 && depth < 64)
            {
                MftE e; if (!entries.TryGetValue(cur, out e)) return null;
                if (cur != frn && !string.IsNullOrEmpty(e.Name)) parts.Insert(0, e.Name);
                long next = e.ParentFRN; if (next == cur) break;
                cur = next; depth++;
            }
            if (cur != 5 && cur != 0) return null;
            return string.Join("\\", parts.ToArray());
        }
        catch { return null; }
    }

    bool? IsFileDeleted(SafeFileHandle h, long frn)
    {
        try
        {
            IntPtr ib = Marshal.AllocHGlobal(8), ob = Marshal.AllocHGlobal(4096 + 16);
            try
            {
                Marshal.WriteInt64(ib, frn);
                uint br;
                if (!DeviceIoControl(h, FSCTL_GET_NTFS_FILE_RECORD, ib, 8, ob, 4096 + 16, out br, IntPtr.Zero)) return null;
                if (br < 16) return null;
                uint ro = (uint)Marshal.ReadInt32(ob, 12);
                if (ro < 16 || ro + 48 > br) return null;
                short fl = Marshal.ReadInt16(ob, (int)ro + 22);
                return (fl & 0x01) == 0;
            }
            finally { Marshal.FreeHGlobal(ib); Marshal.FreeHGlobal(ob); }
        }
        catch { return null; }
    }

    DateTime? FileTime(long ft) { if (ft <= 0) return null; try { return DateTime.FromFileTimeUtc(ft).ToLocalTime(); } catch { return null; } }

    void LoadHistory()
    {
        try
        {
            if (File.Exists(historyPath))
                foreach (string line in File.ReadAllLines(historyPath))
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    int idx = line.IndexOf(" | ");
                    if (idx > 0) historySeen.Add(line.Substring(idx + 3));
                }
        }
        catch { }
    }

    void SaveHistory()
    {
        try
        {
            List<string> newLines = new List<string>();
            foreach (FindingInfo fi in currentFindings)
            {
                string key = fi.Source + ": " + fi.DisplayPath;
                if (historySeen.Contains(key)) continue;
                historySeen.Add(key);
                newLines.Add(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " | " + key);
            }
            if (newLines.Count > 0) File.AppendAllLines(historyPath, newLines, Encoding.UTF8);
        }
        catch { }
    }

    void FinishScan(bool cancelled)
    {
        if (IsDisposed || !IsHandleCreated) return;
        try
        {
            BeginInvoke(new Action(delegate
            {
                progress.SetPercent(100);
                lblStep.Text = cancelled ? "Cancelled" : "Done";
                lblStatus.Text = "Rendering";
                ShowResultsPanel(cancelled);

                if (!cancelled && totalCheats > 0)
                {
                    int running = 0;
                    foreach (FindingInfo fi in currentFindings)
                        if (fi.IsCheat && fi.IsRunning) running++;

                    if (running > 0)
                    {
                        DialogResult r = MessageBox.Show(
                            running + " cheat(s) are RUNNING NOW in memory!\n\n" +
                            "Do you want to open Task Manager to inspect them?",
                            "Mahan Client - Live Cheats Detected",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Warning);
                        if (r == DialogResult.Yes)
                            OpenTaskManager();
                    }
                }
            }));
        }
        catch { }
    }

    DateTime? GetLastRunTime(FindingInfo fi)
    {
        if (fi.IsRunning) return DateTime.Now;
        DateTime? acc = fi.Accessed;
        if (acc != null && (DateTime.Now - acc.Value).TotalSeconds < 60) acc = null;
        if (acc != null && fi.Modified != null && acc > fi.Modified) return acc;
        if (fi.LastChange != null) return fi.LastChange;
        if (acc != null) return acc;
        if (fi.Modified != null) return fi.Modified;
        return fi.Created;
    }

    void ShowResultsPanel(bool cancelled)
    {
        try
        {
            resultsBox.Clear();
            if (cancelled) { lblVerdict.Text = "~ CANCELLED ~"; lblVerdict.ForeColor = Theme.MC_Yellow; }
            else if (totalCheats > 0) { lblVerdict.Text = "! CHEATS DETECTED !"; lblVerdict.ForeColor = Theme.MC_Red; }
            else if (totalSuspicious > 0) { lblVerdict.Text = "? SUSPICIOUS ITEMS ?"; lblVerdict.ForeColor = Theme.MC_Yellow; }
            else { lblVerdict.Text = "~ CLEAN ~"; lblVerdict.ForeColor = Theme.MC_Green; }

            AppendLine("================================================================", Theme.MC_Aqua, true);
            AppendLine("                     SUMMARY", Theme.MC_Aqua, true);
            AppendLine("================================================================", Theme.MC_Aqua, true);
            AppendLine("", Theme.Text, false);
            AppendLine("  Confirmed cheats:  " + totalCheats, totalCheats > 0 ? Theme.MC_Red : Theme.MC_Green, true);
            AppendLine("  Suspicious items:  " + totalSuspicious, totalSuspicious > 0 ? Theme.MC_Yellow : Theme.MC_Green, true);
            AppendLine("  Total findings:    " + currentFindings.Count, Color.White, true);
            AppendLine("  Scan duration:     " + (DateTime.Now - scanStartTime).TotalSeconds.ToString("F1") + "s", Theme.TextDim, false);
            AppendLine("", Theme.Text, false);

            var cheats = currentFindings.Where(f => f.IsCheat).OrderByDescending(f => GetLastRunTime(f) ?? DateTime.MinValue).ToList();
            var susp = currentFindings.Where(f => !f.IsCheat).OrderByDescending(f => GetLastRunTime(f) ?? DateTime.MinValue).ToList();

            if (cheats.Count > 0)
            {
                AppendLine("----------------------------------------------------------------", Theme.MC_Red, true);
                AppendLine("  CONFIRMED CHEATS (" + cheats.Count + ")", Theme.MC_Red, true);
                AppendLine("----------------------------------------------------------------", Theme.MC_Red, true);
                for (int i = 0; i < cheats.Count; i++) AppendFinding(cheats[i], i + 1);
            }

            if (susp.Count > 0)
            {
                AppendLine("", Theme.Text, false);
                AppendLine("----------------------------------------------------------------", Theme.MC_Yellow, true);
                AppendLine("  SUSPICIOUS ITEMS (" + susp.Count + ")", Theme.MC_Yellow, true);
                AppendLine("----------------------------------------------------------------", Theme.MC_Yellow, true);
                for (int i = 0; i < susp.Count; i++) AppendFinding(susp[i], i + 1);
            }

            if (currentFindings.Count == 0)
            {
                AppendLine("", Theme.Text, false);
                AppendLine("  No threats detected. System appears clean.", Theme.MC_Green, true);
            }

            if (btnTaskMgr != null) btnTaskMgr.Visible = (totalCheats > 0);
            if (btnFileLoc != null) btnFileLoc.Visible = (totalCheats > 0);

            ShowResults();
        }
        catch { }
    }

    void AppendFinding(FindingInfo fi, int idx)
    {
        Color border = fi.IsCheat ? Theme.MC_Red : Theme.MC_Yellow;
        Color title = fi.IsCheat ? Theme.MC_Red : Theme.MC_Yellow;
        AppendLine("---------------------------------------------------------------", border, true);
        string header = "  " + (fi.IsCheat ? "! CHEAT" : "? SUSPICIOUS") + " #" + idx + "  --  " + fi.Source;
        if (fi.IsRunning) header += "  [RUNNING NOW]";
        AppendLine(header, title, true);
        AppendLine("---------------------------------------------------------------", border, true);
        AppendLine("  Location:  " + Truncate(fi.DisplayPath, 55), Theme.MC_Aqua, false);
        DateTime? lastRun = GetLastRunTime(fi);
        if (fi.IsRunning) AppendLine("  Last RUN:  * RIGHT NOW *", Theme.MC_Red, true);
        else if (lastRun != null)
        {
            Color rc = (DateTime.Now - lastRun.Value).TotalDays < 1 ? Theme.MC_Gold : Theme.MC_Green;
            AppendLine("  Last RUN:  " + FormatTime(lastRun) + "  (" + FormatRel(lastRun) + ")", rc, true);
        }
        if (fi.Matches.Count > 0)
        {
            string m = string.Join(", ", fi.Matches.Take(3).ToArray());
            if (fi.Matches.Count > 3) m += " +" + (fi.Matches.Count - 3);
            AppendLine("  Matched:   " + Truncate(m, 55), fi.IsCheat ? Theme.MC_Red : Theme.MC_Yellow, false);
        }
        if (fi.Modified != null) AppendLine("  Modified:  " + FormatTime(fi.Modified) + "  (" + FormatRel(fi.Modified) + ")", Theme.TextDim, false);
        if (fi.Created != null) AppendLine("  Installed: " + FormatTime(fi.Created) + "  (" + FormatRel(fi.Created) + ")", Theme.TextDim, false);
        if (!string.IsNullOrEmpty(fi.Note)) AppendLine("  Note:      " + Truncate(fi.Note, 55), Theme.MC_Aqua, false);
        if (!string.IsNullOrEmpty(fi.FullPath) && fi.FullPath != fi.DisplayPath)
            AppendLine("  Full:      " + Truncate(fi.FullPath, 55), Theme.TextDim, false);
        AppendLine("", Theme.Text, false);
    }

    void AppendLine(string text, Color color, bool bold)
    {
        resultsBox.SelectionStart = resultsBox.TextLength;
        resultsBox.SelectionLength = 0;
        resultsBox.SelectionColor = color;
        resultsBox.SelectionFont = new Font(Theme.Mono.FontFamily, 10f, bold ? FontStyle.Bold : FontStyle.Regular);
        resultsBox.AppendText(text + "\n");
    }

    string Truncate(string s, int max) { if (string.IsNullOrEmpty(s)) return ""; if (s.Length <= max) return s; if (max < 4) return s.Substring(0, max); return s.Substring(0, max - 3) + "..."; }
    string FormatTime(DateTime? dt) { if (dt == null) return "unknown"; try { return dt.Value.ToString("yyyy-MM-dd HH:mm"); } catch { return "unknown"; } }
    string FormatRel(DateTime? dt)
    {
        if (dt == null) return "";
        try
        {
            TimeSpan d = DateTime.Now - dt.Value;
            if (d.TotalSeconds < 0) return "future";
            if (d.TotalSeconds < 60) return "just now";
            if (d.TotalMinutes < 60) return (int)d.TotalMinutes + " min ago";
            if (d.TotalHours < 24) return (int)d.TotalHours + " hours ago";
            if (d.TotalDays < 30) return (int)d.TotalDays + " days ago";
            if (d.TotalDays < 365) return (int)(d.TotalDays / 30) + " months ago";
            return (int)(d.TotalDays / 365) + " years ago";
        }
        catch { return ""; }
    }

    void OpenTaskManager()
    {
        try
        {
            List<string> pids = new List<string>();
            foreach (FindingInfo fi in currentFindings)
            {
                if (!fi.IsCheat) continue;
                if (fi.IsRunning && !string.IsNullOrEmpty(fi.FullPath) && fi.FullPath.StartsWith("PID "))
                {
                    string pidStr = fi.FullPath.Substring(4).Trim();
                    int pid;
                    if (int.TryParse(pidStr, out pid)) pids.Add(pid.ToString());
                }
            }

            try { Process.Start("taskmgr.exe"); } catch { }

            if (pids.Count > 0)
            {
                string pidList = string.Join(",", pids.ToArray());
                string cmd = "-NoExit -Command \"Write-Host 'Suspicious processes detected by Mahan Client:' -ForegroundColor Yellow; Get-Process | Where-Object {$_.Id -in @(" + pidList + ")} | Format-Table Id,ProcessName,Path,StartTime,Company,Description -AutoSize\"";
                try { Process.Start("powershell", cmd); } catch { }
            }
            else
            {
                string cmd = "-NoExit -Command \"Get-Process | Where-Object {$_.ProcessName -match 'wurst|meteor|vape|liquid|impact'} | Format-Table Id,ProcessName,Path,Company -AutoSize\"";
                try { Process.Start("powershell", cmd); } catch { }
            }
        }
        catch (Exception ex) { MessageBox.Show("Cannot open Task Manager: " + ex.Message); }
    }

    void OpenFirstCheatLocation()
    {
        try
        {
            FindingInfo target = null;
            foreach (FindingInfo fi in currentFindings)
            {
                if (!fi.IsCheat) continue;
                if (fi.IsRunning) continue;
                if (string.IsNullOrEmpty(fi.FullPath)) continue;
                if (fi.FullPath.StartsWith("PID ")) continue;
                target = fi;
                break;
            }

            if (target == null)
            {
                MessageBox.Show("No file-based cheat found (only RAM detection).\n\nUse Task Manager button instead.");
                return;
            }

            string path = target.FullPath;

            if (File.Exists(path))
            {
                Process.Start("explorer.exe", "/select,\"" + path + "\"");
            }
            else if (Directory.Exists(path))
            {
                Process.Start("explorer.exe", "\"" + path + "\"");
            }
            else
            {
                try
                {
                    string parent = Path.GetDirectoryName(path);
                    if (!string.IsNullOrEmpty(parent) && Directory.Exists(parent))
                        Process.Start("explorer.exe", "\"" + parent + "\"");
                    else
                        MessageBox.Show("File not found:\n" + path);
                }
                catch { MessageBox.Show("Cannot open: " + path); }
            }
        }
        catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
    }

    void SaveReport()
    {
        try
        {
            SaveFileDialog sfd = new SaveFileDialog { Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*", FileName = "mahan_scan_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".txt" };
            if (sfd.ShowDialog() != DialogResult.OK) return;
            File.WriteAllText(sfd.FileName, resultsBox.Text, Encoding.UTF8);
            MessageBox.Show("Saved:\n" + sfd.FileName);
        }
        catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
    }
}

using SimplePlanes2ModManager.Bridge;
using SimplePlanes2ModManager.Services;
using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace SimplePlanes2ModManager
{
    internal sealed class MainForm : Form
    {
        private const int MinimumWindowWidth = 980;
        private const int MinimumWindowHeight = 640;
        private readonly WebBrowser _browser;
        private readonly ManagerSettingsService _settingsService;
        private readonly GameDirectoryService _gameDirectoryService;
        private readonly BepInExService _bepInExService;
        private readonly PluginService _pluginService;
        private readonly PluginInstallRecordService _pluginInstallRecordService;
        private readonly RemotePluginService _remotePluginService;

        public MainForm()
        {
            Text = "SimplePlanes 2 Mod Manager";
            AutoScaleMode = AutoScaleMode.Dpi;
            MinimumSize = ScaleSizeForDpi(MinimumWindowWidth, MinimumWindowHeight);
            Size = ScaleSizeForDpi(1120, 720);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(13, 24, 38);

            _settingsService = new ManagerSettingsService();
            _gameDirectoryService = new GameDirectoryService(_settingsService);
            _bepInExService = new BepInExService(_gameDirectoryService);
            _pluginInstallRecordService = new PluginInstallRecordService();
            _pluginService = new PluginService(_gameDirectoryService, _pluginInstallRecordService);
            _remotePluginService = new RemotePluginService(_pluginService, _pluginInstallRecordService);

            _browser = new WebBrowser();
            _browser.Dock = DockStyle.Fill;
            _browser.ScriptErrorsSuppressed = true;
            _browser.AllowWebBrowserDrop = false;
            _browser.IsWebBrowserContextMenuEnabled = false;
            _browser.WebBrowserShortcutsEnabled = true;
            _browser.ObjectForScripting = new WebBridge(
                this,
                _settingsService,
                _gameDirectoryService,
                _bepInExService,
                _pluginService,
                _remotePluginService);

            Controls.Add(_browser);
            Load += OnLoad;
        }

        private static Size ScaleSizeForDpi(int width, int height)
        {
            using (Graphics graphics = Graphics.FromHwnd(IntPtr.Zero))
            {
                float scale = Math.Max(1f, graphics.DpiX / 96f);
                return new Size(
                    Math.Max(width, (int)Math.Round(width * scale)),
                    Math.Max(height, (int)Math.Round(height * scale)));
            }
        }

        private void OnLoad(object sender, EventArgs eventArgs)
        {
            _browser.DocumentText = BuildHtmlDocument();
        }

        private static string BuildHtmlDocument()
        {
            string html = ReadEmbeddedText("SimplePlanes2ModManager.Web.index.html");
            string css = ReadEmbeddedText("SimplePlanes2ModManager.Web.app.css");
            string script = ReadEmbeddedText("SimplePlanes2ModManager.Web.app.js");

            return html
                .Replace("/* {{APP_CSS}} */", css)
                .Replace("// {{APP_JS}}", script);
        }

        private static string ReadEmbeddedText(string resourceName)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                {
                    return string.Empty;
                }

                using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}

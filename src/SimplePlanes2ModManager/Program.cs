using Microsoft.Win32;
using System;
using System.Windows.Forms;

namespace SimplePlanes2ModManager
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            EnableModernBrowserMode();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }

        private static void EnableModernBrowserMode()
        {
            try
            {
                string executableName = System.IO.Path.GetFileName(Application.ExecutablePath);
                using (RegistryKey featureKey = Registry.CurrentUser.CreateSubKey(
                    @"Software\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION"))
                {
                    if (featureKey != null && !string.IsNullOrEmpty(executableName))
                    {
                        featureKey.SetValue(executableName, 11001, RegistryValueKind.DWord);
                    }
                }
            }
            catch
            {
                // Browser emulation is a UI compatibility hint. The manager can still run without it.
            }
        }
    }
}

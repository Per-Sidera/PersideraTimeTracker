using System;
using System.Globalization;
using PersideraTimeTracker.Form;
using PersideraTimeTracker.Properties;

namespace PersideraTimeTracker
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            if (Settings.Default.language != "none")
            {
                try
                {
                    CultureInfo.DefaultThreadCurrentCulture = CultureInfo.GetCultureInfo(Settings.Default.language);
                    CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.GetCultureInfo(Settings.Default.language);
                }
                catch (CultureNotFoundException)
                {
                    System.Windows.Forms.MessageBox.Show(string.Format("Error: invalid culture (language) '{0}'", Settings.Default.language), "Critical error!", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Stop);
                }
            }

            System.Windows.Forms.Application.EnableVisualStyles();
            System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);

            // .NET dark-mode hint: paints native chrome (title bar, scrollbars,
            // menu rendering) dark to match the Persidera brand theme.
            try
            {
                System.Windows.Forms.Application.SetColorMode(System.Windows.Forms.SystemColorMode.Dark);
            }
            catch
            {
                // SetColorMode is best-effort; ignore if unsupported at runtime.
            }

            System.Windows.Forms.Application.Run(new PersideraTimeTracker.Form.Application());
        }
    }
}
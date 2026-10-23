using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;

namespace PrintScreenAplikace
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            bool firstInstance;
            using (var mutex = new Mutex(true, @"Local\PrintScreenAplikace", out firstInstance))
            {
                if (!firstInstance) return;
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new TrayApp());
            }
        }
    }

    sealed class TrayApp : ApplicationContext
    {
        const string SettingsPath = @"Software\PrintScreenAplikace";
        const string RunPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
        const string RunName = "PrintScreenAplikace";
        const int PaintId = 1;
        const int ClipboardId = 2;
        const int WmHotkey = 0x0312;
        const uint ModAlt = 0x0001;
        const uint ModControl = 0x0002;
        const uint ModShift = 0x0004;
        const uint ModNoRepeat = 0x4000;

        readonly NotifyIcon tray;
        readonly Icon appIcon;
        readonly HotkeyWindow window;
        readonly ToolStripMenuItem settingsItem;
        readonly ToolStripMenuItem startupItem;
        readonly ToolStripMenuItem languageItem;
        readonly ToolStripMenuItem czechItem;
        readonly ToolStripMenuItem englishItem;
        readonly ToolStripMenuItem exitItem;
        Keys paintKey = Keys.Control | Keys.W;
        Keys clipboardKey = Keys.Control | Keys.Shift | Keys.W;
        bool english;
        bool capturing;

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool RegisterHotKey(IntPtr handle, int id, uint modifiers, uint key);

        [DllImport("user32.dll")]
        static extern bool UnregisterHotKey(IntPtr handle, int id);

        public TrayApp()
        {
            using (var key = Registry.CurrentUser.CreateSubKey(SettingsPath))
            {
                paintKey = ReadKey(key, "PaintHotkey", paintKey);
                clipboardKey = ReadKey(key, "ClipboardHotkey", clipboardKey);
                english = string.Equals(Convert.ToString(key.GetValue("Language", "en")), "en", StringComparison.OrdinalIgnoreCase);
            }

            window = new HotkeyWindow(OnHotkey);
            var menu = new ContextMenuStrip();
            settingsItem = new ToolStripMenuItem();
            settingsItem.Click += delegate { ShowSettings(); };
            menu.Items.Add(settingsItem);
            startupItem = new ToolStripMenuItem();
            startupItem.Click += delegate { SetStartup(!startupItem.Checked); };
            menu.Items.Add(startupItem);
            languageItem = new ToolStripMenuItem();
            czechItem = new ToolStripMenuItem("Čeština");
            czechItem.Click += delegate { SetLanguage(false); };
            englishItem = new ToolStripMenuItem("English");
            englishItem.Click += delegate { SetLanguage(true); };
            languageItem.DropDownItems.Add(czechItem);
            languageItem.DropDownItems.Add(englishItem);
            menu.Items.Add(languageItem);
            menu.Items.Add(new ToolStripSeparator());
            exitItem = new ToolStripMenuItem();
            exitItem.Click += delegate { ExitThread(); };
            menu.Items.Add(exitItem);
            tray = new NotifyIcon();
            appIcon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            tray.Icon = appIcon ?? SystemIcons.Application;
            tray.ContextMenuStrip = menu;
            ApplyLanguage();
            tray.Visible = true;

            try
            {
                using (var key = Registry.CurrentUser.CreateSubKey(SettingsPath))
                    SetStartup(Convert.ToInt32(key.GetValue("StartAtLogin", 1)) != 0);
            }
            catch (Exception ex) { ShowError(T("Nastavení spouštění se nezdařilo", "Could not configure startup"), ex); }

            if (!RegisterPair(paintKey, clipboardKey))
                ShowError(T("Klávesové zkratky jsou obsazené. Změňte je v nastavení.",
                    "The keyboard shortcuts are unavailable. Change them in Settings."));
        }

        string T(string czech, string englishText)
        {
            return english ? englishText : czech;
        }

        void ApplyLanguage()
        {
            tray.Text = T("Výřezy obrazovky", "Screen Snips");
            settingsItem.Text = T("Nastavení zkratek…", "Shortcut settings…");
            startupItem.Text = T("Spouštět po přihlášení", "Start when I sign in");
            languageItem.Text = T("Jazyk", "Language");
            exitItem.Text = T("Ukončit", "Exit");
            czechItem.Checked = !english;
            englishItem.Checked = english;
        }

        void SetLanguage(bool useEnglish)
        {
            english = useEnglish;
            ApplyLanguage();
            try
            {
                using (var key = Registry.CurrentUser.CreateSubKey(SettingsPath))
                    key.SetValue("Language", english ? "en" : "cs", RegistryValueKind.String);
            }
            catch (Exception ex) { ShowError(T("Uložení jazyka se nezdařilo", "Could not save the language"), ex); }
        }

        static Keys ReadKey(RegistryKey key, string name, Keys fallback)
        {
            try
            {
                var value = (Keys)Convert.ToInt32(key.GetValue(name, (int)fallback));
                if (SettingsDialog.ValidKey(value)) return value;
            }
            catch (Exception) { }
            return fallback;
        }

        static uint Modifiers(Keys key)
        {
            uint result = ModNoRepeat;
            if ((key & Keys.Control) != 0) result |= ModControl;
            if ((key & Keys.Shift) != 0) result |= ModShift;
            if ((key & Keys.Alt) != 0) result |= ModAlt;
            return result;
        }

        bool RegisterPair(Keys paint, Keys clipboard)
        {
            if (!RegisterHotKey(window.Handle, PaintId, Modifiers(paint), (uint)(paint & Keys.KeyCode)))
                return false;
            if (RegisterHotKey(window.Handle, ClipboardId, Modifiers(clipboard), (uint)(clipboard & Keys.KeyCode)))
                return true;
            UnregisterHotKey(window.Handle, PaintId);
            return false;
        }

        void ShowSettings()
        {
            UnregisterHotKey(window.Handle, PaintId);
            UnregisterHotKey(window.Handle, ClipboardId);
            bool registered = false;
            try
            {
            using (var dialog = new SettingsDialog(paintKey, clipboardKey, english, appIcon))
                {
                    if (dialog.ShowDialog() != DialogResult.OK) return;
                    if (!RegisterPair(dialog.PaintKey, dialog.ClipboardKey))
                    {
                        MessageBox.Show(T("Jednu ze zkratek nelze použít. Pravděpodobně ji používá jiná aplikace.",
                            "One of these shortcuts is unavailable. Another app may be using it."),
                            T("Výřezy obrazovky", "Screen Snips"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    registered = true;
                    paintKey = dialog.PaintKey;
                    clipboardKey = dialog.ClipboardKey;
                    try
                    {
                        using (var key = Registry.CurrentUser.CreateSubKey(SettingsPath))
                        {
                            key.SetValue("PaintHotkey", (int)paintKey, RegistryValueKind.DWord);
                            key.SetValue("ClipboardHotkey", (int)clipboardKey, RegistryValueKind.DWord);
                        }
                    }
                    catch (Exception ex) { ShowError(T("Uložení zkratek se nezdařilo", "Could not save the shortcuts"), ex); }
                }
            }
            finally
            {
                if (!registered && !RegisterPair(paintKey, clipboardKey))
                    ShowError(T("Původní zkratky se nepodařilo obnovit.", "Could not restore the previous shortcuts."));
            }
        }

        void SetStartup(bool enabled)
        {
            try
            {
                using (var run = Registry.CurrentUser.CreateSubKey(RunPath))
                {
                    if (enabled) run.SetValue(RunName, "\"" + Application.ExecutablePath + "\"");
                    else run.DeleteValue(RunName, false);
                }
                using (var key = Registry.CurrentUser.CreateSubKey(SettingsPath))
                    key.SetValue("StartAtLogin", enabled ? 1 : 0, RegistryValueKind.DWord);
                startupItem.Checked = enabled;
            }
            catch (Exception ex) { ShowError(T("Nastavení spouštění se nezdařilo", "Could not configure startup"), ex); }
        }

        void OnHotkey(int id)
        {
            if (capturing) return;
            capturing = true;
            try
            {
                var bounds = SystemInformation.VirtualScreen;
                using (var screen = new Bitmap(bounds.Width, bounds.Height))
                {
                    using (var graphics = Graphics.FromImage(screen))
                        graphics.CopyFromScreen(bounds.Location, Point.Empty, bounds.Size);

                    Rectangle selection;
                    using (var selector = new SelectionForm(screen, bounds))
                    {
                        if (selector.ShowDialog() != DialogResult.OK) return;
                        selection = selector.Selection;
                    }

                    using (var image = screen.Clone(selection, screen.PixelFormat))
                    {
                        if (id == ClipboardId)
                        {
                            Clipboard.SetDataObject(image, true, 3, 100);
                            tray.ShowBalloonTip(1500, T("Výřez uložen", "Snip copied"),
                                T("Obrázek je ve schránce.", "The image is on the clipboard."), ToolTipIcon.Info);
                        }
                        else
                        {
                            var directory = Path.Combine(Path.GetTempPath(), "PrintScreenAplikace");
                            Directory.CreateDirectory(directory);
                            var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss-fff");
                            var file = Path.Combine(directory, timestamp + ".png");
                            for (int suffix = 2; File.Exists(file); suffix++)
                                file = Path.Combine(directory, timestamp + "-" + suffix + ".png");
                            image.Save(file, System.Drawing.Imaging.ImageFormat.Png);
                            Process.Start(new ProcessStartInfo("mspaint.exe", "\"" + file + "\"") { UseShellExecute = false });
                        }
                    }
                }
            }
            catch (Exception ex) { ShowError(T("Pořízení výřezu se nezdařilo", "Could not capture the snip"), ex); }
            finally { capturing = false; }
        }

        void ShowError(string message)
        {
            tray.ShowBalloonTip(5000, T("Výřezy obrazovky", "Screen Snips"), message, ToolTipIcon.Error);
        }

        void ShowError(string message, Exception ex)
        {
            MessageBox.Show(message + ":\n" + ex.Message, T("Výřezy obrazovky", "Screen Snips"),
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        protected override void ExitThreadCore()
        {
            UnregisterHotKey(window.Handle, PaintId);
            UnregisterHotKey(window.Handle, ClipboardId);
            window.DestroyHandle();
            tray.Visible = false;
            tray.ContextMenuStrip.Dispose();
            tray.Dispose();
            if (appIcon != null) appIcon.Dispose();
            base.ExitThreadCore();
        }

        sealed class HotkeyWindow : NativeWindow
        {
            readonly Action<int> onHotkey;

            public HotkeyWindow(Action<int> onHotkey)
            {
                this.onHotkey = onHotkey;
                CreateHandle(new CreateParams());
            }

            protected override void WndProc(ref Message message)
            {
                if (message.Msg == WmHotkey) onHotkey(message.WParam.ToInt32());
                base.WndProc(ref message);
            }
        }
    }

    sealed class SelectionForm : Form
    {
        readonly Bitmap screen;
        Point? start;
        Point current;
        public Rectangle Selection { get; private set; }

        public SelectionForm(Bitmap screen, Rectangle bounds)
        {
            this.screen = screen;
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.Manual;
            Bounds = bounds;
            TopMost = true;
            ShowInTaskbar = false;
            DoubleBuffered = true;
            KeyPreview = true;
            Cursor = Cursors.Cross;
            BackColor = Color.Black;
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            Activate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.DrawImageUnscaled(screen, 0, 0);
            using (var shade = new SolidBrush(Color.FromArgb(100, Color.Black)))
                e.Graphics.FillRectangle(shade, ClientRectangle);
            if (!start.HasValue) return;
            var selected = Rectangle.FromLTRB(Math.Min(start.Value.X, current.X), Math.Min(start.Value.Y, current.Y),
                Math.Max(start.Value.X, current.X), Math.Max(start.Value.Y, current.Y));
            if (selected.Width == 0 || selected.Height == 0) return;
            e.Graphics.DrawImage(screen, selected, selected, GraphicsUnit.Pixel);
            using (var pen = new Pen(Color.White, 1))
                e.Graphics.DrawRectangle(pen, selected.X, selected.Y, selected.Width - 1, selected.Height - 1);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button != MouseButtons.Left) return;
            start = e.Location;
            current = e.Location;
            Capture = true;
            Invalidate();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (!start.HasValue) return;
            current = new Point(Math.Max(0, Math.Min(ClientSize.Width, e.X)), Math.Max(0, Math.Min(ClientSize.Height, e.Y)));
            Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (e.Button != MouseButtons.Left || !start.HasValue) return;
            Capture = false;
            current = new Point(Math.Max(0, Math.Min(ClientSize.Width, e.X)), Math.Max(0, Math.Min(ClientSize.Height, e.Y)));
            Selection = Rectangle.FromLTRB(Math.Min(start.Value.X, current.X), Math.Min(start.Value.Y, current.Y),
                Math.Max(start.Value.X, current.X), Math.Max(start.Value.Y, current.Y));
            if (Selection.Width > 0 && Selection.Height > 0) DialogResult = DialogResult.OK;
            else { start = null; Invalidate(); }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape) DialogResult = DialogResult.Cancel;
            base.OnKeyDown(e);
        }
    }

}

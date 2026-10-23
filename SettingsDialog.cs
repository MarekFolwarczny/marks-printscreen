using System;
using System.Drawing;
using System.Windows.Forms;

namespace PrintScreenAplikace
{
    sealed class SettingsDialog : Form
    {
        readonly bool english;
        readonly ShortcutPicker paintPicker;
        readonly ShortcutPicker clipboardPicker;
        readonly Label status;

        public Keys PaintKey { get; private set; }
        public Keys ClipboardKey { get; private set; }

        public SettingsDialog(Keys paint, Keys clipboard, bool english, Icon icon)
        {
            this.english = english;
            PaintKey = paint;
            ClipboardKey = clipboard;
            Text = T("Nastavení zkratek", "Shortcut settings");
            Icon = icon ?? SystemIcons.Application;
            Font = new Font("Segoe UI", 9F);
            ForeColor = Color.FromArgb(238, 245, 255);
            BackColor = Color.FromArgb(15, 24, 38);
            ClientSize = new Size(500, 346);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterScreen;
            MaximizeBox = false;
            MinimizeBox = false;
            AutoScaleMode = AutoScaleMode.Font;
            DoubleBuffered = true;

            var header = new Panel { Location = Point.Empty, Size = new Size(500, 80), BackColor = Color.FromArgb(24, 39, 61) };
            header.Paint += delegate(object sender, PaintEventArgs e)
            {
                e.Graphics.DrawIcon(icon ?? SystemIcons.Application, new Rectangle(18, 16, 46, 46));
            };
            header.Controls.Add(new Label
            {
                Text = T("Klávesové zkratky", "Keyboard shortcuts"),
                Location = new Point(80, 13), AutoSize = true,
                Font = new Font("Segoe UI", 13F, FontStyle.Bold), ForeColor = Color.White
            });
            header.Controls.Add(new Label
            {
                Text = T("Vyberte klávesu pro každou akci", "Choose a key for each action"),
                Location = new Point(80, 43), AutoSize = true,
                ForeColor = Color.FromArgb(173, 193, 218)
            });
            Controls.Add(header);
            Controls.Add(new Panel { Location = new Point(0, 79), Size = new Size(500, 2), BackColor = Color.FromArgb(39, 126, 245) });

            paintPicker = new ShortcutPicker(T("Otevřít v Malování", "Open in Paint"), paint, english)
            {
                Location = new Point(19, 96)
            };
            clipboardPicker = new ShortcutPicker(T("Kopírovat do schránky", "Copy to clipboard"), clipboard, english)
            {
                Location = new Point(19, 181)
            };
            Controls.Add(paintPicker);
            Controls.Add(clipboardPicker);

            status = new Label
            {
                Text = T("Vyberte Ctrl, Shift nebo Alt a klávesu.", "Choose Ctrl, Shift or Alt and a key."),
                Location = new Point(19, 270), Size = new Size(275, 52),
                ForeColor = Color.FromArgb(173, 193, 218)
            };
            Controls.Add(status);

            var save = new Button
            {
                Text = T("Uložit", "Save"), Location = new Point(310, 301), Size = new Size(80, 32),
                BackColor = Color.FromArgb(39, 126, 245), ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, UseVisualStyleBackColor = false
            };
            save.FlatAppearance.BorderSize = 0;
            save.Click += delegate
            {
                var newPaint = paintPicker.SelectedKey;
                var newClipboard = clipboardPicker.SelectedKey;
                status.ForeColor = Color.FromArgb(255, 178, 178);
                if (!ValidKey(newPaint) || !ValidKey(newClipboard))
                {
                    status.Text = T("Pro běžnou klávesu vyberte Ctrl, Shift nebo Alt.",
                        "Select Ctrl, Shift or Alt for a regular key.");
                    return;
                }
                if (newPaint == newClipboard)
                {
                    status.Text = T("Obě akce musí mít různé zkratky.", "Choose different shortcuts for the two actions.");
                    return;
                }
                PaintKey = newPaint;
                ClipboardKey = newClipboard;
                DialogResult = DialogResult.OK;
            };
            Controls.Add(save);

            var cancel = new Button
            {
                Text = T("Zrušit", "Cancel"), Location = new Point(399, 301), Size = new Size(82, 32),
                BackColor = Color.FromArgb(40, 57, 79), ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, UseVisualStyleBackColor = false,
                DialogResult = DialogResult.Cancel
            };
            cancel.FlatAppearance.BorderColor = Color.FromArgb(65, 87, 116);
            Controls.Add(cancel);
            AcceptButton = save;
            CancelButton = cancel;
        }

        string T(string czech, string englishText)
        {
            return english ? englishText : czech;
        }

        public static bool ValidKey(Keys key)
        {
            Keys code = key & Keys.KeyCode;
            return ((key & (Keys.Control | Keys.Shift | Keys.Alt)) != 0 || (code >= Keys.F1 && code <= Keys.F24)) &&
                code != Keys.None && code != Keys.ControlKey && code != Keys.ShiftKey &&
                code != Keys.Menu && code != Keys.Escape && code != Keys.Return && code != Keys.Tab;
        }
    }

    sealed class ShortcutPicker : Panel
    {
        readonly CheckBox ctrl;
        readonly CheckBox shift;
        readonly CheckBox alt;
        readonly ComboBox key;

        public Keys SelectedKey
        {
            get
            {
                var choice = key.SelectedItem as KeyChoice;
                if (choice == null) return Keys.None;
                Keys result = choice.Value;
                if (ctrl.Checked) result |= Keys.Control;
                if (shift.Checked) result |= Keys.Shift;
                if (alt.Checked) result |= Keys.Alt;
                return result;
            }
        }

        public ShortcutPicker(string title, Keys initial, bool english)
        {
            Size = new Size(462, 76);
            BackColor = Color.FromArgb(28, 43, 66);
            Controls.Add(new Panel { Location = Point.Empty, Size = new Size(3, 76), BackColor = Color.FromArgb(39, 126, 245) });
            Controls.Add(new Label
            {
                Text = title, Location = new Point(16, 10), AutoSize = true,
                ForeColor = Color.White, Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            });
            Controls.Add(new Label
            {
                Text = english ? "Key" : "Klávesa", Location = new Point(247, 10), AutoSize = true,
                ForeColor = Color.FromArgb(173, 193, 218)
            });

            ctrl = MakeCheckBox("Ctrl", 16, (initial & Keys.Control) != 0);
            shift = MakeCheckBox("Shift", 91, (initial & Keys.Shift) != 0);
            alt = MakeCheckBox("Alt", 170, (initial & Keys.Alt) != 0);
            Controls.Add(ctrl);
            Controls.Add(shift);
            Controls.Add(alt);

            key = new ComboBox
            {
                Location = new Point(247, 38), Size = new Size(195, 27),
                DropDownStyle = ComboBoxStyle.DropDownList, FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(39, 58, 84), ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F), IntegralHeight = false, DropDownHeight = 220
            };
            for (int i = 0; i < 26; i++) AddChoice((Keys)((int)Keys.A + i), ((char)('A' + i)).ToString());
            for (int i = 0; i < 10; i++) AddChoice((Keys)((int)Keys.D0 + i), i.ToString());
            for (int i = 1; i <= 24; i++) AddChoice((Keys)((int)Keys.F1 + i - 1), "F" + i);
            AddChoice(Keys.Space, english ? "Space" : "Mezerník");
            AddChoice(Keys.Insert, "Insert");
            AddChoice(Keys.Delete, "Delete");
            AddChoice(Keys.Home, "Home");
            AddChoice(Keys.End, "End");
            AddChoice(Keys.Prior, "Page Up");
            AddChoice(Keys.Next, "Page Down");
            AddChoice(Keys.Up, "↑");
            AddChoice(Keys.Down, "↓");
            AddChoice(Keys.Left, "←");
            AddChoice(Keys.Right, "→");

            Keys initialCode = initial & Keys.KeyCode;
            int index = -1;
            for (int i = 0; i < key.Items.Count; i++)
                if (((KeyChoice)key.Items[i]).Value == initialCode) { index = i; break; }
            if (index < 0)
            {
                AddChoice(initialCode, initialCode.ToString());
                index = key.Items.Count - 1;
            }
            key.SelectedIndex = index;
            Controls.Add(key);
        }

        CheckBox MakeCheckBox(string text, int x, bool isChecked)
        {
            return new CheckBox
            {
                Text = text, Location = new Point(x, 39), Size = new Size(72, 24),
                Checked = isChecked, BackColor = BackColor, ForeColor = Color.FromArgb(220, 232, 247),
                FlatStyle = FlatStyle.Flat
            };
        }

        void AddChoice(Keys value, string text)
        {
            key.Items.Add(new KeyChoice(value, text));
        }

        sealed class KeyChoice
        {
            public readonly Keys Value;
            readonly string text;

            public KeyChoice(Keys value, string text)
            {
                Value = value;
                this.text = text;
            }

            public override string ToString()
            {
                return text;
            }
        }
    }
}

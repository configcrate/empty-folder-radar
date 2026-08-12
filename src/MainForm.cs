using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace ConfigCrate.EmptyFolderRadar
{
    internal sealed class MainForm : Form
    {
        private static readonly Color Bg = Color.FromArgb(15, 18, 24);
        private static readonly Color Surface = Color.FromArgb(25, 31, 41);
        private static readonly Color Raised = Color.FromArgb(35, 43, 56);
        private static readonly Color Border = Color.FromArgb(60, 72, 91);
        private static readonly Color Mint = Color.FromArgb(92, 225, 181);
        private static readonly Color Ink = Color.FromArgb(10, 43, 35);
        private static readonly Color Main = Color.FromArgb(238, 242, 248);
        private static readonly Color Muted = Color.FromArgb(155, 166, 185);
        private static readonly Color Amber = Color.FromArgb(245, 190, 92);

        private readonly LocaleText text = new LocaleText();
        private readonly EmptyFolderScanner scanner = new EmptyFolderScanner();
        private readonly RecycleService recycler = new RecycleService();
        private readonly BackgroundWorker worker = new BackgroundWorker();
        private string rootPath;
        private ScanResult result;

        private Label subtitle;
        private Label chooseTitle;
        private Label chooseHint;
        private Button chooseButton;
        private Button localeButton;
        private Label resultTitle;
        private Label resultHint;
        private Label rootLabel;
        private DataGridView grid;
        private Button selectAll;
        private Button selectNone;
        private Button openButton;
        private Button recycleButton;
        private Button scanAgain;
        private Label privacy;

        public MainForm(string initialPath)
        {
            worker.DoWork += delegate(object sender, DoWorkEventArgs args) { args.Result = scanner.Scan((string)args.Argument); };
            worker.RunWorkerCompleted += ScanCompleted;
            BuildWindow(); ApplyLocale();
            if (!string.IsNullOrWhiteSpace(initialPath) && Directory.Exists(initialPath))
            {
                rootPath = Path.GetFullPath(initialPath);
                Shown += delegate { StartScan(); };
            }
        }

        private void BuildWindow()
        {
            Text = "Empty Folder Radar";
            try { Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath); } catch { }
            BackColor = Bg; ForeColor = Main; Font = new Font("Segoe UI", 10F);
            MinimumSize = new Size(800, 620); ClientSize = new Size(960, 720);
            StartPosition = FormStartPosition.CenterScreen; AutoScaleMode = AutoScaleMode.Dpi;
            AllowDrop = true; DragEnter += DragEntered; DragDrop += Dropped;

            Panel header = new Panel { Dock = DockStyle.Top, Height = 112, BackColor = Bg };
            LogoPanel logo = new LogoPanel { Location = new Point(26, 26) };
            Label title = new Label { Text = "Empty Folder Radar", Font = new Font("Segoe UI Semibold", 20F, FontStyle.Bold), ForeColor = Main, AutoSize = true, Location = new Point(96, 12) };
            subtitle = new Label { ForeColor = Muted, AutoSize = true, Location = new Point(99, 65) };
            localeButton = Secondary("EN", new Size(64, 36)); localeButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            localeButton.Click += delegate { text.Toggle(); ApplyLocale(); Render(); };
            header.Controls.Add(logo); header.Controls.Add(title); header.Controls.Add(subtitle); header.Controls.Add(localeButton);
            header.Layout += delegate
            {
                title.Left = logo.Right + 16; title.Top = 10; subtitle.Location = new Point(title.Left + 3, title.Bottom + 4);
                header.Height = subtitle.Bottom + 18; logo.Top = (header.Height - logo.Height) / 2;
                localeButton.Location = new Point(header.ClientSize.Width - localeButton.Width - 26, (header.Height - localeButton.Height) / 2);
            };

            Panel pickerHost = new Panel { Dock = DockStyle.Top, Height = 204, BackColor = Bg, Padding = new Padding(26, 4, 26, 16) };
            Panel picker = new Panel { Dock = DockStyle.Fill, BackColor = Surface, BorderStyle = BorderStyle.FixedSingle, AllowDrop = true, Cursor = Cursors.Hand };
            Label icon = new Label { Text = "⌕", ForeColor = Mint, Font = new Font("Segoe UI Symbol", 30F), AutoSize = true, Location = new Point(32, 38), Cursor = Cursors.Hand };
            chooseTitle = new Label { ForeColor = Main, Font = new Font("Segoe UI Semibold", 14F, FontStyle.Bold), AutoSize = true, Location = new Point(105, 26), Cursor = Cursors.Hand };
            chooseHint = new Label { ForeColor = Muted, AutoSize = true, Location = new Point(108, 67), Cursor = Cursors.Hand };
            chooseButton = Primary(string.Empty, new Size(148, 42)); chooseButton.Location = new Point(106, 105); chooseButton.Click += delegate { ChooseFolder(); };
            foreach (Control c in new Control[] { picker, icon, chooseTitle, chooseHint }) c.Click += delegate { ChooseFolder(); };
            picker.DragEnter += DragEntered; picker.DragDrop += Dropped;
            picker.Controls.Add(icon); picker.Controls.Add(chooseTitle); picker.Controls.Add(chooseHint); picker.Controls.Add(chooseButton); pickerHost.Controls.Add(picker);
            picker.Layout += delegate
            {
                int contentLeft = icon.Right + 20;
                chooseTitle.Location = new Point(contentLeft, 24);
                chooseHint.Location = new Point(contentLeft + 3, chooseTitle.Bottom + 8);
                chooseButton.Location = new Point(contentLeft, chooseHint.Bottom + 18);
                icon.Top = Math.Max(24, chooseTitle.Top + (chooseTitle.Height - icon.Height) / 2);
                int pickerBottomPadding = 24;
                int desiredPickerHeight = chooseButton.Bottom + pickerBottomPadding;
                int desiredHostHeight = desiredPickerHeight + pickerHost.Padding.Top + pickerHost.Padding.Bottom;
                if (pickerHost.Height != desiredHostHeight) pickerHost.Height = desiredHostHeight;
            };

            Panel summary = new Panel { Dock = DockStyle.Top, Height = 88, BackColor = Bg };
            resultTitle = new Label { ForeColor = Main, Font = new Font("Segoe UI Semibold", 14F, FontStyle.Bold), AutoSize = true, Location = new Point(26, 15) };
            resultHint = new Label { ForeColor = Muted, AutoSize = true, Location = new Point(27, 50) };
            rootLabel = new Label { ForeColor = Muted, TextAlign = ContentAlignment.MiddleRight, AutoEllipsis = true, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right, Location = new Point(520, 18), Size = new Size(410, 42) };
            summary.Controls.Add(resultTitle); summary.Controls.Add(resultHint); summary.Controls.Add(rootLabel);
            summary.Resize += delegate { rootLabel.Left = Math.Max(430, summary.Width / 2); rootLabel.Width = summary.Width - rootLabel.Left - 26; };

            grid = new DataGridView
            {
                Dock = DockStyle.Fill, BackgroundColor = Bg, BorderStyle = BorderStyle.None, GridColor = Border,
                AllowUserToAddRows = false, AllowUserToDeleteRows = false, AllowUserToResizeRows = false,
                RowHeadersVisible = false, MultiSelect = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoGenerateColumns = false, EnableHeadersVisualStyles = false, ColumnHeadersHeight = 40,
                RowTemplate = { Height = 52 },
                DefaultCellStyle = { BackColor = Surface, ForeColor = Main, SelectionBackColor = Color.FromArgb(38, 67, 65), SelectionForeColor = Main, Padding = new Padding(7, 0, 7, 0) },
                AlternatingRowsDefaultCellStyle = { BackColor = Color.FromArgb(22, 27, 36) },
                ColumnHeadersDefaultCellStyle = { BackColor = Bg, ForeColor = Muted, SelectionBackColor = Bg, SelectionForeColor = Muted, Font = new Font("Segoe UI Semibold", 9.5F), Padding = new Padding(7, 0, 7, 0) }
            };
            grid.Columns.Add(new DataGridViewCheckBoxColumn { Name = "Selected", HeaderText = string.Empty, Width = 48 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Folder", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 60 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Includes", Width = 210 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Modified", Width = 145 });
            grid.CellValueChanged += delegate(object sender, DataGridViewCellEventArgs args) { if (args.RowIndex >= 0 && args.ColumnIndex == 0) SyncSelection(args.RowIndex); };
            grid.CurrentCellDirtyStateChanged += delegate { if (grid.IsCurrentCellDirty) grid.CommitEdit(DataGridViewDataErrorContexts.Commit); };
            grid.CellDoubleClick += delegate(object sender, DataGridViewCellEventArgs args) { if (args.RowIndex >= 0) OpenSelected(); };

            Panel gridHost = new Panel { Dock = DockStyle.Fill, BackColor = Bg, Padding = new Padding(26, 0, 26, 0) }; gridHost.Controls.Add(grid);
            Panel footer = new Panel { Dock = DockStyle.Bottom, Height = 96, BackColor = Bg };
            selectAll = Secondary(string.Empty, new Size(90, 38)); selectAll.Location = new Point(26, 15); selectAll.Click += delegate { SetAll(true); };
            selectNone = Secondary(string.Empty, new Size(110, 38)); selectNone.Location = new Point(126, 15); selectNone.Click += delegate { SetAll(false); };
            openButton = Secondary(string.Empty, new Size(130, 38)); openButton.Location = new Point(246, 15); openButton.Click += delegate { OpenSelected(); };
            recycleButton = Primary(string.Empty, new Size(156, 38)); recycleButton.Location = new Point(386, 15); recycleButton.Click += delegate { RecycleSelected(); };
            scanAgain = Secondary(string.Empty, new Size(120, 38)); scanAgain.Location = new Point(552, 15); scanAgain.Click += delegate { StartScan(); };
            privacy = new Label { ForeColor = Muted, AutoSize = true, Anchor = AnchorStyles.Right | AnchorStyles.Bottom };
            footer.Controls.Add(selectAll); footer.Controls.Add(selectNone); footer.Controls.Add(openButton); footer.Controls.Add(recycleButton); footer.Controls.Add(scanAgain); footer.Controls.Add(privacy);
            footer.Resize += delegate { privacy.Location = new Point(footer.Width - privacy.Width - 26, 68); };

            Controls.Add(gridHost); Controls.Add(footer); Controls.Add(summary); Controls.Add(pickerHost); Controls.Add(header);
        }

        private Button Primary(string caption, Size size) { Button b = new Button { Text = caption, Size = size, FlatStyle = FlatStyle.Flat, BackColor = Mint, ForeColor = Ink, Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold), Cursor = Cursors.Hand }; b.FlatAppearance.BorderSize = 0; return b; }
        private Button Secondary(string caption, Size size) { Button b = new Button { Text = caption, Size = size, FlatStyle = FlatStyle.Flat, BackColor = Raised, ForeColor = Main, Cursor = Cursors.Hand }; b.FlatAppearance.BorderColor = Border; b.FlatAppearance.BorderSize = 1; return b; }

        private void ApplyLocale()
        {
            subtitle.Text = text.Subtitle; chooseTitle.Text = text.Choose; chooseHint.Text = text.ChooseHint; chooseButton.Text = text.Browse;
            localeButton.Text = text.IsChinese ? "EN" : "中"; resultTitle.Text = text.Waiting; resultHint.Text = string.Empty;
            grid.Columns["Folder"].HeaderText = text.Folder; grid.Columns["Includes"].HeaderText = text.Includes; grid.Columns["Modified"].HeaderText = text.Modified;
            selectAll.Text = text.SelectAll; selectNone.Text = text.SelectNone; openButton.Text = text.Open; recycleButton.Text = text.Recycle; scanAgain.Text = text.ScanAgain; privacy.Text = text.Local;
            if (privacy.Parent != null) privacy.Location = new Point(privacy.Parent.Width - privacy.Width - 26, 68);
        }

        private void ChooseFolder()
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog { ShowNewFolderButton = false })
            {
                if (dialog.ShowDialog(this) == DialogResult.OK) { rootPath = dialog.SelectedPath; StartScan(); }
            }
        }

        private void DragEntered(object sender, DragEventArgs args) { args.Effect = args.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None; }
        private void Dropped(object sender, DragEventArgs args) { string[] paths = args.Data.GetData(DataFormats.FileDrop) as string[]; if (paths != null && paths.Length > 0 && Directory.Exists(paths[0])) { rootPath = paths[0]; StartScan(); } }

        private void StartScan()
        {
            if (worker.IsBusy || string.IsNullOrWhiteSpace(rootPath)) return;
            if (SafetyGuard.IsUnsafeScanRoot(rootPath)) { MessageBox.Show(this, text.Unsafe, text.Error, MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            SetBusy(true); result = null; grid.Rows.Clear(); resultTitle.Text = text.Scanning; resultTitle.ForeColor = Amber; resultHint.Text = string.Empty;
            rootLabel.Text = Path.GetFileName(rootPath.TrimEnd(Path.DirectorySeparatorChar)); worker.RunWorkerAsync(rootPath);
        }

        private void ScanCompleted(object sender, RunWorkerCompletedEventArgs args)
        {
            SetBusy(false); if (args.Error != null) { MessageBox.Show(this, args.Error.Message, text.Error, MessageBoxButtons.OK, MessageBoxIcon.Error); return; }
            result = args.Result as ScanResult; Render();
        }

        private void Render()
        {
            grid.Rows.Clear(); if (result == null) { UpdateButtons(); return; }
            rootLabel.Text = Path.GetFileName(result.RootPath); resultTitle.ForeColor = result.Items.Count == 0 ? Mint : Amber;
            resultTitle.Text = result.Items.Count == 0 ? text.None : text.Found(result.Items.Count, result.EmptyFoldersRepresented);
            resultHint.Text = result.Warnings.Count > 0 ? text.Warnings(result.Warnings.Count) : (result.Items.Count == 0 ? text.NoneHint : string.Empty);
            foreach (EmptyFolderItem item in result.Items)
            {
                string relative = item.Path.Substring(result.RootPath.Length).TrimStart(Path.DirectorySeparatorChar);
                int rowIndex = grid.Rows.Add(item.Selected, relative, item.EmptyDescendantCount == 0 ? text.EmptyOnly : text.EmptyMany(item.EmptyDescendantCount), item.LastWriteTime == DateTime.MinValue ? "—" : item.LastWriteTime.ToString("yyyy-MM-dd"));
                grid.Rows[rowIndex].Tag = item;
            }
            if (grid.Rows.Count > 0) grid.Rows[0].Selected = true; UpdateButtons();
        }

        private void SyncSelection(int row) { EmptyFolderItem item = grid.Rows[row].Tag as EmptyFolderItem; if (item != null) item.Selected = Convert.ToBoolean(grid.Rows[row].Cells["Selected"].Value); UpdateButtons(); }
        private void SetAll(bool selected) { if (result == null) return; foreach (EmptyFolderItem item in result.Items) item.Selected = selected; foreach (DataGridViewRow row in grid.Rows) row.Cells["Selected"].Value = selected; UpdateButtons(); }
        private EmptyFolderItem Current() { return grid.SelectedRows.Count == 0 ? null : grid.SelectedRows[0].Tag as EmptyFolderItem; }
        private void OpenSelected() { EmptyFolderItem item = Current(); if (item != null && Directory.Exists(item.Path)) Process.Start(new ProcessStartInfo("explorer.exe", "\"" + item.Path + "\"") { UseShellExecute = true }); }

        private void RecycleSelected()
        {
            if (result == null) return; List<EmptyFolderItem> chosen = result.Items.Where(item => item.Selected).ToList(); if (chosen.Count == 0) return;
            int represented = chosen.Sum(item => item.EmptyDescendantCount + 1);
            if (MessageBox.Show(this, text.Confirm(chosen.Count, represented), Text, MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) != DialogResult.Yes) return;
            int done = 0, failed = 0;
            foreach (EmptyFolderItem item in chosen.OrderByDescending(item => item.Path.Length))
            {
                try
                {
                    if (!SafetyGuard.CanRecycle(result.RootPath, item.Path) || !scanner.IsStillRecursivelyEmpty(item.Path))
                    {
                        failed++;
                        continue;
                    }
                    recycler.RecycleDirectory(item.Path);
                    done++;
                }
                catch { failed++; }
            }
            MessageBox.Show(this, failed == 0 ? text.Done(done) : text.Partial(done, failed), Text, MessageBoxButtons.OK, failed == 0 ? MessageBoxIcon.Information : MessageBoxIcon.Warning); StartScan();
        }

        private void SetBusy(bool busy) { UseWaitCursor = busy; chooseButton.Enabled = !busy; localeButton.Enabled = !busy; scanAgain.Enabled = !busy && !string.IsNullOrWhiteSpace(rootPath); UpdateButtons(); }
        private void UpdateButtons() { bool has = result != null && result.Items.Count > 0 && !worker.IsBusy; selectAll.Enabled = has; selectNone.Enabled = has; openButton.Enabled = has && Current() != null; recycleButton.Enabled = has && result.Items.Any(item => item.Selected); scanAgain.Enabled = !worker.IsBusy && !string.IsNullOrWhiteSpace(rootPath); }
    }
}

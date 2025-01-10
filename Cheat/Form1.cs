using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
//using static System.Windows.Forms.VisualStyles.VisualStyleElement;
#pragma warning disable CA1416
namespace Cheat
{
    public partial class Form1 : Form
    {
        private bool _mouseDown;
        private Point _lastLocation;

        // Used to indicate the "Start Typing..." is on display
        //
        private bool _initalState = true;
        private bool _isChanging = false;
        private bool _isBackspace;
        private bool _isEscape;
        private bool _debugMode = true;

        private string[] _fileNames;
        private Dictionary<string, List<string>> _tags = new Dictionary<string, List<string>>();
        private List<string> _candidateList;
        private int _candidateListIndex;

        private Action<string> log; 
        private List<string> _commands = new List<string>();
        private  bool _useCustomTypeahead;
        private bool _includeReadMe;

        protected struct FileInfo
        {
            public string Name;
            public bool AutoCopy;
            public List<string> Tags;
        }


        private Dictionary<string, CUtils.FileInfo> _filesToTags = new Dictionary<string, CUtils.FileInfo>();

        private List<string> _findList = new List<string>();
        private int _findListIndex = 0;
        public Form1()
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.None;
            picCopy.Visible = false;
           
            SetupLogging();
            log("Form Initialization Start");
            customListBox1.Visible = false;
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            
            textBox2.MouseWheel += TextBox2_MouseWheel;

            _useCustomTypeahead = true;
            if (_useCustomTypeahead)
            {
                textBox1.KeyDown -= textBox1_KeyDown;
                textBox1.TextChanged -= textBox1_TextChanged;
                textBox1.KeyDown += TextBox1_CustomKeyDown;
                textBox1.TextChanged += TextBox1_CustomTextChanged;
            }
            log("Form Initialization End");
        }

        private void SetupLogging()
        {
            log = delegate (string msg) 
            { 
                if (_debugMode) 
                { 
                    System.Diagnostics.Debug.WriteLine($"{DateTime.Now.ToString()}: {msg}"); 
                } 
            };
        }


        private void TextBox1_CustomTextChanged(object sender, EventArgs e)
        {
            log($"in the custom text changed event .. text is {textBox1.Text}");
            log($"in the custom text changed event initialstate is  {_initalState}");

            if (_isBackspace)
            {
                log("Backspace Doing nothing just returning... Should close the dialog now");
                _isBackspace = false;
                return;
            }

            if (_isChanging) { _isChanging = false; return; }

            if (textBox1.Text == string.Empty)
            {
                log("Found empty!!!!!!!");
                _isChanging = true;
                textBox1.Text = "Start Typing...";
                textBox1.SelectionStart = 0;
                _initalState = true;
                return;

            }

            List<string> _commands = new List<string>() { "--edit" };//, "--find" }; //

            _initalState = false;
            Console.WriteLine("Not changing ... ");
            //searching the first candidate 
            // string typed = textBox4.Text.Substring(0, textBox4.SelectionStart);
            var leftIndex = 0;

            var currentCommand = string.Empty;
            var newTyped = string.Empty;
            foreach (var s in _commands)
            {
                if (textBox1.Text.StartsWith(s + " "))
                {

                    currentCommand = s + " ";
                    leftIndex = currentCommand.Length; // - 1;
                    Console.WriteLine($"Found command {s} : left index : {leftIndex}");
                    break;
                }
            }

            newTyped = textBox1.Text.Substring(leftIndex).Trim();

            _candidateList = new List<string>();
            _candidateList = _fileNames.Where
                (
                    item =>
                        item.StartsWith(newTyped, StringComparison.OrdinalIgnoreCase)
                     && item != newTyped
                ).ToList<string>();

            log($"Size of candidate list {_candidateList.Count}");

            foreach (var item in _candidateList)
            {

                log($"Searching for {newTyped} against {item}");

                if (newTyped != string.Empty &&
                    item.StartsWith(newTyped, StringComparison.OrdinalIgnoreCase))
                {
                    log($"Found {item}, it begins with {newTyped}");

                    if (item.Length >= newTyped.Length)
                    {
                        // Update the text with the found item and highlight 
                        // the balance
                        //
                        var diff = item.Length - newTyped.Length;
                        var newselestart = item.Length - diff;
                        var newselelength = diff + 1;

                        _isChanging = true;
                        textBox1.Text = currentCommand + item;
                        textBox1.SelectionStart = newselestart + leftIndex;
                        textBox1.SelectionLength = newselelength;
                        // _isInIntialState = false;
                        break;
                    }
                }
            }

        }

        private void TextBox1_CustomKeyDown(object sender, KeyEventArgs e)
        {
            log($"Custom Keydown {e.KeyCode}");
            log($"Custom Keydown : initialstate : {_initalState}");
            _isBackspace = e.KeyCode == Keys.Back || e.KeyCode == Keys.Delete;
            _isEscape = e.KeyCode == Keys.Escape;

            //if (e.KeyCode == Keys.Up)
            //{
            //    if (_candidateList != null && _candidateList.Count > 0)
            //    {
            //        log($"Scrolling up in candidate list : current index {_candidateListIndex}" );

            //    }
            //    return;
            //}


            if (_isEscape && _initalState)
            {
                log("Doing nothing just returning... Should close the dialog now");
                this.Close();
                return;
            }

            if (_initalState) //&& textBox4.Text != string.Empty)
            {
                log("Clearing the initial text");
                _isChanging = true;
                textBox1.Text = "";
                _initalState = false;
                return;
            }



            if (e.KeyCode == Keys.Down)
            {
                customListBox1.KeyWasPressed(e);

                return;
            }
            else if (e.KeyCode == Keys.Up)
            {
                customListBox1.KeyWasPressed(e);

                return;
            }
            else
            {
                _findList = new List<string>();
                _findListIndex = 0;

                customListBox1.Visible = false;
            }

            if (e.KeyCode == Keys.Escape && textBox1.Text.Trim() != string.Empty)
            {
                if (_initalState) { this.Close(); }

                textBox1.Text = "";
                _initalState = true;

                customListBox1.Visible = false;
                return;

            }
            if (e.KeyCode == Keys.Left || e.KeyCode == Keys.Right)
            {
                return;
            }
            if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }


            if (_initalState)
            {
                // Clear the text of the "Start typing and then continue"
                //
                log("Custom key down .. clearing the text");
                textBox1.Text = "";
                Console.WriteLine("key down .. about to set _initialstate");
                _initalState = false;
            }

            if (e.KeyCode == Keys.Enter)
            {

                RecentCommands.Add(_commands, textBox1.Text.ToLower().TrimStart());

                if (textBox1.Text.ToLower().TrimStart() == "--help")
                {
                    ShowHelp(textBox2);
                    return;
                }

                if (textBox1.Text.ToLower().TrimStart() == "--list")
                {
                    ShowList(textBox2);
                    return;
                }

                if (textBox1.Text.ToLower().TrimStart() == "--config")
                {

                    ShowConfig(textBox2);
                    return;
                }
                if (textBox1.Text.ToLower().TrimStart() == "--editconfig")
                {
                    EditConfig(textBox2);
                    return;
                }

                if (textBox1.Text.ToLower().TrimStart() == "--tags")
                {
                    ShowTags(textBox2);
                    return;
                }

                if (textBox1.Text.Length >= 12 && textBox1.Text.ToLower().Substring(0, 12).TrimStart() == "--listcheats")
                {
                    ShowListTags(textBox2, textBox1);
                    return;
                }

                if (textBox1.Text.ToLower().TrimStart() == "--version")
                {
                    ShowVersion(textBox2);
                    return;
                }

                if (textBox1.Text.Length >= 6 && textBox1.Text.ToLower().Substring(0, 6).TrimStart() == "--edit")
                {
                    ShowEditor(textBox1);
                    return;
                }

                if (textBox1.Text.Length >= 6 && textBox1.Text.ToLower().Substring(0, 6).TrimStart() == "--find")
                {
                    ShowSearch(textBox2, textBox1);
                    return;
                }

                if (textBox1.Text.ToLower().TrimStart() == "--history")
                {
                    ShowLastUsedCommands(textBox2);
                    return;
                }

                if (textBox1.Text.ToLower().TrimStart() == "--refresh")
                {
                    ReloadFileList();
                    return;
                }


                var appender = string.Empty;
                if (File.Exists(Configuration.FilesLocation + $"\\{textBox1.Text.TrimStart()}{appender}"))
                {
                    textBox2.Clear();
                    var contents = File.ReadAllLines(Configuration.FilesLocation + $"\\{textBox1.Text.TrimStart()}{appender}");

                    var autoCopyFlag = GetAutoCopyFlag(contents);

                    var index = SkipConfig(contents);

                    if (index > 0 && index <= contents.Length - 1)
                    {
                        StringBuilder sb = new StringBuilder();
                        for (int i = index; i < contents.Length; i++)
                        {
                            sb.Append(contents[i]);
                            sb.Append(Environment.NewLine);
                        }
                        textBox2.Text = sb.ToString();
                    }
                    else
                    {
                        StringBuilder sb = new StringBuilder();
                        foreach (var c in contents)
                        {
                            sb.Append(c);
                            sb.Append(Environment.NewLine);
                        }
                        textBox2.Text = sb.ToString();
                    }

                    if (autoCopyFlag)
                    {
                        picCopy.Visible = true;
                        Clipboard.SetText(textBox2.Text);
                    }
                    else
                    {
                        picCopy.Visible = false;
                    }


                }
            }
        }

        private void TextBox2_MouseWheel(object sender, MouseEventArgs e)
        {
            log($"{e.Delta}");
            ScrollTextBox(sender,e);
        }

        private void ScrollTextBox(object sender, MouseEventArgs e)
        // Mouse wheel has been turned while text box has focus
        {

            // Check scroll amount (+ve is upwards)
            int deltaWheel = e.Delta;
            if (deltaWheel != 0)
            {
                // Find total number of lines
                int nLines = textBox2.Lines.Length;
                if (nLines > 0)
                {
                    // Find line containing caret
                    int iLine = textBox2.GetLineFromCharIndex(textBox2.SelectionStart);
                    if (iLine >= 0)
                    {
                        // Scroll down
                        if (deltaWheel > 0)
                        {
                            // Move caret to start of previous line
                            if (iLine > 0)
                            {
                                int position = textBox2.GetFirstCharIndexFromLine(iLine - 1);
                                textBox2.Select(position, 0);

                            }
                        }
                        else // Scroll up
                        {
                            // Move caret to start of next line
                            if (iLine < (nLines - 1))
                            {
                                int position = textBox2.GetFirstCharIndexFromLine(iLine + 1);
                                textBox2.Select(position, 0);
                            }
                        }

                        // Scroll to new caret position
                        textBox2.ScrollToCaret();
                    }
                }

            }

        }

        // Needed to allow for resizing with no borders
        //
        protected override void WndProc(ref Message m)
        {
            const int RESIZE_HANDLE_SIZE = 10;

            switch (m.Msg)
            {
                case 0x0084/*NCHITTEST*/ :
                    base.WndProc(ref m);

                    if ((int)m.Result == 0x01/*HTCLIENT*/)
                    {
                        Point screenPoint = new Point(m.LParam.ToInt32());
                        Point clientPoint = this.PointToClient(screenPoint);
                        if (clientPoint.Y <= RESIZE_HANDLE_SIZE)
                        {
                            if (clientPoint.X <= RESIZE_HANDLE_SIZE)
                                m.Result = (IntPtr)13/*HTTOPLEFT*/ ;
                            else if (clientPoint.X < (Size.Width - RESIZE_HANDLE_SIZE))
                                m.Result = (IntPtr)12/*HTTOP*/ ;
                            else
                                m.Result = (IntPtr)14/*HTTOPRIGHT*/ ;
                        }
                        else if (clientPoint.Y <= (Size.Height - RESIZE_HANDLE_SIZE))
                        {
                            if (clientPoint.X <= RESIZE_HANDLE_SIZE)
                                m.Result = (IntPtr)10/*HTLEFT*/ ;
                            else if (clientPoint.X < (Size.Width - RESIZE_HANDLE_SIZE))
                                m.Result = (IntPtr)2/*HTCAPTION*/ ;
                            else
                                m.Result = (IntPtr)11/*HTRIGHT*/ ;
                        }
                        else
                        {
                            if (clientPoint.X <= RESIZE_HANDLE_SIZE)
                                m.Result = (IntPtr)16/*HTBOTTOMLEFT*/ ;
                            else if (clientPoint.X < (Size.Width - RESIZE_HANDLE_SIZE))
                                m.Result = (IntPtr)15/*HTBOTTOM*/ ;
                            else
                                m.Result = (IntPtr)17/*HTBOTTOMRIGHT*/ ;
                        }
                    }
                    return;
            }
            base.WndProc(ref m);
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.Style |= 0x20000; // <--- use 0x20000
                return cp;
            }
        }


        private void DirSearch(string sDir, string rootDir, List<string> list)
        {

            try
            {
                if (!Directory.Exists(sDir)) { return; }

                foreach (string f in Directory.GetFiles(sDir))
                {
                    var dirName = rootDir == Path.GetFileName(sDir) ? string.Empty : Path.GetFileName(sDir);

                    //Console.WriteLine($"File {Path.GetFileName(f)}   Dir {Path.GetFileName(sDir)}");
                    if (dirName == string.Empty)
                    {
                        Console.WriteLine($"{Path.GetFileName(f)}");
                        if(!_includeReadMe && Path.GetFileName(f) == "Readme")
                        {
                            continue;
                        }
                        list.Add($"{Path.GetFileName(f)}");
                    }
                    else
                    {
                        Console.WriteLine($"{Path.GetFileName(sDir)}/{Path.GetFileName(f)}");
                        list.Add($"{Path.GetFileName(sDir)}/{Path.GetFileName(f)}");
                    }

                    BuildTagList(f, Path.GetFileName(sDir), rootDir);

                }
                if (Configuration.IncludeSubDirectories)
                {
                    foreach (string d in Directory.GetDirectories(sDir))
                    {
                        DirSearch(d, rootDir, list);
                    }
                }

            }
            catch (System.Exception excpt)
            {
                Console.WriteLine(excpt.Message);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            log("Form_Load Start");
            var statusMessage = string.Empty;
            try
            {
                this.BackColor = Configuration.BackColor;
                textBox1.BackColor = Configuration.BackColor;
                textBox1.ForeColor = Configuration.ForeColor;
                textBox2.ForeColor = Configuration.ForeColor;
                textBox2.BackColor = Configuration.BackColor;

                var tmpFont = new Font(textBox2.Font.Name, Configuration.FontSizePt);
                textBox2.Font = tmpFont;
                statusMessage = $"reading filesLocaton : {Configuration.FilesLocation}";   
                var files = Directory.GetFiles(Configuration.FilesLocation);
                
                ReloadFileList();

                statusMessage = "Setting location";
                SetLocation();
                if (Properties.Settings.Default.ShowHelp)
                {
                    textBox2.Text = "Try --help to start....";
                    Properties.Settings.Default.ShowHelp = false;
                }
                log("Form_Load Done");

            }
            catch (Exception)
            {
                MessageBox.Show($"Error loading while {statusMessage}");
                Application.Exit();
            }

        }
        private void ReloadFileList()
        {
            var tmplist = new List<string>();
            DirSearch(Configuration.FilesLocation, Path.GetFileName(Configuration.FilesLocation), tmplist);
         
            _fileNames = tmplist.ToArray();

            if (!_useCustomTypeahead)
            {
                textBox1.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                textBox1.AutoCompleteSource = AutoCompleteSource.CustomSource;

                AutoCompleteStringCollection suggestions = new AutoCompleteStringCollection();
                suggestions.AddRange(_fileNames);
                textBox1.AutoCompleteCustomSource = suggestions;

            }
            _isChanging = true;
            textBox1.Text = "Start typing...";
            _initalState = true;
            textBox1.Select(0, 0);
        }

        private void SetLocation()
        {
            if (Properties.Settings.Default.Maximized)
            {
                Location = Properties.Settings.Default.Location;
                WindowState = FormWindowState.Maximized;
                Size = Properties.Settings.Default.Size;
            }
            else if (Properties.Settings.Default.Minimised)
            {
                Location = Properties.Settings.Default.Location;
                WindowState = FormWindowState.Minimized;
                Size = Properties.Settings.Default.Size;
            }
            else
            {
                Location = Properties.Settings.Default.Location;
            }
        }

        private void SaveLocation()
        {
            if (WindowState == FormWindowState.Maximized)
            {
                Properties.Settings.Default.Location = RestoreBounds.Location;
                Properties.Settings.Default.Size = RestoreBounds.Size;
                Properties.Settings.Default.Maximized = true;
                Properties.Settings.Default.Minimised = false;
            }
            else if (WindowState == FormWindowState.Normal)
            {
                Properties.Settings.Default.Location = Location;
                Properties.Settings.Default.Size = Size;
                Properties.Settings.Default.Maximized = false;
                Properties.Settings.Default.Minimised = false;
            }
            else
            {
                Properties.Settings.Default.Location = RestoreBounds.Location;
                Properties.Settings.Default.Size = RestoreBounds.Size;
                Properties.Settings.Default.Maximized = false;
                Properties.Settings.Default.Minimised = true;
            }
            Properties.Settings.Default.Save();
        }
        private void ResizeHeightOfListBox(ListBox listBox)
        {
            var computedHeight = (listBox.ItemHeight * listBox.Items.Count) + 4;
            var newHeight = Math.Min(computedHeight, this.Height - textBox1.Bottom - 20);
            listBox.Height = newHeight;
        }
        private void BuildTagList(string fileName, string pathName, string rootDir)
        {
            var contents = File.ReadAllLines(fileName);
            var pname = Path.GetFileName(fileName);
            var fName = pathName == rootDir ? Path.GetFileName(fileName) : $"{pathName}/{Path.GetFileName(fileName)}";
            var tags = ExtractTagsII(contents);

            if (tags != null)
            {
                foreach (var t in tags)
                {
                    if (_tags.ContainsKey(t))
                    {
                        _tags[t.Trim()].Add(fName);
                    }
                    else
                    {
                        _tags[t.Trim()] = new List<string>() { fName };
                    }

                }
                var autoCopy = GetAutoCopyFlag(contents);

                _filesToTags[fName.ToLower()] =
                    new CUtils.FileInfo
                    { Tags = tags, AutoCopy = autoCopy, Name = fName };
                customListBox1.FilesToFlag = _filesToTags;

            }
        }

        private void ShowLastUsedCommands(System.Windows.Forms.TextBox textBox) 
        {
            ShowResults(_commands);
        }


        private void ShowSearch(System.Windows.Forms.TextBox textBox, System.Windows.Forms.TextBox input)
        {
            // Grab the parameter
            //
            if (input.Text.Length >= 7)
            {
                var param = input.Text.ToLower().Substring(7).Trim();

                if (param.Length < 3)
                {
                    textBox.Clear();
                    textBox.Text = "Search requires phrases at least 3 chars in length";
                    return;
                }

                textBox.Clear();
                _findList.Clear();
               
                foreach (var cheat in _fileNames)
                {

                    var contents = File.ReadAllLines($"{Configuration.FilesLocation}\\{cheat}");
                    foreach (var line in contents)
                    {

                        if (line.ToLower().Contains(param))
                        {
                            //  textBox.Text += cheat + Environment.NewLine;
                            _findList.Add(cheat);
                          
                            break;
                        }
                    }
                }
                if (_findList.Count > 0)
                {
                  ShowResults(_findList);
                    //customListBox1.Visible = true;
                    //customListBox1.Items = _findList;
                    //customListBox1.ShowTags = true;
                    //customListBox1.Update();
                    //customListBox1.Invalidate();
                    //customListBox1.Focus();
                    //textBox1.Text = _findList[0];
                    //textBox1.Focus();
                    //textBox1.SelectionLength = 0;
                    
                }
            }
        }

        private void ShowResults(List<string> results) 
        {
            customListBox1.Visible = true;
            customListBox1.Items = results;
            customListBox1.ShowTags = true;
            customListBox1.Update();
            customListBox1.Invalidate();
            customListBox1.Focus();
            textBox1.Text = results[0];
            textBox1.Focus();
            textBox1.SelectionLength = 0;
        }



        private void ShowHelp(System.Windows.Forms.TextBox textBox)
        {
            textBox.Clear();

            StringBuilder sb = new StringBuilder();
            sb.Append("--help");
            sb.Append(Environment.NewLine);
            sb.Append("   Shows this help information");
            sb.Append(Environment.NewLine);
            sb.Append("--list");
            sb.Append(Environment.NewLine);
            sb.Append("   Lists all cheats");
            sb.Append(Environment.NewLine);
            sb.Append("--config");
            sb.Append(Environment.NewLine);
            sb.Append("   Shows configuration locations"); sb.Append(Environment.NewLine);
            sb.Append("--tags");
            sb.Append(Environment.NewLine);
            sb.Append("   Lists all known tags");
            sb.Append(Environment.NewLine);
            sb.Append("--listcheats <tag>");
            sb.Append(Environment.NewLine);
            sb.Append("   Lists all cheats for the given <tag>");
            sb.Append(Environment.NewLine);
            sb.Append("--edit <cheat>");
            sb.Append(Environment.NewLine);
            sb.Append("   Opens the <cheat> file in the configured editor");
            sb.Append(Environment.NewLine);
            sb.Append("--editconfig");
            sb.Append(Environment.NewLine);
            sb.Append("   Opens the configuration file in the configured editor");
            sb.Append(Environment.NewLine);
            sb.Append("--find <text>");
            sb.Append(Environment.NewLine);
            sb.Append("   Lists all cheats containing <text>");
            sb.Append(Environment.NewLine);
            sb.Append("--history");
            sb.Append(Environment.NewLine);
            sb.Append("   Lists the last n commands");
            sb.Append(Environment.NewLine);
            sb.Append("--version");
            sb.Append(Environment.NewLine);
            sb.Append("   Shows version info");
            sb.Append(Environment.NewLine);

            textBox.Text = sb.ToString();

        }
        #region Commands

        private void ShowList(System.Windows.Forms.TextBox textBox)
        {
            textBox.Clear();
            _findList.Clear();
           
            foreach (var f in _fileNames)
            {
                //   textBox.Text += f + Environment.NewLine;
            
                _findList.Add(f);
            }
            if (_findList.Count > 0)
            {
              ShowResults(_findList);
                //customListBox1.Visible = true;
                //customListBox1.Items = _findList;
                //customListBox1.ShowTags = true;
                //customListBox1.Update();
                //customListBox1.Invalidate();
                //customListBox1.Focus();
                //textBox1.Text = _findList[0];

            }

        }

        private void ShowConfig(System.Windows.Forms.TextBox textBox)
        {
            textBox.Clear();
            textBox.Text = "Configuration File:" + Environment.NewLine;
            textBox.Text += Configuration.ConfigFilePath + "Config.xml" + Environment.NewLine;
            textBox.Text += "----------------------------------" + Environment.NewLine + Environment.NewLine;
            var contents = File.ReadAllLines(Configuration.ConfigFilePath + "Config.xml");

            foreach (var line in contents)
            {
                textBox2.Text += line + Environment.NewLine;
            }

            textBox.Text += Environment.NewLine + "----------------------------------" + Environment.NewLine;

        }

        private void EditConfig(System.Windows.Forms.TextBox textBox)
        {
            Process.Start(Configuration.Editor, $"{Configuration.ConfigFilePath}\\Config.xml");
            textBox.Clear();

        }

        private void ShowTags(System.Windows.Forms.TextBox textBox)
        {
            textBox.Clear();
            textBox.Text = "Current Tags:" + Environment.NewLine;
            foreach (var t in _tags)
            {
                textBox.Text += "  " + t.Key + Environment.NewLine;
            }
        }
        private void ShowVersion(System.Windows.Forms.TextBox textBox)
        {
            textBox.Clear();
            var productVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            textBox.Text = Environment.NewLine + $" Version :{productVersion}";
        }

        private void ShowListTags(System.Windows.Forms.TextBox textBox, System.Windows.Forms.TextBox input)
        {
            // Grab the paramater
            //
            if (input.Text.Length >= 13)
            {
                var param = input.Text.ToLower().Substring(13).Trim();

                if (_tags.ContainsKey(param))
                {
                    _findList.Clear();
                   
                    textBox.Clear();
                    // textBox.Text = $"Cheats with tag: {param}" + Environment.NewLine;
                    foreach (var t in _tags[param])
                    {
                        //  textBox.Text += "  " + t + Environment.NewLine;
                        _findList.Add(t);
                   
                    }
                }
            }
            if (_findList.Count > 0)
            {
                ShowResults(_findList);
            }
        }

        private void ShowEditor(System.Windows.Forms.TextBox input)
        {
            // Grab the paramater
            //
            if (input.Text.Length >= 6)
            {
                var param = input.Text.ToLower().Substring(6).Trim();
                Process.Start(Configuration.Editor, $"{Configuration.FilesLocation}\\{param}");
                Console.WriteLine($"{Configuration.FilesLocation}\\{param}");
            }
        }
        #endregion

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            log($"in the text changed event .. text is {textBox1.Text}");
            log($"in the text changed event state is  {_initalState}");
            if (textBox1.Text == string.Empty && !_initalState)
            {
                log("Text changed");
                textBox1.Text = "Start typing...";
                _initalState = true;
            }
            if (_isChanging)
            {

                textBox1.Select(0, 0);
                textBox1.SelectionStart = 0;
                textBox1.SelectionLength = 0;
                _isChanging = false;
                log($"In the if and setting the cursor {textBox1.SelectionStart}");
            }

        }
        #region Command_Handling

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {

            if (e.KeyCode == Keys.Down)
            {
                customListBox1.KeyWasPressed(e);

                return;
            }
            else if (e.KeyCode == Keys.Up)
            {
                customListBox1.KeyWasPressed(e);

                return;
            }
            else
            {
                _findList = new List<string>();
                _findListIndex = 0;
               
                customListBox1.Visible =false;
            }

            if (e.KeyCode == Keys.Escape && textBox1.Text.Trim() != string.Empty)
            {
                if (_initalState) { this.Close(); }

                textBox1.Text = "";
                _initalState = true;
              
                customListBox1.Visible = false;
                return;

            }
            if (e.KeyCode == Keys.Left || e.KeyCode == Keys.Right)
            {
                return;
            }
            if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }


            if (_initalState)
            {
                // Clear the text of the "Start typing and then continue"
                //
                log("key down .. clearing the text");
                textBox1.Text = "";
                Console.WriteLine("key down .. about to set _initialstate");
                _initalState = false;
            }

            if (e.KeyCode == Keys.Enter)
            {

                RecentCommands.Add(_commands, textBox1.Text.ToLower().TrimStart());

                if (textBox1.Text.ToLower().TrimStart() == "--help")
                {
                    ShowHelp(textBox2);
                    return;
                }

                if (textBox1.Text.ToLower().TrimStart() == "--list")
                {
                    ShowList(textBox2);
                    return;
                }

                if (textBox1.Text.ToLower().TrimStart() == "--config")
                {

                    ShowConfig(textBox2);
                    return;
                }
                if (textBox1.Text.ToLower().TrimStart() == "--editconfig")
                {
                    EditConfig(textBox2);
                    return;
                }

                if (textBox1.Text.ToLower().TrimStart() == "--tags")
                {
                    ShowTags(textBox2);
                    return;
                }

                if (textBox1.Text.Length >= 12 && textBox1.Text.ToLower().Substring(0, 12).TrimStart() == "--listcheats")
                {
                    ShowListTags(textBox2, textBox1);
                    return;
                }

                if (textBox1.Text.ToLower().TrimStart() == "--version")
                {
                    ShowVersion(textBox2);
                    return;
                }

                if (textBox1.Text.Length >= 6 && textBox1.Text.ToLower().Substring(0, 6).TrimStart() == "--edit")
                {
                    ShowEditor(textBox1);
                    return;
                }

                if (textBox1.Text.Length >= 6 && textBox1.Text.ToLower().Substring(0, 6).TrimStart() == "--find")
                {
                    ShowSearch(textBox2, textBox1);
                    return;
                }

                if (textBox1.Text.ToLower().TrimStart() == "--history")
                {
                    ShowLastUsedCommands(textBox2);
                    return;
                }

                var appender = string.Empty;
                if (File.Exists(Configuration.FilesLocation + $"\\{textBox1.Text.TrimStart()}{appender}"))
                {
                    textBox2.Clear();
                    var contents = File.ReadAllLines(Configuration.FilesLocation + $"\\{textBox1.Text.TrimStart()}{appender}");

                    var autoCopyFlag = GetAutoCopyFlag(contents);

                    var index = SkipConfig(contents);

                    if (index > 0 && index <= contents.Length - 1)
                    {
                        StringBuilder sb = new StringBuilder();
                        for (int i = index; i < contents.Length; i++)
                        {
                            sb.Append(contents[i]);
                            sb.Append(Environment.NewLine);
                        }
                        textBox2.Text = sb.ToString();
                    }
                    else
                    {
                        StringBuilder sb = new StringBuilder();
                        foreach (var c in contents)
                        {
                            sb.Append(c);
                            sb.Append(Environment.NewLine);
                        }
                        textBox2.Text = sb.ToString();
                    }

                    if (autoCopyFlag)
                    {
                        picCopy.Visible = true;
                        Clipboard.SetText(textBox2.Text);
                    }
                    else
                    {
                        picCopy.Visible = false;
                    }


                }
            }
        }

        private void LoadCheat()
        {
            var appender = string.Empty;
            if (File.Exists(Configuration.FilesLocation + $"\\{textBox1.Text.TrimStart()}{appender}"))
            {
                textBox2.Clear();
                var contents = File.ReadAllLines(Configuration.FilesLocation + $"\\{textBox1.Text.TrimStart()}{appender}");

                var autoCopyFlag = GetAutoCopyFlag(contents);

                var index = SkipConfig(contents);

                if (index > 0 && index <= contents.Length - 1)
                {
                    StringBuilder sb = new StringBuilder();
                    for (int i = index; i < contents.Length; i++)
                    {
                        sb.Append(contents[i]);
                        sb.Append(Environment.NewLine);
                    }
                    textBox2.Text = sb.ToString();
                }
                else
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (var c in contents)
                    {
                        sb.Append(c);
                        sb.Append(Environment.NewLine);
                    }
                    textBox2.Text = sb.ToString();
                }

                if (autoCopyFlag)
                {
                    picCopy.Visible = true;
                    Clipboard.SetText(textBox2.Text);
                }
                else
                {
                    picCopy.Visible = false;
                }


            }
        }

        #endregion

        #region Utils

        public bool GetAutoCopyFlag(string[] contents)
        {
            var startIndex = SkipUntil(contents, "---", 0);
            var endIndex = SkipUntil(contents, "---", startIndex + 1);

            if (endIndex > startIndex)
            {
                var tagValue = FindTagValue(contents, "autocopy", startIndex, endIndex);
                return tagValue.Trim().ToLower() == "true" ? true : false;
            }

            return false;

        }

        public string FindTagValue(string[] contents, string valueToFind, int startIndex, int endIndex)
        {
            var retVal = string.Empty;

            var index = startIndex + 1;
            while (index < endIndex)
            {
                Console.WriteLine(contents[index].Substring(0, valueToFind.Length));
                if (contents[index].Substring(0, valueToFind.Length).ToLower() == valueToFind)
                {
                    return contents[index].Substring(valueToFind.Length + 1);
                }
                index++;
            }
            return retVal;
        }

        public int SkipConfig(string[] contents)
        {
            var index = 0;
            if (contents.Length > 0)
            {
                index = SkipBlank(contents, 0);
                if (contents[index] == "---")
                {
                    index = SkipUntil(contents, "---", index + 1);
                }
            }

            return index == contents.Length ? 0 : index + 1;

        }

        public int SkipUntil(string[] contents, string findthis, int startingAtIndex)
        {
            while (startingAtIndex < contents.Length && !contents[startingAtIndex].StartsWith(findthis))
            {
                startingAtIndex++;
            }

            return startingAtIndex;
        }

        public int SkipBlank(string[] contents, int startingAtIndex)
        {
            while (contents[startingAtIndex] == string.Empty)
            {
                startingAtIndex++;
            }

            return startingAtIndex;
        }

        private List<string> ExtractTagsII(string[] fileContents)
        {
            List<string> retVal = null;
            // 1st three lines of any file can possibly contain tag metadata
            // 
            // Read one line at a time
            // --- <newline>
            // Tags {tag1, tag2} <newline>
            // autocopy : true | false
            // --- <newline>
            //


            var startIndex = SkipUntil(fileContents, "---", 0);
            var endIndex = SkipUntil(fileContents, "---", startIndex + 1);

            if (endIndex > startIndex)
            {
                var tagValue = FindTagValue(fileContents, "tags", startIndex, endIndex);

                if (tagValue == string.Empty) { return retVal; }

                var leftBracket = tagValue.IndexOf('[');
                var rightBracket = tagValue.IndexOf(']');

                var x = tagValue.Substring(leftBracket + 1, rightBracket - leftBracket - 1);

                var list = tagValue.Substring(leftBracket + 1, rightBracket - leftBracket - 1).Split(',');
                retVal = list.ToList<string>();

            }

            return retVal;

        }

        private List<string> ExtractTags(string[] fileContents)
        {
            List<string> retVal = null;
            // 1st three lines of any file can possibly contain tag metadata
            // 
            // Read one line at a time
            // --- <newline>
            // Tags {tag1, tag2} <newline>
            // --- <newline>
            //
            if (fileContents.Length > 0)
            {
                if (fileContents[0] == "---" && fileContents[2] == "---")
                {
                    var space = fileContents[1].IndexOf(' ');
                    if (space > 0)
                    {
                        if (fileContents[1].Substring(0, space).ToLower() == "tags:")
                        {

                            var tmp = fileContents[1].Substring(space, fileContents[1].Length - space);
                            var leftBracket = tmp.IndexOf('[');

                            var taglist = fileContents[1].Substring(leftBracket + space + 1, fileContents[1].Length - leftBracket - 2 - space);

                            var list = taglist.Split(',');
                            retVal = list.ToList<string>();


                        }
                    }
                }
            }

            return retVal;
        }


        private void AutoSizeTextBox(System.Windows.Forms.TextBox txt)
        {
            Size dimensions = GetTextDimensions(txt, txt.Font, txt.Text);

            txt.ScrollBars = dimensions.Height >
                                            txt.Height ?
                                            ScrollBars.Vertical : ScrollBars.None;
        }

        private void textBox1_MouseDown(object sender, MouseEventArgs e)
        {
            _mouseDown = true;
            _lastLocation = e.Location;
        }

        private void textBox1_MouseUp(object sender, MouseEventArgs e)
        {
            _mouseDown = false;
        }

        private void textBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (_mouseDown)
            {
                this.Location = new Point(
                    (this.Location.X - _lastLocation.X) + e.X, (this.Location.Y - _lastLocation.Y) + e.Y);

                this.Update();
            }
        }

        private void textBox2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                //this.Close();
                textBox1_KeyDown(null, e);
                textBox1.Focus();
                textBox1.SelectionStart = 0; //  textBox1.Text.Length;
                textBox1.SelectionLength = 0;
            }
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.Clear(this.BackColor);
            e.Graphics.DrawLine(new Pen(new SolidBrush(Color.DimGray), 1),
                new Point(1, textBox1.Bottom + 9), new Point(this.ClientRectangle.Width, textBox1.Bottom + 9));
            e.Graphics.DrawRectangle(new Pen(new SolidBrush(Color.DimGray), 1),
                new Rectangle(this.ClientRectangle.X,
                              this.ClientRectangle.Y,
                              this.ClientRectangle.Width - 2,
                              this.ClientRectangle.Height - 2));
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveLocation();
        }

        private Size GetTextDimensions(Control control, Font font, string stringData)
        {
            using (Graphics g = control.CreateGraphics())
            {
                SizeF sizeF = g.MeasureString(stringData, font);
                return new Size((int)Math.Ceiling(sizeF.Width), (int)Math.Ceiling(sizeF.Height));
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            AutoSizeTextBox(textBox2);
        }

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            this.Refresh();
            textBox2.Width = this.Width - 15;
            textBox2.Height = this.Height - 60;
            textBox1.Width = this.Width - 80;
          
            customListBox1.Width = this.Width - 6;
            customListBox1.Height = textBox2.Height;
            this.picCopy.Left =  this.ClientRectangle.Right - 36;

        }

        #endregion
     

        public void SetDoubleBuffer(ListBox listbox)
        {
            System.Reflection.PropertyInfo aProp =
         typeof(System.Windows.Forms.Control).GetProperty(
               "DoubleBuffered",
               System.Reflection.BindingFlags.NonPublic |
               System.Reflection.BindingFlags.Instance);

            aProp.SetValue(listbox, true, null);
        }
      
        private void FillRoundedRectangle(Graphics graphics, Brush brush, Rectangle bounds, int cornerRadius)
        {
            if (graphics == null)
                throw new ArgumentNullException(nameof(graphics));
            if (brush == null)
                throw new ArgumentNullException(nameof(brush));

            using (GraphicsPath path = RoundedRect(bounds, cornerRadius))
            {
                graphics.FillPath(brush, path);
            }
        }

        private GraphicsPath RoundedRect(Rectangle bounds, int radius)
        {
            int diameter = radius * 2;
            Size size = new Size(diameter, diameter);
            Rectangle arc = new Rectangle(bounds.Location, size);
            GraphicsPath path = new GraphicsPath();

            if (radius == 0)
            {
                path.AddRectangle(bounds);
                return path;
            }

            // top left arc  
            path.AddArc(arc, 180, 90);

            // top right arc  
            arc.X = bounds.Right - diameter;
            path.AddArc(arc, 270, 90);

            // bottom right arc  
            arc.Y = bounds.Bottom - diameter;
            path.AddArc(arc, 0, 90);

            // bottom left arc 
            arc.X = bounds.Left;
            path.AddArc(arc, 90, 90);

            path.CloseFigure();
            return path;
        }




        private void customListBox1_OnItemSelected(string ItemValue)
        {
            textBox1.Text = ItemValue;
        }

        private void customListBox1_OnEnterPresssed(object sender, EventArgs e)
        {
            LoadCheat();
        }

        private void customListBox1_OnEscapePressed(object sender, KeyEventArgs e)
        {

            textBox1_KeyDown(sender, e);
        }

        private void customListBox1_KeyPress(object sender, KeyPressEventArgs e)
        {

        }

        private void customListBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => { textBox1_KeyDown(sender, e); }));
            }

           
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
//using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Cheat
{
    public partial class Form1 : Form
    {
        private bool _mouseDown;
        private Point _lastLocation;
        // Used to indicate the "Start Typing..." is on display
        //
        private bool _initalState = true;
      
        private string[] _fileNames;
        private Dictionary<string,List<string>> _tags = new Dictionary<string, List<string>>();

        private List<string> _findList = new List<string>();
        private int _findListIndex = -1;
        public Form1()
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.None;
            picCopy.Visible = false;
            
        }


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
                if(!Directory.Exists(sDir)) { return; }

                foreach (string f in Directory.GetFiles(sDir))
                {
                    var dirName = rootDir == Path.GetFileName(sDir) ? string.Empty : Path.GetFileName(sDir);

                    //Console.WriteLine($"File {Path.GetFileName(f)}   Dir {Path.GetFileName(sDir)}");
                    if(dirName == string.Empty)
                    {
                        Console.WriteLine($"{Path.GetFileName(f)}");
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
            var statusMessage = string.Empty;
            try
            {
                this.BackColor = Configuration.BackColor;
                textBox1.BackColor = Configuration.BackColor;
                textBox1.ForeColor = Configuration.ForeColor;
                textBox2.ForeColor = Configuration.ForeColor;
                textBox2.BackColor = Configuration.BackColor;

                var tmpFont = new Font(textBox2.Font.Name,Configuration.FontSizePt);
                textBox2.Font = tmpFont;
                statusMessage = $"reading filesLocaton : {Configuration.FilesLocation}";
                var files = Directory.GetFiles(Configuration.FilesLocation);

                var tmplist = new List<string>();
                statusMessage = "Loading files";
                DirSearch(Configuration.FilesLocation, Path.GetFileName(Configuration.FilesLocation), tmplist);
                _fileNames = tmplist.ToArray();

                textBox1.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                textBox1.AutoCompleteSource = AutoCompleteSource.CustomSource;

                AutoCompleteStringCollection suggestions = new AutoCompleteStringCollection();
                suggestions.AddRange(_fileNames);
                textBox1.AutoCompleteCustomSource = suggestions;

                textBox1.Text = "Start typing...";
                textBox1.Select(0, 0);
                statusMessage = "Setting location";
                SetLocation();

            }
            catch( Exception ex )
            {
                MessageBox.Show($"Error loading while {statusMessage}");
                Application.Exit();
            }
           
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

        private void BuildTagList(string fileName, string pathName, string rootDir)
        {
            var contents = File.ReadAllLines(fileName);
            var pname = Path.GetFileName(fileName);
            var fName =  pathName == rootDir ? Path.GetFileName(fileName) : $"{pathName}/{Path.GetFileName(fileName)}";
            var tags = ExtractTagsII(contents);

            if(tags != null)
            {
                foreach(var t in tags)
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
            }
        }


        
        private void ShowSearch(TextBox textBox, TextBox input)
        {
            // Grab the parameter
            //
            if (input.Text.Length >= 7)
            {
                var param = input.Text.ToLower().Substring(7).Trim();

                if(param.Length < 3)
                {
                    textBox.Clear();
                    textBox.Text = "Search requires phrases at least 3 chars in length";
                    return;
                }

                textBox.Clear();
                foreach(var cheat in _fileNames)
                {
                   
                    var contents = File.ReadAllLines($"{Configuration.FilesLocation}\\{cheat}");
                    foreach(var line in contents)
                    {
                   
                        if (line.ToLower().Contains(param))
                        {
                            textBox.Text += cheat + Environment.NewLine;
                            _findList.Add(cheat);
                            break;
                        }
                    }
                }



            }
        }
        
        
        private void ShowHelp (TextBox textBox)
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
            sb.Append("--version");
            sb.Append(Environment.NewLine);
            sb.Append("   Shows version info");
            sb.Append(Environment.NewLine);

            textBox.Text = sb.ToString();

        }

        private void ShowList(TextBox textBox)
        {
            textBox.Clear();
            foreach (var f in _fileNames)
            {
                textBox.Text += f + Environment.NewLine;
            }

        }
        
        private void ShowConfig(TextBox textBox)
        {
            textBox.Clear();
            textBox.Text = "Configuration File:" + Environment.NewLine;
            textBox.Text += Configuration.ConfigFilePath + "Config.xml" + Environment.NewLine;
            textBox.Text += "----------------------------------" + Environment.NewLine + Environment.NewLine;
            var contents = File.ReadAllLines(Configuration.ConfigFilePath + "Config.xml");

            foreach( var line in contents)
            {
                textBox2.Text += line + Environment.NewLine;
            }

            textBox.Text += Environment.NewLine + "----------------------------------" + Environment.NewLine;

        }

        private void EditConfig(TextBox textBox)
        {
            Process.Start(Configuration.Editor, $"{Configuration.ConfigFilePath}\\Config.xml");
            textBox.Clear();

        }

        private void ShowTags(TextBox textBox)
        {
            textBox.Clear();
            textBox.Text = "Current Tags:" + Environment.NewLine;
            foreach (var t in _tags)
            {
                textBox.Text += "  " + t.Key + Environment.NewLine;
            }
        }
        private void ShowVersion(TextBox textBox)
        {
            textBox.Clear();
            textBox.Text = Environment.NewLine + $" Version :{Application.ProductVersion}";
        }

        private void ShowListTags(TextBox textBox, TextBox input)
        {
            // Grab the paramater
            //
            if (input.Text.Length >= 13)
            {
                var param = input.Text.ToLower().Substring(13).Trim();

                if (_tags.ContainsKey(param))
                {
                    textBox.Clear();
                    textBox.Text = $"Cheats with tag: {param}" + Environment.NewLine;
                    foreach (var t in _tags[param])
                    {
                        textBox.Text += "  " + t + Environment.NewLine;
                    }
                }
            }
        }

        private void ShowEditor(TextBox input)
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

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            Console.WriteLine($"in the text changed event .. text is {textBox1.Text}");
            if (textBox1.Text == string.Empty && !_initalState)
            {
                Console.WriteLine("Text changed");
                textBox1.Text = "Start typing...";
                _initalState = true;
            }
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Down)
            {
                if (_findList.Count > 0)
                {
                    _findListIndex++;
                    if (_findListIndex < 0)
                    {
                        _findListIndex = _findList.Count;
                    }
                    if(_findListIndex >= _findList.Count)
                    {
                        _findListIndex = 0;
                    }
                    textBox1.Text = _findList[_findListIndex];

                    textBox1.SelectAll();

                    e.SuppressKeyPress = true;
                }
                return;
            }
            else if (e.KeyCode == Keys.Up)
            {
                if (_findList.Count > 0)
                {
                    _findListIndex--;

                    if (_findListIndex < 0)
                    {
                        _findListIndex = _findList.Count-1;
                    }
                    if (_findListIndex >= _findList.Count)
                    {
                        _findListIndex = 0;
                    }
                    textBox1.Text = _findList[_findListIndex];

                    textBox1.SelectAll();

                    
                    e.SuppressKeyPress = true;
                }
                return;
            }
            else
            {
                _findList = new List<string>();
                _findListIndex = -1;
            }

            if (e.KeyCode == Keys.Escape && textBox1.Text.Trim() != string.Empty)
            {
                if(_initalState) { this.Close(); }

                textBox1.Text = "";
                _initalState = true;
                return;

            }
            if(e.KeyCode == Keys.Left || e.KeyCode == Keys.Right)
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
                Console.WriteLine("key down .. clearing the text");
                textBox1.Text = "";
                Console.WriteLine("key down .. about to set _initialstate");
                _initalState = false;
            }
            
            if(e.KeyCode== Keys.Enter)
            {

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

                if ( textBox1.Text.Length >= 12 &&  textBox1.Text.ToLower().Substring(0,12).TrimStart() == "--listcheats")
                {
                    ShowListTags(textBox2,textBox1);                    
                    return;
                }

                if(textBox1.Text.ToLower().TrimStart() == "--version"){
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
                    ShowSearch(textBox2,textBox1);
                    return;
                }

                
                var appender = string.Empty;
                if (File.Exists(Configuration.FilesLocation + $"\\{textBox1.Text.TrimStart()}{appender}"))
                {
                    textBox2.Clear();
                    var contents = File.ReadAllLines(Configuration.FilesLocation + $"\\{textBox1.Text.TrimStart()}{appender}");

                    var autoCopyFlag = GetAutoCopyFlag(contents);

                    var index = SkipConfig(contents);

                    if (index > 0 && index <= contents.Length-1)
                    {
                        StringBuilder sb = new StringBuilder();
                        for (int i=index; i<contents.Length; i++)
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

        public bool GetAutoCopyFlag(string[] contents)
        {
            var startIndex = SkipUntil(contents, "---", 0);
            var endIndex = SkipUntil(contents, "---", startIndex+1);

            if(endIndex > startIndex)
            {
                var tagValue = FindTagValue(contents, "autocopy", startIndex, endIndex);
                return tagValue.Trim().ToLower() == "true" || tagValue == string.Empty ? true : false;
            }

            return true;

        }

        public string FindTagValue(string[] contents, string valueToFind, int startIndex, int endIndex)
        {
            var retVal = string.Empty;

            var index = startIndex + 1;
            while(index< endIndex)
            {
                Console.WriteLine(contents[index].Substring(0, valueToFind.Length));
                if(contents[index].Substring(0,valueToFind.Length).ToLower() == valueToFind)
                {
                    return contents[index].Substring(valueToFind.Length+1);
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
                if(contents[index] == "---")
                {
                    index = SkipUntil(contents, "---", index+1);
                }
            }

            return index == contents.Length ? 0 : index+1;

        }

        public int SkipUntil(string[] contents, string findthis, int startingAtIndex)
        {
            while (startingAtIndex <  contents.Length  && contents[startingAtIndex] != findthis)
            {
                startingAtIndex++;
            }

            return startingAtIndex;
        }

        public int SkipBlank(string[] contents, int startingAtIndex)
        {
            while(contents[startingAtIndex] == string.Empty )
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

                if(tagValue == string.Empty) { return retVal; }

                var leftBracket = tagValue.IndexOf('[');
                var rightBracket = tagValue.IndexOf(']');

                var x = tagValue.Substring(leftBracket + 1, rightBracket - leftBracket - 1);

                var list = tagValue.Substring(leftBracket+1,rightBracket-leftBracket-1).Split(',');
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
                if(fileContents[0] == "---" && fileContents[2] == "---")
                {
                    var space = fileContents[1].IndexOf(' ');
                    if(space > 0)
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


        private void AutoSizeTextBox(TextBox txt)
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
                this.Close();
            }
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.Clear(this.BackColor);
            e.Graphics.DrawLine(new Pen(new SolidBrush(Color.DimGray), 1),
                new Point(1,textBox1.Bottom+9), new Point(this.ClientRectangle.Width, textBox1.Bottom+9));
            e.Graphics.DrawRectangle(new Pen(new SolidBrush(Color.DimGray),1),
                new Rectangle(this.ClientRectangle.X,
                              this.ClientRectangle.Y,
                              this.ClientRectangle.Width-2,
                              this.ClientRectangle.Height-2));
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
            textBox2.Width = this.Width - 30;
            textBox2.Height = this.Height - 60;

        }
    }
}

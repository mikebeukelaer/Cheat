using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace Cheat
{
    public partial class Form1 : Form
    {
        private bool _mouseDown;
        private Point _lastLocation;

        private string _FilesLocation;

        private bool _initalState = true;
        private string _configfilePath;

        private string[] _fileNames;
        private Dictionary<string,List<string>> _tags = new Dictionary<string, List<string>>();
        private bool _includeSubDirectories = true;
        public Form1()
        {
            InitializeComponent();
            
        }
        private void DirSearch(string sDir, string rootDir, List<string> list)
        {

            try
            {
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
                if (_includeSubDirectories)
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
            _configfilePath = AppDomain.CurrentDomain.BaseDirectory;
            var configfile = new XmlDocument();

            configfile.Load(_configfilePath + "Config.xml");
            _FilesLocation = configfile.DocumentElement.SelectSingleNode("cheatsfolder").InnerText;
            _includeSubDirectories = configfile.DocumentElement.SelectSingleNode("includeSubDir").InnerText.ToLower() == "true" ? true : false; 


            var files = Directory.GetFiles(_FilesLocation);
            var t = Directory.EnumerateFiles(_FilesLocation, "*.*", SearchOption.AllDirectories);
            var tmplist = new List<string>();
            DirSearch(_FilesLocation,Path.GetFileName(_FilesLocation),tmplist);
            _fileNames = tmplist.ToArray();
            var tmp = new List<string>();
            //foreach(var fi in t)
            //{
            //    tmp.Add(Path.GetFileName(fi));
            //    BuildTagList(fi);
            //}

            //_fileNames = new string[files.Length];

            //for(int i=0; i<files.Length;i++)
            //{
            //    _fileNames[i] = Path.GetFileName(files[i]);
            //}

            textBox1.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            textBox1.AutoCompleteSource = AutoCompleteSource.CustomSource;
            
            AutoCompleteStringCollection suggestions = new AutoCompleteStringCollection();
            suggestions.AddRange(_fileNames);
            textBox1.AutoCompleteCustomSource = suggestions;

            textBox1.GotFocus += TextBox1_GotFocus;
            textBox1.Text = "Start typing...";
            textBox1.Select(0, 0);
            SetLocation();

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
            var tags = ExtractTags(contents);

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

        private void TextBox1_GotFocus(object sender, EventArgs e)
        {
            
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if(textBox1.Text == string.Empty && !_initalState)
            {
                textBox1.Text = "Start typing...";
                _initalState = true;
            }
        }
        private void ShowHelp (TextBox textBox)
        {
            textBox.Clear();
            textBox.Text += "--help" + Environment.NewLine;
            textBox.Text += "   Shows this help information" + Environment.NewLine;
            textBox.Text += "--list" + Environment.NewLine;
            textBox.Text += "   Lists all cheats" + Environment.NewLine;
            textBox.Text += "--config" + Environment.NewLine;
            textBox.Text += "   Shows configuration locations" + Environment.NewLine;
            textBox.Text += "--tags" + Environment.NewLine;
            textBox.Text += "   Lists all known tags" + Environment.NewLine;
            textBox.Text += "--listtags <tag>" + Environment.NewLine;
            textBox.Text += "   Lists all cheats for the given <tag>" + Environment.NewLine;
            textBox.Text += "--version" + Environment.NewLine;
            textBox.Text += "   Shows version info" + Environment.NewLine;

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
            textBox.Text += "   " + _configfilePath + "Config.xml" + Environment.NewLine;
            textBox.Text += "Data Directory:" + Environment.NewLine;
            textBox.Text += "   " + _FilesLocation;
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
            if (input.Text.Length >= 11)
            {
                var param = input.Text.ToLower().Substring(11).Trim();

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
                Process.Start("notepad.exe", $"{_FilesLocation}\\{param}");
                Console.WriteLine($"{_FilesLocation}\\{param}");
            }
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Escape)
            {
                this.Close();
            }

            if (_initalState)
            {
                textBox1.Text = "";
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

                if (textBox1.Text.ToLower().TrimStart() == "--tags")
                {
                    ShowTags(textBox2);
                    return;
                }

                if ( textBox1.Text.Length >= 10 &&  textBox1.Text.ToLower().Substring(0,10).TrimStart() == "--listtags")
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

                //var tag = _locales[textBox1.Text.TrimStart()];
                var appender = string.Empty;
                if (File.Exists(_FilesLocation + $"\\{textBox1.Text.TrimStart()}{appender}"))
                {
                    textBox2.Clear();
                    var contents = File.ReadAllLines(_FilesLocation + $"\\{textBox1.Text.TrimStart()}{appender}");

                    var tags = ExtractTags(contents);
                    
                    if(tags != null)
                    {
                        var index = SkipBlank(contents);

                        for (int i=index; i<contents.Length; i++)
                        {
                             textBox2.Text += contents[i] + Environment.NewLine;
                        }
                    }
                    else
                    {
                        foreach (var c in contents)
                        {
                            textBox2.Text += c + Environment.NewLine;
                        }
                    }
                    

                   
                    Clipboard.SetText(textBox2.Text);
                    
                }
            }
        }

        public int SkipBlank(string[] contents)
        {
            var index = 3;

            while(contents[index] == string.Empty )
            {
                index++;
            }

            return index;
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
    }
}

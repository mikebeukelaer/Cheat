using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Xml;

namespace Cheat
{
    static internal class Configuration
    {

        public static string ConfigFilePath;
        public static string FilesLocation;
        public static bool IncludeSubDirectories;
        public static bool AutoCopyToClipboard;
        public static string Editor;
        public static Color BackColor;
        public static Color ForeColor;
        public static int FontSizePt = 10;

        static Configuration()
        {

            try
            {
                ConfigFilePath = AppDomain.CurrentDomain.BaseDirectory;
                var configfile = new XmlDocument();

                if (File.Exists($"{ConfigFilePath}Config.xml"))
                {
                    
                    configfile.Load(ConfigFilePath + "Config.xml");

                    var _FilesLocation = configfile.DocumentElement.SelectSingleNode("cheatsfolder")?.InnerText;
                    FilesLocation = _FilesLocation;
                    if (_FilesLocation == null)
                    {

                        throw new Exception("Missing node cheatsfoler in config file.");
                    }

                    IncludeSubDirectories =
                        configfile.DocumentElement.SelectSingleNode("includeSubDir").InnerText.ToLower() == "true" ? true : false;

                    AutoCopyToClipboard =
                             configfile.DocumentElement.SelectSingleNode("autocopytoclipboard")?.InnerText == null ?
                                 true : 
                                 configfile.DocumentElement.SelectSingleNode("autocopytoclipboard")?.InnerText.ToLower() == "true" ? 
                                     true : 
                                     false;

                    Editor = configfile.DocumentElement.SelectSingleNode("editor")?.InnerText == null ?
                        "notepad.exe" :
                        configfile.DocumentElement.SelectSingleNode("editor").InnerText;

                    var backcolor = configfile.DocumentElement.SelectSingleNode("backcolor")?.InnerText == null ?
                        "32,32,32" : configfile.DocumentElement.SelectSingleNode("backcolor")?.InnerText;
                    var tmp = backcolor.Split(',');
                    var backColor = Color.FromArgb(int.Parse(tmp[0]), int.Parse(tmp[1]), int.Parse(tmp[2]));
                    BackColor = backColor;

                    var forecolor = configfile.DocumentElement.SelectSingleNode("forecolor")?.InnerText == null ?
                        "32,32,32" : configfile.DocumentElement.SelectSingleNode("forecolor")?.InnerText;

                    tmp = forecolor.Split(',');
                    var foreColor = Color.FromArgb(int.Parse(tmp[0]), int.Parse(tmp[1]), int.Parse(tmp[2]));
                    ForeColor = foreColor;

                    FontSizePt = int.TryParse(configfile.DocumentElement.SelectSingleNode("mainfontsize")?.InnerText, out FontSizePt) ?
                        FontSizePt : 
                        14;

                }
                else
                {
                    throw new Exception($"Cannot load config file {ConfigFilePath}Config.xml");
                }
            }
            catch (Exception ex)
            {
              //  MessageBox.Show(ex.Message,"Error",MessageBoxButtons.OK,MessageBoxIcon.Error);
                throw new Exception(ex.Message);
            }
            
           
        }
    }
}

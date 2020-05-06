﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.IO;
using System.Diagnostics;
using System.Security.Permissions;

namespace DoW_Mod_Manager
{
    public partial class ModManagerForm : Form
    {
        public struct GameExecutable
        {
            public string Soulstorm, DarkCrusade;

            public GameExecutable(string SS, string DC)
            {
                Soulstorm = SS;
                DarkCrusade = DC;
            }
        }

        private const int IMAGE_FILE_LARGE_ADDRESS_AWARE = 0x20;                   //32 in Decimal
        private const string CONFIG_FILE_NAME = "DoW Mod Manager.ini";
        private const string CHOICE_INDEX = "ChoiceIndex";
        private const string DEV = "Dev";
        private const string NO_MOVIES = "NoMovies";
        private const string FORCE_HIGH_POLY = "ForceHighPoly";
        private const string DEV_COMMAND = " -dev";
        private const string NO_MOVIES_COMMAND = " -nomovies";
        private const string FORCE_HIGH_POLY_COMMAND = " -forcehighpoly";

        private readonly string currentDir = Directory.GetCurrentDirectory();      //Is the current Directory of Soulstorm

        private string devMode = "";                                               //Contains the argument for starting the .exe in dev mode
        private string noIntroMode = "";                                           //Contains the argument for starting the .exe with no Intromovies
        private string highPolyMode = "";                                          //Contains the argument for starting the .exe in High Poly Mode.
        private bool[] isInstalled;                                                //A boolean array that maps Index-wise to the filepaths indices. Index 0 checks if required mod at index 0 in the _filepaths is installed or not.
        private bool isGameEXELAAPatched = false;                                  //Tells if soulstorm is LAA patched or NOT.
        private bool isGraphicsConfigLAAPatched = false;                           //Tells if graphicsconfig is LAA patched or NOT.
        private GameExecutable gameExe = new GameExecutable("Soulstorm.exe", "DarkCrusade.exe");
        private string currentGameEXE = "";

        public string[] FilePaths;                                                 //Stores the paths of the found .module files in the Soulstorm directory
        public string[] ModFolderPaths;                                            //Stores the paths of the Required Mods stored within the .module files. This will be used to check for their actual presence/absence in the Soulstorm Dir.
        public List<string> AllFoundModules = null;                                //Contains the list of all available Mods that will be used by the Mod Merger
        public List<string> AllValidModules = null;                                //Contains the list of all playable Mods that will be used by the Mod Merger

        readonly Dictionary<string, int> settings = new Dictionary<string, int> 
        {
            [CHOICE_INDEX] = 0,
            [DEV]         = 0,
            [NO_MOVIES]    = 1,
            [FORCE_HIGH_POLY]    = 0
        };

        /// <summary>
        ///  Initializes all the necessary components used by the GUI
        /// </summary>
        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public ModManagerForm()
        {
            InitializeComponent();

            if (File.Exists(CONFIG_FILE_NAME))
            {
                var lines = File.ReadLines(CONFIG_FILE_NAME);
                int indexOfEqualSign;
                int lastIndexOfEqualSign;
                string setting;
                int value;
                
                // Reading every line of config file and trying ignore or correct all the possible mistakes
                foreach (string line in lines)
                {
                    indexOfEqualSign = line.IndexOf('=');
                    lastIndexOfEqualSign = line.LastIndexOf('=');

                    if (indexOfEqualSign == lastIndexOfEqualSign)
                    {
                        if (indexOfEqualSign > 0)
                        {
                            setting = Convert.ToString(line.Substring(0, indexOfEqualSign));
                            try
                            {
                                value = Convert.ToInt32(line.Substring(indexOfEqualSign + 1, line.Length - indexOfEqualSign - 1));
                            }
                            catch (Exception)
                            {
                                value = 0;
                            }

                            if (setting == CHOICE_INDEX)
                            {
                                if (value >= 0)
                                    settings[setting] = value;
                                else
                                    settings[setting] = 0;
                            }

                            if (setting == DEV || setting == NO_MOVIES || setting == FORCE_HIGH_POLY)
                            {
                                if (value == 0 || value == 1)
                                    settings[setting] = value;
                                else
                                {
                                    if (value > 1)
                                        settings[setting] = 1;
                                    else
                                        settings[setting] = 0;
                                }
                            }
                        }
                    }
                }
            }

            if (Convert.ToBoolean(settings[DEV]))
                devMode = DEV_COMMAND;
            if (Convert.ToBoolean(settings[NO_MOVIES]))
                noIntroMode = NO_MOVIES_COMMAND;
            if (Convert.ToBoolean(settings[FORCE_HIGH_POLY]))
                highPolyMode = FORCE_HIGH_POLY_COMMAND;
        }

        /// <summary>
        /// Performs actions when the Application is started via the .exe file.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ModManagerForm_Load(object sender, EventArgs e)
        {
            currentGameEXE = GetCurrentGameEXE();
            CheckForGraphicsConfigEXE();
            FilePaths = Directory.GetFiles(currentDir, currentGameEXE);

            currentDirTextBox.AppendText(currentDir);
            SetUpAllNecessaryMods();
            isGameEXELAAPatched = IsLargeAware(Directory.GetFiles(currentDir, currentGameEXE)[0]);
            isGraphicsConfigLAAPatched = IsLargeAware(Directory.GetFiles(currentDir, "GraphicsConfig.exe")[0]);
            SetSoulstormLAALabelText();
            SetGraphicsConfigLAALabelText();
            AddFileSystemWatcher();

            // Initialize values with values from previous values or defaults.
            ReselectSavedMod();

            devCheckBox.Checked = Convert.ToBoolean(settings[DEV]);
            nomoviesCheckBox.Checked = Convert.ToBoolean(settings[NO_MOVIES]);
            highpolyCheckBox.Checked = Convert.ToBoolean(settings[FORCE_HIGH_POLY]);
        }

        private void ModManagerForm_Closing(object sender, EventArgs e)
        {
            WriteAllSettingsInFile();
        }

        private void WriteAllSettingsInFile()
        {
            string str = $"{CHOICE_INDEX}={settings[CHOICE_INDEX]}\n" +
                $"{DEV}={settings[DEV]}\n" +
                $"{NO_MOVIES}={settings[NO_MOVIES]}\n" +
                $"{FORCE_HIGH_POLY}={settings[FORCE_HIGH_POLY]}";
            File.WriteAllText(CONFIG_FILE_NAME, str);
        }

        /// <summary>
        /// This function handles the reselection of a previously selected mod.
        /// </summary>
        private void ReselectSavedMod()
        {
            int index = settings[CHOICE_INDEX];

            if (InstalledModsList.Items.Count > index)
            {
                InstalledModsList.SelectedIndex = index;
            }
            else
            {
                InstalledModsList.SelectedIndex = InstalledModsList.Items.Count - 1;
            }
        }

        /// <summary>
        /// This adds FileSystem watcher to capture any file changes in the game directories.
        /// </summary>
        private void AddFileSystemWatcher()
        {
            fileSystemWatcher1.Path = currentDir;

            fileSystemWatcher1.NotifyFilter = NotifyFilters.LastAccess
                                    | NotifyFilters.LastWrite
                                    | NotifyFilters.FileName
                                    | NotifyFilters.DirectoryName;

            fileSystemWatcher1.Changed += FileSystemWatcherOnChanged;
            fileSystemWatcher1.Created += FileSystemWatcherOnChanged;
            fileSystemWatcher1.Deleted += FileSystemWatcherOnChanged;
            fileSystemWatcher1.Renamed += FileSystemWatcherOnChanged;

            // Begin watching.
            fileSystemWatcher1.EnableRaisingEvents = true;
        }

        /// <summary>
        /// This function defines the event handlers for when some file was changed.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private void FileSystemWatcherOnChanged(object source, FileSystemEventArgs e)
        {
            SetUpAllNecessaryMods();
        }

        /// <summary>
        /// A refactored wrapper method that is used to initialize or refresh the Mod Managers main page
        /// </summary>
        public void SetUpAllNecessaryMods()
        {
            GetMods();
            GetModFoldersFromFile();
            ReselectSavedMod();
        }

        /// <summary>
        /// Checks if the Mods are actually REALLY installed by checking if their asset folders are present by the name specified within the .module files "Modfolder" tagline
        /// </summary>
        public void CheckforInstalledMods()
        {
            startButton.Enabled = true;

            string str_Path = Path.GetFullPath(currentDir + "\\DoW Mod Manager Resources\\Checkmark.png");
            try
            {
                pictureBox.Image = Image.FromFile(str_Path);
            }
            catch
            {
                MessageBox.Show("ERROR! COULD NOT FIND FOLDER 'DoW Mod Manager Resources' IN THIS DIRECTORY!");
                Application.Exit();
                return;
            }

            int counter = 0;
            string localstring;
            string folderPath;
            isInstalled = new bool[RequiredModsList.Items.Count];

            while (counter < RequiredModsList.Items.Count)
            {
                folderPath = currentDir + "\\" + ModFolderPaths[counter];

                if (Directory.Exists(folderPath))
                {
                    localstring = RequiredModsList.Items[counter].ToString();
                    RequiredModsList.Items.RemoveAt(counter);
                    RequiredModsList.Items.Insert(counter, localstring + "...INSTALLED!");
                    isInstalled[counter] = true;
                }
                else
                {
                    localstring = RequiredModsList.Items[counter].ToString();
                    RequiredModsList.Items.RemoveAt(counter);
                    RequiredModsList.Items.Insert(counter, localstring + "...MISSING!");
                    isInstalled[counter] = false;
                    startButton.Enabled = false;

                    str_Path = Path.GetFullPath(currentDir + "\\DoW Mod Manager Resources\\cross.png");
                    pictureBox.Image = Image.FromFile(str_Path);
                }
                counter++;
            }
        }

        /// <summary>
        /// This method returns the last word from an inputstring by using regex. For example using "RequiredMod.1 = Yourmod" will result in "Yourmod" beeing returned
        /// </summary>
        /// <param name="text"></param>
        /// <returns>string</returns>
        private string GetLastEntryFromLine(string text)
        {
            string pattern = @"\S*\s*$";
            string result = "";

            // Instantiate the regular expression object.
            Regex require = new Regex(pattern, RegexOptions.IgnoreCase);

            // Match the regular expression pattern against a text string.
            Match match = require.Match(text);

            if (match.Success)
            {
                result = match.Value.Replace(" ", "");
            }
            return result;
        }

        /// <summary>
        /// Returns the name of the Module file without it's extension and all it's whitespaces removed.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private string GetModFolderFromFile(string text)
        {
            string pattern = @"\S*\s*$";
            string result = "";

            // Instantiate the regular expression object.
            Regex require = new Regex(pattern, RegexOptions.IgnoreCase);

            // Match the regular expression pattern against a text string.
            Match match = require.Match(text);

            if (match.Success)
            {
                result = match.Value.Replace(" ", "");
                result = match.Value.Replace(".module", "");
            }
            return result;
        }

        /// <summary>
        /// Finds all installed .module files and displays them in the Installed Mods Listbox without their .module extension
        /// </summary>
        private void GetMods()
        {
            List<string> newfilePathsArray = new List<string>();        //Make a new list for the new Pathitems
            AllFoundModules = new List<string>();
            AllValidModules = new List<string>();

            int Index = 0;
            InstalledModsList.Items.Clear();
            string line;

            FilePaths = Directory.GetFiles(currentDir, "*.module");
            if (FilePaths.Length != 0)
            {
                foreach (string s in FilePaths)
                {
                    //Find the List of ALL found module files for the Mod Merger available Mods List
                    AllFoundModules.Add(Path.GetFileNameWithoutExtension(s));

                    // Read the .module file to see if the mod is playable
                    using (StreamReader file = new StreamReader(s))
                    {
                        // Filter the unplayable mods and populate the List only with playable mods
                        while ((line = file.ReadLine()) != null)
                        {
                            if (ModIsPlayable(line))
                            {
                                newfilePathsArray.Add(FilePaths[Index]);
                                InstalledModsList.Items.Add(Path.GetFileNameWithoutExtension(s));
                                AllValidModules.Add(Path.GetFileNameWithoutExtension(s));
                            }
                        }
                    }
                    Index++;
                }
                FilePaths = newfilePathsArray.ToArray();        //Override the old array that contained unplayable mods with the new one.
            }
            if (FilePaths.Length == 0 || AllFoundModules.Count == 0)
            {
                MessageBox.Show("No mods were found in the specified directory! Please check your current directory again!");
                Application.Exit();
                return;
            }
        }

        /// <summary>
        /// This function returns 'true' if a Mod is set as "Playable = 1" in the .module file 
        /// </summary>
        /// <param name="textline"></param>
        /// <returns></returns>
        private bool ModIsPlayable(string textline)
        {
            string pattern = @"Playable = 1";

            // Instantiate the regular expression object.
            Regex require = new Regex(pattern, RegexOptions.IgnoreCase);

            // Match the regular expression pattern against a text string.
            Match match = require.Match(textline);

            if (match.Success)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Reads the .module file and scans for "RequiredMods" lines and returns if a line was found or not via true/false. This is used to add the lines to the Form Window.
        /// </summary>
        /// <param name="text"></param>
        /// <returns>bool</returns>
        private bool GetRequiredMod(string text)
        {
            string pattern = @"\bRequiredMod\b";
            string patternCommented1 = @"^[;]+";
            string patternCommented2 = @"^[\/]+";
            bool state = false;

            foreach (Match match in Regex.Matches(text, pattern))
            {
                state = true;
            }
            foreach (Match match in Regex.Matches(text, patternCommented1))
            {
                state = false;
            }
            foreach (Match match in Regex.Matches(text, patternCommented2))
            {
                state = false;
            }
            return state;
        }

        /// <summary>
        /// Checks if a line contains the Word "Modfolder" with true/false
        /// </summary>
        /// <param name="str"></param>
        /// <returns>bool</returns>
        private bool CheckRegexModFolderExist(string text)
        {
            string pattern = @"ModFolder";
            bool state = false;

            foreach (Match match in Regex.Matches(text, pattern))
            {
                state = true;
            }
            return state;
        }

        /// <summary>
        /// When selecting a different installed Mod, update the required mods Listbox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InstalledModsList_SelectedIndexChanged(object sender, EventArgs e)
        {
            startButton.Enabled = true;

            int index = InstalledModsList.SelectedIndex;
            if (index < 0 || index >= InstalledModsList.Items.Count)
            {
                index = settings[CHOICE_INDEX];
                InstalledModsList.SelectedIndex = index;
            }
            settings[CHOICE_INDEX] = index;

            string currentPath = FilePaths[index];
            string line;

            RequiredModsList.Items.Clear();

            // Read the file and display it line by line.
            using (StreamReader file = new StreamReader(currentPath))
            {
                // Populate the Required Mods List with entries from the .module file
                while ((line = file.ReadLine()) != null)
                {
                    if (GetRequiredMod(line) == true)
                    {
                        RequiredModsList.Items.Add(line);
                    }
                }

                GetModFoldersFromFile();
                CheckforInstalledMods();
            }
        }

        /// <summary>
        ///  This one checks if the Mod Assett folders, specified in each .module file, do actually exist
        /// </summary>
        private void GetModFoldersFromFile()
        {
            int index = 0;
            int count = 0;
            string currentPath;
            string line;
            ModFolderPaths = new string[RequiredModsList.Items.Count];

            // Read the file and display it line by line.
            while (index < RequiredModsList.Items.Count)
            {
                currentPath = currentDir + "\\" + GetLastEntryFromLine(RequiredModsList.Items[index].ToString()) + ".module";
                if (File.Exists(currentPath))
                {
                    using (StreamReader file = new StreamReader(currentPath))
                    {
                        while ((line = file.ReadLine()) != null)
                        {
                            if (CheckRegexModFolderExist(line) == true)
                            {
                                ModFolderPaths[count] = GetModFolderFromFile(line);
                                count++;
                            }
                        }
                    }
                }
                else
                {
                    ModFolderPaths[count] = "MISSING";
                    count++;
                }
                index++;
            }
        }

        /// <summary>
        /// This is the button to start the vanilla unmodded base game.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StartVanillaGameButton_Click(object sender, EventArgs e)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = currentDir + "\\" + currentGameEXE,
                Arguments = @"-modname W40k" + devMode + noIntroMode + highPolyMode
            };
            Process.Start(startInfo);
        }

        /// <summary>
        /// This is the actual start button with which you can start the game with the currently selected mod
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StartButton_Click(object sender, EventArgs e)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = currentDir + "\\" + currentGameEXE,
                Arguments = @"-modname " + InstalledModsList.SelectedItem + devMode + noIntroMode + highPolyMode
            };
            Process.Start(startInfo);
        }

        /// <summary>
        /// This is the checkbox that controls the starting option '-dev'. 
        /// It allows for additional debug options in-game and log files.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DevCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (devCheckBox.Checked)
            {
                devMode = DEV_COMMAND;
            }
            else
            {
                devMode = "";
            }
            settings[DEV] = Convert.ToInt32(devCheckBox.Checked);
        }

        /// <summary>
        /// This is the checkbox that controls the starting option '-nomovies'. 
        /// It prevents any movies/intros from being played.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NomoviesCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (nomoviesCheckBox.Checked)
            {
                noIntroMode = NO_MOVIES_COMMAND;
            }
            else
            {
                noIntroMode = "";
            }
            settings[NO_MOVIES] = Convert.ToInt32(nomoviesCheckBox.Checked);
        }

        /// <summary>
        /// This is the checkbox that controls the starting option '-forcehighpoly'. 
        /// This disabled the LOD system and will display the highes mesh detail at any distance.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HighpolyCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (highpolyCheckBox.Checked == true)
            {
                highPolyMode = FORCE_HIGH_POLY_COMMAND;
            }
            else
            {
                highPolyMode = "";
            }
            settings[FORCE_HIGH_POLY] = Convert.ToInt32(highpolyCheckBox.Checked);
        }

        /// <summary>
        /// This function collects and displays the list of required mods for a selected mod in order to function correctly.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RequiredModsList_DrawItem(object sender, DrawItemEventArgs e)
        {
            // Draw the background of the ListBox control for each item.
            // Create a new Brush and initialize to a Black colored brush
            // by default.
            e.DrawBackground();

            // Determine the color of the brush to draw each item based on 
            // the index of the item to draw. Could be extended for an Orange Brush for indicating outdated Mods.
            Brush myBrush = Brushes.Black;

            switch (isInstalled[e.Index])
            {
                case true:
                    myBrush = Brushes.Green;
                    break;
                case false:
                    myBrush = Brushes.Red;
                    break;
            }

            // Draw the current item text based on the current 
            // Font and the custom brush settings.
            e.Graphics.DrawString(((ListBox)sender).Items[e.Index].ToString(), e.Font, myBrush, e.Bounds, StringFormat.GenericDefault);

            // If the ListBox has focus, draw a focus rectangle 
            // around the selected item.
            e.DrawFocusRectangle();
        }

        /// <summary>
        /// This is function opens the Mod Merger form when the button is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ModMergeButton_Click(object sender, EventArgs e)
        {
            ModMergerForm mergerWindow = new ModMergerForm(this);
            mergerWindow.Show();
        }

        /// <summary>
        /// This function draws the LAA text for the Soulstorm label depending on whether the flag is true (Green) or false (Red).
        /// </summary>
        private void SetSoulstormLAALabelText()
        {
            if (isGameEXELAAPatched)
            {
                SoulstormLAAStatusLabel.Text = currentGameEXE + ": LAA Active";
                SoulstormLAAStatusLabel.ForeColor = Color.Green;
            }
            else
            {
                SoulstormLAAStatusLabel.Text = currentGameEXE + ": LAA Inactive";
                SoulstormLAAStatusLabel.ForeColor = Color.Red;
            }
        }

        /// <summary>
        /// This function draws the LAA text for the GraphicsConfig label depending on whether the flag is true (Green) or false (Red).
        /// </summary>
        private void SetGraphicsConfigLAALabelText()
        {
            if (isGraphicsConfigLAAPatched)
            {
                GraphicsConfigLAAStatusLabel.Text = "GraphicsConfig.exe: LAA Active";
                GraphicsConfigLAAStatusLabel.ForeColor = Color.Green;
            }
            else
            {
                GraphicsConfigLAAStatusLabel.Text = "GraphicsConfig.exe: LAA Inactive";
                GraphicsConfigLAAStatusLabel.ForeColor = Color.Red;
            }
        }

        /// <summary>
        /// This function instigates the test if a given EXE is LAA patched or not.
        /// </summary>
        /// <param name="file"></param>
        /// <returns>bool</returns>
        static bool IsLargeAware(string file)
        {
            using (FileStream fs = File.OpenRead(file))
            {
                return IsLargeAware(fs);
            }
        }

        /// <summary>
        /// This function performs the data probing to determine if a given file is LAA patched or not.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns>bool</returns>
        static bool IsLargeAware(Stream stream)
        {
            using (BinaryReader br = new BinaryReader(stream))
            {
                if (br.ReadInt16() != 0x5A4D)       //No MZ Header
                    return false;

                br.BaseStream.Position = 0x3C;
                int peloc = br.ReadInt32();         //Get the PE header location.

                br.BaseStream.Position = peloc;
                if (br.ReadInt32() != 0x4550)       //No PE header
                    return false;

                br.BaseStream.Position += 0x12;
                short LAAFlag = br.ReadInt16();

                return (LAAFlag & IMAGE_FILE_LARGE_ADDRESS_AWARE) == IMAGE_FILE_LARGE_ADDRESS_AWARE;
            }
        }

        /// <summary>
        /// This function performs the necessary data operations in order to toggle the LAA for a given EXE file back and forth.
        /// </summary>
        /// <param name="file"></param>
        /// <returns>bool</returns>
        static bool ToggleLAA(string file)
        {
            bool result = false;
            using (FileStream fs = File.Open(file, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
            {
                BinaryReader br = new BinaryReader(fs); 
                BinaryWriter bw = new BinaryWriter(fs);

                if (br.ReadInt16() != 0x5A4D)       //No MZ Header
                    return result;

                br.BaseStream.Position = 0x3C;
                int peloc = br.ReadInt32();         //Get the PE header location.

                br.BaseStream.Position = peloc;
                if (br.ReadInt32() != 0x4550)       //No PE header
                    return result;

                br.BaseStream.Position += 0x12;     //LAA flag position
                long nFilePos = (int)br.BaseStream.Position;
                short LAAFlag = br.ReadInt16();
                br.BaseStream.Position += 0x40;     //Checksum position
                long nSumPos = (int)br.BaseStream.Position;
                short ChckSum = br.ReadInt16();

                if ((LAAFlag & IMAGE_FILE_LARGE_ADDRESS_AWARE) != IMAGE_FILE_LARGE_ADDRESS_AWARE)
                {
                    LAAFlag += IMAGE_FILE_LARGE_ADDRESS_AWARE;
                    ChckSum += IMAGE_FILE_LARGE_ADDRESS_AWARE;
                    bw.Seek((int)nFilePos, SeekOrigin.Begin);
                    bw.Write(LAAFlag);
                    bw.Seek((int)nSumPos, SeekOrigin.Begin);
                    bw.Write(ChckSum);
                    bw.Flush();
                    result = true;
                }
                else if ((LAAFlag & IMAGE_FILE_LARGE_ADDRESS_AWARE) == IMAGE_FILE_LARGE_ADDRESS_AWARE)
                {
                    LAAFlag -= IMAGE_FILE_LARGE_ADDRESS_AWARE;
                    ChckSum -= IMAGE_FILE_LARGE_ADDRESS_AWARE;
                    bw.Seek((int)nFilePos, SeekOrigin.Begin);
                    bw.Write(LAAFlag);
                    bw.Seek((int)nSumPos, SeekOrigin.Begin);
                    bw.Write(ChckSum);
                    bw.Flush();
                    result = false;
                }
                br.Close();
                bw.Close();
            }
            return result;
        }

        /// <summary>
        /// This function checks if a file is yet still opened and thus blocked.
        /// It prevents crashes when attempting to write to files not yet closed.
        /// </summary>
        /// <param name="file"></param>
        /// <returns>bool</returns>
        private bool IsFileLocked(string file)
        {
            FileStream stream = null;
            try
            {
                stream = File.Open(file, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }

            //file is not locked
            return false;
        }

        /// <summary>
        /// This function handles the proper toggling of the LAA flag for the Soulstorm.exe and the GraphicsConfig.exe.
        /// It can handle the cases when users have previously patched the EXE files only partially.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonToggleLAA_Click(object sender, EventArgs e)
        {
            //Check if the Game is LAA Patched and fill in the Label properly
            string curDirSoul = Directory.GetFiles(currentDir, currentGameEXE)[0];
            string curDirGraph = Directory.GetFiles(currentDir, "GraphicsConfig.exe")[0];
            if (!IsFileLocked(curDirSoul) && !IsFileLocked(curDirGraph))
            {
                if ((isGameEXELAAPatched && isGraphicsConfigLAAPatched) || (!isGameEXELAAPatched && !isGraphicsConfigLAAPatched))
                {
                    isGameEXELAAPatched = ToggleLAA(curDirSoul);
                    isGraphicsConfigLAAPatched = ToggleLAA(curDirGraph);
                }
                else if (!isGameEXELAAPatched)
                {
                    isGameEXELAAPatched = ToggleLAA(curDirSoul);
                }
                else if (!isGraphicsConfigLAAPatched)
                {
                    isGraphicsConfigLAAPatched = ToggleLAA(curDirGraph);
                }

                SetSoulstormLAALabelText();
                SetGraphicsConfigLAALabelText();
            }
        }

        /// <summary>
        /// This function scans for either the Soulstorm or the Dark Crusade version of the game.
        /// </summary>
        private string GetCurrentGameEXE()
        {
            string[] curDir = Directory.GetFiles(currentDir, gameExe.Soulstorm);
            if (curDir.Length != 0)
            {
                return gameExe.Soulstorm;
            }

            curDir = Directory.GetFiles(currentDir, gameExe.DarkCrusade);
            if (curDir.Length != 0)
            {
                return gameExe.DarkCrusade;
            }

            MessageBox.Show("ERROR: Neither found the Soulstorm.exe nor the DarkCrusade.exe in this directory!");
            Application.Exit();
            return "";
        }

        private void CheckForGraphicsConfigEXE()
        {
            string[] curDir = Directory.GetFiles(currentDir, "GraphicsConfig.exe");
            if (curDir.Length == 0)
            {
                MessageBox.Show("ERROR: GraphicsConfig.exe was not found!");
                Application.Exit();
            }
        }
    }
}

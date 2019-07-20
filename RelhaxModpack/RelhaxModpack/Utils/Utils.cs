﻿using Ionic.Zip;
using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml;
using System.Xml.Linq;
using IWshRuntimeLibrary;
using File = System.IO.File;
using System.Windows.Threading;
using RelhaxModpack.AtlasesCreator;
using System.Drawing;
using Size = System.Drawing.Size;
using RelhaxModpack.Windows;
using System.Threading;
using System.Windows.Media.Imaging;

namespace RelhaxModpack
{

    public enum ReplacementTypes
    {
        FilePath,
        PatchArguements,
        PatchFiles,
        TextEscape,
        TextUnescape,
        ZipFilePath
    }

    public struct BeforeAfter
    {
        public SelectablePackage Before;
        public SelectablePackage After;
    }

    /// <summary>
    /// A utility class for static functions used in various places in the modpack
    /// </summary>
    public static class Utils
    {
        #region Statics
        public const int TO_SECONDS = 1000;
        public const int TO_MINUETS = 60;
        public static readonly string[] SizeSuffixes = { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
        public const long BYTES_TO_MBYTES = 1048576;
        //MACROS
        //FilePath macro
        //https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/how-to-initialize-a-dictionary-with-a-collection-initializer
        //build at install time
        public static Dictionary<string, string> FilePathDict = new Dictionary<string, string>();
        public static Dictionary<string, string> PatchArguementsDict = new Dictionary<string, string>()
        {
            {@"[sl]", @"/" }
        };
        public static Dictionary<string, string> PatchFilesDict = new Dictionary<string, string>()
        {
            //add all patch file escape characters
            //key (look for), value (replaced with)
            {@"""[xvm_dollar]", @"$" },
            {@"[xvm_rbracket]""", @"}" },
            {@"[lbracket]", @"{" },
            {@"[rbracket]", @"}" },
            {@"[quote]", "\"" },
            {@"[colon]", @":" },
            {@"[dollar]", @"$" },
        };
        public static Dictionary<string, string> TextUnscapeDict = new Dictionary<string, string>()
        {
            //ORDER MATTERS
            {@"\n", "\n" },
            {@"\r", "\r" },
            {@"\t", "\t" },
            //legacy compatibility (i can't believe i did this....)
            {@"newline", "\n" }
        };
        public static Dictionary<string, string> TextEscapeDict = new Dictionary<string, string>()
        {
            //ORDER MATTERS
            {"\n", @"\n" },
            {"\r", @"\r" },
            {"\t", @"\t" }
        };
        #endregion

        #region Unmanaged Library stuff

        public static RelhaxFreeImageLibrary FreeImageLibrary = new RelhaxFreeImageLibrary();

        public static RelhaxNvTexLibrary NvTexLibrary = new RelhaxNvTexLibrary();

        public static string GetAssemblyName(string keyword)
        {
            return Assembly.GetExecutingAssembly().GetManifestResourceNames().FirstOrDefault(rn => rn.Contains(keyword));
        }

        #endregion

        public struct VersionInfos
        {
            public string WoTClientVersion;
            public string WoTOnlineFolderVersion;
            public override string ToString()
            {
                return string.Format("WoTClientVersion={0}, WoTOnlineFolderVersion={1}", WoTClientVersion, WoTOnlineFolderVersion);
            }
        }

        #region Application Utils
        /// <summary>
        /// Return the entire assembely version
        /// </summary>
        /// <returns>The entire assembely version string (major, minor, build, revision)</returns>
        public static string GetApplicationVersion()
        {
            return Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }
        /// <summary>
        /// Return the date and time in EN-US form, the time that the application was built
        /// </summary>
        /// <returns>the application build date and time in EN-US form</returns>
        public static string GetCompileTime()
        {
            return CiInfo.BuildTag + " (EN-US date format)";
        }

        public static async Task<XmlDocument> GetManagerInfoDocumentAsync()
        {
            XmlDocument doc = null;
            //delete the last one and download a new one
            using (WebClient client = new WebClient())
            {
                try
                {
                    if (File.Exists(Settings.ManagerInfoDatFile))
                        File.Delete(Settings.ManagerInfoDatFile);
                    await client.DownloadFileTaskAsync(Settings.ManagerInfoURLBigmods, Settings.ManagerInfoDatFile);
                }
                catch (Exception e)
                {
                    Logging.Exception("Failed to check for updates: \n{0}", e);
                    MessageBox.Show(Translations.GetTranslatedString("failedCheckUpdates"));
                    return null;
                }
            }

            //get the version info string
            string xmlString = Utils.GetStringFromZip(Settings.ManagerInfoDatFile, "manager_version.xml");
            if (string.IsNullOrEmpty(xmlString))
            {
                Logging.WriteToLog("Failed to get get xml string from managerInfo.dat", Logfiles.Application, LogLevel.ApplicationHalt);
                Application.Current.Shutdown();
                return null;
            }

            return XMLUtils.LoadXmlDocument(xmlString, XmlLoadType.FromString);
        }

        public static async Task<bool> IsManagerUptoDate(string currentVersion)
        {
            //actually compare the build of the application of the requested distribution channel
            XmlDocument doc = await GetManagerInfoDocumentAsync();
            if (doc == null)
            {
                Logging.Error("failed to get manager online version");
                return false;
            }

            string applicationOnlineVersion = (ModpackSettings.ApplicationDistroVersion == ApplicationVersions.Stable) ?
                XMLUtils.GetXMLStringFromXPath(doc, "//version/relhax_v2_stable").Trim() ://stable
                XMLUtils.GetXMLStringFromXPath(doc, "//version/relhax_v2_beta").Trim();//beta

            Logging.Info("Current build is {0} Online build is {1}", currentVersion, applicationOnlineVersion);

            //check if versions are equal
            return currentVersion.Equals(applicationOnlineVersion);
        }
        #endregion

        #region Window Utils
        /// <summary>
        /// Get a list of all visual components in the window
        /// </summary>
        /// <param name="window">The window to get the list of</param>
        /// <param name="includeWindow">if the list should include the window itself</param>
        /// <returns>A list of type FrameowrkElement of all components</returns>
        public static List<FrameworkElement> GetAllWindowComponentsVisual(Window window, bool includeWindow)
        {
            //https://stackoverflow.com/questions/874380/wpf-how-do-i-loop-through-the-all-controls-in-a-window
            List<FrameworkElement> windowComponents = new List<FrameworkElement>();
            if (includeWindow)
                windowComponents.Add(window);
            if (VisualTreeHelper.GetChildrenCount(window) > 0)
                GetAllWindowComponentsVisual(window, windowComponents);
            return windowComponents;
        }
        public static List<FrameworkElement> GetAllWindowComponentsLogical(Window window, bool includeWindow)
        {
            List<FrameworkElement> windowComponents = new List<FrameworkElement>();
            if (includeWindow)
                windowComponents.Add(window);
            GetAllWindowComponentsLogical(window, windowComponents);
            return windowComponents;
        }
        public static List<FrameworkElement> GetAllWindowComponentsLogical(FrameworkElement rootElement, bool addRoot)
        {
            List<FrameworkElement> components = new List<FrameworkElement>();
            if (addRoot)
                components.Add(rootElement);
            GetAllWindowComponentsLogical(rootElement, components);
            return components;
        }
        //A recursive method for navigating the visual tree
        private static void GetAllWindowComponentsVisual(FrameworkElement v, List<FrameworkElement> allWindowComponents)
        {
            int ChildrenComponents = VisualTreeHelper.GetChildrenCount(v);
            for (int i = 0; i < ChildrenComponents; i++)
            {
                DependencyObject dep = VisualTreeHelper.GetChild(v, i);
                if(!(dep is FrameworkElement))
                {
                    continue;
                }
                FrameworkElement subV = (FrameworkElement)VisualTreeHelper.GetChild(v, i);
                allWindowComponents.Add(subV);
                if (subV is TabControl tabControl)
                {
                    foreach(FrameworkElement tabVisual in tabControl.Items)
                    {
                        allWindowComponents.Add(tabVisual);
                        GetAllWindowComponentsLogical(tabVisual, allWindowComponents);
                    }
                }
                int childrenCount = VisualTreeHelper.GetChildrenCount(subV);
                if (childrenCount > 0)
                    GetAllWindowComponentsVisual(subV, allWindowComponents);
            }
        }
        //Gets any logical components that are not currently shown (like elemnts behind a tab)
        private static void GetAllWindowComponentsLogical(FrameworkElement v, List<FrameworkElement> allWindowComponents)
        {
            //NOTE: v has been added
            //have to use var here cause i got NO CLUE what type it is #niceMeme
            var children = LogicalTreeHelper.GetChildren(v);
            //Type temp = children.GetType();
            foreach (var child in children)
            {
                //Type temp2 = child.GetType();
                if (child is FrameworkElement childVisual)
                {
                    allWindowComponents.Add(childVisual);
                    GetAllWindowComponentsLogical(childVisual, allWindowComponents);
                }
            }
        }

        /// <summary>Checks if a point is inside the possible monitor space</summary>
        /// <param name="x">The x coordinate of the point</param>
        /// <param name="y">The y coordinate of the point</param>
        public static bool PointWithinScreen(int x, int y)
        {
            return PointWithinScreen(new System.Drawing.Point(x, y));
        }

        /// <summary>Checks if a point is inside the possible monitor space</summary>
        /// <param name="p">The point to check</param>
        public static bool PointWithinScreen(System.Drawing.Point p)
        {
            //if either x or y are negative it's an invalid location
            if (p.X < 0 || p.Y < 0)
                return false;
            int totalWidth = 0, totalHeight = 0;
            foreach (System.Windows.Forms.Screen s in System.Windows.Forms.Screen.AllScreens)
            {
                totalWidth += s.Bounds.Width;
                totalHeight += s.Bounds.Height;
            }
            if (totalWidth > p.X && totalHeight > p.Y)
                return true;
            return false;
        }

        //https://stackoverflow.com/questions/37787388/how-to-force-a-ui-update-during-a-lengthy-task-on-the-ui-thread
        //https://stackoverflow.com/questions/2329978/the-calling-thread-must-be-sta-because-many-ui-components-require-this
        public static void AllowUIToUpdate()
        {
            DispatcherFrame frame = new DispatcherFrame();
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Render, new DispatcherOperationCallback(delegate (object parameter)
            {
                frame.Continue = false;
                return null;
            }), null);

            Dispatcher.PushFrame(frame);
            //EDIT:
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background,
                                          new Action(delegate { }));
        }

        public static void ApplyApplicationScale(Window window, double scaleValue)
        {
            //input filtering
            if(scaleValue < Settings.MinimumDisplayScale)
            {
                Logging.Warning("scale size of {0} is to small, setting to 1", scaleValue.ToString("N"));
                scaleValue = Settings.MinimumDisplayScale;
            }
            if (scaleValue > Settings.MaximumDisplayScale)
            {
                Logging.Warning("scale size of {0} is to large, setting to 3", scaleValue.ToString("N"));
                scaleValue = Settings.MaximumDisplayScale;
            }
            //scale internals
            (window.Content as FrameworkElement).LayoutTransform = new ScaleTransform(scaleValue, scaleValue, 0, 0);
            //scale window itself
            if (window is MainWindow mw)
            {
                mw.Width = mw.OriginalWidth * scaleValue;
                mw.Height = mw.OriginalHeight * scaleValue;
            }
            else if (window is RelhaxWindow rw)
            {
                rw.Width = rw.OriginalWidth * scaleValue;
                rw.Height = rw.OriginalHeight * scaleValue;
            }
            else
                throw new BadMemeException("you should probably make me a RelhaxWindow if you want to use this feature");
        }
        #endregion

        #region File Utilities
        /// <summary>
        /// Creates an MD5 hash calculation of the input file
        /// </summary>
        /// <param name="inputFile">The path to the file to calculate</param>
        /// <returns></returns>
        public static string CreateMD5Hash(string inputFile)
        {
            if (string.IsNullOrWhiteSpace(inputFile))
                return "-1";
            //first, return if the file does not exist
            if (!File.Exists(inputFile))
                return "-1";
            //Create a new Stringbuilder to collect the bytes
            StringBuilder sBuilder = new StringBuilder();
            MD5 md5Hash;
            FileStream stream;
            try
            {
                using (md5Hash = MD5.Create())
                using (stream = File.OpenRead(inputFile))
                {
                    //Convert the input string to a byte array and compute the hash
                    byte[] data = md5Hash.ComputeHash(stream);
                    stream.Close();
                    
                    //Loop through each byte of the hashed data 
                    //and format each one as a hexadecimal string.
                    for (int i = 0; i < data.Length; i++)
                    {
                        sBuilder.Append(data[i].ToString("x2"));
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.Warning("Failed to check crc of local file " + inputFile + ex.ToString());
                return "-1";
            }
            //Return the hexadecimal string.
            return sBuilder.ToString();
        }
        /// <summary>
        /// Creates an MD5 hash calculation of the input file
        /// </summary>
        /// <param name="inputFile">The path to the file to calculate</param>
        /// <returns></returns>
        public static async Task<string> CreateMD5HashAsync(string inputFile)
        {
            //https://docs.microsoft.com/en-us/dotnet/standard/parallel-programming/task-based-asynchronous-programming
            //Task taskA = Task.Run( () => Console.WriteLine("Hello from taskA."));
            //https://stackoverflow.com/questions/38423472/what-is-the-difference-between-task-run-and-task-factory-startnew
            /*
                in the .NET Framework 4.5 Developer Preview, we’ve introduced the new Task.Run method. This in no way obsoletes Task.Factory.StartNew,
                but rather should simply be thought of as a quick way to use Task.Factory.StartNew without needing to specify a bunch of parameters.
                It’s a shortcut. In fact, Task.Run is actually implemented in terms of the same logic used for Task.Factory.StartNew, just passing in
                some default parameters. When you pass an Action to Task.Run:

                'Task.Run(someAction);'

                it's exactly equivalent to:

                'Task.Factory.StartNew(someAction, CancellationToken.None, TaskCreationOptions.DenyChildAttach, TaskScheduler.Default);'
             */
            return await Task.Run(() => CreateMD5Hash(inputFile));
        }
        /// <summary>
        /// Gets a zip file entry in the form of a string
        /// </summary>
        /// <param name="zipFilename">The path to the file in the zip</param>
        /// <param name="archivedFilename">the path to the zip file</param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static string GetStringFromZip(string zipFilename, string archivedFilename, string password = "")
        {
            // https://social.msdn.microsoft.com/Forums/vstudio/en-US/92a36534-0f01-4425-ab63-c5f8830d64ae/help-please-with-dotnetzip-extracting-data-form-ziped-file?forum=csharpgeneral
            if(!File.Exists(zipFilename))
            {
                Logging.Error(string.Format("ERROR: {0} not found", zipFilename));
                return null;
            }
            string textStr = "";
            using (ZipFile zip = ZipFile.Read(zipFilename))
            using (MemoryStream ms = new MemoryStream() { Position=0 })
            using (StreamReader sr = new StreamReader(ms))
            {
                ZipEntry e = zip[archivedFilename];
                if (!string.IsNullOrWhiteSpace(password))
                    e.ExtractWithPassword(ms, password);
                else
                    e.Extract(ms);
                ms.Position = 0;
                textStr = sr.ReadToEnd();
            }
            return textStr;
        }

        public static bool ProcessEmptyDirectories(string startLocation, bool recursive, uint numRetrys = 3, uint timeout = 100)
        {
            //if the root does not exist then stop now
            if (!Directory.Exists(startLocation))
                return true;

            //check to make sure the number of retries is between 1 and 10
            if (numRetrys < 1)
            {
                Logging.WriteToLog(string.Format("numRetrys is invalid (below 1), setting to 1 (numRetryes={0})", numRetrys),
                    Logfiles.Application, LogLevel.Warning);
                numRetrys = 1;
            }
            if (numRetrys > 10)
            {
                Logging.WriteToLog(string.Format("numRetrys is invalid (above 10), setting to 10 (numRetryes={0})", numRetrys),
                    Logfiles.Application, LogLevel.Warning);
                numRetrys = 10;
            }

            uint retryCounter = 0;
            if (recursive)
            {
                //get the list of all directories inside it, no need to recursively process
                List<string> directories = DirectorySearch(startLocation, SearchOption.AllDirectories,false).ToList().Where(direct => Directory.Exists(direct)).ToList();

                //sort and reverse the list to make longer paths on top to simulate recursively deleting from all the way down to up
                directories.Sort();
                directories.Reverse();

                //now can delete for each folder
                foreach (string directory in directories)
                {
                    retryCounter = 0;
                    while (retryCounter < numRetrys)
                    {
                        try
                        {
                            if (Directory.GetFiles(directory).Length == 0 && Directory.GetDirectories(directory).Length == 0)
                            {
                                Logging.Debug("Deleting empty directory {0}", directory);
                                Directory.Delete(directory, false);
                            }
                        }
                        catch (Exception ex)
                        {
                            Logging.Warning("failed to delete {0}, retryCount={1}, message:\n{2}", directory, retryCounter, ex.Message);
                            retryCounter++;
                            System.Threading.Thread.Sleep((int)timeout);
                            if (retryCounter == numRetrys)
                            {
                                Logging.Error("retries = counter, fully failed to delete directory {0}", directory);
                                return false;
                            }
                        }
                    }
                }
            }

            //and process the root
            retryCounter = 0;
            while (retryCounter < numRetrys)
            {
                try
                {
                    if (Directory.GetFiles(startLocation).Length == 0 && Directory.GetDirectories(startLocation).Length == 0)
                    {
                        Logging.Debug("Deleting empty directory {0}", startLocation);
                        Directory.Delete(startLocation, false);
                    }
                    retryCounter = numRetrys;
                }
                catch (Exception ex)
                {
                    Logging.Warning("failed to delete {0}, retryCount={1}, message:\n{2}", startLocation, retryCounter, ex.Message);
                    retryCounter++;
                    System.Threading.Thread.Sleep((int)timeout);
                    if (retryCounter == numRetrys)
                    {
                        Logging.Error("retries = counter, fully failed to delete directory {0}", startLocation);
                        return false;
                    }
                }
            }
            return true;
        }
        
        public static string SizeSuffix(ulong value, uint decimalPlaces = 1, bool sizeSuffix = false)
        {
            if (value == 0)
            {
                if (sizeSuffix)
                    return "0.0 bytes";
                else
                    return "0.0";
            }

            if (value < 1000)
            {
                if (sizeSuffix)
                    return string.Format("{0:n" + decimalPlaces + "} {1}", 0.1, SizeSuffixes[1]);
                else
                    return string.Format("{0:n" + decimalPlaces + "}", 0.1);
            }

            // mag is 0 for bytes, 1 for KB, 2, for MB, etc.
            int mag = (int)Math.Log(value, 1024);

            // 1L << (mag * 10) == 2 ^ (10 * mag) 
            // [i.e. the number of bytes in the unit corresponding to mag]
            decimal adjustedSize = (decimal)value / (1L << (mag * 10));

            // make adjustment when the value is large enough that
            // it would round up to 1000 or more
            if (Math.Round(adjustedSize, (int)decimalPlaces) >= 1000)
            {
                mag += 1;
                adjustedSize /= 1024;
            }

            if (sizeSuffix)
                return string.Format("{0:n" + decimalPlaces + "} {1}", adjustedSize, SizeSuffixes[mag]);
            else
                return string.Format("{0:n" + decimalPlaces + "}", adjustedSize);
        }

        public static long GetFilesize(string filepath)
        {
            //https://stackoverflow.com/questions/1380839/how-do-you-get-the-file-size-in-c
            return new FileInfo(filepath).Length;
        }

        /// <summary>
        /// Checks if a filename has invalid characters and replaces them with underscores
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string GetValidFilename(string fileName)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(c, '_');
            }
            return fileName;
        }

        public static bool FileDelete(string file, uint numRetrys = 3, uint timeout = 100)
        {
            bool overallSuccess = true;
            //check to make sure the number of retries is between 1 and 10
            if (numRetrys < 1)
            {
                Logging.WriteToLog(string.Format("numRetrys is invalid (below 1), setting to 1 (numRetryes={0})", numRetrys),
                    Logfiles.Application, LogLevel.Warning);
                numRetrys = 1;
            }
            if (numRetrys > 10)
            {
                Logging.WriteToLog(string.Format("numRetrys is invalid (above 10), setting to 10 (numRetryes={0})", numRetrys),
                    Logfiles.Application, LogLevel.Warning);
                numRetrys = 10;
            }
            uint retryCounter = 0;
            while (retryCounter < numRetrys)
            {
                try
                {
                    File.Delete(file);
                    retryCounter = numRetrys;
                }
                catch (Exception ex)
                {
                    Logging.WriteToLog(string.Format("failed to delete {0}, retryCount={1}, message:\n{2}", file, retryCounter, ex.Message),
                        Logfiles.Application, LogLevel.Warning);
                    retryCounter++;
                    System.Threading.Thread.Sleep((int)timeout);
                    if (retryCounter == numRetrys)
                    {
                        Logging.Error("retries = counter, fully failed to delete file {0}", file);
                        overallSuccess = false;
                    }
                }
            }
            return overallSuccess;
        }

        public static bool DirectoryDelete(string folderPath, bool deleteSubfolders, bool deleteRoot = true, uint numRetrys = 3, uint timeout = 100, string pattern = "*")
        {
            bool overallSuccess = true;
            //check to make sure the number of retries is between 1 and 10
            if (numRetrys < 1)
            {
                Logging.WriteToLog(string.Format("numRetrys is invalid (below 1), setting to 1 (numRetryes={0})", numRetrys),
                    Logfiles.Application, LogLevel.Warning);
                numRetrys = 1;
            }
            if (numRetrys > 10)
            {
                Logging.WriteToLog(string.Format("numRetrys is invalid (above 10), setting to 10 (numRetryes={0})", numRetrys),
                    Logfiles.Application, LogLevel.Warning);
                numRetrys = 10;
            }
            uint retryCounter = 0;
            foreach (string file in Directory.GetFiles(folderPath,pattern,SearchOption.TopDirectoryOnly))
            {
                retryCounter = 0;
                while (retryCounter < numRetrys)
                {
                    try
                    {
                        File.Delete(file);
                        retryCounter = numRetrys;
                    }
                    catch(Exception ex)
                    {
                        Logging.WriteToLog(string.Format("failed to delete {0}, retryCount={1}, message:\n{2}", file, retryCounter, ex.Message),
                            Logfiles.Application,LogLevel.Warning);
                        retryCounter++;
                        System.Threading.Thread.Sleep((int)timeout);
                        if(retryCounter == numRetrys)
                        {
                            Logging.Error("retries = counter, fully failed to delete file {0}",file);
                            overallSuccess = false;
                        }
                    }
                }
            }
            //if deleting the sub directories
            if (deleteSubfolders)
            {
                foreach (string dir in Directory.GetDirectories(folderPath,pattern,SearchOption.TopDirectoryOnly))
                {
                    if (!DirectoryDelete(dir, deleteSubfolders, true, numRetrys, timeout))
                        overallSuccess = false;
                }
            }
            //delete the folder as well (if requested)
            if(deleteRoot)
            {
                retryCounter = 0;
                while (retryCounter < numRetrys)
                {
                    try
                    {
                        Directory.Delete(folderPath);
                        retryCounter = numRetrys;
                    }
                    catch (Exception ex)
                    {
                        Logging.WriteToLog(string.Format("failed to delete {0} (empty folder), retryCount={1}, message:\n{2}", folderPath, retryCounter, ex.Message),
                            Logfiles.Application, LogLevel.Error);
                        retryCounter++;
                        System.Threading.Thread.Sleep((int)timeout);
                        if (retryCounter == numRetrys)
                        {
                            Logging.Debug("retries = counter, fully failed to delete file {0}", folderPath);
                            overallSuccess = false;
                        }
                    }
                }
            }
            return overallSuccess;
        }

        public static async Task DirectoryDeleteAsync(string folderPath, bool deleteSubfolders, bool deleteRoot = true, uint numRetrys = 3, uint timeout = 100, string pattern = "*")
        {
            //Task taskA = Task.Run( () => Console.WriteLine("Hello from taskA."));
            await Task.Run(() => DirectoryDelete(folderPath, deleteSubfolders, deleteRoot, numRetrys, timeout, pattern));
        }

        public static void DirectoryMove(string source, string destination, bool recursive, uint numRetrys = 3, uint timeout = 100, string pattern = "*")
        {
            //make the destination if it does not already exist
            if (!Directory.Exists(destination))
                Directory.CreateDirectory(destination);

            //DirectoryMove works by getting a directory list of all directories in the source to create,
            //then making the directories, moving the files, and then deleting the old directories
            List<string> directoreisToCreate = Directory.GetDirectories(source, pattern,
                recursive? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly).ToList();

            //create them at the target
            foreach(string fullPath in directoreisToCreate)
            {
                //trim out the base path so we only have the new path left
                string partPath = fullPath.Substring(source.Length+1);
                string newPath = Path.Combine(destination, partPath);
                if(!Directory.Exists(newPath))
                {
                    Directory.CreateDirectory(newPath);
                }
            }

            //move the files over
            List<string> filesToMove = Directory.GetFiles(source, pattern,
                recursive? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly).ToList();
            foreach(string file in filesToMove)
            {
                string partPath = file.Substring(source.Length + 1);
                string newPath = Path.Combine(destination, partPath);
                File.Move(file, newPath);
            }

            //delete all the other old empty source directories
            directoreisToCreate.Sort();
            directoreisToCreate.Reverse();
            foreach(string fullPath in directoreisToCreate)
            {
                if(Directory.Exists(fullPath))
                {
                    if (Directory.GetFiles(fullPath,"*",SearchOption.AllDirectories).Count() > 0)
                        throw new BadMemeException("waaaaaaa?");
                    Directory.Delete(fullPath);
                }
            }
        }

        public static void DirectoryCopy(string source, string destination, bool recursive, uint numRetrys = 3, uint timeout = 100, string pattern = "*")
        {
            List<string> directoreisToCreate = Directory.GetDirectories(source, pattern,
                recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly).ToList();
            //create them at the target
            foreach (string s in directoreisToCreate)
            {
                if (!Directory.Exists(Path.Combine(destination, s)))
                {
                    Directory.CreateDirectory(Path.Combine(destination, s));
                }
            }
            //copy the files over
            List<string> filesToMove = Directory.GetFiles(source, pattern,
                recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly).ToList();
            foreach (string file in filesToMove)
            {
                File.Copy(Path.Combine(source, file), Path.Combine(destination, file));
            }
        }

        public static string[] DirectorySearch(string directoryPath, SearchOption option, bool includeDirectoryRoot, string searchPattern = "*",
            uint timeout = 5, uint numRetrys = 3, bool applyFolderProperties = true)
        {
            //filter input
            if(numRetrys == 0)
            {
                Logging.Warning("numRetrys needs to be larger than 0! setting to 1");
                numRetrys++;
            }
            //loop for how many times to try (in case the OS herped a derp, for example)
            while(numRetrys > 0)
            {
                //if a timout is requested, then sleep the thread
                if (timeout > 0)
                    System.Threading.Thread.Sleep((int)timeout);
                //put it in a try catch block
                try
                {
                    if(!Directory.Exists(directoryPath))
                    {
                        Logging.WriteToLog(string.Format("Path {0} does not exist!", directoryPath), Logfiles.Application, LogLevel.Error);
                        return null;
                    }
                    if (applyFolderProperties)
                        File.SetAttributes(directoryPath, FileAttributes.Directory);
                    //add the directory path itself to the search
                    List<string> files = Directory.GetFiles(directoryPath, searchPattern, option).ToList();
                    if(includeDirectoryRoot)
                        files.Insert(0, directoryPath);
                    return files.ToArray();
                }
                catch (Exception e)
                {
                    //decreate the number of times we will retry to get the files
                    numRetrys--;
                    if(numRetrys == 0)
                    {
                        //give up; report it and move on
                        Logging.WriteToLog(string.Format("Failed to get files fo directory {0}\n{1}", Path.GetFullPath(directoryPath), e.ToString()),
                            Logfiles.Application, LogLevel.Exception);
                        return null;
                    }
                    else
                    {
                        Logging.WriteToLog(string.Format("Failed to get files for direcotry {0}\nThis is attempt {1} of 0",
                            Path.GetFullPath(directoryPath), numRetrys), Logfiles.Application, LogLevel.Warning);
                    }
                }
            }
            Logging.WriteToLog("Code shuld not reach this point: Utils.DirectorySearch()", Logfiles.Application, LogLevel.Warning);
            return null;
        }

        public static void ApplyNormalFileProperties(string file)
        {
            //check to make sure it's eithor a file or folder
            if (!File.Exists(file) && !Directory.Exists(file))
            {
                Logging.WriteToLog("file/folder does not exist " + file, Logfiles.Application, LogLevel.Error);
                return;
            }
            try
            {
                FileAttributes attribute = File.GetAttributes(file);
                if (attribute != FileAttributes.Normal)
                {
                    Logging.WriteToLog(string.Format("file {0} has FileAttribute {1}, setting to FileAttributes.Normal",
                        file, attribute.ToString()), Logfiles.Application, LogLevel.Debug);
                    File.SetAttributes(file, FileAttributes.Normal);
                }
            }
            catch (Exception e)
            {
                Logging.WriteToLog("Failed to apply normal attribute\n" + e.ToString(), Logfiles.Application, LogLevel.Exception);
                return;
            }
        }
        #endregion

        #region Data type from string processing/parsing
        /// <summary>
        /// Try to parse a boolean value based on string input
        /// </summary>
        /// <param name="input">the string to try to parse</param>
        /// <param name="defaultValue">the default value to use if parsing fails</param>
        /// <returns>The bool value of the ipnut string, or the default value if parsing failes</returns>
        public static bool ParseBool(string input, bool defaultValue)
        {
            if (bool.TryParse(input, out bool result))
                return result;
            else return defaultValue;
        }

        public static bool ParseBool(string input, out bool result, bool defaultValue = false)
        {
            if (bool.TryParse(input, out result))
                return true;
            else
                return false;
        }

        /// <summary>
        /// Try to parse an intiger value based on string input
        /// </summary>
        /// <param name="input">the string to try to parse</param>
        /// <param name="defaultValue">the default value to use if parsing fails</param>
        /// <returns>The int value of the ipnut string, or the default value if parsing failes</returns>
        public static int ParseInt(string input, int defaultValue)
        {
            if (int.TryParse(input, out int result))
                return result;
            else return defaultValue;
        }

        public static bool ParseInt(string input, out int result, int defaultValue = 0)
        {
            if (int.TryParse(input, out result))
                return true;
            else
                return false;
        }

        /// <summary>
        /// Try to parse a float value based on string input
        /// </summary>
        /// <param name="input">the string to try to parse</param>
        /// <param name="defaultValue">the default value to use if parsing fails</param>
        /// <returns>The float value of the ipnut string, or the default value if parsing failes</returns>
        public static float ParseFloat(string input, float defaultValue)
        {
            if (float.TryParse(input,NumberStyles.Float,CultureInfo.InvariantCulture,out float result))
                return result;
            else return defaultValue;
        }

        public static bool ParseFloat(string input, out float result, float defaultValue = 0)
        {
            if (float.TryParse(input, NumberStyles.Float,CultureInfo.InvariantCulture,out result))
                return true;
            else
                return false;
        }

        public static long ParseLong(string input, long defaultValue)
        {
            if (long.TryParse(input, out long result))
                return result;
            else return defaultValue;
        }

        public static bool ParseLong(string input, out long result, long defaultValue = 0)
        {
            if (long.TryParse(input, out result))
                return true;
            else
                return false;
        }

        public static ulong ParseuLong(string input, ulong defaultValue)
        {
            if (ulong.TryParse(input, out ulong result))
                return result;
            else return defaultValue;
        }

        public static bool ParseuLong(string input, out ulong result, ulong defaultValue = 0)
        {
            if (ulong.TryParse(input, out result))
                return true;
            else
                return false;
        }

        //https://stackoverflow.com/questions/10685794/how-to-use-generic-tryparse-with-enum
        public static TEnum ParseEnum<TEnum>(string input, TEnum defaultValue)
            where TEnum : struct, IConvertible
        {
            if (Enum.TryParse(input, true, out TEnum result))
                return result;
            else return defaultValue;
        }
        #endregion

        #region Database Utils
        public static List<string> CheckForDuplicates(List<DatabasePackage> globalDependencies, List<Dependency> dependencies,
            List<Category> parsedCategoryList, List<Dependency> logicalDependencies = null)
        {
            List<string> duplicatesList = new List<string>();
            List<DatabasePackage> flatList = GetFlatList(globalDependencies, dependencies, logicalDependencies, parsedCategoryList);
            foreach(DatabasePackage package in flatList)
            {
                List<DatabasePackage> packagesWithPackagename = flatList.Where(item => item.PackageName.Equals(package.PackageName)).ToList();
                if (packagesWithPackagename.Count > 1)
                    duplicatesList.Add(package.PackageName);
            }
            return duplicatesList;
        }

        public static bool IsDuplicateName(List<DatabasePackage> packagesToCheckWith, string nameToCheck)
        {
            foreach(DatabasePackage package in packagesToCheckWith)
            {
                if (package.PackageName.Equals(nameToCheck))
                    return true;
            }
            return false;
        }

        //processes sorting categories by using the sorting property and
        public static void SortDatabase(List<Category> parsedCategoryList)
        {
            //the first level of packages are always sorted
            foreach(Category cat in parsedCategoryList)
            {
                SortDatabase(cat.Packages);
            }
        }

        private static void SortDatabase(List<SelectablePackage> packages)
        {
            //sorts packages in alphabetical order
            packages.Sort(SelectablePackage.CompareModsName);
            //if set in the database, child elements can be sorted as well
            foreach(SelectablePackage child in packages)
            {
                if (child.SortChildPackages)
                {
                    Logging.Debug("Sorting packages of package {0}", child.PackageName);
                    SortDatabase(child.Packages);
                }
            }
        }

        /// <summary>
        /// Links all the refrences (like parent, etc) for each class object making it possible to traverse the list tree in memory
        /// </summary>
        /// <param name="ParsedCategoryList">The List of categories</param>
        public static void BuildLinksRefrence(List<Category> ParsedCategoryList, bool buildFakeParents)
        {
            foreach (Category cat in ParsedCategoryList)
            {
                if(buildFakeParents)
                {
                    cat.CategoryHeader = new SelectablePackage()
                    {
                        Name = string.Format("----------[{0}]----------", cat.Name),
                        TabIndex = cat.TabPage,
                        ParentCategory = cat,
                        Type = SelectionTypes.multi,
                        Visible = true,
                        Enabled = true,
                        Level = -1,
                        PackageName = string.Format("Category_{0}_Header", cat.Name.Replace(' ', '_')),
                        Packages = cat.Packages
                    };
                }
                foreach (SelectablePackage sp in cat.Packages)
                {
                    BuildLinksRefrence(sp, cat, cat.CategoryHeader);
                }
            }
        }
        /// <summary>
        /// Links all the refrences (like parent, etc) for each class object making it possible to traverse the list tree in memory
        /// </summary>
        /// <param name="sp">The package to perform linking on</param>
        /// <param name="cat">The category that the SelectablePackagesp belongs to</param>
        /// <param name="parent">The tree parent of sp</param>
        private static void BuildLinksRefrence(SelectablePackage sp, Category cat, SelectablePackage parent)
        {
            sp.Parent = parent;
            sp.TopParent = cat.CategoryHeader;
            sp.ParentCategory = cat;
            if (sp.Packages.Count > 0)
            {
                foreach (SelectablePackage sp2 in sp.Packages)
                {
                    BuildLinksRefrence(sp2, cat, sp);
                }
            }
        }
        public static void BuildLevelPerPackage(List<Category> ParsedCategoryList, int startingLevel = 0)
        {
            //root level direct form category is 0
            foreach (Category cat in ParsedCategoryList)
            {
                foreach (SelectablePackage package in cat.Packages)
                {
                    package.Level = startingLevel;
                    if (package.Packages.Count > 0)
                        //increase the level BEFORE it calls the method
                        BuildLevelPerPackage(package.Packages, startingLevel+ 1);
                }
            }
        }
        private static void BuildLevelPerPackage(List<SelectablePackage> packages, int level)
        {
            foreach (SelectablePackage package in packages)
            {
                package.Level = level;
                if (package.Packages.Count > 0)
                    //increase the level BEFORE it calls the method
                    BuildLevelPerPackage(package.Packages, level+1);
            }
        }
        public static List<Dependency> CalculateDependencies(List<Dependency> dependencies, List<SelectablePackage> packages, List<Category> parsedCategoryList)
        {
            //flat list is packages
            //1- build the list of calling mods that need it
            List<Dependency> dependenciesToInstall = new List<Dependency>();

            Logging.Debug("Starting step 1 of 4 in dependency calculation: adding from categories");
            foreach (Dependency dependency in dependencies)
            {
                foreach (Category category in parsedCategoryList)
                {
                    foreach (DatabaseLogic logic in category.Dependencies)
                    {
                        if(logic.PackageName.Equals(dependency.PackageName))
                        {
                            Logging.Debug("Category \"{0}\" uses dependency \"{1}\" (logic type {2})", category.Name, dependency.PackageName, logic.Logic);
                            dependency.DatabasePackageLogic.Add(new DatabaseLogic()
                            {
                                PackageName = category.Name,
                                willBeInstalled = category.AnyPackagesChecked(),
                                Logic = logic.Logic,
                                NotFlag = logic.NotFlag
                            });
                        }
                    }
                }
            }
            Logging.Debug("step 1 complete");

            Logging.Debug("Starting step 2 of 4 in dependency calculation: adding from selectable packages that use each dependency");
            foreach(Dependency dependency in dependencies)
            {
                //for each dependency, go through each package, and in each package...
                //Logging.Debug("processing dependency {0}", dependency.PackageName);
                foreach(SelectablePackage package in packages)
                {
                    //got though each logic property. if the package called is this dependency, then add it to it's list
                    foreach(DatabaseLogic logic in package.Dependencies)
                    {
                        if(logic.PackageName.Equals(dependency.PackageName))
                        {
                            Logging.Debug("SelectablePackage \"{0}\" uses dependency \"{1}\" (logic type {2})", package.PackageName, dependency.PackageName, logic.Logic);
                            dependency.DatabasePackageLogic.Add(new DatabaseLogic()
                            {
                                //set PackageName to the selectablepackage package name so later we know where this logic entry came from
                                PackageName = package.PackageName,
                                willBeInstalled = package.Checked,
                                Logic = logic.Logic,
                                NotFlag = logic.NotFlag
                            });
                        }
                    }
                }
            }
            Logging.Debug("step 2 complete");

            //2- append with list of dependencies that need it, regardless if it's an error or not
            Logging.Debug("Starting step 3 of 4 in dependency calculation: adding dependencies that use each dependency");
            foreach (Dependency dependency in dependencies)
            {
                //Logging.Debug("processing dependency {0}", dependency.PackageName);
                //for each dependency go through each dependency's package logic and if it's called then add it
                foreach(Dependency processingDependency in dependencies)
                {
                    if (processingDependency.PackageName.Equals(dependency.PackageName))
                        continue;
                    foreach(DatabaseLogic logic in processingDependency.Dependencies)
                    {
                        if(logic.PackageName.Equals(dependency.PackageName))
                        {
                            Logging.Debug("Dependency \"{0}\" uses dependency \"{1}\" (logic type {2})", processingDependency.PackageName, dependency.PackageName, logic.Logic);
                            dependency.DatabasePackageLogic.Add(new DatabaseLogic()
                            {
                                PackageName = processingDependency.PackageName,
                                //by default, dependences that are dependent on dependencies start as false until proven needed
                                willBeInstalled = false,
                                Logic = logic.Logic,
                                NotFlag = logic.NotFlag
                            });
                        }
                    }
                }
            }
            Logging.Debug("step 3 complete");

            //3 - run calculations IN DEPENDENCY LIST ORDER FROM TOP DOWN
            List<Dependency> notProcessedDependnecies = new List<Dependency>(dependencies);
            Logging.Debug("Starting step 4 of 4 in dependency calculation: calculating dependencies from top down (perspective to list)");
            foreach (Dependency dependency in dependencies)
            {
                //first check if this dependency is referencing a dependency that has not yet been processed
                //if so then note it in the log
                Logging.Debug("Calculating if dependency {0} will be installed",dependency.PackageName);
                foreach(DatabaseLogic login in dependency.DatabasePackageLogic)
                {
                    List<Dependency> matches = notProcessedDependnecies.Where(dep => login.PackageName.Equals(dep.PackageName)).ToList();
                    if(matches.Count > 0)
                    {
                        string errorMessage = string.Format("dependency {0} is referencing the dependency {1} which has not yet been processed!" +
                            "This will lead to logic errors in database calculation! (Tip: this dependency ({0}) should be BELOW ({1}) in the" +
                            "list of dependencies in the editor. (Order matters!)",dependency.PackageName, login.PackageName);
                        Logging.Debug(errorMessage);
                        if (ModpackSettings.DatabaseDistroVersion == DatabaseVersions.Test)
                            MessageBox.Show(errorMessage);
                    }
                }

                //two types of logics - OR and AND (with nots)
                //each can be calculated separately
                List<DatabaseLogic> localOR = dependency.DatabasePackageLogic.Where(logic => logic.Logic == Logic.OR).ToList();
                List<DatabaseLogic> logicalAND = dependency.DatabasePackageLogic.Where(logic => logic.Logic == Logic.AND).ToList();

                //debug logging
                Logging.Debug("logicalOR count: {0}", localOR.Count);
                Logging.Debug("logicalAnd count: {0}", logicalAND.Count);

                //if there are no logical ands, then only do ors, vise versa
                bool ORsPass = localOR.Count > 0? false: true;
                bool ANDSPass = logicalAND.Count > 0? false:true;

                //if ors and ands are both true already, then something's broken
                if(ORsPass && ANDSPass)
                {
                    Logging.Warning("ors and ands already pass for dependency package {0}, (nothing uses it?)", dependency.PackageName);
                    continue;
                }

                //calc the ORs first
                Logging.Debug("processing OR logic");
                foreach(DatabaseLogic orLogic in localOR)
                {
                    //OR logic - if any mod/dependency is checked, then it's installed and can stop there
                    //because only one of them needs to be true
                    //same case goes for negatives - if mod is NOT checked and negateFlag
                    if(!orLogic.willBeInstalled)
                    {
                        Logging.WriteToLog(string.Format("skipping logic check of package {0} because it is not set for installation!", orLogic.PackageName),
                            Logfiles.Application, LogLevel.Debug);
                        continue;
                    }
                    if(!orLogic.NotFlag)
                    {
                        Logging.WriteToLog(string.Format("package {0} is checked and notflag is low (= true), sets orLogic to pass!", orLogic.PackageName),
                            Logfiles.Application, LogLevel.Debug);
                        ORsPass = true;
                        break;
                    }
                    else if (orLogic.NotFlag)
                    {
                        Logging.WriteToLog(string.Format("package {0} is NOT checked and notFlag is high (= false), sets orLogic to pass!", orLogic.PackageName),
                            Logfiles.Application, LogLevel.Debug);
                        ORsPass = true;
                        break;
                    }
                    else
                    {
                        Logging.WriteToLog(string.Format("package {0}, checked={1}, notFlag={2}, does not set orLogic to pass",
                            orLogic.PackageName, orLogic.willBeInstalled, orLogic.NotFlag), Logfiles.Application, LogLevel.Debug);
                    }
                }

                //now calc the ands
                Logging.Debug("processing AND logic");
                foreach(DatabaseLogic andLogic in logicalAND)
                {
                    if (andLogic.willBeInstalled && !andLogic.NotFlag)
                    {
                        Logging.WriteToLog(string.Format("package {0} is checked and (NOT notFlag) = true, correct AND logic, continue",
                            andLogic.PackageName), Logfiles.Application, LogLevel.Debug);
                        ANDSPass = true;
                    }
                    else if (!andLogic.willBeInstalled && andLogic.NotFlag)
                    {
                        Logging.WriteToLog(string.Format("package {0} is NOT checked and (notFlag) = true, correct AND logic, continue",
                            andLogic.PackageName), Logfiles.Application, LogLevel.Debug);
                        ANDSPass = true;
                    }
                    else
                    {
                        Logging.WriteToLog(string.Format("package {0}, checked={1}, notFlag={2}, incorrect AND logic, set andPass=false and break!",
                            andLogic.PackageName, andLogic.willBeInstalled, andLogic.NotFlag), Logfiles.Application, LogLevel.Debug);
                        ANDSPass = false;
                        break;
                    }
                }
                string final = string.Format("final result for dependency {0}: AND={1}, OR={2}", dependency.PackageName, ANDSPass, ORsPass);
                if(ANDSPass && ORsPass)
                {
                    Logging.Debug("{0} (AND and OR) = TRUE, dependency WILL be installed!", final);
                    dependenciesToInstall.Add(dependency);
                }
                else
                {
                    Logging.Debug("{0} (AND and OR) = FALSE, dependency WILL NOT be installed!", final);
                }

                if (dependency.DatabasePackageLogic.Count > 0 && (ANDSPass && ORsPass))
                {
                    Logging.Debug("updating future references (like logicalDependnecies) for if dependency was checked");
                    //update any dependencies that use it
                    foreach (DatabaseLogic callingLogic in dependency.Dependencies)
                    {
                        //get the dependency (if it is a dependency) that logic'ed this dependency
                        List<Dependency> found = dependencies.Where(dep => dep.PackageName.Equals(callingLogic.PackageName)).ToList();

                        if (found.Count > 0)
                        {
                            Dependency refrenced = found[0];
                            //now get the logic entry that refrences the original calculated depdnency
                            List<DatabaseLogic> foundLogic = refrenced.DatabasePackageLogic.Where(logic => logic.PackageName.Equals(dependency.PackageName)).ToList();
                            if (foundLogic.Count > 0)
                            {
                                Logging.Debug("logic refrence entry for dep {0} updated to {1}", refrenced.PackageName, ANDSPass && ORsPass);
                                foundLogic[0].willBeInstalled = ANDSPass && ORsPass;
                            }
                            else
                            {
                                Logging.Error("found logics count is 0 for updating refrences");
                            }
                        }
                        else
                        {
                            Logging.Error("found count is 0 for updating refrences");
                        }
                    }
                }

                notProcessedDependnecies.RemoveAt(0);
            }

            Logging.Debug("step 4 complete");
            return dependenciesToInstall;
        }

        public static List<DatabasePackage>[] CreateOrderedInstallList(List<DatabasePackage> packagesToInstall)
        {
            //get the max number of defined groups
            int maxGrops = packagesToInstall.Select(max => max.InstallGroup).Max();

            //make the list to return
            //make it maxGroups +1 because group 4 exists, but making a aray of 4 is 0-3
            List<DatabasePackage>[] orderedList = new List<DatabasePackage>[maxGrops+1];

            //new up the lists
            for (int i = 0; i < orderedList.Count(); i++)
                orderedList[i] = new List<DatabasePackage>();

            foreach(DatabasePackage package in packagesToInstall)
            {
                orderedList[package.InstallGroup].Add(package);
            }
            return orderedList;
        }

        public static void PropagateInstallGroupsPerLevel(List<DatabasePackage> packagesToInstall)
        {
            foreach(DatabasePackage package in packagesToInstall)
            {
                if (package is SelectablePackage selectablePackage)
                    selectablePackage.InstallGroup = selectablePackage.InstallGroup + selectablePackage.Level;
            }
        }

        public static void ClearSelections(List<Category> ParsedCategoryList)
        {
            foreach (SelectablePackage package in GetFlatList(null, null, null, ParsedCategoryList))
            {
                if(ModpackSettings.SaveDisabledMods && package.FlagForSelectionSave)
                {
                    Logging.Debug("SaveDisabledMods=True and package {0} FlagForSelectionSave is high, setting to low", nameof(Utils), package.Name);
                    package.FlagForSelectionSave = false;
                }
                package.Checked = false;
            }
            foreach (Category category in ParsedCategoryList)
                if (category.CategoryHeader != null && category.CategoryHeader.Checked)
                    category.CategoryHeader.Checked = false;
        }

        public static int GetMaxInstallGroupNumber(List<DatabasePackage> globalDependencies, List<Dependency> dependencies, List<Category> parsedCategoryList)
        {
            return GetMaxInstallGroupNumber(GetFlatList(globalDependencies, dependencies, null, parsedCategoryList));
        }

        public static int GetMaxInstallGroupNumber(List<DatabasePackage> listToCheck)
        {
            int maxInstallGroup = 0;
            foreach (DatabasePackage package in listToCheck)
                if (package.InstallGroup > maxInstallGroup)
                    maxInstallGroup = package.InstallGroup;
            return maxInstallGroup;
        }

        public static int GetMaxPatchGroupNumber(List<DatabasePackage> globalDependencies, List<Dependency> dependencies, List<Category> parsedCategoryList)
        {
            return GetMaxPatchGroupNumber(GetFlatList(globalDependencies, dependencies, null, parsedCategoryList));
        }

        public static int GetMaxPatchGroupNumber(List<DatabasePackage> listToCheck)
        {
            int maxPatchGroup = 0;
            foreach (DatabasePackage package in listToCheck)
                if (package.PatchGroup > maxPatchGroup)
                    maxPatchGroup = package.PatchGroup;
            return maxPatchGroup;
        }

        public static bool SetObjectMember(object packageOfAnyType, MemberInfo packageFieldOrProperty, string valueToSet)
        {
            try
            {
                if(packageFieldOrProperty is FieldInfo packageField)
                {
                    var converter = TypeDescriptor.GetConverter(packageField.FieldType);
                    packageField.SetValue(packageOfAnyType, converter.ConvertFrom(valueToSet));
                    return true;
                }
                else if(packageFieldOrProperty is PropertyInfo packageProperty)
                {
                    var converter = TypeDescriptor.GetConverter(packageProperty.PropertyType);
                    packageProperty.SetValue(packageOfAnyType, converter.ConvertFrom(valueToSet));
                    return true;
                }
                else
                {
                    Logging.Debug("invalid type of memberinfo of member {0}", packageFieldOrProperty.Name);
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logging.Exception(ex.ToString());
                return false;
            }
        }

        public static bool SetListEntriesField(object objectThatHasListProperty, FieldInfo nameOfListField, IEnumerable<XElement> xmlListItems)
        {
            //get a list type to add stuff to
            //https://stackoverflow.com/questions/25757121/c-sharp-how-to-set-propertyinfo-value-when-its-type-is-a-listt-and-i-have-a-li
            object obj = nameOfListField.GetValue(objectThatHasListProperty);
            DatabasePackage packageOfAnyType = objectThatHasListProperty as DatabasePackage;
            //IList list = (IList)packageField.GetValue(obj);
            IList list = (IList)obj;
            //we now have the empty list, now get type type of list it is
            //https://stackoverflow.com/questions/34211815/how-to-get-the-underlying-type-of-an-ilist-item
            Type listObjectType = list.GetType().GetInterfaces().Where(i => i.IsGenericType && i.GenericTypeArguments.Length == 1)
                .FirstOrDefault(i => i.GetGenericTypeDefinition() == typeof(IEnumerable<>)).GenericTypeArguments[0];
            //now for each xml element, get the value information and set it
            //if it originates from the 
            foreach (XElement listElement in xmlListItems)
            {
                //2 types of options for what this list could be: single node values (string, just node value), node of many values (custon type, many values)
                if (listElement.Attributes().Count() > 0)//custom class type
                {
                    object listEntry = Activator.CreateInstance(listObjectType);
                    //get list of fields in this entry object
                    FieldInfo[] fieldsInObjectInstance = listObjectType.GetFields();
                    List<string> missingMembers = new List<string>();
                    List<string> unknownMembers = new List<string>();
                    foreach (FieldInfo info in fieldsInObjectInstance)
                        missingMembers.Add(info.Name);
                    foreach (XAttribute listEntryAttribute in listElement.Attributes())
                    {
                        FieldInfo[] matchingListobjectsField = fieldsInObjectInstance.Where(field => field.Name.Equals(listEntryAttribute.Name.LocalName)).ToArray();
                        if (matchingListobjectsField.Count() == 0)
                        {
                            //no matching entries from xml attribute name to fieldInfo of custom type
                            unknownMembers.Add(listEntryAttribute.Name.LocalName);
                            continue;
                        }
                        FieldInfo listobjectField = matchingListobjectsField[0];
                        missingMembers.Remove(listobjectField.Name);
                        try
                        {
                            var converter = TypeDescriptor.GetConverter(listobjectField.FieldType);
                            listobjectField.SetValue(listEntry, converter.ConvertFrom(listEntryAttribute.Value));
                        }
                        catch (Exception ex)
                        {
                            Logging.Exception(ex.ToString());
                        }
                    }
                    foreach(string missingMember in missingMembers)
                    {
                        //exist in member class info, but not set from xml attributes
                        if(packageOfAnyType!= null)
                            Logging.Error("Missing xml attribute: {0}, package: {1}, line{2}", missingMember, packageOfAnyType.PackageName, ((IXmlLineInfo)listElement).LineNumber);
                        else
                            Logging.Error("Missing xml attribute: {0}, object: {1}, line{2}", missingMember, objectThatHasListProperty.ToString(), ((IXmlLineInfo)listElement).LineNumber);
                    }
                    foreach(string unknownMember in unknownMembers)
                    {
                        //exist in xml attributes, but not known member in memberInfo
                        if (packageOfAnyType != null)
                            Logging.Error("unknown xml attribute: {0}, package: {1}, line{2}", unknownMember, packageOfAnyType.PackageName, ((IXmlLineInfo)listElement).LineNumber);
                        else
                            Logging.Error("unknown xml attribute: {0}, object: {1}, line{2}", unknownMember, objectThatHasListProperty.ToString(), ((IXmlLineInfo)listElement).LineNumber);
                    }
                    list.Add(listEntry);
                }
                else//single primitive entry type
                {
                    list.Add(listElement.Value);
                }
            }
            return true;
        }
        #endregion

        #region Generic Utils
        public static BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            //https://stackoverflow.com/questions/22499407/how-to-display-a-bitmap-in-a-wpf-image
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();
                return bitmapimage;
            }
        }

        /// <summary>
        /// https://stackoverflow.com/questions/1344221/how-can-i-generate-random-alphanumeric-strings-in-c
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string RandomString(int length)
        {
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        // https://stackoverflow.com/questions/30494/compare-version-identifiers
        /// <summary>
        /// Compare versions of form "1,2,3,4" or "1.2.3.4". Throws FormatException
        /// in case of invalid version. See function comments for more informations and samples.
        /// </summary>
        /// <param name="strA">the first version</param>
        /// <param name="strB">the second version</param>
        /// <returns>less than zero if strA is less than strB, equal to zero if
        /// strA equals strB, and greater than zero if strA is greater than strB
        /// Samples:
        /// 1.0.0.0     | 1.0.0.1 = -1
        /// 1.0.0.1     | 1.0.0.0 =  1
        /// 1.0.0.0     | 1.0.0.0 =  0
        /// 1, 0.0.0    | 1.0.0.0 =  0
        /// 9, 5, 1, 44 | 3.4.5.6 =  1
        /// 1, 5, 1, 44 | 3.4.5.6 = -1
        /// 6,5,4,3     | 6.5.4.3 =  0</returns>
        public static int CompareVersions(String strA, String strB)
        {
            Version vA = new Version(strA.Replace(",", "."));
            Version vB = new Version(strB.Replace(",", "."));

            return vA.CompareTo(vB);
        }
        /// <summary>
        /// Gets the current time in the form of universal time
        /// </summary>
        /// <returns>the universal time of now</returns>
        public static long GetCurrentUniversalFiletimeTimestamp()
        {
            return DateTime.Now.ToUniversalTime().ToFileTime();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="timestamp"></param>
        /// <returns></returns>
        public static string ConvertFiletimeTimestampToDate(long timestamp)
        {
            if (timestamp > 0)
                return DateTime.FromFileTime(timestamp).ToString();
            else
                return "(none)";
        }
        /// <summary>
        /// Encode a plain text string into base64 UTF8 encoding
        /// </summary>
        /// <param name="plainText">The plain text string</param>
        /// <returns>The UTF8 base64 encoded version</returns>
        public static string Base64Encode(string plainText)
        {
            //https://stackoverflow.com/questions/11743160/how-do-i-encode-and-decode-a-base64-string
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }
        /// <summary>
        /// Decode a base64 UTF8 encoded string into plain text
        /// </summary>
        /// <param name="base64EncodedData">The base64 stirng</param>
        /// <returns>The plain text version</returns>
        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
            return Encoding.UTF8.GetString(base64EncodedBytes);
        }

        public static Process GetProcess(string processName, string pathToMatch = "")
        {
            //check if path of exe is the same as the one we're looking at
            //first check to make sure wot path is legit
            bool checkWithPath = true;
            if (string.IsNullOrEmpty(pathToMatch))
            {
                Logging.WriteToLog(nameof(pathToMatch) + "Is empty, cannot check for direct path, only checking for num processes",
                    Logfiles.Application, LogLevel.Error);
                checkWithPath = false;
            }
            //get list of running instances of WoT
            //TO GET PROCESS NAME: Process.GetCurrentProcess().ProcessName
            Process[] processes = Process.GetProcessesByName(processName);
            //check if three are any at all
            if (processes.Length == 0)
            {
                return null;
            }
            //first check if the number is 1 or less, if so stop here
            else if (processes.Length == 1)
                return null;
            //if not checking for path, we don't know if is the direct path, 
            else if (!checkWithPath)
                return processes[0];
            else
            {
                foreach (Process p in processes)
                {
                    if (pathToMatch.Equals(Path.GetDirectoryName(p.StartInfo.FileName)))
                    {
                        Logging.WriteToLog(string.Format("Matched process name {0} to path {1}", p.ProcessName, pathToMatch),
                            Logfiles.Application, LogLevel.Debug);
                        return p;
                    }
                }
            }
            return null;
        }

        public static bool IsProcessRunning(string processName, string pathToMatch = "")
        {
            return GetProcess(processName, pathToMatch) == null ? false : true;
        }
        
        public static List<DatabasePackage> GetFlatList(List<DatabasePackage> globalDependnecies = null, List<Dependency> dependencies = null,
            List<Dependency> logicalDependencies = null, List<Category> parsedCategoryList = null)
        {
            if (globalDependnecies == null && dependencies == null && logicalDependencies == null && parsedCategoryList == null)
                return null;
            List<DatabasePackage> flatList = new List<DatabasePackage>();
            if (globalDependnecies != null)
                flatList.AddRange(globalDependnecies);
            if (dependencies != null)
                flatList.AddRange(dependencies);
            if (logicalDependencies != null)
                flatList.AddRange(logicalDependencies);
            if (parsedCategoryList != null)
                foreach (Category cat in parsedCategoryList)
                    flatList.AddRange(cat.GetFlatPackageList());
            return flatList;
        }

        public static List<SelectablePackage> GetFlatSelectablePackageList(List<Category> parsedCategoryList)
        {
            if (parsedCategoryList == null)
                return null;
            List<SelectablePackage> flatList = new List<SelectablePackage>();
            foreach (Category cat in parsedCategoryList)
                flatList.AddRange(cat.GetFlatPackageList());
            return flatList;
        }

        public static bool StartProcess(ProcessStartInfo startInfo)
        {
            try
            {
                Process.Start(startInfo);
                return true;
            }
            catch (Exception ex)
            {
                Logging.Exception("Failed to start process");
                Logging.Exception(ex.ToString());
                return false;
            }
        }

        public static bool StartProcess(string command)
        {
            try
            {
                Process.Start(command);
                return true;
            }
            catch (Exception ex)
            {
                Logging.Exception("Failed to start process");
                Logging.Exception(ex.ToString());
                return false;
            }
        }
        #endregion

        #region Macro Utils
        public static void BuildFilepathMacroList()
        {
            if (FilePathDict == null)
                throw new BadMemeException("REEEEEEEEEE");
            FilePathDict.Clear();
            //add macro versions first then regular versions
            FilePathDict.Add(@"{versiondir}", Settings.WoTClientVersion);
            FilePathDict.Add(@"{tanksversion}", Settings.WoTClientVersion);
            FilePathDict.Add(@"{tanksonlinefolderversion}", Settings.WoTModpackOnlineFolderVersion);
            FilePathDict.Add(@"{appdata}", Settings.AppDataFolder);
            FilePathDict.Add(@"{app}", Settings.WoTDirectory);
            FilePathDict.Add(@"versiondir", Settings.WoTClientVersion);
        }
        public static string MacroReplace(string inputString, ReplacementTypes type)
        {
            //itterate through each entry depending on the dictionary. if the key is contained in the string, replace it
            //use a switch to get which dictionary reaplce we will use
            Dictionary<string, string> dictionary = null;
            switch (type)
            {
                case ReplacementTypes.FilePath:
                    dictionary = FilePathDict;
                    break;
                case ReplacementTypes.PatchArguements:
                    dictionary = PatchArguementsDict;
                    break;
                case ReplacementTypes.PatchFiles:
                    dictionary = PatchFilesDict;
                    break;
                case ReplacementTypes.TextEscape:
                    dictionary = TextEscapeDict;
                    break;
                case ReplacementTypes.TextUnescape:
                    dictionary = TextUnscapeDict;
                    break;
            }
            if (dictionary == null)
            {
                Logging.Error("macro replace dictionary is null! type={0}", type.ToString());
                return inputString;
            }
            for (int i = 0; i < dictionary.Count; i++)
            {
                string key = dictionary.ElementAt(i).Key;
                string replace = dictionary.ElementAt(i).Value;
                //https://stackoverflow.com/questions/444798/case-insensitive-containsstring
                //it's an option, not actually used here cause it would be a lot of work to implement
                //could also try regex, may be easlier to ignore case, but then might have to make it an option
                //so for now, no
                if (inputString.Contains(key))
                    inputString = inputString.Replace(key, replace);
            }
            return inputString;
        }
        #endregion

        #region Tanks Install Auto/Manuel Search Code
        //checks the registry to get the location of where WoT is installed
        public static bool AutoFindWoTDirectory(ref string WoTRoot)
        {
            List<string> searchPathWoT = new List<string>();
            string[] registryPathArray = new string[] { };

            // here we need the value for the searchlist
            // check replay link
            registryPathArray = new string[] { @"HKEY_LOCAL_MACHINE\SOFTWARE\Classes\.wotreplay\shell\open\command", @"HKEY_CURRENT_USER\Software\Classes\.wotreplay\shell\open\command" };
            foreach (string regEntry in registryPathArray)
            {
                // get values from from registry
                object obj = Registry.GetValue(regEntry, "", -1);
                // if it is not "null", it is containing possible a string
                if (obj != null)
                {
                    try
                    {
                        // add the thing to the checklist, but remove the Quotation Marks in front of the string and the trailing -> " "%1"
                        searchPathWoT.Add(((string)obj).Substring(1).Substring(0, ((string)obj).Length - 7));
                    }
                    catch
                    { } // only exception catching
                }
            }

            // here we need the value for the searchlist
            string regPath = @"HKEY_CURRENT_USER\Software\Wargaming.net\Launcher\Apps\wot";
            RegistryKey subKeyHandle = Registry.CurrentUser.OpenSubKey(regPath.Replace(@"HKEY_CURRENT_USER\", ""));
            if (subKeyHandle != null)
            {
                // get the value names at the reg Key one by one
                foreach (string valueName in subKeyHandle.GetValueNames())
                {
                    // read the value from the regPath
                    object obj = Registry.GetValue(regPath, valueName, -1);
                    if (obj != null)
                    {
                        try
                        {
                            // we did get only a path to used WoT folders, so add the game name to the path and add it to the checklist
                            searchPathWoT.Add(Path.Combine((string)obj, "WorldOfTanks.exe"));
                        }
                        catch
                        { } // only exception catching
                    }
                }
            }

            // here we need the value name for the searchlist
            registryPathArray = new string[] { @"Software\Classes\Local Settings\Software\Microsoft\Windows\Shell\MuiCache", @"Software\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\Compatibility Assistant\Store" };
            foreach (string p in registryPathArray)
            {
                // set the handle to the registry key
                subKeyHandle = Registry.CurrentUser.OpenSubKey(p);
                if (subKeyHandle == null) continue;            // subKeyHandle == null not existsting
                // parse all value names of the registry key abouve
                foreach (string valueName in subKeyHandle.GetValueNames())
                {
                    try
                    {
                        // if the lower string "worldoftanks.exe" is contained => match !!
                        if (valueName.ToLower().Contains("Worldoftanks.exe".ToLower()))
                        {
                            // remove (replace it with "") the attachment ".ApplicationCompany" or ".FriendlyAppName" in the string and add the string to the searchlist
                            searchPathWoT.Add(valueName.Replace(".ApplicationCompany", "").Replace(".FriendlyAppName", ""));
                        }
                    }
                    catch
                    { } // only exception catching
                }
            }

            // this searchlist is long, maybe 30-40 entries (system depended), but the best possibility to find a currently installed WoT game.
            foreach (string path in searchPathWoT)
            {
                if (File.Exists(path))
                {
                    Logging.Info(string.Format("valid game path found: {0}", path));
                    // write the path to the central value holder
                    WoTRoot = path;
                    // return the path
                    return true;
                }
            }
            //return false if nothing found
            return false;
        }
        #endregion

        #region Install Utils
        public static void CreateShortcut(Shortcut shortcut, StringBuilder sb)
        {
            Logging.Info(shortcut.ToString());
            Logging.Info("Creating shortcut {0}",shortcut.Name);
            //build the full macro for path (target) and name (also filename)
            string target = MacroReplace(shortcut.Path, ReplacementTypes.FilePath).Replace(@"/",@"\");
            string filename = string.Format("{0}.lnk", shortcut.Name);
            string shortcutPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            shortcutPath = Path.Combine(shortcutPath, filename);
            Logging.Info("target={0}", target);
            Logging.Info("shortcutPath={0}", shortcutPath);
            if(!File.Exists(target))
            {
                Logging.Warning("target does not exist, skipping shortcut", target);
                return;
            }
            if(File.Exists(shortcutPath))
            {
                Logging.Debug("shortcut path exists, checking if update needed");
                WshShell shell = new WshShell();
                IWshShortcut link = (IWshShortcut)shell.CreateShortcut(shortcutPath);
                Logging.Debug("new target = {0}, old target = {1}", target, link.TargetPath);
                if(!target.Equals(link.TargetPath))
                {
                    //needs update
                    Logging.Debug("updating target");
                    link.TargetPath = target;
                    link.Save();
                }
                else
                {
                    //no update needed
                    Logging.Debug("no update needed");
                    return;
                }
            }
            else
            {
                Logging.Debug("shortcut path does not exist, creating");
                IShellLink link = (IShellLink)new ShellLink();
                // setup shortcut information
                link.SetDescription("created by the Relhax Manager");
                link.SetPath(target);
                link.SetIconLocation(target, 0);
                link.SetWorkingDirectory(Path.GetDirectoryName(target));
                //The arguments used when executing the exe (none used for now)
                link.SetArguments("");
                System.Runtime.InteropServices.ComTypes.IPersistFile file = (System.Runtime.InteropServices.ComTypes.IPersistFile)link;
                file.Save(shortcutPath, false);
            }
            //getting here means that the target is updated or created, so log it to the installer
            sb.AppendLine(shortcutPath);
        }
        #endregion

        #region FTP methods
        public static void FTPMakeFolder(string addressWithDirectory, ICredentials credentials)
        {
            WebRequest folderRequest = WebRequest.Create(addressWithDirectory);
            folderRequest.Method = WebRequestMethods.Ftp.MakeDirectory;
            folderRequest.Credentials = credentials;
            using (FtpWebResponse response = (FtpWebResponse)folderRequest.GetResponse())
            { }
        }

        public static async Task FTPMakeFolderAsync(string addressWithDirectory, ICredentials credentials)
        {
            WebRequest folderRequest = WebRequest.Create(addressWithDirectory);
            folderRequest.Method = WebRequestMethods.Ftp.MakeDirectory;
            folderRequest.Credentials = credentials;
            using (FtpWebResponse webResponse = (FtpWebResponse)await folderRequest.GetResponseAsync())
            { }
        }

        public static string[] FTPListFilesFolders(string address, ICredentials credentials)
        {
            WebRequest folderRequest = WebRequest.Create(address);
            folderRequest.Method = WebRequestMethods.Ftp.ListDirectory;
            folderRequest.Credentials = credentials;
            using (FtpWebResponse response = (FtpWebResponse)folderRequest.GetResponse())
            {
                Stream responseStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(responseStream);
                string temp = reader.ReadToEnd();
                return temp.Split(new[] { "\r\n" }, StringSplitOptions.None);
            }
        }

        public static async Task<string[]> FTPListFilesFoldersAsync(string address, ICredentials credentials)
        {
            WebRequest folderRequest = WebRequest.Create(address);
            folderRequest.Method = WebRequestMethods.Ftp.ListDirectory;
            folderRequest.Credentials = credentials;
            using (FtpWebResponse response = (FtpWebResponse)await folderRequest.GetResponseAsync())
            {
                Stream responseStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(responseStream);
                string temp = reader.ReadToEnd();
                return temp.Split(new[] { "\r\n" }, StringSplitOptions.None);
            }
        }

        public static void FTPDeleteFile(string address, ICredentials credentials)
        {
            WebRequest folderRequest = WebRequest.Create(address);
            folderRequest.Method = WebRequestMethods.Ftp.DeleteFile;
            folderRequest.Credentials = credentials;
            using (FtpWebResponse response = (FtpWebResponse)folderRequest.GetResponse())
            { }
        }

        public static async Task FTPDeleteFileAsync(string address, ICredentials credentials)
        {
            WebRequest folderRequest = WebRequest.Create(address);
            folderRequest.Method = WebRequestMethods.Ftp.DeleteFile;
            folderRequest.Credentials = credentials;
            using (FtpWebResponse response = (FtpWebResponse)await folderRequest.GetResponseAsync())
            { }
        }

        public static long FTPGetFilesize(string address, ICredentials credentials)
        {
            long result = -1;
            // Get the object used to communicate with the server.
            //https://stackoverflow.com/questions/4591059/download-file-from-ftp-with-progress-totalbytestoreceive-is-always-1
            WebRequest request = WebRequest.Create(address);
            request.Method = WebRequestMethods.Ftp.GetFileSize;
            request.Credentials = credentials;
            using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
            {
                //Stream responseStream = response.GetResponseStream();
                result = response.ContentLength;
            }
            return result;
        }

        public static async Task<long> FTPGetFilesizeAsync(string address, ICredentials credentials)
        {
            long result = -1;
            // Get the object used to communicate with the server.
            //https://stackoverflow.com/questions/4591059/download-file-from-ftp-with-progress-totalbytestoreceive-is-always-1
            WebRequest request = WebRequest.Create(address);
            request.Method = WebRequestMethods.Ftp.GetFileSize;
            request.Credentials = credentials;
            using (FtpWebResponse response = (FtpWebResponse)await request.GetResponseAsync())
            {
                //Stream responseStream = response.GetResponseStream();
                result = response.ContentLength;
            }
            return result;
        }
        #endregion

        #region Gross shortcut stuff
        // needed for CreateShortcut
        [ComImport]
        [Guid("00021401-0000-0000-C000-000000000046")]
        internal class ShellLink
        {
        }
        // needed for CreateShortcut
        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("000214F9-0000-0000-C000-000000000046")]
        internal interface IShellLink
        {
            void GetPath([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile, int cchMaxPath, out IntPtr pfd, int fFlags);
            void GetIDList(out IntPtr ppidl);
            void SetIDList(IntPtr pidl);
            void GetDescription([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszName, int cchMaxName);
            void SetDescription([MarshalAs(UnmanagedType.LPWStr)] string pszName);
            void GetWorkingDirectory([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszDir, int cchMaxPath);
            void SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] string pszDir);
            void GetArguments([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszArgs, int cchMaxPath);
            void SetArguments([MarshalAs(UnmanagedType.LPWStr)] string pszArgs);
            void GetHotkey(out short pwHotkey);
            void SetHotkey(short wHotkey);
            void GetShowCmd(out int piShowCmd);
            void SetShowCmd(int iShowCmd);
            void GetIconLocation([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszIconPath, int cchIconPath, out int piIcon);
            void SetIconLocation([MarshalAs(UnmanagedType.LPWStr)] string pszIconPath, int iIcon);
            void SetRelativePath([MarshalAs(UnmanagedType.LPWStr)] string pszPathRel, int dwReserved);
            void Resolve(IntPtr hwnd, int fFlags);
            void SetPath([MarshalAs(UnmanagedType.LPWStr)] string pszFile);
        }
        #endregion
    }
}
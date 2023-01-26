using UnityEngine;
using System.IO;

namespace JuliaPlugin
{
    /// <summary>
    /// Class for reading the content of the Inclusions.txt file.
    /// </summary>
    public class FileHandler
    {
        private readonly string INCLUSIONSFILE = Path.Combine(Application.streamingAssetsPath, "Inclusions.txt");
        public string InclusionsFilePath
        {
            get
            { 
                return INCLUSIONSFILE;
            }
        }

        private static FileHandler _instance;
        public static FileHandler Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new FileHandler();
                }
                return _instance;
            }
        }

        public string[] GetInclusions()
        {
            string[] temp;
            try
            {
                temp = File.ReadAllLines(InclusionsFilePath);
            }
            catch (DirectoryNotFoundException)
            {
                return null;
            }
            catch (FileNotFoundException)
            {
                return null;
            }
            string[] validLines = new string[CountValidLines(temp)];

            for (int i = 0, j = 0; i < temp.Length; i++)
            {
                if (!temp[i].StartsWith("#"))
                {
                    validLines[j] = temp[i];
                    j++;
                }
            }
            return validLines;
        }

        private int CountValidLines(string[] allLines)
        {
            int validLines = 0;
            for (int i = 0; i < allLines.Length; i++)
            {
                if (!allLines[i].StartsWith("#"))
                {
                    validLines++;
                }
            }
            return validLines;
        }
    }
}
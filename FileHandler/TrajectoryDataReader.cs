using System;
using System.IO;
using System.Numerics;
using GeometricObject;
using System.Linq;

namespace FileHandler
{
    public class TrajectoryDataReader
    {
        public Trajectory ReadFile(string path, out string errorMessage)
        {
            try
            {
                errorMessage = "";

                using (StreamReader sr = new StreamReader(path))
                {
                    string line;

                    string fileName = path.Split('.')[0].Split('\\').Last();
                    if(TryParseFileName(fileName, out errorMessage))
                    {
                        string wellName = fileName.Split('-')[0];
                        string trajectoryName = fileName.Split('-')[1];
                        Trajectory newTrajectory = new Trajectory(path, wellName, trajectoryName);

                        int lineNumber = 1;
                        while (!string.IsNullOrEmpty(line = sr.ReadLine()))
                        {
                            Vector3 point = ParseLine(lineNumber, line, out errorMessage);
                            if (!string.IsNullOrEmpty(errorMessage))
                            {
                                return null;
                            }
                            newTrajectory.AddNode(point);
                            lineNumber++;
                        }

                        return newTrajectory;
                    }
                }
            }
            catch (FileNotFoundException)
            {
                errorMessage = "Error: File not found.";
            }
            catch (UnauthorizedAccessException)
            {
                errorMessage = "Error: File not accessible.";
            }
            catch (Exception ex)
            {
                errorMessage = "Failed to read trajectory data from file. Error: " + ex.Message;
            }

            return null;
        }

        public bool TryParseFileName(string fileName, out string errorMessage)
        {
            errorMessage = "";
            try
            {
                string[] names = fileName.Split('-');
                if (names.Count() != 2)
                {
                    errorMessage = "File name format error.\nValid format: \"wellName-trajectoryName\"";
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = $"File Name Error: {ex.Message}";
                return false;
            }
        }

        public Vector3 ParseLine(int lineNumber, string line, out string errorMessage)
        {
            errorMessage = "";

            try
            {
                if (!string.IsNullOrEmpty(line))
                {
                    char splitter = ',';
                    string[] coordinates = line.Split(splitter);
                    if (coordinates.Count() > 3)
                    {
                        errorMessage = $"Error in Line {lineNumber}: Data overflow.";
                    }
                    float x = float.Parse(coordinates[0]);
                    float y = float.Parse(coordinates[1]);
                    float z = float.Parse(coordinates[2]);
                    Vector3 vector = new Vector3(x, y, z);
                    return vector;
                }
            }
            catch (IndexOutOfRangeException)
            {
                errorMessage = $"Error in Line {lineNumber}: Data lost.";
            }
            catch (FormatException)  // Parse: coordinates are not float numbers
            {
                errorMessage = $"Error in Line {lineNumber}: Non-float numbers in data.";
            }
            catch (Exception ex)
            {
                errorMessage = $"Error in Line {lineNumber}: {ex.Message}";
            }

            return new Vector3();
        }
    }
}

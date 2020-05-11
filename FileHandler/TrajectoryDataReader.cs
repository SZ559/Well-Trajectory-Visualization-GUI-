using System;
using System.IO;
using System.Collections.Generic;
using GeometricObject;
using System.Numerics;

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
                    Trajectory newTrajectory = new Trajectory();

                    int lineNumber = 0;
                    while (!string.IsNullOrEmpty(line = sr.ReadLine()))
                    {
                        Vector3 point = ParseLine(lineNumber, line, out errorMessage);
                        newTrajectory.AddNode(point);
                        lineNumber++;
                    }

                    return newTrajectory;
                }
            }
            catch (FileNotFoundException)
            {
                errorMessage = "File not found.";
            }
            catch (UnauthorizedAccessException)
            {
                errorMessage = "File not accessible.";
            }
            catch (Exception ex)
            {
                errorMessage = "Failed to read trajectory data from file. " + ex.Message;
            }

            return null;
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
                    float x = float.Parse(coordinates[0]);
                    float y = float.Parse(coordinates[1]);
                    float z = float.Parse(coordinates[2]);
                    Vector3 vector = new Vector3(x, y, z);
                    return vector;
                }
            }
            catch (FormatException)  // Parse: coordinates are not float numbers
            {
                errorMessage = $"Line: {lineNumber}: Non-float numbers in data.";
            }
            catch (IndexOutOfRangeException)
            {
                errorMessage = $"Line: {lineNumber}: Data lost.";
            }
            catch (Exception ex)
            {
                errorMessage = $"Line: {lineNumber}: {ex.Message}";
            }

            return new Vector3();
        }
    }
}

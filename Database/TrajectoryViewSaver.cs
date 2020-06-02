using System;
using System.Drawing;

namespace DataSource
{
    public class WellViewSaver
    {
        public void SaveView(string path, Image image, out string errorMessage)
        {
            errorMessage = "";
            try
            {
                image.Save(path);
            }
            catch (Exception ex)
            {
                errorMessage = $"Saving view failed: {ex.Message}";
            }
        }
    }
}

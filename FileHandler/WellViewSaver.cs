using System.Drawing;

namespace FileHandler
{
    public class WellViewSaver
    {
        public void SaveView(string path, Graphics graphic)
        {
            using (Image image = Image.FromFile(path))
            {
                // Crop and resize the image.
                Rectangle destination = new Rectangle(0, 0, image.Width, image.Height);
                graphic.DrawImage(image, destination);

                image.Save(path);
            }
        }
    }
}

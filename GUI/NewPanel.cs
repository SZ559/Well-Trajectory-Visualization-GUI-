using System.Windows.Forms;


namespace Well_Trajectory_Visualization
{
    public class NewPanel : Panel
    {
        public NewPanel()
        {
            //SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor, true);
        }

        /*
                    protected override void OnPaintBackground(PaintEventArgs e)

                    {

                        return;

                    }


                    protected override void OnPaint(PaintEventArgs e)

                    {


                        this.DoubleBuffered = true;

                        if (this.BackgroundImage != null)

                        {

                            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

                            e.Graphics.DrawImage(

                            this.BackgroundImage,

                            new System.Drawing.Rectangle(0, 0, this.Width, this.Height),

                            0,

                            0,

                            this.BackgroundImage.Width,

                            this.BackgroundImage.Height,

                            System.Drawing.GraphicsUnit.Pixel);

                        }

                        base.OnPaint(e);

                    }

                }

            */
    }
}


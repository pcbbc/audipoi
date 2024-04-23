using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PocketGpsWorld
{
    internal enum IconCameraType
    {
        Speed = 1,
        Average = 2,
        AverageE = 3,
        RedLight = 4,
        Tunnel = 5,
        Mobile = 6,
        PMobile = 7,
    }

    internal class IconBuilder
    {
        internal IconCameraType TypeOfCamera = IconCameraType.Speed;
        internal int Speed = 0;

        internal Bitmap GenerateIcon()
        {
            const int iWidth = 33;  //30;
            const int iHeight = 33; //31;

            Bitmap bmpIcon = new Bitmap(iWidth, iHeight, PixelFormat.Format32bppArgb);
            Color clrTransparent = Color.FromArgb(0, Color.White);
            using (Graphics g = Graphics.FromImage(bmpIcon))
            {
                //g.Clear(Color.FromArgb(0, 0, 0, 0));
                g.Clear(clrTransparent);
                g.SmoothingMode = SmoothingMode.AntiAlias;
                switch (this.TypeOfCamera)
                {
                    case IconCameraType.Speed:
                        this.DrawSpeed(g, Color.White, Color.Black);
                        break;
                    case IconCameraType.Average:
                    case IconCameraType.AverageE:
                        this.DrawAverage(g);
                        break;
                    case IconCameraType.RedLight:
                        if (this.Speed > 0)
                        {
                            this.DrawSpeed(g, Color.White, Color.Black);
                        }
                        this.DrawRedLight(g);
                        break;
                    case IconCameraType.Tunnel:
                        this.DrawTunnel(g);
                        break;
                    case IconCameraType.Mobile:
                        this.DrawSpeed(g, Color.Black, Color.White);
                        break;
                    case IconCameraType.PMobile:
                        this.DrawSpeed(g, Color.Black, Color.White);
                        break;
                    default:
                        throw new Exception("Unable to draw camera type");
                }
            }

            return bmpIcon;
        }

        private void DrawSpeed(Graphics g, Color bg, Color fg)
        {
            g.FillEllipse(Brushes.Red, new RectangleF(-0.5f, -0.5f, 32, 32));
            g.FillEllipse(new SolidBrush(bg), new RectangleF(4f, 4f, 23, 23));

            string sText = this.SpeedText();
            Font oFont;
            if (sText.Length < 3)
            {
                oFont = new Font(FontFamily.GenericSansSerif, 14, FontStyle.Bold);
            }
            else
            {
                oFont = new Font(FontFamily.GenericSansSerif, 9, FontStyle.Bold);
            }

            SizeF oSize = g.MeasureString(sText, oFont);
            g.DrawString(sText, oFont, new SolidBrush(fg), 15.5f - (float)oSize.Width / 2f, 16f - (float)oSize.Height / 2f);
        }

        private void DrawAverage(Graphics g)
        {
            g.FillRectangle(Brushes.Red, new RectangleF(-0.5f, -0.5f, 32, 32));
            g.FillRectangle(Brushes.White, new RectangleF(4f, 4f, 23, 23));

            string sText = this.SpeedText();
            Font oFont;
            if (sText.Length < 3)
            {
                oFont = new Font(FontFamily.GenericSansSerif, 14, FontStyle.Bold);
            }
            else
            {
                oFont = new Font(FontFamily.GenericSansSerif, 9, FontStyle.Bold);
            }

            SizeF oSize = g.MeasureString(sText, oFont);
            g.DrawString(sText, oFont, Brushes.Black, 15.5f - (float)oSize.Width / 2f, 16f - (float)oSize.Height / 2f);
        }

        private string SpeedText()
        {
            if (this.Speed == -2)
            {
                return "?";
            }
            else if (this.Speed == -1)
            {
                return "V";
            }
            else if (this.Speed == 0)
            {
                return "X";
            }
            else
            {
                return this.Speed.ToString();
            }
        }

        private void DrawRedLight(Graphics g)
        {
            if (this.Speed > 0)
            {
                g.TranslateTransform(12, 0);
            }
            Brush b = new SolidBrush(Color.FromArgb(255, Color.Black));
            g.FillEllipse(b, new RectangleF(6.5f, -1.5f, 17, 17));  //5.5f, -0.5f, 18, 18
            g.FillEllipse(b, new RectangleF(6.5f, 7f, 17, 17));     //5.5f,    9f, 18, 18
            g.FillEllipse(b, new RectangleF(6.5f, 15.5f, 17, 17));  //5.5f, 18.5f, 18, 18

            b = new SolidBrush(Color.FromArgb(255, Color.Orange));
            g.FillEllipse(b, new RectangleF(9.5f, 10f, 11, 11));    //8.5f,   12f, 12, 12
            b = new SolidBrush(Color.FromArgb(255, Color.Red));
            g.FillEllipse(b, new RectangleF(9.5f, 1.5f, 11, 11));   //8.5f,  2.5f, 12, 12
            b = new SolidBrush(Color.FromArgb(255, Color.Green));
            g.FillEllipse(b, new RectangleF(9.5f, 18.5f, 11, 11));  //8.5f, 21.5f, 12, 12

            g.ResetTransform();
        }

        private void DrawTunnel(Graphics g)
        {
            g.FillRectangle(Brushes.Red, new RectangleF(-0.5f, -0.5f, 36, 38));
            g.FillRectangle(Brushes.White, new RectangleF(4.5f, 4.5f, 26, 28));

            g.FillEllipse(Brushes.Black, new RectangleF(7.5f, 6.5f, 20, 20));
            g.FillRectangle(Brushes.Black, new RectangleF(7.5f, 16.5f, 20, 14));
        }
    }
}

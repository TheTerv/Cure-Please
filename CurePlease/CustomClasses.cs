using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace CurePlease
{
    public class GroupBoxEx : GroupBox
    {
        private Color borderColor = Color.DimGray;
        [DefaultValue(typeof(Color), "Black")]
        public Color BorderColor
        {
            get => borderColor;
            set { borderColor = value; Invalidate(); }
        }
        private Color textColor = Color.Black;
        [DefaultValue(typeof(Color), "Black")]
        public Color TextColor
        {
            get => textColor;
            set { textColor = value; Invalidate(); }
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            GroupBoxState state = base.Enabled ? GroupBoxState.Normal :
                GroupBoxState.Disabled;
            TextFormatFlags flags = TextFormatFlags.PreserveGraphicsTranslateTransform |
                TextFormatFlags.PreserveGraphicsClipping | TextFormatFlags.TextBoxControl |
                TextFormatFlags.WordBreak;
            Color titleColor = TextColor;
            if (!ShowKeyboardCues)
            {
                flags |= TextFormatFlags.HidePrefix;
            }

            if (RightToLeft == RightToLeft.Yes)
            {
                flags |= TextFormatFlags.RightToLeft | TextFormatFlags.Right;
            }

            if (!Enabled)
            {
                titleColor = SystemColors.GrayText;
            }

            DrawUnthemedGroupBoxWithText(e.Graphics, new Rectangle(0, 0, base.Width,
                base.Height), Text, Font, titleColor, flags, state);
            RaisePaintEvent(this, e);
        }
        private void DrawUnthemedGroupBoxWithText(Graphics g, Rectangle bounds,
            string groupBoxText, Font font, Color titleColor,
            TextFormatFlags flags, GroupBoxState state)
        {
            Rectangle rectangle = bounds;
            rectangle.Width -= 8;
            Size size = TextRenderer.MeasureText(g, groupBoxText, font,
                new Size(rectangle.Width, rectangle.Height), flags);
            rectangle.Width = size.Width;
            rectangle.Height = size.Height;
            if ((flags & TextFormatFlags.Right) == TextFormatFlags.Right)
            {
                rectangle.X = (bounds.Right - rectangle.Width) - 8;
            }
            else
            {
                rectangle.X += 8;
            }

            TextRenderer.DrawText(
                g,
                groupBoxText,
                font,
                rectangle,
                titleColor,
                flags);
            if (rectangle.Width > 0)
            {
                rectangle.Inflate(2, 2);
            }

            using (Pen pen = new Pen(BorderColor))
            {
                int num = bounds.Top + (font.Height / 2);

                g.DrawLine(pen, bounds.Left, num - 1, bounds.Left, bounds.Height - 2);

                g.DrawLine(pen, bounds.Left, bounds.Height - 2, bounds.Width - 2, bounds.Height - 2);

                g.DrawLine(pen, bounds.Left, num - 1, rectangle.X - 3, num - 1);

                g.DrawLine(pen, rectangle.X + rectangle.Width + 2, num - 1, bounds.Width - 2, num - 1);

                g.DrawLine(pen, bounds.Width - 2, num - 1, bounds.Width - 2, bounds.Height - 3);
            }
        }
    }


    public class NewTabControl : TabControl
    {

        public Color ControlBackColor { get; set; }
        public Color SelectedTabColor { get; set; }
        public Color TabTextColor { get; set; }
        public Color SelectedTabTextColor { get; set; }

        public NewTabControl()
        {
            base.SetStyle(ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor, true);
            base.UpdateStyles();

            ControlBackColor = SystemColors.Control;
            SelectedTabColor = SystemColors.Highlight;
            TabTextColor = SystemColors.ControlText;
            SelectedTabTextColor = SystemColors.ControlText;

        }


        protected override void OnPaint(PaintEventArgs e)
        {
            Rectangle boundsControl = ClientRectangle;
            Brush colorBrush = new SolidBrush(ControlBackColor);
            e.Graphics.FillRectangle(colorBrush, boundsControl);

            for (int i = 0; i < TabCount; i++)
            {
                Rectangle bounds = GetTabRect(i);

                if (SelectedIndex == i)
                {
                    Brush selectBrush = new SolidBrush(SelectedTabColor);
                    e.Graphics.FillRectangle(selectBrush, bounds);
                }

                int space = bounds.Height * 1 / 6;
                int imageWidth = 0;
                if (ImageList != null && ImageList.Images.Count >= i + 1)
                {
                    imageWidth = bounds.Height * 2 / 3;
                    Image ins = ImageList.Images[i];
                    Rectangle rectImage = new Rectangle(new Point(bounds.X + space, bounds.Y + bounds.Height / 2 - imageWidth / 2), new Size(imageWidth, imageWidth));
                    e.Graphics.DrawImage(ins, rectImage);
                }

                PointF textPoint = new PointF();
                SizeF textSize = TextRenderer.MeasureText(TabPages[i].Text, Font);

                textPoint.X = bounds.X + imageWidth;
                textPoint.Y = bounds.Bottom - textSize.Height - Padding.Y;
                Rectangle rectText = new Rectangle(bounds.X + imageWidth + space, bounds.Top, bounds.Width - imageWidth, bounds.Height);

                StringFormat sf = new StringFormat
                {
                    LineAlignment = StringAlignment.Center,
                    Alignment = StringAlignment.Near
                };

                Font newFont = new Font("Times New Roman", 15);

                if (SelectedIndex == i)
                {
                    Brush textBrush = new SolidBrush(Color.White);
                    e.Graphics.DrawString(TabPages[i].Text, newFont, textBrush, rectText, sf);
                }
                else
                {
                    Brush textBrush = new SolidBrush(Color.Black);
                    e.Graphics.DrawString(TabPages[i].Text, newFont, textBrush, rectText, sf);
                }
            }
        }
    }




}

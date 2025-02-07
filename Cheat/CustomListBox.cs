using Accessibility;
using Cheat.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Cheat
{
    public partial class CustomListBox : UserControl
    {
        private List<string> _items;
        private bool _shown = false;
        private int _defaultItemHeight = 55;
        private int _itemHeight = 55; 
        private int _smallerItemHeight = 30;
        private int _selectedItem = 0;
        private int _topItemInList = 0;
        private bool _showTags =true;
        private bool _viewOnly = false;
        private Direction _direction;
        

        public Dictionary<string, CUtils.FileInfo> FilesToFlag { set; private get; }



        public delegate void ItemSelected(string ItemValue);
        public event ItemSelected OnItemSelected;

        public event EventHandler OnEnterPresssed;
        public event KeyEventHandler OnEscapePressed;

        private enum Direction
        {
            Up,
            Down
        }

        Bitmap bitmap = null;


        public CustomListBox()
        {
            InitializeComponent();
            ShowTags = true;
            _itemHeight = _defaultItemHeight;
            _items = new List<string>();
           
        }

        public bool ShowTags { get { return _showTags; }
            set 
            {
                _showTags = value;
                if (value)
                {
                    _itemHeight = _defaultItemHeight;

                }
                else
                {
                    _itemHeight = _smallerItemHeight;
                }

            }
        }
        public bool ViewOnly { set { _viewOnly = value; } get { return _viewOnly; } }

        public List<string> Items { get { return _items; } set { _items = value; } }

        private void FireOnItemSelected()
        {
            //if (_viewOnly)
            //{
            //    return;
            //}
                
            if(_items.Count == 0) { return; }

            if (OnItemSelected != null)
            {
                OnItemSelected(_items[_selectedItem]);
            }
        }

        public string SelectedItem {
            get 
            { 
                if(_items.Count == 0) return null;

                return _items?[_selectedItem];
            }
        }
        public void ResetSelectedItem()
        {
            _selectedItem = 0;
        }

        private void FireOnEnterPressed()
        {
            if (OnEnterPresssed != null)
            {
                OnEnterPresssed(null, null);
            }
        }

        private void FireOnEscapePressed(object sender, KeyEventArgs e)
        {
            if (OnEscapePressed != null)
            {
                OnEscapePressed(sender, e);
            }
        }

        private Action<string> log = x => System.Diagnostics.Debug.WriteLine(x);


        public void Update()
        {

            DrawListBox();
            this.Invalidate();
        }

        private int SelectedItemFromMouseClick(Point pt)
        {
            var retVal = 0;

            if (!_shown) { return 0; }
            log($"{pt.X}:{pt.Y}");

            var computedSelection = pt.Y / _itemHeight;
            if (computedSelection > _items.Count - 1)
            {
                retVal = 0;
            }
            else
            {
                retVal = computedSelection + _topItemInList;
            }

            return retVal;
        }

        private bool IsSelectedItemInView()
        {
            var retVal = false;

            if (!_shown) { return false; }

            var bottomItemOnDisplayFully = _topItemInList + this.ClientRectangle.Height / _itemHeight - 1;
            log($"bottomItemOnDisplayFully :  {bottomItemOnDisplayFully}");
            log($"selected {_selectedItem} : top {_topItemInList}");
            if (_selectedItem < _topItemInList)
            {
                retVal = true;
            }
            else if (_selectedItem > bottomItemOnDisplayFully)
            {
                retVal = true;
            }


            return retVal;
        }

        private void DrawSmallerListBox()
        {
            if (!_shown) { return; }

            if (_items.Count == 0) { return; }

            if (this.ClientRectangle.Height == 0 || this.ClientRectangle.Width == 0) { return; }

            if (IsSelectedItemInView())
            {
                log("Sould scroll into view");
                if (_direction == Direction.Up)
                {
                    _topItemInList--;
                }
                else
                {
                    _topItemInList++;
                }
            }
            else
            {
                log("Just shift the focus");
            }


            bitmap = new Bitmap(this.ClientRectangle.Width, this.ClientRectangle.Height);
            Graphics g = Graphics.FromImage(bitmap);

            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            g.TextRenderingHint = TextRenderingHint.AntiAlias;
            var currentTop = 0;
            for (int index = _topItemInList; index < _items.Count; index++)
            {
                string item = _items[index];

                var rect = new Rectangle(2, currentTop, this.ClientRectangle.Width - 8, _itemHeight);

                StringFormat stringFormat = new StringFormat();
                // stringFormat.Alignment = StringAlignment.Near;
                //  stringFormat.LineAlignment = StringAlignment.Center;
                stringFormat.Trimming = StringTrimming.EllipsisCharacter;
                stringFormat.Trimming = StringTrimming.Character;
                stringFormat.FormatFlags = StringFormatFlags.NoWrap;

                if (index == _selectedItem)
                {

                    FillRoundedRectangle(g, new SolidBrush(Color.FromArgb(48, 48, 48)), rect, 2);

                    Rectangle rect2 = new Rectangle(rect.Left, rect.Top + 5, 3, rect.Height - 10);
                    FillRoundedRectangle(g, new SolidBrush(Color.FromArgb(30, 155, 250)), rect2, 2);

                }
                else
                {
                    g.FillRectangle(new SolidBrush(Configuration.BackColor), rect);

                }
                if (FilesToFlag.ContainsKey(item.ToLower()))
                {
                    if (FilesToFlag[item.ToLower()].AutoCopy)
                    {
                        var img = new Bitmap(Resources.copyto);
                        var imgRect = new Rectangle(rect.X + 15, rect.Y + 20, 16, 16);
                        g.DrawImage(img, imgRect);
                    }

                }

                var font = new Font("Segoe UI", 12, FontStyle.Bold);

                var rect1 = new Rectangle();
                rect1.X = rect.Left + 40;
                rect1.Y = rect.Y + rect.Height / 2 - Font.Height / 2;
                rect1.Width = rect.Width - 80;
                rect1.Height = rect.Height - 2;


                g.DrawString(item, font, new SolidBrush(Color.White), rect1, stringFormat);


                currentTop += _itemHeight;

            }
        }

        private void DrawListBox()
        {
          
            if (!_shown) { return; }

            if( _items.Count == 0 ) {  return; }

            if(this.ClientRectangle.Height == 0 || this.ClientRectangle.Width ==0  ) { return; }

            if(!ShowTags)
            {
                DrawSmallerListBox();
                return;
            }


            if (IsSelectedItemInView())
            {
                log("Sould scroll into view");
                if (_direction == Direction.Up)
                {
                    _topItemInList--;
                }
                else
                {
                    _topItemInList++;
                }
            }
            else
            {
                log("Just shift the focus");
            }


            bitmap = new Bitmap(this.ClientRectangle.Width, this.ClientRectangle.Height);
            Graphics g = Graphics.FromImage(bitmap);

            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            g.TextRenderingHint = TextRenderingHint.AntiAlias;
            var currentTop = 0;
            for (int index = _topItemInList; index < _items.Count; index++)
            {
                string item = _items[index];

                var rect = new Rectangle(2, currentTop, this.ClientRectangle.Width - 8, _itemHeight);

                StringFormat stringFormat = new StringFormat();
               // stringFormat.Alignment = StringAlignment.Near;
              //  stringFormat.LineAlignment = StringAlignment.Center;
                stringFormat.Trimming = StringTrimming.EllipsisCharacter;
                stringFormat.Trimming = StringTrimming.Character;
                stringFormat.FormatFlags = StringFormatFlags.NoWrap;

                if (index == _selectedItem)
                {

                    FillRoundedRectangle(g, new SolidBrush(Color.FromArgb(48, 48, 48)), rect, 2);

                    Rectangle rect2 = new Rectangle(rect.Left, rect.Top + 15, 3, rect.Height - 30);
                    FillRoundedRectangle(g, new SolidBrush(Color.FromArgb(30, 155, 250)), rect2, 2);

                }
                else
                {
                    g.FillRectangle(new SolidBrush(Configuration.BackColor), rect);

                }
                if (FilesToFlag.ContainsKey(item.ToLower()))
                {
                    if (FilesToFlag[item.ToLower()].AutoCopy)
                    {
                        var img = new Bitmap(Resources.copyto);
                        var imgRect = new Rectangle(rect.X + 15, rect.Y + 20, 16, 16);
                        g.DrawImage(img, imgRect);
                    }

                }

                var font = new Font("Segoe UI", 12, FontStyle.Bold);

                var rect1 = new Rectangle();
                rect1.X = rect.Left + 40;
                rect1.Y = rect.Y + 2;
                rect1.Width = rect.Width - 80;
                rect1.Height = rect.Height - 2;

                g.DrawString(item, font, new SolidBrush(Color.White), rect1, stringFormat);

                
                var smallerFont = new Font("Segoe UI Semibold", 10);

                if (FilesToFlag.ContainsKey(item.ToLower()))
                {
                    var tags = FilesToFlag[item.ToLower()].Tags.Select(x => x.ToString()).ToArray();
                    var taglist = string.Join(", ", tags);
                    rect1.Offset(2, 25);
                    g.DrawString($"Tags: {taglist}", smallerFont, new SolidBrush(Color.White), rect1, stringFormat); ;
                }
                else
                {
                    rect1.Offset(2, 25);
                    g.DrawString($"Tags: <none>", smallerFont, new SolidBrush(Color.Gray), rect1,stringFormat); ;
                }
                


                currentTop += _itemHeight;

            }


        }


        private void CustomListBox_SizeChanged(object sender, EventArgs e)
        {
            if (!_shown) { return; }

            DrawListBox();

        }

        private void CustomListBox_VisibleChanged(object sender, EventArgs e)
        {
            _shown = true;
        }

        private void CustomListBox_Load(object sender, EventArgs e)
        {
            if (_shown) { CustomListBox_SizeChanged(null, null); };
        }


        private void CustomListBox_Paint(object sender, PaintEventArgs e)
        {
            if (!_shown) { return; }
            if (bitmap == null) { return; }
            var myg = e.Graphics;
            myg.DrawImage(bitmap, 0, 0);

        }


        private void FillRoundedRectangle(Graphics graphics, Brush brush, Rectangle bounds, int cornerRadius)
        {
            if (graphics == null)
                throw new ArgumentNullException(nameof(graphics));
            if (brush == null)
                throw new ArgumentNullException(nameof(brush));

            using (GraphicsPath path = RoundedRect(bounds, cornerRadius))
            {
                graphics.FillPath(brush, path);
            }
        }

        private GraphicsPath RoundedRect(Rectangle bounds, int radius)
        {
            int diameter = radius * 2;
            Size size = new Size(diameter, diameter);
            Rectangle arc = new Rectangle(bounds.Location, size);
            GraphicsPath path = new GraphicsPath();

            if (radius == 0)
            {
                path.AddRectangle(bounds);
                return path;
            }

            // top left arc  
            path.AddArc(arc, 180, 90);

            // top right arc  
            arc.X = bounds.Right - diameter;
            path.AddArc(arc, 270, 90);

            // bottom right arc  
            arc.Y = bounds.Bottom - diameter;
            path.AddArc(arc, 0, 90);

            // bottom left arc 
            arc.X = bounds.Left;
            path.AddArc(arc, 90, 90);

            path.CloseFigure();
            return path;
        }



        private void CustomListBox_MouseDown(object sender, MouseEventArgs e)
        {
            _selectedItem = SelectedItemFromMouseClick(e.Location);
            DrawListBox();
            Invalidate();
            FireOnItemSelected();
        }

        private void CustomListBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            log($"Keypress {e.KeyChar}");
        }

        private void CustomListBox_KeyDown(object sender, KeyEventArgs e)
        {
            log($"CustomListBox_KeyDown {e.KeyCode}");
            switch (e.KeyCode)
            {
                case Keys.Down:
                    _selectedItem = _selectedItem == _items.Count - 1 ? _selectedItem : _selectedItem + 1;
                    _direction = Direction.Down;
                    DrawListBox();
                    Invalidate();
                    FireOnItemSelected();
                    break;
                case Keys.Up:
                    log("keyp up");
                    _selectedItem = _selectedItem == 0 ? _selectedItem : _selectedItem - 1;
                    _direction = Direction.Up;
                    DrawListBox();
                    Invalidate();
                    FireOnItemSelected();
                    break;
                case Keys.Enter:
                    log($"Enter pressed : iem is {_items[_selectedItem]}");
                    FireOnEscapePressed(sender, e);
                    DrawListBox();
                    Invalidate();
                    FireOnItemSelected();
                    break;
                case Keys.Escape:
                    FireOnEscapePressed(sender, e);
                    break;
            }

        }

        private void CustomListBox_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Escape:
                case Keys.Enter:
                case Keys.Down:
                case Keys.Up:
                    e.IsInputKey = true;
                    break;
            }

        }

        private void CustomListBox_Scroll(object sender, ScrollEventArgs e)
        {
            log("int the scrolol");
        }
        public void KeyWasPressed(KeyEventArgs e)
        {
            this.CustomListBox_KeyDown(this, e);
        }
    }
}

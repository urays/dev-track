using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace IDevTrack.STReader
{
    [DefaultProperty("Color")]
    [DefaultEvent("ColorChanged")]
    internal class ColorDropDown : ToolStripDropDown
    {
        #region Constants

        private static readonly object _eventColorChanged = new object();

        #endregion Constants

        #region Fields

        private Color _color;

        private ColorBoard _colorboard;

        #endregion Fields

        #region Constructors

        public ColorDropDown()
        {
            _colorboard = new ColorBoard();
            _colorboard.BackColorChanged += ColorChangedHandler;
            Items.Add(new ToolStripControlHost(_colorboard));
        }

        #endregion Constructors

        #region Events

        [Category("Value Changed")]
        public event EventHandler ColorChanged
        {
            add { Events.AddHandler(_eventColorChanged, value); }
            remove { Events.RemoveHandler(_eventColorChanged, value); }
        }

        #endregion Events

        #region Properties

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color Color
        {
            get { return _color; }
            set
            {
                if (Color != value)
                {
                    _color = value;
                    OnColorChanged(EventArgs.Empty);
                }
            }
        }

        #endregion Properties

        #region Methods

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Items.Clear();
                _colorboard.BackColorChanged -= ColorChangedHandler;
                _colorboard.Dispose();
                _colorboard = null;
            }

            base.Dispose(disposing);
        }

        protected virtual void OnColorChanged(EventArgs e)
        {
            EventHandler handler;
            handler = (EventHandler)Events[_eventColorChanged];
            handler?.Invoke(this, e);
        }

        protected override void OnOpened(EventArgs e)
        {
            base.OnOpened(e);
            _colorboard.Focus();
        }

        private void ColorChangedHandler(object sender, EventArgs e)
        {
            Close(ToolStripDropDownCloseReason.ItemClicked);
            Color = _colorboard.BackColor;
        }

        #endregion Methods
    }
}
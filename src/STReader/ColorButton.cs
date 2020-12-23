using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace IDevTrack.STReader
{
    [DefaultProperty("Color")]
    [DefaultEvent("ColorChanged")]
    [ToolStripItemDesignerAvailability(ToolStripItemDesignerAvailability.ToolStrip)]
    public class ColorButton : ToolStripDropDownButton
    {
        #region Constants

        private static readonly object _eventColorChanged = new object();

        #endregion Constants

        #region Fields

        private ColorDropDown _dropDown;

        #endregion Fields

        #region Constructors

        public ColorButton()
        {
            BackColor = Color.Green;
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

        [Category("Data")]
        [DefaultValue(typeof(Color), "Green")]
        public virtual Color Color
        {
            get { return BackColor; }
            set
            {
                if (Color != value)
                {
                    BackColor = value;
                    OnColorChanged(EventArgs.Empty);
                }
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new ToolStripDropDown DropDown
        {
            get { return base.DropDown; }
            set { base.DropDown = value; }
        }

        #endregion Properties

        #region Methods

        protected override ToolStripDropDown CreateDefaultDropDown()
        {
            EnsureDropDownIsCreated();

            return _dropDown;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_dropDown != null)
                {
                    DropDown = null;
                    _dropDown.Dispose();
                    _dropDown.ColorChanged -= DropDownColorChangedHandler;
                    _dropDown = null;
                }
            }

            base.Dispose(disposing);
        }

        protected virtual void OnColorChanged(EventArgs e)
        {
            EventHandler handler;

            Invalidate();

            handler = (EventHandler)Events[_eventColorChanged];

            handler?.Invoke(this, e);
        }

        protected override void OnDropDownShow(EventArgs e)
        {
            EnsureDropDownIsCreated();
            base.OnDropDownShow(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
        }

        private void DropDownColorChangedHandler(object sender, EventArgs e)
        {
            Color = _dropDown.Color;
        }

        private void EnsureDropDownIsCreated()
        {
            if (_dropDown == null)
            {
                _dropDown = new ColorDropDown();
                _dropDown.ColorChanged += DropDownColorChangedHandler;
            }
        }

        #endregion Methods
    }
}
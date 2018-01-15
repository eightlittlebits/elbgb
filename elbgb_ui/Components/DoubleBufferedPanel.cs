using System.Windows.Forms;

namespace elbgb_ui.Components
{
    class DoubleBufferedPanel : Panel
    {
        private bool _realTimeUpdate;

        public DoubleBufferedPanel()
        {
            this.SetStyle(ControlStyles.ResizeRedraw, true);
        }

        public bool RealTimeUpdate
        {
            get => _realTimeUpdate;
            set
            {
                _realTimeUpdate = value;

                if (value == true)
                {
                    this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);

                    // these settings as applied below conflict with the documents,
                    // we're ok in this instance though as we're rendering the control
                    // ourselves constantly. We don't need windows to be raising either
                    // the WM_ERASEBKGND or WM_PAINT events. Seetting 
                    // AllPaintingInWmPaint to true prevents WM_ERASEBKGND and setting
                    // UserPaint to false prevents WM_PAINT
                    this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
                    this.SetStyle(ControlStyles.UserPaint, false);
                }
                else
                {
                    this.SetStyle(ControlStyles.OptimizedDoubleBuffer, false);
                    this.SetStyle(ControlStyles.AllPaintingInWmPaint, false);
                    this.SetStyle(ControlStyles.UserPaint, false);
                }

                this.UpdateStyles();
            }
        }
    }
}

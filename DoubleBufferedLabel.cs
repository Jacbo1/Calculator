using System.Windows.Forms;

namespace Calculator
{
    public partial class DoubleBufferedLabel : Label
    {
        public DoubleBufferedLabel()
        {
            SetStyle(ControlStyles.DoubleBuffer | ControlStyles.OptimizedDoubleBuffer, true);
        }
    }
}

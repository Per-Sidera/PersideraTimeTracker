using System.Drawing;
using System.Windows.Forms;

namespace PersideraTimeTracker.Form
{
    /// <summary>
    /// Renders MenuStrip / ContextMenuStrip surfaces with the Persidera dark
    /// palette: dark backgrounds, bone text, and a star-tinted hover highlight.
    /// </summary>
    internal sealed class DarkMenuRenderer : ToolStripProfessionalRenderer
    {
        public DarkMenuRenderer() : base(new DarkColorTable()) { }

        protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
        {
            e.TextColor = e.Item.Selected ? Theme.Ink : Theme.Bone;
            base.OnRenderItemText(e);
        }

        protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
        {
            e.ArrowColor = e.Item.Selected ? Theme.Ink : Theme.BoneMute;
            base.OnRenderArrow(e);
        }
    }

    internal sealed class DarkColorTable : ProfessionalColorTable
    {
        public DarkColorTable() { UseSystemColors = false; }

        public override Color ToolStripDropDownBackground => Theme.InkElev;
        public override Color ImageMarginGradientBegin => Theme.InkElev;
        public override Color ImageMarginGradientMiddle => Theme.InkElev;
        public override Color ImageMarginGradientEnd => Theme.InkElev;

        public override Color MenuBorder => Theme.InkLine;
        public override Color MenuItemBorder => Theme.Star;

        public override Color MenuItemSelected => Theme.Star;
        public override Color MenuItemSelectedGradientBegin => Theme.Star;
        public override Color MenuItemSelectedGradientEnd => Theme.Star;
        public override Color MenuItemPressedGradientBegin => Theme.InkElev;
        public override Color MenuItemPressedGradientEnd => Theme.InkElev;

        public override Color MenuStripGradientBegin => Theme.InkElev;
        public override Color MenuStripGradientEnd => Theme.InkElev;

        public override Color SeparatorDark => Theme.InkLine;
        public override Color SeparatorLight => Theme.InkLine;
    }
}

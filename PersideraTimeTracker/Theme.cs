using System.Drawing;

namespace PersideraTimeTracker
{
    /// <summary>
    /// Persidera Industries brand palette and fonts. Colors are taken verbatim
    /// from the Persidera website CSS variables so the desktop app matches the
    /// brand exactly.
    /// </summary>
    public static class Theme
    {
        // Surfaces
        public static readonly Color Ink        = Color.FromArgb(0x19, 0x19, 0x19); // background
        public static readonly Color InkElev    = Color.FromArgb(0x1F, 0x1F, 0x1F); // elevated panels
        public static readonly Color InkLine    = Color.FromArgb(0x2A, 0x2A, 0x2A); // borders / lines

        // Text
        public static readonly Color Bone       = Color.FromArgb(0xF1, 0xF1, 0xF1); // primary text
        public static readonly Color BoneMute   = Color.FromArgb(0x9A, 0x9A, 0x9A); // muted text
        public static readonly Color BoneDim    = Color.FromArgb(0x6A, 0x6A, 0x6A); // dim text

        // Accents
        public static readonly Color Star       = Color.FromArgb(0xF2, 0xDC, 0x14); // yellow accent
        public static readonly Color Ember      = Color.FromArgb(0xFF, 0x6A, 0x1F); // ember orange (CTAs)
        public static readonly Color Danger     = Color.FromArgb(0xEF, 0x16, 0x25); // error / danger

        // Hover variants (slightly darkened)
        public static readonly Color EmberHover = Color.FromArgb(0xE0, 0x5C, 0x14);
        public static readonly Color InkHover   = Color.FromArgb(0x26, 0x26, 0x26);

        // Fonts
        public static readonly Font  FontBase   = new Font("Segoe UI", 9f);
        public static readonly Font  FontMono   = new Font("Consolas", 9f);
        public static readonly Font  FontLarge  = new Font("Segoe UI", 20f, FontStyle.Bold);
        public static readonly Font  FontMed    = new Font("Segoe UI", 11f, FontStyle.Bold);
        public static readonly Font  FontSmall  = new Font("Segoe UI", 8f);

        /// <summary>Big monospaced font for the live timer display.</summary>
        public static readonly Font  FontTimer  = new Font("Consolas", 32f, FontStyle.Bold);
    }
}

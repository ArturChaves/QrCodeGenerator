using System;
using Microsoft.Maui.Graphics;

namespace QrCodeGenerator.Controls.Drawables
{
    /// <summary>
    /// Drawable de QR Code com opção para usar círculos nos módulos comuns.
    /// Os 3 detectores sempre são quadrados com cantos arredondados.
    /// </summary>
    public sealed class QrDrawable : IDrawable
    {
        private bool[,] _matrix;

        public QrDrawable(
            bool[,] matrix,
            Color foreground,
            Color background,
            int quietZone = 4,
            bool fitToAvailable = true,
            double moduleSize = 8.0,
            bool pixelPerfect = true,
            bool useCircles = false,
            float finderCornerRadiusFactor = 0.15f
        )
        {
            _matrix = matrix ?? new bool[21, 21];
            ForegroundColor = foreground;
            BackgroundColor = background;
            QuietZone = Math.Max(0, quietZone);
            FitToAvailable = fitToAvailable;
            ModuleSize = Math.Max(1.0, moduleSize);
            PixelPerfect = pixelPerfect;
            UseCircles = useCircles;
            FinderCornerRadiusFactor = Math.Clamp(finderCornerRadiusFactor, 0f, 0.5f);
        }

        public bool[,] Matrix
        {
            get => _matrix;
            set => _matrix = value ?? new bool[21, 21];
        }

        public Color ForegroundColor { get; set; }
        public Color BackgroundColor { get; set; }
        public int QuietZone { get; set; } = 4;
        public bool FitToAvailable { get; set; } = true;
        public double ModuleSize { get; set; } = 8.0;
        public bool PixelPerfect { get; set; } = true;

        /// <summary>Se true, módulos fora dos detectores são círculos; senão, quadrados.</summary>
        public bool UseCircles { get; set; } = false;

        /// <summary>Raio de canto dos detectores como fração do lado (0..0.5).</summary>
        public float FinderCornerRadiusFactor { get; set; } = 0.15f;

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            var m = _matrix ?? new bool[21, 21];
            int n = m.GetLength(0);

            canvas.FillColor = BackgroundColor;
            canvas.FillRectangle(dirtyRect);

            float s;
            if (FitToAvailable)
            {
                float sX = dirtyRect.Width / (n + 2f * QuietZone);
                float sY = dirtyRect.Height / (n + 2f * QuietZone);
                s = MathF.Min(sX, sY);
                if (PixelPerfect) s = MathF.Max(1, MathF.Floor(s));
            }
            else s = (float)ModuleSize;

            float totalW = (n + 2f * QuietZone) * s;
            float totalH = (n + 2f * QuietZone) * s;
            float offX = dirtyRect.X + (dirtyRect.Width - totalW) / 2f;
            float offY = dirtyRect.Y + (dirtyRect.Height - totalH) / 2f;

            // Detectores
            DrawFinderAt(canvas, offX, offY, s, 0, 0);
            DrawFinderAt(canvas, offX, offY, s, 0, n - 7);
            DrawFinderAt(canvas, offX, offY, s, n - 7, 0);

            // Módulos restantes
            canvas.FillColor = ForegroundColor;
            int q = QuietZone;

            for (int r = 0; r < n; r++)
            {
                for (int c = 0; c < n; c++)
                {
                    if (IsInsideFinderRegion(r, c, n)) continue;
                    if (!m[r, c]) continue;

                    float x = offX + (q + c) * s;
                    float y = offY + (q + r) * s;

                    if (UseCircles)
                        canvas.FillEllipse(x, y, s, s);
                    else
                        canvas.FillRectangle(x, y, s, s);
                }
            }
        }

        private static bool IsInsideFinderRegion(int r, int c, int n) =>
            (r < 7 && c < 7) || (r < 7 && c >= n - 7) || (r >= n - 7 && c < 7);

        private void DrawFinderAt(ICanvas canvas, float offX, float offY, float s, int row, int col)
        {
            float x = offX + (QuietZone + col) * s;
            float y = offY + (QuietZone + row) * s;

            float outerSize = 7 * s;
            float midSize = 5 * s;
            float innerSize = 3 * s;

            float rrOuter = outerSize * FinderCornerRadiusFactor;
            float rrMid = midSize * FinderCornerRadiusFactor;
            float rrInner = innerSize * FinderCornerRadiusFactor;

            canvas.FillColor = ForegroundColor;
            FillRoundedRect(canvas, x, y, outerSize, outerSize, rrOuter);

            canvas.FillColor = BackgroundColor;
            FillRoundedRect(canvas, x + s, y + s, midSize, midSize, rrMid);

            canvas.FillColor = ForegroundColor;
            FillRoundedRect(canvas, x + 2 * s, y + 2 * s, innerSize, innerSize, rrInner);
        }

        private static void FillRoundedRect(ICanvas canvas, float x, float y, float w, float h, float r)
        {
            float rr = MathF.Min(r, MathF.Min(w, h) * 0.5f);
            var path = new PathF();
            path.AppendRoundedRectangle(x, y, w, h, rr);
            canvas.FillPath(path);
        }
    }
}

namespace QrCodeGenerator.Controls.Drawables
{
    public sealed class QrDrawable : IDrawable
    {
        private bool[,] _matrix;

        public QrDrawable(
            bool[,] matrix,
            Color foreground,
            Color background,
            bool pixelPerfect = true,
            bool useCircles = false
        )
        {
            _matrix = matrix ?? new bool[21, 21];
            ForegroundColor = foreground;
            BackgroundColor = background;
            UseCircles = useCircles;
        }

        public bool[,] Matrix
        {
            get => _matrix;
            set => _matrix = value ?? new bool[21, 21];
        }

        public Color ForegroundColor { get; set; }
        public Color BackgroundColor { get; set; }
        public int QuietZone { get; set; } = 2;
        public bool UseCircles { get; set; } = false;
        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            var m = _matrix ?? new bool[21, 21];
            int n = m.GetLength(0);

            canvas.FillColor = BackgroundColor;
            float radius = MathF.Min(dirtyRect.Width, dirtyRect.Height) * 0.10f;
            var path = new PathF();
            path.AppendRoundedRectangle(dirtyRect.X, dirtyRect.Y, dirtyRect.Width, dirtyRect.Height, radius);
            canvas.FillPath(path);

            float s;

            float sX = dirtyRect.Width / (n + 2f * QuietZone);
            float sY = dirtyRect.Height / (n + 2f * QuietZone);
            s = MathF.Min(sX, sY);
            s = MathF.Max(1, MathF.Floor(s));



            float totalW = (n + 2f * QuietZone) * s;
            float totalH = (n + 2f * QuietZone) * s;
            float offX = dirtyRect.X + (dirtyRect.Width - totalW) / 2f;
            float offY = dirtyRect.Y + (dirtyRect.Height - totalH) / 2f;

            DrawFinderAt(canvas, offX, offY, s, 0, 0);
            DrawFinderAt(canvas, offX, offY, s, 0, n - 7);
            DrawFinderAt(canvas, offX, offY, s, n - 7, 0);

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
                    {
                        canvas.FillEllipse(x, y, s, s);
                    }
                    else
                    {
                        float rr = s * 0.35f;
                        FillRoundedRect(canvas, x, y, s, s, rr);
                    }
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

            float rrOuter = outerSize * 0.35f;
            float rrMid = midSize * 0.35f;
            float rrInner = innerSize * 0.35f;

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

namespace QrCodeGenerator.Controls;

public partial class QrView : ContentView
{
    private bool[,] _qr = new bool[21, 21];

    public QrView()
    {
        InitializeComponent();
        Canvas.Drawable = new QrDrawable(this);
        // inicializa com valores padrão
        UpdateQr(Text);
        // atualiza quando o layout muda (ex.: tamanho alterado)
        Canvas.SizeChanged += (_, __) => Canvas.Invalidate();
    }

    // ===== Bindable Properties =====

    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(nameof(Text), typeof(string), typeof(QrView),
            string.Empty, propertyChanged: (b, o, n) => ((QrView)b).OnTextChanged(n as string));

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public static readonly BindableProperty ForegroundColorProperty =
        BindableProperty.Create(nameof(ForegroundColor), typeof(Color), typeof(QrView),
            Colors.Black, propertyChanged: (b, o, n) => ((QrView)b).Canvas.Invalidate());

    public Color ForegroundColor
    {
        get => (Color)GetValue(ForegroundColorProperty);
        set => SetValue(ForegroundColorProperty, value);
    }

    public static readonly BindableProperty BackgroundColorProperty2 =
        BindableProperty.Create(nameof(BackgroundColor2), typeof(Color), typeof(QrView),
            Colors.White, propertyChanged: (b, o, n) => ((QrView)b).Canvas.Invalidate());

    /// <summary>Cor de fundo do QR (não confundir com BackgroundColor do ContentView).</summary>
    public Color BackgroundColor2
    {
        get => (Color)GetValue(BackgroundColorProperty2);
        set => SetValue(BackgroundColorProperty2, value);
    }

    public static readonly BindableProperty QuietZoneProperty =
        BindableProperty.Create(nameof(QuietZone), typeof(int), typeof(QrView),
            4, propertyChanged: (b, o, n) => ((QrView)b).Canvas.Invalidate());

    /// <summary>Quiet zone (margem) em módulos (recomendado >= 4).</summary>
    public int QuietZone
    {
        get => (int)GetValue(QuietZoneProperty);
        set => SetValue(QuietZoneProperty, Math.Max(0, value));
    }

    public static readonly BindableProperty FitToAvailableProperty =
        BindableProperty.Create(nameof(FitToAvailable), typeof(bool), typeof(QrView),
            true, propertyChanged: (b, o, n) => ((QrView)b).Canvas.Invalidate());

    /// <summary>Se true, ajusta automaticamente a escala para caber na área disponível.</summary>
    public bool FitToAvailable
    {
        get => (bool)GetValue(FitToAvailableProperty);
        set => SetValue(FitToAvailableProperty, value);
    }

    public static readonly BindableProperty ModuleSizeProperty =
        BindableProperty.Create(nameof(ModuleSize), typeof(double), typeof(QrView),
            8d, propertyChanged: (b, o, n) => ((QrView)b).Canvas.Invalidate());

    /// <summary>Tamanho do módulo em pixels quando FitToAvailable=false.</summary>
    public double ModuleSize
    {
        get => (double)GetValue(ModuleSizeProperty);
        set => SetValue(ModuleSizeProperty, Math.Max(1, value));
    }

    public static readonly BindableProperty PixelPerfectProperty =
        BindableProperty.Create(nameof(PixelPerfect), typeof(bool), typeof(QrView),
            true, propertyChanged: (b, o, n) => ((QrView)b).Canvas.Invalidate());

    /// <summary>Se true, força escala inteira (evita borrões).</summary>
    public bool PixelPerfect
    {
        get => (bool)GetValue(PixelPerfectProperty);
        set => SetValue(PixelPerfectProperty, value);
    }

    // ===== Internals =====

    private void OnTextChanged(string? s)
    {
        UpdateQr(s ?? string.Empty);
        Canvas.Invalidate();
    }

    private void UpdateQr(string content)
    {
        try
        {
            _qr = QrEncoder.EncodeAutoM(content);
        }
        catch
        {
            // fallback para um QR mínimo vazio
            _qr = new bool[21, 21];
        }
    }

    // ===== Drawable =====

    private sealed class QrDrawable : IDrawable
    {
        private readonly QrView _owner;
        public QrDrawable(QrView owner) => _owner = owner;

        public void Draw(ICanvas canvas, Microsoft.Maui.Graphics.RectF dirtyRect)
        {
            var m = _owner._qr;
            int n = m.GetLength(0);

            // fundo
            canvas.FillColor = _owner.BackgroundColor2;
            canvas.FillRectangle(dirtyRect);

            // escala
            float s;
            if (_owner.FitToAvailable)
            {
                float sX = dirtyRect.Width / (n + 2f * _owner.QuietZone);
                float sY = dirtyRect.Height / (n + 2f * _owner.QuietZone);
                s = MathF.Min(sX, sY);
                if (_owner.PixelPerfect) s = MathF.Max(1, MathF.Floor(s));
            }
            else
            {
                s = (float)_owner.ModuleSize;
            }

            // tamanho total ocupado
            float totalW = (n + 2f * _owner.QuietZone) * s;
            float totalH = (n + 2f * _owner.QuietZone) * s;

            // centraliza
            float offX = dirtyRect.X + (dirtyRect.Width - totalW) / 2f;
            float offY = dirtyRect.Y + (dirtyRect.Height - totalH) / 2f;

            // desenha módulos
            canvas.FillColor = _owner.ForegroundColor;
            int q = _owner.QuietZone;
            for (int r = 0; r < n; r++)
                for (int c = 0; c < n; c++)
                    if (m[r, c])
                    {
                        float x = offX + (q + c) * s;
                        float y = offY + (q + r) * s;
                        canvas.FillRectangle(x, y, s, s);
                    }
        }
    }
}

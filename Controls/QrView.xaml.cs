using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using QrCodeGenerator.Controls.Drawables;
using QrCodeGenerator.Qr;
using QrSharp.Core;

namespace QrCodeGenerator.Controls
{
    public partial class QrView : ContentView
    {
        private bool[,] _qr = new bool[21, 21];
        private readonly QrDrawable _drawable;

        public QrView()
        {
            InitializeComponent();

            _drawable = new QrDrawable(
                matrix: _qr,
                foreground: Colors.Black,
                background: Colors.White,
                quietZone: 4,
                fitToAvailable: true,
                moduleSize: 8.0,
                pixelPerfect: true
            );

            Canvas.Drawable = _drawable;

            UpdateQr(Text);
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
                Colors.Black, propertyChanged: (b, o, n) =>
                {
                    var v = (QrView)b;
                    v._drawable.ForegroundColor = (Color)n;
                    v.Canvas.Invalidate();
                });

        public Color ForegroundColor
        {
            get => (Color)GetValue(ForegroundColorProperty);
            set => SetValue(ForegroundColorProperty, value);
        }

        public static readonly BindableProperty BackgroundColorProperty2 =
            BindableProperty.Create(nameof(BackgroundColor2), typeof(Color), typeof(QrView),
                Colors.White, propertyChanged: (b, o, n) =>
                {
                    var v = (QrView)b;
                    v._drawable.BackgroundColor = (Color)n;
                    v.Canvas.Invalidate();
                });

        /// <summary>Cor de fundo do QR (não confundir com BackgroundColor do ContentView).</summary>
        public Color BackgroundColor2
        {
            get => (Color)GetValue(BackgroundColorProperty2);
            set => SetValue(BackgroundColorProperty2, value);
        }

        public static readonly BindableProperty QuietZoneProperty =
            BindableProperty.Create(nameof(QuietZone), typeof(int), typeof(QrView),
                4, propertyChanged: (b, o, n) =>
                {
                    var v = (QrView)b;
                    v._drawable.QuietZone = Math.Max(0, (int)n);
                    v.Canvas.Invalidate();
                });

        /// <summary>Quiet zone (margem) em módulos (recomendado >= 4).</summary>
        public int QuietZone
        {
            get => (int)GetValue(QuietZoneProperty);
            set => SetValue(QuietZoneProperty, Math.Max(0, value));
        }

        public static readonly BindableProperty FitToAvailableProperty =
            BindableProperty.Create(nameof(FitToAvailable), typeof(bool), typeof(QrView),
                true, propertyChanged: (b, o, n) =>
                {
                    var v = (QrView)b;
                    v._drawable.FitToAvailable = (bool)n;
                    v.Canvas.Invalidate();
                });

        /// <summary>Se true, ajusta automaticamente a escala para caber na área disponível.</summary>
        public bool FitToAvailable
        {
            get => (bool)GetValue(FitToAvailableProperty);
            set => SetValue(FitToAvailableProperty, value);
        }

        public static readonly BindableProperty UseCirclesProperty =
            BindableProperty.Create(nameof(UseCircles), typeof(bool), typeof(QrView),
                false, propertyChanged: (b, o, n) =>
                {
                    var v = (QrView)b;
                    v._drawable.UseCircles = (bool)n;
                    v.Canvas.Invalidate();
                });

        /// <summary>Define se os módulos comuns do QR serão círculos (true) ou quadrados (false).</summary>
        public bool UseCircles
        {
            get => (bool)GetValue(UseCirclesProperty);
            set => SetValue(UseCirclesProperty, value);
        }

        public static readonly BindableProperty ModuleSizeProperty =
            BindableProperty.Create(nameof(ModuleSize), typeof(double), typeof(QrView),
                8d, propertyChanged: (b, o, n) =>
                {
                    var v = (QrView)b;
                    v._drawable.ModuleSize = Math.Max(1.0, (double)n);
                    v.Canvas.Invalidate();
                });

        /// <summary>Tamanho do módulo em pixels quando FitToAvailable=false.</summary>
        public double ModuleSize
        {
            get => (double)GetValue(ModuleSizeProperty);
            set => SetValue(ModuleSizeProperty, Math.Max(1, value));
        }

        public static readonly BindableProperty PixelPerfectProperty =
            BindableProperty.Create(nameof(PixelPerfect), typeof(bool), typeof(QrView),
                true, propertyChanged: (b, o, n) =>
                {
                    var v = (QrView)b;
                    v._drawable.PixelPerfect = (bool)n;
                    v.Canvas.Invalidate();
                });

        /// <summary>Se true, força escala inteira (evita borrões).</summary>
        public bool PixelPerfect
        {
            get => (bool)GetValue(PixelPerfectProperty);
            set => SetValue(PixelPerfectProperty, value);
        }

        // ===== Correction Level =====

        public static readonly BindableProperty CorrectionLevelProperty =
            BindableProperty.Create(
                nameof(CorrectionLevel),
                typeof(EccLevel),
                typeof(QrView),
                EccLevel.M, // padrão
                propertyChanged: (b, o, n) =>
                {
                    var v = (QrView)b;
                    v.UpdateQr(v.Text);
                });

        /// <summary>Nível de correção de erro: L, M, Q ou H.</summary>
        public EccLevel CorrectionLevel
        {
            get => (EccLevel)GetValue(CorrectionLevelProperty);
            set => SetValue(CorrectionLevelProperty, value);
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
                // agora usa a propriedade CorrectionLevel
                _qr = QrEncoder.EncodeAuto(content, CorrectionLevel);
                _drawable.Matrix = _qr;
            }
            catch
            {
                _qr = new bool[21, 21];
                _drawable.Matrix = _qr;
            }
        }
    }
}

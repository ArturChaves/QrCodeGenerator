using QrCodeGenerator.Controls.Drawables;
using QrCodeGenerator.Qr;
using QrCodeGenerator.Qr.Core;

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
                background: Colors.White
            );

            Canvas.Drawable = _drawable;

            UpdateQr(Text);
            Canvas.SizeChanged += (_, __) => Canvas.Invalidate();
        }


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

        public static readonly BindableProperty QuietZoneColorProperty =
            BindableProperty.Create(nameof(QuietZoneColor), typeof(Color), typeof(QrView),
                Colors.White, propertyChanged: (b, o, n) =>
                {
                    var v = (QrView)b;
                    v._drawable.BackgroundColor = (Color)n;
                    v.Canvas.Invalidate();
                });

        public Color QuietZoneColor
        {
            get => (Color)GetValue(QuietZoneColorProperty);
            set => SetValue(QuietZoneColorProperty, value);
        }

        public static readonly BindableProperty UseCirclesProperty =
            BindableProperty.Create(nameof(UseCircles), typeof(bool), typeof(QrView),
                false, propertyChanged: (b, o, n) =>
                {
                    var v = (QrView)b;
                    v._drawable.UseCircles = (bool)n;
                    v.Canvas.Invalidate();
                });

        public bool UseCircles
        {
            get => (bool)GetValue(UseCirclesProperty);
            set => SetValue(UseCirclesProperty, value);
        }


        public static readonly BindableProperty CorrectionLevelProperty =
            BindableProperty.Create(nameof(CorrectionLevel), typeof(EccLevel), typeof(QrView),
                EccLevel.M, propertyChanged: (b, o, n) =>
                {
                    var v = (QrView)b;
                    v.UpdateQr(v.Text);
                });

        public EccLevel CorrectionLevel
        {
            get => (EccLevel)GetValue(CorrectionLevelProperty);
            set => SetValue(CorrectionLevelProperty, value);
        }

        private void OnTextChanged(string? s)
        {
            UpdateQr(s ?? string.Empty);
            Canvas.Invalidate();
        }

        private void UpdateQr(string content)
        {
            try
            {
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

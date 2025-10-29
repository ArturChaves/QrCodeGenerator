using System.Text;

namespace QrCodeGenerator
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            // valor inicial para facilitar teste
            //InputEditor.Text = "https://api.seuservico.com/v1/mobile/activate?token=" + new string('A', 600);
            //RenderQr();
        }

    //    void OnGenerateClicked(object sender, EventArgs e) => RenderQr();

    //    void OnFillExampleClicked(object sender, EventArgs e)
    //    {
    //        // Exemplo ~1KB para forçar versões maiores (e testar render)
    //        var sb = new StringBuilder();
    //        sb.Append("https://api.seuservico.com/v2/deeplink/open?userId=123&session=");
    //        sb.Append(new string('Z', 900));
    //        InputEditor.Text = sb.ToString();
    //        RenderQr();
    //    }

    //    void RenderQr()
    //    {
    //        try
    //        {
    //            // ECC M (já é o padrão da sua QrEncoder)
    //            string text = InputEditor.Text ?? string.Empty;
    //            var matrix = QrEncoder.EncodeAutoM(text); // bool[,] (true = preto)

    //            int n = matrix.GetLength(0);
    //            int version = (n - 17) / 4;

    //            QrView.Drawable = new QrDrawable(matrix);
    //            QrView.Invalidate();

    //            InfoLabel.Text = $"Versão: {version}  |  Tamanho: {n}×{n}  |  ECC: M  |  Módulos: {n * n}";
    //        }
    //        catch (Exception ex)
    //        {
    //            InfoLabel.Text = "Erro: " + ex.Message;
    //        }
    //    }
    //}

    ///// <summary>
    ///// Desenha o QR a partir da matriz bool[,].
    ///// - quietZone: padrão 4 módulos
    ///// - escala automática para caber no GraphicsView
    ///// - alinhamento de pixels para nitidez
    ///// </summary>
    //public sealed class QrDrawable : IDrawable
    //{
    //    private readonly bool[,] _matrix;
    //    private readonly int _quietModules;

    //    public QrDrawable(bool[,] matrix, int quietModules = 4)
    //    {
    //        _matrix = matrix ?? throw new ArgumentNullException(nameof(matrix));
    //        _quietModules = Math.Max(quietModules, 4);
    //    }

    //    public void Draw(ICanvas canvas, RectF dirtyRect)
    //    {
    //        canvas.SaveState();

    //        // Fundo
    //        canvas.FillColor = Colors.White;
    //        canvas.FillRectangle(dirtyRect);

    //        int n = _matrix.GetLength(0);
    //        int totalModules = n + 2 * _quietModules;

    //        // Calcula o tamanho do módulo inteiro (floor para evitar borrão)
    //        float moduleSize = (float)Math.Floor(Math.Min(dirtyRect.Width, dirtyRect.Height) / totalModules);
    //        if (moduleSize < 1f) moduleSize = 1f;

    //        // Calcula a área efetiva e centraliza
    //        float qrSize = moduleSize * totalModules;
    //        float offsetX = dirtyRect.X + (dirtyRect.Width - qrSize) / 2f;
    //        float offsetY = dirtyRect.Y + (dirtyRect.Height - qrSize) / 2f;

    //        // Origem no início da quiet zone
    //        float originX = offsetX + moduleSize * _quietModules;
    //        float originY = offsetY + moduleSize * _quietModules;

    //        // Desenha módulos pretos
    //        canvas.FillColor = Colors.Black;

    //        // Dica de performance: desenhar em linhas contíguas
    //        for (int r = 0; r < n; r++)
    //        {
    //            int c = 0;
    //            while (c < n)
    //            {
    //                // pula brancos
    //                while (c < n && !_matrix[r, c]) c++;
    //                if (c >= n) break;

    //                int start = c;
    //                // encontra run de pretos contíguos
    //                while (c < n && _matrix[r, c]) c++;
    //                int end = c; // exclusivo

    //                // desenha um bloco horizontal
    //                float x = originX + start * moduleSize;
    //                float y = originY + r * moduleSize;
    //                float w = (end - start) * moduleSize;
    //                float h = moduleSize;

    //                // arredonda para 0.5px se quiser forçar nitidez em alguns devices
    //                canvas.FillRectangle(x, y, w, h);
    //            }
    //        }

    //        canvas.RestoreState();
    //    }
    }
}

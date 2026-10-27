using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

// ===================== PROGRAMA PRINCIPAL =====================
static class Program
{
    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new MenuForm());
    }
}

// ===================== CLASSE BASE (herança) =====================
// Todas as telas de fórmula herdam daqui: muda só o título, os campos e o cálculo.
class TelaFormula : Form
{
    private readonly TextBox[] caixas;
    private readonly Label lblResultado;
    private readonly Func<double[], string> calcular;

    public TelaFormula(string titulo, string formula, string[] campos, Func<double[], string> calcular)
    {
        this.calcular = calcular;

        Text = titulo;
        ClientSize = new Size(360, 130 + campos.Length * 40 + 60);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterParent;

        var lblFormula = new Label
        {
            Text = formula,
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleCenter,
            Location = new Point(10, 10),
            Size = new Size(340, 40)
        };
        Controls.Add(lblFormula);

        caixas = new TextBox[campos.Length];
        int y = 65;
        for (int i = 0; i < campos.Length; i++)
        {
            Controls.Add(new Label { Text = campos[i], Location = new Point(15, y + 3), Size = new Size(190, 22) });
            caixas[i] = new TextBox { Location = new Point(210, y), Size = new Size(130, 25) };
            Controls.Add(caixas[i]);
            y += 40;
        }

        var btnCalcular = new Button
        {
            Text = "Calcular",
            Location = new Point(15, y + 5),
            Size = new Size(325, 35)
        };
        btnCalcular.Click += BtnCalcular_Click;
        Controls.Add(btnCalcular);
        AcceptButton = btnCalcular; // Enter também calcula

        lblResultado = new Label
        {
            Text = "Resultado:",
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            Location = new Point(15, y + 50),
            Size = new Size(325, 40)
        };
        Controls.Add(lblResultado);
        ClientSize = new Size(360, y + 100);
    }

    public void DefinirValorInicial(int indice, string valor)
    {
        caixas[indice].Text = valor;
    }

    private void BtnCalcular_Click(object sender, EventArgs e)
    {
        double[] valores = new double[caixas.Length];
        for (int i = 0; i < caixas.Length; i++)
        {
            string texto = caixas[i].Text.Replace(',', '.');
            if (!double.TryParse(texto, NumberStyles.Float, CultureInfo.InvariantCulture, out valores[i]))
            {
                lblResultado.Text = "Preencha todos os campos com números.";
                return;
            }
        }
        lblResultado.Text = "Resultado: " + calcular(valores);
    }

    protected static string Fmt(double v) => v.ToString("0.####", CultureInfo.CurrentCulture);

    // ---------- Fábricas das 6 telas (cada uma configura a fórmula correspondente) ----------
    public static TelaFormula VelocidadeMedia() => new TelaFormula(
        "Velocidade Média", "Vm = ΔS / Δt",
        new[] { "Distância ΔS (m)", "Tempo Δt (s)" },
        v => v[1] == 0 ? "O tempo não pode ser zero." : Fmt(v[0] / v[1]) + " m/s");

    public static TelaFormula Aceleracao() => new TelaFormula(
        "Aceleração", "a = Δv / Δt",
        new[] { "Variação de velocidade Δv (m/s)", "Tempo Δt (s)" },
        v => v[1] == 0 ? "O tempo não pode ser zero." : Fmt(v[0] / v[1]) + " m/s²");

    public static TelaFormula Forca() => new TelaFormula(
        "Força", "F = m · a",
        new[] { "Massa m (kg)", "Aceleração a (m/s²)" },
        v => Fmt(v[0] * v[1]) + " N");

    public static TelaFormula ForcaAtrito() => new TelaFormula(
        "Força de Atrito", "Fat = μ · N",
        new[] { "Coeficiente de atrito μ", "Força normal N (N)" },
        v => Fmt(v[0] * v[1]) + " N");

    public static TelaFormula Peso()
    {
        var t = new TelaFormula(
            "Peso", "P = m · g",
            new[] { "Massa m (kg)", "Gravidade g (m/s²)" },
            v => Fmt(v[0] * v[1]) + " N");
        t.DefinirValorInicial(1, "9,8");
        return t;
    }

    public static TelaFormula LeiDeOhm() => new TelaFormula(
        "1ª Lei de Ohm", "V = R · i",
        new[] { "Resistência R (Ω)", "Corrente i (A)" },
        v => Fmt(v[0] * v[1]) + " V");
}

// ===================== TELA INICIAL (6 botões) =====================
class MenuForm : Form
{
    public MenuForm()
    {
        Text = "Calculadora de Fórmulas de Física";
        ClientSize = new Size(520, 260);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;

        string[] nomes = { "1 - Velocidade Média", "2 - Aceleração", "3 - Força",
                           "4 - Força de Atrito", "5 - Peso", "6 - 1ª Lei de Ohm" };

        Func<TelaFormula>[] telas = {
            TelaFormula.VelocidadeMedia, TelaFormula.Aceleracao, TelaFormula.Forca,
            TelaFormula.ForcaAtrito, TelaFormula.Peso, TelaFormula.LeiDeOhm
        };

        for (int i = 0; i < 6; i++)
        {
            int coluna = i % 3;
            int linha = i / 3;
            var btn = new Button
            {
                Text = nomes[i],
                Size = new Size(150, 90),
                Location = new Point(15 + coluna * 165, 30 + linha * 110),
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            Func<TelaFormula> criar = telas[i];
            btn.Click += (s, e) =>
            {
                using (var tela = criar())
                {
                    tela.ShowDialog(this);
                }
            };
            Controls.Add(btn);
        }
    }
}
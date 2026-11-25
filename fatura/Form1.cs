using System;
using System.Drawing;
using System.Drawing.Printing;
using System.Globalization;
using System.Windows.Forms;

namespace fatura
{
    public partial class Form1 : Form
    {
        private readonly PrintDocument printDocument = new PrintDocument();
        private readonly PrintPreviewDialog printPreview = new PrintPreviewDialog();
        private readonly CultureInfo trCulture = new CultureInfo("tr-TR");

        public Form1()
        {
            InitializeComponent();
            printDocument.PrintPage += PrintDocument_PrintPage;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            dataGridView1.Columns.Clear();
            dataGridView1.Columns.Add("UrunAdi", "Ürün Adı");
            dataGridView1.Columns.Add("Adet", "Adet");
            dataGridView1.Columns.Add("BirimFiyat", "Birim Fiyat (TL)");
            dataGridView1.Columns.Add("Toplam", "Toplam (TL)");

            dataGridView1.Columns["Toplam"].ReadOnly = true;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView1.CellEndEdit -= DataGridView1_CellEndEdit;
            dataGridView1.CellEndEdit += DataGridView1_CellEndEdit;
        }

        private bool TryParseDecimal(object value, out decimal result)
        {
            string text = Convert.ToString(value)?.Trim() ?? string.Empty;
            // Nokta veya virgül farkını esnek yönetmek için
            return decimal.TryParse(text, NumberStyles.Any, trCulture, out result)
                || decimal.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out result);
        }

        private void DataGridView1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= dataGridView1.Rows.Count)
                return;

            DataGridViewRow satir = dataGridView1.Rows[e.RowIndex];
            if (satir.IsNewRow)
                return;

            TryParseDecimal(satir.Cells["Adet"].Value, out decimal adet);
            TryParseDecimal(satir.Cells["BirimFiyat"].Value, out decimal birimFiyat);

            satir.Cells["Toplam"].Value = (adet * birimFiyat).ToString("N2", trCulture);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            dataGridView1.EndEdit(); // Aktif hücre düzenlemesini bitir
            printPreview.Document = printDocument;
            printPreview.Width = 800;
            printPreview.Height = 600;
            printPreview.ShowDialog();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            textBox1.Clear();
            txtMusteri.Clear();
            txtAdres.Clear();
            txtFaturaNo.Clear();
            dateTimePicker1.Value = DateTime.Now;
            dataGridView1.Rows.Clear();
        }

        private void PrintDocument_PrintPage(object sender, PrintPageEventArgs e)
        {
            Graphics g = e.Graphics;

            using (Font baslikFont = new Font("Arial", 18, FontStyle.Bold))
            using (Font altBaslikFont = new Font("Arial", 12, FontStyle.Bold))
            using (Font normalFont = new Font("Arial", 11, FontStyle.Regular))
            using (Pen cizgiKalemi = new Pen(Color.Black, 1))
            {
                int x = 50;
                int y = 50;

                string firmaAdi = string.IsNullOrWhiteSpace(textBox1.Text)
                    ? "FİRMA ADI"
                    : textBox1.Text.ToUpper(trCulture);

                g.DrawString(firmaAdi, baslikFont, Brushes.Black, x, y);
                g.DrawString("Tarih: " + dateTimePicker1.Value.ToString("dd.MM.yyyy", trCulture), normalFont, Brushes.Black, 580, y + 5);

                y += 45;
                g.DrawLine(cizgiKalemi, x, y, 750, y);

                y += 20;
                g.DrawString("Müşteri: " + txtMusteri.Text, normalFont, Brushes.Black, x, y);
                y += 25;
                g.DrawString("Adres: " + txtAdres.Text, normalFont, Brushes.Black, x, y);
                y += 25;
                g.DrawString("Fatura No: " + txtFaturaNo.Text, normalFont, Brushes.Black, x, y);

                y += 40;
                g.DrawLine(cizgiKalemi, x, y, 750, y);

                y += 15;
                g.DrawString("Ürün Adı", altBaslikFont, Brushes.Black, x, y);
                g.DrawString("Adet", altBaslikFont, Brushes.Black, 380, y);
                g.DrawString("Birim Fiyat", altBaslikFont, Brushes.Black, 480, y);
                g.DrawString("Toplam", altBaslikFont, Brushes.Black, 630, y);

                y += 30;
                g.DrawLine(cizgiKalemi, x, y, 750, y);
                y += 15;

                decimal araToplam = 0;

                foreach (DataGridViewRow satir in dataGridView1.Rows)
                {
                    if (satir.IsNewRow) continue;

                    string urun = Convert.ToString(satir.Cells["UrunAdi"].Value) ?? string.Empty;
                    TryParseDecimal(satir.Cells["Adet"].Value, out decimal adet);
                    TryParseDecimal(satir.Cells["BirimFiyat"].Value, out decimal birimFiyat);
                    decimal satirToplam = adet * birimFiyat;

                    araToplam += satirToplam;

                    g.DrawString(urun, normalFont, Brushes.Black, x, y);
                    g.DrawString(adet.ToString("0.##", trCulture), normalFont, Brushes.Black, 380, y);
                    g.DrawString(birimFiyat.ToString("N2", trCulture) + " TL", normalFont, Brushes.Black, 480, y);
                    g.DrawString(satirToplam.ToString("N2", trCulture) + " TL", normalFont, Brushes.Black, 630, y);

                    y += 28;
                }

                decimal kdv = araToplam * 0.20m;
                decimal genelToplam = araToplam + kdv;

                y += 15;
                g.DrawLine(cizgiKalemi, x, y, 750, y);

                y += 20;
                g.DrawString("Ara Toplam: " + araToplam.ToString("N2", trCulture) + " TL", normalFont, Brushes.Black, 500, y);
                y += 25;
                g.DrawString("KDV (%20): " + kdv.ToString("N2", trCulture) + " TL", normalFont, Brushes.Black, 500, y);
                y += 30;
                g.DrawString("GENEL TOPLAM: " + genelToplam.ToString("N2", trCulture) + " TL", altBaslikFont, Brushes.Black, 500, y);
            }
        }
    }
}
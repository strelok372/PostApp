using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Mono.Security;
using Npgsql;
using System.Diagnostics;
using System.Collections;

namespace PostApp
{
    public partial class PostApp : Form
    {

        Relabase relabase;
        public PostApp()
        {
            InitializeComponent();
            relabase = new Relabase();

        }

        private void button1_Click(object sender, EventArgs e) //соединение
        {
            try
            {
                relabase.connect();
                label_display_info.Text = "Соединение установлено";
                pictureBox1.BackColor = Color.Green;
                EIGnc(() => relabase.dropBTreeIndex());
                EIGnc(() => relabase.dropGinIndex());
                EIGnc(() => relabase.dropHashIndex());
            }
            catch (Exception)
            {
                MessageBox.Show("Ошибка соединения");
                return;
            }

        }
        private void button2_Click(object sender, EventArgs e) //заполнение
        {
            EIG(() => richTextBox1.AppendText("Добавлено: 1000000 строк за " + relabase.generateWords(1000000).Elapsed.ToString() + " время\n"));
        }
        private void button3_Click(object sender, EventArgs e) //подгрузка таблицы
        {
            try
            {
                var w = relabase.getDatasource();
                dataGridView1.DataSource = w;
                label_row_count.Text = "Всего строк: " + w.Rows.Count.ToString();
            }
            catch (Exception ew)
            {
                label_display_info.Text = ew.Message;
            }
        }
        private void button4_Click(object sender, EventArgs e) //очистка 
        {
            EIG(() => relabase.clearTable());
        }
        private void button5_Click(object sender, EventArgs e) //создание b индекса
        {
            EIGnc(() => relabase.dropHashIndex());
            EIG(() => relabase.addBTreeIndex());
        }
        private void button6_Click(object sender, EventArgs e) //создание hash индекса
        {
            EIGnc(() => relabase.dropBTreeIndex());
            EIG(() => relabase.addHashIndex());
        }
        private void trackBar1_ValueChanged(object sender, EventArgs e)
        {
            label_trackbar_value.Text = trackBar1.Value.ToString();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
        }
        private void button9_Click(object sender, EventArgs e) //(тест) случайное заполнение
        {
            Stopwatch sw = new Stopwatch();
            EIG(() => sw = relabase.generateWords(trackBar1.Value));
            label_time_elapsed.Text = "s:" + sw.Elapsed.Seconds + ":ms" + sw.Elapsed.Milliseconds / 10;
            StringBuilder s = new StringBuilder();
            s.Append(relabase.getIndexes()
                + ". Время работы: "
                + sw.Elapsed.ToString()
                + ". Случайное заполнение. "
                + "Количество данных: "
                + trackBar1.Value
                + '\n'
                );
            richTextBox1.AppendText(s.ToString());
        }
        private void button10_Click(object sender, EventArgs e) //убрать все индексы
        {
            EIGnc(() => relabase.dropBTreeIndex());
            EIGnc(() => relabase.dropGinIndex());
            EIGnc(() => relabase.dropHashIndex());
        }
        private void button8_Click(object sender, EventArgs e) //(тест) случайная выборка
        {
            try
            {
                var w = relabase.randSelect(trackBar1.Value);
                Stopwatch sw = (Stopwatch)w[1];
                dataGridView1.DataSource = (DataTable)w[0];
                label_row_count.Text = "Всего строк: " + ((DataTable)w[0]).Rows.Count.ToString();
                label_time_elapsed.Text = "s:" + sw.Elapsed.Seconds + ":ms" + sw.Elapsed.Milliseconds / 10;
                StringBuilder s = new StringBuilder();
                s.Append(relabase.getIndexes()
                + ". Время работы: "
                + sw.Elapsed.ToString()
                + ". Случайная выборка. "
                + "Количество строк: "
                + trackBar1.Value
                + '\n'
                );
                richTextBox1.AppendText(s.ToString());
            }
            catch (Exception ew)
            {
                richTextBox1.AppendText(ew.Message + '\n');
            }
        }
        private void button7_Click(object sender, EventArgs e) //(тест) выборка из диапазона
        {
            try
            {
                var w = relabase.diapSelect(trackBar1.Value);
                Stopwatch sw = (Stopwatch)w[1];
                dataGridView1.DataSource = (DataTable)w[0];
                label_row_count.Text = "Всего строк: " + ((DataTable)w[0]).Rows.Count.ToString();
                label_time_elapsed.Text = "s:" + sw.Elapsed.Seconds + ":ms" + sw.Elapsed.Milliseconds / 10;
                StringBuilder s = new StringBuilder();
                s.Append(relabase.getIndexes()
                + ". Время работы: "
                + sw.Elapsed.ToString()
                + ". Выборка из диапазона. "
                + "Количество строк: "
                + ((DataTable)w[0]).Rows.Count.ToString()
                + '\n'
                );
                richTextBox1.AppendText(s.ToString());
            }
            catch (Exception ew)
            {
                richTextBox1.AppendText(ew.Message + '\n');
            }
        }
        public void EIG(Action action)
        {
            try
            {
                action.Invoke();
            }
            catch (Exception ew)
            {
                richTextBox1.AppendText(ew.Message + "\n\r");
            }
        } //обработчик исключений
        public void EIGnc(Action action)
        {
            try
            {
                action.Invoke();
            }
            catch (Exception ew)
            {
            }
        } //тихий обработчик    
        private void button13_Click(object sender, EventArgs e) //(тест)поиск по слову
        {
            try
            {
                var w = relabase.findByWord(textBox1.Text);
                Stopwatch sw = (Stopwatch)w[1];
                dataGridView1.DataSource = (DataTable)w[0];
                label_row_count.Text = "Всего строк: " + ((DataTable)w[0]).Rows.Count.ToString();
                label_time_elapsed.Text = "s:" + sw.Elapsed.Seconds + ":ms" + sw.Elapsed.Milliseconds / 10;
                StringBuilder s = new StringBuilder();
                s.Append(relabase.getIndexes()
                + ". Время работы: "
                + sw.Elapsed.ToString()
                + ". Поиск по слову. "
                + "Количество строк: "
                + ((DataTable)w[0]).Rows.Count.ToString()
                + '\n'
                );
                richTextBox1.AppendText(s.ToString());
            }
            catch (Exception ew)
            {
                richTextBox1.AppendText(ew.Message + '\n');
            }
        }
        private void button11_Click(object sender, EventArgs e) //добавление GIN индекса
        {
            EIG(() => relabase.addGinIndex());
        }
        private void button14_Click(object sender, EventArgs e) //обновление GINa
        {
            EIG(() => relabase.updateGinIndex());
        }

        private void button12_Click(object sender, EventArgs e)
        {
            richTextBox1.AppendText(relabase.getIndexes() + '\n');
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }

    public class Relabase
    {
        static String dbSettings = "Server=localhost;Port=5432;User Id=postgres;Password=1234;Database=dozvla_csharp;";
        NpgsqlConnection dbConnection;
        Random r = new Random();
        Timer t = new Timer();
        List<Record> records = new List<Record>();
        myRand mr = new myRand();
        bool hashEnabled = false;
        bool bTreeEnabled = false;
        bool GinEnabled = false;

        public Relabase()
        {
            dbConnection = new NpgsqlConnection(dbSettings);
        }
        ~Relabase()
        {
            dbConnection.Close();
        }
        public Relabase connect()
        {
            dbConnection.Open();
            NpgsqlCommand np = new NpgsqlCommand("SET default_text_search_config = russian;");
            np.Connection = dbConnection;
            np.ExecuteNonQuery();
            return this;
        }
        public String getIndexes()
        {
            StringBuilder stringBuilder = new StringBuilder();
            if (hashEnabled) stringBuilder.Append("HASH ");
            if (bTreeEnabled) stringBuilder.Append("bTREE ");
            if (GinEnabled) stringBuilder.Append("GIN ");

            if (stringBuilder.Length == 0) return "Индексы не установлены.";
            else return "Установлены индексы: " + stringBuilder;
        }
        public void clearTable()
        {
            NpgsqlCommand clearT = new NpgsqlCommand("DELETE FROM Таблица_1");
            clearT.Connection = dbConnection;
            clearT.ExecuteNonQuery();
        }
        public void getTable()
        {
            NpgsqlCommand getT = new NpgsqlCommand("SELECT * FROM Таблица_1");
            getT.Connection = dbConnection;
            NpgsqlDataReader t_reader = getT.ExecuteReader();
            while (t_reader.Read())
            {
                records.Add(new Record(
                t_reader.GetInt32(0),
                t_reader.GetInt32(1),
                t_reader.GetString(2)
                ));
            }
        }
        public DataTable getDatasource()
        {
            NpgsqlDataAdapter da = new NpgsqlDataAdapter("SELECT * FROM Таблица_1", dbConnection);
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();
            da.Fill(ds);
            dt = ds.Tables[0];
            return dt;
        }
        public void addBTreeIndex()
        {
            NpgsqlCommand np = new NpgsqlCommand(
                "CREATE INDEX \"bTree\" ON \"Таблица_1\" USING btree (\"Поле_1\");"
                );
            np.Connection = dbConnection;
            np.ExecuteNonQuery();
            bTreeEnabled = true;
        }
        public void addHashIndex()
        {
            NpgsqlCommand np = new NpgsqlCommand(
                "CREATE INDEX \"hashTree\" ON \"Таблица_1\" USING hash (\"Поле_1\");"
                );
            np.Connection = dbConnection;
            np.ExecuteNonQuery();
            hashEnabled = true;
        }
        public void dropHashIndex()
        {
            NpgsqlCommand np = new NpgsqlCommand("DROP INDEX \"hashTree\"");
            np.Connection = dbConnection;
            np.ExecuteNonQuery();
            hashEnabled = false;
        }
        public void dropBTreeIndex()
        {
            NpgsqlCommand np = new NpgsqlCommand("DROP INDEX \"bTree\"");
            np.Connection = dbConnection;
            np.ExecuteNonQuery();
            bTreeEnabled = false;
        }
        public void dropGinIndex()
        {
            NpgsqlCommand np = new NpgsqlCommand("DROP INDEX \"ginTree\"");
            np.Connection = dbConnection;
            np.ExecuteNonQuery();
            GinEnabled = false;
        }
        public ArrayList randSelect(int count)
        {
            ArrayList arrayList = new ArrayList();
            Stopwatch sw = new Stopwatch();
            sw.Start();
            NpgsqlDataAdapter da = new NpgsqlDataAdapter("SELECT * FROM Таблица_1 ORDER BY random() limit " + count, dbConnection);
            sw.Stop();
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();
            da.Fill(ds);
            dt = ds.Tables[0];
            arrayList.Add(dt);
            arrayList.Add(sw);
            return arrayList;
        }
        public ArrayList diapSelect(int count)
        {
            ArrayList arrayList = new ArrayList();
            Stopwatch sw = new Stopwatch();
            sw.Start();
            NpgsqlDataAdapter da = new NpgsqlDataAdapter("SELECT * FROM Таблица_1 LIMIT " + count + " OFFSET 50000", dbConnection);
            sw.Stop();
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();
            da.Fill(ds);
            dt = ds.Tables[0];
            arrayList.Add(dt);
            arrayList.Add(sw);
            return arrayList;
        }
        public Stopwatch generateWords(int inputCount)
        {
            int count = 0;
            Stopwatch sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < inputCount; i++)
            {
                String randText = mr.generate();
                int p = r.Next(1, 4);

                NpgsqlCommand fillCommand = new NpgsqlCommand("INSERT INTO Таблица_1 VALUES(DEFAULT, " + p + ", '" + randText + "');");
                fillCommand.Connection = dbConnection;
                count += fillCommand.ExecuteNonQuery();
            }
            sw.Stop();
            return sw;
        }
        public ArrayList findByWord(String word)
        {
            ArrayList arrayList = new ArrayList();
            Stopwatch sw = new Stopwatch();
            sw.Start();
            NpgsqlDataAdapter da = new NpgsqlDataAdapter("SELECT COUNT(*) FROM Таблица_1 WHERE Поле_4 @@ to_tsquery('" + word + "');", dbConnection);
            sw.Stop();
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();
            da.Fill(ds);
            dt = ds.Tables[0];
            arrayList.Add(dt);
            arrayList.Add(sw);
            return arrayList;
        }
        public void addGinIndex()
        {
            NpgsqlCommand np = new NpgsqlCommand(
                "CREATE INDEX \"ginTree\" ON \"Таблица_1\" USING gin (\"Поле_4\");");
            np.Connection = dbConnection;
            np.ExecuteNonQuery();
            GinEnabled = true;
        }
        public void updateGinIndex()
        {
            NpgsqlCommand np = new NpgsqlCommand(
                "UPDATE \"Таблица_1\" SET \"Поле_4\" = to_tsvector(\"Поле_3\") WHERE \"Поле_4\" IS NULL;");
            np.CommandTimeout = 1000;
            np.Connection = dbConnection;
            np.ExecuteNonQuery();
        }
    }
    class myRand
    {
        Random rnd = new Random();
        String text = "some text here is going on my mind with no empty space throw the glasses";

        public String generate()
        {
            var splitted = text.Split();
            List<int> o = new List<int>();
            String total = "";

            for (int i = 0; i < splitted.Length / 3; i++)
            {
                int temp = rnd.Next(0, splitted.Length);
                if (o.Contains(temp))
                {
                    i--;
                }
                else
                {
                    o.Add(temp);
                    total += splitted[temp] + ' ';
                }
            }
            return total;
        }
    }
    struct Record
    {
        int column1;
        int column2;
        String column3;
        public Record(int a, int b, String c)
        {
            column1 = a;
            column2 = b;
            column3 = c;
        }
    }
}

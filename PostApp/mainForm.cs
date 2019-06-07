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

namespace PostApp
{
    public partial class mainForm : Form
    {

        Relabase relabase;
        public mainForm()
        {
            InitializeComponent();
            relabase = new Relabase();
        }

        private void button1_Click(object sender, EventArgs e) //соединение
        {
            EIG(() => relabase.connect());
            label_display_info.Text = "Соединение установлено";
            pictureBox1.BackColor = Color.Green;
        }
        private void button2_Click(object sender, EventArgs e) //заполнение
        {
            EIG(() => label_display_info.Text = "Добавлено: " + relabase.fillTable(100000) + " строк");
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
            EIG(() => relabase.dropHashIndex());
            EIG(() => relabase.addBTreeIndex());
        }
        private void button6_Click(object sender, EventArgs e) //создание hash индекса
        {
            EIG(() => relabase.dropBTreeIndex());
            EIG(() => relabase.addHashIndex());
        }
        private void trackBar1_ValueChanged(object sender, EventArgs e)
        {
            label_trackbar_value.Text = trackBar1.Value.ToString();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            EIG(() => new Relabase().connect().dropHashIndex());
        }
        private void button9_Click(object sender, EventArgs e) //(тест) случайное заполнение
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            EIG(() => relabase.fillTable(trackBar1.Value));
            sw.Stop();
            label_time_elapsed.Text = "s:" + sw.Elapsed.Seconds + ":ms" + sw.Elapsed.Milliseconds / 10;
        }
        private void button10_Click(object sender, EventArgs e) //убрать все индексы
        {
            EIG(() => relabase.dropBTreeIndex());
            EIG(() => relabase.dropGinIndex());
            EIG(() => relabase.dropHashIndex());
        }
        private void button8_Click(object sender, EventArgs e) //(тест) случайная выборка
        {
            try
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                var w = relabase.randSelect(trackBar1.Value);
                dataGridView1.DataSource = w;
                label_row_count.Text = "Всего строк: " + w.Rows.Count.ToString();
                sw.Stop();
                label_time_elapsed.Text = "s:" + sw.Elapsed.Seconds + ":ms" + sw.Elapsed.Milliseconds / 10;
                richTextBox1.AppendText(sw.Elapsed.ToString() + '\n');
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
                Stopwatch sw = new Stopwatch();
                sw.Start();
                var w = relabase.diapSelect(trackBar1.Value);
                dataGridView1.DataSource = w;
                label_row_count.Text = "Всего строк: " + w.Rows.Count.ToString();
                sw.Stop();
                label_time_elapsed.Text = "s:" + sw.Elapsed.Seconds + ":ms" + sw.Elapsed.Milliseconds / 10;
                richTextBox1.AppendText(sw.Elapsed.ToString() + '\n');
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
                richTextBox1.AppendText(ew.Message + '\n');
            }
        } //обработчик исключений

        private void button12_Click(object sender, EventArgs e)
        {
            EIG(() => label_display_info.Text = "Добавлено: " + relabase.generateWords(100000) + " строк");
        }

        private void button13_Click(object sender, EventArgs e)
        {

        }
    }

    public class Relabase
    {
        static String dbSettings = "Server=localhost;Port=5432;User Id=postgres;Password=1234;Database=task2;";
        NpgsqlConnection dbConnection;
        Random r = new Random();
        Timer t = new Timer();
        List<Record> records = new List<Record>();
        myRand mr = new myRand();

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
            return this;
        }
        public int fillTable(int inputCount)
        {
            int count = 0;
            for (int i = 0; i < inputCount; i++)
            {
                String ticks = DateTime.Now.ToString();
                int p = r.Next(1, 4);

                NpgsqlCommand fillCommand = new NpgsqlCommand("INSERT INTO Таблица_1 VALUES(" + p + ", '" + ticks + "');");
                fillCommand.Connection = dbConnection;
                count += fillCommand.ExecuteNonQuery();
            }
            return count;
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
        }
        public void addHashIndex()
        {
            NpgsqlCommand np = new NpgsqlCommand(
                "CREATE INDEX \"hashTree\" ON \"Таблица_1\" USING hash (\"Поле_1\");"
                );
            np.Connection = dbConnection;
            np.ExecuteNonQuery();
        }
        public void dropHashIndex()
        {
            NpgsqlCommand np = new NpgsqlCommand("DROP INDEX \"bTree\"");
            np.Connection = dbConnection;
            np.ExecuteNonQuery();


        }
        public void dropBTreeIndex()
        {
            NpgsqlCommand np = new NpgsqlCommand("DROP INDEX \"hashTree\"");
            np.Connection = dbConnection;
            np.ExecuteNonQuery();
        }
        public void dropGinIndex()
        {
            NpgsqlCommand np = new NpgsqlCommand("DROP INDEX \"ginTree\"");
            np.Connection = dbConnection;
            np.ExecuteNonQuery();
        }
        public DataTable randSelect(int count)
        {
            NpgsqlDataAdapter da = new NpgsqlDataAdapter("SELECT * FROM Таблица_1 ORDER BY random() limit " + count, dbConnection);
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();
            da.Fill(ds);
            dt = ds.Tables[0];
            return dt;
        }
        public DataTable diapSelect(int count)
        {
            NpgsqlDataAdapter da = new NpgsqlDataAdapter("SELECT * FROM Таблица_1 LIMIT " + count + " OFFSET 1000; " + count, dbConnection);
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();
            da.Fill(ds);
            dt = ds.Tables[0];
            return dt;
        }
        public int generateWords(int inputCount)
        {
            int count = 0;
            for (int i = 0; i < inputCount; i++)
            {
                String randText = mr.generate();
                int p = r.Next(1, 4);

                NpgsqlCommand fillCommand = new NpgsqlCommand(
                    "INSERT INTO Таблица_1 VALUES(" + p + ", '" + randText + "');");
                fillCommand.Connection = dbConnection;
                count += fillCommand.ExecuteNonQuery();
            }
            return count;
        }
        public DataTable findByWord()
        {
            NpgsqlDataAdapter da = new NpgsqlDataAdapter("SELECT * FROM Таблица_1", dbConnection);
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();
            da.Fill(ds);
            dt = ds.Tables[0];
            return dt;
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

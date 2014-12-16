using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO.Ports;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;


namespace LED_Matrix
{

    
    
    public partial class Form1 : Form
    {

       
        byte[] dane = new byte[9];
        Int16[,] bufor = new Int16[8, 8];
        Int16[,] bufor2 = new Int16[30, 8];
        int testerino = 0, gg=1; //test        
           
        
        public Form1()
        {
            InitializeComponent();//chuj

            foreach (string port in SerialPort.GetPortNames())
            {
                comboBox1.Items.Add(port);
            }
            comboBox1.DataSource = SerialPort.GetPortNames();
            button4.Enabled = false;

           
            
            dataGridView1.RowCount = 8;
            dataGridView1.ColumnCount = 8;

        }

        private void openport_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedValue != null)
            {
                serialPort1.PortName = comboBox1.SelectedValue.ToString();
                serialPort1.Open();
                if (serialPort1.IsOpen)
                {
                    toolStripStatusLabel1.Text = "Status: Connected to " + serialPort1.PortName.ToString();
                    button1.Enabled = false;
                    button4.Enabled = true;
                }
                else toolStripStatusLabel1.Text = "Status: Connection Error";
            }
            else toolStripStatusLabel1.Text = "Status: COM port not selected!!!";

        }
        private void animacja()
        {

            Random rnd = new Random();

            for (int j = 0; j < 8; j++)
            {
                for (int i = 0; i < 8; i++)
                {
                    bufor[i,j] = (Int16)rnd.Next(2);
                }
                
            }
            send_to_matrix(bufor);
            datagrid_color_update(bufor);
        }
        
        
        private void send_to_matrix(Int16[,] tab)
        {
            int row_data = 0;


            if (serialPort1.IsOpen)
            {
                for (int j = 0; j<tab.GetLength(1); j++)
                {
                    for (int i = 0; i<tab.GetLength(0); i++)
                    {
                        row_data += tab[7-i,j] * (int)Math.Pow(2, i); //rev line
                    }
                    dane[j] = Convert.ToByte(row_data);
                    row_data = 0;
                }
                serialPort1.Write(dane, 0, 9);

            }
            else toolStripStatusLabel1.Text = "Status: Connection Error (port not opened)";

        }
      
        public Int16[,] char_to_matrix(char sign)
        {
          
           foreach (Matrix_sign ms in matrix_list)
           {
               if (ms.sign == sign) return ms.msign;
           }
           return null;
            
        }
       
        
        
        private void datagrid_color_update(Int16[,] tab)
        {
            var cell = dataGridView1.CurrentCell; 
            
            for (int j = 0; j < tab.GetLength(1); j++)
            {
                for (int i = 0; i < tab.GetLength(0); i++)
                {
                    cell = dataGridView1.Rows[j].Cells[i];

                    int kolor = 255;//trackBar1.Value*15;
                    switch (tab[i,j]) 
                    {
                        case 1: cell.Style.BackColor = System.Drawing.Color.FromArgb(kolor, 0, 0);
                                cell.Style.SelectionBackColor = System.Drawing.Color.FromArgb(kolor, 0, 0);
                            break;
                        case 0: cell.Style.BackColor = System.Drawing.Color.LightBlue;
                                cell.Style.SelectionBackColor = System.Drawing.Color.LightBlue;
                            break;
                            
                    }
             
                }

            }
        }
        
        private void button4_Click(object sender, EventArgs e)
        {

            serialPort1.Close();
            toolStripStatusLabel1.Text = "Status: Disconnected";
            button4.Enabled = false;
            button1.Enabled = true;

        }
        private void trackBar1_Scroll(object sender, EventArgs e)
        {

            if (serialPort1.IsOpen)
            {
                dane[8] = Convert.ToByte(trackBar1.Value);
                serialPort1.Write(dane, 0, 9);
                label1.Text = "Brightness: " + (trackBar1.Value + 1).ToString() + "/16";
            }
            else toolStripStatusLabel1.Text = "Status: Connection Error!!! (try to open port)";
        }
        private void Form1_Closing(object sender, CancelEventArgs e)
        {
            serialPort1.Close();
        }
        private void Clear_Click(object sender, EventArgs e)
        {
            bufor_fill(0);
            send_to_matrix(bufor);
            datagrid_color_update(bufor);
        }      
        private void bufor_fill(Int16 x)
        {
            for (int i = 0; i < bufor.GetLength(1); i++)
            {
                for (int j = 0; j < bufor.GetLength(0); j++)
                {
                    bufor[j, i] = x;
                }
            }

        }
        private void fill_Click(object sender, EventArgs e)
        {
           bufor_fill(1);
           send_to_matrix(bufor);
           datagrid_color_update(bufor);   
        }
        private void button2_Click(object sender, EventArgs e)
        {
            animacja();
        }
        private void button5_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();       
        }
        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            System.IO.Stream stream = openFileDialog1.OpenFile();
            System.IO.StreamReader reader = new System.IO.StreamReader(stream);

            string file = reader.ReadToEnd();
            richTextBox1.Text = file;
           
            stream.Close();
        }

        static CancellationTokenSource cts = new CancellationTokenSource();
        static CancellationToken ct = cts.Token;
        static bool isCancel = false;

        private void button6_Click(object sender, EventArgs e)
        {


            for (int i = 0; i < 5; i++)
            {
                msign_to_buff(char_to_matrix((char)richTextBox1.Text.ElementAtOrDefault(i)), i * 6);
            }    

         // wyswietl(bufor2);
            Task.Factory.StartNew(()=>{LED_shift(21);});
               
        }
        private void wyswietl(Int16[,] tab)
        {
            richTextBox1.Clear();
            for (int i = 0; i < tab.GetLength(1); i++)
            {
                for (int j = 0; j < tab.GetLength(0); j++)
                {
                    richTextBox1.Text += tab[j, i]+" ";

                }
                richTextBox1.Text += "\n";
            }
            richTextBox1.Text += "\n";
        }
       
        private void msign_to_buff(Int16[,] tab, int pos )
        {
            
            for (int i = 0; i < tab.GetLength(1); i++)
            {
                for(int j = 0; j < tab.GetLength(0); j++)
                {
                    bufor2[i+pos, j+1] = tab[j,i]; //linia w dół
                }
            }
        }
        
        public void LED_shift(int buff_length)
        {
            for (int i = 0; i <= buff_length; i++ )
            {
                Array.Copy(bufor2, i * 8, bufor, 0, 64);
                datagrid_color_update(bufor);
                send_to_matrix(bufor);
                System.Threading.Thread.Sleep(200);
               if(isCancel == true)
               {
                   break;
               }
            }
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
           
            Array.Copy(bufor2, testerino * 8, bufor, 0, 64);
            datagrid_color_update(bufor);
            send_to_matrix(bufor);
            if (testerino == 21) gg = -1;
            if (testerino == 0) gg = 1;
                testerino += gg;

           
         


        }
        private void timerstart_Click(object sender, EventArgs e)
        {
            timer1.Enabled = true;
            timer1.Start();
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            Point adres = dataGridView1.CurrentCellAddress;

            if (bufor[adres.X, adres.Y] == 0) bufor[adres.X, adres.Y] = 1;
            else bufor[adres.X, adres.Y] = 0;

            datagrid_color_update(bufor);
            send_to_matrix(bufor);

        }

        private void button8_Click(object sender, EventArgs e)
        {
           
            isCancel = true;
            Thread.Sleep(300);
            isCancel = false;
        }

        

       
    }
}

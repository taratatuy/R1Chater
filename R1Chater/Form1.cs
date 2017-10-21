using System;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace R1Chater
{
    public partial class Form1 : Form
    {
        TcpClient tcpClient;
        StreamWriter writer;
        bool breakPoint = false;
        
        public Form1()
        {
            InitializeComponent();

            INIManager ini = new INIManager(Application.StartupPath + "/Confg.txt");

            if (File.Exists(Application.StartupPath + "/Confg.txt"))
            {
                textBox1.Text = ini.GetPrivateString("1", "Name");
                textBox2.Text = ini.GetPrivateString("1", "Token");
            }
        }

        private async Task<bool> Connecting()
        {
            try
            {
                tcpClient = new TcpClient("irc.chat.twitch.tv", 6667);
            }
            catch
            {
                MessageBox.Show("No connection");
                return false;
            }


            writer = new StreamWriter(tcpClient.GetStream());            

            int i = 0;
            if (this.textBox1.Text == "")
            {
                MessageBox.Show("Input Name");
                return false;
            }
            string nickName = Convert.ToString(this.textBox1.Text).ToLower();

            if (this.textBox2.Text == "")
            {
                MessageBox.Show("Input Token");
                return false;
            }
            string token = Convert.ToString(this.textBox2.Text);

            if (this.textBox3.Text == "")
            {
                MessageBox.Show("Input Channel");
                return false;
            }
            string channel = Convert.ToString(this.textBox3.Text).ToLower();
            string userMessage = Convert.ToString(this.textBox4.Text);
            string message;

            if (this.textBox5.Text == "")
            {
                MessageBox.Show("Input Count");
                return false;
            }
            int count = Convert.ToInt32(this.textBox5.Text);

            if (this.textBox6.Text == "")
            {
                MessageBox.Show("Input Pause");
                return false;
            }
            double pause = Convert.ToDouble(this.textBox6.Text) * 1000;
            string messForHost = $":{nickName}!{nickName}@{nickName}.tmi.twitch.tv PRIVMSG #{channel} :";
            bool datePaste = false;

            if (this.checkBox1.Checked)
                datePaste = true;

            if (pause < 3000)
            {
                pause = 3000;
                this.textBox6.Text = "3";
            }

            writer.WriteLine(
                "PASS " + token + Environment.NewLine
                + "NICK " + nickName + Environment.NewLine
                + "JOIN #" + channel
                );
            writer.Flush();

            this.progressBar1.Minimum = 0;
            this.progressBar1.Maximum = count;
            this.progressBar1.Step = 1;
            this.progressBar1.Value = 0;
            this.label9.Text = "";
            this.button1.Enabled = false;

            while ( i < count)
            {
                if (breakPoint)
                    break;

                message = $"{userMessage}  Count: {i + 1}   ";
                if (datePaste)
                    message += "Time: " + DateTime.Now.ToLongTimeString();
                
                await SendMessToHostAsync(writer, messForHost, message);

                this.progressBar1.PerformStep();
                this.label9.Text = $"({i + 1} / {count})";
                i++;
            }
            this.button1.Enabled = true;
            return true;
        }

        private Task SendMessToHostAsync( StreamWriter writer, string messForHot, string message)
        {
            return Task.Run(() =>
            {
                SendMessToHost(ref writer, messForHot, message);
            });
        }

        private void SendMessToHost(ref StreamWriter writer, string messForHot, string message)
        {
            double pause = Convert.ToDouble(this.textBox6.Text) * 1000;
            Thread.Sleep(Convert.ToInt32(pause));
            writer.WriteLine(messForHot + message);
            writer.Flush();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            breakPoint = false;
            Connecting();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            breakPoint = true;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            File.Create(Application.StartupPath + "/Confg.txt").Close();
            INIManager ini = new INIManager(Application.StartupPath + "/Confg.txt");
            ini.WritePrivateString("1", "Name", textBox1.Text);
            ini.WritePrivateString("1", "Token", textBox2.Text);
            MessageBox.Show("Saved");
        }
    }
}

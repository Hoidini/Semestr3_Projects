using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Analise
{
    public partial class MessageForm : Form
    {
        public MessageForm(string statistic)
        {
            InitializeComponent();
            StatisticText.Text = statistic;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            string filepath = default;
            SaveFileDialog saveFile = new SaveFileDialog();
            saveFile.Filter = "txt files (*.txt)|*.txt";
            saveFile.InitialDirectory = "D:\\";
            saveFile.RestoreDirectory = false;
            if(saveFile.ShowDialog() == DialogResult.OK)
            {
                filepath = saveFile.FileName;
                var FileStream = saveFile.OpenFile();
                using (StreamWriter writer = new StreamWriter(FileStream))
                {
                    writer.Write(StatisticText.Text);
                }
            }
            this.Close();
        }
    }
}

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
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button2_MouseClick(object sender, MouseEventArgs e)
        {
            string filePath = default;
            openFileDialog1.InitialDirectory = "D:\\";
            openFileDialog1.Filter = "txt files (*.txt)|*.txt";
            openFileDialog1.RestoreDirectory = false;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                filePath = openFileDialog1.FileName;
                var fileStream = openFileDialog1.OpenFile();
                using (StreamReader reader = new StreamReader(fileStream))
                {
                    textBox1.Text = reader.ReadToEnd();
                }
            }
        }
        void Check_and_Add<T>(Dictionary<T, int> dict, T info)
        {
            try
            {
                if (dict.TryGetValue(info, out _))
                {
                    dict[info]++;
                }
                else
                {
                    dict.Add(info, 1);
                }
            }
            catch (Exception e)
            {
                throw;
            }
        }
        void Get_Statistic(TextBox textBox, Dictionary<string, int> Dict_Of_Words,
            Dictionary<char, int> Dict_Of_Symbols, ref int count_of_symbols,
          ref int count_of_words)
        {
            try
            {
                string text_info = textBox.Text.ToLower() + "\n";
                if (text_info.Length == 1)
                {
                    throw new ArgumentException("TextBox is empty");
                }
                bool IsWord = false;
                StringBuilder word = new StringBuilder();
                foreach (char i_symbol in text_info)
                {
                    if ((i_symbol >= 'a' && i_symbol <= 'z') || (i_symbol >= 'а' &&
                        i_symbol <= 'я'))
                    {
                        count_of_symbols++;
                        Check_and_Add<char>(Dict_Of_Symbols, i_symbol);
                        word.Append(i_symbol);
                        IsWord = true;
                    }
                    else
                    {
                        if (IsWord)
                        {
                            count_of_words++;
                            string St_Word = word.ToString();
                            Check_and_Add<string>(Dict_Of_Words, St_Word);
                            IsWord = false;
                            word.Clear();
                        }
                    }
                }
            }
            catch (Exception excep)
            {
                throw;
            }
        }
        class Compare_Words_By_Count : IComparer <(string, int)>
        {
           int IComparer<(string, int)>.Compare((string, int) x, (string, int) y)
            {
                int result = 1;
                if(x.Item2 > y.Item2)
                {
                    result = -1;
                }
                return result;
            }
        }
        class Compare_Words_Bu_Length : IComparer<(string, int)>
        {
            int IComparer<(string, int)>.Compare((string, int) x, (string, int) y)
            {
                int result = 1;
                if (x.Item1.Length > y.Item1.Length)
                {
                    result = -1;
                }
                return result;
            }
        }
        string Analize(Dictionary<string, int> Dict_Of_Words, int count_of_symbols,
            Dictionary<char, int> Dict_Of_Symbols, int count_of_words)
        {
            try
            {
                StringBuilder result = new StringBuilder(100);
                result.Append("Количество букв: " + count_of_symbols +
                    ", количество слов: " + count_of_words + ", количество" +
                    " уникальных слов: " + Dict_Of_Words.Count);
                {
                    Compare_Words_By_Count compar_count = new Compare_Words_By_Count();
                    SortedSet<(string, int)> set_words_sort_count = new SortedSet<(string, int)>(compar_count);
                    foreach (var data in Dict_Of_Words)
                    {
                        (string, int) data_pack = (data.Key, data.Value);
                        set_words_sort_count.Add(data_pack);
                    }
                    {
                        int i = 0;
                        result.Append("\n Список самых частых слов:\n");
                        foreach ((string, int) data in set_words_sort_count)
                        {
                            result.Append(data.Item1 + " встречается " + data.Item2 + " раз\n");
                            if (++i >= 10)
                            {
                                break;
                            }
                        }
                    }
                }
                {
                    Compare_Words_Bu_Length compare_length = new Compare_Words_Bu_Length();
                    SortedSet<(string, int)> set_words_sort_length = new SortedSet<(string, int)>(compare_length);
                    {
                        foreach (var data in Dict_Of_Words)
                        {
                            (string, int) data_pack = (data.Key, data.Value);
                            set_words_sort_length.Add(data_pack);
                        }
                        int i = 0;
                        result.Append("\n Список самых длинных слов\n");
                        foreach ((string, int) data in set_words_sort_length)
                        {
                            result.Append(data.Item1 + " длина слова - " + data.Item1.Length + "\n");
                            if (++i >= 10)
                            {
                                break;
                            }
                        }
                    }
                }
                result.Append("\n Доля каждой буквы от общего количества в %: \n");
                foreach (var data in Dict_Of_Symbols)
                {
                    float part_procent = ((float)data.Value / (float)count_of_symbols * 100);
                    result.Append(data.Key + " - " + part_procent +
                        " %\n");
                }
                return result.ToString();
            } catch (Exception e)
            {
                throw;
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {

            try
            {
                int count_of_symbols = 0;
                int count_of_words = 0;
                Dictionary<string, int> Dict_Of_Words = new Dictionary<string, int>();
                Dictionary<char, int> Dict_Of_Symbols = new Dictionary<char, int>();
                Get_Statistic(textBox1, Dict_Of_Words, Dict_Of_Symbols,
                    ref count_of_symbols, ref count_of_words);
                string statistic = Analize(Dict_Of_Words, count_of_symbols, Dict_Of_Symbols, count_of_words);
                MessageForm message = new MessageForm(statistic);
                message.ShowDialog();

            }
            catch (ArgumentException excep)
            {
                MessageBox.Show(excep.Message);
            }
            catch (SystemException excep)
            {
                MessageBox.Show(excep.Message + "\n Critical error," +
                    " Application is stop");
                Application.Exit();
            }
            catch (Exception excep)
            {
                MessageBox.Show("Unexpexted exception: " + excep.Message);
            }

        }
    }
}

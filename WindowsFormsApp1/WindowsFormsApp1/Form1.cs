using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        public static Point Size_Of_Window_Global;
        //класс делегата для передачи управления формой интерфейсам
        public delegate void Send_Active_Interface(Active_Interface active, params Control[] controls);
        public delegate void Add_Controls(params Control[] controls);
        public delegate void Controls_Deleter(params Control[] controls);
        readonly Graphics graphics;
        readonly Send_Active_Interface Sender_Of_Interface;
        readonly Controls_Deleter controls_Deleter;
        readonly Add_Controls AddControls;
        public Form1() //инициализация стартого меню
        {
            InitializeComponent();
            {
                int y = this.Size.Height;
                int x = this.Size.Width;
                Size_Of_Window_Global = new Point(x, y);
            }
            this.graphics = this.panel1.CreateGraphics();
            this.panel1.BackColor = Color.White;
            Sender_Of_Interface += this.Set_Active_Interface;
            controls_Deleter += this.Delete_Controls;
            AddControls += AddConntrolsToForm;
            this.DoubleBuffered = true;
            //создание главного меню и передача управления формой
            Main_Interface main_Interface = new Main_Interface(Size_Of_Window_Global,
                this.Sender_Of_Interface, this.controls_Deleter, this.AddControls);
            main_Interface.GetControlOfForm();
        }
        static class Loc_Res
        { //вспомогательный класс для задания положения и размера объектов
            public static Point Construct(double prop_x, double prop_y, Point value)
            {
                Point Result = new Point((int)(value.X * prop_x),
                            (int)(value.Y * prop_y));
                return Result;
            }
            public static void Choose_Loc_And_Size(
                 Control Obj,
                double prop_xL, double prop_xS, double prop_yL, double prop_yS, Point value)
            {
                Obj.Location = Loc_Res.Construct(prop_xL, prop_yL, value);
                Obj.Size = (System.Drawing.Size)Loc_Res.Construct(prop_xS, prop_yS, value);
            }
        }
        public void Load_Active_Interface(Active_Interface active_Interface)
        {
            this.active_Interface = active_Interface;
        }
        //метод, осуществляющий передачу управления формой через делегат
        public void Set_Active_Interface(Active_Interface active,
            params Control[] controls)
        {
            this.panel1.BringToFront();
            
            foreach(Control cont in Controls)
            {
                if(cont != this.panel1)
                {
                    this.Controls.Remove(cont);
                }
            }
            this.graphics.Clear(Color.White);
            this.active_Interface = active;
            this.AddConntrolsToForm(controls);
            
        }
        public void AddConntrolsToForm(params Control[] controls)
        {
            foreach (var cont in controls)
            {
                this.Controls.Add(cont);
                cont.BringToFront();
            }  
            this.graphics.Clear(Color.White);
        }
        public void Delete_Controls(params Control[] controls)
        {

            for (int i = 0; i < controls.Length; i++)
            {
                Control cont = controls[i];
                if (this.Controls.Contains(cont))
                {
                    this.Controls.Remove(cont);
                }
            }
            this.graphics.Clear(Color.White);
        }


    }
}

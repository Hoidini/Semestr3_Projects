using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TowersHan
{
    public partial class Form1 : Form
    {
        ActiveMenu activeMenu;
        public Form1()
        {
            InitializeComponent();
        Action<Control[]> AddContols = this.AddControls;
        Action<Control[]> DeleteControls = this.DeleteControls;
        Action<ActiveMenu, Control[]> InitMenu = this.InitMenu;
            activeMenu = new StartMenu(AddContols, DeleteControls, InitMenu);
            this.activeMenu.GetControlOfMenu();
        }
        void AddControls(params Control[] controls)
        {
            foreach(Control control in controls)
            {
                this.Controls.Add(control);
            }
        }
        void DeleteControls(params Control[] controls)
        {
            foreach(Control control in controls)
            {
                this.Controls.Remove(control);
                control.Dispose();
            }
        }
        void InitMenu(ActiveMenu menu, params Control[] controls)
        {
            this.activeMenu = menu;
            this.Controls.Clear();
            this.AddControls(controls);
        }
    }
    abstract class ActiveMenu
    {
        protected Action<Control[]> AddContols;
        protected Action<Control[]> DeleteControls;
        protected Action<ActiveMenu, Control[]> InitMenu;
        protected ActiveMenu(Action<Control[]> AddContols, Action<Control[]> DeleteControls,
            Action<ActiveMenu, Control[]> InitMenu)
        {
            this.AddContols = AddContols;
            this.DeleteControls = DeleteControls;
            this.InitMenu = InitMenu;
        }
        public abstract void GetControlOfMenu(); 
    }
    abstract class GameObject
    {
        protected Point[] points;
        public GameObject(Action<GameObject> AddToPhys)
        {
            // по возрастанию: левый верхний, левый нижний, правый верхний, правый нижний, центр
            this.points = new Point[5];
            AddToPhys(this);
        }
    }
    abstract class User
    {
        public Func<Point, bool> TryCatchDisk;
        protected Point? CurMousePos;
        public Point? CurrentMousePosition
        {
            get => this.CurMousePos;
        }
    }

}

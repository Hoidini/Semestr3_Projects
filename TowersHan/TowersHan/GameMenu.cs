using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TowersHan
{
    class GameMenu : ActiveMenu
    {
        readonly Panel panel;
        readonly Button BotButton;
        readonly Graphics graphics;
        readonly BufferedGraphics buffer;
        readonly BufferedGraphicsContext context;
        readonly Timer timer;
        readonly GameController controller;
        public GameMenu(Action<Control[]> AddContols, Action<Control[]> DeleteControls,
            Action<ActiveMenu, Control[]> InitMenu, int countOfDisks) : base(AddContols, DeleteControls, InitMenu)
        {
            this.BotButton = new Button
            {
                Location = new Point(1000, 200),
                Size = new Size(200, 100),
                ForeColor = Color.Red,
                Text = "Решить"
            };
            this.BotButton.Enabled = true;
            this.panel = new Panel();
            this.panel.Size = new Size(900, 700);
            this.context = BufferedGraphicsManager.Current;
            this.buffer = context.Allocate(this.panel.CreateGraphics(), this.panel.DisplayRectangle);
            this.graphics = buffer.Graphics;
            this.timer = new Timer();

            this.BotButton.BringToFront();
            this.controller = new GameController(new Player(this.panel), new Bot(), countOfDisks);
            this.timer.Tick += delegate (object sender, EventArgs e)
            {
                if (this.controller.DoStep())
                {
                    StartMenu menu = new StartMenu(AddContols, DeleteControls, InitMenu);
                    menu.GetControlOfMenu();
                    this.timer.Enabled = false;
                }

                this.DrawPicture();
            };
            this.BotButton.Click += delegate (object sender, EventArgs e)
            {
                this.controller.bot.StartAnimation();
            };
            timer.Interval = 30;
            this.timer.Enabled = true;
        }
        public void DrawPicture()
        {
            this.graphics.Clear(Color.Gray);
            List<Tower> towers = this.controller.GetTowers;
            List<Disk> disks = this.controller.GetDisks;
            Size TowerSize = new Size(Tower.radius * 2, Tower.high);
            foreach (Tower tower in towers)
            {
                Rectangle rectangle = new Rectangle(tower.LOCATION, TowerSize);
                this.graphics.FillRectangle(Brushes.Black, rectangle);
                this.graphics.DrawRectangle(Pens.Black, rectangle);
            }
            foreach (Disk disk in disks)
            {
                Size size = new Size(disk.radius * 2, disk.high);
                Rectangle rectangle = new Rectangle(disk.LOCATION, size);
                this.graphics.FillRectangle(Brushes.Green, rectangle);
                this.graphics.DrawRectangle(Pens.Black, rectangle);
            }
            this.buffer.Render();
        }
        public override void GetControlOfMenu()
        {
            base.InitMenu(this, new Control[] {this.BotButton, this.panel });
        }
    }
}

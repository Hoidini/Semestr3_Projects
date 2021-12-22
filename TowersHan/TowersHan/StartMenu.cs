using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TowersHan
{
    class StartMenu : ActiveMenu
    {
        Label HanTowersText;
        Button StartGameButton;
        ListBox CountOfDisks;
        Label InfoToUserAboutCountOfDisks;
        public StartMenu(Action<Control[]> AddContols, Action<Control[]> DeleteControls,
            Action<ActiveMenu, Control[]> InitMenu) : base(AddContols,DeleteControls, InitMenu) 
        {
            this.HanTowersText = new Label()
            {
                Text = "Ханойские башни",
                Location = new System.Drawing.Point(535, 100),
                Font = new System.Drawing.Font("Times New Roman", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204))),
                Size = new System.Drawing.Size(300,100)
            };
            this.StartGameButton = new Button()
            {
                Text = "Start",
                Location = new System.Drawing.Point(525, 450),
                Size = new System.Drawing.Size(250, 150)
            };
            this.CountOfDisks = new ListBox()
            {
                Location = new System.Drawing.Point(550, 300),
                Size = new System.Drawing.Size(200, 110)
            };
            this.InfoToUserAboutCountOfDisks = new Label()
            {
                Text = "Выберите количество колец - от 3 до 10",
                Location = new System.Drawing.Point(550, 250),
                Size = new System.Drawing.Size(200, 50)
            };
            for(int i = 3; i <= 10; i++)
            {
                CountOfDisks.Items.Add(i);
            }
            StartGameButton.MouseClick += delegate (object sender, MouseEventArgs e)
            {
                if(CountOfDisks.SelectedItem == null)
                {
                    return;
                }
                int count = (int)CountOfDisks.SelectedItem;
                ActiveMenu newMenu = new GameMenu(base.AddContols, base.DeleteControls, base.InitMenu,
                    count);
                base.DeleteControls(new Control[] { this.HanTowersText, this.InfoToUserAboutCountOfDisks,
            this.StartGameButton, this.CountOfDisks});
                newMenu.GetControlOfMenu();

            };
        }

        public override void GetControlOfMenu()
        {
            this.InitMenu(this, new Control[] { this.HanTowersText, this.InfoToUserAboutCountOfDisks,
            this.StartGameButton, this.CountOfDisks});
        }
    }
}

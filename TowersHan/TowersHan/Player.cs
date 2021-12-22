using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TowersHan
{
    class Player : User
    {
        bool IsActive;
        Panel GamePanel;
        public bool ISACTIVE
        {
            get => this.IsActive;
            set => this.IsActive = value;
        }
        public Player(Panel panel)
        {
            IsActive = true;
            this.GamePanel = panel;
            this.CurMousePos = new Point();
            this.GamePanel.MouseDown += delegate (object sender, MouseEventArgs e)
             {
                 if (!IsActive)
                 {
                     return;
                 }
                 base.CurMousePos = e.Location;
                 base.TryCatchDisk(e.Location);
             };
            this.GamePanel.MouseMove += delegate (object sender, MouseEventArgs e)
            {
                if (base.CurMousePos.HasValue)
                {
                    base.CurMousePos = e.Location;
                }
            };
            this.GamePanel.MouseUp += delegate (object sender, MouseEventArgs e)
            {
                base.CurMousePos = null;
            };
        }
    }
}

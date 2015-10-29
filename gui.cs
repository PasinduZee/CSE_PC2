using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace tank_game
{
    public partial class Gui : Form
    {
        private SendCommand sc = SendCommand.getInstance();
       
        public Gui()
        {
            InitializeComponent();
        }

        private void AI_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space) { sc.Shoot(); }
            else if (e.KeyCode == Keys.Up) { sc.Up(); }
            else if (e.KeyCode == Keys.Down) { sc.Down(); }
            else if (e.KeyCode == Keys.Left) { sc.Left(); }
            else if (e.KeyCode == Keys.Right) { sc.Right(); }
            else if (e.KeyCode == Keys.J) { sc.Join(); }
        }
    }
}

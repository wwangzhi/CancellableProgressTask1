using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CancellableProgressTask
{
    public partial class ProgressView : Form
    {
        private readonly CancellationTokenSource _source;
        private readonly Action action;

        private  void act()
        {
            progressBar1.Value += 1;
        }

        public ProgressView(CancellationTokenSource source)
        {
            _source = source;
            action = act;
            InitializeComponent();
        }

        public void ProgressValueInc ()
        {
            if (InvokeRequired)
            {
                BeginInvoke(action);

            }
            
        }



        private void button1_Click(object sender, EventArgs e)
        {
            _source.Cancel();
        }
    }
}

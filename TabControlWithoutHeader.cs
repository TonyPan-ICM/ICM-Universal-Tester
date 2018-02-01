using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ICM_Universal_Tester
{
	public partial class TabControlWithoutHeader : TabControl
	{
		public TabControlWithoutHeader()
		{
			InitializeComponent();
		}

		protected override void WndProc(ref Message m)
		{
			if (m.Msg == 0x1328 && !DesignMode)
				m.Result = (IntPtr)1;
			else
				base.WndProc(ref m);
		}
	}
}

using System;
using System.Windows.Forms;

namespace Blogical.Shared.Adapters.Sftp.Management
{
    public partial class SelectBizTalkApplication : Form
    {
        public SelectBizTalkApplication()
        {
            InitializeComponent();
        }

        private void btnYes_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Yes;
            Close();
        }

        private void btnNo_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void SelectBizTalkApplication_Load(object sender, EventArgs e)
        {
            TopMost = true;
        }
    }
}
namespace CurePlease
{
    using EliteMMO.API;
    using System;
    using System.Windows.Forms;
    using static MainForm;

    public partial class ChatlogForm : Form
    {
        public ChatlogForm()
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            this.StartPosition = FormStartPosition.CenterScreen;

            InitializeComponent();

            if (PL != null)
            {
                characterNamed_label.Text = $"Chatlog for character: {PL.Player.Name}";

                UpdateLines();

                chatlog_box.SelectionStart = chatlog_box.Text.Length;
                chatlog_box.ScrollToCaret();
            }
            else
            {
                chatlogscan_timer.Enabled = false;
                MessageBox.Show("No character was selected as the power leveler, this can not be opened yet.");

            }
        }

        private void CloseChatLog_button_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void UpdateLines()
        {
            EliteAPI.ChatEntry cl;

            while ((cl = PL.Chat.GetNextChatLine()) != null)
            {
                chatlog_box.AppendText(cl.Text, cl.ChatColor);
                chatlog_box.AppendText(Environment.NewLine);
            }
        }

        private void Chatlogscan_timer_Tick(object sender, EventArgs e)
        {
            UpdateLines();
        }

        private void Chatlog_box_TextChanged(object sender, EventArgs e)
        {
            chatlog_box.SelectionStart = chatlog_box.Text.Length;
            chatlog_box.ScrollToCaret();
        }

        private void SendMessage_botton_Click(object sender, EventArgs e)
        {
            PL.ThirdParty.SendString(ChatLogMessage_textfield.Text);
            ChatLogMessage_textfield.Text = string.Empty;
        }
    }
}

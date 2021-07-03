using EliteMMO.API;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using static System.Windows.Forms.Control;

namespace CurePlease
{
    public class PartyUtils
    {
        public static List<Label> ListOfPartyLabels = new List<Label>();
        public static List<ProgressBar> ListOfPartyProgressBars = new List<ProgressBar>();
        public static List<Button> ListOfPartyButtons = new List<Button>();

        public static EliteAPI PL = MainForm.PL;

        public static void UpdatePartyControls(ControlCollection controlCollection)
        {
            BuildListOfPartyControls(controlCollection);

            // scanning alliance
            for (int i = 0; i < 18; i++)
            {
                UpdatePartyMember(i);
                UpdateHPProgressBar(i);
            }
        }

        private static void BuildListOfPartyControls(ControlCollection controlCollection)
        {
            // lets do a bit of caching
            if (!ListOfPartyLabels.Any())
            {
                var partyGroupBoxes = controlCollection.OfType<GroupBox>().Where(c => c.Text.Contains("Party"));
                ListOfPartyLabels = partyGroupBoxes.SelectMany(c => c.Controls.OfType<Label>()).ToList();
                ListOfPartyProgressBars = partyGroupBoxes.SelectMany(c => c.Controls.OfType<ProgressBar>()).ToList();
                ListOfPartyButtons = partyGroupBoxes.SelectMany(c => c.Controls.OfType<Button>()).ToList();
            }
        }

        private static bool UpdatePartyMember(int index)
        {
            Label player = ListOfPartyLabels.FirstOrDefault(c => c.Name == $"player{index}");
            ProgressBar playerHP = ListOfPartyProgressBars.FirstOrDefault(c => c.Name == $"player{index}HP");
            Button playerOptionsButton = ListOfPartyButtons.FirstOrDefault(c => c.Name == $"player{index}optionsButton");

            var doUpdate = PartyMemberUpdateMethod((byte)index);

            if (doUpdate)
            {
                var partyMember = PL.Party.GetPartyMember(index);
                player.Text = partyMember?.Name;
            }
            else
            {
                player.Text = "Inactive";
                playerHP.Value = 0;
            }

            player.Enabled = doUpdate;
            playerOptionsButton.Enabled = doUpdate;

            return doUpdate;
        }

        private static bool PartyMemberUpdateMethod(byte partyMemberId)
        {
            if (PL.Party.GetPartyMembers()[partyMemberId].Active >= 1)
            {
                return true;
            }

            return false;
        }

        private static void UpdateHPProgressBar(int index)
        {
            Label player = ListOfPartyLabels.FirstOrDefault(c => c.Name == $"player{index}");
            ProgressBar playerHP = ListOfPartyProgressBars.OfType<ProgressBar>().FirstOrDefault(c => c.Name == $"player{index}HP");

            if (player == null || playerHP == null || !player.Enabled)
            {
                return;
            }

            int CurrentHPP = PL.Party.GetPartyMember(index).CurrentHPP;

            playerHP.Value = CurrentHPP;
            if (CurrentHPP >= 75)
            {
                playerHP.ForeColor = Color.DarkGreen;
            }
            else if (CurrentHPP > 50 && CurrentHPP < 75)
            {
                playerHP.ForeColor = Color.Yellow;
            }
            else if (CurrentHPP > 25 && CurrentHPP < 50)
            {
                playerHP.ForeColor = Color.Orange;
            }
            else if (CurrentHPP < 25)
            {
                playerHP.ForeColor = Color.Red;
            }
        }
    }
}

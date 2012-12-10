using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CS6613_Final
{
    public enum MatchType
    {
        PvP,
        PvC,
        CvC
    }

    public partial class SplashScreen : Form
    {
        public static Dictionary<string, MatchType> MatchTypes = new Dictionary<string, MatchType>()
                                                                     {
                                                                         {"Player vs. Player",      MatchType.PvP},
                                                                         {"Player vs. Computer",    MatchType.PvC},
                                                                         {"Computer vs. Computer",  MatchType.CvC}
                                                                     };

        public static Dictionary<string, ComputerDifficulty> ComputerDifficultyLevels = new Dictionary<string, ComputerDifficulty>()
                                                                     {
                                                                         {"Easy",       ComputerDifficulty.Easy},
                                                                         {"Normal",     ComputerDifficulty.Normal},
                                                                         {"Hard",       ComputerDifficulty.Hard},
                                                                         {"Very Hard",  ComputerDifficulty.VeryHard}
                                                                     };

        public OptionsDialogResult Result { get; set; }

        public SplashScreen()
        {
            Result = new OptionsDialogResult();
            InitializeComponent();

            cOneComboBox.Items.AddRange(Enumerable.ToArray<string>(ComputerDifficultyLevels.Keys));
            cTwoComboBox.Items.AddRange(Enumerable.ToArray<string>(ComputerDifficultyLevels.Keys));
            cOneComboBox.SelectedIndex = 1;
            cTwoComboBox.SelectedIndex = 1;
        }

        private void pvpButton_CheckedChanged(object sender, EventArgs e)
        {
            if(pvpRadio.Enabled)
            {
                cOneComboBox.Enabled = false;
                cTwoComboBox.Enabled = false;

                Result.TypeOfMatch = MatchType.PvP;
            }
        }

        private void pvcButton_CheckedChanged(object sender, EventArgs e)
        {
            if (pvcRadio.Enabled)
            {
                cOneComboBox.Enabled = true;
                cTwoComboBox.Enabled = false;

                Result.TypeOfMatch = MatchType.PvC;
            }
        }

        private void cvcRadio_CheckedChanged(object sender, EventArgs e)
        {
            if (cvcRadio.Enabled)
            {
                cOneComboBox.Enabled = true;
                cTwoComboBox.Enabled = true;

                Result.TypeOfMatch = MatchType.CvC;
            }
        }

        private void playButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void SplashScreen_FormClosed(object sender, FormClosedEventArgs e)
        {
            Result.ComputerOneDifficulty = ComputerDifficultyLevels[cOneComboBox.Text];
            Result.ComputerTwoDifficulty = ComputerDifficultyLevels[cTwoComboBox.Text];
        }
    }
}

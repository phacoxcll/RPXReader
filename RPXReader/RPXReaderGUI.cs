using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace RPXReader
{
    public partial class RPXReaderGUI : Form
    {
        private ELF FileELF;
        private string LastFile;

        public RPXReaderGUI()
        {
            InitializeComponent();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Simplest reader of ELF, RPL and RPX formats.\n\n" +
                "By phacox.cll\n\n" +
                "Based on:\n" +
                "  https://www.sco.com/developers/gabi/2003-12-17/ch4.intro.html\n" +
                "  https://github.com/JonathanSalwan/binary-samples\n" +
                "  https://github.com/Relys/rpl2elf\n" +
                "  https://wiki.superfamicom.org/sfrom-file-format",
                "About", MessageBoxButtons.OK, MessageBoxIcon.None);
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                compressRPLRPXToolStripMenuItem.Enabled = false;
                decompressRPLRPXToolStripMenuItem.Enabled = false;
                extractROMToolStripMenuItem.Enabled = false;

                FileELF = ELF.Open(openFileDialog.FileName);

                richTextBox.Clear();

                if (FileELF != null)
                {
                    LastFile = openFileDialog.FileName;
                    richTextBox.AppendText("File: \"" + openFileDialog.FileName + "\"\n\n" + FileELF.ToString());
                }
                else
                {
                    LastFile = "";
                    richTextBox.AppendText("File: \"" + openFileDialog.FileName + "\"\n\nIt is not an ELF, RPX or RPL file.");
                }

                if (FileELF is RPX)
                {
                    compressRPLRPXToolStripMenuItem.Enabled = true;
                    decompressRPLRPXToolStripMenuItem.Enabled = true;
                }

                if (FileELF is RPXNES || FileELF is RPXSNES)
                    extractROMToolStripMenuItem.Enabled = true;

                richTextBox.Font = new Font(richTextBox.Font.FontFamily, richTextBox.Font.Size, richTextBox.Font.Style);
            }
        }

        private void compressRPLRPXToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (FileELF is RPX && File.Exists(LastFile))
            {
                string destination = Path.GetDirectoryName(LastFile) + "\\" +
                Path.GetFileNameWithoutExtension(LastFile) +
                "_compressed" + Path.GetExtension(LastFile);
                RPX.Compress(LastFile, destination);
                MessageBox.Show("Output: \"" + destination + "\"", "Compressed!");
            }
            else if (!(FileELF is RPX) && File.Exists(LastFile))
                MessageBox.Show("\"" + LastFile + "\" is not an RPX or RPL file.", "Warning!");
            else if (FileELF is RPX && !File.Exists(LastFile))
                MessageBox.Show("The file \"" + LastFile + "\" no longer exists. ", "Warning!");
            else
                MessageBox.Show("Open an RPX or RPL file.", "Warning!");
        }

        private void decompressRPLRPXToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (FileELF is RPX && File.Exists(LastFile))
            {
                string destination = Path.GetDirectoryName(LastFile) + "\\" +
                Path.GetFileNameWithoutExtension(LastFile) +
                "_decompressed" + Path.GetExtension(LastFile);
                RPX.Decompress(LastFile, destination);
                MessageBox.Show("Output: \"" + destination + "\"", "Decompressed!");
            }
            else if (!(FileELF is RPX) && File.Exists(LastFile))
                MessageBox.Show("\"" + LastFile + "\" is not an RPX or RPL file.", "Warning!");
            else if (FileELF is RPX && !File.Exists(LastFile))
                MessageBox.Show("The file \"" + LastFile + "\" no longer exists. ", "Warning!");
            else
                MessageBox.Show("Open an RPX or RPL file.", "Warning!");
        }

        private void extractROMToolStripMenuItem_Click(object sender, EventArgs e)
        {
            folderBrowserDialog.Description = "Choose the folder where to place the ROM file.";
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                if (FileELF is RPXNES)
                {
                    RPXNES vc = FileELF as RPXNES;
                    string filename = folderBrowserDialog.SelectedPath + "\\" + vc.GetROMFileName();
                    FileStream fs = File.Open(filename, FileMode.Create);
                    if (vc.ROM.IsFDS)
                        fs.Write(vc.ROM.Data, 0, vc.ROM.RawSize);
                    else
                    {
                        fs.Write(vc.ROM.Data, 0, vc.ROM.RawSize + 16);
                        fs.Position = 3;
                        fs.WriteByte(0x1A);
                    }
                    fs.Close();
                    MessageBox.Show("Output: \"" + filename + "\"", "ROM extracted!");
                }
                else if (FileELF is RPXSNES)
                {
                    RPXSNES vc = FileELF as RPXSNES;
                    string filename = folderBrowserDialog.SelectedPath + "\\" + vc.GetROMFileName();
                    FileStream fs = File.Open(filename, FileMode.Create);
                    fs.Write(vc.ROM.Data, 0, vc.ROM.Data.Length);
                    fs.Close();
                    MessageBox.Show("Output: \"" + filename + "\"", "ROM extracted!");
                }
                else
                    MessageBox.Show("Open an VC NES RPX or VC SNES RPX file.", "Warning!");
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Undo()
        {
            if (richTextBox.CanUndo)
                richTextBox.Undo();
        }

        private void Redo()
        {
            if (richTextBox.CanRedo)
                richTextBox.Redo();
        }

        private void Cut()
        {
            if (richTextBox.SelectionLength > 0)
                richTextBox.Cut();
        }

        private void Copy()
        {
            if (richTextBox.SelectionLength > 0)
                richTextBox.Copy();
        }

        private void Paste()
        {
            if (Clipboard.GetDataObject().GetDataPresent(DataFormats.Text) == true)
                richTextBox.Paste();
        }

        private void Delete()
        {
            if (richTextBox.SelectionLength > 0)
                richTextBox.SelectedText = "";
        }

        private void SelectAll()
        {
            richTextBox.Select();
            richTextBox.SelectAll();
        }

        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Undo();
        }

        private void redoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Redo();
        }

        private void cutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Cut();
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Copy();
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Paste();
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Delete();
        }

        private void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SelectAll();
        }

        private void cutToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Cut();
        }

        private void copyToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Copy();
        }

        private void pasteToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Paste();
        }

        private void deleteToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Delete();
        }

        private void selectAllToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            SelectAll();
        }
    }
}

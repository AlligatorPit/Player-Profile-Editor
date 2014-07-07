using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Globalization;

namespace PlayerProfileEditor
{
  public partial class FormMain : Form
  {
    string fnmProfile;
    byte[] abAppearance;

    public FormMain(string[] args)
    {
      InitializeComponent();

      if (args.Length > 0) {
        fnmProfile = args[0];
        LoadProfile();
      }
    }

    private void openToolStripButton_Click(object sender, EventArgs e)
    {
      OpenFileDialog ofd = new OpenFileDialog();
      ofd.Filter = "Player files (*.plr)|*.plr|All files (*.*)|*.*";
      if (ofd.ShowDialog() == DialogResult.OK) {
        fnmProfile = ofd.FileName;
        LoadProfile();
      }
    }

    private void saveToolStripButton_Click(object sender, EventArgs e)
    {
      SaveProfile();
    }

    void PictureText(string strText)
    {
      Bitmap bmp = new Bitmap(picName.Width, picName.Height);
      Graphics g = Graphics.FromImage(bmp);

      g.Clear(picName.BackColor);
      g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

      float fXPos = 0;
      bool bBold = false;
      bool bItalic = false;
      Color colColor = Color.White;

      for (int i = 0; i < strText.Length; ) {
        if (strText[i] == '^') {
          if (i == strText.Length - 1) {
            break;
          }
          switch (strText[++i]) {
            case 'r':
              bBold = false;
              bItalic = false;
              colColor = Color.White;
              break;

            case 'c':
              i++;
              try {
                byte ubRed = byte.Parse(strText.Substring(i, 2), NumberStyles.AllowHexSpecifier); i += 2;
                byte ubGreen = byte.Parse(strText.Substring(i, 2), NumberStyles.AllowHexSpecifier); i += 2;
                byte ubBlue = byte.Parse(strText.Substring(i, 2), NumberStyles.AllowHexSpecifier); i++;
                colColor = Color.FromArgb(255, ubRed, ubGreen, ubBlue);
              } catch {
                colColor = Color.Black;
              }
              break;
            case 'b': bBold = true; break;
            case 'i': bItalic = true; break;
            case 'f': i++; break;

            case 'C': colColor = Color.White; break;
            case 'B': bBold = false; break;
            case 'I': bItalic = false; break;

            case '^':
              Font fnt = new Font(SystemFonts.DefaultFont, FontStyle.Regular | (bBold ? FontStyle.Bold : 0) | (bItalic ? FontStyle.Italic : 0));
              g.DrawString("^", fnt, new SolidBrush(colColor), new PointF(fXPos, 4));
              fXPos += g.MeasureString("^", fnt).Width * 0.8f;
              i++;
              break;
          }
          i++;
        } else {
          if (i < strText.Length) {
            string strChar = new string(strText[i], 1);
            Font fnt = new Font(SystemFonts.DefaultFont, FontStyle.Regular | (bBold ? FontStyle.Bold : 0) | (bItalic ? FontStyle.Italic : 0));
            g.DrawString(strChar, fnt, new SolidBrush(colColor), new PointF(fXPos, 4));
            fXPos += g.MeasureString(strChar, fnt).Width * 0.8f;
            i++;
          }
        }
      }

      picName.Image = bmp;
    }

    string RemoveCodes(string str)
    {
      string str_ret = "";
      for (int i = 0; i < str.Length; i++) {
        if (str[i] == '^') {
          char c = str[++i];
          switch (c) {
            case '^': str_ret += '^'; break;
            case 'c': i += 6; break;
            case 'a': i += 2; break;
            case 'f': i++; break;
          }
        } else {
          str_ret += str[i];
        }
      }
      return str_ret;
    }

    void LoadProfile()
    {
      using (BinaryReader reader = new BinaryReader(File.OpenRead(fnmProfile))) {
        if (reader.ReadInt32() != 876825680 /* PLC4 */) {
          MessageBox.Show("Header not found!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
          return;
        }

        string strName = "";
        string strTeam = "";

        {
          int iLen = reader.ReadInt32();
          strName = Encoding.ASCII.GetString(reader.ReadBytes(iLen));
          textName.Text = strName;
        }

        {
          int iLen = reader.ReadInt32();
          strTeam = Encoding.ASCII.GetString(reader.ReadBytes(iLen));
          textTeam.Text = strTeam;
        }

        byte[] abGUID = reader.ReadBytes(16);
        abAppearance = reader.ReadBytes(32);

        string strGUID = "";
        for (int i = 0; i < 16; i++) {
          strGUID += abGUID[i].ToString("x2");
        }
        textGUID.Text = strGUID.ToUpper();

        Text = "Player profile editor - " + fnmProfile;
      }
    }

    void SaveProfile()
    {
      if (textGUID.Text.Length != 32) {
        MessageBox.Show("Invalid GUID!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        return;
      }

      for (int i = 0; i < 32; i++) {
        if (!"0123456789abcdef".Contains(textGUID.Text.ToLower()[i])) {
          MessageBox.Show("Invalid GUID!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
          return;
        }
      }

      if (File.Exists(fnmProfile)) {
        File.Delete(fnmProfile);
      }

      using (BinaryWriter writer = new BinaryWriter(File.OpenWrite(fnmProfile))) {
        writer.Write(Encoding.ASCII.GetBytes("PLC4"));

        writer.Write(textName.Text.Length);
        writer.Write(Encoding.ASCII.GetBytes(textName.Text));

        writer.Write(textTeam.Text.Length);
        writer.Write(Encoding.ASCII.GetBytes(textTeam.Text));

        byte[] abGUID = new byte[16];
        for (int i = 0; i < 32; i += 2) {
          string strPart = textGUID.Text.Substring(i, 2);
          abGUID[i / 2] = byte.Parse(strPart, NumberStyles.AllowHexSpecifier);
        }

        writer.Write(abGUID);
        writer.Write(abAppearance);
      }
    }

    private void textName_TextChanged(object sender, EventArgs e)
    {
      PictureText(textName.Text);
    }

    private void textTeam_TextChanged(object sender, EventArgs e)
    {
      PictureText(textTeam.Text);
    }

    Color GetPureColorCodeFromDialog()
    {
      ColorDialog dialog = new ColorDialog();
      if (dialog.ShowDialog() != DialogResult.OK) {
        return Color.Transparent;
      }
      return dialog.Color;
    }

    string ColorCodeFromColor(Color col)
    {
      return col.R.ToString("x2") + col.G.ToString("x2") + col.B.ToString("x2");
    }

    string GetColorCodeFromDialog()
    {
      Color col = GetPureColorCodeFromDialog();
      if (col == Color.Transparent) {
        return "";
      }
      return ColorCodeFromColor(col);
    }

    private void colorToolStripButton_Click(object sender, EventArgs e)
    {
      int sel_start = textName.SelectionStart;
      int sel_len = textName.SelectionLength;
      string str = textName.Text;

      string col = GetColorCodeFromDialog();
      if (col == "") {
        return;
      }

      str = str.Insert(sel_start, "^c" + col);
      if (sel_len > 0) {
        str = str.Insert(sel_start + 8 + sel_len, "^C");
      }

      textName.Text = str;
      textName.Focus();
      textName.SelectionStart = sel_start;
      textName.SelectionLength = sel_len + 8 + 2; // ^cff0000 (8) + ^C (2)
    }

    float Lerp(float val1, float val2, float x)
    {
      return val1 + (val2 - val1) * x;
    }

    Color Lerp(Color col1, Color col2, float x)
    {
      int r = (int)Lerp((float)col1.R, (float)col2.R, x);
      int g = (int)Lerp((float)col1.G, (float)col2.G, x);
      int b = (int)Lerp((float)col1.B, (float)col2.B, x);
      return Color.FromArgb(r, g, b);
    }

    private void gradientToolStripButton_Click(object sender, EventArgs e)
    {
      int sel_start = textName.SelectionStart;
      int sel_len = textName.SelectionLength;
      string str = textName.Text;

      if (sel_start == str.Length) {
        sel_start = 0;
      }
      if (sel_len == 0) {
        sel_len = str.Length - sel_start;
      }

      Color col1 = GetPureColorCodeFromDialog();
      if (col1 == Color.Transparent) {
        return;
      }

      Color col2 = GetPureColorCodeFromDialog();
      if (col2 == Color.Transparent) {
        return;
      }

      string str_add = "";
      for (int i = 0; i < sel_len; i++) {
        if (str[sel_start + i] == '^') {
          char c = str[sel_start + ++i];
          if (c != '^') {
            switch (c) {
              case 'c': i += 6; break;
              case 'f': str_add += "^f" + str[sel_start + ++i]; break;
              case 'a': str_add += "^a" + str.Substring(sel_start + ++i, 2); i += 2; break;
            }
            continue;
          }
        }
        Color col = Lerp(col1, col2, (float)i / (float)sel_len);
        str_add += "^c" + ColorCodeFromColor(col) + str[sel_start + i];
      }
      str = str.Substring(0, sel_start) + str_add + "^C" + str.Substring(sel_start + sel_len);

      textName.Text = str;
    }
  }
}

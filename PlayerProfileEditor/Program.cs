﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace PlayerProfileEditor
{
  static class Program
  {
    [STAThread]
    static void Main(string[] args)
    {
      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);
      Application.Run(new FormMain(args));
    }
  }
}

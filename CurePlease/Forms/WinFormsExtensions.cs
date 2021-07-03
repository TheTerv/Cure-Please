﻿using System;
using System.Windows.Forms;

namespace CurePlease.Forms
{
    public static class WinFormsExtensions
    {
        public static void ForAllControls(this Control parent, Action<Control> action)
        {
            foreach (Control c in parent.Controls)
            {
                action(c);
                ForAllControls(c, action);
            }
        }
    }
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thismaker.Aretha
{
    internal interface ISoul
    {
        Soul Soul { get;}
        void WaitForCommand(string[] args);
        void Speak(string text);
        string Ask(string question, bool isYN, bool isCase);
        string GetPath(string question, bool input, bool confirm);
    }
}

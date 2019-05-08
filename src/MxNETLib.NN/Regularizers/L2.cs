﻿using System;
using System.Collections.Generic;
using System.Text;
using MxNetLib;

namespace MxNetLib.NN.Regularizers
{
    public class L2 : L1L2
    {
        public L2(float l=0.01f)
            : base(0, l)
        {

        }
    }
}

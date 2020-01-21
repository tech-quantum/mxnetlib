﻿using System;
using System.Collections.Generic;
using System.Text;

namespace MxNetLib.Callbacks
{
    public interface IEpochEndCallback
    {
        void Invoke(int epoch, Symbol symbol, Dictionary<string, NDArray> arg_params, Dictionary<string, NDArray> aux_params);
    }
}

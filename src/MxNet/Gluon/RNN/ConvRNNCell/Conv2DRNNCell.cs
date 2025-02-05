﻿using System;
using System.Collections.Generic;
using System.Text;

namespace MxNet.Gluon.RNN
{
    public class Conv2DRNNCell : _ConvRNNCell
    {
        public Conv2DRNNCell(Shape input_shape, int hidden_channels, (int, int) i2h_kernel, (int, int) h2h_kernel, (int, int)? i2h_pad = null,
            (int, int)? i2h_dilate = null, (int, int)? h2h_dilate = null, string i2h_weight_initializer = null, string h2h_weight_initializer = null,
            string i2h_bias_initializer = "zeros", string h2h_bias_initializer = "zeros", string conv_layout = "NCHW", ActivationType activation = ActivationType.Tanh) 
            : base(input_shape, hidden_channels, new int[] { i2h_kernel.Item1, i2h_kernel.Item2 } , new int[] { h2h_kernel.Item1, h2h_kernel.Item2 },
                  i2h_pad.HasValue ? new int[] { i2h_pad.Value.Item1, i2h_pad.Value.Item2 } : new int[] { 0, 0 },
                  i2h_dilate.HasValue ? new int[] { i2h_dilate.Value.Item1, i2h_dilate.Value.Item2 } : new int[] { 1, 1 },
                  h2h_dilate.HasValue ? new int[] { h2h_dilate.Value.Item1, h2h_dilate.Value.Item2 } : new int[] { 1, 1 },
                  i2h_weight_initializer, h2h_weight_initializer, i2h_bias_initializer, h2h_bias_initializer, 2, conv_layout, activation)
        {
        }
    }
}

﻿using MxNet.Keras.Engine;
using System;
using System.Collections.Generic;
using System.Text;

namespace MxNet.Keras.Layers
{
    public class Permute : Layer
    {
        public Permute(Shape dims)
        {
            throw new NotImplementedException();
        }

        public override KerasSymbol[] Call(KerasSymbol[] inputs, FuncArgs kwargs)
        {
            throw new NotImplementedException();
        }

        public override ConfigDict GetConfig()
        {
            throw new NotImplementedException();
        }

        public override Shape[] ComputeOutputShape(Shape[] input_shape)
        {
            throw new NotImplementedException();
        }
    }
}
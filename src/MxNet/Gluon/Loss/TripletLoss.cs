﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MxNet.Gluon
{
    public class TripletLoss : Loss
    {
        public float Margin { get; set; }

        public TripletLoss(float margin = 1, float? weight = null, int? batch_axis = 0, string prefix = null, ParameterDict @params = null) : base(weight, batch_axis, prefix, @params)
        {
            Margin = margin;
        }

        public override NDArrayOrSymbol HybridForward(NDArrayOrSymbol pred, NDArrayOrSymbol label, NDArrayOrSymbol sample_weight = null, params object[] args)
        {
            NDArrayOrSymbol negative = (NDArrayOrSymbol)args[0];
            if (pred.IsNDArray)
                return F(pred.NdX, label, negative);

            return F(pred.SymX, label, negative);
        }

        private NDArray F(NDArray pred, NDArray positive, NDArray negative)
        {
            positive = nd.ReshapeLike(positive, pred);
            negative = nd.ReshapeLike(negative, pred);
            var loss = nd.Sum(nd.Square(positive - pred) - nd.Square(negative - pred), axis: BatchAxis.Value, exclude: true);
            loss = nd.Relu(loss + Margin);
            loss = ApplyWeighting(loss, Weight, null);
            return loss;
        }

        private Symbol F(Symbol pred, Symbol positive, Symbol negative)
        {
            positive = sym.ReshapeLike(positive, pred);
            negative = sym.ReshapeLike(negative, pred);
            var loss = sym.Sum(sym.Square(positive - pred) - sym.Square(negative - pred), axis: BatchAxis.Value, exclude: true);
            loss = sym.Relu(loss + Margin);
            loss = ApplyWeighting(loss, Weight, null);
            return loss;
        }
    }
}

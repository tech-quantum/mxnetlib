﻿/*****************************************************************************
   Copyright 2018 The MxNet.Sharp Authors. All Rights Reserved.

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
******************************************************************************/
using MxNet.Numpy;
using MxNet.Sym.Numpy;

namespace MxNet.Gluon.Losses
{
    public class HuberLoss : Loss
    {
        public HuberLoss(float rho = 1, float? weight = null, int? batch_axis = 0, string prefix = null,
            ParameterDict @params = null) : base(weight, batch_axis)
        {
            Rho = rho;
        }

        public float Rho { get; set; }

        public override NDArrayOrSymbol HybridForward(NDArrayOrSymbol pred, NDArrayOrSymbol label,
            NDArrayOrSymbol sample_weight = null, params object[] args)
        {
            if (pred.IsNDArray)
                return F(pred.NdX, label, sample_weight);

            return F(pred.SymX, label, sample_weight);
        }

        private ndarray F(ndarray pred, ndarray label, ndarray sample_weight = null)
        {
            label = nd.ReshapeLike(label, pred);
            var loss = nd.Abs(label - pred);
            loss = nd.Where(loss > Rho, loss - 0.5f * Rho, 0.5f / Rho * nd.Square(loss));
            loss = ApplyWeighting(loss, Weight, sample_weight);
            return nd.Mean(loss, BatchAxis.Value, exclude: true);
        }

        private _Symbol F(_Symbol pred, _Symbol label, _Symbol sample_weight = null)
        {
            label = sym.ReshapeLike(label, pred);
            var loss = sym.Abs(label - pred);
            loss = sym.Where(sym.GreaterScalar(loss, 0.5f), loss - 0.5f * Rho, 0.5f / Rho * sym.Square(loss));
            loss = ApplyWeighting(loss, Weight, sample_weight);
            return sym.Mean(loss, BatchAxis.Value, exclude: true);
        }
    }
}
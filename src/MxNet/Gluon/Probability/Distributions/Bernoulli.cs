﻿using System;
using System.Collections.Generic;
using System.Text;

namespace MxNet.Gluon.Probability.Distributions
{
    public class Bernoulli : ExponentialFamily
    {
        public NDArrayOrSymbol Prob
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public NDArrayOrSymbol Logit
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override NDArrayOrSymbol Mean
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override NDArrayOrSymbol Variance
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override object[] NaturalParams
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public Bernoulli(NDArrayOrSymbol prob = null, NDArrayOrSymbol logit = null, bool? validate_args = null)
        {
            throw new NotImplementedException();
        }

        public override NDArrayOrSymbol BroadcastTo(Shape batch_shape)
        {
            throw new NotImplementedException();
        }

        public override NDArrayOrSymbol LogProb(NDArrayOrSymbol value)
        {
            throw new NotImplementedException();
        }

        public override NDArrayOrSymbol Sample(Shape size)
        {
            throw new NotImplementedException();
        }

        public override NDArrayOrSymbol SampleN(Shape size)
        {
            throw new NotImplementedException();
        }

        public override NDArrayOrSymbol LogNormalizer(params object[] natural_params)
        {
            throw new NotImplementedException();
        }

        public override NDArrayOrSymbol Entropy()
        {
            throw new NotImplementedException();
        }
    }
}

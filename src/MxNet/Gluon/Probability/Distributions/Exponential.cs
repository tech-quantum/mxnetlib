﻿using System;
using System.Collections.Generic;
using System.Text;

namespace MxNet.Gluon.Probability.Distributions
{
    public class Exponential : ExponentialFamily
    {
        public float Rate
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

        public override NDArrayOrSymbol StdDev
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

        public Exponential(float scale = 1, bool? validate_args = null)
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

        public override NDArrayOrSymbol BroadcastTo(Shape batch_shape)
        {
            throw new NotImplementedException();
        }

        public override NDArrayOrSymbol Entropy()
        {
            throw new NotImplementedException();
        }

        public override NDArrayOrSymbol LogProb(NDArrayOrSymbol value)
        {
            throw new NotImplementedException();
        }

        public override NDArrayOrSymbol Cdf(NDArrayOrSymbol value)
        {
            throw new NotImplementedException();
        }

        public override NDArrayOrSymbol Icdf(NDArrayOrSymbol value)
        {
            throw new NotImplementedException();
        }

        public override NDArrayOrSymbol LogNormalizer(params object[] natural_params)
        {
            throw new NotImplementedException();
        }
    }
}

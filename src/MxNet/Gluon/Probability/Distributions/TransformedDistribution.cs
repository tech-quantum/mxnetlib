﻿using MxNet.Gluon.Probability.Transformations;
using System;
using System.Collections.Generic;
using System.Text;

namespace MxNet.Gluon.Probability.Distributions
{
    public class TransformedDistribution : Distribution
    {
        public TransformedDistribution()
        {

        }

        public TransformedDistribution(Distribution base_dist, Transformation[] transforms, bool? validate_args = null)
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
    }
}

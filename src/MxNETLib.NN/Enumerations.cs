﻿using System;
using System.Collections.Generic;
using System.Text;

namespace MxNetLib.NN
{
    public enum ActivationType
    {
        Linear,
        ReLU,
        Sigmoid,
        Tanh,
        Elu,
        Exp,
        HargSigmoid,
        LeakyReLU,
        PReLU,
        RReLU,
        SeLU,
        Softmax,
        Softplus,
        SoftSign
    }

    public enum OptimizerType
    {
        SGD,
        Signum,
        RMSprop,
        Adagrad,
        Adadelta,
        Adam
    }

    public enum LossType
    {
        MeanSquaredError,
        MeanAbsoluteError,
        MeanAbsolutePercentageError,
        MeanAbsoluteLogError,
        SquaredHinge,
        Hinge,
        SigmoidBinaryCrossEntropy,
        SoftmaxCategorialCrossEntropy,
        CTC,
        KullbackLeiblerDivergence,
        Poisson
    }
}

﻿using MxNetLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace MxNetLib.Metrics
{
    public sealed class MAE : BaseMetric
    {
        #region Constructors

        public MAE() : base("mae") { }

        #endregion

        #region Methods

        public override void Update(NDArray labels, NDArray preds)
        {
            if (labels == null)
                throw new ArgumentNullException(nameof(labels));
            if (preds == null)
                throw new ArgumentNullException(nameof(preds));

            preds = preds.Ravel();
            var result = nd.Mean(nd.Abs(preds - labels)).Value;
            this.Values.Add(result);
        }

        #endregion

    }
}

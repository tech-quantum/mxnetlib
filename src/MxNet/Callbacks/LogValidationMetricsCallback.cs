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
using System;
using MxNet.Gluon.Metrics;

namespace MxNet.Callbacks
{
    public class LogValidationMetricsCallback : IEvalEndCallback
    {
        public void Invoke(int epoch, EvalMetric eval_metric)
        {
            if (eval_metric == null) return;

            var name_value = eval_metric.GetNameValue();
            foreach (var item in name_value)
                Logger.Log(string.Format("Epoch[{0}] Validation-{1}={2}", epoch, item.Key, Math.Round(item.Value, 2)));
        }
    }
}
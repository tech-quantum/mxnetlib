﻿using System;
using System.Collections.Generic;
using System.Text;

namespace MxNet.Contrib.ONNX.Mx2ONNX
{
    public class OpTranslations
    {
        public static T ParseHelper<T>(Dictionary<string, object> attrs, string attrs_name, T alt_value= default(T))
        {
            throw new NotImplementedRelease2Exception();
        }

        public static int TransformPadding(int pad_width)
        {
            throw new NotImplementedRelease2Exception();
        }

        public static string[] ConvertStringToList(string string_val)
        {
            throw new NotImplementedRelease2Exception();
        }

        public static int GetBooleanAttributeValue(Dictionary<string, object> attrs, string attr_name)
        {
            throw new NotImplementedRelease2Exception();
        }

        public  static (string, dynamic[], Dictionary<string,object>) GetInputs(dynamic node, FuncArgs kwargs, bool with_shapes= false)
        {
            throw new NotImplementedRelease2Exception();
        }

        public static dynamic CreateBasicOpNode(string op_name, dynamic node, FuncArgs kwargs)
        {
            throw new NotImplementedRelease2Exception();
        }

        public static dynamic ConvertWeightsAndInputs(dynamic node, FuncArgs kwargs)
        {
            throw new NotImplementedRelease2Exception();
        }
    }
}

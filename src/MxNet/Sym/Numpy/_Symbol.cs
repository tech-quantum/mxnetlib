﻿using MxNet.Initializers;
using MxNet.Interop;
using MxNet.IO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using SymbolHandle = System.IntPtr;

namespace MxNet.Sym.Numpy
{
    public class _Symbol : DisposableMXNetObject, IEnumerable<_Symbol>
    {
        #region Fields

        #endregion

        #region Constructors

        internal _Symbol()
            : this(SymbolHandle.Zero)
        {
        }

        internal _Symbol(SymbolHandle handle)
        {
            NativePtr = handle;
        }

        internal _Symbol(string name)
        {
            if (NativeMethods.MXSymbolCreateVariable(name, out var @out) != NativeMethods.OK)
                throw new MXNetException($"Failed to create {nameof(_Symbol)}");

            NativePtr = @out;
        }

        //public _Symbol(string operatorName, 
        //              string name,
        //              IList<string> inputKeys,
        //              IList<SymbolHandle> inputValues,
        //              IList<string> configKeys,
        //              IList<string> configValues)
        //{
        //    if (inputKeys == null)
        //        throw new ArgumentNullException(nameof(inputKeys));
        //    if (inputValues == null)
        //        throw new ArgumentNullException(nameof(inputValues));
        //    if (configKeys == null)
        //        throw new ArgumentNullException(nameof(configKeys));
        //    if (configValues == null)
        //        throw new ArgumentNullException(nameof(configValues));

        //    var creator = OpMap.Get_SymbolCreator(operatorName);
        //    NativeMethods.MXSymbolCreateAtomic_Symbol(creator, 
        //                                             (uint)configKeys.Count,
        //                                             configKeys.ToArray(),
        //                                             configValues.ToArray(),
        //                                             out var handle);

        //    NativeMethods.MXSymbolCompose(handle, 
        //                                  operatorName,
        //                                  (uint)inputKeys.Count,
        //                                  inputKeys.ToArray(),
        //                                  inputValues.ToArray());

        //    blob_ptr_ = std::make_shared<SymBlob>(handle);
        //    this.NativePtr = @out;
        //}

        #endregion

        #region Properties

        public string Name
        {
            get
            {
                ThrowIfDisposed();
                if (NativePtr == SymbolHandle.Zero)
                    return null;

                Logging.CHECK_EQ(NativeMethods.MXSymbolGetName(NativePtr, out var @out, out var success), NativeMethods.OK);
                if (@out == SymbolHandle.Zero)
                    return null;

                return Marshal.PtrToStringAnsi(@out);
            }
        }

        public _Symbol this[int index]
        {
            get
            {
                ThrowIfDisposed();

                Logging.CHECK_EQ(NativeMethods.MXSymbolGetOutput(NativePtr, (uint)index, out var @out), NativeMethods.OK);
                return new _Symbol(@out);
            }
        }

        public _Symbol this[string name]
        {
            get
            {
                var names = this.ListOutputs().ToList();
                ThrowIfDisposed();

                Logging.CHECK_EQ(NativeMethods.MXSymbolGetOutput(NativePtr, (uint)names.IndexOf(name), out var @out), NativeMethods.OK);
                return new _Symbol(@out);
            }
        }

        public _Symbol this[int rowBegin, int rowEnd]
        {
            get
            {
                return sym_np_ops.slice(this, rowBegin, rowEnd);
            }
        }

        #endregion

        #region Methods

        public SymbolHandle GetHandle()
        {
            ThrowIfDisposed();
            return NativePtr;
        }

        public _Symbol GetInternals()
        {
            Logging.CHECK_EQ(NativeMethods.MXSymbolGetInternals(GetHandle(), out var handle), NativeMethods.OK);
            return new _Symbol(handle);
        }

        public _Symbol GetChildren()
        {
            Logging.CHECK_EQ(NativeMethods.MXSymbolGetChildren(GetHandle(), out var handle), NativeMethods.OK);
            var ret = new _Symbol(handle);
            if (ret.ListOutputs().Count == 0)
                return null;
            return ret;
        }

        public static _Symbol Group(SymbolList _Symbols)
        {
            var handleList = _Symbols.Select(_Symbol => _Symbol.GetHandle()).ToArray();
            Logging.CHECK_EQ(NativeMethods.MXSymbolCreateGroup((uint)handleList.Length, handleList, out var @out), NativeMethods.OK);
            return new _Symbol(@out);
        }

        public void InferArgsMap(Context context,
            NDArrayDict argsMap,
            NDArrayDict knownArgs)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            if (argsMap == null)
                throw new ArgumentNullException(nameof(argsMap));
            if (knownArgs == null)
                throw new ArgumentNullException(nameof(knownArgs));

            ThrowIfDisposed();

            var argShapes = new Dictionary<string, Shape>();

            var argNameList = ListArguments();
            foreach (var argName in argNameList)
                if (knownArgs[argName] != null)
                    argShapes[argName] = knownArgs[argName].shape;

            var (inShapes, outShapes, auxShapes) = InferShape(argShapes);

            for (var i = 0; i < inShapes.Length; ++i)
            {
                var shape = inShapes[i];
                var argName = argNameList[i];
                if (knownArgs[argName] != null)
                {
                    argsMap[argName] = knownArgs[argName];
                }
                else
                {
                    var array = new NDArray(shape, false);
                    argsMap[argName] = array;
                    //NDArray.SampleGaussian(0, 1, array);
                    nd.Random.Uniform(0, 1, array.Shape).CopyTo(array);
                }
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="argShapes"></param>
        /// <returns>Return arg_shapes, out_shapes, aux_shapes</returns>
        public (Shape[], Shape[], Shape[]) InferShape(Dictionary<string, Shape> argShapes)
        {
            if (argShapes == null)
                throw new ArgumentNullException(nameof(argShapes));

            var inShape = new List<Shape>();
            var auxShape = new List<Shape>();
            var outShape = new List<Shape>();

            ThrowIfDisposed();
            var argIndPtr = new List<int> { 0 };
            var argShapeData = new List<int>();

            foreach (var item in argShapes.Values)
            {
                foreach (var i in item.Data)
                {
                    if (i == 0)
                        continue;

                    argShapeData.Add(i);
                }

                argIndPtr.Add(argShapeData.Count);
            }

            unsafe
            {
                var keys = argShapes.Keys.ToArray();
                var argIndPtrArray = argIndPtr.ToArray();
                var argShapeDataArray = argShapeData.ToArray();
                {

                    int inShapeSize;
                    int* inShapeNdim;
                    int** inShapeData;
                    Logging.CHECK_EQ(NativeMethods.MXSymbolInferShape(NativePtr,
                        (uint)argShapes.Count,
                        keys,
                        argIndPtrArray,
                        argShapeDataArray,
                        &inShapeSize,
                        &inShapeNdim,
                        &inShapeData,
                        out var outShapeSize,
                        out var outShapeNdim,
                        out var outShapeData,
                        out var auxShapeSize,
                        out var auxShapeNdim,
                        out var auxShapeData,
                        out var complete), NativeMethods.OK);

                    if (complete == 0)
                        return (null, null, null);

                    for (var i = 0; i < inShapeSize; ++i)
                    {
                        inShape.Add(new Shape());
                        for (var j = 0; j < inShapeNdim[i]; ++j)
                            inShape[i].Add(inShapeData[i][j]);
                    }

                    for (var i = 0; i < auxShapeSize; ++i)
                    {
                        auxShape.Add(new Shape());
                        for (var j = 0; j < auxShapeNdim[i]; ++j)
                            auxShape[i].Add(auxShapeData[i][j]);
                    }

                    for (var i = 0; i < outShapeSize; ++i)
                    {
                        outShape.Add(new Shape());
                        for (var j = 0; j < outShapeNdim[i]; ++j)
                            outShape[i].Add(outShapeData[i][j]);
                    }
                }
            }

            return (inShape.ToArray(), outShape.ToArray(), auxShape.ToArray());
        }

        public (Shape[], Shape[], Shape[]) InferShapePartial(Dictionary<string, Shape> argShapes = null)
        {
            if (argShapes == null)
                argShapes = new Dictionary<string, Shape>();

            var inShape = new List<Shape>();
            var auxShape = new List<Shape>();
            var outShape = new List<Shape>();

            ThrowIfDisposed();
            var argIndPtr = new List<int> { 0 };
            var argShapeData = new List<int>();

            foreach (var item in argShapes.Values)
            {
                foreach (var i in item.Data)
                {
                    if (i == 0)
                        continue;

                    argShapeData.Add(i);
                }

                argIndPtr.Add(argShapeData.Count);
            }

            unsafe
            {
                var keys = argShapes.Keys.ToArray();
                var argIndPtrArray = argIndPtr.ToArray();
                var argShapeDataArray = argShapeData.ToArray();
                {
                    int inShapeSize;
                    int* inShapeNdim;
                    int** inShapeData;

                    Logging.CHECK_EQ(NativeMethods.MXSymbolInferShapePartial(NativePtr,
                        (uint)argShapes.Count,
                        keys,
                        argIndPtrArray,
                        argShapeDataArray,
                        &inShapeSize,
                        &inShapeNdim,
                        &inShapeData,
                        out var outShapeSize,
                        out var outShapeNdim,
                        out var outShapeData,
                        out var auxShapeSize,
                        out var auxShapeNdim,
                        out var auxShapeData,
                        out var complete), NativeMethods.OK);

                    if (complete == 0)
                        return (null, null, null);

                    for (var i = 0; i < inShapeSize; ++i)
                    {
                        inShape.Add(new Shape());
                        for (var j = 0; j < inShapeNdim[i]; ++j)
                            inShape[i].Add(inShapeData[i][j]);
                    }

                    for (var i = 0; i < auxShapeSize; ++i)
                    {
                        auxShape.Add(new Shape());
                        for (var j = 0; j < auxShapeNdim[i]; ++j)
                            auxShape[i].Add(auxShapeData[i][j]);
                    }

                    for (var i = 0; i < outShapeSize; ++i)
                    {
                        outShape.Add(new Shape());
                        for (var j = 0; j < outShapeNdim[i]; ++j)
                            outShape[i].Add(outShapeData[i][j]);
                    }
                }
            }

            return (inShape.ToArray(), outShape.ToArray(), auxShape.ToArray());
        }

        public (DType[], DType[], DType[]) InferType(Dictionary<string, DType> argTypes = null)
        {
            if (argTypes == null)
                argTypes = new Dictionary<string, DType>();

            var inType = new List<DType>();
            var auxType = new List<DType>();
            var outType = new List<DType>();

            ThrowIfDisposed();
            var argTypeData = argTypes.Values.Select(x => (x.Index)).ToList();

            unsafe
            {
                var keys = argTypes.Keys.ToArray();
                var argShapeDataArray = argTypeData.ToArray();
                {
                    int inShapeSize;
                    int* inShapeData;

                    Logging.CHECK_EQ(NativeMethods.MXSymbolInferType(NativePtr,
                        (uint)argTypes.Count,
                        keys,
                        argShapeDataArray,
                        &inShapeSize,
                        &inShapeData,
                        out var outShapeSize,
                        out var outShapeData,
                        out var auxShapeSize,
                        out var auxShapeData,
                        out var complete), NativeMethods.OK);

                    if (complete == 0)
                        return (null, null, null);

                    for (var i = 0; i < inShapeSize; ++i)
                        inType.Add(DType.GetType(inShapeData[i]));

                    for (var i = 0; i < auxShapeSize; ++i)
                        auxType.Add(DType.GetType(auxShapeData[i]));

                    for (var i = 0; i < outShapeSize; ++i)
                        outType.Add(DType.GetType(outShapeData[i]));
                }
            }

            return (inType.ToArray(), outType.ToArray(), auxType.ToArray());
        }

        public (DType[], DType[], DType[]) InferTypePartial(Dictionary<string, DType> argTypes = null)
        {
            if (argTypes == null)
                argTypes = new Dictionary<string, DType>();

            var inType = new List<DType>();
            var auxType = new List<DType>();
            var outType = new List<DType>();

            ThrowIfDisposed();
            var argTypeData = argTypes.Values.Select(x => (x.Index)).ToList();

            unsafe
            {
                var keys = argTypes.Keys.ToArray();
                var argShapeDataArray = argTypeData.ToArray();
                {
                    int inShapeSize;
                    int* inShapeData;

                    Logging.CHECK_EQ(NativeMethods.MXSymbolInferTypePartial(NativePtr,
                        (uint)argTypes.Count,
                        keys,
                        argShapeDataArray,
                        &inShapeSize,
                        &inShapeData,
                        out var outShapeSize,
                        out var outShapeData,
                        out var auxShapeSize,
                        out var auxShapeData,
                        out var complete), NativeMethods.OK);

                    if (complete == 0)
                        return (null, null, null);

                    for (var i = 0; i < inShapeSize; ++i)
                        inType.Add(DType.GetType(inShapeData[i]));

                    for (var i = 0; i < auxShapeSize; ++i)
                        auxType.Add(DType.GetType(auxShapeData[i]));

                    for (var i = 0; i < outShapeSize; ++i)
                        outType.Add(DType.GetType(outShapeData[i]));
                }
            }

            return (inType.ToArray(), outType.ToArray(), auxType.ToArray());
        }

        public void InferExecutorArrays(Context context,
            NDArrayList argArrays,
            NDArrayList gradArrays,
            IList<OpGradReq> gradReqs,
            NDArrayList auxArrays,
            NDArrayDict argsMap)
        {
            InferExecutorArrays(context,
                argArrays,
                gradArrays,
                gradReqs,
                auxArrays,
                argsMap,
                new NDArrayDict());
        }

        public void InferExecutorArrays(Context context,
            NDArrayList argArrays,
            NDArrayList gradArrays,
            IList<OpGradReq> gradReqs,
            NDArrayList auxArrays,
            NDArrayDict argsMap,
            NDArrayDict argGradStore)
        {
            InferExecutorArrays(context,
                argArrays,
                gradArrays,
                gradReqs,
                auxArrays,
                argsMap,
                argGradStore,
                new Dictionary<string, OpGradReq>());
        }

        public void InferExecutorArrays(Context context,
            NDArrayList argArrays,
            NDArrayList gradArrays,
            IList<OpGradReq> gradReqs,
            NDArrayList auxArrays,
            NDArrayDict argsMap,
            NDArrayDict argGradStore,
            IDictionary<string, OpGradReq> gradReqType)
        {
            InferExecutorArrays(context,
                argArrays,
                gradArrays,
                gradReqs,
                auxArrays,
                argsMap,
                argGradStore,
                gradReqType,
                new NDArrayDict());
        }

        public void InferExecutorArrays(Context context,
            NDArrayList argArrays,
            NDArrayList gradArrays,
            IList<OpGradReq> gradReqs,
            NDArrayList auxArrays,
            NDArrayDict argsMap,
            NDArrayDict argGradStore,
            IDictionary<string, OpGradReq> gradReqType,
            NDArrayDict auxMap)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            if (argArrays == null)
                throw new ArgumentNullException(nameof(argArrays));
            if (gradArrays == null)
                throw new ArgumentNullException(nameof(gradArrays));
            if (gradReqs == null)
                throw new ArgumentNullException(nameof(gradReqs));
            if (auxArrays == null)
                throw new ArgumentNullException(nameof(auxArrays));
            if (argsMap == null)
                throw new ArgumentNullException(nameof(argsMap));
            if (argGradStore == null)
                throw new ArgumentNullException(nameof(argGradStore));
            if (gradReqType == null)
                throw new ArgumentNullException(nameof(gradReqType));
            if (auxMap == null)
                throw new ArgumentNullException(nameof(auxMap));

            ThrowIfDisposed();

            var argNameList = ListArguments();
            var argShapes = new Dictionary<string, Shape>();

            foreach (var argName in argNameList)
                if (argsMap[argName] != null)
                    argShapes[argName] = argsMap[argName].shape;

            var (inShapes, auxShapes, outShapes) = InferShape(argShapes);

            for (var i = 0; i < inShapes.Length; ++i)
            {
                var shape = inShapes[i];
                var argName = argNameList[i];
                if (argsMap[argName] != null)
                {
                    argArrays.Add(argsMap[argName]);
                }
                else
                {
                    argArrays.Add(new NDArray(shape, false));
                    //NDArray.SampleGaussian(0, 1, argArrays.Last());
                    var argArr = argArrays.Last();
                    nd.Random.Uniform(0, 1, argArr.shape).CopyTo(argArr);
                }

                if (argGradStore[argName] != null)
                    gradArrays.Add(argGradStore[argName]);
                else
                    gradArrays.Add(new NDArray(shape, false));

                if (gradReqType.TryGetValue(argName, out var value3))
                    gradReqs.Add(value3);
                else if (argName.LastIndexOf("data", StringComparison.InvariantCulture) == argName.Length - 4 ||
                         argName.LastIndexOf("label", StringComparison.InvariantCulture) == argName.Length - 5)
                    gradReqs.Add(OpGradReq.Null);
                else
                    gradReqs.Add(OpGradReq.Write);
            }

            var auxNameList = ListAuxiliaryStates();
            for (var i = 0; i < auxShapes.Length; ++i)
            {
                var shape = auxShapes[i];
                var auxName = auxNameList[i];
                if (auxMap[auxName] != null)
                {
                    auxArrays.Add(auxMap[auxName]);
                }
                else
                {
                    auxArrays.Add(new NDArray(shape, false));
                    var aux = auxArrays.Last();
                    //NDArray.SampleGaussian(0, 1, auxArrays.Last());
                    nd.Random.Uniform(0, 1, aux.shape).CopyTo(aux);
                }
            }
        }

        public IList<string> ListArguments()
        {
            ThrowIfDisposed();

            Logging.CHECK_EQ(NativeMethods.MXSymbolListArguments(GetHandle(), out var size, out var sarry), NativeMethods.OK);
            var sarryArray = InteropHelper.ToPointerArray(sarry, size);

            var ret = new string[size];
            for (var i = 0; i < size; i++)
                ret[i] = Marshal.PtrToStringAnsi(sarryArray[i]);

            return ret;
        }

        public Dictionary<string, Dictionary<string, string>> ListAttributeDict()
        {
            ThrowIfDisposed();

            Logging.CHECK_EQ(NativeMethods.MXSymbolListAuxiliaryStates(GetHandle(), out var size, out var sarry), NativeMethods.OK);
            var sarryArray = InteropHelper.ToPointerArray(sarry, size);

            Dictionary<string, Dictionary<string, string>> ret = new Dictionary<string, Dictionary<string, string>>();
            for (var i = 0; i < size; i++)
            {
                string[] pair = Marshal.PtrToStringAnsi(sarryArray[i * 2]).Split('$');
                string name = pair[0];
                string key = pair[1];
                string val = Marshal.PtrToStringAnsi(sarryArray[i * 2 + 1]);
                if (!ret.ContainsKey(name))
                    ret.Add(name, new Dictionary<string, string>());

                ret[name][key] = val;
            }

            return ret;
        }

        public IList<string> ListAuxiliaryStates()
        {
            ThrowIfDisposed();

            Logging.CHECK_EQ(NativeMethods.MXSymbolListAuxiliaryStates(GetHandle(), out var size, out var sarry), NativeMethods.OK);
            var sarryArray = InteropHelper.ToPointerArray(sarry, size);

            var ret = new string[size];
            for (var i = 0; i < size; i++)
                ret[i] = Marshal.PtrToStringAnsi(sarryArray[i]);

            return ret;
        }

        public IList<string> ListInputs()
        {
            ThrowIfDisposed();

            Logging.CHECK_EQ(NativeMethods.NNSymbolListInputNames(GetHandle(), 0, out var size, out var sarry), NativeMethods.OK);
            var sarryArray = InteropHelper.ToPointerArray(sarry, size);
            var ret = new string[size];
            for (var i = 0; i < size; i++)
                ret[i] = Marshal.PtrToStringAnsi(sarryArray[i]);

            return ret;
        }

        public IList<string> ListOutputs()
        {
            ThrowIfDisposed();

            Logging.CHECK_EQ(NativeMethods.MXSymbolListOutputs(GetHandle(), out var size, out var sarry), NativeMethods.OK);
            var sarryArray = InteropHelper.ToPointerArray(sarry, size);
            var ret = new string[size];
            for (var i = 0; i < size; i++)
                ret[i] = Marshal.PtrToStringAnsi(sarryArray[i]);

            return ret;
        }

        public static _Symbol Load(string fileName)
        {
            Logging.CHECK_EQ(NativeMethods.MXSymbolCreateFromFile(fileName, out var handle), NativeMethods.OK);
            return new _Symbol(handle);
        }

        public static _Symbol FromJSON(string json)
        {
            Logging.CHECK_EQ(NativeMethods.MXSymbolCreateFromJSON(json, out var handle), NativeMethods.OK);
            return new _Symbol(handle);
        }

        public void Save(string fileName, bool remove_amp_cast = true)
        {
            if (remove_amp_cast)
            {
                Logging.CHECK_EQ(NativeMethods.MXSymbolRemoveAmpCast(GetHandle(), out var h), NativeMethods.OK);
                Logging.CHECK_EQ(NativeMethods.MXSymbolSaveToFile(h, fileName), NativeMethods.OK);
            }
            else
            {
                Logging.CHECK_EQ(NativeMethods.MXSymbolSaveToFile(GetHandle(), fileName), NativeMethods.OK);
            }
        }

        public _Symbol ShallowCopy()
        {
            return (_Symbol)MemberwiseClone();
        }

        public _Symbol Compose(SymbolDict kwargs, string name = "")
        {
            if (kwargs == null)
                throw new ArgumentNullException("kwargs");

            int num_args = kwargs.Count;
            Logging.CHECK_EQ(NativeMethods.MXSymbolCompose(NativePtr, name, num_args, kwargs.Keys.ToArray(), kwargs.Values.Select(x => x.NativePtr).ToArray()), NativeMethods.OK);
            return this;
        }

        public string ToJSON(bool remove_amp_cast = true)
        {
            if (remove_amp_cast)
            {
                Logging.CHECK_EQ(NativeMethods.MXSymbolRemoveAmpCast(this.GetHandle(), out var handle), NativeMethods.OK);
                Logging.CHECK_EQ(NativeMethods.MXSymbolSaveToJSON(handle, out var outJson1), NativeMethods.OK);
                return Marshal.PtrToStringAnsi(outJson1);
            }

            Logging.CHECK_EQ(NativeMethods.MXSymbolSaveToJSON(GetHandle(), out var outJson), NativeMethods.OK);
            return Marshal.PtrToStringAnsi(outJson);
        }

        public static _Symbol Variable(string name)
        {
            return Var(name);
        }

        public static _Symbol Var(string name, Dictionary<string, string> attr = null, Shape shape = null,
            float? lr_mult = null, float? wd_mult = null,
            DType dtype = null, Initializer init = null, StorageStype? stype = null, string profiler_scope = null)
        {
            Logging.CHECK_EQ(NativeMethods.MXSymbolCreateVariable(name, out var handle), NativeMethods.OK);
            var ret = new _Symbol(handle);
            if (attr == null)
                attr = new Dictionary<string, string>();

            if (shape != null)
                attr.Add("__shape__", shape.ToString());

            if (lr_mult.HasValue)
                attr.Add("__lr_mult__", lr_mult.Value.ToString());

            if (wd_mult.HasValue)
                attr.Add("__wd_mult__", wd_mult.Value.ToString());

            if (dtype != null)
                attr.Add("__dtype__", dtype);

            if (init != null)
            {
                var init_string = init.Dumps();
                attr.Add("__init__", init_string);
            }

            if (profiler_scope != null)
            {
                attr["__profiler_scope__"] = profiler_scope;
            }
            else
            {
                attr["__profiler_scope__"] = Profiler._current_scope.Get().ToString();
            }

            if (stype.HasValue)
                attr.Add("__storage_type__", ((int)stype).ToString());

            ret.SetAttr(attr);

            return ret;
        }

        public string Attr(string key)
        {
            Logging.CHECK_EQ(NativeMethods.MXSymbolGetAttr(GetHandle(), key, out var @out, out var success), NativeMethods.OK);
            if (success != 0)
                return @out;

            return null;
        }

        public Dictionary<string, string> ListAttr()
        {
            Logging.CHECK_EQ(NativeMethods.MXSymbolListAttrShallow(GetHandle(), out var out_size, out var pairs), NativeMethods.OK);

            var dict = new Dictionary<string, string>();
            var i = 0;
            while (i < out_size) dict[pairs[i * 2]] = pairs[i * 2 + 1];

            return dict;
        }

        public Dictionary<string, Dictionary<string, string>> AttrDict()
        {
            Logging.CHECK_EQ(NativeMethods.MXSymbolListAttr(GetHandle(), out var out_size, out var sarray), NativeMethods.OK);
            var array = InteropHelper.ToPointerArray(sarray, out_size);
            List<string> pairs = array.Select(x => (Marshal.PtrToStringAnsi(x))).ToList();
            var dict = new Dictionary<string, Dictionary<string, string>>();
            var i = 0;
            while (i < out_size)
            {
                var keys = pairs[i].Split('$');
                if (!dict.ContainsKey(keys[0]))
                    dict[keys[0]] = new Dictionary<string, string>();
                if ((i + 1) != pairs.Count)
                    dict[keys[0]][keys[1]] = pairs[i + 1];
                i = i + 2;
            }

            return dict;
        }

        public void SetAttr(Dictionary<string, string> attrs)
        {
            foreach (var attr in attrs)
            {
                Logging.CHECK_EQ(NativeMethods.MXSymbolSetAttr(GetHandle(), attr.Key, attr.Value), NativeMethods.OK);
            }
        }

        public SymbolList ToList()
        {
            var outputs = ListOutputs();
            SymbolList ret = new SymbolList();
            foreach (var i in outputs)
            {
                ret.Add(this[i]);
            }

            return ret;
        }

        public _Symbol AsNPArray()
        {
            NativeMethods.MXShallowCopySymbol(this.GetHandle(), out SymbolHandle hdl);
            return new _Symbol(hdl);
        }

        // Returns self. For the convenience of conversion between legacy and np _Symbols.
        public _Symbol AsNDArray()
        {
            return this;
        }

        public SymbolList GetInputs()
        {
            NativeMethods.MXSymbolGetInputs(this.GetHandle(), out SymbolHandle handle);
            var sym = new _Symbol(handle);
            return sym.ToList();
        }

        internal (IntPtr[], NDArrayList) GetNDArrayInputs(string arg_key, NDArrayDict args, string[] arg_names, bool allow_missing)
        {
            var arg_handles = new List<IntPtr>();
            var arg_arrays = new NDArrayList();
            foreach (var name in arg_names)
            {
                if (args.Contains(name))
                {
                    var narr = args[name];
                   
                    arg_handles.Add(narr.NativePtr);
                    arg_arrays.Add(narr);
                }
                else if (allow_missing)
                {
                    arg_handles.Add(IntPtr.Zero);
                    arg_arrays.Add(null);
                }
                else
                {
                    throw new Exception(String.Format("key `%s` is missing in `%s`", name, arg_key));
                }
            }

            return (arg_handles.ToArray(), arg_arrays);
        }

        internal (IntPtr[], NDArrayList) GetNDArrayInputs(string arg_key, NDArrayList args, string[] arg_names, bool allow_missing)
        {
            var arg_handles = new List<IntPtr>();
            var arg_arrays = new NDArrayList();
            if (args.Length != arg_names.Length)
                throw new Exception("Length of args does not match the number of arguments");
            foreach (var narr in args)
            {
                arg_handles.Add(narr.NativePtr);
                arg_arrays.Add(narr);
            }

            return (arg_handles.ToArray(), arg_arrays);
        }

        public bool HasDynamicShapeOp()
        {
            NativeMethods.MXCheckDynamicShapeOp(this.NativePtr, out var has_dynamic_shape);
            return has_dynamic_shape;
        }

        public virtual _Symbol OptimizeFor(
                string backend,
                NDArrayDict args = null,
                NDArrayDict aux = null,
                Context ctx = null,
                Dictionary<string, Shape> shape_dict = null,
                Dictionary<string, DType> type_dict = null,
                Dictionary<string, StorageStype> stype_dict = null,
                bool skip_infer = false)
        {
            IntPtr[] aux_handle;
            NDArrayList aux_ = new NDArrayList(); ;
            IntPtr[] args_handle;
            NDArrayList args_;

            if (args == null || args.Count == 0)
            {
                args_ = args.Values.ToArray();
                args_handle = args_.Handles;
            }
            else
            {
                var _tup_1 = this.GetNDArrayInputs("args", args, this.ListArguments().ToArray(), true);
                args_handle = _tup_1.Item1;
                args_ = _tup_1.Item2;
            }

            if (aux == null || aux.Count == 0)
            {
                aux_ = new NDArrayList();
                aux_handle = aux_.Handles;
            }
            else
            {
                var _tup_2 = this.GetNDArrayInputs("aux_states", aux, this.ListAuxiliaryStates().ToArray(), true);
                aux_handle = _tup_2.Item1;
                aux_ = _tup_2.Item2;
            }
            if (ctx == null)
            {
                ctx = Context.CurrentContext;
            }

            unsafe
            {
                var num_input_shapes = 0;
                List<string> input_shape_names = new List<string>();
                List<int[]> input_shape_data = new List<int[]>();
                List<int> input_shape_idx = new List<int>();
                if (shape_dict != null)
                {
                    foreach (var (k, v) in shape_dict)
                    {
                        input_shape_names.Add(k);
                        input_shape_data.Add(v.Data.ToArray());
                        input_shape_idx.Add(input_shape_data.Count);
                    }

                    num_input_shapes = input_shape_names.Count;
                }

                // parse input data types dict
                var num_input_types = 0;
                var input_type_names = new List<string>();
                var input_type_data = new List<string>();
                
                if (type_dict != null)
                {
                    foreach (var (k, v) in type_dict)
                    {
                        input_type_names.Add(k);
                        input_type_data.Add(v.Name);
                    }
                }

                // parse input data storage types dict
                var num_input_stypes = 0;
                // provided storage type argument names
                var input_stype_names = new List<string>();
                var input_stype_data = new List<int>();
                if (stype_dict != null)
                {
                    foreach (var (k, v) in stype_dict)
                    {
                        input_stype_names.Add(k);
                        input_stype_data.Add((int)v);
                    }
                }

                int new_args_size = 0;
                var new_arg_names = new List<string>();
                var new_args_handle = new List<IntPtr>();
                int new_aux_size = 0;
                var new_aux_names = new List<string>();
                var new_aux_handle = new IntPtr();
                var key_list = new List<object>();
                var val_list = new List<object>();
                //foreach (var _tup_6 in kwargs.items())
                //{
                //    var key = _tup_6.Item1;
                //    var val = _tup_6.Item2;
                //    key_list.append(key);
                //    val_list.append(val.ToString());
                //}

                //check_call(_LIB.MXOptimizeForBackend(this.handle, c_str(backend), ctypes.c_int(ctx.device_typeid), ctypes.byref(@out), mx_uint(args_.Count), args_handle, mx_uint(aux_.Count), aux_handle, mx_uint(key_list.Count), c_str_array(key_list), c_str_array(val_list), num_input_shapes, input_shape_names, input_shape_data, input_shape_idx, num_input_types, input_type_names, input_type_data, num_input_stypes, input_stype_names, input_stype_data, ctypes.c_bool(skip_infer), ctypes.byref(new_args_size), ctypes.byref(new_args_handle), ctypes.byref(new_arg_names), ctypes.byref(new_aux_size), ctypes.byref(new_aux_handle), ctypes.byref(new_aux_names)));
            }
            //// parse input data shape dict

            //// add new args/aux
            //if (!(args == null))
            //{
            //    foreach (var i in Enumerable.Range(0, new_args_size.value))
            //    {
            //        args[py_str(new_arg_names[i])] = NDArray(NDArrayHandle(new_args_handle[i]));
            //    }
            //}
            //else if (new_args_size.value > 0)
            //{
            //    throw new RuntimeError("Cannot add new args in optimize_for since args is None\n" + "Provide a dictionary to the args argument to optimize_for");
            //}
            //if (!(aux == null))
            //{
            //    foreach (var i in Enumerable.Range(0, new_aux_size.value))
            //    {
            //        aux[py_str(new_aux_names[i])] = NDArray(NDArrayHandle(new_aux_handle[i]));
            //    }
            //}
            //else if (new_aux_size.value > 0)
            //{
            //    throw new RuntimeError("Cannot add new aux in optimize_for since aux is None\n" + "Provide a dictionary to the aux argument to optimize_for");
            //}
            //var new_sym = new _Symbol(@out);
            //var arg_names = this.list_arguments();
            //new_arg_names = new_sym.list_arguments();
            //var deleted_arg_names = new HashSet<object>((from item in arg_names
            //                                             where !new HashSet<object>(new_arg_names).Contains(item)
            //                                             select item).ToList());
            //if (deleted_arg_names.Count > 0)
            //{
            //    if (args != null)
            //    {
            //        foreach (var a_n in deleted_arg_names)
            //        {
            //            if (args.Contains(a_n))
            //            {
            //                args.pop(a_n);
            //            }
            //        }
            //    }
            //    else
            //    {
            //        warnings.warn("A param was deleted during optimization, but no args dictionary was provided.\n" + "Please ensure that your model weights match the newly optimized model.");
            //    }
            //}
            //var aux_names = this.list_auxiliary_states();
            //new_aux_names = new_sym.list_auxiliary_states();
            //var deleted_aux_names = new HashSet<object>((from item in aux_names
            //                                             where !new HashSet<object>(new_aux_names).Contains(item)
            //                                             select item).ToList());
            //if (deleted_aux_names.Count > 0)
            //{
            //    if (aux != null)
            //    {
            //        foreach (var a_n in deleted_aux_names)
            //        {
            //            if (aux.Contains(a_n))
            //            {
            //                aux.pop(a_n);
            //            }
            //        }
            //    }
            //    else
            //    {
            //        warnings.warn("A param was deleted during optimization, but no args dictionary was provided.\n" + "Please ensure that your model weights match the newly optimized model.");
            //    }
            //}
            //return new_sym;

            throw new NotImplementedException();
        }

        #region Overrides

        #region Operators

        public static _Symbol operator +(_Symbol lhs, _Symbol rhs)
        {
            if (lhs == null)
                throw new ArgumentNullException(nameof(lhs));
            if (rhs == null)
                throw new ArgumentNullException(nameof(rhs));

            lhs.ThrowIfDisposed();
            rhs.ThrowIfDisposed();

            return sym_np_ops.add(lhs, rhs);
        }

        public static _Symbol operator -(_Symbol lhs, _Symbol rhs)
        {
            if (lhs == null)
                throw new ArgumentNullException(nameof(lhs));
            if (rhs == null)
                throw new ArgumentNullException(nameof(rhs));

            lhs.ThrowIfDisposed();
            rhs.ThrowIfDisposed();

            return sym_np_ops.subtract(lhs, rhs);
        }

        public static _Symbol operator *(_Symbol lhs, _Symbol rhs)
        {
            if (lhs == null)
                throw new ArgumentNullException(nameof(lhs));
            if (rhs == null)
                throw new ArgumentNullException(nameof(rhs));

            lhs.ThrowIfDisposed();
            rhs.ThrowIfDisposed();

            return sym_np_ops.multiply(lhs, rhs);
        }

        public static _Symbol operator /(_Symbol lhs, _Symbol rhs)
        {
            if (lhs == null)
                throw new ArgumentNullException(nameof(lhs));
            if (rhs == null)
                throw new ArgumentNullException(nameof(rhs));

            lhs.ThrowIfDisposed();
            rhs.ThrowIfDisposed();

            return sym_np_ops.true_divide(lhs, rhs);
        }

        public static _Symbol operator %(_Symbol lhs, _Symbol rhs)
        {
            if (lhs == null)
                throw new ArgumentNullException(nameof(lhs));
            if (rhs == null)
                throw new ArgumentNullException(nameof(rhs));

            lhs.ThrowIfDisposed();
            rhs.ThrowIfDisposed();

            return sym_np_ops.mod(lhs, rhs);
        }

        public static _Symbol operator +(_Symbol lhs, float scalar)
        {
            if (lhs == null)
                throw new ArgumentNullException(nameof(lhs));

            lhs.ThrowIfDisposed();

            return sym_np_ops.add(lhs, sym_np_ops.full_like(lhs, scalar));
        }

        public static _Symbol operator +(float lhs, _Symbol rhs)
        {
            return rhs + lhs;
        }

        public static _Symbol operator -(_Symbol lhs, float scalar)
        {
            if (lhs == null)
                throw new ArgumentNullException(nameof(lhs));

            lhs.ThrowIfDisposed();

            return sym_np_ops.subtract(lhs, scalar);
        }

        public static _Symbol operator -(float lhs, _Symbol rhs)
        {
            if (rhs == null)
                throw new ArgumentNullException(nameof(rhs));

            rhs.ThrowIfDisposed();
            
            return sym_np_ops.subtract(lhs, rhs);
        }

        public static _Symbol operator *(_Symbol lhs, float scalar)
        {
            if (lhs == null)
                throw new ArgumentNullException(nameof(lhs));

            lhs.ThrowIfDisposed();

            return sym_np_ops.multiply(lhs, scalar);
        }

        public static _Symbol operator *(float lhs, _Symbol rhs)
        {
            return sym_np_ops.multiply(lhs, rhs);
        }

        public static _Symbol operator /(_Symbol lhs, float scalar)
        {
            if (lhs == null)
                throw new ArgumentNullException(nameof(lhs));

            lhs.ThrowIfDisposed();

            return sym_np_ops.true_divide(lhs, scalar);
        }

        public static _Symbol operator /(float lhs, _Symbol rhs)
        {
            if (rhs == null)
                throw new ArgumentNullException(nameof(rhs));

            rhs.ThrowIfDisposed();

            return sym_np_ops.true_divide(lhs, rhs);
        }

        public static _Symbol operator %(_Symbol lhs, float scalar)
        {
            if (lhs == null)
                throw new ArgumentNullException(nameof(lhs));

            lhs.ThrowIfDisposed();

            return sym_np_ops.mod(lhs, scalar);
        }

        public static _Symbol operator %(float lhs, _Symbol rhs)
        {
            if (rhs == null)
                throw new ArgumentNullException(nameof(rhs));

            rhs.ThrowIfDisposed();

            return sym_np_ops.mod(lhs, rhs);
        }

        public static _Symbol operator >(_Symbol lhs, _Symbol rhs)
        {
            return sym_np_ops.greater(lhs, rhs);
        }

        public static _Symbol operator >=(_Symbol lhs, _Symbol rhs)
        {
            return sym_np_ops.greater_equal(lhs, rhs);
        }

        public static _Symbol operator >(_Symbol lhs, float rhs)
        {
            return sym_np_ops.greater(lhs, rhs);
        }

        public static _Symbol operator >=(_Symbol lhs, float rhs)
        {
            return sym_np_ops.greater_equal(lhs, rhs);
        }

        public static _Symbol operator >(float lhs, _Symbol rhs)
        {
            return sym_np_ops.greater(rhs, lhs);
        }

        public static _Symbol operator >=(float lhs, _Symbol rhs)
        {
            return sym_np_ops.greater_equal(rhs, lhs);
        }

        public static _Symbol operator <(_Symbol lhs, _Symbol rhs)
        {
            return sym_np_ops.less(lhs, rhs);
        }

        public static _Symbol operator <=(_Symbol lhs, _Symbol rhs)
        {
            return sym_np_ops.less_equal(lhs, rhs);
        }

        public static _Symbol operator <(_Symbol lhs, float rhs)
        {
            return sym_np_ops.less(lhs, rhs);
        }

        public static _Symbol operator <=(_Symbol lhs, float rhs)
        {
            return sym_np_ops.less_equal(lhs, rhs);
        }

        public static _Symbol operator <(float lhs, _Symbol rhs)
        {
            return sym_np_ops.less(rhs, lhs);
        }

        public static _Symbol operator <=(float lhs, _Symbol rhs)
        {
            return sym_np_ops.less_equal(rhs, lhs);
        }

        #endregion

        #endregion

        #region Overrides

        protected override void DisposeUnmanaged()
        {
            base.DisposeUnmanaged();
            NativeMethods.MXSymbolFree(NativePtr);
        }

        #endregion

        #endregion

        public IEnumerator<_Symbol> GetEnumerator()
        {
            return ListOutputs().Select(x => this[x]).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public static implicit operator _Symbol(Symbol x) => new _Symbol(x.NativePtr);
    }
}

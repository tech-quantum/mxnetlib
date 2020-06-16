﻿using MxNet.Keras.Constraints;
using MxNet.Keras.Initializers;
using MxNet.Keras.Regularizers;
using MxNet.Keras.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using K = MxNet.Keras.MxNetBackend;

namespace MxNet.Keras.Engine
{
    public abstract class Layer
    {
        internal bool _built;
        internal List<Node> _inbound_nodes;
        internal List<KerasSymbol> _initial_weights;
        internal List<Func<KerasSymbol, KerasSymbol, KerasSymbol>> _losses;
        internal List<KerasSymbol> _non_trainable_weights;
        internal List<Node> _outbound_nodes;
        internal Dictionary<object, object> _per_input_losses;
        internal Dictionary<object, object> _per_input_updates;
        internal List<KerasSymbol> _trainable_weights;
        internal List<KerasSymbol> _updates;
        internal Shape batch_input_shape;
        internal DType dtype;
        internal InputSpec[] input_spec;
        internal string name;
        internal bool stateful;
        internal bool supports_masking;
        internal bool trainable;
        internal Regularizer activity_regularizer;

        public KerasSymbol Input
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public KerasSymbol Output
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public KerasSymbol InputMask
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public KerasSymbol OutputMask
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public Shape InputShape
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public Shape OutputShape
        {
            get
            {
                throw new NotImplementedException();
            }
        }
        
        public NDArray[] Weights
        {
            get
            {
                throw new NotImplementedException();
            }
        }


        public Layer(Shape input_shape = null, Shape batch_input_shape = null, int? batch_size = null, DType dtype = null, string name = null, bool? trainable = null, KerasSymbol[] weights = null)
        {
            this.input_spec = null;
            this.supports_masking = false;
            this.stateful = false;
            // These properties will be set upon call of self.build()
            this._trainable_weights = new List<KerasSymbol>();
            this._non_trainable_weights = new List<KerasSymbol>();
            this._losses = new List<Func<KerasSymbol, KerasSymbol, KerasSymbol>>();
            this._updates = new List<KerasSymbol>();
            this._per_input_losses = new Dictionary<object, object>
            {
            };
            this._per_input_updates = new Dictionary<object, object>
            {
            };
            this._built = false;
            // These lists will be filled via successive calls
            // to self._add_inbound_node().
            this._inbound_nodes = new List<Node>();
            this._outbound_nodes = new List<Node>();
           
            if (!string.IsNullOrWhiteSpace(name))
            {
                var prefix = this.GetType().Name;
                name = prefix + "_" + K.GetUid(prefix).ToString();
            }

            this.name = name;
            this.trainable = trainable.HasValue ? trainable.Value : false;
            if (input_shape != null || batch_input_shape != null)
            {
                // In this case we will later create an input layer
                // to insert before the current layer
                if (batch_input_shape!= null)
                {
                    this.batch_input_shape = batch_input_shape;
                }
                else if (input_shape != null)
                {
                    var shapeTuple = input_shape.Data.ToList();
                    if (batch_size.HasValue)
                    {
                        shapeTuple.Insert(0, batch_size.Value);
                    }
                    else
                    {
                        shapeTuple.Insert(0, 0);
                    }

                    batch_input_shape = new Shape(shapeTuple);
                }

                this.batch_input_shape = batch_input_shape;
                // Set dtype.
                if (dtype == null)
                {
                    dtype = K.FloatX();
                }
                
                this.dtype = dtype;
            }
            if (weights != null)
            {
                this._initial_weights = weights.ToList();
            }
            else
            {
                this._initial_weights = null;
            }
        }

        public static string NodeKey(Layer layer, int node_index)
        {
            return layer.name + "_ib-" + node_index.ToString();
        }

        public Func<KerasSymbol, KerasSymbol, KerasSymbol>[] Losses => _losses.ToArray();

        public KerasSymbol[] Updates
        {
            get
            {
                if (!this.trainable && !this.stateful)
                {
                    return new KerasSymbol[0];
                }

                return this._updates.ToArray();
            }
        }

        public bool Built
        {
            get
            {
                return this._built;
            }
            set
            {
                this._built = value;
            }
        }

        public KerasSymbol[] TrainableWeights
        {
            get
            {
                if (trainable)
                {
                    return this._trainable_weights.ToArray();
                }
                else
                {
                    return new KerasSymbol[0];
                }
            }
            set
            {
                this._trainable_weights = value.ToList();
            }
        }

        public KerasSymbol[] NonTrainableWeights
        {
            get
            {
                if (!trainable)
                {
                    List<KerasSymbol> ret = new List<KerasSymbol>();
                    ret.AddRange(_trainable_weights);
                    ret.AddRange(_non_trainable_weights);
                    return ret.ToArray();
                }
                else
                {
                    return _non_trainable_weights.ToArray();
                }
            }
            set
            {
                this._trainable_weights = value.ToList();
            }
        }

        public virtual KerasSymbol AddWeight(string name, Shape shape, DType dtype= null, Initializer initializer= null, Regularizer regularizer = null, bool trainable= true, Constraint constraint= null, bool sparse_weight= false)
        {
            KerasSymbol weight = null;
            
            if (dtype == null)
            {
                dtype = K.FloatX();
            }
            // Use sparse weight only with MXNet Backend
            weight = K.Variable(initializer.Call(shape).Tensor, dtype: dtype, name: name, constraint: constraint, sparse_weight: sparse_weight);

            if (regularizer != null)
            {
                using (var ns = new NameScope("weight_regularizer")) {
                    this.AddLoss(new KerasSymbol[] { regularizer.Call(weight) });
                }
            }
            if (trainable)
            {
                this._trainable_weights.Add(weight);
            }
            else
            {
                this._non_trainable_weights.Add(weight);
            }
            return weight;
        }

        public void AssertInputCompatibility(KerasSymbol[] inputs)
        {
            Shape x_shape;
            int ndim;
            foreach (var x in inputs)
            {
                try
                {
                    K.IsKerasTensor(x);
                }
                catch (Exception ex)
                {
                    throw new Exception("Layer " + this.name + " was called with an input that isn\'t a symbolic tensor. Received type: " + x.GetType().Name + ". Full input: " + inputs.ToString() + ". All inputs to the layer should be tensors.");
                }
            }
            if (this.input_spec == null)
            {
                return;
            }

            if (inputs.Length != input_spec.Length)
            {
                throw new Exception("Layer " + this.name + " expects " + input_spec.Length.ToString() + " inputs, but it received " + inputs.Length.ToString() + " input tensors. Input received: " + inputs.ToString());
            }

            for (int i = 0; i < inputs.Length; i++)
            {
                var input_index = i;
                var x = inputs[i];
                var spec = input_spec[i];
                if (spec.ndim != 0)
                {
                    if (K.NDim(x) != spec.ndim)
                    {
                        throw new Exception("Input " + input_index.ToString() + " is incompatible with layer " + this.name + ": expected ndim=" + spec.ndim.ToString() + ", found ndim=" + K.NDim(x).ToString());
                    }
                }
                if (spec.max_ndim != null)
                {
                    ndim = K.NDim(x);
                    if (ndim != 0 && ndim > spec.max_ndim)
                    {
                        throw new Exception("Input " + input_index.ToString() + " is incompatible with layer " + this.name + ": expected max_ndim=" + spec.max_ndim.ToString() + ", found ndim=" + K.NDim(x).ToString());
                    }
                }

                if (spec.min_ndim != null)
                {
                    ndim = K.NDim(x);
                    if (ndim != 0 && ndim < spec.min_ndim)
                    {
                        throw new Exception("Input " + input_index.ToString() + " is incompatible with layer " + this.name + ": expected min_ndim=" + spec.min_ndim.ToString() + ", found ndim=" + K.NDim(x).ToString());
                    }
                }
                // Check dtype.
                if (spec.dtype != null)
                {
                    if (K.DataType(x).Name != spec.dtype.Name)
                    {
                        throw new Exception("Input " + input_index.ToString() + " is incompatible with layer " + this.name + ": expected dtype=" + spec.dtype.ToString() + ", found dtype=" + K.DataType(x).ToString());
                    }
                }

                if (spec.axes != null)
                {
                    try
                    {
                        x_shape = new Shape(K.IntShape(x));
                    }
                    catch
                    {
                        x_shape = null;
                    }
                    if (x_shape != null)
                    {
                        foreach (var _tup_2 in spec.axes)
                        {
                            var axis = _tup_2.Key;
                            var value = _tup_2.Value;
                            if(value != x_shape[axis])
                            {
                                throw new Exception("Input " + input_index +
                                " is incompatible with layer " +
                                name + ": expected axis " +
                                axis + " of input shape to have " +
                                "value " + value +
                                " but got shape " + x_shape);
                            }
                        }
                    }
                }

                if (spec.shape != null)
                {
                    try
                    {
                        x_shape = new Shape(K.IntShape(x));
                    }
                    catch
                    {
                        x_shape = null;
                    }

                    if (x_shape != null)
                    {
                        for(int dim_i = 0; dim_i < spec.shape.Dimension; dim_i++)
                        {
                            var spec_dim = spec.shape[dim_i];
                            var dim = x_shape[dim_i];
                            if (spec_dim != dim)
                            {
                                throw new Exception("Input " + input_index.ToString() + " is incompatible with layer " + this.name + ": expected shape=" + spec.shape.ToString() + ", found shape=" + x_shape.ToString());
                            }
                        }
                    }
                }
            }
        }

        public abstract KerasSymbol[] Call(KerasSymbol[] inputs, FuncArgs kwargs);

        internal KerasSymbol[] _Call(KerasSymbol[] inputs, FuncArgs kwargs)
        {
            KerasSymbol[] output = null;
            using (var ns = new NameScope(this.name)) {
                // Handle laying building (weight creating, input spec locking).
                if (!this.Built)
                {
                    // Raise exceptions in case the input is not compatible
                    // with the input_spec specified in the layer constructor.
                    this.AssertInputCompatibility(inputs);
                    // Collect input shapes to build layer.
                    var input_shapes = new List<Shape>();
                    foreach (var x_elem in inputs)
                    {
                        if (x_elem._keras_shape != null)
                        {
                            input_shapes.Add(x_elem._keras_shape);
                        }
                        else
                        {
                            input_shapes.Add(x_elem.Shape);
                        }
                    }


                    this.Build(input_shapes[0]);
                    this.Built = true;
                    // Load weights that were specified at layer instantiation.
                    if (this._initial_weights != null)
                    {
                        this.SetWeights(this._initial_weights.ToArray());
                    }
                }
                // Raise exceptions in case the input is not compatible
                // with the input_spec set at build time.
                this.AssertInputCompatibility(inputs);
                // Handle mask propagation.
                var previous_mask = BaseLayers.CollectPreviousMask(inputs);
                var user_kwargs = kwargs;
                if (previous_mask.Where(x=>x == null).Count() == 0)
                {
                    // The previous layer generated a mask.
                    if (!kwargs.Contains("mask"))
                    {
                        // If mask is explicitly passed to __call__,
                        // we should override the default mask.
                        kwargs["mask"] = previous_mask;
                    }
                }

                // Handle automatic shape inference (only useful for Theano).
                var input_shape = BaseLayers.CollectInputShape(inputs);
                // Actually call the layer,
                // collecting output(s), mask(s), and shape(s).
                output = this.Call(inputs, kwargs);
                var output_mask = this.ComputeMask(inputs, previous_mask);
                // If the layer returns tensors from its inputs, unmodified,
                // we copy them to avoid loss of tensor metadata.
                var output_ls = output;
                var inputs_ls = inputs;
                var output_ls_copy = new List<KerasSymbol>();
                for(int index = 0; index < output_ls.Length; index++)
                {
                    var x = output_ls[index];
                    if (inputs_ls.FirstOrDefault(i => i.Name == x.Name) != null)
                    {
                        x = K.Identity(x);
                    }

                    output_ls_copy.Add(x);
                }

                output = new KerasSymbol[] { output_ls_copy[0] };
                Shape[] output_shape = null;
                // Inferring the output shape is only relevant for Theano.
                if (input_shape.Where(x=>x != null).Count() > 0)
                {
                    output_shape = this.ComputeOutputShape(input_shape);
                }
                else
                {
                    output_shape = null;
                }

                if (output_ls.Length == 1)
                {
                    // Augment the mask to match the length of the output.
                    List<KerasSymbol> omList = new List<KerasSymbol>();
                    for(int i = 0; i< output_ls.Length; i++)
                    {
                        omList.Add(output_mask[0]);
                    }

                    output_mask = omList.ToArray();
                }
                // Add an inbound node to the layer, so that it keeps track
                // of the call and of all new variables created during the call.
                // This also updates the layer history of the output tensor(s).
                // If the input tensor(s) had not previous Keras history,
                // this does nothing.
                this.AddInboundNode(input_tensors: inputs, output_tensors: output, input_masks: previous_mask, output_masks: output_mask, input_shapes: input_shape, output_shapes: output_shape, arguments: user_kwargs);
                // Apply activity regularizer if any:
                
                if (this.activity_regularizer != null)
                {
                    KerasSymbol[] regularization_losses = null;
                    using (var ns1 = new NameScope("activity_regularizer")) 
                    {
                        regularization_losses = (from x in output
                                                 select this.activity_regularizer.Call(x)).ToArray();
                    }

                    this.AddLoss(regularization_losses, inputs: inputs);
                }
            }

            return output;
        }

        internal void AddInboundNode(KerasSymbol[] input_tensors, KerasSymbol[] output_tensors,
                                        KerasSymbol[]  input_masks, KerasSymbol[]  output_masks,
                                        Shape[] input_shapes, Shape[] output_shapes, FuncArgs arguments= null)
        {
            throw new NotImplementedException();
        }

        public virtual Shape[] ComputeOutputShape(Shape[] input_shape)
        {
            return input_shape;
        }

        public virtual KerasSymbol[] ComputeMask(KerasSymbol[] inputs, KerasSymbol[] mask = null)
        {
            throw new NotImplementedException();
        }

        public virtual void Build(Shape input_shape)
        {
            Built = true;
        }

        public virtual Node GetNodeAttributeAtIndex(int node_index, string attr, string attr_name)
        {
            throw new NotImplementedException();
        }

        public virtual Shape GetInputShapeAt(int node_index)
        {
            throw new NotImplementedException();
        }

        public virtual Shape GetOutputShapeAt(int node_index)
        {
            throw new NotImplementedException();
        }

        public virtual KerasSymbol GetInputAt(int node_index)
        {
            throw new NotImplementedException();
        }

        public virtual KerasSymbol GetOutputAt(int node_index)
        {
            throw new NotImplementedException();
        }

        public virtual KerasSymbol GetInputMaskAt(int node_index)
        {
            throw new NotImplementedException();
        }

        public virtual KerasSymbol GetOutputMaskAt(int node_index)
        {
            throw new NotImplementedException();
        }

        public virtual void AddLoss(KerasSymbol[] losses, KerasSymbol[]  inputs = null)
        {
            throw new NotImplementedException();
        }

        public virtual void AddUpdate(KerasSymbol[] updates, KerasSymbol[] inputs = null)
        {
            throw new NotImplementedException();
        }

        public virtual KerasSymbol[] GetUpdatesFor(KerasSymbol[] inputs)
        {
            throw new NotImplementedException();
        }

        public virtual KerasSymbol[] GetLossesFor(KerasSymbol[] inputs)
        {
            throw new NotImplementedException();
        }

        public virtual void SetWeights(KerasSymbol[] weights)
        {
            throw new NotImplementedException();
        }

        public virtual NDArray[] GetWeights()
        {
            throw new NotImplementedException();
        }

        public virtual ConfigDict GetConfig()
        {
            throw new NotImplementedException();
        }

        public static Layer FromConfig(string cls, ConfigDict config, CustomObjects custom_objects = null)
        {
            throw new NotImplementedException();
        }

        public virtual int CountParams()
        {
            throw new NotImplementedException();
        }
    }
}
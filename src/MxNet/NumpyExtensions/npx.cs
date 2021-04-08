﻿using MxNet.Sym.Numpy;
using System;
using System.Collections.Generic;
using System.Text;

namespace MxNet.Numpy
{
    public partial class npx
    {
        public static void set_np(bool shape= true,bool array= true, bool dtype= false)
        {
            throw new NotImplementedException();
        }

        public static void reset_np()
        {
            throw new NotImplementedException();
        }

        public static Context cpu(int device_id)
        {
            throw new NotImplementedException();
        }

        public static Context cpu_pinned(int device_id)
        {
            throw new NotImplementedException();
        }

        public static Context gpu(int device_id)
        {
            throw new NotImplementedException();
        }

        public static (int, int) gpu_memory_info(int device_id)
        {
            throw new NotImplementedException();
        }

        public static Context current_context()
        {
            throw new NotImplementedException();
        }

        public static int num_gpus()
        {
            throw new NotImplementedException();
        }

        public static ndarray relu(ndarray data)
        {
            return activation(data);
        }

        public static ndarray activation(ndarray data, string act_type = "relu")
        {
            throw new NotImplementedException();
        }

        public static ndarray batch_norm(ndarray x, ndarray gamma, ndarray beta, ndarray running_mean, 
                                        ndarray running_var, float eps= 0.001f, float momentum= 0.9f, bool fix_gamma= true, 
                                        bool use_global_stats= false, bool output_mean_var= false, int axis= 1, bool cudnn_off= false,
                                        float? min_calib_range= null, float? max_calib_range= null)
        {
            throw new NotImplementedException();
        }

        public static ndarray convolution(ndarray data, ndarray weight, ndarray bias = null, int[] kernel= null, 
                                        int[] stride= null, int[] dilate= null, int[] pad= null, int num_filter= 1, int num_group= 1, 
                                        int workspace= 1024, bool no_bias= false, string cudnn_tune= null, bool cudnn_off= false, string layout= null)
        {
            throw new NotImplementedException();
        }

        public static ndarray dropout(ndarray data, float p= 0.5f, string mode= "training", Shape axes= null, bool cudnn_off= true)
        {
            throw new NotImplementedException();
        }

        public static ndarray embedding(ndarray data, ndarray weight, int input_dim, int output_dim, DType dtype= null, bool sparse_grad= false)
        {
            throw new NotImplementedException();
        }

        public static ndarray fully_connected(ndarray x, ndarray weight, ndarray bias, int num_hidden, bool no_bias= true, bool flatten= true)
        {
            throw new NotImplementedException();
        }

        public static ndarray layer_norm(ndarray data, ndarray gamma, ndarray beta, int axis= -1, float eps= 9.99999975e-06f, bool output_mean_var= false)
        {
            throw new NotImplementedException();
        }

        public static ndarray pooling(ndarray data, int[] kernel, int[] stride = null, int[] pad = null, string pool_type = "max",
                                    string pooling_convention = "valid", bool global_pool = false, bool cudnn_off = false,
                                    int? p_value = null, int? count_include_pad = null, string layout = null)
        {
            throw new NotImplementedException();
        }

        public static ndarray rnn(ndarray data, ndarray parameters, ndarray state, ndarray state_cell= null, ndarray sequence_length= null, 
                                string mode= null, int? state_size= null, int? num_layers= null, bool bidirectional= false, 
                                bool state_outputs= false, float p= 0, bool use_sequence_length= false, int? projection_size= null,
                                double? lstm_state_clip_min= null, double? lstm_state_clip_max= null, double? lstm_state_clip_nan= null)
        {
            throw new NotImplementedException();
        }

        public static ndarray leaky_relu(ndarray data, ndarray gamma= null, string act_type= "leaky", float slope= 0.25f, float lower_bound= 0.125f, float upper_bound= 0.333999991f)
        {
            throw new NotImplementedException();
        }

        public static ndarray multibox_detection(ndarray cls_prob, ndarray loc_pred, ndarray anchor, bool clip= false,
                                                float threshold= 0.00999999978f, int background_id= 0, float nms_threshold= 0.5f, 
                                                bool force_suppress= false, float[] variances= null, int nms_topk= -1)
        {
            throw new NotImplementedException();
        }

        public static ndarray multibox_prior(ndarray data, float[] sizes = null, float[] ratios = null, bool clip = false, float[] steps = null, float[] offsets = null)
        {
            throw new NotImplementedException();
        }

        public static ndarray multibox_target(ndarray anchor, ndarray label, ndarray cls_pred, float overlap_threshold = 0.5f,
                                            float ignore_label = -1, float negative_mining_ratio = -1, float negative_mining_thresh = 0.5f,
                                            int minimum_negative_samples = 0, float[] variances = null)
        {
            throw new NotImplementedException();
        }

        public static ndarray roi_pooling(ndarray data, ndarray rois, int[] pooled_size, float spatial_scale)
        {
            throw new NotImplementedException();
        }

        public static ndarray smooth_l1(ndarray data, float scalar)
        {
            throw new NotImplementedException();
        }

        public static ndarray sigmoid(ndarray data)
        {
            throw new NotImplementedException();
        }

        public static ndarray softmax(ndarray data, int axis = -1, ndarray length = null, double? temperature = null, bool use_length = false, DType dtype = null)
        {
            throw new NotImplementedException();
        }

        public static ndarray log_softmax(ndarray data, int axis = -1, ndarray length = null, double? temperature = null, bool use_length = false, DType dtype = null)
        {
            throw new NotImplementedException();
        }

        public static ndarray topk(ndarray data, int axis = -1, int k = -1, string ret_typ = "value", bool is_ascend = false, DType dtype = null)
        {
            throw new NotImplementedException();
        }

        public static ndarray waitall()
        {
            throw new NotImplementedException();
        }

        public static NDArrayDict load(string file)
        {
            throw new NotImplementedException();
        }

        public static void save(string file, ndarray arr)
        {
            throw new NotImplementedException();
        }

        public static ndarray one_hot(ndarray data, long depth, double on_value = 1.0, double off_value = 0.0, DType dtype = null)
        {
            throw new NotImplementedException();
        }

        public static ndarray pick(ndarray data, ndarray index, int axis= -1, string mode= "clip", bool keepdims= false)
        {
            throw new NotImplementedException();
        }

        public static ndarray reshape_like(ndarray lhs, ndarray rhs, int? lhs_begin = null, int? lhs_end = null, int? rhs_begin = null, int? rhs_end = null)
        {
            throw new NotImplementedException();
        }

        public static ndarray batch_flatten(ndarray data)
        {
            throw new NotImplementedException();
        }

        public static ndarray batch_dot(ndarray lhs, ndarray rhs, bool transpose_a = false, bool transpose_b = false, string forward_stype = null)
        {
            throw new NotImplementedException();
        }

        public static ndarray gamma(ndarray data)
        {
            throw new NotImplementedException();
        }

        public static ndarray sequence_mask(ndarray data, ndarray sequence_length = null, bool use_sequence_length = false, float value = 0, int axis = 0)
        {
            throw new NotImplementedException();
        }
    }
}
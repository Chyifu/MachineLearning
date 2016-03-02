using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.Util;
using Emgu.CV.Structure;
using Emgu.CV.ML;
using Emgu.CV.ML.Structure;

namespace _102378056_HW5
{
    public partial class Form1 : Form
    {
        int trainSampleCount=100;
        int hidLayer=5;
        float scale_dw=0.1F;
        float scale_mom=0.1F;

        Matrix<float> trainData;
        Matrix<float> trainClasses;
        Matrix<float> Sample1=null;                                           
        Matrix<int> Sample2=null;                                          
  
        Image<Bgr, Byte> img = new Image<Bgr, byte>(500, 500);

        Matrix<float> sample;                                           
        Matrix<float> prediction;                                           

        Matrix<float> trainData1;
        Matrix<float> trainData2 ;

        Matrix<float> trainClasses1;
        Matrix<float> trainClasses2;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            trainSampleCount =(int)TrnSmpCon.Value;
            hidLayer=(int)hidlayernode.Value;
            scale_dw= (float)numericUpDown1.Value;
            scale_mom = (float)numericUpDown2.Value;

            Matrix<int> layerSize = new Matrix<int>(new int[] { 2, hidLayer, 1 });

            MCvANN_MLP_TrainParams parameters = new MCvANN_MLP_TrainParams();
            parameters.term_crit = new MCvTermCriteria(10, 1.0e-8);
            parameters.train_method = Emgu.CV.ML.MlEnum.ANN_MLP_TRAIN_METHOD.BACKPROP;
            parameters.bp_dw_scale = scale_dw;
            parameters.bp_moment_scale = scale_mom;

            using (ANN_MLP network = new ANN_MLP(layerSize, Emgu.CV.ML.MlEnum.ANN_MLP_ACTIVATION_FUNCTION.SIGMOID_SYM, 1.0, 1.0))
            {
                network.Train(trainData, trainClasses, Sample1, Sample2, parameters, Emgu.CV.ML.MlEnum.ANN_MLP_TRAINING_FLAG.DEFAULT);

                for (int i = 0; i < img.Height; i++)
                {
                    for (int j = 0; j < img.Width; j++)
                    {
                        sample.Data[0, 0] = j;
                        sample.Data[0, 1] = i;
                        network.Predict(sample, prediction);

                        // estimates the response and get the neighbors' labels
                        float response = prediction.Data[0, 0];

                        // highlight the pixel depending on the accuracy (or confidence)
                        img[i, j] = response < 1.5 ? new Bgr(90, 0, 0) : new Bgr(0, 90, 0);
                            
                    }
                }
            }

            // display the original training samples
            for (int i = 0; i < (trainSampleCount /3); i++)
            {
                PointF p1 = new PointF(trainData1[i, 0], trainData1[i, 1]);
                img.Draw(new CircleF(p1, 2), new Bgr(255, 100, 100), -1);
                PointF p2 = new PointF((int)trainData2[i, 0], (int)trainData2[i, 1]);
                img.Draw(new CircleF(p2, 2), new Bgr(100, 255, 100), -1);
            }
            //Emgu.CV.UI.ImageViewer.Show(img);
            imageBox1.Image = img;
         //   button1.Text = "OK";
        }

        private void TrnSmpCon_ValueChanged(object sender, EventArgs e)
        {
            trainSampleCount = (int)TrnSmpCon.Value;
        }

        private void hidlayernode_ValueChanged(object sender, EventArgs e)
        {
            hidLayer = (int)hidlayernode.Value;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            #region Generate the traning data and classes
            trainData = new Matrix<float>(trainSampleCount, 2);
            trainClasses = new Matrix<float>(trainSampleCount, 1);

  
            img = new Image<Bgr, byte>(500, 500);

            sample = new Matrix<float>(1, 2);                                          
            prediction = new Matrix<float>(1, 1);                                          

            trainData1 = trainData.GetRows(0, trainSampleCount>>1, 1);
            trainData1.SetRandNormal(new MCvScalar(200), new MCvScalar(50));
            trainData2 = trainData.GetRows(trainSampleCount>>1, trainSampleCount, 1);
            trainData2.SetRandNormal(new MCvScalar(300), new MCvScalar(50));


            trainClasses1 = trainClasses.GetRows(0, trainSampleCount>>1, 1);
            trainClasses1.SetValue(1);
           trainClasses2 = trainClasses.GetRows(trainSampleCount >>1, trainSampleCount, 1);
            trainClasses2.SetValue(2);
            #endregion

            for (int i = 0; i < img.Height; i++)
            {
                for (int j = 0; j < img.Width; j++)
                {
                    img[i, j] =new Bgr(0, 0, 0);
                }
            }
            for (int i = 0; i < (trainSampleCount / 3); i++)
            {
                PointF p1 = new PointF(trainData1[i, 0], trainData1[i, 1]);
                img.Draw(new CircleF(p1, 2), new Bgr(255, 100, 100), -1);
                PointF p2 = new PointF((int)trainData2[i, 0], (int)trainData2[i, 1]);
                img.Draw(new CircleF(p2, 2), new Bgr(100, 255, 100), -1);
            }
            imageBox1.Image = img;
        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            scale_mom = (float)numericUpDown2.Value;
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            scale_dw =(float)numericUpDown1.Value;
        }
    }
}

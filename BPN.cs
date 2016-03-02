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
using Emgu.CV.UI;
using Emgu.CV.Structure;

namespace HW4_102378056
{
    public partial class BPN : Form
    {
        float[] X = new float[2];
        float[] Y = new float[2];
        float[,] L = new float[2, 6];
        float[,] Pattern=new float[,]{{0.5F,-0.5F,0.9F,0.1F},{-0.5F,0.5F,0.1F,0.9F}};
        float[, ,] W = new float[,,] { { { 0.01F, 0.1F, 0.3F }, { -0.02F, -0.2F, 0.55F } }, { { 0.31F, 0.37F, -0.22F }, { 0.27F, 0.9F, -0.12F } } };
        float[, ,] DW = new float[2, 2, 3];
        float E1, E2;
        float Lrate=1.2F;
        float Mmut = 0.8F;
        float d1, d2;
        public BPN()
        {
            InitializeComponent();
            InitializeBPN();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            randomWeight();                   
        }

        private void InitializeBPN()
        {
            X11.Text = "" + Pattern[0, 0];
            X21.Text = "" + Pattern[0, 1];
            T11.Text = "" + Pattern[0, 2];
            T21.Text = "" + Pattern[0, 3];
            X12.Text = "" + Pattern[1, 0];
            X22.Text = "" + Pattern[1, 1];
            T12.Text = "" + Pattern[1, 2];
            T22.Text = "" + Pattern[1, 3];
            learningrate.Text = "" + Lrate;
            Momentum.Text = "" + Mmut;
            X1.Text = "" + Pattern[0, 0];
            X2.Text = "" + Pattern[0, 1];
            d1 = Pattern[0, 2];
            d2 = Pattern[0, 3];
            randomWeight();   
        }

        private void randomWeight()
        {
            Random random = new Random(Guid.NewGuid().GetHashCode());  //利用 Guid.NewGuid()每一次所產生出來的結果都是不同的，再利用它產生雜湊碼來當成亂數產生器的種子，產生出真的亂數。
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    for (int k = 0; k < 3; k++)
                    {
                                            W[i, j, k] = (float)random.Next(-6, 6) + (float)random.NextDouble(); 
                    }
                }
            }
            W110.Text = "" + W[0, 0, 0];
            W111.Text = "" + W[0, 0, 1];
            W112.Text = "" + W[0, 0, 2];
            W120.Text = "" + W[0, 1, 0];
            W121.Text = "" + W[0, 1, 1];
            W122.Text = "" + W[0, 1, 2];
            W210.Text = "" + W[1, 0, 0];
            W211.Text = "" + W[1, 0, 1];
            W212.Text = "" + W[1, 0, 2];
            W220.Text = "" + W[1, 1, 0];
            W221.Text = "" + W[1, 1, 1];
            W222.Text = "" + W[1, 1, 2];
        }

        static float F_Count_NodeWeight(float node1,float node2,float weight1,float weight2,float nodeweight) 
        {
            float output;
            output = node1 * weight1 + node2 * weight2 + nodeweight;
            return output;
        }

        static float F_Count_NodeValue(float net)
        {
            float output;
            output = 1/(1+(float)Math.Exp(-net));
            return output;
        }

        static float B_Count_NodeValue(float node1, float node2, float weight1, float weight2, float nodevalue)
        {
            float output;
            output = (node1 * weight1 + node2 * weight2) * nodevalue * (1 - nodevalue);
            return output;
        }

        static float Count_Error(float D,float S)
        {
            float output;
            output = D - S;
            return output;
        }

        static float Count_BackInput(float E, float S)
        {
            float output;
            output = E*S*(1-S);
            return output;
        }

        static float B_Count_NodeWeight(float node, float oldweight, float L)
        {
            float output;
            output = oldweight + L*node;
            return output;
        }

        static float B_Count_DeltaNodeWeight(float node, float L)
        {
            float output;
            output = L * node;
            return output;
        }

        static float BM_Count_NodeWeight(float node, float oldweight, float L,float DW,float M)
        {
            float output;
            output = oldweight + L * node+M*DW;
            return output;
        }

        static float B_Count_NodePathWeight(float node1, float node2, float oldweight, float L)
        {
            float output;
            output = oldweight + L * node1*node2;
            return output;
        }

        static float BM_Count_NodePathWeight(float node1, float node2, float oldweight, float L, float DW, float M)
        {
            float output;
            output = oldweight + L * node1 * node2+M*DW;
            return output;
        }

        static float B_Count_DeltaNodePathWeight(float node1, float node2, float L)
        {
            float output;
            output =  L * node1 * node2;
            return output;
        }

        private void trainingBPN_forward() 
        {
            L[0, 0] = F_Count_NodeWeight(X[0], X[1], W[0, 0, 1], W[0, 1, 1], W[0, 0, 0]);
            L[0, 1] = F_Count_NodeWeight(X[0], X[1], W[0, 0, 2], W[0, 1, 2], W[0, 1, 0]);
            L[0, 2] = F_Count_NodeValue(L[0, 0]);
            L[0, 3] = F_Count_NodeValue(L[0, 1]);

            L[1, 0] = F_Count_NodeWeight(L[0, 2], L[0, 3], W[1, 0, 1], W[1, 1, 1], W[1, 0, 0]);
            L[1, 1] = F_Count_NodeWeight(L[0, 2], L[0, 3], W[1, 0, 2], W[1, 1, 2], W[1, 1, 0]);
            L[1, 2] = F_Count_NodeValue(L[1, 0]);
            L[1, 3] = F_Count_NodeValue(L[1, 1]);

            Net11.Text = "" + L[0, 0];
            Net21.Text = "" + L[0, 1];
            Net12.Text = "" + L[1, 0];
            Net22.Text = "" + L[1, 1];

            Output11.Text = "" + L[0, 2];
            Output21.Text = "" + L[0, 3];
            Output12.Text = "" + L[1, 2];
            Output22.Text = "" + L[1, 3];
            Y1.Text = "" + L[1, 2];
            Y2.Text = "" + L[1, 3];
        }

        private void trainingBPN_Back()
        {
            L[0, 4] = B_Count_NodeValue(L[1,4],L[1,5],W[1,0,1],W[1,0,2],L[0,2]);
            L[0, 5] = B_Count_NodeValue(L[1,4],L[1,5],W[1,1,1], W[1,1,2],L[0,3]);

            Delta11.Text = "" + L[0, 4];
            Delta21.Text = "" + L[0, 5];

            W[1, 0, 0] = B_Count_NodeWeight(L[1,4],W[1,0,0],Lrate);
            DW[1, 0, 0] = B_Count_DeltaNodeWeight(L[1, 4], Lrate); 
            W[1, 0, 1] = B_Count_NodePathWeight(L[1,4],L[0,2],W[1,0,1],Lrate);
            DW[1, 0, 1] = B_Count_DeltaNodePathWeight(L[1, 4], L[0, 2], Lrate);
            W[1, 1, 1] = B_Count_NodePathWeight(L[1,4],L[0,3],W[1,1,1],Lrate);
            DW[1, 1, 1] = B_Count_DeltaNodePathWeight(L[1, 4], L[0, 3], Lrate);

            W[1, 1, 0] = B_Count_NodeWeight(L[1, 5], W[1, 1, 0], Lrate);
            DW[1, 1, 0] = B_Count_DeltaNodeWeight(L[1, 5], Lrate);
            W[1, 1, 2] = B_Count_NodePathWeight(L[1, 5], L[0, 3], W[1, 1, 2], Lrate);
            DW[1, 1, 2] = B_Count_DeltaNodePathWeight(L[1, 5], L[0, 3], Lrate);
            W[1, 0, 2] = B_Count_NodePathWeight(L[1, 5], L[0, 2], W[1, 0, 2], Lrate);
            DW[1,0, 2] = B_Count_DeltaNodePathWeight(L[1, 5], L[0, 2], Lrate);

            W[0, 0, 0] = B_Count_NodeWeight(L[0, 4], W[0, 0, 0], Lrate);
            DW[0, 0, 0] = B_Count_DeltaNodeWeight(L[0, 4], Lrate);
            W[0, 0, 1] = B_Count_NodePathWeight(L[0, 4], X[0], W[0, 0, 1], Lrate);
            DW[0, 0, 1] = B_Count_DeltaNodePathWeight(L[0, 4], X[0], Lrate);
            W[0, 1, 1] = B_Count_NodePathWeight(L[0, 4], X[1], W[0, 1, 1], Lrate);
            DW[0, 1, 1] = B_Count_DeltaNodePathWeight(L[0, 4], X[1], Lrate);

            W[0, 1, 0] = B_Count_NodeWeight(L[0, 5], W[0, 1, 0], Lrate);
            DW[0, 1, 0] = B_Count_DeltaNodeWeight(L[0, 5], Lrate);
            W[0, 1, 2] = B_Count_NodePathWeight(L[0, 5], X[1], W[0, 1, 2], Lrate);
            DW[0,1, 2] = B_Count_DeltaNodePathWeight(L[0, 5], X[1], Lrate);
            W[0, 0, 2] = B_Count_NodePathWeight(L[0, 5], X[0], W[0, 0, 2], Lrate);
            DW[0, 0,2] = B_Count_DeltaNodePathWeight(L[0, 5], X[0], Lrate);

            W110.Text = "" + W[0, 0, 0];
            W111.Text = "" + W[0, 0, 1];
            W112.Text = "" + W[0, 0, 2];
            W120.Text = "" + W[0, 1, 0];
            W121.Text = "" + W[0, 1, 1];
            W122.Text = "" + W[0, 1, 2];
            W210.Text = "" + W[1, 0, 0];
            W211.Text = "" + W[1, 0, 1];
            W212.Text = "" + W[1, 0, 2];
            W220.Text = "" + W[1, 1, 0];
            W221.Text = "" + W[1, 1, 1];
            W222.Text = "" + W[1, 1, 2];
        }
        private void Momentum_trainingBPN_Back()
        {
            L[0, 4] = B_Count_NodeValue(L[1, 4], L[1, 5], W[1, 0, 1], W[1, 0, 2], L[0, 2]);
            L[0, 5] = B_Count_NodeValue(L[1, 4], L[1, 5], W[1, 1, 1], W[1, 1, 2], L[0, 3]);

            Delta11.Text = "" + L[0, 4];
            Delta21.Text = "" + L[0, 5];

            W[1, 0, 0] = BM_Count_NodeWeight(L[1, 4], W[1, 0, 0], Lrate,DW[1,0,0],Mmut);
            W[1, 0, 1] = BM_Count_NodePathWeight(L[1, 4], L[0, 2], W[1, 0, 1], Lrate,DW[1,0,1],Mmut);
            W[1, 1, 1] = BM_Count_NodePathWeight(L[1, 4], L[0, 3], W[1, 1, 1], Lrate,DW[1,1,1],Mmut);

            W[1, 1, 0] = BM_Count_NodeWeight(L[1, 5], W[1, 1, 0], Lrate,DW[1,1,0],Mmut);
            W[1, 1, 2] = BM_Count_NodePathWeight(L[1, 5], L[0, 3], W[1, 1, 2], Lrate,DW[1,1,2],Mmut);
            W[1, 0, 2] = BM_Count_NodePathWeight(L[1, 5], L[0, 2], W[1, 0, 2], Lrate,DW[1,0,2],Mmut);

            W[0, 0, 0] = BM_Count_NodeWeight(L[0, 4], W[0, 0, 0], Lrate,DW[0,0,0],Mmut);
            W[0, 0, 1] = BM_Count_NodePathWeight(L[0, 4], X[0], W[0, 0, 1], Lrate,DW[0,0,1],Mmut);
            W[0, 1, 1] = BM_Count_NodePathWeight(L[0, 4], X[1], W[0, 1, 1], Lrate,DW[0,1,1],Mmut);

            W[0, 1, 0] = BM_Count_NodeWeight(L[0, 5], W[0, 1, 0], Lrate,DW[0,1,0],Mmut);
            W[0, 1, 2] = BM_Count_NodePathWeight(L[0, 5], X[1], W[0, 1, 2], Lrate,DW[0,1,2],Mmut);
            W[0, 0, 2] = BM_Count_NodePathWeight(L[0, 5], X[0], W[0, 0, 2], Lrate,DW[0,0,2],Mmut);

            W110.Text = "" + W[0, 0, 0];
            W111.Text = "" + W[0, 0, 1];
            W112.Text = "" + W[0, 0, 2];
            W120.Text = "" + W[0, 1, 0];
            W121.Text = "" + W[0, 1, 1];
            W122.Text = "" + W[0, 1, 2];
            W210.Text = "" + W[1, 0, 0];
            W211.Text = "" + W[1, 0, 1];
            W212.Text = "" + W[1, 0, 2];
            W220.Text = "" + W[1, 1, 0];
            W221.Text = "" + W[1, 1, 1];
            W222.Text = "" + W[1, 1, 2];
        }
        private void P1_training_Click(object sender, EventArgs e)
        {
            X[0] = Pattern[0, 0];
            X[1] = Pattern[0, 1];
            X1.Text = "" + Pattern[0, 0];
            X2.Text = "" + Pattern[0, 1];
            d1 = Pattern[0, 2];
            d2 = Pattern[0, 3];
            trainingBPN_forward();

            E1 = Count_Error(d1, L[1,2]);
            E2 = Count_Error(d2, L[1,3]);
            L[1, 4] = Count_BackInput(E1, L[1,2]);
            L[1, 5] = Count_BackInput(E2, L[1,3]);
            e1.Text = "" + E1;
            e2.Text = "" + E2;
            Delta12.Text = "" + L[1, 4];
            Delta22.Text = "" + L[1, 5];

            trainingBPN_Back();
        }

        private void P2_training_Click(object sender, EventArgs e)
        {
            X[0] = Pattern[1, 0];
            X[1] = Pattern[1, 1];
            X1.Text = "" + Pattern[1, 0];
            X2.Text = "" + Pattern[1, 1];
            d1 = Pattern[1, 2];
            d2 = Pattern[1, 3];
            trainingBPN_forward();

            E1 = Count_Error(d1, L[1, 2]);
            E2 = Count_Error(d2, L[1, 3]);
            L[1, 4] = Count_BackInput(E1, L[1, 2]);
            L[1, 5] = Count_BackInput(E2, L[1, 3]);
            e1.Text = "" + E1;
            e2.Text = "" + E2;
            Delta12.Text = "" + L[1, 4];
            Delta22.Text = "" + L[1, 5];

            Momentum_trainingBPN_Back();
        }

        private void W110_TextChanged(object sender, EventArgs e)
        {
            if (W110.Text != "-" && W110.Text != "") 
            { 
                W[0, 0, 0] = Convert.ToSingle(W110.Text); 
            }
        }

        private void W111_TextChanged(object sender, EventArgs e)
        {
            if (W111.Text != "-"&& W111.Text != "")
            {
                W[0, 0, 1] = Convert.ToSingle(W111.Text);
            }
        }

        private void W112_TextChanged(object sender, EventArgs e)
        {
            if (W112.Text != "-"&& W112.Text != "")
            {
                W[0, 0, 2] = Convert.ToSingle(W112.Text);
            }
        }

        private void W120_TextChanged(object sender, EventArgs e)
        {
            if (W120.Text != "-"&& W120.Text !="")
            {
                W[0, 1, 0] = Convert.ToSingle(W120.Text);
            }
        }

        private void W121_TextChanged(object sender, EventArgs e)
        {
            if (W121.Text != "-"&& W121.Text !="")
            {
                W[0, 1, 1] = Convert.ToSingle(W121.Text);
            }
        }

        private void W122_TextChanged(object sender, EventArgs e)
        {
            if (W122.Text != "-"&& W122.Text !="")
            {
               W[0, 1, 2] = Convert.ToSingle(W122.Text);
            }
        }

        private void W210_TextChanged(object sender, EventArgs e)
        {
            if (W210.Text != "-"&& W210.Text != "")
            {
                W[1, 0, 0] = Convert.ToSingle(W210.Text);
            }
        }

        private void W211_TextChanged(object sender, EventArgs e)
        {
            if (W211.Text != "-"&& W211.Text !="")
            {
                W[1, 0, 1] = Convert.ToSingle(W211.Text);
            }
        }

        private void W212_TextChanged(object sender, EventArgs e)
        {
            if (W212.Text != "-"&& W212.Text != "")
            {
                W[1, 0, 2] = Convert.ToSingle(W212.Text);
            }
        }

        private void W220_TextChanged(object sender, EventArgs e)
        {
            if (W220.Text != "-"&&  W220.Text != "")
            {
                W[1, 1, 0] = Convert.ToSingle(W220.Text);
            }
        }

        private void W221_TextChanged(object sender, EventArgs e)
        {
            if (W221.Text != "-"&&W221.Text != "")
            {
             W[1, 1, 1] = Convert.ToSingle(W221.Text);
            }
        }

        private void W222_TextChanged(object sender, EventArgs e)
        {
            if (W222.Text != "-" && W222.Text != "")
            {
              W[1,1, 2] = Convert.ToSingle(W222.Text);
            }
        }

        private void learningrate_TextChanged(object sender, EventArgs e)
        {
            if (learningrate.Text != "-" && learningrate.Text != "")
            {
                Lrate = Convert.ToSingle(learningrate.Text);
            }
        }

        private void Momentum_TextChanged(object sender, EventArgs e)
        {
            if (Momentum.Text != "-" && Momentum.Text != "")
            {
                Mmut = Convert.ToSingle(Momentum.Text);
            }
        }

        private void extest_Click(object sender, EventArgs e)
        {
            Lrate = 1.2F;
            Mmut = 0.8F;
            W = new float[,,] { { { 0.01F, 0.1F, 0.3F }, { -0.02F, -0.2F, 0.55F } }, { { 0.31F, 0.37F, -0.22F }, { 0.27F, 0.9F, -0.12F } } };
            W110.Text = "" + W[0, 0, 0];
            W111.Text = "" + W[0, 0, 1];
            W112.Text = "" + W[0, 0, 2];
            W120.Text = "" + W[0, 1, 0];
            W121.Text = "" + W[0, 1, 1];
            W122.Text = "" + W[0, 1, 2];
            W210.Text = "" + W[1, 0, 0];
            W211.Text = "" + W[1, 0, 1];
            W212.Text = "" + W[1, 0, 2];
            W220.Text = "" + W[1, 1, 0];
            W221.Text = "" + W[1, 1, 1];
            W222.Text = "" + W[1, 1, 2];
            learningrate.Text = "" + Lrate;
            Momentum.Text = "" + Mmut;
            
        }

        private void clean_Click(object sender, EventArgs e)
        {
            foreach (Control c in Controls)
            {
                if (c is TextBox)
                       c.Text = "";                   
            }
            foreach (Control tb in groupBox1.Controls) 
            {
                if (tb is TextBox)
                     tb.Text = "";
            }
        }

        private void X11_TextChanged(object sender, EventArgs e)
        {
            if (X11.Text != "-" && X11.Text != "")
            {
                Pattern[0, 0] = Convert.ToSingle(X11.Text);
            }
        }

        private void X21_TextChanged(object sender, EventArgs e)
        {
            if (X21.Text != "-" && X21.Text != "")
            {
                Pattern[0, 1] = Convert.ToSingle(X21.Text);
            }
        }

        private void T11_TextChanged(object sender, EventArgs e)
        {
            if (T11.Text != "-" && T11.Text != "")
            {
                Pattern[0, 2] = Convert.ToSingle(T11.Text);
            }
        }

        private void T21_TextChanged(object sender, EventArgs e)
        {
            if (T21.Text != "-" && T21.Text != "")
            {
                Pattern[0, 3] = Convert.ToSingle(T21.Text);
            }
        }

        private void X12_TextChanged(object sender, EventArgs e)
        {
            if (X12.Text != "-" && X12.Text != "")
            {
                Pattern[1, 0] = Convert.ToSingle(X12.Text);
            }
        }

        private void X22_TextChanged(object sender, EventArgs e)
        {
            if (X22.Text != "-" && X22.Text != "")
            {
                Pattern[1, 1] = Convert.ToSingle(X22.Text);
            }
        }

        private void T12_TextChanged(object sender, EventArgs e)
        {
            if (T12.Text != "-" && T12.Text != "")
            {
                Pattern[1, 2] = Convert.ToSingle(T12.Text);
            }
        }

        private void T22_TextChanged(object sender, EventArgs e)
        {
            if (T22.Text != "-" && T22.Text != "")
            {
                Pattern[1,3] = Convert.ToSingle(T22.Text);
            }
        }


    }
}



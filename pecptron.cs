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

namespace HW3_102378056
{
    public partial class Form1 : Form
    {
        //宣告

        int NodeNum = 0;
        Graphics g;
        float penwidth = 2F;                                                     //初始設定畫筆寬度
        Pen pen = new Pen(Color.Blue, 5);                                       //初始設定畫筆顏色
        SolidBrush Bpen = new SolidBrush(Color.Blue);                           //初始設定填滿畫筆顏色
        Color color = Color.Blue;                                               //初始設定顏色變數顏色
        Bitmap img;                                                              //記錄繪圖
        Bitmap buffer;                                                           //暫時置放繪圖空間
        List<Point> points_G1 = new List<Point>();                               //記錄分類群1的座標點序列
        List<Point> points_G2 = new List<Point>();                               //記錄分類群2的座標點序列
        List<Point> allpoint = new List<Point>();                                 //記錄畫面上所有座標點的序列
        List<Point> Node = new List<Point>();
        List<int> NodeClassific = new List<int>();
        bool isGroup1 = true;                                                     //判別目前所繪座標點為哪種分類群
        Point final1, final2;                                                      //放置最後分群線與左右畫布邊界的交點座標值 final1(0,y)是畫面左邊界和分群線交會點  final2(x,684)是畫面右邊界和分群線交會點

        int step = 0;
        /****************************************************************************************************/
        /*                                   程式初始                                                       */
        /****************************************************************************************************/

        public Form1()
        {
            InitializeComponent();
            img = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            g = Graphics.FromImage(img);
            points_G1.Clear();
            points_G2.Clear();
        }

        /****************************************************************************************************/
        /*                                   在PictureBox中點選滑鼠之控制                                   */
        /****************************************************************************************************/

        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            Point p = new Point(e.X, e.Y);                                      //取得目前滑鼠點選的座標位置
            buffer = new Bitmap(img);
            Graphics g = Graphics.FromImage(buffer);
            if (isGroup1 == true)                                               //判別目前的分群為何組 分類是群一就把點放入points_G1 群二就把點放入points_G2
            {
                points_G1.Add(p);
                allpoint.Add(p);
                NodeClassific.Add(0);
            }
            else
            {
                points_G2.Add(p);
                allpoint.Add(p);
                NodeClassific.Add(1);
            }

            Pen pc = new Pen(color, penwidth);
            g.DrawEllipse(pc, p.X, p.Y, 5, 5);
            //         SolidBrush Bpen = new SolidBrush(color);                           //填滿筆刷畫
            //         g.FillEllipse(Bpen, p.X, p.Y, penwidth, penwidth);                 //依照點下當時的座標畫一個填滿的圓形

            pictureBox1.Image = buffer;
            img = buffer;
        }

        private void group1_Click(object sender, EventArgs e)                  //group1被點下 isGroup1切換成true 顏色換成藍色
        {
            isGroup1 = true;
            color = Color.Blue;
        }

        private void group2_Click(object sender, EventArgs e)                 //group2被點下 isGroup1切換成false 顏色換成橘色
        {
            isGroup1 = false;
            color = Color.Orange;
        }


       

        /****************************************************************************************************/
        /*                                   清除畫面和初始化座標序列                                       */
        /****************************************************************************************************/
        private void clean_Click(object sender, EventArgs e)
        {
            points_G1.Clear();                                           //初始化群一座標list
            points_G2.Clear();                                           //初始化群二座標list
            allpoint.Clear();                                            //初始化所有座標集合list
            Node.Clear();
            NodeClassific.Clear();
            img = new Bitmap(pictureBox1.Width, pictureBox1.Height);     //重新宣告空白的img
            pictureBox1.Image = (img);                                   //放置到picturebox1
            step = 0;
            richTextBox1.Text = "";
        }

        private void Percep_button_Click(object sender, EventArgs e)
        {
            buffer = new Bitmap(img);
            Graphics g = Graphics.FromImage(buffer);
            Pen pc = new Pen(Color.Black, 1);
            Node.Clear();
            float Coe1=-1, Coe2=0.8F, intercept=3;
            int check_Node = 0;
            int judge;
            int check_time;

            changeNode();

            for(check_time=0;check_time<100;check_time++)
            {
                judge=checkline(check_Node, Coe1, Coe2, intercept);
                switch (judge) 
                {
                    case 0:
                        check_Node++;
                        break;
                    case 1:
                        changeCoe(judge, check_Node, ref Coe1, ref Coe2, ref intercept);
                        check_Node = 0;
                        break;
                    case 2:
                        changeCoe(judge, check_Node, ref Coe1, ref Coe2, ref intercept);
                        check_Node = 0;
                        break;
                }
                if (check_Node == allpoint.Count-1)
                {
                    MessageBox.Show("find!");
                    break;
                }
            }
            if (check_time < 100)
            {
                richTextBox1.Text = check_time + "times";
            }
            else
            {
                    MessageBox.Show("can't find!");
                    richTextBox1.Text = check_time + "times";
            }

            final1.X = -350;
            final1.Y = (int)((-intercept / Coe2) - (Coe1 / Coe2) * (-350));
            final2.X = 350;
            final2.Y = (int)((-intercept / Coe2) - (Coe1 / Coe2) * (350));

            final1.X = final1.X + 350;
            final1.Y = 200 - final1.Y;
            final2.X = final2.X + 350;
            final2.Y = 200 - final2.Y;

            g.DrawLine(pc, final1, final2);
            pictureBox1.Image = buffer;
            img = buffer;
            
        }

        private int checkline(int i,float w1,float w2, float b)
        {
            float temp;
            int output=0;
            int T=NodeClassific[i];
            int a;
            temp = w1 * Node[i].X + w2 * Node[i].Y + b;
            if(temp<=0)
                a=0;
            else
                a=1;
            
            if(T==0 && a==0)
                output =0;
            if(T==1 && a==1)
                output =0;
            if(T==1 && a==0)
                output =1;
            if(T==0 && a==1)
                output =2;

            return output;
        }

        private void changeNode()
        {
            int X, Y;
            Point poi;
            for (int i = 0; i < allpoint.Count; i++)
            {
                X = allpoint[i].X-350 ;
                Y = 200-allpoint[i].Y ;
                poi = new Point(X, Y);
                Node.Add(poi);
            }
        }

        private void changeCoe(int judge,int check_Node, ref float w1, ref float w2, ref float B)
        {
            float rate_value = (float)rate.Value;
            float normalization = 0;
            switch (judge)
            {
                case 1:
                    normalization =(float)Math.Sqrt(Node[check_Node].X * Node[check_Node].X + Node[check_Node].Y * Node[check_Node].Y);
                    w1 = rate_value * ((w1 + Node[check_Node].X) / normalization);
                    w2 = rate_value * ((w2*(-1) + Node[check_Node].Y) / normalization);
                    B = B + 1;
                    break;
                case 2:
                    normalization =(float)Math.Sqrt(Node[check_Node].X * Node[check_Node].X + Node[check_Node].Y * Node[check_Node].Y);
                    w1 = rate_value * ((w1 - Node[check_Node].X) / normalization);
                    w2 = rate_value * ((w2*(-1) - Node[check_Node].Y) / normalization);
                    B = B - 1;
                    break;
            }
        }

        float Coe1 =350, Coe2 = 350F, intercept = 3; int check_time=0;
        private void button1_Click(object sender, EventArgs e)
        {
            buffer = new Bitmap(img);
            Graphics g = Graphics.FromImage(buffer);
            Pen pc = new Pen(Color.Black, 1);
            
            int judge;

            if (check_time == 0)
            {
                changeNode(); 
            }
            if (NodeNum < allpoint.Count - 1)
            {
                judge = checkline(NodeNum, Coe1, Coe2, intercept);
                switch (judge)
                {
                    case 0:
                        check_time++;
                        NodeNum++;
                        break;
                    case 1:
                        changeCoe(judge, NodeNum, ref Coe1, ref Coe2, ref intercept);
                        NodeNum = 0;
                        check_time++;
                        break;
                    case 2:
                        changeCoe(judge, NodeNum, ref Coe1, ref Coe2, ref intercept);
                        NodeNum = 0;
                        check_time++;
                        break;
                }
            }
            if (NodeNum == allpoint.Count - 1)
            {
                MessageBox.Show("find!");
            }
            richTextBox1.Text = check_time + "times";    

            final1.X = -350;
            final1.Y = (int)((-intercept / Coe2) - (Coe1 / Coe2)*(-350));
            final2.X = 350;
            final2.Y = (int)((-intercept / Coe2) - (Coe1 / Coe2) * (350));

            final1.X = final1.X + 350;
            final1.Y = 200 - final1.Y;
            final2.X = final2.X + 350;
            final2.Y = 200 - final2.Y;

            g.DrawLine(pc, final1, final2);
            pictureBox1.Image = buffer;
            img = buffer;
            
    }

        private void Perceptron_pocket_Click(object sender, EventArgs e)
        {
            buffer = new Bitmap(img);
            Graphics g = Graphics.FromImage(buffer);
            Pen pc = new Pen(Color.Black, 1);
            Pen ppc = new Pen(Color.Red, 1);
            Node.Clear();
            int NumPocket = 0;//放置目前分對的最多點數 (還要記錄w1,w2,B   記得所有node都要跑到)
            float Pcoe1 = 0, Pcoe2 = 0, Pintercept = 0;
            float Coe1 = -1, Coe2 = 0.8F, intercept = 3;
            int check_Node = 0;
            int judge;
            int check_time;
            Point F1 = new Point(), F2 = new Point();
            changeNode();

            for (check_time = 0; check_time < 100; check_time++)
            {
                judge = checkline(check_Node, Coe1, Coe2, intercept);
                switch (judge)
                {
                    case 0:
                        check_Node++;
                        break;
                    case 1:
                        changeCoe(judge, check_Node, ref Coe1, ref Coe2, ref intercept);
                        pocket(ref NumPocket, Coe1, Coe2, intercept, ref Pcoe1, ref Pcoe2, ref Pintercept);
                        check_Node = 0;
                        break;
                    case 2:
                        changeCoe(judge, check_Node, ref Coe1, ref Coe2, ref intercept);
                        pocket(ref NumPocket, Coe1, Coe2, intercept, ref Pcoe1, ref Pcoe2, ref Pintercept);
                        check_Node = 0;
                        break;
                }
                if (check_Node == allpoint.Count - 1)
                {
                    MessageBox.Show("find!");
                    break;
                }
            }
            if (check_time < 100)
            {
                richTextBox1.Text = check_time + "times";
            }
            else
            {
                MessageBox.Show("can't find!");
                richTextBox1.Text = check_time + "times";
            }

            final1.X = -350;
            final1.Y = (int)((-intercept / Coe2) - (Coe1 / Coe2) * (-350));
            final2.X = 350;
            final2.Y = (int)((-intercept / Coe2) - (Coe1 / Coe2) * (350));

            final1.X = final1.X + 350;
            final1.Y = 200 - final1.Y;
            final2.X = final2.X + 350;
            final2.Y = 200 - final2.Y;

            F1.X = -350;
            F1.Y = (int)((-Pintercept / Pcoe2) - (Pcoe1 / Pcoe2) * (-350));
            F2.X = 350;
            F2.Y = (int)((-Pintercept / Pcoe2) - (Pcoe1 / Pcoe2) * (350));

            F1.X = F1.X + 350;
            F1.Y = 200 - F1.Y;
            F2.X = F2.X + 350;
            F2.Y = 200 - F2.Y;

            g.DrawLine(pc, final1, final2);
            if (check_time == 100)
            {
                MessageBox.Show("red line is pocket line!");
                g.DrawLine(ppc, F1, F2);
            }
            pictureBox1.Image = buffer;
            img = buffer;
        }

        private void pocket(ref int NumPocket, float w1, float w2, float B,ref float Pw1,ref float Pw2, ref float PB)
        {
            float temp;
            int num=0;
            int a;
            for(int i=0;i<Node.Count;i++)
            {
                temp = w1 * Node[i].X + w2 * Node[i].Y + B;
                if (temp <= 0)
                    a = 0;
                else
                    a = 1;
                if (NodeClassific[i] == a)
                    num++;             
            }
            if (num > NumPocket)
            {
                NumPocket = num;
                Pw1 = w1;
                Pw2 = w2;
                PB = B;
            }
        }

    }
}

    

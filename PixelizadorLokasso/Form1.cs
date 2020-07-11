using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

/* {<}================{>} IDEIAS FUTURAS :3 {<}================{>}
--> ADICIONAR FEATURES NOVAS!!!
{<}============================================================{>}*/

namespace PixelizadorLokasso
{
    public partial class Form1 : Form
    {
        private Bitmap rawImgBmp;
        private Bitmap[] alreadyLoadedBmps;

        public Form1()
        {
            InitializeComponent();

            MaximizeBox = false; 
            trackBar1.Enabled = false;
            openFileDialog1.Filter = "Imagem | *.png; *.jpg; *.jpeg; *.bmp;";
            openFileDialog1.FileName = "";
            saveFileDialog1.FileName = "Some Daqui";
            saveFileDialog1.Filter = "Imagem | *.png; *.jpg; *.jpeg; *.bmp;";
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage; // O MODO ZOOM DEIXA A IMG DO MESMO TAMANHO!!! :3
            pictureBox2.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox3.BackColor = Color.Transparent;
            pictureBox4.BackColor = Color.Transparent;
        }

        private void AbrirToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
        }

        private void SalvarComoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveFileDialog1.ShowDialog();
        }

        private void OpenFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            rawImgBmp = new Bitmap(openFileDialog1.FileName);

            trackBar1.Enabled = true;
            trackBar1.Value = 1;

            /*
            int c = 0;
            for (int i = 1; i < trackBar1.Maximum + 1; i++)
            {
                if (rawImgBmp.Width % i == 0 && rawImgBmp.Height % i == 0) { c++; }
            }

            alreadyLoadedBmps = new Bitmap[c];
            loadedBmpCounter = 0;
            */

            alreadyLoadedBmps = new Bitmap[trackBar1.Maximum];

            Bitmap actualBmp = Pixels.NewPixelsBmp(rawImgBmp, trackBar1.Value);

            pictureBox1.Image = rawImgBmp;
            pictureBox2.Image = actualBmp;
            alreadyLoadedBmps[0] = actualBmp;
        }

        private void SaveFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            pictureBox2.Image.Save(saveFileDialog1.FileName);
        }

        private void TrackBar1_ValueChanged(object sender, EventArgs e)
        {
            label1.Text = trackBar1.Value.ToString();

            if (rawImgBmp.Width % trackBar1.Value == 0 && rawImgBmp.Height % trackBar1.Value == 0) 
            {
                label1.BackColor = Color.ForestGreen;

                if (alreadyLoadedBmps[trackBar1.Value - 1] == null)
                {
                    Bitmap bmp = Pixels.NewPixelsBmp(rawImgBmp, trackBar1.Value);

                    pictureBox2.Image = bmp;
                    alreadyLoadedBmps[trackBar1.Value - 1] = bmp;
                }
                else
                {
                    pictureBox2.Image = alreadyLoadedBmps[trackBar1.Value - 1];
                }
            }
            else
            {
                label1.BackColor = Color.DarkRed;
            }
        }
    }

    static class Pixels
    {
        private static SquareVector[] ScaledVectorsMatrix(Size size, int key) 
        {
            // int xKey = size.Width / key;
            // int yKey = size.Height / key;
            // int biggerKey = (xKey > yKey) ? xKey : yKey;
            // biggerKey = biggerKey * biggerKey;
            int biggerKey = (size.Width / key) * (size.Height / key);
            int i = 0;
            
            SquareVector[] matrix = new SquareVector[biggerKey];

            for (int line = 0; line < size.Height; line += key) 
            {
                for (int column = 0; column < size.Width; column += key)
                {
                    SquareVector vector = new SquareVector() // DA PRA INICIALIZA E SETA AS PROP ASSIM, FICA MAIS BONITO!!! :3
                    {
                        NW = new int[] { column, line }, // COLUNA 1° PORQUE ELA REPRESENTA O X E A LINHA O Y!!! :3
                        // SW = new int[] { column, line + key }, // TIRAR ESSE PRA ECONOMIZAR PROCESSADOR!!! :3
                        // NE = new int[] { column + key, line }, // ESSE TAMBÉM!!! :3
                        SE = new int[] { column + key, line + key }
                    };

                    matrix[i] = vector;
                    i++;
                    
                    // if (i + 1 < biggerKey) { i++; }
                }
            }
            
            return matrix;
        }

        public static Bitmap NewPixelsBmp(Bitmap normalBmp, int pixelScale)
        {
            SquareVector[] modelMatrix = ScaledVectorsMatrix(normalBmp.Size, pixelScale);
            Bitmap outputNew = new Bitmap(normalBmp.Size.Width, normalBmp.Size.Height);

            Color[] tempRGBs = new Color[modelMatrix.Length];

            int i = 0;

            foreach (SquareVector vector in modelMatrix)
            {
                int[] startVect = vector.NW; 
                int[] endVect = vector.SE;
                
                int c = 0;

                int R = 0;
                int G = 0;
                int B = 0;

                for (int line = startVect[1]; line < endVect[1]; line += 1)
                {
                    for (int column = startVect[0]; column < endVect[0]; column += 1)
                    {
                        Color pxColor = normalBmp.GetPixel(column, line);
                      
                        R += pxColor.R;
                        G += pxColor.G;
                        B += pxColor.B;
                        
                        c++;
                    }
                }
                
                tempRGBs[i] = Color.FromArgb(R / c, G / c, B / c);

                i++;
            }

            i = 0;

            foreach (Color color in tempRGBs)
            {
                int[] startVect = modelMatrix[i].NW;
                int[] endVect = modelMatrix[i].SE;

                for (int line = startVect[1]; line < endVect[1]; line += 1)
                {
                    for (int column = startVect[0]; column < endVect[0]; column += 1)
                    {
                        outputNew.SetPixel(column, line, color);
                    }
                }

                i++;
            }

            return outputNew;    
        }
    }

    class SquareVector // AMORZIN DE CLASSE S2!!! :3
    {
        private readonly int[][] matrix = new int[2][]; // TAMANHO 4 EM VEZ DE 2 PARA UMA COM QUATRO VECTS!!! :3

        public SquareVector()
        {
            for (int i = 0; i < 2; i++) // DE 0 ATÉ 4 EM VEZ DE ATÉ 2 PARA UMA COM QUATRO VECTS!!! :3
            {
                matrix[i] = new int[2];
            }
        }

        public int[] NW
        {
            get { return matrix[0]; }
            set { matrix[0] = value; }
        }
        
        /*
        public int[] NE
        {
            get { return matrix[1]; }
            set { matrix[1] = value; }
        }

        public int[] SW
        {
            get { return matrix[2]; }
            set { matrix[2] = value; }
        }
        */

        public int[] SE
        {
            get { return matrix[1]; } // 3 EM VEZ DE 1 PARA UMA COM QUATRO VECTS!!! :3
            set { matrix[1] = value; } // 3 EM VEZ DE 1 PARA UMA COM QUATRO VECTS!!! :3
        }
    }
}

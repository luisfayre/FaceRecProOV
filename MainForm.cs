

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using System.IO;
using System.Diagnostics;

namespace MultiFaceRec
{
    public partial class FrmPrincipal : Form
    {
        //Declaracion de variables
        Image<Bgr, Byte> currentFrame;
        Capture grabber;
        HaarCascade face;
        HaarCascade eye;
        MCvFont font = new MCvFont(FONT.CV_FONT_HERSHEY_TRIPLEX, 0.5d, 0.5d);
        Image<Gray, byte> result, TrainedFace = null;
        Image<Gray, byte> gray = null;
        List<Image<Gray, byte>> trainingImages = new List<Image<Gray, byte>>();
        List<string> labels= new List<string>();
        List<string> NamePersons = new List<string>();
        int ContTrain, NumLabels, t;
        string name, names = null;
        List<string> fecha = new List<string>();
        List<string> Fecha = new List<string>();
        string epoca,genial = null;
        List<string> links = new List<string>();
        string link,adjuntar = null;
        List<string> adjunto = new List<string>();

        public FrmPrincipal()
        {
            InitializeComponent();
            //Cargar la cascada para el rostro
            face = new HaarCascade("haarcascade_frontalface_default.xml");
            //eye = new HaarCascade("haarcascade_eye.xml");
            lblTime.Text = DateTime.Now.ToString("HH:mm");
            try
            {
                //Cargado de las caras y laber ya obtenidos previamente
                string Labelsinfo = File.ReadAllText(Application.StartupPath + "/TrainedFaces/TrainedLabels.txt");
                string[] Labels = Labelsinfo.Split('%');
                NumLabels = Convert.ToInt16(Labels[0]);
                ContTrain = NumLabels;
                string LoadFaces;
                string fechasinfo = File.ReadAllText(Application.StartupPath + "/TrainedFaces/FechasLabels.txt");
                string[] fechadenacimiento = fechasinfo.Split('%');
                string linkinfo = File.ReadAllText(Application.StartupPath + "/TrainedFaces/FacebookLabels.txt");
                string[] linkes = linkinfo.Split('%');
                
                for (int tf = 1; tf < NumLabels+1; tf++)
                {
                    LoadFaces = "face" + tf + ".bmp";
                    trainingImages.Add(new Image<Gray, byte>(Application.StartupPath + "/TrainedFaces/" + LoadFaces));
                    labels.Add(Labels[tf]);
                    fecha.Add(fechadenacimiento[tf]);
                    links.Add(linkes[tf]);
                }
            
            }
            catch(Exception e)
            {
             
                MessageBox.Show("Nothing in binary database, please add at least a face(Simply train the prototype with the Add Face Button).", "Triained faces load", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }

        }

        
private void timer_trick(object sender, EventArgs e)
        {
            lblTime.Text = DateTime.Now.ToString("HH:mm");
            lblsec.Text = DateTime.Now.ToString("ss");
            lblfecha.Text = DateTime.Now.ToString("MMM dd yyyy");
            lblsec.Location = new Point(lblTime.Location.X + lblTime.Width - 5,lblsec.Location.Y);
        }

      
        private void FrmPrincipal_Load(object sender, EventArgs e)
        {
            timer.Start();
        }

     
        private void button1_Click(object sender, EventArgs e)
        {
            //captura de la camara
            grabber = new Capture();
            grabber.QueryFrame();
            //inica el evento frameGrabber
            Application.Idle += new EventHandler(FrameGrabber);
            button1.Enabled = false;
        }


        private void button2_Click(object sender, System.EventArgs e)
        {
            try
            {
                //Contador de las caras que tiene el programa
                ContTrain = ContTrain + 1;

                //obtener las escalas de grices
                gray = grabber.QueryGrayFrame().Resize(320, 240, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);

                //Face Detector
                MCvAvgComp[][] facesDetected = gray.DetectHaarCascade(
                face,
                1.2,
                10,
                Emgu.CV.CvEnum.HAAR_DETECTION_TYPE.DO_CANNY_PRUNING,
                new Size(20, 20));

                //La accion de cada  rostro obtenido
                foreach (MCvAvgComp f in facesDetected[0])
                {
                    TrainedFace = currentFrame.Copy(f.rect).Convert<Gray, byte>();
                    break;
                }

                //darle el tamaño correcto para la comparacion
                //Probamos la imagen con una imagenes cubicas
                TrainedFace = result.Resize(100, 100, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
                trainingImages.Add(TrainedFace);
                labels.Add(textBox1.Text);
                fecha.Add(textBox2.Text);
                links.Add(textBox3.Text);

                //mostrar el rostro que se agrego
                imageBox1.Image = TrainedFace;

                //Escribir el numero de rostros que llevamos en el archivo de texto
                File.WriteAllText(Application.StartupPath + "/TrainedFaces/TrainedLabels.txt", trainingImages.ToArray().Length.ToString() + "%");
                File.WriteAllText(Application.StartupPath + "/TrainedFaces/FechasLabels.txt", trainingImages.ToArray().Length.ToString() + "%");
                File.WriteAllText(Application.StartupPath + "/TrainedFaces/FacebookLabels.txt", trainingImages.ToArray().Length.ToString() + "%");

                //Escribir las label que tenemos en el archivo de texto
                for (int i = 1; i < trainingImages.ToArray().Length + 1; i++)
                {
                    trainingImages.ToArray()[i - 1].Save(Application.StartupPath + "/TrainedFaces/face" + i + ".bmp");
                    File.AppendAllText(Application.StartupPath + "/TrainedFaces/TrainedLabels.txt", labels.ToArray()[i - 1] + "%");
                    File.AppendAllText(Application.StartupPath + "/TrainedFaces/FechasLabels.txt", fecha.ToArray()[i - 1] + "%");
                    File.AppendAllText(Application.StartupPath + "/TrainedFaces/FacebookLabels.txt", links.ToArray()[i - 1] + "%");
                }

                MessageBox.Show(textBox1.Text + "´s face detected and added :)", "Training OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch
            {
                MessageBox.Show("Enable the face detection first", "Training Fail", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }


        void FrameGrabber(object sender, EventArgs e)
        {
            label3.Text = "0";
           
            NamePersons.Add("");
            adjunto.Add("");
            Fecha.Add("");

            //obtener el marco del dispositivo que estamos usando 
            currentFrame = grabber.QueryFrame().Resize(320, 240, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);

                    //convertido de escala a grices
                    gray = currentFrame.Convert<Gray, Byte>();

                    //Face Detector
                    MCvAvgComp[][] facesDetected = gray.DetectHaarCascade(
                  face,
                  1.2,
                  10,
                  Emgu.CV.CvEnum.HAAR_DETECTION_TYPE.DO_CANNY_PRUNING,
                  new Size(20, 20));

                    //Action for each element detected
                    foreach (MCvAvgComp f in facesDetected[0])
                    {
                        t = t + 1;
                        result = currentFrame.Copy(f.rect).Convert<Gray, byte>().Resize(100, 100, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
                    
                        currentFrame.Draw(f.rect, new Bgr(Color.Red), 2);


                        if (trainingImages.ToArray().Length != 0)
                        {
                            //Interacciones maximas a usar para recone el rostro 
                        MCvTermCriteria termCrit = new MCvTermCriteria(ContTrain, 0.001);

                        //Eigen face recognizer
                        EigenObjectRecognizer recognizer = new EigenObjectRecognizer(
                           trainingImages.ToArray(),
                           labels.ToArray(),
                           3000,
                           ref termCrit);

                        name = recognizer.Recognize(result);
                    EigenObjectRecognizer dates = new EigenObjectRecognizer(
                       trainingImages.ToArray(),
                       fecha.ToArray(),
                       3000,
                       ref termCrit);
                    epoca = dates.Recognize(result);

                    EigenObjectRecognizer adjunto = new EigenObjectRecognizer(
                      trainingImages.ToArray(),
                      links.ToArray(),
                      3000,
                      ref termCrit);
                    link = adjunto.Recognize(result);

                    //Escribir el laber con el nombre del rostro que reconocio
                    currentFrame.Draw(name, ref font, new Point(f.rect.X - 2, f.rect.Y - 2), new Bgr(Color.LightGreen));

                        }
                adjunto[t-1] = link;
                adjunto.Add("");
                            NamePersons[t-1] = name;
                            NamePersons.Add("");
                Fecha[t - 1] = epoca;
                Fecha.Add("");

                        //mostrar el numero de rostros detectados
                        label3.Text = facesDetected[0].Length.ToString();

                

            }
                        t = 0;

                        //Names concatenation of persons recognized
                    for (int nnn = 0; nnn < facesDetected[0].Length; nnn++)
                    {
                        names = names + NamePersons[nnn] + ", ";
                adjuntar = adjuntar + adjunto[nnn];
                genial = genial + Fecha[nnn];
                    }
                    //Show the faces procesed and recognized
                    imageBoxFrameGrabber.Image = currentFrame;
                    label4.Text = names;
                    label6.Text = genial;
                    linkLabel1.Text = adjuntar;
                    names = "";
                    adjuntar = "";
            genial = "";
                    //Clear the list(vector) of names
                    NamePersons.Clear();
            adjunto.Clear();
            Fecha.Clear();
                }
        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {

        }



    }
}
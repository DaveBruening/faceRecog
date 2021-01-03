using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
namespace ConsoleCoreFaceGitHub {
    partial class Program {
        const string SUBSCRIPTION_KEY = "3139cbfeb88d552f8fc48a845e1918c2"; //replace with your 32-char key
        const string ENDPOINT = "https://mycogntvsrv.cognitiveservices.azure.com/"; //replace w/ your cognitive services URL
        const string IMAGE_BASE_URL = "https://csdx.blob.core.windows.net/resources/Face/Images/";
        const string imgUrl = //"https://st1.thehealthsite.com/wp-content/uploads/2018/08/Angry-People.jpg";
            "https://miro.medium.com/max/2560/1*IBNs0gfdHMIgMboGAGK0Uw.jpeg";
        static List<FaceAttributeType?> lfat;
        static void Main(string[] args) {
            const string RECOGNITION_MODEL3 = RecognitionModel.Recognition03;
            IFaceClient client = Authenticate(ENDPOINT, SUBSCRIPTION_KEY);
            lfat = Enum.GetValues(typeof(FaceAttributeType)).Cast<FaceAttributeType?>().ToList<FaceAttributeType?>();
            personDictionary = new Dictionary<string, string[]> {
                { "Family1-Dad", new[] { "Family1-Dad1.jpg", "Family1-Dad2.jpg" } },
                  { "Family1-Mom", new[] { "Family1-Mom1.jpg", "Family1-Mom2.jpg" } },
                  { "Family1-Son", new[] { "Family1-Son1.jpg", "Family1-Son2.jpg" } },
                  { "Family1-Daughter", new[] { "Family1-Daughter1.jpg", "Family1-Daughter2.jpg" } },
                  { "Family2-Lady", new[] { "Family2-Lady1.jpg", "Family2-Lady2.jpg" } },
                  { "Family2-Man", new[] { "Family2-Man1.jpg", "Family2-Man2.jpg" } }
            };
            davesImageStream(client, "pic7.jpg", RECOGNITION_MODEL3).Wait();
            //davesImageUrl(client, imgUrl, RECOGNITION_MODEL3).Wait();
            //davesFindSimilar(client, IMAGE_BASE_URL, RECOGNITION_MODEL3).Wait();
            //davesIdentifyInPersonGroup(client, IMAGE_BASE_URL, RECOGNITION_MODEL3).Wait();
            Console.ReadLine();
        }
        public static IFaceClient Authenticate(string endpoint, string key) {
            return new FaceClient(new ApiKeyServiceClientCredentials(key)) { Endpoint = endpoint };
        }
        public static async Task davesImageStream(IFaceClient client, string file, string recogModel) {
            //Evaluate an image on your computer instead of on a web site:
            Console.WriteLine("Entering method davesImageStream");
            //Image class was missing from System.Drawing until I added System.Drawing.Common from nuget
            //Image img = Image.FromFile("picture1201.jpg");
            string imgDir = Directory.GetParent(Directory.GetParent(Directory.GetParent(
                Environment.CurrentDirectory).FullName).FullName).FullName;
            Stream strm = new FileStream(imgDir + @"\inputImages\" + file, FileMode.Open);
            IList<DetectedFace> ldf = await client.Face.DetectWithStreamAsync(
                strm, returnFaceAttributes: lfat, recognitionModel: recogModel
                , detectionModel: DetectionModel.Detection01);
            Console.Write($"Face count = {ldf.Count()}");
            int facCnt = 0;
            StringBuilder sbd = new StringBuilder();
            string nl = Environment.NewLine;
            KeyValuePair<string, double> ksd;
            Image img = Image.FromFile(imgDir + @"\inputImages\" + file);
            sbd.Append($"{nl}\timg made from file");
            Graphics grp = Graphics.FromImage(img);
            Pen pen = new Pen(Color.Red, (float)3.2);
            SolidBrush brs = new SolidBrush(Color.Red);
            foreach (DetectedFace df in ldf) {
                ksd = df.FaceAttributes.Emotion.ToRankedList().First();
                sbd.Append($"{nl}Face #{++facCnt}:{nl}" +
                    $"\tprimary emotion: {ksd.Key}, confidence {ksd.Value}.{nl}"
                    + $"\tgender:{df.FaceAttributes.Gender}{nl}");
                if (df.FaceAttributes.Hair.HairColor.Count() > 0) {
                    HairColor hcl = df.FaceAttributes.Hair.HairColor.OrderByDescending(c => c.Confidence).First();
                    sbd.Append($"\tprimary Hair Color={hcl.Color}, confidence={hcl.Confidence}{nl}");
                }
                else
                    sbd.Append($"\tHair Color not detected{nl}");
                sbd.Append($"\tTop={df.FaceRectangle.Top}, Left={df.FaceRectangle.Left}, "
                    + $"Height={df.FaceRectangle.Height}, Width={df.FaceRectangle.Width}");
                //if (facCnt == 1) {
                if (facCnt % 3 == 1) {
                    pen = new Pen(Color.Red, (float)3.2);
                    brs = new SolidBrush(Color.Red);
                }
                else if (facCnt % 3 == 2) {
                    pen = new Pen(Color.Green, (float)3.2);
                    brs = new SolidBrush(Color.Green);
                }
                else {
                    pen = new Pen(Color.Blue, (float)3.2);
                    brs = new SolidBrush(Color.Blue);
                }
                Rectangle rct = new Rectangle(df.FaceRectangle.Left, df.FaceRectangle.Top,
                    df.FaceRectangle.Width, df.FaceRectangle.Height);
                grp.DrawRectangle(pen, rct);
                //sbd.Append($"{nl}\tRectangle drawn on graphic.");
                Font fnt = new Font("Arial", 12, FontStyle.Regular);

                Point pnt = new Point(df.FaceRectangle.Left, df.FaceRectangle.Top + df.FaceRectangle.Height);
                grp.DrawString(ksd.Key, fnt, brs, pnt);
                //sbd.Append($"{nl}\tText drawn on graphic.");
            }
            string imgDirFile = imgDir + @"\outputImages\" + file.Substring(0, file.IndexOf(".")) +
                    "-" + DateTime.Now.Day + "-" + DateTime.Now.Hour.ToString() + "-"
                    + DateTime.Now.Minute.ToString() + "-" + DateTime.Now.Second.ToString()
                    + file.Substring(file.IndexOf("."));
            img.Save(imgDirFile);
            Console.Write(sbd.ToString());
        }
        public static async Task davesImageUrl(IFaceClient client, string url, string recognitionModel) {
            StringBuilder sbd = new StringBuilder();
            Console.WriteLine("Running method davesImageUrl, yee-haw!");
            IList<DetectedFace> ldf = await client.Face.DetectWithUrlAsync(url,
                returnFaceAttributes: lfat, detectionModel: DetectionModel.Detection01,
                recognitionModel: recognitionModel);
            Console.WriteLine("# of faces detected: " + ldf.Count.ToString());
            sbd.Clear(); int facCnt = 0;
            foreach (DetectedFace df in ldf) {
                //foreach (FaceAttribute fa in df.FaceAttributes)  doesn't have GetEnumerator
                Console.WriteLine($"Face #{++facCnt}:");
                Console.WriteLine($"\tSmile: {df.FaceAttributes.Smile}, ");
                KeyValuePair<string, double> ksd = df.FaceAttributes.Emotion.ToRankedList().
                    OrderByDescending(e => e.Value).First();
                Console.WriteLine($"\tThe primary emotion is {ksd.Key} with confidence {ksd.Value}");
                Console.WriteLine("\tThe other emotions are: ");
                foreach (KeyValuePair<string, double> kvp in df.FaceAttributes.Emotion.
                    ToRankedList().Where(e => e.Value != ksd.Value))
                    Console.WriteLine($"\t\t{kvp.Key} = {kvp.Value}");
                Console.WriteLine("\tGender: " + df.FaceAttributes.Gender.Value.ToString());
                Console.WriteLine($"\tEye Makup: {df.FaceAttributes.Makeup.EyeMakeup}, " +
                    $"Lip Makeup: {df.FaceAttributes.Makeup.LipMakeup}");
                Console.WriteLine($"\tHeadPose: Yaw={df.FaceAttributes.HeadPose.Yaw}, " +
                    $"Pitch={df.FaceAttributes.HeadPose.Pitch}, Roll={df.FaceAttributes.HeadPose.Roll}");
                Console.WriteLine($"\tHairColor Count={df.FaceAttributes.Hair.HairColor.Count()}");
                if (df.FaceAttributes.Hair.HairColor.Count() > 0) {
                    HairColor hcl = df.FaceAttributes.Hair.HairColor.OrderByDescending(c => c.Confidence).First();
                    Console.WriteLine($"\tThe primary Hair Color is {hcl.Color} with confidence {hcl.Confidence}");
                }
            }
        }
    }
}

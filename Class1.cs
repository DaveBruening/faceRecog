using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace ConsoleCoreFaceGitHub {
    partial class Program {
        //Since Program.cs was getting so big, I decided to continue class Program in another .cs
        static string personGroupId = Guid.NewGuid().ToString();
        static Dictionary<string, string[]> personDictionary;
        public static async Task<List<DetectedFace>> DetectFaceRecognize(
            IFaceClient faceClient, string url, string recognition_model) {
            // Detect faces from image URL. Since only recognizing, use the recognition model 1.
            // We use detection model 2 because we are not retrieving attributes.
            IList<DetectedFace> detectedFaces = await faceClient.Face.DetectWithUrlAsync(
                url, recognitionModel: recognition_model, detectionModel: DetectionModel.Detection02);
            Console.WriteLine($"{detectedFaces.Count} face(s) detected from image '{Path.GetFileName(url)}'");
            return detectedFaces.ToList();
        }
        public static async Task davesFindSimilar(IFaceClient client, string url, string recognition_model) {
            Console.WriteLine("========Gip's FIND SIMILAR========");
            Console.WriteLine();
            List<string> targetImageFileNames = new List<string> {
                "Family1-Dad1.jpg",
                "Family1-Daughter1.jpg",
                "Family1-Mom1.jpg",
                "Family1-Son1.jpg",
                "Family2-Lady1.jpg",
                "Family2-Man1.jpg",
                "Family3-Lady1.jpg",
                "Family3-Man1.jpg"
            };
            IList<KeyValuePair<Guid?, string>> ksg = new List<KeyValuePair<Guid?, string>>();
            //https://csdx.blob.core.windows.net/resources/Face/Images/findsimilar.jpg
            string sourceImageFileName = "findsimilar.jpg";
            IList<Guid?> targetFaceIds = new List<Guid?>();
            foreach (var targetImageFileName in targetImageFileNames) {
                //Detect faces from target image url
                var faces = await DetectFaceRecognize(client, $"{url}{targetImageFileName}", recognition_model);
                //targetFaceIds.Add(faces[0].FaceId.Value);
                ksg.Add(new KeyValuePair<Guid?, string>(faces[0].FaceId.Value, targetImageFileName));
            }
            //Detect faces from source image url:
            IList<DetectedFace> detectedFaces = await DetectFaceRecognize(client,
                $"{url}{sourceImageFileName}", recognition_model);
            Console.WriteLine();
            ;
            //Find a similar face(s) in the list of IDs, comparing only the first in list for testing.  
            IList<SimilarFace> similarResults = await client.Face.FindSimilarAsync(
                detectedFaces[0].FaceId.Value, null, null, ksg.Select(k => k.Key).ToList());
            //detectedFaces[0].FaceId.Value, null, null, targetFaceIds);
            Console.WriteLine($"Faces that are similar to {sourceImageFileName}:");
            foreach (var similarResult in similarResults)
                Console.WriteLine($"\tFilename=" +
                    $"{ksg.Where(g => g.Key == similarResult.FaceId).Select(s => s.Value).First()}" +
                    $", ID={similarResult.FaceId}, confidence={similarResult.Confidence}.");
            Console.WriteLine();
        }
        public static async Task davesIdentifyInPersonGroup(IFaceClient client, string url, string rcgMdl) {
            string nl = Environment.NewLine;
            StringBuilder sbd = new StringBuilder();
            //Download images from internet: 
            string imgDir = Directory.GetParent(Directory.GetParent(Directory.GetParent(
                Environment.CurrentDirectory).FullName).FullName).FullName + @"\identifyImages\";
            await client.PersonGroup.CreateAsync(personGroupId, personGroupId, recognitionModel: rcgMdl);
            Console.WriteLine("The PersistedFace GUIDs: ");
            foreach (string prsn in personDictionary.Keys) {
                Person person = await client.PersonGroupPerson.CreateAsync(personGroupId: personGroupId, name: prsn);
                foreach (string pic in personDictionary[prsn]) {
                    StreamReader srd = new StreamReader(imgDir + pic);
                    PersistedFace pfc = await client.PersonGroupPerson.AddFaceFromStreamAsync(
                        personGroupId, person.PersonId, srd.BaseStream, pic);
                    Console.WriteLine($"{pfc.PersistedFaceId} - {pic} ");
                }
            }
            await client.PersonGroup.TrainAsync(personGroupId);
            Console.WriteLine("Training Statuses:");
            while (true) {
                await Task.Delay(50);
                var trSt = await client.PersonGroup.GetTrainingStatusAsync(personGroupId);
                Console.WriteLine("\t" + trSt.Status.ToString());
                if (trSt.Status == TrainingStatusType.Succeeded)
                    break;
            }
            List<Guid?> guidsPplInFamilyPic = new List<Guid?>();
            StreamReader strmFullFamily = new StreamReader(imgDir + "identification1.jpg");
            IList<DetectedFace> idf = await client.Face.DetectWithStreamAsync(strmFullFamily.BaseStream, recognitionModel:
                RecognitionModel.Recognition03, detectionModel: DetectionModel.Detection02);
            List<DetectedFace> ldf = idf.ToList();
            int cntFace = 0;
            Console.WriteLine($"The {ldf.Count} detected faces in the full family pic: ");
            foreach (DetectedFace dfc in ldf)
                guidsPplInFamilyPic.Add(dfc.FaceId);
            IList<IdentifyResult> iir = await client.Face.IdentifyAsync(guidsPplInFamilyPic, personGroupId);
            foreach (IdentifyResult ir in iir) {
                Console.WriteLine($"\tFace{++cntFace}");
                foreach (IdentifyCandidate icd in ir.Candidates.OrderByDescending(c => c.Confidence)) {
                    Person prsn = await client.PersonGroupPerson.GetAsync(personGroupId, icd.PersonId);
                    Console.Write($"\t\t{prsn.Name}: {icd.Confidence} {nl}");
                }
            }
        }
    }
}

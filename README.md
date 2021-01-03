## Facial recognition using Microsoft's Cognitive Services
Hello.  This code is the result of my tinkering with: 
Quickstart: Use the Face client library - Azure Cognitive Services | Microsoft Docs
which is located [here](https://docs.microsoft.com/en-us/azure/cognitive-services/Face/Quickstarts/client-libraries?pivots=programming-language-csharp&tabs=visual-studio)
<br/>While the Quickstart uses only images on the internet, I added the ability to uses images local to your machine.<br/>
Method **davesImageStream** draws face rectangle(s) on the image file and stores it in the outputImages directory.  The inputImages directory contains images I downloaded from the internet that I thought had decent facial expressions.<br/>
Methods **davesImageUrl**, **davesFindSimilar** and **davesIdentifyInPersonGroup** uses some LINQ lambda expression razzmatazz :smiley: to embellish the Quickstart's methods. 


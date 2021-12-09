using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.CognitiveServices.Speech;
using System.Diagnostics;
using Azure;
using Azure.AI.FormRecognizer;
using Azure.AI.FormRecognizer.Models;
using System.Collections.Generic;
using System.IO;

namespace SayTheName
{
    class Program
    {
        private static SpeechConfig speechConfig;
        private static FormRecognizerClient formClient;

        private static string speechSvcKey;
        private static string speechSvcRegion;
        private static string formSvcKey;
        private static string formSvcRegion;
        private static string photoPath;
        private static string photoCommand;
        private static string photoCommandParam;

        static async Task Main(string[] args)
        {
            try
            {
                // Get config settings from AppSettings
                IConfigurationBuilder builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
                IConfigurationRoot configuration = builder.Build();
                speechSvcKey = configuration["SpeechServiceKey"];
                speechSvcRegion = configuration["SpeechServiceRegion"];
                formSvcKey = configuration["FormServiceKey"];
                formSvcRegion = configuration["FormServiceRegion"];
                photoPath = configuration["PhotoPath"];
                photoCommand = configuration["PhotoCommand"];
                photoCommandParam = configuration["PhotoCommandParam"];

                // Configure speech service
                speechConfig = SpeechConfig.FromSubscription(speechSvcKey, speechSvcRegion);
                speechConfig.SpeechSynthesisVoiceName = "en-GB-RyanNeural";
                using (SpeechSynthesizer speechSynthesizer = new SpeechSynthesizer(speechConfig))
                {
                    Console.WriteLine("Ready to use speech service in " + speechConfig.Region);

                    AzureKeyCredential credential = new AzureKeyCredential(formSvcKey);
                    formClient = new FormRecognizerClient(new Uri($"https://{formSvcRegion}.api.cognitive.microsoft.com"), credential);
                   
                    while (true)
                    {
                        // Get camera input
                        var name = GetName();
                        if (!String.IsNullOrEmpty(name))
                        {
                            //Speech Synthesis Voice
                            await SayTheNameAsync(speechSynthesizer,name);
                            Console.WriteLine($"Pronounced: {name}");
                        }
                        else
                        {
                            Console.WriteLine("Businesses card does not detected.");
                        }

                        Console.WriteLine("Wait for 5 second...");
                        await Task.Delay(5000);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

        private static string GetName()
        {
            TakePicture();
            return AnalyzeBusinessCard(formClient, photoPath).Result; 
        }

        public static async Task SayTheNameAsync(SpeechSynthesizer speechSynthesizer, string responseText)
        {
            SpeechSynthesisResult speak = await speechSynthesizer.SpeakTextAsync(responseText);
            if (speak.Reason != ResultReason.SynthesizingAudioCompleted)
            {
                Console.WriteLine($"Synthesized: {responseText}");
                Console.WriteLine(speak.Reason);
            }
        }

        public static void TakePicture()
        {
            //fswebcam -r 1280x720 --no-banner image3.jpg

            var info = new ProcessStartInfo();
            info.FileName = photoCommand;
            info.Arguments = photoCommandParam;

            info.UseShellExecute = false;
            info.CreateNoWindow = false;

            info.RedirectStandardOutput = true;
            info.RedirectStandardError = true;

            Console.WriteLine("taking image");

            var p = Process.Start(info);
            p.WaitForExit();

            Console.WriteLine("Image taken");
        }

        private static async Task<string> AnalyzeBusinessCard(FormRecognizerClient recognizerClient, string path)
        {
            string contactName = "";
            string contactTitle = "";
            string contactCompany = "";
            using (var card = File.OpenRead(path))
            {

                RecognizedFormCollection businessCards = await recognizerClient.StartRecognizeBusinessCards(card).WaitForCompletionAsync();

                foreach (RecognizedForm businessCard in businessCards)
                {
                    FormField ContactNamesField;
                    if (businessCard.Fields.TryGetValue("ContactNames", out ContactNamesField))
                    {
                        if (ContactNamesField.Value.ValueType == FieldValueType.List)
                        {
                            foreach (FormField contactNameField in ContactNamesField.Value.AsList())
                            {
                                contactName = contactNameField.ValueData.Text;
                                Console.WriteLine($"\tContact Name: {contactNameField.ValueData.Text}");

                                if (contactNameField.Value.ValueType == FieldValueType.Dictionary)
                                {
                                    IReadOnlyDictionary<string, FormField> contactNameFields = contactNameField.Value.AsDictionary();

                                    FormField firstNameField;
                                    if (contactNameFields.TryGetValue("FirstName", out firstNameField))
                                    {
                                        if (firstNameField.Value.ValueType == FieldValueType.String && firstNameField.Confidence >= 0.9)
                                        {
                                            string firstName = firstNameField.Value.AsString();

                                            Console.WriteLine($"\tFirst Name: '{firstName}', with confidence {firstNameField.Confidence}");
                                        }
                                    }

                                    FormField lastNameField;
                                    if (contactNameFields.TryGetValue("LastName", out lastNameField))
                                    {
                                        if (lastNameField.Value.ValueType == FieldValueType.String && lastNameField.Confidence >= 0.9)
                                        {
                                            string lastName = lastNameField.Value.AsString();

                                            Console.WriteLine($"\tLast Name: '{lastName}', with confidence {lastNameField.Confidence}");
                                        }
                                    }
                                }
                            }
                        }
                    }

                    FormField jobTitlesFields;
                    if (businessCard.Fields.TryGetValue("JobTitles", out jobTitlesFields))
                    {
                        if (jobTitlesFields.Value.ValueType == FieldValueType.List)
                        {
                            foreach (FormField jobTitleField in jobTitlesFields.Value.AsList())
                            {
                                if (jobTitleField.Value.ValueType == FieldValueType.String && jobTitleField.Confidence >= 0.9)
                                {
                                    contactTitle = jobTitleField.Value.AsString();
                                    Console.WriteLine($"\tJob Title: '{contactTitle}', with confidence {jobTitleField.Confidence}");
                                }
                            }
                        }
                    }

                    FormField departmentFields;
                    if (businessCard.Fields.TryGetValue("Departments", out departmentFields))
                    {
                        if (departmentFields.Value.ValueType == FieldValueType.List)
                        {
                            foreach (FormField departmentField in departmentFields.Value.AsList())
                            {
                                if (departmentField.Value.ValueType == FieldValueType.String && departmentField.Confidence >= 0.9)
                                {
                                    string department = departmentField.Value.AsString();

                                    Console.WriteLine($"\tDepartment: '{department}', with confidence {departmentField.Confidence}");
                                }
                            }
                        }
                    }

                    FormField emailFields;
                    if (businessCard.Fields.TryGetValue("Emails", out emailFields))
                    {
                        if (emailFields.Value.ValueType == FieldValueType.List)
                        {
                            foreach (FormField emailField in emailFields.Value.AsList())
                            {
                                if (emailField.Value.ValueType == FieldValueType.String && emailField.Confidence >= 0.9)
                                {
                                    string email = emailField.Value.AsString();

                                    Console.WriteLine($"\tEmail: '{email}', with confidence {emailField.Confidence}");
                                }
                            }
                        }
                    }

                    FormField websiteFields;
                    if (businessCard.Fields.TryGetValue("Websites", out websiteFields))
                    {
                        if (websiteFields.Value.ValueType == FieldValueType.List)
                        {
                            foreach (FormField websiteField in websiteFields.Value.AsList())
                            {
                                if (websiteField.Value.ValueType == FieldValueType.String && websiteField.Confidence >= 0.9)
                                {
                                    string website = websiteField.Value.AsString();

                                    Console.WriteLine($"\tWebsite: '{website}', with confidence {websiteField.Confidence}");
                                }
                            }
                        }
                    }

                    FormField mobilePhonesFields;
                    if (businessCard.Fields.TryGetValue("MobilePhones", out mobilePhonesFields))
                    {
                        if (mobilePhonesFields.Value.ValueType == FieldValueType.List)
                        {
                            foreach (FormField mobilePhoneField in mobilePhonesFields.Value.AsList())
                            {
                                if (mobilePhoneField.Value.ValueType == FieldValueType.PhoneNumber && mobilePhoneField.Confidence >= 0.9)
                                {
                                    string mobilePhone = mobilePhoneField.Value.AsPhoneNumber();

                                    Console.WriteLine($"\tMobile phone number: '{mobilePhone}', with confidence {mobilePhoneField.Confidence}");
                                }
                            }
                        }
                    }

                    FormField otherPhonesFields;
                    if (businessCard.Fields.TryGetValue("OtherPhones", out otherPhonesFields))
                    {
                        if (otherPhonesFields.Value.ValueType == FieldValueType.List)
                        {
                            foreach (FormField otherPhoneField in otherPhonesFields.Value.AsList())
                            {
                                if (otherPhoneField.Value.ValueType == FieldValueType.PhoneNumber && otherPhoneField.Confidence >= 0.9)
                                {
                                    string otherPhone = otherPhoneField.Value.AsPhoneNumber();

                                    Console.WriteLine($"\tOther phone number: '{otherPhone}', with confidence {otherPhoneField.Confidence}");
                                }
                            }
                        }
                    }

                    FormField faxesFields;
                    if (businessCard.Fields.TryGetValue("Faxes", out faxesFields))
                    {
                        if (faxesFields.Value.ValueType == FieldValueType.List)
                        {
                            foreach (FormField faxField in faxesFields.Value.AsList())
                            {
                                if (faxField.Value.ValueType == FieldValueType.PhoneNumber && faxField.Confidence >= 0.9)
                                {
                                    string fax = faxField.Value.AsPhoneNumber();

                                    Console.WriteLine($"\tFax phone number: '{fax}', with confidence {faxField.Confidence}");
                                }
                            }
                        }
                    }

                    FormField addressesFields;
                    if (businessCard.Fields.TryGetValue("Addresses", out addressesFields))
                    {
                        if (addressesFields.Value.ValueType == FieldValueType.List)
                        {
                            foreach (FormField addressField in addressesFields.Value.AsList())
                            {
                                if (addressField.Value.ValueType == FieldValueType.String && addressField.Confidence >= 0.9)
                                {
                                    string address = addressField.Value.AsString();

                                    Console.WriteLine($"\tAddress: '{address}', with confidence {addressField.Confidence}");
                                }
                            }
                        }
                    }

                    FormField companyNamesFields;
                    if (businessCard.Fields.TryGetValue("CompanyNames", out companyNamesFields))
                    {
                        if (companyNamesFields.Value.ValueType == FieldValueType.List)
                        {
                            foreach (FormField companyNameField in companyNamesFields.Value.AsList())
                            {
                                if (companyNameField.Value.ValueType == FieldValueType.String && companyNameField.Confidence >= 0.9)
                                {
                                    contactCompany = companyNameField.Value.AsString();

                                    Console.WriteLine($"\tCompany name: '{contactCompany}', with confidence {companyNameField.Confidence}");
                                }
                            }
                        }
                    }
                }
                card.Close();
            }

            var result = contactName;
            if (contactTitle != "")
                result += $" {contactTitle}";
            if (contactCompany != "")
                result += $" from {contactCompany}";

            return result;
        }

    }
}

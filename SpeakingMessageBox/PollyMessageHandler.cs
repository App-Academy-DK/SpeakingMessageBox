using Amazon.Polly;
using Amazon.Polly.Model;
using Microsoft.Maui.Controls;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;


namespace SpeakingMessageBox
{
    public class PollyMessageHandler
    {
        private readonly IAmazonPolly _pollyClient;
        private readonly string _voiceId;

        public PollyMessageHandler(string voiceId = "Joanna")
        {
            _pollyClient = new AmazonPollyClient();
            _voiceId = voiceId;
        }

        public async Task ShowMessageAsync(string message, MessageType messageType)
        {
            string ssmlMessage = CreateSSMLMessage(message, messageType);
            await SpeakMessageAsync(ssmlMessage);
            await Application.Current.MainPage.DisplayAlert("Besked", message, "OK");
        }

        private string CreateSSMLMessage(string message, MessageType messageType)
        {
            string emphasis;
            switch (messageType)
            {
                case MessageType.Fejl:
                    emphasis = "<emphasis level=\"strong\">" + message + "</emphasis>";
                    break;
                case MessageType.Spørgsmål:
                    emphasis = "<prosody pitch=\"high\">" + message + "</prosody>";
                    break;
                case MessageType.Information:
                    emphasis = "<prosody rate=\"medium\">" + message + "</prosody>";
                    break;
                default:
                    emphasis = message;
                    break;
            }

            return $"<speak>{emphasis}</speak>";
        }

        private async Task SpeakMessageAsync(string ssmlMessage)
        {
            var synthesizeSpeechRequest = new SynthesizeSpeechRequest
            {
                OutputFormat = OutputFormat.Mp3,
                VoiceId = _voiceId,
                TextType = Amazon.Polly.TextType.Ssml,
                Text = ssmlMessage
            };

            try
            {
                var response = await _pollyClient.SynthesizeSpeechAsync(synthesizeSpeechRequest);
                using (var stream = response.AudioStream)
                {
                    string tempFilePath = Path.Combine(Path.GetTempPath(), "polly_output.mp3");
                    using (var fileStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write))
                    {
                        await stream.CopyToAsync(fileStream);
                    }

                    // Afspil lyden ved hjælp af Windows Media Player
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = tempFilePath,
                        UseShellExecute = true
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Fejl ved tekst-til-tale-syntese: " + ex.Message);
            }
        }
    }

    public enum MessageType
    {
        Fejl,
        Spørgsmål,
        Information
    }
}
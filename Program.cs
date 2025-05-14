using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;

namespace ConsoleApp5
{
    class Program
    {
        static void Main(string[] args)
        {


            new logo(){ };
            var chatbot = new ChatbotSystem();
            chatbot.Start();
        }

    }

    public class ChatbotSystem
    {
        private readonly UserDetails _userDetails;
        private readonly ResponseService _responseService;
        private readonly UserMemory _memory;
        private readonly SentimentAnalyzer _sentimentAnalyzer;
        private readonly ChatbotEngine _chatbotEngine;

        public ChatbotSystem()
        {
            _userDetails = new UserDetails();
            _responseService = new ResponseService();
            _memory = new UserMemory();
            _sentimentAnalyzer = new SentimentAnalyzer();
            _chatbotEngine = new ChatbotEngine(_responseService, _memory, _sentimentAnalyzer);
        }

        public void Start()
        {
            _userDetails.GreetUser();
            var userName = _userDetails.StoreName();
            var userSurname = _userDetails.StoreSurname();
            _userDetails.GenerateUserReport(userName, userSurname);

            while (true)
            {
                Console.WriteLine("\nAsk me about cybersecurity topics (type 'exit' to quit): ");
                var userInput = Console.ReadLine();

                if (userInput?.ToLower() == "exit")
                    break;

                var response = _chatbotEngine.ProcessInput(userInput);
                Console.WriteLine(response);
            }
        }
    }

    public class UserDetails
    {
        public void GreetUser()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Hello! Welcome to the Cybersecurity Assistant.");
            Console.ResetColor();
        }

        public string StoreName()
        {
            Console.Write("Enter your first name: ");
            return Console.ReadLine()?.Trim() ?? "";
        }

        public string StoreSurname()
        {
            Console.Write("Enter your surname: ");
            return Console.ReadLine()?.Trim() ?? "";
        }

        public void GenerateUserReport(string name, string surname)
        {
            File.WriteAllText("User_Report.txt", $"Hi {name} {surname}, welcome!");
            Console.WriteLine("User report saved to 'User_Report.txt'.");
        }
    }

    public class ChatbotEngine
    {
        private readonly ResponseService _responseService;
        private readonly UserMemory _memory;
        private readonly SentimentAnalyzer _sentimentAnalyzer;
        private string _currentTopic;
        private bool _inFollowUp = false;

        public ChatbotEngine(ResponseService responseService, UserMemory memory, SentimentAnalyzer sentimentAnalyzer)
        {
            _responseService = responseService;
            _memory = memory;
            _sentimentAnalyzer = sentimentAnalyzer;
        }

        public string ProcessInput(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return "I didn't catch that. Could you please rephrase?";

            if (TryHandleMemoryInput(input, out var memoryResponse))
                return memoryResponse;

            var sentiment = _sentimentAnalyzer.DetectSentiment(input);
            var topic = DetectTopic(input) ?? _currentTopic ?? "general";

            if (_inFollowUp && (input.ToLower().Contains("more") || input.ToLower().Contains("explain")))
            {
                var followUpResponse = _responseService.GetFollowUpResponse(topic);
                return _memory.PersonalizeResponse(followUpResponse);
            }

            var response = _responseService.GetResponse(topic, sentiment);
            _currentTopic = topic;
            _inFollowUp = true;

            var empatheticIntro = _sentimentAnalyzer.GetEmpatheticResponse(sentiment, topic);
            return _memory.PersonalizeResponse($"{empatheticIntro}\n\n{response}");
        }

        private bool TryHandleMemoryInput(string input, out string response)
        {
            var lower = input.ToLower();

            if (lower.Contains("my name is"))
            {
                var name = input.Substring(lower.IndexOf("my name is") + 10).Trim().Split(' ')[0];
                if (!string.IsNullOrEmpty(name))
                {
                    _memory.Remember("name", name);
                    response = $"Nice to meet you, {name}! How can I help with cybersecurity today?";
                    return true;
                }
            }

            if (lower.Contains("interested in"))
            {
                var interest = input.Substring(lower.IndexOf("interested in") + 13).Trim().Split('.')[0];
                _memory.Remember("interest", interest);
                _currentTopic = interest;
                response = $"I'll remember you're interested in {interest}. What would you like to know?";
                return true;
            }

            if (lower.Contains("remember me"))
            {
                if (_memory.TryRecall("name", out var name))
                    response = $"Of course I remember you, {name}! How can I help you today?";
                else
                    response = "I don't think we've been properly introduced yet. What's your name?";
                return true;
            }

            response = null;
            return false;
        }

        private string DetectTopic(string input)
        {
            return _responseService.GetAvailableTopics()
                .FirstOrDefault(topic => input.IndexOf(topic, StringComparison.OrdinalIgnoreCase) >= 0);
        }
    }

    public class UserMemory
    {
        private readonly Dictionary<string, string> _memory = new Dictionary<string, string>();

        public void Remember(string key, string value) => _memory[key] = value;

        public bool TryRecall(string key, out string value) => _memory.TryGetValue(key, out value);

        public string PersonalizeResponse(string response)
        {
            if (TryRecall("name", out var name) && !string.IsNullOrWhiteSpace(name))
                return $"Hey {name}, {response}";

            if (TryRecall("interest", out var interest) && !string.IsNullOrWhiteSpace(interest))
                return $"Since you're interested in {interest}, here's something relevant:\n\n{response}";

            return response;
        }
    }

    public class SentimentAnalyzer
    {
        private readonly Dictionary<string, string[]> _sentimentKeywords = new Dictionary<string, string[]>
        {
            { "happy", new[] { "happy", "great", "awesome", "thanks", "thank you", "good" } },
            { "sad", new[] { "sad", "depressed", "unhappy", "miserable" } },
            { "worried", new[] { "worried", "anxious", "concerned", "nervous" } },
            { "angry", new[] { "angry", "mad", "furious", "annoyed" } },
            { "confused", new[] { "confused", "don't understand", "not sure", "lost", "stuck" } }
        };

        public string DetectSentiment(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return "neutral";

            var lower = input.ToLowerInvariant();
            foreach (var kvp in _sentimentKeywords)
            {
                if (kvp.Value.Any(word => lower.Contains(word)))
                    return kvp.Key;
            }

            return (input.Contains("!") || input.Contains("?")) ? "curious" : "neutral";
        }

        public string GetEmpatheticResponse(string sentiment, string topic)
        {
            switch (sentiment)
            {
                case "happy": return $"I'm glad you're feeling good about {topic}!";
                case "sad": return $"I'm sorry you're feeling down about {topic}. I'm here to help.";
                case "worried": return $"It's okay to feel worried about {topic}. Let's walk through it together.";
                case "angry": return $"I understand your frustration with {topic}. Let's try to make it clearer.";
                case "confused": return $"{topic} can be tricky. Let me simplify it for you.";
                default: return $"Let's explore more about {topic}.";
            }
        }
    }

    public class ResponseService
    {
        private readonly Dictionary<string, List<string>> _responses = new Dictionary<string, List<string>>
        {
            { "password", new List<string>
                {
                    "Use complex passwords with letters, numbers, and symbols.",
                    "Never reuse passwords across sites. Use a password manager!",
                    "Passphrases like 'correct horse battery staple' are strong and easy to remember."
                }
            },
            { "phishing", new List<string>
                {
                    "Verify sender email addresses carefully.",
                    "Hover over links before clicking to see their destination.",
                    "Legit companies won’t ask for sensitive info via email."
                }
            },
            { "privacy", new List<string>
                {
                    "Check app permissions regularly.",
                    "Use privacy browsers like Firefox or Brave.",
                    "Use VPNs when on public WiFi."
                }
            },
            { "malware", new List<string>
                {
                    "Keep antivirus updated and scan regularly.",
                    "Don't download files from unknown sources.",
                    "Update your OS and apps to avoid vulnerabilities."
                }
            },
            { "general", new List<string>
                {
                    "Cybersecurity is essential for everyone.",
                    "Stay vigilant online with smart habits.",
                    "Would you like help with passwords, privacy, or phishing?"
                }
            }
        };

        private readonly Dictionary<string, string> _followUpResponses = new Dictionary<string, string>
        {
            { "password", "Try password managers like LastPass or Bitwarden for better security." },
            { "phishing", "Check out the Anti-Phishing Working Group for more info." },
            { "privacy", "Consider using Tor for maximum privacy." },
            { "malware", "Use tools like Malwarebytes for extra protection." },
            { "general", "CISA.gov has great cybersecurity learning resources." }
        };

        public string GetResponse(string topic, string sentiment)
        {
            if (_responses.ContainsKey(topic))
            {
                var responseList = _responses[topic];
                return responseList.FirstOrDefault() ?? "Let's talk about cybersecurity!";
            }
            return "I'm here to help with any cybersecurity questions.";
        }

        public string GetFollowUpResponse(string topic)
        {
            return _followUpResponses.ContainsKey(topic) ? _followUpResponses[topic] : "What else would you like to know?";
        }

        public List<string> GetAvailableTopics()
        {
            return _responses.Keys.ToList();
        }
    }
}

using System.Text;
using Newtonsoft.Json;

namespace Mir2Assistant.Services
{
    public static class HuoshanAIHelper
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private static readonly Dictionary<string, List<ChatMessage>> _conversationHistory = new();
        private static readonly Random _random = new Random();
        private const int MAX_MESSAGES = 50;
        // private const string API_KEY = "5e6d2940-a9e3-4125-98ac-f443d97e7437";
        private const string API_KEY = "a426e068-0c4d-4fe2-9de6-1faf340bbc65";
        //private const string MODEL = "doubao-seed-1-6-flash-250615";
        private const string MODEL = "kimi-k2-250905";

        public class ChatMessage
        {
            public string Role { get; set; } = "";
            public string Content { get; set; } = "";
        }

        private static readonly string[] ChatPrompts = {
            "随便想日常生活一句话, 要网络风格, 别聊烂大街的, 别带标点, 30字以内",
            "说点有趣的日常话题, 网络用语, 简短点, 30字以内",
            "聊点轻松的话题, 像网友聊天一样, 不要太正式, 30字以内",
            "随便扯点什么, 要自然点, 别像机器人, 30字以内",
            "说点日常的闲话, 网络风格, 简洁点, 30字以内"
        };

        /// <summary>
        /// 重新开始对话，清除指定实例的对话历史
        /// </summary>
        /// <param name="instanceId">实例ID，如果为空则使用当前线程ID</param>
        public static void RestartConversation(string instanceId = null)
        {
            instanceId ??= Thread.CurrentThread.ManagedThreadId.ToString();
            
            if (_conversationHistory.ContainsKey(instanceId))
            {
                _conversationHistory[instanceId].Clear();
                Console.WriteLine($"[HuoshanAI] 已重新开始对话 (实例ID: {instanceId})");
            }
        }

        /// <summary>
        /// 清除所有对话历史
        /// </summary>
        public static void ClearAllConversations()
        {
            _conversationHistory.Clear();
            Console.WriteLine("[HuoshanAI] 已清除所有对话历史");
        }

        /// <summary>
        /// 无参数多轮对话函数，自动生成随机聊天内容并维护对话历史
        /// </summary>
        /// <returns>AI回复</returns>
        public static async Task<string> ChatAsync()
        {
            try
            {
                // 自动生成实例ID（基于调用线程）
                var instanceId = Thread.CurrentThread.ManagedThreadId.ToString();
                
                // 获取或创建该实例的对话历史
                if (!_conversationHistory.ContainsKey(instanceId))
                {
                    _conversationHistory[instanceId] = new List<ChatMessage>();
                }

                var history = _conversationHistory[instanceId];

                // 如果消息数量超过限制，重置对话历史
                if (history.Count >= MAX_MESSAGES)
                {
                    RestartConversation(instanceId);
                }

                string prompt;
                // 如果是第一次对话，使用随机提示词；否则使用继续对话的提示
                if (history.Count == 0)
                {
                    prompt = ChatPrompts[_random.Next(ChatPrompts.Length)];
                }
                else
                {
                    prompt = "你有1万种人格, 随机一个人格, 不要暴露出你是哪个人格, 随机选择回复或重新开始话题，简短点，性格要多变";
                }

                // 添加用户消息到历史
                history.Add(new ChatMessage { Role = "user", Content = prompt });

                // 构建请求体，包含完整的对话历史
                var messages = history.Select(h => new { role = h.Role, content = h.Content }).ToArray();

                var requestBody = new
                {
                    model = MODEL,
                    messages = messages,
                    max_tokens = 200,
                    temperature = 0.9,
                    parameters = new { enable_thinking = false }
                };

                var json = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {API_KEY}");

                // 开始计时
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                
                var response = await _httpClient.PostAsync("https://ark.cn-beijing.volces.com/api/v3/chat/completions", content);
                
                // 停止计时
                stopwatch.Stop();
                var elapsed = stopwatch.ElapsedMilliseconds;
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    dynamic result = JsonConvert.DeserializeObject(responseContent);
                    
                    var aiReply = result?.choices?[0]?.message?.content?.ToString() ?? "没有回复";
                    
                    // 添加AI回复到历史
                    history.Add(new ChatMessage { Role = "assistant", Content = aiReply });
                    
                    // 输出耗时到控制台
                    Console.WriteLine($"[HuoshanAI] API调用耗时: {elapsed}ms");
                    
                    return aiReply;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    //return $"API调用失败: {response.StatusCode} - {errorContent}";
                    return "+";
                }
            }
            catch (Exception ex)
            {
                //return $"错误: {ex.Message}";
                return "++";
            }
        }
    }
}

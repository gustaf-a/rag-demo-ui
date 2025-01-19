using Microsoft.AspNetCore.Mvc;
using AiDemos.Api.Generation;
using Shared.Models;

namespace AiDemos.Api.Controllers.RagDemo;

/// <summary>
/// A controller which handles chat requests, for example a simple chat request or a data driven RAG chat request
/// </summary>
[ApiController]
[Route("generation")]
public class GenerationController(ILogger<GenerationController> _logger, IConfiguration configuration, IGenerationHandler _chatHandler) : ControllerBase
{
    /// <summary>
    /// Gets a chat response.
    /// If SearchOptions are included, search results will be included for the model to use for generation.
    /// If ProvidedDocumentSources are included, they will be used instead of searching.
    /// </summary>
    /// <remarks>
    /// #  Examples
    /// 
    /// ## Simple chat request with chat options high temperature for more creative answers:
    /// ```   
    /// {  
    ///    "chatMessages": [
    ///    {  
    ///        "role": "user",  
    ///        "content": "What do you think about RAG systems for LLMs? Keep it short."  
    ///    }  
    ///    ],
    ///    "chatOptions": {  
    ///        "temperature": 1
    ///    }
    /// }  
    /// ```
    /// 
    /// ## RAG chat request:
    /// ```   
    /// {  
    ///    "chatMessages": [
    ///    {  
    ///        "role": "user",  
    ///        "content": "How do I pair to my Nimbus headphones?"  
    ///    }  
    ///    ],
    ///    "searchOptions": {
    ///        "embeddingsTableName": "embeddings2",
    ///        "itemsToRetrieve": 3
    ///    }  
    /// }  
    /// ```
    /// 
    /// ## RAG chat request with 1 plugin:
    /// ```   
    /// {  
    ///    "chatMessages": [
    ///    {  
    ///        "role": "user",  
    ///        "content": "How long until Jane's next meeting?"  
    ///    }  
    ///    ],  
    ///    "chatOptions": {  
    ///        "temperature": 0.2,  
    ///        "pluginsToUse": [
    ///            "DatePlugin"
    ///        ]  
    ///    },  
    ///    "searchOptions": {
    ///        "embeddingsTableName": "embeddings1",
    ///        "itemsToRetrieve": 3
    ///    }  
    /// }
    /// ```
    /// 
    /// ## Chat request with search plugin letting model decide how to search for data
    /// This is less reliable as the model doesn't always understand when it needs to search for information.
    /// The good thing is that the model can make multiple searches for different things.
    /// 
    /// ```
    /// {  
    ///    "chatMessages": [
    ///    {  
    ///        "role": "user",  
    ///        "content": "What's the difference between our current Q1 sales and the Q3 targets? Search in the database."  
    ///    }  
    ///    ],  
    ///    "chatOptions": {  
    ///        "temperature": 0.2,  
    ///        "pluginsToUse": [
    ///             "DatePlugin",
    ///             "SearchDatabasePlugin"
    ///        ]  
    ///    }  
    /// }
    /// ```
    /// There's a big risk here that the bot doesn't understand that it needs to search multiple times for different things.
    /// For example the search phrase could become "difference between sales" which will not do much.
    /// Which probably will not get anything useful.
    /// Using a system message can help here, but this is a multi stage question more suited for an agent assistant.
    /// 
    /// ## RAG chat request with 1 plugin without auto invoking of plugins. For human-in-the-loop or animating message progress
    /// ```
    /// {
    ///     "chatMessages": [
    ///     {
    ///         "role": "user",
    ///         "content": "How many days until Jane's next follow up?"
    ///     }
    ///     ],
    ///     "searchOptions": {
    ///         "embeddingsTableName": "embeddings1",
    ///         "itemsToRetrieve": 2,
    ///         "itemsToSkip": 0
    ///     },
    ///     "chatOptions": {
    ///         "temperature": 0.2,
    ///         "pluginsToUse": [
    ///             "DatePlugin"
    ///         ],
    ///	        "PluginsAutoInvoke":false
    ///     }
    /// }
    /// ```
    /// </remarks>
    /// <param name="chatRequest"></param>
    /// <returns></returns>
    [HttpPost("get-chat-response")]
    public async Task<ActionResult<ChatResponse>> GetChatResponse([FromBody] ChatRequest chatRequest)
    {
        var response = await _chatHandler.GetChatResponse(chatRequest);

        return Ok(response);
    }

    /// <summary>
    /// Continues a chat using the chat history object.
    /// This is a bit experimental, but should allow human in the loop and/or step-by-step animations of what the model is doing in the background.
    /// When model is finished it will return an Assistant-message with content.
    /// If this chathistory is returned in another reply then code will see metaData:FinishReason:Stop and will return an empty chathistory object as a reply.
    /// 
    /// </summary>
    /// <remarks>
    /// ## First continuation, the model wants to make 2 tool calls and human has had a chance to adjust input arguments if need be
    /// 
    /// Important to send in the ChatRequest, otherwise automatic invokation will be assumed and no plugins will be available for the model then.
    /// 
    /// ```
    ///     {
    ///         "previousChatHistoryJson": "[{\"Role\":{\"Label\":\"user\"},\"Items\":[{\"$type\":\"TextContent\",\"Text\":\"How many days until Jane\\u0027s next follow up?\"}]},{\"Role\":{\"Label\":\"user\"},\"Items\":[{\"$type\":\"TextContent\",\"Text\":\"\\u003Csources to use\\u003E\\r\\n19900530_Jane_Smith.md: Context:\\r\\nThe chunk refers to the scheduling of a follow-up appointment for Jane Smith after her recent thyroid follow-up visit on October 15, 2024, during which her TSH levels showed improvement, necessitating continued monitoring of her thyroid function.\\r\\n\\r\\nInformation:\\r\\nNext follow-up scheduled in 3 months.\\r\\n19900530_Jane_Smith.md: Context:\\r\\nThe chunk refers to a plan made during a visit on September 1, 2024, when Jane Smith underwent a thyroid function test. The results indicated borderline high TSH levels, leading to an adjustment in her levothyroxine dosage. The doctor scheduled a follow-up visit in 6 weeks to reassess her thyroid function and monitor the effect of the medication adjustment.\\r\\n\\r\\nInformation:\\r\\nFollow-up in 6 weeks for reassessment.\\r\\n\\r\\n\\u003C/sources\\u003E\"}]},{\"Role\":{\"Label\":\"Assistant\"},\"Items\":[{\"$type\":\"FunctionCallContent\",\"Id\":\"call_ACvF0dT078Wac5f1W4IC0D6v\",\"PluginName\":\"DatePlugin\",\"FunctionName\":\"add_days_to_date\",\"Arguments\":{\"date\":\"2024-10-15\",\"days\":\"90\"}},{\"$type\":\"FunctionCallContent\",\"Id\":\"call_xXZa6F6rW4p2S51CoWoC12kx\",\"PluginName\":\"DatePlugin\",\"FunctionName\":\"add_days_to_date\",\"Arguments\":{\"date\":\"2024-09-01\",\"days\":\"42\"}}],\"ModelId\":\"gpt-4o-mini\",\"Metadata\":{\"Id\":\"chatcmpl-AZN8zRxCXGm6q7ixeCwIdSJ8iArJw\",\"CreatedAt\":\"2024-11-30T19:10:09+00:00\",\"SystemFingerprint\":\"fp_04751d0b65\",\"Usage\":{\"OutputTokenCount\":70,\"InputTokenCount\":401,\"TotalTokenCount\":471,\"OutputTokenDetails\":null},\"Refusal\":null,\"FinishReason\":\"ToolCalls\",\"ContentTokenLogProbabilities\":[],\"ChatResponseMessage.FunctionToolCalls\":[{\"Kind\":0,\"FunctionName\":\"DatePlugin-add_days_to_date\",\"FunctionArguments\":{},\"Id\":\"call_ACvF0dT078Wac5f1W4IC0D6v\"},{\"Kind\":0,\"FunctionName\":\"DatePlugin-add_days_to_date\",\"FunctionArguments\":{},\"Id\":\"call_xXZa6F6rW4p2S51CoWoC12kx\"}]}}]",
    ///         "chatOptions": {
    ///                 "temperature": 0.2,
    ///                 "pluginsToUse": [
    ///                     "DatePlugin"
    ///                 ],
    ///	                "PluginsAutoInvoke":false
    ///         }
    ///     }
    /// ```
    /// 
    /// ## Second continuation after tool calls.  
    /// 
    /// The model receives the result from the function calls and can continue to reason.  
    /// Here it's important to send the same chatOptions in again, otherwise defaults without plugins and auto invoke set to true will be used.
    /// 
    /// ```
    ///     {
    ///         "previousChatHistoryJson": "[{\"Role\":{\"Label\":\"user\"},\"Items\":[{\"$type\":\"TextContent\",\"Text\":\"How many days until Jane\\u0027s next follow up?\"}]},{\"Role\":{\"Label\":\"user\"},\"Items\":[{\"$type\":\"TextContent\",\"Text\":\"\\u003Csources to use\\u003E\\r\\n19900530_Jane_Smith.md: Context:\\r\\nThe chunk refers to the scheduling of a follow-up appointment for Jane Smith after her recent thyroid follow-up visit on October 15, 2024, during which her TSH levels showed improvement, necessitating continued monitoring of her thyroid function.\\r\\n\\r\\nInformation:\\r\\nNext follow-up scheduled in 3 months.\\r\\n19900530_Jane_Smith.md: Context:\\r\\nThe chunk refers to a plan made during a visit on September 1, 2024, when Jane Smith underwent a thyroid function test. The results indicated borderline high TSH levels, leading to an adjustment in her levothyroxine dosage. The doctor scheduled a follow-up visit in 6 weeks to reassess her thyroid function and monitor the effect of the medication adjustment.\\r\\n\\r\\nInformation:\\r\\nFollow-up in 6 weeks for reassessment.\\r\\n\\r\\n\\u003C/sources\\u003E\"}]},{\"Role\":{\"Label\":\"Assistant\"},\"Items\":[{\"$type\":\"FunctionCallContent\",\"Id\":\"call_ACvF0dT078Wac5f1W4IC0D6v\",\"PluginName\":\"DatePlugin\",\"FunctionName\":\"add_days_to_date\",\"Arguments\":{\"date\":\"2024-10-15\",\"days\":\"90\"}},{\"$type\":\"FunctionCallContent\",\"Id\":\"call_xXZa6F6rW4p2S51CoWoC12kx\",\"PluginName\":\"DatePlugin\",\"FunctionName\":\"add_days_to_date\",\"Arguments\":{\"date\":\"2024-09-01\",\"days\":\"42\"}}],\"ModelId\":\"gpt-4o-mini\",\"Metadata\":{\"Id\":\"chatcmpl-AZN8zRxCXGm6q7ixeCwIdSJ8iArJw\",\"CreatedAt\":\"2024-11-30T19:10:09\\u002B00:00\",\"SystemFingerprint\":\"fp_04751d0b65\",\"Usage\":{\"OutputTokenCount\":70,\"InputTokenCount\":401,\"TotalTokenCount\":471,\"OutputTokenDetails\":null},\"Refusal\":null,\"FinishReason\":\"ToolCalls\",\"ContentTokenLogProbabilities\":[],\"ChatResponseMessage.FunctionToolCalls\":[{\"Kind\":0,\"FunctionName\":\"DatePlugin-add_days_to_date\",\"FunctionArguments\":{},\"Id\":\"call_ACvF0dT078Wac5f1W4IC0D6v\"},{\"Kind\":0,\"FunctionName\":\"DatePlugin-add_days_to_date\",\"FunctionArguments\":{},\"Id\":\"call_xXZa6F6rW4p2S51CoWoC12kx\"}]}},{\"Role\":{\"Label\":\"tool\"},\"Items\":[{\"$type\":\"FunctionResultContent\",\"CallId\":\"call_ACvF0dT078Wac5f1W4IC0D6v\",\"PluginName\":\"DatePlugin\",\"FunctionName\":\"add_days_to_date\",\"Result\":\"2025-01-13\"}]},{\"Role\":{\"Label\":\"tool\"},\"Items\":[{\"$type\":\"FunctionResultContent\",\"CallId\":\"call_xXZa6F6rW4p2S51CoWoC12kx\",\"PluginName\":\"DatePlugin\",\"FunctionName\":\"add_days_to_date\",\"Result\":\"2024-10-13\"}]}]",
    ///         "ChatRequest": 
    ///         {
    ///             "chatOptions": {
    ///                 "temperature": 0.2,
    ///                 "pluginsToUse": [
    ///                     "DatePlugin"
    ///                 ],
    ///	                "PluginsAutoInvoke": false
    ///             }
    ///         }
    ///     }
    /// ```
    /// 
    /// ## Later message when model has done all necessary tool calls and is ready to generate a response to user  
    /// 
    /// The model receives the result from the function calls, but user wishes to add more sources and another question.  
    /// Here it's important to send the same chatRequestOptions in again for the plugins and searching.  
    /// Defaults are used for invokations, so the result after this will contain finished message.  
    /// 
    /// ```
    ///     {
    ///         "previousChatHistoryJson": "[{\"Role\":{\"Label\":\"user\"},\"Items\":[{\"$type\":\"TextContent\",\"Text\":\"How many days until Jane\\u0027s next follow up?\"}]},{\"Role\":{\"Label\":\"user\"},\"Items\":[{\"$type\":\"TextContent\",\"Text\":\"\\u003Csources to use\\u003E\\r\\n19900530_Jane_Smith.md: Context:\\r\\nThe chunk refers to the scheduling of a follow-up appointment for Jane Smith after her recent thyroid follow-up visit on October 15, 2024, during which her TSH levels showed improvement, necessitating continued monitoring of her thyroid function.\\r\\n\\r\\nInformation:\\r\\nNext follow-up scheduled in 3 months.\\r\\n19900530_Jane_Smith.md: Context:\\r\\nThe chunk refers to a plan made during a visit on September 1, 2024, when Jane Smith underwent a thyroid function test. The results indicated borderline high TSH levels, leading to an adjustment in her levothyroxine dosage. The doctor scheduled a follow-up visit in 6 weeks to reassess her thyroid function and monitor the effect of the medication adjustment.\\r\\n\\r\\nInformation:\\r\\nFollow-up in 6 weeks for reassessment.\\r\\n\\r\\n\\u003C/sources\\u003E\"}]},{\"Role\":{\"Label\":\"Assistant\"},\"Items\":[{\"$type\":\"FunctionCallContent\",\"Id\":\"call_ACvF0dT078Wac5f1W4IC0D6v\",\"PluginName\":\"DatePlugin\",\"FunctionName\":\"add_days_to_date\",\"Arguments\":{\"date\":\"2024-10-15\",\"days\":\"90\"}},{\"$type\":\"FunctionCallContent\",\"Id\":\"call_xXZa6F6rW4p2S51CoWoC12kx\",\"PluginName\":\"DatePlugin\",\"FunctionName\":\"add_days_to_date\",\"Arguments\":{\"date\":\"2024-09-01\",\"days\":\"42\"}}],\"ModelId\":\"gpt-4o-mini\",\"Metadata\":{\"Id\":\"chatcmpl-AZN8zRxCXGm6q7ixeCwIdSJ8iArJw\",\"CreatedAt\":\"2024-11-30T19:10:09\\u002B00:00\",\"SystemFingerprint\":\"fp_04751d0b65\",\"Usage\":{\"OutputTokenCount\":70,\"InputTokenCount\":401,\"TotalTokenCount\":471,\"OutputTokenDetails\":null},\"Refusal\":null,\"FinishReason\":\"ToolCalls\",\"ContentTokenLogProbabilities\":[],\"ChatResponseMessage.FunctionToolCalls\":[{\"Kind\":0,\"FunctionName\":\"DatePlugin-add_days_to_date\",\"FunctionArguments\":{},\"Id\":\"call_ACvF0dT078Wac5f1W4IC0D6v\"},{\"Kind\":0,\"FunctionName\":\"DatePlugin-add_days_to_date\",\"FunctionArguments\":{},\"Id\":\"call_xXZa6F6rW4p2S51CoWoC12kx\"}]}},{\"Role\":{\"Label\":\"tool\"},\"Items\":[{\"$type\":\"FunctionResultContent\",\"CallId\":\"call_ACvF0dT078Wac5f1W4IC0D6v\",\"PluginName\":\"DatePlugin\",\"FunctionName\":\"add_days_to_date\",\"Result\":\"2025-01-13\"}]},{\"Role\":{\"Label\":\"tool\"},\"Items\":[{\"$type\":\"FunctionResultContent\",\"CallId\":\"call_xXZa6F6rW4p2S51CoWoC12kx\",\"PluginName\":\"DatePlugin\",\"FunctionName\":\"add_days_to_date\",\"Result\":\"2024-10-13\"}]},{\"Role\":{\"Label\":\"Assistant\"},\"Items\":[{\"$type\":\"FunctionCallContent\",\"Id\":\"call_D3ZvtOhc9beQLWsqLoQnIr7o\",\"PluginName\":\"DatePlugin\",\"FunctionName\":\"get_current_date\",\"Arguments\":{}},{\"$type\":\"FunctionCallContent\",\"Id\":\"call_95X4xwEeawPAzuhiZfrSDerv\",\"PluginName\":\"DatePlugin\",\"FunctionName\":\"get_days_between_dates\",\"Arguments\":{\"startDate\":\"2024-10-13\",\"endDate\":\"2024-10-15\"}},{\"$type\":\"FunctionCallContent\",\"Id\":\"call_880iFUZD4arpMCvJ2jgEsN7g\",\"PluginName\":\"DatePlugin\",\"FunctionName\":\"get_days_between_dates\",\"Arguments\":{\"startDate\":\"2025-01-13\",\"endDate\":\"2024-10-15\"}}],\"ModelId\":\"gpt-4o-mini\",\"Metadata\":{\"Id\":\"chatcmpl-AZNGRDwi7DGEdF16NSFnQlTtlh07d\",\"CreatedAt\":\"2024-11-30T19:17:51\\u002B00:00\",\"SystemFingerprint\":\"fp_04751d0b65\",\"Usage\":{\"OutputTokenCount\":98,\"InputTokenCount\":497,\"TotalTokenCount\":595,\"OutputTokenDetails\":null},\"Refusal\":null,\"FinishReason\":\"ToolCalls\",\"ContentTokenLogProbabilities\":[],\"ChatResponseMessage.FunctionToolCalls\":[{\"Kind\":0,\"FunctionName\":\"DatePlugin-get_current_date\",\"FunctionArguments\":{},\"Id\":\"call_D3ZvtOhc9beQLWsqLoQnIr7o\"},{\"Kind\":0,\"FunctionName\":\"DatePlugin-get_days_between_dates\",\"FunctionArguments\":{},\"Id\":\"call_95X4xwEeawPAzuhiZfrSDerv\"},{\"Kind\":0,\"FunctionName\":\"DatePlugin-get_days_between_dates\",\"FunctionArguments\":{},\"Id\":\"call_880iFUZD4arpMCvJ2jgEsN7g\"}]}},{\"Role\":{\"Label\":\"tool\"},\"Items\":[{\"$type\":\"FunctionResultContent\",\"CallId\":\"call_D3ZvtOhc9beQLWsqLoQnIr7o\",\"PluginName\":\"DatePlugin\",\"FunctionName\":\"get_current_date\",\"Result\":\"2024-11-30\"}]},{\"Role\":{\"Label\":\"tool\"},\"Items\":[{\"$type\":\"FunctionResultContent\",\"CallId\":\"call_95X4xwEeawPAzuhiZfrSDerv\",\"PluginName\":\"DatePlugin\",\"FunctionName\":\"get_days_between_dates\",\"Result\":2}]},{\"Role\":{\"Label\":\"tool\"},\"Items\":[{\"$type\":\"FunctionResultContent\",\"CallId\":\"call_880iFUZD4arpMCvJ2jgEsN7g\",\"PluginName\":\"DatePlugin\",\"FunctionName\":\"get_days_between_dates\",\"Result\":-90}]}]",
    ///         "ChatRequest": 
    ///         {
    ///             "chatMessages": [
    ///             {
    ///                 "role": "user",
    ///                 "content": "And how many days until John's next follow up?"
    ///             }
    ///             ],
    ///             "searchOptions": {
    ///                 "embeddingsTableName": "embeddings1",
    ///                 "itemsToRetrieve": 2,
    ///                 "itemsToSkip": 0
    ///             },
    ///             "chatOptions": {
    ///                 "temperature": 0.2,
    ///                 "pluginsToUse": [
    ///                     "DatePlugin"
    ///                 ],
    ///	                "PluginsAutoInvoke": true
    ///             }
    ///         }
    ///     }
    /// ```
    /// 
    /// 
    /// ## Example reply from the assistant after tool calls are done.
    /// ```
    /// {
    ///   "chatMessages": [
    ///     {
    ///       "role": "Assistant",
    ///       "content": "Jane's next follow-up appointments are as follows:\n\n1. **Follow-up in 6 weeks** from September 1, 2024, is scheduled for **October 13, 2024**. There are **2 days** until this appointment.\n   \n2. **Follow-up in 3 months** from October 15, 2024, is scheduled for **January 13, 2025**. This appointment is **90 days** away, but it is after the October appointment.\n\nSo, Jane's next follow-up is in **2 days** on **October 13, 2024**."
    ///     }
    ///   ],
    ///   "citations": [],
    ///   "chatHistoryJson": "[{\"Role\":{\"Label\":\"user\"},\"Items\":[{\"$type\":\"TextContent\",\"Text\":\"How many days until Jane\\u0027s next follow up?\"}]},{\"Role\":{\"Label\":\"user\"},\"Items\":[{\"$type\":\"TextContent\",\"Text\":\"\\u003Csources to use\\u003E\\r\\n19900530_Jane_Smith.md: Context:\\r\\nThe chunk refers to the scheduling of a follow-up appointment for Jane Smith after her recent thyroid follow-up visit on October 15, 2024, during which her TSH levels showed improvement, necessitating continued monitoring of her thyroid function.\\r\\n\\r\\nInformation:\\r\\nNext follow-up scheduled in 3 months.\\r\\n19900530_Jane_Smith.md: Context:\\r\\nThe chunk refers to a plan made during a visit on September 1, 2024, when Jane Smith underwent a thyroid function test. The results indicated borderline high TSH levels, leading to an adjustment in her levothyroxine dosage. The doctor scheduled a follow-up visit in 6 weeks to reassess her thyroid function and monitor the effect of the medication adjustment.\\r\\n\\r\\nInformation:\\r\\nFollow-up in 6 weeks for reassessment.\\r\\n\\r\\n\\u003C/sources\\u003E\"}]},{\"Role\":{\"Label\":\"Assistant\"},\"Items\":[{\"$type\":\"FunctionCallContent\",\"Id\":\"call_ACvF0dT078Wac5f1W4IC0D6v\",\"PluginName\":\"DatePlugin\",\"FunctionName\":\"add_days_to_date\",\"Arguments\":{\"date\":\"2024-10-15\",\"days\":\"90\"}},{\"$type\":\"FunctionCallContent\",\"Id\":\"call_xXZa6F6rW4p2S51CoWoC12kx\",\"PluginName\":\"DatePlugin\",\"FunctionName\":\"add_days_to_date\",\"Arguments\":{\"date\":\"2024-09-01\",\"days\":\"42\"}}],\"ModelId\":\"gpt-4o-mini\",\"Metadata\":{\"Id\":\"chatcmpl-AZN8zRxCXGm6q7ixeCwIdSJ8iArJw\",\"CreatedAt\":\"2024-11-30T19:10:09\\u002B00:00\",\"SystemFingerprint\":\"fp_04751d0b65\",\"Usage\":{\"OutputTokenCount\":70,\"InputTokenCount\":401,\"TotalTokenCount\":471,\"OutputTokenDetails\":null},\"Refusal\":null,\"FinishReason\":\"ToolCalls\",\"ContentTokenLogProbabilities\":[],\"ChatResponseMessage.FunctionToolCalls\":[{\"Kind\":0,\"FunctionName\":\"DatePlugin-add_days_to_date\",\"FunctionArguments\":{},\"Id\":\"call_ACvF0dT078Wac5f1W4IC0D6v\"},{\"Kind\":0,\"FunctionName\":\"DatePlugin-add_days_to_date\",\"FunctionArguments\":{},\"Id\":\"call_xXZa6F6rW4p2S51CoWoC12kx\"}]}},{\"Role\":{\"Label\":\"tool\"},\"Items\":[{\"$type\":\"FunctionResultContent\",\"CallId\":\"call_ACvF0dT078Wac5f1W4IC0D6v\",\"PluginName\":\"DatePlugin\",\"FunctionName\":\"add_days_to_date\",\"Result\":\"2025-01-13\"}]},{\"Role\":{\"Label\":\"tool\"},\"Items\":[{\"$type\":\"FunctionResultContent\",\"CallId\":\"call_xXZa6F6rW4p2S51CoWoC12kx\",\"PluginName\":\"DatePlugin\",\"FunctionName\":\"add_days_to_date\",\"Result\":\"2024-10-13\"}]},{\"Role\":{\"Label\":\"Assistant\"},\"Items\":[{\"$type\":\"FunctionCallContent\",\"Id\":\"call_D3ZvtOhc9beQLWsqLoQnIr7o\",\"PluginName\":\"DatePlugin\",\"FunctionName\":\"get_current_date\",\"Arguments\":{}},{\"$type\":\"FunctionCallContent\",\"Id\":\"call_95X4xwEeawPAzuhiZfrSDerv\",\"PluginName\":\"DatePlugin\",\"FunctionName\":\"get_days_between_dates\",\"Arguments\":{\"startDate\":\"2024-10-13\",\"endDate\":\"2024-10-15\"}},{\"$type\":\"FunctionCallContent\",\"Id\":\"call_880iFUZD4arpMCvJ2jgEsN7g\",\"PluginName\":\"DatePlugin\",\"FunctionName\":\"get_days_between_dates\",\"Arguments\":{\"startDate\":\"2025-01-13\",\"endDate\":\"2024-10-15\"}}],\"ModelId\":\"gpt-4o-mini\",\"Metadata\":{\"Id\":\"chatcmpl-AZNGRDwi7DGEdF16NSFnQlTtlh07d\",\"CreatedAt\":\"2024-11-30T19:17:51\\u002B00:00\",\"SystemFingerprint\":\"fp_04751d0b65\",\"Usage\":{\"OutputTokenCount\":98,\"InputTokenCount\":497,\"TotalTokenCount\":595,\"OutputTokenDetails\":null},\"Refusal\":null,\"FinishReason\":\"ToolCalls\",\"ContentTokenLogProbabilities\":[],\"ChatResponseMessage.FunctionToolCalls\":[{\"Kind\":0,\"FunctionName\":\"DatePlugin-get_current_date\",\"FunctionArguments\":{},\"Id\":\"call_D3ZvtOhc9beQLWsqLoQnIr7o\"},{\"Kind\":0,\"FunctionName\":\"DatePlugin-get_days_between_dates\",\"FunctionArguments\":{},\"Id\":\"call_95X4xwEeawPAzuhiZfrSDerv\"},{\"Kind\":0,\"FunctionName\":\"DatePlugin-get_days_between_dates\",\"FunctionArguments\":{},\"Id\":\"call_880iFUZD4arpMCvJ2jgEsN7g\"}]}},{\"Role\":{\"Label\":\"tool\"},\"Items\":[{\"$type\":\"FunctionResultContent\",\"CallId\":\"call_D3ZvtOhc9beQLWsqLoQnIr7o\",\"PluginName\":\"DatePlugin\",\"FunctionName\":\"get_current_date\",\"Result\":\"2024-11-30\"}]},{\"Role\":{\"Label\":\"tool\"},\"Items\":[{\"$type\":\"FunctionResultContent\",\"CallId\":\"call_95X4xwEeawPAzuhiZfrSDerv\",\"PluginName\":\"DatePlugin\",\"FunctionName\":\"get_days_between_dates\",\"Result\":2}]},{\"Role\":{\"Label\":\"tool\"},\"Items\":[{\"$type\":\"FunctionResultContent\",\"CallId\":\"call_880iFUZD4arpMCvJ2jgEsN7g\",\"PluginName\":\"DatePlugin\",\"FunctionName\":\"get_days_between_dates\",\"Result\":-90}]},{\"Role\":{\"Label\":\"assistant\"},\"Items\":[{\"$type\":\"TextContent\",\"Text\":\"Jane\\u0027s next follow-up appointments are as follows:\\n\\n1. **Follow-up in 6 weeks** from September 1, 2024, is scheduled for **October 13, 2024**. There are **2 days** until this appointment.\\n   \\n2. **Follow-up in 3 months** from October 15, 2024, is scheduled for **January 13, 2025**. This appointment is **90 days** away, but it is after the October appointment.\\n\\nSo, Jane\\u0027s next follow-up is in **2 days** on **October 13, 2024**.\"}]}]"
    /// }
    /// ```
    /// 
    /// </remarks>
    /// <param name="continueChatRequest"></param>
    /// <returns></returns>
    [HttpPost("continue-chat")]
    public async Task<ActionResult<ChatResponse>> ContinueChatResponse([FromBody] ContinueChatRequest continueChatRequest)
    {
        var response = await _chatHandler.ContinueChatResponse(continueChatRequest);

        return Ok(response);
    }
}

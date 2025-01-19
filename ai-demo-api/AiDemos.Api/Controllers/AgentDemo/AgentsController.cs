using AgentDemo;
using Microsoft.AspNetCore.Mvc;
using Shared.Models;
using Shared.Models.Agents;

namespace AiDemos.Api.Controllers.AgentDemo;

/// <summary>
/// A controller for the agents and tasks
/// </summary>
[ApiController]
[Route("agents")]
public class AgentsController(ILogger<AgentsController> _logger, IAgentHandler _agentHandler) : ControllerBase
{
    /// <summary>
    /// Creates an agent
    /// </summary>
    /// <remarks>
    /// # Examples  
    /// 
    /// ## Simple agent creation
    /// 
    /// ```   
    /// {
    ///  "name": "PoemCreator",
    ///  "description": "Creates a poem of whatever the input is.",
    ///  "options": {
    ///    "AgentSystemPrompt": "Rewrite whatever is sent to you as a short poem. Do not add any new information or respond to anything, only respond with a poem that is a rephrasing of the input."
    ///  }
    /// }
    /// ```
    ///
    /// ## Create a task finished reviewer agent  
    /// ```
    /// {
    ///   "name": "TaskFinishedReviewer",
    ///   "description": "Determines if a task is finished or not.",
    ///   "options": {
    ///     "AgentSystemPrompt": "Determine if a task is finished or not. If a task has been finished respond ONLY with the words 'Task finished'."
    ///   }
    /// }
    /// ```
    /// 
    /// TODO plugins
    /// ## Create a task finished reviewer agent using a plugin
    /// {
    ///   "name": "Task Finished Reviewer Agent",
    ///   "description": "Helps determine if a task is finished or not.",
    ///   "options": {
    ///     "AgentSystemPrompt": "Determine if a task is finished or not. If a task has been finished respond ONLY with the words 'Task finished'."
    ///   }
    /// }
    /// 
    /// ## Create a task finished reviewer agent
    /// {
    ///   "name": "Task Finished Reviewer Agent",
    ///   "description": "Helps determine if a task is finished or not.",
    ///   "options": {
    ///     "AgentSystemPrompt": "Determine if a task is finished or not. If a task has been finished respond ONLY with the words 'Task finished'."
    ///   }
    /// }
    /// 
    /// ## Creating an agent with optional plugin use  
    /// 
    /// ## Creating an agent with required plugin use  
    /// QualityAssuranceReportingPlugin
    /// 
    /// </remarks>
    [HttpPost]
    public async Task<ActionResult<ProcessInfo>> CreateAgent([FromBody] Agent agent)
    {
        if (agent is null)
        {
            return BadRequest("Agent to create is required.");
        }

        var createdAgent = await _agentHandler.CreateAgent(agent);

        return Ok(createdAgent);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Agent>>> GetAgents()
    {
        var agents = await _agentHandler.GetAgents();

        return Ok(agents);
    }

    /// <summary>
    /// Creates and starts an agent task
    /// </summary>
    /// <remarks>
    /// # Examples  
    /// 
    /// ## Simple task for poem creation with default termination strategy and default selection strategy
    /// 
    /// ```   
    /// {
    ///   "taskPrompt": "Create a poem from this: Sunsets are beautiful and make me happy.",
    ///   "Agents": [
    ///     "PoemCreator"
    ///   ],
    ///   "name": "Sunset poem creation"
    /// }
    /// ```
    /// 
    /// ## Create task with termination strategy
    /// ``` 
    /// {
    ///   "taskPrompt": "Create a poem from this: Sunsets are beautiful and make me happy.",
    ///   "Agents": [
    ///     "PoemCreator",
    ///     "TaskFinishedReviewer"
    ///   ],
    ///   "name": "Sunset poem creation",
    ///   "terminationStrategyInfo": {
    ///     "type": "PromptFunktion",
    ///     "payload": {
    ///       "Agents": ["TaskFinishedReviewer"]
    ///     }
    ///   }
    /// }
    /// ```
    ///
    /// ## Task creating files with multiple poems with prompt-based termination strategy and default selection strategy
    /// TODO
    /// 
    /// ## Creating an agent with optional plugin use  
    ///{
    /// ## Creating an agent with required plugin use  
    /// QualityAssuranceReportingPlugin
    /// 
    /// </remarks>
    [HttpPost("task")]
    public async Task<ActionResult<AgentTask>> StartAgentTask([FromBody] StartAgentTaskRequest startAgentTaskRequest)
    {
        var agentTask = await _agentHandler.StartAgentTask(startAgentTaskRequest);

        return Ok(agentTask);
    }

    //TODO Solve task with agents
    //olika agents har tillgång till olika plugins? Går att configurera.
    //Vad är annorlunda än RAG? 
    //RAG handlar just om RAG, det ska inte växa och bli större.
    //AgentDemoAPI kan få innehålla processer etc 


    //Process exempel:
    //    Use Case: Automated Report Generation with Human Approval

    //Agent A(Data Plugin): Extract data from a database.
    //Agent B(Analysis Plugin): Perform data analysis.
    //Agent C(Report Generator): Generate a draft report.
    //Human Approval: Review and approve the report.
    //Agent D (Email Plugin): Send the approved report via email.


    //Plugins som använder RAGDemo-API

    //Databas-plugin som sparar i databasen

    //plugin för human approval

    //plugin för human feedback

    //plugin för agentic review

    //proper logging of actions and steps taken

    //V2: more event driven and can follow completion. With processes maybe that update db.

}

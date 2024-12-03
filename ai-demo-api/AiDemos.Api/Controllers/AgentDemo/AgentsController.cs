using Microsoft.AspNetCore.Mvc;

namespace AiDemos.Api.Controllers.AgentDemo;

/// <summary>
/// A controller for the available plugins
/// </summary>
[ApiController]
[Route("agents")]
public class AgentsController(ILogger<AgentsController> _logger) : ControllerBase
{
    [HttpGet("get-names")]
    public async Task<IActionResult> GetNames()
    {
        //TODO Handler gets names of agents available

        return Ok();
    }

    //TODO Solve task with agents
    //olika agents har tillgång till olika plugins? Går att configurera.
    //Vad är annorlunda än RAG? 
    //RAG handlar just om RAG, det ska inte växa och bli större.
    //AgentDemoAPI kan få innehålla processer etc 


    //Workflow exempel:
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

using Microsoft.AspNetCore.Mvc;
using ProcessDemo.Processes;
using Shared.Models;

namespace AiDemos.Api.Controllers.ProcessDemo;

/// <summary>
/// A controller for the processes.
/// A process is a set of steps that can be started by a user or executed automatically.
/// First a Process with ProcessSteps is created.
/// Then when a process is started, a ProcessInstance with ProcessInstanceSteps are created for each execution of the process.
/// Each execution can have different options and inputs.
/// </summary>
[ApiController]
[Route("processes")]
public class ProcessesController(ILogger<ProcessesController> _logger, IProcessHandler _processHandler) : ControllerBase
{
    #region Process

    [HttpGet("templates/{role}/{guid}")]
    public async Task<ActionResult<ProcessInfo>> GetProcess(string role, Guid guid)
    {
        var process = await _processHandler.GetProcess(role, guid);

        return Ok(process);
    }

    [HttpGet("templates/{role}")]
    public async Task<ActionResult<IEnumerable<ProcessInfo>>> GetProcesses(string role)
    {
        var processes = await _processHandler.GetProcesses(role);

        return Ok(processes);
    }

    [HttpPost("templates")]
    public async Task<ActionResult<ProcessInfo>> CreateProcess([FromBody] ProcessInfo processInfo)
    {
        if (processInfo == null)
        {
            return BadRequest("Process information is required.");
        }

        var createdProcess = await _processHandler.CreateProcessAsync(processInfo);
        return Ok(createdProcess);
    }

    [HttpPut("templates/{role}/{guid}")]
    public async Task<ActionResult<ProcessInfo>> UpdateProcess(string role, Guid guid, [FromBody] ProcessInfo processInfo)
    {
        if (processInfo == null || guid == Guid.Empty)
        {
            return BadRequest("Invalid process information or GUID.");
        }

        var updatedProcess = await _processHandler.UpdateProcessAsync(role, guid, processInfo);
        if (updatedProcess == null)
        {
            return NotFound();
        }

        return Ok(updatedProcess);
    }

    [HttpDelete("templates/{role}/{guid}")]
    public async Task<IActionResult> DeleteProcess(string role, Guid guid)
    {
        var result = await _processHandler.DeleteProcessAsync(role, guid);
        if (!result)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpPost("start")]
    public async Task<ActionResult<ProcessInstance>> StartProcess([FromBody] StartProcessRequest startProcessRequest)
    {
        var process = await _processHandler.StartProcessExecution(startProcessRequest);

        return Ok(process);
    }

    #endregion Process

    #region ProcessStep

    [HttpGet("templates/steps/{stepId}")]
    public async Task<ActionResult<ProcessStepInfo>> GetProcessStep(Guid stepId)
    {
        var step = await _processHandler.GetProcessStepAsync(stepId);
        if (step == null)
        {
            return NotFound();
        }
        return Ok(step);
    }

    [HttpPost("templates/steps")]
    public async Task<ActionResult<ProcessStepInfo>> CreateProcessStep([FromBody] ProcessStepInfo processStepInfo)
    {
        var process = await _processHandler.CreateProcessStepAsync(processStepInfo);

        return Ok(process);
    }

    [HttpPut("templates/steps/{stepId}")]
    public async Task<ActionResult<ProcessStepInfo>> UpdateProcessStep(Guid stepId, [FromBody] ProcessStepInfo processStepInfo)
    {
        if (processStepInfo == null || stepId == Guid.Empty)
        {
            return BadRequest("Invalid process step information or Step ID.");
        }

        var updatedStep = await _processHandler.UpdateProcessStepAsync(stepId, processStepInfo);
        if (updatedStep == null)
        {
            return NotFound();
        }

        return Ok(updatedStep);
    }

    [HttpDelete("templates/steps/{stepId}")]
    public async Task<IActionResult> DeleteProcessStep(Guid stepId)
    {
        var result = await _processHandler.DeleteProcessStepAsync(stepId);
        if (!result)
        {
            return NotFound();
        }

        return NoContent();
    }

    #endregion ProcessStep

    #region ProcessInstance

    [HttpGet("{instanceId}")]
    public async Task<ActionResult<ProcessInstance>> GetProcessInstance(Guid instanceId)
    {
        var instanceInfo = await _processHandler.GetProcessInstance(instanceId);

        return Ok(instanceInfo);
    }

    [HttpGet("instances/{userId}")]
    public async Task<ActionResult<IEnumerable<ProcessInstance>>> GetProcessInstances(string userId)
    {
        var instanceInfos = await _processHandler.GetProcessInstances(userId);

        return Ok(instanceInfos);
    }

    [HttpPost("instances")]
    public async Task<ActionResult<ProcessInstance>> CreateProcessInstance([FromBody] ProcessInstance processInstance)
    {
        if (processInstance == null)
        {
            return BadRequest("Process instance information is required.");
        }

        var createdInstance = await _processHandler.CreateProcessInstanceAsync(processInstance);

        return Ok(createdInstance);
    }

    [HttpPut("instances/{instanceId}")]
    public async Task<ActionResult<ProcessInstance>> UpdateProcessInstance(Guid instanceId, [FromBody] ProcessInstance processInstance)
    {
        if (processInstance == null || instanceId == Guid.Empty)
        {
            return BadRequest("Invalid process instance information or Instance ID.");
        }

        var updatedInstance = await _processHandler.UpdateProcessInstanceAsync(instanceId, processInstance);
        if (updatedInstance == null)
        {
            return NotFound();
        }

        return Ok(updatedInstance);
    }

    [HttpDelete("instances/{instanceId}")]
    public async Task<IActionResult> DeleteProcessInstance(Guid instanceId)
    {
        var result = await _processHandler.DeleteProcessInstanceAsync(instanceId);
        if (!result)
        {
            return NotFound();
        }

        return NoContent();
    }

    #endregion ProcessInstance

    #region ProcessStepInstance

    [HttpGet("step-instances/{stepInstanceId}")]
    public async Task<ActionResult<ProcessStepInstance>> GetProcessStepInstance(Guid stepInstanceId)
    {
        var stepInstance = await _processHandler.GetProcessStepInstanceAsync(stepInstanceId);
        if (stepInstance == null)
        {
            return NotFound();
        }
        return Ok(stepInstance);
    }

    [HttpPost("step-instances")]
    public async Task<ActionResult<ProcessStepInstance>> CreateProcessStepInstance([FromBody] ProcessStepInstance stepInstance)
    {
        if (stepInstance == null)
        {
            return BadRequest("Process step instance information is required.");
        }

        var createdStepInstance = await _processHandler.CreateProcessStepInstanceAsync(stepInstance);

        return Ok(createdStepInstance);
    }

    [HttpPut("step-instances/{stepInstanceId}")]
    public async Task<ActionResult<ProcessStepInstance>> UpdateProcessStepInstance(Guid stepInstanceId, [FromBody] ProcessStepInstance stepInstance)
    {
        if (stepInstance == null || stepInstanceId == Guid.Empty)
        {
            return BadRequest("Invalid process step instance information or Step Instance ID.");
        }

        var updatedStepInstance = await _processHandler.UpdateProcessStepInstanceAsync(stepInstanceId, stepInstance);
        if (updatedStepInstance == null)
        {
            return NotFound();
        }

        return Ok(updatedStepInstance);
    }

    [HttpDelete("step-instances/{stepInstanceId}")]
    public async Task<IActionResult> DeleteProcessStepInstance(Guid stepInstanceId)
    {
        var result = await _processHandler.DeleteProcessStepInstanceAsync(stepInstanceId);
        if (!result)
        {
            return NotFound();
        }

        return NoContent();
    }

    #endregion ProcessStepInstance
}

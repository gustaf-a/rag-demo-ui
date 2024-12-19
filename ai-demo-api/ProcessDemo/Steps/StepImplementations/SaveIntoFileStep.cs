using Microsoft.Extensions.Logging;
using ProcessDemo.Steps.StepServices;
using Shared.Models;
using Shared.Repositories;

namespace ProcessDemo.Steps.StepImplementations;

public class SaveIntoFileStep(ILogger<ProcessStepBase> _logger, IProcessRepository processRepository, IFileService _fileService) : ProcessStepBase(_logger), IProcessStep
{
    public string StepClassName => nameof(SaveIntoFileStep);

    protected async override Task<ProcessStepExecutionResult> ExecuteInternal(ProcessStepInstance stepInstance)
    {
        //TODO Get filepath and file contents from request

        //TODO use service to save file, to cloud for example

        Console.WriteLine("Faking writing to file.");

        var payload = new ProcessPayload();

        payload.EndingState.Add(ProcessPayloadKeys.OutputFileUri, @"c:\users\desktop\fakefile.json");

        return new ProcessStepExecutionResult
        {
            IsSuccess = true,
            Payload = payload
        };
    }
}

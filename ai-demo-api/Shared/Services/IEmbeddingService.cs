﻿namespace AiDemos.Api.Services;

public interface IEmbeddingService
{
    Task<float[]> GetEmbeddings(string content);
}
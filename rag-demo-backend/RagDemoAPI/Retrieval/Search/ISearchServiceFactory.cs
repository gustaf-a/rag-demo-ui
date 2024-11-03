﻿using RagDemoAPI.Models;
using RagDemoAPI.Retrieval.Search;

namespace RagDemoAPI.Generation.LlmServices;

public interface ISearchServiceFactory
{
    ISearchService Create(ChatRequestOptions chatRequestOptions);
    ISearchService Create();
}
using AiDemos.Api.Models;

namespace AiDemos.Api.Ingestion.Chunking
{
    public class ContentChunk
    {
        public ContentChunk()
        {
        }

        public ContentChunk(EmbeddingsRowModel embeddingsRowModel)
        {
            Content = embeddingsRowModel.Content;
            EmbeddingContent = embeddingsRowModel.EmbeddingContent;
            StartIndex = (int)embeddingsRowModel.StartIndex;
            EndIndex = (int)embeddingsRowModel.EndIndex;
        }

        public string Content {  get; set; }
        public string EmbeddingContent { get; set; }
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }
    }
}
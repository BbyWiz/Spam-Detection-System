using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.ContentModerator;
using Microsoft.Azure.CognitiveServices.ContentModerator.Models;

public class ContentModeratorService
{
    private ContentModeratorClient client;

    public ContentModeratorService(string subscriptionKey, string endpoint)
    {
        client = new ContentModeratorClient(new ApiKeyServiceClientCredentials(subscriptionKey))
        {
            Endpoint = endpoint
        };
    }

    public async Task<ScreenResult> ModerateTextAsync(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentException("Text cannot be null or whitespace.", nameof(text));
        }

        string language = "eng"; // Define the language of the text content ("eng" for English)
        byte[] textBytes = Encoding.UTF8.GetBytes(text);

        using (var stream = new MemoryStream(textBytes))
        {
            try
            {
                // Screen the input text: check for profanity, classify the text, and check for PII
                return await client.TextModeration.ScreenTextAsync(
                    "text/plain",
                    stream,
                    language,
                    autocorrect: true,
                    pII: true,
                    listId: null,
                    classify: true).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"An error occurred: {ex.Message}");
                throw; // Re-throw the exception after logging it
            }
        }
    }

    public void AnalyzeScreenResult(ScreenResult result)
    {
        if (result == null)
        {
            Console.WriteLine("No result to analyze.");
            return;
        }

        if (result.Terms != null && result.Terms.Count > 0)
        {
            Console.WriteLine("Potentially offensive terms found:");
            foreach (var term in result.Terms)
            {
                Console.WriteLine($" - Term: {term.Term}, Index: {term.Index}, ListId: {term.ListId}");
            }
        }
        else
        {
            Console.WriteLine("No offensive content was found.");
        }

        if (result.Classification != null)
        {
            Console.WriteLine($"Category 1 (Potential ToS violation): {result.Classification.ReviewRecommended}");
            Console.WriteLine($"Category 2 (Potentially offensive): {result.Classification.Category2}");
            Console.WriteLine($"Category 3 (Potentially sensitive): {result.Classification.Category3}");
        }
    }
}

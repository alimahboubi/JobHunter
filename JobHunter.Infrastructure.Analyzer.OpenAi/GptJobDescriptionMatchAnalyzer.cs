using JobHunter.Domain.Job.Exceptions;
using JobHunter.Domain.Job.Services;
using OpenAI.Chat;
using OpenTelemetry.Trace;
using OpenAIClient = OpenAI.OpenAIClient;

namespace JobHunter.Infrastructure.Analyzer.OpenAi;

public class GptJobDescriptionMatchAnalyzer(OpenAIClient openAiClient, Tracer tracer) : IJobAnalyzerService
{
    public async Task<float> AnalyzeJob(string jobTitle, string? jobDescription, string? resumePath,
        CancellationToken ct)
    {
        using var span = tracer.StartActiveSpan("AnalyzeJob");
        span.SetAttribute("job title", jobTitle);
        if (string.IsNullOrWhiteSpace(jobDescription)) throw new InvalidJobDescription();
        if (string.IsNullOrWhiteSpace(resumePath)) throw new InvalidFilePath();
        // Read the resume content from the file
        string resumeContent = GetResumeContent(resumePath);

        // Calculate the match percentage
        return await CalculateMatchPercentageByGpt(jobTitle, jobDescription, resumeContent, ct);
    }


    private string GetResumeContent(string resumePath)
    {
        var path = Path.GetFullPath(resumePath);

        var isFileExist = File.Exists(path);
        if (!isFileExist) throw new InvalidFilePath();

        using var sr = new StreamReader(path);
        return sr.ReadToEnd();
    }

    private async Task<float> CalculateMatchPercentageByGpt(string jobTitle, string jobDescription,
        string resumeContent,
        CancellationToken ct)
    {
        var prompt = $@"
You are a job matching assistant tasked with evaluating how well a candidate's resume matches a specific job description.

Instructions:

1. **Thoroughly analyze** the job description to extract the key responsibilities, required skills, qualifications, and experience levels.

2. **Carefully review** the candidate's resume to identify their skills, qualifications, experiences, and any achievements relevant to the job.

3. **Compare** the candidate's profile with the job requirements, noting areas of strong alignment and gaps.

4. **Assess** the overall match by considering the relevance and significance of matching skills and experiences.

5. **Determine** a match percentage between 0 and 100, where:
   - 0% indicates no alignment,
   - 100% indicates a perfect match.

**Important**: Provide **only** the numeric match percentage without any additional text, explanations, or characters.

Your final output should be a single number like {"\"75\""} representing the match percentage.

Candidate's Resume:
{resumeContent}

Job Title:
{jobTitle}

Job Description:
{jobDescription}
        ";

        var chatClient = openAiClient.GetChatClient("gpt-4o-mini");

        var completionOptions = new ChatCompletionOptions
        {
            MaxOutputTokenCount = 10,
            Temperature = 1f,
        };

        var response = await chatClient.CompleteChatAsync(
            new[] { new UserChatMessage(prompt) },
            completionOptions,
            cancellationToken: ct);

        var assistantReply = response.Value.Content[0].Text.Trim();

        if (float.TryParse(assistantReply.TrimEnd('%'), out float matchPercentage))
        {
            return matchPercentage;
        }

        throw new CalculateMatchPercentageException();
    }
}
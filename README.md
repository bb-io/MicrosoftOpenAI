# Blackbird.io Azure OpenAI

Blackbird is the new automation backbone for the language technology industry. Blackbird provides enterprise-scale automation and orchestration with a simple no-code/low-code platform. Blackbird enables ambitious organizations to identify, vet and automate as many processes as possible. Not just localization workflows, but any business and IT process. This repository represents an application that is deployable on Blackbird and usable inside the workflow editor.

## Introduction

<!-- begin docs -->

Azure OpenAI Service provides access to advanced language models that you can customize for conversational AI, content creation, and data grounding.

## Before setting up

Before you can connect you need to make sure that:

- You have the `Resource URL` for your Azure OpenAI account.
- You know `Deployment name` and `API key` for your Azure OpenAI account.

You can find how to create and deploy an Azure OpenAI Service resource [here](https://learn.microsoft.com/en-us/azure/ai-services/openai/how-to/create-resource?pivots=web-portal).

## Connecting

1. Navigate to Apps, and identify the **Azure OpenAI** app. You can use search to find it.
2. Click _Add Connection_.
3. Name your connection for future reference e.g. 'My Azure OpenAI connection'.
4. Enter the `Resource URL`, `Deployment name`, and `API key` for your Azure OpenAI account.
5. Click _Connect_.
6. Verify that connection was added successfully.

> **_NOTE:_** Pay attention to your `Resource URL` connection parameter. Sometimes the correct url could have some path after a domain name. For example: https://example.openai.azure.com/**openai**

![connection](image/README/connection.png)

## Actions

### Chat actions

- **Generate completion**: Completes the given prompt.
- **Chat**: Gives a response given a chat message.
- **Chat with system prompt**: Gives a response given a chat message and a configurable system prompt.
- **Create summary**: Summarizes the input text.
- **Generate edit**: Edit the input text given an instruction prompt.
- **Execute Blackbird prompt**: Execute prompt generated by Blackbird's AI utilities.

### Translation-Related Actions

- **Post-edit MT**: Review MT translated text and generate a post-edited version.
- **Get translation issues**: Review text translation and generate a comment with the issue description.
- **Get MQM report**: Perform an LQA Analysis of the translation. The result will be in the MQM framework form.
- **Get MQM dimension values**: Perform an LQA Analysis of the translation. This action only returns the scores (between 1 and 10) of each dimension.
- **Translate text**: Localize the text provided.

### Audio Actions

- **Create English translation**: Generates a translation into English given an audio or video file (mp3, mp4, mpeg, mpga, m4a, wav, or webm).
- **Create transcription**: Generates a transcription given an audio or video file (mp3, mp4, mpeg, mpga, m4a, wav, or webm).

### Image Actions

- **Generate image**: Generates an image based on a prompt.

### Text Analysis Actions

- **Create embedding**: Generate an embedding for a text provided. An embedding is a list of floating point numbers that captures semantic information about the text that it represents.
- **Tokenize text**: Tokenize the text provided. Optionally specify encoding: cl100k_base (used by gpt-4, gpt-3.5-turbo, text-embedding-ada-002) or p50k_base (used by codex models, text-davinci-002, text-davinci-003).

### XLIFF Actions

Note, currently only gpt-4o version: 2024-08-06 supports structured outputs. This means that the actions that support XLIFF files can only be used with this model version. You can find the relevant information about supported models in the [Azure OpenAI documentation](https://learn.microsoft.com/en-us/azure/ai-services/openai/how-to/structured-outputs?tabs=rest).

- **Get Quality Scores for XLIFF file** Gets segment and file level quality scores for XLIFF files. Supports only version 1.2 of XLIFF currently. Optionally, you can add Threshold, New Target State and Condition input parameters to the Blackbird action to change the target state value of segments meeting the desired criteria (all three must be filled).

    Optional inputs:
	- Prompt: Add your criteria for scoring each source-target pair. If none are provided, this is replaced by _"accuracy, fluency, consistency, style, grammar and spelling"_.
	- Bucket size: Amount of translation units to process in the same request. (See dedicated section)
	- Source and Target languages: By defualt, we get these values from the XLIFF header. You can provide different values, no specific format required. 
	- Threshold: value between 0-10.
	- Condition: Criteria to filter segments whose target state will be modified.
	- New Target State: value to update target state to for filtered translation units.

    Output:
	- Average Score: aggregated score of all segment level scores.
	- Updated XLIFF file: segment level score added to extradata attribute & updated target state when instructed.

- **Post-edit XLIFF file** Updates the targets of XLIFF 1.2 files

	Optional inputs:
	- Prompt: Add your linguistic criteria for postediting targets.
	- Bucket size: Amount of translation units to process in the same request. (See dedicated section)
	- Source and Target languages: By default, we get these values from the XLIFF header. You can provide different values, no specific format required.
	- Glossary

- **Process XLIFF file** given an XLIFF file, processes each translation unit according to provided instructions (default is to translate source tags) and updates the target text for each unit. This action supports only version 1.2 of XLIFF currently.

#### Bucket size, performance and cost

XLIFF files can contain a lot of segments. Each action takes your segments and sends them to the AI app for processing. It's possible that the amount of segments is so high that the prompt exceeds the model's context window or that the model takes longer than Blackbird actions are allowed to take. This is why we have introduced the bucket size parameter. You can tweak the bucket size parameter to determine how many segments to send to the AI model at once. This will allow you to split the workload into different API calls. The trade-off is that the same context prompt needs to be send along with each request (which increases the tokens used). From experiments we have found that a bucket size of 1500 is sufficient for models like gpt-4o. That's why 1500 is the default bucket size, however other models may require different bucket sizes.

## Example

Here is an example of how you can use the `Azure OpenAI` app in a workflow:

![example](image/README/example.png)

This workflow automates the process of handling a specific trigger in Slack. Here's a step-by-step breakdown:

1. On app mentioned (Slack): The workflow starts when the app is mentioned in a Slack channel.
2. Find translation issues (Blackbird Prompts): It then uses Blackbird Prompts to find translation issues in the mentioned content.
3. Build prompt: Next, it constructs a prompt based on the identified issues.
4. Chat (Azure OpenAI): The constructed prompt is sent to Azure OpenAI to generate a response or solution.
5. Send message (Slack): Finally, the response from OpenAI is sent back as a message in the Slack channel.

## Feedback

Do you want to use this app or do you have feedback on our implementation? Reach out to us using the [established channels](https://www.blackbird.io/) or create an issue.

<!-- end docs -->

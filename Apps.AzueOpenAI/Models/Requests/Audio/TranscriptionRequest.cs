﻿using Apps.AzureOpenAI.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Files;

namespace Apps.AzureOpenAI.Models.Requests.Audio;

public class TranscriptionRequest
{       
    public FileReference File { get; set; }

    [Display("Language (ISO 639-1)")]
    [DataSource(typeof(IsoLanguageDataSourceHandler))]
    public string? Language { get; set; }

    [Display("Temperature")]
    [StaticDataSource(typeof(TemperatureDataSourceHandler))]
    public float? Temperature { get; set; }

    [Display("Prompt", Description = "An optional hint to guide the model's style or continue from a prior audio segment." +
        "The written language of the prompt should match the primary spoken language of the audio data.")]
    public string? Prompt { get; set; }
}
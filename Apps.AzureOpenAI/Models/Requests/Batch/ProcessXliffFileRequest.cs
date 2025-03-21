﻿using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Files;

namespace Apps.AzureOpenAI.Models.Requests.Batch;

public class ProcessXliffFileRequest
{
    public FileReference File { get; set; }
    
    public FileReference? Glossary { get; set; }

    [Display("Instructions", Description = "Instructions for processing the XLIFF file. For example, 'Translate the text.'")]
    public string? Instructions { get; set; }
}
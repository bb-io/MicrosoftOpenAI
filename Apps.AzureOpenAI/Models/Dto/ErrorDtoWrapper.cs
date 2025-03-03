namespace Apps.AzureOpenAI.Models.Dto;

public record ErrorRestDto(string Message, string Type, string? Code);

public record ErrorDtoWrapper(ErrorRestDto Error);
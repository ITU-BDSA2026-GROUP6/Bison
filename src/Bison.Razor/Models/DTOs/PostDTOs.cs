namespace Bison.Razor.Models.DTOs;

public record ObservationDto(
    int ObservationId,
    string Author,
    string Message,
    string Timestamp);

public record CommentDto(
    string Author,
    string Message,
    string Timestamp);

public record ProposalDto(
    string Author,
    string Message,
    string Timestamp);
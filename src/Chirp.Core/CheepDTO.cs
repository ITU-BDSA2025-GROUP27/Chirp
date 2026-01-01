namespace Chirp.Core;

/// <summary>
/// DTO for transferring cheep data between layers.
/// </summary>
public record CheepDTO(string Author, string Text, string TimeStamp);

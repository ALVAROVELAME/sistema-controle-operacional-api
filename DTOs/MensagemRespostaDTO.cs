namespace SistemaControleOperacionalApi.DTOs;

/// <summary>
/// Envelope padrao de resposta - espelha MensagemRespostaDTO (Java).
/// </summary>
public sealed record MensagemRespostaDTO(bool Sucesso, string Mensagem);
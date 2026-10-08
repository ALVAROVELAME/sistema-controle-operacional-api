namespace SistemaControleOperacionalApi.DTOs.Comum;

public class MensagemResponseDTO
{
    public bool Sucesso { get; set; } = true;
    public string Mensagem { get; set; } = string.Empty;

    public MensagemResponseDTO() { }
    public MensagemResponseDTO(string mensagem, bool sucesso = true)
    {
        Mensagem = mensagem;
        Sucesso = sucesso;
    }
}
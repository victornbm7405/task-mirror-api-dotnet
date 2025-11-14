using System.Threading.Tasks;
using Microsoft.SemanticKernel.ChatCompletion;

namespace TaskMirror.Services
{
    public class OllamaIaService
    {
        private readonly IChatCompletionService _chat;

        public OllamaIaService(IChatCompletionService chat)
        {
            _chat = chat;
        }

        // (Opcional) Método genérico que você já usou pra teste
        public async Task<string> PerguntarAsync(string mensagem)
        {
            var history = new ChatHistory();
            history.AddUserMessage(mensagem);

            var resposta = await _chat.GetChatMessageContentsAsync(history);
            return resposta[0].Content ?? string.Empty;
        }

        // 🔹 MÉTODO ESPECÍFICO: gerar feedback de tarefa
        // Aqui os valores são interpretados como MINUTOS
        public async Task<string> GerarFeedbackTarefaAsync(
            string descricaoTarefa,
            decimal tempoEstimadoHoras,
            decimal tempoRealHoras)
        {
            var history = new ChatHistory();

            var prompt = $@"
Você é um líder de equipe avaliando a execução de uma tarefa de um colaborador.

Regras:
- Escreva em português do Brasil.
- Gere um feedback curto, entre 2 e 4 frases.
- Seja educado, objetivo e construtivo.
- Não mencione que você é uma inteligência artificial ou modelo de linguagem.
- Não use palavrões.
- Se a tarefa atrasou (tempo real > tempo estimado), foque em melhoria, mas sem humilhar.
- Se a tarefa foi adiantada ou dentro do prazo (tempo real <= tempo estimado), reconheça o bom desempenho.

Dados da tarefa:
- Descrição: {descricaoTarefa}
- Tempo estimado: {tempoEstimadoHoras:F2} minutos
- Tempo real: {tempoRealHoras:F2} minutos

Agora gere apenas o texto do feedback, sem título, sem lista, apenas um pequeno parágrafo:
";

            history.AddUserMessage(prompt);

            var resposta = await _chat.GetChatMessageContentsAsync(history);
            return resposta[0].Content?.Trim() ?? string.Empty;
        }
    }
}

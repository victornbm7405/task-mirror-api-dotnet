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

        // 🔹 Método genérico usado para testes
        public async Task<string> PerguntarAsync(string mensagem)
        {
            var history = new ChatHistory();
            history.AddUserMessage(mensagem);

            var resposta = await _chat.GetChatMessageContentsAsync(history);
            return resposta[0].Content ?? string.Empty;
        }

        // 🔹 Feedback de uma tarefa (em minutos)
        public async Task<string> GerarFeedbackTarefaAsync(
            string descricaoTarefa,
            decimal tempoEstimadoMin,
            decimal tempoRealMin)
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
- Tempo estimado: {tempoEstimadoMin:F2} minutos
- Tempo real: {tempoRealMin:F2} minutos

Agora gere apenas o texto do feedback, sem título, sem lista, apenas um pequeno parágrafo:
";

            history.AddUserMessage(prompt);

            var resposta = await _chat.GetChatMessageContentsAsync(history);
            return resposta[0].Content?.Trim() ?? string.Empty;
        }

        // 🔹 Visão geral do colaborador para o líder
        public async Task<string> GerarResumoColaboradorAsync(
            string nomeColaborador,
            string textoFeedbacks)
        {
            var history = new ChatHistory();

            var prompt = $@"
Você é um gestor de pessoas analisando o desempenho de um colaborador com base em vários feedbacks já recebidos.

Nome do colaborador: {nomeColaborador}

Abaixo estão feedbacks de tarefas diferentes (mais recentes primeiro):

================ FEEDBACKS =================
{textoFeedbacks}
===========================================

Sua tarefa:
- Escreva em português do Brasil.
- Gere primeiro um parágrafo curto (3 a 5 frases) com uma visão geral do desempenho do colaborador.
- Depois, produza 3 tópicos:
  - Pontos fortes
  - Pontos de melhoria
  - Recomendações para o líder sobre como apoiar esse colaborador.
- Seja profissional, respeitoso e objetivo.
- Não mencione que você está lendo feedbacks, nem que é um modelo de linguagem.

Responda somente com o texto final, pronto para o líder ler.
";

            history.AddUserMessage(prompt);

            var resposta = await _chat.GetChatMessageContentsAsync(history);
            return resposta[0].Content?.Trim() ?? string.Empty;
        }

        // 🔹 NOVO: previsão de atraso de tarefa com base no histórico do usuário
        public async Task<string> GerarPrevisaoAtrasoAsync(
            string nomeColaborador,
            string descricaoTarefaAtual,
            decimal tempoEstimadoMinAtual,
            string? tipoTarefaAtual,
            string historicoTexto)
        {
            var history = new ChatHistory();

            var prompt = $@"
Você é um analista de produtividade que conhece o histórico de execução de tarefas de um colaborador.

Nome do colaborador: {nomeColaborador}

A seguir estão dados de tarefas anteriores (com tempo estimado x tempo real, em minutos):

================ HISTÓRICO =================
{historicoTexto}
===========================================

Agora, considere a NOVA tarefa abaixo:

- Descrição: {descricaoTarefaAtual}
- Tipo: {(string.IsNullOrWhiteSpace(tipoTarefaAtual) ? "Não informado" : tipoTarefaAtual)}
- Tempo estimado: {tempoEstimadoMinAtual:F2} minutos

Sua tarefa:
- Estimar a probabilidade dessa nova tarefa atrasar.
- Retorne uma frase com a probabilidade em porcentagem e uma justificativa breve.
- Exemplo de formato: ""Há 72% de chance desta tarefa atrasar, pois ..."".
- Escreva em português do Brasil.
- Seja objetivo, profissional e não mencione que você é uma IA.

Responda apenas com o texto final da previsão (uma ou duas frases).
";

            history.AddUserMessage(prompt);

            var resposta = await _chat.GetChatMessageContentsAsync(history);
            return resposta[0].Content?.Trim() ?? string.Empty;
        }
    }
}

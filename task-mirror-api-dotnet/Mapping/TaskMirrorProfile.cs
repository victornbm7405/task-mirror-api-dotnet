using AutoMapper;
using TaskMirror.Models;
using TaskMirror.DTOs;

namespace TaskMirror.Mapping
{
    /// <summary>
    /// Profile central do AutoMapper para o TaskMirror.
    /// </summary>
    public class TaskMirrorProfile : Profile
    {
        public TaskMirrorProfile()
        {
            // =====================================================
            // USUÁRIO
            // =====================================================

            // Entidade -> DTO de saída
            CreateMap<Usuario, UsuarioDto>();

            // DTO de criação -> Entidade
            CreateMap<UsuarioCreateDto, Usuario>();

            // Atualização parcial: só mapeia campos que vierem preenchidos no DTO
            CreateMap<UsuarioUpdateDto, Usuario>()
                .ForAllMembers(opts =>
                    opts.Condition((src, dest, srcMember) => srcMember != null));

            // =====================================================
            // TIPO TAREFA
            // =====================================================

            // Entidade -> DTO de saída
            CreateMap<TipoTarefa, TipoTarefaDto>();
            CreateMap<TipoTarefaCreateDto, TipoTarefa>();
            CreateMap<TipoTarefaUpdateDto, TipoTarefa>();

            // DTOs de entrada (se você for usar POST/PUT depois)
            CreateMap<TipoTarefaCreateDto, TipoTarefa>();
            CreateMap<TipoTarefaUpdateDto, TipoTarefa>();

            // =====================================================
            // STATUS TAREFA
            // =====================================================
            CreateMap<StatusTarefa, StatusTarefaDto>();
            CreateMap<StatusTarefaCreateDto, StatusTarefa>();
            CreateMap<StatusTarefaUpdateDto, StatusTarefa>();

            // =====================================================
            // FEEDBACK
            // =====================================================
            CreateMap<Feedback, FeedbackDto>();
        }
    }
}

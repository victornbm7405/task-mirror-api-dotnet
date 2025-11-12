using AutoMapper;
using TaskMirror.DTOs;
using TaskMirror.Models;

namespace TaskMirror.Mapping;

public class TaskMirrorProfile : Profile
{
    public TaskMirrorProfile()
    {
        CreateMap<Usuario, UsuarioDto>();
        CreateMap<UsuarioCreateDto, Usuario>()
            .ForMember(d => d.PasswordHash, o => o.MapFrom(s => s.Password)); // troque por hash real depois
        CreateMap<UsuarioUpdateDto, Usuario>()
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

        CreateMap<TipoTarefa, TipoTarefaDto>();
        CreateMap<TipoTarefaCreateDto, TipoTarefa>();

        CreateMap<StatusTarefa, StatusTarefaDto>();
        CreateMap<StatusTarefaCreateDto, StatusTarefa>();

        CreateMap<Tarefa, TarefaDto>();
        CreateMap<TarefaCreateDto, Tarefa>();
        CreateMap<TarefaUpdateDto, Tarefa>()
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

        CreateMap<Feedback, FeedbackDto>();
    }
}

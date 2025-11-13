using AutoMapper;
using TaskMirror.Models;
using TaskMirror.DTOs;

namespace TaskMirror.Mapping
{
    // Profile mínimo só para manter o AutoMapper registrado.
    // Não mapeamos Tarefa/Usuario/etc. aqui porque as respostas
    // são projetadas diretamente via LINQ no controller.
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

            // DTO de atualização -> Entidade (ignora campos nulos)
            CreateMap<UsuarioUpdateDto, Usuario>()
                .ForAllMembers(opt =>
                    opt.Condition((src, dest, srcMember) => srcMember != null)
                );

            // =====================================================
            // TIPO TAREFA
            // =====================================================

            // Entidade -> DTO de saída
            CreateMap<TipoTarefa, TipoTarefaDto>();

            // DTOs de entrada (se você for usar POST/PUT depois)
            CreateMap<TipoTarefaCreateDto, TipoTarefa>();
            CreateMap<TipoTarefaUpdateDto, TipoTarefa>();
        }
    }
}

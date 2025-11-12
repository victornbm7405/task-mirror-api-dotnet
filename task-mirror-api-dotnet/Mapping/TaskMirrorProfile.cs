using AutoMapper;

namespace TaskMirror.Mapping
{
    // Profile mínimo só para manter o AutoMapper registrado.
    // Não mapeamos Tarefa/Usuario/etc. aqui porque as respostas
    // são projetadas diretamente via LINQ no controller.
    public class TaskMirrorProfile : Profile
    {
        public TaskMirrorProfile()
        {
            // Adicione mappings aqui quando (e se) criar DTOs específicos.
        }
    }
}

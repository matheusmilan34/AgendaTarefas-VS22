using AgendaTarefas.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgendaTarefas.Repositorio
{
    public class TarefaRepositorio
    {
        private readonly Contexto _contexto;

        public TarefaRepositorio(Contexto contexto)
        {
            _contexto = contexto;
        }

        public async Task CriarTarefaAsync(Tarefa tarefa)
        {
            _contexto.Tarefas.Add(tarefa);
            await _contexto.SaveChangesAsync();
        }

        public async Task<Tarefa> AtualizarTarefa(int tarefaId)
        {
            return await _contexto.Tarefas.FindAsync(tarefaId);
        }

        public async Task AtualizarTarefa(Tarefa tarefa)
        {
            _contexto.Update(tarefa);
            await _contexto.SaveChangesAsync();
        }

        public async Task ExcluirTarefa(int tarefaId)
        {
            Tarefa tarefa = await _contexto.Tarefas.FindAsync(tarefaId);
            _contexto.Tarefas.Remove(tarefa);
            await _contexto.SaveChangesAsync();
        }

        public async Task<List<Tarefa>> BuscarTarefasPorNome(string nomeTarefa)
        {
            return _contexto.Tarefas.Where(t => t.Nome == nomeTarefa).OrderBy(x => x.Data).ToList();
        }
    }
}

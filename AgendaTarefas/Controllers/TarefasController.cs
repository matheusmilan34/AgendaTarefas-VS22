using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AgendaTarefas.Models;
using AgendaTarefas.Repositorio;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace AgendaTarefas.Controllers
{
    public class TarefasController : Controller
    {
        private TarefaRepositorio _tarefaRepositorio;

        public TarefasController(TarefaRepositorio tarefaRepositorio)
        {
            _tarefaRepositorio = tarefaRepositorio;
        }

        public IActionResult Index(DateTime? dataInicial, DateTime? dataFinal, string nomeTarefa)
        {
            if (string.IsNullOrEmpty(nomeTarefa))
            {
                if (!dataInicial.HasValue && !dataFinal.HasValue)
                {
                    if (HttpContext.Session.TryGetValue("DataInicial", out var dataInicialSession) &&
                        HttpContext.Session.TryGetValue("DataFinal", out var dataFinalSession))
                    {
                        dataInicial = DateTime.Parse(Encoding.UTF8.GetString(dataInicialSession));
                        dataFinal = DateTime.Parse(Encoding.UTF8.GetString(dataFinalSession));
                    }
                }
                else
                {
                    HttpContext.Session.SetString("DataInicial", dataInicial?.ToString());
                    HttpContext.Session.SetString("DataFinal", dataFinal?.ToString());
                }
                return View(PegarDatas(dataInicial, dataFinal));
            }
            else
                return View(PegarDatas(nomeTarefa));
        }

        private List<DatasViewModel> PegarDatas(string nomeTarefa)
        {
            DatasViewModel data;
            List<DatasViewModel> listaDatas = new List<DatasViewModel>();
            List<DateTime> listaFeriadosDataAtual = new List<DateTime>();
            string anoPesquisa = "";

            List<Tarefa> listaTarefas = _tarefaRepositorio.BuscarTarefasPorNome(nomeTarefa).Result;
            if (listaTarefas.Count > 0)
            {
                listaFeriadosDataAtual = ListarFeriadosAsync(Convert.ToInt32(listaTarefas[0].Data.Substring(6))).Result;
                anoPesquisa = listaTarefas[0].Data.Substring(6);
            }

            foreach (var item in listaTarefas)
            {
                data = new DatasViewModel();
                data.Datas = item.Data;
                data.Identificadores = "collapse" + item;
                if (item.Data.Substring(6) == anoPesquisa)
                    data.Feriado = listaFeriadosDataAtual.Contains(Convert.ToDateTime(item.Data));
                else
                {
                    listaFeriadosDataAtual = ListarFeriadosAsync(Convert.ToInt32(item.Data.Substring(6))).Result;
                    anoPesquisa = item.Data.Substring(6);
                    data.Feriado = listaFeriadosDataAtual.Contains(Convert.ToDateTime(item.Data));
                }
                listaDatas.Add(data);
            }
            return listaDatas;
        }

        private List<DatasViewModel> PegarDatas(DateTime? dataInicial, DateTime? dataFinal)
        {
            DateTime dataAtual;
            DateTime dataLimite;
            List<DateTime> listaFeriadosDataAtual;
            List<DateTime> listaFeriadosAnoDataFinal = null;

            if (dataInicial.HasValue)
                dataAtual = dataInicial.Value;
            else
                dataAtual = DateTime.Now;

            if (dataFinal.HasValue)
                dataLimite = dataFinal.Value;
            else
                dataLimite = DateTime.Now.AddDays(7);

            listaFeriadosDataAtual = ListarFeriadosAsync(dataAtual.Year).Result;
            if (dataAtual.Year != dataLimite.Year)
                listaFeriadosAnoDataFinal = ListarFeriadosAsync(dataAtual.Year).Result;

            DatasViewModel data;
            List<DatasViewModel> listaDatas = new List<DatasViewModel>();

            while (dataAtual <= dataLimite)
            {
                data = new DatasViewModel();
                data.Datas = dataAtual.ToShortDateString();
                data.Identificadores = "collapse" + dataAtual.ToShortDateString().Replace("/", "");
                data.Feriado = listaFeriadosDataAtual.Contains(dataAtual);
                if (data.Feriado == false && listaFeriadosAnoDataFinal != null)
                    data.Feriado = listaFeriadosAnoDataFinal.Contains(dataAtual);
                listaDatas.Add(data);
                dataAtual = dataAtual.AddDays(1);
            }

            return listaDatas;
        }

        public async Task<List<DateTime>> ListarFeriadosAsync(int ano)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync("https://date.nager.at/api/v3/publicholidays/" + ano + "/BR");
                    if (response.IsSuccessStatusCode)
                    {
                        string responseBody = await response.Content.ReadAsStringAsync();
                        var holidays = JsonConvert.DeserializeAnonymousType(responseBody, new[] { new { Date = "" } });
                        List<DateTime> holidayDates = holidays.Select(h => DateTime.Parse(h.Date)).ToList();
                        return holidayDates;
                    }
                    else
                    {
                        Console.WriteLine($"Erro na solicitação: {response.StatusCode} - {response.ReasonPhrase}");
                    }
                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine($"Erro na solicitação HTTP: {e.Message}");
                }
                return new List<DateTime>();
            }
        }

        [HttpGet]
        public IActionResult CriarTarefa(string dataTarefa)
        {
            Tarefa tarefa = new Tarefa
            {
                Data = dataTarefa
            };
            return PartialView(tarefa);
        }

        [HttpPost]
        public async Task<IActionResult> CriarTarefa(Tarefa tarefa)
        {
            if (ModelState.IsValid)
            {
                _tarefaRepositorio.CriarTarefaAsync(tarefa);
                return RedirectToAction(nameof(Index));
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> AtualizarTarefa(int tarefaId)
        {
            Task<Tarefa> tarefa = _tarefaRepositorio.AtualizarTarefa(tarefaId);

            if (tarefa == null)
                return NotFound();

            return PartialView(tarefa.Result);
        }

        [HttpPost]
        public async Task<IActionResult> AtualizarTarefa(Tarefa tarefa)
        {
            if (ModelState.IsValid)
            {
                _tarefaRepositorio.AtualizarTarefa(tarefa);
                return RedirectToAction(nameof(Index));
            }

            return View(tarefa);
        }

        [HttpPost]
        public async Task<JsonResult> ExcluirTarefa(int tarefaId)
        {
            _tarefaRepositorio.ExcluirTarefa(tarefaId);
            return Json(true);
        }
    }
}
